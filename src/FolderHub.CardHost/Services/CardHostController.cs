using System.Windows;
using FolderHub.CardHost.Views;
using FolderHub.Infrastructure.Ipc;
using FolderHub.UseCases.Ipc;
using FolderHub.UseCases.Persistence;

namespace FolderHub.CardHost.Services;

/// <summary>
/// 多卡片窗口与展示态/打开/取消固定编排。
/// </summary>
public sealed class CardHostController
{
    private readonly CardHostRuntime _runtime;
    private readonly Dictionary<Guid, CardWindow> _windows = new();
    private readonly object _gate = new();
    private IpcNamedPipeServer? _server;
    private CancellationTokenSource? _loopCts;
    private bool _mainConnected;

    public CardHostController(CardHostRuntime runtime)
    {
        _runtime = runtime;
    }

    public event Action? ShutdownRequested;

    public void LoadEnabledCardsFromStore()
    {
        var catalog = _runtime.LoadCatalog();
        var pendingCloses = CardHostPendingClosesFile.Peek();
        var definitions = _runtime.Definitions.LoadAll()
            .Where(d => d.Enabled && !pendingCloses.Contains(d.CardId))
            .ToList();
        var presentations = _runtime.Presentations.LoadAll().ToDictionary(p => p.CardId);

        var index = 0;
        foreach (var def in definitions)
        {
            var folder = catalog.Folders.FirstOrDefault(f => f.Id == def.FolderId);
            if (folder is null)
                continue;

            var payload = new CardDefinitionUpsertPayload(
                def.CardId,
                def.FolderId,
                def.Enabled,
                folder.DisplayName,
                folder.Alias,
                folder.Description,
                folder.Path,
                folder.IsPathUnavailable);

            presentations.TryGetValue(def.CardId, out var state);
            UpsertCard(payload, state, index++);
        }

        ApplyPendingClosesLocally();
    }

    public void UpsertCard(CardDefinitionUpsertPayload payload, CardPresentationStateSnapshot? state = null, int? indexHint = null)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            lock (_gate)
            {
                if (!payload.Enabled || CardHostPendingClosesFile.Peek().Contains(payload.CardId))
                {
                    CloseCardWindow(payload.CardId);
                    return;
                }

                if (_windows.TryGetValue(payload.CardId, out var existing))
                {
                    existing.Model.FolderId = payload.FolderId;
                    existing.Model.DisplayName = payload.DisplayName;
                    existing.Model.Alias = payload.Alias;
                    existing.Model.Description = payload.Description;
                    existing.Model.Path = payload.Path;
                    existing.Model.IsPathUnavailable = payload.IsPathUnavailable;
                    existing.ApplyModel();
                    return;
                }

                state ??= _runtime.Presentations.Load(payload.CardId);
                var index = indexHint ?? _windows.Count;
                var model = CardModel.FromUpsert(payload, state, index);
                var window = new CardWindow(this, model);
                _windows[payload.CardId] = window;
                window.Show();

                if (state is null)
                    PersistPresentation(window);
            }
        });
    }

    public void CloseCard(Guid cardId)
        => Application.Current.Dispatcher.Invoke(() => CloseCardWindow(cardId));

    public void PersistPresentation(CardWindow window)
    {
        var model = window.Model;
        model.Left = window.Left;
        model.Top = window.Top;
        model.StateVersion += 1;

        var snapshot = new CardPresentationStateSnapshot(
            model.CardId,
            model.Left,
            model.Top,
            model.Locked,
            MonitorReference: null,
            DpiReference: null,
            model.StateVersion,
            DateTimeOffset.UtcNow);

        _runtime.Presentations.Upsert(snapshot);
        model.StateVersion = snapshot.StateVersion;
        window.ApplyModel();

        _ = TryNotifyMainAsync(new CardStateChangedNoticePayload(
            model.CardId,
            Action: "presentationSaved",
            Left: model.Left,
            Top: model.Top,
            Locked: model.Locked,
            StateVersion: model.StateVersion));
    }

    public void OpenFolder(CardModel model)
    {
        if (!_runtime.Shell.IsDirectoryAccessible(model.Path))
        {
            model.IsPathUnavailable = true;
            if (_windows.TryGetValue(model.CardId, out var window))
                window.ApplyModel();
            MessageBox.Show(
                $"路径不可用：\n{model.Path}\n\n请在主程序中重新指定。",
                "FolderHub 卡片",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        model.IsPathUnavailable = false;
        if (_windows.TryGetValue(model.CardId, out var w))
            w.ApplyModel();

        if (!_runtime.Shell.TryOpenInExplorer(model.Path, out var error))
        {
            MessageBox.Show(error ?? "无法打开文件夹。", "打开失败", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public void RequestUnpin(Guid cardId)
    {
        // 先入队：Main 离线时也能在下次启动权威删除定义（ADR-035：CardHost 不写 CardDefinition）。
        CardHostPendingClosesFile.Enqueue(cardId);
        CloseCardWindow(cardId);
        _ = TryNotifyMainAsync(new CardStateChangedNoticePayload(cardId, Action: "closeRequested"));
    }

    /// <summary>按待处理取消固定队列关闭本地窗口（定义仍由 Main 删除）。</summary>
    public void ApplyPendingClosesLocally()
    {
        foreach (var cardId in CardHostPendingClosesFile.Peek())
            CloseCard(cardId);
    }

    public Task StartIpcAsync(CancellationToken cancellationToken)
    {
        _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = Task.Run(() => AcceptLoopAsync(_loopCts.Token), _loopCts.Token);
        return Task.CompletedTask;
    }

    public async Task StopIpcAsync()
    {
        _loopCts?.Cancel();
        if (_server is { IsConnected: true })
        {
            try
            {
                var bye = IpcEnvelope.Create(
                    IpcMessageType.HostGoodbye,
                    IpcSenderRole.CardHost,
                    _runtime.IpcOptions.SessionId,
                    _runtime.IpcOptions.HostInstanceId);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                await _server.SendAsync(bye, cts.Token).ConfigureAwait(false);
            }
            catch
            {
                // 忽略退出时发送失败。
            }
        }

        if (_server is not null)
            await _server.DisposeAsync().ConfigureAwait(false);
        _server = null;
        _mainConnected = false;
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            IpcNamedPipeServer? server = null;
            try
            {
                server = new IpcNamedPipeServer(_runtime.IpcOptions);
                _server = server;
                await server.WaitForConnectionAndHandshakeAsync(cancellationToken).ConfigureAwait(false);
                _mainConnected = true;

                var ready = IpcEnvelope.Create(
                    IpcMessageType.HostReady,
                    IpcSenderRole.CardHost,
                    _runtime.IpcOptions.SessionId,
                    _runtime.IpcOptions.HostInstanceId);
                await server.SendAsync(ready, cancellationToken).ConfigureAwait(false);
                ApplyPendingClosesLocally();

                while (!cancellationToken.IsCancellationRequested && server.IsConnected)
                {
                    var envelope = await server.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                    await HandleEnvelopeAsync(envelope, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Main 断开：Keep=ON 时卡片保留，重新监听等待下次连接。
            }
            finally
            {
                _mainConnected = false;
                if (server is not null)
                {
                    await server.DisposeAsync().ConfigureAwait(false);
                    if (ReferenceEquals(_server, server))
                        _server = null;
                }
            }

            if (!cancellationToken.IsCancellationRequested)
                await Task.Delay(300, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task HandleEnvelopeAsync(IpcEnvelope envelope, CancellationToken cancellationToken)
    {
        switch (envelope.MessageType)
        {
            case IpcMessageType.Ping:
                await _server!.SendAsync(
                    IpcEnvelope.Create(
                        IpcMessageType.Pong,
                        IpcSenderRole.CardHost,
                        _runtime.IpcOptions.SessionId,
                        _runtime.IpcOptions.HostInstanceId,
                        correlationId: envelope.MessageId),
                    cancellationToken).ConfigureAwait(false);
                break;

            case IpcMessageType.CardDefinitionUpsert:
            {
                var payload = CardIpcJson.Deserialize<CardDefinitionUpsertPayload>(envelope.PayloadJson);
                if (payload is not null)
                    UpsertCard(payload);
                await SendAckAsync(envelope, cancellationToken).ConfigureAwait(false);
                break;
            }

            case IpcMessageType.CardEnable:
            case IpcMessageType.CardDisable:
            case IpcMessageType.CardClose:
            {
                var idPayload = CardIpcJson.Deserialize<CardIdPayload>(envelope.PayloadJson);
                if (idPayload is not null)
                {
                    if (envelope.MessageType == IpcMessageType.CardEnable)
                    {
                        // 重新启用时由随后的 Upsert 带元数据；此处仅占位。
                    }
                    else
                    {
                        CloseCard(idPayload.CardId);
                        CardHostPendingClosesFile.Remove(idPayload.CardId);
                    }
                }

                await SendAckAsync(envelope, cancellationToken).ConfigureAwait(false);
                break;
            }

            case IpcMessageType.ShutdownRequest:
                await SendAckAsync(envelope, cancellationToken).ConfigureAwait(false);
                _ = Application.Current.Dispatcher.BeginInvoke(() => ShutdownRequested?.Invoke());
                break;

            case IpcMessageType.HostHello:
            {
                var hello = CardIpcJson.Deserialize<HostHelloPayload>(envelope.PayloadJson);
                await SendAckAsync(envelope, cancellationToken).ConfigureAwait(false);
                // detach：受控断开，保持卡片；下一轮 AcceptLoop 继续监听。
                if (hello is not null
                    && string.Equals(hello.Action, HostHelloPayload.Detach, StringComparison.OrdinalIgnoreCase))
                {
                    _mainConnected = false;
                }

                break;
            }

            case IpcMessageType.ReconcileRequest:
            case IpcMessageType.Ack:
            case IpcMessageType.Error:
            case IpcMessageType.CommandResult:
                break;
        }
    }

    private async Task SendAckAsync(IpcEnvelope request, CancellationToken cancellationToken)
    {
        if (_server is null || !_server.IsConnected)
            return;

        var ack = IpcEnvelope.Create(
            IpcMessageType.Ack,
            IpcSenderRole.CardHost,
            _runtime.IpcOptions.SessionId,
            _runtime.IpcOptions.HostInstanceId,
            correlationId: request.MessageId);
        await _server.SendAsync(ack, cancellationToken).ConfigureAwait(false);
    }

    private async Task TryNotifyMainAsync(CardStateChangedNoticePayload payload)
    {
        if (!_mainConnected || _server is null || !_server.IsConnected)
            return;

        try
        {
            var envelope = IpcEnvelope.Create(
                IpcMessageType.CardStateChangedNotice,
                IpcSenderRole.CardHost,
                _runtime.IpcOptions.SessionId,
                _runtime.IpcOptions.HostInstanceId,
                payloadJson: CardIpcJson.Serialize(payload),
                aggregateId: payload.CardId.ToString("D"));
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await _server.SendAsync(envelope, cts.Token).ConfigureAwait(false);
        }
        catch
        {
            _mainConnected = false;
        }
    }

    private void CloseCardWindow(Guid cardId)
    {
        lock (_gate)
        {
            if (!_windows.Remove(cardId, out var window))
                return;
            window.Close();
        }
    }

    public void CloseAllWindows()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            lock (_gate)
            {
                foreach (var window in _windows.Values.ToList())
                    window.Close();
                _windows.Clear();
            }
        });
    }
}

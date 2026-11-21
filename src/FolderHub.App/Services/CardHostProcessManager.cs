using System.Diagnostics;
using System.IO;
using System.Windows;
using FolderHub.Infrastructure.Ipc;
using FolderHub.UseCases.Ipc;
using FolderHub.UseCases.Persistence;

namespace FolderHub.App.Services;

/// <summary>
/// Main Process：按需启动 CardHost、握手、推送卡片定义（ADR-034/036）。
/// </summary>
public sealed class CardHostProcessManager : IAsyncDisposable
{
    private readonly AppSession _session;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private IpcConnectionOptions? _options;
    private IpcNamedPipeClient? _client;
    private CancellationTokenSource? _receiveCts;
    private Process? _process;
    private bool _ready;

    public CardHostProcessManager(AppSession session)
    {
        _session = session;
    }

    public event Action<CardStateChangedNoticePayload>? CardNoticeReceived;

    public bool IsReady => _ready && _client is { IsConnected: true };

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (IsReady)
            return;

        await _connectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsReady)
                return;

            await DisconnectClientAsync().ConfigureAwait(false);

            var identity = WindowsSessionIdentity.CaptureCurrent();
            if (TryReconnectExisting(identity, out var options))
            {
                _options = options;
            }
            else
            {
                await EnsureNoStaleCardHostAsync().ConfigureAwait(false);
                _options = IpcConnectionOptions.CreateNew(identity);
                StartCardHostProcess(_options);
            }

            var client = new IpcNamedPipeClient(_options, identity);
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linked.CancelAfter(TimeSpan.FromSeconds(15));

            var attempts = 0;
            Exception? last = null;
            while (attempts < 40)
            {
                attempts++;
                try
                {
                    await client.ConnectAndHandshakeAsync(linked.Token).ConfigureAwait(false);
                    last = null;
                    break;
                }
                catch (Exception ex)
                {
                    last = ex;
                    await Task.Delay(150, linked.Token).ConfigureAwait(false);
                }
            }

            if (last is not null || !client.IsConnected)
            {
                await client.DisposeAsync().ConfigureAwait(false);
                throw new InvalidOperationException(
                    "无法连接 CardHost IPC：" + (last?.Message ?? "超时"));
            }

            _client = client;
            _ready = true;
            _receiveCts = new CancellationTokenSource();
            _ = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token), _receiveCts.Token);

            await WaitForReadyHintAsync(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task PushDefinitionAsync(CardDefinitionUpsertPayload payload, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        var envelope = IpcEnvelope.Create(
            IpcMessageType.CardDefinitionUpsert,
            IpcSenderRole.MainProcess,
            _options!.SessionId,
            _options.HostInstanceId,
            payloadJson: CardIpcJson.Serialize(payload),
            aggregateId: payload.CardId.ToString("D"));
        await _client!.SendAsync(envelope, cancellationToken).ConfigureAwait(false);
        CardHostPendingClosesFile.Remove(payload.CardId);
    }

    public async Task CloseCardAsync(Guid cardId, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
            var envelope = IpcEnvelope.Create(
                IpcMessageType.CardClose,
                IpcSenderRole.MainProcess,
                _options!.SessionId,
                _options.HostInstanceId,
                payloadJson: CardIpcJson.Serialize(new CardIdPayload(cardId)),
                aggregateId: cardId.ToString("D"));
            await _client!.SendAsync(envelope, cancellationToken).ConfigureAwait(false);
            CardHostPendingClosesFile.Remove(cardId);
        }
        catch
        {
            // 管道不可用时入队，避免 CardHost 孤儿窗在重连/重载时残留。
            CardHostPendingClosesFile.Enqueue(cardId);
        }
    }

    /// <summary>
    /// 按待处理取消固定队列权威删除 CardDefinition（ADR-035）；队列条目在 CardClose 成功后再移除。
    /// </summary>
    public IReadOnlyList<Guid> ApplyPendingCloses()
    {
        var pending = CardHostPendingClosesFile.Peek().ToList();
        if (pending.Count == 0)
            return pending;

        var pendingSet = pending.ToHashSet();
        var cards = _session.CardDefinitions.LoadAll();
        var next = cards.Where(c => !pendingSet.Contains(c.CardId)).ToList();
        if (next.Count != cards.Count)
            _session.CardDefinitions.ReplaceAll(next);

        return pending;
    }

    public async Task SyncAllEnabledAsync(CancellationToken cancellationToken = default)
    {
        var pendingCloses = ApplyPendingCloses();
        var defs = _session.CardDefinitions.LoadAll().Where(d => d.Enabled).ToList();

        if (defs.Count == 0 && pendingCloses.Count == 0)
            return;

        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

        foreach (var cardId in pendingCloses)
            await CloseCardAsync(cardId, cancellationToken).ConfigureAwait(false);

        foreach (var def in defs)
        {
            var payload = BuildUpsertPayload(def);
            if (payload is null)
                continue;
            await PushDefinitionAsync(payload, cancellationToken).ConfigureAwait(false);
        }
    }

    public CardDefinitionUpsertPayload? BuildUpsertPayload(CardDefinitionSnapshot def)
    {
        var folder = _session.Catalog.Folders.FirstOrDefault(f => f.Id == def.FolderId);
        if (folder is null)
            return null;

        return new CardDefinitionUpsertPayload(
            def.CardId,
            def.FolderId,
            def.Enabled,
            folder.DisplayName,
            folder.Alias,
            folder.Description,
            folder.Path,
            folder.IsPathUnavailable);
    }

    /// <summary>
    /// 退出主程序：Keep=OFF 时请求 CardHost 关闭；Keep=ON 时先发受控 detach 再断开 IPC。
    /// </summary>
    public async Task HandleMainExitAsync(bool keepCardsAfterMainExit)
    {
        try
        {
            if (IsReady)
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
                if (keepCardsAfterMainExit)
                {
                    var detach = IpcEnvelope.Create(
                        IpcMessageType.HostHello,
                        IpcSenderRole.MainProcess,
                        _options!.SessionId,
                        _options.HostInstanceId,
                        payloadJson: CardIpcJson.Serialize(new HostHelloPayload(HostHelloPayload.Detach)));
                    await _client!.SendAsync(detach, cts.Token).ConfigureAwait(false);
                    await Task.Delay(150, CancellationToken.None).ConfigureAwait(false);
                }
                else
                {
                    var envelope = IpcEnvelope.Create(
                        IpcMessageType.ShutdownRequest,
                        IpcSenderRole.MainProcess,
                        _options!.SessionId,
                        _options.HostInstanceId);
                    await _client!.SendAsync(envelope, cts.Token).ConfigureAwait(false);
                    await Task.Delay(400, CancellationToken.None).ConfigureAwait(false);
                }
            }
        }
        catch
        {
            // 退出路径尽量不抛。
        }
        finally
        {
            await DisconnectClientAsync().ConfigureAwait(false);
            if (!keepCardsAfterMainExit)
            {
                TryKillTrackedProcess();
                CardHostSessionFile.TryDelete();
            }
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _client is { IsConnected: true })
            {
                var envelope = await _client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                if (envelope.MessageType == IpcMessageType.HostReady)
                    _ready = true;

                if (envelope.MessageType == IpcMessageType.CardStateChangedNotice)
                {
                    var payload = CardIpcJson.Deserialize<CardStateChangedNoticePayload>(envelope.PayloadJson);
                    if (payload is not null)
                    {
                        var handler = CardNoticeReceived;
                        if (handler is not null)
                            _ = Application.Current?.Dispatcher.BeginInvoke(() => handler(payload));
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch
        {
            _ready = false;
        }
    }

    private async Task WaitForReadyHintAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var until = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < until && !cancellationToken.IsCancellationRequested)
        {
            if (_ready)
                return;
            await Task.Delay(50, cancellationToken).ConfigureAwait(false);
        }
    }

    private bool TryReconnectExisting(WindowsSessionIdentity identity, out IpcConnectionOptions options)
    {
        options = null!;
        var session = CardHostSessionFile.TryRead();
        if (session is null)
            return false;
        if (session.SessionId != identity.SessionId)
            return false;

        try
        {
            var existing = Process.GetProcessById(session.ProcessId);
            if (existing.HasExited)
            {
                existing.Dispose();
                return false;
            }

            if (!string.Equals(existing.ProcessName, "FolderHub.CardHost", StringComparison.OrdinalIgnoreCase))
            {
                existing.Dispose();
                return false;
            }

            options = CardHostSessionFile.ToOptions(session);
            _process?.Dispose();
            _process = existing;
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsCardHostProcessAlive()
    {
        try
        {
            if (_process is { HasExited: false })
                return true;

            var session = CardHostSessionFile.TryRead();
            if (session is null)
                return false;

            using var existing = Process.GetProcessById(session.ProcessId);
            return !existing.HasExited
                && string.Equals(existing.ProcessName, "FolderHub.CardHost", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private void StartCardHostProcess(IpcConnectionOptions options)
    {
        CardHostSessionFile.Write(new CardHostSessionRecord(
            options.HostInstanceId,
            options.InstanceToken,
            options.SessionId,
            ProcessId: 0));

        var exe = ResolveCardHostExePath();
        var start = new ProcessStartInfo
        {
            FileName = exe,
            UseShellExecute = false,
            WorkingDirectory = Path.GetDirectoryName(exe) ?? Environment.CurrentDirectory
        };
        foreach (var arg in CardHostLaunchArgs.Build(options))
            start.ArgumentList.Add(arg);

        _process?.Dispose();
        _process = Process.Start(start)
            ?? throw new InvalidOperationException("无法启动 FolderHub.CardHost。");

        CardHostSessionFile.Write(new CardHostSessionRecord(
            options.HostInstanceId,
            options.InstanceToken,
            options.SessionId,
            _process.Id));
    }

    private static string ResolveCardHostExePath()
    {
        var baseDir = AppContext.BaseDirectory;
        var sibling = Path.Combine(baseDir, "FolderHub.CardHost.exe");
        if (File.Exists(sibling))
            return sibling;

        var candidate = Path.GetFullPath(Path.Combine(
            baseDir,
            "..", "..", "..", "..",
            "FolderHub.CardHost", "bin", "Debug", "net8.0-windows", "FolderHub.CardHost.exe"));
        if (File.Exists(candidate))
            return candidate;

        candidate = Path.GetFullPath(Path.Combine(
            baseDir,
            "..", "..", "..", "..",
            "FolderHub.CardHost", "bin", "Release", "net8.0-windows", "FolderHub.CardHost.exe"));
        if (File.Exists(candidate))
            return candidate;

        throw new FileNotFoundException(
            "找不到 FolderHub.CardHost.exe。请先编译 CardHost 项目，或与主程序置于同一目录。",
            sibling);
    }

    private async Task EnsureNoStaleCardHostAsync()
    {
        var session = CardHostSessionFile.TryRead();
        if (session is null)
            return;

        try
        {
            using var existing = Process.GetProcessById(session.ProcessId);
            if (!existing.HasExited
                && string.Equals(existing.ProcessName, "FolderHub.CardHost", StringComparison.OrdinalIgnoreCase))
            {
                existing.Kill(entireProcessTree: true);
                await Task.Delay(200).ConfigureAwait(false);
            }
        }
        catch
        {
            // ignore
        }

        CardHostSessionFile.TryDelete();
    }

    private void TryKillTrackedProcess()
    {
        try
        {
            if (_process is { HasExited: false })
            {
                _process.Kill(entireProcessTree: true);
                return;
            }
        }
        catch
        {
            // fall through to session PID
        }

        try
        {
            var session = CardHostSessionFile.TryRead();
            if (session is null)
                return;

            using var existing = Process.GetProcessById(session.ProcessId);
            if (!existing.HasExited
                && string.Equals(existing.ProcessName, "FolderHub.CardHost", StringComparison.OrdinalIgnoreCase))
            {
                existing.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignore
        }
    }

    private async Task DisconnectClientAsync()
    {
        _ready = false;
        _receiveCts?.Cancel();
        _receiveCts?.Dispose();
        _receiveCts = null;
        if (_client is not null)
        {
            await _client.DisposeAsync().ConfigureAwait(false);
            _client = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectClientAsync().ConfigureAwait(false);
        _process?.Dispose();
        _process = null;
        _connectLock.Dispose();
    }
}

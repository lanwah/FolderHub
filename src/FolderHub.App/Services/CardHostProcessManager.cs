using System.Diagnostics;
using System.IO;
using System.Windows;
using FolderHub.Infrastructure.Ipc;
using FolderHub.UseCases.Ipc;
using FolderHub.UseCases.Persistence;
using FolderHub.UseCases.Recovery;

namespace FolderHub.App.Services;

/// <summary>
/// Main Process：按需启动 CardHost、握手、推送卡片定义，并协调运行期有限重启（ADR-034/036/037/038）。
/// </summary>
public sealed class CardHostProcessManager : IAsyncDisposable
{
    private readonly AppSession _session;
    private readonly CardHostRecoveryPolicy _recovery;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly object _watchGate = new();
    private IpcConnectionOptions? _options;
    private IpcNamedPipeClient? _client;
    private CancellationTokenSource? _receiveCts;
    private CancellationTokenSource? _recoveryCts;
    private Process? _process;
    private EventHandler? _processExitedHandler;
    private bool _ready;
    private bool _intentionalDetachOrShutdown;
    private bool _recoveryLoopRunning;
    private bool _degradedNotified;

    public CardHostProcessManager(AppSession session, CardHostRecoveryPolicy? recovery = null)
    {
        _session = session;
        _recovery = recovery ?? new CardHostRecoveryPolicy();
    }

    public event Action<CardStateChangedNoticePayload>? CardNoticeReceived;

    /// <summary>自动重启预算耗尽（ADR-038）；UI 应提示并可触发 <see cref="ManualRecoverAsync"/>。</summary>
    public event Action<string>? RecoveryDegraded;

    /// <summary>恢复流程状态变化（调试/状态栏可选）。</summary>
    public event Action<string>? RecoveryStatusChanged;

    public CardHostRecoveryPolicy Recovery => _recovery;

    public bool IsReady => _ready && _client is { IsConnected: true };

    public bool IsDegraded => _recovery.IsDegraded;

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
    {
        if (IsReady)
            return;

        // 用户再次固定/同步时，视为手动恢复入口（ADR-038 降级后）。
        if (_recovery.IsDegraded)
        {
            CancelRecoveryLoop();
            _degradedNotified = false;
            _recovery.OnManualRecoverRequested();
        }

        await _connectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsReady)
                return;

            await ConnectCoreAsync(cancellationToken).ConfigureAwait(false);
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
        MarkHealthy();
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

    /// <summary>向已连接 CardHost 同步浅/深色主题；未连接时忽略。</summary>
    public async Task NotifyThemeAsync(string uiTheme, CancellationToken cancellationToken = default)
    {
        if (!IsReady || _client is null || _options is null)
            return;

        try
        {
            var envelope = IpcEnvelope.Create(
                IpcMessageType.HostHello,
                IpcSenderRole.MainProcess,
                _options.SessionId,
                _options.HostInstanceId,
                payloadJson: CardIpcJson.Serialize(
                    new HostHelloPayload(HostHelloPayload.SetTheme, UiTheme.Normalize(uiTheme))));
            await _client.SendAsync(envelope, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            // 主题同步失败不阻断主流程；下次 CardHost 启动会从设置读取。
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
        _recovery.BeginRestoring();

        foreach (var cardId in pendingCloses)
            await CloseCardAsync(cardId, cancellationToken).ConfigureAwait(false);

        foreach (var def in defs)
        {
            var payload = BuildUpsertPayload(def);
            if (payload is null)
                continue;
            await PushDefinitionAsync(payload, cancellationToken).ConfigureAwait(false);
        }

        MarkHealthy();
        RaiseRecoveryStatus("CardHost 已同步");
    }

    /// <summary>
    /// 启动恢复（ADR-037）：若有启用卡片则连接既有 CardHost 或新建并同步最后持久化状态。
    /// </summary>
    public async Task RestoreOnMainStartupAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SyncAllEnabledAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            RaiseRecoveryStatus("启动恢复失败：" + ex.Message);
            // 启动期失败不阻断主程序；有启用卡片时纳入运行期预算，便于后续自动/手动恢复。
            if (HasEnabledCards())
                BeginRecoveryFromDecision(_recovery.OnRestartAttemptFailed());
        }
    }

    /// <summary>用户手动恢复：重置降级预算并重新同步启用卡片。</summary>
    public async Task ManualRecoverAsync(CancellationToken cancellationToken = default)
    {
        if (_recovery.IsStopped)
            return;

        // 打断进行中的自动恢复，避免与手动路径并发启停进程。
        CancelRecoveryLoop();
        _degradedNotified = false;
        _recovery.OnManualRecoverRequested();
        RaiseRecoveryStatus("正在手动恢复 CardHost…");

        try
        {
            await _connectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_intentionalDetachOrShutdown || _recovery.IsStopped)
                    return;

                await DisconnectClientAsync().ConfigureAwait(false);
                _recovery.BeginStarting();
                await ConnectCoreAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _connectLock.Release();
            }

            await SyncAllEnabledAsync(cancellationToken).ConfigureAwait(false);
            RaiseRecoveryStatus("手动恢复成功");
        }
        catch (Exception ex)
        {
            BeginRecoveryFromDecision(_recovery.OnRestartAttemptFailed());
            throw new InvalidOperationException("手动恢复 CardHost 失败：" + ex.Message, ex);
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
            folder.IsPathUnavailable,
            folder.Icon);
    }

    /// <summary>
    /// 退出主程序：Keep=OFF 时请求 CardHost 关闭；Keep=ON 时先发受控 detach 再断开 IPC。
    /// </summary>
    public async Task HandleMainExitAsync(bool keepCardsAfterMainExit)
    {
        _intentionalDetachOrShutdown = true;
        _recovery.OnMainExiting();
        CancelRecoveryLoop();

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
            else
            {
                // Keep=ON：不再监视进程退出（Main 即将退出，ADR-037）。
                DetachProcessWatch();
            }
        }
    }

    private async Task ConnectCoreAsync(CancellationToken cancellationToken)
    {
        if (_intentionalDetachOrShutdown || _recovery.IsStopped)
            throw new OperationCanceledException("Main 正在退出，取消连接 CardHost。");

        await DisconnectClientAsync().ConfigureAwait(false);

        var identity = WindowsSessionIdentity.CaptureCurrent();
        if (TryReconnectExisting(identity, out var options))
        {
            _options = options;
        }
        else
        {
            await EnsureNoStaleCardHostAsync().ConfigureAwait(false);
            if (_intentionalDetachOrShutdown || _recovery.IsStopped)
                throw new OperationCanceledException("Main 正在退出，取消启动 CardHost。");

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
            if (_intentionalDetachOrShutdown || _recovery.IsStopped)
            {
                await client.DisposeAsync().ConfigureAwait(false);
                throw new OperationCanceledException("Main 正在退出，取消连接 CardHost。");
            }

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

        if (_intentionalDetachOrShutdown || _recovery.IsStopped)
        {
            await client.DisposeAsync().ConfigureAwait(false);
            throw new OperationCanceledException("Main 正在退出，取消连接 CardHost。");
        }

        _client = client;
        _ready = true;
        // 不在此处 MarkRunning / 清除 _intentionalDetachOrShutdown：
        // 健康态仅在定义对账或业务推送成功后确认；受控退出标志只能由退出路径置位。
        _receiveCts = new CancellationTokenSource();
        _ = Task.Run(() => ReceiveLoopAsync(_receiveCts.Token), _receiveCts.Token);

        await WaitForReadyHintAsync(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);
    }

    private void BeginRecoveryFromDecision(CardHostRecoveryDecision decision)
    {
        switch (decision)
        {
            case CardHostRecoveryDecision.Ignore:
                RaiseRecoveryStatus("忽略恢复：" + ((CardHostRecoveryDecision.Ignore)decision).Reason);
                break;

            case CardHostRecoveryDecision.EnterDegraded degraded:
                RaiseRecoveryStatus(degraded.Reason);
                RaiseDegraded(degraded.Reason);
                break;

            case CardHostRecoveryDecision.ScheduleRestart schedule:
                RaiseRecoveryStatus(
                    $"计划第 {schedule.AttemptNumber} 次自动重启，退避 {schedule.Backoff.TotalSeconds:0.###}s");
                StartRecoveryLoop(schedule.Backoff);
                break;
        }
    }

    private void StartRecoveryLoop(TimeSpan initialBackoff)
    {
        if (_recovery.IsStopped || !HasEnabledCards())
            return;

        lock (_watchGate)
        {
            if (_recoveryLoopRunning)
                return;
            _recoveryLoopRunning = true;
        }

        CancelRecoveryLoop(disposeOnly: true);
        _recoveryCts = new CancellationTokenSource();
        var token = _recoveryCts.Token;
        _ = Task.Run(() => RunRecoveryLoopAsync(initialBackoff, token), token);
    }

    private async Task RunRecoveryLoopAsync(TimeSpan firstBackoff, CancellationToken cancellationToken)
    {
        var backoff = firstBackoff;
        try
        {
            while (!cancellationToken.IsCancellationRequested && !_recovery.IsStopped)
            {
                if (!HasEnabledCards())
                {
                    RaiseRecoveryStatus("无启用卡片，停止自动恢复");
                    break;
                }

                try
                {
                    if (backoff > TimeSpan.Zero)
                        await Task.Delay(backoff, cancellationToken).ConfigureAwait(false);

                    if (_recovery.IsStopped || _intentionalDetachOrShutdown)
                        break;

                    RaiseRecoveryStatus("正在自动重启 CardHost…");
                    _recovery.BeginStarting();

                    await _connectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                    try
                    {
                        if (_intentionalDetachOrShutdown || _recovery.IsStopped)
                            break;

                        await DisconnectClientAsync().ConfigureAwait(false);
                        await EnsureNoStaleCardHostAsync().ConfigureAwait(false);
                        await ConnectCoreAsync(cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        _connectLock.Release();
                    }

                    if (_intentionalDetachOrShutdown || _recovery.IsStopped)
                        break;

                    // 对账成功后 SyncAllEnabled→MarkHealthy；失败只计一次 OnRestartAttemptFailed，
                    // 不在 Connect 阶段 MarkRunning，避免与 Exited 双计。
                    await SyncAllEnabledAsync(cancellationToken).ConfigureAwait(false);
                    RaiseRecoveryStatus("自动重启成功");
                    break;
                }
                catch (OperationCanceledException) when (
                    cancellationToken.IsCancellationRequested
                    || _intentionalDetachOrShutdown
                    || _recovery.IsStopped)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_intentionalDetachOrShutdown || _recovery.IsStopped)
                        break;

                    RaiseRecoveryStatus("自动重启失败：" + ex.Message);
                    var next = _recovery.OnRestartAttemptFailed();
                    if (next is CardHostRecoveryDecision.EnterDegraded degraded)
                    {
                        RaiseDegraded(degraded.Reason);
                        break;
                    }

                    if (next is CardHostRecoveryDecision.ScheduleRestart schedule)
                    {
                        backoff = schedule.Backoff;
                        RaiseRecoveryStatus(
                            $"将进行第 {schedule.AttemptNumber} 次自动重启，退避 {schedule.Backoff.TotalSeconds:0.###}s");
                        continue;
                    }

                    break;
                }
            }
        }
        finally
        {
            lock (_watchGate)
                _recoveryLoopRunning = false;
        }
    }

    private void OnWatchedProcessExited(object? sender, EventArgs e)
    {
        if (_intentionalDetachOrShutdown || _recovery.IsStopped)
            return;

        _ready = false;
        RaiseRecoveryStatus("检测到 CardHost 进程退出");

        if (!HasEnabledCards())
        {
            _ = DisconnectClientAsync();
            return;
        }

        var decision = _recovery.OnUnexpectedHostExit();
        BeginRecoveryFromDecision(decision);
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
            if (!_intentionalDetachOrShutdown
                && !_recovery.IsStopped
                && HasEnabledCards()
                && !IsCardHostProcessAlive())
            {
                // 管道异常且进程已不在：补一条 HostLost（与 Exited 事件合并计数）。
                var decision = _recovery.OnUnexpectedHostExit();
                BeginRecoveryFromDecision(decision);
            }
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
            AttachProcessWatch(existing);
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

        var process = Process.Start(start)
            ?? throw new InvalidOperationException("无法启动 FolderHub.CardHost。");

        AttachProcessWatch(process);

        CardHostSessionFile.Write(new CardHostSessionRecord(
            options.HostInstanceId,
            options.InstanceToken,
            options.SessionId,
            process.Id));
    }

    private void AttachProcessWatch(Process process)
    {
        DetachProcessWatch(disposeProcess: true);
        _process = process;
        _processExitedHandler = OnWatchedProcessExited;
        process.EnableRaisingEvents = true;
        process.Exited += _processExitedHandler;

        // 订阅前若已退出，Exited 不会再触发，需同步补一次。
        if (process.HasExited)
            OnWatchedProcessExited(process, EventArgs.Empty);
    }

    private void DetachProcessWatch(bool disposeProcess = false)
    {
        if (_process is null)
            return;

        if (_processExitedHandler is not null)
        {
            try
            {
                _process.Exited -= _processExitedHandler;
            }
            catch
            {
                // ignore
            }
        }

        _processExitedHandler = null;

        if (disposeProcess)
        {
            try
            {
                _process.Dispose();
            }
            catch
            {
                // ignore
            }
        }

        _process = null;
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

    private void CancelRecoveryLoop(bool disposeOnly = false)
    {
        try
        {
            _recoveryCts?.Cancel();
        }
        catch
        {
            // ignore
        }

        _recoveryCts?.Dispose();
        _recoveryCts = null;
        if (!disposeOnly)
        {
            lock (_watchGate)
                _recoveryLoopRunning = false;
        }
    }

    private bool HasEnabledCards()
        => _session.CardDefinitions.LoadAll().Any(c => c.Enabled);

    private void MarkHealthy()
    {
        _degradedNotified = false;
        _recovery.MarkRunning(DateTimeOffset.UtcNow);
    }

    private void RaiseDegraded(string reason)
    {
        // 同一降级周期只通知一次，避免 Exited + 循环 catch 叠多个对话框。
        if (_degradedNotified)
            return;
        _degradedNotified = true;

        var handler = RecoveryDegraded;
        if (handler is null)
            return;

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            handler(reason);
        else
            dispatcher.BeginInvoke(() => handler(reason));
    }

    private void RaiseRecoveryStatus(string message)
    {
        var handler = RecoveryStatusChanged;
        if (handler is null)
            return;

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            handler(message);
        else
            dispatcher.BeginInvoke(() => handler(message));
    }

    public async ValueTask DisposeAsync()
    {
        _intentionalDetachOrShutdown = true;
        _recovery.OnMainExiting();
        CancelRecoveryLoop();
        await DisconnectClientAsync().ConfigureAwait(false);
        DetachProcessWatch();
        _process?.Dispose();
        _process = null;
        _connectLock.Dispose();
    }
}

namespace FolderHub.UseCases.Recovery;

/// <summary>
/// CardHost 运行期有限自动恢复策略（纯逻辑，无 IO；ADR-038）。
/// Main 已退出场景由 ADR-037 覆盖，不在本类职责内。
/// </summary>
public sealed class CardHostRecoveryPolicy
{
    private readonly CardHostRecoveryOptions _options;
    private DateTimeOffset? _runningSinceUtc;

    public CardHostRecoveryPolicy(CardHostRecoveryOptions? options = null)
    {
        _options = options ?? CardHostRecoveryOptions.EngineeringDefaults;
        State = CardHostRecoveryState.Running;
    }

    public CardHostRecoveryOptions Options => _options;

    public CardHostRecoveryState State { get; private set; }

    /// <summary>
    /// 当前窗口内已计入的失败次数（异常退出或重启失败）。
    /// 连续健康满 <see cref="CardHostRecoveryOptions.StableWindow"/> 后清零。
    /// </summary>
    public int FailedAttempts { get; private set; }

    public bool IsDegraded => State == CardHostRecoveryState.Degraded;

    public bool IsStopped => State == CardHostRecoveryState.Stopped;

    public bool AllowsAutomaticRestart
        => State is not (CardHostRecoveryState.Degraded or CardHostRecoveryState.Stopped);

    /// <summary>
    /// 标记宿主已健康运行。若已连续健康超过 StableWindow，清零失败计数。
    /// </summary>
    public void MarkRunning(DateTimeOffset utcNow)
    {
        if (State == CardHostRecoveryState.Stopped)
            return;

        _runningSinceUtc ??= utcNow;

        if (FailedAttempts > 0
            && utcNow - _runningSinceUtc.Value >= _options.StableWindow)
        {
            FailedAttempts = 0;
        }

        State = CardHostRecoveryState.Running;
    }

    /// <summary>检测到 CardHost 异常退出（非受控 detach/shutdown）。</summary>
    public CardHostRecoveryDecision OnUnexpectedHostExit()
    {
        if (State == CardHostRecoveryState.Stopped)
            return new CardHostRecoveryDecision.Ignore("Main Process 已退出，停止自动重启（ADR-037/038）。");

        if (State == CardHostRecoveryState.Degraded)
        {
            return new CardHostRecoveryDecision.EnterDegraded(
                FailedAttempts,
                "已处于降级，等待手动恢复。");
        }

        // Exited 与管道断开可能同时到达：恢复流程中不重复计数。
        if (State is CardHostRecoveryState.RestartWait
            or CardHostRecoveryState.Starting
            or CardHostRecoveryState.Restoring
            or CardHostRecoveryState.HostLost)
        {
            return DecideAfterCountedFailure("CardHost 异常退出（已在恢复流程中，不重复计数）");
        }

        return RegisterNewFailureAndDecide("CardHost 异常退出");
    }

    /// <summary>本次自动重启失败（启动或对账失败）；始终计入预算。</summary>
    public CardHostRecoveryDecision OnRestartAttemptFailed()
    {
        if (State == CardHostRecoveryState.Stopped)
            return new CardHostRecoveryDecision.Ignore("Main Process 已退出。");

        if (State == CardHostRecoveryState.Degraded)
        {
            return new CardHostRecoveryDecision.EnterDegraded(
                FailedAttempts,
                "已处于降级，等待手动恢复。");
        }

        return RegisterNewFailureAndDecide("重启/对账失败");
    }

    /// <summary>即将发起进程启动/IPC 重连。</summary>
    public void BeginStarting()
    {
        if (State == CardHostRecoveryState.Stopped)
            return;
        State = CardHostRecoveryState.Starting;
    }

    /// <summary>进程已起、开始推送定义对账。</summary>
    public void BeginRestoring()
    {
        if (State == CardHostRecoveryState.Stopped)
            return;
        State = CardHostRecoveryState.Restoring;
    }

    /// <summary>用户从降级态请求手动恢复；重置预算。</summary>
    public void OnManualRecoverRequested()
    {
        if (State == CardHostRecoveryState.Stopped)
            return;

        FailedAttempts = 0;
        _runningSinceUtc = null;
        State = CardHostRecoveryState.RestartWait;
    }

    /// <summary>Main Process 即将退出：取消后续自动重启。</summary>
    public void OnMainExiting()
    {
        State = CardHostRecoveryState.Stopped;
        _runningSinceUtc = null;
    }

    public TimeSpan ComputeBackoff(int zeroBasedFailureIndex)
    {
        var factor = Math.Pow(_options.BackoffMultiplier, Math.Max(0, zeroBasedFailureIndex));
        var ms = _options.InitialBackoff.TotalMilliseconds * factor;
        var capped = Math.Min(ms, _options.MaxBackoff.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(capped);
    }

    private CardHostRecoveryDecision RegisterNewFailureAndDecide(string reason)
    {
        FailedAttempts++;
        _runningSinceUtc = null;
        State = CardHostRecoveryState.HostLost;
        return DecideAfterCountedFailure(reason);
    }

    private CardHostRecoveryDecision DecideAfterCountedFailure(string reason)
    {
        if (FailedAttempts > _options.MaxAutomaticAttempts)
        {
            State = CardHostRecoveryState.Degraded;
            return new CardHostRecoveryDecision.EnterDegraded(
                FailedAttempts,
                $"{reason}；连续失败 {FailedAttempts} 次，超过上限 {_options.MaxAutomaticAttempts}。");
        }

        var attemptNumber = FailedAttempts;
        var backoff = ComputeBackoff(FailedAttempts - 1);
        State = CardHostRecoveryState.RestartWait;
        return new CardHostRecoveryDecision.ScheduleRestart(backoff, attemptNumber);
    }
}

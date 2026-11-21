namespace FolderHub.UseCases.Recovery;

/// <summary>
/// <see cref="CardHostRecoveryPolicy"/> 在异常退出后的下一步动作。
/// </summary>
public abstract record CardHostRecoveryDecision
{
    private CardHostRecoveryDecision()
    {
    }

    /// <summary>在退避后执行一次自动重启尝试。</summary>
    public sealed record ScheduleRestart(TimeSpan Backoff, int AttemptNumber) : CardHostRecoveryDecision;

    /// <summary>停止自动重启并进入降级；应提示用户。</summary>
    public sealed record EnterDegraded(int FailedAttempts, string Reason) : CardHostRecoveryDecision;

    /// <summary>忽略（例如 Main 已退出，或本来就不在监控）。</summary>
    public sealed record Ignore(string Reason) : CardHostRecoveryDecision;
}

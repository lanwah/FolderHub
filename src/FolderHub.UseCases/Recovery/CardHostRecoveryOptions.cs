namespace FolderHub.UseCases.Recovery;

/// <summary>
/// Main Process 存活期间 CardHost 有限自动重启参数（ADR-038）。
/// 数值为工程实现假定（DESIGN_ASSUMPTION），非用户已确认 SLA。
/// </summary>
public sealed class CardHostRecoveryOptions
{
    /// <summary>
    /// 工程默认：最多 3 次自动重启；退避 1s→2s→4s（封顶 30s）；
    /// 连续健康满 StableWindow 后清零失败计数。
    /// </summary>
    public static CardHostRecoveryOptions EngineeringDefaults { get; } = new(
        maxAutomaticAttempts: 3,
        initialBackoff: TimeSpan.FromSeconds(1),
        maxBackoff: TimeSpan.FromSeconds(30),
        backoffMultiplier: 2.0,
        stableWindow: TimeSpan.FromSeconds(30));

    public CardHostRecoveryOptions(
        int maxAutomaticAttempts,
        TimeSpan initialBackoff,
        TimeSpan maxBackoff,
        double backoffMultiplier,
        TimeSpan stableWindow)
    {
        if (maxAutomaticAttempts < 1)
            throw new ArgumentOutOfRangeException(nameof(maxAutomaticAttempts));
        if (initialBackoff < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(initialBackoff));
        if (maxBackoff < initialBackoff)
            throw new ArgumentOutOfRangeException(nameof(maxBackoff));
        if (backoffMultiplier < 1.0)
            throw new ArgumentOutOfRangeException(nameof(backoffMultiplier));
        if (stableWindow < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(stableWindow));

        MaxAutomaticAttempts = maxAutomaticAttempts;
        InitialBackoff = initialBackoff;
        MaxBackoff = maxBackoff;
        BackoffMultiplier = backoffMultiplier;
        StableWindow = stableWindow;
    }

    public int MaxAutomaticAttempts { get; }
    public TimeSpan InitialBackoff { get; }
    public TimeSpan MaxBackoff { get; }
    public double BackoffMultiplier { get; }
    public TimeSpan StableWindow { get; }
}

using FolderHub.UseCases.Recovery;

namespace FolderHub.Infrastructure.Tests;

public sealed class CardHostRecoveryPolicyTests
{
    private static CardHostRecoveryOptions TightOptions { get; } = new(
        maxAutomaticAttempts: 3,
        initialBackoff: TimeSpan.FromMilliseconds(10),
        maxBackoff: TimeSpan.FromMilliseconds(80),
        backoffMultiplier: 2.0,
        stableWindow: TimeSpan.FromMilliseconds(50));

    [Fact]
    public void Unexpected_exit_schedules_restart_within_budget()
    {
        var policy = new CardHostRecoveryPolicy(TightOptions);
        var decision = policy.OnUnexpectedHostExit();

        var schedule = Assert.IsType<CardHostRecoveryDecision.ScheduleRestart>(decision);
        Assert.Equal(1, schedule.AttemptNumber);
        Assert.Equal(TimeSpan.FromMilliseconds(10), schedule.Backoff);
        Assert.Equal(CardHostRecoveryState.RestartWait, policy.State);
        Assert.Equal(1, policy.FailedAttempts);
    }

    [Fact]
    public void Exhausts_budget_then_enters_degraded_without_infinite_restart()
    {
        var policy = new CardHostRecoveryPolicy(TightOptions);

        for (var i = 0; i < 3; i++)
        {
            policy.MarkRunning(DateTimeOffset.UtcNow);
            var d = policy.OnUnexpectedHostExit();
            Assert.IsType<CardHostRecoveryDecision.ScheduleRestart>(d);
            // Exited 与管道断开合并：恢复中再次 OnUnexpectedHostExit 不重复计数。
            var coalesced = policy.OnUnexpectedHostExit();
            Assert.IsType<CardHostRecoveryDecision.ScheduleRestart>(coalesced);
            Assert.Equal(i + 1, policy.FailedAttempts);
        }

        policy.MarkRunning(DateTimeOffset.UtcNow);
        var degraded = policy.OnUnexpectedHostExit();
        var enter = Assert.IsType<CardHostRecoveryDecision.EnterDegraded>(degraded);
        Assert.Equal(4, enter.FailedAttempts);
        Assert.True(policy.IsDegraded);
        Assert.False(policy.AllowsAutomaticRestart);
    }

    [Fact]
    public void Restart_attempt_failed_always_counts()
    {
        var policy = new CardHostRecoveryPolicy(TightOptions);
        policy.OnUnexpectedHostExit();
        Assert.Equal(1, policy.FailedAttempts);

        var second = policy.OnRestartAttemptFailed();
        Assert.IsType<CardHostRecoveryDecision.ScheduleRestart>(second);
        Assert.Equal(2, policy.FailedAttempts);
    }

    [Fact]
    public void Stable_window_clears_failure_budget()
    {
        var policy = new CardHostRecoveryPolicy(TightOptions);
        policy.OnUnexpectedHostExit();
        Assert.Equal(1, policy.FailedAttempts);

        var t0 = DateTimeOffset.Parse("2026-11-21T12:00:00Z");
        policy.MarkRunning(t0);
        Assert.Equal(1, policy.FailedAttempts);

        policy.MarkRunning(t0 + TimeSpan.FromMilliseconds(60));
        Assert.Equal(0, policy.FailedAttempts);
        Assert.Equal(CardHostRecoveryState.Running, policy.State);
    }

    [Fact]
    public void Manual_recover_resets_degraded_budget()
    {
        var policy = new CardHostRecoveryPolicy(TightOptions);
        for (var i = 0; i < 4; i++)
        {
            if (policy.State != CardHostRecoveryState.Degraded)
                policy.MarkRunning(DateTimeOffset.UtcNow);
            policy.OnUnexpectedHostExit();
        }

        Assert.True(policy.IsDegraded);
        policy.OnManualRecoverRequested();
        Assert.Equal(0, policy.FailedAttempts);
        Assert.Equal(CardHostRecoveryState.RestartWait, policy.State);
        Assert.True(policy.AllowsAutomaticRestart);
    }

    [Fact]
    public void Main_exiting_stops_further_automatic_restart()
    {
        var policy = new CardHostRecoveryPolicy(TightOptions);
        policy.OnMainExiting();
        var decision = policy.OnUnexpectedHostExit();
        Assert.IsType<CardHostRecoveryDecision.Ignore>(decision);
        Assert.True(policy.IsStopped);
    }

    [Fact]
    public void Backoff_grows_until_cap()
    {
        var policy = new CardHostRecoveryPolicy(TightOptions);
        Assert.Equal(TimeSpan.FromMilliseconds(10), policy.ComputeBackoff(0));
        Assert.Equal(TimeSpan.FromMilliseconds(20), policy.ComputeBackoff(1));
        Assert.Equal(TimeSpan.FromMilliseconds(40), policy.ComputeBackoff(2));
        Assert.Equal(TimeSpan.FromMilliseconds(80), policy.ComputeBackoff(3));
        Assert.Equal(TimeSpan.FromMilliseconds(80), policy.ComputeBackoff(10));
    }
}

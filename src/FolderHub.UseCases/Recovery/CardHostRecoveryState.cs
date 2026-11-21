namespace FolderHub.UseCases.Recovery;

/// <summary>
/// Main Process 侧 CardHost 恢复协调状态（ADR-038 候选状态机收敛版）。
/// </summary>
public enum CardHostRecoveryState
{
    /// <summary>已连接或无需宿主（无启用卡片）。</summary>
    Running = 0,

    /// <summary>检测到异常退出，正在判定是否自动重启。</summary>
    HostLost = 1,

    /// <summary>等待退避后再启动。</summary>
    RestartWait = 2,

    /// <summary>正在启动/重连 CardHost。</summary>
    Starting = 3,

    /// <summary>正在同步 CardDefinition / 对账。</summary>
    Restoring = 4,

    /// <summary>自动重启预算耗尽；需用户手动恢复。</summary>
    Degraded = 5,

    /// <summary>Main Process 正在退出；停止一切自动重启。</summary>
    Stopped = 6
}

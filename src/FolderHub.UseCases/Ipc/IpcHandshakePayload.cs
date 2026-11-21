namespace FolderHub.UseCases.Ipc;

/// <summary>
/// 握手载荷（JSON）。instanceToken 由受保护启动上下文派生，不得记入普通日志。
/// </summary>
public sealed class IpcHandshakePayload
{
    public string InstanceToken { get; init; } = "";
    public string Challenge { get; init; } = "";
    public string? ChallengeResponse { get; init; }
    public string ProcessName { get; init; } = "";
    public int ProcessId { get; init; }
}

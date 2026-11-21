namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// 连接双方共享的实例上下文（由受保护启动/发现通道传递）。
/// </summary>
public sealed class IpcConnectionOptions
{
    public required Guid HostInstanceId { get; init; }
    /// <summary>不可猜测的实例令牌；勿写入普通日志。</summary>
    public required string InstanceToken { get; init; }
    public required int SessionId { get; init; }
    public TimeSpan Timeout { get; init; } = FolderHub.UseCases.Ipc.IpcProtocol.DefaultTimeout;

    public static IpcConnectionOptions CreateNew(WindowsSessionIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        return new IpcConnectionOptions
        {
            HostInstanceId = Guid.NewGuid(),
            InstanceToken = Convert.ToHexString(Guid.NewGuid().ToByteArray())
                + Convert.ToHexString(Guid.NewGuid().ToByteArray()),
            SessionId = identity.SessionId
        };
    }
}

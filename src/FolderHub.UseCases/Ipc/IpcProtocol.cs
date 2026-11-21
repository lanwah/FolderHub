namespace FolderHub.UseCases.Ipc;

/// <summary>
/// IPC 协议版本（WP6 v0）。Major 不兼容时拒绝连接。
/// </summary>
public static class IpcProtocol
{
    public const int Major = 0;
    public const int Minor = 1;

    /// <summary>单帧最大载荷（字节），含 JSON 信封。</summary>
    public const int MaxFramePayloadBytes = 256 * 1024;

    /// <summary>握手/读写默认超时。</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
}

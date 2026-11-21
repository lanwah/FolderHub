using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;

namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// 显式 PipeSecurity：以当前 Logon SID 授权读写，不依赖默认 DACL（ADR-036 / P3-07）。
/// 跨会话隔离依赖 Logon SID；User SID 写入握手校验。远程客户端由 CreateNamedPipe 默认拒绝。
/// </summary>
public static class NamedPipeSecurityFactory
{
    public static PipeSecurity CreateForCurrentLogon(WindowsSessionIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        var security = new PipeSecurity();
        // 仅当前 Logon Session：同用户其他会话不因 User SID 单独放行。
        // ReadWrite（非 FullControl）：不含 WriteDac/WriteOwner，避免同会话连接方改 SD。
        security.AddAccessRule(new PipeAccessRule(
            identity.LogonSid,
            PipeAccessRights.ReadWrite,
            AccessControlType.Allow));

        // Owner / 当前用户便于本进程管理句柄。
        security.SetOwner(identity.UserSid);
        return security;
    }
}

using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// 当前进程的会话/用户/Logon SID（Named Pipes DACL 与握手校验）。
/// </summary>
public sealed class WindowsSessionIdentity
{
    public int SessionId { get; }
    public SecurityIdentifier UserSid { get; }
    public SecurityIdentifier LogonSid { get; }
    public string UserSidString => UserSid.Value;
    public string LogonSidString => LogonSid.Value;

    private WindowsSessionIdentity(int sessionId, SecurityIdentifier userSid, SecurityIdentifier logonSid)
    {
        SessionId = sessionId;
        UserSid = userSid;
        LogonSid = logonSid;
    }

    public static WindowsSessionIdentity CaptureCurrent()
    {
        var sessionId = GetCurrentSessionId();
        using var identity = WindowsIdentity.GetCurrent();
        if (identity.User is null)
            throw new InvalidOperationException("无法读取当前用户 SID。");

        var logonSid = GetLogonSid(identity.AccessToken)
            ?? throw new InvalidOperationException("无法读取当前 Logon SID。");

        return new WindowsSessionIdentity(sessionId, identity.User, logonSid);
    }

    public static int GetCurrentSessionId()
    {
        var pid = (uint)Environment.ProcessId;
        if (!ProcessIdToSessionId(pid, out var sessionId))
            throw new InvalidOperationException($"ProcessIdToSessionId 失败：{Marshal.GetLastPInvokeError()}。");
        return unchecked((int)sessionId);
    }

    private static SecurityIdentifier? GetLogonSid(SafeAccessTokenHandle token)
    {
        if (!GetTokenInformation(token, TokenGroups, IntPtr.Zero, 0, out var needed)
            && Marshal.GetLastPInvokeError() != ErrorInsufficientBuffer)
        {
            return null;
        }

        var buffer = Marshal.AllocHGlobal(needed);
        try
        {
            if (!GetTokenInformation(token, TokenGroups, buffer, needed, out _))
                return null;

            var groupCount = Marshal.ReadInt32(buffer);
            // TOKEN_GROUPS：GroupCount 后按指针对齐 SID_AND_ATTRIBUTES 数组。
            var arrayOffset = IntPtr.Size; // 4 on x86, 8 on x64 (含 padding)
            var entrySize = IntPtr.Size + 4 + (IntPtr.Size == 8 ? 4 : 0); // PSID + Attributes (+ x64 pad)

            for (var i = 0; i < groupCount; i++)
            {
                var entry = IntPtr.Add(buffer, arrayOffset + i * entrySize);
                var sidPtr = Marshal.ReadIntPtr(entry);
                var attributes = Marshal.ReadInt32(IntPtr.Add(entry, IntPtr.Size));
                if ((attributes & SeGroupLogonId) == 0 || sidPtr == IntPtr.Zero)
                    continue;

                return new SecurityIdentifier(sidPtr);
            }

            return null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private const int TokenGroups = 2;
    private const int ErrorInsufficientBuffer = 122;
    private const int SeGroupLogonId = unchecked((int)0xC0000000);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetTokenInformation(
        SafeAccessTokenHandle tokenHandle,
        int tokenInformationClass,
        IntPtr tokenInformation,
        int tokenInformationLength,
        out int returnLength);
}

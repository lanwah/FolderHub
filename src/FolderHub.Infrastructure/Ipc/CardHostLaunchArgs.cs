using System.Globalization;

namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// CardHost 命令行启动参数（与会话文件互为备份；token 勿记入日志）。
/// </summary>
public static class CardHostLaunchArgs
{
    public const string HostInstanceId = "--ipc-host-instance-id";
    public const string InstanceToken = "--ipc-instance-token";
    public const string SessionId = "--ipc-session-id";

    public static string[] Build(IpcConnectionOptions options)
        =>
        [
            HostInstanceId, options.HostInstanceId.ToString("D"),
            InstanceToken, options.InstanceToken,
            SessionId, options.SessionId.ToString(CultureInfo.InvariantCulture)
        ];

    public static bool TryParse(string[] args, out IpcConnectionOptions? options)
    {
        options = null;
        if (args is null || args.Length == 0)
            return false;

        string? hostId = null;
        string? token = null;
        string? session = null;

        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], HostInstanceId, StringComparison.OrdinalIgnoreCase))
                hostId = args[++i];
            else if (string.Equals(args[i], InstanceToken, StringComparison.OrdinalIgnoreCase))
                token = args[++i];
            else if (string.Equals(args[i], SessionId, StringComparison.OrdinalIgnoreCase))
                session = args[++i];
        }

        if (hostId is null || token is null || session is null)
            return false;

        if (!Guid.TryParse(hostId, out var hostInstanceId))
            return false;
        if (!int.TryParse(session, NumberStyles.Integer, CultureInfo.InvariantCulture, out var sessionId))
            return false;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        options = new IpcConnectionOptions
        {
            HostInstanceId = hostInstanceId,
            InstanceToken = token.Trim(),
            SessionId = sessionId
        };
        return true;
    }
}

using System.Text.Json;
using FolderHub.Infrastructure.Storage;

namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// Main↔CardHost 受保护启动通道：会话文件位于用户 LocalAppData（勿写入普通日志中的 token）。
/// </summary>
public static class CardHostSessionFile
{
    public const string FileName = "cardhost.ipc.session.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string GetPath()
        => Path.Combine(AppDataPaths.GetAppDirectory(), FileName);

    public static void Write(CardHostSessionRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        AppDataPaths.EnsureAppDirectory();
        var path = GetPath();
        var tmp = path + ".tmp";
        var json = JsonSerializer.Serialize(record, JsonOptions);
        File.WriteAllText(tmp, json);
        File.Copy(tmp, path, overwrite: true);
        File.Delete(tmp);
    }

    public static CardHostSessionRecord? TryRead()
    {
        var path = GetPath();
        if (!File.Exists(path))
            return null;

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<CardHostSessionRecord>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static void TryDelete()
    {
        try
        {
            var path = GetPath();
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // 忽略清理失败。
        }
    }

    public static IpcConnectionOptions ToOptions(CardHostSessionRecord record)
        => new()
        {
            HostInstanceId = record.HostInstanceId,
            InstanceToken = record.InstanceToken,
            SessionId = record.SessionId
        };
}

public sealed record CardHostSessionRecord(
    Guid HostInstanceId,
    string InstanceToken,
    int SessionId,
    int ProcessId);

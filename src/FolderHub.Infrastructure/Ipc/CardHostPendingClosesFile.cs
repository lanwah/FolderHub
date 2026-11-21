using System.Text.Json;
using FolderHub.Infrastructure.Storage;

namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// Main 离线时 CardHost「取消固定」的待处理队列（不写 CardDefinition，遵守 ADR-035）。
/// Main 启动/重连时消费并权威删除定义；CardHost 加载/展示时跳过这些 CardId。
/// </summary>
public static class CardHostPendingClosesFile
{
    public const string FileName = "cardhost.pending-closes.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static readonly object Gate = new();

    public static string GetPath()
        => Path.Combine(AppDataPaths.GetAppDirectory(), FileName);

    public static void Enqueue(Guid cardId)
    {
        lock (Gate)
        {
            var set = ReadUnlocked();
            if (!set.Add(cardId))
                return;
            WriteUnlocked(set);
        }
    }

    public static IReadOnlySet<Guid> Peek()
    {
        lock (Gate)
            return ReadUnlocked();
    }

    public static IReadOnlyList<Guid> ConsumeAll()
    {
        lock (Gate)
        {
            var set = ReadUnlocked();
            TryDeleteUnlocked();
            return set.ToList();
        }
    }

    public static void Remove(Guid cardId)
    {
        lock (Gate)
        {
            var set = ReadUnlocked();
            if (!set.Remove(cardId))
                return;
            if (set.Count == 0)
                TryDeleteUnlocked();
            else
                WriteUnlocked(set);
        }
    }

    private static HashSet<Guid> ReadUnlocked()
    {
        var path = GetPath();
        if (!File.Exists(path))
            return new HashSet<Guid>();

        try
        {
            var json = File.ReadAllText(path);
            var record = JsonSerializer.Deserialize<PendingClosesRecord>(json, JsonOptions);
            return record?.CardIds?.ToHashSet() ?? new HashSet<Guid>();
        }
        catch
        {
            return new HashSet<Guid>();
        }
    }

    private static void WriteUnlocked(HashSet<Guid> cardIds)
    {
        AppDataPaths.EnsureAppDirectory();
        var path = GetPath();
        var tmp = path + ".tmp";
        var json = JsonSerializer.Serialize(new PendingClosesRecord(cardIds.ToArray()), JsonOptions);
        File.WriteAllText(tmp, json);
        File.Copy(tmp, path, overwrite: true);
        File.Delete(tmp);
    }

    private static void TryDeleteUnlocked()
    {
        try
        {
            var path = GetPath();
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignore
        }
    }

    private sealed record PendingClosesRecord(Guid[] CardIds);
}

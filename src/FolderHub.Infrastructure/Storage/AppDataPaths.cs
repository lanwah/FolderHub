namespace FolderHub.Infrastructure.Storage;

/// <summary>
/// 本地数据目录：%LocalAppData%\FolderHub\（开发计划 / ADR-039）。
/// </summary>
public static class AppDataPaths
{
    public const string AppFolderName = "FolderHub";
    public const string DatabaseFileName = "folderhub.db";

    public static string GetAppDirectory()
    {
        var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(root, AppFolderName);
    }

    public static string GetDefaultDatabasePath()
        => Path.Combine(GetAppDirectory(), DatabaseFileName);

    public static void EnsureAppDirectory()
        => Directory.CreateDirectory(GetAppDirectory());
}

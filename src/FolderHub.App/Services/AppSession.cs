using FolderHub.Domain.Catalog;
using FolderHub.Infrastructure.Shell;
using FolderHub.Infrastructure.Sqlite;
using FolderHub.Infrastructure.Storage;
using FolderHub.UseCases.Persistence;
using FolderHub.UseCases.Shell;

namespace FolderHub.App.Services;

/// <summary>
/// Main Process 会话：SQLite 生命周期与权威聚合/设置的读写入口。
/// </summary>
public sealed class AppSession : IDisposable
{
    private readonly SqliteDb _db;

    public AppSession(
        SqliteDb db,
        IFolderCatalogStore catalogStore,
        IMainSettingsStore settingsStore,
        IShellFolderService shell,
        IStartupRegistrationService startup)
    {
        _db = db;
        CatalogStore = catalogStore;
        SettingsStore = settingsStore;
        Shell = shell;
        Startup = startup;
        Catalog = catalogStore.Load();
        Settings = settingsStore.Load();
    }

    public static AppSession OpenDefault()
    {
        var db = SqliteDb.OpenDefault();
        db.Migrate();
        return new AppSession(
            db,
            new SqliteFolderCatalogStore(db),
            new SqliteMainSettingsStore(db),
            new WindowsShellFolderService(),
            new WindowsStartupRegistrationService());
    }

    public string DatabasePath => AppDataPaths.GetDefaultDatabasePath();

    public IFolderCatalogStore CatalogStore { get; }

    public IMainSettingsStore SettingsStore { get; }

    public IShellFolderService Shell { get; }

    public IStartupRegistrationService Startup { get; }

    public FolderCatalog Catalog { get; private set; }

    public MainSettingsSnapshot Settings { get; private set; }

    public void SaveCatalog() => CatalogStore.Save(Catalog);

    /// <summary>仅写入 SQLite 偏好（窗口几何等）；不触碰开机启动注册表。</summary>
    public void SaveSettings(MainSettingsSnapshot settings)
    {
        SettingsStore.Save(settings);
        Settings = settings;
    }

    /// <summary>启动时按已持久化偏好同步启动项（修复手工删改注册表后的漂移）。</summary>
    public void SyncStartupRegistration() => TryApplyStartup(Settings.StartupEnabled, out _);

    /// <summary>保存设置并同步启动项；启动项失败时仍保留库内偏好，返回 false。</summary>
    public bool SaveSettingsWithStartup(MainSettingsSnapshot settings, out string? startupError)
    {
        SaveSettings(settings);
        return TryApplyStartup(settings.StartupEnabled, out startupError);
    }

    private bool TryApplyStartup(bool enabled, out string? error)
    {
        error = null;
        try
        {
            Startup.Apply(enabled);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public void ReloadCatalog() => Catalog = CatalogStore.Load();

    public void Dispose() => _db.Dispose();
}

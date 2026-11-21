using FolderHub.Domain.Catalog;
using FolderHub.Infrastructure.Sqlite;
using FolderHub.Infrastructure.Storage;
using FolderHub.UseCases.Persistence;

namespace FolderHub.App.Services;

/// <summary>
/// Main Process 会话：SQLite 生命周期与权威聚合/设置的读写入口。
/// </summary>
public sealed class AppSession : IDisposable
{
    private readonly SqliteDb _db;

    public AppSession(SqliteDb db, IFolderCatalogStore catalogStore, IMainSettingsStore settingsStore)
    {
        _db = db;
        CatalogStore = catalogStore;
        SettingsStore = settingsStore;
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
            new SqliteMainSettingsStore(db));
    }

    public string DatabasePath => AppDataPaths.GetDefaultDatabasePath();

    public IFolderCatalogStore CatalogStore { get; }

    public IMainSettingsStore SettingsStore { get; }

    public FolderCatalog Catalog { get; private set; }

    public MainSettingsSnapshot Settings { get; private set; }

    public void SaveCatalog() => CatalogStore.Save(Catalog);

    public void SaveSettings(MainSettingsSnapshot settings)
    {
        SettingsStore.Save(settings);
        Settings = settings;
    }

    public void ReloadCatalog() => Catalog = CatalogStore.Load();

    public void Dispose() => _db.Dispose();
}

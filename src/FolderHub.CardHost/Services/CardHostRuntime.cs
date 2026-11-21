using FolderHub.Domain.Catalog;
using FolderHub.Infrastructure.Display;
using FolderHub.Infrastructure.Ipc;
using FolderHub.Infrastructure.Shell;
using FolderHub.Infrastructure.Sqlite;
using FolderHub.UseCases.Display;
using FolderHub.UseCases.Persistence;
using FolderHub.UseCases.Shell;

namespace FolderHub.CardHost.Services;

/// <summary>
/// CardHost 进程运行时：只读权威聚合 + 展示态写入口（ADR-035）。
/// </summary>
public sealed class CardHostRuntime : IDisposable
{
    private readonly SqliteDb _db;

    public CardHostRuntime(
        SqliteDb db,
        IpcConnectionOptions ipcOptions,
        ICardDefinitionStore definitions,
        ICardPresentationStateStore presentations,
        IFolderCatalogStore catalogStore,
        IShellFolderService shell,
        IDisplayMonitorCatalog displays,
        IMainSettingsStore settings)
    {
        _db = db;
        IpcOptions = ipcOptions;
        Definitions = definitions;
        Presentations = presentations;
        CatalogStore = catalogStore;
        Shell = shell;
        Displays = displays;
        Settings = settings;
    }

    public static CardHostRuntime OpenDefault(IpcConnectionOptions ipcOptions)
    {
        var db = SqliteDb.OpenDefault();
        db.Migrate();
        return new CardHostRuntime(
            db,
            ipcOptions,
            new SqliteCardDefinitionStore(db),
            new SqliteCardPresentationStateStore(db),
            new SqliteFolderCatalogStore(db),
            new WindowsShellFolderService(),
            new WindowsDisplayMonitorCatalog(),
            new SqliteMainSettingsStore(db));
    }

    public IpcConnectionOptions IpcOptions { get; }

    public ICardDefinitionStore Definitions { get; }

    public ICardPresentationStateStore Presentations { get; }

    public IFolderCatalogStore CatalogStore { get; }

    public IShellFolderService Shell { get; }

    public IDisplayMonitorCatalog Displays { get; }

    public IMainSettingsStore Settings { get; }

    public FolderCatalog LoadCatalog() => CatalogStore.Load();

    public string LoadUiTheme() => UiTheme.Normalize(Settings.Load().UiTheme);

    public void Dispose() => _db.Dispose();
}

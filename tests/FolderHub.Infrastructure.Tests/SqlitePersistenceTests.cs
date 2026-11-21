using FolderHub.Domain.Catalog;
using FolderHub.Infrastructure.Sqlite;
using FolderHub.UseCases.Persistence;

namespace FolderHub.Infrastructure.Tests;

public class SqlitePersistenceTests
{
    [Fact]
    public void Migrate_CreatesSchemaVersion1_AndSeedsUncategorized()
    {
        using var db = SqliteDb.OpenInMemory();
        db.Migrate();

        Assert.Equal(1, db.GetSchemaVersion());

        var catalog = new SqliteFolderCatalogStore(db).Load();
        Assert.Single(catalog.Categories);
        Assert.True(catalog.Uncategorized.IsUncategorized);
    }

    [Fact]
    public void FolderCatalog_RoundTrip_PreservesCategoriesFoldersAndLinks()
    {
        using var db = SqliteDb.OpenInMemory();
        db.Migrate();
        var store = new SqliteFolderCatalogStore(db);

        var catalog = store.Load();
        var work = catalog.AddCategory("工作");
        var personal = catalog.AddCategory("个人");
        var a = catalog.AddFolder(@"D:\Projects\A\", "A", work.Id, alias: "项目A", description: "说明").Folder;
        catalog.AddFolder(@"d:\projects\a", "A", personal.Id);
        catalog.AddFolder(@"D:\Projects\B", "B", work.Id);
        catalog.ReorderFoldersInCategory(work.Id, [catalog.GetFoldersInCategory(work.Id)[1].Id, a.Id]);
        store.Save(catalog);

        var loaded = store.Load();
        Assert.Equal(3, loaded.Categories.Count);
        Assert.Single(loaded.Folders.Where(f => f.Id == a.Id));
        Assert.Equal("项目A", loaded.Folders.Single(f => f.Id == a.Id).Alias);
        Assert.Equal(2, loaded.GetCategoriesForFolder(a.Id).Count);
        Assert.Equal(["B", "项目A"], loaded.GetFoldersInCategory(work.Id).Select(f => f.ListTitle).ToArray());
    }

    [Fact]
    public void FolderCatalog_Save_DoesNotWipeCardDefinitions_ForExistingFolders()
    {
        using var db = SqliteDb.OpenInMemory();
        db.Migrate();
        var catalogStore = new SqliteFolderCatalogStore(db);
        var cardStore = new SqliteCardDefinitionStore(db);

        var catalog = catalogStore.Load();
        var work = catalog.AddCategory("工作");
        var folder = catalog.AddFolder(@"D:\Keep", "Keep", work.Id).Folder;
        catalogStore.Save(catalog);

        var cardId = Guid.NewGuid();
        cardStore.ReplaceAll([new CardDefinitionSnapshot(cardId, folder.Id, Enabled: true)]);

        catalog.AddCategory("归档");
        catalogStore.Save(catalog);

        var cards = cardStore.LoadAll();
        Assert.Single(cards);
        Assert.Equal(cardId, cards[0].CardId);
        Assert.Equal(folder.Id, cards[0].FolderId);
    }

    [Fact]
    public void FolderCatalog_DeleteFolder_CascadesCardDefinition()
    {
        using var db = SqliteDb.OpenInMemory();
        db.Migrate();
        var catalogStore = new SqliteFolderCatalogStore(db);
        var cardStore = new SqliteCardDefinitionStore(db);

        var catalog = catalogStore.Load();
        var work = catalog.AddCategory("工作");
        var folder = catalog.AddFolder(@"D:\Gone", "Gone", work.Id).Folder;
        catalogStore.Save(catalog);
        cardStore.ReplaceAll([new CardDefinitionSnapshot(Guid.NewGuid(), folder.Id, true)]);

        catalog.DeleteFolder(folder.Id);
        catalogStore.Save(catalog);

        Assert.Empty(cardStore.LoadAll());
    }

    [Fact]
    public void CardDefinition_ReplaceAll_SameCardId_PreservesPresentationState()
    {
        using var db = SqliteDb.OpenInMemory();
        db.Migrate();
        var catalogStore = new SqliteFolderCatalogStore(db);
        var cardStore = new SqliteCardDefinitionStore(db);

        var catalog = catalogStore.Load();
        var work = catalog.AddCategory("工作");
        var folder = catalog.AddFolder(@"D:\Card", "Card", work.Id).Folder;
        catalogStore.Save(catalog);

        var cardId = Guid.NewGuid();
        cardStore.ReplaceAll([new CardDefinitionSnapshot(cardId, folder.Id, Enabled: true)]);
        InsertPresentationState(db, cardId, left: 100, top: 200, stateVersion: 7);

        cardStore.ReplaceAll([new CardDefinitionSnapshot(cardId, folder.Id, Enabled: false)]);

        var cards = cardStore.LoadAll();
        Assert.Single(cards);
        Assert.False(cards[0].Enabled);

        var (left, top, version) = ReadPresentationState(db, cardId);
        Assert.Equal(100, left);
        Assert.Equal(200, top);
        Assert.Equal(7, version);
    }

    [Fact]
    public void CardDefinition_ReplaceAll_RemovesMissingCards_Only()
    {
        using var db = SqliteDb.OpenInMemory();
        db.Migrate();
        var catalogStore = new SqliteFolderCatalogStore(db);
        var cardStore = new SqliteCardDefinitionStore(db);

        var catalog = catalogStore.Load();
        var work = catalog.AddCategory("工作");
        var keep = catalog.AddFolder(@"D:\Keep2", "Keep2", work.Id).Folder;
        var drop = catalog.AddFolder(@"D:\Drop", "Drop", work.Id).Folder;
        catalogStore.Save(catalog);

        var keepId = Guid.NewGuid();
        var dropId = Guid.NewGuid();
        cardStore.ReplaceAll(
        [
            new CardDefinitionSnapshot(keepId, keep.Id, true),
            new CardDefinitionSnapshot(dropId, drop.Id, true)
        ]);
        InsertPresentationState(db, keepId, left: 1, top: 2, stateVersion: 1);
        InsertPresentationState(db, dropId, left: 3, top: 4, stateVersion: 2);

        cardStore.ReplaceAll([new CardDefinitionSnapshot(keepId, keep.Id, true)]);

        Assert.Single(cardStore.LoadAll());
        Assert.Equal(1, ReadPresentationState(db, keepId).Version);
        Assert.False(PresentationExists(db, dropId));
    }

    private static void InsertPresentationState(SqliteDb db, Guid cardId, double left, double top, long stateVersion)
    {
        using var connection = db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO card_presentation_states
                (card_id, left, top, locked, monitor_reference, dpi_reference, state_version, updated_at_utc)
            VALUES ($id, $left, $top, 0, NULL, NULL, $ver, $utc);
            """;
        cmd.Parameters.AddWithValue("$id", cardId.ToString());
        cmd.Parameters.AddWithValue("$left", left);
        cmd.Parameters.AddWithValue("$top", top);
        cmd.Parameters.AddWithValue("$ver", stateVersion);
        cmd.Parameters.AddWithValue("$utc", DateTime.UtcNow.ToString("O"));
        cmd.ExecuteNonQuery();
    }

    private static (double Left, double Top, long Version) ReadPresentationState(SqliteDb db, Guid cardId)
    {
        using var connection = db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT left, top, state_version
            FROM card_presentation_states
            WHERE card_id = $id;
            """;
        cmd.Parameters.AddWithValue("$id", cardId.ToString());
        using var reader = cmd.ExecuteReader();
        Assert.True(reader.Read());
        return (reader.GetDouble(0), reader.GetDouble(1), reader.GetInt64(2));
    }

    private static bool PresentationExists(SqliteDb db, Guid cardId)
    {
        using var connection = db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM card_presentation_states WHERE card_id = $id;";
        cmd.Parameters.AddWithValue("$id", cardId.ToString());
        return cmd.ExecuteScalar() is not null;
    }

    [Fact]
    public void MainSettings_RoundTrip()
    {
        using var db = SqliteDb.OpenInMemory();
        db.Migrate();
        var store = new SqliteMainSettingsStore(db);

        var defaults = store.Load();
        Assert.True(defaults.KeepCardsAfterMainExit);

        store.Save(defaults with
        {
            KeepCardsAfterMainExit = false,
            MainPinned = true,
            MainLeft = 12,
            MainTop = 34,
            MainOpacity = 0.85,
            StartupEnabled = true
        });

        var loaded = store.Load();
        Assert.False(loaded.KeepCardsAfterMainExit);
        Assert.True(loaded.MainPinned);
        Assert.Equal(12, loaded.MainLeft);
        Assert.Equal(34, loaded.MainTop);
        Assert.Equal(0.85, loaded.MainOpacity);
        Assert.True(loaded.StartupEnabled);
    }

    [Fact]
    public void Migrate_OnFileDatabase_IsIdempotent()
    {
        var path = Path.Combine(Path.GetTempPath(), "FolderHubTests", Guid.NewGuid().ToString("N"), "folderhub.db");
        try
        {
            using (var db = new SqliteDb(path))
            {
                db.Migrate();
                Assert.Equal(1, db.GetSchemaVersion());
                db.Migrate();
                Assert.Equal(1, db.GetSchemaVersion());
            }

            using (var db = new SqliteDb(path))
            {
                db.Migrate();
                var catalog = new SqliteFolderCatalogStore(db).Load();
                Assert.True(catalog.Uncategorized.IsUncategorized);
            }
        }
        finally
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            var dir = Path.GetDirectoryName(path);
            if (dir is not null && Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
        }
    }
}

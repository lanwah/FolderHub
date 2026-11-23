using FolderHub.Domain.Catalog;
using FolderHub.Domain.Entities;
using FolderHub.Domain.Services;
using FolderHub.UseCases.Persistence;
using Microsoft.Data.Sqlite;

namespace FolderHub.Infrastructure.Sqlite;

public sealed class SqliteFolderCatalogStore : IFolderCatalogStore
{
    private readonly SqliteDb _db;

    public SqliteFolderCatalogStore(SqliteDb db) => _db = db;

    public FolderCatalog Load()
    {
        using var connection = _db.OpenConnection();

        var categories = ReadCategories(connection);
        if (categories.Count == 0)
            return FolderCatalog.CreateEmpty();

        var folders = ReadFolders(connection);
        var links = ReadLinks(connection);
        return FolderCatalog.Restore(categories, folders, links);
    }

    public void Save(FolderCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        using var connection = _db.OpenConnection();
        using var tx = connection.BeginTransaction();

        // 差分写入，避免 DELETE 全部 folders 时级联清掉 Main 权威的 card_definitions。
        var existingCategoryIds = ReadIdSet(connection, tx, "SELECT id FROM categories;");
        var existingFolderIds = ReadIdSet(connection, tx, "SELECT id FROM folders;");

        var nextCategoryIds = catalog.Categories.Select(c => c.Id.ToString()).ToHashSet(StringComparer.Ordinal);
        var nextFolderIds = catalog.Folders.Select(f => f.Id.ToString()).ToHashSet(StringComparer.Ordinal);

        foreach (var category in catalog.Categories)
            UpsertCategory(connection, tx, category);

        foreach (var folder in catalog.Folders)
            UpsertFolder(connection, tx, folder);

        Execute(connection, tx, "DELETE FROM folder_category_links;");
        foreach (var link in catalog.Links)
            InsertLink(connection, tx, link);

        foreach (var id in existingFolderIds.Where(id => !nextFolderIds.Contains(id)))
        {
            using var cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM folders WHERE id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        foreach (var id in existingCategoryIds.Where(id => !nextCategoryIds.Contains(id)))
        {
            using var cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM categories WHERE id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
    }

    private static List<Category> ReadCategories(SqliteConnection connection)
    {
        var categories = new List<Category>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT id, name, sort_order, is_uncategorized, icon
            FROM categories
            ORDER BY sort_order;
            """;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            categories.Add(new Category(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3) == 1,
                reader.IsDBNull(4) ? null : reader.GetString(4)));
        }

        return categories;
    }

    private static List<FolderRecord> ReadFolders(SqliteConnection connection)
    {
        var folders = new List<FolderRecord>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT id, path, display_name, alias, description, is_path_unavailable, icon
            FROM folders;
            """;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            folders.Add(new FolderRecord(
                Guid.Parse(reader.GetString(0)),
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                reader.GetInt32(5) == 1,
                reader.IsDBNull(6) ? null : reader.GetString(6)));
        }

        return folders;
    }

    private static List<FolderCategoryLink> ReadLinks(SqliteConnection connection)
    {
        var links = new List<FolderCategoryLink>();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT folder_id, category_id, sort_order
            FROM folder_category_links;
            """;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            links.Add(new FolderCategoryLink(
                Guid.Parse(reader.GetString(0)),
                Guid.Parse(reader.GetString(1)),
                reader.GetInt32(2)));
        }

        return links;
    }

    private static HashSet<string> ReadIdSet(SqliteConnection connection, SqliteTransaction tx, string sql)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            set.Add(reader.GetString(0));
        return set;
    }

    private static void UpsertCategory(SqliteConnection connection, SqliteTransaction tx, Category category)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO categories (id, name, sort_order, is_uncategorized, icon)
            VALUES ($id, $name, $sort, $unc, $icon)
            ON CONFLICT(id) DO UPDATE SET
                name = excluded.name,
                sort_order = excluded.sort_order,
                is_uncategorized = excluded.is_uncategorized,
                icon = excluded.icon;
            """;
        cmd.Parameters.AddWithValue("$id", category.Id.ToString());
        cmd.Parameters.AddWithValue("$name", category.Name);
        cmd.Parameters.AddWithValue("$sort", category.SortOrder);
        cmd.Parameters.AddWithValue("$unc", category.IsUncategorized ? 1 : 0);
        cmd.Parameters.AddWithValue("$icon", (object?)category.Icon ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    private static void UpsertFolder(SqliteConnection connection, SqliteTransaction tx, FolderRecord folder)
    {
        var normalized = FolderPathComparer.Normalize(folder.Path);
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO folders
                (id, path, path_key, display_name, alias, description, is_path_unavailable, icon)
            VALUES ($id, $path, $path_key, $display, $alias, $desc, $unavailable, $icon)
            ON CONFLICT(id) DO UPDATE SET
                path = excluded.path,
                path_key = excluded.path_key,
                display_name = excluded.display_name,
                alias = excluded.alias,
                description = excluded.description,
                is_path_unavailable = excluded.is_path_unavailable,
                icon = excluded.icon;
            """;
        cmd.Parameters.AddWithValue("$id", folder.Id.ToString());
        cmd.Parameters.AddWithValue("$path", normalized);
        cmd.Parameters.AddWithValue("$path_key", normalized.ToLowerInvariant());
        cmd.Parameters.AddWithValue("$display", folder.DisplayName);
        cmd.Parameters.AddWithValue("$alias", (object?)folder.Alias ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$desc", (object?)folder.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$unavailable", folder.IsPathUnavailable ? 1 : 0);
        cmd.Parameters.AddWithValue("$icon", (object?)folder.Icon ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    private static void InsertLink(SqliteConnection connection, SqliteTransaction tx, FolderCategoryLink link)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO folder_category_links (folder_id, category_id, sort_order)
            VALUES ($folder, $category, $sort);
            """;
        cmd.Parameters.AddWithValue("$folder", link.FolderId.ToString());
        cmd.Parameters.AddWithValue("$category", link.CategoryId.ToString());
        cmd.Parameters.AddWithValue("$sort", link.SortOrder);
        cmd.ExecuteNonQuery();
    }

    private static void Execute(SqliteConnection connection, SqliteTransaction tx, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}

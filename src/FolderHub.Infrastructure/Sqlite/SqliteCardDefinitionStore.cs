using FolderHub.UseCases.Persistence;
using Microsoft.Data.Sqlite;

namespace FolderHub.Infrastructure.Sqlite;

public sealed class SqliteCardDefinitionStore : ICardDefinitionStore
{
    private readonly SqliteDb _db;

    public SqliteCardDefinitionStore(SqliteDb db) => _db = db;

    public IReadOnlyList<CardDefinitionSnapshot> LoadAll()
    {
        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT card_id, folder_id, enabled
            FROM card_definitions
            ORDER BY card_id;
            """;

        var list = new List<CardDefinitionSnapshot>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new CardDefinitionSnapshot(
                Guid.Parse(reader.GetString(0)),
                Guid.Parse(reader.GetString(1)),
                reader.GetInt32(2) == 1));
        }

        return list;
    }

    public void ReplaceAll(IEnumerable<CardDefinitionSnapshot> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var next = definitions.ToList();
        if (next.Select(d => d.CardId).Distinct().Count() != next.Count)
            throw new ArgumentException("ReplaceAll 不允许重复的 CardId。", nameof(definitions));
        if (next.Select(d => d.FolderId).Distinct().Count() != next.Count)
            throw new ArgumentException("每个 FolderId 至多一条 CardDefinition。", nameof(definitions));

        using var connection = _db.OpenConnection();
        using var tx = connection.BeginTransaction();

        var nextCardIds = next.Select(d => d.CardId.ToString()).ToHashSet(StringComparer.Ordinal);
        var existingCardIds = ReadIdSet(connection, tx, "SELECT card_id FROM card_definitions;");

        // 先删真正移除的卡片（其 presentation 级联删除是预期行为）。
        foreach (var id in existingCardIds.Where(id => !nextCardIds.Contains(id)))
        {
            using var cmd = connection.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = "DELETE FROM card_definitions WHERE card_id = $id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // 存活/新增卡片只做 upsert，保留同 CardId 的 card_presentation_states（ADR-035）。
        foreach (var definition in next)
            UpsertDefinition(connection, tx, definition);

        tx.Commit();
    }

    private static void UpsertDefinition(
        SqliteConnection connection,
        SqliteTransaction tx,
        CardDefinitionSnapshot definition)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO card_definitions (card_id, folder_id, enabled)
            VALUES ($card, $folder, $enabled)
            ON CONFLICT(card_id) DO UPDATE SET
                folder_id = excluded.folder_id,
                enabled = excluded.enabled;
            """;
        cmd.Parameters.AddWithValue("$card", definition.CardId.ToString());
        cmd.Parameters.AddWithValue("$folder", definition.FolderId.ToString());
        cmd.Parameters.AddWithValue("$enabled", definition.Enabled ? 1 : 0);

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException(
                "CardDefinition 的 FolderId 与另一张存活卡片冲突。请先调整对端卡片或拆成两步 ReplaceAll。",
                ex);
        }
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
}

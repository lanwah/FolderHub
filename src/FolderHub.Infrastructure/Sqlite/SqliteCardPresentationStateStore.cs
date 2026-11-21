using FolderHub.UseCases.Persistence;
using Microsoft.Data.Sqlite;

namespace FolderHub.Infrastructure.Sqlite;

public sealed class SqliteCardPresentationStateStore : ICardPresentationStateStore
{
    private readonly SqliteDb _db;

    public SqliteCardPresentationStateStore(SqliteDb db) => _db = db;

    public IReadOnlyList<CardPresentationStateSnapshot> LoadAll()
    {
        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT card_id, left, top, locked, monitor_reference, dpi_reference, state_version, updated_at_utc
            FROM card_presentation_states
            ORDER BY card_id;
            """;

        var list = new List<CardPresentationStateSnapshot>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(ReadRow(reader));
        return list;
    }

    public CardPresentationStateSnapshot? Load(Guid cardId)
    {
        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT card_id, left, top, locked, monitor_reference, dpi_reference, state_version, updated_at_utc
            FROM card_presentation_states
            WHERE card_id = $id;
            """;
        cmd.Parameters.AddWithValue("$id", cardId.ToString());
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? ReadRow(reader) : null;
    }

    public void Upsert(CardPresentationStateSnapshot state)
    {
        ArgumentNullException.ThrowIfNull(state);

        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO card_presentation_states
                (card_id, left, top, locked, monitor_reference, dpi_reference, state_version, updated_at_utc)
            VALUES ($id, $left, $top, $locked, $monitor, $dpi, $ver, $utc)
            ON CONFLICT(card_id) DO UPDATE SET
                left = excluded.left,
                top = excluded.top,
                locked = excluded.locked,
                monitor_reference = excluded.monitor_reference,
                dpi_reference = excluded.dpi_reference,
                state_version = excluded.state_version,
                updated_at_utc = excluded.updated_at_utc;
            """;
        cmd.Parameters.AddWithValue("$id", state.CardId.ToString());
        cmd.Parameters.AddWithValue("$left", state.Left);
        cmd.Parameters.AddWithValue("$top", state.Top);
        cmd.Parameters.AddWithValue("$locked", state.Locked ? 1 : 0);
        cmd.Parameters.AddWithValue("$monitor", (object?)state.MonitorReference ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$dpi", state.DpiReference is double d ? d : DBNull.Value);
        cmd.Parameters.AddWithValue("$ver", state.StateVersion);
        cmd.Parameters.AddWithValue("$utc", state.UpdatedAtUtc.UtcDateTime.ToString("O"));

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            throw new InvalidOperationException(
                "无法写入展示态：对应 CardDefinition 不存在（外键约束）。",
                ex);
        }
    }

    public void Delete(Guid cardId)
    {
        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM card_presentation_states WHERE card_id = $id;";
        cmd.Parameters.AddWithValue("$id", cardId.ToString());
        cmd.ExecuteNonQuery();
    }

    private static CardPresentationStateSnapshot ReadRow(SqliteDataReader reader)
    {
        return new CardPresentationStateSnapshot(
            Guid.Parse(reader.GetString(0)),
            reader.GetDouble(1),
            reader.GetDouble(2),
            reader.GetInt32(3) == 1,
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetDouble(5),
            reader.GetInt64(6),
            DateTimeOffset.Parse(reader.GetString(7), null, System.Globalization.DateTimeStyles.RoundtripKind));
    }
}

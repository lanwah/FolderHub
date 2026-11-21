using FolderHub.UseCases.Persistence;
using Microsoft.Data.Sqlite;

namespace FolderHub.Infrastructure.Sqlite;

public sealed class SqliteMainSettingsStore : IMainSettingsStore
{
    private readonly SqliteDb _db;

    public SqliteMainSettingsStore(SqliteDb db) => _db = db;

    public MainSettingsSnapshot Load()
    {
        using var connection = _db.OpenConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT
                g.keep_cards_after_main_exit,
                m.pinned, m.left, m.top, m.width, m.height, m.locked, m.opacity, m.startup_enabled,
                m.monitor_reference, m.dpi_reference
            FROM global_settings g
            CROSS JOIN main_component_settings m
            WHERE g.id = 1 AND m.id = 1;
            """;

        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return new MainSettingsSnapshot(
                KeepCardsAfterMainExit: true,
                MainPinned: false,
                MainLeft: 80,
                MainTop: 80,
                MainWidth: 420,
                MainHeight: 560,
                MainLocked: false,
                MainOpacity: 1.0,
                StartupEnabled: false);
        }

        return new MainSettingsSnapshot(
            KeepCardsAfterMainExit: reader.GetInt32(0) == 1,
            MainPinned: reader.GetInt32(1) == 1,
            MainLeft: reader.GetDouble(2),
            MainTop: reader.GetDouble(3),
            MainWidth: reader.GetDouble(4),
            MainHeight: reader.GetDouble(5),
            MainLocked: reader.GetInt32(6) == 1,
            MainOpacity: reader.GetDouble(7),
            StartupEnabled: reader.GetInt32(8) == 1,
            MainMonitorReference: reader.IsDBNull(9) ? null : reader.GetString(9),
            MainDpiReference: reader.IsDBNull(10) ? null : reader.GetDouble(10));
    }

    public void Save(MainSettingsSnapshot settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        using var connection = _db.OpenConnection();
        using var tx = connection.BeginTransaction();

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO global_settings (id, keep_cards_after_main_exit)
                VALUES (1, $keep)
                ON CONFLICT(id) DO UPDATE SET keep_cards_after_main_exit = excluded.keep_cards_after_main_exit;
                """;
            cmd.Parameters.AddWithValue("$keep", settings.KeepCardsAfterMainExit ? 1 : 0);
            cmd.ExecuteNonQuery();
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = """
                INSERT INTO main_component_settings
                    (id, pinned, left, top, width, height, locked, opacity, startup_enabled, monitor_reference, dpi_reference)
                VALUES (1, $pinned, $left, $top, $width, $height, $locked, $opacity, $startup, $monitor, $dpi)
                ON CONFLICT(id) DO UPDATE SET
                    pinned = excluded.pinned,
                    left = excluded.left,
                    top = excluded.top,
                    width = excluded.width,
                    height = excluded.height,
                    locked = excluded.locked,
                    opacity = excluded.opacity,
                    startup_enabled = excluded.startup_enabled,
                    monitor_reference = excluded.monitor_reference,
                    dpi_reference = excluded.dpi_reference;
                """;
            cmd.Parameters.AddWithValue("$pinned", settings.MainPinned ? 1 : 0);
            cmd.Parameters.AddWithValue("$left", settings.MainLeft);
            cmd.Parameters.AddWithValue("$top", settings.MainTop);
            cmd.Parameters.AddWithValue("$width", settings.MainWidth);
            cmd.Parameters.AddWithValue("$height", settings.MainHeight);
            cmd.Parameters.AddWithValue("$locked", settings.MainLocked ? 1 : 0);
            cmd.Parameters.AddWithValue("$opacity", settings.MainOpacity);
            cmd.Parameters.AddWithValue("$startup", settings.StartupEnabled ? 1 : 0);
            cmd.Parameters.AddWithValue("$monitor", (object?)settings.MainMonitorReference ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$dpi", (object?)settings.MainDpiReference ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        tx.Commit();
    }
}

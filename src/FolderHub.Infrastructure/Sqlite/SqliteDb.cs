using Microsoft.Data.Sqlite;

namespace FolderHub.Infrastructure.Sqlite;

/// <summary>
/// SQLite 连接工厂与 Schema 迁移。
/// </summary>
public sealed class SqliteDb : IDisposable
{
    private readonly string _connectionString;
    private SqliteConnection? _keptAliveForMemory;

    public SqliteDb(string databasePathOrConnectionString)
    {
        if (databasePathOrConnectionString.Contains('=', StringComparison.Ordinal)
            || databasePathOrConnectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        {
            _connectionString = databasePathOrConnectionString;
        }
        else
        {
            var directory = Path.GetDirectoryName(databasePathOrConnectionString);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            _connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databasePathOrConnectionString,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Default
            }.ToString();
        }
    }

    public static SqliteDb OpenDefault()
        => new(Storage.AppDataPaths.GetDefaultDatabasePath());

    public static SqliteDb OpenInMemory()
    {
        var name = "FolderHub_" + Guid.NewGuid().ToString("N");
        var db = new SqliteDb($"Data Source={name};Mode=Memory;Cache=Shared");
        // Keep a connection open so shared in-memory DB survives between OpenConnection calls.
        db._keptAliveForMemory = db.OpenConnection();
        return db;
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using (var pragma = connection.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL; PRAGMA busy_timeout = 5000;";
            pragma.ExecuteNonQuery();
        }

        return connection;
    }

    public void Migrate()
    {
        using var connection = OpenConnection();
        using var tx = connection.BeginTransaction();

        EnsureSchemaInfo(connection, tx);
        var version = ReadVersion(connection, tx);
        if (version == 0)
        {
            ApplyV1(connection, tx);
            WriteVersion(connection, tx, SqliteSchema.CurrentVersion);
            SeedDefaults(connection, tx);
        }
        else if (version > SqliteSchema.CurrentVersion)
        {
            throw new InvalidOperationException(
                $"数据库版本 {version} 高于程序支持的 {SqliteSchema.CurrentVersion}。");
        }
        else if (version < SqliteSchema.CurrentVersion)
        {
            if (version == 1)
            {
                ApplyV1ToV2(connection, tx);
                WriteVersion(connection, tx, 2);
                version = 2;
            }

            if (version == 2)
            {
                ApplyV2ToV3(connection, tx);
                WriteVersion(connection, tx, 3);
                version = 3;
            }

            if (version != SqliteSchema.CurrentVersion)
            {
                throw new InvalidOperationException(
                    $"缺少从版本 {version} 到 {SqliteSchema.CurrentVersion} 的迁移脚本。");
            }
        }

        tx.Commit();
    }

    public int GetSchemaVersion()
    {
        using var connection = OpenConnection();
        EnsureSchemaInfo(connection, transaction: null);
        return ReadVersion(connection, transaction: null);
    }

    private static void EnsureSchemaInfo(SqliteConnection connection, SqliteTransaction? transaction)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS schema_info (
                id INTEGER PRIMARY KEY CHECK (id = 1),
                version INTEGER NOT NULL
            );
            """;
        cmd.ExecuteNonQuery();
    }

    private static int ReadVersion(SqliteConnection connection, SqliteTransaction? transaction)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "SELECT version FROM schema_info WHERE id = 1;";
        var result = cmd.ExecuteScalar();
        return result is long l ? (int)l : result is int i ? i : 0;
    }

    private static void WriteVersion(SqliteConnection connection, SqliteTransaction transaction, int version)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = """
            INSERT INTO schema_info (id, version) VALUES (1, $v)
            ON CONFLICT(id) DO UPDATE SET version = excluded.version;
            """;
        cmd.Parameters.AddWithValue("$v", version);
        cmd.ExecuteNonQuery();
    }

    private static void ApplyV1(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = SqliteSchema.CreateSql;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyV1ToV2(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = SqliteSchema.MigrateV1ToV2Sql;
        cmd.ExecuteNonQuery();
    }

    private static void ApplyV2ToV3(SqliteConnection connection, SqliteTransaction transaction)
    {
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = SqliteSchema.MigrateV2ToV3Sql;
        cmd.ExecuteNonQuery();
    }

    private static void SeedDefaults(SqliteConnection connection, SqliteTransaction transaction)
    {
        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = """
                INSERT OR IGNORE INTO global_settings (id, keep_cards_after_main_exit)
                VALUES (1, 1);
                """;
            cmd.ExecuteNonQuery();
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = """
                INSERT OR IGNORE INTO main_component_settings
                    (id, pinned, left, top, width, height, locked, opacity, startup_enabled, monitor_reference, dpi_reference, ui_theme)
                VALUES (1, 0, 80, 80, 420, 560, 0, 1.0, 0, NULL, NULL, 'light');
                """;
            cmd.ExecuteNonQuery();
        }

        using (var cmd = connection.CreateCommand())
        {
            cmd.Transaction = transaction;
            cmd.CommandText = """
                INSERT OR IGNORE INTO categories (id, name, sort_order, is_uncategorized)
                VALUES ('00000000-0000-0000-0000-000000000001', '未分类', 0, 1);
                """;
            cmd.ExecuteNonQuery();
        }
    }

    public void Dispose()
    {
        _keptAliveForMemory?.Dispose();
        _keptAliveForMemory = null;
    }
}

namespace FolderHub.Infrastructure.Sqlite;

/// <summary>
/// Schema：Main 权威聚合表 + CardHost 展示状态表（ADR-035/039/040）。
/// CardPresentationState 仅由 CardHost 写入，Main 不得通过业务仓储改写。
/// </summary>
internal static class SqliteSchema
{
    public const int CurrentVersion = 4;

    public const string CreateSql = """
        PRAGMA foreign_keys = ON;

        CREATE TABLE IF NOT EXISTS schema_info (
            id INTEGER PRIMARY KEY CHECK (id = 1),
            version INTEGER NOT NULL
        );

        CREATE TABLE IF NOT EXISTS categories (
            id TEXT PRIMARY KEY NOT NULL,
            name TEXT NOT NULL,
            sort_order INTEGER NOT NULL,
            is_uncategorized INTEGER NOT NULL CHECK (is_uncategorized IN (0, 1)),
            icon TEXT NULL
        );

        CREATE UNIQUE INDEX IF NOT EXISTS ix_categories_name_nocase
            ON categories(name COLLATE NOCASE);

        CREATE TABLE IF NOT EXISTS folders (
            id TEXT PRIMARY KEY NOT NULL,
            path TEXT NOT NULL,
            path_key TEXT NOT NULL,
            display_name TEXT NOT NULL,
            alias TEXT NULL,
            description TEXT NULL,
            is_path_unavailable INTEGER NOT NULL CHECK (is_path_unavailable IN (0, 1)),
            icon TEXT NULL
        );

        CREATE UNIQUE INDEX IF NOT EXISTS ix_folders_path_key
            ON folders(path_key);

        CREATE TABLE IF NOT EXISTS folder_category_links (
            folder_id TEXT NOT NULL,
            category_id TEXT NOT NULL,
            sort_order INTEGER NOT NULL,
            PRIMARY KEY (folder_id, category_id),
            FOREIGN KEY (folder_id) REFERENCES folders(id) ON DELETE CASCADE,
            FOREIGN KEY (category_id) REFERENCES categories(id) ON DELETE CASCADE
        );

        CREATE INDEX IF NOT EXISTS ix_links_category
            ON folder_category_links(category_id, sort_order);

        CREATE TABLE IF NOT EXISTS global_settings (
            id INTEGER PRIMARY KEY CHECK (id = 1),
            keep_cards_after_main_exit INTEGER NOT NULL CHECK (keep_cards_after_main_exit IN (0, 1))
        );

        CREATE TABLE IF NOT EXISTS main_component_settings (
            id INTEGER PRIMARY KEY CHECK (id = 1),
            pinned INTEGER NOT NULL CHECK (pinned IN (0, 1)),
            left REAL NOT NULL,
            top REAL NOT NULL,
            width REAL NOT NULL,
            height REAL NOT NULL,
            locked INTEGER NOT NULL CHECK (locked IN (0, 1)),
            opacity REAL NOT NULL,
            startup_enabled INTEGER NOT NULL CHECK (startup_enabled IN (0, 1)),
            monitor_reference TEXT NULL,
            dpi_reference REAL NULL,
            ui_theme TEXT NOT NULL DEFAULT 'light'
        );

        CREATE TABLE IF NOT EXISTS card_definitions (
            card_id TEXT PRIMARY KEY NOT NULL,
            folder_id TEXT NOT NULL UNIQUE,
            enabled INTEGER NOT NULL CHECK (enabled IN (0, 1)),
            FOREIGN KEY (folder_id) REFERENCES folders(id) ON DELETE CASCADE
        );

        -- CardHost 唯一写入（ADR-035）。
        CREATE TABLE IF NOT EXISTS card_presentation_states (
            card_id TEXT PRIMARY KEY NOT NULL,
            left REAL NOT NULL,
            top REAL NOT NULL,
            locked INTEGER NOT NULL CHECK (locked IN (0, 1)),
            monitor_reference TEXT NULL,
            dpi_reference REAL NULL,
            state_version INTEGER NOT NULL,
            updated_at_utc TEXT NOT NULL,
            FOREIGN KEY (card_id) REFERENCES card_definitions(card_id) ON DELETE CASCADE
        );
        """;

    /// <summary>v1→v2：主组件设置补齐 ADR-040 显示器身份与 DPI 参考。</summary>
    public const string MigrateV1ToV2Sql = """
        ALTER TABLE main_component_settings ADD COLUMN monitor_reference TEXT NULL;
        ALTER TABLE main_component_settings ADD COLUMN dpi_reference REAL NULL;
        """;

    /// <summary>v2→v3：界面主题（light/dark），对齐 design-demo。</summary>
    public const string MigrateV2ToV3Sql = """
        ALTER TABLE main_component_settings ADD COLUMN ui_theme TEXT NOT NULL DEFAULT 'light';
        """;

    /// <summary>v3→v4：分类/文件夹自定义图标（images/ 文件名）。</summary>
    public const string MigrateV3ToV4Sql = """
        ALTER TABLE categories ADD COLUMN icon TEXT NULL;
        ALTER TABLE folders ADD COLUMN icon TEXT NULL;
        UPDATE categories SET icon = 'category-uncategorized.png' WHERE is_uncategorized = 1 AND icon IS NULL;
        """;
}

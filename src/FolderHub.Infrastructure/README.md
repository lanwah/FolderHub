# FolderHub.Infrastructure

基础设施层。

| 能力 | 状态 | 说明 |
|---|---|---|
| SQLite | WP2 | Schema v1、迁移、`FolderCatalog` / Main 设置 / CardDefinition 仓储 |
| Named Pipes | WP6 | 待实现 |
| Shell/Explorer | WP4 | `WindowsShellFolderService`：路径检查 + explorer.exe 打开 |
| 托盘与启动项 | WP5 | 待实现 |

默认库路径：`%LocalAppData%\FolderHub\folderhub.db`。
`card_presentation_states` 表已预建，写入权属 CardHost（ADR-035），Main 业务仓储不写该表。
`SqliteCardDefinitionStore.ReplaceAll` 为差分 upsert：同 `card_id` 更新不删行，避免级联清空展示态；仅真正移除的卡片才 DELETE（连带其 presentation）。

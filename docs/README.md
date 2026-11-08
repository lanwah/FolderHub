# FolderHub 文档索引

本目录统一存放 FolderHub 项目研发过程文档。项目源代码、测试代码、脚本和配置与研发文档分离。项目管理入口 `project.md`、`Task_Board.md`、`plans/done.md` 按 V2.4.2 规范位于项目根目录及 plans。

## 文档目录
| 目录 | 用途 |
|---|---|
| `00_Project_Management/` | 项目章程、阶段状态、决策、变更、风险、澄清记录及一致性检查报告 |
| `01_Requirements/` | 原始需求台账、未决问题台账；尚无正式 SRS |
| `02_Architecture/` | 概要设计；当前未启动 |
| `03_Detailed_Design/` | 详细设计；当前未启动 |
| `04_Reviews/` | 阶段评审记录 |
| `05_Development_Management/` | 开发过程管理 |
| `06_Test_Management/` | 测试管理 |
| `templates/` | 工作流模板副本 |

## 权威管理入口
- 阶段状态：`00_Project_Management/Project_Status.md`
- 原始需求：`01_Requirements/Raw_Requirements.md`
- 未决问题：`01_Requirements/Open_Issues.md`
- 决策：`00_Project_Management/Decision_Log.md`
- 变更：`00_Project_Management/Change_Log.md`
- 风险：`00_Project_Management/Risk_Register.md`
- 澄清索引：`00_Project_Management/问题澄清记录台账.md`
- 澄清过程：`00_Project_Management/Clarifications/README.md`、`CLQ-001.md`～`CLQ-019.md`

## 本轮检查报告
- `00_Project_Management/项目一致性检查报告-管理状态与目录清理V1.9.md`
- `00_Project_Management/项目一致性检查报告-用户确认Q019-020V1.8.md`

## 当前阶段
- P1_REQUIREMENT_COLLECTION / APPROVED
- 需求基线尚未建立；正式 SRS 未启动
- Q-001～Q-014、Q-016～Q-020 已关闭；Q-015 留待 P3
- ADR-001～ADR-026 已确认
- CR-001 的 Windows Server 支持范围已获用户确认；RISK-001 剩余风险已 ACCEPTED；逐版本兼容性验证义务仍保留
- TASK-001～TASK-010 已完成；任务完成不等于阶段批准

## 当前 Windows 支持范围摘要
- Windows 10（22H2）/Windows 11（x64）本地交互式桌面
- Windows Server 2012 及以上版本的单用户交互式桌面
- Server 2012/2012 R2：Server with a GUI；Server 2016 及以上：Desktop Experience
- 不支持 Server Core；不承诺 RDS 多用户
- 以上是目标支持范围，兼容性仍需逐版本验证

## 目录与文件归档说明
- 根目录项目管理入口为 `project.md`、`Task_Board.md`；根目录 `README.md` 和 `.gitignore` 按项目结构保留；完成任务归档位于 `plans/done.md`。
- 正式研发文档、台账、澄清记录、评审报告和检查报告统一放在 `docs/` 对应阶段目录。
- 2026-11-08 目录复核：用户已清理此前两份 RAW 历史同步副本；当前项目根目录不保留 `FH_SYNC_*` 同步副本。权威文件仅以本索引“权威管理入口”列出的路径为准。
- `05_Development_Management/`、`06_Test_Management/` 为本项目既有目录命名，继续沿用；不因技能默认新项目目录名称不同而重命名，以免破坏已有路径和历史引用。

## 管理规则
1. 未经用户明确批准，不进入下一研发阶段。
2. 不将助手建议当作用户需求或决策。
3. 已确认需求和批准基线不得静默修改；变更必须登记并进行影响分析。
4. 每次实质工作维护需求、问题、决策、变更、风险台账和阶段状态。
5. 根目录 `README.md` 为用户维护文件，必须保留；本轮未修改。

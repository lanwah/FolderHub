# FolderHub 文档索引

本目录统一存放 FolderHub 项目的研发过程文档。项目源代码、测试代码、脚本和配置与研发文档分离。项目管理入口 `project.md`、`Task_Board.md`、`plans/done.md` 位于项目根目录/ plans，是 V2.4 管理记录例外；正式研发文档仍统一归档于 `docs/`。

## 文档目录
| 目录 | 用途 | 当前内容 |
|---|---|---|
| `00_Project_Management/` | 项目章程、项目状态、决策、变更、建议、澄清及风险管理 | `Project_Charter.md`、`Project_Status.md`、`Decision_Log.md`、`Change_Log.md`、`Assistant_Suggestions.md`、`Directory_Initialization_Record.md`、`问题澄清记录台账.md`、`Clarifications/`、`项目一致性检查报告-澄清历史恢复V1.0.md`、`项目一致性检查报告-澄清历史追加恢复V1.1.md`、`项目一致性检查报告-用户选择登记V1.2.md`、`项目一致性检查报告-用户补充确认V1.3.md、项目一致性检查报告-用户确认RAW003-006及Q010-V1.4.md、项目一致性检查报告-用户确认Q010细节V1.5.md`、`项目一致性检查报告-RAW001-002完整性检查V1.6.md`、`项目一致性检查报告-用户确认Q016-018V1.7.md`、`Risk_Register.md` |
| `01_Requirements/` | 原始需求、问题、正式需求与追踪 | `Raw_Requirements.md`、`Open_Issues.md`；尚无正式 SRS |
| `02_Architecture/` | 概要架构与技术方案 | 当前尚无正式设计文档 |
| `03_Detailed_Design/` | 详细设计 | 当前尚无正式设计文档 |
| `04_Reviews/` | 阶段评审记录 | 当前尚无正式评审记录 |
| `05_Development_Management/` | 开发过程管理 | 按阶段启用 |
| `06_Test_Management/` | 测试过程管理 | 按阶段启用 |
| `templates/` | 本项目使用的工作流模板副本 | 保留既有模板文件；本次未覆盖或替换历史模板 |
| `_Archive/` | 历史或已替代文件 | 如存在，按原状保留 |

## V2.4 项目管理入口
- 项目摘要看板：项目根目录 `project.md`（只读汇总）。
- 任务台账：项目根目录 `Task_Board.md`（TASK 唯一权威源）。
- 已完成任务归档：项目根目录 `plans/done.md`（追加式）。
- 阶段状态权威源：`00_Project_Management/Project_Status.md`。
- 风险权威源：`00_Project_Management/Risk_Register.md`。
- 问题澄清索引：`00_Project_Management/问题澄清记录台账.md`。
- 澄清逐题过程：`00_Project_Management/Clarifications/README.md` 及 `CLQ-001.md`～`CLQ-018.md`（CLQ-015～017 已确认；CLQ-018 记录 Windows Server 版本待确认）。

## 当前研发阶段
- 当前阶段：`P1_REQUIREMENT_COLLECTION`
- 阶段状态：`IN_PROGRESS`
- 需求基线：尚未建立
- 正式需求分析：未启动
- 概要设计、详细设计：未启动

## 管理规则
1. 所有正式研发文档统一放在项目根目录 `docs/` 下；V2.4 管理入口文件按技能规范放在根目录及 `plans/`。
2. 不将助手建议当作用户确认的需求或决策。
3. 未经用户明确批准，不进入下一研发阶段。
4. 已确认需求和批准基线不得静默修改；变更必须登记并进行影响分析。
5. 每次实质工作维护需求、问题、决策、变更、风险台账和项目阶段状态。
6. 项目阶段以 `Project_Status.md` 为准；任务状态以根目录 `Task_Board.md` 为准。

## V1.1 澄清历史追加恢复
- TASK-003 已完成；V1.0 原始记录保留，CLQ-001～CLQ-009 已追加历史摘要、新澄清选项/推荐及 V1.2 用户回答。
- Q-001～Q-009 均已关闭；Q-010 保持 OPEN，留待设计阶段技术边界澄清。
- P1 仍为 IN_PROGRESS；需求基线未建立，未启动 P2。


## V1.3 用户补充确认
- TASK-005 已完成；用户接受 Q-003/005/006/009 的推荐附加方案。
- Q-001～Q-009 均已关闭，Q-010 保持 OPEN；P1 仍为 IN_PROGRESS，需求基线未建立，未启动 P2。


## V1.4 用户确认登记（2026-11-02）
- TASK-006 已完成；RAW-003～RAW-006 的推荐补充规则均获用户确认，新增 Q-011～Q-014、CLQ-011～CLQ-014 和 ADR-017～ADR-020。
- Q-010 用户选择方案 C（退出时是否保留独立卡片可配置）；默认值及设置作用范围待确认，技术实现留待设计阶段。
- P1 仍为 IN_PROGRESS；需求基线未建立，未启动 P2。

## V1.5 Q-010 细节确认（2026-11-02）
- TASK-007 已完成；Q-010 全局统一设置、默认开启，已关闭并新增 ADR-021。
- Q-015 跟踪独立卡片在主程序退出后的技术生命周期实现，留待 P3 设计评估。
- P1 仍为 IN_PROGRESS；需求基线尚未建立，未启动 P2。


## V1.6 RAW-001 / RAW-002 完整性检查（2026-11-02）
- 新增 Q-016～Q-018、CLQ-015～CLQ-017，分别澄清首版目标运行环境、分类层级和多分类文件夹排序范围。

## V1.7 用户确认 Q-016～Q-018（2026-11-04）
- Q-016～Q-018 已关闭；首版支持 Windows Server 单用户交互式桌面（不含 RDS 多用户），分类采用平级列表，各分类独立维护文件夹排序。
- 新增 ADR-022～ADR-024；CR-001 记录并确认 Windows Server 单用户支持范围扩展；新增 RISK-001 跟踪兼容性验证。
- 新增 Q-019/CLQ-018，等待用户确认具体 Windows Server 版本和 Desktop Experience 安装选项。
- TASK-008 继续 IN_PROGRESS；P1 仍为 IN_PROGRESS，需求基线未建立，未进入 P2。



## 2026-11-05 用户确认
- Q-019 / CLQ-018：用户自定义选择 D，Windows Server 2012 及以上版本，CLOSED。
- Q-020 / CLQ-019：用户选择 A；Server 2012/2012 R2 为 Server with a GUI，2016 及以上为 Desktop Experience，不支持 Server Core，CLOSED。
- RISK-001 继续 OPEN，兼容性验证待执行；P1 未结束，需求基线未建立。


## 2026-11-05 用户确认
- Q-019 / CLQ-018：用户自定义选择 D，Windows Server 2012 及以上版本，CLOSED。
- Q-020 / CLQ-019：用户选择 A；Server 2012/2012 R2 为 Server with a GUI，2016 及以上为 Desktop Experience，不支持 Server Core，CLOSED。
- RISK-001 继续 OPEN，兼容性验证待执行；P1 未结束，需求基线未建立。

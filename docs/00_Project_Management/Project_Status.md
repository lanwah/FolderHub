# FolderHub 项目状态

- 文档编号：FH-PM-002
- 版本：v3.1
- 状态：项目管理状态记录
- 日期：2026-11-14
- 工作流规范：software-predev-workflow-skill V2.5.0

## 0. 工作流规范更新记录
- 2026-11-10：依据用户明确选择 A，将 FolderHub 项目管理规范对齐至 `software-predev-workflow-skill V2.5.0`。完成 RISK-001 三层风险管理字段适配及初步筛查；保留其既有完整评估、`ACCEPTED` 状态和用户接受证据，不改变 RAW/Q/ADR/CR、阶段批准或任何需求/设计基线。
- 2026-11-08：项目执行规范版本标识更新为 `software-predev-workflow-skill V2.4.2`。本次版本差异仅涉及技能包 `.gitignore` 对 `.cursor/` 的跟踪修正及移除技能包内冗余提交规则文件；不改变 FolderHub 项目需求、阶段状态、评审批准或任何基线。

## 1. 当前状态
| 项目 | 状态 |
|---|---|
| 项目初始化 P0 | 已完成（V2.4 接管初始化已完成） |
| 需求采集 P1 | APPROVED |
| 需求分析 P2 | IN_PROGRESS |
| 概要设计 P3 | NOT_STARTED |
| 详细设计 P4 | NOT_STARTED |
| 需求基线 | 尚未建立 |
| 概要设计基线 | 尚未建立 |
| 详细设计基线 | 尚未建立 |

## 2. 当前台账统计
- 原始需求：13 项（RAW-001 至 RAW-013）
- 已确认需求/产品选择：13 项（RAW-001 至 RAW-013；RAW-001/RAW-002 的关键范围已确认，Server 兼容性待验证）
- 待澄清/细化的原始需求：0 项（RAW-001/RAW-002 关键范围已确认；兼容性验证作为风险跟踪）
- 助手建议：8 项（SUG-001 至 SUG-008，非用户需求）
- 未决问题：2 项（Q-015 为 P3 技术设计问题；Q-026 为 P2 多显示器位置恢复澄清）；Q-001～Q-014、Q-016～Q-020、Q-022～Q-025、Q-029～Q-151 均已关闭（Q-029～Q-150 为历史摘要重建记录）
- 决策记录：32 项（ADR-001 至 ADR-032）；ADR-032 汇总 Q-029～Q-150 的 122 项历史确认结论
- 澄清记录：147 项（CLQ-001～CLQ-146、CLQ-151；CLQ-025～CLQ-146 为 RECONSTRUCTED）
- 澄清历史：CLQ-001～CLQ-146、CLQ-151 已登记；Q-001～Q-014、Q-016～Q-020、Q-022～Q-025、Q-029～Q-151 已关闭；Q-015 为 P3 技术设计问题，Q-026 为 P2 待确认。Q-029～Q-150 结论已按历史摘要补写，原始逐字问答缺失边界已逐题标记 RECONSTRUCTED/UNAVAILABLE
- 变更记录：1 项（CR-001 范围已获用户确认；兼容性验证待执行；需求基线尚未建立）
- 任务看板：TASK-001～TASK-010 已完成并归档；TASK-011/012 IN_PROGRESS；TASK-013 TODO

## 3. 当前重点
1. Q-001～Q-009、Q-011～Q-014 均已关闭；RAW-003～RAW-006 的别名、用途说明、路径失效和打开操作规则已确认并同步 RAW/ADR。
2. Q-010 已关闭：退出保留设置为全局统一开关，默认开启；开启时退出主程序后独立卡片继续显示，关闭时一并关闭。具体生命周期技术实现由 Q-015 跟踪，留待 P3。
3. Q-016～Q-020 已全部关闭：保留 Windows 10/11 本地桌面支持，增加 Windows Server 2012 及以上单用户 GUI/Desktop Experience 支持；不含 Server Core/RDS 多用户；分类平级且各分类独立排序。
4. 同类项目调研范围已确认（5～10 个候选及约定交付物）；具体技术栈尚未批准，调研结论不得直接视为技术选型。

## 4. 下一步
P2 需求分析阶段已于 2026-11-10 获用户明确启动授权，当前 IN_PROGRESS。已建立 SRS v0.1～v0.7 草案（v0.7 当前；v0.1～v0.6 历史版本保留）；Q-022～Q-025、Q-151 已关闭，Q-026 为当前 P2 待确认问题。待继续处理 P2 其他需求缺口、执行需求质量检查并形成阶段评审包。未经用户明确批准 SRS 指定版本，不建立需求基线、不启动 P3。Q-015 留待 P3 设计阶段处理。

## 5. 阶段与基线历史
- 2026-10-31：P0 初始化完成，进入 P1 需求采集；需求基线尚未建立。
- 2026-11-01：按用户批准执行 software-predev-workflow-skill V2.4 已有项目接管初始化；未修改需求/设计基线。
- 2026-11-01：用户批准执行澄清历史完整恢复 V1.0；恢复 10 个 CLQ 逐题记录，按证据等级标记历史缺失；未修改 RAW/ADR 已有内容，未关闭 Q，不改变 P1 阶段状态。
- 2026-11-01：用户要求执行 CLQ-001～CLQ-009 V1.1 追加恢复；已追加历史摘要、证据等级和新澄清选项/推荐。
- 2026-11-01：用户回答 `q1A q2B q3A q4A q5A q6A q7A q8C q9A`；更新 CLQ-001～009、RAW/Q/ADR 台账及项目状态。Q-001/002/004/007/008 关闭；Q-003/005/006/009 部分确认；P1 保持 IN_PROGRESS，未建立基线，未进入 P2。
- 2026-11-01：用户回答 `Q3推荐 Q5推荐 Q6推荐 Q9推荐`，明确接受四题推荐附加方案；Q-003/005/006/009 全部关闭，ADR-010/012/013/016 更新为 CONFIRMED；Q-010 仍 OPEN。P1 继续 IN_PROGRESS，未建立需求基线，未进入 P2。
- 2026-11-02：用户确认 RAW-003～RAW-006 推荐方案，新增 Q-011～Q-014、ADR-017～ADR-020 并关闭四题；用户选择 Q-010 方案 C，设置默认值及作用范围待确认。TASK-006 完成；P1 保持 IN_PROGRESS，未建立需求基线，未进入 P2。

## 6. 任务与风险
- 当前活跃任务：TASK-011（P2 SRS 需求整理，IN_PROGRESS）；TASK-012（P2 需求缺口澄清，IN_PROGRESS）；TASK-013（P2 需求质量检查与阶段评审包，TODO）。TASK-001～TASK-010 已完成并归档。
- 已完成任务：TASK-001～TASK-010；TASK-008 为 RAW-001/RAW-002 完整性澄清及 Q-016～Q-020 关闭与台账同步；TASK-009 为 P1 阶段评审批准及管理文件同步。
- 当前风险：RISK-001（Windows Server 2012+ GUI/Desktop Experience 兼容性尚未逐版本验证；V2.5.0 初筛为 NORMAL；既有完整评估及用户接受状态 ACCEPTED 保持有效；P3/P4 仍须逐版本验证）；Q-015 独立卡片生命周期技术实现留待 P3；Q-001～Q-009 部分原始逐轮历史仍不可访问。
- Git 仓库/Commit/Tag：当前环境不可验证，NOT_CHECKED。

- P1 阶段评审：用户已批准 P1 成果；需求基线尚未建立；P2 启动授权待用户单独确认。

## 7. 修订记录
- v1.9（2026-11-08）：修正项目管理文件之间的阶段状态冲突：Task_Board、project.md、docs/README 与本文件统一为 P1_REQUIREMENT_COLLECTION / APPROVED；补齐 TASK-009 状态历史及完成归档。P2 仍 NOT_STARTED，需求基线仍未建立。核查发现根目录 FH_SYNC_Raw_Requirements.md 为过期同步副本，权威文件为 docs/01_Requirements/Raw_Requirements.md；将该同步副本标记为非权威；当前项目资料库接口不允许删除项目附件，故未删除该文件；不修改权威需求内容。文档目录说明补充于 docs/README。

- v0.4（2026-11-01）：记录澄清历史完整恢复 V1.0 的执行状态、证据边界及 TASK-002；P1 保持 IN_PROGRESS，未建立任何基线。
- v0.5（2026-11-01）：完成 TASK-002；10 个 CLQ 文件、索引和关联管理文件写入并复核；P1 保持 IN_PROGRESS，Q 均未关闭，未建立任何基线。
- v0.6（2026-11-01）：完成 CLQ-001～CLQ-009 V1.1 追加恢复及新澄清轮次。
- v0.7（2026-11-01）：登记用户对 Q-001～Q-009 的选择；关闭 Q-001/002/004/007/008，Q-003/005/006/009 部分确认；更新 RAW/Q/ADR/CLQ 和状态摘要；P1 保持 IN_PROGRESS，未建立基线。
- v0.8（2026-11-01）：修正 CLQ-007 与 Open_Issues 中 Q-007/Q-008 状态字段，使其与用户明确选择及澄清索引一致。
- v0.9（2026-11-01）：校正决策统计，区分 CONFIRMED 与 PARTIALLY_CONFIRMED，并进一步规范 CLQ 用户原文摘录标记。
- v1.0（2026-11-01）：TASK-004 完成并归档；更新任务看板、完成归档和项目状态。
- v1.1（2026-11-01）：用户接受 Q-003/005/006/009 推荐附加方案；四题关闭，16 项 ADR 全部 CONFIRMED；仅 Q-010 仍 OPEN；TASK-005 完成并归档。

- v1.2（2026-11-02）：登记 RAW-003～RAW-006 用户确认及 Q-010 方案 C 选择；新增 Q-011～Q-014、ADR-017～ADR-020、CLQ-011～CLQ-014；TASK-006 完成；P1 保持 IN_PROGRESS。
- v1.3（2026-11-02）：确认 Q-010 全局统一设置、默认开启；新增 ADR-021，关闭 Q-010；将技术实现边界拆分为 P3 问题 Q-015；TASK-007 完成；P1 保持 IN_PROGRESS。
- v1.4（2026-11-02）：完成 RAW-001/RAW-002 完整性检查；新增 Q-016～Q-018、CLQ-015～CLQ-017；TASK-008 IN_PROGRESS，等待用户回答；P1 保持 IN_PROGRESS。

- 2026-11-02：用户确认 Q-010 设置作用范围 A（全局统一）及默认值 A（默认开启）；新增 ADR-021，关闭 Q-010；将未确定的技术生命周期实现边界拆分为 P3 问题 Q-015。TASK-007 完成并归档；P1 保持 IN_PROGRESS，未建立需求基线，未进入 P2。

- 2026-11-02：完成 RAW-001/RAW-002 完整性检查，新增 Q-016～Q-018 与 CLQ-015～CLQ-017；TASK-008 IN_PROGRESS。P1 保持 IN_PROGRESS，未建立需求基线，未进入 P2。


- 2026-11-05：用户确认 Q-019 为 Windows Server 2012 及以上版本，Q-020 选择 A（Server 2012/2012 R2 Server with a GUI；2016 及以上 Desktop Experience；不支持 Server Core）。新增 ADR-025/ADR-026，更新 CR-001、RISK-001、RAW/Q/CLQ 及项目摘要；TASK-008 完成并归档。P1 仍 IN_PROGRESS，需求基线未建立，未进入 P2。
- 2026-11-06：用户明确接受 RISK-001 剩余风险，状态更新为 ACCEPTED；用户随后明确批准 P1 需求采集阶段成果（原文：`问题2：A`）。P1 状态更新为 APPROVED；该批准不建立需求基线、不批准 P2，也不授权启动 P2；P2 仍 NOT_STARTED，等待用户单独授权。
\n- v1.7（2026-11-06）：用户明确批准 P1 需求采集阶段成果；P1 状态设为 APPROVED，P2 保持 NOT_STARTED；需求基线仍未建立；RISK-001 为 ACCEPTED 且验证义务保留；等待用户单独授权启动 P2。\n

## 8. 目录规范复核记录
- 2026-11-08：用户确认已清理两份 RAW 历史同步副本，并要求按 software-predev-workflow-skill V2.4.2 调整文件位置。复核结果：项目根目录仅保留 `project.md`、`Task_Board.md`、`README.md`、`.gitignore`；`plans/done.md` 位于规定位置；正式研发文档及管理台账均位于 `docs/` 对应目录。无需要迁移的文件。
- 现有项目目录 `docs/05_Development_Management/`、`docs/06_Test_Management/` 依据既有项目约定继续保留，不为匹配新项目默认目录名而重命名，避免破坏历史路径引用。
- 用户已完成历史同步副本清理；本轮不再将 FH_SYNC 副本列为当前遗留项。需求、问题、决策和基线均未修改；P1 保持 APPROVED，P2 保持 NOT_STARTED。
- v2.0（2026-11-08）：完成目录规范复核，更新 docs 索引、任务看板及完成归档；未发生文件迁移，未修改需求或设计基线。


## 9. V2.5.0 适配与 P1 复核记录
- 2026-11-10：用户明确选择方案 A，授权将 FolderHub 项目工作流规范对齐至 V2.5.0，并完成风险适配和 P1 一致性复核。
- 已完成：核对 V2.5.0 `SKILL.md` 与风险三层管理、质量检查、一致性检查规则；为 RISK-001 追加初步筛查记录；更新本状态文件、项目摘要及 docs 索引的规范版本标识；核对 RAW-001～RAW-013、Q-001～Q-020、ADR-001～ADR-026、CR-001、TASK-001～TASK-010 的既有记录。
- 未修改：任何 RAW 原文/状态、Q 状态、ADR 决策内容、CR-001 范围、P1 批准证据；未新增或建立需求基线；未启动 P2；未生成正式 SRS。
- 风险边界：RISK-001 仍为 ACCEPTED，新增 V2.5.0 初筛紧迫程度 NORMAL；既有概率中/影响高/风险等级高的完整评估及用户接受证据保留。Windows Server 兼容性仍未逐版本验证。
- 阶段状态：P1_REQUIREMENT_COLLECTION / APPROVED；P2_REQUIREMENT_ANALYSIS / NOT_STARTED；需求基线 NOT_ESTABLISHED。
- Git/本地运行环境：NOT_CHECKED；本次仅检查并更新个人 Library 中的 FolderHub 项目管理资料。


## 10. P2 需求分析阶段启动记录
- 2026-11-10：用户明确指示“启动P2”，授权启动 P2_REQUIREMENT_ANALYSIS。
- 阶段状态：P1_REQUIREMENT_COLLECTION / APPROVED；P2_REQUIREMENT_ANALYSIS / IN_PROGRESS；P3/P4 / NOT_STARTED。
- 本轮产物：`docs/01_Requirements/SRS_FolderHub_v0.1.md`（需求规格说明书草案）；`docs/01_Requirements/P2_需求分析工作记录_v0.1.md`；新增 Q-022/CLQ-020 跟踪主组件首次安装时固定到桌面的默认值。
- 本轮工作范围：按 V2.5.0 对 RAW-001～RAW-013、ADR-001～ADR-026、Q-001～Q-020、CR-001、RISK-001 进行需求分解、范围归纳和追踪；未修改既有 RAW/ADR/CR。
- SRS 状态：v0.1 / DRAFT / 待用户评审；需求优先级和少量交互边界仍待评审/澄清。
- 需求基线：NOT_ESTABLISHED。用户尚未批准 SRS；不得建立 `REQ-BL`。
- 下一阶段：P3 不得启动，须等待用户明确批准 SRS 版本并完成需求基线门禁。

- 2026-11-10：用户明确指示“启动P2”，P2_REQUIREMENT_ANALYSIS 由 NOT_STARTED 转为 IN_PROGRESS；建立 SRS v0.1 草案、P2 工作记录及 Q-022/CLQ-020；需求基线仍未建立，P3/P4 未启动。


## 11. Q-022 用户确认更新记录（2026-11-11）
- 用户明确选择 Q-022 方案 A：首次安装时主组件固定到桌面默认关闭；用户可主动开启。
- 已关闭 Q-022 / CLQ-020；新增 ADR-027；SRS 从 v0.1 修订为 v0.2 草案，v0.1 历史版本保留。
- RAW-011 原始内容未修改；ADR-005、ADR-012、ADR-013 原决策未修改，仅新增补充决策 ADR-027。
- 当前台账：RAW-001～013 CONFIRMED；Q-015 OPEN（P3）；Q-022 CLOSED；ADR-001～027 CONFIRMED；CR-001 不变；RISK-001 ACCEPTED。
- 阶段状态：P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。


## 12. P2 后续需求缺口 Q-023（2026-11-11）
- 根据 SRS 数据/异常边界复核，新增 Q-023 / CLQ-021：同一实际文件夹路径再次添加时的处理规则。
- 推荐 A：同一路径只保留一条 FolderHub 记录；再次添加时提示将已有记录关联到目标分类。
- 当前 Q-023：OPEN / PENDING_CONFIRMATION；尚未新增 ADR，未将推荐写入正式需求。
- SRS v0.3 仅登记未决问题并清理已解决的 Q-022 旧提示；FR 需求基线与已确认 RAW/ADR 均未改变。
- P2 继续 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。


## 13. P2 工作进展同步（2026-11-11）
- 当前 SRS：v0.4 / DRAFT；v0.1～v0.3 历史版本保留；需求基线仍 NOT_ESTABLISHED。
- Q-022 / CLQ-020：CLOSED；ADR-027/ADR-028 CONFIRMED。
- 新增 Q-023 / CLQ-021：CLOSED / CONFIRMED；用户选择 A，新增 ADR-028。
- TASK-011/012：IN_PROGRESS；TASK-013：TODO。
- 下一步唯一行动：收集 Q-026 用户选择，确定多显示器/DPI/分辨率变化后的位置恢复行为；之后继续剩余 P2 缺口和质量检查。
- P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。


## 14. Q-023 用户确认与 Q-024 后续澄清（2026-11-12）
- 用户原始回答（VERBATIM）：`FolderHub Q-023 我的选择：A。请按 P2 流程更新相关台账和 SRS，保留历史记录，并继续后续需求分析。`
- Q-023 / CLQ-021：用户选择 A，已 CLOSED / CONFIRMED；同一路径只保留一条记录，重复添加时复用并关联目标分类，已关联则提示已存在。
- 新增 ADR-028；SRS 从 v0.3 修订为 v0.4 草案，新增 FR-018；路径比较算法留待 P3。
- 新增 Q-024 / CLQ-022：搜索匹配、大小写、触发方式及无结果反馈尚待用户确认。
- RAW-001～RAW-013 未修改；ADR-001～ADR-027 未修改；仅新增 ADR-028。
- 阶段：P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。


## 15. Q-023 用户确认与 Q-024 后续澄清（2026-11-12）
- 用户原始回答（VERBATIM）：`FolderHub Q-023 我的选择：A。请按 P2 流程更新相关台账和 SRS，保留历史记录，并继续后续需求分析。`
- Q-023 / CLQ-021：用户选择 A，CLOSED / CONFIRMED；同一路径只保留一条 FolderHub 记录，重复添加时复用并关联目标分类，已关联则提示已存在。
- 新增 ADR-028；SRS v0.4 草案新增 FR-018；路径比较/规范化算法留待 P3。
- 新增 Q-024 / CLQ-022：搜索匹配方式、大小写、触发方式和无结果反馈待用户确认。
- RAW-001～RAW-013 未修改；ADR-001～ADR-027 未修改；仅新增 ADR-028。
- P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。


## 16. Q-024 用户确认与 Q-025 后续澄清（2026-11-12）
- 用户原始回答（VERBATIM）：`FolderHub Q-024 我的选择：A。请按 P2 流程更新相关台账和 SRS，保留历史记录，并继续后续需求分析。`
- Q-024 / CLQ-022：用户选择 A，已 CLOSED / CONFIRMED；不区分大小写包含匹配、输入动态更新、清空恢复当前分类列表、无结果提示。
- 新增 ADR-029；SRS 从 v0.4 修订为 v0.5 草案，细化 FR-007。
- 搜索范围并未由 Q-024 的选择明确；新增 Q-025 / CLQ-023，当前 OPEN / PENDING_CONFIRMATION。
- RAW-001～RAW-013 未修改；ADR-001～ADR-028 未修改，仅新增 ADR-029。
- 阶段：P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。


## 17. Q-025 用户确认与 Q-026 后续澄清（2026-11-12）
- 用户原始回答（VERBATIM）：`Q-025：A`
- Q-025 / CLQ-023：用户选择 A，已 CLOSED / CONFIRMED；搜索覆盖全部已登记文件夹记录，同一记录只显示一次并展示所属分类，清空关键词恢复当前选中分类列表。
- 新增 ADR-030；SRS 从 v0.5 修订为 v0.6 草案，细化 FR-007。
- 新增 Q-026 / CLQ-024：多显示器、DPI 或分辨率变化后的桌面组件位置恢复，OPEN / PENDING_CONFIRMATION。
- RAW-001～RAW-013 未修改；ADR-001～ADR-029 未修改，仅新增 ADR-030。
- 阶段：P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。

## 18. Q-151 用户确认及资料同步（2026-11-12）

- 用户原始回答（VERBATIM）：`Q-151：D（其他）：A。先提问到此先回写到资料库。请记录确认并继续后续需求澄清。`
- 按用户明确填写的 A 记录为选项 A；Q-151 / CLQ-151 CLOSED / CONFIRMED。
- 确认内容：单条失败保留待清理状态及失败原因，跳过并继续其他记录；中断任务优先续行其剩余记录，完成后再处理其他符合条件记录；失败项后续按既定自动重试规则处理。重试间隔/最大次数未由本题确认。
- 新增 ADR-031；SRS 草案补充为 v0.7（DRAFT）；新增 CLQ-151 并更新问题/决策台账及 P2 工作记录。
- 本轮按用户要求暂停后续提问，先进行资料同步。未修改 RAW 原文、ADR-001～ADR-031 或任何已批准基线。
- 阶段状态不变：P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED；设计基线未建立。
- TASK-011/012 继续 IN_PROGRESS；TASK-013 保持 TODO。Git/Commit/Tag 未检查，NOT_CHECKED。


## 19. 2026-11-14 Q-029～Q-150 历史确认结论同步
- Q-029～Q-150 共 122 项历史确认结论已补入 Open_Issues、问题澄清记录台账、ADR-032、CLQ-025～CLQ-146 及 SRS v0.8。
- 证据等级为 RECONSTRUCTED；未恢复到的原始问题全文、选项和用户逐字回答没有伪造，逐题标记 UNAVAILABLE。
- `docs/README.md` 当前阶段摘要已与本文件对齐。
- 当前阶段：P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。
- Q-015 仍为 P3 OPEN；Q-026 仍为 P2 OPEN / PENDING_CONFIRMATION。

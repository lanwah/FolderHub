# Task Board

- 项目名称：FolderHub
- 台账版本：2.2
- 最后更新：2026-11-08
- 当前阶段（引用 Project_Status）：P1_REQUIREMENT_COLLECTION / APPROVED
- 活跃任务数：0
- 阻塞任务数：0
- 编号规则：TASK-001 起，全局递增，永不复用

## 1. 使用规则
- 仅登记有明确执行目标和可验证完成条件的工作。
- 状态仅为 TODO / IN_PROGRESS / BLOCKED / DONE / CANCELLED / DEFERRED。
- DONE 必须有证据并追加至 plans/done.md；任务完成不等于阶段批准。
- 类型必须使用基础枚举：REQUIREMENTS / DESIGN / DEVELOPMENT / TESTING / DOCUMENTATION / PROJECT_MANAGEMENT / RISK_MITIGATION / RELEASE / OTHER。
- 项目扩展类型使用 CUSTOM:<名称>，须先登记在本文件第 6 节。

## 2. 活跃任务
| TASK_ID | 任务名称/目标 | 主阶段 | 类型 | 优先级 | 状态 | 完成条件 | 关联 RAW/Q/ADR/CR/RISK/AC | 负责人 | 阻塞原因 | 完成证据 | Git Commit | Git Tag | 创建日期 | 最后更新 |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|


## 3. 任务状态历史（追加式）
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-01 | TASK-001 | — | TODO | 按用户批准启动 V2.4 已有项目接管初始化 | 用户明确批准全部实施 |
| 2026-11-01 | TASK-001 | TODO | DONE | V2.4 已有项目接管初始化任务完成；验收项见完成归档 | `/FolderHub/project.md`、`/FolderHub/Task_Board.md`、`/FolderHub/plans/done.md` 及初始化记录 |
| 2026-11-01 | TASK-002 | — | TODO | 用户批准执行“澄清历史完整恢复 V1.0” | 本轮用户明确回复“批准” |
| 2026-11-01 | TASK-002 | TODO | IN_PROGRESS | 已完成历史资料检索并开始建立逐题澄清记录 | 当前对话可访问历史记录、RAW/Q/ADR/澄清索引；逐题文件随后写入资料库 |
| 2026-11-01 | TASK-002 | IN_PROGRESS | DONE | 10 个 CLQ 独立记录、澄清索引、项目状态、项目摘要和文档索引已写入；只读一致性检查完成，未关闭 Q 或推进阶段 | `/FolderHub/docs/00_Project_Management/Clarifications/`、`问题澄清记录台账.md`、`Project_Status.md`、`/FolderHub/project.md`、`/FolderHub/docs/README.md`、`项目一致性检查报告-澄清历史恢复V1.0.md` |
| 2026-11-01 | TASK-003 | — | TODO | 用户要求对 CLQ-001～CLQ-009 执行 V1.1 追加恢复，并重新提出未决选项及推荐 | 用户明确要求“按照以上检索结果…做一次 V1.1 追加恢复” |
| 2026-11-01 | TASK-003 | TODO | IN_PROGRESS | 已核验 V2.4 工作流规范、V1.0 澄清记录及可访问历史摘要；开始追加 V1.1 内容 | `/FolderHub/docs/00_Project_Management/Clarifications/`、历史会话摘要 |
| 2026-11-01 | TASK-003 | IN_PROGRESS | DONE | CLQ-001～CLQ-009 已追加历史恢复和新澄清选项/推荐；索引及 README 已更新；文件已写入资料库并完成核验。未修改 RAW/ADR，未关闭 Q，未推进阶段 | `/FolderHub/docs/00_Project_Management/Clarifications/CLQ-001.md`～`CLQ-009.md`、`README.md`、`问题澄清记录台账.md`、一致性检查报告 V1.1 |

| 2026-11-01 | TASK-004 | — | TODO | 用户提交 Q-001～Q-009 选项答案，需要按 V2.4 追加澄清记录并同步专业台账 | 用户原文：`q1A q2B q3A q4A q5A q6A q7A q8C q9A` |
| 2026-11-01 | TASK-004 | TODO | IN_PROGRESS | 开始逐题登记用户选择，区分完整确认与未回答附加项 | CLQ-001～CLQ-009 |
| 2026-11-01 | TASK-004 | IN_PROGRESS | DONE | 9 个 CLQ、RAW/Q/ADR、问题澄清索引、项目状态、项目摘要和文档索引已更新；Q-001/002/004/007/008 关闭，Q-003/005/006/009 部分确认；完成一致性检查并读回核验 | `/FolderHub/docs/00_Project_Management/Clarifications/CLQ-001.md`～`CLQ-009.md`、`Raw_Requirements.md`、`Open_Issues.md`、`Decision_Log.md`、`问题澄清记录台账.md`、`Project_Status.md`、`project.md`、`docs/README.md`、`项目一致性检查报告-用户选择登记V1.2.md` |
| 2026-11-01 | TASK-005 | — | TODO | 用户接受 Q-003/005/006/009 的推荐附加方案，需要追加澄清记录并同步权威台账 | 用户原文：`Q3推荐 Q5推荐 Q6推荐 Q9推荐` |
| 2026-11-01 | TASK-005 | TODO | IN_PROGRESS | 开始追加四项推荐方案的用户确认并同步 RAW/Q/ADR/CLQ 和项目状态 | CLQ-003/005/006/009 |
| 2026-11-01 | TASK-005 | IN_PROGRESS | DONE | 四个 Q 全部关闭；ADR-010/012/013/016 更新为 CONFIRMED；RAW、问题/决策/澄清台账、项目状态和摘要已更新；一致性检查完成并读回核验 | CLQ-003/005/006/009、Raw_Requirements、Open_Issues、Decision_Log、问题澄清记录台账、Project_Status、project.md、docs/README、检查报告 V1.3 |

| 2026-11-02 | TASK-006 | — | TODO | 用户确认 RAW-003～RAW-006 推荐补充规则，并选择 Q-010 方案 C；需同步澄清、需求、问题、决策及项目管理记录 | 用户原文：`RAW-003：A，接受推荐补充规则；RAW-004：A，接受推荐的补充规则；RAW-005：A，接受推荐的补充规则；RAW-006：A，接受推荐的补充规则；Q-010：C，改成C后还有疑问的话问我` |
| 2026-11-02 | TASK-006 | TODO | IN_PROGRESS | 按 V2.4 追加用户回答，区分四项完整确认与 Q-010 部分确认；开始同步台账 | CLQ-011～CLQ-014、CLQ-010 |
| 2026-11-02 | TASK-006 | IN_PROGRESS | DONE | RAW-003～RAW-006 确认，新增 Q-011～Q-014 并关闭，新增 ADR-017～ADR-020；Q-010 方案 C 已登记但默认值/作用范围待确认；项目状态与索引同步，完成一致性检查和读回核验 | CLQ-010～CLQ-014、Raw_Requirements、Open_Issues、Decision_Log、问题澄清记录台账、Project_Status、project.md、docs README、检查报告 V1.4 |
| 2026-11-02 | TASK-007 | — | TODO | 用户确认 Q-010 设置作用范围及默认值，需要补齐澄清、需求、问题、决策和项目管理记录 | 用户原文：`Q-010 细节确认：设置作用范围选择 A；默认值选择 A。请按项目 V2.4 工作规范更新需求记录和相关台账，并进行读回核验。` |
| 2026-11-02 | TASK-007 | TODO | IN_PROGRESS | 按 V2.4 追加式登记用户回答，更新 Q-010 产品规则并隔离 P3 技术实现问题 | CLQ-010、RAW-013、Q-010、ADR-021 |
| 2026-11-02 | TASK-007 | IN_PROGRESS | DONE | Q-010 全局统一设置、默认开启已确认并关闭；新增 ADR-021；技术生命周期实现拆分为 P3 问题 Q-015；同步相关台账、索引、项目状态和摘要，完成资料库读回核验及一致性检查 | CLQ-010、Raw_Requirements、Open_Issues、Decision_Log、问题澄清记录台账、Clarifications README、Project_Status、project.md、docs README、检查报告 V1.5 |
| 2026-11-02 | TASK-008 | — | TODO | 根据用户要求检查 RAW-001/RAW-002 未收敛细节并提出带推荐的选项 | 用户本轮明确要求检查 RAW-001、RAW-002，有未收敛项则带推荐答案询问 |
| 2026-11-02 | TASK-008 | TODO | IN_PROGRESS | 已完成需求完整性检查，新增 Q-016～Q-018 与 CLQ-015～CLQ-017；等待用户回答后同步确认结论 | RAW-001/RAW-002；Open_Issues；问题澄清记录台账；CLQ-015～CLQ-017 |

| 2026-11-05 | TASK-008 | IN_PROGRESS | DONE | Q-016～Q-020 均已取得用户明确回答；RAW/Q/ADR/CR/RISK/CLQ、项目状态、看板和索引完成同步；读回核验及一致性检查完成 | 用户原文：Q-019 Windows Server 2012 及以上；Q-020 A。关联文件：Raw_Requirements、Open_Issues、Decision_Log、Change_Log、Risk_Register、CLQ-015～019、问题澄清记录台账、Project_Status、Task_Board、project.md、docs README、检查报告 V1.8 |

| 2026-11-06 | TASK-009 | — | TODO | P1 阶段评审与用户批准结果同步项目管理文件 | Project_Status.md 记录用户明确批准 P1 阶段成果；该批准不等于建立需求基线或启动 P2 |
| 2026-11-06 | TASK-009 | TODO | IN_PROGRESS | 按已批准的 P1 阶段评审结论核对并同步任务看板、项目摘要和文档索引 | 用户明确批准 P1；Project_Status.md 阶段评审记录 |
| 2026-11-06 | TASK-009 | IN_PROGRESS | DONE | P1 状态为 APPROVED，P2 保持 NOT_STARTED；需求基线未建立；完成管理文件同步与读回核验 | `/FolderHub/docs/00_Project_Management/Project_Status.md`、`/FolderHub/project.md`、`/FolderHub/Task_Board.md`、`/FolderHub/docs/README.md` |

## 4. 任务字段变更历史
| 日期 | TASK_ID | 字段 | 修改前 | 修改后 | 原因 |
|---|---|---|---|---|---|
| 2026-11-01 | TASK-002 | 完成证据/进展 | 进行中；逐题文件待写入资料库 | 10 个逐题文件及索引已写入并完成只读一致性检查 | 记录本次实际写入与验收结果 |
| 2026-11-01 | TASK-003 | 完成证据/进展 | 进行中；CLQ-001～CLQ-009 V1.1 待写入 | 9 个 CLQ、澄清索引及 Clarifications README 已覆盖写入并核验 | 记录 V1.1 追加恢复交付证据 |
| 2026-11-01 | TASK-005 | 完成证据/进展 | 四项推荐附加方案待登记 | 四个 Q 已关闭，相关权威台账和项目状态完成同步并读回核验 | 记录本轮用户补充确认的完成证据 |

| 2026-11-02 | TASK-006 | 完成证据/进展 | RAW-003～RAW-006 与 Q-010 回答待登记 | RAW-003～RAW-006 已确认；Q-011～Q-014 关闭；Q-010 方案 C 部分确认，待默认值/范围；相关台账与索引同步 | 记录本轮用户回答及当前未决边界 |
| 2026-11-02 | TASK-007 | 完成证据/进展 | Q-010 默认值/作用范围待确认 | 全局统一设置、默认开启已确认；Q-010 关闭；Q-015 跟踪 P3 技术实现；相关台账与项目摘要完成同步并读回核验 | 记录本轮用户确认和 P1/P3 边界 |
| 2026-11-02 | TASK-008 | 活跃任务数 | 0 | 1 | 新增 RAW-001/RAW-002 完整性检查与澄清跟踪任务 |

| 2026-11-05 | TASK-008 | 完成证据/进展 | 用户回答待收集 | Q-016～Q-020 已全部关闭，相关专业台账与项目管理记录已同步；RISK-001 保持 OPEN；一致性检查报告 V1.8 已建立 | 记录 Q-019/Q-020 确认及 TASK-008 完成依据 |
| 2026-11-08 | TASK-009 | 当前阶段引用 | P1_REQUIREMENT_COLLECTION / IN_PROGRESS | P1_REQUIREMENT_COLLECTION / APPROVED | 与 Project_Status 的用户批准记录保持一致；不表示 P2 已启动 |
| 2026-11-08 | TASK-010 | — | TODO | 用户确认已清理两份 RAW 同步副本，要求按 V2.4.2 目录规范复核并调整文件位置 | 用户本轮明确要求“按目录规范调整文件位置” |
| 2026-11-08 | TASK-010 | TODO | IN_PROGRESS | 已读取 SKILL.md、02-directory-standards.md；开始核对根目录及 docs 各阶段目录 | 当前资料库目录清单、V2.4.2 目录规范 |
| 2026-11-08 | TASK-010 | IN_PROGRESS | DONE | 完成根目录及 docs/plans 目录核查；根目录仅含允许保留的 project.md、Task_Board.md、README.md、.gitignore，研发文档均位于 docs 对应目录，done.md 位于 plans；无需移动文件。保留现有 05_Development_Management/06_Test_Management 命名以遵循已有项目约定；更新 docs 索引、项目状态和一致性检查报告 | `/FolderHub/docs/README.md`、`/FolderHub/docs/00_Project_Management/Project_Status.md`、`/FolderHub/docs/00_Project_Management/项目一致性检查报告-目录规范复核V2.0.md`、`/FolderHub/Task_Board.md`、`/FolderHub/plans/done.md` |

## 5. 管理约束
- BLOCKED 必须有阻塞原因；DONE 必须有完成证据。
- DONE/CANCELLED 为终态；重新执行创建新 TASK 并关联旧编号。
- 每项任务只有一个主阶段；跨阶段工作拆分或明确主阶段。
- CR、需求和阶段状态由各自权威台账管理。

## 6. 项目扩展任务类型（仅登记实际需要的扩展项）
| 扩展类型代码 | 定义 | 适用范围/使用边界 | 登记日期 |
|---|---|---|---|


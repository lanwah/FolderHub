# Task Board

- 项目名称：FolderHub
- 台账版本：5.2
- 最后更新：2026-11-21
- 工作流规范：已退出 V2.6.1 阶段门禁；工程实现以 `docs/05_Development/开发计划_v0.1.md` 为准
- 当前阶段：ENGINEERING_IMPLEMENTATION
- 活跃任务数：1
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
| TASK-020 | 按开发计划实现 FolderHub（WP0～WP10） | DEV | DEVELOPMENT | P1 | IN_PROGRESS | 按 `docs/05_Development/开发计划_v0.1.md` 推进；WP0/WP1 已完成；持续交付可编译可测切片 | REQ-BL-1.0；ADR-034～040；p3-demo-v0.1 | 助手/用户 | — | WP0 脚手架编译通过；WP1 Domain 8 测通过；`src/FolderHub.sln` | #8 | NOT_CHECKED | 2026-11-21 | 2026-11-21 |

## 3. 任务状态历史（追加式）

| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | CANCELLED | 用户退出原 P3/P4 工作流门禁，改走工程实现；设计文档保留参考 | 用户明确要求不再按现有流程推进并开始编码 |
| 2026-11-21 | TASK-018 | IN_PROGRESS | CANCELLED | 同上；CardHost 边界改在实现中按 ADR-034～038 落地 | 同上 |
| 2026-11-21 | TASK-020 | — | IN_PROGRESS | 建立工程实现任务；已完成 WP0/WP1 | `开发计划_v0.1.md`；`src/`；Domain 单测 |
| 2026-11-20 | TASK-017 | IN_PROGRESS | IN_PROGRESS | P3-02 总体架构候选、模块边界、Windows 集成及 8 个同类项目轻量调研形成；继续后续验证和架构收敛 | `docs/02_Architecture/P3-02_总体架构候选方案与权衡分析_v0.1.md` |
| 2026-11-20 | TASK-018 | IN_PROGRESS | IN_PROGRESS | Q-015 候选分析完成并提交 CLQ-152；等待用户选择，相关架构批准仍受阻 | `docs/02_Architecture/P3-02_总体架构候选方案与权衡分析_v0.1.md`；`docs/00_Project_Management/问题澄清记录台账.md` CLQ-152 |
| 2026-11-20 | TASK-019 | IN_PROGRESS | DONE | P3 Demo v0.1 已交付、自检及追踪资料齐备，用户明确评审通过；Demo 仍非正式设计基线 | `docs/design-demo/`；`docs/04_Reviews/P3_Demo_v0.1_自检报告.md`；`docs/04_Reviews/P3_Demo_v0.1_用户评审通过记录.md`；用户原文：“ok，demo审核通过，继续往前推进” |
| 2026-11-14 | TASK-014 | — | IN_PROGRESS | 按用户要求将 FolderHub 工作流规范同步至 V2.6.1；核对规则差异并更新管理文件，不修改需求/设计基线 | 用户明确请求：“帮我同步下此项目里的 software-predev-workflow-skill” |
| 2026-11-14 | TASK-014 | IN_PROGRESS | DONE | V2.6.1 适配文件已写回资料库并完成读回核验；只读一致性检查 PASS（Git/本地客户端 NOT_CHECKED） | `/FolderHub/docs/00_Project_Management/项目一致性检查报告-V2.6.1同步.md`；Project_Status、Open_Issues、Decision_Log、问题澄清记录台账、project.md、docs README、docs/templates/README、V2.6.1 Q/ADR/CLQ 模板、P2 工作记录、SRS v0.8 |
| 2026-11-10 | TASK-011 | — | TODO | 用户明确启动 P2；需将 P1 已确认需求结构化为 SRS | 用户原文：“启动P2” |
| 2026-11-10 | TASK-011 | TODO | IN_PROGRESS | 已完成需求分解并建立 SRS v0.1 草案 | `/FolderHub/docs/01_Requirements/SRS_FolderHub_v0.1.md` |
| 2026-11-10 | TASK-012 | — | TODO | P2 分析识别到主组件首次安装固定默认值尚未确认 | Q-022 / CLQ-020 |
| 2026-11-10 | TASK-013 | — | TODO | SRS 草案形成后需执行质量检查和阶段评审 | P2 工作记录 / SRS v0.1 |
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


| 2026-11-11 | TASK-012 | TODO | IN_PROGRESS | 用户已明确选择 Q-022 方案 A；完成 CLQ-020/Q-022/ADR-027/SRS v0.2 同步，继续处理其他 P2 需求缺口 | 用户原文：“FolderHub Q-022 我的选择：A。请按 P2 流程更新 CLQ-020、Q-022、必要的 ADR 和 SRS，并保留历史记录。” |

| 2026-11-11 | TASK-012 | IN_PROGRESS | IN_PROGRESS | 完成 Q-022 后继续 P2 缺口分析，新增 Q-023/CLQ-021；等待用户确认重复路径处理规则 | SRS v0.3 草案；CLQ-021；Open_Issues Q-023 |


| 2026-11-12 | TASK-011 | IN_PROGRESS | IN_PROGRESS | Q-023 已确认，SRS 修订为 v0.4，新增 FR-018；继续完善 P2 需求草案 | SRS_FolderHub_v0.4.md |
| 2026-11-12 | TASK-012 | IN_PROGRESS | IN_PROGRESS | Q-023/CLQ-021 已关闭并新增 ADR-028；新增 Q-024/CLQ-022 处理搜索语义缺口 | 用户原文：“FolderHub Q-023 我的选择：A。请按 P2 流程更新相关台账和 SRS，保留历史记录，并继续后续需求分析。” |


| 2026-11-12 | TASK-011 | IN_PROGRESS | IN_PROGRESS | Q-024 已确认，SRS 修订为 v0.5，FR-007 已细化；继续完善 P2 需求草案 | SRS_FolderHub_v0.5.md |
| 2026-11-12 | TASK-012 | IN_PROGRESS | IN_PROGRESS | Q-024/CLQ-022 已关闭并新增 ADR-029；新增 Q-025/CLQ-023 处理搜索跨分类范围 | 用户原文：“FolderHub Q-024 我的选择：A。请按 P2 流程更新相关台账和 SRS，保留历史记录，并继续后续需求分析。” |


| 2026-11-12 | TASK-011 | IN_PROGRESS | IN_PROGRESS | Q-025 已确认，SRS 修订为 v0.6，FR-007 明确跨分类搜索及结果去重；继续完善 P2 需求草案 | SRS_FolderHub_v0.6.md |
| 2026-11-12 | TASK-012 | IN_PROGRESS | IN_PROGRESS | Q-025/CLQ-023 已关闭并新增 ADR-030；新增 Q-026/CLQ-024 处理多显示器/DPI/分辨率变化后的位置恢复 | 用户原文：“Q-025：A” |

## 7. Q-151 澄清同步记录（2026-11-12）

- TASK-011：保持 IN_PROGRESS；SRS 草案补充 Q-151 批量重试失败处理规则，当前草案版本 v0.7，尚未批准。
- TASK-012：保持 IN_PROGRESS；Q-151 / CLQ-151 已 CLOSED / CONFIRMED，新增 ADR-031；后续 P2 需求缺口澄清继续。
- TASK-013：保持 TODO；仍需在 P2 需求缺口收敛后执行质量检查及阶段评审包。
- 阶段状态：P2_REQUIREMENT_ANALYSIS / IN_PROGRESS；需求基线 NOT_ESTABLISHED。


## 8. 2026-11-14 历史澄清结论同步记录
- TASK-011：继续 IN_PROGRESS；SRS 草案由 v0.7 更新为 v0.8，纳入 Q-029～Q-150 的 122 项确认规则；不建立需求基线。
- TASK-012：继续 IN_PROGRESS；Q-029～Q-150 已按历史摘要补入 Open_Issues、问题澄清记录台账、ADR-032 和 CLQ-025～CLQ-146；Q-026 仍 OPEN。
- TASK-013：保持 TODO；后续仍需执行 P2 完整质量检查与阶段评审准备。
- `docs/README.md` 阶段状态已与 Project_Status 对齐。
- 证据边界：Q-029～Q-150 原始逐字问答/完整选项缺失部分标记 RECONSTRUCTED/UNAVAILABLE。
- P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。


| 2026-11-14 | TASK-015 | — | IN_PROGRESS | 按用户要求对 FolderHub V2.6.1 项目实例执行独立复核，覆盖台账、状态字段、SRS 推导标记及 P2 门禁 | 用户明确要求执行 V2.6.1 项目实例独立复核 |
| 2026-11-14 | TASK-015 | IN_PROGRESS | DONE | 完成只读复核并形成独立报告；发现模板索引、Q/ADR/CLQ/CR 实例化、SRS 推导标记及 P2 门禁缺口；未修改需求/决策/变更/风险基线或阶段状态 | `/FolderHub/docs/00_Project_Management/项目一致性检查报告-V2.6.1项目实例独立复核.md` |


## 2026-11-14 V2.6.1 实例修复启动记录
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-14 | TASK-016 | — | IN_PROGRESS | 用户明确批准按六项顺序修复 V2.6.1 项目实例缺口 | 用户原文：“按以下顺序修复：1. 修正模板映射和台账字段规范……6. 完成修复后的独立一致性检查，再准备 P2 阶段评审。” |


## 2026-11-14 V2.6.1 修复后独立检查记录
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-14 | TASK-016 | IN_PROGRESS | IN_PROGRESS | 模板、台账、SRS 与阶段门禁修复已写回并完成读回检查；Q-026 仍等待用户确认，因此任务保持进行中 | `/FolderHub/docs/00_Project_Management/项目一致性检查报告-V2.6.1修复后独立检查.md`；`SRS_FolderHub_v0.9.md`；`P2_阶段评审准备包_v0.1_DRAFT.md` |


## 2026-11-15 Q-026 用户确认与同步记录
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-15 | TASK-011 | IN_PROGRESS | IN_PROGRESS | 根据 Q-026 用户选择 A 修订 SRS v0.10，新增 FR-019、更新 NFR-002 和追踪矩阵；草案待质量复核及用户评审 | `/FolderHub/docs/01_Requirements/SRS_FolderHub_v0.10.md`；ADR-033；CLQ-024 V1.2 |
| 2026-11-15 | TASK-012 | IN_PROGRESS | IN_PROGRESS | Q-026 / CLQ-024 已确认并关闭；继续处理其他 P2 需求缺口 | 用户原文：“FolderHub Q-026 我的选择：A。请按 V2.6.1 追加澄清历史并更新相关台账及 SRS。” |
| 2026-11-15 | TASK-016 | IN_PROGRESS | DONE | 六项 V2.6.1 项目实例修复已完成；Q-026 已明确选择 A 并完成台账/SRS 同步；读回检查通过；未批准 P2 或建立需求基线 | `/FolderHub/docs/00_Project_Management/项目一致性检查报告-V2.6.1实例修复.md`；`CLQ-024.md` V1.2；`ADR-033`；`SRS_FolderHub_v0.10.md`；`P2_阶段评审准备包_v0.2_DRAFT.md` |


## 2026-11-16 P2 最终收尾与评审提交记录
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-16 | TASK-011 | IN_PROGRESS | IN_PROGRESS | SRS 从 v0.10 修订为 v0.11；完成 RAW→FR/BR/UIR/NFR 追踪矩阵、条目字段复核及矛盾修正；SRS 仍为 DRAFT，等待用户评审 | `SRS_FolderHub_v0.11.md`；`P2_需求质量检查与追踪矩阵复核报告_v1.0.md` |
| 2026-11-16 | TASK-012 | IN_PROGRESS | IN_PROGRESS | Q-026 已关闭；Q-015 有效延期保持；已识别图标来源和宿主异常恢复语义为未决边界，未擅自作出用户决策 | `Open_Issues.md`；`SRS_FolderHub_v0.11.md` 第 13 章 |
| 2026-11-16 | TASK-013 | TODO | IN_PROGRESS | 已完成追踪矩阵复核、质量检查和 P2 评审准备包 v0.3；报告结论 WARNING / READY_FOR_USER_REVIEW_WITH_NOTES，等待用户评审 | `P2_需求质量检查与追踪矩阵复核报告_v1.0.md`；`项目一致性检查报告-P2最终收尾V1.0.md`；`P2_阶段评审准备包_v0.3_DRAFT.md` |


## 2026-11-20 P2 批准与 P3 启动任务同步
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-20 | TASK-011 | IN_PROGRESS | DONE | SRS v0.11 获用户批准并建立需求基线 REQ-BL-1.0 | Q-P2-APPROVAL；P2_阶段评审批准记录_v1.0.md；SRS v0.11 |
| 2026-11-20 | TASK-012 | IN_PROGRESS | DONE | P2 需求缺口处理收尾；Q-015 作为 P3 架构事项继续跟踪，不再作为 P2 需求分析任务 | Open_Issues.md；Q-015 P3 任务 TASK-018 |
| 2026-11-20 | TASK-013 | IN_PROGRESS | DONE | P2 追踪矩阵、质量检查及评审准备包已完成，且 P2 阶段已获用户批准 | P2_需求质量检查与追踪矩阵复核报告_v1.0.md；P2_阶段评审批准记录_v1.0.md |
| 2026-11-20 | TASK-017 | — | IN_PROGRESS | 用户明确授权进入 P3 概要设计，启动总体架构与设计框架分析 | 用户原文：`FolderHub Q-P2-APPROVAL 我的选择：A。批准 SRS v0.11，建立需求基线，并授权进入 P3 概要设计。` |
| 2026-11-20 | TASK-018 | — | IN_PROGRESS | P3 启动触发 Q-015 延期目标节点，启动独立卡片宿主生命周期架构分析 | Q-015 / Open_Issues.md；REQ-BL-1.0 |


## 2026-11-20 P3 Demo 启动记录
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-20 | TASK-019 | — | IN_PROGRESS | 用户明确要求按已批准 REQ-BL-1.0 开始制作 P3 交互式网页 Demo；按 V2.6.1 仅创建概要设计评审辅助草案 | 用户原文：“按已批准的 `REQ-BL-1.0` 开始制作 FolderHub 的 P3 交互式网页 Demo。”；`docs/design-demo/` |


## 2026-11-20 P3 Demo 用户评审通过与归档
| 日期 | TASK_ID | 字段 | 修改前 | 修改后 | 原因 |
|---|---|---|---|---|---|
| 2026-11-20 | TASK-019 | 状态/完成证据 | IN_PROGRESS；等待用户评审 | DONE；用户评审通过记录已归档，Demo 仍为 P3 草案 | 用户明确评审通过当前 Demo v0.1；任务完成不等于 P3 阶段批准或设计基线建立 |


## 2026-11-20 Q-015 用户选择与 P3 架构收敛进展
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-20 | TASK-017 | IN_PROGRESS | IN_PROGRESS | Q-015 用户选择 A 后，P3-02 更新至 v0.2；新增 CardHost 生命周期、IPC/数据边界和异常恢复候选；继续总体架构、技术验证和追踪矩阵收敛 | P3-02 v0.2；P3-03 架构收敛与验证计划 |
| 2026-11-20 | TASK-018 | IN_PROGRESS | IN_PROGRESS | 用户明确选择 Q-015 / CLQ-152 A；Q-015 已 CLOSED / CONFIRMED，ADR-034 已登记；异常恢复/IPC/持久化等实现细节仍需设计和验证 | CLQ-152 V3；Open_Issues.md；ADR-034；P3-02 v0.2 |


## 2026-11-20 P3-04 数据模型与持久化边界分析进展
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-20 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 形成 P3-04 数据实体、数据分类、持久化写入权候选、版本一致性及迁移恢复边界；提交 Q-152 / CLQ-153 用户选择 | `docs/02_Architecture/P3-04_CardHost数据模型与持久化边界_v0.1.md`；`CLQ-153.md` |
| 2026-11-20 | TASK-018 | IN_PROGRESS | IN_PROGRESS | 延续 ADR-034 CardHost 独立进程模型，补充卡片状态写入权依赖；Q-152 已确认 A（ADR-035）；继续 IPC 契约、事务/补偿及异常恢复分析 | P3-04 v0.2；P3-05 v0.1；Q-152 / CLQ-153 |


## 2026-11-20 Q-152 用户选择确认与 P3-05 推进记录
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-20 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 用户选择 Q-152 / CLQ-153 A；新增 ADR-035，P3-04 更新至 v0.2；继续 IPC 契约、跨聚合一致性、物理存储候选与验证计划 | CLQ-153 V2；ADR-035；P3-04 v0.2 |
| 2026-11-20 | TASK-018 | IN_PROGRESS | IN_PROGRESS | Q-152 逻辑写入权阻塞已解除；继续 CardHost 断连/重连、崩溃恢复、会话隔离、部署和回退设计 | ADR-034/035；P3-03；P3-04 v0.2 |

- 任务状态未变更；本次仅追加进展和证据。TASK-017/TASK-018 仍 IN_PROGRESS，P3 阶段未完成。


## 2026-11-21 P3-05 IPC 传输候选比较进展
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | P3-05 更新至 v0.2，完成 Windows Named Pipes、Local RPC、TCP loopback、Shared Memory 候选比较和官方安全/兼容性资料核验；新增 Q-153/CLQ-154 等待用户确认；其他无依赖 P3 工作继续 | P3-05 v0.2；Q-153/CLQ-154；Microsoft Learn IPC/Named Pipe/.NET Framework 文档 |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | 衔接 P3-06 生命周期与崩溃恢复分析；IPC 传输尚待确认，不阻断协议无关恢复状态机工作 | P3-03 v0.1；P3-05 v0.2；ADR-034/035；Q-153/CLQ-154 |

- 本轮未改变任务状态；活跃任务仍为 TASK-017、TASK-018。

| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 新增 P3-06 v0.1，完成 CardHost 生命周期状态机、主程序退出/崩溃场景、R1-R4 恢复候选、重启节流/残留进程及验证矩阵；REC-01/02 保留 D2 待决策，不擅自引入 Supervisor | P3-06 v0.1；ADR-021/034/035；REQ-BL-1.0 |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | 继续 CardHost 崩溃恢复、会话隔离、注销/RDP/Explorer 和升级异常分析；RISK-002 已 SCREENED，完整评估安排在 IPC 技术确认后、P3 评审前 | P3-06 v0.1；RISK-002；P3-05 v0.2；Q-153/CLQ-154 |


## 2026-11-21 Q-153 用户选择确认与 Named Pipes 设计推进
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 用户选择 Q-153 / CLQ-154 A；新增 ADR-036 确认 Windows Named Pipes；P3-05 更新至 v0.3，补充安全约束与 NP-01～NP-11 PoC 验收矩阵；继续接口实现边界、OS 验证和需求追踪 | CLQ-154 V2.0；ADR-036；P3-05 v0.3；RISK-002 ASSESSED |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | IPC 传输选择已确认，P3-06 更新至 v0.2 纳入 Named Pipes 故障语义；REC-01/02 恢复策略仍待后续决策，继续恢复、资源、部署和回退分析 | ADR-036；P3-06 v0.2；RISK-002 ASSESSED |

- 任务状态未改变；TASK-017 / TASK-018 均继续 `IN_PROGRESS`。Q-153 的用户选择闭环不代表 P3 完成或设计基线建立。


## 2026-11-21 P3-07 安全设计与 Q-154 恢复策略澄清
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 新增 P3-07 v0.1，完成 Named Pipes 威胁模型、端点访问控制、身份握手、消息层安全和 NP-01～NP-11 隔离 PoC 验收计划；新增 Q-154/CLQ-155 处理 CardHost 崩溃恢复策略 | P3-07 v0.1；Q-154/CLQ-155；ADR-036；RISK-002 ASSESSED |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | Q-154/CLQ-155 提交用户选择，分析 Main Process 已退出时 CardHost 崩溃恢复方案；不依赖该决定的 Named Pipes 安全 PoC 和协议工作继续 | P3-06 v0.2；CLQ-155 V1.0；ADR-021/034/035/036 |

- TASK-017 / TASK-018 均保持 IN_PROGRESS；未发生阶段状态转换。


## 2026-11-21 Q-154 用户选择确认与 P3 推进
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | Q-154/CLQ-155 用户选择 A，新增 ADR-037；P3-06 更新 v0.3，恢复策略收敛；继续整体架构、需求追踪、Named Pipes 安全 PoC 与目标 OS 验证 | CLQ-155 V2.0；ADR-037；P3-06 v0.3；一致性检查报告 V1.0；用户原文：“FolderHub Q-154 / CLQ-155 我的选择：A。请按 software-predev-workflow-skill V2.6.1 追加用户选择记录，更新相关台账和 ADR，并继续 P3。” |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | Q-154 决策已关闭；不增加常驻 Supervisor，Main Process 下次启动恢复最后成功持久化状态；继续持久化原子性、恢复对账、失败诊断/重试及宿主运行期间故障验证 | CLQ-155 V2.0；ADR-037；P3-06 v0.3；一致性检查报告 V1.0 |

- 任务状态未改变；TASK-017 / TASK-018 均继续 `IN_PROGRESS`。Q-154 用户选择闭环不代表 P3 完成或设计基线建立。


## 2026-11-21 P3-08 需求追踪矩阵与质量属性分析
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 新增 P3-08 v0.1，完成 REQ-BL-1.0 的 37 项正式需求到逻辑模块、接口边界和验证工作追踪；补充质量属性现状及 GAP-01～GAP-08；映射完成不等于实现/测试通过 | `docs/02_Architecture/P3-08_需求-模块-接口-验证追踪矩阵与质量属性分析_v0.1.md`；P3 计划 v0.9 |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | P3-08 将 CardHost 持久化/恢复、CardDefinition 对账、Main Process 运行期间 HostLost、RDP/Explorer/升级场景纳入 VFY-10/VFY-15/GAP-02/GAP-03/GAP-07；继续开展隔离 PoC | P3-08 v0.1；P3-06 v0.3；ADR-034/035/037 |

- 本轮未改变任务状态；TASK-017 / TASK-018 均继续 `IN_PROGRESS`。P3-08 的 37/37 仅为文档追踪映射，不是实现或测试覆盖率。


## 2026-11-21 P3 测试环境与运行期恢复策略集中确认
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | Q-P3-TEST-001=E、Q-P3-TEST-002=尚未盘点；明确多环境测试策略但实际环境清单待盘点；Q-P3-CRASH-001=A，新增 ADR-038；继续总体架构、PoC和目标OS验证 | 用户集中确认原文；Project_Status v4.7；P3计划v1.0；ADR-038 |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | Main Process 存活期间由其协调 CardHost 有限自动重启和退避；与 ADR-037 的 Main Process 已退出场景分离；重试参数、幂等、恢复和失败提示仍需 PoC | Q-P3-CRASH-001；ADR-038；P3-06 v0.4 |

- 本轮不改变 TASK-017/TASK-018 状态，不新增 DONE 任务；PoC 尚未执行，不能将设计策略记录为验证通过。


## 2026-11-21 P3 隔离 PoC 工具包准备
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 建立 P3 PoC 工具包，准备环境探测、进程恢复模拟器、部署/停止/清理脚本及测试证据模板；未选定产品运行时，未执行 Windows PoC | `docs/06_Test_Management/P3_PoC_Starter_Kit_v0.1/`；`FH-TEST-POC-001`；`P3_概要设计工作计划_v1.1.md` |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | 建立 ADR-038 运行期故障模拟器骨架；只验证模拟 worker 生命周期控制流，不证明真实 CardHost/IPC/持久化恢复通过 | `Invoke-FolderHubRecoveryPoC.ps1`；`P3_CardHost_Runtime_Recovery_PoC_v0.2.md`；REC-POC-01～05 `NOT RUN` |

- 本轮任务状态保持不变；无任务 DONE/归档；Git Commit/Tag 仍 `NOT_CHECKED`。


## 2026-11-21 P3 实际 Windows 环境证据追加
| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 收到一台 Windows x64 / RDP 环境采集 JSON，新增 W11-RDP-001；OS Caption 与 ProductName 冲突，需独立核实；其他目标 OS 仍未盘点 | 用户上传 `environment-inventory.json`；`P3_Test_Environment_Inventory_v0.4.md`；Project_Status v4.9 |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | 当前环境可作为隔离运行期恢复模拟的候选环境，但尚未运行 REC-POC-01～05；RDP/非管理员条件需纳入结果解释 | W11-RDP-001；PoC 工具包 FH-TEST-POC-001 v0.1；环境盘点 v0.4 |

- 本轮 TASK-017/TASK-018 状态不变；无任务 DONE/归档。环境采集不等于 PoC 执行；Git Commit/Tag 仍 NOT_CHECKED。

- 工具包源码复核发现恢复模拟器依赖 Windows PowerShell 5.1 的 `powershell.exe` 路径；W11-RDP-001 当前报告 PowerShell 7.6.6，运行 REC-POC 前需切换/确认 Windows PowerShell 5.1 宿主。

## 2026-11-21 P3 OS 身份核实与 REC-POC-01 执行证据

| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 独立核实 W11-RDP-001 为 Windows 11 专业版 x64；REC-POC-01 完成模拟进程生命周期验证；仍需完成架构验证、矩阵覆盖和评审准备 | 用户提供 Win32_OperatingSystem 截图；recovery-summary JSON；recovery-events JSONL；P3_Test_Environment_Inventory_v0.5.md |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | REC-POC-01 在 harness 范围观察到一次异常退出、1 秒退避、第二次启动和 5 秒进程存活窗口；不证明真实 CardHost 健康或持久化/IPC 恢复 | RunId `20ee473945ab4343a29533f3bb5a891a`；REC-POC-01 `PASS_WITH_LIMITATIONS` |

- TASK-017/TASK-018 均继续 `IN_PROGRESS`，无任务 DONE/归档。
- REC-POC-02～05、NP-01～11、CP-01～10、桌面集成测试仍未执行。
- Git Commit/Annotated Tag 仍 `NOT_CHECKED`。

## 2026-11-21 Q-P3-GATE-001 用户选择 A 后任务状态

| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 停止后续测试，转为文档级总体架构收敛、需求追踪复核和 P3 预审准备；GAP-01/GAP-06 与 RISK-002 仍阻断 P3 批准 | Q-P3-GATE-001 / CLQ-157；P3-09 v0.1；P3 阶段评审准备包 v0.1_DRAFT |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | 不执行后续恢复/安全 PoC；保留 REC-POC-01 有限证据和所有未执行项，完整披露持久化/恢复及 IPC 安全缺口 | REC-POC-01 `PASS_WITH_LIMITATIONS`；P3-06 v0.4；P3-07 v0.1；RISK-002 `ASSESSED` |

- TASK-017/TASK-018 均保持 `IN_PROGRESS`；没有任务 DONE/归档。
- 本次用户选择不改变任务状态；只调整后续工作边界。P3 仍 `IN_PROGRESS`，P4 `NOT_STARTED`。
- 后续只进行文档整理、追踪、风险/质量核对和用户预审；不执行 REC/NP/CP/桌面集成/兼容性测试。

## 2026-11-21 P3 集中澄清与评审提交任务记录

| 日期 | TASK_ID | 原状态 | 新状态 | 原因 | 证据/用户表达 |
|---|---|---|---|---|---|
| 2026-11-21 | TASK-017 | IN_PROGRESS | IN_PROGRESS | 用户确认 SQLite 本地数据库（ADR-039）和多显示器/DPI 位置策略（ADR-040）；更新 P3-04/P3-08/P3-09 与 P3 评审包并提交用户评审；仍需用户批准 HLD，Git/实机验证未检查 | CLQ-158/159/160；ADR-039/040；P3-04 v0.3；P3-08 v0.2；P3-09 v0.2；P3 Review v0.2 |
| 2026-11-21 | TASK-018 | IN_PROGRESS | IN_PROGRESS | RISK-002 仅在 P4 详细设计阶段受限接受；安全缓解仍未实施/验证，开发/真实数据/运行/部署/发布前验证门禁保留；持久化/恢复 PoC 仍未执行 | CLQ-160；Risk_Register 1.7；P3-04 v0.3；P3 Review v0.2 |

- TASK-017/TASK-018 均继续 `IN_PROGRESS`；本轮无任务 DONE/归档。
- P3 `REVIEW_PENDING`；P4 `NOT_STARTED`；HLD 基线未建立。任务状态不替代阶段批准。

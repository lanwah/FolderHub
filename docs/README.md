# FolderHub 文档索引

本目录统一存放 FolderHub 项目的研发过程文档。当前工作流规范：`software-predev-workflow-skill V2.6.1`。项目源代码、测试代码、脚本和配置与研发文档分离。项目管理入口 `project.md`、`Task_Board.md`、`plans/done.md` 位于项目根目录/ plans，是 V2.4 管理记录例外；正式研发文档仍统一归档于 `docs/`。

## 文档目录
| 目录 | 用途 | 当前内容 |
|---|---|---|
| `00_Project_Management/` | 项目章程、项目状态、决策、变更、建议、澄清及风险管理 | `Project_Charter.md`、`Project_Status.md`、`Decision_Log.md`、`Change_Log.md`、`Assistant_Suggestions.md`、`Directory_Initialization_Record.md`、`问题澄清记录台账.md`、`Clarifications/`（CLQ-001～CLQ-024、CLQ-025～CLQ-146、CLQ-151～CLQ-160）、`项目一致性检查报告-澄清历史恢复V1.0.md`、`项目一致性检查报告-澄清历史追加恢复V1.1.md`、`项目一致性检查报告-用户选择登记V1.2.md`、`项目一致性检查报告-用户补充确认V1.3.md、项目一致性检查报告-用户确认RAW003-006及Q010-V1.4.md、项目一致性检查报告-用户确认Q010细节V1.5.md`、`项目一致性检查报告-RAW001-002完整性检查V1.6.md`、`项目一致性检查报告-用户确认Q016-018V1.7.md`、`Risk_Register.md`、`项目一致性检查报告-V2.6.1同步.md`、`项目一致性检查报告-V2.6.1项目实例独立复核.md` |
| `01_Requirements/` | 原始需求、问题、正式需求与追踪 | `Raw_Requirements.md`、`Open_Issues.md`、`SRS_FolderHub_v0.11.md`（当前已批准需求基线 REQ-BL-1.0；v0.1～v0.10 保留为历史版本）、`P2_需求分析工作记录_v0.9.md` |
| `02_Architecture/` | 概要架构与技术方案 | `P3_概要设计工作计划_v1.3.md（当前，REVIEW_PENDING）`（当前工作计划，非设计基线；旧文件名/版本保留历史）、`P3-01_系统上下文与边界分析_v0.1.md`、`P3-02_总体架构候选方案与权衡分析_v0.1.md（内容 v0.2）`、`P3-03_CardHost生命周期与进程边界收敛_v0.1.md`、`P3-04_CardHost数据模型与持久化边界_v0.3.md（SQLite/显示策略已确认）`、`P3-05_MainProcess-CardHost_IPC消息契约与同步语义分析_v0.3.md`、`P3-06_CardHost生命周期与崩溃恢复策略分析_v0.3.md、P3-06_CardHost生命周期与崩溃恢复策略分析_v0.4.md、P3_CardHost_Runtime_Recovery_PoC_v0.1.md（历史）、P3_CardHost_Runtime_Recovery_PoC_v0.2.md`、`P3-07_NamedPipes_IPC安全边界与PoC验证计划_v0.1.md、P3-09_总体架构收敛与关键验证状态_v0.2.md`、`P3-08_需求-模块-接口-验证追踪矩阵与质量属性分析_v0.2.md`；P3-05 v0.1/v0.2、P3-06 v0.1 为历史修订（均 DRAFT / 非设计基线） |
| `design-demo/` | P3/P4 可交互网页设计 Demo | `README.md`、`change-log.md`、`current/`（p3-demo-v0.1，P3_DEMO_DRAFT） |
| `03_Detailed_Design/` | 详细设计 | 当前尚无正式设计文档 |
| `04_Reviews/` | 阶段评审记录 | `项目一致性检查报告-Q153选择确认与P3-07安全设计V1.0.md、项目一致性检查报告-Q154选择确认与P3继续推进V1.0.md`（Q-153 选择确认与 P3-07 安全设计检查，WARNING）、`P2_需求质量检查与追踪矩阵复核报告_v1.0.md`、`项目一致性检查报告-P2最终收尾V1.0.md`、`P2_阶段评审准备包_v0.3_DRAFT.md`（提交评审时的历史准备包）；`P2_阶段评审批准记录_v1.0.md`；`项目一致性检查报告-P2批准与P3启动V1.0.md`、`P3_Demo_v0.1_自检报告.md`、`P3_Demo_v0.1_用户评审通过记录.md`、`项目一致性检查报告-P3Demo启动V1.0.md`、`项目一致性检查报告-P3Demo评审通过与P3-01推进V1.0.md`、`项目一致性检查报告-P3-02架构候选分析V1.0.md、项目一致性检查报告-Q015用户选择与P3架构收敛V1.1.md` |
| `05_Development_Management/` | 开发过程管理 | 按阶段启用 |
| `06_Test_Management/` | 测试过程管理 | `P3_Test_Environment_Inventory_v0.1.md`（历史）、`P3_Test_Environment_Inventory_v0.2.md`（历史）、`P3_Test_Environment_Inventory_v0.3.md`（历史）、`P3_Test_Environment_Inventory_v0.4.md`（当前，已登记 W11-RDP-001，OS 字段待核实）；`Get-FolderHubEnvironment_v0.2.ps1`；`P3_PoC_Starter_Kit_v0.1.zip`（隔离模拟器/部署清理脚本/测试矩阵/证据模板/Runbook）；`P3_PoC_工程骨架与执行说明_v0.1.md` |
| `templates/` | 本项目使用的工作流模板副本 | Q、ADR、CR、CLQ 已映射至 V2.6.1 独立字段模板；旧合并模板保留为历史兼容 |
| `_Archive/` | 历史或已替代文件 | 如存在，按原状保留 |

## V2.4 项目管理入口
- 项目摘要看板：项目根目录 `project.md`（只读汇总）。
- 任务台账：项目根目录 `Task_Board.md`（TASK 唯一权威源）。
- 已完成任务归档：项目根目录 `plans/done.md`（追加式）。
- 阶段状态权威源：`00_Project_Management/Project_Status.md`。
- 风险权威源：`00_Project_Management/Risk_Register.md`。
- 问题澄清索引：`00_Project_Management/问题澄清记录台账.md`。
- 澄清逐题过程：`00_Project_Management/Clarifications/README.md` 及 CLQ-001～CLQ-024、CLQ-025～CLQ-146、CLQ-151～CLQ-156；其中 CLQ-025～CLQ-146 为历史摘要重建记录。

## 当前研发阶段
- 当前阶段：`P3_HIGH_LEVEL_DESIGN`
- 阶段状态：`IN_PROGRESS`
- P1 需求采集、P2 需求分析：`APPROVED`
- 当前 SRS：`SRS_FolderHub_v0.11.md`（APPROVED / BASELINED）
- 需求基线：`REQ-BL-1.0`
- P4 详细设计：`NOT_STARTED`
- 当前未决项：Q-015（CLOSED / D2 / CONFIRMED；ADR-034）、Q-152（CLOSED / D2 / CONFIRMED；ADR-035）、Q-153（CLOSED / D2 / CONFIRMED；ADR-036 / Windows Named Pipes；安全 PoC 待验证）、Q-154（CLOSED / D2 / CONFIRMED；CLQ-155 V2.0 / ADR-037，不增加常驻 Supervisor，下次 Main Process 启动时恢复）；Q-026 已于 2026-11-15 CLOSED / CONFIRMED（D2，ADR-033），实现算法留待 P3/P4
- P3 Demo v0.1：用户评审通过；仍为 P3 草案，不等于概要设计批准或设计基线
- P3-02：总体架构候选与模块边界分析 v0.2；P3-03：CardHost 生命周期与进程边界收敛 v0.1；Q-015 / CLQ-152 用户选择 A 已确认，详见 ADR-034
- 当前 P3 活跃任务：TASK-017、TASK-018；TASK-019 已完成并归档
- Q-029～Q-150：已依据历史摘要补录结论；逐字证据缺失项标记 RECONSTRUCTED / UNAVAILABLE。

## 管理规则
1. 所有正式研发文档统一放在项目根目录 `docs/` 下；V2.4 管理入口文件按技能规范放在根目录及 `plans/`。
2. 不将助手建议当作用户确认的需求或决策。
3. 未经用户明确批准，不进入下一研发阶段。
4. 已确认需求和批准基线不得静默修改；变更必须登记并进行影响分析。
5. 每次实质工作维护需求、问题、决策、变更、风险台账和项目阶段状态。
6. 项目阶段以 `Project_Status.md` 为准；任务状态以根目录 `Task_Board.md` 为准。

> **历史记录说明：**以下 V1.x 记录保留其发生时的项目状态（例如当时的 P1 IN_PROGRESS），仅作历史审计，不代表当前状态。当前阶段、当前 SRS 和未决项以上方“当前研发阶段”及 `Project_Status.md` 为准。

> **历史状态说明：**以下 V1.x 章节按记录发生时的项目状态保留，仅作历史审计，不代表当前状态。当前阶段、SRS 版本和未决问题以上方“当前研发阶段”及 `Project_Status.md` 为准。

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


## 2026-11-10 P2 需求分析澄清
- 新增 `CLQ-020.md`，关联 Q-022：主组件首次安装时“固定到桌面”开关默认值。
- Q-022 状态 OPEN / PENDING_CONFIRMATION；选项和助手推荐不代表用户决定。
- P2 已启动；SRS v0.1 为草案，需求基线未建立。


## 2026-11-11 Q-022 用户确认
- 用户选择 A：首次安装时主组件“固定到桌面”默认关闭，用户可主动开启。
- Q-022 / CLQ-020 已 CLOSED / CONFIRMED；新增 ADR-027；SRS 更新为 v0.2 草案。
- 历史选项、助手推荐和原待答状态均保留在 `CLQ-020.md` 的追加式修订记录中。


## 2026-11-11 Q-022 用户确认
- 用户选择 A：首次安装时主组件固定到桌面默认关闭，用户可主动开启。
- Q-022 / CLQ-020：CLOSED / CONFIRMED。
- 新增 ADR-027；SRS 更新为 v0.2 草案。
- 原有 CLQ-020 选项和助手推荐保留，用户原始回答逐字追加。


## 2026-11-12 Q-024 用户确认与 Q-025 新增
- Q-024 / CLQ-022：用户选择 A，已关闭；新增 ADR-029。
- 搜索采用不区分大小写包含匹配、输入动态更新、清空恢复当前分类列表、无结果提示。
- Q-025 / CLQ-023：搜索范围跨分类或仅当前分类，当前 OPEN / PENDING_CONFIRMATION。


## 2026-11-12 Q-025 用户确认与 Q-026 新增
- Q-025 / CLQ-023：用户选择 A，已关闭；新增 ADR-030。
- 搜索跨全部分类；同一记录仅显示一次并显示所属分类；清空恢复当前分类列表。
- Q-026 / CLQ-024：多显示器/DPI/分辨率变化后的位置恢复，OPEN / PENDING_CONFIRMATION。

## 2026-11-12 Q-151 澄清记录
- 新增 `00_Project_Management/Clarifications/CLQ-151.md`，记录待清理记录批量重试失败处理与续行规则。
- Q-151 / CLQ-151 已 CLOSED / CONFIRMED；新增 ADR-031；SRS 草案更新至 v0.7，仍待用户评审。
- P2 保持 IN_PROGRESS，需求基线未建立；P3/P4 未启动。


## 2026-11-14 状态与澄清历史同步
- 当前阶段摘要与 Project_Status.md 对齐：P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。
- 当前 SRS 草案为 v0.8；v0.7 保留为历史版本。
- Q-029～Q-150 已补录到问题台账、决策汇总、逐题澄清记录及 SRS 草案；证据边界见各 CLQ 文件。

## V2.6.1 决策澄清与状态记录规则
- 对未知事项先按 D1/D2/D3 判级；无法可靠判级时保持待判级并采取保守措施。
- D1 必须取得用户明确确认；D2 可集中提问或按规则有效延期；D3 仅限低风险、可逆且可可靠推导的普通细节，由助手记录来源、依据、影响和验证方式，不创建 Q/CLQ。
- 实际提出的 D1/D2 问题继续执行 V2.2 全过程追加记录，必须提供选项、优缺点、推荐及理由，并保留用户历次回答。
- 分别判断记录完整性、决策完成度、阶段阻塞性、延期有效性；不得将记录完整误判为决策完成，也不得将延期误判为用户接受。
- RAW、Q、ADR、CR、风险和阶段状态继续由各自权威台账管理；决策处理状态应独立记录，不替换原状态字段。
- CR 涉及实质性用户决策时，必须执行 D1/D2/D3 判级及必要的 Q/CLQ/ADR 关联。

### V2.6.1 模板副本
`templates/` 中已同步独立的 `未决问题台账模板.md`、`决策台账模板.md`、`问题澄清记录台账模板.md`。原 `问题与决策台账模板.md` 保留为历史兼容模板，不用于新建台账。


## V2.6.1 实例修复阶段记录（2026-11-14 历史快照）
- 截至本记录日期，Q-026 为 D2 / PENDING_CONFIRMATION；四维状态 COMPLETE / PENDING / PARTIAL / NOT_DEFERRED。该状态已由 2026-11-15 的用户确认记录更新，当前状态见上方“当前研发阶段”及本文件末尾。
- Q-015：D2 / DEFERRED；有效延期至 P3 架构决策/基线前。
- CR-001：D1 / CONFIRMED；CR 状态仍 APPROVED；RISK-001 的 ACCEPTED 风险及兼容性验证义务保持不变。
- ADR-032 的 122 个历史子项已拆分状态字段，RECONSTRUCTED/UNAVAILABLE 证据边界保留。
- 当时 SRS v0.9 为 DRAFT；AI_DERIVED 已登记，未采纳任何未确认 DESIGN_ASSUMPTION。当前 SRS 已更新为 v0.10。
- P2 仍 IN_PROGRESS；需求基线 NOT_ESTABLISHED；P3/P4 NOT_STARTED。


## 2026-11-15 Q-026 用户确认同步
- Q-026 / CLQ-024：D2 / CLOSED / CONFIRMED；用户选择 A，详见 `00_Project_Management/Clarifications/CLQ-024.md` V1.2。
- 新增 ADR-033；当时 SRS 为 `01_Requirements/SRS_FolderHub_v0.10.md`（DRAFT）；v0.9 及更早版本保留为历史草案。
- 当时 P2 阶段评审准备草案：`04_Reviews/P2_阶段评审准备包_v0.2_DRAFT.md`；仍需最终追踪矩阵和质量检查，未构成用户批准。
- P1 APPROVED；P2 IN_PROGRESS；P3/P4 NOT_STARTED；需求基线 NOT_ESTABLISHED。


## 2026-11-16 P2 最终收尾同步
- 当前 SRS 草案：`01_Requirements/SRS_FolderHub_v0.11.md`。
- 质量检查：`04_Reviews/P2_需求质量检查与追踪矩阵复核报告_v1.0.md`（WARNING / READY_FOR_USER_REVIEW_WITH_NOTES）。
- 评审准备包：`04_Reviews/P2_阶段评审准备包_v0.3_DRAFT.md`。
- P2：`REVIEW_PENDING`，等待用户评审；未批准 SRS/P2，未建立需求基线，未启动 P3。


## 2026-11-20 阶段状态更新
- 用户批准 SRS v0.11，建立需求基线 `REQ-BL-1.0`，并授权进入 P3 概要设计。
- P2：APPROVED；P3：IN_PROGRESS；P4：NOT_STARTED。
- P3 当前重点：总体架构候选方案分析、Q-015 生命周期架构分析。


## 2026-11-20 P3 Demo 工作记录
- 已新增 `design-demo/current/` 可交互网页原型，版本 `p3-demo-v0.1`，状态 `P3_DEMO_DRAFT`。
- 需求输入为已批准基线 `REQ-BL-1.0`；不修改 SRS 或需求/决策/变更/风险基线。
- 自检报告：`04_Reviews/P3_Demo_v0.1_自检报告.md`；JavaScript 语法检查 PASS，Windows 实机及系统级行为 NOT_CHECKED。
- TASK-019 IN_PROGRESS；P3 IN_PROGRESS；P4 NOT_STARTED。


## 2026-11-20 P3 Demo 用户评审通过与 P3-01 进展
- `p3-demo-v0.1` 用户评审通过，批准范围仅为 Demo；已知模拟/未验证限制保留在评审记录。
- TASK-019 已 DONE 并归档；P3-01 系统上下文与边界分析草案已加入 `02_Architecture/`。
- P3 继续 IN_PROGRESS，P4 NOT_STARTED；概要设计基线尚未建立。


## 2026-11-20 P3-01 推进后只读一致性检查
- 报告：`04_Reviews/项目一致性检查报告-P3Demo评审通过与P3-01推进V1.0.md`。
- 结论：WARNING（PASS 8 / WARNING 1 / FAIL 0 / NOT_CHECKED 2）；WARNING 为 Demo 真实 Windows 集成仍未实现/验证；Git、本机浏览器和 Windows 实机检查仍 NOT_CHECKED。

- Q-015：用户选择 A 已登记为 ADR-034；Q-015 CLOSED / CONFIRMED。P3-03 已形成 CardHost 生命周期与进程边界收敛草案；不代表完整 P3 批准或设计基线。


## 2026-11-20 P3-04 更新
- `02_Architecture/P3-04_CardHost数据模型与持久化边界_v0.2.md`：逻辑数据实体、CardHost 持久化边界候选及验证计划（DRAFT，非设计基线）。
- `00_Project_Management/Clarifications/CLQ-153.md`：Q-152 卡片持久化写入权澄清（D2 / CLOSED / CONFIRMED；ADR-035）。
- Q-152 已确认 A，新增 ADR-035；仅确认逻辑写入权，未选择数据库/物理存储/IPC 技术；SRS v0.11 / REQ-BL-1.0 不变。


## 2026-11-20 P3-05 推进
- `02_Architecture/P3-05_MainProcess-CardHost_IPC消息契约与同步语义分析_v0.1.md`：协议无关的 IPC 消息契约、版本一致性、幂等、访问控制边界和跨聚合补偿候选（DRAFT / 非设计基线）。
- Q-152 / CLQ-153 已 CLOSED / CONFIRMED；ADR-035 确认逻辑写入权划分。
- P3 继续 `IN_PROGRESS`；P4 `NOT_STARTED`；概要设计基线未建立。


## 2026-11-20 P3-05 用户选择后检查
- `04_Reviews/项目一致性检查报告-Q152用户选择与P3-05推进V1.0.md`：Q-152 用户选择同步、ADR-035、P3-04 v0.2、P3-05 v0.1 与基线保护的一致性检查；WARNING（PASS 8 / WARNING 2 / FAIL 0 / NOT_CHECKED 2）。


## 2026-11-21 P3-05 IPC 传输候选分析
- `02_Architecture/P3-05_MainProcess-CardHost_IPC消息契约与同步语义分析_v0.2.md`：新增 IPC 传输候选比较及官方安全/兼容性核查（DRAFT，非设计基线）。
- `00_Project_Management/Clarifications/CLQ-154.md`：Q-153 IPC 传输技术选择（D2 / OPEN / PENDING_CONFIRMATION），推荐 Named Pipes。
- 用户确认前不新增 ADR、不将 Named Pipes 作为已选技术；P3-06 生命周期/崩溃恢复分析继续推进。


## 2026-11-21 P3-06 生命周期与崩溃恢复分析
- `02_Architecture/P3-06_CardHost生命周期与崩溃恢复策略分析_v0.1.md`：CardHost 正常生命周期、崩溃恢复、Supervisor 候选、重启节流、残留进程及验证矩阵（DRAFT，非设计基线）。
- REC-01/REC-02 为 D2 待决策；不因分析草案而默认新增 Supervisor 或承诺自动恢复 SLA。


## 2026-11-21 P3 更新
- Q-153 / CLQ-154 V2.0 已确认 A：Windows Named Pipes；ADR-036 已登记。
- P3-05 v0.3 和 P3-06 v0.2 为 DRAFT / 非设计基线。
- RISK-002 已 ASSESSED；安全缓解和目标 OS PoC 未执行；RISK-001 仍 ACCEPTED。
- P3 IN_PROGRESS；P4 NOT_STARTED；REQ-BL-1.0 不变；概要设计基线未建立。


## 2026-11-21 P3-07 与 Q-154
- P3-07 v0.1 已形成 Named Pipes 安全设计与 PoC 验收计划，所有验证项仍 NEEDS_VALIDATION。
- Q-154 / CLQ-155 等待用户选择 CardHost 崩溃后的恢复策略。
- P3 IN_PROGRESS；P4 NOT_STARTED；REQ-BL-1.0 不变；概要设计基线未建立。


## 2026-11-21 Q-154 用户选择确认与 P3 推进
- Q-154 / CLQ-155 CLOSED / CONFIRMED；新增 ADR-037。
- P3-06 更新至 v0.3；恢复策略已收敛，持久化与恢复实现细节继续 P3 验证。
- P3 IN_PROGRESS；P4 NOT_STARTED；REQ-BL-1.0 不变；概要设计基线未建立。


## 2026-11-21 P3-08 需求追踪与质量属性分析
- 新增 `02_Architecture/P3-08_需求-模块-接口-验证追踪矩阵与质量属性分析_v0.1.md`；映射 FR 19、BR 12、UIR 2、NFR 4 共 37 项需求至逻辑模块、接口边界和验证工作。
- 设计追踪映射覆盖率 37/37（100%）；不代表实现或测试覆盖率。
- P3 继续 `IN_PROGRESS`；P4 `NOT_STARTED`；概要设计基线未建立。


## 2026-11-21 P3 测试环境与 ADR-038
- Q-P3-TEST-001=E（多种 Windows 环境）；Q-P3-TEST-002=尚未盘点（具体环境待盘点）。
- Q-P3-CRASH-001=A；新增 ADR-038，Main Process 存活期间有限重启 CardHost，采用退避并在多次失败后提示用户。
- P3-06 更新至 v0.4；P3 工作计划更新至 v1.0；P3 IN_PROGRESS，P4 NOT_STARTED；REQ-BL-1.0 不变，HLD 基线未建立。
- 逐题澄清记录：`Clarifications/CLQ-156.md`；PoC/实际环境验证尚未执行。

## 2026-11-21 P3 测试环境与运行期恢复策略索引更新
- 澄清记录新增 `00_Project_Management/Clarifications/CLQ-156.md`，对应 Q-P3-CRASH-001 / ADR-038。
- P3-06 更新版：`02_Architecture/P3-06_CardHost生命周期与崩溃恢复策略分析_v0.4.md`。
- CardHost 运行期恢复 PoC 方案：`02_Architecture/P3_CardHost_Runtime_Recovery_PoC_v0.1.md`。
- 测试环境盘点模板：`06_Test_Management/P3_Test_Environment_Inventory_v0.1.md`（历史）、`P3_Test_Environment_Inventory_v0.2.md`（当前）。
- 当前具体 Windows 测试环境尚未盘点；上述文件均为计划/草案，不代表 PoC 或兼容性测试已通过。


## 2026-11-21 P3 PoC 工具包
- `06_Test_Management/P3_PoC_工程骨架与执行说明_v0.1.md`（FH-TEST-POC-001）说明隔离 PoC 工具包范围和边界。
- `06_Test_Management/P3_PoC_Starter_Kit_v0.1.zip` 为隔离测试工具包压缩交付物。
- `Get-FolderHubEnvironment_v0.2.ps1` 支持身份字段脱敏；屏幕几何不等于 DPI 测量。
- 当前仅完成静态文件检查；Windows PowerShell 执行、实际环境盘点、NP/CP/桌面集成 PoC 均未完成。
- P3 仍 IN_PROGRESS，P4 NOT_STARTED；REQ-BL-1.0 不变；HLD 基线未建立。


## 2026-11-21 P3 实际 Windows 环境采集
- `06_Test_Management/P3_Test_Environment_Inventory_v0.4.md` 追加环境 `W11-RDP-001`。
- 原始采集 JSON 中 OS Caption 与 ProductName 不一致，保留原值并待独立核实；RDP/非管理员条件已记录。
- Q-P3-TEST-002 更新为 IN_PROGRESS（已盘点一台，完整环境矩阵未完成）。
- 当前仅有环境采集证据；REC-POC、Named Pipes、持久化和桌面集成测试仍未执行。

- 工具包运行前置条件复核：恢复模拟器使用 `$PSHOME\powershell.exe` 启动 worker，建议从 Windows PowerShell 5.1 启动；PowerShell 7.6.6 采集环境不能直接证明恢复模拟器可运行。
- P3 实际环境采集一致性报告：`04_Reviews/项目一致性检查报告-P3实际环境采集V1.0.md`。


## 2026-11-21 P3 REC-POC-01 执行证据
- 当前环境记录：`06_Test_Management/P3_Test_Environment_Inventory_v0.5.md`（W11-RDP-001 已由 Win32_OperatingSystem 独立确认为 Windows 11 专业版 x64）。
- REC-POC-01：`PASS_WITH_LIMITATIONS`，仅验证隔离 harness 的模拟进程生命周期；不代表真实 CardHost 健康恢复。
- 执行一致性报告：`04_Reviews/项目一致性检查报告-P3-REC-POC-01执行V1.0.md`。
- REC-POC-02～05、NP/CP PoC、桌面集成及其他目标 OS 验证仍未完成；P3 `IN_PROGRESS`、P4 `NOT_STARTED`。

## 2026-11-21 Q-P3-GATE-001 用户选择 A 与 P3 预审准备
- `00_Project_Management/Clarifications/CLQ-157.md`：Q-P3-GATE-001 用户选择 A，停止后续测试并保留未验证状态。
- `02_Architecture/P3-09_总体架构收敛与关键验证状态_v0.1.md`：总体逻辑架构、决策边界、验证状态和未收敛缺口。
- `02_Architecture/P3_概要设计工作计划_v1.2.md`：追加停止测试与预审准备安排。
- `04_Reviews/P3_阶段评审准备包_v0.1_DRAFT.md`：用户预审稿，当前 `BLOCKED_FOR_APPROVAL`，尚未正式提交评审。
- `04_Reviews/项目一致性检查报告-P3-GATE-A_V1.0.md`：本轮选择同步和阶段门禁检查。
- REC-POC-01 保持 `PASS_WITH_LIMITATIONS`；其他 PoC/桌面集成/兼容性验证保持 `NOT RUN` / `NOT_CHECKED`。
- RISK-001 保持 `ACCEPTED` 且兼容性验证义务有效；RISK-002 保持 `ASSESSED`，未接受、未关闭。
- P3 `IN_PROGRESS`；P4 `NOT_STARTED`；HLD 基线未建立。


## 2026-11-21 P3 评审提交状态
- 当前 P3 阶段：`REVIEW_PENDING`；P4：`NOT_STARTED`；HLD 基线未建立。
- 最新 P3 评审版本：`P3_阶段评审准备包_v0.2.1_REVIEW_PENDING.md`。
- 最新架构/追踪文件：P3-04 v0.3、P3-08 v0.2、P3-09 v0.2、P3 工作计划 v1.3。
- 新增澄清记录：CLQ-158（SQLite）、CLQ-159（显示器/DPI）、CLQ-160（RISK-002 受限接受）。
- 新增 ADR-039/040。RISK-002 仅接受 P4 设计阶段；安全措施未验证，开发/真实数据/运行/部署/发布仍被阻断。
- REC-POC-01 保持 `PASS_WITH_LIMITATIONS`；其余 PoC/实机验证保持 `NOT RUN` / `NOT_CHECKED`；Git Commit/Tag 未检查。

# Project Board

- 工作流规范：已退出 software-predev-workflow-skill V2.6.1 阶段门禁；历史 P0～P3 文档与 REQ-BL-1.0 仍作输入保留。
- 执行模式：工程实现（见 `docs/05_Development/开发计划_v0.1.md`）

## 1. 项目目标
FolderHub 是一款 Windows 文件夹快速访问工具，通过桌面主组件和独立文件夹卡片帮助用户分类管理并快速打开常用文件夹。需求细节以 RAW 台账及用户确认的 ADR 为准。

## 2. 当前状态
- 当前阶段：`ENGINEERING_IMPLEMENTATION`（已退出原 P3/P4 工作流门禁）。
- 需求输入：REQ-BL-1.0（SRS v0.11）；交互参考：`p3-demo-v0.1`；架构约束：ADR-034～ADR-040。
- 编码进度：WP0～WP5 DONE（托盘 + HKCU 开机启动 + 单实例；Domain 14 + Infrastructure 14 测通过）。
- 技术假定：WPF + .NET 8；Main + CardHost 双进程；SQLite；Named Pipes。
- 历史快照：P1/P2 曾 APPROVED；P3 曾 REVIEW_PENDING / HLD 未建立；该路径不再作为推进条件。
- RISK-001 兼容性验证义务仍有效；RISK-002 安全缓解未验证——本机开发允许，真实用户数据/部署/发布前仍须完成安全验证。

## 3. 关键决策
ADR-001～ADR-040 的详细内容以 `docs/00_Project_Management/Decision_Log.md` 为准。ADR-037 仅适用于 Main Process 已退出且 Keep Cards=ON；ADR-038 仅适用于 Main Process 仍运行期间。

## 4. 待办事项
- 按 `docs/05_Development/开发计划_v0.1.md` 继续 WP6（IPC 契约 v0；WP9 spike 可并行）。
- TASK-017/TASK-018：因退出原流程，状态改为 CANCELLED（设计文档保留作参考）。
- TASK-020：工程实现按开发计划推进（活跃；WP0～WP5 已完成）。

## 5. 风险
- RISK-001：Windows Server 2012+ GUI/Desktop Experience 兼容性尚未逐版本验证；用户接受的剩余风险及验证义务不变。
- RISK-002：Named Pipes 安全缓解未验证；允许本机开发库调试；禁止未验证前对外部署/发布及接入真实生产用户数据。

## 6. 下一步
- 下一步行动：实现 WP6 IPC 契约 v0（Named Pipes 安全骨架）。
- 依据/关联：REQ-BL-1.0；ADR-034～040；`开发计划_v0.1.md`；`src/FolderHub.sln`。

维护规则：本文件仅为管理摘要，不是阶段/任务/决策/风险权威台账。

## 2026-11-21 WP5 完成并提交
- 用户审核通过（含修拖窗不写启动项、托盘 Dispatcher、最小化进托盘与单实例）；文档已同步；进入 WP6。

## 2026-11-21 WP4 完成并提交
- 用户审核通过（含修复同路径编辑误清路径异常标记）；文档已同步；进入 WP5。

## 2026-11-21 WP3 完成并提交
- 用户审核通过（含修复添加已有路径多归属覆盖与 AlreadyLinked 提示）；文档已同步；进入 WP4。

## 2026-11-21 WP2 完成并提交
- 用户审核通过（含修复 CardDefinition ReplaceAll 级联展示态问题）；文档已同步；进入 WP3。

## 7. 2026-11-12 Q-151 同步
- Q-151 / CLQ-151：CLOSED / CONFIRMED；新增 ADR-031。
- 批量重试单条失败时保留待清理状态和失败原因，跳过并继续其他记录；中断任务优先续行；失败项后续按既定自动重试规则处理。
- SRS 草案补充至 v0.7，仍未批准；需求基线 NOT_ESTABLISHED。
- P2 继续 IN_PROGRESS；P3/P4 NOT_STARTED。TASK-011/012 IN_PROGRESS；TASK-013 TODO。


## 7. 2026-11-14 历史澄清同步
- Q-029～Q-150 已按可恢复的历史对话摘要补入权威资料库；CLQ-025～CLQ-146 逐题记录标记 RECONSTRUCTED。
- SRS 当前草案更新为 v0.8；v0.7 保留历史版本。
- `docs/README.md` 已与 Project_Status 阶段状态对齐。
- 原始问题全文、候选选项和逐字回答未恢复的内容明确标记 UNAVAILABLE；不得声称完整逐字历史恢复。

## V2.6.1 工作流适配摘要
- 决策分级：D1/D2/D3；D3 仅用于低风险、可逆且可可靠推导的普通细节，不创建 Q/CLQ。
- 实际提出的 D1/D2 继续保留 V2.2 完整澄清历史；CR 涉及实质性用户决策时执行相同分级。
- 分别记录记录完整性、决策完成度、阶段阻塞性、延期有效性；延期不等于接受或批准。
- RAW/Q/ADR/CR/阶段状态保留各自状态枚举；本次不回写历史分类，不改变任何已确认需求或阶段/基线状态。


## V2.6.1 实例修复状态（当前）
- 模板映射和台账字段规范已修正；ADR-032 历史状态字段已拆分，122 项历史证据等级仍为 RECONSTRUCTED。
- Q-026：D2 / CLOSED / CONFIRMED；用户选择 A；四维状态 COMPLETE / CONFIRMED / NON_BLOCKING / NOT_DEFERRED。用户可见行为已纳入 ADR-033 / FR-019；实现算法留待 P3/P4。
- Q-015：D2 / DEFERRED；有效延期至 P3 架构方案评审/批准前，不阻断 P2。
- CR-001：D1 / CONFIRMED；CR APPROVED 状态不变；RISK-001 兼容性验证义务保留。
- SRS v0.11：AI_DERIVED 已登记；DESIGN_ASSUMPTION NONE_ADOPTED；仍为 DRAFT，未建立需求基线；v0.9 保留历史。
- P2 保持 IN_PROGRESS；P2 阶段评审准备包 v0.2 为 DRAFT / PENDING_FINAL_QUALITY_REVIEW，仍须完成追踪矩阵与质量检查并由用户明确批准。


## 8. 2026-11-15 Q-026 用户确认同步
- 用户原始回答（VERBATIM）：`FolderHub Q-026 我的选择：A。请按 V2.6.1 追加澄清历史并更新相关台账及 SRS。`
- Q-026 / CLQ-024：D2，用户选择 A；Q 状态 CLOSED，确认状态/决策处理状态 CONFIRMED；四维 COMPLETE / CONFIRMED / NON_BLOCKING / NOT_DEFERRED。
- 新增 ADR-033；当前 SRS 草案 v0.10 新增 FR-019，更新 NFR-002 和 RAW-011/RAW-013 追踪。具体实现算法留待 P3/P4。
- P2 仍 IN_PROGRESS，需求基线 NOT_ESTABLISHED；当前下一步是最终追踪矩阵与质量检查，不代表 P2 已批准。


## 9. 2026-11-16 P2 最终收尾
- SRS 当前草案：v0.11；RAW→FR/BR/UIR/NFR 追踪矩阵已补齐。
- P2 质量检查报告 v1.0：WARNING / READY_FOR_USER_REVIEW_WITH_NOTES。
- P2 阶段评审准备包：v0.3 DRAFT；已提交用户评审。
- P2 状态：REVIEW_PENDING；需求基线 NOT_ESTABLISHED；P3/P4 NOT_STARTED。
- 本轮未修改 RAW 原文、用户确认 ADR 语义、CR-001、RISK-001；未代用户批准 SRS/P2。


## 10. 2026-11-20 P2 批准与 P3 启动
- 用户明确批准 SRS v0.11，建立需求基线 `REQ-BL-1.0`，并授权进入 P3。
- P2：APPROVED；P3：IN_PROGRESS；P4：NOT_STARTED。
- 需求语义、RAW 原文及既有 ADR 未修改；Q-015 仍 OPEN / D2，进入 P3 活动分析。


## 11. 2026-11-20 P3 Demo 制作启动
- 用户要求先交付完整项目包，再按 REQ-BL-1.0 开始制作 P3 交互式网页 Demo。
- `docs/design-demo/` 已建立 `p3-demo-v0.1` 初版；状态 `P3_DEMO_DRAFT`，仅为概要设计评审辅助物。
- Demo 覆盖桌面主组件、分类/文件夹列表、搜索、记录表单、托盘模拟、设置和独立卡片交互。
- Q-015 宿主生命周期技术决策仍未确定；Demo 仅模拟用户可见行为，不据此选择进程/服务方案。
- TASK-019 IN_PROGRESS；P3 IN_PROGRESS；P4 NOT_STARTED。


## 2026-11-20 P3 Demo 用户评审通过
- 用户明确评审通过当前 `p3-demo-v0.1`；评审记录：`docs/04_Reviews/P3_Demo_v0.1_用户评审通过记录.md`。
- TASK-019 已完成并归档；Demo 仍是 `P3_DEMO_DRAFT`，不等于 P3 概要设计整体批准。
- P3-01 系统上下文与边界分析草案已形成；TASK-017、TASK-018 继续进行。
- 需求基线 REQ-BL-1.0 不变；Q-015 仍 OPEN / D2 / DEFERRED；P3 IN_PROGRESS、P4 NOT_STARTED。

- Q-015 / CLQ-152：用户已明确选择 A；Q-015 CLOSED / CONFIRMED，ADR-034 已登记；未决实现细节仍由 TASK-017/TASK-018 跟踪。


## 2026-11-20 Q-152 用户选择确认
- 用户原始回答：`FolderHub Q-152 / CLQ-153 我的选择：A。请按 software-predev-workflow-skill V2.6.1 追加用户选择记录，更新相关台账并继续 P3。`
- Q-152 / CLQ-153：CLOSED / CONFIRMED；选择 A，按逻辑聚合划分单一写入者。
- 新增 ADR-035；P3-04 更新为 v0.2；P3-05 IPC 契约与同步语义分析继续推进。
- P3 仍 IN_PROGRESS，P4 NOT_STARTED；未批准完整 P3，未建立概要设计基线。


## 2026-11-21 P3-05 IPC 传输候选分析
- P3-05 更新至 v0.2，比较 Named Pipes、Local RPC、TCP loopback、Shared Memory；推荐 Named Pipes，尚待 Q-153 用户确认。
- Q-153 / CLQ-154：D2 / OPEN / PENDING_CONFIRMATION；不新增 ADR，不将推荐方案视为已批准技术。
- P3-06 生命周期/崩溃恢复可独立继续；P3 IN_PROGRESS、P4 NOT_STARTED；需求基线不变，设计基线未建立。

- P3-06 v0.1 已形成生命周期状态机与 R1-R4 恢复策略比较；REC-01/02 D2 待决策，未默认新增 Supervisor。


## 2026-11-21 Q-153 用户选择确认
- Q-153 / CLQ-154：用户选择 A，CLOSED / D2 / CONFIRMED；新增 ADR-036，确认 Windows Named Pipes。
- P3-05 v0.3、P3-06 v0.2 已形成；RISK-002 完成定性评估，状态 ASSESSED。
- P3 继续 IN_PROGRESS；P4 NOT_STARTED；概要设计基线未建立；安全 PoC/Windows 实机验证仍待执行。


## 2026-11-21 P3-07 Named Pipes 安全设计与 Q-154
- P3-07 v0.1 已形成：Named Pipes 威胁模型、端点 DACL、Logon SID/Session 隔离、远程拒绝、身份握手和 NP-01～NP-11 PoC 验收矩阵。
- Q-154 / CLQ-155：D2 / OPEN / PENDING_CONFIRMATION；推荐 A：不增加常驻 Supervisor，下次 Main Process 启动时恢复；等待用户选择。
- Q-153 / ADR-036 已确认 Named Pipes；RISK-002 ASSESSED，缓解与 PoC 未执行；P3 IN_PROGRESS、P4 NOT_STARTED。


## 8. 2026-11-21 Q-154 用户选择确认
- 用户选择 A：不增加常驻 Supervisor/Windows Service；Main Process 已退出且 Keep=ON 时 CardHost 崩溃，卡片暂时不显示，下次 Main Process 启动时恢复最后成功持久化状态。
- Q-154 / CLQ-155 CLOSED / CONFIRMED；新增 ADR-037；P3-06 v0.3。
- 不承诺自动重启 SLA、零丢失窗口；持久化原子性、恢复对账和异常诊断仍待 P3 验证。
- P3 IN_PROGRESS；P4 NOT_STARTED；REQ-BL-1.0 不变；概要设计基线未建立。


## 2026-11-21 P3-08 追踪矩阵进展
- 新增 P3-08 v0.1，37/37 正式需求已映射至逻辑模块、接口边界和验证工作；该数字仅代表文档追踪映射，不代表实现/测试覆盖。
- 当前质量结论 WARNING：Named Pipes 安全、CardHost 持久化/恢复、桌面层级/DPI 和目标 OS 兼容性仍待验证。
- TASK-017/TASK-018 继续 IN_PROGRESS；P3 IN_PROGRESS；P4 NOT_STARTED；REQ-BL-1.0 不变，HLD 基线未建立。


## 2026-11-21 P3 测试环境与 CardHost 运行期恢复策略确认
- Q-P3-TEST-001=E：采用多种 Windows 环境验证策略；Q-P3-TEST-002=IN_PROGRESS：已登记一台环境 W11-RDP-001（RDP/非管理员），OS Caption 与 ProductName 冲突待核实，其他目标 OS 仍未盘点。
- Q-P3-CRASH-001=A：Main Process 存活期间负责检测 CardHost 崩溃并执行有限自动重启/退避，多次失败后提示用户；新增 ADR-038。
- ADR-038 只覆盖 Main Process 存活期间；Main Process 已退出且 Keep Cards=ON 时继续遵守 ADR-037，不新增常驻 Supervisor/Windows Service。
- P3-06 更新至 v0.4；P3 计划更新至 v1.0；具体重试阈值/退避参数仍待 PoC。
- TASK-017/TASK-018 继续 IN_PROGRESS；P3 IN_PROGRESS、P4 NOT_STARTED；REQ-BL-1.0 不变；HLD 基线未建立。


## 2026-11-21 P3 实际 Windows 环境采集
- 收到 `environment-inventory.json`，登记环境 `W11-RDP-001`：x64、RDP SessionId=1、非管理员、PowerShell 7.6.6；原始 OS 字段 Caption=Windows 11 专业版、ProductName=Windows 10 Pro，Build 26300.9974 / 26H2。
- OS 字段冲突待独立核实；DPI 未测量。不能据此确认正式 OS 身份或兼容性通过。
- 环境盘点更新至 v0.4；Q-P3-TEST-002 为 IN_PROGRESS；下一步先核实 OS，再运行隔离 REC-POC-01～05。
- P3 IN_PROGRESS；P4 NOT_STARTED；REQ-BL-1.0 不变；HLD 基线未建立；TASK-017/TASK-018 继续 IN_PROGRESS。

- PoC 执行注意：当前环境采集运行于 PowerShell 7.6.6；恢复模拟器当前依赖 Windows PowerShell 5.1 (`powershell.exe`)，需先确认该宿主。

## 2026-11-21 P3 执行证据追加
- W11-RDP-001：Win32_OperatingSystem 独立证据确认 Windows 11 专业版 x64，Build 26300。
- REC-POC-01：`PASS_WITH_LIMITATIONS`；模拟 worker 首次异常退出，1 秒退避后第二次启动，5 秒进程存活观察成功，后清理。不是实际 CardHost 健康证明。
- 详情见 `P3_Test_Environment_Inventory_v0.5.md`；REC-POC-02～05、NP/CP PoC、桌面集成和其他目标 OS 验证仍未完成。

## 2026-11-21 Q-P3-GATE-001 用户选择 A
- Q-P3-GATE-001 / CLQ-157：D2 / CLOSED / CONFIRMED；停止后续 PoC/实机测试，保留未验证项，完成 P3 预审准备。
- 新增 P3-09 总体架构收敛草案、P3 阶段评审准备包 v0.1_DRAFT 和 P3 工作计划 v1.2。
- 预审结论 `BLOCKED_FOR_APPROVAL`：GAP-01/GAP-06 未收敛，RISK-002 未接受且缓解未验证，持久化/恢复关键路径缺少故障注入证据。
- P3 `IN_PROGRESS`；P4 `NOT_STARTED`；HLD 基线未建立；不得未经用户明确批准进入 P4。


## 2026-11-21 P3 集中澄清与评审提交
- Q-P3-DATA-001 / CLQ-158=A：SQLite 本地数据库，新增 ADR-039；GAP-01 决策选择关闭。
- Q-P3-DISPLAY-001 / CLQ-159=A：显示器身份 + 工作区归一化坐标 + DPI 换算，新增 ADR-040；GAP-06 策略选择关闭。
- Q-P3-RISK-001 / CLQ-160=A：RISK-002 受限接受，仅限 P4 详细设计；安全验证前禁止开发/真实数据/产品运行/部署/发布。
- P3-04 v0.3、P3-08 v0.2、P3-09 v0.2、P3 Review v0.2.1、P3 工作计划 v1.3 已形成。
- P3 `REVIEW_PENDING`，等待用户明确批准或退回；P4 `NOT_STARTED`；REQ-BL-1.0 不变；HLD 基线未建立。


## 2026-11-21 退出原工作流并开始工程实现
- 用户明确：不再按 software-predev-workflow 的 P3/P4 门禁推进；基于现有需求、ADR、Demo 制定开发计划并开始编码。
- 已交付：`docs/05_Development/开发计划_v0.1.md`；`src/FolderHub.sln`（WP0）；Domain `FolderCatalog` + 8 项单测（WP1）。
- 阶段摘要改为 `ENGINEERING_IMPLEMENTATION`；不建立 HLD/P4 基线；历史 docs 保留作输入。

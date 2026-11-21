# FolderHub P3-06：CardHost 生命周期与崩溃恢复策略分析

- 文档编号：FH-HLD-REC-006
- 版本：v0.4（追加 Q-P3-CRASH-001 / ADR-038 运行期恢复策略）
- 日期：2026-11-21
- 工作流规范：software-predev-workflow-skill V2.6.1
- 阶段：P3_HIGH_LEVEL_DESIGN
- 状态：DRAFT / RECOVERY_STRATEGY_ANALYSIS（非设计基线）
- 输入需求基线：REQ-BL-1.0（SRS v0.11）
- 关联决策：ADR-021、ADR-033、ADR-034、ADR-035、ADR-036、ADR-037、ADR-038
- 关联问题：Q-015/CLQ-152（已确认）；Q-152/CLQ-153（已确认）；Q-153/CLQ-154（CLOSED / CONFIRMED；ADR-036 Windows Named Pipes）；Q-154/CLQ-155（CLOSED / CONFIRMED；ADR-037 不增加常驻监督进程）；Q-P3-CRASH-001/CLQ-156（CONFIRMED；ADR-038 Main Process 存活期间有限重启）
- 关联任务：TASK-017、TASK-018
- 关联风险：RISK-001（ACCEPTED）、RISK-002（ASSESSED）

> 本文分析 CardHost 的正常生命周期、异常终止、重启恢复和残留进程处理。对于 Main Process 已退出且 Keep=ON 时 CardHost 崩溃的场景，用户已通过 ADR-037 选择不增加常驻 Supervisor，改由下次 Main Process 启动时恢复；其他实现细节仍是设计候选/验证项，本文不建立设计基线。

## 1. 目标与边界

在不改变已批准产品行为的前提下，明确 Main Process、CardHost 在启动、正常运行、主程序退出、CardHost 崩溃、会话注销、系统关机、Explorer 重启和升级时的生命周期责任；比较恢复机制候选；定义故障检测、重启节流、状态恢复和验证边界。

Q-154 / ADR-037 已确认：Main Process 已退出且 Keep=ON 时 CardHost 崩溃，不增加常驻 Supervisor/Windows Service；卡片暂时不显示，下次 Main Process 启动时恢复最后成功持久化状态。仍不在本轮决定：恢复 SLA、具体进程启动 API、数据库、运行时或安装升级技术。IPC 传输已由 ADR-036 选择 Named Pipes，但其安全实现/PoC 仍待验证。

## 2. 已确认约束（不得改变）

1. ADR-034：每个交互式用户会话一个 CardHost，承载该会话全部独立卡片；Main Process 可退出。
2. ADR-021 / FR-017 / BR-012：全局“主程序退出后保留卡片”开关默认 ON；ON 时 Main Process 退出后卡片继续显示；OFF 时关闭全部卡片并退出 CardHost；无逐卡片覆盖。
3. ADR-035：Main Process 唯一写入 CardDefinition 和全局策略；CardHost 唯一写入该会话的 CardPresentationState。
4. NFR-002：卡片位置/锁定状态跨应用重启保存；CardHost 的展示状态保存不能依赖已经退出的 Main Process。
5. ADR-033 / FR-019：显示器/DPI/分辨率变化时尽量恢复原显示器与相对位置；不可恢复时迁移至当前主显示器可见区域。
6. Windows Service / Session 0 不得直接承载用户可见卡片窗口。

## 3. 进程生命周期状态模型（逻辑候选）

### 3.1 Main Process 状态

`NotRunning → Starting → Connected → Managing → Detaching/Stopping → Exited`

异常分支：`Connected/Managing → HostLost → Recovering → Connected`；若恢复失败则 `RecoveryLimited`，等待退避窗口或用户下一次启动。

### 3.2 CardHost 状态

`Absent → Starting → Ready → HostingCards → DetachedHosting → Stopping → Exited`

异常分支：`Starting/Ready/HostingCards/DetachedHosting → Crashed`；恢复由选定的恢复责任方触发，CardHost 重启后读取自身持久化的 CardPresentationState，再接收 Main Process 的 CardDefinition 对账。

### 3.3 状态机不变量

- 同一交互式用户会话最多存在一个有效 CardHost 主实例；重复启动请求必须收敛到同一实例或明确拒绝。
- Main Process 退出且 KeepCardsAfterMainExit=ON 时，不得向 CardHost 发送最终关闭命令；仅完成受控 Detach/Goodbye。
- KeepCardsAfterMainExit=OFF 时，关闭流程必须先停止接受新卡片命令，再关闭卡片、保存状态、回报完成，最后退出 CardHost。
- CardHost 重启只恢复其持久化的 CardPresentationState；CardDefinition 由 Main Process 的权威数据源在 Main Process 可用时重新对账。
- 任何恢复流程都不得用旧快照覆盖 CardHost 已保存的新展示状态；遵循 ADR-035 的单一写入者。
- 会话注销/系统关机时，操作系统会话结束是生命周期边界；必须尽力保存状态，但不得承诺超出可验证能力的无损保证。

## 4. 关键生命周期流程

### 4.1 用户登录 / Main Process 启动

1. Main Process 确认当前交互式用户 SID、SessionId 和安装版本。
2. 查找当前会话 CardHost；核验实例身份、协议兼容性和运行状态。
3. 若存在健康实例，建立连接并发送当前 Main Process 权威的 CardDefinition / GlobalSettings 版本。
4. 若不存在且有启用卡片，启动 CardHost；完成握手后按稳定 CardId 创建/恢复卡片。
5. CardHost 对本地 PresentationState 进行恢复，应用显示器/DPI 可见性修正，再返回 Ready/HostHealth。
6. 重复启动、并发启动或旧实例残留必须通过会话级单实例锁/实例令牌和进程核验处理；具体 API 待 P4。

### 4.2 Main Process 正常退出，KeepCardsAfterMainExit=ON

1. Main Process 停止接受会创建新卡片的 UI 命令。
2. 发送 Detach/KeepAlivePolicy（消息名为协议候选），等待 CardHost 确认当前定义/策略版本已接收。
3. Main Process 释放 IPC 客户端资源并退出；不终止 CardHost。
4. CardHost 独立继续显示卡片，并直接保存 CardPresentationState。
5. 后续 Main Process 再启动时重新发现 CardHost，执行定义和策略对账。

### 4.3 Main Process 正常退出，KeepCardsAfterMainExit=OFF

1. Main Process 停止接受新命令，向 CardHost 发送受控 ShutdownRequest。
2. CardHost 拒绝新的卡片创建/移动命令，完成最后一次展示状态保存。
3. CardHost 关闭所有卡片并返回 ShutdownComplete；Main Process 在超时窗口内等待。
4. 正常完成后 CardHost 退出；若超时或进程无响应，进入有记录的强制终止候选路径，需避免把未确认保存误报为成功。
5. Main Process 退出；不得删除用户真实文件夹或 FolderRecord 业务数据。

### 4.4 CardHost 在 Main Process 仍运行时崩溃

1. Main Process 通过进程退出通知/健康检查识别 HostLost。
2. 冻结发往旧 hostInstanceId 的未完成命令；对结果未知的命令先查询/对账，不盲目重复有副作用命令。
3. 按恢复策略执行重启尝试；新实例生成新的 hostInstanceId。
4. CardHost 读取其持久化展示状态；Main Process 重新发送权威 CardDefinition/GlobalSettings 快照或增量。
5. 对账后只重建启用卡片；不存在的/已禁用卡片按 Main Process 权威定义关闭。
6. 连续崩溃达到节流阈值后停止快速重启，记录诊断并等待退避窗口/下一次用户操作。

### 4.5 CardHost 在 Main Process 已退出时崩溃

此场景是恢复机制选择的关键差异：Main Process 已不存在，不能由其监控和重启 CardHost。若要求自动恢复卡片显示，必须有独立于 Main Process 的恢复责任方；若不增加该责任方，只能在 Main Process 下次启动时恢复。

### 4.6 注销、关机、RDP 与 Explorer

- 用户注销：会话结束，CardHost 应响应退出/关机通知，尽力保存状态并释放窗口/IPC/文件句柄；下一次登录按持久化状态恢复。
- 系统关机/重启：与注销类似，保存时间受系统关机时限约束；不承诺超时或断电情况下零数据丢失。
- RDP 断开：不得将断开直接等同于注销；CardHost 可继续留在该会话中，重连后重新校验显示器和 DPI 环境。具体行为需在目标 Server 环境实测。
- Explorer 重启：不应直接导致 CardHost 业务进程退出；桌面层窗口可能需要重新附着/重建，属于 Windows Desktop Adapter 的恢复责任，需单独验证。

## 5. CardHost 崩溃后的恢复策略候选

| 方案 | 描述 | 优点 | 缺点/代价/风险 | 当前判断 |
|---|---|---|---|---|
| R1. 下次 Main Process 启动时恢复 | 不新增常驻恢复进程；CardHost 崩溃后保持缺席，等用户下次启动主程序再恢复卡片 | 最简单；进程数少；部署/升级成本低 | Main Process 退出期间若 CardHost 崩溃，卡片会持续消失直到用户再次启动主程序；不能保证独立卡片自动恢复 | **用户已选择（ADR-037）；局部决策已确认，实施仍需验证** |
| R2. 每会话轻量 Supervisor | 登录/主程序启动时启动独立 Supervisor；Supervisor 在 Main Process 退出后仍运行，监测 CardHost 并按策略重启 | 可在 Main Process 退出后恢复卡片；不需要 Session 0 服务；恢复责任明确 | 增加常驻进程、启动/退出/升级/互斥/权限/崩溃恢复复杂度；Supervisor 自身故障仍需处理；资源占用待测 | **用户未选择；在 ADR-037 覆盖场景中不采用** |
| R3. Windows Service + 会话内 GUI CardHost | Service 管理会话与恢复，用户会话内仍运行 CardHost | 可集中管理多个会话和生命周期 | 需要服务安装/权限/升级；Session 0 与用户会话代理协调复杂；与当前单用户交互桌面范围相比过重 | **用户未选择；在 ADR-037 覆盖场景中不采用** |
| R4. 仅由 Main Process 监控并重启 | Main Process 存活期间监控 CardHost；主程序退出后不提供恢复 | 不增加第三进程；主程序存活时恢复简单 | 与 R1 在主程序退出期间的恢复能力相同；不能解决 4.5 场景 | 可作为 R1 的运行期补充，不是独立方案 |

### 5.1 当前决策状态与后续边界

- 用户已通过 Q-154 / ADR-037 选择 R1：Main Process 已退出且 Keep=ON 时 CardHost 崩溃，不增加常驻 Supervisor/Windows Service；卡片暂时不显示，下次 Main Process 启动时恢复最后成功持久化状态。
- R2/R3 不纳入当前已确认方案；若未来用户提出崩溃后自动恢复或服务级管理要求，必须先评估需求/架构影响并按 CR/Q/CLQ/ADR 流程重新确认。
- R4 仅描述 Main Process 仍运行时的可能处理方式；该子场景不属于 Q-154 的确认范围，具体是否重启、重试与熔断策略仍是 P3 设计/验证项，不得误读为 ADR-037 已批准。
- 不承诺自动恢复 SLA、零丢失窗口或未经验证的目标 OS 行为。

## 6. 重启节流、残留进程和并发启动约束（设计候选）

- 会话级单实例：同一 SessionId 只能有一个有效 CardHost；当前方案不引入常驻 Supervisor。
- 实例识别：使用 hostInstanceId、PID、SessionId、User SID、启动时间和协议版本联合识别；PID 可复用，不能单独作为实例身份。
- 重启退避：失败后采用有上限的指数退避/固定窗口节流；阈值、间隔和自动停止策略待性能/稳定性验证，不在本轮编造数值 SLA。
- 残留进程：启动时先验证进程身份/会话/版本和 IPC 握手；不可识别的同名进程不得直接终止；仅对确认属于 FolderHub 且属于当前会话的旧实例执行受控清理。
- 命令处理：旧实例未确认的命令标记 OutcomeUnknown，重启后通过状态对账确认，不盲目重放非幂等操作。
- 诊断：记录 hostInstanceId、sessionId（脱敏/最小化）、退出码、崩溃时间、重启次数、最近成功保存版本、恢复结果；避免记录完整敏感路径。
- 监督进程：ADR-037 当前不引入 Supervisor；如未来需求变更重新提出自动恢复，应通过 CR/Q/CLQ/ADR 评估其常驻进程、故障恢复和升级回退成本。

## 7. 持久化与恢复一致性

- CardHost 崩溃恢复时以 CardHost 最后成功持久化的 CardPresentationState 为恢复输入；未完成写入的最新拖动/锁定状态可能丢失，需通过保存频率/原子写/日志策略和 PoC 确定边界。
- Main Process 重启时以其权威持久化的 CardDefinition/GlobalSettings 为准；CardHost 不得独立恢复已被禁用或删除的业务定义。
- 若 CardHost 在 Main Process 离线期间保存新位置，Main Process 重启不得将旧 PresentationState 快照回写覆盖。
- 文件夹删除与 CardHost 断连时，业务定义删除/卡片关闭应有显式 PendingReconciliation/补偿状态；不得误删磁盘真实目录。
- 物理存储、写入原子性、备份/迁移和损坏恢复仍待 P3/P4 进一步设计。

## 8. 故障场景与验证矩阵

| 场景 | 预期处理原则 | 验证方式 | 状态 |
|---|---|---|---|
| Main Process 正常退出，Keep=ON | CardHost 留存并继续保存卡片状态 | 退出主进程后拖动/锁定卡片并重启主程序核对 | NEEDS_VALIDATION |
| Main Process 正常退出，Keep=OFF | CardHost 关闭卡片、保存并退出 | 注入正常/超时/无响应关闭 | NEEDS_VALIDATION |
| CardHost 在 Main Process 存活时崩溃 | 检测 HostLost，恢复或限流，状态对账 | 强制终止 CardHost，检查无重复卡片/无旧状态覆盖 | NEEDS_VALIDATION |
| CardHost 在 Main Process 已退出时崩溃 | 按 ADR-037 不自动拉起；卡片暂时不显示，下次 Main Process 启动时恢复最后成功持久化状态 | 主程序退出后终止 CardHost；确认无常驻监督进程/自动重启；下次启动恢复卡片并核对状态 | NEEDS_VALIDATION |
| 连续崩溃/启动失败 | 退避、熔断/停止快速重启、诊断可查 | 故意配置启动失败并连续触发 | NEEDS_VALIDATION |
| 双击/并发启动 Main Process | 会话单实例收敛 | 并发启动压力测试 | NEEDS_VALIDATION |
| 同会话残留旧 CardHost | 验证身份/版本后受控处理，不误杀未知进程 | 制造旧版本/假同名进程 | NEEDS_VALIDATION |
| 用户注销/系统关机 | 尽力保存并释放资源；不承诺断电零丢失 | 注销、重启、关机时序测试 | NEEDS_VALIDATION |
| RDP 断开/重连 | 会话不误判注销；重新校验显示环境 | Server 2012/2012R2/2016+ RDP 测试 | NEEDS_VALIDATION |
| Explorer 重启 | CardHost 业务进程存活，桌面窗口恢复 | 重启 Explorer，检查桌面层窗口 | NEEDS_VALIDATION |
| IPC 连接断开/半包/超时 | 丢弃不完整帧、结果未知时对账 | 注入断连/延迟/半包/乱序 | NEEDS_VALIDATION |
| 升级中 Main/CardHost 版本混用 | 阻止不兼容进程混连；受控退出/替换/回退 | 新旧版本并存和升级中断测试 | NEEDS_VALIDATION |

## 9. 设计未知项与决策等级

| 编号 | 未知项 | 等级 | 当前状态 | 处理安排 |
|---|---|---|---|---|
| REC-01 | CardHost 在 Main Process 已退出时崩溃，是否要求自动恢复卡片 | D2 | CONFIRMED（ADR-037 / R1） | 用户已选择不增加常驻监督进程；下次 Main Process 启动时恢复；恢复实现与可见性边界按 P3 验证 |
| REC-02 | 是否引入每会话 Supervisor（R2） | D2 | CONFIRMED（ADR-037：不引入） | 用户选择 A；R2 不纳入当前方案；未来若提出自动恢复需求须走 CR/澄清流程 |
| REC-03 | 崩溃前最后一次拖动/锁定状态允许丢失的时间窗口 | D2 | NEEDS_DECISION / NEEDS_VALIDATION | 需先做持久化 PoC，不能无证据承诺零丢失 |
| REC-04 | 重启退避阈值、关闭超时和健康检查间隔 | D3 / DESIGN_ASSUMPTION | NEEDS_VALIDATION | 先用 PoC/压力测试测定；不主动向用户提问，不创建 Q/CLQ |
| REC-05 | RDP 断连/重连和 Explorer 重启的具体桌面恢复机制 | D2 | NEEDS_VALIDATION | 目标 Server 环境原型验证；若改变用户可见承诺再建立澄清问题 |

### 9.1 D2 决策闭环与剩余验证安排（P3 评审前复核）

| 事项 | 延期原因 | 影响范围 | 临时防护/候选边界 | 责任方 | 重审节点/触发条件 |
|---|---|---|---|---|---|
| REC-01/REC-02：主程序已退出时 CardHost 崩溃后的恢复责任与 Supervisor | 用户已选择 A 并确认 ADR-037；不增加常驻监督进程，下次 Main Process 启动时恢复 | 用户决策已闭环，不再延期；仅保留实现和验证任务 | 不在 Main Process 已退出期间自动拉起 CardHost；CardHost 自主持久化，Main Process 下次启动负责恢复和对账 | 助手/项目组（待分配） | P3 设计收敛与 PoC；概要设计评审前验证关键恢复路径 |
| REC-03：崩溃前最后一次拖动/锁定状态允许丢失的窗口 | 需先验证持久化写入/刷新频率和原子性，当前没有实测依据 | 影响卡片展示状态恢复质量和数据一致性 | 不承诺零丢失；只在隔离 PoC 中测量，不接入真实用户数据 | 助手/项目组（待分配） | P3 持久化 PoC 完成后、P3 评审前 |

REC-01/REC-02 已由用户确认，不再作为延期事项；REC-03 仍需按表中节点复核。任何技术验证未通过项须在 P3 评审材料中明确披露。

## 10. 需求与决策追踪

| 来源 | 本文设计响应 | 验证重点 |
|---|---|---|
| FR-016 / ADR-007 | 卡片定义与 CardHost 卡片窗口生命周期分离 | 启停命令幂等、恢复无重复卡片 |
| FR-017 / BR-012 / ADR-021 | 全局 Keep 开关决定 Main Process 退出时的 CardHost 留存/关闭 | Keep ON/OFF 两种退出时序 |
| FR-019 / ADR-033 | 每卡片显示位置恢复与显示器/DPI 修正 | 多显示器、DPI、RDP 变化 |
| NFR-002 / ADR-035 | CardHost 自主管理 CardPresentationState；Main Process 管理 CardDefinition | Main Process 退出期间保存、CardHost 崩溃恢复和旧快照冲突 |
| NFR-001 / RISK-001 | 目标 OS 与桌面会话差异验证 | Win10/11 与 Server 2012+ 矩阵 |
| ADR-034 | 每个交互式用户会话一个 CardHost | 多会话隔离、登录/注销和重复启动 |

## 11. 基线边界

- 本文为 DRAFT / 非设计基线；R1 已由用户选择为当前恢复策略；R2/R3 不纳入当前方案；R4 及重启节流/运行期恢复细节仍为分析候选。
- 不修改 SRS v0.11 / REQ-BL-1.0、RAW、ADR-001～ADR-037、CR-001 或 RISK-001/RISK-002。
- RISK-002 已完成 P3 定性评估，状态 ASSESSED；Named Pipes 安全 PoC 和缓解措施验证仍待执行，未被用户接受。
- Q-153 / ADR-036 已确认 Named Pipes；本文件后续故障验证需纳入管道断连、半包、超时、身份拒绝和重连对账。
- Q-154 / ADR-037 已确认 Main Process 已退出时 CardHost 崩溃后的恢复策略：不增加常驻监督进程，下次 Main Process 启动时恢复；实现细节仍需 P3 验证。
- P3 保持 `IN_PROGRESS`；P4 保持 `NOT_STARTED`；概要设计基线未建立。

## 12. 修订记录

- v0.1（2026-11-21）：形成 CardHost 生命周期、正常退出、崩溃恢复、会话边界、Supervisor 候选、重启节流、残留进程和验证矩阵；未作技术/可靠性决策，非设计基线。


## 13. IPC 传输选择后的生命周期影响更新（v0.2）

- 用户已选择 Q-153 / CLQ-154 A；ADR-036 确认 Named Pipes 为 Main Process 与同会话 CardHost 的本机 IPC 传输机制。
- CardHost 状态机和恢复策略候选不因传输选择而自动确定；REC-01/REC-02（主程序退出后 CardHost 崩溃是否自动恢复、是否引入每会话 Supervisor）仍为 D2 待决策。
- REC-03（崩溃前最后一次拖动/锁定状态允许丢失的时间窗口）仍需持久化 PoC 证据后再决定；REC-04/05 继续按验证项处理。
- 将 Named Pipes 断连、超时、半包、客户端身份验证失败、服务端崩溃、重连和协议版本不兼容纳入生命周期故障矩阵；不得把传输选择视为上述故障已解决。
- RISK-002 已由 SCREENED 更新为 ASSESSED（中概率/高影响/高等级）；安全控制未实施/验证，风险未接受。
- 本文件仍为 DRAFT / 非设计基线；不修改 SRS v0.11 / REQ-BL-1.0、RAW、CR-001、既有 ADR 或 RISK-001。

### 修订记录
- v0.2（2026-11-21）：根据 Q-153 / ADR-036 更新 IPC 传输决策状态和故障验证范围；REC-01/02 等恢复策略仍待决策，非设计基线。


## 14. Q-154 恢复策略用户确认后的设计更新（v0.3）

- 用户对 Q-154 / CLQ-155 选择 A；新增 ADR-037。Main Process 已退出且 Keep Cards=ON 时 CardHost 崩溃，不增加常驻 Supervisor/Windows Service；卡片暂时不显示，CardPresentationState 保留最后一次成功持久化状态，下次 Main Process 启动时恢复。
- REC-01/REC-02 的用户决策已收敛，不再作为 P3 用户选择阻塞项；本轮不引入自动重启或崩溃恢复 SLA。
- REC-03（持久化状态允许丢失窗口）、恢复时的 CardDefinition 对账/幂等、损坏/启动失败诊断与重试、Main Process 仍运行时 CardHost 崩溃处理、目标 Windows 版本验证继续作为 P3 技术设计/验证项。
- 验证要求：CardHost 独立持久化；崩溃后无重复窗口/残留实例；Main Process 下次启动时恢复最后成功状态；恢复对账遵守 ADR-035 单一权威写入者及版本防陈旧覆盖规则。所有未实测项保持 NEEDS_VALIDATION。
- 本文仍为 DRAFT / 非设计基线；不修改 SRS v0.11 / REQ-BL-1.0、RAW、CR-001、ADR-001～ADR-036、RISK-001/RISK-002；不批准 P3 整体，不进入 P4。

### 修订记录
- v0.3（2026-11-21）：根据 Q-154 / ADR-037 更新 CardHost 崩溃恢复策略；将用户决策与待验证实现细节分离；非设计基线。

## 15. Main Process 运行期间的有限自动恢复策略（ADR-038）

### 15.1 已确认的高层策略
根据 Q-P3-CRASH-001，Main Process 仍运行时若检测到 CardHost 异常退出，由 Main Process 自身负责监测并协调有限次数自动重启；重启采用退避；多次失败后停止自动重启并提示用户。不得无限重启，不新增独立常驻 Supervisor/Windows Service。

### 15.2 与 ADR-037 的边界
- Main Process 存活：Main Process 可承担会话内 CardHost 进程监测、有限重启和失败提示。
- Main Process 已退出且 Keep Cards=ON：严格执行 ADR-037，不留下后台监控/重启责任主体；卡片暂时不显示，下次 Main Process 启动时恢复最后成功持久化状态。
- Main Process 正常退出且 Keep Cards=OFF：继续执行 ADR-021 既有行为，关闭卡片并退出 CardHost。

### 15.3 候选状态机（DESIGN PROPOSAL，待 PoC）
`RUNNING → HOST_LOST → VERIFY_EXITED → RESTART_WAIT → STARTING → RESTORING → RUNNING`

若启动或恢复失败：
`STARTING/RESTORING → RETRY_WAIT → STARTING`（受有限重试预算约束）

达到重试预算：
`RETRY_WAIT → DEGRADED / AUTO_RESTART_STOPPED → USER_NOTIFIED`

Main Process 自身退出时立即终止监测与后续重启；不得派生能脱离 Main Process 持续运行的监督进程。具体进程树/Job Object/句柄关闭实现仍需技术验证。

### 15.4 必须维持的安全与一致性约束
- 确认旧 CardHost 已退出后才创建替代实例，避免同一会话双宿主。
- 新实例必须重新建立 Named Pipes 连接并重新执行身份/会话验证；不得复用过期连接身份。
- 恢复过程遵守 ADR-035 单一写入者边界。
- CardDefinition 与 CardPresentationState 对账必须幂等，防止重复窗口、旧状态覆盖新定义或跨会话状态混用。
- 重试达到上限后停止自动重启，记录脱敏诊断信息并向用户提示。

### 15.5 尚未确定且必须由 PoC 收敛的参数
- 最大重启次数与统计窗口；
- 初始/最大退避时长及退避函数；
- 是否引入随机抖动；
- 进程启动超时、恢复超时和健康检查判定；
- 达到上限后的用户提示、手动重试入口和冷却规则；
- Main Process 在重启等待/启动/恢复中退出时的竞态处理。

不得在参数未经评估和用户确认前填入任意固定数值。

### 15.6 验证场景
- CardHost 正常退出与异常崩溃的区分；
- 单次崩溃后恢复成功；
- 连续崩溃触发退避及重试上限；
- CardHost 启动成功但恢复失败；
- Main Process 在等待/启动/恢复期间退出；
- 旧进程未退出、句柄残留、重复实例/重复卡片；
- Named Pipes 断连、重连和重新身份认证；
- 状态对账、幂等、持久化数据损坏和恢复失败。

本节为 P3 设计分析，不代表 PoC 已执行或通过；本文仍为 DRAFT / 非设计基线。

### 修订记录
- v0.4（2026-11-21）：追加 ADR-038 用户确认的 Main Process 存活期间有限自动恢复策略；明确与 ADR-037 的进程退出边界；补充候选状态机、约束和 PoC 场景；参数仍待验证。

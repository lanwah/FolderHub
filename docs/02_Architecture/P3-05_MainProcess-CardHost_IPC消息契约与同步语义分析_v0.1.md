# FolderHub P3-05：Main Process–CardHost IPC 消息契约与同步语义分析

- 文档编号：FH-HLD-IPC-005
- 版本：v0.1
- 日期：2026-11-20
- 工作流规范：software-predev-workflow-skill V2.6.1
- 阶段：P3_HIGH_LEVEL_DESIGN
- 状态：DRAFT / IPC_CONTRACT_ANALYSIS（非设计基线）
- 输入需求基线：REQ-BL-1.0（SRS v0.11）
- 关联决策：ADR-021、ADR-034、ADR-035
- 关联问题：Q-015 / CLQ-152（CLOSED）；Q-152 / CLQ-153（CLOSED）
- 关联任务：TASK-017、TASK-018

> 本文定义协议无关的逻辑消息边界和一致性语义。未选择 Named Pipes、Local RPC、套接字、共享内存、数据库共享等具体传输或物理机制；所有候选仍需结合 Windows 目标版本验证。

## 1. 目标与范围

基于 ADR-034 的“每交互式用户会话一个 CardHost”进程模型和 ADR-035 的逻辑聚合单写者规则，定义 Main Process 与 CardHost 的通信职责、消息类别、版本校验、幂等、重连对账和跨聚合失败处理边界。

本轮不定义具体 IPC API、序列化库、数据库、物理存储布局、性能 SLA 或完整崩溃恢复实现。

## 2. 不可改变的设计约束

1. Main Process 唯一写入 Category、FolderRecord、CategoryFolderLink、MainComponentSettings、GlobalSettings 和 CardDefinition。
2. CardHost 唯一写入该会话各卡片的 CardPresentationState。
3. CardHost 在 Main Process 退出且全局保留开关开启时必须能够继续显示卡片并保存其展示状态。
4. Main Process 不得以旧的 CardPresentationState 快照覆盖 CardHost 更新后的状态。
5. 进程通信仅限当前交互式用户会话的受授权进程；不得把 Windows Service / Session 0 作为可见卡片 UI 宿主。
6. 用户确认的产品语义仍以 SRS v0.11 / REQ-BL-1.0 为准；本文不得新增或改变用户可见功能。

## 3. 进程职责与通信方向

| 方向 | 主要消息 | 发送方权威范围 | 接收方处理边界 |
|---|---|---|---|
| Main Process → CardHost | HostDiscover/Connect、CardDefinitionUpsert、CardEnable、CardDisable、CardClose、MetadataSnapshot、ShutdownRequest | Main Process 对卡片定义/启停和业务元数据的权威写入者 | CardHost 校验版本、会话和命令幂等性；更新窗口/卡片运行状态，不成为业务主数据写入者 |
| CardHost → Main Process | HostHello、HostReady、CardStateChangedNotice、HostHealth、CommandResult、HostGoodbye | CardHost 对 CardPresentationState 的权威写入者 | Main Process 可接收状态通知/健康信息，不得据此无条件回写展示状态 |
| Main Process → CardHost（重连） | ReconcileRequest、DefinitionDelta/Snapshot、KeepCardsPolicy | Main Process 仅提供自己拥有的数据和当前全局策略 | CardHost 对比 CardDefinition 版本并报告缺失/多余/冲突，不接受主程序对展示状态的陈旧覆盖 |
| CardHost → Main Process（重连） | ReconcileReport、PresentationStateSummary（只读） | CardHost 提供运行状态和只读摘要 | Main Process 记录诊断/展示摘要；CardHost 本地状态仍由 CardHost 自己管理 |

## 4. 逻辑消息信封（候选）

所有请求/响应建议使用统一 Envelope，字段为协议草案，不是最终序列化结构：

| 字段 | 用途 | 规则 |
|---|---|---|
| protocolMajor / protocolMinor | 协议版本 | Major 不兼容时拒绝或进入受控降级；Minor 向后兼容规则待细化 |
| messageId | 消息唯一标识 | 重试时保持不变，用于幂等去重 |
| correlationId | 请求/响应关联 | 响应复用请求 correlationId |
| sessionId | 交互式用户会话 | 必须与当前进程所属会话一致 |
| hostInstanceId | CardHost 实例标识 | 区分重启前后的宿主实例 |
| senderRole | 发送方角色 | 仅允许 MainProcess / CardHost 两种角色值 |
| messageType | 消息类别 | 采用显式版本化消息类型，不依赖自由文本路由 |
| aggregateId | 逻辑聚合标识 | CardDefinition 或 CardPresentationState 的稳定 ID |
| expectedVersion | 预期聚合版本 | 写命令必须进行版本条件校验；不适用的消息可为空 |
| payload | 业务载荷 | 仅包含发送方有权写入/提供的数据 |
| sentAt | 诊断时间 | 用于日志关联，不作为冲突解决的唯一依据 |

## 5. 核心命令与响应语义

| 命令 | 业务目的 | 成功响应 | 典型失败 |
|---|---|---|---|
| Connect / HostHello | 发现并认证当前会话 CardHost | HostReady（协议/实例/会话信息） | 会话不匹配、版本不兼容、权限拒绝 |
| CardDefinitionUpsert | 新增/更新卡片定义 | Applied + 新定义版本 | VersionConflict、InvalidDefinition、StorageUnavailable |
| CardEnable / CardDisable | 启用或停用卡片 | Applied / AlreadyInTargetState | NotFound、VersionConflict、HostBusy |
| CardClose | 关闭指定卡片窗口/运行实例 | Closed / AlreadyClosed | UnknownCard、CloseFailed |
| ShutdownRequest | 全局保留关闭时关闭全部卡片并退出 CardHost | ShutdownAccepted / HostGoodbye | ShutdownRejected、Timeout |
| ReconcileRequest | 主程序重启后同步其权威 CardDefinition/GlobalSettings | ReconcileReport | ProtocolMismatch、SessionMismatch、PartialFailure |
| CardPresentationStateChanged | CardHost 向主程序发送只读通知 | Ack | NotificationRejected；不影响 CardHost 本地已提交状态 |

所有可重试命令须具备幂等语义；重复接收相同 messageId 不得重复创建卡片、重复删除或产生多次状态副作用。

## 6. 版本与冲突处理

- CardDefinition 使用 Main Process 管理的单调递增聚合版本；CardHost 仅应用高于当前版本的有效更新。
- CardPresentationState 使用 CardHost 管理的独立版本；Main Process 不得以定义快照携带的旧展示状态覆盖它。
- 版本不匹配时返回 VersionConflict，并提供当前版本/冲突类别；禁止“最后写入时间较晚者获胜”作为唯一规则。
- 同一 CardId 的命令按聚合串行化；不同 CardId 可并行，但会话级 Shutdown/Reconcile 需有明确屏障。
- 时间戳只作诊断；跨进程排序以版本、消息 ID 和明确的因果关联为准。

## 7. 跨聚合操作：删除文件夹记录

FolderRecord 与 CardDefinition / CardPresentationState 属于不同权威聚合。用户删除 FolderHub 文件夹记录时，不得删除磁盘真实目录（NFR-003）。候选协调流程：

1. Main Process 将 FolderRecord 标记为待删除/待协调（仅为内部流程状态，不改变用户可见语义）。
2. Main Process 向 CardHost 发送关联 CardId 的 CardDisable/CardClose 命令。
3. CardHost 关闭窗口并返回结果；展示状态的清理由 CardHost 在其权威边界内执行或标记。
4. Main Process 收到确认后，删除自身拥有的 CardDefinition、CategoryFolderLink 和 FolderRecord 记录。
5. 若 CardHost 不可达或部分失败，Main Process 保留可恢复的待协调状态并提示/重试；不得静默遗留窗口或丢失业务记录。

该流程是设计候选；是否采用 Saga/Outbox、状态字段及补偿策略仍待后续分析。

## 8. 断连、重连与幂等

- Main Process 短暂断连不应导致 CardHost 丢失已提交的 CardPresentationState。
- Main Process 重启后先验证 sessionId、hostInstanceId、协议版本和访问权限，再发起 ReconcileRequest。
- 重连仅同步 Main Process 权威的 CardDefinition/GlobalSettings；CardHost 返回其权威的展示状态版本/摘要。
- 未确认结果的命令使用相同 messageId 重试；接收方查询已处理记录或等效幂等状态后返回原结果。
- 对于无法判定是否已提交的操作，返回 UnknownOutcome 并执行状态查询/对账，不得盲目重复有副作用的命令。

## 9. 安全与会话隔离（候选原则）

- IPC 端点应仅允许当前用户会话中经授权的 FolderHub 进程访问；验证调用者身份、会话归属和进程角色。
- 传入的路径、CardId、消息类型和载荷均需校验；不得把 IPC 客户端身份当作业务数据授权的唯一依据。
- 避免在命令行参数、日志或错误消息中泄露敏感路径/用户信息；日志记录使用必要的标识和脱敏策略。
- 同一 SID 多个交互式会话的数据隔离策略尚未确定；不可默认共享或默认隔离为已验证事实，需纳入目标环境测试。
- Named Pipe ACL、端点命名、令牌/句柄校验等是候选实现点，不是本轮已选技术。

## 10. 兼容性与协议演进

- Main Process 与 CardHost 必须使用兼容协议版本；版本不兼容时不得继续发送未知写命令。
- 安装升级期间需避免新旧版本进程混连；需设计升级握手、旧进程退出/替换和回滚策略。
- 协议字段只增不删不是充分兼容策略；Major/Minor 的兼容矩阵、可选字段默认值和未知消息处理需在 P4 明确。
- Windows Server 2012+、Windows 10 22H2、Windows 11 的 API/权限/会话行为需逐版本验证；RISK-001 的验证义务保持有效。

## 11. 设计假设与待验证项

| 编号 | 类型 | 内容 | 依据/影响 | 验证方式 |
|---|---|---|---|---|
| IPC-A01 | DESIGN_ASSUMPTION | 一个用户会话内 Main Process 与 CardHost 通过受限本地 IPC 通信 | ADR-034/035；不涉及远程网络 | 在目标 OS 建立最小连接/身份验证原型 |
| IPC-A02 | AI_DERIVED | 命令需具备 messageId 幂等和 aggregate version 条件更新 | 防止重试副作用及旧状态覆盖 | 断连、超时、重复投递和并发更新测试 |
| IPC-A03 | AI_DERIVED | 跨聚合删除采用显式待协调/补偿流程，而非跨进程假设原子事务 | 两类聚合有不同唯一写入者 | 注入 CardHost 离线、崩溃、部分成功故障 |
| IPC-A04 | DESIGN_ASSUMPTION | 物理 IPC 技术暂不选择；先用协议接口隔离传输适配器 | Windows Server 2012+ 兼容矩阵尚未验证 | 比较 Named Pipes 等候选并在最低版本验证 |

| 验证项 | 当前状态 | 关闭条件 |
|---|---|---|
| 消息重复/乱序/超时 | NEEDS_VALIDATION | 幂等及版本冲突测试通过 |
| CardHost 离线期间位置保存 | NEEDS_VALIDATION | 主应用退出后移动/锁定，CardHost 重启后恢复 |
| 主程序重启对账 | NEEDS_VALIDATION | 不丢失 CardDefinition/PresentationState，不产生重复卡片 |
| 删除文件夹与卡片关闭一致性 | NEEDS_VALIDATION | CardHost 断连/失败时可恢复，不误删磁盘目录 |
| 多会话隔离 | NEEDS_VALIDATION | 在支持环境中明确 SID/Session 行为 |
| 升级协议兼容 | NEEDS_VALIDATION | 新旧版本混连被正确拒绝或安全协商 |

## 12. 需求与决策追踪

| 需求/决策 | 本文覆盖 | 后续验证 |
|---|---|---|
| FR-016 / ADR-007 | CardDefinition 与 CardHost 显示状态边界 | 多卡片启停/关闭命令幂等 |
| FR-017 / BR-012 / ADR-021 | 全局保留开关与 ShutdownRequest | 开关开/关两种退出时序 |
| FR-019 / ADR-033 | PresentationState 的显示环境字段由 CardHost 持有 | 多显示器/DPI 恢复 |
| NFR-002 / ADR-035 | CardHost 持久化展示状态；Main Process 持久化定义 | 主应用退出期间位置/锁定持久化 |
| NFR-003 | 删除记录不删除磁盘目录 | 删除/路径异常测试 |
| ADR-034 | 每交互式用户会话一个 CardHost | 会话隔离与进程发现 |

## 13. 基线边界

- 本文不修改 SRS v0.11 / REQ-BL-1.0、RAW、ADR-021/033/034/035、CR-001 或 RISK-001。
- 本文为 DRAFT / 非设计基线；协议信封、消息类别、同步流程和恢复候选均需后续评审。
- 未选择 Named Pipes、数据库、序列化技术、加密或物理存储布局。
- P3 继续 `IN_PROGRESS`；P4 保持 `NOT_STARTED`；不得将本工作包完成视为 P3 批准。

## 14. 修订记录

- v0.1（2026-11-20）：基于 ADR-034/035、P3-03/04 建立协议无关的 IPC 消息契约、幂等/版本一致性、重连对账、跨聚合补偿和安全边界分析；非设计基线。

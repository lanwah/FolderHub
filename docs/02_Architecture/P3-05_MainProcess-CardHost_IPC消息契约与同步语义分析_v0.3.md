# FolderHub P3-05：Main Process–CardHost IPC 消息契约与同步语义分析

- 文档编号：FH-HLD-IPC-005
- 版本：v0.3
- 日期：2026-11-21
- 工作流规范：software-predev-workflow-skill V2.6.1
- 阶段：P3_HIGH_LEVEL_DESIGN
- 状态：DRAFT / IPC_TRANSPORT_SELECTED_SECURITY_VALIDATION_PENDING（非设计基线）
- 输入需求基线：REQ-BL-1.0（SRS v0.11）
- 关联决策：ADR-021、ADR-034、ADR-035、ADR-036
- 关联问题：Q-015 / CLQ-152（CLOSED）；Q-152 / CLQ-153（CLOSED）；Q-153 / CLQ-154（CLOSED / CONFIRMED）
- 关联任务：TASK-017、TASK-018

> 本文定义逻辑消息边界和一致性语义，并记录用户已确认的 Windows Named Pipes 传输选择。具体安全实现、消息帧/API 包装和目标 Windows 版本兼容性仍需 PoC 验证；本文仍为草案/非设计基线。

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
- Named Pipe ACL、端点命名、令牌/句柄校验等已成为选定技术的强制设计约束，但具体实现仍待 PoC 验证。

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
| IPC-A04 | USER_CONFIRMED_DECISION | Main Process 与同一交互式用户会话 CardHost 使用 Windows Named Pipes | Q-153/CLQ-154 用户明确选择 A；ADR-036 | 在目标 OS 验证 API、ACL、Logon SID、远程拒绝和身份核验 |

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
- Named Pipes 已由用户选择并记录为 ADR-036；数据库、序列化技术、加密和物理存储布局仍未选择。
- P3 继续 `IN_PROGRESS`；P4 保持 `NOT_STARTED`；不得将本工作包完成视为 P3 批准。

## 14. 修订记录

- v0.1（2026-11-20）：基于 ADR-034/035、P3-03/04 建立协议无关的 IPC 消息契约、幂等/版本一致性、重连对账、跨聚合补偿和安全边界分析；非设计基线。


## 15. IPC 物理传输候选比较（v0.2，尚待用户确认）

### 15.1 目标约束

- 仅用于同一台 Windows 设备上的 Main Process 与同一交互式用户会话 CardHost 通信。
- 需支持双向请求/响应、状态通知、连接中断检测、超时、重连和协议版本协商。
- 传输层必须可执行会话/用户身份约束；不得将网络监听端口暴露为产品需求。
- 数据量以控制命令、元数据快照和卡片状态通知为主；目前没有大吞吐/低延迟证据支持共享内存复杂度。
- 必须覆盖 Windows 10 22H2、Windows 11 x64、Windows Server 2012+ 的已确认范围；兼容性须逐版本实测。

### 15.2 候选方案

| 方案 | 优点 | 缺点/代价 | 安全与兼容性关注 | 当前判断 |
|---|---|---|---|---|
| A. Windows Named Pipes（推荐） | Windows 原生；支持双向流；不需端口分配/防火墙规则；可用 DACL/ACL 限制访问；.NET NamedPipe APIs 可封装 | 需自行定义消息帧、长度限制、取消/超时、连接生命周期和重连；管道名称发现与重复实例处理要设计 | 默认 DACL 不应直接使用；按用户 SID + Logon SID 限权；拒绝远程客户端；核验客户端 PID、进程会话及令牌；同一会话内恶意同用户进程仍需防冒用握手 | 最符合当前同机、双向、低流量场景；待原型验证 |
| B. Local RPC（ncalrpc） | Windows 原生本地 RPC；本地端点语义明确；可使用 RPC 安全机制 | IDL/存根、端点注册、部署诊断和调用模型较复杂；与托管运行时集成成本高于 Named Pipes | 必须正确配置 RPC endpoint ACL、认证/模拟级别；实现和诊断人员门槛较高 | 可行备选；当前无复杂 RPC 接口需求证据 |
| C. TCP loopback（127.0.0.1） | 协议/语言无关；易于抓包、复用 HTTP/gRPC 等生态；未来跨设备扩展较自然 | 端口发现、占用、生命周期、监听暴露面和防火墙/安全软件交互增加复杂度；必须强制 loopback 绑定 | 不能仅以“监听 localhost”视为认证；需认证/授权、随机端点或安全令牌、防远程绑定；Server 网络策略需测试 | 当前需求没有网络化/跨设备需求，复杂度偏高 |
| D. Shared Memory + Event/Mutex | 大块数据吞吐高、拷贝少 | 需自行处理缓冲区布局、同步、背压、崩溃恢复、版本兼容和内存映射生命周期；请求/响应仍需额外协议 | 命名对象 ACL/会话隔离同样需要设计；共享状态在崩溃时更难诊断 | 当前数据量不支持其复杂度，不推荐作为主 IPC |
| E. 其他 | 保留其他可论证方案 | 需提供与 A-D 可比较的安全、兼容、复杂度和验证证据 | 不得假设技术可用性 | 待用户提出 |

### 15.3 关键官方资料核验

1. Microsoft Windows IPC 机制指南将 Named Pipes 列为适用于全信任应用的双向流通信机制；同时列出 COM、Shared Memory、RPC 和 Loopback 等选项。
2. Microsoft Named Pipe 文档说明命名管道支持本机/网络访问，且访问受安全检查控制；若要求仅本机使用，需拒绝 NETWORK 访问或采用本地 RPC。
3. Microsoft Named Pipe Security 文档说明默认安全描述符可能授予 Everyone 与匿名账户读取权限；跨 Terminal Services 会话隔离可将 Logon SID 纳入 DACL。不得依赖默认 ACL。
4. CreateNamedPipe 文档提供 `PIPE_REJECT_REMOTE_CLIENTS`，可拒绝远程客户端。
5. Microsoft .NET Framework 系统要求表列出 .NET Framework 4.8 可安装于 Windows Server 2012/2012 R2/2016；但 Server 2012/2012 R2 已标记为 out-of-support。技术可安装/兼容不代表操作系统仍获安全支持。

### 15.4 推荐与置信度

- 推荐方案：A. Windows Named Pipes。
- 置信度：中高。
- 推荐理由：同机、双向、低流量、Windows-only 的当前需求与 Named Pipes 的能力匹配；无需开放端口；可通过显式 DACL 和远程拒绝选项构建最小权限边界；并能覆盖旧版 Windows 的原生机制。
- 适用前提：采用显式安全描述符；以 User SID + Logon SID 限制调用者；拒绝远程客户端；服务端核验客户端进程 PID/SessionId/令牌及签名/安装路径（可行时）；建立连接 nonce/实例令牌，防止同用户同会话的其他进程冒用；对消息长度、速率、类型和路径载荷做校验。
- 未知条件：具体 .NET 运行时/API 包装方式、Windows Server 2012 的安全描述符行为、服务端/客户端 PID 识别 API、同用户多会话 ACL 实际效果、终端安全软件干预、进程崩溃时读写中断语义。
- 选择后仍需最小 PoC：目标最低 OS 建立 duplex pipe；验证 Logon SID DACL、拒绝远程、非授权用户拒绝、跨 Session 拒绝、客户端 PID/Session 核验、超时/取消、半包/超大帧防护、进程退出和重连。

### 15.5 D2 澄清与基线边界（历史提交快照；当前状态以第 17 节为准）

- 提交 CLQ-154 V1.0 时，Q-153 为 D2 / OPEN / PENDING_CONFIRMATION，推荐 A；当时不应将 Named Pipes 写成已选技术。
- 用户随后明确选择 A；Q-153 当前 CLOSED / D2 / CONFIRMED，新增 ADR-036，当前选型与验证边界见第 17 节。
- 传输技术选择完成后，安全实现与目标 Windows 版本 PoC 仍 NEEDS_VALIDATION；这不阻断无依赖的 P3 分析，但阻断将 IPC 安全实现标记为已验证。
- 四维当前状态：记录完整性 COMPLETE；决策完成度 CONFIRMED；阶段阻塞性 NON_BLOCKING（传输选择已完成，安全 PoC 是 P3 验证项）；延期有效性 NOT_DEFERRED。
- 不修改 SRS v0.11 / REQ-BL-1.0、RAW、CR-001 或 RISK-001；不建立设计基线。

### 15.6 参考资料

- Microsoft Learn, Interprocess communication (IPC): https://learn.microsoft.com/en-us/windows/apps/develop/communication/interprocess-communication
- Microsoft Learn, Named Pipes: https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipes
- Microsoft Learn, Named Pipe Security and Access Rights: https://learn.microsoft.com/en-us/windows/win32/ipc/named-pipe-security-and-access-rights
- Microsoft Learn, CreateNamedPipeW: https://learn.microsoft.com/en-us/windows/win32/api/namedpipeapi/nf-namedpipeapi-createnamedpipew
- Microsoft Learn, .NET Framework system requirements: https://learn.microsoft.com/en-us/dotnet/framework/get-started/system-requirements
- Microsoft Learn, Lifecycle FAQ – .NET Framework: https://learn.microsoft.com/en-us/lifecycle/faq/dotnet-framework

## 16. 修订记录

- v0.2（2026-11-21）：追加 Named Pipes / Local RPC / TCP loopback / Shared Memory 候选比较、Microsoft 官方兼容性/安全资料、推荐及 Q-153 / CLQ-154；当时尚未确认具体 IPC 技术，非设计基线。


## 17. 用户确认 Named Pipes 与后续验证边界（v0.3）

### 17.1 已确认选择

- 用户原始回答（VERBATIM）：`FolderHub Q-153 / CLQ-154 我的选择：A。请按 software-predev-workflow-skill V2.6.1 追加用户选择记录，更新相关台账和 ADR，并继续 P3。`
- 用户选择 A：Windows Named Pipes（命名管道）。
- Q-153 / CLQ-154：`CLOSED / D2 / CONFIRMED`；ADR-036 已建立。
- 选择范围仅为 Main Process 与同一交互式用户会话 CardHost 之间的本机 IPC 传输机制；不代表序列化库、数据库、消息帧、运行时或安全实现已获批准。

### 17.2 传输层强制安全约束

1. 创建管道时使用显式 DACL，不依赖默认安全描述符。
2. DACL 结合 User SID 与当前 Logon SID/会话边界，验证跨用户、跨 Logon Session 和跨 Session 访问被拒绝。
3. 显式拒绝远程客户端；目标 Windows 版本验证 `PIPE_REJECT_REMOTE_CLIENTS` 或等效实现。
4. 服务端核验客户端 PID、SessionId、用户令牌和预期进程身份；不得单独信任 PID、管道名称或消息内 `senderRole`。
5. 采用连接 nonce/实例令牌或等效握手，验证同用户同会话非授权进程无法冒用。
6. 消息解析须具备最大帧长度、消息白名单、载荷/路径校验、超时/取消、半包处理和速率限制。
7. 日志不记录不必要的完整路径、访问令牌或认证材料；异常日志需脱敏。
8. 传输认证与业务授权分离；所有命令仍受 ADR-035 聚合写入权约束。

### 17.3 最小 PoC 验收矩阵

| 验证编号 | 场景 | 通过条件 | 状态 |
|---|---|---|---|
| NP-01 | 目标系统创建双向 Named Pipe | 支持的目标系统可建立本机双向连接 | NEEDS_VALIDATION |
| NP-02 | 显式 DACL | 仅预期用户/会话/进程可连接；匿名/非授权 SID 被拒绝 | NEEDS_VALIDATION |
| NP-03 | Logon SID 与 Session 隔离 | 同用户不同 Logon Session/SessionId 连接被拒绝 | NEEDS_VALIDATION |
| NP-04 | 远程客户端 | 非本机客户端连接被拒绝 | NEEDS_VALIDATION |
| NP-05 | 客户端进程身份 | PID/Session/令牌/进程来源核验与握手同时通过才建立授权连接 | NEEDS_VALIDATION |
| NP-06 | 同会话冒用 | 同用户同会话的非 FolderHub 进程不能仅凭管道名或消息字段冒用 | NEEDS_VALIDATION |
| NP-07 | 畸形/超大帧 | 超限/非法消息被拒绝，不导致进程崩溃或资源失控 | NEEDS_VALIDATION |
| NP-08 | 断连/半包/取消/超时 | 结果未知可对账；不重复执行非幂等副作用 | NEEDS_VALIDATION |
| NP-09 | CardHost/Main Process 崩溃与重连 | 连接释放、重建和状态对账符合 ADR-034/035 | NEEDS_VALIDATION |
| NP-10 | 协议版本不兼容 | 不兼容写命令被拒绝并有可诊断结果 | NEEDS_VALIDATION |
| NP-11 | OS 兼容矩阵 | Windows Server 2012/2012 R2 GUI、Server 2016+ Desktop Experience、Windows 10 22H2、Windows 11 x64 分别执行 | NEEDS_VALIDATION |

任一关键安全测试失败时，不得以放宽 DACL、跳过会话校验或关闭身份核验作为临时通过方式；应修正实现并重测。

### 17.4 当前限制与基线边界

- Named Pipes 传输机制已确认；实现与 OS 兼容性尚未通过实测。
- RISK-002 已完成 P3 定性评估并处于 `ASSESSED`；缓解措施未实施，用户未接受风险。
- 不确定数据库、序列化库、加密、物理存储布局、恢复 SLA 或 Supervisor。
- 本文保持 `DRAFT / 非设计基线`；P3 `IN_PROGRESS`，P4 `NOT_STARTED`。
- 不修改 SRS v0.11 / REQ-BL-1.0、RAW、CR-001、ADR-001～ADR-035 或 RISK-001。

### 修订记录
- v0.3（2026-11-21）：根据用户对 Q-153 / CLQ-154 的明确选择 A，记录 Named Pipes 为已确认 IPC 传输机制，新增安全约束与最小 PoC 验收矩阵；安全实现/兼容性仍 NEEDS_VALIDATION，非设计基线。

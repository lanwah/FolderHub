# FolderHub P3-07：Named Pipes IPC 安全边界与 PoC 验证计划

- 文档编号：FH-HLD-SEC-007
- 版本：v0.1
- 日期：2026-11-21
- 工作流规范：software-predev-workflow-skill V2.6.1
- 阶段：P3_HIGH_LEVEL_DESIGN
- 状态：DRAFT / SECURITY_DESIGN_AND_POC_PLAN（非设计基线）
- 输入需求基线：REQ-BL-1.0（SRS v0.11）
- 关联决策：ADR-034、ADR-035、ADR-036
- 关联问题：Q-153 / CLQ-154（CLOSED / CONFIRMED）
- 关联风险：RISK-002（ASSESSED）、RISK-001（ACCEPTED）
- 关联任务：TASK-017、TASK-018

> 本文将 ADR-036 已确认的 Windows Named Pipes 选型细化为安全设计边界和隔离 PoC 计划。本文不代表安全实现已完成，不选择序列化库、数据库、运行时或加密方案，也不建立设计基线。

## 1. 目标与范围

建立 Main Process 与同一交互式用户会话 CardHost 之间 Named Pipes 通信的威胁模型、访问控制要求、连接认证/授权边界、消息处理安全要求、故障响应和目标 OS PoC 验收条件。

不在本文决定：
- 具体 .NET 运行时、NamedPipe API 包装库、序列化库或最终消息帧编码；
- 数据库、物理存储布局、静态数据加密和密钥管理；
- 是否增加 Supervisor、Windows Service 或 CardHost 崩溃自动恢复 SLA；
- 任何新的用户可见功能或 SRS 需求。

## 2. 已确认架构约束

1. ADR-034：每个交互式用户会话一个 CardHost；CardHost 承载该会话全部独立卡片。
2. ADR-035：Main Process 是 CardDefinition / GlobalSettings 等主数据唯一写入者；CardHost 是 CardPresentationState 唯一写入者。
3. ADR-036：Main Process 与同一交互式用户会话 CardHost 使用 Windows Named Pipes 作为本机 IPC 传输机制。
4. Named Pipes 仅作为同机通信；不得因其支持网络管道而开放远程客户端。
5. 传输层身份验证不取代业务层授权；每条命令仍必须遵守 ADR-035 的聚合写入权。
6. SRS v0.11 / REQ-BL-1.0 是唯一需求基线；本设计不得扩展或修改用户可见行为。

## 3. 资产与信任边界

| 资产 | 需要保护的属性 | 典型影响 |
|---|---|---|
| CardDefinition / 启停命令 | 完整性、可用性 | 未授权启停/关闭卡片 |
| CardPresentationState | 完整性、可用性 | 位置/锁定状态被篡改或丢失 |
| FolderRecord 路径/别名/用途元数据 | 最小化机密性、完整性 | 本地敏感路径泄露或被替换 |
| Main Process / CardHost 控制通道 | 身份真实性、会话隔离 | 伪造命令、进程冒用、拒绝服务 |
| 协议版本/聚合版本 | 完整性 | 旧快照覆盖、重放、版本降级 |

信任边界：
- 操作系统内核与安全令牌：提供进程、用户、会话及对象访问控制原语；FolderHub 仍需正确配置和验证。
- Main Process 与 CardHost：均为 FolderHub 受信任组件，但彼此仍按 ADR-035 限定写入权。
- 同一用户/同一会话的其他本地进程：不默认可信，不能仅凭相同 User SID 或知道管道名称而获授权。
- 其他用户、其他 Logon Session、其他 Session、远程客户端：默认拒绝。

## 4. 威胁场景

| 威胁编号 | 威胁 | 可能后果 | 设计控制 |
|---|---|---|---|
| TH-01 | 默认 DACL 权限过宽 | 非授权本地账户读取/连接管道 | 显式安全描述符与最小权限 DACL |
| TH-02 | 同用户其他会话连接 | 跨会话卡片/路径信息访问 | User SID + Logon SID / Session 约束 |
| TH-03 | 远程客户端连接 | 超出本机 IPC 边界 | 显式拒绝远程客户端 |
| TH-04 | 同会话恶意进程冒用 | 伪造 Main Process 或 CardHost 命令 | 客户端进程令牌/会话/来源核验 + nonce/实例握手 |
| TH-05 | 管道名称抢占/伪造 | Main Process 连接到假 CardHost | 安全端点发现、服务端身份校验、握手挑战 |
| TH-06 | 重放/重复消息 | 重复执行启停/删除等副作用 | messageId 幂等、实例标识、版本校验 |
| TH-07 | 畸形/超大消息 | 内存耗尽、解析异常、进程崩溃 | 帧长度上限、白名单消息类型、载荷校验、速率限制 |
| TH-08 | 半包/断连/超时 | 状态不一致或未知结果 | 完整帧校验、超时、OutcomeUnknown、对账恢复 |
| TH-09 | 日志泄露 | 路径或身份材料暴露 | 最小化日志、脱敏、禁止记录令牌/秘密 |
| TH-10 | 版本不兼容/降级 | 新旧进程错误互操作 | 协议版本握手、Major 不兼容拒绝、升级期间禁止混连 |

## 5. 端点访问控制要求

### 5.1 命名与发现

- 管道端点名应包含应用命名空间、当前交互式 Session 标识和不可预测实例标识；不在端点名中包含用户名、完整路径或其他敏感数据。
- 管道名称本身不是秘密，也不是身份认证。
- Main Process 应从受保护的启动/发现上下文获得当前 CardHost 实例信息，并在连接后执行身份握手；不得仅连接第一个同名端点即视为可信。
- 端点创建失败、端点已存在或实例身份不匹配时，必须安全失败并输出脱敏诊断；不得自动连接未知同名进程。

### 5.2 显式 DACL

- 创建管道时显式构造安全描述符/DACL，不依赖默认安全描述符。
- 权限主体至少需要区分当前用户 SID 与当前 Logon SID；只允许完成连接所需的最小访问权。
- 拒绝匿名、Everyone/广泛本地账户及非预期用户访问；具体 ACE 集合、继承规则和访问掩码必须通过 PoC 验证。
- 对 Server 2012/2012 R2、Server 2016+、Windows 10/11 分别测试实际 ACL 行为，不以新系统测试结果推断旧系统。

### 5.3 远程访问拒绝

- 远程客户端必须被拒绝；实现应使用目标系统支持的远程拒绝能力（例如 `PIPE_REJECT_REMOTE_CLIENTS`）或等效控制。
- 不能将“只把管道名称告诉本机进程”视为远程访问控制。
- PoC 必须包含远程访问负向测试；若测试环境无法执行，标记 NOT_CHECKED，不得判 PASS。

## 6. 客户端身份认证与连接握手

建议的逻辑握手步骤（不冻结最终 API）：

1. Main Process 从当前会话安全启动上下文发现 CardHost 端点及预期 hostInstanceId。
2. 建立 Named Pipe 连接后，双方交换 protocolMajor/minor、sessionId、hostInstanceId、senderRole 和随机 challenge。
3. 服务端获取并验证客户端进程 PID、SessionId、用户令牌（User SID / Logon SID）及预期进程身份；客户端也必须验证服务端实例身份。
4. 双方完成 challenge-response；nonce/实例令牌应经受保护的启动/发现通道传递或派生，不得通过命令行明文、普通日志或未受 ACL 保护的共享位置传递。
5. 校验成功后建立本次连接上下文，并将授权角色绑定到已验证进程，而非信任消息中的 senderRole 字段。
6. 任一验证失败立即拒绝连接并关闭当前管道句柄；记录脱敏失败类别。

重要限制：
- PID 可复用，不能单独作为身份依据。
- 相同 User SID 不等于相同 Logon Session；必须测试 Logon SID/SessionId。
- 签名/安装路径检查可作为进程来源辅助证据，但不能替代 ACL、令牌和握手。
- 具体 token impersonation、客户端 PID 查询及句柄访问 API 需要在目标运行时/OS PoC 中验证。

## 7. 消息层安全与业务授权

- 使用显式消息类型白名单；未知消息、未知 Major 版本或不允许的方向一律拒绝。
- 对帧长度、嵌套深度、字符串长度、路径字段、枚举值和必填字段设置上限/约束。
- 每条命令绑定 messageId、correlationId、sessionId、hostInstanceId、aggregateId 和 expectedVersion；校验幂等与版本冲突。
- Main Process 不得向 CardHost 发送写入 CardPresentationState 的命令；CardHost 不得写入 Category、FolderRecord、GlobalSettings 或 CardDefinition。
- 失败结果区分 Unauthorized、SessionMismatch、ProtocolMismatch、InvalidPayload、VersionConflict、Timeout、OutcomeUnknown 和 StorageUnavailable。
- 任何无法确认是否提交的操作通过查询/对账恢复，不盲目重放非幂等副作用。

## 8. 生命周期与故障处理

- Main Process 或 CardHost 退出时，OS 应释放相应管道句柄；重连必须重新执行完整身份握手。
- CardHost 重启后 hostInstanceId 必须变化；旧连接/旧 nonce 不可复用。
- 连接断开、半包、超时和客户端身份验证失败不得改变已成功持久化的 CardPresentationState。
- 多实例/端点抢占时安全失败，不自动连接不明进程。
- 认证失败或跨会话连接尝试应被拒绝并留下最小化诊断。
- 升级期间新旧协议不兼容时拒绝写命令；升级/回滚策略由后续部署设计定义。

## 9. PoC 计划与验收矩阵

| 编号 | 场景 | 测试方式 | 通过条件 | 当前状态 |
|---|---|---|---|---|
| NP-01 | 双向本机连接 | 建立服务端/客户端最小原型 | 同机当前会话双向通信成功 | NEEDS_VALIDATION |
| NP-02 | 显式 DACL | 使用授权/非授权 SID 连接 | 授权主体可连接；其他主体被拒绝 | NEEDS_VALIDATION |
| NP-03 | Logon SID / Session 隔离 | 同用户不同登录会话尝试连接 | 非目标 Logon Session / SessionId 被拒绝 | NEEDS_VALIDATION |
| NP-04 | 远程客户端拒绝 | 从另一设备尝试连接 | 远程连接失败且无可用远程监听面 | NEEDS_VALIDATION |
| NP-05 | 客户端身份核验 | 比较真实 Main Process 与伪造进程 | 仅预期进程通过 PID/Session/令牌/来源核验及握手 | NEEDS_VALIDATION |
| NP-06 | 同会话冒用与端点抢占 | 同会话非 FolderHub 进程创建/连接同名管道 | Main Process 不接受伪造 CardHost；冒用方无法通过握手 | NEEDS_VALIDATION |
| NP-07 | 消息健壮性 | 畸形、未知类型、超大帧、半包、乱序 | 安全拒绝，不崩溃、不越权、不发生重复副作用 | NEEDS_VALIDATION |
| NP-08 | 超时/取消/断连 | 在读写中杀进程、断开管道 | 资源释放、OutcomeUnknown 可对账、重连重新认证 | NEEDS_VALIDATION |
| NP-09 | 重放/幂等/版本冲突 | 重复 messageId、旧 expectedVersion | 返回原结果或 VersionConflict，不重复执行副作用 | NEEDS_VALIDATION |
| NP-10 | 升级协议不兼容 | 新旧 Main/CardHost 混合启动 | 不兼容 Major 拒绝连接/写命令 | NEEDS_VALIDATION |
| NP-11 | OS 兼容矩阵 | 在目标 OS 分别执行 NP-01～NP-10 适用项 | Windows Server 2012/2012 R2 GUI、Server 2016+ Desktop Experience、Windows 10 22H2、Windows 11 x64 均形成独立结果 | NEEDS_VALIDATION |

PoC 不接入真实用户数据，不部署到生产环境，不开放远程监听。每项记录 OS 版本/补丁、运行时版本、构建号、测试脚本/Commit、预期结果、实际结果和日志证据。无可用目标环境时标记 NOT_CHECKED。

## 10. RISK-002 处置映射

- 当前状态：`ASSESSED`（中概率 / 高影响 / 高风险等级）；未接受、未关闭。
- 缓解措施：显式 DACL、User SID + Logon SID、远程拒绝、客户端身份核验、nonce/实例握手、消息白名单/帧长限制、幂等和版本检查、日志脱敏。
- 应急：发现越权连接、跨会话访问或冒用成功时停止相关 IPC 接入，拒绝连接并保留脱敏证据；修正控制后完整重测，不得以放宽 ACL 绕过。
- 残余风险：PoC 通过前维持高；通过后依据测试证据重评，不预先降低。
- 用户未接受风险；ADR-036 仅选择传输机制，不构成风险接受。

## 11. 需求与决策追踪

| 来源 | 本文响应 | 验证 |
|---|---|---|
| FR-016 / FR-017 / BR-012 | 卡片启停与 CardHost 生命周期命令受授权保护 | NP-05～NP-09 |
| FR-019 / ADR-033 | 展示状态更新消息受聚合/会话边界保护 | NP-03、NP-07、NP-09 |
| NFR-001 / RISK-001 | 目标 Windows 版本兼容性 | NP-11；RISK-001 原验证义务不变 |
| NFR-002 / ADR-035 | CardPresentationState 单一写入者及版本一致性 | NP-07～NP-09 |
| ADR-034 | 每交互式用户会话一个 CardHost | NP-03、NP-05、NP-06 |
| ADR-036 | Windows Named Pipes 传输机制 | NP-01～NP-11 |

## 12. 未决实现事项

以下内容不影响用户已确认的 Named Pipes 选型，但需在 P3/P4 完成设计/验证：
- 具体 .NET API 与运行时兼容矩阵；
- DACL ACE 细节、端点实例数/命名和启动发现实现；
- nonce/实例令牌安全传递方式；
- 进程来源/签名验证策略；
- 消息帧编码、序列化库、协议兼容策略；
- 同一用户多个交互式会话的持久化数据隔离与迁移；
- 与安装升级、崩溃恢复和 Supervisor 候选的协同。

## 13. 基线边界

- 本文为 DRAFT / 非设计基线；所有 NP 项均 NEEDS_VALIDATION。
- 不修改 SRS v0.11 / REQ-BL-1.0、RAW、CR-001、ADR-001～ADR-036 或 RISK-001。
- RISK-002 保持 ASSESSED，缓解措施未实施/验证，未获用户接受。
- P3 继续 IN_PROGRESS；P4 NOT_STARTED；概要设计基线未建立。

## 14. 修订记录

- v0.1（2026-11-21）：基于 ADR-036 建立 Named Pipes 威胁模型、安全设计约束、连接握手、消息层安全、故障边界及 NP-01～NP-11 隔离 PoC 计划；非设计基线。

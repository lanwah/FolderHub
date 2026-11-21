# FolderHub P3 CardHost 运行期故障恢复状态机与 PoC 方案

- 文档编号：FH-HLD-REC-010（建议编号，正式登记前核对编号台账）
- 版本：v0.1
- 日期：2026-11-21
- 状态：DRAFT / DESIGN_PROPOSAL / 非设计基线
- 输入：REQ-BL-1.0、ADR-021、ADR-034～ADR-038、P3-06 v0.4
- 关联任务：TASK-017、TASK-018

## 1. 适用范围

仅适用于 Main Process 仍运行且检测到同一交互式用户会话 CardHost 异常退出的情况。Main Process 已退出且 Keep Cards=ON 时仍遵守 ADR-037：不留下常驻 Supervisor/Windows Service，卡片暂时不显示，下次 Main Process 启动恢复最后成功持久化状态。

## 2. 已确认策略与设计假设分离

**用户已确认（ADR-038）：** Main Process 负责监测并有限重启 CardHost；采用退避；多次失败后提示用户；不无限重启。

**尚待验证的设计假设：**
- Main Process 内部生命周期协调器负责检测和重启，不派生独立常驻监督进程。
- 以进程句柄/退出事件为首选检测来源；具体 Windows/.NET API 待运行时选型与 PoC。
- 同一会话同一时刻最多一个有效 CardHost；重启前确认旧进程退出。
- 重启成功后重新建立 Named Pipes，并重新执行身份和会话验证。
- 重启恢复流程幂等；遵守 ADR-035 聚合写入权。

## 3. 候选状态机

```text
RUNNING
  └─ unexpected host exit → HOST_LOST
HOST_LOST
  └─ verify old process exited → VERIFY_EXITED
VERIFY_EXITED
  ├─ retry budget available → RESTART_WAIT
  └─ retry budget exhausted → DEGRADED
RESTART_WAIT
  ├─ backoff elapsed and Main Process alive → STARTING
  └─ Main Process exits → STOPPED_WITH_MAIN
STARTING
  ├─ process created → CONNECTING
  └─ start timeout/failure → RETRY_WAIT
CONNECTING
  ├─ pipe connected + peer authenticated → RESTORING
  └─ connect/auth timeout/failure → RETRY_WAIT
RESTORING
  ├─ state reconciliation succeeds + health check passes → RUNNING
  └─ restore/health failure → RETRY_WAIT
RETRY_WAIT
  ├─ retry budget available → RESTART_WAIT
  └─ retry budget exhausted → DEGRADED
DEGRADED
  └─ notify user; no automatic infinite restart; await explicit recovery action
```

## 4. 重试预算与退避参数

用户未指定数值。PoC 应通过实验比较候选参数，不在此预设固定值。至少评估：
- 最大重启尝试次数；
- 统计窗口/连续失败定义；
- 初始退避、最大退避、退避增长函数；
- 是否加入随机抖动；
- 启动超时、IPC连接超时、恢复超时；
- 连续失败后的冷却与人工恢复方式。

验收必须证明自动重启次数有硬上限，失败后最终进入 DEGRADED，不发生无限循环或资源持续增长。

## 5. PoC 测试用例

| ID | 场景 | 预期 |
|---|---|---|
| REC-POC-01 | CardHost 单次异常退出 | Main Process 检测并在退避后重启，恢复成功 |
| REC-POC-02 | 连续崩溃/启动即退出 | 重试次数受上限约束，最终停止自动重启并提示 |
| REC-POC-03 | 旧进程未完全退出 | 不创建第二个 CardHost，不出现重复卡片 |
| REC-POC-04 | Named Pipes 断连/认证失败 | 重新连接并重新认证；认证失败不得绕过 |
| REC-POC-05 | 状态恢复失败/数据损坏 | 不伪报健康；进入受控失败并保留诊断证据 |
| REC-POC-06 | Main Process 在退避等待时退出 | 取消后续启动，不留下后台重启责任主体 |
| REC-POC-07 | Main Process 在新 Host 启动期间退出 | 清理/终止受控子进程；验证无残留 Supervisor |
| REC-POC-08 | 恢复期间再次崩溃 | 计入有限预算，不重置计数造成无限循环 |
| REC-POC-09 | 连续重启与窗口状态恢复 | 幂等，无重复窗口、无旧状态覆盖新定义 |
| REC-POC-10 | 正常退出与异常退出区分 | 正常关闭不触发自动恢复 |

## 6. 安全、数据与进程边界

- 只接受预期用户会话的 CardHost；校验 PID、SessionId、用户令牌和预期进程身份。
- 每次新建连接均执行 Named Pipes 安全验证；不把 PID 或消息角色字段单独作为身份凭据。
- Main Process 不直接写 CardPresentationState；CardHost 不写 CardDefinition/GlobalSettings。
- CardDefinition 与 CardPresentationState 恢复对账需有版本检查、幂等和冲突诊断。
- 不记录敏感令牌材料；日志对路径等敏感信息脱敏。

## 7. 完成条件

- 状态机与进程边界在隔离 PoC 中可复现；
- 重试硬上限与退避行为有日志/时间线证据；
- 连续失败可进入 DEGRADED 并提示；
- Main Process 退出时不遗留后台监督实体；
- 恢复成功/失败均遵守 ADR-035 写入权和 ADR-036 IPC 安全约束；
- 记录环境版本、构建 Commit/Tag、测试结果和缺陷关联。

PoC 尚未执行；本文仅为 P3 设计方案，不代表测试 PASS、风险关闭或 HLD 基线建立。

# FolderHub P3 隔离 PoC 工具包准备一致性检查报告

- 报告编号：FH-REV-P3-POC-20261121-01
- 日期：2026-11-21
- 工作流：software-predev-workflow-skill V2.6.1
- 触发原因：P3 实质工作完成；TASK-017/TASK-018 证据字段和阶段管理摘要更新后执行强制只读检查
- 检查性质：只读一致性检查；非阶段评审批准、非设计基线建立
- 总体结论：WARNING（工具包已准备并静态检查；PowerShell 运行、真实 Windows 环境、产品级 PoC 尚未验证）

## 1. 检查范围

- 输入需求基线：SRS v0.11 / REQ-BL-1.0（只读）。
- 已确认决策：ADR-034～ADR-038（只读）。
- 问题台账：Q-P3-TEST-001、Q-P3-TEST-002、Q-P3-CRASH-001（只读）。
- 风险台账：RISK-001、RISK-002（只读）。
- 管理文件：Project_Status.md、Task_Board.md、project.md、docs/README.md、P3 工作计划。
- 新增交付：FH-TEST-POC-001、隔离 PoC 工具包 v0.1、环境盘点 v0.2、恢复 PoC 计划 v0.2。

## 2. 本轮交付

| 文件/工具 | 内容 | 状态 |
|---|---|---|
| `P3_PoC_Starter_Kit_v0.1.zip` | 隔离测试工具包 | 已生成 |
| `Get-FolderHubEnvironment_v0.2.ps1` | OS/Build/Edition/用户会话/显示器几何采集，支持身份脱敏 | 静态检查；Windows 未运行 |
| `Invoke-FolderHubRecoveryPoC.ps1` | dummy worker 生命周期、有限重启、退避、DEGRADED 模拟 | 静态检查；Windows 未运行 |
| `Start-FolderHubRecoveryWorker.ps1` | 无业务逻辑 worker 进程 | 静态检查；Windows 未运行 |
| `Deploy-FolderHubPoCKit.ps1` | 复制至新目录，拒绝覆盖 | 静态检查；Windows 未运行 |
| `Stop-FolderHubRecoveryPoC.ps1` | 校验 PID/命令行后停止 PoC worker | 静态检查；Windows 未运行 |
| `Remove-FolderHubPoCArtifacts.ps1` | 限定 artifacts 目录，支持 WhatIf/Confirm | 静态检查；Windows 未运行 |
| 测试矩阵/证据模板/Runbook | 定义执行顺序、证据和边界 | 已建立 |

## 3. 设计边界核对

- PowerShell 仅作为 PoC 测试驱动/进程模拟器，不代表产品运行时、框架、数据库、序列化库或 IPC 技术选型。
- 模拟器只观察 dummy worker 进程生命周期，不验证真实 CardHost 健康状态。
- 不实现 Named Pipes 安全、用户/会话身份认证、数据持久化、CardDefinition/CardPresentationState 对账、桌面 UI 或 Explorer 集成。
- 不安装服务、计划任务、开机启动项，不接入真实业务数据。
- 实验输入的重试次数/退避参数不是产品设计决策。
- 不以模拟器的 Main Process 退出清理行为替代 ADR-037 的 Keep Cards=ON 行为验证。

## 4. 一致性检查

| 检查项 | 结果 | 说明 |
|---|---|---|
| REQ-BL-1.0 / SRS v0.11 | PASS | 未修改需求基线 |
| RAW / Open_Issues / Decision_Log / CLQ | PASS | 本轮不修改既有需求、问题或决策；Q-P3 选择按既有记录引用 |
| ADR-037 / ADR-038 范围 | PASS | 仅按现有确认范围准备模拟测试，不扩展决策含义 |
| RISK-001 / RISK-002 | PASS | 风险状态不变；RISK-002 未标记缓解完成或用户接受 |
| 阶段门禁 | PASS | P3 IN_PROGRESS；P4 NOT_STARTED；HLD 基线未建立 |
| 任务状态 | PASS | TASK-017/TASK-018 保持 IN_PROGRESS；无新增 DONE |
| 工具包文件清单/README 引用 | PASS | 脚本引用存在，文件结构完整 |
| PowerShell 分隔符静态检查 | PASS（有限） | 括号/花括号/方括号平衡；不是 PowerShell AST/编译检查 |
| PowerShell AST 解析/执行 | NOT_CHECKED | 当前执行容器无 Windows PowerShell/pwsh |
| 真实 Windows 环境盘点 | NOT_CHECKED | Q-P3-TEST-002=尚未盘点 |
| REC-POC-01～05 执行 | NOT RUN | 工具包已准备，尚未在 Windows 执行 |
| NP-01～NP-11 Named Pipes 安全 PoC | NOT RUN | 未实现/未执行产品级安全验证 |
| CP-01～CP-10 持久化/恢复 PoC | NOT RUN | 产品存储技术未定，未执行 |
| Windows 兼容性/桌面集成 | NOT_CHECKED | 无实测证据 |
| Git Commit/Annotated Tag | NOT_CHECKED | 当前未连接项目 Git 仓库 |

## 5. 管理台账与阶段状态

- `Project_Status.md` 更新至 v4.8；P1/P2 APPROVED、P3 IN_PROGRESS、P4 NOT_STARTED；需求基线 REQ-BL-1.0 不变。
- `Task_Board.md` 更新至 v4.8；TASK-017/TASK-018 均 IN_PROGRESS，补充 PoC 工具包证据；不新增任务、不改变任务状态。
- `project.md` 六节摘要更新为 ADR-038 与当前 PoC 工具包状态。
- `docs/README.md` 更新 P3 工作计划、恢复 PoC 计划、环境盘点及工具包索引。
- 需求台账、问题台账、决策台账、风险台账均保持原权威记录；本轮无需求变更、无新增用户决策、无风险接受/关闭。

## 6. 后续动作

1. 在可用 Windows 环境执行 `Get-FolderHubEnvironment_v0.2.ps1`，完成实际环境清单。
2. 运行 REC-POC-01～05，保存 JSONL 日志和 JSON 摘要。
3. 评估 ADR-038 重试参数候选，不直接将实验输入作为产品参数。
4. 后续独立推进 Named Pipes NP-01～NP-11、持久化 CP-01～CP-10 及桌面/Explorer/RDP/DPI/升级兼容性测试。
5. 完成 P3 设计追踪、质量检查和评审准备后，等待用户明确评审/批准；未经批准不得进入 P4。

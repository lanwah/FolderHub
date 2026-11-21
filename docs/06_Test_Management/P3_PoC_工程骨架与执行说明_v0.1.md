# FolderHub P3 PoC 工程骨架与执行说明

- 文档编号：FH-TEST-POC-001
- 版本：v0.1
- 日期：2026-11-21
- 状态：DRAFT / ISOLATED_TEST_HARNESS_ONLY（非产品实现、非设计基线）
- 工作流：software-predev-workflow-skill V2.6.1
- 输入需求基线：REQ-BL-1.0（只读）
- 关联决策：ADR-034～ADR-038
- 关联任务：TASK-017、TASK-018

## 1. 本轮目标

根据 Q-P3-TEST-002=尚未盘点，先准备可在后续 Windows 测试环境中部署的隔离验证工具，不等待实际环境清单完成才开始整理测试流程。

## 2. 交付结构

`P3_PoC_Starter_Kit_v0.1.zip` 包含：

- `scripts/Get-FolderHubEnvironment_v0.2.ps1`：只读采集 Windows OS/Build/Edition、当前用户/会话、硬件和显示器几何信息；可选择脱敏。
- `scripts/Invoke-FolderHubRecoveryPoC.ps1`：模拟 ADR-038 高层策略的进程启动、崩溃检测、有限重启和退避。
- `scripts/Start-FolderHubRecoveryWorker.ps1`：模拟 CardHost 的无业务逻辑子进程。
- `scripts/Stop-FolderHubRecoveryPoC.ps1`：只针对记录中的 PoC worker PID，并校验命令行路径后停止。
- `scripts/Deploy-FolderHubPoCKit.ps1`：复制工具包至一个新测试工作目录，不覆盖既有目录，不注册系统服务/计划任务/开机启动。
- `scripts/Remove-FolderHubPoCArtifacts.ps1`：限制清理范围为工具包 artifacts 目录，并支持 WhatIf/Confirm。
- `docs/PoC_Test_Matrix.md`、`PoC_Evidence_Record_Template.md`、`Runbook.md`：测试范围、证据模板和执行说明。

## 3. 设计与安全边界

1. PowerShell 仅是测试驱动/模拟器，不是 FolderHub 产品运行时选择。
2. 模拟器只验证子进程生命周期控制流；进程存活观察不等于应用健康检查。
3. 不实现 Named Pipes、身份认证、安全描述符、持久化、CardDefinition/CardPresentationState 对账、桌面窗口或 Explorer 集成。
4. 不读取/写入真实 FolderHub 业务数据，不安装常驻服务，不创建开机启动项。
5. 重试次数和退避参数均为每次实验的显式输入，不是已批准产品参数。
6. 模拟器的 Main Process 退出清理行为不验证 ADR-037 的 Keep Cards=ON 产品行为。

## 4. 执行前置条件

- Windows PowerShell 5.1（`powershell.exe`）。
- 使用当前用户会话、尽可能低权限执行。
- 优先使用一次性虚拟机或隔离测试账户。
- 实际环境信息必须通过环境探测和人工核验后才可标记 `INVENTORIED`。

## 5. 静态检查结果

- 已检查 README 中脚本引用均有对应文件。
- 已对 PowerShell 源文件执行括号/花括号/方括号平衡检查。
- 当前执行容器不具备 Windows PowerShell，无法运行 PowerShell AST 解析、脚本执行或 Windows 行为验证；故脚本运行状态为 `NOT_CHECKED`，不能标记 PoC PASS。

## 6. 后续验证顺序

1. 在实际 Windows 环境运行环境探测脚本，建立环境清单。
2. 执行 REC-POC-01～REC-POC-05，保存 JSONL 事件日志和 summary。
3. 根据实测结果评估 ADR-038 的参数范围，不将单次实验参数直接写成产品设计决策。
4. 独立准备 Named Pipes NP-01～NP-11 和持久化/恢复 CP-01～CP-10 PoC；在运行时、序列化和存储技术未确认前，不把模拟器扩展成产品实现。
5. 按 Windows Server 2012/2012 R2、Server 2016+ Desktop Experience、Windows 10 22H2、Windows 11 x64 等目标范围建立逐环境验证矩阵。

## 7. 修订记录

- v0.1（2026-11-21）：建立隔离 PoC 工具包骨架和运行期恢复模拟器；仅完成静态检查，未在 Windows 执行。

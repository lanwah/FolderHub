# FolderHub P3 OS 身份核实与 REC-POC-01 执行一致性检查报告

- 报告编号：FH-REV-P3-REC01-20261121-01
- 版本：v1.0
- 日期：2026-11-21
- 工作流：software-predev-workflow-skill V2.6.1
- 检查性质：环境身份补充核实、PoC 执行证据判读、管理台账同步核查
- 总体结论：WARNING（REC-POC-01 的 harness 控制流验证通过，但不代表真实 CardHost 健康；后续 PoC 和目标环境覆盖未完成）

## 1. 输入证据

1. 用户提供的 `Win32_OperatingSystem` 截图：
   - Caption=`Microsoft Windows 11 专业版`
   - Version=`10.0.26300`
   - BuildNumber=`26300`
   - OSArchitecture=`64 位`
2. `recovery-summary-20ee473945ab4343a29533f3bb5a891a.json`
3. `recovery-events-20ee473945ab4343a29533f3bb5a891a.jsonl`

## 2. 环境身份判定

- `W11-RDP-001` 当前按 Windows 11 专业版 x64 登记。
- 原始环境 JSON 的 `ProductName=Windows 10 Pro` 保留，不覆盖；本次 Win32_OperatingSystem 查询提供独立身份佐证。
- 仍未取得完整注册表输出、UBR/EditionID 的独立复核结果；其他 Windows 目标版本和会话矩阵未盘点。

## 3. REC-POC-01 结果

| 指标 | 证据 |
|---|---|
| RunId | `20ee473945ab4343a29533f3bb5a891a` |
| Result | `STABLE_WINDOW_OBSERVED_NOT_HEALTH_PROOF` |
| LaunchCount / RestartCount / FailureCount | 2 / 1 / 1 |
| 首次 worker | 异常退出，ExitCode 73 |
| Backoff | 1 秒 |
| 第二次 worker | 启动成功，5 秒进程存活观察窗口结束 |
| Cleanup | 第二次 worker 已清理 |
| HarnessOnly | `true` |
| MainProcessLifetimeSeconds | 0 |

事件顺序：RUN_STARTED → WORKER_STARTED（模拟崩溃）→ WORKER_ABNORMAL_EXIT → RESTART_BACKOFF_STARTED → WORKER_STARTED → STABLE_OBSERVATION_WINDOW_ELAPSED → WORKER_CLEANED_UP。

判定：`PASS_WITH_LIMITATIONS`，只证明隔离 harness 的模拟进程生命周期和一次退避重启控制流。

## 4. 不可外推范围

本次 PoC 不实现/不验证 Named Pipes、对端认证、持久化、CardDefinition 对账、CardPresentationState、桌面 UI、真实 CardHost 健康检查，也不验证 Main Process 退出后的 Keep Cards=ON 行为。`MainProcessLifetimeSeconds=0`，不能据此证明 Main Process 长时间监控行为。

因此：
- REC-POC-01 不能作为 ADR-038 产品行为最终验证。
- REC-POC-02～05、NP-01～11、CP-01～10、桌面集成测试仍未执行。
- Windows 11 RDP 测试不能替代本地交互式桌面和 Windows Server 目标版本验证。

## 5. 管理状态与基线保护

- Q-P3-TEST-002：继续 `IN_PROGRESS`（一台环境身份已核实，完整矩阵未完成）。
- TASK-017/TASK-018：继续 `IN_PROGRESS`。
- RISK-001：保持 `ACCEPTED`，兼容性验证义务不变。
- RISK-002：保持 `ASSESSED`，Named Pipes 安全 PoC 未验证。
- REQ-BL-1.0 / SRS v0.11、RAW、ADR-001～ADR-038、CR-001：不变。
- P1/P2：`APPROVED`；P3：`IN_PROGRESS`；P4：`NOT_STARTED`；HLD 基线未建立。
- 本次不构成阶段评审批准、设计基线建立或进入 P4。

## 6. 同步文件

- `P3_Test_Environment_Inventory_v0.5.md`
- `Open_Issues.md`
- `Project_Status.md` v5.0
- `Task_Board.md`
- `project.md`

风险台账和决策台账无状态变更；本次无新增需求或架构决策。

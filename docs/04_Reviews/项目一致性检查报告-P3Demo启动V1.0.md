# FolderHub 项目一致性检查报告：P3 Demo 启动与资料同步

- 文档编号：FH-PM-CHECK-P3DEMO-001
- 版本：v1.0
- 日期：2026-11-20
- 工作流：software-predev-workflow-skill V2.6.1
- 触发原因：新增 TASK-019、任务字段变化、P3 Demo 实质工作结束前强制只读检查
- 检查方式：本地快照核验 + 资料库写回读回；只读检查，不自动修复、不更改需求/决策/风险状态
- 总体结论：WARNING
- 统计：PASS 8 / WARNING 1 / FAIL 0 / NOT_CHECKED 2

## 1. 检查范围

- `Project_Status.md`（阶段状态权威源）
- `Task_Board.md`（TASK 唯一权威源）
- `project.md`、`docs/README.md`（摘要/索引）
- `SRS_FolderHub_v0.11.md`（REQ-BL-1.0）
- `Raw_Requirements.md`、`Open_Issues.md`、`Decision_Log.md`、`Change_Log.md`、`Risk_Register.md`
- `P3_概要设计工作计划_v0.1.md`
- `docs/design-demo/` 及 P3 Demo 自检报告

## 2. 检查结果

| 编号 | 检查项 | 结果 | 证据/结论 |
|---|---|---|---|
| C-01 | P1/P2/P3/P4 阶段状态 | PASS | P1/P2 APPROVED、P3 IN_PROGRESS、P4 NOT_STARTED；本轮未推进阶段 |
| C-02 | 需求基线标识与完整性 | PASS | SRS 为 APPROVED / BASELINED / REQ-BL-1.0；SHA-256 仍为 `263926943cf870344369b2b430ed90963db7e655a907a0b097dc53be5b2ab8a7` |
| C-03 | TASK 唯一权威源与数量 | PASS | TASK-019 已登记为 P3 / DESIGN / IN_PROGRESS；活跃任务数 3，与摘要同步 |
| C-04 | 项目摘要和文档索引 | PASS | project.md、docs/README 已列出 TASK-019、P3 Demo 和当前阶段 |
| C-05 | Demo 目录与阶段定位 | PASS | Demo 位于 `docs/design-demo/current/`，未放入 `src/`；标记 P3_DEMO_DRAFT |
| C-06 | Demo 需求追踪 | PASS | README 建立 FR/BR/UIR 追踪，明确真实系统行为的模拟边界 |
| C-07 | 需求/决策/变更/风险基线保护 | PASS | SRS、RAW、ADR-001～033、CR-001、RISK-001 未修改；未新增产品 ADR 或 CR |
| C-08 | Q-015 状态与技术决策边界 | PASS | Q-015 仍 OPEN / D2 / DEFERRED；Demo 不选择独立卡片宿主进程/服务架构 |
| C-09 | 系统集成与视觉行为 | WARNING | 托盘、桌面层级、文件夹打开、开机启动、文件系统拖放均为网页模拟，不能作为 Windows 实机验证证据 |
| C-10 | Git Commit / Annotated Tag | NOT_CHECKED | 当前工作环境未连接或核验项目 Git 仓库；未创建或宣称任何 Git Tag |
| C-11 | 本机浏览器/Windows 兼容性 | NOT_CHECKED | 无头 Chromium 在当前执行容器启动异常；需在本地 Edge/Chrome 及目标 Windows 版本验证 |

## 3. 结论与边界

1. P3 初版 Demo 已形成，可供用户预览与交互评审；当前仍为 `P3_DEMO_DRAFT`。
2. 本轮未修改已批准需求基线；REQ-BL-1.0 保持不变。
3. Q-015 继续阻断独立卡片宿主/生命周期架构批准及相应设计基线，但不阻断本轮用户可见体验原型。
4. P3 概要设计仍 IN_PROGRESS；未批准 P3、未进入 P4、未建立正式设计基线。
5. 下一步为用户预览 Demo 并提出视觉/交互反馈；涉及需求语义变化时先走澄清/CR。

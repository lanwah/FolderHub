# FolderHub 项目一致性检查报告：P3 Demo 评审通过与 P3-01 推进

- 文档编号：FH-PM-CHECK-P3-20261120-02
- 版本：v1.0
- 日期：2026-11-20
- 工作流：software-predev-workflow-skill V2.6.1
- 触发原因：TASK-019 状态变化并从活跃区移入 `plans/done.md`；TASK 完成证据/关联更新；本轮实质工作结束前强制检查
- 检查方式：资料库文件读回 + 本地快照规则核验；只读检查，不自动修复、不改变源文件状态
- 总体结论：WARNING
- 统计：PASS 8 / WARNING 1 / FAIL 0 / NOT_CHECKED 2

## 1. 检查范围

- `Task_Board.md`：活跃任务、TASK-019 状态历史、任务计数与关联。
- `plans/done.md`：TASK-019 DONE 归档。
- `Project_Status.md`：阶段状态、需求/设计基线、Q-015 和风险状态。
- `project.md`、`docs/README.md`：管理摘要和文档索引。
- `SRS_FolderHub_v0.11.md`：REQ-BL-1.0（仅核对，未修改）。
- `Raw_Requirements.md`、`Open_Issues.md`、`Decision_Log.md`、`Change_Log.md`、`Risk_Register.md`（核对现有状态，未修改）。
- `P3_概要设计工作计划_v0.1.md`、`P3-01_系统上下文与边界分析_v0.1.md`。
- `docs/design-demo/README.md`、`change-log.md`、Demo 当前版本及 P3 Demo 自检/用户评审记录。

## 2. 检查结果

| 编号 | 检查项 | 结果 | 证据/结论 |
|---|---|---|---|
| C-01 | 阶段状态与门禁 | PASS | P1/P2 APPROVED、P3 IN_PROGRESS、P4 NOT_STARTED；未发生阶段转换 |
| C-02 | 需求基线保护 | PASS | SRS v0.11 仍为 APPROVED / BASELINED / REQ-BL-1.0；已登记基线 SHA-256 不变；本轮未写入 SRS |
| C-03 | TASK 唯一源、状态和归档 | PASS | TASK-019 已从活跃区移除、状态历史为 IN_PROGRESS→DONE、完成归档恰有一条；当前活跃任务为 TASK-017/018，计数 2 |
| C-04 | 项目摘要与文档索引 | PASS | project.md、docs/README 和 Project_Status 当前摘要均列出 P3 Demo 用户评审通过、TASK-019 归档及 P3 当前状态 |
| C-05 | Demo 用户评审批准边界 | PASS | 用户评审记录为 USER_REVIEW_APPROVED；明确仅批准 P3 Demo v0.1，不等于 P3 概要设计批准或正式设计基线 |
| C-06 | P3-01 设计草案质量与追踪 | PASS | 系统上下文、参与者、边界、运行约束、信息流、未知项和 RAW/SRS 追踪已形成；明确标记 DRAFT / 非设计基线 |
| C-07 | 专业台账/需求/决策/变更/风险保护 | PASS | 未修改 RAW、Q 状态、ADR-001～ADR-033、CR-001、RISK-001 或 SRS v0.11；未新增产品 ADR/CR |
| C-08 | Q-015 生命周期决策边界 | PASS | Q-015 仍 OPEN / D2 / DEFERRED；TASK-018 仍 IN_PROGRESS；未批准独立卡片宿主/生命周期架构 |
| C-09 | Demo 真实系统集成 | WARNING | 双击打开仍为模拟动作；未调用 Windows Explorer；真实文件系统、托盘、开机启动、桌面层级和持久化未实现/未验证，不能作为 Windows 实机证据 |
| C-10 | Git Commit / Annotated Tag | NOT_CHECKED | 当前环境未连接或核验项目 Git 仓库；未创建或宣称任何 Commit/Tag |
| C-11 | 本机浏览器与 Windows/Server 兼容性 | NOT_CHECKED | 当前环境未完成本机 Edge/Chrome 和目标 Windows 实机验证；RISK-001 的逐版本验证义务保留 |

## 3. 只读一致性核验摘要

- Task_Board 活跃任务：TASK-017、TASK-018；活跃任务数 2，与 Project_Status 和 project.md 当前摘要一致。
- TASK-019：不在活跃任务区；状态历史已追加 DONE；`plans/done.md` 存在对应归档。
- 阶段状态：P1/P2 APPROVED、P3 IN_PROGRESS、P4 NOT_STARTED；概要设计基线尚未建立。
- 需求基线：REQ-BL-1.0 保持不变；SRS v0.11 未修改。
- 未决问题：Q-015 保持 OPEN / D2 / DEFERRED；P3 相关架构批准/基线仍受其阻塞。
- 风险：RISK-001 保持 ACCEPTED；Windows Server 2012+ 兼容性验证义务未解除。

## 4. 结论与后续

1. 当前可见项目管理文件在阶段状态、任务状态/归档、需求基线和 Q-015 边界上保持一致。
2. Demo v0.1 用户评审通过，但仍是 P3 评审辅助草案；不构成 P3 概要设计批准或设计基线。
3. P3-01 首轮上下文分析草案已形成，可作为架构候选比较的输入。
4. 下一步完成 P3-02 总体架构候选方案与权衡初稿，并将 Q-015 生命周期约束纳入候选评估。
5. 本轮不进入 P4；需用户明确批准完整 P3 概要设计后才可进入 P4。


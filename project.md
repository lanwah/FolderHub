# Project Board

- 工作流规范：software-predev-workflow-skill V2.6.1

## 1. 项目目标
FolderHub 是一款 Windows 文件夹快速访问工具，通过桌面主组件和独立文件夹卡片帮助用户分类管理并快速打开常用文件夹。需求细节以 RAW 台账及用户确认的 ADR 为准。

## 2. 当前状态
- 当前阶段：P2_REQUIREMENT_ANALYSIS / REVIEW_PENDING（P1 已 APPROVED）。
- 原始需求 13 项；SRS v0.1～v0.11 草案已建立（v0.1～v0.10 历史版本保留，当前 v0.11），需求基线尚未建立；P3/P4 未启动。
- Q-001～Q-014、Q-016～Q-020 已关闭；Q-022～Q-025、Q-029～Q-151 已 CLOSED（Q-029～Q-150 为摘要重建）；Q-026 为 D2 / CLOSED / CONFIRMED（用户选择 A，已新增 ADR-033）；Q-015 为 D2 / DEFERRED（有效延期至 P3 架构决策节点）。
- ADR-001～ADR-031 为既有用户确认；ADR-032 汇总 Q-029～Q-150 历史确认；ADR-033 记录 Q-026 选择 A；TASK-001～TASK-010 已完成，TASK-011～TASK-013 为当前 P2 任务（评审包已提交，等待用户评审）；TASK-014（V2.6.1 工作流规范同步）和 TASK-015（独立复核）已完成；TASK-016（实例修复）DONE。
- Git Commit/Tag：NOT_CHECKED。

## 3. 关键决策
已确认桌面布局、分类关系、文件夹管理/删除规则、别名、用途说明、路径失效处理、打开交互、启动与托盘菜单、主组件锁定及跨重启保存、透明度、搜索和同类项目调研范围。详见 `docs/00_Project_Management/Decision_Log.md`。

## 4. 待办事项
TASK-011 已形成 SRS v0.11 草案；TASK-012 跟踪未决边界；TASK-013 已完成质量检查和评审包准备，等待用户评审；TASK-016 已完成。Q-022～Q-026、Q-029～Q-151 已关闭；Q-026 D2 已由用户选择 A 确认；Q-015 D2 已有效延期至 P3 架构决策节点。

## 5. 风险
- Q-015：独立卡片在主程序退出后的生命周期技术实现尚未确定，留待 P3 评估。
- RISK-001：Windows Server 2012+ GUI/Desktop Experience 兼容性尚未逐版本验证；用户已接受剩余风险（ACCEPTED），P3/P4 仍须逐版本验证。
- Q-001～Q-009 历史逐轮原文仍有部分不可访问，已按证据等级保留恢复边界。

## 6. 下一步
- 下一步行动：由用户评审 SRS v0.11、质量检查报告 v1.0 和 P2 评审准备包 v0.3，明确批准或退回修改。
- 依据/关联：RAW-001～RAW-013；Q-015/Q-023；RISK-001；Project_Status。
- 完成判据：SRS 覆盖完整、追踪和质量检查通过、未决项披露；等待用户明确批准指定版本后才建立需求基线。

维护规则：本文件仅为管理摘要，不是阶段/任务/决策/风险权威台账。

## 7. 2026-11-12 Q-151 同步
- Q-151 / CLQ-151：CLOSED / CONFIRMED；新增 ADR-031。
- 批量重试单条失败时保留待清理状态和失败原因，跳过并继续其他记录；中断任务优先续行；失败项后续按既定自动重试规则处理。
- SRS 草案补充至 v0.7，仍未批准；需求基线 NOT_ESTABLISHED。
- P2 继续 IN_PROGRESS；P3/P4 NOT_STARTED。TASK-011/012 IN_PROGRESS；TASK-013 TODO。


## 7. 2026-11-14 历史澄清同步
- Q-029～Q-150 已按可恢复的历史对话摘要补入权威资料库；CLQ-025～CLQ-146 逐题记录标记 RECONSTRUCTED。
- SRS 当前草案更新为 v0.8；v0.7 保留历史版本。
- `docs/README.md` 已与 Project_Status 阶段状态对齐。
- 原始问题全文、候选选项和逐字回答未恢复的内容明确标记 UNAVAILABLE；不得声称完整逐字历史恢复。

## V2.6.1 工作流适配摘要
- 决策分级：D1/D2/D3；D3 仅用于低风险、可逆且可可靠推导的普通细节，不创建 Q/CLQ。
- 实际提出的 D1/D2 继续保留 V2.2 完整澄清历史；CR 涉及实质性用户决策时执行相同分级。
- 分别记录记录完整性、决策完成度、阶段阻塞性、延期有效性；延期不等于接受或批准。
- RAW/Q/ADR/CR/阶段状态保留各自状态枚举；本次不回写历史分类，不改变任何已确认需求或阶段/基线状态。


## V2.6.1 实例修复状态（当前）
- 模板映射和台账字段规范已修正；ADR-032 历史状态字段已拆分，122 项历史证据等级仍为 RECONSTRUCTED。
- Q-026：D2 / CLOSED / CONFIRMED；用户选择 A；四维状态 COMPLETE / CONFIRMED / NON_BLOCKING / NOT_DEFERRED。用户可见行为已纳入 ADR-033 / FR-019；实现算法留待 P3/P4。
- Q-015：D2 / DEFERRED；有效延期至 P3 架构方案评审/批准前，不阻断 P2。
- CR-001：D1 / CONFIRMED；CR APPROVED 状态不变；RISK-001 兼容性验证义务保留。
- SRS v0.11：AI_DERIVED 已登记；DESIGN_ASSUMPTION NONE_ADOPTED；仍为 DRAFT，未建立需求基线；v0.9 保留历史。
- P2 保持 IN_PROGRESS；P2 阶段评审准备包 v0.2 为 DRAFT / PENDING_FINAL_QUALITY_REVIEW，仍须完成追踪矩阵与质量检查并由用户明确批准。


## 8. 2026-11-15 Q-026 用户确认同步
- 用户原始回答（VERBATIM）：`FolderHub Q-026 我的选择：A。请按 V2.6.1 追加澄清历史并更新相关台账及 SRS。`
- Q-026 / CLQ-024：D2，用户选择 A；Q 状态 CLOSED，确认状态/决策处理状态 CONFIRMED；四维 COMPLETE / CONFIRMED / NON_BLOCKING / NOT_DEFERRED。
- 新增 ADR-033；当前 SRS 草案 v0.10 新增 FR-019，更新 NFR-002 和 RAW-011/RAW-013 追踪。具体实现算法留待 P3/P4。
- P2 仍 IN_PROGRESS，需求基线 NOT_ESTABLISHED；当前下一步是最终追踪矩阵与质量检查，不代表 P2 已批准。


## 9. 2026-11-16 P2 最终收尾
- SRS 当前草案：v0.11；RAW→FR/BR/UIR/NFR 追踪矩阵已补齐。
- P2 质量检查报告 v1.0：WARNING / READY_FOR_USER_REVIEW_WITH_NOTES。
- P2 阶段评审准备包：v0.3 DRAFT；已提交用户评审。
- P2 状态：REVIEW_PENDING；需求基线 NOT_ESTABLISHED；P3/P4 NOT_STARTED。
- 本轮未修改 RAW 原文、用户确认 ADR 语义、CR-001、RISK-001；未代用户批准 SRS/P2。

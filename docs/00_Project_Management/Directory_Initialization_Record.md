# FolderHub 目录初始化与清理记录

- 记录编号：FH-PM-DIR-001
- 版本：v1.1
- 日期：2026-10-31
- 执行依据：software-predev-workflow-skill V2.0
- 执行范围：资料库 `/FolderHub/` 已有项目接管初始化及冗余项清理
- 用户批准：用户明确批准按扫描方案执行，并允许清理无效文件

## 1. 已完成的初始化

- 建立 `docs/templates/`，归档 V2.0 技能包提供的项目工作流模板及模板索引 README。
- 建立 `docs/02_Architecture/`。
- 确认项目管理、需求、详细设计、评审、开发管理、测试管理目录存在。
- 建立 `src/`、`tests/`、`scripts/`、`config/`。
- 建立根目录 `README.md`、`.gitignore` 和 `docs/README.md`。
- 将旧版项目根目录 `SKILL.md` 移至 `docs/_Archive/Legacy_SKILL.md`，作为历史参考，不作为当前执行规范。
- 将历史 ZIP 中的7份文档归档为独立文件：项目章程、项目状态、决策台账、变更台账、助手建议、原始需求台账、未决问题台账。

## 2. 冗余项清理

- 删除旧模板目录 `docs/_Templates/`；其可用模板已由 `docs/templates/` 中的 V2.0 模板覆盖。
- 删除旧命名且无文件内容的空目录 `docs/02_High_Level_Design/`；统一采用 `docs/02_Architecture/`。
- 此前创建的空目录 `docs/_Packages/` 已删除。
- 保留 `docs/_Archive/Legacy_SKILL.md`，因为它是历史规范文件，留档有助于追溯，不属于无效文件。
- 保留 `src/`、`tests/`、`scripts/`、`config/` 及后续阶段文档目录。它们目前为空，但属于项目结构，不是冗余文件。

## 3. 保留项与边界

- `docs/05_Development_Management/`、`docs/06_Test_Management/` 等后续阶段目录保留，按阶段启用。
- `assets/`、`tools/` 为按需目录，本次没有创建。
- 当前资料库目录清单未显示历史 ZIP 文件，因此本次未对该 ZIP 执行删除操作。

## 4. 需求与阶段影响

- RAW-001 至 RAW-012：内容、编号和状态未修改。
- Q-001 至 Q-009：内容、编号和状态未修改。
- ADR-001 至 ADR-006：内容、编号和状态未修改。
- CR：未新增，正式变更仍为0项（当前尚未建立需求基线）。
- 当前阶段：`P1_REQUIREMENT_COLLECTION`，状态 `IN_PROGRESS`。
- 需求基线：尚未建立。
- P2 需求分析：未启动。

## 5. 结论

本次仅整理目录、模板和文档索引，不代表需求采集阶段完成，不构成需求确认、需求基线批准或进入下一研发阶段的授权。

---

# V2.4 接管初始化追加记录

- 记录编号：FH-PM-DIR-001
- 版本：v1.2
- 日期：2026-11-01
- 执行依据：software-predev-workflow-skill V2.4
- 用户批准：2026-11-01 明确批准全部实施

## 6. 本次新增内容
- 根目录新增 `project.md` 项目摘要看板。
- 根目录新增 `Task_Board.md`，登记 TASK-001；根目录新增 `plans/done.md` 完成归档。
- 新增 `docs/00_Project_Management/问题澄清记录台账.md`，建立 Q-001 至 Q-010 的索引；可核验历史不足部分明确标记待补录，不伪造历史。
- 新增 `docs/00_Project_Management/Risk_Register.md`，建立风险管理权威台账；没有证据的事项未伪造成已评估风险。
- `Project_Status.md` 更新至 v0.3；`docs/README.md` 更新 V2.4 文件索引。

## 7. 保留与阶段影响
- 保留 RAW-001 至 RAW-013、Q-001 至 Q-010、ADR-001 至 ADR-007 原有内容和编号；未修改需求或决策基线。
- 保留根目录 README、原有常用命令内容、既有模板和目录；本次未删除、迁移、重命名既有文件。
- P0 V2.4 接管初始化完成；当前仍为 `P1_REQUIREMENT_COLLECTION / IN_PROGRESS`。
- 需求基线、概要设计基线、详细设计基线均尚未建立；正式基线变更为 0。
- Git 仓库、Commit、Tag 在当前资料库环境不可验证，标记 `NOT_CHECKED`。

## 8. 结论
本次仅补齐 V2.4 项目管理体系，不代表需求采集完成，不构成需求确认、需求基线批准或进入 P2 需求分析的授权。

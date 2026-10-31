# FolderHub 原始需求台账

- 文档编号：FH-REQ-RAW-001
- 版本：v0.2
- 状态：需求采集草稿（非基线）
- 日期：2026-10-31

> 原始需求编号永久保留。状态表示当前登记处理情况，不代表需求基线已批准。用户原始意图与具体实现细节分开管理。

| 编号 | 原始需求/历史表达摘要 | 来源类型 | 当前状态 | 备注 |
|---|---|---|---|---|
| RAW-001 | 开发一款 Windows 文件夹快速访问工具 | USER_ORIGINAL | PENDING_CONFIRMATION | 项目总体目标明确，具体范围待细化 |
| RAW-002 | 文件夹支持分类管理 | USER_ORIGINAL | PENDING_CONFIRMATION | 分类规则待澄清 |
| RAW-003 | 文件夹支持别名 | USER_ORIGINAL | PENDING_CONFIRMATION | 别名编辑规则待澄清 |
| RAW-004 | 文件夹支持功能说明 | USER_ORIGINAL | PENDING_CONFIRMATION | 说明字段及展示方式待澄清 |
| RAW-005 | 保存文件夹真实路径 | USER_ORIGINAL | PENDING_CONFIRMATION | 路径失效处理待澄清 |
| RAW-006 | 支持通过双击或其他操作打开文件夹 | USER_ORIGINAL | PENDING_CONFIRMATION | 具体交互待确认 |
| RAW-007 | 桌面显示采用桌面壁纸层 | USER_CONFIRMED_DECISION | CONFIRMED | 用户此前明确选择 |
| RAW-008 | 文件夹布局采用左侧分类、右侧列表 | USER_CONFIRMED_DECISION | CONFIRMED | 用户此前明确选择 |
| RAW-009 | 开机启动可配置 | USER_CONFIRMED_DECISION | CONFIRMED | 用户明确提出可配置是否开机启动 |
| RAW-010 | 默认启动后托盘常驻 | USER_CONFIRMED_DECISION | CONFIRMED | 用户明确提出默认托盘常驻 |
| RAW-011 | 支持配置是否固定到桌面；支持调整桌面位置及调整后固定操作 | USER_CONFIRMED_DECISION | CONFIRMED | “固定”的精确定义仍需澄清 |
| RAW-012 | 固定到桌面时支持配置显示透明度 | USER_CONFIRMED_DECISION | CONFIRMED | 透明度范围、默认值、作用对象待澄清 |
| RAW-013 | 文件夹支持固定到桌面显示，默认不开启，由用户针对单个文件夹手动开启。经用户澄清，该功能是独立于 FolderHub 主组件的单个文件夹桌面卡片；卡片显示图标、名称和用途说明；主组件隐藏或退出后仍继续显示；允许同时创建多个独立卡片；每个卡片支持独立调整位置和固定位置。 | USER_CLARIFICATION | CONFIRMED | 与 RAW-011 的主组件整体固定功能严格区分；独立组件的进程生命周期实现方式待后续设计澄清。 |

## 本次修订记录
- v0.2（2026-10-31）：新增 RAW-013，记录用户澄清的独立文件夹桌面卡片行为；RAW-011 主组件整体固定需求保持不变。

## 需求来源与状态说明
- USER_ORIGINAL：用户最初提出的原始需求。
- USER_CLARIFICATION：用户后续澄清。
- USER_CONFIRMED_DECISION：用户明确确认的决策。
- ASSISTANT_SUGGESTION：助手建议，单独记录，不计为用户需求。
- PENDING_CONFIRMATION：已登记，尚未形成完整确认。
- CONFIRMED：用户明确确认需求意图或产品选择；不代表所有实现细节均已确定。

## 历史来源说明
本台账根据 FolderHub 历史对话整理。若后续发现更完整的原始措辞，以可核验的历史原文补充，不覆盖现有编号。

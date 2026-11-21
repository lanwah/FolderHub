# FolderHub P3 Demo v0.1 自检报告

- 文档编号：FH-REV-DEMO-001
- 版本：v1.0
- 日期：2026-11-20
- 工作流：software-predev-workflow-skill V2.6.1
- 对象：`docs/design-demo/current/`（p3-demo-v0.1）
- 状态：SELF_CHECK_COMPLETE / USER_REVIEW_PENDING
- 输入基线：REQ-BL-1.0

## 1. 范围

检查 Demo 是否符合 P3 初版定位：整体布局、页面结构、主要视觉方向、导航及核心用户可见流程。Demo 不替代概要设计，不形成正式设计基线。

## 2. 检查结果

| 检查项 | 结果 | 说明 |
|---|---|---|
| 文件目录符合 V2.6.1 | PASS | 位于 `docs/design-demo/`，当前版本位于 `current/`；未放入 `src/` |
| 需求输入基线标识 | PASS | README 标记 REQ-BL-1.0 / SRS v0.11 |
| 主组件布局 | PASS | 左侧分类、右侧文件夹列表 |
| 独立卡片用户可见行为 | PASS | 支持固定、拖动和独立锁定的交互模拟 |
| 搜索与记录操作 | PASS | 动态搜索、添加/编辑、重复路径提示、右键操作模拟 |
| 托盘和设置 | PASS | 托盘菜单、隐藏/显示、透明度与全局卡片保留开关模拟 |
| 基线保护 | PASS | 未修改 SRS、RAW、ADR、CR、REQ-BL-1.0 |
| JavaScript 语法检查 | PASS | `node --check` 通过 |
| 浏览器渲染（当前执行环境） | NOT_CHECKED | 尝试使用无头 Chromium 截图但运行环境的 DBus/zygote 启动异常并超时；需在本地 Edge/Chrome 实际打开检查 |
| Windows 实机/Server 兼容性 | NOT_CHECKED | 不属于网页原型自检结论；RISK-001 验证义务保留 |
| Git Commit/Tag | NOT_CHECKED | 当前工作容器未连接项目 Git 仓库 |

## 3. 未决与限制

1. Q-015 保持 OPEN / D2 / DEFERRED；Demo 只模拟独立卡片，不决定宿主生命周期架构。
2. FR-009 真实 Windows Explorer 拖放尚未实现。
3. 文件夹打开、托盘、开机启动、桌面层级、位置持久化均为模拟行为。
4. 数据持久化、路径规范化、DPI/多显示器恢复算法未在本 Demo 中确定。

## 4. 结论

P3 初版 Demo 已形成，可提交用户进行界面与交互评审；当前状态仍为 `P3_DEMO_DRAFT`。不代表 P3 概要设计整体完成，不批准进入 P4，也不建立正式设计基线。

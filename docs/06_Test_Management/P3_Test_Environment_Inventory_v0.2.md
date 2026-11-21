# FolderHub P3 测试环境策略与盘点记录

- 文档编号：FH-TEST-ENV-001
- 版本：v0.2
- 日期：2026-11-21
- 状态：DRAFT / 环境待盘点
- 关联选择：Q-P3-TEST-001、Q-P3-TEST-002
- 需求基线：REQ-BL-1.0（只读输入）

## 1. 用户确认

- Q-P3-TEST-001：E，多种 Windows 测试环境。
- Q-P3-TEST-002：尚未盘点。

这表示验证策略应覆盖多种 Windows 环境，但当前尚无可用环境的具体版本、Edition、Build、补丁水平、运行形态和访问方式清单。不得将候选测试矩阵误记为现成环境。

## 2. 先行准备工作

1. 建立 PoC solution/project 骨架，拆分 Named Pipes 安全、CardHost 持久化/恢复、生命周期恢复三个隔离验证目标。
2. 建立 PowerShell 环境探测脚本，采集 Windows ProductName、DisplayVersion/ReleaseId、CurrentBuild、UBR、Edition、架构、会话类型、用户 SID/Logon SID、SessionId、DPI/显示器信息。
3. 准备测试部署/清理脚本，确保 PoC 不接入真实业务数据，不留下持久后台进程。
4. 为每次执行生成环境清单、测试步骤、日志、结果、缺陷和 Git Commit/Tag 关联。
5. 实际环境清单确认后，再按目标 OS 范围安排覆盖；最低支持版本优先，不可用环境标记 BLOCKED/NOT RUN，不得伪报 PASS。

## 3. 候选环境矩阵（待盘点，不代表已具备）

| 环境类别 | 候选版本 | 关注点 | 当前状态 |
|---|---|---|---|
| 最低边界服务器 | Windows Server 2012 / 2012 R2 | API/IPC/会话/权限兼容性 | NOT_INVENTORIED |
| 中间服务器 | Windows Server 2016 / 2019 | Desktop Experience、会话生命周期 | NOT_INVENTORIED |
| 新版服务器 | Windows Server 2022 / 2025 | 新版本回归与安全策略 | NOT_INVENTORIED |
| 客户端桌面 | Windows 10 22H2 / Windows 11 x64 | Explorer、桌面层级、DPI、多显示器、托盘 | NOT_INVENTORIED |
| 远程桌面 | RDP（按实际目标 OS） | 断连/重连、会话切换、注销 | NOT_INVENTORIED |

## 4. 环境探测脚本

同目录提供 `Get-FolderHubEnvironment_v0.2.ps1`，目标为 Windows PowerShell 5.1+，只读采集 OS/Build/Edition、用户 SID/Logon SID、SessionId、会话类型、硬件及显示器基础信息，并输出 JSON。脚本尚未在目标 Windows 环境执行；显示器 DPI 细节仍需在实际 PoC 中单独核验。输出含机器/用户标识，分享前应脱敏。

## 5. 证据与状态规则

每个环境记录：环境 ID、OS 名称/版本/Edition/Build/补丁、架构、实体机/虚拟机、Local/RDP、权限配置、显示器/DPI、FolderHub 构建、Git Commit/Tag、执行人/时间、测试编号、日志与截图、PASS/FAIL/BLOCKED/NOT RUN。


## 6. 工具包补充（v0.2）

- 新增 `docs/06_Test_Management/P3_PoC_Starter_Kit_v0.1/` 隔离 PoC 工具包。
- 环境探测脚本增加 `-RedactIdentity` 参数，并将显示器像素几何与 DPI 测量状态分开；`DpiMeasurement` 明确为 `NOT_MEASURED_BY_THIS_SCRIPT`，不可把屏幕尺寸误当 DPI 证据。
- 环境探测脚本会自动创建输出文件的父目录。
- 该脚本仍未在目标 Windows 环境执行；实际环境盘点状态仍 `NOT_INVENTORIED`。

### 修订记录
- v0.2（2026-11-21）：更新环境探测脚本的隐私脱敏和显示器/DPI 字段边界；新增 PoC 工具包引用；不改变候选 OS 范围。

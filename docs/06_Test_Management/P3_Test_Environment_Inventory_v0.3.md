# FolderHub P3 测试环境策略与盘点记录

- 文档编号：FH-TEST-ENV-001
- 版本：v0.3
- 日期：2026-11-21
- 状态：DRAFT / 部分环境已盘点
- 关联选择：Q-P3-TEST-001、Q-P3-TEST-002
- 需求基线：REQ-BL-1.0（只读输入）

## 1. 用户确认

- Q-P3-TEST-001：E，多种 Windows 测试环境。
- Q-P3-TEST-002：尚未盘点。

这表示验证策略应覆盖多种 Windows 环境。此前尚无具体环境清单；2026-11-21 收到一份实际环境采集 JSON，新增一台 Windows x64 / RDP 环境记录。该记录仍有 OS 产品名称字段冲突，且不是完整的目标环境矩阵；不得将单台环境误记为全部候选环境均已具备。

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
| 客户端桌面 | Windows 10 22H2 / Windows 11 x64 | Explorer、桌面层级、DPI、多显示器、托盘 | PARTIALLY_INVENTORIED（单台环境，OS 字段待核实） |
| 远程桌面 | RDP（按实际目标 OS） | 断连/重连、会话切换、注销 | PARTIALLY_INVENTORIED（当前采集为 RDP） |

## 4. 环境探测脚本

同目录提供 `Get-FolderHubEnvironment_v0.2.ps1`，目标为 Windows PowerShell 5.1+，只读采集 OS/Build/Edition、用户 SID/Logon SID、SessionId、会话类型、硬件及显示器基础信息，并输出 JSON。脚本尚未在目标 Windows 环境执行；显示器 DPI 细节仍需在实际 PoC 中单独核验。输出含机器/用户标识，分享前应脱敏。

## 5. 证据与状态规则

每个环境记录：环境 ID、OS 名称/版本/Edition/Build/补丁、架构、实体机/虚拟机、Local/RDP、权限配置、显示器/DPI、FolderHub 构建、Git Commit/Tag、执行人/时间、测试编号、日志与截图、PASS/FAIL/BLOCKED/NOT RUN。


## 6. 工具包补充（v0.2）

- 新增 `docs/06_Test_Management/P3_PoC_Starter_Kit_v0.1/` 隔离 PoC 工具包。
- 环境探测脚本增加 `-RedactIdentity` 参数，并将显示器像素几何与 DPI 测量状态分开；`DpiMeasurement` 明确为 `NOT_MEASURED_BY_THIS_SCRIPT`，不可把屏幕尺寸误当 DPI 证据。
- 环境探测脚本会自动创建输出文件的父目录。
- 该脚本此前未在目标 Windows 环境执行；本次收到的环境 JSON 已作为一次实际采集证据，但尚未核实字段冲突，其他目标环境仍未盘点。

### 修订记录
- v0.2（2026-11-21）：更新环境探测脚本的隐私脱敏和显示器/DPI 字段边界；新增 PoC 工具包引用；不改变候选 OS 范围。


## 7. 实际环境记录（2026-11-21）

| 字段 | 采集值 |
|---|---|
| 环境 ID | `W11-RDP-001`（本记录分配的追踪编号） |
| 来源 | 用户上传 `environment-inventory.json` |
| 采集时间（UTC） | 2026-11-21T08:37:33.5865139Z |
| OS Caption | `Microsoft Windows 11 专业版` |
| ProductName | `Windows 10 Pro` |
| Version / Build / UBR | `10.0.26300` / `26300` / `9974` |
| DisplayVersion | `26H2` |
| EditionID / Architecture | `Professional` / x64 |
| Session | `RDP-Tcp#0`，SessionId `1`，类型 `RDP` |
| Elevation | 非管理员 |
| PowerShell | `7.6.6` |
| Hardware | Intel Core i5-14600KF；20 logical processors；约 31.9 GiB RAM |
| Display geometry | Primary bounds `1920x1080`；working area `1920x1032` |
| Display devices | 5 个设备条目；部分分辨率为空；另有一个 3840x2160 条目 |
| DPI | `NOT_MEASURED_BY_THIS_SCRIPT` |

### 7.1 证据判读与限制

1. `Caption` 与 `ProductName` 不一致：前者为 Windows 11 专业版，后者为 Windows 10 Pro。当前记录保留原始字段，不自行修正或推定最终产品版本。
2. `Version=10.0.26300`、`BuildNumber=26300`、`UBR=9974`、`DisplayVersion=26H2` 按原始 JSON 记录；尚未独立核实该构建与正式支持版本矩阵的对应关系。
3. 当前会话是 RDP 且采集进程非管理员。该环境可用于非提权用户态 PoC 的初步运行准备；不能代替本地交互式桌面、管理员/标准用户权限边界或其他目标 Windows 版本验证。
4. 屏幕几何信息不等于 DPI 证据；多个显示设备条目与部分空字段需在桌面集成测试中单独核实。
5. CPU/内存/显示器信息仅描述本次采集主机，不构成性能验收或目标硬件覆盖结论。

### 7.2 当前盘点状态

- 已盘点：1 台 Windows x64 / RDP 环境（`W11-RDP-001`），OS 身份字段待核实。
- 未盘点：Windows Server 2012 / 2012 R2、Server 2016/2019、Server 2022/2025、Windows 10 22H2 独立环境，以及其他 Windows 11 环境。
- 测试执行状态：环境采集证据已收到；REC-POC-01～05、NP-01～NP-11、CP-01～CP-10 和桌面集成测试均未因此自动变为已执行。
- 后续：先在该 RDP 环境运行隔离的 REC-POC-01～05；并通过 Windows 系统信息/注册表读取等独立只读证据核实 OS 字段。完成后再扩展其他目标 OS 环境。

### 修订记录
- v0.3（2026-11-21）：追加实际环境 `W11-RDP-001` 的采集证据与限制；不修改目标 OS 支持范围、不宣称兼容性通过、不改变需求或设计基线。

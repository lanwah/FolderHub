# FolderHub

<p align="center">
  <img src="assets/icons/png/folderhub-app/folderhub-app-128.png" alt="FolderHub" width="96"/>
</p>

<p align="center">
  <strong>Windows 桌面文件夹快速访问工具</strong><br/>
  壁纸层主组件管理分类与列表，常用目录可固定为独立桌面卡片。
</p>

<p align="center">
  <img src="https://img.shields.io/badge/platform-Windows%2010%2F11-blue" alt="Platform"/>
  <img src="https://img.shields.io/badge/.NET-8.0-512bd4" alt=".NET 8"/>
  <img src="https://img.shields.io/badge/version-0.1.0-informational" alt="Version"/>
</p>

---

## 项目介绍

桌面上的快捷方式一多，就难以按用途分类、也难以一眼认出路径。**FolderHub** 在 Windows 桌面壁纸层提供一块可定制的主面板：左侧是平级分类，右侧是当前分类下的文件夹卡片。你可以为每条记录设置别名和用途说明，用搜索快速定位，双击即可用系统默认资源管理器打开。

除了主面板，还可以把单个文件夹 **固定为独立桌面卡片**——位置、锁定状态与主面板无关，多个卡片可同时存在。程序默认 **托盘常驻**，开机可自动启动；数据保存在本机 SQLite，不依赖云服务。

### 技术概览

| 项 | 说明 |
|---|---|
| UI | WPF（.NET 8，`net8.0-windows`） |
| 架构 | 主程序 `FolderHub.App` + 独立卡片进程 `FolderHub.CardHost` |
| 数据 | SQLite（`%LocalAppData%\FolderHub\`） |
| 进程通信 | Named Pipes（主程序与卡片宿主） |

更完整的需求与架构说明见仓库内 `docs/`（SRS、开发计划、设计 Demo 等）。

---

## 功能特性

### 分类与文件夹

- 平级分类列表（不支持嵌套）；一个文件夹可同时属于多个分类
- 新增 / 编辑 / 删除分类与文件夹记录，支持手动排序
- 可选 **别名** 与 **用途说明**；列表优先显示别名
- **名称 / 别名搜索**（不区分大小写，动态过滤）
- 从资源管理器 **拖拽文件夹** 到主界面即可建立记录
- 删除记录 **不会删除磁盘上的真实目录**；路径失效时保留记录并可重新指定

### 主组件（壁纸层）

- 左侧分类 + 右侧文件夹网格/列表
- 可固定到桌面、拖动与调整大小、**位置锁定**
- **背景透明度** 可调（文字与图标保持不透明）
- **浅色 / 深色** 主题

### 独立桌面卡片

- 右键文件夹 →「固定到桌面」，每张卡片独立显示
- 卡片可单独移动、锁定；主面板隐藏时仍可操作
- 设置中可配置：**退出主程序后是否保留独立卡片**（全局开关，默认开启）

### 托盘与启动

- 系统托盘图标：打开主界面、设置、恢复桌面卡片、退出
- 可选 **开机自动启动**（当前 Windows 用户）

### 当前版本说明（0.1.0）

- 目标环境：**Windows 10（22H2）**、**Windows 11（x64）** 本地交互式桌面
- 尚不包含：远程桌面多用户、Windows Server Core、云同步等（见 `docs/01_Requirements/SRS_FolderHub_v0.11.md` 非目标范围）

---

## 环境要求

| 要求 | 说明 |
|---|---|
| 操作系统 | Windows 10 22H2 或 Windows 11（64 位） |
| 运行库 | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)（使用框架依赖发布包时必需） |
| 从源码构建 | [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) + Windows 版 Visual Studio 2022 或 `dotnet` CLI |
| 权限 | 普通用户即可；开机启动项写入当前用户注册表（HKCU） |

---

## 安装

### 方式一：下载发布包（推荐）

1. 在 GitHub **Releases** 中下载对应 `win-x64` 压缩包（发布后提供）。
2. 解压到任意目录（例如 `D:\Apps\FolderHub`）。
3. 若发布包为 **框架依赖** 类型，请先安装 **.NET 8 Desktop Runtime**。
4. 运行 `FolderHub.App.exe`。

> 发布包内应同时包含 `FolderHub.App.exe` 与 `FolderHub.CardHost.exe`（同一目录）。

### 方式二：本机脚本安装（从源码或 CI 产物）

在项目根目录执行发布与安装：

```powershell
# 1. 发布到 artifacts（默认 Release + win-x64，需本机已装 Desktop Runtime）
.\packaging\Publish-FolderHub.ps1

# 自包含发布（体积更大，适合未安装 Runtime 的环境）
.\packaging\Publish-FolderHub.ps1 -SelfContained

# 2. 安装到 %LocalAppData%\FolderHub\app\（升级时保留数据库）
.\packaging\Install-FolderHub.ps1 -SourceDir .\artifacts\publish\win-x64

# 可选：安装前备份数据库，安装后自动启动
.\packaging\Install-FolderHub.ps1 -SourceDir .\artifacts\publish\win-x64 -BackupDatabase -StartAfterInstall
```

**数据与程序路径：**

```text
%LocalAppData%\FolderHub\
  folderhub.db          # 用户数据（升级保留）
  app\
    FolderHub.App.exe
    FolderHub.CardHost.exe
    …
```

卸载二进制（保留数据）：

```powershell
.\packaging\Uninstall-FolderHub.ps1
```

删除数据目录需使用 `-RemoveUserData`（详见脚本帮助）。

### 方式三：从源码运行（开发）

```powershell
git clone <你的仓库 URL>
cd FolderHub

dotnet build src\FolderHub.sln
dotnet run --project src\FolderHub.App

# 调试时启动即显示主窗口（否则默认仅托盘）
dotnet run --project src\FolderHub.App -- --show
```

---

## 如何使用

### 首次启动

1. 启动后图标出现在 **通知区域（托盘）**；主界面默认可能隐藏。
2. **双击托盘图标** 或右键托盘 → **「打开主界面」**。
3. 在 **设置** 中可按需开启：开机启动、主组件固定到桌面、透明度、退出后保留卡片等。

### 管理文件夹

1. 左侧选择或新建 **分类**（侧栏支持分类排序）。
2. 点击 **添加文件夹**，或从资源管理器 **拖拽文件夹** 到右侧区域。
3. 在卡片上 **右键**：打开文件夹、编辑记录、重新指定路径、固定到桌面、删除记录。
4. 使用顶栏 **搜索框** 按名称或别名过滤当前视图。
5. **双击** 文件夹卡片 → 用默认资源管理器打开路径。

### 独立桌面卡片

1. 文件夹右键 → **「固定到桌面」**。
2. 拖动卡片调整位置；在卡片或设置中可 **锁定** 位置。
3. 主程序退出后，若设置中 **「退出主程序后保留独立卡片」** 为开启，卡片仍可显示（由 `FolderHub.CardHost` 进程承载）。

### 隐藏与退出

- 主窗口标题栏 **「—」或「×」**：隐藏主组件，**不退出** 程序（托盘仍在）。
- 托盘 → **「退出应用」**：结束主程序；是否关闭独立卡片由上述全局设置决定。

### 恢复卡片

若桌面卡片因 Explorer 重启等原因异常，托盘菜单可使用 **「恢复桌面卡片」**。

---

## 开发与测试

```powershell
dotnet build src\FolderHub.sln
dotnet test src\FolderHub.sln
```

打包与兼容冒烟说明：`docs/05_Development/WP10_打包与兼容冒烟.md`。

```powershell
.\packaging\Invoke-CompatSmoke.ps1 -PublishDir .\artifacts\publish\win-x64
```

---

## 仓库结构

| 路径 | 说明 |
|---|---|
| `src/FolderHub.App` | 主程序（托盘、主面板、设置） |
| `src/FolderHub.CardHost` | 独立桌面卡片宿主 |
| `src/FolderHub.Domain` / `UseCases` / `Infrastructure` | 领域与应用层 |
| `tests/` | 单元测试 |
| `packaging/` | 发布、安装、卸载、冒烟脚本 |
| `docs/` | 需求（SRS）、架构、设计 Demo、开发计划 |
| `assets/` | 图标与界面资源 |

---

## 参与与反馈

欢迎通过本仓库 **Issues** 提交缺陷与功能建议；Pull Request 前请先 `dotnet test` 通过。

需求基线与变更流程见 `docs/01_Requirements/`。

---

## 许可证

发布到 GitHub 前请在仓库根目录添加 `LICENSE` 并在此处写明许可类型（当前仓库尚未包含许可证文件）。
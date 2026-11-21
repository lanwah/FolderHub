# FolderHub

Windows 文件夹快速访问工具：桌面主组件 + 独立文件夹卡片。

## 当前工程状态

- 需求输入：`REQ-BL-1.0`（`docs/01_Requirements/SRS_FolderHub_v0.11.md`）
- 交互参考：`docs/design-demo/`（`p3-demo-v0.1` 已评审）
- 开发计划：`docs/05_Development/开发计划_v0.1.md`（WP0～WP10 DONE）
- 技术栈假定：WPF + .NET 8（`net8.0-windows`），Main + CardHost 双进程，SQLite，Named Pipes
- 安全门禁：Named Pipes NP PoC 未完成前，仅本机开发/冒烟；禁止对外部署/发布

## 构建与运行

```powershell
dotnet build src\FolderHub.sln
dotnet test src\FolderHub.sln
dotnet run --project src\FolderHub.App
# 调试时直接显示主窗口：
dotnet run --project src\FolderHub.App -- --show
```

## 打包（WP10 骨架）

```powershell
.\packaging\Publish-FolderHub.ps1
.\packaging\Install-FolderHub.ps1 -SourceDir .\artifacts\publish\win-x64
.\packaging\Invoke-CompatSmoke.ps1 -PublishDir .\artifacts\publish\win-x64
```

说明见 `docs/05_Development/WP10_打包与兼容冒烟.md`。

## 目录

- `src/`：应用程序源代码
- `tests/`：单元测试（Domain / Infrastructure）
- `packaging/`：发布 / 安装 / 兼容冒烟脚本
- `docs/`：需求、架构、Demo 与开发文档

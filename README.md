# FolderHub

Windows 文件夹快速访问工具：桌面主组件 + 独立文件夹卡片。

## 当前工程状态

- 需求输入：`REQ-BL-1.0`（`docs/01_Requirements/SRS_FolderHub_v0.11.md`）
- 交互参考：`docs/design-demo/`（`p3-demo-v0.1` 已评审）
- 开发计划：`docs/05_Development/开发计划_v0.1.md`（WP0～WP3 DONE；下一步 WP4）
- 技术栈假定：WPF + .NET 8（`net8.0-windows`），Main + CardHost 双进程，SQLite，Named Pipes
- Main 主界面已可运行（分类/列表/搜索/表单/设置壳 + 本地库）

## 构建与运行

```powershell
dotnet build src\FolderHub.sln
dotnet test src\FolderHub.sln
dotnet run --project src\FolderHub.App
```

## 目录

- `src/`：应用程序源代码
- `tests/`：单元测试（Domain / Infrastructure）
- `docs/`：需求、架构、Demo 与开发文档

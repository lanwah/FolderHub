namespace FolderHub.UseCases.Shell;

/// <summary>
/// Windows Shell 适配端口：路径可访问性检查与默认资源管理器打开（FR-009～011；BR-009/010）。
/// </summary>
public interface IShellFolderService
{
    /// <summary>
    /// 路径是否为可访问的本地目录。主要在用户尝试打开时调用（BR-010）。
    /// </summary>
    bool IsDirectoryAccessible(string path);

    /// <summary>
    /// 用系统默认文件资源管理器打开目录。成功返回 true。
    /// </summary>
    bool TryOpenInExplorer(string path, out string? error);
}

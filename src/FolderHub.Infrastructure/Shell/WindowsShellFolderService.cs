using System.Diagnostics;
using FolderHub.UseCases.Shell;

namespace FolderHub.Infrastructure.Shell;

/// <summary>
/// 基于 Directory / explorer.exe 的 Shell 适配实现。
/// </summary>
public sealed class WindowsShellFolderService : IShellFolderService
{
    public bool IsDirectoryAccessible(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            return Directory.Exists(path.Trim());
        }
        catch
        {
            return false;
        }
    }

    public bool TryOpenInExplorer(string path, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(path))
        {
            error = "路径为空。";
            return false;
        }

        var trimmed = path.Trim();
        if (!IsDirectoryAccessible(trimmed))
        {
            error = "目标路径不可用或不是目录。";
            return false;
        }

        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = QuoteForExplorer(trimmed),
                UseShellExecute = true
            });

            if (process is null)
            {
                error = "未能启动资源管理器。";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static string QuoteForExplorer(string path)
    {
        // explorer.exe 对含空格路径需要引号；避免重复包裹。
        if (path.StartsWith('"') && path.EndsWith('"'))
            return path;
        return $"\"{path}\"";
    }
}

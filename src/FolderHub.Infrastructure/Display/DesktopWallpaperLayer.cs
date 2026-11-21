using System.Runtime.InteropServices;

namespace FolderHub.Infrastructure.Display;

/// <summary>
/// 桌面壁纸层（WorkerW）嵌入 spike（UIR-001）。
/// 官方未承诺跨版本稳定；失败时调用方应回退为普通窗口并提示。
/// </summary>
public sealed class DesktopWallpaperLayer
{
    private const uint SmtoNormal = 0x0000;
    private const uint WorkerWSpawnMessage = 0x052C;

    public bool TryAttach(IntPtr hwnd, out string? error)
    {
        error = null;
        if (hwnd == IntPtr.Zero)
        {
            error = "窗口句柄无效。";
            return false;
        }

        try
        {
            var worker = FindWallpaperWorker();
            if (worker == IntPtr.Zero)
            {
                error = "未找到桌面 WorkerW（本机可能不支持壁纸层嵌入）。";
                return false;
            }

            // 顶层窗成功时 previous parent 常为 NULL；必须先清 LastError 再判定。
            SetLastError(0);
            _ = SetParent(hwnd, worker);
            var err = Marshal.GetLastWin32Error();
            if (err != 0)
            {
                error = $"SetParent 失败（Win32 {err}），无法嵌入壁纸层。";
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

    public bool TryDetach(IntPtr hwnd, out string? error)
    {
        error = null;
        if (hwnd == IntPtr.Zero)
        {
            error = "窗口句柄无效。";
            return false;
        }

        try
        {
            SetParent(hwnd, IntPtr.Zero);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static IntPtr FindWallpaperWorker()
    {
        var progman = FindWindow("Progman", null);
        if (progman == IntPtr.Zero)
            return IntPtr.Zero;

        // 触发 Explorer 创建 WorkerW（常见桌面嵌入技巧；跨版本不稳定）。
        SendMessageTimeout(
            progman,
            WorkerWSpawnMessage,
            new IntPtr(0xD),
            new IntPtr(0x1),
            SmtoNormal,
            1000,
            out _);

        IntPtr wallpaperWorker = IntPtr.Zero;
        IntPtr worker = IntPtr.Zero;
        do
        {
            worker = FindWindowEx(IntPtr.Zero, worker, "WorkerW", null);
            if (worker == IntPtr.Zero)
                break;

            var shellView = FindWindowEx(worker, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (shellView != IntPtr.Zero)
            {
                // 图标宿主 WorkerW 的下一个 WorkerW 常为壁纸层；找不到则失败，勿回退 Progman。
                wallpaperWorker = FindWindowEx(IntPtr.Zero, worker, "WorkerW", null);
                break;
            }
        } while (worker != IntPtr.Zero);

        return wallpaperWorker;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("kernel32.dll")]
    private static extern void SetLastError(uint dwErrCode);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(
        IntPtr hWnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam,
        uint fuFlags,
        uint uTimeout,
        out IntPtr lpdwResult);
}

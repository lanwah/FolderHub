using System.Windows;
using System.Windows.Interop;
using FolderHub.Infrastructure.Display;

namespace FolderHub.App.Services;

/// <summary>
/// 主组件壁纸层嵌入编排（UIR-001 spike）。失败时保持普通窗口，不抛异常。
/// </summary>
public sealed class MainDesktopHost
{
    private readonly DesktopWallpaperLayer _layer = new();
    private bool _attached;

    public bool IsAttached => _attached;

    /// <summary>
    /// 按固定状态附加/卸下壁纸层。返回失败说明（成功为 null）。
    /// </summary>
    public string? ApplyPinnedState(Window window, bool pinned)
    {
        ArgumentNullException.ThrowIfNull(window);
        var hwnd = new WindowInteropHelper(window).EnsureHandle();

        if (pinned)
        {
            if (_attached)
                return null;

            if (_layer.TryAttach(hwnd, out var error))
            {
                _attached = true;
                return null;
            }

            return error ?? "壁纸层嵌入失败。";
        }

        if (!_attached)
            return null;

        _layer.TryDetach(hwnd, out _);
        _attached = false;
        return null;
    }
}

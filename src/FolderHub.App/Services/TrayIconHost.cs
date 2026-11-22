namespace FolderHub.App.Services;

/// <summary>
/// 系统托盘常驻入口（FR-012 / ADR-004 / ADR-012）。
/// 右键弹出 WPF 托盘菜单（对齐 design-demo），不再使用系统 ContextMenuStrip。
/// </summary>
public sealed class TrayIconHost : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
    private readonly System.Drawing.Icon _icon;
    private TrayMenuWindow? _menu;
    private DateTime _suppressOpenUntilUtc;
    private bool _disposed;

    public TrayIconHost()
    {
        _icon = CreateIcon();
        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = _icon,
            Text = "FolderHub",
            Visible = true
        };
        _notifyIcon.MouseUp += NotifyIcon_MouseUp;
        _notifyIcon.DoubleClick += (_, _) => OpenMainRequested?.Invoke();
    }

    public event Action? OpenMainRequested;
    public event Action? OpenSettingsRequested;
    public event Action? RecoverCardsRequested;
    public event Action? ExitRequested;

    public void ShowBalloon(string title, string text, System.Windows.Forms.ToolTipIcon icon = System.Windows.Forms.ToolTipIcon.Warning)
    {
        if (_disposed)
            return;

        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = text;
        _notifyIcon.BalloonTipIcon = icon;
        _notifyIcon.ShowBalloonTip(8000);
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        CloseMenu();
        _notifyIcon.MouseUp -= NotifyIcon_MouseUp;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon.Dispose();
    }

    private void NotifyIcon_MouseUp(object? sender, System.Windows.Forms.MouseEventArgs e)
    {
        if (_disposed || e.Button != System.Windows.Forms.MouseButtons.Right)
            return;

        var screenX = System.Windows.Forms.Control.MousePosition.X;
        var screenY = System.Windows.Forms.Control.MousePosition.Y;
        RunOnUi(() => ShowMenu(screenX, screenY));
    }

    private void ShowMenu(int screenX, int screenY)
    {
        if (_disposed)
            return;

        // 点托盘图标关闭菜单时，同一右键会先 Deactivated 再 MouseUp；短时抑制避免立刻重开。
        if (_menu is not null)
        {
            CloseMenu();
            return;
        }

        if (DateTime.UtcNow < _suppressOpenUntilUtc)
            return;

        var menu = new TrayMenuWindow();
        menu.OpenMainRequested += () => OpenMainRequested?.Invoke();
        menu.OpenSettingsRequested += () => OpenSettingsRequested?.Invoke();
        menu.RecoverCardsRequested += () => RecoverCardsRequested?.Invoke();
        menu.ExitRequested += () => ExitRequested?.Invoke();
        menu.Closed += (_, _) =>
        {
            if (ReferenceEquals(_menu, menu))
                _menu = null;
            _suppressOpenUntilUtc = DateTime.UtcNow.AddMilliseconds(280);
        };

        _menu = menu;
        menu.ShowNearScreenPoint(screenX, screenY);
    }

    private void CloseMenu()
    {
        var menu = _menu;
        _menu = null;
        _suppressOpenUntilUtc = DateTime.UtcNow.AddMilliseconds(280);
        menu?.RequestClose();
    }

    private static void RunOnUi(Action action)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null)
            return;

        if (dispatcher.CheckAccess())
            action();
        else
            dispatcher.BeginInvoke(action);
    }

    private static System.Drawing.Icon CreateIcon()
    {
        try
        {
            var path = Environment.ProcessPath;
            if (!string.IsNullOrWhiteSpace(path))
            {
                var extracted = System.Drawing.Icon.ExtractAssociatedIcon(path);
                if (extracted is not null)
                    return extracted;
            }
        }
        catch
        {
            // fall through
        }

        return (System.Drawing.Icon)System.Drawing.SystemIcons.Application.Clone();
    }
}

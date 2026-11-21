namespace FolderHub.App.Services;

/// <summary>
/// 系统托盘常驻入口（FR-012 / ADR-004 / ADR-012）。
/// </summary>
public sealed class TrayIconHost : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
    private readonly System.Drawing.Icon _icon;
    private bool _disposed;

    public TrayIconHost()
    {
        _icon = CreateIcon();
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("打开主界面", null, (_, _) => OpenMainRequested?.Invoke());
        menu.Items.Add("设置", null, (_, _) => OpenSettingsRequested?.Invoke());
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("退出应用", null, (_, _) => ExitRequested?.Invoke());

        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = _icon,
            Text = "FolderHub",
            Visible = true,
            ContextMenuStrip = menu
        };
        _notifyIcon.DoubleClick += (_, _) => OpenMainRequested?.Invoke();
    }

    public event Action? OpenMainRequested;
    public event Action? OpenSettingsRequested;
    public event Action? ExitRequested;

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _icon.Dispose();
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

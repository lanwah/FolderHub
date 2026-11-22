using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace FolderHub.App;

/// <summary>托盘右键菜单（对齐 design-demo .tray-popover）。</summary>
public partial class TrayMenuWindow : Window
{
    private bool _closing;

    public TrayMenuWindow()
    {
        InitializeComponent();
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
                SetForegroundWindow(hwnd);
        };
    }

    public event Action? OpenMainRequested;
    public event Action? OpenSettingsRequested;
    public event Action? RecoverCardsRequested;
    public event Action? ExitRequested;

    /// <summary>在屏幕物理像素坐标附近弹出，菜单贴在指针上方。</summary>
    public void ShowNearScreenPoint(int screenX, int screenY)
    {
        Opacity = 0;
        Show();

        var dpi = VisualTreeHelper.GetDpi(this);
        var scaleX = dpi.DpiScaleX;
        var scaleY = dpi.DpiScaleY;

        Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var width = DesiredSize.Width;
        var height = DesiredSize.Height;

        var left = screenX / scaleX - width + 16;
        var top = screenY / scaleY - height + 8;
        var work = SystemParameters.WorkArea;
        if (left + width > work.Right)
            left = work.Right - width;
        if (left < work.Left)
            left = work.Left;
        if (top + height > work.Bottom)
            top = work.Bottom - height;
        if (top < work.Top)
            top = work.Top;

        Left = left;
        Top = top;
        Opacity = 1;
        Activate();
    }

    public void RequestClose()
    {
        if (_closing)
            return;
        _closing = true;
        Close();
    }

    private void Window_Deactivated(object? sender, EventArgs e)
        => RequestClose();

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            RequestClose();
        }
    }

    private void OpenMain_Click(object sender, RoutedEventArgs e)
        => RaiseAndClose(OpenMainRequested);

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
        => RaiseAndClose(OpenSettingsRequested);

    private void RecoverCards_Click(object sender, RoutedEventArgs e)
        => RaiseAndClose(RecoverCardsRequested);

    private void Exit_Click(object sender, RoutedEventArgs e)
        => RaiseAndClose(ExitRequested);

    private void RaiseAndClose(Action? handler)
    {
        RequestClose();
        handler?.Invoke();
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}

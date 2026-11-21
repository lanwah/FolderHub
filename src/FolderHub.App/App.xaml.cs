using System.Threading;
using System.Windows;
using FolderHub.App.Services;
using FolderHub.App.ViewModels;

namespace FolderHub.App;

public partial class App : Application
{
    private const string SingleInstanceMutexName = @"Local\FolderHub.App.SingleInstance";
    private const string ShowMainEventName = @"Local\FolderHub.App.ShowMain";

    private Mutex? _singleInstanceMutex;
    private EventWaitHandle? _showMainEvent;
    private CancellationTokenSource? _showMainWatchCts;
    private AppSession? _session;
    private MainViewModel? _viewModel;
    private MainWindow? _mainWindow;
    private TrayIconHost? _tray;
    private bool _exiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!TryAcquireSingleInstance())
        {
            TrySignalExistingInstanceToShow();
            Shutdown(0);
            return;
        }

        try
        {
            _session = AppSession.OpenDefault();
            _session.SyncStartupRegistration();

            _viewModel = new MainViewModel(_session);
            _mainWindow = new MainWindow();
            _mainWindow.Attach(_viewModel);
            _mainWindow.Closing += MainWindow_Closing;
            _mainWindow.StateChanged += MainWindow_StateChanged;
            MainWindow = _mainWindow;

            _tray = new TrayIconHost();
            _tray.OpenMainRequested += () => DispatchToUi(ShowMainWindow);
            _tray.OpenSettingsRequested += () => DispatchToUi(OpenSettingsFromTray);
            _tray.ExitRequested += () => DispatchToUi(ExitApplication);

            StartShowMainWatcher();

            // ADR-012：启动后主组件默认隐藏，仅托盘常驻；`--show` 便于开发调试。
            if (ShouldShowMainOnStartup(e.Args))
                ShowMainWindow();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"无法启动 FolderHub：\n{ex.Message}",
                "FolderHub",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _showMainWatchCts?.Cancel();
        _showMainWatchCts?.Dispose();
        _showMainEvent?.Dispose();
        _tray?.Dispose();
        _session?.Dispose();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private bool TryAcquireSingleInstance()
    {
        _singleInstanceMutex = new Mutex(initiallyOwned: true, SingleInstanceMutexName, out var createdNew);
        if (createdNew)
            return true;

        _singleInstanceMutex.Dispose();
        _singleInstanceMutex = null;
        return false;
    }

    private static void TrySignalExistingInstanceToShow()
    {
        try
        {
            using var showEvent = EventWaitHandle.OpenExisting(ShowMainEventName);
            showEvent.Set();
        }
        catch (WaitHandleCannotBeOpenedException)
        {
            // 既有实例尚未就绪或已退出；忽略。
        }
        catch (UnauthorizedAccessException)
        {
            // 会话隔离下无权通知；忽略。
        }
    }

    private void StartShowMainWatcher()
    {
        _showMainEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ShowMainEventName);
        _showMainWatchCts = new CancellationTokenSource();
        var token = _showMainWatchCts.Token;

        _ = Task.Run(() =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_showMainEvent.WaitOne(500))
                        DispatchToUi(ShowMainWindow);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }, token);
    }

    private void DispatchToUi(Action action)
    {
        if (Dispatcher.CheckAccess())
            action();
        else
            Dispatcher.BeginInvoke(action);
    }

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_exiting || _mainWindow is null)
            return;

        // 关闭按钮 = 隐藏到托盘，不退出进程（FR-012）。
        e.Cancel = true;
        _mainWindow.Hide();
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (_exiting || _mainWindow is null)
            return;

        // 最小化与关闭一致：进入托盘，不占用任务栏按钮。
        if (_mainWindow.WindowState == WindowState.Minimized)
        {
            _mainWindow.WindowState = WindowState.Normal;
            _mainWindow.Hide();
        }
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
            return;

        if (!_mainWindow.IsVisible)
            _mainWindow.Show();

        if (_mainWindow.WindowState == WindowState.Minimized)
            _mainWindow.WindowState = WindowState.Normal;

        _mainWindow.Activate();
        _mainWindow.Topmost = true;
        _mainWindow.Topmost = false;
        _mainWindow.Focus();
    }

    private void OpenSettingsFromTray()
    {
        if (_mainWindow is null)
            return;

        ShowMainWindow();
        _mainWindow.OpenSettings();
    }

    private void ExitApplication()
    {
        if (_exiting)
            return;

        _exiting = true;
        _mainWindow?.PersistBoundsIfNeeded();
        _tray?.Dispose();
        _tray = null;
        Shutdown(0);
    }

    private static bool ShouldShowMainOnStartup(string[] args)
        => args.Any(a => string.Equals(a, "--show", StringComparison.OrdinalIgnoreCase));
}

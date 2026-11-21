using System.Threading;
using System.Windows;
using FolderHub.CardHost.Services;
using FolderHub.Infrastructure.Ipc;

namespace FolderHub.CardHost;

public partial class App : Application
{
    private const string SingleInstanceMutexName = @"Local\FolderHub.CardHost.SingleInstance";

    private Mutex? _singleInstanceMutex;
    private CardHostRuntime? _runtime;
    private CardHostController? _controller;
    private CancellationTokenSource? _ipcCts;
    private bool _exiting;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (!TryAcquireSingleInstance())
        {
            Shutdown(0);
            return;
        }

        try
        {
            if (!TryResolveIpcOptions(e.Args, out var options) || options is null)
            {
                MessageBox.Show(
                    "CardHost 需要由 FolderHub 主程序启动（缺少 IPC 启动参数/会话）。",
                    "FolderHub CardHost",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Shutdown(1);
                return;
            }

            _runtime = CardHostRuntime.OpenDefault(options);
            _controller = new CardHostController(_runtime);
            _controller.ShutdownRequested += () => Dispatcher.BeginInvoke(ExitHost);

            CardHostSessionFile.Write(new CardHostSessionRecord(
                options.HostInstanceId,
                options.InstanceToken,
                options.SessionId,
                Environment.ProcessId));

            _controller.LoadEnabledCardsFromStore();

            _ipcCts = new CancellationTokenSource();
            // 避免 async void OnStartup：同步启动后在后台跑 IPC 接受循环。
            _ = StartIpcSafeAsync(_ipcCts.Token);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"无法启动 CardHost：\n{ex.Message}",
                "FolderHub CardHost",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (!_exiting)
            _exiting = true;

        _ipcCts?.Cancel();
        if (_controller is not null)
        {
            try
            {
                // 退出路径同步等待，避免 async void OnExit 被运行时截断。
                _controller.StopIpcAsync().GetAwaiter().GetResult();
            }
            catch
            {
                // ignore
            }

            _controller.CloseAllWindows();
        }

        CardHostSessionFile.TryDelete();
        _runtime?.Dispose();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    private async Task StartIpcSafeAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_controller is null)
                return;
            await _controller.StartIpcAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                MessageBox.Show(
                    $"CardHost IPC 启动失败：\n{ex.Message}",
                    "FolderHub CardHost",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                ExitHost();
            });
        }
    }

    private void ExitHost()
    {
        if (_exiting)
            return;
        _exiting = true;
        Shutdown(0);
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

    private static bool TryResolveIpcOptions(string[] args, out IpcConnectionOptions? options)
        => CardHostLaunchArgs.TryParse(args, out options) && options is not null;
}

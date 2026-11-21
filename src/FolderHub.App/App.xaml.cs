using System.Windows;
using FolderHub.App.Services;
using FolderHub.App.ViewModels;

namespace FolderHub.App;

public partial class App : Application
{
    private AppSession? _session;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _session = AppSession.OpenDefault();
            var viewModel = new MainViewModel(_session);
            var window = new MainWindow();
            window.Attach(viewModel);
            MainWindow = window;
            window.Show();
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
        _session?.Dispose();
        base.OnExit(e);
    }
}

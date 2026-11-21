using FolderHub.UseCases.Shell;
using Microsoft.Win32;

namespace FolderHub.Infrastructure.Shell;

/// <summary>
/// 通过 HKCU Run 注册开机启动（单用户交互式桌面；无需管理员）。
/// </summary>
public sealed class WindowsStartupRegistrationService : IStartupRegistrationService
{
    public const string DefaultValueName = "FolderHub";
    public const string DefaultRunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    private readonly string _valueName;
    private readonly string _runKeyPath;
    private readonly Func<string> _commandLineFactory;

    public WindowsStartupRegistrationService(
        Func<string>? commandLineFactory = null,
        string? valueName = null,
        string? runKeyPath = null)
    {
        _commandLineFactory = commandLineFactory ?? BuildDefaultCommandLine;
        _valueName = string.IsNullOrWhiteSpace(valueName) ? DefaultValueName : valueName.Trim();
        _runKeyPath = string.IsNullOrWhiteSpace(runKeyPath) ? DefaultRunKeyPath : runKeyPath.Trim();
    }

    public bool IsRegistered()
    {
        using var key = Registry.CurrentUser.OpenSubKey(_runKeyPath, writable: false);
        if (key is null)
            return false;

        return key.GetValue(_valueName) is string s && !string.IsNullOrWhiteSpace(s);
    }

    public void Apply(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(_runKeyPath, writable: true)
            ?? throw new InvalidOperationException($"无法打开或创建注册表键：HKCU\\{_runKeyPath}");

        if (enabled)
        {
            var command = _commandLineFactory();
            if (string.IsNullOrWhiteSpace(command))
                throw new InvalidOperationException("启动命令行为空，无法注册开机启动。");

            key.SetValue(_valueName, command.Trim(), RegistryValueKind.String);
        }
        else
        {
            key.DeleteValue(_valueName, throwOnMissingValue: false);
        }
    }

    private static string BuildDefaultCommandLine()
    {
        var path = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(path))
            throw new InvalidOperationException("无法解析当前进程路径。");

        return Quote(path);
    }

    private static string Quote(string path)
        => path.StartsWith('"') ? path : $"\"{path}\"";
}

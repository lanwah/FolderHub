namespace FolderHub.UseCases.Ipc;

/// <summary>
/// Main→CardHost HostHello 载荷。
/// action=detach 表示受控断开（Keep=ON）；action=set-theme 同步界面主题。
/// </summary>
public sealed record HostHelloPayload(string Action, string? UiTheme = null)
{
    public const string Detach = "detach";
    public const string SetTheme = "set-theme";
}

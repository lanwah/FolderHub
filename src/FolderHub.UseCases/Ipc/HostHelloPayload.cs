namespace FolderHub.UseCases.Ipc;

/// <summary>
/// Main→CardHost HostHello 载荷。action=detach 表示受控断开（Keep=ON），非关机。
/// </summary>
public sealed record HostHelloPayload(string Action)
{
    public const string Detach = "detach";
}

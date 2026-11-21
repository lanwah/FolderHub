using FolderHub.Infrastructure.Ipc;

namespace FolderHub.Infrastructure.Tests;

public sealed class CardHostLaunchArgsTests
{
    [Fact]
    public void BuildAndParse_RoundTrip()
    {
        var identity = WindowsSessionIdentity.CaptureCurrent();
        var options = IpcConnectionOptions.CreateNew(identity);
        var args = CardHostLaunchArgs.Build(options);

        Assert.True(CardHostLaunchArgs.TryParse(args, out var parsed));
        Assert.NotNull(parsed);
        Assert.Equal(options.HostInstanceId, parsed!.HostInstanceId);
        Assert.Equal(options.InstanceToken, parsed.InstanceToken);
        Assert.Equal(options.SessionId, parsed.SessionId);
    }

    [Fact]
    public void TryParse_RejectsIncompleteArgs()
    {
        Assert.False(CardHostLaunchArgs.TryParse(["--ipc-host-instance-id", Guid.NewGuid().ToString("D")], out _));
    }
}

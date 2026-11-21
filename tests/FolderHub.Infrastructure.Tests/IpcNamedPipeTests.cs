using FolderHub.Infrastructure.Ipc;
using FolderHub.UseCases.Ipc;

namespace FolderHub.Infrastructure.Tests;

public sealed class IpcNamedPipeTests
{
    [Fact]
    public void CaptureCurrent_ReturnsSessionAndSids()
    {
        var identity = WindowsSessionIdentity.CaptureCurrent();
        Assert.True(identity.SessionId >= 0);
        Assert.False(string.IsNullOrWhiteSpace(identity.UserSidString));
        Assert.False(string.IsNullOrWhiteSpace(identity.LogonSidString));
        Assert.StartsWith("S-1-", identity.UserSidString);
        Assert.StartsWith("S-1-", identity.LogonSidString);
    }

    [Fact]
    public void EndpointNaming_IncludesSessionAndInstance()
    {
        var id = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var name = NamedPipeEndpointNaming.BuildPipeName(3, id);
        Assert.Equal("FolderHub.Ipc.s3.11111111222233334444555555555555", name);
        Assert.Equal(@"\\.\pipe\" + name, NamedPipeEndpointNaming.ToPipePath(name));
    }

    [Fact]
    public void Handshake_ComputeResponse_IsDeterministic()
    {
        var a = IpcHandshake.ComputeResponse("challenge", "token");
        var b = IpcHandshake.ComputeResponse("challenge", "token");
        var c = IpcHandshake.ComputeResponse("challenge", "other");
        Assert.Equal(a, b);
        Assert.NotEqual(a, c);
    }

    [Fact]
    public async Task NamedPipe_HandshakeAndPingPong_Succeeds()
    {
        var identity = WindowsSessionIdentity.CaptureCurrent();
        var options = IpcConnectionOptions.CreateNew(identity);

        await using var server = new IpcNamedPipeServer(options, identity);
        await using var client = new IpcNamedPipeClient(options, identity);

        var serverTask = server.WaitForConnectionAndHandshakeAsync();
        // 稍候再连，避免竞态连到未就绪管道
        await Task.Delay(50);
        await client.ConnectAndHandshakeAsync();
        await serverTask;

        Assert.True(server.IsConnected);
        Assert.True(client.IsConnected);

        var ping = IpcEnvelope.Create(
            IpcMessageType.Ping,
            IpcSenderRole.MainProcess,
            options.SessionId,
            options.HostInstanceId,
            payloadJson: "{\"t\":\"wp6\"}");

        var receiveTask = server.ReceiveAsync();
        await client.SendAsync(ping);
        var received = await receiveTask;

        Assert.Equal(IpcMessageType.Ping, received.MessageType);
        Assert.Equal(ping.MessageId, received.MessageId);

        var pong = IpcEnvelope.Create(
            IpcMessageType.Pong,
            IpcSenderRole.CardHost,
            options.SessionId,
            options.HostInstanceId,
            correlationId: received.MessageId);

        var clientReceive = client.ReceiveAsync();
        await server.SendAsync(pong);
        var pongReceived = await clientReceive;
        Assert.Equal(IpcMessageType.Pong, pongReceived.MessageType);
        Assert.Equal(received.MessageId, pongReceived.CorrelationId);
    }

    [Fact]
    public async Task NamedPipe_WrongInstanceToken_FailsHandshake()
    {
        var identity = WindowsSessionIdentity.CaptureCurrent();
        var serverOptions = IpcConnectionOptions.CreateNew(identity);
        var clientOptions = new IpcConnectionOptions
        {
            HostInstanceId = serverOptions.HostInstanceId,
            SessionId = serverOptions.SessionId,
            InstanceToken = "deadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef",
            Timeout = TimeSpan.FromSeconds(3)
        };

        await using var server = new IpcNamedPipeServer(serverOptions, identity);
        await using var client = new IpcNamedPipeClient(clientOptions, identity);

        var serverTask = server.WaitForConnectionAndHandshakeAsync();
        await Task.Delay(50);

        await Assert.ThrowsAnyAsync<Exception>(async () =>
        {
            try
            {
                await client.ConnectAndHandshakeAsync();
                await serverTask;
            }
            catch
            {
                // 任一侧失败即可
                throw;
            }
        });
    }
}

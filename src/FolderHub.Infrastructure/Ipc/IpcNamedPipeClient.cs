using System.IO.Pipes;
using FolderHub.UseCases.Ipc;

namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// Main Process 侧 Named Pipe 客户端：连接后执行完整握手。
/// </summary>
public sealed class IpcNamedPipeClient : IAsyncDisposable
{
    private readonly IpcConnectionOptions _options;
    private readonly WindowsSessionIdentity _identity;
    private NamedPipeClientStream? _pipe;
    private bool _connected;

    public IpcNamedPipeClient(IpcConnectionOptions options, WindowsSessionIdentity? identity = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _identity = identity ?? WindowsSessionIdentity.CaptureCurrent();
        if (_options.SessionId != _identity.SessionId)
            throw new ArgumentException("连接选项 SessionId 与当前会话不一致。", nameof(options));
    }

    public string PipeName => NamedPipeEndpointNaming.BuildPipeName(_options.SessionId, _options.HostInstanceId);

    public bool IsConnected => _connected && _pipe is { IsConnected: true };

    public async Task ConnectAndHandshakeAsync(CancellationToken cancellationToken = default)
    {
        if (_pipe is not null)
            throw new InvalidOperationException("客户端已启动。");

        var pipe = new NamedPipeClientStream(
            serverName: ".",
            pipeName: PipeName,
            direction: PipeDirection.InOut,
            options: PipeOptions.Asynchronous);

        _pipe = pipe;
        try
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            linked.CancelAfter(_options.Timeout);
            await pipe.ConnectAsync(linked.Token).ConfigureAwait(false);

            await IpcHandshake.PerformAsClientAsync(pipe, _options, _identity, cancellationToken)
                .ConfigureAwait(false);
            _connected = true;
        }
        catch
        {
            _connected = false;
            _pipe = null;
            await pipe.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public Task SendAsync(IpcEnvelope envelope, CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        return IpcFrameCodec.WriteFrameAsync(_pipe!, envelope, cancellationToken);
    }

    public Task<IpcEnvelope> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        return IpcFrameCodec.ReadFrameAsync(_pipe!, cancellationToken);
    }

    private void EnsureConnected()
    {
        if (!IsConnected)
            throw new InvalidOperationException("管道未连接或握手未完成。");
    }

    public async ValueTask DisposeAsync()
    {
        _connected = false;
        if (_pipe is not null)
        {
            await _pipe.DisposeAsync().ConfigureAwait(false);
            _pipe = null;
        }
    }
}

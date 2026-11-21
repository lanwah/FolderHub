using System.IO.Pipes;
using FolderHub.UseCases.Ipc;

namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// CardHost 侧 Named Pipe 服务端：显式 DACL + 握手（远程客户端由运行时默认拒绝）。
/// </summary>
public sealed class IpcNamedPipeServer : IAsyncDisposable
{
    private readonly IpcConnectionOptions _options;
    private readonly WindowsSessionIdentity _identity;
    private NamedPipeServerStream? _pipe;
    private bool _connected;

    public IpcNamedPipeServer(IpcConnectionOptions options, WindowsSessionIdentity? identity = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _identity = identity ?? WindowsSessionIdentity.CaptureCurrent();
        if (_options.SessionId != _identity.SessionId)
            throw new ArgumentException("连接选项 SessionId 与当前会话不一致。", nameof(options));
    }

    public string PipeName => NamedPipeEndpointNaming.BuildPipeName(_options.SessionId, _options.HostInstanceId);

    public bool IsConnected => _connected && _pipe is { IsConnected: true };

    public async Task WaitForConnectionAndHandshakeAsync(CancellationToken cancellationToken = default)
    {
        if (_pipe is not null)
            throw new InvalidOperationException("服务端已启动。");

        var security = NamedPipeSecurityFactory.CreateForCurrentLogon(_identity);
        var pipe = NamedPipeServerStreamAcl.Create(
            PipeName,
            PipeDirection.InOut,
            maxNumberOfServerInstances: 1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            inBufferSize: 4096,
            outBufferSize: 4096,
            pipeSecurity: security);

        _pipe = pipe;
        try
        {
            await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            await IpcHandshake.PerformAsServerAsync(pipe, _options, _identity, cancellationToken)
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

using System.Buffers.Binary;
using FolderHub.UseCases.Ipc;

namespace FolderHub.Infrastructure.Tests;

public sealed class IpcFrameCodecTests
{
    [Fact]
    public void EncodeDecode_RoundTripsEnvelope()
    {
        var original = IpcEnvelope.Create(
            IpcMessageType.Ping,
            IpcSenderRole.MainProcess,
            sessionId: 1,
            hostInstanceId: Guid.NewGuid(),
            payloadJson: "{\"n\":1}");

        var frame = IpcFrameCodec.Encode(original);
        var decoded = IpcFrameCodec.Decode(frame);

        Assert.Equal(original.MessageId, decoded.MessageId);
        Assert.Equal(original.CorrelationId, decoded.CorrelationId);
        Assert.Equal(original.MessageType, decoded.MessageType);
        Assert.Equal(original.SenderRole, decoded.SenderRole);
        Assert.Equal(original.SessionId, decoded.SessionId);
        Assert.Equal(original.HostInstanceId, decoded.HostInstanceId);
        Assert.Equal(original.PayloadJson, decoded.PayloadJson);
        Assert.Equal(IpcProtocol.Major, decoded.ProtocolMajor);
    }

    [Fact]
    public void Encode_DisallowedDirection_Throws()
    {
        var envelope = IpcEnvelope.Create(
            IpcMessageType.CardStateChangedNotice,
            IpcSenderRole.MainProcess,
            sessionId: 1,
            hostInstanceId: Guid.NewGuid());

        Assert.Throws<IpcProtocolException>(() => IpcFrameCodec.Encode(envelope));
    }

    [Fact]
    public void Decode_OversizedLengthPrefix_Throws()
    {
        var frame = new byte[8];
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(0, 4), (uint)(IpcProtocol.MaxFramePayloadBytes + 1));
        Assert.Throws<IpcProtocolException>(() => IpcFrameCodec.Decode(frame));
    }

    [Fact]
    public void Decode_IncompatibleProtocolMajor_Throws()
    {
        var envelope = new IpcEnvelope
        {
            ProtocolMajor = IpcProtocol.Major + 1,
            ProtocolMinor = 0,
            MessageId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            SessionId = 1,
            HostInstanceId = Guid.NewGuid(),
            SenderRole = IpcSenderRole.MainProcess,
            MessageType = IpcMessageType.Ping
        };

        // Encode bypasses Decode checks; craft payload via Encode after temporarily using compatible version is hard —
        // serialize via a compatible encode then mutate is overkill; decode raw JSON payload path instead.
        var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(envelope, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
        });
        var frame = new byte[4 + json.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(0, 4), (uint)json.Length);
        json.CopyTo(frame.AsSpan(4));

        var ex = Assert.Throws<IpcProtocolException>(() => IpcFrameCodec.Decode(frame));
        Assert.Contains("协议版本不兼容", ex.Message);
    }

    [Fact]
    public async Task WriteRead_Stream_RoundTrips()
    {
        await using var stream = new MemoryStream();
        var envelope = IpcEnvelope.Create(
            IpcMessageType.Pong,
            IpcSenderRole.CardHost,
            sessionId: 2,
            hostInstanceId: Guid.NewGuid());

        await IpcFrameCodec.WriteFrameAsync(stream, envelope, CancellationToken.None);
        stream.Position = 0;
        var decoded = await IpcFrameCodec.ReadFrameAsync(stream, CancellationToken.None);
        Assert.Equal(envelope.MessageId, decoded.MessageId);
        Assert.Equal(IpcMessageType.Pong, decoded.MessageType);
    }
}

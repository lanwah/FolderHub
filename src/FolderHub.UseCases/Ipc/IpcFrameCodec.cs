using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FolderHub.UseCases.Ipc;

/// <summary>
/// 长度前缀帧：uint32 LE + UTF-8 JSON 信封。超限/畸形拒绝。
/// </summary>
public static class IpcFrameCodec
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static byte[] Encode(IpcEnvelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        if (!IpcMessageCatalog.IsAllowed(envelope.SenderRole, envelope.MessageType))
            throw new IpcProtocolException($"消息类型 {envelope.MessageType} 对角色 {envelope.SenderRole} 不在白名单内。");

        var payload = JsonSerializer.SerializeToUtf8Bytes(envelope, JsonOptions);
        if (payload.Length > IpcProtocol.MaxFramePayloadBytes)
            throw new IpcProtocolException($"帧载荷过大：{payload.Length} > {IpcProtocol.MaxFramePayloadBytes}。");

        var frame = new byte[4 + payload.Length];
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(0, 4), (uint)payload.Length);
        payload.CopyTo(frame.AsSpan(4));
        return frame;
    }

    public static IpcEnvelope Decode(ReadOnlySpan<byte> frame)
    {
        if (frame.Length < 4)
            throw new IpcProtocolException("帧过短：缺少长度前缀。");

        var length = BinaryPrimitives.ReadUInt32LittleEndian(frame[..4]);
        if (length > IpcProtocol.MaxFramePayloadBytes)
            throw new IpcProtocolException($"帧长度超限：{length} > {IpcProtocol.MaxFramePayloadBytes}。");

        if (frame.Length != 4 + (int)length)
            throw new IpcProtocolException($"帧长度不一致：声明 {length}，实际 {frame.Length - 4}。");

        return DecodePayload(frame[4..]);
    }

    public static IpcEnvelope DecodePayload(ReadOnlySpan<byte> payload)
    {
        if (payload.IsEmpty)
            throw new IpcProtocolException("空载荷。");

        if (payload.Length > IpcProtocol.MaxFramePayloadBytes)
            throw new IpcProtocolException($"载荷过大：{payload.Length}。");

        IpcEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<IpcEnvelope>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new IpcProtocolException("JSON 信封解析失败。", ex);
        }

        if (envelope is null)
            throw new IpcProtocolException("信封为空。");

        if (!IpcMessageCatalog.IsProtocolCompatible(envelope.ProtocolMajor, envelope.ProtocolMinor))
            throw new IpcProtocolException(
                $"协议版本不兼容：{envelope.ProtocolMajor}.{envelope.ProtocolMinor}。");

        if (!IpcMessageCatalog.IsKnown(envelope.MessageType))
            throw new IpcProtocolException($"未知消息类型：{envelope.MessageType}。");

        if (!IpcMessageCatalog.IsAllowed(envelope.SenderRole, envelope.MessageType))
            throw new IpcProtocolException($"消息类型 {envelope.MessageType} 对角色 {envelope.SenderRole} 不在白名单内。");

        return envelope;
    }

    public static async Task WriteFrameAsync(Stream stream, IpcEnvelope envelope, CancellationToken cancellationToken)
    {
        var frame = Encode(envelope);
        await stream.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<IpcEnvelope> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
    {
        var lengthBuffer = new byte[4];
        await ReadExactAsync(stream, lengthBuffer, cancellationToken).ConfigureAwait(false);

        var length = BinaryPrimitives.ReadUInt32LittleEndian(lengthBuffer);
        if (length == 0)
            throw new IpcProtocolException("帧长度为 0。");
        if (length > IpcProtocol.MaxFramePayloadBytes)
            throw new IpcProtocolException($"帧长度超限：{length}。");

        var payload = new byte[length];
        await ReadExactAsync(stream, payload, cancellationToken).ConfigureAwait(false);
        return DecodePayload(payload);
    }

    private static async Task ReadExactAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken)
                .ConfigureAwait(false);
            if (read == 0)
                throw new EndOfStreamException("管道在完整帧前关闭。");
            offset += read;
        }
    }

    /// <summary>诊断用：不抛出时返回载荷 UTF-8 预览长度。</summary>
    public static int Utf8ByteCount(string text) => Encoding.UTF8.GetByteCount(text);
}

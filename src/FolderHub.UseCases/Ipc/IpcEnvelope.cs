namespace FolderHub.UseCases.Ipc;

/// <summary>
/// 逻辑消息信封（P3-05）。序列化为 JSON 载荷放入长度前缀帧。
/// </summary>
public sealed class IpcEnvelope
{
    public int ProtocolMajor { get; init; } = IpcProtocol.Major;
    public int ProtocolMinor { get; init; } = IpcProtocol.Minor;
    public Guid MessageId { get; init; }
    public Guid CorrelationId { get; init; }
    public int SessionId { get; init; }
    public Guid HostInstanceId { get; init; }
    public IpcSenderRole SenderRole { get; init; }
    public IpcMessageType MessageType { get; init; }
    public string? AggregateId { get; init; }
    public long? ExpectedVersion { get; init; }
    /// <summary>业务 JSON 载荷；握手阶段可放 challenge/nonce。</summary>
    public string? PayloadJson { get; init; }
    public DateTimeOffset SentAt { get; init; } = DateTimeOffset.UtcNow;

    public static IpcEnvelope Create(
        IpcMessageType type,
        IpcSenderRole role,
        int sessionId,
        Guid hostInstanceId,
        string? payloadJson = null,
        Guid? messageId = null,
        Guid? correlationId = null,
        string? aggregateId = null,
        long? expectedVersion = null)
    {
        var id = messageId ?? Guid.NewGuid();
        return new IpcEnvelope
        {
            MessageId = id,
            CorrelationId = correlationId ?? id,
            SessionId = sessionId,
            HostInstanceId = hostInstanceId,
            SenderRole = role,
            MessageType = type,
            AggregateId = aggregateId,
            ExpectedVersion = expectedVersion,
            PayloadJson = payloadJson
        };
    }
}

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FolderHub.UseCases.Ipc;

namespace FolderHub.Infrastructure.Ipc;

/// <summary>
/// 连接后身份握手：协议版本、Session、HostInstance、InstanceToken、challenge-response。
/// </summary>
public static class IpcHandshake
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task PerformAsServerAsync(
        Stream stream,
        IpcConnectionOptions options,
        WindowsSessionIdentity identity,
        CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(options.Timeout);

        var offer = await IpcFrameCodec.ReadFrameAsync(stream, linked.Token).ConfigureAwait(false);
        ValidateOfferFromMain(offer, options, identity);

        var offerPayload = DeserializePayload(offer.PayloadJson)
            ?? throw new IpcProtocolException("HandshakeOffer 缺少载荷。");

        if (!FixedTimeEquals(offerPayload.InstanceToken, options.InstanceToken))
            throw new IpcProtocolException("实例令牌不匹配。");

        var serverChallenge = CreateChallenge();
        var responseToClient = ComputeResponse(offerPayload.Challenge, options.InstanceToken);

        var acceptPayload = new IpcHandshakePayload
        {
            InstanceToken = options.InstanceToken,
            Challenge = serverChallenge,
            ChallengeResponse = responseToClient,
            ProcessName = Process.GetCurrentProcess().ProcessName,
            ProcessId = Environment.ProcessId
        };

        var accept = IpcEnvelope.Create(
            IpcMessageType.HandshakeAccept,
            IpcSenderRole.CardHost,
            options.SessionId,
            options.HostInstanceId,
            SerializePayload(acceptPayload),
            correlationId: offer.MessageId);

        await IpcFrameCodec.WriteFrameAsync(stream, accept, linked.Token).ConfigureAwait(false);

        // 约定：客户端最终确认帧为 HandshakeAccept，载荷含对 serverChallenge 的响应。
        var confirm = await IpcFrameCodec.ReadFrameAsync(stream, linked.Token).ConfigureAwait(false);
        if (confirm.MessageType != IpcMessageType.HandshakeAccept)
            throw new IpcProtocolException($"期望握手确认 HandshakeAccept，收到 {confirm.MessageType}。");
        if (confirm.SenderRole != IpcSenderRole.MainProcess)
            throw new IpcProtocolException("握手确认发送方角色无效。");
        if (!IpcMessageCatalog.IsProtocolCompatible(confirm.ProtocolMajor, confirm.ProtocolMinor))
            throw new IpcProtocolException($"协议版本不兼容：{confirm.ProtocolMajor}.{confirm.ProtocolMinor}。");
        if (confirm.SessionId != options.SessionId || confirm.SessionId != identity.SessionId)
            throw new IpcProtocolException("会话 ID 不匹配。");
        if (confirm.HostInstanceId != options.HostInstanceId)
            throw new IpcProtocolException("HostInstanceId 不匹配。");

        var confirmPayload = DeserializePayload(confirm.PayloadJson)
            ?? throw new IpcProtocolException("握手确认缺少载荷。");
        var expected = ComputeResponse(serverChallenge, options.InstanceToken);
        if (!FixedTimeEquals(confirmPayload.ChallengeResponse, expected))
            throw new IpcProtocolException("握手 challenge 响应无效。");
    }

    public static async Task PerformAsClientAsync(
        Stream stream,
        IpcConnectionOptions options,
        WindowsSessionIdentity identity,
        CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linked.CancelAfter(options.Timeout);

        var clientChallenge = CreateChallenge();
        var offerPayload = new IpcHandshakePayload
        {
            InstanceToken = options.InstanceToken,
            Challenge = clientChallenge,
            ProcessName = Process.GetCurrentProcess().ProcessName,
            ProcessId = Environment.ProcessId
        };

        var offer = IpcEnvelope.Create(
            IpcMessageType.HandshakeOffer,
            IpcSenderRole.MainProcess,
            options.SessionId,
            options.HostInstanceId,
            SerializePayload(offerPayload));

        await IpcFrameCodec.WriteFrameAsync(stream, offer, linked.Token).ConfigureAwait(false);

        var accept = await IpcFrameCodec.ReadFrameAsync(stream, linked.Token).ConfigureAwait(false);
        if (accept.MessageType == IpcMessageType.HandshakeReject)
            throw new IpcProtocolException("对端拒绝握手。");
        if (accept.MessageType != IpcMessageType.HandshakeAccept)
            throw new IpcProtocolException($"期望 HandshakeAccept，收到 {accept.MessageType}。");

        ValidateAcceptFromHost(accept, options, identity);

        var acceptPayload = DeserializePayload(accept.PayloadJson)
            ?? throw new IpcProtocolException("HandshakeAccept 缺少载荷。");

        if (!FixedTimeEquals(acceptPayload.InstanceToken, options.InstanceToken))
            throw new IpcProtocolException("实例令牌不匹配。");

        var expectedClientResponse = ComputeResponse(clientChallenge, options.InstanceToken);
        if (!FixedTimeEquals(acceptPayload.ChallengeResponse, expectedClientResponse))
            throw new IpcProtocolException("服务端 challenge 响应无效。");

        var confirmPayload = new IpcHandshakePayload
        {
            InstanceToken = options.InstanceToken,
            Challenge = acceptPayload.Challenge,
            ChallengeResponse = ComputeResponse(acceptPayload.Challenge, options.InstanceToken),
            ProcessName = Process.GetCurrentProcess().ProcessName,
            ProcessId = Environment.ProcessId
        };

        var confirm = IpcEnvelope.Create(
            IpcMessageType.HandshakeAccept,
            IpcSenderRole.MainProcess,
            options.SessionId,
            options.HostInstanceId,
            SerializePayload(confirmPayload),
            correlationId: accept.MessageId);

        await IpcFrameCodec.WriteFrameAsync(stream, confirm, linked.Token).ConfigureAwait(false);
    }

    private static void ValidateOfferFromMain(
        IpcEnvelope offer,
        IpcConnectionOptions options,
        WindowsSessionIdentity identity)
    {
        if (offer.MessageType != IpcMessageType.HandshakeOffer)
            throw new IpcProtocolException($"期望 HandshakeOffer，收到 {offer.MessageType}。");
        if (offer.SenderRole != IpcSenderRole.MainProcess)
            throw new IpcProtocolException("HandshakeOffer 发送方角色无效。");
        if (!IpcMessageCatalog.IsProtocolCompatible(offer.ProtocolMajor, offer.ProtocolMinor))
            throw new IpcProtocolException($"协议版本不兼容：{offer.ProtocolMajor}.{offer.ProtocolMinor}。");
        if (offer.SessionId != options.SessionId || offer.SessionId != identity.SessionId)
            throw new IpcProtocolException("会话 ID 不匹配。");
        if (offer.HostInstanceId != options.HostInstanceId)
            throw new IpcProtocolException("HostInstanceId 不匹配。");
    }

    private static void ValidateAcceptFromHost(
        IpcEnvelope accept,
        IpcConnectionOptions options,
        WindowsSessionIdentity identity)
    {
        if (accept.SenderRole != IpcSenderRole.CardHost)
            throw new IpcProtocolException("HandshakeAccept 发送方角色无效。");
        if (!IpcMessageCatalog.IsProtocolCompatible(accept.ProtocolMajor, accept.ProtocolMinor))
            throw new IpcProtocolException($"协议版本不兼容：{accept.ProtocolMajor}.{accept.ProtocolMinor}。");
        if (accept.SessionId != options.SessionId || accept.SessionId != identity.SessionId)
            throw new IpcProtocolException("会话 ID 不匹配。");
        if (accept.HostInstanceId != options.HostInstanceId)
            throw new IpcProtocolException("HostInstanceId 不匹配。");
    }

    public static string CreateChallenge()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    public static string ComputeResponse(string challenge, string instanceToken)
    {
        var data = Encoding.UTF8.GetBytes(challenge + ":" + instanceToken);
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash);
    }

    private static bool FixedTimeEquals(string? a, string? b)
    {
        if (a is null || b is null)
            return false;
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(ba, bb);
    }

    private static string SerializePayload(IpcHandshakePayload payload)
        => JsonSerializer.Serialize(payload, JsonOptions);

    private static IpcHandshakePayload? DeserializePayload(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        return JsonSerializer.Deserialize<IpcHandshakePayload>(json, JsonOptions);
    }
}

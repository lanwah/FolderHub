namespace FolderHub.UseCases.Ipc;

/// <summary>
/// 方向白名单：Main→CardHost / CardHost→Main（ADR-035 / P3-05）。
/// </summary>
public static class IpcMessageCatalog
{
    private static readonly HashSet<IpcMessageType> MainToHost =
    [
        IpcMessageType.HandshakeOffer,
        IpcMessageType.HandshakeAccept,
        IpcMessageType.HandshakeReject,
        IpcMessageType.Ping,
        IpcMessageType.Pong,
        IpcMessageType.HostHello,
        IpcMessageType.CardDefinitionUpsert,
        IpcMessageType.CardEnable,
        IpcMessageType.CardDisable,
        IpcMessageType.CardClose,
        IpcMessageType.ShutdownRequest,
        IpcMessageType.ReconcileRequest,
        IpcMessageType.Ack,
        IpcMessageType.Error,
        IpcMessageType.CommandResult
    ];

    private static readonly HashSet<IpcMessageType> HostToMain =
    [
        IpcMessageType.HandshakeOffer,
        IpcMessageType.HandshakeAccept,
        IpcMessageType.HandshakeReject,
        IpcMessageType.Ping,
        IpcMessageType.Pong,
        IpcMessageType.HostReady,
        IpcMessageType.HostGoodbye,
        IpcMessageType.ReconcileReport,
        IpcMessageType.CardStateChangedNotice,
        IpcMessageType.Ack,
        IpcMessageType.Error,
        IpcMessageType.CommandResult
    ];

    public static bool IsKnown(IpcMessageType type)
        => Enum.IsDefined(typeof(IpcMessageType), type);

    public static bool IsAllowed(IpcSenderRole sender, IpcMessageType type)
    {
        if (!IsKnown(type))
            return false;

        return sender switch
        {
            IpcSenderRole.MainProcess => MainToHost.Contains(type),
            IpcSenderRole.CardHost => HostToMain.Contains(type),
            _ => false
        };
    }

    public static bool IsProtocolCompatible(int major, int minor)
        => major == IpcProtocol.Major && minor <= IpcProtocol.Minor;
}

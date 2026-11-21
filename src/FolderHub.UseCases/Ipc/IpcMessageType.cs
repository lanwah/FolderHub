namespace FolderHub.UseCases.Ipc;

/// <summary>
/// IPC 消息类型白名单（v0）。未知类型一律拒绝。
/// </summary>
public enum IpcMessageType
{
    HandshakeOffer = 1,
    HandshakeAccept = 2,
    HandshakeReject = 3,
    Ping = 10,
    Pong = 11,
    HostHello = 20,
    HostReady = 21,
    HostGoodbye = 22,
    CardDefinitionUpsert = 30,
    CardEnable = 31,
    CardDisable = 32,
    CardClose = 33,
    ShutdownRequest = 34,
    ReconcileRequest = 40,
    ReconcileReport = 41,
    CardStateChangedNotice = 50,
    CommandResult = 60,
    Ack = 70,
    Error = 71
}

using FolderHub.UseCases.Ipc;

namespace FolderHub.Infrastructure.Tests;

public sealed class IpcMessageCatalogTests
{
    [Theory]
    [InlineData(IpcSenderRole.MainProcess, IpcMessageType.CardDefinitionUpsert, true)]
    [InlineData(IpcSenderRole.MainProcess, IpcMessageType.CardStateChangedNotice, false)]
    [InlineData(IpcSenderRole.CardHost, IpcMessageType.CardStateChangedNotice, true)]
    [InlineData(IpcSenderRole.CardHost, IpcMessageType.CardDefinitionUpsert, false)]
    [InlineData(IpcSenderRole.MainProcess, IpcMessageType.HandshakeOffer, true)]
    [InlineData(IpcSenderRole.CardHost, IpcMessageType.HostReady, true)]
    public void IsAllowed_RespectsDirectionWhitelist(IpcSenderRole role, IpcMessageType type, bool expected)
        => Assert.Equal(expected, IpcMessageCatalog.IsAllowed(role, type));

    [Fact]
    public void IsProtocolCompatible_SameMajor_AcceptsEqualOrLowerMinor()
    {
        Assert.True(IpcMessageCatalog.IsProtocolCompatible(IpcProtocol.Major, IpcProtocol.Minor));
        Assert.True(IpcMessageCatalog.IsProtocolCompatible(IpcProtocol.Major, 0));
        Assert.False(IpcMessageCatalog.IsProtocolCompatible(IpcProtocol.Major + 1, 0));
        Assert.False(IpcMessageCatalog.IsProtocolCompatible(IpcProtocol.Major, IpcProtocol.Minor + 1));
    }
}

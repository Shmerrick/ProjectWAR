using System.Net.Sockets;
using FrameWork.NetWork.V4;

namespace LobbyServer.NetWork;

public class LobbyClientFactory : IClientFactory<LobbyClient>
{
    private readonly IPacketSerializerFactory _packetSerializerFactory;
    private readonly AccountMgr.AccountMgrClient _accountMgrClient;

    public LobbyClientFactory(IPacketSerializerFactory packetSerializerFactory, AccountMgr.AccountMgrClient accountMgrClient)
    {
        _packetSerializerFactory = packetSerializerFactory;
        _accountMgrClient = accountMgrClient;
    }
    
    public LobbyClient Create(TcpClient tcpClient)
    {
        return new LobbyClient(tcpClient, _packetSerializerFactory, _accountMgrClient);
    }
}
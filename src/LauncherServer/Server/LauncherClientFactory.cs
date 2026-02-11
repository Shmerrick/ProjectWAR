using System.Net.Sockets;
using FrameWork.NetWork.V4;
using LauncherServer.Config;

namespace LauncherServer.Server;

public class LauncherClientFactory : IClientFactory<LauncherClient>
{
    private readonly IPacketSerializerFactory _serializerFactory;
    private readonly AccountMgr.AccountMgrClient _accountMgrClient;
    private readonly MythLoginServiceConfigManager _loginServiceConfigManager;
    private readonly LauncherConfig _launcherConfig;

    public LauncherClientFactory(IPacketSerializerFactory serializerFactory, AccountMgr.AccountMgrClient accountMgrClient, MythLoginServiceConfigManager loginServiceConfigManager, LauncherConfig launcherConfig)
    {
        _serializerFactory = serializerFactory;
        _accountMgrClient = accountMgrClient;
        _launcherConfig = launcherConfig;
        _loginServiceConfigManager = loginServiceConfigManager;
    }

    public LauncherClient Create(TcpClient tcpClient)
    {
        return new LauncherClient(tcpClient, _serializerFactory, _accountMgrClient, _loginServiceConfigManager, _launcherConfig);
    }
}
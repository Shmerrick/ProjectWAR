using FrameWork;

namespace AccountCacher
{
    [aConfigAttributes("Configs/AccountConfig.xml")]
    public class AccountConfig : aConfig
    {
        public RpcServerConfig RpcInfo = new RpcServerConfig("127.0.0.1", 6800, 6000);
        public DatabaseInfo AccountDB = new DatabaseInfo();
        public LogInfo LogLevel = new LogInfo();
    }
}
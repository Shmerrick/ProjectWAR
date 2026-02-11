using FrameWork;

namespace AccountCacher
{
    [aConfigAttributes("Configs/Account.xml")]
    public class AccountConfig : aConfig
    {
        public DatabaseInfo AccountDB = new DatabaseInfo();
        public LogInfo LogLevel = new LogInfo();
        public bool EnableCache = true;
        public int MaxCacheSize = 10000;
    }
}
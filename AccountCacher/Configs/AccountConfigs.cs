using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace AccountCacher
{
    [aConfigAttributes("Configs/Account.xml")]
    public class AccountConfigs : aConfig
    {
        public RpcServerConfig RpcInfo = new RpcServerConfig("127.0.0.1", 6800, 6000);
        public DatabaseInfo AccountDB = new DatabaseInfo();
        public LogInfo LogLevel = new LogInfo();
    }
}

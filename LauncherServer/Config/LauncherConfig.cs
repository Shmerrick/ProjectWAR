using AuthenticationServer.Server;
using FrameWork;

namespace AuthenticationServer.Config
{
    [aConfigAttributes("Configs/LauncherConfig.xml")]
    public class LauncherConfig : aConfig
    {
        public int LauncherServerPort = 8000;
        public LogInfo LogLevel = new LogInfo();
        public string Message = "Invalid launcher version.";
        public string PatcherFilesPath = "PatcherFilesDirectory";
        public string PatchNotes = "Welcome to Warhammer Online: Age of Reckoning!";
        public RpcClientConfig RpcInfo = new RpcClientConfig("127.0.0.1", "127.0.0.1", 6800);
        public ServerState ServerState = ServerState.CLOSED;
        public bool SeverOnConnect = true;
        public string TempFilesPath = "TempFilesDirectory";
        public int Version = 1;
    }
}
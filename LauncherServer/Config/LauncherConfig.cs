using AuthenticationServer.Server;
using FrameWork;

namespace AuthenticationServer.Config
{
    [aConfigAttributes("Configs/Launcher.xml")]
    public class LauncherConfig : aConfig
    {
        public int LauncherServerPort = 8000;
        public int Version = 1;
        public string Message = "Invalid launcher version.";
        public bool SeverOnConnect = true;
        public RpcClientConfig RpcInfo = new RpcClientConfig("127.0.0.1", "127.0.0.1", 6800);
        public LogInfo LogLevel = new LogInfo();


        public string PatcherFilesPath = "PatcherFilesDirectory";
        public string TempFilesPath = "TempFilesDirectory";
        public ServerState ServerState = ServerState.CLOSED;
        public string PatchNotes = "Welcome to Warhammer Online: Age of Reckoning!";
    }
}
using FrameWork;
using LauncherServer.Dtos;
using LauncherServer.Server;

namespace LauncherServer.Config
{
    [aConfigAttributes("Configs/Launcher.xml")]
    public class LauncherConfig : aConfig
    {
        public int LauncherServerPort = 8000;
        public int Version = 1;
        public string Message = "Invalid launcher version.";
        public bool SeverOnConnect = true;
        public LogInfo LogLevel = new LogInfo();

        public string PatcherFilesPath = "PatcherFilesDirectory";
        public string TempFilesPath = "TempFilesDirectory";
        public ServerState ServerState = ServerState.CLOSED;
        public string PatchNotes = "Welcome to Warhammer Online: Age of Reckoning!";
    }
}
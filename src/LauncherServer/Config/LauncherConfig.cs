using FrameWork;
using LauncherServer.Dtos;
using LauncherServer.Server;

namespace LauncherServer.Config
{
    [aConfigAttributes("Configs/Launcher.xml")]
    public class LauncherConfig : aConfig
    {
        public int LauncherServerPort { get; init; } = 8000;
        public int Version { get; init; } = 1;
        public string Message { get; init; } = "Invalid launcher version.";
        public bool SeverOnConnect { get; init; } = true;
        public LogInfo LogLevel { get; init; } = new LogInfo();

        public string PatcherFilesPath { get; init; } = "PatcherFilesDirectory";
        public string TempFilesPath { get; init; } = "TempFilesDirectory";
        public ServerState ServerState { get; init; } = ServerState.CLOSED;
        public string PatchNotes { get; init; } = "Welcome to Warhammer Online: Age of Reckoning!";
    }
}
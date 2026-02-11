using FrameWork;

namespace LobbyServer
{
    [aConfigAttributes("Configs/Lobby.xml")]
    public class LobbyConfigs : aConfig
    {
        public int ClientPort = 8048;
        public string ClientVersion = "1.4.8";
        public bool SeverOnFinish = true;

        public LogInfo LogLevel = new LogInfo();
    }
}
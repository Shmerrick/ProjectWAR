using System.IO;
using FrameWork;

namespace LauncherServer;

public class MythLoginServiceConfigManager
{
    public string Content { get; }
    
    public MythLoginServiceConfigManager(string filePath)
    {
        var file = new FileInfo("Configs/mythloginserviceconfig.xml");
        if (!file.Exists)
        {
            Log.Error("Configs/mythloginserviceconfig.xml", "Config file missing !");
            throw new FileNotFoundException("Config file missing !", filePath);
        }

        Content = file.OpenText().ReadToEnd();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace WorldServer
{
    [aConfigAttributes("Configs/World.xml")]
    public class WorldConfigs : aConfig
    {
        public RpcClientConfig AccountCacherInfo = new RpcClientConfig("127.0.0.1", "127.0.0.1", 6800);
        public LogInfo LogLevel = new LogInfo();

        public DatabaseInfo CharacterDatabase = new DatabaseInfo();
        public DatabaseInfo WorldDatabase = new DatabaseInfo();

        public byte RealmId = 1;

        // Level / Looting
        public int GlobalLootRate = 1;
        public int CommonLootRate = 1;
        public int UncommonLootRate = 1;
        public int RareLootRate = 1;
        public int VeryRareLootRate = 1;
        public int ArtifactLootRate = 1;
        public int GoldRate = 1;
        public int XpRate = 1;
        public int RenownRate = 1;
        public int InfluenceRate = 1;
        public int RankCap = 32;
        public int RenownCap = 32;

        // Crossrealming
        public bool ChatBetweenRealms = true;
        public bool CreateBothRealms = true;

        // Area / NPC
        public bool CleanSpawns = true;
        public bool DiscoverAll = false;
        public bool OpenRvR = true;

        // Loading
        public bool PreloadAllCharacters = true;
        public string Motd = "Welcome to WAR: Apocalypse, Max Level 32, Max RR 32.";
        public string RegionOcclusionFolder = "los/";
        public string ZoneFolder = "zones/";

        // API
        public bool EnableAPI = true;
        public string APIAddress = "127.0.0.1";
        public int APIPort = 51932;

        // Networking
        public int PacketCollateLength = 0;
    }
}

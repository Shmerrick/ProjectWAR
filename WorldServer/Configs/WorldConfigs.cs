using FrameWork;

namespace WorldServer.Configs
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
        public int RankCap = 40;
        public int RenownCap = 40;

        // Crossrealming
        public bool ChatBetweenRealms = true;
        public bool CreateBothRealms = true;

        // Area / NPC
        public bool CleanSpawns = true;
        public bool DiscoverAll = false;
        public bool OpenRvR = false;

        // Loading
        public bool PreloadAllCharacters = true;
        public string Motd = "Welcome to Warhammer Online: Age of Reckoning!";
        public string RegionOcclusionFolder = "los/";
        public string ZoneFolder = "zones/";

        // API
        public bool EnableAPI = true;
        public string APIAddress = "127.0.0.1";
        public int APIPort = 51932;

        // Networking
        public int PacketCollateLength = 0;


        // RVR Configuration

        // Minimum number of defenders to set Lord Rank.
        public int LordRankOne = 40;        // When there are at least this many defenders, set the Lord's rank to 1 (weakest)
        public int LordRankTwo = 20;
        public int LordRankThree = 0;

        public int FortDefenceTimer = 600000;   // Number of MS for a tick (there are 4 ticks) before a Fort Defends itself.

        public int DominationPointsRequired = 6;    // Number of domination points required to start the domination timer.
        public int DestructionDominationTimerLength = 20;   // Number of minutes for Dest Domination to finish.
        public int OrderDominationTimerLength = 20;   // Number of minutes for Order Domination to finish.

        public int DoorRepairTimerLength = 30 * 60;
        public int SeizedTimerLength = 1 * 2;
        public int LordKilledTimerLength = 1 * 2;
        public int DefenceTickTimerLength = 20 * 60;
        public int BackToSafeTimerLength = 3 * 60;  // should be "short"

        public int REALM_CAPTAIN_RENOWN_KILL_SOLO = 1200;  // RP for killing RC solo
        public int REALM_CAPTAIN_RENOWN_KILL_PARTY = 450;  // RP for killing RC as part of a party
        public int REALM_CAPTAIN_INFLUENCE_KILL = 500;    // INF for killing RC solo
        public int REALM_CAPTAIN_KILL_CRESTS = 6;
        public int REALM_CAPTAIN_ASSIST_CRESTS = 1;

        public int EligiblePlayerBagBonusIncrement = 1;     // Amount contribution is increased if player is eligible but does not get a bag.
        public int BagRollRandomUpperLimit = 10;            // Upper limit to random bag roll
        public int BagRollRandomLowerLimit = 1;             // Lower limit to random bag roll
        public int PairingContributionTimeIntervalHours = 2; // Number of hours we trace back for zone/pairing contribution
        public int PairingBonusIncrement = 5;               // Increment for each leadin zone player was eligible in.

        public string AllowBagBonusContribution = "Y";
        public string AllowPairingContribution = "Y";
        public string AllowRandomContribution = "Y";
        public int KeepDoorPercentHealthBoundary = 98;      // % of door health required to trigger door under attack logic
        public string DebugLootRolls = "N";                    // Display loot roll details to players

        public int BOCapturingRewardXp = 1000;
        public int BOCapturingRewardRp = 100;
        public int BOCapturingRewardInf = 100;
        public int BOCapturedRewardXp = 2000;
        public int BOCapturedRewardRp = 200;
        public int BOCapturedRewardInf = 200;
        public int BOGuardedRewardXp = 1000;
        public int BOGuardedRewardRp = 200;
        public int BOGuardedRewardInf = 200;
        public double AdditionalBagKillCountStep = 2f;
        public int AdditionalBagRatioMinimumWinners = 3;        // Minimum eligible winners before bag ratios are applied
        public int AdditionalBagRatioMinimumLosers = 3;         // Minimum eligible losers before bag ratios are applied
    }
}

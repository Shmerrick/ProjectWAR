
namespace WorldServer.World.Battlefronts.Apocalypse
{
    /// <summary>
    /// Constants for RoR BattleFronts that can be changed using GM Commands.
    /// </summary>
    /// <remarks>
    /// After these values have been stabilized, GM commands should be removed
    /// and declarations have to be converted to "const".
    /// </remarks>
    public class BattleFrontConstants
    {
        private BattleFrontConstants() { }

        #region Ranges
        /// <summary>LastUpdatedTime in millis between each detection of close players around flags<summary>
        public static int CLOSE_DETECTION_INTERVAL = 5000;

        /// <summary>Range allowing player to be considered as assaulting or defending a flag</summary>
        public static int CLOSE_RANGE = 70;

        /// <summary>Range under which opponents are disabling warcamp to objective portals</summary>
        public static int THREATEN_RANGE = 200;

        /// <summary>Range of warcamp farm detection, applying dealed and received damage debuff</summary>
        public static float WARCAMP_FARM_RANGE = 250;
        #endregion

        #region Ownership and assault state
        /// <summary>Absolute maximum of the control gauges of the flags<summary>
        public static int MAX_SECURE_PROGRESS = 80;

        /// <summary>Absolute maximum of the control gauges of the flags.
        /// Is used as a base timer in milliseconds when securing objectives.<summary>
        public static int MAX_CONTROL_GAUGE = MAX_SECURE_PROGRESS * 200;

        /// <summary>Maximum players each side taken in consideration for assaults a flag</summary>
        public static short MAX_CLOSE_PLAYERS = 6;
        #endregion

        #region Reward
        /// <summary>Reward check timer for players defending a secured flag</summary>
        public static int FLAG_SECURE_REWARD_INTERVAL = 6000;

        /// <summary>Base reward scaler for players defending a secured flag</summary>
        public static float FLAG_SECURE_REWARD_SCALER = 50f; //50f

        /// <summary>Scaler applied to contributions in order to compute lock rewards</summary>
        public static float LOCK_REWARD_SCALER = 0.25f;
        #endregion

        #region Victory points
        /// <summary>
        /// Minimal number of victory points required for locking
        /// NOT What the game is using as lock values.
        /// </summary>
        public static float LOCK_VICTORY_POINTS = 1000f;

        #endregion


        #region NPCs and misc

        /// <summary>Delay between each usage of warcamp to objective portals.</summary>
        public static long PORTAL_DELAY = 5000;
        #endregion


        #region Battlefront Tiers and Ids

        public static int BATTLEFRONT_DWARF_GREENSKIN_TIER1_EKRUND = 11;
        public static int BATTLEFRONT_DWARF_GREENSKIN_TIER2 = 11;
        public static int BATTLEFRONT_DWARF_GREENSKIN_TIER3 = 11;
        public static int BATTLEFRONT_DWARF_GREENSKIN_TIER4_BLACK_CRAG = 4;
        public static int BATTLEFRONT_DWARF_GREENSKIN_TIER4_THUNDER_MOUNTAIN = 5;
        public static int BATTLEFRONT_DWARF_GREENSKIN_TIER4_KADRIN_VALLEY = 6;
        public static int BATTLEFRONT_DWARF_GREENSKIN_TIER4_STONEWATCH = 14;
        public static int BATTLEFRONT_DWARF_GREENSKIN_TIER4_BUTCHERS_PASS = 16;


        public static int BATTLEFRONT_EMPIRE_CHAOS_TIER1_NORDLAND = 10;
        public static int BATTLEFRONT_EMPIRE_CHAOS_TIER2 = 11;
        public static int BATTLEFRONT_EMPIRE_CHAOS_TIER3 = 11;
        public static int BATTLEFRONT_EMPIRE_CHAOS_TIER4_CHAOS_WASTES = 1;
        public static int BATTLEFRONT_EMPIRE_CHAOS_TIER4_PRAAG = 2;
        public static int BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKLAND = 3;
        public static int BATTLEFRONT_EMPIRE_CHAOS_TIER4_REIKWALD = 13;
        public static int BATTLEFRONT_EMPIRE_CHAOS_TIER4_THE_MAW = 15;


        public static int BATTLEFRONT_ELF_DARKELF_TIER1_CHRACE = 12;
        public static int BATTLEFRONT_ELF_DARKELF_TIER2 = 11;
        public static int BATTLEFRONT_ELF_DARKELF_TIER3 = 11;
        public static int BATTLEFRONT_ELF_DARKELF_TIER4_EATAINE =7;
        public static int BATTLEFRONT_ELF_DARKELF_TIER4_DRAGONWAKE = 8;
        public static int BATTLEFRONT_ELF_DARKELF_TIER4_CALEDOR = 9;
        public static int BATTLEFRONT_ELF_DARKELF_TIER4_SHINING_WAY = 17;
        public static int BATTLEFRONT_ELF_DARKELF_TIER4_FELL_LANDING = 18;

        public const int ZONE_STATUS_CONTESTED = 0;
        public const int ZONE_STATUS_ORDER_LOCKED = 1;
        public const int ZONE_STATUS_DESTRO_LOCKED = 2;


        #endregion



    }
}

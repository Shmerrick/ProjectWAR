
namespace WorldServer.World.Battlefronts.Apocalypse
{
    /// <summary>
    /// Constants for RoR BattleFronts that can be changed using GM Commands.
    /// </summary>
    /// <remarks>
    /// After these values have been stabilized, GM commands should be removed
    /// and declarations have to be converted to "const".
    /// </remarks>
    internal class BattleFrontConstants
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
        public static int MAX_CONTROL_GAUGE = MAX_SECURE_PROGRESS * 600;

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
        public static float LOCK_VICTORY_POINTS = 5000f;

        #endregion


        #region NPCs and misc
        /// <summary>Lord maximum damage mitigation scaler</summary>
        public static float MAX_LORD_SCALER = 0.75f;
        /// <summary>Maximum enemies in lakes for lord incoming damage reduction (-75%)</summary>
        public static int MAX_LORD_SCALER_POP = 70;

        /// <summary>Delay between each usage of warcamp to objective portals.</summary>
        public static long PORTAL_DELAY = 5000;
        #endregion

#if _DEBUG
        static BattleFrontConstants() {
            MAX_SECURE_PROGRESS /= 5;
            MAX_CONTROL_GAUGE /= 5;
            T1_LOCK_VICTORY_POINTS /= 10;
        }
#endif

    }
}

using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.Services.World
{
    [Service]
    public static class CitySiegeService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static bool IsSiegeActive { get; private set; }
        public static Realms AttackingRealm { get; private set; }
        public static int CityRating { get; private set; }
        public static long SiegeStartTime { get; private set; }

        [LoadingFunction(true)]
        public static void Init()
        {
            Logger.Info("CitySiegeService", "Initializing...");
            IsSiegeActive = false;
            CityRating = 1; // Default starting rating
            Logger.Info("CitySiegeService", "Initialized");
        }

        public static void BeginSiege(Realms attackingRealm)
        {
            if (IsSiegeActive)
            {
                Logger.Warn("CitySiegeService", "Attempted to begin a siege while one is already active.");
                return;
            }

            IsSiegeActive = true;
            AttackingRealm = attackingRealm;
            SiegeStartTime = TCPManager.GetTimeStamp();

            string attacker = attackingRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction";
            Logger.Info("CITY SIEGE", $"A siege has begun! {attacker} is attacking the enemy capital!");

            // Announce to the world
            foreach (var player in Player._Players)
            {
                player.SendClientMessage($"The forces of {attacker} are laying siege to the enemy capital!", ChatLogFilters.CHATLOGFILTERS_RVR);
                player.SendClientMessage("A queue will open shortly for those wishing to join the fight.", ChatLogFilters.CHATLOGFILTERS_RVR);
            }

            // TODO: Start a 10-minute timer for the queue to open, as per the design document.
        }

        public static void EndSiege()
        {
            if (!IsSiegeActive)
            {
                Logger.Warn("CitySiegeService", "Attempted to end a siege while none is active.");
                return;
            }

            IsSiegeActive = false;
            Logger.Info("CITY SIEGE", "The city siege has ended.");

            // TODO: Reset campaign state, teleport players, etc.
        }

        public static bool CheckCampaignVictory(Realms lockingRealm, IBattleFrontManager bfm)
        {
            // This check is only for Tier 4
            if (bfm.GetActiveCampaign().Tier != 4)
                return false;

            // Check if the locking realm owns all T4 pairings
            var t4Progressions = bfm.BattleFrontProgressions.Where(p => p.Tier == 4);

            // Exclude the final "city" pseudo-zones from the check
            var campaignZones = t4Progressions.Where(p => p.OrderWinProgression != 0 && p.DestWinProgression != 0);

            bool allLocked = campaignZones.All(p => (Realms)p.LastOwningRealm == lockingRealm);

            if (allLocked)
            {
                Logger.Info("CITY SIEGE", $"{lockingRealm} has locked all T4 zones. A city siege will begin!");
                return true;
            }

            return false;
        }
    }
}

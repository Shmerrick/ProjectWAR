using System.Collections.Generic;
using System.Linq;
using Common.Database.World.Battlefront;
using GameData;
using NLog;

// ReSharper disable InconsistentNaming

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class LowerTierBattleFrontManager : IBattleFrontManager
    {
        public List<RVRProgression> BattleFrontProgressions { get; }
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");
        /// <summary>
        /// The RacialPair that is currently active.
        /// </summary>
        public RVRProgression ActiveBattleFront { get; set; }

        public LowerTierBattleFrontManager(List<RVRProgression> _RVRT1Progressions)
        {
            BattleFrontProgressions = _RVRT1Progressions;
        }

        /// <summary>
        /// Log the status of all battlefronts 
        /// </summary>
        public void AuditBattleFronts(int tier)
        {
            foreach (var regionMgr in WorldMgr._Regions)
            {
                if (regionMgr.GetTier() == tier)
                {
                    foreach (var objective in regionMgr.ndbf.Objectives)
                    {
                        ProgressionLogger.Debug($"{regionMgr.RegionName} {objective.Name} {objective.FlagState} {objective.State}");
                    }
                }
            }
        }

        /// <summary>
        /// Log the status of all battlefronts 
        /// </summary>
        public void LockBattleFronts(int tier)
        {
            foreach (var regionMgr in WorldMgr._Regions)
            {
                if (regionMgr.GetTier() == tier)
                {
                    foreach (var objective in regionMgr.ndbf.Objectives)
                    {
                        objective.LockObjective(Realms.REALMS_REALM_NEUTRAL, false);
                        ProgressionLogger.Debug($" Locking to Neutral {objective.Name} {objective.FlagState} {objective.State}");
                    }
                   
                }
            }
        }

        public void LockBattleFront(Realms realm)
        {
            var activeRegion = WorldMgr._Regions.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Locking battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

            foreach (var flag in activeRegion.ndbf.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.LockObjective(realm, true);
            }

            activeRegion.ndbf.LockPairing(realm);
        }

        public void OpenActiveBattlefront()
        {
            var activeRegion = WorldMgr._Regions.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Opening battlefront in {activeRegion.RegionName}");

            activeRegion.ndbf.VictoryPointProgress.Reset(activeRegion.ndbf);
            activeRegion.ndbf.LockingRealm = Realms.REALMS_REALM_NEUTRAL;

            foreach (var flag in activeRegion.ndbf.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.UnlockObjective();
            }

        }

        /// <summary>
        /// Set the Active Pairing to be null. Not expected to be needed.
        /// </summary>
        public RVRProgression ResetBattleFrontProgression()
        {
            ProgressionLogger.Info($" Resetting battlefront...");
            // HACK
            ActiveBattleFront = GetBattleFrontByName("Norsca");
            ProgressionLogger.Info($"Active : {this.ActiveBattleFrontName}");
            return ActiveBattleFront;
        }

        public RVRProgression GetBattleFrontByName(string name)
        {
            return BattleFrontProgressions.Single(x => x.Description.Contains(name));
        }

        public RVRProgression GetBattleFrontByBattleFrontId(int id)
        {
            return BattleFrontProgressions.Single(x => x.BattleFrontId == id);
        }

        public string ActiveBattleFrontName
        {
            get => ActiveBattleFront.Description;
            set => ActiveBattleFront.Description = value;
        }

        /// <summary>
        /// </summary>
        public RVRProgression AdvanceBattleFront(Realms lockingRealm)
        {
            if (lockingRealm == Realms.REALMS_REALM_ORDER)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.OrderWinProgression);
                ProgressionLogger.Info($"Order Win : Advancing Battlefront from {this.ActiveBattleFrontName} to {newBattleFront.Description}");
                return ActiveBattleFront = newBattleFront;
            }

            if (lockingRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.DestWinProgression);
                ProgressionLogger.Info($"Destruction Win : Advancing Battlefront from {this.ActiveBattleFrontName} to {newBattleFront.Description}");
                return ActiveBattleFront = newBattleFront;
            }
            return ResetBattleFrontProgression();
        }

    }
}
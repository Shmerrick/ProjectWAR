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
        public List<RegionMgr> RegionMgrs { get; }
        public List<RVRProgression> BattleFrontProgressions { get; }
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");
     public RVRProgression ActiveBattleFront { get; set; }
        public List<ApocBattleFrontStatus> ApocBattleFrontStatuses { get; set; }

        public LowerTierBattleFrontManager(List<RVRProgression> _RVRT1Progressions, List<RegionMgr> regionMgrs)
        {
            BattleFrontProgressions = _RVRT1Progressions;
            RegionMgrs = regionMgrs;
            ApocBattleFrontStatuses = new List<ApocBattleFrontStatus>();

            BuildApocBattleFrontStatusList(BattleFrontProgressions);
        }

        /// <summary>
        /// Sets up the Battlefront status list with default values. 
        /// </summary>
        /// <param name="battleFrontProgressions"></param>
        private void BuildApocBattleFrontStatusList(List<RVRProgression> battleFrontProgressions)
        {
            foreach (var battleFrontProgression in battleFrontProgressions)
            {
                this.ApocBattleFrontStatuses.Add(new ApocBattleFrontStatus
                {
                    BattleFrontId = battleFrontProgression.BattleFrontId,
                    LockingRealm = Realms.REALMS_REALM_NEUTRAL,
                    FinalVictoryPoint = new VictoryPointProgress(),
                    OpenTimeStamp = 0,
                    LockTimeStamp = 0,
                    Locked = true
                });
            }
        }

        /// <summary>
        /// Log the status of all battlefronts 
        /// </summary>
        public void AuditBattleFronts(int tier)
        {
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.GetTier() == tier)
                {
                    foreach (var objective in regionMgr.BattleFront.Objectives)
                    {
                        ProgressionLogger.Debug($"{regionMgr.RegionName} {objective.Name} {objective.FlagState} {objective.State}");
                    }
                }
            }
        }

        /// <summary>
        /// Log the status of all battlefronts 
        /// </summary>
        public void LockBattleFrontsAllRegions(int tier)
        {
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.GetTier() == tier)
                {
                    // Find and update the status of the battlefront status (list of BFStatuses is only for this Tier)
                    foreach (var ApocBattleFrontStatus in ApocBattleFrontStatuses)
                    {
                        ApocBattleFrontStatus.Locked = true;
                        ApocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                        ApocBattleFrontStatus.LockingRealm = Realms.REALMS_REALM_NEUTRAL;
                        ApocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                        ApocBattleFrontStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                    }

                    foreach (var objective in regionMgr.BattleFront.Objectives)
                    {
                        objective.LockObjective(Realms.REALMS_REALM_NEUTRAL, false);
                        ProgressionLogger.Debug($" Locking to Neutral {objective.Name} {objective.FlagState} {objective.State}");
                    }
                   
                }
            }
        }
        
        public List<ApocBattleFrontStatus> GetBattleFrontStatusList()
        {
            return this.ApocBattleFrontStatuses;
        }

        public bool IsBattleFrontLocked(int battleFrontId)
        {
            foreach (var ApocBattleFrontStatus in ApocBattleFrontStatuses)
            {
                if (ApocBattleFrontStatus.BattleFrontId == this.ActiveBattleFront.BattleFrontId)
                {
                    return ApocBattleFrontStatus.Locked;
                }
            }
            return false;
        }

        public ApocBattleFrontStatus GetBattleFrontStatus(int battleFrontId)
        {
            return this.ApocBattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);
        }

        public RVRProgression LockActiveBattleFront(Realms realm)
        {
            var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Locking battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

            foreach (var ApocBattleFrontStatus in ApocBattleFrontStatuses)
            {
                if (ApocBattleFrontStatus.BattleFrontId == this.ActiveBattleFront.BattleFrontId)
                {
                    ApocBattleFrontStatus.Locked = true;
                    ApocBattleFrontStatus.LockingRealm = realm;
                    ApocBattleFrontStatus.FinalVictoryPoint = activeRegion.BattleFront.VictoryPointProgress;
                    ApocBattleFrontStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                }
            }

            foreach (var flag in activeRegion.BattleFront.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.LockObjective(realm, true);
            }

            activeRegion.BattleFront.LockBattleFront(realm);

            // Use Locking Realm in the BFM, not the BF (BF applies to region)
            return this.ActiveBattleFront;
        }

        public RVRProgression OpenActiveBattlefront()
        {
            var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Opening battlefront in {activeRegion.RegionName}");

            activeRegion.BattleFront.VictoryPointProgress.Reset(activeRegion.BattleFront);

            // Find and update the status of the battlefront status.
            foreach (var ApocBattleFrontStatus in ApocBattleFrontStatuses)
            {
                if (ApocBattleFrontStatus.BattleFrontId == this.ActiveBattleFront.BattleFrontId)
                {
                    ApocBattleFrontStatus.Locked = false;
                    ApocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                    ApocBattleFrontStatus.LockingRealm = Realms.REALMS_REALM_NEUTRAL;
                    ApocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                    ApocBattleFrontStatus.LockTimeStamp = 0;
                }
            }

            foreach (var flag in activeRegion.BattleFront.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.UnlockObjective();
            }
            return this.ActiveBattleFront;
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
            get { return ActiveBattleFront.Description; }
            set { ActiveBattleFront.Description = value;  }
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
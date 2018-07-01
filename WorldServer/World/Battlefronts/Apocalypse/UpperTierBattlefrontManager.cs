using System.Collections.Generic;
using System.Linq;
using Common.Database.World.Battlefront;
using GameData;
using NLog;
using WorldServer.World.BattleFronts.Keeps;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class UpperTierBattleFrontManager : IBattleFrontManager
    {
        static readonly object _lockObject = new object();

        public const int BATTLEFRONT_TIER = 4;
        public List<RegionMgr> RegionMgrs { get; }
        public List<RVRProgression> BattleFrontProgressions { get; }
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");
        public RVRProgression ActiveBattleFront { get; set; }
        public List<ApocBattleFrontStatus> ApocBattleFrontStatuses { get; set; }

        public UpperTierBattleFrontManager(List<RVRProgression> _RVRT4Progressions, List<RegionMgr> regionMgrs)
        {
            BattleFrontProgressions = _RVRT4Progressions;
            RegionMgrs = regionMgrs;
            ApocBattleFrontStatuses = new List<ApocBattleFrontStatus>();
            if (_RVRT4Progressions != null)
                BuildApocBattleFrontStatusList(BattleFrontProgressions);
        }

        /// <summary>
        /// Sets up the Battlefront status list with default values. 
        /// </summary>
        /// <param name="battleFrontProgressions"></param>
        private void BuildApocBattleFrontStatusList(List<RVRProgression> battleFrontProgressions)
        {

            lock (_lockObject)
            {
                this.ApocBattleFrontStatuses.Clear();
                foreach (var battleFrontProgression in battleFrontProgressions)
                {
                    this.ApocBattleFrontStatuses.Add(new ApocBattleFrontStatus
                    {
                        BattleFrontId = battleFrontProgression.BattleFrontId,
                        LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == battleFrontProgression.BattleFrontId).DefaultRealmLock,
                    FinalVictoryPoint = new VictoryPointProgress(),
                        OpenTimeStamp = 0,
                        LockTimeStamp = 0,
                        Locked = true
                    });
                }
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
                        ProgressionLogger.Debug($"{regionMgr.RegionName} {objective.Name} {objective.State}");
                    }
                    foreach (var keep in regionMgr.BattleFront.Keeps)
                    {
                        ProgressionLogger.Debug($"{regionMgr.RegionName} {keep.Name} {keep.KeepStatus} ");
                    }
                }
            }
        }


        /// <summary>
        /// Lock Battlefronts across all the regions.
        /// </summary>
        public void LockBattleFrontsAllRegions(int tier)
        {
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.GetTier() == tier)
                {

                    // Find and update the status of the battlefront status (list of BFStatuses is only for this Tier)
                    foreach (var apocBattleFrontStatus in ApocBattleFrontStatuses)
                    {
                        apocBattleFrontStatus.Locked = true;
                        apocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                        // Determine what the "start" realm this battlefront should be locked to. 
                        apocBattleFrontStatus.LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).DefaultRealmLock;
                        apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                        apocBattleFrontStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                    }

                    foreach (var objective in regionMgr.BattleFront.Objectives)
                    {
                        objective.LockObjective(Realms.REALMS_REALM_NEUTRAL, false);
                        ProgressionLogger.Debug($" Locking BO to Neutral {objective.Name} {objective.State} {objective.State}");
                    }
                    foreach (var keep in regionMgr.BattleFront.Keeps)
                    {
                        keep.LockKeep(Realms.REALMS_REALM_NEUTRAL, false, false);
                        ProgressionLogger.Debug($" Locking Keep to Neutral {keep.Name} {keep.KeepStatus} ");
                    }
                }
            }
        }



        public RVRProgression OpenActiveBattlefront()
        {
            // If this battlefront is the reset battlefront (ie Praag), reset all the progressions upon our return to it.
            if (this.ActiveBattleFront.ResetProgressionOnEntry == 1)
            {
                LockBattleFrontsAllRegions(4);
                // Reset the status list.
                BuildApocBattleFrontStatusList(this.BattleFrontProgressions);
                // Tells the attached players about it.
                WorldMgr.UpdateRegionCaptureStatus();
            }


            var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Opening battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

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

            foreach (Keep keep in activeRegion.BattleFront.Keeps)
            {
                if (this.ActiveBattleFront.ZoneId == keep.ZoneId)
                    keep.NotifyPairingUnlocked();
            }

            return this.ActiveBattleFront;
        }

        public RVRProgression LockActiveBattleFront(Realms realm)
        {
            var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Locking battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

            LockBattleFrontStatus(this.ActiveBattleFront.BattleFrontId, realm, activeRegion.BattleFront.VictoryPointProgress);

            foreach (var flag in activeRegion.BattleFront.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.LockObjective(realm, true);
            }

            foreach (Keep keep in activeRegion.BattleFront.Keeps)
            {
                if (this.ActiveBattleFront.ZoneId == keep.ZoneId)
                    keep.LockKeep(realm, true, true);
            }

            activeRegion.BattleFront.LockBattleFront(realm);

            // Use Locking Realm in the BFM, not the BF (BF applies to region)
            return this.ActiveBattleFront;

        }

        public void LockBattleFrontStatus(int battleFrontId, Realms lockingRealm, VictoryPointProgress vpp)
        {
            var activeStatus = ApocBattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);

            activeStatus.Locked = true;
            activeStatus.LockingRealm = lockingRealm;
            activeStatus.FinalVictoryPoint = vpp;
            activeStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
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


        /// <summary>
        /// Set the Active Pairing to be null. Not expected to be needed.
        /// </summary>
        public RVRProgression ResetBattleFrontProgression()
        {
            ProgressionLogger.Debug($" Resetting battlefront progression...");
            // HACK
            ActiveBattleFront = GetBattleFrontByName("Praag");
            ProgressionLogger.Debug($"Active : {this.ActiveBattleFrontName}");
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
            get
            {
                return ActiveBattleFront?.Description;
            }
            set { ActiveBattleFront.Description = value; }
        }

        /// <summary>
        /// </summary>
        public RVRProgression AdvanceBattleFront(Realms lockingRealm)
        {
            if (lockingRealm == Realms.REALMS_REALM_ORDER)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.OrderWinProgression);
                ProgressionLogger.Debug($"Order Win : Advancing Battlefront from {this.ActiveBattleFrontName} to {newBattleFront.Description}");
                return ActiveBattleFront = newBattleFront;
            }

            if (lockingRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.DestWinProgression);
                ProgressionLogger.Debug($"Destruction Win : Advancing Battlefront from {this.ActiveBattleFrontName} to {newBattleFront.Description}");
                return ActiveBattleFront = newBattleFront;
            }
            return ResetBattleFrontProgression();
        }
    }
}
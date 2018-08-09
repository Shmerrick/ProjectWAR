using System;
using Common.Database.World.Battlefront;
using GameData;
using NLog;
using System.Collections.Generic;
using System.Linq;
using WorldServer.World.BattleFronts.Keeps;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class UpperTierCampaignManager : IBattleFrontManager
    {
        private static readonly object LockObject = new object();
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");

        public const int BATTLEFRONT_TIER = 4;
        public List<RegionMgr> RegionMgrs { get; }
        public List<RVRProgression> BattleFrontProgressions { get; }

        public RVRProgression ActiveBattleFront { get; set; }
        public List<BattleFrontStatus> BattleFrontStatuses { get; set; }

        public UpperTierCampaignManager(List<RVRProgression> _RVRT4Progressions, List<RegionMgr> regionMgrs)
        {
            BattleFrontProgressions = _RVRT4Progressions;
            RegionMgrs = regionMgrs;
            BattleFrontStatuses = new List<BattleFrontStatus>();
            if (_RVRT4Progressions != null)
                BuildApocBattleFrontStatusList(BattleFrontProgressions);
        }

        /// <summary>
        /// Sets up the Battlefront status list with default values.
        /// </summary>
        /// <param name="battleFrontProgressions"></param>
        private void BuildApocBattleFrontStatusList(List<RVRProgression> battleFrontProgressions)
        {
            lock (LockObject)
            {
                this.BattleFrontStatuses.Clear();
                foreach (var battleFrontProgression in battleFrontProgressions)
                {
                    this.BattleFrontStatuses.Add(new BattleFrontStatus
                    {
                        BattleFrontId = battleFrontProgression.BattleFrontId,
                        LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == battleFrontProgression.BattleFrontId).DefaultRealmLock,
                        FinalVictoryPoint = new VictoryPointProgress(),
                        OpenTimeStamp = 0,
                        LockTimeStamp = 0,
                        Locked = true,
                        RegionId = battleFrontProgression.RegionId,
                        Description = battleFrontProgression.Description
                    });
                }
            }
        }

        /// <summary>
        /// Returns the active campaign based upon the region.
        /// </summary>
        /// <returns></returns>
        public Campaign GetActiveCampaign()
        {
            var activeRegionId = ActiveBattleFront.RegionId;
            foreach (var regionMgr in RegionMgrs)
            {
                if (regionMgr.RegionId == activeRegionId)
                {
                    return regionMgr.Campaign;
                }
            }
            return null;
        }

        public BattleFrontStatus GetRegionBattleFrontStatus(int regionId)
        {
            if (this.BattleFrontStatuses != null)
            {
                return this.BattleFrontStatuses.SingleOrDefault(x => x.RegionId == regionId);
            }
            else
            {
                ProgressionLogger.Debug($"Call to get region status with no statuses");
                return null;
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
                    foreach (var objective in regionMgr.Campaign.Objectives)
                    {
                        ProgressionLogger.Debug($"{regionMgr.RegionName} {objective.Name} {objective.State}");
                    }
                    foreach (var keep in regionMgr.Campaign.Keeps)
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
                    foreach (var apocBattleFrontStatus in BattleFrontStatuses)
                    {
                        apocBattleFrontStatus.Locked = true;
                        apocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                        // Determine what the "start" realm this battlefront should be locked to.
                        apocBattleFrontStatus.LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).DefaultRealmLock;
                        apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                        apocBattleFrontStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                    }

                    if (regionMgr.Campaign == null)
                        continue;

                    regionMgr.Campaign.VictoryPointProgress = new VictoryPointProgress();

                    foreach (var objective in regionMgr.Campaign.Objectives)
                    {
                        objective.LockObjective(Realms.REALMS_REALM_NEUTRAL, false);
                        ProgressionLogger.Debug($" Locking BO to Neutral {objective.Name} {objective.State} {objective.State}");
                    }
                    foreach (var keep in regionMgr.Campaign.Keeps)
                    {
                        keep.LockKeep(Realms.REALMS_REALM_NEUTRAL, false, false);
                        ProgressionLogger.Debug($" Locking Keep to Neutral {keep.Name} {keep.KeepStatus} ");
                    }
                }
            }
        }

        public RVRProgression OpenActiveBattlefront()
        {
            try
            {
                // If this battlefront is the reset battlefront (ie Praag), reset all the progressions upon our return to it.
                if (this.ActiveBattleFront.ResetProgressionOnEntry == 1)
                {
                    ProgressionLogger.Info($" Resetting Progress. Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");
                    LockBattleFrontsAllRegions(4);
                    // Reset the status list.
                    BuildApocBattleFrontStatusList(this.BattleFrontProgressions);
                    // Tells the attached players about it.
                    WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, this);
                }

                var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
                ProgressionLogger.Info($"Opening battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");
                ProgressionLogger.Info($"Resetting VP Progress {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");
                activeRegion.Campaign.VictoryPointProgress.Reset(activeRegion.Campaign);

                ProgressionLogger.Info($"Resetting BFStatus {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");
                // Find and update the status of the battlefront status.
                foreach (var apocBattleFrontStatus in BattleFrontStatuses)
                {
                    if (apocBattleFrontStatus.BattleFrontId == this.ActiveBattleFront.BattleFrontId)
                    {
                        apocBattleFrontStatus.Locked = false;
                        apocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                        apocBattleFrontStatus.LockingRealm = Realms.REALMS_REALM_NEUTRAL;
                        apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                        apocBattleFrontStatus.LockTimeStamp = 0;
                    }
                }

                ProgressionLogger.Info($"Unlocking objectives {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

                if (activeRegion.Campaign == null)
                {
                    ProgressionLogger.Info($"activeRegion.Campaign is null");
                    return this.ActiveBattleFront;
                }

                if (activeRegion.Campaign.Objectives == null)
                {
                    ProgressionLogger.Warn($"activeRegion.Campaign (objectives) is null");
                    return this.ActiveBattleFront;
                }
                if (activeRegion.Campaign.Keeps == null)
                {
                    ProgressionLogger.Warn($"activeRegion.Campaign (keeps) is null");
                    return this.ActiveBattleFront;
                }


                foreach (var flag in activeRegion.Campaign.Objectives)
                {
                    if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                        flag.UnlockObjective();
                }

                ProgressionLogger.Info($"Unlocking keeps {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

                foreach (Keep keep in activeRegion.Campaign.Keeps)
                {
                    if (this.ActiveBattleFront.ZoneId == keep.ZoneId)
                        keep.NotifyPairingUnlocked();
                }

                return this.ActiveBattleFront;
            }
            catch (Exception e)
            {
                ProgressionLogger.Error($"Exception. Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName} {e.Message} {e.StackTrace}");
                throw;
            }
        }

        public RVRProgression LockActiveBattleFront(Realms realm)
        {
            var activeRegion = RegionMgrs.Single(x => x.RegionId == this.ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Locking battlefront in {activeRegion.RegionName} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

            LockBattleFrontStatus(this.ActiveBattleFront.BattleFrontId, realm, activeRegion.Campaign.VictoryPointProgress);

            foreach (var flag in activeRegion.Campaign.Objectives)
            {
                if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.LockObjective(realm, true);
            }

            foreach (Keep keep in activeRegion.Campaign.Keeps)
            {
                if (this.ActiveBattleFront.ZoneId == keep.ZoneId)
                    keep.LockKeep(realm, true, true);
            }

            activeRegion.Campaign.LockBattleFront(realm);

            // Use Locking Realm in the BFM, not the BF (BF applies to region)
            return this.ActiveBattleFront;
        }

        public void LockBattleFrontStatus(int battleFrontId, Realms lockingRealm, VictoryPointProgress vpp)
        {
            var activeStatus = BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);

            activeStatus.Locked = true;
            activeStatus.LockingRealm = lockingRealm;
            activeStatus.FinalVictoryPoint = vpp;
            activeStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
        }

        public BattleFrontStatus GetActiveBattleFrontStatus(int battleFrontId)
        {
            return BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);
        }


        public List<BattleFrontStatus> GetBattleFrontStatusList()
        {
            return this.BattleFrontStatuses;
        }

        public bool IsBattleFrontLocked(int battleFrontId)
        {
            foreach (var ApocBattleFrontStatus in BattleFrontStatuses)
            {
                if (ApocBattleFrontStatus.BattleFrontId == this.ActiveBattleFront.BattleFrontId)
                {
                    return ApocBattleFrontStatus.Locked;
                }
            }
            return false;
        }

        public BattleFrontStatus GetBattleFrontStatus(int battleFrontId)
        {
            return this.BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);
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
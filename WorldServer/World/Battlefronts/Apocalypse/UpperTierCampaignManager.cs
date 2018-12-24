using Common.Database.World.Battlefront;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.BattleFronts.Keeps;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class UpperTierCampaignManager : IBattleFrontManager
    {

        public static int POPULATION_BROADCAST_CHANCE = 0;
        public static int RALLY_CALL_BROADCAST_TIME_LAPSE = 6000;
        public static int RALLY_CALL_ORDER_BROADCAST_BOUNDARY = -5;
        public static int RALLY_CALL_DEST_BROADCAST_BOUNDARY = 5;

        private static readonly object LockObject = new object();
        private static readonly Logger ProgressionLogger = LogManager.GetLogger("RVRProgressionLogger");

        protected readonly EventInterface _EvtInterface = new EventInterface();

        public const int BATTLEFRONT_TIER = 4;
        public List<RegionMgr> RegionMgrs { get; }
        public List<RVRProgression> BattleFrontProgressions { get; }
        public RVRProgression ActiveBattleFront { get; set; }
        public List<BattleFrontStatus> BattleFrontStatuses { get; set; }
        public ImpactMatrixManager ImpactMatrixManagerInstance { get; set; }
        public BountyManager BountyManagerInstance { get; set; }
        public volatile int LastAAORallyCall = 0;


        public UpperTierCampaignManager(List<RVRProgression> _RVRT4Progressions, List<RegionMgr> regionMgrs)
        {
            BattleFrontProgressions = _RVRT4Progressions;
            RegionMgrs = regionMgrs;
            BattleFrontStatuses = new List<BattleFrontStatus>();
            ImpactMatrixManagerInstance = new ImpactMatrixManager();
            BountyManagerInstance = new BountyManager();
            if (_RVRT4Progressions != null)
                BuildApocBattleFrontStatusList(BattleFrontProgressions);

            LastAAORallyCall = FrameWork.TCPManager.GetTimeStamp();

            _EvtInterface.AddEvent(BroadcastPlayerMessages, 30000, 0);

        }

        public void Update(long tick)
        {
            _EvtInterface.Update(tick);
        }

        private void BroadcastPlayerMessages()
        {
            ProgressionLogger.Debug($"Checking player broadcast messages... {DateTime.Now.ToString("s")} Last : {LastAAORallyCall} Now : {FrameWork.TCPManager.GetTimeStamp()}");

            if (FrameWork.TCPManager.GetTimeStamp() >= RALLY_CALL_BROADCAST_TIME_LAPSE + LastAAORallyCall)
            {
                var activeBattleFrontId = WorldMgr.UpperTierCampaignManager.ActiveBattleFront.BattleFrontId;
                var activeBattleFrontStatus =
                    WorldMgr.UpperTierCampaignManager.GetActiveBattleFrontStatus(activeBattleFrontId);

                LastAAORallyCall = FrameWork.TCPManager.GetTimeStamp();

                // _aaoTracker.AgainstAllOddsMult is defined in multiples of 20 (eg 400 AAO is 20). Negative numbers means Order has AAO, Positive numbers means Dest has AAO
                if (this.GetActiveCampaign().AgainstAllOddsTracker.AgainstAllOddsMult <= RALLY_CALL_ORDER_BROADCAST_BOUNDARY)
                {
                    foreach (var player in Player._Players)
                    {
                        if (player.Realm == Realms.REALMS_REALM_ORDER)
                        {
                            // Only tell players that are valid in the tier
                            if (player.ValidInTier(4, true))
                            {
                                player.SendMessage(
                                    $"Your realm is under serious attack. Proceed with all haste to {activeBattleFrontStatus.Description}.",
                                    ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE);
                            }
                        }
                    }
                }

                if (this.GetActiveCampaign().AgainstAllOddsTracker.AgainstAllOddsMult >= RALLY_CALL_DEST_BROADCAST_BOUNDARY)
                {

                    foreach (var player in Player._Players)
                    {
                        if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                        {
                            if (player.ValidInTier(4, true))
                            {
                                player.SendMessage(
                                    $"Your realm is under serious attack. Proceed with all haste to {activeBattleFrontStatus.Description}.",
                                    ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets up the Battlefront status list with default values.
        /// </summary>
        /// <param name="battleFrontProgressions"></param>
        private void BuildApocBattleFrontStatusList(List<RVRProgression> battleFrontProgressions, CampaignRerollMode rerollMode = CampaignRerollMode.NONE)
        {
            lock (LockObject)
            {
                BattleFrontStatuses.Clear();
                foreach (var battleFrontProgression in battleFrontProgressions)
                {
                    this.BattleFrontStatuses.Add(new BattleFrontStatus(this.ImpactMatrixManagerInstance)
                    {
                        BattleFrontId = battleFrontProgression.BattleFrontId,
                        LockingRealm = (rerollMode == CampaignRerollMode.INIT)
                            ? (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == battleFrontProgression.BattleFrontId).LastOwningRealm
                            : (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == battleFrontProgression.BattleFrontId).DefaultRealmLock,
                        FinalVictoryPoint = new VictoryPointProgress(battleFrontProgression.OrderVP, battleFrontProgression.DestroVP),
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
            if (BattleFrontStatuses != null)
            {
                return BattleFrontStatuses.SingleOrDefault(x => x.RegionId == regionId);
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
        public void LockBattleFrontsAllRegions(int tier, CampaignRerollMode rerollMode = CampaignRerollMode.NONE)
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
                        if (rerollMode == CampaignRerollMode.INIT)
                            apocBattleFrontStatus.LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).LastOwningRealm;
                        else
                            apocBattleFrontStatus.LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).DefaultRealmLock;
                        apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                        apocBattleFrontStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                    }

                    if (regionMgr.Campaign == null)
                        continue;

                    regionMgr.Campaign.VictoryPointProgress = new VictoryPointProgress();

                    foreach (var objective in regionMgr.Campaign.Objectives)
                    {
                        if (rerollMode == CampaignRerollMode.INIT)
                        {
                            objective.LockObjective((Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm, false);
                            ProgressionLogger.Debug($" Locking BO to {(Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm} {objective.Name} {objective.State} {objective.State}");
                        }
                        else
                        {
                            objective.LockObjective(Realms.REALMS_REALM_NEUTRAL, false);
                            ProgressionLogger.Debug($" Locking BO to Neutral {objective.Name} {objective.State} {objective.State}");
                        }
                    }

                    foreach (var keep in regionMgr.Campaign.Keeps)
                    {
                        if (rerollMode == CampaignRerollMode.INIT)
                        {
                            keep.LockKeep((Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm, false, false);
                            ProgressionLogger.Debug($" Locking Keep {keep.Info.Name} to {(Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm} {keep.Name} {keep.KeepStatus} ");
                        }
                        else
                        {
                            keep.LockKeep(Realms.REALMS_REALM_NEUTRAL, false, false);
                            ProgressionLogger.Debug($" Locking Keep {keep.Info.Name} to Neutral {keep.Name} {keep.KeepStatus} ");
                        }
                    }
                }
            }
        }

        public RVRProgression OpenActiveBattlefront(CampaignRerollMode rerollMode = CampaignRerollMode.NONE)
        {
            try
            {
                // If this battlefront is the reset battlefront (ie Praag), reset all the progressions upon our return to it.
                //if (this.ActiveBattleFront.ResetProgressionOnEntry == 1)
                if (rerollMode == CampaignRerollMode.INIT || rerollMode == CampaignRerollMode.REROLL)
                {
                    ProgressionLogger.Info($" Resetting Progress. Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");
                    LockBattleFrontsAllRegions(4, rerollMode);
                    // Reset the status list.
                    BuildApocBattleFrontStatusList(BattleFrontProgressions, rerollMode);
                    // Tells the attached players about it.
                    WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, this);
                }

                var activeRegion = RegionMgrs.Single(x => x.RegionId == ActiveBattleFront.RegionId);
                ProgressionLogger.Info($"Opening battlefront in {activeRegion.RegionName} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");
                ProgressionLogger.Info($"Resetting VP Progress {activeRegion.RegionName} BF Id : {ActiveBattleFront.BattleFrontId} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");

                if (rerollMode == CampaignRerollMode.INIT)
                {
                    activeRegion.Campaign.VictoryPointProgress.OrderVictoryPoints = ActiveBattleFront.OrderVP;
                    activeRegion.Campaign.VictoryPointProgress.DestructionVictoryPoints = ActiveBattleFront.DestroVP;
                }
                else
                    activeRegion.Campaign.VictoryPointProgress.Reset(activeRegion.Campaign);

                ProgressionLogger.Info($"Resetting BFStatus {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

                // Find and update the status of the battlefront status.
                foreach (var apocBattleFrontStatus in BattleFrontStatuses)
                {
                    if (apocBattleFrontStatus.BattleFrontId == ActiveBattleFront.BattleFrontId)
                    {
                        apocBattleFrontStatus.Locked = false;
                        apocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                        apocBattleFrontStatus.LockingRealm = Realms.REALMS_REALM_NEUTRAL;
                        if (rerollMode == CampaignRerollMode.INIT)
                        {
                            apocBattleFrontStatus.FinalVictoryPoint =
                            new VictoryPointProgress(
                                BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).OrderVP,
                                BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).DestroVP
                                );
                        }
                        else
                        {
                            apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                        }
                        apocBattleFrontStatus.LockTimeStamp = 0;

                        // Reset the population for the battle front status
                        ProgressionLogger.Info($"InitializePopulationList {activeRegion.RegionName} BF Id : {ActiveBattleFront.BattleFrontId} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");
                        GetActiveCampaign().InitializePopulationList(ActiveBattleFront.BattleFrontId);
                    }
                }

                if (activeRegion.Campaign == null)
                {
                    ProgressionLogger.Info($"activeRegion.Campaign is null");
                    return ActiveBattleFront;
                }

                ProgressionLogger.Info($"Unlocking objectives {activeRegion.RegionName} BF Id : {this.ActiveBattleFront.BattleFrontId} Zone : {this.ActiveBattleFront.ZoneId} {this.ActiveBattleFrontName}");

                if (activeRegion.Campaign.Objectives == null)
                {
                    ProgressionLogger.Warn($"activeRegion.Campaign (objectives) is null");
                    return this.ActiveBattleFront;
                }

                foreach (var flag in activeRegion.Campaign.Objectives)
                {
                    if (this.ActiveBattleFront.ZoneId == flag.ZoneId)
                        flag.UnlockObjective();
                }

                ProgressionLogger.Info($"Unlocking keeps {activeRegion.RegionName} BF Id : {ActiveBattleFront.BattleFrontId} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");

                if (activeRegion.Campaign.Keeps == null)
                {
                    ProgressionLogger.Warn($"activeRegion.Campaign (keeps) is null");
                    return this.ActiveBattleFront;
                }

                foreach (Keep keep in activeRegion.Campaign.Keeps)
                {
                    if (ActiveBattleFront.ZoneId == keep.ZoneId)
                    {
                        ProgressionLogger.Debug($"Notifying Pairing unlocked Name : {keep.Info.Name} Zone : {keep.ZoneId} ");
                        keep.NotifyPairingUnlocked();
                    }
                }

                return ActiveBattleFront;
            }
            catch (Exception e)
            {
                ProgressionLogger.Error($"Exception. Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName} {e.Message} {e.StackTrace}");
                throw;
            }
        }

        public RVRProgression LockActiveBattleFront(Realms realm, int forceNumberBags = 0)
        {
            var activeRegion = RegionMgrs.Single(x => x.RegionId == ActiveBattleFront.RegionId);
            ProgressionLogger.Info($" Locking battlefront in {activeRegion.RegionName} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");

            LockBattleFrontStatus(ActiveBattleFront.BattleFrontId, realm, activeRegion.Campaign.VictoryPointProgress);

            foreach (var flag in activeRegion.Campaign.Objectives)
            {
                if (ActiveBattleFront.ZoneId == flag.ZoneId)
                    flag.LockObjective(realm, true);
            }

            foreach (Keep keep in activeRegion.Campaign.Keeps)
            {
                if (ActiveBattleFront.ZoneId == keep.ZoneId)
                    keep.LockKeep(realm, true, true);
            }

            activeRegion.Campaign.LockBattleFront(realm, forceNumberBags);

            // Use Locking Realm in the BFM, not the BF (BF applies to region)
            return ActiveBattleFront;
        }

        public void LockBattleFrontStatus(int battleFrontId, Realms lockingRealm, VictoryPointProgress vpp)
        {
            var activeStatus = BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);

            if (activeStatus == null)
                ProgressionLogger.Warn($"Could not locate Active Status for battlefront Id {battleFrontId}");

            activeStatus.Locked = true;
            activeStatus.LockingRealm = lockingRealm;
            activeStatus.FinalVictoryPoint = vpp;
            activeStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();

            ProgressionLogger.Info($"Locking BF Status {activeStatus.Description} to realm:{lockingRealm}");
        }

        public BattleFrontStatus GetActiveBattleFrontStatus(int battleFrontId)
        {
            return BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);
        }

      


        public List<BattleFrontStatus> GetBattleFrontStatusList()
        {
            return BattleFrontStatuses;
        }

        public bool IsBattleFrontLocked(int battleFrontId)
        {
            foreach (var ApocBattleFrontStatus in BattleFrontStatuses)
            {
                if (ApocBattleFrontStatus.BattleFrontId == ActiveBattleFront.BattleFrontId)
                {
                    return ApocBattleFrontStatus.Locked;
                }
            }
            return false;
        }

        public BattleFrontStatus GetBattleFrontStatus(int battleFrontId)
        {
            try
            {
                return BattleFrontStatuses.Single(x => x.BattleFrontId == battleFrontId);
            }
            catch (Exception e)
            {
                ProgressionLogger.Warn($"Battlefront Id : {battleFrontId} Exception : {e.Message} ");
                throw;
            }
        }

        /// <summary>
        /// Set the Active Pairing to be null. Not expected to be needed.
        /// </summary>
        public RVRProgression ResetBattleFrontProgression(CampaignRerollMode rerollMode = CampaignRerollMode.NONE)
        {
            ProgressionLogger.Debug($" Resetting battlefront progression...");

            if (rerollMode == CampaignRerollMode.INIT)
            {
                var list = BattleFrontProgressions.Select(x => x).Where(x => x.LastOpenedZone == 1).ToList();
                if (list == null || list.Count == 0)
                    ActiveBattleFront = GetBattleFrontByName("Praag");
                else
                    ActiveBattleFront = list.FirstOrDefault();
            }
            else
            {
                ActiveBattleFront = GetBattleFrontByName("Praag");
            }

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
        public RVRProgression AdvanceBattleFront(Realms lockingRealm, out CampaignRerollMode rerollMode)
        {
            if (lockingRealm == Realms.REALMS_REALM_ORDER)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.OrderWinProgression);
                ProgressionLogger.Debug($"Order Win : Advancing Battlefront from {ActiveBattleFrontName} to {newBattleFront.Description}");

                if (newBattleFront.ResetProgressionOnEntry == 1 && ActiveBattleFront.RegionId != newBattleFront.RegionId)
                    rerollMode = CampaignRerollMode.REROLL;
                else
                    rerollMode = CampaignRerollMode.NONE;

                UpdateRVRPRogression(lockingRealm, ActiveBattleFront, newBattleFront);

                return ActiveBattleFront = newBattleFront;
            }

            if (lockingRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.DestWinProgression);
                ProgressionLogger.Debug($"Destruction Win : Advancing Battlefront from {ActiveBattleFrontName} to {newBattleFront.Description}");

                if (newBattleFront.ResetProgressionOnEntry == 1 && ActiveBattleFront.RegionId != newBattleFront.RegionId)
                    rerollMode = CampaignRerollMode.REROLL;
                else
                    rerollMode = CampaignRerollMode.NONE;

                UpdateRVRPRogression(lockingRealm, ActiveBattleFront, newBattleFront);

                return ActiveBattleFront = newBattleFront;
            }

            rerollMode = CampaignRerollMode.REROLL;
            return ResetBattleFrontProgression(rerollMode);
        }

        private void UpdateRVRPRogression(Realms lockingRealm, RVRProgression oldProg, RVRProgression newProg)
        {
            oldProg.DestroVP = oldProg.OrderVP = 0;
            oldProg.LastOpenedZone = 0;
            oldProg.LastOwningRealm = (byte)lockingRealm;

            newProg.LastOwningRealm = 0;
            newProg.LastOpenedZone = 1;
        }
    }
}
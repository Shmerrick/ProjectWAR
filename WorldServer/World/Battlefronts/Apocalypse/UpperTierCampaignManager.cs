using Common.Database.World.Battlefront;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects;

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

            _EvtInterface.AddEvent(BroadcastPlayerMessages, 600000, 0);

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
                if (GetActiveCampaign().AgainstAllOddsTracker.AgainstAllOddsMult <= RALLY_CALL_ORDER_BROADCAST_BOUNDARY)
                {
                    foreach (var player in Player._Players)
                    {
                        if (player.Realm == Realms.REALMS_REALM_ORDER)
                        {
                            // Only tell players that are valid in the tier
                            if (player.ValidInTier(BATTLEFRONT_TIER, true))
                            {
                                player.SendMessage(
                                    $"Your realm is under serious attack. Proceed with all haste to {activeBattleFrontStatus.Description}.",
                                    ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE);
                            }
                        }
                    }
                }

                if (GetActiveCampaign().AgainstAllOddsTracker.AgainstAllOddsMult >= RALLY_CALL_DEST_BROADCAST_BOUNDARY)
                {

                    foreach (var player in Player._Players)
                    {
                        if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                        {
                            if (player.ValidInTier(BATTLEFRONT_TIER, true))
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
        private void BuildApocBattleFrontStatusList(List<RVRProgression> battleFrontProgressions)
        {
            lock (LockObject)
            {
                BattleFrontStatuses.Clear();
                foreach (var battleFrontProgression in battleFrontProgressions)
                {
                    // Keeps for this region.
                    var keeps = BattleFrontService.GetZoneKeeps(battleFrontProgression.RegionId, battleFrontProgression.ZoneId);
                    // BattleFront Objectives for this region.
                    var battlefieldObjectives = BattleFrontService.GetZoneBattlefrontObjectives(battleFrontProgression.RegionId, battleFrontProgression.ZoneId);

                    BattleFrontStatuses.Add(new BattleFrontStatus(ImpactMatrixManagerInstance, battleFrontProgression.BattleFrontId)
                    {
                        LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == battleFrontProgression.BattleFrontId).LastOwningRealm,
                        FinalVictoryPoint = new VictoryPointProgress(battleFrontProgression.OrderVP, battleFrontProgression.DestroVP),
                        OpenTimeStamp = 0,
                        LockTimeStamp = 0,
                        Locked = true,
                        RegionId = battleFrontProgression.RegionId,
                        Description = battleFrontProgression.Description,
                        ZoneId = battleFrontProgression.ZoneId,
                        KeepList = keeps,
                        BattlefieldObjectives = battlefieldObjectives

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
                        ProgressionLogger.Debug($"OBJECTIVE : {regionMgr.RegionName} {objective.Name} {objective.State}");
                    }
                    foreach (var keep in regionMgr.Campaign.Keeps)
                    {
                        ProgressionLogger.Debug($"KEEP : {regionMgr.RegionName} {keep.Info.Name} {keep.KeepStatus} ");
                        foreach (var keepDoor in keep.Doors)
                        {
                            ProgressionLogger.Debug($"++ DOOR : {keep.Info.Name} {keepDoor.Info.Number} {keepDoor.GameObject?.PctHealth} {keepDoor.GameObject?.IsAttackable}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Lock Battlefronts across all the regions.
        /// </summary>
        public void LockBattleFrontsAllRegions(int tier, bool forceDefaultRealm = false)
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
                        if (forceDefaultRealm)
                            apocBattleFrontStatus.LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).DefaultRealmLock;
                        else
                        {
                            apocBattleFrontStatus.LockingRealm = (Realms)BattleFrontProgressions.Single(x => x.BattleFrontId == apocBattleFrontStatus.BattleFrontId).LastOwningRealm;
                        }
                        apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                        apocBattleFrontStatus.LockTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                    }

                    if (regionMgr.Campaign == null)
                        continue;

                    regionMgr.Campaign.VictoryPointProgress = new VictoryPointProgress();

                    foreach (var objective in regionMgr.Campaign.Objectives)
                    {
                        if (forceDefaultRealm)
                            objective.OwningRealm = (Realms) regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.DefaultRealmLock;
                        else
                        {
                            objective.OwningRealm = (Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm;
                        }
                        //objective.fsm.Fire(CampaignObjectiveStateMachine.Command.OnLockZone);
                        objective.SetObjectiveLocked();
                        ProgressionLogger.Debug($" Locking BattlefieldObjective to {(Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm} {objective.Name} {objective.State} {objective.State}");
                    }

                    foreach (var keep in regionMgr.Campaign.Keeps)
                    {
                        if (forceDefaultRealm)
                            keep.PendingRealm =
                                (Realms) regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.DefaultRealmLock;
                        else
                        {
                            keep.PendingRealm =
                                (Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm;
                        }
                        //keep.fsm.Fire(SM.Command.OnLockZone);
                        keep.SetKeepLocked();
                        ProgressionLogger.Debug($" Locking Keep {keep.Info.Name} to {(Realms)regionMgr.Campaign.BattleFrontManager.ActiveBattleFront.LastOwningRealm} {keep.KeepStatus} ");
                    }
                }
            }
        }

        /// <summary>
        /// Open the active battlefront (which has been set in this class [ActiveBattleFront]).
        /// Reset the VPP for the active battlefront.
        /// </summary>
        /// <returns></returns>
        public RVRProgression OpenActiveBattlefront()
        {
            try
            {
                var activeRegion = RegionMgrs.Single(x => x.RegionId == ActiveBattleFront.RegionId);
                ProgressionLogger.Info($"Opening Active battlefront in {activeRegion.RegionName} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");

                activeRegion.Campaign.VictoryPointProgress.Reset(activeRegion.Campaign);
                ProgressionLogger.Info($"Resetting VP Progress {activeRegion.RegionName} BF Id : {ActiveBattleFront.BattleFrontId} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");


                // Find and update the status of the battlefront status.
                foreach (var apocBattleFrontStatus in BattleFrontStatuses)
                {
                    if (apocBattleFrontStatus.BattleFrontId == ActiveBattleFront.BattleFrontId)
                    {
                        lock (apocBattleFrontStatus)
                        {
                            ProgressionLogger.Info(
                                $"Resetting BFStatus {activeRegion.RegionName} BF Id : {ActiveBattleFront.BattleFrontId} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");

                            apocBattleFrontStatus.Locked = false;
                            apocBattleFrontStatus.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                            apocBattleFrontStatus.LockingRealm = Realms.REALMS_REALM_NEUTRAL;
                            apocBattleFrontStatus.FinalVictoryPoint = new VictoryPointProgress();
                            apocBattleFrontStatus.LockTimeStamp = 0;

                            // Reset the population for the battle front status
                            ProgressionLogger.Info(
                                $"InitializePopulationList {activeRegion.RegionName} BF Id : {ActiveBattleFront.BattleFrontId} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");
                            GetActiveCampaign().InitializePopulationList(ActiveBattleFront.BattleFrontId);

                            //GetActiveCampaign().StartWanderingMobs(ActiveBattleFront.ZoneId);
                        }
                    }
                }

                if (activeRegion.Campaign == null)
                {
                    ProgressionLogger.Info($"activeRegion.Campaign is null");
                    return ActiveBattleFront;
                }


                if (activeRegion.Campaign.Objectives == null)
                {
                    ProgressionLogger.Warn($"activeRegion.Campaign (objectives) is null");
                    return ActiveBattleFront;
                }

                ProgressionLogger.Info($"Unlocking objectives {activeRegion.RegionName} BF Id : {ActiveBattleFront.BattleFrontId} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");
                foreach (var flag in activeRegion.Campaign.Objectives)
                {
                    if (ActiveBattleFront.ZoneId == flag.ZoneId)
                    {
                        flag.OpenBattleFront();
                        //flag.SetObjectiveSafe();
                    }
                }

                if (activeRegion.Campaign.Keeps == null)
                {
                    ProgressionLogger.Warn($"activeRegion.Campaign (keeps) is null");
                    return ActiveBattleFront;
                }

                ProgressionLogger.Info($"Unlocking keeps {activeRegion.RegionName} BF Id : {ActiveBattleFront.BattleFrontId} Zone : {ActiveBattleFront.ZoneId} {ActiveBattleFrontName}");
                foreach (var keep in activeRegion.Campaign.Keeps)
                {
                    if (ActiveBattleFront.ZoneId == keep.ZoneId)
                    {
                        ProgressionLogger.Debug($"Informing Keep of Open battlefront Name : {keep.Info.Name} Zone : {keep.ZoneId} ");
                        keep.OpenBattleFront();
                    }
                }

                activeRegion.Campaign.DestructionDominationCounter = Program.Config.DestructionDominationTimerLength;
                activeRegion.Campaign.OrderDominationCounter = Program.Config.OrderDominationTimerLength;


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
                {
                    flag.OwningRealm = realm;
                    flag.SetObjectiveLocked();
                }
                
            }

            foreach (BattleFrontKeep keep in activeRegion.Campaign.Keeps)
            {
                if (ActiveBattleFront.ZoneId == keep.ZoneId)
                    keep.OnLockZone(realm);
            }
            ProgressionLogger.Debug($"Removing any siege from active region");
            // Destroy any active siege in this zone.
            try
            {
                var siegeInRegion = activeRegion?.Objects.Where(x=>x is Siege);
                foreach (var siege in siegeInRegion)
                {
                    if (siege is Siege)
                    {
                        ProgressionLogger.Debug($"Calling Destroy on {siege.Name}");
                        siege.Destroy();
                    }
                }

            }
            catch (Exception e)
            {
                ProgressionLogger.Debug($"{e.Message}{e.StackTrace}");
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
        /// Get the ActiveBattleFront to be Praag if no zones are marked open in rvr_progression, otherwise use the LastOpenedZone
        /// </summary>
        public RVRProgression GetActiveBattleFrontFromProgression()
        {
            ProgressionLogger.Debug($" Getting battlefront progression...");

            var list = BattleFrontProgressions.Select(x => x).Where(x => x.LastOpenedZone == 1).ToList();
            if (list == null || list.Count == 0)
                ActiveBattleFront = GetBattleFrontByName("Praag");
            else
                ActiveBattleFront = list.FirstOrDefault();

            ProgressionLogger.Debug($"Active : {ActiveBattleFrontName}");
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
        /// Given a realm that locked the current Active Battlefront, find the next Battlefront.
        /// </summary>
        public RVRProgression AdvanceBattleFront(Realms lockingRealm)
        {
            if (lockingRealm == Realms.REALMS_REALM_ORDER)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.OrderWinProgression);
                ProgressionLogger.Debug($"Order Win : Advancing Battlefront from {ActiveBattleFrontName} to {newBattleFront.Description}");
                return ActiveBattleFront = newBattleFront;
            }

            if (lockingRealm == Realms.REALMS_REALM_DESTRUCTION)
            {
                var newBattleFront = GetBattleFrontByBattleFrontId(ActiveBattleFront.DestWinProgression);
                ProgressionLogger.Debug($"Destruction Win : Advancing Battlefront from {ActiveBattleFrontName} to {newBattleFront.Description}");
                return ActiveBattleFront = newBattleFront;
            }

            return GetActiveBattleFrontFromProgression();
        }

        public void UpdateRVRPRogression(Realms lockingRealm, RVRProgression oldProg, RVRProgression newProg)
        {
            oldProg.DestroVP = oldProg.OrderVP = 0;
            oldProg.LastOpenedZone = 0;
            oldProg.LastOwningRealm = (byte)lockingRealm;

            newProg.LastOwningRealm = 0;
            newProg.LastOpenedZone = 1;
        }


        [ConsoleHandler("battlefront-audit", 1, "Audit Battlefront <Tier>")]
        public class AuditBattleFrontConsole : IConsoleHandler
        {
            public bool HandleCommand(string command, List<string> args)
            {
                string tier = args[0];

                WorldMgr.UpperTierCampaignManager.AuditBattleFronts(Convert.ToInt32(tier));

                return true;

            }
        }
    }
}
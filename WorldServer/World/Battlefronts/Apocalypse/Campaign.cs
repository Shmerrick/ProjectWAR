using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using WorldServer.Managers;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Positions;


namespace WorldServer.World.Battlefronts.Apocalypse
{


    /// <summary>
    /// Represents an open RVR front in a given Region (1 Region -> n Zones) Eg Region 14 (T2 Emp -> Zone 101 Troll Country & Zone 107 Ostland
    /// </summary>
    public class Campaign
    {
        public static IObjectDatabase Database = null;

        public static int DOMINATION_POINTS_REQUIRED = Program.Config.DominationPointsRequired;
        public static int FORT_DEFENCE_TIMER = Program.Config.FortDefenceTimer;  // 15 mins is 900k.
        static readonly object LockObject = new object();

        private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public VictoryPointProgress VictoryPointProgress { get; set; }
        public RegionMgr Region { get; set; }
        public IBattleFrontManager BattleFrontManager { get; set; }
        public IApocCommunications CommunicationsEngine { get; }
        // List of battlefront statuses for this Campaign
        public List<BattleFrontStatus> ApocBattleFrontStatuses => GetBattleFrontStatuses(Region.RegionId);
        public BattleFrontStatus ActiveBattleFrontStatus => BattleFrontManager.GetBattleFrontStatus(BattleFrontManager.ActiveBattleFront.BattleFrontId);
        /// <summary>
        /// A list of keeps within this Campaign.
        /// </summary>
        public readonly List<BattleFrontKeep> Keeps = new List<BattleFrontKeep>();
        public string ActiveCampaignName => BattleFrontManager.ActiveBattleFrontName;

        protected readonly EventInterface _EvtInterface = new EventInterface();

        public SiegeManager SiegeManager { get; set; }

        public HashSet<Player> PlayersInLakeSet;
        public List<BattlefieldObjective> Objectives;

        public ConcurrentDictionary<int, int> OrderPlayerPopulationList = new ConcurrentDictionary<int, int>();
        public ConcurrentDictionary<int, int> DestructionPlayerPopulationList = new ConcurrentDictionary<int, int>();

        private volatile int _orderCount = 0;
        private volatile int _destroCount = 0;

        #region Against All Odds

        private readonly HashSet<Player> _playersInLakeSet = new HashSet<Player>();

        public readonly List<Player> _orderInLake = new List<Player>();
        public readonly List<Player> _destroInLake = new List<Player>();

        public int _totalMaxOrder = 0;
        public int _totalMaxDestro = 0;


        public int AgainstAllOddsMult => AgainstAllOddsTracker.AgainstAllOddsMult;

        #endregion


        public AAOTracker AgainstAllOddsTracker;
        private RVRRewardManager _rewardManager;

        public string ActiveZoneName { get; }
        public bool DefenderPopTooSmall { get; set; }
        public int Tier { get; set; }

        public int DestructionDominationTimerLength { get; set; }
        public int OrderDominationTimerLength { get; set; }

        public int DestructionDominationCounter { get; set; }
        public int OrderDominationCounter { get; set; }

        public RegionLockManager RegionLockManager { get; set; }
        public IRewardManager RewardManager { get; set; }



        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="regionMgr"></param>
        /// <param name="objectives"></param>
        /// <param name="players"></param>
        public Campaign(RegionMgr regionMgr,
            List<BattlefieldObjective> objectives,
            HashSet<Player> players,
            IBattleFrontManager bfm,
            IApocCommunications communicationsEngine)
        {
            Region = regionMgr;
            VictoryPointProgress = new VictoryPointProgress();
            PlayersInLakeSet = players;
            Objectives = objectives;
            BattleFrontManager = bfm;
            CommunicationsEngine = communicationsEngine;

            Tier = (byte)Region.GetTier();
            PlaceObjectives();

            LoadKeeps();
            AgainstAllOddsTracker = new AAOTracker();
            _rewardManager = new RVRRewardManager();
            SiegeManager = new SiegeManager();
            

            DestructionDominationCounter = Program.Config.DestructionDominationTimerLength;
            OrderDominationCounter = Program.Config.OrderDominationTimerLength;

            _EvtInterface.AddEvent(UpdateVictoryPoints, 6000, 0);

            _EvtInterface.AddEvent(UpdateBOs, 5000, 0);
            // Tell each player the RVR status
            _EvtInterface.AddEvent(UpdateRVRStatus, 120000, 0);
            // Recalculate AAO
            _EvtInterface.AddEvent(UpdateAAOBuffs, 30000, 0);
            _EvtInterface.AddEvent(DetermineCaptains, 120000, 0);
            // Check RVR zone for highest contributors (Captains)
            _EvtInterface.AddEvent(SavePlayerContribution, 180000, 0);
            // record metrics
            _EvtInterface.AddEvent(RecordMetrics, 600000, 0);
            _EvtInterface.AddEvent(DestructionDominationCheck, 60000, 0);
            _EvtInterface.AddEvent(OrderDominationCheck, 60000, 0);
            _EvtInterface.AddEvent(UpdateCampaignObjectiveBuffs, 10000, 0);
            _EvtInterface.AddEvent(CheckKeepTimers, 10000, 0);
            _EvtInterface.AddEvent(UpdateKeepResources, 60000, 0);
            _EvtInterface.AddEvent(IPCheck, 180000, 0);
            // _EvtInterface.AddEvent(RefreshObjectiveStatus, 20000, 0);
            _EvtInterface.AddEvent(CountdownFortDefenceTimer, FORT_DEFENCE_TIMER, 0);

            RegionLockManager = new RegionLockManager(Region);
        }

        /// <summary>
        /// Loop through players in the campaign and if any have the same IP - inform a GM.
        /// </summary>
        private void IPCheck()
        {
            var hash = new HashSet<string>();
            if (PlayersInLakeSet == null)
                return;
            foreach (var item in PlayersInLakeSet)
            {
                var ipAddress = item?.Client?._Account?.Ip;
                if (ipAddress != "")
                {
                    if (item.Client._Account.GmLevel == 1)
                    {
                        if (!hash.Add(ipAddress))
                        {
                            PlayerUtil.SendGMBroadcastMessage(Player._Players,
                                $"{item.Name} has a duplicate IP address in game.");
                        }
                    }
                }
            }
        }

        private void DetermineCaptains()
        {
            RealmCaptainManager.DetermineCaptains(BattleFrontManager, Region);
        }

        /// <summary>
        /// If there is a keep under attack, check it's defence timer count down.
        /// </summary>
        private void CountdownFortDefenceTimer()
        {
            // If its a fort, not locked and the active zone
            var k = Keeps.SingleOrDefault(x => x.IsFortress() && x.ZoneId == ActiveBattleFrontStatus.ZoneId && x.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED);
            k?.CountdownFortDefenceTimer((int)FORT_DEFENCE_TIMER / 1000 / 60);
        }


        /// <summary>
        /// Inform all players in the active battlefront about the objective status
        /// </summary>
        private void RefreshObjectiveStatus()
        {
            var status = ActiveBattleFrontStatus;
            if (status.Locked)
                return;

            if (status != null)
            {
                lock (status)
                {
                    BattlefrontLogger.Trace($"Checking RefreshObjectiveStatus...");
                    var playersToAnnounceTo = Player._Players.Where(x => !x.IsDisposed
                                                                         && x.IsInWorld()
                                                                         && x.CbtInterface.IsPvp
                                                                         && x.ScnInterface.Scenario == null
                                                                         && x.ZoneId == status.ZoneId
                                                                         && x.Region.RegionId == status.RegionId);

                    foreach (var player in playersToAnnounceTo)
                    {
                        SendObjectives(player, Objectives.Where(x => x.ZoneId == status.ZoneId));
                    }
                }
            }
        }

        private void CheckKeepTimers()
        {
            // There is a race condition here.
            var activeCampaign = BattleFrontManager.GetActiveCampaign();
            var status = activeCampaign?.ActiveBattleFrontStatus;
            if (status != null)
            {
                lock (status)
                {
                    BattlefrontLogger.Trace($"Checking Keep Timers...");
                    if (status.RegionId == Region.RegionId)
                    {
                        foreach (var keep in Keeps)
                        {
                            if (keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                            {
                                keep.CheckTimers();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Disabled at the moment - intent is to track who was close to the lord for when rewards are handed out.
        /// </summary>
        private void CheckKeepPlayersInvolved()
        {
            // There is a race condition here.
            var activeCampaign = BattleFrontManager.GetActiveCampaign();
            var status = activeCampaign?.ActiveBattleFrontStatus;
            if (status != null)
            {
                lock (status)
                {
                    BattlefrontLogger.Trace($"Checking Keep Players Involved...");
                    if (status.RegionId == Region.RegionId)
                    {
                        foreach (var keep in Keeps)
                        {
                            keep.CheckKeepPlayersInvolved();
                        }
                    }
                }
            }
        }

        private void UpdateKeepResources()
        {
            // There is a race condition here.
            var activeCampaign = BattleFrontManager.GetActiveCampaign();
            var status = activeCampaign?.ActiveBattleFrontStatus;
            if (status != null)
            {
                lock (status)
                {
                    BattlefrontLogger.Trace($"Checking Keep Resources...");

                    if (NumberDestructionKeepsInZone() == 2)
                    {
                        // Place Siege merchant out the front of the WC
                        // TODO
                    }
                    if (NumberOrderKeepsInZone() == 2)
                    {
                        // Place Siege merchant out the front of the WC
                        // TODO
                    }
                }
            }
        }

        public int NumberOrderKeepsInZone()
        {
            var activeCampaign = BattleFrontManager.GetActiveCampaign();
            var status = activeCampaign?.ActiveBattleFrontStatus;
            var orderCount = 0;


            if (status != null)
            {
                foreach (var keep in status.KeepList)
                {
                    if ((Realms)keep.Realm == Realms.REALMS_REALM_ORDER)
                    {
                        orderCount++;
                    }
                }
            }

            return orderCount;
        }

        public int NumberDestructionKeepsInZone()
        {
            var activeCampaign = BattleFrontManager.GetActiveCampaign();
            var status = activeCampaign?.ActiveBattleFrontStatus;
            var destCount = 0;


            if (status != null)
            {
                foreach (var keep in status.KeepList)
                {
                    if ((Realms)keep.Realm == Realms.REALMS_REALM_DESTRUCTION)
                    {
                        destCount++;
                    }
                }
            }

            return destCount;
        }

        private void DestructionDominationCheck()
        {
            var status = BattleFrontManager.GetActiveCampaign().ActiveBattleFrontStatus;

            // Only worry about the battlefrontstatus in this region.
            if (status.RegionId != Region.RegionId)
                return;

            if (BattleFrontManager.GetActiveCampaign().Tier != 4)
                return;

            var objectives = BattleFrontManager.GetActiveCampaign().Objectives.Where(x => x.ZoneId == status.ZoneId);
            foreach (var battlefieldObjective in objectives)
            {
                if (battlefieldObjective.State != StateFlags.Locked)
                {
                    return;
                }
                if (battlefieldObjective.OwningRealm != Realms.REALMS_REALM_DESTRUCTION)
                {
                    return;
                }
            }
            var keeps = BattleFrontManager.GetActiveCampaign().Keeps.Where(x => x.ZoneId == status.ZoneId);
            foreach (var battleFrontKeep in keeps)
            {
                if (battleFrontKeep.KeepStatus != KeepStatus.KEEPSTATUS_SAFE)
                {
                    return;
                }
                if (battleFrontKeep.Realm != Realms.REALMS_REALM_DESTRUCTION)
                {
                    return;
                }

                if (battleFrontKeep.Fortress)
                    return;

            }

            

            DestructionDominationCounter--;

            if (DestructionDominationCounter <= 0)
            {
                BattlefrontLogger.Info($"Destruction Domination Victory!");
                NotifyPlayersOfDomination($"Destruction Domination Victory!", status);
                VictoryPointProgress.DestructionVictoryPoints = BattleFrontConstants.LOCK_VICTORY_POINTS;
            }
            else
            {
                NotifyPlayersOfDomination($"Destruction is dominating - {DestructionDominationCounter} minutes remain", status);
            }

        }

        private void OrderDominationCheck()
        {
            var status = BattleFrontManager.GetActiveCampaign().ActiveBattleFrontStatus;

            // Only worry about the battlefrontstatus in this region.
            if (status.RegionId != Region.RegionId)
                return;

            if (BattleFrontManager.GetActiveCampaign().Tier != 4)
                return;

            var objectives = BattleFrontManager.GetActiveCampaign().Objectives.Where(x => x.ZoneId == status.ZoneId);
            foreach (var battlefieldObjective in objectives)
            {
                if (battlefieldObjective.State != StateFlags.Locked)
                {
                    return;
                }
                if (battlefieldObjective.OwningRealm != Realms.REALMS_REALM_ORDER)
                {
                    return;
                }
            }
            var keeps = BattleFrontManager.GetActiveCampaign().Keeps.Where(x => x.ZoneId == status.ZoneId);
            foreach (var battleFrontKeep in keeps)
            {
                if (battleFrontKeep.KeepStatus != KeepStatus.KEEPSTATUS_SAFE)
                {
                    return;
                }
                if (battleFrontKeep.Realm != Realms.REALMS_REALM_ORDER)
                {
                    return;
                }
                
                if (battleFrontKeep.Fortress)
                    return;
            }

            OrderDominationCounter--;

            if (OrderDominationCounter <= 0)
            {
                BattlefrontLogger.Info($"Order Domination Victory!");
                NotifyPlayersOfDomination($"Order Domination Victory!", status);
                VictoryPointProgress.OrderVictoryPoints = BattleFrontConstants.LOCK_VICTORY_POINTS;
            }
            else
            {
                NotifyPlayersOfDomination($"Order is dominating - {OrderDominationCounter} minutes remain", status);
            }

        }
        


        private int SecondsToNearestMinute(int seconds)
        {
            return Convert.ToInt32(Math.Round((double)seconds / 60, MidpointRounding.AwayFromZero));
        }

        

        private void NotifyPlayersOfDomination(string message, BattleFrontStatus status)
        {
            var playersToNotify = Player._Players.Where(x => !x.IsDisposed
                                                             && x.IsInWorld()
                                                             && x.CbtInterface.IsPvp
                                                             && x.ScnInterface.Scenario == null
                                                             && x.Region.RegionId == status.RegionId
                                                             && x.ZoneId == status.ZoneId);

            foreach (var player in playersToNotify)
            {
                player.SendClientMessage(message, ChatLogFilters.CHATLOGFILTERS_RVR);
                BattlefrontLogger.Debug($"{message}");
            }
        }

        private void BuffAssigned(NewBuff buff)
        {
            var newBuff = buff;
        }

        public void UpdateCampaignObjectiveBuffs()
        {
            // There is a race condition here.
            var activeCampaign = BattleFrontManager.GetActiveCampaign();
            var status = activeCampaign?.ActiveBattleFrontStatus;
            if (status != null)
            {
                lock (status)
                {
                    BattlefrontLogger.Trace($"Updating Campaign Objective Buffs...");
                    if (status.RegionId == Region.RegionId)
                    {
                        foreach (var objective in Objectives)
                        {
                            if (objective.BuffId != 0)
                            {
                                if (objective.ZoneId == status.ZoneId)
                                {
                                    if (objective.OwningRealm != Realms.REALMS_REALM_NEUTRAL)
                                    {
                                        var buffId = objective.BuffId;
                                        BattlefrontLogger.Trace($"Applying BuffId {buffId} to players.");
                                        var playersToApply = Player._Players.Where(x => !x.IsDisposed
                                                                                        && x.IsInWorld()
                                                                                        && x.CbtInterface.IsPvp
                                                                                        && x.ScnInterface.Scenario == null
                                                                                        && x.Region.RegionId == status.RegionId
                                                                                        && x.ZoneId == status.ZoneId
                                                                                        && x.Realm == objective.OwningRealm);

                                        foreach (var player in playersToApply)
                                        {
                                            player.BuffInterface.QueueBuff(
                                                new BuffQueueInfo(
                                                    player, player.Level, AbilityMgr.GetBuffInfo((ushort)buffId), BuffAssigned));
                                        }

                                        BattlefrontLogger.Trace($"Removing BuffId {buffId} from opposing players.");

                                        var playersToRemove = Player._Players.Where(x => !x.IsDisposed
                                                                                         && x.IsInWorld()
                                                                                         && x.CbtInterface.IsPvp
                                                                                         && x.ScnInterface.Scenario == null
                                                                                         && x.Region.RegionId == status.RegionId
                                                                                         && x.ZoneId == status.ZoneId
                                                                                         && x.Realm != objective.OwningRealm);

                                        foreach (var player in playersToRemove)
                                        {
                                            player.BuffInterface.RemoveBuffByEntry((ushort)buffId);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }



        public void UpdateDoorMsg()
        {
            var oVp = Region.Campaign.VictoryPointProgress.OrderVictoryPoints;
            var dVp = Region.Campaign.VictoryPointProgress.DestructionVictoryPoints;

            //get order/destro keeps
            var oKeep = Region.Campaign.Keeps.FirstOrDefault(x => x.Realm == Realms.REALMS_REALM_ORDER);
            var dKeep = Region.Campaign.Keeps.FirstOrDefault(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION);

            if (oKeep != null)
            {
                //update keep door health
                foreach (var door in oKeep.Doors)
                {
                    if (!door.GameObject.IsDead)
                    {
                        BattlefrontLogger.Debug("ORDER " + Region.RegionName + " | Door " + door.Info.Number + " Health: " + door.GameObject.Health);
                    }
                }
            }

            if (dKeep != null)
            {
                //update keep door health
                foreach (var door in dKeep.Doors)
                {
                    if (!door.GameObject.IsDead)
                    {
                        BattlefrontLogger.Debug("DESTRO" + Region.RegionName + " | Door " + door.Info.Number + " Health: " + door.GameObject.Health);
                    }
                }
            }

        }

        public void InitializePopulationList(int battlefrontId)
        {
            if (OrderPlayerPopulationList.ContainsKey(battlefrontId))
            {
                OrderPlayerPopulationList[battlefrontId] = 0;
            }
            else
            {
                OrderPlayerPopulationList.TryAdd(battlefrontId, 0);
            }
            if (DestructionPlayerPopulationList.ContainsKey(battlefrontId))
            {
                DestructionPlayerPopulationList[battlefrontId] = 0;
            }
            else
            {
                DestructionPlayerPopulationList.TryAdd(battlefrontId, 0);
            }
        }

        public BattleFrontStatus GetActiveBattleFrontStatus()
        {
            if (ApocBattleFrontStatuses.Count == 0)
            {
                BattlefrontLogger.Error("No BattlefrontStatuses have been created!");
                throw new Exception("No BattlefrontStatuses have been created!");
            }
            try
            {
                return ApocBattleFrontStatuses.Single(x => x.Locked == false);
            }
            catch (Exception e)
            {
                BattlefrontLogger.Warn($"Exception ALL BF Statuses are LOCKED : {e.Message} {e.StackTrace}");
                throw;
            }
            ;

        }


        private void RecordMetrics()
        {
            CampaignMetrics.RecordMetrics(BattlefrontLogger, Tier, Region, BattleFrontManager);
        }

        /// <summary>
        /// Return the list of Battlefront statuses for a give region.
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public List<BattleFrontStatus> GetBattleFrontStatuses(int regionId)
        {
            return BattleFrontManager.GetBattleFrontStatusList().Where(x => x.RegionId == regionId).ToList();
        }


        private void PlaceObjectives()
        {
            foreach (var battleFrontObjective in Objectives)
            {
                Region.AddObject(battleFrontObjective, battleFrontObjective.ZoneId);
                battleFrontObjective.BattleFront = this;
            }
        }

        public string GetBattleFrontStatus()
        {
            return $"Victory Points Progress for {ActiveCampaignName} : {VictoryPointProgress.ToString()}";
        }


        private void UpdateAAOBuffs()
        {
            var orderPlayersInZone = PlayerUtil.GetOrderPlayersInZone(BattleFrontManager.ActiveBattleFront.ZoneId);
            var destPlayersInZone = PlayerUtil.GetDestPlayersInZone(BattleFrontManager.ActiveBattleFront.ZoneId);

            var allPlayersInZone = new List<Player>();
            allPlayersInZone.AddRange(destPlayersInZone);
            allPlayersInZone.AddRange(orderPlayersInZone);

            if (Tier == 4)
            {
                BattlefrontLogger.Trace(
                    $"Calculating AAO. {ActiveCampaignName} Order players : {orderPlayersInZone.Count} Dest players : {destPlayersInZone.Count}");
            }

            AgainstAllOddsTracker.RecalculateAAO(allPlayersInZone, orderPlayersInZone.Count, destPlayersInZone.Count);

            // Used to set keep defence sizes
            foreach (var keep in Keeps)
            {
                keep.UpdateCurrentAAO(AgainstAllOddsTracker.AgainstAllOddsMult);
            }

        }


        private void SavePlayerContribution()
        {
            lock (ActiveBattleFrontStatus.ContributionManagerInstance)
            {
                if (ActiveBattleFrontStatus.RegionId == Region.RegionId)
                {
                    PlayerContributionManager.SavePlayerContribution(ActiveBattleFrontStatus.BattleFrontId, ActiveBattleFrontStatus.ContributionManagerInstance);
                }
            }
        }

        private void UpdateRVRStatus()
        {
            // Update players with status of campaign
            foreach (Player plr in Region.Players)
            {
                if (Region.GetTier() == 1)
                {
                    plr.SendClientMessage($"RvR Status : {BattleFrontManager.GetActiveCampaign().GetBattleFrontStatus()}", ChatLogFilters.CHATLOGFILTERS_RVR);
                }
                else
                {
                    plr.SendClientMessage($"RvR Status : {BattleFrontManager.GetActiveCampaign().GetBattleFrontStatus()}", ChatLogFilters.CHATLOGFILTERS_RVR);
                }
            }
            VictoryPointProgress.UpdateStatus(this);
            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);

            // also save into db
            RVRProgressionService.SaveRVRProgression(WorldMgr.LowerTierCampaignManager.BattleFrontProgressions);
            RVRProgressionService.SaveRVRProgression(WorldMgr.UpperTierCampaignManager.BattleFrontProgressions);
        }

        /// <summary>
        /// Loads keeps, keep units and doors.
        /// </summary>
        private void LoadKeeps()
        {
            List<Keep_Info> keeps = BattleFrontService.GetKeepInfos(Region.RegionId);

            if (keeps == null)
                return; // t1 or database lack

            BattlefrontLogger.Debug($"Loading {keeps.Count} keeps for Region {Region.RegionId}");
            foreach (Keep_Info info in keeps)
            {
                BattleFrontKeep keep = new BattleFrontKeep(info, (byte)Tier, Region, new KeepCommunications(), info.IsFortress);
                keep.Realm = (Realms)keep.Info.Realm;

                Keeps.Add(keep);
                keep.Load();

                Region.AddObject(keep, info.ZoneId);

                if (info.Creatures != null)
                {
                    BattlefrontLogger.Trace($"Adding {info.Creatures.Count} mobs for Keep {info.KeepId}");
                    foreach (Keep_Creature crea in info.Creatures)
                    {
                        if (!crea.IsPatrol)
                            keep.Creatures.Add(new KeepNpcCreature(Region, crea, keep));
                    }
                }

                if (info.Doors != null)
                {
                    BattlefrontLogger.Trace($"Adding {info.Doors.Count} doors for Keep {info.KeepId}");
                    foreach (Keep_Door door in info.Doors)
                        keep.Doors.Add(new KeepDoor(Region, door, keep));
                }
            }
        }

        public BattleFrontKeep GetClosestKeep(Point3D destPos, ushort zoneId, KeepStatus excludedKeepStatus = KeepStatus.KEEPSTATUS_RUINED)
        {
            BattleFrontKeep bestKeep = null;
            ulong bestDist = 0;

            foreach (var keep in Keeps)
            {
                ulong curDist = keep.GetDistanceSquare(destPos);

                if (bestKeep == null || curDist < bestDist)
                {
                    // Dont process keeps that are in excluded status
                    if (keep.KeepStatus == excludedKeepStatus)
                        continue;

                    if (keep.ZoneId == zoneId)
                    {
                        bestKeep = keep;
                        bestDist = keep.GetDistanceSquare(destPos);
                    }
                }
            }

            return bestKeep;
        }



        public BattleFrontKeep GetClosestFriendlyKeep(Point3D destPos, Realms myRealm)
        {
            BattleFrontKeep bestKeep = null;
            ulong bestDist = 0;

            foreach (var keep in Keeps)
            {
                if (keep.Realm == myRealm)
                {
                    ulong curDist = keep.GetDistanceSquare(destPos);

                    if (bestKeep == null || curDist < bestDist)
                    {
                        bestKeep = keep;
                        bestDist = keep.GetDistanceSquare(destPos);
                    }
                }
            }

            return bestKeep;
        }

        public List<BattleFrontKeep> GetZoneKeeps(ushort zoneId)
        {
            return Keeps.Where(x => x.ZoneId == zoneId).ToList();
        }



        //public void WriteCaptureStatus(PacketOut Out)
        //{
        //    // Not implemented.
        //    BattlefrontLogger.Trace(".");
        //}

        /// <summary>
        /// Writes the current zone capture status (gauge in upper right corner of client UI).
        /// </summary>
        /// <param name="Out">Packet to write</param>
        /// <param name="lockingRealm">Realm that is locking the Campaign</param>
        public void WriteCaptureStatus(PacketOut Out, Realms lockingRealm)
        {
            BattlefrontLogger.Trace(".");
            Out.WriteByte(0);
            float orderPercent, destroPercent = 0;
            switch (lockingRealm)
            {
                case Realms.REALMS_REALM_ORDER:
                    orderPercent = 80;
                    destroPercent = 20;
                    break;

                case Realms.REALMS_REALM_DESTRUCTION:
                    orderPercent = 20;
                    destroPercent = 80;
                    break;

                default:
                    orderPercent = (VictoryPointProgress.OrderVictoryPoints * 100) / BattleFrontConstants.LOCK_VICTORY_POINTS;
                    destroPercent = (VictoryPointProgress.DestructionVictoryPoints * 100) / BattleFrontConstants.LOCK_VICTORY_POINTS;
                    break;
            }

            BattlefrontLogger.Debug($"{ActiveCampaignName} : {(byte)orderPercent} {(byte)destroPercent}");

            Out.WriteByte((byte)orderPercent);
            Out.WriteByte((byte)destroPercent);
        }


        public void Update(long tick)
        {
            _EvtInterface.Update(tick);
            RegionLockManager.Update(tick);
        }

        /// <summary>
        /// Notifies the given player has entered the lake,
        /// removing it from the Campaign's active players list and setting the rvr buff(s).
        /// </summary>
        /// <param name="plr">Player to add, not null</param>
        public void NotifyEnteredLake(Player plr)
        {
            if (plr == null)
                return;

            if (!plr.ValidInTier(Tier, true))
                return;

            // Player list tracking
            lock (PlayersInLakeSet)
            {
                if (PlayersInLakeSet.Add(plr))
                {
                    // Which battlefrontId?
                    var battleFrontId = BattleFrontManager.ActiveBattleFront.BattleFrontId;
                    BattlefrontLogger.Info($"{plr.Name} {plr.Realm} BF Id : {battleFrontId}");
                    try
                    {
                        if (plr.Realm == Realms.REALMS_REALM_ORDER)
                        {
                            OrderPlayerPopulationList[battleFrontId] += 1;
                            _orderCount++;
                        }
                        else
                        {
                            DestructionPlayerPopulationList[battleFrontId] += 1;
                            _destroCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        BattlefrontLogger.Debug($"{OrderPlayerPopulationList.Count} {DestructionPlayerPopulationList.Count}");
                        BattlefrontLogger.Debug($"Could not add {plr.Name} to PopulationList. ");
                        BattlefrontLogger.Warn($"{ex.Message}");
                    }

                    // Tell the player about the objectives.
                    SendObjectives(plr);
                    // Update worldmap
                    WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);
                }
            }

            // Buffs
            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.FieldOfGlory), FogAssigned));

            // AAO
            AgainstAllOddsTracker.NotifyEnteredLake(plr);
        }

        /// <summary>
        /// Invoked by buff interface to remove field of glory if necessary.
        /// </summary>
        /// <param name="fogBuff">Buff that was created</param>
        public void FogAssigned(NewBuff fogBuff)
        {
            if (fogBuff == null || !(fogBuff.Caster is Player))
                return;

            lock (PlayersInLakeSet)
            {
                if (!PlayersInLakeSet.Contains(fogBuff.Caster))
                    fogBuff.BuffHasExpired = true;
            }
        }

        /// <summary>
        /// Notifies the given player has left the lake,
        /// removing it from the Campaign's active players lift and removing the rvr buff(s).
        /// </summary>
        /// <param name="plr">Player to remove, not null</param>
        public void NotifyLeftLake(Player plr)
        {
            if (!plr.ValidInTier(Tier, true))
                return;

            // Player list tracking
            lock (PlayersInLakeSet)
            {
                if (PlayersInLakeSet.Remove(plr))
                {
                    // Which battlefrontId?
                    var battleFrontId = BattleFrontManager.ActiveBattleFront.BattleFrontId;

                    try
                    {
                        if (plr.Realm == Realms.REALMS_REALM_ORDER)
                        {
                            OrderPlayerPopulationList[battleFrontId] -= 1;
                            _orderCount--;
                        }
                        else
                        {
                            DestructionPlayerPopulationList[battleFrontId] -= 1;
                            _destroCount--;
                        }
                    }
                    catch { }
                }
            }

            // Buffs
            plr.BuffInterface.RemoveBuffByEntry((ushort)GameBuffs.FieldOfGlory);

            // AAO
            AgainstAllOddsTracker.NotifyLeftLake(plr);
        }

        public void LockBattleObjectivesByZone(int zoneId, Realms realm)
        {
            foreach (var flag in Objectives)
            {
                if ((flag.ZoneId != zoneId) && (flag.RegionId == Region.RegionId))
                {
                    flag.OwningRealm = realm;
                    flag.SetObjectiveLocked();
                }
            }
        }

        public void LockBattleObjective(Realms realm, int objectiveToLock)
        {
            BattlefrontLogger.Debug($"Locking Battle Objective : {realm.ToString()}...");

            foreach (var flag in Objectives)
            {
                if (flag.Id == objectiveToLock)
                {
                    flag.OwningRealm = realm;
                    flag.SetObjectiveLocked();
                }
            }
        }

        /// <summary>
        /// Lock, Advance and handle rewards for Lock of Battlefront
        /// </summary>
        /// <param name="lockingRealm"></param>
        public void LockBattleFront(Realms lockingRealm, int forceNumberBags = 0)
        {
            var lockId = WriteZoneLockSummary(lockingRealm);

            BattlefrontLogger.Info($"*************************BATTLEFRONT LOCK-START [LockId:{lockId}]*******************");
            BattlefrontLogger.Info($"forceNumberBags = {forceNumberBags}");
            BattlefrontLogger.Info($"Locking Battlefront {ActiveCampaignName} to {lockingRealm.ToString()}...");

            string message = string.Concat(ActiveBattleFrontStatus.Description, " locked by ", (lockingRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"), "!");

            BattlefrontLogger.Debug(message);

            if (PlayersInLakeSet == null)
                BattlefrontLogger.Warn($"No players in the Lake!!");
            if (_rewardManager == null)
                BattlefrontLogger.Warn($"_rewardManager is null!!");

            BattlefrontLogger.Info($"*************************BATTLEFRONT LOCK-END [LockId:{lockId}] *********************");

        }

        private long WriteZoneLockSummary(Realms lockingRealm)
        {
            var lockId = Convert.ToInt64(DateTime.Now.ToString("yyyyMMddHHmm"));
            try
            {
                var zonelockSummary = new ZoneLockSummary
                {
                    LockId = lockId,
                    Description = $"Locking Battlefront {ActiveCampaignName} to {lockingRealm.ToString()}...",
                    DestroVP = (int)ActiveBattleFrontStatus.DestructionVictoryPointPercentage,
                    OrderVP = (int)ActiveBattleFrontStatus.OrderVictoryPointPercentage,
                    LockingRealm = (int)lockingRealm,
                    RegionId = ActiveBattleFrontStatus.RegionId,
                    Timestamp = DateTime.Now,
                    TotalPlayersAtLock = PlayerUtil.GetAllFlaggedPlayersInZone(BattleFrontManager.ActiveBattleFront.ZoneId).Count(),
                    EligiblePlayersAtLock = ActiveBattleFrontStatus.ContributionManagerInstance.GetEligiblePlayers(0).Count()
                };

                WorldMgr.Database.AddObject(zonelockSummary);
                BattlefrontLogger.Info($"Writing ZoneLockSummary. Lockid = {lockId}...");

                return lockId;
            }
            catch (Exception ex)
            {
                BattlefrontLogger.Error($"Could not write ZoneLockSummary {ex.Message} {ex.StackTrace}");
                return lockId;
            }


        }

        /// <summary>
        /// Generate zone lock rewards. 
        /// </summary>
        /// <param name="lockingRealm"></param>
        /// <param name="zoneId"></param>
        /// <param name="orderLootChest"></param>
        /// <param name="destructionLootChest"></param>
        /// <param name="lootOptions"></param>
        /// <param name="forceNumberBags">By default 0 allows the system to decide the number of bags, setting to -1 forces no rewards.</param>
        private void GenerateZoneLockRewards(Realms lockingRealm, int zoneId)
        {
           try
            { 
                var eligiblitySplits =
                    Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance.DetermineEligiblePlayers(BattlefrontLogger, lockingRealm);

                // Distribute RR, INF, etc to contributing players
                // Get All players in the zone and if they are not in the eligible list, they receive minor awards
                var allPlayersInZone = PlayerUtil.GetAllFlaggedPlayersInZone((ushort)zoneId);

                Region.Campaign.GetActiveBattleFrontStatus().RewardManagerInstance.DistributeZoneFlipBaseRewards(
                    eligiblitySplits.Item3,
                    eligiblitySplits.Item2, 
                    lockingRealm, 
                    Region.Campaign.GetActiveBattleFrontStatus().ContributionManagerInstance.GetMaximumContribution(), 
                    Tier == 1 ? 0.5f : 1f, 
                    allPlayersInZone);

                var fortZones = new List<int> { 4, 10, 104, 110, 204, 210 };
                if (fortZones.Contains((ushort)zoneId))
                {
                    return;
                }

               // For all eligible players present them with 5 invader crests (only for non-fort zones)
                foreach (var  player in eligiblitySplits.Item1)
                {
                    try
                    {
                        var zoneDescription = Region.Campaign.GetActiveBattleFrontStatus()?.Description;
                        Logger.Debug($"Assigning Invader Crests for Zone Flip {player.Key.Name}");
                        player.Key.SendClientMessage($"You have been awarded 5 Invader Crests - check your mail.", ChatLogFilters.CHATLOGFILTERS_LOOT);
                        Region.Campaign.GetActiveBattleFrontStatus().RewardManagerInstance.MailItem(player.Key.CharacterId, ItemService.GetItem_Info(208453), 5, zoneDescription , "Zone Flip", "Invader crests");

                        RecordZoneLockEligibilityHistory(player, lockingRealm, Region.Campaign.GetActiveBattleFrontStatus().ZoneId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn($"Could not mail invader crests to {player.Key.CharacterId} {ex.Message} {ex.StackTrace}");
                    }

                }
               

            }
            catch (Exception e)
            {
                BattlefrontLogger.Error($" GenerateZoneLockRewards : {e.Message} {e.StackTrace}");
                throw;
            }

        }

        private void RecordZoneLockEligibilityHistory(KeyValuePair<Player, int> player, Realms lockingRealm, int zoneId)
        {
            var zone = ZoneService.GetZone_Info((ushort)zoneId);

            var zoneLockEligibility = new ZoneLockEligibilityHistory
            {
                CharacterId = (int) player.Key.CharacterId,
                CharacterName = player.Key.Name,
                ContributionValue = player.Value,
                LockingRealm = (int) lockingRealm,
                Timestamp = DateTime.UtcNow,
                ZoneId = zoneId,
                ZoneName = zone.Name,
                Dirty = true
            };
            WorldMgr.Database.AddObject(zoneLockEligibility);

        }


        public void ClearDictionaries()
        {
            ActiveBattleFrontStatus.ContributionManagerInstance.ContributionDictionary.Clear();
            ActiveBattleFrontStatus.DestructionRealmCaptain = null;
            ActiveBattleFrontStatus.OrderRealmCaptain = null;
            BattleFrontManager.BountyManagerInstance.BountyDictionary.Clear();
            SiegeManager.DestroyAllSiege();
            SiegeManager = new SiegeManager();  //HACK TODO : fix
            // Remove rvr player contribution.
            SavePlayerContribution();

            BattlefrontLogger.Debug($"RVR Player Contribution, Contribution and Bounty Dictionaries cleared");
        }

        /// <summary>
        /// Helper function to determine whether the active battlefront progression associated with this battlefront is locked.
        /// </summary>
        /// <returns></returns>
        public bool IsBattleFrontLocked()
        {
            return BattleFrontManager.IsBattleFrontLocked(BattleFrontManager.ActiveBattleFront.BattleFrontId);
        }

        /// <summary>
        /// A scale factor for the general reward received from capturing a Battlefield Objective, which increases as more players join the zone.
        /// </summary>
        public float PopulationScaleFactor { get; private set; }

        /// <summary>
        /// A scale factor determined by the population ratio between the realms as determined by the maximum players they fielded over the last 15 minutes.
        /// </summary>
        private readonly float _relativePopulationFactor;

        /// <summary>
        /// Returns the enemy lockingRealm's population divided by the input lockingRealm's population.
        /// </summary>
        private float GetRelativePopFactor(Realms realm)
        {
            if (_relativePopulationFactor == 0)
                return 0;
            return realm == Realms.REALMS_REALM_DESTRUCTION ? _relativePopulationFactor : 1f / _relativePopulationFactor;
        }




        /// <summary>
        /// Sends information to a player about the objectives within a Campaign upon their entry.
        /// </summary>
        /// <param name="plr"></param>
        public void SendObjectives(Player plr, IEnumerable<BattlefieldObjective> filteredObjectives = null)
        {
            if (filteredObjectives == null)
            {
                foreach (var bo in Objectives)
                    bo.SendState(plr, false);
            }
            else
            {
                foreach (var bo in filteredObjectives)
                {
                    bo.SendState(plr, false);
                }
            }
        }

        private void UpdateBOs()
        {
            // RegionLockManager by Order/Dest
            if (IsBattleFrontLocked())
                return; // Nothing to do

            // Only update an active battlefront
            if (BattleFrontManager.ActiveBattleFront.RegionId != Region.RegionId)
                return;

            foreach (var flag in Objectives)
            {
                flag.Update(TCPManager.GetTimeStampMS());
            }



        }

        /// <summary>
        ///  Updates the victory points per lockingRealm and fires lock when necessary.
        /// </summary>
        private void UpdateVictoryPoints()
        {
            BattlefrontLogger.Trace($"Updating Victory Points for {ActiveCampaignName}");
            // RegionLockManager by Order/Dest
            if (IsBattleFrontLocked())
                return; // Nothing to do

            // Only update an active battlefront
            if (BattleFrontManager.ActiveBattleFront.RegionId != Region.RegionId)
                return;

            // Victory depends on objective securization in t1
            float orderVictoryPoints = VictoryPointProgress.OrderVictoryPoints;
            float destroVictoryPoints = VictoryPointProgress.DestructionVictoryPoints;

            // Victory points update
            VictoryPointProgress.OrderVictoryPoints = Math.Min(BattleFrontConstants.LOCK_VICTORY_POINTS, orderVictoryPoints);
            VictoryPointProgress.DestructionVictoryPoints = Math.Min(BattleFrontConstants.LOCK_VICTORY_POINTS, destroVictoryPoints);

            // update also rvr progression
            BattleFrontManager.ActiveBattleFront.OrderVP = (int)Math.Round(VictoryPointProgress.OrderVictoryPoints);
            BattleFrontManager.ActiveBattleFront.DestroVP = (int)Math.Round(VictoryPointProgress.DestructionVictoryPoints);



            ///
            /// Check to Lock and Advance the Battlefront
            /// 
            if (VictoryPointProgress.OrderVictoryPoints >= BattleFrontConstants.LOCK_VICTORY_POINTS)
            {
                var orderWarcampEntrance = BattleFrontService.GetWarcampEntrance(
                    (ushort)ActiveBattleFrontStatus.ZoneId, Realms.REALMS_REALM_ORDER);

                if (orderWarcampEntrance == null)
                {
                    BattlefrontLogger.Error($"orderWarcampEntrance is null. {(ushort)ActiveBattleFrontStatus.ZoneId} ");
                }

                var destructionWarcampEntrance = BattleFrontService.GetWarcampEntrance(
                    (ushort)ActiveBattleFrontStatus.ZoneId, Realms.REALMS_REALM_DESTRUCTION);

                if (destructionWarcampEntrance == null)
                {
                    BattlefrontLogger.Error($"destructionWarcampEntrance is null. {(ushort)ActiveBattleFrontStatus.ZoneId} ");
                }

                try
                {
                    // TODO : This is a bit of a hack - assumes that if the WC entrances are null, this is a fort.
                    if ((orderWarcampEntrance == null) && (destructionWarcampEntrance == null))
                    {
                        ExecuteBattleFrontLock(Realms.REALMS_REALM_ORDER, null, null, RVRZoneRewardService.RVRRewardFortItems);
                    }
                    else
                    {
                        var tuple = PlaceChestsAtWarcampEntrances(orderWarcampEntrance, destructionWarcampEntrance);
                        ExecuteBattleFrontLock(Realms.REALMS_REALM_ORDER, tuple.Item1, tuple.Item2,
                            RVRZoneRewardService.RVRRewardKeepItems);
                    }
                }
                catch (Exception e)
                {
                    BattlefrontLogger.Error($"Attempt to lock and advance BF failed. {e.Message} {e.StackTrace}");
                    throw;
                }


            }
            else if (VictoryPointProgress.DestructionVictoryPoints >=
                     BattleFrontConstants.LOCK_VICTORY_POINTS)
            {
                var orderWarcampEntrance = BattleFrontService.GetWarcampEntrance(
                    (ushort)ActiveBattleFrontStatus.ZoneId, Realms.REALMS_REALM_ORDER);

                if (orderWarcampEntrance == null)
                {
                    BattlefrontLogger.Error($"orderWarcampEntrance is null. {(ushort)ActiveBattleFrontStatus.ZoneId} ");
                }

                var destructionWarcampEntrance = BattleFrontService.GetWarcampEntrance(
                    (ushort)ActiveBattleFrontStatus.ZoneId, Realms.REALMS_REALM_DESTRUCTION);

                if (destructionWarcampEntrance == null)
                {
                    BattlefrontLogger.Error($"destructionWarcampEntrance is null. {(ushort)ActiveBattleFrontStatus.ZoneId} ");
                }

                try
                {
                    // TODO : This is a bit of a hack - assumes that if the WC entrances are null, this is a fort.
                    if ((orderWarcampEntrance == null) && (destructionWarcampEntrance == null))
                    {
                        ExecuteBattleFrontLock(Realms.REALMS_REALM_DESTRUCTION, null, null, RVRZoneRewardService.RVRRewardFortItems);
                    }
                    else
                    {
                        var tuple = PlaceChestsAtWarcampEntrances(orderWarcampEntrance, destructionWarcampEntrance);
                        ExecuteBattleFrontLock(Realms.REALMS_REALM_DESTRUCTION, tuple.Item1, tuple.Item2, RVRZoneRewardService.RVRRewardKeepItems);
                    }
                }
                catch (Exception e)
                {
                    BattlefrontLogger.Error($"Attempt to lock and advance BF failed. {e.Message} {e.StackTrace}");
                    throw;
                }
            }
        }

        private Tuple<LootChest, LootChest> PlaceChestsAtWarcampEntrances(Point3D orderWarcampEntrance, Point3D destructionWarcampEntrance)
        {
            LootChest orderLootChest = null;
            LootChest destructionLootChest = null; ;

            if (orderWarcampEntrance != null)
            {
                orderLootChest = LootChest.Create(
                    Region,
                    orderWarcampEntrance,
                    (ushort)ActiveBattleFrontStatus.ZoneId);

                orderLootChest.Title = $"Zone Assault {ActiveCampaignName}";
                orderLootChest.Content = $"Zone Assault Rewards";
                orderLootChest.SenderName = $"{ActiveCampaignName}";
            }

            if (destructionWarcampEntrance != null)
            {
                destructionLootChest = LootChest.Create(
                    Region,
                    destructionWarcampEntrance,
                    (ushort)ActiveBattleFrontStatus.ZoneId);

                destructionLootChest.Title = $"Zone Assault {ActiveCampaignName}";
                destructionLootChest.Content = $"Zone Assault Rewards";
                destructionLootChest.SenderName = $"{ActiveCampaignName}";
            }

            return new Tuple<LootChest, LootChest>(orderLootChest, destructionLootChest);
        }

        public void ExecuteBattleFrontLock(Realms lockingRealm, LootChest orderLootChest, LootChest destructionLootChest, List<RVRRewardItem> lootOptions, int forceNumberBags = 0)
        {

            var oldBattleFront = BattleFrontManager.GetActiveBattleFrontFromProgression();
            BattlefrontLogger.Info($"Executing BattleFront Lock on {oldBattleFront.Description} for {lockingRealm}");
            Logger.Info($"***Executing BattleFront Lock on {oldBattleFront.Description} for {lockingRealm}***");
            // Must be called before locking the battlefront
            GenerateZoneLockRewards(lockingRealm, oldBattleFront.ZoneId);
            BattleFrontManager.LockActiveBattleFront(lockingRealm, forceNumberBags);
            // Remove eligible players.
            ClearDictionaries();

            // Select the next Progression
            var nextBattleFront = BattleFrontManager.AdvanceBattleFront(lockingRealm);

            // If the next RVRProgression is the Reset progression, then reset all of the pairings to default.
            if (nextBattleFront.ResetProgressionOnEntry == 1)
            {
                BattlefrontLogger.Info($"ResetProgressionOnEntry is TRUE");
                // Set all regions back to their default owners.
                foreach (var progression in WorldMgr.UpperTierCampaignManager.BattleFrontProgressions)
                {
                    if (progression.Tier == 4)
                    {
                        progression.DestroVP = 0;
                        progression.OrderVP = 0;
                        progression.LastOpenedZone = 0;
                        progression.LastOwningRealm = progression.DefaultRealmLock;

                        if (progression.ResetProgressionOnEntry == 1) // PRAAG
                        {
                            progression.LastOpenedZone = 1;
                            WorldMgr.UpperTierCampaignManager.ActiveBattleFront = progression;
                            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Keeps.SingleOrDefault(x => x.Info.KeepId == progression.OrderKeepId).Realm = Realms.REALMS_REALM_ORDER;
                            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Keeps.SingleOrDefault(x => x.Info.KeepId == progression.OrderKeepId).SetKeepSafe();
                            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Keeps.SingleOrDefault(x => x.Info.KeepId == progression.OrderKeepId).Realm = Realms.REALMS_REALM_DESTRUCTION;
                            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Keeps.SingleOrDefault(x => x.Info.KeepId == progression.DestroKeepId).SetKeepSafe();
                            var objectives = WorldMgr.UpperTierCampaignManager.GetActiveCampaign().Objectives
                                .Where(x => x.ZoneId == progression.ZoneId);
                            foreach (var battlefieldObjective in objectives)
                            {
                                battlefieldObjective.SetObjectiveSafe();
                            }
                        }

                        var status = WorldMgr.UpperTierCampaignManager.GetBattleFrontStatusList().SingleOrDefault(x => x.BattleFrontId == progression.BattleFrontId);
                        if (status != null)
                        {
                            status.Locked = true;
                            status.OpenTimeStamp = FrameWork.TCPManager.GetTimeStamp();
                            status.LockingRealm = (Realms)progression.DefaultRealmLock;
                            status.FinalVictoryPoint = new VictoryPointProgress();
                            status.LockTimeStamp = 0;
                            // Reset the population for the battle front status
                            WorldMgr.UpperTierCampaignManager.GetActiveCampaign().InitializePopulationList(status.BattleFrontId);
                        }
                    }
                }

            }
            // Set the RVR Progression table values.
            BattleFrontManager.UpdateRVRPRogression(lockingRealm, oldBattleFront, nextBattleFront);
            // Tell the players
            SendCampaignMovementMessage(nextBattleFront);
            // Unlock the next Progression
            BattleFrontManager.OpenActiveBattlefront();

            // This is kind of nasty, should use an event to signal the WorldMgr
            // Tell the server that the RVR status has changed.
            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);
            // Logs the status of all battlefronts known to the Battlefront Manager.
            // BattleFrontManager.AuditBattleFronts(this.Tier);        
        }

        private void SendCampaignMovementMessage(RVRProgression nextBattleFront)
        {
            var campaignMoveMessage = $"The campaign has moved to {nextBattleFront.Description}";
            BattlefrontLogger.Info(campaignMoveMessage);
            CommunicationsEngine.Broadcast(campaignMoveMessage, Tier);
        }


        public int GetZoneOwnership(ushort zoneId)
        {
            BattlefrontLogger.Trace($"GetZoneOwnership {zoneId}");
            const int ZONE_STATUS_CONTESTED = 0;
            const int ZONE_STATUS_ORDER_LOCKED = 1;
            const int ZONE_STATUS_DESTRO_LOCKED = 2;


            byte orderKeepsOwned = 0;
            byte destroKeepsOwned = 0;

            if (orderKeepsOwned == 2)
            {
                return ZONE_STATUS_ORDER_LOCKED;
            }
            if (destroKeepsOwned == 2)
            {
                return ZONE_STATUS_DESTRO_LOCKED;
            }
            return ZONE_STATUS_CONTESTED;
        }

        public void WriteBattleFrontStatus(PacketOut Out)
        {
            BattlefrontLogger.Trace(".");
            //Out.WriteByte((byte)GetZoneOwnership(Zones[2].ZoneId));
            //Out.WriteByte((byte)GetZoneOwnership(Zones[1].ZoneId));
            //Out.WriteByte((byte)GetZoneOwnership(Zones[0].ZoneId));
        }


        public bool PreventKillReward()
        {
            return BattleFrontManager.IsBattleFrontLocked(BattleFrontManager.ActiveBattleFront.BattleFrontId); // Removed from legacy : && Tier > 1
        }

        public float GetArtilleryDamageScale(Realms weaponRealm)
        {
            return 1f;
        }




    }
}
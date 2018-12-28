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
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.BattleFronts.Keeps;
using WorldServer.World.BattleFronts.Objectives;
using PlayerContribution = WorldServer.World.Battlefronts.Bounty.PlayerContribution;


namespace WorldServer.World.Battlefronts.Apocalypse
{
    public enum CampaignRerollMode
    {
        NONE = 0,
        INIT = 1,
        REROLL = 2
    }

    /// <summary>
    /// Represents an open RVR front in a given Region (1 Region -> n Zones) Eg Region 14 (T2 Emp -> Zone 101 Troll Country & Zone 107 Ostland
    /// </summary>
    public class Campaign
    {
        public static int REALM_CAPTAIN_TELL_CHANCE = 10;
        public static IObjectDatabase Database = null;
        public static int REALM_CAPTAIN_MINIMUM_CONTRIBUTION = 50;
        static readonly object LockObject = new object();

        private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");

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

        public HashSet<Player> PlayersInLakeSet;
        public List<CampaignObjective> Objectives;

        public ConcurrentDictionary<int, int> OrderPlayerPopulationList = new ConcurrentDictionary<int, int>();
        public ConcurrentDictionary<int, int> DestructionPlayerPopulationList = new ConcurrentDictionary<int, int>();

        //public bool BattleFrontLocked => LockingRealm != Realms.REALMS_REALM_NEUTRAL;
        //public Realms LockingRealm { get; set; } = Realms.REALMS_REALM_NEUTRAL;
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

        public int DestructionDominationTimer { get; set; }
        public int OrderDominationTimer { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="regionMgr"></param>
        /// <param name="objectives"></param>
        /// <param name="players"></param>
        public Campaign(RegionMgr regionMgr,
            List<CampaignObjective> objectives,
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

            DestructionDominationTimer = 5 * 60;
            OrderDominationTimer = 5 * 60;


            _EvtInterface.AddEvent(UpdateBattleFrontScalers, 12000, 0); // 120000
            _EvtInterface.AddEvent(UpdateVictoryPoints, 6000, 0);

            _EvtInterface.AddEvent(UpdateBOs, 5000, 0);
            // Tell each player the RVR status
            _EvtInterface.AddEvent(UpdateRVRStatus, 60000, 0);
            // Recalculate AAO
            _EvtInterface.AddEvent(UpdateAAOBuffs, 30000, 0);
            _EvtInterface.AddEvent(DetermineCaptains, 60000, 0);
            // Check RVR zone for highest contributors (Captains)
            // record metrics
            _EvtInterface.AddEvent(SavePlayerContribution, 60000, 0);
            _EvtInterface.AddEvent(RecordMetrics, 60000, 0);
            _EvtInterface.AddEvent(DestructionDominationCheck, 60000, 0);

        }

        private void DestructionDominationCheck()
        {
            if (this.BattleFrontManager.GetActiveCampaign().VictoryPointProgress.GetDominationCount(
                Realms.REALMS_REALM_DESTRUCTION) == 8)
            {
                _EvtInterface.AddEvent(DestructionDominationZoneLock, DestructionDominationTimer, 0);
            }
            else
            {
                _EvtInterface.RemoveEvent(DestructionDominationZoneLock);
            }

        }

        /// <summary>
        /// Lock this zone as a Domination Lock.
        /// </summary>
        private void DestructionDominationZoneLock()
        {
            BattlefrontLogger.Info($"Destruction Domination Victory!");
            VictoryPointProgress.DestructionVictoryPoints = BattleFrontConstants.LOCK_VICTORY_POINTS;

            // Remove the timer
            _EvtInterface.RemoveEvent(DestructionDominationZoneLock);

        }

        /// <summary>
        /// Lock this zone as a Domination Lock.
        /// </summary>
        private void OrderDominationZoneLock()
        {
            BattlefrontLogger.Info($"Order Domination Victory!");
            VictoryPointProgress.OrderVictoryPoints = BattleFrontConstants.LOCK_VICTORY_POINTS;

            // Remove the timer
            _EvtInterface.RemoveEvent(OrderDominationZoneLock);
        }

        private void OrderDominationCheck()
        {
            if (this.BattleFrontManager.GetActiveCampaign().VictoryPointProgress.GetDominationCount(
                    Realms.REALMS_REALM_ORDER) == 8)
            {
                _EvtInterface.AddEvent(OrderDominationZoneLock, OrderDominationTimer, 0);
            }
            else
            {
                _EvtInterface.RemoveEvent(OrderDominationZoneLock);
            }
        }

        private void DetermineCaptains()
        {
            var status = BattleFrontManager.GetActiveCampaign().ActiveBattleFrontStatus;
            lock (status)
            {
                BattlefrontLogger.Trace($"Checking for new Realm Captains...");
                if (status.RegionId == Region.RegionId)
                {
                    status.RemoveAsRealmCaptain(status.DestructionRealmCaptain);
                    status.RemoveAsRealmCaptain(status.OrderRealmCaptain);

                    var realmCaptains = ActiveBattleFrontStatus.ContributionManagerInstance.GetHigestContributors(
                        REALM_CAPTAIN_MINIMUM_CONTRIBUTION,
                        Player._Players.Where(x => !x.IsDisposed
                        && x.IsInWorld()
                        && x.CbtInterface.IsPvp
                        && x.ScnInterface.Scenario == null
                        && x.Region.RegionId == status.RegionId));

                    var playersToAnnounceTo = Player._Players.Where(x => !x.IsDisposed
                                                                         && x.IsInWorld()
                                                                         && x.CbtInterface.IsPvp
                                                                         && x.ScnInterface.Scenario == null
                                                                         && x.Region.RegionId == status.RegionId);

                    // Destruction
                    if (realmCaptains[0] != null)
                    {
                        status.SetAsRealmCaptain(realmCaptains[0]);
                        if (StaticRandom.Instance.Next(100) < REALM_CAPTAIN_TELL_CHANCE)
                        {
                            foreach (var player in playersToAnnounceTo.Where(x => x.Realm == Realms.REALMS_REALM_ORDER))
                            {

                                player.SendMessage(
                                    $"A captain has emerged from the ranks of the enemy. Take the head of {realmCaptains[0].Name}!",
                                    ChatLogFilters.CHATLOGFILTERS_RVR_KILLS_DESTRUCTION);
                            }
                        }
                    }
                    // Order
                    if (realmCaptains[1] != null)
                    {
                        status.SetAsRealmCaptain(realmCaptains[1]);
                        if (StaticRandom.Instance.Next(100) < REALM_CAPTAIN_TELL_CHANCE)
                        {
                            foreach (var player in playersToAnnounceTo.Where(x => x.Realm == Realms.REALMS_REALM_ORDER))
                            {
                                player.SendMessage(
                                    $"A captain has emerged from the ranks of the enemy. Take the head of {realmCaptains[1].Name}!",
                                    ChatLogFilters.CHATLOGFILTERS_RVR_KILLS_ORDER);
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
                BattlefrontLogger.Warn($"Exception : {e.Message} {e.StackTrace}");
                throw;
            }
            ;

        }


        private void RecordMetrics()
        {

            try
            {
                var groupId = Guid.NewGuid().ToString();

                BattlefrontLogger.Trace($"There are {BattleFrontManager.GetBattleFrontStatusList().Count} battlefront statuses ({BattleFrontManager.GetType().ToString()}).");
                foreach (var status in BattleFrontManager.GetBattleFrontStatusList())
                {
                    lock (status)
                    {
                        if (status.RegionId == Region.RegionId)
                        {
                            BattlefrontLogger.Trace($"Recording metrics for BF Status : ({status.BattleFrontId}) {status.Description}");
                            if (!status.Locked)
                            {
                                var metrics = new RVRMetrics
                                {
                                    BattlefrontId = status.BattleFrontId,
                                    BattlefrontName = status.Description,
                                    DestructionVictoryPoints = (int)BattleFrontManager.ActiveBattleFront.DestroVP,
                                    OrderVictoryPoints = (int)BattleFrontManager.ActiveBattleFront.OrderVP,
                                    Locked = status.LockStatus,
                                    OrderPlayersInLake = GetTotalOrderPVPPlayerCountInZone(BattleFrontManager.ActiveBattleFront.ZoneId),
                                    DestructionPlayersInLake = GetTotalDestPVPPlayerCountInZone(BattleFrontManager.ActiveBattleFront.ZoneId),
                                    Tier = Tier,
                                    Timestamp = DateTime.UtcNow,
                                    GroupId = groupId,
                                    TotalPlayerCountInRegion = GetTotalPVPPlayerCountInRegion(status.RegionId),
                                    TotalDestPlayerCountInRegion = GetTotalDestPVPPlayerCountInRegion(status.RegionId),
                                    TotalOrderPlayerCountInRegion = GetTotalOrderPVPPlayerCountInRegion(status.RegionId),
                                    TotalPlayerCount = Player._Players.Count(x => !x.IsDisposed && x.IsInWorld() && x != null),
                                    TotalFlaggedPlayerCount = Player._Players.Count(x => !x.IsDisposed && x.IsInWorld() && x != null && x.CbtInterface.IsPvp)
                                };
                                WorldMgr.Database.AddObject(metrics);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BattlefrontLogger.Error($"Could not write rvr metrics..continuing. {e.Message} {e.StackTrace}");
            }
        }

        private int GetTotalPVPPlayerCountInRegion(int regionId)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionId && x.CbtInterface.IsPvp);
            }
        }

        private int GetTotalDestPVPPlayerCountInRegion(int regionId)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION && !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionId && x.CbtInterface.IsPvp);
            }
        }

        private int GetTotalOrderPVPPlayerCountInRegion(int regionId)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_ORDER && !x.IsDisposed && x.IsInWorld() && x != null && x.Region.RegionId == regionId && x.CbtInterface.IsPvp);
            }
        }

        private int GetTotalDestPVPPlayerCountInZone(int zoneID)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION && !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneID && x.CbtInterface.IsPvp);
            }
        }

        private int GetTotalOrderPVPPlayerCountInZone(int zoneID)
        {
            lock (Player._Players)
            {
                return Player._Players.Count(x => x.Realm == Realms.REALMS_REALM_ORDER && !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneID && x.CbtInterface.IsPvp);
            }
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
            var orderPlayersInZone = GetOrderPlayersInZone(BattleFrontManager.ActiveBattleFront.ZoneId);
            var destPlayersInZone = GetDestPlayersInZone(BattleFrontManager.ActiveBattleFront.ZoneId);

            var allPlayersInZone = new List<Player>();
            allPlayersInZone.AddRange(destPlayersInZone);
            allPlayersInZone.AddRange(orderPlayersInZone);

            BattlefrontLogger.Trace(
                $"Calculating AAO. Order players : {orderPlayersInZone.Count} Dest players : {destPlayersInZone.Count}");

            AgainstAllOddsTracker.RecalculateAAO(allPlayersInZone, orderPlayersInZone.Count, destPlayersInZone.Count);

            foreach (var keep in Keeps)
            {
                keep.UpdateCurrentAAO(AgainstAllOddsTracker.AgainstAllOddsMult);
            }

        }


        /// <summary>
        /// Buffs all lords in a region depending on VP
        /// 0VP = 200%
        /// 2500VP = 100%
        /// 4000VP = 0%(Regular Health)
        /// </summary>
        /// <returns></returns>
        public void UpdateLordsFromVP()
        {
            var oVp = Region.Campaign.VictoryPointProgress.OrderVictoryPoints;
            var dVp = Region.Campaign.VictoryPointProgress.DestructionVictoryPoints;

            //get order/destro keeps
            var oKeep = Region.Campaign.Keeps.FirstOrDefault(x => x.Realm == Realms.REALMS_REALM_ORDER);
            var dKeep = Region.Campaign.Keeps.FirstOrDefault(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION);

            //update order
            //oKeep?.ScaleLordVP((int)oVp);
            //dKeep?.ScaleLordVP((int)dVp);

        }

        /// <summary>
        /// Buffs all keep doors in a region depending on VP
        /// 0VP = 200%
        /// 2500VP = 100%
        /// 4000VP = 0%(Regular Health)
        /// </summary>
        /// <returns></returns>
        //public void UpdateDoorsFromVP()
        //{
        //    //get order/destro vp's
        //    var oVp = (int) this.Region.Campaign.VictoryPointProgress.OrderVictoryPoints;
        //    var dVp = (int) this.Region.Campaign.VictoryPointProgress.DestructionVictoryPoints;

        //    //get order/destro keeps
        //    var oKeep = this.Region.Campaign.Keeps.FirstOrDefault(x => x.Realm == Realms.REALMS_REALM_ORDER);
        //    var dKeep = this.Region.Campaign.Keeps.FirstOrDefault(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION);

        //    if (oKeep != null)
        //    {
        //        //update keep door health
        //        foreach (var door in oKeep.Doors)
        //        {
        //            if (!door.GameObject.IsDead)
        //            {
        //            }
        //        }
        //    }

        //    if (dKeep != null)
        //    {
        //        //update keep door health
        //        foreach (var door in dKeep.Doors)
        //        {
        //            if (!door.GameObject.IsDead)
        //            {
        //                door.GameObject.SetDoorHealthFromVP((int)oVp);
        //            }
        //        }
        //    }
        //}

        private List<Player> GetOrderPlayersInZone(int zoneId)
        {
            lock (Player._Players)
            {
                return Player._Players.Where(x => x.Realm == Realms.REALMS_REALM_ORDER && !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneId && x.CbtInterface.IsPvp).ToList();
            }
        }

        private List<Player> GetDestPlayersInZone(int zoneId)
        {
            lock (Player._Players)
            {
                return Player._Players.Where(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION && !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneId && x.CbtInterface.IsPvp).ToList();
            }
        }

        private List<Player> GetAllFlaggedPlayersInZone(int zoneId)
        {
            lock (Player._Players)
            {
                return Player._Players.Where(x => !x.IsDisposed && x.IsInWorld() && !x.IsAFK && !x.IsAutoAFK && x != null && x.ZoneId == zoneId && x.CbtInterface.IsPvp).ToList();
            }
        }

        private void SavePlayerContribution()
        {
            lock (ActiveBattleFrontStatus.ContributionManagerInstance)
            {
                if (ActiveBattleFrontStatus.RegionId == Region.RegionId)
                    ActiveBattleFrontStatus.SavePlayerContribution(ActiveBattleFrontStatus.BattleFrontId);
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

            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);

            // also save into db
            RVRProgressionService.SaveRVRProgression(WorldMgr.LowerTierCampaignManager.BattleFrontProgressions);
            RVRProgressionService.SaveRVRProgression(WorldMgr.UpperTierCampaignManager.BattleFrontProgressions);

        }

        /// <summary>
        /// Loads Campaign objectives.
        /// </summary>

        private void LoadObjectives()
        {
            List<BattleFront_Objective> objectives = BattleFrontService.GetBattleFrontObjectives(Region.RegionId);

            if (objectives == null)
                return; // t1 or database lack

            foreach (BattleFront_Objective obj in objectives)
            {
                CampaignObjective flag = new CampaignObjective(Region, obj);
                Objectives.Add(flag);
                Region.AddObject(flag, obj.ZoneId);
                flag.BattleFront = this;

                //orderDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_ORDER);
                //destroDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_DESTRUCTION);
            }
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
                BattleFrontKeep keep = new BattleFrontKeep(info, (byte)Tier, Region, new KeepCommunications());
                keep.Realm = (Realms)keep.Info.Realm;

                Keeps.Add(keep);

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

        public BattleFrontKeep GetClosestKeep(Point3D destPos)
        {
            BattleFrontKeep bestKeep = null;
            ulong bestDist = 0;

            foreach (var keep in Keeps)
            {
                ulong curDist = keep.GetDistanceSquare(destPos);

                if (bestKeep == null || curDist < bestDist)
                {
                    bestKeep = keep;
                    bestDist = keep.GetDistanceSquare(destPos);
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

        public BattleFrontKeep GetZoneKeep(ushort zoneId, int realm)
        {
            foreach (var keep in Keeps)
                if (keep.Info.KeepId == realm)
                    return keep;
            return null;
        }

        public void WriteCaptureStatus(PacketOut Out)
        {
            // Not implemented.
            BattlefrontLogger.Trace(".");
        }

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
                    orderPercent = 100;
                    destroPercent = 0;
                    break;

                case Realms.REALMS_REALM_DESTRUCTION:
                    orderPercent = 0;
                    destroPercent = 100;
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

        #region Reward Splitting

        /// <summary>
        /// A scaler for the reward of objectives captured in this Campaign, based on its activity relative to other fronts of the same tier.
        /// </summary>
        public float RelativeActivityFactor { get; private set; } = 1f;

        /// <summary>
        /// 100 players max for consideration. Push 30% of reward per hour spent in zone = 0.5% per minute shift max.
        /// </summary>
        public void UpdateBattleFrontScalers()
        {

            // 12.05.18 RA - Not this function currently does nothing.
            BattlefrontLogger.Trace($"Updating Campaign scaler : {BattleFrontManager.ActiveBattleFrontName}");
            // Update comparative gain
            int index = Tier - 1;

            if (index < 0 || index > 3)
            {
                Log.Error("Campaign", "Region " + Region.RegionId + " has Campaign with tier index " + index);
                return;
            }

            // RA - TODO 12/05/18
            //foreach (Player player in Region.Players)
            //    WorldMgr.SendCampaignStatus(player);
        }

        #endregion Reward Splitting

        public void Update(long tick)
        {
            _EvtInterface.Update(tick);
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
                    catch { }
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
                    flag.LockObjective(realm, true);
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
                    flag.LockObjective(realm, true);
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

            BattlefrontLogger.Info($"*************************BATTLEFRONT GENERATING REWARDS***********");
            GenerateZoneLockRewards(lockingRealm, forceNumberBags);
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
                    Description = $"Locking Battlefront {this.ActiveCampaignName} to {lockingRealm.ToString()}...",
                    DestroVP = (int)ActiveBattleFrontStatus.DestructionVictoryPointPercentage,
                    OrderVP = (int)ActiveBattleFrontStatus.OrderVictoryPointPercentage,
                    LockingRealm = (int)lockingRealm,
                    RegionId = ActiveBattleFrontStatus.RegionId,
                    Timestamp = DateTime.Now,
                    TotalPlayersAtLock = GetAllFlaggedPlayersInZone(BattleFrontManager.ActiveBattleFront.ZoneId).Count(),
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
        /// <param name="forceNumberBags">By default 0 allows the system to decide the number of bags, setting to -1 forces no rewards.</param>
        private void GenerateZoneLockRewards(Realms lockingRealm, int forceNumberBags = 0)
        {
            var winningRealmPlayers = new ConcurrentDictionary<Player, int>();
            var losingRealmPlayers = new ConcurrentDictionary<Player, int>();
            var allEligiblePlayerDictionary = new ConcurrentDictionary<Player, int>();

            // Calculate no rewards
            if (forceNumberBags == -1)
                return;

            var rewardAssigner = new RewardAssigner(StaticRandom.Instance);
            try
            {

                // Get all players with at least some contribution.
                var allContributingPlayers = ActiveBattleFrontStatus.ContributionManagerInstance.GetEligiblePlayers(0);

                BattlefrontLogger.Debug($"forceNumberBags = {forceNumberBags}");

                // Determine the number of Bags to be handed out.
                var numberOfBags = forceNumberBags;
                if (forceNumberBags == 0)
                    numberOfBags = (int)rewardAssigner.DetermineNumberOfAwards(allContributingPlayers.Count());

                BattlefrontLogger.Debug($"AllContributing Players Count = {allContributingPlayers.Count()}, numberBags = {numberOfBags}");

                // Partition the players by winning realm. 
                foreach (var contributingPlayer in allContributingPlayers)
                {
                    var player = Player.GetPlayer(contributingPlayer.Key);
                    if (player != null)
                    {
                        // Update the Honor Points of the Contributing Players
                        player.Info.HonorPoints += (ushort)contributingPlayer.Value;
                        CharMgr.Database.SaveObject(player.Info);

                        if (player.Realm == lockingRealm)
                            winningRealmPlayers.TryAdd(player, contributingPlayer.Value);
                        else
                        {
                            losingRealmPlayers.TryAdd(player, contributingPlayer.Value);
                        }
                        allEligiblePlayerDictionary.TryAdd(player, contributingPlayer.Value);

                        // Get the contribution list for this player
                        //var playerContributionList = ActiveBattleFrontStatus.ContributionManagerInstance.GetContribution(contributingPlayer.Key);
                        var contributionDictionary = ActiveBattleFrontStatus.ContributionManagerInstance.GetContributionStageDictionary(contributingPlayer.Key);
                        // Record the contribution types and values for the player for analytics
                        RecordContributionAnalytics(player, contributionDictionary);

                    }
                }
                BattlefrontLogger.Debug($"winningRealmPlayers Players Count = {winningRealmPlayers.Count()}");
                BattlefrontLogger.Debug($"losingRealmPlayers Players Count = {losingRealmPlayers.Count()}");

                // Distribute RR, INF, etc to contributing players
                DistributeBaseRewards(losingRealmPlayers, winningRealmPlayers, lockingRealm, ContributionManager.MAXIMUM_CONTRIBUTION);
                // Select the highest contribution players for bag assignment - those eligible (either realm). These are sorted in eligibility order.
                var eligiblePlayers = ActiveBattleFrontStatus.ContributionManagerInstance.GetEligiblePlayers(numberOfBags);
                BattlefrontLogger.Debug($"Eligible Player Count = {eligiblePlayers.Count()} for maximum {numberOfBags} Bags");
                // Get the character Ids of the eligible characters
                var eligiblePlayerCharacterIds = eligiblePlayers.Select(x => x.Key).ToList();
                // Determine and build out the bag types to be assigned
                var bagDefinitions = rewardAssigner.DetermineBagTypes(numberOfBags);
                // Assign eligible players to the bag definitions.
                var rewardAssignments = rewardAssigner.AssignLootToPlayers(eligiblePlayerCharacterIds, bagDefinitions);

                if (rewardAssignments == null)
                {
                    BattlefrontLogger.Warn($"No reward assignments found (null).");
                    return;
                }

                if (rewardAssignments.Count == 0)
                {
                    BattlefrontLogger.Warn($"No reward assignments found.");
                }
                else
                {
                    var bagContentSelector = new BagContentSelector(RVRZoneRewardService.RVRZoneLockItemOptions, StaticRandom.Instance);

                    foreach (var reward in rewardAssignments)
                    {
                        if (reward.Assignee != 0)
                        {
                            BattlefrontLogger.Debug($"Assigning reward to {reward.Assignee} {reward.BagRarity}");
                            KeyValuePair<Player, int> player;
                            try
                            {
                                player = allEligiblePlayerDictionary.Single(x => x.Key.CharacterId == reward.Assignee);
                            }
                            catch (Exception e)
                            {
                                BattlefrontLogger.Warn($"Could not locate player {reward.Assignee} {e.Message} {e.StackTrace}");
                                continue;
                            }


                            var playerItemList = (from item in player.Key.ItmInterface.Items where item != null select item.Info.Entry).ToList();
                            var playerRenown = player.Key.CurrentRenown.Level;
                            var playerClass = player.Key.Info.CareerLine;
                            var playerRenownBand = _rewardManager.CalculateRenownBand(playerRenown);

                            var lootDefinition = bagContentSelector.SelectBagContentForPlayer(reward, playerRenownBand, playerClass, playerItemList.ToList(), true);
                            BattlefrontLogger.Debug($"Award to be handed out : {lootDefinition.ToString()}");
                            if (lootDefinition.IsValid())
                            {
                                BattlefrontLogger.Debug($"{player.Key.Info.Name} has received {lootDefinition.FormattedString()}");
                                BattlefrontLogger.Debug($"{lootDefinition.ToString()}");
                                // Only distribute if loot is valid
                                var rewardDescription = WorldMgr.RewardDistributor.DistributeWinningRealm(lootDefinition, player.Key, playerRenownBand);
                                player.Key.SendClientMessage($"{rewardDescription}", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

                            }
                            else
                            {
                                BattlefrontLogger.Debug($"{player.Key.Info.Name} has received [INVALID for Player] {lootDefinition.FormattedString()}");
                            }

                        }
                    }
                }
                // Remove eligible players.
                ClearDictionaries();

            }
            catch (Exception e)
            {
                BattlefrontLogger.Error($" GenerateZoneLockRewards : {e.Message} {e.StackTrace}");
                throw;
            }

        }

        private void RecordContributionAnalytics(Player player, ConcurrentDictionary<short, ContributionStage> playerContributionList)
        {

            var analyticsRecord = new ContributionAnalytics
            {
                Timestamp = DateTime.Now,
                CharacterId = player.CharacterId,
                RenownRank = player.RenownRank,
                Name = player.Name,
                Class = player.Info.CareerLine,
                AccountId = player.Info.AccountId
            };

            WorldMgr.Database.AddObject(analyticsRecord);

            foreach (var playerContribution in playerContributionList)
            {
                var contributionDetails = new ContributionAnalyticsDetails
                {
                    CharacterId = player.CharacterId,
                    Timestamp = DateTime.Now,
                    ContributionId = playerContribution.Key,
                    ContributionSum = playerContribution.Value.ContributionStageSum
                };

                WorldMgr.Database.AddObject(contributionDetails);
            }
        }

        /// <summary>
        /// Distribute RR XP INF rewards to players that have some contribution
        /// </summary>
        /// <param name="eligibleLosingRealmPlayers">non-zero contribution losing realm players</param>
        /// <param name="eligibleWinningRealmPlayers">non-zero contribution winning realm playes</param>
        /// <param name="lockingRealm"></param>
        /// <param name="baselineContribution">The baseline expected of an 'average' player. If player is below this amount, lower reward, above, increase.</param>
        private void DistributeBaseRewards(ConcurrentDictionary<Player, int> eligibleLosingRealmPlayers, ConcurrentDictionary<Player, int> eligibleWinningRealmPlayers, Realms lockingRealm, int baselineContribution)
        {
            // Ensure that tier 1 gets half rewards.
            var tierRewardScale = Tier == 1 ? 0.5f : 1f;

            // Distribute rewards to losing players with eligibility - halve rewards.
            foreach (var losingRealmPlayer in eligibleLosingRealmPlayers)
            {
                // Scale of player contribution against the highest contributor
                double contributionScale = CalculateContributonScale(losingRealmPlayer.Value, baselineContribution);
                WorldMgr.RewardDistributor.DistributeNonBagAwards(
                    losingRealmPlayer.Key,
                    _rewardManager.CalculateRenownBand(losingRealmPlayer.Key.RenownRank),
                    (1f + contributionScale) * tierRewardScale);
            }

            // Distribute rewards to winning players with eligibility - full rewards.
            foreach (var winningRealmPlayer in eligibleWinningRealmPlayers)
            {
                double contributionScale = CalculateContributonScale(winningRealmPlayer.Value, baselineContribution);
                WorldMgr.RewardDistributor.DistributeNonBagAwards(
                    winningRealmPlayer.Key,
                    _rewardManager.CalculateRenownBand(winningRealmPlayer.Key.RenownRank),
                    (1.5f + contributionScale) * tierRewardScale);
            }

            // Get All players in the zone and if they are not in the eligible list, they receive minor awards
            var allPlayersInZone = GetAllFlaggedPlayersInZone(BattleFrontManager.ActiveBattleFront.ZoneId);
            if (allPlayersInZone != null)
            {
                foreach (var player in allPlayersInZone)
                {

                    if (player.Realm == lockingRealm)
                    {
                        // Ensure player is not in the eligible list.
                        if (eligibleWinningRealmPlayers.All(x => x.Key.CharacterId != player.CharacterId))
                        {
                            // Give player no bag, but half rewards
                            WorldMgr.RewardDistributor.DistributeNonBagAwards(
                                player,
                                _rewardManager.CalculateRenownBand(player.RenownRank),
                                0.5 * tierRewardScale);
                        }
                    }
                    else
                    {
                        // Ensure player is not in the eligible list.
                        if (eligibleLosingRealmPlayers.All(x => x.Key.CharacterId != player.CharacterId))
                        {
                            // Give player no bag, but quarter rewards
                            WorldMgr.RewardDistributor.DistributeNonBagAwards(
                                player,
                                _rewardManager.CalculateRenownBand(player.RenownRank),
                                0.25 * tierRewardScale);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Calculate the contribution scale for this player. This is to vary the reward given for individual contribution to the zone lock.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maximumContribution"></param>
        /// <returns></returns>
        private double CalculateContributonScale(int value, int maximumContribution)
        {
            if (maximumContribution == 0)
                return 0;
            return (double)value / (double)maximumContribution;
        }


        public void ClearDictionaries()
        {
            ActiveBattleFrontStatus.ContributionManagerInstance.ContributionDictionary.Clear();
            ActiveBattleFrontStatus.DestructionRealmCaptain = null;
            ActiveBattleFrontStatus.OrderRealmCaptain = null;
            BattleFrontManager.BountyManagerInstance.BountyDictionary.Clear();
            BattlefrontLogger.Debug($"Contribution and Bounty Dictionaries cleared");
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
        private float _relativePopulationFactor;

        /// <summary>
        /// Returns the enemy lockingRealm's population divided by the input lockingRealm's population.
        /// </summary>
        private float GetRelativePopFactor(Realms realm)
        {
            if (_relativePopulationFactor == 0)
                return 0;
            return realm == Realms.REALMS_REALM_DESTRUCTION ? _relativePopulationFactor : 1f / _relativePopulationFactor;
        }

        private readonly List<int>[] _popHistory = { new List<int>(), new List<int>() };

        /// <summary>
        /// Scales battlefield objective rewards by the following factors:
        /// <para>- The internal AAO</para>
        /// <para>- The relative activity in this Campaign compared to others in its tier</para>
        /// <para>- The total number of people fighting</para>
        /// <para>- The capturing lockingRealm's population at this objective.</para>
        /// </summary>
        //public float GetObjectiveRewardScaler(Realms capturingRealm, int playerCount)
        //{
        //    float scaleMult = GetRelativePopFactor(capturingRealm) * PopulationScaleFactor * RelativeActivityFactor;

        //    int maxRewardPlayers = 6;

        //    if (_popHistory[(int)capturingRealm - 1].Count > 0)
        //        maxRewardPlayers = Math.Max(6, _popHistory[(int)capturingRealm - 1].Max() / 5);

        //    if (playerCount > maxRewardPlayers)
        //        scaleMult *= maxRewardPlayers / (float)playerCount;

        //    return scaleMult;
        //}


        /////
        ///// Unlocks this NDBF for capture.
        ///// 
        //public void ResetBattleFront()
        //{
        //    BattlefrontLogger.Trace($"Resetting Battlefront...{this.ActiveCampaignName}");

        //    VictoryPointProgress.Reset(this);
        //    LockingRealm = Realms.REALMS_REALM_NEUTRAL;

        //    foreach (var flag in Objectives)
        //        flag.UnlockObjective();

        //    foreach (Keep keep in Keeps)
        //        keep.NotifyPairingUnlocked();

        //    //UpdateStateOfTheRealm();

        //    // This seems to look at all BattleFronts and report their status, but incorrectly in the new system.
        //    // TODO - fix
        //    // WorldMgr.SendCampaignStatus(null);
        //}



        /// <summary>
        /// Sends information to a player about the objectives within a Campaign upon their entry.
        /// </summary>
        /// <param name="plr"></param>
        public void SendObjectives(Player plr)
        {
            BattlefrontLogger.Trace(".");
            foreach (CampaignObjective bo in Objectives)
                bo.SendState(plr, false);
        }

        private void UpdateBOs()
        {
            // Locked by Order/Dest
            if (IsBattleFrontLocked())
                return; // Nothing to do

            // Only update an active battlefront
            if (BattleFrontManager.ActiveBattleFront.RegionId != Region.RegionId)
                return;

            foreach (CampaignObjective flag in Objectives)
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
            // Locked by Order/Dest
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
                try
                {
                    BattleFrontManager.LockActiveBattleFront(Realms.REALMS_REALM_ORDER, 0);
                    // Select the next Progression
                    var nextBattleFront = BattleFrontManager.AdvanceBattleFront(Realms.REALMS_REALM_ORDER);
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
                catch (Exception e)
                {
                    BattlefrontLogger.Error($"Attempt to lock and advance BF failed. {e.Message} {e.StackTrace}");
                    throw;
                }


            }
            else if (VictoryPointProgress.DestructionVictoryPoints >=
                     BattleFrontConstants.LOCK_VICTORY_POINTS)
            {
                try
                {
                    BattleFrontManager.LockActiveBattleFront(Realms.REALMS_REALM_DESTRUCTION, 0);
                    // Select the next Progression
                    var nextBattleFront = BattleFrontManager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION);
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
                catch (Exception e)
                {
                    BattlefrontLogger.Error($"Attempt to lock and advance BF failed. {e.Message} {e.StackTrace}");
                    throw;
                }
            }
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
            // const int ZONE_STATUS_UNLOCKABLE    = 3;

            byte orderKeepsOwned = 0;
            byte destroKeepsOwned = 0;

            if (orderKeepsOwned == 2 /*&& _held[Zones.FindIndex(z => z.ZoneId == zoneId), 0] == 4*/)
            {
                return ZONE_STATUS_ORDER_LOCKED;
            }
            if (destroKeepsOwned == 2 /*&& _held[Zones.FindIndex(z => z.ZoneId == zoneId), 1] == 4*/)
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




        #region Battlefield Objective Lock Mechanics
        /// <summary>List of players in lake accessible through main update thread without locking</summary>
        private List<Player> _syncPlayersList = new List<Player>();

        private void UpdatePopulationDistribution()
        {
            int orderCount, destroCount;

            _syncPlayersList.Clear();

            lock (_orderInLake)
            {
                _syncPlayersList.AddRange(_orderInLake);
                orderCount = _orderInLake.Count;
                if (_totalMaxOrder < orderCount)
                    _totalMaxOrder = orderCount;
            }

            lock (_destroInLake)
            {
                _syncPlayersList.AddRange(_destroInLake);
                destroCount = _destroInLake.Count;
                if (_totalMaxDestro < destroCount)
                    _totalMaxDestro = destroCount;
            }

            foreach (var obj in Objectives)
            {
                if (obj.State != StateFlags.ZoneLocked)
                {
                    //obj.AdvancePopHistory(orderCount, destroCount);
                    BattlefrontLogger.Debug($"AdvancePopHistory Order={orderCount} DestCount={destroCount}");
                }
            }
        }


        // Higher if enemy lockingRealm's population is lower.
        public float GetLockPopulationScaler(Realms realm)
        {
            if (realm == Realms.REALMS_REALM_NEUTRAL)
                return 1f;

            // Factor for how much this lockingRealm outnumbers the enemy.
            float popFactor = Point2D.Clamp((realm == Realms.REALMS_REALM_ORDER ? _relativePopulationFactor : 1f / _relativePopulationFactor), 0.33f, 3f);

            if (popFactor > 1f)
                return popFactor / ((popFactor + 1f) / 2f);

            return 1f / ((1f / popFactor + 1f) / 2f);
        }

        #endregion


        #region Proximity
        /// <summary>
        /// Periodically checks player positions around flags to debuff
        /// them when they approach warcamp entrances.
        /// </summary>
        private void CheckSpawnFarm()
        {
            foreach (Player player in _syncPlayersList)
            {
                var flag = GetClosestFlag(player.WorldPosition, true);

                //if (flag != null && flag.State != StateFlags.ZoneLocked)
                //flag.AddPlayerInQuadrant(player);

                // Check warcamp farm
                if (player.Zone != null)
                {
                    Realms opposite = player.Realm == Realms.REALMS_REALM_DESTRUCTION ? Realms.REALMS_REALM_ORDER : Realms.REALMS_REALM_DESTRUCTION;
                    Point3D warcampLoc = BattleFrontService.GetWarcampEntrance(player.Zone.ZoneId, opposite);

                    if (warcampLoc != null)
                    {
                        float range = (float)player.GetDistanceTo(warcampLoc);
                        if (range < BattleFrontConstants.WARCAMP_FARM_RANGE)
                            player.WarcampFarmScaler = range / BattleFrontConstants.WARCAMP_FARM_RANGE;
                        else
                            player.WarcampFarmScaler = 1f;
                    }
                }
            }
        }

        public CampaignObjective GetClosestFlag(Point3D destPos, bool inPlay = false)
        {
            BattlefrontLogger.Trace(".");
            CampaignObjective bestFlag = null;
            ulong bestDist = 0;

            foreach (CampaignObjective flag in Objectives)
            {
                ulong curDist = flag.GetDistanceSquare(destPos);

                if (bestFlag == null || (curDist < bestDist && (!inPlay || flag.State != StateFlags.ZoneLocked)))
                {
                    bestFlag = flag;
                    bestDist = flag.GetDistanceSquare(destPos);
                }
            }

            return bestFlag;
        }


        #endregion

        public bool PreventKillReward()
        {
            return BattleFrontManager.IsBattleFrontLocked(BattleFrontManager.ActiveBattleFront.BattleFrontId); // Removed from legacy : && Tier > 1
        }

        /// <summary>
        /// Increases the value of the closest battlefield objective to the kill and determines reward scaling based on proximity to the objective. 
        /// </summary>
        public float ModifyKill(Player killer, Player killed)
        {
            if (killed.WorldPosition == null)
            {
                Log.Error("ModifyKill", "Player died with NULL WorldPosition!");
                return 1f;
            }

            float rewardMod = 1f;
            if (IsBattleFrontLocked())
                return rewardMod;

            var closestFlag = GetClosestFlag(killed.WorldPosition);

            if (closestFlag != null)
            {
                closestFlag.AccumulatedKills++;

                // Defense kill. Weight the kill higher depending on the distance from the opposing objective (proactive defending)
                if (killer.Realm == closestFlag.OwningRealm)
                    rewardMod += Math.Min(killed.GetDistanceTo(closestFlag), 1000) * 0.001f * 0.5f;
                // Attack kill. Weight the kill higher if it was closer to the objective (high penetration)
                else
                    rewardMod += (1000 - Math.Min(killed.GetDistanceTo(closestFlag), 1000)) * 0.001f * 0.5f;

                // calculate ObjectiveReward scale on near players
                var scale = closestFlag.CalculateObjectiveRewardScale(killer);
                BattlefrontLogger.Debug("objective scale = " + scale);
                rewardMod += scale;
            }

            return rewardMod;
        }

        public float GetArtilleryDamageScale(Realms weaponRealm)
        {
            return 1f;
        }
    }

    public class PlayerRewardOptions
    {
        public uint CharacterId { get; set; }
        public Item[] ItemList { get; set; }
        public uint RenownLevel { get; set; }
        public uint RenownBand { get; set; }
        public string CharacterName { get; set; }
        public Realms CharacterRealm { get; set; }
    }
}
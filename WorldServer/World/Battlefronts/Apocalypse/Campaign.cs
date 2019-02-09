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


namespace WorldServer.World.Battlefronts.Apocalypse
{


    /// <summary>
    /// Represents an open RVR front in a given Region (1 Region -> n Zones) Eg Region 14 (T2 Emp -> Zone 101 Troll Country & Zone 107 Ostland
    /// </summary>
    public class Campaign
    {
        public static int REALM_CAPTAIN_TELL_CHANCE = 10;
        public static IObjectDatabase Database = null;
        public static int REALM_CAPTAIN_MINIMUM_CONTRIBUTION = 50;
        public static int DOMINATION_POINTS_REQUIRED = 6;
        static readonly object LockObject = new object();
        public static int SCALE_MODEL_UP = 1;
        public static int SCALE_MODEL_DOWN = 0;

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


        public int OrderDominationTimerEnd { get; set; }
        public int DestructionDominationTimerEnd { get; set; }

        public bool RegionLocked { get; set; }

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
            RegionLocked = false;

            Tier = (byte)Region.GetTier();
            PlaceObjectives();

            LoadKeeps();
            AgainstAllOddsTracker = new AAOTracker();
            _rewardManager = new RVRRewardManager();
            SiegeManager = new SiegeManager();

            DestructionDominationTimerLength = 5 * 60;
            OrderDominationTimerLength = 5 * 60;
            
            _EvtInterface.AddEvent(UpdateVictoryPoints, 6000, 0);

            _EvtInterface.AddEvent(UpdateBOs, 5000, 0);
            // Tell each player the RVR status
            _EvtInterface.AddEvent(UpdateRVRStatus, 120000, 0);
            // Recalculate AAO
            _EvtInterface.AddEvent(UpdateAAOBuffs, 30000, 0);
            _EvtInterface.AddEvent(DetermineCaptains, 120000, 0);
            // Check RVR zone for highest contributors (Captains)
            _EvtInterface.AddEvent(SavePlayerContribution, 60000, 0);
            // record metrics
            _EvtInterface.AddEvent(RecordMetrics, 600000, 0);
            _EvtInterface.AddEvent(DestructionDominationCheck, 60000, 0);
            _EvtInterface.AddEvent(OrderDominationCheck, 60000, 0);
            _EvtInterface.AddEvent(UpdateCampaignObjectiveBuffs, 10000, 0);
            _EvtInterface.AddEvent(CheckKeepTimers, 10000, 0);
            _EvtInterface.AddEvent(UpdateKeepResources, 60000, 0);
            _EvtInterface.AddEvent(RefreshObjectiveStatus, 20000, 0);
            _EvtInterface.AddEvent(CountdownFortDefenceTimer, 9000000, 0);

            //_EvtInterface.AddEvent(UpdateWanderingMobs, 5000, 0);
        }

        /// <summary>
        /// If there is a keep under attack, check it's defence timer count down.
        /// </summary>
        private void CountdownFortDefenceTimer()
        {
            // If its a fort, not locked and the active zone
            var k = Keeps.SingleOrDefault(x => x.IsFortress() && x.ZoneId == ActiveBattleFrontStatus.ZoneId && x.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED);
            k?.CountdownFortDefenceTimer();
        }


        /// <summary>
        /// Inform all players in the active battlefront about the objective status
        /// </summary>
        private void RefreshObjectiveStatus()
        {
            var status = this.ActiveBattleFrontStatus;
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
                        SendObjectives(player, Objectives.Where(x=>x.ZoneId == status.ZoneId));
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
                    if (status.RegionId == Region.RegionId)
                    {
                        foreach (var keep in Keeps)
                        {
                            keep.UpdateResources();
                        }
                    }
                }
            }
        }

        private void DestructionDominationCheck()
        {
            var status = BattleFrontManager.GetActiveCampaign().ActiveBattleFrontStatus;
            // Only worry about the battlefrontstatus in this region.
            if (status.RegionId != Region.RegionId)
                return;

            BattlefrontLogger.Trace
            ($"Destruction Domination Count = " +
             $"{BattleFrontManager.GetActiveCampaign().VictoryPointProgress.GetDominationCount(Realms.REALMS_REALM_DESTRUCTION)}");

            if (BattleFrontManager.GetActiveCampaign().VictoryPointProgress.GetDominationCount(Realms.REALMS_REALM_DESTRUCTION) == DOMINATION_POINTS_REQUIRED)
            {
                if (!_EvtInterface.HasEvent(DestructionDominationZoneLockCheck))
                {

                    lock (status)
                    {
                        var playersToNotify = Player._Players.Where(x => !x.IsDisposed
                                                                         && x.IsInWorld()
                                                                         && x.CbtInterface.IsPvp
                                                                         && x.ScnInterface.Scenario == null
                                                                         && x.Region.RegionId == status.RegionId
                                                                         && x.ZoneId == status.ZoneId);

                        foreach (var player in playersToNotify)
                        {
                            player.SendClientMessage("Destruction is dominating. Zone will lock unless Order intercedes.");
                        }
                    }

                    DestructionDominationTimerEnd = FrameWork.TCPManager.GetTimeStamp() + DestructionDominationTimerLength;

                    _EvtInterface.AddEvent(DestructionDominationZoneLockCheck, 30000, 0);
                    BattlefrontLogger.Info($"Destruction Domination Timer has started");
                }
            }
            else
            {
                if (_EvtInterface.HasEvent(DestructionDominationZoneLockCheck))
                {
                    _EvtInterface.RemoveEvent(DestructionDominationZoneLockCheck);
                    BattlefrontLogger.Info($"Destruction Domination Timer has stopped");
                }
            }

        }
        /// <summary>
        /// Lock this zone as a Domination Lock if timer has expired
        /// </summary>
        private void DestructionDominationZoneLockCheck()
        {
            if (DestructionDominationTimerEnd > TCPManager.GetTimeStamp())
                return;

            BattlefrontLogger.Info($"Destruction Domination Victory!");
            VictoryPointProgress.DestructionVictoryPoints = BattleFrontConstants.LOCK_VICTORY_POINTS;

            // Remove the timer
            _EvtInterface.RemoveEvent(DestructionDominationZoneLockCheck);
        }

        /// <summary>
        /// Lock this zone as a Domination Lock if timer has expired
        /// </summary>
        private void OrderDominationZoneLockCheck()
        {
            if (OrderDominationTimerEnd > TCPManager.GetTimeStamp())
                return;

            BattlefrontLogger.Info($"Order Domination Victory!");
            VictoryPointProgress.OrderVictoryPoints = BattleFrontConstants.LOCK_VICTORY_POINTS;

            // Remove the timer
            _EvtInterface.RemoveEvent(OrderDominationZoneLockCheck);
        }

        private void OrderDominationCheck()
        {
            var status = BattleFrontManager.GetActiveCampaign().ActiveBattleFrontStatus;
            // Only worry about the battlefrontstatus in this region.
            if (status.RegionId != Region.RegionId)
                return;

            BattlefrontLogger.Trace
            ($"Order Domination Count = " +
             $"{BattleFrontManager.GetActiveCampaign().VictoryPointProgress.GetDominationCount(Realms.REALMS_REALM_ORDER)}");

            if (BattleFrontManager.GetActiveCampaign().VictoryPointProgress.GetDominationCount(Realms.REALMS_REALM_ORDER) == DOMINATION_POINTS_REQUIRED)
            {
                if (!_EvtInterface.HasEvent(OrderDominationZoneLockCheck))
                {
                    lock (status)
                    {
                        var playersToNotify = Player._Players.Where(x => !x.IsDisposed
                                                                        && x.IsInWorld()
                                                                        && x.CbtInterface.IsPvp
                                                                        && x.ScnInterface.Scenario == null
                                                                        && x.Region.RegionId == status.RegionId
                                                                        && x.ZoneId == status.ZoneId);

                        foreach (var player in playersToNotify)
                        {
                            player.SendClientMessage("Order is dominating. Zone will lock unless Destruction intercedes.");
                        }
                    }

                    OrderDominationTimerEnd = FrameWork.TCPManager.GetTimeStamp() + OrderDominationTimerLength;

                    _EvtInterface.AddEvent(OrderDominationZoneLockCheck, 30000, 0);
                    BattlefrontLogger.Info($"Order Domination Timer has started");
                }
            }
            else
            {
                if (_EvtInterface.HasEvent(OrderDominationZoneLockCheck))
                {
                    _EvtInterface.RemoveEvent(OrderDominationZoneLockCheck);
                    BattlefrontLogger.Info($"Order Domination Timer has stopped");
                }
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

        private void DetermineCaptains()
        {
            var status = BattleFrontManager.GetActiveCampaign().ActiveBattleFrontStatus;
            lock (status)
            {
               
                BattlefrontLogger.Trace($"Checking for new Realm Captains...");
                if (status.RegionId == Region.RegionId)
                {
                    var zonePlayers = Player._Players.Where(x => !x.IsDisposed
                                                                 && x.IsInWorld()
                                                                 && x.CbtInterface.IsPvp
                                                                 && x.ScnInterface.Scenario == null
                                                                 && x.ZoneId == status.ZoneId).ToList();


                    var realmCaptains = ActiveBattleFrontStatus.ContributionManagerInstance.GetHigestContributors(
                        REALM_CAPTAIN_MINIMUM_CONTRIBUTION, zonePlayers);

                   
                    ScaleModel(status.DestructionRealmCaptain, zonePlayers, SCALE_MODEL_DOWN);
                    ScaleModel(status.OrderRealmCaptain, zonePlayers, SCALE_MODEL_DOWN);

                    status.RemoveAsRealmCaptain(status.DestructionRealmCaptain);
                    status.RemoveAsRealmCaptain(status.OrderRealmCaptain);
                    

                    // Destruction
                    if (realmCaptains[0] != null)
                    {
                        status.SetAsRealmCaptain(realmCaptains[0]);

                        ScaleModel(realmCaptains[0], zonePlayers, SCALE_MODEL_UP);

                        if (StaticRandom.Instance.Next(100) < REALM_CAPTAIN_TELL_CHANCE)
                        {
                            foreach (var player in zonePlayers.Where(x => x.Realm == Realms.REALMS_REALM_ORDER))
                            {
                                player.SendMessage(
                                    $"A captain has emerged from the ranks of the enemy. Take the head of {realmCaptains[0].Name}!",
                                    ChatLogFilters.CHATLOGFILTERS_RVR);
                            }
                        }
                    }
                    // Order
                    if (realmCaptains[1] != null)
                    {
                        status.SetAsRealmCaptain(realmCaptains[1]);
                        ScaleModel(realmCaptains[1], zonePlayers, SCALE_MODEL_UP);
                        if (StaticRandom.Instance.Next(100) < REALM_CAPTAIN_TELL_CHANCE)
                        {
                            foreach (var player in zonePlayers.Where(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION))
                            {
                                player.SendMessage(
                                    $"A captain has emerged from the ranks of the enemy. Take the head of {realmCaptains[1].Name}!",
                                    ChatLogFilters.CHATLOGFILTERS_RVR);
                            }
                        }
                    }

                }
            }
        }

        private void ScaleModel(Player player, List<Player> playersToAnnounce, int upDown)
        {
            if (player == null) return;
            if (playersToAnnounce == null) return;

            if (upDown ==SCALE_MODEL_UP)
                player.EffectStates.Add((byte)ObjectEffectState.OBJECTEFFECTSTATE_SCALE_UP);
            if (upDown == SCALE_MODEL_DOWN)
                player.EffectStates.Remove((byte)ObjectEffectState.OBJECTEFFECTSTATE_SCALE_UP);

            //var Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE);

            //Out.WriteUInt16(player.Oid);
            //Out.WriteByte(1);
            //Out.WriteByte((byte)ObjectEffectState.OBJECTEFFECTSTATE_SCALE_UP);
            //Out.WriteByte((byte)(upDown));
            //Out.WriteByte(0);

            foreach (var announce in playersToAnnounce)
            {
                //announce.DispatchPacket(Out, true);
                player.SendMeTo(announce);
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

        public BattleFrontKeep GetClosestKeep(Point3D destPos, ushort zoneId, KeepStatus excludedKeepStatus=KeepStatus.KEEPSTATUS_RUINED)
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
                    catch(Exception ex)
                    {
                        BattlefrontLogger.Debug($"Error adding {plr.Name} to PopulationList");
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
                    Description = $"Locking Battlefront {ActiveCampaignName} to {lockingRealm.ToString()}...",
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
            SiegeManager.DestroyAllSiege();
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
            // Locked by Order/Dest
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
                    ExecuteBattleFrontLock(Realms.REALMS_REALM_ORDER);
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
                    ExecuteBattleFrontLock(Realms.REALMS_REALM_DESTRUCTION);
                }
                catch (Exception e)
                {
                    BattlefrontLogger.Error($"Attempt to lock and advance BF failed. {e.Message} {e.StackTrace}");
                    throw;
                }
            }
        }

        public void ExecuteBattleFrontLock(Realms lockingRealm)
        {
            BattlefrontLogger.Info($"Executing BattleFront Lock for {lockingRealm}");
            var oldBattleFront = BattleFrontManager.GetActiveBattleFrontFromProgression();

            BattleFrontManager.LockActiveBattleFront(lockingRealm, 0);
            // Select the next Progression
            var nextBattleFront = BattleFrontManager.AdvanceBattleFront(lockingRealm);

            // If the next RVRProgression is the Reset progression, then reset all of the pairings to default.
            if (nextBattleFront.ResetProgressionOnEntry == 1)
            {
                foreach (var progression in BattleFrontManager.BattleFrontProgressions)
                {
                    if (progression.Tier == 4)
                    {
                        progression.DestroVP = 0;
                        progression.OrderVP = 0;
                        progression.LastOpenedZone = 0;
                        progression.LastOwningRealm = progression.DefaultRealmLock;
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
            if (destroKeepsOwned == 2 )
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

        public void StartRegionLock()
        {
            BattlefrontLogger.Info($"Starting Region Lock for Region : {this.Region.RegionId}");
            _EvtInterface.AddEvent(EndRegionLock, 90000, 1);
            RegionLocked = true;
            var playersToAnnounceTo = Player._Players.Where(x => !x.IsDisposed
                                                                 && x.IsInWorld()
                                                                 && x.ScnInterface.Scenario == null
                                                                 && x.Region.RegionId == this.Region.RegionId);

            foreach (var player in playersToAnnounceTo)
            {
                player.SendClientMessage($"{Region.RegionName} is in ruins, but it shall rise again!");
            }
        }

        private void EndRegionLock()
        {
            BattlefrontLogger.Info($"Ending Region Lock for Region : {this.Region.RegionId}");
            _EvtInterface.RemoveEvent(EndRegionLock);
            RegionLocked = false;
            var playersToAnnounceTo = Player._Players.Where(x => !x.IsDisposed
                                                                 && x.IsInWorld()
                                                                 && x.ScnInterface.Scenario == null
                                                                 && x.Region.RegionId == this.Region.RegionId);

            foreach (var player in playersToAnnounceTo)
            {
                player.SendClientMessage($"{Region.RegionName} has recovered and is available for battle!");
            }
        }

       
    }
}
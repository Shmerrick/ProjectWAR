using System;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.BattleFronts;
using WorldServer.World.BattleFronts.Keeps;
using WorldServer.World.BattleFronts.Objectives;
using WorldServer.World.Objects.PublicQuests;


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
        public static int POPULATION_BROADCAST_CHANCE = 90;
        public static IObjectDatabase Database = null;
        static readonly object LockObject = new object();

        private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");
        public VictoryPointProgress VictoryPointProgress { get; set; }

        public RegionMgr Region { get; set; }
        public IBattleFrontManager BattleFrontManager { get; set; }
        public IApocCommunications CommunicationsEngine { get; }

        // List of battlefront statuses for this Campaign
        public List<BattleFrontStatus> ApocBattleFrontStatuses => GetBattleFrontStatuses(this.Region.RegionId);

        /// <summary>
        /// A list of keeps within this Campaign.
        /// </summary>
        public readonly List<Keep> Keeps = new List<Keep>();
        public string CampaignName => this.BattleFrontManager.ActiveBattleFrontName;

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


        public int AgainstAllOddsMult => _aaoTracker.AgainstAllOddsMult;

        #endregion


        private AAOTracker _aaoTracker;
        private ContributionTracker _contributionTracker;
        private RVRRewardManager _rewardManager;

        public string ActiveZoneName { get; }
        public bool DefenderPopTooSmall { get; set; }
        public int Tier { get; set; }

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
            this.Region = regionMgr;
            this.VictoryPointProgress = new VictoryPointProgress();
            this.PlayersInLakeSet = players;
            this.Objectives = objectives;
            this.BattleFrontManager = bfm;
            CommunicationsEngine = communicationsEngine;

            Tier = (byte)Region.GetTier();
            PlaceObjectives();

            LoadKeeps();

            _contributionTracker = new ContributionTracker(Tier, regionMgr);
            _aaoTracker = new AAOTracker();
            _rewardManager = new RVRRewardManager();
            //OrderBattleFrontPlayerDictionary = new Dictionary<Player, int>();
            //DestructionBattleFrontPlayerDictionary = new Dictionary<int, int>();

            _EvtInterface.AddEvent(UpdateBattleFrontScalers, 12000, 0); // 120000
            _EvtInterface.AddEvent(UpdateVictoryPoints, 6000, 0);

            _EvtInterface.AddEvent(UpdateBOs, 5000, 0);
            // Tell each player the RVR status
            _EvtInterface.AddEvent(UpdateRVRStatus, 60000, 0);
            // Recalculate AAO
            _EvtInterface.AddEvent(UpdateAAOBuffs, 60000, 0);
            // Recalculate AAO
            _EvtInterface.AddEvent(RecordMetrics, 30000, 0);

            if (Tier == 4)
            {
                _EvtInterface.AddEvent(UpdateDoorsFromVP, 1, 0);
            }

        }

        public void InitializePopulationList(int battlefrontId)
        {
            if (this.OrderPlayerPopulationList.ContainsKey(battlefrontId))
            {
                this.OrderPlayerPopulationList[battlefrontId] = 0;
            }
            else
            {
                this.OrderPlayerPopulationList.TryAdd(battlefrontId, 0);
            }
            if (this.DestructionPlayerPopulationList.ContainsKey(battlefrontId))
            {
                this.DestructionPlayerPopulationList[battlefrontId] = 0;
            }
            else
            {
                this.DestructionPlayerPopulationList.TryAdd(battlefrontId, 0);
            }
        }

        private void RecordMetrics()
        {

            try
            {
                var groupId = Guid.NewGuid().ToString();
                
                BattlefrontLogger.Info($"There are {BattleFrontManager.GetBattleFrontStatusList().Count} battlefront statuses ({BattleFrontManager.GetType().ToString()}).");
                foreach (var status in BattleFrontManager.GetBattleFrontStatusList())
                {
                    lock (status)
                    {
                        if (status.RegionId == this.Region.RegionId)
                        {
                            BattlefrontLogger.Debug($"Recording metrics for BF Status : ({status.BattleFrontId}) {status.Description}");
                            if (!status.Locked)
                            {
                                var metrics = new RVRMetrics
                                {
                                    BattlefrontId = status.BattleFrontId,
                                    BattlefrontName = status.Description,
                                    DestructionVictoryPoints = (int) this.BattleFrontManager.ActiveBattleFront.DestroVP,
                                    OrderVictoryPoints = (int) this.BattleFrontManager.ActiveBattleFront.OrderVP,
                                    Locked = status.LockStatus,
                                    OrderPlayersInLake = GetTotalOrderPVPPlayerCountInZone(this.BattleFrontManager.ActiveBattleFront.ZoneId),
                                    DestructionPlayersInLake = GetTotalDestPVPPlayerCountInZone(this.BattleFrontManager.ActiveBattleFront.ZoneId),
                                    Tier = this.Tier,
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
            return this.BattleFrontManager.GetBattleFrontStatusList().Where(x => x.RegionId == regionId).ToList();
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
            return $"Victory Points Progress for {this.CampaignName} : {this.VictoryPointProgress.ToString()}";
        }


        private void UpdateAAOBuffs()
        {
            var orderPlayersInZone = GetOrderPlayersInZone(this.BattleFrontManager.ActiveBattleFront.ZoneId);
            var destPlayersInZone = GetDestPlayersInZone(this.BattleFrontManager.ActiveBattleFront.ZoneId);

            var allPlayersInZone = new List<Player>();
            allPlayersInZone.AddRange(destPlayersInZone);
            allPlayersInZone.AddRange(orderPlayersInZone);

            BattlefrontLogger.Debug($"Calculating AAO. Order players : {orderPlayersInZone.Count} Dest players : {destPlayersInZone.Count}");

            foreach (var status in BattleFrontManager.GetBattleFrontStatusList())
            {
                if (!status.Locked)
                {
                    // Randomly let players know the population
                    if (StaticRandom.Instance.Next(100) > POPULATION_BROADCAST_CHANCE)
                    {
                        foreach (var player in allPlayersInZone)
                        {
                            if (player.Realm == Realms.REALMS_REALM_DESTRUCTION)
                            {
                                player.SendMessage($"Messengers report {orderPlayersInZone.Count} Order players in the zone.",
                                    ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
                            }
                            else
                            {
                                player.SendMessage($"Messengers report {destPlayersInZone.Count} Destruction players in the zone.",
                                    ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE);
                            }
                        }
                    }
                }
            }

            _aaoTracker.RecalculateAAO(allPlayersInZone, orderPlayersInZone.Count, destPlayersInZone.Count);
        }

        /// <summary>
        /// Buffs all keep doors in a region depending on VP
        /// 0VP = 200%
        /// 2500VP = 100%
        /// 4000VP = 0%(Regular Health)
        /// </summary>
        /// <returns></returns>
        public void UpdateDoorsFromVP()
        {
            //get order/destro vp's
            var oVp = this.Region.Campaign.VictoryPointProgress.OrderVictoryPoints;
            var dVp = this.Region.Campaign.VictoryPointProgress.DestructionVictoryPoints;

            //get order/destro keeps
            var oKeep = this.Region.Campaign.Keeps.FirstOrDefault(x => x.Realm == Realms.REALMS_REALM_ORDER);
            var dKeep = this.Region.Campaign.Keeps.FirstOrDefault(x => x.Realm == Realms.REALMS_REALM_DESTRUCTION);

            if (oKeep != null)
            {
                //update keep door health
                foreach (var door in oKeep.Doors)
                {
                    if (!door.GameObject.IsDead)
                    {
                        var tH = door.GameObject.TotalHealth;
                        uint tHBuffed = 0;
                        bool is100wobuff = false;
                        switch (oVp)
                        {
                            case 0:

                                //200% buff(totalhealth * 3)
                                tHBuffed = (tH * 3);
                                is100wobuff = (door.GameObject.Health == tH) ? true : false;
                                if (is100wobuff)
                                {
                                    door.GameObject.Health = tHBuffed;
                                }

                                break;

                            case 2500:
                                tHBuffed = (tH * 2);
                                is100wobuff = (door.GameObject.Health == tH * 3 || door.GameObject.Health > tHBuffed)
                                    ? true
                                    : false;
                                if (is100wobuff)
                                {
                                    door.GameObject.Health = tHBuffed;
                                }

                                break;

                            case 4000:
                                tHBuffed = (tH);
                                is100wobuff = (door.GameObject.Health == tH * 2 || door.GameObject.Health > tHBuffed)
                                    ? true
                                    : false;
                                if (is100wobuff)
                                {
                                    door.GameObject.Health = tHBuffed;
                                }

                                break;
                        }
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
                        var tH = door.GameObject.TotalHealth;
                        uint tHBuffed = 0;
                        bool is100wobuff = false;
                        switch (oVp)
                        {
                            case 0:

                                //200% buff(totalhealth * 3)
                                tHBuffed = (tH * 3);
                                is100wobuff = (door.GameObject.Health == tH) ? true : false;
                                if (is100wobuff)
                                {
                                    door.GameObject.Health = tHBuffed;
                                }

                                break;

                            case 2500:
                                tHBuffed = (tH * 2);
                                is100wobuff = (door.GameObject.Health == tH * 3 || door.GameObject.Health > tHBuffed)
                                    ? true
                                    : false;
                                if (is100wobuff)
                                {
                                    door.GameObject.Health = tHBuffed;
                                }

                                break;

                            case 4000:
                                tHBuffed = (tH);
                                is100wobuff = (door.GameObject.Health == tH * 2 || door.GameObject.Health > tHBuffed)
                                    ? true
                                    : false;
                                if (is100wobuff)
                                {
                                    door.GameObject.Health = tHBuffed;
                                }

                                break;
                        }
                    }
                }
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

        private void UpdateRVRStatus()
        {
            // Update players with status of campaign
            foreach (Player plr in Region.Players)
            {
                if (Region.GetTier() == 1)
                {
                    plr.SendClientMessage($"RvR Status : {this.BattleFrontManager.GetActiveCampaign().GetBattleFrontStatus()}", ChatLogFilters.CHATLOGFILTERS_RVR);
                }
                else
                {
                    plr.SendClientMessage($"RvR Status : {this.BattleFrontManager.GetActiveCampaign().GetBattleFrontStatus()}", ChatLogFilters.CHATLOGFILTERS_RVR);
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

            float orderDistanceSum = 0f;
            float destroDistanceSum = 0f;

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
                Keep keep = new Keep(info, (byte)this.Tier, Region);
                keep.Realm = (Realms)keep.Info.Realm;

                Keeps.Add(keep);

                Region.AddObject(keep, info.ZoneId);

                if (info.Creatures != null)
                {
                    BattlefrontLogger.Debug($"Adding {info.Creatures.Count} mobs for Keep {info.KeepId}");
                    foreach (Keep_Creature crea in info.Creatures)
                        keep.Creatures.Add(new KeepNpcCreature(Region, crea, keep));
                }

                if (info.Doors != null)
                {
                    BattlefrontLogger.Debug($"Adding {info.Doors.Count} doors for Keep {info.KeepId}");
                    foreach (Keep_Door door in info.Doors)
                        keep.Doors.Add(new KeepDoor(Region, door, keep));
                }
            }
        }

        public Keep GetClosestKeep(Point3D destPos)
        {
            Keep bestKeep = null;
            ulong bestDist = 0;

            foreach (Keep keep in Keeps)
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

        public Keep GetZoneKeep(ushort zoneId, int realm)
        {
            foreach (Keep keep in Keeps)
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

            BattlefrontLogger.Debug($"{CampaignName} : {(byte)orderPercent} {(byte)destroPercent}");

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
            BattlefrontLogger.Trace($"Updating Campaign scaler : {this.BattleFrontManager.ActiveBattleFrontName}");
            // Update comparative gain
            int index = Tier - 1;

            if (index < 0 || index > 3)
            {
                Log.Error("Campaign", "Region " + Region.RegionId + " has Campaign with tier index " + index);
                return;
            }

            ulong maxContribution = 1;

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
                    var battleFrontId = this.BattleFrontManager.ActiveBattleFront.BattleFrontId;

                    try
                    {
                        if (plr.Realm == Realms.REALMS_REALM_ORDER)
                        {
                            this.OrderPlayerPopulationList[battleFrontId] += 1;
                            _orderCount++;
                        }
                        else
                        {
                            this.DestructionPlayerPopulationList[battleFrontId] += 1;
                            _destroCount++;
                        }
                    }
                    catch { }
                }
            }

            // Buffs
            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.FieldOfGlory), FogAssigned));

            // AAO
            _aaoTracker.NotifyEnteredLake(plr);
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
                    var battleFrontId = this.BattleFrontManager.ActiveBattleFront.BattleFrontId;

                    try
                    {
                        if (plr.Realm == Realms.REALMS_REALM_ORDER)
                        {
                            this.OrderPlayerPopulationList[battleFrontId] -= 1;
                            _orderCount--;
                        }
                        else
                        {
                            this.DestructionPlayerPopulationList[battleFrontId] -= 1;
                            _destroCount--;
                        }
                    }
                    catch { }
                }
            }

            // Buffs
            plr.BuffInterface.RemoveBuffByEntry((ushort)GameBuffs.FieldOfGlory);

            // AAO
            _aaoTracker.NotifyLeftLake(plr);
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

            BattlefrontLogger.Info($"*************************BATTLEFRONT LOCK-START*******************");
            BattlefrontLogger.Info($"forceNumberBags = {forceNumberBags}");
            BattlefrontLogger.Info($"Locking Battlefront {this.CampaignName} to {lockingRealm.ToString()}...");

            var activeBattleFrontId = BattleFrontManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = BattleFrontManager.GetActiveBattleFrontStatus(activeBattleFrontId);

            string message = string.Concat(activeBattleFrontStatus.Description, " locked by ", (lockingRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"), "!");

            BattlefrontLogger.Debug(message);

            if (PlayersInLakeSet == null)
                BattlefrontLogger.Warn($"No players in the Lake!!");
            if (_rewardManager == null)
                BattlefrontLogger.Warn($"_rewardManager is null!!");

            BattlefrontLogger.Info($"*************************BATTLEFRONT GENERATING REWARDS***********");
            GenerateZoneLockRewards(lockingRealm, forceNumberBags);
            BattlefrontLogger.Info($"*************************BATTLEFRONT LOCK-END*********************");

        }

        /// <summary>
        /// Generate zone lock rewards. 
        /// </summary>
        /// <param name="lockingRealm"></param>
        /// <param name="forceNumberBags">By default 0 allows the system to decide the number of bags, setting to -1 forces no rewards.</param>
        private void GenerateZoneLockRewards(Realms lockingRealm, int forceNumberBags = 0)
        {
            var winningRealmPlayers = new List<Player>();
            var losingRealmPlayers = new List<Player>();

            // Calculate no rewards
            if (forceNumberBags == -1)
                return;

            var activeBattleFrontId = BattleFrontManager.ActiveBattleFront.BattleFrontId;
            var activeBattleFrontStatus = BattleFrontManager.GetActiveBattleFrontStatus(activeBattleFrontId);

            var eligiblePlayers = this.BattleFrontManager.GetEligiblePlayers(activeBattleFrontStatus);

            // Remove eligible players from losing realm
            foreach (var eligiblePlayer in eligiblePlayers)
            {
                var player = Player.GetPlayer(eligiblePlayer);
                if (player != null)
                {
                    if (player.Realm == lockingRealm)
                        winningRealmPlayers.Add(player);
                    else
                    {
                        losingRealmPlayers.Add(player);
                    }
                }
            }

            DistributeBaseRewards(losingRealmPlayers, winningRealmPlayers, lockingRealm);

            // Select players from the shortlist to actually assign a reward to. (Eligible and winning realm)
            var rewardSelector = new RewardSelector(new RandomGenerator());
            // Get the character Ids of the winningRealm characters
            var winningRealmCharacterIdList = winningRealmPlayers.Select(x => x.CharacterId).ToList();
            var rewardAssignments = new RewardAssigner(new RandomGenerator(), rewardSelector).AssignLootToPlayers(winningRealmCharacterIdList, forceNumberBags);

            if (rewardAssignments == null)
            {
                BattlefrontLogger.Warn($"No reward assignments found.");
            }
            else
            {
                foreach (var lootBagTypeDefinition in rewardAssignments)
                {
                    BattlefrontLogger.Debug($"Award to be handed out : {lootBagTypeDefinition.ToString()}");
                }

                var lootDecider = new LootDecider(RVRZoneRewardService.RVRZoneLockItemOptions, new RandomGenerator());

                foreach (var lootBagTypeDefinition in rewardAssignments)
                {

                    if (lootBagTypeDefinition.Assignee != 0)
                    {
                        var player = winningRealmPlayers.Single(x => x.CharacterId == lootBagTypeDefinition.Assignee);

                        var playerItemList = (from item in player.ItmInterface.Items where item != null select item.Info.Entry).ToList();

                        var playerRenown = player.CurrentRenown.Level;
                        var playerClass = player.Info.CareerLine;
                        var playerRenownBand = _rewardManager.CalculateRenownBand(playerRenown);

                        var lootDefinition = lootDecider.DetermineRVRZoneReward(lootBagTypeDefinition, playerRenownBand, playerClass, playerItemList.ToList(), true);
                        if (lootDefinition.IsValid())
                        {
                            BattlefrontLogger.Debug($"{player.Info.Name} has received {lootDefinition.FormattedString()}");
                            BattlefrontLogger.Debug($"{lootDefinition.ToString()}");
                            // Only distribute if loot is valid
                            var rewardDescription = WorldMgr.RewardDistributor.DistributeWinningRealm(lootDefinition, player, playerRenownBand);
                            player.SendClientMessage($"{rewardDescription}", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

                        }
                        else
                        {
                            BattlefrontLogger.Debug($"{player.Info.Name} has received [INVALID for Player] {lootDefinition.FormattedString()}");
                        }

                    }
                }
            }
            // Remove eligible players.
            ClearEligiblePlayers(activeBattleFrontStatus, eligiblePlayers);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eligibleLosingRealmPlayers">Eligible losing realm players</param>
        /// <param name="eligibleWinningRealmPlayers">Eligible winning realm playes</param>
        /// <param name="lockingRealm"></param>
        private void DistributeBaseRewards(List<Player> eligibleLosingRealmPlayers, List<Player> eligibleWinningRealmPlayers, Realms lockingRealm)
        {
            // Distribute rewards to losing players with eligibility - halve rewards.
            foreach (var losingRealmPlayer in eligibleLosingRealmPlayers)
            {
                WorldMgr.RewardDistributor.DistributeNonBagAwards(losingRealmPlayer, _rewardManager.CalculateRenownBand(losingRealmPlayer.RenownRank), 0.5);
            }

            // Distribute rewards to winning players with eligibility - full rewards.
            foreach (var winningRealmPlayer in eligibleWinningRealmPlayers)
            {
                WorldMgr.RewardDistributor.DistributeNonBagAwards(winningRealmPlayer, _rewardManager.CalculateRenownBand(winningRealmPlayer.RenownRank), 1);
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
                        if (!eligibleWinningRealmPlayers.Any(x => x.CharacterId == player.CharacterId))
                        {
                            // Give player no bag, but half rewards
                            WorldMgr.RewardDistributor.DistributeNonBagAwards(player, _rewardManager.CalculateRenownBand(player.RenownRank), 0.5);
                        }
                    }
                    else
                    {
                        // Ensure player is not in the eligible list.
                        if (!eligibleLosingRealmPlayers.Any(x => x.CharacterId == player.CharacterId))
                        {
                            // Give player no bag, but quarter rewards
                            WorldMgr.RewardDistributor.DistributeNonBagAwards(player, _rewardManager.CalculateRenownBand(player.RenownRank), 0.25);
                        }
                    }
                }
            }
        }

        


        public void ClearEligiblePlayers(BattleFrontStatus activeBattleFrontStatus, HashSet<uint> eligiblePlayers)
        {
            eligiblePlayers.Clear();
            activeBattleFrontStatus.KillContributionSet.Clear();
            foreach (var campaignObjective in Objectives)
            {
                campaignObjective.CampaignObjectiveContributions.Clear();
            }
            BattlefrontLogger.Debug($"Eligible Players cleared");
        }

        /// <summary>
        /// Helper function to determine whether the active battlefront progression associated with this battlefront is locked.
        /// </summary>
        /// <returns></returns>
        public bool IsBattleFrontLocked()
        {
            return this.BattleFrontManager.IsBattleFrontLocked(this.BattleFrontManager.ActiveBattleFront.BattleFrontId);
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
        //    BattlefrontLogger.Trace($"Resetting Battlefront...{this.CampaignName}");

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
            if (BattleFrontManager.ActiveBattleFront.RegionId != this.Region.RegionId)
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
            BattlefrontLogger.Trace($"Updating Victory Points for {this.CampaignName}");
            // Locked by Order/Dest
            if (IsBattleFrontLocked())
                return; // Nothing to do

            // Only update an active battlefront
            if (BattleFrontManager.ActiveBattleFront.RegionId != this.Region.RegionId)
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
                    CampaignRerollMode rerollMode;
                    var nextBattleFront = BattleFrontManager.AdvanceBattleFront(Realms.REALMS_REALM_ORDER, out rerollMode);
                    // Tell the players
                    SendCampaignMovementMessage(nextBattleFront);
                    // Unlock the next Progression
                    BattleFrontManager.OpenActiveBattlefront(rerollMode);
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
                    CampaignRerollMode rerollMode;
                    var nextBattleFront = BattleFrontManager.AdvanceBattleFront(Realms.REALMS_REALM_DESTRUCTION, out rerollMode);
                    // Tell the players
                    SendCampaignMovementMessage(nextBattleFront);
                    // Unlock the next Progression
                    BattleFrontManager.OpenActiveBattlefront(rerollMode);
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
            CommunicationsEngine.Broadcast(campaignMoveMessage, this.Tier);
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


        /// <summary>
        /// Gets a ream players contribution.
        /// </summary>
        /// <returns>Contribution infos indexed by character id</returns>
        public Dictionary<uint, ContributionInfo> GetContributorsFromRealm(Realms realm)
        {
            return _contributionTracker.GetContributorsFromRealm(realm);
        }

        /// <summary>
        /// <para>Adds contribution for a player. This is based on renown earned and comes from 4 sources at the moment:</para>
        /// <para>- Killing players.</para>
        /// <para>- Objective personal capture rewards.</para>
        /// <para>- Objective defense tick rewards.</para>
        /// <para>- Destroying siege weapons.</para>
        /// </summary>
        /// <param name="plr">Player to give contribution to</param>
        /// <param name="contribution">Contribution value, will be scaled to compute rewards</param>
        public void AddContribution(Player plr, uint contribution)
        {
            _contributionTracker.AddContribution(plr, contribution);
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

            foreach (var obj in this.Objectives)
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
            return BattleFrontManager.IsBattleFrontLocked(this.BattleFrontManager.ActiveBattleFront.BattleFrontId); // Removed from legacy : && Tier > 1
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
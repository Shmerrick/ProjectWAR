using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using SystemData;
using NLog;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.World.Objects.PublicQuests;
using static WorldServer.World.Battlefronts.BattlefrontConstants;

namespace WorldServer.World.Battlefronts.NewDawn
{
    /// <summary>
    /// Represents an open RVR front in a given Region (1 Region -> n Zones) Eg Region 14 (T2 Emp -> Zone 101 Troll Country & Zone 107 Ostland
    /// </summary>
    public class NewDawnBattlefront
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        public VictoryPointProgress VictoryPointProgress { get; set; }

        public RegionMgr Region { get; set; }
        public IBattlefrontManager BattleFrontManager { get; set; }
        /// <summary>
        /// A list of keeps within this Battlefront.
        /// </summary>
        private readonly List<Keep> _Keeps = new List<Keep>();
        public List<BattlefrontObjective> BattlefrontObjectives { get; set; }
        public string BattlefrontName { get; set; }

        protected readonly EventInterface _EvtInterface = new EventInterface();

        public HashSet<Player> PlayersInLakeSet;
        public List<NewDawnBattlefieldObjective> Objectives;
        public bool PairingLocked => LockingRealm != Realms.REALMS_REALM_NEUTRAL;
        public Realms LockingRealm { get; protected set; } = Realms.REALMS_REALM_NEUTRAL;
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
        /// <summary>
        /// List of existing keeps in Battlefront.
        /// </summary>
        /// <remarks>
        /// Must not be updated outside Battlefront implementations.
        /// </remarks>
        public List<Keep> Keeps
        {
            get
            {
                return _Keeps;
            }
        }

        public int Tier { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="regionMgr"></param>
        /// <param name="objectives"></param>
        /// <param name="players"></param>
        public NewDawnBattlefront(RegionMgr regionMgr, List<BattlefrontObjective> objectives, HashSet<Player> players, IBattlefrontManager bfm)
        {
            this.Region = regionMgr;
            this.VictoryPointProgress = new VictoryPointProgress();
            this.BattlefrontObjectives = objectives;
            this.PlayersInLakeSet = players;
            this.Objectives = new List<NewDawnBattlefieldObjective>();
            this.BattleFrontManager = bfm;
            this.BattlefrontName = bfm.GetActivePairing().PairingName;

            Tier = (byte)Region.GetTier();
            LoadObjectives();
            LoadKeeps();
            //On making a battlefront if the tier is 4 locks Objectives adjacent to starting zone
            if (Tier == 4)
            {
                switch (Region.RegionId)
                {
                    case 11:
                        LockBattleObjectivesByZone(105);
                        break;
                    case 4:
                        LockBattleObjectivesByZone(205);
                        break;
                    case 2:
                        LockBattleObjectivesByZone(5);
                        break;
                }
                
            }


            _contributionTracker = new ContributionTracker(Tier, regionMgr);
            _aaoTracker = new AAOTracker();
            _rewardManager = new RVRRewardManager();

            _EvtInterface.AddEvent(UpdateBattlefrontScalers, 12000, 0); // 120000
            _EvtInterface.AddEvent(UpdateVictoryPoints, 6000, 0);
            // Tell each player the RVR status
            _EvtInterface.AddEvent(UpdateRVRStatus, 60000, 0);
            // Recalculate AAO
            _EvtInterface.AddEvent(UpdateAAOBuffs, 60000, 0);
        }

        private void UpdateAAOBuffs()
        {
            _aaoTracker.RecalculateAAO(Region.Players, _orderCount, _destroCount);
        }

        private void UpdateRVRStatus()
        {
            // Update players with status of campaign
            foreach (Player plr in Region.Players)
            {
                if (Region.GetTier() == 1)
                {
                    plr.SendClientMessage($"RvR Status : {WorldMgr.GetRegion((ushort)WorldMgr.LowerTierBattlefrontManager.GetActivePairing().RegionId, false).GetBattleFrontStatus()}", ChatLogFilters.CHATLOGFILTERS_RVR);
                }
                else
                {
                    plr.SendClientMessage($"RvR Status : {WorldMgr.GetRegion((ushort)WorldMgr.UpperTierBattlefrontManager.GetActivePairing().RegionId, false).GetBattleFrontStatus()}", ChatLogFilters.CHATLOGFILTERS_RVR);
                }
            }
        }

        /// <summary>
        /// Loads battlefront objectives.
        /// </summary>

        private void LoadObjectives()
        {
            List<Battlefront_Objective> objectives = BattlefrontService.GetBattlefrontObjectives(Region.RegionId);

            if (objectives == null)
                return; // t1 or database lack

            float orderDistanceSum = 0f;
            float destroDistanceSum = 0f;

            foreach (Battlefront_Objective obj in objectives)
            {
                NewDawnBattlefieldObjective flag = new NewDawnBattlefieldObjective(obj, Region.GetTier());
                Objectives.Add(flag);
                Region.AddObject(flag, obj.ZoneId);
                flag.Battlefront = this;

                //orderDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_ORDER);
                //destroDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_DESTRUCTION);
            }
        }

        /// <summary>
        /// Loads keeps, keep units and doors.
        /// </summary>
        private void LoadKeeps()
        {
            List<Keep_Info> keeps = BattlefrontService.GetKeepInfos(Region.RegionId);

            if (keeps == null)
                return; // t1 or database lack

            foreach (Keep_Info info in keeps)
            {
                Keep keep = new Keep(info, (byte)this.Tier, Region);
                keep.Realm = (Realms)keep.Info.Realm;
                _Keeps.Add(keep);
                Region.AddObject(keep, info.ZoneId);

                if (info.Creatures != null)
                    foreach (Keep_Creature crea in info.Creatures)
                        keep.Creatures.Add(new KeepNpcCreature(Region, crea, keep));

                if (info.Doors != null)
                    foreach (Keep_Door door in info.Doors)
                        keep.Doors.Add(new KeepDoor(Region, door, keep));
            }
        }

        public Keep GetClosestKeep(Point3D destPos)
        {
            Keep bestKeep = null;
            ulong bestDist = 0;

            foreach (Keep keep in _Keeps)
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
            foreach (Keep keep in _Keeps)
                if (keep.Info.KeepId == realm)
                    return keep;
            return null;
        }

        public void WriteCaptureStatus(PacketOut Out)
        {
            // Not implemented.
            _logger.Trace(".");
        }

        /// <summary>
        /// Writes the current zone capture status (gauge in upper right corner of client UI).
        /// </summary>
        /// <param name="Out">Packet to write</param>
        /// <param name="lockingRealm">Realm that is locking the Battlefront</param>
        public void WriteCaptureStatus(PacketOut Out, Realms lockingRealm)
        {
            _logger.Trace(".");
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
                    orderPercent = (VictoryPointProgress.OrderVictoryPoints * 100) / BattlefrontConstants.LOCK_VICTORY_POINTS;
                    destroPercent = (VictoryPointProgress.DestructionVictoryPoints * 100) / BattlefrontConstants.LOCK_VICTORY_POINTS;
                    break;
            }

            _logger.Debug($"{BattlefrontName} : {(byte)orderPercent} {(byte)destroPercent}");

            Out.WriteByte((byte)orderPercent);
            Out.WriteByte((byte)destroPercent);
        }

        #region Reward Splitting

        /// <summary>
        /// A scaler for the reward of objectives captured in this Battlefront, based on its activity relative to other fronts of the same tier.
        /// </summary>
        public float RelativeActivityFactor { get; private set; } = 1f;

        /// <summary>
        /// 100 players max for consideration. Push 30% of reward per hour spent in zone = 0.5% per minute shift max.
        /// </summary>
        public void UpdateBattlefrontScalers()
        {

            // 12.05.18 RA - Not this function currently does nothing.
            _logger.Trace($"Updating Battlefront scaler : {this.BattleFrontManager.GetActivePairing().PairingName}");
            // Update comparative gain
            int index = Tier - 1;

            if (index < 0 || index > 3)
            {
                Log.Error("Battlefront", "Region " + Region.RegionId + " has Battlefront with tier index " + index);
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
        /// removing it from the battlefront's active players list and setting the rvr buff(s).
        /// </summary>
        /// <param name="plr">Player to add, not null</param>
        public void NotifyEnteredLake(Player plr)
        {
            if (!plr.ValidInTier(Tier, true))
                return;

            // Player list tracking
            lock (PlayersInLakeSet)
            {
                if (PlayersInLakeSet.Add(plr))
                {
                    if (plr.Realm == Realms.REALMS_REALM_ORDER)
                        _orderCount++;
                    else
                        _destroCount++;

                    // Add player to the bounty manager on entry to the Lake.
                    Region.BountyManager.AddCharacter(plr.CharacterId, plr.Level, plr.RenownRank);
                    Region.ContributionManager.AddCharacter(plr.CharacterId, 0);
                }
            }

            // Buffs
            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.FieldOfGlory), FogAssigned));
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
        /// removing it from the battlefront's active players lift and removing the rvr buff(s).
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
                    if (plr.Realm == Realms.REALMS_REALM_ORDER)
                        _orderCount--;
                    else
                        _destroCount--;

                    // Remove player from the bounty manager on leaving the Lake.
                    Region.BountyManager.RemoveCharacter(plr.CharacterId);
                }
            }

            // Buffs
            plr.BuffInterface.RemoveBuffByEntry((ushort)GameBuffs.FieldOfGlory);
        }

        public void LockBattleObjective(Realms realm, int objectiveToLock, IList<Battlefront_Objective> objectives)
        {
            _logger.Debug($"Locking Battle Objective : {realm.ToString()}...");

            //if (PairingLocked)
            //{
            //    _logger.Debug($"Pairing already locked");
            //    return; // No effect
            //}

            var myObjectives = new List<NewDawnBattlefieldObjective>();

            foreach (Battlefront_Objective obj in objectives)
            {
                NewDawnBattlefieldObjective flag = new NewDawnBattlefieldObjective(obj, Region.GetTier());
                Objectives.Add(flag);
                Region.AddObject(flag, obj.ZoneId);
                flag.Battlefront = this;

                //orderDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_ORDER);
                //destroDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_DESTRUCTION);
            }

            foreach (var flag in Objectives)
            {
                if (flag.Id == objectiveToLock)
                {
                    flag.LockObjective(realm, false);
                }
            
            }
        }
         


        public void LockBattleObjectivesByZone(int zoneId)
        {
            foreach (var flag in Objectives)
            {
                if ((flag.ZoneId != zoneId) && (flag.RegionId == Region.RegionId))
                {
                    flag.LockObjective(LockingRealm, true);
                }
            }
        }

        public void LockBattleObjective(Realms realm, int objectiveToLock)
        {
            _logger.Debug($"Locking Battle Objective : {realm.ToString()}...");

            foreach (var flag in Objectives)
            {
                if (flag.Id == objectiveToLock)
                {
                    flag.LockObjective(realm, true);
                }
            }
            }

            public void LockPairing(Realms realm)
        {
            _logger.Debug($"Locking Pair : {realm.ToString()}...");

            if (PairingLocked)
            {
                _logger.Warn($"But... it's already locked?!?");
                return; // No effect
            }

            foreach (var flag in Objectives)
                flag.LockObjective(realm, false);

            foreach (var keep in _Keeps)
                keep.LockKeep(realm, true, false);

            this.VictoryPointProgress.Lock(realm);

            // Assigning a non-neutral realm effectively locks the Pair
            LockingRealm = realm;

            WorldMgr.SendCampaignStatus(null);

            string message = string.Concat(Region.ZonesInfo[0].Name, " and ", Region.ZonesInfo[1].Name, " have been locked by ", (realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"), "!");
            _logger.Debug(message);


            if (Tier == 1)
            {
                // Advance the pairing
                WorldMgr.LowerTierBattlefrontManager.AdvancePairing();
                Broadcast($" The campaign has moved to  {WorldMgr.LowerTierBattlefrontManager.GetActivePairing().PairingName}");

                // This may need a rethink and restructure -- reset the VPP for the new Region
                var newRegionId = WorldMgr.LowerTierBattlefrontManager.GetActivePairing().RegionId;
                var newRegion = WorldMgr.GetRegion((ushort)newRegionId, false);

                newRegion.ndbf.ResetPairing();
               
            }
            else
            {
                WorldMgr.UpperTierBattlefrontManager.AdvancePairing();
                Broadcast($"The campaign has moved to {WorldMgr.UpperTierBattlefrontManager.GetActivePairing().PairingName}");

                // This may need a rethink and restructure -- reset the VPP for the new Region
                var newRegionId = WorldMgr.UpperTierBattlefrontManager.GetActivePairing().RegionId;
                var newRegion = WorldMgr.GetRegion((ushort)newRegionId, false);
                if (Tier < 3) { 
                newRegion.ndbf.ResetPairing();
                }
                
            }

            // Generate RP and rewards
            _contributionTracker.CreateGoldChest(realm);
            _contributionTracker.HandleLockReward(realm, 1, message, 0);

            _logger.Debug($"Generating Lock Rewards..");
            foreach (var player in PlayersInLakeSet)
            {
                _logger.Trace($"{player.Name}...");
                _rewardManager.GenerateLockReward(player);
            }
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
        /// Returns the enemy realm's population divided by the input realm's population.
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
        /// <para>- The relative activity in this Battlefront compared to others in its tier</para>
        /// <para>- The total number of people fighting</para>
        /// <para>- The capturing realm's population at this objective.</para>
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


        public void ResetPairing()
        {
            _logger.Trace($"Resetting Pairing...");

            VictoryPointProgress.Reset(this);
            LockingRealm = Realms.REALMS_REALM_NEUTRAL;

            //VictoryPoints = 50;
            //LastAnnouncedVictoryPoints = 50;

            foreach (var flag in Objectives)
                flag.UnlockObjective();

            foreach (Keep keep in _Keeps)
                keep.NotifyPairingUnlocked();

            //Broadcast(Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name + " battlefield objectives are now open for capture!");

            //ActiveSupplyLine = 1;

            //UpdateStateOfTheRealm();

            // This seems to look at all battlefronts and report their status, but incorrectly in the new system.
            // TODO - fix
            // WorldMgr.SendCampaignStatus(null);
        }

        public string ActiveZoneName { get; }
        public bool DefenderPopTooSmall

        { get; set; }

        /// <summary>
        /// Sends information to a player about the objectives within a Battlefront upon their entry.
        /// </summary>
        /// <param name="plr"></param>
        public void SendObjectives(Player plr)
        {
            _logger.Trace(".");
            foreach (NewDawnBattlefieldObjective bo in Objectives)
                bo.SendFlagState(plr, false);
        }

        /// <summary>
        ///  Updates the victory points per realm and fires lock when necessary.
        /// </summary>
        private void UpdateVictoryPoints()
        {
            _logger.Trace($"Updating Victory Points for {this.BattlefrontName}");
            // Locked by Order/Dest
            if (PairingLocked)
                return; // Nothing to do

            // Only update an active pair
            if (BattleFrontManager.GetActivePairing().RegionId != this.Region.RegionId)
                return;

            // Victory depends on objective securization in t1
            float orderVictoryPoints = VictoryPointProgress.OrderVictoryPoints;
            float destroVictoryPoints = VictoryPointProgress.DestructionVictoryPoints;
            int flagCount = 0, destroFlagCount = 0, orderFlagCount = 0;

            foreach (NewDawnBattlefieldObjective flag in Objectives)
            {
                _logger.Trace($"Reward Ticks {this.BattlefrontName} - {flag.ToString()}");
                // TODO - perhaps use AAO calculation here as a pairing scaler?.
                var vp = flag.RewardCaptureTick(1f);

                orderVictoryPoints += vp.OrderVictoryPoints;
                destroVictoryPoints += vp.DestructionVictoryPoints;

                // Make sure VP dont go less than 0
                if (orderVictoryPoints <= 0)
                    orderVictoryPoints = 0;

                if (destroVictoryPoints <= 0)
                    destroVictoryPoints = 0;

                //flagCount++;
                //Realms secureRealm = flag.GetSecureRealm();
                //if (secureRealm == Realms.REALMS_REALM_ORDER)
                //    orderFlagCount++;
                //else if (secureRealm == Realms.REALMS_REALM_DESTRUCTION)
                //    destroFlagCount++;
                _logger.Trace($"{flag.Name} Order VP:{VictoryPointProgress.OrderVictoryPoints} Dest VP:{VictoryPointProgress.DestructionVictoryPoints}");
            }

            // Victory points update
            VictoryPointProgress.OrderVictoryPoints = Math.Min(LOCK_VICTORY_POINTS, orderVictoryPoints);
            VictoryPointProgress.DestructionVictoryPoints = Math.Min(LOCK_VICTORY_POINTS, destroVictoryPoints);

            if (VictoryPointProgress.OrderVictoryPoints >= LOCK_VICTORY_POINTS)
                LockPairing(Realms.REALMS_REALM_ORDER);
            else if (VictoryPointProgress.DestructionVictoryPoints >= LOCK_VICTORY_POINTS)
                LockPairing(Realms.REALMS_REALM_DESTRUCTION);
        }

        public void WriteVictoryPoints(Realms realm, PacketOut Out)
        {

            Out.WriteByte((byte)this.VictoryPointProgress.OrderVictoryPoints);
            Out.WriteByte((byte)this.VictoryPointProgress.DestructionVictoryPoints);

            //no clue but set to a value wont show the pool updatetimer
            Out.WriteByte(0);
            Out.WriteByte(0);

            Out.WriteByte(00);

            //local timer for poolupdates
            //int curTimeSeconds = TCPManager.GetTimeStamp();

            //if (_nextVpUpdateTime == 0 || curTimeSeconds > _nextVpUpdateTime)
            //    Out.WriteUInt32(0);
            //else
            //    Out.WriteUInt32((uint) (_nextVpUpdateTime - curTimeSeconds)); //in seconds
        }

        public int GetZoneOwnership(ushort zoneId)
        {
            _logger.Trace($"GetZoneOwnership {zoneId}");
            const int ZONE_STATUS_CONTESTED = 0;
            const int ZONE_STATUS_ORDER_LOCKED = 1;
            const int ZONE_STATUS_DESTRO_LOCKED = 2;
            // const int ZONE_STATUS_UNLOCKABLE    = 3;

            byte orderKeepsOwned = 0;
            byte destroKeepsOwned = 0;

            //foreach (Keep keep in _Keeps)
            //{
            //    if (keep.Realm == Realms.REALMS_REALM_ORDER && keep.Info.ZoneId == zoneId && keep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
            //    {
            //        orderKeepsOwned++;
            //    }
            //    else if (keep.Realm == Realms.REALMS_REALM_DESTRUCTION && keep.Info.ZoneId == zoneId && keep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED)
            //    {
            //        destroKeepsOwned++;
            //    }
            //}

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

        public void WriteBattlefrontStatus(PacketOut Out)
        {
            _logger.Trace(".");
            //Out.WriteByte((byte)GetZoneOwnership(Zones[2].ZoneId));
            //Out.WriteByte((byte)GetZoneOwnership(Zones[1].ZoneId));
            //Out.WriteByte((byte)GetZoneOwnership(Zones[0].ZoneId));
        }

        public void Broadcast(string message)
        {
            _logger.Info(message);
            lock (Player._Players)
            {
                foreach (Player plr in Player._Players)
                {
                    if (!plr.ValidInTier(Tier, true))
                        continue;

                    plr.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                    plr.SendLocalizeString(message, plr.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
                }
            }
        }

        public void Broadcast(string message, Realms realm)
        {
            _logger.Info(message);
            foreach (Player plr in Region.Players)
            {
                if (!plr.ValidInTier(Tier, true))
                    continue;

                plr.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                plr.SendLocalizeString(message, realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
            }
        }

        /// <summary>
        /// Sends campain diagnostic information to player (gm only).
        /// </summary>
        /// <param name="player">GM to send data to</param>
        /// <param name="bLocalZone">True to display player's local zone, false for tier zones</param>
        public void CampaignDiagnostic(Player player, bool bLocalZone)
        {
            player.SendClientMessage("***** Campaign Status : Region " + Region.RegionId + " *****", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

            player.SendClientMessage("The pairing is " + (PairingLocked ? "locked" : "contested."));

            //foreach (var keep in _Keeps)
            //    keep.SendDiagnostic(player);

            foreach (var objective in Objectives)
                objective.SendDiagnostic(player);

            if (!bLocalZone)
            {
                List<IBattlefront> battleFronts = BattlefrontList.Battlefronts[Tier - 1];
                foreach (IBattlefront battleFront in battleFronts)
                    if (!ReferenceEquals(battleFront, this))
                        battleFront.CampaignDiagnostic(player, true);
            }
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
                if (obj.FlagState != ObjectiveFlags.ZoneLocked)
                {
                    obj.AdvancePopHistory(orderCount, destroCount);
                    _logger.Debug($"AdvancePopHistory Order={orderCount} DestCount={destroCount}");
                }
            }
        }

        public float GetControlHighFor(IBattlefrontFlag currentFlag, Realms realm)
        {
            int count = 0, totalCount;

            if (realm == Realms.REALMS_REALM_ORDER)
            {
                lock (_orderInLake)
                {
                    _syncPlayersList.AddRange(_orderInLake);
                    totalCount = _orderInLake.Count;
                }
            }

            else
            {
                lock (_destroInLake)
                {
                    _syncPlayersList.AddRange(_destroInLake);
                    totalCount = _destroInLake.Count;
                }
            }

            foreach (Player player in _syncPlayersList)
            {
                var flag = GetClosestFlag(player.WorldPosition, true);

                if (flag != null && flag == currentFlag)
                    ++count;
            }

            _syncPlayersList.Clear();

            _logger.Debug($"GetControlHighFor Count={count} TotalCount={totalCount} result={(float)count / totalCount}");

            return (float)count / totalCount;
        }

        // Higher if enemy realm's population is lower.
        public float GetLockPopulationScaler(Realms realm)
        {
            if (realm == Realms.REALMS_REALM_NEUTRAL)
                return 1f;

            // Factor for how much this realm outnumbers the enemy.
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

                if (flag != null && flag.FlagState != ObjectiveFlags.ZoneLocked)
                    flag.AddPlayerInQuadrant(player);

                // Check warcamp farm
                if (player.Zone != null)
                {
                    Realms opposite = player.Realm == Realms.REALMS_REALM_DESTRUCTION ? Realms.REALMS_REALM_ORDER : Realms.REALMS_REALM_DESTRUCTION;
                    Point3D warcampLoc = BattlefrontService.GetWarcampEntrance(player.Zone.ZoneId, opposite);

                    if (warcampLoc != null)
                    {
                        float range = (float)player.GetDistanceTo(warcampLoc);
                        if (range < WARCAMP_FARM_RANGE)
                            player.WarcampFarmScaler = range / WARCAMP_FARM_RANGE;
                        else
                            player.WarcampFarmScaler = 1f;
                    }
                }
            }
        }

        public NewDawnBattlefieldObjective GetClosestFlag(Point3D destPos, bool inPlay = false)
        {
            _logger.Trace(".");
            NewDawnBattlefieldObjective bestFlag = null;
            ulong bestDist = 0;

            foreach (NewDawnBattlefieldObjective flag in Objectives)
            {
                ulong curDist = flag.GetDistanceSquare(destPos);

                if (bestFlag == null || (curDist < bestDist && (!inPlay || flag.FlagState != ObjectiveFlags.ZoneLocked)))
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
            return PairingLocked; // Removed from legacy : && Tier > 1
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
            if (PairingLocked)
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
            }

            return rewardMod;
        }
    }
}
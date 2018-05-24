using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using System.Linq;
using NLog;
using static WorldServer.World.Battlefronts.BattlefrontConstants;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.Services.World;

namespace WorldServer.World.Battlefronts
{
     /// <summary>
    /// Responsible for tracking the RvR campaign's progress along a single front of battle.
    /// </summary>
    public class RoRBattlefront : IBattlefront
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// A list of battlefield objectives within this Battlefront.
        /// </summary>
        private readonly List<ProximityFlag> _Objectives = new List<ProximityFlag>();
        /// <summary>
        /// A list of keeps within this Battlefront.
        /// </summary>
        private readonly List<Keep> _Keeps = new List<Keep>();

        /// <summary>
        /// A list of zones within the scope of this Battlefront.
        /// </summary>
        private readonly List<Zone_Info> Zones;
        /// <summary>
        /// The associated region managed by this Battlefront.
        /// </summary>
        private readonly RegionMgr Region;
        /// <summary>
        /// The tier within which this Battlefront exists.
        /// </summary>
        public readonly byte Tier;

        protected readonly EventInterface _EvtInterface = new EventInterface();

        public virtual void SupplyLineReset() { }

        private AAOTracker _aaoTracker;
        private RationTracker _rationTracker;
        private ContributionTracker _contributionTracker;

        #region Load
        public RoRBattlefront(RegionMgr region, bool oRvRFront)
        {
            Region = region;
            Region.Bttlfront = this;

            Tier = (byte)Region.GetTier();

            // TODO - this is a nasty piece of work by previous devs. Replace with something more suitable.
            if (Tier == 2)
                Tier = 3;
            
            if (oRvRFront && Tier > 0)
                 BattlefrontList.AddBattlefront(this, Tier);

            Zones = Region.ZonesInfo;

            if (oRvRFront)
            {
                LoadObjectives();
                LoadKeeps();

                _aaoTracker = new AAOTracker();
                _rationTracker = new RationTracker(region);
                _contributionTracker = new ContributionTracker(Tier, region);

                _EvtInterface.AddEvent(RecalculateAAO, 10000, 0); // 20000
                _EvtInterface.AddEvent(BattlePopulationDistributionData, 6000, 0); // 60000
                _EvtInterface.AddEvent(UpdateBattlefrontScalers, 12000, 0); // 120000
            }
        }

        /// <summary>
        /// Loads battlefront objectives.
        /// </summary>
        private void LoadObjectives()
        {
            List<Battlefront_Objective> objectives = BattlefrontService.GetBattlefrontObjectives(Region.RegionId);

            _logger.Warn($"Calling LoadObjectives from RoRBattleFront");

            if (objectives == null)
                return; // t1 or database lack

            float orderDistanceSum = 0f;
            float destroDistanceSum = 0f;

            foreach (Battlefront_Objective obj in objectives)
            {
                ProximityFlag flag = new ProximityFlag(obj, this, Region, Tier);
                _Objectives.Add(flag);
                Region.AddObject(flag, obj.ZoneId);

                orderDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_ORDER);
                destroDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_DESTRUCTION);

                _logger.Debug($"...Obj Entry:{obj.Entry} Name:{obj.Name} ZoneId:{obj.ZoneId} Region:{obj.RegionId}");
                _logger.Debug($"...Flag State:{flag.FlagState} State:{flag.State}");
            }

            // Sets the scaler to compute securization rewards
#if DEBUG
            if (Tier != 1)
            {
                _logger.Error("Tier != 1, Distance scaler in t2, t3, t4 - should consider keeps");
                //throw new NotImplementedException("Distance scaler in t2, t3, t4 - should consider keeps");
            }
#endif
            foreach (ProximityFlag flag in _Objectives)
                flag.SetWarcampDistanceScaler(orderDistanceSum / objectives.Count, destroDistanceSum / objectives.Count);
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
                Keep keep = new Keep(info, Tier, Region);
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
        #endregion

        #region Updates
        /// <summary>
        /// Main battlefront update method, invoked by region manager, short perdiod.
        /// </summary>
        /// <param name="start">Timestamp of region manager update start time</param>
        public void Update(long tick)
        {
            //UpdateStateOfTheRealm();
            _EvtInterface.Update(tick);
        }

        private void BattlePopulationDistributionData()
        {
            long tick = TCPManager.GetTimeStampMS();

            RecalculatePopFactor();

            foreach (ProximityFlag flag in _Objectives)
            {
                if (flag.OwningRealm != Realms.REALMS_REALM_NEUTRAL)
                    ; //Console.Error.WriteLine("TODO flag ccontribution"); // flag.TickDefense(tick / 1000);
            }

            _contributionTracker.TickContribution(tick / 1000);

            UpdatePopulationDistribution();

            //if (Tier > 1)
            //{
            //    if (!PairingLocked)
            //        UpdateVictoryPoints();
            //    UpdateRationing();
            //}

            //// Unlocking
            //if (PairingUnlockTime > 0 && PairingUnlockTime < TCPManager.GetTimeStampMS())
            //    ResetPairing();
        }
        #endregion

        #region Players tracking
        private readonly HashSet<Player> _playersInLakeSet = new HashSet<Player>();

        //private readonly List<Player> _orderInLake = new List<Player>();
        //private readonly List<Player> _destroInLake = new List<Player>();
        private volatile int _orderCount = 0;
        private volatile int _destroCount = 0;

        /// <summary>
        /// A scale factor for the general reward received from capturing a Battlefield Objective, which increases as more players join the zone.
        /// </summary>
        public float PopulationScaleFactor { get; private set; }

        /// <summary>
        /// A scale factor determined by the population ratio between the realms as determined by the maximum players they fielded over the last 15 minutes.
        /// </summary>
        private float _relativePopulationFactor;

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
            lock (_playersInLakeSet)
            {
                if (_playersInLakeSet.Add(plr))
                {
                    if (plr.Realm == Realms.REALMS_REALM_ORDER)
                        _orderCount++;
                    else
                        _destroCount++;
                }
            }

            _aaoTracker.NotifyEnteredLake(plr);

            // Buffs
            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.FieldOfGlory), FogAssigned));
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
            lock (_playersInLakeSet)
            {
                if (_playersInLakeSet.Remove(plr))
                {
                    if (plr.Realm == Realms.REALMS_REALM_ORDER)
                        _orderCount--;
                    else
                        _destroCount--;
                }
            }

            _aaoTracker.NotifyLeftLake(plr);

            // Buffs
            plr.BuffInterface.RemoveBuffByEntry((ushort)GameBuffs.FieldOfGlory);
            
            _rationTracker.RemoveRationed(plr);
        }

        /// <summary>
        /// Invoked by buff interface to remove field of glory if necessary.
        /// </summary>
        /// <param name="fogBuff">Buff that was created</param>
        public void FogAssigned(NewBuff fogBuff)
        {
            if (fogBuff == null || !(fogBuff.Caster is Player))
                return;

            lock (_playersInLakeSet)
            {
                if (!_playersInLakeSet.Contains(fogBuff.Caster))
                    fogBuff.BuffHasExpired = true;
            }
        }

        /// <summary>
        /// Recalculates AAO.
        /// </summary>
        public void RecalculateAAO()
        {
            _aaoTracker.RecalculateAAO(_syncPlayersList, _orderCount, _destroCount);
        }
        #endregion

        #region Population Reward Scale Factors
        private readonly List<int>[] _popHistory = { new List<int>(), new List<int>() };

        /// <summary>Scale factor or artillery damages</summary>
        private float[] _ArtilleryDamageScale = { 1f, 1f };

        /// <summary>
        /// Gets the artillery damage scale factor.
        /// </summary>
        /// <param name="realm">Owner realm of the weapon</param>
        /// <returns>Scale factor</returns>
        public float GetArtilleryDamageScale(Realms realm)
        {
            return _ArtilleryDamageScale[(int)realm - 1];
        }

        private void RecalculatePopFactor()
        {
            if (_popHistory[0].Count > 14)
            {
                _popHistory[0].RemoveAt(0);
                _popHistory[1].RemoveAt(0);
            }

            _popHistory[0].Add(_orderCount);
            _popHistory[1].Add(_destroCount);

            int orderPop = Math.Max(12, _popHistory[0].Max());
            int destroPop = Math.Max(12, _popHistory[1].Max());

            foreach (Keep keep in _Keeps)
                keep.ScaleLord(keep.Realm == Realms.REALMS_REALM_ORDER ? destroPop : orderPop);

            _relativePopulationFactor = Point2D.Clamp(orderPop / (float)destroPop, 0.25f, 4f);

            int popBase = Math.Min(orderPop, destroPop);

            if (popBase < 50)
                PopulationScaleFactor = 1;
            else if (popBase < 100)
                PopulationScaleFactor = 1.35f;
            else if (popBase < 200)
                PopulationScaleFactor = 1.65f;
            else
                PopulationScaleFactor = 2f;

            if (orderPop > destroPop)
            {
                _ArtilleryDamageScale[0] = 2f - Math.Min(1.5f, _relativePopulationFactor);
                _ArtilleryDamageScale[1] = Math.Min(1.1f, _relativePopulationFactor);
            }

            else
            {
                _ArtilleryDamageScale[1] = 2f - Math.Min(1.5f, 1f / _relativePopulationFactor);
                _ArtilleryDamageScale[0] = Math.Min(1.1f, 1f / _relativePopulationFactor);
            }
        }

        /// <summary>
        /// Returns the enemy realm's population divided by the input realm's population.
        /// </summary>
        private float GetRelativePopFactor(Realms realm)
        {
            return realm == Realms.REALMS_REALM_DESTRUCTION ? _relativePopulationFactor : 1f / _relativePopulationFactor;
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
                BattlefrontFlag flag = (BattlefrontFlag)GetClosestFlag(player.WorldPosition, true);

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

        public IBattlefrontFlag GetClosestFlag(Point3D destPos, bool inPlay = false)
        {
            ProximityFlag bestFlag = null;
            ulong bestDist = 0;

            foreach (ProximityFlag flag in _Objectives)
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
        #endregion

        #region Battlefield Objective Lock Mechanics
        /// <summary>List of players in lake accessible through main update thread without locking</summary>
        private List<Player> _syncPlayersList = new List<Player>();

        /// <summary>
        /// Traces population distribution around objectives to update history.
        /// <para/>The collected data is used to handle zerg movement detection.
        /// <para/>Update the list of players around dedicated to update thread.
        /// </summary>
        private void UpdatePopulationDistribution()
        {
            _syncPlayersList.Clear();
            lock (_playersInLakeSet)
                _syncPlayersList.AddRange(_playersInLakeSet);

            foreach (Player player in _syncPlayersList)
            {
                ProximityFlag flag = (ProximityFlag)GetClosestFlag(player.WorldPosition, true);

                if (flag != null && flag.State != StateFlags.ZoneLocked)
                    flag.AddPlayerInQuadrant(player);
            }

            foreach (ProximityFlag obj in _Objectives)
            {
                if (obj.State != StateFlags.ZoneLocked)
                    obj.AdvancePopHistory(_orderCount, _destroCount);
            }
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
        
        /// <summary>
        /// Scales battlefield objective rewards by the following factors:
        /// <para>- The internal AAO</para>
        /// <para>- The relative activity in this Battlefront compared to others in its tier</para>
        /// <para>- The total number of people fighting</para>
        /// <para>- The capturing realm's population at this objective.</para>
        /// </summary>
        public float GetObjectiveRewardScaler(Realms capturingRealm, int playerCount)
        {
            float scaleMult = GetRelativePopFactor(capturingRealm) * PopulationScaleFactor * RelativeActivityFactor;

            int maxRewardPlayers = 6;

            if (_popHistory[(int)capturingRealm - 1].Count > 0)
                maxRewardPlayers = Math.Max(6, _popHistory[(int)capturingRealm - 1].Max() / 5);

            if (playerCount > maxRewardPlayers)
                scaleMult *= maxRewardPlayers / (float)playerCount;

            return scaleMult;
        }
        #endregion

        #region Kill Modifiers

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

            ProximityFlag closestFlag = (ProximityFlag)GetClosestFlag(killed.WorldPosition);

            if (closestFlag != null)
            {
                closestFlag.AccumulatedKills++;

                // Defense kill. Weight the kill higher depending on the distance from the opposing objective (proactive defending)
                if (killer.Realm == closestFlag.OwningRealm)
                    rewardMod += Math.Min(killed.GetDistanceTo(closestFlag), 1000)*0.001f*0.5f;
                // Attack kill. Weight the kill higher if it was closer to the objective (high penetration)
                else
                    rewardMod += (1000 - Math.Min(killed.GetDistanceTo(closestFlag), 1000))*0.001f*0.5f;
            }

            return rewardMod;
        }

        public bool PreventKillReward()
        {
            return PairingLocked; // Removed from legacy : && Tier > 1
        }

        public Keep GetZoneKeep(ushort zoneId, int realm)
        {
            foreach (Keep keep in _Keeps)
                if (keep.Info.KeepId == realm)
                    return keep;
            return null;
        }
        #endregion

        #region Pairing Lock

        /// <summary>
        /// Set if this pairing's zones are currently locked.
        /// </summary>
        public bool PairingLocked => LockingRealm != Realms.REALMS_REALM_NEUTRAL;

        public Realms LockingRealm { get; protected set; } = Realms.REALMS_REALM_NEUTRAL;

        /// <summary>
        /// Locks a pairing, preventing any interaction with objectives within.
        /// </summary>
        /// <param name="realm">Realm that locked the battlefront</param>
        /// <param name="announce">True to announce the lock to players</param>
        public virtual void LockPairing(Realms realm, bool announce, bool restoreStatus = false, bool noRewards = false, bool draw = false)
        {
            if (PairingLocked)
                return; // No effect

            foreach (ProximityFlag flag in _Objectives)
                flag.LockObjective(realm, announce);

            foreach (Keep keep in _Keeps)
            {
                _contributionTracker.CreateGoldChest(keep, realm);
                keep.LockKeep(realm, announce, false);
            }
            
            LockingRealm = realm;

            WorldMgr.SendCampaignStatus(null);

            string message = string.Concat(Region.ZonesInfo[0].Name, " and ", Region.ZonesInfo[1].Name, " have been locked by ", (realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"), "!");

            try
            {
                //Log.Info("Zone Lock", "Locking "+Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name);
                _contributionTracker.HandleLockReward(realm, 1, message, 0);
            }
            catch (Exception e)
            {
                Log.Error("HandleLockReward", "Exception thrown: "+e);
            }

            ActiveSupplyLine = 0;

            UpdateStateOfTheRealm();

            _contributionTracker.Reset();
        }
        
        /// <summary>
        /// Returns the pairing to its open state, allowing interaction with objectives.
        /// </summary>
        public virtual void ResetPairing()
        {
            LockingRealm = Realms.REALMS_REALM_NEUTRAL;

            VictoryPoints = 50;
            LastAnnouncedVictoryPoints = 50;

            foreach (var flag in _Objectives)
                flag.UnlockObjective();

            foreach (Keep keep in _Keeps)
                keep.NotifyPairingUnlocked();

            Broadcast(Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name + " battlefield objectives are now open for capture!");

            ActiveSupplyLine = 1;

            UpdateStateOfTheRealm();

            WorldMgr.SendCampaignStatus(null);
        }
#endregion

        #region Keeps
        /// <summary>
        /// List of existing battlefield objectives within this Battlefront.
        /// </summary>
        /// <remarks>
        /// Must not be updated outside Battlefront implementations.
        /// </remarks>
        public IEnumerable<IBattlefrontFlag> Objectives
        {
            get
            {
                return _Objectives;
            }
        }

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
        #endregion
        
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
            _contributionTracker.UpdateLoserShare(_orderCount, _destroCount);

            // Update comparative gain
            int index = Tier - 1;

            if (index < 0 || index > 3)
            {
                Log.Error("Battlefront", "Region "+Region.RegionId+" has Battlefront with tier index "+index);
                return;
            }

            ulong maxContribution = 1;

            foreach (RoRBattlefront fnt in BattlefrontList.Battlefronts[index])
            {
                if (fnt == null || fnt._contributionTracker == null)
                    continue; // Still not fully initialized
                if (fnt._contributionTracker.TotalContribFromRenown > maxContribution)
                    maxContribution = fnt._contributionTracker.TotalContribFromRenown;
            }

            RelativeActivityFactor = _contributionTracker.TotalContribFromRenown / (float)maxContribution;

#if Battlefront_DEBUG
            foreach (Player player in Region.Players)
                player.SendClientMessage($"UpdateBattlefrontScalers: Relative scaler for this Battlefront: {RelativeActivityFactor} Winner VP: {WinnerShare} Loser VP: {LoserShare}");
#endif

            _nextVpUpdateTime = TCPManager.GetTimeStamp() + 120;

            foreach (Player player in Region.Players)
                WorldMgr.SendCampaignStatus(player);
        }
        #endregion

        #region Victory Points
        protected int VictoryPoints = 50;
        protected int LastAnnouncedVictoryPoints = 50;

        private long _nextVpUpdateTime;

        /// <summary>
        /// Writes current front victory points.
        /// </summary>
        /// <param name="realm">Recipent player's realm</param>
        /// <param name="Out">TCP output</param>
        public void WriteVictoryPoints(Realms realm, PacketOut Out)
        {
            if (realm == Realms.REALMS_REALM_ORDER)
            {
                Out.WriteByte((byte)VictoryPoints);
                Out.WriteByte((byte)(100 - VictoryPoints));
            }
            else
            {
                Out.WriteByte((byte)(100 - VictoryPoints));
                Out.WriteByte((byte)VictoryPoints);
            }

            //no clue but set to a value wont show the pool updatetimer
            Out.WriteByte(0);
            Out.WriteByte(0);

            Out.WriteByte(00);

            //local timer for poolupdates
            int curTimeSeconds = TCPManager.GetTimeStamp();

            if (_nextVpUpdateTime == 0 || curTimeSeconds > _nextVpUpdateTime)
                Out.WriteUInt32(0);
            else
                Out.WriteUInt32((uint)(_nextVpUpdateTime - curTimeSeconds));   //in seconds
        }

        /// <summary>
        /// Checks whether the given realm can reclaim opposite keep.
        /// </summary>
        /// <param name="realm">Realm to check</param>
        /// <returns>True if can reclaim</returns>
        public bool CanReclaimKeep(Realms realm)
        {
            return false;
        }


        /// <summary>
        /// Checks whether the given realm's keep can sustain it's current rank.
        /// </summary>
        /// <param name="realm">Realm to check</param>
        /// <param name="resourceValueMax">Max resource value of the keep to check (in its current rank)</param>
        /// <returns>True if can sustain</returns>
        /// <remarks>
        /// May move this method to Keep class, kept it here for compatibility break risks.
        /// </remarks>
        public bool CanSustainRank(Realms realm, int resourceValueMax)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Send
        /// <summary>
        /// Sends information to a player about the objectives within a Battlefront upon their entry.
        /// </summary>
        /// <param name="plr"></param>
        public void SendObjectives(Player plr)
        {
            foreach (ProximityFlag flag in _Objectives)
                flag.SendFlagState(plr, false);
        }

        public void Broadcast(string message)
        {
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
            foreach (Player plr in Region.Players)
            {
                if (!plr.ValidInTier(Tier, true))
                    continue;

                plr.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                plr.SendLocalizeString(message, realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
            }
        }

        public virtual void WriteBattlefrontStatus(PacketOut Out)
        {
            //throw new InvalidOperationException("Only valid for a T4 Battlefront.");
        }

        public virtual void WriteCaptureStatus(PacketOut Out)
        {
            Out.WriteByte(0);

            switch (LockingRealm)
            {
                case Realms.REALMS_REALM_NEUTRAL:
                    Out.WriteByte(50);
                    Out.WriteByte(50);
                    break;
                case Realms.REALMS_REALM_ORDER:
                    Out.WriteByte(100);
                    Out.WriteByte(0);
                    break;
                case Realms.REALMS_REALM_DESTRUCTION:
                    Out.WriteByte(0);
                    Out.WriteByte(100);
                    break;
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
            player.SendClientMessage($"Ration factors:  Order {_rationTracker.RationFactor[0]} Destruction {_rationTracker.RationFactor[1]}");

            InternalCampaignDiagnostic(player, bLocalZone);

            foreach (var keep in _Keeps)
                keep.SendDiagnostic(player);

            foreach (var objective in _Objectives)
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
        /// May be overridden by implementors to display specific diagnostic data.
        /// </summary>
        /// <param name="player">GM to send data to</param>
        /// <param name="bLocalZone">True to display player's local zone, false for tier zones</param>
        protected virtual void InternalCampaignDiagnostic(Player player, bool bLocalZone) { }
        #endregion

        #region Delegates
        /// <summary>
        /// Gets the ration factor that should be applied to given unit.
        /// </summary>
        /// <param name="unit">To applie factor to, not null</param>
        /// <returns>Factor less or equal 1f</returns>
        public float GetRationFactor(Unit unit)
        {
            return _rationTracker.GetRationFactor(unit);
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

        /// <summary>Gets the active zones names</summary>
        [Obsolete("in use ?")]
        public string ActiveZoneName => $"{Zones[0].Name} and {Zones[1].Name}";

        /// <summary>Gets the pairing of the battlefront</summary>
        public Pairing Pairing => (Pairing)Region.ZonesInfo[0].Pairing;
        #endregion

        #region State of the Realm

        public int ActiveSupplyLine = 0;

        public void UpdateStateOfTheRealm()
        {
            string boStatus = "";
            //string keepStatus = "";

            foreach (Zone_Info zone in Zones)
            {
                if (zone != null && !PairingLocked && zone.Tier == 1)
                {
                    boStatus = "SoR_T" + zone.Tier + "_BO:" + zone.ZoneId;

                    foreach (ProximityFlag flag in _Objectives)
                    {
                        if (flag != null)
                        {
                            boStatus = boStatus + ":" + flag.ID + ":" + (int)flag.OwningRealm + ":" + (int)flag._state;
                            long now = TCPManager.GetTimeStampMS();
                            long timer = (flag._nextTransitionTimestamp - TCPManager.GetTimeStampMS()) / 1000;
                            if (timer > 0)
                                boStatus = boStatus + ":" + timer;
                            else
                                boStatus = boStatus + ":0";
                        }
                    }
                    if (zone.ZoneId == 106)
                    {
                        boStatus = boStatus + ":0:0:0:0";
                    }

                    boStatus = boStatus + ":" + VictoryPoints + ":" + ActiveSupplyLine;

                    /*keepStatus = "SoR_T" + zone.Tier + "_Keep:" + zone.ZoneId;

                    foreach (Keep keep in _Keeps)
                    {
                        if (keep != null)
                        {
                            keepStatus = keepStatus + ":" + keep.Info.KeepId + ":" + (int)keep.Realm + ":" + keep.Rank + ":" + (int)keep.KeepStatus + ":" + (int)keep.LastMessage;
                        }
                    }*/
                }
            }
            if (boStatus != "")
            {
                foreach (Player plr in Player._Players.ToList())
                {
                    if (plr != null && plr.SoREnabled)
                    {
                        //plr.SendLocalizeString(boStatus, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);

                        //if (keepStatus != "")
                            //plr.SendLocalizeString(keepStatus, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
                    }
                }
            }
        }

        #endregion

        #region Legacy
        /// <summary>For legacy purpose.</summary>
        public bool NoSupplies { get { return true; } }

        /// <summary>For legacy purpose.</summary>
        public float GetControlHighFor(IBattlefrontFlag currentFlag, Realms realm)
        {
            return 0f;
        }
        #endregion
    }
}
//#define BattleFront_DEBUG

#define BattleFront_FLAG_GUARDS

#if !DEBUG && BattleFront_DEBUG
#error BattleFront DEBUG ENABLED IN RELEASE BUILD
#endif

/*

    TODO BattleFront:

    - Examine whether locking is really necessary here

*/

using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using System.Linq;
using NLog;
using WorldServer.World.BattleFronts;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.BattleFronts.Keeps;
using WorldServer.World.BattleFronts.Objectives;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;
using BattleFrontConstants = WorldServer.World.BattleFronts.BattleFrontConstants;

namespace WorldServer
{
    /// <summary>
    /// Responsible for tracking the RvR campaign's progress along a single front of battle.
    /// </summary>
    public class BattleFront : IBattleFront
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        #region Load

        public string Name;

        public readonly BattleFrontStatus _BattleFrontStatus;
        public BattleFront Info;

        /// <summary>
        /// This is used to modify the timer of VP Update - default from Aza system was 120 seconds, 
        /// so TIMER_MODIFIER should be set to 1.0f, currently we are cutting it by half, change it
        /// back to 1.0f to restore default value
        /// </summary>
        private const float TIMER_MODIFIER = 0.5f;

        /// <summary>
        /// A list of battlefield objectives within this BattleFront.
        /// </summary>
        public List<BattleFrontFlag> _Objectives = new List<BattleFrontFlag>();

        /// <summary>
        /// A list of keeps within this BattleFront.
        /// </summary>
        public List<Keep> _Keeps = new List<Keep>();

        /// <summary>
        /// A list of zones within the scope of this BattleFront.
        /// </summary>
        protected List<Zone_Info> Zones;
        /// <summary>
        /// The associated region managed by this BattleFront.
        /// </summary>
        public RegionMgr Region { get; }
        /// <summary>
        /// The tier within which this BattleFront exists.
        /// </summary>
        protected readonly byte Tier;
        /// <summary>
        /// This is variable that stores current active supply line
        /// </summary>
        public int ActiveSupplyLine = 0;
        /// <summary>
        /// This is pairing
        /// </summary>
        public Pairing pairing;

        public EventInterface EvtInterface { get; protected set; } = new EventInterface();

        public BattleFront(RegionMgr region, bool oRvRFront)
        {
            _BattleFrontStatus = BattleFrontService.GetStatusFor(region.RegionId);

            _logger.Warn($"BattleFront : Status = {_BattleFrontStatus}");

            Region = region;
            Region.Bttlfront = this;

            Tier = (byte)Region.GetTier();

            if (Constants.DoomsdaySwitch == 0)
            { 
                if (Tier == 2)
                    Tier = 3;
            }

            if (oRvRFront && Tier > 0)
                 BattleFrontList.AddBattleFront(this, Tier);

            TotalContribFromRenown = (ulong)(Tier * 50);

            Zones = Region.ZonesInfo;

            if (oRvRFront)
            {
                LoadObjectives();
                LoadKeeps();

                EvtInterface.AddEvent(RecalculateAAO, 10000, 0);
                EvtInterface.AddEvent(MinuteUpdate, 60000, 0);
                EvtInterface.AddEvent(UpdateBattleFrontScalers, (int)(120000 * TIMER_MODIFIER), 0);
                EvtInterface.AddEvent(CheckSpawnFarm, 10000, 0);
            }
            switch (Region.RegionId)
            {
                case 1: //Dwarf
                case 2:
                case 10:
                case 12:
                    pairing = Pairing.PAIRING_GREENSKIN_DWARVES;
                    break;

                case 3: //Elf
                case 4:
                case 15:
                case 16:
                    pairing = Pairing.PAIRING_ELVES_DARKELVES;
                    break;

                case 6: //Empire
                case 8:
                case 11:
                case 14:
                    pairing = Pairing.PAIRING_EMPIRE_CHAOS;
                    break;
            }

            // This is new and used for DoomsDay event
            if (Constants.DoomsdaySwitch > 0)
            { 
                if (oRvRFront && Tier > 0)
                    LoadMidTierPairing();
            }
        }

        // This is new and used for DoomsDay event
        private void LoadMidTierPairing()
        {
            Log.Info("BattleFront.LoadPairing", " Region: " + Region.RegionId + " | LOADING CAMPAIGN");

            if (_BattleFrontStatus == null)
            {
                Log.Error("BattleFront.LoadPairing", "No BattleFront Status - campaign resetting.");
                ResetPairing();
                return;
            }

            PairingLocked = false;
            GraceDisabled = false;

            if (Tier == 2)
            {
                ResetPairing();
            }
            else if (Tier > 2)
                LockPairing(Realms.REALMS_REALM_NEUTRAL, false, true);

            LockingRealm = Realms.REALMS_REALM_NEUTRAL;
            DefendingRealm = Realms.REALMS_REALM_NEUTRAL;

            new ApocCommunications().SendCampaignStatus(null, null);
        }

        public void MinuteUpdate()
        {
            UpdateContributions();
            UpdatePopulationDistribution();
            if (Tier > 1)
            {
                if (!PairingLocked && !_NoSupplies)
                    UpdateVictoryPoints();
                UpdateRationing();
            }

            // This is used by State of the Realm addon
            UpdateStateOfTheRealm();
        }

        private void LoadObjectives()
        {
            List<BattleFront_Objective> objectives = BattleFrontService.GetBattleFrontObjectives(Region.RegionId);

            if (objectives == null)
                return;

            foreach (BattleFront_Objective obj in objectives)
            {
                BattleFrontFlag flag = new BattleFrontFlag(obj.Entry, obj.Name, obj.ZoneId, obj.X, obj.Y, obj.Z, obj.O, (int)obj.TokDiscovered, (int)obj.TokUnlocked, Tier);
                _Objectives.Add(flag);
                Region.AddObject(flag, obj.ZoneId);
#if BattleFront_FLAG_GUARDS
                if (obj.Guards != null)
                    foreach (BattleFront_Guard guard in obj.Guards)
                        flag.Guards.Add(new RvRFlagGuard(Region, guard.ZoneId, guard.OrderId, guard.DestroId, guard.X, guard.Y, guard.Z, guard.O));
#endif
            }
        }

        private void LoadKeeps()
        {
            List<Keep_Info> keeps = BattleFrontService.GetKeepInfos(Region.RegionId);

            if (keeps == null)
                return;

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

        /*
        private static readonly List<BattleFront>[] RegionManagers = { new List<BattleFront>(), new List<BattleFront>(), new List<BattleFront>(), new List<BattleFront>()};

        public static void AddFront(BattleFront front)
        {
            lock (RegionManagers[front.Tier - 1])
                RegionManagers[front.Tier - 1].Add(front);
        }*/

        #endregion

        public void Update(long tick)
        {
            if (PairingUnlockTime > 0 && PairingUnlockTime < tick)
            {
                ResetPairing();
                PairingUnlockTime = 0;
            }

            EvtInterface.Update(tick);
        }

        public virtual string ActiveZoneName => $"{Zones[0].Name} and {Zones[1].Name}";

        #region Against All Odds

        private readonly HashSet<Player> _playersInLakeSet = new HashSet<Player>();

        private readonly List<Player> _orderInLake = new List<Player>();
        private readonly List<Player> _destroInLake = new List<Player>();

        private readonly List<NewBuff> _orderAAOBuffs = new List<NewBuff>();
        private readonly List<NewBuff> _destroAAOBuffs = new List<NewBuff>();

        private int _againstAllOddsMult;

        /// <summary>
        /// A scale factor determined by the population ratio between the realms as determined by the maximum players they fielded over the last 15 minutes.
        /// </summary>
        private float _relativePopulationFactor;

        /// <summary>
        /// A scale factor for the general reward received from capturing a Battlefield Objective, which increases as more players join the zone.
        /// </summary>
        public float PopulationScaleFactor { get; private set; }

        public void NotifyEnteredLake(Player plr)
        {
            if (!plr.ValidInTier(Tier, true))
                return;

            //Log.Info("AAO", "Lake Entry by "+plr.Name+" in "+_region.ZonesInfo[0].Name);
            lock (_playersInLakeSet)
            {
                if (_playersInLakeSet.Contains(plr))
                    return;
                _playersInLakeSet.Add(plr);
            }

            if (plr.Realm == Realms.REALMS_REALM_ORDER)
            {
                lock (_orderInLake)
                    _orderInLake.Add(plr);
            }
            else
            {
                lock (_destroInLake)
                    _destroInLake.Add(plr);
            }

            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.FieldOfGlory), AssignFOG));

            if ((plr.Realm == Realms.REALMS_REALM_ORDER && _againstAllOddsMult < 0) || (plr.Realm == Realms.REALMS_REALM_DESTRUCTION && _againstAllOddsMult > 0))
                plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, plr.Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.AgainstAllOdds), AssignAAO));
        }

        public void AssignAAO(NewBuff aaoBuff)
        {
            if (aaoBuff == null)
                return;

            if (aaoBuff.Caster.Realm == Realms.REALMS_REALM_ORDER)
            {
                if (_againstAllOddsMult >= 0)
                    aaoBuff.BuffHasExpired = true;
                else
                {
                    lock (_orderAAOBuffs)
                        _orderAAOBuffs.Add(aaoBuff);

                    aaoBuff.AddBuffParameter(1, 1);
                    aaoBuff.AddBuffParameter(2, Math.Abs(_againstAllOddsMult) * 20);
                    aaoBuff.AddBuffParameter(3, Math.Abs(_againstAllOddsMult) * 20);
                    aaoBuff.AddBuffParameter(4, Math.Abs(_againstAllOddsMult) * 20);
                    ((Player)aaoBuff.Caster).AAOBonus = Math.Abs(_againstAllOddsMult) * 0.2f;
                }

            }
            else if (aaoBuff.Caster.Realm == Realms.REALMS_REALM_DESTRUCTION)
            {
                if (_againstAllOddsMult <= 0)
                    aaoBuff.BuffHasExpired = true;

                else
                {
                    lock (_destroAAOBuffs)
                        _destroAAOBuffs.Add(aaoBuff);

                    aaoBuff.AddBuffParameter(1, 1);
                    aaoBuff.AddBuffParameter(2, Math.Abs(_againstAllOddsMult) * 20);
                    aaoBuff.AddBuffParameter(3, Math.Abs(_againstAllOddsMult) * 20);
                    aaoBuff.AddBuffParameter(4, Math.Abs(_againstAllOddsMult) * 20);
                    ((Player)aaoBuff.Caster).AAOBonus = Math.Abs(_againstAllOddsMult) * 0.2f;
                }
            }
        }

        public void AssignFOG(NewBuff fogBuff)
        {
            if (fogBuff == null)
                return;

            lock (_playersInLakeSet)
            {
                if (!_playersInLakeSet.Contains(fogBuff.Caster))
                    fogBuff.BuffHasExpired = true;
            }
        }

        public void NotifyLeftLake(Player plr)
        {
            if (!plr.ValidInTier(Tier, true))
                return;

            //Log.Info("AAO", "Lake Exit by " + plr.Name + " in " + _region.ZonesInfo[0].Name);
            lock (_playersInLakeSet)
            {
                if (!_playersInLakeSet.Contains(plr))
                    return;

                _playersInLakeSet.Remove(plr);
            }

            plr.BuffInterface.RemoveBuffByEntry((ushort)GameBuffs.FieldOfGlory);
            plr.WarcampFarmScaler = 1f;

            if (plr.Realm == Realms.REALMS_REALM_ORDER)
            {
                lock (_orderInLake)
                    _orderInLake.Remove(plr);
                lock (_orderAAOBuffs)
                {
                    for (int i = 0; i < _orderAAOBuffs.Count; ++i)
                    {
                        if (_orderAAOBuffs[i].Caster == plr)
                        {
                            _orderAAOBuffs[i].BuffHasExpired = true;
                            _orderAAOBuffs.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
            else
            {
                lock (_destroInLake)
                    _destroInLake.Remove(plr);
                lock (_destroAAOBuffs)
                {
                    for (int i = 0; i < _destroAAOBuffs.Count; ++i)
                    {
                        if (_destroAAOBuffs[i].Caster == plr)
                        {
                            _destroAAOBuffs[i].BuffHasExpired = true;
                            _destroAAOBuffs.RemoveAt(i);
                            break;
                        }
                    }
                }
            }

            int realmIndex = (int)plr.Realm - 1;

            lock (_withinKeepRange)
            {
                if (!_withinKeepRange[realmIndex].Contains(plr))
                    return;
            }

            RemoveRationed(plr, realmIndex);
        }

        private void UpdateAAOBuffs(byte realmIndex, float newMult)
        {
            if (realmIndex == 0)
            {
                lock (_orderAAOBuffs)
                {
                    foreach (NewBuff buff in _orderAAOBuffs)
                    {
                        buff.DeleteBuffParameter(1);
                        buff.DeleteBuffParameter(2);
                        buff.DeleteBuffParameter(3);
                        buff.DeleteBuffParameter(4);
                        buff.AddBuffParameter(1, 1);
                        buff.AddBuffParameter(2, (short)(Math.Abs(newMult) * 20));
                        buff.AddBuffParameter(3, (short)(Math.Abs(newMult) * 20));
                        buff.AddBuffParameter(4, (short)(Math.Abs(newMult) * 20));
                        ((Player)buff.Caster).AAOBonus = Math.Abs(newMult) * 0.2f;
                        buff.SoftRefresh();
                    }
                }
            }

            else
            {
                lock (_destroAAOBuffs)
                {
                    foreach (NewBuff buff in _destroAAOBuffs)
                    {
                        buff.DeleteBuffParameter(1);
                        buff.DeleteBuffParameter(2);
                        buff.DeleteBuffParameter(3);
                        buff.DeleteBuffParameter(4);
                        buff.AddBuffParameter(1, 1);
                        buff.AddBuffParameter(2, (short)(Math.Abs(newMult) * 20));
                        buff.AddBuffParameter(3, (short)(Math.Abs(newMult) * 20));
                        buff.AddBuffParameter(4, (short)(Math.Abs(newMult) * 20));
                        ((Player)buff.Caster).AAOBonus = Math.Abs(newMult) * 0.2f;
                        buff.SoftRefresh();
                    }
                }
            }
        }

        private void ClearAAO(byte realmIndex)
        {
            if (realmIndex == 0)
            {
                lock (_orderAAOBuffs)
                {
                    foreach (NewBuff buff in _orderAAOBuffs)
                    {
                        buff.BuffHasExpired = true;
                        ((Player)buff.Caster).AAOBonus = 0;
                    }
                    _orderAAOBuffs.Clear();
                }
            }

            else
            {
                lock (_destroAAOBuffs)
                {
                    foreach (NewBuff buff in _destroAAOBuffs)
                    {
                        buff.BuffHasExpired = true;
                        ((Player)buff.Caster).AAOBonus = 0;
                    }
                    _destroAAOBuffs.Clear();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void RecalculateAAO()
        {
            int newMult;

            //Log.Info(_region.ZonesInfo[0].Name, "O: " + _orderInLake.Count + " D: " + _destroInLake.Count);

            if (_orderInLake.Count == 0)
            {
                if (_destroInLake.Count == 0)
                    newMult = 0;
                else newMult = -20;
            }
            else if (_destroInLake.Count == 0)
                newMult = 20;
            else
            {
                float ratio = _orderInLake.Count / (float)_destroInLake.Count;

                //Log.Info(_region.ZonesInfo[0].Name, "R: "+ratio);

                // Less than one means Order AAO
                if (ratio < 1f)
                {
                    ratio = 1f / ratio;
                    ratio *= -1;
                    ratio += 1f;
                }

                else
                {
                    ratio -= 1f;
                }

                newMult = (int)(ratio * 5f);

                if (newMult < -20)
                    newMult = -20;
                else if (newMult > 20)
                    newMult = 20;
            }

            if (newMult == _againstAllOddsMult)
                return;

            // No AAO for either side. Clear it from whichever side presently had it.
            if (newMult == 0)
                ClearAAO(_againstAllOddsMult < 0 ? (byte)0 : (byte)1);

            // Destro AAO
            else if (newMult > 0)
            {
                // Switching from Order, or no AAO, to Destro.
                if (_againstAllOddsMult <= 0)
                {
                    lock (_destroInLake)
                        foreach (Player plr in _destroInLake)
                            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, 40, AbilityMgr.GetBuffInfo((ushort)GameBuffs.AgainstAllOdds), AssignAAO));

                    // Order had AAO, clear theirs.
                    if (_againstAllOddsMult < 0)
                        ClearAAO(0);
                }

                // Destro already have AAO. Update it.
                else
                    UpdateAAOBuffs(1, newMult);
            }

            // Order AAO
            else if (newMult < 0)
            {
                if (_againstAllOddsMult >= 0)
                {
                    lock (_orderInLake)
                        foreach (Player plr in _orderInLake)
                            plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, 40, AbilityMgr.GetBuffInfo((ushort)GameBuffs.AgainstAllOdds), AssignAAO));

                    if (_againstAllOddsMult > 0)
                        ClearAAO(1);
                }

                else
                    UpdateAAOBuffs(0, newMult);
            }

            _againstAllOddsMult = newMult;
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

            _popHistory[0].Add(_orderInLake.Count);
            _popHistory[1].Add(_destroInLake.Count);

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

#if BattleFront_DEBUG
            foreach (Player player in Region.Players)
                player.SendClientMessage($"Population factors updated. Relative pop factor: {_relativePopulationFactor} Population scale factor: {PopulationScaleFactor}");
#endif
        }

        /// <summary>
        /// Returns the enemy realm's population divided by the input realm's population.
        /// </summary>
        public float GetRelativePopFactor(Realms realm)
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
                BattleFrontFlag flag = (BattleFrontFlag)GetClosestFlag(player.WorldPosition, true);

                if (flag != null && flag.FlagState != ObjectiveFlags.ZoneLocked)
                    flag.AddPlayerInQuadrant(player);

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

        public IBattleFrontFlag GetClosestFlag(Point3D destPos, bool inPlay = false)
        {
            BattleFrontFlag bestFlag = null;
            ulong bestDist = 0;

            foreach (BattleFrontFlag flag in _Objectives)
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

        public BattleFrontFlag GetClosestNeutralFlagTo(Point3D destPos)
        {
            BattleFrontFlag bestFlag = null;
            ulong bestDist = 0;

            foreach (BattleFrontFlag flag in _Objectives)
            {
                if (flag._owningRealm != Realms.REALMS_REALM_NEUTRAL)
                    continue;

                ulong curDist = flag.GetDistanceSquare(destPos);

                if (bestFlag == null || curDist < bestDist)
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

        private void UpdatePopulationDistribution()
        {
            int orderCount, destroCount;

            _syncPlayersList.Clear();

            lock (_orderInLake)
            {
                _syncPlayersList.AddRange(_orderInLake);
                orderCount = _orderInLake.Count;
            }

            lock (_destroInLake)
            {
                _syncPlayersList.AddRange(_destroInLake);
                destroCount = _destroInLake.Count;
            }

            foreach (var obj in _Objectives)
            {
                if (obj.FlagState != ObjectiveFlags.ZoneLocked)
                    obj.AdvancePopHistory(orderCount, destroCount);
            }
        }

        public float GetControlHighFor(IBattleFrontFlag currentFlag, Realms realm)
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
                IBattleFrontFlag flag = GetClosestFlag(player.WorldPosition, true);

                if (flag != null && flag == currentFlag)
                    ++count;
            }

            _syncPlayersList.Clear();

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

        public int GetBaseLockTime(Realms realm)
        {
            /*
            int popCount = realm == Realms.REALMS_REALM_ORDER ? _orderInLake.Count : _destroInLake.Count;

            if (popCount >= 96) // 4+ WB
                return 3 * 60000;
            if (popCount >= 72) // 3-4 WB
                return (int)(4.5f * 60000);
            if (popCount >= 48) // 2-3 WB
                return 6 * 60000;
            if (popCount >= 24) // 1-2 WB
                return (int)(7.5f * 60000);

            return 9 * 60000; // 1 WB or less*/

            return (int)(12 * 60 * 1000 * TIMER_MODIFIER);
        }

        /// <summary>
        /// Scales battlefield objective rewards by the following factors:
        /// <para>- The internal AAO</para>
        /// <para>- The relative activity in this BattleFront compared to others in its tier</para>
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

        #region Rationing

#if BattleFront_DEBUG
        const int SUPPLY_BASE_SUPPORT = 1;
#else
        /// <summary>
        /// The number of players which a keep is always capable of supporting.
        /// </summary>
        const int SUPPLY_BASE_SUPPORT = 24;
#endif

        public float[] RationFactor { get; } = { 1f, 1f };
        private readonly int[] _supplyCaps = new int[2];
        private readonly HashSet<Player>[] _withinKeepRange = { new HashSet<Player>(), new HashSet<Player>() };
        private readonly List<RationBuff>[] _rationDebuffs = { new List<RationBuff>(), new List<RationBuff>() };

        protected Realms DefendingRealm = Realms.REALMS_REALM_NEUTRAL;

        /// <summary>
        /// Gets the ration factor that should be applied to given unit.
        /// </summary>
        /// <param name="unit">To applie factor to, not null</param>
        /// <returns>Factor less or equal 1f</returns>
        public float GetRationFactor(Unit unit) {
            return RationFactor[(int)unit.Realm - 1];
        }

        private void AddRationed(Player player, int index)
        {
            #if BattleFront_DEBUG
            foreach (Player plr in Region.Players)
                plr.SendClientMessage($"{player.Name} is now in ration range.");
            #endif
            lock (_withinKeepRange)
                _withinKeepRange[index].Add(player);

            if (RationFactor[index] > 1f)
                player.BuffInterface.QueueBuff(new BuffQueueInfo(player, 1, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Rationing), RationBuff.CreateRationBuff, AssignRationDebuff));
        }

        private void RemoveRationed(Player player, int index)
        {
            #if BattleFront_DEBUG
            foreach (Player plr in Region.Players)
                plr.SendClientMessage($"{plr.Name} is no longer in ration range.");
            #endif

            lock (_withinKeepRange)
                _withinKeepRange[index].Remove(player);

            lock (_rationDebuffs[index])
                for (int i = 0; i < _rationDebuffs[index].Count; ++i)
                {
                    if (_rationDebuffs[index][i].Target == player)
                    {
                        _rationDebuffs[index][i].BuffHasExpired = true;
                        _rationDebuffs[index].RemoveAt(i);
                        break;
                    }
                }
        }

        private void RemoveAllRationed()
        {
            #if BattleFront_DEBUG
            foreach (Player plr in Region.Players)
                plr.SendClientMessage("Removing all rationed players.");
            #endif
            for (int index = 0; index < 2; ++index)
            {
                lock (_withinKeepRange)
                    _withinKeepRange[index].Clear();

                lock (_rationDebuffs[index])
                    foreach (RationBuff buff in _rationDebuffs[index])
                        buff.BuffHasExpired = true;

                _rationDebuffs[index].Clear();
            }
        }

        public void UpdateRationing()
        {
            int[] popHigh = { _popHistory[0].Max(), _popHistory[1].Max() };

            for (int i = 0; i < 2; ++i)
            {
                Keep currentKeep = _Keeps.Find(keep => (int)keep.Realm == i + 1 && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && keep.KeepStatus != KeepStatus.KEEPSTATUS_SEIZED);

                if (currentKeep == null)
                { 
                    RemoveAllRationed();
                    return;
                }

                #if BattleFront_DEBUG
                foreach (Player plr in Region.Players)
                    plr.SendClientMessage("Found keep "+currentKeep.Info.Name+" for "+i);
                #endif

                List<Player> curList = (i == 0 ? _orderInLake : _destroInLake);

                lock (curList)
                {
                    foreach (Player plr in curList)
                    {
                        bool isInKeepRange = plr.IsWithinRadiusFeet(currentKeep, 600);
                        if (_withinKeepRange[i].Contains(plr))
                        {
                            if (!isInKeepRange)
                                RemoveRationed(plr, i);
                        }
                        else if (isInKeepRange)
                        {
                            AddRationed(plr, i);
                        }
                    }
                }

                int rationedCount = _withinKeepRange[i].Count;

                // The keep's supply cap is the population high of the enemy realm over the last 15 minutes.
                // TODO
                //_supplyCaps[i] = (int)Math.Max(SUPPLY_BASE_SUPPORT, popHigh[1 - i] * currentKeep.GetRationFactor());

                if (rationedCount <= _supplyCaps[i])
                {
                    // The realm is within ration tolerance.
                    // Return if not penalized.
                    if (RationFactor[i] == 1f)
                        continue;

                    //Remove the penalty.
                    RationFactor[i] = 1f;

                    lock (_rationDebuffs[i])
                    {
                        foreach (RationBuff b in _rationDebuffs[i])
                            b.BuffHasExpired = true;

                        _rationDebuffs[i].Clear();
                    }
                }

                else
                {
                    // There are more members within the keep than its supplies can allow for.
                    float newRationFactor = (int)((rationedCount * 5) / (float)_supplyCaps[i]);
                    newRationFactor *= 0.2f;

                    if (newRationFactor > 1f)
                    {
                        newRationFactor -= 1f;
                        newRationFactor *= 0.35f;
                        newRationFactor += 1f;
                    }

                    #if BattleFront_DEBUG
                    foreach (Player player in Region.Players)
                        player.SendClientMessage($"Rationing: {(i == 0 ? "Order" : "Destruction")} penalty ({newRationFactor}).");
                    #endif

                    if (newRationFactor == RationFactor[i])
                        continue;

                    if (RationFactor[i] == 1f)
                    {
                        lock (_withinKeepRange)
                            foreach (Player player in _withinKeepRange[i])
                                player.BuffInterface.QueueBuff(new BuffQueueInfo(player, 1, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Rationing), RationBuff.CreateRationBuff, AssignRationDebuff));
                    }

                    else
                    {
                        // Update existing ration debuffs
                        lock (_rationDebuffs[i])
                        {
                            foreach (RationBuff debuff in _rationDebuffs[i])
                                debuff.PendingDebuffFactor = newRationFactor;
                        }
                    }

                    RationFactor[i] = newRationFactor;
                }
            }
        }

        public void AssignRationDebuff(NewBuff rationDebuff)
        {
            if (rationDebuff == null)
                return;

            int realmIndex = (int) rationDebuff.Caster.Realm - 1;

            if (RationFactor[realmIndex] == 1f || rationDebuff.Target.Region != Region)
                rationDebuff.BuffHasExpired = true;
            else
            {
                lock (_rationDebuffs[realmIndex])
                    _rationDebuffs[realmIndex].Add((RationBuff)rationDebuff);
            }
        }

        #endregion

        #region Tiers 1-3

        public int GetCapturedPct(int team)
        {
            double captured = 0;
            byte keepcaptured = 0;

            foreach (BattleFrontFlag flag in _Objectives)
            {
                if ((byte) flag._owningRealm == team)
                    captured++;
            }
            foreach (Keep keep in _Keeps)
            { 
                if ((byte)keep.Realm == team)
                    keepcaptured++;
            }


            if (Tier == 1)
            {
                if (PairingLocked)
                    return team == (byte) LockingRealm ? 100 : 0;
                return (int) ((float) captured/_Objectives.Count*99);
            }

            return keepcaptured * 50;
        }

        #endregion

        #region Capture Events

        /// <summary>
        /// The number of held battlefield objectives for each realm within the active zone in the BattleFront.
        /// </summary>
        protected readonly byte[] HeldObjectives = new byte[2];

        private Realms _dominatingRealm = Realms.REALMS_REALM_NEUTRAL;

        public virtual void ObjectiveCaptured(Realms oldRealm, Realms newRealm, ushort zoneId)
        {
            HeldObjectives[(byte)newRealm - 1]++;

            if (oldRealm != Realms.REALMS_REALM_NEUTRAL)
                HeldObjectives[(byte)oldRealm - 1]--;

            if (Tier == 1)
            {
                if (HeldObjectives[(byte) newRealm - 1] == _Objectives.Count)
                {
                    _dominatingRealm = newRealm;
                    #if DEBUG
                    EvtInterface.AddEvent(DominationTimerEnd, 60 * 1000 * 2, 1);
                    #else
                    EvtInterface.AddEvent(DominationTimerEnd, (int)(60 * 1000 * 15 * TIMER_MODIFIER), 1);
                    #endif
                    Broadcast($"The forces of {(newRealm == Realms.REALMS_REALM_ORDER ? "Order " : "Destruction ")} threaten to capture this area!", newRealm);
                }

                else if (_dominatingRealm != Realms.REALMS_REALM_NEUTRAL)
                {
                    _dominatingRealm = Realms.REALMS_REALM_NEUTRAL;
                    EvtInterface.RemoveEvent(DominationTimerEnd);
                    Broadcast($"The forces of {(oldRealm == Realms.REALMS_REALM_ORDER ? "Order " : "Destruction ")} have regained a foothold.", newRealm);
                }
            }

            // Enable supplies on first BattleFront to have 4 objectives captured
            else
            {
                if (HeldObjectives[0] + HeldObjectives[1] == 2)
                {
                    int arr;
                    if (Constants.DoomsdaySwitch == 2)
                        arr = (int)pairing;
                    else
                        arr = Tier;
                    if (BattleFrontList.ActiveFronts[arr - 1] == null)
                    {
                        lock (BattleFrontList.ActiveFronts)
                        {
                            if (BattleFrontList.ActiveFronts[arr - 1] == null)
                                BattleFrontList.ActiveFronts[arr - 1] = this;
                        }

                        if (BattleFrontList.ActiveFronts[arr - 1] == this)
                        { 
                            if (Constants.DoomsdaySwitch > 0)
                            { 
                                if (Tier == 2)
                                {
                                    Zone_Info info = ZoneService.GetZone_Info((ushort)zoneId);

                                    BattleFrontStatus BattleFrontStatus = BattleFrontService.GetStatusFor(info.Region);

                                    if (Constants.DoomsdaySwitch != 2)
                                    {
                                        foreach (BattleFront b in BattleFrontList.BattleFronts[Tier - 1])
                                        {
                                            if (b.Region.RegionId != info.Region)
                                            {
                                                b.LockPairing(Realms.REALMS_REALM_NEUTRAL, false, true);
                                            }
                                        }
                                    }
                                }
                            }
                            EnableSupplies();
                        }
                    }

                    UpdateStateOfTheRealm();
                }
            }
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

            BattleFrontFlag closestFlag = (BattleFrontFlag)GetClosestFlag(killed.WorldPosition);

            if (closestFlag != null && closestFlag.FlagActive())
            {
                closestFlag.AccumulatedKills++;

                // Defense kill. Weight the kill higher depending on the distance from the opposing objective (proactive defending)
                if (killer.Realm == closestFlag._owningRealm)
                    rewardMod += Math.Min(killed.GetDistanceTo(closestFlag), 1000)*0.001f*0.5f;
                // Attack kill. Weight the kill higher if it was closer to the objective (high penetration)
                else
                    rewardMod += (1000 - Math.Min(killed.GetDistanceTo(closestFlag), 1000))*0.001f*0.5f;
            }

			#if BattleFront_DEBUG
            killer.SendClientMessage($"Closest flag for kill: {closestFlag.ObjectiveName} Reward mod: {rewardMod}");
			#endif

            return rewardMod;
        }

        public bool PreventKillReward()
        {
            return GraceDisabled && Tier > 1;
        }

        public Keep GetZoneKeep(ushort zoneId, int realm)
        {
            return _Keeps.First(keep => keep.Info.KeepId == realm);
        }

        /// <summary>
        /// Gets a factor to applies on defenders lock rewards.
        /// </summary>
        /// <param name="realm">Attacking realm</param>
        /// <returns>Positive factor</returns>
        public float GetDefenseBias(Realms realm)
        {
            return 0.8f + HeldObjectives[(int) realm - 1] * 0.1f;
        }

        /// <summary>
        /// Gets a factor to applies on attackers lock rewards.
        /// </summary>
        /// <param name="realm">Attacking realm</param>
        /// <returns>Positive factor</returns>
        public float GetAttackBias(Realms realm)
        {
            return 0.8f + ((4 - HeldObjectives[(int) realm - 1]) * 0.1f);
        }

        #endregion

        #region Pairing Lock

        /// <summary>
        /// Set if this pairing's zones are currently locked.
        /// </summary>
        public bool PairingLocked { get; set; }

        /// <summary>
        /// Set after zone is locked to allow for some finishing moves from players still there.
        /// </summary>
        public bool GraceDisabled { get; set; }

        /// <summary>
        /// The time when this pairing will reopen.
        /// </summary>
        public long PairingUnlockTime { get; set; }

        public Realms LockingRealm { get; protected set; } = Realms.REALMS_REALM_NEUTRAL;

        public void DominationTimerEnd()
        {
            LockPairing(_dominatingRealm, true);
            _dominatingRealm = Realms.REALMS_REALM_NEUTRAL;
        }

        /// <summary>
        /// Locks a pairing, preventing any interaction with objectives within.
        /// </summary>
        /// <param name="realm"></param>
        /// <param name="announce"></param>
        public void LockPairing(Realms realm, bool announce, bool restoreStatus = false, bool noRewards = false, bool draw = false)
        {
            foreach (BattleFrontFlag flag in _Objectives)
                flag.LockObjective(realm, announce);

            foreach (Keep keep in _Keeps)
            {
                Dictionary<uint, ContributionInfo> contributors = GetContributorsFromRealm((Realms) keep.Info.Realm);

                if (contributors.Count > 0 && !noRewards)
                {
                    Log.Info("BattleFront", $"Creating gold chest for {keep.Info.Name} for {contributors.Count} {((Realms)keep.Info.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")} contributors");
                    GoldChest.Create(Region, keep.Info.PQuest, ref contributors, (Realms) keep.Info.Realm == realm ? WinnerShare : LoserShare);
                }

                keep.LockKeep(realm, announce, false);
            }

            UpdateStateOfTheRealm();

            PairingLocked = true;

            #if DEBUG
            EvtInterface.AddEvent(EndGrace, 90 * 1000, 1);
            #else
            EvtInterface.AddEvent(EndGrace, 10 * 60 * 1000, 1);
            #endif

            LockingRealm = realm;

            new ApocCommunications().SendCampaignStatus(null, null);

            // DoomsDay change - we want to unlock stuff only after we locked something
            if (Constants.DoomsdaySwitch == 0)
                SetPairingUnlockTimer();

            try
            {
                string message = Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name + " have been locked by " + (realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + "!";
                //Log.Info("Zone Lock", "Locking "+Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name);
                if(!noRewards)
                    HandleLockReward(realm, 1, message, 0);
            }
            catch (Exception e)
            {
                Log.Error("HandleLockReward", "Exception thrown: "+e);
            }

            PlayerContributions.Clear();

            TotalContribFromRenown = (ulong)(Tier * 50);

            int arr;
            if (Constants.DoomsdaySwitch == 2)
                arr = (int)pairing;
            else
                arr = Tier;

            if (BattleFrontList.ActiveFronts[arr - 1] == this)
                BattleFrontList.ActiveFronts[arr - 1] = null;

            DisableSupplies();

            // DoomsDay change - we change the way zone unlocks are handled for now
            if (Constants.DoomsdaySwitch == 0)
            { 
                foreach (BattleFront b in BattleFrontList.BattleFronts[Tier - 1])
                    if (b != this)
                        b.EvtInterface.AddEvent(b.SupplyLineReset, 1, 1);
            }
            else
            { 
                if (!restoreStatus)
                {
                    _BattleFrontStatus.ControlingRealm = (int)realm;
                    _BattleFrontStatus.ActiveRegionOrZone = 0;
                    WorldMgr.Database.SaveObject(_BattleFrontStatus);

                    UnlockRegion();
                }
            }
        }

        /// <summary>
        /// This is used instead of BattleFrontLocked to allow some killing after zone is locked
        /// </summary>
        public void EndGrace()
        {
            GraceDisabled = true;
        }
        public void StartGrace()
        {
            GraceDisabled = false;
        }

        /// <summary>
        /// This unlocks the next region in correct order
        /// </summary>
        protected void UnlockRegion()
        {
            IBattleFront bttlfrnt = null;
            BattleFront t2Bttlfrnt = null;
            
            int region = -1;
            int openZoneIndex = 0;
            int activeRegionOrZone = 0;
            int keepRank = 0;

            switch (Region.RegionId)
            {
                case 12: //T2 Dwarf vs Greenskins
                    bttlfrnt = WorldMgr.GetRegion(10, false).Bttlfront;
                    region = 10;
                    activeRegionOrZone = 1;
                    break;

                case 14: //T2 Empire vs Chaos
                    bttlfrnt = WorldMgr.GetRegion(6, false).Bttlfront;
                    region = 6;
                    activeRegionOrZone = 1;
                    break;

                case 15: //T2 HE vs DE
                    bttlfrnt = WorldMgr.GetRegion(16, false).Bttlfront;
                    region = 16;
                    activeRegionOrZone = 1;
                    break;

                case 10: //T3 Dwarf vs Greenskins
                    t2Bttlfrnt = (BattleFront)WorldMgr.GetRegion(12, false).Bttlfront;
                    if (t2Bttlfrnt.LockingRealm == LockingRealm)
                        keepRank = 1;

                    bttlfrnt = WorldMgr.GetRegion(2, false).Bttlfront;
                    region = 2;
                    openZoneIndex = 1;
                    activeRegionOrZone = 5;
                    break;

                case 6: //T3 Empire vs Chaos
                    t2Bttlfrnt = (BattleFront)WorldMgr.GetRegion(14, false).Bttlfront;
                    if (t2Bttlfrnt.LockingRealm == LockingRealm)
                        keepRank = 1;

                    bttlfrnt = WorldMgr.GetRegion(11, false).Bttlfront;
                    region = 11;
                    openZoneIndex = 1;
                    activeRegionOrZone = 105;
                    break;

                case 16: //T3 HE vs DE
                    t2Bttlfrnt = (BattleFront)WorldMgr.GetRegion(15, false).Bttlfront;
                    if (t2Bttlfrnt.LockingRealm == LockingRealm)
                        keepRank = 1;

                    bttlfrnt = WorldMgr.GetRegion(4, false).Bttlfront;
                    region = 4;
                    openZoneIndex = 1;
                    activeRegionOrZone = 205;
                    break;

                case 2: //T4 D vs O
                case 4: //T4 E vs C
                case 11: //T4 HE vs DE

                    foreach (BattleFront b in BattleFrontList.BattleFronts[1])
                    {
                        b._BattleFrontStatus.OpenZoneIndex = 0;
                        b._BattleFrontStatus.ActiveRegionOrZone = 1;
                        WorldMgr.Database.SaveObject(b._BattleFrontStatus);
                        b.ResetPairing();
                        b.SupplyLineReset();
                        b.GraceDisabled = false;
                        b.EvtInterface.AddEvent(StartGrace, 60 * 1000, 5);
                    }

                    bttlfrnt = null;
                    break;

            }

            if (bttlfrnt != null)
            { 
                bttlfrnt.ResetPairing();
                bttlfrnt.SupplyLineReset();
                if (keepRank != 0)
                { 
                    foreach(Keep keep in bttlfrnt.Keeps)
                    {
                        if (keep.Realm == LockingRealm && (keep.ZoneId == 5 || keep.ZoneId == 105 || keep.ZoneId == 205))
                        {
                            keep.Rank = 1;
                        }
                    }
                }

                BattleFrontStatus bttlfrntStatus = BattleFrontService.GetStatusFor(region);
                bttlfrntStatus.OpenZoneIndex = openZoneIndex;
                bttlfrntStatus.ActiveRegionOrZone = activeRegionOrZone;
                bttlfrntStatus.ControlingRealm = 0;
                WorldMgr.Database.SaveObject(bttlfrntStatus);
            }

        }

        /// <summary>
        /// Rewards players based on their contribution, converting it to XP, RP, Influence and Medallions.
        /// </summary>
        protected void HandleLockReward(Realms realm, float winnerRewardScale, string lockMessage, int zoneId)
        {
            /*
            Some general notes on this.

            The ticker, if kicked consistently by a player during the course of one hour, will grant contribution which converts to 3000 RP over 60 minutes.
            Rewards are based on a player's contribution performance relative to the best player on their realm.
            The RP from a lock cannot exceed Tier * 5000, and by extension, the XP cannot exceed Tier * 15000, 
            the Influence cannot exceed Tier * 500 and there can be no more than 6 medallions issued.

            The rewards are scaled by the proximity of a zone to the enemy fortress.

            The rewards are also scaled by the relative BattleFront scaler, which cripples rewards for players 
            refusing to fight in the most hotly contested zone.

            For the losing side, the reward is also scaled by the % of rewards, linked to the Victory Point pool.

            To receive the max losing reward, the top contributor on the losing realm must have contribution greater than 1/3 of the top attacker contribution.
            */

            string zoneName;

            if (zoneId == 0)
            {
                zoneName = Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name;
            }
                
                
            else
            {
                zoneName = ZoneService.GetZone_Info((ushort)zoneId).Name;
            }


            uint xpCap = (uint)(Tier * 40000);
            uint rpCap = (uint)(Tier * 10000);
            ushort infCap = (ushort)(Tier * 2000);

#region Init winner rewards
            Dictionary<uint, ContributionInfo> winnerContrib = Region.Bttlfront.GetContributorsFromRealm(realm);

            uint winMaxContrib = 1;
            if (winnerContrib.Count > 0)
                winMaxContrib = winnerContrib.Values.Max(x => x.BaseContribution);
            //Log.Info(zoneName, $"Winner contributor count : {winnerContrib.Count} Max contribution: {winMaxContrib}");

            uint winRP = (uint)(winMaxContrib * 1.5 * winnerRewardScale);
            uint winXP = winRP * 4;
            ushort winInf = (ushort)(winRP * 0.25f);
            ushort winMedallionCount = (ushort)Math.Min(20, winRP / (450 * Tier));

            //Log.Info(zoneName, $"Lock XP: {winXP} RP: {winRP} Inf: {winInf} Medals: {winMedallionCount}");

#endregion

#region Init loser rewards

            Dictionary<uint, ContributionInfo> loserContrib = Region.Bttlfront.GetContributorsFromRealm(realm == Realms.REALMS_REALM_ORDER ? Realms.REALMS_REALM_DESTRUCTION : Realms.REALMS_REALM_ORDER);

            uint loserMaxContrib = 1;

            if (loserContrib.Count > 0)
                loserMaxContrib = loserContrib.Values.Max(x => x.BaseContribution);

            //Log.Info(zoneName, $"Loser contributor count : {loserContrib.Count} Max contribution: {loserMaxContrib}");

            uint lossRP = (uint)(winRP * LoserShare * Math.Min(1f, (loserMaxContrib * 3) / (float)winMaxContrib));
            uint lossXP = lossRP * 5;
            ushort lossInf = (ushort)(lossRP * 0.35f);
            ushort lossMedallionCount = (ushort)Math.Min(15, winMedallionCount * LoserShare);

            //Log.Info(zoneName, $"Lock XP: {lossXP} RP: {lossRP} Inf: {lossInf} Medallions: {lossMedallionCount}");

#endregion

            Item_Info medallionInfo = ItemService.GetItem_Info((uint)(208399 + Tier));
            Item_Info T3Token = ItemService.GetItem_Info(2165);
            Item_Info T4Token = ItemService.GetItem_Info(2166);
            Item_Info conqMedallion = ItemService.GetItem_Info(1698);

            ushort tokenCount = 2;

            foreach (var kV in PlayerContributions)
            {
                Player plr = Player.GetPlayer(kV.Key);

                Character chara = plr != null ? plr.Info : CharMgr.GetCharacter(kV.Key, false);

                if (chara == null)
                    continue;

                if (plr != null)
                {
                    plr.SendLocalizeString(lockMessage, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                    plr.SendLocalizeString(lockMessage, realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE, Localized_text.CHAT_TAG_DEFAULT);
                    // AAO multiplier needs to be multiplied with 20 to get the AAO that player sees. 
                    // AAO mult is the global value for the server to grab the difference in size of the teams while AAOBonus is the players individual bonus
                    int aaoBuff = Convert.ToInt32(plr.AAOBonus);
                    if (aaoBuff < 0.1 && _againstAllOddsMult >= 1)
                    {
                        tokenCount = 1;
                    }
                    if (aaoBuff >= 2)
                    {
                        tokenCount = 3;
                    }
                    if (aaoBuff >= 3)
                    {
                        tokenCount = 4;
                    }
                    if (aaoBuff >= 4)
                    {
                        tokenCount = 5;
                    }

#if DEBUG
                    Log.Texte("", plr.Name + ": AAO mult: " + _againstAllOddsMult + " and " + plr.AAOBonus + " aao bonus, mod:" + aaoBuff, ConsoleColor.DarkMagenta);
#endif

                }


                ContributionInfo contrib = kV.Value;

                if (chara.Realm == (int)realm)
                {
                    if (winRP == 0)
                        continue;

                    float contributionFactor = Math.Min(1f, contrib.BaseContribution / (winMaxContrib * 0.7f));

                    string contributionDesc;

                    if (contributionFactor > 0.75f)
                        contributionDesc = "valiant";
                    else if (contributionFactor > 0.5f)
                        contributionDesc = "stalwart";
                    else if (contributionFactor > 0.25f)
                        contributionDesc = "modest";
                    else
                        contributionDesc = "small";

                    plr?.SendClientMessage($"Your realm has captured {zoneName}, and you have been rewarded for your {contributionDesc} effort!", ChatLogFilters.CHATLOGFILTERS_RVR);

                    if (plr != null)
                    {
                        if (plr.Region == Region)
                        {
                            plr.AddXp(Math.Min(xpCap, (uint)(winXP * contributionFactor)), false, false);
                            if (plr.AAOBonus > 1)
                            {
                                plr.AddRenown(Math.Min(rpCap, (uint)(winRP * contributionFactor + (tokenCount *100))), false, RewardType.ZoneKeepCapture, zoneName);
                                if (plr.CurrentArea != null)
                                {
                                    ushort influenceId = realm == Realms.REALMS_REALM_ORDER ? (ushort)plr.CurrentArea.OrderInfluenceId : (ushort)plr.CurrentArea.DestroInfluenceId;
                                    plr.AddInfluence(influenceId, Math.Min(infCap, (ushort)(winInf * contributionFactor + (tokenCount * 100))));
                                }
                            }
                            else
                            {
                                plr.AddRenown(Math.Min(rpCap, (uint)(winRP * contributionFactor)), false, RewardType.ZoneKeepCapture, zoneName);
                                if (plr.CurrentArea != null)
                                {
                                    ushort influenceId = realm == Realms.REALMS_REALM_ORDER ? (ushort)plr.CurrentArea.OrderInfluenceId : (ushort)plr.CurrentArea.DestroInfluenceId;
                                    plr.AddInfluence(influenceId, Math.Min(infCap, (ushort)(winInf * contributionFactor)));
                                }
                            }

                        }

                        else
                        {
                            plr.AddPendingXP(Math.Min(xpCap, (uint)(winXP * contributionFactor)));
                            plr.AddPendingRenown(Math.Min(rpCap, (uint)(winRP * contributionFactor)));
                        }
                    }

                    else if (chara.Value != null)
                    {
                        chara.Value.PendingXp += Math.Min(xpCap, (uint)(winXP * contributionFactor));
                        chara.Value.PendingRenown += Math.Min(rpCap, (uint)(winRP * contributionFactor));
                        CharMgr.Database.SaveObject(chara.Value);
                    }

                    ushort resultantCount = (ushort)(winMedallionCount * contributionFactor);
                    /*
                    if (plr != null && plr.AAOBonus > 1 && plr.Region == Region)
                    {
                        resultantCount = Convert.ToUInt16(resultantCount * ((AgainstAllOddsMult * 20)/100));
                    }
                    */
                    if (contributionFactor > 0 && plr != null)
                    {
                        if ((Tier == 2 || Tier == 3) && plr.ItmInterface.CreateItem(T3Token, tokenCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { T3Token.Name, tokenCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (Tier == 4 && plr.ItmInterface.CreateItem(T4Token, tokenCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { T4Token.Name, tokenCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (Tier == 2 && plr.ItmInterface.CreateItem(conqMedallion, 1) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { conqMedallion.Name, 1.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (Tier == 3 && plr.ItmInterface.CreateItem(conqMedallion, 2) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { conqMedallion.Name, 2.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (Tier == 4 && plr.ItmInterface.CreateItem(conqMedallion, 3) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { conqMedallion.Name, 3.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                    }

                    if (resultantCount > 0)
                    {
                        if (plr != null && plr.Region == Region && plr.ItmInterface.CreateItem(medallionInfo, resultantCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { medallionInfo.Name, resultantCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }

                        else
                        {
                            Character_mail medallionMail = new Character_mail
                            {
                                Guid = CharMgr.GenerateMailGuid(),
                                CharacterId = chara.CharacterId,
                                CharacterIdSender = chara.CharacterId,
                                SenderName = chara.Name,
                                ReceiverName = chara.Name,
                                SendDate = (uint)TCPManager.GetTimeStamp(),
                                Title = "Medallion Reward",
                                Content = "You've received a medallion reward for your realm's victory in a recent battle in which you were a participant.",
                                Money = 0,
                                Opened = false
                            };
                            medallionMail.Items.Add(new MailItem(medallionInfo.Entry, resultantCount));
                            if (Tier == 2)
                            {
                                medallionMail.Items.Add(new MailItem(T3Token.Entry, tokenCount));
                                medallionMail.Items.Add(new MailItem(conqMedallion.Entry, 1));
                            }
                            if (Tier == 3)
                            {
                                medallionMail.Items.Add(new MailItem(T3Token.Entry, tokenCount));
                                medallionMail.Items.Add(new MailItem(conqMedallion.Entry, 2));
                            }
                            if (Tier == 4)
                            {
                                medallionMail.Items.Add(new MailItem(T4Token.Entry, tokenCount));
                                medallionMail.Items.Add(new MailItem(conqMedallion.Entry, 3));
                            }
                            CharMgr.AddMail(medallionMail);
                        }
                    }
                }

                else
                {
                    if (lossRP == 0)
                        continue;

                    float scaleFactor = Math.Min(1f, contrib.BaseContribution / (loserMaxContrib * 0.7f));

                    string contributionDesc;

                    if (scaleFactor > 0.75f)
                        contributionDesc = "valiant";
                    else if (scaleFactor > 0.5f)
                        contributionDesc = "stalwart";
                    else if (scaleFactor > 0.25f)
                        contributionDesc = "modest";
                    else
                        contributionDesc = "small";

                    plr?.SendClientMessage($"Though your realm lost {zoneName}, you have been rewarded for your {contributionDesc} effort in defense.", ChatLogFilters.CHATLOGFILTERS_RVR);

                    if (plr != null)
                    {
                        if (plr.Region == Region)
                        {
                            plr.AddXp((uint)Math.Min(xpCap * 0.9f, lossXP * scaleFactor), false, false);
                            if (plr.AAOBonus > 1)
                            {
                                plr.AddRenown((uint)Math.Min(rpCap * 0.9f, lossRP * scaleFactor + (tokenCount * 100)), false, RewardType.ObjectiveDefense, zoneName);
                                if (plr.CurrentArea != null)
                                {
                                    ushort influenceId = realm == Realms.REALMS_REALM_ORDER ? (ushort)plr.CurrentArea.OrderInfluenceId : (ushort)plr.CurrentArea.DestroInfluenceId;
                                    plr.AddInfluence(influenceId, (ushort)Math.Min(infCap * 0.9f, lossInf * scaleFactor + (tokenCount * 100)));
                                }
                            }
                            else
                            {
                                plr.AddRenown((uint)Math.Min(rpCap * 0.9f, lossRP * scaleFactor), false, RewardType.ObjectiveDefense, zoneName);
                                if (plr.CurrentArea != null)
                                {
                                    ushort influenceId = realm == Realms.REALMS_REALM_ORDER ? (ushort)plr.CurrentArea.OrderInfluenceId : (ushort)plr.CurrentArea.DestroInfluenceId;
                                    plr.AddInfluence(influenceId, (ushort)Math.Min(infCap * 0.9f, lossInf * scaleFactor));
                                }
                            }

                        }

                        else
                        {
                            plr.AddPendingXP((uint)Math.Min(xpCap * 0.9f, lossXP * scaleFactor));
                            plr.AddPendingRenown((uint)Math.Min(rpCap * 0.9f, lossRP * scaleFactor));
                        }
                    }

                    else if (chara.Value != null)
                    {
                        chara.Value.PendingXp += (uint)Math.Min(xpCap * 0.9f, lossXP * scaleFactor);
                        chara.Value.PendingRenown += (uint)Math.Min(rpCap * 0.9f, lossRP * scaleFactor);
                        CharMgr.Database.SaveObject(chara.Value);
                    }

                    ushort resultantCount = (ushort)(lossMedallionCount * scaleFactor);
                    /*
                    if (plr != null && plr.AAOBonus > 1 && plr.Region == Region)
                    {
                        resultantCount = Convert.ToUInt16(resultantCount * ((AgainstAllOddsMult * 20)/100));
                    }
                    */
                    if (scaleFactor > 0 && plr != null)
                    {
                        if ((Tier == 2 || Tier == 3) && plr.ItmInterface.CreateItem(T3Token, tokenCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { T3Token.Name, tokenCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (Tier == 4 && plr.ItmInterface.CreateItem(T4Token, tokenCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { T4Token.Name, tokenCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (Tier == 3 && plr.ItmInterface.CreateItem(conqMedallion, 1) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { conqMedallion.Name, 1.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        if (Tier == 4 && plr.ItmInterface.CreateItem(conqMedallion, 2) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { conqMedallion.Name, 2.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                    }
                    if (resultantCount > 0)
                    {
                        if (plr != null && plr.Region == Region && plr.ItmInterface.CreateItem(medallionInfo, resultantCount) == ItemResult.RESULT_OK)
                        {
                            plr.SendLocalizeString(new[] { medallionInfo.Name, resultantCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }
                        else
                        {
                            Character_mail medallionMail = new Character_mail
                            {
                                Guid = CharMgr.GenerateMailGuid(),
                                CharacterId = chara.CharacterId,
                                CharacterIdSender = chara.CharacterId,
                                SenderName = chara.Name,
                                ReceiverName = chara.Name,
                                SendDate = (uint)TCPManager.GetTimeStamp(),
                                Title = "Medallion Reward",
                                Content = "You've received a medallion reward for your participation in a recent battle.",
                                Money = 0,
                                Opened = false
                            };
                            medallionMail.Items.Add(new MailItem(medallionInfo.Entry, resultantCount));
                            if (Tier == 2)
                            {
                                medallionMail.Items.Add(new MailItem(T3Token.Entry, tokenCount));
                            }
                            if (Tier == 3)
                            {
                                medallionMail.Items.Add(new MailItem(T3Token.Entry, tokenCount));
                                medallionMail.Items.Add(new MailItem(conqMedallion.Entry, 1));
                            }
                            if (Tier == 4)
                            {
                                medallionMail.Items.Add(new MailItem(T4Token.Entry, tokenCount));
                                medallionMail.Items.Add(new MailItem(conqMedallion.Entry, 1));
                            }
                            CharMgr.AddMail(medallionMail);
                        }
                    }
                }
            }

            PlayerContributions.Clear();
        }

        /// <summary>
        /// Sets the pairing reopening timer for a Tier 2 or Tier 3 zone...?
        /// </summary>
        public void SetPairingUnlockTimer()
        {
            if (Constants.DoomsdaySwitch > 0)
                PairingUnlockTime = TCPManager.GetTimeStampMS() + (10 * 60 * 1000);
            else
                PairingUnlockTime = TCPManager.GetTimeStampMS() + (int)(15 * Math.Max((int)Tier, 2) * 60 * 1000 * TIMER_MODIFIER);

        }

        /// <summary>
        /// Returns the pairing to its open state, allowing interaction with objectives.
        /// </summary>
        public virtual void ResetPairing()
        {
            PairingLocked = false;
            GraceDisabled = false;

            LockingRealm = Realms.REALMS_REALM_NEUTRAL;
            DefendingRealm = Realms.REALMS_REALM_NEUTRAL;

            HeldObjectives[0] = 0;
            HeldObjectives[1] = 0;

            VictoryPoints = 50;
            LastAnnouncedVictoryPoints = 50;

            foreach (var flag in _Objectives)
                flag.UnlockObjective();

            foreach (Keep keep in _Keeps)
                keep.NotifyPairingUnlocked();

            Broadcast(Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name + " battlefield objectives are now open for capture!");

            _BattleFrontStatus.ActiveRegionOrZone = 1;
            _BattleFrontStatus.ControlingRealm = 0;
            WorldMgr.Database.SaveObject(_BattleFrontStatus);

            new ApocCommunications().SendCampaignStatus(null, null);

            UpdateStateOfTheRealm();
        }

#endregion

#region Keeps
        /// <summary>
        /// List of existing battlefield objectives within this BattleFront.
        /// </summary>
        /// <remarks>
        /// Must not be updated outside BattleFront implementations.
        /// </remarks>
        public IEnumerable<IBattleFrontFlag> Objectives
        {
            get
            {
                return _Objectives;
            }
        }

        /// <summary>
        /// List of existing keeps in BattleFront.
        /// </summary>
        /// <remarks>
        /// Must not be updated outside BattleFront implementations.
        /// </remarks>
        public List<Keep> Keeps
        {
            get
            {
                return _Keeps;
            }
        }

        public virtual float GetDoorRegenFactor(Realms team, ushort zone)
        {
            switch (HeldObjectives[(byte)team - 1])
            {
                case 0:
                    return 0;
                case 1:
                    return 0f;
                case 2:
                    return 1;
                case 3:
                    return 2;
                case 4:
                    return 3;
                default:
                    return 0;
            }
        }

        public virtual bool CanAttackDoor(Realms realm, ushort zoneId)
        {
            if (realm == Realms.REALMS_REALM_ORDER)
                return HeldObjectives[1] > 2;
            return HeldObjectives[0] > 2;
        }

#endregion

#region Contribution

        private const float RENOWN_CONTRIBUTION_FACTOR = 0.1f;
        private const int CONTRIB_ELAPSE_INTERVAL = 60 * 60; // 1 hour of no contribution forfeits.
        /// <summary>
        /// Used to compare BattleFronts within a tier, to catch out zone dodging nitwits.
        /// </summary>
        public ulong TotalContribFromRenown;
         
        protected readonly Dictionary<uint, ContributionInfo> PlayerContributions = new Dictionary<uint, ContributionInfo>();

        /// <summary>
        /// <para>Adds contribution for a player. This is based on renown earned and comes from 4 sources at the moment:</para>
        /// <para>- Killing players.</para>
        /// <para>- Objective personal capture rewards.</para>
        /// <para>- Objective defense tick rewards.</para>
        /// <para>- Destroying siege weapons.</para>
        /// </summary>
        public void AddContribution(Player plr, uint contribution)
        {
            if (!plr.ValidInTier(Tier, true))
            {
                if (plr.DebugMode)
                    plr.SendClientMessage("Not in the current tier.");
                return;
            }

            ContributionInfo contrib;

            if (!PlayerContributions.TryGetValue(plr.CharacterId, out contrib))
            {
                contrib = new ContributionInfo(plr);
                PlayerContributions.Add(plr.CharacterId, contrib);
            }

            float contributionScaler;

            // Better rewards depending on group organization status.
            if (plr.WorldGroup == null)
            {
                contributionScaler = 1f;
                contrib.ActiveTimeEnd = TCPManager.GetTimeStamp() + 90; // 1.5 minutes for solo
            }
            else
            {
                int memberCount = plr.WorldGroup.TotalMemberCount;
                contributionScaler = 1f + memberCount * 0.05f;
                contrib.ActiveTimeEnd = TCPManager.GetTimeStamp() + 90 + (10 * memberCount); // 4.5 minutes for full warband
            }

            uint contribGain = (uint) (contribution*RENOWN_CONTRIBUTION_FACTOR*contributionScaler);
            contrib.BaseContribution += contribGain;
            TotalContribFromRenown += contribGain;

            if (plr.DebugMode)
                plr.SendClientMessage($"Added {contribGain} contribution.");

#if BattleFront_DEBUG
            plr.SendClientMessage($"Contribution update: added {contribGain} - contribution active status end: {(contrib.ActiveTimeEnd - TCPManager.GetTimeStamp())}s from now.");
#endif
        }

        private readonly List<KeyValuePair<uint, ContributionInfo>> _toRemove = new List<KeyValuePair<uint, ContributionInfo>>(8);

        private void TickContribution(long curTimeSeconds)
        {
            foreach (KeyValuePair<uint, ContributionInfo> kV in PlayerContributions)
            {
                if (kV.Value.ActiveTimeEnd > curTimeSeconds)
                {
#if BattleFront_DEBUG
                    kV.Value.Player.SendClientMessage($"Contribution tick - {(uint)(125 * Tier * RENOWN_CONTRIBUTION_FACTOR)}");
#endif
                    Player targPlayer = Player.GetPlayer(kV.Value.PlayerCharId);
                    if (targPlayer != null)
                    {
                        if (targPlayer.DebugMode)
                            targPlayer.SendClientMessage($"Contribution tick - {125 * Tier * RENOWN_CONTRIBUTION_FACTOR}");
                        kV.Value.BaseContribution += (uint)(125 * Tier * RENOWN_CONTRIBUTION_FACTOR);
                    }

                }

                else if (curTimeSeconds - kV.Value.ActiveTimeEnd > CONTRIB_ELAPSE_INTERVAL)
                    _toRemove.Add(kV);
            }

            if (_toRemove.Count > 0)
            {
                Item_Info medallionInfo = ItemService.GetItem_Info((uint)(208399 + Tier));

                uint rpCap = (uint)(Tier * 7000);

                uint maxContrib = Math.Max(1, PlayerContributions.Values.Max(x => x.BaseContribution));

                foreach (var kVr in _toRemove)
                {
                    // Convert contribution to XP/RP based on current loss rates.
                    float contributionFactor = Math.Min(1f, kVr.Value.BaseContribution / (maxContrib * 0.7f));

                    uint rp = (uint)(Math.Min(rpCap, maxContrib * 1.5f * LoserShare * contributionFactor));
                    uint xp = rp * 5;
                    ushort medallionCount = (ushort)Math.Min(12, rp / (450 * Tier));

                    Player player = Player.GetPlayer(kVr.Key);

                    if (player != null)
                    {
                        player.SendClientMessage("You have received a reward for your contributions to a recent battle.", ChatLogFilters.CHATLOGFILTERS_RVR);

                        if (player.Region == Region)
                        {
                            player.AddXp(xp, false, false);
                            player.AddRenown(rp, false);

                            if (medallionCount > 0 && player.ItmInterface.CreateItem(medallionInfo, medallionCount) == ItemResult.RESULT_OK)
                                player.SendLocalizeString(new[] {medallionInfo.Name, medallionCount.ToString()}, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }

                        else
                        {
                            player.AddPendingXP(xp);
                            player.AddPendingRenown(rp);

                            if (medallionCount > 0)

                            {
                                Character_mail medallionMail = new Character_mail
                                {
                                    Guid = CharMgr.GenerateMailGuid(),
                                    CharacterId = player.CharacterId,
                                    CharacterIdSender = player.CharacterId,
                                    SenderName = player.Name,
                                    ReceiverName = player.Name,
                                    SendDate = (uint) TCPManager.GetTimeStamp(),
                                    Title = "Medallion Reward",
                                    Content = "You've received a medallion reward for your participation in a recent battle.",
                                    Money = 0,
                                    Opened = false
                                };
                                medallionMail.Items.Add(new MailItem(medallionInfo.Entry, medallionCount));
                                CharMgr.AddMail(medallionMail);
                            }
                        }
                    }

                    else
                    {
                        Character chara = CharMgr.GetCharacter(kVr.Key, false);

                        if (chara != null && chara.Value != null)
                        {
                            chara.Value.PendingXp += xp;
                            chara.Value.PendingRenown += rp;

                            CharMgr.Database.SaveObject(chara.Value);

                            if (medallionCount > 0)
                            {
                                Character_mail medallionMail = new Character_mail
                                {
                                    Guid = CharMgr.GenerateMailGuid(),
                                    CharacterId = chara.CharacterId,
                                    CharacterIdSender = chara.CharacterId,
                                    SenderName = chara.Name,
                                    ReceiverName = chara.Name,
                                    SendDate = (uint) TCPManager.GetTimeStamp(),
                                    Title = "Medallion Reward",
                                    Content = "You've received a medallion reward for your participation in a recent battle.",
                                    Money = 0,
                                    Opened = false
                                };
                                medallionMail.Items.Add(new MailItem(medallionInfo.Entry, medallionCount));
                                CharMgr.AddMail(medallionMail);
                            }
                        }
                    }

                    PlayerContributions.Remove(kVr.Key);
                }
                _toRemove.Clear();
            }
        }

        public Dictionary<uint, ContributionInfo> GetContributorsFromRealm(Realms realm)
        {
            Dictionary<uint, ContributionInfo> newDic = new Dictionary<uint, ContributionInfo>();

            foreach (var contrib in PlayerContributions.Values)
            {
                if (contrib.PlayerRealm == realm)
                    newDic.Add(contrib.PlayerCharId, contrib);
            }

            return newDic;
        }

        public List<ContributionInfo> GetContributionListFromRealm(Realms realm)
        {
            List<ContributionInfo> newList = new List<ContributionInfo>();

            foreach (var contrib in PlayerContributions.Values)
            {
                if (contrib.PlayerRealm == realm)
                    newList.Add(contrib);
            }

            return newList;
        }

        public void UpdateContributions()
        {
            long tick = TCPManager.GetTimeStampMS();

            RecalculatePopFactor();

            foreach (BattleFrontFlag flag in _Objectives)
            {
                if (flag._owningRealm != Realms.REALMS_REALM_NEUTRAL)
                    flag.TickDefense(tick / 1000);
            }

            TickContribution(tick / 1000);
        }

#endregion

#region Reward Splitting

        protected float WinnerShare = 1f;
        protected float LoserShare = 0.1f;

        /// <summary>
        /// A scaler for the reward of objectives captured in this BattleFront, based on its activity relative to other fronts of the same tier.
        /// </summary>
        public float RelativeActivityFactor { get; private set; } = 1f;

        /// <summary>
        /// 100 players max for consideration. Push 30% of reward per hour spent in zone = 0.5% per minute shift max.
        /// </summary>
        public void UpdateBattleFrontScalers()
        {
            int minPlayerCount = Math.Min(_orderInLake.Count, _destroInLake.Count);

            if (LoserShare < 0.6f)
                LoserShare += minPlayerCount * 0.01f * 0.015f;

            // Update comparative gain
            int index = Tier - 1;

            if (index < 0 || index > 3)
            {
                Log.Error("BattleFront", "Region "+Region.RegionId+" has BattleFront with tier index "+index);
                return;
            }

            ulong maxContribution = 1;

            foreach (BattleFront fnt in BattleFrontList.BattleFronts[index])
            {
                if (fnt.TotalContribFromRenown > maxContribution)
                    maxContribution = fnt.TotalContribFromRenown;
            }

            RelativeActivityFactor = TotalContribFromRenown / (float)maxContribution;

#if BattleFront_DEBUG
            foreach (Player player in Region.Players)
                player.SendClientMessage($"UpdateBattleFrontScalers: Relative scaler for this BattleFront: {RelativeActivityFactor} Winner VP: {WinnerShare} Loser VP: {LoserShare}");
#endif

            _nextVpUpdateTime = TCPManager.GetTimeStamp() + (int)(120 * TIMER_MODIFIER);

            foreach (Player player in Region.Players)
                new ApocCommunications().SendCampaignStatus(player, null);
        }

#endregion

#region Victory Points

        protected int VictoryPoints = 50;
        protected int LastAnnouncedVictoryPoints = 50;

        private long _nextVpUpdateTime;

        public virtual void UpdateVictoryPoints()
        {
            int delta = (HeldObjectives[0] - HeldObjectives[1]) / 2;

            int maxShift = 50 + (delta * 15);

            // This gives +1% VPs per 60s to Order if Order controls all 4 BOs
            if (HeldObjectives[0] == 4)
                delta += 1;

            // This gives +1% VPs per 60s to Destro if Destro controls all 4 BOs
            if (HeldObjectives[1] == 4)
                delta -= 1;

            if (_Keeps.Count(keep => keep.Realm == Realms.REALMS_REALM_ORDER && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED) == 2)
            { 
                maxShift = Math.Min(100, maxShift + 35);
                // If Order controls 2 Keeps we increase Order VPs by another +1%
                delta += 1;
            }

            else if (_Keeps.Count(keep => keep.Realm == Realms.REALMS_REALM_DESTRUCTION && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED) == 2)
            {
                maxShift = Math.Max(0, maxShift - 35);
                // If Destro controls 2 Keeps we increase Destro VPs by another +1%
                delta -= 1;
            }

#if DEBUG
            //delta *= 30;
#endif

            // Order has overall control
            if (delta > 0)
            {
                // If less than max shift, increase VP and check for lock / advancement of message
                if (VictoryPoints < maxShift)
                {
                    VictoryPoints = Math.Min(maxShift, VictoryPoints + delta);

                    if (VictoryPoints == 100)
                        LockPairing(Realms.REALMS_REALM_ORDER, true);

                    else if (VictoryPoints - LastAnnouncedVictoryPoints >= 15)
                    {
                        LastAnnouncedVictoryPoints += 15;
                        Broadcast("Order has gained " + LastAnnouncedVictoryPoints + "% control of " + ActiveZoneName + "!", Realms.REALMS_REALM_ORDER);
                    }
                }

                // Decay VP if present value is higher than maximum allowed
                else if (VictoryPoints > maxShift + 15)
                {
                    --VictoryPoints;

                    if (LastAnnouncedVictoryPoints - VictoryPoints >= 15)
                    {
                        LastAnnouncedVictoryPoints -= 15;
                        Broadcast("Destruction has gained " + (100 - LastAnnouncedVictoryPoints) + "% control of " + ActiveZoneName + "!", Realms.REALMS_REALM_DESTRUCTION);
                    }
                }
            }

            // Destruction has overall control
            else if (delta < 0)
            {
                if (VictoryPoints > maxShift)
                {
                    VictoryPoints = Math.Max(maxShift, VictoryPoints + delta);
                    if (VictoryPoints == 0)
                        LockPairing(Realms.REALMS_REALM_DESTRUCTION, true);

                    else if (LastAnnouncedVictoryPoints - VictoryPoints >= 15)
                    {
                        LastAnnouncedVictoryPoints -= 15;
                        Broadcast("Destruction has gained " + (100 - LastAnnouncedVictoryPoints) + "% control of " + ActiveZoneName + "!", Realms.REALMS_REALM_DESTRUCTION);
                    }
                }

                // Add Order VP if present value is lower than minimum currently allowed
                else if (VictoryPoints < maxShift - 15)
                {
                    ++VictoryPoints;

                    if (VictoryPoints - LastAnnouncedVictoryPoints >= 15)
                    {
                        LastAnnouncedVictoryPoints += 15;
                        Broadcast("Order has gained " + LastAnnouncedVictoryPoints + "% control of " + ActiveZoneName + "!", Realms.REALMS_REALM_ORDER);
                    }
                }
            }

            // Equal control, check VP within acceptable range
            else
            {
                if (VictoryPoints < maxShift - 15)
                {
                    ++VictoryPoints;

                    if (VictoryPoints - LastAnnouncedVictoryPoints >= 15)
                    {
                        LastAnnouncedVictoryPoints += 15;
                        Broadcast("Order has gained " + LastAnnouncedVictoryPoints + "% control of " + ActiveZoneName + "!", Realms.REALMS_REALM_ORDER);
                    }
                }

                else if (VictoryPoints > maxShift + 15)
                {
                    --VictoryPoints;

                    if (LastAnnouncedVictoryPoints - VictoryPoints >= 15)
                    {
                        LastAnnouncedVictoryPoints -= 15;
                        Broadcast("Destruction has gained " + (100 - LastAnnouncedVictoryPoints) + "% control of " + ActiveZoneName + "!", Realms.REALMS_REALM_DESTRUCTION);
                    }
                }
            }
        }

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
            if (realm == Realms.REALMS_REALM_ORDER)
                return VictoryPoints >= 40;
            return VictoryPoints <= 60;
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
            if (Tier == 4)
                return GetBaseResourceValue(realm) / resourceValueMax > 0.65f;

            return GetBaseResourceValue(realm) / resourceValueMax > 0.30f;
        }

#endregion

#region Resource

        /// <summary>
        /// Initially all BattleFronts are without a supply line.
        /// </summary>
        protected bool _NoSupplies = true;

        public float GetBaseResourceValue(Realms realm)
        {
            return Math.Max(_orderInLake.Count, _destroInLake.Count);
        }

        public float GetResourceValue(Realms realm, int maxValue)
        {
            float val = Point2D.Clamp(Math.Max(_orderInLake.Count, _destroInLake.Count) * GetRelativePopFactor(realm), maxValue * 0.2f, maxValue);

            // Resource value varies between 50% and 150% based on Victory Points
            if (realm == Realms.REALMS_REALM_ORDER)
                return val * (0.5f + VictoryPoints * 0.01f);

            return val * (0.5f + (100 - VictoryPoints) * 0.01f);
        }

        public bool NoSupplies
        {
            get
            {
                return _NoSupplies;
            }
        }

        public void ToggleSupplies()
        {
            _NoSupplies = !_NoSupplies;

            if (_NoSupplies)
            {
                foreach (var objective in _Objectives)
                    objective.BlockSupplySpawn();
            }

            else
            {
                foreach (var objective in _Objectives)
                {
                    if (objective._owningRealm != Realms.REALMS_REALM_NEUTRAL && (objective.FlagState == ObjectiveFlags.Open || objective.FlagState == ObjectiveFlags.Locked))
                        objective.StartSupplyRespawnTimer(SupplyEvent.ZoneActiveStatusChanged);
                }
            }
        }

        public virtual void EnableSupplies()
        {
            if (!_NoSupplies)
                return;

            if (HeldObjectives[0] + HeldObjectives[1] < 4)
            {
                Keep orderKeep = _Keeps.First(keep => keep.Realm == Realms.REALMS_REALM_ORDER);

                if (orderKeep == null)
                {
                    Log.Error("BattleFront", "Unable to find an open Order keep?");
                    return;
                }

                Keep destroKeep = _Keeps.First(keep => keep.Realm == Realms.REALMS_REALM_DESTRUCTION);

                if (destroKeep == null)
                {
                    Log.Error("BattleFront", "Unable to find an open Destruction keep?");
                    return;
                }

                while (HeldObjectives[0] < 2)
                {
                    BattleFrontFlag flag = GetClosestNeutralFlagTo(orderKeep.WorldPosition);
#if DEBUG
                    flag.OpenObjective(Realms.REALMS_REALM_ORDER, 30 * 1000);
#else
                    flag.OpenObjective(Realms.REALMS_REALM_ORDER, (int)(8 * 60 * 1000 * TIMER_MODIFIER));
#endif
                }


                while (HeldObjectives[1] < 2)
                {
                    BattleFrontFlag flag = GetClosestNeutralFlagTo(destroKeep.WorldPosition);
#if DEBUG
                    flag.OpenObjective(Realms.REALMS_REALM_DESTRUCTION, 30 * 1000);
#else
                    flag.OpenObjective(Realms.REALMS_REALM_DESTRUCTION, (int)(8 * 60 * 1000 * TIMER_MODIFIER));
#endif
                }
            }

            _NoSupplies = !_NoSupplies;

            foreach (var objective in _Objectives)
            {
                if (objective.FlagState == ObjectiveFlags.Open || objective.FlagState == ObjectiveFlags.Locked)
                {
                    objective.OpenObjective(objective._owningRealm, (int)(8 * 60 * 1000 * TIMER_MODIFIER));
                    objective.StartSupplyRespawnTimer(SupplyEvent.ZoneActiveStatusChanged);
                }
            }

            string message = "The forces of Order and Destruction direct their supply lines towards " + Zones[0].Name + " and " + Zones[1].Name + "!";

            Log.Info("Supplies", message);

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player.ValidInTier(Tier, true))
                    {
                        player.SendClientMessage(message, player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
                        player.SendClientMessage(message, ChatLogFilters.CHATLOGFILTERS_RVR);
                    }
                }
            }

            // WorldMgr.Database.ExecuteNonQuery("UPDATE war_world.BattleFront_status SET ActiveRegionOrZone = 0");
            _BattleFrontStatus.ActiveRegionOrZone = 1;
            WorldMgr.Database.SaveObject(_BattleFrontStatus);
            ActiveSupplyLine = 1;
        }

        public virtual void DisableSupplies()
        {
            if (_NoSupplies)
                return;

            _NoSupplies = true;

            foreach (var objective in _Objectives)
                objective.BlockSupplySpawn();
           
            Log.Info("Supplies", "Supply retraction from "+Zones[0].Name+" and "+Zones[1].Name);

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    if (player.ValidInTier(Tier, true))
                    {
                        if (player.CbtInterface.IsPvp)
                            player.SendClientMessage("The forces of Order and Destruction have pulled their supply lines out of " + Zones[0].Name + " and " + Zones[1].Name + "!", player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
                    }
                }
            }

            ActiveSupplyLine = 0;

            RemoveSiege();
        }

        public virtual void SupplyLineReset()
        {
            foreach (var obj in _Objectives)
            {
                if (obj.FlagState != ObjectiveFlags.ZoneLocked)
                    obj.OpenObjective(Realms.REALMS_REALM_NEUTRAL, (int)(5 * 60 * 1000));
            }

            HeldObjectives[0] = 0;
            HeldObjectives[1] = 0;

            GraceDisabled = false;

            UpdateStateOfTheRealm();
        }

#endregion

        protected void RemoveSiege()
        {
            List<Siege> siege = new List<Siege>();
            foreach (Keep keep in _Keeps)
                keep.AddAllSiege(siege);

            foreach (Siege s in siege)
                s.Destroy();
        }

#region Send

        /// <summary>
        /// Sends information to a player about the objectives within a BattleFront upon their entry.
        /// </summary>
        /// <param name="plr"></param>
        public void SendObjectives(Player plr)
        {
            foreach (BattleFrontFlag flag in _Objectives)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
                Out.WriteUInt32((uint)flag.ID);
                Out.Fill(0xFF, 6);
                Out.WriteUInt16(0);
                Out.WriteByte((byte)flag._owningRealm);
                Out.Fill(0, 3);
                plr.SendPacket(Out);
            }
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

        /// <summary>
        /// This is generating strings in Channel 9 that are read by State of the Realm addon modified by Sullemunk
        /// </summary>
        public void UpdateStateOfTheRealm()
        {
            string boStatus = "";
            string keepStatus = "";

            foreach (Zone_Info zone in Zones.ToList())
            {
                if (zone != null && !PairingLocked && (zone.Tier == 1 || zone.Tier == 2 || zone.Tier == 3 || zone.Tier == 4))
                {
                    boStatus = "SoR_T" + zone.Tier + "_BO:" + zone.ZoneId;

                    foreach (BattleFrontFlag flag in _Objectives.ToList())
                    {
                        if (flag != null)
                        {
                            long now = TCPManager.GetTimeStampMS();
                            long timer = (flag._stateTransitionTimer - TCPManager.GetTimeStampMS()) / 1000;

                            if (zone.Tier == 4)
                            { 
                                if (flag.ZoneId == zone.ZoneId)
                                {
                                    boStatus = boStatus + ":" + flag.ID + ":" + (int)flag._owningRealm + ":" + (int)flag.FlagState;
                                    now = TCPManager.GetTimeStampMS();
                                    timer = (flag._stateTransitionTimer - TCPManager.GetTimeStampMS()) / 1000;
                                    if (timer > 0)
                                        boStatus = boStatus + ":" + timer;
                                    else
                                        boStatus = boStatus + ":0";
                                    
                                        
                                }
                            }
                            else
                            {
                                boStatus = boStatus + ":" + flag.ID + ":" + (int)flag._owningRealm + ":" + (int)flag.FlagState;
                                now = TCPManager.GetTimeStampMS();
                                timer = (flag._stateTransitionTimer - TCPManager.GetTimeStampMS()) / 1000;
                                if (timer > 0)
                                    boStatus = boStatus + ":" + timer;
                                else
                                    boStatus = boStatus + ":0";
                            }
                            
                        }
                    }

                    boStatus = boStatus + ":" + VictoryPoints + ":" + ActiveSupplyLine;

                    if (zone.Tier == 4)
                        boStatus = boStatus + ":" + _BattleFrontStatus.OpenZoneIndex;

                    keepStatus = "SoR_T" + zone.Tier + "_Keep:" + zone.ZoneId;

                    foreach (Keep keep in _Keeps.ToList())
                    {
                        if (keep != null)
                        {
                            if (zone.Tier == 4)
                            {
                                if (keep.ZoneId == zone.ZoneId)
                                    keepStatus = keepStatus + ":" + keep.Info.KeepId + ":" + (int)keep.Realm + ":" + keep.Rank + ":" + (int)keep.KeepStatus + ":" + (int)keep.LastMessage;
                            }
                            else
                            {
                                keepStatus = keepStatus + ":" + keep.Info.KeepId + ":" + (int)keep.Realm + ":" + keep.Rank + ":" + (int)keep.KeepStatus + ":" + (int)keep.LastMessage;
                            }
                        }
                    }
                }

                if (boStatus != "" && Tier == 4)
                {
                    keepStatus = keepStatus + ":" + _BattleFrontStatus.OpenZoneIndex;
                    foreach (Player plr in Player._Players.ToList())
                    {
                        if (plr != null && plr.SoREnabled)
                        {
                            plr.SendLocalizeString(boStatus, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
                            plr.SendLocalizeString(keepStatus, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
                        }
                    }
                }
            }

            if (boStatus != "" && Tier != 4)
            {
                foreach (Player plr in Player._Players.ToList())
                {
                    if (plr != null && plr.SoREnabled)
                    {
                        plr.SendLocalizeString(boStatus, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
                        plr.SendLocalizeString(keepStatus, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
                    }
                }
            }
        }

        public virtual void WriteBattleFrontStatus(PacketOut Out)
        {
            throw new InvalidOperationException("Only valid for a T4 BattleFront.");
        }

        public void WriteCaptureStatus(PacketOut Out)
        {
            Out.WriteByte(0);

            if (Tier == 1)
            {
                Out.WriteByte((byte)this.GetCapturedPct(1));
                Out.WriteByte((byte)this.GetCapturedPct(2));
            }

            else
            {
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
        }

#endregion

#region Diagnostic

        /// <summary>
        /// Sends debug information about this BattleFront to the requesting player.
        /// </summary>
        /// <param name="plr"></param>
        public virtual void CampaignDiagnostic(Player plr, bool localZone)
        {
            plr.SendClientMessage("***** Campaign Status : Region " + Region.RegionId + " *****", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

            plr.SendClientMessage("The pairing is " + (PairingLocked ? "locked for " + ((PairingUnlockTime - TCPManager.GetTimeStampMS()) / 60000) +" more minutes." : "contested."));
            plr.SendClientMessage("Order objectives controlled: " + HeldObjectives[0]);
            plr.SendClientMessage("Destruction objectives controlled: " + HeldObjectives[1]);
            plr.SendClientMessage("Ration factors:  Order " + RationFactor[0]+", Destruction "+ RationFactor[1]);
            plr.SendClientMessage("GraceDisabled: " + Convert.ToString(GraceDisabled));

            foreach (var keep in _Keeps)
            {
                keep.SendDiagnostic(plr);
            }

            foreach (var objective in _Objectives)
            {
                objective.SendDiagnostic(plr);
            }

            int arr;
            if (Constants.DoomsdaySwitch == 2)
                arr = (int)pairing;
            else
                arr = Tier;

            BattleFront activeFront = BattleFrontList.ActiveFronts[arr - 1] as BattleFront;
            plr.SendClientMessage("Currently active BattleFront: " + (activeFront != null ? activeFront.Zones[0].Name : "None"));
        }

#endregion
    }
}
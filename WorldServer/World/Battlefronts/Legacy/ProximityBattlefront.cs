#define BattleFront_DEBUG

#define BattleFront_FLAG_GUARDS

//#if !DEBUG && BattleFront_DEBUG
//#error BattleFront DEBUG ENABLED IN RELEASE BUILD
//#endif

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

namespace WorldServer
{
    /// <summary>
    /// Responsible for tracking the RvR campaign's progress along a single front of battle.
    /// </summary>
    public class ProximityBattleFront : IBattleFront
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        #region Load

        public string Name;

        private readonly BattleFrontStatus _BattleFrontStatus;

        /// <summary>
        /// This is used to modify the timer of VP Update - default from Aza system was 120 seconds, 
        /// so TIMER_MODIFIER should be set to 1.0f, currently we are cutting it by half, change it
        /// back to 1.0f to restore default value
        /// </summary>
        private const float TIMER_MODIFIER = 0.5f;

        /// <summary>
        /// A list of battlefield objectives within this BattleFront.
        /// </summary>
        public List<ProximityFlag> _Objectives = new List<ProximityFlag>();

        /// <summary>
        /// A list of keeps within this BattleFront.
        /// </summary>
        public List<Keep> _Keeps = new List<Keep>();

        /// <summary>
        /// A list of zones within the scope of this BattleFront.
        /// </summary>
        public List<Zone_Info> Zones;
        /// <summary>
        /// The associated region managed by this BattleFront.
        /// </summary>
        public RegionMgr Region { get; }
        /// <summary>
        /// The tier within which this BattleFront exists.
        /// </summary>
        public readonly byte Tier;
        /// <summary>
        /// This is variable that stores current active supply line
        /// </summary>
        public int ActiveSupplyLine = 0;
        /// <summary>
        /// This is pairing
        /// </summary>
        public Pairing pairing;
        /// <summary>
        /// This will prevent rewards for winners if population was too low
        /// </summary>
        public bool DefenderPopTooSmall = false;

        public EventInterface EvtInterface { get; protected set; } = new EventInterface();

        public ProximityBattleFront(RegionMgr region, bool oRvRFront)
        {
            _BattleFrontStatus = BattleFrontService.GetStatusFor(region.RegionId);

            Region = region;
            Region.Bttlfront = this;

            Tier = (byte)Region.GetTier();

            _logger.Debug($" ProximityBattleFront Load region={region.RegionId}  RVRFront?={oRvRFront} ActiveRealmZone={_BattleFrontStatus.ActiveRegionOrZone} Controlling Realm={_BattleFrontStatus.ControlingRealm} Tier={Tier}");
   
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
        public void LoadMidTierPairing(bool populationOpener = false)
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
            RealmRank[0] = 0;
            RealmRank[1] = 0;
            RealmCurrentResources[0] = 0;
            RealmCurrentResources[1] = 0;
            RealmMaxResource[0] = 0;
            RealmMaxResource[1] = 0;
            RealmLastReturnSeconds[0] = 0;
            RealmLastReturnSeconds[1] = 0;
            PairingDrawTime = 0;
            DefenderPopTooSmall = false;
            _totalMaxOrder = 0;
            _totalMaxDestro = 0;
            RealmLostKeep[0] = false;
            RealmLostKeep[1] = false;
            RealmDeployedRam[0] = 0;
            RealmDeployedRam[1] = 0;
            RealmCannon[0] = 0;
            RealmCannon[1] = 0;

            if (Tier == 2 && (long)pairing == WorldMgr.StartingPairing)
            {
                ResetPairing();
            }
            else if (populationOpener)
            {
                ResetPairing();
            }
            else if (Tier > 1)
                LockPairing(Realms.REALMS_REALM_NEUTRAL, false, true);

            LockingRealm = Realms.REALMS_REALM_NEUTRAL;
            DefendingRealm = Realms.REALMS_REALM_NEUTRAL;

            WorldMgr.SendCampaignStatus(null);
        }

        public void MinuteUpdate()
        {
            UpdateContributions();
            UpdatePopulationDistribution();

            if (Tier > 1)
            {
                if (!PairingLocked && !_NoSupplies)
                {
                    UpdateVictoryPoints();
                    CheckUnlockZone();
                }
                //UpdateRationing();

            }

            if (Tier == 2 || Tier == 3 || Tier == 4)
                CheckNewLockCondition();

            // This is used by State of the Realm addon
            UpdateStateOfTheRealm();
        }

        private byte CountOwnedKeeps(Realms realm)
        {
            byte i = 0;
            ProximityBattleFront bttlfrnt = this;
            foreach (Keep keep in Region.Bttlfront.Keeps)
            {
                if (keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                {
                    if (Tier > 1 && Tier < 4)
                    {
                        if (keep.Realm == realm)
                            i++;
                    }
                    else if (Tier == 4)
                    {
                        ProximityProgressingBattleFront progBttlfrnt = (ProximityProgressingBattleFront)bttlfrnt;
                        if (keep.ZoneId == progBttlfrnt.Zones[progBttlfrnt._BattleFrontStatus.OpenZoneIndex].ZoneId && keep.Realm == realm)
                                i++;
                    }
                }
            }

            return i;
        }

        void CheckNewLockCondition()
        {
            // New locking mechanics
            if (Tier > 1)
            {
                //ProximityBattleFront bttlfrnt = (ProximityBattleFront)Region.Bttlfront;
                ProximityBattleFront bttlfrnt = this;
                if (bttlfrnt != null && !bttlfrnt.PairingLocked)
                {
                    foreach (Keep keep in Region.Bttlfront.Keeps)
                    {
                        if (keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                        {
                            bttlfrnt.CountRealmObjectives();

                            if (keep.Realm == Realms.REALMS_REALM_ORDER)
                            {
                                if (Tier == 4)
                                {
                                    ProximityProgressingBattleFront progBttlfrnt = (ProximityProgressingBattleFront)bttlfrnt;
                                    progBttlfrnt.CountRealmObjectives();

                                    // This is 5 star win, reduced rewards
                                    if (!keep.Ruin && RealmRank[0] > 4 && bttlfrnt.HeldObjectives[1] > 3 && _againstAllOddsMult > -4 && _againstAllOddsMult < 4)
                                    { 
                                        progBttlfrnt.LockZone(Realms.REALMS_REALM_ORDER, progBttlfrnt.Zones[progBttlfrnt._BattleFrontStatus.OpenZoneIndex].ZoneId, true, false);
                                        return;
                                    }

                                    // This is win after we take keep, full rewards
                                    if (keep.Ruin && CountOwnedKeeps(Realms.REALMS_REALM_ORDER) > 1 && bttlfrnt.HeldObjectives[1] > bttlfrnt._Objectives.Count(z => z.ZoneId == keep.ZoneId) - 2)
                                    { 
                                        progBttlfrnt.LockZone(Realms.REALMS_REALM_ORDER, progBttlfrnt.Zones[progBttlfrnt._BattleFrontStatus.OpenZoneIndex].ZoneId, true, false);
                                        return;
                                    }
                                }
                                else
                                {
                                    // This is 5 star win, reduced rewards
                                    if (!keep.Ruin && RealmRank[0] > 4 && bttlfrnt.HeldObjectives[1] > 3 && _againstAllOddsMult > -4 && _againstAllOddsMult < 4)
                                    { 
                                        bttlfrnt.LockPairing(Realms.REALMS_REALM_ORDER, true, false, false, false);
                                        return;
                                    }

                                    // This is win after we take keep, full rewards
                                    if (keep.Ruin && CountOwnedKeeps(Realms.REALMS_REALM_ORDER) > 1 && bttlfrnt.HeldObjectives[1] > bttlfrnt._Objectives.Count() - 2)
                                    {
                                        bttlfrnt.LockPairing(Realms.REALMS_REALM_ORDER, true, false, false, false);
                                        return;
                                    }
                                }
                            }

                            if (keep.Realm == Realms.REALMS_REALM_DESTRUCTION)
                            {
                                if (Tier == 4)
                                {
                                    ProximityProgressingBattleFront progBttlfrnt = (ProximityProgressingBattleFront)bttlfrnt;
                                    progBttlfrnt.CountRealmObjectives();

                                    // This is 5 star win, reduced rewards
                                    if ((!keep.Ruin && RealmRank[1] > 4 && bttlfrnt.HeldObjectives[2] > 3 && _againstAllOddsMult > -4 && _againstAllOddsMult < 4))
                                    {
                                        progBttlfrnt.LockZone(Realms.REALMS_REALM_DESTRUCTION, progBttlfrnt.Zones[progBttlfrnt._BattleFrontStatus.OpenZoneIndex].ZoneId, true, false);
                                        return;
                                    }

                                    // This is win after we take keep, full rewards
                                    if (keep.Ruin && CountOwnedKeeps(Realms.REALMS_REALM_DESTRUCTION) > 1 && bttlfrnt.HeldObjectives[2] > bttlfrnt._Objectives.Count(z => z.ZoneId == keep.ZoneId) - 2)
                                    {
                                        progBttlfrnt.LockZone(Realms.REALMS_REALM_DESTRUCTION, progBttlfrnt.Zones[progBttlfrnt._BattleFrontStatus.OpenZoneIndex].ZoneId, true, false);
                                        return;
                                    }
                                }
                                else
                                {
                                    // This is 5 star win, reduced rewards
                                    if (!keep.Ruin && RealmRank[1] > 4 && bttlfrnt.HeldObjectives[2] > 3 && _againstAllOddsMult > -4 && _againstAllOddsMult < 4)
                                    {
                                        bttlfrnt.LockPairing(Realms.REALMS_REALM_DESTRUCTION, true, false, false, false);
                                        return;
                                    }

                                    // This is win after we take keep, full rewards
                                    if (keep.Ruin && CountOwnedKeeps(Realms.REALMS_REALM_DESTRUCTION) > 1 && bttlfrnt.HeldObjectives[2] > bttlfrnt._Objectives.Count() - 2)
                                    {
                                        bttlfrnt.LockPairing(Realms.REALMS_REALM_DESTRUCTION, true, false, false, false);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void LoadObjectives()
        {
            List<BattleFront_Objective> objectives = BattleFrontService.GetBattleFrontObjectives(Region.RegionId);

            _logger.Debug($"Objective Count={objectives.Count} RegionId={Region.RegionId}");

           
            if (objectives == null)
                return;

            float orderDistanceSum = 0f;
            float destroDistanceSum = 0f;

            foreach (BattleFront_Objective BattleFrontObjective in objectives)
            {
                ProximityFlag flag = new ProximityFlag(BattleFrontObjective, this, Region, Tier);
                _Objectives.Add(flag);
                Region.AddObject(flag, BattleFrontObjective.ZoneId);

                orderDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_ORDER);
                destroDistanceSum += flag.GetWarcampDistance(Realms.REALMS_REALM_DESTRUCTION);

                _logger.Debug(
                    $"...Name={BattleFrontObjective.Name} Entry={BattleFrontObjective.Entry} ZoneId={BattleFrontObjective.ZoneId} \n " +
                    $"\t\t FlagState={flag.FlagState} State={flag.State} Threat={flag.HasThreateningPlayer} Locked={flag.IsLocked} \n " +
                    $"\t\t ProxFlag:{flag.ToString()} \n " +
                    $"\t\t XYZO={BattleFrontObjective.X},{BattleFrontObjective.Y},{BattleFrontObjective.Z},{BattleFrontObjective.O}");
                
            }

            // Need to be correctly set
            foreach (ProximityFlag flag in _Objectives)
                flag.SetWarcampDistanceScaler(1, 1);
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

        public readonly List<Player> _orderInLake = new List<Player>();
        public readonly List<Player> _destroInLake = new List<Player>();

        public int _totalMaxOrder = 0;
        public int _totalMaxDestro = 0;

        private readonly List<NewBuff> _orderAAOBuffs = new List<NewBuff>();
        private readonly List<NewBuff> _destroAAOBuffs = new List<NewBuff>();

        public int _againstAllOddsMult { get; private set; }

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
                ProximityFlag flag = (ProximityFlag)GetClosestFlag(player.WorldPosition, true);

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

            _logger.Debug($"Closest Flag ={bestFlag}");

            return bestFlag;
        }

        public ProximityFlag GetClosestNeutralFlagTo(Point3D destPos)
        {
            ProximityFlag bestFlag = null;
            ulong bestDist = 0;

            foreach (ProximityFlag flag in _Objectives)
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
            _logger.Debug($"Closest Flag ={bestFlag}");
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

            foreach (var obj in _Objectives)
            {
                if (obj.FlagState != ObjectiveFlags.ZoneLocked)
                {
                    obj.AdvancePopHistory(orderCount, destroCount);
                    _logger.Debug($"AdvancePopHistory Order={orderCount} DestCount={destroCount}");
                }
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

            int zmienna1 = _popHistory[(int)capturingRealm - 1].Count;
            int zmienna2 = Math.Max(6, _popHistory[(int)capturingRealm - 1].Max() / 5);
            float zmienna3 = scaleMult *= maxRewardPlayers / (float)playerCount;

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
        public float GetRationFactor(Unit unit)
        {
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

//        public void UpdateRationing()
//        {
//            int[] popHigh = { _popHistory[0].Max(), _popHistory[1].Max() };

//            // i == 0 -> Order; i == 1 --> Destro
//            for (int i = 0; i < 2; ++i)
//            {
//                Keep currentKeep = _Keeps.Find(keep => (int)keep.Realm == i + 1 && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && keep.KeepStatus != KeepStatus.KEEPSTATUS_SEIZED);

//                if (currentKeep == null)
//                {
//                    RemoveAllRationed();
//                    return;
//                }

//#if BattleFront_DEBUG
//                foreach (Player plr in Region.Players)
//                    plr.SendClientMessage("Found keep "+currentKeep.Info.Name+" for "+i);
//#endif

//                List<Player> curList = (i == 0 ? _orderInLake : _destroInLake);

//                lock (curList)
//                {
//                    foreach (Player plr in curList)
//                    {
//                        bool isInKeepRange = plr.IsWithinRadiusFeet(currentKeep, 600);
//                        if (_withinKeepRange[i].Contains(plr))
//                        {
//                            if (!isInKeepRange)
//                                RemoveRationed(plr, i);
//                        }
//                        else if (isInKeepRange)
//                        {
//                            AddRationed(plr, i);
//                        }
//                    }
//                }

//                int rationedCount = _withinKeepRange[i].Count;

//                // The keep's supply cap is the population high of the enemy realm over the last 15 minutes.
//                _supplyCaps[i] = (int)Math.Max(SUPPLY_BASE_SUPPORT, popHigh[1 - i] * currentKeep.GetRationFactor());

//                if (rationedCount <= _supplyCaps[i])
//                {
//                    // The realm is within ration tolerance.
//                    // Return if not penalized.
//                    if (RationFactor[i] == 1f)
//                        continue;

//                    //Remove the penalty.
//                    RationFactor[i] = 1f;

//                    lock (_rationDebuffs[i])
//                    {
//                        foreach (RationBuff b in _rationDebuffs[i])
//                            b.BuffHasExpired = true;

//                        _rationDebuffs[i].Clear();
//                    }
//                }

//                else
//                {
//                    // There are more members within the keep than its supplies can allow for.
//                    float newRationFactor = (int)((rationedCount * 5) / (float)_supplyCaps[i]);
//                    newRationFactor *= 0.2f;

//                    if (newRationFactor > 1f)
//                    {
//                        newRationFactor -= 1f;
//                        newRationFactor *= 0.35f;
//                        newRationFactor += 1f;
//                    }

//#if BattleFront_DEBUG
//                    foreach (Player player in Region.Players)
//                        player.SendClientMessage($"Rationing: {(i == 0 ? "Order" : "Destruction")} penalty ({newRationFactor}).");
//#endif

//                    if (newRationFactor == RationFactor[i])
//                        continue;

//                    if (RationFactor[i] == 1f)
//                    {
//                        lock (_withinKeepRange)
//                            foreach (Player player in _withinKeepRange[i])
//                                player.BuffInterface.QueueBuff(new BuffQueueInfo(player, 1, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Rationing), RationBuff.CreateRationBuff, AssignRationDebuff));
//                    }

//                    else
//                    {
//                        // Update existing ration debuffs
//                        lock (_rationDebuffs[i])
//                        {
//                            foreach (RationBuff debuff in _rationDebuffs[i])
//                                debuff.PendingDebuffFactor = newRationFactor;
//                        }
//                    }

//                    RationFactor[i] = newRationFactor;
//                }
//            }
//        }

        public void AssignRationDebuff(NewBuff rationDebuff)
        {
            if (rationDebuff == null)
                return;

            int realmIndex = (int)rationDebuff.Caster.Realm - 1;

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

            foreach (ProximityFlag flag in _Objectives)
            {
                if ((byte)flag._owningRealm == team)
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
                    return team == (byte)LockingRealm ? 100 : 0;
                return (int)((float)captured / _Objectives.Count * 99);
            }

            return keepcaptured * 50;
        }

        #endregion

        #region Capture Events

        /// <summary>
        /// The number of held battlefield objectives for each realm within the active zone in the BattleFront.
        /// </summary>
        public readonly byte[] HeldObjectives = new byte[3];

        private Realms _dominatingRealm = Realms.REALMS_REALM_NEUTRAL;

        public virtual void ObjectiveCaptured(Realms oldRealm, Realms newRealm, ushort zoneId)
        {
            _logger.Debug($"Objective Captured! ZoneId={zoneId}");
            CountRealmObjectives();

            // Enable supplies on first BattleFront to have 1 objectives captured
            if (Tier != 1 && ActiveSupplyLine == 0 && !PairingLocked)
            {   // Look here before push, it was == 2 before
                if (HeldObjectives[1] + HeldObjectives[2] > 0)
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
                                        foreach (ProximityBattleFront b in BattleFrontList.BattleFronts[Tier - 1])
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
                }
            }

            CountRealmObjectives();

            UpdateStateOfTheRealm();
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

            ProximityFlag closestFlag = (ProximityFlag)GetClosestFlag(killed.WorldPosition);

            if (closestFlag != null)
            {
                closestFlag.AccumulatedKills++;

                // Defense kill. Weight the kill higher depending on the distance from the opposing objective (proactive defending)
                if (killer.Realm == closestFlag._owningRealm)
                    rewardMod += Math.Min(killed.GetDistanceTo(closestFlag), 1000) * 0.001f * 0.5f;
                // Attack kill. Weight the kill higher if it was closer to the objective (high penetration)
                else
                    rewardMod += (1000 - Math.Min(killed.GetDistanceTo(closestFlag), 1000)) * 0.001f * 0.5f;
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
            return 0.8f + HeldObjectives[(int)realm] * 0.1f;
        }

        /// <summary>
        /// Gets a factor to applies on attackers lock rewards.
        /// </summary>
        /// <param name="realm">Attacking realm</param>
        /// <returns>Positive factor</returns>
        public float GetAttackBias(Realms realm)
        {
            return 0.8f + ((4 - HeldObjectives[(int)realm]) * 0.1f);
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

        /// <summary>
        /// The time that will cause the zone to draw
        /// </summary>
        public long PairingDrawTime = 0;

        /// <summary>
        /// The global rank of your Realm - it is the same as Keep Rank, until the Keep is ruined
        /// </summary>
        public int[] RealmRank = new int[2];

        public float[] RealmCurrentResources = new float[2];
        public int[] RealmMaxResource = new int[2];
        public long[] RealmLastReturnSeconds = new long[2];
        
        public readonly int[] _RealmResourcePerRank = { 3, 5, 7, 9, 12, 14 };
        public readonly int[] _RealmResourceValueMax = { 12, 24, 48, 72, 108, 144 };

        private readonly int[] _RealmRankDecayTimer = { 60, 60, 60, 60, 60, 40 };
        public bool[] RealmLostKeep = new bool[2];
        public int[] RealmDeployedRam = new int [2];
        public int[] RealmCannon = new int[2];
        //public int RealmRankOrder = 0, RealmRankDestro = 0;

        public Realms LockingRealm { get; protected set; } = Realms.REALMS_REALM_NEUTRAL;

        public void TickRealmRankTimer()
        {
            for (int i = 0; i < 2; i++)
            {
                if (RealmRank[i] == 0 && RealmCurrentResources[i] == 0)
                    return;

                int curTime = TCPManager.GetTimeStamp();

                // Sustain keep if resources were returned within last 10 minutes and enough players exist to support the rank
                ProximityBattleFront front = (ProximityBattleFront)Region.Bttlfront;
                if (RealmLastReturnSeconds[i] + _RealmRankDecayTimer[RealmRank[i]] > curTime && CanRealmSustainRank((Realms)(i+1), RealmRank[i]) && front.HeldObjectives[i+1] > WorldMgr.WorldSettingsMgr.GetGenericSetting(9))
                    return;

                // Degeneration for failing to supply keep or for failing numbers threshold.
                // Loss of 10% of rank per minute -> rank loss every 10 minutes
                float rankLoss = RealmMaxResource[i] * 0.1f;
                // Changed to 50% per minute
                rankLoss = RealmMaxResource[i] * (float)WorldMgr.WorldSettingsMgr.GetGenericSetting(8) / 10.0f;
                if (rankLoss > RealmCurrentResources[i])
                {
                    // Keeps and Realms Rank 3 or less do not derank
                    if (RealmRank[i] < 4)
                        RealmCurrentResources[i] = 0;
                    else
                    {
                        --RealmRank[i];
                        SetRealmSupplyRequirement(i);
                        RealmCurrentResources[i] = RealmMaxResource[i] * 0.95f;
                        Region.Bttlfront.Broadcast($"{ActiveZoneName}'s rank has fallen to {RealmRank[i]}!", (Realms)(i+1));
                    }
                }

                else
                    RealmCurrentResources[i] -= rankLoss;
            }
        }

        public void SetRealmSupplyRequirement(int realm)
        {
            RealmMaxResource[realm] = _RealmResourcePerRank[RealmRank[realm]] * _RealmResourceValueMax[RealmRank[realm]];
        }


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
            foreach (ProximityFlag flag in _Objectives.ToList())
            {
                if (flag != null && flag.Ruin)
                {
                    _Objectives.Remove(flag);
                    flag.RemoveFromWorld();
                }
            }

            foreach (ProximityFlag flag in _Objectives)
                flag.LockObjective(realm, announce);

            foreach (Keep keep in _Keeps)
            {
                Dictionary<uint, ContributionInfo> contributors = GetContributorsFromRealm((Realms)keep.Info.Realm);

                if (contributors.Count > 0 && !noRewards)
                {
                    if (DefenderPopTooSmall)
                        WinnerShare = 0.0f;

                    if (draw)
                    {
                        WinnerShare = 0.1f;
                        LoserShare = 0.1f;
                    }

                    Log.Success("Logging keep rewards...", "");
                    Log.Success("Zone", ActiveZoneName);
                    Log.Success("Is defender pop too small to award rewards", DefenderPopTooSmall.ToString());
                    Log.Success("BattleFront", $"Creating gold chest for {keep.Info.Name} for {contributors.Count} {((Realms)keep.Info.Realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction")} contributors");
                    GoldChest.Create(Region, keep.Info.PQuest, ref contributors, (Realms)keep.Info.Realm == realm ? WinnerShare : LoserShare);
                }

                keep.LockKeep(realm, announce, false);
            }

            if (DefenderPopTooSmall)
                Broadcast($"The forces of {(realm == Realms.REALMS_REALM_ORDER ? "Order " : "Destruction ")} conquered abandoned keep, no spoils of war were found!");

            if (draw)
                Broadcast("As forces of Order and Destruction were reluctant to trade final blows the war moved elsewhere!");

            UpdateStateOfTheRealm();

            PairingLocked = true;

#if DEBUG
            EvtInterface.AddEvent(EndGrace, 90 * 1000, 1);
#else
            EvtInterface.AddEvent(EndGrace, 10 * 60 * 1000, 1);
#endif

            LockingRealm = realm;

            WorldMgr.SendCampaignStatus(null);

            // DoomsDay change - we want to unlock stuff only after we locked something
            if (Constants.DoomsdaySwitch == 0)
                SetPairingUnlockTimer();

            try
            {
                string message = Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name + " have been locked by " + (realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction") + "!";
                //Log.Info("Zone Lock", "Locking "+Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name);
                if (!noRewards)
                    HandleLockReward(realm, 1, message, 0);
            }
            catch (Exception e)
            {
                Log.Error("HandleLockReward", "Exception thrown: " + e);
            }

            PlayerContributions.Clear();
            PlayerContributions = new Dictionary<uint, ContributionInfo>();

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
                foreach (ProximityBattleFront b in BattleFrontList.BattleFronts[Tier - 1])
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

                    CheckUnlockZone(true);
                }
            }

            DefenderPopTooSmall = false;
            _totalMaxOrder = 0;
            _totalMaxDestro = 0;

            RealmRank[0] = 0;
            RealmRank[1] = 0;
            RealmCurrentResources[0] = 0;
            RealmCurrentResources[1] = 0;
            RealmMaxResource[0] = 0;
            RealmMaxResource[1] = 0;
            RealmLastReturnSeconds[0] = 0;
            RealmLastReturnSeconds[1] = 0;
            RealmLostKeep[0] = false;
            RealmLostKeep[1] = false;
            RealmDeployedRam[0] = 0;
            RealmDeployedRam[1] = 0;
            RealmCannon[0] = 0;
            RealmCannon[1] = 0;
        }

        public Random random = new Random();

        /// <summary>
        /// This function checks if population is big enough to open new zone
        /// </summary>
        public void CheckUnlockZone(bool zoneLock = false)
        {
            int maxOpenZones = 1;
            int currentOpenZones = 0;

            List<Player> players;

            lock (Player._Players)
            {
                players = Player._Players.ToList();
            }
            int totalRvRPlayers = players.Where(p => p.CbtInterface.IsPvp == true && p.Level > 15 && p.ScnInterface.Scenario == null).Count();

            if (totalRvRPlayers < 201)
                maxOpenZones = 1;
            else if (totalRvRPlayers > 200 && totalRvRPlayers < 401)
                maxOpenZones = 2;
            else
                maxOpenZones = 3;

            for (int i = 0; i < 4; i++)
            {
                foreach (IBattleFront b in BattleFrontList.BattleFronts[i])
                {
                    ProximityBattleFront front = b as ProximityBattleFront;
                    if (front != null && !front.PairingLocked && front.Tier > 1)
                        currentOpenZones++;
                }
            }
            if (zoneLock)
            { 
                if (currentOpenZones == 0 || currentOpenZones < maxOpenZones)
                    UnlockRegion();
                else
                    Broadcast("Not enough warriors to support offensive in new region!");
            }
            else
            {
                if (currentOpenZones < maxOpenZones)
                {
                    int dwarfPairingsOpen = 0; // 1
                    int empirePairingsOpen = 0; // 2
                    int elfPairingsOpen = 0; // 3

                    for (int i = 1; i < 4; i++)
                    {
                        foreach (ProximityBattleFront b in BattleFrontList.BattleFronts[i])
                        {
                            if ((int)b.pairing == 1 && !b.PairingLocked)
                            {
                                dwarfPairingsOpen++;
                            }
                            if ((int)b.pairing == 2 && !b.PairingLocked)
                            {
                                empirePairingsOpen++;
                            }
                            if ((int)b.pairing == 3 && !b.PairingLocked)
                            {
                                elfPairingsOpen++;
                            }
                        }
                    }

                    int newPairing = (int)pairing;

                    while (newPairing == (int)pairing)
                    {
                        newPairing = random.Next(1, 4);
                        while (newPairing == 1 && dwarfPairingsOpen > 0)
                        {
                            newPairing = random.Next(1, 4);
                        }

                        while (newPairing == 2 && empirePairingsOpen > 0)
                        {
                            newPairing = random.Next(1, 4);
                        }

                        while (newPairing == 3 && elfPairingsOpen > 0)
                        {
                            newPairing = random.Next(1, 4);
                        }
                    }

                    foreach (IBattleFront b in BattleFrontList.BattleFronts[1])
                    {
                        ProximityBattleFront front = b as ProximityBattleFront;
                        if (front != null && front.PairingLocked && front.Tier == 2 && front.pairing != pairing && (int)front.pairing == newPairing)
                        {
                            front.LoadMidTierPairing(true);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is used instead of PairingLocked to allow some killing after zone is locked
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
            ProximityBattleFront t2Bttlfrnt = null;

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
                    t2Bttlfrnt = (ProximityBattleFront)WorldMgr.GetRegion(12, false).Bttlfront;
                    if (t2Bttlfrnt.LockingRealm == LockingRealm)
                        keepRank = 1;

                    bttlfrnt = WorldMgr.GetRegion(2, false).Bttlfront;
                    region = 2;
                    openZoneIndex = 1;
                    activeRegionOrZone = 5;
                    break;

                case 6: //T3 Empire vs Chaos
                    t2Bttlfrnt = (ProximityBattleFront)WorldMgr.GetRegion(14, false).Bttlfront;
                    if (t2Bttlfrnt.LockingRealm == LockingRealm)
                        keepRank = 1;

                    bttlfrnt = WorldMgr.GetRegion(11, false).Bttlfront;
                    region = 11;
                    openZoneIndex = 1;
                    activeRegionOrZone = 105;
                    break;

                case 16: //T3 HE vs DE
                    t2Bttlfrnt = (ProximityBattleFront)WorldMgr.GetRegion(15, false).Bttlfront;
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

                    if (Constants.DoomsdaySwitch != 2)
                        foreach (IBattleFront b in BattleFrontList.BattleFronts[1])
                        {
                            ProximityBattleFront front = b as ProximityBattleFront;
                            if (front != null)
                            {
                                front._BattleFrontStatus.OpenZoneIndex = 0;
                                front._BattleFrontStatus.ActiveRegionOrZone = 1;
                                WorldMgr.Database.SaveObject(front._BattleFrontStatus);
                                front.ResetPairing();
                                front.SupplyLineReset();
                                front.GraceDisabled = false;
                            }
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
                    foreach (Keep keep in bttlfrnt.Keeps)
                    {
                        if (keep.Realm == LockingRealm && (keep.ZoneId == 5 || keep.ZoneId == 105 || keep.ZoneId == 205))
                        {
                            keep.Rank = 1;
                            ProximityBattleFront front = bttlfrnt as ProximityBattleFront;
                            front.RealmRank[(int)keep.Realm-1] = 1;
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
                                plr.AddRenown(Math.Min(rpCap, (uint)(winRP * contributionFactor + (tokenCount * 100))), false, RewardType.ZoneKeepCapture, zoneName);
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
            PlayerContributions = new Dictionary<uint, ContributionInfo>();
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
            //Log.Info("LockTimer: ", "Timer was at " + PairingDrawTime.ToString() + " and its now " + TCPManager.GetTimeStamp().ToString() + " ProximityBattleFront");
            PairingLocked = false;
            GraceDisabled = false;
            DefenderPopTooSmall = false;
            _totalMaxOrder = 0;
            _totalMaxDestro = 0;
            PairingDrawTime = 0;

            RealmRank[0] = 0;
            RealmRank[1] = 0;
            RealmCurrentResources[0] = 0;
            RealmCurrentResources[1] = 0;
            RealmMaxResource[0] = 0;
            RealmMaxResource[1] = 0;
            RealmLastReturnSeconds[0] = 0;
            RealmLastReturnSeconds[1] = 0;
            RealmLostKeep[0] = false;
            RealmLostKeep[1] = false;
            RealmDeployedRam[0] = 0;
            RealmDeployedRam[1] = 0;
            RealmCannon[0] = 0;
            RealmCannon[1] = 0;

            foreach (ProximityFlag flag in _Objectives.ToList())
            {
                if (flag != null && flag.Ruin)
                {
                    _Objectives.Remove(flag);
                    flag.RemoveFromWorld();
                }
            }

            _Objectives.Clear();
            LoadObjectives();

            LockingRealm = Realms.REALMS_REALM_NEUTRAL;
            DefendingRealm = Realms.REALMS_REALM_NEUTRAL;

            HeldObjectives[0] = 4;
            HeldObjectives[1] = 0;
            HeldObjectives[2] = 0;

            VictoryPoints = 50;
            LastAnnouncedVictoryPoints = 50;

            foreach (var flag in _Objectives)
                flag.UnlockObjective();

            Random coinFlip = new Random();
            int flip = coinFlip.Next(1, 3);

            foreach (Keep keep in _Keeps)
            {
                keep.Rank = 0;
                keep._currentResource = 0;
                keep.NotifyPairingUnlocked();

                if (flip == 2)
                {
                    if (keep.Realm == Realms.REALMS_REALM_ORDER)
                        keep.Realm = Realms.REALMS_REALM_DESTRUCTION;
                    else
                        keep.Realm = Realms.REALMS_REALM_ORDER;
                }

                foreach (KeepDoor door in keep.Doors)
                {
                    door.GameObject?.SetAttackable(keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && CanAttackDoor(keep.Realm, keep.Info.ZoneId));

                }

                keep.LockKeep(keep.Realm, false, false);
                keep.SendKeepStatus(null);
            }

            Broadcast(Region.ZonesInfo[0].Name + " and " + Region.ZonesInfo[1].Name + " battlefield objectives are now open for capture!");

            _BattleFrontStatus.ActiveRegionOrZone = 1;
            _BattleFrontStatus.ControlingRealm = 0;
            WorldMgr.Database.SaveObject(_BattleFrontStatus);

            WorldMgr.SendCampaignStatus(null);

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
            switch (HeldObjectives[(byte)team])
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
                return HeldObjectives[2] > 2;
            return HeldObjectives[1] > 2;
        }

        #endregion

        #region Contribution

        private const float RENOWN_CONTRIBUTION_FACTOR = 0.1f;
        private const int CONTRIB_ELAPSE_INTERVAL = 60 * 60; // 1 hour of no contribution forfeits.
        /// <summary>
        /// Used to compare BattleFronts within a tier, to catch out zone dodging nitwits.
        /// </summary>
        public ulong TotalContribFromRenown;

        protected Dictionary<uint, ContributionInfo> PlayerContributions = new Dictionary<uint, ContributionInfo>();

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

            if (plr.RenownRank < 40)
                contribution = contribution / 2;

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

            uint contribGain = (uint)(contribution * RENOWN_CONTRIBUTION_FACTOR * contributionScaler);
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
                    //kV.Value.Player.SendClientMessage($"Contribution tick - {(uint)(125 * Tier * RENOWN_CONTRIBUTION_FACTOR)}");
#endif
                    Player targPlayer = Player.GetPlayer(kV.Value.PlayerCharId);
                    if (targPlayer != null)
                    {
                        if (targPlayer.DebugMode)
                            targPlayer.SendClientMessage($"Contribution tick - {125 * Tier * RENOWN_CONTRIBUTION_FACTOR}");
                        kV.Value.BaseContribution += (uint)(125 * Tier * RENOWN_CONTRIBUTION_FACTOR);
                    }

                }

                //else if (curTimeSeconds - kV.Value.ActiveTimeEnd > CONTRIB_ELAPSE_INTERVAL)
                //    _toRemove.Add(kV);
            }

            /*if (_toRemove.Count > 0)
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
                                player.SendLocalizeString(new[] { medallionInfo.Name, medallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
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
                                    SendDate = (uint)TCPManager.GetTimeStamp(),
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
                                    SendDate = (uint)TCPManager.GetTimeStamp(),
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
            }*/
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

        public void UpdateContributions()
        {
            long tick = TCPManager.GetTimeStampMS();

            RecalculatePopFactor();

            foreach (ProximityFlag flag in _Objectives)
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
                Log.Error("BattleFront", "Region " + Region.RegionId + " has BattleFront with tier index " + index);
                return;
            }

            ulong maxContribution = 1;

            foreach (ProximityBattleFront fnt in BattleFrontList.BattleFronts[index])
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
                WorldMgr.SendCampaignStatus(player);
        }

        #endregion

        #region Victory Points

        protected int VictoryPoints = 50;
        protected int LastAnnouncedVictoryPoints = 50;

        private long _nextVpUpdateTime;

        internal void CountRealmObjectives()
        {
            HeldObjectives[0] = 0;
            HeldObjectives[1] = 0;
            HeldObjectives[2] = 0;
            foreach (ProximityFlag flag in _Objectives)
            {
                foreach (Zone_Info zone in Zones)
                {
                    if (zone.ZoneId == flag.ZoneId && flag._state == StateFlags.Secure)
                    {
                        switch (flag.OwningRealm)
                        {
                            case Realms.REALMS_REALM_NEUTRAL:
                                HeldObjectives[0]++;
                                break;

                            case Realms.REALMS_REALM_ORDER:
                                HeldObjectives[1]++;
                                break;

                            case Realms.REALMS_REALM_DESTRUCTION:
                                HeldObjectives[2]++;
                                break;
                        }
                    }
                }
            }
        }

        public virtual void UpdateVictoryPoints()
        {
            if (Constants.DoomsdaySwitch == 2)
            {
                foreach (ProximityFlag flag in _Objectives)
                {
                    if (flag != null && !flag.Ruin)
                        EvtInterface.AddEvent(flag.GrantT2SecureRewards, 6000, 10);
                }
            }

            int time = TCPManager.GetTimeStamp();

            // First we check if this time is a draw time!
            if (PairingDrawTime != 0 && (PairingDrawTime < time || PairingDrawTime - time < 0))
            {
                Random random = new Random();
                Realms realm;

                switch (random.Next(1, 3))
                {
                    case 1:
                        realm = Realms.REALMS_REALM_ORDER;
                        break;
                    case 2:
                        realm = Realms.REALMS_REALM_DESTRUCTION;
                        break;
                    default:
                        realm = Realms.REALMS_REALM_ORDER;
                        break;
                }

                LockPairing(realm, true, false, false, true);
            }

            // This is to make sure we counted the objectives correctly, I should still check what is the root cause here...
            CountRealmObjectives();

            // Checking new lock conditions...
            

            int delta = (HeldObjectives[1] - HeldObjectives[2]) / 2;

            int maxShift = 50 + (delta * 15);

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

            // This gives +1% VPs per 60s to Order if Order controls all 4 BOs
            if (HeldObjectives[1] == 4)
                delta += 1;

            // This gives +1% VPs per 60s to Destro if Destro controls all 4 BOs
            if (HeldObjectives[2] == 4)
                delta -= 1;

#if DEBUG
            //delta *= 30;
#endif

            // Reloading siege if holding more than 2 BOs
            foreach (Keep keep in _Keeps)
            {
                float ammo = WorldMgr.WorldSettingsMgr.GetAmmoRefresh() / 10f;
                if (keep.Realm == Realms.REALMS_REALM_ORDER && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                {
                    keep.ProximityReloadSiege((int)(ammo * HeldObjectives[1]));
                }
                if (keep.Realm == Realms.REALMS_REALM_DESTRUCTION && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                {
                    keep.ProximityReloadSiege((int)(ammo * HeldObjectives[2]));
                }
            }

            // Order has overall control
            /*if (delta > 0)
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
            }*/
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
                return GetBaseResourceValue(realm) / resourceValueMax > 0.30f;

            return GetBaseResourceValue(realm) / resourceValueMax > 0.30f;
        }

        public bool CanRealmSustainRank(Realms realm, int realmRank)
        {
#if DEBUG
            return true;
#endif
            if (Constants.DoomsdaySwitch == 2)
            {
                //return true;
            }
            if (realmRank == 0)
                return true;
            if (realmRank > 5)
                realmRank = 5;

            return Region.Bttlfront.CanSustainRank(realm, _RealmResourceValueMax[realmRank]);
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

            _logger.Debug($"NoSupplies = {_NoSupplies}");

            if (!_NoSupplies)
                return;

            CountRealmObjectives();

            if (HeldObjectives[1] + HeldObjectives[2] < 4)
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

                if (Constants.DoomsdaySwitch != 2)
                {
                    while (HeldObjectives[1] < 2)
                    {
                        ProximityFlag flag = GetClosestNeutralFlagTo(orderKeep.WorldPosition);
#if DEBUG
                        flag.UnlockObjective();
#else
                    flag.UnlockObjective();
#endif
                    }


                    while (HeldObjectives[2] < 2)
                    {
                        ProximityFlag flag = GetClosestNeutralFlagTo(destroKeep.WorldPosition);
#if DEBUG
                        flag.UnlockObjective();
#else
                    flag.UnlockObjective();
#endif
                    }
                }
            }

            _NoSupplies = !_NoSupplies;

            if (Constants.DoomsdaySwitch != 2)
            {
                foreach (var objective in _Objectives)
                {
                    if (objective.FlagState == ObjectiveFlags.Open || objective.FlagState == ObjectiveFlags.Locked)
                    {
                        objective.UnlockObjective();
                        objective.StartSupplyRespawnTimer(SupplyEvent.ZoneActiveStatusChanged);
                    }
                }
            }
            else
            {
                foreach (var objective in _Objectives)
                {
                    if (objective.FlagState == ObjectiveFlags.Open || objective.FlagState == ObjectiveFlags.Locked)
                    {
                        objective.UnlockObjective();
                        objective.ActivatePortals();
                    }
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

            if (PairingDrawTime == 0)
                PairingDrawTime = TCPManager.GetTimeStamp() + 14400;

            //Log.Info("LockTimer: ", "set to: " + PairingDrawTime.ToString() + " on zone " + Zones[_BattleFrontStatus.OpenZoneIndex].Name);
        }

        public virtual void DisableSupplies()
        {
            if (_NoSupplies)
                return;

            _NoSupplies = true;

            //foreach (var objective in _Objectives)
            //    objective.BlockSupplySpawn();

            Log.Info("Supplies", "Supply retraction from " + Zones[0].Name + " and " + Zones[1].Name);

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
            foreach (ProximityFlag flag in _Objectives.ToList())
            {
                if (flag != null && flag.Ruin)
                {
                    _Objectives.Remove(flag);
                    flag.RemoveFromWorld();
                }
            }

            foreach (var obj in _Objectives)
            {
                if (obj.FlagState != ObjectiveFlags.ZoneLocked)
                    obj.UnlockObjective();
            }

            HeldObjectives[0] = 4;
            HeldObjectives[1] = 0;
            HeldObjectives[2] = 0;

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

        public bool CanDeploySiegeAtWarcamp(Player player, int level, uint protoEntry)
        {
            //return true;

            if (Constants.DoomsdaySwitch == 0)
            {
                if (level / 10 != Tier)
                {
                    player.SendClientMessage("Invalid weapon tier", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    player.SendClientMessage("This weapon is not of the correct tier.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }
            }

            Creature_proto siegeProto = CreatureService.GetCreatureProto(protoEntry);

            if (siegeProto == null)
                return false;

            int type;

            //if (RealmDeployedRam)
            switch ((GameData.CreatureSubTypes)siegeProto.CreatureSubType)
            {
                case GameData.CreatureSubTypes.SIEGE_GTAOE:
                    type = (int)MaterielType.Artillery;
                    if (RealmCannon[(int)player.Realm - 1] > _materielCaps[type][RealmRank[(int)player.Realm - 1]])
                    {
                        player.SendClientMessage("No artillery available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more artillery pieces.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
                    break;
                case GameData.CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    type = (int)MaterielType.Cannon;
                    if (RealmCannon[(int)player.Realm - 1] > _materielCaps[type][RealmRank[(int)player.Realm - 1]])
                    {
                        player.SendClientMessage("No cannon available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more cannon.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
                    break;
                case GameData.CreatureSubTypes.SIEGE_RAM:
                    type = (int)MaterielType.Ram;
                    if (RealmDeployedRam[(int)player.Realm -1] > _materielCaps[type][RealmRank[(int)player.Realm - 1]])
                    {
                        player.SendClientMessage("No rams available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more rams.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
#if !DEBUG
                    if (player.GldInterface.Guild == null || (Tier == 4 && player.GldInterface.Guild.Info.Level < 20))
                    {
                        player.SendClientMessage(Tier == 4 ? "Must be in guild of rank 20" : "Must be in guild", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage($"In order to deploy a ram, you must be in a { (Tier == 4 ? "reputable guild of rank 20 or higher." : "guild.") }", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
#endif
                    break;
                default:
                    return true;
            }
            /*switch ((GameData.CreatureSubTypes)siegeProto.CreatureSubType)
            {
                case GameData.CreatureSubTypes.SIEGE_GTAOE:
                    type = (int)MaterielType.Artillery;
                    if ((int)_materielSupply[type] < 1f || _activeMateriel[type].Count >= _materielCaps[type][RealmRank[(int)player.Realm - 1]])
                    {
                        player.SendClientMessage("No artillery available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more artillery pieces.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
                    break;
                case GameData.CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    type = (int)MaterielType.Cannon;
                    if ((int)_materielSupply[type] < 1f || _activeMateriel[type].Count >= _materielCaps[type][RealmRank[(int)player.Realm - 1]])
                    {
                        player.SendClientMessage("No cannon available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more cannon.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
                    break;
                case GameData.CreatureSubTypes.SIEGE_RAM:
                    type = (int)MaterielType.Ram;
                    if ((int)_materielSupply[type] < 1f || _activeMateriel[type].Count >= _materielCaps[type][RealmRank[(int)player.Realm - 1]])
                    {
                        player.SendClientMessage("No rams available", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage("Your supply lines cannot support any more rams.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
#if !DEBUG
                    if (player.GldInterface.Guild == null || (Tier == 4 && player.GldInterface.Guild.Info.Level < 20))
                    {
                        player.SendClientMessage(Tier == 4 ? "Must be in guild of rank 20" : "Must be in guild", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                        player.SendClientMessage($"In order to deploy a ram, you must be in a { (Tier == 4 ? "reputable guild of rank 20 or higher." : "guild.") }", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                        return false;
                    }
#endif
                    break;
                default:
                    return true;
            }*/

            switch ((GameData.CreatureSubTypes)siegeProto.CreatureSubType)
            {
                case GameData.CreatureSubTypes.SIEGE_GTAOE:
                    RealmCannon[(int)player.Realm - 1]++;
                    break;
                case GameData.CreatureSubTypes.SIEGE_SINGLE_TARGET:
                    RealmCannon[(int)player.Realm - 1]++;
                    break;
                case GameData.CreatureSubTypes.SIEGE_RAM:
                    RealmDeployedRam[(int)player.Realm - 1] = 1;
                    break;
            }
            return true;
        }

        #region Materiel

        private enum MaterielType
        {
            Barricade,
            Artillery,
            Cannon,
            Ram,
            MaxMateriel
        }

        private readonly string[] _materielMessageNames = { "palisade", "artillery piece", "cannon", "ram" };
        private readonly float[] _materielSupply = new float[4];
        private readonly List<Siege>[] _activeMateriel = { new List<Siege>(), new List<Siege>(), new List<Siege>(), new List<Siege>() };

        private readonly int[][] _materielCaps =
        {
            //new[] { 0, 2, 4, 6, 8, 10 }, // barricades
            //new[] { 1, 2, 3, 4, 5, 6 }, // artillery
            //new[] { 0, 2, 3, 5, 6, 8 }, // cannon
            //new[] { 0, 1, 1, 1, 2, 3 } // ram
            new[] { 0, 2, 2, 2, 2, 2 }, // barricades
            new[] { 1, 2, 2, 2, 2, 2 }, // artillery
            new[] { 0, 2, 2, 2, 2, 2 }, // cannon
            new[] { 0, 1, 1, 1, 1, 1 } // ram
        };

        private readonly int[][] _materielRegenTime =
        {
            new[] { 5, 5, 4, 3, 2, 2 }, // barricades
            new[] { 3, 3, 3, 3, 2, 1 }, // artillery
            new[] { 4, 3, 3, 3, 2, 1 }, // cannon
            new[] { 15, 15, 10, 7, 5, 3 } // ram
        };
        #endregion

        #region Send

        /// <summary>
        /// Sends information to a player about the objectives within a BattleFront upon their entry.
        /// </summary>
        /// <param name="plr"></param>
        public void SendObjectives(Player plr)
        {
            foreach (ProximityFlag flag in _Objectives)
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

                    foreach (ProximityFlag flag in _Objectives.ToList())
                    {
                        if (flag != null && !flag.Ruin)
                        {
                            long now = TCPManager.GetTimeStampMS();
                            long timer = (flag._nextTransitionTimestamp - TCPManager.GetTimeStampMS()) / 1000;

                            if (zone.Tier == 4)
                            {
                                if (flag.ZoneId == zone.ZoneId)
                                {
                                    boStatus = boStatus + ":" + flag.ID + ":" + (int)flag._owningRealm + ":" + (int)flag.FlagState;
                                    now = TCPManager.GetTimeStampMS();
                                    timer = (flag._nextTransitionTimestamp - TCPManager.GetTimeStampMS()) / 1000;
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
                                timer = (flag._nextTransitionTimestamp - TCPManager.GetTimeStampMS()) / 1000;
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
            CountRealmObjectives();

            plr.SendClientMessage("***** Campaign Status : Region " + Region.RegionId + " *****", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

            plr.SendClientMessage("The pairing is " + (PairingLocked ? "locked for " + ((PairingUnlockTime - TCPManager.GetTimeStampMS()) / 60000) + " more minutes." : "contested."));
            plr.SendClientMessage("Order objectives controlled: " + HeldObjectives[1]);
            plr.SendClientMessage("Destruction objectives controlled: " + HeldObjectives[2]);
            plr.SendClientMessage("Ration factors:  Order " + RationFactor[0] + ", Destruction " + RationFactor[1]);
            plr.SendClientMessage("GraceDisabled: " + Convert.ToString(GraceDisabled), ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            plr.SendClientMessage("DefenderPopToSmall: " + DefenderPopTooSmall, ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            plr.SendClientMessage("_totalMaxOrder: " + _totalMaxOrder, ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            plr.SendClientMessage("_totalMaxDestro: " + _totalMaxDestro, ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

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

            ProximityBattleFront activeFront = BattleFrontList.ActiveFronts[arr - 1] as ProximityBattleFront;
            plr.SendClientMessage("Currently active BattleFront: " + (activeFront != null ? activeFront.Zones[0].Name : "None"));
        }

        #endregion
    }
}
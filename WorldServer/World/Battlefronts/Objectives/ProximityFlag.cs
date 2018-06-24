using System;
using System.Collections.Generic;
using System.Linq;
using GameData;
using FrameWork;
using SystemData;
using Common;
using NLog;
using static WorldServer.World.BattleFronts.BattleFrontConstants;
using WorldServer.Services.World;
using WorldServer.Scenarios.Objects;
using WorldServer.World.BattleFronts.Keeps;

namespace WorldServer.World.BattleFronts.Objectives
{

    /// <summary>
    /// Internal implementation of BattleFront flags.
    /// </summary>
    public class ProximityFlag : BattleFrontObjective, IBattleFrontFlag
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public readonly IBattleFront BattleFront;

        const int DEFENSE_TICK_INTERVAL_SECONDS = 300;

        /// <summary>BattleFront objective id</summary>
        public readonly int ID;
        /// <summary>The zone info id within which the BattleFront exists.</summary>
        public new readonly ushort ZoneId;
        /// <summary>The tier within which the BattleFront exists.</summary>
        public readonly byte _tier;
        /// <summary>Influence area containing the objective</summary>
        private Zone_Area _area;
        /// <summary> This marks BO flags that are created inside ruins</summary>
        public bool Ruin = false;

        private uint _x, _y; // why other attributes ?
        private ushort _z, _o; // why other attributes ?
        private uint _tokdiscovery, _tokunlocked; // This is for ToK unlocks

        private QuadrantHistoryTracker _quadrantHistoryTracker;
        private ObjectivePortalsMgr _objectivePortalMgr;

        public override string ToString()
        {
            if (_area != null)
            {
                return
                    $"ID={ID} ZoneId={ZoneId} Tier={_tier} ZoneArea={_area.AreaId} {_area.AreaName} {_area.IsRvR} {_area.Realm} Ruin={Ruin} XYZO={_x},{_y},{_z},{_o}";
            }
            else
            {
                return
                    $"ID={ID} ZoneId={ZoneId} Tier={_tier} ZoneArea=null Ruin={Ruin} XYZO={_x},{_y},{_z},{_o}";
            }
        }

        #region Load
        public ProximityFlag(BattleFront_Objective obj, IBattleFront BattleFront, RegionMgr region, byte tier)
        {
            ID = obj.Entry;
            Name = obj.Name;
            ZoneId = obj.ZoneId;

            _x = (uint)obj.X;
            _y = (uint)obj.Y;
            _z = (ushort)obj.Z;
            _o = (ushort)obj.O;
            _tokdiscovery = obj.TokDiscovered;
            _tokunlocked = obj.TokUnlocked;

            //CaptureDuration = 10;

            Heading = _o;
            WorldPosition.X = (int)_x;
            WorldPosition.Y = (int)_y;
            WorldPosition.Z = _z;


            // TODO - remove the tier = X or ... should just be a general cast.
            // Region data
            //if ((tier == 1 || tier == 2 || tier ==3))
                BattleFront = (RoRBattleFront)BattleFront;
            //else
            //    BattleFront = (ProximityBattleFront)BattleFront;

            _tier = tier;
            _supplySpawns = BattleFrontService.GetResourceSpawns(ID);

            ZoneMgr zone = region.GetZoneMgr(obj.ZoneId);
            _area = zone.ClientInfo.GetZoneAreaFor((ushort)X, (ushort)Y, ZoneId,(ushort)Z);

            // Distance to warcamps
            _orderWarcampDistance = ComputeWarcampDistance(zone, Realms.REALMS_REALM_ORDER);
            _destroWarcampDistance = ComputeWarcampDistance(zone, Realms.REALMS_REALM_DESTRUCTION);

            // Misc
            _quadrantHistoryTracker = new QuadrantHistoryTracker(this);
            _objectivePortalMgr = new ObjectivePortalsMgr(this, region);

            
        }

        /// <summary>
        /// This constructor is used when we want to create a flag after keep take
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="name"></param>
        /// <param name="zoneId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="o"></param>
        /// <param name="BattleFront"></param>
        /// <param name="region"></param>
        /// <param name="tier"></param>
        public ProximityFlag(int entry, string name, ushort zoneId, uint x, uint y, ushort z, ushort o, IBattleFront BattleFront, RegionMgr region, byte tier)
        {
            ID = entry;
            Name = name;
            ZoneId = zoneId;

            _x = x;
            _y = y;
            _z = z;
            _o = o;
            _tokdiscovery = 0;
            _tokunlocked = 0;

            //CaptureDuration = 10;

            Heading = _o;
            WorldPosition.X = (int)_x;
            WorldPosition.Y = (int)_y;
            WorldPosition.Z = _z;

            // Region data
            if (tier == 1)
                BattleFront = (RoRBattleFront)BattleFront;
            else
                BattleFront = (ProximityBattleFront)BattleFront;

            _tier = tier;
            _supplySpawns = BattleFrontService.GetResourceSpawns(ID);

            ZoneMgr zone = region.GetZoneMgr(zoneId);
            _area = zone.ClientInfo.GetZoneAreaFor((ushort)X, (ushort)Y, ZoneId, (ushort)Z);

            // Distance to warcamps
            _orderWarcampDistance = ComputeWarcampDistance(zone, Realms.REALMS_REALM_ORDER);
            _destroWarcampDistance = ComputeWarcampDistance(zone, Realms.REALMS_REALM_DESTRUCTION);

            // Misc
            _quadrantHistoryTracker = new QuadrantHistoryTracker(this);
            //_objectivePortalMgr = new ObjectivePortalsMgr(this, region);

            _logger.Debug($"Entry={entry} Name={name} BattleFront={BattleFront.ActiveZoneName} NoSupplyFlag={BattleFront.NoSupplies} NbrObjectives={BattleFront.Objectives.Count()}");
            foreach (var obj in BattleFront.Objectives)
            {
                _logger.Debug($"Objective={obj.ObjectiveName} {obj.FlagState} {obj.OwningRealm}");
            }
            
        }

        public override void OnLoad()
        {
            // Objective position
            Z = _z;
            X = Zone.CalculPin(_x, true);
            Y = Zone.CalculPin(_y, false);
            base.OnLoad();

            SetOffset((ushort)(_x >> 12), (ushort)(_y >> 12));

            // Events
            EvtInterface.AddEvent(CheckPlayersInCloseRange, CLOSE_DETECTION_INTERVAL, 0);

            // Initial state
            IsActive = true;

            if (!Ruin)
                _objectivePortalMgr.ObjectiveUnlocked(); // Updates portals

            //if (_supplySpawns != null && _supplySpawns.Count > 0 && _tier > 1)
            //    LoadResources();
        }
        #endregion

        #region Update
        /// <summary>Timestamp of next update tick --> TODO move to EvtInterface</summary>
        private long _nextUpdateTick = 0;

        int _suppliesThrottle = 0;

        public override void Update(long tick)
        {
            ProximityBattleFront bttlfrnt = Region.Bttlfront as ProximityBattleFront;

            if (bttlfrnt != null && _owningRealm != Realms.REALMS_REALM_NEUTRAL)
            {
                bool allowGeneration = false;
                switch (bttlfrnt.RealmRank[(int)_owningRealm -1])
                {
                    case 0:
                    case 1:
                    case 2:
                        if (bttlfrnt.HeldObjectives[(int) _owningRealm] > 1)
                        {
                            allowGeneration = true;
                        }
                        break;
                    case 3:
                    case 4:
                        if (bttlfrnt.HeldObjectives[(int) _owningRealm] > 2)
                        {
                            allowGeneration = true;
                        }
                        break;
                    case 5:
                        if (bttlfrnt.HeldObjectives[(int) _owningRealm] > 3)
                        {
                            allowGeneration = true;
                        }
                        break;
                }

                if (!Ruin && _state == StateFlags.Secure && _nextTransitionTimestamp == 0)
                {
                    _suppliesThrottle++;

                    if (_suppliesThrottle > 120) // This is meet every 6 seconds
                    {
                        if (allowGeneration)
                        {
                            GenerateSuppliesFromProximityFlag(); // This is generating Supplies for keeps
                            _suppliesThrottle = 0;
                        }
                        else
                        {
                            RepairDoorsFromProximityFlag();
                            _suppliesThrottle = 0;
                        }
                    }
                }
            }

            if (PendingDisposal)
            {
                Dispose();
                return;
            }

            EvtInterface.Update(tick);

            if (tick < _nextUpdateTick)
                return;
            //_nextUpdateTick = tick + 500;

            UpdateGauge(tick);
        }
        #endregion

        #region Ownership and assault state
        public StateFlags _state;
        public Realms _owningRealm;
        /// <summary>The currently assaulting team, may be neutral</summary>
        private Realms _assaultingRealm;
        /// <summary>
        /// Main control jauge in milliseconds :
        /// -CONTROL_GAUGE_MAX : generating resources for order
        /// -CONTROL_GAUGE_MAX to CONTROL_GAUGE_MAX : transition
        /// CONTROL_GAUGE_MAX : generating resources for destro
        /// </summary>
        /// <remarks>
        /// This is the base value when computing transition timer.
        /// </remarks>
        private int _controlGauge = 0;

        private long _lastGaugeUpdateTick;

        /// <summary>Expected timestamp of next owner change<summary>
        public long _nextTransitionTimestamp;
        /// <summary>Positive between 0 and SECURE_PROGRESS_MAX indicating the objective securisation indicator, in seconds<summary>
        private int _secureProgress;

        /// <summary>Displayed timer in seconds</summary>
        private short _displayedTimer;

        /// <summary>Absolute transition speed last time it was recomputed</summary>
        private float _lastTransitionUpdateSpeed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tick"></param>
        private void UpdateGauge(long tick)
        {
            if (_state == StateFlags.ZoneLocked)
                return;

            // Carbonara check for weirdly unlocking T2 and T3 BOs. Possibly broken for T4, but 
            // we will handle that when we get there...
            //if (_tier > 1)
            //{
                T1BattleFront frnt = (T1BattleFront)Region.Bttlfront;
                if (frnt != null && frnt.PairingLocked)
                    return;
            //}

            //if (_tier == 4)
            //{
            //    ProximityProgressingBattleFront prFrnt = (ProximityProgressingBattleFront)Region.Bttlfront;
            //    if (prFrnt != null && ZoneId != prFrnt.Zones[prFrnt._BattleFrontStatus.OpenZoneIndex].ZoneId)
            //        return;
            //}

            // Apply previously computed transition speed since last update
            if (_lastTransitionUpdateSpeed != 0)
            {
                _controlGauge += (int)((tick - _lastGaugeUpdateTick) * _lastTransitionUpdateSpeed);
                _controlGauge = Clamp(_controlGauge, -MAX_CONTROL_GAUGE, MAX_CONTROL_GAUGE);
            }
            // If flag was unsecured by any realm, it lose control until it reachs zero
            else if (_state == StateFlags.Unsecure)
            {
                bool wasOrder = _controlGauge < 0;
                float defaultTransitionSpeed = -Clamp(_controlGauge, -1, 1);
                _controlGauge += (int)((tick - _lastGaugeUpdateTick) * defaultTransitionSpeed);
                if (wasOrder != _controlGauge < 0) // If reached zero
                    _controlGauge = 0;

                //if (_tier > 1)
                //BlockSupplySpawn();
            }

            float newTransitionSpeed = GetNewTransitionSpeed();
            int incomingSecureProgress = Math.Abs(_controlGauge) * MAX_SECURE_PROGRESS / MAX_CONTROL_GAUGE;

            bool announce = true;

            if (newTransitionSpeed == 0f) // Status quo or fully secured || (OwningRealm == Realms.REALMS_REALM_DESTRUCTION) == assaultSpeed > 0
            {
                // _controlGauge = CONTROL_GAUGE_MAX;
                _assaultingRealm = Realms.REALMS_REALM_NEUTRAL;

                if (incomingSecureProgress != MAX_SECURE_PROGRESS) // Intermediate state
                {
                    //if (_tier == 1) // This was added to allow removal of intermediate state when there are no players around in T2+
                    //{
                        _owningRealm = Realms.REALMS_REALM_NEUTRAL;
                        _state = StateFlags.Unsecure;
                    //}
                    //else
                        //_state = StateFlags.Abandoned;
                }
                else if (newTransitionSpeed == 0 && _closeOrderCount == _closeDestroCount) // Abandonned
                {
                    //if (_tier == 1) // This was added to allow removal of Abandonned state when there are no players around in T2+
                    //{
                        _owningRealm = Realms.REALMS_REALM_NEUTRAL;
                        _state = StateFlags.Unsecure;
                    //}
                    //else
                      //  _state = StateFlags.Abandoned;
                }
                else // Fully secured
                {
                    if (_state == StateFlags.ZoneLocked)
                        return;

                    _state = StateFlags.Secure;
                    if (_controlGauge == -MAX_CONTROL_GAUGE)
                    {
                        _owningRealm = Realms.REALMS_REALM_ORDER;
                        if (_tier == 4 && Constants.DoomsdaySwitch == 2)
                        {
                            ProximityProgressingBattleFront proxBttlfrnt = (ProximityProgressingBattleFront)Region.Bttlfront;
                            proxBttlfrnt.ObjectiveCaptured(_assaultingRealm, _owningRealm, ZoneId);
                        }
                        else if (_tier > 1 && Constants.DoomsdaySwitch == 2)
                        {
                            //T1BattleFront proxBttlfrnt = (T1BattleFront)Region.Bttlfront;
                            ProximityBattleFront proxBttlfrnt = (ProximityBattleFront)Region.Bttlfront;

                            proxBttlfrnt.ObjectiveCaptured(_assaultingRealm, _owningRealm, ZoneId);
                        }
                        
                    }
                    else if (_controlGauge == MAX_CONTROL_GAUGE)
                    {
                        _owningRealm = Realms.REALMS_REALM_DESTRUCTION;
                        if (_tier == 4 && Constants.DoomsdaySwitch == 2)
                        {
                            ProximityProgressingBattleFront proxBttlfrnt = (ProximityProgressingBattleFront)Region.Bttlfront;
                            proxBttlfrnt.ObjectiveCaptured(_assaultingRealm, _owningRealm, ZoneId);
                        }
                        else if (_tier > 1 && Constants.DoomsdaySwitch == 2)
                        {
                            ProximityBattleFront proxBttlfrnt = (ProximityBattleFront)Region.Bttlfront;
                            proxBttlfrnt.ObjectiveCaptured(_assaultingRealm, _owningRealm, ZoneId);
                        }
                    }

                    // codeword 0ni0n
                    //StartSupplyRespawnTimer(SupplyEvent.Reset);

                    if (Ruin)
                    {
                        foreach (Keep keep in Region.Bttlfront.Keeps)
                        {
                            if (ID == keep.Info.KeepId)
                            {
                                keep.Realm = _owningRealm;
                                keep.SendKeepStatus(null);
                            }
                        }
                    }
                }

                // No more update until next close player change or lock / unlock
                _displayedTimer = 0;
                _nextTransitionTimestamp = 0;
                _nextUpdateTick = long.MaxValue;
                announce = true;
            }
            else // Changing
            {
                bool toOrder = newTransitionSpeed < 0f;
                bool isOrder = _controlGauge != 0 ? _controlGauge < 0f : toOrder;

                // Updates owning and assault teams
                _assaultingRealm = Realms.REALMS_REALM_NEUTRAL;
                if (isOrder)
                {
                    _owningRealm = Realms.REALMS_REALM_ORDER;
                    if (!toOrder)
                        _assaultingRealm = Realms.REALMS_REALM_DESTRUCTION;
                }
                else
                {
                    _owningRealm = Realms.REALMS_REALM_DESTRUCTION;
                    if (toOrder)
                        _assaultingRealm = Realms.REALMS_REALM_ORDER;
                }

                // Computes timers
                int remainingTransitionTime; // LastUpdatedTime in millis until next update
                if (toOrder == isOrder) // Securing
                {
                    _state = StateFlags.Secure;
                    remainingTransitionTime = MAX_CONTROL_GAUGE;
                    if (toOrder)
                    {
                        remainingTransitionTime += _controlGauge;
                        //newTransitionSpeed = -newTransitionSpeed; // Negative speed to set it to o
                    }
                    else
                        remainingTransitionTime -= _controlGauge;
                    remainingTransitionTime = (int)Math.Abs(remainingTransitionTime / newTransitionSpeed);
                    _displayedTimer = (short)(remainingTransitionTime / 1000); // Full secure time - already secured time
                }
                else // Assaulting
                {
                    _state = StateFlags.Contested;
                    remainingTransitionTime = (int)Math.Abs(_controlGauge / newTransitionSpeed);
                    _displayedTimer = (short)((Math.Abs(MAX_CONTROL_GAUGE / newTransitionSpeed) + remainingTransitionTime) / 1000); // Full secure time + remaining assault time
                }

                // Updates next transition timestamp
                _nextTransitionTimestamp = tick + remainingTransitionTime;
                _nextUpdateTick = _nextTransitionTimestamp;
                // Log.Info("remainingTransitionTime", remainingTransitionTime.ToString());

                //BlockSupplySpawn();
            }

            _lastGaugeUpdateTick = tick;
            _lastTransitionUpdateSpeed = newTransitionSpeed;
            _secureProgress = incomingSecureProgress;

#if disabled
        Log.Info("newSecureProgress", newSecureProgress.ToString());
        Log.Info("newTransitionSpeed", newTransitionSpeed.ToString());
        Log.Info("_displayedTimer", _displayedTimer.ToString());
        Log.Info("_owningRealm", _owningRealm.ToString());
        Log.Info("_assaultingRealm", _assaultingRealm.ToString());
#endif

            // This is for State of the Realms addon
            UpdateStateOfTheRealmBO();

            BroadcastFlagInfo(announce);
        }

        public void UpdateStateOfTheRealmBO()
        {
            if (this != null && !Ruin)
            {
                long now = TCPManager.GetTimeStampMS();
                long timer = (_nextTransitionTimestamp - TCPManager.GetTimeStampMS()) / 1000;

                string stateOfTheRealmMessage = "SoR_T" + _tier + "_BO_Update:" + ZoneId + ":" + ID + ":" + (int)_owningRealm + ":" + (int)State;

                if (timer > 0)
                    stateOfTheRealmMessage = stateOfTheRealmMessage + ":" + timer;
                else
                    stateOfTheRealmMessage = stateOfTheRealmMessage + ":0";

                if (stateOfTheRealmMessage != "")
                {
                    foreach (Player player in Player._Players.ToList())
                    {
                        if (player != null && player.SoREnabled)
                            player.SendLocalizeString(stateOfTheRealmMessage, ChatLogFilters.CHATLOGFILTERS_CHANNEL_9, Localized_text.CHAT_TAG_DEFAULT);
                    }

                }
            }
        }

        /// <summary>Gets the current state of the flag.</summary>
        public StateFlags State { get { return _state; } }
        //Disabled Obsolete - why was it?
        //[Obsolete]
        public ObjectiveFlags FlagState { get { return (ObjectiveFlags)_state; } } // TODO clean up

        /// <summary>
        /// Gets the realm that have actually secured to objective.
        /// </summary>
        /// <returns>Realm or n</returns>
        public Realms GetSecureRealm() {
            if ((_state != StateFlags.ZoneLocked && _state != StateFlags.Secure) || _secureProgress < MAX_SECURE_PROGRESS)
                return Realms.REALMS_REALM_NEUTRAL;
            return _owningRealm;
        }

        /// <summary>Gets the currently owning realm, may be neutral.</summary>
        public Realms OwningRealm { get { return _owningRealm; } }

        /// <summary>
        /// Prevents this objective from being captured.
        /// </summary>
        public void LockObjective(Realms lockingRealm, bool announce)
        {
            _accumulatedKills = 0;

            _owningRealm = lockingRealm;
            _assaultingRealm = Realms.REALMS_REALM_NEUTRAL;
            _state = StateFlags.ZoneLocked;
            _nextTransitionTimestamp = 0;
            _accumulatedKills = 0; 
             _controlGauge = lockingRealm == Realms.REALMS_REALM_ORDER ? -MAX_CONTROL_GAUGE : MAX_CONTROL_GAUGE;
            _lastGaugeUpdateTick = 0;
            _lastTransitionUpdateSpeed = 0;
            _secureProgress = MAX_SECURE_PROGRESS;
            _displayedTimer = 0;

            _closeOrderCount = 0;
            _closeDestroCount = 0;
            _closePlayers.Clear();

            //if (_tier == 1)
            //{
                _objectivePortalMgr.ObjectiveLocked(); // Updates portals
            //}

            if (!announce)
                return;

            BroadcastFlagInfo(false);
            _delayedRewards.Clear();
        }
       
        /// <summary>
        /// Allows this objective to be captured if it was previously locked.
        /// </summary>
        public void UnlockObjective()
        {
            _state = StateFlags.Unsecure;

            _owningRealm = Realms.REALMS_REALM_NEUTRAL;
            _assaultingRealm = Realms.REALMS_REALM_NEUTRAL;
            _state = StateFlags.Unsecure;
            _nextTransitionTimestamp = 0;
            _accumulatedKills = 0;
            _controlGauge = 0;
            _lastGaugeUpdateTick = 0;
            _lastTransitionUpdateSpeed = 0;
            _secureProgress = 0;
            _displayedTimer = 0;

            if (!Ruin)
                _objectivePortalMgr.ObjectiveUnlocked(); // Updates portals

            BroadcastFlagInfo(false);
        }

        public void ActivatePortals()
        {
            if (!Ruin)
                _objectivePortalMgr.ObjectiveUnlocked(); // Updates portals
        }

        /// <summary>
        /// Checks chether the flag is locked.
        /// </summary>
        /// <returns>True if locked</returns>
        public bool IsLocked => _state == StateFlags.ZoneLocked;

        /// <summary>Computes a string for a realm.</summary>
        /// <param name="realm">To compute string for, should not be neutral</param>
        /// <returns>"Order" / "Destruction"</returns>
        private static string GetRealmString(Realms realm)
        {
            return realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction";
        }
        #endregion

        #region Rewarding
        /// <summary>Distances scalers for objective rewards</summary>
        /// <remarks>
        /// Depends on tier and distance between objective and warcamp.
        /// Average values of objectives in the same zone is 1f.
        /// </remarks>
        private float _orderDistanceRewardScaler, _destroDistanceRewardScaler;

        /// <summary>Total kill cound in objective area since unlock.</summary>
        private int _accumulatedKills;

        private readonly Dictionary<uint, XpRenown> _delayedRewards = new Dictionary<uint, XpRenown>();

        /// <summary>
        /// Grands a small reward to all players in close range for defending.
        /// </summary>
        /// <returns>Positive or negative value depending on the own flag scaler for destro or order,
        /// 0 if an update was done (for optimization purpose)</returns>
        /// <remarks>Invoked in short periods of time</remarks>
        internal float GrantSecureRewards()
        {
            if (_secureProgress != MAX_SECURE_PROGRESS)
                return 0f;
            if (_closeOrderCount > 0 == _closeDestroCount > 0)
                return 0f; // Both sides have players in range, or none of them -> not fully secured

            int playerCount;
            ushort influenceId;
            float scaleMult;
            if (_owningRealm == Realms.REALMS_REALM_ORDER)
            {
                playerCount = _closeOrderCount;
                scaleMult = _orderDistanceRewardScaler;
            }
            else
            {
                playerCount = _closeDestroCount;
                scaleMult = _destroDistanceRewardScaler;
            }

            // No multiplier based on population
            scaleMult *= Region.Bttlfront.GetObjectiveRewardScaler(_owningRealm, playerCount);

            // Because of the Field of Glory buff, the XP value here is doubled.
            // The base reward in T4 is therefore 3000 XP.
            // Population scale factors can up this to 9000 if the region is full of people and then raise or lower it depending on population balance.
            float baseXp = FLAG_SECURE_REWARD_SCALER * _tier * scaleMult;
            float baseRp = baseXp / 10f;
            float baseInf = baseRp / 2.5f;

            foreach (Player player in _closePlayers)
            {
                if (player.Realm != _owningRealm || player.CbtInterface.IsInCombat || player.IsAFK || player.IsAutoAFK)
                    continue;

                // Bad dirty hak by Hargrim to fix Area Influence bug
                if (_owningRealm == Realms.REALMS_REALM_ORDER)
                    influenceId = (ushort)player.CurrentArea.OrderInfluenceId;
                else
                    influenceId = (ushort)player.CurrentArea.DestroInfluenceId;


                player.AddXp(Math.Max((uint)baseXp, 1), false, false);
                player.AddRenown(Math.Max((uint)baseRp, 1), false, RewardType.ObjectiveCapture, Name);
                player.AddInfluence(influenceId, Math.Max((ushort)baseInf, (ushort)1));
                BattleFront.AddContribution(player, (uint)baseRp);
            }

            // Scaler return
            if (_owningRealm == Realms.REALMS_REALM_ORDER)
                return -_orderDistanceRewardScaler;
            else
                return _destroDistanceRewardScaler;
        }

        internal void GrantT2SecureRewards()
        {
            if (_secureProgress != MAX_SECURE_PROGRESS)
                return;
            if (_closeOrderCount > 0 == _closeDestroCount > 0)
                return; // Both sides have players in range, or none of them -> not fully secured

            int playerCount;
            ushort influenceId;
            float scaleMult;

            if (_tier > 1)
            {
                _orderDistanceRewardScaler = 1.0f;
                _destroDistanceRewardScaler = 1.0f;
            }

            if (_owningRealm == Realms.REALMS_REALM_ORDER)
            {
                playerCount = _closeOrderCount;
                scaleMult = _orderDistanceRewardScaler;
            }
            else
            {
                playerCount = _closeDestroCount;
                scaleMult = _destroDistanceRewardScaler;
            }

            // No multiplier based on population
            scaleMult *= Region.Bttlfront.GetObjectiveRewardScaler(_owningRealm, playerCount);

            // This method generates supplies for keeps from holding Battle Objectives
            //GenerateSuppliesFromProximityFlag();

            // Because of the Field of Glory buff, the XP value here is doubled.
            // The base reward in T4 is therefore 3000 XP.
            // Population scale factors can up this to 9000 if the region is full of people and then raise or lower it depending on population balance.
            float keepRank = 0;

            ProximityBattleFront bttlfrnt = (ProximityBattleFront)Region.Bttlfront;

            keepRank = bttlfrnt.RealmRank[(int)OwningRealm-1];
            
            float tierModifier = 1f + ((float)_tier / 4.0f);
            float baseXp = 60f * tierModifier; //* scaleMult; //30f
            float baseRp = baseXp * (1f / (2f+keepRank));
            float baseInf = baseXp / 3f;

            baseXp = Math.Max((uint)baseXp, 1);
            baseRp = Math.Max((uint)baseRp, 1);
            baseInf = Math.Max((uint)baseInf, 1);

            if (keepRank > 2)
            {
                baseXp = 0;
                baseRp = 0;
                baseInf = 0;
            }

            foreach (Player player in _closePlayers)
            {
                if (player.Realm != _owningRealm || player.CbtInterface.IsInCombat || player.IsAFK || player.IsAutoAFK)
                    continue;

                // Bad dirty hak by Hargrim to fix Area Influence bug
                if (_owningRealm == Realms.REALMS_REALM_ORDER)
                    influenceId = (ushort)player.CurrentArea.OrderInfluenceId;
                else
                    influenceId = (ushort)player.CurrentArea.DestroInfluenceId;

                if (bttlfrnt.HeldObjectives[(int)_owningRealm] > 1)
                {
                    player.AddXp((uint)baseXp, false, false);
                    player.AddRenown((uint)baseRp, false, RewardType.ObjectiveCapture, Name);
                    player.AddInfluence(influenceId, (ushort)baseInf);
                    // We are removing contribution from holding BOs
                    //BattleFront.AddContribution(player, (uint)baseRp);
                }
            }
        }

        public bool CheckKillValid(Player player)
        {
            if (!IsLocked && _playersInRangeCount > 4 && Get2DDistanceToObject(player) < 200)
            {
                if (player.Realm != _owningRealm)
                    AccumulatedKills++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Grants rewards upon a zone lock, depending on acumulated kills.
        /// </summary>
        public void GrantKeepCaptureRewards()
        {
            //throw new NotImplementedException("GrantKeepCaptureRewards");
        }

        public void AddDelayedRewardsFrom(Player killer, Player killed, uint xpShare, uint renownShare)
        {
            if (xpShare == 0 && renownShare == 0)
                return;

            XpRenown curEntry;

            uint renownReward = (uint)(renownShare * killer.GetKillRewardScaler(killed));

#if BattleFront_DEBUG
            player.SendClientMessage($"{ObjectiveName} storing {xpShare} XP and {renownReward} renown");
#endif
            _delayedRewards.TryGetValue(killer.CharacterId, out curEntry);

            if (curEntry == null)
            {
                curEntry = new XpRenown(xpShare, renownReward, 0, 0, TCPManager.GetTimeStamp());
                if (killer.Realm == _owningRealm)
                    curEntry.LastUpdatedTime = TCPManager.GetTimeStamp() + DEFENSE_TICK_INTERVAL_SECONDS;

                _delayedRewards.Add(killer.CharacterId, curEntry);
            }

            else
            {
                curEntry.XP += xpShare;
                curEntry.Renown += renownReward;
            }

            if (killer.Realm != _owningRealm)
                curEntry.LastUpdatedTime = TCPManager.GetTimeStamp();
        }
        
        /// <summary>Total kill cound in objective area since unlock.</summary>
        internal int AccumulatedKills {
            get { return _accumulatedKills; }
            set { _accumulatedKills = value; }
        }

        /// <summary>Sets the reward scalers depending on distances to warcamps.<summary>
        /// <param name="avgOrderDistance">Average objectives distances to order warcamp</param>
        /// <param name="avgDestroDistance">Average objectives distances to destro warcamp</param>
        internal void SetWarcampDistanceScaler(float avgOrderDistance, float avgDestroDistance)
        {
            _orderDistanceRewardScaler = _orderWarcampDistance / avgOrderDistance;
            _destroDistanceRewardScaler = _destroWarcampDistance / avgDestroDistance;
        }
        #endregion
        
        #region Range
        private short _playersInRangeCount;

        /// <summary>Set of all players in close range, not limited</summary>
        private ISet<Player> _closePlayers = new HashSet<Player>();
        /// <summary>For memory optimization</summary>
        private ISet<Player> _closePlayersBuffer = new HashSet<Player>();

        /// <summary>Numbers of players close to the flag who can influence the state of the flag (0/1 for o/d), limited to CLOSE_PLAYERS_MAX</summary>
        private volatile short _closeOrderCount = 0, _closeDestroCount = 0;

        /// <summary>True if a player is threatening players (opposite of secure realm)</summary>
        private bool _hasThreateningPlayer;

        internal bool HasThreateningPlayer { get { return _hasThreateningPlayer; } }

        /// <summary>Distance between the objective and the warcamps</summary>
        private float _orderWarcampDistance, _destroWarcampDistance;

        public override void AddInRange(Object obj)
        {
            Player plr = obj as Player;
            if (plr != null && plr.ValidInTier(_tier, true))
            {
                SendFlagInfo(plr);
                //NEWDAWN
                //plr.CurrentObjectiveFlag = this;
                ++_playersInRangeCount;

                if (_tokdiscovery > 0)
                    plr.TokInterface.AddTok((ushort)this._tokdiscovery);
                if (_tokunlocked > 0)
                    plr.TokInterface.AddTok((ushort)this._tokunlocked);

                base.AddInRange(obj);
            }
        }

        public override void RemoveInRange(Object obj)
        {
            Player plr = obj as Player;
            if (plr != null)
            {
                SendFlagLeft(plr);
                //NEWDAWN
                //if (plr.CurrentObjectiveFlag == this)
                //    plr.CurrentObjectiveFlag = null;
                --_playersInRangeCount;
            }

            base.RemoveInRange(obj);
        }

        /// <summary>
        /// Checks where are the players in range to update their count depending on distances.
        /// </summary>
        private void CheckPlayersInCloseRange()
        {
            if (_state == StateFlags.ZoneLocked)
                return;

            short orderCount = 0;
            short destroCount = 0;
            int closeHeight = CLOSE_RANGE / 2 * UNITS_TO_FEET;
            int threatenHeight = THREATEN_RANGE / 2 * UNITS_TO_FEET;

            _hasThreateningPlayer = false;

            foreach (Player player in PlayersInRange)
            {
                if (player.IsDead || player.StealthLevel != 0 || !player.CbtInterface.IsPvp || player.IsInvulnerable)
                    continue;

                int distance = GetDistanceToObject(player);
                int heightDiff = Math.Abs(Z - player.Z);
                if (distance < CLOSE_RANGE && heightDiff < closeHeight)
                {
                    if (player.Realm == Realms.REALMS_REALM_ORDER)
                        orderCount++;
                    else
                        destroCount++;
                    _closePlayersBuffer.Add(player);
                }

                // Updates the threatening flag if was not already set
                _hasThreateningPlayer |= player.Realm != _owningRealm // One player of opposite realm
                    && distance < THREATEN_RANGE && heightDiff < threatenHeight; // under threaten range
            }

            // Switch the players sets
            ISet<Player> previous = _closePlayers;
            _closePlayers = _closePlayersBuffer;
            _closePlayersBuffer = previous;
            _closePlayersBuffer.Clear();

            // Trim counts
            if (orderCount > MAX_CLOSE_PLAYERS)
                orderCount = MAX_CLOSE_PLAYERS;
            if (destroCount > MAX_CLOSE_PLAYERS)
                destroCount = MAX_CLOSE_PLAYERS;

            if (orderCount - destroCount != _closeOrderCount - _closeDestroCount)
                _nextUpdateTick = 0; // Asks for immediate refresh of flag's state

            _closeOrderCount = orderCount;
            _closeDestroCount = destroCount;

            // Portals depend on players around
            //if (_tier == 1 && (ZoneId == 101 || ZoneId == 106 || ZoneId == 107) && _owningRealm == Realms.REALMS_REALM_ORDER && _state == StateFlags.Secure)
            if (!Ruin)
                _objectivePortalMgr.UpdateWarcampPortals();
        }

        /// <summary>
        /// Conputes the assaulting speed depending of players in close range.
        /// </summary>
        /// <returns>
        /// Timer accelerator :
        ///  -2 to -1 for order assault
        ///  -0 for no assault
        ///  1 to 2 for destro assault
        /// </returns>
        private float GetNewTransitionSpeed()
        {
            int diff = _closeDestroCount - _closeOrderCount;
            if (diff == 0)
                return 0f; // no assault or assault stopped
            else if (diff > 0 && _controlGauge == MAX_CONTROL_GAUGE)
                return 0f; // Already fully controlled by destro
            else if (diff < 0 && _controlGauge == -MAX_CONTROL_GAUGE)
                return 0f; // Already fully controlled by order

            float speed;
            if (diff > 0)
                speed = 1f + (diff - 1) / 5f;
            else
                speed = -1f + (diff + 1) / 5f;
            return speed;
        }

        /// <summary>
        /// Utility method computing the distance of the objective to to a warcamp.
        /// </summary>
        /// <param name="zone">Zone containing te objective</param>
        /// <param name="realm">Warcamp owner realm</param>
        /// <returns>Positive distance</returns>
        private float ComputeWarcampDistance(ZoneMgr zone, Realms realm)
        {
            Zone_Info zoneInfo = zone.Info;
            Point3D warcampPosition = BattleFrontService.GetWarcampEntrance(ZoneId, realm);

            // Warcamps can be declared for a single zone in region
            if (warcampPosition == null)
            {
                foreach (Zone_Info other in zone.Region.ZonesInfo)
                {
                    if (other.ZoneId != ZoneId)
                    {
                        warcampPosition = BattleFrontService.GetWarcampEntrance(other.ZoneId, realm);
                        if (warcampPosition != null)
                            zoneInfo = other;
                            break;
                    }
                }
            }
            warcampPosition = ZoneService.GetWorldPosition(zoneInfo, (ushort)warcampPosition.X, (ushort)warcampPosition.Y, (ushort)warcampPosition.Z);

            return Get2DDistanceToWorldPoint(warcampPosition);
        }

        /// <summary>
        /// Gets the distance between the objective and the warcamp.
        /// </summary>
        /// <param name="realm">Realm to get distance of, order or destro</param>
        /// <returns>World distance</returns>
        public float GetWarcampDistance(Realms realm)
        {
            if (realm == Realms.REALMS_REALM_ORDER)
                return _orderWarcampDistance;
            else
                return _destroWarcampDistance;
        }
        #endregion

        #region Send
        /// <summary>
        /// Broadcasts flag state to all players withing range.
        /// </summary>
        /// <param name="announce">True </param>
        public void BroadcastFlagInfo(bool announce)
        {
            foreach (Player plr in PlayersInRange) // probably synchronization bug
            {
                SendMeTo(plr);

                SendFlagInfo(plr);
            }

            SendFlagState(null, announce);
        }

        /// <summary>
        /// Sends the flag object ti given player.
        /// </summary>
        /// <param name="plr">Player that entered the objective area</param>
        public override void SendMeTo(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC, 64);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);

            Out.WriteUInt16(_o);
            Out.WriteUInt16(_z);
            Out.WriteUInt32(_x);
            Out.WriteUInt32(_y);

            ushort displayId;

            switch (OwningRealm)
            {
                case Realms.REALMS_REALM_NEUTRAL:
                    displayId = 3442;
                    break;
                case Realms.REALMS_REALM_ORDER:
                    displayId = 3443;
                    break;
                case Realms.REALMS_REALM_DESTRUCTION:
                    displayId = 3438;
                    break;
                default:
                    displayId = 3442;
                    break;
            }

            Out.WriteUInt16(displayId);

            Out.WriteUInt16(0x1E);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteByte(0);
            
            Out.WriteUInt16(0); // Write 4 here to set interactable

            Out.WriteByte(0);

            Out.WriteUInt16(0);
            Out.WriteByte(0);
            Out.WriteByte(100);

            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteUInt32(0);

            Out.WritePascalString("Battlefield Objective");
            Out.WriteByte(0);

            plr.SendPacket(Out);
            // does nothing : base.SendMeTo(plr);
        }
        
        /// <summary>
        /// Sends objective's detailed info in upper right corner of the screen.
        /// </summary>
        /// <param name="plr"></param>
        public void SendFlagInfo(Player plr)
        {
            // return;
            Realms owningRealm = _owningRealm;
            Realms assaultingRealm = _assaultingRealm;

            if (_tier == 4)
            {
                ProximityProgressingBattleFront prFrnt = (ProximityProgressingBattleFront)Region.Bttlfront;
                if (prFrnt != null && ZoneId != prFrnt.Zones[prFrnt._BattleFrontStatus.OpenZoneIndex].ZoneId)
                {
                    owningRealm = _owningRealm;
                    assaultingRealm = _owningRealm;
                }
            }

            // Log.Info("Name", ID.ToString());
            // Log.Info("_owningRealm", Enum.GetName(typeof(Realms), owningRealm));
            // Log.Info("_assaultingRealm", Enum.GetName(typeof(Realms), assaultingRealm));

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO, 32);
            Out.WriteUInt32((uint)ID);
            Out.WriteByte(0);
            Out.WriteByte((byte)owningRealm); //(byte)_owningRealm
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WritePascalString(Name);
            // 
            // 
            // 
            Out.WriteByte(2);
            Out.WriteUInt32(0x0000348F);
            Out.WriteByte((byte)assaultingRealm); // (byte)_assaultingRealm

            // Expansion for objective goal
            Out.WriteByte(0);

            Out.WriteUInt16(0xFF00);
            Out.WritePascalString(GetStateText(plr.Realm));
            Out.WriteByte(0);

            switch (_state)
            {
                case StateFlags.ZoneLocked:
                case StateFlags.Locked:
                    Out.WritePascalString("This area has been captured by ", GetRealmString(_owningRealm), ". The battle wages on elsewhere!");
                    break;

                case StateFlags.Contested:
                    if (plr.Realm != _owningRealm)
                        Out.WritePascalString("This Battlefield Objective is being assaulted by ", GetRealmString(_assaultingRealm),
                            ". Ensure the timer elapses to claim it for your Realm!");
                    else
                        Out.WritePascalString("This Battlefield Objective is being assaulted by ", GetRealmString(_assaultingRealm),
                            ". Reclaim this Battlefield Objective for ", GetRealmString(plr.Realm), "!");
                    break;

                case StateFlags.Secure:
                    if (plr.Realm == _owningRealm)
                        Out.WritePascalString("This Battlefield Objective is generating resources for ", GetRealmString(_owningRealm),
                            ". Defend the flag from enemy assault!");
                    else
                        Out.WritePascalString("This Battlefield Objective is generating resources for ", GetRealmString(_owningRealm),
                            ". Claim this Battlefield Objective for ", GetRealmString(plr.Realm), "!");
                    break;

                case StateFlags.Unsecure:
                    Out.WritePascalString("This Battlefield Objective is open for capture!");
                    break;

                default:
                    Out.WritePascalString("");
                    break;
            }

            // Displayed transition timer in seconds
            ushort transitionTimer = (_nextTransitionTimestamp == 0 ? (ushort)0 : (ushort)((_nextTransitionTimestamp - TCPManager.GetTimeStampMS()) / 1000));
            transitionTimer = (ushort)_displayedTimer;
            // Log.Info("transitionTimer", _displayedTimer.ToString());
            
            Out.WriteUInt16(0); // _displayedTimer
            Out.WriteUInt16((ushort)_displayedTimer); // _displayedTimer
            Out.WriteUInt16(0); // _displayedTimer
            Out.WriteUInt16((ushort)_displayedTimer); // _displayedTimer
            Out.Fill(0, 4);
            Out.WriteByte(0x71);
            Out.WriteByte(1);
            Out.Fill(0, 3);

            plr.SendPacket(Out);
        }

        public void SendFlagLeft(Player plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE, 8);

            Out.WriteUInt32((uint)ID);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        private StateFlags _lastBroadCastState = StateFlags.Hidden;
        private bool _lastSecutedState = false;

        /// <summary>
        /// Sends the state of the objective in user's map, can announce state changing.
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="announce"></param>
        /// <param name="update"></param>
        public void SendFlagState(Player plr, bool announce, bool update = true)
        {
            if (!Loaded)
                return;

            byte flagState = GetStateFlags();

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
            Out.WriteUInt32((uint)ID);

            if (_state == StateFlags.Contested /*|| (_state == StateFlags.Secure && MAX_SECURE_PROGRESS == _secureProgress)*/)
            {
                Out.Fill(0, 2);
                Out.WriteUInt16((ushort)_displayedTimer);
                Out.Fill(0xFF, 2);
                Out.WriteUInt16(0);
            }
            /*else if (GeneratingSupplies && _tier > 1 && _displayedTimer == 0)
            {
                Out.Fill(0xFF, 4);
                Out.WriteByte(0);
                Out.WriteByte((byte)Math.Max(0, _generationTimerEnd - TCPManager.GetTimeStamp())); // Unk6 - time till next resource release
                Out.WriteUInt16(175); // Unk7 - vertical offset for drawing overlay - Unk6 may not exceed
            }*/
            else if (_state == StateFlags.Secure)
            {
                //Out.Fill(0xFF, 4);
                Out.Fill(0, 2);
                Out.WriteUInt16((ushort)_displayedTimer);
                //Out.WriteByte((byte)(SECURE_PROGRESS_MAX - _lastSecureProgress)); // Unk6 - time till next resource release
                //Out.WriteByte(0);
                //if (SECURE_PROGRESS_MAX == _lastSecureProgress)
                //{
                //    Out.Fill(0xFF, 2);
                //} else
                //{
                //    Out.WriteUInt16((ushort)(SECURE_PROGRESS_MAX - _lastSecureProgress)); // Unk6 - time till next resource release
                //}
                Out.WriteByte(0);
                Out.WriteByte((byte)(MAX_SECURE_PROGRESS - _secureProgress)); // Unk6 - time till next resource release
                Out.WriteUInt16((ushort)MAX_SECURE_PROGRESS); // Unk7 - vertical offset for drawing overlay - Unk6 may not exceed
            }
            else
            {
                Out.Fill(0xFF, 6);
                Out.WriteUInt16(0);
            }

            Out.WriteByte((byte)OwningRealm);
            Out.WriteByte(update ? (byte)1 : (byte)0);
            Out.WriteByte(flagState);
            Out.WriteByte(0);

            if (!announce)
            {
                if (plr != null)
                    plr.SendPacket(Out);
                else foreach (Player player in Region.Players)
                    player.SendPacket(Out);
                return;
            }

            string message = null;
            ChatLogFilters largeFilter = _owningRealm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;
            PacketOut snd = null;
            
            switch (_state)
            {
                //case StateFlags.Unsecure:
                //    message = string.Concat(Name, " is now open for capture !");
                //    break;
                case StateFlags.Contested:
                    if (_lastBroadCastState != _state)
                    {
                        _lastBroadCastState = _state;
                        _lastSecutedState = false;
                        message = string.Concat(Name, " is under assault by the forces of ", GetRealmString(_assaultingRealm), "!");
                    }
                    break;
                case StateFlags.Secure:
                    bool securedState = _secureProgress == MAX_SECURE_PROGRESS;
                    if (_lastBroadCastState != _state || _lastSecutedState != securedState)
                    {
                        _lastBroadCastState = _state;
                        _lastSecutedState = securedState;

                        if (_secureProgress == MAX_SECURE_PROGRESS)
                        {
                            message = string.Concat($"The forces of ", GetRealmString(_owningRealm), " have taken ", Name, "!");
                            snd = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
                            snd.WriteByte(0);
                            snd.WriteUInt16(_owningRealm == Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
                            snd.Fill(0, 10);
                        }
                        else
                        {
                            message = string.Concat($"The forces of ", GetRealmString(_owningRealm), " are securing ", Name, "!");
                        }
                    }
                    break;
            }

            if (plr != null)
                plr.SendPacket(Out);
            else
            {
                foreach (Player player in Region.Players)
                {
                    player.SendPacket(Out); // Objective's state

                    if (string.IsNullOrEmpty(message) || !player.CbtInterface.IsPvp)
                        continue;

                    // Notify RvR flagged players of activity
                    //player.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                    //player.SendLocalizeString(message, largeFilter, Localized_text.CHAT_TAG_DEFAULT);
                    if (snd != null)
                        player.SendPacket(snd);
                }
            }

        }

        /// <summary>
        /// Builds binary flag depending on the objective's current state.
        /// </summary>
        /// <param name="realm">Realm of the player that will get the state</param>
        /// <returns>String constance representation</returns>
        private string GetStateText(Realms realm)
        {
            switch (_state)
            {
                case StateFlags.Secure:
                    return _secureProgress == MAX_SECURE_PROGRESS ? "GENERATING" : "SECURING";
                case StateFlags.Abandoned:
                    return "ABANDONED";
                case StateFlags.Contested:
                    return realm == _owningRealm ? "DEFEND" : "ASSAULT";
                case StateFlags.Unsecure:
                    return "OPEN";
                case StateFlags.Hidden:
                    return "";
                case StateFlags.ZoneLocked:
                    return "LOCKED";
                case StateFlags.Locked:
                    return "SECURED";

            }

            return "UNKNOWN";
        }

        /// <summary>
        /// Builds binary flag depending on the objective's current state.
        /// </summary>
        /// <returns>Bit flags representation</returns>
        private byte GetStateFlags()
        {
            if (FlagState == ObjectiveFlags.ZoneLocked)
                return (byte)ObjectiveFlags.Locked;

            byte flagState = (byte)_state;

            //if (_state == StateFlags.Securing)
            //    flagState += (byte)ObjectiveFlags.ResourceInteraction;

            return flagState;
        }

        /// <summary>
        /// Sends objective diagnostic information to player (gm only).
        /// </summary>
        /// <param name="player">GM to send data to</param>
        /// <param name="bLocalZone">True to display player's local zone, false for tier zones</param>
        public void SendDiagnostic(Player plr)
        {
            plr.SendClientMessage($"[{Name}]", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            plr.SendClientMessage($"{Enum.GetName(typeof(StateFlags), _state)} and held by {(_owningRealm == Realms.REALMS_REALM_NEUTRAL ? "no realm" : (_owningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"))}");

            plr.SendClientMessage($"Control gauge: {_controlGauge}");
            plr.SendClientMessage($"Secure progress: {_secureProgress}");
            plr.SendClientMessage($"Flag status: {_state}");

            _quadrantHistoryTracker.SendDiagnostic(plr);
            
            plr.SendClientMessage($"Order / Destro warcamp distances: {_orderWarcampDistance} / {_destroWarcampDistance}");
        }
        #endregion

        #region Delegates
        /// <summary>
        /// Advances the internal history tables of this objective's quadrant tracker.
        /// </summary>
        /// <param name="orderCount">Total number of orders in the BattleFront lake</param>
        /// <param name="destroCount">Total number of orders in the BattleFront lake</param>
        public void AdvancePopHistory(int orderCount, int destroCount)
            => _quadrantHistoryTracker.AdvancePopHistory(orderCount, destroCount);

        /// <summary>
        /// Registers a player as around the objective.
        /// </summary>
        /// <param name="player">Player around, not null</param>
        public void AddPlayerInQuadrant(Player player)
            => _quadrantHistoryTracker.AddPlayerInQuadrant(player);
        #endregion

        #region Supply Generation

        // This is used to modify the timer of BO Lock - default from Aza system was is below, 
        // TIMER_MODIFIER should be set to 1.0f, currently we are cutting it by half, change it
        // back to 1.0f to restore default value
        private const float TIMER_MODIFIER = 0.5f;

        private int _supplyRespawnTimeMs = (int)(180 * 1000);
        private const int SUPPLY_CLIENT_TIMER_MS = (int)(175 * 1000);

        private readonly List<BattleFrontResourceSpawn> _supplySpawns;

        private ResourceBox _supplies;
        private int _generationTimerEnd;
        private bool GeneratingSupplies => _generationTimerEnd > 0;

        /// <summary>
        /// Sets up the supply box.
        /// </summary>
        private void LoadResources()
        {
            //codeword p0tat02
            /*BattleFrontResourceSpawn destSpawn = _supplySpawns[StaticRandom.Instance.Next(_supplySpawns.Count)];

            Point3D homePos = ZoneService.GetWorldPosition(Zone.Info, (ushort)destSpawn.X, (ushort)destSpawn.Y, (ushort)destSpawn.Z);
            _supplies = new ResourceBox(
                (uint)ID, "Supplies", homePos, (ushort)GameBuffs.ResourceCarrier, (int)(120000 * TIMER_MODIFIER), SuppliesPickedUp, null, SuppliesReset, ResBuffAssigned,
                GameObjectService.GetGameObjectProto(429).DisplayID, GameObjectService.GetGameObjectProto(429).DisplayID)
            {
                Objective = this,
                ColorMatchesRealm = true,
                PreventsRide = false
            };

            Region.AddObject(_supplies, Zone.ZoneId);*/
        }

        /// <summary>
        /// Assigns the visual effects to a player carrying supplies.
        /// </summary>
        /// <param name="b"></param>
        public void ResBuffAssigned(NewBuff b)
        {
            HoldObjectBuff hB = (HoldObjectBuff)b;

            switch (hB.Target.Realm)
            {
                case Realms.REALMS_REALM_ORDER:
                    hB.FlagEffect = FLAG_EFFECTS.Blue;
                    break;
                case Realms.REALMS_REALM_DESTRUCTION:
                    hB.FlagEffect = FLAG_EFFECTS.Red;
                    break;
                default:
                    hB.FlagEffect = FLAG_EFFECTS.Mball1;
                    break;
            }
        }

        public void SuppliesPickedUp(HoldObject holdObject, Player player)
        {
            player.SendClientMessage("You have picked up the supplies!", player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);

            if (_generationTimerEnd > 0)
            {
                _generationTimerEnd = 0;
                holdObject.SetRealmAssociation(0);
                SendFlagState(null, false, false);

                foreach (Player plr in Region.Players)
                {
                    if (plr.CbtInterface.IsPvp)
                        plr.SendClientMessage($"{player.Name} is transporting supplies from {ObjectiveName}!", player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
                }
            }

            else
            {
                lock (player.PlayersInRange)
                    foreach (Player plr in player.PlayersInRange)
                        plr.SendClientMessage($"{player.Name} is now carrying the supplies!", player.Realm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);
            }

        }

        /// <summary>
        /// Begins respawning of the supplies after a reset.
        /// </summary>
        public void SuppliesReset(HoldObject holdObject)
        {
            if (Region.Bttlfront.NoSupplies)
                return;

            if (holdObject.HeldState == EHeldState.Home)
            {
                _generationTimerEnd = TCPManager.GetTimeStamp();
                return;
            }

            if (FlagState == ObjectiveFlags.Locked || FlagState == ObjectiveFlags.Open || FlagState == ObjectiveFlags.Contested)
                StartSupplyRespawnTimer(SupplyEvent.Reset);
        }

        /// <summary>
        /// Begins the process of making the supplies available at the Battlefield Objective.
        /// </summary>
        public void StartSupplyRespawnTimer(SupplyEvent supplyEvent)
        {
            if (Region.Bttlfront.NoSupplies)
                return;

            if (GeneratingSupplies)
            {
                _generationTimerEnd = 0;
                SendFlagState(null, false, false);
            }

            //Region.Bttlfront.SendPairingBroadcast("Resource timer for "+ObjectiveName+" started.", Realms.REALMS_REALM_ORDER);

            switch (supplyEvent)
            {
                case SupplyEvent.Reset:
                    EvtInterface.AddEvent(SendClientSupplyTimer, _supplyRespawnTimeMs - SUPPLY_CLIENT_TIMER_MS, 1);
                    EvtInterface.AddEvent(RespawnSupplies, _supplyRespawnTimeMs, 1);
                    break;

                case SupplyEvent.OwnershipChanged:
                    SendClientSupplyTimer();
                    EvtInterface.AddEvent(RespawnSupplies, SUPPLY_CLIENT_TIMER_MS, 1);
                    break;

                case SupplyEvent.ZoneActiveStatusChanged:
                    int respawnTime = (int)(StaticRandom.Instance.Next(8, 10) * 60 * 1000 * TIMER_MODIFIER);
                    EvtInterface.AddEvent(SendClientSupplyTimer, respawnTime - SUPPLY_CLIENT_TIMER_MS, 1);
                    EvtInterface.AddEvent(RespawnSupplies, respawnTime, 1);
                    break;
            }
        }

        /// <summary>
        /// Sends the outline around the Battlefield Objective flag to the clients.
        /// </summary>
        public void SendClientSupplyTimer()
        {
            if (Region.Bttlfront.NoSupplies)
                return;

            //Region.Bttlfront.SendPairingBroadcast("Client timer for " + ObjectiveName + " sent.", Realms.REALMS_REALM_ORDER);

            _generationTimerEnd = TCPManager.GetTimeStamp() + (int)(175);
            SendFlagState(null, false, false);
        }

        /// <summary>
        /// Renders the supplies active for capturing.
        /// </summary>
        public void RespawnSupplies()
        {
            if (Region.Bttlfront.NoSupplies)
                return;

            if (_supplies == null)
            {
                Log.Error(ObjectiveName + " in " + Zone.Info.Name + " with BattleFront supply block status " + Region.Bttlfront.NoSupplies, "Supplies are null!");
                return;
            }
            //Region.Bttlfront.SendPairingBroadcast("Respawning resource for " + ObjectiveName + ".", Realms.REALMS_REALM_ORDER);
            _supplies.SetActive(_owningRealm);
        }

        /// <summary>
        /// Prevents the supplies from respawning and hides any existing supplies.
        /// </summary>
        public void BlockSupplySpawn()
        {
            EvtInterface.RemoveEvent(SendClientSupplyTimer);
            EvtInterface.RemoveEvent(RespawnSupplies);

            if (GeneratingSupplies)
            {
                _generationTimerEnd = 0;
                SendFlagState(null, false, false);
            }

            if (_supplies == null)
            {
                Log.Error("BattleFront", "NO SUPPLIES AT " + ObjectiveName + " WITH ID " + ID);
                return;
            }

            if (_supplies.HeldState != EHeldState.Inactive)
                _supplies.ResetTo(EHeldState.Inactive);

            _supplies.SetRealmAssociation(Realms.REALMS_REALM_NEUTRAL);
        }

        public void RepairDoorsFromProximityFlag()
        {
            if (WorldMgr.WorldSettingsMgr.GetGenericSetting(10) == 0 && _state == StateFlags.Abandoned)
                return;

            ProximityBattleFront bttlfrnt = ((ProximityBattleFront)Region.Bttlfront);
            ProximityProgressingBattleFront pBttlfrnt = Region.Bttlfront as ProximityProgressingBattleFront;

            if (!bttlfrnt.PairingLocked && (_tier < 4 || (pBttlfrnt != null && ZoneId == pBttlfrnt.Zones[pBttlfrnt._BattleFrontStatus.OpenZoneIndex].ZoneId)))
            {

                // Some zones have bad BO placement - we try to fix this here: Eataine 209
                float zoneModifier = 1.0f;
                if (ZoneId == 209 || ZoneId == 109)
                    zoneModifier = 2.0f;

                float distFactor = 2.0f;

                foreach (Keep keep in Region.Bttlfront.Keeps)
                {
                    if (keep.Realm == OwningRealm && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                    {
                        distFactor = GetDistanceToObject(keep.ResourceReturnFlag) / 2000f * (2.0f / _tier) * zoneModifier;
                    }
                }

                foreach (Keep keep in Region.Bttlfront.Keeps)
                {
                    //Removed requirement of unlocked keep to rank it, should not be possible in zones that are locked...
                    //if (!keep.Ruin && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && OwningRealm == keep.Realm && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                    if (!keep.Ruin && OwningRealm == keep.Realm && (_tier < 4 || (_tier == 4 && ZoneId == keep.ZoneId)))
                    {
                        bool RamDeployed = keep.RamDeployed;

                        // Intercept to check if any doors need repairing
                        foreach (KeepDoor door in keep.Doors)
                        {
                            if (door.GameObject.PctHealth < 100 && !door.GameObject.IsDead && (bttlfrnt.HeldObjectives[(byte)OwningRealm] > 3 || (!RamDeployed && bttlfrnt.HeldObjectives[(byte)OwningRealm] > 1)))
                            {
                                // One box heals 30%.
                                float scaler = WorldMgr.WorldSettingsMgr.GetDoorRegenValue() / 10000f;

                                float healCapability = Math.Min(door.GameObject.MaxHealth * scaler, distFactor * scaler * door.GameObject.MaxHealth); // The boxes are 'spawned' every 6s so we reduce the amount of healing by 20

                                uint currentDamage = door.GameObject.MaxHealth - door.GameObject.Health;

                                float consumeFactor = Math.Min(1f, currentDamage / healCapability);

                                door.GameObject.ReceiveHeal(null, (uint)healCapability);

                                // If resources are not stored in keep, keep continues to derank
                                if (consumeFactor == 1f)
                                {
                                    keep.SendKeepStatus(null);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        private int _keepRankOrder = 0;
        private int _keepRankDestro = 0;
        /// <summary>
        /// GenerateSupplie every 6 seconds
        /// </summary> 
        public void GenerateSuppliesFromProximityFlag()
        {
            
            //{
                // This switch disables resources from abandoned flags, keeps should derank properly
            if (WorldMgr.WorldSettingsMgr.GetGenericSetting(10) == 0 && _state == StateFlags.Abandoned)
                return;

            /*if (OwningRealm == keep.Realm && OwningRealm != Realms.REALMS_REALM_NEUTRAL && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && !keep.Ruin && (_state == StateFlags.Secure || _state == StateFlags.Abandoned))
            {
                ProximityBattleFront bttlfrnt = ((ProximityBattleFront)Region.Bttlfront);

                float resourceValue = bttlfrnt.GetResourceValue(OwningRealm, keep._resourceValueMax[keep.Rank]);
                resourceValue = resourceValue / WorldMgr.WorldSettingsMgr.GetSuppliesScaler(); // New "box" is generated every 6 seconds, not every 120 seconds, that's why we divide it by 20

                if (_state == StateFlags.Abandoned)
                    resourceValue = resourceValue / 2;

                // Some zones have bad BO placement - we try to fix this here: Eataine 209
                float zoneModifier = 1.0f;
                if (ZoneId == 209 || ZoneId == 109)
                    zoneModifier = 2.0f;


                float distFactor = GetDistanceToObject(keep.ResourceReturnFlag) / 2000f * (2.0f / _tier) * zoneModifier;

#if DEBUG
                //resourceValue *= 50;
#endif
                    
                resourceValue *= distFactor;

                // Reload siege weapons
                //keep.ReloadSiege();

                bool RamDeployed = keep.RamDeployed;

                // Intercept to check if any doors need repairing
                if (!keep.Ruin)
                {
                    foreach (KeepDoor door in keep.Doors)
                    {
                        if (door.GameObject.PctHealth < 100 && !door.GameObject.IsDead && (bttlfrnt.HeldObjectives[(byte)OwningRealm] == 4 ||
                            (!RamDeployed && bttlfrnt.HeldObjectives[(byte)OwningRealm] > 1)))
                        {
                            // One box heals 30%.
                            float scaler = WorldMgr.WorldSettingsMgr.GetDoorRegenValue() / 10000f;
#if DEBUG
                            //Log.Info("Scaler: ", scaler.ToString());
#endif

                            float healCapability = Math.Min(door.GameObject.MaxHealth * scaler, distFactor * scaler * door.GameObject.MaxHealth); // The boxes are 'spawned' every 6s so we reduce the amount of healing by 20

                            uint currentDamage = door.GameObject.MaxHealth - door.GameObject.Health;

                            float consumeFactor = Math.Min(1f, currentDamage / healCapability);

                            // codeword 0ni0n, null in first parameter can possibly break something
                            door.GameObject.ReceiveHeal(null, (uint)healCapability);

                            //resourceValue *= 1f - consumeFactor;
                            resourceValue *= 1f - healCapability * 50 / 20;

                            // If resources are not stored in keep, keep continues to derank
                            if (consumeFactor == 1f)
                            {
                                keep.SendKeepStatus(null);
                                return;
                            }
                        }
                    }
                }

                if (keep.Rank < 6 && keep._currentResource + resourceValue >= keep._maxResource)
                //if (RealmRank < 6 && keep._currentResource + resourceValue >= keep._maxResource)
                {
                    if (keep.CanSustainRank(keep.Rank + 1))
                    {
                        resourceValue -= keep._maxResource - keep._currentResource;
                        if (keep.Rank < 5)
                            ++keep.Rank;
                        keep.SetSupplyRequirement();
                        keep._currentResource = resourceValue;
                        Region.Bttlfront.Broadcast($"{keep.Info.Name} is now Rank {keep.Rank}!", OwningRealm);
                        if (keep.Rank == 1)
                        {
                            if (keep.LastMessage >= Keep.KeepMessage.Inner0)
                                Region.Bttlfront.Broadcast($"{keep.Info.Name}'s postern doors are barred once again!", OwningRealm);
                            else if (keep.LastMessage >= Keep.KeepMessage.Outer0)
                                Region.Bttlfront.Broadcast($"{keep.Info.Name}'s outer postern doors are barred once again!", OwningRealm);
                        }
                    }

                    else
                    {
                        keep._currentResource = keep._maxResource - 1;
                    }

                    // We notify everyone that this keep is Rank 1
                    if (keep.Rank == 1)
                    {
                        foreach (Player player in Player._Players)
                        {
                            if (player.Region.GetTier() > 1 && player.ValidInTier(keep.Tier, true) && !keep.InformRankOne && player.Region.RegionId != Region.RegionId)
                            {
                                player.SendLocalizeString($"{keep.Info.Name} keep in {Zone.Info.Name} just reached Rank 1!", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                            }
                        }
                        keep.InformRankOne = true;
                    }
                }

                else
                    keep._currentResource += resourceValue;

                keep._currentResourcePercent = (byte)(keep._currentResource / keep._maxResource * 100f);

                if (Constants.DoomsdaySwitch == 2)
                {
                    foreach (Player player in _closePlayers)
                    {
                        if (player.DebugMode)
                        { 
                            player.SendClientMessage($"Resource worth {resourceValue} ({((ProximityBattleFront)Region.Bttlfront).GetResourceValue(player.Realm, keep._resourceValueMax[keep.Rank])} base * {GetDistanceToObject(keep.ResourceReturnFlag) / 2000f * (2.0f / _tier) * zoneModifier} dist factor) returned!");
                            player.SendClientMessage($"Resources: {keep._currentResource}/{keep._maxResource} ({keep._currentResourcePercent}%)");
                        }
                    }
                }

                EvtInterface.AddEvent(keep.UpdateStateOfTheRealmKeep, 100, 1);
                keep._lastReturnSeconds = TCPManager.GetTimeStamp();
                keep.SendKeepStatus(null);
            }*/

            if (OwningRealm != Realms.REALMS_REALM_NEUTRAL && (_state == StateFlags.Secure))
            { 
                int arr = (int)OwningRealm - 1;

                ProximityBattleFront bttlfrnt = ((ProximityBattleFront)Region.Bttlfront);
                ProximityProgressingBattleFront pBttlfrnt = Region.Bttlfront as ProximityProgressingBattleFront;

                bttlfrnt.SetRealmSupplyRequirement(arr);

                if (!bttlfrnt.PairingLocked && (_tier < 4 || (pBttlfrnt != null && ZoneId == pBttlfrnt.Zones[pBttlfrnt._BattleFrontStatus.OpenZoneIndex].ZoneId)))
                {
                    float resourceValue = bttlfrnt.GetResourceValue(OwningRealm, bttlfrnt._RealmResourceValueMax[bttlfrnt.RealmRank[arr]]);
                    resourceValue = resourceValue / WorldMgr.WorldSettingsMgr.GetSuppliesScaler(); // New "box" is generated every 6 seconds, not every 120 seconds, that's why we divide it by 20

                    // Some zones have bad BO placement - we try to fix this here: Eataine 209
                    float zoneModifier = 1.0f;
                    if (ZoneId == 209 || ZoneId == 109)
                        zoneModifier = 2.0f;

                    float distFactor = 2.0f;

                    foreach (Keep keep in Region.Bttlfront.Keeps)
                    {
                        if (keep.Realm == OwningRealm && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                        { 
                            distFactor = GetDistanceToObject(keep.ResourceReturnFlag) / 2000f * (2.0f / _tier) * zoneModifier;
                        }
                    }

                    resourceValue *= distFactor;

                    byte Rank = (byte)bttlfrnt.RealmRank[arr];
                    float _currentResource = bttlfrnt.RealmCurrentResources[arr];
                    int _maxResource = bttlfrnt.RealmMaxResource[arr];

                    if (bttlfrnt.RealmRank[arr] < 6 && bttlfrnt.RealmCurrentResources[arr] + resourceValue >= bttlfrnt.RealmMaxResource[arr] && !bttlfrnt.PairingLocked)
                    {
                        if (bttlfrnt.CanRealmSustainRank(OwningRealm, bttlfrnt.RealmRank[arr] + 1))
                        {
                            resourceValue -= bttlfrnt.RealmMaxResource[arr] - bttlfrnt.RealmCurrentResources[arr];
                            if (bttlfrnt.RealmRank[arr] < 5)
                                ++bttlfrnt.RealmRank[arr];
                            bttlfrnt.SetRealmSupplyRequirement(arr);
                            bttlfrnt.RealmCurrentResources[arr] = resourceValue;
                            //Region.Bttlfront.Broadcast($"{bttlfrnt.ActiveZoneName} is now Rank {bttlfrnt.RealmRank[arr]}!", OwningRealm);
                        }
                        else
                        {
                            bttlfrnt.RealmCurrentResources[(int)OwningRealm - 1] = bttlfrnt.RealmMaxResource[arr] - 1;
                        }
                    }
                    else
                        bttlfrnt.RealmCurrentResources[(int)OwningRealm - 1] += resourceValue;

                    bttlfrnt.RealmLastReturnSeconds[(int)OwningRealm - 1] = TCPManager.GetTimeStamp();

                    foreach (Keep keep in Region.Bttlfront.Keeps)
                    {
                        //Removed requirement of unlocked keep to rank it, should not be possible in zones that are locked...
                        //if (!keep.Ruin && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && OwningRealm == keep.Realm && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED)
                        if (!keep.Ruin && OwningRealm == keep.Realm && (_tier < 4 || (_tier == 4 && ZoneId == keep.ZoneId)))
                        {
                            keep.Rank = Rank;
                            keep._currentResource = _currentResource;
                            keep._maxResource = _maxResource;

                            bool RamDeployed = keep.RamDeployed;

                            // Intercept to check if any doors need repairing
                            foreach (KeepDoor door in keep.Doors)
                            {
                                if (door.GameObject.PctHealth < 100 && !door.GameObject.IsDead && (bttlfrnt.HeldObjectives[(byte)OwningRealm] > 3 || (!RamDeployed && bttlfrnt.HeldObjectives[(byte)OwningRealm] > 1)))
                                {
                                    // One box heals 30%.
                                    float scaler = WorldMgr.WorldSettingsMgr.GetDoorRegenValue() / 10000f;

                                    float healCapability = Math.Min(door.GameObject.MaxHealth * scaler, distFactor * scaler * door.GameObject.MaxHealth); // The boxes are 'spawned' every 6s so we reduce the amount of healing by 20

                                    uint currentDamage = door.GameObject.MaxHealth - door.GameObject.Health;

                                    float consumeFactor = Math.Min(1f, currentDamage / healCapability);

                                    door.GameObject.ReceiveHeal(null, (uint)healCapability);

                                    resourceValue *= 1f - healCapability * 50 / 20;

                                    // If resources are not stored in keep, keep continues to derank
                                    if (consumeFactor == 1f)
                                    {
                                        keep.SendKeepStatus(null);
                                        return;
                                    }
                                }
                            }

                            //// Here we are ranking or deranking the keep
                            //if (keep.Rank < 6 && keep._currentResource + resourceValue >= keep._maxResource)
                            //{
                            //    if (keep.CanSustainRank(keep.Rank + 1))
                            //    {
                            //        resourceValue -= keep._maxResource - keep._currentResource;
                            //        if (keep.Rank < 5)
                            //            ++keep.Rank;
                            //        keep.SetSupplyRequirement();
                            //        keep._currentResource = resourceValue;
                            //        Region.Bttlfront.Broadcast($"{keep.Info.Name} is now Rank {keep.Rank}!", OwningRealm);
                            //        if (keep.Rank == 1)
                            //        {
                            //            if (keep.LastMessage >= Keep.KeepMessage.Inner0)
                            //                Region.Bttlfront.Broadcast($"{keep.Info.Name}'s postern doors are barred once again!", OwningRealm);
                            //            else if (keep.LastMessage >= Keep.KeepMessage.Outer0)
                            //                Region.Bttlfront.Broadcast($"{keep.Info.Name}'s outer postern doors are barred once again!", OwningRealm);
                            //        }
                            //    }
                            //    else
                            //        keep._currentResource = keep._maxResource - 1;

                            //    // We notify everyone that this keep is Rank 1
                            //    if (keep.Rank == 1)
                            //    {
                            //        foreach (Player player in Player._Players)
                            //        {
                            //            if (player.Region.GetTier() > 1 && player.ValidInTier(keep.Tier, true) && !keep.InformRankOne && player.Region.RegionId != Region.RegionId)
                            //            {
                            //                player.SendLocalizeString($"{keep.Info.Name} keep in {Zone.Info.Name} just reached Rank 1!", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                            //            }
                            //        }
                            //        keep.InformRankOne = true;
                            //    }
                            //}
                            //else
                            //    keep._currentResource += resourceValue;

                            bttlfrnt.RealmRank[arr] = keep.Rank;
                            bttlfrnt.RealmCurrentResources[arr] = keep._currentResource;
                            bttlfrnt.RealmMaxResource[arr] = keep._maxResource;

                            keep._currentResourcePercent = (byte)(keep._currentResource / keep._maxResource * 100f);

                            if (Constants.DoomsdaySwitch == 2)
                            {
                                foreach (Player player in _closePlayers)
                                {
                                    if (player.DebugMode)
                                    {
                                        player.SendClientMessage($"Resource worth {resourceValue} ({((ProximityBattleFront)Region.Bttlfront).GetResourceValue(player.Realm, keep._resourceValueMax[keep.Rank])} base * {GetDistanceToObject(keep.ResourceReturnFlag) / 2000f * (2.0f / _tier) * zoneModifier} dist factor) returned!");
                                        player.SendClientMessage($"Resources: {keep._currentResource}/{keep._maxResource} ({keep._currentResourcePercent}%)");
                                    }
                                }
                            }

                            EvtInterface.AddEvent(keep.UpdateStateOfTheRealmKeep, 100, 1);
                            keep._lastReturnSeconds = TCPManager.GetTimeStamp();
                            keep.SendKeepStatus(null);
                        }

                        /*if (OwningRealm == keep.Realm && OwningRealm != Realms.REALMS_REALM_NEUTRAL && keep.KeepStatus != KeepStatus.KEEPSTATUS_LOCKED && !keep.Ruin && (_state == StateFlags.Secure || _state == StateFlags.Abandoned))
                        {












                        }*/

                        /*else if (keep.Ruin && OwningRealm != Realms.REALMS_REALM_NEUTRAL && (_state == StateFlags.Secure || _state == StateFlags.Abandoned))
                        {
                            float resourceValue = bttlfrnt.GetResourceValue(OwningRealm, keep._resourceValueMax[bttlfrnt.RealmRank[(int)OwningRealm-1]]);
                            resourceValue = resourceValue / WorldMgr.WorldSettingsMgr.GetSuppliesScaler(); // New "box" is generated every 6 seconds, not every 120 seconds, that's why we divide it by 20

                            if (_state == StateFlags.Abandoned)
                                resourceValue = resourceValue / 2;

                            // Some zones have bad BO placement - we try to fix this here: Eataine 209
                            float zoneModifier = 1.0f;
                            if (ZoneId == 209 || ZoneId == 109)
                                zoneModifier = 2.0f;

                            float distFactor = GetDistanceToObject(keep.ResourceReturnFlag) / 2000f * (2.0f / _tier) * zoneModifier;

                            resourceValue *= distFactor;

                            if (bttlfrnt.RealmRank[(int)OwningRealm-1] < 6 && bttlfrnt.RealmCurrentResources[(int)OwningRealm-1] + resourceValue >= keep._maxResource)
                            {
                                if (keep.CanSustainRank(bttlfrnt.RealmRank[(int)OwningRealm - 1] + 1))
                                {
                                    resourceValue -= keep._maxResource - keep._currentResource;
                                    if (bttlfrnt.RealmRank[(int)OwningRealm - 1] < 5)
                                        ++bttlfrnt.RealmRank[(int)OwningRealm - 1];
                                    keep.SetSupplyRequirement((int)OwningRealm);
                                    bttlfrnt.RealmCurrentResources[(int)OwningRealm - 1] = resourceValue;
                                    Region.Bttlfront.Broadcast($"{bttlfrnt.ActiveZoneName} is now Rank {keep.Rank}!", OwningRealm);
                                }
                                else
                                {
                                    bttlfrnt.RealmCurrentResources[(int)OwningRealm - 1] = keep._maxResource - 1;
                                }
                            }

                            else
                                bttlfrnt.RealmCurrentResources[(int)OwningRealm - 1] += resourceValue;

                            //EvtInterface.AddEvent(keep.UpdateStateOfTheRealmKeep, 100, 1);
                            bttlfrnt.RealmLastReturnSeconds[(int)OwningRealm - 1] = TCPManager.GetTimeStamp();
                        }*/
                    }
                }
            }    
        }


    #endregion

        /// <summary>
        /// Grants a reward to each player in the region based on their kill contribution, if any, to this defense over the last 10 minutes.
        /// </summary>
        public void TickDefense(long curTimeSeconds)
        {
            if (_owningRealm == Realms.REALMS_REALM_NEUTRAL)
                return;

            // Defense rewards scale the more objectives are held.
            float defenseBias;
            if (Constants.DoomsdaySwitch == 2)
                defenseBias = ((ProximityBattleFront)Region.Bttlfront).GetDefenseBias(_owningRealm);
            else
                defenseBias = ((BattleFront)Region.Bttlfront).GetDefenseBias(_owningRealm);

            Item_Info medallionInfo = null;

            if (_tier > 1)
            {
                //medallionInfo = ItemService.GetItem_Info((uint)(208399 + (_tier - 1)));
                medallionInfo = ItemService.GetItem_Info((uint)(208402));

                try
                {
                    foreach (Player plr in Region.Players)
                    {
                        if (plr.Realm != _owningRealm || !plr.Loaded)
                            continue;

                        if (plr._Value == null)
                        {
                            Log.Error("TickDefense", "Player " + plr.Name + " with no char values");
                            continue;
                        }

                        // Base reward for being stationed within this area

                        XpRenown curEntry;

                        _delayedRewards.TryGetValue(plr.CharacterId, out curEntry);

                        // Not a contributor.
                        if (curEntry == null)
                            continue;

                        // Not ready to distribute rewards.
                        if (curTimeSeconds < curEntry.LastUpdatedTime)
                        {
#if BattleFront_DEBUG
                        plr.SendClientMessage($"TickDefense: next tick is in {curEntry.LastUpdatedTime - curTimeSeconds}s.");
#endif
                            continue;
                        }

                        // No XP earned here between this defense tick and the last one.
                        if (curEntry.XP == 0)
                        {
                            _delayedRewards.Remove(plr.CharacterId);
                            continue;
                        }

#if BattleFront_DEBUG
                    plr.SendClientMessage($"{ObjectiveName} distributing {curEntry.XP} XP and {curEntry.Renown} with bias scale {defenseBias}");
#endif

                        plr.SendClientMessage($"You've received a reward for your contribution to the holding of {ObjectiveName}.", ChatLogFilters.CHATLOGFILTERS_RVR);
                        plr.AddXp((uint)(curEntry.XP * defenseBias), true, false);
                        plr.AddRenown((uint)(curEntry.Renown * defenseBias), true, RewardType.ObjectiveDefense, ObjectiveName);
                        plr.AddInfluence(curEntry.InfluenceId, (ushort)(curEntry.Influence * defenseBias));

                        Region.Bttlfront.AddContribution(plr, curEntry.Renown);

                        if (medallionInfo != null)
                        {
                            ushort medallionCount = Math.Max((ushort)1, (ushort)(curEntry.Renown / (450 * _tier)));

                            if (plr.ItmInterface.CreateItem(medallionInfo, medallionCount) == ItemResult.RESULT_OK)
                                plr.SendLocalizeString(new[] { medallionInfo.Name, medallionCount.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                        }

                        // Delayed rewards for defense are removed over time, to preserve the tick interval for that player
                        curEntry.XP = 0;
                        curEntry.Renown = 0;
                        curEntry.Influence = 0;

                        curEntry.LastUpdatedTime = curTimeSeconds + DEFENSE_TICK_INTERVAL_SECONDS;
                    }
                }
                catch (Exception e)
                {
                    Log.Error("TickDefense", e.ToString());
                }
            }
        }

        /// <summary>Returns _assaultingRealm and not owning realm if not neutral !</summary>
        [Obsolete("_assaultingRealm")]
        private Realms GetOwningRealm()
        {
            return _assaultingRealm != Realms.REALMS_REALM_NEUTRAL ? _assaultingRealm : OwningRealm;
        }
        
        [Obsolete("Alias for name")]
        public string ObjectiveName { get { return Name; } }
    }
}

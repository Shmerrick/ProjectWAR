using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.World.Battlefronts.Objectives;


namespace WorldServer.World.Battlefronts.NewDawn
{
    public class NewDawnBattlefieldObjective : Object
    {
        private const int DEFENSE_TICK_INTERVAL_SECONDS = 300;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Absolute maximum of the control gauges of the flags
        /// <summary>
        public static int MAX_SECURE_PROGRESS = 80;

        /// <summary>
        /// Absolute maximum of the control gauges of the flags.
        /// Is used as a base timer in milliseconds when securing objectives.
        /// <summary>
        public static int MAX_CONTROL_GAUGE = MAX_SECURE_PROGRESS * 600;

        /// <summary>Maximum players each side taken in consideration for assaults a flag</summary>
        public static short MAX_CLOSE_PLAYERS = 6;

        /// <summary>The tier within which the battlefront exists.</summary>
        public readonly byte _tier;

        /// <summary>Battlefront objective id</summary>
        public readonly int Id;

        /// <summary>The zone info id within which the battlefront exists.</summary>
        public new readonly ushort ZoneId;

        /// <summary>Influence area containing the objective</summary>
        private Zone_Area _area;

        /// <summary>
        ///     Numbers of players close to the flag who can influence the state of the flag (0/1 for o/d), limited to
        ///     CLOSE_PLAYERS_MAX
        /// </summary>
        private volatile short _closeOrderCount, _closeDestroCount;

        
        /// <summary>Set of all players in close range, not limited</summary>
        private ISet<Player> _closePlayers = new HashSet<Player>();

        /// <summary>For memory optimization</summary>
        private ISet<Player> _closePlayersBuffer = new HashSet<Player>();

        /// <summary>
        ///     Main control jauge in milliseconds :
        ///     -CONTROL_GAUGE_MAX : generating resources for order
        ///     -CONTROL_GAUGE_MAX to CONTROL_GAUGE_MAX : transition
        ///     CONTROL_GAUGE_MAX : generating resources for destro
        /// </summary>
        /// <remarks>
        ///     This is the base value when computing transition timer.
        /// </remarks>
        private int _controlGauge;

        /// <summary>Displayed timer in seconds</summary>
        private short _displayedTimer;

        /// <summary>True if a player is threatening players (opposite of secure realm)</summary>
        private bool _hasThreateningPlayer;

        private StateFlags _lastBroadCastState = StateFlags.Hidden;

        private long _lastGaugeUpdateTick;
        private bool _lastSecutedState;


        /// <summary>Absolute transition speed last time it was recomputed</summary>
        private float _lastTransitionUpdateSpeed;

        /// <summary>Expected timestamp of next owner change
        ///     <summary>
        public long _nextTransitionTimestamp;

        private long _nextUpdateTick;
        private short _playersInRangeCount;

        /// <summary>Positive between 0 and SECURE_PROGRESS_MAX indicating the objective securisation indicator, in seconds
        ///     <summary>
        private int _secureProgress;

        private readonly uint _tokdiscovery; // This is for ToK unlocks
        private readonly uint _tokunlocked; // This is for ToK unlocks

        private readonly uint _x; // why other attributes ?
        private readonly uint _y; // why other attributes ?
        private readonly ushort _z; // why other attributes ?
        private readonly ushort _o; // why other attributes ?

        public uint AccumulatedKills;

        /// <summary> This marks BO flags that are created inside ruins</summary>
        public bool Ruin = false;
        private QuadrantHistoryTracker _quadrantHistoryTracker;


        public NewDawnCommunications CommsEngine { get; set; }
        public ProximityEngine ProximityEngine { get; set; }
        public NewDawnBattlefront Battlefront { get; set; }
        public string Name { get; set; }

        public StateFlags State { get; set; }

        /// <summary>Gets the currently owning realm, may be neutral.</summary>
        public Realms OwningRealm { get; set; }

        private Realms AssaultingRealm { get; set; }

        public RVRRewardManager RewardManager { get; set; }

        #region Helpers
        /*
         * HELPER Methods
         */
        internal bool HasThreateningPlayer => _hasThreateningPlayer;

        public ObjectiveFlags FlagState // TODO clean up
            => (ObjectiveFlags)State;

        #endregion

        /// <summary>
        /// Constructor to assist in isolation testing.
        /// </summary>
        public NewDawnBattlefieldObjective()
        {
            CommsEngine = new NewDawnCommunications();
            ProximityEngine = new ProximityEngine();
            RewardManager = new RVRRewardManager();
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="obj"></param>
        public NewDawnBattlefieldObjective(Battlefront_Objective obj, int Tier)
        {
            Id = obj.Entry;
            Name = obj.Name;
            ZoneId = obj.ZoneId;
            _tier = (byte)Tier;

            _x = (uint)obj.X;
            _y = (uint)obj.Y;
            _z = (ushort)obj.Z;
            _o = (ushort)obj.O;
            _tokdiscovery = obj.TokDiscovered;
            _tokunlocked = obj.TokUnlocked;

            Heading = _o;
            WorldPosition.X = (int)_x;
            WorldPosition.Y = (int)_y;
            WorldPosition.Z = _z;

            CommsEngine = new NewDawnCommunications();

            ProximityEngine = new ProximityEngine();

            RewardManager = new RVRRewardManager();
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
            EvtInterface.AddEvent(CheckPlayersInCloseRange, 5000, 0);

            // Initial state
            IsActive = true;


            //if (!Ruin)
            //    _objectivePortalMgr.ObjectiveUnlocked(); // Updates portals

            ////if (_supplySpawns != null && _supplySpawns.Count > 0 && _tier > 1)
            ////    LoadResources();
        }

        public override string ToString()
        {
            return $"Objective : {this.Name} Owner : {this.OwningRealm} Close (O/D) : {this._closeOrderCount}/{this._closeDestroCount}";
        }

        /// <summary>
        ///  For this objective, reward close players with a tick.
        /// </summary>
        /// <returns></returns>
        public VictoryPoint RewardCaptureTick(float pairingRewardScaler)
        {
            if (_secureProgress != BattlefrontConstants.MAX_SECURE_PROGRESS)
                return new VictoryPoint(0, 0);
            if (_closeOrderCount > 0 == _closeDestroCount > 0)
                return new VictoryPoint(0, 0); // Both sides have players in range, or none of them -> not fully secured


            //var scaleMultiplier = this.RewardManager.CalculateRewardScaleMultipler(
            //    _closeOrderCount, 
            //    _closeDestroCount,
            //    OwningRealm, 
            //    _secureProgress,
            //    objectiveRewardScaler);

            
            var objectiveRewardScaler = this.RewardManager.CalculateObjectiveRewardScale(OwningRealm, _closeOrderCount, _closeDestroCount);

            // Scalers in this model are additive. 

            return this.RewardManager.RewardCaptureTick(_closePlayers, 
                OwningRealm, 
                _tier, 
                Name,
                objectiveRewardScaler + pairingRewardScaler);
        }


       


        public bool FlagActive()
        {
            return FlagState != ObjectiveFlags.Locked && FlagState != ObjectiveFlags.ZoneLocked &&
                   OwningRealm != Realms.REALMS_REALM_NEUTRAL;
        }

        public bool CheckKillValid(Player player)
        {
            if (FlagActive() && _playersInRangeCount > 4 && Get2DDistanceToObject(player) < 200)
            {
                if (player.Realm != OwningRealm)
                    AccumulatedKills++;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Gets the realm that have actually secured to objective.
        /// </summary>
        /// <returns>Realm or n</returns>
        public Realms GetSecureRealm()
        {
            if (State != StateFlags.ZoneLocked && State != StateFlags.Secure || _secureProgress < MAX_SECURE_PROGRESS)
                return Realms.REALMS_REALM_NEUTRAL;
            return OwningRealm;
        }

        private void UpdateGauge(long tick)
        {
            if (State == StateFlags.ZoneLocked)
                return;

            var frnt = Battlefront;
            if (frnt != null && frnt.PairingLocked)
                return;
            
            // Apply previously computed transition speed since last update
            if (_lastTransitionUpdateSpeed != 0)
            {
                _controlGauge += (int)((tick - _lastGaugeUpdateTick) * _lastTransitionUpdateSpeed);
                _controlGauge = Clamp(_controlGauge, -MAX_CONTROL_GAUGE, MAX_CONTROL_GAUGE);
            }
            // If flag was unsecured by any realm, it lose control until it reachs zero
            else if (State == StateFlags.Unsecure)
            {
                var wasOrder = _controlGauge < 0;
                float defaultTransitionSpeed = -Clamp(_controlGauge, -1, 1);
                _controlGauge += (int)((tick - _lastGaugeUpdateTick) * defaultTransitionSpeed);
                if (wasOrder != _controlGauge < 0) // If reached zero
                    _controlGauge = 0;

            }

            var newTransitionSpeed = GetNewTransitionSpeed();
            var incomingSecureProgress = Math.Abs(_controlGauge) * MAX_SECURE_PROGRESS / MAX_CONTROL_GAUGE;

            var announce = true;

            if (newTransitionSpeed == 0f
            ) // Status quo or fully secured || (OwningRealm == Realms.REALMS_REALM_DESTRUCTION) == assaultSpeed > 0
            {
                // _controlGauge = CONTROL_GAUGE_MAX;
                AssaultingRealm = Realms.REALMS_REALM_NEUTRAL;

                if (incomingSecureProgress != MAX_SECURE_PROGRESS) // Intermediate state
                {
                    OwningRealm = Realms.REALMS_REALM_NEUTRAL;
                    State = StateFlags.Unsecure;
                }
                else if (newTransitionSpeed == 0 && _closeOrderCount == _closeDestroCount) // Abandonned
                {
                    OwningRealm = Realms.REALMS_REALM_NEUTRAL;
                    State = StateFlags.Unsecure;
                }
                else { 

                    if (Ruin)
                        foreach (var keep in Region.Bttlfront.Keeps)
                            if (Id == keep.Info.KeepId)
                            {
                                keep.Realm = OwningRealm;
                                keep.SendKeepStatus(null);
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
                var toOrder = newTransitionSpeed < 0f;
                var isOrder = _controlGauge != 0 ? _controlGauge < 0f : toOrder;

                // Updates owning and assault teams
                AssaultingRealm = Realms.REALMS_REALM_NEUTRAL;
                if (isOrder)
                {
                    OwningRealm = Realms.REALMS_REALM_ORDER;
                    if (!toOrder)
                        AssaultingRealm = Realms.REALMS_REALM_DESTRUCTION;
                }
                else
                {
                    OwningRealm = Realms.REALMS_REALM_DESTRUCTION;
                    if (toOrder)
                        AssaultingRealm = Realms.REALMS_REALM_ORDER;
                }

                // Computes timers
                int remainingTransitionTime; // LastUpdatedTime in millis until next update
                if (toOrder == isOrder) // Securing
                {
                    State = StateFlags.Secure;
                    remainingTransitionTime = MAX_CONTROL_GAUGE;
                    if (toOrder)
                        remainingTransitionTime += _controlGauge;
                    else
                        remainingTransitionTime -= _controlGauge;
                    remainingTransitionTime = (int)Math.Abs(remainingTransitionTime / newTransitionSpeed);
                    _displayedTimer =
                        (short)(remainingTransitionTime / 1000); // Full secure time - already secured time
                }
                else // Assaulting
                {
                    State = StateFlags.Contested;
                    remainingTransitionTime = (int)Math.Abs(_controlGauge / newTransitionSpeed);
                    _displayedTimer =
                        (short)((Math.Abs(MAX_CONTROL_GAUGE / newTransitionSpeed) + remainingTransitionTime) /
                                 1000); // Full secure time + remaining assault time
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
        Log.Info("OwningRealm", OwningRealm.ToString());
        Log.Info("AssaultingRealm", AssaultingRealm.ToString());
#endif

            // This is for State of the Realms addon
            //UpdateStateOfTheRealmBO();

            BroadcastFlagInfo(announce);
        }


        /// <summary>
        ///     Conputes the assaulting speed depending of players in close range.
        /// </summary>
        /// <returns>
        ///     Timer accelerator :
        ///     -2 to -1 for order assault
        ///     -0 for no assault
        ///     1 to 2 for destro assault
        /// </returns>
        private float GetNewTransitionSpeed()
        {
            var diff = _closeDestroCount - _closeOrderCount;
            if (diff == 0)
                return 0f; // no assault or assault stopped
            if (diff > 0 && _controlGauge == MAX_CONTROL_GAUGE)
                return 0f; // Already fully controlled by destro
            if (diff < 0 && _controlGauge == -MAX_CONTROL_GAUGE)
                return 0f; // Already fully controlled by order

            float speed;
            if (diff > 0)
                speed = 1f + (diff - 1) / 5f;
            else
                speed = -1f + (diff + 1) / 5f;
            return speed;
        }


        

        public override void AddInRange(Object obj)
        {
            var plr = obj as Player;
            if (plr != null && plr.ValidInTier(_tier, true))
            {
                SendFlagInfo(plr);
                plr.CurrentObjectiveFlag = this;
                ++_playersInRangeCount;

                if (_tokdiscovery > 0)
                    plr.TokInterface.AddTok((ushort)_tokdiscovery);
                if (_tokunlocked > 0)
                    plr.TokInterface.AddTok((ushort)_tokunlocked);

                base.AddInRange(obj);
            }
        }

        public override void RemoveInRange(Object obj)
        {
            var plr = obj as Player;
            if (plr != null)
            {
                CommsEngine.SendFlagLeft(plr, Id);
                if (plr.CurrentObjectiveFlag == this)
                    plr.CurrentObjectiveFlag = null;
                --_playersInRangeCount;
            }

            base.RemoveInRange(obj);
        }

        /// Checks where are the players in range to update their count depending on distances.
        /// </summary>
        private void CheckPlayersInCloseRange()
        {
            if (State == StateFlags.ZoneLocked)
                return;

            short orderCount = 0;
            short destroCount = 0;
            var closeHeight = 70 / 2 * UNITS_TO_FEET;
            var threatenHeight = 200 / 2 * UNITS_TO_FEET;

            _hasThreateningPlayer = false;

            foreach (var player in PlayersInRange)
            {
                if (player.IsDead || player.StealthLevel != 0 || !player.CbtInterface.IsPvp || player.IsInvulnerable)
                    continue;

                var distance = GetDistanceToObject(player);
                var heightDiff = Math.Abs(Z - player.Z);
                if (distance < 70 && heightDiff < closeHeight)
                {
                    if (player.Realm == Realms.REALMS_REALM_ORDER)
                        orderCount++;
                    else
                        destroCount++;
                    _closePlayersBuffer.Add(player);
                }

                // Updates the threatening flag if was not already set
                _hasThreateningPlayer |= player.Realm != OwningRealm // One player of opposite realm
                                         && distance < 200 && heightDiff < threatenHeight; // under threaten range
            }

            // Switch the players sets
            var previous = _closePlayers;
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
        }

        /// <summary>
        ///     Update thread.
        /// </summary>
        /// <param name="tick"></param>
        public override void Update(long tick)
        {
            EvtInterface.Update(tick);
            if (tick < _nextUpdateTick)
                return;

            UpdateGauge(tick);
        }

        /// <summary>
        ///     Allows this objective to be captured if it was previously locked.
        /// </summary>
        public void UnlockObjective()
        {
            State = StateFlags.Unsecure;
            OwningRealm = Realms.REALMS_REALM_NEUTRAL;
            BroadcastFlagInfo(false);
        }

        /// <summary>
        ///     Broadcasts flag state to all players withing range.
        /// </summary>
        /// <param name="announce">True </param>
        public void BroadcastFlagInfo(bool announce)
        {
            foreach (var plr in PlayersInRange) // probably synchronization bug
            {
                SendMeTo(plr);

                SendFlagInfo(plr);
            }
        }

        /// <summary>
        ///     Sends objective's detailed info in upper right corner of the screen.
        /// </summary>
        /// <param name="plr"></param>
        public void SendFlagInfo(Player plr)
        {
            // return;
            var owningRealm = OwningRealm;
            var assaultingRealm = AssaultingRealm;

            if (_tier == 4)
            {
                var prFrnt = (ProximityProgressingBattlefront)Region.Bttlfront;
                if (prFrnt != null && ZoneId != prFrnt.Zones[prFrnt._battlefrontStatus.OpenZoneIndex].ZoneId)
                {
                    owningRealm = OwningRealm;
                    assaultingRealm = OwningRealm;
                }
            }

            // Log.Info("Name", ID.ToString());
            // Log.Info("OwningRealm", Enum.GetName(typeof(Realms), owningRealm));
            // Log.Info("AssaultingRealm", Enum.GetName(typeof(Realms), assaultingRealm));

            var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO, 32);
            Out.WriteUInt32((uint)Id);
            Out.WriteByte(0);
            Out.WriteByte((byte)owningRealm); //(byte)OwningRealm
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WritePascalString(Name);
            // 
            // 
            // 
            Out.WriteByte(2);
            Out.WriteUInt32(0x0000348F);
            Out.WriteByte((byte)assaultingRealm); // (byte)AssaultingRealm

            // Expansion for objective goal
            Out.WriteByte(0);

            Out.WriteUInt16(0xFF00);
            Out.WritePascalString(GetStateText(plr.Realm));
            Out.WriteByte(0);

            switch (State)
            {
                case StateFlags.ZoneLocked:
                case StateFlags.Locked:
                    Out.WritePascalString("This area has been captured by ", GetRealmString(OwningRealm),
                        ". The battle wages on elsewhere!");
                    break;

                case StateFlags.Contested:
                    if (plr.Realm != OwningRealm)
                        Out.WritePascalString("This Battlefield Objective is being assaulted by ",
                            GetRealmString(AssaultingRealm),
                            ". Ensure the timer elapses to claim it for your Realm!");
                    else
                        Out.WritePascalString("This Battlefield Objective is being assaulted by ",
                            GetRealmString(AssaultingRealm),
                            ". Reclaim this Battlefield Objective for ", GetRealmString(plr.Realm), "!");
                    break;

                case StateFlags.Secure:
                    if (plr.Realm == OwningRealm)
                        Out.WritePascalString("This Battlefield Objective is generating resources for ",
                            GetRealmString(OwningRealm),
                            ". Defend the flag from enemy assault!");
                    else
                        Out.WritePascalString("This Battlefield Objective is generating resources for ",
                            GetRealmString(OwningRealm),
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
            var transitionTimer = _nextTransitionTimestamp == 0
                ? (ushort)0
                : (ushort)((_nextTransitionTimestamp - TCPManager.GetTimeStampMS()) / 1000);
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

        /// <summary>
        ///     Builds binary flag depending on the objective's current state.
        /// </summary>
        /// <param name="realm">Realm of the player that will get the state</param>
        /// <returns>String constance representation</returns>
        private string GetStateText(Realms realm)
        {
            switch (State)
            {
                case StateFlags.Secure:
                    return _secureProgress == MAX_SECURE_PROGRESS ? "GENERATING" : "SECURING";
                case StateFlags.Abandoned:
                    return "ABANDONED";
                case StateFlags.Contested:
                    return realm == OwningRealm ? "DEFEND" : "ASSAULT";
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
        ///     Sends the flag object to given player.
        /// </summary>
        /// <param name="plr">Player that entered the objective area</param>
        public override void SendMeTo(Player plr)
        {
            var Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC, 64);
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
        ///     Builds binary flag depending on the objective's current state.
        /// </summary>
        /// <returns>Bit flags representation</returns>
        private byte GetStateFlags()
        {
            if (FlagState == ObjectiveFlags.ZoneLocked)
                return (byte)ObjectiveFlags.Locked;

            var flagState = (byte)State;

            //if (State == StateFlags.Securing)
            //    flagState += (byte)ObjectiveFlags.ResourceInteraction;

            return flagState;
        }

        /// <summary>
        ///     Sends the state of the objective in user's map, can announce state changing.
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="announce"></param>
        /// <param name="update"></param>
        public void SendFlagState(Player plr, bool announce, bool update = true)
        {
            if (!Loaded)
                return;

            var flagState = GetStateFlags();

            var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
            Out.WriteUInt32((uint)Id);

            if (
                State == StateFlags
                    .Contested /*|| (State == StateFlags.Secure && MAX_SECURE_PROGRESS == _secureProgress)*/)
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
            else if (State == StateFlags.Secure)
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
                Out.WriteUInt16(
                    (ushort)MAX_SECURE_PROGRESS); // Unk7 - vertical offset for drawing overlay - Unk6 may not exceed
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
                else
                    foreach (var player in Region.Players)
                        player.SendPacket(Out);
                return;
            }

            string message = null;
            var largeFilter = OwningRealm == Realms.REALMS_REALM_ORDER
                ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE
                : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;
            PacketOut snd = null;

            switch (State)
            {
                //case StateFlags.Unsecure:
                //    message = string.Concat(Name, " is now open for capture !");
                //    break;
                case StateFlags.Contested:
                    if (_lastBroadCastState != State)
                    {
                        _lastBroadCastState = State;
                        _lastSecutedState = false;
                        message = string.Concat(Name, " is under assault by the forces of ",
                            GetRealmString(AssaultingRealm), "!");
                    }
                    break;
                case StateFlags.Secure:
                    var securedState = _secureProgress == MAX_SECURE_PROGRESS;
                    if (_lastBroadCastState != State || _lastSecutedState != securedState)
                    {
                        _lastBroadCastState = State;
                        _lastSecutedState = securedState;

                        if (_secureProgress == MAX_SECURE_PROGRESS)
                        {
                            message = string.Concat($"The forces of ", GetRealmString(OwningRealm), " have taken ",
                                Name, "!");
                            snd = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
                            snd.WriteByte(0);
                            snd.WriteUInt16(OwningRealm == Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
                            snd.Fill(0, 10);
                        }
                        else
                        {
                            message = string.Concat($"The forces of ", GetRealmString(OwningRealm), " are securing ",
                                Name, "!");
                        }
                    }
                    break;
            }

            if (plr != null)
                plr.SendPacket(Out);
            else
                foreach (var player in Region.Players)
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

        /// <summary>Computes a string for a realm.</summary>
        /// <param name="realm">To compute string for, should not be neutral</param>
        /// <returns>"Order" / "Destruction"</returns>
        private static string GetRealmString(Realms realm)
        {
            return realm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction";
        }

        /// <summary>
        ///     Sends objective diagnostic information to player (gm only).
        /// </summary>
        /// <param name="player">GM to send data to</param>
        /// <param name="bLocalZone">True to display player's local zone, false for tier zones</param>
        public void SendDiagnostic(Player player)
        {
            player.SendClientMessage($"[{Name}]", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            player.SendClientMessage(
                $"{Enum.GetName(typeof(StateFlags), State)} and held by {(OwningRealm == Realms.REALMS_REALM_NEUTRAL ? "no realm" : (OwningRealm == Realms.REALMS_REALM_ORDER ? "Order" : "Destruction"))}");

            player.SendClientMessage($"Control gauge: {_controlGauge}");
            player.SendClientMessage($"Secure progress: {_secureProgress}");
            player.SendClientMessage($"Flag status: {State}");

            //_quadrantHistoryTracker.SendDiagnostic(player);

            //player.SendClientMessage($"Order / Destro warcamp distances: {_orderWarcampDistance} / {_destroWarcampDistance}");
        }


        /// <summary>
        ///     Prevents this objective from being captured.
        /// </summary>
        public void LockObjective(Realms lockingRealm, bool announce)
        {
            _logger.Debug($"Locking Objective {Name} for {lockingRealm.ToString()}");

            OwningRealm = lockingRealm;
            AssaultingRealm = Realms.REALMS_REALM_NEUTRAL;
            State = StateFlags.ZoneLocked;
            _nextTransitionTimestamp = 0;
            AccumulatedKills = 0;
            _controlGauge = lockingRealm == Realms.REALMS_REALM_ORDER ? -MAX_CONTROL_GAUGE : MAX_CONTROL_GAUGE;
            _lastGaugeUpdateTick = 0;
            _lastTransitionUpdateSpeed = 0;
            _secureProgress = MAX_SECURE_PROGRESS;
            _displayedTimer = 0;

            _closeOrderCount = 0;
            _closeDestroCount = 0;
            _closePlayers.Clear();

            if (!announce)
                return;

            BroadcastFlagInfo(false);
        }

        /// <summary>
        /// Get rewards for holding a flag when keep falls.
        /// </summary>
        public void GrantKeepCaptureRewards()
        {
            foreach (Player plr in PlayersInRange)
            {
                if (plr.Realm == this.OwningRealm && plr.ValidInTier(_tier, true))
                {
                    if (AccumulatedKills < 3)
                    {
                        plr.SendLocalizeString("Defending this objective was a noble duty. You have received a small reward for your service.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + _tier, 1);
                    }
                    else if (AccumulatedKills <= 6)
                    {
                        plr.SendLocalizeString("Your defense of this objective has been noteworthy! You have received a moderate reward.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + _tier, 2);
                    }
                    else if (AccumulatedKills > 6)
                    {
                        plr.SendLocalizeString("Your defense of this objective has been heroic! You have received a respectable reward.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + _tier, 3);
                    }
                }
            }

            AccumulatedKills = 0;
        }

        #region Delegates
        /// <summary>
        /// Advances the internal history tables of this objective's quadrant tracker.
        /// </summary>
        /// <param name="orderCount">Total number of orders in the battlefront lake</param>
        /// <param name="destroCount">Total number of orders in the battlefront lake</param>
        public void AdvancePopHistory(int orderCount, int destroCount)
            => _quadrantHistoryTracker.AdvancePopHistory(orderCount, destroCount);

        /// <summary>
        /// Registers a player as around the objective.
        /// </summary>
        /// <param name="player">Player around, not null</param>
        public void AddPlayerInQuadrant(Player player)
            => _quadrantHistoryTracker.AddPlayerInQuadrant(player);
        #endregion


        
    }
}
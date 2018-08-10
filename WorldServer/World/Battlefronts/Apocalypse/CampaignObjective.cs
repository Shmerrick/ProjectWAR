using Common;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SystemData;
using WorldServer.Scenarios.Objects;
using WorldServer.World.BattleFronts.Objectives;

namespace WorldServer.World.Battlefronts.Apocalypse
{
	public class CampaignObjective : Object
    {
        private const int DEFENSE_TICK_INTERVAL_SECONDS = 300;
        private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");

		/// <summary>
		/// Absolute maximum of the control gauges of the flags
		/// <summary>
		//public static int MAX_SECURE_PROGRESS = 80;
		public static int MAX_SECURE_PROGRESS = 100;
		
		/// <summary>
		/// Absolute maximum of the control gauges of the flags.
		/// Is used as a base timer in milliseconds when securing objectives.
		/// <summary>
		public static int MAX_CONTROL_GAUGE = MAX_SECURE_PROGRESS * 200;

		public static int CONTESTED_TIMESPAN = 60; //300; // 5 min contested
		public static int SECURED_TIMESPAN = 60; //900; // 15 min secured

		private int _stopWatch_Mode = 0;
		private Stopwatch _stopWatch = null;

		private volatile bool _captureInProgress = false;

		/// <summary>Maximum players each side taken in consideration for assaults a flag</summary>
		public static short MAX_CLOSE_PLAYERS = 6;

        /// <summary>The tier within which the Campaign exists.</summary>
        public readonly byte Tier;

        /// <summary>Campaign objective id</summary>
        public readonly int Id;

        /// <summary>The zone info id within which the Campaign exists.</summary>
        public new readonly ushort ZoneId;

        /// <summary> The region id within which the Campaign exists.</summary>
        public readonly ushort RegionId;

        /// <summary>Influence area containing the objective</summary>
        private Zone_Area _area;

        /// <summary>
        ///     Numbers of players close to the flag who can influence the state of the flag (0/1 for o/d), limited to
        ///     CLOSE_PLAYERS_MAX
        /// </summary>
        private volatile short _closeOrderCount, _closeDestroCount;

		private volatile short _nearOrderCount, _nearDestroCount;

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
        private int _displayedTimer;

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

        public ApocCommunications CommsEngine { get; set; }
        public ProximityEngine ProximityEngine { get; set; }
        public Campaign BattleFront { get; set; }
        public string Name { get; set; }

		public StateFlags State { get; set; }

        /// <summary>Gets the currently owning realm, may be neutral.</summary>
        public Realms OwningRealm { get; set; }

        private Realms AssaultingRealm { get; set; }

        public RVRRewardManager RewardManager { get; set; }

		private List<FlagGuard> Guards = new List<FlagGuard>();

		private static bool _allowLockTimer = true;

		#region Helpers

		/*
         * HELPER Methods
         */
		internal bool HasThreateningPlayer => _hasThreateningPlayer;

        #endregion Helpers

        /// <summary>
        /// Constructor to assist in isolation testing.
        /// </summary>
        public CampaignObjective()
        {
            CommsEngine = new ApocCommunications();
            ProximityEngine = new ProximityEngine();
            RewardManager = new RVRRewardManager();
        }

        /// <summary>
        /// Constructor to assist in testing - dont use in production.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="zoneId"></param>
        /// <param name="regionId"></param>
        /// <param name="tier"></param>
        public CampaignObjective(int id, string name, int zoneId, int regionId, int tier)
        {
            Id = id;
            Name = name;
            ZoneId = (ushort)zoneId;
            RegionId = (ushort)regionId;
            Tier = (byte)tier;
            State = StateFlags.ZoneLocked;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="tier"></param>
        public CampaignObjective(RegionMgr region, BattleFront_Objective objective)
        {
            Id = objective.Entry;
            Name = objective.Name;
            ZoneId = objective.ZoneId;
            RegionId = objective.RegionId;
            Tier = (byte)region.GetTier();

            _x = (uint)objective.X;
            _y = (uint)objective.Y;
            _z = (ushort)objective.Z;
            _o = (ushort)objective.O;
            _tokdiscovery = objective.TokDiscovered;
            _tokunlocked = objective.TokUnlocked;

            State = StateFlags.ZoneLocked;
			CaptureDuration = 10;

			Heading = _o;
            WorldPosition.X = (int)_x;
            WorldPosition.Y = (int)_y;
            WorldPosition.Z = _z;

            CommsEngine = new ApocCommunications();

            ProximityEngine = new ProximityEngine();

            RewardManager = new RVRRewardManager();

			if (objective.Guards != null)
			{
				foreach (BattleFront_Guard Guard in objective.Guards)
				{
					Guards.Add(new FlagGuard(region, objective.ZoneId, Guard.OrderId, Guard.DestroId, Guard.X, Guard.Y, Guard.Z, Guard.O));
				}
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
            return $"Objective : {this.Name} \t Status : {State} \t Owner : {this.OwningRealm} \t Close players (O/D) : {this._closeOrderCount}/{this._closeDestroCount}";
        }

        /// <summary>
        ///  For this objective, reward close players with a tick.
        /// </summary>
        /// <returns></returns>
        public VictoryPoint RewardCaptureTick(float pairingRewardScaler)
        {
            if (_secureProgress != BattleFrontConstants.MAX_SECURE_PROGRESS)
                return new VictoryPoint(0, 0);
            if (_closeOrderCount > 0 == _closeDestroCount > 0)
                return new VictoryPoint(0, 0); // Both sides have players in range, or none of them -> not fully secured

			// Scalers in this model are additive.
			return this.RewardManager.RewardCaptureTick(_closePlayers,
                OwningRealm,
                Tier,
                Name,
                pairingRewardScaler);
        }

        public bool FlagActive()
        {
            return this.State != StateFlags.ZoneLocked && this.State != StateFlags.Locked && OwningRealm != Realms.REALMS_REALM_NEUTRAL;
        }

        public bool CheckKillValid(Player player)
        {
            if (FlagActive() && Get2DDistanceToObject(player) < 200)
            {
                if (player.Realm != OwningRealm)
                    AccumulatedKills++;
                return true;
            }

            return false;
        }

		private void SpawnAllGuards(Realms owner)
		{
			if (owner == Realms.REALMS_REALM_DESTRUCTION || owner == Realms.REALMS_REALM_ORDER)
			{
				if (Guards != null)
					foreach (FlagGuard guard in Guards)
						guard.SpawnGuard((int)owner);
			}
		}

		private void DespawnAllGuards()
		{
			if (Guards != null)
				foreach (FlagGuard guard in Guards)
					guard.DespawnGuard();
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

            var frnt = BattleFront;
            if (frnt != null && frnt.IsBattleFrontLocked())
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
				{
					_controlGauge = 0;
				}
            }

            var newTransitionSpeed = GetNewTransitionSpeed();
            var incomingSecureProgress = Math.Abs(_controlGauge) * MAX_SECURE_PROGRESS / MAX_CONTROL_GAUGE;

            var announce = true;

            if (newTransitionSpeed == 0f) // Status quo or fully secured || (OwningRealm == Realms.REALMS_REALM_DESTRUCTION) == assaultSpeed > 0
            {
                // _controlGauge = CONTROL_GAUGE_MAX;
                AssaultingRealm = Realms.REALMS_REALM_NEUTRAL;

                //if (incomingSecureProgress != MAX_SECURE_PROGRESS) // Intermediate state
                //{
                //    OwningRealm = Realms.REALMS_REALM_NEUTRAL;
                //    State = StateFlags.Unsecure;
                //    SendState(GetPlayer(), announce, true);
                //}

                ////Objective changes back to secured if attackers repelled
                //if (State == StateFlags.Contested)
                //{
                //    if (_closeOrderCount == 0 || _closeDestroCount == 0)
                //    {
                //        State = StateFlags.Secure;
                //        SendState(GetPlayer(), announce, true);
                //    }
                //}

     //           //Sets flag state to contested
     //           if (_closeOrderCount != 0 && _closeDestroCount != 0)
     //           {   
     //               if (OwningRealm == Realms.REALMS_REALM_ORDER)
     //               {
     //                   AssaultingRealm = Realms.REALMS_REALM_DESTRUCTION;
     //               }
     //               else
     //               {
     //                   AssaultingRealm = Realms.REALMS_REALM_ORDER;
     //               }
     //               State = StateFlags.Contested;
     //               SendState(GetPlayer(), announce, true);
     //           }
     //           else if (newTransitionSpeed == 0 && _closeOrderCount == _closeDestroCount) // Abandonned
     //           {
					//if (State == StateFlags.Secure)
					//	DespawnAllGuards();

					//OwningRealm = Realms.REALMS_REALM_NEUTRAL;
     //               State = StateFlags.Unsecure;
     //               SendState(GetPlayer(), announce, true);
     //           }
     //           else
     //           {
     //               if (Ruin)
     //                   foreach (var keep in Region.Campaign.Keeps)
     //                       if (Id == keep.Info.KeepId)
     //                       {
     //                           keep.Realm = OwningRealm;
     //                           keep.SendKeepStatus(null);
     //                       }

					//SpawnAllGuards(OwningRealm);
     //           }

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

		public override void SendInteract(Player player, InteractMenu menu)
		{
			if (OwningRealm == player.Realm && AssaultingRealm == Realms.REALMS_REALM_NEUTRAL)
			{
				player.SendClientMessage("Your realm already owns this flag.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
				return;
			}

			if (OwningRealm != player.Realm && OwningRealm == player.Realm)
			{
				player.SendClientMessage("Your realm is already assaulting this flag.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
				return;
			}

			if (!player.CbtInterface.IsPvp)
			{
				player.SendClientMessage("You must be flagged to cap.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
				return;
			}

			if (player.StealthLevel > 0)
			{
				player.SendClientMessage("You can't interact with objects while in stealth.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
				return;
			}

			foreach (var guard in Guards)
			{
				if (guard.Creature != null && !guard.Creature.IsDead && guard.Creature.Realm == OwningRealm && GetDistanceTo(guard.Creature) < 100)
				{
					player.SendClientMessage("Can't capture while a guard (" + guard.Creature.Name + ") is still alive.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
					return;
				}
			}

			if (AllowInteract(player) && InteractableFor(player) && !_captureInProgress)
			{
				CapturingPlayer = null;
				_captureInProgress = true;
				BeginInteraction(player);
			}

			_secureProgress = Math.Abs(_controlGauge) * MAX_SECURE_PROGRESS / MAX_CONTROL_GAUGE;
		}

		public override void NotifyInteractionBroken(NewBuff b)
		{
			_captureInProgress = false;
		}

		public override void NotifyInteractionComplete(NewBuff b)
		{
			// Updates owning and assault teams
			AssaultingRealm = Realms.REALMS_REALM_NEUTRAL;
			
			if (CapturingPlayer != null)
			{
				OwningRealm = CapturingPlayer.Realm;
			}
			State = StateFlags.Contested;
			_captureInProgress = false;
			SendState(CapturingPlayer, true, true);
			
			_displayedTimer = Convert.ToInt16(CONTESTED_TIMESPAN);
			SpawnAllGuards(OwningRealm);
			BroadcastFlagInfo(true);
			GrantCaptureRewards(OwningRealm);
			_stopWatch_Mode = 0;
			_stopWatch = new Stopwatch();
			_stopWatch.Reset();
			_stopWatch.Start();
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
            if (plr != null && plr.ValidInTier(Tier, true))
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

			short orderCount_200 = 0;
			short destroCount_200 = 0;

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
					SendMeTo(player);
                }

				// near count is used for calculating objectiveRewardScaler
				if (distance < 200 && heightDiff < closeHeight)
				{
					if (player.Realm == Realms.REALMS_REALM_ORDER)
						orderCount_200++;
					else
						destroCount_200++;
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
			
            _closeOrderCount = orderCount;
            _closeDestroCount = destroCount;

			_nearOrderCount = orderCount_200;
			_nearDestroCount = destroCount_200;
		}

        /// <summary>
        ///     Update thread.
        /// </summary>
        /// <param name="tick"></param>
        public override void Update(long tick)
        {
            EvtInterface.Update(tick);
			//if (tick < _nextUpdateTick)
			//    return;

			//UpdateGauge(tick);

			if (State == StateFlags.ZoneLocked)
				return;

			if (_stopWatch != null)
			{
				if (_stopWatch_Mode == 0)
				{
					_displayedTimer = Convert.ToInt16(CONTESTED_TIMESPAN - (int)_stopWatch.Elapsed.TotalSeconds);

					if (_stopWatch?.Elapsed.TotalSeconds >= CONTESTED_TIMESPAN)
					{
						_stopWatch.Stop();
						State = StateFlags.Secure;
						GrantCaptureRewards(OwningRealm);
						DespawnAllGuards();
						BroadcastFlagInfo(true);

						_displayedTimer = Convert.ToInt16(SECURED_TIMESPAN);
						_stopWatch.Reset();
						_stopWatch_Mode = 1;
						_stopWatch.Start();
					}

					SendState(GetPlayer(), true, true);
				}
				else if (_stopWatch_Mode == 1)
				{
					_displayedTimer = Convert.ToInt16(SECURED_TIMESPAN - (int)_stopWatch.Elapsed.TotalSeconds);

					if (_stopWatch?.Elapsed.TotalSeconds >= CONTESTED_TIMESPAN)
					{
						_stopWatch.Stop();
						State = StateFlags.Locked;
						GrantCaptureRewards(OwningRealm);
						State = StateFlags.Unsecure;
						OwningRealm = Realms.REALMS_REALM_NEUTRAL;
						BroadcastFlagInfo(true);

						_displayedTimer = 0;
						_stopWatch.Reset();
						_stopWatch_Mode = -1;
					}

					SendState(GetPlayer(), true, true);
				}
			}
		}

        /// <summary>
        ///     Allows this objective to be captured if it was previously locked.
        /// </summary>
        public void UnlockObjective()
        {
            BattlefrontLogger.Debug($"Unlocking objective {this.Name}");
            State = StateFlags.Unsecure;
            OwningRealm = Realms.REALMS_REALM_NEUTRAL;
            BroadcastFlagInfo(true);
            SendState(GetPlayer(), false, true);
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

            //if (Tier == 4)
            //{
            //    var prFrnt = (ProximityProgressingBattleFront)Region.Bttlfront;
            //    if (prFrnt != null && ZoneId != prFrnt.Zones[prFrnt._BattleFrontStatus.OpenZoneIndex].ZoneId)
            //    {
            //        owningRealm = OwningRealm;
            //        assaultingRealm = OwningRealm;
            //    }
            //}

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
			//var transitionTimer = _nextTransitionTimestamp == 0
   //             ? (ushort)0
   //             : (ushort)((_nextTransitionTimestamp - TCPManager.GetTimeStampMS()) / 1000);
   //         transitionTimer = (ushort)_displayedTimer;
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

			if (State != StateFlags.Locked && State != StateFlags.ZoneLocked && InteractableFor(plr))
				Out.WriteUInt16(4);
			else
				Out.WriteUInt16(0);

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
        private StateFlags GetStateFlags()
        {
            if (State == StateFlags.ZoneLocked)
                return StateFlags.Locked;
			
            //if (State == StateFlags.Securing)
            //    State += (byte)StateFlags.ResourceInteraction;

            return State;
        }

        /// <summary>
        ///     Sends the state of the objective in user's map, can announce state changing.
        /// </summary>
        /// <param name="plr"></param>
        /// <param name="announce"></param>
        /// <param name="update"></param>
        public void SendState(Player plr, bool announce, bool update = true)
        {
            if (!Loaded)
                return;

            var State = GetStateFlags();

            var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
            Out.WriteUInt32((uint)Id);

            if (
                State == StateFlags.Contested /*|| (State == StateFlags.Secure && MAX_SECURE_PROGRESS == _secureProgress)*/)
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
            Out.WriteByte((byte)State);
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
            foreach (var player in Region.Players)
                player.SendPacket(Out);
            PacketOut snd = null;

            switch (State)
            {
                case StateFlags.Unsecure:
                    message = string.Concat(Name, " is now open for capture !");
                    break;

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
                            break;
                        }
                        else
                        {
                            message = string.Concat($"The forces of ", GetRealmString(OwningRealm), " are securing ",
                                Name, "!");
                            break;
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
                    player.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                    player.SendLocalizeString(message, largeFilter, Localized_text.CHAT_TAG_DEFAULT);
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
            BattlefrontLogger.Debug($"Locking Objective {Name} for {lockingRealm.ToString()}");

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

            SendState(GetPlayer(), true, true);

            if (!announce)
                return;

            BroadcastFlagInfo(false);
        }

		/// <summary>
		/// Grants rewards for taking this battlefield objective from the enemy.
		/// </summary>
		/// <param name="capturingRealm"></param>
		public void GrantCaptureRewards(Realms capturingRealm)
		{
			if (State == StateFlags.ZoneLocked)
				return;

			VictoryPoint VP = RewardManager.RewardCaptureTick(_closePlayers,
				OwningRealm,
				Tier,
				Name,
				1f); // AAO
			
			switch (State)
			{
				case StateFlags.Contested: // small tick
					if (capturingRealm == Realms.REALMS_REALM_ORDER)
					{

						BattleFront.VictoryPointProgress.OrderVictoryPoints += 5;
					}
					else if (capturingRealm == Realms.REALMS_REALM_DESTRUCTION)
					{
						BattleFront.VictoryPointProgress.DestructionVictoryPoints += 5;
					}
					break;

				case StateFlags.Secure: // big tick
					if (capturingRealm == Realms.REALMS_REALM_ORDER)
					{

						BattleFront.VictoryPointProgress.OrderVictoryPoints += 50;
					}
					else if (capturingRealm == Realms.REALMS_REALM_DESTRUCTION)
					{
						BattleFront.VictoryPointProgress.DestructionVictoryPoints += 50;
					}
					break;

				case StateFlags.Locked: // small tick
					if (capturingRealm == Realms.REALMS_REALM_ORDER)
					{

						BattleFront.VictoryPointProgress.OrderVictoryPoints += 10;
					}
					else if (capturingRealm == Realms.REALMS_REALM_DESTRUCTION)
					{
						BattleFront.VictoryPointProgress.DestructionVictoryPoints += 10;
					}
					break;

				default:
					break;
			}
			
			// Make sure VP dont go less than 0
			if (BattleFront.VictoryPointProgress.OrderVictoryPoints <= 0)
				BattleFront.VictoryPointProgress.OrderVictoryPoints = 0;

			if (BattleFront.VictoryPointProgress.DestructionVictoryPoints <= 0)
				BattleFront.VictoryPointProgress.DestructionVictoryPoints = 0;
			
			BattlefrontLogger.Trace($"{Name} Order VP:{BattleFront.VictoryPointProgress.OrderVictoryPoints} Dest VP:{BattleFront.VictoryPointProgress.DestructionVictoryPoints}");
		}

		/// <summary>
		/// Get rewards for holding a flag when keep falls.
		/// </summary>
		public void GrantKeepCaptureRewards()
        {
            foreach (Player plr in PlayersInRange)
            {
                if (plr.Realm == this.OwningRealm && plr.ValidInTier(Tier, true))
                {
                    if (AccumulatedKills < 3)
                    {
                        plr.SendLocalizeString("Defending this objective was a noble duty. You have received a small reward for your service.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + Tier, 1);
                    }
                    else if (AccumulatedKills <= 6)
                    {
                        plr.SendLocalizeString("Your defense of this objective has been noteworthy! You have received a moderate reward.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + Tier, 2);
                    }
                    else if (AccumulatedKills > 6)
                    {
                        plr.SendLocalizeString("Your defense of this objective has been heroic! You have received a respectable reward.", ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
                        plr.ItmInterface.CreateItem((uint)208399 + Tier, 3);
                    }
                }
            }

            AccumulatedKills = 0;
        }

		public float CalculateObjectiveRewardScale(Player player)
		{
			return RewardManager.CalculateObjectiveRewardScale(player.Realm, _nearOrderCount, _nearDestroCount);
		}

		#region Interaction

		private bool InteractableFor(Player plr)
		{
			Log.Debug($"CampaignObjective", $"State={State}");
			
			switch (State)
			{
				case StateFlags.ZoneLocked:
				case StateFlags.Locked:
				case StateFlags.Secure:
					return false;
				case StateFlags.Unsecure:
				case StateFlags.Contested:
					return plr.Realm != OwningRealm;
				default:
					return false;
			}
		}
		
		private void ChangeOwnership(Realms newRealm)
		{
			if (OwningRealm == newRealm)
				return;

			if (newRealm == Realms.REALMS_REALM_NEUTRAL)
			{
				OwningRealm = newRealm;
				AssaultingRealm = Realms.REALMS_REALM_NEUTRAL;
			}
			else
			{
				Realms oldRealm = OwningRealm;
				OwningRealm = newRealm;

				//if (!Region.Bttlfront.NoSupplies)
				//{
				//	if (_supplies.HeldState == EHeldState.Inactive)
				//		StartSupplyRespawnTimer(SupplyEvent.OwnershipChanged);

				//	else _supplies.SetRealmAssociation(newRealm);
				//}

				//if (Constants.DoomsdaySwitch == 2)
				//	((ProximityBattlefront)Region.Bttlfront).ObjectiveCaptured(oldRealm, newRealm, ZoneId);
				//else
				//	((Battlefront)Region.Bttlfront).ObjectiveCaptured(oldRealm, newRealm, ZoneId);
			}
		}

		#endregion
	}
}
using Appccelerate.StateMachine;
using Common;
using Common.Database.World.Battlefront;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class BattlefieldObjective : Object
    {
        public const int TRANSITION_SPEED = 9;
        private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");
        private List<FlagGuard> Guards = new List<FlagGuard>();

        /// <summary>
        /// Absolute maximum of the control gauges of the flags
        /// <summary>
        public static int MAX_SECURE_PROGRESS = 100;

        /// <summary>
        /// Absolute maximum of the control gauges of the flags.
        /// Is used as a base timer in milliseconds when securing objectives.
        /// <summary>
        public static int MAX_CONTROL_GAUGE = MAX_SECURE_PROGRESS * 200;


        /// <summary>The tier within which the Campaign exists.</summary>
        public readonly byte Tier;

        /// <summary>Campaign objective id</summary>
        public readonly int Id;

        /// <summary>The zone info id within which the Campaign exists.</summary>
        public new readonly ushort ZoneId;

        /// <summary> The region id within which the Campaign exists.</summary>
        public readonly ushort RegionId;

        /// <summary>Influence area containing the objective</summary>
        

        /// <summary>Set of all players in close range, not limited</summary>
        // private ISet<Player> _closePlayers = new HashSet<Player>();

        /// <summary>Displayed timer in seconds</summary>
        public int DisplayedTimer;

        private readonly uint _tokdiscovery; // This is for ToK unlocks
        private readonly uint _tokunlocked; // This is for ToK unlocks

        private readonly uint _x; // why other attributes ?
        private readonly uint _y; // why other attributes ?
        private readonly ushort _z; // why other attributes ?
        private readonly ushort _o; // why other attributes ?

        public uint AccumulatedKills;

        public ApocCommunications CommsEngine { get; set; }

        public Campaign BattleFront { get; set; }
        public BattleFrontStatus battleFrontStatus { get; set; }

        public string Name { get; set; }

        public StateFlags State { get; set; }

        /// <summary>Gets the currently owning realm, may be neutral.</summary>
        public Realms OwningRealm { get; set; }

        private Realms AssaultingRealm { get; set; }

        public int BuffId { get; set; }

        public RVRRewardManager RewardManager { get; set; }

        public PassiveStateMachine<CampaignObjectiveStateMachine.ProcessState, CampaignObjectiveStateMachine.Command> fsm { get; set; }

        // Position of the capture
        private int _captureProgress;
        // Whether a capture is in progress
        private bool _captureInProgress;
        // Positive between 0 and SECURE_PROGRESS_MAX indicating the objective securisation indicator, in seconds
        private int _secureProgress;



        #region timers
        public int CaptureTimer;
        public int GuardedTimer;

        public const int CaptureTimerLength = 2 * 60;
        public const int GuardedTimerLength = 1 * 60;
        #endregion

        /// <summary>
        /// Constructor to assist in isolation testing.
        /// </summary>
        public BattlefieldObjective()
        {
            CommsEngine = new ApocCommunications();
            RewardManager = new RVRRewardManager();
        }

        /// <summary>
        /// **** TEST : : Constructor to assist in testing - dont use in production. ***
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="zoneId"></param>
        /// <param name="regionId"></param>
        /// <param name="tier"></param>
        public BattlefieldObjective(int id, string name, int zoneId, int regionId, int tier)
        {
            Id = id;
            Name = name;
            ZoneId = (ushort)zoneId;
            RegionId = (ushort)regionId;
            Tier = (byte)tier;
            State = StateFlags.Unsecure;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="tier"></param>
        public BattlefieldObjective(RegionMgr region, BattleFront_Objective objective)
        {
            Id = objective.Entry;
            Name = objective.Name;
            ZoneId = objective.ZoneId;
            RegionId = objective.RegionId;
            Tier = (byte)region.GetTier();
            State = StateFlags.Unsecure;

            _x = (uint)objective.X;
            _y = (uint)objective.Y;
            _z = (ushort)objective.Z;
            _o = (ushort)objective.O;
            _tokdiscovery = objective.TokDiscovered;
            _tokunlocked = objective.TokUnlocked;

            Heading = _o;
            WorldPosition.X = (int)_x;
            WorldPosition.Y = (int)_y;
            WorldPosition.Z = _z;

            CommsEngine = new ApocCommunications();
            RewardManager = new RVRRewardManager();
            fsm = new CampaignObjectiveStateMachine(this).fsm;
            fsm.Initialize(CampaignObjectiveStateMachine.ProcessState.Neutral);
            if (objective.Guards != null)
            {
                foreach (BattleFront_Guard Guard in objective.Guards)
                {
                    Guards.Add(new FlagGuard(this, region, objective.ZoneId, Guard.OrderId, Guard.DestroId, Guard.X, Guard.Y, Guard.Z, Guard.O));
                }
            }
            _captureProgress = 20000;
            CaptureDuration = 10;
            EvtInterface.AddEvent(CheckTimers, 1000, 0);
            BuffId = 0;
        }

        public override void OnLoad()
        {
            // Objective position
            Z = _z;
            X = Zone.CalculPin(_x, true);
            Y = Zone.CalculPin(_y, false);
            base.OnLoad();

            SetOffset((ushort)(_x >> 12), (ushort)(_y >> 12));

            // Initial state
            IsActive = true;

        }

        public override string ToString()
        {
            return $"Objective : {Name} Status : {State} Owner : {OwningRealm}";
        }

        /// <summary>
        /// Check the various timers and determine whether to fire any events
        /// </summary>
        private void CheckTimers()
        {
            var currentTime = TCPManager.GetTimeStamp();

            if (CaptureTimer > 0 && CaptureTimer <= currentTime)
                OnCapturedTimerEnd();
            if (GuardedTimer > 0 && GuardedTimer <= currentTime)
                OnGuardedTimerEnd();

            DisplayedTimer--;
            if (DisplayedTimer <= 0)
                DisplayedTimer = 0;

        }

        private void OnGuardedTimerEnd()
        {
            fsm.Fire(CampaignObjectiveStateMachine.Command.OnGuardedTimerEnd);
        }

        private void OnCapturedTimerEnd()
        {
            fsm.Fire(CampaignObjectiveStateMachine.Command.OnCaptureTimerEnd);
        }

        private bool SpawnAllGuards(Realms owner)
        {
            if (owner == Realms.REALMS_REALM_DESTRUCTION || owner == Realms.REALMS_REALM_ORDER)
            {
                if (Guards != null)
                    foreach (FlagGuard guard in Guards)
                        guard.SpawnGuard((int)owner);
                return true;
            }
            return false;
        }

        private bool DespawnAllGuards()
        {
            if (Guards != null)
            {
                foreach (FlagGuard guard in Guards)
                {
                    guard.DespawnGuard();
                    if (guard.Creature != null)
                    {
                        guard.Creature = null;
                    }
                }
                return true;
            }
            return false;
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
                if (guard.Creature != null && !guard.Creature.IsDead && GetDistanceTo(guard.Creature) < 100)
                {
                    player.SendClientMessage("Can't capture while a guard (" + guard.Creature.Name + ") is still alive.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }
            }

            if (AllowInteract(player) && InteractableFor(player))
            {
                if (_captureInProgress)  // cross realm fires here
                {
                    player.SendClientMessage(CapturingPlayer?.Name + " is already interacting with this object.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                }
                else
                {
                    CapturingPlayer = null;
                    _captureInProgress = true;
                    BeginInteraction(player);

                    _secureProgress = Math.Abs(_captureProgress) * MAX_SECURE_PROGRESS / MAX_CONTROL_GAUGE;
                }
            }
        }

        public override void NotifyInteractionBroken(NewBuff b)
        {
            fsm.Fire(CampaignObjectiveStateMachine.Command.OnPlayerInteractionBroken);
            _captureInProgress = false;
            CapturingPlayer = null;
        }

        public override void NotifyInteractionComplete(NewBuff b)
        {
            fsm.Fire(CampaignObjectiveStateMachine.Command.OnPlayerInteractionComplete, CapturingPlayer);
            _captureInProgress = false;
            CapturingPlayer = null;
        }

        public void OpenBattleFront()
        {
            if (!fsm.IsRunning)
            {
                BattlefrontLogger.Debug($"Starting BattlefieldObjective {Name} FSM...");
                fsm.Fire(CampaignObjectiveStateMachine.Command.OnOpenBattleFront);
                fsm.Start();
            }
            else
            {
                BattlefrontLogger.Debug($"Stopping BattlefieldObjective {Name} FSM...");
                fsm.Stop();
                
                BattlefrontLogger.Debug($"Starting BattlefieldObjective {Name} FSM...");
                fsm.Fire(CampaignObjectiveStateMachine.Command.OnOpenBattleFront);
                fsm.Start();
            }

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
            return TRANSITION_SPEED;
        }

        public override void AddInRange(Object obj)
        {
            var plr = obj as Player;
            if (plr != null && plr.ValidInTier(Tier, true))
            {
                SendFlagInfo(plr);
                plr.CurrentObjectiveFlag = this;

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
            }

            base.RemoveInRange(obj);
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


            Out.WriteUInt16(0); // _displayedTimer
            Out.WriteUInt16((ushort)DisplayedTimer); // Timer on map UI
            Out.WriteUInt16(0); // _displayedTimer
            Out.WriteUInt16((ushort)(DisplayedTimer)); // Timer in top right of UI
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
                    //return _secureProgress == MAX_SECURE_PROGRESS ? "GENERATING" : "SECURING";
                    return "GUARDED";

                case StateFlags.Abandoned:
                    return "SECURED";

                case StateFlags.Contested:
                    return realm == OwningRealm ? "RECLAIM" : "HOLD";

                case StateFlags.Unsecure:
                    return "AVAILABLE";

                case StateFlags.Hidden:
                    return "";

                case StateFlags.ZoneLocked:
                    return "ZONE-LOCKED";

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

            BattlefrontLogger.Trace($"{State} / {InteractableFor(plr)}");
            if (InteractableFor(plr))
            {
                BattlefrontLogger.Trace($"4");
                Out.WriteUInt16(4);
            }
            else
            {
                BattlefrontLogger.Trace($"0");
                Out.WriteUInt16(0);
            }

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

            return State;
        }

        /// <summary>
        ///     Update thread.
        /// </summary>
        /// <param name="msTick"></param>
        public override void Update(long msTick)
        {


            EvtInterface.Update(msTick);

            if (State == StateFlags.ZoneLocked)
                return;

            var frnt = BattleFront;
            if (frnt != null && frnt.IsBattleFrontLocked())
                return;


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

            if (State == StateFlags.Contested)
            {
                Out.Fill(0, 2);
                Out.WriteUInt16((ushort)DisplayedTimer);
                Out.Fill(0xFF, 2);
                Out.WriteUInt16(0);
            }
            else if (State == StateFlags.Secure)
            {
                //Out.Fill(0xFF, 4);
                Out.Fill(0, 2);
                Out.WriteUInt16((ushort)DisplayedTimer);

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
                    message = string.Concat(Name, " is under assault by the forces of ", GetRealmString(AssaultingRealm), "!");
                    break;

                case StateFlags.Secure:
                    var securedState = _secureProgress == MAX_SECURE_PROGRESS;

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
                        message = string.Concat($"The forces of ", GetRealmString(OwningRealm), " are securing ", Name, "!");
                        break;
                    }

                default:
                    message = string.Empty;
                    break;
            }
            try
            {
                if (plr != null)
                {
                    plr.SendPacket(Out);
                    
                }
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
            catch (Exception e)
            {
                Log.Error("Exception", e.Message + "\r\b" + e.StackTrace);
                return;
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

            player.SendClientMessage($"Control progress: {_captureProgress}");
            player.SendClientMessage($"Secure progress: {_secureProgress}");
            player.SendClientMessage($"Flag status: {State}");

        }


        /// <summary>
        /// Grants rewards for taking this battlefield objective from the enemy.
        /// </summary>
        /// <param name="capturingRealm"></param>
        public void GrantCaptureRewards(Realms capturingRealm)
        {
            if (State == StateFlags.ZoneLocked)
                return;

            var closePlayers = GetClosePlayers(capturingRealm);

            var contributionDefinition = new ContributionDefinition();
            var activeBattleFrontStatus = BattleFront.GetActiveBattleFrontStatus();

            VictoryPoint VP = new VictoryPoint(0, 0);

            // Check RC is within range of this BO to award RP for.
            var destructionRealmCaptain = activeBattleFrontStatus.DestructionRealmCaptain;
            if (destructionRealmCaptain?.GetDistanceToObject(this) < 200)
            {
                destructionRealmCaptain?.AddRenown(150, false, RewardType.ObjectiveCapture,
                    "for being Realm Captain");
            }
            var orderRealmCaptain = activeBattleFrontStatus.OrderRealmCaptain;
            if (orderRealmCaptain?.GetDistanceToObject(this) < 200)
            {
                orderRealmCaptain?.AddRenown(150, false, RewardType.ObjectiveCapture,
                    "for being Realm Captain");
            }

            switch (State)
            {
                case StateFlags.Contested: // small tick
                    VP = RewardManager.RewardCaptureTick(closePlayers, capturingRealm, Tier, Name, 1f, BORewardType.CAPTURING);
                    lock (closePlayers)
                    {
                        foreach (var closePlayer in closePlayers)
                        {
                            closePlayer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.BO_TAKE_SMALL_TICK);
                        }
                    }
                    break;

                case StateFlags.Secure: // big tick
                    VP = RewardManager.RewardCaptureTick(closePlayers, capturingRealm, Tier, Name, 1f, BORewardType.CAPTURED);

                    WorldMgr.UpperTierCampaignManager.GetActiveCampaign().VictoryPointProgress.UpdateStatus(WorldMgr.UpperTierCampaignManager.GetActiveCampaign());

                    lock (closePlayers)
                    {
                        foreach (var closePlayer in closePlayers)
                        {
                            closePlayer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.BO_TAKE_BIG_TICK);

                            // is this player the group leader?
                            if (closePlayer.PriorityGroup?.GetLeader() == closePlayer)
                            {
                                closePlayer.UpdatePlayerBountyEvent((byte)ContributionDefinitions.GROUP_LEADER_BO_BIG_TICK);
                            }
                        }
                    }
                    break;
                case StateFlags.Locked: // unlock tick
                    VP = RewardManager.RewardCaptureTick(closePlayers, capturingRealm, Tier, Name, 1f, BORewardType.GUARDED);
                    lock (closePlayers)
                    {
                        foreach (var closePlayer in closePlayers)
                        {
                            // ContributionManagerInstance holds the long term values of contribution for a player.
                            closePlayer.UpdatePlayerBountyEvent((byte) ContributionDefinitions.BO_TAKE_UNLOCK_TICK);
                        }
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
        /// Get players that are close and members of a given realm.
        /// </summary>
        /// <param name="capturingRealm"></param>
        /// <returns></returns>
        private ISet<Player> GetClosePlayers(Realms capturingRealm)
        {
            var applicablePlayerList = PlayersInRange.Where(x => x.Realm == capturingRealm).ToList();

            return GetClosePlayers(applicablePlayerList);
        }

        private HashSet<Player> GetClosePlayers(List<Player> playerList)
        {
            var closeHeight = 70 / 2 * UNITS_TO_FEET;
            var closePlayers = new HashSet<Player>();

            foreach (var player in playerList)
            {
                if (player.IsDead || player.StealthLevel != 0 || !player.CbtInterface.IsPvp || player.IsInvulnerable)
                    continue;

                var distance = GetDistanceToObject(player);
                var heightDiff = Math.Abs(Z - player.Z);
                if (distance < 200 && heightDiff < closeHeight)
                {
                    closePlayers.Add(player);
                    SendMeTo(player);
                }
            }
            return closePlayers;
        }

        #region Interaction

        private bool InteractableFor(Player plr)
        {
            BattlefrontLogger.Trace($"CampaignObjective..State={State}");
            var result = false;
            switch (State)
            {
                case StateFlags.Unsecure:
                    result = true;
                    break;
                case StateFlags.ZoneLocked:
                    result = false;
                    break;
                case StateFlags.Secure:
                    result = false;
                    break;
                case StateFlags.Contested:
                    result = plr.Realm != AssaultingRealm;  // interact if not the assaulting realm
                    break;
                case StateFlags.Locked:
                    result = plr.Realm != OwningRealm;      // interact if not the owning realm
                    break;
                default:
                    result = false;
                    break;
            }
            BattlefrontLogger.Trace($"result={result}");

            return result;
        }

        #endregion

        public void SetObjectiveSafe()
        {
            BattlefrontLogger.Trace($"{Name} : Safe : (NEUTRAL)");
            DisplayedTimer = 0;
            DespawnAllGuards();

            // state change and send state
            State = StateFlags.Unsecure;
            OwningRealm = Realms.REALMS_REALM_NEUTRAL;
            // Make sure we remove the Buff Id.
            BuffId = 0;
            BroadcastFlagInfo(true);
            SendState(GetPlayer(), false, true);

            GuardedTimer = 0;
            CaptureTimer = 0;

            RemoveGlow();

        }

        public void SetObjectiveLocked()
        {
            BattlefrontLogger.Debug($"{Name} : Locking : {OwningRealm}");
            DisplayedTimer = 0;
            DespawnAllGuards();

            // state change and send state
            State = StateFlags.ZoneLocked;

            BroadcastFlagInfo(true);
            SendState(GetPlayer(), false, true);

            GuardedTimer = 0;
            CaptureTimer = 0;

            RemoveGlow();
        }

        public void SetObjectiveCapturing()
        {
            BattlefrontLogger.Debug($"{Name} : Capturing : {OwningRealm} => {AssaultingRealm}");
            if (CapturingPlayer != null)
            {
                AssaultingRealm = CapturingPlayer.Realm;
            }

            State = StateFlags.Contested;

            var timerLength = CaptureTimerLength + StaticRandom.Instance.Next(0, 60);

            CaptureTimer = TCPManager.GetTimeStamp() + timerLength;
            DisplayedTimer = timerLength;
            SendState(CapturingPlayer, true, true);
            DespawnAllGuards();
            BroadcastFlagInfo(true);
            GrantCaptureRewards(AssaultingRealm);

            AddGlow(AssaultingRealm);

        }

        private void AddGlow(Realms assaultingRealm)
        {
            GameObject_proto glowProto = GameObjectService.GetGameObjectProto(99858); //99858

            if (glowProto != null)
            {
                GameObject_spawn spawn = new GameObject_spawn
                {
                    Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                    WorldO = Heading,
                    WorldX = WorldPosition.X,
                    WorldY = WorldPosition.Y,
                    WorldZ = WorldPosition.Z,
                    ZoneId = ZoneId,
                };
                spawn.BuildFromProto(glowProto);

                var siegeRangeFlag = new GameObject(spawn);
                if (assaultingRealm == Realms.REALMS_REALM_DESTRUCTION)
                    siegeRangeFlag.VfxState = 2; //1 blue, 2 red, 3 white, 4 - white;
                else
                {
                    siegeRangeFlag.VfxState = 1; //1 blue, 2 red, 3 white, 4 - white;
                }
                Region.AddObject(siegeRangeFlag, ZoneId);
            }
        }

        public void SetObjectiveCaptured()
        {
            BattlefrontLogger.Debug($"{Name} : Captured : {OwningRealm} => {AssaultingRealm} ");

            OwningRealm = AssaultingRealm;

            // Add buffs to Assaulting Realm
            var campaignObjectiveBuff = RVRProgressionService._CampaignObjectiveBuffs.SingleOrDefault(x => x.ObjectiveId == Id);
            if (campaignObjectiveBuff != null)
            {
                BuffId = campaignObjectiveBuff.BuffId;
                BattlefrontLogger.Info($"Setting Campaign Objective Buff {campaignObjectiveBuff.BuffId} for Objective {Id}");
            }

            var timerLength = GuardedTimerLength + StaticRandom.Instance.Next(30, 60);

            GuardedTimer = TCPManager.GetTimeStamp() + timerLength;
            DisplayedTimer = timerLength;

            State = StateFlags.Secure;
            SendState(CapturingPlayer, true, true);
            SpawnAllGuards(OwningRealm);
            BroadcastFlagInfo(true);
            GrantCaptureRewards(OwningRealm);

            RemoveGlow();
        }

        public void SetObjectiveGuarded()
        {
            BattlefrontLogger.Debug($"{Name} : Guarded : {OwningRealm}");
            DisplayedTimer = 0;
            State = StateFlags.Locked;
            BroadcastFlagInfo(true);
            SendState(GetPlayer(), true, true);
            GrantCaptureRewards(OwningRealm);

            RemoveGlow();
        }

        private void RemoveGlow()
        {
            var goList = Region.GetObjects<GameObject>().Where(x => x.Entry == 99858);
            foreach (var gameObject in goList)
            {
                gameObject.Destroy();
            }
        }
    }
}
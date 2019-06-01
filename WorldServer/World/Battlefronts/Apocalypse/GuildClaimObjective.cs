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
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.World.Map;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class GuildClaimObjective : Object
    {
        public Guild.Guild ClaimingGuild { get; set; }
        public BattleFrontKeep Keep { get; set; }

        public const int TRANSITION_SPEED = 9;
        private static readonly Logger BattlefrontLogger = LogManager.GetLogger("BattlefrontLogger");

        /// <summary>
        /// Absolute maximum of the control gauges of the flags
        /// <summary>
        public static int MAX_SECURE_PROGRESS = 100;

        /// <summary>
        /// Absolute maximum of the control gauges of the flags.
        /// Is used as a base timer in milliseconds when securing objectives.
        /// <summary>
        public static int MAX_CONTROL_GAUGE = MAX_SECURE_PROGRESS * 200;

        public int Id { get; set; }
        public byte Tier { get; set; }

        /// <summary>The zone info id within which the Campaign exists.</summary>
        public new readonly ushort ZoneId;

        /// <summary> The region id within which the Campaign exists.</summary>
        public readonly ushort RegionId;

        /// <summary>Influence area containing the objective</summary>
        private Zone_Area _area;


        /// <summary>Set of all players in close range, not limited</summary>
        // private ISet<Player> _closePlayers = new HashSet<Player>();

        /// <summary>Displayed timer in seconds</summary>
        private int _displayedTimer;

        private readonly uint _tokdiscovery; // This is for ToK unlocks
        private readonly uint _tokunlocked; // This is for ToK unlocks

        private readonly uint _x; // why other attributes ?
        private readonly uint _y; // why other attributes ?
        private readonly ushort _z; // why other attributes ?
        private readonly ushort _o; // why other attributes ?

        public ApocCommunications CommsEngine { get; set; }

        public Campaign BattleFront { get; set; }

        public string Name { get; set; }

        public StateFlags State { get; set; }

        /// <summary>Gets the currently owning realm, may be neutral.</summary>
        public Realms OwningRealm { get; set; }

        public int BuffId { get; set; }

        // Position of the capture
        private int _captureProgress;
        // Whether a capture is in progress
        private bool _captureInProgress;
        // Positive between 0 and SECURE_PROGRESS_MAX indicating the objective securisation indicator, in seconds
        private int _secureProgress;


        /// <summary>
        /// Constructor to assist in isolation testing.
        /// </summary>
        public GuildClaimObjective()
        {
            CommsEngine = new ApocCommunications();
        }

        /// <summary>
        /// **** TEST : : Constructor to assist in testing - dont use in production. ***
        /// </summary>
        public GuildClaimObjective(string name, int zoneId, int regionId)
        {
            Name = name;
            ZoneId = (ushort)zoneId;
            RegionId = (ushort)regionId;
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="tier"></param>
        public GuildClaimObjective(RegionMgr region, BattleFront_Objective objective)
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
            _captureProgress = 20000;
            CaptureDuration = 10;
            // TODO : Can add a default buff here.
            BuffId = 14121;  // King of the hill
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
            return $"Objective : {Name} Status : {State} Owner : {OwningRealm} Guild : {ClaimingGuild?.Info.Name}";
        }

        public bool CanClaim(Player player)
        {
            //foreach (var vaultUser in player.GldInterface.Guild.GuildVaultUser)
            //{
            //    if (player.CharacterId == vaultUser.CharacterId)
            //        return true;
            //}

            return true;
        }

        
        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (player.GldInterface == null)
            {
                player.SendClientMessage("Only guild members can claim a keep", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (!player.GldInterface.IsInGuild())
            {
                player.SendClientMessage("Only guild members can claim a keep", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (player.GldInterface.Guild == null)
            {
                player.SendClientMessage("Only guild members can claim a keep", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (this.Keep.KeepStatus != KeepStatus.KEEPSTATUS_SEIZED)
            {
                player.SendClientMessage("You can only claim a keep when it's lord has been killed", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (!CanClaim(player))
            {
                player.SendClientMessage("You are not senior enough in your guild to claim this keep", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (OwningRealm == player.Realm)
            {
                player.SendClientMessage("Your realm already owns this keep.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
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
            
            _captureInProgress = false;
            CapturingPlayer = null;
        }

        public override void NotifyInteractionComplete(NewBuff b)
        {
            this.CapturingPlayer.SendClientMessage(CapturingPlayer?.Name + $" has claimed {this.Keep.Info.Name} for {this.CapturingPlayer.GldInterface.Guild.Info.Name}");
            BattlefrontLogger.Info(CapturingPlayer?.Name + $" has claimed {this.Keep.Info.Name} for {this.CapturingPlayer.GldInterface.Guild.Info.Name}");
            this.ClaimingGuild = this.CapturingPlayer.GldInterface.Guild;
            this.OwningRealm = this.CapturingPlayer.Realm;
            _captureInProgress = false;
            CapturingPlayer = null;

            this.Keep.OnGuildClaimInteracted(this.ClaimingGuild.Info.GuildId);
        }
        //public override void AddInRange(Object obj)
        //{
        //    var plr = obj as Player;
        //    if (plr != null && plr.ValidInTier(Tier, true))
        //    {
        //        SendFlagInfo(plr);
        //        plr.CurrentObjectiveFlag = this;

        //        if (_tokdiscovery > 0)
        //            plr.TokInterface.AddTok((ushort)_tokdiscovery);
        //        if (_tokunlocked > 0)
        //            plr.TokInterface.AddTok((ushort)_tokunlocked);

        //        base.AddInRange(obj);
        //    }
        //}

        //public override void RemoveInRange(Object obj)
        //{
        //    var plr = obj as Player;
        //    if (plr != null)
        //    {
        //        CommsEngine.SendFlagLeft(plr, Id);
        //        if (plr.CurrentObjectiveFlag == this)
        //            plr.CurrentObjectiveFlag = null;
        //    }

        //    base.RemoveInRange(obj);
        //}


        /// <summary>
        ///     Broadcasts flag state to all players withing range.
        /// </summary>
        /// <param name="announce">True </param>
        public void BroadcastFlagInfo(bool announce)
        {
            foreach (var plr in PlayersInRange) // probably synchronization bug
            {
                SendMeTo(plr);

                //SendFlagInfo(plr);
            }
        }

        /// <summary>
        ///     Sends objective's detailed info in upper right corner of the screen.
        /// </summary>
        /// <param name="plr"></param>
        //public void SendFlagInfo(Player plr)
        //{
        //    // return;
        //    var owningRealm = OwningRealm;
        //    var assaultingRealm = AssaultingRealm;
        //    var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO, 32);
        //    Out.WriteUInt32((uint)Id);
        //    Out.WriteByte(0);
        //    Out.WriteByte((byte)owningRealm); //(byte)OwningRealm
        //    Out.WriteByte(1);
        //    Out.WriteUInt16(0);
        //    Out.WritePascalString(Name);
        //    //
        //    //
        //    //
        //    Out.WriteByte(2);
        //    Out.WriteUInt32(0x0000348F);
        //    Out.WriteByte((byte)assaultingRealm); // (byte)AssaultingRealm

        //    // Expansion for objective goal
        //    Out.WriteByte(0);

        //    Out.WriteUInt16(0xFF00);
        //    Out.WritePascalString(GetStateText(plr.Realm));
        //    Out.WriteByte(0);

        //    switch (State)
        //    {
        //        case StateFlags.ZoneLocked:
        //        case StateFlags.RegionLockManager:
        //            Out.WritePascalString("This area has been captured by ", GetRealmString(OwningRealm),
        //                ". The battle wages on elsewhere!");
        //            break;

        //        case StateFlags.Contested:
        //            if (plr.Realm != OwningRealm)
        //                Out.WritePascalString("This Battlefield Objective is being assaulted by ",
        //                    GetRealmString(AssaultingRealm),
        //                    ". Ensure the timer elapses to claim it for your Realm!");
        //            else
        //                Out.WritePascalString("This Battlefield Objective is being assaulted by ",
        //                    GetRealmString(AssaultingRealm),
        //                    ". Reclaim this Battlefield Objective for ", GetRealmString(plr.Realm), "!");
        //            break;

        //        case StateFlags.Secure:
        //            if (plr.Realm == OwningRealm)
        //                Out.WritePascalString("This Battlefield Objective is generating resources for ",
        //                    GetRealmString(OwningRealm),
        //                    ". Defend the flag from enemy assault!");
        //            else
        //                Out.WritePascalString("This Battlefield Objective is generating resources for ",
        //                    GetRealmString(OwningRealm),
        //                    ". Claim this Battlefield Objective for ", GetRealmString(plr.Realm), "!");
        //            break;

        //        case StateFlags.Unsecure:
        //            Out.WritePascalString("This Battlefield Objective is open for capture!");
        //            break;

        //        default:
        //            Out.WritePascalString("");
        //            break;
        //    }


        //    Out.WriteUInt16(0); // _displayedTimer
        //    Out.WriteUInt16((ushort)_displayedTimer); // _displayedTimer
        //    Out.WriteUInt16(0); // _displayedTimer
        //    Out.WriteUInt16((ushort)_displayedTimer); // _displayedTimer
        //    Out.Fill(0, 4);
        //    Out.WriteByte(0x71);
        //    Out.WriteByte(1);
        //    Out.Fill(0, 3);

        //    plr.SendPacket(Out);
        //}

        /// <summary>
        ///     Builds binary flag depending on the objective's current state.
        /// </summary>
        /// <param name="realm">Realm of the player that will get the state</param>
        /// <returns>String constance representation</returns>
        //private string GetStateText(Realms realm)
        //{
        //    switch (State)
        //    {
        //        case StateFlags.Secure:
        //            //return _secureProgress == MAX_SECURE_PROGRESS ? "GENERATING" : "SECURING";
        //            return "GUARDED";

        //        case StateFlags.Abandoned:
        //            return "SECURED";

        //        case StateFlags.Contested:
        //            return realm == OwningRealm ? "RECLAIM" : "HOLD";

        //        case StateFlags.Unsecure:
        //            return "AVAILABLE";

        //        case StateFlags.Hidden:
        //            return "";

        //        case StateFlags.ZoneLocked:
        //            return "ZONE-LOCKED";

        //        case StateFlags.RegionLockManager:
        //            return "SECURED";
        //    }

        //    return "UNKNOWN";
        //}

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
        //public void SendState(Player plr, bool announce, bool update = true)
        //{
        //    if (!Loaded)
        //        return;

        //    var State = GetStateFlags();

        //    var Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_STATE, 16);
        //    Out.WriteUInt32((uint)Id);

        //    if (State == StateFlags.Contested)
        //    {
        //        Out.Fill(0, 2);
        //        Out.WriteUInt16((ushort)_displayedTimer);
        //        Out.Fill(0xFF, 2);
        //        Out.WriteUInt16(0);
        //    }
        //    else if (State == StateFlags.Secure)
        //    {
        //        //Out.Fill(0xFF, 4);
        //        Out.Fill(0, 2);
        //        Out.WriteUInt16((ushort)_displayedTimer);

        //        Out.WriteByte(0);
        //        Out.WriteByte((byte)(MAX_SECURE_PROGRESS - _secureProgress)); // Unk6 - time till next resource release
        //        Out.WriteUInt16(
        //            (ushort)MAX_SECURE_PROGRESS); // Unk7 - vertical offset for drawing overlay - Unk6 may not exceed
        //    }
        //    else
        //    {
        //        Out.Fill(0xFF, 6);
        //        Out.WriteUInt16(0);
        //    }

        //    Out.WriteByte((byte)OwningRealm);
        //    Out.WriteByte(update ? (byte)1 : (byte)0);
        //    Out.WriteByte((byte)State);
        //    Out.WriteByte(0);

        //    if (!announce)
        //    {
        //        if (plr != null)
        //            plr.SendPacket(Out);
        //        else
        //            foreach (var player in Region.Players)
        //                player.SendPacket(Out);
        //        return;
        //    }

        //    string message = null;
        //    var largeFilter = OwningRealm == Realms.REALMS_REALM_ORDER
        //        ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE
        //        : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE;
        //    foreach (var player in Region.Players)
        //        player.SendPacket(Out);
        //    PacketOut snd = null;

        //    switch (State)
        //    {
        //        case StateFlags.Unsecure:
        //            message = string.Concat(Name, " is now open for capture !");
        //            break;

        //        case StateFlags.Contested:
        //            message = string.Concat(Name, " is under assault by the forces of ", GetRealmString(AssaultingRealm), "!");
        //            break;

        //        case StateFlags.Secure:
        //            var securedState = _secureProgress == MAX_SECURE_PROGRESS;

        //            if (_secureProgress == MAX_SECURE_PROGRESS)
        //            {
        //                message = string.Concat($"The forces of ", GetRealmString(OwningRealm), " have taken ",
        //                    Name, "!");
        //                snd = new PacketOut((byte)Opcodes.F_PLAY_SOUND);
        //                snd.WriteByte(0);
        //                snd.WriteUInt16(OwningRealm == Realms.REALMS_REALM_ORDER ? (ushort)0x0C : (ushort)0x332);
        //                snd.Fill(0, 10);
        //                break;
        //            }
        //            else
        //            {
        //                message = string.Concat($"The forces of ", GetRealmString(OwningRealm), " are securing ", Name, "!");
        //                break;
        //            }
        //            break;

        //        default:
        //            message = string.Empty;
        //            break;
        //    }
        //    try
        //    {
        //        if (plr != null)
        //        {
        //            plr.SendPacket(Out);
        //            BattlefrontLogger.Debug("Sending State to Player", "Player: " + plr.Name + ", BattlefieldObjective: " + Name);
        //        }
        //        else
        //            foreach (var player in Region.Players)
        //            {
        //                player.SendPacket(Out); // Objective's state
        //                BattlefrontLogger.Debug("Sending State to Player", "Player: " + player.Name + ", BattlefieldObjective: " + Name);

        //                if (string.IsNullOrEmpty(message) || !player.CbtInterface.IsPvp)
        //                    continue;

        //                // Notify RvR flagged players of activity
        //                player.SendLocalizeString(message, ChatLogFilters.CHATLOGFILTERS_RVR, Localized_text.CHAT_TAG_DEFAULT);
        //                player.SendLocalizeString(message, largeFilter, Localized_text.CHAT_TAG_DEFAULT);
        //                BattlefrontLogger.Debug("Sending Activity to RVR-Player", "Player: " + player.Name + ", BattlefieldObjective: " + Name + "Message: " + message);
        //                if (snd != null)
        //                    player.SendPacket(snd);
        //            }
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Error("Exception", e.Message + "\r\b" + e.StackTrace);
        //        return;
        //    }
        //}

       
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
            BattlefrontLogger.Trace($"CampaignObjective", $"State={State}");
            var result = false;
            switch (State)
            {
                case StateFlags.Unsecure:
                    result= true;
                    break;
                case StateFlags.ZoneLocked:
                    result = false;
                    break;
                case StateFlags.Secure:
                    result = false;
                    break;
                default:
                    result =false;
                    break;
            }
            BattlefrontLogger.Trace($"result={result}");

            return result;
        }

        #endregion

        
    }
}
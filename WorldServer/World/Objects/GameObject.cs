using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects.PublicQuests;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    public class GameObject : Unit
    {
        public GameObject_spawn Spawn;

        private long CreatureSpawnCooldown = 0;
        private long SoundCooldown = 0;

        public byte IsAttackable;
        public byte Respawn = 1;

        private byte _vfxState; // This doesn't just handle doors. It can change the object's physical state in other ways too (scenario glows around domination objectives have 3 states)
        public byte VfxState
        {
            get
            {
                return _vfxState;
            }

            set
            {
                _vfxState = value;
                UpdateVfxState(value);
            }
        }

        public ushort? Flags = null;
        public uint Entry => Spawn?.Entry ?? 0;

        public GameObject()
            : base()
        {

        }

        public GameObject(GameObject_spawn spawn)
            : this()
        {
            Spawn = spawn;
            if (!string.IsNullOrEmpty(Spawn.AlternativeName))
                Name = Spawn.AlternativeName;
            else
                Name = spawn.Proto.Name;
            VfxState = (byte)spawn.VfxState;
        }

        public override void OnLoad()
        {
            Faction = Spawn.Proto.Faction;
            while (Faction >= 8) Faction -= 8;
            if (Faction < 2) Rank = 0;
            else if (Faction < 4) Rank = 1;
            else if (Faction < 6) Rank = 2;
            else if (Faction < 9) Rank = 3;
            Faction = Spawn.Proto.Faction;

            // Setting realm...?
            if (Faction > 63 && Faction < 118)
                Realm = Realms.REALMS_REALM_ORDER;
            else if (Faction > 127 && FactionId < 181)
                Realm = Realms.REALMS_REALM_DESTRUCTION;
            else if (Faction == 1)
                Realm = Realms.REALMS_REALM_HOSTILE;

            Level = Spawn.Proto.Level;
            MaxHealth = Spawn.Proto.HealthPoints;
            Health = TotalHealth;

            X = Zone.CalculPin((uint)(Spawn.WorldX), true);
            Y = Zone.CalculPin((uint)(Spawn.WorldY), false);
            Z = (ushort)(Spawn.WorldZ);

            Heading = (ushort)Spawn.WorldO;

            WorldPosition.X = Spawn.WorldX;
            WorldPosition.Y = Spawn.WorldY;
            WorldPosition.Z = Spawn.WorldZ;

            UpdateOffset();

            ScrInterface.AddScript(Spawn.Proto.ScriptName);
            base.OnLoad();
            IsActive = true;
            Looted = false;
            Interactable = true;

            IsAttackable = Spawn.Proto.IsAttackable;

            if (Entry == 2000589)
                EvtInterface.AddEvent(CheckPlayersInCloseRange, 6000, 0);
        }

        private void CheckPlayersInCloseRange()
        {
            int closeHeight = 100;

            foreach (Player player in PlayersInRange)
            {
                if (player.IsDead)
                    continue;

                int distance = GetDistanceToObject(player);
                int heightDiff = Math.Abs(Z - player.Z);
                if (distance < closeHeight && heightDiff < closeHeight)
                {
                    if (Spawn.TokUnlock != null && Spawn.TokUnlock.Length > 1)
                    {
                        player.TokInterface.AddToks(Spawn.TokUnlock);
                    }
                }
            }
        }

        public override void SendMeTo(Player plr)
        {
            // Log.Info("STATIC", "Creating static oid=" + Oid + " name=" + Name + " x=" + Spawn.WorldX + " y=" + Spawn.WorldY + " z=" + Spawn.WorldZ + " doorID=" + Spawn.DoorId);
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC, 46 + (Name?.Length ?? 0));
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(VfxState); //ie: red glow, open door, lever pushed, etc

            Out.WriteUInt16((ushort)Spawn.WorldO);
            Out.WriteUInt16((ushort)Spawn.WorldZ);
            Out.WriteUInt32((uint)Spawn.WorldX);
            Out.WriteUInt32((uint)Spawn.WorldY);
            Out.WriteUInt16((ushort)Spawn.DisplayID);

            Out.WriteByte((byte)(Spawn.GetUnk(0) >> 8));

            // Get the database if the value hasnt been changed (currently only used for keep doors)
            if (Realm == GameData.Realms.REALMS_REALM_NEUTRAL)
                Out.WriteByte((byte)(Spawn.GetUnk(0) & 0xFF));
            else
                Out.WriteByte((byte)Realm);

            Out.WriteUInt16(Spawn.GetUnk(1));
            Out.WriteUInt16(Spawn.GetUnk(2));
            Out.WriteByte(Spawn.Unk1);

            int flags = Spawn.GetUnk(3);

            if (Realm != GameData.Realms.REALMS_REALM_NEUTRAL && !IsInvulnerable)
                flags |= 8; // Attackable (stops invalid target errors)

            LootContainer lootsContainer = LootsMgr.GenerateLoot(this, plr, 1);

            if ((lootsContainer != null && lootsContainer.IsLootable()) || ((plr.QtsInterface.PublicQuest != null) && Interactable) || (plr.QtsInterface.GameObjectNeeded(Spawn.Entry) && Interactable) || Spawn.Entry == 188 || Spawn.DoorId != 0)
            {
                flags |= 4; // Interactable
            }

            if(Flags.HasValue)
                Out.WriteUInt16(Flags.Value);
            else
                Out.WriteUInt16((ushort)flags);

            Out.WriteByte(Spawn.Unk2);
            Out.WriteUInt32(Spawn.Unk3);
            Out.WriteUInt16(Spawn.GetUnk(4));
            Out.WriteUInt16(Spawn.GetUnk(5));
            Out.WriteUInt32(Spawn.Unk4);

            Out.WritePascalString(Name);

            if (Spawn.DoorId != 0)
            {
                Out.WriteByte(0x04);

                Out.WriteUInt32(Spawn.DoorId);
            }
            else
                Out.WriteByte(0x00);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        #region Interaction

        private bool _interactable;
        public bool Interactable
        {
            get { return _interactable; }
            set
            {
                if (_interactable == value)
                    return;

                _interactable = value;
                foreach (Player plr in PlayersInRange)
                    SendMeTo(plr);
            }
        }

        private byte _interactState;
        public byte InteractState
        {
            get
            {
                return _interactState;
            }

            set
            {
                _interactState = value;
                UpdateInteractState(value);
            }
        }

        private Func<Player, bool> _captureCheck;
        private Action<Player, GameObject> _onCapture;

        public void AssignCaptureCheck(Func<Player, bool> check)
        {
            _captureCheck = check;
        }

        public void AssignCaptureDelegate(Action<Player, GameObject> del)
        {
            _onCapture = del;
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (_captureCheck != null && !_captureCheck(player))
                return;

            if (CaptureDuration > 0)
                BeginInteraction(player);

            else
            {
                HandleInteractionEvents(player);

                TryLoot(player, menu);

                switch (Spawn.Entry)
                {
                    case 242:
                    case 511:       // Inside hunters vale portal  // vale vine
                    case 99891:     // Hardcoded portal for Thanquil's incursion 
                    case 100132:
                    case 99667:
                    case 98845:
                    case 344:
                    case 100575:
                    case 100576:
                    case 100577:
                    case 100578:
                    case 99644:
                    case 100610:
                    case 100611:
                    case 100612:
                        ZoneJump(player);
                        return;
                        
                    case 98878:     // Hardcoded portal for Gunbad
                        ZoneJump(player, 60);
                        return;
                }

                if (Spawn.DoorId != 0)
                {
                    if (VfxState == 0)
                        OpenDoor(true);
                }

                else
                {
                    Region?.Scenario?.Interact(this, player, menu);

                    base.SendInteract(player, menu);
                }
            }
        }

        // This variable stores the last use of GO for future reference
        public long LastUsedTimestamp = 0;

        private void HandleInteractionEvents(Player player)
        {
            if (!IsDead)
                player.QtsInterface.HandleEvent(Objective_Type.QUEST_USE_GO, Spawn.Entry, 1);

            // This will spawn a creature after interacting with the GO
            if (this.Spawn.Proto.CreatureId != 0 && IsAttackable == 0) SpawnNPCFromGO();

            // This will play sound after clicking on the GO
            long now = TCPManager.GetTimeStampMS();
            if (this.Spawn.SoundId != 0 && now > SoundCooldown)
            {
                this.PlaySound((ushort)this.Spawn.SoundId);
                SoundCooldown = TCPManager.GetTimeStampMS() + 60 * 1000;
            }

            HandleCustomInteraction();

            PublicQuest pq = player.QtsInterface.PublicQuest;
            pq?.HandleEvent(player, Objective_Type.QUEST_USE_GO, Spawn.Entry, 1, 50);

            if (Spawn.Proto.TokUnlock != null && Spawn.Proto.TokUnlock.Length > 1 && IsAttackable == 0)
            { 
                player.TokInterface.AddToks(Spawn.Proto.TokUnlock);

                // This check if the GO we clicked is a Door - if it is we don't want to mess with its 
                // VFXState because we can break it
                if (this.Spawn.DoorId == 0)
                {
                    this.EvtInterface.AddEvent(EventSendMeTo, 60000, 1);
                    if (this.Spawn.AllowVfxUpdate == 1) this.VfxState = 1;
                    foreach (Player plr in PlayersInRange)
                        this.UpdateVfxState(VfxState);
                }
            }

            if (Spawn.TokUnlock != null && Spawn.TokUnlock.Length > 1 && IsAttackable == 0)
            { 
                player.TokInterface.AddToks(Spawn.TokUnlock);
                
                // This check if the GO we clicked is a Door - if it is we don't want to mess with its 
                // VFXState because we can break it
                if (this.Spawn.DoorId == 0)
                {
                    this.EvtInterface.AddEvent(EventSendMeTo, 60000, 1);

                    if (this.Spawn.AllowVfxUpdate == 1) this.VfxState = 1;
                    foreach (Player plr in PlayersInRange)
                        this.UpdateVfxState(VfxState);
                }
            }
        }

        // This is for handling custom GO interaction
        private void HandleCustomInteraction()
        {
            if (Entry == 2000579) // This is Mourkain Gem for Ard 'ta Feed Gunbad Boss and part of his mechanics
            {
                if (LastUsedTimestamp == 0)
                {
                    Say("*** Sinister energy is gone from the gem... ***", ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                }

                LastUsedTimestamp = TCPManager.GetTimeStampMS();

                bool removeBuff = false;

                foreach (Object o in ObjectsInRange)
                {
                    GameObject go = o as GameObject;
                    if (go != null && go != this && go.Entry == 2000579) // This is 2nd Mourkain Gem
                    {
                        if (go.LastUsedTimestamp != 0 && (LastUsedTimestamp - go.LastUsedTimestamp < 2001))
                        {
                            LastUsedTimestamp = 0;
                            go.LastUsedTimestamp = 0;
                            removeBuff = true;
                        }
                        else if (go.LastUsedTimestamp != 0)
                        {
                            LastUsedTimestamp = 0;
                            go.LastUsedTimestamp = 0;
                            Say("*** You are too late! Better hurry and try again! ***", ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        }
                    }
                }

                if (removeBuff)
                {
                    foreach (Object ob in ObjectsInRange) // This is looking for ard to feed in range
                    {
                        Creature c = ob as Creature;
                        if (c != null && c.Entry == 15102) // This is ard ta feed
                        {
                            c.BuffInterface.RemoveBuffByEntry(20364);
                            NewBuff newBuff = c.BuffInterface.GetBuff(20364, null);
                            if (newBuff != null)
                                newBuff.RemoveBuff(true);

                            c.AbtInterface.StartCast(c, 5308, 0);
                            Say("*** Wicked energy evaporates from the cursed jewel... ***", ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                            c.Say("*** Monstrous squig calms a bit... ***", ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                        }

                        GameObject gobject = ob as GameObject;
                        if (gobject != null && gobject.Entry == 98876) // This is Mourkain Henge
                        {
                            gobject.VfxState = 0;
                            foreach (Player player in gobject.PlayersInRange)
                            {
                                gobject.SendMeTo(player);
                            }
                        }
                    }
                }
                
            }
        }

        private void SpawnNPCFromGO(Unit killer = null)
        {
            long now = TCPManager.GetTimeStampMS();
            if (now > CreatureSpawnCooldown)
            {
                Random rand = new Random();

                Creature_proto Proto = CreatureService.GetCreatureProto(this.Spawn.Proto.CreatureId);
                for (int i = 0; i < this.Spawn.Proto.CreatureCount; i++) // CreatureCount - how many creatures are spawned
                {
                    Creature_spawn CreSpawn = new Creature_spawn();
                    CreSpawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                    CreSpawn.BuildFromProto(Proto);
                    CreSpawn.WorldO = this.Spawn.WorldO;
                    CreSpawn.WorldX = (int)(this.Spawn.WorldX + 50 - 150 * rand.NextDouble());
                    CreSpawn.WorldY = (int)(this.Spawn.WorldY + 50 - 150 * rand.NextDouble());
                    CreSpawn.WorldZ = this.Spawn.WorldZ;
                    CreSpawn.ZoneId = this.Spawn.ZoneId;
                    Creature c = null;
                    if (killer != null && killer is Player)
                        c = killer.Region.CreateCreature(CreSpawn);
                    else
                        c = this.Region.CreateCreature(CreSpawn);

                    if (this.Spawn.Proto.CreatureCooldownMinutes > 0)
                    {
                        CreatureSpawnCooldown = TCPManager.GetTimeStampMS() + (int)this.Spawn.Proto.CreatureCooldownMinutes * 60000; // Cannot be spawned more often than once per respawn time
                        c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveNPC);
                        c.EvtInterface.AddEvent(c.Destroy, (int)this.Spawn.Proto.CreatureCooldownMinutes * 60000, 1); // Creature goes away after the respawn time
                        this.EvtInterface.AddEvent(EventSendMeTo, (int)this.Spawn.Proto.CreatureCooldownMinutes * 60000, 1);
                    }
                    else
                    {
                        CreatureSpawnCooldown = TCPManager.GetTimeStampMS() + 120000; // Creature cannot be spawned more than once every 120 second if it is normal NPC
                        c.EvtInterface.AddEventNotify(EventName.OnDie, RemoveNPC);
                        c.EvtInterface.AddEvent(c.Destroy, 120000, 1); // Creature goes away after 2 minutes
                        this.EvtInterface.AddEvent(EventSendMeTo, 120000, 1);
                    }

                    var prms = new List<object>() { c, Spawn.Proto.CreatureSpawnText };

                    if (!String.IsNullOrEmpty(this.Spawn.Proto.CreatureSpawnText)) // It is possible to allow the GO to say something after the NPCs spawned
                        //if (c != null)
                            EvtInterface.AddEvent(DelayedSay, 500, 1, prms);
                            //c.Say(this.Spawn.Proto.CreatureSpawnText, ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
                }

                /*if (!String.IsNullOrEmpty(this.Spawn.Proto.CreatureSpawnText)) // It is possible to allow the GO to say something after the NPCs spawned
                    this.Say(this.Spawn.Proto.CreatureSpawnText, ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);*/

                // This check if the GO we clicked is a Door - if it is we don't want to mess with its 
                // VFXState because we can break it
                if (this.Spawn.DoorId == 0)
                {
                    this.EvtInterface.AddEvent(EventSendMeTo, 60000, 1);
                    if (this.Spawn.AllowVfxUpdate == 1) this.VfxState = 1;
                    foreach (Player plr in PlayersInRange)
                        this.UpdateVfxState(this.VfxState);
                }
            }
            else
            {
                this.Say("**You must wait a little longer...**", ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
            }
        }

        public void DelayedSay(object creature)
        {
            var Params = (List<object>)creature;
            Creature c = Params[0] as Creature;
            string s = (string)Params[1];

            c.Say(s, ChatLogFilters.CHATLOGFILTERS_MONSTER_SAY);
        }

        private bool RemoveNPC(Object npc = null, object instigator = null)
        {
            Creature c = npc as Creature;
            if (c != null) c.EvtInterface.AddEvent(c.Destroy, 20000, 1);

            return false;
        }

        private void EventSendMeTo()
        {
            if (this.Spawn.AllowVfxUpdate == 1) this.VfxState = 0;
            foreach (Player plr in PlayersInRange)
                this.UpdateVfxState(this.VfxState);
        }

        public override void NotifyInteractionComplete(NewBuff b)
        {
            HandleInteractionEvents(CapturingPlayer);

            _onCapture?.Invoke(CapturingPlayer, this);

            base.NotifyInteractionComplete(b);
        }

        #endregion

        public void UpdateVfxState(byte vfxState)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 20);
            Out.WriteUInt16(Oid);
            Out.WriteByte(6); //state
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(8);
            Out.WriteByte(0);
            Out.WriteByte(vfxState);
            Out.Fill(0, 10);
            DispatchPacket(Out, false);
        }

        protected override void SetDeath(Unit killer)
        {
            if (Spawn.DoorId == 0)
            {
                if (Spawn.AllowVfxUpdate == 1)
                    UpdateVfxState(1);
                else
                    IsActive = false;
            }
            else
                OpenDoor(false);

            // I'm ashamed by the way I fixed it...
            if (Name != "Keep Door" && Entry != 100 && Respawn == 1)
                SetRespawnTimer();

            if (this.IsAttackable == 1 && this.Spawn.Proto.CreatureId != 0) SpawnNPCFromGO();

            base.SetDeath(killer);

            AiInterface.ProcessCombatEnd();
        }

        protected virtual void SetRespawnTimer()
        {
            int baseRespawn = 90 * 1000;
            EvtInterface.AddEvent(RezUnit, baseRespawn, 1); // 30 seconde Rez
        }

        public override void RezUnit()
        {
            Region.CreateGameObject(Spawn);
            Destroy();
        }

        protected override void HandleDeathRewards(Player killer)
        {
            if (Spawn.Proto.TokUnlock != null && Spawn.Proto.TokUnlock.Length > 1 && Spawn.Proto.HealthPoints > 1)
            {
                killer.TokInterface.AddToks(Spawn.Proto.TokUnlock);
            }

            if (Spawn.TokUnlock != null && Spawn.TokUnlock.Length > 1 && Spawn.Proto.HealthPoints > 1)
            {
                killer.TokInterface.AddToks(Spawn.TokUnlock);
            }
           CreditQuestKill(killer);
        }

        protected void CreditQuestKill(Player killer)
        {
            if (killer.PriorityGroup != null)
            {
                List<Player> curMembers = killer.PriorityGroup.GetPlayersCloseTo(killer, 150);
            }

            if (!string.IsNullOrEmpty(Spawn.Proto.TokUnlock))
            {
                killer.TokInterface.AddToks(Spawn.Proto.TokUnlock);

                if (killer.WorldGroup != null)
                {
                    List<Player> members = killer.WorldGroup.GetPlayerListCopy();

                    foreach (var member in members)
                        if (member != killer)
                            member.TokInterface.AddToks(Spawn.Proto.TokUnlock);
                }
            }

            killer.QtsInterface.HandleEvent(Objective_Type.QUEST_KILL_GO, Spawn.Entry, 1);
            PublicQuest pq = killer.QtsInterface.PublicQuest;
            pq?.HandleEvent(killer, Objective_Type.QUEST_KILL_GO, Spawn.Entry, 1, 100);
        }

        #region Teleport

        private void ZoneJump(Player player)
        {
            #if DEBUG
            Log.Info("Jump", "Jump to :" + Spawn.Guid);
            #endif

            Zone_jump jump = ZoneService.GetZoneJump(Spawn.Guid);

            if (jump == null)
                return;

            if (jump.Type == 3)
            {
                if (!player.GldInterface.IsInGuild() || player.GldInterface.GetGuildLevel() < 6)
                {
                    player.SendClientMessage("A Guild Rank of 6 is required to use this portal.");
                    player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, GameData.Localized_text.TEXT_PLAYER_REGION_NOT_AVAILABLE);
                    return;
                }
            }
            if (jump.Enabled || Utils.HasFlag(player.GmLevel, (int) EGmLevel.DatabaseDev))
                player.Teleport(jump.ZoneID, jump.WorldX, jump.WorldY, jump.WorldZ, jump.WorldO);
        }

        private void ZoneJump(Player player, ushort ZoneId) // This is used to teleport players outside from instances to the entrance of dungeon
        {
            uint X = 0, Y = 0;
            ushort Z = 0, O = 0;

            switch (ZoneId)
            {
                case 60:
                    X = 851953;
                    Y = 848249;
                    Z = 28899;
                    O = 3846;
                    break;
            }

            player.Teleport(ZoneId, X,Y,Z,O);
        }

        #endregion

        #region Loot

        private const int RELOOTABLE_TIME = 120000; // 2 Mins

        private bool _looted;
        public bool Looted
        {
            get { return _looted; }
            set
            {
                if (_looted == value) return;
                _looted = value;
                foreach (Player plr in PlayersInRange)
                    SendMeTo(plr);
            }
        }

        public override void TryLoot(Player player, InteractMenu menu)
        { 
            LootContainer lootsContainer = LootsMgr.GenerateLoot(this, player, 1);

            if (lootsContainer != null)
            {
                lootsContainer.SendInteract(player, menu);
                // If object has been looted, make it unlootable
                // and then Reset its lootable staus in XX seconds
                if (!lootsContainer.IsLootable())
                {
                    Looted = true;
                    EvtInterface.AddEvent(ResetLoot, RELOOTABLE_TIME, 1);
                }
            }
        }

        // This will reset the GameObject loot after it has
        // been looted. Allowing it to be looted again.
        public void ResetLoot()
        {
            Looted = false;
        }

        #endregion

        #region Door

        public void OpenDoor(bool autoClose)
        {
            if (VfxState == 1)
                return;

            VfxState = 1;

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 20);
            Out.WriteUInt16(Oid);
            Out.WriteByte(6); //state
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(8);
            Out.WriteByte(0);
            Out.WriteByte(1);
            Out.Fill(0, 10);
            DispatchPacket(Out, false);


            if (Zone != null && Zone.Region != null && Spawn != null)
                Occlusion.SetFixtureVisible(Spawn.DoorId, false);

            if (autoClose)
                EvtInterface.AddEvent(CloseDoor, 7000, 1);
        }

        public void CloseDoor()
        {
            EvtInterface.RemoveEvent(CloseDoor);

            if (VfxState == 0)
                return;

            VfxState = 0;

            if (Zone != null && Zone.Region != null && Spawn != null)
                Occlusion.SetFixtureVisible(Spawn.DoorId, true); 

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 20);
                Out.WriteUInt16(Oid);
                Out.WriteByte(6); //state
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(8);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.Fill(0, 10);
            DispatchPacket(Out, false);
        }

        #endregion

        public override string ToString()
        {
            return "SpawnId=" + Spawn.Guid + ",Entry=" + Spawn.Entry + ",Name=" + Name + ",Level=" + Level + ",Faction=" + Faction + ",Position :" + base.ToString();
        }
    }
}

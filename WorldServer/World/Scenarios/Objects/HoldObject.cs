using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Object = WorldServer.World.Objects.Object;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios.Objects
{
    public enum EHeldState
    {
        Inactive,
        Home,
        Carried,
        Ground
    }
    /// <summary>
    /// An object which can be picked up by players.
    /// </summary>
    public class HoldObject : Object
    {
        public string name;

        protected readonly EventInterface _evtInterface = new EventInterface();

        public Player Holder;

        public uint Identifier { get; protected set; }
        public byte ObjectType = 2;

        public EHeldState HeldState { get; private set; } = EHeldState.Inactive;

        // Mounting
        public bool PreventsRide = true;

        // Appearance
        protected ushort GroundModelId, HomeModelId;
        public ushort VfxState;

        private GameObject _glowObject;
        public bool ColorMatchesRealm = false;

        // Resetting
        protected Point3D HomePosition;

        // Delegates
        public delegate void InteractAction(HoldObject ball, Player holder);
        public delegate void BallAction(HoldObject ball);

        protected InteractAction OnPickupAction;
        protected BallAction OnDropAction, OnResetAction;
        protected BuffQueueInfo.BuffCallbackDelegate OnBuffCallback;

        // Granted Buff
        protected ushort BuffId;
        protected int GroundResetTime;
        public int HoldResetTimeSeconds;
        private long _holdResetTime;
        protected HoldObjectBuff MyBuff;

        public HoldObject(uint identifier, string name, Point3D homePosition, ushort buffId, int groundResetTime, InteractAction onPickupAction, BallAction onDropAction, BallAction onResetAction, BuffQueueInfo.BuffCallbackDelegate onBuffCallback, ushort groundModelId, ushort homeModelId)
        {
            IsActive = false;
            Identifier = identifier;
            this.name = name;
            HomePosition = homePosition;
            BuffId = buffId;
            //Heading = 1024;

            GroundResetTime = groundResetTime;

            OnPickupAction = onPickupAction;
            OnDropAction = onDropAction;
            OnResetAction = onResetAction;
            OnBuffCallback = onBuffCallback;

            GroundModelId = groundModelId;
            HomeModelId = homeModelId;

            CaptureDuration = 3;
        }

        public override void OnLoad()
        {
            X = Zone.CalculPin((uint)HomePosition.X, true);
            Y = Zone.CalculPin((uint)HomePosition.Y, false);
            Z = HomePosition.Z;

            base.OnLoad();

            WorldPosition.SetCoordsFrom(HomePosition);

            HomePosition = new Point3D(WorldPosition);

            SetOffset((ushort)(HomePosition.X >> 12), (ushort)(HomePosition.Y >> 12));

            // Spawn objective glow (Big Poofy Glowy Nuet)
            GameObject_proto glowProto = GameObjectService.GetGameObjectProto(99858);
            if (glowProto == null)
                return;

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldO = Heading,
                WorldY = WorldPosition.Y + 2,
                WorldZ = WorldPosition.Z,
                WorldX = WorldPosition.X + 6,
                ZoneId = Zone.ZoneId,
            };
            spawn.BuildFromProto(glowProto);

            _glowObject = new GameObject(spawn);

            SetGlowColorFor(RealmCapturableFor);

            Region.AddObject(_glowObject, spawn.ZoneId);
        }

        public override void Update(long msTick)
        {
            if (PendingDisposal)
            {
                Dispose();
                return;
            }

            if (_holdResetTime > 0 && HeldState == EHeldState.Carried && _holdResetTime < msTick)
                ResetFromHeld();

            _evtInterface.Update(msTick);
        }

        private void SetGlowColorFor(Realms realm)
        {
            switch (realm)
            {
                case Realms.REALMS_REALM_NEUTRAL:
                    _glowObject.VfxState = 0;
                    break;
                case Realms.REALMS_REALM_ORDER:
                    if (ColorMatchesRealm)
                        _glowObject.VfxState = 1;
                    else
                        _glowObject.VfxState = 2;
                    break;
                case Realms.REALMS_REALM_DESTRUCTION:
                    if (ColorMatchesRealm)
                        _glowObject.VfxState = 2;
                    else
                        _glowObject.VfxState = 1;
                    break;
            }
        }

        #region Capturing

        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (HeldState == EHeldState.Inactive || HeldState == EHeldState.Carried || (RealmCapturableFor != 0 && player.Realm != RealmCapturableFor))
                return;

            if (player.StealthLevel > 0)
            {
                player.SendClientMessage("You can't interact with objects while in stealth", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return;
            }

            if (player.HeldObject != null)
            {
                player.SendClientMessage("You can't carry more than one object", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return;
            }

            if (HeldState == EHeldState.Ground)
                _evtInterface.RemoveEvent(ResetFromGround);

            BeginInteraction(player);
        }

        public override void NotifyInteractionComplete(NewBuff b)
        {
            if (CapturingPlayer == null || HeldState == EHeldState.Inactive)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.Fill(0, 6);
            DispatchPacket(Out, true);

            // Automatically sets object invisible
            SetHeldState(EHeldState.Carried);

            Holder = CapturingPlayer;
            CapturingPlayer = null;

            BuffInfo buffInfo = AbilityMgr.GetBuffInfo(BuffId);

            buffInfo.Duration = 0;

            Holder.BuffInterface.QueueBuff(new BuffQueueInfo(Holder, Holder.EffectiveLevel, buffInfo, HoldObjectBuff.GetNew, HoldObjectCallback));

            OnPickupAction?.Invoke(this, Holder);

        }

        public override void SendRemove(Player plr)
        {
            /*if (plr != null)
                Log.Info("SendRemove", "Removing from "+plr.Name);
            else 
                Log.Info("SendRemove", "Removing from all");*/

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.Fill(0, 6);

            if (plr != null)
                plr.SendPacket(Out);
            else
                DispatchPacket(Out, false);
        }

        #endregion

        public Realms RealmCapturableFor { get; protected set; }

        public void SetActive(Realms realmAssociation)
        {
            SetRealmAssociation(realmAssociation);

            SetHeldState(EHeldState.Home);
        }

        public void SetHeldState(EHeldState newState)
        {
            HeldState = newState;

                switch (HeldState)
                {
                    case EHeldState.Carried:
                        if (_glowObject != null && _glowObject.IsActive)
                            _glowObject.IsActive = false;
                        IsActive = false;
                        if (HoldResetTimeSeconds > 0)
                        {
                            _holdResetTime = TCPManager.GetTimeStampMS() + HoldResetTimeSeconds*1000;
                            CapturingPlayer?.SendClientMessage($"You may hold this object for up to {HoldResetTimeSeconds / 60} minutes before it will reset.");
                            CapturingPlayer?.SendClientMessage($"You may hold this object for up to {HoldResetTimeSeconds / 60} minutes before it will reset.", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                        }
                        break;
                    case EHeldState.Ground:
                        _holdResetTime = 0;
                        if (_glowObject != null && _glowObject.IsActive)
                            _glowObject.IsActive = false;
                        IsActive = true;
                        break;
                    case EHeldState.Home:
                        _holdResetTime = 0;
                        if (_glowObject != null && !_glowObject.IsActive)
                            _glowObject.IsActive = true;
                        IsActive = true;
                        break;
                    case EHeldState.Inactive:
                        _holdResetTime = 0;
                        if (_glowObject != null && !_glowObject.IsActive)
                            _glowObject.IsActive = true;
                        IsActive = false;
                        break;
                }
        }

        public void SetRealmAssociation(Realms realm)
        {
            if (RealmCapturableFor == realm)
                return;

            RealmCapturableFor = realm;

            if (_glowObject != null && _glowObject.IsActive)
            {
                SetGlowColorFor(RealmCapturableFor);

                PacketOut Out = new PacketOut((byte) Opcodes.F_UPDATE_STATE, 20);

                Out.WriteUInt16(_glowObject.Oid);
                Out.WriteByte(6); //state
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(8);
                Out.WriteByte(0);
                Out.WriteByte(_glowObject.VfxState);
                Out.Fill(0, 10);

                DispatchPacket(Out, false);
            }
        }

        public virtual void HoldObjectCallback(NewBuff b)
        {
            if (b == null)
                HolderDied();

            else if (Holder != null)
            {
                MyBuff = (HoldObjectBuff)b;
                MyBuff.HeldObject = this;
                OnBuffCallback?.Invoke(b);
            }
        }

        public void ForceDrop()
        {
            _holdResetTime = 0;

            if (MyBuff != null)
            {
                MyBuff.BuffHasExpired = true;
                MyBuff = null;
            }
        }

        public virtual void HolderDied()
        {
            if (Holder == null)
                return;

            WorldPosition.X = Holder.WorldPosition.X + 32;
            WorldPosition.Y = Holder.WorldPosition.Y;
            WorldPosition.Z = Holder.WorldPosition.Z;

            X = Holder.X + 32;
            Y = Holder.Y;
            Z = Holder.Z;

            SetOffset((ushort)(WorldPosition.X >> 12), (ushort)(WorldPosition.Y >> 12));

            // Automatically sets object visible
            SetHeldState(EHeldState.Ground);

            _evtInterface.AddEvent(ResetFromGround, GroundResetTime, 1);

            OnDropAction?.Invoke(this);

            Holder = null;
        }

        public virtual void ResetFromGround()
        {
            ClearRange();

            X = Zone.CalculPin((uint)HomePosition.X, true);
            Y = Zone.CalculPin((uint)HomePosition.Y, false);
            Z = HomePosition.Z;

            WorldPosition.X = HomePosition.X;
            WorldPosition.Y = HomePosition.Y;
            WorldPosition.Z = HomePosition.Z;

            SetOffset((ushort)(HomePosition.X >> 12), (ushort)(HomePosition.Y >> 12));

            // Object already visible so we must manually update range
            SetHeldState(EHeldState.Home);
            
            Region.UpdateRange(this, true);

            OnResetAction?.Invoke(this);
        }

        public virtual void ResetFromHeld()
        {
            Holder?.SendClientMessage("You have held this object for too long and it has been reset.");
            Holder?.SendClientMessage("You have held this object for too long and it has been reset.", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
            ResetTo(EHeldState.Home);
        }

        public void ResetTo(EHeldState newState)
        {
            if (MyBuff != null)
            {
                MyBuff.BuffHasExpired = true;
                MyBuff = null;
            }

            if (HeldState == newState)
                return;

            SetHeldState(newState);

            if (Holder == null)
                ClearRange();

            X = Zone.CalculPin((uint)HomePosition.X, true);
            Y = Zone.CalculPin((uint)HomePosition.Y, false);
            Z = HomePosition.Z;

            WorldPosition.X = HomePosition.X;
            WorldPosition.Y = HomePosition.Y;
            WorldPosition.Z = HomePosition.Z;

            SetOffset((ushort)(WorldPosition.X >> 12), (ushort)(WorldPosition.Y >> 12));

            Region.UpdateRange(this, true);

            OnResetAction?.Invoke(this);

            Holder = null;
        }

        public override void SendMeTo(Player plr)
        {
            if (HeldState == EHeldState.Carried || HeldState == EHeldState.Inactive)
            {
                /*if (plr != null)
                    Log.Info("SendMeTo", "Sending to " + plr.Name + " failed: state is "+Enum.GetName(typeof(EHeldState), HeldState));
                else
                    Log.Info("SendMeTo", "Sending to all failed: state is "+Enum.GetName(typeof(EHeldState), HeldState));*/
                return;
            }

            /*if (plr != null)
                Log.Info("SendMeTo", "Sending to " + plr.Name);
            else
                Log.Info("SendMeTo", "Sending to all");*/

            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(VfxState);

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt32((uint)WorldPosition.X);
            Out.WriteUInt32((uint)WorldPosition.Y);

            Out.WriteUInt16(HeldState == EHeldState.Ground ? GroundModelId : HomeModelId);

            Out.WriteByte(0x1E);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0x1C23);
            Out.WriteByte(0);

            // flags
            Out.WriteUInt16(0x24);

            Out.WriteByte(0);

            Out.WriteUInt16(0);
            Out.WriteByte(0);
            Out.WriteByte(100);

            Out.WriteUInt32(0xAB942AEC);
            Out.WriteUInt32(0);

            Out.WritePascalString(name);
            Out.WriteByte(0);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }
    }
}
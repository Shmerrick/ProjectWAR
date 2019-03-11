using System;
using System.Collections.Generic;
using SystemData;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.Buffs.SpecialBuffs;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Abilities.Objects;
using WorldServer.World.Interfaces;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    public class RvRStructure : Unit
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private enum EConstructionState
        {
            Constructing,
            Active,
            Destroying,
            Destroyed
        }

        private readonly RvRObjectInfo _info;
        private readonly Vector2 _unitDirVec;

        private readonly HashSet<Player> _interactors = new HashSet<Player>();
        private readonly List<NewBuff> _interactionBuffs = new List<NewBuff>();

        private EConstructionState _buildState = EConstructionState.Constructing;

        public RvRStructure(RvRObjectInfo info, Point3D loc, ushort rot, Player constructor)
        {
            // _logger.Debug($"RVRStructure constructor {info.ModelId} {info.Name} {loc.ToString()} {rot} {constructor.Name}");

            WorldPosition.X = loc.X;
            WorldPosition.Y = loc.Y;
            WorldPosition.Z = loc.Z - 36;

            Heading = rot;

            _unitDirVec = Vector2.HeadingToVector(Heading, 1);
            _unitDirVec.Normalize();

            _info = info;
            _constructor = constructor;
        }

        public override void OnLoad()
        {
            Faction = _constructor.Faction;
            Realm = _constructor.Realm;

            Level = _constructor.Level;
            MaxHealth = (uint)(_info.MaxHealth * Level/40f);
            Health = TotalHealth/10;

            X = Zone.CalculPin((uint)(WorldPosition.X), true);
            Y = Zone.CalculPin((uint)(WorldPosition.Y), false);
            Z = (ushort)(WorldPosition.Z);

            SetOffset((ushort)(WorldPosition.X >> 12), (ushort)(WorldPosition.Y >> 12));
            base.OnLoad();
            IsActive = true;

            // _logger.Debug($"RVRStructure OnLoad Oid={Oid} {IsActive} {Faction} {Realm} {Level} {X} {Y} {Z} {XOffset} {YOffset} ");

        }

        public override void SendMeTo(Player plr)
        {
            // Log.Info("STATIC", "Creating static oid=" + Oid + " name=" + Name + " x=" + Spawn.WorldX + " y=" + Spawn.WorldY + " z=" + Spawn.WorldZ + " doorID=" + Spawn.DoorId);
            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_STATIC, 46 + (Name?.Length ?? 0));
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0); //ie: red glow, open door, lever pushed, etc

            Out.WriteUInt16(Heading);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt32((uint)WorldPosition.X);
            Out.WriteUInt32((uint)WorldPosition.Y);
            Out.WriteUInt16(_info.ModelId);

            Out.WriteByte(50);

            Out.WriteByte((byte)Realm);

            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            int flags = 0;

            if (Realm != Realms.REALMS_REALM_NEUTRAL && !IsInvulnerable)
                flags |= 8; // Attackable (stops invalid target errors)
            if (plr.Realm == Realm)
                flags |= 4; // Interactable

            Out.WriteUInt16((ushort)flags);

            Out.WriteByte(0);
            Out.WriteUInt32(0);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0);
            Out.WriteUInt32(0);

            Out.WritePascalString(_info.Name);

            Out.WriteByte(0x00);

            plr.SendPacket(Out);

            base.SendMeTo(plr);
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            Vector2 toPlayerVec = new Vector2(player.WorldPosition.X - WorldPosition.X, player.WorldPosition.Y - WorldPosition.Y);

            float dist = Vector2.DotProduct2D(toPlayerVec, _unitDirVec);

            if (Math.Abs(dist) * 0.083f > _info.MaxInteractionDist)
            {
                player.SendClientMessage("Too far from palisade", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                player.SendClientMessage("Too far away from the " + _info.Name);
                return;
            }

            if (player.Palisade != null && player.Palisade != this)
            {
                player.SendClientMessage("Already in cover", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                player.SendClientMessage("Already interacting with another palisade.");
                return;
            }

            switch (_buildState)
            {
                case EConstructionState.Constructing:
                    BeginInteraction(player);
                    break;
                case EConstructionState.Active:
                    HandleUser(player);
                    break;
            }

            // _logger.Debug($"RVRStructure Sendinteract Oid={Oid} {IsActive} {_buildState} {Faction} {Realm} {Level} {X} {Y} {Z} {XOffset} {YOffset} ");
        }

        public override void Update(long msTick)
        {
            switch (_buildState)
            {
                case EConstructionState.Constructing:
                    ProcessConstruction(msTick);
                    break;
                case EConstructionState.Destroying:
                    ProcessDestruction();
                    break;
                case EConstructionState.Destroyed:
                    foreach (Player player in PlayersInRange)
                        SendRemove(player);
                    PendingDisposal = true;
                    break;
            }

            base.Update(msTick);
        }

        #region Construction

        private readonly Player _constructor;
        private int _buildProgress;
        private float _buildProgressFloat;
        private long _lastTick;

        public static void CreateNew(Player plr, int entry)
        {
            Point3D destPos;
            ushort destHeading;

            RvRStructure curStruct = plr.CbtInterface.GetCurrentTarget() as RvRStructure;

            RvRObjectInfo info = BattleFrontService.GetRvRObjectInfo(entry);
            // _logger.Debug($"RVRStructure BattleFrontService.GetRvRObjectInfo Entry={entry} info={info.Name} {info.ModelId} {info.ObjectId}");

            if (curStruct != null)
            {
                ushort shiftHeading = (ushort)((curStruct.Heading + 1024) & 0xFFF);
                destPos = new Point3D(curStruct.WorldPosition);
                destHeading = curStruct.Heading;

                Point2D offsetPoint = GetOffsetFromHeading(shiftHeading, (ushort)(info.ExclusionRadius + curStruct._info.ExclusionRadius + 1));

                Vector2 offsetVector = new Vector2(offsetPoint.X, offsetPoint.Y);
                offsetVector.Normalize();

                Vector2 toPlayer = new Vector2(plr.WorldPosition.X - curStruct.WorldPosition.X, plr.WorldPosition.Y - curStruct.WorldPosition.Y);
                toPlayer.Normalize();

                float dotP = Vector2.DotProduct2D(offsetVector, toPlayer);

                if (dotP >= 0)
                {
                    destPos.X += offsetPoint.X;
                    destPos.Y += offsetPoint.Y;
                }

                else
                {
                    destPos.X -= offsetPoint.X;
                    destPos.Y -= offsetPoint.Y;
                }
            }

            else
            {
                Point2D offset = GetOffsetFromHeading(plr.Heading, 10);
                destHeading = plr.Heading;
                destPos = new Point3D(plr.WorldPosition.X + offset.X, plr.WorldPosition.Y + offset.Y, plr.WorldPosition.Z);
            }

            foreach (Object obj in plr.ObjectsInRange)
            {
                RvRStructure structure = obj as RvRStructure;

                if (structure == null)
                    continue;

                if (structure.WorldPosition.GetDistanceTo(destPos) < info.ExclusionRadius + structure._info.ExclusionRadius)
                {
                    plr.SendClientMessage("Too close to existing structure", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                    plr.SendClientMessage("Too close to an existing structure (one exists within "+structure.WorldPosition.GetDistanceTo(destPos)+"ft and "+ (info.ExclusionRadius + structure._info.ExclusionRadius) +"ft clearance is required)", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }
            }

            RvRStructure newStruct = new RvRStructure(info, destPos, destHeading, plr);

            plr.Region.AddObject(newStruct, plr.Zone.ZoneId, true);

            // _logger.Debug($"RVRStructure CreateNew Entry={entry} {curStruct.Name} {curStruct.Oid} Zone={plr.Zone.ZoneId}");
        }

        private void ProcessConstruction(long tick)
        {
            float delta = (tick - _lastTick) * 0.001f;

            _lastTick = tick;

            if (_buildProgress < 100 && _interactors.Count > 0)
            {
                _buildProgressFloat += delta;

                if (_buildProgressFloat >= 1)
                {
                    _buildProgressFloat -= 1;
                    Health += MaxHealth / 10;
                    _buildProgress += 10;
                    WorldPosition.Z += 4;
                    Z += 4;

                    foreach (Player plr in PlayersInRange)
                    {
                        SendRemove(plr);
                        SendMeTo(plr);
                    }
                    SendState();
                }
            }

            if (_buildProgress >= 100)
            {
                foreach (Player player in _interactors)
                {
                    player.SendClientMessage("Completed construction of the " + _info.Name);
                    player.KneelDown(Oid, false);
                }

                _interactors.Clear();

                _buildState = EConstructionState.Active;
            }
        }

        public override void BeginInteraction(Player player)
        {
            if (_buildProgress < 100)
            {
                if (_interactors.Contains(player))
                {
                    foreach (NewBuff buff in _interactionBuffs)
                        if (buff.Caster == player)
                        {
                            buff.BuffHasExpired = true;
                            return;
                        }
                }

                else
                    player.BuffInterface.QueueBuff(new BuffQueueInfo(player, player.Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Interaction), InteractionBuff.GetNew, LinkToCaptureBuff));
            }
        }

        public override void LinkToCaptureBuff(NewBuff b)
        {
            if (b != null)
            {
                if (!_interactors.Contains((Player)b.Caster))
                {
                    InteractionBuff captureBuff = (InteractionBuff)b;
                    captureBuff.SetObject(this);
                    _interactors.Add((Player)b.Caster);
                    _interactionBuffs.Add(captureBuff);
                }

                else
                    b.BuffHasExpired = true;
            }
        }

        public override void NotifyInteractionBroken(NewBuff b)
        {
            Player plrCaster = (Player)b.Caster;
            _interactors.Remove(plrCaster);
            _interactionBuffs.Remove((InteractionBuff)b);
        }

        public override void NotifyInteractionComplete(NewBuff b)
        {
            Player plrCaster = (Player)b.Caster;
            _interactors.Remove(plrCaster);
            _interactionBuffs.Remove((InteractionBuff)b);
        }

        #endregion

        #region Usage

        private void HandleUser(Player player)
        {
            if (_interactors.Contains(player))
            {
                _interactors.Remove(player);

                for (int i = 0; i < _interactionBuffs.Count; ++i)
                {
                    if (_interactionBuffs[i].Target == player)
                    {
                        _interactionBuffs[i].BuffHasExpired = true;
                        _interactionBuffs.RemoveAt(i);
                        --i;
                    }
                }

                player.SendClientMessage("You are no longer in cover behind the "+_info.Name+".");
                player.Palisade = null;
            }

            else
            {
                _interactors.Add(player);

                player.Palisade = this;

                // Range increase buff
                player.BuffInterface.QueueBuff(new BuffQueueInfo(this, 1, AbilityMgr.GetBuffInfo(10356), RegisterUserBuff));

                // Cast time reduction buff
                player.BuffInterface.QueueBuff(new BuffQueueInfo(this, 1, AbilityMgr.GetBuffInfo(10936), RegisterUserBuff));

                player.SendClientMessage("You are in in cover behind the " + _info.Name + ".\nWhile in cover, you may not move, but your range is extended, your cast times are reduced, and the palisade will intercept direct attacks aimed at you.");

                player.EvtInterface.AddEventNotify(EventName.OnDie, EventRemovePlayer);
            }
        }

        private bool EventRemovePlayer(Object plr, object args)
        {
            Player player = (Player) plr;
            _interactors.Remove(player);

            for (int i = 0; i < _interactionBuffs.Count; ++i)
            {
                if (_interactionBuffs[i].Target == plr)
                {
                    _interactionBuffs[i].BuffHasExpired = true;
                    _interactionBuffs.RemoveAt(i);
                    --i;
                }
            }

            player.Palisade = null;

            return true;
        }

        public void RegisterUserBuff(NewBuff b)
        {
            if (b != null)
            {
                if (_interactors.Contains((Player)b.Target))
                    _interactionBuffs.Add(b);

                else
                    b.BuffHasExpired = true;
            }
        }

        #endregion

        #region Destruction

        private void ProcessDestruction()
        {
            if (_info.ModelId == 5470)
            {
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

                _buildState = EConstructionState.Destroyed;
                return;
            }

            if (_buildProgress > 0)
            {
                _buildProgressFloat -= 1f;

                if (_buildProgressFloat <= 0)
                {
                    _buildProgressFloat = 1;
                    _buildProgress -= 2;
                    WorldPosition.Z -= 3;
                    Z -= 3;

                    foreach (Player plr in PlayersInRange)
                    {
                        SendRemove(plr);
                        SendMeTo(plr);
                    }
                }
            }

            if (_buildProgress == 0)
                _buildState = EConstructionState.Destroyed;
        }

        #endregion

        #region Health/Damage

        protected override void SetDeath(Unit killer)
        {
            Health = 0;

            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(killer.IsPet() ? killer.GetPet().Owner.Oid : killer.Oid);
            Out.Fill(0, 6);
            DispatchPacket(Out, true);

            AbtInterface.Cancel(true);
            ScrInterface.OnDie(this);
            BuffInterface.RemoveBuffsOnDeath();

            EvtInterface.Notify(EventName.OnDie, this, killer);

            Pet pet = killer as Pet;
            Player credited = (pet != null) ? pet.Owner : (killer as Player);

            if (credited != null)
                HandleDeathRewards(credited);

            ClearTrackedDamage();

            _buildState = EConstructionState.Destroying;

            foreach (Player plr in _interactors)
                plr.Palisade = null;

            _interactors.Clear();

            foreach (NewBuff buff in _interactionBuffs)
                buff.BuffHasExpired = true;

            _interactionBuffs.Clear();

            GroundTarget gt = new GroundTarget(_constructor, new Point3D(WorldPosition), GameObjectService.GetGameObjectProto(23));

            Region.AddObject(gt, Zone.ZoneId);

            gt.BuffInterface.QueueBuff(new BuffQueueInfo(_constructor, 40, AbilityMgr.GetBuffInfo(23762)));

            gt.SetExpiry(TCPManager.GetTimeStampMS() + 5000);
        }

        public override void UpdateHealth(long tick)
        {

        }

        public override void ModifyDamageIn(AbilityDamageInfo incDamage)
        {
            switch (incDamage.SubDamageType)
            {
                case SubDamageTypes.Cleave:
                    incDamage.DamageReduction *= 0.1f;
                    break;
                case SubDamageTypes.Artillery:
                    incDamage.Damage *= 0.35f;
                    break;
                case SubDamageTypes.None:
                    switch (incDamage.DamageType)
                    {
                        case DamageTypes.Physical:
                            incDamage.DamageEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                            break;
                        case DamageTypes.RawDamage:
                            incDamage.Damage *= 0.1f;
                            break;
                        default:
                            incDamage.DamageReduction *= 0.1f;
                            break;
                    }
                    break;
            }
        }

        #endregion
    }
}

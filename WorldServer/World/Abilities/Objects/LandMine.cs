using Common;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using WorldServer.World.Positions;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.Objects
{
    class LandMine : BuffHostObject
    {
        private bool _hasExploded;
        private long _nextCheckInterval;

        public LandMine(Player owner, Point3D spawnVec, Creature_proto proto)
        {
            Owner = owner;
            Proto = proto;
            Realm = Owner.Realm;
            Level = Owner.EffectiveLevel;
            Name = Proto.Name;
            WorldPosition.X = spawnVec.X;
            WorldPosition.Y = spawnVec.Y;
            WorldPosition.Z = spawnVec.Z;

            IsInvulnerable = true;

            Faction = Realm == Realms.REALMS_REALM_ORDER ? (byte)0x41 : (byte)0x81;

            Health = 1;
            MaxHealth = 1;
        }

        public override void OnLoad()
        {
            X = Zone.CalculPin((uint)WorldPosition.X, true);
            Y = Zone.CalculPin((uint)WorldPosition.Y, false);
            Z = WorldPosition.Z;

            SetOffset((ushort)(WorldPosition.X >> 12), (ushort)(WorldPosition.Y >> 12));

            base.OnLoad();

            _nextCheckInterval = TCPManager.GetTimeStampMS() + 1000;
        }

        public override void Update(long msTick)
        {
            if (!_hasExploded && msTick > _nextCheckInterval)
            {
                foreach (var obj in ObjectsInRange)
                {
                    Unit victim = obj as Unit;

                    if (victim == null || victim.IsDead)
                        continue;

                    if (!(victim is Player) && !(victim is Creature))
                        continue;

                    if (victim.Realm != Owner.Realm && ObjectWithinRadiusFeet(victim, 30))
                    {
                        BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.Manual, null, victim);

                        _hasExploded = true;
                        return;
                    }
                }

                _nextCheckInterval = msTick + 1000;
            }

            if (PendingDisposal)
            {
                Dispose();
                return;
            }

            BuffInterface.Update(msTick);
            EvtInterface.Update(msTick);
        }

        protected override void SetDeath(Unit killer)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(Owner.Realm == Realms.REALMS_REALM_ORDER ? (ushort)24680 : (ushort)24681); // 00 00 07 D D
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteByte(5);   //7
            Out.WriteByte(0);
            DispatchPacket(Out, true);

            States.Add((byte)CreatureState.Dead); // Death State

            Out = new PacketOut((byte)Opcodes.F_OBJECT_DEATH, 12);
            Out.WriteUInt16(Oid);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteUInt16(0);
            Out.Fill(0, 6);
            DispatchPacket(Out, true);

            EvtInterface.AddEvent(Destroy, 1500, 0);
        }
    }
}

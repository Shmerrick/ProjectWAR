using System.Collections.Generic;
using System.Linq;
using FrameWork;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Interfaces
{
    public class ObjectStateInterface : BaseInterface
    {
        Dictionary<byte,byte> EffectList = new Dictionary<byte, byte>();

        public void SendObjectStates(Player Plr)
        {
            var list = new Dictionary<byte, byte>();
            lock(EffectList)
                list = EffectList.ToDictionary(e=>e.Key, e=>e.Value);

            foreach (var effect in list)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE, 6);
                Out.WriteUInt16(GetUnit().Oid);
                Out.WriteByte(1);
                Out.WriteByte(effect.Key);
                Out.WriteByte(effect.Value);   // active
                Out.WriteByte(0);
                Plr.SendPacket(Out);
            }
        }

        public void AddEffect(byte Effect, byte Sub = 1)
        {
            lock(EffectList)
                EffectList[Effect] = Sub;

            if (_Owner is Player)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE, 6);
                Out.WriteUInt16(GetUnit().Oid);
                Out.WriteByte(1);
                Out.WriteByte(Effect);
                Out.WriteByte(Sub);   // active
                Out.WriteByte(0);
                ((Player)_Owner).SendPacket(Out);
            }
            foreach (Player plr in _Owner.PlayersInRange)
            {
                    PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE, 6);
                    Out.WriteUInt16(GetUnit().Oid);
                    Out.WriteByte(1);
                    Out.WriteByte(Effect);
                    Out.WriteByte(Sub);   // active
                    Out.WriteByte(0);
                    plr.SendPacket(Out);
            }
        }

        public void RemoveEffect(byte Effect)
        {
            lock(EffectList)
                if(EffectList.ContainsKey(Effect))
                    EffectList.Remove(Effect);
            
            if (_Owner.IsPlayer())
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE, 6);
                Out.WriteUInt16(GetUnit().Oid);
                Out.WriteByte(1);
                Out.WriteByte(Effect);
                Out.WriteByte(0);   // active
                Out.WriteByte(0);
                _Owner.GetPlayer().SendPacket(Out);
            }

            foreach (Player plr in _Owner.PlayersInRange)
            {
                    PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE, 6);
                    Out.WriteUInt16(GetUnit().Oid);
                    Out.WriteByte(1);
                    Out.WriteByte(Effect);
                    Out.WriteByte(0);   // active
                    Out.WriteByte(0);
                    plr.SendPacket(Out);
            }
        }

        public bool HasEffect(byte Effect)
        {
            bool hasEffect = false;
            lock (EffectList)
                hasEffect = EffectList.ContainsKey(Effect);

            return hasEffect;
        }
    }
}

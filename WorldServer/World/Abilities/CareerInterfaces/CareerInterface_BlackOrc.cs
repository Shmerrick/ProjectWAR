using FrameWork;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    public class CareerInterface_BlackOrc : CareerInterface
    {

        public CareerInterface_BlackOrc(Player player) : base(player)
        {
            _maxResource = 2;
        }

        public override bool HasResource(byte amount)
        {
            return _careerResource == amount;
        }

        public override bool AddResource(byte amount, bool blockEvent)
        {
            _lastResourceTime = TCPManager.GetTimeStampMS();
            _lastResource = _careerResource;
            if (_careerResource == 2)
                _careerResource = 1;
            else ++_careerResource;

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);

            SendResource();

            return true;
        }

        public override void SendResource()
        {
            PacketOut Out;
            if (_lastResource != 0)
            {
                Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 12);
                Out.WriteByte(1);
                Out.WriteByte(2); // add
                Out.WriteUInt16(0x7C00); // unk3, God only knows
                Out.WriteUInt16(_Owner.Oid);
                Out.WriteByte(40); // buffID - some number I pulled out of the air
                Out.WriteByte(0);
                Out.WriteUInt16R(258); // Balance
                Out.WriteByte(00);

                myPlayer.SendPacket(Out);
            }
            if (_careerResource == 0)
                return; // zero resource means there's no buff left on the client

            Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 18);
            Out.WriteByte(1);
            Out.WriteByte(1); // add
            Out.WriteUInt16(0xbdff); // unk3, God only knows
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(40); // buffID - some number I pulled out of the air
            Out.WriteByte(0);
            Out.WriteUInt16R(258); // Balance
            Out.WriteByte(00); // Duration
            Out.WriteUInt16R(_Owner.Oid);

            Out.WriteByte(0x01); // Line count is always 1 for SM resource
            Out.WriteByte(00);
            Out.WriteZigZag(_careerResource);

            Out.WriteByte(00);
            myPlayer.SendPacket(Out);
        }

        public override EArchetype GetArchetype()
        {
            return EArchetype.ARCHETYPE_Tank;
        }
    }
}
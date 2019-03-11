using FrameWork;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    class CareerInterface_WHWE : CareerInterface
    {
        private ushort _resourceBuff;
        public CareerInterface_WHWE(Player player) : base(player)
        {
            _maxResource = 5;
            if (player.Info.CareerLine == 9)
                _resourceBuff = 270;
            else
                _resourceBuff = 274;

            _resourceTimeout = 20000;
        }

        public override void Notify_PlayerLoaded()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 48);
            Out.WriteByte(1);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WriteUInt16(myPlayer.Oid);
            Out.WriteByte(253);
            Out.WriteByte(0);
            Out.WriteUInt16R(myPlayer.Info.CareerLine == 9 ? (ushort)269 : (ushort)273);
            Out.WriteHexStringBytes("E2B88202");
            Out.WriteUInt16R(myPlayer.Oid);

            // Buff Lines
            Out.WriteByte(3);

            Out.WriteByte(0);
            Out.WriteZigZag(1);
            Out.WriteByte(2);
            Out.WriteZigZag(1);
            Out.WriteByte(4);
            Out.WriteZigZag(1);

            Out.WriteByte(0);

            myPlayer.SendPacket(Out);

            Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 48);
            Out.WriteByte(1);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WriteUInt16(myPlayer.Oid);
            Out.WriteByte(254);
            Out.WriteByte(0);
            Out.WriteUInt16R(myPlayer.Info.CareerLine == 9 ? (ushort)341 : (ushort)342);
            Out.WriteHexStringBytes("E2B88202");
            Out.WriteUInt16R(myPlayer.Oid);

            // Buff Lines
            Out.WriteByte(5);

            for (int i = 0; i < 5; ++i)
            {
                Out.WriteByte((byte)i);
                Out.WriteZigZag(-20 * (i + 1));
            }

            Out.WriteByte(0);

            myPlayer.SendPacket(Out);
        }

        public override bool ConsumeResource(byte amount, bool blockEvent)
        {
            if (_careerResource == 0)
                return false;

            amount = (byte)-_careerResource;

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceLost, _lastResource, ref amount);

            _lastResource = _careerResource;
            _careerResource += amount;
            SendResource();

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceSet, _careerResource, ref _careerResource);

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
                Out.WriteUInt16R(_resourceBuff); // Balance
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
            Out.WriteUInt16R(_resourceBuff); // Balance
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
            return EArchetype.ARCHETYPE_DPS;
        }

        public override byte GetCurrentResourceLevelForClass(byte which)
        {
            if (which > 0)
                return (byte)(_careerResource / ((which * 10f)));

            return _careerResource;
        }
    }
}

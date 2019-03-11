using FrameWork;
using GameData;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    public class CareerInterface_AMShaman: CareerInterface
    {
        private readonly ushort _damageBuffID; //, _tranqID;
        private const byte _healMax = 10, _damageMax = 5;
        private const byte _healMin = 6, _damageMin = 1;

        public CareerInterface_AMShaman(Player player) : base(player)
        {
            _maxResource = 5;
            _resourceTimeout = 15000;

            if (player.Info.CareerLine == 20)
            {
                _damageBuffID = 339;
                //_tranqID = 338;
                // high magic base ID = 333;
            }
            else
            {
                _damageBuffID = 3906;
                //_tranqID = 3905;
            }
        }

        public override void Notify_PlayerLoaded()
        {
            PacketOut Out = new PacketOut((byte) Opcodes.F_INIT_EFFECTS, 48);
            Out.WriteByte(1);
            Out.WriteByte(1);
            Out.WriteUInt16(0);
            Out.WriteUInt16(myPlayer.Oid);
            Out.WriteByte(254);
            Out.WriteByte(0);
            if (myPlayer.Info.CareerLine == 20)
                Out.WriteUInt16R(332);
            else Out.WriteUInt16R(267); // 263 for shammy
            Out.WriteHexStringBytes("E4EECC02");
            Out.WriteUInt16R(myPlayer.Oid);
            
            // Buff Lines
            Out.WriteByte(10);

            for (int i = 0; i < 5; ++i)
            { 
                Out.WriteByte((byte)i);
                if (ExperimentalMode)
                    Out.WriteZigZag(-40);
                else Out.WriteZigZag(-20 * (i + 1));
            }

            for (int i = 0; i < 5; ++i)
            {
                Out.WriteByte((byte)(i + 5));
                if (ExperimentalMode)
                    Out.WriteZigZag(-40);
                else Out.WriteZigZag(-20 * (i + 1));
            }

            Out.WriteByte(0);

            myPlayer.SendPacket(Out);
        }

        // FORCE
        public override bool AddResource(byte amount, bool blockEvent)
        {
            _lastResourceTime = TCPManager.GetTimeStampMS();

            if (_careerResource == _damageMax)
                return true; // AM / Shaman resource is non-blocking

            _lastResource = _careerResource;

            // Consume one
            
            
            if (_careerResource > _healMin)
                --_careerResource;
            else if (_careerResource == _healMin)
                _careerResource = _damageMin;
            else
                ++_careerResource;
            
            /*
            // Consume all
            
            if (_careerResource > _damageMax)
                _careerResource = _damageMin;
            else _careerResource++;
            
            */
            SendResource();
            return true;
        }

        // TRANQ
        public override bool ConsumeResource(byte amount, bool blockEvent)
        {
            _lastResourceTime = TCPManager.GetTimeStampMS();
            if (_careerResource == _healMax)
                return true; // AM / Shaman resource is non-blocking
            _lastResource = _careerResource;

            
            //consume one
            if (_careerResource <= _damageMin)
                _careerResource = _healMin;
            else if (_careerResource <= _damageMax)
                --_careerResource;
            else
                ++_careerResource;
            /*
            //consume all
            if (_careerResource <= _damageMax)
                _careerResource = _healMin;
            else _careerResource++;
            
            */
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
                Out.WriteByte(BUFF_REMOVE); // add
                Out.WriteUInt16(0x7C00); // unk3, God only knows
                Out.WriteUInt16(_Owner.Oid);
                Out.WriteByte(255); // buffID - some number I pulled out of the air
                Out.WriteByte(0);
                Out.WriteUInt16R(_damageBuffID); // Balance
                Out.WriteByte(00);

                myPlayer.SendPacket(Out);
            }
            if (_careerResource == 0)
                return; // zero resource means there's no buff left on the client

            Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 18);
            Out.WriteByte(1);
            Out.WriteByte(BUFF_ADD); // add
            Out.WriteUInt16(0); // unk3, God only knows
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(255); // buffID - some number I pulled out of the air
            Out.WriteByte(0);
            Out.WriteUInt16R(_damageBuffID); // Balance
            Out.WriteByte(00); // Duration
            Out.WriteUInt16R(_Owner.Oid);

            Out.WriteByte(0x01);
            Out.WriteByte(00);
            Out.WriteZigZag(_careerResource);

            Out.WriteByte(00);
            myPlayer.SendPacket(Out);
        }

        public override EArchetype GetArchetype()
        {
            // Check for Divine Fury
            if (myPlayer.BuffInterface.GetBuff(585, myPlayer) != null)
                return EArchetype.ARCHETYPE_DPS;

            // Check for Intelligence greater than Willpower
            if (myPlayer.StsInterface.GetTotalStat(Stats.Intelligence) > myPlayer.StsInterface.GetTotalStat(Stats.Willpower))
                return EArchetype.ARCHETYPE_DPS;

            // Check for Damage mastery greater than Heal mastery
            if (myPlayer.AbtInterface.GetMasteryLevelFor(1) < myPlayer.AbtInterface.GetMasteryLevelFor(2))
                return EArchetype.ARCHETYPE_DPS;

            return EArchetype.ARCHETYPE_Healer;
        }
    }
}
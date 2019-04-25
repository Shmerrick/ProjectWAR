using System;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    public class CareerInterface_BWSorc: CareerInterface
    {
        private ushort _resourceID, _backlashID;

        private static readonly ushort[] _critBonuses = { 5, 10, 15, 25, 35 };

        private static readonly ushort[] _critDmgBonuses = { 10, 20, 40, 80, 100 };

        private const ushort _backlashBase = 10, _backlashGain = 65;

        public CareerInterface_BWSorc(Player player) : base(player)
        {
            _maxResource = 100;
            if (player.Info.CareerLine == 11)
            {
                //_resourceID = 350;
                _resourceID = 298;
                _backlashID = 301;
            }
            else
            {
                _resourceID= 348;
                _backlashID = 291;
            }
        }

        public override void SetResource(byte amount, bool blockEvent)
        {
            if (amount != _careerResource)
            {
                _lastResource = _careerResource;
                _careerResource = amount;
                UpdateCombustion();
                SendResource();
            }
        }

        public override bool AddResource(byte amount, bool blockEvent)
        {
            _lastResourceTime = TCPManager.GetTimeStampMS();
            _lastResource = _careerResource;
            if (_careerResource == _maxResource)
            {
                UpdateCombustion();
                return false;
            }
            _careerResource = (byte)Math.Min(_maxResource, _careerResource + amount);
            UpdateCombustion();
            SendResource();
            return true;
        }

        public override bool ConsumeResource(byte amount, bool blockEvent)
        {
            _lastResource = _careerResource;
            if (_careerResource < amount)
            {
                _careerResource = 0;
                UpdateCombustion();
                SendResource();
                return false;
            }
            _lastResource = _careerResource;
            _careerResource = (byte)Math.Max(0, _careerResource - amount);
            UpdateCombustion();
            SendResource();
            return true;
        }

        private long _lastTick;
        private int _decayTimer;

        public override void Update(long tick)
        {
            if (_careerResource > 0 && _lastResourceTime != 0 && tick - _lastResourceTime > _resourceTimeout)
            {
                _decayTimer += (int)(tick - _lastTick);

                if (_decayTimer >= 2000)
                {
                    ConsumeResource(20, false);
                    _decayTimer -= 2000;
                }
            }

            else
                _decayTimer = 0;

            _lastTick = tick;
        }

        public bool AutoBacklash { get; set; }

        private void UpdateCombustion()
        {
            byte curResLevel = GetLocalResourceLevel(_careerResource);
            byte lastResLevel = GetLocalResourceLevel(_lastResource);

            if (AutoBacklash || (lastResLevel != 0 && _careerResource >= _lastResource && StaticRandom.Instance.Next(0, 100) <= _critBonuses[lastResLevel - 1]))
            {
                PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 18);

                // This is abrasive, but a tactic needs to process the Combustion self-damage event
                // Should probably just create this and reuse it
                AbilityDamageInfo backDamageInfo = new AbilityDamageInfo
                {
                    NoCrits = true,
                    Entry = _backlashID,
                    Damage = (int) ((_backlashBase + (_backlashGain*(myPlayer.Level - 1)/39))*0.1f*_critDmgBonuses[lastResLevel > 0 ? lastResLevel - 1 : 0])
                };


                damageOut.WriteUInt16(myPlayer.Oid);
                damageOut.WriteUInt16(myPlayer.Oid);
                damageOut.WriteUInt16(_backlashID);

                damageOut.WriteByte(2);
                damageOut.WriteByte(0); // DAMAGE EVENT
                damageOut.WriteByte(0x7);

                damageOut.WriteZigZag((int)-backDamageInfo.Damage);
                damageOut.WriteByte(0);
                myPlayer.DispatchPacketUnreliable(damageOut, true, null);

                myPlayer.ReceiveDamage(myPlayer, backDamageInfo);

                myPlayer.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.Manual, backDamageInfo, myPlayer);

                AutoBacklash = false;
            }

            if (curResLevel != lastResLevel)
            {
                if (lastResLevel != 0)
                {
                    myPlayer.StsInterface.RemoveBonusStat(Stats.CriticalHitRate, _critBonuses[lastResLevel - 1], BuffClass.Career);
                    myPlayer.StsInterface.RemoveBonusStat(Stats.CriticalDamage, _critDmgBonuses[lastResLevel - 1], BuffClass.Career);
                }

                if (curResLevel != 0)
                {
                    myPlayer.StsInterface.AddBonusStat(Stats.CriticalHitRate, _critBonuses[curResLevel - 1], BuffClass.Career);
                    myPlayer.StsInterface.AddBonusStat(Stats.CriticalDamage, _critDmgBonuses[curResLevel - 1], BuffClass.Career);
                }
            }
        }

        public byte GetLocalResourceLevel(byte res)
        {
            if (res == 0)
                return 0;
            if (res > 90)
                return 5;
            if (res > 70)
                return 4;
            if (res > 30)
                return 3;
            if (res > 10)
                return 2;
            return 1;
        }

        public override byte GetCurrentResourceLevel(byte which)
        {
            if (_careerResource == 0)
                return 0;
            if (_careerResource > 90)
                return 5;
            if (_careerResource > 70)
                return 4;
            if (_careerResource > 30)
                return 3;
            if (_careerResource > 10)
                return 2;
            return 1;
        }

        public override void SendResource()
        {
            PacketOut Out;
            if (_lastResource != 0)
            {
                Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 12);
                Out.WriteByte(1);
                Out.WriteByte(BUFF_REMOVE); // add
                Out.WriteUInt16(0x7C00);
                Out.WriteUInt16(_Owner.Oid);
                Out.WriteByte(255); // buffID - some number I pulled out of the air
                Out.WriteByte(0);
                Out.WriteUInt16R(_resourceID); // Balance
                Out.WriteByte(00);

                myPlayer.SendPacket(Out);
            }
            if (_careerResource == 0)
                return; // zero resource means there's no buff left on the client

            Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 18);
            Out.WriteByte(1);
            Out.WriteByte(BUFF_ADD); // add
            Out.WriteUInt16(0);
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(255); // buffID - some number I pulled out of the air
            Out.WriteByte(0);
            Out.WriteUInt16R(_resourceID); // Balance
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
            return EArchetype.ARCHETYPE_DPS;
        }
    }
}
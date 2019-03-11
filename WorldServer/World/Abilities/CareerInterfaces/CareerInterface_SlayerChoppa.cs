using System;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.CareerInterfaces
{
    public class CareerInterface_SlayerChoppa : CareerInterface
    {
        const byte RAGE_FURIOUS = 25;
        const byte RAGE_BERSERK = 75;

        const byte EFFECT_FURIOUS = 3;
        const byte EFFECT_BERSERK = 4;

        const float DMGBONUS_FURIOUS = 0.25f;
        const float DMGBONUS_BERSERK = 0.5f;

        private byte _rageState;

        private ushort _counterBuff, _selfBuff;

        private long _nextRegenTime;

        private bool _wasFighting;

        public CareerInterface_SlayerChoppa(Player player) : base(player)
        {
            if (player.Info.CareerLine == 6)
            {
                _selfBuff = 1051;
                _counterBuff = 1052;
            }

            else
            {
                _selfBuff = 1062;
                _counterBuff = 1063;
            }
        }

        public override bool HasResource(byte amount)
        {
            return _careerResource >= amount;
        }

        public override byte GetCurrentResourceLevel(byte which)
        {
            if (_careerResource < RAGE_FURIOUS)
                return 0;
            if (_careerResource < RAGE_BERSERK)
                return 1;
            return 2;
        }

        public override byte GetLevelForResource(byte res, byte which)
        {
            if (res < RAGE_FURIOUS)
                return 0;
            if (res < RAGE_BERSERK)
                return 1;
            return 2;
        }

        public override void SetResource(byte amount, bool blockEvent)
        {
            _lastResource = _careerResource;
            _careerResource = amount;

            SendResource();

            if (GetCurrentResourceLevel(0) != _rageState)
            {
                SwitchRageState(GetCurrentResourceLevel(0));
                myPlayer.BuffInterface.NotifyResourceEvent((byte) BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);
            }
        }

        public override bool AddResource(byte amount, bool blockEvent)
        {
            _lastResource = _careerResource;
            _careerResource = Math.Min((byte)100, (byte)(_careerResource + amount));

            SendResource();

            if (GetCurrentResourceLevel(0) != _rageState)
            {
                SwitchRageState(GetCurrentResourceLevel(0));
                myPlayer.BuffInterface.NotifyResourceEvent((byte) BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);
            }

            return true;
        }

        public override bool ConsumeResource(byte amount, bool blockEvent)
        {
            _lastResource = _careerResource;
            amount = Math.Min(amount, _careerResource);

            if (!blockEvent)
                myPlayer.BuffInterface.NotifyResourceEvent((byte)BuffCombatEvents.ResourceLost, _lastResource, ref amount);

            _careerResource -= amount;

            SendResource();

            if (GetCurrentResourceLevel(0) != _rageState)
            {
                SwitchRageState(GetCurrentResourceLevel(0));
                myPlayer.BuffInterface.NotifyResourceEvent((byte) BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);
            }

            return true;
        }

        public override void Update(long tick)
        {
            if (myPlayer == null)
                return;

            if (myPlayer.Panicked)
            {
                if (_nextRegenTime <= tick)
                {
                    ConsumeResource(10, true);
                    _nextRegenTime = tick + 1000;
                }

                return;
            }

            bool isFighting = myPlayer.CbtInterface.IsInCombat && !myPlayer.IsDead;

            if (_wasFighting == isFighting)
            {
                if (isFighting)
                {
                    if (_nextRegenTime <= tick)
                    {
                        AddResource(5, true);
                        _nextRegenTime = tick + 1000;
                    }
                }

                else
                {
                    if (_nextRegenTime <= tick)
                    {
                        ConsumeResource(5, true);
                        _nextRegenTime = tick + 1000;
                    }
                }
            }

            else
            {
                _wasFighting = isFighting;

                if (isFighting)
                    _nextRegenTime = tick + 1000;
                else
                    _nextRegenTime = tick + 5000;
            }
        }

        private void SwitchRageState(byte newState)
        {
            // Removal
            switch (_rageState)
            {
                case 0:
                    break;
                case 1:
                    myPlayer.StsInterface.RemoveBonusMultiplier(Stats.OutgoingDamagePercent, DMGBONUS_FURIOUS, BuffClass.Career);
                    if (newState != 2)
                        SendBerserkOwnerEffect(EFFECT_FURIOUS, 0);
                    break;
                case 2:
                    myPlayer.StsInterface.RemoveBonusMultiplier(Stats.OutgoingDamagePercent, DMGBONUS_BERSERK, BuffClass.Career);
                    myPlayer.StsInterface.RemoveReducedMultiplier(Stats.Armor, 0.5f, BuffClass.Career);
                    myPlayer.StsInterface.RemoveReducedMultiplier(Stats.SpiritResistance, 0.5f, BuffClass.Career);
                    myPlayer.StsInterface.RemoveReducedMultiplier(Stats.CorporealResistance, 0.5f, BuffClass.Career);
                    myPlayer.StsInterface.RemoveReducedMultiplier(Stats.ElementalResistance, 0.5f, BuffClass.Career);
                    if (newState == 0)
                        SendBerserkOwnerEffect(EFFECT_FURIOUS, 0);
                    SendBerserkOwnerEffect(EFFECT_BERSERK, 0);
                    myPlayer.OSInterface.RemoveEffect(1);
                    break;
            }
            // Addition
            switch (newState)
            {
                case 0:
                    break;
                case 1:
                    myPlayer.StsInterface.AddBonusMultiplier(Stats.OutgoingDamagePercent, DMGBONUS_FURIOUS, BuffClass.Career);
                    SendBerserkOwner(newState);
                    if (_rageState == 0)
                        SendBerserkOwnerEffect(EFFECT_FURIOUS, 1);
                    break;
                case 2:
                    myPlayer.StsInterface.AddBonusMultiplier(Stats.OutgoingDamagePercent, DMGBONUS_BERSERK, BuffClass.Career);

                    myPlayer.StsInterface.AddReducedMultiplier(Stats.Armor, 0.5f, BuffClass.Career);
                    myPlayer.StsInterface.AddReducedMultiplier(Stats.SpiritResistance, 0.5f, BuffClass.Career);
                    myPlayer.StsInterface.AddReducedMultiplier(Stats.CorporealResistance, 0.5f, BuffClass.Career);
                    myPlayer.StsInterface.AddReducedMultiplier(Stats.ElementalResistance, 0.5f, BuffClass.Career);

                    SendBerserkOwner(newState);
                    SendBerserkOwnerEffect(EFFECT_BERSERK, 1);
                    myPlayer.OSInterface.AddEffect(1);
                    break;
            }

            _rageState = newState;
        }

        private void SendBerserkOwner(byte newState)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 20);
            Out.WriteByte(1);
            Out.WriteByte(BUFF_ADD);
            Out.WriteUInt16(0xBDFF); // unk3
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(254); // Buff ID
            Out.WriteByte(0);
            Out.WriteUInt16R(_selfBuff); // Enrage/Berserk Owner
            Out.WriteByte(0); // Duration
            Out.WriteUInt16R(_Owner.Oid);

            if (newState == 1)
            {
                Out.WriteByte(2); // Line count

                Out.WriteByte(0);
                Out.WriteZigZag(1);

                Out.WriteByte(3);
                Out.WriteZigZag(1);
            }

            else
            {
                Out.WriteByte(3); // Line count

                Out.WriteByte(0);
                Out.WriteZigZag(1);

                Out.WriteByte(3);
                Out.WriteZigZag(1);

                Out.WriteByte(4);
                Out.WriteZigZag(1);
            }

            Out.WriteByte(00);
            myPlayer.SendPacket(Out);
        }

        private void SendBerserkOwnerEffect(byte newState, byte addRemove)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
            Out.WriteUInt16(myPlayer.Oid);
            Out.WriteUInt16(myPlayer.Oid);
            Out.WriteUInt16(_selfBuff);  // Enrage/Berserk Owner
            Out.WriteByte(newState);
            Out.WriteByte(0);
            Out.WriteByte(addRemove);
            Out.WriteByte(0);

            myPlayer.DispatchPacket(Out, true);
        }

        public override byte GetResourceLevelMax(byte which)
        {
            return 2;
        }

        public override void SendResource()
        {
            PacketOut Out;
            if (_lastResource != 0)
            {
                Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 12);
                Out.WriteByte(1);
                Out.WriteByte(BUFF_REMOVE);
                Out.WriteUInt16(0x7C00); // unk3, God only knows
                Out.WriteUInt16(_Owner.Oid);
                Out.WriteByte(255); // buffID - some number I pulled out of the air
                Out.WriteByte(0);
                Out.WriteUInt16R(_counterBuff);  // Enrage/Berserk Counters
                Out.WriteByte(00);

                myPlayer.SendPacket(Out);
            }
            if (_careerResource == 0)
                return; // zero resource means there's no buff left on the client

            Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 18);
            Out.WriteByte(1);
            Out.WriteByte(BUFF_ADD);
            Out.WriteUInt16(0xBDFF); // unk3
            Out.WriteUInt16(_Owner.Oid);
            Out.WriteByte(255); // buffID - some number I pulled out of the air
            Out.WriteByte(0);
            Out.WriteUInt16R(_counterBuff);  // Enrage/Berserk Counters
            Out.WriteByte(0); // Duration
            Out.WriteUInt16R(_Owner.Oid);

            Out.WriteByte(0x01); // Line count is always 1
            Out.WriteByte(00);
            Out.WriteZigZag(_careerResource);

            Out.WriteByte(00);
            myPlayer.SendPacket(Out);
        }

        public override void NotifyPanicked()
        {
        }

        public override EArchetype GetArchetype()
        {
            return EArchetype.ARCHETYPE_DPS;
        }
    }
}
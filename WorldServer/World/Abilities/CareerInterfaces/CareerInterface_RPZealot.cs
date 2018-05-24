using SystemData;
using FrameWork;
using GameData;

namespace WorldServer
{
    class CareerInterface_RPZealot : CareerInterface
    {
        private readonly ushort _fakeBuffEntry;
        private readonly ushort _vfxid;
        private bool _initialized;
        private NewBuff _vfxBuff;

        public CareerInterface_RPZealot(Player player) : base(player)
        {
            if (player.Info.CareerLine == 3)
            {
                _fakeBuffEntry = 1659;
                _vfxid = 3081;
            }
            else
                _fakeBuffEntry = 8550;

            _resourceTimeout = 0;
        }

        public override bool HasResource(byte amount)
        {
            return true;
        }

        public override void Notify_PlayerLoaded()
        {
            if (_careerResource == 0)
                BalanceMode(false);
            else
                BreakMode();
        }

        public override void NotifyClientLoaded()
        {
            if (CareerResource == 1 && _fakeBuffEntry == 8550)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(_fakeBuffEntry); // 00 00 07 D D

                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(1);

                Out.WriteByte(0);
                myPlayer.SendPacket(Out);
            }
        }

        public override void SetResource(byte amount, bool blockEvent)
        {
            if (amount == 0)
                return;
            if (_careerResource == 0)
            {
                BreakMode();
                _careerResource++;
            }
            else
            {
                BalanceMode(true);
                _careerResource--;
            }
        }

        private void BreakMode()
        {
            myPlayer.StsInterface.RemoveItemStatGift(Stats.Willpower);
            myPlayer.StsInterface.RemoveItemStatGift(Stats.HealCritRate);
            myPlayer.StsInterface.RemoveItemStatGift(Stats.HealingPower);

            myPlayer.StsInterface.DisableItemBonus(Stats.Intelligence, false);
            myPlayer.StsInterface.DisableItemBonus(Stats.MagicCritRate, false);
            myPlayer.StsInterface.DisableItemBonus(Stats.MagicPower, false);

            /*
            PacketOut OutRm = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 12);
            OutRm.WriteByte(1);
            OutRm.WriteByte(BUFF_REMOVE);
            OutRm.WriteUInt16(0);
            OutRm.WriteUInt16(myPlayer.Oid);
            OutRm.WriteByte(255); // buffID - some number I pulled out of the air
            OutRm.WriteByte(0);
            OutRm.WriteUInt16R(_fakeBuffEntry);
            OutRm.WriteByte(0);

            myPlayer.SendPacket(OutRm);
            */

            int IntBonus = myPlayer.StsInterface.GiftItemStatTo(Stats.Intelligence, Stats.Willpower);
            int MagCritBonus = myPlayer.StsInterface.GiftItemStatTo(Stats.MagicCritRate, Stats.HealCritRate);
            int MagPwrBonus = myPlayer.StsInterface.GiftItemStatTo(Stats.MagicPower, Stats.HealingPower);

            myPlayer.StsInterface.DisableItemBonus(Stats.Willpower, true);
            myPlayer.StsInterface.DisableItemBonus(Stats.HealCritRate, true);
            myPlayer.StsInterface.DisableItemBonus(Stats.HealingPower, true);

            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 24);
            Out.WriteByte(1);
            Out.WriteByte(BUFF_ADD);
            Out.WriteUInt16(0xF4FF);
            Out.WriteUInt16(myPlayer.Oid);
            Out.WriteByte(255);
            Out.WriteByte(0);
            Out.WriteUInt16R(_fakeBuffEntry);
            Out.WriteZigZag(0);
            Out.WriteUInt16R(myPlayer.Oid);
            Out.WriteByte(6);

            Out.WriteByte(0);
            Out.WriteZigZag(MagCritBonus);      // magic crit

            Out.WriteByte(1);
            Out.WriteZigZag(IntBonus);    // intelligence component

            Out.WriteByte(2);
            Out.WriteZigZag(MagPwrBonus);    // magic pwr

            Out.WriteByte(3);
            Out.WriteZigZag(-MagCritBonus);      // heal crit

            Out.WriteByte(4);
            Out.WriteZigZag(-IntBonus);   // willpower component

            Out.WriteByte(5);
            Out.WriteZigZag(-MagPwrBonus);    // heal pwr
            Out.WriteByte(0);

            myPlayer.SendPacket(Out);

            if (_vfxid != 0)
            {
                if (_vfxBuff == null || _vfxBuff.BuffHasExpired)
                    myPlayer.BuffInterface.QueueBuff(new BuffQueueInfo(myPlayer, myPlayer.EffectiveLevel, AbilityMgr.GetBuffInfo(_vfxid), LinkVfxBuff));
            }
            else
            {
                Out = new PacketOut((byte) Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(_fakeBuffEntry); // 00 00 07 D D

                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(1);

                Out.WriteByte(0);
                myPlayer.DispatchPacket(Out, true);
            }
        }

        private void BalanceMode(bool clear)
        {
            if (_initialized && clear)
            {
                myPlayer.StsInterface.RemoveItemStatGift(Stats.Intelligence);
                myPlayer.StsInterface.RemoveItemStatGift(Stats.MagicCritRate);
                myPlayer.StsInterface.RemoveItemStatGift(Stats.MagicPower);

                myPlayer.StsInterface.DisableItemBonus(Stats.Willpower, false);
                myPlayer.StsInterface.DisableItemBonus(Stats.HealCritRate, false);
                myPlayer.StsInterface.DisableItemBonus(Stats.HealingPower, false);

                PacketOut OutRm = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 12);
                OutRm.WriteByte(1);
                OutRm.WriteByte(BUFF_REMOVE);
                OutRm.WriteUInt16(0);
                OutRm.WriteUInt16(myPlayer.Oid);
                OutRm.WriteByte(255); // buffID - some number I pulled out of the air
                OutRm.WriteByte(0);
                OutRm.WriteUInt16R(_fakeBuffEntry);
                OutRm.WriteByte(0);

                myPlayer.SendPacket(OutRm);
            }

            _initialized = true;

            int WillBonus = myPlayer.StsInterface.GiftItemStatTo(Stats.Willpower, Stats.Intelligence);
            int HealCritBonus = myPlayer.StsInterface.GiftItemStatTo(Stats.HealCritRate, Stats.MagicCritRate);
            int HealPwrBonus = myPlayer.StsInterface.GiftItemStatTo(Stats.HealingPower, Stats.MagicPower);

            myPlayer.StsInterface.DisableItemBonus(Stats.Intelligence, true);
            myPlayer.StsInterface.DisableItemBonus(Stats.MagicCritRate, true);
            myPlayer.StsInterface.DisableItemBonus(Stats.MagicPower, true);

            /*
            PacketOut Out = new PacketOut((byte)Opcodes.F_INIT_EFFECTS, 24);
            Out.WriteByte(1);
            Out.WriteByte(BUFF_ADD);
            Out.WriteUInt16(0xF4FF);
            Out.WriteUInt16(myPlayer.Oid);
            Out.WriteByte(255);
            Out.WriteByte(0);
            Out.WriteUInt16R(_fakeBuffEntry);
            Out.WriteZigZag(0);
            Out.WriteUInt16R(myPlayer.Oid);
            Out.WriteByte(6);

            Out.WriteByte(0);
            Out.WriteZigZag(-HealCritBonus);      // magic crit

            Out.WriteByte(1);
            Out.WriteZigZag(-WillBonus);    // intelligence component

            Out.WriteByte(02);
            Out.WriteZigZag(-HealPwrBonus);    // magic pwr

            Out.WriteByte(3);
            Out.WriteZigZag(HealCritBonus);      // heal crit

            Out.WriteByte(4);
            Out.WriteZigZag(WillBonus);   // willpower component

            Out.WriteByte(5);
            Out.WriteZigZag(HealPwrBonus);    // heal pwr

            Out.WriteByte(0);

            myPlayer.SendPacket(Out);

            */

            if (_vfxBuff != null)
                _vfxBuff.BuffHasExpired = true;
            else if (_vfxid == 0)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(myPlayer.Oid);
                Out.WriteUInt16(_fakeBuffEntry); // 00 00 07 D D

                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);

                Out.WriteByte(0);
                myPlayer.SendPacket(Out);
            }
        }

        public void LinkVfxBuff(NewBuff B)
        {
            _vfxBuff = B;
        }

        public override void SendResource()
        {

        }

        public override EArchetype GetArchetype()
        {
            // Check for Divine Fury
            if (myPlayer.BuffInterface.GetBuff(585, myPlayer) != null)
                return EArchetype.ARCHETYPE_DPS;

            return EArchetype.ARCHETYPE_Healer;
        }

        public override void NotifyInitialized()
        {
            myPlayer.SendClientMessage("This class has modifications. Enter the command \".ab changelist\" to see the changelist.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
        }

        public override void DisplayChangeList()
        {
            if (myPlayer.Info.CareerLine == (int)CareerLine.CAREERLINE_RUNE_PRIEST)
            {
                myPlayer.SendClientMessage("Global changes to Runepriest:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Rune of Mending can be cast while moving.");
                myPlayer.SendClientMessage("Path of Valaya changes:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Rune of Fate cooldown reduced to 20s.");
                myPlayer.SendClientMessage("Path of Grimnir changes:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Rune of battle cooldown cut to 30s ability is unaffected by cooldown reduction abilities.");
                myPlayer.SendClientMessage("Path of Grungni changes:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Rune of Nullification duration increased to 10s.");
            }

            else
            {
                myPlayer.SendClientMessage("Global changes to Zealot:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Dark Medicine can be cast while moving.");
                myPlayer.SendClientMessage("Path of Witchcraft changes:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Storm of Ravens now carries a snare component.");
                myPlayer.SendClientMessage("+ Mirror of Madness now causes damage to the recipient on DIRECT HEAL RECEIVED instead of heal cast, damage occurs once per second max.");
                myPlayer.SendClientMessage("+ Mirror of Madness can be cast on the move.");
            }
        }
    }
}

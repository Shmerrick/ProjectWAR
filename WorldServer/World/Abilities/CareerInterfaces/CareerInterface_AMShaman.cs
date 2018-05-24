using SystemData;
using FrameWork;
using GameData;

namespace WorldServer
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
        /*
        public override bool SetExperimentalMode(bool fullExplanation)
        {
            ExperimentalMode = !ExperimentalMode;

            Notify_PlayerLoaded();

            if (!fullExplanation && ExperimentalMode)
            {
                myPlayer.SendClientMessage("Experimental mode for Archmage and Shaman is currently enabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return true;
            }

            if (ExperimentalMode)
            {
                myPlayer.SendClientMessage("Experimental mode for Archmage and Shaman has been enabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("General changes:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Only one mechanic point is consumed, rather than all points.");
                myPlayer.SendClientMessage("+ I'll Take That! and Balance Essence will additionally heal for 250% of their base damage and scale with Willpower.");
                myPlayer.SendClientMessage("+ Fury of Da Green and Energy of Vaul will additionally heal for their base damage multiplied by the number of foes hit.");
                myPlayer.SendClientMessage("+ The heal component gains the Healing type instead of the Raw Healing type. It will be heal debuffed/blessed, can crit as a heal would and is able to proc effects.");
                myPlayer.SendClientMessage("+ These abilities gain 20% block and disrupt strikethrough.");
                myPlayer.SendClientMessage("- The damage has no stat contribution and cannot critically hit.");
                myPlayer.SendClientMessage("The bonuses received by abilities from the career mechanic are now the following:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ The effects of an ability on other players will use a bonus stat contribution which is derived from a factor of your total bonus stats from items, if this is higher than the ability's normal scaling stat. ");
                myPlayer.SendClientMessage("~ Reduced stat contribution works as normal.");
                myPlayer.SendClientMessage("+ The effects of an ability on other players will use your Career Rank instead of your Mastery Level.");
                myPlayer.SendClientMessage("Mechanic modifications affecting instant cast abilities and channeled abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Effectiveness on other players is increased by 25%.");
                myPlayer.SendClientMessage("Mechanic modifications affecting cleanse abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Cleansing abilities will build mechanic points as a healing ability would.");
                myPlayer.SendClientMessage("+ Cleansing abilities consuming mechanic points will have a 3 second cooldown when used on other players.");
                myPlayer.SendClientMessage("Mechanic modifications affecting all build up cast abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE); ;
                myPlayer.SendClientMessage("+ The cast time is reduced by 40%.");
                myPlayer.SendClientMessage("Mechanic modifications affecting non-lifetap build up cast abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ The AP cost is reduced by 40%.");
                myPlayer.SendClientMessage("Mechanic modifications affecting lifetap build up cast abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ These abilities can be cast while moving.");
                myPlayer.SendClientMessage("Mechanic modifications affecting tactics:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ The tactics Expanded Control and Waaagh! Frenzy! will now cause Balance Essence and I'll Take That! to heal two additional allies within 30ft of the target for 75% of the single target heal value.");
            }

            else
                myPlayer.SendClientMessage("Experimental mode has been disabled. The career mechanic will now function as it did in Age of Reckoning.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

            return true;
        }*/

        public override void DisplayChangeList()
        {
            if (myPlayer.Info.CareerLine == (int)CareerLine.CAREERLINE_ARCHMAGE)
            {
                myPlayer.SendClientMessage("Global changes to Archmage:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Only one mechanic point is consumed, rather than all points, please note that the castbars are not correct.");
                myPlayer.SendClientMessage("+ Energy of Vaul heals the group.");
                myPlayer.SendClientMessage("+ Healing Energy casts on the move.");
                myPlayer.SendClientMessage("+ Drain Magic steals 20 AP per second, rather than 60 AP every 3 seconds.");
                myPlayer.SendClientMessage("+ Magical Infusion increases the value of heals received by the target by 25%.");
                myPlayer.SendClientMessage("Mechanic modifications affecting all build up cast abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE); ;
                myPlayer.SendClientMessage("+ The cast time is reduced by 40%.");
                myPlayer.SendClientMessage("Mechanic modifications affecting lifetap build up cast abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ These abilities can be cast while moving.");
            }

            else
            {
                myPlayer.SendClientMessage("Global changes to Shaman:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Only one mechanic point is consumed, rather than all points, please note that the castbars are not correct.");
                myPlayer.SendClientMessage("+ Fury of Da Green heals the group.");
                myPlayer.SendClientMessage("+ Gork'll Fix It casts on the move.");
                myPlayer.SendClientMessage("+ Yer Not So Bad steals 20 AP per second, rather than 60 AP every 3 seconds.");
                myPlayer.SendClientMessage("+ Shrug It Off increases the value of heals received by the target by 25%.");
                myPlayer.SendClientMessage("+ Gained ability Get'n Smarter at rank 30.");
                myPlayer.SendClientMessage("- Lost ability Yer A Weaklin'.");
                myPlayer.SendClientMessage("Mechanic modifications affecting all build up cast abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE); ;
                myPlayer.SendClientMessage("+ The cast time is reduced by 40%.");
                myPlayer.SendClientMessage("Mechanic modifications affecting lifetap build up cast abilities:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ These abilities can be cast while moving.");
            }
        }

        public override void NotifyInitialized()
        {
            myPlayer.SendClientMessage("This class has modifications. Enter the command \".ab changelist\" to see the changelist.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            /*
            if (myPlayer._Value.ExperimentalMode)
                SetExperimentalMode(false);
            else
                myPlayer.SendClientMessage("This class has an experimental mode available. Enter the command \".ab ex\" to activate it and see the changes.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                */
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
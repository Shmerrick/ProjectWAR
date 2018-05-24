using SystemData;
using FrameWork;
using GameData;

namespace WorldServer
{
    public class CareerInterface_WPDoK : CareerInterface
    {
        private ushort _resourceID;
        private int _updateTimer;
        private long _lastUpdateTime;
        private float _conversionMod;

        public CareerInterface_WPDoK(Player player) : base(player)
        {
            _maxResource = 250;
            _careerResource = _maxResource;
            if (player.Info.CareerLine == (byte) CareerLine.CAREERLINE_WARRIOR_PRIEST)
            {
                _resourceID = 308;
                _conversionMod = 0.16f;
            }
            else
            {
                _resourceID = 314;
                _conversionMod = 0.1f;
            }

            _resourceTimeout = 0;
        }

        public override void Notify_PlayerLoaded()
        {
            SendResource();
        }

        public override void Update(long tick)
        {
            if (_lastUpdateTime > 0)
            {
                _updateTimer += (int)(tick - _lastUpdateTime);

                if (_updateTimer >= 1000)
                {
                    int newResourceVal = 0;
                    while (_updateTimer >= 1000)
                    {
                        if (CurrentStance == 1 && DrainActive(tick))
                        {
                            if (!myPlayer.CbtInterface.IsInCombat)
                                newResourceVal += 15;
                            else
                                newResourceVal -= 5;
                        }
                        else if (!myPlayer.CbtInterface.IsInCombat)
                            newResourceVal += 20;
                        _updateTimer -= 1000;
                    }

                    if (!myPlayer.IsDead)
                    {
                        if (_careerResource < 250 && newResourceVal > 0)
                        {
                            AddResource((byte) newResourceVal, true);
                            myPlayer.BuffInterface.NotifyResourceEvent((byte) BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);
                        }

                        else if (_careerResource > 0 && newResourceVal < 0)
                        {
                            ConsumeResource((byte) -newResourceVal, true);
                            myPlayer.BuffInterface.NotifyResourceEvent((byte) BuffCombatEvents.ResourceSet, _lastResource, ref _careerResource);
                        }
                    }
                }
            }

            _lastUpdateTime = tick;
        }

        public override byte GetCurrentResourceLevel(byte type)
        {
            return (byte)(_careerResource * _conversionMod);
        }

        public override byte GetLevelForResource(byte res, byte which)
        {
            return (byte)(res * _conversionMod);
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
                Out.WriteUInt16R(_resourceID); // Balance
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
            // Check for Damage mastery
            if (myPlayer.Info.CareerLine == 12)
            {
                int dpsMastery = myPlayer.AbtInterface.GetMasteryLevelFor(3);

                if (dpsMastery > myPlayer.AbtInterface.GetMasteryLevelFor(2) && dpsMastery > myPlayer.AbtInterface.GetMasteryLevelFor(1))
                    return EArchetype.ARCHETYPE_DPS;
            }

            else
            {
                int dpsMastery = myPlayer.AbtInterface.GetMasteryLevelFor(2);

                if (dpsMastery > myPlayer.AbtInterface.GetMasteryLevelFor(3) && dpsMastery > myPlayer.AbtInterface.GetMasteryLevelFor(1))
                    return EArchetype.ARCHETYPE_DPS;
            }

            return EArchetype.ARCHETYPE_Healer;
        }

        public override void DisplayChangeList()
        {
            if (myPlayer.Info.CareerLine == (int) CareerLine.CAREERLINE_WARRIOR_PRIEST)
            {
                myPlayer.SendClientMessage("Global changes to Warrior Priest:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Hammer of Sigmar inflicts 20-150 base damage and 53-399 improved damage, and will always critically hit when improved.");
                myPlayer.SendClientMessage("+ Sigmar's Shield drains 15 RF per second instead of 20 RF every time it is triggered and lasts 7 seconds.");
                myPlayer.SendClientMessage("- Cleansing Power will cause you to cleanse either yourself or your groupmates, instead of your entire group.");
            }

            else
            {
                myPlayer.SendClientMessage("Global changes to Disciple of Khaine:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                myPlayer.SendClientMessage("+ Wracking Agony inflicts 20-150 base damage and 53-399 improved damage, and will always critically hit when improved.");
                myPlayer.SendClientMessage("- Efficient Patching will cause you to cleanse either yourself or your groupmates, instead of your entire group.");
                myPlayer.SendClientMessage("- Khaine's Withdrawal will no longer cleanse Curses.");
            }
        }

        #region Experimental Mode - Prayers/Covenant effects
        /*
        public override bool SetExperimentalMode(bool fullExplanation)
        {
            myPlayer.SendClientMessage("Experimental mode for this class is no longer available.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            return false;

            if (ExperimentalMode)
            {
                PrintExModeChangeset();
                myPlayer.SendClientMessage("Experimental mode for this class has been forced for a short period, and cannot be disabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return false;
            }

            CurrentStance = 0;

            if (fullExplanation)
            {
                if (myPlayer.Info.CareerLine == (int) GameData.CareerLine.CAREERLINE_WARRIOR_PRIEST)
                {
                    myPlayer.BuffInterface.RemoveBuffByEntry(8242);
                    myPlayer.BuffInterface.RemoveBuffByEntry(8243);
                    myPlayer.BuffInterface.RemoveBuffByEntry(8249);
                }
                else
                {
                    myPlayer.BuffInterface.RemoveBuffByEntry(9559);
                    myPlayer.BuffInterface.RemoveBuffByEntry(9563);
                    myPlayer.BuffInterface.RemoveBuffByEntry(9567);
                }
            }

            ExperimentalMode = !ExperimentalMode;

            if (!fullExplanation && ExperimentalMode)
            {
                myPlayer.SendClientMessage("Experimental mode for Warrior Priest and Disciple of Khaine is currently enabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return true;
            }

            if (ExperimentalMode)
            {
                PrintExModeChangeset();
            }

            else
                myPlayer.SendClientMessage("Experimental mode for Warrior Priest and Disciple of Khaine has been disabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);

            return true;
        }

        private void PrintExModeChangeset()
        {
            myPlayer.SendClientMessage("Experimental mode for Warrior Priest and Disciple of Khaine has been enabled.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("General changes:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("- Righteous Fury and Soul Essence regeneration from tomes and chalices is linked to the Morale bonus scaler.");
            myPlayer.SendClientMessage("- Switching between Prayers and Covenants invokes a 15 second cooldown for all Prayers and Covenants.");
            myPlayer.SendClientMessage("- When switching into or out of the Prayer of Righteousness or the Covenant of Celerity, a 5 minute cooldown is used instead.");
            myPlayer.SendClientMessage("- If in combat, switching to or from Righteousness or Celerity consumes all action points and mechanic points.");
            myPlayer.SendClientMessage("- The cooldown of Absence of Faith is now 10 seconds.");
            myPlayer.SendClientMessage("Prayer of Absolution and Covenant of Tenacity:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("+ Provide 8 Righteous Fury / Soul Essence per second.");
            myPlayer.SendClientMessage("+ Convert your Strength and Melee Power bonus from items into Willpower and Healing Power.");
            myPlayer.SendClientMessage("- Reduce your armor by 50%. This is a multiplicative modifier, and thus also reduces the effect of potions, talismans and debuffs for armor.");
            myPlayer.SendClientMessage("Prayer of Devotion and Covenant of Vitality:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("+ Convert your Willpower and Healing Power bonus from items into Strength and Melee Power.");
            myPlayer.SendClientMessage("- You will inflict 50% less damage.");
            myPlayer.SendClientMessage("+ You will gain 50% of your Strength bonus from items as Willpower whenever you strike a target with a Path of Grace or Path of Sacrifice ability.");
            myPlayer.SendClientMessage("This effect stacks up to three times and lasts for 12 seconds, and will be removed if you remove the Prayer or Covenant.");
            myPlayer.SendClientMessage("+ Your detaunt will gain the effect of the Intimidating Repent or Terrifying Aura tactic, becoming AoE.");
            myPlayer.SendClientMessage("- The cooldown of your detaunt increases to 25 seconds.");
            myPlayer.SendClientMessage("+ Sigmar's Radiance and Transfer Essence gain 20% strikethrough, and heal for 250% of the damage dealt and mitigated, with no base component.");
            myPlayer.SendClientMessage("+ Sigmar's Shield gains 20% strikethrough.");
            myPlayer.SendClientMessage("+ Rend Soul and Divine Assault become undefendable, healing for 75% more and healing for the mitigated value as well.");
            myPlayer.SendClientMessage("Prayer of Righteousness:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("+ Convert your Willpower and Healing Power bonus from items into Strength and Melee Power.");
            myPlayer.SendClientMessage("+ The damage inflicted by Path of Wrath skills increases based on your Righteous Fury level, to a maximum of 40%.");
            myPlayer.SendClientMessage("- Any time you use a Path of Wrath skill, you will lose 5 Righteous Fury per second for 10 seconds.");
            myPlayer.SendClientMessage("+ Absence of Faith will debuff heals by 50%.");
            myPlayer.SendClientMessage("+ When you trigger your Prayer of Righteousness, it will increase your movement speed by 20% for 5 seconds. This will only affect you.");
            myPlayer.SendClientMessage("Covenant of Celerity:", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            myPlayer.SendClientMessage("+ Convert your Willpower and Healing Power bonus from items into Strength and Melee Power.");
            myPlayer.SendClientMessage("+ The damage inflicted by Path of Torture skills increases based on your Soul Essence level, to a maximum of 25%.");
            myPlayer.SendClientMessage("- Any time you use a Path of Torture skill, you will lose 5 Soul Essence per second for 10 seconds.");
        }*/

        public override bool ExperimentalModeCheckAbility(AbilityInfo abInfo)
        {
            if (!ExperimentalMode)
            {
                /*// Repent -> Intimidating Repent
                if (abInfo.Entry == 8245)
                {
                    Item mainHand = myPlayer.ItmInterface.GetItemInSlot((ushort) EquipSlot.MAIN_HAND);

                    return mainHand?.Info != null && mainHand.Info.TwoHanded;
                }*/

                return false;
            }

            switch (abInfo.Entry)
            {
                // Repent -> Intimidating Repent if Devotion is on
                // Terrifying Vision -> Terrifying Aura if Vitality is on
                case 8245:
                case 9555:
                    return _currentStance == 2;
            }
            return true;
        }

        public override void ExperimentalModeModifyAbility(AbilityInfo abInfo)
        {
            switch (abInfo.Entry)
            {
                case 8252: // Sigmar's Radiance
                case 9562: // Transfer Essence

                    if (_currentStance == 2)
                    {
                        foreach (AbilityCommandInfo command in abInfo.CommandInfo)
                        {
                            for (AbilityCommandInfo cmd = command; cmd != null; cmd = cmd.NextCommand)
                            {
                                // Is undefendable, to compensate deals half damage but heals for more
                                if (cmd.CommandName == "StealLife")
                                {
                                    cmd.DamageInfo.MinDamage = 0;
                                    cmd.DamageInfo.MaxDamage = 0;
                                    cmd.PrimaryValue = (short)(cmd.PrimaryValue * 5f);
                                }
                                else if (cmd.DamageInfo != null)
                                {
                                    cmd.DamageInfo.Defensibility -= 20;
                                    cmd.DamageInfo.ResultFromRaw = true;
                                }
                            }
                        }
                    }
                    break;
                case 8267: // Sigmar's Shield
                    if (_currentStance == 2)
                    {
                        foreach (AbilityCommandInfo command in abInfo.CommandInfo)
                        {
                            for (AbilityCommandInfo cmd = command; cmd != null; cmd = cmd.NextCommand)
                            {
                                if (cmd.DamageInfo != null)
                                    cmd.DamageInfo.Defensibility -= 20;
                            }
                        }
                    }
                    break;
                case 8270: // Absence of Faith
                    abInfo.Cooldown = 10;
                    break;
                case 9555: // Terrifying Vision
                case 8245: // Repent
                    if (_currentStance == 2)
                        abInfo.Cooldown = 25;
                    break;
                case 8242: // Prayer of Absolution
                case 9563: // Covenant of Tenacity
                case 8249: // Prayer of Devotion
                case 9567: // Covenant of Vitality
                    if (_currentStance == 1)
                        abInfo.Cooldown = (ushort)(DPS_SWITCH_CD_MS / 1000);
                    else
                        abInfo.Cooldown = (ushort)(SWITCH_CD_MS/1000);
                    break;
                case 8243: // Prayer of Righteousness
                case 9559: // Covenant of Celerity
                    abInfo.Cooldown = (ushort)(DPS_SWITCH_CD_MS / 1000);
                    break;
            }
        }

        private int _currentStance;

        private int CurrentStance
        {
            get { return _currentStance; }
            set
            {
                if (_currentStance == 2)
                    myPlayer.BuffInterface.RemoveBuffByEntry(18139);
                _currentStance = value;
            }
        }

        private long _drainTimeEnd;

        private int DRAIN_DURATION_MS = 10000;

        private bool DrainActive(long tick)
        {
            return tick < _drainTimeEnd;
        }

        public void UpdateDrain()
        {
            _drainTimeEnd = TCPManager.GetTimeStampMS() + DRAIN_DURATION_MS;
        }

        private int SWITCH_CD_MS = 15000;
        private int DPS_SWITCH_CD_MS = 300000;

        public override void ExperimentalModeModifyBuff(BuffInfo buffInfo, Unit target)
        {
            switch (buffInfo.Entry)
            {
                // Prayer of Absolution
                case 8242:
                    if (target != myPlayer)
                        return;
                    
                    // Reduces armor by 50% and converts Strength to Willpower
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 1));
                    // Adds 8 RF/sec
                    buffInfo.Interval = 1000;
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 2));
                    myPlayer.AbtInterface.SetCooldown(8249, _currentStance == 1 ? DPS_SWITCH_CD_MS : SWITCH_CD_MS);
                    myPlayer.AbtInterface.SetCooldown(8243, _currentStance == 1 ? DPS_SWITCH_CD_MS : SWITCH_CD_MS);
                    // Neutralize SE
                    if (_currentStance == 1 && myPlayer.CbtInterface.IsInCombat)
                    {
                        myPlayer.ConsumeActionPoints(myPlayer.ActionPoints);
                        ConsumeResource(250, true);
                    }
                    CurrentStance = 3;
                    break;
                // Covenant of Tenacity
                case 9563:
                    if (target != myPlayer)
                        return;

                    // Reduces armor by 50% and converts Strength to Willpower
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 1));
                    // Adds 8 SE/sec
                    buffInfo.Interval = 1000;
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 2));
                    myPlayer.AbtInterface.SetCooldown(9559, _currentStance == 1 ? DPS_SWITCH_CD_MS : SWITCH_CD_MS);
                    myPlayer.AbtInterface.SetCooldown(9567, _currentStance == 1 ? DPS_SWITCH_CD_MS : SWITCH_CD_MS);
                    // Neutralize SE
                    if (_currentStance == 1 && myPlayer.CbtInterface.IsInCombat)
                    {
                        myPlayer.ConsumeActionPoints(myPlayer.ActionPoints);
                        ConsumeResource(250, true);
                    }
                    CurrentStance = 3;

                    break;
                // Prayer of Devotion
                case 8249:
                    if (target != myPlayer)
                        return;

                    // Gain Willpower when striking a target
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 1));
                    // Convert Willpower to Strength
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 2));
                    myPlayer.AbtInterface.SetCooldown(8242, _currentStance == 1 ? DPS_SWITCH_CD_MS : SWITCH_CD_MS);
                    myPlayer.AbtInterface.SetCooldown(8243, _currentStance == 1 ? DPS_SWITCH_CD_MS : SWITCH_CD_MS);
                    // Neutralize SE
                    if (_currentStance == 1 && myPlayer.CbtInterface.IsInCombat)
                    {
                        myPlayer.ConsumeActionPoints(myPlayer.ActionPoints);
                        ConsumeResource(250, true);
                    }
                    // AoE detaunt
                    CurrentStance = 2;
                    break;
                // Covenant of Vitality
                case 9567:
                    if (target != myPlayer)
                        return;
                    // Gain Willpower when striking a target
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 2));
                    // Convert Willpower to Strength
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 3));
                    myPlayer.AbtInterface.SetCooldown(9559, _currentStance == 1 ? DPS_SWITCH_CD_MS : SWITCH_CD_MS);
                    myPlayer.AbtInterface.SetCooldown(9563, _currentStance == 1 ? DPS_SWITCH_CD_MS : SWITCH_CD_MS);
                    // Neutralize SE
                    if (_currentStance == 1 && myPlayer.CbtInterface.IsInCombat)
                    {
                        myPlayer.ConsumeActionPoints(myPlayer.ActionPoints);
                        ConsumeResource(250, true);
                    }
                    // AoE detaunt
                    CurrentStance = 2;
                    break;
                // Prayer of Righteousness
                case 8243:
                    if (target != myPlayer)
                        return;

                    // Procs on the caster also increase speed by 20%
                    buffInfo.AppendBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 1), 0);
                    // Damage of Path of Wrath skills increased by 1% for every 10 RF
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 2));
                    // Convert Willpower to Strength
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 3));
                    myPlayer.AbtInterface.SetCooldown(8249, DPS_SWITCH_CD_MS);
                    myPlayer.AbtInterface.SetCooldown(8242, DPS_SWITCH_CD_MS);
                    // Neutralize Fury
                    if (myPlayer.CbtInterface.IsInCombat)
                    {
                        myPlayer.ConsumeActionPoints(myPlayer.ActionPoints);
                        ConsumeResource(250, true);
                    }
                    CurrentStance = 1;
                    break;
                // Covenant of Celerity
                case 9559:
                    if (target != myPlayer)
                        return;

                    //Damage of Path of Torture skills increased by 1 % for every 10 SE
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 2));
                    // Convert Willpower to Strength
                    buffInfo.AddBuffCommand(AbilityMgr.GetBuffCommand(buffInfo.Entry, 3));
                    myPlayer.AbtInterface.SetCooldown(9563, DPS_SWITCH_CD_MS);
                    myPlayer.AbtInterface.SetCooldown(9567, DPS_SWITCH_CD_MS);
                    // Neutralize SE
                    if (myPlayer.CbtInterface.IsInCombat)
                    {
                        myPlayer.ConsumeActionPoints(myPlayer.ActionPoints);
                        ConsumeResource(250, true);
                    }
                    CurrentStance = 1;
                    break;
                case 8244: // Divine Assault
                case 9554: // Rend Soul
                    if (_currentStance == 2)
                    {
                        foreach (BuffCommandInfo command in buffInfo.CommandInfo)
                        {
                            for (BuffCommandInfo cmd = command; cmd != null; cmd = cmd.NextCommand)
                            {
                                // Is undefendable, to compensate deals half damage but heals for 75% more
                                if (cmd.CommandName == "StealLife")
                                    cmd.PrimaryValue = (int)(cmd.PrimaryValue * 1.75f);
                                else if (cmd.DamageInfo != null)
                                {
                                    cmd.DamageInfo.Undefendable = true;
                                    cmd.DamageInfo.ResultFromRaw = true;
                                }
                            }
                        }
                    }
                    break;
                case 8270: // Absence of Faith becomes 50% with DPS on
                    if (_currentStance == 1)
                        buffInfo.CommandInfo[0].SecondaryValue = -50;
                    break;
                // WP/DoK DoTs (easier than using modifier system)
                /*case 3054:
                case 3055:
                case 3737:
                case 3757:
                case 8271:
                case 8295:
                case 9575:
                    if (_currentStance == 1)
                    {
                        foreach (BuffCommandInfo cmd in buffInfo.CommandInfo)
                        {
                            for (BuffCommandInfo command = cmd; command != null; command = command.NextCommand)
                                if (command.DamageInfo != null)
                                    command.DamageInfo.DamageBonus += GetCurrentResourceLevel(0)*0.01f;
                        }

                        UpdateDrain();
                    }
                    break;
                */
            }
        }

        public override void NotifyInitialized()
        {
            myPlayer.SendClientMessage("This class has modifications. Enter the command \".ab changelist\" to see the changelist.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            /*
            if (myPlayer._Value.ExperimentalMode)
            {
                myPlayer._Value.ExperimentalMode = false;
                CharMgr.Database.SaveObject(myPlayer._Value);
                myPlayer.SendClientMessage("Experimental mode for this career is not currently available.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            }
            */
        }

        #endregion
    }
}
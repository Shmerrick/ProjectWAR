using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities.CareerInterfaces
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
                                    cmd.DamageInfo.MinDamage = 0;
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
            }
        }
    }
}
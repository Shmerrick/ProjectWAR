using System;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Abilities
{
    public static class CombatManager
    {


        #region Defense
        public static bool CheckDefense(AbilityCommandInfo cmdInfo, Unit caster, Unit target, bool isAoE)
        {
            if (target is KeepDoor.KeepGameObject)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

                Out.WriteUInt16(caster.Oid);
                Out.WriteUInt16(target.Oid);
                Out.WriteUInt16(cmdInfo.Entry);

                Out.WriteByte(0);
                Out.WriteByte((byte)CombatEvent.COMBATEVENT_BLOCK);
                Out.WriteByte(5);

                Out.WriteByte(0);

                target.DispatchPacketUnreliable(Out, true, caster);

                return true;
            }

            if (target is GameObject)
                return false;

            bool isInFront = target.IsObjectInFront(caster, 140);

            // Disruption is 360 degrees
            if (!isInFront && cmdInfo.AttackingStat < 8)
                return false;

            int offensiveStat, defensiveStat;
            byte defenseEvent = 0;

            #region GetStat
            switch (cmdInfo.AttackingStat)
            {
                case 1: // Strength and Weaponskill
                    {
                        offensiveStat = Math.Max(1, (int)caster.StsInterface.GetTotalStat(Stats.Strength));
                        defensiveStat = Math.Max(1, (int)target.StsInterface.GetTotalStat(Stats.WeaponSkill));
                    }
                    break;
                case 8: // Ballistic and Initiative
                    {
                        offensiveStat = Math.Max(1, (int)caster.StsInterface.GetTotalStat(Stats.BallisticSkill));
                        defensiveStat = Math.Max(1, (int)target.StsInterface.GetTotalStat(Stats.Initiative));
                    }
                    break;
                case 9: // Intelligence and Willpower
                    {
                        offensiveStat = Math.Max(1, (int)caster.StsInterface.GetTotalStat(Stats.Intelligence));
                        defensiveStat = Math.Max(1, (int)target.StsInterface.GetTotalStat(Stats.Willpower));
                    }
                    break;
                default:
                    return false;
            }
            #endregion

            #region Block

            if (isInFront)
            {
                /*
                byte onehandweaponblockbonus = 0;

                if (cmdInfo.AttackingStat == 1 && Caster.ItmInterface.Items[10] != null && Caster.ItmInterface.Items[10].Info.TwoHanded == false)
                    onehandweaponblockbonus = 10;
                */

                if (target.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND) != null && target.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND).Info?.Type == 5)
                {
                    //Block is [Block Rating of Shield / (Level * 7.5 + 50)] * 20
                    //double block = (target.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND).Info.Armor / (target.EffectiveLevel * 7.5 + 50) * 20);
                    //Contestion based on offensive stat. This gets added to make it harder to actually do a defensive event, without actually contesting it directly above.
                    //This should mimic the live formula.
                    //double removedDefense = (((offensiveStat) * 100) / (((caster.EffectiveLevel * 7.5) + 50) * 7.5));

                    //double baseRoll = 0d;
                    //baseRoll += removedDefense;

                    double block = CalculateBlockRoll(target.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND).Info.Armor, offensiveStat, cmdInfo.DamageInfo, target.StsInterface.GetTotalStat(Stats.Block), caster.StsInterface.GetStatLinearModifier(Stats.BlockStrikethrough));
                    //block = (int)(block * caster.StsInterface.GetStatPercentageModifier(Stats.BlockStrikethrough));
                    //double finalRoll = (StaticRandom.Instance.NextDouble() * (100d + baseRoll));

                    if (StaticRandom.Instance.Next(100) <= block)
                    {
                        target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_BLOCK);
                        caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_BLOCK);

                        defenseEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                    }
                }
            }

            #endregion

            if (defenseEvent == 0)
            {
                #region Parry, Dodge, Disrupt                

                ////Parry/Dodge/Disrupt chance from tooltip
                //double secondaryDefense = (int)((((double)defensiveStat / offensiveStat * 0.075) * 100));
                ////Contestion based on offensive stat. This gets added to make it harder to actually do a defensive event, without actually contesting it directly above.
                ////This should mimic the live formula.
                ////double removedDefense = (((offensiveStat) * 100) / (((caster.EffectiveLevel * 7.5) + 50) * 7.5));

                //// There is no cap on parry from stats. There is, however a max defensible amount of 0.75 similar to armor to the final roll.
                //if (secondaryDefense > 25)
                //    secondaryDefense = 25;

                ////double baseRoll = 0d;
                ////baseRoll += removedDefense;

                //if (cmdInfo.DamageInfo != null)
                //    secondaryDefense += cmdInfo.DamageInfo.Defensibility;

                double secondaryDefense = 0.0;
                switch (cmdInfo.AttackingStat)
                {
                    case 1: // Parry
                        {
                            //secondaryDefense += target.StsInterface.GetTotalStat(Stats.Parry) - caster.StsInterface.GetStatLinearModifier(Stats.ParryStrikethrough);
                            secondaryDefense = CalculatePDDRoll(defensiveStat, offensiveStat, cmdInfo.DamageInfo, target.StsInterface.GetTotalStat(Stats.Parry), caster.StsInterface.GetStatLinearModifier(Stats.ParryStrikethrough));

                            if (StaticRandom.Instance.Next(100) <= secondaryDefense)
                            {
                                target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_PARRY); // Parry
                                caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_PARRY);
                                defenseEvent = (byte)CombatEvent.COMBATEVENT_PARRY;
                            }
                        }
                        break;
                    case 8: // Evade
                        {
                            //secondaryDefense += target.StsInterface.GetTotalStat(Stats.Evade) - caster.StsInterface.GetStatLinearModifier(Stats.EvadeStrikethrough);
                            secondaryDefense = CalculatePDDRoll(defensiveStat, offensiveStat, cmdInfo.DamageInfo, target.StsInterface.GetTotalStat(Stats.Evade), caster.StsInterface.GetStatLinearModifier(Stats.EvadeStrikethrough));

                            if (StaticRandom.Instance.Next(100) <= secondaryDefense)
                            {
                                target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_EVADE); // Evade
                                caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_EVADE);
                                defenseEvent = (byte)CombatEvent.COMBATEVENT_EVADE;
                            }
                        }
                        break;
                    case 9: // Disrupt
                        {
                            //secondaryDefense += target.StsInterface.GetTotalStat(Stats.Disrupt) - caster.StsInterface.GetStatLinearModifier(Stats.DisruptStrikethrough);
                            secondaryDefense = CalculatePDDRoll(defensiveStat, offensiveStat, cmdInfo.DamageInfo, target.StsInterface.GetTotalStat(Stats.Disrupt), caster.StsInterface.GetStatLinearModifier(Stats.DisruptStrikethrough));

                            if (StaticRandom.Instance.Next(100) <= secondaryDefense) // Disrupt
                            {
                                target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_DISRUPT);
                                caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_DISRUPT);
                                defenseEvent = (byte)CombatEvent.COMBATEVENT_DISRUPT;
                            }
                        }
                        break;
                }

                #endregion

                if (defenseEvent == 0)
                    return false;
            }

            #region Packet

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(cmdInfo.Entry);

            outl.WriteByte(0);
            outl.WriteByte(defenseEvent);
            outl.WriteByte(5);

            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, caster);

            #endregion

            AbilityDamageInfo tempDmg = new AbilityDamageInfo
            {
                Entry = cmdInfo.Entry,
                DamageEvent = defenseEvent,
                IsAoE = isAoE
            };

            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasDefended, tempDmg, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DefendedAgainst, tempDmg, caster);

            return true;
        }

        public static double CalculateBlockRoll(ushort blockRatingOffhand, int offensiveStat, AbilityDamageInfo damageInfo, short targetBlock, int casterBlockStrikeThrough)
        {
            double block = (((double)blockRatingOffhand / offensiveStat) * 0.2) * 100;
            if (block > 50)
                block = 50;

            if (damageInfo != null)
                block += damageInfo.Defensibility;

            block += targetBlock - casterBlockStrikeThrough;
            return block;
        }

        public static double CalculatePDDRoll(int defensiveStat, int offensiveStat, AbilityDamageInfo damageInfo, short targetDef, int casterDefStrikeThrough)
        {
            double secondaryDefense = ((double)defensiveStat / offensiveStat * 0.075) * 100;
            if (secondaryDefense > 25)
                secondaryDefense = 25;

            if (damageInfo != null)
                secondaryDefense += damageInfo.Defensibility;

            secondaryDefense += targetDef - casterDefStrikeThrough;

            return secondaryDefense;
        }

        // Awful, but it's not worth implementing defense handling for buff commands for one ability.
        public static bool CheckMagnetDefense(ushort entry, Unit caster, Unit target, bool isAoE)
        {
            uint offensiveStat = Math.Max(1, (uint)caster.StsInterface.GetTotalStat(Stats.BallisticSkill));
            uint defensiveStat = Math.Max(1, (uint)target.StsInterface.GetTotalStat(Stats.Initiative));

            byte defenseEvent = 0;

            AbilityDamageInfo tempDmg = new AbilityDamageInfo
            {
                Entry = entry,
                DamageEvent = defenseEvent,
                IsAoE = isAoE
            };

            double secondaryDefense = CalculatePDDRoll((int)defensiveStat, (int)offensiveStat, tempDmg, target.StsInterface.GetTotalStat(Stats.Evade), caster.StsInterface.GetStatLinearModifier(Stats.EvadeStrikethrough));

            if (StaticRandom.Instance.Next(100) <= secondaryDefense) // Evade
            {
                target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_EVADE);
                caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_EVADE);
                defenseEvent = (byte)CombatEvent.COMBATEVENT_EVADE;
            }

            if (Math.Abs(caster.Z - target.Z) > 300)
            {
                target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_EVADE);
                caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_EVADE);
                defenseEvent = (byte)CombatEvent.COMBATEVENT_EVADE;
            }

            if (defenseEvent == 0)
                return false;

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(entry);

            outl.WriteByte(0);
            outl.WriteByte(defenseEvent);
            outl.WriteByte(5);

            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, caster);


            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasDefended, tempDmg, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DefendedAgainst, tempDmg, caster);

            return true;
        }

        public static bool CheckRiftDefense(ushort entry, Unit caster, Unit target, bool isAoE)
        {
            uint offensiveStat = Math.Max(1, (uint)caster.StsInterface.GetTotalStat(Stats.Intelligence));
            uint defensiveStat = Math.Max(1, (uint)target.StsInterface.GetTotalStat(Stats.Willpower));

            byte defenseEvent = 0;

            AbilityDamageInfo tempDmg = new AbilityDamageInfo
            {
                Entry = entry,
                DamageEvent = defenseEvent,
                IsAoE = isAoE
            };

            double secondaryDefense = CalculatePDDRoll((int)defensiveStat, (int)offensiveStat, tempDmg, target.StsInterface.GetTotalStat(Stats.Disrupt), caster.StsInterface.GetStatLinearModifier(Stats.DisruptStrikethrough));

            if (StaticRandom.Instance.Next(100) <= secondaryDefense) // Disrupt
            {
                target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_DISRUPT);
                caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_DISRUPT);
                defenseEvent = (byte)CombatEvent.COMBATEVENT_DISRUPT;
            }

            if (Math.Abs(caster.Z - target.Z) > 300)
            {
                target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_DISRUPT);
                caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_DISRUPT);
                defenseEvent = (byte)CombatEvent.COMBATEVENT_DISRUPT;
            }

            if (defenseEvent == 0)
                return false;

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(entry);

            outl.WriteByte(0);
            outl.WriteByte(defenseEvent);
            outl.WriteByte(5);

            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, caster);

            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasDefended, tempDmg, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DefendedAgainst, tempDmg, caster);

            return true;
        }

        private static bool WasDefended(AbilityDamageInfo damageInfo, Unit caster, Unit target)
        {
            if (target is GameObject)
                return false;

            if (target.ShouldDefend(caster, damageInfo))
            {
                damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                return true;
            }

            bool isInFront = target.IsObjectInFront(caster, 140);

            // Disruption is 360 degrees
            // Exempt Inevitable Doom
            if (!isInFront && damageInfo.StatUsed < 8 && damageInfo.Entry != 3320)
                return false;

            int offensiveStat, defensiveStat;

            #region GetStat
            switch (damageInfo.StatUsed)
            {
                case 1: // Strength and Weaponskill
                    {
                        offensiveStat = Math.Max(1, (int)caster.StsInterface.GetTotalStat(Stats.Strength));
                        defensiveStat = Math.Max(1, (int)target.StsInterface.GetTotalStat(Stats.WeaponSkill));
                    }
                    break;
                case 8: // Ballistic and Initiative
                    {
                        offensiveStat = Math.Max(1, (int)caster.StsInterface.GetTotalStat(Stats.BallisticSkill));
                        defensiveStat = Math.Max(1, (int)target.StsInterface.GetTotalStat(Stats.Initiative));
                    }
                    break;
                case 9: // Intelligence and Willpower
                    {
                        offensiveStat = Math.Max(1, (int)caster.StsInterface.GetTotalStat(Stats.Intelligence));
                        defensiveStat = Math.Max(1, (int)target.StsInterface.GetTotalStat(Stats.Willpower));
                    }
                    break;
                default:
                    return false;
            }
            #endregion

            #region Block

            if (isInFront)
            {
                if (target.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND) != null && target.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND).Info?.Type == 5)
                {

                    double block = CalculateBlockRoll(target.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND).Info.Armor, offensiveStat, damageInfo, target.StsInterface.GetTotalStat(Stats.Block), caster.StsInterface.GetStatLinearModifier(Stats.BlockStrikethrough));
                    if (StaticRandom.Instance.Next(100) <= block)
                    {
                        target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_BLOCK);
                        caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_BLOCK);

                        damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                        return true;
                    }
                }
            }

            #endregion

            #region Parry, Dodge, Disrupt


            double secondaryDefense = 0.0;
            switch (damageInfo.StatUsed)
            {
                case 1: // Parry
                    {
                        //secondaryDefense += target.StsInterface.GetTotalStat(Stats.Parry) - caster.StsInterface.GetStatLinearModifier(Stats.ParryStrikethrough);
                        secondaryDefense = CalculatePDDRoll(defensiveStat, offensiveStat, damageInfo, target.StsInterface.GetTotalStat(Stats.Parry), caster.StsInterface.GetStatLinearModifier(Stats.ParryStrikethrough));

                        if (StaticRandom.Instance.Next(100) <= secondaryDefense)
                        {
                            target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_PARRY);
                            caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_PARRY);
                            damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_PARRY;

                            target.CbtInterface.SetDefenseTimer(damageInfo.DamageEvent);
                            caster.CbtInterface.SetDefendedAgainstTimer(damageInfo.DamageEvent);
                            return true;
                        }
                    }
                    break;
                case 8: // Evade
                    {
                        //secondaryDefense += target.StsInterface.GetTotalStat(Stats.Evade) - caster.StsInterface.GetStatLinearModifier(Stats.EvadeStrikethrough);
                        secondaryDefense = CalculatePDDRoll(defensiveStat, offensiveStat, damageInfo, target.StsInterface.GetTotalStat(Stats.Evade), caster.StsInterface.GetStatLinearModifier(Stats.EvadeStrikethrough));

                        if (StaticRandom.Instance.Next(100) <= secondaryDefense)
                        {
                            target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_EVADE);
                            caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_EVADE);
                            damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_EVADE;

                            target.CbtInterface.SetDefenseTimer(damageInfo.DamageEvent);
                            caster.CbtInterface.SetDefendedAgainstTimer(damageInfo.DamageEvent);
                            return true;
                        }
                    }
                    break;
                case 9: // Disrupt
                    {
                        //secondaryDefense += target.StsInterface.GetTotalStat(Stats.Disrupt) - caster.StsInterface.GetStatLinearModifier(Stats.DisruptStrikethrough);
                        secondaryDefense = CalculatePDDRoll(defensiveStat, offensiveStat, damageInfo, target.StsInterface.GetTotalStat(Stats.Disrupt), caster.StsInterface.GetStatLinearModifier(Stats.DisruptStrikethrough));

                        if (StaticRandom.Instance.Next(100) <= secondaryDefense) // Disrupt
                        {
                            target.CbtInterface.SetDefenseTimer((byte)CombatEvent.COMBATEVENT_DISRUPT);
                            caster.CbtInterface.SetDefendedAgainstTimer((byte)CombatEvent.COMBATEVENT_DISRUPT);
                            damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_DISRUPT;

                            target.CbtInterface.SetDefenseTimer(damageInfo.DamageEvent);
                            caster.CbtInterface.SetDefendedAgainstTimer(damageInfo.DamageEvent);
                            return true;
                        }
                    }
                    break;
            }

            #endregion

            return false;
        }

        #endregion

        #region DoT

        public static void SetDamageAmount(AbilityDamageInfo damageInfo, byte level, Unit caster, Unit target)
        {
            target.CbtInterface.OnAttacked(caster);
            caster.CbtInterface.RefreshCombatTimer();

            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.AttackedTarget, damageInfo, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasAttacked, damageInfo, caster);

            #region Base Damage

            if (damageInfo.CastTimeDamageMult == 0)
                damageInfo.CastTimeDamageMult = 1.5f;

            damageInfo.PrecalcDamage = damageInfo.GetDamageForLevel(level);

            #endregion

            caster.ModifyDamageOut(damageInfo);
            target.ModifyDamageIn(damageInfo);

            Creature creature = target.GetCreature();
            if (creature != null)
            {
                creature.CheckDamageCaster(caster, damageInfo);
            }

            if (damageInfo.DamageType == DamageTypes.RawDamage)
                return;
            if (damageInfo.PriStatMultiplier > 0.0f)
            {
                damageInfo.PrecalcDamage += caster.ItmInterface.GetWeaponDamage(damageInfo.WeaponMod) * damageInfo.PriStatMultiplier;
            }
            else
            {
                damageInfo.PrecalcDamage += caster.ItmInterface.GetWeaponDamage(damageInfo.WeaponMod) * damageInfo.WeaponDamageScale * damageInfo.CastTimeDamageMult;
            }

            if (damageInfo.StatUsed > 0)
            {
                AddOffensiveStats(caster, damageInfo, 0.2f, true);
                AddLinearMitigation(target, damageInfo, 0.2f, true);
            }

            if (damageInfo.DamageType == DamageTypes.Physical)
                CheckArmorReduction(caster, target, damageInfo, true);
            else
                CheckResistanceReduction(caster, target, damageInfo, true);

            #region Phys/Mag multipliers

            damageInfo.DamageBonus +=
                caster.StsInterface.GetStatBonusModifier(damageInfo.StatUsed == 9 ? Stats.MagicPower : Stats.MeleePower)
                + target.StsInterface.GetStatBonusModifier(damageInfo.StatUsed == 9 ? Stats.IncomingMagicDamage : Stats.IncomingMeleeDamage);
            damageInfo.DamageReduction *= target.StsInterface.GetStatReductionModifier(damageInfo.StatUsed == 9 ? Stats.IncomingMagicDamage : Stats.IncomingMeleeDamage)
                * caster.StsInterface.GetStatReductionModifier(damageInfo.StatUsed == 9 ? Stats.MagicPower : Stats.MeleePower);

            #endregion

            damageInfo.ApplyDamageModifiers(true);

            if (!caster.CbtInterface.IsAttacking)
                caster.CbtInterface.IsAttacking = true;
        }

        public static void SetHealAmount(AbilityDamageInfo damageInfo, byte level, Unit caster, Unit target)
        {
            if (damageInfo.CastTimeDamageMult == 0)
                damageInfo.CastTimeDamageMult = 1.5f;

            damageInfo.PrecalcDamage = damageInfo.GetDamageForLevel(level);

            target.ModifyHealOut(damageInfo);
            target.ModifyHealIn(damageInfo);

            if (damageInfo.DamageType != DamageTypes.RawHealing)
            {
                // Neutralize some effects of AM/Shaman mechanic bonuses if self-cast
                if (damageInfo.UseItemStatTotal && caster == target)
                {
                    // damageInfo.DamageBonus -= 0.25f; // normalization
                    // damageInfo.DamageReduction *= 0.8f; // additional "risk" penalty
                    damageInfo.UseItemStatTotal = false;
                }

                AddOffensiveStats(caster, damageInfo, 0.2f, true);

                damageInfo.ApplyDamageModifiers(true);
            }

            caster.CbtInterface.OnDealHeal(target, 0);
        }

        public static void InflictPrecalculatedDamage(AbilityDamageInfo damageInfo, Unit caster, Unit target, float multiplier, bool bFinalize)
        {
            damageInfo.Damage = damageInfo.PrecalcDamage * multiplier;
            damageInfo.Mitigation = damageInfo.PrecalcMitigation * multiplier;
            damageInfo.Absorption = 0;
            damageInfo.TransferFactor = 1;

            if (target is KeepDoor.KeepGameObject && caster is Player)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

                Out.WriteUInt16(caster.Oid);
                Out.WriteUInt16(target.Oid);
                Out.WriteUInt16(damageInfo.Entry);

                Out.WriteByte(0);
                Out.WriteByte((byte)CombatEvent.COMBATEVENT_BLOCK);
                Out.WriteByte(5);

                Out.WriteByte(0);

                target.DispatchPacketUnreliable(Out, true, caster);

                return;
            }

            if (damageInfo.DamageType != DamageTypes.RawDamage)
            {
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ShieldPass, damageInfo, caster);

                #region Dealing/Receiving Event
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingDamage, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingDamage, damageInfo, caster);
                #endregion

                CheckCriticalHit(caster, target, damageInfo);

                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealtDamage, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivedDamage, damageInfo, caster);

                #region General Multipliers

                damageInfo.DamageBonus +=
                    caster.StsInterface.GetStatBonusModifier(Stats.OutgoingDamagePercent)
                    + target.StsInterface.GetStatBonusModifier(Stats.IncomingDamagePercent);
                damageInfo.DamageReduction *=
                    target.StsInterface.GetStatReductionModifier(Stats.IncomingDamagePercent)
                    * caster.StsInterface.GetStatReductionModifier(Stats.OutgoingDamagePercent);

                #endregion

                // Guard is separate because it needs to come after shielding
                target.BuffInterface.CheckGuard(damageInfo, caster);

                damageInfo.ApplyDamageModifiers();
            }

            #region Application

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(damageInfo.DisplayEntry); // 00 00 07 D D

            outl.WriteByte(damageInfo.CastPlayerSubID);
            outl.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
            if (multiplier == 1.0f)
                outl.WriteByte(0x07);
            else if (!bFinalize)
                outl.WriteByte(0x0B);
            else
                outl.WriteByte(0x0F);

            outl.WriteZigZag(-(int)damageInfo.Damage);
            if (damageInfo.Mitigation > 0)
                outl.WriteZigZag((ushort)damageInfo.Mitigation);
            if (damageInfo.Absorption > 0)
                outl.WriteZigZag((ushort)damageInfo.Absorption);
            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, caster);

            if (target.ReceiveDamage(caster, damageInfo))
            {
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.OnKill, damageInfo, target);

                damageInfo.WasLethalDamage = true;

                Pet pet = caster as Pet;
                Player plrCaster = pet != null ? pet.Owner : caster as Player;

                if (plrCaster != null)
                {
                    Player plrTarget = target as Player;

                    if (plrTarget != null)
                        SendDeathSpam(damageInfo, plrCaster, plrTarget);
                }
            }

            #endregion
        }

        public static void PrecalculatedHealTarget(AbilityDamageInfo damageInfo, Unit caster, Unit target, byte divisor, bool bFinalize)
        {
            damageInfo.Damage = damageInfo.PrecalcDamage / divisor;

            if (damageInfo.DamageType != DamageTypes.RawHealing)
            {
                if (!damageInfo.NoCrits)
                {
                    #region CriticalHeal

                    int rand = StaticRandom.Instance.Next(0, 100);

                    if (target != null && target.IsUnit())
                    {
                        int chanceToBeCrit = 10 + damageInfo.CriticalHitRate + caster.StsInterface.GetTotalStat(Stats.CriticalHitRate) + caster.StsInterface.GetTotalStat(Stats.HealCritRate);
                        if (rand <= chanceToBeCrit)
                        {
                            damageInfo.Damage *= 1.35f + (float)StaticRandom.Instance.NextDouble() * 0.2f;

                            damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_ABILITY_CRITICAL;
                        }
                        else
                            damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_HIT;
                    }
                    #endregion
                }

                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingHeal, damageInfo, target);

                damageInfo.DamageBonus += caster.StsInterface.GetStatBonusModifier(Stats.OutgoingHealPercent) + target.StsInterface.GetStatBonusModifier(Stats.IncomingHealPercent);
                damageInfo.DamageReduction *= target.StsInterface.GetStatReductionModifier(Stats.IncomingHealPercent) * caster.StsInterface.GetStatReductionModifier(Stats.OutgoingHealPercent);

                damageInfo.ApplyDamageModifiers();

                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingHeal, damageInfo, caster);

                AwardOutOfGroupHealing(caster, target, (int) damageInfo.Damage, 45, 4);
                
            }

            #region Application

            int pointsHealed = target.ReceiveHeal(caster, (ushort)damageInfo.Damage, damageInfo.HealHatredScale);

            if (pointsHealed == -1)
                return;

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(damageInfo.DisplayEntry); // 00 00 07 D D

            outl.WriteByte(damageInfo.CastPlayerSubID);
            outl.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
            outl.WriteByte(bFinalize ? (byte)0xF : (byte)0xB);   //7    o 42

            outl.WriteZigZag((ushort)(pointsHealed));
            if (damageInfo.Damage > pointsHealed)
                outl.WriteZigZag((ushort)damageInfo.Damage - pointsHealed);
            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, caster);

            #endregion
        }

        private static void AwardOutOfGroupHealing(Unit caster, Unit target, int healAmount, int divisor, int random)
        {
            if (caster is Player)
            {
                if (target is Player)
                {
                    if ((target as Player).ImpactMatrixManager.HasImpacts((target as Player).CharacterId))
                    {
                        var rand = StaticRandom.Instance.Next(100);
                        // General healing
                        if (rand < HEAL_CONTRIBUTION_CHANCE)
                            (caster as Player)?.UpdatePlayerBountyEvent((byte)ContributionDefinitions.GENERAL_HEALING);


                        // Check for out of group healing
                        if (((caster as Player).PriorityGroup != (target as Player).PriorityGroup) || ((caster as Player).PriorityGroup == null))
                        {
                            if (rand < HEAL_CONTRIBUTION_CHANCE)
                            {
                                (caster as Player)?.UpdatePlayerBountyEvent((byte) ContributionDefinitions
                                    .OUT_OF_GROUP_HEALING);
                            }
                            if (StaticRandom.Instance.Next(100) < OOG_HEAL_RENOWN_CHANCE)
                            {
                                (caster as Player).AddRenown(
                                    (uint) (healAmount / divisor) + (uint) StaticRandom.Instance.Next(random),
                                    false,
                                    RewardType.None, "Out of Group Healing");
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Proc

        public static void InflictProcDamage(AbilityDamageInfo damageInfo, byte level, Unit caster, Unit target)
        {
            if (target.IsDead)
                return;

            target.CbtInterface.RefreshCombatTimer();
            caster.CbtInterface.RefreshCombatTimer();

            #region Base Damage

            if (damageInfo.CastTimeDamageMult == 0)
                damageInfo.CastTimeDamageMult = 1.5f;

            damageInfo.Damage = damageInfo.GetDamageForLevel(level);

            #endregion

            target.ModifyDamageIn(damageInfo);

            Creature creature = target.GetCreature();
            if (creature != null)
            {
                creature.CheckDamageCaster(caster, damageInfo);
            }

            if (damageInfo.DamageType != DamageTypes.RawDamage)
            {
                #region Weapon DPS
                // Procs SHOULD NOT add damage from weapons.
                // damageInfo.Damage += caster.ItmInterface.GetWeaponDamage(damageInfo.WeaponMod)*damageInfo.WeaponDamageScale*damageInfo.CastTimeDamageMult;

                #endregion

                if (damageInfo.StatUsed > 0)
                {
                    AddOffensiveStats(caster, damageInfo, 0.2f, false);
                    AddLinearMitigation(target, damageInfo, 0.2f, false);
                }

                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingDamage, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingDamage, damageInfo, caster);
            }

            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ShieldPass, damageInfo, caster);

            if (damageInfo.DamageType != DamageTypes.RawDamage)
            {
                #region Armour / Resistance

                float originalResistance, damageTypeResistance = 0;

                if (damageInfo.DamageType == 0) // physical damage
                {
                    if (damageInfo.SubDamageType != SubDamageTypes.Oil)
                    {
                        float secondaryStat = caster.StsInterface.GetTotalStat(Stats.WeaponSkill);
                        float pen = secondaryStat / (7.5f * caster.EffectiveLevel + 50f) * 0.25f;

                        originalResistance = target.StsInterface.GetTotalStat(Stats.Armor);

                        if (originalResistance <= 0)
                            damageTypeResistance = 0;
                        else
                        {
                            damageTypeResistance = originalResistance / (caster.EffectiveLevel * 44f) * 0.4f; //this will give you the total mitigation from armour.
                            damageTypeResistance *= 1f - pen;
                            if (damageTypeResistance > 0.75f) //puts in hard cap for physical mitigation of 75%
                                damageTypeResistance = 0.75f;
                        }
                    }
                    else
                    {
                        originalResistance = target.StsInterface.GetTotalStat(Stats.Armor);

                        if (originalResistance <= 0)
                            damageTypeResistance = 0;
                        else
                        {
                            damageTypeResistance = originalResistance / (caster.EffectiveLevel * 44f) * 0.4f; //this will give you the total mitigation from armour.
                            //damageTypeResistance *= 1f - pen;
                            if (damageTypeResistance > 0.75f) //puts in hard cap for physical mitigation of 75%
                                damageTypeResistance = 0.75f;
                        }
                    }
                }
                else
                {
                    originalResistance = target.StsInterface.GetTotalStat((Stats)(13 + damageInfo.DamageType)); // 14 Spirit, 15 Elemental, 16 Corporeal.
                    if (originalResistance == 0)
                        damageTypeResistance = 0;
                    else
                    {
                        damageTypeResistance = (originalResistance / (caster.EffectiveLevel * 8.4f)) * 0.2f;
                        if (damageTypeResistance > 0.4)
                            damageTypeResistance = ((originalResistance / (caster.EffectiveLevel * 8.4f)) * 0.2f - 0.4f) / 3.0f + 0.4f;
                        if (damageTypeResistance > 0.75f)
                            damageTypeResistance = 0.75f;
                    }
                }

                damageInfo.Mitigation += (ushort)(damageInfo.Damage * damageTypeResistance);
                damageInfo.Damage -= (ushort)(damageInfo.Damage * damageTypeResistance);

                #endregion

                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivedDamage, damageInfo, caster);

                // Guard is separate because it needs to come after shielding
                target.BuffInterface.CheckGuard(damageInfo, caster);

                damageInfo.ApplyDamageModifiers();
            }

            #region Application

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(damageInfo.DisplayEntry); // 00 00 07 D D

            outl.WriteByte(damageInfo.CastPlayerSubID);
            outl.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
            outl.WriteByte(7);   //7    o 42

            outl.WriteZigZag(-(ushort)damageInfo.Damage);
            if (damageInfo.Mitigation > 0)
                outl.WriteZigZag((ushort)damageInfo.Mitigation);
            if (damageInfo.Absorption > 0)
                outl.WriteZigZag((ushort)damageInfo.Absorption);
            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, caster);

            if (target.ReceiveDamage(caster, damageInfo))
            {
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.OnKill, damageInfo, target);

                damageInfo.WasLethalDamage = true;

                Pet pet = caster as Pet;
                Player plrCaster = pet != null ? pet.Owner : caster as Player;

                if (plrCaster != null)
                {
                    Player plrTarget = target as Player;

                    if (plrTarget != null)
                        SendDeathSpam(damageInfo, plrCaster, plrTarget);
                }
            }
            #endregion

        }

        public static void ProcHealTarget(AbilityDamageInfo damageInfo, byte level, Unit caster, Unit target)
        {
            #region Base Damage

            if (damageInfo.CastTimeDamageMult == 0)
                damageInfo.CastTimeDamageMult = 1.5f;

            damageInfo.Damage = damageInfo.GetDamageForLevel(level);

            #endregion

            target.ModifyHealIn(damageInfo);

            AddOffensiveStats(caster, damageInfo, 0.2f, false);

            if (damageInfo.DamageType != DamageTypes.RawHealing)
            {
                /*
                #region CriticalHeal

                int rand = StaticRandom.Instance.Next(0, 100);

                if (target != null && target.IsUnit())
                {
                    int chanceToBeCrit = 10 + damageInfo.CriticalHitRate + caster.StsInterface.GetTotalStat(BonusTypes.BONUSTYPES_EBONUS_CRITICAL_HIT_RATE) + caster.StsInterface.GetTotalStat(BonusTypes.BONUSTYPES_EBONUS_CRITICAL_HIT_RATE_HEALING);
                    if (rand <= chanceToBeCrit)
                    {
                        damageInfo.Damage *= 1.5f;

                        damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_ABILITY_CRITICAL;
                    }
                    else
                        damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_HIT;
                }

                #endregion
                */

                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingHeal, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingHeal, damageInfo, caster);

                /*
                damageInfo.DamageBonus += caster.StsInterface.GetStatBonusModifier(BonusTypes.BONUSTYPES_EBONUS_OUT_HEAL_PERCENT) + target.StsInterface.GetStatBonusModifier(BonusTypes.BONUSTYPES_EBONUS_IN_HEAL_PERCENT);
                damageInfo.DamageReduction *= target.StsInterface.GetStatReductionModifier(BonusTypes.BONUSTYPES_EBONUS_IN_HEAL_PERCENT) * caster.StsInterface.GetStatReductionModifier(BonusTypes.BONUSTYPES_EBONUS_OUT_HEAL_PERCENT);
                */

                damageInfo.ApplyDamageModifiers();
            }

            #region Application

            int pointsHealed = target.ReceiveHeal(caster, (ushort)damageInfo.Damage, damageInfo.HealHatredScale);

            if (pointsHealed == -1)
                return;

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(damageInfo.DisplayEntry); // 00 00 07 D D

            outl.WriteByte(damageInfo.CastPlayerSubID);
            outl.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
            outl.WriteByte(7);   //7    o 42

            outl.WriteZigZag((ushort)(pointsHealed));
            if (damageInfo.Damage > pointsHealed)
                outl.WriteZigZag((ushort)damageInfo.Damage - pointsHealed);
            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, null);

            #endregion
        }

        #endregion

        #region Damage

        private const float OFFHAND_DAMAGE_PEN = 0.9f;
        private const float OFFHAND_STAT_COEFF = 0.05f;
        private const int HEAL_CONTRIBUTION_CHANCE = 8;
        private const int OOG_HEAL_RENOWN_CHANCE = 25;

        public static void InflictDamage(AbilityDamageInfo damageInfo, byte level, Unit caster, Unit target)
        {
            float damageBonus = 0;
            float damageReduction = 1;

            caster.CbtInterface.RefreshCombatTimer();
            target.CbtInterface.OnAttacked(caster);

            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.AttackedTarget, damageInfo, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasAttacked, damageInfo, caster);

            #region Base Damage

            if (damageInfo.CastTimeDamageMult == 0)
                damageInfo.CastTimeDamageMult = 1.5f;

            damageInfo.Damage = damageInfo.GetDamageForLevel(level);

            #endregion

            caster.ModifyDamageOut(damageInfo);
            target.ModifyDamageIn(damageInfo);

            Creature creature = target.GetCreature();
            if (creature != null)
            {
                creature.CheckDamageCaster(caster, damageInfo);
            }

            if (damageInfo.DamageEvent > 0 || damageInfo.DamageType != DamageTypes.RawDamage)
            {
                #region Defense
                // Perform Block/Parry/Dodge/Evasion check
                if (damageInfo.DamageEvent > 0 || (!damageInfo.Undefendable && WasDefended(damageInfo, caster, target)))
                {
                    PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

                    outl.WriteUInt16(caster.Oid);
                    outl.WriteUInt16(target.Oid);
                    outl.WriteUInt16(damageInfo.DisplayEntry);

                    outl.WriteByte(0);
                    outl.WriteByte(damageInfo.DamageEvent);
                    outl.WriteByte(5);

                    outl.WriteByte(0);

                    target.DispatchPacketUnreliable(outl, true, caster);

                    caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasDefended, damageInfo, target);
                    target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DefendedAgainst, damageInfo, caster);

                    return;
                }

                #endregion

                if (damageInfo.PriStatMultiplier > 0.0f)
                {
                    damageInfo.Damage += caster.ItmInterface.GetWeaponDamage(damageInfo.WeaponMod) * damageInfo.PriStatMultiplier;
                }
                else
                {
                    damageInfo.Damage += caster.ItmInterface.GetWeaponDamage(damageInfo.WeaponMod) * damageInfo.WeaponDamageScale * damageInfo.CastTimeDamageMult;
                }

                if (damageInfo.StatUsed > 0)
                {
                    AddOffensiveStats(caster, damageInfo, 0.2f, false);
                    AddLinearMitigation(target, damageInfo, 0.2f, false);
                }

                #region Dealing/Receiving Event
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingDamage, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingDamage, damageInfo, caster);
                #endregion

                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ShieldPass, damageInfo, caster);

                CheckCriticalHit(caster, target, damageInfo);

                if (damageInfo.DamageType == DamageTypes.Physical)
                    CheckArmorReduction(caster, target, damageInfo, false);
                else
                    CheckResistanceReduction(caster, target, damageInfo, false);

                #region Phys/Mag multipliers

                damageBonus +=
                    caster.StsInterface.GetStatBonusModifier(damageInfo.StatUsed == 9 ? Stats.MagicPower : Stats.MeleePower)
                    + target.StsInterface.GetStatBonusModifier(damageInfo.StatUsed == 9 ? Stats.IncomingMagicDamage : Stats.IncomingMeleeDamage);
                damageReduction *= target.StsInterface.GetStatReductionModifier(damageInfo.StatUsed == 9 ? Stats.IncomingMagicDamage : Stats.IncomingMeleeDamage)
                    * caster.StsInterface.GetStatReductionModifier(damageInfo.StatUsed == 9 ? Stats.MagicPower : Stats.MeleePower);

                #endregion

                #region General Multipliers
                damageBonus += caster.StsInterface.GetStatBonusModifier(Stats.OutgoingDamagePercent) + target.StsInterface.GetStatBonusModifier(Stats.IncomingDamagePercent);
                damageReduction *= target.StsInterface.GetStatReductionModifier(Stats.IncomingDamagePercent) * caster.StsInterface.GetStatReductionModifier(Stats.OutgoingDamagePercent);

                damageInfo.DamageBonus += damageBonus;
                damageInfo.DamageReduction *= damageReduction;

                #endregion

                // Guard is separate because it needs to come after shielding
                target.BuffInterface.CheckGuard(damageInfo, caster);

                damageInfo.ApplyDamageModifiers();

                // Dealt/Received Event
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealtDamage, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivedDamage, damageInfo, caster);
            }

            if (damageInfo.IsAoE && target is Pet)
            {
                damageInfo.Damage *= 0.5f;
                damageInfo.Mitigation *= 0.5f;
            }

            #region Application

            if (!target.IsDead)
            {
                // Damage cap
                //  if (caster is Player)
                //  {
                //      int damageCap = Point2D.Lerp(550, 4000, caster.EffectiveLevel / 40f);
                //      damageInfo.Damage = Math.Min(damageCap, damageInfo.Damage);
                //  }

                PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

                damageOut.WriteUInt16(caster.Oid);
                damageOut.WriteUInt16(target.Oid);
                damageOut.WriteUInt16(damageInfo.DisplayEntry); // 00 00 07 D D

                damageOut.WriteByte(damageInfo.CastPlayerSubID);
                damageOut.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
                if (damageInfo.Absorption > 0)
                    damageOut.WriteByte(42);
                else damageOut.WriteByte(7);

                damageOut.WriteZigZag(-(ushort)damageInfo.Damage);
                if (damageInfo.Mitigation > 0)
                    damageOut.WriteZigZag((ushort)damageInfo.Mitigation);
                if (damageInfo.Absorption > 0)
                    damageOut.WriteZigZag((ushort)damageInfo.Absorption);
                damageOut.WriteByte(0);

                target.DispatchPacketUnreliable(damageOut, true, caster);
            }

            if (target.ReceiveDamage(caster, damageInfo))
            {
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.OnKill, damageInfo, target);

                damageInfo.WasLethalDamage = true;

                Pet pet = caster as Pet;
                Player plrCaster = pet != null ? pet.Owner : caster as Player;

                if (plrCaster != null)
                {
                    Player plrTarget = target as Player;

                    if (plrTarget != null)
                        SendDeathSpam(damageInfo, plrCaster, plrTarget);
                }
            }

            #endregion

            // 8: OnPostDealt/ReceivedDamage event (Tooth of Tzeentch, Shining Blade, Blurring Shock, Skull Thumper, etc)
            if (damageInfo.DamageType != DamageTypes.RawDamage)
            {
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectDamageDealt, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectDamageReceived, damageInfo, caster);
            }

            if (damageInfo.Damage > 0)
                target.AbtInterface.OnPlayerHit();

            if (!caster.CbtInterface.IsAttacking)
                caster.CbtInterface.IsAttacking = true;
        }

        public static void InflictAutoAttackDamage(EquipSlot slot, Unit caster, Unit target)
        {
            float damageBonus = 0;
            float damageReduction = 1;

            AbilityDamageInfo damageInfo = new AbilityDamageInfo { StatDamageScale = 1 };

            caster.SendAttackMovement(target);

            target.CbtInterface.OnAttacked(caster);
            caster.CbtInterface.RefreshCombatTimer();

            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.AttackedTarget, damageInfo, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasAttacked, damageInfo, caster);

            if (slot == EquipSlot.MAIN_HAND)
                damageInfo.StatUsed = 1;
            else if (slot == EquipSlot.RANGED_WEAPON)
                damageInfo.StatUsed = 8;
            else
            {
                damageInfo.StatUsed = 9;
                damageInfo.DamageType = DamageTypes.Elemental;
            }

            damageInfo.CastTimeDamageMult = caster.ItmInterface.GetAttackTime(slot) / 100f;

            target.ModifyDamageIn(damageInfo);

            Creature creature = target.GetCreature();
            if (creature != null)
            {
                creature.CheckDamageCaster(caster, damageInfo);
            }

            #region Defense
            // Perform Block/Parry/Dodge/Evasion check
            if (damageInfo.DamageEvent > 0 || WasDefended(damageInfo, caster, target))
            {
                PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

                outl.WriteUInt16(caster.Oid);
                outl.WriteUInt16(target.Oid);
                outl.WriteUInt16(0);

                outl.WriteByte(0);
                outl.WriteByte(damageInfo.DamageEvent);
                outl.WriteByte(0x11);

                outl.WriteByte(0);

                target.DispatchPacketUnreliable(outl, true, caster);

                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasDefended, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DefendedAgainst, damageInfo, caster);
            }

            #endregion

            else
            {

                if (damageInfo.PriStatMultiplier > 0.0f)
                {
                    damageInfo.Damage = (caster.ItmInterface.GetWeaponDamage(slot)) * damageInfo.PriStatMultiplier;
                }
                else
                {
                    damageInfo.Damage = (caster.ItmInterface.GetWeaponDamage(slot)) * damageInfo.CastTimeDamageMult;
                }

                //TODO : REMOVE BEFORE PRODUCTION
                if (target is Player)
                {
                    if (target.Name.Contains("Ikthaleon"))
                    {
                        damageInfo.Damage *= 0.05f;
                    }
                }


                if (damageInfo.StatUsed > 0)
                {
                    AddOffensiveStats(caster, damageInfo, 0.1f, false, true);
                    AddLinearMitigation(target, damageInfo, 0.1f, false);
                }

                #region Dealing/Receiving Event
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingDamage, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingDamage, damageInfo, caster);
                #endregion

                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ShieldPass, damageInfo, caster);

                CheckCriticalHit(caster, target, damageInfo);

                if (damageInfo.DamageType == DamageTypes.Physical)
                    CheckArmorReduction(caster, target, damageInfo, false);
                else
                    CheckResistanceReduction(caster, target, damageInfo, false);

                #region Phys/Mag multipliers

                damageBonus +=
                    caster.StsInterface.GetStatBonusModifier(damageInfo.StatUsed == 9 ? Stats.MagicPower : Stats.MeleePower)
                    + target.StsInterface.GetStatBonusModifier(damageInfo.StatUsed == 9 ? Stats.IncomingMagicDamage : Stats.IncomingMeleeDamage);
                damageReduction *= target.StsInterface.GetStatReductionModifier(damageInfo.StatUsed == 9 ? Stats.IncomingMagicDamage : Stats.IncomingMeleeDamage)
                    * caster.StsInterface.GetStatReductionModifier(damageInfo.StatUsed == 9 ? Stats.MagicPower : Stats.MeleePower);

                #endregion

                #region General Multipliers
                damageBonus += caster.StsInterface.GetStatBonusModifier(Stats.OutgoingDamagePercent) + caster.StsInterface.GetStatBonusModifier(Stats.AutoAttackDamage) + target.StsInterface.GetStatBonusModifier(Stats.IncomingDamagePercent);

                damageReduction *= target.StsInterface.GetStatReductionModifier(Stats.IncomingDamagePercent) * caster.StsInterface.GetStatReductionModifier(Stats.OutgoingDamagePercent);

                damageInfo.DamageBonus += damageBonus;
                damageInfo.DamageReduction *= damageReduction;

                #endregion

                if (caster is Creature && !(caster is Pet) && caster.Level > target.EffectiveLevel + 3)
                    damageInfo.DamageBonus += (caster.Level - 3 - target.EffectiveLevel) * 0.4f;

                // Guard is separate because it needs to come after shielding
                target.BuffInterface.CheckGuard(damageInfo, caster);

                damageInfo.ApplyDamageModifiers();

                // 6: DealtDamage event
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealtDamage, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivedDamage, damageInfo, caster);

                #region Application

                if (!target.IsDead)
                {
                    PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

                    damageOut.WriteUInt16(caster.Oid);
                    damageOut.WriteUInt16(target.Oid);
                    damageOut.WriteUInt16(0);

                    damageOut.WriteByte(0);
                    damageOut.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
                    if (damageInfo.Absorption > 0)
                        damageOut.WriteByte(42);
                    else damageOut.WriteByte(0x13);

                    damageOut.WriteZigZag(-(ushort)damageInfo.Damage);
                    if (damageInfo.Mitigation > 0)
                        damageOut.WriteZigZag((ushort)damageInfo.Mitigation);
                    if (damageInfo.Absorption > 0)
                        damageOut.WriteZigZag((ushort)damageInfo.Absorption);
                    damageOut.WriteByte(0);

                    target.DispatchPacketUnreliable(damageOut, true, caster);
                }

                if (target.ReceiveDamage(caster, damageInfo))
                {
                    caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.OnKill, damageInfo, target);

                    Pet pet = caster as Pet;
                    Player killer = pet != null ? pet.Owner : caster as Player;

                    if (killer != null && target is Player)
                    {
                        Player victim = (Player)target;


                        PacketOut Out = new PacketOut((byte)Opcodes.F_DEATHSPAM, 96);
                        WriteKillerDeathSpamInfo(Out, killer);
                        WriteVictimDeathSpamInfo(Out, victim);
                        if (pet == null)
                            WriteWeaponDeathSpamInfo(Out, killer, slot);
                        else WritePetDeathSpamInfo(Out, killer);

                        lock (Player._Players)
                        {
                            foreach (Player subPlayer in Player._Players)
                            {
                                if (subPlayer.Region == target.Region)
                                    subPlayer.SendCopy(Out);
                            }
                        }
                    }
                }

                #endregion

                // 8: OnPostDealt/ReceivedDamage event (Tooth of Tzeentch, Shining Blade, Blurring Shock, Skull Thumper, etc)
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectDamageDealt, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectDamageReceived, damageInfo, caster);
            }

            if (damageInfo.Damage > 0)
                target.AbtInterface.OnPlayerHit();

            if (!(caster is Player))
                return;

            if (slot == EquipSlot.MAIN_HAND && caster.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND) != null && caster.ItmInterface.GetItemInSlot(11).Info.Type != (byte)ItemTypes.ITEMTYPES_SHIELD && caster.ItmInterface.GetItemInSlot(11).Info.Type != (byte)ItemTypes.ITEMTYPES_CHARM)
            {
                if (StaticRandom.Instance.Next(1, 100) <= 45 + caster.StsInterface.GetBonusStat(Stats.OffhandProcChance))
                    InflictOffhandDamage(caster, target);
            }
        }

        public static void InflictOffhandDamage(Unit caster, Unit target)
        {
            float damageBonus = 0;
            float damageReduction = 1;

            AbilityDamageInfo damageInfo = new AbilityDamageInfo { StatUsed = 1, StatDamageScale = 1 };

            target.ModifyDamageIn(damageInfo);

            Creature creature = target.GetCreature();
            if (creature != null)
            {
                creature.CheckDamageCaster(caster, damageInfo);
            }

            #region Defense
            // Perform Block/Parry/Dodge/Evasion check
            if (damageInfo.DamageEvent > 0 || WasDefended(damageInfo, caster, target))
            {
                PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

                outl.WriteUInt16(caster.Oid);
                outl.WriteUInt16(target.Oid);
                outl.WriteUInt16(0);

                outl.WriteByte(0);
                outl.WriteByte(damageInfo.DamageEvent);
                outl.WriteByte(0x11);

                outl.WriteByte(0);

                target.DispatchPacketUnreliable(outl, true, caster);

                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.WasDefended, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DefendedAgainst, damageInfo, caster);

                return;
            }

            #endregion

            float wSpeed = caster.ItmInterface.GetAttackTime(EquipSlot.MAIN_HAND) * 0.01f;
            float ohWdps = caster.ItmInterface.GetWeaponDamage(EquipSlot.OFF_HAND);

            damageInfo.Damage = wSpeed * ohWdps * OFFHAND_DAMAGE_PEN;

            #region Offensive Stat

            // Handle this elsewhere.
            uint softcap = (uint)(50 + 25 * caster.EffectiveLevel);
            uint hardcap = (uint)(50 + 55 * caster.EffectiveLevel);

            var stat = (uint)caster.StsInterface.GetTotalStat((Stats)damageInfo.StatUsed);

            if (stat > hardcap)
                stat = hardcap;

            else if (stat > softcap)
                stat = softcap + ((stat - softcap) / 3);
            // End

            damageInfo.Damage += wSpeed * stat * OFFHAND_STAT_COEFF * OFFHAND_DAMAGE_PEN;

            #endregion

            #region Toughness Mitigation

            softcap = (uint)(50 + 25 * target.EffectiveLevel);
            hardcap = (uint)(50 + 55 * target.EffectiveLevel);

            // Cap toughness mitigation at foe's offensive stat value
            stat = (ushort)target.StsInterface.GetTotalStat(Stats.Toughness);

            if (stat > hardcap)
                stat = hardcap;

            else if (stat > softcap)
                stat = softcap + ((stat - softcap) / 3);

            damageInfo.Mitigation = wSpeed * stat * OFFHAND_STAT_COEFF * OFFHAND_DAMAGE_PEN;

            if (damageInfo.Mitigation >= damageInfo.Damage)
            {
                damageInfo.Mitigation = damageInfo.Damage - 1;
                damageInfo.Damage = 1;
            }
            else
                damageInfo.Damage -= damageInfo.Mitigation;

            #endregion

            if (caster.StsInterface.GetBonusStat(Stats.OffhandDamage) > 0)
            {
                damageInfo.Damage *= 1 + caster.StsInterface.GetBonusStat(Stats.OffhandDamage) * 0.01f;
                damageInfo.Mitigation *= 1 + caster.StsInterface.GetBonusStat(Stats.OffhandDamage) * 0.01f;
            }

            #region Dealing/Receiving Event
            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingDamage, damageInfo, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingDamage, damageInfo, caster);
            #endregion

            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ShieldPass, damageInfo, caster);

            // 4: Percentage modifiers.

            CheckCriticalHit(caster, target, damageInfo);

            if (damageInfo.DamageType == DamageTypes.Physical)
                CheckArmorReduction(caster, target, damageInfo, false);
            else
                CheckResistanceReduction(caster, target, damageInfo, false);

            #region Phys/Mag multipliers

            damageBonus +=
                caster.StsInterface.GetStatBonusModifier(damageInfo.StatUsed == 9 ? Stats.MagicPower : Stats.MeleePower)
                + target.StsInterface.GetStatBonusModifier(damageInfo.StatUsed == 9 ? Stats.IncomingMagicDamage : Stats.IncomingMeleeDamage);
            damageReduction *= target.StsInterface.GetStatReductionModifier(damageInfo.StatUsed == 9 ? Stats.IncomingMagicDamage : Stats.IncomingMeleeDamage)
                * caster.StsInterface.GetStatReductionModifier(damageInfo.StatUsed == 9 ? Stats.MagicPower : Stats.MeleePower);

            #endregion

            #region General Multipliers
            damageBonus += caster.StsInterface.GetStatBonusModifier(Stats.OutgoingDamagePercent) + caster.StsInterface.GetStatBonusModifier(Stats.AutoAttackDamage) + target.StsInterface.GetStatBonusModifier(Stats.IncomingDamagePercent);
            damageReduction *= target.StsInterface.GetStatReductionModifier(Stats.IncomingDamagePercent) * caster.StsInterface.GetStatReductionModifier(Stats.OutgoingDamagePercent);

            damageInfo.DamageBonus += damageBonus;
            damageInfo.DamageReduction *= damageReduction;

            #endregion

            // Guard is separate because it needs to come after shielding
            target.BuffInterface.CheckGuard(damageInfo, caster);

            damageInfo.ApplyDamageModifiers();

            // 6: DealtDamage event (Shielding)

            #region Dealt/Received Event
            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealtDamage, damageInfo, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivedDamage, damageInfo, caster);
            #endregion

            #region Application

            if (!target.IsDead)
            {
                PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

                damageOut.WriteUInt16(caster.Oid);
                damageOut.WriteUInt16(target.Oid);
                damageOut.WriteUInt16(0);

                damageOut.WriteByte(0);
                damageOut.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
                if (damageInfo.Absorption > 0)
                    damageOut.WriteByte(42);
                else damageOut.WriteByte(0x13);

                damageOut.WriteZigZag(-(ushort)damageInfo.Damage);
                if (damageInfo.Mitigation > 0)
                    damageOut.WriteZigZag((ushort)damageInfo.Mitigation);
                if (damageInfo.Absorption > 0)
                    damageOut.WriteZigZag((ushort)damageInfo.Absorption);
                damageOut.WriteByte(0);

                target.DispatchPacketUnreliable(damageOut, true, caster);
            }

            if (target.ReceiveDamage(caster, damageInfo))
            {
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.OnKill, damageInfo, target);

                Pet pet = caster as Pet;
                Player killer = pet != null ? pet.Owner : caster as Player;

                if (killer != null && target is Player)
                {
                    Player victim = (Player)target;

                    PacketOut Out = new PacketOut((byte)Opcodes.F_DEATHSPAM, 96);
                    WriteKillerDeathSpamInfo(Out, killer);
                    WriteVictimDeathSpamInfo(Out, victim);
                    if (pet == null)
                        WriteWeaponDeathSpamInfo(Out, killer, EquipSlot.OFF_HAND);
                    else WritePetDeathSpamInfo(Out, killer);



                    lock (Player._Players)
                    {
                        foreach (Player subPlayer in Player._Players)
                        {
                            if (subPlayer.Region == target.Region)
                                subPlayer.SendCopy(Out);
                        }
                    }
                }
            }
            #endregion

            // 8: OnPostDealt/ReceivedDamage event (Tooth of Tzeentch, Shining Blade, Blurring Shock, Skull Thumper, etc)

            #region PostDamage Event
            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectDamageDealt, damageInfo, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectDamageReceived, damageInfo, caster);
            #endregion
        }

        public static void InflictGuardDamage(Unit attacker, Player receiver, ushort entry, AbilityDamageInfo originalDamage, float casterDamageSplitFactor)
        {
            AbilityDamageInfo tempDmg = new AbilityDamageInfo { Entry = 0, Damage = originalDamage.Damage };

            tempDmg.Damage *= casterDamageSplitFactor;
            tempDmg.Mitigation *= casterDamageSplitFactor;

            byte defenseEvent = 0;

            receiver.CbtInterface.RefreshCombatTimer();

            originalDamage.TransferFactor = 2;

            // 11/10/16 Azarael - Cannot block or parry damage that was originally undefendable with Guard
            if (!originalDamage.Undefendable)
            {
                int offensiveStat, defensiveStat;

                #region GetStat
                switch (originalDamage.StatUsed)
                {
                    case 1: // Strength and Weaponskill
                        {
                            offensiveStat = Math.Max(1, (int)attacker.StsInterface.GetTotalStat(Stats.Strength));
                            defensiveStat = Math.Max(1, (int)receiver.StsInterface.GetTotalStat(Stats.WeaponSkill));
                        }
                        break;
                    case 8: // Ballistic and Initiative
                        {
                            offensiveStat = Math.Max(1, (int)attacker.StsInterface.GetTotalStat(Stats.BallisticSkill));
                            defensiveStat = Math.Max(1, (int)receiver.StsInterface.GetTotalStat(Stats.Initiative));
                        }
                        break;
                    case 9: // Intelligence and Willpower
                        {
                            offensiveStat = Math.Max(1, (int)attacker.StsInterface.GetTotalStat(Stats.Intelligence));
                            defensiveStat = Math.Max(1, (int)receiver.StsInterface.GetTotalStat(Stats.Willpower));
                        }
                        break;
                    default:
                        {
                            offensiveStat = Math.Max(1, (int)attacker.StsInterface.GetTotalStat(Stats.Strength));
                            defensiveStat = Math.Max(1, (int)receiver.StsInterface.GetTotalStat(Stats.WeaponSkill));
                        }
                        break;
                }
                #endregion
                //try a block
                if (receiver.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND) != null && receiver.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND).Info.Type == 5)
                {
                    double block = (receiver.ItmInterface.GetItemInSlot((ushort)EquipSlot.OFF_HAND).Info.Armor / (receiver.EffectiveLevel * 7.5 + 50) * 20);

                    //Contestion based on offensive stat. This gets added to make it harder to actually do a defensive event, without actually contesting it directly above.
                    //This should mimic the live formula.
                    double removedBlockDefense = (((offensiveStat) * 100) / (((attacker.EffectiveLevel * 7.5) + 50) * 7.5));

                    double baseBlockRoll = 0d;
                    baseBlockRoll += removedBlockDefense;

                    block += receiver.StsInterface.GetTotalStat(Stats.Block) - attacker.StsInterface.GetStatLinearModifier(Stats.BlockStrikethrough);
                    double finalBlockRoll = (StaticRandom.Instance.NextDouble() * (100d + baseBlockRoll));

                    if (block >= finalBlockRoll)
                    {
                        defenseEvent = (byte)CombatEvent.COMBATEVENT_BLOCK;
                    }
                }

                //try a parry
                double secondaryDefense = (((defensiveStat) * 100) / ((receiver.EffectiveLevel * 7.5 + 50) * 7.5));
                double removedDefense = (((offensiveStat) * 100) / (((attacker.EffectiveLevel * 7.5) + 50) * 7.5));

                double baseRoll = 0d;
                baseRoll += removedDefense;

                secondaryDefense += receiver.StsInterface.GetTotalStat(Stats.Parry) - attacker.StsInterface.GetStatLinearModifier(Stats.ParryStrikethrough);
                secondaryDefense = (secondaryDefense * attacker.StsInterface.GetStatPercentageModifier(Stats.ParryStrikethrough));
                double finalRoll = (StaticRandom.Instance.NextDouble() * (100d + baseRoll));

                if (secondaryDefense >= finalRoll)
                {
                    defenseEvent = (byte)CombatEvent.COMBATEVENT_PARRY;
                }
                if (defenseEvent != 0)
                {
                    tempDmg.DamageEvent = defenseEvent;
                    receiver.CbtInterface.SetDefenseTimer(defenseEvent);
                    receiver.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DefendedAgainst, tempDmg, receiver);
                }
            }

            receiver.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ShieldPass, tempDmg, receiver);

            #region Application

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);

            outl.WriteUInt16(receiver.Oid);
            outl.WriteUInt16(receiver.Oid);
            outl.WriteUInt16(entry);

            outl.WriteByte(4); // always seems to be 5 for Guard's hits
            outl.WriteByte(defenseEvent); // DAMAGE EVENT
            outl.WriteByte(tempDmg.Absorption > 0 ? (byte)111 : (byte)7);

            if (defenseEvent != 0)
                outl.WriteByte(0);

            else
            {
                outl.WriteZigZag(-(ushort)tempDmg.Damage);
                outl.WriteByte(0);
                if (receiver.ReceiveDamage(receiver, tempDmg) && receiver.CbtInterface.IsPvp && attacker is Player)
                {
                    PacketOut Out = new PacketOut((byte)Opcodes.F_DEATHSPAM, 96);

                    Out.WriteByte((byte)("Guard damage".Length + 1));
                    Out.WriteHexStringBytes("6504");
                    Out.WriteByte(receiver.Realm == Realms.REALMS_REALM_ORDER ? (byte)2 : (byte)1); // faction
                    Out.WriteByte(0);
                    Out.WriteStringBytes("Guard damage");
                    Out.WriteByte(0);

                    Out.WriteByte((byte)(receiver.Name.Length + 1)); // len for weapon name
                    Out.WriteHexStringBytes("4207");
                    Out.WriteByte(receiver.Realm == Realms.REALMS_REALM_ORDER ? (byte)2 : (byte)1); // faction
                    Out.WriteByte(1);
                    Out.WriteStringBytes(receiver.Name);
                    Out.WriteByte(0);

                    string areaName = receiver.GetAreaName();

                    Out.WriteByte((byte)(areaName.Length + 1));
                    Out.WriteHexStringBytes("B0FE0000");
                    Out.WriteUInt16R(0);
                    Out.WriteByte(0);
                    Out.WriteByte(0); // attack type
                    Out.WriteByte(1); // len for weapon name
                    Out.WriteStringBytes(areaName);
                    Out.WriteByte(0);
                    Out.WriteStringBytes("");
                    Out.WriteByte(0);
                    Out.WriteByte(0);

                    lock (Player._Players)
                    {
                        foreach (Player subPlayer in Player._Players)
                        {
                            if (subPlayer.Region == receiver.Region)
                                subPlayer.SendPacket(Out);
                        }
                    }
                }

                Player attackerPlr = attacker as Player;
                attackerPlr?.ScnInterface.Scenario?.OnGuardHit((Player)attacker, (uint)tempDmg.Damage, (Player)receiver);
            }

            receiver.DispatchPacketUnreliable(outl, true, null);

            #endregion
        }

        public static void HealTarget(AbilityDamageInfo damageInfo, byte level, Unit caster, Unit target, int criticalNumerator=0)
        {
            #region Base Damage

            if (damageInfo.CastTimeDamageMult == 0)
                damageInfo.CastTimeDamageMult = 1.5f;

            damageInfo.Damage = damageInfo.GetDamageForLevel(level);

            #endregion

            target.ModifyHealOut(damageInfo);
            target.ModifyHealIn(damageInfo);

            if (damageInfo.DamageType != DamageTypes.RawHealing)
            {
                // Neutralize effect of AM/Shaman mechanic against self
                if (damageInfo.UseItemStatTotal && caster == target)
                {
                    damageInfo.UseItemStatTotal = false;

                    /*if (damageInfo.Entry == 9258) // Funnel Essence
                    {
                        damageInfo.DamageBonus -= 0.25f;
                        //damageInfo.DamageReduction *= 0.8f;
                    }
                    else
                        damageInfo.DamageReduction *= 0.6f; // to counteract 60% faster cast, removed risk penalty*/
                }

                AddOffensiveStats(caster, damageInfo, 0.2f, false);

                #region CriticalHeal

                int rand = StaticRandom.Instance.Next(criticalNumerator, 100);

                int chanceToBeCrit = 10 + damageInfo.CriticalHitRate + caster.StsInterface.GetTotalStat(Stats.CriticalHitRate) + caster.StsInterface.GetTotalStat(Stats.HealCritRate);
                if (rand <= chanceToBeCrit)
                {
                    damageInfo.Damage *= 1.35f + (float)StaticRandom.Instance.NextDouble() * 0.2f;

                    damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_ABILITY_CRITICAL;
                }
                else
                    damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_HIT;

                #endregion

                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingHeal, damageInfo, target);

                damageInfo.DamageBonus += caster.StsInterface.GetStatBonusModifier(Stats.OutgoingHealPercent) + target.StsInterface.GetStatBonusModifier(Stats.IncomingHealPercent);
                damageInfo.DamageReduction *= target.StsInterface.GetStatReductionModifier(Stats.IncomingHealPercent) * caster.StsInterface.GetStatReductionModifier(Stats.OutgoingHealPercent);

                damageInfo.ApplyDamageModifiers();

                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingHeal, damageInfo, caster);
            }

            else
            {
                damageInfo.ApplyDamageModifiers();
            }

            #region Application

            int pointsHealed = target.ReceiveHeal(caster, (ushort)damageInfo.Damage, damageInfo.HealHatredScale);

            if (pointsHealed == -1)
                return;

            if (pointsHealed > 0)
            {
                AwardOutOfGroupHealing(caster, target, pointsHealed, 40, 8);
            }

            damageInfo.Mitigation = damageInfo.Damage - pointsHealed;
            damageInfo.Damage = pointsHealed;

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(damageInfo.DisplayEntry); // 00 00 07 D D

            outl.WriteByte(damageInfo.CastPlayerSubID);
            outl.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
            outl.WriteByte(7);   //7    o 42

            outl.WriteZigZag((ushort)damageInfo.Damage);
            if (damageInfo.Mitigation > 0)
                outl.WriteZigZag((ushort)damageInfo.Mitigation);
            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, caster);

            #endregion

            if (damageInfo.DamageType != DamageTypes.RawHealing)
            {
                caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectHealDealt, damageInfo, target);
                target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectHealReceived, damageInfo, caster);
            }
        }

        public static void LifeSteal(AbilityDamageInfo damageInfo, byte level, Unit caster, Unit target)
        {
            AddOffensiveStats(caster, damageInfo, 0.2f, false);

            #region CriticalHeal

            int rand = StaticRandom.Instance.Next(0, 100);

            int chanceToBeCrit = 10 + damageInfo.CriticalHitRate + caster.StsInterface.GetTotalStat(Stats.CriticalHitRate) + caster.StsInterface.GetTotalStat(Stats.HealCritRate);
            if (rand <= chanceToBeCrit)
            {
                damageInfo.Damage *= 1.35f + (float)StaticRandom.Instance.NextDouble() * 0.2f;

                damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_ABILITY_CRITICAL;
            }
            else
                damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_HIT;

            #endregion

            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DealingHeal, damageInfo, target);

            damageInfo.DamageBonus += caster.StsInterface.GetStatBonusModifier(Stats.OutgoingHealPercent) + target.StsInterface.GetStatBonusModifier(Stats.IncomingHealPercent);
            damageInfo.DamageReduction *= target.StsInterface.GetStatReductionModifier(Stats.IncomingHealPercent) * caster.StsInterface.GetStatReductionModifier(Stats.OutgoingHealPercent);

            damageInfo.ApplyDamageModifiers();

            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.ReceivingHeal, damageInfo, caster);

            #region Application

            int pointsHealed = target.ReceiveHeal(caster, (ushort)damageInfo.Damage, damageInfo.HealHatredScale);

            if (pointsHealed == -1)
                return;

            damageInfo.Mitigation = damageInfo.Damage - pointsHealed;
            damageInfo.Damage = pointsHealed;

            PacketOut outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

            outl.WriteUInt16(caster.Oid);
            outl.WriteUInt16(target.Oid);
            outl.WriteUInt16(damageInfo.DisplayEntry); // 00 00 07 D D

            outl.WriteByte(damageInfo.CastPlayerSubID);
            outl.WriteByte(damageInfo.DamageEvent); // DAMAGE EVENT
            outl.WriteByte(7);   //7    o 42

            outl.WriteZigZag((ushort)damageInfo.Damage);
            if (damageInfo.Mitigation > 0)
                outl.WriteZigZag((ushort)damageInfo.Mitigation);
            outl.WriteByte(0);

            target.DispatchPacketUnreliable(outl, true, caster);

            #endregion

            caster.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectHealDealt, damageInfo, target);
            target.BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.DirectHealReceived, damageInfo, caster);
        }

        public static int RawLifeSteal(ushort healValue, ushort entry, byte componentId, Unit caster, Unit target)
        {
            //healValue = (ushort) (healValue * caster.StsInterface.GetStatReductionModifier(Stats.LifestealPercent));

            int pointsHealed = target.ReceiveHeal(caster, healValue);

            if (pointsHealed > -1)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 18);
                Out.WriteUInt16(caster.Oid);
                Out.WriteUInt16(target.Oid);
                Out.WriteUInt16(entry);
                Out.WriteByte(componentId);
                Out.WriteByte(0);
                Out.WriteByte(0x7);
                Out.WriteZigZag(pointsHealed);
                Out.WriteZigZag(healValue - pointsHealed);
                Out.WriteByte(0);

                target.DispatchPacketUnreliable(Out, true, caster);
            }

            return pointsHealed;
        }

        #endregion

        private static void SendDeathSpam(AbilityDamageInfo damageInfo, Player killer, Player victim)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_DEATHSPAM, 96);

            WriteKillerDeathSpamInfo(Out, killer);
            WriteVictimDeathSpamInfo(Out, victim);

            string areaName = killer.GetAreaName();

            Out.WriteByte((byte)(areaName.Length + 1));
            Out.WriteHexStringBytes("B0FE0000");
            Out.WriteUInt16R(damageInfo.DisplayEntry);
            Out.WriteHexStringBytes("0001");
            Out.WriteByte(1); // len for weapon name
            Out.WriteStringBytes(areaName);
            Out.WriteByte(0);
            //Out.WriteStringBytes("Save The Queen");
            Out.WriteByte(0);
            Out.WriteByte(0);


            lock (Player._Players)
            {
                foreach (Player subPlayer in Player._Players)
                {
                    if (subPlayer.Region == victim.Region)
                        subPlayer.SendCopy(Out);
                }
            }
        }

        private static void WriteKillerDeathSpamInfo(PacketOut Out, Player killer)
        {
            Out.WriteByte((byte)(killer.Info.Name.Length + 1));
            Out.WriteHexStringBytes("6504");
            Out.WriteByte(killer.Info.Realm == (byte)Realms.REALMS_REALM_ORDER ? (byte)1 : (byte)2); // faction
            Out.WriteByte(0);
            Out.WriteStringBytes(killer.Info.Name);
            Out.WriteByte(0);
        }

        private static void WriteVictimDeathSpamInfo(PacketOut Out, Player victim)
        {
            Out.WriteByte((byte)(victim.Info.Name.Length + 1));
            Out.WriteHexStringBytes("4207");
            Out.WriteByte(victim.Info.Realm == (byte)Realms.REALMS_REALM_ORDER ? (byte)2 : (byte)1); // faction
            Out.WriteByte(1);
            Out.WriteStringBytes(victim.Info.Name);
            Out.WriteByte(0);
        }

        private static void WriteWeaponDeathSpamInfo(PacketOut Out, Player plrCaster, EquipSlot slot)
        {
            Item_Info wepInfo = plrCaster.ItmInterface.GetItemInSlot((ushort)slot).Info;

            string areaName = plrCaster.GetAreaName();

            Out.WriteByte((byte)(areaName.Length + 1));
            Out.WriteHexStringBytes("B0FE0000");
            Out.WriteUInt16R(0);
            Out.WriteByte(0);
            Out.WriteByte(wepInfo.Type); // attack type
            Out.WriteByte((byte)(wepInfo.Name.Length + 1)); // len for weapon name
            Out.WriteStringBytes(areaName);
            Out.WriteByte(0);
            Out.WriteStringBytes(wepInfo.Name);
            Out.WriteByte(0);
            Out.WriteByte(0);
        }

        private static void WritePetDeathSpamInfo(PacketOut Out, Player killer)
        {
            string areaName = killer.GetAreaName();

            string name;
            byte type = 0;

            switch ((CareerLine)killer.Info.CareerLine)
            {
                case CareerLine.CAREERLINE_ENGINEER:
                    name = "Turret";
                    type = 9;
                    break;
                case CareerLine.CAREERLINE_MAGUS:
                    name = "Daemon";
                    type = 1;
                    break;
                case CareerLine.CAREERLINE_SQUIG_HERDER:
                    name = "Squig";
                    type = 9;
                    break;
                case CareerLine.CAREERLINE_WHITELION:
                    name = "War Lion";
                    type = 1;
                    break;
                default:
                    name = "pet";
                    break;
            }

            Out.WriteByte((byte)(areaName.Length + 1));
            Out.WriteHexStringBytes("B0FE0000");
            Out.WriteUInt16R(0);
            Out.WriteByte(0);
            Out.WriteByte(type); // attack type
            Out.WriteByte((byte)(name.Length + 1)); // len for weapon name
            Out.WriteStringBytes(areaName);
            Out.WriteByte(0);
            Out.WriteStringBytes(name);
            Out.WriteByte(0);
            Out.WriteByte(0);
        }

        #region Calculations

        private static void AddOffensiveStats(Unit caster, AbilityDamageInfo damageInfo, float coefficient, bool toPrecalc, bool bIsAutoAttack = false)
        {
            uint softcap = (uint)(50 + 25 * caster.EffectiveLevel);
            uint hardcap = (uint)(50 + 55 * caster.EffectiveLevel);

            float stat = caster.StsInterface.GetTotalStat((Stats)damageInfo.StatUsed);

            if (damageInfo.UseItemStatTotal)
                stat = Math.Max(stat, caster.StsInterface.GetBaseStat(damageInfo.StatUsed) + (caster.StsInterface.ItemStatTotal * 0.7f) - caster.StsInterface.GetReducedStat((Stats)damageInfo.StatUsed));

#if DEBUG
            if (caster is Player && damageInfo.UseItemStatTotal)
                ((Player)caster).SendClientMessage($"Using total item stat factor contribution of {caster.StsInterface.GetBaseStat(damageInfo.StatUsed) + (caster.StsInterface.ItemStatTotal * 0.7f)} instead of {(damageInfo.StatUsed == 3 ? "Willpower" : "Intelligence")} contribution of {caster.StsInterface.GetTotalStat((Stats)damageInfo.StatUsed)}.");
#endif

            if (stat > hardcap)
                stat = hardcap;

            else if (stat > softcap)
                stat = softcap + ((stat - softcap) / 3);

            // Power stats bypass the caps - 1 power = 1 stat
            switch (damageInfo.StatUsed)
            {
                case 1:
                    stat += (uint)caster.StsInterface.GetBonusStat(Stats.MeleePower);
                    break;
                case 3:
                    stat += (uint)caster.StsInterface.GetBonusStat(Stats.HealingPower);
                    break;
                case 8:
                    stat += (uint)caster.StsInterface.GetBonusStat(Stats.RangedPower);
                    break;
                case 9:
                    stat += (uint)caster.StsInterface.GetBonusStat(Stats.MagicPower);
                    break;
            }

            if (damageInfo.PriStatMultiplier > 0.0f)
            {
                if (toPrecalc)
                    damageInfo.PrecalcDamage += (stat / 5) * damageInfo.PriStatMultiplier;
                else
                    damageInfo.Damage += (stat / 5) * damageInfo.PriStatMultiplier;
            }
            else
            {
                if (toPrecalc)
                    damageInfo.PrecalcDamage += stat * coefficient * damageInfo.StatDamageScale * damageInfo.CastTimeDamageMult;
                else
                    damageInfo.Damage += stat * coefficient * damageInfo.StatDamageScale * damageInfo.CastTimeDamageMult;
            }
        }

        private static void AddLinearMitigation(Unit target, AbilityDamageInfo damageInfo, float coefficient, bool toPrecalc)
        {
            uint softcap = (uint)(50 + 25 * target.EffectiveLevel);
            uint hardcap = (uint)(50 + 55 * target.EffectiveLevel);

            var stat = (uint)target.StsInterface.GetTotalStat(Stats.Toughness);

            if (stat > hardcap)
                stat = hardcap;

            else if (stat > softcap)
                stat = softcap + (stat - softcap) / 3;
            if (damageInfo.PriStatMultiplier > 0.0f)
            {
                if (toPrecalc)
                {
                    damageInfo.PrecalcMitigation = (stat / 5) * damageInfo.PriStatMultiplier;
                    if (damageInfo.PrecalcMitigation >= damageInfo.PrecalcDamage)
                    {
                        damageInfo.PrecalcMitigation = damageInfo.PrecalcDamage - 1;
                        damageInfo.PrecalcDamage = 1;
                    }
                    else
                        damageInfo.PrecalcDamage -= damageInfo.PrecalcMitigation;
                }
                else
                {
                    damageInfo.Mitigation = (stat / 5) * damageInfo.PriStatMultiplier;

                    if (damageInfo.Mitigation >= damageInfo.Damage)
                    {
                        damageInfo.Mitigation = damageInfo.Damage - 1;
                        damageInfo.Damage = 1;
                    }
                    else
                        damageInfo.Damage -= damageInfo.Mitigation;
                }
            }
            else
            {
                if (toPrecalc)
                {
                    damageInfo.PrecalcMitigation = stat * coefficient * damageInfo.StatDamageScale * damageInfo.CastTimeDamageMult;

                    if (damageInfo.PrecalcMitigation >= damageInfo.PrecalcDamage)
                    {
                        damageInfo.PrecalcMitigation = damageInfo.PrecalcDamage - 1;
                        damageInfo.PrecalcDamage = 1;
                    }
                    else
                        damageInfo.PrecalcDamage -= damageInfo.PrecalcMitigation;
                }

                else
                {
                    damageInfo.Mitigation = stat * coefficient * damageInfo.StatDamageScale * damageInfo.CastTimeDamageMult;

                    if (damageInfo.Mitigation >= damageInfo.Damage)
                    {
                        damageInfo.Mitigation = damageInfo.Damage - 1;
                        damageInfo.Damage = 1;
                    }
                    else
                        damageInfo.Damage -= damageInfo.Mitigation;
                }
            }
        }

        private static void CheckCriticalHit(Unit caster, Unit target, AbilityDamageInfo damageInfo)
        {
            if (damageInfo.DamageType == DamageTypes.RawDamage || damageInfo.NoCrits || target.StsInterface.GetTotalStat(Stats.Initiative) == 0)
                return;

            int rand = StaticRandom.Instance.Next(0, 100);

            // Basic
            int chanceToBeCrit = (int)((caster.EffectiveLevel * 7.5f + 50f) / 10f / target.StsInterface.GetTotalStat(Stats.Initiative) * 100f);

            // Add from stats
            chanceToBeCrit += damageInfo.CriticalHitRate + caster.StsInterface.GetTotalStat(Stats.CriticalHitRate) - target.StsInterface.GetTotalStat(Stats.CriticalHitRateReduction);

            switch (damageInfo.StatUsed)
            {
                case 1:
                    chanceToBeCrit += caster.StsInterface.GetTotalStat(Stats.MeleeCritRate);
                    break;
                case 8:
                    chanceToBeCrit += caster.StsInterface.GetTotalStat(Stats.RangedCritRate);
                    break;
                case 9:
                    chanceToBeCrit += caster.StsInterface.GetTotalStat(Stats.MagicCritRate);
                    break;
                //Leonine Frenzy 9194 - primary stat
                case 4:
                    chanceToBeCrit += caster.StsInterface.GetTotalStat(Stats.CriticalHitRateReduction);
                    break;
                //Leonine Frenzy 9194 - alternate stat
                case 5:
                    chanceToBeCrit += caster.StsInterface.GetTotalStat(Stats.CriticalHitRateReduction);
                    break;
            }

            if (rand <= chanceToBeCrit)
            {
                float critDmgMult = 1.35f + (float)StaticRandom.Instance.NextDouble() * 0.2f + damageInfo.CriticalHitDamageBonus + (caster.StsInterface.GetTotalStat(Stats.CriticalDamage) * 0.01f) + (target.StsInterface.GetTotalStat(Stats.CriticalDamageTaken) * 0.01f);

                if (critDmgMult > 3.5f)
                    (caster as Player)?.SendClientMessage("Suspiciously high critical damage multiplier of " + (critDmgMult * 100) + "%.", ChatLogFilters.CHATLOGFILTERS_SHOUT);
                damageInfo.Damage *= critDmgMult;
                damageInfo.Mitigation *= critDmgMult;

                damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_ABILITY_CRITICAL;
            }
            else
                damageInfo.DamageEvent = (byte)CombatEvent.COMBATEVENT_HIT;

        }

        private static void CheckArmorReduction(Unit caster, Unit target, AbilityDamageInfo damageInfo, bool toPrecalc)
        {
            float damageTypeResistance;

            StatsInterface casterStats = caster.StsInterface;
            StatsInterface targetStats = target.StsInterface;

            float secondaryStat = casterStats.GetTotalStat(Stats.WeaponSkill);
            float bonusStat = (casterStats.GetBonusStat(Stats.ArmorPenetration) - casterStats.GetReducedStat(Stats.ArmorPenetration)) * 0.01f;
            float pen = Math.Min(1f, secondaryStat / (7.5f * caster.EffectiveLevel + 50f) * 0.25f + bonusStat);
            float penRes = targetStats.GetBonusStat(Stats.ArmorPenetrationReduction) * 0.01f;

            if (penRes > pen)
                pen = 0;
            else pen -= penRes;

            int originalResistance = targetStats.GetTotalStat(Stats.Armor) - damageInfo.GetArmorPenetrationForLevel(caster.EffectiveLevel);

            if (originalResistance <= 0)
                damageTypeResistance = 0;
            else
            {
                damageTypeResistance = originalResistance / (caster.EffectiveLevel * 44f) * 0.4f;
                damageTypeResistance *= 1f - pen;
                damageTypeResistance *= 1f - damageInfo.ArmorResistPenFactor;

                if (damageTypeResistance > 0.75f) //puts in hard cap for physical mitigation of 75%
                    damageTypeResistance = 0.75f;
            }

            if (toPrecalc)
            {
                damageInfo.PrecalcMitigation += (ushort)(damageInfo.PrecalcDamage * damageTypeResistance);
                damageInfo.PrecalcDamage -= (ushort)(damageInfo.PrecalcDamage * damageTypeResistance);
            }

            else
            {
                damageInfo.Mitigation += (ushort)(damageInfo.Damage * damageTypeResistance);
                damageInfo.Damage -= (ushort)(damageInfo.Damage * damageTypeResistance);
            }
        }

        private static void CheckResistanceReduction(Unit caster, Unit target, AbilityDamageInfo damageInfo, bool toPrecalc)
        {
            short originalResistance = target.StsInterface.GetTotalStat((Stats)(13 + damageInfo.DamageType)); // 14 Spirit, 15 Elemental, 16 Corporeal.

            if (originalResistance == 0)
                return;

            float damageTypeResistance = (originalResistance / (caster.EffectiveLevel * 8.4f)) * 0.2f;
            if (damageTypeResistance > 0.4)
                damageTypeResistance = ((originalResistance / (caster.EffectiveLevel * 8.4f)) * 0.2f - 0.4f) / 3.0f + 0.4f;
            damageTypeResistance *= 1f - damageInfo.ArmorResistPenFactor;
            if (damageTypeResistance > 0.75f)
                damageTypeResistance = 0.75f;

            if (toPrecalc)
            {
                damageInfo.PrecalcMitigation += (ushort)(damageInfo.PrecalcDamage * damageTypeResistance);
                damageInfo.PrecalcDamage -= (ushort)(damageInfo.PrecalcDamage * damageTypeResistance);
            }

            else
            {
                damageInfo.Mitigation += (ushort)(damageInfo.Damage * damageTypeResistance);
                damageInfo.Damage -= (ushort)(damageInfo.Damage * damageTypeResistance);
            }
        }

        #endregion
    }
}

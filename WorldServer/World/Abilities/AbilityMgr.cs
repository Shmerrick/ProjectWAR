using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;

namespace WorldServer.World.Abilities
{
    [Service(typeof(WorldMgr))]
    public static class AbilityMgr
    {
        #region Ability System Setup

        // Abilities
        public static Dictionary<ushort, AbilityInfo> NewAbilityVolatiles = new Dictionary<ushort, AbilityInfo>();
        public static Dictionary<ushort, List<AbilityCommandInfo>> AbilityCommandInfos = new Dictionary<ushort, List<AbilityCommandInfo>>();

        // Modifiers
        public static Dictionary<ushort, List<AbilityModifier>> AbilityPreCastModifiers = new Dictionary<ushort, List<AbilityModifier>>();
        public static Dictionary<ushort, List<AbilityModifier>> AbilityModifiers = new Dictionary<ushort, List<AbilityModifier>>();
        public static Dictionary<ushort, List<AbilityModifier>> AbilityDelayedModifiers = new Dictionary<ushort, List<AbilityModifier>>();
        public static Dictionary<ushort, List<AbilityModifier>> BuffModifiers = new Dictionary<ushort, List<AbilityModifier>>();

        // Buffs
        public static Dictionary<ushort, BuffInfo> BuffInfos = new Dictionary<ushort, BuffInfo>();
        public static Dictionary<ushort, List<BuffCommandInfo>> BuffCommandInfos = new Dictionary<ushort, List<BuffCommandInfo>>();
        
        // Knockback
        public static Dictionary<ushort, List<AbilityKnockbackInfo>> KnockbackInfos = new Dictionary<ushort, List<AbilityKnockbackInfo>>(); 

        // Extra Damage Info (type-2 damage)
        public static Dictionary<ushort, List<List<AbilityDamageInfo>>> ExtraDamage = new Dictionary<ushort, List<List<AbilityDamageInfo>>>();

        // Career abilities
        public static List<AbilityInfo>[] CareerAbilities = new List<AbilityInfo>[24];

        public static void ReloadAbilities()
        {
            NewAbilityVolatiles.Clear();
            AbilityCommandInfos.Clear();
            AbilityPreCastModifiers.Clear();
            AbilityModifiers.Clear();
            AbilityDelayedModifiers.Clear();

            BuffInfos.Clear();
            BuffCommandInfos.Clear();
            BuffModifiers.Clear();

            KnockbackInfos.Clear();

            ExtraDamage.Clear();

            for(byte i=0;i<24;++i)
                CareerAbilities[i].Clear();

            LoadNewAbilityInfo();
        }

        [LoadingFunction(true)]
        public static void LoadNewAbilityInfo()
        {
            Log.Info("AbilityMgr", "Loading New Ability Info...");

            IObjectDatabase db = WorldMgr.Database;

            #region Database

            List<DBAbilityInfo> dbAbilities = (List<DBAbilityInfo>) db.SelectAllObjects<DBAbilityInfo>();

            List<AbilityInfo> abVolatiles = AbilityInfo.Convert(dbAbilities);
            Dictionary<ushort, AbilityConstants> abConstants = AbilityConstants.Convert(dbAbilities).ToDictionary(key => key.Entry);
            List<AbilityDamageInfo> abDmgHeals = AbilityDamageInfo.Convert(db.SelectAllObjects<DBAbilityDamageInfo>().OrderBy(dmg => dmg.ParentCommandID).ThenBy(dmg => dmg.ParentCommandSequence).ToList());
            List<AbilityCommandInfo> abCommands = AbilityCommandInfo.Convert(db.SelectAllObjects<DBAbilityCommandInfo>().OrderBy(cmd => cmd.CommandID).ToList());

            IList<AbilityModifierCheck> abChecks = db.SelectAllObjects<AbilityModifierCheck>().OrderBy(check => check.ID).ToList();
            IList<AbilityModifierEffect> abMods = db.SelectAllObjects<AbilityModifierEffect>().OrderBy(mod => mod.Sequence).ToList();

            List<BuffInfo> buffInfos = BuffInfo.Convert((List<DBBuffInfo>)db.SelectAllObjects<DBBuffInfo>());
            List<BuffCommandInfo> buffCommands = BuffCommandInfo.Convert(db.SelectAllObjects<DBBuffCommandInfo>().OrderBy(buffcmd => buffcmd.CommandID).ToList());

            IList<AbilityKnockbackInfo> knockbackInfos = db.SelectAllObjects<AbilityKnockbackInfo>().OrderBy(kbinfo => kbinfo.Id).ToList();

            List<AbilityCommandInfo> slaveCommands = new List<AbilityCommandInfo>();
            List<BuffCommandInfo> slaveBuffCommands = new List<BuffCommandInfo>();

            Dictionary<ushort, int> damageTypeDictionary = new Dictionary<ushort, int>();
            #endregion

            for (byte i = 0; i < 24; ++i)
                CareerAbilities[i] = new List<AbilityInfo>();

            #region AbilityChecks

            foreach (AbilityModifierCheck check in abChecks)
            {
                switch (check.PreOrPost)
                {
                    case 0:
                        if (!AbilityPreCastModifiers.ContainsKey(check.Entry))
                        {
                            AbilityPreCastModifiers.Add(check.Entry, new List<AbilityModifier>());

                            while (AbilityPreCastModifiers[check.Entry].Count < check.ID + 1)
                                AbilityPreCastModifiers[check.Entry].Add(new AbilityModifier(check.Entry, check.Affecting));

                            AbilityPreCastModifiers[check.Entry][check.ID].AddCheck(check);
                        }

                        else
                        {
                            if (AbilityPreCastModifiers[check.Entry].Count == check.ID)
                                AbilityPreCastModifiers[check.Entry].Add(new AbilityModifier(check.Entry, check.Affecting));
                            AbilityPreCastModifiers[check.Entry][check.ID].AddCheck(check);
                        }
                        break;

                    case 1:
                        if (!AbilityModifiers.ContainsKey(check.Entry))
                        {
                            AbilityModifiers.Add(check.Entry, new List<AbilityModifier>());

                            while (AbilityModifiers[check.Entry].Count < check.ID + 1)
                                AbilityModifiers[check.Entry].Add(new AbilityModifier(check.Entry, check.Affecting));
                            AbilityModifiers[check.Entry][check.ID].AddCheck(check);
                        }

                        else
                        {
                            if (AbilityModifiers[check.Entry].Count == check.ID)
                                AbilityModifiers[check.Entry].Add(new AbilityModifier(check.Entry, check.Affecting));
                            AbilityModifiers[check.Entry][check.ID].AddCheck(check);
                        }
                        break;
                    case 2:
                        if (!BuffModifiers.ContainsKey(check.Entry))
                        {
                            BuffModifiers.Add(check.Entry, new List<AbilityModifier>());
                            while (BuffModifiers[check.Entry].Count < check.ID + 1)
                                BuffModifiers[check.Entry].Add(new AbilityModifier(check.Entry, check.Affecting));
                            BuffModifiers[check.Entry][check.ID].AddCheck(check);
                        }

                        else
                        {
                            if (BuffModifiers[check.Entry].Count == check.ID)
                                BuffModifiers[check.Entry].Add(new AbilityModifier(check.Entry, check.Affecting));
                            BuffModifiers[check.Entry][check.ID].AddCheck(check);
                        }
                        break;
                    case 3:
                        if (!AbilityDelayedModifiers.ContainsKey(check.Entry))
                        {
                            AbilityDelayedModifiers.Add(check.Entry, new List<AbilityModifier>());

                            while (AbilityDelayedModifiers[check.Entry].Count < check.ID + 1)
                                AbilityDelayedModifiers[check.Entry].Add(new AbilityModifier(check.Entry, check.Affecting));

                            AbilityDelayedModifiers[check.Entry][check.ID].AddCheck(check);
                        }

                        else
                        {
                            if (AbilityDelayedModifiers[check.Entry].Count == check.ID)
                                AbilityDelayedModifiers[check.Entry].Add(new AbilityModifier(check.Entry, check.Affecting));
                            AbilityDelayedModifiers[check.Entry][check.ID].AddCheck(check);
                        }
                        break;
                }
            }

            #endregion

            #region AbilityModifiers

            foreach (AbilityModifierEffect effect in abMods)
            {
                switch(effect.PreOrPost)
                {
                    case 0:
                        if (!AbilityPreCastModifiers.ContainsKey(effect.Entry))
                        {
                            AbilityPreCastModifiers.Add(effect.Entry, new List<AbilityModifier>());
                            AbilityPreCastModifiers[effect.Entry].Add(new AbilityModifier(effect.Entry, effect.Affecting));
                            AbilityPreCastModifiers[effect.Entry][0].AddModifier(effect);
                        }

                        else
                        {
                            if (AbilityPreCastModifiers[effect.Entry].Count == effect.Sequence)
                                AbilityPreCastModifiers[effect.Entry].Add(new AbilityModifier(effect.Entry, effect.Affecting));
                            AbilityPreCastModifiers[effect.Entry][effect.Sequence].AddModifier(effect);
                        }
                        break;
                    case 1:
                        if (!AbilityModifiers.ContainsKey(effect.Entry))
                        {
                            AbilityModifiers.Add(effect.Entry, new List<AbilityModifier>());
                            AbilityModifiers[effect.Entry].Add(new AbilityModifier(effect.Entry, effect.Affecting));
                            AbilityModifiers[effect.Entry][0].AddModifier(effect);
                        }

                        else
                        {
                            if (AbilityModifiers[effect.Entry].Count == effect.Sequence)
                                AbilityModifiers[effect.Entry].Add(new AbilityModifier(effect.Entry, effect.Affecting));
                            AbilityModifiers[effect.Entry][effect.Sequence].AddModifier(effect);
                        }
                        break;
                    case 2:
                        if (!BuffModifiers.ContainsKey(effect.Entry))
                        {
                            BuffModifiers.Add(effect.Entry, new List<AbilityModifier>());
                            BuffModifiers[effect.Entry].Add(new AbilityModifier(effect.Entry, effect.Affecting));
                            BuffModifiers[effect.Entry][0].AddModifier(effect);
                        }

                        else
                        {
                            if (BuffModifiers[effect.Entry].Count == effect.Sequence)
                                BuffModifiers[effect.Entry].Add(new AbilityModifier(effect.Entry, effect.Affecting));
                            BuffModifiers[effect.Entry][effect.Sequence].AddModifier(effect);
                        }
                        break;
                    case 3:
                        if (!AbilityDelayedModifiers.ContainsKey(effect.Entry))
                        {
                            AbilityDelayedModifiers.Add(effect.Entry, new List<AbilityModifier>());
                            AbilityDelayedModifiers[effect.Entry].Add(new AbilityModifier(effect.Entry, effect.Affecting));
                            AbilityDelayedModifiers[effect.Entry][0].AddModifier(effect);
                        }

                        else
                        {
                            if (AbilityDelayedModifiers[effect.Entry].Count == effect.Sequence)
                                AbilityDelayedModifiers[effect.Entry].Add(new AbilityModifier(effect.Entry, effect.Affecting));
                            AbilityDelayedModifiers[effect.Entry][effect.Sequence].AddModifier(effect);
                        }
                        break;
                }
            }
            #endregion

            #region CommandInfo

            // Ability commands
            foreach (AbilityCommandInfo abCommand in abCommands)
            {
                if (abCommand.CommandSequence != 0)
                    slaveCommands.Add(abCommand);

                else
                {
                    if (!AbilityCommandInfos.ContainsKey(abCommand.Entry))
                        AbilityCommandInfos.Add(abCommand.Entry, new List<AbilityCommandInfo>());

                    AbilityCommandInfos[abCommand.Entry].Add(abCommand);
                }
            }

            foreach (AbilityCommandInfo slaveCommand in slaveCommands)
            {
                if (AbilityCommandInfos.ContainsKey(slaveCommand.Entry))
                    AbilityCommandInfos[slaveCommand.Entry][slaveCommand.CommandID].AddCommandToChain(slaveCommand);
                else
                    Log.Debug("AbilityMgr", "Slave command with entry " + slaveCommand.Entry + " and depending upon master command ID " + slaveCommand.CommandID + " has no master!");
            }

            #endregion

            #region BuffCommands

            foreach (BuffCommandInfo buffCommand in buffCommands)
            {
                if (buffCommand.CommandSequence != 0)
                    slaveBuffCommands.Add(buffCommand);
                else
                {
                    if (!BuffCommandInfos.ContainsKey(buffCommand.Entry))
                        BuffCommandInfos.Add(buffCommand.Entry, new List<BuffCommandInfo>());
                    BuffCommandInfos[buffCommand.Entry].Add(buffCommand);
                }
            }

            foreach (BuffCommandInfo slaveBuffCommand in slaveBuffCommands)
            {
                if (BuffCommandInfos.ContainsKey(slaveBuffCommand.Entry))
                    BuffCommandInfos[slaveBuffCommand.Entry][slaveBuffCommand.CommandID].AddCommandToChain(slaveBuffCommand);
                else
                    Log.Debug("AbilityMgr", "Slave buff command with entry " + slaveBuffCommand.Entry + " and depending upon master command ID " + slaveBuffCommand.CommandID + " has no master!");
            }

            #endregion

            #region Damage/Heals

            // Damage and heal info gets tacked onto the command that's going to use it
            foreach (AbilityDamageInfo abDmgHeal in abDmgHeals)
            {
                if (abDmgHeal.DisplayEntry == 0)
                    abDmgHeal.DisplayEntry = abDmgHeal.Entry;
                switch(abDmgHeal.Index)
                {
                    case 0:
                        if (AbilityCommandInfos.ContainsKey(abDmgHeal.Entry))
                        {
                            AbilityCommandInfo desiredCommand = AbilityCommandInfos[abDmgHeal.Entry][abDmgHeal.ParentCommandID].GetSubcommand(abDmgHeal.ParentCommandSequence);
                            if (desiredCommand != null)
                                desiredCommand.DamageInfo = abDmgHeal;
                        }

                        if (!damageTypeDictionary.ContainsKey(abDmgHeal.Entry))
                            damageTypeDictionary.Add(abDmgHeal.Entry, (int)abDmgHeal.DamageType);
                        break;

                    case 1:
                        if (BuffCommandInfos.ContainsKey(abDmgHeal.Entry))
                        {
                            try
                            {
                                BuffCommandInfo desiredCommand = BuffCommandInfos[abDmgHeal.Entry][abDmgHeal.ParentCommandID].GetSubcommand(abDmgHeal.ParentCommandSequence);
                                if (desiredCommand != null)
                                    desiredCommand.DamageInfo = abDmgHeal;
                            }
                            catch
                            {
                                Log.Error("AbilityMgr", "Failed Load: " + abDmgHeal.Entry + " " + abDmgHeal.ParentCommandID);
                            }

                            if (!damageTypeDictionary.ContainsKey(abDmgHeal.Entry))
                                damageTypeDictionary.Add(abDmgHeal.Entry, (int)abDmgHeal.DamageType);
                        }
                        break;
                    case 2:
                        if (!ExtraDamage.ContainsKey(abDmgHeal.Entry))
                            ExtraDamage.Add(abDmgHeal.Entry, new List<List<AbilityDamageInfo>>());
                        if (ExtraDamage[abDmgHeal.Entry].Count == abDmgHeal.ParentCommandID)
                            ExtraDamage[abDmgHeal.Entry].Add(new List<AbilityDamageInfo>());
                        ExtraDamage[abDmgHeal.Entry][abDmgHeal.ParentCommandID].Add(abDmgHeal);
                        break;
                    default:
                        throw new Exception("Invalid index specified for ability damage with ID " + abDmgHeal.Entry);
                }
            }

            #endregion

            #region KnockbackInfo

            foreach (var kbInfo in knockbackInfos)
            {
                if (!KnockbackInfos.ContainsKey(kbInfo.Entry))
                    KnockbackInfos.Add(kbInfo.Entry, new List<AbilityKnockbackInfo>());
                KnockbackInfos[kbInfo.Entry].Add(kbInfo);
            }

            #endregion

            // Volatiles -> Constants
            //           -> Commands -> DamageHeals
            foreach (AbilityInfo abVolatile in abVolatiles)
            {
                if (!NewAbilityVolatiles.ContainsKey(abVolatile.Entry))
                    NewAbilityVolatiles.Add(abVolatile.Entry, abVolatile);

                if (AbilityCommandInfos.ContainsKey(abVolatile.Entry))
                {
                    abVolatile.TargetType = AbilityCommandInfos[abVolatile.Entry][0].TargetType;
                    if (AbilityCommandInfos[abVolatile.Entry][0].AoESource != 0)
                        abVolatile.TargetType = AbilityCommandInfos[abVolatile.Entry][0].AoESource;
                }
            }

            #region ConstantInfo

            foreach (AbilityConstants abConstant in abConstants.Values)
            {
                if (NewAbilityVolatiles.ContainsKey(abConstant.Entry))
                {
                    NewAbilityVolatiles[abConstant.Entry].ConstantInfo = abConstant;

                    if (damageTypeDictionary.ContainsKey(abConstant.Entry))
                    {
                        if (damageTypeDictionary[abConstant.Entry] == (ushort) DamageTypes.Healing || damageTypeDictionary[abConstant.Entry] == (ushort) DamageTypes.RawHealing)
                            abConstant.IsHealing = true;
                        else abConstant.IsDamaging = true;
                    }

                    uint careerRequirement = abConstant.CareerLine;
                    byte count = 0;

                    while (careerRequirement > 0 && count < 24)
                    {
                        if ((careerRequirement & 1) > 0)
                            CareerAbilities[count].Add(NewAbilityVolatiles[abConstant.Entry]);
                        careerRequirement = careerRequirement >> 1;
                        count++;
                    }
                }
            }

            #endregion

            #region Damage to ConstantInfo linkage

            foreach (AbilityDamageInfo damageInfo in abDmgHeals)
            {
                if (abConstants.ContainsKey(damageInfo.Entry))
                    damageInfo.MasteryTree = abConstants[damageInfo.Entry].MasteryTree;
            }

            #endregion

            #region Buff/Command linkage

            foreach (BuffInfo buffInfo in buffInfos)
            {
                if (!BuffInfos.ContainsKey(buffInfo.Entry))
                    BuffInfos.Add(buffInfo.Entry, buffInfo);

                if (BuffCommandInfos.ContainsKey(buffInfo.Entry))
                    buffInfo.CommandInfo = BuffCommandInfos[buffInfo.Entry];

                if (abConstants.ContainsKey(buffInfo.Entry))
                    buffInfo.MasteryTree = abConstants[buffInfo.Entry].MasteryTree;
            }

            #endregion

            Log.Success("AbilityMgr", "Finished loading " + NewAbilityVolatiles.Count + " abilities and " + BuffInfos.Count + " buffs!");

            LoadCreatureAbilities();
        }

        #region Creature Abilities

        public static Dictionary<uint, List<NPCAbility>> CreatureAbilities = new Dictionary<uint, List<NPCAbility>>();

        public static void LoadCreatureAbilities()
        {
            CreatureAbilities.Clear();

            IList<Creature_abilities> creaAbs = WorldMgr.Database.SelectAllObjects<Creature_abilities>();

            Dictionary<uint, List<NPCAbility>> temp = new Dictionary<uint, List<NPCAbility>>();

            foreach (var cAb in creaAbs)
            {
                if (!temp.ContainsKey(cAb.ProtoEntry))
                    temp.Add(cAb.ProtoEntry, new List<NPCAbility>());

                AbilityInfo abInfo = GetAbilityInfo(cAb.AbilityId);
                if (abInfo != null)
                {
                    temp[cAb.ProtoEntry].Add(new NPCAbility(cAb.AbilityId, abInfo.ConstantInfo.AIRange, Math.Max(abInfo.Cooldown, cAb.Cooldown), true, cAb.Text, cAb.TimeStart, cAb.ActivateAtHealthPercent, cAb.AbilityCycle, cAb.Active, cAb.ActivateOnCombatStart, cAb.RandomTarget, cAb.TargetFocus, cAb.DisableAtHealthPercent, cAb.MinRange));
                    Log.Dump("Entry: " + cAb.ProtoEntry, cAb.AbilityId + " " + abInfo.Name + " ~ Loaded");
                }
                else
                {
                    Log.Error("Entry: " + cAb.ProtoEntry, cAb.AbilityId + " ~ Failed loading");
                }
            }

            foreach (uint key in temp.Keys)
                CreatureAbilities[key] = temp[key].OrderByDescending(x => x.Cooldown).ToList();
        }

        public static List<NPCAbility> GetCreatureAbilities(uint entry)
        {
            if (CreatureAbilities.ContainsKey(entry))
                return CreatureAbilities[entry];
            return null;
        }

        #endregion
        #endregion

        #region Accessors

        public static bool HasPreCastModifiers(ushort entry)
        {
            return AbilityPreCastModifiers.ContainsKey(entry);
        }
        public static List<AbilityModifier> GetAbilityPreCastModifiers(ushort entry)
        {
            return AbilityPreCastModifiers.ContainsKey(entry) ? AbilityPreCastModifiers[entry] : null;
        }

        public static bool HasModifiers(ushort entry)
        {
            return AbilityModifiers.ContainsKey(entry);
        }
        public static List<AbilityModifier> GetAbilityModifiers(ushort entry)
        {
            return AbilityModifiers.ContainsKey(entry) ? AbilityModifiers[entry] : null;
        }

        public static List<AbilityModifier> GetAbilityDelayedModifiers(ushort entry)
        {
            return AbilityDelayedModifiers.ContainsKey(entry) ? AbilityDelayedModifiers[entry] : null;
        }

        public static List<AbilityInfo> GetAvailableCareerAbilities(byte careerLine, int minRank, int maxRank)
        {
            List<AbilityInfo> charAbilities = new List<AbilityInfo>();

            foreach (AbilityInfo ab in CareerAbilities[careerLine - 1])
            {
                if (ab.ConstantInfo.MasteryTree > 0 && ab.ConstantInfo.PointCost > 0)
                    continue;

                if (ab.ConstantInfo.MinimumRank < minRank || ab.ConstantInfo.MinimumRank > maxRank)
                    continue;

                charAbilities.Add(ab);
            }

            return charAbilities;
        }

        public static List<AbilityInfo> GetMasteryAbilities(byte careerLine)
        {
            List<AbilityInfo> masteryAbilities = new List<AbilityInfo>();

            foreach (AbilityInfo ab in CareerAbilities[careerLine - 1])
            {
                if (ab.ConstantInfo.MasteryTree > 0 && ab.ConstantInfo.PointCost > 0)
                    masteryAbilities.Add(ab);
            }

            return masteryAbilities;

        } 

        public static AbilityInfo GetAbilityInfo(ushort entry)
        {
            if (!NewAbilityVolatiles.ContainsKey(entry))
                return null;

            return NewAbilityVolatiles[entry].Clone();
        }
        public static string GetAbilityNameFor(ushort abilityEntry)
        {
            if (NewAbilityVolatiles.ContainsKey(abilityEntry))
                return NewAbilityVolatiles[abilityEntry].Name;
            if (BuffInfos.ContainsKey(abilityEntry))
                return BuffInfos[abilityEntry].Name;
            return "attack";
        }
        public static byte GetMasteryTreeFor(ushort entry)
        {
            if (NewAbilityVolatiles.ContainsKey(entry))
                return NewAbilityVolatiles[entry].ConstantInfo.MasteryTree;
            return 0;
        }
        public static ushort GetCooldownFor(ushort entry)
        {
            if (NewAbilityVolatiles.ContainsKey(entry))
                return NewAbilityVolatiles[entry].Cooldown;
            return 0;
        }
        public static AbilityDamageInfo GetExtraDamageFor(ushort entry, byte id, byte index)
        {
            try
            {
                return ExtraDamage[entry][id][index].Clone();
            }
            catch (Exception)
            {
                Log.Error("AbilityMgr", "Couldn't get damage info for Entry " + entry + " ID "+ id +" Index " + index);
                return null;
            }
        }
        public static bool RequiresResource(ushort entry)
        {
            if (!NewAbilityVolatiles.ContainsKey(entry))
                return false;

            return NewAbilityVolatiles[entry].SpecialCost > 0;
        }

        public static AbilityKnockbackInfo GetKnockbackInfo(ushort entry, int id)
        {
            return KnockbackInfos[entry][id];
        }

        public static bool HasCommandsFor(ushort abilityEntry)
        {
            return AbilityCommandInfos.ContainsKey(abilityEntry);
        }
        public static void GetCommandsFor(Unit caster, AbilityInfo abInfo)
        {
            if (AbilityCommandInfos.ContainsKey(abInfo.Entry))
            {
                // Add commands to the new info if they're applicable to the player.
                // Has to be done here because of bloody tactics and career crap
                foreach (AbilityCommandInfo abCommand in AbilityCommandInfos[abInfo.Entry])
                {
                    if (!abCommand.NoAutoUse)
                    {
                        abInfo.CommandInfo.Add(abCommand.Clone(caster));

                        for (AbilityCommandInfo slaveCommand = abCommand.NextCommand; slaveCommand != null; slaveCommand = slaveCommand.NextCommand)
                        {
                            if (!slaveCommand.NoAutoUse)
                                abInfo.CommandInfo[(byte)(abInfo.CommandInfo.Count - 1)].AddCommandToChain(slaveCommand.Clone(caster));
                        }
                    }
                }
            }
        }

        public static AbilityCommandInfo GetAbilityCommand(Unit caster, ushort entry, byte comIndex)
        {
            return AbilityCommandInfos[entry][comIndex].Clone(caster);
        }
        public static AbilityCommandInfo GetAbilityCommand(Unit caster, ushort entry, byte comIndex, byte comSeq)
        {
            return AbilityCommandInfos[entry][comIndex].GetSubcommand(comSeq).Clone(caster);
        }

        #endregion

        #region Buff Interface

        public static bool HasBuffModifiers(ushort entry)
        {
            return BuffModifiers.ContainsKey(entry);
        }

        public static List<AbilityModifier> GetBuffModifiers(ushort entry)
        {
            return BuffModifiers.ContainsKey(entry) ? BuffModifiers[entry] : null;
        }

        public static BuffInfo GetBuffInfo(ushort entry)
        {
            if (BuffInfos.ContainsKey(entry))
                return BuffInfos[entry].Clone();
            Log.Error("GetBuffInfo(entry)", $"Nonexistent buff: {entry}");
            return null;
        }

        public static BuffInfo GetBuffInfo(ushort entry, Unit caster, Unit target)
        {
            if (BuffInfos.ContainsKey(entry))
            {
                BuffInfo buffInfo = BuffInfos[entry].Clone();

                List<AbilityModifier> myModifiers = GetBuffModifiers(entry);
                if (myModifiers != null)
                {
                    foreach (var modifier in myModifiers)
                        modifier.ModifyBuff(caster, target, buffInfo);
                }

                if (caster is Player)
                    ((Player)caster).TacInterface.ModifyBuff(buffInfo, target);

                return buffInfo;
            }
            Log.Error("GetBuffInfo(entry, caster, target)", $"Nonexistent buff: {entry}");
            return null;
        }

        public static BuffCommandInfo GetBuffCommand(ushort entry, byte commandIndex)
        {
            return BuffCommandInfos[entry][commandIndex].CloneChain();
        }

        public static BuffCommandInfo GetBuffCommand(ushort entry, byte commandIndex, byte comSeq)
        {
            return BuffCommandInfos[entry][commandIndex].GetSubcommand(comSeq).CloneChain();
        }

        #endregion
    }
}

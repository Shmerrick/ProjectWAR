using PWARAbilityTool.Client.Ui;
using PWARAbilityTool.Dtos;
using SqlKata;
using SqlKata.Compilers;
using System.Collections.Generic;

namespace PWARAbilityTool.apoc_api_serverrunner.Services
{
    public class AbilityService
    {
        #region selects
        public static void getSelectAllAbilites(out string query)
        {
            query = "select * from war_world.abilities";
        }

        public static void getSelectAbilityByEntry(int Entry, out string query)
        {
            query = $"select * from war_world.abilities where Entry = '{Entry}'";
        }

        public static void getSelectAbilityCommandQueryByEntry(int Entry, out string query)
        {
            query = $"select * from war_world.ability_commands where Entry = '{Entry}'";
        }

        public static void getSelectAbilityDmgHealQueryByEntry(int Entry, out string query)
        {
            query = $"select * from war_world.ability_damage_heals where Entry = '{Entry}'";
        }

        public static void getSelectAbilityKnockBackInfoQueryByEntry(int Entry, out string query)
        {
            query = $"select * from war_world.ability_knockback_info where Entry = '{Entry}'";
        }

        public static void getSelectAbilityModifiersQueryByEntry(int Entry, out string query)
        {
            query = $"select * from war_world.ability_modifiers where Entry = '{Entry}'";
        }

        public static void getSelectAbilityModifierChecksQueryByEntry(int Entry, out string query)
        {
            query = $"select * from war_world.ability_modifier_checks where Entry = '{Entry}'";
        }

        public static void getSelectAbilityBuffCommandsQueryByEntry(int Entry, out string query)
        {
            query = $"select * from war_world.buff_commands where Entry = '{Entry}'";
        }

        public static void getSelectAbilityBuffInfosQueryByEntry(int Entry, out string query)
        {
            query = $"select * from war_world.buff_infos where Entry = '{Entry}'";
        }

        public static void getSelectAbilityBySpeclinePlayer(out string query)
        {
            query = $"select * from war_world.abilities where Specline != 'NPC'";
        }

        public static void getSelectAbilityBySpeclineNPC(out string query)
        {
            query = $"select * from war_world.abilities where Specline = 'NPC'";
        }

        public static void getSelectAllSpeclines(out string query)
        {
            query = "select distinct Specline from war_world.abilities order by Specline asc";
        }

        public static void getSelectAllCareerLines(out string query)
        {
            query = "select distinct CareerLine from war_world.abilities order by CareerLine asc";
        }

        public static void getSelectAllAbilityTypes(out string query)
        {
            query = "select distinct AbilityType from war_world.abilities order by AbilityType asc";
        }

        public static void getSelectAllMasteryTrees(out string query)
        {
            query = "select distinct MasteryTree from war_world.abilities order by MasteryTree asc";
        }

        public static void getSelectAllTargets(out string query, string tableName)
        {
            query = $"select distinct Target from war_world.{tableName} order by Target asc";
        }

        public static void getSelectAllEffectSources(out string query, string tableName)
        {
            query = $"select distinct EffectSource from war_world.{tableName} order by EffectSource asc";
        }

        public static void getSelectAllCommandNames(out string query, string tableName)
        {
            query = $"select distinct CommandName from war_world.{tableName} order by CommandName asc";
        }

        public static void getSelectAllFailCodes(out string query)
        {
            query = "select distinct FailCode from war_world.ability_modifier_checks order by FailCode asc";
        }

        public static void getSelectAllModifierCommandNames(out string query)
        {
            query = "select distinct ModifierCommandName from war_world.ability_modifiers order by ModifierCommandName asc";
        }

        public static void getSelectAllBuffCommandNames(out string query)
        {
            query = "select distinct CommandName from war_world.buff_commands order by CommandName asc";
        }

        public static void getSelectAllBuffInfoClassStrings(out string query)
        {
            query = "select distinct BuffClassString from war_world.buff_infos order by BuffClassString asc";
        }

        public static void getSelectAllBuffInfoTypeString(out string query)
        {
            query = "select distinct TypeString from war_world.buff_infos order by TypeString asc";
        }
        #endregion

        #region updates
        public static void buildUpdateAbilitySingle(AbilitySingle abilitySingle, int Entry, out string query)
        {
            query = buildUpdateAbilitySingleQuery(abilitySingle, Entry);
        }

        public static void buildUpdateAbilityCommand(AbilityCommand abilityCommand, int Entry, out string query)
        {
            query = buildUpdateAbilityCommandQuery(abilityCommand, Entry);
        }

        public static void buildUpdateAbilityDamageHeals(AbilityDamageHeals abilityDamageHeals, int Entry, out string query)
        {
            query = buildUpdateAbilityDamageHealsQuery(abilityDamageHeals, Entry);
        }

        public static void buildUpdateAbilityModifiers(AbilityModifiers abilityModifiers, int Entry, out string query)
        {
            query = buildUpdateAbilityModifiersQuery(abilityModifiers, Entry);
        }

        public static void buildUpdateAbilityModifierChecks(AbilityModifierChecks abilityModifierChecks, int Entry, out string query)
        {
            query = buildUpdateAbilityModifierChecksQuery(abilityModifierChecks, Entry);
        }

        public static void buildUpdateAbilityKnockBackInfo(AbilityKnockBackInfo abilityKnockBackInfo, int Entry, out string query)
        {
            query = buildUpdateAbilityKnockBackInfoQuery(abilityKnockBackInfo, Entry);
        }

        public static void buildUpdateAbilityBuffInfos(AbilityBuffInfos abilityBuffInfos, int Entry, out string query)
        {
            query = buildUpdateAbilityBuffInfosQuery(abilityBuffInfos, Entry);
        }

        public static void buildUpdateAbilityBuffCommand(AbilityBuffCommands abilityBuffCommands, int Entry, out string query)
        {
            query = buildUpdateAbilityBuffCommandsQuery(abilityBuffCommands, Entry);
        }
        #endregion

        #region inserts
        public static void buildInsertAbilitySingleQuery(AbilitySingle abilitySingle, out string query, out object queryDataBindings)
        {
            query = $"insert into war_world.abilities " +
                        $"(Entry, CareerLine, war_world.abilities.Name, MinRange, war_world.abilities.Range, CastTime, Cooldown, ApCost, SpecialCost, MoveCast, " +
                        $"InvokeDelay, EffectDelay, EffectID, ChannelID, CooldownEntry, ToggleEntry, CastAngle, AbilityType, MasteryTree, Specline, WeaponNeeded, " +
                        $"AffectsDead, IgnoreGlobalCooldown, IgnoreOwnModifiers, Fragile, MinimumRank, MinimumRenown, IconId, Category, war_world.abilities.Flags, PointCost, CashCost, " +
                        $"StealthInteraction, AIRange, IgnoreCooldownReduction, CDCap, VFXTarget, abilityID, effectID2, war_world.abilities.Time) " +
                        $"values " +
                        $"(@Entry, @CareerLine, @Name, @MinRange, @Range, @CastTime, @Cooldown, @ApCost, @SpecialCost, @MoveCast, " +
                        $"@InvokeDelay, @EffectDelay, @EffectID, @ChannelID, @CooldownEntry, @ToggleEntry, @CastAngle, @AbilityType, @MasteryTree, @Specline, @WeaponNeeded, " +
                        $"@AffectsDead, @IgnoreGlobalCooldown, @IgnoreOwnModifiers, @Fragile, @MinimumRank, @MinimumRenown, @IconId, @Category, @Flags, @PointCost, @CashCost, " +
                        $"@StealthInteraction, @AIRange, @IgnoreCooldownReduction, @CDCap, @VFXTarget, @abilityID, @effectID2, @Time);";

            queryDataBindings = new
            {
                Entry = abilitySingle.Entry,
                CareerLine = abilitySingle.CareerLine,
                Name = abilitySingle.Name,
                MinRange = abilitySingle.MinRange,
                Range = abilitySingle.Range,
                CastTime = abilitySingle.CastTime,
                Cooldown = abilitySingle.Cooldown,
                ApCost = abilitySingle.ApCost,
                SpecialCost = abilitySingle.SpecialCost,
                MoveCast = abilitySingle.MoveCast,
                InvokeDelay = abilitySingle.InvokeDelay,
                EffectDelay = abilitySingle.EffectDelay,
                EffectID = abilitySingle.EffectID,
                ChannelID = abilitySingle.ChannelID,
                CooldownEntry = abilitySingle.CooldownEntry,
                ToggleEntry = abilitySingle.ToggleEntry,
                CastAngle = abilitySingle.CastAngle,
                AbilityType = abilitySingle.AbilityType,
                MasteryTree = abilitySingle.MasteryTree,
                Specline = abilitySingle.Specline,
                WeaponNeeded = abilitySingle.WeaponNeeded,
                AffectsDead = abilitySingle.AffectsDead,
                IgnoreGlobalCooldown = abilitySingle.IgnoreGlobalCooldown,
                IgnoreOwnModifiers = abilitySingle.IgnoreOwnModifiers,
                Fragile = abilitySingle.Fragile,
                MinimumRank = abilitySingle.MinimumRank,
                MinimumRenown = abilitySingle.MinimumRenown,
                IconId = abilitySingle.IconId,
                Category = abilitySingle.Category,
                Flags = abilitySingle.Flags,
                PointCost = abilitySingle.PointCost,
                CashCost = abilitySingle.CashCost,
                StealthInteraction = abilitySingle.StealthInteraction,
                AIRange = abilitySingle.AIRange,
                IgnoreCooldownReduction = abilitySingle.IgnoreCooldownReduction,
                CDCap = abilitySingle.CDcap,
                VFXTarget = abilitySingle.VFXTarget,
                abilityID = abilitySingle.abilityID,
                effectID2 = abilitySingle.effectID2,
                Time = abilitySingle.Time
            };
        }


        public static void buildInsertAbilityBuffInfo(AbilityBuffInfos absBuffInfos, out string query, out object queryDataBindings)
        {
            query = $"insert into war_world.buff_infos " +
                $"(Entry, war_world.buff_infos.Name, BuffClassString, TypeString, war_world.buff_infos.Group, AuraPropagation, MaxCopies, MaxStack, UseMaxStackAsInitial, " +
                $"StackLine, StacksFromCaster, Duration, LeadInDelay, war_world.buff_infos.Interval, PersistsOnDeath, CanRefresh, FriendlyEffectID, EnemyEffectID, Silent) " +
                $"values " +
                $"(@Entry, @Name, @BuffClassString, @TypeString, @Group, @AuraPropagation, @MaxCopies, @MaxStack, @UseMaxStackAsInitial, " +
                $"@StackLine, @StacksFromCaster, @Duration, @LeadInDelay, @Interval, @PersistsOnDeath, @CanRefresh, @FriendlyEffectID, @EnemyEffectID, @Silent)";

            queryDataBindings = new
            {
                Entry = absBuffInfos.Entry,
                Name = absBuffInfos.Name,
                BuffClassString = absBuffInfos.BuffClassString,
                TypeString = absBuffInfos.TypeString,
                Group = absBuffInfos.Group,
                AuraPropagation = absBuffInfos.AuraPropagation,
                MaxCopies = absBuffInfos.MaxCopies,
                MaxStack = absBuffInfos.MaxStack,
                UseMaxStackAsInitial = absBuffInfos.UserMaxStackAsInitial,
                StackLine = absBuffInfos.StackLine,
                StacksFromCaster = absBuffInfos.StacksFromCaster,
                Duration = absBuffInfos.Duration,
                LeadInDelay = absBuffInfos.LeadInDelay,
                Interval = absBuffInfos.Interval,
                PersistsOnDeath = absBuffInfos.PersistsOnDeath,
                CanRefresh = absBuffInfos.CanRefresh,
                FriendlyEffectID = absBuffInfos.FriendlyEffectID,
                EnemyEffectID = absBuffInfos.EnemyEffectID,
                Silent = absBuffInfos.Silent
            };
        }

        public static void buildInsertAbilityBuffCommand(AbilityBuffCommands absBuffCommands, out string query, out object queryDataBindings)
        {
            query = $"insert into war_world.buff_commands " +
                $"(Entry, war_world.buff_commands.Name, CommandID, CommandSequence, CommandName, PrimaryValue, SecondaryValue, TertiaryValue, " +
                $"InvokeOn, war_world.buff_commands.Target, EffectSource, EffectRadius, EffectAngle, MaxTargets, EventIDString, EventCheck, " +
                $"EventCheckParam, EventChance, ConsumesStack, RetriggerInterval, BuffLine, NoAutoUse, BuffClassString)" +
                $"values " +
                $"(@Entry, @Name, @CommandID, @CommandSequence, @CommandName, @PrimaryValue, @SecondaryValue, @TertiaryValue, " +
                $"@InvokeOn, @Target, @EffectSource, @EffectRadius, @EffectAngle, @MaxTargets, @EventIDString, @EventCheck, " +
                $"@EventCheckParam, @EventChance, @ConsumesStack, @RetriggerInterval, @BuffLine, @NoAutoUse, @BuffClassString)";

            queryDataBindings = new
            {
                Entry = absBuffCommands.Entry,
                Name = absBuffCommands.Name,
                CommandID = absBuffCommands.CommandID,
                CommandSequence = absBuffCommands.CommandSequence,
                CommandName = absBuffCommands.CommandName,
                PrimaryValue = absBuffCommands.PrimaryValue,
                SecondaryValue = absBuffCommands.SecondaryValue,
                TertiaryValue = absBuffCommands.TertiaryValue,
                InvokeOn = absBuffCommands.InvokeOn,
                Target = absBuffCommands.Target,
                EffectSource = absBuffCommands.EffectSource,
                EffectRadius = absBuffCommands.EffectRadius,
                EffectAngle = absBuffCommands.EffectAngle,
                MaxTargets = absBuffCommands.MaxTargets,
                EventIDString = absBuffCommands.EventIDString,
                EventCheck = absBuffCommands.EventCheck,
                EventCheckParam = absBuffCommands.EventCheckParam,
                EventChance = absBuffCommands.EventChance,
                ConsumesStack = absBuffCommands.ConsumesStack,
                RetriggerInterval = absBuffCommands.RetriggerInterval,
                BuffLine = absBuffCommands.BuffLine,
                NoAutoUse = absBuffCommands.NoAutoUse,
                BuffClassString = absBuffCommands.BuffClassString
            };
        }

        public static void buildInsertAbilityKnockBackInfo(AbilityKnockBackInfo absKnockBackInfo, out string query, out object queryDataBindings)
        {
            query = $"insert into war_world.ability_knockback_info " +
                $"(Entry, war_world.ability_knockback_info.Id, Angle, Power, RangeExtension, GravMultiplier, Unk) " +
                $"values " +
                $"(@Entry, @Id, @Angle, @Power, @RangeExtension, @GravMultiplier, @Unk)";

            queryDataBindings = new
            {
                Entry = absKnockBackInfo.Entry,
                Id = absKnockBackInfo.Id,
                Angle = absKnockBackInfo.Angle,
                Power = absKnockBackInfo.Power,
                RangeExtension = absKnockBackInfo.RangeExtension,
                GravMultiplier = absKnockBackInfo.GravMultiplier,
                Unk = absKnockBackInfo.Unk
            };
        }

        public static void buildInsertAbilityModifiers(AbilityModifiers absModifier, out string query, out object queryDataBindings)
        {
            query = $"insert into war_world.ability_modifiers " +
                $"(Entry, SourceAbility, Affecting, AffectedAbility, PreOrPost, Sequence, ModifierCommandName, " +
                $"TargetCommandID, TargetCommandSequence, PrimaryValue, SecondaryValue, BuffLine, ability_modifiers_ID) " +
                $"values " +
                $"(@Entry, @SourceAbility, @Affecting, @AffectedAbility, @PreOrPost, @Sequence, @ModifierCommandName, " +
                $"@TargetCommandID, @TargetCommandSequence, @PrimaryValue, @SecondaryValue, @BuffLine, @ability_modifiers_ID)";

            queryDataBindings = new
            {
                Entry = absModifier.Entry,
                SourceAbility = absModifier.SourceAbility,
                Affecting = absModifier.Affecting,
                AffectedAbility = absModifier.AffectedAbility,
                PreOrPost = absModifier.PreOrPost,
                Sequence = absModifier.Sequence,
                ModifierCommandName = absModifier.ModifierCommandName,
                TargetCommandID = absModifier.TargetCommandID,
                TargetCommandSequence = absModifier.TargetCommandSequence,
                PrimaryValue = absModifier.PrimaryValue,
                SecondaryValue = absModifier.SecondaryValue,
                BuffLine = absModifier.BuffLine,
                ability_modifiers_ID = absModifier.ability_modifiers_ID
            };
        }

        public static void buildInsertAbilityModifierChecks(AbilityModifierChecks absModChecks, out string query, out object queryDataBindings)
        {
            query = $"insert into war_world.ability_modifier_checks " +
                $"(Entry, SourceAbility, Affecting, AffectedAbility, PreOrPost, war_world.ability_modifier_checks.ID, Sequence, CommandName, FailCode, PrimaryValue, SecondaryValue, ability_modifier_checks_ID) " +
                $"values " +
                $"(@Entry, @SourceAbility, @Affecting, @AffectedAbility, @PreOrPost, @ID, @Sequence, @CommandName, @FailCode, @PrimaryValue, @SecondaryValue, @ability_modifier_checks_ID)";

            queryDataBindings = new
            {
                Entry = absModChecks.Entry,
                SourceAbility = absModChecks.SourceAbility,
                Affecting = absModChecks.Affecting,
                AffectedAbility = absModChecks.AffectedAbility,
                PreOrPost = absModChecks.PreOrPost,
                ID = absModChecks.ID,
                Sequence = absModChecks.Sequence,
                CommandName = absModChecks.CommandName,
                FailCode = absModChecks.FailCode,
                PrimaryValue = absModChecks.PrimaryValue,
                SecondaryValue = absModChecks.SecondaryValue,
                ability_modifier_checks_ID = absModChecks.ability_modifier_checks_ID
            };
        }

        public static void buildInsertAbilityDamageHeals(AbilityDamageHeals absDmHeal, out string query, out object queryDataBindings)
        {
            query = "insert into war_world.ability_damage_heals " +
                "(Entry, DisplayEntry, war_world.ability_damage_heals.Index, war_world.ability_damage_heals.Name, MinDamage, MaxDamage, DamageVariance, DamageType, " +
                "ParentCommandID, ParentCommandSequence, CastTimeDamageMult, WeaponDamageFrom, WeaponDamageScale, NoCrits," +
                "Undefendable, OverrideDefenseEvent, StatUsed, StatDamageScale, war_world.ability_damage_heals.ResourceBuild, CastPlayerSubID, " +
                "ArmorResistPenFactor, HatredScale, HealHatredScale, PriStatMultiplier) " +
                "values " +
                "(@Entry, @DisplayEntry, @Index, @Name, @MinDamage, @MaxDamage, @DamageVariance, @DamageType, " +
                "@ParentCommandID, @ParentCommandSequence, @CastTimeDamageMult, @WeaponDamageFrom, @WeaponDamageScale, @NoCrits," +
                "@Undefendable, @OverrideDefenseEvent, @StatUsed, @StatDamageScale, @ResourceBuild, @CastPlayerSubID, " +
                "@ArmorResistPenFactor, @HatredScale, @HealHatredScale, @PriStatMultiplier)";

            queryDataBindings = new
            {
                Entry = absDmHeal.Entry,
                DisplayEntry = absDmHeal.DisplayEntry,
                Index = absDmHeal.Index,
                Name = absDmHeal.Name,
                MinDamage = absDmHeal.MinDamage,
                MaxDamage = absDmHeal.MaxDamage,
                DamageVariance = absDmHeal.DamageVariance,
                DamageType = absDmHeal.DamageType,
                ParentCommandID = absDmHeal.ParentCommandID,
                ParentCommandSequence = absDmHeal.ParentCommandSequence,
                CastTimeDamageMult = absDmHeal.CastTimeDamageMult,
                WeaponDamageFrom = absDmHeal.WeaponDamageFrom,
                WeaponDamageScale = absDmHeal.WeaponDamageScale,
                NoCrits = absDmHeal.NoCrits,
                Undefendable = absDmHeal.Undefendable,
                OverrideDefenseEvent = absDmHeal.OverrideDefenseEvent,
                StatUsed = absDmHeal.StatUsed,
                StatDamageScale = absDmHeal.StatDamageScale,
                ResourceBuild = absDmHeal.RessourceBuild,
                CastPlayerSubID = absDmHeal.CastPlayerSubID,
                ArmorResistPenFactor = absDmHeal.ArmorResistPenFactor,
                HatredScale = absDmHeal.HatredScale,
                HealHatredScale = absDmHeal.HealHatredScale,
                PriStatMultiplier = absDmHeal.PriStatMultiplier
            };
        }

        public static void buildInsertAbilityCommand(AbilityCommand absCommand, out string query, out object queryDataBindings)
        {
            query = $"insert into war_world.ability_commands " +
                $"(AbilityName, AttackingStat, CommandID, CommandName, CommandSequence, EffectAngle, EffectRadius, EffectSource, " +
                $"Entry, FromAllTargets, isDelayedEffect, MaxTargets, NoAutoUse, PrimaryValue, SecondaryValue, Target) " +
                $"values " +
                $"(@AbilityName, @AttackingStat, @CommandID, @CommandName, @CommandSequence, @EffectAngle, @EffectRadius, @EffectSource, " +
                $"@Entry, @FromAllTargets, @isDelayedEffect, @MaxTargets, @NoAutoUse, @PrimaryValue, @SecondaryValue, @Target)";

            queryDataBindings = new
            {
                AbilityName = absCommand.AbilityName,
                AttackingStat = absCommand.AttackingStat,
                CommandID = absCommand.CommandID,
                CommandName = absCommand.CommandName,
                CommandSequence = absCommand.CommandSequence,
                EffectAngle = absCommand.EffectAngle,
                EffectRadius = absCommand.EffectRadius,
                EffectSource = absCommand.EffectSource,
                Entry = absCommand.Entry,
                FromAllTargets = absCommand.FromAllTargets,
                isDelayedEffect = absCommand.isDelayedEffect,
                MaxTargets = absCommand.MaxTargets,
                NoAutoUse = absCommand.NoAutoUse,
                PrimaryValue = absCommand.PrimaryValue,
                SecondaryValue = absCommand.SecondaryValue,
                Target = absCommand.Target
            };
        }
        #endregion

        #region helpers
        private static MySqlCompiler getMySqlCompiler()
        {
            return new MySqlCompiler();
        }

        public static AbilitySingle getSelectedAbility(List<AbilitySingle> abilities)
        {
            CustomDuplicatePresenter cdp = new CustomDuplicatePresenter(abilities);
            cdp.ShowDialog();
            return cdp.SelectedItem as AbilitySingle;
        }

        public static AbilityModifiers getSelectedAbility(List<AbilityModifiers> abilities)
        {
            CustomDuplicatePresenter cdp = new CustomDuplicatePresenter(abilities);
            cdp.ShowDialog();
            return cdp.SelectedItem as AbilityModifiers;
        }

        public static AbilityModifierChecks getSelectedAbility(List<AbilityModifierChecks> abilities)
        {
            CustomDuplicatePresenter cdp = new CustomDuplicatePresenter(abilities);
            cdp.ShowDialog();
            return cdp.SelectedItem as AbilityModifierChecks;
        }

        public static AbilityKnockBackInfo getSelectedAbility(List<AbilityKnockBackInfo> abilities)
        {
            CustomDuplicatePresenter cdp = new CustomDuplicatePresenter(abilities);
            cdp.ShowDialog();
            return cdp.SelectedItem as AbilityKnockBackInfo;
        }

        public static AbilityDamageHeals getSelectedAbility(List<AbilityDamageHeals> abilities)
        {
            CustomDuplicatePresenter cdp = new CustomDuplicatePresenter(abilities);
            cdp.ShowDialog();
            return cdp.SelectedItem as AbilityDamageHeals;
        }

        public static AbilityCommand getSelectedAbility(List<AbilityCommand> abilities)
        {
            CustomDuplicatePresenter cdp = new CustomDuplicatePresenter(abilities);
            cdp.ShowDialog();
            return cdp.SelectedItem as AbilityCommand;
        }

        public static AbilityBuffInfos getSelectedAbility(List<AbilityBuffInfos> abilities)
        {
            CustomDuplicatePresenter cdp = new CustomDuplicatePresenter(abilities);
            cdp.ShowDialog();
            return cdp.SelectedItem as AbilityBuffInfos;
        }

        public static AbilityBuffCommands getSelectedAbility(List<AbilityBuffCommands> abilities)
        {
            CustomDuplicatePresenter cdp = new CustomDuplicatePresenter(abilities);
            cdp.ShowDialog();
            return cdp.SelectedItem as AbilityBuffCommands;
        }

        #region update
        private static string buildUpdateAbilityBuffInfosQuery(AbilityBuffInfos abilityBuffInfos, int Entry)
        {
            //there should be no buffinfos with duplicate entry.
            string query = $"update war_world.buff_infos set ";
            List<string> queryData = getAbilityBuffInfosUpdate(abilityBuffInfos);
            query = formatQuery(queryData, query);
            query += $" where Entry = '{Entry}'";
            return query;
        }

        private static string buildUpdateAbilityBuffCommandsQuery(AbilityBuffCommands abilityBuffCommands, int Entry)
        {
            string query = $"update war_world.buff_commands set ";
            List<string> queryData = getAbilityBuffCommandsUpdate(abilityBuffCommands);
            query = formatQuery(queryData, query);
            query += $" where Entry = '{Entry}' and CommandID = '{abilityBuffCommands.CommandID}'";
            return query;
        }

        private static string buildUpdateAbilityModifierChecksQuery(AbilityModifierChecks abilityModifierChecks, int Entry)
        {
            string query = $"update war_world.ability_modifier_checks set ";
            List<string> queryData = getAbilityModifierChecksUpdate(abilityModifierChecks);
            query = formatQuery(queryData, query);
            query += $" where Entry = '{Entry}' and PreOrPost = '{abilityModifierChecks.PreOrPost}' and ID = '{abilityModifierChecks.ID}' and Sequence = '{abilityModifierChecks.Sequence}'";
            return query;
        }

        private static string buildUpdateAbilityModifiersQuery(AbilityModifiers abilityModifiers, int Entry)
        {
            string query = $"update war_world.ability_modifiers set ";
            List<string> queryData = getAbilityModifiersUpdate(abilityModifiers);
            query = formatQuery(queryData, query);
            query += $" where Entry = '{Entry}' and Sequence = '{abilityModifiers.Sequence}'";
            return query;
        }

        private static string buildUpdateAbilityKnockBackInfoQuery(AbilityKnockBackInfo abilityKnockBackInfo, int Entry)
        {
            string query = $"update war_world.ability_knockback_info set ";
            List<string> queryData = getAbilityKnockbackInfoUpdate(abilityKnockBackInfo);
            query = formatQuery(queryData, query);
            query += $" where Entry = '{Entry}' and Id = '{abilityKnockBackInfo.Id}'";
            return query;
        }

        private static string buildUpdateAbilityDamageHealsQuery(AbilityDamageHeals abilityDamageHeals, int Entry)
        {
            string query = $"update war_world.ability_damage_heals set ";
            List<string> queryData = getAbilityDamageHealsQueryDataUpdate(abilityDamageHeals);
            query = formatQuery(queryData, query);
            query += $" where Entry = '{Entry}' and ParentCommandID = '{abilityDamageHeals.ParentCommandID}' and ParentCommandSequence = '{abilityDamageHeals.ParentCommandSequence}'";
            return query;
        }

        private static string buildUpdateAbilityCommandQuery(AbilityCommand abilityCommand, int Entry)
        {
            string query = $"update war_world.ability_commands set ";
            List<string> queryData = getAbilityCommandQueryDataUpdate(abilityCommand);
            query = formatQuery(queryData, query);
            query += $" where Entry = '{Entry}' and CommandID = '{abilityCommand.CommandID}' and CommandSequence = '{abilityCommand.CommandSequence}'";
            return query;
        }

        private static string buildUpdateAbilitySingleQuery(AbilitySingle abilitySingle, int Entry)
        {
            // there should be no abilities with duplicate entry.
            string query = $"update war_world.abilities set ";
            List<string> queryData = getAbilitySingleQueryDataUpdate(abilitySingle);
            query = formatQuery(queryData, query);
            query += $" where Entry = '{Entry}'";
            return query;
        }

        private static string formatQuery(List<string> queryData, string query)
        {
            queryData.ForEach(data =>
            {
                if (queryData.IndexOf(data) != 0)
                {
                    query += ", " + data;
                }
                else
                {
                    query += data;
                }
            });

            return query;
        }
        #endregion

        #region get queryData

        #region ability single
        private static List<string> getAbilitySingleQueryDataUpdate(AbilitySingle abilitySingle)
        {
            List<string> queryData = new List<string>();

            if (abilitySingle.toUpdateMembers.Contains("CareerLine"))
            {
                queryData.Add($"CareerLine = '{abilitySingle.CareerLine}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("MinRange"))
            {
                queryData.Add($"MinRange = '{abilitySingle.MinRange}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("Range"))
            {
                queryData.Add($"Range = '{abilitySingle.Range}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("CastTime"))
            {
                queryData.Add($"CastTime = '{abilitySingle.CastTime}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("Cooldown"))
            {
                queryData.Add($"Cooldown = '{abilitySingle.Cooldown}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("ApCost"))
            {
                queryData.Add($"ApCost = '{abilitySingle.ApCost}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("SpecialCost"))
            {
                queryData.Add($"SpecialCost = '{abilitySingle.SpecialCost}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("MoveCast"))
            {
                queryData.Add($"MoveCast = '{abilitySingle.MoveCast}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("InvokeDelay"))
            {
                queryData.Add($"InvokeDelay = '{abilitySingle.InvokeDelay}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("EffectDelay"))
            {
                queryData.Add($"EffectDelay = '{abilitySingle.EffectDelay}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("EffectID"))
            {
                queryData.Add($"EffectID = '{abilitySingle.EffectID}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("ChannelID"))
            {
                queryData.Add($"ChannelID = '{abilitySingle.ChannelID}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("CooldownEntry"))
            {
                queryData.Add($"CooldownEntry = '{abilitySingle.CooldownEntry}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("ToggleEntry"))
            {
                queryData.Add($"ToggleEntry = '{abilitySingle.ToggleEntry}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("CastAngle"))
            {
                queryData.Add($"CastAngle = '{abilitySingle.CastAngle}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("AbilityType"))
            {
                queryData.Add($"AbilityType = '{abilitySingle.AbilityType}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("MasteryTree"))
            {
                queryData.Add($"MasteryTree = '{abilitySingle.MasteryTree}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("WeaponNeeded"))
            {
                queryData.Add($"WeaponNeeded = '{abilitySingle.WeaponNeeded}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("AffectsDead"))
            {
                queryData.Add($"AffectsDead = '{abilitySingle.AffectsDead}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("IgnoreGlobalCooldown"))
            {
                queryData.Add($"IgnoreGlobalCooldown = '{abilitySingle.IgnoreGlobalCooldown}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("IgnoreOwnModifiers"))
            {
                queryData.Add($"IgnoreOwnModifiers = '{abilitySingle.IgnoreOwnModifiers}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("Fragile"))
            {
                queryData.Add($"Fragile = '{abilitySingle.Fragile}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("MinimumRank"))
            {
                queryData.Add($"MinimumRank = '{abilitySingle.MinimumRank}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("MinimumRenown"))
            {
                queryData.Add($"MinimumRenown = '{abilitySingle.MinimumRenown}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("IconId"))
            {
                queryData.Add($"IconId = '{abilitySingle.IconId}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("Category"))
            {
                queryData.Add($"Category = '{abilitySingle.Category}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("Flags"))
            {
                queryData.Add($"Flags = '{abilitySingle.Flags}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("PointCost"))
            {
                queryData.Add($"PointCost = '{abilitySingle.PointCost}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("CashCost"))
            {
                queryData.Add($"CashCost = '{abilitySingle.CashCost}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("StealthInteraction"))
            {
                queryData.Add($"StealthInteraction = '{abilitySingle.StealthInteraction}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("AIRange"))
            {
                queryData.Add($"AIRange = '{abilitySingle.AIRange}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("IgnoreCooldownReduction"))
            {
                queryData.Add($"IgnoreCooldownReduction = '{abilitySingle.IgnoreCooldownReduction}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("CDcap"))
            {
                queryData.Add($"CDcap = '{abilitySingle.CDcap}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("abilityID"))
            {
                queryData.Add($"abilityID = '{abilitySingle.abilityID}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("effectID2"))
            {
                queryData.Add($"effectID2 = '{abilitySingle.effectID2}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("Time"))
            {
                queryData.Add($"Time = '{abilitySingle.Time}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("Name"))
            {
                queryData.Add($"Name = '{abilitySingle.Name}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("Specline"))
            {
                queryData.Add($"Specline = '{abilitySingle.Specline}'");
            }

            if (abilitySingle.toUpdateMembers.Contains("VFXTarget"))
            {
                queryData.Add($"VFXTarget = '{abilitySingle.VFXTarget}'");
            }

            return queryData;
        }

        private static Query getAbilitySingleQueryDataInsert(AbilitySingle abilitySingle)
        {
            string[] cols = { };
            object[][] data = { };

            if (abilitySingle.toUpdateMembers.Contains("Entry"))
            {
                cols.SetValue("Entry", 1);
                data.SetValue(abilitySingle.Entry, 1);
            }

            if (abilitySingle.toUpdateMembers.Contains("CareerLine"))
            {
                cols.SetValue("CareerLine", cols.Length);
                data.SetValue(abilitySingle.CareerLine, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("MinRange"))
            {
                cols.SetValue("MinRange", cols.Length);
                data.SetValue(abilitySingle.MinRange, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("Range"))
            {
                cols.SetValue("Range", cols.Length);
                data.SetValue(abilitySingle.Range, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("CastTime"))
            {
                cols.SetValue("CastTime", cols.Length);
                data.SetValue(abilitySingle.CastTime, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("Cooldown"))
            {
                cols.SetValue("Cooldown", cols.Length);
                data.SetValue(abilitySingle.Cooldown, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("ApCost"))
            {
                cols.SetValue("ApCost", cols.Length);
                data.SetValue(abilitySingle.ApCost, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("SpecialCost"))
            {
                cols.SetValue("SpecialCost", cols.Length);
                data.SetValue(abilitySingle.SpecialCost, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("MoveCast"))
            {
                cols.SetValue("MoveCast", cols.Length);
                data.SetValue(abilitySingle.MoveCast, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("InvokeDelay"))
            {
                cols.SetValue("InvokeDelay", cols.Length);
                data.SetValue(abilitySingle.InvokeDelay, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("EffectDelay"))
            {
                cols.SetValue("EffectDelay", cols.Length);
                data.SetValue(abilitySingle.EffectDelay, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("EffectID"))
            {
                cols.SetValue("EffectID", cols.Length);
                data.SetValue(abilitySingle.EffectID, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("ChannelID"))
            {
                cols.SetValue("ChannelID", cols.Length);
                data.SetValue(abilitySingle.ChannelID, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("CooldownEntry"))
            {
                cols.SetValue("CooldownEntry", cols.Length);
                data.SetValue(abilitySingle.CooldownEntry, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("ToggleEntry"))
            {
                cols.SetValue("ToggleEntry", cols.Length);
                data.SetValue(abilitySingle.ToggleEntry, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("CastAngle"))
            {
                cols.SetValue("CastAngle", cols.Length);
                data.SetValue(abilitySingle.CastAngle, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("AbilityType"))
            {
                cols.SetValue("AbilityType", cols.Length);
                data.SetValue(abilitySingle.AbilityType, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("MasteryTree"))
            {
                cols.SetValue("MasteryTree", cols.Length);
                data.SetValue(abilitySingle.MasteryTree, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("WeaponNeeded"))
            {
                cols.SetValue("WeaponNeeded", cols.Length);
                data.SetValue(abilitySingle.WeaponNeeded, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("AffectsDead"))
            {
                cols.SetValue("AffectsDead", cols.Length);
                data.SetValue(abilitySingle.AffectsDead, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("IgnoreGlobalCooldown"))
            {
                cols.SetValue("IgnoreGlobalCooldown", cols.Length);
                data.SetValue(abilitySingle.IgnoreGlobalCooldown, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("IgnoreOwnModifiers"))
            {
                cols.SetValue("IgnoreOwnModifiers", cols.Length);
                data.SetValue(abilitySingle.IgnoreOwnModifiers, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("Fragile"))
            {
                cols.SetValue("Fragile", cols.Length);
                data.SetValue(abilitySingle.Fragile, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("MinimumRank"))
            {
                cols.SetValue("MinimumRank", cols.Length);
                data.SetValue(abilitySingle.MinimumRank, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("MinimumRenown"))
            {
                cols.SetValue("MinimumRenown", cols.Length);
                data.SetValue(abilitySingle.MinimumRenown, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("IconId"))
            {
                cols.SetValue("IconId", cols.Length);
                data.SetValue(abilitySingle.IconId, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("Category"))
            {
                cols.SetValue("Category", cols.Length);
                data.SetValue(abilitySingle.Category, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("Flags"))
            {
                cols.SetValue("Flags", cols.Length);
                data.SetValue(abilitySingle.Flags, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("PointCost"))
            {
                cols.SetValue("PointCost", cols.Length);
                data.SetValue(abilitySingle.PointCost, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("CashCost"))
            {
                cols.SetValue("CashCost", cols.Length);
                data.SetValue(abilitySingle.CashCost, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("StealthInteraction"))
            {
                cols.SetValue("StealthInteraction", cols.Length);
                data.SetValue(abilitySingle.StealthInteraction, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("AIRange"))
            {
                cols.SetValue("AIRange", cols.Length);
                data.SetValue(abilitySingle.AIRange, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("IgnoreCooldownReduction"))
            {
                cols.SetValue("IgnoreCooldownReduction", cols.Length);
                data.SetValue(abilitySingle.IgnoreCooldownReduction, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("CDcap"))
            {
                cols.SetValue("CDcap", cols.Length);
                data.SetValue(abilitySingle.CDcap, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("abilityID"))
            {
                cols.SetValue("abilityID", cols.Length);
                data.SetValue(abilitySingle.abilityID, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("effectID2"))
            {
                cols.SetValue("effectID2", cols.Length);
                data.SetValue(abilitySingle.effectID2, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("Time"))
            {
                cols.SetValue("Time", cols.Length);
                data.SetValue(abilitySingle.Time, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("Name"))
            {
                cols.SetValue("Name", cols.Length);
                data.SetValue(abilitySingle.Name, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("Specline"))
            {
                cols.SetValue("Specline", cols.Length);
                data.SetValue(abilitySingle.Specline, data.Length);
            }

            if (abilitySingle.toUpdateMembers.Contains("VFXTarget"))
            {
                cols.SetValue("VFXTarget", cols.Length);
                data.SetValue(abilitySingle.VFXTarget, data.Length);
            }

            return new Query("war_world.abilities").AsInsert(cols, data);
        }
        #endregion

        #region ability Command
        private static List<string> getAbilityCommandQueryDataUpdate(AbilityCommand abilityCommand)
        {
            List<string> queryData = new List<string>();

            if (abilityCommand.toUpdateMembers.Contains("Entry"))
            {
                queryData.Add($"Entry = '{ abilityCommand.Entry }'");
            }

            if (abilityCommand.toUpdateMembers.Contains("AbilityName"))
            {
                queryData.Add($"AbilityName = '{abilityCommand.AbilityName}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("CommandID"))
            {
                queryData.Add($"CommandID = '{abilityCommand.CommandID}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("CommandSequence"))
            {
                queryData.Add($"CommandSequence = '{abilityCommand.CommandSequence}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("CommandName"))
            {
                queryData.Add($"CommandName = '{abilityCommand.CommandName}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("PrimaryValue"))
            {
                queryData.Add($"PrimaryValue = '{abilityCommand.PrimaryValue}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("SecondaryValue"))
            {
                queryData.Add($"SecondaryValue = '{abilityCommand.SecondaryValue}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("Target"))
            {
                queryData.Add($"Target = '{abilityCommand.Target}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("EffectRadius"))
            {
                queryData.Add($"EffectRadius = '{abilityCommand.EffectRadius}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("EffectAngle"))
            {
                queryData.Add($"EffectAngle = '{abilityCommand.EffectAngle}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("EffectSource"))
            {
                queryData.Add($"EffectSource = '{abilityCommand.EffectSource}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("FromAllTargets"))
            {
                queryData.Add($"FromAllTargets = '{abilityCommand.FromAllTargets}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("MaxTargets"))
            {
                queryData.Add($"MaxTargets = '{abilityCommand.MaxTargets}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("AttackingStat"))
            {
                queryData.Add($"AttackingStat = '{abilityCommand.AttackingStat}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("isDelayedEffect"))
            {
                queryData.Add($"isDelayedEffect = '{abilityCommand.isDelayedEffect}'");
            }

            if (abilityCommand.toUpdateMembers.Contains("NoAutoUse"))
            {
                queryData.Add($"NoAutoUse = '{abilityCommand.NoAutoUse}'");
            }
            return queryData;
        }

        private static Query getAbilityCommandQueryDataInsert(AbilityCommand abilityCommand)
        {
            string[] cols = { };
            object[][] data = { };

            if (abilityCommand.toUpdateMembers.Contains("Entry"))
            {
                cols.SetValue("Entry", cols.Length);
                data.SetValue(abilityCommand.Entry, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("AbilityName"))
            {
                cols.SetValue("AbilityName", cols.Length);
                data.SetValue(abilityCommand.AbilityName, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("CommandID"))
            {
                cols.SetValue("CommandID", cols.Length);
                data.SetValue(abilityCommand.CommandID, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("CommandSequence"))
            {
                cols.SetValue("CommandSequence", cols.Length);
                data.SetValue(abilityCommand.CommandSequence, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("CommandName"))
            {
                cols.SetValue("CommandName", cols.Length);
                data.SetValue(abilityCommand.CommandName, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("PrimaryValue"))
            {
                cols.SetValue("PrimaryValue", cols.Length);
                data.SetValue(abilityCommand.PrimaryValue, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("SecondaryValue"))
            {
                cols.SetValue("SecondaryValue", cols.Length);
                data.SetValue(abilityCommand.SecondaryValue, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("Target"))
            {
                cols.SetValue("Target", cols.Length);
                data.SetValue(abilityCommand.Target, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("EffectRadius"))
            {
                cols.SetValue("EffectRadius", cols.Length);
                data.SetValue(abilityCommand.EffectRadius, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("EffectAngle"))
            {
                cols.SetValue("EffectAngle", cols.Length);
                data.SetValue(abilityCommand.EffectAngle, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("EffectSource"))
            {
                cols.SetValue("EffectSource", cols.Length);
                data.SetValue(abilityCommand.EffectSource, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("FromAllTargets"))
            {
                cols.SetValue("FromAllTargets", cols.Length);
                data.SetValue(abilityCommand.FromAllTargets, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("MaxTargets"))
            {
                cols.SetValue("MaxTargets", cols.Length);
                data.SetValue(abilityCommand.MaxTargets, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("AttackingStat"))
            {
                cols.SetValue("AttackingStat", cols.Length);
                data.SetValue(abilityCommand.AttackingStat, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("isDelayedEffect"))
            {
                cols.SetValue("isDelayedEffect", cols.Length);
                data.SetValue(abilityCommand.isDelayedEffect, data.Length);
            }

            if (abilityCommand.toUpdateMembers.Contains("NoAutoUse"))
            {
                cols.SetValue("NoAutoUse", cols.Length);
                data.SetValue(abilityCommand.NoAutoUse, data.Length);
            }

            return new Query("war_world.ability_commands").AsInsert(cols, data);
        }
        #endregion

        #region ability dmgheals
        private static List<string> getAbilityDamageHealsQueryDataUpdate(AbilityDamageHeals abilityDamageHeals)
        {
            List<string> queryData = new List<string>();

            if (abilityDamageHeals.ToUpdateMembers.Contains("Entry"))
            {
                queryData.Add($"Entry = '{ abilityDamageHeals.Entry }'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("DisplayEntry"))
            {
                queryData.Add($"DisplayEntry = '{abilityDamageHeals.DisplayEntry}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("Index"))
            {
                queryData.Add($"war_world.ability_damage_heals.Index = '{abilityDamageHeals.Index}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("Name"))
            {
                queryData.Add($"war_world.ability_damage_heals.Name = '{abilityDamageHeals.Name}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("MinDamage"))
            {
                queryData.Add($"MinDamage = '{abilityDamageHeals.MinDamage}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("MaxDamage"))
            {
                queryData.Add($"MaxDamage = '{abilityDamageHeals.MaxDamage}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("DamageVariance"))
            {
                queryData.Add($"DamageVariance = '{abilityDamageHeals.DamageVariance}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("DamageType"))
            {
                queryData.Add($"DamageType = '{abilityDamageHeals.DamageType}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("ParentCommandID"))
            {
                queryData.Add($"ParentCommandID = '{abilityDamageHeals.ParentCommandID}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("ParentCommandSequence"))
            {
                queryData.Add($"ParentCommandSequence = '{abilityDamageHeals.ParentCommandSequence}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("CastTimeDamageMult"))
            {
                queryData.Add($"CastTimeDamageMult = '{abilityDamageHeals.CastTimeDamageMult}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("WeaponDamageFrom"))
            {
                queryData.Add($"WeaponDamageFrom = '{abilityDamageHeals.WeaponDamageFrom}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("WeaponDamageScale"))
            {
                queryData.Add($"WeaponDamageScale = '{abilityDamageHeals.WeaponDamageScale}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("NoCrits"))
            {
                queryData.Add($"NoCrits = '{abilityDamageHeals.NoCrits}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("Undefendable"))
            {
                queryData.Add($"Undefendable = '{abilityDamageHeals.Undefendable}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("OverrideDefenseEvent"))
            {
                queryData.Add($"OverrideDefenseEvent = '{abilityDamageHeals.OverrideDefenseEvent}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("StatUsed"))
            {
                queryData.Add($"StatUsed = '{abilityDamageHeals.StatUsed}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("StatDamageScale"))
            {
                queryData.Add($"StatDamageScale = '{abilityDamageHeals.StatDamageScale}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("RessourceBuild"))
            {
                queryData.Add($"RessourceBuild = '{abilityDamageHeals.RessourceBuild}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("CastPlayerSubID"))
            {
                queryData.Add($"CastPlayerSubID = '{abilityDamageHeals.CastPlayerSubID}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("ArmorResistPenFactor"))
            {
                queryData.Add($"ArmorResistPenFactor = '{abilityDamageHeals.ArmorResistPenFactor}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("HatredScale"))
            {
                queryData.Add($"HatredScale = '{abilityDamageHeals.HatredScale}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("HealHatredScale"))
            {
                queryData.Add($"HealHatredScale = '{abilityDamageHeals.HealHatredScale}'");
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("PriStatMultiplier"))
            {
                queryData.Add($"PriStatMultiplier = '{abilityDamageHeals.PriStatMultiplier}'");
            }


            return queryData;
        }

        private static Query getAbilityDamageHealsQueryDataInsert(AbilityDamageHeals abilityDamageHeals)
        {
            string[] cols = { };
            object[][] data = { };

            if (abilityDamageHeals.ToUpdateMembers.Contains("Entry"))
            {
                cols.SetValue("Entry", cols.Length);
                data.SetValue(abilityDamageHeals.Entry, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("DisplayEntry"))
            {
                cols.SetValue("DisplayEntry", cols.Length);
                data.SetValue(abilityDamageHeals.DisplayEntry, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("Index"))
            {
                cols.SetValue("war_world.ability_damage_heals.Index", cols.Length);
                data.SetValue(abilityDamageHeals.Index, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("Name"))
            {
                cols.SetValue("war_world.ability_damage_heals.Name", cols.Length);
                data.SetValue(abilityDamageHeals.Name, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("MinDamage"))
            {
                cols.SetValue("MinDamage", cols.Length);
                data.SetValue(abilityDamageHeals.MinDamage, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("MaxDamage"))
            {
                cols.SetValue("MaxDamage", cols.Length);
                data.SetValue(abilityDamageHeals.MaxDamage, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("DamageVariance"))
            {
                cols.SetValue("DamageVariance", cols.Length);
                data.SetValue(abilityDamageHeals.DamageVariance, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("DamageType"))
            {
                cols.SetValue("DamageType", cols.Length);
                data.SetValue(abilityDamageHeals.DamageType, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("ParentCommandID"))
            {
                cols.SetValue("ParentCommandID", cols.Length);
                data.SetValue(abilityDamageHeals.ParentCommandID, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("ParentCommandSequence"))
            {
                cols.SetValue("ParentCommandSequence", cols.Length);
                data.SetValue(abilityDamageHeals.ParentCommandSequence, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("CastTimeDamageMult"))
            {
                cols.SetValue("CastTimeDamageMult", cols.Length);
                data.SetValue(abilityDamageHeals.CastTimeDamageMult, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("WeaponDamageFrom"))
            {
                cols.SetValue("WeaponDamageFrom", cols.Length);
                data.SetValue(abilityDamageHeals.WeaponDamageFrom, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("WeaponDamageScale"))
            {
                cols.SetValue("WeaponDamageScale", cols.Length);
                data.SetValue(abilityDamageHeals.WeaponDamageScale, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("NoCrits"))
            {
                cols.SetValue("NoCrits", cols.Length);
                data.SetValue(abilityDamageHeals.NoCrits, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("Undefendable"))
            {
                cols.SetValue("Undefendable", cols.Length);
                data.SetValue(abilityDamageHeals.Undefendable, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("OverrideDefenseEvent"))
            {
                cols.SetValue("OverrideDefenseEvent", cols.Length);
                data.SetValue(abilityDamageHeals.OverrideDefenseEvent, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("StatUsed"))
            {
                cols.SetValue("StatUsed", cols.Length);
                data.SetValue(abilityDamageHeals.StatUsed, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("StatDamageScale"))
            {
                cols.SetValue("StatDamageScale", cols.Length);
                data.SetValue(abilityDamageHeals.StatDamageScale, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("RessourceBuild"))
            {
                cols.SetValue("RessourceBuild", cols.Length);
                data.SetValue(abilityDamageHeals.RessourceBuild, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("CastPlayerSubID"))
            {
                cols.SetValue("CastPlayerSubID", cols.Length);
                data.SetValue(abilityDamageHeals.CastPlayerSubID, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("ArmorResistPenFactor"))
            {
                cols.SetValue("ArmorResistPenFactor", cols.Length);
                data.SetValue(abilityDamageHeals.ArmorResistPenFactor, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("HatredScale"))
            {
                cols.SetValue("HatredScale", cols.Length);
                data.SetValue(abilityDamageHeals.HatredScale, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("HealHatredScale"))
            {
                cols.SetValue("HealHatredScale", cols.Length);
                data.SetValue(abilityDamageHeals.HealHatredScale, data.Length);
            }

            if (abilityDamageHeals.ToUpdateMembers.Contains("PriStatMultiplier"))
            {
                cols.SetValue("PriStatMultiplier", cols.Length);
                data.SetValue(abilityDamageHeals.PriStatMultiplier, data.Length);
            }

            return new Query("war_world.ability_damage_heals").AsInsert(cols, data);
        }
        #endregion

        #region ability knockback
        private static List<string> getAbilityKnockbackInfoUpdate(AbilityKnockBackInfo abilityKnockBackInfo)
        {
            List<string> queryData = new List<string>();

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Entry"))
            {
                queryData.Add($"Entry = '{ abilityKnockBackInfo.Entry }'");
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Id"))
            {
                queryData.Add($"war_world.ability_knockback_info.Id = '{abilityKnockBackInfo.Id}'");
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Angle"))
            {
                queryData.Add($"Angle = '{abilityKnockBackInfo.Angle}'");
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Power"))
            {
                queryData.Add($"Power = '{abilityKnockBackInfo.Power}'");
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("RangeExtension"))
            {
                queryData.Add($"RangeExtension = '{abilityKnockBackInfo.RangeExtension}'");
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("GravMultiplier"))
            {
                queryData.Add($"GravMultiplier = '{abilityKnockBackInfo.GravMultiplier}'");
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Unk"))
            {
                queryData.Add($"Unk = '{abilityKnockBackInfo.Unk}'");
            }

            return queryData;
        }

        private static Query getAbilityKnockbackInfoInsert(AbilityKnockBackInfo abilityKnockBackInfo)
        {
            string[] cols = { };
            object[][] data = { };

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Entry"))
            {
                cols.SetValue("Entry", cols.Length);
                data.SetValue(abilityKnockBackInfo.Entry, data.Length);
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Id"))
            {
                cols.SetValue("war_world.ability_knockback_info.Id", cols.Length);
                data.SetValue(abilityKnockBackInfo.Id, data.Length);
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Angle"))
            {
                cols.SetValue("Angle", cols.Length);
                data.SetValue(abilityKnockBackInfo.Angle, data.Length);
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Power"))
            {
                cols.SetValue("Power", cols.Length);
                data.SetValue(abilityKnockBackInfo.Power, data.Length);
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("RangeExtension"))
            {
                cols.SetValue("RangeExtension", cols.Length);
                data.SetValue(abilityKnockBackInfo.RangeExtension, data.Length);
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("GravMultiplier"))
            {
                cols.SetValue("GravMultiplier", cols.Length);
                data.SetValue(abilityKnockBackInfo.GravMultiplier, data.Length);
            }

            if (abilityKnockBackInfo.ToUpdateMembers.Contains("Unk"))
            {
                cols.SetValue("Unk", cols.Length);
                data.SetValue(abilityKnockBackInfo.Unk, data.Length);
            }

            return new Query("war_world.ability_knockback_info").AsInsert(cols, data);
        }
        #endregion

        #region ability modchecks
        private static List<string> getAbilityModifierChecksUpdate(AbilityModifierChecks abilityModifierChecks)
        {
            List<string> queryData = new List<string>();

            if (abilityModifierChecks.toUpdateMembers.Contains("Entry"))
            {
                queryData.Add($"Entry = '{ abilityModifierChecks.Entry }'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("SourceAbility"))
            {
                queryData.Add($"SourceAbility = '{abilityModifierChecks.SourceAbility}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("Affecting"))
            {
                queryData.Add($"Affecting = '{abilityModifierChecks.Affecting}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("AffectedAbility"))
            {
                queryData.Add($"AffectedAbility = '{abilityModifierChecks.AffectedAbility}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("PreOrPost"))
            {
                queryData.Add($"PreOrPost = '{abilityModifierChecks.PreOrPost}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("ID"))
            {
                queryData.Add($"war_world.ability_modifier_checks.ID = '{abilityModifierChecks.ID}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("Sequence"))
            {
                queryData.Add($"Sequence = '{abilityModifierChecks.Sequence}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("CommandName"))
            {
                queryData.Add($"CommandName = '{abilityModifierChecks.CommandName}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("FailCode"))
            {
                queryData.Add($"FailCode = '{abilityModifierChecks.FailCode}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("PrimaryValue"))
            {
                queryData.Add($"PrimaryValue = '{abilityModifierChecks.PrimaryValue}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("SecondaryValue"))
            {
                queryData.Add($"SecondaryValue = '{abilityModifierChecks.SecondaryValue}'");
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("ability_modifier_checks_ID"))
            {
                queryData.Add($"ability_modifier_checks_ID = '{abilityModifierChecks.ability_modifier_checks_ID}'");
            }

            return queryData;
        }

        private static Query getAbilityModifierChecksInsert(AbilityModifierChecks abilityModifierChecks)
        {
            string[] cols = { };
            object[][] data = { };

            if (abilityModifierChecks.toUpdateMembers.Contains("Entry"))
            {
                cols.SetValue("Entry", cols.Length);
                data.SetValue(abilityModifierChecks.Entry, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("SourceAbility"))
            {
                cols.SetValue("SourceAbility", cols.Length);
                data.SetValue(abilityModifierChecks.SourceAbility, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("Affecting"))
            {
                cols.SetValue("Affecting", cols.Length);
                data.SetValue(abilityModifierChecks.Affecting, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("AffectedAbility"))
            {
                cols.SetValue("AffectedAbility", cols.Length);
                data.SetValue(abilityModifierChecks.AffectedAbility, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("PreOrPost"))
            {
                cols.SetValue("PreOrPost", cols.Length);
                data.SetValue(abilityModifierChecks.PreOrPost, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("ID"))
            {
                cols.SetValue("war_world.ability_modifier_checks.ID", cols.Length);
                data.SetValue(abilityModifierChecks.ID, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("Sequence"))
            {
                cols.SetValue("Sequence", cols.Length);
                data.SetValue(abilityModifierChecks.Sequence, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("CommandName"))
            {
                cols.SetValue("CommandName", cols.Length);
                data.SetValue(abilityModifierChecks.CommandName, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("FailCode"))
            {
                cols.SetValue("FailCode", cols.Length);
                data.SetValue(abilityModifierChecks.FailCode, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("PrimaryValue"))
            {
                cols.SetValue("PrimaryValue", cols.Length);
                data.SetValue(abilityModifierChecks.PrimaryValue, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("SecondaryValue"))
            {
                cols.SetValue("SecondaryValue", cols.Length);
                data.SetValue(abilityModifierChecks.SecondaryValue, data.Length);
            }

            if (abilityModifierChecks.toUpdateMembers.Contains("ability_modifier_checks_ID"))
            {
                cols.SetValue("ability_modifier_checks_ID", cols.Length);
                data.SetValue(abilityModifierChecks.ability_modifier_checks_ID, data.Length);
            }

            return new Query("war_world.ability_modifier_checks").AsInsert(cols, data);
        }
        #endregion

        #region ability modifiers
        private static List<string> getAbilityModifiersUpdate(AbilityModifiers abilityModifiers)
        {
            List<string> queryData = new List<string>();

            if (abilityModifiers.toUpdateMembers.Contains("Entry"))
            {
                queryData.Add($"Entry = '{ abilityModifiers.Entry }'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("SourceAbility"))
            {
                queryData.Add($"SourceAbility = '{abilityModifiers.SourceAbility}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("Affecting"))
            {
                queryData.Add($"Affecting = '{abilityModifiers.Affecting}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("AffectedAbility"))
            {
                queryData.Add($"AffectedAbility = '{abilityModifiers.AffectedAbility}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("PreOrPost"))
            {
                queryData.Add($"PreOrPost = '{abilityModifiers.PreOrPost}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("Sequence"))
            {
                queryData.Add($"Sequence = '{abilityModifiers.Sequence}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("ModifierCommandName"))
            {
                queryData.Add($"ModifierCommandName = '{abilityModifiers.ModifierCommandName}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("TargetCommandID"))
            {
                queryData.Add($"TargetCommandID = '{abilityModifiers.TargetCommandID}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("TargetCommandSequence"))
            {
                queryData.Add($"TargetCommandSequence = '{abilityModifiers.TargetCommandSequence}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("PrimaryValue"))
            {
                queryData.Add($"PrimaryValue = '{abilityModifiers.PrimaryValue}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("SecondaryValue"))
            {
                queryData.Add($"SecondaryValue = '{abilityModifiers.SecondaryValue}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("BuffLine"))
            {
                queryData.Add($"BuffLine = '{abilityModifiers.BuffLine}'");
            }

            if (abilityModifiers.toUpdateMembers.Contains("ability_modifiers_ID"))
            {
                queryData.Add($"ability_modifiers_ID = '{abilityModifiers.ability_modifiers_ID}'");
            }

            return queryData;
        }

        private static Query getAbilityModifiersInsert(AbilityModifiers abilityModifiers)
        {
            string[] cols = { };
            object[][] data = { };

            if (abilityModifiers.toUpdateMembers.Contains("Entry"))
            {
                cols.SetValue("Entry", cols.Length);
                data.SetValue(abilityModifiers.Entry, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("SourceAbility"))
            {
                cols.SetValue("SourceAbility", cols.Length);
                data.SetValue(abilityModifiers.SourceAbility, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("Affecting"))
            {
                cols.SetValue("Affecting", cols.Length);
                data.SetValue(abilityModifiers.Affecting, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("AffectedAbility"))
            {
                cols.SetValue("AffectedAbility", cols.Length);
                data.SetValue(abilityModifiers.AffectedAbility, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("PreOrPost"))
            {
                cols.SetValue("PreOrPost", cols.Length);
                data.SetValue(abilityModifiers.PreOrPost, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("Sequence"))
            {
                cols.SetValue("Sequence", cols.Length);
                data.SetValue(abilityModifiers.Sequence, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("ModifierCommandName"))
            {
                cols.SetValue("ModifierCommandName", cols.Length);
                data.SetValue(abilityModifiers.ModifierCommandName, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("TargetCommandID"))
            {
                cols.SetValue("TargetCommandID", cols.Length);
                data.SetValue(abilityModifiers.TargetCommandID, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("TargetCommandSequence"))
            {
                cols.SetValue("TargetCommandSequence", cols.Length);
                data.SetValue(abilityModifiers.TargetCommandSequence, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("PrimaryValue"))
            {
                cols.SetValue("PrimaryValue", cols.Length);
                data.SetValue(abilityModifiers.PrimaryValue, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("SecondaryValue"))
            {
                cols.SetValue("EnSecondaryValuetry", cols.Length);
                data.SetValue(abilityModifiers.SecondaryValue, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("BuffLine"))
            {
                cols.SetValue("BuffLine", cols.Length);
                data.SetValue(abilityModifiers.BuffLine, data.Length);
            }

            if (abilityModifiers.toUpdateMembers.Contains("ability_modifiers_ID"))
            {
                cols.SetValue("ability_modifiers_ID", cols.Length);
                data.SetValue(abilityModifiers.ability_modifiers_ID, data.Length);
            }

            return new Query("war_world.ability_modifiers").AsInsert(cols, data);
        }
        #endregion

        #region ability buffInfos
        private static List<string> getAbilityBuffInfosUpdate(AbilityBuffInfos abilityBuffInfos)
        {
            List<string> queryData = new List<string>();

            if (abilityBuffInfos.toUpdateMembers.Contains("Entry"))
            {
                queryData.Add($"Entry = '{abilityBuffInfos.Entry}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Name"))
            {
                queryData.Add($"war_world.buff_infos.Name = '{abilityBuffInfos.Name}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("BuffClassString"))
            {
                queryData.Add($"BuffClassString = '{abilityBuffInfos.BuffClassString}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("TypeString"))
            {
                queryData.Add($"TypeString = '{abilityBuffInfos.TypeString}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Group"))
            {
                queryData.Add($"war_world.buff_infos.Group = '{abilityBuffInfos.Group}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("AuraPropagation"))
            {
                queryData.Add($"AuraPropagation = '{abilityBuffInfos.AuraPropagation}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("MaxCopies"))
            {
                queryData.Add($"MaxCopies = '{abilityBuffInfos.MaxCopies}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("MaxStack"))
            {
                queryData.Add($"MaxStack = '{abilityBuffInfos.MaxStack}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("UserMaxStackAsInitial"))
            {
                queryData.Add($"UserMaxStackAsInitial = '{abilityBuffInfos.UserMaxStackAsInitial}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("StackLine"))
            {
                queryData.Add($"StackLine = '{abilityBuffInfos.StackLine}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("StacksFromCaster"))
            {
                queryData.Add($"StacksFromCaster = '{abilityBuffInfos.StacksFromCaster}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Duration"))
            {
                queryData.Add($"Duration = '{abilityBuffInfos.Duration}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("LeadInDelay"))
            {
                queryData.Add($"LeadInDelay = '{abilityBuffInfos.LeadInDelay}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Interval"))
            {
                queryData.Add($"Interval = '{abilityBuffInfos.Interval}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("PersistsOnDeath"))
            {
                queryData.Add($"PersistsOnDeath = '{abilityBuffInfos.PersistsOnDeath}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("CanRefresh"))
            {
                queryData.Add($"CanRefresh = '{abilityBuffInfos.CanRefresh}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("FriendlyEffectID"))
            {
                queryData.Add($"FriendlyEffectID = '{abilityBuffInfos.FriendlyEffectID}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("EnemyEffectID"))
            {
                queryData.Add($"EnemyEffectID = '{abilityBuffInfos.EnemyEffectID}'");
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Silent"))
            {
                queryData.Add($"Silent = '{abilityBuffInfos.Silent}'");
            }

            return queryData;
        }

        private static Query getAbilityBuffInfosInsert(AbilityBuffInfos abilityBuffInfos)
        {
            string[] cols = { };
            object[][] data = { };

            if (abilityBuffInfos.toUpdateMembers.Contains("Entry"))
            {
                cols.SetValue("Entry", cols.Length);
                data.SetValue(abilityBuffInfos.Entry, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Name"))
            {
                cols.SetValue("war_world.buff_infos.Name", cols.Length);
                data.SetValue(abilityBuffInfos.Name, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("BuffClassString"))
            {
                cols.SetValue("BuffClassString", cols.Length);
                data.SetValue(abilityBuffInfos.BuffClassString, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("TypeString"))
            {
                cols.SetValue("TypeString", cols.Length);
                data.SetValue(abilityBuffInfos.TypeString, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Group"))
            {
                cols.SetValue("war_world.buff_infos.Group", cols.Length);
                data.SetValue(abilityBuffInfos.Group, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("AuraPropagation"))
            {
                cols.SetValue("AuraPropagation", cols.Length);
                data.SetValue(abilityBuffInfos.AuraPropagation, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("MaxCopies"))
            {
                cols.SetValue("MaxCopies", cols.Length);
                data.SetValue(abilityBuffInfos.MaxCopies, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("MaxStack"))
            {
                cols.SetValue("MaxStack", cols.Length);
                data.SetValue(abilityBuffInfos.MaxStack, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("UserMaxStackAsInitial"))
            {
                cols.SetValue("UserMaxStackAsInitial", cols.Length);
                data.SetValue(abilityBuffInfos.UserMaxStackAsInitial, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("StackLine"))
            {
                cols.SetValue("StackLine", cols.Length);
                data.SetValue(abilityBuffInfos.StackLine, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("StacksFromCaster"))
            {
                cols.SetValue("StacksFromCaster", cols.Length);
                data.SetValue(abilityBuffInfos.StacksFromCaster, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Duration"))
            {
                cols.SetValue("Duration", cols.Length);
                data.SetValue(abilityBuffInfos.Duration, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("LeadInDelay"))
            {
                cols.SetValue("LeadInDelay", cols.Length);
                data.SetValue(abilityBuffInfos.LeadInDelay, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Interval"))
            {
                cols.SetValue("Interval", cols.Length);
                data.SetValue(abilityBuffInfos.Interval, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("PersistsOnDeath"))
            {
                cols.SetValue("PersistsOnDeath", cols.Length);
                data.SetValue(abilityBuffInfos.PersistsOnDeath, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("CanRefresh"))
            {
                cols.SetValue("CanRefresh", cols.Length);
                data.SetValue(abilityBuffInfos.CanRefresh, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("FriendlyEffectID"))
            {
                cols.SetValue("FriendlyEffectID", cols.Length);
                data.SetValue(abilityBuffInfos.FriendlyEffectID, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("EnemyEffectID"))
            {
                cols.SetValue("EnemyEffectID", cols.Length);
                data.SetValue(abilityBuffInfos.EnemyEffectID, data.Length);
            }

            if (abilityBuffInfos.toUpdateMembers.Contains("Silent"))
            {
                cols.SetValue("Silent", cols.Length);
                data.SetValue(abilityBuffInfos.Silent, data.Length);
            }

            return new Query("war_world.buff_infos").AsInsert(cols, data);
        }
        #endregion

        #region ability buffcommands
        private static List<string> getAbilityBuffCommandsUpdate(AbilityBuffCommands abilityBuffCommands)
        {
            List<string> queryData = new List<string>();

            if (abilityBuffCommands.toUpdateMembers.Contains("Entry"))
            {
                queryData.Add($"Entry = '{abilityBuffCommands.Entry}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("Name"))
            {
                queryData.Add($"war_world.buff_commands.Name = '{abilityBuffCommands.Name}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("CommandID"))
            {
                queryData.Add($"CommandID = '{abilityBuffCommands.CommandID}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("CommandSequence"))
            {
                queryData.Add($"CommandSequence = '{abilityBuffCommands.CommandSequence}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("CommandName"))
            {
                queryData.Add($"CommandName = '{abilityBuffCommands.CommandName}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("PrimaryValue"))
            {
                queryData.Add($"PrimaryValue = '{abilityBuffCommands.PrimaryValue}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("SecondaryValue"))
            {
                queryData.Add($"SecondaryValue = '{abilityBuffCommands.SecondaryValue}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("TertiaryValue"))
            {
                queryData.Add($"TertiaryValue = '{abilityBuffCommands.TertiaryValue}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("InvokeOn"))
            {
                queryData.Add($"InvokeOn = '{abilityBuffCommands.InvokeOn}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("Target"))
            {
                queryData.Add($"Target = '{abilityBuffCommands.Target}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EffectSource"))
            {
                queryData.Add($"EffectSource = '{abilityBuffCommands.EffectSource}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EffectRadius"))
            {
                queryData.Add($"EffectRadius = '{abilityBuffCommands.EffectRadius}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EffectAngle"))
            {
                queryData.Add($"EffectAngle = '{abilityBuffCommands.EffectAngle}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("MaxTargets"))
            {
                queryData.Add($"MaxTargets = '{abilityBuffCommands.MaxTargets}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EventIDString"))
            {
                queryData.Add($"EventIDString = '{abilityBuffCommands.EventIDString}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EventCheck"))
            {
                queryData.Add($"EventCheck = '{abilityBuffCommands.EventCheck}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EventCheckParam"))
            {
                queryData.Add($"EventCheckParam = '{abilityBuffCommands.EventCheckParam}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EventChance"))
            {
                queryData.Add($"EventChance = '{abilityBuffCommands.EventChance}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("ConsumesStack"))
            {
                queryData.Add($"ConsumesStack = '{abilityBuffCommands.ConsumesStack}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("RetriggerInterval"))
            {
                queryData.Add($"RetriggerInterval = '{abilityBuffCommands.RetriggerInterval}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("BuffLine"))
            {
                queryData.Add($"BuffLine = '{abilityBuffCommands.BuffLine}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("NoAutoUse"))
            {
                queryData.Add($"NoAutoUse = '{abilityBuffCommands.NoAutoUse}'");
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("BuffClassString"))
            {
                queryData.Add($"BuffClassString = '{abilityBuffCommands.BuffClassString}'");
            }

            return queryData;
        }

        private static Query getAbilityBuffCommandsInsert(AbilityBuffCommands abilityBuffCommands)
        {
            string[] cols = { };
            object[][] data = { };

            if (abilityBuffCommands.toUpdateMembers.Contains("Entry"))
            {
                cols.SetValue("Entry", cols.Length);
                data.SetValue(abilityBuffCommands.Entry, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("Name"))
            {
                cols.SetValue("war_world.buff_commands.Name", cols.Length);
                data.SetValue(abilityBuffCommands.Name, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("CommandID"))
            {
                cols.SetValue("CommandID", cols.Length);
                data.SetValue(abilityBuffCommands.CommandID, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("CommandSequence"))
            {
                cols.SetValue("CommandSequence", cols.Length);
                data.SetValue(abilityBuffCommands.CommandSequence, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("CommandName"))
            {
                cols.SetValue("CommandName", cols.Length);
                data.SetValue(abilityBuffCommands.CommandName, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("PrimaryValue"))
            {
                cols.SetValue("PrimaryValue", cols.Length);
                data.SetValue(abilityBuffCommands.PrimaryValue, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("SecondaryValue"))
            {
                cols.SetValue("SecondaryValue", cols.Length);
                data.SetValue(abilityBuffCommands.SecondaryValue, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("TertiaryValue"))
            {
                cols.SetValue("TertiaryValue", cols.Length);
                data.SetValue(abilityBuffCommands.TertiaryValue, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("InvokeOn"))
            {
                cols.SetValue("InvokeOn", cols.Length);
                data.SetValue(abilityBuffCommands.InvokeOn, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("Target"))
            {
                cols.SetValue("Target", cols.Length);
                data.SetValue(abilityBuffCommands.Target, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EffectSource"))
            {
                cols.SetValue("EffectSource", cols.Length);
                data.SetValue(abilityBuffCommands.EffectSource, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EffectRadius"))
            {
                cols.SetValue("EffectRadius", cols.Length);
                data.SetValue(abilityBuffCommands.EffectRadius, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EffectAngle"))
            {
                cols.SetValue("EffectAngle", cols.Length);
                data.SetValue(abilityBuffCommands.EffectAngle, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("MaxTargets"))
            {
                cols.SetValue("MaxTargets", cols.Length);
                data.SetValue(abilityBuffCommands.MaxTargets, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EventIDString"))
            {
                cols.SetValue("EventIDString", cols.Length);
                data.SetValue(abilityBuffCommands.EventIDString, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EventCheck"))
            {
                cols.SetValue("EventCheck", cols.Length);
                data.SetValue(abilityBuffCommands.EventCheck, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EventCheckParam"))
            {
                cols.SetValue("EventCheckParam", cols.Length);
                data.SetValue(abilityBuffCommands.EventCheckParam, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("EventChance"))
            {
                cols.SetValue("EventChance", cols.Length);
                data.SetValue(abilityBuffCommands.EventChance, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("ConsumesStack"))
            {
                cols.SetValue("ConsumesStack", cols.Length);
                data.SetValue(abilityBuffCommands.ConsumesStack, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("RetriggerInterval"))
            {
                cols.SetValue("RetriggerInterval", cols.Length);
                data.SetValue(abilityBuffCommands.RetriggerInterval, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("BuffLine"))
            {
                cols.SetValue("BuffLine", cols.Length);
                data.SetValue(abilityBuffCommands.BuffLine, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("NoAutoUse"))
            {
                cols.SetValue("NoAutoUse", cols.Length);
                data.SetValue(abilityBuffCommands.NoAutoUse, data.Length);
            }

            if (abilityBuffCommands.toUpdateMembers.Contains("BuffClassString"))
            {
                cols.SetValue("BuffClassString", cols.Length);
                data.SetValue(abilityBuffCommands.BuffClassString, data.Length);
            }

            return new Query("war_world.buff_commands").AsInsert(cols, data);
        }
        #endregion
        #endregion
        #endregion
    }
}
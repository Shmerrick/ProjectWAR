using Common;
using FrameWork;
using GameData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        // Canonical entry maps used to resolve pseudo/alias ability and buff entries.
        private static readonly Dictionary<ushort, ushort> AbilityEntryAliases = new Dictionary<ushort, ushort>();
        private static readonly Dictionary<ushort, ushort> BuffEntryAliases = new Dictionary<ushort, ushort>();
        private static readonly Dictionary<ushort, ushort> MythicCsvAbilityEffects = new Dictionary<ushort, ushort>();
        private static readonly Dictionary<ushort, ushort[]> MythicCsvEffectLinks = new Dictionary<ushort, ushort[]>();

        internal const ushort RenownEmpoweredMasteryEntry = 27870;
        internal const ushort RenownEternalMasteryEntry = 27871;
        internal const ushort RenownInfiniteMasteryEntry = 27872;
        internal const ushort RenownAugmentVigorEntry = 27873;
        private const ushort RenownSilentBonusRankOneEntry = 22275;
        private const ushort RenownSilentBonusRankTwoEntry = 27875;
        private const ushort RenownSilentActionPointBonus = 25;
        private const ushort ImDaBiggestBuffEntry = 734;
        private const ushort InteractionBuffEntry = 60000;

        /// <summary>
        /// The 4 slottable renown tactics (3 granted at RR90, 1 at RR100).
        /// Used by TacticsInterface to classify tactic slots correctly.
        /// </summary>
        public static readonly HashSet<ushort> RenownTacticEntries = new HashSet<ushort>
        {
            RenownEmpoweredMasteryEntry,
            RenownEternalMasteryEntry,
            RenownInfiniteMasteryEntry,
            RenownAugmentVigorEntry
        };

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
            AbilityEntryAliases.Clear();
            BuffEntryAliases.Clear();
            MythicCsvAbilityEffects.Clear();
            MythicCsvEffectLinks.Clear();

            for (byte i = 0; i < 24; ++i)
                CareerAbilities[i].Clear();

            LoadNewAbilityInfo();
        }

        [LoadingFunction(true)]
        public static void LoadNewAbilityInfo()
        {
            Log.Info("AbilityMgr", "Loading New Ability Info...");

            IObjectDatabase db = WorldMgr.Database;
            bool useMythicSourceTables = Program.Config != null && Program.Config.UseMythicActionCoverageTables;

            Log.Info("AbilityMgr",
                useMythicSourceTables
                    ? "Ability loader source: mythic_src_* tables (UseMythicActionCoverageTables=true)."
                    : "Ability loader source: legacy ability tables (UseMythicActionCoverageTables=false).");

            #region Database

            List<DBAbilityInfo> dbAbilities = useMythicSourceTables
                ? db.SelectAllObjects<MythicSourceAbilityInfo>().Cast<DBAbilityInfo>().ToList()
                : db.SelectAllObjects<DBAbilityInfo>().ToList();

            List<AbilityInfo> abVolatiles = AbilityInfo.Convert(dbAbilities);
            Dictionary<ushort, AbilityConstants> abConstants = AbilityConstants.Convert(dbAbilities).ToDictionary(key => key.Entry);
            IEnumerable<DBAbilityDamageInfo> dbAbilityDamageInfos = useMythicSourceTables
                ? db.SelectAllObjects<MythicSourceAbilityDamageInfo>().Cast<DBAbilityDamageInfo>()
                : db.SelectAllObjects<DBAbilityDamageInfo>();
            List<AbilityDamageInfo> abDmgHeals = AbilityDamageInfo.Convert(dbAbilityDamageInfos
                .OrderBy(dmg => dmg.ParentCommandID)
                .ThenBy(dmg => dmg.ParentCommandSequence)
                .ToList());

            IEnumerable<DBAbilityCommandInfo> dbAbilityCommands = useMythicSourceTables
                ? db.SelectAllObjects<MythicSourceAbilityCommandInfo>().Cast<DBAbilityCommandInfo>()
                : db.SelectAllObjects<DBAbilityCommandInfo>();
            List<AbilityCommandInfo> abCommands = AbilityCommandInfo.Convert(dbAbilityCommands
                .OrderBy(cmd => cmd.CommandID)
                .ToList());

            IEnumerable<AbilityModifierCheck> dbAbilityChecks = useMythicSourceTables
                ? db.SelectAllObjects<MythicSourceAbilityModifierCheck>().Cast<AbilityModifierCheck>()
                : db.SelectAllObjects<AbilityModifierCheck>();
            IList<AbilityModifierCheck> abChecks = dbAbilityChecks
                .OrderBy(check => check.ID)
                .ToList();

            IEnumerable<AbilityModifierEffect> dbAbilityModifiers = useMythicSourceTables
                ? db.SelectAllObjects<MythicSourceAbilityModifierEffect>().Cast<AbilityModifierEffect>()
                : db.SelectAllObjects<AbilityModifierEffect>();
            IList<AbilityModifierEffect> abMods = dbAbilityModifiers
                .OrderBy(mod => mod.Sequence)
                .ToList();

            IEnumerable<DBBuffInfo> dbBuffInfos = useMythicSourceTables
                ? db.SelectAllObjects<MythicSourceBuffInfo>().Cast<DBBuffInfo>()
                : db.SelectAllObjects<DBBuffInfo>();
            List<BuffInfo> buffInfos = BuffInfo.Convert(dbBuffInfos.ToList());

            IEnumerable<DBBuffCommandInfo> dbBuffCommands = useMythicSourceTables
                ? db.SelectAllObjects<MythicSourceBuffCommandInfo>().Cast<DBBuffCommandInfo>()
                : db.SelectAllObjects<DBBuffCommandInfo>();
            List<BuffCommandInfo> buffCommands = BuffCommandInfo.Convert(dbBuffCommands
                .OrderBy(buffcmd => buffcmd.CommandID)
                .ToList());

            IEnumerable<AbilityKnockbackInfo> dbKnockbackInfos = useMythicSourceTables
                ? db.SelectAllObjects<MythicSourceAbilityKnockbackInfo>().Cast<AbilityKnockbackInfo>()
                : db.SelectAllObjects<AbilityKnockbackInfo>();
            IList<AbilityKnockbackInfo> knockbackInfos = dbKnockbackInfos
                .OrderBy(kbinfo => kbinfo.Id)
                .ToList();

            List<AbilityCommandInfo> slaveCommands = new List<AbilityCommandInfo>();
            List<BuffCommandInfo> slaveBuffCommands = new List<BuffCommandInfo>();

            Dictionary<ushort, int> damageTypeDictionary = new Dictionary<ushort, int>();

            #endregion Database

            bool useMythicAbilityGraphTables = Program.Config != null && Program.Config.UseMythicAbilityGraphTables;
            bool mythicAbilityGraphOverride = Program.Config != null && Program.Config.MythicAbilityGraphOverrideExistingCommands;

            if (useMythicAbilityGraphTables)
            {
                HashSet<ushort> knownAbilityEntries = new HashSet<ushort>(dbAbilities.Select(x => x.Entry));
                MythicAbilityGraphTranslator graphTranslator = new MythicAbilityGraphTranslator(db, mythicAbilityGraphOverride);
                MythicAbilityGraphTranslationReport graphReport;

                if (graphTranslator.TryTranslate(
                    knownAbilityEntries,
                    abCommands,
                    abDmgHeals,
                    buffInfos,
                    buffCommands,
                    out graphReport))
                {
                    PopulateGraphOverrideDiagnostics(graphReport, abDmgHeals, abMods);
                    Log.Info("AbilityMgr", graphReport.ToLogLine());
                }
                else if (graphReport != null && !string.IsNullOrWhiteSpace(graphReport.Note))
                {
                    Log.Info("AbilityMgr", "Mythic ability graph translation skipped: " + graphReport.Note);
                }
            }

            ApplyPerAbilityLevelScalars(db, dbAbilities, abDmgHeals);

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

            #endregion AbilityChecks

            #region AbilityModifiers

            foreach (AbilityModifierEffect effect in abMods)
            {
                switch (effect.PreOrPost)
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

            #endregion AbilityModifiers

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
                AbilityCommandInfo masterCommand = FindAbilityCommandRoot(slaveCommand.Entry, slaveCommand.CommandID);
                if (masterCommand != null)
                    masterCommand.AddCommandToChain(slaveCommand);
                else
                    Log.Debug("AbilityMgr", "Slave command with entry " + slaveCommand.Entry + " and depending upon master command ID " + slaveCommand.CommandID + " has no master!");
            }

            #endregion CommandInfo

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
                BuffCommandInfo masterBuffCommand = FindBuffCommandRoot(slaveBuffCommand.Entry, slaveBuffCommand.CommandID);
                if (masterBuffCommand != null)
                    masterBuffCommand.AddCommandToChain(slaveBuffCommand);
                else
                    Log.Debug("AbilityMgr", "Slave buff command with entry " + slaveBuffCommand.Entry + " and depending upon master command ID " + slaveBuffCommand.CommandID + " has no master!");
            }

            #endregion BuffCommands

            #region Damage/Heals

            // Damage and heal info gets tacked onto the command that's going to use it
            foreach (AbilityDamageInfo abDmgHeal in abDmgHeals)
            {
                if (abDmgHeal.DisplayEntry == 0)
                    abDmgHeal.DisplayEntry = abDmgHeal.Entry;
                switch (abDmgHeal.Index)
                {
                    case 0:
                        if (AbilityCommandInfos.ContainsKey(abDmgHeal.Entry))
                        {
                            AbilityCommandInfo desiredCommand =
                                FindAbilityCommandSubcommand(abDmgHeal.Entry, abDmgHeal.ParentCommandID, abDmgHeal.ParentCommandSequence);
                            if (desiredCommand != null)
                                desiredCommand.DamageInfo = abDmgHeal;
                        }

                        if (!damageTypeDictionary.ContainsKey(abDmgHeal.Entry))
                            damageTypeDictionary.Add(abDmgHeal.Entry, (int)abDmgHeal.DamageType);
                        break;

                    case 1:
                        if (BuffCommandInfos.ContainsKey(abDmgHeal.Entry))
                        {
                            BuffCommandInfo desiredCommand =
                                FindBuffCommandSubcommand(abDmgHeal.Entry, abDmgHeal.ParentCommandID, abDmgHeal.ParentCommandSequence);
                            if (desiredCommand != null)
                                desiredCommand.DamageInfo = abDmgHeal;
                            else
                                Log.Debug("AbilityMgr",
                                    "Failed Load: " + abDmgHeal.Entry + " " + abDmgHeal.ParentCommandID + " " + abDmgHeal.ParentCommandSequence);

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

            #endregion Damage/Heals

            #region KnockbackInfo

            foreach (var kbInfo in knockbackInfos)
            {
                if (!KnockbackInfos.ContainsKey(kbInfo.Entry))
                    KnockbackInfos.Add(kbInfo.Entry, new List<AbilityKnockbackInfo>());
                KnockbackInfos[kbInfo.Entry].Add(kbInfo);
            }

            #endregion KnockbackInfo

            // Volatiles -> Constants
            //           -> Commands -> DamageHeals
            foreach (AbilityInfo abVolatile in abVolatiles)
            {
                if (!NewAbilityVolatiles.ContainsKey(abVolatile.Entry))
                    NewAbilityVolatiles.Add(abVolatile.Entry, abVolatile);

                if (AbilityCommandInfos.ContainsKey(abVolatile.Entry))
                {
                    AbilityCommandInfo primaryCommand = FindAbilityCommandRoot(abVolatile.Entry, 0)
                        ?? AbilityCommandInfos[abVolatile.Entry].FirstOrDefault();

                    if (primaryCommand != null)
                    {
                        abVolatile.TargetType = primaryCommand.TargetType;
                        if (primaryCommand.AoESource != 0)
                            abVolatile.TargetType = primaryCommand.AoESource;
                    }
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
                        if (damageTypeDictionary[abConstant.Entry] == (ushort)DamageTypes.Healing || damageTypeDictionary[abConstant.Entry] == (ushort)DamageTypes.RawHealing)
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

            #endregion ConstantInfo

            #region Damage to ConstantInfo linkage

            foreach (AbilityDamageInfo damageInfo in abDmgHeals)
            {
                if (abConstants.ContainsKey(damageInfo.Entry))
                    damageInfo.MasteryTree = abConstants[damageInfo.Entry].MasteryTree;
            }

            #endregion Damage to ConstantInfo linkage

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

            EnsureRenownTacticBuffs();
            LoadCanonicalEntryResolvers();
            LoadMythicCsvAbilityLinks();

            #endregion Buff/Command linkage

            Log.Success("AbilityMgr", "Finished loading " + NewAbilityVolatiles.Count + " abilities and " + BuffInfos.Count + " buffs!");

            LoadCreatureAbilities();
        }

        private static void PopulateGraphOverrideDiagnostics(
            MythicAbilityGraphTranslationReport report,
            List<AbilityDamageInfo> abilityDamageInfos,
            IList<AbilityModifierEffect> abilityModifiers)
        {
            if (report == null || report.OverriddenAbilityEntrySet.Count == 0 || abilityDamageInfos == null)
                return;

            HashSet<ushort> retainedExtraDamageEntries = new HashSet<ushort>(
                abilityDamageInfos
                    .Where(info => info.Index == 2 && report.OverriddenAbilityEntrySet.Contains(info.Entry))
                    .Select(info => info.Entry));

            if (retainedExtraDamageEntries.Count == 0)
                return;

            report.OverriddenEntriesWithLegacyExtraDamage = retainedExtraDamageEntries.Count;
            report.RetainedLegacyExtraDamageRows = abilityDamageInfos.Count(
                info => info.Index == 2 && report.OverriddenAbilityEntrySet.Contains(info.Entry));

            if (abilityModifiers != null)
            {
                report.ModifierReferencedLegacyExtraDamageEntries = abilityModifiers
                    .Where(mod =>
                        retainedExtraDamageEntries.Contains(mod.Affecting)
                        && (mod.ModifierCommandName == "ResourceSwitchDamage"
                            || mod.ModifierCommandName == "ResourceBuffSwitchDamage"))
                    .Select(mod => mod.Affecting)
                    .Distinct()
                    .Count();
            }

            report.LegacyExtraDamageEntrySample = string.Join(
                ",",
                retainedExtraDamageEntries
                    .OrderBy(entry => entry)
                    .Take(10)
                    .Select(entry => entry.ToString(CultureInfo.InvariantCulture)));
        }

        private static void ApplyPerAbilityLevelScalars(
            IObjectDatabase db,
            IList<DBAbilityInfo> dbAbilities,
            List<AbilityDamageInfo> abilityDamageInfos)
        {
            if (db == null || dbAbilities == null || abilityDamageInfos == null || abilityDamageInfos.Count == 0)
                return;

            string sourceName;
            Dictionary<ushort, float> upgradeLevelScalars = BuildMythicUpgradeLevelScalars(db, out sourceName);
            if (upgradeLevelScalars.Count == 0)
                return;

            Dictionary<ushort, float> abilityLevelScalars = BuildAbilityLevelScalars(dbAbilities, upgradeLevelScalars);
            if (abilityLevelScalars.Count == 0)
            {
                Log.Info("AbilityMgr",
                    $"Per-ability level scaling skipped: no ability entries matched upgrade scalars (source={sourceName}, upgrades={upgradeLevelScalars.Count}).");
                return;
            }

            int appliedRows = 0;
            int unresolvedRows = 0;
            HashSet<ushort> appliedEntries = new HashSet<ushort>();

            foreach (AbilityDamageInfo damageInfo in abilityDamageInfos)
            {
                if (damageInfo.MaxDamage != 0)
                    continue;

                float levelScalar;
                if (!abilityLevelScalars.TryGetValue(damageInfo.Entry, out levelScalar))
                {
                    unresolvedRows++;
                    continue;
                }

                damageInfo.LevelScalingFactor = levelScalar;
                appliedRows++;
                appliedEntries.Add(damageInfo.Entry);
            }

            Log.Info("AbilityMgr",
                $"Per-ability level scaling applied: source={sourceName}, upgrades={upgradeLevelScalars.Count}, ability_entries={abilityLevelScalars.Count}, applied_rows={appliedRows}, applied_entries={appliedEntries.Count}, unresolved_rows={unresolvedRows}.");
        }

        private static Dictionary<ushort, float> BuildAbilityLevelScalars(
            IList<DBAbilityInfo> dbAbilities,
            Dictionary<ushort, float> upgradeLevelScalars)
        {
            Dictionary<ushort, float> abilityLevelScalars = new Dictionary<ushort, float>();
            if (dbAbilities == null || upgradeLevelScalars == null || upgradeLevelScalars.Count == 0)
                return abilityLevelScalars;

            foreach (DBAbilityInfo ability in dbAbilities)
            {
                if (ability == null)
                    continue;

                float levelScalar;
                if (ability.EffectID != 0 && upgradeLevelScalars.TryGetValue(ability.EffectID, out levelScalar))
                {
                    abilityLevelScalars[ability.Entry] = levelScalar;
                    continue;
                }

                if (upgradeLevelScalars.TryGetValue(ability.Entry, out levelScalar))
                    abilityLevelScalars[ability.Entry] = levelScalar;
            }

            return abilityLevelScalars;
        }

        private static Dictionary<ushort, float> BuildMythicUpgradeLevelScalars(
            IObjectDatabase db,
            out string sourceName)
        {
            sourceName = "none";
            Dictionary<ushort, float> upgradeScalars = new Dictionary<ushort, float>();
            if (db == null)
                return upgradeScalars;

            List<MythicBinAbilityUpgradeBinRow> upgradeBins = null;
            List<MythicBinAbilityUpgradeEntryRow> upgradeEntries = null;

            IList<MythicBinAbilityUpgradeBinRow> mythicBins = TrySelectAllRows<MythicBinAbilityUpgradeBinRow>(db);
            IList<MythicBinAbilityUpgradeEntryRow> mythicEntries = TrySelectAllRows<MythicBinAbilityUpgradeEntryRow>(db);
            if (mythicBins != null && mythicBins.Count > 0 && mythicEntries != null && mythicEntries.Count > 0)
            {
                sourceName = "mythic_bin_*";
                upgradeBins = mythicBins.ToList();
                upgradeEntries = mythicEntries.ToList();
            }
            else
            {
                IList<LondoAbilityUpgradeBinRow> londoBins = TrySelectAllRows<LondoAbilityUpgradeBinRow>(db);
                IList<LondoAbilityUpgradeEntryRow> londoEntries = TrySelectAllRows<LondoAbilityUpgradeEntryRow>(db);
                if (londoBins != null && londoBins.Count > 0 && londoEntries != null && londoEntries.Count > 0)
                {
                    sourceName = "Londo AbilityUpgrade*";
                    upgradeBins = londoBins.Cast<MythicBinAbilityUpgradeBinRow>().ToList();
                    upgradeEntries = londoEntries.Cast<MythicBinAbilityUpgradeEntryRow>().ToList();
                }
            }

            if (upgradeBins == null || upgradeEntries == null || upgradeBins.Count == 0 || upgradeEntries.Count == 0)
                return upgradeScalars;

            Dictionary<long, List<MythicBinAbilityUpgradeEntryRow>> entriesByBin = upgradeEntries
                .Where(x => x.AbilityUpgradeBinID.HasValue)
                .GroupBy(x => x.AbilityUpgradeBinID.Value)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(x => x.Index ?? int.MaxValue).ToList());

            foreach (MythicBinAbilityUpgradeBinRow upgradeBin in upgradeBins)
            {
                if (upgradeBin == null || !upgradeBin.UpgradeID.HasValue)
                    continue;

                long rawUpgradeId = upgradeBin.UpgradeID.Value;
                if (rawUpgradeId <= 0 || rawUpgradeId > ushort.MaxValue)
                    continue;

                List<MythicBinAbilityUpgradeEntryRow> entries;
                if (!entriesByBin.TryGetValue(upgradeBin.ID, out entries) || entries == null || entries.Count == 0)
                    continue;

                float candidateScalar;
                if (!TryExtractUpgradeLevelScalar(entries, out candidateScalar))
                    continue;

                ushort upgradeId = (ushort)rawUpgradeId;
                float existingScalar;
                if (upgradeScalars.TryGetValue(upgradeId, out existingScalar))
                    upgradeScalars[upgradeId] = SelectPreferredLevelScalar(existingScalar, candidateScalar);
                else
                    upgradeScalars.Add(upgradeId, candidateScalar);
            }

            return upgradeScalars;
        }

        private static bool TryExtractUpgradeLevelScalar(IList<MythicBinAbilityUpgradeEntryRow> entries, out float levelScalar)
        {
            levelScalar = 0f;
            if (entries == null || entries.Count == 0)
                return false;

            List<float> candidates = new List<float>();

            foreach (MythicBinAbilityUpgradeEntryRow entry in entries)
            {
                if (entry == null)
                    continue;

                float decodedScalar;
                if (TryDecodeUpgradeRowScalar(entry, out decodedScalar) && IsPlausibleLevelScalar(decodedScalar))
                    candidates.Add(decodedScalar);

                float decodedStringScalar;
                if (TryDecodeUpgradeValuesScalar(entry.Values, out decodedStringScalar) && IsPlausibleLevelScalar(decodedStringScalar))
                    candidates.Add(decodedStringScalar);
            }

            if (candidates.Count == 0)
                return false;

            foreach (float candidate in candidates)
            {
                if (candidate > 0f && candidate < 1f)
                {
                    levelScalar = candidate;
                    return true;
                }
            }

            foreach (float candidate in candidates)
            {
                if (Math.Abs(candidate - 1f) < 0.0001f)
                {
                    levelScalar = 1f;
                    return true;
                }
            }

            foreach (float candidate in candidates)
            {
                if (candidate > 1f && candidate <= 2f)
                {
                    levelScalar = candidate;
                    return true;
                }
            }

            return false;
        }

        private static float SelectPreferredLevelScalar(float existingScalar, float candidateScalar)
        {
            bool existingFractional = existingScalar > 0f && existingScalar < 1f;
            bool candidateFractional = candidateScalar > 0f && candidateScalar < 1f;

            if (candidateFractional && !existingFractional)
                return candidateScalar;

            return existingScalar;
        }

        private static bool TryDecodeUpgradeRowScalar(MythicBinAbilityUpgradeEntryRow entry, out float levelScalar)
        {
            levelScalar = 0f;
            if (entry == null || !entry.V1.HasValue || !entry.V2.HasValue)
                return false;

            uint lowBits = (uint)(entry.V1.Value & 0xFFFF);
            uint highBits = (uint)(entry.V2.Value & 0xFFFF);
            uint rawBits = (highBits << 16) | lowBits;
            levelScalar = BitConverter.ToSingle(BitConverter.GetBytes(rawBits), 0);

            return !(float.IsNaN(levelScalar) || float.IsInfinity(levelScalar));
        }

        private static bool TryDecodeUpgradeValuesScalar(string valuesField, out float levelScalar)
        {
            levelScalar = 0f;
            if (string.IsNullOrWhiteSpace(valuesField))
                return false;

            string[] tokens = valuesField.Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                float parsed;
                if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                {
                    levelScalar = parsed;
                    return true;
                }
            }

            return false;
        }

        private static bool IsPlausibleLevelScalar(float levelScalar)
        {
            return !(float.IsNaN(levelScalar) || float.IsInfinity(levelScalar))
                && levelScalar > 0f
                && levelScalar <= 2f;
        }

        private static IList<T> TrySelectAllRows<T>(IObjectDatabase db) where T : DataObject
        {
            if (db == null)
                return null;

            try
            {
                return db.SelectAllObjects<T>();
            }
            catch
            {
                return null;
            }
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
                    Log.Debug("Entry: " + cAb.ProtoEntry, cAb.AbilityId + " ~ Failed loading (ability not found in AbilityInfo)");
                }
            }

            foreach (uint key in temp.Keys)
                CreatureAbilities[key] = temp[key].OrderByDescending(x => x.Cooldown).ToList();
        }

        public static List<NPCAbility> GetCreatureAbilities(uint entry)
        {
            if (!CreatureAbilities.TryGetValue(entry, out List<NPCAbility> definitions))
                return null;

            var abilities = new List<NPCAbility>(definitions.Count);
            foreach (NPCAbility definition in definitions)
                abilities.Add(definition.CreateInstance());
            return abilities;
        }

        #endregion Creature Abilities

        #endregion Ability System Setup

        private static void EnsureRenownTacticBuffs()
        {
            EnsureMasteryBonusTacticBuff(RenownEmpoweredMasteryEntry, "Empowered Mastery", Stats.Mastery1Bonus);
            EnsureMasteryBonusTacticBuff(RenownEternalMasteryEntry, "Eternal Mastery", Stats.Mastery2Bonus);
            EnsureMasteryBonusTacticBuff(RenownInfiniteMasteryEntry, "Infinite Mastery", Stats.Mastery3Bonus);
            EnsureAugmentVigorBuff();
            EnsureRenownSilentBonusBuffs();
            EnsureInteractionBuff();
        }

        private static void EnsureRenownSilentBonusBuffs()
        {
            EnsureRenownSilentBonusBuff(RenownSilentBonusRankOneEntry, "Booster Renown Bonuses", true);
            EnsureRenownSilentBonusBuff(RenownSilentBonusRankTwoEntry, "Booster Renown Bonuses II", true);
        }

        private static void EnsureRenownSilentBonusBuff(ushort entry, string name, bool addsActionPoints)
        {
            BuffInfo buffInfo = CreatePersistentSilentBuffShell(entry, name);
            byte buffLine = buffInfo.StackLine == 0 ? (byte)1 : buffInfo.StackLine;
            buffInfo.StackLine = buffLine;

            if (addsActionPoints)
            {
                buffInfo.CommandInfo.Add(new BuffCommandInfo
                {
                    Entry = entry,
                    Name = name,
                    CommandID = 0,
                    CommandSequence = 0,
                    CommandName = "ModifyStat",
                    BuffClass = BuffClass.Persist,
                    PrimaryValue = (int)Stats.MaxActionPoints,
                    SecondaryValue = RenownSilentActionPointBonus,
                    InvokeOn = 5,
                    TargetType = CommandTargetTypes.Host,
                    BuffLine = buffLine
                });
            }

            BuffInfos[entry] = buffInfo;
            Log.Info("AbilityMgr", addsActionPoints
                ? $"Installed renown silent buff {entry} ({name}) with +{RenownSilentActionPointBonus} AP."
                : $"Installed renown silent buff {entry} ({name}) as a persistent mastery marker.");
        }

        private static void EnsureMasteryBonusTacticBuff(ushort entry, string name, Stats masteryBonusStat)
        {
            BuffInfo buffInfo = CreateTacticBuffShell(entry, name);
            byte buffLine = buffInfo.StackLine == 0 ? (byte)1 : buffInfo.StackLine;
            buffInfo.StackLine = buffLine;
            buffInfo.CommandInfo.Add(new BuffCommandInfo
            {
                Entry = entry,
                Name = name,
                CommandID = 0,
                CommandSequence = 0,
                CommandName = "ModifyStat",
                BuffClass = BuffClass.Tactic,
                PrimaryValue = (int)masteryBonusStat,
                SecondaryValue = 1,
                InvokeOn = 5,
                TargetType = CommandTargetTypes.Host,
                BuffLine = buffLine
            });

            BuffInfos[entry] = buffInfo;
            Log.Info("AbilityMgr", $"Installed mastery tactic buff {entry} ({name}) using {masteryBonusStat}.");
        }

        private static void EnsureAugmentVigorBuff()
        {
            if (BuffInfos.ContainsKey(RenownAugmentVigorEntry))
                return;

            if (!BuffInfos.ContainsKey(ImDaBiggestBuffEntry))
            {
                Log.Error("AbilityMgr", $"Unable to generate fallback buff for tactic {RenownAugmentVigorEntry} (Augment Vigor): source buff {ImDaBiggestBuffEntry} missing.");
                return;
            }

            BuffInfo augmentVigor = BuffInfos[ImDaBiggestBuffEntry].Clone();
            augmentVigor.Entry = RenownAugmentVigorEntry;
            augmentVigor.Name = "Augment Vigor";
            augmentVigor.MasteryTree = 0;

            if (augmentVigor.CommandInfo != null)
            {
                foreach (BuffCommandInfo command in augmentVigor.CommandInfo)
                {
                    command.Entry = RenownAugmentVigorEntry;
                    command.Name = augmentVigor.Name;
                }
            }
            else
            {
                augmentVigor.CommandInfo = new List<BuffCommandInfo>();
            }

            BuffInfos[RenownAugmentVigorEntry] = augmentVigor;
            Log.Info("AbilityMgr", $"Generated fallback tactic buff {RenownAugmentVigorEntry} ({augmentVigor.Name}) from {ImDaBiggestBuffEntry}.");
        }

        private static BuffInfo CreateTacticBuffShell(ushort entry, string name)
        {
            BuffInfo shell = BuffInfos.ContainsKey(ImDaBiggestBuffEntry)
                ? BuffInfos[ImDaBiggestBuffEntry].Clone()
                : new BuffInfo();

            shell.Entry = entry;
            shell.Name = name;
            shell.BuffClass = BuffClass.Tactic;
            shell.Type = BuffTypes.None;
            shell.Group = 0;
            shell.MaxCopies = 0;
            shell.MaxStack = 1;
            shell.InitialStacks = 1;
            shell.StacksFromCaster = false;
            shell.Duration = 0;
            shell.LeadInDelay = 0;
            shell.Interval = 0;
            shell.BuffIntervals = 0;
            shell.PersistsOnDeath = 1;
            shell.CanRefresh = false;
            shell.MasteryTree = 0;
            shell.CommandInfo = new List<BuffCommandInfo>();
            return shell;
        }

        private static BuffInfo CreatePersistentSilentBuffShell(ushort entry, string name)
        {
            BuffInfo shell = BuffInfos.ContainsKey(entry)
                ? BuffInfos[entry].Clone()
                : (BuffInfos.ContainsKey(ImDaBiggestBuffEntry) ? BuffInfos[ImDaBiggestBuffEntry].Clone() : new BuffInfo());

            shell.Entry = entry;
            shell.Name = name;
            shell.BuffClass = BuffClass.Persist;
            shell.Type = BuffTypes.None;
            shell.Group = 0;
            shell.MaxCopies = 0;
            shell.MaxStack = 1;
            shell.InitialStacks = 1;
            shell.StacksFromCaster = false;
            shell.Duration = 0;
            shell.LeadInDelay = 0;
            shell.Interval = 0;
            shell.BuffIntervals = 0;
            shell.PersistsOnDeath = 1;
            shell.CanRefresh = false;
            shell.MasteryTree = 0;
            shell.FriendlyEffectID = 0;
            shell.EnemyEffectID = 0;
            shell.EffectType = 0;
            shell.CommandInfo = new List<BuffCommandInfo>();
            return shell;
        }

        private static void EnsureInteractionBuff()
        {
            if (BuffInfos.ContainsKey(InteractionBuffEntry))
                return;

            BuffInfo shell = new BuffInfo
            {
                Entry = InteractionBuffEntry,
                Name = "Interaction",
                BuffClass = BuffClass.Standard,
                Type = BuffTypes.None,
                Group = 0,
                AuraPropagation = string.Empty,
                MaxCopies = 1,
                InitialStacks = 1,
                MaxStack = 1,
                StackLine = 0,
                StacksFromCaster = false,
                Duration = 0,
                LeadInDelay = 0,
                Interval = 0,
                BuffIntervals = 0,
                PersistsOnDeath = 0,
                CanRefresh = false,
                MasteryTree = 0,
                FriendlyEffectID = 0,
                EnemyEffectID = 0,
                EffectType = 0,
                CommandInfo = new List<BuffCommandInfo>()
            };

            BuffInfos[InteractionBuffEntry] = shell;
            Log.Info("AbilityMgr", $"Installed fallback interaction buff {InteractionBuffEntry} for BO and world-object capture flows.");
        }

        private static void LoadCanonicalEntryResolvers()
        {
            AbilityEntryAliases.Clear();
            BuffEntryAliases.Clear();

            IObjectDatabase db = WorldMgr.Database;
            IList<AbilityEntryResolver> resolvers;

            try
            {
                resolvers = db.SelectAllObjects<AbilityEntryResolver>();
            }
            catch (Exception e)
            {
                Log.Error("AbilityMgr", $"Failed to load ability_entry_resolver table: {e.Message}");
                return;
            }

            if (resolvers == null || resolvers.Count == 0)
            {
                Log.Info("AbilityMgr", "No canonical ability resolver rows found.");
                return;
            }

            int appliedAbilityAliases = 0;
            int appliedBuffAliases = 0;
            int skippedRows = 0;

            foreach (AbilityEntryResolver resolver in resolvers)
            {
                if (!resolver.Enabled)
                    continue;

                ushort sourceEntry = resolver.SourceEntry;
                ushort canonicalAbilityEntry = resolver.CanonicalAbilityEntry;

                if (sourceEntry == 0
                    || canonicalAbilityEntry == 0
                    || sourceEntry == canonicalAbilityEntry
                    || !NewAbilityVolatiles.ContainsKey(sourceEntry)
                    || !NewAbilityVolatiles.ContainsKey(canonicalAbilityEntry))
                {
                    skippedRows++;
                    continue;
                }

                AbilityEntryAliases[sourceEntry] = canonicalAbilityEntry;
                appliedAbilityAliases++;

                ushort canonicalBuffEntry = resolver.CanonicalBuffEntry;
                if (canonicalBuffEntry != 0 && BuffInfos.ContainsKey(canonicalBuffEntry))
                {
                    BuffEntryAliases[sourceEntry] = canonicalBuffEntry;
                    appliedBuffAliases++;
                }
                else if (BuffInfos.ContainsKey(canonicalAbilityEntry))
                {
                    BuffEntryAliases[sourceEntry] = canonicalAbilityEntry;
                    appliedBuffAliases++;
                }
            }

            Log.Info("AbilityMgr",
                $"Installed canonical ability aliases: {appliedAbilityAliases}, buff aliases: {appliedBuffAliases}, skipped resolver rows: {skippedRows}.");
        }

        private static void LoadMythicCsvAbilityLinks()
        {
            MythicCsvAbilityEffects.Clear();
            MythicCsvEffectLinks.Clear();

            IObjectDatabase db = WorldMgr.Database;

            IList<MythicCsvAbility> mythicAbilities;
            try
            {
                mythicAbilities = db.SelectAllObjects<MythicCsvAbility>();
            }
            catch (Exception e)
            {
                Log.Info("AbilityMgr", $"Mythic CSV ability table unavailable ({e.Message}). Skipping mythic ability link load.");
                return;
            }

            IList<MythicCsvEffect> mythicEffects;
            try
            {
                mythicEffects = db.SelectAllObjects<MythicCsvEffect>();
            }
            catch (Exception e)
            {
                Log.Info("AbilityMgr", $"Mythic CSV effects table unavailable ({e.Message}). Skipping mythic effect link load.");
                return;
            }

            foreach (MythicCsvAbility row in mythicAbilities)
            {
                if (!TryToUShort(row.AbilityId, out ushort abilityEntry))
                    continue;
                if (!TryToUShort(row.EffectId, out ushort effectEntry))
                    continue;
                if (abilityEntry == effectEntry)
                    continue;

                MythicCsvAbilityEffects[abilityEntry] = effectEntry;
            }

            foreach (MythicCsvEffect row in mythicEffects)
            {
                if (!TryToUShort(row.EffectId, out ushort effectEntry))
                    continue;

                List<ushort> links = new List<ushort>(8);
                TryAddMythicEffectLink(links, effectEntry, row.ActivateId);
                TryAddMythicEffectLink(links, effectEntry, row.CastId);
                TryAddMythicEffectLink(links, effectEntry, row.ImpactId);
                TryAddMythicEffectLink(links, effectEntry, row.AoeId);
                TryAddMythicEffectLink(links, effectEntry, row.ChannelingId);
                TryAddMythicEffectLink(links, effectEntry, row.BuildUpId);
                TryAddMythicEffectLink(links, effectEntry, row.ProjectileId);

                if (links.Count > 0)
                    MythicCsvEffectLinks[effectEntry] = links.ToArray();
            }

            Log.Info("AbilityMgr",
                $"Loaded Mythic CSV links: ability_effect_links={MythicCsvAbilityEffects.Count}, effect_link_rows={MythicCsvEffectLinks.Count}.");
        }

        private static bool TryToUShort(uint value, out ushort entry)
        {
            if (value == 0 || value > ushort.MaxValue)
            {
                entry = 0;
                return false;
            }

            entry = (ushort)value;
            return true;
        }

        private static bool TryToUShort(int value, out ushort entry)
        {
            if (value <= 0 || value > ushort.MaxValue)
            {
                entry = 0;
                return false;
            }

            entry = (ushort)value;
            return true;
        }

        private static void TryAddMythicEffectLink(List<ushort> links, ushort sourceEntry, int targetValue)
        {
            if (!TryToUShort(targetValue, out ushort targetEntry))
                return;

            targetEntry = ResolveAbilityEntry(targetEntry);
            if (targetEntry == 0 || targetEntry == sourceEntry || links.Contains(targetEntry))
                return;

            links.Add(targetEntry);
        }

        private static void AddLinkedAbilityCandidate(List<ushort> candidates, ushort sourceEntry, ushort candidateEntry)
        {
            if (candidateEntry == 0 || candidateEntry == sourceEntry)
                return;

            ushort resolvedCandidate = ResolveAbilityEntry(candidateEntry);
            if (resolvedCandidate == 0 || resolvedCandidate == sourceEntry || candidates.Contains(resolvedCandidate))
                return;

            candidates.Add(resolvedCandidate);
        }

        private static List<ushort> GetLinkedAbilityCandidates(ushort entry)
        {
            List<ushort> candidates = new List<ushort>(8);

            if (NewAbilityVolatiles.TryGetValue(entry, out AbilityInfo worldAbility))
            {
                ushort worldEffect = worldAbility.ConstantInfo?.EffectID ?? 0;
                AddLinkedAbilityCandidate(candidates, entry, worldEffect);
            }

            if (MythicCsvAbilityEffects.TryGetValue(entry, out ushort mythicEffect))
                AddLinkedAbilityCandidate(candidates, entry, mythicEffect);

            if (MythicCsvEffectLinks.TryGetValue(entry, out ushort[] effectLinks))
            {
                foreach (ushort linkedEntry in effectLinks)
                    AddLinkedAbilityCandidate(candidates, entry, linkedEntry);
            }

            return candidates;
        }

        private static ushort FindLinkedAbilityEntry(ushort startEntry, Func<ushort, bool> predicate)
        {
            ushort resolvedStart = ResolveAbilityEntry(startEntry);
            if (resolvedStart == 0)
                return 0;

            Queue<ushort> pending = new Queue<ushort>();
            HashSet<ushort> visited = new HashSet<ushort>();
            pending.Enqueue(resolvedStart);
            visited.Add(resolvedStart);

            byte depth = 0;

            while (pending.Count > 0 && depth < 8)
            {
                int width = pending.Count;
                for (int i = 0; i < width; ++i)
                {
                    ushort current = pending.Dequeue();
                    foreach (ushort candidate in GetLinkedAbilityCandidates(current))
                    {
                        if (predicate(candidate))
                            return candidate;

                        if (visited.Add(candidate))
                            pending.Enqueue(candidate);
                    }
                }

                depth++;
            }

            return 0;
        }

        private static ushort ResolveAlias(ushort entry, Dictionary<ushort, ushort> aliasMap)
        {
            ushort current = entry;
            byte guard = 0;

            while (guard < 8 && aliasMap.TryGetValue(current, out ushort next) && next != current)
            {
                current = next;
                guard++;
            }

            return current;
        }

        public static ushort ResolveAbilityEntry(ushort entry)
        {
            return ResolveAlias(entry, AbilityEntryAliases);
        }

        private static ushort ResolveAbilityCommandEntry(ushort entry)
        {
            if (AbilityCommandInfos.ContainsKey(entry))
                return entry;

            ushort resolvedEntry = ResolveAbilityEntry(entry);
            if (AbilityCommandInfos.ContainsKey(resolvedEntry))
                return resolvedEntry;

            ushort linkedCommandEntry = FindLinkedAbilityEntry(resolvedEntry, candidate => AbilityCommandInfos.ContainsKey(candidate));
            if (linkedCommandEntry != 0)
                return linkedCommandEntry;

            return resolvedEntry;
        }

        public static ushort ResolveBuffEntry(ushort entry)
        {
            ushort resolved = ResolveAlias(entry, BuffEntryAliases);
            if (resolved != entry)
                return resolved;

            if (BuffInfos.ContainsKey(entry))
                return entry;

            ushort resolvedAbility = ResolveAbilityEntry(entry);
            if (resolvedAbility != entry && BuffInfos.ContainsKey(resolvedAbility))
                return resolvedAbility;

            if (NewAbilityVolatiles.TryGetValue(entry, out AbilityInfo ability))
            {
                ushort effectEntry = ability.ConstantInfo?.EffectID ?? 0;
                if (effectEntry != 0 && BuffInfos.ContainsKey(effectEntry))
                    return effectEntry;
            }

            ushort linkedBuffEntry = FindLinkedAbilityEntry(entry, candidate =>
            {
                if (BuffInfos.ContainsKey(candidate))
                    return true;

                ushort aliasCandidate = ResolveAlias(candidate, BuffEntryAliases);
                return aliasCandidate != candidate && BuffInfos.ContainsKey(aliasCandidate);
            });

            if (linkedBuffEntry != 0)
            {
                if (BuffInfos.ContainsKey(linkedBuffEntry))
                    return linkedBuffEntry;

                ushort aliasCandidate = ResolveAlias(linkedBuffEntry, BuffEntryAliases);
                if (BuffInfos.ContainsKey(aliasCandidate))
                    return aliasCandidate;
            }

            return entry;
        }

        private static ushort ResolveBuffCommandEntry(ushort entry)
        {
            if (BuffCommandInfos.ContainsKey(entry))
                return entry;

            ushort resolvedEntry = ResolveBuffEntry(entry);
            if (BuffCommandInfos.ContainsKey(resolvedEntry))
                return resolvedEntry;

            ushort linkedBuffAbilityEntry = FindLinkedAbilityEntry(entry, candidate =>
            {
                if (BuffCommandInfos.ContainsKey(candidate))
                    return true;

                ushort aliasCandidate = ResolveAlias(candidate, BuffEntryAliases);
                return aliasCandidate != candidate && BuffCommandInfos.ContainsKey(aliasCandidate);
            });

            if (linkedBuffAbilityEntry != 0)
            {
                if (BuffCommandInfos.ContainsKey(linkedBuffAbilityEntry))
                    return linkedBuffAbilityEntry;

                ushort aliasCandidate = ResolveAlias(linkedBuffAbilityEntry, BuffEntryAliases);
                if (BuffCommandInfos.ContainsKey(aliasCandidate))
                    return aliasCandidate;
            }

            return resolvedEntry;
        }

        private static AbilityCommandInfo FindAbilityCommandRoot(ushort entry, byte commandId)
        {
            if (!AbilityCommandInfos.TryGetValue(entry, out List<AbilityCommandInfo> commands) || commands == null || commands.Count == 0)
                return null;

            if (commandId < commands.Count)
            {
                AbilityCommandInfo byIndex = commands[commandId];
                if (byIndex != null && byIndex.CommandID == commandId)
                    return byIndex;
            }

            return commands.FirstOrDefault(command => command != null && command.CommandID == commandId);
        }

        private static AbilityCommandInfo FindAbilityCommandSubcommand(ushort entry, byte commandId, byte commandSequence)
        {
            AbilityCommandInfo root = FindAbilityCommandRoot(entry, commandId);
            return root?.GetSubcommand(commandSequence);
        }

        private static BuffCommandInfo FindBuffCommandRoot(ushort entry, byte commandId)
        {
            if (!BuffCommandInfos.TryGetValue(entry, out List<BuffCommandInfo> commands) || commands == null || commands.Count == 0)
                return null;

            if (commandId < commands.Count)
            {
                BuffCommandInfo byIndex = commands[commandId];
                if (byIndex != null && byIndex.CommandID == commandId)
                    return byIndex;
            }

            return commands.FirstOrDefault(command => command != null && command.CommandID == commandId);
        }

        private static BuffCommandInfo FindBuffCommandSubcommand(ushort entry, byte commandId, byte commandSequence)
        {
            BuffCommandInfo root = FindBuffCommandRoot(entry, commandId);
            return root?.GetSubcommand(commandSequence);
        }

        #region Accessors

        public static bool HasPreCastModifiers(ushort entry)
        {
            if (AbilityPreCastModifiers.ContainsKey(entry))
                return true;

            return AbilityPreCastModifiers.ContainsKey(ResolveAbilityEntry(entry));
        }

        public static List<AbilityModifier> GetAbilityPreCastModifiers(ushort entry)
        {
            if (AbilityPreCastModifiers.ContainsKey(entry))
                return AbilityPreCastModifiers[entry];

            ushort resolvedEntry = ResolveAbilityEntry(entry);
            return AbilityPreCastModifiers.ContainsKey(resolvedEntry) ? AbilityPreCastModifiers[resolvedEntry] : null;
        }

        public static bool HasModifiers(ushort entry)
        {
            if (AbilityModifiers.ContainsKey(entry))
                return true;

            return AbilityModifiers.ContainsKey(ResolveAbilityEntry(entry));
        }

        public static List<AbilityModifier> GetAbilityModifiers(ushort entry)
        {
            if (AbilityModifiers.ContainsKey(entry))
                return AbilityModifiers[entry];

            ushort resolvedEntry = ResolveAbilityEntry(entry);
            return AbilityModifiers.ContainsKey(resolvedEntry) ? AbilityModifiers[resolvedEntry] : null;
        }

        public static List<AbilityModifier> GetAbilityDelayedModifiers(ushort entry)
        {
            if (AbilityDelayedModifiers.ContainsKey(entry))
                return AbilityDelayedModifiers[entry];

            ushort resolvedEntry = ResolveAbilityEntry(entry);
            return AbilityDelayedModifiers.ContainsKey(resolvedEntry) ? AbilityDelayedModifiers[resolvedEntry] : null;
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
            if (NewAbilityVolatiles.TryGetValue(entry, out AbilityInfo directInfo))
                return directInfo.Clone();

            ushort resolvedEntry = ResolveAbilityEntry(entry);
            if (resolvedEntry != entry && NewAbilityVolatiles.TryGetValue(resolvedEntry, out AbilityInfo resolvedInfo))
                return resolvedInfo.Clone();

            return null;
        }

        public static string GetAbilityNameFor(ushort abilityEntry)
        {
            if (NewAbilityVolatiles.ContainsKey(abilityEntry))
                return NewAbilityVolatiles[abilityEntry].Name;

            ushort resolvedAbility = ResolveAbilityEntry(abilityEntry);
            if (resolvedAbility != abilityEntry && NewAbilityVolatiles.ContainsKey(resolvedAbility))
                return NewAbilityVolatiles[resolvedAbility].Name;

            ushort resolvedBuff = ResolveBuffEntry(abilityEntry);
            if (BuffInfos.ContainsKey(resolvedBuff))
                return BuffInfos[resolvedBuff].Name;

            return "attack";
        }

        public static byte GetMasteryTreeFor(ushort entry)
        {
            ushort resolvedEntry = ResolveAbilityEntry(entry);
            if (NewAbilityVolatiles.ContainsKey(resolvedEntry))
                return NewAbilityVolatiles[resolvedEntry].ConstantInfo.MasteryTree;
            return 0;
        }

        public static ushort GetCooldownFor(ushort entry)
        {
            ushort resolvedEntry = ResolveAbilityEntry(entry);
            if (NewAbilityVolatiles.ContainsKey(resolvedEntry))
                return NewAbilityVolatiles[resolvedEntry].Cooldown;
            return 0;
        }

        public static AbilityDamageInfo GetExtraDamageFor(ushort entry, byte id, byte index)
        {
            ushort resolvedEntry = ResolveAbilityEntry(entry);
            try
            {
                return ExtraDamage[resolvedEntry][id][index].Clone();
            }
            catch (Exception)
            {
                Log.Error("AbilityMgr", "Couldn't get damage info for Entry " + entry + " (resolved " + resolvedEntry + ") ID " + id + " Index " + index);
                return null;
            }
        }

        public static bool RequiresResource(ushort entry)
        {
            ushort resolvedEntry = ResolveAbilityEntry(entry);
            if (!NewAbilityVolatiles.ContainsKey(resolvedEntry))
                return false;

            return NewAbilityVolatiles[resolvedEntry].SpecialCost > 0;
        }

        public static AbilityKnockbackInfo GetKnockbackInfo(ushort entry, int id)
        {
            ushort resolvedEntry = ResolveAbilityEntry(entry);
            return KnockbackInfos[resolvedEntry][id];
        }

        public static bool HasCommandsFor(ushort abilityEntry)
        {
            if (AbilityCommandInfos.ContainsKey(abilityEntry))
                return true;

            return AbilityCommandInfos.ContainsKey(ResolveAbilityCommandEntry(abilityEntry));
        }

        public static void GetCommandsFor(Unit caster, AbilityInfo abInfo)
        {
            ushort commandEntry = AbilityCommandInfos.ContainsKey(abInfo.Entry)
                ? abInfo.Entry
                : ResolveAbilityCommandEntry(abInfo.Entry);

            if (AbilityCommandInfos.ContainsKey(commandEntry))
            {
                // Add commands to the new info if they're applicable to the player.
                // Has to be done here because of bloody tactics and career crap
                foreach (AbilityCommandInfo abCommand in AbilityCommandInfos[commandEntry])
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
            ushort resolvedEntry = AbilityCommandInfos.ContainsKey(entry) ? entry : ResolveAbilityCommandEntry(entry);
            AbilityCommandInfo rootCommand = FindAbilityCommandRoot(resolvedEntry, comIndex);
            if (rootCommand == null)
            {
                Log.Error("AbilityMgr", $"Missing ability command root: entry={entry} resolved={resolvedEntry} commandId={comIndex}");
                return null;
            }

            return rootCommand.Clone(caster);
        }

        public static AbilityCommandInfo GetAbilityCommand(Unit caster, ushort entry, byte comIndex, byte comSeq)
        {
            ushort resolvedEntry = AbilityCommandInfos.ContainsKey(entry) ? entry : ResolveAbilityCommandEntry(entry);
            AbilityCommandInfo subcommand = FindAbilityCommandSubcommand(resolvedEntry, comIndex, comSeq);
            if (subcommand == null)
            {
                Log.Error("AbilityMgr", $"Missing ability command subcommand: entry={entry} resolved={resolvedEntry} commandId={comIndex} seq={comSeq}");
                return null;
            }

            return subcommand.Clone(caster);
        }

        #endregion Accessors

        #region Buff Interface

        public static bool HasBuffModifiers(ushort entry)
        {
            if (BuffModifiers.ContainsKey(entry))
                return true;

            return BuffModifiers.ContainsKey(ResolveBuffEntry(entry));
        }

        public static List<AbilityModifier> GetBuffModifiers(ushort entry)
        {
            if (BuffModifiers.ContainsKey(entry))
                return BuffModifiers[entry];

            ushort resolvedEntry = ResolveBuffEntry(entry);
            return BuffModifiers.ContainsKey(resolvedEntry) ? BuffModifiers[resolvedEntry] : null;
        }

        public static BuffInfo GetBuffInfo(ushort entry)
        {
            ushort resolvedEntry = ResolveBuffEntry(entry);
            if (BuffInfos.ContainsKey(resolvedEntry))
                return BuffInfos[resolvedEntry].Clone();
            Log.Error("GetBuffInfo(entry)", $"Nonexistent buff: {entry}");
            return null;
        }

        public static BuffInfo GetBuffInfo(ushort entry, Unit caster, Unit target)
        {
            ushort resolvedEntry = ResolveBuffEntry(entry);
            if (BuffInfos.ContainsKey(resolvedEntry))
            {
                BuffInfo buffInfo = BuffInfos[resolvedEntry].Clone();

                List<AbilityModifier> myModifiers = GetBuffModifiers(resolvedEntry);
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
            ushort resolvedEntry = BuffCommandInfos.ContainsKey(entry) ? entry : ResolveBuffCommandEntry(entry);
            BuffCommandInfo rootCommand = FindBuffCommandRoot(resolvedEntry, commandIndex);
            if (rootCommand == null)
            {
                Log.Error("AbilityMgr", $"Missing buff command root: entry={entry} resolved={resolvedEntry} commandId={commandIndex}");
                return null;
            }

            return rootCommand.CloneChain();
        }

        public static BuffCommandInfo GetBuffCommand(ushort entry, byte commandIndex, byte comSeq)
        {
            ushort resolvedEntry = BuffCommandInfos.ContainsKey(entry) ? entry : ResolveBuffCommandEntry(entry);
            BuffCommandInfo subcommand = FindBuffCommandSubcommand(resolvedEntry, commandIndex, comSeq);
            if (subcommand == null)
            {
                Log.Error("AbilityMgr", $"Missing buff command subcommand: entry={entry} resolved={resolvedEntry} commandId={commandIndex} seq={comSeq}");
                return null;
            }

            return subcommand.CloneChain();
        }

        #endregion Buff Interface
    }
}

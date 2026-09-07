using System.Collections.Generic;

using Common;
using FrameWork;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Objects;

namespace WorldServer.Services.World
{
    /// <summary>
    /// The nine tome tactic lines and the bestiary entries that feed their fragment counters.
    /// See Common.Tome_Tactic_Line, Common.Tome_Tactic_Fragment and migration 55.
    /// </summary>
    [Service]
    public class TomeTacticService : ServiceBase
    {
        private static Dictionary<ushort, Tome_Tactic_Line> _lines = new Dictionary<ushort, Tome_Tactic_Line>();

        /// <summary>Bestiary tok entry -> the line counter it advances.</summary>
        private static Dictionary<ushort, ushort> _fragments = new Dictionary<ushort, ushort>();

        /// <summary>Line counter -> every bestiary tok entry that advances it.</summary>
        private static Dictionary<ushort, List<ushort>> _fragmentsByLine = new Dictionary<ushort, List<ushort>>();

        /// <summary>Tactic ability entry -> (line, tier), for slot validation and purchase.</summary>
        private static readonly Dictionary<ushort, KeyValuePair<Tome_Tactic_Line, int>> _tacticTiers =
            new Dictionary<ushort, KeyValuePair<Tome_Tactic_Line, int>>();

        [LoadingFunction(true)]
        public static void LoadTomeTacticLines()
        {
            Log.Debug("WorldMgr", "Loading Tome_Tactic_Lines...");

            Dictionary<ushort, Tome_Tactic_Line> lines = new Dictionary<ushort, Tome_Tactic_Line>();
            _tacticTiers.Clear();

            IList<Tome_Tactic_Line> rows = Database.SelectAllObjects<Tome_Tactic_Line>();

            if (rows != null)
            {
                foreach (Tome_Tactic_Line line in rows)
                {
                    if (line == null || lines.ContainsKey(line.AcId))
                        continue;

                    // A zero threshold would unlock its tier the moment the counter is touched,
                    // and non-ascending thresholds would let a higher tier unlock before a lower
                    // one. Both mean the row was mis-seeded, so refuse it rather than ship a line
                    // that hands out tier 3 for free.
                    if (line.Threshold1 == 0 || line.Threshold2 <= line.Threshold1 || line.Threshold3 <= line.Threshold2)
                    {
                        Log.Error("LoadTomeTacticLines", "Tactic line " + line.AcId + " (" + line.Name
                            + ") has non-ascending thresholds " + line.Threshold1 + "/" + line.Threshold2
                            + "/" + line.Threshold3 + "; skipped.");
                        continue;
                    }

                    lines.Add(line.AcId, line);

                    for (int tier = 1; tier <= 3; tier++)
                    {
                        ushort tactic = line.TacticForTier(tier);
                        if (tactic != 0)
                            _tacticTiers[tactic] = new KeyValuePair<Tome_Tactic_Line, int>(line, tier);
                    }
                }
            }

            _lines = lines;

            Log.Success("LoadTomeTacticLines", "Loaded " + lines.Count + " tome tactic lines ("
                + _tacticTiers.Count + " tactics)");

            ValidateLineCompletability();
        }

        [LoadingFunction(true)]
        public static void LoadTomeTacticFragments()
        {
            Log.Debug("WorldMgr", "Loading Tome_Tactic_Fragments...");

            Dictionary<ushort, ushort> fragments = new Dictionary<ushort, ushort>();

            IList<Tome_Tactic_Fragment> rows = Database.SelectAllObjects<Tome_Tactic_Fragment>();

            if (rows != null)
            {
                foreach (Tome_Tactic_Fragment fragment in rows)
                {
                    if (fragment == null || fragments.ContainsKey(fragment.TokEntry))
                        continue;

                    fragments.Add(fragment.TokEntry, fragment.AcId);
                }
            }

            _fragments = fragments;

            Dictionary<ushort, List<ushort>> byLine = new Dictionary<ushort, List<ushort>>();
            foreach (KeyValuePair<ushort, ushort> kv in fragments)
            {
                List<ushort> entries;
                if (!byLine.TryGetValue(kv.Value, out entries))
                {
                    entries = new List<ushort>();
                    byLine.Add(kv.Value, entries);
                }

                entries.Add(kv.Key);
            }

            _fragmentsByLine = byLine;

            Log.Success("LoadTomeTacticFragments", "Loaded " + fragments.Count + " tome tactic fragments");

            ValidateLineCompletability();
        }

        /// <summary>
        /// Reports any line whose bound fragments cannot reach its own final threshold, so an
        /// uncompletable line surfaces at boot rather than being found by a player who ground the
        /// whole line out.
        ///
        /// Called from both loaders because LoadingFunction ordering within a service is not
        /// guaranteed, and this needs both tables. It returns until both are populated, so
        /// whichever loader runs second performs the check.
        /// </summary>
        private static void ValidateLineCompletability()
        {
            if (_lines.Count == 0 || _fragments.Count == 0)
                return;

            Dictionary<ushort, int> counts = new Dictionary<ushort, int>();
            foreach (KeyValuePair<ushort, ushort> kv in _fragments)
            {
                int existing;
                counts.TryGetValue(kv.Value, out existing);
                counts[kv.Value] = existing + 1;
            }

            int unreachable = 0;
            foreach (KeyValuePair<ushort, Tome_Tactic_Line> kv in _lines)
            {
                int available;
                counts.TryGetValue(kv.Key, out available);

                if (available < kv.Value.Threshold3)
                {
                    Log.Error("TomeTacticService", "Tactic line " + kv.Value.Name + " needs "
                        + kv.Value.Threshold3 + " fragments for its final tier but only " + available
                        + " are bound; that tier is unreachable (BUG-117).");
                    ++unreachable;
                }
            }

            if (unreachable == 0)
                Log.Success("TomeTacticService", "Every tome tactic line is completable from the fragments bound");
        }

        /// <summary>creature_protos.CreatureType -> the tactic line that acts against it.</summary>
        private static Dictionary<byte, ushort> _creatureTypeLines = new Dictionary<byte, ushort>();

        /// <summary>Fraction of its normal aggro range a monster keeps against these tactics.</summary>
        public const float AggroRangeMultiplier = 0.5f;

        [LoadingFunction(true)]
        public static void LoadTomeTacticLineCreatureTypes()
        {
            Dictionary<byte, ushort> byType = new Dictionary<byte, ushort>();

            IList<Tome_Tactic_Line_Creature_Type> rows = Database.SelectAllObjects<Tome_Tactic_Line_Creature_Type>();

            if (rows != null)
            {
                foreach (Tome_Tactic_Line_Creature_Type row in rows)
                {
                    if (row == null || row.CreatureType == 0)
                        continue;

                    // A creature type owned by two lines would make tactic matching ambiguous.
                    ushort existing;
                    if (byType.TryGetValue(row.CreatureType, out existing))
                    {
                        Log.Error("LoadTomeTacticLineCreatureTypes", "Creature type " + row.CreatureType
                            + " is claimed by both tactic line " + existing + " and " + row.AcId
                            + "; keeping " + existing + ".");
                        continue;
                    }

                    byType.Add(row.CreatureType, row.AcId);
                }
            }

            _creatureTypeLines = byType;

            Log.Success("LoadTomeTacticLineCreatureTypes", "Loaded " + byType.Count + " tome tactic creature type bindings");
        }

        /// <summary>
        /// True when this tactic reduces the aggro range of monsters of the given creature type.
        /// </summary>
        public static bool ReducesAggroRange(ushort abilityEntry, byte creatureType)
        {
            return Grants(abilityEntry, TomeTacticEffect.AggroRange, creatureType);
        }

        /// <summary>
        /// What a tome tactic does, decoded from the component data in mythic_bin_ability and
        /// cross-read against the tactic descriptions in the client's abilitydesc.txt.
        /// </summary>
        public enum TomeTacticEffect
        {
            /// <summary>
            /// BONUS_TYPE_ADJUST stat 54, -50%: how close the type lets you approach before it
            /// aggroes. Applied in AIInterface.GetAttackableUnit.
            /// </summary>
            AggroRange,

            /// <summary>ComponentOperationType.DAMAGE_CHANGE, +5%: damage dealt to the type.</summary>
            DamageDealt,

            /// <summary>DAMAGE_CHANGE, -5%: damage taken from the type.</summary>
            DamageTaken,

            /// <summary>DEFENSIVE_STAT_CHANGE, +5%: chance to defend against the type.</summary>
            DefendChance,

            /// <summary>BONUS_TYPE_ADJUST stat 42, +5%: chance to crit the type.</summary>
            CritChance,

            /// <summary>BONUS_TYPE_ADJUST stat 41, -10%: action point cost against the type.</summary>
            ActionPointCost,

            /// <summary>BONUS_TYPE_ADJUST stat 58, +50%: experience from the type.</summary>
            Experience,

            /// <summary>MORALE_REGEN_CHANGE, +25% while attacking the type.</summary>
            MoraleRate,

            /// <summary>COOLDOWN_CHANGE, -2000ms while attacking the type.</summary>
            Cooldown
        }

        /// <summary>
        /// Which effects each tactic grants. Tiers are cumulative: tier 2 repeats tier 1's effect
        /// and adds one, tier 3 repeats both and adds a third, exactly as the component data shows.
        /// </summary>
        private static readonly Dictionary<ushort, TomeTacticEffect[]> TacticEffects =
            new Dictionary<ushort, TomeTacticEffect[]>
            {
                // Daemonic
                { 15100, new[] { TomeTacticEffect.DamageDealt } },
                { 15101, new[] { TomeTacticEffect.DamageDealt, TomeTacticEffect.DamageTaken } },
                { 15102, new[] { TomeTacticEffect.DamageDealt, TomeTacticEffect.DamageTaken, TomeTacticEffect.MoraleRate } },
                // Beastial
                { 15103, new[] { TomeTacticEffect.AggroRange } },
                { 15104, new[] { TomeTacticEffect.AggroRange, TomeTacticEffect.DamageTaken } },
                { 15105, new[] { TomeTacticEffect.AggroRange, TomeTacticEffect.DamageTaken, TomeTacticEffect.Experience } },
                // Giant
                { 15106, new[] { TomeTacticEffect.DefendChance } },
                { 15107, new[] { TomeTacticEffect.DefendChance, TomeTacticEffect.CritChance } },
                { 15108, new[] { TomeTacticEffect.DefendChance, TomeTacticEffect.CritChance, TomeTacticEffect.MoraleRate } },
                // Greenskin
                { 15109, new[] { TomeTacticEffect.AggroRange } },
                { 15110, new[] { TomeTacticEffect.AggroRange, TomeTacticEffect.ActionPointCost } },
                { 15111, new[] { TomeTacticEffect.AggroRange, TomeTacticEffect.ActionPointCost, TomeTacticEffect.MoraleRate } },
                // Chaos
                { 15112, new[] { TomeTacticEffect.DefendChance } },
                { 15113, new[] { TomeTacticEffect.DefendChance, TomeTacticEffect.DamageTaken } },
                { 15114, new[] { TomeTacticEffect.DefendChance, TomeTacticEffect.DamageTaken, TomeTacticEffect.Cooldown } },
                // Mythical
                { 15115, new[] { TomeTacticEffect.DamageDealt } },
                { 15116, new[] { TomeTacticEffect.DamageDealt, TomeTacticEffect.ActionPointCost } },
                { 15117, new[] { TomeTacticEffect.DamageDealt, TomeTacticEffect.ActionPointCost, TomeTacticEffect.Cooldown } },
                // Man
                { 15118, new[] { TomeTacticEffect.AggroRange } },
                { 15119, new[] { TomeTacticEffect.AggroRange, TomeTacticEffect.ActionPointCost } },
                { 15120, new[] { TomeTacticEffect.AggroRange, TomeTacticEffect.ActionPointCost, TomeTacticEffect.Experience } },
                // Skaven
                { 15121, new[] { TomeTacticEffect.DefendChance } },
                { 15122, new[] { TomeTacticEffect.DefendChance, TomeTacticEffect.CritChance } },
                { 15123, new[] { TomeTacticEffect.DefendChance, TomeTacticEffect.CritChance, TomeTacticEffect.Experience } },
                // Undead
                { 15124, new[] { TomeTacticEffect.DamageDealt } },
                { 15125, new[] { TomeTacticEffect.DamageDealt, TomeTacticEffect.DamageTaken } },
                { 15126, new[] { TomeTacticEffect.DamageDealt, TomeTacticEffect.DamageTaken, TomeTacticEffect.Cooldown } }
            };

        /// <summary>Magnitudes from the component data. Percentages except Cooldown, in ms.</summary>
        public const float DamageDealtBonus = 0.05f;
        public const float DamageTakenReduction = 0.05f;
        public const float DefendChanceBonus = 5f;
        public const float CritChanceBonus = 5f;
        public const float ActionPointCostReduction = 0.10f;
        public const float ExperienceBonus = 0.50f;
        public const float MoraleRateBonus = 0.25f;
        public const int CooldownReductionMs = 2000;

        /// <summary>
        /// True when this tactic grants the given effect against the given creature type. The
        /// creature type must belong to the tactic's own line, which is what makes every tome
        /// tactic conditional on what it is fighting.
        /// </summary>
        private static bool Grants(ushort abilityEntry, TomeTacticEffect effect, byte creatureType)
        {
            TomeTacticEffect[] effects;
            if (creatureType == 0 || !TacticEffects.TryGetValue(abilityEntry, out effects))
                return false;

            bool has = false;
            for (int i = 0; i < effects.Length; i++)
                if (effects[i] == effect) { has = true; break; }

            if (!has)
                return false;

            ushort acId;
            if (!_creatureTypeLines.TryGetValue(creatureType, out acId))
                return false;

            Tome_Tactic_Line line;
            int tier;
            return TryGetTactic(abilityEntry, out line, out tier) && line.AcId == acId;
        }

        /// <summary>
        /// True when any tactic this player has slotted grants the effect against this creature
        /// type. Callers sit on combat paths, so this indexes and allocates nothing.
        /// </summary>
        public static bool PlayerHasEffect(Player player, TomeTacticEffect effect, byte creatureType)
        {
            if (player == null || creatureType == 0)
                return false;

            IList<ushort> slotted = player.TacInterface?.GetActiveTactics();
            if (slotted == null)
                return false;

            for (int i = 0; i < slotted.Count; i++)
                if (Grants(slotted[i], effect, creatureType))
                    return true;

            return false;
        }

        /// <summary>
        /// The bestiary creature type of a unit, or 0 when it has none. Pets are excluded: they
        /// carry their prototype's creature type but belong to a player, and every tome tactic is
        /// PvE-only ("closer to a Greenskin mob (not a player)").
        /// </summary>
        public static byte GetCreatureType(Unit unit)
        {
            if (unit == null || unit is Pet)
                return 0;

            Creature creature = unit as Creature;
            return creature?.Spawn?.Proto?.CreatureType ?? (byte)0;
        }

        /// <summary>
        /// Applies the damage-dealt and damage-taken tactics to one exchange. Called from
        /// CombatManager where both sides are in scope, since neither ModifyDamageOut nor
        /// ModifyDamageIn is given the other party.
        /// </summary>
        public static void ApplyDamageModifiers(Unit caster, Unit target, AbilityDamageInfo damageInfo)
        {
            if (damageInfo == null)
                return;

            // Player striking a monster: +5% to that monster's type.
            Player attacker = caster as Player;
            if (attacker != null && PlayerHasEffect(attacker, TomeTacticEffect.DamageDealt, GetCreatureType(target)))
            {
                damageInfo.DamageBonus += DamageDealtBonus;
                attacker.TacInterface?.CountTomeTacticEffect(TomeTacticEffect.DamageDealt);
            }

            // Monster striking a player: -5% from that monster's type.
            Player victim = target as Player;
            if (victim != null && PlayerHasEffect(victim, TomeTacticEffect.DamageTaken, GetCreatureType(caster)))
            {
                damageInfo.DamageReduction *= 1f - DamageTakenReduction;
                victim.TacInterface?.CountTomeTacticEffect(TomeTacticEffect.DamageTaken);
            }
        }

        /// <summary>
        /// Returns the tactic line counter this Tome entry feeds, or false when the entry is not
        /// a tactic fragment.
        /// </summary>
        public static bool TryGetFragmentLine(ushort tokEntry, out ushort acId)
        {
            return _fragments.TryGetValue(tokEntry, out acId);
        }

        /// <summary>Every bestiary Tome entry that advances this line, or an empty list.</summary>
        public static IList<ushort> GetFragmentsForLine(ushort acId)
        {
            List<ushort> entries;
            return _fragmentsByLine.TryGetValue(acId, out entries) ? entries : (IList<ushort>)new ushort[0];
        }

        public static bool TryGetLine(ushort acId, out Tome_Tactic_Line line)
        {
            return _lines.TryGetValue(acId, out line);
        }

        /// <summary>
        /// Returns the line and tier a tactic ability belongs to, or false when the ability is
        /// not a tome tactic.
        /// </summary>
        public static bool TryGetTactic(ushort abilityEntry, out Tome_Tactic_Line line, out int tier)
        {
            KeyValuePair<Tome_Tactic_Line, int> found;
            if (_tacticTiers.TryGetValue(abilityEntry, out found))
            {
                line = found.Key;
                tier = found.Value;
                return true;
            }

            line = null;
            tier = 0;
            return false;
        }

        /// <summary>
        /// The 27 tactics in ability-entry order, which is the order the live server advertises
        /// them in and therefore fixes each one's package index (index + 1). Built once at load.
        /// </summary>
        private static List<AbilityInfo> _orderedTactics = new List<AbilityInfo>();

        private static readonly Dictionary<ushort, Tome_Tactic_Line_Lookup> _packageInfo =
            new Dictionary<ushort, Tome_Tactic_Line_Lookup>();

        /// <summary>
        /// The first advance id the live server uses for tome tactic package 1. Package n carries
        /// AdvanceId 809 + n, read straight from the capture corpus (packages 1-27 -> 810-836).
        /// </summary>
        private const ushort AdvanceIdBase = 809;

        /// <summary>
        /// Resolves the ability list and per-package fields. Deferred to first use because
        /// AbilityMgr loads after the LoadingFunctions here and GetAbilityInfo would return null.
        /// </summary>
        private static void EnsureOrderedTactics()
        {
            if (_orderedTactics.Count > 0 || _lines.Count == 0)
                return;

            List<ushort> entries = new List<ushort>(_tacticTiers.Keys);
            entries.Sort();

            List<AbilityInfo> ordered = new List<AbilityInfo>();

            foreach (ushort entry in entries)
            {
                AbilityInfo info = AbilityMgr.GetAbilityInfo(entry);
                if (info == null)
                {
                    Log.Error("TomeTacticService", "Tome tactic " + entry + " has no ability info; the "
                        + "librarian list would be short and every later package index would shift.");
                    return;
                }

                ordered.Add(info);
            }

            for (int i = 0; i < ordered.Count; i++)
            {
                Tome_Tactic_Line line;
                int tier;
                if (!TryGetTactic(ordered[i].Entry, out line, out tier))
                    continue;

                _packageInfo[ordered[i].Entry] = new Tome_Tactic_Line_Lookup
                {
                    PackageIndex = (byte)(i + 1),
                    AdvanceId = (ushort)(AdvanceIdBase + i + 1),
                    TokEntry = line.TokEntryForTier(tier),
                    Line = line,
                    Tier = tier
                };
            }

            _orderedTactics = ordered;
        }

        /// <summary>
        /// All 27 tactics in package order. The live server sends the whole set regardless of what
        /// the player has earned -- the client greys out the unavailable ones itself using its
        /// fragment counters -- so the package index is stable for every character.
        /// </summary>
        public static List<AbilityInfo> GetOrderedTactics()
        {
            EnsureOrderedTactics();
            return _orderedTactics;
        }

        /// <summary>Packet fields for a tactic, or a zeroed record when it is not a tome tactic.</summary>
        public static Tome_Tactic_Line_Lookup GetPackageInfo(ushort abilityEntry)
        {
            EnsureOrderedTactics();

            Tome_Tactic_Line_Lookup found;
            return _packageInfo.TryGetValue(abilityEntry, out found) ? found : new Tome_Tactic_Line_Lookup();
        }

        /// <summary>Resolves a 1-based package index from the client to its tactic entry.</summary>
        public static bool TryGetTacticByPackageIndex(int packageIndex, out ushort abilityEntry)
        {
            EnsureOrderedTactics();

            if (packageIndex >= 1 && packageIndex <= _orderedTactics.Count)
            {
                abilityEntry = _orderedTactics[packageIndex - 1].Entry;
                return true;
            }

            abilityEntry = 0;
            return false;
        }

        /// <summary>The tactic line that acts against this creature type, if any.</summary>
        public static bool TryGetLineForCreatureType(byte creatureType, out Tome_Tactic_Line line)
        {
            ushort acId;
            if (_creatureTypeLines.TryGetValue(creatureType, out acId))
                return _lines.TryGetValue(acId, out line);

            line = null;
            return false;
        }

        /// <summary>Human-readable magnitude of an effect, for the .tometactic GM report.</summary>
        public static string DescribeEffect(TomeTacticEffect effect)
        {
            switch (effect)
            {
                case TomeTacticEffect.AggroRange:      return "aggro range x" + AggroRangeMultiplier;
                case TomeTacticEffect.DamageDealt:     return "+" + (DamageDealtBonus * 100) + "% damage dealt";
                case TomeTacticEffect.DamageTaken:     return "-" + (DamageTakenReduction * 100) + "% damage taken";
                case TomeTacticEffect.DefendChance:    return "+" + DefendChanceBonus + " defend";
                case TomeTacticEffect.CritChance:      return "+" + CritChanceBonus + "% crit";
                case TomeTacticEffect.ActionPointCost: return "-" + (ActionPointCostReduction * 100) + "% AP cost";
                case TomeTacticEffect.Experience:      return "+" + (ExperienceBonus * 100) + "% experience";
                case TomeTacticEffect.MoraleRate:      return "+" + (MoraleRateBonus * 100) + "% morale (NOT IMPLEMENTED)";
                case TomeTacticEffect.Cooldown:        return "-" + CooldownReductionMs + "ms cooldown (NOT IMPLEMENTED)";
                default:                               return effect.ToString();
            }
        }

        /// <summary>True when the ability is one of the 27 tome tactics.</summary>
        public static bool IsTomeTactic(ushort abilityEntry)
        {
            return _tacticTiers.ContainsKey(abilityEntry);
        }

        public static IEnumerable<Tome_Tactic_Line> GetLines()
        {
            return _lines.Values;
        }
    }
    /// <summary>
    /// The per-package fields the client needs for one tome tactic in F_CAREER_PACKAGE_INFO.
    /// Field meanings and values are transcribed from the official capture corpus.
    /// </summary>
    public struct Tome_Tactic_Line_Lookup
    {
        /// <summary>1-based position in the advertised list; echoed back on purchase.</summary>
        public byte PackageIndex;

        /// <summary>Sequential advance id, 809 + PackageIndex.</summary>
        public ushort AdvanceId;

        /// <summary>The Section 26 Tome entry this tactic unlocks behind.</summary>
        public ushort TokEntry;

        public Common.Tome_Tactic_Line Line;

        public int Tier;
    }
}

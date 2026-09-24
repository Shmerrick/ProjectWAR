using ClientDataMatrix.Model;
using ClientDataMatrix.Parsers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Proves the COM-token reading against the whole dataset, and reports where our ability data
    /// disagrees with the client's.
    ///
    /// TWO DIFFERENT QUESTIONS, MEASURED SEPARATELY, AND THE DISTINCTION IS THE POINT.
    ///
    ///   RESOLUTION RATE -- of every damage token in the client's own tooltips, how many resolve to
    ///   a number under the reading in <see cref="AbilityTokenResolver"/>. This tests the *logic*.
    ///   A wrong reading cannot resolve cleanly at scale: indices would run off the end of component
    ///   lists constantly. Near-total resolution is the proof.
    ///
    ///   AGREEMENT RATE -- of the tokens that resolve, how many equal what our database says. This
    ///   tests the *data*, and it is expected to be lower. Every disagreement is either drift
    ///   introduced somewhere in the database's merged history or a place the reading needs
    ///   qualifying, and the report names each one with both numbers.
    ///
    /// Conflating the two would let a broken reading look like a broken database, or the reverse.
    ///
    /// THE CLIENT SIDE COMES FROM THE CLIENT. Components, timings, ranges and costs are read from
    /// `abilityexport.bin`, `abilitycomponentexport.bin` and `upgradetableexport.bin` in the
    /// extracted client -- never from the toolkit's import of them (`mythic_bin_ability`,
    /// `mythic_bin_abilityupgrade*`), which is a copy and differs in places. The import is checked
    /// against the client in its own section instead of being trusted.
    ///
    /// A VALUE ON ONE SIDE AND NONE ON THE OTHER IS NOT A DISAGREEMENT. The column checks separate
    /// "both sides carry a value and they differ" from "one side carries none", because the second
    /// is often a convention rather than drift: the client binds the Squig pet abilities 6-9 to no
    /// career line while ours carry a mask. Only the first counts against the agreement rate; both
    /// are listed.
    /// </summary>
    public sealed class AbilityCrosswalkService
    {
        public const string KindDamageDiffers = "damage differs from client";
        public const string KindCrossReference = "not comparable: tooltip quotes another ability";
        public const string KindReferenceLike = "not comparable: slot holds a reference";
        public const string KindValueDiffers = "value differs from client";
        public const string KindDatabaseEmpty = "ours carries no value";
        public const string KindClientEmpty = "client carries no value";
        public const string KindNotRepresentable = "client value not representable in our unit";
        public const string KindSeveralCareerLines = "not comparable: ours grants several career lines";
        public const string KindImportDiffers = "toolkit import differs from client";

        public sealed class AbilityFinding
        {
            /// <summary>`abilitynames.txt` — the client's own name, the arbiter for ability names.</summary>
            public string ClientName { get; set; }

            public long DatabaseEntry { get; set; }

            /// <summary>The column or value compared: MinDamage, Cooldown, ComponentIds, ...</summary>
            public string Field { get; set; }

            /// <summary>Where the client value came from: a tooltip token or a client file field.</summary>
            public string Token { get; set; }

            public long ClientValue { get; set; }
            public long DatabaseValue { get; set; }
            public string Kind { get; set; }
            public string Detail { get; set; }
        }

        /// <summary>One column of ours checked against the client, split by what kind of mismatch.</summary>
        public sealed class FieldTally
        {
            public string Field;
            public string Rule;
            public int Agreements;

            /// <summary>Both sides carry a value and they differ: candidate drift.</summary>
            public int Disagreements;

            /// <summary>Ours is 0 or NULL where the client carries a value.</summary>
            public int DatabaseEmpty;

            /// <summary>The client carries 0 where ours carries a value.</summary>
            public int ClientEmpty;

            /// <summary>The client's value cannot be held in our column's unit at all.</summary>
            public int NotRepresentable;

            public int NotComparable;

            /// <summary>Ours is NULL and the client carries 0: no information either way.</summary>
            public int DatabaseNull;

            public int Comparable
            {
                get { return Agreements + Disagreements; }
            }

            public double AgreementRate
            {
                get { return Comparable == 0 ? 0 : 100.0 * Agreements / Comparable; }
            }
        }

        public sealed class Report
        {
            public int AbilitiesWithDescriptions;
            public int AbilitiesWithComponents;

            public int DamageTokensSeen;
            public int DamageTokensResolved;
            public int DamageTokensUnresolved;
            public readonly Dictionary<string, int> UnresolvedReasons =
                new Dictionary<string, int>(StringComparer.Ordinal);

            /// <summary>The first abilities behind each unresolved reason, so a reason can be checked.</summary>
            public readonly Dictionary<string, List<long>> UnresolvedExamples =
                new Dictionary<string, List<long>>(StringComparer.Ordinal);

            public int ComparableAbilities;
            public int Agreements;
            public int Disagreements;

            /// <summary>Excluded: the tooltip quotes a different ability's component.</summary>
            public int CrossReferenced;

            /// <summary>Excluded: the value slot holds an id the client follows, not a number.</summary>
            public int ReferenceLike;

            public int DatabaseAbilityRows;
            public int DatabaseRowsWithoutClientRecord;
            public readonly List<FieldTally> Fields = new List<FieldTally>();

            public int ComponentListsCompared;
            public int ComponentListsDiffer;
            public int ClientListsWithoutImport;
            public int ImportListsWithoutClientRecord;

            public int UpgradeItemsCompared;
            public int UpgradeItemsDiffer;
            public int UpgradeRecordsWithoutImport;

            public readonly List<AbilityFinding> Findings = new List<AbilityFinding>();

            public double ResolutionRate
            {
                get { return DamageTokensSeen == 0 ? 0 : 100.0 * DamageTokensResolved / DamageTokensSeen; }
            }

            public double AgreementRate
            {
                get { return ComparableAbilities == 0 ? 0 : 100.0 * Agreements / ComparableAbilities; }
            }
        }

        private sealed class DatabaseAbilityRow
        {
            public long Entry;
            public long? CareerLine;
            public long? Range;
            public long? CastTime;
            public long? Cooldown;
            public long? ApCost;
            public long? EffectId;
            public long? ChannelId;
            public long? ChannelDuration;
            public long? ChannelInterval;
        }

        private sealed class DatabaseBuffRow
        {
            public long? Duration;
            public long? Interval;
        }

        /// <summary>
        /// Above this, a "damage" value is an id rather than a number. Component and ability ids run
        /// into the thousands while real ability damage at rank 40 is in the hundreds, so the two
        /// ranges do not overlap in practice. Only applied when our own value is small, so a genuine
        /// four-figure hit is never silently discarded.
        /// </summary>
        private const long ReferenceLikeThreshold = 1000;

        // Our columns' units against the client's: seconds against milliseconds, and feet against a
        // client range unit twelve to the foot.
        private const long SecondsToMilliseconds = 1000;
        private const long FeetToClientRange = 12;

        private const int ExampleLimit = 50;

        private static readonly string[] FieldOrder =
        {
            "MinDamage", "CastTime", "Cooldown", "Range", "ApCost", "Channel", "ChannelDuration", "ChannelInterval",
            "EffectID", "CareerLine",
            "Duration", "Interval", "ComponentIds", "UpgradeItem"
        };

        private static readonly string[] KindOrder =
        {
            KindDamageDiffers, KindValueDiffers, KindImportDiffers, KindNotRepresentable,
            KindSeveralCareerLines, KindDatabaseEmpty, KindClientEmpty, KindCrossReference, KindReferenceLike
        };

        private readonly string _connectionString;
        private readonly string _extractedRoot;

        public AbilityCrosswalkService(string connectionString, string extractedRoot)
        {
            _connectionString = connectionString;
            _extractedRoot = extractedRoot;
        }

        public Report Run()
        {
            var report = new Report();

            Dictionary<long, string> descriptions = LoadStringTable("abilitydesc.txt");
            Dictionary<long, string> names = LoadStringTable("abilitynames.txt");
            report.AbilitiesWithDescriptions = descriptions.Count;

            string binDirectory = Path.Combine(_extractedRoot, "data", "bin");
            Dictionary<ushort, BinaryAbilityRecord> abilities = AbilityBinaryParser
                .ParseAbilityExport(Path.Combine(binDirectory, "abilityexport.bin"))
                .GroupBy(row => row.AbilityId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.RecordIndex).First());
            Dictionary<ushort, BinaryComponentRecord> componentRecords = AbilityBinaryParser
                .ParseAbilityComponentExport(Path.Combine(binDirectory, "abilitycomponentexport.bin"))
                .GroupBy(row => row.ComponentId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.RecordIndex).First());
            List<BinaryUpgradeTableRecord> upgradeTables = AbilityBinaryParser
                .ParseUpgradeTableExport(Path.Combine(binDirectory, "upgradetableexport.bin"));

            var components = new Dictionary<long, List<AbilityTokenResolver.Component>>();
            foreach (BinaryAbilityRecord ability in abilities.Values)
            {
                List<AbilityTokenResolver.Component> ordered = AbilityTokenResolver.FromClient(ability, componentRecords);
                if (ordered.Count > 0)
                    components[ability.AbilityId] = ordered;
            }

            report.AbilitiesWithComponents = components.Count;

            var databaseDamage = new Dictionary<long, long>();
            var databaseAbilities = new List<DatabaseAbilityRow>();
            var databaseBuffs = new Dictionary<long, DatabaseBuffRow>();
            var importedComponentLists = new Dictionary<long, List<long>>();
            var importedUpgradeItems = new Dictionary<long, long[]>();
            bool channelColumns = false;

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                // MinDamage is the field our server actually swings with, so it is the one worth
                // comparing. Index 0 only: a multi-index ability has several damage rows and pairing
                // them to tokens needs the component mapping this report is establishing.
                ReadRows(connection,
                    "SELECT Entry, MinDamage FROM mythic_src_ability_damage_heals WHERE `Index` = 0 AND MinDamage > 0",
                    reader => databaseDamage[Convert.ToInt64(reader[0], CultureInfo.InvariantCulture)] =
                        Convert.ToInt64(reader[1], CultureInfo.InvariantCulture));

                // Migration 10 added ChannelDuration and ChannelInterval. Against an older database only
                // whether an ability is a channel is compared.
                ReadRows(connection,
                    "SELECT COUNT(*) FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE()"
                    + " AND TABLE_NAME = 'mythic_src_abilities' AND COLUMN_NAME IN ('ChannelDuration', 'ChannelInterval')",
                    reader => channelColumns = Convert.ToInt64(reader[0], CultureInfo.InvariantCulture) == 2);

                ReadRows(connection,
                    "SELECT Entry, CareerLine, `Range`, CastTime, Cooldown, ApCost, EffectID, ChannelID, "
                    + (channelColumns ? "ChannelDuration, ChannelInterval" : "NULL, NULL") + " FROM mythic_src_abilities",
                    reader => databaseAbilities.Add(new DatabaseAbilityRow
                    {
                        Entry = Convert.ToInt64(reader[0], CultureInfo.InvariantCulture),
                        CareerLine = NullableLong(reader, 1),
                        Range = NullableLong(reader, 2),
                        CastTime = NullableLong(reader, 3),
                        Cooldown = NullableLong(reader, 4),
                        ApCost = NullableLong(reader, 5),
                        EffectId = NullableLong(reader, 6),
                        ChannelId = NullableLong(reader, 7),
                        ChannelDuration = NullableLong(reader, 8),
                        ChannelInterval = NullableLong(reader, 9)
                    }));

                ReadRows(connection,
                    "SELECT Entry, Duration, `Interval` FROM mythic_src_buff_infos",
                    reader => databaseBuffs[Convert.ToInt64(reader[0], CultureInfo.InvariantCulture)] = new DatabaseBuffRow
                    {
                        Duration = NullableLong(reader, 1),
                        Interval = NullableLong(reader, 2)
                    });

                ReadRows(connection,
                    "SELECT ID, MythicComponentData FROM mythic_bin_ability "
                    + "WHERE MythicComponentData IS NOT NULL AND MythicComponentData <> '[]'",
                    reader =>
                    {
                        List<AbilityTokenResolver.Component> parsed =
                            AbilityTokenResolver.ParseComponents(reader[1] as string);
                        if (parsed.Count > 0)
                            importedComponentLists[Convert.ToInt64(reader[0], CultureInfo.InvariantCulture)] =
                                parsed.Select(component => component.ComponentId).ToList();
                    });

                ReadRows(connection,
                    "SELECT b.UpgradeID, e.`Index`, e.V1, e.V2, e.V3, e.V4, e.V5, e.V6, e.V7, e.V8 "
                    + "FROM mythic_bin_abilityupgradeentry e JOIN mythic_bin_abilityupgradebin b ON b.ID = e.AbilityUpgradeBinID",
                    reader =>
                    {
                        var values = new long[8];
                        for (int slot = 0; slot < values.Length; ++slot)
                            values[slot] = NullableLong(reader, slot + 2) ?? 0;

                        importedUpgradeItems[UpgradeKey(Convert.ToInt64(reader[0], CultureInfo.InvariantCulture),
                            Convert.ToInt64(reader[1], CultureInfo.InvariantCulture))] = values;
                    });
            }

            FieldTally castTime = AddTally(report, "CastTime", "client ms = ours ms");
            FieldTally cooldown = AddTally(report, "Cooldown", "client ms = ours s x 1000");
            FieldTally range = AddTally(report, "Range", "client = ours ft x 12");
            FieldTally apCost = AddTally(report, "ApCost", "client = ours");
            FieldTally channel = AddTally(report, "Channel", "client FlagsRaw bit 22 = ours ChannelID set");
            FieldTally channelDuration = AddTally(report, "ChannelDuration", "client first timed component Duration ms = ours ms");
            FieldTally channelInterval = AddTally(report, "ChannelInterval", "client ChannelInterval ms = ours ms");
            FieldTally effectId = AddTally(report, "EffectID", "client EffectId = ours");
            FieldTally careerLine = AddTally(report, "CareerLine", "ours mask = 1 << (client career line - 1)");
            FieldTally duration = AddTally(report, "Duration", "first own DURA token ms = buff Duration s x 1000");
            FieldTally interval = AddTally(report, "Interval", "first own FREQ token ms = buff Interval ms");

            Func<long, List<AbilityTokenResolver.Component>> lookup = id =>
            {
                List<AbilityTokenResolver.Component> found;
                return components.TryGetValue(id, out found) ? found : null;
            };

            foreach (KeyValuePair<long, string> pair in descriptions)
            {
                long abilityId = pair.Key;
                List<AbilityTokenResolver.Token> tokens = AbilityTokenResolver.Parse(pair.Value);
                if (tokens.Count == 0)
                    continue;

                List<AbilityTokenResolver.Component> own = lookup(abilityId);
                string clientName = CleanName(Lookup(names, abilityId));

                AbilityTokenResolver.Resolution firstDamage = null;
                AbilityTokenResolver.Resolution firstDuration = null;
                AbilityTokenResolver.Resolution firstInterval = null;
                foreach (AbilityTokenResolver.Token token in tokens)
                {
                    AbilityTokenResolver.Resolution resolution = AbilityTokenResolver.Resolve(token, own, lookup);

                    // A duration or interval quoted from another ability says nothing about this
                    // ability's buff row, so only the ability's own tokens are compared.
                    if (resolution.Resolved && !token.ForeignAbilityId.HasValue)
                    {
                        if (token.Field == "DURA" && firstDuration == null)
                            firstDuration = resolution;
                        else if (token.Field == "FREQ" && firstInterval == null)
                            firstInterval = resolution;
                    }

                    if (!token.IsDamage)
                        continue;

                    ++report.DamageTokensSeen;
                    if (!resolution.Resolved)
                    {
                        ++report.DamageTokensUnresolved;
                        CountUnresolved(report, resolution.Failure, abilityId);
                        continue;
                    }

                    ++report.DamageTokensResolved;
                    if (firstDamage == null)
                        firstDamage = resolution;
                }

                CompareDamage(report, abilityId, clientName, firstDamage, databaseDamage);

                DatabaseBuffRow buff;
                if (databaseBuffs.TryGetValue(abilityId, out buff))
                {
                    if (firstDuration != null)
                        Compare(report, duration, abilityId, clientName, firstDuration.Token.Raw, firstDuration.Value,
                            buff.Duration, "mythic_src_buff_infos.Duration", SecondsToMilliseconds);
                    if (firstInterval != null)
                        Compare(report, interval, abilityId, clientName, firstInterval.Token.Raw, firstInterval.Value,
                            buff.Interval, "mythic_src_buff_infos.Interval", 1);
                }
            }

            report.DatabaseAbilityRows = databaseAbilities.Count;
            foreach (DatabaseAbilityRow row in databaseAbilities)
            {
                BinaryAbilityRecord client;
                if (row.Entry < 0 || row.Entry > ushort.MaxValue || !abilities.TryGetValue((ushort)row.Entry, out client))
                {
                    ++report.DatabaseRowsWithoutClientRecord;
                    continue;
                }

                string clientName = CleanName(Lookup(names, row.Entry));
                Compare(report, castTime, row.Entry, clientName, "abilityexport.bin CastTime", client.CastTime,
                    row.CastTime, "mythic_src_abilities.CastTime", 1);
                Compare(report, cooldown, row.Entry, clientName, "abilityexport.bin Cooldown", client.Cooldown,
                    row.Cooldown, "mythic_src_abilities.Cooldown", SecondsToMilliseconds);
                Compare(report, range, row.Entry, clientName, "abilityexport.bin Range", client.Range,
                    row.Range, "mythic_src_abilities.Range", FeetToClientRange);
                Compare(report, apCost, row.Entry, clientName, "abilityexport.bin ApCost", client.ApCost,
                    row.ApCost, "mythic_src_abilities.ApCost", 1);

                // A channel is FlagsRaw bit 22. Its length is the Duration of its first timed component --
                // the length the live server sent when a channel started -- and it spends AP every
                // ChannelInterval. Length and interval mean nothing unless both sides channel.
                bool oursChannel = (row.ChannelId ?? 0) > 0;
                Compare(report, channel, row.Entry, clientName, "abilityexport.bin FlagsRaw bit 22", client.IsChanneled ? 1 : 0,
                    oursChannel ? 1 : 0, "mythic_src_abilities.ChannelID set", 1);
                if (channelColumns && client.IsChanneled && oursChannel)
                {
                    Compare(report, channelDuration, row.Entry, clientName, "abilitycomponentexport.bin first timed Duration",
                        ClientChannelLength(lookup(row.Entry)), row.ChannelDuration, "mythic_src_abilities.ChannelDuration", 1);
                    Compare(report, channelInterval, row.Entry, clientName, "abilityexport.bin ChannelInterval",
                        client.ChannelInterval, row.ChannelInterval, "mythic_src_abilities.ChannelInterval", 1);
                }
                Compare(report, effectId, row.Entry, clientName, "abilityexport.bin EffectId", client.EffectId,
                    row.EffectId, "mythic_src_abilities.EffectID", 1);
                CompareCareerLine(report, careerLine, row.Entry, clientName, client.CareerLine, row.CareerLine);
            }

            CompareImportedComponentLists(report, abilities, importedComponentLists, names);
            CompareImportedUpgradeItems(report, upgradeTables, importedUpgradeItems);

            return report;
        }

        private static void CompareDamage(Report report, long abilityId, string clientName,
            AbilityTokenResolver.Resolution damage, Dictionary<long, long> databaseDamage)
        {
            long ours;
            if (damage == null || !databaseDamage.TryGetValue(abilityId, out ours))
                return;

            long client = damage.Value;
            string token = damage.Token.Raw;

            // NOT COMPARABLE: the tooltip is quoting a DIFFERENT ability's component. "Spine
            // Fling" (392) renders ABIL_7_COM_1, the pet's damage, while our damage row for 392
            // is the player ability's own. Counting that as a mismatch measures nothing but the
            // fact that they are two abilities.
            if (damage.Token.ForeignAbilityId.HasValue)
            {
                ++report.CrossReferenced;
                AddFinding(report, clientName, abilityId, "MinDamage", token, client, ours, KindCrossReference,
                    "the token resolves against ability " + damage.Token.ForeignAbilityId.Value
                    + ", so this is not our row's value to match");
                return;
            }

            // NOT COMPARABLE: the slot holds a reference, not a number. Some operations put a
            // component or ability id in Values[0] -- ability 5's component 142 carries 3682 --
            // and the client follows it rather than printing it. Values in the thousands beside
            // a two-digit MinDamage are that, not a balance change.
            if (client >= ReferenceLikeThreshold && ours < ReferenceLikeThreshold)
            {
                ++report.ReferenceLike;
                AddFinding(report, clientName, abilityId, "MinDamage", token, client, ours, KindReferenceLike,
                    "Values[slot] is " + client + ", an id rather than a damage number; the client follows it instead of printing it");
                return;
            }

            ++report.ComparableAbilities;
            if (ours == client)
            {
                ++report.Agreements;
                return;
            }

            ++report.Disagreements;
            AddFinding(report, clientName, abilityId, "MinDamage", token, client, ours, KindDamageDiffers,
                "client tooltip resolves to " + client + " (" + damage.RawValue + " x " + damage.MultiplierPercent
                + "%); mythic_src_ability_damage_heals.MinDamage is " + ours);
        }

        /// <summary>
        /// One column against the client. <paramref name="scale"/> converts ours into the client's
        /// unit, so a client value that is not a multiple of it cannot be held in our column at all.
        /// </summary>
        private static void Compare(Report report, FieldTally tally, long entry, string clientName, string clientSource,
            long clientValue, long? databaseValue, string databaseColumn, long scale)
        {
            if (!databaseValue.HasValue)
            {
                if (clientValue == 0)
                {
                    ++tally.DatabaseNull;
                    return;
                }

                ++tally.DatabaseEmpty;
                AddFinding(report, clientName, entry, tally.Field, clientSource, clientValue, 0, KindDatabaseEmpty,
                    databaseColumn + " is NULL; the client carries " + clientValue);
                return;
            }

            long ours = databaseValue.Value;
            string working = scale == 1
                ? databaseColumn + " is " + ours
                : databaseColumn + " is " + ours + " (x " + scale + " = " + (ours * scale) + ")";

            if (ours * scale == clientValue)
            {
                ++tally.Agreements;
                return;
            }

            if (ours == 0)
            {
                ++tally.DatabaseEmpty;
                AddFinding(report, clientName, entry, tally.Field, clientSource, clientValue, ours, KindDatabaseEmpty,
                    working + "; the client carries " + clientValue);
                return;
            }

            if (clientValue == 0)
            {
                ++tally.ClientEmpty;
                AddFinding(report, clientName, entry, tally.Field, clientSource, clientValue, ours, KindClientEmpty,
                    "the client carries 0; " + working);
                return;
            }

            if (clientValue % scale != 0)
            {
                ++tally.NotRepresentable;
                AddFinding(report, clientName, entry, tally.Field, clientSource, clientValue, ours, KindNotRepresentable,
                    "the client carries " + clientValue + ", not a multiple of " + scale + "; " + working);
                return;
            }

            ++tally.Disagreements;
            AddFinding(report, clientName, entry, tally.Field, clientSource, clientValue, ours, KindValueDiffers,
                "the client carries " + clientValue + "; " + working);
        }

        /// <summary>
        /// The client names one career line; ours stores a bit mask, bit (line - 1). A mask holding the
        /// client's bit among others grants the ability to more careers, which the client's single
        /// field cannot express, so it is set aside rather than counted either way.
        /// </summary>
        private static void CompareCareerLine(Report report, FieldTally tally, long entry, string clientName,
            uint clientLine, long? databaseMask)
        {
            const string source = "abilityexport.bin CareerLine";
            long expected = clientLine > 0 && clientLine <= 32 ? 1L << (int)(clientLine - 1) : 0;

            if (!databaseMask.HasValue)
            {
                if (clientLine == 0)
                {
                    ++tally.DatabaseNull;
                    return;
                }

                ++tally.DatabaseEmpty;
                AddFinding(report, clientName, entry, tally.Field, source, clientLine, 0, KindDatabaseEmpty,
                    "mythic_src_abilities.CareerLine is NULL; the client binds career line " + clientLine);
                return;
            }

            long mask = databaseMask.Value;
            if (mask == expected)
            {
                ++tally.Agreements;
                return;
            }

            if (mask == 0)
            {
                ++tally.DatabaseEmpty;
                AddFinding(report, clientName, entry, tally.Field, source, clientLine, mask, KindDatabaseEmpty,
                    "mythic_src_abilities.CareerLine is 0; the client binds career line " + clientLine + " (mask " + expected + ")");
                return;
            }

            if (clientLine == 0)
            {
                ++tally.ClientEmpty;
                AddFinding(report, clientName, entry, tally.Field, source, clientLine, mask, KindClientEmpty,
                    "the client binds no career line; mythic_src_abilities.CareerLine is mask " + mask);
                return;
            }

            if ((mask & expected) != 0)
            {
                ++tally.NotComparable;
                AddFinding(report, clientName, entry, tally.Field, source, clientLine, mask, KindSeveralCareerLines,
                    "mythic_src_abilities.CareerLine mask " + mask + " includes the client's career line " + clientLine
                    + " (mask " + expected + ") among others");
                return;
            }

            ++tally.Disagreements;
            AddFinding(report, clientName, entry, tally.Field, source, clientLine, mask, KindValueDiffers,
                "the client binds career line " + clientLine + " (mask " + expected + "); mythic_src_abilities.CareerLine is mask " + mask);
        }

        private static void CompareImportedComponentLists(Report report, Dictionary<ushort, BinaryAbilityRecord> abilities,
            Dictionary<long, List<long>> importedComponentLists, Dictionary<long, string> names)
        {
            foreach (BinaryAbilityRecord ability in abilities.Values.OrderBy(row => row.AbilityId))
            {
                List<long> client = ability.ComponentIds.Where(id => id != 0).Select(id => (long)id).ToList();
                string clientName = CleanName(Lookup(names, ability.AbilityId));

                List<long> imported;
                if (!importedComponentLists.TryGetValue(ability.AbilityId, out imported))
                {
                    if (client.Count == 0)
                        continue;

                    ++report.ClientListsWithoutImport;
                    AddFinding(report, clientName, ability.AbilityId, "ComponentIds", "abilityexport.bin ComponentIds",
                        client.Count, 0, KindImportDiffers,
                        "the client lists " + JoinIds(client) + "; mythic_bin_ability holds no component data for this ability");
                    continue;
                }

                ++report.ComponentListsCompared;
                if (client.SequenceEqual(imported))
                    continue;

                ++report.ComponentListsDiffer;
                AddFinding(report, clientName, ability.AbilityId, "ComponentIds", "abilityexport.bin ComponentIds",
                    client.Count, imported.Count, KindImportDiffers,
                    "the client lists " + JoinIds(client) + "; mythic_bin_ability.MythicComponentData lists " + JoinIds(imported));
            }

            report.ImportListsWithoutClientRecord = importedComponentLists.Keys
                .Count(id => id < 0 || id > ushort.MaxValue || !abilities.ContainsKey((ushort)id));
        }

        private static void CompareImportedUpgradeItems(Report report, List<BinaryUpgradeTableRecord> upgradeTables,
            Dictionary<long, long[]> importedUpgradeItems)
        {
            foreach (BinaryUpgradeTableRecord record in upgradeTables)
            {
                bool imported = false;
                foreach (BinaryUpgradeItemRecord item in record.Items)
                {
                    long[] ours;
                    if (!importedUpgradeItems.TryGetValue(UpgradeKey(record.UpgradeId, item.Index), out ours))
                        continue;

                    imported = true;
                    ++report.UpgradeItemsCompared;

                    long[] client = { item.V1, item.V2, item.V3, item.V4, item.V5, item.V6, item.V7, item.V8 };
                    int slot = 0;
                    while (slot < client.Length && client[slot] == ours[slot])
                        ++slot;

                    if (slot == client.Length)
                        continue;

                    ++report.UpgradeItemsDiffer;
                    AddFinding(report, string.Empty, record.UpgradeId, "UpgradeItem",
                        "upgradetableexport.bin record " + record.UpgradeId + " item " + item.Index + " V" + (slot + 1),
                        client[slot], ours[slot], KindImportDiffers,
                        "the client's V1-V8 are " + string.Join(", ", client)
                        + "; mythic_bin_abilityupgradeentry holds " + string.Join(", ", ours));
                }

                if (!imported)
                    ++report.UpgradeRecordsWithoutImport;
            }
        }

        /// <summary>The length the client gives a channel: the Duration of its first timed component, or 0.</summary>
        private static long ClientChannelLength(List<AbilityTokenResolver.Component> ordered)
        {
            if (ordered == null)
                return 0;

            foreach (AbilityTokenResolver.Component component in ordered)
            {
                if (component.Duration > 0)
                    return component.Duration;
            }

            return 0;
        }

        private static FieldTally AddTally(Report report, string field, string rule)
        {
            var tally = new FieldTally { Field = field, Rule = rule };
            report.Fields.Add(tally);
            return tally;
        }

        private static void AddFinding(Report report, string clientName, long entry, string field, string token,
            long clientValue, long databaseValue, string kind, string detail)
        {
            report.Findings.Add(new AbilityFinding
            {
                ClientName = clientName,
                DatabaseEntry = entry,
                Field = field,
                Token = token,
                ClientValue = clientValue,
                DatabaseValue = databaseValue,
                Kind = kind,
                Detail = detail
            });
        }

        private static void CountUnresolved(Report report, string failure, long abilityId)
        {
            // Bucket by shape, not by the specific numbers, or the histogram is useless.
            string reason = Regex.Replace(failure ?? "unknown", @"\d+", "N");
            int count;
            report.UnresolvedReasons.TryGetValue(reason, out count);
            report.UnresolvedReasons[reason] = count + 1;

            List<long> examples;
            if (!report.UnresolvedExamples.TryGetValue(reason, out examples))
            {
                examples = new List<long>();
                report.UnresolvedExamples[reason] = examples;
            }

            if (examples.Count < ExampleLimit && !examples.Contains(abilityId))
                examples.Add(abilityId);
        }

        private static void ReadRows(MySqlConnection connection, string sql, Action<MySqlDataReader> read)
        {
            using (var command = new MySqlCommand(sql, connection))
            {
                command.CommandTimeout = 300;
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        read(reader);
                }
            }
        }

        private static long? NullableLong(MySqlDataReader reader, int ordinal)
        {
            return reader.IsDBNull(ordinal)
                ? (long?)null
                : Convert.ToInt64(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
        }

        private static long UpgradeKey(long upgradeId, long index)
        {
            return (upgradeId << 8) | (index & 0xFF);
        }

        /// <summary>
        /// Reads an `id{tab}text` string table straight from the extracted client. These are
        /// UTF-16LE with a BOM, which StreamReader detects on its own.
        /// </summary>
        private Dictionary<long, string> LoadStringTable(string fileName)
        {
            var table = new Dictionary<long, string>();
            string path = Path.Combine(_extractedRoot, "data", "strings", "english", fileName);
            if (!File.Exists(path))
                return table;

            foreach (string line in File.ReadLines(path, Encoding.Unicode))
            {
                int tab = line.IndexOf('\t');
                if (tab <= 0)
                    continue;

                long id;
                if (!long.TryParse(line.Substring(0, tab).Trim(), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out id))
                    continue;

                table[id] = line.Substring(tab + 1);
            }

            return table;
        }

        private static string Lookup(Dictionary<long, string> table, long id)
        {
            string value;
            return table.TryGetValue(id, out value) ? value : null;
        }

        /// <summary>Drops the trailing gender/plural marker the client appends (`^n`, `^m`, `^f`).</summary>
        private static string CleanName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            int caret = name.LastIndexOf('^');
            return caret > 0 ? name.Substring(0, caret) : name;
        }

        public static string Write(Report report, string outputRoot, int cap)
        {
            string directory = Path.Combine(outputRoot, "crosswalk");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "ability-crosswalk.md");

            var text = new StringBuilder();
            text.AppendLine("# Ability crosswalk — the client's own numbers against ours");
            text.AppendLine();
            text.AppendLine("Generated by `ClientDataMatrix crosswalk abilities`. Read-only. The client side is read from the");
            text.AppendLine("extracted client itself — `abilityexport.bin`, `abilitycomponentexport.bin`, `upgradetableexport.bin`,");
            text.AppendLine("`abilitydesc.txt` and `abilitynames.txt` — never from the toolkit's import of those files.");
            text.AppendLine();
            text.AppendLine("## How a tooltip token is read");
            text.AppendLine();
            text.AppendLine("```");
            text.AppendLine("{ [ABIL_<abilityId>_] COM_<componentIndex> _ <field> _ <meaning> }");
            text.AppendLine("```");
            text.AppendLine();
            text.AppendLine("`componentIndex` indexes **that ability's own ordered component list**, not a component");
            text.AppendLine("id. `field` is `VAL<n>` for the nth entry of the component's `Values` array, or a scalar");
            text.AppendLine("(`DURA`, `FREQ`, `RADI`). `meaning` is presentation only. The optional `ABIL_<id>` prefix");
            text.AppendLine("addresses another ability's components.");
            text.AppendLine();
            text.AppendLine("Worked example — ability 7 *Spine Fling*, \"dealing `{COM_1_VAL0_DAMAGE}` every second for");
            text.AppendLine("`{COM_0_DURA_SECONDS}`\": its ordered components are `3301, 2`, so `COM_0` is 3301");
            text.AppendLine("(Duration 3000ms → three seconds) and `COM_1` is component 2, whose `Values` are");
            text.AppendLine("`15,0,0,0,0,0,0,0` → **15**, which is exactly our `MinDamage`.");
            text.AppendLine();
            text.AppendLine("The ordered list is `abilityexport.bin`'s `ComponentIds` in slot order, empty slots skipped, each");
            text.AppendLine("looked up in `abilitycomponentexport.bin`. **Do not** use the ability report's \"Related component");
            text.AppendLine("IDs\" line — that is a *sorted* set, and for ability 7 it prints `2, 3301`, which resolves `COM_0` to");
            text.AppendLine("the wrong component. Ability 1 agrees under both readings, so a one-ability check proves nothing.");
            text.AppendLine();

            text.AppendLine("## Does the reading hold? (tests the logic)");
            text.AppendLine();
            text.AppendLine("| | |");
            text.AppendLine("|---|---:|");
            text.AppendLine("| Abilities with a client description | " + N(report.AbilitiesWithDescriptions) + " |");
            text.AppendLine("| Abilities with client components | " + N(report.AbilitiesWithComponents) + " |");
            text.AppendLine("| Damage tokens found | " + N(report.DamageTokensSeen) + " |");
            text.AppendLine("| **Damage tokens resolved** | **" + N(report.DamageTokensResolved) + " ("
                + report.ResolutionRate.ToString("F2", CultureInfo.InvariantCulture) + "%)** |");
            text.AppendLine("| Unresolved | " + N(report.DamageTokensUnresolved) + " |");
            text.AppendLine();

            if (report.UnresolvedReasons.Count > 0)
            {
                text.AppendLine("Why the unresolved ones failed:");
                text.AppendLine();
                text.AppendLine("| Reason | Tokens | Abilities |");
                text.AppendLine("|---|---:|---|");
                foreach (KeyValuePair<string, int> pair in report.UnresolvedReasons.OrderByDescending(p => p.Value))
                {
                    List<long> examples;
                    report.UnresolvedExamples.TryGetValue(pair.Key, out examples);
                    text.AppendLine("| " + pair.Key + " | " + N(pair.Value) + " | " + JoinIds(examples) + " |");
                }

                text.AppendLine();
                text.AppendLine("*No components* means the client lists none for the ability. *An index past a non-empty list*");
                text.AppendLine("means the client's own record holds fewer components than its tooltip names. The lists come");
                text.AppendLine("from the client, so neither is an import gap, and a wrong ordering would scatter such failures");
                text.AppendLine("across the dataset rather than cluster them in one family of abilities.");
                text.AppendLine();
            }

            text.AppendLine("## Does our damage match? (tests the database)");
            text.AppendLine();
            text.AppendLine("| | |");
            text.AppendLine("|---|---:|");
            text.AppendLine("| Abilities comparable (token resolved **and** we hold a MinDamage) | " + N(report.ComparableAbilities) + " |");
            text.AppendLine("| **Agree with the client** | **" + N(report.Agreements) + " ("
                + report.AgreementRate.ToString("F2", CultureInfo.InvariantCulture) + "%)** |");
            text.AppendLine("| Disagree — candidate drift | " + N(report.Disagreements) + " |");
            text.AppendLine("| Excluded: tooltip quotes another ability | " + N(report.CrossReferenced) + " |");
            text.AppendLine("| Excluded: value slot holds a reference | " + N(report.ReferenceLike) + " |");
            text.AppendLine();
            text.AppendLine("The two exclusions are comparison errors, not data faults, and are listed below so the");
            text.AppendLine("judgement can be checked rather than taken on trust. A cross-reference token renders a");
            text.AppendLine("*different* ability's number — \"Spine Fling\" (392) shows `ABIL_7_COM_1`, the pet's damage —");
            text.AppendLine("so matching it against our row for 392 compares two unrelated abilities. A reference-like");
            text.AppendLine("value is an id the client follows rather than prints; ability 5's component 142 carries");
            text.AppendLine("3682 in `Values[0]`.");
            text.AppendLine();

            text.AppendLine("## Do our other columns match? (tests the database)");
            text.AppendLine();
            text.AppendLine("Every `mythic_src_abilities` row that has an `abilityexport.bin` record, plus each ability's first");
            text.AppendLine("own `DURA` and `FREQ` tooltip token against `mythic_src_buff_infos` (buff `Entry` = ability id).");
            text.AppendLine("The unit rules are measured, not assumed. **Differ** means both sides carry a value and they");
            text.AppendLine("disagree: that is candidate drift, and it is all the agreement rate counts. A value on one side");
            text.AppendLine("and none on the other is listed separately because it is often a convention — the client binds");
            text.AppendLine("the Squig pet abilities 6–9 to no career line while ours carry a mask — so read those before");
            text.AppendLine("acting on them. *Not representable* is a client value our column's unit cannot hold, such as a");
            text.AppendLine("4,500 ms cooldown in whole seconds. A channel is `FlagsRaw` bit 22, set on every ability the");
            text.AppendLine("captures show channelling; its length and AP tick are compared only where both sides channel.");
            text.AppendLine();
            text.AppendLine("| Column | Rule | Agree | Differ | Ours empty | Client empty | Not representable | Not comparable | Agreement |");
            text.AppendLine("|---|---|---:|---:|---:|---:|---:|---:|---:|");
            foreach (FieldTally tally in report.Fields)
            {
                text.AppendLine("| " + tally.Field + " | " + tally.Rule + " | " + N(tally.Agreements) + " | " + N(tally.Disagreements)
                    + " | " + N(tally.DatabaseEmpty) + " | " + N(tally.ClientEmpty) + " | " + N(tally.NotRepresentable)
                    + " | " + N(tally.NotComparable) + " | "
                    + tally.AgreementRate.ToString("F2", CultureInfo.InvariantCulture) + "% |");
            }

            text.AppendLine();
            text.AppendLine(N(report.DatabaseAbilityRows) + " `mythic_src_abilities` rows read; " + N(report.DatabaseRowsWithoutClientRecord)
                + " have no `abilityexport.bin` record and are not compared. A NULL where the client carries 0");
            text.AppendLine("is not counted at all.");
            text.AppendLine();

            text.AppendLine("### Not compared, and why");
            text.AppendLine();
            text.AppendLine("Measured 2026-09-13; each needs a mapping that has not been proven.");
            text.AppendLine();
            text.AppendLine("- **MinRange.** No client field is established. `Value44` equals `MinRange × 12` on the 12 rows");
            text.AppendLine("  where ours is non-zero (5 ft, 60) and is non-zero on 13 more where ours is 0, several of them");
            text.AppendLine("  *Charge* abilities — suggestive, but 25 rows are too few to name the field.");
            text.AppendLine("- **`RADI` tokens.** No mapping to our `EffectRadius` columns holds: of 384 tokens, 268 differ, 113");
            text.AppendLine("  have no radius on our side, and 3 match directly or at ×12.");
            text.AppendLine("- **Heal tokens** (`…HEALTH`). Which `mythic_src_ability_damage_heals` index a heal belongs to is");
            text.AppendLine("  unproven: matched against any index, 48 equal, 37 differ and 20 have no row.");
            text.AppendLine();

            text.AppendLine("## Is the toolkit import faithful? (tests `mythic_bin_*`)");
            text.AppendLine();
            text.AppendLine("`mythic_bin_ability` and `mythic_bin_abilityupgradeentry` are the toolkit's import of these same");
            text.AppendLine("client files. They are checked here rather than used, and anything read from them inherits");
            text.AppendLine("these differences.");
            text.AppendLine();
            text.AppendLine("| | Compared | Differ from the client | Client has, import lacks |");
            text.AppendLine("|---|---:|---:|---:|");
            text.AppendLine("| `mythic_bin_ability.MythicComponentData` component lists | " + N(report.ComponentListsCompared)
                + " | " + N(report.ComponentListsDiffer) + " | " + N(report.ClientListsWithoutImport) + " abilities |");
            text.AppendLine("| `mythic_bin_abilityupgradeentry` items (V1–V8) | " + N(report.UpgradeItemsCompared)
                + " | " + N(report.UpgradeItemsDiffer) + " | " + N(report.UpgradeRecordsWithoutImport) + " records |");
            text.AppendLine();
            text.AppendLine(N(report.ImportListsWithoutClientRecord) + " imported component lists belong to ability ids with no");
            text.AppendLine("`abilityexport.bin` record.");
            text.AppendLine();

            if (report.Findings.Count > 0)
            {
                text.AppendLine("## Findings");
                text.AppendLine();
                text.AppendLine("Grouped by column and kind, largest difference first, at most " + N(cap)
                    + " rows per group. Every row is in `ability-crosswalk.csv`.");
                text.AppendLine();

                foreach (IGrouping<string, AbilityFinding> group in report.Findings
                    .GroupBy(f => f.Field + "|" + f.Kind)
                    .OrderBy(g => Rank(FieldOrder, g.First().Field))
                    .ThenBy(g => Rank(KindOrder, g.First().Kind))
                    .ThenBy(g => g.Key, StringComparer.Ordinal))
                {
                    AbilityFinding first = group.First();
                    List<AbilityFinding> rows = group
                        .OrderByDescending(f => Distance(f))
                        .ThenBy(f => f.DatabaseEntry)
                        .ToList();

                    text.AppendLine("### " + first.Field + " — " + first.Kind + " (" + N(rows.Count) + ")");
                    text.AppendLine();
                    text.AppendLine("| Client name (`abilitynames.txt`) | DB entry | Client source | Client | Ours | Detail |");
                    text.AppendLine("|---|---:|---|---:|---:|---|");
                    foreach (AbilityFinding finding in rows.Take(cap))
                    {
                        text.AppendLine("| " + Cell(finding.ClientName) + " | " + finding.DatabaseEntry + " | `" + Cell(finding.Token)
                            + "` | " + finding.ClientValue + " | " + finding.DatabaseValue + " | " + Cell(finding.Detail) + " |");
                    }

                    if (rows.Count > cap)
                        text.AppendLine("| … | | | | | " + N(rows.Count - cap) + " more in `ability-crosswalk.csv` |");

                    text.AppendLine();
                }
            }

            File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));

            var csv = new StringBuilder();
            csv.AppendLine("ClientName,DatabaseEntry,Field,Token,ClientValue,DatabaseValue,Kind,Detail");
            foreach (AbilityFinding f in report.Findings.OrderBy(x => x.DatabaseEntry).ThenBy(x => Rank(FieldOrder, x.Field)))
            {
                csv.Append(Csv(f.ClientName)).Append(',').Append(f.DatabaseEntry).Append(',')
                   .Append(Csv(f.Field)).Append(',').Append(Csv(f.Token)).Append(',')
                   .Append(f.ClientValue).Append(',').Append(f.DatabaseValue).Append(',')
                   .Append(Csv(f.Kind)).Append(',').Append(Csv(f.Detail)).AppendLine();
            }
            File.WriteAllText(Path.Combine(directory, "ability-crosswalk.csv"), csv.ToString(), new UTF8Encoding(false));

            return directory;
        }

        /// <summary>How far apart a finding's two values are in the client's unit. Ordering only.</summary>
        private static long Distance(AbilityFinding finding)
        {
            switch (finding.Field)
            {
                case "Cooldown":
                case "Duration":
                    return Math.Abs(finding.ClientValue - finding.DatabaseValue * SecondsToMilliseconds);
                case "Range":
                    return Math.Abs(finding.ClientValue - finding.DatabaseValue * FeetToClientRange);
                case "CareerLine":
                    return 0; // a career line against a bit mask has no meaningful distance
                default:
                    return Math.Abs(finding.ClientValue - finding.DatabaseValue);
            }
        }

        private static int Rank(string[] order, string value)
        {
            int index = Array.IndexOf(order, value);
            return index < 0 ? order.Length : index;
        }

        private static string JoinIds(IList<long> ids)
        {
            return ids == null || ids.Count == 0
                ? "(none)"
                : string.Join(", ", ids.Select(id => id.ToString(CultureInfo.InvariantCulture)));
        }

        private static string Cell(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("|", "\\|");
        }

        private static string N(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string Csv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0
                ? value
                : "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Proves the COM-token reading against the whole dataset, and reports where our ability damage
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
    /// </summary>
    public sealed class AbilityCrosswalkService
    {
        public sealed class AbilityFinding
        {
            /// <summary>`abilitynames.txt` — the client's own name, the arbiter for ability names.</summary>
            public string ClientName { get; set; }

            public long DatabaseEntry { get; set; }
            public string Token { get; set; }
            public long ClientValue { get; set; }
            public long DatabaseValue { get; set; }
            public string Kind { get; set; }
            public string Detail { get; set; }
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

            public int ComparableAbilities;
            public int Agreements;
            public int Disagreements;

            /// <summary>Excluded: the tooltip quotes a different ability's component.</summary>
            public int CrossReferenced;

            /// <summary>Excluded: the value slot holds an id the client follows, not a number.</summary>
            public int ReferenceLike;

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

        /// <summary>
        /// Above this, a "damage" value is an id rather than a number. Component and ability ids run
        /// into the thousands while real ability damage at rank 40 is in the hundreds, so the two
        /// ranges do not overlap in practice. Only applied when our own value is small, so a genuine
        /// four-figure hit is never silently discarded.
        /// </summary>
        private const long ReferenceLikeThreshold = 1000;

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

            var components = new Dictionary<long, List<AbilityTokenResolver.Component>>();
            var databaseDamage = new Dictionary<long, long>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand(
                    "SELECT ID, MythicComponentData FROM mythic_bin_ability "
                    + "WHERE MythicComponentData IS NOT NULL AND MythicComponentData <> '[]'", connection))
                {
                    command.CommandTimeout = 300;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long id = Convert.ToInt64(reader[0], CultureInfo.InvariantCulture);
                            List<AbilityTokenResolver.Component> parsed =
                                AbilityTokenResolver.ParseComponents(reader[1] as string);

                            if (parsed.Count > 0)
                                components[id] = parsed;
                        }
                    }
                }

                report.AbilitiesWithComponents = components.Count;

                // MinDamage is the field our server actually swings with, so it is the one worth
                // comparing. Index 0 only: a multi-index ability has several damage rows and pairing
                // them to tokens needs the component mapping this report is establishing.
                using (var command = new MySqlCommand(
                    "SELECT Entry, MinDamage FROM mythic_src_ability_damage_heals "
                    + "WHERE `Index` = 0 AND MinDamage > 0", connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                            databaseDamage[Convert.ToInt64(reader[0], CultureInfo.InvariantCulture)] =
                                Convert.ToInt64(reader[1], CultureInfo.InvariantCulture);
                    }
                }
            }

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

                long? firstDamage = null;
                foreach (AbilityTokenResolver.Token token in tokens)
                {
                    if (!token.IsDamage)
                        continue;

                    ++report.DamageTokensSeen;
                    AbilityTokenResolver.Resolution resolution =
                        AbilityTokenResolver.Resolve(token, own, lookup);

                    if (!resolution.Resolved)
                    {
                        ++report.DamageTokensUnresolved;
                        string reason = resolution.Failure ?? "unknown";
                        // Bucket by shape, not by the specific numbers, or the histogram is useless.
                        reason = System.Text.RegularExpressions.Regex.Replace(reason, @"\d+", "N");
                        int count;
                        report.UnresolvedReasons.TryGetValue(reason, out count);
                        report.UnresolvedReasons[reason] = count + 1;
                        continue;
                    }

                    ++report.DamageTokensResolved;
                    if (!firstDamage.HasValue)
                        firstDamage = resolution.Value;
                }

                long ours;
                if (!firstDamage.HasValue || !databaseDamage.TryGetValue(abilityId, out ours))
                    continue;

                AbilityTokenResolver.Token damageToken = tokens.First(t => t.IsDamage);
                string clientName;
                names.TryGetValue(abilityId, out clientName);

                // NOT COMPARABLE: the tooltip is quoting a DIFFERENT ability's component. "Spine
                // Fling" (392) renders ABIL_7_COM_1, the pet's damage, while our damage row for 392
                // is the player ability's own. Counting that as a mismatch measures nothing but the
                // fact that they are two abilities.
                if (damageToken.ForeignAbilityId.HasValue)
                {
                    ++report.CrossReferenced;
                    report.Findings.Add(new AbilityFinding
                    {
                        ClientName = CleanName(clientName),
                        DatabaseEntry = abilityId,
                        Token = damageToken.Raw,
                        ClientValue = firstDamage.Value,
                        DatabaseValue = ours,
                        Kind = "not comparable: tooltip quotes another ability",
                        Detail = "the token resolves against ability " + damageToken.ForeignAbilityId.Value
                            + ", so this is not our row's value to match"
                    });
                    continue;
                }

                // NOT COMPARABLE: the slot holds a reference, not a number. Some operations put a
                // component or ability id in Values[0] -- ability 5's component 142 carries 3682 --
                // and the client follows it rather than printing it. Values in the thousands beside
                // a two-digit MinDamage are that, not a balance change.
                if (firstDamage.Value >= ReferenceLikeThreshold && ours < ReferenceLikeThreshold)
                {
                    ++report.ReferenceLike;
                    report.Findings.Add(new AbilityFinding
                    {
                        ClientName = CleanName(clientName),
                        DatabaseEntry = abilityId,
                        Token = damageToken.Raw,
                        ClientValue = firstDamage.Value,
                        DatabaseValue = ours,
                        Kind = "not comparable: slot holds a reference",
                        Detail = "Values[slot] is " + firstDamage.Value
                            + ", an id rather than a damage number; the client follows it instead of printing it"
                    });
                    continue;
                }

                ++report.ComparableAbilities;

                if (ours == firstDamage.Value)
                {
                    ++report.Agreements;
                }
                else
                {
                    ++report.Disagreements;
                    report.Findings.Add(new AbilityFinding
                    {
                        ClientName = CleanName(clientName),
                        DatabaseEntry = abilityId,
                        Token = damageToken.Raw,
                        ClientValue = firstDamage.Value,
                        DatabaseValue = ours,
                        Kind = "damage differs from client",
                        Detail = "client tooltip resolves to " + firstDamage.Value
                            + "; mythic_src_ability_damage_heals.MinDamage is " + ours
                    });
                }
            }

            return report;
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
            text.AppendLine("# Ability crosswalk — the client's own tooltip numbers against ours");
            text.AppendLine();
            text.AppendLine("Generated by `ClientDataMatrix crosswalk abilities`. Read-only.");
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
            text.AppendLine("The ordered list is `mythic_bin_ability.MythicComponentData`, ordered by each entry's");
            text.AppendLine("`Index`. **Do not** use the ability report's \"Related component IDs\" line — that is a");
            text.AppendLine("*sorted* set, and for ability 7 it prints `2, 3301`, which resolves `COM_0` to the wrong");
            text.AppendLine("component. Ability 1 agrees under both readings, so a one-ability check proves nothing.");
            text.AppendLine();

            text.AppendLine("## Does the reading hold? (tests the logic)");
            text.AppendLine();
            text.AppendLine("| | |");
            text.AppendLine("|---|---:|");
            text.AppendLine("| Abilities with a client description | " + N(report.AbilitiesWithDescriptions) + " |");
            text.AppendLine("| Abilities with parsed components | " + N(report.AbilitiesWithComponents) + " |");
            text.AppendLine("| Damage tokens found | " + N(report.DamageTokensSeen) + " |");
            text.AppendLine("| **Damage tokens resolved** | **" + N(report.DamageTokensResolved) + " ("
                + report.ResolutionRate.ToString("F2", CultureInfo.InvariantCulture) + "%)** |");
            text.AppendLine("| Unresolved | " + N(report.DamageTokensUnresolved) + " |");
            text.AppendLine();

            if (report.UnresolvedReasons.Count > 0)
            {
                text.AppendLine("Why the unresolved ones failed:");
                text.AppendLine();
                text.AppendLine("| Reason | Count |");
                text.AppendLine("|---|---:|");
                foreach (KeyValuePair<string, int> pair in report.UnresolvedReasons.OrderByDescending(p => p.Value))
                    text.AppendLine("| " + pair.Key + " | " + N(pair.Value) + " |");
                text.AppendLine();
            }

            text.AppendLine("## Does our data match? (tests the database)");
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

            if (report.Findings.Count > 0)
            {
                text.AppendLine("## Findings");
                text.AppendLine();
                text.AppendLine("| Client name (`abilitynames.txt`) | DB entry | Kind | Token | Client | DB MinDamage |");
                text.AppendLine("|---|---:|---|---|---:|---:|");

                int shown = 0;
                foreach (AbilityFinding finding in report.Findings
                    .OrderByDescending(f => Math.Abs(f.ClientValue - f.DatabaseValue)))
                {
                    if (shown++ >= cap)
                    {
                        text.AppendLine("| … | | | | " + N(report.Findings.Count - cap)
                            + " more, see `ability-crosswalk.csv` |");
                        break;
                    }

                    text.AppendLine("| " + finding.ClientName + " | " + finding.DatabaseEntry + " | " + finding.Kind + " | `"
                        + finding.Token + "` | " + finding.ClientValue + " | " + finding.DatabaseValue + " |");
                }
            }

            File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));

            var csv = new StringBuilder();
            csv.AppendLine("ClientName,DatabaseEntry,Token,ClientValue,DatabaseMinDamage,Kind,Detail");
            foreach (AbilityFinding f in report.Findings.OrderBy(x => x.DatabaseEntry))
            {
                csv.Append(Csv(f.ClientName)).Append(',').Append(f.DatabaseEntry).Append(',')
                   .Append(Csv(f.Token)).Append(',').Append(f.ClientValue).Append(',')
                   .Append(f.DatabaseValue).Append(',').Append(Csv(f.Kind)).Append(',')
                   .Append(Csv(f.Detail)).AppendLine();
            }
            File.WriteAllText(Path.Combine(directory, "ability-crosswalk.csv"), csv.ToString(), new UTF8Encoding(false));

            return directory;
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

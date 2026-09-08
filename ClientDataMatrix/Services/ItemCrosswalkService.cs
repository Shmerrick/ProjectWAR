using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Compares every row of the world database's item table against the client, and says which rows
    /// are holes and which are violations.
    ///
    /// WHY THE DISTINCTION MATTERS. `item_infos` is an amalgamation of several people's private
    /// databases -- WarEmu's public base, a released Return of Reckoning database, Londo's
    /// Mythic-connected data, and other contributions (see docs/CROSS_REPO.md). There is no single
    /// upstream authority for a row, so "our database says X" carries different weight in different
    /// id ranges, and a blanket trust or a blanket rewrite would both be wrong. What can be checked
    /// mechanically is whether a row is *coherent with the client*, and that splits cleanly:
    ///
    ///   VIOLATION -- the row asserts something the client contradicts. A ModelId that is not in
    ///                objects.csv is not a matter of opinion; nothing can render it.
    ///   HOLE      -- the row leaves something empty that the client could fill. Recoverable.
    ///   SUSPECT   -- the row looks self-damaged (a name beginning mid-word, a placeholder). The
    ///                client cannot arbitrate item names at all, so these are flagged, never
    ///                rewritten from a guess.
    ///
    /// Nothing here writes to the database. It emits a report and, on request, the SQL it would take
    /// to act -- because the correct action for a violation is frequently "find the right value",
    /// not "null the column", and that is a judgement call this tool has no standing to make.
    /// </summary>
    public sealed class ItemCrosswalkService
    {
        public enum Severity
        {
            Violation,
            Hole,
            Suspect
        }

        /// <summary>
        /// One row's disagreement with the client.
        ///
        /// NAMING IS DELIBERATE HERE. Every field that came out of our world database is prefixed
        /// `Database`, and the client's own wording is carried unprefixed under the name the client
        /// file gives it. Two different things get called "the name of item 2005602" -- Mythic's art
        /// calls it `tk_soultalisman_intelligence` and our table calls it "Omnipotent Myrmidon's
        /// Soul" -- and quoting either without saying which is how the wrong one ends up in a
        /// migration. The client's word wins the plain label; ours has to announce itself.
        /// </summary>
        public sealed class Finding
        {
            /// <summary>`item_infos.Entry`. Ours; the client has no item key at all.</summary>
            public long DatabaseEntry;

            /// <summary>`item_infos.Name`. Ours. No client file holds item display names.</summary>
            public string DatabaseName;

            /// <summary>`objects.csv` column `name` — what Mythic calls this art.</summary>
            public string ClientName;

            /// <summary>`item_infos.ModelId`, which addresses `objects.csv` column `ID`.</summary>
            public long DatabaseModelId;

            public Severity Severity;
            public string Kind;
            public string Detail;
        }

        public sealed class Report
        {
            public int ItemsExamined;
            public readonly List<Finding> Findings = new List<Finding>();
            public readonly Dictionary<string, int> CountsByKind = new Dictionary<string, int>(StringComparer.Ordinal);
            public string SourceTable;
            public int IconsResolved;
        }

        private readonly string _connectionString;
        private readonly ClientItemArtService _art;

        public ItemCrosswalkService(string connectionString, ClientItemArtService art)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A world database connection string is required.", "connectionString");
            if (art == null)
                throw new ArgumentNullException("art");

            _connectionString = connectionString;
            _art = art;
        }

        /// <summary>
        /// Reads the item table the running server would read and checks every row.
        /// </summary>
        /// <param name="table">
        /// `mythic_src_item_infos` or `item_infos`. The server picks by `UseMythicActionCoverageTables`
        /// in World.xml and ships true, so the mythic_src table is the meaningful default -- checking
        /// the other one describes rows nobody loads.
        /// </param>
        public Report Run(string table, HashSet<long> captureVerifiedEntries)
        {
            if (table != "mythic_src_item_infos" && table != "item_infos")
                throw new ArgumentException("Unknown item table: " + table, "table");

            var report = new Report { SourceTable = table };

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new MySqlCommand(
                    "SELECT Entry, Name, ModelId, Type, SlotId, MinRank, Rarity FROM " + table, connection))
                {
                    command.CommandTimeout = 120;
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            long entry = Convert.ToInt64(reader["Entry"], CultureInfo.InvariantCulture);
                            string name = reader["Name"] as string ?? string.Empty;
                            long modelId = Convert.ToInt64(reader["ModelId"], CultureInfo.InvariantCulture);

                            ++report.ItemsExamined;
                            Examine(report, entry, name, modelId, captureVerifiedEntries);
                        }
                    }
                }
            }

            foreach (Finding finding in report.Findings)
            {
                int count;
                report.CountsByKind.TryGetValue(finding.Kind, out count);
                report.CountsByKind[finding.Kind] = count + 1;
            }

            return report;
        }

        private void Examine(Report report, long entry, string name, long modelId,
            HashSet<long> captureVerifiedEntries)
        {
            // Resolved once up front so every finding can carry the client's own name for the art
            // alongside our database name, rather than quoting one of the two unlabelled.
            ClientItemArtService.ItemArt art = modelId == 0 ? null : _art.Resolve(modelId);
            string clientName = art == null ? null : art.ObjectName;

            // --- art -------------------------------------------------------------------------
            if (art == null)
            {
                Add(report, entry, name, clientName, modelId, Severity.Hole, "ModelId missing",
                    "ModelId is 0, so the item has no art at all");
            }
            else if (art.Failure != null && art.ObjectName == null)
            {
                Add(report, entry, name, clientName, modelId, Severity.Violation,
                    "ModelId not in objects.csv", art.Failure + " -- nothing can render this item");
            }
            else if (art.HasIcon)
            {
                ++report.IconsResolved;
            }
            else if (art.IconId < 0)
            {
                Add(report, entry, name, clientName, modelId, Severity.Hole, "art has no icon",
                    "objects.csv row " + modelId + " leaves Icon # blank");
            }
            else if (art.TextureName == null)
            {
                Add(report, entry, name, clientName, modelId, Severity.Violation, "icon not declared",
                    "objects.csv points at icon " + art.IconId + ", which icons.xml does not define");
            }
            else
            {
                Add(report, entry, name, clientName, modelId, Severity.Hole, "icon texture absent",
                    "icons.xml names " + art.TextureName + ", which is not in the extraction");
            }

            // --- name ------------------------------------------------------------------------
            // These are self-consistency checks on OUR data. The client holds no item display names,
            // so nothing here is a comparison -- a name is not "wrong", it is malformed.
            if (string.IsNullOrWhiteSpace(name))
            {
                Add(report, entry, name, clientName, modelId, Severity.Hole,
                    "database name empty", "no name at all");
            }
            else
            {
                if (char.IsLower(name[0]))
                {
                    bool placeholder = name.StartsWith("unk", StringComparison.Ordinal);
                    Add(report, entry, name, clientName, modelId, Severity.Suspect,
                        placeholder ? "database name is a placeholder" : "database name begins lowercase",
                        placeholder
                            ? "hand-added placeholder from the database merge"
                            : "a real item name does not begin mid-word -- likely a lost prefix");
                }

                // TRAILING SPACES ARE AUTHENTIC. Do not flag them, and do not let anyone "clean"
                // them. 12,672 item names end in a space, and on every one of the 36 that a packet
                // capture covers the live server sent the space too -- 36 of 36. They are Mythic's
                // data, not import damage. This is the same trap as the ^m/^f/^n caret suffixes,
                // where 5,210 rows were stripped as artifacts and the change had to be reverted out
                // of the base dump (CLAUDE.md hard rule 1). A leading space is a different matter
                // and would be suspicious, so it stays checked -- there are currently none.
                if (name.StartsWith(" ", StringComparison.Ordinal))
                {
                    Add(report, entry, name, clientName, modelId, Severity.Suspect,
                        "database name begins with a space",
                        "leading whitespace, which no captured name shows");
                }
                if (captureVerifiedEntries != null && captureVerifiedEntries.Contains(entry))
                {
                    // Recorded, not a finding: the useful signal is how thin this coverage is.
                }
            }
        }

        private static void Add(Report report, long entry, string databaseName, string clientName,
            long modelId, Severity severity, string kind, string detail)
        {
            report.Findings.Add(new Finding
            {
                DatabaseEntry = entry,
                DatabaseName = databaseName,
                ClientName = clientName,
                DatabaseModelId = modelId,
                Severity = severity,
                Kind = kind,
                Detail = detail
            });
        }

        /// <summary>
        /// Writes the report as markdown. Violations first, because those are the ones that break
        /// rendering rather than merely reading badly.
        /// </summary>
        public static string Write(Report report, string outputRoot, int perKindCap)
        {
            string directory = Path.Combine(outputRoot, "crosswalk");
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "item-crosswalk.md");

            var text = new StringBuilder();
            text.AppendLine("# Item crosswalk — world database against the client");
            text.AppendLine();
            text.AppendLine("Generated by `ClientDataMatrix crosswalk items`. Read-only; nothing here has been applied.");
            text.AppendLine();
            text.AppendLine("`item_infos` is an amalgamation of several contributors' databases (see `docs/CROSS_REPO.md`),");
            text.AppendLine("so there is no single upstream authority for a row. What can be checked mechanically is whether");
            text.AppendLine("a row is coherent with the client, which splits three ways:");
            text.AppendLine();
            text.AppendLine("- **Violation** — the row asserts something the client contradicts. Not a matter of opinion.");
            text.AppendLine("- **Hole** — the row leaves something empty that the client could fill.");
            text.AppendLine("- **Suspect** — the row looks self-damaged. The client cannot arbitrate item names, so these are");
            text.AppendLine("  flagged and never rewritten from a guess; the packet captures are the only source that can settle them.");
            text.AppendLine();
            text.AppendLine("**Naming.** Columns carrying Mythic's own wording are labelled by the client file they come from;");
            text.AppendLine("anything from our world database says so explicitly. Item 2005602 is `tk_soultalisman_intelligence`");
            text.AppendLine("to the client and \"Omnipotent Myrmidon's Soul\" to us — both are \"the name\", and quoting one without");
            text.AppendLine("saying which is how the wrong one ends up in a migration. Note that the client has **no item display");
            text.AppendLine("names at all**: `objects.csv` names the *art*, so a blank client name means that art row is unnamed,");
            text.AppendLine("never that the item is.");
            text.AppendLine();
            text.AppendLine("| | |");
            text.AppendLine("|---|---|");
            text.AppendLine("| Table read | `" + report.SourceTable + "` |");
            text.AppendLine("| Items examined | " + report.ItemsExamined.ToString("N0", CultureInfo.InvariantCulture) + " |");
            text.AppendLine("| Icons fully resolved | " + report.IconsResolved.ToString("N0", CultureInfo.InvariantCulture)
                + " (" + (100.0 * report.IconsResolved / Math.Max(1, report.ItemsExamined)).ToString("F1", CultureInfo.InvariantCulture) + "%) |");
            text.AppendLine("| Findings | " + report.Findings.Count.ToString("N0", CultureInfo.InvariantCulture) + " |");
            text.AppendLine();

            text.AppendLine("## Summary by kind");
            text.AppendLine();
            text.AppendLine("| Severity | Kind | Rows |");
            text.AppendLine("|---|---|---:|");

            foreach (Severity severity in new[] { Severity.Violation, Severity.Hole, Severity.Suspect })
            {
                foreach (KeyValuePair<string, int> pair in report.CountsByKind.OrderByDescending(p => p.Value))
                {
                    Finding sample = report.Findings.FirstOrDefault(f => f.Kind == pair.Key);
                    if (sample == null || sample.Severity != severity)
                        continue;

                    text.AppendLine("| " + severity + " | " + pair.Key + " | "
                        + pair.Value.ToString("N0", CultureInfo.InvariantCulture) + " |");
                }
            }

            foreach (Severity severity in new[] { Severity.Violation, Severity.Hole, Severity.Suspect })
            {
                List<Finding> group = report.Findings.Where(f => f.Severity == severity).ToList();
                if (group.Count == 0)
                    continue;

                text.AppendLine();
                text.AppendLine("## " + severity + " (" + group.Count.ToString("N0", CultureInfo.InvariantCulture) + ")");

                foreach (IGrouping<string, Finding> byKind in group.GroupBy(f => f.Kind).OrderByDescending(g => g.Count()))
                {
                    text.AppendLine();
                    text.AppendLine("### " + byKind.Key + " — " + byKind.Count().ToString("N0", CultureInfo.InvariantCulture) + " rows");
                    text.AppendLine();

                    // Client name first. Two things get called "the name" of an item -- Mythic's art
                    // name in objects.csv and ours in item_infos -- and a column headed plain "Name"
                    // is how the wrong one gets quoted into a migration.
                    text.AppendLine("| Client name (`objects.csv`) | DB entry | DB name (`item_infos.Name`) | Detail |");
                    text.AppendLine("|---|---:|---|---|");

                    int shown = 0;
                    foreach (Finding finding in byKind.OrderBy(f => f.DatabaseEntry))
                    {
                        if (shown++ >= perKindCap)
                        {
                            text.AppendLine("| … | | | "
                                + (byKind.Count() - perKindCap).ToString("N0", CultureInfo.InvariantCulture)
                                + " more, see `item-crosswalk.csv` |");
                            break;
                        }

                        text.AppendLine("| " + Escape(finding.ClientName ?? "—")
                            + " | " + finding.DatabaseEntry
                            + " | " + Escape(finding.DatabaseName)
                            + " | " + Escape(finding.Detail) + " |");
                    }
                }
            }

            File.WriteAllText(path, text.ToString(), new UTF8Encoding(false));

            // The uncapped companion, because a markdown cap once hid the true positives while
            // plausible nonsense sat at the top of the visible rows.
            string csvPath = Path.Combine(directory, "item-crosswalk.csv");
            var csv = new StringBuilder();
            csv.AppendLine("ClientName,DatabaseEntry,DatabaseName,DatabaseModelId,Severity,Kind,Detail");
            foreach (Finding finding in report.Findings.OrderBy(f => f.Severity).ThenBy(f => f.DatabaseEntry))
            {
                csv.Append(Csv(finding.ClientName)).Append(',')
                   .Append(finding.DatabaseEntry).Append(',')
                   .Append(Csv(finding.DatabaseName)).Append(',')
                   .Append(finding.DatabaseModelId).Append(',')
                   .Append(finding.Severity).Append(',')
                   .Append(Csv(finding.Kind)).Append(',')
                   .Append(Csv(finding.Detail)).AppendLine();
            }
            File.WriteAllText(csvPath, csv.ToString(), new UTF8Encoding(false));

            return directory;
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static string Csv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
                return value;

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Finds which columns of which client files point at which other files.
    ///
    /// This is the question that keeps costing time: a column holds integers and nobody knows what
    /// they index. `item_infos.ModelId` turned out to be `data/gamedata/objects.csv` -- 88,676 of
    /// 88,677 values resolve there -- but that took reading a spreadsheet by hand. Working it out
    /// mechanically means testing, for every integer column, what share of its values exist as keys
    /// of every keyed table, and reporting the ones that overwhelmingly do.
    ///
    /// WHAT THIS IS AND IS NOT. A high resolve rate is evidence of a link and nothing more. Small
    /// id spaces overlap by coincidence, which is why the thresholds below demand both a high rate
    /// AND a decent number of distinct values, and why the report prints those numbers instead of a
    /// verdict. abilities.csv agrees with the client's real ability ids on 13 of 3,115 -- a link
    /// that looked plausible and was catastrophically wrong -- so a candidate is a lead to check,
    /// never a conclusion to act on.
    /// </summary>
    public sealed class ClientLinkAnalyzer
    {
        /// <summary>A column that mostly resolves into another table's key.</summary>
        public sealed class LinkCandidate
        {
            public string FromTable;
            public string FromColumn;
            public int FromColumnIndex;
            public string ToTable;

            /// <summary>Distinct non-zero integers in the column.</summary>
            public int DistinctValues;

            /// <summary>How many of those exist as a key in the target.</summary>
            public int Resolved;

            /// <summary>
            /// How tightly packed the target's keys are: key count over the span they cover. A
            /// target at 1.0 is a contiguous run of ids, and ANY column of numbers inside that span
            /// resolves into it completely, whether or not it means to.
            /// </summary>
            public double TargetDensity;

            /// <summary>Rows showing the source value beside the target's name for it.</summary>
            public List<string> Samples = new List<string>();

            public double ResolveRate
            {
                get { return DistinctValues == 0 ? 0d : (double)Resolved / DistinctValues; }
            }

            /// <summary>
            /// How surprising the match is. Resolving into a sparse, scattered key set is evidence;
            /// resolving into a contiguous run of ids is nearly free. Used for ordering only -- it
            /// is a heuristic for what to read first, not a measure of truth.
            /// </summary>
            public double Specificity
            {
                get { return ResolveRate * (1d - TargetDensity); }
            }
        }

        /// <summary>
        /// Minimum share of a column's distinct values that must resolve before it is worth
        /// reporting. Set high on purpose: a partial overlap is far more often two id spaces that
        /// happen to start at 1 than a real reference.
        /// </summary>
        public const double MinimumResolveRate = 0.90d;

        /// <summary>
        /// Minimum distinct values before a column is considered at all. Below this, agreement
        /// carries no weight -- every table has keys 1..10.
        /// </summary>
        public const int MinimumDistinctValues = 50;

        private readonly Dictionary<string, ClientSourceCatalog.LoadedTable> _tables =
            new Dictionary<string, ClientSourceCatalog.LoadedTable>(StringComparer.OrdinalIgnoreCase);

        public void Add(ClientSourceCatalog.LoadedTable table)
        {
            if (table == null || table.Source == null || table.Error != null)
                return;

            // Only English strings take part in link analysis. The client ships the same string
            // tables in fourteen locales keyed identically, so including the rest multiplies every
            // candidate by fourteen and buries the readable one. The inventory still lists them all.
            if (table.Source.Family.StartsWith("strings/", StringComparison.OrdinalIgnoreCase)
                && !table.Source.Family.Equals("strings/english", StringComparison.OrdinalIgnoreCase))
                return;

            if (!_tables.ContainsKey(table.Source.RelativePath))
                _tables.Add(table.Source.RelativePath, table);
        }

        public int TableCount
        {
            get { return _tables.Count; }
        }

        public int KeyedTableCount
        {
            get { return _tables.Values.Count(t => t.HasUniqueIntegerKey && t.Keys.Count > 0); }
        }

        /// <summary>
        /// Every column that resolves into another table's key above the thresholds, strongest
        /// first. A column is never matched against its own table.
        /// </summary>
        public List<LinkCandidate> FindCandidates()
        {
            var candidates = new List<LinkCandidate>();

            List<ClientSourceCatalog.LoadedTable> targets = _tables.Values
                .Where(t => t.HasUniqueIntegerKey && t.Keys.Count >= MinimumDistinctValues)
                .ToList();

            foreach (ClientSourceCatalog.LoadedTable from in _tables.Values)
            {
                if (from.Rows.Count == 0)
                    continue;

                for (int column = 0; column < from.ColumnCount; ++column)
                {
                    // Column 0 is the row id by convention in every one of these files. A table's
                    // own key matching another table's is the single largest source of noise here.
                    if (column == 0)
                        continue;

                    HashSet<long> values = CollectIntegers(from, column);
                    if (values.Count < MinimumDistinctValues)
                        continue;

                    foreach (ClientSourceCatalog.LoadedTable to in targets)
                    {
                        if (ReferenceEquals(from, to))
                            continue;

                        int resolved = 0;
                        foreach (long value in values)
                        {
                            if (to.Keys.Contains(value))
                                ++resolved;
                        }

                        double rate = (double)resolved / values.Count;
                        if (rate < MinimumResolveRate)
                            continue;

                        var candidate = new LinkCandidate
                        {
                            FromTable = from.Source.RelativePath,
                            FromColumn = column < from.Columns.Count ? from.Columns[column] : "col" + column,
                            FromColumnIndex = column,
                            ToTable = to.Source.RelativePath,
                            DistinctValues = values.Count,
                            Resolved = resolved,
                            TargetDensity = Density(to)
                        };

                        BuildSamples(candidate, to, values);
                        candidates.Add(candidate);
                    }
                }
            }

            // Ordered by weight of evidence -- how many distinct values had to line up -- and NOT by
            // rate or by the specificity score. Both of those look like rankings of truth and are
            // not: a dense target swallows any column inside its range, and no arithmetic on these
            // numbers separates that from a real reference. The samples do.
            return candidates
                .OrderByDescending(c => c.DistinctValues)
                .ThenByDescending(c => c.ResolveRate)
                .ToList();
        }

        /// <summary>
        /// Key count over the span the keys cover. 1.0 means a contiguous run of ids.
        /// </summary>
        private static double Density(ClientSourceCatalog.LoadedTable table)
        {
            if (table.Keys.Count == 0)
                return 1d;

            long min = long.MaxValue;
            long max = long.MinValue;

            foreach (long key in table.Keys)
            {
                if (key < min) min = key;
                if (key > max) max = key;
            }

            long span = max - min + 1;
            return span <= 0 ? 1d : Math.Min(1d, (double)table.Keys.Count / span);
        }

        /// <summary>
        /// Puts a few source values beside whatever the target calls that row.
        ///
        /// This is the part that actually settles a link, and it is why the numbers alone are not
        /// enough. Nothing about 88,676 item rows resolving into objects.csv proves ModelId means
        /// art -- what proves it is that ModelId 8334 is named tk_soultalisman_intelligence and the
        /// item carrying it grants Intelligence. A reader can judge that in seconds; no threshold
        /// can.
        /// </summary>
        private static void BuildSamples(LinkCandidate candidate, ClientSourceCatalog.LoadedTable target,
            HashSet<long> values)
        {
            int nameColumn = FindNameColumn(target);
            if (nameColumn < 0)
                return;

            var byKey = new Dictionary<long, string>();
            foreach (string[] row in target.Rows)
            {
                if (row.Length == 0 || nameColumn >= row.Length)
                    continue;

                long key;
                if (long.TryParse(row[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out key)
                    && !byKey.ContainsKey(key))
                    byKey.Add(key, row[nameColumn].Trim());
            }

            foreach (long value in values.OrderBy(v => v))
            {
                string name;
                if (!byKey.TryGetValue(value, out name) || name.Length == 0)
                    continue;

                candidate.Samples.Add(value.ToString(CultureInfo.InvariantCulture) + " = " + name);
                if (candidate.Samples.Count >= 3)
                    break;
            }
        }

        /// <summary>
        /// The column holding a human-readable name, if there is one. Column 1 by convention in the
        /// client's CSVs and in the "id{tab}text" string tables.
        /// </summary>
        private static int FindNameColumn(ClientSourceCatalog.LoadedTable table)
        {
            if (table.ColumnCount < 2)
                return -1;

            for (int i = 1; i < table.ColumnCount && i < 3; ++i)
            {
                string header = table.Columns[i];
                if (string.IsNullOrEmpty(header))
                    continue;

                if (header.IndexOf("name", StringComparison.OrdinalIgnoreCase) >= 0
                    || header.Equals("Text", StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return 1;
        }

        /// <summary>
        /// Distinct positive integers in a column. Zero and negatives are excluded: zero is this
        /// data's universal "unset" and would inflate every comparison.
        /// </summary>
        private static HashSet<long> CollectIntegers(ClientSourceCatalog.LoadedTable table, int column)
        {
            var values = new HashSet<long>();

            foreach (string[] row in table.Rows)
            {
                if (column >= row.Length)
                    continue;

                string cell = row[column].Trim();
                if (cell.Length == 0)
                    continue;

                long value;
                if (!long.TryParse(cell, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    continue;

                if (value > 0)
                    values.Add(value);
            }

            return values;
        }
    }
}

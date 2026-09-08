using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Answers the two questions the client actually gets asked, without writing a report first.
    ///
    /// WHY. The inventory and link documents are worth having, but they are things to read, and
    /// every real question during restoration has been one of two shapes:
    ///
    ///   "where does the client mention this?"   -- searching 8,499 files by hand
    ///   "what IS id 8334?"                      -- opening a spreadsheet and scrolling
    ///
    /// Both were done by hand repeatedly while building this, which is the tool failing at its own
    /// purpose. <see cref="Find"/> and <see cref="Lookup"/> are those two questions.
    ///
    /// Tables are loaded one at a time and released, so a query costs a few hundred megabytes rather
    /// than holding 5.5 million rows at once, and skips the link analysis entirely.
    /// </summary>
    public static class ClientQueryService
    {
        public sealed class Match
        {
            public string RelativePath;
            public int RowIndex;
            public string Location;
            public string Content;
        }

        /// <summary>
        /// Every place the client mentions <paramref name="text"/>, case-insensitively.
        ///
        /// Searches cell contents, column headers and plain-text lines. Binary files are skipped:
        /// their "content" is a byte count.
        /// </summary>
        public static List<Match> Find(string root, string text, int limit)
        {
            var matches = new List<Match>();

            if (string.IsNullOrEmpty(text))
                return matches;

            var catalog = new ClientSourceCatalog(root);

            foreach (ClientSourceCatalog.ClientSource source in catalog.Discover())
            {
                if (source.Format == ClientSourceCatalog.SourceFormat.Binary)
                    continue;

                if (matches.Count >= limit)
                    break;

                ClientSourceCatalog.LoadedTable table = catalog.Load(source);
                if (table.Error != null)
                    continue;

                for (int i = 0; i < table.Rows.Count && matches.Count < limit; ++i)
                {
                    string[] row = table.Rows[i];

                    for (int c = 0; c < row.Length; ++c)
                    {
                        if (row[c] == null || row[c].IndexOf(text, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        matches.Add(new Match
                        {
                            RelativePath = source.RelativePath,
                            RowIndex = i,
                            Location = DescribeLocation(table, i, c),
                            Content = Summarise(row)
                        });

                        break; // One hit per row is enough to find the row.
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Every keyed table that holds <paramref name="id"/>, and what that row says.
        ///
        /// This is the "what is 8334?" question. Answering it meant opening objects.csv by hand and
        /// scrolling; the answer was tk_soultalisman_intelligence, and it settled a stat mapping.
        /// Asking every keyed file at once is the same work in a second, and shows when an id means
        /// different things in different files -- which, given how many id spaces this client has, is
        /// the thing most worth seeing.
        /// </summary>
        public static List<Match> Lookup(string root, long id)
        {
            var matches = new List<Match>();
            var catalog = new ClientSourceCatalog(root);

            foreach (ClientSourceCatalog.ClientSource source in catalog.Discover())
            {
                if (source.Format == ClientSourceCatalog.SourceFormat.Binary
                    || source.Format == ClientSourceCatalog.SourceFormat.PlainText)
                    continue;

                ClientSourceCatalog.LoadedTable table = catalog.Load(source);
                if (table.Error != null || !table.HasUniqueIntegerKey || !table.Keys.Contains(id))
                    continue;

                string wanted = id.ToString(CultureInfo.InvariantCulture);

                for (int i = 0; i < table.Rows.Count; ++i)
                {
                    string[] row = table.Rows[i];
                    if (row.Length == 0 || row[0].Trim() != wanted)
                        continue;

                    matches.Add(new Match
                    {
                        RelativePath = source.RelativePath,
                        RowIndex = i,
                        Location = "row " + (i + 1).ToString(CultureInfo.InvariantCulture),
                        Content = Summarise(row)
                    });

                    break;
                }
            }

            return matches;
        }

        private static string DescribeLocation(ClientSourceCatalog.LoadedTable table, int row, int column)
        {
            string header = column < table.Columns.Count && !string.IsNullOrWhiteSpace(table.Columns[column])
                ? table.Columns[column]
                : "col" + column.ToString(CultureInfo.InvariantCulture);

            return "row " + (row + 1).ToString(CultureInfo.InvariantCulture) + ", " + header;
        }

        /// <summary>
        /// A row rendered short enough to scan. Trailing empty cells are dropped -- these files pad
        /// every row out to the widest one, so a three-field row can carry 250 commas.
        /// </summary>
        private static string Summarise(string[] row)
        {
            int last = row.Length - 1;
            while (last >= 0 && string.IsNullOrWhiteSpace(row[last]))
                --last;

            if (last < 0)
                return string.Empty;

            var text = new StringBuilder();
            for (int i = 0; i <= last && i < 12; ++i)
            {
                if (i > 0)
                    text.Append(" | ");

                string cell = row[i] ?? string.Empty;
                if (cell.Length > 60)
                    cell = cell.Substring(0, 57) + "...";

                text.Append(cell);
            }

            if (last >= 12)
                text.Append(" | ...");

            return text.ToString();
        }
    }
}

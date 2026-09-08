using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Writes the client's every id-to-name pair as one flat, greppable file.
    ///
    /// WHY THIS AND NOT A COMMAND. `find` and `lookup` each re-walk and re-parse 8,499 files, about
    /// ten seconds a question. That is fine occasionally and wrong as a habit: the questions come in
    /// runs of twenty, and a tool that costs a process start per question gets skipped in favour of
    /// guessing, which is how wrong names get into the database in the first place.
    ///
    /// This exports once. Afterwards "what does the client call 8334?" is a grep against a local
    /// file measured in milliseconds, with no process, no parse and -- the point -- no query against
    /// the world database. Checking the client becomes cheaper than checking ourselves, which is the
    /// only arrangement in which it will actually happen every time.
    ///
    /// FORMAT. Tab-separated, four columns, no header:
    ///
    ///     data/gamedata/objects.csv{tab}8334{tab}Name{tab}tk_soultalisman_intelligence
    ///
    /// Tabs and newlines inside a name are replaced with spaces so one row is always one line and
    /// grep -P "\t8334\t" is exact. The path is first because the file an id belongs to is part of
    /// its meaning: 8334 is a talisman's art in objects.csv and an ability called Dreadful Agony in
    /// abilitynames.txt, and an id quoted without its file is the mistake this whole tool exists to
    /// prevent.
    ///
    /// The third column is the client's own header for the value beside it -- "Textual Name",
    /// "icon", "type". It is carried because matching Mythic's vocabulary is half the point of
    /// reading the client at all, and because it says which field you are looking at when a table
    /// has no column called anything so obliging as Name.
    /// </summary>
    public static class ClientIndexExporter
    {
        public static string Write(string extractedRoot, string outputRoot)
        {
            var catalog = new ClientSourceCatalog(extractedRoot);

            string directory = Path.Combine(outputRoot, "client-sources");
            Directory.CreateDirectory(directory);

            string indexPath = Path.Combine(directory, "client-index.tsv");
            long rows = 0;
            int files = 0;

            using (var writer = new StreamWriter(indexPath, false, new UTF8Encoding(false)))
            {
                foreach (ClientSourceCatalog.ClientSource source in catalog.Discover())
                {
                    if (source.Format == ClientSourceCatalog.SourceFormat.Binary
                        || source.Format == ClientSourceCatalog.SourceFormat.PlainText
                        || source.Format == ClientSourceCatalog.SourceFormat.Xml)
                        continue;

                    // English only. The client ships the same string tables in fourteen locales
                    // keyed identically, which triples the file and buries the readable line under
                    // thirteen translations of it -- a grep for one id returned 47 rows of which 43
                    // were the same sentence in other languages. The question this index answers is
                    // what Mythic called something, and that is the English row.
                    if (IsNonEnglishLocale(source.RelativePath))
                        continue;

                    ClientSourceCatalog.LoadedTable table = catalog.Load(source);
                    if (table.Error != null || !table.HasUniqueIntegerKey)
                        continue;

                    int nameColumn = ChooseNameColumn(table);
                    if (nameColumn < 0)
                        continue;

                    ++files;

                    string field = Clean(nameColumn < table.Columns.Count
                        && !string.IsNullOrWhiteSpace(table.Columns[nameColumn])
                        ? table.Columns[nameColumn]
                        : "col" + nameColumn.ToString(CultureInfo.InvariantCulture));

                    foreach (string[] row in table.Rows)
                    {
                        if (row.Length == 0)
                            continue;

                        string id = row[0].Trim();
                        if (id.Length == 0)
                            continue;

                        writer.Write(source.RelativePath);
                        writer.Write('\t');
                        writer.Write(id);
                        writer.Write('\t');
                        writer.Write(field);
                        writer.Write('\t');
                        writer.Write(Clean(nameColumn < row.Length ? row[nameColumn] : string.Empty));
                        writer.Write('\n');
                        ++rows;
                    }
                }
            }

            Console.WriteLine("Indexed " + rows.ToString("N0", CultureInfo.InvariantCulture)
                + " rows from " + files.ToString("N0", CultureInfo.InvariantCulture) + " keyed files.");

            return indexPath;
        }

        /// <summary>
        /// Which column after the key actually holds a name, or -1 if none does.
        ///
        /// WHY NOT JUST COLUMN 1. It was column 1, and that was wrong twice over. In
        /// zones/*/fixtures.csv column 1 is "NIF #" and column 2 is "Textual Name", so 440,913 rows
        /// -- 53% of the index -- carried a mesh number where their name was sitting one column
        /// over. In itemdata.csv column 1 is "icon", a number, hiding the readable "type" (SWORD,
        /// ASHLD) behind it. Both printed as digits or blanks, which reads as "the client does not
        /// name this" when the client names it perfectly well. An index that quietly answers "no
        /// name" is worse than no index, because the answer looks like evidence.
        ///
        /// So: take the first column whose values are mostly words rather than numbers. Sampling is
        /// capped because a decision this coarse does not improve after a few hundred rows, and
        /// these tables run to 65,000.
        /// </summary>
        private static int ChooseNameColumn(ClientSourceCatalog.LoadedTable table)
        {
            const int MaxSampledRows = 400;
            const int MaxSampledColumns = 24;

            int width = 0;
            for (int i = 0; i < table.Rows.Count && i < MaxSampledRows; ++i)
                if (table.Rows[i].Length > width)
                    width = table.Rows[i].Length;

            if (width > MaxSampledColumns)
                width = MaxSampledColumns;

            for (int column = 1; column < width; ++column)
            {
                int populated = 0;
                int textual = 0;

                for (int i = 0; i < table.Rows.Count && i < MaxSampledRows; ++i)
                {
                    string[] row = table.Rows[i];
                    if (column >= row.Length)
                        continue;

                    string cell = (row[column] ?? string.Empty).Trim();
                    if (cell.Length == 0)
                        continue;

                    ++populated;
                    if (!LooksNumeric(cell))
                        ++textual;
                }

                // Half is deliberately lenient. Real name columns carry blanks and the odd purely
                // numeric name ("100 Gold"), and a column that is half words is still the most
                // readable thing in the row.
                if (populated > 0 && textual * 2 >= populated)
                    return column;
            }

            // Nothing readable anywhere: a pure numeric lookup table. Indexing it by name would add
            // rows that can never answer a name question, and every such row is one more line a grep
            // for an id has to be read past.
            return -1;
        }

        private static bool LooksNumeric(string cell)
        {
            for (int i = 0; i < cell.Length; ++i)
            {
                char c = cell[i];
                if (!char.IsDigit(c) && c != '-' && c != '+' && c != '.' && c != 'e' && c != 'E')
                    return false;
            }

            return true;
        }

        /// <summary>
        /// True for a string table in a language other than English. Matched on the path segment
        /// after data/strings, so data/strings/english and everything outside data/strings pass.
        /// </summary>
        private static bool IsNonEnglishLocale(string relativePath)
        {
            const string prefix = "data/strings/";

            if (!relativePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            int start = prefix.Length;
            int end = relativePath.IndexOf('/', start);
            if (end < 0)
                return false;

            string locale = relativePath.Substring(start, end - start);
            return !locale.Equals("english", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// One row must stay one line. Caret suffixes are left alone: ^n, ^m and ^f are grammatical
        /// gender markers the client itself uses, and stripping them once cost 5,210 rows of the
        /// world database and had to be reverted.
        /// </summary>
        private static string Clean(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Replace('\t', ' ')
                .Replace('\r', ' ')
                .Replace('\n', ' ');
        }
    }
}

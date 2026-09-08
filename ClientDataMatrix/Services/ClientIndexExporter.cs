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
    /// FORMAT. Tab-separated, three columns, no header:
    ///
    ///     data/gamedata/objects.csv{tab}8334{tab}tk_soultalisman_intelligence
    ///
    /// Tabs and newlines inside a name are replaced with spaces so one row is always one line and
    /// grep -P "\t8334\t" is exact. The path is first because the file an id belongs to is part of
    /// its meaning: 8334 is a talisman's art in objects.csv and an ability called Dreadful Agony in
    /// abilitynames.txt, and an id quoted without its file is the mistake this whole tool exists to
    /// prevent.
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

                    ++files;

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
                        writer.Write(Clean(row.Length > 1 ? row[1] : string.Empty));
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

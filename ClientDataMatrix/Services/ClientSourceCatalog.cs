using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Reads every data file in the extracted client, not a hand-picked handful.
    ///
    /// WHY THIS EXISTS. <see cref="Model.AbilityDataset"/> loads eight files, all of them about
    /// abilities. The client ships 103 CSVs, 6 XML files, 56 English string tables and 68 zone map
    /// definitions, and the ones nobody had read turned out to matter: it was
    /// data/gamedata/objects.csv that finally settled what a talisman's stat is, and
    /// interface/interfacecore/maps/zone191/mappoints.xml that carried the tomb glyph costs. Both
    /// were sitting there the whole time.
    ///
    /// Restoration keeps running aground on the same thing -- a table in our database says one
    /// thing, the client says another, and there is no cheap way to find out which files even bear
    /// on the question. This class makes the whole client surface enumerable and loadable through
    /// one interface so that becomes a query rather than an afternoon.
    ///
    /// NOTHING HERE INTERPRETS. It reports what a file contains, its shape and its key, and stops.
    /// Deciding that a column means something is the caller's job, and wants evidence -- reading a
    /// column as an id because the numbers looked right is exactly how mythic_src_abilities came to
    /// be populated from an art-authoring sheet keyed in an unrelated id space.
    /// </summary>
    public sealed class ClientSourceCatalog
    {
        public enum SourceFormat
        {
            /// <summary>Comma-separated, one or two header rows, integer id in column 0.</summary>
            HeaderedCsv,

            /// <summary>UTF-16 "id{tab}text" table under data/strings/&lt;locale&gt;.</summary>
            IndexedStringTable,

            Xml,

            /// <summary>Text with no table structure: Lua UI source, notes, unkeyed lists.</summary>
            PlainText,

            /// <summary>Binary. Size is recorded; contents are not interpreted.</summary>
            Binary,

            Unknown
        }

        public sealed class ClientSource
        {
            /// <summary>Path relative to the extracted root, always with forward slashes.</summary>
            public string RelativePath;

            /// <summary>Short name for reports: the file name without its extension.</summary>
            public string Name;

            /// <summary>Grouping for reports -- gamedata, strings/english, maps/zone191, ...</summary>
            public string Family;

            public SourceFormat Format;
            public long ByteSize;

            public override string ToString()
            {
                return RelativePath;
            }
        }

        public sealed class LoadedTable
        {
            public ClientSource Source;

            /// <summary>Column headers, best available. Empty for a format that has none.</summary>
            public List<string> Columns = new List<string>();

            /// <summary>Data rows only. Header and comment rows are not included.</summary>
            public List<string[]> Rows = new List<string[]>();

            /// <summary>Rows skipped before the data began, plus in-file comment rows.</summary>
            public int HeaderRows;
            public int CommentRows;

            /// <summary>True when column 0 holds an integer that is unique across every data row.</summary>
            public bool HasUniqueIntegerKey;

            /// <summary>Populated when <see cref="HasUniqueIntegerKey"/>; the distinct key values.</summary>
            public HashSet<long> Keys = new HashSet<long>();

            /// <summary>How many rows repeat an id already seen. Zero for a strict key.</summary>
            public int DuplicateKeys;

            /// <summary>How many rows carry no parseable id in column 0.</summary>
            public int UnkeyedRows;

            /// <summary>Non-empty lines seen while reading, for the string-table parse ratio.</summary>
            public int NonEmptyLines;

            public string Error;

            /// <summary>True when the file could only be read partially -- malformed XML.</summary>
            public bool Degraded;

            public int ColumnCount
            {
                get { return Columns.Count; }
            }
        }

        private readonly string _root;

        public ClientSourceCatalog(string extractedRoot)
        {
            if (string.IsNullOrWhiteSpace(extractedRoot))
                throw new ArgumentException("Extracted client root is required.", "extractedRoot");

            _root = extractedRoot;
        }

        public string Root
        {
            get { return _root; }
        }

        /// <summary>
        /// Extensions worth reading. Everything else in the extraction is art -- .dds textures,
        /// .nif meshes, .wav audio -- which is 170,000 of the 178,000 files and says nothing about
        /// how the game is wired.
        /// </summary>
        private static readonly HashSet<string> DataExtensions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".csv", ".xml", ".txt", ".lua", ".ini", ".dat" };

        /// <summary>
        /// How many of a table's rows may repeat an id before column 0 stops counting as its key.
        /// </summary>
        private const double MaxDuplicateKeyRate = 0.01d;

        /// <summary>
        /// Share of a .txt file's non-empty lines that must parse as an entry before it counts as a
        /// string table rather than prose.
        /// </summary>
        private const double MinimumStringTableParseRate = 0.80d;

        public List<ClientSource> Discover()
        {
            var sources = new List<ClientSource>();

            if (!Directory.Exists(_root))
                return sources;

            // Walks the whole extraction rather than a list of directories somebody thought to add.
            // An earlier version hard-coded three, which reached 701 files out of the 8,026 that
            // carry data -- it missed every string table below the top level of data/strings, and
            // the whole of interface/, where the client's own UI source states what it expects the
            // server to send.
            foreach (string file in EnumerateFilesSafely(_root).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                string extension = Path.GetExtension(file);
                if (!DataExtensions.Contains(extension))
                    continue;

                FileInfo info;
                try
                {
                    info = new FileInfo(file);
                }
                catch (IOException)
                {
                    continue;
                }

                string relative = file.Substring(_root.Length).TrimStart('\\', '/').Replace('\\', '/');
                int lastSlash = relative.LastIndexOf('/');

                sources.Add(new ClientSource
                {
                    RelativePath = relative,
                    Name = Path.GetFileNameWithoutExtension(info.Name),
                    Family = lastSlash <= 0 ? "(root)" : relative.Substring(0, lastSlash),
                    Format = ClassifyByExtension(extension),
                    ByteSize = info.Length
                });
            }

            return sources;
        }

        /// <summary>
        /// Depth-first walk that skips directories it cannot enter instead of abandoning the sweep.
        /// The extraction is often still running when this is used, so a directory can vanish or be
        /// locked between being listed and being opened.
        /// </summary>
        private static IEnumerable<string> EnumerateFilesSafely(string root)
        {
            var pending = new Stack<string>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                string directory = pending.Pop();

                string[] subdirectories;
                try
                {
                    subdirectories = Directory.GetDirectories(directory);
                }
                catch (Exception)
                {
                    continue;
                }

                foreach (string subdirectory in subdirectories)
                    pending.Push(subdirectory);

                string[] files;
                try
                {
                    files = Directory.GetFiles(directory);
                }
                catch (Exception)
                {
                    continue;
                }

                foreach (string file in files)
                    yield return file;
            }
        }

        private static SourceFormat ClassifyByExtension(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".csv": return SourceFormat.HeaderedCsv;
                case ".xml": return SourceFormat.Xml;

                // A .txt is usually an "id{tab}text" table but not always -- notes and unkeyed
                // lists share the extension -- so the loader sniffs the first lines and downgrades
                // to PlainText when the shape is wrong, rather than reporting an empty table.
                case ".txt": return SourceFormat.IndexedStringTable;

                case ".lua":
                case ".ini": return SourceFormat.PlainText;
                case ".dat": return SourceFormat.Binary;
                default: return SourceFormat.Unknown;
            }
        }

        /// <summary>
        /// Loads one source into columns and rows. Never throws for a malformed file: the failure is
        /// recorded on the table so a single bad file cannot stop a sweep of two hundred.
        /// </summary>
        public LoadedTable Load(ClientSource source)
        {
            var table = new LoadedTable { Source = source };

            try
            {
                string path = Path.Combine(_root, source.RelativePath.Replace('/', Path.DirectorySeparatorChar));

                switch (source.Format)
                {
                    case SourceFormat.HeaderedCsv:
                        LoadCsv(path, table);
                        break;

                    case SourceFormat.IndexedStringTable:
                        // Not every .txt is a keyed table. If too few lines are "int{tab}text" the
                        // file is something else -- a note, a filename list -- and is recorded as
                        // plain text rather than reported as a table with no rows.
                        LoadIndexedStrings(path, table);
                        if (!LooksLikeStringTable(table))
                        {
                            table.Rows.Clear();
                            table.Columns.Clear();
                            table.NonEmptyLines = 0;
                            source.Format = SourceFormat.PlainText;
                            LoadPlainText(path, table);
                        }
                        break;

                    case SourceFormat.Xml:
                        LoadXmlShape(path, table);
                        break;

                    case SourceFormat.PlainText:
                        LoadPlainText(path, table);
                        break;

                    case SourceFormat.Binary:
                        table.Columns.Add("Bytes");
                        table.Rows.Add(new[] { source.ByteSize.ToString(CultureInfo.InvariantCulture) });
                        break;
                }

                IndexKeys(table);
            }
            catch (Exception error)
            {
                table.Error = error.GetType().Name + ": " + error.Message;
            }

            return table;
        }

        /// <summary>
        /// Reads a client CSV.
        ///
        /// The header is however many leading rows do not start with an integer -- one in
        /// itemdata.csv, two in abilities.csv and objects.csv, where the first row groups columns and
        /// the second names them. The last such row is kept as the column names because it is the
        /// more specific of the two.
        ///
        /// Rows beginning with ';' are authoring comments and appear THROUGHOUT the data, not only at
        /// the top -- abilities.csv is full of them. They are counted and dropped. Treating them as
        /// data is how that file's ids drift out of step with the client's real ones.
        /// </summary>
        private static void LoadCsv(string path, LoadedTable table)
        {
            string[] lines = ReadLines(path);
            bool inData = false;
            List<string> lastHeader = null;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                List<string> cells = SplitCsvLine(line);
                string first = cells.Count > 0 ? cells[0].Trim() : string.Empty;

                if (first.StartsWith(";", StringComparison.Ordinal))
                {
                    ++table.CommentRows;
                    continue;
                }

                long ignored;
                bool isDataRow = long.TryParse(first, NumberStyles.Integer, CultureInfo.InvariantCulture, out ignored);

                if (!isDataRow && !inData)
                {
                    lastHeader = cells;
                    ++table.HeaderRows;
                    continue;
                }

                if (!isDataRow)
                {
                    // A row of nothing but separators is a spacer left by whatever spreadsheet
                    // produced the file, and carries no content. anim_db.csv has 423 of them
                    // scattered through 41,000 rows; keeping them made its id column fail to parse
                    // and cost the file its key, and with it every link into the animation data.
                    if (IsBlankRow(cells))
                        continue;

                    // A non-numeric row with content once the data has started is not a header.
                    // Keep it rather than silently dropping something nobody has characterised.
                    table.Rows.Add(cells.ToArray());
                    continue;
                }

                inData = true;
                table.Rows.Add(cells.ToArray());
            }

            if (lastHeader != null)
                table.Columns.AddRange(lastHeader.Select(c => c.Trim()));

            EnsureColumnCount(table);
        }

        /// <summary>True when every cell in the row is empty or whitespace.</summary>
        private static bool IsBlankRow(List<string> cells)
        {
            for (int i = 0; i < cells.Count; ++i)
            {
                if (!string.IsNullOrWhiteSpace(cells[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Reads a "id{tab}text" string table. These are UTF-16 and carry the client's caret
        /// suffixes (^n, ^m, ^f), which are grammatical gender markers and are preserved verbatim --
        /// stripping them once cost 5,210 rows of the world database and had to be reverted.
        /// </summary>
        private static void LoadIndexedStrings(string path, LoadedTable table)
        {
            table.Columns.Add("Id");
            table.Columns.Add("Text");

            foreach (string line in ReadLines(path))
            {
                string trimmed = line.TrimEnd('\r', '\n');
                if (trimmed.Length == 0)
                    continue;

                ++table.NonEmptyLines;

                int tab = trimmed.IndexOf('\t');

                // An entry whose text is empty may drop the tab entirely and be nothing but its id.
                // scenarionames.txt is 2,208 entries of which 2,151 are written that way; requiring
                // a tab discarded all of them and left the file looking like 57 rows, which then
                // made it look like something other than a string table.
                //
                // An id with no text is still the client saying that id exists, which is exactly the
                // kind of thing this tool is asked.
                if (tab < 0)
                {
                    long bareId;
                    if (long.TryParse(trimmed.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out bareId))
                        table.Rows.Add(new[] { bareId.ToString(CultureInfo.InvariantCulture), string.Empty });

                    continue;
                }

                if (tab == 0)
                    continue;

                long id;
                if (!long.TryParse(trimmed.Substring(0, tab).Trim(), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out id))
                    continue;

                table.Rows.Add(new[]
                {
                    id.ToString(CultureInfo.InvariantCulture),
                    trimmed.Substring(tab + 1)
                });
            }
        }

        /// <summary>
        /// True when the file is a keyed string table rather than prose that happens to end in .txt.
        ///
        /// Judged on the SHARE of lines that parsed, not on a count. "Two or more rows parsed" -- the
        /// earlier test -- called an 8,054-line file a string table on the strength of two lines that
        /// happened to start with a number, and 71 files were classified that way.
        /// </summary>
        private static bool LooksLikeStringTable(LoadedTable table)
        {
            if (table.Rows.Count < 2 || table.NonEmptyLines == 0)
                return false;

            return (double)table.Rows.Count / table.NonEmptyLines >= MinimumStringTableParseRate;
        }

        /// <summary>
        /// Keeps a text file as lines. Lua UI source is the reason this exists: it is not a table,
        /// but it is the client stating what it expects -- the contested-instance lobby's handler
        /// signature and its sixty-second timeout were read straight out of it -- so it belongs in
        /// the inventory even though nothing can be joined against it.
        /// </summary>
        private static void LoadPlainText(string path, LoadedTable table)
        {
            table.Columns.Add("Line");

            foreach (string line in ReadLines(path))
                table.Rows.Add(new[] { line });

            // Trailing newline produces one empty final line; not worth reporting as content.
            if (table.Rows.Count > 0 && table.Rows[table.Rows.Count - 1][0].Length == 0)
                table.Rows.RemoveAt(table.Rows.Count - 1);
        }

        /// <summary>
        /// Records an XML file's element shape rather than parsing it into rows: one row per distinct
        /// element name, with how many times it occurs and which attributes it carries. Enough to see
        /// what a file holds and decide whether it is worth reading properly.
        /// </summary>
        private static void LoadXmlShape(string path, LoadedTable table)
        {
            table.Columns.Add("Element");
            table.Columns.Add("Count");
            table.Columns.Add("Attributes");

            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            var attributes = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);

            // Same encoding detection as every other reader here. This used to default to
            // Windows-1252, which mangles a UTF-8 XML file that carries no byte order mark in the
            // opposite direction.
            string raw = ReadAllText(path);

            System.Xml.Linq.XDocument document;
            try
            {
                document = System.Xml.Linq.XDocument.Parse(raw);
            }
            catch (System.Xml.XmlException)
            {
                // Some of these files are not strictly well-formed and the client reads them
                // anyway: interface/interfacecore/maps/zone006/mappoints.xml carries a landmark
                // named "Pick & Goggles" with a bare ampersand. Refusing the file would lose a whole
                // zone's map data over a character the game itself accepts, so escape the stray
                // ampersands and retry once.
                try
                {
                    document = System.Xml.Linq.XDocument.Parse(EscapeBareAmpersands(raw));
                }
                catch (System.Xml.XmlException)
                {
                    // Six files are malformed past that -- unescaped '<' inside attributes,
                    // '=' inside element names, curly quotes around attribute values. keybindings.xml
                    // and command.xml are among them, so refusing to read them would mean the tool
                    // could not report on the client's own key and command definitions at all.
                    // Fall back to counting element names textually and say the read was degraded.
                    ScanElementsTextually(raw, table);
                    return;
                }
            }

            foreach (System.Xml.Linq.XElement element in document.Descendants())
            {
                string name = element.Name.LocalName;

                int count;
                counts.TryGetValue(name, out count);
                counts[name] = count + 1;

                SortedSet<string> names;
                if (!attributes.TryGetValue(name, out names))
                {
                    names = new SortedSet<string>(StringComparer.Ordinal);
                    attributes.Add(name, names);
                }

                foreach (System.Xml.Linq.XAttribute attribute in element.Attributes())
                    names.Add(attribute.Name.LocalName);
            }

            foreach (KeyValuePair<string, int> pair in counts.OrderByDescending(p => p.Value).ThenBy(p => p.Key))
            {
                table.Rows.Add(new[]
                {
                    pair.Key,
                    pair.Value.ToString(CultureInfo.InvariantCulture),
                    string.Join(" ", attributes[pair.Key])
                });
            }
        }

        /// <summary>
        /// Decides whether column 0 is a usable key. A table whose first column is a unique integer
        /// can be joined against; one whose is not cannot, and saying so up front stops a caller
        /// assuming otherwise.
        /// </summary>
        private static void IndexKeys(LoadedTable table)
        {
            if (table.Source.Format == SourceFormat.Xml
                || table.Source.Format == SourceFormat.PlainText
                || table.Source.Format == SourceFormat.Binary
                || table.Rows.Count == 0)
                return;

            var keys = new HashSet<long>();
            int duplicates = 0;
            int unkeyed = 0;

            foreach (string[] row in table.Rows)
            {
                if (row.Length == 0)
                {
                    ++unkeyed;
                    continue;
                }

                long key;
                if (!long.TryParse(row[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out key))
                {
                    // A row with no id at all. anim_db.csv has 325 of them among 41,000 -- section
                    // labels sitting in the name column ",Dwarf Male Core,,," and partial rows with
                    // timings but no id. Spreadsheet leavings, not records.
                    ++unkeyed;
                    continue;
                }

                if (!keys.Add(key))
                    ++duplicates;
            }

            // A few repeated ids do not stop a column being the key. anim_db.csv has 41,009 distinct
            // ids and 41 that repeat; refusing it outright -- which an earlier version did, bailing
            // on the first duplicate -- discarded a 41,000-row reference table over a tenth of a
            // percent of its rows, and with it every link into the client's animation data.
            //
            // The tolerance is deliberately small. A column where ids repeat freely is a category or
            // a foreign key, not an identity, and treating one as joinable would invent links.
            if (keys.Count == 0)
                return;

            double impureRate = (double)(duplicates + unkeyed) / table.Rows.Count;
            if (impureRate > MaxDuplicateKeyRate)
                return;

            table.HasUniqueIntegerKey = true;
            table.DuplicateKeys = duplicates;
            table.UnkeyedRows = unkeyed;
            table.Keys = keys;
        }

        private static void EnsureColumnCount(LoadedTable table)
        {
            int widest = 0;
            foreach (string[] row in table.Rows)
            {
                if (row.Length > widest)
                    widest = row.Length;
            }

            while (table.Columns.Count < widest)
            {
                table.Columns.Add("col" + table.Columns.Count.ToString(CultureInfo.InvariantCulture));
            }
        }

        /// <summary>
        /// Counts element names by text search, for XML no parser will accept. Attributes are not
        /// recovered -- the file is malformed in the attribute syntax, which is exactly why the
        /// parser refused it -- so this reports which elements are present and how often, and marks
        /// the result degraded rather than pretending to a full read.
        /// </summary>
        private static void ScanElementsTextually(string raw, LoadedTable table)
        {
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < raw.Length - 1; ++i)
            {
                if (raw[i] != '<')
                    continue;

                char next = raw[i + 1];
                if (!char.IsLetter(next) && next != '_')
                    continue;

                int end = i + 1;
                while (end < raw.Length && (char.IsLetterOrDigit(raw[end]) || raw[end] == '_' || raw[end] == '-'))
                    ++end;

                string name = raw.Substring(i + 1, end - i - 1);
                if (name.Length == 0)
                    continue;

                int count;
                counts.TryGetValue(name, out count);
                counts[name] = count + 1;
                i = end - 1;
            }

            foreach (KeyValuePair<string, int> pair in counts.OrderByDescending(p => p.Value).ThenBy(p => p.Key))
            {
                table.Rows.Add(new[]
                {
                    pair.Key,
                    pair.Value.ToString(CultureInfo.InvariantCulture),
                    "(not parsed: malformed XML)"
                });
            }

            table.Degraded = true;
        }

        /// <summary>
        /// Escapes ampersands that do not already begin a character entity, leaving real entities
        /// such as &amp;amp; and &amp;#39; untouched.
        /// </summary>
        private static string EscapeBareAmpersands(string xml)
        {
            var repaired = new StringBuilder(xml.Length + 16);

            for (int i = 0; i < xml.Length; ++i)
            {
                if (xml[i] != '&')
                {
                    repaired.Append(xml[i]);
                    continue;
                }

                // An entity is & then up to a few name or numeric characters then ';'.
                int end = -1;
                for (int j = i + 1; j < xml.Length && j <= i + 10; ++j)
                {
                    char c = xml[j];
                    if (c == ';')
                    {
                        end = j;
                        break;
                    }

                    if (!char.IsLetterOrDigit(c) && c != '#')
                        break;
                }

                if (end > i + 1)
                {
                    repaired.Append(xml, i, end - i + 1);
                    i = end;
                }
                else
                    repaired.Append("&amp;");
            }

            return repaired.ToString();
        }

        private static string[] ReadLines(string path)
        {
            return ReadAllText(path).Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// Reads a client text file with the right encoding.
        ///
        /// Three cases, and getting this wrong is silent. A byte order mark settles it outright, and
        /// the string tables carry one. Without a mark, strict UTF-8 is tried: if the bytes are valid
        /// UTF-8 they are UTF-8, and if they are not the file is Windows-1252, which is what a 2008
        /// toolchain produced.
        ///
        /// 86 files in the extraction are non-UTF-8 with no mark, including nine in data/gamedata and
        /// fifteen under data/strings. Reading them as UTF-8 -- which an earlier version did, passing
        /// UTF8 as the fallback encoding -- replaces every accented character with U+FFFD, so a name
        /// silently stops matching anything it is compared against. That is exactly the class of
        /// failure this tool exists to catch, so it must not commit it.
        /// </summary>
        private static string ReadAllText(string path)
        {
            byte[] bytes;

            // Opened shared so a running extraction or an open spreadsheet cannot block a sweep.
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var memory = new MemoryStream())
            {
                stream.CopyTo(memory);
                bytes = memory.ToArray();
            }

            if (bytes.Length >= 2)
            {
                if (bytes[0] == 0xFF && bytes[1] == 0xFE)
                    return Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2);

                if (bytes[0] == 0xFE && bytes[1] == 0xFF)
                    return Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2);
            }

            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return new UTF8Encoding(false).GetString(bytes, 3, bytes.Length - 3);

            try
            {
                // Throws rather than substituting, so invalid bytes are detected instead of hidden.
                return new UTF8Encoding(false, true).GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                return Encoding.GetEncoding(1252).GetString(bytes);
            }
        }

        private static List<string> SplitCsvLine(string line)
        {
            var columns = new List<string>();
            if (line == null)
            {
                columns.Add(string.Empty);
                return columns;
            }

            var current = new StringBuilder(line.Length);
            bool inQuotes = false;

            for (int index = 0; index < line.Length; ++index)
            {
                char c = line[index];

                if (c == '"')
                {
                    if (inQuotes && index + 1 < line.Length && line[index + 1] == '"')
                    {
                        current.Append('"');
                        ++index;
                    }
                    else
                        inQuotes = !inQuotes;

                    continue;
                }

                if (c == ',' && !inQuotes)
                {
                    columns.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(c);
            }

            columns.Add(current.ToString());
            return columns;
        }
    }
}

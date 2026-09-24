using ClientDataMatrix.Parsers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace ClientDataMatrix.Model
{
    public sealed class AbilityDataset
    {
        private static readonly Regex ClientFormattingRegex = new Regex(@"\^[A-Za-z]", RegexOptions.Compiled);

        private Dictionary<uint, List<ClientAbilityRecord>> _clientAbilitiesByEffectId = new Dictionary<uint, List<ClientAbilityRecord>>();
        private Dictionary<ushort, List<BinaryAbilityRecord>> _binaryAbilitiesById = new Dictionary<ushort, List<BinaryAbilityRecord>>();

        public string RootPath { get; private set; }
        public List<TableLoadStatus> TableStatuses { get; private set; }
        public List<ClientAbilityRecord> ClientAbilities { get; private set; }
        public List<ClientEffectRecord> ClientEffects { get; private set; }
        public List<IndexedStringRecord> AbilityNames { get; private set; }
        public List<IndexedStringRecord> AbilityDescriptions { get; private set; }
        public List<IndexedStringRecord> AbilityEffectTexts { get; private set; }
        public List<IndexedStringRecord> RaceNames { get; private set; }
        public List<IndexedStringRecord> CareerNames { get; private set; }
        public List<IndexedStringRecord> CareerLines { get; private set; }
        public List<IndexedStringRecord> ComponentEffects { get; private set; }
        public List<PregameCharacterRecord> PregameCharacters { get; private set; }
        public List<BinaryAbilityRecord> BinaryAbilities { get; private set; }
        public List<BinaryComponentRecord> BinaryComponents { get; private set; }
        public List<BinaryRequirementRecord> BinaryRequirements { get; private set; }
        public List<BinaryUpgradeTableRecord> BinaryUpgradeTables { get; private set; }

        public static AbilityDataset Load(string rootPath)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentException("Extracted root path is required.", "rootPath");

            AbilityDataset dataset = new AbilityDataset
            {
                RootPath = Path.GetFullPath(rootPath),
                TableStatuses = new List<TableLoadStatus>()
            };

            dataset.ClientAbilities = dataset.LoadAbilities(@"data\gamedata\abilities.csv");
            dataset.ClientEffects = dataset.LoadEffects(@"data\gamedata\effects.csv");
            dataset.AbilityNames = dataset.LoadIndexedStrings(@"data\strings\english\abilitynames.txt", "client_strings", "abilitynames.txt", "ability_names", "english");
            dataset.AbilityDescriptions = dataset.LoadIndexedStrings(@"data\strings\english\abilitydesc.txt", "client_strings", "abilitydesc.txt", "ability_descriptions", "english");
            dataset.AbilityEffectTexts = dataset.LoadIndexedStrings(@"data\strings\english\abilityeffect.txt", "client_strings", "abilityeffect.txt", "ability_effect_text", "english");
            dataset.RaceNames = dataset.LoadIndexedStrings(@"data\strings\english\racenames_m.txt", "client_strings", "racenames_m.txt", "race_names_male", "english");
            dataset.CareerNames = dataset.LoadIndexedStrings(@"data\strings\english\careernames_m.txt", "client_strings", "careernames_m.txt", "career_names_male", "english");
            dataset.CareerLines = dataset.LoadIndexedStrings(@"data\strings\english\careerlines_m.txt", "client_strings", "careerlines_m.txt", "career_lines_male", "english");
            dataset.ComponentEffects = dataset.LoadIndexedStrings(@"data\strings\english\componenteffects.txt", "client_strings", "componenteffects.txt", "component_effects", "english");
            dataset.PregameCharacters = dataset.LoadPregameCharacters(@"data\gamedata\pregame_chars.xml");
            dataset.BinaryAbilities = dataset.LoadFile(@"data\bin\abilityexport.bin", "client_bin", "abilityexport.bin", AbilityBinaryParser.ParseAbilityExport);
            dataset.BinaryComponents = dataset.LoadFile(@"data\bin\abilitycomponentexport.bin", "client_bin", "abilitycomponentexport.bin", AbilityBinaryParser.ParseAbilityComponentExport);
            dataset.BinaryRequirements = dataset.LoadFile(@"data\bin\abilityrequirementexport.bin", "client_bin", "abilityrequirementexport.bin", AbilityBinaryParser.ParseAbilityRequirementExport);
            dataset.BinaryUpgradeTables = dataset.LoadFile(@"data\bin\upgradetableexport.bin", "client_bin", "upgradetableexport.bin", AbilityBinaryParser.ParseUpgradeTableExport);

            dataset.ApplyComponentDescriptions();
            dataset.BuildIndexes();

            return dataset;
        }

        /// <summary>abilities.csv rows for an effect id -- the sheet's own key. See <see cref="ClientAbilityRecord"/>.</summary>
        public List<ClientAbilityRecord> GetClientAbilityRowsByEffectId(int effectId)
        {
            List<ClientAbilityRecord> rows;
            return effectId > 0 && _clientAbilitiesByEffectId.TryGetValue((uint)effectId, out rows)
                ? rows
                : new List<ClientAbilityRecord>();
        }

        /// <summary>
        /// abilities.csv rows for an ability, reached through the EffectId on its abilityexport.bin
        /// record -- the only correct join. An ability with no BIN record, or EffectId 0, has none.
        /// </summary>
        public List<ClientAbilityRecord> GetClientAbilityRowsForAbility(ushort abilityId)
        {
            List<BinaryAbilityRecord> binRows;
            return _binaryAbilitiesById.TryGetValue(abilityId, out binRows)
                ? GetClientAbilityRowsForBinaryRows(binRows)
                : new List<ClientAbilityRecord>();
        }

        public List<ClientAbilityRecord> GetClientAbilityRowsForBinaryRows(IEnumerable<BinaryAbilityRecord> binRows)
        {
            return (binRows ?? Enumerable.Empty<BinaryAbilityRecord>())
                .Select(row => (int)row.EffectId)
                .Where(effectId => effectId > 0)
                .Distinct()
                .SelectMany(effectId => GetClientAbilityRowsByEffectId(effectId))
                .OrderBy(row => row.LineNumber)
                .ToList();
        }

        private void BuildIndexes()
        {
            _clientAbilitiesByEffectId = ClientAbilities
                .GroupBy(row => row.EffectId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.LineNumber).ToList());
            _binaryAbilitiesById = BinaryAbilities
                .GroupBy(row => row.AbilityId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.RecordIndex).ToList());
        }

        private List<ClientAbilityRecord> LoadAbilities(string relativePath)
        {
            return LoadFile(relativePath, "client_csv", "abilities.csv", fullPath =>
            {
                List<ClientAbilityRecord> rows = new List<ClientAbilityRecord>();
                string[] lines = ReadAllLinesShared(fullPath);

                for (int index = 5; index < lines.Length; ++index)
                {
                    string line = lines[index];
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    List<string> columns = SplitCsvLine(line);
                    // The ID column is an effect id, not an ability id -- see ClientAbilityRecord.
                    uint effectId;
                    if (!TryParseUInt(GetColumn(columns, 0), out effectId))
                        continue;

                    rows.Add(new ClientAbilityRecord
                    {
                        SourceFamily = "client_csv",
                        TableName = "abilities.csv",
                        SourcePath = fullPath,
                        RowKey = "line=" + (index + 1).ToString(CultureInfo.InvariantCulture) + ";EffectId=" + effectId.ToString(CultureInfo.InvariantCulture),
                        LineNumber = index + 1,
                        RawRow = line,
                        EffectId = effectId,
                        Name = GetColumn(columns, 1),
                        Description = GetColumn(columns, 2),
                        Notes = GetColumn(columns, 3),
                        IconId = ParseInt(GetColumn(columns, 4)),
                        AnimationId = ParseInt(GetColumn(columns, 7)),
                        SpecialEffectId = ParseInt(GetColumn(columns, 8)),
                        PlaybackAnimationId = ParseNullableInt(GetColumn(columns, 9)),
                        ActivateAggro = ParseNullableInt(GetColumn(columns, 12))
                    });
                }

                return rows;
            });
        }

        private List<ClientEffectRecord> LoadEffects(string relativePath)
        {
            return LoadFile(relativePath, "client_csv", "effects.csv", fullPath =>
            {
                List<ClientEffectRecord> rows = new List<ClientEffectRecord>();
                string[] lines = ReadAllLinesShared(fullPath);

                for (int index = 5; index < lines.Length; ++index)
                {
                    string line = lines[index];
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    List<string> columns = SplitCsvLine(line);
                    uint effectId;
                    if (!TryParseUInt(GetColumn(columns, 0), out effectId))
                        continue;

                    rows.Add(new ClientEffectRecord
                    {
                        SourceFamily = "client_csv",
                        TableName = "effects.csv",
                        SourcePath = fullPath,
                        RowKey = "line=" + (index + 1).ToString(CultureInfo.InvariantCulture) + ";EffectId=" + effectId.ToString(CultureInfo.InvariantCulture),
                        LineNumber = index + 1,
                        RawRow = line,
                        EffectId = effectId,
                        Name = GetColumn(columns, 1),
                        BuildUpId = ParseInt(GetColumn(columns, 2)),
                        ActivateId = ParseInt(GetColumn(columns, 3)),
                        CastId = ParseInt(GetColumn(columns, 4)),
                        ProjectileId = ParseInt(GetColumn(columns, 5)),
                        ImpactId = ParseInt(GetColumn(columns, 6)),
                        AoeId = ParseInt(GetColumn(columns, 7)),
                        ChannelingId = ParseInt(GetColumn(columns, 8)),
                        VfxId = ParseInt(GetColumn(columns, 9)),
                        AoeTarget = ParseInt(GetColumn(columns, 10)),
                        AoeEffectsPerSecond = ParseInt(GetColumn(columns, 11)),
                        AoeRadius = ParseInt(GetColumn(columns, 12)),
                        AoeDuration = ParseInt(GetColumn(columns, 13)),
                        AoeLocation = ParseInt(GetColumn(columns, 14)),
                        WeaponTrail = ParseInt(GetColumn(columns, 15)),
                        ProjectileOffset = ParseInt(GetColumn(columns, 16)),
                        ProjectileOverride = ParseInt(GetColumn(columns, 17))
                    });
                }

                return rows;
            });
        }

        private List<IndexedStringRecord> LoadIndexedStrings(string relativePath, string sourceFamily, string tableName, string stringSet, string locale)
        {
            return LoadFile(relativePath, sourceFamily, tableName, fullPath =>
            {
                List<IndexedStringRecord> rows = new List<IndexedStringRecord>();
                string[] lines = ReadAllLinesShared(fullPath);

                for (int index = 0; index < lines.Length; ++index)
                {
                    string line = lines[index];
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    int tabIndex = line.IndexOf('\t');
                    if (tabIndex <= 0)
                        continue;

                    uint entryId;
                    if (!TryParseUInt(line.Substring(0, tabIndex), out entryId))
                        continue;

                    string rawValue = line.Substring(tabIndex + 1);
                    rows.Add(new IndexedStringRecord
                    {
                        SourceFamily = sourceFamily,
                        TableName = tableName,
                        SourcePath = fullPath,
                        RowKey = "line=" + (index + 1).ToString(CultureInfo.InvariantCulture) + ";EntryId=" + entryId.ToString(CultureInfo.InvariantCulture),
                        LineNumber = index + 1,
                        RawRow = line,
                        StringSet = stringSet,
                        Locale = locale,
                        EntryId = entryId,
                        Value = rawValue,
                        NormalizedValue = NormalizeClientText(rawValue)
                    });
                }

                return rows;
            });
        }

        private List<PregameCharacterRecord> LoadPregameCharacters(string relativePath)
        {
            return LoadFile(relativePath, "client_xml", "pregame_chars.xml", fullPath =>
            {
                List<PregameCharacterRecord> rows = new List<PregameCharacterRecord>();
                XDocument document = LoadXmlDocumentShared(fullPath);
                int sequence = 0;

                foreach (XElement sceneElement in document.Descendants("scene"))
                {
                    string sceneType = GetAttributeValue(sceneElement, "type");
                    foreach (XElement charElement in sceneElement.Elements("char"))
                    {
                        ++sequence;

                        PregameCharacterRecord record = new PregameCharacterRecord
                        {
                            SourceFamily = "client_xml",
                            TableName = "pregame_chars.xml",
                            SourcePath = fullPath,
                            RowKey = "scene=" + sceneType + ";char=" + sequence.ToString(CultureInfo.InvariantCulture),
                            LineNumber = GetLineNumber(charElement),
                            RawRow = charElement.ToString(SaveOptions.DisableFormatting),
                            Sequence = sequence,
                            SceneType = sceneType,
                            CareerName = GetAttributeValue(charElement, "career"),
                            RaceName = GetAttributeValue(charElement, "race")
                        };

                        foreach (XElement abilityElement in charElement.Elements("ability"))
                        {
                            int? abilityId = ParseNullableInt(GetAttributeValue(abilityElement, "id"));
                            string type = GetAttributeValue(abilityElement, "type");

                            if (string.Equals(type, "on load", StringComparison.OrdinalIgnoreCase))
                                record.OnLoadAbilityId = abilityId;
                            else if (string.Equals(type, "mouse over", StringComparison.OrdinalIgnoreCase))
                                record.MouseOverAbilityId = abilityId;
                            else if (string.Equals(type, "on click", StringComparison.OrdinalIgnoreCase))
                                record.OnClickAbilityId = abilityId;
                        }

                        rows.Add(record);
                    }
                }

                return rows;
            });
        }

        private void ApplyComponentDescriptions()
        {
            Dictionary<uint, IndexedStringRecord> descriptionsById = ComponentEffects
                .GroupBy(row => row.EntryId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.LineNumber).First());

            foreach (BinaryComponentRecord record in BinaryComponents)
            {
                IndexedStringRecord descriptionRow;
                if (descriptionsById.TryGetValue(record.ComponentId, out descriptionRow))
                    record.Description = descriptionRow.NormalizedValue;
            }
        }

        private List<T> LoadFile<T>(string relativePath, string sourceFamily, string tableName, Func<string, List<T>> loader)
        {
            string fullPath = ResolvePath(relativePath);

            try
            {
                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("Source file not found.", fullPath);

                List<T> rows = loader(fullPath) ?? new List<T>();
                TableStatuses.Add(new TableLoadStatus
                {
                    SourceFamily = sourceFamily,
                    TableName = tableName,
                    SourcePath = fullPath,
                    Loaded = true,
                    RowCount = rows.Count,
                    ErrorMessage = null
                });

                return rows;
            }
            catch (Exception exception)
            {
                TableStatuses.Add(new TableLoadStatus
                {
                    SourceFamily = sourceFamily,
                    TableName = tableName,
                    SourcePath = fullPath,
                    Loaded = false,
                    RowCount = 0,
                    ErrorMessage = exception.Message
                });

                return new List<T>();
            }
        }

        private string ResolvePath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(RootPath, relativePath));
        }

        private static List<string> SplitCsvLine(string line)
        {
            List<string> columns = new List<string>();
            if (line == null)
            {
                columns.Add(string.Empty);
                return columns;
            }

            StringBuilder current = new StringBuilder(line.Length);
            bool inQuotes = false;

            for (int index = 0; index < line.Length; ++index)
            {
                char currentChar = line[index];

                if (currentChar == '"')
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

                if (currentChar == ',' && !inQuotes)
                {
                    columns.Add(current.ToString());
                    current.Clear();
                    continue;
                }

                current.Append(currentChar);
            }

            columns.Add(current.ToString());
            return columns;
        }

        private static string GetColumn(IList<string> columns, int index)
        {
            return index >= 0 && index < columns.Count ? columns[index] : string.Empty;
        }

        private static int ParseInt(string rawValue)
        {
            int parsedValue;
            return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue) ? parsedValue : 0;
        }

        private static int? ParseNullableInt(string rawValue)
        {
            int parsedValue;
            return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue) ? (int?)parsedValue : null;
        }

        private static bool TryParseUInt(string rawValue, out uint value)
        {
            return uint.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        private static string NormalizeClientText(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            string normalized = ClientFormattingRegex.Replace(rawValue, string.Empty);
            return normalized.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
        }

        private static string GetAttributeValue(XElement element, string attributeName)
        {
            XAttribute attribute = element == null ? null : element.Attribute(attributeName);
            return attribute == null ? string.Empty : attribute.Value;
        }

        private static int GetLineNumber(XElement element)
        {
            IXmlLineInfo lineInfo = element as IXmlLineInfo;
            return lineInfo != null && lineInfo.HasLineInfo() ? lineInfo.LineNumber : 0;
        }

        private static string[] ReadAllLinesShared(string path)
        {
            string text = ReadAllTextShared(path, Encoding.UTF8);
            return text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        private static string ReadAllTextShared(string path, Encoding defaultEncoding)
        {
            Exception lastException = null;
            for (int attempt = 0; attempt < 5; ++attempt)
            {
                try
                {
                    using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                    using (StreamReader reader = new StreamReader(stream, defaultEncoding, true))
                        return reader.ReadToEnd();
                }
                catch (IOException exception)
                {
                    lastException = exception;
                    System.Threading.Thread.Sleep(150);
                }
            }

            throw lastException ?? new IOException("Unable to read file: " + path);
        }

        private static XDocument LoadXmlDocumentShared(string path)
        {
            string rawXml = ReadAllTextShared(path, Encoding.GetEncoding(1252));
            string sanitizedXml = SanitizeXml(rawXml);
            return XDocument.Parse(sanitizedXml, LoadOptions.SetLineInfo);
        }

        private static string SanitizeXml(string rawXml)
        {
            StringBuilder builder = new StringBuilder(rawXml.Length);
            foreach (char currentChar in rawXml)
            {
                if (XmlConvert.IsXmlChar(currentChar))
                    builder.Append(currentChar);
                else
                    builder.Append(' ');
            }

            return builder.ToString();
        }
    }
}

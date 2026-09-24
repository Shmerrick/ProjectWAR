using ClientDataMatrix.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClientDataMatrix.Services
{
    public sealed class ControlLiteralCrosswalkCatalog
    {
        public const int DefaultLiteralCount = 24;
        private const int SampleIdLimit = 12;
        private const int CompanionLimit = 5;
        private static readonly Regex SampleAbilityIdRegex = new Regex(@"\b\d{1,5}\b", RegexOptions.Compiled);
        private static readonly Dictionary<string, LiteralInterpretation> KnownLiteralInterpretations = new Dictionary<string, LiteralInterpretation>(StringComparer.OrdinalIgnoreCase)
        {
            { "445", new LiteralInterpretation("Hatred threshold selector family", "High-confidence from `Exile`, `None Shall Pass`, `Hatred`, and Black Guard requirement ladders. `Val7` behaves like bracket edges or floor thresholds, and paired `3/5` plus `3/4` rows create ranges like `0-29`, `30-59`, `60-89`, and `90-100`. Do not use effect-id `445` (`Siege Wrecker`) as semantic evidence for this literal; that is a cross-domain numeric collision.") },
            { "708", new LiteralInterpretation("Movement-control profile with strongest knockback usage", "Anchored by generic `Knockback`, `Triumphant Blasting`, `Exile`, `Snare Net`, and `Boss Immunities`. Safe reading: reused movement-control/displacement family spanning `KNOCKBACK`, `VELOCITY`, and `CC`; exact retail enum name is still unresolved.") },
            { "1014", new LiteralInterpretation("Knockback-side control/status literal", "Observed beside `IMMUNITY Value[0]=24/46` and adjacent to known knockback-side immunity families. Strong domain read; exact client label is still unresolved.") },
            { "1015", new LiteralInterpretation("Knockdown control/status literal", "Recovered from toolkit enum-backed immunity notes and reinforced by nearby requirement and immunity families.") },
            { "1016", new LiteralInterpretation("Root-side control/status literal", "High-confidence from `Immovable`, breakable-root CC families, paired APPLY_ABILITY control overlays, and requirement `9093`.") },
            { "1019", new LiteralInterpretation("Snare control/status literal", "Recovered from toolkit enum-backed immunity notes and consistent with snare-linked control families.") },
            { "1030", new LiteralInterpretation("Stun control/status literal", "Recovered from toolkit enum-backed immunity notes and reused throughout the `Unstoppable` immunity bundle.") },
            { "1119", new LiteralInterpretation("Hatred tier selector bit A", "Observed only inside Black Guard Hatred-scaled requirement/component ladders. `1119` also appears alone as the two-state gate in `Driven By Hate`, `Furious Howl`, `Shield of Rage`, `Choking Fury`, and `Bolstering Anger`. Combined with `1120` as a four-state selector (`00`, `10`, `01`, `11`). Do not confuse the literal with unrelated component-id `1119` rows.") },
            { "1120", new LiteralInterpretation("Hatred tier selector bit B", "Observed only inside Black Guard Hatred-scaled requirement/component ladders. It appears only when the skill needs the full four-state selector shared by `Blade of Ruin` and `Monstrous Rending`. Exact bit ordering is still provisional. Do not confuse the literal with unrelated component-id `1120` string rows.") }
        };

        private readonly AbilityDataset _dataset;
        private readonly Dictionary<uint, List<BinaryComponentRecord>> _componentsByOperationId;
        private readonly Dictionary<ushort, List<AbilityComponentUsage>> _abilityUsagesByComponentId;

        public ControlLiteralCrosswalkCatalog(AbilityDataset dataset)
        {
            _dataset = dataset;
            _componentsByOperationId = dataset == null
                ? new Dictionary<uint, List<BinaryComponentRecord>>()
                : dataset.BinaryComponents
                    .GroupBy(row => row.Operation)
                    .ToDictionary(group => group.Key, group => group.OrderBy(row => row.ComponentId).ThenBy(row => row.RecordIndex).ToList());
            _abilityUsagesByComponentId = BuildAbilityUsagesByComponentId(dataset);
        }

        public ControlLiteralCrosswalkDocument BuildDocument(string extractedRootPath, RequirementLedgerDocument requirements, int topCount, string searchText)
        {
            Dictionary<ushort, RequirementLedgerRecord> requirementsById = requirements == null || requirements.Requirements == null
                ? new Dictionary<ushort, RequirementLedgerRecord>()
                : requirements.Requirements.ToDictionary(row => row.RequirementId);
            Dictionary<string, LiteralAccumulator> literals = new Dictionary<string, LiteralAccumulator>(StringComparer.OrdinalIgnoreCase);

            foreach (LiteralSourceDefinition source in BuildOperationSources())
                AddOperationSource(source, literals);

            AddRequirementSource(requirementsById, literals);

            List<ControlLiteralRecord> rows = literals.Values
                .Select(row => row.ToRecord())
                .Where(row => !string.IsNullOrWhiteSpace(row.RawValue))
                .OrderByDescending(row => row.PriorityScore)
                .ThenByDescending(row => row.DistinctSourceCount)
                .ThenByDescending(row => row.ObservationCount)
                .ThenBy(row => ParseSortBucket(row.RawValue))
                .ThenBy(row => row.RawValue, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!string.IsNullOrWhiteSpace(searchText))
                rows = rows.Where(row => MatchesSearch(row, searchText)).ToList();

            if (topCount > 0)
                rows = rows.Take(topCount).ToList();

            for (int index = 0; index < rows.Count; ++index)
                rows[index].GlobalRank = index + 1;

            return new ControlLiteralCrosswalkDocument
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                ExtractedRootPath = extractedRootPath,
                FilterDescription = BuildFilterDescription(topCount, searchText),
                Literals = rows
            };
        }

        private void AddOperationSource(LiteralSourceDefinition source, Dictionary<string, LiteralAccumulator> literals)
        {
            List<BinaryComponentRecord> components;
            if (!_componentsByOperationId.TryGetValue(source.OperationId, out components))
                return;

            foreach (BinaryComponentRecord component in components)
            {
                foreach (LiteralObservation observation in ExtractComponentObservations(component, source))
                {
                    if (!ShouldIncludeRawValue(observation.RawValue))
                        continue;

                    LiteralAccumulator literal = GetOrCreateLiteral(literals, observation.RawValue);
                    SourceAccumulator accumulator = literal.GetOrCreateSource(source.SourceKey, source.SourceLabel);
                    accumulator.ObservationCount += 1;
                    accumulator.ComponentIds.Add(component.ComponentId);
                    accumulator.AddCompanions(observation.CompanionTokens);

                    List<AbilityComponentUsage> usages;
                    if (!_abilityUsagesByComponentId.TryGetValue(component.ComponentId, out usages))
                        continue;

                    foreach (AbilityComponentUsage usage in usages)
                    {
                        accumulator.AbilityIds.Add(usage.AbilityId);
                        accumulator.AddContextTags(usage.ContextTags);
                        if (!string.IsNullOrWhiteSpace(usage.TriggerText))
                            accumulator.TriggerTexts.Add(usage.TriggerText);
                    }
                }
            }
        }

        private void AddRequirementSource(Dictionary<ushort, RequirementLedgerRecord> requirementsById, Dictionary<string, LiteralAccumulator> literals)
        {
            if (_dataset == null)
                return;

            foreach (BinaryRequirementRecord requirement in _dataset.BinaryRequirements)
            {
                RequirementLedgerRecord ledger;
                requirementsById.TryGetValue(requirement.RequirementId, out ledger);
                List<ushort> abilityIds = ParseAbilityIds(ledger == null ? string.Empty : ledger.SampleAbilitiesText);
                List<string> contextTags = ParseDelimitedText(ledger == null ? string.Empty : ledger.ContextTagsText);

                foreach (BinaryExtDataRecord extData in requirement.ExtData ?? new List<BinaryExtDataRecord>())
                {
                    string rawValue = extData.Val6.ToString(CultureInfo.InvariantCulture);
                    if (!ShouldIncludeRawValue(rawValue))
                        continue;

                    LiteralAccumulator literal = GetOrCreateLiteral(literals, rawValue);
                    SourceAccumulator accumulator = literal.GetOrCreateSource("Requirement:ExtData[*].Val6", "Requirements ExtData[*].Val6");
                    accumulator.ObservationCount += 1;
                    accumulator.RequirementIds.Add(requirement.RequirementId);
                    accumulator.AddAbilityIds(abilityIds);
                    accumulator.AddContextTags(contextTags);
                    accumulator.AddCompanions(BuildRequirementCompanions(extData));
                }
            }
        }

        private IEnumerable<LiteralObservation> ExtractComponentObservations(BinaryComponentRecord component, LiteralSourceDefinition source)
        {
            if (component == null || source == null)
                yield break;

            if (source.Kind == LiteralSourceKind.FlagsRaw)
            {
                if (component.FlagsRaw != 0U)
                {
                    yield return new LiteralObservation
                    {
                        RawValue = component.FlagsRaw.ToString(CultureInfo.InvariantCulture),
                        CompanionTokens = BuildRowCompanions(component, null, null, null, false)
                    };
                }

                yield break;
            }

            if (source.Kind == LiteralSourceKind.ValueIndex)
            {
                if (component.Values != null && source.ValueIndex >= 0 && source.ValueIndex < component.Values.Count && component.Values[source.ValueIndex] != 0)
                {
                    yield return new LiteralObservation
                    {
                        RawValue = component.Values[source.ValueIndex].ToString(CultureInfo.InvariantCulture),
                        CompanionTokens = BuildRowCompanions(component, source.ValueIndex, null, null)
                    };
                }

                yield break;
            }

            foreach (BinaryExtDataRecord extData in component.ExtData ?? new List<BinaryExtDataRecord>())
            {
                int value = GetExtDataValue(extData, source.ExtDataFieldName);
                if (value == 0)
                    continue;

                yield return new LiteralObservation
                {
                    RawValue = value.ToString(CultureInfo.InvariantCulture),
                    CompanionTokens = BuildRowCompanions(component, null, extData, source.ExtDataFieldName)
                };
            }
        }

        private static LiteralAccumulator GetOrCreateLiteral(Dictionary<string, LiteralAccumulator> literals, string rawValue)
        {
            LiteralAccumulator literal;
            if (!literals.TryGetValue(rawValue, out literal))
            {
                literal = new LiteralAccumulator(rawValue);
                literals[rawValue] = literal;
            }

            return literal;
        }

        private static List<LiteralSourceDefinition> BuildOperationSources()
        {
            return new List<LiteralSourceDefinition>
            {
                LiteralSourceDefinition.Flags(12U),
                LiteralSourceDefinition.ExtData(12U, "Val6"),
                LiteralSourceDefinition.ExtData(23U, "Val6"),
                LiteralSourceDefinition.Value(24U, 0),
                LiteralSourceDefinition.ExtData(24U, "Val6"),
                LiteralSourceDefinition.Value(38U, 0),
                LiteralSourceDefinition.ExtData(38U, "Val6")
            };
        }

        private static string BuildFilterDescription(int topCount, string searchText)
        {
            string countText = topCount > 0 ? topCount.ToString(CultureInfo.InvariantCulture) + " literal(s)" : "all matching literals";
            return "Focused on repeated numeric literals from CC, APPLY_ABILITY, KNOCKBACK, IMMUNITY, and requirement `ExtData[*].Val6`; returning " + countText + (string.IsNullOrWhiteSpace(searchText) ? "." : "; search filter: `" + searchText + "`.");
        }

        private static bool MatchesSearch(ControlLiteralRecord row, string searchText)
        {
            string haystack = string.Join(" ", new[]
            {
                row.RawValue,
                row.Interpretation,
                row.Notes,
                row.SourceKeysText,
                row.ContextTagsText,
                row.SampleAbilityIdsText,
                row.SampleRequirementIdsText,
                row.Summary,
                row.Sources == null ? string.Empty : string.Join(" ", row.Sources.Select(source => (source.SourceLabel ?? string.Empty) + " " + (source.ContextTagSummaryText ?? string.Empty) + " " + (source.CompanionSummaryText ?? string.Empty)))
            });

            return haystack.IndexOf(searchText ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ShouldIncludeRawValue(string rawValue)
        {
            long parsed;
            return long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) && parsed != 0L;
        }

        private static int ParseSortBucket(string rawValue)
        {
            int parsed;
            return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : int.MaxValue;
        }

        private static int GetExtDataValue(BinaryExtDataRecord extData, string fieldName)
        {
            if (extData == null)
                return 0;

            switch (fieldName)
            {
                case "Val1": return extData.Val1;
                case "Val2": return extData.Val2;
                case "Val3": return extData.Val3;
                case "Val4": return extData.Val4;
                case "Val5": return extData.Val5;
                case "Val6": return extData.Val6;
                case "Val7": return extData.Val7;
                case "Val8": return extData.Val8;
                case "Val9": return extData.Val9;
                default: return 0;
            }
        }

        private static List<string> BuildRowCompanions(BinaryComponentRecord component, int? excludedValueIndex, BinaryExtDataRecord focusedExtData, string excludedExtDataFieldName)
        {
            return BuildRowCompanions(component, excludedValueIndex, focusedExtData, excludedExtDataFieldName, true);
        }

        private static List<string> BuildRowCompanions(BinaryComponentRecord component, int? excludedValueIndex, BinaryExtDataRecord focusedExtData, string excludedExtDataFieldName, bool includeFlagsRaw)
        {
            HashSet<string> companions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (component == null)
                return companions.OrderBy(row => row, StringComparer.OrdinalIgnoreCase).ToList();

            if (includeFlagsRaw && component.FlagsRaw != 0U)
                companions.Add("FlagsRaw=" + component.FlagsRaw.ToString(CultureInfo.InvariantCulture));
            if (component.Value15 != 0)
                companions.Add("Value15=" + component.Value15.ToString(CultureInfo.InvariantCulture));

            if (component.Values != null)
            {
                for (int index = 0; index < component.Values.Count; ++index)
                {
                    if (excludedValueIndex.HasValue && excludedValueIndex.Value == index)
                        continue;
                    if (component.Values[index] != 0)
                        companions.Add("Value[" + index.ToString(CultureInfo.InvariantCulture) + "]=" + component.Values[index].ToString(CultureInfo.InvariantCulture));
                }
            }

            if (focusedExtData != null)
                AddExtDataCompanions(companions, focusedExtData, excludedExtDataFieldName);
            else
            {
                foreach (BinaryExtDataRecord extData in component.ExtData ?? new List<BinaryExtDataRecord>())
                    AddExtDataCompanions(companions, extData, null);
            }

            return companions.OrderBy(row => row, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static List<string> BuildRequirementCompanions(BinaryExtDataRecord extData)
        {
            HashSet<string> companions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddExtDataCompanions(companions, extData, "Val6");
            return companions.OrderBy(row => row, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void AddExtDataCompanions(HashSet<string> companions, BinaryExtDataRecord extData, string excludedFieldName)
        {
            if (companions == null || extData == null)
                return;

            AddExtDataCompanion(companions, "Val1", extData.Val1, excludedFieldName);
            AddExtDataCompanion(companions, "Val2", extData.Val2, excludedFieldName);
            AddExtDataCompanion(companions, "Val3", extData.Val3, excludedFieldName);
            AddExtDataCompanion(companions, "Val4", extData.Val4, excludedFieldName);
            AddExtDataCompanion(companions, "Val5", extData.Val5, excludedFieldName);
            AddExtDataCompanion(companions, "Val6", extData.Val6, excludedFieldName);
            AddExtDataCompanion(companions, "Val7", extData.Val7, excludedFieldName);
            AddExtDataCompanion(companions, "Val8", extData.Val8, excludedFieldName);
            if (extData.Val9 != 0 && !string.Equals(excludedFieldName, "Val9", StringComparison.OrdinalIgnoreCase))
                companions.Add("Val9=" + extData.Val9.ToString(CultureInfo.InvariantCulture));
        }

        private static void AddExtDataCompanion(HashSet<string> companions, string fieldName, int value, string excludedFieldName)
        {
            if (value == 0 || string.Equals(fieldName, excludedFieldName, StringComparison.OrdinalIgnoreCase))
                return;
            companions.Add(fieldName + "=" + value.ToString(CultureInfo.InvariantCulture));
        }

        private static Dictionary<ushort, List<AbilityComponentUsage>> BuildAbilityUsagesByComponentId(AbilityDataset dataset)
        {
            Dictionary<ushort, List<AbilityComponentUsage>> usagesByComponentId = new Dictionary<ushort, List<AbilityComponentUsage>>();
            if (dataset == null)
                return usagesByComponentId;

            Dictionary<ushort, string> namesById = BuildAbilityNames(dataset);
            Dictionary<ushort, string> descriptionsById = BuildStringLookup(dataset.AbilityDescriptions);
            Dictionary<ushort, string> effectTextsById = BuildStringLookup(dataset.AbilityEffectTexts);

            foreach (BinaryAbilityRecord ability in dataset.BinaryAbilities)
            {
                List<string> contextTags = BuildContextTags(
                    LookupText(namesById, ability.AbilityId),
                    LookupText(descriptionsById, ability.AbilityId),
                    LookupText(effectTextsById, ability.AbilityId));

                for (int slotIndex = 0; slotIndex < (ability.ComponentIds == null ? 0 : ability.ComponentIds.Count); ++slotIndex)
                {
                    ushort componentId = ability.ComponentIds[slotIndex];
                    if (componentId == 0)
                        continue;

                    List<AbilityComponentUsage> usages;
                    if (!usagesByComponentId.TryGetValue(componentId, out usages))
                    {
                        usages = new List<AbilityComponentUsage>();
                        usagesByComponentId[componentId] = usages;
                    }

                    uint triggerValue = ability.Triggers != null && slotIndex < ability.Triggers.Count ? ability.Triggers[slotIndex] : 0U;
                    usages.Add(new AbilityComponentUsage
                    {
                        AbilityId = ability.AbilityId,
                        TriggerText = triggerValue == 0U ? string.Empty : DefinitionCatalog.DescribeTriggerValue(triggerValue),
                        ContextTags = contextTags
                    });
                }
            }

            return usagesByComponentId;
        }

        // abilitynames.txt only: abilities.csv is keyed by effect id, so a name read from it at an
        // ability id belongs to some effect instead.
        private static Dictionary<ushort, string> BuildAbilityNames(AbilityDataset dataset)
        {
            Dictionary<ushort, string> values = new Dictionary<ushort, string>();
            foreach (IndexedStringRecord row in dataset.AbilityNames.Where(row => row.EntryId <= ushort.MaxValue))
            {
                ushort abilityId = (ushort)row.EntryId;
                if (!values.ContainsKey(abilityId) && !string.IsNullOrWhiteSpace(row.NormalizedValue))
                    values[abilityId] = row.NormalizedValue;
            }

            return values;
        }

        private static Dictionary<ushort, string> BuildStringLookup(IList<IndexedStringRecord> rows)
        {
            Dictionary<ushort, string> values = new Dictionary<ushort, string>();
            foreach (IndexedStringRecord row in rows ?? new List<IndexedStringRecord>())
            {
                if (row.EntryId > ushort.MaxValue || string.IsNullOrWhiteSpace(row.NormalizedValue))
                    continue;

                ushort abilityId = (ushort)row.EntryId;
                if (!values.ContainsKey(abilityId))
                    values[abilityId] = row.NormalizedValue;
            }

            return values;
        }

        private static string LookupText(Dictionary<ushort, string> valuesById, ushort abilityId)
        {
            string value;
            return valuesById.TryGetValue(abilityId, out value) ? value : string.Empty;
        }

        private static List<string> BuildContextTags(params string[] values)
        {
            List<string> tags = new List<string>();
            string text = string.Join(" ", values ?? new string[0]).ToLowerInvariant();
            AddTagIfPresent(tags, text, "knockback", "Knockback");
            AddTagIfPresent(tags, text, "knock down", "Knockdown");
            AddTagIfPresent(tags, text, "knockdown", "Knockdown");
            AddTagIfPresent(tags, text, "crowd control", "CrowdControl");
            AddTagIfPresent(tags, text, "immunity", "Immunity");
            AddTagIfPresent(tags, text, "immune", "Immunity");
            AddTagIfPresent(tags, text, "stagger", "Stagger");
            AddTagIfPresent(tags, text, "stun", "Stun");
            AddTagIfPresent(tags, text, "root", "Root");
            AddTagIfPresent(tags, text, "snare", "Snare");
            AddTagIfPresent(tags, text, "silence", "Silence");
            AddTagIfPresent(tags, text, "disarm", "Disarm");
            AddTagIfPresent(tags, text, "heal", "Heal");
            AddTagIfPresent(tags, text, "damage", "Damage");
            return tags;
        }

        private static void AddTagIfPresent(List<string> tags, string text, string needle, string tag)
        {
            if (text.Contains(needle) && !tags.Contains(tag))
                tags.Add(tag);
        }

        private static List<ushort> ParseAbilityIds(string text)
        {
            List<ushort> ids = new List<ushort>();
            foreach (Match match in SampleAbilityIdRegex.Matches(text ?? string.Empty))
            {
                ushort abilityId;
                if (ushort.TryParse(match.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out abilityId) && !ids.Contains(abilityId))
                    ids.Add(abilityId);
            }

            return ids.OrderBy(row => row).ToList();
        }

        private static List<string> ParseDelimitedText(string text)
        {
            return (text ?? string.Empty)
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(row => row.Trim())
                .Where(row => row.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(row => row, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private sealed class AbilityComponentUsage
        {
            public ushort AbilityId { get; set; }
            public string TriggerText { get; set; }
            public List<string> ContextTags { get; set; }
        }

        private sealed class LiteralObservation
        {
            public string RawValue { get; set; }
            public List<string> CompanionTokens { get; set; }
        }

        private sealed class LiteralSourceDefinition
        {
            public string SourceKey { get; set; }
            public string SourceLabel { get; set; }
            public LiteralSourceKind Kind { get; set; }
            public uint OperationId { get; set; }
            public int ValueIndex { get; set; }
            public string ExtDataFieldName { get; set; }

            public static LiteralSourceDefinition Flags(uint operationId)
            {
                string operationName = DefinitionCatalog.DescribeComponentOperationValue(operationId);
                return new LiteralSourceDefinition { SourceKey = "Operation:" + operationId.ToString(CultureInfo.InvariantCulture) + ":FlagsRaw", SourceLabel = operationName + " FlagsRaw", Kind = LiteralSourceKind.FlagsRaw, OperationId = operationId };
            }

            public static LiteralSourceDefinition Value(uint operationId, int valueIndex)
            {
                string operationName = DefinitionCatalog.DescribeComponentOperationValue(operationId);
                return new LiteralSourceDefinition { SourceKey = "Operation:" + operationId.ToString(CultureInfo.InvariantCulture) + ":Value[" + valueIndex.ToString(CultureInfo.InvariantCulture) + "]", SourceLabel = operationName + " Value[" + valueIndex.ToString(CultureInfo.InvariantCulture) + "]", Kind = LiteralSourceKind.ValueIndex, OperationId = operationId, ValueIndex = valueIndex };
            }

            public static LiteralSourceDefinition ExtData(uint operationId, string fieldName)
            {
                string operationName = DefinitionCatalog.DescribeComponentOperationValue(operationId);
                return new LiteralSourceDefinition { SourceKey = "Operation:" + operationId.ToString(CultureInfo.InvariantCulture) + ":ExtData[*]." + fieldName, SourceLabel = operationName + " ExtData[*]." + fieldName, Kind = LiteralSourceKind.ExtDataValue, OperationId = operationId, ExtDataFieldName = fieldName };
            }
        }

        private enum LiteralSourceKind
        {
            FlagsRaw,
            ValueIndex,
            ExtDataValue
        }

        private sealed class LiteralAccumulator
        {
            private readonly Dictionary<string, SourceAccumulator> _sources = new Dictionary<string, SourceAccumulator>(StringComparer.OrdinalIgnoreCase);

            public LiteralAccumulator(string rawValue)
            {
                RawValue = rawValue;
            }

            public string RawValue { get; private set; }

            public SourceAccumulator GetOrCreateSource(string sourceKey, string sourceLabel)
            {
                SourceAccumulator source;
                if (!_sources.TryGetValue(sourceKey, out source))
                {
                    source = new SourceAccumulator(sourceKey, sourceLabel);
                    _sources[sourceKey] = source;
                }

                return source;
            }

            public ControlLiteralRecord ToRecord()
            {
                List<ControlLiteralSourceRecord> sources = _sources.Values.Select(row => row.ToRecord()).OrderByDescending(row => row.ObservationCount).ThenBy(row => row.SourceLabel, StringComparer.OrdinalIgnoreCase).ToList();
                HashSet<ushort> componentIds = new HashSet<ushort>();
                HashSet<ushort> abilityIds = new HashSet<ushort>();
                HashSet<ushort> requirementIds = new HashSet<ushort>();
                HashSet<string> contextTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (SourceAccumulator source in _sources.Values)
                {
                    componentIds.UnionWith(source.ComponentIds);
                    abilityIds.UnionWith(source.AbilityIds);
                    requirementIds.UnionWith(source.RequirementIds);
                    contextTags.UnionWith(source.ContextTags);
                }

                int observationCount = _sources.Values.Sum(row => row.ObservationCount);
                string sourceKeysText = string.Join(", ", sources.Select(row => row.SourceLabel));
                LiteralInterpretation interpretation;
                KnownLiteralInterpretations.TryGetValue(RawValue, out interpretation);
                return new ControlLiteralRecord
                {
                    PriorityScore = (_sources.Count * 100000) + (requirementIds.Count * 1000) + (abilityIds.Count * 25) + observationCount,
                    RawValue = RawValue,
                    ObservationCount = observationCount,
                    DistinctSourceCount = _sources.Count,
                    DistinctComponentCount = componentIds.Count,
                    DistinctAbilityCount = abilityIds.Count,
                    DistinctRequirementCount = requirementIds.Count,
                    SourceKeysText = sourceKeysText,
                    ContextTagsText = string.Join(", ", contextTags.OrderBy(row => row, StringComparer.OrdinalIgnoreCase)),
                    SampleAbilityIdsText = JoinIds(abilityIds),
                    SampleRequirementIdsText = JoinIds(requirementIds),
                    Interpretation = interpretation == null ? string.Empty : interpretation.Summary,
                    Notes = interpretation == null ? string.Empty : interpretation.Notes,
                    Summary = "Observed " + observationCount.ToString(CultureInfo.InvariantCulture) + " time(s) across " + _sources.Count.ToString(CultureInfo.InvariantCulture) + " source group(s): " + sourceKeysText,
                    Sources = sources
                };
            }
        }

        private sealed class SourceAccumulator
        {
            private readonly Dictionary<string, int> _companionCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            public SourceAccumulator(string sourceKey, string sourceLabel)
            {
                SourceKey = sourceKey;
                SourceLabel = sourceLabel;
                ComponentIds = new HashSet<ushort>();
                AbilityIds = new HashSet<ushort>();
                RequirementIds = new HashSet<ushort>();
                ContextTags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                TriggerTexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            public string SourceKey { get; private set; }
            public string SourceLabel { get; private set; }
            public int ObservationCount { get; set; }
            public HashSet<ushort> ComponentIds { get; private set; }
            public HashSet<ushort> AbilityIds { get; private set; }
            public HashSet<ushort> RequirementIds { get; private set; }
            public HashSet<string> ContextTags { get; private set; }
            public HashSet<string> TriggerTexts { get; private set; }

            public void AddCompanions(IEnumerable<string> companions)
            {
                foreach (string companion in companions ?? new List<string>())
                {
                    if (string.IsNullOrWhiteSpace(companion))
                        continue;

                    int count;
                    _companionCounts[companion] = _companionCounts.TryGetValue(companion, out count) ? count + 1 : 1;
                }
            }

            public void AddContextTags(IEnumerable<string> tags)
            {
                foreach (string tag in tags ?? new List<string>())
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                        ContextTags.Add(tag);
                }
            }

            public void AddAbilityIds(IEnumerable<ushort> abilityIds)
            {
                foreach (ushort abilityId in abilityIds ?? new List<ushort>())
                    AbilityIds.Add(abilityId);
            }

            public ControlLiteralSourceRecord ToRecord()
            {
                return new ControlLiteralSourceRecord
                {
                    SourceKey = SourceKey,
                    SourceLabel = SourceLabel,
                    ObservationCount = ObservationCount,
                    DistinctComponentCount = ComponentIds.Count,
                    DistinctAbilityCount = AbilityIds.Count,
                    DistinctRequirementCount = RequirementIds.Count,
                    SampleComponentIdsText = JoinIds(ComponentIds),
                    SampleAbilityIdsText = JoinIds(AbilityIds),
                    SampleRequirementIdsText = JoinIds(RequirementIds),
                    TriggerSummaryText = string.Join(", ", TriggerTexts.OrderBy(row => row, StringComparer.OrdinalIgnoreCase).Take(SampleIdLimit)),
                    ContextTagSummaryText = string.Join(", ", ContextTags.OrderBy(row => row, StringComparer.OrdinalIgnoreCase)),
                    CompanionSummaryText = string.Join(", ", _companionCounts.OrderByDescending(row => row.Value).ThenBy(row => row.Key, StringComparer.OrdinalIgnoreCase).Take(CompanionLimit).Select(row => row.Key + " (" + row.Value.ToString(CultureInfo.InvariantCulture) + ")"))
                };
            }
        }

        private static string JoinIds(IEnumerable<ushort> ids)
        {
            return string.Join(", ", (ids ?? new List<ushort>()).Distinct().OrderBy(row => row).Take(SampleIdLimit).Select(row => row.ToString(CultureInfo.InvariantCulture)));
        }

        private sealed class LiteralInterpretation
        {
            public LiteralInterpretation(string summary, string notes)
            {
                Summary = summary ?? string.Empty;
                Notes = notes ?? string.Empty;
            }

            public string Summary { get; private set; }
            public string Notes { get; private set; }
        }
    }
}

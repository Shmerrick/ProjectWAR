using ClientDataMatrix.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ClientDataMatrix.Services
{
    public static class SemanticConfidence
    {
        public const string Confirmed = "Confirmed";
        public const string Inferred = "Inferred";
        public const string Structural = "Structural";
        public const string Unknown = "Unknown";
        public const string Londo = "Londo";
    }

    public sealed class ComponentSchemaCatalog
    {
        private static readonly Regex ComponentTokenRegex = new Regex(@"COM_(\d+)_([A-Z0-9_]+)", RegexOptions.Compiled);
        private static readonly HashSet<uint> PriorityOperations = new HashSet<uint> { 12U, 23U, 24U, 38U };
        private const string OverrideRelativePath = @"ClientDataMatrix\Configuration\component-schema-overrides.tsv";

        private readonly AbilityDataset _dataset;
        private readonly Dictionary<ushort, BinaryComponentRecord> _componentsById;
        private readonly Dictionary<ushort, BinaryRequirementRecord> _requirementsById;
        private readonly HashSet<ushort> _knownRequirementIds;
        private readonly Dictionary<ushort, string> _abilityNamesById;
        private readonly RequirementCatalog _requirements;
        private readonly Dictionary<string, List<ComponentTokenEvidence>> _evidenceByDomainKey;
        private readonly Dictionary<string, List<ComponentTokenEvidence>> _evidenceByFieldKey;
        private readonly Dictionary<string, List<ComponentTokenEvidence>> _evidenceByAbilityComponentField;
        private readonly Dictionary<string, ComponentSchemaOverride> _overridesByDomainKey;

        public ComponentSchemaCatalog(AbilityDataset dataset)
        {
            _dataset = dataset;
            if (dataset == null)
            {
                _componentsById = new Dictionary<ushort, BinaryComponentRecord>();
                _requirementsById = new Dictionary<ushort, BinaryRequirementRecord>();
                _knownRequirementIds = new HashSet<ushort>();
                _abilityNamesById = new Dictionary<ushort, string>();
                _requirements = new RequirementCatalog(null);
                _evidenceByDomainKey = new Dictionary<string, List<ComponentTokenEvidence>>(StringComparer.OrdinalIgnoreCase);
                _evidenceByFieldKey = new Dictionary<string, List<ComponentTokenEvidence>>(StringComparer.OrdinalIgnoreCase);
                _evidenceByAbilityComponentField = new Dictionary<string, List<ComponentTokenEvidence>>(StringComparer.OrdinalIgnoreCase);
                _overridesByDomainKey = new Dictionary<string, ComponentSchemaOverride>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            _componentsById = dataset.BinaryComponents
                .GroupBy(row => row.ComponentId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.RecordIndex).First());
            _requirementsById = dataset.BinaryRequirements
                .GroupBy(row => row.RequirementId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.RecordIndex).First());
            _knownRequirementIds = new HashSet<ushort>(_requirementsById.Keys);
            _abilityNamesById = BuildAbilityNames(dataset);
            _requirements = new RequirementCatalog(dataset);
            _evidenceByDomainKey = new Dictionary<string, List<ComponentTokenEvidence>>(StringComparer.OrdinalIgnoreCase);
            _evidenceByFieldKey = new Dictionary<string, List<ComponentTokenEvidence>>(StringComparer.OrdinalIgnoreCase);
            _evidenceByAbilityComponentField = new Dictionary<string, List<ComponentTokenEvidence>>(StringComparer.OrdinalIgnoreCase);
            _overridesByDomainKey = LoadOverrides();

            BuildClientTokenEvidence(dataset);
        }

        public static string BuildOperationFieldDomainKey(uint operation, string fieldKey)
        {
            return "Component.Operation." + operation.ToString(CultureInfo.InvariantCulture) + "." + fieldKey;
        }

        public ComponentFieldSemantic ResolveFieldSemantic(AbilityAnalysisResult report, BinaryComponentRecord componentRow, string fieldKey, string rawValue)
        {
            if (componentRow == null || string.IsNullOrWhiteSpace(fieldKey))
                return CreateUnknown("No component field context is available.");

            string domainKey = BuildOperationFieldDomainKey(componentRow.Operation, fieldKey);
            string abilitySpecificKey = report == null
                ? string.Empty
                : BuildAbilityComponentFieldKey(report.AbilityId, componentRow.ComponentId, fieldKey);

            List<ComponentTokenEvidence> directEvidence;
            if (!string.IsNullOrWhiteSpace(abilitySpecificKey) && _evidenceByAbilityComponentField.TryGetValue(abilitySpecificKey, out directEvidence) && directEvidence.Count > 0)
            {
                return new ComponentFieldSemantic
                {
                    DomainKey = domainKey,
                    Meaning = BuildEvidenceMeaning(directEvidence),
                    Confidence = SemanticConfidence.Confirmed,
                    Source = BuildSourceLabel(directEvidence),
                    SourcePath = directEvidence[0].SourcePath,
                    SourceLocation = directEvidence[0].SourceLocation,
                    Notes = BuildEvidenceNotes(directEvidence)
                };
            }

            List<ComponentTokenEvidence> domainEvidence;
            if (_evidenceByDomainKey.TryGetValue(domainKey, out domainEvidence) && domainEvidence.Count > 0)
            {
                return new ComponentFieldSemantic
                {
                    DomainKey = domainKey,
                    Meaning = "Operation-level client text evidence: " + BuildEvidenceMeaning(domainEvidence),
                    Confidence = SemanticConfidence.Inferred,
                    Source = "Aggregated client strings",
                    SourcePath = domainEvidence[0].SourcePath,
                    SourceLocation = domainEvidence[0].SourceLocation,
                    Notes = BuildEvidenceNotes(domainEvidence)
                };
            }

            ComponentSchemaOverride schemaOverride;
            if (_overridesByDomainKey.TryGetValue(domainKey, out schemaOverride))
            {
                return new ComponentFieldSemantic
                {
                    DomainKey = domainKey,
                    Meaning = schemaOverride.Meaning,
                    Confidence = string.IsNullOrWhiteSpace(schemaOverride.Confidence) ? SemanticConfidence.Londo : schemaOverride.Confidence,
                    Source = schemaOverride.SourceLabel,
                    SourcePath = schemaOverride.SourcePath,
                    SourceLocation = schemaOverride.SourceLocation,
                    Notes = schemaOverride.Notes
                };
            }

            ComponentFieldSemantic requirementSemantic;
            if (TryResolveRequirementFieldSemantic(fieldKey, rawValue, out requirementSemantic))
                return requirementSemantic;

            ComponentFieldSemantic extDataVal1Semantic;
            if (TryResolveExtDataVal1Semantic(fieldKey, rawValue, out extDataVal1Semantic))
                return extDataVal1Semantic;

            ComponentFieldSemantic extDataVal2Semantic;
            if (TryResolveExtDataVal2Semantic(componentRow.Operation, fieldKey, rawValue, out extDataVal2Semantic))
                return extDataVal2Semantic;

            ComponentFieldSemantic extDataVal3Semantic;
            if (TryResolveExtDataVal3Semantic(fieldKey, rawValue, out extDataVal3Semantic))
                return extDataVal3Semantic;

            ComponentFieldSemantic extDataVal4Semantic;
            if (TryResolveExtDataVal4Semantic(fieldKey, rawValue, out extDataVal4Semantic))
                return extDataVal4Semantic;

            ComponentFieldSemantic extDataVal7Semantic;
            if (TryResolveExtDataVal7Semantic(fieldKey, rawValue, componentRow, out extDataVal7Semantic))
                return extDataVal7Semantic;

            ComponentFieldSemantic extDataVal5Semantic;
            if (TryResolveExtDataVal5Semantic(fieldKey, rawValue, componentRow, out extDataVal5Semantic))
                return extDataVal5Semantic;

            ComponentFieldSemantic extDataVal6Semantic;
            if (TryResolveExtDataVal6Semantic(fieldKey, rawValue, componentRow, out extDataVal6Semantic))
                return extDataVal6Semantic;

            ComponentFieldSemantic extDataVal8Semantic;
            if (TryResolveExtDataVal8Semantic(fieldKey, rawValue, componentRow, out extDataVal8Semantic))
                return extDataVal8Semantic;

            ComponentFieldSemantic extDataVal9Semantic;
            if (TryResolveExtDataVal9Semantic(fieldKey, rawValue, componentRow, out extDataVal9Semantic))
                return extDataVal9Semantic;

            ComponentFieldSemantic applyAbilityExtDataSemantic;
            if (TryResolveApplyAbilityExtDataKnownFieldSemantic(componentRow.Operation, fieldKey, rawValue, out applyAbilityExtDataSemantic))
                return applyAbilityExtDataSemantic;

            ComponentFieldSemantic structuralSemantic;
            if (TryResolveOperationSpecificStructuralSemantic(componentRow.Operation, fieldKey, rawValue, out structuralSemantic))
                return structuralSemantic;

            ComponentFieldSemantic genericFlagsSemantic;
            if (TryResolveGenericFlagsRawSemantic(componentRow.Operation, fieldKey, rawValue, out genericFlagsSemantic))
                return genericFlagsSemantic;

            ComponentFieldSemantic namedControlSemantic;
            if (TryResolveNamedControlFieldSemantic(componentRow.Operation, fieldKey, rawValue, out namedControlSemantic))
                return namedControlSemantic;

            List<ComponentTokenEvidence> sharedFieldEvidence;
            if (IsSharedFieldFallbackAllowed(fieldKey)
                && _evidenceByFieldKey.TryGetValue(fieldKey, out sharedFieldEvidence)
                && sharedFieldEvidence.Count > 0)
            {
                return new ComponentFieldSemantic
                {
                    DomainKey = domainKey,
                    Meaning = "Shared component-field evidence: " + BuildEvidenceMeaning(sharedFieldEvidence),
                    Confidence = SemanticConfidence.Inferred,
                    Source = "Aggregated client strings from other operations",
                    SourcePath = sharedFieldEvidence[0].SourcePath,
                    SourceLocation = sharedFieldEvidence[0].SourceLocation,
                    Notes = BuildSharedEvidenceNotes(fieldKey, sharedFieldEvidence)
                };
            }

            // Multiplier[N]: near-constant 100 (= unmodified scaling) for most operations.
            {
                int multiplierIndex;
                if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex) && string.Equals(rawValue, "100", StringComparison.OrdinalIgnoreCase))
                {
                    return new ComponentFieldSemantic
                    {
                        DomainKey = domainKey,
                        Meaning = "Percentage scaling multiplier — Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "]=100 = 100% (unmodified baseline).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "Cross-operation multiplier pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] scales the corresponding Value[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] slot. "
                            + "Value=100 = 100% = no scaling change (dominant value across most operations)."
                    };
                }
            }

            return new ComponentFieldSemantic
            {
                DomainKey = domainKey,
                Meaning = "No extracted-client semantic mapping is known yet for this field.",
                Confidence = SemanticConfidence.Unknown,
                Source = "No direct evidence",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "This field is present in the client BIN data, but no direct client-string token or override currently explains its semantics."
            };
        }

        public DefinitionDomain BuildDomain(string domainKey)
        {
            if (string.IsNullOrWhiteSpace(domainKey))
                return null;

            List<DefinitionOption> options = new List<DefinitionOption>();
            bool usesSharedFieldEvidence = false;
            List<ComponentTokenEvidence> evidenceRows;
            if (_evidenceByDomainKey.TryGetValue(domainKey, out evidenceRows))
            {
                foreach (IGrouping<string, ComponentTokenEvidence> group in evidenceRows.GroupBy(row => row.TokenBody, StringComparer.OrdinalIgnoreCase).OrderBy(group => group.Key))
                {
                    ComponentTokenEvidence example = group.OrderBy(row => row.AbilityId).ThenBy(row => row.SourcePath).ThenBy(row => row.SourceLocation).First();
                    options.Add(new DefinitionOption
                    {
                        RawValue = group.Key,
                        Meaning = example.Meaning,
                        Confidence = SemanticConfidence.Confirmed,
                        Source = BuildSourceLabel(group),
                        SourcePath = example.SourcePath,
                        Location = example.SourceLocation,
                        Notes = BuildEvidenceNotes(group)
                    });
                }
            }

            string fieldKey;
            List<ComponentTokenEvidence> sharedFieldEvidenceRows;
            if (options.Count == 0
                && TryExtractFieldKey(domainKey, out fieldKey)
                && IsSharedFieldFallbackAllowed(fieldKey)
                && _evidenceByFieldKey.TryGetValue(fieldKey, out sharedFieldEvidenceRows))
            {
                foreach (IGrouping<string, ComponentTokenEvidence> group in sharedFieldEvidenceRows.GroupBy(row => row.TokenBody, StringComparer.OrdinalIgnoreCase).OrderBy(group => group.Key))
                {
                    ComponentTokenEvidence example = group.OrderBy(row => row.AbilityId).ThenBy(row => row.SourcePath).ThenBy(row => row.SourceLocation).First();
                    options.Add(new DefinitionOption
                    {
                        RawValue = group.Key,
                        Meaning = example.Meaning,
                        Confidence = SemanticConfidence.Inferred,
                        Source = BuildSourceLabel(group),
                        SourcePath = example.SourcePath,
                        Location = example.SourceLocation,
                        Notes = BuildSharedEvidenceNotes(fieldKey, group)
                    });
                }

                usesSharedFieldEvidence = options.Count > 0;
            }

            ComponentSchemaOverride schemaOverride;
            if (_overridesByDomainKey.TryGetValue(domainKey, out schemaOverride))
            {
                options.Add(new DefinitionOption
                {
                    RawValue = "override",
                    Meaning = schemaOverride.Meaning,
                    Confidence = string.IsNullOrWhiteSpace(schemaOverride.Confidence) ? SemanticConfidence.Londo : schemaOverride.Confidence,
                    Source = schemaOverride.SourceLabel,
                    SourcePath = schemaOverride.SourcePath,
                    Location = schemaOverride.SourceLocation,
                    Notes = schemaOverride.Notes
                });
            }

            return new DefinitionDomain
            {
                DomainKey = domainKey,
                DisplayName = BuildDisplayName(domainKey),
                Description = usesSharedFieldEvidence
                    ? "Shared semantic evidence for this component field gathered from extracted client text across other component operations. The rows below are semantic tokens and notes, not a closed numeric enum."
                    : "Operation-specific semantic evidence for a component field. The rows below are semantic tokens and notes, not a closed numeric enum.",
                Notes = options.Count == 0
                    ? "No client-token evidence or manual override exists yet for this field."
                    : usesSharedFieldEvidence
                        ? "No direct operation-specific token evidence exists yet. These rows come from the same field key observed in other component operations and are therefore inferred for this operation."
                        : "Client-token evidence comes from extracted ability text. Londo rows are last-resort toolkit references and should be treated cautiously.",
                IsFiniteDomain = false,
                Options = options
            };
        }

        public TokenDictionaryDocument BuildTokenDictionary(string extractedRootPath)
        {
            List<TokenDefinitionRecord> definitions = _evidenceByDomainKey
                .SelectMany(pair => pair.Value)
                .GroupBy(row => row.TokenBody, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    ComponentTokenEvidence example = group
                        .OrderBy(row => row.AbilityId)
                        .ThenBy(row => row.ComponentSlotIndex)
                        .ThenBy(row => row.ComponentId)
                        .First();

                    return new TokenDefinitionRecord
                    {
                        TokenBody = group.Key,
                        ExampleToken = "COM_" + example.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture) + "_" + group.Key,
                        FieldKey = example.FieldKey,
                        PlainEnglishMeaning = BuildPlainEnglishDefinition(example),
                        Confidence = SemanticConfidence.Confirmed,
                        Source = BuildSourceLabel(group),
                        SourcePath = example.SourcePath,
                        ContextTags = group.SelectMany(row => row.ContextTags ?? new List<string>()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value).ToList(),
                        Notes = BuildGlossaryNotes(group),
                        ExampleAbilityIds = group.Select(row => row.AbilityId).Distinct().OrderBy(value => value).Take(24).ToList(),
                        Evidence = group
                            .OrderBy(row => row.AbilityId)
                            .ThenBy(row => row.ComponentSlotIndex)
                            .ThenBy(row => row.SourcePath)
                            .ThenBy(row => row.SourceLocation)
                            .Select(row => new TokenEvidenceRecord
                            {
                                ExampleToken = "COM_" + row.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture) + "_" + row.TokenBody,
                                AbilityId = row.AbilityId,
                                ComponentSlotIndex = row.ComponentSlotIndex,
                                ComponentId = row.ComponentId,
                                Operation = row.Operation,
                                Source = row.Source,
                                SourcePath = row.SourcePath,
                                SourceLocation = row.SourceLocation,
                                Meaning = BuildPlainEnglishDefinition(row),
                                AbilityName = row.AbilityName,
                                TextExcerpt = row.TextExcerpt,
                                ContextTags = row.ContextTags == null ? new List<string>() : row.ContextTags.ToList()
                            })
                            .ToList()
                    };
                })
                .OrderBy(row => row.TokenBody, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new TokenDictionaryDocument
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                ExtractedRootPath = extractedRootPath,
                TokenGrammar = "COM_<ComponentSlotIndex>_<TokenBody>",
                TokenGrammarMeaning = "The number after COM is the ability-local component slot index. The suffix identifies which field from that component is being formatted into client text.",
                OverrideLedgerPath = ResolveOverridePath(),
                Definitions = definitions
            };
        }

        public OperationSchemaDocument BuildOperationSchemas(string extractedRootPath)
        {
            List<ComponentOperationSchemaRecord> operations = new List<ComponentOperationSchemaRecord>();
            if (_dataset != null)
            {
                Dictionary<ushort, List<AbilityComponentReference>> referencesByComponentId = BuildAbilityReferencesByComponentId();
                IEnumerable<IGrouping<uint, BinaryComponentRecord>> groupedOperations = _dataset.BinaryComponents
                    .GroupBy(row => row.Operation)
                    .OrderBy(group => PriorityOperations.Contains(group.Key) ? 0 : 1)
                    .ThenBy(group => DefinitionCatalog.DescribeComponentOperationValue(group.Key), StringComparer.OrdinalIgnoreCase)
                    .ThenBy(group => group.Key);

                foreach (IGrouping<uint, BinaryComponentRecord> group in groupedOperations)
                {
                    List<BinaryComponentRecord> components = group
                        .OrderBy(row => row.ComponentId)
                        .ThenBy(row => row.RecordIndex)
                        .ToList();
                    List<AbilityComponentReference> abilityReferences = DistinctAbilityReferences(components, referencesByComponentId);
                    List<ComponentOperationFieldRecord> fields = BuildOperationFieldRecords(group.Key, components, abilityReferences);
                    List<string> contextTags = abilityReferences
                        .SelectMany(row => row.ContextTags ?? new List<string>())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    operations.Add(new ComponentOperationSchemaRecord
                    {
                        OperationId = group.Key,
                        OperationName = DefinitionCatalog.DescribeComponentOperationValue(group.Key),
                        IsPriority = PriorityOperations.Contains(group.Key),
                        ComponentCount = components.Select(row => row.ComponentId).Distinct().Count(),
                        AbilityCount = abilityReferences.Select(row => row.AbilityId).Distinct().Count(),
                        LayoutVariantsText = JoinValues(components.Select(row => row.LayoutVariant).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase)),
                        ContextTags = contextTags,
                        SampleComponentIds = components.Select(row => row.ComponentId).Distinct().OrderBy(value => value).Take(24).ToList(),
                        Fields = fields,
                        SampleAbilities = abilityReferences
                            .OrderBy(row => row.AbilityId)
                            .ThenBy(row => row.ComponentSlotIndex)
                            .ThenBy(row => row.ComponentId)
                            .Take(48)
                            .Select(row => new ComponentOperationAbilityRecord
                            {
                                AbilityId = row.AbilityId,
                                AbilityName = row.AbilityName,
                                ComponentId = row.ComponentId,
                                ComponentSlotIndex = row.ComponentSlotIndex,
                                TriggerText = row.TriggerText,
                                ContextTagsText = JoinValues(row.ContextTags ?? new List<string>()),
                                TextExcerpt = row.TextExcerpt,
                                SourcePath = row.SourcePath,
                                SourceLocation = row.SourceLocation
                            })
                            .ToList()
                    });
                }
            }

            return new OperationSchemaDocument
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                ExtractedRootPath = extractedRootPath,
                OverrideLedgerPath = ResolveOverridePath(),
                TableStatuses = _dataset == null
                    ? new List<TableLoadStatus>()
                    : _dataset.TableStatuses.OrderBy(row => row.SourceFamily).ThenBy(row => row.TableName).ToList(),
                PriorityOperationIds = PriorityOperations.OrderBy(value => value).ToList(),
                Operations = operations
            };
        }

        public List<ComponentOperationFieldValueRecord> BuildOperationFieldValueEvidence(uint operationId, string fieldKey)
        {
            if (_dataset == null || string.IsNullOrWhiteSpace(fieldKey))
                return new List<ComponentOperationFieldValueRecord>();

            Dictionary<ushort, List<AbilityComponentReference>> referencesByComponentId = BuildAbilityReferencesByComponentId();
            List<FieldValueObservation> observations = new List<FieldValueObservation>();

            foreach (BinaryComponentRecord component in _dataset.BinaryComponents.Where(row => row.Operation == operationId))
            {
                string rawValue;
                if (!TryReadFieldRawValue(component, fieldKey, out rawValue))
                    continue;

                List<AbilityComponentReference> references;
                if (!referencesByComponentId.TryGetValue(component.ComponentId, out references))
                    references = new List<AbilityComponentReference>();

                observations.Add(new FieldValueObservation
                {
                    RawValue = rawValue,
                    ComponentId = component.ComponentId,
                    AbilityReferences = references
                });
            }

            return observations
                .GroupBy(row => row.RawValue, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    List<AbilityComponentReference> sampleAbilities = group
                        .SelectMany(row => row.AbilityReferences ?? new List<AbilityComponentReference>())
                        .GroupBy(row => row.AbilityId.ToString(CultureInfo.InvariantCulture)
                            + "|"
                            + row.ComponentId.ToString(CultureInfo.InvariantCulture)
                            + "|"
                            + row.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture), StringComparer.OrdinalIgnoreCase)
                        .Select(match => match.First())
                        .OrderBy(row => row.AbilityId)
                        .ThenBy(row => row.ComponentSlotIndex)
                        .ThenBy(row => row.ComponentId)
                        .Take(24)
                        .ToList();

                    return new ComponentOperationFieldValueRecord
                    {
                        RawValue = group.Key,
                        ObservationCount = group.Count(),
                        DistinctComponentCount = group.Select(row => row.ComponentId).Distinct().Count(),
                        DistinctAbilityCount = sampleAbilities.Select(row => row.AbilityId).Distinct().Count(),
                        SampleComponentIdsText = JoinValues(group.Select(row => row.ComponentId.ToString(CultureInfo.InvariantCulture)).Distinct().OrderBy(value => value).Take(12)),
                        SampleAbilityIdsText = JoinValues(sampleAbilities.Select(row => row.AbilityId.ToString(CultureInfo.InvariantCulture)).Distinct().OrderBy(value => value).Take(12)),
                        SampleAbilities = sampleAbilities.Select(row => new ComponentOperationAbilityRecord
                        {
                            AbilityId = row.AbilityId,
                            AbilityName = row.AbilityName,
                            ComponentId = row.ComponentId,
                            ComponentSlotIndex = row.ComponentSlotIndex,
                            TriggerText = row.TriggerText,
                            ContextTagsText = JoinValues(row.ContextTags ?? new List<string>()),
                            TextExcerpt = row.TextExcerpt,
                            SourcePath = row.SourcePath,
                            SourceLocation = row.SourceLocation
                        }).ToList()
                    };
                })
                .OrderByDescending(row => row.ObservationCount)
                .ThenByDescending(row => row.DistinctAbilityCount)
                .ThenBy(row => row.RawValue, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public ComponentOperationFieldValueInsightRecord BuildOperationFieldValueInsight(uint operationId, string fieldKey, string rawValue)
        {
            ComponentOperationFieldValueInsightRecord empty = new ComponentOperationFieldValueInsightRecord
            {
                OperationId = operationId,
                FieldKey = fieldKey,
                RawValue = rawValue,
                TriggerSummaryText = "No sampled trigger evidence is available for this value yet.",
                ContextTagSummaryText = "No sampled context tags are available for this value yet.",
                CompanionSummaryText = "No strong non-zero companion fields were isolated for this value yet.",
                CompanionFields = new List<ComponentOperationFieldCorrelationRecord>()
            };

            if (_dataset == null || string.IsNullOrWhiteSpace(fieldKey) || string.IsNullOrWhiteSpace(rawValue))
                return empty;

            List<BinaryComponentRecord> matchingComponents = _dataset.BinaryComponents
                .Where(row => row.Operation == operationId)
                .Where(row =>
                {
                    string candidateValue;
                    return TryReadFieldRawValue(row, fieldKey, out candidateValue)
                        && string.Equals(candidateValue, rawValue, StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
            if (matchingComponents.Count == 0)
                return empty;

            Dictionary<ushort, List<AbilityComponentReference>> referencesByComponentId = BuildAbilityReferencesByComponentId();
            List<AbilityComponentReference> abilityReferences = DistinctAbilityReferences(matchingComponents, referencesByComponentId);
            Dictionary<string, List<FieldObservation>> observationsByField = BuildFieldObservationsByField(matchingComponents);

            List<ComponentOperationFieldCorrelationRecord> allCompanionFields = observationsByField
                .Where(pair => !string.Equals(pair.Key, fieldKey, StringComparison.OrdinalIgnoreCase))
                .Select(pair => BuildCompanionFieldCorrelation(pair.Key, pair.Value, matchingComponents.Count))
                .Where(row => row != null)
                .OrderByDescending(row => row.CoveragePercent)
                .ThenByDescending(row => row.MatchCount)
                .ThenByDescending(row => row.ObservationCount)
                .ThenBy(row => GetFieldSortKey(row.FieldKey), StringComparer.OrdinalIgnoreCase)
                .ToList();
            List<ComponentOperationFieldCorrelationRecord> nonMultiplierCompanions = allCompanionFields
                .Where(row => !row.FieldKey.StartsWith("Multiplier[", StringComparison.OrdinalIgnoreCase))
                .ToList();
            List<ComponentOperationFieldCorrelationRecord> companionFields = (nonMultiplierCompanions.Count == 0 ? allCompanionFields : nonMultiplierCompanions)
                .Take(12)
                .ToList();

            List<string> highSignalCompanions = companionFields
                .Where(row => row.CoveragePercent >= 75)
                .Take(5)
                .Select(row => row.FieldKey
                    + "="
                    + row.DominantValue
                    + " ("
                    + row.MatchCount.ToString(CultureInfo.InvariantCulture)
                    + "/"
                    + matchingComponents.Count.ToString(CultureInfo.InvariantCulture)
                    + ", "
                    + row.CoveragePercent.ToString(CultureInfo.InvariantCulture)
                    + "%)")
                .ToList();

            empty.ObservationCount = matchingComponents.Count;
            empty.DistinctComponentCount = matchingComponents.Select(row => row.ComponentId).Distinct().Count();
            empty.DistinctAbilityCount = abilityReferences.Select(row => row.AbilityId).Distinct().Count();
            empty.TriggerSummaryText = BuildValueCountSummary(
                abilityReferences.Select(row => row.TriggerText),
                6,
                "No sampled trigger evidence is available for this value yet.");
            empty.ContextTagSummaryText = BuildValueCountSummary(
                abilityReferences.SelectMany(row => row.ContextTags ?? new List<string>()),
                8,
                "No sampled context tags are available for this value yet.");
            empty.CompanionSummaryText = highSignalCompanions.Count == 0
                ? "No strong non-zero companion fields were isolated for this value yet."
                : string.Join("; ", highSignalCompanions);
            empty.CompanionFields = companionFields;
            return empty;
        }

        private void BuildClientTokenEvidence(AbilityDataset dataset)
        {
            Dictionary<ushort, List<IndexedStringRecord>> descriptionsById = dataset.AbilityDescriptions
                .Where(row => row.EntryId <= ushort.MaxValue)
                .GroupBy(row => (ushort)row.EntryId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.LineNumber).ToList());
            Dictionary<ushort, List<IndexedStringRecord>> effectTextsById = dataset.AbilityEffectTexts
                .Where(row => row.EntryId <= ushort.MaxValue)
                .GroupBy(row => (ushort)row.EntryId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.LineNumber).ToList());

            foreach (BinaryAbilityRecord abilityRow in dataset.BinaryAbilities)
            {
                List<IndexedStringRecord> textRows = new List<IndexedStringRecord>();
                List<IndexedStringRecord> descriptionRows;
                if (descriptionsById.TryGetValue(abilityRow.AbilityId, out descriptionRows))
                    textRows.AddRange(descriptionRows);
                List<IndexedStringRecord> effectRows;
                if (effectTextsById.TryGetValue(abilityRow.AbilityId, out effectRows))
                    textRows.AddRange(effectRows);

                foreach (IndexedStringRecord textRow in textRows)
                {
                    foreach (Match match in ComponentTokenRegex.Matches(textRow.Value ?? string.Empty))
                    {
                        int componentSlotIndex;
                        if (!int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out componentSlotIndex))
                            continue;
                        if (componentSlotIndex < 0 || componentSlotIndex >= abilityRow.ComponentIds.Count)
                            continue;

                        ushort componentId = abilityRow.ComponentIds[componentSlotIndex];
                        if (componentId <= 0)
                            continue;

                        BinaryComponentRecord componentRow;
                        if (!_componentsById.TryGetValue(componentId, out componentRow))
                            continue;

                        string tokenBody = match.Groups[2].Value;
                        ComponentFieldDescriptor descriptor;
                        if (!TryParseTokenField(tokenBody, componentRow.Operation, out descriptor))
                            continue;

                        ComponentTokenEvidence evidence = new ComponentTokenEvidence
                        {
                            AbilityId = abilityRow.AbilityId,
                            AbilityName = LookupAbilityName(abilityRow.AbilityId),
                            ComponentId = componentId,
                            ComponentSlotIndex = componentSlotIndex,
                            Operation = componentRow.Operation,
                            DomainKey = descriptor.DomainKey,
                            FieldKey = descriptor.FieldKey,
                            TokenBody = tokenBody,
                            Meaning = descriptor.TokenMeaning,
                            Source = textRow.TableName,
                            SourcePath = textRow.SourcePath,
                            SourceLocation = "line " + textRow.LineNumber.ToString(CultureInfo.InvariantCulture),
                            TextExcerpt = textRow.NormalizedValue,
                            ContextTags = BuildContextTags(textRow.NormalizedValue)
                        };

                        AddEvidence(_evidenceByDomainKey, descriptor.DomainKey, evidence);
                        AddEvidence(_evidenceByFieldKey, descriptor.FieldKey, evidence);
                        AddEvidence(_evidenceByAbilityComponentField, BuildAbilityComponentFieldKey(abilityRow.AbilityId, componentId, descriptor.FieldKey), evidence);
                    }
                }
            }
        }

        private Dictionary<ushort, List<AbilityComponentReference>> BuildAbilityReferencesByComponentId()
        {
            Dictionary<ushort, List<AbilityComponentReference>> referencesByComponentId = new Dictionary<ushort, List<AbilityComponentReference>>();
            if (_dataset == null)
                return referencesByComponentId;

            Dictionary<ushort, List<IndexedStringRecord>> textRowsByAbilityId = BuildAbilityTextRowsByAbilityId();
            foreach (BinaryAbilityRecord abilityRow in _dataset.BinaryAbilities.OrderBy(row => row.AbilityId).ThenBy(row => row.RecordIndex))
            {
                List<IndexedStringRecord> textRows;
                textRowsByAbilityId.TryGetValue(abilityRow.AbilityId, out textRows);

                for (int slotIndex = 0; slotIndex < abilityRow.ComponentIds.Count; ++slotIndex)
                {
                    ushort componentId = abilityRow.ComponentIds[slotIndex];
                    if (componentId <= 0)
                        continue;

                    AbilityTextSelection textSelection = SelectAbilityText(textRows, slotIndex);
                    AbilityComponentReference reference = new AbilityComponentReference
                    {
                        AbilityId = abilityRow.AbilityId,
                        AbilityName = LookupAbilityName(abilityRow.AbilityId),
                        ComponentId = componentId,
                        ComponentSlotIndex = slotIndex,
                        TriggerValue = slotIndex < abilityRow.Triggers.Count ? abilityRow.Triggers[slotIndex] : 0,
                        TriggerText = BuildTriggerText(slotIndex < abilityRow.Triggers.Count ? abilityRow.Triggers[slotIndex] : 0),
                        TextExcerpt = textSelection.TextExcerpt,
                        SourcePath = string.IsNullOrWhiteSpace(textSelection.SourcePath) ? abilityRow.SourcePath : textSelection.SourcePath,
                        SourceLocation = string.IsNullOrWhiteSpace(textSelection.SourceLocation) ? "byte " + abilityRow.ByteOffset.ToString(CultureInfo.InvariantCulture) : textSelection.SourceLocation,
                        ContextTags = textSelection.ContextTags
                    };

                    AddReference(referencesByComponentId, componentId, reference);
                }
            }

            return referencesByComponentId;
        }

        private Dictionary<ushort, List<IndexedStringRecord>> BuildAbilityTextRowsByAbilityId()
        {
            Dictionary<ushort, List<IndexedStringRecord>> rowsByAbilityId = new Dictionary<ushort, List<IndexedStringRecord>>();
            if (_dataset == null)
                return rowsByAbilityId;

            foreach (IndexedStringRecord row in _dataset.AbilityDescriptions.Where(row => row.EntryId <= ushort.MaxValue).OrderBy(row => row.EntryId).ThenBy(row => row.LineNumber))
                AddTextRow(rowsByAbilityId, (ushort)row.EntryId, row);
            foreach (IndexedStringRecord row in _dataset.AbilityEffectTexts.Where(row => row.EntryId <= ushort.MaxValue).OrderBy(row => row.EntryId).ThenBy(row => row.LineNumber))
                AddTextRow(rowsByAbilityId, (ushort)row.EntryId, row);

            return rowsByAbilityId;
        }

        private static void AddTextRow(Dictionary<ushort, List<IndexedStringRecord>> rowsByAbilityId, ushort abilityId, IndexedStringRecord row)
        {
            List<IndexedStringRecord> rows;
            if (!rowsByAbilityId.TryGetValue(abilityId, out rows))
            {
                rows = new List<IndexedStringRecord>();
                rowsByAbilityId[abilityId] = rows;
            }

            rows.Add(row);
        }

        private static AbilityTextSelection SelectAbilityText(List<IndexedStringRecord> rows, int slotIndex)
        {
            string tokenNeedle = "COM_" + slotIndex.ToString(CultureInfo.InvariantCulture) + "_";
            IndexedStringRecord preferred = rows == null
                ? null
                : rows.FirstOrDefault(row => !string.IsNullOrWhiteSpace(row.NormalizedValue) && row.NormalizedValue.IndexOf(tokenNeedle, StringComparison.OrdinalIgnoreCase) >= 0);
            if (preferred == null && rows != null)
                preferred = rows.FirstOrDefault(row => !string.IsNullOrWhiteSpace(row.NormalizedValue));

            string excerpt = preferred == null ? string.Empty : preferred.NormalizedValue;
            return new AbilityTextSelection
            {
                TextExcerpt = excerpt,
                SourcePath = preferred == null ? string.Empty : preferred.SourcePath,
                SourceLocation = preferred == null ? string.Empty : "line " + preferred.LineNumber.ToString(CultureInfo.InvariantCulture),
                ContextTags = BuildContextTags(excerpt)
            };
        }

        private static void AddReference(Dictionary<ushort, List<AbilityComponentReference>> referencesByComponentId, ushort componentId, AbilityComponentReference reference)
        {
            List<AbilityComponentReference> items;
            if (!referencesByComponentId.TryGetValue(componentId, out items))
            {
                items = new List<AbilityComponentReference>();
                referencesByComponentId[componentId] = items;
            }

            items.Add(reference);
        }

        private static List<AbilityComponentReference> DistinctAbilityReferences(IEnumerable<BinaryComponentRecord> components, Dictionary<ushort, List<AbilityComponentReference>> referencesByComponentId)
        {
            Dictionary<string, AbilityComponentReference> unique = new Dictionary<string, AbilityComponentReference>(StringComparer.OrdinalIgnoreCase);
            foreach (BinaryComponentRecord component in components)
            {
                List<AbilityComponentReference> references;
                if (!referencesByComponentId.TryGetValue(component.ComponentId, out references))
                    continue;

                foreach (AbilityComponentReference reference in references)
                {
                    string key = reference.AbilityId.ToString(CultureInfo.InvariantCulture)
                        + "|"
                        + reference.ComponentId.ToString(CultureInfo.InvariantCulture)
                        + "|"
                        + reference.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture);
                    if (!unique.ContainsKey(key))
                        unique[key] = reference;
                }
            }

            return unique.Values.ToList();
        }

        private List<ComponentOperationFieldRecord> BuildOperationFieldRecords(uint operationId, List<BinaryComponentRecord> components, List<AbilityComponentReference> abilityReferences)
        {
            Dictionary<string, List<FieldObservation>> observationsByField = BuildFieldObservationsByField(components);

            List<string> operationContextTags = abilityReferences
                .SelectMany(row => row.ContextTags ?? new List<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            int operationAbilityCount = abilityReferences
                .Select(row => row.AbilityId)
                .Distinct()
                .Count();
            List<ComponentOperationFieldRecord> rows = new List<ComponentOperationFieldRecord>();
            foreach (KeyValuePair<string, List<FieldObservation>> pair in observationsByField.OrderBy(entry => GetFieldSortKey(entry.Key), StringComparer.OrdinalIgnoreCase))
            {
                string domainKey = BuildOperationFieldDomainKey(operationId, pair.Key);
                DefinitionDomain domain = BuildDomain(domainKey);
                FieldObservationInference inference = BuildFieldObservationInference(operationId, pair.Key, pair.Value);
                List<string> tokenRenderings = domain == null
                    ? new List<string>()
                    : domain.Options.Where(option => !string.Equals(option.RawValue, "override", StringComparison.OrdinalIgnoreCase)).Select(option => option.RawValue).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList();
                List<string> semanticMeanings = domain == null
                    ? new List<string>()
                    : domain.Options.Select(option => option.Meaning).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToList();
                List<string> domainContextTags = GetDomainContextTags(domainKey);
                string notes = BuildOperationFieldNotes(pair.Key, pair.Value, domain, components.Count);
                if (inference != null && !string.IsNullOrWhiteSpace(inference.Notes))
                    notes += " " + inference.Notes;
                string confidence = semanticMeanings.Count == 0 && inference != null ? inference.Confidence : DetermineDomainConfidence(domain);
                FieldTriage triage = BuildFieldTriage(operationId, pair.Key, pair.Value, confidence, operationAbilityCount);

                rows.Add(new ComponentOperationFieldRecord
                {
                    FieldKey = pair.Key,
                    NonZeroCount = pair.Value.Count,
                    DistinctValueCount = pair.Value.Select(row => row.RawValue).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    SampleValuesText = JoinValues(pair.Value.Select(row => row.RawValue).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).Take(12)),
                    TokenRenderingsText = tokenRenderings.Count == 0 ? "(none)" : JoinValues(tokenRenderings),
                    SemanticSummary = semanticMeanings.Count == 0
                        ? inference == null ? "No extracted-client semantic mapping is known yet for this field." : inference.SemanticSummary
                        : string.Join(" ", semanticMeanings),
                    Confidence = confidence,
                    ContextTagsText = JoinValues(domainContextTags.Count == 0 ? operationContextTags : domainContextTags),
                    Notes = notes,
                    TriageScore = triage.Score,
                    TriageBucket = triage.Bucket,
                    TriageNotes = triage.Notes
                });
            }

            return rows;
        }

        private static Dictionary<string, List<FieldObservation>> BuildFieldObservationsByField(IEnumerable<BinaryComponentRecord> components)
        {
            Dictionary<string, List<FieldObservation>> observationsByField = new Dictionary<string, List<FieldObservation>>(StringComparer.OrdinalIgnoreCase);
            foreach (BinaryComponentRecord component in components ?? Enumerable.Empty<BinaryComponentRecord>())
            {
                AddFieldObservation(observationsByField, "ActivationDelay", component.ComponentId, component.LayoutVariant, component.ActivationDelay);
                AddFieldObservation(observationsByField, "Duration", component.ComponentId, component.LayoutVariant, component.Duration);
                AddFieldObservation(observationsByField, "FlagsRaw", component.ComponentId, component.LayoutVariant, component.FlagsRaw);
                AddFieldObservation(observationsByField, "Value08", component.ComponentId, component.LayoutVariant, component.Value08);
                AddFieldObservation(observationsByField, "Radius", component.ComponentId, component.LayoutVariant, component.Radius);
                AddFieldObservation(observationsByField, "ConeAngle", component.ComponentId, component.LayoutVariant, component.ConeAngle);
                AddFieldObservation(observationsByField, "FlightSpeed", component.ComponentId, component.LayoutVariant, component.FlightSpeed);
                AddFieldObservation(observationsByField, "Value15", component.ComponentId, component.LayoutVariant, component.Value15);
                AddFieldObservation(observationsByField, "MaxTargets", component.ComponentId, component.LayoutVariant, component.MaxTargets);
                AddFieldObservation(observationsByField, "Value00", component.ComponentId, component.LayoutVariant, component.Value00);
                AddFieldObservation(observationsByField, "Interval", component.ComponentId, component.LayoutVariant, component.Interval);

                for (int index = 0; index < component.Values.Count; ++index)
                    AddFieldObservation(observationsByField, "Value[" + index.ToString(CultureInfo.InvariantCulture) + "]", component.ComponentId, component.LayoutVariant, component.Values[index]);
                for (int index = 0; index < component.Multipliers.Count; ++index)
                    AddFieldObservation(observationsByField, "Multiplier[" + index.ToString(CultureInfo.InvariantCulture) + "]", component.ComponentId, component.LayoutVariant, component.Multipliers[index]);

                foreach (BinaryExtDataRecord extData in component.ExtData)
                {
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 1), component.ComponentId, component.LayoutVariant, extData.Val1);
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 2), component.ComponentId, component.LayoutVariant, extData.Val2);
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 3), component.ComponentId, component.LayoutVariant, extData.Val3);
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 4), component.ComponentId, component.LayoutVariant, extData.Val4);
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 5), component.ComponentId, component.LayoutVariant, extData.Val5);
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 6), component.ComponentId, component.LayoutVariant, extData.Val6);
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 7), component.ComponentId, component.LayoutVariant, extData.Val7);
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 8), component.ComponentId, component.LayoutVariant, extData.Val8);
                    AddFieldObservation(observationsByField, BuildExtDataFieldKey(extData.SlotIndex, 9), component.ComponentId, component.LayoutVariant, extData.Val9);
                }
            }

            return observationsByField;
        }

        private static void AddFieldObservation(Dictionary<string, List<FieldObservation>> observationsByField, string fieldKey, ushort componentId, string layoutVariant, uint rawValue)
        {
            if (rawValue == 0)
                return;

            AddFieldObservation(observationsByField, fieldKey, componentId, layoutVariant, rawValue.ToString(CultureInfo.InvariantCulture));
        }

        private static void AddFieldObservation(Dictionary<string, List<FieldObservation>> observationsByField, string fieldKey, ushort componentId, string layoutVariant, int rawValue)
        {
            if (rawValue == 0)
                return;

            AddFieldObservation(observationsByField, fieldKey, componentId, layoutVariant, rawValue.ToString(CultureInfo.InvariantCulture));
        }

        private static void AddFieldObservation(Dictionary<string, List<FieldObservation>> observationsByField, string fieldKey, ushort componentId, string layoutVariant, byte rawValue)
        {
            if (rawValue == 0)
                return;

            AddFieldObservation(observationsByField, fieldKey, componentId, layoutVariant, rawValue.ToString(CultureInfo.InvariantCulture));
        }

        private static void AddFieldObservation(Dictionary<string, List<FieldObservation>> observationsByField, string fieldKey, ushort componentId, string layoutVariant, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(fieldKey) || string.IsNullOrWhiteSpace(rawValue))
                return;

            List<FieldObservation> rows;
            if (!observationsByField.TryGetValue(fieldKey, out rows))
            {
                rows = new List<FieldObservation>();
                observationsByField[fieldKey] = rows;
            }

            rows.Add(new FieldObservation
            {
                FieldKey = fieldKey,
                RawValue = rawValue,
                ComponentId = componentId,
                LayoutVariant = layoutVariant
            });
        }

        private static string BuildExtDataFieldKey(int slotIndex, int valueIndex)
        {
            return "ExtData[" + slotIndex.ToString(CultureInfo.InvariantCulture) + "].Val" + valueIndex.ToString(CultureInfo.InvariantCulture);
        }

        private static bool TryReadFieldRawValue(BinaryComponentRecord component, string fieldKey, out string rawValue)
        {
            rawValue = null;
            if (component == null || string.IsNullOrWhiteSpace(fieldKey))
                return false;

            if (string.Equals(fieldKey, "ActivationDelay", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.ActivationDelay, out rawValue);
            if (string.Equals(fieldKey, "Duration", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.Duration, out rawValue);
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.FlagsRaw, out rawValue);
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.Value08, out rawValue);
            if (string.Equals(fieldKey, "Radius", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.Radius, out rawValue);
            if (string.Equals(fieldKey, "ConeAngle", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.ConeAngle, out rawValue);
            if (string.Equals(fieldKey, "FlightSpeed", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.FlightSpeed, out rawValue);
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.Value15, out rawValue);
            if (string.Equals(fieldKey, "MaxTargets", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.MaxTargets, out rawValue);
            if (string.Equals(fieldKey, "Value00", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.Value00, out rawValue);
            if (string.Equals(fieldKey, "Interval", StringComparison.OrdinalIgnoreCase))
                return TryFormatFieldValue(component.Interval, out rawValue);

            int index;
            if (TryParseIndexedField(fieldKey, "Value[", out index))
            {
                if (index >= 0 && index < component.Values.Count)
                    return TryFormatFieldValue(component.Values[index], out rawValue);
                return false;
            }

            if (TryParseIndexedField(fieldKey, "Multiplier[", out index))
            {
                if (index >= 0 && index < component.Multipliers.Count)
                    return TryFormatFieldValue(component.Multipliers[index], out rawValue);
                return false;
            }

            int slotIndex;
            int valueIndex;
            if (TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
            {
                BinaryExtDataRecord extRecord = component.ExtData.FirstOrDefault(row => row.SlotIndex == slotIndex);
                if (extRecord == null)
                    return false;

                switch (valueIndex)
                {
                    case 1: return TryFormatFieldValue(extRecord.Val1, out rawValue);
                    case 2: return TryFormatFieldValue(extRecord.Val2, out rawValue);
                    case 3: return TryFormatFieldValue(extRecord.Val3, out rawValue);
                    case 4: return TryFormatFieldValue(extRecord.Val4, out rawValue);
                    case 5: return TryFormatFieldValue(extRecord.Val5, out rawValue);
                    case 6: return TryFormatFieldValue(extRecord.Val6, out rawValue);
                    case 7: return TryFormatFieldValue(extRecord.Val7, out rawValue);
                    case 8: return TryFormatFieldValue(extRecord.Val8, out rawValue);
                    case 9: return TryFormatFieldValue(extRecord.Val9, out rawValue);
                }
            }

            return false;
        }

        private static bool TryParseIndexedField(string fieldKey, string prefix, out int index)
        {
            index = -1;
            if (string.IsNullOrWhiteSpace(fieldKey)
                || string.IsNullOrWhiteSpace(prefix)
                || !fieldKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                || !fieldKey.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                return false;

            return int.TryParse(fieldKey.Substring(prefix.Length, fieldKey.Length - prefix.Length - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out index);
        }

        private static bool TryParseExtDataField(string fieldKey, out int slotIndex, out int valueIndex)
        {
            slotIndex = -1;
            valueIndex = -1;
            if (string.IsNullOrWhiteSpace(fieldKey)
                || !fieldKey.StartsWith("ExtData[", StringComparison.OrdinalIgnoreCase))
                return false;

            int closeBracket = fieldKey.IndexOf(']');
            if (closeBracket <= "ExtData[".Length)
                return false;

            int dotIndex = fieldKey.IndexOf(".Val", StringComparison.OrdinalIgnoreCase);
            if (dotIndex <= closeBracket)
                return false;

            return int.TryParse(fieldKey.Substring("ExtData[".Length, closeBracket - "ExtData[".Length), NumberStyles.Integer, CultureInfo.InvariantCulture, out slotIndex)
                && int.TryParse(fieldKey.Substring(dotIndex + 4), NumberStyles.Integer, CultureInfo.InvariantCulture, out valueIndex);
        }

        private static bool TryDescribeDamageExtDataField(int valueIndex, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            switch (valueIndex)
            {
                case 1:
                    semanticSummary = "DAMAGE ext-data branch selector for the slot-local damage payload.";
                    roleNotes = "This low-cardinality field splits the dominant DAMAGE payload branches and tracks closely with the neighboring Val2, Val3, and Val7 fields.";
                    return true;
                case 2:
                    semanticSummary = "DAMAGE ext-data payload-A field for the slot-local damage payload.";
                    roleNotes = "This field mixes compact selector-like values with scalar-looking payloads, so it needs to be read together with the neighboring Val1, Val3, and Val7 fields.";
                    return true;
                case 3:
                    semanticSummary = "DAMAGE ext-data profile selector for the slot-local damage payload.";
                    roleNotes = "This compact enum-like field behaves like the main profile selector inside the recurring DAMAGE ext-data block.";
                    return true;
                case 4:
                    semanticSummary = "DAMAGE ext-data family marker for the slot-local damage payload.";
                    roleNotes = "This field behaves like a layout-family tag rather than a free scalar and overwhelmingly stays in the 8-family with a small 9-variant minority.";
                    return true;
                case 5:
                    semanticSummary = "DAMAGE ext-data auxiliary selector for minority slot-local damage branches.";
                    roleNotes = "This low-cardinality field only appears in minority DAMAGE ext-data branches and behaves more like an auxiliary selector or modifier than a wide scalar.";
                    return true;
                case 6:
                    semanticSummary = "DAMAGE ext-data reference/link field for the slot-local damage payload.";
                    roleNotes = "This field behaves like a branch-local reference slot: some DAMAGE branches carry exact RequirementId matches here, while other branches carry identifier-like values instead of scalar payloads.";
                    return true;
                case 7:
                    semanticSummary = "DAMAGE ext-data payload-B field for the slot-local damage payload.";
                    roleNotes = "This field behaves like the second profile-specific payload slot and carries the widest scalar-looking values in the recurring DAMAGE ext-data block.";
                    return true;
                case 8:
                    semanticSummary = "DAMAGE ext-data sparse tail payload field for minority slot-local damage branches.";
                    roleNotes = "This field only appears in minority DAMAGE ext-data branches after the main Val7 payload and behaves like an auxiliary tail parameter.";
                    return true;
                case 9:
                    semanticSummary = "DAMAGE ext-data slot-presence marker for minority slot-local damage branches.";
                    roleNotes = "This field is almost always fixed at 1 when present, so it behaves like a branch marker or enabled-flag rather than a free scalar.";
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryDescribeBonusTypeAdjustExtDataField(int valueIndex, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            switch (valueIndex)
            {
                case 1:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data branch selector for the slot-local bonus payload.";
                    roleNotes = "This low-cardinality field splits the dominant BONUS_TYPE_ADJUST payload branches and tracks closely with the neighboring Val2, Val3, and Val7 fields.";
                    return true;
                case 2:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data payload-A field for the slot-local bonus payload.";
                    roleNotes = "This field mixes compact selector-like values with scalar-looking payloads, so it needs to be read together with the neighboring Val1, Val3, and Val7 fields.";
                    return true;
                case 3:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data profile selector for the slot-local bonus payload.";
                    roleNotes = "This compact enum-like field behaves like the main profile selector inside the recurring BONUS_TYPE_ADJUST ext-data block.";
                    return true;
                case 4:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data family marker for the slot-local bonus payload.";
                    roleNotes = "This field behaves like a layout-family tag rather than a free scalar and overwhelmingly stays in the 8-family with a small 9-variant minority.";
                    return true;
                case 5:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data auxiliary selector for minority slot-local bonus branches.";
                    roleNotes = "This low-cardinality field only appears in minority BONUS_TYPE_ADJUST ext-data branches and behaves more like an auxiliary selector or modifier than a wide scalar.";
                    return true;
                case 6:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data reference/link field for the slot-local bonus payload.";
                    roleNotes = "This field behaves like a branch-local reference slot: some BONUS_TYPE_ADJUST branches carry exact RequirementId matches here, while other branches carry identifier-like values instead of scalar payloads.";
                    return true;
                case 7:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data payload-B field for the slot-local bonus payload.";
                    roleNotes = "This field behaves like the second profile-specific payload slot and carries the widest scalar-looking values in the recurring BONUS_TYPE_ADJUST ext-data block.";
                    return true;
                case 8:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data sparse tail payload field for minority slot-local bonus branches.";
                    roleNotes = "This field only appears in minority BONUS_TYPE_ADJUST ext-data branches after the main Val7 payload and behaves like an auxiliary tail parameter.";
                    return true;
                case 9:
                    semanticSummary = "BONUS_TYPE_ADJUST ext-data slot-presence marker for minority slot-local bonus branches.";
                    roleNotes = "This field is almost always fixed at 1 when present, so it behaves like a branch marker or enabled-flag rather than a free scalar.";
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryDescribeApplyAbilityValueField(string fieldKey, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "APPLY_ABILITY referenced ability ID — the AbilityId of the ability being applied by this component.";
                roleNotes = "Value[0] is non-zero in 99.7% of APPLY_ABILITY records (1839 distinct ability IDs across 2,075 records). "
                    + "Most frequent applied IDs: 24543 (56 records), 3682 (10 records). "
                    + "Cross-reference with abilityexport.bin AbilityId to resolve the full applied-ability definition.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "APPLY_ABILITY secondary application selector (typically 0; Value[1]=1 may indicate a modifier mode).";
                roleNotes = "Value[1] is 0 in 99.5% of APPLY_ABILITY records. "
                    + "Non-zero cases: Value[1]=1 (10 records, 0.5%), Value[1]=99 (1 record). "
                    + "Exact semantics of the non-zero values are unresolved.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "APPLY_ABILITY Value[2] — unused in extracted client data (always 0 for this operation).";
                roleNotes = "Value[2] is 0 in 100% of the 2,075 extracted APPLY_ABILITY records.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "APPLY_ABILITY Value[3] — near-inactive field (99.8% zero; Value[3]=7 in rare variant records).";
                roleNotes = "Value[3] is 0 in 99.8% of APPLY_ABILITY records (2071/2075). "
                    + "The 4 records with Value[3]=7 may encode a special application variant.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[4]", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "Value[6]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "APPLY_ABILITY Values[4-6] — unused in extracted client data (always 0 for this operation).";
                roleNotes = fieldKey + " is 0 in 100% of the 2,075 extracted APPLY_ABILITY records.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "APPLY_ABILITY Value[7] — near-inactive field (99.95% zero; Value[7]=75 in one record).";
                roleNotes = "Value[7] is 0 in 2074 of 2075 APPLY_ABILITY records. The single non-zero value (75) may be a scalar payload for a rare variant.";
                return true;
            }

            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "APPLY_ABILITY Value15 — near-inactive control flags (99.4% zero; non-zero values use CrowdControlTypes bit encoding).";
                roleNotes = "Value15 is 0 in 99.4% of APPLY_ABILITY records. "
                    + "Non-zero values observed: 4=Disarm(6 records), 1=Snare(2), 64=bit6(2), 16=Knockdown(1). "
                    + "These are the same CrowdControlTypes bits used by FlagsRaw, suggesting Value15 encodes a CC-gate or CC-immune flag.";
                return true;
            }

            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "APPLY_ABILITY Value08 — binary modifier flag (98% zero; Value08=1 in rare variant records).";
                roleNotes = "Value08 is 0 in 98% of APPLY_ABILITY records (2050/2075). "
                    + "Value08=1 appears in 25 records (1.2%) — may indicate a secondary application mode or persistence flag. "
                    + "Exact semantics of Value08=1 are unresolved.";
                return true;
            }

            int multiplierIndex;
            if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
            {
                semanticSummary = "APPLY_ABILITY " + fieldKey + " — percentage scaling multiplier (100 = 100% = no change; dominant value in 99.95% of records).";
                roleNotes = "Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] is 100 in 2074 of 2075 APPLY_ABILITY records. "
                    + "100 = 100% scaling = unmodified baseline. "
                    + "Rare exceptions: Multiplier[2]=68 (1 record), Multiplier[3]=150 (1 record). "
                    + "These 8 multiplier slots correspond to the 8 Values[] slots and scale each value slot independently.";
                return true;
            }

            return false;
        }

        private static bool TryDescribeApplyAbilityExtDataField(int valueIndex, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            switch (valueIndex)
            {
                case 1:
                    semanticSummary = "APPLY_ABILITY ext-data branch selector for the slot-local embedded payload.";
                    roleNotes = "This low-cardinality field splits the dominant APPLY_ABILITY payload branches, but the exact retail meaning of each raw value is still unresolved.";
                    return true;
                case 2:
                    semanticSummary = "APPLY_ABILITY ext-data operation-type code for the slot-local embedded payload.";
                    roleNotes = "This field consistently holds a ComponentOperation-family type code across extracted client BIN data. Values in the known range map to ComponentOperation names; values above the known range are undocumented operation types.";
                    return true;
                case 3:
                    semanticSummary = "APPLY_ABILITY ext-data profile selector for the slot-local embedded payload.";
                    roleNotes = "This compact enum-like field is the clearest profile or behavior selector inside the APPLY_ABILITY ext-data block.";
                    return true;
                case 4:
                    semanticSummary = "APPLY_ABILITY ext-data family marker for the slot-local embedded payload.";
                    roleNotes = "This field behaves like a payload-layout tag rather than a free scalar; extracted rows overwhelmingly use one dominant value with a small minority variant.";
                    return true;
                case 5:
                    semanticSummary = "APPLY_ABILITY ext-data auxiliary selector for minority slot-local payload branches.";
                    roleNotes = "This low-cardinality field only appears in minority APPLY_ABILITY ext-data branches and behaves more like an auxiliary selector or modifier than a wide scalar.";
                    return true;
                case 6:
                    semanticSummary = "APPLY_ABILITY ext-data reference/link field for the slot-local embedded payload.";
                    roleNotes = "This field behaves like a branch-local reference slot: some neighboring APPLY_ABILITY branches carry exact RequirementId matches here, while other branches carry large identifier-like values instead of scalar payloads.";
                    return true;
                case 7:
                    semanticSummary = "APPLY_ABILITY ext-data payload-B field for the slot-local embedded payload.";
                    roleNotes = "This field behaves like the second profile-specific payload slot and varies far more widely than the small selector fields around it.";
                    return true;
                case 8:
                    semanticSummary = "APPLY_ABILITY ext-data sparse tail payload field for minority slot-local payload branches.";
                    roleNotes = "This field only appears in minority APPLY_ABILITY ext-data branches after the main Val7 payload and behaves like an auxiliary tail parameter.";
                    return true;
                case 9:
                    semanticSummary = "APPLY_ABILITY ext-data slot-presence marker for minority slot-local payload branches.";
                    roleNotes = "This field is almost always fixed at 1 when present, so it behaves like a branch marker or enabled-flag rather than a free scalar.";
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryDescribeCcExtDataField(int valueIndex, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            switch (valueIndex)
            {
                case 1:
                    semanticSummary = "CC ext-data branch selector for the slot-local control payload.";
                    roleNotes = "This low-cardinality field splits the dominant CC payload branches and tracks closely with the neighboring Val2, Val3, and Val7 fields.";
                    return true;
                case 2:
                    semanticSummary = "CC ext-data operation-type code for the slot-local control payload.";
                    roleNotes = "This field consistently holds a ComponentOperation-family type code across extracted client BIN data. Values in the known range map to ComponentOperation names; values above the known range are undocumented operation types.";
                    return true;
                case 3:
                    semanticSummary = "CC ext-data profile selector for the slot-local control payload.";
                    roleNotes = "This compact enum-like field behaves like the main profile selector inside the recurring CC ext-data block.";
                    return true;
                case 4:
                    semanticSummary = "CC ext-data family marker for the slot-local control payload.";
                    roleNotes = "This field is almost fixed at 8 with a minority 9 variant, so it behaves like a layout-family tag rather than a free scalar.";
                    return true;
                case 5:
                    semanticSummary = "CC ext-data auxiliary selector for minority slot-local control branches.";
                    roleNotes = "This low-cardinality field only appears in minority CC ext-data branches and behaves more like an auxiliary selector or modifier than a wide scalar.";
                    return true;
                case 6:
                    semanticSummary = "CC ext-data reference/link field for the slot-local control payload.";
                    roleNotes = "This field behaves like a branch-local reference slot: some nearby CC branches carry exact RequirementId matches here, while other branches carry identifier-like values instead of scalar payloads.";
                    return true;
                case 7:
                    semanticSummary = "CC ext-data payload-B field for the slot-local control payload.";
                    roleNotes = "This field behaves like the second profile-specific payload slot and carries the widest scalar-looking values in the recurring CC ext-data block.";
                    return true;
                case 8:
                    semanticSummary = "CC ext-data sparse tail payload field for minority slot-local control branches.";
                    roleNotes = "This field only appears in minority CC ext-data branches after the main Val7 payload and behaves like an auxiliary tail parameter.";
                    return true;
                case 9:
                    semanticSummary = "CC ext-data slot-presence marker for minority slot-local control branches.";
                    roleNotes = "This field is almost always fixed at 1 when present, so it behaves like a branch marker or enabled-flag rather than a free scalar.";
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryDescribeKnockbackExtDataField(int valueIndex, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            switch (valueIndex)
            {
                case 1:
                    semanticSummary = "KNOCKBACK ext-data branch selector for the slot-local displacement payload.";
                    roleNotes = "This low-cardinality field splits the dominant KNOCKBACK payload branches and tracks closely with the neighboring Val2, Val3, and Val7 fields.";
                    return true;
                case 2:
                    semanticSummary = "KNOCKBACK ext-data payload-A field for the slot-local displacement payload.";
                    roleNotes = "This field mixes compact selector-like values with scalar-looking payloads, so it needs to be read together with the neighboring Val1, Val3, and Val7 fields.";
                    return true;
                case 3:
                    semanticSummary = "KNOCKBACK ext-data profile selector for the slot-local displacement payload.";
                    roleNotes = "This compact enum-like field behaves like the main profile selector inside the recurring KNOCKBACK ext-data block.";
                    return true;
                case 4:
                    semanticSummary = "KNOCKBACK ext-data family marker for the slot-local displacement payload.";
                    roleNotes = "This field behaves like a layout-family tag rather than a free scalar and overwhelmingly stays in the 8-family with small minority variants.";
                    return true;
                case 5:
                    semanticSummary = "KNOCKBACK ext-data auxiliary selector for minority slot-local displacement branches.";
                    roleNotes = "This low-cardinality field only appears in minority KNOCKBACK ext-data branches and behaves more like an auxiliary selector or modifier than a wide scalar.";
                    return true;
                case 6:
                    semanticSummary = "KNOCKBACK ext-data reference/link field for the slot-local displacement payload.";
                    roleNotes = "This field behaves like a branch-local reference slot: some KNOCKBACK branches carry exact RequirementId matches here, while other branches carry identifier-like values instead of scalar payloads.";
                    return true;
                case 7:
                    semanticSummary = "KNOCKBACK ext-data payload-B field for the slot-local displacement payload.";
                    roleNotes = "This field behaves like the second profile-specific payload slot and carries the widest scalar-looking values in the recurring KNOCKBACK ext-data block.";
                    return true;
                case 8:
                    semanticSummary = "KNOCKBACK ext-data sparse tail payload field for minority slot-local displacement branches.";
                    roleNotes = "This field only appears in minority KNOCKBACK ext-data branches after the main Val7 payload and behaves like an auxiliary tail parameter.";
                    return true;
                case 9:
                    semanticSummary = "KNOCKBACK ext-data slot-presence marker for minority slot-local displacement branches.";
                    roleNotes = "This field is almost always fixed at 1 when present, so it behaves like a branch marker or enabled-flag rather than a free scalar.";
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryDescribeImmunityExtDataField(int valueIndex, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            switch (valueIndex)
            {
                case 1:
                    semanticSummary = "IMMUNITY ext-data branch selector for the slot-local immunity payload.";
                    roleNotes = "This low-cardinality field splits the dominant IMMUNITY payload branches and tracks closely with the neighboring Val2, Val3, and Val7 fields.";
                    return true;
                case 2:
                    semanticSummary = "IMMUNITY ext-data payload-A field for the slot-local immunity payload.";
                    roleNotes = "This field mixes compact selector-like values with scalar-looking payloads, so it needs to be read together with the neighboring Val1, Val3, and Val7 fields.";
                    return true;
                case 3:
                    semanticSummary = "IMMUNITY ext-data profile selector for the slot-local immunity payload.";
                    roleNotes = "This compact enum-like field behaves like the main profile selector inside the recurring IMMUNITY ext-data block and drives the dominant IMMUNITY layout family.";
                    return true;
                case 4:
                    semanticSummary = "IMMUNITY ext-data family marker for the slot-local immunity payload.";
                    roleNotes = "This field behaves like a layout-family tag rather than a free scalar and stays almost entirely in the 8-family with a minority 9-variant.";
                    return true;
                case 5:
                    semanticSummary = "IMMUNITY ext-data auxiliary selector for minority slot-local immunity branches.";
                    roleNotes = "This low-cardinality field only appears in minority IMMUNITY ext-data branches and behaves more like an auxiliary selector or modifier than a wide scalar.";
                    return true;
                case 6:
                    semanticSummary = "IMMUNITY ext-data reference/link field for the slot-local immunity payload.";
                    roleNotes = "This field behaves like a branch-local reference slot: some IMMUNITY branches carry exact RequirementId matches here, while other branches carry identifier-like values instead of scalar payloads.";
                    return true;
                case 7:
                    semanticSummary = "IMMUNITY ext-data payload-B field for the slot-local immunity payload.";
                    roleNotes = "This field behaves like the second profile-specific payload slot and varies more widely than the small selector fields around it.";
                    return true;
                case 8:
                    semanticSummary = "IMMUNITY ext-data sparse tail payload field for minority slot-local immunity branches.";
                    roleNotes = "This field only appears in minority IMMUNITY ext-data branches after the main Val7 payload and behaves like an auxiliary tail parameter.";
                    return true;
                case 9:
                    semanticSummary = "IMMUNITY ext-data slot-presence marker for minority slot-local immunity branches.";
                    roleNotes = "This field is almost always fixed at 1 when present, so it behaves like a branch marker or enabled-flag rather than a free scalar.";
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryDescribeKnockbackValueField(string fieldKey, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "KNOCKBACK primary displacement magnitude for the component payload.";
                roleNotes = "This large-magnitude field is populated on nearly every KNOCKBACK row, including pure shove rows like Rune of Sundering and airborne launch rows like Gore, Khaine's Wrath, Exoneration, and Unleash the Winds, so it behaves like the base displacement strength rather than a branch selector.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "KNOCKBACK secondary launch or lift control for the component payload.";
                roleNotes = "This field is zero on flatter shove rows such as Rune of Sundering, but commonly lands in the 300-950 band on airborne launch rows and staged launch families such as Azyr's Beckoning, so it behaves like a secondary launch-vector or lift parameter rather than a branch selector.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "KNOCKBACK tertiary trajectory-bias control for the component payload.";
                roleNotes = "This field stays at 0 on simpler shove rows such as Rune of Sundering and Flames Of Fate, appears as small positive values on compact knockback test rows such as Knockback and Raid Boss Test Ability 1, and flips negative on mount-launch rows such as Mount - Arabian Disc - Midnight and Aphotic, so it behaves like a secondary trajectory-axis or bias control rather than another primary magnitude.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "KNOCKBACK displacement profile selector for the component payload.";
                roleNotes = "This low-cardinality field clusters with distinct motion families: 0 on simpler shove rows, 2 on the dominant player-launch family, and 3 or higher on minority scripted variants, so it behaves like a motion-profile selector rather than a scalar magnitude.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[4]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "KNOCKBACK Value[4] — near-inactive auxiliary field (1 non-zero record with value=7 across 262 extracted KNOCKBACK records).";
                roleNotes = "Value[4] is 0 in 261 of 262 KNOCKBACK records. The single non-zero exception (value=7) has unresolved semantics.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "KNOCKBACK Value[5] — near-inactive auxiliary field (2 non-zero records with value=50 across 262 extracted KNOCKBACK records).";
                roleNotes = "Value[5] is 0 in 260 of 262 KNOCKBACK records. The 2 non-zero exceptions (value=50) may represent a speed or percentage modifier for a rare KNOCKBACK variant.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[6]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "KNOCKBACK Value[6] — near-inactive auxiliary field (1 non-zero record with value=50 across 262 extracted KNOCKBACK records).";
                roleNotes = "Value[6] is 0 in 261 of 262 KNOCKBACK records. The single non-zero exception (value=50) has unresolved semantics.";
                return true;
            }

            if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "KNOCKBACK Value[7] — near-inactive auxiliary field (1 non-zero record with value=95 across 262 extracted KNOCKBACK records).";
                roleNotes = "Value[7] is 0 in 261 of 262 KNOCKBACK records. The single non-zero exception (value=95) may represent a percentage-scale modifier.";
                return true;
            }

            return false;
        }

        private static bool TryDescribeNamedControlField(string fieldKey, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            if (string.Equals(fieldKey, "ActivationDelay", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "Delay before the component payload activates after its trigger fires.";
                roleNotes = "The client-side field name and the observed round-number values indicate a timing control field rather than an enum or identifier.";
                return true;
            }

            if (string.Equals(fieldKey, "ConeAngle", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "Directional cone arc width used by the component payload.";
                roleNotes = "The client-side field name and the observed values align with common arc widths such as 45, 90, 120, 135, 180, and 360.";
                return true;
            }

            if (string.Equals(fieldKey, "FlightSpeed", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "Projectile or travel-speed control for the component payload.";
                roleNotes = "The client-side field name and the observed values behave like speed magnitudes rather than compact selector enums.";
                return true;
            }

            if (string.Equals(fieldKey, "MaxTargets", StringComparison.OrdinalIgnoreCase))
            {
                semanticSummary = "Maximum target-count cap for the component payload.";
                roleNotes = "The client-side field name and the observed small integer values indicate a hard cap on how many targets the component may affect.";
                return true;
            }

            return false;
        }

        private static bool TryDescribeGenericFlagsRawField(string fieldKey, out string semanticSummary, out string roleNotes)
        {
            semanticSummary = null;
            roleNotes = null;

            if (!string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                return false;

            semanticSummary = "Packed raw flag bitfield for the component payload.";
            roleNotes = "The client field name is explicit, but the exact meaning of each bit is still operation-specific and unresolved. Value-note bit positions are zero-based.";
            return true;
        }

        private static string BuildDominantRawValueSummary(IEnumerable<FieldObservation> observations, int limit)
        {
            List<FieldObservation> items = observations == null ? new List<FieldObservation>() : observations.ToList();
            if (items.Count == 0)
                return string.Empty;

            return string.Join(", ", items
                .GroupBy(row => row.RawValue, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Take(limit)
                .Select(group => group.Key + " x" + group.Count().ToString(CultureInfo.InvariantCulture)));
        }

        private static bool TryBuildDamageStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 1 || observations == null || observations.Count == 0)
                return false;

            // DAMAGE Value[1] = MaxCounter — counter cap / tick limit.
            // Confirmed from decompiled server-side DAMAGE.cs: public int MaxCounter { get { return Values[1]; } }
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string dmgV1Dominant = BuildDominantRawValueSummary(observations, 12);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value[1] — MaxCounter: counter cap / tick limit for this damage component.",
                    Confidence = SemanticConfidence.Confirmed,
                    Notes = "Confirmed from decompiled server-side DAMAGE.cs: `public int MaxCounter { get { return Values[1]; } }`. "
                        + "Controls the maximum number of ticks or counter threshold before the damage component stops applying. "
                        + "591 non-zero records with 81 distinct values; zero means no counter limit. "
                        + (string.IsNullOrWhiteSpace(dmgV1Dominant) ? string.Empty : "Observed: " + dmgV1Dominant + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string dmgFrDominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE FlagsRaw — DamageFlag enum (NONE=0, UNMITIGATABLE=1); bit0=unmitigatable 11%, bits1+ rare.",
                    Confidence = SemanticConfidence.Confirmed,
                    Notes = "Confirmed from decompiled server-side DAMAGE.cs: `public DamageFlag Type { get => (DamageFlag)Flags; }` where DamageFlag is NONE=0, UNMITIGATABLE=1. "
                        + "FlagsRaw=1 (UNMITIGATABLE) means the damage bypasses mitigation/armor calculations. "
                        + "FlagsRaw is 0 in 3494 of 3963 DAMAGE records (88%); value=1 in 453 records (11.4%); values 2 and 3 are rare (<0.25%). "
                        + (string.IsNullOrWhiteSpace(dmgFrDominant) ? string.Empty : "Observed: " + dmgFrDominant + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string dmgV2Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value[2] — damage sub-type or variant selector (74% zero; 7=type-7 13%, 6=type-6 12%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is 0 in 2945 of 3963 DAMAGE records (74%). "
                        + "When non-zero, the two dominant values are 7 (531 records, 13.4%) and 6 (472 records, 11.9%), together accounting for 25.3%. "
                        + "The low cardinality of non-zero values suggests this is a damage-type or sub-type selector. Retail names for 6 and 7 are unresolved. "
                        + (string.IsNullOrWhiteSpace(dmgV2Dominant) ? string.Empty : "Observed: " + dmgV2Dominant + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                string dmgV3Dominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value[3] — damage profile selector (86% zero; 7=profile-7 12.4%, 6=profile-6 1.8%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] is 0 in 3401 of 3963 DAMAGE records (86%). "
                        + "When non-zero: 7=491 records(12.4%), 6=70 records(1.8%), 10=1 record. "
                        + "This appears to be a secondary profile or sub-variant selector for DAMAGE components. Retail names for 6 and 7 are unresolved. "
                        + (string.IsNullOrWhiteSpace(dmgV3Dominant) ? string.Empty : "Observed: " + dmgV3Dominant + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string dmgV15Dominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value15 — CrowdControlTypes interaction flag (70% zero; non-zero values use CrowdControlTypes bit encoding).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 is 0 in 2780 of 3963 DAMAGE records (70%). "
                        + "Non-zero values use the same CrowdControlTypes bit pattern as CC FlagsRaw: "
                        + "1=Snare(16%), 16=Knockdown(10%), 17=Snare+Knockdown(1.1%), 64=bit6(1.1%), 65=Snare+bit6(0.8%), 81=Snare+Knockdown+bit6(0.5%), 4=Disarm(0.3%). "
                        + "This field likely encodes which CC condition triggers or is paired with this damage component. "
                        + (string.IsNullOrWhiteSpace(dmgV15Dominant) ? string.Empty : "Observed: " + dmgV15Dominant + ".")
                };
                return true;
            }

            // DAMAGE Multiplier[N]: percentage scaling multipliers; varied values (not near-constant 100 like passive ops).
            {
                int multiplierIndex;
                if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
                {
                    string dmgMulDominant = BuildDominantRawValueSummary(observations, 6);
                    inference = new FieldObservationInference
                    {
                        SemanticSummary = "DAMAGE " + fieldKey + " — percentage scaling multiplier for this damage component (100 = no change; varied values indicate active damage scaling).",
                        Confidence = SemanticConfidence.Inferred,
                        Notes = "DAMAGE Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] is a percentage multiplier applied to the damage formula. "
                            + "Unlike passive-operation multipliers (CC, KNOCKBACK, IMMUNITY) which are universally 100, DAMAGE multipliers carry meaningful non-100 values. "
                            + "100 = no scaling change; values above/below 100 scale the damage up/down. Retail slot-role names are unresolved. "
                            + (string.IsNullOrWhiteSpace(dmgMulDominant) ? string.Empty : "Observed: " + dmgMulDominant + ".")
                    };
                    return true;
                }
            }

            // DAMAGE Value[4]: sparse sub-type/variant selector (4 records; values 3 and 6 — matches Value[2] sub-type pattern).
            if (string.Equals(fieldKey, "Value[4]", StringComparison.OrdinalIgnoreCase))
            {
                string dmgV4Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value[4] — sparse sub-type or variant selector (4 non-zero records; values 3 and 6 match the Value[2] sub-type pattern).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[4] is non-zero in only 4 DAMAGE records with 2 distinct values (3, 6). "
                        + "Values 3 and 6 appear in the same sub-type range as DAMAGE Value[2] and Value[3]. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(dmgV4Dominant) ? string.Empty : "Observed: " + dmgV4Dominant + ".")
                };
                return true;
            }

            // DAMAGE Value[5]: near-inactive (4 records; single value = 90).
            if (string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value[5] — near-inactive modifier field (4 non-zero records; single value = 90).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[5] is non-zero in only 4 DAMAGE records, all with value=90. "
                        + "Likely a percentage-based secondary modifier (90 = 90%). Retail name unresolved."
                };
                return true;
            }

            // DAMAGE Value[6]: near-inactive (4 records; single value = 90).
            if (string.Equals(fieldKey, "Value[6]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value[6] — near-inactive modifier field (4 non-zero records; single value = 90).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[6] is non-zero in only 4 DAMAGE records, all with value=90. "
                        + "Likely a percentage-based secondary modifier (90 = 90%). Retail name unresolved."
                };
                return true;
            }

            // DAMAGE Value08: 14 non-zero, single value = 1. Binary activation flag (same pattern as BTA/STAT_CHANGE Value08).
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value08 — binary activation flag (value=1 in 14 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all DAMAGE records where it appears. "
                        + "Functions as a binary toggle or mode flag, consistent with the Value08=1 pattern seen in BTA, STAT_CHANGE, and EVENT_LISTENER. "
                        + "Retail name unresolved."
                };
                return true;
            }

            // DAMAGE Value[7]: likely a chance percentage or damage modifier (11 distinct values, mostly 5–100%).
            if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
            {
                string dmgV7Dominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE Value[7] — likely a chance percentage or conditional scalar (27 non-zero records; values mostly 5–100).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[7] is non-zero in 27 DAMAGE records (11 distinct values). "
                        + "Observed distribution (5, 10, 25, 50, 70, 75, 80, 85, 90, 95 plus outlier 500) suggests a chance/proc percentage or conditional multiplier. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(dmgV7Dominant) ? string.Empty : "Observed: " + dmgV7Dominant + ".")
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                return false;

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeDamageExtDataField(valueIndex, out semanticSummary, out roleNotes))
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 6);
            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Structural,
                Notes = "This is a structural inference from recurring DAMAGE ext-data layouts in extracted client BIN rows for ExtData slot "
                    + slotIndex.ToString(CultureInfo.InvariantCulture)
                    + ". "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                    + " Exact per-value retail semantics are still unresolved; use the value-profile evidence to inspect raw-value clusters."
            };
            return true;
        }

        private static bool TryBuildDamageChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 4 || observations == null || observations.Count == 0)
                return false;

            // DAMAGE_CHANGE Value08: 346 non-zero, single value = 1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE_CHANGE Value08 — binary activation flag (value=1 in 346 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all DAMAGE_CHANGE records where it appears. "
                        + "Functions as a binary toggle or mode flag, consistent with the Value08=1 pattern seen in BTA, STAT_CHANGE, and DAMAGE. "
                        + "Retail name unresolved."
                };
                return true;
            }

            // DAMAGE_CHANGE Value[1]: 43 non-zero, 7 distinct (-20, 1, 2, 5, 7, 20, 40) — damage-change magnitude or modifier parameter.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string dcV1Dominant = BuildDominantRawValueSummary(observations, 7);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE_CHANGE Value[1] — damage-change magnitude or secondary modifier (43 non-zero records, 7 distinct values: -20, 1, 2, 5, 7, 20, 40).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 43 non-zero records with 7 distinct values. "
                        + "Small integers (1, 2, 5, 7, 20, 40) plus -20 suggest a percentage or flat modifier applied to the damage-change effect. "
                        + "Negative value (-20) indicates the field can encode a reduction or penalty. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(dcV1Dominant) ? string.Empty : "Observed: " + dcV1Dominant + ".")
                };
                return true;
            }

            // DAMAGE_CHANGE Value[5]: 1 non-zero record, value=90. Near-inactive percentage modifier.
            if (string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE_CHANGE Value[5] — near-inactive percentage modifier (1 non-zero record; value = 90).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[5] is non-zero in only 1 DAMAGE_CHANGE record (value=90). Matches the 90% near-inactive modifier pattern in DAMAGE Value[5]/Value[6]. Retail name unresolved."
                };
                return true;
            }

            // DAMAGE_CHANGE Value[6]: 1 non-zero record, value=90. Near-inactive percentage modifier.
            if (string.Equals(fieldKey, "Value[6]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE_CHANGE Value[6] — near-inactive percentage modifier (1 non-zero record; value = 90).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[6] is non-zero in only 1 DAMAGE_CHANGE record (value=90). Matches the 90% near-inactive modifier pattern in DAMAGE Value[5]/Value[6]. Retail name unresolved."
                };
                return true;
            }

            // DAMAGE_CHANGE Value[2]: 5 non-zero, 4 distinct (-10, -33, 5, 10) — sparse secondary modifier or threshold.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string dcV2Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE_CHANGE Value[2] — sparse secondary modifier or threshold (5 non-zero records; values -33, -10, 5, 10).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in only 5 DAMAGE_CHANGE records with 4 distinct values. "
                        + "Positive and negative values suggest a conditional modifier or percentage threshold. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(dcV2Dominant) ? string.Empty : "Observed: " + dcV2Dominant + ".")
                };
                return true;
            }

            // DAMAGE_CHANGE FlagsRaw: 232 non-zero, 3 distinct (2=Root, 4=Disarm, 52=Disarm+Knockdown+Stagger). CrowdControlTypes.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string dcFrDominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE_CHANGE FlagsRaw — CrowdControlTypes gate flag (3 distinct values: 2=Root, 4=Disarm, 52=Disarm+Knockdown+Stagger).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 232 DAMAGE_CHANGE records with 3 distinct values. "
                        + "Values use CrowdControlTypes bit encoding: 2=Root(bit1), 4=Disarm(bit2), 52=Disarm+Knockdown+Stagger(bits2+4+5). "
                        + "Mirrors the CC gate pattern in DAMAGE Value15 and BTA Value15. "
                        + (string.IsNullOrWhiteSpace(dcFrDominant) ? string.Empty : "Observed: " + dcFrDominant + ".")
                };
                return true;
            }

            // DAMAGE_CHANGE Value15: CrowdControlTypes bit encoding (4=Disarm, 8=Silence, 12=Disarm+Silence, 36=Stagger+Disarm).
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string dcV15Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DAMAGE_CHANGE Value15 — CrowdControlTypes gate flag (4=Disarm 57%, 8=Silence 19%, 12=Disarm+Silence 14%, 36=Stagger+Disarm 10%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding to gate or condition the damage-change modifier. "
                        + "Observed values: 4=Disarm(57%), 8=Silence(19%), 12=Disarm+Silence(14%), 36=Stagger(32)+Disarm(4) ~10%. "
                        + "0 = no CC gate (inactive). "
                        + (string.IsNullOrWhiteSpace(dcV15Dominant) ? string.Empty : "Observed: " + dcV15Dominant + ".")
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 1)
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 5);
            inference = new FieldObservationInference
            {
                SemanticSummary = "DAMAGE_CHANGE ExtData Val1 — application target/scope selector (3=modifier-scope 49%, 2=target-apply 36%, 1=self-apply 11%).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "In DAMAGE_CHANGE components, Val1 selects the application target. "
                    + "Val1=3 (49%) = primary DAMAGE_CHANGE modifier scope (exact retail name unresolved). "
                    + "Val1=2 (36%) = standard target-application (universal Val1 meaning). "
                    + "Val1=1 (11%) = standard self-application (universal Val1 meaning). "
                    + "Val1=4 (<1%) = rare variant. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryBuildEventListenerStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 13 || observations == null || observations.Count == 0)
                return false;

            // EVENT_LISTENER Value15: 12 non-zero, 2 distinct (4=Disarm, 32=Stagger). CrowdControlTypes bits.
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EVENT_LISTENER Value15 — CrowdControlTypes gate flag (2 distinct values: 4=Disarm, 32=Stagger).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding: 4=Disarm(bit2), 32=Stagger(bit5). "
                        + "Only 12 EVENT_LISTENER records have non-zero Value15. 0 = no CC gate."
                };
                return true;
            }

            // EVENT_LISTENER Value[2]: 1 non-zero record, value=1. Near-inactive.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EVENT_LISTENER Value[2] — near-inactive field (1 non-zero record; value = 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in only 1 EVENT_LISTENER record (value=1). Retail name unresolved."
                };
                return true;
            }

            // EVENT_LISTENER FlagsRaw: 1 non-zero record, value=224 (Stagger+bit6+Grapple in CrowdControlTypes encoding).
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EVENT_LISTENER FlagsRaw — CrowdControlTypes gate flag (1 non-zero record; value=224=Stagger(32)+bit6(64)+Grapple(128)).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in only 1 EVENT_LISTENER record with value=224. "
                        + "224 = Stagger(32) + bit6(64) + Grapple(128) using CrowdControlTypes bit encoding. "
                        + "Likely gates the event listener on a CC condition. Retail name unresolved."
                };
                return true;
            }

            // EVENT_LISTENER Value08: 414 non-zero records, all value=1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EVENT_LISTENER Value08 — binary activation flag (value=1 in 414 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all EVENT_LISTENER records. "
                        + "Functions as a binary toggle or mode flag. Retail name unresolved."
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 1)
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 5);
            inference = new FieldObservationInference
            {
                SemanticSummary = "EVENT_LISTENER ExtData Val1 — listener scope selector (4=primary-listener 63%, 2=target-scope 34%, 1=self-scope 2%).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "In EVENT_LISTENER components, Val1 selects the listener application scope. "
                    + "Val1=4 (63%) = primary EVENT_LISTENER scope (see companion Val2 for event type code, Val3 for profile). "
                    + "Val1=2 (34%) = target-scope listener (universal Val1 meaning). "
                    + "Val1=1 (2%) = self-scope listener (universal Val1 meaning). "
                    + "Retail names for Val1=4 are unresolved. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryBuildBonusTypeAdjustStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 22 || observations == null || observations.Count == 0)
                return false;

            // BONUS_TYPE_ADJUST FlagsRaw: 29 non-zero, 7 distinct (1, 2, 8, 16, 64, 512, 15360) — bit-field encoding bonus-type flags.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string btaFrDominant = BuildDominantRawValueSummary(observations, 7);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "BONUS_TYPE_ADJUST FlagsRaw — bit-field encoding bonus-type modifier flags (7 distinct values: 1, 2, 8, 16, 64, 512, 15360).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 29 BONUS_TYPE_ADJUST records with 7 distinct bit-pattern values: "
                        + "1(bit0), 2(bit1), 8(bit3), 16(bit4), 64(bit6), 512(bit9), 15360(bits10-13=0x3C00). "
                        + "Values are all powers-of-two or grouped bit combinations — consistent with a bonus-type flag set rather than an enum. "
                        + "Retail bit names unresolved. "
                        + (string.IsNullOrWhiteSpace(btaFrDominant) ? string.Empty : "Observed: " + btaFrDominant + ".")
                };
                return true;
            }

            // BONUS_TYPE_ADJUST Value[2]: 78 non-zero, 6 distinct (-1, 1, 2, 7, 25, 100) — sub-type or modifier selector.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string btaV2Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "BONUS_TYPE_ADJUST Value[2] — sub-type or modifier selector (78 non-zero records, 6 distinct values: -1, 1, 2, 7, 25, 100).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] has 6 distinct values including -1 (possibly 'all types'), 100 (percentage max), and small integers (1, 2, 7, 25). "
                        + "Likely encodes a sub-type, modifier scope, or secondary bonus parameter. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(btaV2Dominant) ? string.Empty : "Observed: " + btaV2Dominant + ".")
                };
                return true;
            }

            // BONUS_TYPE_ADJUST Value[3]: 4 non-zero, 2 distinct (2, 10) — sparse secondary modifier.
            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "BONUS_TYPE_ADJUST Value[3] — sparse secondary modifier (4 non-zero records; values 2 and 10).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] is non-zero in only 4 BTA records (values 2 and 10). Retail name unresolved."
                };
                return true;
            }

            // BONUS_TYPE_ADJUST Value[7]: 1 non-zero record, value=90. Near-inactive.
            if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "BONUS_TYPE_ADJUST Value[7] — near-inactive modifier (1 non-zero record; value = 90).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[7] is non-zero in only 1 BONUS_TYPE_ADJUST record (value=90). Likely a 90% percentage modifier. Retail name unresolved."
                };
                return true;
            }

            // BONUS_TYPE_ADJUST Multiplier[1]: 1497 non-zero records, 32 distinct values — bonus scaling multiplier.
            {
                int multiplierIndex;
                if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
                {
                    string btaMulDominant = BuildDominantRawValueSummary(observations, 6);
                    inference = new FieldObservationInference
                    {
                        SemanticSummary = "BONUS_TYPE_ADJUST " + fieldKey + " — bonus scaling multiplier (100 = base 100%; higher values indicate scaled bonus amounts).",
                        Confidence = SemanticConfidence.Inferred,
                        Notes = "BONUS_TYPE_ADJUST Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] carries the scaling factor for this bonus-type adjustment. "
                            + "Observed values include 100 (baseline), and larger values (1000–2500) likely representing high-magnitude bonus percentages or fixed-point scaling. "
                            + "Retail slot-role name unresolved. "
                            + (string.IsNullOrWhiteSpace(btaMulDominant) ? string.Empty : "Observed: " + btaMulDominant + ".")
                    };
                    return true;
                }
            }

            // BONUS_TYPE_ADJUST Value08: 696 non-zero records, all value=1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "BONUS_TYPE_ADJUST Value08 — binary activation flag (value=1 in 696 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all BONUS_TYPE_ADJUST records. "
                        + "Functions as a binary toggle or mode flag for the bonus-type adjustment. Retail name unresolved."
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string btaV15Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "BONUS_TYPE_ADJUST Value15 — CrowdControlTypes gate flag (41% zero; 4=Disarm 55%, 8=Silence 4%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 is 0 in 610 of 1497 BONUS_TYPE_ADJUST records (41%). "
                        + "Non-zero values use CrowdControlTypes bit encoding: 4=Disarm(830 records, 55%), 8=Silence(57 records, 4%). "
                        + "This field likely gates the bonus-type adjustment to targets under specific CC conditions. "
                        + (string.IsNullOrWhiteSpace(btaV15Dominant) ? string.Empty : "Observed: " + btaV15Dominant + ".")
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                return false;

            // BONUS_TYPE_ADJUST Val1: upgrade from Structural to Inferred with distribution-specific notes.
            if (valueIndex == 1)
            {
                string btaVal1Dominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "BONUS_TYPE_ADJUST ExtData Val1 — application target/scope selector (3=modifier-scope 55%, 2=target-apply 34%, 1=self-apply 10%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "In BONUS_TYPE_ADJUST components, Val1 selects the application target. "
                        + "Val1=3 (55%) = primary BONUS_TYPE_ADJUST modifier scope (exact retail name unresolved). "
                        + "Val1=2 (34%) = standard target-application (universal Val1 meaning). "
                        + "Val1=1 (10%) = standard self-application (universal Val1 meaning). "
                        + "Val1=4 (<1%) = rare variant. "
                        + (string.IsNullOrWhiteSpace(btaVal1Dominant) ? string.Empty : "Observed: " + btaVal1Dominant + ".")
                };
                return true;
            }

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeBonusTypeAdjustExtDataField(valueIndex, out semanticSummary, out roleNotes))
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 6);
            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Structural,
                Notes = "This is a structural inference from recurring BONUS_TYPE_ADJUST ext-data layouts in extracted client BIN rows for ExtData slot "
                    + slotIndex.ToString(CultureInfo.InvariantCulture)
                    + ". "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                    + " Exact per-value retail semantics are still unresolved; use the value-profile evidence to inspect raw-value clusters."
            };
            return true;
        }

        private static bool TryBuildEffectBuffStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 15 || observations == null || observations.Count == 0)
                return false;

            // EFFECT_BUFF Value08: 322 non-zero records, all value=1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EFFECT_BUFF Value08 — binary activation flag (value=1 in 322 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all EFFECT_BUFF records. "
                        + "Functions as a binary toggle or mode flag. Retail name unresolved."
                };
                return true;
            }

            // EFFECT_BUFF FlagsRaw: 4 distinct values (1, 2, 16, 2081=1+16+2048). Bit-field with bits 0, 1, 4, 11.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string ebFrDominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EFFECT_BUFF FlagsRaw — sparse bit-field (4 distinct values: 1, 2, 16, 2081).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw has 4 distinct values: 1 (bit0), 2 (bit1), 16 (bit4), 2081 (bits 0+4+11). "
                        + "Likely encodes effect-buff behavioral flags. Bit 11 (2048) also appears in CC FlagsRaw dominant patterns. "
                        + "Retail names unresolved. "
                        + (string.IsNullOrWhiteSpace(ebFrDominant) ? string.Empty : "Observed: " + ebFrDominant + ".")
                };
                return true;
            }

            // EFFECT_BUFF Value[2]: 3 records, values {1, 6} — sparse sub-type selector.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EFFECT_BUFF Value[2] — sparse sub-type or variant selector (3 non-zero records; values 1 and 6).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in only 3 EFFECT_BUFF records with 2 distinct values (1, 6). Retail name unresolved."
                };
                return true;
            }

            // EFFECT_BUFF Value[3]: 4 records, values {6, 7} — sparse sub-type selector.
            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EFFECT_BUFF Value[3] — sparse sub-type or variant selector (4 non-zero records; values 6 and 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] is non-zero in only 4 EFFECT_BUFF records with 2 distinct values (6, 7). Retail name unresolved."
                };
                return true;
            }

            // EFFECT_BUFF Value[5]: 1 record, value = 50. Near-inactive.
            if (string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EFFECT_BUFF Value[5] — near-inactive modifier (1 non-zero record; value = 50).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[5] is non-zero in only 1 EFFECT_BUFF record (value=50). Likely a percentage modifier. Retail name unresolved."
                };
                return true;
            }

            // EFFECT_BUFF Value15: 6 distinct values (1, 4, 16, 32, 64, 65). CrowdControlTypes bit encoding.
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string ebV15Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "EFFECT_BUFF Value15 — CrowdControlTypes gate flag (6 distinct values using bit encoding: 1=Snare, 4=Disarm, 16=Knockdown, 32=Stagger, 64=bit6, 65=Snare+bit6).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding similar to CC FlagsRaw and DAMAGE Value15. "
                        + "Observed values: 1=Snare, 4=Disarm, 16=Knockdown, 32=Stagger, 64=bit6-unknown, 65=Snare+bit6. "
                        + "Likely gates or conditions the effect-buff on a CC state. "
                        + (string.IsNullOrWhiteSpace(ebV15Dominant) ? string.Empty : "Observed: " + ebV15Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildStatChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 2 || observations == null || observations.Count == 0)
                return false;

            // STAT_CHANGE Value08: 191 non-zero, single value = 1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "STAT_CHANGE Value08 — binary activation flag (value=1 in 191 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all STAT_CHANGE records. Functions as a binary toggle or mode flag. Retail name unresolved."
                };
                return true;
            }

            // STAT_CHANGE Value15: 181 non-zero, 2 distinct (4=Disarm, 32=Stagger). CrowdControlTypes bits.
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string scV15Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "STAT_CHANGE Value15 — CrowdControlTypes gate flag (2 distinct values: 4=Disarm, 32=Stagger).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding to gate or condition the stat change. "
                        + "Non-zero values: 4=Disarm and 32=Stagger. 0 = no CC gate (inactive). "
                        + "Mirrors the CC gate pattern seen in DAMAGE Value15, BTA Value15, and DAMAGE_CHANGE Value15. "
                        + (string.IsNullOrWhiteSpace(scV15Dominant) ? string.Empty : "Observed: " + scV15Dominant + ".")
                };
                return true;
            }

            // STAT_CHANGE Value[2]: 37 non-zero, single value = 7. Near-inactive sub-type variant.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "STAT_CHANGE Value[2] — near-inactive sub-type selector (37 non-zero records; single value = 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in 37 STAT_CHANGE records, all with value=7. "
                        + "Matches the sub-type-7 pattern seen in DAMAGE Value[2] and Value[3]. Retail name unresolved."
                };
                return true;
            }

            // STAT_CHANGE Multiplier[N]: 1363 non-zero, 11 distinct (-25, 1, 10, 30, 50, 70, 75, 90, 100, 133, 150).
            // Percentage scaling multiplier for the stat-change effect — same role as DAMAGE Multiplier[N].
            {
                int multiplierIndex;
                if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
                {
                    string scMulDominant = BuildDominantRawValueSummary(observations, 8);
                    inference = new FieldObservationInference
                    {
                        SemanticSummary = "STAT_CHANGE " + fieldKey + " — percentage scaling multiplier for this stat-change component (100 = no change; varied values indicate active stat scaling).",
                        Confidence = SemanticConfidence.Inferred,
                        Notes = "STAT_CHANGE Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] is a percentage multiplier applied to the stat-change formula. "
                            + "Observed values include 100 (baseline), percentage increments (10, 30, 50, 70, 75, 90), above-baseline values (133, 150), and a negative value (-25 = 25% reduction). "
                            + "Mirrors the DAMAGE Multiplier[N] pattern; retail slot-role name unresolved. "
                            + (string.IsNullOrWhiteSpace(scMulDominant) ? string.Empty : "Observed: " + scMulDominant + ".")
                    };
                    return true;
                }
            }

            return false;
        }

        private static bool TryBuildDiskUpdateStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 44 || observations == null || observations.Count == 0)
                return false;

            // DISK_UPDATE Value[0]: 43 non-zero, 40 distinct values (high IDs: 507, 1356–9311). ID reference.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string diskV0Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DISK_UPDATE Value[0] — referenced disk/zone object ID (43 non-zero records, 40 distinct values).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] holds a non-zero ID in 43 DISK_UPDATE records (40 distinct IDs). "
                        + "High distinct-value count relative to record count indicates a direct ID reference. "
                        + "Sample IDs: 507, 1356–1358, 6340, 6390, 7703, 9307–9311. "
                        + (string.IsNullOrWhiteSpace(diskV0Dominant) ? string.Empty : "Observed: " + diskV0Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildDefensiveStatChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 16 || observations == null || observations.Count == 0)
                return false;

            // DEFENSIVE_STAT_CHANGE FlagsRaw: 166 non-zero, 12 distinct values (1-9, 12, 14, 15). Bit-field or defense-type enum.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string dscFrDominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DEFENSIVE_STAT_CHANGE FlagsRaw — defense-type selector or bit-field (12 distinct values: 1–9, 12, 14, 15).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 166 DEFENSIVE_STAT_CHANGE records with 12 distinct values. "
                        + "Values 1–9 are sequential; 12 (=0b1100), 14 (=0b1110), 15 (=0b1111) suggest bit combinations. "
                        + "Likely encodes a defense type (magic school, damage type, or stat category) or behavioral flags. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(dscFrDominant) ? string.Empty : "Observed: " + dscFrDominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildServerCommandStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 36 || observations == null || observations.Count == 0)
                return false;

            // SERVER_COMMAND Value[0]: command code. Londos DB (War_AbilityComponentBin.sql, 1065 op=36 rows) confirms Values[0] = command code
            // (72+ distinct values: 32, 50, 87, 101, 228, 304, 327, 328, 332, 374, etc.). Value[0] has no handler below — falls through to generic.
            // TODO: add per-command-code named inferences once BIN Values[0] distribution is verified.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string scV0Dominant = BuildDominantRawValueSummary(observations, 12);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND Value[0] — command code (polymorphic command selector).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] holds the SERVER_COMMAND command code. Londos DB (War_AbilityComponentBin.sql) confirms 72+ distinct command codes in this position "
                        + "(e.g., 32=award item/ability, 50=similar award, 327/328=state toggle, 332=career mastery unlock, 87=unresolved, 304=unresolved). "
                        + "FlagsRaw (1,2,4,5,6,16,18,175) is a separate behavioral bit-field, NOT the command code. "
                        + "Retail name for Value[0] unresolved. "
                        + (string.IsNullOrWhiteSpace(scV0Dominant) ? string.Empty : "Observed: " + scV0Dominant + ".")
                };
                return true;
            }

            // SERVER_COMMAND Value[1]: primary command argument — the polymorphic arg keyed on Value[0] command code.
            // Londos DB: Values[1] = ability/item/zone ID for most commands; small enum for some (327/328 always = 1).
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string scV1Dominant = BuildDominantRawValueSummary(observations, 12);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND Value[1] — primary command argument (polymorphic; role depends on Value[0] command code).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] is the primary argument for the command specified in Value[0]. Londos DB patterns: "
                        + "command 32 → large IDs (30000–60000, likely award IDs); "
                        + "command 50 → similar large IDs; "
                        + "command 327/328 → constant value 1; "
                        + "command 332 → career line or mastery IDs (6–59 range); "
                        + "command 173 → 0 (arg is in Value[2]); "
                        + "command 87/88/101 → small enum or 0; "
                        + "command 304 → an ability ID, naming the ability whose persistent state the command acts on. "
                        + "Command 304 has ten rows and every one of them resolves: \"Tear Down\" (24854) carries four of them "
                        + "targeting the deployable Warp-Energy Condenser and Accumulator abilities (24809, 24810, 24818, 24819) "
                        + "alongside two DISPEL_BUFF components, and \"Detonate\" (24828) targets \"Sabotage\" (24826) - the "
                        + "ability that planted the charge it detonates. \"Sabotage\" and \"Bolster Down\" (24945) each target "
                        + "themselves, and the eight Skaven Play-as-Monster controls (24857-24864) share component 26669, which "
                        + "targets \"Play-As-Monster Master Client Controller\" (27950) - the same ability those controls also "
                        + "APPLY_ABILITY through component 26660. Teardown of the named ability's persistent object fits every "
                        + "row except that last pair, where apply-and-304 on one ability is more consistent with rebinding or "
                        + "clearing a prior controller before installing this one. Not yet separated. "
                        + "Retail field name unresolved. "
                        + (string.IsNullOrWhiteSpace(scV1Dominant) ? string.Empty : "Observed: " + scV1Dominant + ".")
                };
                return true;
            }

            // SERVER_COMMAND Value[2]: 337 non-zero, 59 distinct — secondary command argument.
            // Londos confirms this is the second argument slot (after command code in Value[0] and primary arg in Value[1]).
            // Some commands (e.g., 173) use Value[2] as the primary arg with Value[1]=0.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string scV2Dominant = BuildDominantRawValueSummary(observations, 12);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND Value[2] — secondary command argument (337 non-zero records, 59 distinct values; tri-modal: small enums, ID references, and sentinel values).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is the secondary argument slot for SERVER_COMMAND. Distinct value ranges: "
                        + "−1 (sentinel: all-types or no-filter), "
                        + "small integers 1–17 (sub-type enum or mode selector), "
                        + "mid-range 150–1000 (percentage or scaled parameter), "
                        + "high-range 12823/16336 (entity or resource ID references, used when Value[1]=0), "
                        + "100000 (maximum/infinity sentinel). "
                        + "Londos: command 332 has Values[2]=5 (constant); command 53 has Values[2]=0/1/2/3 (variant enum); command 173 has Values[2]=large ID. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(scV2Dominant) ? string.Empty : "Observed: " + scV2Dominant + ".")
                };
                return true;
            }

            // SERVER_COMMAND Value[3]: 38 non-zero, 5 distinct (-2, 1, 5, 6, 621) — secondary command parameter.
            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                string scV3Dominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND Value[3] — secondary command parameter (38 non-zero records, 5 distinct values: -2, 1, 5, 6, 621).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] has 38 non-zero records with 5 distinct values. "
                        + "Small integers (1, 5, 6) dominate; -2 is a special sentinel or flag value; 621 is a large outlier possibly an ID reference. "
                        + "Likely encodes a secondary command mode, target override, or parametric argument. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(scV3Dominant) ? string.Empty : "Observed: " + scV3Dominant + ".")
                };
                return true;
            }

            // SERVER_COMMAND Value[4]: 5 non-zero, 2 distinct (1, 2) — near-inactive binary or mode flag.
            if (string.Equals(fieldKey, "Value[4]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND Value[4] — near-inactive binary or mode flag (5 non-zero records; values 1 and 2).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[4] is non-zero in only 5 SERVER_COMMAND records (values 1 and 2). "
                        + "Likely an optional binary switch or secondary mode selector. Retail name unresolved."
                };
                return true;
            }

            // SERVER_COMMAND Value[5]: 1 non-zero record, value=10. Near-inactive.
            if (string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND Value[5] — near-inactive field (1 non-zero record; value = 10).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[5] is non-zero in only 1 SERVER_COMMAND record (value=10). Retail name unresolved."
                };
                return true;
            }

            // SERVER_COMMAND Value08: 1 non-zero record, value=1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND Value08 — binary activation flag (value=1 in 1 record; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) in the single SERVER_COMMAND record where it appears. "
                        + "Consistent with the Value08=1 binary-flag pattern across operations. Retail name unresolved."
                };
                return true;
            }

            // SERVER_COMMAND Value15: 3 non-zero, 3 distinct (1=Snare, 32=Stagger, 64=bit6). CrowdControlTypes bits.
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND Value15 — CrowdControlTypes gate flag (3 distinct values: 1=Snare, 32=Stagger, 64=bit6).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding: 1=Snare(bit0), 32=Stagger(bit5), 64=bit6-unknown. "
                        + "Only 3 SERVER_COMMAND records have non-zero Value15. 0 = no CC gate."
                };
                return true;
            }

            // SERVER_COMMAND FlagsRaw: 8 distinct values — bit-field (1, 2, 4, 5, 6, 16, 18, 175).
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string scFrDominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SERVER_COMMAND FlagsRaw — behavioral bit-field (8 distinct values: 1, 2, 4, 5, 6, 16, 18, 175).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 144 SERVER_COMMAND records with 8 distinct values. "
                        + "Values are bit-flag combinations: 1(bit0), 2(bit1), 4(bit2), 5(bits0+2), 6(bits1+2), 16(bit4), 18(bits1+4), 175(multiple bits). "
                        + "Likely encodes server-side command mode or behavior flags. Retail names unresolved. "
                        + (string.IsNullOrWhiteSpace(scFrDominant) ? string.Empty : "Observed: " + scFrDominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildApChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 6 || observations == null || observations.Count == 0)
                return false;

            // AP_CHANGE Value15: 39 non-zero, 4 distinct (1=Snare, 4=Disarm, 16=Knockdown, 32=Stagger). CrowdControlTypes bits.
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string apV15Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "AP_CHANGE Value15 — CrowdControlTypes gate flag (4 distinct values: 1=Snare, 4=Disarm, 16=Knockdown, 32=Stagger).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding: 1=Snare(bit0), 4=Disarm(bit2), 16=Knockdown(bit4), 32=Stagger(bit5). "
                        + "Non-zero in 39 AP_CHANGE records. 0 = no CC gate (inactive). "
                        + "Mirrors the CC gate pattern seen across DAMAGE, STAT_CHANGE, BTA, and other operations. "
                        + (string.IsNullOrWhiteSpace(apV15Dominant) ? string.Empty : "Observed: " + apV15Dominant + ".")
                };
                return true;
            }

            // AP_CHANGE FlagsRaw: 5 non-zero, 2 distinct (1=bit0, 114688=bits14-16) — bit-field mode flags.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string apFrDominant = BuildDominantRawValueSummary(observations, 2);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "AP_CHANGE FlagsRaw — bit-field mode flags (2 distinct values: 1=bit0, 114688=bits14-16).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 5 AP_CHANGE records with 2 distinct values: 1(bit0) and 114688(0x1C000, bits14-16). "
                        + "High-bit pattern 114688 is the same 0x1C000 grouping seen in ARMOR_CHANGE FlagsRaw. "
                        + "Likely encodes AP-change mode or targeting flags. Retail bit names unresolved. "
                        + (string.IsNullOrWhiteSpace(apFrDominant) ? string.Empty : "Observed: " + apFrDominant + ".")
                };
                return true;
            }

            // AP_CHANGE Value[7]: 2 non-zero, 2 distinct (25, 50) — sparse percentage threshold or modifier.
            if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "AP_CHANGE Value[7] — sparse percentage threshold or modifier (2 non-zero records; values 25 and 50).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[7] is non-zero in only 2 AP_CHANGE records (values 25 and 50). "
                        + "Both values are percentage-like (25% and 50%). Retail name unresolved."
                };
                return true;
            }

            // AP_CHANGE Value[1]: 13 non-zero, 3 distinct (-1, -20, 7) — AP modifier magnitude or index.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string apV1Dominant = BuildDominantRawValueSummary(observations, 3);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "AP_CHANGE Value[1] — AP modifier magnitude or parameter (13 non-zero records, 3 distinct values: -20, -1, 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 3 distinct values: -1 (sentinel or all-types marker), -20 (20-unit AP reduction), 7 (sub-type 7). "
                        + "Mixed positive and negative values suggest an AP amount or percentage modifier. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(apV1Dominant) ? string.Empty : "Observed: " + apV1Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildUpdateCounterStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 25 || observations == null || observations.Count == 0)
                return false;

            // UPDATE_COUNTER Value08: 148 non-zero, single value = 1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "UPDATE_COUNTER Value08 — binary activation flag (value=1 in 148 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all UPDATE_COUNTER records where it appears. "
                        + "Consistent with the Value08=1 binary-flag pattern across operations. Retail name unresolved."
                };
                return true;
            }

            // UPDATE_COUNTER FlagsRaw: 2 non-zero, 2 distinct (1=bit0, 32=bit5) — sparse bit-field modifier.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "UPDATE_COUNTER FlagsRaw — sparse bit-field modifier (2 non-zero records; values 1=bit0, 32=bit5).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in only 2 UPDATE_COUNTER records with 2 distinct bit values: 1(bit0) and 32(bit5). "
                        + "Likely encodes an optional counter mode or behavioral flag. Retail bit names unresolved."
                };
                return true;
            }

            // UPDATE_COUNTER Value[3]: 3 non-zero, single value = 6. Near-inactive sub-type.
            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "UPDATE_COUNTER Value[3] — near-inactive sub-type field (3 non-zero records; single value = 6).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] is non-zero in only 3 UPDATE_COUNTER records, all with value=6. Retail name unresolved."
                };
                return true;
            }

            // UPDATE_COUNTER Value[4]: 3 non-zero, single value = 100. Near-inactive percentage baseline.
            if (string.Equals(fieldKey, "Value[4]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "UPDATE_COUNTER Value[4] — near-inactive percentage baseline (3 non-zero records; single value = 100).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[4] is non-zero in only 3 UPDATE_COUNTER records, all with value=100. "
                        + "100 is consistent with a percentage baseline or scaling constant. Retail name unresolved."
                };
                return true;
            }

            // UPDATE_COUNTER Value[0]: 263 non-zero, 23 distinct (1–240 range). Counter threshold or interval.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string ucV0Dominant = BuildDominantRawValueSummary(observations, 10);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "UPDATE_COUNTER Value[0] — counter threshold or primary update parameter (263 non-zero records, 23 distinct values: 1–240).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 23 distinct values in the range 1–240. "
                        + "Regular round-number distribution (1, 2, 3, 4, 10, 15, 20, 25, 30, 35, 100, 240) suggests a stack count threshold, interval denominator, or update frequency. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(ucV0Dominant) ? string.Empty : "Observed: " + ucV0Dominant + ".")
                };
                return true;
            }

            // UPDATE_COUNTER Value[1]: 252 non-zero, 25 distinct (1–1250 range). Counter value or secondary parameter.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string ucV1Dominant = BuildDominantRawValueSummary(observations, 10);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "UPDATE_COUNTER Value[1] — counter value or secondary update parameter (252 non-zero records, 25 distinct values: 1–1250).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 25 distinct values in the range 1–1250. "
                        + "Larger values (250, 1000, 1250) relative to Value[0] suggest this may encode a step size, total count, or maximum accumulation. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(ucV1Dominant) ? string.Empty : "Observed: " + ucV1Dominant + ".")
                };
                return true;
            }

            // UPDATE_COUNTER Value[2]: 14 non-zero, 3 distinct (1, 2, 7) — sparse sub-type or mode selector.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string ucV2Dominant = BuildDominantRawValueSummary(observations, 3);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "UPDATE_COUNTER Value[2] — sparse sub-type or mode selector (14 non-zero records; values 1, 2, 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in 14 UPDATE_COUNTER records with 3 distinct values (1, 2, 7). "
                        + "Value 7 matches the sub-type-7 pattern seen in DAMAGE and STAT_CHANGE. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(ucV2Dominant) ? string.Empty : "Observed: " + ucV2Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildRecoverStandardStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 42 || observations == null || observations.Count == 0)
                return false;

            // RECOVER_STANDARD Value[1]: 6 non-zero, single value = 14. Near-inactive parameter.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "RECOVER_STANDARD Value[1] — near-inactive parameter (6 non-zero records; single value = 14).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] is non-zero in 6 RECOVER_STANDARD records, all with value=14. Likely a fixed parameter or threshold. Retail name unresolved."
                };
                return true;
            }

            // RECOVER_STANDARD Value[0]: 6 non-zero, 6 distinct — sequential high-range IDs (187701–187706). ID reference.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string rsV0Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "RECOVER_STANDARD Value[0] — sequential ID reference (6 non-zero records, 6 distinct values: 187701–187706).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] holds 6 sequential IDs in the range 187701–187706. "
                        + "Sequential high-range IDs indicate a resource, zone-object, or entity ID reference. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(rsV0Dominant) ? string.Empty : "Observed: " + rsV0Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildUnknownOp41StructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 41 || observations == null || observations.Count == 0)
                return false;

            // Unknown op 41: Value[0] = 14 (near-inactive fixed parameter); Value[1/2/3] are paired sequential ID groups.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 41 Value[0] — near-inactive fixed parameter (4 non-zero records; single value = 14).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] is non-zero in 4 op-41 records, all with value=14. Operation 41 has no confirmed retail name. Retail name unresolved."
                };
                return true;
            }

            int valueIndex;
            {
                int tmp;
                if (!TryParseIndexedField(fieldKey, "Value[", out tmp))
                    return false;
                valueIndex = tmp;
            }
            if (valueIndex < 1 || valueIndex > 3)
                return false;

            string op41Dominant = BuildDominantRawValueSummary(observations, 6);
            inference = new FieldObservationInference
            {
                SemanticSummary = "Unknown op 41 Value[" + valueIndex.ToString(CultureInfo.InvariantCulture) + "] — sequential ID reference slot (4 non-zero records, 2 distinct sequential IDs).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Value[1/2/3] together hold three consecutive IDs from the 187701–187706 range, split across record pairs. "
                    + "The sequential-triple pattern suggests these encode a correlated resource or zone-object ID group. "
                    + "Operation 41 has no confirmed retail name. "
                    + (string.IsNullOrWhiteSpace(op41Dominant) ? string.Empty : "Observed: " + op41Dominant + ".")
            };
            return true;
        }

        private static bool TryBuildUnknownOp40StructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 40 || observations == null || observations.Count == 0)
                return false;

            // Unknown op 40 Value[0]: 1 non-zero record, value=1. Near-inactive.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 40 Value[0] — near-inactive field (1 non-zero record; value = 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] is non-zero in only 1 op-40 record (value=1). Operation 40 has no confirmed retail name. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildUnknownOp43StructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 43 || observations == null || observations.Count == 0)
                return false;

            // Unknown op 43 Value[0]: 2 non-zero, 2 distinct (21, 27) — very sparse small-integer parameter.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 43 Value[0] — sparse small-integer parameter (2 non-zero records; values 21 and 27).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 2 distinct values (21, 27) in only 2 op-43 records. Operation 43 has no confirmed retail name. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildUnknownOp30StructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 30 || observations == null || observations.Count == 0)
                return false;

            // Unknown op 30 Value[0]: 1 non-zero record, value=18. Near-inactive single-record field.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 30 Value[0] — near-inactive field (1 non-zero record; value = 18).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] is non-zero in only 1 op-30 record (value=18). Operation 30 has no confirmed retail name. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildRessurrectStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 10 || observations == null || observations.Count == 0)
                return false;

            // RESSURRECT FlagsRaw: 2 non-zero, single value = 1. Single-bit binary flag.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "RESSURRECT FlagsRaw — single-bit binary flag (2 non-zero records; single value = 1 = bit0).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in only 2 RESSURRECT records, both with value=1(bit0). "
                        + "Likely a rarely-activated mode switch. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildCooldownChangeValue1Inference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 20 || observations == null || observations.Count == 0)
                return false;

            // COOLDOWN_CHANGE Value[1]: 1 non-zero, single value = 7. Near-inactive sub-type 7 pattern.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "COOLDOWN_CHANGE Value[1] — near-inactive sub-type field (1 non-zero record; single value = 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] is non-zero in only 1 COOLDOWN_CHANGE record (value=7). Matches the sub-type-7 pattern seen across operations. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildMoraleRegenChangeFlagsRawInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 18 || observations == null || observations.Count == 0)
                return false;

            // MORALE_REGEN_CHANGE FlagsRaw: 1 non-zero, single value = 1. Single-record binary flag.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MORALE_REGEN_CHANGE FlagsRaw — single-record binary flag (1 non-zero record; value = 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in only 1 MORALE_REGEN_CHANGE record (value=1). Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildHateStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 7 || observations == null || observations.Count == 0)
                return false;

            // HATE Multiplier[N]: hate scaling multiplier (26 distinct: -500, 10, 100, 1000, 1100, 1200...) — actual hate amounts.
            {
                int multiplierIndex;
                if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
                {
                    string hateMulDominant = BuildDominantRawValueSummary(observations, 8);
                    inference = new FieldObservationInference
                    {
                        SemanticSummary = "HATE " + fieldKey + " — hate-amount scaling multiplier (26 distinct values; range −500 to 1900+; 100 = baseline no change).",
                        Confidence = SemanticConfidence.Inferred,
                        Notes = "HATE Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] has 26 distinct values including large positive values (1000–1900+) indicating aggressive hate generation, "
                            + "100 (baseline), and −500 (hate reduction). Unlike passive-operation multipliers, HATE Multiplier is a primary scaling control for the hate amount. "
                            + "Retail slot-role name unresolved. "
                            + (string.IsNullOrWhiteSpace(hateMulDominant) ? string.Empty : "Observed: " + hateMulDominant + ".")
                    };
                    return true;
                }
            }

            // HATE Value[1]: 6 non-zero, 2 distinct (-1=sentinel/all-targets, 9999999=max hate marker).
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "HATE Value[1] — hate modifier sentinel field (2 distinct values: -1=all-targets or sentinel, 9999999=maximum hate marker).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has only 2 distinct values in 6 HATE records: -1 (universal sentinel meaning 'all targets' or 'override') and 9999999 (an extreme cap or forced-maximum hate value). "
                        + "Both values are non-standard sentinels rather than regular hate amounts. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildMonsterForceTargetStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 39 || observations == null || observations.Count == 0)
                return false;

            // MONSTER_FORCE_TARGET Value[0]: 6 non-zero, 3 distinct (1, 2, 3) — target mode or scope selector.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string mftV0Dominant = BuildDominantRawValueSummary(observations, 3);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MONSTER_FORCE_TARGET Value[0] — target mode or scope selector (6 non-zero records, 3 distinct values: 1, 2, 3).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 3 distinct sequential values (1, 2, 3). "
                        + "Sequential enum pattern likely encodes a force-target mode: primary, secondary, or AOE scope. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(mftV0Dominant) ? string.Empty : "Observed: " + mftV0Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildApRegenChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 17 || observations == null || observations.Count == 0)
                return false;

            // AP_REGEN_CHANGE Value[1]: 4 non-zero, single value = 7. Near-inactive sub-type 7.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "AP_REGEN_CHANGE Value[1] — near-inactive sub-type field (4 non-zero records; single value = 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] is non-zero in only 4 AP_REGEN_CHANGE records, all with value=7. "
                        + "Matches the sub-type-7 pattern seen across DAMAGE, STAT_CHANGE, MONSTER_CONTROLLER, and others. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildDefensiveStatChangeValue1Inference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 16 || observations == null || observations.Count == 0)
                return false;

            // DEFENSIVE_STAT_CHANGE Value[1]: 2 non-zero, single value = 1. Near-inactive binary flag.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DEFENSIVE_STAT_CHANGE Value[1] — near-inactive binary flag (2 non-zero records; single value = 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] is non-zero in only 2 DEFENSIVE_STAT_CHANGE records (value=1). "
                        + "Consistent with a rarely-activated mode switch. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildMoraleChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 19 || observations == null || observations.Count == 0)
                return false;

            // MORALE_CHANGE FlagsRaw: 1 non-zero, single value = 1. Single-record binary flag.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MORALE_CHANGE FlagsRaw — single-record binary flag (1 non-zero record; value = 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in only 1 MORALE_CHANGE record (value=1). Retail name unresolved."
                };
                return true;
            }

            // MORALE_CHANGE Value[1]: 1 non-zero record, value=50. Percentage parameter.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MORALE_CHANGE Value[1] — near-inactive percentage parameter (1 non-zero record; value = 50).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] is non-zero in only 1 MORALE_CHANGE record (value=50). Likely a 50% threshold or multiplier. Retail name unresolved."
                };
                return true;
            }

            // MORALE_CHANGE Value[2]: 1 non-zero record, value=7. Sub-type 7 pattern.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MORALE_CHANGE Value[2] — near-inactive sub-type field (1 non-zero record; value = 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in only 1 MORALE_CHANGE record (value=7). Matches the sub-type-7 pattern. Retail name unresolved."
                };
                return true;
            }

            // MORALE_CHANGE Value[7]: 1 non-zero record, value=75. Percentage modifier.
            if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MORALE_CHANGE Value[7] — near-inactive percentage modifier (1 non-zero record; value = 75).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[7] is non-zero in only 1 MORALE_CHANGE record (value=75). Likely a 75% percentage modifier. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildHealValue1Inference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 3 || observations == null || observations.Count == 0)
                return false;

            // HEAL Value[1]: 19 non-zero, 16 distinct (1–33 range, many multiples of 3). Likely scaling level or modifier.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string healV1Dominant = BuildDominantRawValueSummary(observations, 10);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "HEAL Value[1] — scaling level or periodic modifier (19 non-zero records, 16 distinct values in range 1–33).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 16 distinct values (1, 2, 3, 10, 12, 15, 18, 21, 24, 27, 30, 33) in 19 HEAL records. "
                        + "Values in the 12–33 range are multiples of 3, suggesting a rank-based healing level or periodic scaling step. "
                        + "Smaller values (1, 2, 3, 10) may encode sub-type or mode. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(healV1Dominant) ? string.Empty : "Observed: " + healV1Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildAutoAttackAdjustStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 33 || observations == null || observations.Count == 0)
                return false;

            // AUTO_ATTACK_ADJUST FlagsRaw: 1 non-zero, single value = 1. Single-record binary flag.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "AUTO_ATTACK_ADJUST FlagsRaw — single-record binary flag (1 non-zero record; value = 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in only 1 AUTO_ATTACK_ADJUST record (value=1). Retail name unresolved."
                };
                return true;
            }

            // AUTO_ATTACK_ADJUST Value[0]: 7 non-zero, single value = 2. Near-inactive mode selector.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "AUTO_ATTACK_ADJUST Value[0] — near-inactive mode selector (7 non-zero records; single value = 2).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] is non-zero in 7 AUTO_ATTACK_ADJUST records, all with value=2. Likely a mode or sub-type selector. Retail name unresolved."
                };
                return true;
            }

            // AUTO_ATTACK_ADJUST Value[2]: 58 non-zero, 17 distinct — mixed small integers (14-26) and large IDs (2073+).
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string aaV2Dominant = BuildDominantRawValueSummary(observations, 10);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "AUTO_ATTACK_ADJUST Value[2] — mixed parameter: small integers (14–26) or large ID references (2073+) (58 non-zero records, 17 distinct values).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] has 17 distinct values with a bimodal distribution: small integers (14, 15, 23, 26) and large values (2073, 2182–2184, 2293, 2549–2551). "
                        + "Small values may encode a sub-type, modifier index, or attack-speed parameter. "
                        + "Large values (2000+) are in the range of ability or resource ID references. "
                        + "The dual-range pattern suggests this field may encode different semantic roles depending on a companion FlagsRaw or context value. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(aaV2Dominant) ? string.Empty : "Observed: " + aaV2Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildStealthStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 34 || observations == null || observations.Count == 0)
                return false;

            // STEALTH FlagsRaw: 1 non-zero, single value = 2 (bit1). Single-bit behavioral flag.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "STEALTH FlagsRaw — single-bit behavioral flag (1 non-zero record; value = 2 = bit1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in only 1 STEALTH record (value=2=bit1). Retail name unresolved."
                };
                return true;
            }

            // STEALTH Value[0]: 6 non-zero, 5 distinct (50, 60, 80, 100, 150) — stealth level or visibility percentage.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string stV0Dominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "STEALTH Value[0] — stealth level or visibility percentage (6 non-zero records, 5 distinct values: 50, 60, 80, 100, 150).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 5 distinct values (50, 60, 80, 100, 150) that read as percentage levels. "
                        + "100 = baseline (full stealth), below-100 = partial visibility, 150 = enhanced stealth. "
                        + "Likely encodes the stealth strength or detection-range multiplier. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(stV0Dominant) ? string.Empty : "Observed: " + stV0Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildUnknownOp47StructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 47 || observations == null || observations.Count == 0)
                return false;

            // Unknown op 47 Value[0]: 4 non-zero, single value = 1. Near-inactive binary flag.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 47 Value[0] — near-inactive binary flag (4 non-zero records; single value = 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] is non-zero in 4 op-47 records, all with value=1. Operation 47 has no confirmed retail name. Retail name unresolved."
                };
                return true;
            }

            // Unknown op 47 Value[1]: 8 non-zero, 4 distinct (10, 20, 30, 40) — regular 10-unit increment, quantity or percentage.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string op47V1Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 47 Value[1] — regular increment parameter (8 non-zero records, 4 distinct values: 10, 20, 30, 40).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 4 distinct values in a regular 10-unit step sequence (10, 20, 30, 40). "
                        + "Consistent with a quantity level, percentage increment, or tier selector. "
                        + "Operation 47 has no confirmed retail name. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(op47V1Dominant) ? string.Empty : "Observed: " + op47V1Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildUnknownOp51StructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 51 || observations == null || observations.Count == 0)
                return false;

            // Op 51 replaces the actor's career action set. Component 26389 carries the only
            // self-describing text in the whole operation, and it states the behaviour outright:
            // "Manifesting an Aspect of Fire. Normal career abilities have been replaced with
            // those of the Aspect's." (Londos War_AbilityComponentBin, Operation=51.) The other
            // 38 rows are the Skaven Play-as-Monster controls and two further form families.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string op51V0Dominant = BuildDominantRawValueSummary(observations, 10);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Op 51 Value[0] — replacement action-set reference (39 non-zero records, 38 distinct values: 17643–30030).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Operation 51 replaces the actor's career action set; component 26389's own description says so "
                        + "(\"Normal career abilities have been replaced with those of the Aspect's\"). Value[0] is the reference to "
                        + "the replacement set. Its domain is NOT abilityexport.bin ability IDs, despite the numeric overlap: "
                        + "components 26661-26668 use 20760-20767, which land inside a developer block in abilitynames.txt "
                        + "(\"; TEST Skaven - Grey Seer\" plus its seven spells), and the Order Warlock Engineer control resolves "
                        + "to the Grey Seer header. Components 26900-26923 use 30030-30069, past the end of abilitynames.txt at "
                        + "29000. Components 27992 and 27993 share Value[0]=17643 while differing in Value[3], so Value[0] and "
                        + "Value[3] are independent fields, not one derived from the other. "
                        + "Ruled out for the paired Value[3]: careerlines_m.txt (0-24), careernames_m.txt (rejected as a career "
                        + "identity by the domain ledger), Londos Career (IDs 130-154) and Londos CareerType (20-27, 60-67, 100-107). "
                        + (string.IsNullOrWhiteSpace(op51V0Dominant) ? string.Empty : "Observed: " + op51V0Dominant + ".")
                };
                return true;
            }

            // Unknown op 51 FlagsRaw: 32 non-zero, single value = 4 (bit2). Single-bit activation flag.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Op 51 FlagsRaw — single-bit flag set on the indefinite form families (32 non-zero records; single value = 4 = bit2).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw has exactly one non-zero value (4=bit2) across all op-51 records where it appears. "
                        + "Bit2 implies Duration 0: all 32 records carrying it are untimed, and both timed records (component "
                        + "1597 at 30000 ms and 26389 at 120000 ms) leave it clear. The implication does not reverse - "
                        + "components 26941-26943 and 27992/27993 are untimed with the bit clear - so bit2 distinguishes a "
                        + "subset of the untimed forms rather than simply meaning \"no timer\". Retail bit name unresolved."
                };
                return true;
            }

            // Unknown op 51 Value[2]: 30 non-zero, 4 distinct (1, 2, 3, 4) — small sequential enum.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string op51V2Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Op 51 Value[2] — form slot within the replacement set (30 non-zero records, 4 distinct values: 1, 2, 3, 4).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] selects which form of a set is being applied, and it is realm-invariant. The Skaven "
                        + "Play-as-Monster controls prove it: abilityexport.bin gives 24857-24860 (Order Controlled Warlock "
                        + "Engineer / Gutter Runner / Rat Ogre / Pack Master) components 26661-26664 and 24861-24864 (the "
                        + "Destruction four) components 26665-26668, and the (Value[2], Value[3]) pairs repeat exactly across "
                        + "the two realm blocks: Gutter Runner (0, 41), Warlock Engineer (1, 42), Rat Ogre (2, 40), Pack Master "
                        + "(3, 39). Components 26900-26923 repeat the same shape as six further groups cycling 1, 2, 3, 4. "
                        + "Value[2] is therefore a slot index inside a set, not a global identity. "
                        + (string.IsNullOrWhiteSpace(op51V2Dominant) ? string.Empty : "Observed: " + op51V2Dominant + ".")
                };
                return true;
            }

            // Unknown op 51 Value[3]: 38 non-zero, 34 distinct (13–58 range) — index or ordinal reference.
            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                string op51V3Dominant = BuildDominantRawValueSummary(observations, 10);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Op 51 Value[3] — form identity paired one-to-one with Value[2] (38 non-zero records, 34 distinct values: 13–91).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] identifies the specific form and is realm-invariant: the Skaven controls pair it with "
                        + "Value[2] as (0, 41), (1, 42), (2, 40), (3, 39) in both the Order and Destruction blocks. Within the "
                        + "26900-26923 family Value[0] minus Value[3] is a constant 29981, and within 26941-26943 it is a "
                        + "constant 30929, so the two fields are allocated in step there; components 27992 and 27993 break that "
                        + "by sharing Value[0]=17643 with Value[3] of 13 and 14, which is what shows the fields are independent. "
                        + "Domain unresolved, but four candidates are excluded by range: careerlines_m.txt is 0-24, Londos Career "
                        + "is 130-154, Londos CareerType is 20-27/60-67/100-107, and careernames_m.txt is rejected as a career "
                        + "identity by this tool's own domain ledger. Londos AbilityLine contains 41 and 42 but neither 39 nor 40. "
                        + (string.IsNullOrWhiteSpace(op51V3Dominant) ? string.Empty : "Observed: " + op51V3Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildUnknownOp29StructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 29 || observations == null || observations.Count == 0)
                return false;

            // Unknown op 29 Value[1]: 10 non-zero, 2 distinct (11, 40) — sparse secondary parameter.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 29 Value[1] — sparse secondary parameter (10 non-zero records; values 11 and 40).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 2 distinct values (11, 40) in 10 op-29 records. "
                        + "Small integer values suggest a mode parameter or sub-type index. Operation 29 has no confirmed retail name."
                };
                return true;
            }

            // Unknown op 29 Value[2]: 5 non-zero, 2 distinct (7, 10) — sparse sub-type or mode parameter.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 29 Value[2] — sparse sub-type parameter (5 non-zero records; values 7 and 10).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] has 2 distinct values (7, 10) in 5 op-29 records. "
                        + "Value 7 matches the sub-type-7 pattern. Operation 29 has no confirmed retail name."
                };
                return true;
            }

            // Unknown op 29 FlagsRaw: 9 non-zero, 3 distinct (1, 7, 1143) — bit-field or enum.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string op29FrDominant = BuildDominantRawValueSummary(observations, 3);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 29 FlagsRaw — bit-field or enum (3 distinct values: 1, 7, 1143).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 9 op-29 records with 3 distinct values: 1(bit0), 7(bits0-2), 1143(multiple bits). "
                        + "Value 1143 = 0x477 = bits0+1+2+6+7+10. Mixed bit pattern suggests a mode or behavioral flag set. "
                        + "Operation 29 has no confirmed retail name. "
                        + (string.IsNullOrWhiteSpace(op29FrDominant) ? string.Empty : "Observed: " + op29FrDominant + ".")
                };
                return true;
            }

            // Unknown op 29 Value[0]: 11 non-zero, 8 distinct (10, 9979, 66001, 66298-66300, 192555, 208470) — likely ID reference.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string op29V0Dominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 29 Value[0] — ID reference (11 non-zero records, 8 distinct high-range values: 10, 9979, 66001–66300, 192555, 208470).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 8 distinct values spanning a wide ID range. "
                        + "High-range values (66001–66300, 192555, 208470) are consistent with an asset, zone, or creature ID reference. "
                        + "Operation 29 itself has no confirmed retail name. "
                        + (string.IsNullOrWhiteSpace(op29V0Dominant) ? string.Empty : "Observed: " + op29V0Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildArmorChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 5 || observations == null || observations.Count == 0)
                return false;

            // ARMOR_CHANGE Multiplier[N]: percentage scaling multiplier for the armor-change effect (4 distinct: 1, 10, 100, 400).
            {
                int multiplierIndex;
                if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
                {
                    string acMulDominant = BuildDominantRawValueSummary(observations, 6);
                    inference = new FieldObservationInference
                    {
                        SemanticSummary = "ARMOR_CHANGE " + fieldKey + " — percentage scaling multiplier for this armor-change component (100 = no change; 1/10/400 are active scaling values).",
                        Confidence = SemanticConfidence.Inferred,
                        Notes = "ARMOR_CHANGE Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] is a percentage multiplier applied to the armor-change formula. "
                            + "Observed distinct values: 1(near-zero), 10(10%), 100(baseline), 400(4× scaling). "
                            + "Mirrors the Multiplier[N] pattern in DAMAGE, STAT_CHANGE, and HEAL. Retail slot-role name unresolved. "
                            + (string.IsNullOrWhiteSpace(acMulDominant) ? string.Empty : "Observed: " + acMulDominant + ".")
                    };
                    return true;
                }
            }

            // ARMOR_CHANGE Value[2]: 6 non-zero, single value = 7. Near-inactive sub-type 7 (matches DAMAGE/STAT_CHANGE pattern).
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "ARMOR_CHANGE Value[2] — near-inactive sub-type field (6 non-zero records; single value = 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in only 6 ARMOR_CHANGE records, all with value=7. "
                        + "Matches the sub-type-7 pattern seen in DAMAGE Value[2]/[3], STAT_CHANGE Value[2], and MONSTER_CONTROLLER Value[7]. Retail name unresolved."
                };
                return true;
            }

            // ARMOR_CHANGE Value[1]: 16 non-zero, 3 distinct (1, 2, 7) — sub-type or mode selector.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string acV1Dominant = BuildDominantRawValueSummary(observations, 3);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "ARMOR_CHANGE Value[1] — sub-type or mode selector (16 non-zero records, 3 distinct values: 1, 2, 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 3 distinct values: 1, 2, and 7. "
                        + "Value 7 matches the sub-type-7 pattern seen in DAMAGE, STAT_CHANGE, and HEAL. "
                        + "Likely encodes an armor-change sub-type or application mode. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(acV1Dominant) ? string.Empty : "Observed: " + acV1Dominant + ".")
                };
                return true;
            }

            // ARMOR_CHANGE FlagsRaw: 51 non-zero, 5 distinct (16, 64, 32768, 65536, 114688).
            // All values are power-of-two or grouped bit combinations outside the CC bit range.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string acFrDominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "ARMOR_CHANGE FlagsRaw — bit-field encoding armor-change modifier flags (5 distinct values: 16, 64, 32768, 65536, 114688).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 51 ARMOR_CHANGE records with 5 distinct values: "
                        + "16(bit4), 64(bit6), 32768(bit15), 65536(bit16), 114688(bits14-16=0x1C000). "
                        + "Values are all power-of-two or grouped high-bit combinations, not in the CC bit range. "
                        + "Likely encodes armor type, penetration mode, or mitigation-override flags. Retail bit names unresolved. "
                        + (string.IsNullOrWhiteSpace(acFrDominant) ? string.Empty : "Observed: " + acFrDominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildCatapultStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 46 || observations == null || observations.Count == 0)
                return false;

            // CATAPULT Multiplier[N]: percentage scaling multiplier (2 distinct: 75, 100).
            {
                int multiplierIndex;
                if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
                {
                    string catMulDominant = BuildDominantRawValueSummary(observations, 4);
                    inference = new FieldObservationInference
                    {
                        SemanticSummary = "CATAPULT " + fieldKey + " — percentage scaling multiplier (2 distinct values: 75=75% launch power, 100=full power).",
                        Confidence = SemanticConfidence.Inferred,
                        Notes = "CATAPULT Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] has 2 distinct values: 100 (baseline, no change) and 75 (75% of nominal launch force). "
                            + "Mirrors the Multiplier[N] scaling pattern seen across other operations. Retail slot-role name unresolved. "
                            + (string.IsNullOrWhiteSpace(catMulDominant) ? string.Empty : "Observed: " + catMulDominant + ".")
                    };
                    return true;
                }
            }

            // CATAPULT FlagsRaw: 8 non-zero, 3 distinct (1, 2, 3) — small sequential enum or mode selector.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string catFrDominant = BuildDominantRawValueSummary(observations, 3);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CATAPULT FlagsRaw — sequential enum or mode selector (3 distinct values: 1, 2, 3).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw has 3 distinct sequential values (1, 2, 3) in 8 CATAPULT records. "
                        + "Sequential values without gaps suggest an enum (launch type, targeting mode) rather than a true bit-field. "
                        + "Retail names unresolved. "
                        + (string.IsNullOrWhiteSpace(catFrDominant) ? string.Empty : "Observed: " + catFrDominant + ".")
                };
                return true;
            }

            // CATAPULT Value[0]: 20 non-zero, 3 distinct (1, 2, 3) — launch type or target mode enum.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string catV0Dominant = BuildDominantRawValueSummary(observations, 3);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CATAPULT Value[0] — launch type or target mode enum (20 non-zero records, 3 distinct values: 1, 2, 3).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 3 distinct sequential values (1, 2, 3). "
                        + "Matches the FlagsRaw enum range, suggesting a correlated launch type or mode selector. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(catV0Dominant) ? string.Empty : "Observed: " + catV0Dominant + ".")
                };
                return true;
            }

            // CATAPULT Value[1]: 20 non-zero, 8 distinct (1, 100, 150, 200, 240, 300, 500, 2000) — launch parameter or percentage.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string catV1Dominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CATAPULT Value[1] — launch parameter or scaling factor (20 non-zero records, 8 distinct values: 1, 100, 150, 200, 240, 300, 500, 2000).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 20 non-zero records with 8 distinct values. "
                        + "The distribution includes percentage-like values (100, 150, 200, 300) and raw integers (1, 240, 500, 2000). "
                        + "Likely encodes a launch velocity, range scalar, or arc-height parameter. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(catV1Dominant) ? string.Empty : "Observed: " + catV1Dominant + ".")
                };
                return true;
            }

            // CATAPULT Value[2]: 2 non-zero, single value = 7. Near-inactive sub-type 7 pattern.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CATAPULT Value[2] — near-inactive sub-type field (2 non-zero records; single value = 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in only 2 CATAPULT records (value=7). Matches the sub-type-7 pattern seen across operations. Retail name unresolved."
                };
                return true;
            }

            // CATAPULT Value[3]: 12 non-zero, 7 distinct (12, 20, 30, 50, 60, 180, 500) — angular or range parameter.
            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                string catV3Dominant = BuildDominantRawValueSummary(observations, 7);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CATAPULT Value[3] — angular or range parameter (12 non-zero records, 7 distinct values: 12, 20, 30, 50, 60, 180, 500).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] has 12 non-zero records with 7 distinct values. "
                        + "Values include angular multiples (12, 20, 30, 60, 180) and larger integers (50, 500) suggesting an arc angle in degrees, a radius, or a knockback range. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(catV3Dominant) ? string.Empty : "Observed: " + catV3Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildMoraleRegenChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 18 || observations == null || observations.Count == 0)
                return false;

            // MORALE_REGEN_CHANGE ExtData Val1: 2 non-zero records, 2 distinct (2=target, 3=tertiary scope).
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 1)
                return false;

            string mrDominant = BuildDominantRawValueSummary(observations, 4);
            inference = new FieldObservationInference
            {
                SemanticSummary = "MORALE_REGEN_CHANGE ExtData Val1 — application scope selector (2=target, 3=tertiary scope variant).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "ExtData Val1 has 2 distinct values across MORALE_REGEN_CHANGE records: 2=target-scope (universal), 3=tertiary scope variant. "
                    + "Only 2 records have non-zero Val1. Retail name for Val1=3 is unresolved. "
                    + (string.IsNullOrWhiteSpace(mrDominant) ? string.Empty : "Observed: " + mrDominant + ".")
            };
            return true;
        }

        private static bool TryBuildInterruptStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 9 || observations == null || observations.Count == 0)
                return false;

            // INTERRUPT Value[0]: 47 non-zero, 5 distinct (100, 1000, 3000, 4000, 10000) — range or magnitude in fixed units.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string intV0Dominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "INTERRUPT Value[0] — range or magnitude parameter (47 non-zero records, 5 distinct values: 100, 1000, 3000, 4000, 10000).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 5 distinct values: 100, 1000, 3000, 4000, 10000. "
                        + "Regular order-of-magnitude distribution suggests a distance in fixed-point units, an area radius, or an interrupt-power threshold. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(intV0Dominant) ? string.Empty : "Observed: " + intV0Dominant + ".")
                };
                return true;
            }

            // INTERRUPT Value[2]: 1 non-zero record, value=1. Near-inactive binary flag.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "INTERRUPT Value[2] — near-inactive binary flag (1 non-zero record; value = 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in only 1 INTERRUPT record (value=1). Likely a rarely-activated mode switch. Retail name unresolved."
                };
                return true;
            }

            // INTERRUPT Value[1]: 45 non-zero, 7 distinct (250, 500, 1000, 2000, 3000, 4000, 5000) — duration in ms.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string intV1Dominant = BuildDominantRawValueSummary(observations, 7);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "INTERRUPT Value[1] — interrupt duration or lockout period in milliseconds (45 non-zero records, 7 distinct values: 250–5000 ms).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 7 distinct values (250, 500, 1000, 2000, 3000, 4000, 5000) that form a regular progression in multiples of 250 or 1000 ms. "
                        + "The regular step pattern strongly suggests a duration in milliseconds (0.25 s – 5 s). "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(intV1Dominant) ? string.Empty : "Observed: " + intV1Dominant + ".")
                };
                return true;
            }

            // INTERRUPT FlagsRaw: 70 non-zero, 9 distinct values (1, 27, 29, 31, 32, 47, 51, 63, 127) — CrowdControlTypes bitmask.
            // 1=Snare, 27=Knockdown+Silence+Root+Snare, 29=Knockdown+Silence+Disarm+Snare,
            // 31=Knockdown+Silence+Disarm+Root+Snare, 32=Stagger, 47=Stagger+Silence+Disarm+Root+Snare,
            // 51=Stagger+Knockdown+Root+Snare, 63=Stagger+Knockdown+Silence+Disarm+Root+Snare,
            // 127=bit6+Stagger+Knockdown+Silence+Disarm+Root+Snare.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string intFrDominant = BuildDominantRawValueSummary(observations, 9);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "INTERRUPT FlagsRaw — CrowdControlTypes bitmask gate (9 distinct values, all confirmed as CC bit combinations).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 70 INTERRUPT records with 9 distinct values. "
                        + "All values decode as CrowdControlTypes bit combinations: "
                        + "1=Snare, 32=Stagger, 27=Knockdown+Silence+Root+Snare, 29=Knockdown+Silence+Disarm+Snare, "
                        + "31=Knockdown+Silence+Disarm+Root+Snare, 47=Stagger+Silence+Disarm+Root+Snare, "
                        + "51=Stagger+Knockdown+Root+Snare, 63=Stagger+Knockdown+Silence+Disarm+Root+Snare, "
                        + "127=bit6+Stagger+Knockdown+Silence+Disarm+Root+Snare. "
                        + "Mirrors the CC gate pattern in DAMAGE Value15 and BTA Value15. "
                        + (string.IsNullOrWhiteSpace(intFrDominant) ? string.Empty : "Observed: " + intFrDominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildRankChangeStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 37 || observations == null || observations.Count == 0)
                return false;

            // RANK_CHANGE Value[1]: 6 non-zero, 5 distinct (8, 9, 10, 11, 12) — sequential rank levels, secondary threshold.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string rcV1Dominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "RANK_CHANGE Value[1] — secondary rank level threshold (6 non-zero records, 5 distinct sequential values: 8–12).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 5 sequential distinct values (8, 9, 10, 11, 12) — a contiguous rank range within WAR career levels. "
                        + "Likely encodes a secondary rank threshold or sub-range modifier. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(rcV1Dominant) ? string.Empty : "Observed: " + rcV1Dominant + ".")
                };
                return true;
            }

            // RANK_CHANGE Value[0]: 22 non-zero, 12 distinct values (8, 11, 15, 23, 26, 39, 40, 41, 42, 43, 44, 45) — rank level threshold.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string rcV0Dominant = BuildDominantRawValueSummary(observations, 12);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "RANK_CHANGE Value[0] — rank level threshold or target rank (22 non-zero records, 12 distinct values: 8, 11, 15, 23, 26, 39–45).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 12 distinct values in the range 8–45. "
                        + "These values align with WAR career rank levels (1–40) and renown rank thresholds. "
                        + "Likely encodes the target rank, minimum rank threshold, or rank-change delta for this component. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(rcV0Dominant) ? string.Empty : "Observed: " + rcV0Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildUnknownOp32StructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 32 || observations == null || observations.Count == 0)
                return false;

            // Unknown op 32 Value[1]: 4 non-zero, 2 distinct (65, 100) — sparse percentage-like values.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 32 Value[1] — sparse percentage-like parameter (4 non-zero records; values 65 and 100).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] has 2 distinct values (65, 100) in 4 op-32 records. "
                        + "Both values are percentage-like (65% and 100% = baseline). Operation 32 has no confirmed retail name."
                };
                return true;
            }

            // Unknown op 32 Value[0]: 15 non-zero, 13 distinct — high-range ID values (5424–10020). Likely ID reference.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string op32V0Dominant = BuildDominantRawValueSummary(observations, 12);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "Unknown op 32 Value[0] — ID reference (15 non-zero records, 13 distinct high-range values: 5424–10020).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] has 13 distinct values in the high-ID range 5424–10020. "
                        + "High-cardinality values in this range are consistent with an ability, effect, or resource ID reference. "
                        + "Operation 32 itself has no confirmed retail name; the field role is inferred from the ID-range pattern. "
                        + (string.IsNullOrWhiteSpace(op32V0Dominant) ? string.Empty : "Observed: " + op32V0Dominant + ".")
                };
                return true;
            }

            return false;
        }

        // Operations where ExtData Val1 has exactly 3 scopes: 1=self, 2=target, 3=tertiary.
        // All confirmed from extracted BIN data.
        private static readonly HashSet<uint> _threeWayScopeVal1Operations = new HashSet<uint> { 16, 20, 21 };

        private static bool TryBuildThreeWayScopeVal1Inference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (!_threeWayScopeVal1Operations.Contains(operationId) || observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 1)
                return false;

            string opName = operationId == 16 ? "DEFENSIVE_STAT_CHANGE"
                : operationId == 20 ? "COOLDOWN_CHANGE"
                : "CASTTIME_CHANGE";

            string dominant = BuildDominantRawValueSummary(observations, 5);
            inference = new FieldObservationInference
            {
                SemanticSummary = opName + " ExtData Val1 — application scope selector (1=self, 2=target, 3=tertiary scope).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "ExtData Val1 has exactly 3 distinct values across " + opName + " records: 1=self/caster, 2=target/enemy, 3=tertiary scope variant. "
                    + "Matches the universal Val1 1/2 pattern plus a third scope value seen in VELOCITY, EVENT_LISTENER, and other modifier operations. "
                    + "Retail name for Val1=3 is unresolved. "
                    + (string.IsNullOrWhiteSpace(dominant) ? string.Empty : "Observed: " + dominant + ".")
            };
            return true;
        }

        private static bool TryBuildDispelBuffStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 11 || observations == null || observations.Count == 0)
                return false;

            // DISPEL_BUFF FlagsRaw: 3 distinct values (1, 16, 18=16+2). Bit-field combinations.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string dbFrDominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DISPEL_BUFF FlagsRaw — bit-field (3 distinct values: 1=bit0, 16=bit4, 18=bits1+4).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw has 3 distinct values: 1 (bit0), 16 (bit4), 18 (bit1+bit4). "
                        + "Likely encodes dispel targeting or effect-type restrictions. "
                        + "Retail bit names unresolved. "
                        + (string.IsNullOrWhiteSpace(dbFrDominant) ? string.Empty : "Observed: " + dbFrDominant + ".")
                };
                return true;
            }

            // DISPEL_BUFF Value08: 6 non-zero, single value = 1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DISPEL_BUFF Value08 — binary activation flag (value=1 in 6 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all DISPEL_BUFF records where it appears. "
                        + "Functions as a binary toggle or mode flag, consistent with the Value08=1 pattern across operations. Retail name unresolved."
                };
                return true;
            }

            // DISPEL_BUFF Value15: 12 non-zero, 2 distinct (1=Snare, 4=Disarm). CrowdControlTypes bits.
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "DISPEL_BUFF Value15 — CrowdControlTypes gate flag (2 distinct values: 1=Snare, 4=Disarm).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding: 1=Snare(bit0), 4=Disarm(bit2). "
                        + "Only 12 DISPEL_BUFF records have non-zero Value15. 0 = no CC gate."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildHealStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 3 || observations == null || observations.Count == 0)
                return false;

            // HEAL Value15: 269 non-zero, 3 distinct values (1=Snare, 4=Disarm, 5=Snare+Disarm). CrowdControlTypes bits.
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string healV15Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "HEAL Value15 — CrowdControlTypes gate flag (3 distinct values: 1=Snare, 4=Disarm, 5=Snare+Disarm).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding to gate or condition the heal effect. "
                        + "Observed: 1=Snare(bit0), 4=Disarm(bit2), 5=Snare+Disarm(bits0+2). 0 = no CC gate. "
                        + (string.IsNullOrWhiteSpace(healV15Dominant) ? string.Empty : "Observed: " + healV15Dominant + ".")
                };
                return true;
            }

            // HEAL Value08: 10 non-zero, single value = 1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "HEAL Value08 — binary activation flag (value=1 in 10 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all HEAL records where it appears. "
                        + "Consistent with the Value08=1 binary-flag pattern across operations. Retail name unresolved."
                };
                return true;
            }

            // HEAL Multiplier[N]: 750 non-zero, 77 distinct — varied percentage multiplier (same role as DAMAGE/STAT_CHANGE Multiplier[N]).
            {
                int multiplierIndex;
                if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
                {
                    string healMulDominant = BuildDominantRawValueSummary(observations, 8);
                    inference = new FieldObservationInference
                    {
                        SemanticSummary = "HEAL " + fieldKey + " — percentage scaling multiplier for this heal component (100 = no change; varied values indicate active heal scaling).",
                        Confidence = SemanticConfidence.Inferred,
                        Notes = "HEAL Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] is a percentage multiplier applied to the heal formula. "
                            + "Unlike passive-operation multipliers which are universally 100, HEAL multipliers carry meaningful non-100 values (77 distinct values). "
                            + "100 = no scaling change; values above/below 100 scale the heal up/down. Retail slot-role name unresolved. "
                            + (string.IsNullOrWhiteSpace(healMulDominant) ? string.Empty : "Observed: " + healMulDominant + ".")
                    };
                    return true;
                }
            }

            // HEAL Value[3]: 1 non-zero record, value=100. Near-inactive percentage cap or baseline.
            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "HEAL Value[3] — near-inactive field (1 non-zero record; value = 100).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] is non-zero in only 1 HEAL record (value=100). Likely a 100% percentage baseline or cap. Retail name unresolved."
                };
                return true;
            }

            // HEAL Value[7]: 1 non-zero record, value=7. Near-inactive sub-type indicator.
            if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "HEAL Value[7] — near-inactive field (1 non-zero record; value = 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[7] is non-zero in only 1 HEAL record (value=7). "
                        + "Value 7 matches the dominant sub-type-7 pattern seen in DAMAGE and STAT_CHANGE. Retail name unresolved."
                };
                return true;
            }

            // HEAL Value[2]: 4 non-zero, 3 distinct (2, 40, 100) — sparse secondary heal parameter.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string healV2Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "HEAL Value[2] — sparse secondary heal parameter (4 non-zero records; values 2, 40, 100).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] is non-zero in only 4 HEAL records with 3 distinct values (2, 40, 100). "
                        + "Value 100 suggests a percentage cap or scaling constant; 2 and 40 may encode a sub-type or conditional threshold. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(healV2Dominant) ? string.Empty : "Observed: " + healV2Dominant + ".")
                };
                return true;
            }

            // HEAL FlagsRaw: 335 non-zero, 7 distinct values (1–7 sequential) — heal-type or sub-mode enum.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string healFrDominant = BuildDominantRawValueSummary(observations, 7);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "HEAL FlagsRaw — heal-type or sub-mode selector (7 distinct sequential values: 1–7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 335 HEAL records with exactly 7 distinct values (1, 2, 3, 4, 5, 6, 7). "
                        + "Sequential values without gaps suggest this is an enum rather than a bit-field. "
                        + "Likely encodes heal type, source, or delivery mode. Retail names unresolved. "
                        + (string.IsNullOrWhiteSpace(healFrDominant) ? string.Empty : "Observed: " + healFrDominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildVelocityStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 8 || observations == null || observations.Count == 0)
                return false;

            // VELOCITY Value08: 47 non-zero, single value = 1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "VELOCITY Value08 — binary activation flag (value=1 in 47 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all VELOCITY records where it appears. "
                        + "Consistent with the Value08=1 binary-flag pattern across operations. Retail name unresolved."
                };
                return true;
            }

            // VELOCITY Value[1]: 2 non-zero, 2 distinct (-1, 1) — sparse directional or sign flag.
            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "VELOCITY Value[1] — sparse directional or sign flag (2 non-zero records; values -1 and 1).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] is non-zero in only 2 VELOCITY records with opposite-sign values (-1, 1). "
                        + "Likely encodes a directional modifier or sign/polarity flag for the velocity effect. Retail name unresolved."
                };
                return true;
            }

            // VELOCITY Value15: 340 non-zero, 3 distinct values (4=Disarm, 5=Snare+Disarm, 20=Knockdown+Disarm). CrowdControlTypes.
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string velV15Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "VELOCITY Value15 — CrowdControlTypes gate flag (3 distinct values: 4=Disarm, 5=Snare+Disarm, 20=Knockdown+Disarm).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 uses CrowdControlTypes bit encoding: 4=Disarm(bit2), 5=Snare(bit0)+Disarm(bit2), 20=Knockdown(bit4)+Disarm(bit2). "
                        + "0 = no CC gate (inactive). "
                        + (string.IsNullOrWhiteSpace(velV15Dominant) ? string.Empty : "Observed: " + velV15Dominant + ".")
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 1)
                return false;

            // VELOCITY ExtData Val1: 3 distinct values (1=self, 2=target, 3=area/scope).
            string velVal1Dominant = BuildDominantRawValueSummary(observations, 5);
            inference = new FieldObservationInference
            {
                SemanticSummary = "VELOCITY ExtData Val1 — application scope selector (1=self, 2=target, 3=area-or-scope).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "VELOCITY Val1 has exactly 3 distinct values across ExtData slots. "
                    + "Val1=1 = self-scope (universal), Val1=2 = target-scope (universal), Val1=3 = third scope variant (area or broadcast). "
                    + "Retail name for Val1=3 is unresolved. "
                    + (string.IsNullOrWhiteSpace(velVal1Dominant) ? string.Empty : "Observed: " + velVal1Dominant + ".")
            };
            return true;
        }

        private static bool TryBuildMonsterControllerStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 26 || observations == null || observations.Count == 0)
                return false;

            // MONSTER_CONTROLLER Value[2]: 211 non-zero, 13 distinct timing/speed values.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string mcV2Dominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MONSTER_CONTROLLER Value[2] — timing or speed parameter (211 non-zero records, 13 distinct values).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] has 13 distinct values (6, 7, 10, 33, 35, 45, 60, 75, 100, 120, 200, 240). "
                        + "Value distribution suggests a timing interval (ms), a speed coefficient, or a radius/range parameter. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(mcV2Dominant) ? string.Empty : "Observed: " + mcV2Dominant + ".")
                };
                return true;
            }

            // MONSTER_CONTROLLER Value[3]: 217 non-zero, 19 distinct angle/interval values.
            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                string mcV3Dominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MONSTER_CONTROLLER Value[3] — angular or interval parameter (217 non-zero records, 19 distinct values).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[3] has 19 distinct values including 120, 135, 144, 160, 180, 216, 220, 230, 240, 270, 288. "
                        + "Distribution near multiples of common angular increments suggests a facing angle (degrees), sweep arc, or time interval. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(mcV3Dominant) ? string.Empty : "Observed: " + mcV3Dominant + ".")
                };
                return true;
            }

            // MONSTER_CONTROLLER FlagsRaw: 295 non-zero, 16 distinct values — likely bit-field.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string mcFrDominant = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MONSTER_CONTROLLER FlagsRaw — behavioral bit-field (16 distinct values; combinations of bits 0–5).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 295 MONSTER_CONTROLLER records with 16 distinct values. "
                        + "Sample values (1, 2, 3, 4, 12, 14, 33, 34, 35, 36, 40, 46) appear to be bit-flag combinations. "
                        + "Exact bit semantics are unresolved. "
                        + (string.IsNullOrWhiteSpace(mcFrDominant) ? string.Empty : "Observed: " + mcFrDominant + ".")
                };
                return true;
            }

            // MONSTER_CONTROLLER Value08: 147 non-zero, single value = 1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MONSTER_CONTROLLER Value08 — binary activation flag (value=1 in 147 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across all MONSTER_CONTROLLER records where it appears. "
                        + "Consistent with the Value08=1 binary-flag pattern across operations. Retail name unresolved."
                };
                return true;
            }

            // MONSTER_CONTROLLER Value[7]: 6 non-zero, single value = 7. Near-inactive sub-type indicator.
            if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "MONSTER_CONTROLLER Value[7] — near-inactive sub-type field (6 non-zero records; single value = 7).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[7] is non-zero in only 6 MONSTER_CONTROLLER records, all with value=7. "
                        + "Value 7 matches the sub-type-7 pattern seen across DAMAGE, STAT_CHANGE, and other operations. Retail name unresolved."
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildGrantedAbilityStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 28 || observations == null || observations.Count == 0)
                return false;

            // GRANTED_ABILITY Value[0]: 249 non-zero records, 239 distinct values — referenced ability ID.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string gaV0Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "GRANTED_ABILITY Value[0] — referenced ability ID (249 non-zero records, 239 distinct values).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] holds the ability ID granted by this GRANTED_ABILITY component. "
                        + "Distribution (249 non-zero, 239 distinct) mirrors APPLY_ABILITY Value[0] ability-reference pattern. "
                        + (string.IsNullOrWhiteSpace(gaV0Dominant) ? string.Empty : "Sample values: " + gaV0Dominant + ".")
                };
                return true;
            }

            // GRANTED_ABILITY FlagsRaw: 98 non-zero, 2 distinct (2=bit1, 4=bit2) — bit-field modifier flags.
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string gaFrDominant = BuildDominantRawValueSummary(observations, 2);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "GRANTED_ABILITY FlagsRaw — bit-field modifier flags (2 distinct values: 2=bit1, 4=bit2).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is non-zero in 98 GRANTED_ABILITY records with 2 distinct values: 2(bit1) and 4(bit2). "
                        + "Likely encodes granted-ability delivery mode or activation flags. Retail bit names unresolved. "
                        + (string.IsNullOrWhiteSpace(gaFrDominant) ? string.Empty : "Observed: " + gaFrDominant + ".")
                };
                return true;
            }

            // GRANTED_ABILITY Value[2]: 60 non-zero, 2 distinct (2=target, 3=tertiary scope) — application scope selector.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string gaV2Dominant = BuildDominantRawValueSummary(observations, 3);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "GRANTED_ABILITY Value[2] — application scope selector (60 non-zero records; 2=target, 3=tertiary scope).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] has 2 distinct values: 2 (target/enemy scope) and 3 (tertiary scope variant). "
                        + "Both values align with the universal Val1 scope encoding pattern seen across operations. "
                        + "Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(gaV2Dominant) ? string.Empty : "Observed: " + gaV2Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildSummonMountStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 35 || observations == null || observations.Count == 0)
                return false;

            // SUMMON_MOUNT Value08: 231 non-zero, single value = 1. Binary activation flag.
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SUMMON_MOUNT Value08 — binary activation flag (value=1 in 231 records; only one distinct non-zero value).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 has exactly one non-zero value (1) across almost all SUMMON_MOUNT records. "
                        + "Functions as a binary toggle or mode flag, consistent with the Value08=1 pattern across operations. Retail name unresolved."
                };
                return true;
            }

            // SUMMON_MOUNT Value[0]: 232 non-zero records, 186 distinct values — referenced mount/creature ID.
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string smV0Dominant = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SUMMON_MOUNT Value[0] — referenced mount or creature ID (232 non-zero records, 186 distinct values).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] holds the mount or creature ID summoned by this SUMMON_MOUNT component. "
                        + "High distinct-value count (186) relative to record count (232) indicates a direct ID reference, not an enum. "
                        + (string.IsNullOrWhiteSpace(smV0Dominant) ? string.Empty : "Sample values: " + smV0Dominant + ".")
                };
                return true;
            }

            // SUMMON_MOUNT Value[2]: 77 non-zero, 4 distinct (3192, 7449, 7666, 7667) — secondary entity/resource ID reference.
            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string smV2Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "SUMMON_MOUNT Value[2] — secondary entity or resource ID reference (77 non-zero records, 4 distinct values: 3192, 7449, 7666, 7667).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] has 77 non-zero records with only 4 distinct high-range IDs, indicating an ID reference rather than an enum or percentage. "
                        + "May encode a companion appearance, alternate form, or overlay entity ID. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(smV2Dominant) ? string.Empty : "Observed: " + smV2Dominant + ".")
                };
                return true;
            }

            return false;
        }

        private static bool TryBuildExtDataVal2Inference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 2)
                return false;

            // Tally how many observed values map to a known ComponentOperation name.
            int totalCount = observations.Count;
            int knownCount = 0;
            int zeroCount = 0;
            Dictionary<string, int> valueCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (FieldObservation obs in observations)
            {
                string rv = obs.RawValue ?? string.Empty;
                if (!valueCounts.ContainsKey(rv))
                    valueCounts[rv] = 0;
                valueCounts[rv]++;

                long num;
                if (long.TryParse(rv, NumberStyles.Integer, CultureInfo.InvariantCulture, out num) && num >= 0)
                {
                    if (num == 0)
                    {
                        zeroCount++;
                        knownCount++; // 0 = NONE / inactive — known sentinel
                    }
                    else
                    {
                        string opName = DefinitionCatalog.DescribeComponentOperationValue((uint)num);
                        if (!string.IsNullOrWhiteSpace(opName) && !opName.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase))
                            knownCount++;
                    }
                }
            }

            string coverageLine = knownCount.ToString(CultureInfo.InvariantCulture)
                + " of " + totalCount.ToString(CultureInfo.InvariantCulture)
                + " observed values (" + (totalCount > 0 ? (knownCount * 100 / totalCount).ToString(CultureInfo.InvariantCulture) : "0") + "%) resolve to a named ComponentOperation.";

            string dominantValues = string.Join(", ", valueCounts
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key, StringComparer.OrdinalIgnoreCase)
                .Take(8)
                .Select(kvp =>
                {
                    long num;
                    string label = kvp.Key;
                    if (long.TryParse(kvp.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out num) && num > 0)
                    {
                        string opName = DefinitionCatalog.DescribeComponentOperationValue((uint)num);
                        if (!string.IsNullOrWhiteSpace(opName) && !opName.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase))
                            label = kvp.Key + "=" + opName;
                    }
                    return label + " x" + kvp.Value.ToString(CultureInfo.InvariantCulture);
                }));

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val2 — ComponentOperation type code for this ext-data expression slot.",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Across all component operations in the extracted client BIN, ExtData Val2 consistently encodes a ComponentOperation-family type code "
                    + "identifying what kind of effect the slot describes (DAMAGE, STAT_CHANGE, HEAL, AP_CHANGE, etc.). "
                    + coverageLine
                    + (zeroCount > 0 ? " Value 0 (" + zeroCount.ToString(CultureInfo.InvariantCulture) + " occurrences) = NONE / inactive slot." : string.Empty)
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant observed values: " + dominantValues + ".")
                    + " Values not yet present in the ComponentOperation enum represent undocumented operation type codes observed in client data."
            };
            return true;
        }

        private static bool TryBuildApplyAbilityExtDataKnownFieldInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 23 || observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                return false;

            string semanticSummary;
            string fieldNotes;

            switch (valueIndex)
            {
                case 1:
                    semanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val1 — application target for the embedded APPLY_ABILITY sub-effect.";
                    fieldNotes = "Val1=1 = Self (apply sub-effect to the caster). Val1=2 = Target (apply sub-effect to the ability target or enemy). "
                        + "STAT_CHANGE and DAMAGE sub-ops are target-directed (Val1=2) in 98%+ of cases; "
                        + "INTERRUPT, AP_REGEN_CHANGE, AP_CHANGE, and HEAL sub-ops are self-directed (Val1=1) in 95%+ of cases.";
                    break;

                default:
                    return false;
            }

            string dominantValues = string.Join(", ", observations
                .GroupBy(row => row.RawValue, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.OrdinalIgnoreCase)
                .Take(6)
                .Select(group => group.Key + " x" + group.Count().ToString(CultureInfo.InvariantCulture)));

            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Inferred,
                Notes = "Pattern analysis of 2075 APPLY_ABILITY component records in the extracted client BIN. "
                    + fieldNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryBuildApplyAbilityStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 23 || observations == null || observations.Count == 0)
                return false;

            string applyAbilityValueSummary;
            string applyAbilityValueNotes;
            if (TryDescribeApplyAbilityValueField(fieldKey, out applyAbilityValueSummary, out applyAbilityValueNotes))
            {
                string dominantValuesAA = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = applyAbilityValueSummary,
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Inferred from APPLY_ABILITY value distribution across 2,075 extracted client BIN records. "
                        + applyAbilityValueNotes
                        + (string.IsNullOrWhiteSpace(dominantValuesAA) ? string.Empty : " Dominant raw values: " + dominantValuesAA + ".")
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                return false;

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeApplyAbilityExtDataField(valueIndex, out semanticSummary, out roleNotes))
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 6);
            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Structural,
                Notes = "This is a structural inference from recurring APPLY_ABILITY ext-data layouts in extracted client BIN rows for ExtData slot "
                    + slotIndex.ToString(CultureInfo.InvariantCulture)
                    + ". "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                    + " Exact per-value retail semantics are still unresolved; use the value-profile evidence to inspect raw-value clusters."
            };
            return true;
        }

        private static bool TryBuildNearConstantMultiplierInference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int index;
            if (!TryParseIndexedField(fieldKey, "Multiplier[", out index))
                return false;

            int totalCount = observations.Count;
            int count100 = observations.Count(obs => string.Equals(obs.RawValue, "100", StringComparison.OrdinalIgnoreCase));
            if (count100 * 100 / totalCount < 95)
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 6);
            inference = new FieldObservationInference
            {
                SemanticSummary = fieldKey + " — percentage scaling multiplier (100 = 100% = no change; constant across 95%+ of records for this operation).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Multiplier[" + index.ToString(CultureInfo.InvariantCulture) + "] is 100 in "
                    + count100.ToString(CultureInfo.InvariantCulture) + " of "
                    + totalCount.ToString(CultureInfo.InvariantCulture)
                    + " records for this operation. "
                    + "100 = 100% scaling = unmodified baseline. "
                    + "These multiplier slots scale the corresponding Value[N] slots independently. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryBuildNamedControlFieldInference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeNamedControlField(fieldKey, out semanticSummary, out roleNotes))
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 6);
            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Inferred,
                Notes = "This inferred meaning comes from the descriptive client field name and the observed value shapes in extracted client BIN rows. "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                    + " Exact display formatting is not directly token-confirmed yet."
            };
            return true;
        }

        private static bool TryBuildStatChangeFlagsRawInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 2 || observations == null || observations.Count == 0)
                return false;
            if (!string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                return false;

            int totalCount = observations.Count;
            int zeroCount = observations.Count(obs =>
            {
                long num;
                return long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num) && num == 0;
            });
            int pctNonZero = totalCount > 0 ? (totalCount - zeroCount) * 100 / totalCount : 0;
            string dominantValues = BuildDominantRawValueSummary(observations, 8);

            inference = new FieldObservationInference
            {
                SemanticSummary = "STAT_CHANGE stat-type bitmask — bit N identifies which Stat (Stats enum value N) this component modifies.",
                Confidence = SemanticConfidence.Inferred,
                Notes = "FlagsRaw encodes the stat type(s) being modified using a bitmask where bit position N corresponds to Stats enum value N (Common/Database/GameData.cs). "
                    + "Common single-stat values: 16=Toughness(16%), 2=Strength(12%), 512=Intelligence(7%), 256=BallisticSkill(7%), "
                    + "65536=CorporealResistance(6%), 16384=SpiritResistance(6%), 32768=ElementalResistance(6%). "
                    + "Multi-stat value 0x1C000=SpiritResist+ElementalResist+CorporealResist appears in 3% of records. "
                    + pctNonZero.ToString(CultureInfo.InvariantCulture) + "% of observations for this operation have non-zero FlagsRaw. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryBuildApplyAbilityFlagsRawInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 23 || observations == null || observations.Count == 0)
                return false;
            if (!string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                return false;

            int totalCount = observations.Count;
            int zeroCount = observations.Count(obs =>
            {
                long num;
                return long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num) && num == 0;
            });
            int pctZero = totalCount > 0 ? zeroCount * 100 / totalCount : 0;
            string dominantValues = BuildDominantRawValueSummary(observations, 6);

            inference = new FieldObservationInference
            {
                SemanticSummary = "APPLY_ABILITY FlagsRaw — CrowdControlTypes gate flags (typically 0 for unconditional applies; non-zero encodes CC types that gate or restrict the sub-op).",
                Confidence = SemanticConfidence.Inferred,
                Notes = pctZero.ToString(CultureInfo.InvariantCulture) + "% of APPLY_ABILITY records have FlagsRaw=0 (unconditional apply). "
                    + "Non-zero values use the CrowdControlTypes bit encoding: "
                    + "bit 0=Snare(1), bit 1=Root(2), bit 2=Disarm(4), bit 3=Silence(8), bit 4=Knockdown(16), bit 5=Stagger(32), bit 7=Grapple(128). "
                    + "Common non-zero: 16=Knockdown-gate, 1=Snare-gate, 4=Disarm-gate, 7=MoveImpedance. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed values: " + dominantValues + ".")
            };
            return true;
        }

        // Maximum valid CrowdControlTypes bitmask: 1+2+4+8+16+32+64+128+2048 = 2303.
        private const long CcBitsMask = 2303L;

        // Generic catch-all: Value08 with a single distinct non-zero value of 1 → binary activation flag.
        // Fires for any operation that doesn't have a dedicated Value08 handler.
        private static bool TryBuildGenericValue08BinaryInference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (!string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
                return false;
            if (observations == null || observations.Count == 0)
                return false;

            // Check that every non-zero observation is exactly 1.
            bool hasAny = false;
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (!long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    return false;
                if (num != 0 && num != 1)
                    return false;
                if (num == 1) hasAny = true;
            }
            if (!hasAny)
                return false;

            inference = new FieldObservationInference
            {
                SemanticSummary = "Value08 — binary activation flag (all non-zero values = 1; single-bit toggle pattern seen across operations).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Value08 has exactly one distinct non-zero value (1) in " + observations.Count.ToString(CultureInfo.InvariantCulture) + " observed records. "
                    + "This binary-flag pattern (Value08=1) is consistent across DAMAGE, STAT_CHANGE, BTA, VELOCITY, HEAL, SUMMON_MOUNT, MONSTER_CONTROLLER, and many other operations. "
                    + "Retail name unresolved."
            };
            return true;
        }

        // Generic catch-all: Value15 where all non-zero distinct values are valid CrowdControlTypes bit combinations.
        // Fires for any operation that doesn't have a dedicated Value15 handler.
        private static bool TryBuildGenericValue15CcGateInference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (!string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
                return false;
            if (observations == null || observations.Count == 0)
                return false;

            HashSet<long> distinctNonZero = new HashSet<long>();
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (!long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    return false;
                if (num == 0) continue;
                if (num < 0 || (num & ~CcBitsMask) != 0)
                    return false; // value has bits outside CC mask
                distinctNonZero.Add(num);
            }
            if (distinctNonZero.Count == 0)
                return false;

            string dominant = BuildDominantRawValueSummary(observations, 8);
            inference = new FieldObservationInference
            {
                SemanticSummary = "Value15 — CrowdControlTypes gate flag (" + distinctNonZero.Count.ToString(CultureInfo.InvariantCulture) + " distinct non-zero values, all valid CC bit combinations).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Value15 uses CrowdControlTypes bit encoding: 1=Snare, 2=Root, 4=Disarm, 8=Silence, 16=Knockdown, 32=Stagger, 64=bit6, 128=Grapple. "
                    + "All " + distinctNonZero.Count.ToString(CultureInfo.InvariantCulture) + " distinct non-zero values decompose entirely into known CC bits. "
                    + "0 = no CC gate (inactive). This CC-gate pattern appears across many operations. Retail name unresolved. "
                    + (string.IsNullOrWhiteSpace(dominant) ? string.Empty : "Observed: " + dominant + ".")
            };
            return true;
        }

        // Generic catch-all: FlagsRaw with a single distinct non-zero power-of-two value → single-bit binary flag.
        // Fires before TryBuildGenericFlagsRawInference to promote single-bit fields from Structural to Inferred.
        private static bool TryBuildGenericSingleBitFlagsRawInference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (!string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                return false;
            if (observations == null || observations.Count == 0)
                return false;

            long distinctValue = 0;
            bool hasAny = false;
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (!long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    return false;
                if (num == 0) continue;
                if (!hasAny)
                {
                    distinctValue = num;
                    hasAny = true;
                }
                else if (num != distinctValue)
                    return false; // more than one distinct non-zero value
            }
            if (!hasAny)
                return false;
            // Require exactly one bit set (power of 2) or a very small integer (1, 2, 3, 4).
            if (distinctValue <= 0)
                return false;
            bool isPowerOfTwo = (distinctValue & (distinctValue - 1)) == 0;
            if (!isPowerOfTwo && distinctValue > 4)
                return false;

            string bitDesc = isPowerOfTwo
                ? "bit" + (long)(System.Math.Log(distinctValue, 2)) + " (value=" + distinctValue.ToString(CultureInfo.InvariantCulture) + ")"
                : "value=" + distinctValue.ToString(CultureInfo.InvariantCulture);
            inference = new FieldObservationInference
            {
                SemanticSummary = "FlagsRaw — single-bit activation flag (" + observations.Count.ToString(CultureInfo.InvariantCulture) + " non-zero records; single value = " + distinctValue.ToString(CultureInfo.InvariantCulture) + ").",
                Confidence = SemanticConfidence.Inferred,
                Notes = "FlagsRaw has exactly one distinct non-zero value (" + bitDesc + ") across all observed records. "
                    + "Functions as a binary on/off flag. Retail bit name unresolved."
            };
            return true;
        }

        private static bool TryBuildGenericFlagsRawInference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeGenericFlagsRawField(fieldKey, out semanticSummary, out roleNotes))
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 6);
            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Structural,
                Notes = "This structural inference comes from the explicit client field name and the recurring raw bit-pattern values in extracted client BIN rows. "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                    + " Exact per-bit retail semantics are still unresolved; use the value-profile evidence to inspect recurring flag combinations."
            };
            return true;
        }

        private static bool TryBuildCcStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 12 || observations == null || observations.Count == 0)
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 6);
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CCType bitmask — CrowdControlTypes flags (Snare=1, Root=2, Disarm=4, Silence=8, Knockdown=16, Stagger=32, Grapple=128).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Bit mapping from CrowdControlTypes server enum (Common/Database/GameData.cs): "
                        + "bit 0=Snare(1), bit 1=Root(2), bit 2=Disarm(4), bit 3=Silence(8), "
                        + "bit 4=Knockdown(16), bit 5=Stagger(32), bit 7=Grapple(128). "
                        + "Bit 6 (64) and bit 11 (2048) are present in client BIN data but not named in the server enum. "
                        + "Dominant values: 2175(AllStandardCC+Stagger+bit6+bit11, 22%), 2303(All+bit11, 15%), 8(Silence, 14%), 1(Snare, 12%). "
                        + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
                };
                return true;
            }

            if (fieldKey.StartsWith("Value[", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CC " + fieldKey + " — near-inactive (99-100% zero across all 586 extracted CC records).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = fieldKey + " is 0 in 99-100% of CC records across 586 extracted client BIN rows. "
                        + "Rare non-zero exceptions (Value[1]=1 and Value[3]=7 each appear once) have unresolved semantics. "
                        + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CC Value15 — application type marker (4=dominant-CC-type 69%, 0=absent 31%, 5=rare-CC-variant <1%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value15 is a low-cardinality companion selector for the CC component. "
                        + "Value=4 (405/586 records, 69%) = dominant CC application type. "
                        + "Value=0 (180/586, 31%) = no type marker. "
                        + "Value=5 (1/586, <1%) = rare CC variant. "
                        + "Retail names for these values are unresolved. "
                        + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "CC Value08 — binary modifier flag (96% zero; Value08=1 in 24 records, 4%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 is 0 in 562 of 586 CC records. "
                        + "Value08=1 appears in 24 records (4%). "
                        + "The binary distribution suggests a boolean toggle or mode flag. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                return false;

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeCcExtDataField(valueIndex, out semanticSummary, out roleNotes))
                return false;

            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Structural,
                Notes = "This is a structural inference from recurring CC ext-data layouts in extracted client BIN rows for ExtData slot "
                    + slotIndex.ToString(CultureInfo.InvariantCulture)
                    + ". "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                    + " Exact per-value retail semantics are still unresolved; use the value-profile evidence to inspect raw-value clusters."
            };
            return true;
        }

        private static bool TryBuildKnockbackStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 24 || observations == null || observations.Count == 0)
                return false;

            string semanticSummary;
            string roleNotes;
            string dominantValues = BuildDominantRawValueSummary(observations, 6);

            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "KNOCKBACK FlagsRaw — sparse modifier bitfield (96% zero; non-zero values: 2=bit1-set 1.9%, 127=bits0-6-set 1.5%, 1=bit0-set 0.4%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is 0 in 252 of 262 KNOCKBACK records. "
                        + "Non-zero values: 2=bit1(5 records), 127=bits0-6(4 records), 1=bit0(1 record). "
                        + "Bit semantics are unresolved; the low cardinality and sparse occurrence suggest modifier flags rather than a free bitfield. "
                        + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "KNOCKBACK Value08 — near-inactive modifier flag (261 of 262 records = 0; Value08=1 in 1 record).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 is 0 in 261 of 262 KNOCKBACK records. "
                        + "The single non-zero occurrence (value=1) represents a rare modifier. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Observed: " + dominantValues + ".")
                };
                return true;
            }

            if (TryDescribeKnockbackValueField(fieldKey, out semanticSummary, out roleNotes))
            {
                string knockbackFieldContext = BuildKnockbackValueFieldAggregateDecode(fieldKey);
                inference = new FieldObservationInference
                {
                    SemanticSummary = semanticSummary,
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Inferred from KNOCKBACK value pattern analysis (262 extracted client BIN rows). "
                        + roleNotes
                        + (string.IsNullOrWhiteSpace(knockbackFieldContext) ? string.Empty : " " + knockbackFieldContext)
                        + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                return false;

            if (!TryDescribeKnockbackExtDataField(valueIndex, out semanticSummary, out roleNotes))
                return false;

            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Structural,
                Notes = "This is a structural inference from recurring KNOCKBACK ext-data layouts in extracted client BIN rows for ExtData slot "
                    + slotIndex.ToString(CultureInfo.InvariantCulture)
                    + ". "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                    + " Exact per-value retail semantics are still unresolved; use the value-profile evidence to inspect raw-value clusters."
            };
            return true;
        }

        private static bool TryBuildImmunityStructuralInference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (operationId != 38 || observations == null || observations.Count == 0)
                return false;

            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                string immunityV08Dominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "IMMUNITY Value08 — binary modifier flag (61% zero; value=1 in 66 of 169 records, 39%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value08 is 0 in 103 of 169 IMMUNITY records (61%). "
                        + "Value08=1 appears in 66 records (39%). "
                        + "The binary distribution suggests a boolean modifier or purgeable-immunity flag. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(immunityV08Dominant) ? string.Empty : "Observed: " + immunityV08Dominant + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string immunityFlagsDominant = BuildDominantRawValueSummary(observations, 4);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "IMMUNITY FlagsRaw — sparse binary flag (84% zero; value=1 in 27 of 169 records, 16%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "FlagsRaw is 0 in 142 of 169 IMMUNITY records. "
                        + "The only non-zero value observed is 1 (bit 0 set), present in 27 records (16%). "
                        + "The binary distribution suggests a boolean modifier or purgeable-immunity flag. Retail name unresolved. "
                        + (string.IsNullOrWhiteSpace(immunityFlagsDominant) ? string.Empty : "Observed: " + immunityFlagsDominant + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                string immunityDominantValues = BuildDominantRawValueSummary(observations, 8);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "IMMUNITY Value[0] — OperationType code identifying which effect category this component grants immunity to.",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[0] encodes the OperationType (component operation code) being blocked by this immunity component. "
                        + "Dominant values: 12=CC-immunity(41%), 1=DAMAGE-immunity(15%), 7=op7-immunity(8%), 24=KNOCKBACK-immunity(4%). "
                        + "Less common values (35, 38, 39, 46, 1015, 1020) appear in minority immunity layouts. "
                        + (string.IsNullOrWhiteSpace(immunityDominantValues) ? string.Empty : "Observed values: " + immunityDominantValues + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                string immunityDominantValues1 = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "IMMUNITY Value[1] — immunity strength level (0–100 scale; 90% of records = 100 = full immunity).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[1] encodes the immunity strength on a 0–100 scale. "
                        + "90% of IMMUNITY records carry value=100 (full immunity); 9% carry value=0 (no strength applied). "
                        + (string.IsNullOrWhiteSpace(immunityDominantValues1) ? string.Empty : "Observed values: " + immunityDominantValues1 + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                string immunityDominantValues2 = BuildDominantRawValueSummary(observations, 6);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "IMMUNITY Value[2] — immunity sub-type selector (0=no-sub-type 66%, 6=sub-type-6 9%, 1=sub-type-1 8%, 3=sub-type-3 8%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Value[2] selects an immunity sub-type variant within the IMMUNITY component. "
                        + "Value=0 (66%) = no sub-type applied (default full-immunity). "
                        + "Non-zero values (1, 3, 6 most common) represent named sub-type categories whose retail semantics are unresolved. "
                        + (string.IsNullOrWhiteSpace(immunityDominantValues2) ? string.Empty : "Observed values: " + immunityDominantValues2 + ".")
                };
                return true;
            }

            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "Value[4]", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase))
            {
                inference = new FieldObservationInference
                {
                    SemanticSummary = "IMMUNITY " + fieldKey + " — near-inactive (100% zero across all extracted IMMUNITY records).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "All extracted IMMUNITY records carry value=0 for " + fieldKey + ". "
                        + "This field has no active payload in any extracted IMMUNITY layout."
                };
                return true;
            }

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                return false;

            // IMMUNITY ExtData Val1: upgrade Val1=3 (passive/wide-scope, 51%) and Val1=7 (active/trigger-specific, 33%) to Inferred.
            // Val1=2 (13%) is already covered by the universal Val1 decoder.
            if (valueIndex == 1)
            {
                string immunityVal1Dominant = BuildDominantRawValueSummary(observations, 5);
                inference = new FieldObservationInference
                {
                    SemanticSummary = "IMMUNITY ExtData Val1 — application scope selector (3=passive/wide-scope 51%, 7=active/trigger-specific 33%, 2=target-application 13%).",
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Val1 in IMMUNITY ExtData slots selects the immunity application scope. "
                        + "Val1=3 (51%) = passive or wide-scope immunity application. "
                        + "Val1=7 (33%) = active or trigger-specific immunity application. "
                        + "Val1=2 (13%) = standard target-application (universal Val1 meaning). "
                        + "Retail names for Val1=3 and Val1=7 are unresolved. "
                        + (string.IsNullOrWhiteSpace(immunityVal1Dominant) ? string.Empty : "Observed values: " + immunityVal1Dominant + ".")
                };
                return true;
            }

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeImmunityExtDataField(valueIndex, out semanticSummary, out roleNotes))
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 6);
            inference = new FieldObservationInference
            {
                SemanticSummary = semanticSummary,
                Confidence = SemanticConfidence.Structural,
                Notes = "This is a structural inference from recurring IMMUNITY ext-data layouts in extracted client BIN rows for ExtData slot "
                    + slotIndex.ToString(CultureInfo.InvariantCulture)
                    + ". "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant raw values: " + dominantValues + ".")
                    + " Exact per-value retail semantics are still unresolved; use the value-profile evidence to inspect raw-value clusters."
            };
            return true;
        }

        private static string BuildDamageStructuralValueNote(int valueIndex, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            switch (valueIndex)
            {
                case 1:
                    if (string.Equals(rawValue, "2", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant DAMAGE Val1 branch marker in the recurring ext-data layout.";
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the secondary DAMAGE Val1 branch marker in the recurring ext-data layout.";
                    break;
                case 4:
                    if (string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant DAMAGE family marker across the recurring ext-data layout.";
                    if (string.Equals(rawValue, "9", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority DAMAGE family-marker variant.";
                    break;
                case 5:
                    if (string.Equals(rawValue, "3", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant DAMAGE Val5 auxiliary-selector branch in the minority tail layout.";
                    break;
                case 6:
                    long damageLinkValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out damageLinkValue))
                    {
                        if (damageLinkValue >= 1000)
                            return "This large DAMAGE Val6 value behaves like an identifier-style link target rather than a scalar payload.";
                        if (damageLinkValue > 0)
                            return "This DAMAGE Val6 value behaves like a compact branch-local link marker rather than a free scalar.";
                    }
                    break;
                case 7:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant compact DAMAGE Val7 branch in the recurring ext-data layout.";

                    long damagePayloadValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out damagePayloadValue)
                        && damagePayloadValue >= 100)
                        return "This is a scalar-looking DAMAGE Val7 branch variant compared with the dominant compact 1-branch.";
                    break;
                case 8:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant sparse DAMAGE Val8 tail branch.";
                    break;
                case 9:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This fixed 1-value behaves like a slot-presence marker rather than a scalar payload.";
                    break;
            }

            return string.Empty;
        }

        private static string BuildBonusTypeAdjustStructuralValueNote(int valueIndex, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            switch (valueIndex)
            {
                case 1:
                    if (string.Equals(rawValue, "2", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant BONUS_TYPE_ADJUST Val1 branch marker in the recurring ext-data layout.";
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the secondary BONUS_TYPE_ADJUST Val1 branch marker in the recurring ext-data layout.";
                    break;
                case 4:
                    if (string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant BONUS_TYPE_ADJUST family marker across the recurring ext-data layout.";
                    if (string.Equals(rawValue, "9", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority BONUS_TYPE_ADJUST family-marker variant.";
                    break;
                case 5:
                    if (string.Equals(rawValue, "3", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant BONUS_TYPE_ADJUST Val5 auxiliary-selector branch in the minority tail layout.";
                    break;
                case 6:
                    long bonusLinkValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out bonusLinkValue))
                    {
                        if (bonusLinkValue >= 1000)
                            return "This large BONUS_TYPE_ADJUST Val6 value behaves like an identifier-style link target rather than a scalar payload.";
                        if (bonusLinkValue > 0)
                            return "This BONUS_TYPE_ADJUST Val6 value behaves like a compact branch-local link marker rather than a free scalar.";
                    }
                    break;
                case 7:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant compact BONUS_TYPE_ADJUST Val7 branch in the recurring ext-data layout.";

                    long bonusPayloadValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out bonusPayloadValue)
                        && bonusPayloadValue >= 100)
                        return "This is a scalar-looking BONUS_TYPE_ADJUST Val7 branch variant compared with the dominant compact 1-branch.";
                    break;
                case 8:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant sparse BONUS_TYPE_ADJUST Val8 tail branch.";
                    break;
                case 9:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This fixed 1-value behaves like a slot-presence marker rather than a scalar payload.";
                    break;
            }

            return string.Empty;
        }

        private static string BuildApplyAbilityStructuralValueNote(int valueIndex, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            switch (valueIndex)
            {
                case 1:
                    if (string.Equals(rawValue, "2", StringComparison.OrdinalIgnoreCase))
                        return "This raw value is the dominant branch marker and frequently appears with Val2=2, Val3=1, and Val7=1.";
                    break;
                case 2:
                    if (string.Equals(rawValue, "2", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant Val2 branch and is strongly paired with Val1=2, Val3=1, Val4=8, and Val7=1.";
                    if (string.Equals(rawValue, "9", StringComparison.OrdinalIgnoreCase))
                        return "This common secondary branch is strongly paired with Val1=1, Val3=4, Val4=8, and often Val6=100.";
                    break;
                case 3:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant Val3 profile and is strongly paired with Val1=2, Val2=2, Val4=8, and Val7=1.";
                    if (string.Equals(rawValue, "4", StringComparison.OrdinalIgnoreCase))
                        return "This secondary Val3 profile is strongly paired with Val1=1, Val2=9, Val4=8, and often Val6=100.";
                    break;
                case 4:
                    if (string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant APPLY_ABILITY family marker across the extracted rows.";
                    if (string.Equals(rawValue, "9", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority variant of the dominant 8-family marker.";
                    break;
                case 5:
                    if (string.Equals(rawValue, "3", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant APPLY_ABILITY Val5 auxiliary-selector branch in the minority tail layout.";
                    if (string.Equals(rawValue, "5", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "9", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority APPLY_ABILITY Val5 auxiliary-selector variant.";
                    break;
                case 6:
                    if (string.Equals(rawValue, "100", StringComparison.OrdinalIgnoreCase))
                        return "This compact APPLY_ABILITY Val6 link marker is common in the secondary Val1=1, Val2=9, Val3=4 branch.";

                    long applyAbilityLinkValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out applyAbilityLinkValue))
                    {
                        if (applyAbilityLinkValue >= 1000)
                            return "This large APPLY_ABILITY Val6 value behaves like an identifier-style link target rather than a scalar payload.";
                        if (applyAbilityLinkValue > 0)
                            return "This APPLY_ABILITY Val6 value behaves like a compact branch-local link marker rather than a free scalar.";
                    }
                    break;
                case 7:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant Val7 payload branch and is strongly paired with Val1=2, Val2=2, Val3=1, and Val4=8.";
                    if (string.Equals(rawValue, "5", StringComparison.OrdinalIgnoreCase))
                        return "This common secondary Val7 branch is strongly paired with Val1=1, Val2=9, Val3=4, Val4=8, and Val6=100.";
                    if (string.Equals(rawValue, "250", StringComparison.OrdinalIgnoreCase))
                        return "This branch is strongly paired with Val2=3, Val3=4, and Val4=8.";
                    break;
                case 8:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant sparse APPLY_ABILITY Val8 tail branch.";
                    if (string.Equals(rawValue, "2", StringComparison.OrdinalIgnoreCase) || string.Equals(rawValue, "3", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority sparse APPLY_ABILITY Val8 tail variant.";
                    break;
                case 9:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This fixed 1-value behaves like a slot-presence marker rather than a scalar payload.";
                    break;
            }

            return string.Empty;
        }

        private static string BuildKnockbackStructuralValueNote(int valueIndex, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            switch (valueIndex)
            {
                case 1:
                    if (string.Equals(rawValue, "2", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant KNOCKBACK Val1 branch marker in the recurring ext-data layout.";
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the secondary KNOCKBACK Val1 branch marker in the recurring ext-data layout.";
                    break;
                case 4:
                    if (string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant KNOCKBACK family marker across the recurring ext-data layout.";
                    if (string.Equals(rawValue, "9", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority KNOCKBACK family-marker variant.";
                    break;
                case 5:
                    if (string.Equals(rawValue, "3", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant KNOCKBACK Val5 auxiliary-selector branch in the minority tail layout.";
                    break;
                case 6:
                    long knockbackLinkValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out knockbackLinkValue))
                    {
                        if (knockbackLinkValue >= 1000)
                            return "This large KNOCKBACK Val6 value behaves like an identifier-style link target rather than a scalar payload.";
                        if (knockbackLinkValue > 0)
                            return "This KNOCKBACK Val6 value behaves like a compact branch-local link marker rather than a free scalar.";
                    }
                    break;
                case 7:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant compact KNOCKBACK Val7 branch in the recurring ext-data layout.";

                    long knockbackPayloadValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out knockbackPayloadValue)
                        && knockbackPayloadValue >= 100)
                        return "This is a scalar-looking KNOCKBACK Val7 branch variant compared with the dominant compact 1-branch.";
                    break;
                case 8:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant sparse KNOCKBACK Val8 tail branch.";
                    break;
                case 9:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This fixed 1-value behaves like a slot-presence marker rather than a scalar payload.";
                    break;
            }

            return string.Empty;
        }

        private static string BuildImmunityStructuralValueNote(int valueIndex, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            switch (valueIndex)
            {
                case 4:
                    if (string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant IMMUNITY family marker across the recurring ext-data layout.";
                    if (string.Equals(rawValue, "9", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority IMMUNITY family-marker variant.";
                    break;
                case 5:
                    if (string.Equals(rawValue, "3", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant IMMUNITY Val5 auxiliary-selector branch in the minority tail layout.";
                    break;
                case 6:
                    long immunityLinkValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out immunityLinkValue))
                    {
                        if (immunityLinkValue >= 1000)
                            return "This large IMMUNITY Val6 value behaves like an identifier-style link target rather than a scalar payload.";
                        if (immunityLinkValue > 0)
                            return "This IMMUNITY Val6 value behaves like a compact branch-local link marker rather than a free scalar.";
                    }
                    break;
                case 7:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant compact IMMUNITY Val7 branch in the recurring ext-data layout.";

                    long immunityPayloadValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out immunityPayloadValue)
                        && immunityPayloadValue >= 1000)
                        return "This is an identifier-like IMMUNITY Val7 branch variant compared with the dominant compact 1-branch.";
                    break;
                case 8:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant sparse IMMUNITY Val8 tail branch.";
                    break;
                case 9:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This fixed 1-value behaves like a slot-presence marker rather than a scalar payload.";
                    break;
            }

            return string.Empty;
        }

        private static string BuildImmunityValue0Note(string rawValue)
        {
            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return string.Empty;

            switch ((int)numericValue)
            {
                case 1: return "Value[0]=1: DAMAGE-immunity — blocks damage-type component effects.";
                case 7: return "Value[0]=7: op-7-immunity — blocks operation-7 effects (op-7 semantics unresolved).";
                case 12: return "Value[0]=12: CC-immunity — blocks crowd-control component effects (most common, 41%).";
                case 24: return "Value[0]=24: KNOCKBACK-immunity — blocks knockback component effects.";
                default: return "Value[0]=" + rawValue + ": less common immunity target; may be an operation code or ID reference.";
            }
        }

        private static string BuildKnockbackValueFieldNote(string fieldKey, string rawValue)
        {
            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return string.Empty;

            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
            {
                if (numericValue <= 25000)
                    return "This is a short-range KNOCKBACK displacement branch compared with the common 40000-65000 launch band.";
                if (numericValue >= 80000)
                    return "This is a high-magnitude KNOCKBACK displacement branch compared with the common 40000-65000 launch band.";
                return "This is a midrange KNOCKBACK displacement value in the common player-launch band.";
            }

            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
            {
                if (numericValue == 0)
                    return "This zero value matches flatter shove rows that do not use a secondary lift control.";
                if (numericValue <= 450)
                    return "This is a shallow secondary launch-control value in the lower observed lift band.";
                if (numericValue >= 800)
                    return "This is a steep secondary launch-control value in the upper observed lift band.";
                return "This is a midrange secondary launch-control value in the common KNOCKBACK lift band.";
            }

            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
            {
                if (numericValue < 0)
                    return "This negative trajectory-bias branch appears on mount-style launch rows rather than the common shove layout.";
                if (numericValue == 0)
                    return "This zero trajectory-bias branch aligns with the simpler shove layout.";
                if (numericValue <= 100)
                    return "This small positive trajectory-bias branch appears on compact knockback test and short-range shove variants.";
                return "This large positive trajectory-bias branch appears on minority scripted launch variants rather than the common shove layout.";
            }

            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
            {
                if (numericValue == 0)
                    return "This value matches the simplest shove profile with no extra motion-profile selector.";
                if (numericValue == 2)
                    return "This is the dominant KNOCKBACK motion-profile selector in the player-launch family.";
                if (numericValue == 3)
                    return "This is a secondary KNOCKBACK motion-profile selector seen in mixed or hidden side-effect rows.";
                return "This is a minority KNOCKBACK motion-profile selector variant.";
            }

            return string.Empty;
        }

        private static string BuildKnockbackValueFieldAggregateDecode(string fieldKey)
        {
            if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
                return "Horizontal knockback force magnitude in fixed-point (Multiplier[0] is always 100). "
                    + "Dominant values: 25000=short-range(24%), 50000=standard(11%), 45000(7%), 65000(4%), 90000=high-magnitude(4%). "
                    + "Distinct value count: 51 across 262 records.";

            if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
                return "Vertical arc height or lift parameter for the knockback trajectory (0 = flat/no-arc trajectory). "
                    + "Dominant values: 300(14%), 0-flat(13%), 600(12%), 500(10%), 1000(7%). "
                    + "Proportional to Value[0] in common pairs (25000/300, 50000/600).";

            if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
                return "Trajectory direction bias (0=neutral/simpler-shove(39%), 50=small-forward-lean(30%), negative=backward-pull/mount-launch(4%)). "
                    + "Dominant values: 0(39%), 50(30%), -200(4%), 100(3%).";

            if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
                return "Knockback motion-profile/type selector (7 distinct values). "
                    + "Dominant values: 1=standard-knockback(50%), 2=player-launch-family(24%), 0=no-motion-profile(15%), 3=scripted-variant(4%), 5=special-type(4%).";

            return string.Empty;
        }

        private static string BuildNamedControlFieldValueNote(string fieldKey, string rawValue)
        {
            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return string.Empty;

            if (string.Equals(fieldKey, "ActivationDelay", StringComparison.OrdinalIgnoreCase))
            {
                if (numericValue <= 100)
                    return "This is a short activation-delay branch compared with the common 250-1500 range.";
                if (numericValue >= 10000)
                    return "This is an unusually long activation-delay branch compared with the common sub-2-second timings.";
                return "This is a midrange activation-delay value in the common extracted-client timing band.";
            }

            if (string.Equals(fieldKey, "ConeAngle", StringComparison.OrdinalIgnoreCase))
            {
                if (numericValue >= 360)
                    return "This value behaves like a full-circle or effectively omni-directional arc.";
                if (numericValue >= 180)
                    return "This value behaves like a very wide directional arc.";
                return "This value behaves like a standard forward cone width.";
            }

            if (string.Equals(fieldKey, "FlightSpeed", StringComparison.OrdinalIgnoreCase))
            {
                if (numericValue <= 500)
                    return "This is on the slow end of the observed projectile-speed range.";
                if (numericValue >= 1500)
                    return "This is on the fast end of the observed projectile-speed range.";
                return "This is a midrange projectile-speed value in the observed set.";
            }

            if (string.Equals(fieldKey, "MaxTargets", StringComparison.OrdinalIgnoreCase))
            {
                if (numericValue <= 1)
                    return "This branch is capped to a single target.";
                if (numericValue >= 9)
                    return "This branch allows a broad multi-target cap compared with the common small counts.";
                return "This branch allows a small multi-target cap.";
            }

            return string.Empty;
        }

        private static string BuildGenericFlagsRawValueNote(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            long numericValue;
            if (!long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return "This raw value still behaves like an opaque packed flag field rather than a free scalar.";

            if (numericValue < 0)
                return "This negative raw value is unusual for a flags field and should be treated as a signed packed mask.";

            if (numericValue == 0)
                return "This zero raw value indicates that no flag bits are set.";

            ulong unsignedValue = (ulong)numericValue;
            int bitCount = CountSetBits(unsignedValue);
            string bitSummary = BuildSetBitSummary(unsignedValue, 6);

            if (bitCount <= 1)
                return "This raw value sets bit " + bitSummary + " only, so it behaves like an isolated flag branch.";

            if (bitCount <= 3)
                return "This raw value combines " + bitCount.ToString(CultureInfo.InvariantCulture) + " flag bits (" + bitSummary + "), so it behaves like a compact mode mask.";

            return "This raw value combines " + bitCount.ToString(CultureInfo.InvariantCulture) + " flag bits (" + bitSummary + "), so it behaves like a packed mode mask rather than a scalar.";
        }

        private static string BuildCcStructuralValueNote(string fieldKey, int? valueIndex, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(rawValue, "2175", StringComparison.OrdinalIgnoreCase))
                    return "Dominant packed CC FlagsRaw branch; commonly pairs with Value15=4 and ExtData Val1=2, Val3=1, Val4=8, Val7=1.";
                if (string.Equals(rawValue, "2303", StringComparison.OrdinalIgnoreCase))
                    return "Common packed CC FlagsRaw branch; stays in the dominant CC layout family and pairs with Value15=4.";
                if (string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase))
                    return "Compact low-bit CC FlagsRaw branch appearing in shorter CC layouts.";
                if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                    return "Minimal single-flag CC FlagsRaw variant inside the broader bitfield family.";
                return string.Empty;
            }

            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(rawValue, "4", StringComparison.OrdinalIgnoreCase))
                    return "This is the dominant CC Value15 marker and appears across almost every major FlagsRaw branch.";
                if (string.Equals(rawValue, "5", StringComparison.OrdinalIgnoreCase))
                    return "This is a rare Value15 variant inside the dominant CC layout family.";
                return string.Empty;
            }

            if (!valueIndex.HasValue)
                return string.Empty;

            switch (valueIndex.Value)
            {
                case 1:
                    if (string.Equals(rawValue, "2", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant CC Val1 branch marker and most often pairs with Val3=1 and Val4=8.";
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the secondary CC Val1 branch marker and commonly appears with the non-dominant Val3 profiles.";
                    break;
                case 2:
                    if (string.Equals(rawValue, "2", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant compact CC Val2 branch inside the recurring ext-data layout.";
                    if (string.Equals(rawValue, "15", StringComparison.OrdinalIgnoreCase))
                        return "This is a common secondary compact CC Val2 branch inside the recurring ext-data layout.";
                    if (string.Equals(rawValue, "88", StringComparison.OrdinalIgnoreCase))
                        return "This is a high scalar-looking Val2 variant compared with the compact selector-like CC branches.";
                    break;
                case 3:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant CC Val3 profile in the recurring ext-data layout.";
                    if (string.Equals(rawValue, "4", StringComparison.OrdinalIgnoreCase))
                        return "This is the main secondary CC Val3 profile in the recurring ext-data layout.";
                    if (string.Equals(rawValue, "5", StringComparison.OrdinalIgnoreCase) || string.Equals(rawValue, "6", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority CC Val3 profile variant in the recurring ext-data layout.";
                    break;
                case 4:
                    if (string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant CC family marker across the recurring ext-data layout.";
                    if (string.Equals(rawValue, "9", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority CC family-marker variant.";
                    break;
                case 5:
                    if (string.Equals(rawValue, "3", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant CC Val5 auxiliary-selector branch in the minority tail layout.";
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "5", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "8", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority CC Val5 auxiliary-selector variant.";
                    break;
                case 6:
                    long ccLinkValue;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out ccLinkValue))
                    {
                        if (ccLinkValue >= 1000)
                            return "This large CC Val6 value behaves like an identifier-style link target rather than a scalar payload.";
                        if (ccLinkValue > 0)
                            return "This CC Val6 value behaves like a compact branch-local link marker rather than a free scalar.";
                    }
                    break;
                case 7:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant compact CC Val7 branch in the recurring ext-data layout.";
                    if (string.Equals(rawValue, "20", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "50", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "100", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "200", StringComparison.OrdinalIgnoreCase))
                        return "This is a scalar-looking CC Val7 branch variant compared with the dominant compact 1-branch.";
                    break;
                case 8:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This is the dominant sparse CC Val8 tail branch.";
                    if (string.Equals(rawValue, "16", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "25", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "34", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rawValue, "150", StringComparison.OrdinalIgnoreCase))
                        return "This is a minority scalar-looking CC Val8 tail variant.";
                    break;
                case 9:
                    if (string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase))
                        return "This fixed 1-value behaves like a slot-presence marker rather than a scalar payload.";
                    break;
            }

            return string.Empty;
        }

        private static string BuildStatChangeFlagsRawBitDecode(string rawValue)
        {
            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return string.Empty;

            if (numericValue == 0)
                return "0 (no stat type specified)";

            // Bit N = Stats enum value N from Common/Database/GameData.cs.
            // Base stats: bits 1-9, 14-16.  Derived stats: bits 22+.
            Dictionary<int, string> statBits = new Dictionary<int, string>
            {
                { 1, "Strength" }, { 2, "Agility" }, { 3, "Willpower" }, { 4, "Toughness" },
                { 5, "Wounds" }, { 6, "Initiative" }, { 7, "WeaponSkill" }, { 8, "BallisticSkill" },
                { 9, "Intelligence" }, { 10, "BlockSkill" }, { 11, "ParrySkill" },
                { 12, "EvadeSkill" }, { 13, "DisruptSkill" },
                { 14, "SpiritResistance" }, { 15, "ElementalResistance" }, { 16, "CorporealResistance" },
                { 22, "IncomingDamage" }, { 23, "IncomingDamagePercent" },
                { 24, "OutgoingDamage" }, { 25, "OutgoingDamagePercent" },
                { 26, "Armor" }, { 28, "Block" }, { 29, "Parry" }, { 30, "Evade" }, { 31, "Disrupt" },
                { 32, "ActionPointRegen" }, { 33, "MoraleRegen" }, { 34, "Cooldown" },
                { 36, "CriticalDamage" }, { 37, "Range" }, { 38, "AutoAttackSpeed" },
                { 40, "AutoAttackDamage" }, { 41, "ActionPointCost" }, { 42, "CriticalHitRate" },
                { 43, "CriticalDamageTaken" }, { 44, "EffectResist" }, { 45, "EffectBuff" },
                { 47, "DamageAbsorb" }, { 65, "Stealth" }, { 66, "StealthDetection" },
                { 76, "MeleeCritRate" }, { 77, "RangedCritRate" }, { 78, "MagicCritRate" },
                { 79, "HealthRegen" }, { 80, "MeleePower" }, { 81, "RangedPower" }, { 82, "MagicPower" },
                { 84, "CriticalHitRateReduction" }, { 89, "HealCritRate" }, { 94, "HealingPower" }
            };

            List<string> parts = new List<string>();
            for (int n = 0; n < 64; n++)
            {
                if ((numericValue & (1L << n)) == 0)
                    continue;
                string statName;
                if (statBits.TryGetValue(n, out statName))
                    parts.Add("Stats." + statName);
                else
                    parts.Add("bit" + n.ToString(CultureInfo.InvariantCulture) + "(stat-" + n.ToString(CultureInfo.InvariantCulture) + "-unknown)");
            }

            return rawValue + " = " + string.Join(" | ", parts);
        }

        private static string BuildCcFlagsRawBitDecode(string rawValue)
        {
            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return string.Empty;

            if (numericValue == 0)
                return "0 (no CC types set)";

            List<string> parts = new List<string>();
            if ((numericValue & 1) != 0) parts.Add("Snare");
            if ((numericValue & 2) != 0) parts.Add("Root");
            if ((numericValue & 4) != 0) parts.Add("Disarm");
            if ((numericValue & 8) != 0) parts.Add("Silence");
            if ((numericValue & 16) != 0) parts.Add("Knockdown");
            if ((numericValue & 32) != 0) parts.Add("Stagger");
            if ((numericValue & 64) != 0) parts.Add("bit6(unknown-64)");
            if ((numericValue & 128) != 0) parts.Add("Grapple");
            if ((numericValue & 256) != 0) parts.Add("bit8(unknown-256)");
            if ((numericValue & 512) != 0) parts.Add("bit9(unknown-512)");
            if ((numericValue & 1024) != 0) parts.Add("bit10(unknown-1024)");
            if ((numericValue & 2048) != 0) parts.Add("bit11(unknown-2048)");
            long remainingBits = numericValue & ~0xFFFFL;
            if (remainingBits != 0)
                parts.Add("unresolved-bits-" + remainingBits.ToString(CultureInfo.InvariantCulture));

            return rawValue + " = " + string.Join(" | ", parts);
        }

        private static bool TryResolveExtDataVal1Semantic(string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 1)
                return false;

            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return false;

            // Only decode values 1 (Self) and 2 (Target) universally.
            // Values 3/4/5/7 are operation-specific and are handled by per-op structural handlers.
            string meaning;
            string valueNote;
            if (numericValue == 1)
            {
                meaning = "ApplicationTarget — Self: this sub-effect expression is applied to the caster.";
                valueNote = "Val1=1 (Self/caster) — present in 32% of all populated ExtData slots globally. Most common in INTERRUPT, AP_CHANGE, AP_REGEN_CHANGE, and HEAL sub-ops.";
            }
            else if (numericValue == 2)
            {
                meaning = "ApplicationTarget — Target: this sub-effect expression is applied to the primary target.";
                valueNote = "Val1=2 (Target/enemy) — present in 54% of all populated ExtData slots globally. Most common in DAMAGE, STAT_CHANGE, and debuff sub-ops.";
            }
            else
            {
                // Fall through to per-operation structural handlers for values 3, 4, 5, 7, etc.
                return false;
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ExtDataApplicationTarget",
                Meaning = meaning,
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val1 pattern analysis (18,526 extracted client BIN component records)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Val1 encodes the application target for the sub-effect in this ext-data slot. "
                    + "Val1=1 (Self) and Val1=2 (Target) account for ~86% of all populated ExtData slots globally. "
                    + "Val1=3/4/5/7 are operation-specific and remain unresolved as universal semantics. "
                    + valueNote
            };
            return true;
        }

        // Operations with dedicated Val1 handlers — skip universal decode for these so their specific handler fires.
        // 4=DAMAGE_CHANGE, 8=VELOCITY, 13=EVENT_LISTENER, 16=DEFENSIVE_STAT_CHANGE, 20=COOLDOWN_CHANGE,
        // 21=CASTTIME_CHANGE, 22=BONUS_TYPE_ADJUST, 38=IMMUNITY.
        private static readonly HashSet<uint> _operationsWithDedicatedVal1Handlers = new HashSet<uint> { 4, 8, 13, 16, 20, 21, 22, 38 };

        private static bool TryBuildExtDataVal1Inference(uint operationId, string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            // Skip operations with dedicated Val1 handlers — they fire after this in the dispatch chain.
            if (_operationsWithDedicatedVal1Handlers.Contains(operationId))
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 1)
                return false;

            int selfCount = 0;
            int targetCount = 0;
            int thirdCount = 0;
            int nonZeroCount = 0;
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (!long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    continue;
                if (num == 0) continue; // ignore zero-value slots
                nonZeroCount++;
                if (num == 1) selfCount++;
                else if (num == 2) targetCount++;
                else if (num == 3) thirdCount++;
            }

            if (nonZeroCount == 0)
                return false;

            // Compute percentages over non-zero observations only; zero-value slots carry no scope information.
            int pctSelfTarget = (selfCount + targetCount) * 100 / nonZeroCount;
            int pctAll3 = (selfCount + targetCount + thirdCount) * 100 / nonZeroCount;
            bool hasThirdVariant = thirdCount > 0 && pctAll3 >= 90;

            // Fire if 1+2 account for ≥80% of non-zero observations (original rule),
            // OR if 1+2+3 account for ≥90% of non-zero observations and 3 is not dominant.
            if (pctSelfTarget < 80 && !(hasThirdVariant && thirdCount * 100 / nonZeroCount < 50))
                return false;

            string dominantValues = BuildDominantRawValueSummary(observations, 5);
            string val3Note = hasThirdVariant && thirdCount > 0
                ? " Val1=3 (" + (thirdCount * 100 / nonZeroCount).ToString(CultureInfo.InvariantCulture) + "%) = tertiary scope variant (area, broadcast, or operation-specific third target; retail name unresolved)."
                : " Val1=3/4/5/7 are operation-specific variants with unresolved universal semantics.";

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val1 — ApplicationTarget: 1=Self (caster), 2=Target"
                    + (hasThirdVariant ? ", 3=tertiary scope variant." : "."),
                Confidence = SemanticConfidence.Inferred,
                Notes = "Val1 encodes the application target for the sub-effect in this ext-data slot. "
                    + "Val1=1 (Self/caster) and Val1=2 (Target/enemy) are the two primary universal values (account for " + pctSelfTarget.ToString(CultureInfo.InvariantCulture) + "% of non-zero observations). "
                    + "Val1=1 is most common in INTERRUPT, AP_CHANGE, and HEAL sub-ops; Val1=2 in DAMAGE, STAT_CHANGE, and debuff sub-ops. "
                    + val3Note
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryResolveExtDataVal2Semantic(uint operationId, string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 2)
                return false;

            string valueNote = BuildExtDataVal2ValueNote(operationId, rawValue);
            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ComponentOperation",
                Meaning = "Operation-type code identifying what kind of effect this ext-data expression slot describes.",
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val2 pattern analysis (extracted client BIN)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Across all component operations in extracted client BIN data, ExtData Val2 consistently holds a ComponentOperation-family type code. "
                    + "Values in the known range map directly to ComponentOperation names. "
                    + "Values above the known range represent operation type codes not yet named in the current enum. "
                    + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : valueNote)
            };
            return true;
        }

        private static string BuildExtDataVal2ValueNote(uint operationId, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            long numericValue;
            if (!long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue) || numericValue < 0)
                return string.Empty;

            if (numericValue == 0)
                return "Value 0 = NONE / inactive slot.";

            string opName = DefinitionCatalog.DescribeComponentOperationValue((uint)numericValue);
            if (!string.IsNullOrWhiteSpace(opName) && !opName.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase))
                return "ComponentOperation: " + opName + " (op=" + numericValue.ToString(CultureInfo.InvariantCulture) + ").";

            return "Extended operation code " + numericValue.ToString(CultureInfo.InvariantCulture) + " — present in extracted client BIN data but not yet named in the ComponentOperation enum.";
        }

        private static bool TryResolveExtDataVal3Semantic(string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 3)
                return false;

            long numericValue = 0;
            bool hasNumeric = !string.IsNullOrWhiteSpace(rawValue)
                && long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue);

            string valueNote = string.Empty;
            if (hasNumeric)
            {
                switch ((int)numericValue)
                {
                    case 0: valueNote = "Val3=0: slot inactive / no profile assigned."; break;
                    case 1: valueNote = "Val3=1 (Direct): immediate single application on trigger."; break;
                    case 4: valueNote = "Val3=4 (Event-triggered): proc on event; Val7 encodes chance %, duration ms, or amount depending on sub-op type."; break;
                    case 5: valueNote = "Val3=5 (Periodic/recovery): periodic or stackable application."; break;
                    case 6: valueNote = "Val3=6 (Rate-modifier): ongoing resource or AP rate modification; Val7 is typically 0."; break;
                    case 7: valueNote = "Val3=7 (Conditional/damage-variant): common in DAMAGE ext-data; exact retail semantics unresolved."; break;
                    default: valueNote = "Val3=" + rawValue + " is an uncommon profile code present in client BIN data; exact retail semantics unresolved."; break;
                }
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ExtDataApplicationProfile",
                Meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val3 — application profile encoding the delivery mode of the sub-effect for this slot.",
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val3 pattern analysis (18,526 extracted client BIN component records)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Val3 encodes the same profile-type across all component operations in the extracted client BIN. "
                    + "Val3=1 (Direct, 66%): immediate single application on trigger. "
                    + "Val3=4 (Event-triggered, 8%): proc on event, often with a chance % or duration in Val7. "
                    + "Val3=5 (Periodic/recovery, 2%): periodic or stackable application. "
                    + "Val3=6 (Rate-modifier, 11%): ongoing resource/AP rate modification. "
                    + "Val3=7 (10%): frequently seen in DAMAGE-operation contexts; exact semantics unresolved. "
                    + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : valueNote)
            };
            return true;
        }

        private static bool TryBuildExtDataVal3Inference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 3)
                return false;

            int totalCount = observations.Count;
            int knownCount = 0;
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num)
                    && (num == 0 || num == 1 || num == 4 || num == 5 || num == 6 || num == 7))
                    knownCount++;
            }

            string coverageLine = knownCount.ToString(CultureInfo.InvariantCulture)
                + " of " + totalCount.ToString(CultureInfo.InvariantCulture)
                + " observed values (" + (totalCount > 0 ? (knownCount * 100 / totalCount).ToString(CultureInfo.InvariantCulture) : "0") + "%) map to a named application profile.";

            string dominantValues = BuildDominantRawValueSummary(observations, 6);

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val3 — application profile (delivery mode) for this ext-data sub-effect slot.",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Val3 encodes the same profile-type across all component operations. "
                    + "Val3=1 (Direct): 66% of all records. "
                    + "Val3=4 (Event-triggered): 8%. "
                    + "Val3=5 (Periodic/recovery): 2%. "
                    + "Val3=6 (Rate-modifier): 11%. "
                    + "Val3=7 (Conditional/damage-variant): 10%, most common in DAMAGE-operation ext-data. "
                    + coverageLine
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryResolveExtDataVal4Semantic(string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 4)
                return false;

            long numericValue = 0;
            bool hasNumeric = !string.IsNullOrWhiteSpace(rawValue)
                && long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue);

            string valueNote = string.Empty;
            if (hasNumeric)
            {
                if (numericValue == 8) valueNote = "Val4=8: standard layout — this is the expected value for the vast majority of ext-data slots.";
                else if (numericValue == 9) valueNote = "Val4=9: requirement-conditional variant — Val6 in this slot carries a RequirementId chain reference instead of a scalar payload.";
                else valueNote = "Val4=" + rawValue + " is an uncommon layout tag present in client BIN data.";
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ExtDataLayoutTag",
                Meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val4 — layout tag identifying the encoding variant for this ext-data slot.",
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val4 pattern analysis (18,526 extracted client BIN component records)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Across all component operations in extracted client BIN data, Val4 is almost always 8 (standard layout, 97% of records). "
                    + "Val4=9 (2%) marks a requirement-conditional variant where Val6 carries a RequirementId chain reference. "
                    + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : valueNote)
            };
            return true;
        }

        private static bool TryBuildExtDataVal4Inference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 4)
                return false;

            int totalCount = observations.Count;
            int knownCount = observations.Count(obs =>
            {
                long num;
                return long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num) && (num == 8 || num == 9);
            });

            string coverageLine = knownCount.ToString(CultureInfo.InvariantCulture)
                + " of " + totalCount.ToString(CultureInfo.InvariantCulture)
                + " observed values (" + (totalCount > 0 ? (knownCount * 100 / totalCount).ToString(CultureInfo.InvariantCulture) : "0") + "%) resolve to a named layout tag.";

            string dominantValues = BuildDominantRawValueSummary(observations, 4);

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val4 — layout tag for the ext-data slot encoding variant (8=Standard, 9=Requirement-conditional).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Val4 is almost always 8 (standard layout, 97% globally). "
                    + "Val4=9 marks requirement-conditional slots where Val6 carries a RequirementId chain reference. "
                    + coverageLine
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : " Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryResolveExtDataVal7Semantic(string fieldKey, string rawValue, BinaryComponentRecord componentRow, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 7)
                return false;

            // Look up the companion Val3 for this slot to give contextual semantics.
            int companionVal3 = -1;
            if (componentRow != null)
            {
                BinaryExtDataRecord companionSlot = componentRow.ExtData.FirstOrDefault(row => row.SlotIndex == slotIndex);
                if (companionSlot != null)
                    companionVal3 = companionSlot.Val3;
            }

            long numericValue = 0;
            bool hasNumeric = !string.IsNullOrWhiteSpace(rawValue)
                && long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue);

            string profileContext;
            string valueNote = string.Empty;
            switch (companionVal3)
            {
                case 1:
                    profileContext = "Val3=1 (Direct): Val7 is a single-application marker.";
                    if (hasNumeric)
                    {
                        if (numericValue == 1) valueNote = "Val7=1: standard single-application flag (present in 72% of Direct-profile slots).";
                        else if (numericValue == 0) valueNote = "Val7=0: inactive / no marker (present in 17% of Direct-profile slots).";
                        else valueNote = "Val7=" + rawValue + ": uncommon value for Direct-profile slots.";
                    }
                    break;
                case 6:
                    profileContext = "Val3=6 (Rate-modifier): Val7 is expected to be 0 — no discrete payload; AP/resource rate is encoded via companion fields.";
                    if (hasNumeric && numericValue != 0)
                        valueNote = "Val7=" + rawValue + " is unusual for Rate-modifier profile slots (85% have Val7=0).";
                    break;
                case 7:
                    profileContext = "Val3=7 (Conditional/damage-variant): Val7 is expected to be 0 for this profile.";
                    if (hasNumeric && numericValue != 0)
                        valueNote = "Val7=" + rawValue + " is unusual for Conditional-profile slots (99% have Val7=0).";
                    break;
                case 4:
                    profileContext = "Val3=4 (Event-triggered): Val7 encodes a context-dependent payload — proc chance %, duration ms, or heal/damage amount depending on the sub-op type (Val2).";
                    if (hasNumeric)
                        valueNote = "Val7=" + rawValue + " — interpret in context of Val2 sub-op type; common values include 5, 10, 25 (chances), 1080, 1800 (duration ms).";
                    break;
                case 5:
                    profileContext = "Val3=5 (Periodic/recovery): Val7 encodes an amount or count for periodic application.";
                    if (hasNumeric)
                        valueNote = "Val7=" + rawValue + " — common values include 1, 25, 50, 75 (amounts or percentages).";
                    break;
                default:
                    profileContext = companionVal3 >= 0
                        ? "Val3=" + companionVal3.ToString(CultureInfo.InvariantCulture) + " (uncommon profile): Val7 semantics unresolved for this profile code."
                        : "Val3 companion not available; Val7 is context-dependent on the application profile (Val3) for this slot.";
                    break;
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ExtDataVal7Payload",
                Meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val7 — context-dependent payload field; semantics vary by application profile (Val3).",
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val7 pattern analysis (18,526 extracted client BIN component records)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Val7 is context-dependent on the application profile (Val3) in the same ext-data slot. "
                    + profileContext
                    + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
            };
            return true;
        }

        private static bool TryBuildExtDataVal7Inference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 7)
                return false;

            int totalCount = observations.Count;
            int zeroCount = 0;
            int oneCount = 0;
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (!long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    continue;
                if (num == 0) zeroCount++;
                else if (num == 1) oneCount++;
            }
            int pctZero = totalCount > 0 ? zeroCount * 100 / totalCount : 0;
            int pctOne = totalCount > 0 ? oneCount * 100 / totalCount : 0;

            string dominantValues = BuildDominantRawValueSummary(observations, 8);

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val7 — context-dependent payload field; semantics vary by application profile (Val3) in the same slot.",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Val7 is context-dependent on Val3 (application profile) in the same ext-data slot. "
                    + "Val3=1 (Direct, 66%): Val7=1 is a single-application marker (72% of Direct slots), Val7=0 means inactive (17%). "
                    + "Val3=4 (Event-triggered, 8%): Val7 encodes proc chance %, duration ms, or heal/damage amount depending on sub-op type (Val2). "
                    + "Val3=5 (Periodic, 2%): Val7 encodes an amount or count. "
                    + "Val3=6 (Rate-modifier, 11%): Val7 is expected to be 0 (85% of Rate-modifier slots). "
                    + "Val3=7 (Conditional, 10%): Val7 is expected to be 0 (99% of Conditional slots). "
                    + "Across all profiles for this slot: Val7=0 is " + pctZero.ToString(CultureInfo.InvariantCulture) + "% and Val7=1 is " + pctOne.ToString(CultureInfo.InvariantCulture) + "% of observations. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryResolveExtDataVal5Semantic(string fieldKey, string rawValue, BinaryComponentRecord componentRow, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 5)
                return false;

            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return false;

            if (numericValue != 0 && numericValue != 3)
                return false;

            // Look up companion Val2 and Val3 for context.
            int companionVal2 = -1;
            int companionVal3 = -1;
            if (componentRow != null)
            {
                BinaryExtDataRecord companionSlot = componentRow.ExtData.FirstOrDefault(row => row.SlotIndex == slotIndex);
                if (companionSlot != null)
                {
                    companionVal2 = companionSlot.Val2;
                    companionVal3 = companionSlot.Val3;
                }
            }

            string meaning;
            string valueNote;
            if (numericValue == 0)
            {
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val5 — inactive/default (no secondary sub-mode active for this ext-data expression).";
                valueNote = "Val5=0: inactive, present in 82% of all populated ExtData slots globally.";
            }
            else
            {
                // numericValue == 3
                string contextNote = string.Empty;
                if (companionVal2 == 10 && companionVal3 == 7)
                    contextNote = " Most commonly occurs in RESSURRECT (Val2=10) Conditional-profile (Val3=7) expressions where it appears in 97% of slots.";
                else if (companionVal2 == 6)
                    contextNote = " Occurs in AP_CHANGE (Val2=6) expressions.";
                else if (companionVal2 == 3)
                    contextNote = " Occurs in HEAL (Val2=3) expressions.";
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val5 — secondary sub-mode selector (Val5=3).";
                valueNote = "Val5=3: secondary sub-mode discriminant, present in 16% of all populated ExtData slots globally." + contextNote;
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ExtDataVal5SubMode",
                Meaning = meaning,
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val5 pattern analysis (18,526 extracted client BIN component records)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Val5=0 (82% globally) and Val5=3 (16% globally) account for 98%+ of all populated ExtData slots. "
                    + "Val5=3 is most dominant in RESSURRECT Conditional expressions (Val2=10, Val3=7), appearing in 97% of those slots. "
                    + valueNote
            };
            return true;
        }

        private static bool TryBuildExtDataVal5Inference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 5)
                return false;

            int totalCount = observations.Count;
            int zeroCount = 0;
            int threeCount = 0;
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (!long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    continue;
                if (num == 0) zeroCount++;
                else if (num == 3) threeCount++;
            }
            int pctKnown = totalCount > 0 ? (zeroCount + threeCount) * 100 / totalCount : 0;
            string dominantValues = BuildDominantRawValueSummary(observations, 6);

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val5 — secondary sub-mode discriminant (Val5=0=inactive, Val5=3=secondary-mode).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Val5=0 (82% globally) and Val5=3 (16% globally) account for 98%+ of all populated ExtData slots. "
                    + "Val5=3 is most dominant in RESSURRECT Conditional (Val2=10, Val3=7) expressions (97% of those slots); "
                    + "also present in AP_CHANGE and HEAL expressions across multiple application profiles. "
                    + pctKnown.ToString(CultureInfo.InvariantCulture) + "% of observations for this slot are Val5=0 or Val5=3. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryResolveExtDataVal6Semantic(string fieldKey, string rawValue, BinaryComponentRecord componentRow, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 6)
                return false;

            // Look up companion Val4 (layout tag) for context.
            int companionVal4 = 8; // default to standard
            if (componentRow != null)
            {
                BinaryExtDataRecord companionSlot = componentRow.ExtData.FirstOrDefault(row => row.SlotIndex == slotIndex);
                if (companionSlot != null)
                    companionVal4 = companionSlot.Val4;
            }

            long numericValue = 0;
            bool hasNumeric = !string.IsNullOrWhiteSpace(rawValue)
                && long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue);

            string meaning;
            string valueNote;

            if (companionVal4 == 9)
            {
                // Val4=9: Val6 should carry a RequirementId, handled by requirement linkage handler earlier.
                // This branch fires when Val6 did not match a known RequirementId.
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val6 — RequirementId reference (Val4=9 requirement-conditional layout).";
                valueNote = hasNumeric && numericValue == 0
                    ? "Val6=0: no RequirementId linked — this requirement-conditional slot has no active requirement chain."
                    : "Val6=" + rawValue + ": RequirementId reference; the referenced requirement row was not found in abilityrequirementexport.bin.";
            }
            else if (!hasNumeric || numericValue == 0)
            {
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val6 — inactive (no scalar payload or reference in this slot).";
                valueNote = "Val6=0: no payload active (present in 60% of all populated ExtData slots globally).";
            }
            else
            {
                // Non-zero Val6 on a standard (Val4=8) slot.
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val6 — scalar or ID reference payload; exact semantics are operation- and sub-op-specific.";
                valueNote = "Val6=" + rawValue + ": non-zero payload. May encode an ability ID, component ID, or scalar amount depending on the operation and application profile.";
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ExtDataVal6Payload",
                Meaning = meaning,
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val6 pattern analysis (18,526 extracted client BIN component records)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Val6 is a context-dependent payload field. "
                    + "For Val4=9 (requirement-conditional) slots, Val6 carries a RequirementId chain reference. "
                    + "For Val4=8 (standard) slots, Val6 is inactive (0) in 60% of all populated slots; non-zero values encode operation-specific scalars or ID references. "
                    + valueNote
            };
            return true;
        }

        private static bool TryBuildExtDataVal6Inference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 6)
                return false;

            int totalCount = observations.Count;
            int zeroCount = observations.Count(obs =>
            {
                long num;
                return long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num) && num == 0;
            });
            int pctZero = totalCount > 0 ? zeroCount * 100 / totalCount : 0;
            string dominantValues = BuildDominantRawValueSummary(observations, 8);

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val6 — context-dependent payload: RequirementId reference (Val4=9) or scalar/ID payload (Val4=8, 0=inactive).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Val6 is context-dependent on the layout tag (Val4) in the same ext-data slot. "
                    + "Val4=9 (requirement-conditional): Val6 carries a RequirementId chain reference to abilityrequirementexport.bin. "
                    + "Val4=8 (standard, 97% of slots): Val6 is inactive (0) in 60% of all populated slots; non-zero values encode operation-specific scalars or ID references. "
                    + "For this slot, " + pctZero.ToString(CultureInfo.InvariantCulture) + "% of observations have Val6=0 (inactive). "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryResolveExtDataVal8Semantic(string fieldKey, string rawValue, BinaryComponentRecord componentRow, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 8)
                return false;

            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return false;

            if (numericValue != 0 && numericValue != 1)
                return false;

            // Look up companion Val3 for context.
            int companionVal3 = -1;
            if (componentRow != null)
            {
                BinaryExtDataRecord companionSlot = componentRow.ExtData.FirstOrDefault(row => row.SlotIndex == slotIndex);
                if (companionSlot != null)
                    companionVal3 = companionSlot.Val3;
            }

            string meaning;
            string valueNote;
            if (numericValue == 0)
            {
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val8 — inactive/default (no proc-count or trigger-count override).";
                valueNote = "Val8=0: inactive, present in 94% of all populated ExtData slots globally.";
            }
            else
            {
                // numericValue == 1
                string contextNote = string.Empty;
                if (companionVal3 == 4)
                    contextNote = " Most common in Event-triggered (Val3=4) sub-ops (37% of those slots) — likely a single-proc or one-application limit.";
                else if (companionVal3 == 5)
                    contextNote = " Occurs in Periodic (Val3=5) sub-ops (7%).";
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val8 — proc-count or trigger-limit override (Val8=1 = single-proc limit).";
                valueNote = "Val8=1: single-instance trigger limit, present in 4% of all populated ExtData slots globally." + contextNote;
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ExtDataVal8TriggerCount",
                Meaning = meaning,
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val8 pattern analysis (18,526 extracted client BIN component records)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Val8=0 (94% globally) and Val8=1 (4% globally) account for 98%+ of all populated ExtData slots. "
                    + "Val8=1 is most notable in Event-triggered (Val3=4) sub-ops where it appears in 37% of those slots. "
                    + valueNote
            };
            return true;
        }

        private static bool TryBuildExtDataVal8Inference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 8)
                return false;

            int totalCount = observations.Count;
            int zeroCount = 0;
            int oneCount = 0;
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (!long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    continue;
                if (num == 0) zeroCount++;
                else if (num == 1) oneCount++;
            }
            int pctKnown = totalCount > 0 ? (zeroCount + oneCount) * 100 / totalCount : 0;
            string dominantValues = BuildDominantRawValueSummary(observations, 6);

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val8 — proc-count/trigger-limit field (Val8=0=inactive, Val8=1=single-proc limit).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Val8=0 (94% globally) and Val8=1 (4% globally) account for 98%+ of all populated ExtData slots. "
                    + "Val8=1 is most notable in Event-triggered (Val3=4) sub-ops at 37% of those slots, likely encoding a single-proc limit. "
                    + pctKnown.ToString(CultureInfo.InvariantCulture) + "% of observations for this slot are Val8=0 or Val8=1. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryResolveExtDataVal9Semantic(string fieldKey, string rawValue, BinaryComponentRecord componentRow, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 9)
                return false;

            long numericValue;
            if (string.IsNullOrWhiteSpace(rawValue)
                || !long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue))
                return false;

            if (numericValue != 0 && numericValue != 1)
                return false;

            // Look up companion Val3 for context.
            int companionVal3 = -1;
            if (componentRow != null)
            {
                BinaryExtDataRecord companionSlot = componentRow.ExtData.FirstOrDefault(row => row.SlotIndex == slotIndex);
                if (companionSlot != null)
                    companionVal3 = companionSlot.Val3;
            }

            string meaning;
            string valueNote;
            if (numericValue == 0)
            {
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val9 — inactive (binary modifier flag not set).";
                valueNote = "Val9=0: inactive, present in 79% of all populated ExtData slots globally.";
            }
            else
            {
                string contextNote = string.Empty;
                if (companionVal3 == 6)
                    contextNote = " Notably common in Rate-modifier (Val3=6) sub-ops (42% of those slots).";
                else if (companionVal3 == 1)
                    contextNote = " Occurs in Direct (Val3=1) sub-ops (21% of those slots).";
                meaning = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val9 — binary modifier flag (set).";
                valueNote = "Val9=1: modifier flag set, present in 20% of all populated ExtData slots globally." + contextNote;
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ExtDataVal9Flag",
                Meaning = meaning,
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-operation ExtData Val9 pattern analysis (18,526 extracted client BIN component records)",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "Val9 is a byte field with a near-binary distribution: Val9=0 (79% globally) and Val9=1 (20% globally) account for 99%+ of all populated ExtData slots. "
                    + "Val9=1 is most common in Rate-modifier (Val3=6) sub-ops at 42%, and Direct (Val3=1) at 21%. "
                    + "Exact retail semantics of Val9=1 are unresolved but it behaves as a binary modifier toggle. "
                    + valueNote
            };
            return true;
        }

        private static bool TryBuildExtDataVal9Inference(string fieldKey, List<FieldObservation> observations, out FieldObservationInference inference)
        {
            inference = null;
            if (observations == null || observations.Count == 0)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex) || valueIndex != 9)
                return false;

            int totalCount = observations.Count;
            int zeroCount = 0;
            int oneCount = 0;
            foreach (FieldObservation obs in observations)
            {
                long num;
                if (!long.TryParse(obs.RawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out num))
                    continue;
                if (num == 0) zeroCount++;
                else if (num == 1) oneCount++;
            }
            int pctKnown = totalCount > 0 ? (zeroCount + oneCount) * 100 / totalCount : 0;
            string dominantValues = BuildDominantRawValueSummary(observations, 4);

            inference = new FieldObservationInference
            {
                SemanticSummary = "ExtData slot " + slotIndex.ToString(CultureInfo.InvariantCulture) + " Val9 — binary modifier flag (byte; Val9=0=inactive, Val9=1=modifier-set).",
                Confidence = SemanticConfidence.Inferred,
                Notes = "Val9 is a byte with near-binary distribution: Val9=0 (79% globally) and Val9=1 (20% globally) account for 99%+ of populated slots. "
                    + "Val9=1 is most common in Rate-modifier (Val3=6) sub-ops at 42%, suggesting it toggles a modifier variant. "
                    + pctKnown.ToString(CultureInfo.InvariantCulture) + "% of observations for this slot are Val9=0 or Val9=1. "
                    + (string.IsNullOrWhiteSpace(dominantValues) ? string.Empty : "Dominant observed values: " + dominantValues + ".")
            };
            return true;
        }

        private static bool TryResolveApplyAbilityExtDataKnownFieldSemantic(uint operationId, string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            if (operationId != 23)
                return false;

            int slotIndex;
            int valueIndex;
            if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                return false;

            long numericValue = 0;
            bool hasNumeric = !string.IsNullOrWhiteSpace(rawValue)
                && long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out numericValue);

            string meaning;
            string notes;
            string valueNote = string.Empty;

            switch (valueIndex)
            {
                case 1:
                    meaning = "APPLY_ABILITY ext-data application target: indicates whether the embedded sub-effect applies to the caster (self) or the ability target.";
                    notes = "Across 2075 APPLY_ABILITY component records in the extracted client BIN, ExtData Val1 consistently encodes the sub-effect application direction. "
                        + "Value 1 = Self (apply to the caster); Value 2 = Target (apply to the ability target or enemy). "
                        + "STAT_CHANGE and DAMAGE sub-ops use Val1=2 (target) in 98%+ of cases; "
                        + "INTERRUPT, AP_REGEN_CHANGE, AP_CHANGE, and HEAL sub-ops use Val1=1 (self) in 95%+ of cases.";
                    if (hasNumeric)
                    {
                        if (numericValue == 1) valueNote = "Val1=1: apply embedded sub-effect to SELF (the caster).";
                        else if (numericValue == 2) valueNote = "Val1=2: apply embedded sub-effect to TARGET (the ability target or enemy).";
                        else valueNote = "Val1=" + rawValue + " is an unrecognised application-target code for APPLY_ABILITY.";
                    }
                    break;

                default:
                    return false;
            }

            semantic = new ComponentFieldSemantic
            {
                DomainKey = "ApplyAbilityExtData",
                Meaning = meaning,
                Confidence = SemanticConfidence.Inferred,
                Source = "Cross-record ExtData pattern analysis of 2075 APPLY_ABILITY component rows in extracted client BIN",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = notes + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
            };
            return true;
        }

        private static bool TryResolveOperationSpecificStructuralSemantic(uint operationId, string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;

            if (operationId == 2)
            {
                if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                {
                    string decodedBits = BuildStatChangeFlagsRawBitDecode(rawValue);
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "STAT_CHANGE stat-type bitmask — bit N identifies which Stat (Stats enum value N) this component modifies.",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "STAT_CHANGE FlagsRaw bit analysis + Stats server enum (Common/Database/GameData.cs)",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "FlagsRaw encodes the stat type(s) being modified using a bitmask where bit position N = Stats enum value N. "
                            + "Key stats: bit 1=Strength, bit 2=Agility, bit 3=Willpower, bit 4=Toughness, bit 5=Wounds, "
                            + "bit 6=Initiative, bit 7=WeaponSkill, bit 8=BallisticSkill, bit 9=Intelligence, "
                            + "bit 14=SpiritResistance, bit 15=ElementalResistance, bit 16=CorporealResistance. "
                            + (string.IsNullOrWhiteSpace(decodedBits) ? string.Empty : "Current value: " + decodedBits + ".")
                    };
                    return true;
                }
            }

            if (operationId == 1)
            {
                if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                {
                    long frVal;
                    string frNote = (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out frVal) && frVal != 0)
                        ? "Current value " + rawValue + " = " + (frVal == 1 ? "bit0 set (11% of DAMAGE records)." : frVal == 3 ? "bits 0+1 set (0.25%)." : frVal == 2 ? "bit1 set (0.15%)." : "non-standard value.")
                        : "Current value = 0 (inactive, dominant at 88%).";
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "DAMAGE FlagsRaw — sparse 2-bit modifier field (88% zero; 1=bit0-set 11%, 2=bit1-set rare, 3=both-bits rare).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "DAMAGE FlagsRaw pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "FlagsRaw is 0 in 88% of DAMAGE records. Bit 0 (value=1) is the main active flag at 11.4%. Bit semantics are unresolved. " + frNote
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
                {
                    string v2Note = string.Equals(rawValue, "7", StringComparison.OrdinalIgnoreCase) ? "Value=7 = sub-type-7 (dominant non-zero, 13.4%)."
                        : string.Equals(rawValue, "6", StringComparison.OrdinalIgnoreCase) ? "Value=6 = sub-type-6 (second most common non-zero, 11.9%)."
                        : string.Equals(rawValue, "0", StringComparison.OrdinalIgnoreCase) ? "Value=0 = inactive (74%)."
                        : "Value=" + rawValue + " = rare DAMAGE sub-type variant.";
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "DAMAGE sub-type/variant selector (74% zero; 7=type-7 13%, 6=type-6 12%).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "DAMAGE Value[2] pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[2] selects a DAMAGE sub-type or variant. " + v2Note + " Retail names for values 6 and 7 are unresolved."
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase))
                {
                    string v3Note = string.Equals(rawValue, "7", StringComparison.OrdinalIgnoreCase) ? "Value=7 = profile-7 (dominant non-zero, 12.4%)."
                        : string.Equals(rawValue, "6", StringComparison.OrdinalIgnoreCase) ? "Value=6 = profile-6 (1.8%)."
                        : string.Equals(rawValue, "0", StringComparison.OrdinalIgnoreCase) ? "Value=0 = inactive (86%)."
                        : "Value=" + rawValue + " = rare DAMAGE profile variant.";
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "DAMAGE profile selector (86% zero; 7=profile-7 12%, 6=profile-6 2%).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "DAMAGE Value[3] pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[3] selects a DAMAGE profile or secondary variant. " + v3Note + " Retail names for values 6 and 7 are unresolved."
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
                {
                    string dmgBits = BuildCcFlagsRawBitDecode(rawValue);
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "DAMAGE CrowdControlTypes interaction flag — encodes CC condition linked to this damage component.",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "DAMAGE Value15 bit analysis + CrowdControlTypes server enum",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value15 is 0 (inactive) in 70% of DAMAGE records. "
                            + "Non-zero values use CrowdControlTypes bit encoding: 1=Snare(16%), 16=Knockdown(10%), 64=bit6(1.1%), 4=Disarm(0.3%). "
                            + (string.IsNullOrWhiteSpace(dmgBits) ? string.Empty : "Current value: " + dmgBits + ".")
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value[4]", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "DAMAGE Value[4] — sparse sub-type or variant selector (4 non-zero records; values 3 and 6).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "DAMAGE Value[4] pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[4] is non-zero in only 4 DAMAGE records (values 3 and 6). Retail name unresolved. Current value: " + (rawValue ?? "0") + "."
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(fieldKey, "Value[6]", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "DAMAGE " + fieldKey + " — near-inactive modifier field (4 non-zero records; single value = 90).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "DAMAGE " + fieldKey + " pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = fieldKey + " is non-zero in only 4 DAMAGE records (all value=90). Likely a 90% percentage modifier. "
                            + "Retail name unresolved. Current value: " + (rawValue ?? "0") + "."
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value[7]", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "DAMAGE Value[7] — likely a chance percentage or conditional scalar (27 non-zero records; values mostly 5–100).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "DAMAGE Value[7] pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[7] is non-zero in 27 DAMAGE records. Observed values (5, 10, 25, 50, 70–95, 500) suggest a chance/proc percentage or modifier. "
                            + "Retail name unresolved. Current value: " + (rawValue ?? "0") + "."
                    };
                    return true;
                }

                // DAMAGE Multiplier[N]: percentage scaling multiplier for this damage component.
                {
                    int multiplierIndex;
                    if (TryParseIndexedField(fieldKey, "Multiplier[", out multiplierIndex))
                    {
                        semantic = new ComponentFieldSemantic
                        {
                            DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                            Meaning = "DAMAGE " + fieldKey + " — percentage scaling multiplier (100 = no change; non-100 values scale damage up/down).",
                            Confidence = SemanticConfidence.Inferred,
                            Source = "DAMAGE multiplier pattern analysis",
                            SourcePath = string.Empty,
                            SourceLocation = string.Empty,
                            Notes = "DAMAGE Multiplier[" + multiplierIndex.ToString(CultureInfo.InvariantCulture) + "] carries meaningful scaling values unlike passive-op multipliers. "
                                + "100 = baseline/no scaling. Retail slot-role name unresolved. Current value: " + (rawValue ?? "0") + "."
                        };
                        return true;
                    }
                }

                int slotIndex;
                int valueIndex;
                if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                    return false;

                string semanticSummary;
                string roleNotes;
                if (!TryDescribeDamageExtDataField(valueIndex, out semanticSummary, out roleNotes))
                    return false;

                string valueNote = BuildDamageStructuralValueNote(valueIndex, rawValue);
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = semanticSummary,
                    Confidence = SemanticConfidence.Structural,
                    Source = "DAMAGE structural inference",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "This extracted-client-only inference comes from recurring DAMAGE ext-data layouts in slot "
                        + slotIndex.ToString(CultureInfo.InvariantCulture)
                        + ". "
                        + roleNotes
                        + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                        + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
                };
                return true;
            }

            if (operationId == 22)
            {
                if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
                {
                    string btaBits = BuildCcFlagsRawBitDecode(rawValue);
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "BONUS_TYPE_ADJUST CrowdControlTypes gate flag — encodes CC condition gating this bonus-type adjustment.",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "BONUS_TYPE_ADJUST Value15 bit analysis + CrowdControlTypes server enum",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value15 is 0 (inactive) in 41% of BONUS_TYPE_ADJUST records. "
                            + "Non-zero values use CrowdControlTypes bit encoding: 4=Disarm(55%), 8=Silence(4%). "
                            + (string.IsNullOrWhiteSpace(btaBits) ? string.Empty : "Current value: " + btaBits + ".")
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "BONUS_TYPE_ADJUST Value08 — binary activation flag (value=1 in 696 records; only one distinct non-zero value).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "BONUS_TYPE_ADJUST Value08 pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value08 has exactly one non-zero value (1) across all BONUS_TYPE_ADJUST records. "
                            + "Functions as a binary toggle or mode flag. Retail name unresolved. Current value: " + (rawValue ?? "0") + "."
                    };
                    return true;
                }

                int slotIndex;
                int valueIndex;
                if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                    return false;

                // BONUS_TYPE_ADJUST Val1: upgrade from Structural to Inferred.
                if (valueIndex == 1)
                {
                    long btaVal1;
                    string btaVal1Meaning = null;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out btaVal1))
                    {
                        if (btaVal1 == 3) btaVal1Meaning = "Val1=3 = primary BTA modifier scope (55% of BTA ExtData slots).";
                        else if (btaVal1 == 2) btaVal1Meaning = "Val1=2 = target-application (34%; universal target decode).";
                        else if (btaVal1 == 1) btaVal1Meaning = "Val1=1 = self-application (10%; universal self decode).";
                        else if (btaVal1 == 4) btaVal1Meaning = "Val1=4 = rare BTA variant (<1%).";
                    }
                    if (btaVal1Meaning != null)
                    {
                        semantic = new ComponentFieldSemantic
                        {
                            DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                            Meaning = "BONUS_TYPE_ADJUST ExtData Val1 — application target/scope selector (3=modifier-scope 55%, 2=target 34%, 1=self 10%).",
                            Confidence = SemanticConfidence.Inferred,
                            Source = "BONUS_TYPE_ADJUST Val1 pattern analysis",
                            SourcePath = string.Empty,
                            SourceLocation = string.Empty,
                            Notes = btaVal1Meaning + " Retail name for Val1=3 is unresolved."
                        };
                        return true;
                    }
                }

                string semanticSummary;
                string roleNotes;
                if (!TryDescribeBonusTypeAdjustExtDataField(valueIndex, out semanticSummary, out roleNotes))
                    return false;

                string valueNote = BuildBonusTypeAdjustStructuralValueNote(valueIndex, rawValue);
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = semanticSummary,
                    Confidence = SemanticConfidence.Structural,
                    Source = "BONUS_TYPE_ADJUST structural inference",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "This extracted-client-only inference comes from recurring BONUS_TYPE_ADJUST ext-data layouts in slot "
                        + slotIndex.ToString(CultureInfo.InvariantCulture)
                        + ". "
                        + roleNotes
                        + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                        + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
                };
                return true;
            }

            if (operationId == 23)
            {
                if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                {
                    string decodedBits = BuildCcFlagsRawBitDecode(rawValue);
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "CrowdControlTypes gate flags — CC types this APPLY_ABILITY sub-op is conditioned on or restricted by (typically 0 for unconditional applies).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "APPLY_ABILITY FlagsRaw bit analysis + CrowdControlTypes server enum (Common/Database/GameData.cs)",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "87% of APPLY_ABILITY records have FlagsRaw=0 (unconditional). "
                            + "Non-zero values use the same CrowdControlTypes bit encoding as CC FlagsRaw: "
                            + "bit 0=Snare(1), bit 1=Root(2), bit 2=Disarm(4), bit 3=Silence(8), bit 4=Knockdown(16), bit 5=Stagger(32), bit 7=Grapple(128). "
                            + "Common non-zero values: 16=Knockdown-gate(3%), 1=Snare-gate(1%), 4=Disarm-gate(1%), 7=MoveImpedance(1%). "
                            + (string.IsNullOrWhiteSpace(decodedBits) ? string.Empty : "Current value: " + decodedBits + ".")
                    };
                    return true;
                }

                string applyAbilityValueSummary;
                string applyAbilityValueRoleNotes;
                if (TryDescribeApplyAbilityValueField(fieldKey, out applyAbilityValueSummary, out applyAbilityValueRoleNotes))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = applyAbilityValueSummary,
                        Confidence = SemanticConfidence.Inferred,
                        Source = "APPLY_ABILITY value distribution analysis (2,075 extracted client BIN records)",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = applyAbilityValueRoleNotes
                            + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                    };
                    return true;
                }

                int slotIndex;
                int valueIndex;
                if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                    return false;

                string semanticSummary;
                string roleNotes;
                if (!TryDescribeApplyAbilityExtDataField(valueIndex, out semanticSummary, out roleNotes))
                    return false;

                string valueNote = BuildApplyAbilityStructuralValueNote(valueIndex, rawValue);
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = semanticSummary,
                    Confidence = SemanticConfidence.Structural,
                    Source = "APPLY_ABILITY structural inference",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "This extracted-client-only inference comes from recurring APPLY_ABILITY ext-data layouts in slot "
                        + slotIndex.ToString(CultureInfo.InvariantCulture)
                        + ". "
                        + roleNotes
                        + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                        + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
                };
                return true;
            }

            if (operationId == 24)
            {
                if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "KNOCKBACK FlagsRaw — sparse modifier bitfield (96% zero; non-zero values: 2=bit1-set, 127=bits0-6-set, 1=bit0-set).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "KNOCKBACK FlagsRaw pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "FlagsRaw is 0 in 252 of 262 KNOCKBACK records. "
                            + "Non-zero values: 2=bit1(5 records), 127=bits0-6(4 records), 1=bit0(1 record). "
                            + "Bit semantics are unresolved. "
                            + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : "Current value: " + rawValue + ".")
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "KNOCKBACK Value08 — near-inactive field (261 of 262 records = 0; Value08=1 in 1 record).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "KNOCKBACK Value08 pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value08 is 0 in all but 1 KNOCKBACK record. "
                            + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : "Current value: " + rawValue + ".")
                    };
                    return true;
                }

                string semanticSummary;
                string roleNotes;
                if (TryDescribeKnockbackValueField(fieldKey, out semanticSummary, out roleNotes))
                {
                    string knockbackValueNote = BuildKnockbackValueFieldNote(fieldKey, rawValue);
                    string knockbackFieldContext = BuildKnockbackValueFieldAggregateDecode(fieldKey);
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = semanticSummary,
                        Confidence = SemanticConfidence.Inferred,
                        Source = "KNOCKBACK value pattern analysis (262 extracted client BIN records)",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Inferred from KNOCKBACK value patterns in extracted client BIN rows. "
                            + roleNotes
                            + (string.IsNullOrWhiteSpace(knockbackFieldContext) ? string.Empty : " " + knockbackFieldContext)
                            + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                            + (string.IsNullOrWhiteSpace(knockbackValueNote) ? string.Empty : " " + knockbackValueNote)
                    };
                    return true;
                }

                int slotIndex;
                int valueIndex;
                if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                    return false;

                if (!TryDescribeKnockbackExtDataField(valueIndex, out semanticSummary, out roleNotes))
                    return false;

                string valueNote = BuildKnockbackStructuralValueNote(valueIndex, rawValue);
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = semanticSummary,
                    Confidence = SemanticConfidence.Structural,
                    Source = "KNOCKBACK structural inference",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "This extracted-client-only inference comes from recurring KNOCKBACK ext-data layouts in slot "
                        + slotIndex.ToString(CultureInfo.InvariantCulture)
                        + ". "
                        + roleNotes
                        + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                        + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
                };
                return true;
            }

            if (operationId == 38)
            {
                if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
                {
                    string immunityValue0Note = BuildImmunityValue0Note(rawValue);
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "OperationType code — identifies which effect category this immunity component blocks.",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "IMMUNITY Value[0] pattern analysis + component operation enum cross-reference",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[0] encodes the ComponentOperation type being blocked: 12=CC(41%), 1=DAMAGE(15%), 7=op7(8%), 24=KNOCKBACK(4%). "
                            + (string.IsNullOrWhiteSpace(immunityValue0Note) ? string.Empty : immunityValue0Note)
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value[1]", StringComparison.OrdinalIgnoreCase))
                {
                    string strengthNote = string.Equals(rawValue, "100", StringComparison.OrdinalIgnoreCase)
                        ? "Value=100 = full immunity (dominant at 90%)."
                        : string.Equals(rawValue, "0", StringComparison.OrdinalIgnoreCase)
                            ? "Value=0 = no strength applied (9% of IMMUNITY records)."
                            : "Value=" + rawValue + " = non-dominant immunity strength level.";
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "Immunity strength level — 0–100 scale; 100 = full immunity (present in 90% of IMMUNITY records).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "IMMUNITY Value[1] pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[1] encodes the immunity strength. " + strengthNote
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value[2]", StringComparison.OrdinalIgnoreCase))
                {
                    string subTypeNote;
                    switch (rawValue ?? string.Empty)
                    {
                        case "0": subTypeNote = "Value=0 = no sub-type (dominant at 66%)."; break;
                        case "6": subTypeNote = "Value=6 = sub-type-6 (9%; retail name unresolved)."; break;
                        case "1": subTypeNote = "Value=1 = sub-type-1 (8%; retail name unresolved)."; break;
                        case "3": subTypeNote = "Value=3 = sub-type-3 (8%; retail name unresolved)."; break;
                        default: subTypeNote = "Value=" + rawValue + " = minority sub-type variant."; break;
                    }
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "Immunity sub-type selector — identifies an immunity sub-category variant.",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "IMMUNITY Value[2] pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[2] selects the immunity sub-type. " + subTypeNote
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value[3]", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(fieldKey, "Value[4]", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(fieldKey, "Value[5]", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "IMMUNITY " + fieldKey + " — near-inactive field (100% zero in all extracted IMMUNITY records).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "IMMUNITY value pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = fieldKey + " is zero in all extracted IMMUNITY component records; no active payload for this field."
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
                {
                    string immV08Note = string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase)
                        ? "Value=1 = flag set (present in 39% of IMMUNITY records)."
                        : "Value=0 = flag absent (61% of records).";
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "IMMUNITY Value08 — binary modifier flag (61% zero; value=1 in 66 of 169 records).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "IMMUNITY Value08 pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value08 is a boolean toggle or mode flag for IMMUNITY components. " + immV08Note + " Retail name unresolved."
                    };
                    return true;
                }

                if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
                {
                    string flagNote = string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase)
                        ? "Value=1 = flag set (present in 16% of IMMUNITY records)."
                        : "Value=0 = flag absent (dominant at 84%).";
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "IMMUNITY FlagsRaw — binary modifier flag (84% zero; value=1 in 27 of 169 records).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "IMMUNITY FlagsRaw pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "FlagsRaw has only one non-zero value (1 = bit 0) across all extracted IMMUNITY records. " + flagNote + " Retail name unresolved."
                    };
                    return true;
                }

                int slotIndex;
                int valueIndex;
                if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                    return false;

                // IMMUNITY ExtData Val1: Val1=3 (passive/wide-scope, 51%) and Val1=7 (active/trigger-specific, 33%)
                // are IMMUNITY-specific variants not covered by the universal Val1 decoder (which only handles 1/2).
                if (valueIndex == 1)
                {
                    long val1Numeric;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out val1Numeric)
                        && (val1Numeric == 3 || val1Numeric == 7))
                    {
                        string val1Meaning = val1Numeric == 3
                            ? "IMMUNITY application scope — Val1=3: passive/wide-scope application (51% of IMMUNITY ExtData slots)."
                            : "IMMUNITY application scope — Val1=7: active/trigger-specific application (33% of IMMUNITY ExtData slots).";
                        string val1Notes = "Val1=3 (51%) = passive or wide-scope immunity application. "
                            + "Val1=7 (33%) = active or trigger-specific immunity application. "
                            + "Val1=2 (13%) = standard target-application (handled by universal Val1 decoder). "
                            + "Retail names for Val1=3 and Val1=7 are unresolved.";
                        semantic = new ComponentFieldSemantic
                        {
                            DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                            Meaning = val1Meaning,
                            Confidence = SemanticConfidence.Inferred,
                            Source = "IMMUNITY ExtData Val1 pattern analysis",
                            SourcePath = string.Empty,
                            SourceLocation = string.Empty,
                            Notes = val1Notes
                        };
                        return true;
                    }
                }

                string semanticSummary;
                string roleNotes;
                if (!TryDescribeImmunityExtDataField(valueIndex, out semanticSummary, out roleNotes))
                    return false;

                string valueNote = BuildImmunityStructuralValueNote(valueIndex, rawValue);
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = semanticSummary,
                    Confidence = SemanticConfidence.Structural,
                    Source = "IMMUNITY structural inference",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "This extracted-client-only inference comes from recurring IMMUNITY ext-data layouts in slot "
                        + slotIndex.ToString(CultureInfo.InvariantCulture)
                        + ". "
                        + roleNotes
                        + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                        + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
                };
                return true;
            }

            if (operationId == 4)
            {
                // DAMAGE_CHANGE Value15: CrowdControlTypes gate flag.
                if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
                {
                    string dcBits = BuildCcFlagsRawBitDecode(rawValue);
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "DAMAGE_CHANGE CrowdControlTypes gate flag — encodes CC condition gating this damage-change modifier.",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "DAMAGE_CHANGE Value15 bit analysis + CrowdControlTypes server enum",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value15 uses CrowdControlTypes bit encoding: 4=Disarm(57%), 8=Silence(19%), 12=Disarm+Silence(14%), 36=Stagger+Disarm(10%). "
                            + "0 = no CC gate (inactive). "
                            + (string.IsNullOrWhiteSpace(dcBits) ? string.Empty : "Current value: " + dcBits + ".")
                    };
                    return true;
                }

                int slotIndex;
                int valueIndex;
                if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                    return false;

                // DAMAGE_CHANGE Val1: 3=49%, 2=36%, 1=11% — not covered by the universal Val1 decoder.
                if (valueIndex == 1)
                {
                    long dcVal1;
                    string dcVal1Meaning = null;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out dcVal1))
                    {
                        if (dcVal1 == 3) dcVal1Meaning = "Val1=3 = primary DAMAGE_CHANGE modifier scope (49% of DAMAGE_CHANGE ExtData slots).";
                        else if (dcVal1 == 2) dcVal1Meaning = "Val1=2 = target-application (36%; universal target decode).";
                        else if (dcVal1 == 1) dcVal1Meaning = "Val1=1 = self-application (11%; universal self decode).";
                    }
                    if (dcVal1Meaning != null)
                    {
                        semantic = new ComponentFieldSemantic
                        {
                            DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                            Meaning = "DAMAGE_CHANGE ExtData Val1 — application target/scope selector (3=modifier-scope 49%, 2=target 36%, 1=self 11%).",
                            Confidence = SemanticConfidence.Inferred,
                            Source = "DAMAGE_CHANGE Val1 pattern analysis",
                            SourcePath = string.Empty,
                            SourceLocation = string.Empty,
                            Notes = dcVal1Meaning + " Retail name for Val1=3 is unresolved."
                        };
                        return true;
                    }
                }

                return false;
            }

            if (operationId == 13)
            {
                // EVENT_LISTENER Value08: binary activation flag (all non-zero = 1).
                if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "EVENT_LISTENER Value08 — binary activation flag (value=1 in 414 records; only one distinct non-zero value).",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "EVENT_LISTENER Value08 pattern analysis",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value08 has exactly one non-zero value (1) across EVENT_LISTENER records. "
                            + "Functions as a binary toggle or mode flag. Retail name unresolved. Current value: " + (rawValue ?? "0") + "."
                    };
                    return true;
                }

                int slotIndex;
                int valueIndex;
                if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                    return false;

                // EVENT_LISTENER Val1: 4=63%, 2=34%, 1=2% — Val1=4 is EVENT_LISTENER-specific.
                if (valueIndex == 1)
                {
                    long elVal1;
                    string elVal1Meaning = null;
                    if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out elVal1))
                    {
                        if (elVal1 == 4) elVal1Meaning = "Val1=4 = primary EVENT_LISTENER scope (63% of EVENT_LISTENER ExtData slots).";
                        else if (elVal1 == 2) elVal1Meaning = "Val1=2 = target-scope (34%; universal target decode).";
                        else if (elVal1 == 1) elVal1Meaning = "Val1=1 = self-scope (2%; universal self decode).";
                    }
                    if (elVal1Meaning != null)
                    {
                        semantic = new ComponentFieldSemantic
                        {
                            DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                            Meaning = "EVENT_LISTENER ExtData Val1 — application target/scope selector (4=listener-scope 63%, 2=target 34%, 1=self 2%).",
                            Confidence = SemanticConfidence.Inferred,
                            Source = "EVENT_LISTENER Val1 pattern analysis",
                            SourcePath = string.Empty,
                            SourceLocation = string.Empty,
                            Notes = elVal1Meaning + " Retail name for Val1=4 is unresolved."
                        };
                        return true;
                    }
                }

                return false;
            }

            if (operationId == 28)
            {
                if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "GRANTED_ABILITY Value[0] — references the ability ID granted by this component.",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "GRANTED_ABILITY Value[0] pattern analysis (249 non-zero records, 239 distinct values)",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[0] holds a non-zero ability ID in 249 of extracted GRANTED_ABILITY records (239 distinct IDs). "
                            + "Distribution mirrors APPLY_ABILITY Value[0] (ability reference). "
                            + "Current raw value: " + (rawValue ?? "0") + "."
                    };
                    return true;
                }

                return false;
            }

            if (operationId == 35)
            {
                if (string.Equals(fieldKey, "Value[0]", StringComparison.OrdinalIgnoreCase))
                {
                    semantic = new ComponentFieldSemantic
                    {
                        DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                        Meaning = "SUMMON_MOUNT Value[0] — referenced mount or creature ID.",
                        Confidence = SemanticConfidence.Inferred,
                        Source = "SUMMON_MOUNT Value[0] pattern analysis (232 non-zero records, 186 distinct values)",
                        SourcePath = string.Empty,
                        SourceLocation = string.Empty,
                        Notes = "Value[0] holds a non-zero mount or creature ID in 232 of extracted SUMMON_MOUNT records (186 distinct IDs). "
                            + "High distinct-value count indicates a direct ID reference, not an enum. "
                            + "Current raw value: " + (rawValue ?? "0") + "."
                    };
                    return true;
                }

                return false;
            }

            if (operationId != 12)
                return false;

            string fieldSemanticSummary;
            string fieldRoleNotes;
            string fieldValueNote;
            string sourceLabel;

            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                string decodedBits = BuildCcFlagsRawBitDecode(rawValue);
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = "CCType bitmask — CrowdControlTypes flags indicating which CC conditions this component applies, modifies, or checks.",
                    Confidence = SemanticConfidence.Inferred,
                    Source = "CC FlagsRaw bit analysis + CrowdControlTypes server enum (Common/Database/GameData.cs)",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "Bit mapping from CrowdControlTypes enum: "
                        + "bit 0=Snare(1), bit 1=Root(2), bit 2=Disarm(4), bit 3=Silence(8), "
                        + "bit 4=Knockdown(16), bit 5=Stagger(32), bit 7=Grapple(128). "
                        + "Bit 6 (64) is not in the server enum and its retail semantics are unresolved. "
                        + "Bit 11 (2048) is present in 46% of CC records but its exact semantics are unresolved. "
                        + "Dominant CC values: 2175=AllStandardCC+Stagger+bit6+bit11(22%), 2303=All+bit11(15%), 8=Silence-only(14%), 1=Snare-only(12%). "
                        + (string.IsNullOrWhiteSpace(decodedBits) ? string.Empty : "Current value: " + decodedBits + ".")
                };
                return true;
            }
            else if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase))
            {
                string v15Note = string.Equals(rawValue, "4", StringComparison.OrdinalIgnoreCase)
                    ? "Value=4 = dominant CC application type (69% of records)."
                    : string.Equals(rawValue, "5", StringComparison.OrdinalIgnoreCase)
                        ? "Value=5 = rare CC variant (<1% of records)."
                        : string.Equals(rawValue, "0", StringComparison.OrdinalIgnoreCase)
                            ? "Value=0 = no type marker applied (31% of records)."
                            : "Value=" + rawValue + " = non-dominant CC type marker.";
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = "CC application type marker (4=dominant 69%, 0=absent 31%, 5=rare <1%).",
                    Confidence = SemanticConfidence.Inferred,
                    Source = "CC Value15 pattern analysis (586 extracted records)",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "Value15 is a low-cardinality companion selector for the CC component. " + v15Note + " Retail name unresolved."
                };
                return true;
            }
            else if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase))
            {
                string v08Note = string.Equals(rawValue, "1", StringComparison.OrdinalIgnoreCase)
                    ? "Value=1 = modifier flag set (present in 4% of CC records)."
                    : "Value=0 = flag absent (dominant at 96%).";
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = "CC binary modifier flag (96% zero; Value08=1 in 24 of 586 records).",
                    Confidence = SemanticConfidence.Inferred,
                    Source = "CC Value08 pattern analysis (586 extracted records)",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "Value08 is a boolean toggle or mode flag. " + v08Note + " Retail name unresolved."
                };
                return true;
            }
            else if (fieldKey.StartsWith("Value[", StringComparison.OrdinalIgnoreCase))
            {
                // CC Values[0-4] are near-inactive (99-100% zero in 586 CC records).
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = "CC " + fieldKey + " — near-inactive (99-100% zero across all 586 extracted CC records).",
                    Confidence = SemanticConfidence.Inferred,
                    Source = "CC value distribution analysis (586 extracted client BIN records)",
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = fieldKey + " is 0 in 99-100% of CC records. "
                        + "The rare non-zero cases (1 record has Value[1]=1, 1 has Value[3]=7) have unresolved semantics. "
                        + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : "Current raw value: " + rawValue + ".")
                };
                return true;
            }
            else
            {
                int slotIndex;
                int valueIndex;
                if (!TryParseExtDataField(fieldKey, out slotIndex, out valueIndex))
                    return false;

                if (!TryDescribeCcExtDataField(valueIndex, out fieldSemanticSummary, out fieldRoleNotes))
                    return false;

                fieldValueNote = BuildCcStructuralValueNote(fieldKey, valueIndex, rawValue);
                sourceLabel = "CC structural inference";
                semantic = new ComponentFieldSemantic
                {
                    DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                    Meaning = fieldSemanticSummary,
                    Confidence = SemanticConfidence.Structural,
                    Source = sourceLabel,
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "This extracted-client-only inference comes from recurring CC ext-data layouts in slot "
                        + slotIndex.ToString(CultureInfo.InvariantCulture)
                        + ". "
                        + fieldRoleNotes
                        + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                        + (string.IsNullOrWhiteSpace(fieldValueNote) ? string.Empty : " " + fieldValueNote)
                };
                return true;
            }
        }

        private static bool TryResolveNamedControlFieldSemantic(uint operationId, string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeNamedControlField(fieldKey, out semanticSummary, out roleNotes))
                return false;

            string valueNote = BuildNamedControlFieldValueNote(fieldKey, rawValue);
            semantic = new ComponentFieldSemantic
            {
                DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                Meaning = semanticSummary,
                Confidence = SemanticConfidence.Inferred,
                Source = "Named control field inference",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "This inferred meaning comes from the descriptive client field name and the observed value shapes in extracted client BIN rows. "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                    + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
                    + " Exact display formatting is not directly token-confirmed yet."
            };
            return true;
        }

        private static bool TryResolveGenericFlagsRawSemantic(uint operationId, string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;

            string semanticSummary;
            string roleNotes;
            if (!TryDescribeGenericFlagsRawField(fieldKey, out semanticSummary, out roleNotes))
                return false;

            string valueNote = BuildGenericFlagsRawValueNote(rawValue);
            semantic = new ComponentFieldSemantic
            {
                DomainKey = BuildOperationFieldDomainKey(operationId, fieldKey),
                Meaning = semanticSummary,
                Confidence = SemanticConfidence.Structural,
                Source = "FlagsRaw structural inference",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = "This extracted-client-only inference comes from the explicit client field name and the recurring raw bit-pattern values in the component rows. "
                    + roleNotes
                    + (string.IsNullOrWhiteSpace(rawValue) ? string.Empty : " Current raw value: " + rawValue + ".")
                    + (string.IsNullOrWhiteSpace(valueNote) ? string.Empty : " " + valueNote)
            };
            return true;
        }

        private static int CountSetBits(ulong value)
        {
            int bitCount = 0;
            while (value != 0)
            {
                if ((value & 1UL) != 0)
                    bitCount++;

                value >>= 1;
            }

            return bitCount;
        }

        private static string BuildSetBitSummary(ulong value, int limit)
        {
            if (value == 0)
                return "0";

            List<string> bits = new List<string>();
            int bitIndex = 0;
            int totalCount = 0;
            ulong remaining = value;

            while (remaining != 0)
            {
                if ((remaining & 1UL) != 0)
                {
                    totalCount++;
                    if (bits.Count < limit)
                        bits.Add(bitIndex.ToString(CultureInfo.InvariantCulture));
                }

                remaining >>= 1;
                bitIndex++;
            }

            if (bits.Count == 0)
                return string.Empty;

            string summary = string.Join(", ", bits);
            if (totalCount > bits.Count)
                summary += ", ...";

            return summary;
        }

        private static bool TryFormatFieldValue(long rawValue, out string valueText)
        {
            valueText = null;
            if (rawValue == 0)
                return false;

            valueText = rawValue.ToString(CultureInfo.InvariantCulture);
            return true;
        }

        private static ComponentOperationFieldCorrelationRecord BuildCompanionFieldCorrelation(string fieldKey, List<FieldObservation> observations, int selectedComponentCount)
        {
            if (string.IsNullOrWhiteSpace(fieldKey)
                || observations == null
                || observations.Count == 0
                || selectedComponentCount <= 0)
                return null;

            List<FieldValueCount> values = observations
                .GroupBy(row => row.RawValue, StringComparer.OrdinalIgnoreCase)
                .Select(group => new FieldValueCount
                {
                    RawValue = group.Key,
                    Count = group.Select(row => row.ComponentId).Distinct().Count()
                })
                .OrderByDescending(row => row.Count)
                .ThenBy(row => row.RawValue, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (values.Count == 0)
                return null;

            FieldValueCount dominant = values[0];
            int observationCount = observations.Select(row => row.ComponentId).Distinct().Count();
            int coveragePercent = dominant.Count * 100 / selectedComponentCount;
            int minimumObservations = Math.Max(3, selectedComponentCount / 5);
            if (coveragePercent < 50 && observationCount < minimumObservations)
                return null;

            return new ComponentOperationFieldCorrelationRecord
            {
                FieldKey = fieldKey,
                DominantValue = dominant.RawValue,
                MatchCount = dominant.Count,
                ObservationCount = observationCount,
                CoveragePercent = coveragePercent,
                DistinctValueCount = values.Count,
                SampleValuesText = JoinValues(values.Select(row => row.RawValue).Take(6))
            };
        }

        private static string BuildValueCountSummary(IEnumerable<string> values, int maxItems, string emptyText)
        {
            List<string> items = values == null
                ? new List<string>()
                : values
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
                    .Select(group => new FieldValueCount
                    {
                        RawValue = group.Key,
                        Count = group.Count()
                    })
                    .OrderByDescending(row => row.Count)
                    .ThenBy(row => row.RawValue, StringComparer.OrdinalIgnoreCase)
                    .Select(row => row.RawValue + " x" + row.Count.ToString(CultureInfo.InvariantCulture))
                    .Take(maxItems)
                    .ToList();

            if (items.Count == 0)
                return emptyText;

            return string.Join(", ", items);
        }

        private List<string> GetDomainContextTags(string domainKey)
        {
            List<ComponentTokenEvidence> evidenceRows;
            if (!_evidenceByDomainKey.TryGetValue(domainKey, out evidenceRows))
                return new List<string>();

            return evidenceRows
                .SelectMany(row => row.ContextTags ?? new List<string>())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string DetermineDomainConfidence(DefinitionDomain domain)
        {
            if (domain == null || domain.Options == null || domain.Options.Count == 0)
                return SemanticConfidence.Unknown;
            if (domain.Options.Any(option => string.Equals(option.Confidence, SemanticConfidence.Confirmed, StringComparison.OrdinalIgnoreCase)))
                return SemanticConfidence.Confirmed;
            if (domain.Options.Any(option => string.Equals(option.Confidence, SemanticConfidence.Inferred, StringComparison.OrdinalIgnoreCase)))
                return SemanticConfidence.Inferred;
            if (domain.Options.Any(option => string.Equals(option.Confidence, SemanticConfidence.Structural, StringComparison.OrdinalIgnoreCase)))
                return SemanticConfidence.Structural;
            if (domain.Options.Any(option => string.Equals(option.Confidence, SemanticConfidence.Londo, StringComparison.OrdinalIgnoreCase)))
                return SemanticConfidence.Londo;
            return SemanticConfidence.Unknown;
        }

        private static string BuildOperationFieldNotes(string fieldKey, List<FieldObservation> observations, DefinitionDomain domain, int componentCount)
        {
            List<string> sampleComponents = observations.Select(row => row.ComponentId.ToString(CultureInfo.InvariantCulture)).Distinct().OrderBy(value => value).Take(12).ToList();
            List<string> sampleLayouts = observations.Select(row => row.LayoutVariant).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).Take(6).ToList();
            string summary = observations.Count.ToString(CultureInfo.InvariantCulture)
                + " non-zero observation(s) across "
                + componentCount.ToString(CultureInfo.InvariantCulture)
                + " component row(s); example component ids: "
                + JoinValues(sampleComponents)
                + ".";

            if (sampleLayouts.Count > 0)
                summary += " Layouts: " + JoinValues(sampleLayouts) + ".";
            if (domain != null && !string.IsNullOrWhiteSpace(domain.Notes))
                summary += " " + domain.Notes;

            return summary.Trim();
        }

        private static FieldTriage BuildFieldTriage(uint operationId, string fieldKey, List<FieldObservation> observations, string confidence, int operationAbilityCount)
        {
            bool isStructural = string.Equals(confidence, SemanticConfidence.Structural, StringComparison.OrdinalIgnoreCase);
            if (!string.Equals(confidence, SemanticConfidence.Unknown, StringComparison.OrdinalIgnoreCase) && !isStructural)
            {
                return new FieldTriage
                {
                    Score = 0,
                    Bucket = "Resolved",
                    Notes = "This field already has extracted-client semantic coverage and does not need unknown-first triage."
                };
            }

            int nonZeroCount = observations == null ? 0 : observations.Count;
            int distinctValueCount = observations == null ? 0 : observations.Select(row => row.RawValue).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            int score = 0;
            List<string> reasons = new List<string>();

            if (PriorityOperations.Contains(operationId))
            {
                score += 100;
                reasons.Add("priority operation");
            }

            if (operationAbilityCount > 0)
            {
                score += Math.Min(30, operationAbilityCount / 40);
                if (operationAbilityCount >= 200)
                    reasons.Add(operationAbilityCount.ToString(CultureInfo.InvariantCulture) + " referencing abilities");
            }

            if (nonZeroCount > 0)
            {
                score += Math.Min(60, nonZeroCount / 20);
                if (nonZeroCount >= 100)
                    reasons.Add(nonZeroCount.ToString(CultureInfo.InvariantCulture) + " non-zero rows");
            }

            if (distinctValueCount > 0)
            {
                score += Math.Min(40, distinctValueCount * 2);
                if (distinctValueCount >= 10)
                    reasons.Add(distinctValueCount.ToString(CultureInfo.InvariantCulture) + " distinct values");
            }

            if (fieldKey.StartsWith("ExtData[", StringComparison.OrdinalIgnoreCase))
            {
                score += 35;
                reasons.Add("ext-data slot");
            }
            else if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase))
            {
                score += 30;
                reasons.Add("flag bitfield candidate");
            }
            else if (string.Equals(fieldKey, "ActivationDelay", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "Interval", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "MaxTargets", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "ConeAngle", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "FlightSpeed", StringComparison.OrdinalIgnoreCase))
            {
                score += 20;
                reasons.Add("gameplay control field");
            }
            else if (fieldKey.StartsWith("Value[", StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
                reasons.Add("generic value slot");
            }

            if (fieldKey.StartsWith("Multiplier[", StringComparison.OrdinalIgnoreCase))
            {
                score -= 80;
                reasons.Add("multiplier-array noise");
            }

            if (isStructural)
            {
                score = Math.Max(0, score - 20);
                reasons.Add("structural role inferred");
            }

            if (score < 0)
                score = 0;

            return new FieldTriage
            {
                Score = score,
                Bucket = DetermineFieldTriageBucket(score),
                Notes = reasons.Count == 0
                    ? "Low-signal unknown field."
                    : string.Join("; ", reasons.Distinct(StringComparer.OrdinalIgnoreCase)) + "."
            };
        }

        private static string DetermineFieldTriageBucket(int score)
        {
            if (score >= 170)
                return "Critical";
            if (score >= 120)
                return "High";
            if (score >= 70)
                return "Medium";
            if (score > 0)
                return "Low";
            return "Noise";
        }

        private static string JoinValues(IEnumerable<string> values)
        {
            List<string> items = values == null
                ? new List<string>()
                : values.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return items.Count == 0 ? string.Empty : string.Join(", ", items);
        }

        private static string BuildTriggerText(uint triggerValue)
        {
            if (triggerValue == 0)
                return "0 -> OnEnd";

            return triggerValue.ToString(CultureInfo.InvariantCulture) + " -> " + DefinitionCatalog.DescribeTriggerValue(triggerValue);
        }

        private static string GetFieldSortKey(string fieldKey)
        {
            int bucket = GetFieldSortBucket(fieldKey);
            return bucket.ToString("D3", CultureInfo.InvariantCulture) + "|" + fieldKey;
        }

        private static int GetFieldSortBucket(string fieldKey)
        {
            if (string.Equals(fieldKey, "ActivationDelay", StringComparison.OrdinalIgnoreCase)) return 10;
            if (string.Equals(fieldKey, "Duration", StringComparison.OrdinalIgnoreCase)) return 20;
            if (string.Equals(fieldKey, "Interval", StringComparison.OrdinalIgnoreCase)) return 30;
            if (string.Equals(fieldKey, "Radius", StringComparison.OrdinalIgnoreCase)) return 40;
            if (string.Equals(fieldKey, "ConeAngle", StringComparison.OrdinalIgnoreCase)) return 50;
            if (string.Equals(fieldKey, "FlightSpeed", StringComparison.OrdinalIgnoreCase)) return 60;
            if (string.Equals(fieldKey, "MaxTargets", StringComparison.OrdinalIgnoreCase)) return 70;
            if (string.Equals(fieldKey, "FlagsRaw", StringComparison.OrdinalIgnoreCase)) return 80;
            if (string.Equals(fieldKey, "Value00", StringComparison.OrdinalIgnoreCase)) return 90;
            if (string.Equals(fieldKey, "Value08", StringComparison.OrdinalIgnoreCase)) return 100;
            if (string.Equals(fieldKey, "Value15", StringComparison.OrdinalIgnoreCase)) return 110;
            if (fieldKey.StartsWith("Value[", StringComparison.OrdinalIgnoreCase)) return 200;
            if (fieldKey.StartsWith("Multiplier[", StringComparison.OrdinalIgnoreCase)) return 300;
            if (fieldKey.StartsWith("ExtData[", StringComparison.OrdinalIgnoreCase)) return 400;
            return 900;
        }

        private static void AddEvidence(Dictionary<string, List<ComponentTokenEvidence>> target, string key, ComponentTokenEvidence evidence)
        {
            List<ComponentTokenEvidence> items;
            if (!target.TryGetValue(key, out items))
            {
                items = new List<ComponentTokenEvidence>();
                target[key] = items;
            }

            items.Add(evidence);
        }

        private FieldObservationInference BuildFieldObservationInference(uint operationId, string fieldKey, List<FieldObservation> observations)
        {
            if (observations == null || observations.Count == 0)
                return null;

            if (IsRequirementLinkField(fieldKey) && _knownRequirementIds.Count > 0)
            {
                List<ushort> matchedRequirementIds = observations
                    .Select(row => ParseRequirementId(row.RawValue))
                    .Where(value => value.HasValue)
                    .Select(value => value.Value)
                    .Distinct()
                    .OrderBy(value => value)
                    .ToList();
                if (matchedRequirementIds.Count > 0)
                {
                    int distinctValueCount = observations.Select(row => row.RawValue).Distinct(StringComparer.OrdinalIgnoreCase).Count();
                    string exampleIds = string.Join(", ", matchedRequirementIds.Take(12).Select(value => value.ToString(CultureInfo.InvariantCulture)));
                    return new FieldObservationInference
                    {
                        SemanticSummary = matchedRequirementIds.Count == distinctValueCount
                            ? "Exact raw values in this field match known RequirementId rows in abilityrequirementexport.bin."
                            : "This field includes raw values that match known RequirementId rows in abilityrequirementexport.bin.",
                        Confidence = SemanticConfidence.Inferred,
                        Notes = matchedRequirementIds.Count == distinctValueCount
                            ? "Every distinct observed value in this field matches a known RequirementId; example requirement ids: " + exampleIds + "."
                            : matchedRequirementIds.Count.ToString(CultureInfo.InvariantCulture) + " distinct value(s) in this field match known RequirementId rows; example requirement ids: " + exampleIds + "."
                    };
                }
            }

            FieldObservationInference structuralInference;
            if (TryBuildNamedControlFieldInference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildNearConstantMultiplierInference(fieldKey, observations, out structuralInference))
                return structuralInference;

            // Universal ExtData decodes fire before per-operation structural handlers.
            if (TryBuildExtDataVal1Inference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildExtDataVal2Inference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildExtDataVal3Inference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildExtDataVal4Inference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildExtDataVal7Inference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildExtDataVal5Inference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildExtDataVal6Inference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildExtDataVal8Inference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildExtDataVal9Inference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildDamageStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildBonusTypeAdjustStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildDamageChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildEventListenerStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildGrantedAbilityStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildSummonMountStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildHealStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildVelocityStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildMonsterControllerStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildEffectBuffStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildDispelBuffStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildThreeWayScopeVal1Inference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildStatChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildDiskUpdateStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildDefensiveStatChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildServerCommandStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildInterruptStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildRankChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUnknownOp32StructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildArmorChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildCatapultStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildMoraleRegenChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildApChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUpdateCounterStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildRecoverStandardStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUnknownOp29StructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildStealthStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUnknownOp47StructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUnknownOp51StructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildHateStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildMonsterForceTargetStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildApRegenChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildDefensiveStatChangeValue1Inference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildMoraleChangeStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildHealValue1Inference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildAutoAttackAdjustStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUnknownOp41StructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUnknownOp40StructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUnknownOp43StructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildUnknownOp30StructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildRessurrectStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildCooldownChangeValue1Inference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildMoraleRegenChangeFlagsRawInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildCcStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildKnockbackStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildImmunityStructuralInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildStatChangeFlagsRawInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildApplyAbilityFlagsRawInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            // Generic catch-alls: promote Value08=1 binary flags and Value15=CC-bits gates from Unknown→Inferred
            // for any operation that doesn't have a dedicated handler above.
            if (TryBuildGenericValue08BinaryInference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildGenericValue15CcGateInference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildGenericSingleBitFlagsRawInference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildGenericFlagsRawInference(fieldKey, observations, out structuralInference))
                return structuralInference;

            if (TryBuildApplyAbilityExtDataKnownFieldInference(operationId, fieldKey, observations, out structuralInference))
                return structuralInference;

            return TryBuildApplyAbilityStructuralInference(operationId, fieldKey, observations, out structuralInference)
                ? structuralInference
                : null;
        }

        private static string BuildAbilityComponentFieldKey(ushort abilityId, ushort componentId, string fieldKey)
        {
            return abilityId.ToString(CultureInfo.InvariantCulture)
                + "|"
                + componentId.ToString(CultureInfo.InvariantCulture)
                + "|"
                + fieldKey;
        }

        private static bool TryParseTokenField(string tokenBody, uint operation, out ComponentFieldDescriptor descriptor)
        {
            descriptor = null;
            if (string.IsNullOrWhiteSpace(tokenBody))
                return false;

            if (tokenBody.StartsWith("DURA_", StringComparison.OrdinalIgnoreCase))
            {
                descriptor = CreateFieldDescriptor(operation, "Duration", tokenBody, BuildMeaningForTokenBody(tokenBody, 0));
                return true;
            }

            if (tokenBody.StartsWith("FREQ_", StringComparison.OrdinalIgnoreCase))
            {
                descriptor = CreateFieldDescriptor(operation, "Interval", tokenBody, BuildMeaningForTokenBody(tokenBody, 0));
                return true;
            }

            if (tokenBody.StartsWith("RADI_", StringComparison.OrdinalIgnoreCase))
            {
                descriptor = CreateFieldDescriptor(operation, "Radius", tokenBody, BuildMeaningForTokenBody(tokenBody, 0));
                return true;
            }

            if (tokenBody.StartsWith("VAL", StringComparison.OrdinalIgnoreCase))
            {
                int index = 3;
                while (index < tokenBody.Length && char.IsDigit(tokenBody[index]))
                    ++index;

                int valueIndex;
                if (!int.TryParse(tokenBody.Substring(3, index - 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out valueIndex))
                    return false;

                descriptor = CreateFieldDescriptor(operation, "Value[" + valueIndex.ToString(CultureInfo.InvariantCulture) + "]", tokenBody, BuildMeaningForTokenBody(tokenBody, 0));
                return true;
            }

            return false;
        }

        private static ComponentFieldDescriptor CreateFieldDescriptor(uint operation, string fieldKey, string tokenBody, string meaning)
        {
            return new ComponentFieldDescriptor
            {
                FieldKey = fieldKey,
                DomainKey = BuildOperationFieldDomainKey(operation, fieldKey),
                TokenBody = tokenBody,
                TokenMeaning = meaning
            };
        }

        private static string BuildEvidenceMeaning(IEnumerable<ComponentTokenEvidence> evidenceRows)
        {
            List<string> meanings = evidenceRows
                .Select(row => row.Meaning)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return meanings.Count == 0 ? "No semantic text evidence is known." : string.Join(" ", meanings);
        }

        private static string BuildEvidenceNotes(IEnumerable<ComponentTokenEvidence> evidenceRows)
        {
            List<ComponentTokenEvidence> rows = evidenceRows.ToList();
            if (rows.Count == 0)
                return string.Empty;

            string tokens = string.Join(", ", rows.Select(row => row.TokenBody).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value));
            string abilities = string.Join(", ", rows.Select(row => row.AbilityId.ToString(CultureInfo.InvariantCulture)).Distinct().OrderBy(value => value).Take(12));
            return rows.Count.ToString(CultureInfo.InvariantCulture) + " evidence row(s); tokens: " + tokens + "; example ability ids: " + abilities + ".";
        }

        private static string BuildSharedEvidenceNotes(string fieldKey, IEnumerable<ComponentTokenEvidence> evidenceRows)
        {
            List<ComponentTokenEvidence> rows = evidenceRows.ToList();
            if (rows.Count == 0)
                return string.Empty;

            string tokens = string.Join(", ", rows.Select(row => row.TokenBody).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
            string operations = string.Join(", ", rows.Select(row => DefinitionCatalog.DescribeComponentOperationValue(row.Operation) + " [" + row.Operation.ToString(CultureInfo.InvariantCulture) + "]").Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value, StringComparer.OrdinalIgnoreCase).Take(8));
            string abilities = string.Join(", ", rows.Select(row => row.AbilityId.ToString(CultureInfo.InvariantCulture)).Distinct().OrderBy(value => value).Take(12));
            return rows.Count.ToString(CultureInfo.InvariantCulture)
                + " shared-field evidence row(s) for "
                + fieldKey
                + "; tokens: "
                + tokens
                + "; source operations: "
                + operations
                + "; example ability ids: "
                + abilities
                + ".";
        }

        private static string BuildSourceLabel(IEnumerable<ComponentTokenEvidence> evidenceRows)
        {
            return string.Join(", ", evidenceRows.Select(row => row.Source).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value));
        }

        private bool TryResolveRequirementFieldSemantic(string fieldKey, string rawValue, out ComponentFieldSemantic semantic)
        {
            semantic = null;
            if (!IsRequirementLinkField(fieldKey))
                return false;

            ushort requirementId;
            if (!TryParseRequirementId(rawValue, out requirementId))
                return false;

            BinaryRequirementRecord requirementRow;
            if (!_requirementsById.TryGetValue(requirementId, out requirementRow))
                return false;

            RequirementSemanticDescription description = _requirements.DescribeRequirement(requirementId);
            semantic = new ComponentFieldSemantic
            {
                DomainKey = "Requirement.RequirementId",
                Meaning = description.Meaning,
                Confidence = description.Confidence,
                Source = "Inferred requirement link",
                SourcePath = requirementRow.SourcePath,
                SourceLocation = FormatSourceLocation(requirementRow),
                Notes = description.Notes
            };
            return true;
        }

        private static ComponentFieldSemantic CreateUnknown(string notes)
        {
            return new ComponentFieldSemantic
            {
                DomainKey = string.Empty,
                Meaning = "No extracted-client semantic mapping is known yet for this field.",
                Confidence = SemanticConfidence.Unknown,
                Source = "No direct evidence",
                SourcePath = string.Empty,
                SourceLocation = string.Empty,
                Notes = notes
            };
        }

        private static string BuildDisplayName(string domainKey)
        {
            string[] parts = (domainKey ?? string.Empty).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4)
                return domainKey ?? string.Empty;

            uint operation;
            string operationLabel = uint.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out operation)
                ? "Operation " + operation.ToString(CultureInfo.InvariantCulture)
                : "Operation";
            return operationLabel + " :: " + string.Join(".", parts.Skip(3));
        }

        private static bool TryExtractFieldKey(string domainKey, out string fieldKey)
        {
            fieldKey = string.Empty;
            const string prefix = "Component.Operation.";
            if (string.IsNullOrWhiteSpace(domainKey) || !domainKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            int separatorIndex = domainKey.IndexOf('.', prefix.Length);
            if (separatorIndex < 0 || separatorIndex + 1 >= domainKey.Length)
                return false;

            fieldKey = domainKey.Substring(separatorIndex + 1);
            return !string.IsNullOrWhiteSpace(fieldKey);
        }

        private static bool IsRequirementLinkField(string fieldKey)
        {
            return !string.IsNullOrWhiteSpace(fieldKey) && fieldKey.EndsWith("Val6", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSharedFieldFallbackAllowed(string fieldKey)
        {
            return string.Equals(fieldKey, "Duration", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "Interval", StringComparison.OrdinalIgnoreCase)
                || string.Equals(fieldKey, "Radius", StringComparison.OrdinalIgnoreCase);
        }

        private ushort? ParseRequirementId(string rawValue)
        {
            ushort requirementId;
            return TryParseRequirementId(rawValue, out requirementId) ? requirementId : (ushort?)null;
        }

        private bool TryParseRequirementId(string rawValue, out ushort requirementId)
        {
            requirementId = 0;
            if (string.IsNullOrWhiteSpace(rawValue))
                return false;

            if (!ushort.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out requirementId))
                return false;

            return _knownRequirementIds.Contains(requirementId);
        }

        private static string FormatSourceLocation(SourceRowBase row)
        {
            if (row == null)
                return string.Empty;
            if (row.ByteOffset > 0)
                return "byte " + row.ByteOffset.ToString(CultureInfo.InvariantCulture);
            if (row.LineNumber > 0)
                return "line " + row.LineNumber.ToString(CultureInfo.InvariantCulture);
            return string.Empty;
        }

        private static string BuildPlainEnglishDefinition(ComponentTokenEvidence evidence)
        {
            if (evidence == null)
                return string.Empty;

            return "Read component slot "
                + evidence.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture)
                + " (component id "
                + evidence.ComponentId.ToString(CultureInfo.InvariantCulture)
                + "), then "
                + TrimTrailingPeriod(evidence.Meaning);
        }

        private static string BuildGlossaryNotes(IEnumerable<ComponentTokenEvidence> evidenceRows)
        {
            List<ComponentTokenEvidence> rows = evidenceRows.ToList();
            if (rows.Count == 0)
                return string.Empty;

            ComponentTokenEvidence example = rows[0];
            string operations = string.Join(", ", rows.Select(row => row.Operation.ToString(CultureInfo.InvariantCulture)).Distinct().OrderBy(value => value));
            string abilityIds = string.Join(", ", rows.Select(row => row.AbilityId.ToString(CultureInfo.InvariantCulture)).Distinct().OrderBy(value => value).Take(24));
            string exampleText = rows.Select(row => row.TextExcerpt).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
            string contextTags = string.Join(", ", rows.SelectMany(row => row.ContextTags ?? new List<string>()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(value => value));
            return "Observed in "
                + rows.Count.ToString(CultureInfo.InvariantCulture)
                + " extracted string occurrence(s); component field "
                + example.FieldKey
                + "; component operation ids "
                + operations
                + (string.IsNullOrWhiteSpace(contextTags) ? string.Empty : "; context tags " + contextTags)
                + "; example ability ids "
                + abilityIds
                + (string.IsNullOrWhiteSpace(exampleText) ? "." : "; example text: \"" + Truncate(exampleText, 220) + "\".");
        }

        private static string BuildMeaningForTokenBody(string tokenBody, int depth)
        {
            if (string.IsNullOrWhiteSpace(tokenBody))
                return "read an unresolved component field and format it for client text.";
            if (depth > 12)
                return "resolve a deeply nested token chain that exceeds the current decomposition limit.";

            if (tokenBody.StartsWith("DURA_", StringComparison.OrdinalIgnoreCase))
                return "read the selected component's Duration field and format it as " + DescribeRenderSuffix(tokenBody.Substring(5), depth + 1) + ".";

            if (tokenBody.StartsWith("FREQ_", StringComparison.OrdinalIgnoreCase))
                return "read the selected component's Interval field and format it as " + DescribeRenderSuffix(tokenBody.Substring(5), depth + 1) + ".";

            if (tokenBody.StartsWith("RADI_", StringComparison.OrdinalIgnoreCase))
                return "read the selected component's Radius field and format it as " + DescribeRenderSuffix(tokenBody.Substring(5), depth + 1) + ".";

            if (tokenBody.StartsWith("VAL", StringComparison.OrdinalIgnoreCase))
            {
                int index = 3;
                while (index < tokenBody.Length && char.IsDigit(tokenBody[index]))
                    ++index;

                int valueIndex;
                if (!int.TryParse(tokenBody.Substring(3, index - 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out valueIndex))
                    return "read an unresolved component value field and format it for client text.";

                string suffix = index < tokenBody.Length && tokenBody[index] == '_' ? tokenBody.Substring(index + 1) : string.Empty;
                if (string.IsNullOrWhiteSpace(suffix))
                    return "read the selected component's Value[" + valueIndex.ToString(CultureInfo.InvariantCulture) + "] field and insert the raw numeric result into client text.";

                return "read the selected component's Value[" + valueIndex.ToString(CultureInfo.InvariantCulture) + "] field and render it as " + DescribeRenderSuffix(suffix, depth + 1) + " in this text context.";
            }

            return "resolve token `" + tokenBody + "` using a client-text formatting rule that has not been named yet.";
        }

        private static string DescribeRenderSuffix(string suffix, int depth)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                return "a raw client value";
            if (depth > 12)
                return "a deeply nested client value";

            if (suffix.StartsWith("COM_", StringComparison.OrdinalIgnoreCase))
            {
                EmbeddedComReference reference;
                if (TryParseEmbeddedComReference(suffix, out reference))
                {
                    return "the same presentation as `" + reference.FullToken + "` (" + TrimTrailingPeriod(BuildEmbeddedComMeaning(reference, depth + 1)) + ")";
                }
            }

            string normalized = NormalizeSpecialSuffix(suffix);
            return string.IsNullOrWhiteSpace(normalized) ? suffix.ToLowerInvariant().Replace("_", " ") : normalized;
        }

        private static string BuildEmbeddedComMeaning(EmbeddedComReference reference, int depth)
        {
            return "read component slot "
                + reference.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture)
                + ", then "
                + TrimTrailingPeriod(BuildMeaningForTokenBody(reference.TokenBody, depth + 1));
        }

        private static bool TryParseEmbeddedComReference(string rawValue, out EmbeddedComReference reference)
        {
            reference = null;
            if (string.IsNullOrWhiteSpace(rawValue) || !rawValue.StartsWith("COM_", StringComparison.OrdinalIgnoreCase))
                return false;

            int cursor = 4;
            int digitStart = cursor;
            while (cursor < rawValue.Length && char.IsDigit(rawValue[cursor]))
                ++cursor;

            if (cursor <= digitStart || cursor >= rawValue.Length || rawValue[cursor] != '_')
                return false;

            int componentSlotIndex;
            if (!int.TryParse(rawValue.Substring(digitStart, cursor - digitStart), NumberStyles.Integer, CultureInfo.InvariantCulture, out componentSlotIndex))
                return false;

            string tokenBody = rawValue.Substring(cursor + 1);
            reference = new EmbeddedComReference
            {
                ComponentSlotIndex = componentSlotIndex,
                TokenBody = tokenBody,
                FullToken = "COM_" + componentSlotIndex.ToString(CultureInfo.InvariantCulture) + "_" + tokenBody
            };
            return true;
        }

        private static string NormalizeSpecialSuffix(string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                return string.Empty;

            string upper = suffix.ToUpperInvariant();
            if (upper == "TOD")
                return "an over-time amount";
            if (upper == "DAMAGE")
                return "damage";
            if (upper == "HEALTH")
                return "health";
            if (upper == "ACTIONPOINTS")
                return "action points";
            if (upper == "SECONDS")
                return "seconds";
            if (upper == "MINUTES")
                return "minutes";
            if (upper == "HOURS")
                return "hours";
            if (upper == "FEET")
                return "feet";
            if (upper == "ELEMENTALDAMAGE")
                return "elemental damage";
            if (upper == "SPIRITDAMAGE")
                return "spirit damage";
            if (upper == "CORPOREALDAMAGE")
                return "corporeal damage";
            if (upper.StartsWith("TOD_", StringComparison.OrdinalIgnoreCase))
                return NormalizeSpecialSuffix(upper.Substring(4)) + " over time";

            return upper.ToLowerInvariant().Replace("_", " ");
        }

        private static string TrimTrailingPeriod(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.TrimEnd().TrimEnd('.');
        }

        private string LookupAbilityName(ushort abilityId)
        {
            string name;
            return _abilityNamesById.TryGetValue(abilityId, out name) ? name : string.Empty;
        }

        private static Dictionary<ushort, string> BuildAbilityNames(AbilityDataset dataset)
        {
            Dictionary<ushort, string> names = new Dictionary<ushort, string>();
            foreach (ClientAbilityRecord row in dataset.ClientAbilities.Where(row => row.AbilityId <= ushort.MaxValue))
            {
                ushort abilityId = (ushort)row.AbilityId;
                if (!names.ContainsKey(abilityId) && !string.IsNullOrWhiteSpace(row.Name))
                    names[abilityId] = row.Name;
            }

            foreach (IndexedStringRecord row in dataset.AbilityNames.Where(row => row.EntryId <= ushort.MaxValue))
            {
                ushort abilityId = (ushort)row.EntryId;
                if (!names.ContainsKey(abilityId) && !string.IsNullOrWhiteSpace(row.NormalizedValue))
                    names[abilityId] = row.NormalizedValue;
            }

            return names;
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
                return value ?? string.Empty;

            return value.Substring(0, Math.Max(0, maxLength - 3)).TrimEnd() + "...";
        }

        private static List<string> BuildContextTags(string textExcerpt)
        {
            List<string> tags = new List<string>();
            string text = (textExcerpt ?? string.Empty).ToLowerInvariant();
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

        private static Dictionary<string, ComponentSchemaOverride> LoadOverrides()
        {
            Dictionary<string, ComponentSchemaOverride> overrides = new Dictionary<string, ComponentSchemaOverride>(StringComparer.OrdinalIgnoreCase);
            string path = ResolveOverridePath();
            if (!File.Exists(path))
                return overrides;

            foreach (string rawLine in File.ReadAllLines(path))
            {
                if (string.IsNullOrWhiteSpace(rawLine) || rawLine.StartsWith("#", StringComparison.Ordinal))
                    continue;

                string[] columns = rawLine.Split('\t');
                if (columns.Length < 6 || string.Equals(columns[0], "DomainKey", StringComparison.OrdinalIgnoreCase))
                    continue;

                overrides[columns[0]] = new ComponentSchemaOverride
                {
                    DomainKey = columns[0],
                    Meaning = columns[1],
                    Confidence = columns[2],
                    SourceLabel = columns[3],
                    SourcePath = columns[4],
                    SourceLocation = columns[5],
                    Notes = columns.Length > 6 ? columns[6] : string.Empty
                };
            }

            return overrides;
        }

        private static string ResolveOverridePath()
        {
            foreach (string root in new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Path.GetDirectoryName(typeof(ComponentSchemaCatalog).Assembly.Location),
                Environment.CurrentDirectory
            }.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                string resolved = TryResolveOverridePath(root);
                if (!string.IsNullOrWhiteSpace(resolved))
                    return resolved;
            }

            return Path.GetFullPath(OverrideRelativePath);
        }

        private static string TryResolveOverridePath(string startDirectory)
        {
            string current = startDirectory;
            for (int depth = 0; depth < 8 && !string.IsNullOrWhiteSpace(current); ++depth)
            {
                string candidate = Path.Combine(current, OverrideRelativePath);
                if (File.Exists(candidate))
                    return candidate;

                DirectoryInfo parent = Directory.GetParent(current);
                current = parent == null ? null : parent.FullName;
            }

            return string.Empty;
        }

        private sealed class ComponentFieldDescriptor
        {
            public string FieldKey { get; set; }
            public string DomainKey { get; set; }
            public string TokenBody { get; set; }
            public string TokenMeaning { get; set; }
        }

        private sealed class ComponentTokenEvidence
        {
            public ushort AbilityId { get; set; }
            public string AbilityName { get; set; }
            public ushort ComponentId { get; set; }
            public int ComponentSlotIndex { get; set; }
            public uint Operation { get; set; }
            public string DomainKey { get; set; }
            public string FieldKey { get; set; }
            public string TokenBody { get; set; }
            public string Meaning { get; set; }
            public string Source { get; set; }
            public string SourcePath { get; set; }
            public string SourceLocation { get; set; }
            public string TextExcerpt { get; set; }
            public List<string> ContextTags { get; set; }
        }

        private sealed class ComponentSchemaOverride
        {
            public string DomainKey { get; set; }
            public string Meaning { get; set; }
            public string Confidence { get; set; }
            public string SourceLabel { get; set; }
            public string SourcePath { get; set; }
            public string SourceLocation { get; set; }
            public string Notes { get; set; }
        }

        private sealed class AbilityTextSelection
        {
            public string TextExcerpt { get; set; }
            public string SourcePath { get; set; }
            public string SourceLocation { get; set; }
            public List<string> ContextTags { get; set; }
        }

        private sealed class AbilityComponentReference
        {
            public ushort AbilityId { get; set; }
            public string AbilityName { get; set; }
            public ushort ComponentId { get; set; }
            public int ComponentSlotIndex { get; set; }
            public uint TriggerValue { get; set; }
            public string TriggerText { get; set; }
            public string TextExcerpt { get; set; }
            public string SourcePath { get; set; }
            public string SourceLocation { get; set; }
            public List<string> ContextTags { get; set; }
        }

        private sealed class FieldObservation
        {
            public string FieldKey { get; set; }
            public string RawValue { get; set; }
            public ushort ComponentId { get; set; }
            public string LayoutVariant { get; set; }
        }

        private sealed class FieldObservationInference
        {
            public string SemanticSummary { get; set; }
            public string Confidence { get; set; }
            public string Notes { get; set; }
        }

        private sealed class FieldValueObservation
        {
            public string RawValue { get; set; }
            public ushort ComponentId { get; set; }
            public List<AbilityComponentReference> AbilityReferences { get; set; }
        }

        private sealed class FieldValueCount
        {
            public string RawValue { get; set; }
            public int Count { get; set; }
        }

        private sealed class FieldTriage
        {
            public int Score { get; set; }
            public string Bucket { get; set; }
            public string Notes { get; set; }
        }

        private sealed class EmbeddedComReference
        {
            public int ComponentSlotIndex { get; set; }
            public string TokenBody { get; set; }
            public string FullToken { get; set; }
        }
    }

    public sealed class ComponentFieldSemantic
    {
        public string DomainKey { get; set; }
        public string Meaning { get; set; }
        public string Confidence { get; set; }
        public string Source { get; set; }
        public string SourcePath { get; set; }
        public string SourceLocation { get; set; }
        public string Notes { get; set; }
    }
}

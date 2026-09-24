using ClientDataMatrix.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ClientDataMatrix.Services
{
    public sealed class RequirementCatalog
    {
        private readonly AbilityDataset _dataset;
        private readonly Dictionary<ushort, List<BinaryRequirementRecord>> _requirementsById;
        private readonly HashSet<ushort> _knownRequirementIds;
        private readonly Dictionary<ushort, string> _abilityNamesById;
        private readonly Dictionary<ushort, AbilityContext> _abilityContextsById;
        private readonly Dictionary<ushort, List<AbilityComponentUsage>> _abilityUsagesByComponentId;
        private readonly Dictionary<ushort, RequirementAnalysisRecord> _analysisByRequirementId;

        // Requirement ExtData field schema (from decompiled client AbilityExport.cs):
        //   Val1 = AbilitySourceType, Val2 = AbilityOperation, Val3 = AbilityCondition,
        //   Val4 = AbilityLogicOperator, Val5 = threshold/parameter, Val6 = child RequirementId or op-specific ref,
        //   Val7/Val8 = auxiliary parameters, Val9 = binary flag byte.

        private static readonly Dictionary<int, string> AbilitySourceTypeNames = new Dictionary<int, string>
        {
            { 0, "Self" }, { 1, "Cast" }, { 2, "Apply" }, { 3, "Watch" },
            { 4, "EventReq" }, { 6, "Result" }, { 7, "Immunity" },
        };

        private static readonly Dictionary<int, string> AbilityOperationNames = new Dictionary<int, string>
        {
            { 0, "Default" }, { 1, "FriendlyTargeted" }, { 2, "EnemyTargeted" },
            { 3, "BuffGroupCounterSum" }, { 4, "Range" }, { 5, "Unk5" },
            { 6, "BuffGroupCount" }, { 7, "Unk7" }, { 8, "GetGroupOnAbilityUsed" },
            { 9, "Random" }, { 10, "RequirmentGroupCheck" }, { 11, "TargetActivatedComponent" },
            { 12, "AbilityType" }, { 13, "SpellConvention" }, { 14, "TargetCheck" },
            { 15, "TargetIsPlayer" }, { 16, "MonsterType" }, { 17, "PreviousComponentActivated" },
            { 18, "WoundsPercent" }, { 19, "TargetRace" }, { 20, "TargetInSameGroup" },
            { 21, "TargetBackSide" }, { 22, "Unk22" }, { 23, "RequirmentGroupCheck2" },
            { 24, "MasteryPath" }, { 25, "Ap" }, { 26, "ApPercent" },
            { 27, "TargetRandom" }, { 28, "TargetIsMonster" }, { 29, "DamageType" },
            { 30, "Unk30" }, { 31, "InCombat" }, { 32, "Unk32" }, { 33, "Unk33" },
            { 34, "TargetIsMoving" }, { 35, "KeepDoor" }, { 36, "Unk36" }, { 37, "Unk37" },
            { 38, "TargetGreatWeapon" }, { 39, "Unk39" }, { 40, "SiegeControl" },
            { 41, "Unk41" }, { 42, "Unk42" }, { 43, "Unk43" }, { 44, "EquippedInventorySlot" },
            { 45, "Unk45" }, { 46, "Unk46" }, { 47, "Unk47" }, { 48, "Unk48" },
            { 49, "GuildLevel" }, { 50, "Unk50" }, { 51, "TargetBack" },
            { 52, "TargetBuildTimesChanged" }, { 53, "Unk53" }, { 54, "Unk54" },
            { 55, "TargetDead" }, { 56, "Unk56" }, { 57, "Unk57" }, { 58, "Unk58" },
            { 59, "Zone" }, { 60, "DoorTarget" }, { 61, "Unk61" }, { 62, "Unk62" },
            { 63, "Unk63" }, { 64, "Unk64" }, { 65, "Unk65" }, { 66, "Unk66" },
            { 67, "BuffAbilityIDSum" }, { 68, "Unk68" }, { 69, "Unk69" },
            { 70, "ItemSlotted" }, { 71, "Unk71" }, { 72, "Zone2" },
            { 73, "TimerLastTick" }, { 74, "AbilityLevel" }, { 75, "Finisher" },
            { 76, "ScenarioCheck" }, { 77, "BolsterLevel" }, { 78, "CanMounted" },
            { 79, "Mounted" }, { 80, "HasPet" }, { 81, "Unk81" },
            { 82, "LastAbilityResult" }, { 83, "Unk83" }, { 84, "Unk84" },
            { 85, "Unk85" }, { 86, "Unk86" }, { 87, "Unk87" },
            { 88, "TierCheck88" }, { 89, "Unk89" },
        };

        private static readonly Dictionary<int, string> AbilityConditionNames = new Dictionary<int, string>
        {
            { 0, "None" }, { 1, "Equal" }, { 2, "Unk2" }, { 3, "Unk3" }, { 4, "LessThanEqual" },
            { 5, "GreaterThanEqual" }, { 6, "LessThan" }, { 7, "GreaterThan" }, { 8, "FriendlyTarget" },
        };

        private static readonly Dictionary<int, string> AbilityLogicOperatorNames = new Dictionary<int, string>
        {
            { 0, "None" }, { 8, "And" }, { 9, "Or" }, { 10, "Unk10" }, { 11, "Unk11" }, { 12, "Unk12" },
        };

        public RequirementCatalog(AbilityDataset dataset)
        {
            _dataset = dataset;
            _requirementsById = dataset == null
                ? new Dictionary<ushort, List<BinaryRequirementRecord>>()
                : dataset.BinaryRequirements
                    .GroupBy(row => row.RequirementId)
                    .ToDictionary(group => group.Key, group => group.OrderBy(row => row.RecordIndex).ToList());
            _knownRequirementIds = new HashSet<ushort>(_requirementsById.Keys);
            _abilityNamesById = BuildAbilityNames(dataset);
            _abilityContextsById = BuildAbilityContexts(dataset, _abilityNamesById);
            _abilityUsagesByComponentId = BuildAbilityUsagesByComponentId(dataset, _abilityContextsById);
            _analysisByRequirementId = BuildRequirementAnalyses();
        }

        public RequirementLedgerDocument BuildRequirementLedger(string extractedRootPath)
        {
            return new RequirementLedgerDocument
            {
                GeneratedAtUtc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                ExtractedRootPath = extractedRootPath,
                TableStatuses = _dataset == null
                    ? new List<TableLoadStatus>()
                    : _dataset.TableStatuses.OrderBy(row => row.SourceFamily).ThenBy(row => row.TableName).ToList(),
                Requirements = _analysisByRequirementId.Values
                    .OrderBy(row => row.RequirementId)
                    .Select(ToLedgerRecord)
                    .ToList()
            };
        }

        public RequirementSemanticDescription DescribeRequirement(ushort requirementId)
        {
            RequirementAnalysisRecord analysis;
            if (!_analysisByRequirementId.TryGetValue(requirementId, out analysis))
            {
                return new RequirementSemanticDescription
                {
                    Meaning = "Requirement row " + requirementId.ToString(CultureInfo.InvariantCulture),
                    Confidence = SemanticConfidence.Inferred,
                    SourcePath = string.Empty,
                    SourceLocation = string.Empty,
                    Notes = "No additional requirement-ledger context is available for this RequirementId yet."
                };
            }

            BinaryRequirementRecord exampleRow = analysis.Rows.FirstOrDefault();
            return new RequirementSemanticDescription
            {
                Meaning = string.IsNullOrWhiteSpace(analysis.SemanticSummary)
                    ? "Requirement row " + requirementId.ToString(CultureInfo.InvariantCulture)
                    : analysis.SemanticSummary,
                Confidence = SemanticConfidence.Inferred,
                SourcePath = exampleRow == null ? string.Empty : exampleRow.SourcePath,
                SourceLocation = FormatSourceLocation(exampleRow),
                Notes = analysis.Notes
            };
        }

        private Dictionary<ushort, RequirementAnalysisRecord> BuildRequirementAnalyses()
        {
            List<RequirementReferenceRecord> allReferences = BuildRequirementReferences();
            Dictionary<ushort, List<RequirementReferenceRecord>> inboundByRequirementId = allReferences
                .GroupBy(row => row.RequirementId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.SourceKind).ThenBy(row => row.SourceId).ThenBy(row => row.ExtSlotIndex).ToList());
            Dictionary<ushort, List<RequirementReferenceRecord>> outboundByRequirementId = allReferences
                .Where(row => string.Equals(row.SourceKind, "Requirement", StringComparison.OrdinalIgnoreCase))
                .GroupBy(row => row.SourceId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.RequirementId).ThenBy(row => row.ExtSlotIndex).ToList());

            Dictionary<ushort, RequirementAnalysisRecord> analyses = new Dictionary<ushort, RequirementAnalysisRecord>();
            foreach (KeyValuePair<ushort, List<BinaryRequirementRecord>> pair in _requirementsById.OrderBy(entry => entry.Key))
            {
                List<RequirementReferenceRecord> inboundReferences;
                if (!inboundByRequirementId.TryGetValue(pair.Key, out inboundReferences))
                    inboundReferences = new List<RequirementReferenceRecord>();

                List<RequirementReferenceRecord> outboundReferences;
                if (!outboundByRequirementId.TryGetValue(pair.Key, out outboundReferences))
                    outboundReferences = new List<RequirementReferenceRecord>();

                List<AbilityUsageContext> directAbilityContexts = BuildDirectAbilityContexts(inboundReferences);
                List<RequirementFieldRecord> fields = BuildFieldRecords(pair.Value);

                RequirementAnalysisRecord analysis = new RequirementAnalysisRecord
                {
                    RequirementId = pair.Key,
                    Rows = pair.Value,
                    Fields = fields,
                    InboundReferences = inboundReferences.Select(BuildLinkEvidence).ToList(),
                    OutboundReferences = outboundReferences.Select(BuildLinkEvidence).ToList(),
                    DirectAbilityContexts = directAbilityContexts
                };

                analysis.ContextTags = directAbilityContexts
                    .SelectMany(row => row.ContextTags ?? new List<string>())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(row => row, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                analysis.SampleAbilities = directAbilityContexts
                    .GroupBy(row => row.AbilityId)
                    .OrderBy(group => group.Key)
                    .Select(group =>
                    {
                        AbilityUsageContext example = group.First();
                        return FormatAbilityLabel(example.AbilityId, example.AbilityName);
                    })
                    .Take(8)
                    .ToList();
                analysis.SemanticSummary = BuildSemanticSummary(analysis);
                analysis.Notes = BuildAnalysisNotes(analysis);

                analyses[pair.Key] = analysis;
            }

            return analyses;
        }

        private List<RequirementReferenceRecord> BuildRequirementReferences()
        {
            Dictionary<string, RequirementReferenceRecord> references = new Dictionary<string, RequirementReferenceRecord>(StringComparer.OrdinalIgnoreCase);
            if (_dataset == null)
                return new List<RequirementReferenceRecord>();

            foreach (BinaryAbilityRecord row in _dataset.BinaryAbilities)
                AddRequirementReferencesFromExtData(references, "Ability", row.AbilityId, BuildAbilityLabel(row.AbilityId), row.ExtData, row);

            foreach (BinaryComponentRecord row in _dataset.BinaryComponents)
                AddRequirementReferencesFromExtData(references, "Component", row.ComponentId, "Component " + row.ComponentId.ToString(CultureInfo.InvariantCulture) + " (" + DefinitionCatalog.DescribeComponentOperationValue(row.Operation) + ")", row.ExtData, row);

            foreach (BinaryRequirementRecord row in _dataset.BinaryRequirements)
                AddRequirementReferencesFromExtData(references, "Requirement", row.RequirementId, "Requirement " + row.RequirementId.ToString(CultureInfo.InvariantCulture), row.ExtData, row);

            return references.Values
                .OrderBy(row => row.RequirementId)
                .ThenBy(row => row.SourceKind)
                .ThenBy(row => row.SourceId)
                .ThenBy(row => row.ExtSlotIndex)
                .ToList();
        }

        private void AddRequirementReferencesFromExtData(Dictionary<string, RequirementReferenceRecord> references, string sourceKind, ushort sourceId, string sourceLabel, IList<BinaryExtDataRecord> extDataRows, SourceRowBase sourceRow)
        {
            foreach (BinaryExtDataRecord extRecord in extDataRows ?? new List<BinaryExtDataRecord>())
            {
                ushort requirementId;
                if (!TryGetKnownRequirementId(extRecord.Val6, out requirementId))
                    continue;

                RequirementReferenceRecord reference = new RequirementReferenceRecord
                {
                    SourceKind = sourceKind,
                    SourceId = sourceId,
                    SourceLabel = sourceLabel,
                    SourceField = "ExtData[" + extRecord.SlotIndex.ToString(CultureInfo.InvariantCulture) + "].Val6",
                    ExtSlotIndex = extRecord.SlotIndex,
                    RequirementId = requirementId,
                    Confidence = SemanticConfidence.Inferred,
                    Notes = "Inferred requirement reference because ExtData[*].Val6 matches a known RequirementId row in abilityrequirementexport.bin.",
                    SourceFamily = sourceRow == null ? string.Empty : sourceRow.SourceFamily,
                    SourceTableName = sourceRow == null ? string.Empty : sourceRow.TableName,
                    SourcePath = sourceRow == null ? string.Empty : sourceRow.SourcePath,
                    SourceRowKey = sourceRow == null ? string.Empty : sourceRow.RowKey,
                    LineNumber = sourceRow == null ? 0 : sourceRow.LineNumber,
                    ByteOffset = sourceRow == null ? 0 : sourceRow.ByteOffset,
                    SourceLocation = FormatSourceLocation(sourceRow)
                };

                string key = sourceKind
                    + "|"
                    + sourceId.ToString(CultureInfo.InvariantCulture)
                    + "|"
                    + reference.SourceField
                    + "|"
                    + requirementId.ToString(CultureInfo.InvariantCulture);
                if (!references.ContainsKey(key))
                    references[key] = reference;
            }
        }

        private RequirementLinkEvidenceRecord BuildLinkEvidence(RequirementReferenceRecord reference)
        {
            string relatedAbilitiesText = string.Empty;
            string contextTagsText = string.Empty;

            if (reference != null)
            {
                if (string.Equals(reference.SourceKind, "Ability", StringComparison.OrdinalIgnoreCase))
                {
                    AbilityContext abilityContext;
                    if (_abilityContextsById.TryGetValue(reference.SourceId, out abilityContext))
                    {
                        relatedAbilitiesText = FormatAbilityLabel(abilityContext.AbilityId, abilityContext.AbilityName);
                        contextTagsText = JoinValues(abilityContext.ContextTags);
                    }
                }
                else if (string.Equals(reference.SourceKind, "Component", StringComparison.OrdinalIgnoreCase))
                {
                    List<AbilityComponentUsage> usages;
                    if (_abilityUsagesByComponentId.TryGetValue(reference.SourceId, out usages))
                    {
                        relatedAbilitiesText = JoinValues(usages
                            .GroupBy(row => row.AbilityId)
                            .Select(group =>
                            {
                                AbilityComponentUsage example = group.First();
                                return FormatAbilityLabel(example.AbilityId, example.AbilityName);
                            })
                            .Take(8));
                        contextTagsText = JoinValues(usages
                            .SelectMany(row => row.ContextTags ?? new List<string>())
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(row => row, StringComparer.OrdinalIgnoreCase));
                    }
                }
            }

            return new RequirementLinkEvidenceRecord
            {
                SourceKind = reference == null ? string.Empty : reference.SourceKind,
                SourceId = reference == null ? (ushort)0 : reference.SourceId,
                SourceLabel = reference == null ? string.Empty : reference.SourceLabel,
                SourceField = reference == null ? string.Empty : reference.SourceField,
                ExtSlotIndex = reference == null ? 0 : reference.ExtSlotIndex,
                LinkedRequirementId = reference == null ? (ushort)0 : reference.RequirementId,
                RelatedAbilitiesText = relatedAbilitiesText,
                ContextTagsText = contextTagsText,
                SourcePath = reference == null ? string.Empty : reference.SourcePath,
                SourceLocation = reference == null ? string.Empty : reference.SourceLocation,
                Notes = reference == null ? string.Empty : reference.Notes
            };
        }

        private List<AbilityUsageContext> BuildDirectAbilityContexts(IEnumerable<RequirementReferenceRecord> inboundReferences)
        {
            Dictionary<string, AbilityUsageContext> contexts = new Dictionary<string, AbilityUsageContext>(StringComparer.OrdinalIgnoreCase);

            foreach (RequirementReferenceRecord reference in inboundReferences ?? Enumerable.Empty<RequirementReferenceRecord>())
            {
                if (string.Equals(reference.SourceKind, "Ability", StringComparison.OrdinalIgnoreCase))
                {
                    AbilityContext abilityContext;
                    if (_abilityContextsById.TryGetValue(reference.SourceId, out abilityContext))
                    {
                        string key = "Ability|" + abilityContext.AbilityId.ToString(CultureInfo.InvariantCulture);
                        if (!contexts.ContainsKey(key))
                        {
                            contexts[key] = new AbilityUsageContext
                            {
                                AbilityId = abilityContext.AbilityId,
                                AbilityName = abilityContext.AbilityName,
                                SourceKind = "Ability",
                                ContextTags = abilityContext.ContextTags == null ? new List<string>() : abilityContext.ContextTags.ToList()
                            };
                        }
                    }

                    continue;
                }

                if (!string.Equals(reference.SourceKind, "Component", StringComparison.OrdinalIgnoreCase))
                    continue;

                List<AbilityComponentUsage> usages;
                if (!_abilityUsagesByComponentId.TryGetValue(reference.SourceId, out usages))
                    continue;

                foreach (AbilityComponentUsage usage in usages)
                {
                    string key = usage.AbilityId.ToString(CultureInfo.InvariantCulture) + "|" + usage.ComponentId.ToString(CultureInfo.InvariantCulture) + "|" + usage.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture);
                    if (contexts.ContainsKey(key))
                        continue;

                    contexts[key] = new AbilityUsageContext
                    {
                        AbilityId = usage.AbilityId,
                        AbilityName = usage.AbilityName,
                        SourceKind = "Component",
                        ContextTags = usage.ContextTags == null ? new List<string>() : usage.ContextTags.ToList()
                    };
                }
            }

            return contexts.Values
                .OrderBy(row => row.AbilityId)
                .ThenBy(row => row.SourceKind, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private List<RequirementFieldRecord> BuildFieldRecords(IEnumerable<BinaryRequirementRecord> rows)
        {
            Dictionary<string, List<string>> valuesByField = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (BinaryRequirementRecord row in rows ?? Enumerable.Empty<BinaryRequirementRecord>())
            {
                foreach (BinaryExtDataRecord extRecord in row.ExtData ?? new List<BinaryExtDataRecord>())
                {
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 1, extRecord.Val1);
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 2, extRecord.Val2);
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 3, extRecord.Val3);
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 4, extRecord.Val4);
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 5, extRecord.Val5);
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 6, extRecord.Val6);
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 7, extRecord.Val7);
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 8, extRecord.Val8);
                    AddRequirementFieldValue(valuesByField, extRecord.SlotIndex, 9, extRecord.Val9);
                }
            }

            List<RequirementFieldRecord> fields = new List<RequirementFieldRecord>();
            foreach (KeyValuePair<string, List<string>> pair in valuesByField.OrderBy(entry => BuildFieldSortKey(entry.Key), StringComparer.OrdinalIgnoreCase))
            {
                List<string> distinctValues = pair.Value
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(row => ParseSortBucket(row))
                    .ThenBy(row => row, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                string semanticSummary = string.Empty;
                string confidence = SemanticConfidence.Unknown;
                string notes = string.Empty;
                if (pair.Key.EndsWith(".Val1", StringComparison.OrdinalIgnoreCase))
                {
                    TryDecodeEnumField(distinctValues, AbilitySourceTypeNames, "AbilitySourceType", out confidence, out semanticSummary, out notes);
                }
                else if (pair.Key.EndsWith(".Val2", StringComparison.OrdinalIgnoreCase))
                {
                    TryDecodeEnumField(distinctValues, AbilityOperationNames, "AbilityOperation", out confidence, out semanticSummary, out notes);
                }
                else if (pair.Key.EndsWith(".Val3", StringComparison.OrdinalIgnoreCase))
                {
                    TryDecodeEnumField(distinctValues, AbilityConditionNames, "AbilityCondition", out confidence, out semanticSummary, out notes);
                }
                else if (pair.Key.EndsWith(".Val4", StringComparison.OrdinalIgnoreCase))
                {
                    TryDecodeEnumField(distinctValues, AbilityLogicOperatorNames, "AbilityLogicOperator", out confidence, out semanticSummary, out notes);
                }
                else if (pair.Key.EndsWith(".Val5", StringComparison.OrdinalIgnoreCase))
                {
                    confidence = SemanticConfidence.Inferred;
                    semanticSummary = "Operation-specific threshold or comparison value (semantics depend on AbilityOperation). Position confirmed from decompiled client ExtData schema.";
                }
                else if (pair.Key.EndsWith(".Val6", StringComparison.OrdinalIgnoreCase))
                {
                    List<ushort> matchedRequirementIds = distinctValues
                        .Select(ParseRequirementId)
                        .Where(row => row.HasValue)
                        .Select(row => row.Value)
                        .Distinct()
                        .OrderBy(row => row)
                        .ToList();

                    if (matchedRequirementIds.Count > 0)
                    {
                        confidence = SemanticConfidence.Inferred;
                        semanticSummary = matchedRequirementIds.Count == distinctValues.Count
                            ? "Every distinct observed value in this field matches a known RequirementId row."
                            : "Some observed values in this field match known RequirementId rows.";
                        notes = "Example requirement ids: " + string.Join(", ", matchedRequirementIds.Take(12).Select(row => row.ToString(CultureInfo.InvariantCulture))) + ".";
                    }
                    else
                    {
                        confidence = SemanticConfidence.Inferred;
                        semanticSummary = "Operation-specific reference value (RequirementId child link, ability ID, monster type, or other reference depending on AbilityOperation). Position confirmed from decompiled client ExtData schema.";
                    }
                }
                else if (pair.Key.EndsWith(".Val7", StringComparison.OrdinalIgnoreCase)
                      || pair.Key.EndsWith(".Val8", StringComparison.OrdinalIgnoreCase))
                {
                    confidence = SemanticConfidence.Inferred;
                    semanticSummary = "Operation-specific auxiliary parameter. Position confirmed from decompiled client ExtData schema.";
                }
                else if (pair.Key.EndsWith(".Val9", StringComparison.OrdinalIgnoreCase))
                {
                    confidence = SemanticConfidence.Inferred;
                    semanticSummary = "Binary flag byte. Position confirmed from decompiled client ExtData schema.";
                }

                fields.Add(new RequirementFieldRecord
                {
                    FieldKey = pair.Key,
                    NonZeroCount = pair.Value.Count,
                    DistinctValueCount = distinctValues.Count,
                    SampleValuesText = JoinValues(distinctValues.Take(12)),
                    SemanticSummary = semanticSummary,
                    Confidence = confidence,
                    Notes = notes
                });
            }

            return fields;
        }

        private RequirementLedgerRecord ToLedgerRecord(RequirementAnalysisRecord analysis)
        {
            int directAbilityCount = analysis.DirectAbilityContexts
                .Select(row => row.AbilityId)
                .Distinct()
                .Count();
            int directComponentCount = analysis.InboundReferences
                .Where(row => string.Equals(row.SourceKind, "Component", StringComparison.OrdinalIgnoreCase))
                .Select(row => row.SourceId)
                .Distinct()
                .Count();
            int parentRequirementCount = analysis.InboundReferences
                .Where(row => string.Equals(row.SourceKind, "Requirement", StringComparison.OrdinalIgnoreCase))
                .Select(row => row.SourceId)
                .Distinct()
                .Count();
            int childRequirementCount = analysis.OutboundReferences
                .Select(row => row.LinkedRequirementId)
                .Distinct()
                .Count();

            return new RequirementLedgerRecord
            {
                RequirementId = analysis.RequirementId,
                RecordCount = analysis.Rows.Count,
                FieldCount = analysis.Fields.Count,
                DirectAbilityCount = directAbilityCount,
                DirectComponentCount = directComponentCount,
                ParentRequirementCount = parentRequirementCount,
                ChildRequirementCount = childRequirementCount,
                ContextTagsText = JoinValues(analysis.ContextTags),
                SampleAbilitiesText = JoinValues(analysis.SampleAbilities),
                SemanticSummary = analysis.SemanticSummary,
                Notes = analysis.Notes,
                Rows = analysis.Rows
                    .OrderBy(row => row.RecordIndex)
                    .Select(row => new RequirementRowRecord
                    {
                        RecordIndex = row.RecordIndex,
                        ExtDataText = FormatExtData(row.ExtData),
                        SourcePath = row.SourcePath,
                        SourceLocation = FormatSourceLocation(row)
                    })
                    .ToList(),
                Fields = analysis.Fields,
                InboundReferences = analysis.InboundReferences
                    .OrderBy(row => row.SourceKind, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(row => row.SourceId)
                    .ThenBy(row => row.ExtSlotIndex)
                    .ToList(),
                OutboundReferences = analysis.OutboundReferences
                    .OrderBy(row => row.LinkedRequirementId)
                    .ThenBy(row => row.ExtSlotIndex)
                    .ToList()
            };
        }

        private string BuildSemanticSummary(RequirementAnalysisRecord analysis)
        {
            List<string> parts = new List<string>();
            if (analysis.ContextTags.Count > 0)
                parts.Add("Observed in " + JoinValues(analysis.ContextTags.Take(4)) + " contexts");

            int directAbilityCount = analysis.DirectAbilityContexts.Select(row => row.AbilityId).Distinct().Count();
            if (directAbilityCount > 0)
                parts.Add("direct ability usage " + directAbilityCount.ToString(CultureInfo.InvariantCulture));

            int directComponentCount = analysis.InboundReferences
                .Where(row => string.Equals(row.SourceKind, "Component", StringComparison.OrdinalIgnoreCase))
                .Select(row => row.SourceId)
                .Distinct()
                .Count();
            if (directComponentCount > 0)
                parts.Add("component sources " + directComponentCount.ToString(CultureInfo.InvariantCulture));

            List<ushort> childRequirementIds = analysis.OutboundReferences
                .Select(row => row.LinkedRequirementId)
                .Distinct()
                .OrderBy(row => row)
                .ToList();
            if (childRequirementIds.Count > 0)
                parts.Add("child requirement ids " + string.Join(", ", childRequirementIds.Take(8).Select(row => row.ToString(CultureInfo.InvariantCulture))));

            int parentRequirementCount = analysis.InboundReferences
                .Where(row => string.Equals(row.SourceKind, "Requirement", StringComparison.OrdinalIgnoreCase))
                .Select(row => row.SourceId)
                .Distinct()
                .Count();
            if (parts.Count == 0 && parentRequirementCount > 0)
                parts.Add("referenced from " + parentRequirementCount.ToString(CultureInfo.InvariantCulture) + " parent requirement row(s)");

            if (parts.Count == 0)
                parts.Add("No direct ability or component context is decoded yet");

            return string.Join("; ", parts) + ".";
        }

        private string BuildAnalysisNotes(RequirementAnalysisRecord analysis)
        {
            List<ushort> childRequirementIds = analysis.OutboundReferences
                .Select(row => row.LinkedRequirementId)
                .Distinct()
                .OrderBy(row => row)
                .Take(12)
                .ToList();
            int directAbilityCount = analysis.DirectAbilityContexts.Select(row => row.AbilityId).Distinct().Count();
            int directComponentCount = analysis.InboundReferences
                .Where(row => string.Equals(row.SourceKind, "Component", StringComparison.OrdinalIgnoreCase))
                .Select(row => row.SourceId)
                .Distinct()
                .Count();
            int parentRequirementCount = analysis.InboundReferences
                .Where(row => string.Equals(row.SourceKind, "Requirement", StringComparison.OrdinalIgnoreCase))
                .Select(row => row.SourceId)
                .Distinct()
                .Count();

            StringBuilder builder = new StringBuilder();
            builder.Append("Requirement rows: ");
            builder.Append(analysis.Rows.Count.ToString(CultureInfo.InvariantCulture));
            builder.Append("; direct abilities: ");
            builder.Append(directAbilityCount.ToString(CultureInfo.InvariantCulture));
            builder.Append("; direct components: ");
            builder.Append(directComponentCount.ToString(CultureInfo.InvariantCulture));
            builder.Append("; parent requirements: ");
            builder.Append(parentRequirementCount.ToString(CultureInfo.InvariantCulture));

            if (childRequirementIds.Count > 0)
            {
                builder.Append("; child requirements: ");
                builder.Append(string.Join(", ", childRequirementIds.Select(row => row.ToString(CultureInfo.InvariantCulture))));
            }

            if (analysis.SampleAbilities.Count > 0)
            {
                builder.Append("; sample abilities: ");
                builder.Append(JoinValues(analysis.SampleAbilities));
            }

            if (analysis.ContextTags.Count > 0)
            {
                builder.Append("; context tags: ");
                builder.Append(JoinValues(analysis.ContextTags));
            }

            builder.Append(".");
            return builder.ToString();
        }

        private static void AddRequirementFieldValue(Dictionary<string, List<string>> valuesByField, int slotIndex, int valueIndex, int rawValue)
        {
            if (rawValue == 0)
                return;

            AddRequirementFieldValue(valuesByField, slotIndex, valueIndex, rawValue.ToString(CultureInfo.InvariantCulture));
        }

        private static void AddRequirementFieldValue(Dictionary<string, List<string>> valuesByField, int slotIndex, int valueIndex, byte rawValue)
        {
            if (rawValue == 0)
                return;

            AddRequirementFieldValue(valuesByField, slotIndex, valueIndex, rawValue.ToString(CultureInfo.InvariantCulture));
        }

        private static void AddRequirementFieldValue(Dictionary<string, List<string>> valuesByField, int slotIndex, int valueIndex, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return;

            string fieldKey = "ExtData[" + slotIndex.ToString(CultureInfo.InvariantCulture) + "].Val" + valueIndex.ToString(CultureInfo.InvariantCulture);
            List<string> values;
            if (!valuesByField.TryGetValue(fieldKey, out values))
            {
                values = new List<string>();
                valuesByField[fieldKey] = values;
            }

            values.Add(rawValue);
        }

        private static string BuildFieldSortKey(string fieldKey)
        {
            int slotIndex = 99;
            int valueIndex = 99;

            int openBracket = (fieldKey ?? string.Empty).IndexOf('[');
            int closeBracket = (fieldKey ?? string.Empty).IndexOf(']');
            int valueToken = (fieldKey ?? string.Empty).LastIndexOf("Val", StringComparison.OrdinalIgnoreCase);

            if (openBracket >= 0 && closeBracket > openBracket)
                int.TryParse(fieldKey.Substring(openBracket + 1, closeBracket - openBracket - 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out slotIndex);
            if (valueToken >= 0 && valueToken + 3 < (fieldKey ?? string.Empty).Length)
                int.TryParse(fieldKey.Substring(valueToken + 3), NumberStyles.Integer, CultureInfo.InvariantCulture, out valueIndex);

            return slotIndex.ToString("D2", CultureInfo.InvariantCulture) + "." + valueIndex.ToString("D2", CultureInfo.InvariantCulture);
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

        private bool TryGetKnownRequirementId(int rawValue, out ushort requirementId)
        {
            requirementId = 0;
            if (rawValue <= 0 || rawValue > ushort.MaxValue)
                return false;

            requirementId = (ushort)rawValue;
            return _knownRequirementIds.Contains(requirementId);
        }

        private static void TryDecodeEnumField(
            List<string> distinctValues,
            Dictionary<int, string> enumNames,
            string enumTypeName,
            out string confidence,
            out string semanticSummary,
            out string notes)
        {
            confidence = SemanticConfidence.Unknown;
            semanticSummary = string.Empty;
            notes = string.Empty;

            if (distinctValues == null || distinctValues.Count == 0)
                return;

            List<string> namedEntries = new List<string>();
            int matchCount = 0;
            foreach (string rawValue in distinctValues)
            {
                int intValue;
                if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out intValue))
                    continue;

                string enumName;
                if (enumNames.TryGetValue(intValue, out enumName))
                {
                    namedEntries.Add(intValue.ToString(CultureInfo.InvariantCulture) + "=" + enumName);
                    matchCount++;
                }
                else
                {
                    namedEntries.Add(intValue.ToString(CultureInfo.InvariantCulture) + "=Unk");
                }
            }

            if (matchCount == 0)
                return;

            confidence = matchCount == distinctValues.Count ? SemanticConfidence.Confirmed : SemanticConfidence.Inferred;
            semanticSummary = enumTypeName + ": " + string.Join(", ", namedEntries) + ".";
        }

        private string BuildAbilityLabel(ushort abilityId)
        {
            string name;
            return _abilityNamesById.TryGetValue(abilityId, out name)
                ? "Ability " + abilityId.ToString(CultureInfo.InvariantCulture) + " (" + name + ")"
                : "Ability " + abilityId.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatAbilityLabel(ushort abilityId, string abilityName)
        {
            return string.IsNullOrWhiteSpace(abilityName)
                ? abilityId.ToString(CultureInfo.InvariantCulture)
                : abilityId.ToString(CultureInfo.InvariantCulture) + " (" + abilityName + ")";
        }

        private static Dictionary<ushort, string> BuildAbilityNames(AbilityDataset dataset)
        {
            Dictionary<ushort, string> names = new Dictionary<ushort, string>();
            if (dataset == null)
                return names;

            // abilitynames.txt only: abilities.csv is keyed by effect id, not ability id.
            foreach (IndexedStringRecord row in dataset.AbilityNames.Where(row => row.EntryId <= ushort.MaxValue && !string.IsNullOrWhiteSpace(row.NormalizedValue)))
            {
                ushort abilityId = (ushort)row.EntryId;
                if (!names.ContainsKey(abilityId))
                    names[abilityId] = row.NormalizedValue;
            }

            return names;
        }

        private static Dictionary<ushort, AbilityContext> BuildAbilityContexts(AbilityDataset dataset, IDictionary<ushort, string> abilityNamesById)
        {
            Dictionary<ushort, AbilityContext> contexts = new Dictionary<ushort, AbilityContext>();
            if (dataset == null)
                return contexts;

            Dictionary<ushort, string> stringDescriptions = dataset.AbilityDescriptions
                .Where(row => row.EntryId <= ushort.MaxValue && !string.IsNullOrWhiteSpace(row.NormalizedValue))
                .GroupBy(row => (ushort)row.EntryId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.LineNumber).Select(row => row.NormalizedValue).FirstOrDefault() ?? string.Empty);
            Dictionary<ushort, string> effectTexts = dataset.AbilityEffectTexts
                .Where(row => row.EntryId <= ushort.MaxValue && !string.IsNullOrWhiteSpace(row.NormalizedValue))
                .GroupBy(row => (ushort)row.EntryId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.LineNumber).Select(row => row.NormalizedValue).FirstOrDefault() ?? string.Empty);

            HashSet<ushort> abilityIds = new HashSet<ushort>(abilityNamesById.Keys);
            foreach (ushort abilityId in stringDescriptions.Keys)
                abilityIds.Add(abilityId);
            foreach (ushort abilityId in effectTexts.Keys)
                abilityIds.Add(abilityId);

            foreach (ushort abilityId in abilityIds.OrderBy(row => row))
            {
                string abilityName;
                string stringDescription;
                string effectText;
                abilityNamesById.TryGetValue(abilityId, out abilityName);
                stringDescriptions.TryGetValue(abilityId, out stringDescription);
                effectTexts.TryGetValue(abilityId, out effectText);

                string combinedText = string.Join(" ", new[]
                {
                    abilityName ?? string.Empty,
                    stringDescription ?? string.Empty,
                    effectText ?? string.Empty
                }.Where(row => !string.IsNullOrWhiteSpace(row)));

                contexts[abilityId] = new AbilityContext
                {
                    AbilityId = abilityId,
                    AbilityName = abilityName ?? string.Empty,
                    TextExcerpt = FirstNonEmpty(stringDescription, effectText, abilityName),
                    ContextTags = BuildContextTags(combinedText)
                };
            }

            return contexts;
        }

        private static Dictionary<ushort, List<AbilityComponentUsage>> BuildAbilityUsagesByComponentId(AbilityDataset dataset, IDictionary<ushort, AbilityContext> abilityContextsById)
        {
            Dictionary<ushort, List<AbilityComponentUsage>> usagesByComponentId = new Dictionary<ushort, List<AbilityComponentUsage>>();
            if (dataset == null)
                return usagesByComponentId;

            Dictionary<ushort, BinaryComponentRecord> componentsById = dataset.BinaryComponents
                .GroupBy(row => row.ComponentId)
                .ToDictionary(group => group.Key, group => group.OrderBy(row => row.RecordIndex).First());

            foreach (BinaryAbilityRecord abilityRow in dataset.BinaryAbilities)
            {
                AbilityContext abilityContext;
                abilityContextsById.TryGetValue(abilityRow.AbilityId, out abilityContext);

                for (int index = 0; index < abilityRow.ComponentIds.Count; ++index)
                {
                    ushort componentId = abilityRow.ComponentIds[index];
                    if (componentId <= 0)
                        continue;

                    List<AbilityComponentUsage> usages;
                    if (!usagesByComponentId.TryGetValue(componentId, out usages))
                    {
                        usages = new List<AbilityComponentUsage>();
                        usagesByComponentId[componentId] = usages;
                    }

                    BinaryComponentRecord componentRow;
                    componentsById.TryGetValue(componentId, out componentRow);
                    string componentDescription = componentRow == null ? string.Empty : componentRow.Description;
                    uint triggerValue = index < abilityRow.Triggers.Count ? abilityRow.Triggers[index] : 0;
                    List<string> contextTags = new List<string>();
                    if (abilityContext != null && abilityContext.ContextTags != null)
                        contextTags.AddRange(abilityContext.ContextTags);
                    contextTags.AddRange(BuildContextTags(componentDescription));

                    string key = abilityRow.AbilityId.ToString(CultureInfo.InvariantCulture)
                        + "|"
                        + componentId.ToString(CultureInfo.InvariantCulture)
                        + "|"
                        + index.ToString(CultureInfo.InvariantCulture);
                    if (usages.Any(row => string.Equals(row.Key, key, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    usages.Add(new AbilityComponentUsage
                    {
                        Key = key,
                        AbilityId = abilityRow.AbilityId,
                        AbilityName = abilityContext == null ? string.Empty : abilityContext.AbilityName,
                        ComponentId = componentId,
                        ComponentSlotIndex = index,
                        TriggerText = triggerValue > 0 ? DefinitionCatalog.DescribeTriggerValue(triggerValue) : string.Empty,
                        TextExcerpt = FirstNonEmpty(componentDescription, abilityContext == null ? string.Empty : abilityContext.TextExcerpt),
                        ContextTags = contextTags.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(row => row, StringComparer.OrdinalIgnoreCase).ToList()
                    });
                }
            }

            foreach (KeyValuePair<ushort, List<AbilityComponentUsage>> pair in usagesByComponentId.ToList())
                usagesByComponentId[pair.Key] = pair.Value.OrderBy(row => row.AbilityId).ThenBy(row => row.ComponentSlotIndex).ToList();

            return usagesByComponentId;
        }

        private static string FirstNonEmpty(params string[] values)
        {
            return values.FirstOrDefault(row => !string.IsNullOrWhiteSpace(row)) ?? string.Empty;
        }

        private static List<string> BuildContextTags(string text)
        {
            List<string> tags = new List<string>();
            string normalized = (text ?? string.Empty).ToLowerInvariant();
            AddTagIfPresent(tags, normalized, "knockback", "Knockback");
            AddTagIfPresent(tags, normalized, "knock down", "Knockdown");
            AddTagIfPresent(tags, normalized, "knockdown", "Knockdown");
            AddTagIfPresent(tags, normalized, "crowd control", "CrowdControl");
            AddTagIfPresent(tags, normalized, "immunity", "Immunity");
            AddTagIfPresent(tags, normalized, "immune", "Immunity");
            AddTagIfPresent(tags, normalized, "stagger", "Stagger");
            AddTagIfPresent(tags, normalized, "stun", "Stun");
            AddTagIfPresent(tags, normalized, "root", "Root");
            AddTagIfPresent(tags, normalized, "snare", "Snare");
            AddTagIfPresent(tags, normalized, "silence", "Silence");
            AddTagIfPresent(tags, normalized, "disarm", "Disarm");
            AddTagIfPresent(tags, normalized, "heal", "Heal");
            AddTagIfPresent(tags, normalized, "damage", "Damage");
            return tags;
        }

        private static void AddTagIfPresent(List<string> tags, string text, string needle, string tag)
        {
            if (text.Contains(needle) && !tags.Contains(tag))
                tags.Add(tag);
        }

        private static string FormatExtData(IList<BinaryExtDataRecord> extData)
        {
            if (extData == null || extData.Count == 0)
                return string.Empty;

            return string.Join(" ; ", extData.Select(record =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "slot{0}=[{1},{2},{3},{4},{5},{6},{7},{8},{9}]",
                    record.SlotIndex,
                    record.Val1,
                    record.Val2,
                    record.Val3,
                    record.Val4,
                    record.Val5,
                    record.Val6,
                    record.Val7,
                    record.Val8,
                    record.Val9)));
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

        private static long ParseSortBucket(string rawValue)
        {
            long parsedValue;
            return long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedValue) ? parsedValue : long.MaxValue;
        }

        private static string JoinValues(IEnumerable<string> values)
        {
            List<string> items = values == null
                ? new List<string>()
                : values.Where(row => !string.IsNullOrWhiteSpace(row)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return items.Count == 0 ? string.Empty : string.Join(", ", items);
        }

        private sealed class AbilityContext
        {
            public ushort AbilityId { get; set; }
            public string AbilityName { get; set; }
            public string TextExcerpt { get; set; }
            public List<string> ContextTags { get; set; }
        }

        private sealed class AbilityComponentUsage
        {
            public string Key { get; set; }
            public ushort AbilityId { get; set; }
            public string AbilityName { get; set; }
            public ushort ComponentId { get; set; }
            public int ComponentSlotIndex { get; set; }
            public string TriggerText { get; set; }
            public string TextExcerpt { get; set; }
            public List<string> ContextTags { get; set; }
        }

        private sealed class AbilityUsageContext
        {
            public ushort AbilityId { get; set; }
            public string AbilityName { get; set; }
            public string SourceKind { get; set; }
            public List<string> ContextTags { get; set; }
        }

        private sealed class RequirementAnalysisRecord
        {
            public ushort RequirementId { get; set; }
            public List<BinaryRequirementRecord> Rows { get; set; }
            public List<RequirementFieldRecord> Fields { get; set; }
            public List<RequirementLinkEvidenceRecord> InboundReferences { get; set; }
            public List<RequirementLinkEvidenceRecord> OutboundReferences { get; set; }
            public List<AbilityUsageContext> DirectAbilityContexts { get; set; }
            public List<string> ContextTags { get; set; }
            public List<string> SampleAbilities { get; set; }
            public string SemanticSummary { get; set; }
            public string Notes { get; set; }
        }
    }

    public sealed class RequirementSemanticDescription
    {
        public string Meaning { get; set; }
        public string Confidence { get; set; }
        public string SourcePath { get; set; }
        public string SourceLocation { get; set; }
        public string Notes { get; set; }
    }
}

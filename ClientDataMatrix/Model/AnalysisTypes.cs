using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ClientDataMatrix.Model
{
    public sealed class TableLoadStatus
    {
        public string SourceFamily { get; set; }
        public string TableName { get; set; }
        public string SourcePath { get; set; }
        public bool Loaded { get; set; }
        public int RowCount { get; set; }
        public string ErrorMessage { get; set; }
    }

    public abstract class SourceRowBase
    {
        public string SourceFamily { get; set; }
        public string TableName { get; set; }
        public string SourcePath { get; set; }
        public string RowKey { get; set; }
        public int LineNumber { get; set; }
        public long ByteOffset { get; set; }
        public string RawRow { get; set; }
    }

    public sealed class ClientAbilityRecord : SourceRowBase
    {
        public uint AbilityId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public int IconId { get; set; }
        public int AnimationId { get; set; }
        public int EffectId { get; set; }
        public int? PlaybackAnimationId { get; set; }
        public int? ActivateAggro { get; set; }
    }

    public sealed class ClientEffectRecord : SourceRowBase
    {
        public uint EffectId { get; set; }
        public string Name { get; set; }
        public int BuildUpId { get; set; }
        public int ActivateId { get; set; }
        public int CastId { get; set; }
        public int ProjectileId { get; set; }
        public int ImpactId { get; set; }
        public int AoeId { get; set; }
        public int ChannelingId { get; set; }
        public int VfxId { get; set; }
        public int AoeTarget { get; set; }
        public int AoeEffectsPerSecond { get; set; }
        public int AoeRadius { get; set; }
        public int AoeDuration { get; set; }
        public int AoeLocation { get; set; }
        public int WeaponTrail { get; set; }
        public int ProjectileOffset { get; set; }
        public int ProjectileOverride { get; set; }
    }

    public sealed class IndexedStringRecord : SourceRowBase
    {
        public string StringSet { get; set; }
        public string Locale { get; set; }
        public uint EntryId { get; set; }
        public string Value { get; set; }
        public string NormalizedValue { get; set; }
    }

    public sealed class PregameCharacterRecord : SourceRowBase
    {
        public int Sequence { get; set; }
        public string SceneType { get; set; }
        public string CareerName { get; set; }
        public string RaceName { get; set; }
        public int? OnLoadAbilityId { get; set; }
        public int? MouseOverAbilityId { get; set; }
        public int? OnClickAbilityId { get; set; }
    }

    public sealed class BinaryExtDataRecord
    {
        public int SlotIndex { get; set; }
        public int Val1 { get; set; }
        public int Val2 { get; set; }
        public int Val3 { get; set; }
        public int Val4 { get; set; }
        public int Val5 { get; set; }
        public int Val6 { get; set; }
        public int Val7 { get; set; }
        public int Val8 { get; set; }
        public byte Val9 { get; set; }
    }

    public sealed class BinaryAbilityRecord : SourceRowBase
    {
        public int RecordIndex { get; set; }
        public uint Header { get; set; }
        public ushort AbilityId { get; set; }
        public ushort EffectId { get; set; }
        public uint CastTime { get; set; }
        public uint Cooldown { get; set; }
        public uint TacticType { get; set; }
        public uint TargetType { get; set; }
        public uint AbilityType { get; set; }
        public uint AttackType { get; set; }
        public uint Opaque24 { get; set; }
        public uint CareerLine { get; set; }
        public uint CounterAmount { get; set; }
        public uint FlagsRaw { get; set; }
        public ushort Value44 { get; set; }
        public ushort Range { get; set; }
        public ushort Angle { get; set; }
        public ushort MoraleCost { get; set; }
        public ushort ChannelInterval { get; set; }
        public ushort Value54 { get; set; }
        public ushort ScaleStatMultiplier { get; set; }
        public byte NumTacticSlots { get; set; }
        public byte MoraleLevel { get; set; }
        public byte ApCost { get; set; }
        public byte Byte61 { get; set; }
        public byte Byte62 { get; set; }
        public byte Faction { get; set; }
        public byte ImprovementThreshold { get; set; }
        public byte ImprovementCap { get; set; }
        public byte Specialization { get; set; }
        public byte StanceOrder { get; set; }
        public byte Byte68 { get; set; }
        public byte MinLevel { get; set; }
        public List<ushort> ComponentIds { get; set; }
        public List<uint> Triggers { get; set; }
        public List<ushort> UsableWithBuff { get; set; }
        public uint Value136 { get; set; }
        public ushort Value140 { get; set; }
        public List<ushort> Groups { get; set; }
        public List<ushort> Labels { get; set; }
        public List<byte> ComponentVfx { get; set; }
        public ushort Value170 { get; set; }
        public List<BinaryExtDataRecord> ExtData { get; set; }
        public bool IsGranted { get; set; }
        public bool IsPassive { get; set; }
        public bool IsBuff { get; set; }
        public bool IsDebuff { get; set; }
        public bool IsDamaging { get; set; }
        public bool IsHealing { get; set; }
        public bool IsDefensive { get; set; }
        public bool IsOffensive { get; set; }
        public bool RequiresPet { get; set; }
        public bool IsStatsBuff { get; set; }
        public bool IsBuffDebuff { get; set; }
    }

    public sealed class BinaryComponentRecord : SourceRowBase
    {
        public int RecordIndex { get; set; }
        public uint Header { get; set; }
        public string LayoutVariant { get; set; }
        public ushort ComponentId { get; set; }
        public ushort Value00 { get; set; }
        public List<BinaryExtDataRecord> ExtData { get; set; }
        public List<int> Values { get; set; }
        public List<int> Multipliers { get; set; }
        public uint ActivationDelay { get; set; }
        public uint Duration { get; set; }
        public uint FlagsRaw { get; set; }
        public uint Value08 { get; set; }
        public uint Operation { get; set; }
        public uint Interval { get; set; }
        public ushort Radius { get; set; }
        public ushort ConeAngle { get; set; }
        public ushort FlightSpeed { get; set; }
        public ushort Value15 { get; set; }
        public byte MaxTargets { get; set; }
        public string Description { get; set; }
    }

    public sealed class BinaryRequirementRecord : SourceRowBase
    {
        public int RecordIndex { get; set; }
        public uint Header { get; set; }
        public ushort RequirementId { get; set; }
        public List<BinaryExtDataRecord> ExtData { get; set; }
    }

    public sealed class GraphNode
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Key { get; set; }
        public string Label { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }

    public sealed class GraphEdge
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string FromNodeId { get; set; }
        public string ToNodeId { get; set; }
        public string Label { get; set; }
        public string ClaimId { get; set; }
    }

    public sealed class ClaimRecord
    {
        public string Id { get; set; }
        public string SubjectKey { get; set; }
        public string FactName { get; set; }
        public string Domain { get; set; }
        public string ValueKey { get; set; }
        public string RawValue { get; set; }
        public string NormalizedValue { get; set; }
        public string SourceFamily { get; set; }
        public string TableName { get; set; }
        public string SourcePath { get; set; }
        public string RowKey { get; set; }
        public int LineNumber { get; set; }
        public long ByteOffset { get; set; }
        public string FieldName { get; set; }
        public string Confidence { get; set; }
        public string Notes { get; set; }
    }

    public sealed class ConflictRecord
    {
        public string SubjectKey { get; set; }
        public string FactName { get; set; }
        public string Domain { get; set; }
        public List<string> DistinctValues { get; set; }
        public List<ClaimRecord> Claims { get; set; }
        public int TriageScore { get; set; }
        public string TriageBucket { get; set; }
        public string TriageCategory { get; set; }
        public string TriageNotes { get; set; }
        public bool IsNoise { get; set; }
        public string ResolutionRule { get; set; }
        public string RecommendedSourceFamily { get; set; }
        public string CanonicalValue { get; set; }
        public string CanonicalMeaning { get; set; }
        public string ResolutionNotes { get; set; }
    }

    public sealed class AnalysisGraph
    {
        private readonly Dictionary<string, GraphNode> _nodes = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);
        private readonly List<GraphEdge> _edges = new List<GraphEdge>();
        private readonly List<ClaimRecord> _claims = new List<ClaimRecord>();

        public List<GraphNode> Nodes
        {
            get { return _nodes.Values.OrderBy(node => node.Kind).ThenBy(node => node.Key).ToList(); }
        }

        public List<GraphEdge> Edges
        {
            get { return _edges.OrderBy(edge => edge.Kind).ThenBy(edge => edge.FromNodeId).ThenBy(edge => edge.ToNodeId).ToList(); }
        }

        public List<ClaimRecord> Claims
        {
            get { return _claims.OrderBy(claim => claim.SubjectKey).ThenBy(claim => claim.FactName).ThenBy(claim => claim.SourcePath).ThenBy(claim => claim.LineNumber).ToList(); }
        }

        public GraphNode AddNode(string kind, string key, string label)
        {
            string id = kind + ":" + key;
            GraphNode existingNode;
            if (_nodes.TryGetValue(id, out existingNode))
                return existingNode;

            GraphNode node = new GraphNode
            {
                Id = id,
                Kind = kind,
                Key = key,
                Label = label,
                Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };

            _nodes[id] = node;
            return node;
        }

        public void AddNodeProperty(GraphNode node, string propertyName, string propertyValue)
        {
            if (node == null || string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(propertyValue))
                return;

            if (!node.Properties.ContainsKey(propertyName))
                node.Properties[propertyName] = propertyValue;
        }

        public GraphEdge AddEdge(string kind, GraphNode fromNode, GraphNode toNode, string label, string claimId)
        {
            if (fromNode == null || toNode == null)
                return null;

            GraphEdge edge = new GraphEdge
            {
                Id = "edge:" + (_edges.Count + 1).ToString(CultureInfo.InvariantCulture),
                Kind = kind,
                FromNodeId = fromNode.Id,
                ToNodeId = toNode.Id,
                Label = label,
                ClaimId = claimId
            };

            _edges.Add(edge);
            return edge;
        }

        public ClaimRecord AddClaim(ClaimRecord claim)
        {
            if (claim == null)
                return null;

            if (string.IsNullOrWhiteSpace(claim.Id))
                claim.Id = "claim:" + (_claims.Count + 1).ToString(CultureInfo.InvariantCulture);

            _claims.Add(claim);
            return claim;
        }

        public List<ConflictRecord> GetConflicts()
        {
            return _claims
                .GroupBy(claim => claim.SubjectKey + "|" + claim.FactName + "|" + claim.Domain)
                .Select(group =>
                {
                    List<ClaimRecord> groupClaims = group.ToList();
                    List<string> distinctValues = groupClaims
                        .Select(claim => claim.NormalizedValue ?? string.Empty)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(value => value)
                        .ToList();

                    return new ConflictRecord
                    {
                        SubjectKey = groupClaims[0].SubjectKey,
                        FactName = groupClaims[0].FactName,
                        Domain = groupClaims[0].Domain,
                        DistinctValues = distinctValues,
                        Claims = groupClaims
                    };
                })
                .Where(conflict => conflict.DistinctValues.Count > 1)
                .OrderBy(conflict => conflict.SubjectKey)
                .ThenBy(conflict => conflict.FactName)
                .ToList();
        }
    }

    /// <summary>One ability reached by following component references from the subject ability.</summary>
    public sealed class AbilityChainEntry
    {
        public int Depth { get; set; }
        public ushort AbilityId { get; set; }
        public string Name { get; set; }
        public string ArrivedFrom { get; set; }
        public string Via { get; set; }
        public string Operation { get; set; }
        public string Field { get; set; }
        public string Basis { get; set; }
        public string ComponentIds { get; set; }
        public bool Revisited { get; set; }
    }

    public sealed class AbilityAnalysisResult
    {
        public ushort AbilityId { get; set; }
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public AnalysisGraph Graph { get; set; }
        public List<TableLoadStatus> TableStatuses { get; set; }
        public List<string> Warnings { get; set; }
        public List<ClientAbilityRecord> ClientAbilityRows { get; set; }
        public List<ClientEffectRecord> ClientEffectRows { get; set; }
        public List<IndexedStringRecord> AbilityNameRows { get; set; }
        public List<IndexedStringRecord> AbilityDescriptionRows { get; set; }
        public List<IndexedStringRecord> AbilityEffectTextRows { get; set; }
        public List<IndexedStringRecord> ComponentEffectRows { get; set; }
        public List<PregameCharacterRecord> PregameRows { get; set; }
        public List<int> RelatedEffectIds { get; set; }
        public List<ushort> RelatedComponentIds { get; set; }
        public List<BinaryAbilityRecord> BinaryAbilityRows { get; set; }
        public List<BinaryComponentRecord> BinaryComponentRows { get; set; }

        /// <summary>
        /// The ability-to-ability chain reachable from this ability through its components.
        /// Empty for an ability whose behaviour is entirely its own.
        /// </summary>
        public List<AbilityChainEntry> AbilityChain { get; set; }
        public List<BinaryRequirementRecord> BinaryRequirementRows { get; set; }
        public List<RequirementReferenceRecord> RequirementReferences { get; set; }
    }

    public sealed class RequirementReferenceRecord
    {
        public string SourceKind { get; set; }
        public ushort SourceId { get; set; }
        public string SourceLabel { get; set; }
        public string SourceField { get; set; }
        public int ExtSlotIndex { get; set; }
        public ushort RequirementId { get; set; }
        public string Confidence { get; set; }
        public string Notes { get; set; }
        public string SourceFamily { get; set; }
        public string SourceTableName { get; set; }
        public string SourcePath { get; set; }
        public string SourceRowKey { get; set; }
        public int LineNumber { get; set; }
        public long ByteOffset { get; set; }
        public string SourceLocation { get; set; }
    }

    public sealed class ConflictReportDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public List<TableLoadStatus> TableStatuses { get; set; }
        public List<ClaimRecord> Claims { get; set; }
        public List<ConflictRecord> Conflicts { get; set; }
    }

    public sealed class CoverageReportDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public List<TableLoadStatus> TableStatuses { get; set; }
        public List<CoverageAbilityRecord> Abilities { get; set; }
    }

    public sealed class DomainLedgerDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public List<TableLoadStatus> TableStatuses { get; set; }
        public List<IdentityDomainRecord> Domains { get; set; }
    }

    public sealed class IdentityDomainRecord
    {
        public string DomainKey { get; set; }
        public string DisplayName { get; set; }
        public string Confidence { get; set; }
        public string Canonicality { get; set; }
        public string SourceFilesText { get; set; }
        public int ValueCount { get; set; }
        public int DistinctMeaningCount { get; set; }
        public int DuplicateMeaningCount { get; set; }
        public bool DuplicatesAreExpected { get; set; }
        public string RecommendedUsage { get; set; }
        public string Notes { get; set; }
        public List<IdentityDomainValueRecord> Values { get; set; }
    }

    public sealed class IdentityDomainValueRecord
    {
        public string RawValue { get; set; }
        public string Meaning { get; set; }
        public string Confidence { get; set; }
        public string Source { get; set; }
        public string SourcePath { get; set; }
        public string SourceLocation { get; set; }
        public string Notes { get; set; }
    }

    public sealed class CoverageAbilityRecord
    {
        public ushort AbilityId { get; set; }
        public string Name { get; set; }
        public int? EffectId { get; set; }
        public bool HasClientCsv { get; set; }
        public bool HasClientBin { get; set; }
        public bool HasLocalizedName { get; set; }
        public bool HasDescriptionText { get; set; }
        public bool HasEffectText { get; set; }
        public bool HasRootEffectRow { get; set; }
        public int RelatedEffectCount { get; set; }
        public int ComponentCount { get; set; }
        public int RequirementLinkCount { get; set; }
        public int RequirementRowCount { get; set; }
        public bool HasPregameReference { get; set; }
        public string CoverageStatus { get; set; }
        public string MissingText { get; set; }
        public string SourcesText { get; set; }
    }

    public sealed class TokenDictionaryDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public string TokenGrammar { get; set; }
        public string TokenGrammarMeaning { get; set; }
        public string OverrideLedgerPath { get; set; }
        public List<TokenDefinitionRecord> Definitions { get; set; }
    }

    public sealed class TokenDefinitionRecord
    {
        public string TokenBody { get; set; }
        public string ExampleToken { get; set; }
        public string FieldKey { get; set; }
        public string PlainEnglishMeaning { get; set; }
        public string Confidence { get; set; }
        public string Source { get; set; }
        public string SourcePath { get; set; }
        public List<string> ContextTags { get; set; }
        public string Notes { get; set; }
        public List<ushort> ExampleAbilityIds { get; set; }
        public List<TokenEvidenceRecord> Evidence { get; set; }
    }

    public sealed class TokenEvidenceRecord
    {
        public string ExampleToken { get; set; }
        public ushort AbilityId { get; set; }
        public string AbilityName { get; set; }
        public int ComponentSlotIndex { get; set; }
        public ushort ComponentId { get; set; }
        public uint Operation { get; set; }
        public string Source { get; set; }
        public string SourcePath { get; set; }
        public string SourceLocation { get; set; }
        public string Meaning { get; set; }
        public string TextExcerpt { get; set; }
        public List<string> ContextTags { get; set; }
    }

    public sealed class RequirementLedgerDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public List<TableLoadStatus> TableStatuses { get; set; }
        public List<RequirementLedgerRecord> Requirements { get; set; }
    }

    public sealed class RequirementLedgerRecord
    {
        public ushort RequirementId { get; set; }
        public int RecordCount { get; set; }
        public int FieldCount { get; set; }
        public int DirectAbilityCount { get; set; }
        public int DirectComponentCount { get; set; }
        public int ParentRequirementCount { get; set; }
        public int ChildRequirementCount { get; set; }
        public string ContextTagsText { get; set; }
        public string SampleAbilitiesText { get; set; }
        public string SemanticSummary { get; set; }
        public string Notes { get; set; }
        public List<RequirementRowRecord> Rows { get; set; }
        public List<RequirementFieldRecord> Fields { get; set; }
        public List<RequirementLinkEvidenceRecord> InboundReferences { get; set; }
        public List<RequirementLinkEvidenceRecord> OutboundReferences { get; set; }
    }

    public sealed class RequirementRowRecord
    {
        public int RecordIndex { get; set; }
        public string ExtDataText { get; set; }
        public string SourcePath { get; set; }
        public string SourceLocation { get; set; }
    }

    public sealed class RequirementFieldRecord
    {
        public string FieldKey { get; set; }
        public int NonZeroCount { get; set; }
        public int DistinctValueCount { get; set; }
        public string SampleValuesText { get; set; }
        public string SemanticSummary { get; set; }
        public string Confidence { get; set; }
        public string Notes { get; set; }
    }

    public sealed class RequirementLinkEvidenceRecord
    {
        public string SourceKind { get; set; }
        public ushort SourceId { get; set; }
        public string SourceLabel { get; set; }
        public string SourceField { get; set; }
        public int ExtSlotIndex { get; set; }
        public ushort LinkedRequirementId { get; set; }
        public string RelatedAbilitiesText { get; set; }
        public string ContextTagsText { get; set; }
        public string SourcePath { get; set; }
        public string SourceLocation { get; set; }
        public string Notes { get; set; }
    }

    public sealed class RemainingWorkDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public List<TableLoadStatus> TableStatuses { get; set; }
        public RemainingWorkSummaryRecord Summary { get; set; }
        public List<RemainingWorkAreaRecord> Areas { get; set; }
        public List<RemainingWorkItemRecord> Items { get; set; }
    }

    public sealed class RemainingWorkSummaryRecord
    {
        public int AreaCount { get; set; }
        public int ItemCount { get; set; }
        public int CriticalCount { get; set; }
        public int HighCount { get; set; }
        public int CoverageGapAbilityCount { get; set; }
        public int HighSignalConflictCount { get; set; }
        public int UnknownFieldCount { get; set; }
        public int StructuralFieldCount { get; set; }
        public int RequirementGapCount { get; set; }
        public int TokenGapCount { get; set; }
        public int DomainIssueCount { get; set; }
    }

    public sealed class RemainingWorkAreaRecord
    {
        public string AreaKey { get; set; }
        public string Title { get; set; }
        public int ItemCount { get; set; }
        public int CriticalCount { get; set; }
        public int HighCount { get; set; }
        public int PeakScore { get; set; }
        public string PeakBucket { get; set; }
        public string Summary { get; set; }
        public string RecommendedNextStep { get; set; }
    }

    public sealed class RemainingWorkItemRecord
    {
        public string AreaKey { get; set; }
        public string AreaTitle { get; set; }
        public int Rank { get; set; }
        public int GlobalRank { get; set; }
        public string PriorityBucket { get; set; }
        public int PriorityScore { get; set; }
        public string SubjectKind { get; set; }
        public string SubjectKey { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Evidence { get; set; }
        public string RecommendedAction { get; set; }
        public ushort? ExampleAbilityId { get; set; }
        public string ReferencePath { get; set; }
        public string ReferenceLocation { get; set; }
    }

    public sealed class OperationSchemaDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public string OverrideLedgerPath { get; set; }
        public List<TableLoadStatus> TableStatuses { get; set; }
        public List<uint> PriorityOperationIds { get; set; }
        public List<ComponentOperationSchemaRecord> Operations { get; set; }
    }

    public sealed class OperationFieldWorkPacketDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public string FilterDescription { get; set; }
        public List<OperationFieldWorkPacketRecord> Packets { get; set; }
    }

    public sealed class ControlLiteralCrosswalkDocument
    {
        public string GeneratedAtUtc { get; set; }
        public string ExtractedRootPath { get; set; }
        public string FilterDescription { get; set; }
        public List<ControlLiteralRecord> Literals { get; set; }
    }

    public sealed class ControlLiteralRecord
    {
        public int GlobalRank { get; set; }
        public int PriorityScore { get; set; }
        public string RawValue { get; set; }
        public int ObservationCount { get; set; }
        public int DistinctSourceCount { get; set; }
        public int DistinctComponentCount { get; set; }
        public int DistinctAbilityCount { get; set; }
        public int DistinctRequirementCount { get; set; }
        public string SourceKeysText { get; set; }
        public string ContextTagsText { get; set; }
        public string SampleAbilityIdsText { get; set; }
        public string SampleRequirementIdsText { get; set; }
        public string Interpretation { get; set; }
        public string Notes { get; set; }
        public string Summary { get; set; }
        public List<ControlLiteralSourceRecord> Sources { get; set; }
    }

    public sealed class ControlLiteralSourceRecord
    {
        public string SourceKey { get; set; }
        public string SourceLabel { get; set; }
        public int ObservationCount { get; set; }
        public int DistinctComponentCount { get; set; }
        public int DistinctAbilityCount { get; set; }
        public int DistinctRequirementCount { get; set; }
        public string SampleComponentIdsText { get; set; }
        public string SampleAbilityIdsText { get; set; }
        public string SampleRequirementIdsText { get; set; }
        public string TriggerSummaryText { get; set; }
        public string ContextTagSummaryText { get; set; }
        public string CompanionSummaryText { get; set; }
    }

    public sealed class OperationFieldWorkPacketRecord
    {
        public int GlobalRank { get; set; }
        public int AreaRank { get; set; }
        public uint OperationId { get; set; }
        public string OperationName { get; set; }
        public string FieldKey { get; set; }
        public string PriorityBucket { get; set; }
        public int PriorityScore { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Evidence { get; set; }
        public string RecommendedAction { get; set; }
        public ushort? ExampleAbilityId { get; set; }
        public string SampleValuesText { get; set; }
        public List<OperationFieldWorkPacketValueRecord> TopValues { get; set; }
    }

    public sealed class OperationFieldWorkPacketValueRecord
    {
        public string RawValue { get; set; }
        public int ObservationCount { get; set; }
        public int DistinctComponentCount { get; set; }
        public int DistinctAbilityCount { get; set; }
        public string SampleComponentIdsText { get; set; }
        public string SampleAbilityIdsText { get; set; }
        public string TriggerSummaryText { get; set; }
        public string ContextTagSummaryText { get; set; }
        public string CompanionSummaryText { get; set; }
        public List<ComponentOperationAbilityRecord> SampleAbilities { get; set; }
    }

    public sealed class ComponentOperationSchemaRecord
    {
        public uint OperationId { get; set; }
        public string OperationName { get; set; }
        public bool IsPriority { get; set; }
        public int ComponentCount { get; set; }
        public int AbilityCount { get; set; }
        public string LayoutVariantsText { get; set; }
        public List<string> ContextTags { get; set; }
        public List<ushort> SampleComponentIds { get; set; }
        public List<ComponentOperationFieldRecord> Fields { get; set; }
        public List<ComponentOperationAbilityRecord> SampleAbilities { get; set; }
    }

    public sealed class ComponentOperationFieldRecord
    {
        public string FieldKey { get; set; }
        public int NonZeroCount { get; set; }
        public int DistinctValueCount { get; set; }
        public string SampleValuesText { get; set; }
        public string TokenRenderingsText { get; set; }
        public string SemanticSummary { get; set; }
        public string Confidence { get; set; }
        public string ContextTagsText { get; set; }
        public string Notes { get; set; }
        public int TriageScore { get; set; }
        public string TriageBucket { get; set; }
        public string TriageNotes { get; set; }
    }

    public sealed class ComponentOperationAbilityRecord
    {
        public ushort AbilityId { get; set; }
        public string AbilityName { get; set; }
        public ushort ComponentId { get; set; }
        public int ComponentSlotIndex { get; set; }
        public string TriggerText { get; set; }
        public string ContextTagsText { get; set; }
        public string TextExcerpt { get; set; }
        public string SourcePath { get; set; }
        public string SourceLocation { get; set; }
    }

    public sealed class ComponentOperationFieldValueRecord
    {
        public string RawValue { get; set; }
        public int ObservationCount { get; set; }
        public int DistinctComponentCount { get; set; }
        public int DistinctAbilityCount { get; set; }
        public string SampleComponentIdsText { get; set; }
        public string SampleAbilityIdsText { get; set; }
        public List<ComponentOperationAbilityRecord> SampleAbilities { get; set; }
    }

    public sealed class ComponentOperationFieldValueInsightRecord
    {
        public uint OperationId { get; set; }
        public string FieldKey { get; set; }
        public string RawValue { get; set; }
        public int ObservationCount { get; set; }
        public int DistinctComponentCount { get; set; }
        public int DistinctAbilityCount { get; set; }
        public string TriggerSummaryText { get; set; }
        public string ContextTagSummaryText { get; set; }
        public string CompanionSummaryText { get; set; }
        public List<ComponentOperationFieldCorrelationRecord> CompanionFields { get; set; }
    }

    public sealed class ComponentOperationFieldCorrelationRecord
    {
        public string FieldKey { get; set; }
        public string DominantValue { get; set; }
        public int MatchCount { get; set; }
        public int ObservationCount { get; set; }
        public int CoveragePercent { get; set; }
        public int DistinctValueCount { get; set; }
        public string SampleValuesText { get; set; }
    }
}

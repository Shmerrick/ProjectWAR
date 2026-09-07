using ClientDataMatrix.Model;
using ClientDataMatrix.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ClientDataMatrix.Output
{
    public static class ReportWriters
    {
        public static void WriteAbilityReport(string outputRoot, AbilityAnalysisResult report)
        {
            string abilityDirectory = Path.Combine(outputRoot, "ability");
            Directory.CreateDirectory(abilityDirectory);

            string fileStem = Path.Combine(abilityDirectory, report.AbilityId.ToString(CultureInfo.InvariantCulture));
            File.WriteAllText(fileStem + ".md", BuildAbilityMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(AbilityAnalysisResult));
            File.WriteAllText(fileStem + ".dot", BuildDot(report.Graph), Encoding.UTF8);
            WriteNodesCsv(fileStem + ".nodes.csv", report.Graph.Nodes);
            WriteEdgesCsv(fileStem + ".edges.csv", report.Graph.Edges);
            WriteClaimsCsv(fileStem + ".claims.csv", report.Graph.Claims);
        }

        public static void WriteConflictReport(string outputRoot, ConflictReportDocument report)
        {
            string conflictDirectory = Path.Combine(outputRoot, "conflicts");
            Directory.CreateDirectory(conflictDirectory);

            string fileStem = Path.Combine(conflictDirectory, "client-conflicts");
            File.WriteAllText(fileStem + ".md", BuildConflictMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(ConflictReportDocument));
            WriteClaimsCsv(fileStem + ".claims.csv", report.Claims);
        }

        public static void WriteCoverageReport(string outputRoot, CoverageReportDocument report)
        {
            string coverageDirectory = Path.Combine(outputRoot, "coverage");
            Directory.CreateDirectory(coverageDirectory);

            string fileStem = Path.Combine(coverageDirectory, "ability-coverage");
            File.WriteAllText(fileStem + ".md", BuildCoverageMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(CoverageReportDocument));
            WriteCoverageCsv(fileStem + ".csv", report.Abilities ?? new List<CoverageAbilityRecord>());
        }

        public static void WriteDomainLedgerReport(string outputRoot, DomainLedgerDocument report)
        {
            string referenceDirectory = Path.Combine(outputRoot, "reference");
            Directory.CreateDirectory(referenceDirectory);

            string fileStem = Path.Combine(referenceDirectory, "core-identity-domains");
            File.WriteAllText(fileStem + ".md", BuildDomainLedgerMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(DomainLedgerDocument));
            WriteDomainLedgerCsv(fileStem + ".csv", report.Domains ?? new List<IdentityDomainRecord>());
            WriteDomainLedgerValuesCsv(fileStem + ".values.csv", report.Domains ?? new List<IdentityDomainRecord>());
        }

        public static void WriteRequirementLedgerReport(string outputRoot, RequirementLedgerDocument report)
        {
            string referenceDirectory = Path.Combine(outputRoot, "reference");
            Directory.CreateDirectory(referenceDirectory);

            string fileStem = Path.Combine(referenceDirectory, "requirement-ledger");
            File.WriteAllText(fileStem + ".md", BuildRequirementLedgerMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(RequirementLedgerDocument));
            WriteRequirementLedgerCsv(fileStem + ".csv", report.Requirements ?? new List<RequirementLedgerRecord>());
            WriteRequirementLedgerFieldsCsv(fileStem + ".fields.csv", report.Requirements ?? new List<RequirementLedgerRecord>());
            WriteRequirementLedgerReferencesCsv(fileStem + ".references.csv", report.Requirements ?? new List<RequirementLedgerRecord>());
            WriteRequirementLedgerRowsCsv(fileStem + ".rows.csv", report.Requirements ?? new List<RequirementLedgerRecord>());
        }

        public static void WriteTokenDictionaryReport(string outputRoot, TokenDictionaryDocument report)
        {
            string referenceDirectory = Path.Combine(outputRoot, "reference");
            Directory.CreateDirectory(referenceDirectory);

            string fileStem = Path.Combine(referenceDirectory, "com-token-dictionary");
            File.WriteAllText(fileStem + ".md", BuildTokenDictionaryMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(TokenDictionaryDocument));
            WriteTokenDefinitionsCsv(fileStem + ".csv", report.Definitions ?? new List<TokenDefinitionRecord>());
        }

        public static void WriteOperationSchemaReport(string outputRoot, OperationSchemaDocument report)
        {
            string referenceDirectory = Path.Combine(outputRoot, "reference");
            Directory.CreateDirectory(referenceDirectory);

            string fileStem = Path.Combine(referenceDirectory, "component-operation-schemas");
            File.WriteAllText(fileStem + ".md", BuildOperationSchemaMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(OperationSchemaDocument));
            WriteOperationSchemasCsv(fileStem + ".csv", report.Operations ?? new List<ComponentOperationSchemaRecord>());
            WriteOperationFieldsCsv(fileStem + ".fields.csv", report.Operations ?? new List<ComponentOperationSchemaRecord>());
            WriteOperationAbilitiesCsv(fileStem + ".abilities.csv", report.Operations ?? new List<ComponentOperationSchemaRecord>());
        }

        public static void WriteRemainingWorkReport(string outputRoot, RemainingWorkDocument report)
        {
            string overviewDirectory = Path.Combine(outputRoot, "overview");
            Directory.CreateDirectory(overviewDirectory);

            string fileStem = Path.Combine(overviewDirectory, "remaining-work");
            File.WriteAllText(fileStem + ".md", BuildRemainingWorkMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(RemainingWorkDocument));
            WriteRemainingWorkAreasCsv(fileStem + ".areas.csv", report.Areas ?? new List<RemainingWorkAreaRecord>());
            WriteRemainingWorkItemsCsv(fileStem + ".items.csv", report.Items ?? new List<RemainingWorkItemRecord>());

            List<RemainingWorkItemRecord> nextItems = RemainingWorkCatalog.BuildNextBatch(report);
            string nextFileStem = Path.Combine(overviewDirectory, "remaining-work-next");
            File.WriteAllText(
                nextFileStem + ".md",
                BuildRemainingWorkFocusMarkdown(
                    report,
                    nextItems,
                    "Remaining Work Next Batch",
                    RemainingWorkCatalog.BuildFilterDescription(null, RemainingWorkCatalog.DefaultNextBatchMinimumPriorityBucket, null, RemainingWorkCatalog.DefaultNextBatchTopCount)),
                Encoding.UTF8);
            WriteJson(nextFileStem + ".json", nextItems, typeof(List<RemainingWorkItemRecord>));
            WriteRemainingWorkItemsCsv(nextFileStem + ".csv", nextItems);
        }

        public static void WriteRemainingWorkFocusReport(string outputRoot, RemainingWorkDocument report, string fileNameStem, IList<RemainingWorkItemRecord> items, string title, string filterDescription)
        {
            string overviewDirectory = Path.Combine(outputRoot, "overview");
            Directory.CreateDirectory(overviewDirectory);

            string safeStem = string.IsNullOrWhiteSpace(fileNameStem) ? "remaining-work-focus" : fileNameStem;
            string fullStem = Path.Combine(overviewDirectory, safeStem);
            List<RemainingWorkItemRecord> safeItems = items == null ? new List<RemainingWorkItemRecord>() : items.ToList();
            File.WriteAllText(fullStem + ".md", BuildRemainingWorkFocusMarkdown(report, safeItems, title, filterDescription), Encoding.UTF8);
            WriteJson(fullStem + ".json", safeItems, typeof(List<RemainingWorkItemRecord>));
            WriteRemainingWorkItemsCsv(fullStem + ".csv", safeItems);
        }

        public static void WriteOperationFieldWorkPacketReport(string outputRoot, OperationFieldWorkPacketDocument report)
        {
            string overviewDirectory = Path.Combine(outputRoot, "overview");
            Directory.CreateDirectory(overviewDirectory);

            string fileStem = Path.Combine(overviewDirectory, "remaining-work-operation-fields");
            File.WriteAllText(fileStem + ".md", BuildOperationFieldWorkPacketMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(OperationFieldWorkPacketDocument));
            WriteOperationFieldWorkPacketCsv(fileStem + ".csv", report == null ? new List<OperationFieldWorkPacketRecord>() : report.Packets ?? new List<OperationFieldWorkPacketRecord>());
            WriteOperationFieldWorkPacketValuesCsv(fileStem + ".values.csv", report == null ? new List<OperationFieldWorkPacketRecord>() : report.Packets ?? new List<OperationFieldWorkPacketRecord>());
            WriteOperationFieldWorkPacketAbilitiesCsv(fileStem + ".abilities.csv", report == null ? new List<OperationFieldWorkPacketRecord>() : report.Packets ?? new List<OperationFieldWorkPacketRecord>());
        }

        public static void WriteControlLiteralCrosswalkReport(string outputRoot, ControlLiteralCrosswalkDocument report)
        {
            string overviewDirectory = Path.Combine(outputRoot, "overview");
            Directory.CreateDirectory(overviewDirectory);

            string fileStem = Path.Combine(overviewDirectory, "remaining-work-control-literals");
            File.WriteAllText(fileStem + ".md", BuildControlLiteralCrosswalkMarkdown(report), Encoding.UTF8);
            WriteJson(fileStem + ".json", report, typeof(ControlLiteralCrosswalkDocument));
            WriteControlLiteralCrosswalkCsv(fileStem + ".csv", report == null ? new List<ControlLiteralRecord>() : report.Literals ?? new List<ControlLiteralRecord>());
            WriteControlLiteralCrosswalkSourcesCsv(fileStem + ".sources.csv", report == null ? new List<ControlLiteralRecord>() : report.Literals ?? new List<ControlLiteralRecord>());
        }

        private static void WriteJson(string path, object instance, Type instanceType)
        {
            DataContractJsonSerializerSettings settings = new DataContractJsonSerializerSettings
            {
                UseSimpleDictionaryFormat = true
            };

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(instanceType, settings);
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                serializer.WriteObject(stream, instance);
        }

        private static string BuildAbilityMarkdown(AbilityAnalysisResult report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Ability " + report.AbilityId + " Data Matrix");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + report.GeneratedAtUtc + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + report.ExtractedRootPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("- abilities.csv rows: " + report.ClientAbilityRows.Count);
            builder.AppendLine("- abilityexport.bin rows: " + report.BinaryAbilityRows.Count);
            builder.AppendLine("- effects.csv rows: " + report.ClientEffectRows.Count);
            builder.AppendLine("- abilitycomponentexport.bin rows: " + report.BinaryComponentRows.Count);
            builder.AppendLine("- abilitynames.txt rows: " + report.AbilityNameRows.Count);
            builder.AppendLine("- abilitydesc.txt rows: " + report.AbilityDescriptionRows.Count);
            builder.AppendLine("- abilityeffect.txt rows: " + report.AbilityEffectTextRows.Count);
            builder.AppendLine("- componenteffects.txt rows: " + report.ComponentEffectRows.Count);
            builder.AppendLine("- inferred requirement links: " + (report.RequirementReferences == null ? 0 : report.RequirementReferences.Count));
            builder.AppendLine("- linked requirement rows: " + report.BinaryRequirementRows.Count);
            builder.AppendLine("- pregame_chars.xml references: " + report.PregameRows.Count);
            builder.AppendLine("- Related effect IDs: " + JoinValues(report.RelatedEffectIds.Select(value => value.ToString(CultureInfo.InvariantCulture))));
            builder.AppendLine("- Related component IDs: " + JoinValues(report.RelatedComponentIds.Select(value => value.ToString(CultureInfo.InvariantCulture))));
            builder.AppendLine("- Conflict count: " + report.Graph.GetConflicts().Count);
            builder.AppendLine();

            if (report.Warnings.Count > 0)
            {
                builder.AppendLine("## Warnings");
                builder.AppendLine();
                foreach (string warning in report.Warnings)
                    builder.AppendLine("- " + warning);
                builder.AppendLine();
            }

            AppendTable(builder, "Source Status", report.TableStatuses,
                new[] { "Source", "File", "Loaded", "Rows", "Path", "Error" },
                row => new[] { row.SourceFamily, row.TableName, row.Loaded.ToString(), row.RowCount.ToString(CultureInfo.InvariantCulture), row.SourcePath, NullToEmpty(row.ErrorMessage) });

            AppendTable(builder, "abilities.csv Rows", report.ClientAbilityRows,
                new[] { "AbilityId", "Name", "Description", "EffectId", "AnimationId", "Path", "Line", "ByteOffset" },
                row => new[] { row.AbilityId.ToString(CultureInfo.InvariantCulture), row.Name, row.Description, row.EffectId.ToString(CultureInfo.InvariantCulture), row.AnimationId.ToString(CultureInfo.InvariantCulture), row.SourcePath, row.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(row.ByteOffset) });

            AppendTable(builder, "abilityexport.bin Rows", report.BinaryAbilityRows,
                new[] { "AbilityId", "EffectId", "CareerLine", "CastTime", "Cooldown", "Range", "ApCost", "Faction", "Components", "ExtData", "Path", "ByteOffset" },
                row => new[]
                {
                    row.AbilityId.ToString(CultureInfo.InvariantCulture),
                    row.EffectId.ToString(CultureInfo.InvariantCulture),
                    row.CareerLine.ToString(CultureInfo.InvariantCulture),
                    row.CastTime.ToString(CultureInfo.InvariantCulture),
                    row.Cooldown.ToString(CultureInfo.InvariantCulture),
                    row.Range.ToString(CultureInfo.InvariantCulture),
                    row.ApCost.ToString(CultureInfo.InvariantCulture),
                    row.Faction.ToString(CultureInfo.InvariantCulture),
                    JoinValues(row.ComponentIds.Where(value => value > 0).Select(value => value.ToString(CultureInfo.InvariantCulture))),
                    FormatExtData(row.ExtData),
                    row.SourcePath,
                    FormatByteOffset(row.ByteOffset)
                });

            AppendTable(builder, "effects.csv Rows", report.ClientEffectRows,
                new[] { "EffectId", "Name", "BuildUp", "Activate", "Cast", "Projectile", "Impact", "AOE", "Channel", "Path", "Line", "ByteOffset" },
                row => new[] { row.EffectId.ToString(CultureInfo.InvariantCulture), row.Name, row.BuildUpId.ToString(CultureInfo.InvariantCulture), row.ActivateId.ToString(CultureInfo.InvariantCulture), row.CastId.ToString(CultureInfo.InvariantCulture), row.ProjectileId.ToString(CultureInfo.InvariantCulture), row.ImpactId.ToString(CultureInfo.InvariantCulture), row.AoeId.ToString(CultureInfo.InvariantCulture), row.ChannelingId.ToString(CultureInfo.InvariantCulture), row.SourcePath, row.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(row.ByteOffset) });

            AppendTable(builder, "abilitynames.txt Rows", report.AbilityNameRows,
                new[] { "EntryId", "Raw", "Normalized", "Path", "Line", "ByteOffset" },
                row => new[] { row.EntryId.ToString(CultureInfo.InvariantCulture), row.Value, row.NormalizedValue, row.SourcePath, row.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(row.ByteOffset) });

            AppendTable(builder, "abilitydesc.txt Rows", report.AbilityDescriptionRows,
                new[] { "EntryId", "Raw", "Normalized", "Path", "Line", "ByteOffset" },
                row => new[] { row.EntryId.ToString(CultureInfo.InvariantCulture), row.Value, row.NormalizedValue, row.SourcePath, row.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(row.ByteOffset) });

            AppendTable(builder, "abilityeffect.txt Rows", report.AbilityEffectTextRows,
                new[] { "EntryId", "Raw", "Normalized", "Path", "Line", "ByteOffset" },
                row => new[] { row.EntryId.ToString(CultureInfo.InvariantCulture), row.Value, row.NormalizedValue, row.SourcePath, row.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(row.ByteOffset) });

            AppendTable(builder, "abilitycomponentexport.bin Rows", report.BinaryComponentRows,
                new[] { "ComponentId", "Operation", "Duration", "Interval", "Radius", "MaxTargets", "Description", "Values", "ExtData", "Layout", "Path", "ByteOffset" },
                row => new[]
                {
                    row.ComponentId.ToString(CultureInfo.InvariantCulture),
                    row.Operation.ToString(CultureInfo.InvariantCulture),
                    row.Duration.ToString(CultureInfo.InvariantCulture),
                    row.Interval.ToString(CultureInfo.InvariantCulture),
                    row.Radius.ToString(CultureInfo.InvariantCulture),
                    row.MaxTargets.ToString(CultureInfo.InvariantCulture),
                    NullToEmpty(row.Description),
                    JoinValues(row.Values.Select(value => value.ToString(CultureInfo.InvariantCulture))),
                    FormatExtData(row.ExtData),
                    row.LayoutVariant,
                    row.SourcePath,
                    FormatByteOffset(row.ByteOffset)
                });

            AppendTable(builder, "Ability Chain (behaviour reached through components)",
                report.AbilityChain ?? new List<AbilityChainEntry>(),
                new[] { "Depth", "AbilityId", "Name", "From", "ViaComponent", "Operation", "Field", "Components", "Note" },
                row => new[]
                {
                    row.Depth.ToString(CultureInfo.InvariantCulture),
                    row.AbilityId.ToString(CultureInfo.InvariantCulture),
                    NullToEmpty(row.Name),
                    NullToEmpty(row.ArrivedFrom),
                    NullToEmpty(row.Via),
                    NullToEmpty(row.Operation),
                    NullToEmpty(row.Field),
                    NullToEmpty(row.ComponentIds),
                    row.Revisited ? "already listed; not expanded again" : NullToEmpty(row.Basis)
                });

            AppendTable(builder, "Inferred Requirement Links", report.RequirementReferences ?? new List<RequirementReferenceRecord>(),
                new[] { "SourceKind", "SourceId", "SourceLabel", "Field", "RequirementId", "Confidence", "Path", "Location", "Notes" },
                row => new[]
                {
                    row.SourceKind,
                    row.SourceId.ToString(CultureInfo.InvariantCulture),
                    row.SourceLabel,
                    row.SourceField,
                    row.RequirementId.ToString(CultureInfo.InvariantCulture),
                    row.Confidence,
                    row.SourcePath,
                    row.SourceLocation,
                    row.Notes
                });

            AppendTable(builder, "abilityrequirementexport.bin Rows", report.BinaryRequirementRows,
                new[] { "RequirementId", "ExtData", "Path", "ByteOffset" },
                row => new[]
                {
                    row.RequirementId.ToString(CultureInfo.InvariantCulture),
                    FormatExtData(row.ExtData),
                    row.SourcePath,
                    FormatByteOffset(row.ByteOffset)
                });

            AppendTable(builder, "componenteffects.txt Rows", report.ComponentEffectRows,
                new[] { "EntryId", "Raw", "Normalized", "Path", "Line", "ByteOffset" },
                row => new[] { row.EntryId.ToString(CultureInfo.InvariantCulture), row.Value, row.NormalizedValue, row.SourcePath, row.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(row.ByteOffset) });

            AppendTable(builder, "pregame_chars.xml References", report.PregameRows,
                new[] { "Sequence", "Scene", "Career", "Race", "OnLoad", "MouseOver", "OnClick", "Path", "Line", "ByteOffset" },
                row => new[] { row.Sequence.ToString(CultureInfo.InvariantCulture), row.SceneType, row.CareerName, row.RaceName, NullableToText(row.OnLoadAbilityId), NullableToText(row.MouseOverAbilityId), NullableToText(row.OnClickAbilityId), row.SourcePath, row.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(row.ByteOffset) });

            AppendConflictSection(builder, report.Graph.GetConflicts());
            AppendClaimSection(builder, report.Graph.Claims);

            return builder.ToString();
        }

        private static string BuildConflictMarkdown(ConflictReportDocument report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Client Data Conflict Ledger");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + report.GeneratedAtUtc + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + report.ExtractedRootPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("- Total claims: " + report.Claims.Count);
            builder.AppendLine("- Total conflicts: " + report.Conflicts.Count);
            builder.AppendLine();

            AppendTable(builder, "Source Status", report.TableStatuses,
                new[] { "Source", "File", "Loaded", "Rows", "Path", "Error" },
                row => new[] { row.SourceFamily, row.TableName, row.Loaded.ToString(), row.RowCount.ToString(CultureInfo.InvariantCulture), row.SourcePath, NullToEmpty(row.ErrorMessage) });

            AppendConflictSection(builder, report.Conflicts);
            AppendClaimSection(builder, report.Claims);

            return builder.ToString();
        }

        private static string BuildTokenDictionaryMarkdown(TokenDictionaryDocument report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# COM Token Dictionary");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + report.GeneratedAtUtc + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + report.ExtractedRootPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Grammar");
            builder.AppendLine();
            builder.AppendLine("- Pattern: `" + report.TokenGrammar + "`");
            builder.AppendLine("- Meaning: " + report.TokenGrammarMeaning);
            builder.AppendLine("- Override ledger: `" + report.OverrideLedgerPath + "`");
            builder.AppendLine();

            AppendTable(builder, "Token Summary", report.Definitions ?? new List<TokenDefinitionRecord>(),
                new[] { "TokenBody", "ExampleToken", "Field", "Meaning", "Confidence", "ContextTags", "ExampleAbilities", "Source" },
                row => new[]
                {
                    row.TokenBody,
                    row.ExampleToken,
                    row.FieldKey,
                    row.PlainEnglishMeaning,
                    row.Confidence,
                    JoinValues(row.ContextTags ?? new List<string>()),
                    JoinValues(row.ExampleAbilityIds.Select(value => value.ToString(CultureInfo.InvariantCulture))),
                    row.Source
                });

            foreach (TokenDefinitionRecord definition in report.Definitions ?? new List<TokenDefinitionRecord>())
            {
                builder.AppendLine("## " + definition.TokenBody);
                builder.AppendLine();
                builder.AppendLine("- Example token: `" + definition.ExampleToken + "`");
                builder.AppendLine("- Field: `" + definition.FieldKey + "`");
                builder.AppendLine("- Plain-English meaning: " + definition.PlainEnglishMeaning);
                builder.AppendLine("- Confidence: `" + definition.Confidence + "`");
                builder.AppendLine("- Context tags: `" + JoinValues(definition.ContextTags ?? new List<string>()) + "`");
                builder.AppendLine("- Source family: `" + definition.Source + "`");
                builder.AppendLine("- Source path: `" + definition.SourcePath + "`");
                if (!string.IsNullOrWhiteSpace(definition.Notes))
                    builder.AppendLine("- Notes: " + definition.Notes);
                builder.AppendLine();

                AppendTable(builder, "Evidence", definition.Evidence ?? new List<TokenEvidenceRecord>(),
                    new[] { "ExampleToken", "AbilityId", "AbilityName", "Slot", "ComponentId", "Operation", "ContextTags", "Source", "Path", "Location", "Meaning", "TextExcerpt" },
                    row => new[]
                    {
                        row.ExampleToken,
                        row.AbilityId.ToString(CultureInfo.InvariantCulture),
                        row.AbilityName,
                        row.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture),
                        row.ComponentId.ToString(CultureInfo.InvariantCulture),
                        row.Operation.ToString(CultureInfo.InvariantCulture),
                        JoinValues(row.ContextTags ?? new List<string>()),
                        row.Source,
                        row.SourcePath,
                        row.SourceLocation,
                        row.Meaning,
                        row.TextExcerpt
                    });
            }

            return builder.ToString();
        }

        private static string BuildCoverageMarkdown(CoverageReportDocument report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Ability Coverage");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + report.GeneratedAtUtc + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + report.ExtractedRootPath + "`");
            builder.AppendLine();

            List<CoverageAbilityRecord> abilities = report.Abilities ?? new List<CoverageAbilityRecord>();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("- Ability ids covered: " + abilities.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- `MappedWithRequirements`: " + abilities.Count(row => string.Equals(row.CoverageStatus, "MappedWithRequirements", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- `Mapped`: " + abilities.Count(row => string.Equals(row.CoverageStatus, "Mapped", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- `PlayableSurface`: " + abilities.Count(row => string.Equals(row.CoverageStatus, "PlayableSurface", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- `Partial`: " + abilities.Count(row => string.Equals(row.CoverageStatus, "Partial", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- `StringsOnly`: " + abilities.Count(row => string.Equals(row.CoverageStatus, "StringsOnly", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- `BlankSlot`: " + abilities.Count(row => string.Equals(row.CoverageStatus, "BlankSlot", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture) + " (irrecoverable empty string-table slots; excluded from gap count)");
            builder.AppendLine("- `Sparse`: " + abilities.Count(row => string.Equals(row.CoverageStatus, "Sparse", StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine();

            AppendTable(builder, "Source Status", report.TableStatuses ?? new List<TableLoadStatus>(),
                new[] { "Source", "File", "Loaded", "Rows", "Path", "Error" },
                row => new[] { row.SourceFamily, row.TableName, row.Loaded.ToString(), row.RowCount.ToString(CultureInfo.InvariantCulture), row.SourcePath, NullToEmpty(row.ErrorMessage) });

            AppendTable(builder, "Ability Coverage", abilities,
                new[] { "AbilityId", "Name", "Status", "EffectId", "Csv", "Bin", "NameText", "DescText", "EffectText", "EffectRow", "Effects", "Components", "ReqLinks", "ReqRows", "Pregame", "Sources", "Missing" },
                row => new[]
                {
                    row.AbilityId.ToString(CultureInfo.InvariantCulture),
                    row.Name,
                    row.CoverageStatus,
                    row.EffectId.HasValue ? row.EffectId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    row.HasClientCsv ? "Yes" : "No",
                    row.HasClientBin ? "Yes" : "No",
                    row.HasLocalizedName ? "Yes" : "No",
                    row.HasDescriptionText ? "Yes" : "No",
                    row.HasEffectText ? "Yes" : "No",
                    row.HasRootEffectRow ? "Yes" : "No",
                    row.RelatedEffectCount.ToString(CultureInfo.InvariantCulture),
                    row.ComponentCount.ToString(CultureInfo.InvariantCulture),
                    row.RequirementLinkCount.ToString(CultureInfo.InvariantCulture),
                    row.RequirementRowCount.ToString(CultureInfo.InvariantCulture),
                    row.HasPregameReference ? "Yes" : "No",
                    row.SourcesText,
                    row.MissingText
                });

            return builder.ToString();
        }

        private static string BuildRemainingWorkMarkdown(RemainingWorkDocument report)
        {
            RemainingWorkSummaryRecord summary = report == null ? null : report.Summary;
            List<RemainingWorkAreaRecord> areas = report == null || report.Areas == null ? new List<RemainingWorkAreaRecord>() : report.Areas;
            List<RemainingWorkItemRecord> items = report == null || report.Items == null ? new List<RemainingWorkItemRecord>() : report.Items;
            List<RemainingWorkItemRecord> nextItems = RemainingWorkCatalog.BuildNextBatch(report).Take(25).ToList();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Remaining Work");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + (report == null ? string.Empty : report.GeneratedAtUtc) + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + (report == null ? string.Empty : report.ExtractedRootPath) + "`");
            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("- Areas: " + (summary == null ? 0 : summary.AreaCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Items: " + (summary == null ? 0 : summary.ItemCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Critical: " + (summary == null ? 0 : summary.CriticalCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- High: " + (summary == null ? 0 : summary.HighCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Coverage gaps: " + (summary == null ? 0 : summary.CoverageGapAbilityCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- High-signal conflicts: " + (summary == null ? 0 : summary.HighSignalConflictCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Unknown fields: " + (summary == null ? 0 : summary.UnknownFieldCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Structural fields: " + (summary == null ? 0 : summary.StructuralFieldCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Requirement rows with unresolved fields: " + (summary == null ? 0 : summary.RequirementGapCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Token gaps: " + (summary == null ? 0 : summary.TokenGapCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Identity-domain risks: " + (summary == null ? 0 : summary.DomainIssueCount).ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("- Default next batch file: `overview/remaining-work-next.md`");
            builder.AppendLine("- Default operation field packet file: `overview/remaining-work-operation-fields.md`");
            builder.AppendLine("- Default control literal file: `overview/remaining-work-control-literals.md`");
            builder.AppendLine();

            AppendTable(builder, "Next Batch Preview", nextItems,
                new[] { "Global", "Area", "Priority", "Score", "Title", "Subject", "Action", "ExampleAbilityId" },
                row => new[]
                {
                    row.GlobalRank.ToString(CultureInfo.InvariantCulture),
                    row.AreaTitle,
                    row.PriorityBucket,
                    row.PriorityScore.ToString(CultureInfo.InvariantCulture),
                    row.Title,
                    row.SubjectKey,
                    row.RecommendedAction,
                    row.ExampleAbilityId.HasValue ? row.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty
                });

            AppendTable(builder, "Area Summary", areas,
                new[] { "Area", "Items", "Critical", "High", "Peak", "Bucket", "Summary", "Next Step" },
                row => new[]
                {
                    row.Title,
                    row.ItemCount.ToString(CultureInfo.InvariantCulture),
                    row.CriticalCount.ToString(CultureInfo.InvariantCulture),
                    row.HighCount.ToString(CultureInfo.InvariantCulture),
                    row.PeakScore.ToString(CultureInfo.InvariantCulture),
                    row.PeakBucket,
                    row.Summary,
                    row.RecommendedNextStep
                });

            foreach (RemainingWorkAreaRecord area in areas)
            {
                List<RemainingWorkItemRecord> areaItems = items
                    .Where(row => string.Equals(row.AreaKey, area.AreaKey, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(row => row.Rank)
                    .ThenByDescending(row => row.PriorityScore)
                    .Take(20)
                    .ToList();

                AppendTable(builder, area.Title + " Top Items", areaItems,
                    new[] { "Rank", "Global", "Priority", "Score", "Title", "Subject", "Summary", "Evidence", "Action", "ExampleAbilityId" },
                    row => new[]
                    {
                        row.Rank.ToString(CultureInfo.InvariantCulture),
                        row.GlobalRank.ToString(CultureInfo.InvariantCulture),
                        row.PriorityBucket,
                        row.PriorityScore.ToString(CultureInfo.InvariantCulture),
                        row.Title,
                        row.SubjectKey,
                        row.Summary,
                        row.Evidence,
                        row.RecommendedAction,
                        row.ExampleAbilityId.HasValue ? row.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty
                    });
            }

            return builder.ToString();
        }

        private static string BuildRemainingWorkFocusMarkdown(RemainingWorkDocument report, IList<RemainingWorkItemRecord> items, string title, string filterDescription)
        {
            List<RemainingWorkItemRecord> safeItems = items == null ? new List<RemainingWorkItemRecord>() : items.ToList();
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# " + (string.IsNullOrWhiteSpace(title) ? "Remaining Work Focus" : title));
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + (report == null ? string.Empty : report.GeneratedAtUtc) + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + (report == null ? string.Empty : report.ExtractedRootPath) + "`");
            builder.AppendLine();
            builder.AppendLine("Filter: " + (string.IsNullOrWhiteSpace(filterDescription) ? "(none)" : filterDescription));
            builder.AppendLine();
            builder.AppendLine("Matching items: " + safeItems.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine();

            AppendTable(builder, "Focus Items", safeItems,
                new[] { "Global", "Area", "AreaRank", "Priority", "Score", "Title", "Subject", "Summary", "Action", "ExampleAbilityId" },
                row => new[]
                {
                    row.GlobalRank.ToString(CultureInfo.InvariantCulture),
                    row.AreaTitle,
                    row.Rank.ToString(CultureInfo.InvariantCulture),
                    row.PriorityBucket,
                    row.PriorityScore.ToString(CultureInfo.InvariantCulture),
                    row.Title,
                    row.SubjectKey,
                    row.Summary,
                    row.RecommendedAction,
                    row.ExampleAbilityId.HasValue ? row.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty
                });

            return builder.ToString();
        }

        private static string BuildOperationFieldWorkPacketMarkdown(OperationFieldWorkPacketDocument report)
        {
            List<OperationFieldWorkPacketRecord> packets = report == null || report.Packets == null
                ? new List<OperationFieldWorkPacketRecord>()
                : report.Packets;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Remaining Work Operation Field Packets");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + (report == null ? string.Empty : report.GeneratedAtUtc) + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + (report == null ? string.Empty : report.ExtractedRootPath) + "`");
            builder.AppendLine();
            builder.AppendLine("Filter: " + (report == null ? string.Empty : report.FilterDescription));
            builder.AppendLine();
            builder.AppendLine("Packets: " + packets.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine();

            AppendTable(builder, "Packet Summary", packets,
                new[] { "Global", "AreaRank", "Operation", "Field", "Priority", "Score", "Title", "ExampleAbilityId", "SampleValues" },
                row => new[]
                {
                    row.GlobalRank.ToString(CultureInfo.InvariantCulture),
                    row.AreaRank.ToString(CultureInfo.InvariantCulture),
                    row.OperationName + " (" + row.OperationId.ToString(CultureInfo.InvariantCulture) + ")",
                    row.FieldKey,
                    row.PriorityBucket,
                    row.PriorityScore.ToString(CultureInfo.InvariantCulture),
                    row.Title,
                    row.ExampleAbilityId.HasValue ? row.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    row.SampleValuesText
                });

            foreach (OperationFieldWorkPacketRecord packet in packets)
            {
                builder.AppendLine("## " + packet.Title);
                builder.AppendLine();
                builder.AppendLine("- Operation: `" + packet.OperationName + "` (`" + packet.OperationId.ToString(CultureInfo.InvariantCulture) + "`)");
                builder.AppendLine("- Field: `" + packet.FieldKey + "`");
                builder.AppendLine("- Ranks: global `" + packet.GlobalRank.ToString(CultureInfo.InvariantCulture) + "`, area `" + packet.AreaRank.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Priority: `" + packet.PriorityBucket + "` `" + packet.PriorityScore.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Example ability: `" + (packet.ExampleAbilityId.HasValue ? packet.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty) + "`");
                builder.AppendLine();
                builder.AppendLine("Summary: " + packet.Summary);
                builder.AppendLine();
                builder.AppendLine("Evidence: " + packet.Evidence);
                builder.AppendLine();
                builder.AppendLine("Recommended action: " + packet.RecommendedAction);
                builder.AppendLine();

                AppendTable(builder, "Top Values", packet.TopValues ?? new List<OperationFieldWorkPacketValueRecord>(),
                    new[] { "RawValue", "Obs", "Components", "Abilities", "SampleAbilityIds", "Triggers", "Context", "Companions" },
                    row => new[]
                    {
                        row.RawValue,
                        row.ObservationCount.ToString(CultureInfo.InvariantCulture),
                        row.DistinctComponentCount.ToString(CultureInfo.InvariantCulture),
                        row.DistinctAbilityCount.ToString(CultureInfo.InvariantCulture),
                        row.SampleAbilityIdsText,
                        row.TriggerSummaryText,
                        row.ContextTagSummaryText,
                        row.CompanionSummaryText
                    });

                List<OperationFieldWorkPacketAbilityRow> abilityRows = (packet.TopValues ?? new List<OperationFieldWorkPacketValueRecord>())
                    .SelectMany(value => (value.SampleAbilities ?? new List<ComponentOperationAbilityRecord>())
                        .Select(ability => new OperationFieldWorkPacketAbilityRow
                        {
                            RawValue = value.RawValue,
                            AbilityId = ability.AbilityId,
                            AbilityName = ability.AbilityName,
                            ComponentId = ability.ComponentId,
                            ComponentSlotIndex = ability.ComponentSlotIndex,
                            TriggerText = ability.TriggerText,
                            ContextTagsText = ability.ContextTagsText,
                            TextExcerpt = ability.TextExcerpt,
                            SourcePath = ability.SourcePath,
                            SourceLocation = ability.SourceLocation
                        }))
                    .Take(18)
                    .ToList();

                AppendTable(builder, "Sample Abilities", abilityRows,
                    new[] { "RawValue", "AbilityId", "AbilityName", "ComponentId", "Slot", "Trigger", "Context", "TextExcerpt" },
                    row => new[]
                    {
                        row.RawValue,
                        row.AbilityId.ToString(CultureInfo.InvariantCulture),
                        row.AbilityName,
                        row.ComponentId.ToString(CultureInfo.InvariantCulture),
                        row.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture),
                        row.TriggerText,
                        row.ContextTagsText,
                        row.TextExcerpt
                    });
            }

            return builder.ToString();
        }

        private static string BuildControlLiteralCrosswalkMarkdown(ControlLiteralCrosswalkDocument report)
        {
            List<ControlLiteralRecord> literals = report == null || report.Literals == null
                ? new List<ControlLiteralRecord>()
                : report.Literals;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Remaining Work Control Literal Crosswalk");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + (report == null ? string.Empty : report.GeneratedAtUtc) + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + (report == null ? string.Empty : report.ExtractedRootPath) + "`");
            builder.AppendLine();
            builder.AppendLine("Filter: " + (report == null ? string.Empty : report.FilterDescription));
            builder.AppendLine();
            builder.AppendLine("Literals: " + literals.Count.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine();

            AppendTable(builder, "Literal Summary", literals,
                new[] { "Rank", "Score", "RawValue", "Obs", "Sources", "Abilities", "Requirements", "Interpretation", "Contexts", "SourceKeys" },
                row => new[]
                {
                    row.GlobalRank.ToString(CultureInfo.InvariantCulture),
                    row.PriorityScore.ToString(CultureInfo.InvariantCulture),
                    row.RawValue,
                    row.ObservationCount.ToString(CultureInfo.InvariantCulture),
                    row.DistinctSourceCount.ToString(CultureInfo.InvariantCulture),
                    row.DistinctAbilityCount.ToString(CultureInfo.InvariantCulture),
                    row.DistinctRequirementCount.ToString(CultureInfo.InvariantCulture),
                    row.Interpretation,
                    row.ContextTagsText,
                    row.SourceKeysText
                });

            foreach (ControlLiteralRecord literal in literals)
            {
                builder.AppendLine("## Literal `" + literal.RawValue + "`");
                builder.AppendLine();
                builder.AppendLine("- Rank: `" + literal.GlobalRank.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Score: `" + literal.PriorityScore.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Observations: `" + literal.ObservationCount.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Distinct sources: `" + literal.DistinctSourceCount.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Distinct components: `" + literal.DistinctComponentCount.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Distinct abilities: `" + literal.DistinctAbilityCount.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Distinct requirements: `" + literal.DistinctRequirementCount.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Sample ability ids: `" + literal.SampleAbilityIdsText + "`");
                builder.AppendLine("- Sample requirement ids: `" + literal.SampleRequirementIdsText + "`");
                if (!string.IsNullOrWhiteSpace(literal.Interpretation))
                    builder.AppendLine("- Interpretation: " + literal.Interpretation);
                if (!string.IsNullOrWhiteSpace(literal.Notes))
                    builder.AppendLine("- Notes: " + literal.Notes);
                builder.AppendLine("- Contexts: " + literal.ContextTagsText);
                builder.AppendLine();
                builder.AppendLine("Summary: " + literal.Summary);
                builder.AppendLine();

                AppendTable(builder, "Source Breakdown", literal.Sources ?? new List<ControlLiteralSourceRecord>(),
                    new[] { "Source", "Obs", "Components", "Abilities", "Requirements", "AbilityIds", "RequirementIds", "Triggers", "Contexts", "Companions" },
                    row => new[]
                    {
                        row.SourceLabel,
                        row.ObservationCount.ToString(CultureInfo.InvariantCulture),
                        row.DistinctComponentCount.ToString(CultureInfo.InvariantCulture),
                        row.DistinctAbilityCount.ToString(CultureInfo.InvariantCulture),
                        row.DistinctRequirementCount.ToString(CultureInfo.InvariantCulture),
                        row.SampleAbilityIdsText,
                        row.SampleRequirementIdsText,
                        row.TriggerSummaryText,
                        row.ContextTagSummaryText,
                        row.CompanionSummaryText
                    });
            }

            return builder.ToString();
        }

        private static string BuildDomainLedgerMarkdown(DomainLedgerDocument report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Core Identity Domains");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + report.GeneratedAtUtc + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + report.ExtractedRootPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Purpose");
            builder.AppendLine();
            builder.AppendLine("- This ledger separates extracted-client identity domains so names like `CareerLine`, `CareerName`, and future `CareerId` work do not collapse into one ambiguous label.");
            builder.AppendLine("- It does not invent a canonical `CareerId` domain when the extracted files do not prove one.");
            builder.AppendLine();

            AppendTable(builder, "Source Status", report.TableStatuses ?? new List<TableLoadStatus>(),
                new[] { "Source", "File", "Loaded", "Rows", "Path", "Error" },
                row => new[] { row.SourceFamily, row.TableName, row.Loaded.ToString(), row.RowCount.ToString(CultureInfo.InvariantCulture), row.SourcePath, NullToEmpty(row.ErrorMessage) });

            AppendTable(builder, "Domain Summary", report.Domains ?? new List<IdentityDomainRecord>(),
                new[] { "DomainKey", "DisplayName", "Confidence", "Canonicality", "Values", "DistinctMeanings", "DuplicateMeaningGroups", "Sources", "RecommendedUsage" },
                row => new[]
                {
                    row.DomainKey,
                    row.DisplayName,
                    row.Confidence,
                    row.Canonicality,
                    row.ValueCount.ToString(CultureInfo.InvariantCulture),
                    row.DistinctMeaningCount.ToString(CultureInfo.InvariantCulture),
                    row.DuplicateMeaningCount.ToString(CultureInfo.InvariantCulture),
                    row.SourceFilesText,
                    row.RecommendedUsage
                });

            foreach (IdentityDomainRecord domain in report.Domains ?? new List<IdentityDomainRecord>())
            {
                builder.AppendLine("## " + domain.DisplayName);
                builder.AppendLine();
                builder.AppendLine("- Domain key: `" + domain.DomainKey + "`");
                builder.AppendLine("- Confidence: `" + domain.Confidence + "`");
                builder.AppendLine("- Canonicality: `" + domain.Canonicality + "`");
                builder.AppendLine("- Source files: `" + domain.SourceFilesText + "`");
                builder.AppendLine("- Recommended usage: " + domain.RecommendedUsage);
                builder.AppendLine("- Notes: " + domain.Notes);
                builder.AppendLine();

                AppendTable(builder, "Values", domain.Values ?? new List<IdentityDomainValueRecord>(),
                    new[] { "RawValue", "Meaning", "Confidence", "Source", "Path", "Location", "Notes" },
                    row => new[]
                    {
                        row.RawValue,
                        row.Meaning,
                        row.Confidence,
                        row.Source,
                        row.SourcePath,
                        row.SourceLocation,
                        row.Notes
                    });
            }

            return builder.ToString();
        }

        private static string BuildRequirementLedgerMarkdown(RequirementLedgerDocument report)
        {
            List<RequirementLedgerRecord> requirements = report == null || report.Requirements == null ? new List<RequirementLedgerRecord>() : report.Requirements;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Requirement Ledger");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + report.GeneratedAtUtc + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + report.ExtractedRootPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- This ledger keeps the requirement-linking rule narrow: only exact `ExtData[*].Val6` matches against known `RequirementId` rows are treated as requirement references.");
            builder.AppendLine("- It makes requirement rows inspectable by showing inbound references, direct child requirement links, active ext-data fields, and the ability-context tags attached to direct usage.");
            builder.AppendLine();

            AppendTable(builder, "Source Status", report.TableStatuses ?? new List<TableLoadStatus>(),
                new[] { "Source", "File", "Loaded", "Rows", "Path", "Error" },
                row => new[] { row.SourceFamily, row.TableName, row.Loaded.ToString(), row.RowCount.ToString(CultureInfo.InvariantCulture), row.SourcePath, NullToEmpty(row.ErrorMessage) });

            AppendTable(builder, "Requirement Summary", requirements,
                new[] { "RequirementId", "Rows", "Fields", "DirectAbilities", "DirectComponents", "ParentRequirements", "ChildRequirements", "ContextTags", "SampleAbilities", "Summary" },
                row => new[]
                {
                    row.RequirementId.ToString(CultureInfo.InvariantCulture),
                    row.RecordCount.ToString(CultureInfo.InvariantCulture),
                    row.FieldCount.ToString(CultureInfo.InvariantCulture),
                    row.DirectAbilityCount.ToString(CultureInfo.InvariantCulture),
                    row.DirectComponentCount.ToString(CultureInfo.InvariantCulture),
                    row.ParentRequirementCount.ToString(CultureInfo.InvariantCulture),
                    row.ChildRequirementCount.ToString(CultureInfo.InvariantCulture),
                    row.ContextTagsText,
                    row.SampleAbilitiesText,
                    row.SemanticSummary
                });

            foreach (RequirementLedgerRecord requirement in requirements)
            {
                builder.AppendLine("## Requirement " + requirement.RequirementId.ToString(CultureInfo.InvariantCulture));
                builder.AppendLine();
                builder.AppendLine("- Summary: " + requirement.SemanticSummary);
                builder.AppendLine("- Notes: " + requirement.Notes);
                builder.AppendLine();

                AppendTable(builder, "Rows", requirement.Rows ?? new List<RequirementRowRecord>(),
                    new[] { "RecordIndex", "ExtData", "Path", "Location" },
                    row => new[]
                    {
                        row.RecordIndex.ToString(CultureInfo.InvariantCulture),
                        row.ExtDataText,
                        row.SourcePath,
                        row.SourceLocation
                    });

                AppendTable(builder, "Field Activity", requirement.Fields ?? new List<RequirementFieldRecord>(),
                    new[] { "FieldKey", "NonZeroCount", "DistinctValueCount", "SampleValues", "SemanticSummary", "Confidence", "Notes" },
                    row => new[]
                    {
                        row.FieldKey,
                        row.NonZeroCount.ToString(CultureInfo.InvariantCulture),
                        row.DistinctValueCount.ToString(CultureInfo.InvariantCulture),
                        row.SampleValuesText,
                        row.SemanticSummary,
                        row.Confidence,
                        row.Notes
                    });

                AppendTable(builder, "Referenced By", requirement.InboundReferences ?? new List<RequirementLinkEvidenceRecord>(),
                    new[] { "SourceKind", "SourceId", "SourceLabel", "Field", "RelatedAbilities", "ContextTags", "Path", "Location", "Notes" },
                    row => new[]
                    {
                        row.SourceKind,
                        row.SourceId.ToString(CultureInfo.InvariantCulture),
                        row.SourceLabel,
                        row.SourceField,
                        row.RelatedAbilitiesText,
                        row.ContextTagsText,
                        row.SourcePath,
                        row.SourceLocation,
                        row.Notes
                    });

                AppendTable(builder, "References To", requirement.OutboundReferences ?? new List<RequirementLinkEvidenceRecord>(),
                    new[] { "RequirementId", "Field", "Path", "Location", "Notes" },
                    row => new[]
                    {
                        row.LinkedRequirementId.ToString(CultureInfo.InvariantCulture),
                        row.SourceField,
                        row.SourcePath,
                        row.SourceLocation,
                        row.Notes
                    });
            }

            return builder.ToString();
        }

        private static string BuildOperationSchemaMarkdown(OperationSchemaDocument report)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# Component Operation Schemas");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: `" + report.GeneratedAtUtc + "`");
            builder.AppendLine();
            builder.AppendLine("Extracted root: `" + report.ExtractedRootPath + "`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Override ledger: `" + report.OverrideLedgerPath + "`");
            builder.AppendLine("- Priority operations: `" + JoinValues((report.PriorityOperationIds ?? new List<uint>()).Select(value => value.ToString(CultureInfo.InvariantCulture))) + "`");
            builder.AppendLine("- This report is built from extracted client files first. `Londo` should appear only if a last-resort override was explicitly recorded.");
            builder.AppendLine();

            AppendTable(builder, "Source Status", report.TableStatuses ?? new List<TableLoadStatus>(),
                new[] { "Source", "File", "Loaded", "Rows", "Path", "Error" },
                row => new[] { row.SourceFamily, row.TableName, row.Loaded.ToString(), row.RowCount.ToString(CultureInfo.InvariantCulture), row.SourcePath, NullToEmpty(row.ErrorMessage) });

            AppendTable(builder, "Operation Summary", report.Operations ?? new List<ComponentOperationSchemaRecord>(),
                new[] { "OperationId", "OperationName", "Priority", "Components", "Abilities", "Layouts", "ContextTags", "SampleComponentIds" },
                row => new[]
                {
                    row.OperationId.ToString(CultureInfo.InvariantCulture),
                    row.OperationName,
                    row.IsPriority ? "Yes" : "No",
                    row.ComponentCount.ToString(CultureInfo.InvariantCulture),
                    row.AbilityCount.ToString(CultureInfo.InvariantCulture),
                    row.LayoutVariantsText,
                    JoinValues(row.ContextTags ?? new List<string>()),
                    JoinValues((row.SampleComponentIds ?? new List<ushort>()).Select(value => value.ToString(CultureInfo.InvariantCulture)))
                });

            foreach (ComponentOperationSchemaRecord operation in report.Operations ?? new List<ComponentOperationSchemaRecord>())
            {
                builder.AppendLine("## Operation " + operation.OperationId.ToString(CultureInfo.InvariantCulture) + " - " + operation.OperationName);
                builder.AppendLine();
                builder.AppendLine("- Priority: `" + (operation.IsPriority ? "Yes" : "No") + "`");
                builder.AppendLine("- Component rows: `" + operation.ComponentCount.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Referencing abilities: `" + operation.AbilityCount.ToString(CultureInfo.InvariantCulture) + "`");
                builder.AppendLine("- Layout variants: `" + NullToEmpty(operation.LayoutVariantsText) + "`");
                builder.AppendLine("- Context tags: `" + JoinValues(operation.ContextTags ?? new List<string>()) + "`");
                builder.AppendLine("- Sample component ids: `" + JoinValues((operation.SampleComponentIds ?? new List<ushort>()).Select(value => value.ToString(CultureInfo.InvariantCulture))) + "`");
                builder.AppendLine();

                AppendTable(builder, "Field Schema", operation.Fields ?? new List<ComponentOperationFieldRecord>(),
                    new[] { "FieldKey", "NonZero", "DistinctValues", "SampleValues", "TokenRenderings", "SemanticSummary", "Confidence", "Triage", "Score", "ContextTags", "Notes" },
                    row => new[]
                    {
                        row.FieldKey,
                        row.NonZeroCount.ToString(CultureInfo.InvariantCulture),
                        row.DistinctValueCount.ToString(CultureInfo.InvariantCulture),
                        row.SampleValuesText,
                        row.TokenRenderingsText,
                        row.SemanticSummary,
                        row.Confidence,
                        row.TriageBucket,
                        row.TriageScore.ToString(CultureInfo.InvariantCulture),
                        row.ContextTagsText,
                        string.IsNullOrWhiteSpace(row.TriageNotes) ? row.Notes : row.Notes + " " + row.TriageNotes
                    });

                AppendTable(builder, "Sample Abilities", operation.SampleAbilities ?? new List<ComponentOperationAbilityRecord>(),
                    new[] { "AbilityId", "AbilityName", "ComponentId", "Slot", "Trigger", "ContextTags", "TextExcerpt", "Path", "Location" },
                    row => new[]
                    {
                        row.AbilityId.ToString(CultureInfo.InvariantCulture),
                        row.AbilityName,
                        row.ComponentId.ToString(CultureInfo.InvariantCulture),
                        row.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture),
                        row.TriggerText,
                        row.ContextTagsText,
                        row.TextExcerpt,
                        row.SourcePath,
                        row.SourceLocation
                    });
            }

            return builder.ToString();
        }

        private static string BuildDot(AnalysisGraph graph)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("digraph ClientDataMatrix {");
            builder.AppendLine("  rankdir=LR;");

            foreach (GraphNode node in graph.Nodes)
                builder.AppendLine("  \"" + EscapeDot(node.Id) + "\" [label=\"" + EscapeDot(node.Label) + "\\n" + EscapeDot(node.Kind) + "\"];");

            foreach (GraphEdge edge in graph.Edges)
                builder.AppendLine("  \"" + EscapeDot(edge.FromNodeId) + "\" -> \"" + EscapeDot(edge.ToNodeId) + "\" [label=\"" + EscapeDot(edge.Kind + (string.IsNullOrWhiteSpace(edge.Label) ? string.Empty : " " + edge.Label)) + "\"];");

            builder.AppendLine("}");
            return builder.ToString();
        }

        private static void AppendTable<T>(StringBuilder builder, string title, IList<T> rows, string[] headers, Func<T, string[]> rowSelector)
        {
            builder.AppendLine("## " + title);
            builder.AppendLine();
            if (rows == null || rows.Count == 0)
            {
                builder.AppendLine("No rows found.");
                builder.AppendLine();
                return;
            }

            builder.AppendLine("| " + string.Join(" | ", headers) + " |");
            builder.AppendLine("| " + string.Join(" | ", headers.Select(header => header.EndsWith("Id", StringComparison.OrdinalIgnoreCase) || header == "Rows" || header == "Line" ? "---:" : "---")) + " |");
            foreach (T row in rows)
                builder.AppendLine("| " + string.Join(" | ", rowSelector(row).Select(EscapeMarkdownCell)) + " |");
            builder.AppendLine();
        }

        private static void AppendConflictSection(StringBuilder builder, IList<ConflictRecord> conflicts)
        {
            builder.AppendLine("## Conflicts");
            builder.AppendLine();
            if (conflicts == null || conflicts.Count == 0)
            {
                builder.AppendLine("No conflicting claims were found for the current scope.");
                builder.AppendLine();
                return;
            }

            foreach (ConflictRecord conflict in conflicts)
            {
                builder.AppendLine("### " + conflict.SubjectKey + " :: " + conflict.FactName);
                builder.AppendLine();
                builder.AppendLine("- Domain: `" + conflict.Domain + "`");
                builder.AppendLine("- Triage: `" + conflict.TriageBucket + "` | score `" + conflict.TriageScore.ToString(CultureInfo.InvariantCulture) + "` | category `" + conflict.TriageCategory + "`");
                builder.AppendLine("- Distinct values: `" + string.Join("`, `", conflict.DistinctValues.Select(NullToEmpty)) + "`");
                if (!string.IsNullOrWhiteSpace(conflict.ResolutionRule) || !string.IsNullOrWhiteSpace(conflict.CanonicalValue))
                {
                    string resolution = string.Join(" -> ", new[]
                    {
                        conflict.RecommendedSourceFamily,
                        conflict.CanonicalValue,
                        conflict.CanonicalMeaning
                    }.Where(value => !string.IsNullOrWhiteSpace(value)));
                    builder.AppendLine("- Resolution: `" + NullToEmpty(conflict.ResolutionRule) + "`" + (string.IsNullOrWhiteSpace(resolution) ? string.Empty : " | `" + EscapeMarkdownCell(resolution) + "`"));
                }
                if (!string.IsNullOrWhiteSpace(conflict.ResolutionNotes))
                    builder.AppendLine("- Resolution notes: " + EscapeMarkdownCell(conflict.ResolutionNotes));
                builder.AppendLine("- Notes: " + EscapeMarkdownCell(NullToEmpty(conflict.TriageNotes)));
                builder.AppendLine();
                AppendTable(builder, "Evidence", conflict.Claims,
                    new[] { "Value", "Source", "File", "Path", "Line", "ByteOffset", "Field", "Confidence" },
                    claim => new[] { claim.NormalizedValue, claim.SourceFamily, claim.TableName, claim.SourcePath, claim.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(claim.ByteOffset), claim.FieldName, claim.Confidence });
            }
        }

        private static void AppendClaimSection(StringBuilder builder, IList<ClaimRecord> claims)
        {
            AppendTable(builder, "Claim Ledger", claims,
                new[] { "Subject", "Fact", "Domain", "Value", "Source", "File", "Path", "Line", "ByteOffset", "Field", "Confidence" },
                claim => new[] { claim.SubjectKey, claim.FactName, claim.Domain, claim.NormalizedValue, claim.SourceFamily, claim.TableName, claim.SourcePath, claim.LineNumber.ToString(CultureInfo.InvariantCulture), FormatByteOffset(claim.ByteOffset), claim.FieldName, claim.Confidence });
        }

        private static void WriteNodesCsv(string path, IList<GraphNode> nodes)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Id,Kind,Key,Label,Properties");
            foreach (GraphNode node in nodes)
            {
                string properties = string.Join(" | ", node.Properties.OrderBy(entry => entry.Key).Select(entry => entry.Key + "=" + entry.Value));
                builder.AppendLine(string.Join(",", new[] { EscapeCsv(node.Id), EscapeCsv(node.Kind), EscapeCsv(node.Key), EscapeCsv(node.Label), EscapeCsv(properties) }));
            }
            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteEdgesCsv(string path, IList<GraphEdge> edges)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Id,Kind,FromNodeId,ToNodeId,Label,ClaimId");
            foreach (GraphEdge edge in edges)
                builder.AppendLine(string.Join(",", new[] { EscapeCsv(edge.Id), EscapeCsv(edge.Kind), EscapeCsv(edge.FromNodeId), EscapeCsv(edge.ToNodeId), EscapeCsv(edge.Label), EscapeCsv(edge.ClaimId) }));
            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteClaimsCsv(string path, IList<ClaimRecord> claims)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Id,SubjectKey,FactName,Domain,ValueKey,RawValue,NormalizedValue,SourceFamily,TableName,SourcePath,RowKey,LineNumber,ByteOffset,FieldName,Confidence,Notes");
            foreach (ClaimRecord claim in claims)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(claim.Id),
                    EscapeCsv(claim.SubjectKey),
                    EscapeCsv(claim.FactName),
                    EscapeCsv(claim.Domain),
                    EscapeCsv(claim.ValueKey),
                    EscapeCsv(claim.RawValue),
                    EscapeCsv(claim.NormalizedValue),
                    EscapeCsv(claim.SourceFamily),
                    EscapeCsv(claim.TableName),
                    EscapeCsv(claim.SourcePath),
                    EscapeCsv(claim.RowKey),
                    EscapeCsv(claim.LineNumber.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(claim.ByteOffset.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(claim.FieldName),
                    EscapeCsv(claim.Confidence),
                    EscapeCsv(claim.Notes)
                }));
            }
            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteTokenDefinitionsCsv(string path, IList<TokenDefinitionRecord> definitions)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("TokenBody,ExampleToken,FieldKey,PlainEnglishMeaning,Confidence,ContextTags,Source,SourcePath,Notes,ExampleAbilityIds");
            foreach (TokenDefinitionRecord definition in definitions)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(definition.TokenBody),
                    EscapeCsv(definition.ExampleToken),
                    EscapeCsv(definition.FieldKey),
                    EscapeCsv(definition.PlainEnglishMeaning),
                    EscapeCsv(definition.Confidence),
                    EscapeCsv(string.Join(" ", definition.ContextTags ?? new List<string>())),
                    EscapeCsv(definition.Source),
                    EscapeCsv(definition.SourcePath),
                    EscapeCsv(definition.Notes),
                    EscapeCsv(string.Join(" ", definition.ExampleAbilityIds.Select(value => value.ToString(CultureInfo.InvariantCulture))))
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteCoverageCsv(string path, IList<CoverageAbilityRecord> abilities)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("AbilityId,Name,CoverageStatus,EffectId,HasClientCsv,HasClientBin,HasLocalizedName,HasDescriptionText,HasEffectText,HasRootEffectRow,RelatedEffectCount,ComponentCount,RequirementLinkCount,RequirementRowCount,HasPregameReference,SourcesText,MissingText");
            foreach (CoverageAbilityRecord row in abilities)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(row.AbilityId.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(row.Name),
                    EscapeCsv(row.CoverageStatus),
                    EscapeCsv(row.EffectId.HasValue ? row.EffectId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                    EscapeCsv(row.HasClientCsv ? "Yes" : "No"),
                    EscapeCsv(row.HasClientBin ? "Yes" : "No"),
                    EscapeCsv(row.HasLocalizedName ? "Yes" : "No"),
                    EscapeCsv(row.HasDescriptionText ? "Yes" : "No"),
                    EscapeCsv(row.HasEffectText ? "Yes" : "No"),
                    EscapeCsv(row.HasRootEffectRow ? "Yes" : "No"),
                    EscapeCsv(row.RelatedEffectCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(row.ComponentCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(row.RequirementLinkCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(row.RequirementRowCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(row.HasPregameReference ? "Yes" : "No"),
                    EscapeCsv(row.SourcesText),
                    EscapeCsv(row.MissingText)
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteRemainingWorkAreasCsv(string path, IList<RemainingWorkAreaRecord> areas)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("AreaKey,Title,ItemCount,CriticalCount,HighCount,PeakScore,PeakBucket,Summary,RecommendedNextStep");
            foreach (RemainingWorkAreaRecord area in areas)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(area.AreaKey),
                    EscapeCsv(area.Title),
                    EscapeCsv(area.ItemCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(area.CriticalCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(area.HighCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(area.PeakScore.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(area.PeakBucket),
                    EscapeCsv(area.Summary),
                    EscapeCsv(area.RecommendedNextStep)
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteRemainingWorkItemsCsv(string path, IList<RemainingWorkItemRecord> items)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("AreaKey,AreaTitle,Rank,GlobalRank,PriorityBucket,PriorityScore,SubjectKind,SubjectKey,Title,Summary,Evidence,RecommendedAction,ExampleAbilityId,ReferencePath,ReferenceLocation");
            foreach (RemainingWorkItemRecord item in items)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(item.AreaKey),
                    EscapeCsv(item.AreaTitle),
                    EscapeCsv(item.Rank.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(item.GlobalRank.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(item.PriorityBucket),
                    EscapeCsv(item.PriorityScore.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(item.SubjectKind),
                    EscapeCsv(item.SubjectKey),
                    EscapeCsv(item.Title),
                    EscapeCsv(item.Summary),
                    EscapeCsv(item.Evidence),
                    EscapeCsv(item.RecommendedAction),
                    EscapeCsv(item.ExampleAbilityId.HasValue ? item.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                    EscapeCsv(item.ReferencePath),
                    EscapeCsv(item.ReferenceLocation)
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteOperationFieldWorkPacketCsv(string path, IList<OperationFieldWorkPacketRecord> packets)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("GlobalRank,AreaRank,OperationId,OperationName,FieldKey,PriorityBucket,PriorityScore,Title,Summary,Evidence,RecommendedAction,ExampleAbilityId,SampleValues");
            foreach (OperationFieldWorkPacketRecord packet in packets)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(packet.GlobalRank.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(packet.AreaRank.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(packet.OperationId.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(packet.OperationName),
                    EscapeCsv(packet.FieldKey),
                    EscapeCsv(packet.PriorityBucket),
                    EscapeCsv(packet.PriorityScore.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(packet.Title),
                    EscapeCsv(packet.Summary),
                    EscapeCsv(packet.Evidence),
                    EscapeCsv(packet.RecommendedAction),
                    EscapeCsv(packet.ExampleAbilityId.HasValue ? packet.ExampleAbilityId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty),
                    EscapeCsv(packet.SampleValuesText)
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteOperationFieldWorkPacketValuesCsv(string path, IList<OperationFieldWorkPacketRecord> packets)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("OperationId,OperationName,FieldKey,RawValue,ObservationCount,DistinctComponentCount,DistinctAbilityCount,SampleComponentIds,SampleAbilityIds,TriggerSummary,ContextTagSummary,CompanionSummary");
            foreach (OperationFieldWorkPacketRecord packet in packets)
            {
                foreach (OperationFieldWorkPacketValueRecord value in packet.TopValues ?? new List<OperationFieldWorkPacketValueRecord>())
                {
                    builder.AppendLine(string.Join(",", new[]
                    {
                        EscapeCsv(packet.OperationId.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(packet.OperationName),
                        EscapeCsv(packet.FieldKey),
                        EscapeCsv(value.RawValue),
                        EscapeCsv(value.ObservationCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(value.DistinctComponentCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(value.DistinctAbilityCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(value.SampleComponentIdsText),
                        EscapeCsv(value.SampleAbilityIdsText),
                        EscapeCsv(value.TriggerSummaryText),
                        EscapeCsv(value.ContextTagSummaryText),
                        EscapeCsv(value.CompanionSummaryText)
                    }));
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteOperationFieldWorkPacketAbilitiesCsv(string path, IList<OperationFieldWorkPacketRecord> packets)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("OperationId,OperationName,FieldKey,RawValue,AbilityId,AbilityName,ComponentId,ComponentSlotIndex,TriggerText,ContextTagsText,TextExcerpt,SourcePath,SourceLocation");
            foreach (OperationFieldWorkPacketRecord packet in packets)
            {
                foreach (OperationFieldWorkPacketValueRecord value in packet.TopValues ?? new List<OperationFieldWorkPacketValueRecord>())
                {
                    foreach (ComponentOperationAbilityRecord ability in value.SampleAbilities ?? new List<ComponentOperationAbilityRecord>())
                    {
                        builder.AppendLine(string.Join(",", new[]
                        {
                            EscapeCsv(packet.OperationId.ToString(CultureInfo.InvariantCulture)),
                            EscapeCsv(packet.OperationName),
                            EscapeCsv(packet.FieldKey),
                            EscapeCsv(value.RawValue),
                            EscapeCsv(ability.AbilityId.ToString(CultureInfo.InvariantCulture)),
                            EscapeCsv(ability.AbilityName),
                            EscapeCsv(ability.ComponentId.ToString(CultureInfo.InvariantCulture)),
                            EscapeCsv(ability.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture)),
                            EscapeCsv(ability.TriggerText),
                            EscapeCsv(ability.ContextTagsText),
                            EscapeCsv(ability.TextExcerpt),
                            EscapeCsv(ability.SourcePath),
                            EscapeCsv(ability.SourceLocation)
                        }));
                    }
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteControlLiteralCrosswalkCsv(string path, IList<ControlLiteralRecord> literals)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("GlobalRank,PriorityScore,RawValue,ObservationCount,DistinctSourceCount,DistinctComponentCount,DistinctAbilityCount,DistinctRequirementCount,SourceKeys,ContextTags,SampleAbilityIds,SampleRequirementIds,Interpretation,Notes,Summary");
            foreach (ControlLiteralRecord literal in literals)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(literal.GlobalRank.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(literal.PriorityScore.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(literal.RawValue),
                    EscapeCsv(literal.ObservationCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(literal.DistinctSourceCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(literal.DistinctComponentCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(literal.DistinctAbilityCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(literal.DistinctRequirementCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(literal.SourceKeysText),
                    EscapeCsv(literal.ContextTagsText),
                    EscapeCsv(literal.SampleAbilityIdsText),
                    EscapeCsv(literal.SampleRequirementIdsText),
                    EscapeCsv(literal.Interpretation),
                    EscapeCsv(literal.Notes),
                    EscapeCsv(literal.Summary)
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteControlLiteralCrosswalkSourcesCsv(string path, IList<ControlLiteralRecord> literals)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("RawValue,SourceKey,SourceLabel,ObservationCount,DistinctComponentCount,DistinctAbilityCount,DistinctRequirementCount,SampleComponentIds,SampleAbilityIds,SampleRequirementIds,TriggerSummary,ContextTagSummary,CompanionSummary");
            foreach (ControlLiteralRecord literal in literals)
            {
                foreach (ControlLiteralSourceRecord source in literal.Sources ?? new List<ControlLiteralSourceRecord>())
                {
                    builder.AppendLine(string.Join(",", new[]
                    {
                        EscapeCsv(literal.RawValue),
                        EscapeCsv(source.SourceKey),
                        EscapeCsv(source.SourceLabel),
                        EscapeCsv(source.ObservationCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(source.DistinctComponentCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(source.DistinctAbilityCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(source.DistinctRequirementCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(source.SampleComponentIdsText),
                        EscapeCsv(source.SampleAbilityIdsText),
                        EscapeCsv(source.SampleRequirementIdsText),
                        EscapeCsv(source.TriggerSummaryText),
                        EscapeCsv(source.ContextTagSummaryText),
                        EscapeCsv(source.CompanionSummaryText)
                    }));
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteDomainLedgerCsv(string path, IList<IdentityDomainRecord> domains)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DomainKey,DisplayName,Confidence,Canonicality,ValueCount,DistinctMeaningCount,DuplicateMeaningCount,SourceFilesText,RecommendedUsage,Notes");
            foreach (IdentityDomainRecord row in domains)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(row.DomainKey),
                    EscapeCsv(row.DisplayName),
                    EscapeCsv(row.Confidence),
                    EscapeCsv(row.Canonicality),
                    EscapeCsv(row.ValueCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(row.DistinctMeaningCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(row.DuplicateMeaningCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(row.SourceFilesText),
                    EscapeCsv(row.RecommendedUsage),
                    EscapeCsv(row.Notes)
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteDomainLedgerValuesCsv(string path, IList<IdentityDomainRecord> domains)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("DomainKey,DisplayName,RawValue,Meaning,Confidence,Source,SourcePath,SourceLocation,Notes");
            foreach (IdentityDomainRecord domain in domains)
            {
                foreach (IdentityDomainValueRecord value in domain.Values ?? new List<IdentityDomainValueRecord>())
                {
                    builder.AppendLine(string.Join(",", new[]
                    {
                        EscapeCsv(domain.DomainKey),
                        EscapeCsv(domain.DisplayName),
                        EscapeCsv(value.RawValue),
                        EscapeCsv(value.Meaning),
                        EscapeCsv(value.Confidence),
                        EscapeCsv(value.Source),
                        EscapeCsv(value.SourcePath),
                        EscapeCsv(value.SourceLocation),
                        EscapeCsv(value.Notes)
                    }));
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteRequirementLedgerCsv(string path, IList<RequirementLedgerRecord> requirements)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("RequirementId,RecordCount,FieldCount,DirectAbilityCount,DirectComponentCount,ParentRequirementCount,ChildRequirementCount,ContextTags,SampleAbilities,SemanticSummary,Notes");
            foreach (RequirementLedgerRecord requirement in requirements)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(requirement.RequirementId.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(requirement.RecordCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(requirement.FieldCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(requirement.DirectAbilityCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(requirement.DirectComponentCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(requirement.ParentRequirementCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(requirement.ChildRequirementCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(requirement.ContextTagsText),
                    EscapeCsv(requirement.SampleAbilitiesText),
                    EscapeCsv(requirement.SemanticSummary),
                    EscapeCsv(requirement.Notes)
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteRequirementLedgerFieldsCsv(string path, IList<RequirementLedgerRecord> requirements)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("RequirementId,FieldKey,NonZeroCount,DistinctValueCount,SampleValues,SemanticSummary,Confidence,Notes");
            foreach (RequirementLedgerRecord requirement in requirements)
            {
                foreach (RequirementFieldRecord field in requirement.Fields ?? new List<RequirementFieldRecord>())
                {
                    builder.AppendLine(string.Join(",", new[]
                    {
                        EscapeCsv(requirement.RequirementId.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(field.FieldKey),
                        EscapeCsv(field.NonZeroCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(field.DistinctValueCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(field.SampleValuesText),
                        EscapeCsv(field.SemanticSummary),
                        EscapeCsv(field.Confidence),
                        EscapeCsv(field.Notes)
                    }));
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteRequirementLedgerReferencesCsv(string path, IList<RequirementLedgerRecord> requirements)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("RequirementId,Direction,SourceKind,SourceId,SourceLabel,SourceField,ExtSlotIndex,LinkedRequirementId,RelatedAbilities,ContextTags,SourcePath,SourceLocation,Notes");
            foreach (RequirementLedgerRecord requirement in requirements)
            {
                foreach (RequirementLinkEvidenceRecord reference in requirement.InboundReferences ?? new List<RequirementLinkEvidenceRecord>())
                    WriteRequirementReferenceCsvLine(builder, requirement.RequirementId, "Inbound", reference);
                foreach (RequirementLinkEvidenceRecord reference in requirement.OutboundReferences ?? new List<RequirementLinkEvidenceRecord>())
                    WriteRequirementReferenceCsvLine(builder, requirement.RequirementId, "Outbound", reference);
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteRequirementLedgerRowsCsv(string path, IList<RequirementLedgerRecord> requirements)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("RequirementId,RecordIndex,ExtData,SourcePath,SourceLocation");
            foreach (RequirementLedgerRecord requirement in requirements)
            {
                foreach (RequirementRowRecord row in requirement.Rows ?? new List<RequirementRowRecord>())
                {
                    builder.AppendLine(string.Join(",", new[]
                    {
                        EscapeCsv(requirement.RequirementId.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(row.RecordIndex.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(row.ExtDataText),
                        EscapeCsv(row.SourcePath),
                        EscapeCsv(row.SourceLocation)
                    }));
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteRequirementReferenceCsvLine(StringBuilder builder, ushort requirementId, string direction, RequirementLinkEvidenceRecord reference)
        {
            builder.AppendLine(string.Join(",", new[]
            {
                EscapeCsv(requirementId.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(direction),
                EscapeCsv(reference.SourceKind),
                EscapeCsv(reference.SourceId.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(reference.SourceLabel),
                EscapeCsv(reference.SourceField),
                EscapeCsv(reference.ExtSlotIndex.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(reference.LinkedRequirementId.ToString(CultureInfo.InvariantCulture)),
                EscapeCsv(reference.RelatedAbilitiesText),
                EscapeCsv(reference.ContextTagsText),
                EscapeCsv(reference.SourcePath),
                EscapeCsv(reference.SourceLocation),
                EscapeCsv(reference.Notes)
            }));
        }

        private static void WriteOperationSchemasCsv(string path, IList<ComponentOperationSchemaRecord> operations)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("OperationId,OperationName,IsPriority,ComponentCount,AbilityCount,LayoutVariants,ContextTags,SampleComponentIds");
            foreach (ComponentOperationSchemaRecord operation in operations)
            {
                builder.AppendLine(string.Join(",", new[]
                {
                    EscapeCsv(operation.OperationId.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(operation.OperationName),
                    EscapeCsv(operation.IsPriority ? "Yes" : "No"),
                    EscapeCsv(operation.ComponentCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(operation.AbilityCount.ToString(CultureInfo.InvariantCulture)),
                    EscapeCsv(operation.LayoutVariantsText),
                    EscapeCsv(string.Join(" ", operation.ContextTags ?? new List<string>())),
                    EscapeCsv(string.Join(" ", (operation.SampleComponentIds ?? new List<ushort>()).Select(value => value.ToString(CultureInfo.InvariantCulture))))
                }));
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteOperationFieldsCsv(string path, IList<ComponentOperationSchemaRecord> operations)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("OperationId,OperationName,FieldKey,NonZeroCount,DistinctValueCount,SampleValues,TokenRenderings,SemanticSummary,Confidence,TriageBucket,TriageScore,TriageNotes,ContextTags,Notes");
            foreach (ComponentOperationSchemaRecord operation in operations)
            {
                foreach (ComponentOperationFieldRecord field in operation.Fields ?? new List<ComponentOperationFieldRecord>())
                {
                    builder.AppendLine(string.Join(",", new[]
                    {
                        EscapeCsv(operation.OperationId.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(operation.OperationName),
                        EscapeCsv(field.FieldKey),
                        EscapeCsv(field.NonZeroCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(field.DistinctValueCount.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(field.SampleValuesText),
                        EscapeCsv(field.TokenRenderingsText),
                        EscapeCsv(field.SemanticSummary),
                        EscapeCsv(field.Confidence),
                        EscapeCsv(field.TriageBucket),
                        EscapeCsv(field.TriageScore.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(field.TriageNotes),
                        EscapeCsv(field.ContextTagsText),
                        EscapeCsv(field.Notes)
                    }));
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private static void WriteOperationAbilitiesCsv(string path, IList<ComponentOperationSchemaRecord> operations)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("OperationId,OperationName,AbilityId,AbilityName,ComponentId,ComponentSlotIndex,TriggerText,ContextTags,TextExcerpt,SourcePath,SourceLocation");
            foreach (ComponentOperationSchemaRecord operation in operations)
            {
                foreach (ComponentOperationAbilityRecord ability in operation.SampleAbilities ?? new List<ComponentOperationAbilityRecord>())
                {
                    builder.AppendLine(string.Join(",", new[]
                    {
                        EscapeCsv(operation.OperationId.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(operation.OperationName),
                        EscapeCsv(ability.AbilityId.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(ability.AbilityName),
                        EscapeCsv(ability.ComponentId.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(ability.ComponentSlotIndex.ToString(CultureInfo.InvariantCulture)),
                        EscapeCsv(ability.TriggerText),
                        EscapeCsv(ability.ContextTagsText),
                        EscapeCsv(ability.TextExcerpt),
                        EscapeCsv(ability.SourcePath),
                        EscapeCsv(ability.SourceLocation)
                    }));
                }
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }

        private sealed class OperationFieldWorkPacketAbilityRow
        {
            public string RawValue { get; set; }
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

        private static string FormatByteOffset(long byteOffset)
        {
            return byteOffset > 0 ? byteOffset.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        private static string EscapeCsv(string value)
        {
            string safeValue = value ?? string.Empty;
            if (safeValue.Contains("\""))
                safeValue = safeValue.Replace("\"", "\"\"");
            if (safeValue.Contains(",") || safeValue.Contains("\r") || safeValue.Contains("\n"))
                safeValue = "\"" + safeValue + "\"";
            return safeValue;
        }

        private static string EscapeDot(string value)
        {
            return (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private static string EscapeMarkdownCell(string value)
        {
            return NullToEmpty(value).Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }

        private static string NullToEmpty(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }

        private static string NullableToText(object value)
        {
            return value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        private static string JoinValues(IEnumerable<string> values)
        {
            List<string> items = values == null ? new List<string>() : values.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
            return items.Count == 0 ? "(none)" : string.Join(", ", items);
        }
    }
}

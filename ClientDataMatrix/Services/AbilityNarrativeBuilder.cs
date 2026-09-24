using ClientDataMatrix.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ClientDataMatrix.Services
{
    public sealed class AbilityNarrativeBuilder
    {
        private readonly DefinitionCatalog _definitions;

        public AbilityNarrativeBuilder(DefinitionCatalog definitions)
        {
            _definitions = definitions;
        }

        public string BuildNarrative(AbilityAnalysisResult report)
        {
            if (report == null)
                return "No ability report has been generated yet.";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("This explanation is inferred from extracted client files only.");
            builder.AppendLine("It does not use emulator database rows.");
            builder.AppendLine();

            string abilityName = GetAbilityName(report);
            BinaryAbilityRecord abilityRow = report.BinaryAbilityRows.FirstOrDefault();

            // abilities.csv is reached through the BIN record's EffectId, so without a BIN record
            // there is no client row of any kind to explain.
            if (abilityRow == null)
            {
                builder.AppendLine("No abilityexport.bin record was found for this ability ID.");
                return builder.ToString().TrimEnd();
            }

            builder.AppendLine(abilityName + " (" + report.AbilityId.ToString(CultureInfo.InvariantCulture) + ")");
            builder.AppendLine();

            builder.AppendLine("1. The BIN row says this is a "
                + _definitions.DescribeAbilityType(abilityRow.AbilityType).ToLowerInvariant()
                + " ability that targets "
                + _definitions.DescribeTargetType(abilityRow.TargetType).ToLowerInvariant()
                + " and uses "
                + _definitions.DescribeAttackType(abilityRow.AttackType).ToLowerInvariant()
                + " attack typing.");

            builder.AppendLine("2. The cost/timing block says it has cast time "
                + abilityRow.CastTime.ToString(CultureInfo.InvariantCulture) + " ms, cooldown "
                + abilityRow.Cooldown.ToString(CultureInfo.InvariantCulture) + " ms, range "
                + abilityRow.Range.ToString(CultureInfo.InvariantCulture) + " feet, and AP cost "
                + abilityRow.ApCost.ToString(CultureInfo.InvariantCulture) + ".");

            builder.AppendLine("3. The career line value "
                + abilityRow.CareerLine.ToString(CultureInfo.InvariantCulture)
                + " resolves to "
                + _definitions.DescribeCareerLine(abilityRow.CareerLine) + ".");

            if (abilityRow.EffectId > 0)
            {
                builder.AppendLine("4. The BIN row points at client effect " + abilityRow.EffectId.ToString(CultureInfo.InvariantCulture) + ", which is the visual or staged effect entry the client will follow.");
                ClientAbilityRecord artRow = report.ClientAbilityRows.FirstOrDefault(row => row.EffectId == abilityRow.EffectId);
                if (artRow != null)
                    builder.AppendLine("   Its abilities.csv art row, keyed by that effect id, is \"" + artRow.Name + "\" with icon " + artRow.IconId.ToString(CultureInfo.InvariantCulture) + ".");
            }

            AppendEffectNarrative(builder, report, abilityRow.EffectId);
            AppendComponentNarrative(builder, report, abilityRow);
            AppendRequirementNarrative(builder, report);

            if (report.Warnings != null && report.Warnings.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Open questions and missing links:");
                for (int index = 0; index < report.Warnings.Count; ++index)
                    builder.AppendLine("- " + report.Warnings[index]);
            }

            if (report.Graph != null && report.Graph.GetConflicts().Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Conflict note:");
                builder.AppendLine("- This ability has " + report.Graph.GetConflicts().Count.ToString(CultureInfo.InvariantCulture) + " conflicting claims across the extracted sources, so the explanation above is only the current best client-data reading.");
            }

            return builder.ToString().TrimEnd();
        }

        private void AppendEffectNarrative(StringBuilder builder, AbilityAnalysisResult report, int rootEffectId)
        {
            if (rootEffectId <= 0)
                return;

            ClientEffectRecord effectRow = report.ClientEffectRows.FirstOrDefault(row => row.EffectId == rootEffectId);
            if (effectRow == null)
            {
                builder.AppendLine("5. No decoded effects.csv row was found for root effect " + rootEffectId.ToString(CultureInfo.InvariantCulture) + ".");
                return;
            }

            builder.AppendLine("5. The root effect is " + effectRow.EffectId.ToString(CultureInfo.InvariantCulture) + " (" + effectRow.Name + ").");

            List<string> links = new List<string>();
            AddEffectLink(links, "build-up", effectRow.BuildUpId, report);
            AddEffectLink(links, "cast", effectRow.CastId, report);
            AddEffectLink(links, "activate", effectRow.ActivateId, report);
            AddEffectLink(links, "projectile", effectRow.ProjectileId, report);
            AddEffectLink(links, "impact", effectRow.ImpactId, report);
            AddEffectLink(links, "aoe", effectRow.AoeId, report);
            AddEffectLink(links, "channel", effectRow.ChannelingId, report);

            if (links.Count == 0)
                builder.AppendLine("6. No linked effect stages were found for the root effect, so the client graph looks single-stage from the extracted data.");
            else
                builder.AppendLine("6. The linked client effect graph references: " + string.Join("; ", links) + ".");
        }

        private void AppendComponentNarrative(StringBuilder builder, AbilityAnalysisResult report, BinaryAbilityRecord abilityRow)
        {
            List<string> componentSteps = new List<string>();
            for (int index = 0; index < abilityRow.ComponentIds.Count; ++index)
            {
                ushort componentId = abilityRow.ComponentIds[index];
                if (componentId <= 0)
                    continue;

                BinaryComponentRecord componentRow = report.BinaryComponentRows.FirstOrDefault(row => row.ComponentId == componentId);
                uint triggerValue = index < abilityRow.Triggers.Count ? abilityRow.Triggers[index] : 0;
                string triggerName = triggerValue > 0 ? _definitions.DescribeTrigger(triggerValue) : "No explicit trigger";

                if (componentRow == null)
                {
                    componentSteps.Add("component " + componentId.ToString(CultureInfo.InvariantCulture) + " has no decoded row, so its operation is still unknown");
                    continue;
                }

                string operation = _definitions.DescribeComponentOperation(componentRow.Operation);
                string detail = componentRow.Description;
                string sentence = "component " + componentId.ToString(CultureInfo.InvariantCulture)
                    + " runs as " + operation.ToLowerInvariant()
                    + " with trigger " + triggerName;

                if (!string.IsNullOrWhiteSpace(detail))
                    sentence += " and description \"" + detail + "\"";

                componentSteps.Add(sentence);
            }

            if (componentSteps.Count == 0)
            {
                builder.AppendLine("7. No component execution rows were decoded for this ability, so the actual gameplay action is still opaque.");
                return;
            }

            builder.AppendLine("7. The component stack suggests the next gameplay steps are:");
            for (int index = 0; index < componentSteps.Count; ++index)
                builder.AppendLine("   - " + componentSteps[index] + ".");
        }

        private static void AppendRequirementNarrative(StringBuilder builder, AbilityAnalysisResult report)
        {
            List<RequirementReferenceRecord> references = report.RequirementReferences ?? new List<RequirementReferenceRecord>();
            if (references.Count == 0)
            {
                builder.AppendLine("8. No inferred requirement references were detected for this ability under the current ExtData[*].Val6 linking rule.");
                return;
            }

            List<string> sources = references
                .Select(row => row.SourceKind + " " + row.SourceId.ToString(CultureInfo.InvariantCulture))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .Take(12)
                .ToList();

            builder.AppendLine("8. The BIN data also points at "
                + references.Select(row => row.RequirementId).Distinct().Count().ToString(CultureInfo.InvariantCulture)
                + " requirement row(s). These links are inferred because ExtData[*].Val6 matches known RequirementId values in abilityrequirementexport.bin.");
            builder.AppendLine("9. Requirement references originate from: " + string.Join("; ", sources) + ".");
        }

        private static string GetAbilityName(AbilityAnalysisResult report)
        {
            IndexedStringRecord nameRow = report.AbilityNameRows.FirstOrDefault(row => !string.IsNullOrWhiteSpace(row.NormalizedValue));
            if (nameRow != null)
                return nameRow.NormalizedValue;

            return "Ability";
        }

        private static void AddEffectLink(List<string> links, string label, int effectId, AbilityAnalysisResult report)
        {
            if (effectId <= 0)
                return;

            ClientEffectRecord effectRow = report.ClientEffectRows.FirstOrDefault(row => row.EffectId == effectId);
            string text = effectRow == null
                ? label + " -> effect " + effectId.ToString(CultureInfo.InvariantCulture)
                : label + " -> effect " + effectId.ToString(CultureInfo.InvariantCulture) + " (" + effectRow.Name + ")";
            links.Add(text);
        }
    }
}

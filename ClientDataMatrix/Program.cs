using ClientDataMatrix.Configuration;
using ClientDataMatrix.Model;
using ClientDataMatrix.Services;
using ClientDataMatrix.UI;
using ClientDataMatrix.Output;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Windows.Forms;

namespace ClientDataMatrix
{
    internal static class Program
    {
        private sealed class ToolArguments
        {
            public string Command { get; set; }
            public ushort AbilityId { get; set; }
            public string OutputRoot { get; set; }
            public string ExtractedRootPath { get; set; }
            public string RemainingWorkAreaKey { get; set; }
            public string RemainingWorkMinimumPriorityBucket { get; set; }
            public string RemainingWorkSearchText { get; set; }
            public int? RemainingWorkTopCount { get; set; }
            public string QueryText { get; set; }
            public long QueryId { get; set; }
            public int QueryLimit { get; set; }
            public bool ShowUsage { get; set; }
        }

        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (Exception exception)
            {
                ConsoleManager.EnsureConsole();
                Console.Error.WriteLine(exception.GetType().Name + ": " + exception.Message);
                Console.Error.WriteLine(exception);
                return 1;
            }
        }

        private static int Run(string[] args)
        {
            ToolArguments toolArguments = ParseArguments(args);
            if (toolArguments == null)
                return 0;

            if (toolArguments.ShowUsage)
            {
                ConsoleManager.EnsureConsole();
                PrintUsage();
                return 0;
            }

            string outputRoot = Path.GetFullPath(toolArguments.OutputRoot ?? Path.Combine("docs", "data-matrix"));
            if (string.Equals(toolArguments.Command, "gui", StringComparison.OrdinalIgnoreCase))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(ExtractedDataRootResolver.Resolve(toolArguments.ExtractedRootPath), outputRoot));
                return 0;
            }

            ConsoleManager.EnsureConsole();

            if (string.Equals(toolArguments.Command, "clean_workspace", StringComparison.OrdinalIgnoreCase))
            {
                WorkspaceCleanupReport report = WorkspaceCleanupService.CleanWorkspace(Environment.CurrentDirectory);
                Console.WriteLine(report.BuildSummary());
                foreach (string removedPath in report.RemovedPaths)
                    Console.WriteLine("Removed: " + removedPath);
                foreach (string failedPath in report.FailedPaths)
                    Console.WriteLine("Failed: " + failedPath);
                return report.HasFailures ? 1 : 0;
            }
            if (string.Equals(toolArguments.Command, "export_index", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleManager.EnsureConsole();
                string indexPath = ClientIndexExporter.Write(toolArguments.ExtractedRootPath, outputRoot);
                Console.WriteLine("Client index written to " + indexPath);
                return 0;
            }

            // Queries read the client directly and want no ability dataset and no link analysis, so
            // they run before that load and stream tables one at a time.
            if (string.Equals(toolArguments.Command, "find", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleManager.EnsureConsole();
                List<ClientQueryService.Match> hits =
                    ClientQueryService.Find(toolArguments.ExtractedRootPath, toolArguments.QueryText,
                        toolArguments.QueryLimit == 0 ? int.MaxValue : toolArguments.QueryLimit);

                foreach (ClientQueryService.Match hit in hits)
                    Console.WriteLine(hit.RelativePath + "  [" + hit.Location + "]  " + hit.Content);

                Console.WriteLine(toolArguments.QueryLimit != 0 && hits.Count == toolArguments.QueryLimit
                    ? hits.Count + " matches (limit reached; raise --limit for more)"
                    : hits.Count + " match" + (hits.Count == 1 ? "" : "es"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "lookup", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleManager.EnsureConsole();
                List<ClientQueryService.Match> rows =
                    ClientQueryService.Lookup(toolArguments.ExtractedRootPath, toolArguments.QueryId);

                foreach (ClientQueryService.Match row in rows)
                    Console.WriteLine(row.RelativePath + "  [" + row.Location + "]  " + row.Content);

                Console.WriteLine(rows.Count + " keyed file" + (rows.Count == 1 ? "" : "s") + " hold id "
                    + toolArguments.QueryId.ToString(CultureInfo.InvariantCulture));
                return 0;
            }


            // Reads the client directly and needs none of the ability dataset, so it runs before
            // that load rather than paying for it.
            if (string.Equals(toolArguments.Command, "report_sources", StringComparison.OrdinalIgnoreCase))
            {
                string sourcesDirectory = ClientSourceReport.Write(toolArguments.ExtractedRootPath, outputRoot);
                Console.WriteLine("Client data inventory written to " + sourcesDirectory);
                return 0;
            }

            MatrixAnalysisSession session = MatrixAnalysisSession.Load(toolArguments.ExtractedRootPath);

            if (string.Equals(toolArguments.Command, "doctor_ability", StringComparison.OrdinalIgnoreCase)
                || string.Equals(toolArguments.Command, "export_graph_ability", StringComparison.OrdinalIgnoreCase))
            {
                AbilityAnalysisResult report = session.BuildAbilityAnalysis(toolArguments.AbilityId);
                session.WriteAbilityReport(outputRoot, report);
                Console.WriteLine("Ability report written to " + Path.Combine(outputRoot, "ability"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "ability", toolArguments.AbilityId.ToString(CultureInfo.InvariantCulture) + ".md"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_conflicts", StringComparison.OrdinalIgnoreCase))
            {
                ConflictReportDocument report = session.BuildConflictReport();
                session.WriteConflictReport(outputRoot, report);
                Console.WriteLine("Conflict report written to " + Path.Combine(outputRoot, "conflicts"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "conflicts", "client-conflicts.md"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_coverage", StringComparison.OrdinalIgnoreCase))
            {
                CoverageReportDocument report = session.BuildCoverageReport();
                session.WriteCoverageReport(outputRoot, report);
                Console.WriteLine("Coverage report written to " + Path.Combine(outputRoot, "coverage"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "coverage", "ability-coverage.md"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_domains", StringComparison.OrdinalIgnoreCase))
            {
                DomainLedgerDocument report = session.BuildDomainLedger();
                session.WriteDomainLedgerReport(outputRoot, report);
                Console.WriteLine("Domain ledger written to " + Path.Combine(outputRoot, "reference"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "reference", "core-identity-domains.md"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_requirements", StringComparison.OrdinalIgnoreCase))
            {
                RequirementLedgerDocument report = session.BuildRequirementLedger();
                session.WriteRequirementLedgerReport(outputRoot, report);
                Console.WriteLine("Requirement ledger written to " + Path.Combine(outputRoot, "reference"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "reference", "requirement-ledger.md"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_tokens", StringComparison.OrdinalIgnoreCase))
            {
                TokenDictionaryDocument report = session.BuildTokenDictionary();
                session.WriteTokenDictionaryReport(outputRoot, report);
                Console.WriteLine("Token dictionary written to " + Path.Combine(outputRoot, "reference"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "reference", "com-token-dictionary.md"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_operations", StringComparison.OrdinalIgnoreCase))
            {
                OperationSchemaDocument report = session.BuildOperationSchemas();
                session.WriteOperationSchemaReport(outputRoot, report);
                Console.WriteLine("Operation schema report written to " + Path.Combine(outputRoot, "reference"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "reference", "component-operation-schemas.md"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_remaining", StringComparison.OrdinalIgnoreCase))
            {
                OperationSchemaDocument operations = session.BuildOperationSchemas();
                RequirementLedgerDocument requirements = session.BuildRequirementLedger();
                RemainingWorkDocument report = session.BuildRemainingWorkReport(
                    session.BuildCoverageReport(),
                    session.BuildConflictReport(),
                    session.BuildDomainLedger(),
                    requirements,
                    session.BuildTokenDictionary(),
                    operations);
                OperationFieldWorkPacketDocument packetReport = session.BuildOperationFieldWorkPackets(
                    report,
                    operations,
                    OperationFieldWorkPacketCatalog.DefaultPacketCount,
                    RemainingWorkCatalog.DefaultNextBatchMinimumPriorityBucket,
                    null);
                ControlLiteralCrosswalkDocument literalReport = session.BuildControlLiteralCrosswalk(
                    requirements,
                    ControlLiteralCrosswalkCatalog.DefaultLiteralCount,
                    null);
                session.WriteRemainingWorkReport(outputRoot, report);
                session.WriteOperationFieldWorkPacketReport(outputRoot, packetReport);
                session.WriteControlLiteralCrosswalkReport(outputRoot, literalReport);
                Console.WriteLine("Remaining-work report written to " + Path.Combine(outputRoot, "overview"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "overview", "remaining-work.md"));
                Console.WriteLine("Next-batch markdown: " + Path.Combine(outputRoot, "overview", "remaining-work-next.md"));
                Console.WriteLine("Operation packet markdown: " + Path.Combine(outputRoot, "overview", "remaining-work-operation-fields.md"));
                Console.WriteLine("Control literal markdown: " + Path.Combine(outputRoot, "overview", "remaining-work-control-literals.md"));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_remaining_next", StringComparison.OrdinalIgnoreCase))
            {
                RemainingWorkDocument report = session.BuildRemainingWorkReport();
                bool useDefaultNextBatch = string.IsNullOrWhiteSpace(toolArguments.RemainingWorkAreaKey)
                    && string.IsNullOrWhiteSpace(toolArguments.RemainingWorkMinimumPriorityBucket)
                    && string.IsNullOrWhiteSpace(toolArguments.RemainingWorkSearchText)
                    && !toolArguments.RemainingWorkTopCount.HasValue;
                string minimumPriorityBucket = useDefaultNextBatch
                    ? RemainingWorkCatalog.DefaultNextBatchMinimumPriorityBucket
                    : toolArguments.RemainingWorkMinimumPriorityBucket;
                int topCount = useDefaultNextBatch
                    ? RemainingWorkCatalog.DefaultNextBatchTopCount
                    : toolArguments.RemainingWorkTopCount.GetValueOrDefault();
                List<RemainingWorkItemRecord> items = RemainingWorkCatalog.FilterItems(
                    report,
                    toolArguments.RemainingWorkAreaKey,
                    minimumPriorityBucket,
                    toolArguments.RemainingWorkSearchText,
                    topCount);
                string resolvedOutputRoot = session.ResolveOutputRoot(outputRoot);
                string fileStem = useDefaultNextBatch ? "remaining-work-next" : "remaining-work-focus";
                string title = useDefaultNextBatch ? "Remaining Work Next Batch" : "Remaining Work Focus";
                string filterDescription = RemainingWorkCatalog.BuildFilterDescription(
                    toolArguments.RemainingWorkAreaKey,
                    minimumPriorityBucket,
                    toolArguments.RemainingWorkSearchText,
                    topCount);
                ReportWriters.WriteRemainingWorkFocusReport(resolvedOutputRoot, report, fileStem, items, title, filterDescription);
                Console.WriteLine("Remaining-work focus report written to " + Path.Combine(outputRoot, "overview"));
                Console.WriteLine("Primary markdown: " + Path.Combine(outputRoot, "overview", fileStem + ".md"));
                Console.WriteLine("Matching items: " + items.Count.ToString(CultureInfo.InvariantCulture));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_remaining_packets", StringComparison.OrdinalIgnoreCase))
            {
                OperationSchemaDocument operations = session.BuildOperationSchemas();
                RemainingWorkDocument report = session.BuildRemainingWorkReport(
                    session.BuildCoverageReport(),
                    session.BuildConflictReport(),
                    session.BuildDomainLedger(),
                    session.BuildRequirementLedger(),
                    session.BuildTokenDictionary(),
                    operations);
                string minimumPriorityBucket = string.IsNullOrWhiteSpace(toolArguments.RemainingWorkMinimumPriorityBucket)
                    ? RemainingWorkCatalog.DefaultNextBatchMinimumPriorityBucket
                    : toolArguments.RemainingWorkMinimumPriorityBucket;
                int topCount = toolArguments.RemainingWorkTopCount.HasValue && toolArguments.RemainingWorkTopCount.Value > 0
                    ? toolArguments.RemainingWorkTopCount.Value
                    : OperationFieldWorkPacketCatalog.DefaultPacketCount;
                OperationFieldWorkPacketDocument packetReport = session.BuildOperationFieldWorkPackets(
                    report,
                    operations,
                    topCount,
                    minimumPriorityBucket,
                    toolArguments.RemainingWorkSearchText);
                string packetPath = session.WriteOperationFieldWorkPacketReport(outputRoot, packetReport);
                Console.WriteLine("Operation-field packet report written to " + Path.Combine(outputRoot, "overview"));
                Console.WriteLine("Primary markdown: " + packetPath);
                Console.WriteLine("Packet count: " + (packetReport.Packets == null ? 0 : packetReport.Packets.Count).ToString(CultureInfo.InvariantCulture));
                return 0;
            }

            if (string.Equals(toolArguments.Command, "report_remaining_literals", StringComparison.OrdinalIgnoreCase))
            {
                RequirementLedgerDocument requirements = session.BuildRequirementLedger();
                int topCount = toolArguments.RemainingWorkTopCount.HasValue
                    ? toolArguments.RemainingWorkTopCount.Value
                    : ControlLiteralCrosswalkCatalog.DefaultLiteralCount;
                ControlLiteralCrosswalkDocument report = session.BuildControlLiteralCrosswalk(
                    requirements,
                    topCount,
                    toolArguments.RemainingWorkSearchText);
                string reportPath = session.WriteControlLiteralCrosswalkReport(outputRoot, report);
                Console.WriteLine("Control literal crosswalk written to " + Path.Combine(outputRoot, "overview"));
                Console.WriteLine("Primary markdown: " + reportPath);
                Console.WriteLine("Literal count: " + (report.Literals == null ? 0 : report.Literals.Count).ToString(CultureInfo.InvariantCulture));
                return 0;
            }

            PrintUsage();
            return 1;
        }

        private static ToolArguments ParseArguments(string[] args)
        {
            args = args ?? new string[0];
            List<string> positionalArguments = new List<string>();
            ToolArguments parsedArguments = new ToolArguments
            {
                OutputRoot = Path.Combine("docs", "data-matrix"),

                // A search across 8,499 files can match thousands of rows. Capped by default so a
                // careless term prints a screenful rather than a session's worth of scrollback;
                // --limit 0 lifts it.
                QueryLimit = 200
            };

            for (int index = 0; index < args.Length; ++index)
            {
                string currentArgument = args[index];
                if (string.Equals(currentArgument, "--output", StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Length)
                        throw new ArgumentException("Missing value for --output.");
                    parsedArguments.OutputRoot = args[++index];
                    continue;
                }

                if (string.Equals(currentArgument, "--root", StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Length)
                        throw new ArgumentException("Missing value for --root.");
                    parsedArguments.ExtractedRootPath = args[++index];
                    continue;
                }

                if (string.Equals(currentArgument, "--help", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(currentArgument, "-h", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(currentArgument, "/?", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArguments.ShowUsage = true;
                    return parsedArguments;
                }

                if (string.Equals(currentArgument, "--area", StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Length)
                        throw new ArgumentException("Missing value for --area.");
                    parsedArguments.RemainingWorkAreaKey = args[++index];
                    continue;
                }

                if (string.Equals(currentArgument, "--priority", StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Length)
                        throw new ArgumentException("Missing value for --priority.");
                    parsedArguments.RemainingWorkMinimumPriorityBucket = args[++index];
                    continue;
                }

                if (string.Equals(currentArgument, "--search", StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Length)
                        throw new ArgumentException("Missing value for --search.");
                    parsedArguments.RemainingWorkSearchText = args[++index];
                    continue;
                }

                if (string.Equals(currentArgument, "--limit", StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Length)
                        throw new ArgumentException("Missing value for --limit.");
                    parsedArguments.QueryLimit = ParseTopCount(args[++index]);
                    continue;
                }

                if (string.Equals(currentArgument, "--top", StringComparison.OrdinalIgnoreCase))
                {
                    if (index + 1 >= args.Length)
                        throw new ArgumentException("Missing value for --top.");
                    parsedArguments.RemainingWorkTopCount = ParseTopCount(args[++index]);
                    continue;
                }

                positionalArguments.Add(currentArgument);
            }

            if (positionalArguments.Count == 0)
            {
                parsedArguments.Command = "gui";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 1
                && string.Equals(positionalArguments[0], "clean", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "clean_workspace";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 1
                && string.Equals(positionalArguments[0], "gui", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "gui";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 3
                && string.Equals(positionalArguments[0], "doctor", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "ability", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "doctor_ability";
                parsedArguments.AbilityId = ParseAbilityId(positionalArguments[2]);
                return parsedArguments;
            }

            if (positionalArguments.Count >= 4
                && string.Equals(positionalArguments[0], "export", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "graph", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[2], "ability", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "export_graph_ability";
                parsedArguments.AbilityId = ParseAbilityId(positionalArguments[3]);
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "export", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "index", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "export_index";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "find", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "find";
                // Everything after the verb is the search text, so a phrase needs no quoting.
                parsedArguments.QueryText = string.Join(" ", positionalArguments.Skip(1));
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "lookup", StringComparison.OrdinalIgnoreCase))
            {
                long id;
                if (!long.TryParse(positionalArguments[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                    throw new ArgumentException("lookup takes an integer id.");

                parsedArguments.Command = "lookup";
                parsedArguments.QueryId = id;
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "report", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "sources", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "report_sources";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "report", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "conflicts", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "report_conflicts";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "report", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "coverage", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "report_coverage";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "report", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "domains", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "report_domains";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "report", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "requirements", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "report_requirements";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "report", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "tokens", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "report_tokens";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "report", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "operations", StringComparison.OrdinalIgnoreCase))
            {
                parsedArguments.Command = "report_operations";
                return parsedArguments;
            }

            if (positionalArguments.Count >= 2
                && string.Equals(positionalArguments[0], "report", StringComparison.OrdinalIgnoreCase)
                && string.Equals(positionalArguments[1], "remaining", StringComparison.OrdinalIgnoreCase))
            {
                if (positionalArguments.Count >= 3
                    && string.Equals(positionalArguments[2], "next", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArguments.Command = "report_remaining_next";
                    return parsedArguments;
                }

                if (positionalArguments.Count >= 3
                    && string.Equals(positionalArguments[2], "packets", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArguments.Command = "report_remaining_packets";
                    return parsedArguments;
                }

                if (positionalArguments.Count >= 3
                    && string.Equals(positionalArguments[2], "literals", StringComparison.OrdinalIgnoreCase))
                {
                    parsedArguments.Command = "report_remaining_literals";
                    return parsedArguments;
                }

                parsedArguments.Command = "report_remaining";
                return parsedArguments;
            }

            parsedArguments.ShowUsage = true;
            return parsedArguments;
        }

        private static ushort ParseAbilityId(string rawValue)
        {
            ushort abilityId;
            if (!ushort.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out abilityId))
                throw new ArgumentException("Ability id must be a valid unsigned 16-bit integer.");
            return abilityId;
        }

        private static int ParseTopCount(string rawValue)
        {
            int topCount;
            if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out topCount) || topCount < 0)
                throw new ArgumentException("--top must be a non-negative integer.");
            return topCount;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("ClientDataMatrix");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  ClientDataMatrix [gui] [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix clean");
            Console.WriteLine("  ClientDataMatrix doctor ability <abilityId> [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix export graph ability <abilityId> [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix export index [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix find <text> [--limit <count>] [--root <path>]");
            Console.WriteLine("  ClientDataMatrix lookup <id> [--root <path>]");
            Console.WriteLine("  ClientDataMatrix report sources [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report conflicts [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report coverage [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report domains [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report requirements [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report tokens [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report operations [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report remaining [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report remaining next [--area <area>] [--priority <bucket>] [--search <text>] [--top <count>] [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report remaining packets [--priority <bucket>] [--search <text>] [--top <count>] [--root <path>] [--output <path>]");
            Console.WriteLine("  ClientDataMatrix report remaining literals [--search <text>] [--top <count>] [--root <path>] [--output <path>]");
            Console.WriteLine();
            Console.WriteLine("Running without a command launches the GUI.");
            Console.WriteLine("Clean removes tmp-data-matrix* folders and local ClientDataMatrix log files from the workspace.");
            Console.WriteLine("Remaining-work focus filters use area keys like Coverage or Operations; priority buckets are Critical, High, Medium, or Low.");
            Console.WriteLine("--top 0 means keep every matching item.");
            Console.WriteLine("Default output path: docs\\data-matrix");
            Console.WriteLine("Default extracted root: C:\\Users\\Admin\\Pictures\\WAR_extracted");
        }
    }
}

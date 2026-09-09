using ClientDataMatrix.Configuration;
using ClientDataMatrix.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientDataMatrix.UI
{
    /// <summary>
    /// The ability crosswalk, with the reading it depends on explained on screen beside it.
    ///
    /// The explanation is a tab rather than a comment in the source because the numbers here will be
    /// used to change the game's balance. Anyone about to act on "our damage disagrees with the
    /// client" needs to be able to check *how* the client's number was arrived at, and to see the
    /// two corrections that took agreement from 58% to 93% — otherwise the report is an oracle.
    /// </summary>
    internal sealed class AbilityCrosswalkTab
    {
        private readonly Button _run = new Button { Text = "Run Ability Crosswalk", AutoSize = true };
        private readonly Button _openReport = new Button { Text = "Open Report", AutoSize = true, Enabled = false };
        private readonly ComboBox _kind = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 320,
            Margin = new Padding(12, 3, 0, 3)
        };
        private readonly TextBox _summary;
        private readonly DataGridView _grid;

        private string _root;
        private string _outputRoot;
        private string _reportDirectory;
        private List<AbilityCrosswalkService.AbilityFinding> _all =
            new List<AbilityCrosswalkService.AbilityFinding>();

        public TabPage Page { get; private set; }

        public AbilityCrosswalkTab()
        {
            Page = new TabPage("Ability Crosswalk");

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            actions.Controls.Add(_run);
            actions.Controls.Add(_openReport);
            actions.Controls.Add(new Label { Text = "Show", AutoSize = true, Margin = new Padding(16, 8, 0, 0) });
            _kind.Items.AddRange(new object[]
            {
                "All",
                "damage differs from client",
                "not comparable: tooltip quotes another ability",
                "not comparable: slot holds a reference"
            });
            _kind.SelectedIndex = 0;
            _kind.SelectedIndexChanged += (s, e) => ApplyFilter();
            actions.Controls.Add(_kind);

            _run.Click += async (s, e) => await RunAsync();
            _openReport.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(_reportDirectory) && Directory.Exists(_reportDirectory))
                    Process.Start(new ProcessStartInfo(_reportDirectory) { UseShellExecute = true });
            };

            var sub = new TabControl { Dock = DockStyle.Fill };

            var findingsPage = new TabPage("Findings");
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText
            };
            // Client first and labelled by its source, ours labelled as ours.
            _grid.Columns.Add(Column("Client name (abilitynames.txt)", "ClientName", 240));
            _grid.Columns.Add(Column("DB entry", "DatabaseEntry", 90));
            _grid.Columns.Add(Column("Kind", "Kind", 280));
            _grid.Columns.Add(Column("Token", "Token", 220));
            _grid.Columns.Add(Column("Client value", "ClientValue", 100));
            _grid.Columns.Add(Column("DB MinDamage", "DatabaseValue", 110));
            _grid.Columns.Add(Column("Detail", "Detail", 460));
            findingsPage.Controls.Add(_grid);
            sub.TabPages.Add(findingsPage);

            var summaryPage = new TabPage("Summary");
            _summary = ReadOnlyText();
            _summary.Text = "Run the crosswalk to compare every ability's client tooltip damage against ours.";
            summaryPage.Controls.Add(_summary);
            sub.TabPages.Add(summaryPage);

            var explainPage = new TabPage("How this works");
            TextBox explain = ReadOnlyText();
            explain.Text = Explanation();
            explainPage.Controls.Add(explain);
            sub.TabPages.Add(explainPage);

            layout.Controls.Add(actions, 0, 0);
            layout.Controls.Add(sub, 0, 1);
            Page.Controls.Add(layout);
        }

        public void Bind(string extractedRoot, string outputRoot)
        {
            _root = extractedRoot;
            _outputRoot = outputRoot;
        }

        private static DataGridViewTextBoxColumn Column(string header, string property, int width)
        {
            return new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                DataPropertyName = property,
                Width = width,
                SortMode = DataGridViewColumnSortMode.Automatic
            };
        }

        private static TextBox ReadOnlyText()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font(FontFamily.GenericMonospace, 9F)
            };
        }

        private async Task RunAsync()
        {
            _run.Enabled = false;
            _summary.Text = "Reading the client string tables and the ability component data...";

            try
            {
                string root = _root;
                string outputRoot = _outputRoot;
                AbilityCrosswalkService.Report report = null;
                string directory = null;

                await Task.Run(() =>
                {
                    string connection = WorldDatabaseLocator.Resolve(null);
                    report = new AbilityCrosswalkService(connection, root).Run();
                    directory = AbilityCrosswalkService.Write(report, outputRoot, 40);
                });

                _reportDirectory = directory;
                _all = report.Findings;
                _openReport.Enabled = directory != null;
                _summary.Text = Summarise(report);
                ApplyFilter();
            }
            catch (Exception exception)
            {
                _summary.Text = "Ability crosswalk failed: " + exception.Message;
            }
            finally
            {
                _run.Enabled = true;
            }
        }

        private static string Summarise(AbilityCrosswalkService.Report report)
        {
            var text = new System.Text.StringBuilder();
            text.AppendLine("DOES THE READING HOLD?  (tests the logic)");
            text.AppendLine("  client descriptions          " + N(report.AbilitiesWithDescriptions));
            text.AppendLine("  abilities with components    " + N(report.AbilitiesWithComponents));
            text.AppendLine("  damage tokens found          " + N(report.DamageTokensSeen));
            text.AppendLine("  resolved                     " + N(report.DamageTokensResolved)
                + "   (" + report.ResolutionRate.ToString("F2", CultureInfo.InvariantCulture) + "%)");
            text.AppendLine("  unresolved                   " + N(report.DamageTokensUnresolved));
            foreach (KeyValuePair<string, int> pair in report.UnresolvedReasons.OrderByDescending(p => p.Value))
                text.AppendLine("      " + pair.Key.PadRight(42) + N(pair.Value));
            text.AppendLine();
            text.AppendLine("DOES OUR DATA MATCH?  (tests the database)");
            text.AppendLine("  comparable abilities         " + N(report.ComparableAbilities));
            text.AppendLine("  agree with the client        " + N(report.Agreements)
                + "   (" + report.AgreementRate.ToString("F2", CultureInfo.InvariantCulture) + "%)");
            text.AppendLine("  disagree - candidate drift   " + N(report.Disagreements));
            text.AppendLine();
            text.AppendLine("EXCLUDED FROM THE COMPARISON (they are comparison errors, not data faults)");
            text.AppendLine("  tooltip quotes another ability   " + N(report.CrossReferenced));
            text.AppendLine("  value slot holds a reference     " + N(report.ReferenceLike));
            return text.ToString();
        }

        private static string N(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private void ApplyFilter()
        {
            string wanted = _kind.SelectedItem as string;
            IEnumerable<AbilityCrosswalkService.AbilityFinding> rows = _all;

            if (!string.IsNullOrEmpty(wanted) && wanted != "All")
                rows = rows.Where(f => f.Kind == wanted);

            _grid.DataSource = new BindingList<AbilityCrosswalkService.AbilityFinding>(
                rows.OrderBy(f => f.Kind).ThenBy(f => f.DatabaseEntry).ToList());
        }

        private static string Explanation()
        {
            return string.Join(Environment.NewLine, new[]
            {
"HOW AN ABILITY'S DAMAGE IS READ OUT OF THE CLIENT",
"",
"The client ships every ability's tooltip in data/strings/english/abilitydesc.txt, with the",
"numbers left as tokens. A token is a complete address:",
"",
"    { [ABIL_<abilityId>_]  COM_<componentIndex>  _  <field>  _  <meaning> }",
"",
"    componentIndex   indexes THAT ABILITY'S OWN ORDERED COMPONENT LIST -- not a component id",
"    field            VAL<n> for the nth entry of the component's Values array,",
"                     or a scalar: DURA (duration), FREQ (interval), RADI (radius)",
"    meaning          presentation only -- DAMAGE, SPIRITDAMAGE, TOD_DAMAGE, SECONDS, FEET",
"    ABIL_<id>        optional, addresses a DIFFERENT ability's components",
"",
"So no operation table is needed to find an ability's damage. The ability's own description",
"says which component and which slot.",
"",
"",
"WORKED EXAMPLE -- ability 7, Spine Fling",
"",
"    \"...dealing {COM_1_VAL0_DAMAGE} every second for {COM_0_DURA_SECONDS}.\"",
"",
"    ordered components   3301, 2",
"    COM_0 -> 3301        Duration 3000ms, Interval 1000ms  ->  'every second for 3 seconds'",
"    COM_1 -> 2           Values [15,0,0,0,0,0,0,0]         ->  15",
"    our MinDamage        15                                    match",
"",
"",
"TWO CORRECTIONS THAT MATTER, BOTH FOUND BY MEASURING",
"",
"1. THE MULTIPLIER IS PART OF THE VALUE.",
"   Each component has a Multipliers array beside Values. It is a percentage:",
"",
"       value = Values[slot] * Multipliers[slot] / 100",
"",
"   Ignoring it left 21 abilities off by exactly 4x, 15 by 2x, 10 by 6x, 8 by 8x. A tidy",
"   integer ratio across unrelated abilities is never balance drift -- it is a missing factor.",
"",
"       627 Sever Nerve   40 x 400% = 160    our MinDamage 160",
"       670 Mage Bolt     40 x 200% =  80    our MinDamage  80",
"       5   KABOOM!       50 x 110% =  55    our MinDamage  55",
"",
"   Applying it moved agreement from 58.62% to 86.21%.",
"",
"2. THE ORDERED LIST IS NOT THE SORTED LIST.",
"   Take the order from mythic_bin_ability.MythicComponentData, sorted by each entry's Index.",
"   Do NOT use the ability report's 'Related component IDs' line -- that is a SORTED set. For",
"   ability 7 it prints '2, 3301' while the real order is '3301, 2', so COM_0 resolves to the",
"   wrong component and every token above index 0 is silently wrong.",
"",
"   Ability 1 agrees under both readings. Validating on one ability proves nothing; validate",
"   on one whose sorted and ordered lists differ.",
"",
"",
"WHAT IS DELIBERATELY NOT COMPARED",
"",
"   Cross-reference tokens. 'Spine Fling' (392) renders ABIL_7_COM_1 -- the pet's damage --",
"   while our damage row for 392 is the player ability's own. Matching those compares two",
"   different abilities.",
"",
"   Reference-like values. Some operations put a component or ability id in Values[0] and the",
"   client follows it instead of printing it; ability 5's component 142 carries 3682. A",
"   four-figure 'damage' beside a two-digit MinDamage is that, not a balance change.",
"",
"   Excluding both moved agreement from 86.21% to 93.38%.",
"",
"",
"WHERE THE REMAINING UNRESOLVED TOKENS GO",
"",
"   46 of 1,414 damage tokens do not resolve. 5 belong to abilities with no component data",
"   imported at all. The other 41 are a single contiguous family -- abilities 7717-7756 plus",
"   15557, the blast potions -- which share a description template referencing COM_2 while our",
"   import holds only two components for them. Both are gaps in mythic_bin_ability, not faults",
"   in the reading: a wrong reading fails scattered across the dataset, not in one block of",
"   forty consecutive ids of the same potion line.",
"",
"",
"BEFORE YOU MIGRATE FROM THIS REPORT",
"",
"   A wrong Val slot silently rebalances the whole game and, unlike a wrong item name, nobody",
"   sees it in a tooltip. Read the disagreements one at a time. 93.38% agreement means the",
"   method is sound; it does not mean the remaining 6.62% are all our fault."
            });
        }
    }
}

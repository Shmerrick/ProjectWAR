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
        private const string AllItems = "All";

        private readonly Button _run = new Button { Text = "Run Ability Crosswalk", AutoSize = true };
        private readonly Button _openReport = new Button { Text = "Open Report", AutoSize = true, Enabled = false };
        private readonly ComboBox _field = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 140,
            Margin = new Padding(6, 3, 0, 3)
        };
        private readonly ComboBox _kind = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 320,
            Margin = new Padding(6, 3, 0, 3)
        };
        private readonly TextBox _summary;
        private readonly DataGridView _grid;

        private string _root;
        private string _outputRoot;
        private string _reportDirectory;
        private bool _resettingFilters;
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
            actions.Controls.Add(new Label { Text = "Column", AutoSize = true, Margin = new Padding(16, 8, 0, 0) });
            actions.Controls.Add(_field);
            actions.Controls.Add(new Label { Text = "Kind", AutoSize = true, Margin = new Padding(12, 8, 0, 0) });
            actions.Controls.Add(_kind);
            ResetFilters();
            _field.SelectedIndexChanged += (s, e) => ApplyFilter();
            _kind.SelectedIndexChanged += (s, e) => ApplyFilter();

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
            _grid.Columns.Add(Column("Client name (abilitynames.txt)", "ClientName", 220));
            _grid.Columns.Add(Column("DB entry", "DatabaseEntry", 80));
            _grid.Columns.Add(Column("Column", "Field", 100));
            _grid.Columns.Add(Column("Kind", "Kind", 260));
            _grid.Columns.Add(Column("Client source", "Token", 220));
            _grid.Columns.Add(Column("Client value", "ClientValue", 90));
            _grid.Columns.Add(Column("Our value", "DatabaseValue", 90));
            _grid.Columns.Add(Column("Detail", "Detail", 460));
            findingsPage.Controls.Add(_grid);
            sub.TabPages.Add(findingsPage);

            var summaryPage = new TabPage("Summary");
            _summary = ReadOnlyText();
            _summary.Text = "Run the crosswalk to compare every ability's client data against ours.";
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
            _summary.Text = "Reading the client ability files and the database...";

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
                ResetFilters();
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
                text.AppendLine("      " + pair.Key.PadRight(48) + N(pair.Value));
            text.AppendLine();
            text.AppendLine("DOES OUR DAMAGE MATCH?  (tests the database)");
            text.AppendLine("  comparable abilities         " + N(report.ComparableAbilities));
            text.AppendLine("  agree with the client        " + N(report.Agreements)
                + "   (" + report.AgreementRate.ToString("F2", CultureInfo.InvariantCulture) + "%)");
            text.AppendLine("  disagree - candidate drift   " + N(report.Disagreements));
            text.AppendLine();
            text.AppendLine("EXCLUDED FROM THE DAMAGE COMPARISON (comparison errors, not data faults)");
            text.AppendLine("  tooltip quotes another ability   " + N(report.CrossReferenced));
            text.AppendLine("  value slot holds a reference     " + N(report.ReferenceLike));
            text.AppendLine();
            text.AppendLine("DO OUR OTHER COLUMNS MATCH?  (differ = both sides carry a value)");
            text.AppendLine("  column         agree   differ   ours empty   client empty   unrepresentable   not comparable");
            foreach (AbilityCrosswalkService.FieldTally tally in report.Fields)
            {
                text.AppendLine("  " + tally.Field.PadRight(12) + Pad(tally.Agreements, 8) + Pad(tally.Disagreements, 9)
                    + Pad(tally.DatabaseEmpty, 13) + Pad(tally.ClientEmpty, 15) + Pad(tally.NotRepresentable, 18)
                    + Pad(tally.NotComparable, 17));
            }
            text.AppendLine("  " + N(report.DatabaseRowsWithoutClientRecord) + " of " + N(report.DatabaseAbilityRows)
                + " mythic_src_abilities rows have no client record and are not compared");
            text.AppendLine();
            text.AppendLine("IS THE TOOLKIT IMPORT FAITHFUL?  (mythic_bin_* against the client)");
            text.AppendLine("  component lists compared       " + N(report.ComponentListsCompared));
            text.AppendLine("  differ from the client         " + N(report.ComponentListsDiffer));
            text.AppendLine("  client lists the import lacks  " + N(report.ClientListsWithoutImport));
            text.AppendLine("  upgrade items compared         " + N(report.UpgradeItemsCompared));
            text.AppendLine("  differ from the client         " + N(report.UpgradeItemsDiffer));
            return text.ToString();
        }

        private static string N(int value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static string Pad(int value, int width)
        {
            return N(value).PadLeft(width);
        }

        /// <summary>Rebuilds both filter lists from the current findings, back at "All".</summary>
        private void ResetFilters()
        {
            _resettingFilters = true;
            try
            {
                Fill(_field, _all.Select(f => f.Field));
                Fill(_kind, _all.Select(f => f.Kind));
            }
            finally
            {
                _resettingFilters = false;
            }
        }

        private static void Fill(ComboBox box, IEnumerable<string> values)
        {
            box.Items.Clear();
            box.Items.Add(AllItems);
            foreach (string value in values
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(v => v, StringComparer.Ordinal))
                box.Items.Add(value);
            box.SelectedIndex = 0;
        }

        private void ApplyFilter()
        {
            if (_resettingFilters)
                return;

            string field = _field.SelectedItem as string;
            string kind = _kind.SelectedItem as string;
            IEnumerable<AbilityCrosswalkService.AbilityFinding> rows = _all;

            if (!string.IsNullOrEmpty(field) && field != AllItems)
                rows = rows.Where(f => f.Field == field);
            if (!string.IsNullOrEmpty(kind) && kind != AllItems)
                rows = rows.Where(f => f.Kind == kind);

            _grid.DataSource = new BindingList<AbilityCrosswalkService.AbilityFinding>(
                rows.OrderBy(f => f.Field).ThenBy(f => f.Kind).ThenBy(f => f.DatabaseEntry).ToList());
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
"Everything on the client side is read from the extracted client: abilityexport.bin,",
"abilitycomponentexport.bin, upgradetableexport.bin and the string tables. The toolkit's import",
"of those files (mythic_bin_ability, mythic_bin_abilityupgrade*) is checked, never used.",
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
"   Take the order from abilityexport.bin's ComponentIds, in slot order with empty slots skipped,",
"   and look each id up in abilitycomponentexport.bin. Do NOT use the ability report's 'Related",
"   component IDs' line -- that is a SORTED set. For ability 7 it prints '2, 3301' while the real",
"   order is '3301, 2', so COM_0 resolves to the wrong component and every token above index 0",
"   is silently wrong.",
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
"   46 of 1,414 damage tokens do not resolve. 5 belong to abilities the client lists no",
"   components for. The other 41 are a single contiguous family -- abilities 7717-7756 plus",
"   15557, the blast potions -- which share a description template referencing COM_2 while the",
"   client's own abilityexport.bin records hold only two components for them. That was once put",
"   down to gaps in mythic_bin_ability; reading the client directly shows the client has two as",
"   well, so the template names a component the records never carried. Either way it is not a",
"   fault in the reading: a wrong reading fails scattered across the dataset, not in one block of",
"   forty consecutive ids of the same potion line.",
"",
"",
"THE OTHER COLUMNS",
"",
"   Every mythic_src_abilities row with a client record is checked column by column, and each",
"   ability's first own DURA and FREQ token against mythic_src_buff_infos (buff Entry = ability",
"   id). The units differ, and the rules are measured rather than assumed:",
"",
"       CastTime     client ms          = ours ms",
"       Cooldown     client ms          = effective runtime milliseconds",
"       Range        client             = ours feet x 12",
"       ApCost       client             = ours",
"       EffectID     client             = ours",
"       CareerLine   ours (a bit mask)  = 1 << (client career line - 1)",
"       Duration     client DURA ms     = buff Duration seconds x 1000",
"       Interval     client FREQ ms     = buff Interval ms",
"       Channel      client FlagsRaw bit 22 = ours ChannelID set; where both channel,",
"                    the first timed component's Duration ms = ours ChannelDuration ms,",
"                    and client ChannelInterval ms = ours ChannelInterval ms",
"",
"   Only 'differ' -- both sides carry a value and they disagree -- is candidate drift. A value",
"   on one side and none on the other is listed separately because it is often a convention:",
"   the client binds the Squig pet abilities 6-9 to no career line while ours carry a mask.",
"   'Not representable' is a client value our column's unit cannot hold, such as a fractional-foot",
"   range in whole feet. Cooldowns retain milliseconds. A mask granting the client's career line among others is set",
"   aside as not comparable.",
"",
"   Not compared: MinRange (no client field is established), RADI (no mapping to our",
"   EffectRadius holds -- 268 of 384 tokens differ) and heal tokens (which damage_heals index a",
"   heal belongs to is unproven -- 48 equal, 37 differ, 20 have no row).",
"",
"",
"IS THE TOOLKIT IMPORT FAITHFUL?",
"",
"   mythic_bin_ability and mythic_bin_abilityupgradeentry are the toolkit's import of these same",
"   client files. They are checked against the client rather than trusted -- the component",
"   lists and a few upgrade-table items differ -- and anything read from the import inherits",
"   those differences. The summary gives the counts.",
"",
"",
"WHY MATCHING MinDamage IS NOT THE SAME AS MATCHING THE TOOLTIP",
"",
"   The tooltip is a contract with the player. It is rendered from client files, so it says what",
"   it says no matter what the server does. If it reads 500 and the hit lands for 400, the",
"   server is wrong by definition.",
"",
"   Our MinDamage is NOT the tooltip number -- it is a base the server scales by level",
"   (AbilityDamageInfo.cs):",
"",
"       MaxDamage == 0:  damage = ((level - 1) * LevelScalingFactor * MinDamage) + MinDamage",
"       MaxDamage  > 0:  damage = MinDamage + (MaxDamage - MinDamage) * ((level - 1) / 39)",
"",
"   The second branch is effectively dead: MaxDamage is NULL on 1,433 of 1,434 rows and the one",
"   exception equals MinDamage. DamageVariance is NULL on 1,401, so the random spread is a no-op",
"   too. The data is single-valued, which matches the client's one-value-plus-multiplier shape --",
"   the Min/Max columns are vestigial rather than actively wrong.",
"",
"   So at rank 40 the first branch gives 7.5x MinDamage, and everything rests on",
"   LevelScalingFactor.",
"",
"",
"THE LEVEL SCALING FACTOR IS PER-ABILITY, AND WE DEFAULT IT",
"",
"   The decode is already right. AbilityMgr.TryDecodeUpgradeRowScalar packs",
"   mythic_bin_abilityupgradeentry.V1 (low half) and V2 (high half) into a 32-bit float:",
"   43713/15914 gives exactly 0.166667, which is also the hardcoded DefaultLevelScalingFactor.",
"   0/16256 is 1.0; 0/16384 is 2.0. The table holds many distinct factors -- 1.0 (60 rows),",
"   0.8147, 0.7346, 0.6544, 0.5742, 0.5344, 0.4941, 0.2052 -- in blocks of 12-16.",
"",
"   What went wrong was downstream. The server's startup log before BUG-151's fix:",
"",
"       upgrades=70  ability_entries=211  applied_rows=121  applied_entries=86",
"       unresolved_rows=3940",
"",
"   121 damage rows got a per-ability scalar and 3,940 fell back to the default. BUG-151 now",
"   takes a scalar only from an entry whose V3 is 1 -- inferred to mean damage, not proven -- so",
"   a bin with no such entry keeps the default instead of installing an unrelated property's",
"   factor. What V3 enumerates, and whether bins map to abilities by EffectID then Entry, are",
"   still open; see docs/INTERNAL_BUG_TRACKER.md.",
"",
"   This is the most likely mechanism behind 'the tooltip says one number and the hit does",
"   another', and it is invisible to this report, which compares the unscaled base.",
"",
"",
"BEFORE YOU MIGRATE FROM THIS REPORT",
"",
"   A wrong Val slot silently rebalances the whole game and, unlike a wrong item name, nobody",
"   sees it in a tooltip. Read the disagreements one at a time. 93.38% agreement means the",
"   method is sound; it does not mean the remaining 6.62% are all our fault -- and it says",
"   nothing at all about whether the scaled damage a player actually takes is right."
            });
        }
    }
}

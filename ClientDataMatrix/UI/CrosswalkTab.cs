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
    /// The item crosswalk, on screen rather than only in a markdown file.
    ///
    /// The CLI wrote a report and that was enough for a machine reading it back; it is not enough
    /// for a person deciding what to repair, which needs sorting, filtering and the ability to see
    /// a row's icon next to the complaint about it. Same service underneath -- this adds no
    /// analysis of its own, so the grid and the report can never disagree.
    /// </summary>
    internal sealed class CrosswalkTab
    {
        private readonly Button _run = new Button { Text = "Run Crosswalk", AutoSize = true };
        private readonly Button _openReport = new Button { Text = "Open Report", AutoSize = true, Enabled = false };
        private readonly ComboBox _table = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 200,
            Margin = new Padding(12, 3, 0, 3)
        };
        private readonly ComboBox _severity = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 130,
            Margin = new Padding(12, 3, 0, 3)
        };
        private readonly TextBox _summary;
        private readonly DataGridView _grid;
        private readonly PictureBox _icon;
        private readonly Label _iconCaption = new Label { AutoSize = true, ForeColor = SystemColors.GrayText };

        private string _root;
        private string _outputRoot;
        private string _reportDirectory;
        private ClientItemArtService _art;
        private List<ItemCrosswalkService.Finding> _all = new List<ItemCrosswalkService.Finding>();
        private Bitmap _currentIcon;

        public TabPage Page { get; private set; }

        public CrosswalkTab()
        {
            Page = new TabPage("Crosswalk");

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            actions.Controls.Add(_run);
            actions.Controls.Add(_openReport);
            actions.Controls.Add(new Label { Text = "Table", AutoSize = true, Margin = new Padding(16, 8, 0, 0) });
            _table.Items.AddRange(new object[] { "mythic_src_item_infos", "item_infos" });
            _table.SelectedIndex = 0;
            actions.Controls.Add(_table);
            actions.Controls.Add(new Label { Text = "Show", AutoSize = true, Margin = new Padding(16, 8, 0, 0) });
            _severity.Items.AddRange(new object[] { "All", "Violation", "Hole", "Suspect" });
            _severity.SelectedIndex = 0;
            _severity.SelectedIndexChanged += (s, e) => ApplyFilter();
            actions.Controls.Add(_severity);

            _run.Click += async (s, e) => await RunAsync();
            _openReport.Click += (s, e) => Open(_reportDirectory);

            _summary = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true,
                ScrollBars = ScrollBars.Both, WordWrap = false,
                Font = new Font(FontFamily.GenericMonospace, 9F),
                Text = "Run the crosswalk to compare every item row against the client."
                    + Environment.NewLine
                    + "Violation = the client contradicts the row.  Hole = empty where the client could fill."
                    + Environment.NewLine
                    + "Suspect = looks self-damaged; item names have no client arbiter, so these are flagged, never rewritten."
            };

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoGenerateColumns = false,
                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Entry", DataPropertyName = "Entry", Width = 90 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Severity", DataPropertyName = "Severity", Width = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Kind", DataPropertyName = "Kind", Width = 190 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Name", DataPropertyName = "Name", Width = 230 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Detail", DataPropertyName = "Detail", Width = 520 });
            _grid.SelectionChanged += (s, e) => ShowIconForSelection();

            _icon = new PictureBox
            {
                Width = 160, Height = 160,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = SystemColors.ControlDark
            };
            var side = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Margin = new Padding(12, 0, 0, 0) };
            side.Controls.Add(new Label { Text = "Selected row's art", AutoSize = true });
            side.Controls.Add(_icon);
            side.Controls.Add(_iconCaption);

            layout.Controls.Add(actions, 0, 0);
            layout.SetColumnSpan(actions, 2);
            layout.Controls.Add(_summary, 0, 1);
            layout.SetColumnSpan(_summary, 2);
            layout.Controls.Add(_grid, 0, 2);
            layout.Controls.Add(side, 1, 2);

            Page.Controls.Add(layout);
        }

        public void Bind(string extractedRoot, string outputRoot)
        {
            _root = extractedRoot;
            _outputRoot = outputRoot;
            _art = null;
        }

        private async Task RunAsync()
        {
            _run.Enabled = false;
            _summary.Text = "Reading the client art tables and the world database...";

            try
            {
                string root = _root;
                string outputRoot = _outputRoot;
                string table = (string)_table.SelectedItem;

                ItemCrosswalkService.Report report = null;
                string directory = null;
                ClientItemArtService art = null;

                // Off the UI thread: this reads 88,727 rows and the whole art table, which freezes
                // the window for several seconds if run inline.
                await Task.Run(() =>
                {
                    art = new ClientItemArtService(root);
                    string connection = WorldDatabaseLocator.Resolve(null);
                    report = new ItemCrosswalkService(connection, art).Run(table, null);
                    directory = ItemCrosswalkService.Write(report, outputRoot, 40);
                });

                _art = art;
                _reportDirectory = directory;
                _all = report.Findings;

                var text = new System.Text.StringBuilder();
                text.AppendLine("Table            " + report.SourceTable);
                text.AppendLine("Items examined   " + report.ItemsExamined.ToString("N0", CultureInfo.InvariantCulture));
                text.AppendLine("Icons resolved   " + report.IconsResolved.ToString("N0", CultureInfo.InvariantCulture)
                    + "  (" + (100.0 * report.IconsResolved / Math.Max(1, report.ItemsExamined))
                        .ToString("F1", CultureInfo.InvariantCulture) + "%)");
                text.AppendLine("Findings         " + report.Findings.Count.ToString("N0", CultureInfo.InvariantCulture));
                text.AppendLine();

                foreach (IGrouping<ItemCrosswalkService.Severity, ItemCrosswalkService.Finding> bySeverity
                    in report.Findings.GroupBy(f => f.Severity).OrderBy(g => g.Key))
                {
                    foreach (IGrouping<string, ItemCrosswalkService.Finding> byKind
                        in bySeverity.GroupBy(f => f.Kind).OrderByDescending(g => g.Count()))
                    {
                        text.AppendLine(bySeverity.Key.ToString().PadRight(10) + " "
                            + byKind.Count().ToString("N0", CultureInfo.InvariantCulture).PadLeft(6) + "  " + byKind.Key);
                    }
                }

                _summary.Text = text.ToString();
                _openReport.Enabled = directory != null;
                ApplyFilter();
            }
            catch (Exception exception)
            {
                _summary.Text = "Crosswalk failed: " + exception.Message
                    + Environment.NewLine + Environment.NewLine
                    + "The world database connection is read from bin/Release/Configs/World.xml. "
                    + "Build the solution once if that file does not exist yet.";
            }
            finally
            {
                _run.Enabled = true;
            }
        }

        private void ApplyFilter()
        {
            string wanted = _severity.SelectedItem as string;
            IEnumerable<ItemCrosswalkService.Finding> rows = _all;

            if (!string.IsNullOrEmpty(wanted) && wanted != "All")
                rows = rows.Where(f => f.Severity.ToString() == wanted);

            _grid.DataSource = new BindingList<ItemCrosswalkService.Finding>(
                rows.OrderBy(f => f.Severity).ThenBy(f => f.Entry).ToList());
        }

        private void ShowIconForSelection()
        {
            if (_currentIcon != null)
            {
                _icon.Image = null;
                _currentIcon.Dispose();
                _currentIcon = null;
            }

            _iconCaption.Text = string.Empty;

            if (_art == null || _grid.CurrentRow == null)
                return;

            var finding = _grid.CurrentRow.DataBoundItem as ItemCrosswalkService.Finding;
            if (finding == null)
                return;

            // The finding does not carry the ModelId, so re-read it the same way the report did.
            // Cheap: one dictionary hit plus one file read.
            long modelId;
            if (!TryFindModelId(finding.Entry, out modelId))
            {
                _iconCaption.Text = "no ModelId";
                return;
            }

            ClientItemArtService.ItemArt art = _art.Resolve(modelId);
            if (art.TexturePath == null)
            {
                _iconCaption.Text = art.ObjectName ?? "no art";
                return;
            }

            string error;
            _currentIcon = DdsImage.TryLoad(art.TexturePath, out error);
            _icon.Image = _currentIcon;
            _iconCaption.Text = _currentIcon != null ? art.ObjectName : "decode failed";
        }

        private readonly Dictionary<long, long> _modelIdByEntry = new Dictionary<long, long>();

        private bool TryFindModelId(long entry, out long modelId)
        {
            if (_modelIdByEntry.TryGetValue(entry, out modelId))
                return modelId > 0;

            try
            {
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(WorldDatabaseLocator.Resolve(null)))
                {
                    connection.Open();
                    using (var command = new MySql.Data.MySqlClient.MySqlCommand(
                        "SELECT ModelId FROM " + (string)_table.SelectedItem + " WHERE Entry = @e", connection))
                    {
                        command.Parameters.AddWithValue("@e", entry);
                        object result = command.ExecuteScalar();
                        modelId = result == null ? 0 : Convert.ToInt64(result, CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception)
            {
                modelId = 0;
            }

            _modelIdByEntry[entry] = modelId;
            return modelId > 0;
        }

        private static void Open(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return;

            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}

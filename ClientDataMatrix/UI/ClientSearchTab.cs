using ClientDataMatrix.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientDataMatrix.UI
{
    /// <summary>
    /// Searching the client, and building the index that makes it fast.
    ///
    /// Both of these were CLI-only, which meant the answer to "what does the client call 8334?" was
    /// visible to whoever ran the exe from a terminal and to nobody else. They are the two questions
    /// the client actually gets asked, so they belong on screen.
    ///
    /// The index is the point of the Build button. `find` and `lookup` re-parse 8,499 files per
    /// question, roughly ten seconds each; the exported index answers the same question by grep in
    /// about seventy milliseconds. Build it once per session and search stops costing anything.
    /// </summary>
    internal sealed class ClientSearchTab
    {
        private readonly TextBox _query = new TextBox { Width = 320 };
        private readonly RadioButton _byText = new RadioButton { Text = "by text", AutoSize = true, Checked = true, Margin = new Padding(12, 6, 0, 0) };
        private readonly RadioButton _byId = new RadioButton { Text = "by id", AutoSize = true, Margin = new Padding(8, 6, 0, 0) };
        private readonly Button _search = new Button { Text = "Search", AutoSize = true, Margin = new Padding(12, 0, 0, 0) };
        private readonly Button _buildIndex = new Button { Text = "Build Index", AutoSize = true, Margin = new Padding(24, 0, 0, 0) };
        private readonly Label _indexState = new Label { AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(12, 8, 0, 0) };
        private readonly ListView _results;

        private string _root;
        private string _outputRoot;

        public TabPage Page { get; private set; }

        public ClientSearchTab()
        {
            Page = new TabPage("Client Search");

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            bar.Controls.Add(new Label { Text = "Find", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
            bar.Controls.Add(_query);
            bar.Controls.Add(_byText);
            bar.Controls.Add(_byId);
            bar.Controls.Add(_search);
            bar.Controls.Add(_buildIndex);
            bar.Controls.Add(_indexState);

            _search.Click += async (s, e) => await SearchAsync();
            _buildIndex.Click += async (s, e) => await BuildIndexAsync();
            _query.KeyDown += async (s, e) =>
            {
                if (e.KeyCode != Keys.Enter)
                    return;

                e.SuppressKeyPress = true;
                await SearchAsync();
            };

            _results = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font(FontFamily.GenericMonospace, 9F)
            };
            _results.Columns.Add("File", 380);
            _results.Columns.Add("Where", 150);
            _results.Columns.Add("Content", 900);

            layout.Controls.Add(bar, 0, 0);
            layout.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Margin = new Padding(0, 4, 0, 8),
                Text = "The client is the authority for abilities, zones, objectives and the Tome. "
                     + "It holds no item names at all — those live only in the packet captures."
            }, 0, 1);
            layout.Controls.Add(_results, 0, 2);

            Page.Controls.Add(layout);
        }

        public void Bind(string extractedRoot, string outputRoot)
        {
            _root = extractedRoot;
            _outputRoot = outputRoot;
            RefreshIndexState();
        }

        private string IndexPath
        {
            get
            {
                return string.IsNullOrWhiteSpace(_outputRoot)
                    ? null
                    : Path.Combine(_outputRoot, "client-sources", "client-index.tsv");
            }
        }

        private void RefreshIndexState()
        {
            string path = IndexPath;
            if (path != null && File.Exists(path))
            {
                var info = new FileInfo(path);
                _indexState.Text = "index: " + (info.Length / 1048576).ToString(CultureInfo.InvariantCulture)
                    + " MB, built " + info.LastWriteTime.ToString("g", CultureInfo.CurrentCulture);
            }
            else
            {
                _indexState.Text = "index: not built";
            }
        }

        private async Task BuildIndexAsync()
        {
            _buildIndex.Enabled = false;
            _indexState.Text = "index: building...";

            try
            {
                string root = _root;
                string outputRoot = _outputRoot;
                string written = null;
                await Task.Run(() => { written = ClientIndexExporter.Write(root, outputRoot); });
                RefreshIndexState();

                if (written != null)
                    _indexState.Text += "  (" + written + ")";
            }
            catch (Exception exception)
            {
                _indexState.Text = "index: failed — " + exception.Message;
            }
            finally
            {
                _buildIndex.Enabled = true;
            }
        }

        private async Task SearchAsync()
        {
            string text = (_query.Text ?? string.Empty).Trim();
            if (text.Length == 0)
                return;

            _search.Enabled = false;
            _results.Items.Clear();
            _results.Items.Add(new ListViewItem(new[] { "searching...", string.Empty, string.Empty }));

            try
            {
                string root = _root;
                bool byId = _byId.Checked;
                List<ClientQueryService.Match> hits = null;

                await Task.Run(() =>
                {
                    if (byId)
                    {
                        long id;
                        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                            hits = ClientQueryService.Lookup(root, id);
                    }
                    else
                    {
                        hits = ClientQueryService.Find(root, text, 400);
                    }
                });

                _results.Items.Clear();

                if (hits == null)
                {
                    _results.Items.Add(new ListViewItem(new[] { "Enter a whole number to search by id.", string.Empty, string.Empty }));
                    return;
                }

                foreach (ClientQueryService.Match hit in hits)
                    _results.Items.Add(new ListViewItem(new[] { hit.RelativePath, hit.Location, hit.Content }));

                if (hits.Count == 0)
                    _results.Items.Add(new ListViewItem(new[] { "no matches", string.Empty, string.Empty }));
            }
            catch (Exception exception)
            {
                _results.Items.Clear();
                _results.Items.Add(new ListViewItem(new[] { "failed", string.Empty, exception.Message }));
            }
            finally
            {
                _search.Enabled = true;
            }
        }
    }
}

using ClientDataMatrix.Services;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace ClientDataMatrix.UI
{
    /// <summary>
    /// Shows an item's client art: the icon as the game draws it, and the 3D asset it names.
    ///
    /// WHY IT TAKES A ModelId AND NOT AN ITEM ENTRY. The client has no item table -- no names, no
    /// stats, nothing keyed by our `Entry`. The only thing linking an item row to client art is
    /// `item_infos.ModelId`, which addresses `objects.csv`. So this panel is honest about what it
    /// is: an objects.csv browser. Type a ModelId and see what the client would draw. The crosswalk
    /// report is what maps those back to item entries in bulk.
    ///
    /// 3D is identified, not rendered, and that is not only a scoping preference. WAR-RE-Toolkit
    /// owns mesh work (`mesh-viewer`, `geom2fbx`, `geom2obj`, `xac2ms`), so world objects with a
    /// NIF number belong there. Worn armour is a harder case: it resolves through Figleaf, and the
    /// toolkit's own `RE_FINDINGS/world/figleaf_status.md` records Figleaf as "partially decoded and
    /// useful; not fully reverse-engineered", with its `FigureParts` table still carrying `Unk1a`,
    /// `Unk1ba`, `Unk2a` and an integer `Geometry` index rather than a mesh name. So worn-armour
    /// geometry cannot be shown by any tool in either repo today, and claiming otherwise here would
    /// mean re-deriving a format the toolkit has already spent real effort on.
    /// </summary>
    internal sealed class ItemArtTab
    {
        private readonly TextBox _modelIdBox = new TextBox { Width = 120 };
        private readonly PictureBox _iconBox;
        private readonly Label _iconCaption = new Label { AutoSize = true, ForeColor = SystemColors.GrayText };
        private readonly TextBox _detail;
        private readonly Label _summary = new Label { AutoSize = true, ForeColor = SystemColors.GrayText };

        private ClientItemArtService _art;
        private string _root;
        private Bitmap _current;

        public TabPage Page { get; private set; }

        public ItemArtTab()
        {
            Page = new TabPage("Item Art");

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var query = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
            query.Controls.Add(new Label
            {
                Text = "ModelId (item_infos.ModelId)",
                AutoSize = true,
                Margin = new Padding(0, 6, 8, 0)
            });
            query.Controls.Add(_modelIdBox);

            var show = new Button { Text = "Show", AutoSize = true, Margin = new Padding(8, 0, 0, 0) };
            show.Click += (s, e) => Show(_modelIdBox.Text);
            query.Controls.Add(show);
            _modelIdBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter)
                    return;

                e.SuppressKeyPress = true;
                Show(_modelIdBox.Text);
            };

            layout.Controls.Add(query, 0, 0);
            layout.SetColumnSpan(query, 2);
            layout.Controls.Add(_summary, 0, 1);
            layout.SetColumnSpan(_summary, 2);

            // The icons are 64x64. Drawn at 4x with nearest-neighbour so the pixels stay crisp
            // rather than being smeared by the default interpolation.
            _iconBox = new PictureBox
            {
                Width = 256,
                Height = 256,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = SystemColors.ControlDark
            };

            var left = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true };
            left.Controls.Add(_iconBox);
            left.Controls.Add(_iconCaption);
            layout.Controls.Add(left, 0, 2);

            _detail = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font(FontFamily.GenericMonospace, 9F),
                Margin = new Padding(12, 0, 0, 0)
            };
            layout.Controls.Add(_detail, 1, 2);

            Page.Controls.Add(layout);
        }

        /// <summary>Rebinds to a client root. Cheap enough to call on every Reload Data.</summary>
        public void Bind(string extractedRoot)
        {
            _root = extractedRoot;
            _art = null;
            _summary.Text = "Not loaded. Enter a ModelId to read the client art tables.";
        }

        private void Show(string modelIdText)
        {
            long modelId;
            if (!long.TryParse((modelIdText ?? string.Empty).Trim(), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out modelId))
            {
                _detail.Text = "Enter a numeric ModelId.";
                return;
            }

            try
            {
                if (_art == null)
                {
                    _art = new ClientItemArtService(_root);
                    _summary.Text = string.Format(CultureInfo.InvariantCulture,
                        "objects.csv: {0:N0} rows   icons.xml: {1:N0} icons   textures on disk: {2:N0}",
                        _art.ObjectCount, _art.IconCount, _art.TextureFileCount);
                }
            }
            catch (Exception exception)
            {
                _detail.Text = "Could not read the client art tables: " + exception.Message;
                return;
            }

            ClientItemArtService.ItemArt art = _art.Resolve(modelId);
            Render(art);
        }

        private void Render(ClientItemArtService.ItemArt art)
        {
            // Every line names the file the value came from, and the one value that is ours says
            // so. The client calls this art `tk_soultalisman_intelligence`; our table calls the
            // item "Omnipotent Myrmidon's Soul". Both are "the name" and neither should appear
            // here unattributed.
            var text = new System.Text.StringBuilder();
            text.AppendLine("DB  item_infos.ModelId      " + art.ModelId.ToString(CultureInfo.InvariantCulture));
            text.AppendLine("    objects.csv  ID          " + art.ModelId.ToString(CultureInfo.InvariantCulture));
            text.AppendLine("    objects.csv  name        " + (art.ObjectName ?? "(no row)"));
            text.AppendLine("    objects.csv  Icon #      " + (art.IconId >= 0
                ? art.IconId.ToString(CultureInfo.InvariantCulture) : "(blank)"));
            text.AppendLine("    icons.xml    texture     " + (art.TextureName ?? "(unresolved)"));
            text.AppendLine("    file on disk             " + (art.TexturePath ?? "(absent)"));
            text.AppendLine();
            text.AppendLine("The client has no item display name. objects.csv names the ART;");
            text.AppendLine("item display names exist only in our database and the packet captures.");
            text.AppendLine();
            text.AppendLine("3D asset");

            if (art.FigleafPart != null)
            {
                text.AppendLine("  Figleaf part " + art.FigleafPart);
                text.AppendLine("  Worn art, assembled by the character-art system rather than loaded");
                text.AppendLine("  as a standalone .nif. Open it with WAR-RE-Toolkit's mesh-viewer.");
            }
            else if (art.NifNumber != null)
            {
                text.AppendLine("  NIF #        " + art.NifNumber);
                text.AppendLine("  World object. Resolve through the toolkit's geom2fbx / mesh-viewer.");
            }
            else
            {
                text.AppendLine("  (this row names neither a NIF number nor a Figleaf part)");
            }

            if (art.Failure != null)
            {
                text.AppendLine();
                text.AppendLine("Stopped: " + art.Failure);
            }

            _detail.Text = text.ToString();

            if (_current != null)
            {
                _iconBox.Image = null;
                _current.Dispose();
                _current = null;
            }

            if (art.TexturePath == null)
            {
                _iconCaption.Text = "no icon";
                return;
            }

            string error;
            _current = DdsImage.TryLoad(art.TexturePath, out error);
            _iconBox.Image = _current;
            _iconCaption.Text = _current != null
                ? art.TextureName + "  " + _current.Width + "x" + _current.Height
                : "could not decode: " + error;
        }
    }
}

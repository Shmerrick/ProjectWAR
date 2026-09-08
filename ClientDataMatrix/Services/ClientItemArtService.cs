using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ClientDataMatrix.Services
{
    /// <summary>
    /// Resolves an item's client art: the icon texture, and the 3D asset it is drawn from.
    ///
    /// THE CHAIN, AND THE WRONG TURN NEXT TO IT. There are two "icon" columns in the client and only
    /// one of them belongs to an item:
    ///
    ///     item_infos.ModelId -> data/gamedata/objects.csv, column "ID"
    ///                        -> that row's column 3, "Icon #"
    ///                        -> interface/default/eatemplate_icons/source/icons.xml
    ///                        -> texture -> eatemplate_icons/textures/&lt;name&gt;.dds
    ///
    /// 87,396 of 88,727 items resolve the whole way, and the texture name matches the object name --
    /// 8334 is `tk_soultalisman_intelligence` and its icon is `tk_soultalisman_intelligence.dds`.
    ///
    /// The wrong turn is `data/gamedata/itemdata.csv`, which has an `icon` column of its own. That is
    /// a different id space; it lands inside icons.xml only by coincidence and yields nonsense --
    /// "Greataxe of Chaotic Torsion" resolves to `Or_TM_shield10`, and only 2,516 of 12,523 armour
    /// items reach an armour texture. WorldServer's bot editor API takes that path today (BUG-150).
    ///
    /// 3D is split by asset kind. World objects carry a numeric "NIF #"; worn armour and weapons
    /// instead carry a Figleaf part name in `Figpart/Nif_ColumnA` (`DW_Armor_IB_01_Body`), assembled
    /// by the character-art system rather than loaded as a standalone .nif. This class reports which
    /// kind an item is and the asset it names; it does not render geometry.
    /// </summary>
    public sealed class ClientItemArtService
    {
        private const int ObjectsColumnName = 1;
        private const int ObjectsColumnNif = 2;
        private const int ObjectsColumnIcon = 3;
        private const int ObjectsColumnFigleafPart = 4;

        private readonly string _root;
        private readonly Dictionary<long, string[]> _objects = new Dictionary<long, string[]>();
        private readonly Dictionary<long, string> _iconTextures = new Dictionary<long, string>();
        private readonly Dictionary<string, string> _texturePaths =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public sealed class ItemArt
        {
            /// <summary>The objects.csv row key, i.e. the item's ModelId.</summary>
            public long ModelId;

            /// <summary>The client's own name for the art, e.g. tk_soultalisman_intelligence.</summary>
            public string ObjectName;

            /// <summary>objects.csv "Icon #", or -1 when the row leaves it blank.</summary>
            public long IconId = -1;

            /// <summary>The texture icons.xml names for that icon, e.g. tk_soultalisman_intelligence.dds.</summary>
            public string TextureName;

            /// <summary>Absolute path to the .dds, when the file is actually present.</summary>
            public string TexturePath;

            /// <summary>objects.csv "NIF #" for world objects, or null.</summary>
            public string NifNumber;

            /// <summary>Figleaf part name for worn art, or null.</summary>
            public string FigleafPart;

            /// <summary>Why resolution stopped, when it did. Null on full success.</summary>
            public string Failure;

            public bool HasIcon { get { return TexturePath != null; } }
        }

        public ClientItemArtService(string extractedRoot)
        {
            if (string.IsNullOrWhiteSpace(extractedRoot))
                throw new ArgumentException("Extracted client root is required.", "extractedRoot");

            _root = extractedRoot;
            LoadObjects();
            LoadIcons();
            LoadTextureFiles();
        }

        public int ObjectCount { get { return _objects.Count; } }
        public int IconCount { get { return _iconTextures.Count; } }
        public int TextureFileCount { get { return _texturePaths.Count; } }

        public ItemArt Resolve(long modelId)
        {
            var art = new ItemArt { ModelId = modelId };

            string[] row;
            if (!_objects.TryGetValue(modelId, out row))
            {
                art.Failure = "ModelId " + modelId.ToString(CultureInfo.InvariantCulture)
                    + " is not in objects.csv";
                return art;
            }

            art.ObjectName = Cell(row, ObjectsColumnName);
            art.NifNumber = NullIfEmpty(Cell(row, ObjectsColumnNif));
            art.FigleafPart = NullIfEmpty(Cell(row, ObjectsColumnFigleafPart));

            string iconText = Cell(row, ObjectsColumnIcon);
            long iconId;
            if (!long.TryParse(iconText, NumberStyles.Integer, CultureInfo.InvariantCulture, out iconId))
            {
                art.Failure = "objects.csv row " + modelId.ToString(CultureInfo.InvariantCulture)
                    + " has no Icon #";
                return art;
            }

            art.IconId = iconId;

            string texture;
            if (!_iconTextures.TryGetValue(iconId, out texture))
            {
                art.Failure = "Icon " + iconId.ToString(CultureInfo.InvariantCulture)
                    + " is not declared in icons.xml";
                return art;
            }

            art.TextureName = texture;

            string path;
            if (!_texturePaths.TryGetValue(texture, out path))
            {
                art.Failure = "Texture " + texture + " is named by icons.xml but absent from the extraction";
                return art;
            }

            art.TexturePath = path;
            return art;
        }

        private void LoadObjects()
        {
            string path = Path.Combine(_root, "data", "gamedata", "objects.csv");
            if (!File.Exists(path))
                return;

            // Two header rows: a group banner ("Information,,NIF,Icon,...") then the real names
            // ("ID,name,#,#,Part,..."). Rows are taken on the key parsing as an integer rather than
            // on a row count, so a change in banner rows cannot silently drop the first item.
            foreach (string line in File.ReadLines(path))
            {
                string[] cells = line.Split(',');
                long id;
                if (cells.Length <= ObjectsColumnIcon
                    || !long.TryParse(cells[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                    continue;

                _objects[id] = cells;
            }
        }

        private void LoadIcons()
        {
            string path = Path.Combine(_root, "interface", "default", "eatemplate_icons", "source", "icons.xml");
            if (!File.Exists(path))
                return;

            System.Xml.Linq.XDocument document;
            try
            {
                document = System.Xml.Linq.XDocument.Load(path);
            }
            catch (System.Xml.XmlException)
            {
                return;
            }

            foreach (System.Xml.Linq.XElement element in document.Descendants())
            {
                if (!string.Equals(element.Name.LocalName, "Icon", StringComparison.OrdinalIgnoreCase))
                    continue;

                System.Xml.Linq.XAttribute idAttribute = element.Attribute("id");
                System.Xml.Linq.XAttribute textureAttribute = element.Attribute("texture");
                if (idAttribute == null || textureAttribute == null)
                    continue;

                long id;
                // ids are written zero-padded to five digits; parse rather than compare as text.
                if (!long.TryParse(idAttribute.Value.Trim(), NumberStyles.Integer,
                        CultureInfo.InvariantCulture, out id))
                    continue;

                string texture = textureAttribute.Value.Trim();
                int slash = texture.LastIndexOfAny(new[] { '/', '\\' });
                if (slash >= 0)
                    texture = texture.Substring(slash + 1);

                if (texture.Length > 0)
                    _iconTextures[id] = texture;
            }
        }

        private void LoadTextureFiles()
        {
            string directory = Path.Combine(_root, "interface", "default", "eatemplate_icons", "textures");
            if (!Directory.Exists(directory))
                return;

            // Indexed case-insensitively on purpose: icons.xml writes Dw_Armor_IB_01_Body.dds where
            // the file on disk is dw_armor_ib_01_body.dds, and the client does not care.
            foreach (string file in Directory.GetFiles(directory))
                _texturePaths[Path.GetFileName(file)] = file;
        }

        private static string Cell(string[] row, int index)
        {
            return index < row.Length ? row[index].Trim() : string.Empty;
        }

        private static string NullIfEmpty(string value)
        {
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
}

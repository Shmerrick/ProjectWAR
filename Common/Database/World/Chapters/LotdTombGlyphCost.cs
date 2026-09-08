using System;
using FrameWork;

namespace Common
{
    /// <summary>
    /// What a Land of the Dead tomb charges to enter: one row per glyph a tomb requires.
    ///
    /// Stored by glyph INDEX rather than by Tome entry, because each glyph exists twice -- 7960-7969
    /// for Destruction and 7970-7979 for Order, the same ten in the same order. An index is
    /// realm-agnostic, so one row covers both realms and there is no way to bind a tomb to the
    /// wrong realm's glyph by mistake.
    ///
    ///   0 Reed   1 Vulture   2 Scroll   3 Horse    4 Ankhra
    ///   5 Scarab 6 Vase      7 Riverbarge 8 Scorpion 9 Skull
    ///
    /// A tomb with no rows here charges nothing and is not gated, which is the state the server
    /// ships in: the real per-tomb costs are not established from the client or any capture, and
    /// inventing them would lock content on a guess. Populate this table and the gate turns itself
    /// on for that tomb.
    /// </summary>
    [DataTable(PreCache = true, TableName = "lotd_tomb_glyph_costs", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class LotdTombGlyphCost : DataObject
    {
        /// <summary>Destination zone of the tomb: 241 Stars, 242 Moon, 243 Sky, 244 Sun, 179 Vulture Lord.</summary>
        [PrimaryKey]
        public ushort TombZoneId { get; set; }

        /// <summary>0-9, indexing the glyph within its realm's block.</summary>
        [PrimaryKey]
        public byte GlyphIndex { get; set; }

        /// <summary>How many of that glyph the tomb costs. The Tome holds one of each, so this is 1.</summary>
        [DataElement(AllowDbNull = false)]
        public byte Count { get; set; }
    }
}

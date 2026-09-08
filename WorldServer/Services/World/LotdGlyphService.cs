using System.Collections.Generic;
using System.Text;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services;
using WorldServer.World.Objects;

namespace WorldServer.Services.World
{
    /// <summary>
    /// Land of the Dead glyphs, and what the tombs charge for them.
    ///
    /// The ten glyphs are Tome of Knowledge unlocks, not items, and exist twice -- 7960-7969 for
    /// Destruction and 7970-7979 for Order, the same ten in the same order. They are awarded by
    /// <c>pquest_objectives.TokCompleted</c> on a public quest's final stage.
    ///
    /// Retail gated the tombs on them: complete the public quests, and the tomb entrance spends the
    /// glyphs you earned and resets your glyph progress. This service is that rule. See
    /// docs/LOTD_GLYPHS_AND_TOMBS.md.
    /// </summary>
    [Service(typeof(TokService))]
    public class LotdGlyphService : ServiceBase
    {
        /// <summary>First glyph of each realm's block; the ten run consecutively from there.</summary>
        public const ushort DestructionGlyphBase = 7960;
        public const ushort OrderGlyphBase = 7970;
        public const byte GlyphCount = 10;

        /// <summary>Index order of the glyph blocks, for messages. Matches 7960..7969 exactly.</summary>
        private static readonly string[] GlyphNames =
        {
            "Reed", "Vulture", "Scroll", "Horse", "Ankhra",
            "Scarab", "Vase", "Riverbarge", "Scorpion", "Skull"
        };

        /// <summary>Tomb zone -> the glyph indices it charges. Empty until the costs are populated.</summary>
        private static readonly Dictionary<ushort, List<LotdTombGlyphCost>> CostsByTomb =
            new Dictionary<ushort, List<LotdTombGlyphCost>>();

        [LoadingFunction(true)]
        public static void LoadTombGlyphCosts()
        {
            CostsByTomb.Clear();

            IList<LotdTombGlyphCost> rows = Database.SelectAllObjects<LotdTombGlyphCost>();
            if (rows == null)
                return;

            foreach (LotdTombGlyphCost row in rows)
            {
                if (row.GlyphIndex >= GlyphCount)
                {
                    Log.Error("LotdGlyphService", "Tomb " + row.TombZoneId + " requires glyph index "
                        + row.GlyphIndex + ", which is outside 0-" + (GlyphCount - 1) + "; ignored.");
                    continue;
                }

                List<LotdTombGlyphCost> costs;
                if (!CostsByTomb.TryGetValue(row.TombZoneId, out costs))
                {
                    costs = new List<LotdTombGlyphCost>();
                    CostsByTomb.Add(row.TombZoneId, costs);
                }

                costs.Add(row);
            }

            Log.Info("LotdGlyphService", "Loaded glyph costs for " + CostsByTomb.Count + " tomb(s).");
        }

        /// <summary>The Tome entry for a glyph index in the player's realm.</summary>
        public static ushort GetGlyphEntry(byte glyphIndex, Realms realm)
        {
            ushort realmBase = realm == Realms.REALMS_REALM_ORDER ? OrderGlyphBase : DestructionGlyphBase;
            return (ushort)(realmBase + glyphIndex);
        }

        public static string GetGlyphName(byte glyphIndex)
        {
            return glyphIndex < GlyphNames.Length ? GlyphNames[glyphIndex] : glyphIndex.ToString();
        }

        /// <summary>True if this zone charges glyphs at all. A tomb with no rows is not gated.</summary>
        public static bool IsGatedTomb(ushort zoneId)
        {
            return CostsByTomb.ContainsKey(zoneId);
        }

        /// <summary>
        /// Whether <paramref name="player"/> may enter, and what they are missing if not.
        ///
        /// A tomb with no cost rows always passes. That is deliberate: the per-tomb costs are not
        /// established from the client or any capture, so the table ships empty and the tombs behave
        /// exactly as they did before until someone fills it in. Failing closed on absent data would
        /// lock every tomb on a guess.
        /// </summary>
        public static bool CanEnter(Player player, ushort tombZoneId, out string missingDescription)
        {
            missingDescription = null;

            List<LotdTombGlyphCost> costs;
            if (player == null || !CostsByTomb.TryGetValue(tombZoneId, out costs))
                return true;

            StringBuilder missing = null;

            for (int i = 0; i < costs.Count; ++i)
            {
                ushort entry = GetGlyphEntry(costs[i].GlyphIndex, player.Realm);
                if (player.TokInterface.HasTok(entry))
                    continue;

                if (missing == null)
                    missing = new StringBuilder();
                else
                    missing.Append(", ");

                missing.Append(GetGlyphName(costs[i].GlyphIndex));
            }

            if (missing == null)
                return true;

            missingDescription = missing.ToString();
            return false;
        }

        /// <summary>
        /// Spends the entry cost and clears the player's glyph progress.
        ///
        /// "The entrance of the tomb would cost the glyphs that the player had earned from the PQs
        /// and the players glyph progress would be reset" -- so this removes ALL ten of the player's
        /// realm, not only the ones the tomb charged. Both readings were available; the reset is the
        /// one the report describes, and it is also what makes the glyphs a repeatable currency
        /// rather than a one-time unlock. If that turns out to be wrong, narrowing it to the cost
        /// rows is a two-line change here.
        ///
        /// Returns how many glyphs were actually taken.
        /// </summary>
        public static int ConsumeGlyphs(Player player, ushort tombZoneId)
        {
            if (player == null || !CostsByTomb.ContainsKey(tombZoneId))
                return 0;

            int removed = 0;

            for (byte index = 0; index < GlyphCount; ++index)
            {
                if (player.TokInterface.RemoveTok(GetGlyphEntry(index, player.Realm)))
                    ++removed;
            }

            if (removed > 0)
            {
                Log.Info("LotdGlyphService", player.Name + " spent " + removed
                    + " glyph(s) entering tomb zone " + tombZoneId + ".");
            }

            return removed;
        }
    }
}

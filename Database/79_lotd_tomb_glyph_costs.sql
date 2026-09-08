-- 79_lotd_tomb_glyph_costs.sql
--
-- Creates the table that says what each Land of the Dead tomb charges to enter, and leaves it
-- empty.
--
-- WHAT THE GATE DOES. On a zone jump into a tomb, LotdGlyphService checks the player holds every
-- glyph the tomb lists. If not, the jump is refused and nothing is taken. If so, the player enters
-- and their glyph progress is reset -- all ten of their realm's glyphs are removed, not just the
-- ones this tomb charged, which is what "the players glyph progress would be reset" describes.
-- Glyphs are spent only after the instance has actually accepted the player, so a lockout or a
-- full instance cannot swallow them.
--
-- WHY IT IS EMPTY. Which glyphs each tomb costs is not established. It is not in zone_jumps, it is
-- not in any capture in the corpus, and it has not been found in the client. A tomb with no rows
-- here is not gated at all and behaves exactly as it did before this script, so the server ships
-- with the mechanism in place and the policy absent. Failing closed on data nobody has verified
-- would lock five instances on a guess.
--
-- HOW TO POPULATE IT. Glyphs are Tome entries, ten per realm in the same order -- 7960-7969 for
-- Destruction and 7970-7979 for Order -- so they are stored here by INDEX and resolved to the
-- player's realm at runtime. One row per glyph a tomb requires:
--
--   0 Reed        1 Vulture     2 Scroll      3 Horse       4 Ankhra
--   5 Scarab      6 Vase        7 Riverbarge  8 Scorpion    9 Skull
--
-- Tomb zones: 241 Tomb of the Stars, 242 Tomb of the Moon, 243 Tomb of the Sky,
--             244 Tomb of the Sun, 179 Tomb of the Vulture Lord.
--
-- So, for example, if the Tomb of the Sky costs the Vulture and Horse glyphs:
--
--   INSERT INTO lotd_tomb_glyph_costs (TombZoneId, GlyphIndex, Count) VALUES (243, 1, 1), (243, 3, 1);
--
-- The costs are cached at boot, so a restart is needed after changing them.
--
-- Note that until the missing public quest spawns are placed (BUG-134) only a few glyphs can
-- actually be earned, so any requirement set now will refuse most players. That is the honest
-- state of the content, not a fault in the gate.

CREATE TABLE IF NOT EXISTS `lotd_tomb_glyph_costs` (
    `TombZoneId` SMALLINT UNSIGNED NOT NULL COMMENT 'Destination zone: 241,242,243,244,179',
    `GlyphIndex` TINYINT UNSIGNED NOT NULL COMMENT '0-9, index within the realm glyph block',
    `Count`      TINYINT UNSIGNED NOT NULL DEFAULT 1,
    PRIMARY KEY (`TombZoneId`, `GlyphIndex`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

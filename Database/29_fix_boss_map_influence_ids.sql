-- 29_fix_boss_map_influence_ids.sql
--
-- Extends script 23 to Bastion Stair's four boss maps, which it missed.
--
-- Script 23 repointed zone_areas influence for zones 60 and 160 only. Zones 163-166 kept
-- OrderInfluenceId 129 / DestroInfluenceId 128 -- "Warcamp: Krung's Scrappin' Spot" and
-- "Chapter 20: Surprise Attack", both in zone 9, Nordland. They belong to the same dungeon and
-- must credit the same chapters: 6 for Order, 2 for Destruction.
--
-- Note zone 164 additionally carries a row with PieceId 0. When a zone has no areasNNN.png the
-- AreaPixels grid is all zeroes, so GetZoneAreaFor computes areaId 0 and matches that row --
-- which is why influence resolved in Lord Slaurith's map and nowhere else in Bastion Stair. That
-- accident is left in place: it is the only thing making CurrentArea resolve there, and removing
-- it would silently disable influence in that map until BUG-041 is addressed.
--
-- Idempotent: fixed assignment.

USE `war_world`;

UPDATE zone_areas SET OrderInfluenceId = 6, DestroInfluenceId = 2 WHERE ZoneId IN (163,164,165,166);

SELECT ZoneId, AreaName, PieceId, OrderInfluenceId, DestroInfluenceId
  FROM zone_areas WHERE ZoneId IN (160,163,164,165,166) ORDER BY ZoneId, PieceId;

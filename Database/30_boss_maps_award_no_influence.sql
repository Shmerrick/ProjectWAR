-- 30_boss_maps_award_no_influence.sql
--
-- Bastion Stair's instanced boss fights award no influence. Corrects script 29.
--
-- Video evidence of the live dungeon: influence accrued throughout the Bastion Stair map but
-- **not** inside the instanced boss fights. Script 29 had repointed zones 163-166 at the dungeon
-- chapters (6 Order / 2 Destruction) on the assumption they should credit the same track as the
-- main map. They should not credit anything.
--
-- Zeroing the influence ids is the correct expression of that: Player.AddInfluence returns
-- immediately on chapter 0, and Creature.GrantDungeonKillInfluence skips a zero id, so boss kills
-- award no influence while everything else about those rows -- respawn points, Tome explore
-- entry, area naming -- is untouched.
--
-- Zone 160, the dungeon proper, keeps 6 / 2 from script 23 and is unaffected.
--
-- Idempotent: fixed assignment.

USE `war_world`;

UPDATE zone_areas SET OrderInfluenceId = 0, DestroInfluenceId = 0 WHERE ZoneId IN (163,164,165,166);

SELECT ZoneId, AreaName, PieceId, OrderInfluenceId, DestroInfluenceId
  FROM zone_areas WHERE ZoneId IN (160,163,164,165,166) ORDER BY ZoneId, PieceId;

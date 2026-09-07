-- 61_move_tome_tactic_dummies_to_known_ground.sql
--
-- Migration 60 placed the tome tactic dummies at (…, 923187, 11264), about 1,600 units north of
-- the Dragonslayer Ridge rally point, chosen because that band had no other spawns. The Z was
-- taken from the rally point, but there is no creature anywhere near that line to corroborate it:
-- every real spawn in the area lies *south* of the rally point, at Z 11076-11187. An unverified Z
-- on unreferenced ground is how a spawn ends up under the terrain and invisible, which is what was
-- reported.
--
-- Moved to a tight line around the rally point itself (1423580, 921587, 11264). That point is
-- known-good walkable ground -- it is a bind point players rez at -- so Z 11264 is corroborated
-- there rather than extrapolated. The dummies sit 400-1,000 units from it, while the nearest real
-- creature spawn is 1,924 units away, so they still interfere with nothing.
--
-- Ten dummies 200 units apart along X at Y 921987, X 1422680..1424480.
--
-- Reach them with:  .teleport map 5 1423580 921987 11264
--
-- Re-runnable: plain UPDATEs keyed on entry.

START TRANSACTION;

UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1422680 WHERE `Entry` = 999500;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1422880 WHERE `Entry` = 999501;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1423080 WHERE `Entry` = 999502;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1423280 WHERE `Entry` = 999503;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1423480 WHERE `Entry` = 999504;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1423680 WHERE `Entry` = 999505;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1423880 WHERE `Entry` = 999506;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1424080 WHERE `Entry` = 999507;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1424280 WHERE `Entry` = 999508;
UPDATE `creature_spawns` SET `WorldY` = 921987, `WorldZ` = 11264, `WorldX` = 1424480 WHERE `Entry` = 999509;

COMMIT;

-- Verification:
--   SELECT Entry, WorldX, WorldY, WorldZ FROM creature_spawns WHERE Entry BETWEEN 999500 AND 999509 ORDER BY Entry;
--   -- distance from the Dragonslayer Ridge rally point should be 400-1000 for every row:
--   SELECT Entry, ROUND(SQRT(POW(WorldX-1423580,2)+POW(WorldY-921587,2))) dist
--     FROM creature_spawns WHERE Entry BETWEEN 999500 AND 999509 ORDER BY Entry;

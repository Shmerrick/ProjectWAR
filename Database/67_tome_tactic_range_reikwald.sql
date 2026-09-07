-- 67_tome_tactic_range_reikwald.sql
--
-- Moves the tome tactic test range to Reikwald (zone 110) at map pin 50244, 19322, and fixes the
-- reason none of the previous placements could be interacted with.
--
-- ROOT CAUSE. Every earlier placement used a WorldZ extrapolated from a distant reference. The
-- server can be asked instead: ClientFileMgr.GetHeight reads deps/zones/zone<id>/{offset,terrain}.png
-- as (offset.R * 31 + terrain.R) * 16 - 30, halved, indexed by pin >> 6. Evaluating that at the
-- Thunder Mountain position used by migrations 61-66 gives a true ground height of 9393, while
-- those rows carried Z 11264 -- the creatures were standing roughly 1,870 units in the air. That
-- matches every symptom: not visible (above the view), tab-targetable because the server's range
-- check is 3D-distance based, but not clickable and not attackable. Model, Flag, Icone and faction
-- were all genuine defects and were fixed, but Z is what kept the range unusable throughout.
--
-- Every Z below is computed from the height rasters at that exact pin, not copied from a neighbour,
-- and the ground slopes from 16449 down to 16105 across the line, so each creature gets its own.
--
--   pin X = worldX - (OffX * 4096) = worldX - 1409024      (zone 110 OffX 344)
--   pin Y = worldY - (OffY * 4096) = worldY -  950272      (zone 110 OffY 232)
--
-- Ten real creatures, one per tactic line plus a control, 200 units apart along X centred on the
-- requested pin. Prototypes are untouched; Faction 67 is set on the spawn rows only, which
-- Creature.cs prefers over the prototype's, so the world's other copies of these creatures are
-- unaffected.
--
-- Reach them with:  .teleport map 110 1459268 969594 16441
--
-- Re-runnable: clears both the Thunder Mountain range and any previous Reikwald run.

START TRANSACTION;

DELETE FROM `creature_spawns`
 WHERE `ZoneId` = 5 AND `WorldY` = 921987 AND `WorldZ` = 11264
   AND `WorldX` BETWEEN 1422680 AND 1424480;

DELETE FROM `creature_spawns`
 WHERE `ZoneId` = 110 AND `WorldY` = 969594 AND `WorldX` BETWEEN 1458368 AND 1460168;

INSERT INTO `creature_spawns`
 (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`Icone`,`Emote`,`Faction`,`WaypointType`,
  `Level`,`Ward`,`Oid`,`RespawnMinutes`,`Enabled`)
VALUES
 (  42814,110,1458368,969594,16393,2048,18,0,67,0,40,0,0,4,1), -- Daemonic  (type 10) Pestilent Tentacle
 (   2485,110,1458568,969594,16425,2048,18,0,67,0,40,0,0,4,1), -- Beastial  (type 1)  Lionmarch Hunter
 (   1703,110,1458768,969594,16449,2048,18,0,67,0,40,0,0,4,1), -- Giant     (type 21) Bubor
 (  20842,110,1458968,969594,16449,2048,18,0,67,0,40,0,0,4,1), -- Greenskin (type 15) Mush 'unta Squig
 (   1921,110,1459168,969594,16441,2048,18,0,67,0,40,0,0,4,1), -- Chaos     (type 11) Mottled Gor
 (  15688,110,1459368,969594,16401,2048,18,0,67,0,40,0,0,4,1), -- Mythical  (type 22) Sootwing Woodspirit
 (   6956,110,1459568,969594,16313,2048,18,0,67,0,40,0,0,4,1), -- Man       (type 16) Drakk Hellcat
 (   6947,110,1459768,969594,16185,2048,18,0,67,0,40,0,0,4,1), -- Skaven    (type 18) Burrowing Fiend
 (  33584,110,1459968,969594,16121,2048,18,0,67,0,40,0,0,4,1), -- Undead    (type 27) Skeletal Warrior
 (2000701,110,1460168,969594,16105,2048,18,0,67,0,40,0,0,4,1); -- Control   (type 0)  Skulltaker Rager

COMMIT;

-- Verification:
--   SELECT s.WorldX, s.WorldZ, s.Entry, p.Name, p.CreatureType, s.Faction
--     FROM creature_spawns s JOIN creature_protos p ON p.Entry = s.Entry
--    WHERE s.ZoneId = 110 ORDER BY s.WorldX;                                   -- 10 rows
--   SELECT COUNT(*) FROM creature_spawns WHERE ZoneId = 5 AND WorldY = 921987;  -- 0

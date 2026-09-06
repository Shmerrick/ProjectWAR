-- 51_archive_deleted_bastion_creature_placements.sql
-- Preserve the 24 unresolved creature placements deleted by migration 46.
-- Authority for archival recovery: untouched Database/war_world.7z, war_world.sql,
-- creature_spawns GUIDs listed verbatim below (entry 2000689, zone 163).
-- These are historical emulator records, NOT established 1.4.8 spawn identities or
-- geometry. No prototype, coordinate conversion, or live spawn is introduced.
-- All 24 source rows were already disabled (Enabled=0). Their invalid runtime
-- coordinates and missing prototype did not justify destroying the research record.
-- Migration 49 repaired pquest_spawns only and did not preserve these creatures.
--
-- Safe after either revision of migration 47; apply after migrations 46-50.
-- Explicit original columns preserve the base records; later ORM fields retain
-- the archive table's defaults. INSERT IGNORE preserves any already archived row.
-- No live table rows are inserted, updated, or deleted. Safe to re-run.
USE `war_world`;

CREATE TABLE IF NOT EXISTS `creature_spawns_unresolved` LIKE `creature_spawns`;

INSERT IGNORE INTO `creature_spawns_unresolved`
    (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`,
     `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`)
VALUES
(1081553,2000689,163,23368,141477,8500,1069,18,0,1,0,33,2826,4,0),
(1081554,2000689,163,22984,141500,8526,2867,18,0,1,0,33,2971,4,0),
(1081555,2000689,163,23469,141925,8496,159,18,0,1,0,33,2847,4,0),
(1081556,2000689,163,23491,142228,8484,1729,18,0,1,0,33,2957,4,0),
(1081557,2000689,163,24988,141230,8566,3913,18,0,1,0,33,2958,4,0),
(1081558,2000689,163,25222,141446,8560,1069,18,0,1,0,33,2960,4,0),
(1081559,2000689,163,24455,142888,8540,2241,18,0,1,0,33,2959,4,0),
(1081560,2000689,163,24584,142580,8508,193,18,0,1,0,33,2903,4,0),
(1081561,2000689,163,22744,144334,8886,3447,18,0,1,0,33,2966,4,0),
(1081562,2000689,163,23042,144481,8886,1035,18,0,1,0,33,2968,4,0),
(1081563,2000689,163,22523,143338,8886,3709,18,0,1,0,33,2967,4,0),
(1081564,2000689,163,22645,143587,8886,1763,18,0,1,0,33,2822,4,0),
(1081565,2000689,163,25486,143607,8886,3857,18,0,1,0,33,2905,4,0),
(1081566,2000689,163,25299,143713,8886,3242,18,0,1,0,33,2735,4,0),
(1081567,2000689,163,24642,141999,8498,750,18,0,1,0,33,2973,4,0),
(1081568,2000689,163,24381,141920,8484,3527,18,0,1,0,33,2824,4,0),
(1081569,2000689,163,23799,143147,8562,227,18,0,1,0,33,2763,4,0),
(1081570,2000689,163,23782,143353,8622,1956,18,0,1,0,33,2770,4,0),
(1081571,2000689,163,23902,141873,8500,3606,18,0,1,0,33,2771,4,0),
(1081572,2000689,163,24020,142078,8454,1820,18,0,1,0,33,2926,4,0),
(1081573,2000689,163,23933,142381,8440,3891,18,0,1,0,33,2930,4,0),
(1081574,2000689,163,24084,142558,8444,1251,18,0,1,0,33,2891,4,0),
(1081575,2000689,163,25262,144715,8886,1649,18,0,1,0,33,2795,4,0),
(1081576,2000689,163,25132,144535,8886,3697,18,0,1,0,33,2772,4,0);

-- Expect 24 archived disabled records, zero matching live records.
SELECT COUNT(*) AS archived_placements, SUM(Enabled = 0) AS disabled_placements
  FROM creature_spawns_unresolved WHERE Entry = 2000689 AND ZoneId = 163;
SELECT COUNT(*) AS live_placements
  FROM creature_spawns WHERE Entry = 2000689 AND ZoneId = 163;

-- Correct migration 47's historical always-zero INNER JOIN/COUNT(*) audit.
-- Scope to archived objectives: other quests can legitimately use world/script spawns.
SELECT o.Guid, o.Entry, o.Objective
  FROM pquest_objectives o
 WHERE o.Type = 2
   AND EXISTS (SELECT 1 FROM pquest_spawns_unresolved a WHERE a.Objective = o.Guid)
   AND NOT EXISTS (SELECT 1 FROM pquest_spawns s WHERE s.Objective = o.Guid);

-- 24_bastion_stair_realm_instance.sql
--
-- Makes Bastion Stair a realm-instanced dungeon, matching Mount Gunbad and the 1.4.8 design.
--
-- Bastion Stair was a realm-instanced public dungeon: uncapped, with each realm in its own copy
-- reached through its own portal, and only the wing bosses as six-man private instances. See
-- docs/BASTION_STAIR.md.
--
-- `15_restore_shared_bastion_stair.sql` made the base map a shared PvE zone instead, setting all
-- 18 of its entry jumps to Type 0 with no InstanceID. That premise was wrong. This script
-- supersedes that part of script 15; its other changes -- the boss-map routing and the spawn
-- re-enables -- stand.
--
-- Two changes are needed, and the second is what stops the conversion double-spawning.
--
-- 1. Entry jumps become Type 4 with InstanceID 160.
--
--    MovementHandlers routes Jump.Type 4-6 to InstanceMgr.ZoneIn, where type 4 is the realm
--    instance: one persistent copy per realm, uncapped, never closed while empty so its public
--    quests stay in cycle. Mount Gunbad already does exactly this -- all ten of its jumps are
--    Type 4 with InstanceID 60.
--
-- 2. The 195 duplicate `instance_creature_spawns` rows for zone 160 are removed.
--
--    A dungeon zone has Region = ZoneId, so an instance's RegionMgr carries the same region id as
--    the world region and loads the same cell data. Zone 160's population lives in
--    `creature_spawns` (650 rows) and is loaded that way, while `Instance.LoadSpawns` separately
--    loads `instance_creature_spawns`. All 195 instance rows are exact duplicates of world rows
--    -- same Entry and same WorldX/WorldY -- so once the zone is instanced every one of those 195
--    creatures would spawn twice. Mount Gunbad has no such overlap: its 550 instance rows are
--    the population and its 10 world rows are the entrance NPCs.
--
--    The world table is kept because it is the complete population; the instance table holds only
--    a 195-row subset of it.
--
-- Idempotent: the update assigns fixed values, and the delete matches only rows that duplicate a
-- world spawn, of which there will be none after the first run.

USE `war_world`;

UPDATE zone_jumps
   SET Type = 4,
       InstanceID = 160
 WHERE ZoneId = 160;

DELETE i
  FROM instance_creature_spawns i
  JOIN creature_spawns c
    ON c.ZoneId = i.ZoneId
   AND c.Entry  = i.Entry
   AND c.WorldX = i.WorldX
   AND c.WorldY = i.WorldY
 WHERE i.ZoneId = 160;

-- Verification.
SELECT
    (SELECT COUNT(*) FROM zone_jumps WHERE ZoneId = 160 AND Type = 4 AND InstanceID = 160) AS realm_jumps,
    (SELECT COUNT(*) FROM zone_jumps WHERE ZoneId = 160)                                   AS total_jumps,
    (SELECT COUNT(*) FROM instance_creature_spawns WHERE ZoneId = 160)                      AS instance_spawns_left,
    (SELECT COUNT(*) FROM creature_spawns WHERE ZoneId = 160)                               AS world_spawns_kept;

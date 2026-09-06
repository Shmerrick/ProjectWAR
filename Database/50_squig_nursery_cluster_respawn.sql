-- 50_squig_nursery_cluster_respawn.sql
--
-- Gives The Squig Nursery's small squigs the same 15-second respawn as the monstrous squigs they
-- are placed with, so a cleared cluster comes back as a cluster.
--
-- Background. Migration 44 added `pquest_objectives.RespawnSeconds` and set objective 2301
-- "Monstrous Squigs" to 15 at the user's direction. Its sibling objective 2302 "Swarmin' Lit'l
-- Squig" was left on the flat ten-minute dungeon default, which splits a pack that is placed as
-- one: the big squig returns in fifteen seconds and the small squigs around it stay dead for ten
-- minutes.
--
-- The clusters are real, and this is capture-verified rather than assumed. Parsing
-- INSTANCE_GUNBAD_PART1/PART2 for F_CREATE_MONSTER and reducing to one record per object id gives
-- 40 distinct monstrous squigs (Deathspewin' 38630, Warchargin' 38629, Spikestabba 38631) and 169
-- distinct small squigs (Swarmin' Lit'l Squig 38628 and its Squigling form). Thirteen of the 40
-- big squigs have between one and five small squigs within 300 units, at typical separations of
-- 37 to 280 units -- for example a Warchargin' Squig at client 50091,117358 with five smalls at
-- 88, 90, 149, 228 and 259 units, and a Deathspewin' Squig at 51180,116654 with four at 78, 85,
-- 102 and 223.
--
-- The database placement already matches that shape and is NOT changed here: of the 30 spawn rows
-- on objective 2301, eleven have at least one 2302 row within 300 units, against thirteen of 40
-- in the capture. Only the respawn timing was wrong.
--
-- Effect. Both objectives now respawn on 15 seconds, honoured by `PQuestCreature.SetRespawnTimer`
-- ahead of the dungeon default. Because a pack is killed within a few seconds, its members return
-- within a few seconds of each other. Note the timers are still per-creature and independent: this
-- is a shared interval, not a linked cluster that waits for every member to die before respawning
-- any of them. Nothing in `pquest_spawns` records cluster membership, so a true grouped respawn
-- would need a new grouping key rather than a data fix.
--
-- Idempotent: the assignment is absolute and the second run is a no-op.
--
-- USE `war_world`;

USE `war_world`;

UPDATE `pquest_objectives`
   SET `RespawnSeconds` = 15
 WHERE `Guid` = 2302 AND `Entry` = 514;

-- Verification: both Squig Nursery kill objectives on 15 seconds, with their spawn counts and
-- kill targets, plus the count of monstrous-squig spawns that have a small squig placed within
-- 300 units -- the clustering this migration is meant to preserve.
SELECT o.Guid, o.Objective, o.Count AS kills_needed, o.RespawnSeconds,
       (SELECT COUNT(*) FROM pquest_spawns s WHERE s.Objective = o.Guid) AS spawn_rows
  FROM pquest_objectives o
 WHERE o.Entry = 514 AND o.Guid IN (2301, 2302)
 ORDER BY o.Guid;

SELECT COUNT(*) AS big_spawns_with_a_small_within_300
  FROM pquest_spawns b
 WHERE b.Objective = 2301
   AND EXISTS (SELECT 1 FROM pquest_spawns s
                WHERE s.Objective = 2302
                  AND SQRT(POW(s.WorldX - b.WorldX, 2) + POW(s.WorldY - b.WorldY, 2)) <= 300);

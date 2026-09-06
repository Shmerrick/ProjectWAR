-- 45_restore_land_of_the_dead_objective_targets.sql
--
-- Gives the Land of the Dead public quests the creatures their kill objectives are supposed to
-- count, and restores one missing boss spawn.
--
-- Background. Most zone-191 kill objectives carry ObjectId 0, so nothing could ever satisfy them
-- and no Land of the Dead public quest could advance -- the reported "no PQ trackers show on
-- screen". An earlier note treated this as a missing population needing a large rebuild from
-- captures; measuring first showed that is not the case. Every creature named below already has
-- creature_spawns rows in zone 191, so with the crediting fix that lets an ordinary creature
-- report its death to an attached public quest, only the objective-to-creature link was missing.
-- No creature is invented here; the sole INSERT restores one boss that the capture shows and the
-- database does not have.
--
-- Authority: the nine per-quest official captures under
-- WAR-RE-Toolkit/libs/protocolservices/Packet Logs/ named "LAND O* THE DEAD ... PQ <quest>",
-- parsed for F_CREATE_MONSTER. Zone 191 is not an instance, so capture coordinates are world
-- coordinates directly -- confirmed against untouched rows: the Quayside Archer spawns at
-- 216741,1516644,6068 and 216993,1517989,6022 appear identically in the capture and in
-- creature_spawns.
--
-- Each mapping below is the creature that both (a) matches the objective's own wording and
-- (b) is actually present in that quest's capture, with its zone-191 spawn count given so the
-- claim can be rechecked against the objective's kill target.
--
-- USE `war_world`;

USE `war_world`;

-- Aerie of Death. Capture holds three carrion types in the quest area: Screeching (49 object
-- ids), Merciless (35), Carrion (5); zone spawns 61/58/88 against a 30-kill target.
UPDATE pquest_objectives SET ObjectId='93706', ObjectId2='93705', ObjectId3='93620' WHERE Guid=2407 AND (ObjectId='0' OR ObjectId IS NULL);
UPDATE pquest_objectives SET ObjectId='93699' WHERE Guid=2409 AND (ObjectId='0' OR ObjectId IS NULL); -- Kheiret^M, 1 spawn

-- Hall of the Heavens. "Celestial Images" are the two Image creatures, 16 object ids each in the
-- quest capture, 20 and 42 zone spawns against a 20-kill target.
UPDATE pquest_objectives SET ObjectId='93717', ObjectId2='93555' WHERE Guid=2417 AND (ObjectId='0' OR ObjectId IS NULL);

-- Forbidden Vaults. Invoker count matches the objective exactly (4 spawns, 4 kills).
UPDATE pquest_objectives SET ObjectId='93718' WHERE Guid=2421 AND (ObjectId='0' OR ObjectId IS NULL); -- Forbidden Invoker^m
UPDATE pquest_objectives SET ObjectId='93635' WHERE Guid=2423 AND (ObjectId='0' OR ObjectId IS NULL); -- Bahiti Net
UPDATE pquest_objectives SET ObjectId='93572' WHERE Guid=2419 AND (ObjectId='0' OR ObjectId IS NULL); -- Zandri Scarab^m, 54 spawns

-- The Quay of Seftu. Quayside Archer (10) plus Quayside Sandblade (20) is exactly the 30 the
-- objective asks for, and both appear in that quest's capture with those same counts.
UPDATE pquest_objectives SET ObjectId='99399', ObjectId2='99398' WHERE Guid=2424 AND (ObjectId='0' OR ObjectId IS NULL);
UPDATE pquest_objectives SET ObjectId='99714' WHERE Guid=2426 AND (ObjectId='0' OR ObjectId IS NULL); -- Gahije the Invincible^M

-- Temple of Ualatp / Amsu's Charge / named bosses, each a single zone-191 spawn.
UPDATE pquest_objectives SET ObjectId='97493' WHERE Guid=2429 AND (ObjectId='0' OR ObjectId IS NULL); -- Amen-Ser, Master of the Gate
UPDATE pquest_objectives SET ObjectId='93585' WHERE Guid=2446 AND (ObjectId='0' OR ObjectId IS NULL); -- Amsu^M
UPDATE pquest_objectives SET ObjectId='93667' WHERE Guid=2771 AND (ObjectId='0' OR ObjectId IS NULL); -- Dregg Stinkeye^M
UPDATE pquest_objectives SET ObjectId='93553' WHERE Guid=2768 AND (ObjectId='0' OR ObjectId IS NULL); -- Graven Goldbarrow^M

-- The Assault of Nekh Akhet, all realm copies. Gaunt Dunebow/Soldier/Cavalry are the three gaunt
-- types in that quest's capture (28/27/18 object ids), 42/40/25 zone spawns against 20 kills.
UPDATE pquest_objectives SET ObjectId='93703', ObjectId2='93696', ObjectId3='93691'
 WHERE Guid IN (2447,2449,2451,2453,2455,2456,2458) AND (ObjectId='0' OR ObjectId IS NULL);
UPDATE pquest_objectives SET ObjectId='93692'
 WHERE Guid IN (2448,2450,2452,2454,2457) AND (ObjectId='0' OR ObjectId IS NULL); -- Nekh Akhet^M, 6 spawns

-- Ricci's Raiders, all realm copies. The four Sakhmet dune types are the only Sakhmet creatures
-- in that quest's capture (10/8/8/9 object ids), 9/8/8/9 zone spawns, 34 against 30 kills.
UPDATE pquest_objectives SET ObjectId='93607', ObjectId2='93633', ObjectId3='93740', ObjectId4='93601'
 WHERE Guid IN (2433,2437,2441,2460,2464,2468,2472,2476) AND (ObjectId='0' OR ObjectId IS NULL);

-- Amsu's Charge, all realm copies. The three Withered types, 4/7/8 zone spawns against 20 kills.
UPDATE pquest_objectives SET ObjectId='93562', ObjectId2='99406', ObjectId3='93582'
 WHERE Guid IN (2431,2444,2445) AND (ObjectId='0' OR ObjectId IS NULL);

-- Gahije the Invincible has no spawn anywhere, so the objective above would still be
-- unsatisfiable. The Quay of Seftu capture sights him once, at world 216720,1515280,6192 facing
-- 113, level 41 -- 486 units from the nearest Quayside Sandblade, inside the quest's area. The
-- row copies the surrounding Quayside spawns' settings, which is the only guess here and is
-- confined to presentation fields, not position or identity.
DELETE FROM creature_spawns WHERE Guid = 900001;
INSERT INTO creature_spawns
    (Guid, Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, Icone, Emote, Faction, WaypointType,
     Level, Ward, Oid, RespawnMinutes, Enabled)
VALUES
    (900001, 99714, 191, 216720, 1515280, 6192, 113, 18, 0, 1, 0, 41, 0, 0, 4, 1);

-- Verification: every zone-191 kill objective, and how many spawns its named creatures have.
SELECT o.Guid, o.Entry AS pquest, o.Objective, o.Count AS needed,
       (SELECT COUNT(*) FROM creature_spawns c WHERE c.ZoneId = 191
         AND c.Entry IN (o.ObjectId, o.ObjectId2, o.ObjectId3, o.ObjectId4, o.ObjectId5, o.ObjectId6)) AS zone_spawns
  FROM pquest_objectives o
  JOIN pquest_info p ON p.Entry = o.Entry
 WHERE p.ZoneId = 191 AND o.Type = 2
 ORDER BY zone_spawns, o.Guid;

-- 38_restore_squig_nursery_objects_and_squig_credit.sql
--
-- Makes Mount Gunbad's "The Squig Nursery" (public quest 514) completable.
--
-- Three of its five objectives could not be finished:
--
--   2301 Monstrous Squigs      kill 50   -- only 16 of its 30 spawns counted
--   2303 Break Nursery Slime   kill 10   -- nothing spawned at all
--   2304 Foul Mouf da 'ungry   use GO 1  -- nothing spawned at all
--
-- Authority: the official 1.4.8 capture
-- WAR-RE-Toolkit/libs/protocolservices/Packet Logs/INSTANCE_GUNBAD_PART1.txt.gz.
-- Packet ordinals are 1-based across both directions and reproducible with
-- tools/validation/Read-OfficialPackets.ps1. F_CREATE_STATIC payload offsets read here,
-- taken from GameObject.SendMeTo: +0 OID, +2 VfxState, +4 heading, +6 Z, +8 client X,
-- +12 client Y, +16 DisplayID, +25 flags, +28 the uint32 this schema stores as Unk3, and
-- at +40 a Pascal string holding the name. This is the same reading migration 34 used to
-- restore the Holmsteinn supply prototype.
--
-- USE `war_world`;

USE `war_world`;

-- 1. The two missing gameobject prototypes.
--
--    pquest_spawns places 19 objects of prototype 100515 and one of 100516 in zone 60, but
--    neither prototype exists, so PQuestObjective logged "missing creature/gameobject
--    prototype" and spawned nothing. The user's session log shows exactly that: 19 x
--    "missing gameobject prototype 100515" and 1 x "missing gameobject prototype 100516".
--
--    Both were identified by position rather than by name guessing. Converting each spawn's
--    world coordinates into the capture's client frame for zone 60 -- OffX/OffY 200/200 with
--    the (1,9) instance atlas shift, world = client - (shift << 13) + (Off << 12) -- and
--    taking the nearest F_CREATE_STATIC sighting gives, for all 19 rows of 100515, the object
--    "Nursery Slime" with DisplayID 166 at a distance of 11 units, and for the single row of
--    100516, "Writhing Effigy" with DisplayID 148, also at 11 units. Nursery Slime is sighted
--    291 times in the capture and Writhing Effigy 4 times.
--
--    Unk3 is 100 on every sighting of both, which is the same value the capture gives for
--    Holmsteinn Supplies (prototype 551) and the same value carried by "Monastery Door" (59),
--    the one other prototype in this database used by a QUEST_KILL_GO objective. The
--    remaining columns follow that prototype rather than being invented: Level 1, Faction 0,
--    HealthPoints 1, IsAttackable 0. Per-spawn opaque data already lives on the pquest_spawns
--    rows, so UnksString is left at the neutral value entry 188 uses.
--
--    Idempotent: INSERT IGNORE, then a fixed assignment so a partially-present row is
--    corrected rather than skipped.

INSERT IGNORE INTO gameobject_protos
    (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock,
     Unk1, Unk2, Unk3, Unk4, UnksString, IsAttackable)
VALUES
    (100515, 'Nursery Slime',   166, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0),
    (100516, 'Writhing Effigy', 148, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0);

UPDATE gameobject_protos SET Name = 'Nursery Slime',   DisplayID = 166, Unk3 = 100 WHERE Entry = 100515;
UPDATE gameobject_protos SET Name = 'Writhing Effigy', DisplayID = 148, Unk3 = 100 WHERE Entry = 100516;

-- 2. Monstrous Squigs must credit the third squig it spawns.
--
--    Objective 2301 asks for 50 kills and credits ObjectId 38631 (Spikestabba Squig) and
--    ObjectId2 38629 (Warchargin' Squig). Its own spawn set is 7 Spikestabba, 9 Warchargin'
--    and 14 Deathspewin' Squig (38630) -- so the objective spawns 14 creatures for itself
--    that it then refuses to count, leaving 16 credited spawns for a 50-kill target. That is
--    the "about 20 monstrous squig spawns are missing" in the test report: they are present
--    and killable, they simply award nothing.
--
--    38630 is a Gunbad creature in its own right (107 sightings in the capture, level 26) and
--    is placed by this objective's own spawn set, which is the evidence for crediting it.
--    ObjectId3 already exists in the schema and is used by 55 other objectives.

UPDATE pquest_objectives
   SET ObjectId3 = '38630'
 WHERE Guid = 2301
   AND Entry = 514
   AND ObjectId = '38631'
   AND ObjectId2 = '38629';

-- Verification.
SELECT
    (SELECT COUNT(*) FROM gameobject_protos WHERE Entry IN (100515, 100516))                     AS nursery_protos_present,
    (SELECT COUNT(*) FROM pquest_spawns s LEFT JOIN gameobject_protos g ON g.Entry = s.Entry
      WHERE s.Type = 2 AND s.ZoneId = 60 AND g.Entry IS NULL)                                    AS gunbad_pq_objects_still_missing,
    (SELECT ObjectId3 FROM pquest_objectives WHERE Guid = 2301)                                  AS monstrous_squigs_third_target;

-- Every objective of The Squig Nursery, with how many of its own spawn rows it can credit.
SELECT o.Guid, o.StageId, o.Objective, o.Type, o.Count AS needed,
       (SELECT COUNT(*) FROM pquest_spawns s WHERE s.Objective = o.Guid) AS spawns,
       (SELECT COUNT(*) FROM pquest_spawns s WHERE s.Objective = o.Guid
          AND (s.Entry = o.ObjectId OR s.Entry = o.ObjectId2 OR s.Entry = o.ObjectId3)) AS credited_spawns
  FROM pquest_objectives o
 WHERE o.Entry = 514
 ORDER BY o.StageId, o.Guid;

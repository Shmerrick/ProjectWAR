-- 72_restore_deleted_wagons_and_thanquol_portal.sql
--
-- First recovery from the gameobject deletion documented in docs/GAMEOBJECT_DATA_LOSS.md.
-- Restores the Pillage and Plunder wagons and the Thanquol's Incursion portal, and establishes
-- the method for the remaining 25,157 rows.
--
-- WHERE THE ROWS COME FROM. Not reconstructed and not invented: they are the original rows, read
-- back out of this repository's own git history. The world dump at commit a4995e92 holds 26,313
-- `gameobject_spawns` rows; the current base dump holds 1,146. These ten are lifted verbatim from
-- the pre-deletion dump, including their original Guids, which are still unused:
--
--   git cat-file blob a4995e92:Database/war_world.7z > old_world.7z
--
-- Every column is preserved exactly as it was, including the opaque Unks payloads and the
-- differing NULL-versus-empty-string conventions between the zone 106/410 rows and the zone 110
-- rows. Nothing here is normalised.
--
-- WHY PROTOTYPES ARE NEEDED. The pre-deletion dump has no `gameobject_protos` table at all - it
-- was introduced later - so restoring spawn rows alone is not enough. `GameObjectService`
-- looks the prototype up and `PQuestObjective.Reset` skips the spawn when it is missing, which is
-- why these objects would still not appear.
--
-- WHERE THE PROTOTYPE NAMES COME FROM. The official captures, matched by DisplayID, exactly as
-- migration 68 did for the Siphoning Contraption:
--
--   * DisplayID 211 resolves to "Weapon Wagon" in the Nordland captures (Nordland P4/P5), which
--     is zone 106 - the same zone as five of these spawns, and the objective is "Destroy Wagons".
--   * DisplayID 9290 resolves to "Thanquol's Incursion" in all three Thanquol captures.
--
-- Remaining prototype fields follow the two destructible public-quest objects already in the
-- database - Nursery Slime (100515) and Siphoning Contraption (100517): Scale 50, Faction 0,
-- HealthPoints 1, Unk3 100, IsAttackable 0. The client-side "attackable" bit is carried per spawn
-- in Unks, not by the prototype: both wagon variants set GetUnk(3) = 8, the same bit those two
-- objects use, so the restored rows already declare it.
--
-- WHAT THIS FIXES.
--   * Pillage and Plunder (public quest 199) Stage II "Destroy Wagons" needs 5 wagons. Objective
--     850 has no `pquest_spawns` rows and never did - not in the base dump either - because the
--     wagons are ordinary world objects. `GameObject.SetDeath` credits QUEST_KILL_GO to the
--     player's active public quest for any game object of the matching Entry, so the five zone-106
--     wagons are what completes that stage. It has been uncompletable.
--   * Thanquol's Incursion had no portal spawned. Entry 99891 is hardcoded in
--     `GameObject.cs:259` as a teleport, and its three rows are the three instance copies, at the
--     same Y offsets (0, +196608, +262144) the zone's creature spawns use.

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 1. Prototypes
-- ---------------------------------------------------------------------------

INSERT INTO gameobject_protos
    (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock,
     Unk1, Unk2, Unk3, Unk4, CreatureSpawnText, UnksString, CreatureId, CreatureCount,
     CreatureCooldownMinutes, IsAttackable)
VALUES
    (2000560, 'Weapon Wagon',          211, 50, 40, 0, 1, NULL, NULL, 0, 0, 100, 0, NULL, '0', NULL, NULL, NULL, 0),
    (  99891, 'Thanquol''s Incursion', 9290, 50, 40, 0, 1, NULL, NULL, 0, 0, 100, 0, NULL, '0', NULL, NULL, NULL, 0)
ON DUPLICATE KEY UPDATE Name = VALUES(Name), DisplayID = VALUES(DisplayID);

-- ---------------------------------------------------------------------------
-- 2. The original spawn rows, verbatim
-- ---------------------------------------------------------------------------

DELETE FROM gameobject_spawns
 WHERE Guid IN (245641, 245580, 245579, 245578, 245548, 2107955, 2107958,
                2069673, 2069703, 2069726);

INSERT INTO gameobject_spawns
    (Guid, Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, DisplayID, Unk1, Unk2, Unk3, Unk4,
     Unks, DoorId, VfxState, TokUnlock, SoundId, AllowVfxUpdate, AlternativeName)
VALUES
    -- Pillage and Plunder, Nordland (zone 106). Five wagons; Stage II requires five destroyed.
    ( 245641, 2000560, 106,  872090, 896084,  5400,  273, 211, 0, 0, 100, 0, '7681 0 13550 8 177 16148',   NULL, 0, NULL, 0, 1, NULL),
    ( 245580, 2000560, 106,  874322, 894208,  5248,  102, 211, 0, 0, 100, 0, '7681 0 13550 8 177 16148',   NULL, 0, NULL, 0, 1, NULL),
    ( 245579, 2000560, 106,  873260, 895001,  5392,  625, 211, 0, 0, 100, 0, '7681 0 13550 8 177 16148',   NULL, 0, NULL, 0, 1, NULL),
    ( 245578, 2000560, 106,  873479, 893813,  5208,  329, 211, 0, 0, 100, 0, '7681 0 13550 8 177 16148',   NULL, 0, NULL, 0, 1, NULL),
    ( 245548, 2000560, 106,  873746, 892783,  5048, 1137, 211, 0, 0, 100, 0, '7681 0 13550 8 177 16148',   NULL, 0, NULL, 0, 1, NULL),
    -- Two further wagons in Reikwald (zone 110), unrelated to the public quest.
    (2107955, 2000560, 110, 1432370, 970666, 16190, 2212, 211, 0, 0,   0, 0, '7681 0 13550 553 -29823 -30682 ', 0, 0, '', 0, 0, ''),
    (2107958, 2000560, 110, 1423630, 959093, 15947, 3006, 211, 0, 0,   0, 0, '7681 0 13550 553 -29823 -30682 ', 0, 0, '', 0, 0, ''),
    -- Thanquol's Incursion portal (zone 410), one per instance copy.
    (2069673,   99891, 410,   83240,  83275,  8511, 3936, 9290, 0, 0, 100, 0, '7680 1 20966 36 1465 42012',  NULL, 0, NULL, 0, 1, NULL),
    (2069703,   99891, 410,   83240, 345419,  8511, 3936, 9290, 0, 0, 100, 0, '7680 1 20966 36 33195 32231', NULL, 0, NULL, 0, 1, NULL),
    (2069726,   99891, 410,   83240, 279883,  8511, 3936, 9290, 0, 0, 100, 0, '7680 1 20966 36 152 21781',   NULL, 0, NULL, 0, 1, NULL);

COMMIT;

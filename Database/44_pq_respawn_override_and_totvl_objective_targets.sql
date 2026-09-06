-- 44_pq_respawn_override_and_totvl_objective_targets.sql
--
-- Adds a per-objective public-quest respawn override and uses it for The Squig Nursery, and
-- fills in the three Tomb of the Vulture Lord objectives that name no creature at all.
--
-- USE `war_world`;

USE `war_world`;

-- 1. Per-objective respawn override.
--
--    PQuestCreature.SetRespawnTimer gives a flat ten minutes inside a dungeon
--    (`_publicQuest.IsDungeon()`), which an objective cannot meet when its kill target exceeds
--    its spawn count. The Squig Nursery's "Monstrous Squigs" asks for 50 from 30 spawn points,
--    so a full clear yields 30 kills and then stalls for ten minutes -- the reported "not seeing
--    enough monstrous squigs to progress".
--
--    The column defaults to 0, which keeps the existing behaviour everywhere it is not set. The
--    ORM would add the column on its own from the new DataElement, but it is declared here so
--    the schema is deterministic and the change is reviewable.

--    MySQL 8 has no ADD COLUMN IF NOT EXISTS, so the add is guarded through information_schema
--    to keep the script re-runnable.

SET @add_respawn_seconds = (
    SELECT IF(COUNT(*) = 0,
        'ALTER TABLE `pquest_objectives` ADD COLUMN `RespawnSeconds` INT UNSIGNED NOT NULL DEFAULT 0 AFTER `NoRespawn`',
        'DO 0')
      FROM information_schema.COLUMNS
     WHERE TABLE_SCHEMA = DATABASE()
       AND TABLE_NAME = 'pquest_objectives'
       AND COLUMN_NAME = 'RespawnSeconds');

PREPARE add_respawn_seconds FROM @add_respawn_seconds;
EXECUTE add_respawn_seconds;
DEALLOCATE PREPARE add_respawn_seconds;

-- Monstrous Squigs and the squigs it spawns alongside them: 15 seconds, at the user's direction.
UPDATE `pquest_objectives` SET `RespawnSeconds` = 15 WHERE `Guid` = 2301 AND `Entry` = 514;

-- 2. Tomb of the Vulture Lord objectives with no target creature.
--
--    Three kill objectives carry ObjectId 0, so nothing can ever satisfy them. Each is resolved
--    from the twelve official Tomb of the Vulture Lord captures under
--    WAR-RE-Toolkit/libs/protocolservices/Packet Logs/ (3,383 F_CREATE_MONSTER frames, 93
--    distinct creature names), and every entry named below already has spawns in zone 179, so
--    no population is being invented -- only the link from the objective to the creature.
--
--    2483 "Anointing Embalmers Destroyed" (Ossuary of the Anointed, 5 kills)
--         -> 93901 "Anointing Embalmer^m", an exact name match, 10 distinct object ids in the
--            capture and 10 instance spawns in zone 179. "Anointing Attendant^m" (93899) also
--            exists in that room but is not what the objective names.
--
--    2484 "High Priest Herakh Defeated" (Ossuary of the Anointed, 1 kill)
--         -> 93834 "High Priest Herakh". The identically named objective on both Sepulcher of
--            Swords (2482) and Hall of Awakening (2486) already names 93834; this row is the
--            only one of the three left blank.
--
--    2495 / 10052 "Skeletal Soldiers Destroyed" (The Regiment of Khsar, 27 kills, one row per
--         realm) -> the six Khsar creatures. The capture holds exactly six, all present in zone
--         179 as world spawns and none anywhere else: Augurer 93735 (25 object ids, 19 spawns),
--         Bowman 93759 (20, 18), Chasseur 93738 (21, 18), Physician 93758 (22, 17), Militiaman
--         93737 (17, 12) and Veteran 93760 (15, 14) -- 98 spawns against a 27-kill target.
--         ObjectId through ObjectId6 give exactly six slots.

UPDATE `pquest_objectives` SET `ObjectId` = '93901' WHERE `Guid` = 2483 AND `Entry` = 588 AND (`ObjectId` = '0' OR `ObjectId` IS NULL);
UPDATE `pquest_objectives` SET `ObjectId` = '93834' WHERE `Guid` = 2484 AND `Entry` = 588 AND (`ObjectId` = '0' OR `ObjectId` IS NULL);

UPDATE `pquest_objectives`
   SET `ObjectId`  = '93735', `ObjectId2` = '93759', `ObjectId3` = '93738',
       `ObjectId4` = '93758', `ObjectId5` = '93737', `ObjectId6` = '93760'
 WHERE `Guid` IN (2495, 10052) AND (`ObjectId` = '0' OR `ObjectId` IS NULL);

-- Verification.
SELECT `Guid`, `Entry`, `Objective`, `Count`, `RespawnSeconds`,
       `ObjectId`, `ObjectId2`, `ObjectId3`, `ObjectId4`, `ObjectId5`, `ObjectId6`
  FROM `pquest_objectives`
 WHERE `Guid` IN (2301, 2483, 2484, 2495, 10052)
 ORDER BY `Guid`;

-- Every Tomb of the Vulture Lord kill objective should now name at least one creature that has
-- spawns in zone 179.
SELECT o.Guid, o.Entry AS pquest, o.Objective, o.Count,
       (SELECT COUNT(*) FROM creature_spawns c
         WHERE c.ZoneId = 179 AND c.Entry IN (o.ObjectId, o.ObjectId2, o.ObjectId3, o.ObjectId4, o.ObjectId5, o.ObjectId6)) AS world_spawns,
       (SELECT COUNT(*) FROM instance_creature_spawns i
         WHERE i.ZoneID = 179 AND i.Entry IN (o.ObjectId, o.ObjectId2, o.ObjectId3, o.ObjectId4, o.ObjectId5, o.ObjectId6)) AS instance_spawns
  FROM pquest_objectives o
  JOIN pquest_info p ON p.Entry = o.Entry
 WHERE p.ZoneId = 179 AND o.Type = 2
 ORDER BY o.Guid;

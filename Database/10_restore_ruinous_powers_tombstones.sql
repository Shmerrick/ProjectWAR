-- Restores the Perished Soul game objects used by Ruinous Powers stage II (PQ 185).
--
-- The model, all 25 placements, and headings come from the 1.4.8 packet capture and
-- WAR-RE-Toolkit StaticObject data. A three-second capture duration reproduces the
-- official interaction progress bar; credit is awarded only when that timer completes.

USE `war_world`;

SET @capture_duration_column := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'pquest_spawns'
      AND COLUMN_NAME = 'CaptureDuration'
);
SET @capture_duration_sql := IF(
    @capture_duration_column = 0,
    'ALTER TABLE `pquest_spawns` ADD COLUMN `CaptureDuration` smallint unsigned NOT NULL DEFAULT 0 AFTER `AllowVfxUpdate`',
    'SELECT 1'
);
PREPARE capture_duration_statement FROM @capture_duration_sql;
EXECUTE capture_duration_statement;
DEALLOCATE PREPARE capture_duration_statement;

START TRANSACTION;

INSERT INTO `gameobject_protos`
    (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`,
     `ScriptName`, `TokUnlock`, `Unk1`, `Unk2`, `Unk3`, `Unk4`,
     `CreatureSpawnText`, `UnksString`, `CreatureId`, `CreatureCount`,
     `CreatureCooldownMinutes`, `IsAttackable`)
VALUES
    (1080, 'Perished Soul', 120, 50, 1, 0, 1,
     '', '', 0, 0, 0, 0,
     NULL, '7680 0 14801 32 0 0', 0, 0, 0, 0)
ON DUPLICATE KEY UPDATE
    `Name` = VALUES(`Name`),
    `DisplayID` = VALUES(`DisplayID`),
    `Scale` = VALUES(`Scale`),
    `Level` = VALUES(`Level`),
    `Faction` = VALUES(`Faction`),
    `HealthPoints` = VALUES(`HealthPoints`),
    `ScriptName` = VALUES(`ScriptName`),
    `TokUnlock` = VALUES(`TokUnlock`),
    `Unk1` = VALUES(`Unk1`),
    `Unk2` = VALUES(`Unk2`),
    `Unk3` = VALUES(`Unk3`),
    `Unk4` = VALUES(`Unk4`),
    `CreatureSpawnText` = VALUES(`CreatureSpawnText`),
    `UnksString` = VALUES(`UnksString`),
    `CreatureId` = VALUES(`CreatureId`),
    `CreatureCount` = VALUES(`CreatureCount`),
    `CreatureCooldownMinutes` = VALUES(`CreatureCooldownMinutes`),
    `IsAttackable` = VALUES(`IsAttackable`);

-- 1080 is the objective identifier sent by the official server and is also the
-- local prototype entry, allowing the existing PQ event matcher to remain data-driven.
UPDATE `pquest_objectives`
SET `ObjectId` = '1080'
WHERE `Guid` = 800 AND `Entry` = 185;

DELETE FROM `pquest_spawns`
WHERE `Objective` = 800;

INSERT INTO `pquest_spawns`
    (`pquest_spawns_ID`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`,
     `Objective`, `Type`, `VfxState`, `AllowVfxUpdate`, `CaptureDuration`, `Unks`, `Unk3`)
VALUES
    ('ruinous-powers-soul-01', 1080, 100, 857180, 839612, 6563,  534, 800, 2, 0, 0, 3, '7680 0 14801 32 0 0', 100),
    ('ruinous-powers-soul-02', 1080, 100, 857187, 839242, 6552, 3197, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-03', 1080, 100, 857388, 840946, 6528, 2924, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-04', 1080, 100, 857391, 839102, 6516, 3219, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-05', 1080, 100, 857432, 839753, 6569, 2821, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-06', 1080, 100, 857594, 838672, 6490, 1763, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-07', 1080, 100, 857744, 840561, 6545, 3049, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-08', 1080, 100, 857806, 838812, 6449, 3242, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-09', 1080, 100, 857947, 840483, 6544,  136, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-10', 1080, 100, 858049, 838151, 6529,   11, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-11', 1080, 100, 858127, 837898, 6474, 2412, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-12', 1080, 100, 858246, 840782, 6553, 1399, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-13', 1080, 100, 858307, 837702, 6471, 4061, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-14', 1080, 100, 858427, 840205, 6537,  364, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-15', 1080, 100, 858958, 840567, 6577, 2673, 800, 2, 0, 0, 3, '7680 0 14064 32 0 0', 100),
    ('ruinous-powers-soul-16', 1080, 100, 858968, 837591, 6677, 3356, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-17', 1080, 100, 859261, 837630, 6623, 2025, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-18', 1080, 100, 859296, 838264, 6651, 3766, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-19', 1080, 100, 859500, 839544, 6602,  921, 800, 2, 0, 0, 3, '7680 0 14801 32 0 0', 100),
    ('ruinous-powers-soul-20', 1080, 100, 859582, 837700, 6639,  682, 800, 2, 0, 0, 3, '7680 0 14801 32 0 0', 100),
    ('ruinous-powers-soul-21', 1080, 100, 859629, 838364, 6672, 3640, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-22', 1080, 100, 859773, 839250, 6608,  398, 800, 2, 0, 0, 3, '7680 0 14801 32 0 0', 100),
    ('ruinous-powers-soul-23', 1080, 100, 859814, 837472, 6684, 1752, 800, 2, 0, 0, 3, '7680 0 14801 32 0 0', 100),
    ('ruinous-powers-soul-24', 1080, 100, 860002, 838377, 6660,  341, 800, 2, 0, 0, 3, '7680 0 14800 32 0 0', 100),
    ('ruinous-powers-soul-25', 1080, 100, 860113, 838009, 6680, 2878, 800, 2, 0, 0, 3, '7680 0 14801 32 0 0', 100);

COMMIT;

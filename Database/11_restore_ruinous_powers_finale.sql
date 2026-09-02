-- Restores the two-phase Ruinous Powers finale from the official 1.4.8 capture.
--
-- Wizard Lord Mathus first walks into the ritual for a 37-second scripted phase.
-- Kar'thok then appears at the capture-verified ritual center as the final champion.

USE `war_world`;

SET @stage_id_column := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'pquest_objectives'
      AND COLUMN_NAME = 'StageId'
);
SET @stage_id_sql := IF(
    @stage_id_column = 0,
    'ALTER TABLE `pquest_objectives` ADD COLUMN `StageId` smallint unsigned NOT NULL DEFAULT 0 AFTER `StageName`',
    'SELECT 1'
);
PREPARE stage_id_statement FROM @stage_id_sql;
EXECUTE stage_id_statement;
DEALLOCATE PREPARE stage_id_statement;

START TRANSACTION;

UPDATE `pquest_objectives`
SET `StageId` = 1
WHERE `Guid` = 799 AND `Entry` = 185;

UPDATE `pquest_objectives`
SET `StageId` = 2
WHERE `Guid` = 800 AND `Entry` = 185;

UPDATE `pquest_objectives`
SET `StageName` = 'Wizard Lord Mathus',
    `StageId` = 3,
    `Type` = 12,
    `Objective` = 'Observe Wizard Lord Mathus',
    `Count` = 1,
    `Description` = 'The Wizard Lord Mathus is attempting to end the ritual!',
    `ObjectId` = '4506',
    `ObjectId2` = NULL,
    `ObjectId3` = NULL,
    `ObjectId4` = NULL,
    `ObjectId5` = NULL,
    `ObjectId6` = NULL,
    `TokCompleted` = 0,
    `Time` = 37,
    `NoRespawn` = 1,
    `SoundId` = 0,
    `SoundDelay` = 1,
    `SoundIteration` = 1
WHERE `Guid` = 801 AND `Entry` = 185;

INSERT INTO `pquest_objectives`
    (`Guid`, `Entry`, `StageName`, `StageId`, `Type`, `Objective`, `Count`, `Description`,
     `ObjectId`, `ObjectId2`, `ObjectId3`, `ObjectId4`, `TokCompleted`, `Time`, `NoRespawn`,
     `SoundId`, `SoundDelay`, `SoundIteration`, `ObjectId5`, `ObjectId6`)
VALUES
    (185001, 185, 'Stage III', 4, 2, 'Kar''thok the Bloodhowler', 1,
     'A daemon of another dark power emerges. Prove the Raven Host''s supremacy over all other ruinous powers. Defeat the denizen of rage.',
     '1273', '185', NULL, NULL, 8262, 600, 1, 0, 1, 1, NULL, NULL)
ON DUPLICATE KEY UPDATE
    `Entry` = 185,
    `StageName` = 'Stage III',
    `StageId` = 4,
    `Type` = 2,
    `Objective` = 'Kar''thok the Bloodhowler',
    `Count` = 1,
    `Description` = 'A daemon of another dark power emerges. Prove the Raven Host''s supremacy over all other ruinous powers. Defeat the denizen of rage.',
    `ObjectId` = '1273',
    `ObjectId2` = '185',
    `ObjectId3` = NULL,
    `ObjectId4` = NULL,
    `TokCompleted` = 8262,
    `Time` = 600,
    `NoRespawn` = 1,
    `SoundId` = 0,
    `SoundDelay` = 1,
    `SoundIteration` = 1,
    `ObjectId5` = NULL,
    `ObjectId6` = NULL;

DELETE FROM `pquest_spawns`
WHERE `Objective` = 801 AND `Entry` = 185;

INSERT INTO `pquest_spawns`
    (`pquest_spawns_ID`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`,
     `Objective`, `Type`, `Emote`, `Level`, `Ward`, `SoundId`, `VfxState`,
     `AllowVfxUpdate`, `CaptureDuration`, `Unks`, `Unk3`)
VALUES
    ('52df83bd-879d-4993-b9ef-8c312c52355c', 1474, 100,
     859268, 837987, 6576, 307, 801, 1, 0, 4, 0, 0, 0, 1, 0, '0 0 0 0 0 0', NULL),
    ('4a43ccc9-08fd-11e4-ac0c-406c8f12b734', 185, 100,
     858717, 839275, 6352, 1991, 185001, 1, 0, 3, 0, 0, 0, 1, 0, '0 0 0 0 0 0', NULL)
ON DUPLICATE KEY UPDATE
    `Entry` = IF(`pquest_spawns_ID` = '52df83bd-879d-4993-b9ef-8c312c52355c', 1474, 185),
    `ZoneId` = 100,
    `WorldX` = IF(`pquest_spawns_ID` = '52df83bd-879d-4993-b9ef-8c312c52355c', 859268, 858717),
    `WorldY` = IF(`pquest_spawns_ID` = '52df83bd-879d-4993-b9ef-8c312c52355c', 837987, 839275),
    `WorldZ` = IF(`pquest_spawns_ID` = '52df83bd-879d-4993-b9ef-8c312c52355c', 6576, 6352),
    `WorldO` = IF(`pquest_spawns_ID` = '52df83bd-879d-4993-b9ef-8c312c52355c', 307, 1991),
    `Objective` = IF(`pquest_spawns_ID` = '52df83bd-879d-4993-b9ef-8c312c52355c', 801, 185001),
    `Type` = 1,
    `Emote` = 0,
    `Level` = IF(`pquest_spawns_ID` = '52df83bd-879d-4993-b9ef-8c312c52355c', 4, 3),
    `Ward` = 0,
    `SoundId` = 0,
    `VfxState` = 0,
    `AllowVfxUpdate` = 1,
    `CaptureDuration` = 0,
    `Unks` = '0 0 0 0 0 0',
    `Unk3` = NULL;

COMMIT;

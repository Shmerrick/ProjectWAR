-- Restores the missing Norsca chapter transition data and nearby 1.4.8 static objects.
--
-- Coordinates, headings, display IDs, and packet unknowns come from the official
-- PvE_T1CHAOS_NORSCA_NORDLAND capture and WAR-RE-Toolkit StaticObject data.

USE `war_world`;

START TRANSACTION;

-- Wulfsiege Forest is Chaos Chapter 2. The shipped row incorrectly points at
-- Chapter 1 influence (66); the zone's client influence map identifies 67.
UPDATE `zone_areas`
SET `DestroInfluenceId` = 67
WHERE `ZoneId` = 100
  AND `PieceId` = 3
  AND `AreaId` = 48;

-- The old reward point was almost forty world units from the official chest on
-- sloped terrain. Preserve the capture's exact position instead of sharing its Z.
UPDATE `pquest_info`
SET `GoldChestWorldX` = 858858,
    `GoldChestWorldY` = 838868,
    `GoldChestWorldZ` = 6344
WHERE `Entry` = 185
  AND `ZoneId` = 100;

INSERT INTO `gameobject_protos`
    (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`,
     `ScriptName`, `TokUnlock`, `Unk1`, `Unk2`, `Unk3`, `Unk4`,
     `CreatureSpawnText`, `UnksString`, `CreatureId`, `CreatureCount`,
     `CreatureCooldownMinutes`, `IsAttackable`)
VALUES
    (2000571, 'Mutilated Livestock', 350, 50, 1, 0, 1,
     '', '', 0, 0, 100, 0, NULL, '7680 0 14081 32 5 40364', 0, 0, 0, 0),
    (2000572, 'Eye of Change', 904, 50, 1, 0, 1,
     '', '', 0, 0, 100, 0, NULL, '7682 0 14284 4 5 40364', 0, 0, 0, 0),
    (2000573, 'Chaos Portal', 1638, 50, 1, 0, 1,
     '', '', 0, 0, 100, 0, NULL, '7680 0 14825 33 33925 42232', 0, 0, 0, 0)
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

INSERT INTO `gameobject_spawns`
    (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`,
     `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unks`, `DoorId`, `VfxState`, `TokUnlock`,
     `SoundId`, `AllowVfxUpdate`, `AlternativeName`)
VALUES
    (185000001, 2000571, 100, 858572, 838813, 6320, 3401, 350,
     0, 0, 100, 0, '7680 0 14081 32 5 40364', 0, 0, '', 0, 0, 'Mutilated Livestock'),
    (185000002, 2000572, 100, 858707, 839223, 6352, 2332, 904,
     0, 0, 100, 0, '7682 0 14284 4 5 40364', 0, 0, '2375', 0, 1, 'Eye of Change'),
    (185000003, 2000573, 100, 858707, 839224, 6373, 2252, 1638,
     0, 0, 100, 0, '7680 0 14825 33 33925 42232', 0, 0, '', 0, 0, 'Chaos Portal')
ON DUPLICATE KEY UPDATE
    `Entry` = VALUES(`Entry`),
    `ZoneId` = VALUES(`ZoneId`),
    `WorldX` = VALUES(`WorldX`),
    `WorldY` = VALUES(`WorldY`),
    `WorldZ` = VALUES(`WorldZ`),
    `WorldO` = VALUES(`WorldO`),
    `DisplayID` = VALUES(`DisplayID`),
    `Unk1` = VALUES(`Unk1`),
    `Unk2` = VALUES(`Unk2`),
    `Unk3` = VALUES(`Unk3`),
    `Unk4` = VALUES(`Unk4`),
    `Unks` = VALUES(`Unks`),
    `DoorId` = VALUES(`DoorId`),
    `VfxState` = VALUES(`VfxState`),
    `TokUnlock` = VALUES(`TokUnlock`),
    `SoundId` = VALUES(`SoundId`),
    `AllowVfxUpdate` = VALUES(`AllowVfxUpdate`),
    `AlternativeName` = VALUES(`AlternativeName`);

COMMIT;

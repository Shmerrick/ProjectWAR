-- Holmsteinn Revisited: 43 existing objective-808 placements reference missing proto 551.
-- Evidence: WAR-RE-Toolkit/libs/protocolservices/Packet Logs/
-- PQ_T1CHAOS_EASY_holmsteinn revisited_CH2.txt.gz, F_CREATE_STATIC packets
-- #266-268 (1-based packet ordinal, both directions). Payload +16 = model 0x000A,
-- +18 = 0x1E00, +20 = 0, +22 = 0x37AA, +28 = 100, name = Holmsteinn Supplies.
-- Existing coordinates and per-object opaque fields are retained, not reconstructed.
-- Level/health/scale use the schema's inert-object defaults; no combat stats are inferred.
USE `war_world`;
START TRANSACTION;
INSERT INTO `gameobject_protos`
    (`Entry`, `Name`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `UnksString`)
VALUES
    (551, 'Holmsteinn Supplies', 10, 0, 0, 100, 0, '7680 0 14250 32 10620 57450')
ON DUPLICATE KEY UPDATE
    `Name` = VALUES(`Name`), `DisplayID` = VALUES(`DisplayID`);

-- The spawn value overrides the prototype in F_CREATE_STATIC; NULL was loaded as zero.
UPDATE `pquest_spawns` SET `Unk3` = 100
WHERE `Objective` = 808 AND `Entry` = 551 AND `Type` = 2 AND `Unk3` IS NULL;
COMMIT;

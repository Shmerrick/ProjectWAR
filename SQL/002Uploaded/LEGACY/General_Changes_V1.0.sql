/*
	V1.0
*/

/* Adding some Entrys needed for Fort-Zones */
DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '66699796');
DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '666571');
DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '666572');
DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '666573');
DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '66698761');
DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '66699369');
DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '66699823');
DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '66699824');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `Unks`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('66699796', '', '143', '50', '1', '0', '1', '', '7682 0 26604 800 5 44455', '0', '0', '100', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('666571', 'Siege Spawnpoint', '3441', '50', '1', '0', '1', '', '0', '0', '0', '0');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('666572', 'Siege Spawnpoint', '3440', '50', '1', '0', '1', '', '0', '0', '0', '0');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `TokUnlock`, `Unks`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('666573', 'Siege Spawnpoint Marker', '5395', '0', '0', '0', '0', '', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `TokUnlock`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('66698761', 'Supply Chest', '16', '50', '1', '0', '1', '', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('66699369', 'Supply Box', '10', '50', '1', '0', '1', '', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('66699823', 'Supply Bag', '46', '50', '1', '0', '1', '', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('66699824', 'Supply Bag', '47', '50', '1', '0', '1', '', '0', '0', '0', '0', '0', '0', '0', '0');

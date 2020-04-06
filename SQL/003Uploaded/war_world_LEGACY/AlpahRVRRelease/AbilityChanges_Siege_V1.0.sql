UPDATE `war_world`.`ability_commands` SET `PrimaryValue` = '86210' WHERE (`Entry` = '24770') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_commands` SET `PrimaryValue` = '86222' WHERE (`Entry` = '24776') and (`CommandID` = '0') and (`CommandSequence` = '0');


DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23668') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23668', 'Deploy Cannon', '0', '0', 'SummonSiegeWeapon', '86236', 'Caster');

DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '24664') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '24666') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '24670') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '24672') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '24770') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '24776') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('24664', 'Deploy Empire Cannon', '0', '0', 'SummonSiegeWeapon', '86212', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('24666', 'Deploy Hellblaster', '0', '0', 'SummonSiegeWeapon', '86209', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('24670', 'Deploy Hellcannon', '0', '0', 'SummonSiegeWeapon', '86221', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('24672', 'Deploy Tri-Barrel Hellcannon', '0', '0', 'SummonSiegeWeapon', '86224', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('24770', 'Deploy Empire Ram', '0', '0', 'SummonSiegeWeapon', '86210', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('24776', 'Deploy Chaos Ram', '0', '0', 'SummonSiegeWeapon', '86222', 'Caster');

DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23668') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23670') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23674') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23676') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23680') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23682') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23686') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '23688') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23668', 'Deploy Cannon', '0', '0', 'SummonSiegeWeapon', '86236', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23670', 'Deploy Organ Gun', '0', '0', 'SummonSiegeWeapon', '86233', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23674', 'Deploy Supa-Chucka', '0', '0', 'SummonSiegeWeapon', '86248', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23676', 'Deploy Orcapult', '0', '0', 'SummonSiegeWeapon', '86245', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23680', 'Deploy High Elf Ballista', '0', '0', 'SummonSiegeWeapon', '86260', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23682', 'Deploy High Elf Bolt Thrower', '0', '0', 'SummonSiegeWeapon', '86257', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23686', 'Deploy Dark Elf Ballista', '0', '0', 'SummonSiegeWeapon', '86272', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('23688', 'Deploy Dark Elf Bolt Thrower', '0', '0', 'SummonSiegeWeapon', '86269', 'Caster');

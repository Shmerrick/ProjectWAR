#dmg reduction ability implementation 
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `Cooldown`, `EffectID`, `Specline`, `IconId`, `Category`, `Flags`, `PointCost`) VALUES ('20003', '0', 'Damage Reduction - Fort Lord', '15', '621', 'NPC', '4675', '0', '10', '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('20003', 'Damage Reduction - Fort Lord', '0', '0', 'InvokeBuff', '20003', 'Caster');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `TertiaryValue`, `InvokeOn`, `Target`, `MaxTargets`, `BuffLine`) VALUES ('20003', 'Damage Reduction - Fort Lord', '0', '0', 'ModifyPercentageStatByNearbyAllies', '25', '-2', '-2', '5', 'Host', '40', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `Duration`, `CanRefresh`, `Silent`) VALUES ('20003', 'Damage Reduction - Fort Lord', '1', '40', '30', '1', '1');

#Frenzy Animation
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`) VALUES ('5802', 'Frenzy', '1', '0', 'ObjectEffectState', '1', '5', 'Host');

#Assigning DMG reduction to boss
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('34178', '20003', '15', '104005ad-0e72-11e7-9ea9-000c29d63948', 'I will claim you!', '0', '0', '0', '1', '1', '0', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('15959', '20003', '30', '004005ad-0e72-11e7-9ea9-000c29d63948', 'I will claim you!', '0', '0', '0', '1', '1', '0', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6884', '20003', '30', '010005ad-0e72-11e7-9ea9-000c29d63948', 'I will claim you!', '0', '0', '0', '1', '1', '0', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('778039', '20003', '30', '110005ad-0e72-11e7-9ea9-000c29d63948', 'I will claim you!', '0', '0', '0', '1', '1', '0', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6667', '20003', '30', '204005ad-0e72-11e7-9ea9-000c29d63948', 'I will claim you!', '0', '0', '0', '1', '1', '0', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('38156', '20003', '30', '210005ad-0e72-11e7-9ea9-000c29d63948', 'I will claim you!', '0', '0', '0', '1', '1', '0', '0', '0');

#Lord bug Fix
UPDATE `war_world`.`abilities` SET `Cooldown` = '30' WHERE (`Entry` = '5802');
UPDATE `war_world`.`abilities` SET `Cooldown` = '7' WHERE (`Entry` = '13098');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '30' WHERE (`creature_abilities_ID` = '104001ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '7' WHERE (`creature_abilities_ID` = '104003ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '15' WHERE (`creature_abilities_ID` = '104004ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '-500' WHERE (`Entry` = '5802') and (`CommandID` = '0') and (`CommandSequence` = '1');

UPDATE `war_world`.`creature_abilities` SET `Text` = 'Unstoppable power pours through my blood!', `DisableAtHealthPercent` = '0' WHERE (`creature_abilities_ID` = '104001ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Text` = 'Curse you!' WHERE (`creature_abilities_ID` = '104002ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Text` = 'All will suffer!' WHERE (`creature_abilities_ID` = '104003ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Text` = 'Away Cretin!' WHERE (`creature_abilities_ID` = '104004ad-0e72-11e7-9ea9-000c29d63948');

UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '15', `Text` = 'Away Cretin!' WHERE (`creature_abilities_ID` = '004004ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '7', `Text` = 'All will suffer!' WHERE (`creature_abilities_ID` = '004003ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '30', `Text` = 'Unstoppable power pours through my blood!' WHERE (`creature_abilities_ID` = '004001ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Text` = 'Curse you!' WHERE (`creature_abilities_ID` = '004002ad-0e72-11e7-9ea9-000c29d63948');

UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '15', `Text` = 'Away Cretin!' WHERE (`creature_abilities_ID` = '010004ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '7', `Text` = 'All will suffer!' WHERE (`creature_abilities_ID` = '010003ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '30', `Text` = 'Unstoppable power pours through my blood!' WHERE (`creature_abilities_ID` = '010001ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Text` = 'Curse you!' WHERE (`creature_abilities_ID` = '010002ad-0e72-11e7-9ea9-000c29d63948');

UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '15', `Text` = 'Away Cretin!' WHERE (`creature_abilities_ID` = '110004ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '7', `Text` = 'All will suffer!' WHERE (`creature_abilities_ID` = '110003ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '30', `Text` = 'Unstoppable power pours through my blood!' WHERE (`creature_abilities_ID` = '110001ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Text` = 'Curse you!' WHERE (`creature_abilities_ID` = '110002ad-0e72-11e7-9ea9-000c29d63948');

UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '15', `Text` = 'Away Cretin!' WHERE (`creature_abilities_ID` = '204004ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '7', `Text` = 'All will suffer!' WHERE (`creature_abilities_ID` = '204003ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '30', `Text` = 'Unstoppable power pours through my blood!' WHERE (`creature_abilities_ID` = '204001ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Text` = 'Curse you!' WHERE (`creature_abilities_ID` = '204002ad-0e72-11e7-9ea9-000c29d63948');

UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '15', `Text` = 'Away Cretin!' WHERE (`creature_abilities_ID` = '210004ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '7', `Text` = 'All will suffer!' WHERE (`creature_abilities_ID` = '210003ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '30', `Text` = 'Unstoppable power pours through my blood!' WHERE (`creature_abilities_ID` = '210001ad-0e72-11e7-9ea9-000c29d63948');
UPDATE `war_world`.`creature_abilities` SET `Text` = 'Curse you!' WHERE (`creature_abilities_ID` = '210002ad-0e72-11e7-9ea9-000c29d63948');

#Blorc Arm Breaka - 50 Ap Drain
UPDATE `war_world`.`ability_commands` SET `CommandName` = 'ModifyAP', `PrimaryValue` = '-50', `Target` = 'Enemy', `FromAllTargets` = NULL WHERE (`Entry` = '1687') and (`CommandID` = '0') and (`CommandSequence` = '1');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('1687', 'Arm Breaka', '0', '2', 'ModifyAP', '50', 'Caster');

DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '1687') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '1687');

#SM Sapping Strike - 50 Ap Drain
UPDATE `war_world`.`ability_commands` SET `CommandName` = 'ModifyAP', `PrimaryValue` = '-50', `Target` = 'Enemy', `FromAllTargets` = NULL WHERE (`Entry` = '9014') and (`CommandID` = '0') and (`CommandSequence` = '1');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('9014', 'Sapping Strike', '0', '2', 'ModifyAP', '50', 'Caster');

DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '9014') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '9014');



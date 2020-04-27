

DELETE FROM `war_world`.`abilities` WHERE (`Entry` = '245');
DELETE FROM `war_world`.`abilities` WHERE (`Entry` = '27820');
DELETE FROM `war_world`.`abilities` WHERE (`Entry` = '27821');

INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `Cooldown`, `EffectID`, `Specline`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('245', '16777215', 'Flee', '30', '69', 'Core Ability', '0', '0', '5004', '19', '10', '0', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `Specline`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('27820', '0', 'Improved Flee I', '0', 'Core Ability', '0', '1', '5004', '13', '642', '10', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `Specline`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('27821', '0', 'Improved Flee II', '0', 'Core Ability', '0', '1', '5004', '13', '642', '15', '0');



DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '245') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '245') and (`CommandID` = '0') and (`CommandSequence` = '1');

INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('245', 'Flee', '0', '0', 'InvokeBuff', '245', 'Caster');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('245', 'Flee', '0', '1', 'InvokeBuff', '247', 'Caster');



DELETE FROM `war_world`.`ability_modifiers` WHERE (`ability_modifiers_ID` = 'be931424-db19-11e8-9f8b-f2801f1b9fd1');
DELETE FROM `war_world`.`ability_modifiers` WHERE (`ability_modifiers_ID` = 'be9316d6-db19-11e8-9f8b-f2801f1b9fd1');
DELETE FROM `war_world`.`ability_modifiers` WHERE (`ability_modifiers_ID` = 'be93183e-db19-11e8-9f8b-f2801f1b9fd1');

INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `BuffLine`, `ability_modifiers_ID`) VALUES ('27820', 'Improved Flee I', '245', 'Flee', '2', '0', 'SwitchDuration', '15', '1', 'be931424-db19-11e8-9f8b-f2801f1b9fd1');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `BuffLine`, `ability_modifiers_ID`) VALUES ('27821', 'Improved Flee II', '247', 'Panic!', '2', '0', 'SwitchParameters', '1', '1', 'be9316d6-db19-11e8-9f8b-f2801f1b9fd1');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `BuffLine`, `ability_modifiers_ID`) VALUES ('27821', 'Improved Flee II', '247', 'Panic!', '2', '0', 'SwitchDuration', '1', '1', 'be93183e-db19-11e8-9f8b-f2801f1b9fd1');



DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '245') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27820') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27820') and (`CommandID` = '1') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27821') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27821') and (`CommandID` = '1') and (`CommandSequence` = '0');

INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('245', 'Flee', '0', '0', 'ModifySpeed', '30', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`) VALUES ('27820', 'Improved Flee I', '0', '0', 'RegisterModifiers', '0', '0', '5', 'Host');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `BuffLine`, `NoAutoUse`) VALUES ('27820', 'Improved Flee I', '1', '0', 'AddBuffLine', '1', '1', 'Host', '1', '0');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`) VALUES ('27821', 'Improved Flee II', '0', '0', 'RegisterModifiers', '0', '0', '5', 'Host');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `BuffLine`, `NoAutoUse`) VALUES ('27821', 'Improved Flee II', '1', '0', 'AddBuffLine', '1', '1', 'Host', '1', '0');




DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '245');
DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '27820');
DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '27821');

INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `Duration`, `Silent`) VALUES ('245', 'Flee', '1', '1', '10', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('27820', 'Improved Flee I', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('27821', 'Improved Flee II', '1', '1', '1');



DELETE FROM `war_world`.`characterinfo_renown` WHERE (`SpellId` = '10034');
DELETE FROM `war_world`.`characterinfo_renown` WHERE (`SpellId` = '10035');

INSERT INTO `war_world`.`characterinfo_renown` (`SpellId`, `Name`, `ID`, `CommandName`, `Stat`, `Value`, `Passive`, `Tree`, `Position`, `Renown_Costs`, `Slotreq`, `Unk`) VALUES ('10034', 'Improved Flee I', '74', 'AddBuff', '0', '27820', '0', '13', '10', '10', '0', '0200006CAC0001141A0100000000000000000000000000');
INSERT INTO `war_world`.`characterinfo_renown` (`SpellId`, `Name`, `ID`, `CommandName`, `Stat`, `Value`, `Passive`, `Tree`, `Position`, `Renown_Costs`, `Slotreq`, `Unk`) VALUES ('10035', 'Improved Flee II', '75', 'AddBuff', '0', '27821', '0', '13', '11', '10', '10', '0200006CAD0000141A0100000000000000000000000000');



-- class revert 12
-- phisheface
-- 7/27/18

-- WE Sharpened edge tactic will now debuff armor for 1600 dependant on spec points for 3 sec's. 
-- This effects Puncture, Sacrificial Stab, Heart Render, Ruthless Assault
-- Tried to get it to work on On your knee's! but can't figure it out yet.
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '9443') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `AbilityType`, `MasteryTree`, `Specline`, `WeaponNeeded`) VALUES ('3623', '0', 'Sharpened Edge', '255', '0', 'Core Ability', '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('9443', 'Sharpened Edge', '0', '0', 'InvokeBuff', '3623', 'Enemy');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `TertiaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('3623', 'Sharpened Edge', '0', '0', 'ModifyStat', '26', '-40', '-1600', '5', 'Host', '2');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxStack`, `Duration`, `Silent`) VALUES ('3623', 'Sharpened Edge', '1', '3', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `NoAutoUse`) VALUES ('9443', 'Sharpened Edge', '0', '0', 'InvokeBuff', '3623', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('9443', 'Sharpened Edge', '1', '0', 'AddBuffLine', '1', 'Host', '1');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `ability_modifiers_ID`) VALUES ('9443', 'Sharpened Edge', '9410', 'Puncture', '1', '1', 'AppendAbilityCommand', '7faa52be-8b2b-11e63rrt-b8e9-00ff0731187a');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `TargetCommandSequence`, `ability_modifiers_ID`) VALUES ('9443', 'Sharpened Edge', '9410', 'Puncture', '1', '1', 'AppendAbilityCommand', '1', '7faa5591-8b2b-11e6-5tyeb8e9-00ff0731187a');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `TargetCommandSequence`, `ability_modifiers_ID`) VALUES ('9443', 'Sharpened Edge', '9428', 'Sacrificial Stab', '1', '2', 'AppendAbilityCommand', '1', '7faa5c00-8b2b-11e6-b8e9-03217g0ff0731187a');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `TargetCommandID`, `TargetCommandSequence`, `ability_modifiers_ID`) VALUES ('9443', 'Sharpened Edge', '9428', 'Sacrificial Stab', '1', '2', 'AppendAbilityCommand', '1', '1', '7faa6ffe-8b2b-11e6serq875g-b8e9-00ff0731187a');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `ability_modifiers_ID`) VALUES ('9443', 'Sharpened Edge', '9404', 'Heart Render Toxin', '1', '0', 'AppendAbilityCommand', '7faa8d98-8b2b-11e6-b8e9-00ff073dddeq1187a');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `TargetCommandID`, `ability_modifiers_ID`) VALUES ('9443', 'Sharpened Edge', '9404', 'Heart Render Toxin', '1', '0', 'AppendAbilityCommand', '1', '7faa935f-8b2b-11e6ytge-b8e9-00ff0731187a');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `ability_modifiers_ID`) VALUES ('9443', 'Sharpened Edge', '9399', 'Ruthless Assault', '2', '0', 'AppendBuffCommand', '7faaaf99-8b2b-11e6-b8e9-00ffoliuj0731187a');

-- to undo the delete above
-- INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `InvokeOn`, `Target`, `EventIDString`, `EventCheck`, `EventCheckParam`, `RetriggerInterval`, `BuffLine`, `NoAutoUse`) VALUES ('9443', 'Sharpened Edge', '0', '0', 'DealProcDamage', '0', 'EventInstigator', 'WasDefended', 'CheckDefenseFlags', '3', '2000', '1', '0');
-- delete all inserts

-- Ruthless Assault range reduced to 10ft
UPDATE `war_world`.`abilities` SET `Range` = '10' WHERE (`Entry` = '9422');

-- Da Waaagh is coming is now affected by chop fast
UPDATE `war_world`.`abilities` SET `IgnoreCooldownReduction` = '0' WHERE (`Entry` = '1931');

-- Repel darkness/Hastened dismissal cooldown no longer requires a great weapon
UPDATE `war_world`.`ability_modifiers` SET `ModifierCommandName` = 'AddCooldownMS', `PrimaryValue` = '-10000' WHERE (`ability_modifiers_ID` = '7faa4254-8b2b-11e6-b8e9-00ff0731187a');
UPDATE `war_world`.`ability_modifiers` SET `ModifierCommandName` = 'AddCooldownMS', `PrimaryValue` = '-10000' WHERE (`ability_modifiers_ID` = '7fa9f3c8-8b2b-11e6-b8e9-00ff0731187a');

-- sigmar's shield duration increased to 20 (this was a bug, didn't operate as the tooltip reads) Now works as the tooltip
UPDATE `war_world`.`buff_infos` SET `Duration` = '20' WHERE (`Entry` = '3752');


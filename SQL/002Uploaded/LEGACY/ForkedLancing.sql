#Forked Lancing <----ALREADY IMPLEMENTED AS HOTFIX
UPDATE `war_world`.`ability_commands` SET `AbilityName` = 'Forked Lancing', `NoAutoUse` = '1' WHERE (`Entry` = '9237') and (`CommandID` = '2') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_modifiers` SET `ModifierCommandName` = 'AddAbilityCommandWithDamage', `PrimaryValue` = '2' WHERE (`ability_modifiers_ID` = '7fa9e69a-8b2b-11e6-b8e9-00ff0731187a');

UPDATE `war_world`.`buff_commands` SET `PrimaryValue` = '100' WHERE (`Entry` = '5700') and (`CommandID` = '0') and (`CommandSequence` = '1');
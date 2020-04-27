INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('4998', 'Stun', '0', '0', 'ApplyCC', '16', '1', '7', 'Host', '1');
UPDATE `war_world`.`buff_commands` SET `CommandName` = 'InvokeBuff', `PrimaryValue` = '4998', `SecondaryValue` = NULL, `InvokeOn` = '0', `Target` = 'EventInstigator', `EventIDString` = 'WasAttacked', `EventCheck` = 'DamageIsMeleeAbility', `EventChance` = '25', `BuffLine` = '1', `NoAutoUse` = '0' WHERE (`Entry` = '8197') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxStack`, `Duration`, `Silent`) VALUES ('4998', 'Stun', '1', '1', '1');
DELETE FROM `war_world`.`ability_modifiers` WHERE (`ability_modifiers_ID` = '7faa28ed-8b2b-11e6-b8e9-00ff0731187a');
UPDATE `war_world`.`buff_commands` SET `EventCheck` = NULL WHERE (`Entry` = '8197') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `EventCheck` = 'DamageIsFromStat', `EventCheckParam` = '1' WHERE (`Entry` = '8197') and (`CommandID` = '0') and (`CommandSequence` = '0');

-- class revert 12
-- phisheface
-- 7/27/18

-- AM and Shaman change on dispel magic and Mork's touch tactic from directdamage to any damage. (dealingdamage cmd string) This needs to be tested before pushed to production. 
-- I can't test this by myself
UPDATE `war_world`.`buff_commands` SET `EventIDString` = 'DealingDamage' WHERE (`Entry` = '9290') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `EventIDString` = 'DealingDamage' WHERE (`Entry` = '9290') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`buff_commands` SET `EventIDString` = 'DealingDamage' WHERE (`Entry` = '1954') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `EventIDString` = 'DealingDamage' WHERE (`Entry` = '1954') and (`CommandID` = '0') and (`CommandSequence` = '1');

-- to undo above delete above and insert below
-- INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `Target`, `EventIDString`, `EventChance`, `BuffLine`) VALUES ('9290', 'Dispel Magic', '0', '0', 'CleanseDebuffType', '8', '1', 'EventInstigator', 'DirectDamageDealt', '25', '1');
-- INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `Target`, `EventIDString`) VALUES ('9290', 'Dispel Magic', '0', '1', 'DealProcDamage', 'EventInstigator', 'DirectDamageDealt');
-- INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `Target`, `EventIDString`, `EventChance`, `BuffLine`) VALUES ('1954', 'Mork\'s Touch', '0', '0', 'CleanseDebuffType', '8', '1', 'EventInstigator', 'DirectDamageDealt', '25', '1');
-- INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `Target`, `EventIDString`) VALUES ('1954', 'Mork\'s Touch', '0', '1', 'DealProcDamage', 'EventInstigator', 'DirectDamageDealt');

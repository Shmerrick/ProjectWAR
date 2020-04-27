
DELETE FROM `war_world`.`abilities` WHERE (`Entry` = '27801');
DELETE FROM `war_world`.`abilities` WHERE (`Entry` = '27803');
DELETE FROM `war_world`.`abilities` WHERE (`Entry` = '27804');
DELETE FROM `war_world`.`abilities` WHERE (`Entry` = '27805');
DELETE FROM `war_world`.`abilities` WHERE (`Entry` = '27806');

INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `Specline`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('27801', '0', 'Hardy Concession I', '0', 'Core Ability', '0', '1', '22265', '14', '642', '1', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `Specline`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('27803', '0', 'Hardy Concession II', '0', 'Core Ability', '0', '1', '22265', '14', '642', '3', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `Specline`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('27804', '0', 'Hardy Concession III', '0', 'Core Ability', '0', '1', '22265', '14', '642', '6', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `Specline`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('27805', '0', 'Hardy Concession IV', '0', 'Core Ability', '0', '1', '22265', '14', '642', '10', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `Specline`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('27806', '0', 'Hardy Concession V', '0', 'Core Ability', '0', '1', '22265', '14', '642', '14', '0');



DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '27801') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('27801', 'Hardy Concession I', '0', '0', 'InvokeBuff', '27801', 'Host');


DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27801') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27801') and (`CommandID` = '0') and (`CommandSequence` = '1');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27801') and (`CommandID` = '0') and (`CommandSequence` = '2');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27803') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27803') and (`CommandID` = '0') and (`CommandSequence` = '1');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27803') and (`CommandID` = '0') and (`CommandSequence` = '2');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27804') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27804') and (`CommandID` = '0') and (`CommandSequence` = '1');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27804') and (`CommandID` = '0') and (`CommandSequence` = '2');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27805') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27805') and (`CommandID` = '0') and (`CommandSequence` = '1');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27805') and (`CommandID` = '0') and (`CommandSequence` = '2');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27806') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27806') and (`CommandID` = '0') and (`CommandSequence` = '1');
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '27806') and (`CommandID` = '0') and (`CommandSequence` = '2');

INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27801', 'Hardy Concession I', '0', '0', 'ModifyPercentageStat', '23', '-1', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27801', 'Hardy Concession I', '0', '1', 'ModifyPercentageStat', '25', '-1', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27801', 'Hardy Concession I', '0', '2', 'ModifyPercentageStat', '100', '-1', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27803', 'Hardy Concession II', '0', '0', 'ModifyPercentageStat', '23', '-3', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27803', 'Hardy Concession II', '0', '1', 'ModifyPercentageStat', '25', '-3', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27803', 'Hardy Concession II', '0', '2', 'ModifyPercentageStat', '100', '-3', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27804', 'Hardy Concession III', '0', '0', 'ModifyPercentageStat', '23', '-6', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27804', 'Hardy Concession III', '0', '1', 'ModifyPercentageStat', '25', '-6', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27804', 'Hardy Concession III', '0', '2', 'ModifyPercentageStat', '100', '-6', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27805', 'Hardy Concession IV', '0', '0', 'ModifyPercentageStat', '23', '-11', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27805', 'Hardy Concession IV', '0', '1', 'ModifyPercentageStat', '25', '-11', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27805', 'Hardy Concession IV', '0', '2', 'ModifyPercentageStat', '100', '-11', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27806', 'Hardy Concession V', '0', '0', 'ModifyPercentageStat', '23', '-19', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27806', 'Hardy Concession V', '0', '1', 'ModifyPercentageStat', '25', '-19', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('27806', 'Hardy Concession V', '0', '2', 'ModifyPercentageStat', '100', '-19', '5', 'Host', '1');



DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '27801');
DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '27803');
DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '27804');
DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '27805');
DELETE FROM `war_world`.`buff_infos` WHERE (`Entry` = '27806');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('27801', 'Hardy Concession I', 'Career', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('27803', 'Hardy Concession II', 'Career', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('27804', 'Hardy Concession III', 'Career', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('27805', 'Hardy Concession IV', 'Career', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('27806', 'Hardy Concession V', 'Career', '1', '1', '1');



DELETE FROM `war_world`.`characterinfo_renown` WHERE (`SpellId` = '9964');
DELETE FROM `war_world`.`characterinfo_renown` WHERE (`SpellId` = '9965');
DELETE FROM `war_world`.`characterinfo_renown` WHERE (`SpellId` = '9966');
DELETE FROM `war_world`.`characterinfo_renown` WHERE (`SpellId` = '9967');
DELETE FROM `war_world`.`characterinfo_renown` WHERE (`SpellId` = '9968');

INSERT INTO `war_world`.`characterinfo_renown` (`SpellId`, `Name`, `ID`, `CommandName`, `Stat`, `Value`, `Passive`, `Tree`, `Position`, `Renown_Costs`, `Slotreq`, `Unk`) VALUES ('9964', 'Hardy Concessions I', '84', 'AddBuff', '0', '27801', '0', '14', '9', '1', '0', '02 00 00 6C 99 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00');
INSERT INTO `war_world`.`characterinfo_renown` (`SpellId`, `Name`, `ID`, `CommandName`, `Stat`, `Value`, `Passive`, `Tree`, `Position`, `Renown_Costs`, `Slotreq`, `Unk`) VALUES ('9965', 'Hardy Concessions II', '85', 'AddBuff', '0', '27803', '0', '14', '10', '3', '9', '02 00 00 6C 9B 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00');
INSERT INTO `war_world`.`characterinfo_renown` (`SpellId`, `Name`, `ID`, `CommandName`, `Stat`, `Value`, `Passive`, `Tree`, `Position`, `Renown_Costs`, `Slotreq`, `Unk`) VALUES ('9966', 'Hardy Concessions III', '86', 'AddBuff', '0', '27804', '0', '14', '11', '6', '10', '02 00 00 6C 9C 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00');
INSERT INTO `war_world`.`characterinfo_renown` (`SpellId`, `Name`, `ID`, `CommandName`, `Stat`, `Value`, `Passive`, `Tree`, `Position`, `Renown_Costs`, `Slotreq`, `Unk`) VALUES ('9967', 'Hardy Concessions IV', '87', 'AddBuff', '0', '27805', '0', '14', '12', '10', '11', '02 00 00 6C 9D 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00');
INSERT INTO `war_world`.`characterinfo_renown` (`SpellId`, `Name`, `ID`, `CommandName`, `Stat`, `Value`, `Passive`, `Tree`, `Position`, `Renown_Costs`, `Slotreq`, `Unk`) VALUES ('9968', 'Hardy Concessions V', '88', 'AddBuff', '0', '27806', '0', '14', '13', '14', '12', '02 00 00 6C 9E 00 01 14 10 01 00 00 00 00 00 00 00 00 00 00 00 00 00');

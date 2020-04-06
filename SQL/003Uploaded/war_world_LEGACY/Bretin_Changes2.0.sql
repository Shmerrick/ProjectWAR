#NEW CHANGES

#AM-DRAIN MAGIC
UPDATE `war_world`.`buff_infos` SET `Interval`='3000' WHERE `Entry`='9249';
UPDATE `war_world`.`buff_commands` SET `PrimaryValue`='60' WHERE `Entry`='9249' and`CommandID`='0' and`CommandSequence`='0';

#Shaman Yer not so bad
UPDATE `war_world`.`buff_commands` SET `PrimaryValue`='60' WHERE `Entry`='1911' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_infos` SET `Interval`='3000' WHERE `Entry`='1911';

#DoK Restored Motivation
DELETE FROM `war_world`.`buff_infos` WHERE `Entry`='9596';
DELETE FROM `war_world`.`ability_modifiers` WHERE `ability_modifiers_ID`='0';
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `EventIDString`, `EventCheck`, `EventChance`, `BuffLine`, `NoAutoUse`) VALUES ('9596', 'Restored Motivation', '0', '0', 'ModifyAP', '50', '0', 'EventInstigator', 'DirectHealDealt', 'TargetNotCaster', '25', '1', '0');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('9596', 'Restored Motivation', 'Tactic', '1', '1', '1');

#DoK Covenant Retrigger Interval
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`=NULL WHERE `Entry`='9559' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`=NULL WHERE `Entry`='9567' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`=NULL WHERE `Entry`='9567' and`CommandID`='2' and`CommandSequence`='0';

#WP Prayer
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`=NULL WHERE `Entry`='8243' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`=NULL WHERE `Entry`='8243' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`=NULL WHERE `Entry`='8249' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`=NULL WHERE `Entry`='8249' and`CommandID`='1' and`CommandSequence`='0';

#SW Wrist Slash
UPDATE `war_world`.`buff_infos` SET `BuffClassString`=NULL WHERE `Entry`='9116';
UPDATE `war_world`.`buff_infos` SET `BuffClassString`=NULL WHERE `Entry`='9116';
UPDATE `war_world`.`buff_infos` SET `TypeString`='Ailment' WHERE `Entry`='9116';


#AM Energy of Vaul / SH Fury of da green
UPDATE war_world.ability_commands SET Target='AllyOrSelf' WHERE Entry='9274' andCommandID='2' andCommandSequence='0';
UPDATE war_world.ability_commands SET Target='AllyOrSelf' WHERE Entry='1935' andCommandID='2' andCommandSequence='0';
UPDATE war_world.ability_commands SET EffectRadius=NULL, EffectSource=NULL WHERE Entry='9274' andCommandID='2' andCommandSequence='0';
UPDATE war_world.ability_commands SET EffectRadius=NULL, EffectSource=NULL WHERE Entry='1935' andCommandID='2' andCommandSequence='0';

#AM Shaman/I'll take that/Balance Essence
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='962cb62c-8e82-11e6-a1bb-00ff0731187a';
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='f53a8285-8ed9-11e6-b1c5-00ff0731187a';
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='a557d19b-8e82-11e6-a1bb-00ff0731187a';
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='53a8057-8ed9-11e6-b1c5-00ff0731187a';


#DoK/WP Cleanse
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `Target`, `EffectRadius`, `NoAutoUse`) VALUES ('8289', 'Cleansing Power', '0', '0', 'CleanseDebuffType', '3', '1', 'Group', '150', '1');
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='c118e45b-9c84-11e6-8ae8-00ff0731187a';
UPDATE `war_world`.`ability_modifiers` SET `BuffLine`='1' WHERE `ability_modifiers_ID`='7faa4915-8b2b-11e6-b8e9-00ff0731187a';


#Zealot Mirror of Madness
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`=NULL WHERE `Entry`='8575' and`CommandID`='0' and`CommandSequence`='0';

#SM Potent Enchantments
UPDATE `war_world`.`buff_infos` SET `CanRefresh`=NULL WHERE `Entry`='3461';

#SM Heaven's Blade
UPDATE `war_world`.`buff_infos` SET `CanRefresh`=NULL WHERE `Entry`='3230';

#SM Wods
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits`='1' WHERE `Entry`='9021' and`Index`='1' and`ParentCommandID`='1' and`ParentCommandSequence`='0';
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '9021') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

#WP Smite
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('8250', 'Smite', '1', '0', 'ModifyCareerRes', '45', 'Caster');

#DoK Essence Lash
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('9566', 'Essence Lash', '1', '0', 'ModifyCareerRes', '45', 'Caster');

#Challenge
UPDATE `war_world`.`buff_commands` SET `EventIDString`='DealingDamage' WHERE `Entry`='8333' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `EventIDString`='DealingDamage' WHERE `Entry`='9013' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `EventIDString`='DealingDamage' WHERE `Entry`='9332' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `EventIDString`='DealingDamage' WHERE `Entry`='1368' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `EventIDString`='DealingDamage' WHERE `Entry`='1679' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `EventIDString`='DealingDamage' WHERE `Entry`='8021' and`CommandID`='0' and`CommandSequence`='0';

#Chosen Ravage
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale`='1.2' WHERE `Entry`='8323' and`Index`='0' and`ParentCommandID`='0' and`ParentCommandSequence`='0';

#Chosen Power From The Gods
UPDATE `war_world`.`buff_commands` SET `CommandName`='ModifyAP', `PrimaryValue`='5', `SecondaryValue`=NULL, `InvokeOn`='3', `RetriggerInterval`='1000' WHERE `Entry`='8358' and`CommandID`='0' and`CommandSequence`='0';

#Kotbs Bellow Commands
UPDATE `war_world`.`buff_commands` SET `CommandName`='ModifyAP', `PrimaryValue`='5', `SecondaryValue`=NULL, `InvokeOn`='3', `RetriggerInterval`='1000' WHERE `Entry`='8048' and`CommandID`='0' and`CommandSequence`='0';


#SW leading shots
UPDATE `war_world`.`buff_commands` SET `SecondaryValue`='15' WHERE `Entry`='3566' and`CommandID`='0' and`CommandSequence`='0';
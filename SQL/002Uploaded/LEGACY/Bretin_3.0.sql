#Riposte
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval`='1000' WHERE `Entry`='528' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`ability_damage_heals` SET `Undefendable`=NULL, `StatDamageScale`=NULL WHERE `Entry`='528' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';
UPDATE `war_world`.`ability_damage_heals` SET `MaxDamage`='239' WHERE `Entry`='528' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';

#Bleed em Out
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageFrom`='DualWield', `WeaponDamageScale`='1.35', `StatDamageScale`='1' WHERE `Entry`='3306' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';

#Taunt
UPDATE `war_world`.`buff_commands` SET `CommandID`='0', `CommandSequence`='1', `EventIDString`='ReceivingDamage' WHERE `Entry`='1360' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `CommandID`='0', `CommandSequence`='1', `EventIDString`='ReceivingDamage' WHERE `Entry`='1671' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `CommandID`='0', `CommandSequence`='1', `EventIDString`='ReceivingDamage' WHERE `Entry`='8010' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `CommandID`='0', `CommandSequence`='1', `EventIDString`='ReceivingDamage' WHERE `Entry`='8322' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `CommandID`='0', `CommandSequence`='1', `EventIDString`='ReceivingDamage' WHERE `Entry`='9005' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `CommandID`='0', `CommandSequence`='1', `EventIDString`='ReceivingDamage' WHERE `Entry`='9322' and`CommandID`='1' and`CommandSequence`='0';

#Removal of RoR experimental
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='2ff6b634-8b2b-11e6-b8e9-00ff0731187a';
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='c573ac96-9bb3-11e6-8286-00ff0731187a';

#WH openers
UPDATE `war_world`.`ability_commands` SET `AttackingStat`='1' WHERE `Entry`='8096' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`ability_commands` SET `AttackingStat`='1' WHERE `Entry`='8091' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`ability_commands` SET `AttackingStat`='1' WHERE `Entry`='8098' and`CommandID`='0' and`CommandSequence`='0';

#WE openers
UPDATE `war_world`.`ability_commands` SET `AttackingStat`='1' WHERE `Entry`='9406' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`ability_commands` SET `AttackingStat`='1' WHERE `Entry`='9401' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`ability_commands` SET `AttackingStat`='1' WHERE `Entry`='9411' and`CommandID`='0' and`CommandSequence`='0';

#BG punt
DELETE FROM `war_world`.`ability_modifiers` WHERE `ability_modifiers_ID`='71e8cc2c-e4ba-11e6-a489-000c29d63948';
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='590d8b8c-e4ba-11e6-a489-000c29d63948';

#WH Bullet duration
UPDATE `war_world`.`buff_infos` SET `Duration`='1800' WHERE `Entry`='8084';
UPDATE `war_world`.`buff_infos` SET `Duration`='1800' WHERE `Entry`='8089';
UPDATE `war_world`.`buff_infos` SET `Duration`='1800' WHERE `Entry`='8099';

#WE Kiss duration
UPDATE `war_world`.`buff_infos` SET `Duration`='1800' WHERE `Entry`='9402';
UPDATE `war_world`.`buff_infos` SET `Duration`='1800' WHERE `Entry`='9407';
UPDATE `war_world`.`buff_infos` SET `Duration`='1800' WHERE `Entry`='9412';

#AM lifetap tactic
DELETE FROM `war_world`.`ability_commands` WHERE `Entry`='9291' and`CommandID`='0' and`CommandSequence`='0';
DELETE FROM `war_world`.`ability_damage_heals` WHERE `Entry`='9291' and`Index`='0' and`ParentCommandID`='0' and`ParentCommandSequence`='0';
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='f53a8306-8ed9-11e6-b1c5-00ff0731187a';
DELETE FROM `war_world`.`ability_modifiers` WHERE `ability_modifiers_ID`='acbed9b9-8ed9-11e6-b1c5-00ff0731187a';
DELETE FROM `war_world`.`ability_modifiers` WHERE `ability_modifiers_ID`='e81cde7f-8e7d-11e6-a1bb-00ff0731187a';
UPDATE `war_world`.`buff_infos` SET `Silent`='1' WHERE `Entry`='9291';

#Shaman lifetap tactic
DELETE FROM `war_world`.`ability_commands` WHERE `Entry`='1955' and`CommandID`='0' and`CommandSequence`='0';
DELETE FROM `war_world`.`ability_damage_heals` WHERE `Entry`='1955' and`Index`='0' and`ParentCommandID`='0' and`ParentCommandSequence`='0';
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='f53a836f-8ed9-11e6-b1c5-00ff0731187a';
DELETE FROM `war_world`.`ability_modifiers` WHERE `ability_modifiers_ID`='acbedb4d-8ed9-11e6-b1c5-00ff0731187a';
DELETE FROM `war_world`.`ability_modifiers` WHERE `ability_modifiers_ID`='e81d051d-8e7d-11e6-a1bb-00ff0731187a';
UPDATE `war_world`.`buff_infos` SET `Silent`='1' WHERE `Entry`='1955';

#WL Nature's Bond
UPDATE `war_world`.`ability_damage_heals` SET `MaxDamage`='785' WHERE `Entry`='9164' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';

#WL Furious Mending
DELETE FROM `war_world`.`ability_modifiers` WHERE `ability_modifiers_ID`='3b24f8d7-4caf-11e7-b4d3-000c29d63948';
DELETE FROM `war_world`.`ability_modifiers` WHERE `ability_modifiers_ID`='b9f155d5-4cac-11e7-b4d3-000c29d63948';

#Murderous Wrath
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `ability_modifiers_ID`) VALUES ('9320', 'Murderous Wrath', '9320', 'Murderous Wrath', '1', '0', 'ModifyArmorPenFactor', '25', 'b50f4834-e5a3-11e8-b358-5a000199677e');
UPDATE `war_world`.`ability_damage_heals` SET `ArmorResistPenFactor`=NULL WHERE `Entry`='9320' and`Index`='0' and`ParentCommandID`='0' and`ParentCommandSequence`='0';

#Leonine Frenzy
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits`=NULL WHERE `Entry`='9194' and`Index`='1' and`ParentCommandID`='1' and`ParentCommandSequence`='0';

#Stalker
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('9215', 'Stalker', '0', '0', 'MasterPetModifyStat', '67', '-75', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('9215', 'Stalker', 'Tactic', '1', '1', '1');

#DO THIS ONE ONLY IF YOU APPLIED THE 2 (STALKER) ABOVE!
UPDATE `war_world`.`buff_commands` SET `EventIDString`='PetEvent' WHERE `Entry`='9215' and`CommandID`='0' and`CommandSequence`='0';


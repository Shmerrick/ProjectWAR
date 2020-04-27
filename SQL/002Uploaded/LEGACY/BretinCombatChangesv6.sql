#Shining Blade
UPDATE `war_world`.`buff_infos` SET `Duration` = '5' WHERE (`Entry` = '3014');

#Gift of Savagery
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL WHERE (`Entry` = '3050') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Potent Enchantments
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '540' WHERE (`Entry` = '3461') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '540' WHERE (`Entry` = '9047') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

UPDATE `war_world`.`buff_infos` SET `TypeString` = 'Hex' WHERE (`Entry` = '3461');

#Crashing Wave
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '9028') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '9028') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

#Turrets
UPDATE `war_world`.`abilities` SET `WeaponNeeded` = '1' WHERE (`Entry` = '1511');
UPDATE `war_world`.`abilities` SET `WeaponNeeded` = '1' WHERE (`Entry` = '1518');
UPDATE `war_world`.`abilities` SET `WeaponNeeded` = '1' WHERE (`Entry` = '1526');

#Forked Lancing
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '9289') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `Target`, `EffectRadius`, `EffectSource`, `MaxTargets`, `IsDelayedEffect`) VALUES ('9237', 'Radiant Lance', '2', '0', 'DealDamage', 'Enemy', '15', 'Enemy', '2', '1');
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `CastTimeDamageMult`, `StatUsed`, `StatDamageScale`, `ResourceBuild`, `HatredScale`, `HealHatredScale`) VALUES ('9237', '0', 'Radiant Lance', '53', '399', 'Spiritual', '2', '0', '2', '9', '1', '1', '1', '1');
UPDATE `war_world`.`ability_modifiers` SET `ModifierCommandName` = 'AddAbilityCommand', `PrimaryValue` = '1' WHERE (`ability_modifiers_ID` = '7fa9e69a-8b2b-11e6-b8e9-00ff0731187a');

#Chaotic Agitation
UPDATE `war_world`.`ability_modifiers` SET `TargetCommandID` = '1' WHERE (`ability_modifiers_ID` = '7faa6bc1-8b2b-11e6-b8e9-00ff0731187a');
UPDATE `war_world`.`ability_modifiers` SET `TargetCommandID` = '0' WHERE (`ability_modifiers_ID` = '7faa6b5b-8b2b-11e6-b8e9-00ff0731187a');
UPDATE `war_world`.`ability_modifier_checks` SET `ID` = '1' WHERE (`ability_modifier_checks_ID` = '2ff712f1-8b2b-11e6-b8e9-00ff0731187a');

#Sacrifical Stab
UPDATE `war_world`.`ability_commands` SET `Entry` = '0', `AbilityName` = 'Sacrificial Stab OLD' WHERE (`Entry` = '9428') and (`CommandID` = '2') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_commands` SET `SecondaryValue` = '1' WHERE (`Entry` = '9428') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`ability_commands` SET `SecondaryValue` = '1' WHERE (`Entry` = '9428') and (`CommandID` = '1') and (`CommandSequence` = '1');
UPDATE `war_world`.`ability_commands` SET `IsDelayedEffect` = '1' WHERE (`Entry` = '9428') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_commands` SET `IsDelayedEffect` = '1' WHERE (`Entry` = '9428') and (`CommandID` = '1') and (`CommandSequence` = '1');

UPDATE `war_world`.`ability_damage_heals` SET `Entry` = '0', `DisplayEntry` = '0', `Name` = 'Sacrificial Stab OLD' WHERE (`Entry` = '9428') and (`Index` = '0') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'RawHealing' WHERE (`Entry` = '9428') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'RawHealing' WHERE (`Entry` = '9428') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');

#You Weren't Using Dat (AP)
UPDATE `war_world`.`ability_commands` SET `PrimaryValue` = '50' WHERE (`Entry` = '1966') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`ability_commands` SET `Target` = 'Enemy', `EffectRadius` = '30' WHERE (`Entry` = '1966') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`ability_commands` SET `FromAllTargets` = '1' WHERE (`Entry` = '1966') and (`CommandID` = '0') and (`CommandSequence` = '1');

#Immaculate Defense
UPDATE `war_world`.`buff_infos` SET `MaxCopies` = NULL WHERE (`Entry` = '613');

#Run Away
UPDATE `war_world`.`buff_infos` SET `MaxStack` = '3' WHERE (`Entry` = '3571');
UPDATE `war_world`.`buff_infos` SET `UseMaxStackAsInitial` = '1', `EnemyEffectID` = '4' WHERE (`Entry` = '3571');

#Git Em!
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '12', `MaxDamage` = '86' WHERE (`Entry` = '1824') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

#Brutal Pounce
UPDATE `war_world`.`buff_infos` SET `Duration` = '20' WHERE (`Entry` = '9193');

#Hurt's dont it?
UPDATE `war_world`.`abilities` SET `MasteryTree` = NULL, `Specline` = NULL WHERE (`Entry` = '3220');

#Bathing in Blood
UPDATE `war_world`.`buff_infos` SET `CanRefresh` = NULL WHERE (`Entry` = '3260');

#WL Bite
UPDATE `war_world`.`abilities` SET `Cooldown` = '5' WHERE (`Entry` = '41');
UPDATE `war_world`.`abilities` SET `Cooldown` = '5' WHERE (`Entry` = '426');

#Bolstering Enchant
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'RawHealing', `StatUsed` = NULL, `StatDamageScale` = NULL WHERE (`Entry` = '3462') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Finish Em Faster
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `ability_modifiers_ID`) VALUES ('1779', 'Finish \'Em Faster', '1774', 'Wot\'s Da Rush?', '0', '8', 'MultiplyCooldown', '-25', '03c57323-11f5-11e9-b8e1-5a000199677e');

#Loudmouth
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '225' WHERE (`Entry` = '1706') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '225' WHERE (`Entry` = '1706') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Taste of Blood
UPDATE `war_world`.`buff_commands` SET `CommandName` = 'AddDamageBonus', `PrimaryValue` = '15', `SecondaryValue` = NULL WHERE (`Entry` = '3602') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `EventIDString` = 'DealingDamage', `EventCheck` = NULL WHERE (`Entry` = '3602') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `InvokeOn` = NULL WHERE (`Entry` = '3602') and (`CommandID` = '0') and (`CommandSequence` = '0');

#Masterful Treachery
UPDATE `war_world`.`buff_commands` SET `CommandName` = 'AddDamageBonus', `PrimaryValue` = '15', `SecondaryValue` = NULL WHERE (`Entry` = '3222') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `EventIDString` = 'DealingDamage' WHERE (`Entry` = '3222') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `InvokeOn` = NULL WHERE (`Entry` = '3222') and (`CommandID` = '0') and (`CommandSequence` = '0');

#Infernal Pain
UPDATE `war_world`.`ability_damage_heals` SET `MaxDamage` = '246' WHERE (`Entry` = '3939') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '3939') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Grasping Darkness
UPDATE `war_world`.`ability_modifiers` SET `PreOrPost` = '1', `ModifierCommandName` = 'AppendAbilityCommand', `PrimaryValue` = NULL, `BuffLine` = '1' WHERE (`ability_modifiers_ID` = '7faa1b95-8b2b-11e6-b8e9-00ff0731187a');

#Lashing Waves - Implementation
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('8599', 'Lashing Waves', 'Tactic', '1', '1', '1');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `TargetCommandID`, `PrimaryValue`, `BuffLine`, `ability_modifiers_ID`) VALUES ('8599', 'Lashing Waves', '8565', 'Tzeentch\'s Lash', '1', '0', 'SetCommandRadius', '1', '20', '1', '04c4d47f-1258-11e9-b8e1-5a000199677e');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `TypeString`, `MaxStack`, `Duration`, `EnemyEffectID`, `Silent`) VALUES ('9620', 'Tzeentch\'s Lash', 'Hex', '1', '4', '1', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('9620', 'Tzeentch\'s Lash', '0', '0', 'ApplyCC', '8', '5', 'Host', '4');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('8565', 'Tzeentch\'s Lash', '1', '0', 'InvokeBuff', '9620', 'Enemy');
UPDATE `war_world`.`buff_commands` SET `Name` = 'Tzeentch\'s Lash Damage' WHERE (`Entry` = '8565') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_commands` SET `AbilityName` = 'Tzeentch\'s Lash Damage' WHERE (`Entry` = '8565') and (`CommandID` = '0') and (`CommandSequence` = '0');

#Accuracy
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `BuffLine`, `ability_modifiers_ID`) VALUES ('1490', 'Accuracy', '1435', 'Flurry', '1', '0', 'SetCommandRadius', '30', '1', '292b8655-125f-11e9-b8e1-5a000199677e');

#Immaculate Defense
UPDATE `war_world`.`buff_commands` SET `CommandName` = 'ReduceDamage', `PrimaryValue` = '25', `SecondaryValue` = NULL, `InvokeOn` = NULL, `EventIDString` = 'ShieldPass' WHERE (`Entry` = '613') and (`CommandID` = '0') and (`CommandSequence` = '0');

#Flashfire
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval` = '3000' WHERE (`Entry` = '8196') and (`CommandID` = '0') and (`CommandSequence` = '0');

#Winds' Protection
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `Target`, `EffectRadius`) VALUES ('9308', 'Winds\' Protection', '1', '0', 'InvokeBuff', '9621', NULL, 'Group', '100');
UPDATE `war_world`.`ability_damage_heals` SET `ParentCommandID` = '0' WHERE (`Entry` = '9308') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `Entry` = '9621', `CommandID` = '1' WHERE (`Entry` = '9308') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `CommandID` = '0' WHERE (`Entry` = '9308') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `Name` = 'Winds\' Protection - Shield' WHERE (`Entry` = '9621') and (`CommandID` = '1') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxStack`, `Duration`, `Silent`) VALUES ('9621', 'Winds\' Protection - Shield', 'Morale', '1', '15', '1');

#Swirling Vortex
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `BuffLine`, `ability_modifiers_ID`) VALUES ('8601', 'Swirling Vortex', '3613', 'Vortex', '2', '0', 'SwitchDuration', '15', '1', '957adc3a-12d1-11e9-b8e1-5a000199677e');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('8601', 'Swirling Vortex', 'Tactic', '1', '1', '1');

#Potent Enchantment
UPDATE `war_world`.`buff_infos` SET `TypeString` = 'Hex' WHERE (`Entry` = '3461');

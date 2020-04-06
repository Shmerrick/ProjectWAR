UPDATE `war_world`.`abilities` SET `EffectID` = '504' WHERE (`Entry` = '27844');

delete from `war_world`.`ability_commands` where entry = 27844 and PrimaryValue = '27844';
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('27844', 'Last Stand', '0', '0', 'InvokeBuff', '27844', 'Caster');
delete from `war_world`.`ability_commands` where entry = 27844 and PrimaryValue = '28997';
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('27844', 'Last Stand', '1', '0', 'InvokeBuff', '28997', 'Caster');


delete from `war_world`.`ability_damage_heals` where Entry = 27844;
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `StatDamageScale`, `HatredScale`, `HealHatredScale`) VALUES ('27844', '0', 'Last Stand', '0', '0', 'Physical', '0', '0', '1', '1', '1');
delete from `war_world`.`ability_damage_heals` where Entry = 28997;
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `Undefendable`, `HatredScale`, `HealHatredScale`) VALUES ('28997', '0', 'Last Stand Selfkill', '30000', '30000', 'RawDamage', '0', '0', '1', '1', '1');

delete from `war_world`.`ability_modifiers` where entry = 27845;
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `BuffLine`, ability_modifiers_id) VALUES ('27845', 'Last Stand II', '27844', 'Last Stand', '2', '0', 'SwitchDuration', '15', '1', UUID());

delete from `war_world`.`buff_infos` where entry = 27844;
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `Duration`, `Silent`) VALUES ('27844', 'Last Stand', 'Career', '1', '1', '10', '1');
delete from `war_world`.`buff_infos` where entry = 27845;
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`) VALUES ('27845', 'Last Stand II', 'Career', '1', '1');
delete from `war_world`.`buff_infos` where entry = 28997;
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `Duration`, `Interval`) VALUES ('28997', 'Last Stand Selfkill', '1', '1', '10', '10500');

delete from `war_world`.`buff_commands` where entry = 27844;
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`, `BuffLine`) VALUES ('27844', 'Last Stand', '0', '0', 'ReduceDamage', '0', 'Host', '1');
delete from `war_world`.`buff_commands` where entry = 28997;
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `InvokeOn`, `Target`) VALUES ('28997', 'Last Stand Selfkill', '0', '0', 'DamageOverTime', '5', 'Host');




Note: Physical Damage dont set Raw Damage from all Moral Damage abilities to 0. Means if you use last stand you still get full damage of morales. Maybe have to change `ability_damage_heals` from physical to rawdamage.
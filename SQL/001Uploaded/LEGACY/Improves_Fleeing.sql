SET SQL_SAFE_UPDATES = 0;
delete from `war_world`.`ability_modifiers` where entry = 27820;
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `BuffLine`, `ability_modifiers_ID`) VALUES ('27820', 'Improved Flee I', '245', 'Flee', '2', '0', 'SwitchDuration', '15', '1', uuid());

delete from `war_world`.`ability_modifiers` where entry = 27821;
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `BuffLine`, `ability_modifiers_ID`) VALUES ('27821', 'Improved Flee II', '247', 'Panic', '2', '0', 'SwitchDuration', '0', '1', uuid());

delete from `war_world`.`buff_infos` where entry = 27820;
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxStack`, `PersistsOnDeath`) VALUES ('27820', 'Improved Flee I', 'Career', '1', '1');
delete from `war_world`.`buff_infos` where entry = 27821;
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxStack`, `PersistsOnDeath`) VALUES ('27821', 'Improved Flee II', 'Career', '1', '1');

delete from `war_world`.`buff_commands` where entry = 27820 and CommandSequence = 0;
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`) VALUES ('27820', 'Improved Flee I', '0', '0', 'RegisterModifiers', '0', '0', '5');
delete from `war_world`.`buff_commands` where entry = 27820 and CommandSequence = 1;
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `BuffLine`, `NoAutoUse`) VALUES ('27820', 'Improved Flee I', '1', '0', 'AddBuffLine', '1', '1', '1', '0');
delete from `war_world`.`buff_commands` where entry = 27821 and CommandSequence = 0;
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`) VALUES ('27821', 'Improved Flee II', '0', '0', 'RegisterModifiers', '0', '0', '5');
delete from `war_world`.`buff_commands` where entry = 27821 and CommandSequence = 1;
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `BuffLine`, `NoAutoUse`) VALUES ('27821', 'Improved Flee II', '1', '0', 'AddBuffLine', '1', '1', '1', '0');



dont know if the ability_modifiers_ID of ability_modifiers are right !
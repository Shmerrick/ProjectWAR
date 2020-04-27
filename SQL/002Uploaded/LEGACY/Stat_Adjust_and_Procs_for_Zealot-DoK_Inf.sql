INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('10290', 'Barrier VII', 'Career', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `Group`, `MaxStack`, `Duration`, `Silent`) VALUES ('10658', 'Barrier VII Proc', '30', '1', '5', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`, `EventIDString`, `EventChance`, `BuffLine`, `NoAutoUse`) VALUES ('10290', 'Barrier VII', '0', '0', 'InvokeBuff', '10660', 'Host', 'DirectDamageReceived', '3', '1', '0');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `TertiaryValue`, `InvokeOn`, `Target`, `EventIDString`, `BuffLine`, `NoAutoUse`) VALUES ('10658', 'Barrier VII Proc', '0', '0', 'Shield', '20', '267', '0', '1', 'Host', 'ShieldPass', '1', '0');
UPDATE `war_world`.`item_infos` SET `Effects` = '10290' WHERE (`Entry` = '476603');
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `NoCrits`, `HatredScale`, `HealHatredScale`) VALUES ('10553', '1', 'Chill VII Proc', '144', '144', 'Elemental', '0', '0', '1', '1', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `EffectRadius`, `EventIDString`, `EventChance`, `BuffLine`) VALUES ('10127', 'Chill VII', '0', '0', 'InvokeBuff', '10554', '0', 'EventInstigator', '0', 'DirectDamageDealt', '5', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('10553', 'Chill VII Proc', '0', '0', 'DamageOverTime', '7', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('10553', 'Chill VII Proc', '1', '0', 'ModifySpeed', '-46', '5', 'Host', '2');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('10127', 'Chill VII', 'Career', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `Group`, `MaxStack`, `Duration`, `Interval`, `Silent`) VALUES ('10553', 'Chill VII Proc', '30', '1', '9', '3000', '1');
UPDATE `war_world`.`item_infos` SET `Stats` = '0:0;3:37;89:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', `Effects` = '10127' WHERE (`Entry` = '475139');



INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `Target`, `EventIDString`, `EventChance`, `BuffLine`) VALUES ('10067', 'Decay VII', '0', '0', 'DealProcDamage', 'EventInstigator', 'DirectDamageDealt', '5', '1');
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `NoCrits`, `CastPlayerSubID`, `HatredScale`, `HealHatredScale`) VALUES ('10067', '1', 'Decay VII', '267', '267', 'Corporeal', '0', '0', '1', '0', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`, `CanRefresh`, `Silent`) VALUES ('10067', 'Decay VII', '1', '1', '1', '1', '1');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:37;29:1;76:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', `Effects` = '10067' WHERE (`Entry` = '475143');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:37;7:29;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', `Effects` = '10103' WHERE (`Entry` = '475167');


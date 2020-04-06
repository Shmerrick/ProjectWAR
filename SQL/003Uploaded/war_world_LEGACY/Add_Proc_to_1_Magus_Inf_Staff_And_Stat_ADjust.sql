INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `Target`, `EventIDString`, `EventChance`, `BuffLine`) VALUES ('10068', 'Decay VIII', '0', '0', 'DealProcDamage', 'EventInstigator', 'DirectDamageDealt', '5', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`, `CanRefresh`, `Silent`) VALUES ('10068', 'Decay VIII', '1', '1', '1', '1', '1');
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `NoCrits`, `CastPlayerSubID`, `HatredScale`, `HealHatredScale`) VALUES ('10068', '1', 'Decay VIII', '287', '287', 'Corporeal', '0', '0', '1', '0', '1', '1');
UPDATE `war_world`.`item_infos` SET `Effects` = '10068' WHERE (`Entry` = '476604');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:78;0:0;33:3;78:5;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '475140');

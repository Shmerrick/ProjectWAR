
-- removing flame shield, daemonic shield, Prismatic shield, and mork's buffer cooldown
UPDATE `war_world`.`abilities` SET `Cooldown` = '0' WHERE (`Entry` = '8161');
UPDATE `war_world`.`abilities` SET `Cooldown` = '0' WHERE (`Entry` = '9473');
UPDATE `war_world`.`abilities` SET `Cooldown` = '0' WHERE (`Entry` = '9248');
UPDATE `war_world`.`abilities` SET `Cooldown` = '0' WHERE (`Entry` = '1910');

-- Crippling Strikes able to apply on any crit. Currently set to single target. 
UPDATE `war_world_restore`.`buff_commands` SET `EventIDString` = '0', `BuffLine` = '2' WHERE (`Entry` = '3443') and (`CommandID` = '0') and (`CommandSequence` = '0');
-- changed 1k1 blessings buffstring to not stack with sprout carapace
UPDATE `war_world_restore`.`buff_infos` SET `BuffClassString` = 'Morale' WHERE (`Entry` = '9616');

-- shortened the height of kaboom's self punt slightly
UPDATE `war_world`.`ability_knockback_info` SET `Power` = '850' WHERE (`Entry` = '5') and (`Id` = '1');


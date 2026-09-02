-- Normalizes spawn ward columns when they predate the final 08 schema definition.
--
-- Some local databases already contained non-null Ward columns without a default. Inserts that
-- omit Ward would then fail instead of creating an ordinary unwarded spawn. Invalid values are
-- normalized before enforcing the final unsigned, non-null, default-zero definition.

USE `war_world`;

UPDATE `creature_spawns` SET `Ward` = 0 WHERE `Ward` > 5;
ALTER TABLE `creature_spawns`
    MODIFY COLUMN `Ward` tinyint unsigned NOT NULL DEFAULT 0 AFTER `Level`;

UPDATE `instance_creature_spawns` SET `Ward` = 0 WHERE `Ward` > 5;
ALTER TABLE `instance_creature_spawns`
    MODIFY COLUMN `Ward` tinyint unsigned NOT NULL DEFAULT 0 AFTER `Level`;

UPDATE `instance_boss_spawns` SET `Ward` = 0 WHERE `Ward` > 5;
ALTER TABLE `instance_boss_spawns`
    MODIFY COLUMN `Ward` tinyint unsigned NOT NULL DEFAULT 0 AFTER `Level`;

UPDATE `pquest_spawns` SET `Ward` = 0 WHERE `Ward` > 5;
ALTER TABLE `pquest_spawns`
    MODIFY COLUMN `Ward` tinyint unsigned NOT NULL DEFAULT 0 AFTER `Level`;

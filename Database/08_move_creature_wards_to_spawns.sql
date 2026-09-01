-- Moves ward ownership from reusable creature prototypes to concrete creature spawns.
--
-- A prototype can appear at different levels, ranks, and ward tiers in different locations.
-- Ward therefore belongs to the world, instance, boss, or public-quest spawn that creates the
-- runtime creature. All columns default to no ward until a location has authoritative evidence.
--
-- This script also reverses only the 79 prototype changes made by 07. It intentionally leaves
-- older prototype low bits intact as historical packet data; runtime packet generation replaces
-- those bits with the spawn Ward value.

USE `war_world`;

SET @col := (SELECT COUNT(*) FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'creature_spawns'
               AND COLUMN_NAME = 'Ward');
SET @sql := IF(@col = 0,
    'ALTER TABLE `creature_spawns` ADD COLUMN `Ward` tinyint unsigned NOT NULL DEFAULT 0 AFTER `Level`',
    'SELECT ''creature_spawns.Ward already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @col := (SELECT COUNT(*) FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'instance_creature_spawns'
               AND COLUMN_NAME = 'Ward');
SET @sql := IF(@col = 0,
    'ALTER TABLE `instance_creature_spawns` ADD COLUMN `Ward` tinyint unsigned NOT NULL DEFAULT 0 AFTER `Level`',
    'SELECT ''instance_creature_spawns.Ward already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @col := (SELECT COUNT(*) FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'instance_boss_spawns'
               AND COLUMN_NAME = 'Ward');
SET @sql := IF(@col = 0,
    'ALTER TABLE `instance_boss_spawns` ADD COLUMN `Ward` tinyint unsigned NOT NULL DEFAULT 0 AFTER `Level`',
    'SELECT ''instance_boss_spawns.Ward already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

SET @col := (SELECT COUNT(*) FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'pquest_spawns'
               AND COLUMN_NAME = 'Ward');
SET @sql := IF(@col = 0,
    'ALTER TABLE `pquest_spawns` ADD COLUMN `Ward` tinyint unsigned NOT NULL DEFAULT 0 AFTER `Level`',
    'SELECT ''pquest_spawns.Ward already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

UPDATE `creature_protos`
SET `Unk2` = `Unk2` - 1
WHERE `Entry` IN (
    3040, 3649, 3650, 3651, 3659, 6807, 6834, 6842, 6850, 6856, 7358, 8530,
    16078, 16085, 19409, 20756, 20760, 25721, 26812, 26814, 26815, 33172,
    33173, 33180, 33181, 33182, 33401, 41775, 45224, 46325, 46327, 46334,
    47438, 48128, 49164, 52462, 52594, 61598, 61599, 61601, 93692, 93757,
    93814, 93834, 93835, 93836, 93987, 94101, 94102, 94103, 94190, 94192,
    94272, 94273, 94389, 97425, 97435, 97441, 778041, 1000728, 1000731,
    2000684, 2000725, 2000764, 2000765, 2000766, 2000767, 2000772, 2000774,
    10600231
)
  AND (`Unk2` & 7) = 1;

UPDATE `creature_protos`
SET `Unk2` = `Unk2` - 2
WHERE `Entry` IN (6858, 40782, 97420)
  AND (`Unk2` & 7) = 2;

UPDATE `creature_protos`
SET `Unk2` = `Unk2` - 3
WHERE `Entry` IN (99621, 99624)
  AND (`Unk2` & 7) = 3;

UPDATE `creature_protos`
SET `Unk2` = `Unk2` - 4
WHERE `Entry` IN (98657, 98663, 98678, 98843)
  AND (`Unk2` & 7) = 4;

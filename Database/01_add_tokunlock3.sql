-- Adds item_infos.TokUnlock3, required for the third Tome of Knowledge unlock to fire on equip.
--
-- Selects its own database and is safe to re-run, so it can be applied by the "run every NN_ script
-- in order" loop in the README without needing a database argument or erroring on a second pass.

USE `war_world`;

SET @col := (SELECT COUNT(*) FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'item_infos'
               AND COLUMN_NAME = 'TokUnlock3');
SET @sql := IF(@col = 0,
    'ALTER TABLE `item_infos` ADD COLUMN `TokUnlock3` int(11) unsigned NOT NULL DEFAULT 0',
    'SELECT ''item_infos.TokUnlock3 already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

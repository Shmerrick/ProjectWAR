-- Adds indexes to the character tables that are queried on every login and every guild load.
--
-- Why: CharMgr issues one "WHERE CharacterId = ..." query per table when a character is loaded
-- (CharMgr.LoadAdditionalCharacterInfo, plus the item load at CharMgr.cs:1738), and the account's
-- character list and guild roster are looked up the same way. Four of those columns had no index
-- with them as the leading column, so MySQL resolved them with a full table scan:
--
--   EXPLAIN SELECT * FROM characters_items WHERE CharacterId='215';
--   -> type: ALL, possible_keys: NULL, key: NULL, rows: 3224
--
-- That is cheap on a small test database and gets progressively worse with population: every login
-- scans every row of characters_items, which grows at roughly one row per item per character.
--
-- These are additive index-only changes. No data is modified and the base dumps are untouched.
-- Safe to re-run: each statement is skipped if the index already exists.

USE `war_characters`;

-- characters_items: scanned in full on every character load. Highest-impact of the four.
SET @idx := (SELECT COUNT(*) FROM information_schema.STATISTICS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'characters_items'
               AND INDEX_NAME = 'idx_characters_items_characterid');
SET @sql := IF(@idx = 0,
    'ALTER TABLE `characters_items` ADD INDEX `idx_characters_items_characterid` (`CharacterId`)',
    'SELECT ''idx_characters_items_characterid already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- characters_mails: same per-load lookup; grows without bound as mail accumulates.
SET @idx := (SELECT COUNT(*) FROM information_schema.STATISTICS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'characters_mails'
               AND INDEX_NAME = 'idx_characters_mails_characterid');
SET @sql := IF(@idx = 0,
    'ALTER TABLE `characters_mails` ADD INDEX `idx_characters_mails_characterid` (`CharacterId`)',
    'SELECT ''idx_characters_mails_characterid already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- characters: the account's character list is resolved by AccountId at the character-select screen.
SET @idx := (SELECT COUNT(*) FROM information_schema.STATISTICS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'characters'
               AND INDEX_NAME = 'idx_characters_accountid');
SET @sql := IF(@idx = 0,
    'ALTER TABLE `characters` ADD INDEX `idx_characters_accountid` (`AccountId`)',
    'SELECT ''idx_characters_accountid already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

-- guild_members: drives the roster subquery in CharMgr.cs:1537.
SET @idx := (SELECT COUNT(*) FROM information_schema.STATISTICS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'guild_members'
               AND INDEX_NAME = 'idx_guild_members_guildid');
SET @sql := IF(@idx = 0,
    'ALTER TABLE `guild_members` ADD INDEX `idx_guild_members_guildid` (`GuildId`)',
    'SELECT ''idx_guild_members_guildid already present'' AS result');
PREPARE stmt FROM @sql; EXECUTE stmt; DEALLOCATE PREPARE stmt;

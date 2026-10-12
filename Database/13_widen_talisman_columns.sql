-- Widen the three talisman columns from VARCHAR(40) to VARCHAR(255).
-- Server persistence repair, not retail data: no row's content changes.
--
-- Evidence: CharacterItem/Auction/GuildVaultItem.Talismans serialise each socketed talisman as
-- "entry:slot:fused:timer;" -- at most 30 characters (uint, byte, byte, uint). Three untimed
-- talismans ("2005497:0:0:0;" x 3 = 42) or two timed ones (a Unix-time expiry, 23 each = 46)
-- already overflow 40, and 3,229 items have two or more talisman slots (TalismanSlots 2: 2,866,
-- 3: 362, 4: 1 in mythic_src_item_infos). The server runs with STRICT_TRANS_TABLES, so the
-- over-long value failed the save: the whole persistence batch rolled back, and in the per-row
-- fallback an update kept the old talisman string while an insert (an item received socketed,
-- by trade, mail or vault) was never written, so the item was gone after a restart.
-- A slot can carry a fused and an unfused talisman while the fusing window is open, so the
-- ceiling is 8 x 30 = 240 characters.
--
-- Safe to re-run: MODIFY COLUMN to the same definition is a no-op.
USE war_characters;

ALTER TABLE characters_items MODIFY COLUMN Talismans VARCHAR(255) DEFAULT NULL;
ALTER TABLE auctions MODIFY COLUMN Talismans VARCHAR(255) DEFAULT NULL;
ALTER TABLE guild_vault_item MODIFY COLUMN Talismans VARCHAR(255) DEFAULT NULL;

SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = 'war_characters' AND COLUMN_NAME = 'Talismans'
ORDER BY TABLE_NAME;

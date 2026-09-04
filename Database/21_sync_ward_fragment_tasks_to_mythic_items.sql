-- 21_sync_ward_fragment_tasks_to_mythic_items.sql
--
-- Propagates the ward fragment equip tasks into the item table the server actually reads.
--
-- WorldServer loads items from one of two tables, chosen by World.xml:
--
--     ItemService.LoadItem_Info:
--         UseMythicActionCoverageTables = true  -> mythic_src_item_infos
--         UseMythicActionCoverageTables = false -> item_infos
--
-- The shipped configuration is true, so the live server reads mythic_src_item_infos.
-- Scripts 01, 05 and 20 all wrote TokUnlock3 to item_infos only, so on a default install the
-- ward fragment equip tasks were invisible to the server: Item_Info.TokUnlock3 was 0 in memory
-- for every item, ItemsInterface.GrantEquipUnlocks never fired, and no ward fragment was ever
-- awarded for equipping Annihilator, Sentinel, Invader or any other ward set. Diagnosed
-- 2026-09-04 (BUG-033); mythic_src_item_infos had TokUnlock3 populated on 0 of 88,727 rows
-- while item_infos had 1,377.
--
-- The two tables are entry-for-entry identical (88,727 rows each, every Entry matched) and
-- already agree exactly on TokUnlock and TokUnlock2, so copying TokUnlock3 across by Entry
-- introduces no new data. It only moves the mapping script 20 established from client evidence
-- into the table the loader consumes.
--
-- Idempotent: it assigns the same value on every run and only touches rows that differ.

USE `war_world`;

-- Guard: this script is a no-op if the mythic source table is not present on this install.
SET @has_mythic := (
    SELECT COUNT(*) FROM information_schema.tables
    WHERE table_schema = 'war_world' AND table_name = 'mythic_src_item_infos'
);

SET @sql := IF(@has_mythic = 0,
    'SELECT ''mythic_src_item_infos not present; nothing to sync.'' AS Result',
    'UPDATE mythic_src_item_infos m
        JOIN item_infos i ON i.Entry = m.Entry
        SET m.TokUnlock3 = i.TokUnlock3
      WHERE m.TokUnlock3 <> i.TokUnlock3');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verification: both counts must match once this has been applied.
SELECT
    (SELECT COUNT(*) FROM item_infos WHERE TokUnlock3 > 0)             AS item_infos_ward_tasks,
    (SELECT COUNT(*) FROM mythic_src_item_infos WHERE TokUnlock3 > 0)  AS mythic_ward_tasks,
    (SELECT COUNT(*)
       FROM item_infos i
       JOIN mythic_src_item_infos m ON m.Entry = i.Entry
      WHERE m.TokUnlock3 <> i.TokUnlock3)                              AS remaining_mismatches;

-- 58_tome_tactic_buff_infos_mythic_source.sql
--
-- Fixes migration 56, which wrote the 27 tome tactic buff rows to `buff_infos` only.
--
-- Abilities and buffs live in two parallel tables exactly as items do (CLAUDE.md hard rule 1,
-- BUG-033). AbilityMgr picks its source from World.xml: `UseMythicActionCoverageTables` defaults
-- to true (WorldConfigs.cs:65) and is not overridden in the shipped Configs/World.xml, so
-- AbilityMgr.cs:149-151 loads buffs from `mythic_src_buff_infos`. Migration 56's rows were
-- therefore invisible to the running server: AbilityMgr.GetBuffInfo returned null for every tome
-- tactic and TacticsInterface.HandleTactics rejected each one as "Nonexistent tactic", so a
-- purchased tactic could not be slotted.
--
-- The two tables have identical schemas, so the same rows go to both. Migration 56 is left as it
-- is -- it was applied and is not wrong, only incomplete -- and this script makes both tables
-- agree. Any future migration touching ability or buff columns must do the same.
--
-- Re-runnable: DELETE then INSERT over the fixed 15100-15126 range in both tables.

START TRANSACTION;

DELETE FROM `mythic_src_buff_infos` WHERE `Entry` BETWEEN 15100 AND 15126;
INSERT INTO `mythic_src_buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`)
SELECT `Entry`, `Name`, 'Tactic', 1, 1, 1 FROM `abilities` WHERE `Entry` BETWEEN 15100 AND 15126;

-- Re-asserted so a database that skipped 56 still ends up consistent.
DELETE FROM `buff_infos` WHERE `Entry` BETWEEN 15100 AND 15126;
INSERT INTO `buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`)
SELECT `Entry`, `Name`, 'Tactic', 1, 1, 1 FROM `abilities` WHERE `Entry` BETWEEN 15100 AND 15126;

COMMIT;

-- Verification:
--   SELECT COUNT(*) FROM buff_infos WHERE Entry BETWEEN 15100 AND 15126;             -- 27
--   SELECT COUNT(*) FROM mythic_src_buff_infos WHERE Entry BETWEEN 15100 AND 15126;  -- 27

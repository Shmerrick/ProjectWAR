-- 56_tome_tactic_buff_infos.sql
--
-- BUG-116 (continued): the 27 tome tactics existed in `abilities` but had no `buff_infos` row.
-- TacticsInterface.HandleTactics resolves every slotted tactic through AbilityMgr.GetBuffInfo and
-- rejects it with "Nonexistent tactic: <id>" when that returns null, so before this script none of
-- the 27 could be slotted at all even once purchased.
--
-- Shape copied from the career tactics already in this table (e.g. entry 1861 "Sharpened Arrers"):
-- BuffClassString 'Tactic', MaxCopies/MaxStack 1, PersistsOnDeath 1, no duration. A tome tactic is
-- a passive that lives as long as it is slotted, exactly like a career tactic.
--
-- Effects are NOT defined here. The client-side component data (mythic_bin_ability, decoded
-- against abilitydesc.txt) describes fifteen distinct effects across the 27 tactics, all of them
-- conditional on the target's monster type -- damage dealt and taken, defend and crit chance,
-- ability cost, XP gain, morale rate and cooldown rate. The aggro-radius effect is applied
-- directly in AIInterface.GetAttackableUnit; the combat modifiers need a creature-type-conditional
-- buff command that does not exist yet and are tracked as BUG-118.
--
-- Re-runnable: DELETE then INSERT over the fixed 15100-15126 range.

START TRANSACTION;

DELETE FROM `buff_infos` WHERE `Entry` BETWEEN 15100 AND 15126;

INSERT INTO `buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`)
SELECT `Entry`, `Name`, 'Tactic', 1, 1, 1 FROM `abilities` WHERE `Entry` BETWEEN 15100 AND 15126;

COMMIT;

-- Verification:
--   SELECT COUNT(*) FROM buff_infos WHERE Entry BETWEEN 15100 AND 15126;  -- 27

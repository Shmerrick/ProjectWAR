-- 59_tome_tactic_effect_ids.sql
--
-- The 27 tome tactics carry EffectID 0 in both ability tables, but the live server's
-- F_CAREER_PACKAGE_INFO for category 16 puts a non-zero EffectID in the packet. Verified against
-- the official capture corpus (WAR-RE-Toolkit/libs/protocolservices/Packet Logs,
-- "Inevitable City Shaman 40 94 Defense"), where all 27 tome tactic packages appear with:
--
--   package 1-27  ->  tok_infos 6200-6226, abilities 15100-15126, EffectID 1861/1862/1864/1865/
--                     1866/1867/1868/1869/1870 (one per tactic line, three tactics each)
--
-- Those EffectIDs are already present in `mythic_bin_ability` (the client's own ability data, where
-- 15100 reads EffectID 1861), and are the Mythic dev-spreadsheet ability ids for the tome tactic
-- line: 1861 Daemonic, 1862 Beastial, 1864 Giant, 1865 Greenskin, 1866 Chaos, 1867 Mythical,
-- 1868 Man, 1869 Skaven, 1870 Undead. This copies them across so AbilityConstants.EffectID carries
-- the value the packet needs instead of a hardcoded table in the sender.
--
-- Written to BOTH ability tables: UseMythicActionCoverageTables selects between them at load and
-- defaults to true, so `mythic_src_abilities` is the one the running server reads (see BUG-120).
--
-- Re-runnable: an UPDATE ... JOIN keyed on entry.

START TRANSACTION;

UPDATE `mythic_src_abilities` a
  JOIN `mythic_bin_ability` b ON b.`ID` = a.`Entry`
   SET a.`EffectID` = b.`EffectID`
 WHERE a.`Entry` BETWEEN 15100 AND 15126 AND b.`EffectID` > 0;

UPDATE `abilities` a
  JOIN `mythic_bin_ability` b ON b.`ID` = a.`Entry`
   SET a.`EffectID` = b.`EffectID`
 WHERE a.`Entry` BETWEEN 15100 AND 15126 AND b.`EffectID` > 0;

COMMIT;

-- Verification (each must return 9 rows of 3, matching the capture):
--   SELECT EffectID, COUNT(*) FROM mythic_src_abilities WHERE Entry BETWEEN 15100 AND 15126 GROUP BY EffectID;
--   SELECT EffectID, COUNT(*) FROM abilities            WHERE Entry BETWEEN 15100 AND 15126 GROUP BY EffectID;

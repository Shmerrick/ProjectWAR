-- Restores the "equip this item" ward fragment tasks.
--
-- Each ward fragment can be earned by any one of several tasks, and every task has its own
-- Tome unlock. The client's interface/interfacecore/tome/unlockmapping.csv and the server's
-- tok_infos agree on the encoding for Tome section 5: Index is the sigil tier (1 Lesser,
-- 2 Greater, 3 Superior, 4 Excelsior, 5 Supreme) and Flag is (fragment * 10 + task), where
-- fragment 1-5 is boots, gloves, shoulders, helm, chest. Task 0 is the fragment award itself
-- (entries 7600-7624); tasks 1-6 are the alternative ways to earn it.
--
-- The armour tasks come straight from that file:
--
--   task 1  Annihilator (Lesser)  Conqueror (Greater)  Invader (Superior)
--           Warlord (Excelsior)   Sovereign (Supreme)
--   task 2  Doomflayer (Supreme)
--   task 3  Bloodlord (Lesser)    Sentinel (Greater)   Darkpromise (Superior)
--           Warpforged (Supreme)
--
-- TokUnlock3 now carries the task entry rather than the fragment entry: TokService derives the
-- fragment from the task, so granting the task ticks the Tome checkbox *and* awards the
-- fragment. This supersedes 05_restore_invader_superior_ward_unlocks.sql, which set the Invader
-- pieces to the fragment entries (7610-7614) and so never ticked their task.
--
-- Rows are selected by TokUnlock2, the set-completion Tome entry, plus SlotId. That is an exact
-- key: no name matching, and it stays correct where two sets share a per-slot TokUnlock (the
-- Doomflayer and Warpforged helm and shoulder entries collide, their set entries do not).
--
-- Belts, cloaks, jewellery and weapons carry no ward fragment and are excluded by the slot list.
--
-- Safe to re-run.
USE `war_world`;

-- Lesser Ward -----------------------------------------------------------------------------
UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7625 WHEN 21 THEN 7626 WHEN 24 THEN 7627 WHEN 23 THEN 7628 WHEN 20 THEN 7629 END
  WHERE `TokUnlock2` = 10313 AND `SlotId` IN (20,21,22,23,24);   -- Annihilator, task 1

UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7675 WHEN 21 THEN 7676 WHEN 24 THEN 7677 WHEN 23 THEN 7678 WHEN 20 THEN 7679 END
  WHERE `TokUnlock2` = 10322 AND `SlotId` IN (20,21,22,23,24);   -- Bloodlord, task 3

-- Greater Ward ----------------------------------------------------------------------------
UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7630 WHEN 21 THEN 7631 WHEN 24 THEN 7632 WHEN 23 THEN 7633 WHEN 20 THEN 7634 END
  WHERE `TokUnlock2` = 10325 AND `SlotId` IN (20,21,22,23,24);   -- Conqueror, task 1

UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7680 WHEN 21 THEN 7681 WHEN 24 THEN 7682 WHEN 23 THEN 7683 WHEN 20 THEN 7684 END
  WHERE `TokUnlock2` = 10323 AND `SlotId` IN (20,21,22,23,24);   -- Sentinel, task 3

-- Superior Ward ---------------------------------------------------------------------------
UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7635 WHEN 21 THEN 7636 WHEN 24 THEN 7637 WHEN 23 THEN 7638 WHEN 20 THEN 7639 END
  WHERE `TokUnlock2` = 10326 AND `SlotId` IN (20,21,22,23,24);   -- Invader, task 1

UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7685 WHEN 21 THEN 7686 WHEN 24 THEN 7687 WHEN 23 THEN 7688 WHEN 20 THEN 7689 END
  WHERE `TokUnlock2` = 10324 AND `SlotId` IN (20,21,22,23,24);   -- Darkpromise, task 3

-- Excelsior Ward --------------------------------------------------------------------------
UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7640 WHEN 21 THEN 7641 WHEN 24 THEN 7642 WHEN 23 THEN 7643 WHEN 20 THEN 7644 END
  WHERE `TokUnlock2` = 10327 AND `SlotId` IN (20,21,22,23,24);   -- Warlord, task 1

-- Supreme Ward ----------------------------------------------------------------------------
UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7645 WHEN 21 THEN 7646 WHEN 24 THEN 7647 WHEN 23 THEN 7648 WHEN 20 THEN 7649 END
  WHERE `TokUnlock2` = 10328 AND `SlotId` IN (20,21,22,23,24);   -- Sovereign, task 1

UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7670 WHEN 21 THEN 7671 WHEN 24 THEN 7672 WHEN 23 THEN 7673 WHEN 20 THEN 7674 END
  WHERE `TokUnlock2` = 10046 AND `SlotId` IN (20,21,22,23,24);   -- Doomflayer, task 2

UPDATE `item_infos` SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7695 WHEN 21 THEN 7696 WHEN 24 THEN 7697 WHEN 23 THEN 7698 WHEN 20 THEN 7699 END
  WHERE `TokUnlock2` = 10047 AND `SlotId` IN (20,21,22,23,24);   -- Warpforged, task 3

-- Restore the ten sigil task entries the world dump left blank -----------------------------
-- tok_infos carried empty placeholder rows (Section 0, Flag 0) for the Doomflayer and
-- Warpforged armour tasks, so those unlocks resolved to nothing and awarded no fragment.
-- Name, Xp, Section, Index and Flag are copied verbatim from the client's
-- interface/interfacecore/tome/unlockmapping.csv.
REPLACE INTO `tok_infos` (`Entry`, `Name`, `Xp`, `Section`, `Index`, `Flag`, `EventName`, `Rewards`, `Realm`) VALUES
(7670, 'Acquire Doomflayer Boots',      50, 5, 5, 12, '', NULL, NULL),
(7671, 'Acquire Doomflayer Gloves',     50, 5, 5, 22, '', NULL, NULL),
(7672, 'Acquire Doomflayer Shoulders',  50, 5, 5, 32, '', NULL, NULL),
(7673, 'Acquire Doomflayer Helm',       50, 5, 5, 42, '', NULL, NULL),
(7674, 'Acquire Doomflayer Chest',      50, 5, 5, 52, '', NULL, NULL),
(7695, 'Acquire Warpforged Boots',      50, 5, 5, 13, '', NULL, NULL),
(7696, 'Acquire Warpforged Gloves',     50, 5, 5, 23, '', NULL, NULL),
(7697, 'Acquire Warpforged Shoulders',  50, 5, 5, 33, '', NULL, NULL),
(7698, 'Acquire Warpforged Helm',       50, 5, 5, 43, '', NULL, NULL),
(7699, 'Acquire Warpforged Chest',      50, 5, 5, 53, '', NULL, NULL);

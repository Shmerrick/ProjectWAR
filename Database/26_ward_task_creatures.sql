-- 26_ward_task_creatures.sql
--
-- Maps ward fragment task 4 counters to the creatures whose death advances them.
--
-- `fragment_tasks.csv` gives the counter id and threshold but not the target: the target is named
-- only in the task's own `tok_infos` name, e.g. "Kill Lord Slaurith 5 Times". Each row below is
-- that name matched to a `creature_protos` row, tolerating the trailing gender marker (`^M`,
-- `^F`, `^m`, `^n`), which is why `Lord Slaurith^M` resolves to `Lord Slaurith`.
--
-- A counter may name more than one creature -- "Kill Warlock Peenk and/or Korthuk the Raging 12
-- Times" -- so the mapping is many-to-one and any listed creature advances the counter.
--
-- **Only names that resolve to an existing prototype are seeded.** The following are named by a
-- task but have no matching `creature_protos` row, so their counters are deliberately left
-- without a target rather than pointed at a guess:
--
--     Warlock Peenk            (part of AcId 717)
--     Necromancer Malcidious   (part of AcId 718)
--     Seraphine                (AcId 701)
--     Ssyridian Morbidae       (AcId 701)
--     Twin Lectors             (part of AcId 707)
--     "Any Lost Vale Mini Boss" (AcId 720) -- a category, not a single creature
--
-- AcId 701 therefore has no target at all and cannot currently advance. Restore the missing
-- prototypes, or establish the Lost Vale mini-boss set, before filling these in.
--
-- Idempotent: the table is created if absent and every row is REPLACEd.

USE `war_world`;

CREATE TABLE IF NOT EXISTS `ward_task_creatures` (
  `AcId`          SMALLINT UNSIGNED NOT NULL,
  `CreatureEntry` INT UNSIGNED      NOT NULL,
  PRIMARY KEY (`AcId`, `CreatureEntry`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

REPLACE INTO `ward_task_creatures` (AcId, CreatureEntry) VALUES
 (715, 45084),    -- Kill Thar'lgnan 5 Times
 (700, 48112),    -- Kill Lord Slaurith 5 Times
 (703, 2000751),  -- Kill Kaarn the Vanquisher 5 Times
 (706, 64106),    -- Kill Skull Lord 5 Times  (Skull Lord Var'Ithrok)
 (717, 46204),    -- Kill Warlock Peenk and/or Korthuk the Raging 12 Times
 (717, 2000757),  --   second Korthuk prototype
 (718, 48128),    -- Kill Necromancer Malcidious and/or Bartholomeus the Sickly 12 Times
 (707, 52594),    -- Kill Twin Lectors and/or The Bile Lord 12 Times
 (719, 6843),     -- Kill Dralel the Whitefire Matron 8 Times
 (702, 6842);     -- Kill Sarthain the Worldbearer 8 Times

-- Verification: every mapped creature must exist, and the report lists counters still untargeted.
SELECT
    (SELECT COUNT(*) FROM ward_task_creatures) AS mappings,
    (SELECT COUNT(*) FROM ward_task_creatures m
      WHERE NOT EXISTS (SELECT 1 FROM creature_protos c WHERE c.Entry = m.CreatureEntry)) AS broken,
    (SELECT GROUP_CONCAT(w.AcId ORDER BY w.AcId) FROM ward_fragment_tasks w
      WHERE w.TaskNum = 4
        AND w.AcId NOT IN (SELECT AcId FROM ward_task_creatures)) AS task4_without_target;

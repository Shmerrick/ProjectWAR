-- 25_ward_fragment_task_counters.sql
--
-- Seeds the ward fragment task counter bindings from the 1.4.8 client.
--
-- Tasks 4, 5 and 6 of a ward fragment are progress counters -- "Kill Thar'lgnan 5 Times",
-- "Kill 225 RR 45+ Players", "Capture and/or Defend 3 Fortresses". `tok_infos` carries no
-- threshold and no counter reference: the "5 Times" exists only inside the display name.
--
-- The client supplies both, in `interface/interfacecore/tome/sigils/fragment_tasks.csv`:
--
--     fragment id, sigil entry id, task num, AcId, AcId Max
--     6,2,4,717,12      Greater, fragment 1, task 4 -> action counter 717, threshold 12
--     6,2,5,726,225     Greater, fragment 1, task 5 -> action counter 726, threshold 225
--
-- `AcId` is an action counter id and `AcId Max` its completion threshold, which matches the
-- "(0/12)" and "(0/225)" the client renders on those two tasks. The server already speaks this
-- protocol: `TokInterface.SendActionCounterUpdate` emits `F_ACTION_COUNTER_UPDATE(subtype,
-- count)`, used today for bestiary kill counters. Ward AcIds occupy 700-735 and do not collide
-- with any bestiary id.
--
-- `fragments.csv` maps fragment id to (sigil entry, fragment index), which is simply
-- ((fragment id - 1) MOD 5) + 1, so a row resolves to its `tok_infos` entry as
-- Index = sigil entry and Flag = fragment index * 10 + task num.
--
-- All 32 client rows are seeded. Three of them have no `tok_infos` row to award and are stored
-- with TokEntry 0 so the gap is visible rather than silent: sigil 2 fragment 4 task 4 (AcId 704),
-- sigil 3 fragment 4 task 4 (705) and sigil 3 fragment 5 task 4 (709). Their counters still
-- advance and display; they simply cannot complete until the missing rows are restored, in the
-- same way script 20 restored ten empty section 5 placeholders.
--
-- Idempotent: the table is created if absent and every row is REPLACEd.

USE `war_world`;

CREATE TABLE IF NOT EXISTS `ward_fragment_tasks` (
  `AcId`          SMALLINT UNSIGNED NOT NULL,
  `SigilEntry`    TINYINT  UNSIGNED NOT NULL,
  `FragmentIndex` TINYINT  UNSIGNED NOT NULL,
  `TaskNum`       TINYINT  UNSIGNED NOT NULL,
  `TokEntry`      SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  `Threshold`     INT      UNSIGNED NOT NULL DEFAULT 0,
  PRIMARY KEY (`AcId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TEMPORARY TABLE `tmp_ward_tasks` (
  `FragmentId` TINYINT UNSIGNED NOT NULL,
  `SigilEntry` TINYINT UNSIGNED NOT NULL,
  `TaskNum`    TINYINT UNSIGNED NOT NULL,
  `AcId`       SMALLINT UNSIGNED NOT NULL,
  `Threshold`  INT UNSIGNED NOT NULL
);

INSERT INTO `tmp_ward_tasks` (FragmentId, SigilEntry, TaskNum, AcId, Threshold) VALUES
 (1,1,4,715,5),   (1,1,5,721,100),
 (2,1,4,716,5),   (2,1,5,722,20),
 (3,1,4,700,5),   (3,1,5,723,10),
 (4,1,4,703,5),   (4,1,5,724,10),
 (5,1,4,706,5),   (5,1,5,725,10),
 (6,2,4,717,12),  (6,2,5,726,225),
 (7,2,4,718,12),  (7,2,5,727,9),
 (8,2,4,701,12),  (8,2,5,728,9),
 (9,2,4,704,12),  (9,2,5,729,9),
 (10,2,4,707,12), (10,2,5,708,24), (10,2,6,730,3),
 (11,3,4,719,8),  (11,3,5,731,300),
 (12,3,4,720,48), (12,3,5,732,10),
 (13,3,4,702,8),  (13,3,5,733,15),
 (14,3,4,705,8),  (14,3,5,734,15),
 (15,3,4,709,8),  (15,3,5,710,4),  (15,3,6,735,5);

REPLACE INTO `ward_fragment_tasks` (AcId, SigilEntry, FragmentIndex, TaskNum, TokEntry, Threshold)
SELECT t.AcId,
       t.SigilEntry,
       ((t.FragmentId - 1) MOD 5) + 1                                   AS FragmentIndex,
       t.TaskNum,
       IFNULL((SELECT ti.Entry
                 FROM tok_infos ti
                WHERE ti.Section = 5
                  AND ti.`Index` = t.SigilEntry
                  AND ti.Flag    = ((((t.FragmentId - 1) MOD 5) + 1) * 10) + t.TaskNum
                LIMIT 1), 0)                                            AS TokEntry,
       t.Threshold
  FROM `tmp_ward_tasks` t;

DROP TEMPORARY TABLE `tmp_ward_tasks`;

-- Verification: 32 bindings, of which 29 resolve to a Tome entry and 3 are the known gaps.
SELECT COUNT(*)                        AS bindings,
       SUM(TokEntry > 0)               AS resolved,
       SUM(TokEntry = 0)               AS unresolved,
       GROUP_CONCAT(CASE WHEN TokEntry = 0 THEN AcId END ORDER BY AcId) AS unresolved_acids
  FROM `ward_fragment_tasks`;

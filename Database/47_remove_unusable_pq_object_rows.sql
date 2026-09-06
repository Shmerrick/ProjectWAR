-- 47_remove_unusable_pq_object_rows.sql
--
-- Removes public-quest game-object spawn rows that can never do anything, and which make up the
-- bulk of BUG-072's count.
--
-- What these rows are. 2,771 `pquest_spawns` rows carry Type 2 (game object) with a prototype
-- that exists in neither `gameobject_protos` nor `creature_protos`, so `PQuestObjective.Reset`
-- logs "missing gameobject prototype" and spawns nothing. 1,841 of them are attached to **kill**
-- objectives (`pquest_objectives.Type` 2), where a game object could not satisfy the objective
-- even if its prototype existed.
--
-- Why they are safe to delete. Every one of the 88 kill objectives involved still has usable
-- spawn rows without them -- none is left with an empty spawn set. The pattern is clearest in
-- Barony of Nordland, which alone accounts for 1,504 of the rows:
--
--   objective 762 "Norse Plunderer"    kill 8, target creature 3551 -- 8 Type-1 spawns, plus 402 junk
--   objective 764 "Hralgar the Kraken" kill 1, target creature 3548 -- 1 Type-1 spawn,  plus 734 junk
--   objective 1197 "Seeker Cultist"    kill 6, target creature  535 -- 6 Type-1 spawns, plus  53 junk
--   objective 1198 "Baruun the Seeker" kill 1, target creature  538 -- 1 Type-1 spawn,  plus 315 junk
--
-- In each case the Type-1 creature spawn count already equals the objective's kill target exactly,
-- so the quest is fully supplied and the Type-2 rows are duplicated debris on top. They are the
-- same shape as the Bloodherd Champion row removed by migration 46: a kill objective carrying a
-- game-object row pointing at a prototype that does not exist.
--
-- What is deliberately NOT touched. The remaining rows of this kind sit on interaction objectives
-- (`Type` 3 use-object and `Type` 11 destroy-object), where a game object genuinely is the target
-- and the prototype has to be restored rather than the row removed -- as migrations 38, 41, 43 and
-- 46 did for Nursery Slime, Writhing Effigy, the Tomb of the Vulture Lord traps, the Gunbad exit
-- portal and the Khornite Altar. Those stay open under BUG-072.
--
-- Idempotent: the second run matches nothing.
--
-- USE `war_world`;

USE `war_world`;

DELETE s
  FROM `pquest_spawns` s
  JOIN `pquest_objectives` o ON o.`Guid` = s.`Objective`
  LEFT JOIN `gameobject_protos` g ON g.`Entry` = s.`Entry`
 WHERE s.`Type` = 2
   AND g.`Entry` IS NULL
   AND o.`Type` = 2;

-- Verification.
--   pq_object_rows_missing_prototype  -- what remains of BUG-072, all on interaction objectives
--   on_kill_objectives                -- must be 0
--   kill_objectives_left_empty        -- must be 0
SELECT
    (SELECT COUNT(*) FROM pquest_spawns s LEFT JOIN gameobject_protos g ON g.Entry = s.Entry
      WHERE s.Type = 2 AND g.Entry IS NULL)                                        AS pq_object_rows_missing_prototype,
    (SELECT COUNT(*) FROM pquest_spawns s
       JOIN pquest_objectives o ON o.Guid = s.Objective
       LEFT JOIN gameobject_protos g ON g.Entry = s.Entry
      WHERE s.Type = 2 AND g.Entry IS NULL AND o.Type = 2)                         AS on_kill_objectives,
    (SELECT COUNT(*) FROM (
        SELECT o.Guid FROM pquest_objectives o
          JOIN pquest_spawns s0 ON s0.Objective = o.Guid
         WHERE o.Type = 2
         GROUP BY o.Guid
        HAVING COUNT(*) = 0) z)                                                    AS kill_objectives_left_empty;

-- What is left, by objective type, so the remaining BUG-072 work is visible.
SELECT o.Type AS objective_type, COUNT(*) AS rows_remaining, COUNT(DISTINCT s.Entry) AS distinct_prototypes
  FROM pquest_spawns s
  LEFT JOIN pquest_objectives o ON o.Guid = s.Objective
  LEFT JOIN gameobject_protos g ON g.Entry = s.Entry
 WHERE s.Type = 2 AND g.Entry IS NULL
 GROUP BY o.Type
 ORDER BY rows_remaining DESC;

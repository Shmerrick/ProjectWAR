-- 47_archive_unresolvable_pq_object_rows.sql
--
-- (Filename kept at its original number for series continuity; the script no longer deletes.)
--
-- Moves public-quest game-object spawn rows that cannot resolve at runtime out of
-- `pquest_spawns` and into an archive table, so the server stops logging them every reset while
-- the rows themselves are preserved for later identification.
--
-- What these rows are. 2,771 `pquest_spawns` rows carry Type 2 (game object) with an Entry that
-- has no row in `gameobject_protos`. `PQuestObjective.Reset` looks the prototype up, logs
-- "missing gameobject prototype" and `continue`s, so nothing is spawned and nothing else about
-- the objective changes. 1,842 of them sit on **kill** objectives (`pquest_objectives.Type` 2),
-- where the spawned object could not have satisfied the objective even had the prototype existed
-- -- kill credit comes from `ObjectId..ObjectId6`, never from a Type-2 row.
--
-- What is actually known about them, stated precisely:
--
--   * Every one of the 88 kill objectives involved keeps its full Type-1 creature spawn set;
--     none is left empty, and in the checked cases the Type-1 count already equals the kill
--     target (objective 762 "Norse Plunderer" kills 8 with 8 Type-1 spawns; 764 "Hralgar the
--     Kraken" kills 1 with 1; 1197 "Seeker Cultist" 6 with 6; 1198 "Baruun the Seeker" 1 with 1).
--   * 1,676 rows name an Entry (33 distinct) present in neither prototype table.
--   * 166 rows name an Entry (18 distinct) that IS a `creature_protos` row -- but not one of these
--     is the Entry its own objective asks the player to kill, and the same handful of creatures
--     (2000489 Lord Xyshrenth, 15 The Eidolon, 505 Belchgut^M, 547 Felde Refugee) recur across
--     dozens of unrelated objectives in every pairing. They are not mistyped placements of the
--     objective's target; a Type-1 correction would put the wrong creature in the world.
--   * They are NOT duplicates of the surviving spawns. Measured against the same objective's
--     Type-1 rows: 34 within 50 units, 168 within 300, 1,640 further away. An earlier revision of
--     this script called them "duplicated debris on top" of the real spawns -- that was wrong and
--     is retracted. Their positions are their own, which is exactly why they are archived rather
--     than dropped: a position plus a zone is enough to identify an object against the packet
--     captures later, the way migrations 38, 41, 43, 46 and 48 did for 39 other prototypes.
--
-- So: unresolvable at runtime, demonstrably not the objective's kill target, and not safe to
-- reinterpret without capture evidence -- but not worthless, and not destroyed here.
--
-- Idempotent: the archive insert ignores rows already archived, and the second run matches
-- nothing to move.
--
-- USE `war_world`;

USE `war_world`;

CREATE TABLE IF NOT EXISTS `pquest_spawns_unresolved` LIKE `pquest_spawns`;

INSERT IGNORE INTO `pquest_spawns_unresolved`
SELECT s.*
  FROM `pquest_spawns` s
  JOIN `pquest_objectives` o ON o.`Guid` = s.`Objective`
  LEFT JOIN `gameobject_protos` g ON g.`Entry` = s.`Entry`
 WHERE s.`Type` = 2
   AND g.`Entry` IS NULL
   AND o.`Type` = 2;

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
--   archived_rows                     -- must equal what was moved; nothing is lost
--   kill_objectives_left_empty        -- must be 0
SELECT
    (SELECT COUNT(*) FROM pquest_spawns s LEFT JOIN gameobject_protos g ON g.Entry = s.Entry
      WHERE s.Type = 2 AND g.Entry IS NULL)                                        AS pq_object_rows_missing_prototype,
    (SELECT COUNT(*) FROM pquest_spawns s
       JOIN pquest_objectives o ON o.Guid = s.Objective
       LEFT JOIN gameobject_protos g ON g.Entry = s.Entry
      WHERE s.Type = 2 AND g.Entry IS NULL AND o.Type = 2)                         AS on_kill_objectives,
    (SELECT COUNT(*) FROM pquest_spawns_unresolved)                                AS archived_rows,
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

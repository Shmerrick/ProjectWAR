-- 69_pquest_stage_timer_opt_out.sql
--
-- Adds pquest_objectives.NoStageTimer and sets it on Thanquol's Incursion stages I-V.
--
-- WHY. PublicQuest.NextStage arms a Failed timer on every stage that is not
-- QUEST_PROTECT_UNIT or QUEST_SCRIPTED_EVENT, defaulting to TIME_EACH_STAGE (540s) when
-- pquest_objectives.Time is zero. 2748 of the 2766 objective rows in the world database
-- have Time = 0, so that default is load-bearing for essentially every public quest and
-- must not be reinterpreted.
--
-- The Thanquol's Incursion captures, however, show all five numbered stages sending a
-- stage total AND remaining of zero in F_OBJECTIVE_INFO - no countdown at all - while the
-- 300s Setup stage sends 300 / 193. Leaving the 540s default in place would silently fail
-- the public quest partway through the Thanquol fight. The Gunbad captures agree on the
-- shape: Stage I sends 0 and only the boss stage carries a timer (720s for public quest
-- 181, 600s for 507/508/510).
--
-- Rather than change what Time = 0 means for the whole world database, this adds an
-- explicit per-stage opt-out. Default 0 preserves current behaviour everywhere.
--
--   Time = 0, NoStageTimer = 0  ->  engine default (540s)      [every pre-existing row]
--   Time > 0, NoStageTimer = 0  ->  that many seconds
--   NoStageTimer = 1            ->  no countdown, no fail timer

START TRANSACTION;

SET @col := (SELECT COUNT(*) FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'pquest_objectives'
               AND COLUMN_NAME = 'NoStageTimer');
SET @sql := IF(@col = 0,
    'ALTER TABLE pquest_objectives ADD COLUMN NoStageTimer TINYINT UNSIGNED NOT NULL DEFAULT 0 AFTER Time',
    'SELECT 1');
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;

-- Thanquol's Incursion stages I-V. The Setup stage (185010) keeps its captured 300s timer.
UPDATE pquest_objectives
   SET NoStageTimer = 1
 WHERE Entry = 911
   AND Guid BETWEEN 185011 AND 185015;

COMMIT;

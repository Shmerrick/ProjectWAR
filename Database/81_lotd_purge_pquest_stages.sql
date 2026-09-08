-- 81_lotd_purge_pquest_stages.sql
--
-- Repairs what can be repaired on the five Land of the Dead "Purge" public quests, the ones that
-- run when a tomb is invaded.
--
-- WHAT ALREADY EXISTS. pquest_info 595-599, one per invadable instance, all PQType 1 and Type 0
-- (both realms), each in its own zone:
--
--   595 Purge the Tomb of the Stars        241
--   596 Purge the Tomb of the Moon         242
--   597 Purge the Tomb of the Sky          243
--   598 Purge the Tomb of the Sun          244
--   599 Purge the Tomb of the Vulture Lord 179
--
-- 599 is worth noting on its own: the Vulture Lord has a Purge quest, which is the database's own
-- confirmation that it is invadable. It had been excluded from the invadable set on the mistaken
-- reasoning that it carries no glyph entry cost.
--
-- Each carries a "Purge!" stage counting enemy kills and a "Survive!" stage, except the Tomb of the
-- Sky, which is missing its "Survive!" stage entirely. This adds it, copied from its four siblings,
-- and gives all five Survive stages the timer the live tracker shows.
--
-- THE TIMER. The in-game tracker reads "Outlast the Invaders ... or 29:54", so the outlast is thirty
-- minutes. The stage machinery for this already exists and is not being invented here: a stage whose
-- objectives are QUEST_SCRIPTED_EVENT (12) with a non-zero Time is completed automatically by
-- PublicQuest.ScheduleScriptedStageAdvance after that many seconds, and PublicQuest's tracker packet
-- sends Stage.Time as the countdown. Left at Type 0 with Time 0 the Survive stage instead inherits
-- the 540-second TIME_EACH_STAGE default -- a nine-minute stage where the client showed thirty.
--
-- WHAT IS DELIBERATELY NOT CHANGED. The "Defenders Purged" objectives stay at Type 0
-- (QUEST_UNKNOWN). It looks wrong, and QUEST_KILL_PLAYERS (5) looks like the obvious correction, but
-- it is not: PublicQuest.HandleEvent has no QUEST_KILL_PLAYERS case at all, so type 5 would go from
-- "manual, waiting for a driver" to "never handled by anything". Type 0 shares a branch with
-- QUEST_SCRIPTED_EVENT and advances when something calls HandleEvent with the objective's own Guid,
-- which is what an invasion kill handler would do. That handler does not exist yet (BUG-138), so
-- these counters are inert either way -- but Type 0 is the shape that will work once it does.
--
-- WHAT IS STILL MISSING, and needs evidence rather than a guess:
--
--   * The defender's counter. The live tracker shows BOTH sides at once -- "Order Defenders Purged
--     0/6" and "Destruction Invaders Purged 0/6" -- so the objective names encode a role. This
--     database has only "Defenders Purged" rows; there is no "Invaders Purged" objective anywhere.
--   * The realm asymmetry. 595 reads "Order Defenders Purged" while 596-599 read "Destruction
--     Defenders Purged", which would mean each tomb supports invasion from one direction only.
--     Either realm can hold the expedition, so that cannot be right, but whether live used one quest
--     with two objectives or a pair per tomb is not established.
--   * The name. The client tracker header reads "Conflict Within the Tomb - Purge (Normal)", not
--     "Purge the Tomb of the Stars". Whether that is a different quest, a display convention or a
--     rename is unknown.
--
-- None of this is reachable in game yet: invasion itself is unimplemented (BUG-138).

START TRANSACTION;

-- The Tomb of the Sky's missing Survive stage, copied from its siblings. Guid 2515 is the first
-- free id in the block these quests occupy (2503-2514 and 2518-2520 are taken).
DELETE FROM pquest_objectives WHERE Guid = 2515;

INSERT INTO pquest_objectives
    (Guid, Entry, StageName, StageTitle, StageId, Type, Objective, Count, Description,
     ObjectId, ObjectId2, ObjectId3, ObjectId4, TokCompleted, Time, NoStageTimer, NoRespawn,
     RespawnSeconds, SoundId, SoundDelay, SoundIteration, ObjectId5, ObjectId6, ClientObjectiveId)
SELECT 2515, 597, twin.StageName, twin.StageTitle, twin.StageId, twin.Type, twin.Objective,
       twin.Count, twin.Description, twin.ObjectId, twin.ObjectId2, twin.ObjectId3, twin.ObjectId4,
       twin.TokCompleted, twin.Time, twin.NoStageTimer, twin.NoRespawn, twin.RespawnSeconds,
       twin.SoundId, twin.SoundDelay, twin.SoundIteration, twin.ObjectId5, twin.ObjectId6,
       twin.ClientObjectiveId
  FROM pquest_objectives twin
 WHERE twin.Guid = 2506;   -- Tomb of the Moon's "Survive!"

-- Thirty-minute outlast on every Survive stage, driven by the existing scripted-stage scheduler.
UPDATE pquest_objectives
   SET Type = 12,
       Time = 1800
 WHERE Entry BETWEEN 595 AND 599
   AND Objective = 'Survive the Purge';

COMMIT;

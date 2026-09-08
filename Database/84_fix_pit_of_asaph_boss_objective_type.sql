-- 84_fix_pit_of_asaph_boss_objective_type.sql
--
-- Pit of Asaph's final stage cannot complete because its boss objective is typed as a gameobject
-- interaction rather than a kill.
--
-- "Ibehme the Fury of Asaph Destroyed" carries Type 3, QUEST_USE_GO, on both realm copies of the
-- quest (559 Destruction, 891 Order). Ibehme is a creature: entry 93719 has a row in
-- creature_protos, none in gameobject_protos, and is spawned once in zone 191 at
-- (206139, 1504737). Killing her therefore raises QUEST_KILL_MOB, which the stage is not listening
-- for, and the quest sits on its last stage forever.
--
-- Every other Land of the Dead temple boss objective is Type 2 -- Kheiret, Amsu, Amen-Ser, Bahiti
-- Net, Gahije the Invincible -- and each of those bosses is a creature spawned the same way. This
-- brings Ibehme into line with them.
--
-- WHY THIS MATTERS BEYOND ONE QUEST. Pit of Asaph awards the Scarab glyph. It is not the only
-- source -- Pit of Kem Senef awards it too and is intact -- so this was a blocked quest rather than
-- a blocked glyph, which is why the glyph could still be earned.
--
-- NOT FIXED HERE, because it needs placements that do not exist rather than a corrected column:
--
--   93695 Kae Seki          Sedjhet Temple, Stage III boss
--   93698 Doomed Skeleton   Obelisk of Judgement, Stage I
--   93700 Condemned Skeleton Obelisk of Judgement, Stage II
--
-- All three have creature_protos rows and zero creature_spawns anywhere in the database, so those
-- two quests cannot complete. Both glyphs they award have another intact source: Nikosi Temple also
-- gives Reed, and The Quarry of Bone also gives Scroll.

START TRANSACTION;

UPDATE pquest_objectives
   SET Type = 2
 WHERE Entry IN (559, 891)
   AND ObjectId = 93719
   AND Type = 3;

COMMIT;

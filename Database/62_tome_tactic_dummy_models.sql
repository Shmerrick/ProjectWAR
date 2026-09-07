-- 62_tome_tactic_dummy_models.sql
--
-- Migration 60 gave the tome tactic dummies Model1 = 999, copied from the "Practice Target" proto
-- (36987). That was wrong: 999 is a prop/effect model, not a creature body -- the protos using it
-- are things like "Demon Circle", "Unadorned Disk", "Book" and "Broom". The dummies spawned and
-- were targetable, but rendered nothing. Practice Target is not spawned anywhere in the world, so
-- copying it carried no evidence that the model displays.
--
-- Replaced with models taken from creatures of the matching CreatureType that are actually spawned
-- in the world, so each is known to render and looks like its line. Spawn counts in brackets are
-- how many live placements already use that model.
--
-- The control dummy deliberately keeps a normal humanoid body: it must look ordinary while having
-- CreatureType 0, so it can serve as the baseline for measuring the +-5% effects.
--
-- Re-runnable: plain UPDATEs keyed on entry.

START TRANSACTION;

UPDATE `creature_protos` SET `Model1` = 1100 WHERE `Entry` = 999500; -- Daemonic  (Blackheart Warhound, 355)
UPDATE `creature_protos` SET `Model1` = 1058 WHERE `Entry` = 999501; -- Beastial  (Aatu, 736)
UPDATE `creature_protos` SET `Model1` = 1080 WHERE `Entry` = 999502; -- Giant     (Bubor, 11)
UPDATE `creature_protos` SET `Model1` = 1216 WHERE `Entry` = 999503; -- Greenskin ('Eadcraka, 2681)
UPDATE `creature_protos` SET `Model1` = 1017 WHERE `Entry` = 999504; -- Chaos     (Altered Gor, 513)
UPDATE `creature_protos` SET `Model1` = 1155 WHERE `Entry` = 999505; -- Mythical  (Alethar, 204)
UPDATE `creature_protos` SET `Model1` = 1218 WHERE `Entry` = 999506; -- Man       (Abelhard Lankdorf, 3422)
UPDATE `creature_protos` SET `Model1` = 1015 WHERE `Entry` = 999507; -- Skaven    (Arsqueek Two-Claws, 601)
UPDATE `creature_protos` SET `Model1` = 1718 WHERE `Entry` = 999508; -- Undead    (Amenemhetum's Aid, 935)
UPDATE `creature_protos` SET `Model1` = 1218 WHERE `Entry` = 999509; -- Control   (ordinary human body, CreatureType 0)

COMMIT;

-- Verification (no dummy may keep a prop model):
--   SELECT Entry, Name, Model1, CreatureType FROM creature_protos
--    WHERE Entry BETWEEN 999500 AND 999509 ORDER BY Entry;
--   SELECT COUNT(*) FROM creature_protos WHERE Entry BETWEEN 999500 AND 999509 AND Model1 IN (0, 999); -- 0

-- 78_restore_order_lotd_pq_objectives.sql
--
-- Gives the twelve Order-side Land of the Dead public quests the objectives they are missing, so
-- six glyphs stop being unearnable for Order.
--
-- THE PROBLEM. Every LOTD temple PQ exists twice in pquest_info, once per realm, with identical
-- Name, PinX, PinY and PQAreaId and differing only in Type -- 1 for Order, 2 for Destruction.
-- (Pit of Asaph's Destruction row carries Type 0; the pairing is unambiguous from the name and pin
-- regardless.) The Destruction rows 550-563 all carry their objectives. Of the Order rows,
-- only two do:
--
--   886 Sedjhet Temple        3 objectives, final stage awards 7970 Reed Glyph (Order)
--   887 Obelisk of Judgement  3 objectives, final stage awards 7962 Scroll Glyph (Destruction)
--
-- The other twelve -- 888-899 -- have zero. A PQ with no objectives can never reach a final stage,
-- so its TokCompleted never fires. Six of the ten Order glyphs had no source anywhere in the
-- database as a result: Vulture (7971), Ankhra (7974), Scarab (7975), Vase (7976), Riverbarge
-- (7977) and Skull (7979).
--
-- THE TEMPLATE IS ALREADY HERE. 886 and 887 are byte-for-byte copies of their twins except for two
-- columns: a Guid in a fresh 100000+ range, and TokCompleted moved to the other realm's glyph.
-- 886 copies 556 and shifts 7960 -> 7970; 887 copies 558 and shifts 7972 -> 7962. The glyph blocks
-- are 7960-7969 for Destruction and 7970-7979 for Order, ten entries in the same order, so the
-- shift is plus or minus ten. This script does exactly what whoever built 886 and 887 did, for the
-- remaining twelve, rather than inventing objectives.
--
-- Each new row is copied from its Destruction twin and its TokCompleted moved into the Order block:
--
--   888 Aerie of Death           <- 557   7961 -> 7971  Vulture
--   889 Forbidden Vaults         <- 561   7964 -> 7974  Ankhra
--   890 Hall of the Heavens      <- 560   7966 -> 7976  Vase
--   891 Pit of Asaph             <- 559   7965 -> 7975  Scarab
--   892 Pit of Kem Senef         <- 554   7965 -> 7975  Scarab
--   893 Temple of Ualatp         <- 563   7969 -> 7979  Skull
--   894 The Quay of Seftu        <- 562   7967 -> 7977  Riverbarge
--   895 The Library of Zandri    <- 553   7966 -> 7976  Vase
--   896 Tombs of the Bitter Wind <- 555   7964 -> 7974  Ankhra
--   897 The Quarry of Bone       <- 552   7962 -> 7972  Scroll
--   898 Nikosi Temple            <- 550   7960 -> 7970  Reed
--   899 The Carrion Nest         <- 551   7961 -> 7971  Vulture
--
-- WHAT THIS DOES NOT FIX. Ten of these twelve PQs still have no creature spawns, so they cannot be
-- completed yet either -- see BUG-134. This removes one of the two reasons the Order glyphs are
-- unreachable; the other needs placement data that does not exist in any dump. Restoring the
-- objectives first is still worth doing: it is the half that can be sourced from data already here,
-- and the spawns have nothing to attach to until the objectives exist.
--
-- Guids are 110000 + (Entry - 888) * 10 + n, which is deterministic, collision-free against the
-- current maximum of 185015 only because nothing occupies 110000-110119, and re-runnable.

START TRANSACTION;

DELETE FROM pquest_objectives WHERE Entry BETWEEN 888 AND 899;

INSERT INTO pquest_objectives
    (Guid, Entry, StageName, StageTitle, StageId, Type, Objective, Count, Description,
     ObjectId, ObjectId2, ObjectId3, ObjectId4, TokCompleted, Time, NoStageTimer, NoRespawn,
     RespawnSeconds, SoundId, SoundDelay, SoundIteration, ObjectId5, ObjectId6, ClientObjectiveId)
SELECT
    110000 + (pair.OrderEntry - 888) * 10
           + ROW_NUMBER() OVER (PARTITION BY pair.OrderEntry ORDER BY twin.Guid),
    pair.OrderEntry,
    twin.StageName, twin.StageTitle, twin.StageId, twin.Type, twin.Objective, twin.Count,
    twin.Description, twin.ObjectId, twin.ObjectId2, twin.ObjectId3, twin.ObjectId4,
    CASE WHEN twin.TokCompleted BETWEEN 7960 AND 7969 THEN twin.TokCompleted + 10
         ELSE twin.TokCompleted END,
    twin.Time, twin.NoStageTimer, twin.NoRespawn, twin.RespawnSeconds, twin.SoundId,
    twin.SoundDelay, twin.SoundIteration, twin.ObjectId5, twin.ObjectId6, twin.ClientObjectiveId
FROM pquest_objectives twin
JOIN (
    SELECT 888 AS OrderEntry, 557 AS DestEntry UNION ALL
    SELECT 889, 561 UNION ALL
    SELECT 890, 560 UNION ALL
    SELECT 891, 559 UNION ALL
    SELECT 892, 554 UNION ALL
    SELECT 893, 563 UNION ALL
    SELECT 894, 562 UNION ALL
    SELECT 895, 553 UNION ALL
    SELECT 896, 555 UNION ALL
    SELECT 897, 552 UNION ALL
    SELECT 898, 550 UNION ALL
    SELECT 899, 551
) AS pair ON pair.DestEntry = twin.Entry;

COMMIT;

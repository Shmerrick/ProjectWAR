-- 23_fix_dungeon_influence_ids.sql
--
-- Points Bastion Stair and Mount Gunbad influence at their own chapters.
--
-- `zone_areas` carries the influence track per realm, and both dungeons had it wrong:
--
--     zone 160 Bastion Stair   DestroInfluenceId 128, OrderInfluenceId 129
--     zone  60 Mount Gunbad    DestroInfluenceId  65, OrderInfluenceId  64
--
-- 128 and 129 are "Chapter 20: Surprise Attack" and "Warcamp: Krung's Scrappin' Spot", both in
-- zone 9 (Nordland) and unrelated to either dungeon. 64 and 65 do not exist in `chapter_infos`
-- at all, and `Player.AddInfluence` returns silently when `ChapterService.GetChapterEntry`
-- misses -- so every point of Gunbad influence was discarded without a log line, and Bastion
-- Stair influence accumulated into two unrelated Nordland bars.
--
-- The real dungeon chapters are the ones `chapter_rewards` is built around, each with three
-- reward tiers of one item per career, which is what the entrance rally master hands out:
--
--     chapter_infos 2  "Chaos & Empire Lands: Bastion Stair"   ZoneId 160   Destruction
--     chapter_infos 6  "Empire & Chaos Lands: Bastion Stair"   ZoneId 160   Order
--     chapter_infos 1  "Greenskin & Dwarf Lands: Mount Gunbad" ZoneId  60   Destruction
--     chapter_infos 5  "Dwarf & Greenskin Lands: Mount Gunbad" ZoneId  60   Order
--
-- The realm of each is given by the name order: the owning realm is named first, matching the
-- convention used throughout `chapter_infos`.
--
-- `pquest_info.ChapterId` is corrected alongside. Despite the column name it holds an influence
-- id, and both dungeons stored the Destruction one for every PQ (128 for all ten Bastion PQs, 65
-- for all nine Gunbad PQs). `PublicQuest.GetInfluenceId` now prefers the per-realm area id and
-- only falls back to this column, but the fallback should still name the right dungeon rather
-- than a Nordland chapter.
--
-- Idempotent: every statement is an assignment to a fixed value.

USE `war_world`;

UPDATE zone_areas SET OrderInfluenceId = 6, DestroInfluenceId = 2 WHERE ZoneId = 160;
UPDATE zone_areas SET OrderInfluenceId = 5, DestroInfluenceId = 1 WHERE ZoneId = 60;

UPDATE pquest_info SET ChapterId = 2 WHERE ZoneId = 160;
UPDATE pquest_info SET ChapterId = 1 WHERE ZoneId = 60;

-- Verification: every id below must resolve to a chapter_infos row in the matching zone.
SELECT a.ZoneId,
       a.AreaName,
       a.OrderInfluenceId,
       (SELECT Name FROM chapter_infos c WHERE c.Entry = a.OrderInfluenceId)  AS order_chapter,
       a.DestroInfluenceId,
       (SELECT Name FROM chapter_infos c WHERE c.Entry = a.DestroInfluenceId) AS destro_chapter
  FROM zone_areas a
 WHERE a.ZoneId IN (60, 160)
 ORDER BY a.ZoneId, a.PieceId;

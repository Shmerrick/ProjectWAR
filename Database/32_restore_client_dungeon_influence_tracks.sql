-- Correct migration 23's confusion between chapter_infos.Entry (database row key)
-- and chapter_infos.InfluenceEntry (the client influence track / reward lookup key).
-- Authority: extracted 1.4.8 client at C:\Users\Admin\Downloads\myps:
-- interface/interfacecore/maps/zone160/influenceids.csv, rows 2-3:
--   Area Number 31, Realm 1 -> 129; Realm 2 -> 128.
-- interface/interfacecore/maps/zone060/influenceids.csv, rows 2-3:
--   Area Number 31, Realm 1 -> 64; Realm 2 -> 65.
-- ChapterService.GetChapterEntry resolves InfluenceEntry, NOT Entry.
-- Leave boss maps 163-166 at zero (migration 30); this only restores the two
-- dungeon proper tracks. Past character awards cannot be safely reassigned:
-- their influence rows do not record which zone generated each point.

USE `war_world`;

START TRANSACTION;
UPDATE zone_areas SET OrderInfluenceId = 129, DestroInfluenceId = 128 WHERE ZoneId = 160 AND AreaId = 31;
UPDATE zone_areas SET OrderInfluenceId = 64, DestroInfluenceId = 65 WHERE ZoneId = 60 AND AreaId = 31;
UPDATE pquest_info SET ChapterId = 128 WHERE ZoneId = 160;
UPDATE pquest_info SET ChapterId = 65 WHERE ZoneId = 60;
COMMIT;

-- Each join must resolve to the same zone as the area, through InfluenceEntry.
SELECT a.ZoneId, a.PieceId, a.OrderInfluenceId, o.ZoneId AS OrderChapterZone,
       a.DestroInfluenceId, d.ZoneId AS DestroChapterZone
FROM zone_areas a
LEFT JOIN chapter_infos o ON o.InfluenceEntry = a.OrderInfluenceId
LEFT JOIN chapter_infos d ON d.InfluenceEntry = a.DestroInfluenceId
WHERE a.ZoneId IN (60, 160)
ORDER BY a.ZoneId, a.PieceId;

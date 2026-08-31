-- Restores the Superior Ward Tome unlock assigned when an Invader armor piece is equipped.
--
-- The client Tome mapping defines one unlock per ward-bearing armor slot:
-- boots 7610, gloves 7611, shoulders 7612, helm 7613, and chest 7614.
-- Invader item sets 4432-4455 contain those five slots plus a belt; belts do not grant a ward fragment.
-- Existing nonzero mappings are preserved so the script is safe to re-run and does not replace custom data.

USE `war_world`;

UPDATE `item_infos`
SET `TokUnlock3` = CASE `SlotId`
    WHEN 22 THEN 7610 -- boots
    WHEN 21 THEN 7611 -- gloves
    WHEN 24 THEN 7612 -- shoulders
    WHEN 23 THEN 7613 -- helm
    WHEN 20 THEN 7614 -- chest
END
WHERE `ItemSet` BETWEEN 4432 AND 4455
  AND `Name` LIKE 'Invader %'
  AND `SlotId` IN (20, 21, 22, 23, 24)
  AND `TokUnlock3` = 0;

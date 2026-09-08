-- 85_restore_truncated_item_names_from_londo.sql
--
-- Repairs four item names that lost their leading characters.
--
-- SOURCE. WAR-RE-Toolkit's `data/database-tables/Londos Server v2/War_Item.sql` -- a dump of
-- Mythic's own server-side `Item` table (ID, Name, Description, ModelID, SlotIndex, ItemTypeID,
-- DPS, Speed, CareerMask, RaceMask, Rarity, TalismanSlotCount, bind flags). It is a real
-- server-side table, not client art data, which matters because item display names are never sent
-- to the client as data: WorldServer writes them into the item packet itself
-- (`World/Objects/Item.cs:512`, `Out.WritePascalString(info.Name)`), so no client file can arbitrate
-- them and this dump is the only independent record we hold.
--
-- 2,617 of its 9,948 rows share an Entry with ours and the names agree on 2,562. Of the 34
-- disagreements, 12 are trailing whitespace, 39 are genuine cross-patch renames, and these four are
-- our value being a strict suffix of Mythic's -- a dropped prefix, not a different name:
--
--     66320   "rought Key"                  <- "Bloodwrought Key"
--     204073  "Flash of Brilliance"         <- "Minor Flash of Brilliance"
--     2000520 "of Geheb: Hondo"             <- "Glyph of Geheb: Hondo"
--     2015458 "tched Axebelt of the Flesh"  <- "Notched Axebelt of the Flesh"
--
-- 204073 is the one that could have been a real rename rather than damage, so it was checked
-- against its own family: 204074 is "Major Flash of Brilliance", which makes "Minor" the missing
-- word rather than an addition.
--
-- Both tables are written. `UseMythicActionCoverageTables` decides which one the server reads
-- (`ItemService.LoadItem_Info`), and the shipped value is true, so writing only `item_infos` would
-- leave the running server showing the corrupt names with nothing logged anywhere.
-- Item data is cached at boot; a restart is required.

UPDATE item_infos             SET Name = 'Bloodwrought Key'             WHERE Entry = 66320   AND Name = 'rought Key';
UPDATE mythic_src_item_infos  SET Name = 'Bloodwrought Key'             WHERE Entry = 66320   AND Name = 'rought Key';

UPDATE item_infos             SET Name = 'Minor Flash of Brilliance'    WHERE Entry = 204073  AND Name = 'Flash of Brilliance';
UPDATE mythic_src_item_infos  SET Name = 'Minor Flash of Brilliance'    WHERE Entry = 204073  AND Name = 'Flash of Brilliance';

UPDATE item_infos             SET Name = 'Glyph of Geheb: Hondo'        WHERE Entry = 2000520 AND Name = 'of Geheb: Hondo';
UPDATE mythic_src_item_infos  SET Name = 'Glyph of Geheb: Hondo'        WHERE Entry = 2000520 AND Name = 'of Geheb: Hondo';

UPDATE item_infos             SET Name = 'Notched Axebelt of the Flesh' WHERE Entry = 2015458 AND Name = 'tched Axebelt of the Flesh';
UPDATE mythic_src_item_infos  SET Name = 'Notched Axebelt of the Flesh' WHERE Entry = 2015458 AND Name = 'tched Axebelt of the Flesh';

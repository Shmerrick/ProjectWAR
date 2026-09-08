-- 85_restore_truncated_item_names_from_londo.sql
--
-- Repairs four item names that lost their leading characters.
--
-- SOURCE, and what it is not. WAR-RE-Toolkit's `data/database-tables/Londos Server v2/War_Item.sql`
-- is a second, independently-assembled copy of item data (ID, Name, Description, ModelID,
-- SlotIndex, ItemTypeID, DPS, Speed, CareerMask, RaceMask, Rarity, TalismanSlotCount, bind flags).
--
-- It is not Mythic's own database, though its content is the best-developed layer of the amalgamated
-- world database, owing to Londo's connection with the Mythic developers (docs/CROSS_REPO.md). The
-- dump itself still carries reverse-engineering marks -- `Unk5`-`Unk24` placeholder columns, typos
-- (`AllowAltApperance`), and a MySQL 8.0.13 header from years after shutdown -- so it corroborates
-- rather than arbitrates. A packet capture outranks it.
--
-- It is worth consulting because item names cannot be checked against the client at all:
-- WorldServer writes the name into the item packet itself (`World/Objects/Item.cs:512`,
-- `Out.WritePascalString(info.Name)`), so the client never stores them.
--
-- 2,617 of its 9,948 rows share an Entry with ours and the names agree on 2,562. Of the 34
-- disagreements, 12 are trailing whitespace, 39 are genuine cross-patch renames, and these four are
-- our value being a strict suffix of theirs -- a dropped prefix, not a different name:
--
--     66320   "rought Key"                  <- "Bloodwrought Key"
--     204073  "Flash of Brilliance"         <- "Minor Flash of Brilliance"
--     2000520 "of Geheb: Hondo"             <- "Glyph of Geheb: Hondo"
--     2015458 "tched Axebelt of the Flesh"  <- "Notched Axebelt of the Flesh"
--
-- Three of the four are self-evidently damaged in our copy regardless of what the other dump says:
-- a name cannot begin mid-word at "rought", "tched" or "of Geheb". For those the other copy only
-- supplies the obvious completion, and the case does not rest on its authority.
--
-- 204073 is weaker and is called out as such. "Flash of Brilliance" is a well-formed name, so it is
-- only suspicious because 204074 is "Major Flash of Brilliance" and potion families run
-- Minor/Major. If that inference is wrong, this one row is wrong; revert it alone.
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

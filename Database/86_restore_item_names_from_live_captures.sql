-- 86_restore_item_names_from_live_captures.sql
--
-- Repairs two item names against the live-server packet captures.
--
-- SOURCE, and why it outranks everything else we have for item names. Item names are never in the
-- client: WorldServer writes the name into the item packet itself (`World/Objects/Item.cs:512`), and
-- the real 1.4.8 server did the same. The 1,027 capture logs in
-- `D:\Repos\Shmerrick\WAR-RE-Toolkit\libs\protocolservices\Packet Logs` therefore contain the live
-- server's own answer, in its own bytes, dated 2013 -- the 1.4.8 era itself. That is primary
-- evidence, not a reconstruction, and it is the only item-name arbiter that exists.
--
-- 24,977 F_GET_ITEM (0xAA) frames across those logs name 1,955 distinct items. The extractor is
-- `tools/captures/extract_item_names.awk`; it reads the entry as a big-endian uint32 at frame offset
-- 10 and the name as the first length-prefixed printable run after it.
--
-- DECODE CONFIDENCE. 1,760 of the extracted entries exist in our table and 1,709 names already
-- agree -- 97.1%, which a wrong offset could not produce. The two rows below are further filtered:
-- the frame's ModelId (offset 14) also matches ours, so the packet is unambiguously about that item
-- rather than a mis-aligned read landing on a neighbouring id, and each entry carries exactly one
-- name across every frame that mentions it.
--
-- Both are also self-evidently wrong in our copy independently of the capture:
--
--     1005031  "unk25"                 -> "Talisman of the Prodigal Fighter"   (a placeholder)
--     2000200  "ry Shroud of Khutep"   -> "Funerary Shroud of Khutep"          (begins mid-word)
--
-- The captures separately confirm three of migration 85's four repairs -- 66320 "Bloodwrought Key",
-- 2015458 "Notched Axebelt of the Flesh", and 204073 "Minor Flash of Brilliance", which that
-- migration had flagged as resting only on inference from its sibling 204074. It no longer does.
--
-- ELEVEN FURTHER DISAGREEMENTS ARE NOT ACTED ON HERE. Entries 197307, 206124, 435296, 2000124,
-- 2000132, 5500821, 5501221, 5501460, 5757174, 5758904 and 5758932 pass the same Entry+ModelId test
-- but carry names wholly unlike ours ("Sovereign Grudgecoat of the Unbreakable" against "Mantle of
-- the Fireborn"). A wholly different name is the signature both of a real disagreement and of a
-- decode landing on the wrong item, and one shared ModelId does not separate them, since many items
-- share art. They are recorded in BUG-148 for review rather than applied.
--
-- Both tables are written; `UseMythicActionCoverageTables` decides which the server reads and ships
-- true. Item data is cached at boot, so a restart is required.

UPDATE item_infos            SET Name = 'Talisman of the Prodigal Fighter' WHERE Entry = 1005031 AND Name = 'unk25';
UPDATE mythic_src_item_infos SET Name = 'Talisman of the Prodigal Fighter' WHERE Entry = 1005031 AND Name = 'unk25';

UPDATE item_infos            SET Name = 'Funerary Shroud of Khutep' WHERE Entry = 2000200 AND Name = 'ry Shroud of Khutep';
UPDATE mythic_src_item_infos SET Name = 'Funerary Shroud of Khutep' WHERE Entry = 2000200 AND Name = 'ry Shroud of Khutep';

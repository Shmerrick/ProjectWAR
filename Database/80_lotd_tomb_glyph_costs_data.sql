-- 80_lotd_tomb_glyph_costs_data.sql
--
-- Fills in what each Land of the Dead tomb charges to enter. Migration 79 created the table and
-- left it empty because the costs were not established; they are now, from the client.
--
-- THE SOURCE. interface/interfacecore/maps/zone191/mappoints.xml is the Necropolis of Zandri zone
-- map, and it carries both halves of the system, exactly as the in-game tip says it does ("Lairs,
-- the Glyphs they require for entry, and the Public Quests that award those Glyphs can all be found
-- on the Land of the Dead zone map"). Each <landmark> tomb lists the glyphs it needs, and each
-- <publicQuest> lists the glyph it awards:
--
--   <landmark iname="Tomb of the Stars"> <glyphs> 1, 2, 3 </glyphs>
--   <landmark iname="Tomb of the Sky">   <glyphs> 4, 5, 6 </glyphs>
--   <landmark iname="Tomb of the Moon">  <glyphs> 7, 8    </glyphs>
--   <landmark iname="Tomb of the Sun">   <glyphs> 9, 10   </glyphs>
--   <landmark iname="Tomb of the Vulture Lord">  -- no <glyphs> element at all
--
-- TRANSLATING THE CLIENT'S NUMBERING. Those numbers are the map's own, not Tome entries. They
-- resolve through the public quests, because each PQ appears in both the map (with its glyph
-- number) and pquest_objectives (with its TokCompleted), so the two can be joined on the PQ:
--
--   client 1  -> 7960 Reed        (Sedjhet Temple 556, Nikosi Temple 550)
--   client 2  -> 7961 Vulture     (Aerie of Death 557, The Carrion Nest 551)
--   client 4  -> 7962 Scroll      (Obelisk of Judgment 558, The Quarry of Bone 552)
--   client 5  -> 7964 Ankhra      (Forbidden Vaults 561, Tombs of the Bitter Wind 555)
--   client 7  -> 7965 Scarab      (Pit of Asaph 559, Pit of Kem Senef 554)
--   client 8  -> 7966 Vase        (Hall of the Heavens 560, The Library of Zandri 553)
--   client 9  -> 7967 Riverbarge  (The Quay of Seftu 562)
--   client 20 -> 7969 Skull       (Temple of Ualatp 563)
--
-- Two numbers are left over on the tomb side, 3 and 6, and exactly two glyphs are left over on the
-- Tome side: Horse (7963) and Scorpion (7968). Those are the two awarded by the ROAMING public
-- quests -- Amsu's Charge, The Assault of Nekh Akhet and Ricci's Raiders -- which have no map entry
-- precisely because they roam. So client 3 = Horse and client 6 = Scorpion, and every one of the
-- ten is accounted for with none spare.
--
-- The "20" on Temple of Ualatp is a typo for 10 in the client's own file: 10 is required by the
-- Tomb of the Sun and awarded by nothing else, 20 is required by nothing, and Ualatp's
-- TokCompleted is 7969 Skull, which is glyph 10's slot.
--
-- This mapping was arrived at independently of the report that prompted it and agrees with it on
-- all ten.
--
-- STORED BY INDEX, NOT ENTRY. Each glyph exists twice -- 7960-7969 Destruction, 7970-7979 Order,
-- the same ten in the same order -- so a 0-based index is realm-agnostic and one row serves both.
--
--   0 Reed   1 Vulture   2 Scroll   3 Horse     4 Ankhra
--   5 Scarab 6 Vase      7 Riverbarge 8 Scorpion 9 Skull
--
-- THE VULTURE LORD IS DELIBERATELY ABSENT. Its landmark carries no <glyphs> element, and live
-- footage shows a player's tracker still holding every glyph inside it. A tomb with no rows here is
-- not gated, which is the correct outcome either way; if a cost is ever established, add it.

START TRANSACTION;

DELETE FROM lotd_tomb_glyph_costs WHERE TombZoneId IN (241, 242, 243, 244);

INSERT INTO lotd_tomb_glyph_costs (TombZoneId, GlyphIndex, Count) VALUES
    -- Tomb of the Stars: Reed, Vulture, Horse
    (241, 0, 1), (241, 1, 1), (241, 3, 1),
    -- Tomb of the Sky: Scroll, Ankhra, Scorpion
    (243, 2, 1), (243, 4, 1), (243, 8, 1),
    -- Tomb of the Moon: Scarab, Vase
    (242, 5, 1), (242, 6, 1),
    -- Tomb of the Sun: Riverbarge, Skull
    (244, 7, 1), (244, 9, 1);

COMMIT;

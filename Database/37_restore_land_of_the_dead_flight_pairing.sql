-- 37_restore_land_of_the_dead_flight_pairing.sql
--
-- Makes the Land of the Dead flight destination reachable at all.
--
-- Authority: WAR-RE-Toolkit/libs/protocolservices/Packet Logs/
-- MECHANIC_orderflymaster_NecropoleOFZandri(LoD).txt.gz, F_INTERACT_RESPONSE #9. Packet
-- ordinals are 1-based across both directions (tools/validation/Read-OfficialPackets.ps1).
--
-- That packet is 0x0A, a destination count of 0x1C = 28, then 28 fixed 8-byte records shaped
-- [id:2][pairing:1][price:2][zone:2][available:1]. Its final record is
--
--     00 3D  64  0B B8  00 BF  01
--     id 61  pairing 100  price 3000  zone 191  available
--
-- Two fields in zone_infos disagreed with that.
--
-- 1. Pairing was 4, and must be 100.
--
--    WorldServer writes zone_infos.Pairing straight into that pairing byte. The client keeps
--    two disjoint ranges: the three ordinary pairings 1-3, and the expansion map regions
--    starting at GameData.ExpansionMapRegion.FIRST. EA_InteractionFlightMasterWindow's
--    GetNewDataAndSort discards any destination that is in neither --
--
--        if( (flightData.pairing <= NUM_PAIRINGS and flightData.pairing > 0) or
--            (flightData.pairing >= ExpansionMapRegion.FIRST and <= ExpansionMapRegion.LAST) )
--
--    -- and 4 is above NUM_PAIRINGS (3) and below FIRST. So the row was thrown away before it
--    reached the map, which is why the Tomb Kings tab was empty and travel was impossible even
--    for a realm that held the expedition. That FIRST is 100 is corroborated inside the client
--    independently of the capture: ea_worldmapwindow/source/pairingview.lua:249 indexes string
--    ids as LABEL_EXPANSION_MAP_REGION_100 + index - ExpansionMapRegion.FIRST, and
--    worldview.lua:14 names the button EA_Window_WorldMapWorldViewPairingButton100.
--
-- 2. Price was 0, and must be 3000.
--
--    Read from the same record; WorldServer both advertises and charges zone_infos.Price.
--
-- Only zone 191 is touched. Other zones' pairing values disagree with that capture in places
-- (Reikwald 110 is sent as pairing 0 there but is 2 here), but those destinations are not
-- discarded by the client and are not what this change is about, so they are left alone.
--
-- Idempotent: fixed assignments.

USE `war_world`;

UPDATE zone_infos
   SET Pairing = 100,
       Price   = 3000
 WHERE ZoneId = 191;

-- Verification: expect pairing 100, price 3000, and one enabled taxi row per realm.
SELECT z.ZoneId,
       z.Name,
       z.Pairing,
       z.Price,
       (SELECT COUNT(*) FROM zone_taxis t WHERE t.ZoneID = 191 AND t.Enable = 1) AS enabled_taxis
  FROM zone_infos z
 WHERE z.ZoneId = 191;

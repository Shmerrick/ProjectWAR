-- 31_bastion_stair_zone_type_and_portals.sql
--
-- Corrects two things script 24 got wrong about Bastion Stair, both reported from testing.
--
-- 1. zone_infos.Type
--
--    Zone 160 was Type 0 while Mount Gunbad, the same kind of dungeon, is Type 4. That matters
--    beyond labelling: Player.Teleport(zoneID, ...) contains
--
--        if (destination.Type < 4 && !string.IsNullOrEmpty(InstanceID)) -> leave the instance
--
--    so every in-dungeon portal ejected the player from their realm instance. Type 4 is the
--    realm-instanced public dungeon type; only Hunter's Vale (50) and Mount Gunbad (60) carry it,
--    while the group instances -- Tomb of the Vulture Lord, Bloodwrought Enclave, Bilerot Burrow,
--    The Lost Vale -- are Type 6.
--
-- 2. Internal portals must not open instances
--
--    Script 24 set all eighteen jumps into zone 160 to Type 4. Only three are entrances: 108003496,
--    108003624 and 108856040 all arrive within one unit of (1015781, 1034124, 4984), the entrance
--    hall. The other fifteen are portals *inside* the dungeon that move a player between wings, and
--    with Type 4 each one re-entered InstanceMgr.ZoneIn and opened an instance -- reported in
--    testing as "the left wing also triggered an instance, which isn't right".
--
--    They return to Type 0, a plain teleport. With zone_infos.Type now 4 that teleport keeps the
--    player inside their realm instance rather than ejecting them, which is the behaviour the wings
--    need: the wings are part of the same map, and only the four boss rooms (163-166) are instances.
--
-- Not fixed here, and still open: several of those fifteen arrivals put the player through the
-- floor or outside the map. Their coordinates come from the base dump unchanged, so that is a
-- separate data problem, not a consequence of the jump type.
--
-- Idempotent: fixed assignments.

USE `war_world`;

UPDATE zone_infos SET Type = 4 WHERE ZoneId = 160;

UPDATE zone_jumps
   SET Type = 0
 WHERE ZoneId = 160
   AND Entry NOT IN (108003496, 108003624, 108856040);

SELECT (SELECT Type FROM zone_infos WHERE ZoneId = 160)                                   AS zone_type,
       (SELECT COUNT(*) FROM zone_jumps WHERE ZoneId = 160 AND Type = 4)                  AS entrances,
       (SELECT COUNT(*) FROM zone_jumps WHERE ZoneId = 160 AND Type = 0)                  AS internal_portals;

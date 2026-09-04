-- Restores the Bilerot Burrow instance entry and location-scoped ward tier.
--
-- Two independent 1.4.8 packet captures initialize players at local zone pins
-- (27213, 25133, 13421) with heading 22. The shipped destination is displaced
-- into invalid geometry, causing the client to fall out of bounds.
--
-- Bilerot is a group instance (jump type 6), and its combatants use Greater
-- Ward. Ward 2 is assigned to concrete zone/instance spawns so reused creature
-- prototypes remain unaffected elsewhere in the world.

USE `war_world`;

UPDATE `zone_jumps`
SET `WorldX` = 1501773,
    `WorldY` = 1040941,
    `WorldZ` = 13421,
    `WorldO` = 22,
    `Enabled` = 1,
    `Type` = 6,
    `InstanceID` = 196
WHERE `Entry` = 168899368
  AND `ZoneId` = 196;

UPDATE `instance_creature_spawns`
SET `Ward` = 2
WHERE `ZoneID` = 196;

UPDATE `instance_boss_spawns`
SET `Ward` = 2
WHERE `ZoneID` = 196
  AND `InstanceID` = 196;

UPDATE `creature_spawns`
SET `Ward` = 2
WHERE `ZoneID` = 196
  AND `Enabled` = 1;

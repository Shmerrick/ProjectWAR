-- Restores the retail Bilerot Burrow death-release destination for Destruction.
--
-- Official 1.4.8 Bilerot captures switch a released Destruction player to
-- zone 161 (the Inevitable City). The existing zone-196 respawn row instead
-- points back inside the dungeon. Reuse the capital's authoritative respawn
-- coordinates and use InZoneID to make zone 161 the actual destination.

USE `war_world`;

UPDATE `zone_respawns` AS `bilerot`
JOIN `zone_respawns` AS `capital`
  ON `capital`.`RespawnID` = 6
 AND `capital`.`ZoneID` = 161
 AND `capital`.`Realm` = 2
SET `bilerot`.`PinX` = `capital`.`PinX`,
    `bilerot`.`PinY` = `capital`.`PinY`,
    `bilerot`.`PinZ` = `capital`.`PinZ`,
    `bilerot`.`WorldO` = `capital`.`WorldO`,
    `bilerot`.`InZoneID` = 161
WHERE `bilerot`.`RespawnID` = 340
  AND `bilerot`.`ZoneID` = 196
  AND `bilerot`.`Realm` = 2;

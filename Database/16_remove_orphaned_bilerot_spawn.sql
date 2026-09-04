-- Removes one malformed Bilerot Burrow spawn whose creature prototype does not exist.
--
-- The row is the only zone-196 instance spawn without a matching creature_protos
-- entry. Official 1.4.8 Bilerot captures contain no creature at this exact
-- coordinate, so substituting a guessed prototype would create an unauthentic mob.

USE `war_world`;

DELETE FROM `instance_creature_spawns`
WHERE `Instance_spawns_ID` = '19601260'
  AND `Entry` = 10505036
  AND `ZoneID` = 196
  AND `WorldX` = 1501431
  AND `WorldY` = 1041540
  AND `WorldZ` = 13271;

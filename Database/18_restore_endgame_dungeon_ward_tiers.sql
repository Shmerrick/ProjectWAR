-- Restores location-scoped ward requirements for the two Destruction city
-- dungeons and The Lost Vale.
--
-- Lesser Ward is required in city dungeons. Greater Ward is required in
-- The Lost Vale and cumulatively satisfies Lesser Ward encounters.
-- Creature rank and the client difficulty-mask field remain independent.

USE `war_world`;

UPDATE `instance_creature_spawns`
SET `Ward` = CASE
    WHEN `ZoneID` IN (195, 196) THEN 1
    WHEN `ZoneID` = 260 THEN 2
    ELSE `Ward`
END
WHERE `ZoneID` IN (195, 196, 260);

UPDATE `instance_boss_spawns`
SET `Ward` = CASE
    WHEN `ZoneID` IN (195, 196) THEN 1
    WHEN `ZoneID` = 260 THEN 2
    ELSE `Ward`
END
WHERE `ZoneID` IN (195, 196, 260)
  AND `InstanceID` = `ZoneID`;

-- Retain the same ward if one of these maps is deliberately loaded as a
-- shared world zone for diagnostics or recovery.
UPDATE `creature_spawns`
SET `Ward` = CASE
    WHEN `ZoneID` IN (195, 196) THEN 1
    WHEN `ZoneID` = 260 THEN 2
    ELSE `Ward`
END
WHERE `ZoneID` IN (195, 196, 260)
  AND `Enabled` = 1;

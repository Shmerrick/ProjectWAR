USE `war_world`;

-- Bastion Stair's base map is a shared PvE zone. Only its four boss maps
-- (zones 163-166) are instanced. Preserve the dormant instance definitions so
-- existing boss/lockout data is not destroyed, but stop routing base-map jumps
-- through InstanceMgr.
UPDATE `zone_infos`
SET `Type` = 0
WHERE `ZoneId` = 160
  AND `Type` BETWEEN 4 AND 6;

UPDATE `zone_jumps`
SET `Type` = 0,
    `InstanceID` = NULL
WHERE `ZoneId` = 160
  AND `Type` BETWEEN 4 AND 6;

-- Every arrival into a boss encounter must stay on the instance path. Two
-- Skull Lord Var'Ithrok jumps were incorrectly left as ordinary teleports.
UPDATE `zone_jumps`
SET `Type` = 6,
    `InstanceID` = `ZoneId`
WHERE `ZoneId` BETWEEN 163 AND 166
  AND (`Type` <> 6 OR `InstanceID` IS NULL);

-- The base-map population was copied into instance_creature_spawns and then
-- disabled in creature_spawns. Re-enable only the 195 exact coordinate matches;
-- do not enable the zone's 455 unrelated historical rows.
UPDATE `creature_spawns` AS `world_spawn`
INNER JOIN `instance_creature_spawns` AS `instance_spawn`
    ON `instance_spawn`.`ZoneID` = 160
   AND `world_spawn`.`ZoneId` = `instance_spawn`.`ZoneID`
   AND `world_spawn`.`Entry` = `instance_spawn`.`Entry`
   AND `world_spawn`.`WorldX` = `instance_spawn`.`WorldX`
   AND `world_spawn`.`WorldY` = `instance_spawn`.`WorldY`
   AND `world_spawn`.`WorldZ` = `instance_spawn`.`WorldZ`
SET `world_spawn`.`Enabled` = 1
WHERE `world_spawn`.`Enabled` = 0;

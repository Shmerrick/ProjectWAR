-- Fucked up and forgot to readd order entrance to Lost Vale
INSERT INTO `war_world`.`zone_jumps` VALUES (211812584, 260, 1409240, 1588500, 5892, 1000, 1, 4, 260);

-- Faction change for beastmen in Lost Vale. Were passive mobs.
UPDATE `war_world`.`creature_spawns` SET `Faction` = 3 WHERE `Guid` = 564360;
UPDATE `war_world`.`creature_spawns` SET `Faction` = 3 WHERE `Guid` = 564358;
UPDATE `war_world`.`creature_spawns` SET `Faction` = 3 WHERE `Guid` = 564359;
UPDATE `war_world`.`creature_spawns` SET `Faction` = 3 WHERE `Guid` = 564368;
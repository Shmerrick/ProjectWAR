-- Chaos Waste Rally Master Update for Destruction
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '7098292');
UPDATE `war_world`.`creature_spawns` SET `WorldX` = '1432858', `WorldY` = '784787', `WorldZ` = '14398', `WorldO` = '329' WHERE (`Guid` = '556218');

-- Updates level 55 champion monster locations in destro warcamp
UPDATE `war_world`.`creature_spawns` SET `WorldX` = '1439659', `WorldY` = '784862', `WorldZ` = '14704' WHERE (`Guid` = '556229');
UPDATE `war_world`.`creature_spawns` SET `WorldX` = '1434353', `WorldY` = '783190', `WorldZ` = '14520' WHERE (`Guid` = '556545');

-- Praag. Deleted random non functional rally masters
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1037098');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '7098297');

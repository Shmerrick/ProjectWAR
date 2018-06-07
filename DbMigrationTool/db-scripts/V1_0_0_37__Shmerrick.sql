INSERT INTO `war_world`.`instance_infos` VALUES ('10', 260, 260, 'The Lost Vale', 720, 5, NULL, NULL);

INSERT INTO `war_world`.`zone_jumps` VALUES (272804328, 111, 995617, 844347, 22188, 1000, 1, 0, NULL);
INSERT INTO `war_world`.`zone_jumps` VALUES (211812648, 260, 1409240, 1588379, 5892, 1000, 1, 4, 260);

INSERT INTO `war_world`.`zone_respawns` VALUES (337, 260, 1, 40771, 50401, 3511, 1000, 202);
INSERT INTO `war_world`.`zone_respawns` VALUES (338, 260, 2, 6391, 44903, 4232, 1000, 202);

INSERT INTO `war_world`.`instance_boss_spawns` VALUES ('1', 59211, 0, 43, 0, 260, 260, 332, 2, 1394494, 1583074, 5860, 2844);
INSERT INTO `war_world`.`instance_boss_spawns` VALUES ('2', 6821, 0, 44, 0, 260, 260, 333, 3, 1400575, 1582973, 7830, 1025);

UPDATE `war_world`.`creature_spawns` SET `Enabled` = 0 WHERE `Guid` = 564388;
UPDATE `war_world`.`creature_spawns` SET `Enabled` = 0 WHERE `Guid` = 564418;
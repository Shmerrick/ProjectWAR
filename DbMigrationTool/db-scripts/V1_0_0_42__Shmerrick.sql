/*PURGE GM COMMAND LOG*/
DELETE FROM `war_characters`.`gmcommandlogs`;
/*PURGE OLD INSTANCES*/
DELETE FROM `war_world`.`instance_lockouts`;

/*IC TO MAW*/
INSERT INTO `war_world`.`zone_jumps` VALUES (1168940072, 104, 1430456, 727901, 16728, 16713, 1, 0, NULL);

/*IC TO BLOOD*/
INSERT INTO `war_world`.`zone_jumps` VALUES (168944168, 195, 1571120, 1045651, 11434, 5985, 1, 4, 195);

/*IC TO BILE*/
INSERT INTO `war_world`.`zone_jumps` VALUES (168899368, 196, 1499266, 1046507, 10888, 5000, 1, 4, 196);

/*BILE TO IC*/
INSERT INTO `war_world`.`zone_jumps` VALUES (205522664, 161, 431625, 133563, 16207, 5985, 1, 0, NULL);

/*BILELORD TO IC*/
INSERT INTO `war_world`.`zone_jumps` VALUES (205554024, 161, 431734, 133321, 16209, 5985, 1, 0, NULL);

/*BLOOD TO IC*/
INSERT INTO `war_world`.`zone_jumps` VALUES (204474536, 161, 449178, 151364, 16969, 5985, 1, 0, NULL);

/*BLOOD RESPAWN*/
INSERT INTO `war_world`.`zone_respawns` VALUES (339, 161, 2, 50001, 49569, 17544, 2000, 195);
/*BILE RESPAWN*/
INSERT INTO `war_world`.`zone_respawns` VALUES (340, 161, 2, 50001, 49569, 17544, 2000, 196);

/*ADD INSTANCES*/
INSERT INTO `war_world`.`instance_infos` VALUES (5, 195, 195, 'Bloodwrought Enclave', 720, 5, NULL, NULL);
INSERT INTO `war_world`.`instance_infos` VALUES (6, 196, 196, 'Bilerot Burrow', 720, 5, NULL, NULL);
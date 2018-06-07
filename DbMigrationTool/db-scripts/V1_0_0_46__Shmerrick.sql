/*IC TO BLOOD FIX.
Instance spawn point was in aggro range of monsters. This has been fixed.
*/
DELETE FROM `war_world`.`zone_jumps` WHERE `Entry`=168944168;
INSERT INTO `war_world`.`zone_jumps` VALUES (168944168, 195, 1571122, 1045295, 11434, 5985, 1, 4, 195);
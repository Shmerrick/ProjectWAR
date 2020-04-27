Delete FROM war_world.zone_areas where zoneid in (110, 10, 104, 204, 4, 210);
DELETE FROM `war_world`.`zone_areas` WHERE (`ZoneId` = '4') and (`PieceId` = '30');
DELETE FROM `war_world`.`zone_areas` WHERE (`ZoneId` = '10') and (`PieceId` = '30');
DELETE FROM `war_world`.`zone_areas` WHERE (`ZoneId` = '104') and (`PieceId` = '30');
DELETE FROM `war_world`.`zone_areas` WHERE (`ZoneId` = '110') and (`PieceId` = '30');
DELETE FROM `war_world`.`zone_areas` WHERE (`ZoneId` = '204') and (`PieceId` = '30');
DELETE FROM `war_world`.`zone_areas` WHERE (`ZoneId` = '210') and (`PieceId` = '30');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`) VALUES ('Butcher\'s Pass', '4', '61', '0', '30', '58', '49', '5837');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`) VALUES ('Stonewatch', '10', '61', '0', '30', '58', '49', '5838');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`) VALUES ('The Maw', '104', '1', '0', '30', '122', '113', '5839');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`) VALUES ('Reikwald', '110', '1', '0', '30', '122', '113', '5840');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`) VALUES ('Fell Landing', '204', '61', '0', '30', '186', '177', '5841');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`) VALUES ('Shining Way', '210', '61', '0', '30', '186', '177', '5842');


UPDATE `war_world`.`keep_infos` SET `IsFortress` = '1' WHERE (`KeepId` = '100');
UPDATE `war_world`.`keep_infos` SET `IsFortress` = '1' WHERE (`KeepId` = '101');
UPDATE `war_world`.`keep_infos` SET `IsFortress` = '1' WHERE (`KeepId` = '102');
UPDATE `war_world`.`keep_infos` SET `IsFortress` = '1' WHERE (`KeepId` = '103');
UPDATE `war_world`.`keep_infos` SET `IsFortress` = '1' WHERE (`KeepId` = '104');
UPDATE `war_world`.`keep_infos` SET `IsFortress` = '1' WHERE (`KeepId` = '105');

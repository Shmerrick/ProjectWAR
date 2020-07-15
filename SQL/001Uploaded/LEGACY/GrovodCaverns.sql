INSERT INTO `scenario_infos` VALUES ('2107', 'Grovod Caverns', '1', '40', '4', '12', '7', '4', '137', '5', '1', '0', '0', '0', '137');

DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '260');
DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '261');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('260', '137', '1', '47662', '31084', '8279', '946', '0');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('261', '137', '2', '35299', '30159', '8280', '3014', '0');

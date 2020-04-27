INSERT INTO `scenario_infos` VALUES ('2109', 'Reikland Hills', '1', '40', '0', '12', '1', '4', '134', '5', '1', '1', '0', '0', '134');

DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '242');
DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '243');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('242', '134', '2', '31071', '31104', '6560', '0', '0');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('243', '134', '1', '30474', '42271', '6420', '2200', '0');

DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = '11dc5e47-59b9-11e6-9679-00ff0731887a');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = '21dc5e47-59b9-11e6-9679-00ff0731887a');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = '31dc5e47-59b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2109', '7019', 'Fallen Bridge', 'Flag', '621043', '364903', '5734', '4084', '10', '2', '11dc5e47-59b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2109', '7020', 'The Factory', 'Flag', '625879', '367208', '6199', '4084', '10', '2', '21dc5e47-59b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2109', '7021', 'The Mill', 'Flag', '615971', '362595', '5867', '4084', '10', '2', '31dc5e47-59b9-11e6-9679-00ff0731887a');

DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '254');
DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '255');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('254', '235', '1', '22275', '36816', '7192', '3052', '0');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('255', '235', '2', '43259', '36603', '7207', '1024', '0');

DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b12c5e47-59b9-11e6-9679-00ff0731887a');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b1ac5e47-59b9-11e6-9679-00ff0731887a');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b13c5e47-59b9-11e6-9679-00ff0731887a');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b14c5e47-59b9-11e6-9679-00ff0731887a');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b1bc5e47-59b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2205', '7065', 'The Summoning Tower', 'Flag', '688188', '370079', '7481', '2058', '10', '2', 'b13c5e47-59b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2205', '7066', 'The Commorancy', 'Flag', '688225', '358806', '7193', '3808', '10', '2', 'b14c5e47-59b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2205', '7063', 'The Academy', 'Flag', '688391', '364403', '6758', '1240', '20', '5', 'b1bc5e47-59b9-11e6-9679-00ff0731887a');

DELETE FROM `war_world`.`scenario_infos` WHERE (`ScenarioID` = '2205');
INSERT INTO `war_world`.`scenario_infos` (`ScenarioID`, `Name`, `MinLevel`, `MaxLevel`, `MinPlayers`, `MaxPlayers`, `Type`, `Tier`, `MapID`, `KillPointScore`, `RewardScaler`, `DeferKills`, `Enabled`, `QueueType`, `RegionId`) VALUES ('2205', 'Dragon\'s Bane', '1', '40', '0', '12', '1', '4', '235', '5', '1', '0', '1', '0', '235');

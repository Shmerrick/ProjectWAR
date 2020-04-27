DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '345');
DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '346');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('345', '32', '2', '17657', '22032', '4221', '856', '0');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('346', '32', '1', '15783', '23043', '4760', '1186', '0');


DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b1dc4dcb-59b9-11e6-9679-00ff073118aa');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b1dc4dcb-59b9-11e6-9679-00ff073118ab');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b1dc4dcb-59b9-11e6-9679-00ff073118ac');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2015', '5053', 'The Engine Room', 'Flag', '308074', '185029', '3708', '1420', '15', '2', 'b1dc4dcb-59b9-11e6-9679-00ff073118aa');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2015', '5052', 'The Central Gangway', 'Flag', '309026', '185720', '4199', '1394', '15', '2', 'b1dc4dcb-59b9-11e6-9679-00ff073118ab');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `WorldPosX`, `WorldPosY`, `PosZ`, `Heading`, `PointGain`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2015', '5050', 'The Upper Deck', 'Flag', '310035', '186408', '4769', '3470', '15', '2', 'b1dc4dcb-59b9-11e6-9679-00ff073118ac');

DELETE FROM `war_world`.`scenario_infos` WHERE (`ScenarioID` = '2015');
INSERT INTO `war_world`.`scenario_infos` (`ScenarioID`, `Name`, `MinLevel`, `MaxLevel`, `MinPlayers`, `MaxPlayers`, `Type`, `Tier`, `MapID`, `KillPointScore`, `RewardScaler`, `DeferKills`, `Enabled`, `QueueType`, `RegionId`) VALUES ('2015', 'The Ironclad', '1', '40', '0', '12', '1', '4', '32', '5', '1', '0', '1', '0', '32');

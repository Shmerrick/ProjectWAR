DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '244');
DELETE FROM `war_world`.`zone_respawns` WHERE (`RespawnID` = '245');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('244', '135', '1', '35185', '36189', '19841', '1814', '0');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('245', '135', '2', '30077', '24723', '19836', '3850', '0');

DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b1dc5e47-19b9-11e6-9679-00ff0731887a');
DELETE FROM `war_world`.`scenario_objects` WHERE (`scenario_objects_ID` = 'b1dc5e47-29b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2105', '7003', 'The Atrium', 'Flag', '2', 'b1dc5e47-19b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_objects` (`ScenarioId`, `Identifier`, `ObjectiveName`, `Type`, `PointOverTimeGain`, `scenario_objects_ID`) VALUES ('2105', '7004', 'The Loft', 'Flag', '2', 'b1dc5e47-29b9-11e6-9679-00ff0731887a');
INSERT INTO `war_world`.`scenario_infos` (`ScenarioID`, `Name`, `MinLevel`, `MaxLevel`, `MinPlayers`, `MaxPlayers`, `Type`, `Tier`, `MapID`, `KillPointScore`, `RewardScaler`, `DeferKills`, `Enabled`, `QueueType`, `RegionId`) VALUES ('2105', 'Twisting Tower', '1', '40', '0', '12', '1', '4', '135', '5', '1', '0', '1', '0', '135');

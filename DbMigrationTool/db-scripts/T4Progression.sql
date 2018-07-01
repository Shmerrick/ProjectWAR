CREATE TABLE `war_world`.`rvr_progression` (
  `tier` int(11) DEFAULT NULL,
  `PairingId` int(11) DEFAULT NULL,
  `Description` varchar(80) DEFAULT NULL,
  `BattleFrontId` int(11) NOT NULL,
  `OrderWinProgression` int(11) NOT NULL,
  `DestWinProgression` int(11) NOT NULL,
  `OrderWinReward` int(11) NOT NULL,
  `DestWinReward` int(11) NOT NULL,
  `OrderLossReward` int(11) NOT NULL,
  `DestLossReward` int(11) NOT NULL,
  `RegionId` int(11) NOT NULL,
  `ZoneId` int(11) DEFAULT NULL,
  `DefaultRealmLock` int(11) DEFAULT NULL,
  `ResetProgressionOnEntry` int(11) DEFAULT NULL,
  PRIMARY KEY (`BattleFrontId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('4', '2', 'Chaos Wastes', '1', '5', '2', '0', '0', '0', '0', '11', '103', '2');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`, `ResetProgressionOnEntry`) VALUES ('4', '2', 'Praag', '2', '1', '3', '0', '0', '0', '0', '11', '105', '0', '1');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('4', '2', 'Reikland', '3', '2', '5', '0', '0', '0', '0', '11', '109', '1');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('4', '1', 'Black Crag', '4', '8', '5', '0', '0', '0', '0', '2', '3', '2');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('4', '1', 'Thunder Mountain', '5', '4', '6', '0', '0', '0', '0', '2', '5', '0');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('4', '1', 'Kadrin Valley', '6', '5', '8', '0', '0', '0', '0', '2', '9', '1');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('4', '3', 'Eataine', '7', '8', '2', '0', '0', '0', '0', '4', '209', '1');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('4', '3', 'Dragonwake', '8', '7', '9', '0', '0', '0', '0', '4', '205', '0');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('4', '3', 'Caledor', '9', '2', '8', '0', '0', '0', '0', '4', '203', '2');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`, `ResetProgressionOnEntry`) VALUES ('1', '2', 'Norsca / Nordland', '10', '11', '11', '0', '0', '0', '0', '8', '106', '0', '1');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('1', '1', 'Ekrund / Mt Bloodhorn', '11', '12', '12', '0', '0', '0', '0', '1', '6', '2');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `OrderWinReward`, `DestWinReward`, `OrderLossReward`, `DestLossReward`, `RegionId`, `ZoneId`, `DefaultRealmLock`) VALUES ('1', '3', 'Chrace / Blighted Isle', '12', '10', '10', '0', '0', '0', '0', '3', '206', '1');
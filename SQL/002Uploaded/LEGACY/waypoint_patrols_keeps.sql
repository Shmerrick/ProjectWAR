ALTER TABLE `war_world`.`keep_creatures` 
ADD COLUMN `WaypointGUID` INT NOT NULL AFTER `KeepLord`;
ALTER TABLE `war_world`.`keep_creatures` 
ADD COLUMN `IsPatrol` TINYINT(3) NOT NULL AFTER `KeepLord`,
DROP PRIMARY KEY,
ADD PRIMARY KEY (`X`, `Y`, `Z`);

ALTER TABLE `war_world`.`waypoints` 
CHANGE COLUMN `GUID` `GUID` INT(11) UNSIGNED NOT NULL DEFAULT '0' ,
CHANGE COLUMN `CreatureSpawnGUID` `CreatureSpawnGUID` INT(11) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `GameObjectSpawnGUID` `GameObjectSpawnGUID` INT(11) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `X` `X` INT(11) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `Y` `Y` INT(11) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `Z` `Z` INT(11) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `O` `O` INT(11) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `NextWaypointGUID` `NextWaypointGUID` INT(11) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `war_world`.`waypoints` 
CHANGE COLUMN `GUID` `GUID` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0' ,
CHANGE COLUMN `CreatureSpawnGUID` `CreatureSpawnGUID` SMALLINT(5) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `GameObjectSpawnGUID` `GameObjectSpawnGUID` SMALLINT(5) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `war_world`.`waypoints` 
CHANGE COLUMN `GUID` `GUID` INT(11) UNSIGNED NOT NULL DEFAULT '0' ,
CHANGE COLUMN `CreatureSpawnGUID` `CreatureSpawnGUID` INT(11) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `GameObjectSpawnGUID` `GameObjectSpawnGUID` INT(11) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `war_world`.`waypoints` 
CHANGE COLUMN `CreatureSpawnGUID` `CreatureSpawnGUID` INT(11) UNSIGNED NULL ,
CHANGE COLUMN `X` `X` INT(11) UNSIGNED NOT NULL ,
CHANGE COLUMN `Y` `Y` INT(11) UNSIGNED NOT NULL ,
CHANGE COLUMN `Z` `Z` INT(11) UNSIGNED NOT NULL ,
CHANGE COLUMN `O` `O` INT(11) UNSIGNED NOT NULL ;


INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('26', '209', '1000096', '1000099', '1030595', '1645402', '6606', '190', '0', '1', '26');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('25', '209', '1000105', '1000108', '1067306', '1637120', '5232', '2122', '0', '1', '25');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('27', '205', '1000086', '1000089', '989182', '1639708', '11411', '4078', '0', '1', '27');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('28', '205', '1000074', '1000077', '970052', '1640191', '8103', '6', '0', '1', '28');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('29', '203', '1000051', '1000054', '888619', '1634227', '6042', '1892', '0', '1', '29');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('30', '203', '1000060', '1000062', '931981', '1635047', '7929', '1838', '0', '1', '30');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('16', '109', '777973', '777974', '1428768', '943365', '15928', '904', '0', '1', '16');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('15', '109', '777967', '777968', '1436482', '909799', '17784', '1048', '0', '1', '15');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('17', '105', '777955', '777956', '1445928', '879050', '15779', '1832', '0', '1', '17');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('18', '105', '777961', '777962', '1446682', '834275', '14432', '4050', '0', '1', '18');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('19', '103', '777929', '777928', '1438535', '763712', '12789', '1306', '0', '1', '19');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('20', '103', '777950', '777949', '1449087', '798010', '13552', '2256', '0', '1', '20');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('5', '9', '778111', '778115', '1412447', '848644', '9872', '3904', '0', '1', '5');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('6', '9', '777979', '777980', '1399769', '879244', '9240', '242', '0', '1', '6');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('7', '5', '778192', '778196', '1400495', '932069', '11437', '622', '0', '1', '7');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('8', '5', '778150', '778155', '1373082', '923995', '11923', '2056', '0', '1', '8');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('9', '3', '778251', '1000004', '1395722', '982753', '8386', '474', '0', '1', '9');
INSERT INTO `war_world`.`keep_creatures` (`KeepId`, `ZoneId`, `OrderId`, `DestroId`, `X`, `Y`, `Z`, `O`, `KeepLord`, `IsPatrol`, `WaypointGUID`) VALUES ('10', '3', '1000033', '1000037', '1412633', '1016748', '5590', '3954', '0', '1', '10');


UPDATE `war_world`.`keep_creatures` SET `DestroId` = '778157' WHERE (`X` = '1371612') and (`Y` = '924452') and (`Z` = '12074');
UPDATE `war_world`.`keep_creatures` SET `DestroId` = '778155' WHERE (`X` = '1371531') and (`Y` = '924516') and (`Z` = '12074');
UPDATE `war_world`.`keep_creatures` SET `DestroId` = '778158' WHERE (`X` = '1371336') and (`Y` = '924474') and (`Z` = '12074');


INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26000', '26', '0', '1030595', '1645402', '6606', '190', '2000', '26001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26001', '26', '0', '1030881', '1645028', '6775', '2630', '2000', '26002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26002', '26', '0', '1031573', '1644546', '7237', '2650', '2000', '26003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26003', '26', '0', '1031618', '1643212', '7320', '2040', '2000', '26004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26004', '26', '0', '1032757', '1642591', '7291', '1870', '2000', '26005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26005', '26', '0', '1032522', '1641395', '6924', '1912', '2000', '26006', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26006', '26', '0', '1030769', '1641936', '6520', '790', '2000', '26007', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26007', '26', '0', '1030693', '1640378', '6536', '1992', '2000', '26008', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26008', '26', '0', '1030448', '1639699', '6535', '1810', '2000', '26009', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('26009', '26', '0', '1031440', '1639360', '6536', '3030', '2000', '26000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('25000', '25', '0', '1067306', '1637120', '5232', '2122', '2000', '25001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('25001', '25', '0', '1067344', '1637847', '5282', '3872', '2000', '25002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('25002', '25', '0', '1068407', '1639132', '5536', '3644', '2000', '25003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('25003', '25', '0', '1069319', '1639807', '5728', '788', '2000', '25004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('25004', '25', '0', '1069261', '1641540', '5622', '8', '2000', '25005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('25005', '25', '0', '1068859', '1643564', '5234', '146', '2000', '25000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('28000', '28', '0', '970052', '1640191', '8103', '6', '2000', '28001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('28001', '28', '0', '969642', '1638876', '8176', '1800', '2000', '28002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('28002', '28', '0', '968043', '1638115', '8186', '678', '2000', '28003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('28003', '28', '0', '966967', '1638260', '8105', '924', '2000', '28004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('28004', '28', '0', '965543', '1638329', '8104', '828', '2000', '28005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('28005', '28', '0', '965243', '1639098', '8113', '3866', '2000', '28000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('27000', '27', '0', '989182', '1639708', '11411', '4078', '2000', '27001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('27001', '27', '0', '988560', '1638218', '11443', '1794', '2000', '27002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('27002', '27', '0', '987933', '1637497', '11549', '1460', '2000', '27003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('27003', '27', '0', '987065', '1637052', '11567', '436', '2000', '27004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('27004', '27', '0', '985607', '1636435', '11424', '1218', '2000', '27005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('27005', '27', '0', '984288', '1636066', '11392', '1206', '2000', '27000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('29000', '29', '0', '888619', '1634227', '6042', '1892', '2000', '29001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('29001', '29', '0', '888675', '1635235', '6076', '208', '2000', '29002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('29002', '29', '0', '888007', '1635896', '6254', '542', '2000', '29003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('29003', '29', '0', '888046', '1637498', '6271', '3388', '2000', '29004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('29004', '29', '0', '888234', '1638654', '6064', '3986', '2000', '29005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('29005', '29', '0', '889015', '1639637', '5898', '3656', '2000', '29000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('30000', '30', '0', '931981', '1635047', '7929', '1838', '2000', '30001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('30001', '30', '0', '932598', '1636196', '7928', '3824', '2000', '30002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('30002', '30', '0', '932626', '1636598', '7940', '4068', '2000', '30003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('30003', '30', '0', '931131', '1637620', '8416', '3146', '2000', '30004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('30004', '30', '0', '932686', '1638908', '7936', '3514', '2000', '30005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('30005', '30', '0', '933619', '1639945', '7936', '3684', '2000', '30000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('15000', '15', '0', '1436482', '909799', '17784', '1048', '2000', '15001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('15001', '15', '0', '1437549', '910090', '17797', '3250', '2000', '15002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('15002', '15', '0', '1439053', '909733', '17737', '2290', '2000', '15003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('15003', '15', '0', '1441242', '909629', '17778', '3040', '2000', '15004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('15004', '15', '0', '1442103', '910740', '17790', '3650', '2000', '15000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('16000', '16', '0', '1428768', '943365', '15928', '904', '2000', '16001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('16001', '16', '0', '1430792', '942295', '15889', '2724', '2000', '16002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('16002', '16', '0', '1432063', '943284', '16216', '1970', '2000', '16003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('16003', '16', '0', '1434000', '942697', '16209', '2868', '2000', '16004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('16004', '16', '0', '1434490', '941486', '15927', '2208', '2000', '16005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('16005', '16', '0', '1434074', '940635', '15907', '2164', '2000', '16000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('17000', '17', '0', '1445928', '879050', '15779', '1832', '2000', '17001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('17001', '17', '0', '1446966', '880798', '15728', '3740', '2000', '17002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('17002', '17', '0', '1446850', '881701', '15728', '2732', '2000', '17003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('17003', '17', '0', '1447573', '882347', '15672', '3546', '2000', '17004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('17004', '17', '0', '1447853', '883065', '15704', '3730', '2000', '17005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('17005', '17', '0', '1449110', '883636', '15774', '3186', '2000', '17000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('18000', '18', '0', '1446682', '834275', '14432', '4050', '2000', '18001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('18001', '18', '0', '1446441', '832824', '14416', '1908', '2000', '18002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('18002', '18', '0', '1444211', '831876', '14408', '176', '2000', '18003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('18003', '18', '0', '1443318', '832186', '14408', '1230', '2000', '18004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('18004', '18', '0', '1441445', '831564', '14188', '1230', '2000', '18005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('18005', '18', '0', '1441029', '831120', '14337', '1546', '2000', '18000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19000', '19', '0', '1438535', '763712', '12789', '1306', '2000', '19001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19001', '19', '0', '1439366', '762938', '13321', '2558', '2000', '19002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19002', '19', '0', '1440200', '764528', '13360', '3706', '2000', '19003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19003', '19', '0', '1441181', '764055', '13328', '3726', '2000', '19004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19004', '19', '0', '1441648', '764975', '13216', '3556', '2000', '19005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19005', '19', '0', '1443025', '765706', '13296', '3392', '2000', '19006', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19006', '19', '0', '1443660', '765635', '13336', '3028', '2000', '19007', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19007', '19', '0', '1443500', '767242', '12912', '132', '2000', '19008', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19008', '19', '0', '1443170', '768122', '12739', '234', '2000', '19009', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('19009', '19', '0', '1443314', '768489', '12696', '3852', '2000', '19000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('20000', '20', '0', '1449087', '798010', '13552', '2256', '2000', '20001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('20001', '20', '0', '1448260', '798912', '13592', '498', '2000', '20002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('20002', '20', '0', '1448059', '797667', '14083', '1942', '2000', '20003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('20003', '20', '0', '1447504', '797602', '14089', '1098', '2000', '20004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('20004', '20', '0', '1446961', '798742', '13784', '320', '2000', '20005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('20005', '20', '0', '1445908', '798323', '13590', '524', '2000', '20006', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('20006', '20', '0', '1444530', '798190', '13436', '946', '2000', '20007', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('20007', '20', '0', '1443111', '799074', '13548', '650', '2000', '20000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('5000', '5', '0', '1412447', '848644', '9872', '3904', '2000', '5001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('5001', '5', '0', '1411877', '847364', '9952', '1742', '2000', '5002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('5002', '5', '0', '1412521', '845992', '10032', '1032', '2000', '5003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('5003', '5', '0', '1411487', '845108', '9968', '1486', '2000', '5004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('5004', '5', '0', '1410729', '844136', '9936', '1618', '2000', '5005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('5005', '5', '0', '1410078', '843217', '9872', '1800', '2000', '5000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6000', '6', '0', '1399769', '879244', '9240', '242', '2000', '6001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6001', '6', '0', '1400034', '877861', '9195', '2168', '2000', '6002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6002', '6', '0', '1400555', '877003', '9266', '2400', '2000', '6003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6003', '6', '0', '1400576', '875703', '9312', '2052', '2000', '6004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6004', '6', '0', '1401483', '874597', '9376', '940', '2000', '6005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6005', '6', '0', '1400551', '874776', '9329', '1060', '2000', '6006', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6006', '6', '0', '1400232', '874386', '9288', '2132', '2000', '6007', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6007', '6', '0', '1400455', '872649', '9325', '2132', '2000', '6008', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('6008', '6', '0', '1400524', '871845', '9268', '2132', '2000', '6000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('7000', '7', '0', '1400495', '932069', '11437', '622', '2000', '7001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('7001', '7', '0', '1402186', '929842', '11448', '2472', '2000', '7002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('7002', '7', '0', '1402211', '928244', '11456', '1056', '2000', '7003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('7003', '7', '0', '1404597', '928165', '12064', '1016', '2000', '7004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('7004', '7', '0', '1402269', '927921', '11471', '1076', '2000', '7005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('7005', '7', '0', '1402150', '926205', '11448', '1990', '2000', '7006', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('7006', '7', '0', '1400796', '925513', '11448', '1354', '2000', '7007', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('7007', '7', '0', '1400509', '924416', '11456', '1880', '2000', '7000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('8000', '8', '0', '1373082', '923995', '11923', '2056', '2000', '8001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('8001', '8', '0', '1373369', '925772', '11977', '4012', '2000', '8002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('8002', '8', '0', '1373234', '927213', '11928', '110', '2000', '8003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('8003', '8', '0', '1371611', '928024', '11984', '2976', '2000', '8004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('8004', '8', '0', '1373461', '928649', '11928', '3394', '2000', '8005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('8005', '8', '0', '1373390', '929810', '12024', '3620', '2000', '8006', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('8006', '8', '0', '1374217', '931112', '11928', '3704', '2000', '8000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9000', '9', '0', '1395722', '982753', '8386', '474', '2000', '9001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9001', '9', '0', '1398059', '979385', '7026', '2444', '2000', '9002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9002', '9', '0', '1397904', '977977', '7164', '1498', '2000', '9003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9003', '9', '0', '1397044', '977517', '7527', '1064', '2000', '9004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9004', '9', '0', '1396193', '977686', '7768', '1366', '2000', '9005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9005', '9', '0', '1395264', '976911', '7767', '544', '2000', '9006', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9006', '9', '0', '1394564', '977797', '7768', '1386', '2000', '9007', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9007', '9', '0', '1395334', '976898', '7760', '2790', '2000', '9008', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9008', '9', '0', '1398572', '978127', '6971', '3294', '2000', '9009', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9009', '9', '0', '1398626', '975218', '7165', '2046', '2000', '9010', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('9010', '9', '0', '1397393', '972942', '7017', '1732', '2000', '9000', '60');

INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('10000', '10', '0', '1412633', '1016748', '5590', '3954', '2000', '10001', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('10001', '10', '0', '1412194', '1015111', '5618', '1860', '2000', '10002', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('10002', '10', '0', '1411668', '1012053', '5539', '1182', '2000', '10003', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('10003', '10', '0', '1411785', '1010499', '5555', '2082', '2000', '10004', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('10004', '10', '0', '1412674', '1008916', '5543', '2346', '2000', '10005', '60');
INSERT INTO `war_world`.`waypoints` (`GUID`, `CreatureSpawnGUID`, `GameObjectSpawnGUID`, `X`, `Y`, `Z`, `O`, `WaitAtEndMS`, `NextWaypointGUID`, `Speed`) VALUES ('10005', '10', '0', '1412345', '1007866', '5234', '1852', '2000', '10000', '60');










UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='25000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='25001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='25002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='25003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='25004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='25005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26006';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='30005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='30004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='30003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='5000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='5001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='5002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='5003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='5004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='5005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6006';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6007';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='6008';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='7000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='7001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='30001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='30002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='30000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='29005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='29004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='29003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='29002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='29001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='29000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='28005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='28004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='28003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='28002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='28001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='28000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='27005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='27004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='27003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='27002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='27001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='27000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26009';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26008';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='26007';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='20007';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='20006';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='20005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='20004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='20003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='20002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='20001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='20000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19009';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19008';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19007';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19006';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='7002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='7003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='7004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='7005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='7006';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='7007';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='8000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='8001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='8002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='8003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='8004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='8005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='8006';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='19000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='18005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='18004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='18003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='18002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='18001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='18000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='17005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='17004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='17003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='17002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='17001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='17000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='16005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='16004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='10002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='10003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='10004';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='10005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='15000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9005';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9006';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9007';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9008';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9009';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='9010';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='10000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='10001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='16003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='16002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='16001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='16000';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='15001';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='15002';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='15003';
UPDATE `war_world`.`waypoints` SET `Speed`='100' WHERE `GUID`='15004';

UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='5' WHERE `X`='1412447' and`Y`='848644' and`Z`='9872';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='6' WHERE `X`='1399769' and`Y`='879244' and`Z`='9240';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='7' WHERE `X`='1400495' and`Y`='932069' and`Z`='11437';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='8' WHERE `X`='1373082' and`Y`='923995' and`Z`='11923';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='9' WHERE `X`='1395722' and`Y`='982753' and`Z`='8386';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='10' WHERE `X`='1412633' and`Y`='1016748' and`Z`='5590';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='15' WHERE `X`='1436482' and`Y`='909799' and`Z`='17784';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='16' WHERE `X`='1428768' and`Y`='943365' and`Z`='15928';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='17' WHERE `X`='1445928' and`Y`='879050' and`Z`='15779';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='18' WHERE `X`='1446682' and`Y`='834275' and`Z`='14432';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='19' WHERE `X`='1438535' and`Y`='763712' and`Z`='12789';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='20' WHERE `X`='1449087' and`Y`='798010' and`Z`='13552';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='25' WHERE `X`='1067306' and`Y`='1637120' and`Z`='5232';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='26' WHERE `X`='1030595' and`Y`='1645402' and`Z`='6606';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='27' WHERE `X`='989182' and`Y`='1639708' and`Z`='11411';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='28' WHERE `X`='970052' and`Y`='1640191' and`Z`='8103';
UPDATE `war_world`.`keep_creatures` SET `IsPatrol`='1', `WaypointGUID`='29' WHERE `X`='888619' and`Y`='1634227' and`Z`='6042';
SET SQL_SAFE_UPDATES = 0;


##Heavy Chaos HellCannon
UPDATE `war_world`.`creature_protos` SET `Entry` = '86221' WHERE (`Entry` = '13463');
##Heavy Chaos Ram
UPDATE `war_world`.`creature_protos` SET `Entry` = '86222' WHERE (`Entry` = '13465');
##heavy Empire Oil
UPDATE `war_world`.`creature_protos` SET `Entry` = '86211' WHERE (`Entry` = '13426');
##heavy chaos oil
UPDATE `war_world`.`creature_protos` SET `Entry`='86223' WHERE `Entry`='13462';
## Heavy Chaos Tri-Barrel HellCannon
UPDATE `war_world`.`creature_protos` SET `Entry`='86224' WHERE `Entry`='13464';
## Heavy Empire Cannon
UPDATE `war_world`.`creature_protos` SET `Entry`='86212' WHERE `Entry`='13427';
## Heavy Empire  Helblaster
UPDATE `war_world`.`creature_protos` SET `Entry`='86209' WHERE `Entry`='13428';
## Heavy Empire ram
UPDATE `war_world`.`creature_protos` SET `Entry`='86210' WHERE `Entry`='13429';


## Reikwald Setup
DELETE FROM `war_world`.`zone_areas` WHERE (`ZoneId` = '110') and (`PieceId` = '0');
DELETE FROM `war_world`.`zone_areas` WHERE (`ZoneId` = '110') and (`PieceId` = '1');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`) VALUES ('Reikwald', '110', '0', '0', '0', '42', '113', '5660');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`) VALUES ('Reikwald', '110', '1', '0', '1', '42', '113', '5660');



DELETE FROM `war_world`.`pquest_info` WHERE (`Entry` = '9999');
INSERT INTO `war_world`.`pquest_info` (`Entry`, `Name`, `Type`, `Level`, `ZoneId`, `PinX`, `PinY`, `TokDiscovered`, `TokUnlocked`, `ChapterId`, `GoldChestWorldX`, `GoldChestWorldY`, `GoldChestWorldZ`, `PQType`, `PQDifficult`, `Chapter`, `PQTier`, `PQCraftingBag`, `PQAreaId`, `SoundPQEnd`, `RespawnID`) VALUES ('9999', 'Reikwald', '2', '0', '110', '22196', '9788', '0', '0', '0', '0', '0', '0', '2', '3', '22', '4', '4', '30', '0', '0');

DELETE FROM `war_world`.`keep_infos` WHERE (`KeepId` = '100');
INSERT INTO `war_world`.`keep_infos` (`KeepId`, `Name`, `Realm`, `DoorCount`, `ZoneId`, `RegionId`, `X`, `Y`, `Z`, `O`, `PQuestId`, `Race`, `OilX`, `OilY`, `OilZ`, `OilO`, `OilOuterX`, `OilOuterY`, `OilOuterZ`, `OilOuterO`, `RamX`, `RamY`, `RamZ`, `RamO`, `RamOuterX`, `RamOuterY`, `RamOuterZ`, `RamOuterO`, `IsFortress`, `OilOuter1X`, `OilOuter1Y`, `OilOuter1Z`, `OilOuter1O`, `OilOuter2X`, `OilOuter2Y`, `OilOuter2Z`, `OilOuter2O`, `RamOuter1X`, `RamOuter1Y`, `RamOuter1Z`, `RamOuter1O`, `RamOuter2X`, `RamOuter2Y`, `RamOuter2Z`, `RamOuter2O`) VALUES ('100', 'Reikwald', '1', '4', '110', '11', '1431158', '966414', '15955', '2042', '9999', '3', '1431156', '965739', '16455', '2026', '1431197', '961314', '16790', '2030', '1431157', '965556', '15904', '78', '1431205', '961235', '15952', '52', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');

UPDATE `war_world`.`keep_infos` SET `IsFortress` = '1' WHERE (`KeepId` = '100');

DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '554080');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '554081');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '554082');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '554093');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '554094');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '860808');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('554080', '93398', '110', '1416758', '952340', '16846', '3925', '18', '0', '64', '0', '40', '5807', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('554081', '761', '110', '1420212', '953412', '16158', '2616', '20', '0', '67', '0', '40', '10717', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('554082', '761', '110', '1420675', '953908', '16114', '1604', '20', '0', '67', '0', '40', '14230', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('554093', '5576', '110', '1411035', '970978', '17568', '2036', '24', '0', '67', '0', '40', '7070', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('554094', '5576', '110', '1410111', '970989', '17568', '2013', '18', '0', '67', '0', '40', '13007', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('860808', '1000141', '110', '1431252', '958035', '16397', '2212', '0', '0', '0', '0', '0', '0', '0', '1');


DELETE FROM `war_world`.`keep_doors` where ZoneId = 110;
DELETE FROM `war_world`.`keep_doors` WHERE `DoorId`='115343464';
DELETE FROM `war_world`.`keep_doors` WHERE `DoorId`='115343465';
DELETE FROM `war_world`.`keep_doors` WHERE `DoorId`='115343466';
DELETE FROM `war_world`.`keep_doors` WHERE `DoorId`='115343528';
DELETE FROM `war_world`.`keep_doors` WHERE `DoorId`='115343720';
DELETE FROM `war_world`.`keep_doors` WHERE `DoorId`='115343784';
DELETE FROM `war_world`.`keep_doors` WHERE `DoorId`='115343848';
DELETE FROM `war_world`.`keep_doors` WHERE `DoorId`='115445800';
INSERT INTO `war_world`.`keep_doors` (`KeepId`, `Number`, `ZoneId`, `GameObjectId`, `DoorId`, `X`, `Y`, `Z`, `O`, `TeleportX1`, `TeleportY1`, `TeleportZ1`, `TeleportO1`, `TeleportX2`, `TeleportY2`, `TeleportZ2`, `TeleportO2`) VALUES ('100', '1', '110', '100', '115343464', '1431157', '965691', '15987', '989', '1431158', '965772', '15915', '32', '1431158', '965585', '15904', '2058');
INSERT INTO `war_world`.`keep_doors` (`KeepId`, `Number`, `ZoneId`, `GameObjectId`, `DoorId`, `X`, `Y`, `Z`, `O`, `TeleportX1`, `TeleportY1`, `TeleportZ1`, `TeleportO1`, `TeleportX2`, `TeleportY2`, `TeleportZ2`, `TeleportO2`) VALUES ('100', '4', '110', '72', '115343465', '1432974', '966710', '16077', '1024', '1432862', '966713', '15945', '3066', '1433060', '966709', '15912', '1024');
INSERT INTO `war_world`.`keep_doors` (`KeepId`, `Number`, `ZoneId`, `GameObjectId`, `DoorId`, `X`, `Y`, `Z`, `O`, `TeleportX1`, `TeleportY1`, `TeleportZ1`, `TeleportO1`, `TeleportX2`, `TeleportY2`, `TeleportZ2`, `TeleportO2`) VALUES ('100', '4', '110', '72', '115343466', '1431291', '968156', '16061', '989', '1431190', '968157', '15939', '1063', '1431355', '968162', '15927', '1066');
INSERT INTO `war_world`.`keep_doors` (`KeepId`, `Number`, `ZoneId`, `GameObjectId`, `DoorId`, `X`, `Y`, `Z`, `O`, `TeleportX1`, `TeleportY1`, `TeleportZ1`, `TeleportO1`, `TeleportX2`, `TeleportY2`, `TeleportZ2`, `TeleportO2`) VALUES ('100', '2', '110', '100', '115343528', '1431201', '961432', '16032', '11', '1431203', '961499', '15960', '2064', '1431206', '961351', '15952', '4090');
INSERT INTO `war_world`.`keep_doors` (`KeepId`, `Number`, `ZoneId`, `GameObjectId`, `DoorId`, `X`, `Y`, `Z`, `O`, `TeleportX1`, `TeleportY1`, `TeleportZ1`, `TeleportO1`, `TeleportX2`, `TeleportY2`, `TeleportZ2`, `TeleportO2`) VALUES ('100', '2', '110', '100', '115343720', '1426272', '961445', '16032', '534', '1426273', '961507', '15944', '2022', '1426276', '961355', '15944', '46');
INSERT INTO `war_world`.`keep_doors` (`KeepId`, `Number`, `ZoneId`, `GameObjectId`, `DoorId`, `X`, `Y`, `Z`, `O`, `TeleportX1`, `TeleportY1`, `TeleportZ1`, `TeleportO1`, `TeleportX2`, `TeleportY2`, `TeleportZ2`, `TeleportO2`) VALUES ('100', '2', '110', '100', '115343784', '1436112', '961419', '16032', '3618', '1436109', '961501', '15944', '2146', '1436112', '961302', '15944', '2');
INSERT INTO `war_world`.`keep_doors` (`KeepId`, `Number`, `ZoneId`, `GameObjectId`, `DoorId`, `X`, `Y`, `Z`, `O`, `TeleportX1`, `TeleportY1`, `TeleportZ1`, `TeleportO1`, `TeleportX2`, `TeleportY2`, `TeleportZ2`, `TeleportO2`) VALUES ('100', '4', '110', '100', '115343848', '1431392', '971530', '16207', '4084', '1431389', '971620', '16128', '1030', '1431397', '971446', '16117', '4076');
INSERT INTO `war_world`.`keep_doors` (`KeepId`, `Number`, `ZoneId`, `GameObjectId`, `DoorId`, `X`, `Y`, `Z`, `O`, `TeleportX1`, `TeleportY1`, `TeleportZ1`, `TeleportO1`, `TeleportX2`, `TeleportY2`, `TeleportZ2`, `TeleportO2`) VALUES ('100', '4', '110', '72', '115445800', '1429023', '970563', '16195', '989', '1429059', '970167', '16121', '2100', '1429023', '970563', '16195', '2128');


##Progression Setup

DELETE FROM `war_world`.`rvr_progression` WHERE (`BattleFrontId` = '13');
DELETE FROM `war_world`.`rvr_progression` WHERE (`BattleFrontId` = '14');
DELETE FROM `war_world`.`rvr_progression` WHERE (`BattleFrontId` = '15');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `RegionId`, `ZoneId`, `DefaultRealmLock`, `ResetProgressionOnEntry`, `LastOwningRealm`, `LastOpenedZone`, `OrderVP`, `DestroVP`, `DestroKeepId`, `OrderKeepId`) VALUES ('4', '2', 'Reikwald', '13', '2', '2', '11', '110', '0', '0', '0', '1', '300', '0', '100', '100');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `RegionId`, `ZoneId`, `DefaultRealmLock`, `ResetProgressionOnEntry`, `LastOwningRealm`, `LastOpenedZone`, `OrderVP`, `DestroVP`, `DestroKeepId`, `OrderKeepId`) VALUES ('4', '1', 'Stonewatch', '14', '2', '2', '6', '102', '0', '0', '0', '0', '0', '0', '14', '14');
INSERT INTO `war_world`.`rvr_progression` (`tier`, `PairingId`, `Description`, `BattleFrontId`, `OrderWinProgression`, `DestWinProgression`, `RegionId`, `ZoneId`, `DefaultRealmLock`, `ResetProgressionOnEntry`, `LastOwningRealm`, `LastOpenedZone`, `OrderVP`, `DestroVP`, `DestroKeepId`, `OrderKeepId`) VALUES ('4', '2', 'The Maw', '15', '5', '5', '11', '101', '0', '0', '2', '0', '0', '0', '101', '101');
UPDATE `war_world`.`rvr_progression` SET `OrderWinProgression`='15' WHERE `BattleFrontId`='1';

## The Maw

DELETE FROM `war_world`.`zone_areas` WHERE `ZoneId`='101' and`PieceId`='0';
DELETE FROM `war_world`.`zone_areas` WHERE `ZoneId`='101' and`PieceId`='1';
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`, `OrderRespawnId`, `DestroRespawnId`) VALUES ('The Maw', '101', '0', '0', '0', '42', '113', '5660', '343', '344');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `TokExploreEntry`, `OrderRespawnId`, `DestroRespawnId`) VALUES ('The Maw', '101', '1', '2', '1', '42', '113', '5660', '343', '344');


DELETE FROM `war_world`.`keep_doors` where ZoneId = 101;
INSERT INTO `war_world`.`keep_doors` VALUES ('101', '2', '101', '100', '109068840', '1429552', '746937', '13600', '2044', '1429559', '746846', '13371', '2', '1429552', '747063', '13371', '2054');
INSERT INTO `war_world`.`keep_doors` VALUES ('101', '2', '101', '100', '109069032', '1424567', '749633', '13600', '1580', '1424516', '749582', '13371', '3576', '1434462', '749696', '13371', '2570');
INSERT INTO `war_world`.`keep_doors` VALUES ('101', '2', '101', '100', '109070760', '1434539', '749621', '13600', '2590', '1434597', '749566', '13371', '532', '1434462', '749696', '13371', '2562');
INSERT INTO `war_world`.`keep_doors` VALUES ('101', '2', '101', '100', '109086696', '1430008', '735391', '14318', '2102', '1430009', '735299', '14088', '10', '1430008', '735484', '14088', '2064');
INSERT INTO `war_world`.`keep_doors` VALUES ('101', '1', '101', '100', '109117672', '1429589', '741872', '14718', '2062', '1429589', '741783', '14488', '22', '1429585', '741952', '14488', '2106');
INSERT INTO `war_world`.`keep_doors` VALUES ('101', '4', '101', '72', '109117673', '1427830', '740903', '14631', '3120', '1427992', '740905', '14549', '1030', '1427768', '740903', '14497', '3000');
INSERT INTO `war_world`.`keep_doors` VALUES ('101', '4', '101', '72', '109117674', '1429465', '739569', '14658', '3472', '1429540', '739569', '14528', '720', '1429420', '739567', '14488', '344');

DELETE FROM `war_world`.`keep_infos` WHERE `KeepId`='101';
INSERT INTO `war_world`.`keep_infos` (`KeepId`, `Name`, `Realm`, `DoorCount`, `ZoneId`, `RegionId`, `X`, `Y`, `Z`, `O`, `PQuestId`, `Race`, `OilX`, `OilY`, `OilZ`, `OilO`, `OilOuterX`, `OilOuterY`, `OilOuterZ`, `OilOuterO`, `RamX`, `RamY`, `RamZ`, `RamO`, `RamOuterX`, `RamOuterY`, `RamOuterZ`, `RamOuterO`, `IsFortress`, `OilOuter1X`, `OilOuter1Y`, `OilOuter1Z`, `OilOuter1O`, `OilOuter2X`, `OilOuter2Y`, `OilOuter2Z`, `OilOuter2O`, `RamOuter1X`, `RamOuter1Y`, `RamOuter1Z`, `RamOuter1O`, `RamOuter2X`, `RamOuter2Y`, `RamOuter2Z`, `RamOuter2O`) VALUES ('101', 'The Maw', '2', '4', '101', '11', '1429597', '740733', '14518', '2', '0', '4', '1429595', '741865', '15018', '14', '1429554', '747051', '14181', '58', '1429586', '741962', '14488', '2108', '1429552', '747063', '13371', '2040', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');

UPDATE `war_world`.`keep_infos` SET `PQuestId`='9998' WHERE `KeepId`='101';

DELETE FROM  `war_world`.`pquest_info` where entry = 9998;
INSERT INTO `war_world`.`pquest_info` (`Entry`, `Name`, `Type`, `Level`, `ZoneId`, `PinX`, `PinY`, `TokDiscovered`, `TokUnlocked`, `ChapterId`, `GoldChestWorldX`, `GoldChestWorldY`, `GoldChestWorldZ`, `PQType`, `PQDifficult`, `Chapter`, `PQTier`, `PQCraftingBag`, `PQAreaId`, `SoundPQEnd`, `RespawnID`) VALUES ('9998', 'The Maw', '2', '0', '101', '22196', '9788', '0', '0', '0', '0', '0', '0', '2', '3', '22', '4', '4', '30', '0', '0');

UPDATE `war_world`.`keep_doors` SET `Number`='2' WHERE `DoorId`='109068840';
UPDATE `war_world`.`keep_doors` SET `Number`='1' WHERE `DoorId`='109117672';
UPDATE `war_world`.`keep_doors` SET `Number`='2' WHERE `DoorId`='109069032';
UPDATE `war_world`.`keep_doors` SET `Number`='2' WHERE `DoorId`='109070760';

# Fix Z for Maw doors
UPDATE `war_world`.`keep_doors` SET `Z`='13390' WHERE `DoorId`='109068840';
UPDATE `war_world`.`keep_doors` SET `Z`='13390' WHERE `DoorId`='109069032';
UPDATE `war_world`.`keep_doors` SET `Z`='13390' WHERE `DoorId`='109070760';



## Spawn Points

delete from war_world.keep_spawn_points;


## Spawn points for GTAOE at Garrison of Skulls / Sth Garrison


INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('18', '1444101', '835109', '14375', '5', '2036');
INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('18', '1444211', '832437', '14381', '5', '2044');

INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('17', '1450339', '880919', '15320', '5', '906');
INSERT INTO `war_world`.`keep_spawn_points` ( `KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('17', '1447219', '881430', '15745', '5', '480');

INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('15', '1439271', '907237', '17321', '5', '12');
INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('15', '1438703', '913973', '18649', '5', '2368');

INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('16', '1429407', '939837', '15411', '2', '3612');
INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('16', '1431975', '942028', '15945', '2', '4048');

INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('100', '1431310', '959417', '16023', '5', '32');
INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('100', '1426105', '959410', '16002', '5', '4084');
INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('100', '1426197', '959419', '15976', '5', '4060');
INSERT INTO `war_world`.`keep_spawn_points` (`KeepId`, `X`, `Y`, `Z`, `SiegeType`, `O`) VALUES ('100', '1431264', '963777', '15664', '5', '34');


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (17, 1450339, 880919, 15320, 906, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (17, 1447219, 881430, 15745, 480, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (17, 1448319, 881267, 16619, 2974, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (17, 1446630, 881909, 16268, 2608, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (18, 1444101, 835109, 14375, 2036, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (18, 1444211, 832437, 14381, 2044, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (18, 1444167, 833460, 15219, 4090, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (18, 1444245, 831549, 14938, 26, 3);



insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (19, 1441102, 769580, 12580, 2036, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (19, 1441078, 767553, 12741, 1752, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (19, 1441076, 767657, 13551, 4076, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (19, 1441078, 767553, 12741, 3746, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (20, 1446563, 802696, 13293, 2048, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (20, 1447105, 798799, 13788, 2210, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (20, 1446561, 800684, 14359, 3896, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (20, 1447506, 797247, 14624, 4074, 3);

insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (15, 1439271, 907237, 17321, 12, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (15, 1438703, 913973, 18649, 2368, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (15, 1439253, 908837, 18581, 2162, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (15, 1439529, 912425, 19297, 192, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (16, 1429407, 939837, 15411, 3612, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (16, 1431975, 942028, 15945, 4048, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (16, 1430588, 941162, 16735, 1602, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (16, 1432038, 943507, 16770, 2038, 3);


delete from war_world.keep_spawn_points where keepid >= 100

insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (101, 1429650, 748877, 13134, 2010, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (101, 1432972, 751411, 13049, 2520, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (101, 1426023, 751181, 13001, 1550, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (101, 1429726, 744258, 13799, 2024, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (101, 1429550, 747013, 14181, 4062, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (101, 1434483, 749678, 14181, 520, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (101, 1424625, 749692, 14181, 3568, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (101, 1429596, 741820, 15018, 20, 2);

insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (100, 1431310, 959417, 16023, 32, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (100, 1436227, 959475, 16033, 4084, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (100, 1426197, 959419, 15976, 4060, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (100, 1431264, 963777, 15664, 34, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (100, 1431205, 961326, 16790, 2032, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (100, 1436114, 961309, 16790, 2014, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (100, 1426274, 961337, 16790, 2066, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (100, 1431159, 965698, 16455, 2062, 2);

insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (102, 1399068, 1032502, 8220, 4088, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (102, 1405401, 1037932, 8224, 646, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (102, 1392997, 1037697, 8194, 3480, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (102, 1398972, 1039710, 8536, 34, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (102, 1399054, 1034646, 9068, 2042, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (102, 1403478, 1039189, 9068, 2720, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (102, 1394687, 1039028, 9068, 1134, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (102, 1398840, 1038969, 9562, 2072, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (103, 1398235, 824166, 11533, 2004, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (103, 1403130, 822812, 11419, 1866, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (103, 1393174, 823734, 11783, 2054, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (103, 1398076, 819746, 11842, 2024, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (103, 1398119, 821929, 12303, 4002, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (103, 1402715, 821144, 12303, 3886, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (103, 1393156, 821863, 12303, 4048, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (103, 1398032, 818037, 12774, 3968, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (104, 876487, 1636168, 7539, 1110, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (104, 875075, 1641252, 8036, 1372, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (104, 876198, 1631163, 7696, 936, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (104, 871612, 1635565, 8249, 1046, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (104, 874714, 1635926, 8986, 3060, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (104, 873183, 1640299, 8986, 3406, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (104, 874468, 1631464, 8986, 2888, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (104, 869261, 1635589, 9508, 3056, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (105, 1086851, 1638524, 6441, 2936, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (105, 1088351, 1633496, 6314, 3072, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (105, 1088583, 1642887, 6371, 3026, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (105, 1093069, 1638357, 7312, 3072, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (105, 1089409, 1638190, 7483, 1018, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (105, 1090533, 1633822, 7481, 1198, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (105, 1090488, 1642586, 7487, 712, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (105, 1094674, 1638366, 7970, 874, 3);


delete from `war_world`.`creature_protos` where entry = 86211;

INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `FigLeafData`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`, `Unk2`) VALUES ('86211', 'Heavy Destruction Oil', '1551', '0', '50', '50', '40', '40', '64', '5', '18', '0', '0', '5', '1', '0', '1', '26368', '1', '33383079756513514', '', '32', '161', '0', '0', '0', '0 0 0 8 1 10\r', '0', '1.00', '1.00', '0', '0', '0', '0');



INSERT INTO `war_world`.`keep_infos` (`KeepId`, `Name`, `Realm`, `DoorCount`, `ZoneId`, `RegionId`, `X`, `Y`, `Z`, `O`, `PQuestId`, `Race`, `OilX`, `OilY`, `OilZ`, `OilO`, `OilOuterX`, `OilOuterY`, `OilOuterZ`, `OilOuterO`, `RamX`, `RamY`, `RamZ`, `RamO`, `RamOuterX`, `RamOuterY`, `RamOuterZ`, `RamOuterO`, `IsFortress`, `OilOuter1X`, `OilOuter1Y`, `OilOuter1Z`, `OilOuter1O`, `OilOuter2X`, `OilOuter2Y`, `OilOuter2Z`, `OilOuter2O`, `RamOuter1X`, `RamOuter1Y`, `RamOuter1Z`, `RamOuter1O`, `RamOuter2X`, `RamOuter2Y`, `RamOuter2Z`, `RamOuter2O`) VALUES ('103', 'Stonewatch', '2', '4', '4', '2', '1398796', '1010120', '8847', '2068', '0', '2', '1398846', '1038904', '9566', '2082', '1399056', '1034587', '9068', '2110', '1398854', '1038904', '8903', '42', '1399062', '1034508', '8264', '24', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`keep_infos` (`KeepId`, `Name`, `Realm`, `DoorCount`, `ZoneId`, `RegionId`, `X`, `Y`, `Z`, `O`, `PQuestId`, `Race`, `OilX`, `OilY`, `OilZ`, `OilO`, `OilOuterX`, `OilOuterY`, `OilOuterZ`, `OilOuterO`, `RamX`, `RamY`, `RamZ`, `RamO`, `RamOuterX`, `RamOuterY`, `RamOuterZ`, `RamOuterO`, `IsFortress`, `OilOuter1X`, `OilOuter1Y`, `OilOuter1Z`, `OilOuter1O`, `OilOuter2X`, `OilOuter2Y`, `OilOuter2Z`, `OilOuter2O`, `RamOuter1X`, `RamOuter1Y`, `RamOuter1Z`, `RamOuter1O`, `RamOuter2X`, `RamOuter2Y`, `RamOuter2Z`, `RamOuter2O`) VALUES ('101', 'Fell Landing', '2', '4', '4', '2', '1398796', '1010120', '8847', '2068', '0', '2', '1398846', '1038904', '9566', '2082', '1399056', '1034587', '9068', '2110', '1398854', '1038904', '8903', '42', '1399062', '1034508', '8264', '24', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`keep_infos` (`KeepId`, `Name`, `Realm`, `DoorCount`, `ZoneId`, `RegionId`, `X`, `Y`, `Z`, `O`, `PQuestId`, `Race`, `OilX`, `OilY`, `OilZ`, `OilO`, `OilOuterX`, `OilOuterY`, `OilOuterZ`, `OilOuterO`, `RamX`, `RamY`, `RamZ`, `RamO`, `RamOuterX`, `RamOuterY`, `RamOuterZ`, `RamOuterO`, `IsFortress`, `OilOuter1X`, `OilOuter1Y`, `OilOuter1Z`, `OilOuter1O`, `OilOuter2X`, `OilOuter2Y`, `OilOuter2Z`, `OilOuter2O`, `RamOuter1X`, `RamOuter1Y`, `RamOuter1Z`, `RamOuter1O`, `RamOuter2X`, `RamOuter2Y`, `RamOuter2Z`, `RamOuter2O`) VALUES ('105', 'Sining Way', '2', '4', '4', '2', '1398796', '1010120', '8847', '2068', '0', '2', '1398846', '1038904', '9566', '2082', '1399056', '1034587', '9068', '2110', '1398854', '1038904', '8903', '42', '1399062', '1034508', '8264', '24', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (7, 1398984, 928152, 11312, 3080, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (7, 1403259, 928149, 11734, 3080, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (7, 1401183, 928201, 12252, 1050, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (7, 1404960, 928184, 12672, 996, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (8, 1375613, 927330, 11741, 860, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (8, 1372682, 927945, 11897, 938, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (8, 1374208, 927662, 12824, 3018, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (8, 1371289, 928066, 12615, 3080, 3);



insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (9, 1392460, 975834, 7465, 3600, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (9, 1394289, 977672, 7768, 3468, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (9, 1393561, 977080, 8582, 1516, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (9, 1394964, 978155, 8379, 1402, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (10, 1409007, 1011763, 5017, 3128, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (10, 1412350, 1011993, 5750, 3092, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (10, 1410830, 1011937, 6340, 998, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (10, 1413507, 1012042, 6643, 1054, 3);



insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (5, 1412895, 846004, 10038, 2854, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (5, 1412895, 846004, 10038, 3044, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (5, 1412895, 846004, 10038, 928, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (5, 1412895, 846004, 10038, 912, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (6, 1397614, 876188, 9107, 2904, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (6, 1400706, 875194, 9312, 2606, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (6, 1399440, 875724, 10066, 902, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (6, 1401703, 874397, 10006, 738, 3);



insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (28, 967067, 35692, 8092, 2124, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (28, 967827, 1638266, 8152, 2508, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (28, 967330, 1639294, 8954, 150, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (28, 968502, 1637425, 8761, 308, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (27, 984811, 1640045, 11392, 2456, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (27, 986766, 1637285, 11470, 2550, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (27, 986069, 1638309, 12226, 376, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (27, 987460, 1636525, 12171, 366, 3);



insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (30, 935740, 1637193, 7648, 1006, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (30, 932192, 1637291, 8114, 842, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (30, 933714, 1637227, 8768, 2818, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (30, 930387, 1637645, 9010, 2906, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (29, 891447, 1637149, 5765, 1074, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (29, 888790, 1637269, 6028, 914, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (29, 889540, 1637022, 6759, 2938, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (29, 887550, 1637433, 6833, 2994, 3);



insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (26, 102810, 1642054, 6237, 3062, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (26, 1032386, 1641694, 6987, 3900, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (26, 1030038, 1642026, 7369, 858, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (26, 1032829, 1643140, 7842, 1884, 3);


insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (25, 1064315, 1639886, 5219, 3040, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (25, 1068262, 1639817, 5655, 3088, 2);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (25, 1066487, 1639794, 6005, 1006, 3);
insert into war_world.keep_spawn_points (KeepId, X, Y, Z, O, SiegeType) values (25, 1069712, 1639854, 6276, 1004, 3);



DELETE FROM `war_world`.`keep_infos` WHERE `KeepId`='102';
INSERT INTO `war_world`.`keep_infos` (`KeepId`, `Name`, `Realm`, `DoorCount`, `ZoneId`, `RegionId`, `X`, `Y`, `Z`, `O`, `PQuestId`, `Race`, `OilX`, `OilY`, `OilZ`, `OilO`, `OilOuterX`, `OilOuterY`, `OilOuterZ`, `OilOuterO`, `RamX`, `RamY`, `RamZ`, `RamO`, `RamOuterX`, `RamOuterY`, `RamOuterZ`, `RamOuterO`, `IsFortress`, `OilOuter1X`, `OilOuter1Y`, `OilOuter1Z`, `OilOuter1O`, `OilOuter2X`, `OilOuter2Y`, `OilOuter2Z`, `OilOuter2O`, `RamOuter1X`, `RamOuter1Y`, `RamOuter1Z`, `RamOuter1O`, `RamOuter2X`, `RamOuter2Y`, `RamOuter2Z`, `RamOuter2O`) VALUES ('102', 'Butcher\'s pass', '2', '4', '4', '2', '1398796', '1010120', '8847', '2068', '0', '2', '1398846', '1038904', '9566', '2082', '1399056', '1034587', '9068', '2110', '1398854', '1038904', '8903', '42', '1399062', '1034508', '8264', '24', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');


UPDATE `war_world`.`item_infos` SET `IsSiege`='1' WHERE `Entry`='86210';
UPDATE `war_world`.`item_infos` SET `IsSiege`='1' WHERE `Entry`='86222';
UPDATE `war_world`.`item_infos` SET `IsSiege`='1' WHERE `Entry`='86234';
UPDATE `war_world`.`item_infos` SET `IsSiege`='1' WHERE `Entry`='86246';
UPDATE `war_world`.`item_infos` SET `IsSiege`='1' WHERE `Entry`='86270';

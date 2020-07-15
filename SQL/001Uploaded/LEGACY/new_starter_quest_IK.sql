######Portal spawn and proto####

UPDATE `war_world`.`gameobject_protos` SET `Entry`='3100417', `Name`='Rift in Time', `DisplayID`='1583', `Scale`='50', `Level`='1', `Faction`='0', `HealthPoints`='1', `ScriptName`='', `TokUnlock`=NULL, `Unks`='7680 0 18181 36 30501 64168', `Unk1`='0', `Unk2`='0', `Unk3`='25700', `Unk4`='0', `CreatureId`='0', `CreatureCount`='0', `CreatureSpawnText`=NULL, `CreatureCooldownMinutes`='0', `IsAttackable`='0' WHERE (`Entry`='3100417');
UPDATE `war_world`.`gameobject_protos` SET `Entry`='3100418', `Name`='Rift in Time', `DisplayID`='1583', `Scale`='50', `Level`='1', `Faction`='0', `HealthPoints`='1', `ScriptName`='', `TokUnlock`=NULL, `Unks`='7680 0 18181 36 30501 64168', `Unk1`='0', `Unk2`='0', `Unk3`='25700', `Unk4`='0', `CreatureId`='0', `CreatureCount`='0', `CreatureSpawnText`=NULL, `CreatureCooldownMinutes`='0', `IsAttackable`='0' WHERE (`Entry`='3100418');

delete from  `war_world`.`gameobject_spawns` where guid in (2119359, 2119659);

INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unks`, `DoorId`, `VfxState`, `TokUnlock`, `SoundId`, `AllowVfxUpdate`, `AlternativeName`) VALUES ('2119359', '3100417', '106', '834940', '936182', '6976', '104', '1583', '0', '0', '0', '0', '7682 0 0 4 5 0 ', '0', '0', '', '0', '0', '');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unks`, `DoorId`, `VfxState`, `TokUnlock`, `SoundId`, `AllowVfxUpdate`, `AlternativeName`) VALUES ('2119659', '3100418', '100', '848490', '829913', '7976', '962', '1583', '0', '0', '0', '0', '7680 0 18181 36 30501 -1368 ', '0', '0', '', '0', '0', '');

########Creature proto and spawns #######


delete from `war_world`.`creature_protos` where entry in (10505154, 10505155, 10505156, 10505157);

INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `BaseRadiusUnits`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505154', 'Griffon Recruiters', '1219', '0', '52', '52', '40', '40', '64', '5', '18', '0', '0', '61', '1', '1000', '0', '0', '13573', '0', '64525633521916729', '', '16', '91', '0', '0', '0', NULL, '0 0 0 3 1 10\r', NULL, '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `BaseRadiusUnits`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505155', 'Gyropack Riders', '1214', '0', '55', '55', '40', '40', '80', '5', '18', '0', '0', '29', '1', '1000', '0', '0', '50904', '1', '51107032763080272', '', '13', '66', '0', '0', '0', NULL, '0 0 136 8 1 10\r', NULL, '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `BaseRadiusUnits`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505156', 'Dark Recruiters', '1222', '0', '60', '60', '40', '40', '131', '5', '18', '0', '0', '51', '0', '1000', '0', '1', '17556', '5', '21802436713533324', '', '16', '89', '0', '0', '0', NULL, NULL, NULL, '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `BaseRadiusUnits`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505157', 'Dark Recruiter', '1222', '0', '60', '60', '40', '40', '131', '5', '18', '0', '0', '51', '0', '1000', '0', '1', '17556', '5', '21802436713533324', '', '16', '89', '0', '0', '0', NULL, NULL, NULL, '0', '1.00', '1.00', '0', '0', '0');


delete from `war_world`.`creature_spawns` where guid in ('7210873',
'7210877',
'7210890',
'7210891',
'7210892',
'7210882',
'7210876',
'7210883',
'7210871');

INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210873', '10505154', '162', '123976', '125202', '13131', '1888', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210877', '10505155', '162', '126777', '128891', '12699', '738', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210890', '10505157', '161', '440359', '140532', '17057', '1784', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210891', '10505157', '161', '440061', '140537', '17057', '2334', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210892', '10505156', '161', '440204', '140548', '17057', '2020', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210882', '2000831', '100', '848358', '829856', '7972', '890', '0', '2', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210876', '1000225', '106', '834852', '936097', '6979', '112', '0', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210883', '1000226', '100', '848436', '830027', '7977', '1136', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210871', '98323', '106', '835013', '936318', '6984', '242', '18', '0', '0', '0', '0', '0', '0', '1');

    
delete from war_world.gameobject_protos where entry in (3100417,3100418,2119359,2119659);
INSERT INTO war_world.gameobject_protos (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock, Unks, Unk1, Unk2, Unk3, Unk4, CreatureId, CreatureCount, CreatureSpawnText, CreatureCooldownMinutes, IsAttackable) VALUES ('3100417', 'Rift in Time', '1583', '50', '1', '0', '1', '', NULL, '7680 0 18181 36 30501 64168', '0', '0', '25700', '0', '0', '0', NULL, '0', '0');
INSERT INTO war_world.gameobject_protos (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock, Unks, Unk1, Unk2, Unk3, Unk4, CreatureId, CreatureCount, CreatureSpawnText, CreatureCooldownMinutes, IsAttackable) VALUES ('3100418', 'Rift in Time', '1583', '50', '1', '0', '1', '', NULL, '7680 0 18181 36 30501 64168', '0', '0', '25700', '0', '0', '0', NULL, '0', '0');
#INSERT INTO war_world.gameobject_spawns (Guid, Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, DisplayID, Unk1, Unk2, Unk3, Unk4, Unks, DoorId, VfxState, TokUnlock, SoundId, AllowVfxUpdate, AlternativeName) VALUES ('2119359', '3100417', '106', '834940', '936182', '6976', '104', '1583', '0', '0', '0', '0', '7682 0 0 4 5 0 ', '0', '0', NULL, '0', '0', NULL);
#INSERT INTO war_world.gameobject_spawns (Guid, Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, DisplayID, Unk1, Unk2, Unk3, Unk4, Unks, DoorId, VfxState, TokUnlock, SoundId, AllowVfxUpdate, AlternativeName) VALUES ('2119659', '3100418', '100', '848490', '829913', '7976', '962', '1583', '0', '0', '0', '0', '7680 0 18181 36 30501 -1368 ', '0', '0', NULL, '0', '0', NULL);

delete from war_world.zone_jumps where entry in (2119359,2119659);
INSERT INTO war_world.zone_jumps (Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, Enabled, Type, InstanceID) VALUES ('2119359', '162', '123860', '124815', '13131', '0', '1', '0', NULL);
INSERT INTO war_world.zone_jumps (Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, Enabled, Type, InstanceID) VALUES ('2119659', '161', '440227', '139944', '17057', '6', '1', '0', NULL);

#### Vendor Items corrected anni merchant###


delete from `war_world`.`vendor_items` where vendorid in (50,51) and itemid in ('1337', 
'208281',
'208282',
'208291',
'208292',
'434096',
'434097',
'434098',
'434099',
'434100',
'434101',
'434102',
'434103',
'434104',
'434105',
'434106',
'434107',
'434108',
'434109',
'434110',
'434111',
'434112',
'434113',
'434114',
'434115',
'434116',
'434117',
'434118',
'434119',
'434120',
'434121',
'434122',
'434123',
'434124',
'434125',
'434126',
'434127',
'434128',
'434129',
'434130',
'434131',
'434132',
'434133',
'434134',
'434135',
'434136',
'434137',
'434138',
'434139',
'434140',
'434141',
'434142',
'434143',
'434144',
'434145',
'434146',
'434147',
'434148',
'434149',
'434150',
'434151',
'434152',
'434153',
'434154',
'434155',
'1337', 
'208281',
'208282',
'208291',
'208292',
'434156',
'434157',
'434158',
'434159',
'434160',
'434161',
'434162',
'434163',
'434164',
'434165',
'434166',
'434167',
'434168',
'434169',
'434170',
'434171',
'434172',
'434173',
'434174',
'434175',
'434176',
'434177',
'434178',
'434179',
'434180',
'434181',
'434182',
'434183',
'434184',
'434185',
'434186',
'434187',
'434188',
'434189',
'434190',
'434191',
'434192',
'434193',
'434194',
'434195',
'434196',
'434197',
'434198',
'434199',
'434200',
'434201',
'434202',
'434203',
'434204',
'434205',
'434206',
'434207',
'434208',
'434209',
'434210',
'434211',
'434212',
'434213',
'434214',
'434215');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '1337', '0', NULL, '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '208281', '0', '0', '0', '(1,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '208282', '0', '0', '0', '(1,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '208291', '0', '0', '0', '(2,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '208292', '0', '0', '0', '(2,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434096', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434097', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434098', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434099', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434100', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434101', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434102', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434103', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434104', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434105', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434106', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434107', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434108', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434109', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434110', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434111', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434112', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434113', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434114', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434115', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434116', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434117', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434118', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434119', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434120', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434121', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434122', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434123', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434124', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434125', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434126', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434127', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434128', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434129', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434130', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434131', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434132', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434133', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434134', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434135', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434136', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434137', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434138', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434139', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434140', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434141', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434142', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434143', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434144', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434145', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434146', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434147', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434148', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434149', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434150', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434151', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434152', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434153', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434154', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '434155', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '1337', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '208281', '0', '0', '0', '(1,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '208282', '0', '0', '0', '(1,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '208291', '0', '0', '0', '(2,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '208292', '0', '0', '0', '(2,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434156', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434157', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434158', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434159', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434160', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434161', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434162', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434163', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434164', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434165', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434166', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434167', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434168', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434169', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434170', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434171', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434172', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434173', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434174', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434175', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434176', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434177', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434178', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434179', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434180', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434181', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434182', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434183', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434184', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434185', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434186', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434187', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434188', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434189', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434190', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434191', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434192', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434193', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434194', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434195', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434196', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434197', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434198', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434199', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434200', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434201', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434202', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434203', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434204', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434205', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434206', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434207', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434208', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434209', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434210', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434211', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434212', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434213', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434214', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '434215', '0', '0', '0', '0');


######quests #######

delete from `war_world`.`quests` where entry in ('60501',
'60502',
'60503',
'60504',
'60505',
'60506',
'60507',
'60508',
'60509',
'60510',
'60511',
'60512',
'60513',
'60514',
'60515',
'60516');

INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60501', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Soldier.\nBy order of Emperor Karl Franz we welcome you to the Alliance of Order. We offer you a set of armor and a magical scroll to prepare you for the front lines of battle. Should you choose to accept this offer, The Annihilator Merchant will prepare you for the battle ahead. Speak with him to receive these gifts your Emperor has provided for you. \nShould you choose to decline this offer the lands in front of you are vast and filled with peril.\nThe journey ahead will be a long and hard one.', 'Speak to the Annihilator Merchant and buy your Gifts for free.', '5000', '50', '', '', '1', '0', 'Forward, Soldier!', 'Ah, you\'ve learned the ways of the War? Then let us be about our grim task!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60502', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'By Sigmar! Welcome Soldier, you have been sent to me by the Griffon Recruiter to prepare you for the battle ahead, buy this gear and scroll off me to get you ready for War!', 'Your King has prepared for you the finest of equipment! Buy the Annihilator Gear set and magical scroll.', '5000', '50', '', '', '1', '60501', 'Lets get to business!', 'You have Bought your starter gears (i Hope) now let\'s continue!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60503', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Now Soldier! You have received your gifts and accepted your Faith! In front of you Lies a Rift of Time take this to meet up with the Griffon Recruiters at the Palace Gates of our city Altdorf!', 'Journey through the Rift! And meet up with the Griffon Recruiters by the Palace Gates.', '5000', '50', '', '', '1', '60502', 'Journey through the Rift! Speak to the Griffon Recruiters by the Palace Gates when you arrive.', 'Well done Soldier!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60504', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Soldier, The pinnacle of Greatness Altdorf the city of the Alliance of Order. Here we are going to train you further to prepare you the best we can for War!', 'Speak to the Career Trainer by The Temple of Sigmar to train and sharpen your skills.', '5000', '5000', '', '', '1', '60503', 'Let\'s Continue!', 'Very well done Soldier u might progress to be a Realm Captain 1 day!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60505', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Soldier, I\'m the Career Trainer we provide you specialization into your chosen path, which increases your strengths or defenses! Choose it wisely! Don\'t worry to you can always respecialize it into another path!', 'Train your Mastery Abilities by the Career trainer!', '5000', '5000', '', '', '1', '60504', 'Let\'s Continue sharpening your skills!', 'Very well done Soldier!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60506', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done soldier! you are Almost ready for the War!', 'Speak to Martial Marshal to Buy your weapons! ', '5000', '5000', '', '', '1', '60505', 'Let\'s Continue on the road to progress this time ill send you off to buy yourself some Weapons!', 'Very well done Soldier! Make sure you check the other merchants out as well!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60507', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done soldier! you are very close for the War! Equip this fine weaponry and check for some jewels!', 'Make haste with buying all your gears and speak to the Marshal protector to pick up your accessories!', '5000', '5000', '', '', '1', '60506', 'By the Grace of Sigmar Fight well and Serve with Honor!', 'Sigmars Grace be with you!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60508', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done soldier! you are ready for the War! Fly to the battlefield as soon as you are ready and crush our enemies!', 'Speak to the Gyropack Riders and then the Flightmaster to Fly to the Battlefield! ', '5000', '5000', '', '[1337,1]', '1', '60507', 'By the Grace of Sigmar Fight well and Serve with Honor!', 'Sigmars Grace be with you!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60509', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome, Fiend.\nBy order of King Tchar\'zanek, we welcome you to the lands of Chaos. We offer you a set of Armor and magical scrolls to prepare you for the front lines of battle. Should you choose to accept this offer, Annihilator Merchant will prepare you for the battle ahead. Speak with him to receive these gifts your King has provided for you. \nShould you choose to decline this offer the lands in front of you are vast and filled with peril.\nThe journey ahead will be a long and hard one.', 'Speak to the Annihilator Merchant and buy your Gifts for free.', '5000', '50', '', '', '1', '0', 'Forward, Maggot!', 'Ah, you\'ve learned the ways of the War? Then let us be about our grim task!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60510', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'By the Dark Gods! Welcome Maggot, you have been sent to me by Dal\'nishra, Weaver of Fate to prepare you for the battle ahead, buy this gear and scroll off me to get you ready for War!', 'Your King has prepared for you the finest of equipment! Buy the Annihilator Gear set and magical scroll.', '5000', '50', '', '', '1', '60509', 'Lets get to business!', 'You have Bought your starter gears (i Hope) now let\'s continue!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60511', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Now Maggot! You have received your gifts and accepted your Faith! In front of you Lies a Rift of Time take this to meet up with the Dark Recruiters at the south of our Inevitable city!', 'Journey through the Rift! And meet up with the Dark Recruiters by the south side of the city.', '5000', '50', '', '', '1', '60510', 'Journey through the Rift! Speak to the Dark Recruiters by the south side of the city when you arrive.', 'Well done Maggot!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60512', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Maggot, The pinnacle of Darkness The Inevitable city the Alliance of Destruction. Here we are going to train you further to prepare you the best we can for War!', 'Speak to the Career Trainer by The Monolith to train and sharpen your skills.', '5000', '5000', '', '', '1', '60511', 'Let\'s Continue!', 'Very well done Maggot u might progress to be a Realm Captain 1 day!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60513', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Maggot, I\'m the Career Trainer we provide you specialization into your chosen path, which increases your strengths or defenses! Choose it wisely! Don\'t worry to you can always respecialize it into another path!', 'Train your Mastery Abilities by the Career trainer!', '5000', '5000', '', '', '1', '60512', 'Let\'s Continue sharpening your skills!', 'Very well done Maggot!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60514', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done Maggot! you are Almost ready for the War!', 'Speak to Martial Berserker to Buy your weapons! ', '5000', '5000', '', '', '1', '60513', 'Let\'s Continue on the road to progress this time ill send you off to buy yourself some Weapons!', 'Very well done Maggot! Make sure you check the other merchants out as well!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60515', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done Maggot! you are very close for the War! Equip this fine weaponry and check for some jewels!', 'Make haste with buying all your gears and speak to the Marshal Steelhide to pick up your accessories!', '5000', '5000', '', '', '1', '60514', 'By the Grace of The Dark God Fight well and Serve with Honor!', 'The Dark Gods be with you!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60516', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done Maggot! you are ready for the War! Fly to the battlefield as soon as you are ready and crush our enemies!', 'Speak to the Wyvern tendin git and then the Flightmaster to Fly to the Battlefield! ', '5000', '5000', '', '[1337,1]', '1', '60515', 'By the Grace of The Dark God Fight well and Serve with Honor!', 'The Dark Gods be with you!', '0', '1', '0');



#####Quest starter######

delete from `war_world`.`quests_creature_starter` where entry in ('60501',
'60502',
'60503',
'60504',
'60505',
'60506',
'60507',
'60508',
'60509',
'60510',
'60511',
'60512',
'60513',
'60514',
'60515',
'60516');

INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60501', '98323');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60502', '1000225');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60503', '1000225');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60504', '10505154');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60505', '99797');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60506', '99797');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60507', '1000244');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60508', '1000244');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60509', '2000831');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60510', '1000226');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60511', '1000226');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60512', '10505156');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60513', '1351');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60514', '1351');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60515', '1000235');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60516', '1000235');


#####Quest Finisher####
delete from `war_world`.`quests_creature_finisher` where entry in ('60501',
'60502',
'60503',
'60504',
'60505',
'60506',
'60507',
'60508',
'60509',
'60510',
'60511',
'60512',
'60513',
'60514',
'60515',
'60516');

INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60501', '1000225');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60502', '1000225');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60503', '10505154');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60504', '99797');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60505', '99797');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60506', '1000244');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60507', '1000244');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60508', '10505155');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60509', '1000226');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60510', '1000226');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60511', '10505156');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60512', '1351');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60513', '1351');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60514', '1000235');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60515', '1000235');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60516', '1008000');

UPDATE war_world.quests_creature_finisher SET Entry='60508', CreatureID='10505155' WHERE (Entry='60508');
UPDATE war_world.zone_jumps SET Entry='2119659', ZoneId='161', WorldX='440227', WorldY='139944', WorldZ='17057', WorldO='6', Enabled='1', Type='0', InstanceID=NULL WHERE (Entry='2119659');
UPDATE war_world.quests_creature_finisher SET Entry='60514', CreatureID='1000235' WHERE (Entry='60514');

#####Quest maps#######

delete from `war_world`.`quests_maps` where `quests_maps_ID` in ('60504',
'60506',
'60508',
'60512',
'60514',
'60516');

INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60504', '60504', '2', 'Trainer Eduardt', 'Trainer Eduardt', '162', '1130', '15539', '38383', '0', '0');
INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60506', '60506', '2', 'Speak to Martial Marshal', 'Speak to Martial Marshal', '162', '1130', '20720', '48351', '0', '0');
INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60508', '60508', '2', 'Speak to Gyropack Riders', 'Speak to Gyropack Riders', '162', '1130', '28473', '30587', '0', '0');
INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60512', '60512', '2', 'Seer Uresha', 'Seer Uresha', '161', '1130', '23348', '37808', '0', '0');
INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60514', '60514', '2', 'Speak to Martial Berserker', 'Speak to Martial Berserker', '161', '1130', '32247', '53687', '0', '0');
INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60516', '60516', '2', 'Speak to Wyvern tendin git', 'Speak to Wyvern tendin git', '161', '1130', '28439', '31122', '0', '0');


#######Quest objectives####

delete from `war_world`.`quests_objectives` where `Guid` in ('74835849',
'74835850',
'74835851',
'74835852',
'74835853',
'74835854');

INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835849', '60504', '1', '1', 'Speak with Trainer Eduardt', '99797', NULL, NULL, '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835850', '60506', '1', '1', 'Speak to Martial Marshal', '1000244', NULL, NULL, '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835851', '60508', '1', '1', 'Speak to Gyropack Riders', '10505155', NULL, NULL, '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835852', '60512', '1', '1', 'Speak with Seer Uresha', '1351', NULL, NULL, '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835853', '60514', '1', '1', 'Speak to Martial Berserker', '1000235', NULL, NULL, '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835854', '60516', '1', '1', 'Speak to Wyvern tendin git', '1008000', NULL, NULL, '0');

delete from `war_world`.`creature_items` where entry = 10505154;
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '10', '8819', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '11', '8811', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '20', '7186', '40473', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '21', '7188', '40473', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '22', '7187', '40473', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '23', '7184', '40473', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '24', '7189', '40473', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '25', '3098', '40473', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '27', '7190', '40473', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505154', '28', '7185', '40473', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '20', '2091', '0', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '21', '2093', '0', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '22', '2092', '0', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '23', '2095', '0', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '24', '2094', '0', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '28', '2090', '0', '0', '0');
DELETE FROM  `war_world`.`quests_creature_finisher` where entry = 60508;
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60508', '10505155');

delete from `war_world`.`creature_items` where entry = 10505156;
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '10', '3414', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '11', '5740', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '20', '3208', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '21', '3210', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '22', '3209', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '23', '3213', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '24', '3212', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '25', '3211', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '28', '3207', '31868', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '10', '3414', '0', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '11', '5740', '0', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '20', '3208', '31868', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '21', '3210', '31868', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '22', '3209', '31868', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '23', '3213', '31868', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '24', '3212', '31868', '0', '0');
#NSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '25', '3211', '31868', '0', '0');
#INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '28', '3207', '31868', '0', '0');
DELETE FROM  `war_world`.`quests_creature_finisher` where entry = 60516;
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60516', '1008000');

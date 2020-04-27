# Part1
#
#
# 1 Start
#
#
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('436306:Bloodlord Carcass-Guard|436318:Bloodlord Wingmantle|436330:Bloodlord Cowl|436342:Bloodlord Handwraps|436354|Bloodlord Footwraps', '9:62|3:62|78:5|', '6666', 'Bloodlord Zealot Placeholder', '40');
UPDATE `war_world`.`item_sets` SET `BonusString` = '9:62|3:62|78:5|92:2|' WHERE (`Entry` = '6666');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '1' WHERE (`Entry` = '436354');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '1' WHERE (`Entry` = '436342');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '1' WHERE (`Entry` = '436330');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '1' WHERE (`Entry` = '436318');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '1' WHERE (`Entry` = '436306');
-- v1
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,62,0|35:3,62,0|36:78,5,0|37:93,2,0|' WHERE (`Entry` = '6666');

# Bloodlord Footwraps
UPDATE `war_world`.`item_infos` SET `Stats` = '3:29;4:16;9:12;7:8;', `ObjectLevel` = '47' WHERE (`Entry` = '436354');

-- v2
UPDATE `war_world`.`item_sets` SET `Name` = 'Bloodlord Devotee\'s Kit' WHERE (`Entry` = '6666');

-- v3
UPDATE `war_world`.`item_infos` SET `Stats` = '3:32;6:14;9:19;79:13;' WHERE (`Entry` = '436306');

-- v4
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436306:Bloodlord Carcass-Guard|436318:Bloodlord Wingmantle|436330:Bloodlord Cowl|436342:Bloodlord Handwraps|436354:Bloodlord Footwraps|' WHERE (`Entry` = '6666');

# Bloodlord Carcass-Guard
UPDATE `war_world`.`item_infos` SET `Stats` = '3:32;6:14;9:19;79:52;' WHERE (`Entry` = '436306');
#
#
# 1 End
#
#

#
#
# 2 Start
#
#
UPDATE `war_world`.`gameobject_spawns` SET `WorldX` = '1027018', `WorldY` = '998056', `WorldZ` = '14365', `DoorId` = '167773224' WHERE (`Guid` = '260708');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '260318');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '260319');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '260352');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '260353');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '260354');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '260355');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('1000', '2', '160', '1014256', '1013977', '18242', '0', '65535', '0', '0', '100', '0', '168030888', '0', '0', '1');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('1001', '2', '160', '1014257', '1013977', '17385', '0', '65535', '0', '0', '100', '0', '168030889', '0', '0', '1');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('1002', '2', '160', '1014256', '1016404', '16527', '0', '65535', '0', '0', '100', '0', '168030890', '0', '0', '1');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('1003', '2', '160', '1017402', '1016404', '16527', '0', '65535', '0', '0', '100', '0', '168030891', '0', '0', '1');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('1004', '2', '160', '1017402', '1015191', '17385', '0', '65535', '0', '0', '100', '0', '168030892', '0', '0', '1');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('1005', '2', '160', '1017402', '1013977', '18242', '0', '65535', '0', '0', '100', '0', '168030893', '0', '0', '1');
#
#
# 2 End
#
#

#
#
# 3 Start
#
#
# Direhelm of the Bloodborne 656137
UPDATE `war_world`.`item_infos` SET `Stats` = '1:18;4:15;7:18;', `ObjectLevel` = '40' WHERE (`Entry` = '656137');
#
#
# 3 Start
#
#

#
#
# 4 Start
#
#
UPDATE `war_world`.`item_infos` SET `Stats` = '1:12;4:21;6:18;' WHERE (`Entry` = '656157');
#
#
# 4 End
#
#

#
#
# 5 Start
#
#
# Shouldateef of the Bloodherd
UPDATE `war_world`.`item_infos` SET `Name` = 'Sholdateef of the Bloodherd', `Rarity` = '3', `Stats` = '4:14;5:17;8:21', `ObjectLevel` = '40' WHERE (`Entry` = '901837');
#
#
# 5 End
#
#

#
#
# 6 Start
#
#
UPDATE `war_world`.`item_infos` SET `Stats` = '1:17;5:18;6:15;', `ObjectLevel` = '45' WHERE (`Entry` = '656153');
#
#
# 6 End
#
#

#
#
# 7 Start
#
#
# Bastion Stair Portal Jumps
-- 167772329
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167772329', '160', '1040866', '993086', '11579', '1600', '1', '0');
-- 167775848
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167775848', '160', '1017032', '1026722', '6189', '1500', '1', '0');
-- 171972648
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('171972648', '160', '1017027', '1026702', '6189', '1', '1', '0');
-- 167782248
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('167782248', '164', '1020790', '999249', '14618', '1500', '1', '4', '164');
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1020785', `WorldY` = '998472', `WorldZ` = '14598', `WorldO` = '1' WHERE (`Entry` = '171972648');
-- 167775464
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167775464', '160', '1017027', '1026702', '6189', '1', '1', '0');
#
#
# 7 End
#
#

#
#
# 8 Start
#
#
# Instance Establishment
-- Lord Slaurith
INSERT INTO `war_world`.`instance_infos` (`instance_infos_ID`, `Entry`, `ZoneID`, `Name`, `LockoutTimer`, `TrashRespawnTimer`, `OrderExitZoneJumpID`, `DestrExitZoneJumpID`) VALUES ('4', '164', '164', 'Lord Slaurith', '1440', '5', '0', '0');
#
#
# 8 End
#
#
# Part2

#
#
#
# Inserts Start
#
#
#

-- Left Wing to Thar'lgnan 167782184
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('167782184', '163', '998950', '988695', '8886', '1000', '1', '0', '163');
-- Thar'lgnan to Left Wing 170923752
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('170923752', '160', '999019', '989291', '8347', '1000', '1', '0');

# BS Center
-- BS Center to First Stage 167819500
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167819500', '160', '1018168', '1015019', '12060', '1000', '1', '0');

# BS First Stage to Center 167772264
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167772264', '160', '1013247', '1018282', '7964', '1000', '1', '0');

# BS First Stage to Second 167772267
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167772267', '160', '1019102', '1016397', '16749', '1000', '1', '0');

# BS Second to First 167772266
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167772266', '160', '1012972', '1015051', '12060', '1000', '1', '0');

# BS Boss Warp - Kaarn the Vanquisher 167782376
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167782376', '165', '1012302', '1002996', '9205', '1000', '1', '0');

# BS Boss Exit - Kaarn the Vanquisher 173020904
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('173020904', '160', '1013206', '1014167', '17849', '1000', '1', '0');

# BS Kaarn Level to Skull Lord Varithrok 167772268
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167772268', '166', '1012146', '1014508', '19238', '1000', '1', '0');

# BS Skull Lord to Karen Level 174063721
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('174063721', '160', '1011513', '1014209', '17989', '1000', '1', '0');

# BS Skull Lord to BS Start 174063726
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('174063726', '160', '1015834', '1028594', '5827', '1500', '1', '0');

# BS WC to Left Wing 167772328
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167772328', '160', '1003631', '1001158', '9712', '1800', '1', '0');

# BS Left Wing to WC 167776232
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167776232', '160', '1014520', '1026710', '6189', '2000', '1', '0');

# Left Wing After Boss 167776360
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('167776360', '160', '1014591', '1026684', '6189', '2000', '1', '0');

# Instance Add Tharlgnan
INSERT INTO `war_world`.`instance_infos` (`instance_infos_ID`, `Entry`, `ZoneID`, `Name`, `LockoutTimer`, `TrashRespawnTimer`, `OrderExitZoneJumpID`, `DestrExitZoneJumpID`) VALUES ('B160', '160', '163', 'Thar\'lgnan', '1440', '5', '0', '0');

# Instance Add Kaarn
INSERT INTO `war_world`.`instance_infos` (`instance_infos_ID`, `Entry`, `ZoneID`, `Name`, `LockoutTimer`, `TrashRespawnTimer`, `OrderExitZoneJumpID`, `DestrExitZoneJumpID`) VALUES ('D160', '160', '165', 'Kaarn the Vanquisher', '1440', '5', '0', '0');

# Instance Add Skull Lord
INSERT INTO `war_world`.`instance_infos` (`instance_infos_ID`, `Entry`, `ZoneID`, `Name`, `LockoutTimer`, `TrashRespawnTimer`, `OrderExitZoneJumpID`, `DestrExitZoneJumpID`) VALUES ('E160', '160', '166', 'Skull Lord Var\'throk', '1440', '5', '0', '0');

# Skull Lord Exit Door 174063720
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('2000', '2', '166', '1017468', '1011553', '19963', '0', '65535', '0', '0', '100', '0', '174063720', '0', '0', '1');

# Lord Saul Gate
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `DoorId`) VALUES ('3000', '2', '164', '1020794', '1002418', '14624', '0', '65535', '171967848');

# Respawn Locations
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('600', '160', '1', '32534', '46842', '5808', '3936', '160');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('601', '160', '1', '32534', '46842', '5808', '3936', '163');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('602', '160', '1', '32534', '46842', '5808', '3936', '164');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('603', '160', '1', '32534', '46842', '5808', '3936', '165');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('604', '160', '1', '32534', '46842', '5808', '3936', '166');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('605', '160', '2', '32534', '46842', '5808', '3936', '160');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('606', '160', '2', '32534', '46842', '5808', '3936', '163');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('607', '160', '2', '32534', '46842', '5808', '3936', '164');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('608', '160', '2', '32534', '46842', '5808', '3936', '165');
INSERT INTO `war_world`.`zone_respawns` (`RespawnID`, `ZoneID`, `Realm`, `PinX`, `PinY`, `PinZ`, `WorldO`, `InZoneID`) VALUES ('609', '160', '2', '32534', '46842', '5808', '3936', '166');
INSERT INTO `war_world`.`zone_areas` (`AreaName`, `ZoneId`, `AreaId`, `Realm`, `PieceId`, `OrderInfluenceId`, `DestroInfluenceId`, `OrderRespawnId`, `DestroRespawnId`) VALUES ('Lord Slaurith', '164', '31', '2', '1', '129', '128', '602', '607');

# Order BS to CW 168030760
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`) VALUES ('168030760', '103', '1472503', '814028', '16067', '1500', '1', '0');

# Order CW to BS 108003496
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('108003496', '160', '1015780', '1034124', '4984', '1024', '1', '4', '160');
#
#
# Inserts End
#
#

#
#
# Updates Start
#
#

# AzukThul Gate Entrance
UPDATE `war_world`.`gameobject_spawns` SET `WorldX` = '993936', `WorldY` = '994219', `WorldZ` = '7172', `Unks` = '', `DoorId` = '167774632' WHERE (`Guid` = '260087');

# Instance Update
UPDATE `war_world`.`instance_infos` SET `Entry` = '160' WHERE (`instance_infos_ID` = '4');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'A60' WHERE (`instance_infos_ID` = '4791faee-8596-11e7-8d92-00ffbe5f3044');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'B60' WHERE (`instance_infos_ID` = 'cee83dcc-9183-11e7-9c82-000c29d63948');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'C60' WHERE (`instance_infos_ID` = 'f335cce1-9183-11e7-9c82-000c29d63948');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'D60' WHERE (`instance_infos_ID` = '0f9d3126-9184-11e7-9c82-000c29d63948');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'E60' WHERE (`instance_infos_ID` = '16ee359e-8f53-11e7-8540-00ffbe5f3044');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'A160' WHERE (`instance_infos_ID` = '9fc2e9bc-9185-11e7-9c82-000c29d63948');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'C160' WHERE (`instance_infos_ID` = '4');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'A179' WHERE (`instance_infos_ID` = '0bb21ab9-9ca6-11e7-8dcf-000c29d63948');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'A195' WHERE (`instance_infos_ID` = '5');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'A196' WHERE (`instance_infos_ID` = '6');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = 'A260' WHERE (`instance_infos_ID` = '10');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '260A' WHERE (`instance_infos_ID` = 'A260');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '196A' WHERE (`instance_infos_ID` = 'A196');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '195A' WHERE (`instance_infos_ID` = 'A195');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '179A' WHERE (`instance_infos_ID` = 'A179');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '160A' WHERE (`instance_infos_ID` = 'A160');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '160B' WHERE (`instance_infos_ID` = 'B160');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '160C' WHERE (`instance_infos_ID` = 'C160');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '160D' WHERE (`instance_infos_ID` = 'D160');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '160E' WHERE (`instance_infos_ID` = 'E160');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '60A' WHERE (`instance_infos_ID` = 'A60');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '60B' WHERE (`instance_infos_ID` = 'B60');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '60C' WHERE (`instance_infos_ID` = 'C60');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '60D' WHERE (`instance_infos_ID` = 'D60');
UPDATE `war_world`.`instance_infos` SET `instance_infos_ID` = '60E' WHERE (`instance_infos_ID` = 'E60');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '160A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '160B');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '160C');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '160D');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '160E');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '179A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '60A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '60B');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '60C');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '60D');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '60E');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '720' WHERE (`instance_infos_ID` = '260A');

# Zone 163 Fix
UPDATE `war_world`.`zone_jumps` SET `Type` = '4' WHERE (`Entry` = '167782184');

# Zone 165 Fix
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '165' WHERE (`Entry` = '167782376');

# Zone 166 Fix
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '166' WHERE (`Entry` = '167772268');

# Zone 160 Fix
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '108856040');

# Height Correction for Doors
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '16153' WHERE (`Guid` = '1003');
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '16153' WHERE (`Guid` = '1002');
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '17011' WHERE (`Guid` = '1004');
UPDATE `war_world`.`gameobject_spawns` SET `WorldX` = '1014256', `WorldZ` = '17011' WHERE (`Guid` = '1001');
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '18242' WHERE (`Guid` = '1001');
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '17011' WHERE (`Guid` = '1000');
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '17011' WHERE (`Guid` = '1001');
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '17869' WHERE (`Guid` = '1005');
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '17869' WHERE (`Guid` = '1000');

# Zone 166 Exit Door Fix
UPDATE `war_world`.`gameobject_spawns` SET `WorldZ` = '19542' WHERE (`Guid` = '2000');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '260447');
UPDATE `war_world`.`gameobject_spawns` SET `WorldX` = '1017425', `WorldY` = '1011561' WHERE (`Guid` = '2000');
# Doesnt work whatever

# Mailbox Location Fix
UPDATE `war_world`.`gameobject_spawns` SET `WorldX` = '1016191', `WorldY` = '1029338', `WorldZ` = '5828' WHERE (`Guid` = '260410');

# BS Respawn Mod and AreaID Mod
UPDATE `war_world`.`zone_areas` SET `AreaId` = '31', `OrderRespawnId` = '600', `DestroRespawnId` = '605' WHERE (`ZoneId` = '160') and (`PieceId` = '2');
UPDATE `war_world`.`zone_areas` SET `AreaId` = '31', `OrderRespawnId` = '600', `DestroRespawnId` = '605' WHERE (`ZoneId` = '160') and (`PieceId` = '3');
UPDATE `war_world`.`zone_areas` SET `OrderRespawnId` = '600', `DestroRespawnId` = '605' WHERE (`ZoneId` = '160') and (`PieceId` = '1');
UPDATE `war_world`.`zone_areas` SET `AreaId` = '0', `Realm` = '0', `PieceId` = '0' WHERE (`ZoneId` = '164') and (`PieceId` = '1');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '164', `InZoneID` = '160' WHERE (`RespawnID` = '607');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '195', `InZoneID` = '161' WHERE (`RespawnID` = '339');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '196', `InZoneID` = '161' WHERE (`RespawnID` = '340');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '163', `InZoneID` = '160' WHERE (`RespawnID` = '601');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '163', `InZoneID` = '160' WHERE (`RespawnID` = '606');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '164', `InZoneID` = '160' WHERE (`RespawnID` = '602');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '165', `InZoneID` = '160' WHERE (`RespawnID` = '603');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '165', `InZoneID` = '160' WHERE (`RespawnID` = '608');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '166', `InZoneID` = '160' WHERE (`RespawnID` = '604');
UPDATE `war_world`.`zone_respawns` SET `ZoneID` = '166', `InZoneID` = '160' WHERE (`RespawnID` = '609');
#
#
# Updates End
#
#
#
# Part 3

#
#
# Delete Start
#
#

# Lost Vale Cleanup
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564536');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564643');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564411');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564412');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564413');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564414');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564415');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564416');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564417');

# Thar Normal Spawn
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081552');

# Thar Rename UpdateDel
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '2000689');
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '2000688');

#
#
# Delete End
#
#

#
#
# Inserts Start
#
#

# Bastion Praag Entrance Destro
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('108003624', '160', '1015782', '1034124', '4984', '2048', '1', '4', '160');

# Tharlgnan Instance Spawn
INSERT INTO `war_world`.`instance_boss_spawns` (`Instance_spawns_ID`, `Entry`, `Realm`, `Level`, `Emote`, `ZoneID`, `InstanceID`, `BossID`, `SpawnGroupID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`) VALUES ('1601000', '45084', '0', '34', '0', '163', '163', '1', '1', '998977', '984755', '9004', '2048');

# Blood Stomp Added
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `Active`, `ActivateOnCombatStart`, `RandomTarget`) VALUES ('45084', '5092', '40', '1631000', 'Blood Stomp', '1', '0', '0');

# Blood Roots Added
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('45084', '5063', '30', '1631001', 'Blood Roots', '0', '0', '0', '1', '1', '0', '1', '0');

# Dog Inserts
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '1000071', '985346', '8554', '2700', '1630000', '0', '33', '163', '1', '1');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '999649', '9855504', '8482', '2700', '1630001', '0', '33', '163', '1', '1');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '998240', '985239', '8499', '2700', '1630002', '0', '33', '163', '1', '1');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '997856', '985316', '8525', '2700', '1630003', '0', '33', '163', '1', '1');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '998180', '986401', '8539', '2700', '1630004', '0', '33', '163', '1', '1');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '999444', '986431', '8521', '2700', '1630005', '0', '33', '163', '1', '1');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '999461', '985683', '8492', '2700', '1630006', '0', '33', '163', '1', '1');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '998785', '985763', '8482', '2700', '1630007', '0', '33', '163', '1', '1');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('45088', '998414', '986148', '8475', '2700', '1630008', '0', '33', '163', '1', '1');

#
#
# Inserts End
#
#

#
#
# Update Start
#
#

# Zone Jump Group Size Correction
UPDATE `war_world`.`zone_jumps` SET `Type` = '6' WHERE (`Entry` = '211812584');
UPDATE `war_world`.`zone_jumps` SET `Type` = '6' WHERE (`Entry` = '211812648');
UPDATE `war_world`.`zone_jumps` SET `Type` = '6' WHERE (`Entry` = '168899368');
UPDATE `war_world`.`zone_jumps` SET `Type` = '6' WHERE (`Entry` = '168944168');
UPDATE `war_world`.`zone_jumps` SET `Type` = '6' WHERE (`Entry` = '167772268');
UPDATE `war_world`.`zone_jumps` SET `Type` = '6' WHERE (`Entry` = '167782376');
UPDATE `war_world`.`zone_jumps` SET `Type` = '6' WHERE (`Entry` = '167782248');
UPDATE `war_world`.`zone_jumps` SET `Type` = '6' WHERE (`Entry` = '167782184');
UPDATE `war_world`.`zone_jumps` SET `InstanceID` = NULL WHERE (`Entry` = '2086761');

# Bastion Zone in Heading
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2048' WHERE (`Entry` = '108003496');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2048' WHERE (`Entry` = '108856040');

# BS Thar
UPDATE `war_world`.`creature_protos` SET `Name` = 'Thar\'lgnan^M' WHERE (`Entry` = '45084');
UPDATE `war_world`.`creature_protos` SET `MaxLevel` = '34' WHERE (`Entry` = '45084');
UPDATE `war_world`.`creature_protos` SET `Name` = 'Frenzied Bloodsnout^f' WHERE (`Entry` = '45088');
UPDATE `war_world`.`instance_infos` SET `Entry` = '163' WHERE (`instance_infos_ID` = '160B');

# Entry must equal instance id from zone jump
UPDATE `war_world`.`instance_infos` SET `Entry` = '164' WHERE (`instance_infos_ID` = '160C');
UPDATE `war_world`.`instance_infos` SET `Entry` = '165' WHERE (`instance_infos_ID` = '160D');
UPDATE `war_world`.`instance_infos` SET `Entry` = '166' WHERE (`instance_infos_ID` = '160E');

# Zone In Heading
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2048' WHERE (`Entry` = '167782184');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '1024' WHERE (`Entry` = '167772268');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '1024' WHERE (`Entry` = '167782376');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '1024' WHERE (`Entry` = '167782248');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2048' WHERE (`Entry` = '211812584');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2048' WHERE (`Entry` = '211812648');

#
#
# Update End
#
#
# Blood Stomp
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `MinRange`, `Range`, `CastTime`, `Cooldown`, `ApCost`, `SpecialCost`, `MoveCast`, `EffectID`, `Specline`, `WeaponNeeded`, `AffectsDead`, `IgnoreGlobalCooldown`, `MinimumRank`, `MinimumRenown`) VALUES ('5092', '0', 'Blood Stomp', '0', '430', '0', '40', '0', '0', '1', '3236', 'NPC', '0', '0', '1', '0', '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('5092', 'Blood Stomp', '0', '0', 'DealDamage', '17', 'Enemy');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('5092', 'Blood Stomp', '0', '1', 'InvokeBuff', '5092', 'Enemy');
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `CastTimeDamageMult`, `WeaponDamageFrom`, `WeaponDamageScale`, `StatUsed`, `StatDamageScale`, `HatredScale`, `HealHatredScale`) VALUES ('5092', '0', 'Blood Stomp', '72', '540', 'Physical', '0', '0', '0', '0', '0', '0', '0', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `TypeString`, `MaxCopies`, `MaxStack`) VALUES ('5092', 'Blood Stomp', 'Career', 'Debuff', '1', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('5092', 'Blood Stomp', '0', '0', 'ApplyCC', '32', '1', '1', 'Enemy', '1');

# Lord slau
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081759');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081760');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081761');
INSERT INTO `war_world`.`instance_boss_spawns` (`Instance_spawns_ID`, `Entry`, `Realm`, `Level`, `Emote`, `ZoneID`, `InstanceID`, `BossID`, `SpawnGroupID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`) VALUES ('1602000', '48112', '0', '40', '0', '164', '164', '1', '1', '1020808', '1001549', '14401', '2048');

# Kaarn
INSERT INTO `war_world`.`instance_boss_spawns` (`Instance_spawns_ID`, `Entry`, `Realm`, `Level`, `Emote`, `ZoneID`, `InstanceID`, `BossID`, `SpawnGroupID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`) VALUES ('1603000', '2000751', '0', '39', '0', '165', '165', '1', '1', '1012370', '1000647', '9439', '0');

# Skull Lord
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081738');
INSERT INTO `war_world`.`instance_boss_spawns` (`Instance_spawns_ID`, `Entry`, `Realm`, `Level`, `Emote`, `ZoneID`, `InstanceID`, `BossID`, `SpawnGroupID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`) VALUES ('1604000', '64106', '0', '40', '0', '166', '166', '1', '1', '1015782', '1001792', '21999', '4078');
# Blood Stomp
UPDATE `war_world`.`ability_commands` SET `PrimaryValue` = NULL WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `TypeString` = NULL WHERE (`Entry` = '5092');
-- v2
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '5092');

# Godslayer Name
UPDATE `war_world`.`creature_protos` SET `Name` = 'Kaarn the Vanquisher^M', `MinLevel` = '39', `MaxLevel` = '39', `CreatureType` = '10', `CreatureSubType` = '53' WHERE (`Entry` = '2000751');
UPDATE `war_world`.`creature_protos` SET `Name` = 'Barakus the Godslayer^M' WHERE (`Entry` = '46205');
UPDATE `war_world`.`creature_spawns` SET `Entry` = '46205' WHERE (`Guid` = '1086176');

# Skull Lord Lv
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '40', `MaxLevel` = '40' WHERE (`Entry` = '64106');
# Bloodwraught Enclave Tweaks
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '4096' WHERE (`Entry` = '168944168');
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('204478248', '195', '1567385', '1048964', '11594', '2070', '1', '6', '195');
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('204473832', '195', '1589243', '1059988', '7490', '6', '1', '6', '195');
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('204476136', '195', '1567383', '1047049', '11594', '4050', '1', '6', '195');
UPDATE `war_world`.`zone_respawns` SET `PinX` = '30233', `PinY` = '44876', `PinZ` = '17384', `WorldO` = '2048' WHERE (`RespawnID` = '339');
UPDATE `war_world`.`zone_respawns` SET `PinX` = '30234', `PinY` = '44876', `PinZ` = '17384', `WorldO` = '2048' WHERE (`RespawnID` = '340');
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('204473768', '195', '1560222', '1048000', '11400', '1092', '1', '6', '195');
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('204478312', '195', '1566447', '1048001', '11594', '3056', '1', '6', '195');
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('204473704', '195', '1566894', '1041142', '11912', '1960', '1', '6', '195');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('1950', '2', '195', '1589242', '1065242', '7496', '0', '65535', '0', '0', '100', '0', '204475432', '0', '0', '1');

# Makes IC Doors Open as Deafault
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250282');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250280');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250283');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250320');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250319');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250318');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250317');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250316');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250285');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250284');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250281');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250279');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250278');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250261');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250260');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250259');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250257');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250256');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250255');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250254');

# Add iron body
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('64106', '14891', '15', '1604000', 'Blood!', '0', '100', '0', '1', '1', '1', '0', '0');

# Correct Iron Body value
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '-50' WHERE (`Entry` = '14891') and (`CommandID` = '0') and (`CommandSequence` = '0');
DROP TABLE war_world.creature_protos_bo_backup;
DROP TABLE war_world.creature_spawns_backup;
DROP TABLE war_world.emote_backup_creature_protos;
DROP TABLE war_world.gameobject_spawns_chest_bkp;
# Updates BossID to unique values
UPDATE `war_world`.`instance_boss_spawns` SET `BossID` = '600' WHERE (`Instance_spawns_ID` = '600000');
UPDATE `war_world`.`instance_boss_spawns` SET `BossID` = '601' WHERE (`Instance_spawns_ID` = '600001');
UPDATE `war_world`.`instance_boss_spawns` SET `BossID` = '602' WHERE (`Instance_spawns_ID` = '600002');
UPDATE `war_world`.`instance_boss_spawns` SET `BossID` = '603' WHERE (`Instance_spawns_ID` = '600003');
UPDATE `war_world`.`instance_boss_spawns` SET `BossID` = '163' WHERE (`Instance_spawns_ID` = '1604000');
UPDATE `war_world`.`instance_boss_spawns` SET `BossID` = '160' WHERE (`Instance_spawns_ID` = '1601000');
UPDATE `war_world`.`instance_boss_spawns` SET `BossID` = '161' WHERE (`Instance_spawns_ID` = '1602000');
UPDATE `war_world`.`instance_boss_spawns` SET `BossID` = '162' WHERE (`Instance_spawns_ID` = '1603000');
# Heading Only
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2090' WHERE (`Entry` = '167782376');

# Heading and Instance and Type
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2066', `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '173020904');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '1001');
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1473305', `WorldY` = '817221', `WorldZ` = '16248', `WorldO` = '722' WHERE (`Entry` = '168030760');
UPDATE `war_world`.`instance_infos` SET `DestrExitZoneJumpID` = '168030760' WHERE (`instance_infos_ID` = '160A');
UPDATE `war_world`.`instance_infos` SET `DestrExitZoneJumpID` = '168030760' WHERE (`instance_infos_ID` = '160B');
UPDATE `war_world`.`instance_infos` SET `DestrExitZoneJumpID` = '168030760' WHERE (`instance_infos_ID` = '160C');
UPDATE `war_world`.`instance_infos` SET `DestrExitZoneJumpID` = '168030760' WHERE (`instance_infos_ID` = '160D');
UPDATE `war_world`.`instance_infos` SET `DestrExitZoneJumpID` = '168030760' WHERE (`instance_infos_ID` = '160E');
UPDATE `war_world`.`instance_infos` SET `OrderExitZoneJumpID` = '167782696' WHERE (`instance_infos_ID` = '160A');
UPDATE `war_world`.`instance_infos` SET `OrderExitZoneJumpID` = '167782696' WHERE (`instance_infos_ID` = '160B');
UPDATE `war_world`.`instance_infos` SET `OrderExitZoneJumpID` = '167782696' WHERE (`instance_infos_ID` = '160C');
UPDATE `war_world`.`instance_infos` SET `OrderExitZoneJumpID` = '167782696' WHERE (`instance_infos_ID` = '160D');
UPDATE `war_world`.`instance_infos` SET `OrderExitZoneJumpID` = '167782696' WHERE (`instance_infos_ID` = '160E');
# Darkpromise Beast
UPDATE `war_world`.`creature_protos` SET `CreatureType` = '10', `CreatureSubType` = '54' WHERE (`Entry` = '4276');
# Skull Lord Mod
UPDATE `war_world`.`creature_protos` SET `Ranged` = '25', `PowerModifier` = '2', `WoundsModifier` = '5' WHERE (`Entry` = '64106');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '60' WHERE (`creature_abilities_ID` = '1604000');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('64106', '4427', '1604001', 'SKULLS! SKULLS FOR THE SKULL THRONE!', '480', '0', '0', '0', '1', '0', '0', '0');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = NULL, `Text` = 'BLOOD FOR THE BLOOD GOD!' WHERE (`creature_abilities_ID` = '1604000');
UPDATE `war_world`.`creature_protos` SET `Ranged` = '50' WHERE (`Entry` = '64106');

# Blood Stomp buff_commands remove
DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '0');

# Blood Stomp Damage update
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '2000', `MaxDamage` = '2000', `DamageVariance` = '0', `CastTimeDamageMult` = '1', `WeaponDamageFrom` = '1', `WeaponDamageScale` = '1', `StatUsed` = '0', `StatDamageScale` = '1', `PriStatMultiplier` = '1' WHERE (`Entry` = '5092') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

# Thar Proto Update
UPDATE `war_world`.`creature_protos` SET `Ranged` = '15', `PowerModifier` = '2', `WoundsModifier` = '3' WHERE (`Entry` = '45084');

# Thar Enrage
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('45084', '4427', '1631002', 'unga unga unga', '480', '0', '0', '0', '1', '0', '0', '0');

# Skull Lord Ability Number Change
UPDATE `war_world`.`creature_abilities` SET `creature_abilities_ID` = '1664000' WHERE (`creature_abilities_ID` = '1604000');
UPDATE `war_world`.`creature_abilities` SET `creature_abilities_ID` = '1664001' WHERE (`creature_abilities_ID` = '1604001');

# Skull Lord Enrage Change
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '540' WHERE (`creature_abilities_ID` = '1664000');

# Blood Stomp Damage Update
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500', `MaxDamage` = '1000' WHERE (`Entry` = '5092') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

# Lord Saul Lv Update
UPDATE `war_world`.`instance_boss_spawns` SET `Level` = '36' WHERE (`Instance_spawns_ID` = '1602000');

# Add Enrage Skull Lord 13928
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('64106', '13928', '0', '1664002', 'Let the hate flow through you.', '0', '100', '0', '1', '1', '0', '1', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `MinRange`, `Range`, `CastTime`, `Cooldown`, `ApCost`, `SpecialCost`, `MoveCast`, `InvokeDelay`, `EffectDelay`, `EffectID`, `ChannelID`, `CooldownEntry`, `ToggleEntry`, `CastAngle`, `AbilityType`, `MasteryTree`, `Specline`, `WeaponNeeded`, `AffectsDead`, `IgnoreGlobalCooldown`, `IgnoreOwnModifiers`, `Fragile`, `MinimumRank`, `MinimumRenown`, `PointCost`, `CashCost`, `AIRange`) VALUES ('13928', '0', 'Enrage', '0', '200', '0', '0', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '255', '0', 'NPC', '0', '0', '0', '0', '0', '0', '0', '0', '0', '200');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('13928', 'Enrage', '0', '0', 'InvokeBuff', '13928', 'Enemy');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`) VALUES ('13928', 'Enrage');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('13928', 'Enrage', '0', '0', 'ModifyPercentageStat', '67', '200', '1', 'Enemy', '2');

# Update Iron Body
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '60' WHERE (`creature_abilities_ID` = '1664000');

# Skull Lord Ab
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('64106', '5968', '0', '1664003', 'Fear!', '0', '100', '0', '0', '1', '1', '0', '0');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '0' WHERE (`creature_abilities_ID` = '1664001');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('64106', '5057', '60', '1664004', 'Infectious Rage', '0', '100', '0', '1', '1', '0', '1', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('64106', '5054', '75', '1664005', 'Destroy Mind', '0', '100', '0', '1', '1', '0', '1', '0');
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '30' WHERE (`creature_abilities_ID` = '1664004');

# Bloodlord Squig Herder
UPDATE `war_world`.`item_infos` SET `Stats` = '4:19;6:14;8:32;79:52;', `ObjectLevel` = '48' WHERE (`Entry` = '436303');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3', `ItemSet` = '6665' WHERE (`Entry` = '436303');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6665' WHERE (`Entry` = '436327');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3', `ItemSet` = '6665' WHERE (`Entry` = '436315');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436327');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3', `ItemSet` = '6665' WHERE (`Entry` = '436339');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('436303:Bloodlord Chewtop|436315:Bloodlord Sholdateef|436327:Bloodlord Squignoggin|436339:Bloodlord Bitestoppas|436351:Bloodlord Squigkickas|', '34:8,62,0|35:15,320,0|36:4,62,0|37:93,2,0|', '6665', 'Bloodlord Squig Calla\'s Kit', '40');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3', `ItemSet` = '6665' WHERE (`Entry` = '436351');

# Zealot Bloodlord Fix
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6666' WHERE (`Entry` = '436306');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6666' WHERE (`Entry` = '436318');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6666' WHERE (`Entry` = '436330');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6666' WHERE (`Entry` = '436342');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6666' WHERE (`Entry` = '436354');

# Darkboots of the Bloodborne
UPDATE `war_world`.`item_infos` SET `Stats` = '1:17;5:18;7:15;', `ObjectLevel` = '45' WHERE (`Entry` = '656148');

# Bloodlord HP regen fix
UPDATE `war_world`.`item_infos` SET `Stats` = '3:32;6:14;9:19;79:13;' WHERE (`Entry` = '436306');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:19;6:14;8:32;79:13;' WHERE (`Entry` = '436303');

# Skull Lord Iron Body Remove
DELETE FROM `war_world`.`creature_abilities` WHERE (`creature_abilities_ID` = '1664000');

# Bloodlord Chest Item Update
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436172');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436179');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436300');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436305');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436306');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436307');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436308');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436309');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436311');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436175');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436174');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436168');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436304');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436310');

# Bloodlord Rarity Update
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436172');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436179');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436193');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436196');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436199');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436202');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436203');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436300');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436305');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436306');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436307');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436308');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436309');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436311');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436324');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436325');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436328');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436329');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436330');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436331');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436332');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436333');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436334');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436335');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436201');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436200');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436198');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436197');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436195');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436194');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436192');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436175');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436174');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436168');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436304');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436326');
UPDATE `war_world`.`item_infos` SET `Rarity` = '3' WHERE (`Entry` = '436310');

# Boss Power Update
UPDATE `war_world`.`creature_protos` SET `Ranged` = '45', `PowerModifier` = '1' WHERE (`Entry` = '64106');
UPDATE `war_world`.`creature_protos` SET `Ranged` = '10', `PowerModifier` = '1' WHERE (`Entry` = '45084');

# Add Terror Mobs
# After some investigation this is a two part abillity
# Terror is applied to the boss and terror is applied to the player
# I think that terror is applied via the Cause Terror and Cause Fear abilities
# I do not know if it is aura based but i would assume so
UPDATE `war_world`.`creature_abilities` SET `Text` = '' WHERE (`creature_abilities_ID` = '1664003');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('45084', '5968', '0', '1631003', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('48112', '5968', '0', '1642000', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('2000751', '5968', '0', '1653000', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('4276', '5968', '0', '2601003', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6821', '5968', '0', '2603002', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6841', '5968', '0', '2604003', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('46205', '5968', '0', '1954000', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('2000757', '5968', '0', '1953000', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('46995', '5968', '0', '1952000', '0', '10', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('2000763', '5968', '0', '1951000', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6843', '6841', '0', '2605003', '0', '100', '0', '0', '1', '1', '0', '0');

# Scalebreaker Choppa Update
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '55' WHERE (`Entry` = '640228');

# Default Bloodwrought Encalve Door Shut
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '0' WHERE (`Guid` = '250285');

# Proto Wounds Update
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '48112');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '45084');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '2000751');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '2000763');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '46995');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '2000757');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '46205');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '4276');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '6821');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '5' WHERE (`Entry` = '6841');
UPDATE `war_world`.`creature_protos` SET `PowerModifier` = '1', `WoundsModifier` = '5' WHERE (`Entry` = '6843');

# Infectious Bite Update
UPDATE `war_world`.`abilities` SET `Cooldown` = '5' WHERE (`Entry` = '5700');
UPDATE `war_world`.`buff_infos` SET `Duration` = '5000', `Interval` = '1000' WHERE (`Entry` = '5700');
UPDATE `war_world`.`buff_infos` SET `Duration` = '5', `Interval` = '1' WHERE (`Entry` = '5700');
UPDATE `war_world`.`buff_infos` SET `Interval` = '1000' WHERE (`Entry` = '5700');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '1500', `MaxDamage` = '2500' WHERE (`Entry` = '5700') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '1000', `MaxDamage` = '1000' WHERE (`Entry` = '5700') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '1000', `MaxDamage` = '1000' WHERE (`Entry` = '5700') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '2000', `MaxDamage` = '2000' WHERE (`Entry` = '5700') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

# Whitefire Weird Shit
UPDATE `war_world`.`creature_protos` SET `PowerModifier` = '.5', `WoundsModifier` = '1' WHERE (`Entry` = '6843');

# Duplicate Creature Spawn Remove
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564338');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564594');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564388');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564418');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564520');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564300');

# Whitefire Level Drop
UPDATE `war_world`.`instance_boss_spawns` SET `Level` = '44' WHERE (`Instance_spawns_ID` = '2600004');

# Whitefire doesnt ahve terror idk why
DELETE FROM `war_world`.`creature_abilities` WHERE (`creature_abilities_ID` = '2605003');

# Possible Fix for worm being invisible
UPDATE `war_world`.`creature_protos` SET `Model1` = '1606', `Model2` = '0' WHERE (`Entry` = '4276');

UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250546');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250598');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250579');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250578');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250577');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250627');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250600');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250625');
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250601');
# Destro Healer
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081463');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`) VALUES ('7147', '1015509', '1030003', '5828', '2377', '1600000', '2', '40', '160');

# No Instance Exit Portal Fix
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1473304', `WorldY` = '817221', `WorldZ` = '16248', `WorldO` = '722' WHERE (`Entry` = '167782696');

# Destro Rally Master
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081461');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`) VALUES ('9218', '1017191', '1029676', '5808', '1001', '1600001', '2', '40', '160');

# Destro Livia Aselta
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081464');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`) VALUES ('7145', '1016537', '1029901', '5808', '1308', '1600002', '2', '25', '160');

# Destro Shakal Daemoncaller
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081460');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`) VALUES ('7135', '1014957', '1029342', '5828', '3481', '1600003', '2', '25', '160');

# Vidkun Geldik
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081465');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`) VALUES ('7155', '1014532', '1030029', '5828', '2787', '1600004', '2', '25', '160');

# Jodis Wolfscar
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1081462');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `ZoneID`) VALUES ('7141', '1015513', '1029330', '5828', '3936', '1600005', '2', '25', '160');

ALTER TABLE `war_world`.`creature_protos` 
CHANGE COLUMN `Unk2` `NpcRank` SMALLINT(5) UNSIGNED NOT NULL DEFAULT '0' ;
UPDATE `war_world`.`gameobject_spawns` SET `VfxState` = '1' WHERE (`Guid` = '250602');
ALTER TABLE `war_world`.`instance_objects` 
ADD COLUMN `Realm` TINYINT(3) UNSIGNED NULL DEFAULT NULL AFTER `VfxState`;

ALTER TABLE `war_world`.`instance_objects` 
CHANGE COLUMN `EncounterID` `BossID` INT(10) UNSIGNED NULL DEFAULT NULL ;
# Spider
UPDATE `war_world`.`creature_protos` SET `PowerModifier` = '1', `WoundsModifier` = '1' WHERE (`Entry` = '6843');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6843', '5968', '0', '2605003', '0', '100', '0', '0', '1', '1', '0', '0');

# Worm
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '1' WHERE (`Entry` = '4276');

# Lizard
DELETE FROM `war_world`.`creature_abilities` WHERE (`creature_abilities_ID` = '2602001');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('59211', '5968', '0', '2602003', '0', '100', '0', '0', '1', '1', '0', '0');
UPDATE `war_world`.`creature_protos` SET `PowerModifier` = '1', `WoundsModifier` = '1' WHERE (`Entry` = '59211');

# Malghor
UPDATE `war_world`.`creature_protos` SET `Name` = 'Malghor Greathorn^M', `PowerModifier` = '1', `WoundsModifier` = '1' WHERE (`Entry` = '6821');

# Horgulul
UPDATE `war_world`.`creature_protos` SET `Name` = 'Horgulul^F', `PowerModifier` = '1', `WoundsModifier` = '1' WHERE (`Entry` = '6841');

# Entry Location
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1410081', `WorldY` = '1588460', `WorldZ` = '5828', `WorldO` = '2054' WHERE (`Entry` = '211812584');
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1410082', `WorldY` = '1588460', `WorldZ` = '5828', `WorldO` = '2054' WHERE (`Entry` = '211812648');

# Terror
UPDATE `war_world`.`abilities` SET `Range` = '320', `CastTime` = '0', `Cooldown` = '0', `ApCost` = '0', `AbilityType` = '1', `Specline` = 'NPC', `MinimumRank` = '0', `MinimumRenown` = '0', `AIRange` = '320' WHERE (`Entry` = '5968');
UPDATE `war_world`.`buff_commands` SET `PrimaryValue` = '24' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('5968', 'Terror', '0', '1', 'ModifyPercentageStat', '33', '-2', '5', 'Host', '1');
UPDATE `war_world`.`ability_commands` SET `EffectRadius` = '255' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');

# Terror Cast
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1631003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1642000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1653000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1664003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1951000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1952000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1953000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1954000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2601003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2602003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2603002');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2604003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2605003');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '1631003');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '1642000');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '1653000');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '1664003');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '1951000');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '1952000');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '1953000');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '1954000');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '2601003');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '2602003');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '2603002');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '2604003');
UPDATE `war_world`.`creature_abilities` SET `AbilityCycle` = '1' WHERE (`creature_abilities_ID` = '2605003');

# Terror Bounce
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`, `MaxTargets`) VALUES ('5968', 'Terror', '1', '0', 'InvokeBouncingBuff', '5968', 'AllyOrSelf', '6');

# Health and Damage
UPDATE `war_world`.`creature_protos` SET `PowerModifier` = '.5', `WoundsModifier` = '3.75' WHERE (`Entry` = '6843');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '3.75' WHERE (`Entry` = '59211');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '3.75' WHERE (`Entry` = '4276');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '3.75' WHERE (`Entry` = '6821');
UPDATE `war_world`.`creature_protos` SET `PowerModifier` = '1.5' WHERE (`Entry` = '6821');
UPDATE `war_world`.`creature_protos` SET `PowerModifier` = '1.5' WHERE (`Entry` = '4276');
UPDATE `war_world`.`creature_protos` SET `PowerModifier` = '1.5' WHERE (`Entry` = '59211');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '2' WHERE (`Entry` = '45084');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '2' WHERE (`Entry` = '48112');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '2' WHERE (`Entry` = '2000751');
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '2' WHERE (`Entry` = '64106');
# Random Quests
DELETE FROM `war_world`.`quests` WHERE (`Entry` = '0');
DELETE FROM `war_world`.`quests` WHERE (`Entry` = '601');

# Skull Item
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ScriptName`, `ObjectLevel`, `UniqueEquiped`, `Crafts`, `Unk27`, `SellRequiredItems`, `TwoHanded`, `ItemSet`, `Craftresult`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('1', 'Skull', 'A skull pulled from Skull Lord Var’lthrok\'s pile.', '21', '0', '3419', '0', '0', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');

# Quest Add
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('1', 'Rivers of Blood', '5', '30', '40', '0', '100', 'A great rumbling has been felt through the lands of Kadrin Valley. The frozen rivers once skull white are now stained. Great slabs of crimson colored marble now stand in their place. Warbands of Khorne daemons haven been seen heading south to Thunder Mountain. While it is not clear why these daemons have suddenly appeared so far from their home in the Chaos Wastes, word has spread that a champion of Khorne has appeared. Be warned warrior, champions of Khorne dream to build a mountain of skulls to appease their god.', 'Darkness has fallen over the dwarven lands once again. If these whispers are true, the daemon must be vanquished from this realm and pushed back to his Bastion Stair. You must act quickly, lest the daemons of Khorne invade this land and bring ruin to all. No doubt the allies of the chaos gods will be among this beast of Khorne. Bring justice to all those that support these vile daemons.', '6666', '0', '0', '[129838021,5]', '1', '0', '0', 'You have done well. Skull Lord Var’lthrok and his supporters are now banished to his Bastion Stair. Claim your new weapon and send this daemon to hell once and for all.', '0', '1', '1');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('2', 'Rivers of Blood', '5', '30', '40', '0', '100', 'A great rumbling has been felt through the lands of Kadrin Valley. The frozen rivers once skull white are now stained. Great slabs of crimson colored marble now stand in their place. Warbands of Khorne daemons haven been seen heading south to Thunder Mountain. While it is not clear why these daemons have suddenly appeared so far from their home in the Chaos Wastes, word has spread that a champion of Khorne has appeared. Be warned warrior, champions of Khorne dream to build a mountain of skulls to appease their god.', 'A Lord of Khorne dares to interfere with the war path of the armies of Destruction for ownership over the dwarven strongholds. Push back this beast to Bastion Stair in which he belongs, and slay all those that get in your way.', '6666', '0', '0', '[129838021,5]', '1', '0', '0', 'You have done well. Skull Lord Var’lthrok and his supporters are now banished to his Bastion Stair. Claim your new weapon and send this daemon to hell once and for all.', '0', '1', '1');
UPDATE `war_world`.`quests` SET `Choice` = '[129838021,5],[1,1]', `ChoiceCount` = '2' WHERE (`Entry` = '1');
UPDATE `war_world`.`quests` SET `Choice` = '[129838021,5],[1,1]', `ChoiceCount` = '2' WHERE (`Entry` = '2');

# Destro Quest NPC
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('1', '343920');
UPDATE `war_world`.`quests_creature_starter` SET `Entry` = '2' WHERE (`Entry` = '1');

# Order Quest NPC
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('1', '344539');

# Quest Objectives
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `inZones`, `PreviousObj`) VALUES ('74835849', '1', '5', '1', 'Skull Lord Var\'Ithrok', '64106', '5', '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PreviousObj`) VALUES ('74835850', '1', '2', '200', 'Enemy Players', '0', '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `inZones`, `PreviousObj`) VALUES ('74835851', '2', '5', '1', 'Skull Lord Var\'Ithrok', '64106', '5', '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PreviousObj`) VALUES ('74835852', '2', '2', '200', 'Enemy Players', '0', '0');

# Quest Starter Proto
UPDATE `war_world`.`quests_creature_starter` SET `CreatureID` = '462' WHERE (`Entry` = '2');
UPDATE `war_world`.`quests_creature_starter` SET `CreatureID` = '98662' WHERE (`Entry` = '2');
UPDATE `war_world`.`quests_creature_starter` SET `CreatureID` = '98807' WHERE (`Entry` = '1');

# Enemy Player Update
UPDATE `war_world`.`quests_objectives` SET `ObjID` = '0' WHERE (`Guid` = '74835852');
UPDATE `war_world`.`quests_objectives` SET `ObjID` = '0' WHERE (`Guid` = '74835850');
UPDATE `war_world`.`quests_objectives` SET `inZones` = '3,5,9' WHERE (`Guid` = '74835852');
UPDATE `war_world`.`quests_objectives` SET `inZones` = '3,5,9' WHERE (`Guid` = '74835850');

# Quest Type
UPDATE `war_world`.`quests` SET `Type` = '32' WHERE (`Entry` = '1');
UPDATE `war_world`.`quests` SET `Type` = '32' WHERE (`Entry` = '2');

UPDATE `war_world`.`quests_objectives` SET `ObjType` = '5', `inZones` = '' WHERE (`Guid` = '74835852');
UPDATE `war_world`.`quests_objectives` SET `ObjType` = '2' WHERE (`Guid` = '74835851');
UPDATE `war_world`.`quests_objectives` SET `ObjType` = '5', `inZones` = '' WHERE (`Guid` = '74835850');
UPDATE `war_world`.`quests_objectives` SET `ObjType` = '2' WHERE (`Guid` = '74835849');

# Headwall Door
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unks`, `DoorId`, `VfxState`, `SoundId`, `AllowVfxUpdate`) VALUES ('5000', '2', '5', '1389149', '920426', '11721', '0', '65535', '0', '0', '0', '0', '0', '5298280', '0', '0', '1');
# Quest Finisher
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('1', '98807');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('2', '98662');

# Items
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('100', 'Bloodlord Chaos Sword', '1', '64', '8936', '10', '4', '0', '1', '1', '0', '0', '550', '240', '40', '0', '0', '1:100;5:100:4:-140', '0', '1', '60', '1', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '100', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `NpcRank`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('166666', 'Magpie', '1691', '0', '50', '50', '40', '40', '129', '50', '0', '0', '0', '0', '0', '1002', '0', '0', '0', '0', '0', '16', '89', '0', '0', '422', '0', '0', '0', '1', '1', '0', '0', '0');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('1188000', '166666', '103', '1473283', '817644', '16248', '1346', '0', '0', '131', '0', '40', '0', '1', '1');
UPDATE `war_world`.`creature_protos` SET `Faction` = '128' WHERE (`Entry` = '166666');
UPDATE `war_world`.`creature_spawns` SET `Faction` = '128' WHERE (`Guid` = '1188000');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1600000', '20', '5937', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1600001', '24', '5938', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1600002', '23', '5940', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1600003', '21', '6937', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1600004', '22', '2739', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1600005', '28', '2743', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1600006', '27', '27', '0', '0', '0');
UPDATE `war_world`.`creature_items` SET `Entry` = '436307' WHERE (`Entry` = '1600000') and (`SlotId` = '20');
UPDATE `war_world`.`creature_items` SET `Entry` = '166666' WHERE (`Entry` = '436307') and (`SlotId` = '20');
UPDATE `war_world`.`creature_items` SET `Entry` = '166666' WHERE (`Entry` = '1600001') and (`SlotId` = '24');
UPDATE `war_world`.`creature_items` SET `Entry` = '166666' WHERE (`Entry` = '1600002') and (`SlotId` = '23');
UPDATE `war_world`.`creature_items` SET `Entry` = '166666' WHERE (`Entry` = '1600003') and (`SlotId` = '21');
UPDATE `war_world`.`creature_items` SET `Entry` = '166666' WHERE (`Entry` = '1600004') and (`SlotId` = '22');
UPDATE `war_world`.`creature_items` SET `Entry` = '166666' WHERE (`Entry` = '1600005') and (`SlotId` = '28');
UPDATE `war_world`.`creature_items` SET `Entry` = '166666' WHERE (`Entry` = '1600006') and (`SlotId` = '27');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '20');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '24');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '23');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '21');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '22');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '28');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '27');
UPDATE `war_world`.`creature_items` SET `SecondaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '20');
UPDATE `war_world`.`creature_items` SET `SecondaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '24');
UPDATE `war_world`.`creature_items` SET `SecondaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '23');
UPDATE `war_world`.`creature_items` SET `SecondaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '21');
UPDATE `war_world`.`creature_items` SET `SecondaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '22');
UPDATE `war_world`.`creature_items` SET `SecondaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '28');
UPDATE `war_world`.`creature_items` SET `SecondaryColor` = '125' WHERE (`Entry` = '166666') and (`SlotId` = '27');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('16666', '10', '8846', '0', '0', '0');
UPDATE `war_world`.`creature_items` SET `ModelId` = '1952' WHERE (`Entry` = '166666') and (`SlotId` = '27');
UPDATE `war_world`.`creature_items` SET `ModelId` = '1952' WHERE (`Entry` = '166666') and (`SlotId` = '27');
UPDATE `war_world`.`creature_protos` SET `Title` = '10' WHERE (`Entry` = '166666');
UPDATE `war_world`.`item_infos` SET `Type` = '0' WHERE (`Entry` = '1');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;' WHERE (`Entry` = '100');

UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1410092' WHERE (`Entry` = '211812648');
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1410091' WHERE (`Entry` = '211812584');
UPDATE `war_world`.`ability_commands` SET `EffectRadius` = '0' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `Duration` = '10' WHERE (`Entry` = '5968');
UPDATE `war_world`.`ability_commands` SET `Target` = 'Group' WHERE (`Entry` = '5968') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `Duration` = '30' WHERE (`Entry` = '5968');
DELETE FROM `war_world`.`item_infos` WHERE (`Entry` = '100');
DELETE FROM `war_world`.`vendor_items` WHERE (`VendorId` = '422') and (`ItemId` = '100');
DROP TABLE `war_world`.`quests_bkp`;
DROP TABLE `war_world`.`quests_maps_bkp`;
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:6,62,0|35:1,62,0|36:29,5,0|37:93,2,0|', '6646', 'Bloodlord Accursed Defender Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:9,62,0|35:5,62,0|36:78,5,0|37:93,2,0|', '6649', 'Bloodlord Daemoncaller\'s Robes', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:1,62,0|35:26,484,0|36:76,5,0|37:93,2,0|', '6663', 'Bloodlord Warped Ravager Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:6,62,0|35:1,62,0|36:29,5,0|37:93,2,0|', '6651', 'Bloodlord Dusk Warrior\'s Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:9,62,0|35:3,62,0|36:78,5,0|37:93,2,0|', '6648', 'Bloodlord Blood Offering Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:1,62,0|35:26,484,0|36:76,5,0|37:93,2,0|', '6658', 'Bloodlord Painbringer\'s Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:9,62,0|35:3,62,0|36:78,5,0|37:93,2,0|', '6661', 'Bloodlord Waaaghbringa\'s Fings', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:8,62,0|35:15,320,0|36:4,62,0|37:93,2,0|', '6664', 'Bloodlord Work Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('436204:Bloodlord Irongaunts|436216:Bloodlord Steadkeeps|436192:Bloodlord Greathelm|436180:Bloodlord Ironmantle|436168:Bloodlord Klad|', '34:6,62,0|35:1,62,0|36:29,5,0|37:93,2,0|', '6662', 'Bloodlord War Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:9,62,0|35:3,62,0|36:78,5,0|37:93,2,0|', '6659', 'Bloodlord Runerobes', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:1,62,0|35:26,484,0|36:76,5,0|37:93,2,0|', '6647', 'Bloodlord Battle Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:9,62,0|35:5,62,0|36:78,5,0|37:93,2,0|', '6652', 'Bloodlord Firecaller\'s Robes', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:6,62,0|35:1,62,0|36:29,5,0|37:93,2,0|', '6654', 'Bloodlord Guardian\'s Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:9,62,0|35:3,62,0|36:78,5,0|37:93,2,0|', '6660', 'Bloodlord Sigmarite Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:1,62,0|35:26,484,0|36:76,5,0|37:93,2,0|', '6656', 'Bloodlord Inquisitor\'s Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:9,62,0|35:3,62,0|36:78,5,0|37:93,2,0|', '6655', 'Bloodlord High Mage\'s Robes', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:8,62,0|35:15,320,0|36:4,62,0|37:93,2,0|', '6653', 'Bloodlord Forest Courser\'s Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:6,62,0|35:1,62,0|36:29,5,0|37:93,2,0|', '6650', 'Bloodlord Defender\'s Kit', '40');
INSERT INTO `war_world`.`item_sets` (`ItemsString`, `BonusString`, `Entry`, `Name`, `Unk`) VALUES ('TEMP', '34:1,62,0|35:26,484,0|36:76,5,0|37:93,2,0|', '6657', 'Bloodlord Lionmark Kit', '40');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6662' WHERE (`Entry` = '436168');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6662' WHERE (`Entry` = '436204');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6662' WHERE (`Entry` = '436216');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6662' WHERE (`Entry` = '436192');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6662' WHERE (`Entry` = '436180');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436328:Bloodlord Skullcase|436340:Bloodlord Gauntlets|436352:Bloodlord Darkboots|436304:Bloodlord Carapace|436316:Bloodlord Shoulderguards|' WHERE (`Entry` = '6646');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436181:Bloodlord Torc|436193:Bloodlord War Shroud|436205:Bloodlord Armbraces|436217:Bloodlord Chargers|436169:Bloodlord Leathers|' WHERE (`Entry` = '6647');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436182:Bloodlord Runemantle|436206:Bloodlord Runecuffs|436218:Bloodlord Clogs|436194:Bloodlord Skullcap|436173:Bloodlord Vestments|' WHERE (`Entry` = '6659');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6646' WHERE (`Entry` = '436328');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6646' WHERE (`Entry` = '436340');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6646' WHERE (`Entry` = '436352');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6646' WHERE (`Entry` = '436304');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6646' WHERE (`Entry` = '436316');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6647' WHERE (`Entry` = '436181');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6647' WHERE (`Entry` = '436193');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6647' WHERE (`Entry` = '436205');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6647' WHERE (`Entry` = '436217');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6647' WHERE (`Entry` = '436169');
UPDATE `war_world`.`item_infos` SET `Armor` = '240', `MinRank` = '36', `Stats` = '3:32;6:14;9:19;79:13;', `SellPrice` = '3359', `MaxStack` = '1', `ObjectLevel` = '48', `ItemSet` = '6659' WHERE (`Entry` = '436173');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6659' WHERE (`Entry` = '436182');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6659' WHERE (`Entry` = '436206');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6659' WHERE (`Entry` = '436218');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6659' WHERE (`Entry` = '436194');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6664' WHERE (`Entry` = '436183');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6664' WHERE (`Entry` = '436219');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6664' WHERE (`Entry` = '436195');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6664' WHERE (`Entry` = '436207');
UPDATE `war_world`.`item_infos` SET `Armor` = '240', `MinRank` = '36', `Stats` = '5:12;7:17;8:32;32:3;', `SellPrice` = '3023', `ObjectLevel` = '48', `ItemSet` = '6664' WHERE (`Entry` = '436170');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436183:Bloodlord Sparkplate|436219:Bloodlord Steeltoes|436195:Bloodlord Hardhat|436207:Bloodlord Work Gloves|436170:Bloodlord Bulwark|' WHERE (`Entry` = '6664');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6661' WHERE (`Entry` = '436302');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6661' WHERE (`Entry` = '436314');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6661' WHERE (`Entry` = '436338');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6661' WHERE (`Entry` = '436350');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6661' WHERE (`Entry` = '436326');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436302:Bloodlord Robefings|436314:Bloodlord Sholdafings|436338:Bloodlord Greenbringas|436350:Bloodlord Morkyfeets|436326:Bloodlord Waaaghat|' WHERE (`Entry` = '6661');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6656' WHERE (`Entry` = '436172');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6656' WHERE (`Entry` = '436184');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6656' WHERE (`Entry` = '436196');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6656' WHERE (`Entry` = '436208');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6656' WHERE (`Entry` = '436220');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436172:Bloodlord Longcoat|436184:Bloodlord Bandolier|436196:Bloodlord Hat|436208:Bloodlord Coursegloves|436220:Bloodlord Shankboots|' WHERE (`Entry` = '6656');
UPDATE `war_world`.`item_infos` SET `Armor` = '721', `MinRank` = '36', `Stats` = '1:19;4:32;5:14;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', `SellPrice` = '3359', `MaxStack` = '1', `ObjectLevel` = '48', `ItemSet` = '6654' WHERE (`Entry` = '436171');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6654' WHERE (`Entry` = '436185');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6654' WHERE (`Entry` = '436209');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6654' WHERE (`Entry` = '436221');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6654' WHERE (`Entry` = '436197');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436171:Bloodlord Corslet|436185:Bloodlord Espaliers|436197:Bloodlord Casque|436209:Bloodlord Brassarts|436221:Bloodlord Sabatons' WHERE (`Entry` = '6654');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6652' WHERE (`Entry` = '436222');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6652' WHERE (`Entry` = '436210');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6652' WHERE (`Entry` = '436198');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6652' WHERE (`Entry` = '436186');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6652' WHERE (`Entry` = '436174');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436222:Bloodlord Ashboots|436210:Bloodlord Scorchguards|436198:Bloodlord Grille|436186:Bloodlord Illuminations|436174:Bloodlord Flamerobe|' WHERE (`Entry` = '6652');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6660' WHERE (`Entry` = '436199');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6660' WHERE (`Entry` = '436223');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6660' WHERE (`Entry` = '436211');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6660' WHERE (`Entry` = '436187');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6660' WHERE (`Entry` = '436175');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436199:Bloodlord Helm|436223:Bloodlord Spatterguards|436211:Bloodlord Fists|436187:Bloodlord Mantle|436175:Bloodlord Cassock|' WHERE (`Entry` = '6660');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6663' WHERE (`Entry` = '436305');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6663' WHERE (`Entry` = '436317');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6663' WHERE (`Entry` = '436329');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6663' WHERE (`Entry` = '436341');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6663' WHERE (`Entry` = '436353');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436305:Bloodlord Chestplate|436317:Bloodlord Hardmantle|436329:Bloodlord Direhelm|436341:Bloodlord Pummelers|436353:Bloodlord Tramplers|' WHERE (`Entry` = '6663');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6649' WHERE (`Entry` = '436307');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6649' WHERE (`Entry` = '436319');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6649' WHERE (`Entry` = '436331');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6649' WHERE (`Entry` = '436343');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6649' WHERE (`Entry` = '436355');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436307:Bloodlord Nightrobe|436319:Bloodlord Warpmantle|436331:Bloodlord Barbute|436343:Bloodlord Bracers|436355:Bloodlord Daemonspurs|' WHERE (`Entry` = '6649');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6650' WHERE (`Entry` = '436212');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6650' WHERE (`Entry` = '436224');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6650' WHERE (`Entry` = '436200');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6650' WHERE (`Entry` = '436188');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6650' WHERE (`Entry` = '436176');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436212:Bloodlord Vambraces|436224:Bloodlord Solerets|436200:Bloodlord Taen|436188:Bloodlord Balancers|436176:Bloodlord Platecoat|' WHERE (`Entry` = '6650');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6653' WHERE (`Entry` = '436177');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6653' WHERE (`Entry` = '436189');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6653' WHERE (`Entry` = '436213');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6653' WHERE (`Entry` = '436225');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6653' WHERE (`Entry` = '436201');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436177:Bloodlord Tunic|436189:Bloodlord Shadowmantle|436213:Bloodlord Armguards|436225:Bloodlord Softsoles|436201:Bloodlord Maskhelm|' WHERE (`Entry` = '6653');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6657' WHERE (`Entry` = '436178');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6657' WHERE (`Entry` = '436190');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6657' WHERE (`Entry` = '436202');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6657' WHERE (`Entry` = '436214');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6657' WHERE (`Entry` = '436226');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436178:Bloodlord Mailcoat|436190:Bloodlord Mane|436202:Bloodlord Steelhelm|436214:Bloodlord Wristguards|436226:Bloodlord Shinsteels|' WHERE (`Entry` = '6657');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6655' WHERE (`Entry` = '436179');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6655' WHERE (`Entry` = '436191');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6655' WHERE (`Entry` = '436203');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6655' WHERE (`Entry` = '436215');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6655' WHERE (`Entry` = '436227');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436179:Bloodlord Hoeth Robe|436191:Bloodlord Highmantle|436203:Bloodlord Circlet|436215:Bloodlord Braces|436227:Bloodlord Slippers|' WHERE (`Entry` = '6655');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6651' WHERE (`Entry` = '436308');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6651' WHERE (`Entry` = '436320');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6651' WHERE (`Entry` = '436332');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6651' WHERE (`Entry` = '436344');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6651' WHERE (`Entry` = '436356');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436308:Bloodlord Hauberk|436320:Bloodlord Pauldrons|436332:Bloodlord Dreadhelm|436344:Bloodlord Grimbraces|436356:Bloodlord Greaves|' WHERE (`Entry` = '6651');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6658' WHERE (`Entry` = '436309');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6658' WHERE (`Entry` = '436321');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6658' WHERE (`Entry` = '436333');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6658' WHERE (`Entry` = '436357');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6658' WHERE (`Entry` = '436345');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436309:Bloodlord Corset|436321:Bloodlord Throatlatch|436333:Bloodlord Hexveil|436357:Bloodlord Thighbinds|436345:Bloodlord Embracers|' WHERE (`Entry` = '6658');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6648' WHERE (`Entry` = '436322');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6648' WHERE (`Entry` = '436334');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6648' WHERE (`Entry` = '436346');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6648' WHERE (`Entry` = '436358');
UPDATE `war_world`.`item_infos` SET `ItemSet` = '6648' WHERE (`Entry` = '436310');
UPDATE `war_world`.`item_sets` SET `ItemsString` = '436322:Bloodlord Painmantle|436334:Bloodlord Gorget|436346:Bloodlord Armscales|436358:Bloodlord Bloodwades|436310:Bloodlord Breastscale|' WHERE (`Entry` = '6648');
ALTER TABLE `war_world`.`loot_groups` 
CHANGE COLUMN `ReqGroupUsable` `ReqGroupUsable` TINYINT(3) NOT NULL DEFAULT 0 ;

INSERT INTO `war_world`.`loot_groups` (`Entry`, `Name`, `DropEvent`, `CreatureID`, `CreatureSubType`) VALUES ('561', 'a', '4', '1', '1');
INSERT INTO `war_world`.`loot_groups` (`Entry`, `Name`, `DropEvent`, `CreatureID`, `CreatureSubType`) VALUES ('562', 'a', '4', '1', '1');
INSERT INTO `war_world`.`loot_groups` (`Entry`, `Name`, `DropEvent`, `CreatureID`, `CreatureSubType`) VALUES ('563', 'a', '4', '1', '1');
INSERT INTO `war_world`.`loot_groups` (`Entry`, `Name`, `DropEvent`, `CreatureID`, `CreatureSubType`) VALUES ('564', 'a', '4', '1', '1');
UPDATE `war_world`.`loot_groups` SET `Name` = 'InstanceBoss: Horgulul', `CreatureSubType` = '9', `SpecificZone` = '163' WHERE (`Entry` = '561');
UPDATE `war_world`.`loot_groups` SET `Name` = 'InstanceBoss: Horgulul', `CreatureSubType` = '9', `SpecificZone` = '164' WHERE (`Entry` = '562');
UPDATE `war_world`.`loot_groups` SET `Name` = 'InstanceBoss: Horgulul', `CreatureSubType` = '9', `SpecificZone` = '165' WHERE (`Entry` = '563');
UPDATE `war_world`.`loot_groups` SET `Name` = 'InstanceBoss: Horgulul', `CreatureSubType` = '9', `SpecificZone` = '166' WHERE (`Entry` = '564');
UPDATE `war_world`.`loot_groups` SET `Name` = 'InstanceBoss: Thar\'lgnan', `CreatureID` = '45084' WHERE (`Entry` = '561');
UPDATE `war_world`.`loot_groups` SET `Name` = 'InstanceBoss: Lord Slaurith', `CreatureID` = '48112' WHERE (`Entry` = '562');
UPDATE `war_world`.`loot_groups` SET `Name` = 'InstanceBoss: Kaarn the Vanquisher', `CreatureID` = '2000751' WHERE (`Entry` = '563');
UPDATE `war_world`.`loot_groups` SET `Name` = 'InstanceBoss: Skull Lord Var\'Ithrok', `CreatureID` = '64106' WHERE (`Entry` = '564');
UPDATE `war_world`.`loot_groups` SET `DropChance` = '0.4' WHERE (`Entry` = '561');
UPDATE `war_world`.`loot_groups` SET `DropChance` = '0.4' WHERE (`Entry` = '562');
UPDATE `war_world`.`loot_groups` SET `DropChance` = '0.4' WHERE (`Entry` = '563');
UPDATE `war_world`.`loot_groups` SET `DropChance` = '0.4', `DropCount` = '2' WHERE (`Entry` = '564');
INSERT INTO `war_world`.`loot_groups` (`Entry`, `Name`, `DropEvent`, `CreatureID`, `CreatureSubType`, `DropChance`, `DropCount`, `ReqGroupUsable`, `ReqActiveQuest`, `SpecificZone`) VALUES ('565', 'InstanceBoss: Thar\'lgnan', '4', '45084', '9', '0.1', '1', '0', '0', '163');
INSERT INTO `war_world`.`loot_groups` (`Entry`, `Name`, `DropEvent`, `CreatureID`, `CreatureSubType`, `DropChance`, `DropCount`, `ReqGroupUsable`, `ReqActiveQuest`, `SpecificZone`) VALUES ('566', 'InstanceBoss: Lord Slaurith', '4', '48112', '9', '0.1', '1', '0', '0', '164');
INSERT INTO `war_world`.`loot_groups` (`Entry`, `Name`, `DropEvent`, `CreatureID`, `CreatureSubType`, `DropChance`, `DropCount`, `ReqGroupUsable`, `ReqActiveQuest`, `SpecificZone`) VALUES ('567', 'InstanceBoss: Kaarn the Vanquisher', '4', '2000751', '9', '0.1', '1', '0', '0', '165');
INSERT INTO `war_world`.`loot_groups` (`Entry`, `Name`, `DropEvent`, `CreatureID`, `CreatureSubType`, `DropChance`, `DropCount`, `ReqGroupUsable`, `ReqActiveQuest`, `SpecificZone`) VALUES ('568', 'InstanceBoss: Skull Lord Var\'Ithrok', '4', '64106', '9', '0.1', '1', '0', '0', '166');
UPDATE `war_world`.`loot_groups` SET `ReqGroupUsable` = '1' WHERE (`Entry` = '561');
UPDATE `war_world`.`loot_groups` SET `ReqGroupUsable` = '1' WHERE (`Entry` = '562');
UPDATE `war_world`.`loot_groups` SET `ReqGroupUsable` = '1' WHERE (`Entry` = '563');
UPDATE `war_world`.`loot_groups` SET `ReqGroupUsable` = '1' WHERE (`Entry` = '564');
UPDATE `war_world`.`loot_groups` SET `ReqGroupUsable` = '1' WHERE (`Entry` = '565');
UPDATE `war_world`.`loot_groups` SET `ReqGroupUsable` = '1' WHERE (`Entry` = '566');
UPDATE `war_world`.`loot_groups` SET `ReqGroupUsable` = '1' WHERE (`Entry` = '567');
UPDATE `war_world`.`loot_groups` SET `ReqGroupUsable` = '1' WHERE (`Entry` = '568');
UPDATE `war_world`.`item_infos` SET `MinRank` = 36 where career != 0 and type != 35 and type != 1 and type != 3 and `name` like "%bloodlord%";
UPDATE `war_world`.`item_infos` SET `SellPrice` = '3359' WHERE (`Entry` = '436311');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2960' WHERE (`Entry` = '436323');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '3023' WHERE (`Entry` = '436335');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2631' WHERE (`Entry` = '436347');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2631' WHERE (`Entry` = '436359');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2960' WHERE (`Entry` = '436313');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2960' WHERE (`Entry` = '436325');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2631' WHERE (`Entry` = '436337');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2631' WHERE (`Entry` = '436349');
UPDATE `war_world`.`item_infos` SET `Armor` = '480', `Stats` = '1:32;4:19;5:14;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', `SellPrice` = '3359' WHERE (`Entry` = '436301');
UPDATE `war_world`.`item_infos` SET `Armor` = '480', `Stats` = '1:32;4:19;5:14;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', `SellPrice` = '3359', `MaxStack` = '1', `ObjectLevel` = '36' WHERE (`Entry` = '436169');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2287' WHERE (`Entry` = '436300');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2960' WHERE (`Entry` = '436312');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '3023' WHERE (`Entry` = '436324');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2631' WHERE (`Entry` = '436336');
UPDATE `war_world`.`item_infos` SET `SellPrice` = '2631' WHERE (`Entry` = '436348');
UPDATE `war_world`.`item_infos` SET `MaxStack` = '1' WHERE (`Entry` = '436170');
UPDATE `war_world`.`item_infos` SET `Armor` = '721', `ObjectLevel` = '48' WHERE (`Entry` = '436176');
UPDATE `war_world`.`item_infos` SET `MaxStack` = '1', `ObjectLevel` = '48' WHERE (`Entry` = '436301');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436177');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436302');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436178');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '48' WHERE (`Entry` = '436169');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436172', '30', '40', '0', '100', '1660000');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436177', '30', '40', '0', '100', '1660001');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436178', '30', '40', '0', '100', '1660002');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436179', '30', '40', '0', '100', '1660003');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436300', '30', '40', '0', '100', '1660004');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436302', '30', '40', '0', '100', '1660005');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436303', '30', '40', '0', '100', '1660006');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436305', '30', '40', '0', '100', '1660007');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436306', '30', '40', '0', '100', '1660008');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436307', '30', '40', '0', '100', '1660009');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436308', '30', '40', '0', '100', '1660010');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436309', '30', '40', '0', '100', '1660011');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436311', '30', '40', '0', '100', '1660012');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436171', '30', '40', '0', '100', '1660013');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436301', '30', '40', '0', '100', '1660014');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436176', '30', '40', '0', '100', '1660015');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436175', '30', '40', '0', '100', '1660016');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436174', '30', '40', '0', '100', '1660017');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436168', '30', '40', '0', '100', '1660018');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436304', '30', '40', '0', '100', '1660019');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436310', '30', '40', '0', '100', '1660020');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436169', '30', '40', '0', '100', '1660021');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436170', '30', '40', '0', '100', '1660022');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('564', '436173', '30', '40', '0', '100', '1660023');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436181', '30', '40', '0', '100', '1640001');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436182', '30', '40', '0', '100', '1640002');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436183', '30', '40', '0', '100', '1640003');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436184', '30', '40', '0', '100', '1640004');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436185', '30', '40', '0', '100', '1640005');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436189', '30', '40', '0', '100', '1640006');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436190', '30', '40', '0', '100', '1640007');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436191', '30', '40', '0', '100', '1640008');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436312', '30', '40', '0', '100', '1640009');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436313', '30', '40', '0', '100', '1640010');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436314', '30', '40', '0', '100', '1640011');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436315', '30', '40', '0', '100', '1640012');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436317', '30', '40', '0', '100', '1640013');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436318', '30', '40', '0', '100', '1640014');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436319', '30', '40', '0', '100', '1640015');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436320', '30', '40', '0', '100', '1640016');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436321', '30', '40', '0', '100', '1640017');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436322', '30', '40', '0', '100', '1640018');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436323', '30', '40', '0', '100', '1640019');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436188', '30', '40', '0', '100', '1640020');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436187', '30', '40', '0', '100', '1640021');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436186', '30', '40', '0', '100', '1640022');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436180', '30', '40', '0', '100', '1640023');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('562', '436316', '30', '40', '0', '100', '1640024');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436216');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436217');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436218');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436219');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436220');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436221');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436224');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436225');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436226');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436227');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436348');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436349');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436350');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436351');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436352');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436353');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436355');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436356');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436358');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436359');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436223');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436222');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '47' WHERE (`Entry` = '436357');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436216', '30', '40', '0', '100', '1630001');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436217', '30', '40', '0', '100', '1630002');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436218', '30', '40', '0', '100', '1630003');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436219', '30', '40', '0', '100', '1630004');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436220', '30', '40', '0', '100', '1630005');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436221', '30', '40', '0', '100', '1630006');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436222', '30', '40', '0', '100', '1630007');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436223', '30', '40', '0', '100', '1630008');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436224', '30', '40', '0', '100', '1630009');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436225', '30', '40', '0', '100', '1630010');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436226', '30', '40', '0', '100', '1630011');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436227', '30', '40', '0', '100', '1630012');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436348', '30', '40', '0', '100', '1630013');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436349', '30', '40', '0', '100', '1630014');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436350', '30', '40', '0', '100', '1630015');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436351', '30', '40', '0', '100', '1630016');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436352', '30', '40', '0', '100', '1630017');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436353', '30', '40', '0', '100', '1630018');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436354', '30', '40', '0', '100', '1630019');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436355', '30', '40', '0', '100', '1630020');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436356', '30', '40', '0', '100', '1630021');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436357', '30', '40', '0', '100', '1630022');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436358', '30', '40', '0', '100', '1630023');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('561', '436349', '30', '40', '0', '100', '1630024');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436192', '30', '40', '0', '100', '1650001');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436193', '30', '40', '0', '100', '1650002');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436194', '30', '40', '0', '100', '1650003');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436195', '30', '40', '0', '100', '1650004');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436196', '30', '40', '0', '100', '1650005');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436197', '30', '40', '0', '100', '1650006');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436198', '30', '40', '0', '100', '1650007');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436199', '30', '40', '0', '100', '1650008');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436200', '30', '40', '0', '100', '1650009');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436201', '30', '40', '0', '100', '1650010');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436202', '30', '40', '0', '100', '1650011');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436203', '30', '40', '0', '100', '1650012');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436324', '30', '40', '0', '100', '1650013');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436325', '30', '40', '0', '100', '1650014');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436326', '30', '40', '0', '100', '1650015');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436327', '30', '40', '0', '100', '1650016');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436328', '30', '40', '0', '100', '1650017');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436329', '30', '40', '0', '100', '1650018');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436330', '30', '40', '0', '100', '1650019');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436331', '30', '40', '0', '100', '1650020');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436332', '30', '40', '0', '100', '1650021');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436333', '30', '40', '0', '100', '1650022');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436334', '30', '40', '0', '100', '1650023');
INSERT INTO `war_world`.`loot_group_items` (`LootGroupID`, `ItemID`, `MinRank`, `MaxRank`, `MinRenown`, `MaxRenown`, `Loot_Group_Items_ID`) VALUES ('563', '436335', '30', '40', '0', '100', '1650024');
UPDATE `war_world`.`loot_groups` SET `DropChance` = '1' WHERE (`Entry` = '564');
UPDATE `war_world`.`loot_groups` SET `DropChance` = '1' WHERE (`Entry` = '563');
UPDATE `war_world`.`loot_groups` SET `DropChance` = '1' WHERE (`Entry` = '562');
UPDATE `war_world`.`loot_groups` SET `DropChance` = '1' WHERE (`Entry` = '561');


UPDATE `war_world`.`zone_jumps` SET `WorldO` = '4088' WHERE (`Entry` = '167782248');
UPDATE `war_world`.`creature_protos` SET `MinScale` = '100', `MaxScale` = '100', `Unk1` = '0' WHERE (`Entry` = '48112');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2048', `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '171972648');
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1040841', `WorldY` = '993038', `WorldO` = '3492', `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167772329');
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1017041', `WorldY` = '1026708', `WorldO` = '1035', `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167775848');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '4', `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167819500');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167772264');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167772266');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167772267');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167772328');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167775464');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167776232');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '167776360');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '170923752');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '174063721');
UPDATE `war_world`.`zone_jumps` SET `Type` = '4', `InstanceID` = '160' WHERE (`Entry` = '174063726');
UPDATE `war_world`.`creature_protos` SET `MinScale` = '100', `MaxScale` = '100' WHERE (`Entry` = '2000751');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2224' WHERE (`Entry` = '167772268');
UPDATE `war_world`.`zone_jumps` SET `WorldX` = '1011533', `WorldY` = '1014177', `WorldO` = '3276' WHERE (`Entry` = '174063721');
UPDATE `war_world`.`loot_groups` SET `DropCount` = '1' WHERE (`Entry` = '564');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '160A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '160B');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '160C');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '160D');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '160E');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '179A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '195A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '196A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '260A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '60A');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '60B');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '60C');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '60D');
UPDATE `war_world`.`instance_infos` SET `LockoutTimer` = '1440' WHERE (`instance_infos_ID` = '60E');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `NpcRank`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('2500945', 'Accumulator', '1218', '0', '50', '50', '40', '40', '65', '5', '0', '0', '0', '0', '0', '1002', '0', '0', '0', '0', '0', '16', '91', '0', '0', '423', '0', '1', '1', '0', '0', '0');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('1215691', '2500945', '103', '1472702', '814482', '16094', '1244', '0', '0', '0', '0', '40', '0', '0', '1');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('2500945', '10', '4964', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('2500945', '20', '5974', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('2500945', '21', '6955', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('2500945', '22', '6954', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('2500945', '23', '4480', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('2500945', '24', '5975', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('2500945', '27', '1952', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('2500945', '28', '6953', '0', '0', '0');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '0', `SecondaryColor` = '0' WHERE (`Entry` = '166666') and (`SlotId` = '20');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '0', `SecondaryColor` = '0' WHERE (`Entry` = '166666') and (`SlotId` = '24');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '0', `SecondaryColor` = '0' WHERE (`Entry` = '166666') and (`SlotId` = '23');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '0', `SecondaryColor` = '0' WHERE (`Entry` = '166666') and (`SlotId` = '21');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '0', `SecondaryColor` = '0' WHERE (`Entry` = '166666') and (`SlotId` = '22');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '0', `SecondaryColor` = '0' WHERE (`Entry` = '166666') and (`SlotId` = '28');
UPDATE `war_world`.`creature_items` SET `PrimaryColor` = '0', `SecondaryColor` = '0' WHERE (`Entry` = '166666') and (`SlotId` = '27');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('166666', '10', '1808', '0', '0', '0');


UPDATE `war_world`.`zone_jumps` SET `WorldO` = '3062' WHERE (`Entry` = '167776232');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '2056' WHERE (`Entry` = '167772328');
UPDATE `war_world`.`zone_jumps` SET `WorldO` = '4090' WHERE (`Entry` = '170923752');
UPDATE `war_world`.`creature_protos` SET `Title` = '10' WHERE (`Entry` = '2500945');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `CastAngle`, `AbilityType`, `Specline`, `WeaponNeeded`, `AffectsDead`, `IgnoreGlobalCooldown`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('203', '0', 'Bloodlord Weapon', '203', '0', '0', 'Item', '0', '0', '0', '0', '0', '203', '0', '0', '0', '0');
UPDATE `war_world`.`abilities` SET `Name` = 'Bloodlust Weapon' WHERE (`Entry` = '203');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('203', 'Bloodlust Weapon', '0', '0', 'InvokeBuff', '203', 'Caster');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('203', 'Bloodlust Weapon', 'Morale', '1', '1', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `Target`, `BuffLine`) VALUES ('203', 'Bloodlust Weapon', '0', '0', 'ModifyPercentageStat', '23', '10', 'Host', '1');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('100', 'Bloodlust Cutta', 'With great power comes great responsibility; or in this case a blood sacrifice.', '1', '6', '8293', '13', '4', '0', '1', '1', '0', '0', '650', '240', '40', '0', '0', '1:100:5:100;', '0', '0', '1', '60', '0', '0', '0', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('101', 'Bloodlust Chaosaxe', 'With great power comes great responsibility; or in this case a blood sacrifice.', '2', '64', '8357', '13', '4', '0', '2', '1', '0', '0', '650', '240', '40', '0', '0', '1:100:5:100;', '0', '0', '1', '60', '0', '0', '0', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('102', 'Bloodlust Painblade', 'With great power comes great responsibility; or in this case a blood sacrifice.', '1', '16', '8766', '13', '4', '0', '1', '1', '0', '0', '650', '240', '40', '0', '0', '1:100:5:100;', '0', '0', '1', '60', '0', '0', '0', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('103', 'Bloodlust Stiletto', 'With great power comes great responsibility; or in this case a blood sacrifice.', '12', '16', '8286', '13', '4', '0', '2048', '1', '0', '0', '650', '240', '40', '0', '0', '1:100:5:100;', '0', '0', '1', '60', '0', '0', '0', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('104', 'Bloodlust Xiphos', 'With great power comes great responsibility; or in this case a blood sacrifice.', '1', '16', '8766', '13', '4', '0', '1', '1', '0', '0', '650', '240', '40', '0', '0', '1:100:5:100;', '0', '0', '1', '60', '0', '0', '0', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('105', 'Bloodlust Orcsplitter', 'With great power comes great responsibility; or in this case a blood sacrifice.', '2', '1', '9129', '13', '4', '0', '2', '1', '0', '0', '650', '240', '40', '0', '0', '1:100:5:100;', '0', '0', '1', '60', '0', '0', '0', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('106', 'Bloodlust Battlehammer', 'With great power comes great responsibility; or in this case a blood sacrifice.', '3', '32', '8772', '13', '4', '0', '4', '1', '0', '0', '650', '240', '40', '0', '0', '1:100:5:100;', '0', '0', '1', '60', '0', '0', '0', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('107', 'Bloodlust Blade', 'With great power comes great responsibility; or in this case a blood sacrifice.', '1', '8', '9010', '13', '4', '0', '1', '1', '0', '0', '650', '240', '40', '0', '0', '1:100:5:100;', '0', '0', '1', '60', '0', '0', '0', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '100', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '101', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '102', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '103', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '104', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '105', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '106', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '107', '0', '0', '0', '(1,1)');


UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '-25' WHERE (`Entry` = '203') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.', `Stats` = '1:100;5:100;' WHERE (`Entry` = '100');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.', `Stats` = '1:100;5:100;' WHERE (`Entry` = '101');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.', `Stats` = '1:100;5:100;' WHERE (`Entry` = '102');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.', `Stats` = '1:100;5:100;' WHERE (`Entry` = '103');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.', `Stats` = '1:100;5:100;' WHERE (`Entry` = '104');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.', `Stats` = '1:100;5:100;' WHERE (`Entry` = '105');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.', `Stats` = '1:100;5:100;' WHERE (`Entry` = '106');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.', `Stats` = '1:100;5:100;' WHERE (`Entry` = '107');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `Target`, `BuffLine`) VALUES ('202', 'Bloodlust 2H Weapon', '0', '0', 'ModifyPercentageStat', '23', '-50', 'Host', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('202', 'Bloodlust 2H Weapon', 'Morale', '1', '1', '1');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('108', 'Bloodlust Sword', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '1', '64', '8936', '10', '4', '0', '1', '1', '0', '0', '650', '240', '40', '0', '0', '1:100;5:100;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('109', 'Bloodlust Bigloppa', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '2', '6', '8425', '10', '4', '0', '2097154', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('110', 'Bloodlust Deathaxe', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '2', '64', '8359', '10', '4', '0', '2097154', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('111', 'Bloodlust Glaive', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '14', '16', '8400', '10', '4', '0', '2105344', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('112', 'Bloodlust Rapier', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '1', '32', '8782', '10', '4', '256', '1', '1', '0', '0', '650', '240', '40', '0', '0', '1:100;5:100;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('113', 'Bloodlust Sword', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '1', '32', '8818', '10', '4', '0', '1', '1', '0', '0', '650', '240', '40', '0', '0', '1:100;5:100;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('114', 'Bloodlust Warhammer', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '3', '0', '9150', '10', '4', '1', '2097156', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('115', 'Bloodlust Giantsplitter', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '2', '0', '9139', '10', '4', '2', '2097154', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('116', 'Bloodlust Claymore', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '1', '32', '8822', '10', '4', '0', '2097153', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('117', 'Bloodlust Masterblade', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '1', '8', '9026', '10', '4', '0', '2097153', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('118', 'Bloodlust Greataxe', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '2', '8', '6323', '10', '4', '0', '2097154', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('119', 'Bloodlust Sledge', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '3', '32', '8775', '10', '4', '2048', '2097156', '1', '0', '0', '943', '340', '40', '0', '0', '1:160;5:160;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '108', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '109', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '110', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '111', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '112', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '113', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '114', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '115', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '116', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '117', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '118', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '119', '0', '0', '0', '(1,1)');


INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('120', 'Bloodlust Chalice', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '25', '16', '8729', '11', '4', '0', '8388608', '1', '0', '0', '0', '0', '40', '0', '0', '1:100;5:100;', '0', '0', '0', '60', '0', '0', '0', '1', '0', '0', '0', '203;14205', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('121', 'Bloodlust Pistol', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '15', '32', '8779', '11', '4', '0', '1064960', '1', '0', '0', '650', '240', '40', '0', '0', '1:100;5:100;', '0', '0', '0', '60', '0', '0', '0', '1', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('122', 'Bloodlust Tome', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '25', '32', '8806', '11', '4', '0', '8388608', '1', '0', '0', '0', '0', '40', '0', '0', '1:100;5:100;', '0', '0', '0', '60', '0', '0', '0', '1', '0', '0', '0', '203;14193', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '120', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '121', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '122', '0', '0', '0', '(1,1)');
UPDATE `war_world`.`buff_commands` SET `InvokeOn` = '5' WHERE (`Entry` = '203') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `InvokeOn` = '5' WHERE (`Entry` = '202') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('123', 'Bloodlust Shoota', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '7', '4', '8437', '12', '4', '0', '64', '1', '0', '0', '650', '240', '40', '0', '0', '8:100;5:100;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('124', 'Bloodlust Rifle', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '9', '1', '9189', '12', '4', '0', '256', '1', '0', '0', '650', '240', '40', '0', '0', '8:100;5:100;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('125', 'Bloodlust Longbow', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '7', '8', '9039', '12', '4', '0', '64', '1', '0', '0', '650', '240', '40', '0', '0', '8:100;5:100;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '123', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '124', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '125', '0', '0', '0', '(1,1)');


UPDATE `war_world`.`item_infos` SET `Description` = 'A skull pulled from Skull Lord Var\'Ithrok\'s pile.' WHERE (`Entry` = '1');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '50' WHERE (`Entry` = '202') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '25' WHERE (`Entry` = '203') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`creature_abilities` SET `ActivateAtHealthPercent` = '100' WHERE (`creature_abilities_ID` = '1952000');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1086216');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1086217');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1086218');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `Emote`, `ZoneID`, `ConnectedBossID`) VALUES ('2000758', '1576656', '1046124', '11060', '1012', '19530001', '0', '42', '0', '195', '197');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `Emote`, `ZoneID`, `ConnectedBossID`) VALUES ('2000759', '1576656', '1046316', '11054', '1012', '19530002', '0', '42', '0', '195', '197');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1086176');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1086240');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '1086252');
INSERT INTO `war_world`.`instance_boss_spawns` (`Instance_spawns_ID`, `Entry`, `Realm`, `Level`, `Emote`, `ZoneID`, `InstanceID`, `BossID`, `SpawnGroupID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`) VALUES ('1951000', '2000763', '0', '42', '0', '195', '195', '195', '1', '1557367', '1048053', '11500', '2798');
INSERT INTO `war_world`.`instance_boss_spawns` (`Instance_spawns_ID`, `Entry`, `Realm`, `Level`, `Emote`, `ZoneID`, `InstanceID`, `BossID`, `SpawnGroupID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`) VALUES ('1952000', '46995', '0', '42', '0', '195', '195', '196', '1', '1589266', '1064332', '7274', '2025');
INSERT INTO `war_world`.`instance_boss_spawns` (`Instance_spawns_ID`, `Entry`, `Realm`, `Level`, `Emote`, `ZoneID`, `InstanceID`, `BossID`, `SpawnGroupID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`) VALUES ('1953000', '2000757', '0', '43', '0', '195', '195', '197', '1', '1576583', '1046218', '11044', '1012');
INSERT INTO `war_world`.`instance_boss_spawns` (`Instance_spawns_ID`, `Entry`, `Realm`, `Level`, `Emote`, `ZoneID`, `InstanceID`, `BossID`, `SpawnGroupID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`) VALUES ('1954000', '46205', '0', '44', '0', '195', '195', '198', '1', '1570000', '1050447', '11232', '2013');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564211');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `Emote`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('4284', '1409856', '1588332', '5864', '944', '2600000', '1', '40', '0', '260', '0', '0');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `Emote`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('4284', '1409856', '1588332', '5864', '944', '2600001', '2', '40', '0', '260', '0', '0');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1000' WHERE (`Instance_spawns_ID` = '2601008');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1000' WHERE (`Instance_spawns_ID` = '2601005');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1001' WHERE (`Instance_spawns_ID` = '2601006');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1001' WHERE (`Instance_spawns_ID` = '2601000');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1001' WHERE (`Instance_spawns_ID` = '2601009');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1002' WHERE (`Instance_spawns_ID` = '2601007');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1002' WHERE (`Instance_spawns_ID` = '2601001');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1002' WHERE (`Instance_spawns_ID` = '2601002');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1003' WHERE (`Instance_spawns_ID` = '2601003');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1003' WHERE (`Instance_spawns_ID` = '2601004');
UPDATE `war_world`.`instance_creature_spawns` SET `SpawnGroupID` = '1003' WHERE (`Instance_spawns_ID` = '2601010');
INSERT INTO `war_world`.`instance_creature_spawns` (`Entry`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Instance_spawns_ID`, `Realm`, `Level`, `Emote`, `ZoneID`, `ConnectedBossID`, `SpawnGroupID`) VALUES ('4282', '1410129', '1582565', '6104', '4061', '2600002', '0', '40', '0', '260', '0', '0');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '564357');
UPDATE `war_world`.`instance_objects` SET `instance_objects_ID` = '1' WHERE (`instance_objects_ID` = '2584901');
UPDATE `war_world`.`instance_objects` SET `instance_objects_ID` = '2' WHERE (`instance_objects_ID` = '2584921');
UPDATE `war_world`.`instance_objects` SET `Realm` = '0' WHERE (`instance_objects_ID` = '1');
UPDATE `war_world`.`instance_objects` SET `Realm` = '0' WHERE (`instance_objects_ID` = '2');


UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '2000' WHERE (`Instance_spawns_ID` = '600000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '2001' WHERE (`Instance_spawns_ID` = '600001');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '2002' WHERE (`Instance_spawns_ID` = '600002');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '2003' WHERE (`Instance_spawns_ID` = '600003');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '1999' WHERE (`Instance_spawns_ID` = '2600004');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '1998' WHERE (`Instance_spawns_ID` = '2600003');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '1997' WHERE (`Instance_spawns_ID` = '2600002');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '1996' WHERE (`Instance_spawns_ID` = '2600001');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '1995' WHERE (`Instance_spawns_ID` = '2600000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '3000' WHERE (`Instance_spawns_ID` = '1951000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '3001' WHERE (`Instance_spawns_ID` = '1952000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '3002' WHERE (`Instance_spawns_ID` = '1953000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '3003' WHERE (`Instance_spawns_ID` = '1954000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '4003' WHERE (`Instance_spawns_ID` = '1604000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '4000' WHERE (`Instance_spawns_ID` = '1601000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '4001' WHERE (`Instance_spawns_ID` = '1602000');
UPDATE `war_world`.`instance_boss_spawns` SET `SpawnGroupID` = '4002' WHERE (`Instance_spawns_ID` = '1603000');
INSERT INTO `war_world`.`instance_objects` (`instance_objects_ID`, `Entry`, `InstanceID`, `BossID`, `GameObjectSpawnID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `VfxState`, `Realm`) VALUES ('3', '100272', '260', '334', '258470', '1404210', '1573508', '7952', '3458', '1789', '0', '0');
UPDATE `war_world`.`instance_objects` SET `WorldX` = '1410071', `WorldY` = '1581747', `WorldZ` = '6136', `WorldO` = '92' WHERE (`instance_objects_ID` = '1');


INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('126', 'Bloodlust Blasta', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '11', '4', '1115', '10', '4', '0', '1024', '1', '0', '0', '943', '340', '40', '0', '0', '9:160;5:160;67:20;32:10;78:6;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('127', 'Bloodlust Focus', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '25', '64', '8366', '11', '4', '0', '8388608', '1', '0', '0', '0', '0', '40', '0', '0', '9:100;5:100;67:10;32:5;78:3;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('128', 'Bloodlust Greatstaff', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '11', '32', '8816', '10', '4', '0', '1024', '1', '0', '0', '943', '340', '40', '0', '0', '9:160;5:160;67:20;32:10;78:6;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '203', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('129', 'Bloodlust Ninestaff', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '11', '64', '8846', '10', '4', '0', '1024', '1', '0', '0', '943', '340', '40', '0', '0', '9:160;5:160;67:20;32:10;78:6;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('130', 'Bloodlust Skinflayer', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '12', '64', '8361', '10', '4', '0', '2048', '1', '0', '0', '650', '240', '40', '0', '0', '9:100;5:100;67:10;32:5;78:3;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('131', 'Bloodlust Witchstaff', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '11', '16', '8408', '10', '4', '0', '1024', '1', '0', '0', '943', '340', '40', '0', '0', '9:160;5:160;67:20;32:10;78:6;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '203', '0', '0');
UPDATE `war_world`.`item_infos` SET `Salvageable` = '1' WHERE (`Entry` = '5501424');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '126', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '127', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '128', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '129', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '130', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '131', '0', '0', '0', '(1,1)');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;', `Salvageable` = '1' WHERE (`Entry` = '100');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;', `Salvageable` = '1' WHERE (`Entry` = '101');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;', `Salvageable` = '1' WHERE (`Entry` = '102');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;', `Salvageable` = '1' WHERE (`Entry` = '103');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;', `Salvageable` = '1' WHERE (`Entry` = '104');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;', `Salvageable` = '1' WHERE (`Entry` = '105');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;', `Salvageable` = '1' WHERE (`Entry` = '106');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;', `Salvageable` = '1' WHERE (`Entry` = '107');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;' WHERE (`Entry` = '108');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;' WHERE (`Entry` = '112');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;' WHERE (`Entry` = '113');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;' WHERE (`Entry` = '120');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;' WHERE (`Entry` = '121');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:100;5:100;67:10;32:5;76:3;' WHERE (`Entry` = '122');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:100;5:100;67:10;32:5;77:3;' WHERE (`Entry` = '123');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:100;5:100;67:10;32:5;77:3;' WHERE (`Entry` = '124');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:100;5:100;67:10;32:5;77:3;' WHERE (`Entry` = '125');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '114');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '115');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '116');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '117');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '118');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '119');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '109');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '110');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:160;5:160;67:30;32:10;76:6;' WHERE (`Entry` = '111');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:160;5:160;67:30;32:10;78:6;' WHERE (`Entry` = '128');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:160;5:160;67:30;32:10;78:6;' WHERE (`Entry` = '129');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:160;5:160;67:30;32:10;78:6;' WHERE (`Entry` = '131');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:160;5:160;67:30;32:10;78:6;' WHERE (`Entry` = '126');


INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('132', 'Bloodlust Stabba', '14', '4', '8795', '10', '4', '0', '8192', '1', '0', '0', '650', '240', '40', '0', '0', '8:100;5:100;67:10;32:5;77:3;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '203', '0', '0');
UPDATE `war_world`.`item_infos` SET `Salvageable` = '1' WHERE (`Entry` = '5501375');
UPDATE `war_world`.`item_infos` SET `Salvageable` = '1' WHERE (`Entry` = '5501373');
UPDATE `war_world`.`item_infos` SET `Salvageable` = '1' WHERE (`Entry` = '5501383');
UPDATE `war_world`.`item_infos` SET `Description` = 'With great power comes great responsibility; or in this case, a blood sacrifice.' WHERE (`Entry` = '132');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('133', 'Bloodlust Crusher', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '3', '64', '8374', '13', '4', '0', '4', '1', '0', '0', '650', '240', '40', '0', '0', '1:100;5:100;67:10;32:5;76:3;', '0', '0', '1', '60', '0', '0', '1', '0', '0', '0', '203', '0', '0');
UPDATE `war_world`.`item_infos` SET `Salvageable` = '1' WHERE (`Entry` = '5501423');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '132', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('422', '133', '0', '0', '0', '(1,1)');


INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('134', 'Bloodlust Runestaff', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '11', '1', '9200', '10', '4', '0', '1024', '1', '0', '0', '943', '340', '40', '0', '0', '9:160;5:160;67:30;32:10;78:6;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '134', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '135', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('135', 'Bloodlust Spanner', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '3', '0', '9178', '13', '4', '8', '4', '1', '0', '0', '650', '240', '40', '0', '0', '8:100;5:100;67:10;32:5;77:3;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '203', '0', '0');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '2078321');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '2078322');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '2078323');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '136', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('136', 'Bloodlust Windstaff', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '11', '8', '9063', '10', '4', '0', '1024', '1', '0', '0', '943', '340', '40', '0', '0', '9:160;5:160;67:30;32:10;78:6;', '0', '0', '1', '60', '0', '1', '0', '1', '0', '0', '0', '202', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('423', '137', '0', '0', '0', '(1,1)');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`, `IsSiege`) VALUES ('137', 'Bloodlust Falcata', 'With great power comes great responsibility; or in this case, a blood sacrifice.', '1', '8', '9010', '13', '4', '0', '1', '1', '0', '0', '650', '240', '40', '0', '0', '8:100;5:100;67:10;32:5;77:3;', '0', '0', '1', '60', '0', '0', '0', '1', '0', '0', '0', '203', '0', '0');


INSERT INTO `war_world`.`instance_objects` (`instance_objects_ID`, `Entry`, `InstanceID`, `GameObjectSpawnID`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `VfxState`, `Realm`) VALUES ('4', '44', '160', '260410', '1016191', '1029338', '5828', '4070', '4496', '0', '2');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`) VALUES ('5092', 'Blood Stomp', '0', '0', 'ApplyCC', '32', '1', '5', 'Host');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `MinRange`, `Range`, `CastTime`, `Cooldown`, `ApCost`, `SpecialCost`, `EffectID`, `Specline`, `IgnoreGlobalCooldown`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('5063', '0', 'Blood Roots', '0', '25', '0', '30', '0', '0', '5063', 'NPC', '1', '0', '0', '5063', '0', '0', '0', '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`, `EffectRadius`, `MaxTargets`) VALUES ('5063', 'Blood Roots', '0', '0', 'InvokeBuff', '5063', 'Enemy', '255', '6');
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageVariance`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `CastTimeDamageMult`, `WeaponDamageFrom`, `WeaponDamageScale`, `NoCrits`, `Undefendable`, `OverrideDefenseEvent`, `StatUsed`, `StatDamageScale`, `ResourceBuild`, `HatredScale`, `HealHatredScale`, `PriStatMultiplier`) VALUES ('5063', '0', 'Blood Roots', '1000', '2000', '0', 'Physical', '0', '0', '1', '1', '1', '0', '1', '1', '0', '1', '0', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxStack`, `Duration`, `Silent`) VALUES ('5063', 'Blood Roots', '1', '10', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('5063', 'Blood Roots', '0', '0', 'Root', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `InvokeOn`, `Target`) VALUES ('5063', 'Blood Roots', '1', '0', 'DamageOverTime', '5', 'Host');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `MinRange`, `Range`, `CastTime`, `Cooldown`, `ApCost`, `SpecialCost`, `MoveCast`, `EffectID`, `Specline`, `WeaponNeeded`, `AffectsDead`, `IgnoreGlobalCooldown`, `IgnoreOwnModifiers`, `MinimumRank`, `MinimumRenown`, `IconId`, `abilityID`) VALUES ('5066', '0', 'Bloodpulse', '0', '333', '0', '20', '0', '0', '1', '2112', 'NPC', '0', '0', '1', '1', '0', '0', '5066', '5066');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`, `EffectRadius`, `MaxTargets`) VALUES ('5066', 'Bloodpulse', '0', '0', 'InvokeBuff', '5066', 'Enemy', '55', '6');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `Duration`, `Silent`) VALUES ('5066', 'Bloodpulse', '1', '1', '10', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('5066', 'Bloodpulse', '0', '0', 'DamageOverTime', '5', 'Host', '1');
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `StatDamageScale`, `HatredScale`, `HealHatredScale`) VALUES ('5066', '0', 'Bloodpulse', '4000', '8000', 'Spiritual', '0', '0', '1', '1', '1');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `RandomTarget`) VALUES ('48112', '5066', '20', '1642001', 'Bloodpulse', '0', '100', '0', '1', '1', '1');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `Cooldown`, `AbilityType`, `Specline`, `IgnoreGlobalCooldown`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('5551', '0', 'Bloodscent Aura', '15', '3', 'NPC', '1', '0', '0', '5551', '0', '0', '0', '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('5551', 'Bloodscent Aura', '0', '0', 'InvokeAura', '5551', 'Host');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `AuraPropagation`, `MaxStack`, `PersistsOnDeath`) VALUES ('5551', 'Bloodscent Aura', 'Group', '1', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`, `NoAutoUse`) VALUES ('5551', 'Bloodscent Aura', '0', '0', 'ModifyStat', '67', '100', '5', 'Host', '3', '1');
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `MinDamage`, `MaxDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `StatDamageScale`, `HatredScale`, `HealHatredScale`) VALUES ('5551', '0', 'Bloodscent Aura', '0', '0', 'Spiritual', '0', '0', '1', '1', '1');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('48112', '5551', '15', '1642002', 'Bloodscent', '0', '100', '0', '1', '1', '0', '1', '1');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `Range`, `CastTime`, `Cooldown`, `ApCost`, `SpecialCost`, `CastAngle`, `Specline`, `WeaponNeeded`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`, `AIRange`) VALUES ('5809', '0', 'Scent of Blood', '10', '0', '0', '0', '0', '0', 'NPC', '0', '0', '0', '5809', '0', '0', '0', '0', '10');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('5809', 'Scent of Blood', '0', '0', 'InvokeBuff', '5809', 'Enemy');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxStack`, `Duration`, `Interval`, `CanRefresh`, `Silent`) VALUES ('5809', 'Scent of Blood', '1', '15', '1000', '1', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('5809', 'Scent of Blood', '0', '0', 'ModifyStat', '67', '100', '5', 'Host', '1');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('45088', '5809', '0', '1600000', 'Scent of Blood', '0', '100', '0', '1', '1', '1', '1', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `Range`, `CastTime`, `Cooldown`, `ApCost`, `EffectID`, `AbilityType`, `Specline`, `WeaponNeeded`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`, `AIRange`) VALUES ('13713', '0', 'Thar\'lgnan\'s Hurl', '10', '0', '25', '0', '13713', '1', 'NPC', '0', '0', '0', '13713', '0', '0', '0', '0', '10');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('13713', 'Thar\'lgnan\'s Hurl', '0', '0', 'PuntEnemy', '13713', 'Enemy');
INSERT INTO `war_world`.`ability_knockback_info` (`Entry`, `Id`, `Angle`, `Power`, `GravMultiplier`) VALUES ('13713', '0', '72', '600', '2');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('45084', '13713', '25', '1631004', 'Thar\'lgnan\'s Hurl', '0', '100', '0', '1', '1', '0', '1', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `CastTime`, `Cooldown`, `ApCost`, `SpecialCost`, `MoveCast`, `EffectID`, `Specline`, `AffectsDead`, `IgnoreGlobalCooldown`, `MinimumRank`, `MinimumRenown`, `IconId`, `Category`, `Flags`, `PointCost`, `CashCost`) VALUES ('13307', '0', 'Charge', '0', '0', '0', '0', '1', '13307', 'NPC', '0', '0', '0', '0', '13307', '0', '0', '0', '0');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('13307', 'Charge', '0', '0', 'InvokeBuff', '13307', 'Caster');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `Duration`, `Silent`) VALUES ('13307', 'Charge', '1', '1', '5', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('13307', 'Charge', '0', '0', 'ModifySpeed', '200', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('13307', 'Charge', '0', '1', 'ApplyCC', '16', '1', '5', 'Host', '2');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('45084', '13307', '0', '1631005', 'Charge', '0', '100', '0', '1', '1', '0', '1', '0');


UPDATE `war_world`.`ability_commands` SET `PrimaryValue` = NULL WHERE (`Entry` = '13713') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `MaxCopies` = '1' WHERE (`Entry` = '5809');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1600000');
UPDATE `war_world`.`creature_abilities` SET `Text` = '' WHERE (`creature_abilities_ID` = '1600000');
-- working
UPDATE `war_world`.`buff_infos` SET `Silent` = '0' WHERE (`Entry` = '5063');
UPDATE `war_world`.`ability_damage_heals` SET `Index` = '1' WHERE (`Entry` = '5063') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `InvokeOn` = '7' WHERE (`Entry` = '5063') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`abilities` SET `IgnoreGlobalCooldown` = '0' WHERE (`Entry` = '5063');
UPDATE `war_world`.`abilities` SET `IgnoreGlobalCooldown` = '0' WHERE (`Entry` = '5092');
UPDATE `war_world`.`abilities` SET `Range` = '35', `MinimumRank` = '1', `AIRange` = '35' WHERE (`Entry` = '5063');
UPDATE `war_world`.`ability_commands` SET `EffectRadius` = '100', `FromAllTargets` = '1' WHERE (`Entry` = '5063') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `MaxCopies` = '1', `Duration` = '5' WHERE (`Entry` = '5063');
UPDATE `war_world`.`ability_commands` SET `FromAllTargets` = '1', `MaxTargets` = '6' WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`ability_knockback_info` SET `Power` = '1200', `RangeExtension` = '60', `GravMultiplier` = '3' WHERE (`Entry` = '13713') and (`Id` = '0');
UPDATE `war_world`.`ability_knockback_info` SET `RangeExtension` = '300' WHERE (`Entry` = '13713') and (`Id` = '0');
UPDATE `war_world`.`buff_commands` SET `PrimaryValue` = '48', `TertiaryValue` = '5' WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `Duration` = '5' WHERE (`Entry` = '5092');
UPDATE `war_world`.`buff_commands` SET `EffectRadius` = '180', `MaxTargets` = '6' WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `BuffLine` = '1' WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_knockback_info` SET `RangeExtension` = '500' WHERE (`Entry` = '13713') and (`Id` = '0');
-- ability type 1
UPDATE `war_world`.`abilities` SET `AbilityType` = '1' WHERE (`Entry` = '5092');
-- working
UPDATE `war_world`.`ability_knockback_info` SET `RangeExtension` = '700' WHERE (`Entry` = '13713') and (`Id` = '0');
UPDATE `war_world`.`ability_commands` SET `FromAllTargets` = NULL, `AttackingStat` = '1' WHERE (`Entry` = '5063') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = 'Career', `Group` = '0', `Interval` = '1000', `CanRefresh` = '1' WHERE (`Entry` = '5063');
UPDATE `war_world`.`buff_commands` SET `EffectSource` = 'Caster', `BuffLine` = '1' WHERE (`Entry` = '5063') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `EffectSource` = 'Caster' WHERE (`Entry` = '5063') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`abilities` SET `AbilityType` = '1' WHERE (`Entry` = '5063');
UPDATE `war_world`.`quests` SET `OnCompletionQuest` = 'You have done well. Skull Lord Var\'lthrok and his supporters are now banished to his Bastion Stair. Claim your new weapon and send this daemon to hell once and for all.' WHERE (`Entry` = '1');
UPDATE `war_world`.`quests` SET `OnCompletionQuest` = 'You have done well. Skull Lord Var\'lthrok and his supporters are now banished to his Bastion Stair. Claim your new weapon and send this daemon to hell once and for all.' WHERE (`Entry` = '2');
UPDATE `war_world`.`quests_objectives` SET `ObjCount` = '1000' WHERE (`Guid` = '74835850');
UPDATE `war_world`.`quests_objectives` SET `ObjCount` = '1000' WHERE (`Guid` = '74835852');

# 13
UPDATE `war_world`.`quests_objectives` SET `ObjCount` = '250' WHERE (`Guid` = '74835852');
UPDATE `war_world`.`quests_objectives` SET `ObjCount` = '250' WHERE (`Guid` = '74835850');

UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '168899368');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '168944168');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204473704');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204473768');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204473832');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204476136');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204478248');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204478312');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772268');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167782376');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167782248');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167782184');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '108003496');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '108003624');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '108856040');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772264');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772266');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772267');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772328');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772329');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167775464');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167775848');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167776232');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167776360');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167819500');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '170923752');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '171972648');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '173020904');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '174063721');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '174063726');

# Dye Reward Update
UPDATE `war_world`.`quests` SET `Choice` = '[129838021,7],[1,1]' WHERE (`Entry` = '2');
UPDATE `war_world`.`quests` SET `Choice` = '[129838021,7],[1,1]' WHERE (`Entry` = '1');

# Horgul HP Mod
UPDATE `war_world`.`creature_protos` SET `WoundsModifier` = '4' WHERE (`Entry` = '6841');

# Thar Engrage
UPDATE `war_world`.`creature_abilities` SET `Cooldown` = '0' WHERE (`creature_abilities_ID` = '1631002');

# Terror Mod
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '2601003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '2602003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '2603002');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '2604003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '2605003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '1631003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '1642000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '1653000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '1664003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '1951000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '1952000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '1953000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '1', `RandomTarget` = '1' WHERE (`creature_abilities_ID` = '1954000');
UPDATE `war_world`.`buff_commands` SET `PrimaryValue` = '22' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `CommandID` = '1', `CommandSequence` = '0' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`buff_commands` SET `BuffLine` = '2' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `BuffLine` = '3' WHERE (`Entry` = '5968') and (`CommandID` = '1') and (`CommandSequence` = '0');

# Instance Purge
DELETE FROM `war_world`.`instance_lockouts`;
DELETE FROM `war_world`.`instance_statistics`;

# Teror
UPDATE `war_world`.`buff_commands` SET `PrimaryValue` = '23' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `BuffLine` = '1' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `BuffLine` = '1' WHERE (`Entry` = '5968') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `NoAutoUse` = '0' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`abilities` SET `AbilityType` = '3' WHERE (`Entry` = '5968');
UPDATE `war_world`.`ability_commands` SET `MaxTargets` = '64' WHERE (`Entry` = '5968') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_commands` SET `CommandID` = '0', `CommandSequence` = '1', `CommandName` = 'InvokeBuff', `Target` = 'Enemy', `EffectRadius` = '255' WHERE (`Entry` = '5968') and (`CommandID` = '1') and (`CommandSequence` = '0');


INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `Range`, `CastTime`, `Cooldown`, `ApCost`, `EffectID`, `AbilityType`, `Specline`, `WeaponNeeded`, `IgnoreGlobalCooldown`, `MinimumRank`, `MinimumRenown`, `AIRange`) VALUES ('5967', '0', 'Terror', '320', '0', '0', '0', '5967', '3', 'NPC', '0', '1', '0', '0', '320');
UPDATE `war_world`.`abilities` SET `EffectID` = '5968', `IgnoreGlobalCooldown` = '1' WHERE (`Entry` = '5968');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`, `EffectRadius`) VALUES ('5967', 'Terror', '0', '0', 'InvokeBuff', '5967', 'Enemy', '0');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`, `CanRefresh`, `Silent`) VALUES ('5967', 'Terror', '1', '1', '1', '1', '1');
UPDATE `war_world`.`buff_commands` SET `Entry` = '5967' WHERE (`Entry` = '5968') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `CommandID` = '0' WHERE (`Entry` = '5968') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1631003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1642000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1653000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1664003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1951000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1952000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1953000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1954000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2601003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2602003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2603002');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2604003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2605003');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('45084', '5967', '0', '1631006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('48112', '5967', '0', '1642006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('2000751', '5967', '0', '1653006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('64106', '5967', '0', '1664006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('2000763', '5967', '0', '1951006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('46995', '5967', '0', '1952006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('2000757', '5967', '0', '1953006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('46205', '5967', '0', '1954006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('4276', '5967', '0', '2601006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('59211', '5967', '0', '2602006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6821', '5967', '0', '2603006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6841', '5967', '0', '2604006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');
INSERT INTO `war_world`.`creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`) VALUES ('6843', '5967', '0', '2605006', 'FEAR ME', '0', '100', '0', '0', '1', '1', '0', '0');


UPDATE `war_world`.`ability_commands` SET `Target` = 'Caster' WHERE (`Entry` = '5967') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `Duration` = '480' WHERE (`Entry` = '5967');

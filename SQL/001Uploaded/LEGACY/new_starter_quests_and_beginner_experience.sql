#proto
SET SQL_SAFE_UPDATES = 0;
DELETE FROM `war_world`.`creature_protos` where entry in (10505154,10505155,10505156,10505157);


INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `BaseRadiusUnits`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505154', 'Griffon Recruiters', '1219', '0', '52', '52', '40', '40', '64', '5', '18', '0', '0', '61', '1', '1000', '0', '0', '13573', '0', '64525633521916729', '', '16', '91', '0', '0', '0', NULL, '0 0 0 3 1 10\r', NULL, '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `BaseRadiusUnits`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505155', 'Gyropack Riders', '1214', '0', '55', '55', '40', '40', '80', '5', '18', '0', '0', '29', '1', '1000', '0', '0', '50904', '1', '51107032763080272', '', '13', '66', '0', '0', '0', NULL, '0 0 136 8 1 10\r', NULL, '0', '1.00', '1.00', '0', '0', '0');
UPDATE `war_world`.`creature_protos` SET `Entry`='2000831', `Name`='Dal\'nishra, Weaver of Fate', `Model1`='1033', `Model2`='0', `MinScale`='75', `MaxScale`='75', `MinLevel`='40', `MaxLevel`='40', `Faction`='132', `Ranged`='10', `Icone`='0', `Emote`='2', `Title`='0', `Unk`='0', `Unk1`='0', `Unk2`='0', `Unk3`='0', `Unk4`='0', `Unk5`='0', `Unk6`='0', `Flag`='0', `ScriptName`='', `CreatureType`='0', `CreatureSubType`='148', `TokUnlock`=NULL, `LairBoss`='0', `VendorID`='0', `States`='24', `FigLeafData`='4 203 1 10', `BaseRadiusUnits`=NULL, `Career`='0', `PowerModifier`='1.00', `WoundsModifier`='1.00', `Invulnerable`='0', `WeaponDPS`='0', `ImmuneToCC`='0' WHERE (`Entry`='2000831');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `BaseRadiusUnits`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505156', 'Dark Recruiters', '1222', '0', '60', '60', '40', '40', '131', '5', '18', '0', '0', '51', '0', '1000', '0', '1', '17556', '5', '21802436713533324', '', '16', '89', '0', '0', '0', NULL, NULL, NULL, '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `States`, `FigLeafData`, `BaseRadiusUnits`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505157', 'Dark Recruiter', '1222', '0', '60', '60', '40', '40', '131', '5', '18', '0', '0', '51', '0', '1000', '0', '1', '17556', '5', '21802436713533324', '', '16', '89', '0', '0', '0', NULL, NULL, NULL, '0', '1', '1', '0', '0', '0');

# quest mobs spawned

DELETE FROM `war_world`.`creature_spawns` where guid in ('7210882',
'7210883',
'7210877',
'7210876',
'7210873',
'7210874',
'7210875',
'7210871',
'7210890',
'7210891',
'7210892');

INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210882', '2000831', '100', '848358', '829856', '7972', '890', '0', '2', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210883', '1000226', '100', '848436', '830027', '7977', '1136', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210877', '10505155', '162', '126777', '128891', '12699', '738', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210876', '1000225', '106', '834852', '936097', '6979', '112', '0', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210873', '10505154', '162', '123976', '125202', '13131', '1888', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210874', '98323', '162', '124065', '125155', '13131', '1294', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210875', '98323', '162', '123879', '125192', '13131', '2670', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210871', '98323', '106', '835013', '936318', '6984', '242', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210890', '10505157', '161', '440359', '140532', '17057', '1784', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210891', '10505157', '161', '440061', '140537', '17057', '2334', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7210892', '10505156', '161', '440204', '140548', '17057', '2020', '18', '0', '0', '0', '0', '0', '0', '1');


#added scroll to merchant Needs to be removed from the player

DELETE FROM `war_world`.`vendor_items` where ItemId = 1337;

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '51', '1337', '0', '0', '0', '0');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqTokUnlock`, `ReqGuildlvl`, `ReqItems`) VALUES ('0', '50', '1337', '0', NULL, '0', '0');
UPDATE `war_world`.`item_infos` SET `Entry`='1337', `Name`='The Winds of Magic', `Description`='By reading this magical scroll you will level up to Rank 40 instantly!', `Type`='31', `Race`='0', `ModelId`='7034', `SlotId`='0', `Rarity`='5', `Career`='0', `Skills`='0', `Bind`='1', `Armor`='0', `SpellId`='0', `Dps`='0', `Speed`='0', `MinRank`='1', `MinRenown`='0', `StartQuest`='0', `Stats`='0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', `SellPrice`='0', `TalismanSlots`='0', `MaxStack`='60', `ScriptName`='', `ObjectLevel`='5', `UniqueEquiped`='0', `Crafts`='0', `Unk27`='0', `SellRequiredItems`='0', `TwoHanded`='0', `ItemSet`='0', `Craftresult`='0', `DyeAble`='0', `Salvageable`='0', `BaseColor1`='0', `BaseColor2`='0', `TokUnlock`='0', `Effects`='0', `TokUnlock2`='0', `IsSiege`='0' WHERE (`Entry`='1337');

# creature items (gear)

delete from `war_world`.`creature_items` where entry in (10505155,10505156,10505157,10505154);

INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '20', '2091', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '21', '2093', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '22', '2092', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '23', '2095', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '24', '2094', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505155', '28', '2090', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '10', '3414', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '11', '5740', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '20', '3208', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '21', '3210', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '22', '3209', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '23', '3213', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '24', '3212', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '25', '3211', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505156', '28', '3207', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '10', '3414', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '11', '5740', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '20', '3208', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '21', '3210', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '22', '3209', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '23', '3213', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '24', '3212', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '25', '3211', '31868', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505157', '28', '3207', '31868', '0', '0');
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

# Portal to city and teleport
delete from `war_world`.`gameobject_protos` where entry in (3100417,3100418);
delete from `war_world`.`gameobject_spawns` where Guid in (2119359,2119659);
delete from `war_world`.`zone_jumps` where entry in (2119359,2119659);

INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `TokUnlock`, `Unks`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureSpawnText`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('3100417', 'Rift in Time', '1583', '50', '1', '0', '1', '', NULL, '7680 0 18181 36 30501 64168', '0', '0', '25700', '0', '0', '0', NULL, '0', '0');
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('2119359', '162', '123860', '124815', '13131', '0', '1', '0', NULL);
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `TokUnlock`, `Unks`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureSpawnText`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('3100418', 'Rift in Time', '1583', '50', '1', '0', '1', '', NULL, '7680 0 18181 36 30501 64168', '0', '0', '25700', '0', '0', '0', NULL, '0', '0');
INSERT INTO `war_world`.`zone_jumps` (`Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Enabled`, `Type`, `InstanceID`) VALUES ('2119659', '162', '440227', '139944', '17057', '6', '1', '0', NULL);

INSERT INTO war_world.gameobject_spawns (Guid, Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, DisplayID, Unk1, Unk2, Unk3, Unk4, Unks, DoorId, VfxState, TokUnlock, SoundId, AllowVfxUpdate, AlternativeName) VALUES ('2119359', '3100417', '106', '834940', '936182', '6976', '104', '1583', '0', '0', '0', '0', '7682 0 0 4 5 0 ', '0', '0', '', '0', '0', '');
INSERT INTO war_world.gameobject_spawns (Guid, Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, DisplayID, Unk1, Unk2, Unk3, Unk4, Unks, DoorId, VfxState, TokUnlock, SoundId, AllowVfxUpdate, AlternativeName) VALUES ('2119659', '3100418', '100', '848490', '829913', '7976', '962', '1583', '0', '0', '0', '0', '7680 0 18181 36 30501 -1368 ', '0', '0', '', '0', '0', '');

#Quest marker order

delete from `war_world`.`quests_objectives`  where guid in (74835849,74835850,74835851);
delete from `war_world`.`quests_maps`  where quests_maps_ID in ('60504','60506','60508');

INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60504', '60504', '2', 'Trainer Eduardt', 'Trainer Eduardt', '162', '1130', '15539', '38383', '0', '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835849', '60504', '1', '1', 'Speak with Trainer Eduardt', '99797', NULL, NULL, '0');

INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60506', '60506', '2', 'Speak to Martial Marshal', 'Speak to Martial Marshal', '162', '1130', '20720', '48351', '0', '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835850', '60506', '1', '1', 'Speak to Martial Marshal', '1000244', NULL, NULL, '0');

INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60508', '60508', '2', 'Speak to Gyropack Riders', 'Speak to Gyropack Riders', '162', '1130', '28473', '30587', '0', '0');
INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835851', '60508', '1', '1', 'Speak to Gyropack Riders', '10505155', NULL, NULL, '0');

#Quest marker Destro

delete from `war_world`.`quests_objectives`  where guid in (74835852,74835853,74835854);
delete from `war_world`.`quests_maps`  where quests_maps_ID in ('60512','60514','60516');

INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835852', '60512', '1', '1', 'Speak with Seer Uresha', '1351', NULL, NULL, '0');
INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60512', '60512', '2', 'Seer Uresha', 'Seer Uresha', '161', '1130', '23348', '37808', '0', '0');

INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835853', '60514', '1', '1', 'Speak to Martial Berserker', '1000235', NULL, NULL, '0');
INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60514', '60514', '2', 'Speak to Martial Berserker', 'Speak to Martial Berserker', '161', '1130', '32247', '53687', '0', '0');

INSERT INTO `war_world`.`quests_objectives` (`Guid`, `Entry`, `ObjType`, `ObjCount`, `Description`, `ObjID`, `PQArea`, `inZones`, `PreviousObj`) VALUES ('74835854', '60516', '1', '1', 'Speak to Wyvern tendin git', '1008000', NULL, NULL, '0');
INSERT INTO `war_world`.`quests_maps` (`quests_maps_ID`, `Entry`, `Id`, `Name`, `Description`, `ZoneId`, `Icon`, `X`, `Y`, `Unk`, `When`) VALUES ('60516', '60516', '2', 'Speak to Wyvern tendin git', 'Speak to Wyvern tendin git', '161', '1130', '28439', '31122', '0', '0');


#delete old welcome to apoc quest

DELETE FROM `war_world`.`quests_creature_starter` WHERE (`Entry` = '60500');
DELETE FROM `war_world`.`quests_creature_starter` WHERE (`Entry` = '60501');
DELETE FROM `war_world`.`quests_creature_starter` WHERE (`Entry` = '60502');
DELETE FROM `war_world`.`quests_creature_starter` WHERE (`Entry` = '60503');
DELETE FROM `war_world`.`quests_creature_starter` WHERE (`Entry` = '65535');


DELETE FROM `war_world`.`quests_creature_finisher` WHERE (`Entry` = '60500');
DELETE FROM `war_world`.`quests_creature_finisher` WHERE (`Entry` = '60501');
DELETE FROM `war_world`.`quests_creature_finisher` WHERE (`Entry` = '60502');
DELETE FROM `war_world`.`quests_creature_finisher` WHERE (`Entry` = '60503');
DELETE FROM `war_world`.`quests_creature_finisher` WHERE (`Entry` = '65535');

# quest new Order side

delete from `war_world`.`quests` where entry in ('60501',
'60502',
'60503',
'60504',
'60505',
'60506',
'60507',
'60508');

INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60501', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Soldier.\nBy order of Emperor Karl Franz we welcome you to the Alliance of Order. We offer you a set of armor and a magical scroll to prepare you for the front lines of battle. Should you choose to accept this offer, The Annihilator Merchant will prepare you for the battle ahead. Speak with him to receive these gifts your Emperor has provided for you. \nShould you choose to decline this offer the lands in front of you are vast and filled with peril.\nThe journey ahead will be a long and hard one.', 'Speak to the Annihilator Merchant and buy your Gifts for free.', '5000', '50', '', '', '1', '0', 'Forward, Soldier!', 'Ah, you\'ve learned the ways of the War? Then let us be about our grim task!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60502', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'By Sigmar! Welcome Soldier, you have been sent to me by the Griffon Recruiter to prepare you for the battle ahead, buy this gear and scroll off me to get you ready for War!', 'Your King has prepared for you the finest of equipment! Buy the Annihilator Gear set and magical scroll.', '5000', '50', '', '', '1', '60501', 'Lets get to business!', 'You have Bought your starter gears (i Hope) now let\'s continue!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60503', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Now Soldier! You have received your gifts and accepted your Faith! In front of you Lies a Rift of Time take this to meet up with the Griffon Recruiters at the Palace Gates of our city Altdorf!', 'Journey through the Rift! And meet up with the Griffon Recruiters by the Palace Gates.', '5000', '50', '', '', '1', '60502', 'Journey through the Rift! Speak to the Griffon Recruiters by the Palace Gates when you arrive.', 'Well done Soldier!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60504', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Soldier, The pinnacle of Greatness Altdorf the city of the Alliance of Order. Here we are going to train you further to prepare you the best we can for War!', 'Speak to the Career Trainer by The Temple of Sigmar to train and sharpen your skills.', '5000', '5000', '', '', '1', '60503', 'Let\'s Continue!', 'Very well done Soldier u might progress to be a Realm Captain 1 day!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60505', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Soldier, I\'m the Career Trainer we provide you specialization into your chosen path, which increases your strengths or defenses! Choose it wisely! Don\'t worry to you can always respecialize it into another path!', 'Train your Mastery Abilities by the Career trainer!', '5000', '5000', '', '', '1', '60504', 'Let\'s Continue sharpening your skills!', 'Very well done Soldier!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60506', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done soldier! you are Almost ready for the War!', 'Speak to Martial Marshal to Buy your weapons! ', '5000', '5000', '', '', '1', '60505', 'Let\'s Continue on the road to progress this time ill send you off to buy yourself some Weapons!', 'Very well done Soldier! Make sure you check the other merchants out as well!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60507', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done soldier! you are very close for the War! Equip this fine weaponry and check for some jewels!', 'Make haste with buying all your gears and speak to the Marshal protector to pick up your accessories!', '5000', '5000', '', '', '1', '60506', 'By the Grace of Sigmar Fight well and Serve with Honor!', 'Sigmars Grace be with you!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60508', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done soldier! you are ready for the War! Fly to the battlefield as soon as you are ready and crush our enemies!', 'Speak to the Gyropack Riders and then the Flightmaster to Fly to the Battlefield! ', '5000', '5000', '', '', '1', '60507', 'By the Grace of Sigmar Fight well and Serve with Honor!', 'Sigmars Grace be with you!', '0', '1', '0');


# quest new destro side

delete from `war_world`.`quests` where entry in ('60509',
'60510',
'60511',
'60512',
'60513',
'60514',
'60515',
'60516');

INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60509', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome, Fiend.\nBy order of King Tchar\'zanek, we welcome you to the lands of Chaos. We offer you a set of Armor and magical scrolls to prepare you for the front lines of battle. Should you choose to accept this offer, Annihilator Merchant will prepare you for the battle ahead. Speak with him to receive these gifts your King has provided for you. \nShould you choose to decline this offer the lands in front of you are vast and filled with peril.\nThe journey ahead will be a long and hard one.', 'Speak to the Annihilator Merchant and buy your Gifts for free.', '5000', '50', '', '', '1', '0', 'Forward, Maggot!', 'Ah, you\'ve learned the ways of the War? Then let us be about our grim task!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60510', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'By the Dark Gods! Welcome Maggot, you have been sent to me by Dal\'nishra, Weaver of Fate to prepare you for the battle ahead, buy this gear and scroll off me to get you ready for War!', 'Your King has prepared for you the finest of equipment! Buy the Annihilator Gear set and magical scroll.', '5000', '50', '', '', '1', '60509', 'Lets get to business!', 'You have Bought your starter gears (i Hope) now let\'s continue!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60511', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Now Maggot! You have received your gifts and accepted your Faith! In front of you Lies a Rift of Time take this to meet up with the Dark Recruiters at the south of our Inevitable city!', 'Journey through the Rift! And meet up with the Dark Recruiters by the south side of the city.', '5000', '50', '', '', '1', '60510', 'Journey through the Rift! Speak to the Dark Recruiters by the south side of the city when you arrive.', 'Well done Maggot!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60512', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Maggot, The pinnacle of Darkness The Inevitable city the Alliance of Destruction. Here we are going to train you further to prepare you the best we can for War!', 'Speak to the Career Trainer by The Monolith to train and sharpen your skills.', '5000', '5000', '', '', '1', '60511', 'Let\'s Continue!', 'Very well done Maggot u might progress to be a Realm Captain 1 day!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60513', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Welcome Maggot, I\'m the Career Trainer we provide you specialization into your chosen path, which increases your strengths or defenses! Choose it wisely! Don\'t worry to you can always respecialize it into another path!', 'Train your Mastery Abilities by the Career trainer!', '5000', '5000', '', '', '1', '60512', 'Let\'s Continue sharpening your skills!', 'Very well done Maggot!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60514', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done Maggot! you are Almost ready for the War!', 'Speak to Martial Berserker to Buy your weapons! ', '5000', '5000', '', '', '1', '60513', 'Let\'s Continue on the road to progress this time ill send you off to buy yourself some Weapons!', 'Very well done Maggot! Make sure you check the other merchants out as well!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60515', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done Maggot! you are very close for the War! Equip this fine weaponry and check for some jewels!', 'Make haste with buying all your gears and speak to the Marshal Steelhide to pick up your accessories!', '5000', '5000', '', '', '1', '60514', 'By the Grace of The Dark God Fight well and Serve with Honor!', 'The Dark Gods be with you!', '0', '1', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60516', 'Welcome To Warhammer Online!', '0', '1', '40', '0', '80', 'Nicely done Maggot! you are ready for the War! Fly to the battlefield as soon as you are ready and crush our enemies!', 'Speak to the Wyvern tendin git and then the Flightmaster to Fly to the Battlefield! ', '5000', '5000', '', '', '1', '60515', 'By the Grace of The Dark God Fight well and Serve with Honor!', 'The Dark Gods be with you!', '0', '1', '0');

#quests_creature_starter

DELETE FROM `war_world`.`quests_creature_starter` where entry in ('60501',
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


#quests_creature_finisher

DELETE FROM `war_world`.`quests_creature_finisher` where entry in ('60501',
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




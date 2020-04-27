DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '1910001');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `FigLeafData`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('1910001', 'Kem Senef the Traitor', '1729', '0', '50', '50', '41', '41', '5', '5', '18', '0', '0', '5', '0', '1001', '0', '1', '19581', '0', '8078040539733916', '', '25', '139', '0', '0', '0', '0 0 0 14 1 10\r', '0', '1.00', '1.00', '0', '0', '0');
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '93719');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `FigLeafData`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('93719', 'Ibehme the Fury of Asaph^F', '1763', '0', '120', '120', '41', '41', '5', '5', '18', '0', '0', '5', '0', '1001', '0', '0', '0', '0', '59150681346480376', '', '25', '134', '4404', '0', '0', '0 0 0 79 1 10\r', '0', '1.00', '1.00', '0', '0', '0');
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '93692');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `FigLeafData`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('93692', 'Nekh Akhet^M', '1717', '0', '100', '100', '37', '37', '5', '5', '18', '0', '0', '37', '0', '1001', '0', '1', '9014', '5', '48616561798553361', '', '26', '142', '0', '0', '0', '0 0 0 11 1 10\r', '0', '1.00', '1.00', '0', '0', '0');
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '93585');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `FigLeafData`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('93585', 'Amsu^M', '1717', '0', '65', '65', '37', '37', '5', '5', '18', '0', '0', '37', '0', '1001', '0', '1', '7666', '4', '24990910125385088', '', '26', '142', '0', '0', '0', '0 0 0 66 1 10\r', '0', '1.00', '1.00', '0', '0', '0');
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '10505106');
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '10505107');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505106', 'Treasure Hunter Kurim', '1001', '0', '50', '50', '40', '40', '65', '5', '0', '0', '11', '0', '0', '0', '0', '0', '0', '0', '0', '', '15', '85', '0', '0', '0', '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505107', 'Treasure Hunter Rungni', '1001', '0', '50', '50', '40', '40', '65', '5', '0', '0', '12', '0', '0', '0', '0', '0', '0', '0', '0', '', '15', '85', '0', '0', '0', '0', '1.00', '1.00', '0', '0', '0');



DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505106') and (`SlotId` = '10');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505106') and (`SlotId` = '20');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505106') and (`SlotId` = '21');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505106') and (`SlotId` = '22');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505106') and (`SlotId` = '25');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505106') and (`SlotId` = '28');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505106', '10', '1175', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505106', '20', '3196', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505106', '21', '3198', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505106', '22', '3197', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505106', '25', '3199', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505106', '28', '3195', '0', '0', '0');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505107') and (`SlotId` = '10');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505107') and (`SlotId` = '20');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505107') and (`SlotId` = '21');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505107') and (`SlotId` = '22');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505107') and (`SlotId` = '23');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505107') and (`SlotId` = '24');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505107') and (`SlotId` = '25');
DELETE FROM `war_world`.`creature_items` WHERE (`Entry` = '10505107') and (`SlotId` = '28');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505107', '10', '1300', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505107', '20', '2981', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505107', '21', '2983', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505107', '22', '2982', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505107', '23', '2979', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505107', '24', '2985', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505107', '25', '2984', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505107', '28', '2980', '0', '0', '0');





DELETE FROM `war_world`.`gameobject_protos` WHERE (`Entry` = '2000570');
INSERT INTO `war_world`.`gameobject_protos` (`Entry`, `Name`, `DisplayID`, `Scale`, `Level`, `Faction`, `HealthPoints`, `ScriptName`, `Unks`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `CreatureId`, `CreatureCount`, `CreatureCooldownMinutes`, `IsAttackable`) VALUES ('2000570', 'Mailbox chosen', '4496', '50', '1', '0', '1', 'MailBoxScript', '7745 0 50943 0 5 44655', '0', '0', '100', '0', '0', '0', '0', '0');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '2118573');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unks`, `DoorId`, `VfxState`, `TokUnlock`, `SoundId`, `AllowVfxUpdate`, `AlternativeName`) VALUES ('2118573', '200028', '191', '256839', '1536096', '10267', '36', '8302', '0', '0', '0', '0', '7745 0 -14593 0 5 -20881 ', '0', '0', '', '0', '0', '');
DELETE FROM `war_world`.`gameobject_spawns` WHERE (`Guid` = '2118417');
INSERT INTO `war_world`.`gameobject_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `DisplayID`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unks`, `DoorId`, `VfxState`, `TokUnlock`, `SoundId`, `AllowVfxUpdate`, `AlternativeName`) VALUES ('2118417', '98928', '191', '256346', '1531173', '10983', '1510', '1789', '0', '0', '0', '0', '0 0 0 1 0 0 ', '0', '0', '', '0', '0', '');










DELETE FROM `war_world`.`pquest_info` WHERE (`Entry` = '559');
INSERT INTO `war_world`.`pquest_info` (`Entry`, `Name`, `Type`, `Level`, `ZoneId`, `PinX`, `PinY`, `TokDiscovered`, `TokUnlocked`, `ChapterId`, `GoldChestWorldX`, `GoldChestWorldY`, `GoldChestWorldZ`, `PQType`, `PQDifficult`, `Chapter`, `PQTier`, `PQCraftingBag`, `PQAreaId`, `SoundPQEnd`, `RespawnID`) VALUES ('559', 'Pit of Asaph', '0', '0', '191', '12725', '13864', '0', '0', '0', '207947', '1504833', '6432', '1', '2', '0', '4', '4', '9', '0', '0');

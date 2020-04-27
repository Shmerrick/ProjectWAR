SET SQL_SAFE_UPDATES = 0;
delete from `war_world`.`quests` where entry = 60501
delete from `war_world`.`quests` where entry = 60503
delete from `war_world`.`quests_creature_finisher` where  entry = '60501'
delete from `war_world`.`quests_creature_finisher` where  entry = '60503'
delete from `war_world`.`creature_spawns` where guid  = '1215689'
delete from `war_world`.`creature_protos` where entry= '1008001'
delete from `war_world`.`quests_creature_finisher` where entry = 60503


INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60501', 'Welcome To APOCALYPSE!', '0', '1', '40', '0', '80', 'Sigmar be praised, the reinforcements have arrived at last! Not a moment too soon either, looks to me as if the Raven Host is gearing up for another assault. We\'ve got things pretty well contained here at the moment, but beyond the city... well, you\'ll see. Anyway, that\'s neither here nor there. You didn\'t come here to talk, you came to fight. Go see the Martial Marshal just over yonder in the War Quarter. He\'ll get you kitted out.', 'Go to the War Quarter in Altdorf. Talk to the Martial Marshal for your battle gear.', '0', '50', '', '', '1', '0', '', '', '0', '1', '1');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60503', 'Welcome To APOCALYPSE!', '0', '1', '40', '0', '80', 'Pick out your gear and head up to the dwarven flightmaster when you\'re done. They\'re set up near the square, they\'ll take you right to the action. Sigmar\'s grace be with you.', 'Go to the Flightmaster in Altdorf.', '0', '2000', '', '', '1', '60501', '', 'Don\'t you worry your pretty little head! We\'ll get you there in one piece! First ride is on us.', '0', '1', '0');
UPDATE `war_world`.`quests_creature_starter` SET `Entry` = '60501' WHERE (`CreatureId` = '98323');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60501', '1000244');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60503', '1000244');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '317286');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('1215689', '98323', '162', '123981', '128045', '12493', '1684', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `FigLeafData`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('1008001', 'Flight Mechanic^m', '1214', '0', '50', '50', '40', '40', '65', '5', '18', '0', '0', '37', '1', '1000', '0', '0', '36392', '0', '56414925688353302', '', '13', '66', '0', '0', '0', '0 0 0 8 1 10\r', '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008001', '10', '4957', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008001', '20', '2007', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008001', '21', '2009', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008001', '22', '2008', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008001', '23', '2011', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008001', '24', '2010', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008001', '25', '650', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008001', '28', '2006', '0', '0', '0');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('1215690', '1008001', '162', '126352', '128743', '12699', '2468', '18', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60503', '1008001');


UPDATE `war_world`.`item_sets` SET `BonusString` = '34:15,344,0|35:8,66,0|36:5,66,0|37:42,5,0|86:10412|' WHERE (`Entry` = '4375');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:4,72,0|37:29,5,0|86:10389|' WHERE (`Entry` = '4437');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:4,72,0|37:29,5,0|86:10389|' WHERE (`Entry` = '4448');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,66,0|35:3,66,0|36:5,66,0|37:78,5,0|86:10410|' WHERE (`Entry` = '4426');


delete from `war_world`.`quests` where entry = 60501;
delete from `war_world`.`quests` where entry = 60500;

delete from `war_world`.`quests_creature_finisher` where  entry = '60500';
delete from `war_world`.`creature_spawns` where guid  = '1215687';
delete from `war_world`.`creature_protos` where entry= '1008000';
delete from `war_world`.`quests_creature_finisher` where entry = 60502;
delete from `war_world`.`quests_creature_starter` where entry=60502;


DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '317499');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('1215687', '13', '161', '440822', '123997', '16848', '1626', '18', '0', '0', '0', '0', '0', '0', '1');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '40', `MaxLevel` = '40' WHERE (`Entry` = '13');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7300' WHERE (`Entry` = '13') and (`SlotId` = '20');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7302' WHERE (`Entry` = '13') and (`SlotId` = '21');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7301' WHERE (`Entry` = '13') and (`SlotId` = '22');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7298' WHERE (`Entry` = '13') and (`SlotId` = '23');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7303' WHERE (`Entry` = '13') and (`SlotId` = '24');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7299' WHERE (`Entry` = '13') and (`SlotId` = '28');
UPDATE `war_world`.`creature_items` SET `ModelId` = '9237' WHERE (`Entry` = '13') and (`SlotId` = '10');
UPDATE `war_world`.`creature_items` SET `ModelId` = '9231' WHERE (`Entry` = '13') and (`SlotId` = '11');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60501', 'Welcome To APOCALYPSE!', '0', '1', '40', '0', '80', '', '', '0', '50', '', '', '1', '0', '', '', '0', '0', '0');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60500', 'Welcome To APOCALYPSE!', '0', '1', '40', '0', '80', 'Well, look what the portal has belched forth! Another weak fool who wants to be of use to our war! You shall prove that you are worthy of service to our lord Tchar\'zanek or you shall die trying! Go forth and speak to the Martial Berserker. He shall supply you with tools of battle, now go!', 'Go to the Undercroft in the Inevitable City. An icon will display over the Martial Beserker to help identify him as the person you must speak to in order to complete the quest.', '0', '50', '', '', '1', '0', '', '', '0', '1', '1');
UPDATE `war_world`.`quests_creature_starter` SET `Entry` = '60500' WHERE (`Entry` = '51821');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60500', '1000235');
INSERT INTO `war_world`.`quests` (`Entry`, `Name`, `Type`, `MinLevel`, `MaxLevel`, `MinRenown`, `MaxRenown`, `Description`, `Particular`, `XP`, `Gold`, `Given`, `Choice`, `ChoiceCount`, `PrevQuest`, `ProgressText`, `OnCompletionQuest`, `Repeatable`, `Active`, `Shareable`) VALUES ('60502', 'Welcome To APOCALYPSE!', '0', '1', '40', '0', '80', 'Your wargear will serve you well in battle. Now, get to the battle. Talk to the wyvern-riding greenskin. Begone!', 'Go to the Flightmaster in the Inevitable City.', '0', '2000', '', '', '1', '60500', '', '\'ere, one free ride. Don be gettin used ta dat.', '0', '1', '0');
INSERT INTO `war_world`.`quests_creature_finisher` (`Entry`, `CreatureID`) VALUES ('60502', '1008000');
INSERT INTO `war_world`.`quests_creature_starter` (`Entry`, `CreatureID`) VALUES ('60502', '1000235');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('1008000', 'Wyvern Tendin Git', '1217', '0', '50', '50', '21', '21', '129', '5', '18', '0', '0', '21', '0', '1000', '0', '0', '6510', '2', '31266949237052210', '', '15', '80', '0', '0', '0', '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008000', '11', '1031', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008000', '20', '3020', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008000', '22', '2308', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('1008000', '24', '3043', '0', '0', '0');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('1215688', '1008000', '161', '438039', '129426', '17411', '3442', '18', '0', '0', '0', '0', '0', '0', '1');

DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '10505108');
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '10505109');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505108', 'Skaven Black Market Dealer', '157', '0', '55', '55', '40', '40', '128', '5', '0', '0', '10', '0', '0', '0', '0', '0', '0', '0', '0', '', '0', '0', '0', '0', '0', '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505109', 'Skaven Black Market Dealer', '157', '0', '55', '55', '40', '40', '64', '5', '0', '0', '10', '0', '0', '0', '0', '0', '0', '0', '0', '', '0', '0', '0', '0', '0', '0', '1.00', '1.00', '0', '0', '0');




DELETE FROM `war_world`.`creature_texts` WHERE (`Creature_texts_ID` = '10505109');
INSERT INTO `war_world`.`creature_texts` (`Creature_texts_ID`, `Entry`, `Text`) VALUES ('10505109', '10505109', 'Interested in Black Market Fings? I can arrange just about any kind yer lookin\' for, if you can provide me with a specialised Mail with ya Shiny war Warlord Fing,To \"Black Market\" that binds ya gear to my Fings together...Black Market provides ya War Warlord Fing Back Back you see!');



DELETE FROM `war_world`.`creature_texts` WHERE (`Creature_texts_ID` = '10505108');
INSERT INTO `war_world`.`creature_texts` (`Creature_texts_ID`, `Entry`, `Text`) VALUES ('10505108', '10505108', 'Interested in Black Market Fings? I can arrange just about any kind yer lookin\' for, if you can provide me with a specialised Mail with ya Shiny war Warlord Fing,To \"Black Market\" that binds ya gear to my Fings together...Black Market provides ya War Warlord Fing Back Back you see!');

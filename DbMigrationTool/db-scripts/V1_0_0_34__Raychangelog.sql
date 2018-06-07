

INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('2000889', 'Sentry Champion', '1218', '0', '50', '50', '40', '40', '64', '5', '18', '0', '118', '0', '0', '0', '0', '0', '0', '0', '0', '', '16', '91', '0', '0', '65', '0', '1.00', '1.00', '0', '0', '0');


UPDATE `war_world`.`creature_protos` SET `Name`='Onslaught vendor', `VendorID`='395' WHERE `Entry`='2000889';


INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('2000888', 'Onslaught vendor', '1008', '0', '50', '50', '10', '10', '128', '5', '18', '0', '16', '45', '1', '1000', '0', '0', '50922', '2', '', '', '16', '89', '0', '0', '395', '0', '1.00', '1.00', '0', '0', '0');

/* on your knees and pierce armor damage fix
- already applied 6/3/2019
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.5', `StatDamageScale` = '0.33' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.68', `StatDamageScale` = '0.33' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.85', `StatDamageScale` = '0.33' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1.18', `StatDamageScale` = '0.33' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.33' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');

UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9421') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
*/

#Droprate increase - Droprate requirement decrease
UPDATE `war_world`.`rvr_player_gear_drop` SET `MaximumRenownRank` = '53', `DropChance` = '2500' WHERE (`ItemId` = '208453') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MaximumRenownRank` = '53', `DropChance` = '2500' WHERE (`ItemId` = '208453') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '54', `DropChance` = '2500' WHERE (`ItemId` = '208454') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '54', `DropChance` = '2500' WHERE (`ItemId` = '208454') and (`Realm` = '2');

#Droprate increase - Droprate requirement decrease of Player gear drops (Warlord/Inv)
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434912') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434913') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434914') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434915') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434916') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434917') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434918') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434919') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434920') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434921') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434922') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434923') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434924') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434925') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434926') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434927') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434928') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434929') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434930') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434931') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434932') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434933') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434934') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434935') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434984') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434985') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434986') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434987') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434988') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434989') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434990') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434991') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434992') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434993') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434994') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434995') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434996') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434997') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434998') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '434999') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '435000') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '435001') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '435002') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '435003') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '435004') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '435005') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '435006') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '40', `MaximumRenownRank` = '54', `DropChance` = '500' WHERE (`ItemId` = '435007') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434311') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435163') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435162') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435161') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435160') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435159') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435158') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435157') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435156') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435155') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55' WHERE (`ItemId` = '208454') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55' WHERE (`ItemId` = '208454') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435154') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435153') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435152') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435151') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435150') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435149') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435148') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435147') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435146') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435145') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435144') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435143') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435142') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435141') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435140') and (`Realm` = '2');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435079') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435078') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435077') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435076') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435075') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435074') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435073') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435072') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435071') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435070') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435069') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435068') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435067') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435066') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435065') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435064') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435063') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435062') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435061') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435060') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435059') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435058') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435057') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MinimumRenownRank` = '55', `DropChance` = '250' WHERE (`ItemId` = '435056') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MaximumRenownRank` = '54' WHERE (`ItemId` = '208453') and (`Realm` = '1');
UPDATE `war_world`.`rvr_player_gear_drop` SET `MaximumRenownRank` = '54' WHERE (`ItemId` = '208453') and (`Realm` = '2');


#Barricade IX & II
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = 'Career' WHERE (`Entry` = '10671');
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = 'Career' WHERE (`Entry` = '10664');

#Invader SC Currency Nr: 208436
UPDATE `war_world`.`item_infos` SET `Description` = 'An enemy general\'s insignia. These can be used to trade for powerful equipment and supplies at the appropriate Merchant in your capital city.' WHERE (`Entry` = '208436');

#Scenario Weapon Vendors
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505120', 'Darian Silverheart', '30', '0', '0', '0', '40', '40', '66', '5', '0', '0', '130', '0', '0', '0', '0', '0', '0', '0', NULL, '0', '0', '0', '0', '430', '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `ScriptName`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505121', 'Volarion the Great', '25', '0', '60', '60', '40', '40', '130', '5', '0', '0', '130', '0', '0', '0', '0', '0', '0', '0', '0', NULL, '0', '0', '0', '0', '430', '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505120', '10', '8807', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505120', '22', '8194', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505120', '21', '7188', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505120', '24', '7189', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505120', '28', '7185', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505120', '23', '7184', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505120', '27', '7190', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505120', '20', '7186', '0', '0', '0');
#^^^^p2
UPDATE `war_world`.`creature_items` SET `ModelId` = '7978' WHERE (`Entry` = '10505120') and (`SlotId` = '20');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7980' WHERE (`Entry` = '10505120') and (`SlotId` = '21');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7979' WHERE (`Entry` = '10505120') and (`SlotId` = '22');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7976' WHERE (`Entry` = '10505120') and (`SlotId` = '23');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7981' WHERE (`Entry` = '10505120') and (`SlotId` = '24');
UPDATE `war_world`.`creature_items` SET `ModelId` = '7977' WHERE (`Entry` = '10505120') and (`SlotId` = '28');
#Destru Merchant
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '10', '1950', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '20', '7942', '0', '181', '181');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '21', '7944', '0', '181', '181');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '22', '7943', '0', '181', '181');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '23', '7940', '0', '181', '181');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '24', '7945', '0', '181', '181');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '27', '8103', '0', '181', '181');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '28', '7941', '0', '181', '181');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505121', '26', '7043', '0', '0', '0');

#SC WEAPONS
#2h
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501613', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501347', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501348', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501352', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501353', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501357', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501358', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501611', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501615', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501623', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501630', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501637', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501639', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501640', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501644', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501648', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501651', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501665', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501669', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501670', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501672', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501674', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501682', '0', '(710,208436)(1230,208470)');
#1h
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501331', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501332', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501336', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501337', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501338', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501341', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501342', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501343', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501361', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501362', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501366', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501367', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501370', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501371', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501546', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501601', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501602', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501603', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501604', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501608', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501609', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501620', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501622', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501631', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501632', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501633', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501634', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501635', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501636', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501638', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501641', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501642', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501643', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501645', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501647', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501649', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501650', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501652', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501660', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501661', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501662', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501663', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501664', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501666', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501667', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501677', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501678', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501679', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501680', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501681', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501360', '0', '(355,208436)(615,208470)');

#DPS Correction
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501602');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501603');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501604');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501608');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501609');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501622');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501643');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501645');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501647');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501649');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501650');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501652');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501660');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501661');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501662');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501663');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501664');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501666');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501667');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501677');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501679');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501681');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501347');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501348');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501352');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501357');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501353');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501358');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501611');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501613');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501615');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501623');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501630');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501637');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501639');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501640');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501644');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501648');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501651');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501665');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501669');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501670');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501672');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501674');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501682');

#Stat Rework
UPDATE `war_world`.`item_infos` SET `Stats` = '8:30;5:22;77:2;32:3;81:17;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '5501660');
UPDATE `war_world`.`item_infos` SET `Name` = 'Sentry\'s Spanner of Reverence', `Stats` = '8:22;5:30;83:2;84:2;79:28;' WHERE (`Entry` = '5501601');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:30:0:0;5:22:0:0;77:2:0:0;32:3:0:0;81:17:0:0;' WHERE (`Entry` = '5501677');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `Unk27`, `TwoHanded`, `ItemSet`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501360', 'Sentry\'s Rifle of Reverence', '9', '1', '1256', '12', '3', '0', '256', '1', '0', '0', '610', '240', '40', '55', '0', '8:22;5:30;83:2;84:2;79:28;', '30', '1', '1', '56', '0', '1 1 0 0 3 4 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 ', '0', '0', '1', '0', '0', '0', '0', '0');

#Link Classes -> STEP 2
UPDATE `war_world`.`item_infos` SET `Career` = '8' WHERE (`Entry` = '5501360');
UPDATE `war_world`.`item_infos` SET `Career` = '8' WHERE (`Entry` = '5501677');
UPDATE `war_world`.`item_infos` SET `Career` = '8' WHERE (`Entry` = '5501601');
UPDATE `war_world`.`item_infos` SET `Career` = '8' WHERE (`Entry` = '5501600');

#Armor Removal of SC Weapons
UPDATE `war_world`.`item_infos` SET `Armor` = '0' WHERE (`Entry` = '5501638');
UPDATE `war_world`.`item_infos` SET `Armor` = '0' WHERE (`Entry` = '5501370');
UPDATE `war_world`.`item_infos` SET `Armor` = '0' WHERE (`Entry` = '5501361');
UPDATE `war_world`.`item_infos` SET `Armor` = '0' WHERE (`Entry` = '5501678');
UPDATE `war_world`.`item_infos` SET `Armor` = '0' WHERE (`Entry` = '5501366');
UPDATE `war_world`.`item_infos` SET `Armor` = '0' WHERE (`Entry` = '5501680');
UPDATE `war_world`.`item_infos` SET `Armor` = '0' WHERE (`Entry` = '5501676');

#DPS Speed Correction
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501630');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501637');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501639');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501347');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501348');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501352');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501357');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501353');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501358');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501640');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501644');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501648');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501651');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501611');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501613');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501615');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501682');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501623');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501665');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501669');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501670');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501672');
UPDATE `war_world`.`item_infos` SET `Dps` = '880' WHERE (`Entry` = '5501674');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501632');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501667');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501666');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501664');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501662');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501663');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501661');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501660');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501622');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501681');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501679');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501677');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501609');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501608');
UPDATE `war_world`.`item_infos` SET `Dps` = '610', `Speed` = '240' WHERE (`Entry` = '5501604');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501603');
UPDATE `war_world`.`item_infos` SET `Dps` = '610', `Speed` = '240' WHERE (`Entry` = '5501602');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501652');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501650');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501649');
UPDATE `war_world`.`item_infos` SET `Dps` = '610', `Speed` = '240' WHERE (`Entry` = '5501647');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501645');
UPDATE `war_world`.`item_infos` SET `Dps` = '610' WHERE (`Entry` = '5501643');
UPDATE `war_world`.`item_infos` SET `Dps` = '610', `Speed` = '240' WHERE (`Entry` = '5501546');

#Getting rid of Effects
UPDATE `war_world`.`item_infos` SET `Effects` = NULL WHERE (`Entry` = '5501371');
UPDATE `war_world`.`item_infos` SET `Effects` = NULL WHERE (`Entry` = '5501647');
UPDATE `war_world`.`item_infos` SET `Effects` = NULL WHERE (`Entry` = '5501650');
UPDATE `war_world`.`item_infos` SET `Effects` = NULL WHERE (`Entry` = '5501679');
UPDATE `war_world`.`item_infos` SET `Effects` = NULL WHERE (`Entry` = '5501620');
UPDATE `war_world`.`item_infos` SET `Effects` = NULL WHERE (`Entry` = '5501682');
UPDATE `war_world`.`item_infos` SET `Effects` = NULL WHERE (`Entry` = '5501623');

#Stat correction p1
UPDATE `war_world`.`item_infos` SET `Stats` = '3:30;5:22;84:1;89:2;94:21;' WHERE (`Entry` = '5501371');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:30;5:22;84:1;89:2;94:21;' WHERE (`Entry` = '5501343');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:58;5:42;32:6;78:4;79:64;' WHERE (`Entry` = '5501348');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:36;5:26;32:3;78:2;79:32;' WHERE (`Entry` = '5501367');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:29;5:21;32:3;78:2;79:32;' WHERE (`Entry` = '5501338');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:36;84:2;' WHERE (`Entry` = '5501370');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:52;83:4;84:4;' WHERE (`Entry` = '5501347');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:36;84:2;' WHERE (`Entry` = '5501361');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501337');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501331');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:52;83:4;84:4;' WHERE (`Entry` = '5501352');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501608');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:52;83:4;84:4;' WHERE (`Entry` = '5501357');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:45;5:60;79:48;83:4;84:4;' WHERE (`Entry` = '5501613');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:36;84:2;' WHERE (`Entry` = '5501678');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:36;84:2;' WHERE (`Entry` = '5501366');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501622');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:45;5:60;79:48;83:4;84:4;' WHERE (`Entry` = '5501353');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501341');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:36;84:2;' WHERE (`Entry` = '5501680');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501604');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501602');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:58;5:42;32:6;78:4;79:64;' WHERE (`Entry` = '5501611');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501362');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:52;83:4;84:4;' WHERE (`Entry` = '5501623');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501601');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501332');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501342');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501336');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501603');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:37;5:26;32:3;76:2;80:17;' WHERE (`Entry` = '5501620');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501609');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:45;5:60;79:48;83:4;84:4;' WHERE (`Entry` = '5501615');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:45;5:60;79:48;83:4;84:4;' WHERE (`Entry` = '5501358');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:31;5:20;28:2;29:2;83:1;' WHERE (`Entry` = '5501676');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:30;5:22;32:3;77:2;81:17;' WHERE (`Entry` = '5501666');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:30;5:22;32:3;77:2;81:17;' WHERE (`Entry` = '5501681');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:30;5:22;32:3;77:2;81:17;' WHERE (`Entry` = '5501677');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:30;5:22;32:3;77:2;81:17;' WHERE (`Entry` = '5501660');
UPDATE `war_world`.`item_infos` SET `Dps` = '424', `Speed` = '0', `Stats` = '4:31;5:20;28:2;29:2;83:1;' WHERE (`Entry` = '5501645');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501667');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:60;5:43;32:6;76:4;80:34;' WHERE (`Entry` = '5501644');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501664');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:60;5:43;84:2;89:4;94:42;' WHERE (`Entry` = '5501648');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:31;5:20;28:2;29:2;83:1;' WHERE (`Entry` = '5501643');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:60;5:43;32:6;76:4;80:34;' WHERE (`Entry` = '5501651');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:37;5:27;84:1;89:2;94:21;' WHERE (`Entry` = '5501650');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501642');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501641');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:60;5:43;32:6;76:4;80:34;' WHERE (`Entry` = '5501640');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:37;5:27;84:1;89:2;94:21;' WHERE (`Entry` = '5501647');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:60;5:43;32:6;76:4;80:34;' WHERE (`Entry` = '5501669');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:60;5:43;32:6;76:4;80:34;' WHERE (`Entry` = '5501639');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:60;5:43;32:6;78:4;82:34;' WHERE (`Entry` = '5501672');
UPDATE `war_world`.`item_infos` SET `Dps` = '424', `Speed` = '0', `Stats` = '4:31;5:20;28:2;29:2;83:1;' WHERE (`Entry` = '5501638');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501662');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:30;5:22;84:1;89:2;94:21;' WHERE (`Entry` = '5501652');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:60;5:43;32:6;78:4;82:34;' WHERE (`Entry` = '5501637');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501636');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501663');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501661');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:60;5:43;84:2;89:4;94:42;' WHERE (`Entry` = '5501670');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:30;5:22;32:3;77:2;81:17;' WHERE (`Entry` = '5501635');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:30;5:22;84:1;89:2;94:21;' WHERE (`Entry` = '5501546');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:60;5:43;32:6;76:4;80:34;' WHERE (`Entry` = '5501682');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:30;5:22;32:3;77:2;81:17;' WHERE (`Entry` = '5501634');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501632');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501631');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:37;5:27;84:1;89:2;94:21;' WHERE (`Entry` = '5501679');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501633');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:60;5:43;32:6;76:4;80:34;' WHERE (`Entry` = '5501665');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:60;5:43;84:2;89:4;94:42;' WHERE (`Entry` = '5501674');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:60;5:43;32:6;78:4;82:34;' WHERE (`Entry` = '5501630');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501649');

#Classes
UPDATE `war_world`.`item_infos` SET `Race` = '1' WHERE (`Entry` = '5501660');
UPDATE `war_world`.`item_infos` SET `Race` = '1' WHERE (`Entry` = '5501665');
UPDATE `war_world`.`item_infos` SET `Race` = '1' WHERE (`Entry` = '5501669');
UPDATE `war_world`.`item_infos` SET `Career` = '48' WHERE (`Entry` = '5501331');
UPDATE `war_world`.`item_infos` SET `Career` = '128' WHERE (`Entry` = '5501332');
UPDATE `war_world`.`item_infos` SET `Career` = '4096' WHERE (`Entry` = '5501336');
UPDATE `war_world`.`item_infos` SET `Career` = '12288' WHERE (`Entry` = '5501337');
UPDATE `war_world`.`item_infos` SET `Career` = '16384' WHERE (`Entry` = '5501338');
UPDATE `war_world`.`item_infos` SET `Career` = '4194304' WHERE (`Entry` = '5501341');
UPDATE `war_world`.`item_infos` SET `Career` = '4194304' WHERE (`Entry` = '5501343');
UPDATE `war_world`.`item_infos` SET `Career` = '2097152' WHERE (`Entry` = '5501342');
UPDATE `war_world`.`item_infos` SET `Career` = '48' WHERE (`Entry` = '5501347');
UPDATE `war_world`.`item_infos` SET `Career` = '16' WHERE (`Entry` = '5501361');
UPDATE `war_world`.`item_infos` SET `Career` = '8388608' WHERE (`Entry` = '5501358');
UPDATE `war_world`.`item_infos` SET `Career` = '1048576' WHERE (`Entry` = '5501357');
UPDATE `war_world`.`item_infos` SET `Career` = '1048576' WHERE (`Entry` = '5501370');
UPDATE `war_world`.`item_infos` SET `Career` = '1048576' WHERE (`Entry` = '5501645');
UPDATE `war_world`.`item_infos` SET `Career` = '3' WHERE (`Entry` = '5501667');
UPDATE `war_world`.`item_infos` SET `Career` = '131072' WHERE (`Entry` = '5501666');
UPDATE `war_world`.`item_infos` SET `Career` = '512' WHERE (`Entry` = '5501662');
UPDATE `war_world`.`item_infos` SET `Career` = '65536' WHERE (`Entry` = '5501680');
UPDATE `war_world`.`item_infos` SET `Career` = '512' WHERE (`Entry` = '5501678');
UPDATE `war_world`.`item_infos` SET `Career` = '1' WHERE (`Entry` = '5501676');
UPDATE `war_world`.`item_infos` SET `Career` = '8' WHERE (`Entry` = '5501677');
UPDATE `war_world`.`item_infos` SET `Career` = '3' WHERE (`Entry` = '5501609');
UPDATE `war_world`.`item_infos` SET `Career` = '3' WHERE (`Entry` = '5501608');
UPDATE `war_world`.`item_infos` SET `Career` = '768' WHERE (`Entry` = '5501603');
UPDATE `war_world`.`item_infos` SET `Career` = '8' WHERE (`Entry` = '5501601');
UPDATE `war_world`.`item_infos` SET `Career` = '48' WHERE (`Entry` = '5501644');
UPDATE `war_world`.`item_infos` SET `Career` = '4194304' WHERE (`Entry` = '5501371');
UPDATE `war_world`.`item_infos` SET `Career` = '16384' WHERE (`Entry` = '5501367');
UPDATE `war_world`.`item_infos` SET `Career` = '4096' WHERE (`Entry` = '5501366');
UPDATE `war_world`.`item_infos` SET `Career` = '128' WHERE (`Entry` = '5501362');
UPDATE `war_world`.`item_infos` SET `Career` = '32768' WHERE (`Entry` = '5501353');
UPDATE `war_world`.`item_infos` SET `Career` = '4096' WHERE (`Entry` = '5501352');
UPDATE `war_world`.`item_infos` SET `Career` = '64' WHERE (`Entry` = '5501348');
UPDATE `war_world`.`item_infos` SET `Career` = '65536' WHERE (`Entry` = '5501664');

#Additional Changes
UPDATE `war_world`.`item_infos` SET `Career` = '3' WHERE (`Entry` = '5501665');
UPDATE `war_world`.`item_infos` SET `Career` = '3' WHERE (`Entry` = '5501669');
UPDATE `war_world`.`item_infos` SET `Career` = '48' WHERE (`Entry` = '5501641');
UPDATE `war_world`.`item_infos` SET `Career` = '768' WHERE (`Entry` = '5501662');
UPDATE `war_world`.`item_infos` SET `Career` = '768' WHERE (`Entry` = '5501602');
UPDATE `war_world`.`item_infos` SET `Career` = '12288' WHERE (`Entry` = '5501642');
UPDATE `war_world`.`item_infos` SET `Career` = '12288' WHERE (`Entry` = '5501631');
UPDATE `war_world`.`item_infos` SET `Career` = '12288' WHERE (`Entry` = '5501336');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:37;5:27;84:1;89:2;94:21;' WHERE (`Entry` = '5501371');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:37;5:26;32:3;76:2;80:17;' WHERE (`Entry` = '5501650');

#Shields
UPDATE `war_world`.`item_infos` SET `Armor` = '424' WHERE (`Entry` = '5501361');
UPDATE `war_world`.`item_infos` SET `Armor` = '424' WHERE (`Entry` = '5501366');
UPDATE `war_world`.`item_infos` SET `Armor` = '424' WHERE (`Entry` = '5501370');
UPDATE `war_world`.`item_infos` SET `Armor` = '424' WHERE (`Entry` = '5501638');
UPDATE `war_world`.`item_infos` SET `Armor` = '424' WHERE (`Entry` = '5501645');
UPDATE `war_world`.`item_infos` SET `Armor` = '424' WHERE (`Entry` = '5501676');
UPDATE `war_world`.`item_infos` SET `Armor` = '424' WHERE (`Entry` = '5501678');
UPDATE `war_world`.`item_infos` SET `Armor` = '424' WHERE (`Entry` = '5501680');

#Stat Correction 2
UPDATE `war_world`.`item_infos` SET `Name` = 'Sentry\'s Warhammer of Reverence', `Stats` = '4:43;5:60;79:52;83:4;84:4;' WHERE (`Entry` = '5501665');

##############
#NEW#
##############
#CONQUEROR ARMOR REWORK
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434218');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434219');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434220');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434222');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434225');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434227');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434230');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434231');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434232');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434234');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434303');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434306');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434307');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434309');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434311');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434237');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434239');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434290');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434291');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434294');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434295');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434297');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434299');
UPDATE `war_world`.`item_infos` SET `Armor` = '220' WHERE (`Entry` = '434302');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '27426');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '27427');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '27542');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '27543');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434326');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434327');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434330');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434331');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434333');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434335');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434338');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434339');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434342');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434343');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434345');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434347');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434254');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434255');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434256');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434258');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434261');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434263');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434266');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434267');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434268');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434270');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434273');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '434275');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '27428');
UPDATE `war_world`.`item_infos` SET `Armor` = '247' WHERE (`Entry` = '27544');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434350');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434351');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434278');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434279');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434280');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434282');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434285');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434287');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434357');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434354');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434355');
UPDATE `war_world`.`item_infos` SET `Armor` = '275' WHERE (`Entry` = '434359');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434217');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434223');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434226');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434229');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434305');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434310');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434235');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434238');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434289');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434293');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434298');
UPDATE `war_world`.`item_infos` SET `Armor` = '440' WHERE (`Entry` = '434301');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434325');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434329');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434334');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434337');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434341');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434346');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434253');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434259');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434262');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434265');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434271');
UPDATE `war_world`.`item_infos` SET `Armor` = '495' WHERE (`Entry` = '434274');
UPDATE `war_world`.`item_infos` SET `Armor` = '550' WHERE (`Entry` = '434349');
UPDATE `war_world`.`item_infos` SET `Armor` = '550' WHERE (`Entry` = '434353');
UPDATE `war_world`.`item_infos` SET `Armor` = '550' WHERE (`Entry` = '434277');
UPDATE `war_world`.`item_infos` SET `Armor` = '550' WHERE (`Entry` = '434283');
UPDATE `war_world`.`item_infos` SET `Armor` = '550' WHERE (`Entry` = '434286');
UPDATE `war_world`.`item_infos` SET `Armor` = '550' WHERE (`Entry` = '434358');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434216');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434221');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434224');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434228');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434233');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434304');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434308');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434236');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434288');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434292');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434296');
UPDATE `war_world`.`item_infos` SET `Armor` = '661' WHERE (`Entry` = '434300');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434324');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434328');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434332');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434336');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434340');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434344');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434252');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434257');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434260');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434264');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434269');
UPDATE `war_world`.`item_infos` SET `Armor` = '744' WHERE (`Entry` = '434272');
UPDATE `war_world`.`item_infos` SET `Armor` = '827' WHERE (`Entry` = '434348');
UPDATE `war_world`.`item_infos` SET `Armor` = '827' WHERE (`Entry` = '434352');
UPDATE `war_world`.`item_infos` SET `Armor` = '827' WHERE (`Entry` = '434276');
UPDATE `war_world`.`item_infos` SET `Armor` = '827' WHERE (`Entry` = '434281');
UPDATE `war_world`.`item_infos` SET `Armor` = '827' WHERE (`Entry` = '434284');
UPDATE `war_world`.`item_infos` SET `Armor` = '827' WHERE (`Entry` = '434356');

#Boni P1
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:16,320,0|35:1,72,0|36:5,72,0|37:29,5,0|86:10389|' WHERE (`Entry` = '4408');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:16,344,0|36:5,72,0|37:86,5,0|86:10388|' WHERE (`Entry` = '4409');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:3,72,0|36:5,72,0|37:78,5,0|86:10410|' WHERE (`Entry` = '4410');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,72,0|35:6,72,0|36:4,72,0|37:77,5,0|86:10410|' WHERE (`Entry` = '4411');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:7,72,0|36:4,72,0|37:76,5,0|86:10385|' WHERE (`Entry` = '4412');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:16,344,0|35:1,72,0|36:5,72,0|37:29,5,0|38:32,3,0|' WHERE (`Entry` = '4413');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:26,260,0|36:5,72,0|37:78,5,0|86:10406|' WHERE (`Entry` = '4414');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:3,72,0|36:5,72,0|37:76,5,0|86:10391|' WHERE (`Entry` = '4415');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:16,344,0|35:1,72,0|36:5,72,0|37:29,5,0|86:10389|' WHERE (`Entry` = '4416');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,72,0|35:7,72,0|36:4,72,0|37:77,5,0|86:10410|' WHERE (`Entry` = '4417');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:26,520,0|36:4,72,0|37:76,5,0|86:10399|' WHERE (`Entry` = '4418');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:3,72,0|36:4,72,0|37:78,5,0|86:10410|' WHERE (`Entry` = '4419');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:15,344,0|35:1,72,0|36:5,72,0|37:29,5,0|86:10389|' WHERE (`Entry` = '4420');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:15,344,0|36:5,72,0|37:86,5,0|86:10388|' WHERE (`Entry` = '4421');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:3,72,0|36:4,72,0|37:78,5,0|86:10410|' WHERE (`Entry` = '4422');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,72,0|35:7,72,0|36:4,72,0|37:77,5,0|86:10410|' WHERE (`Entry` = '4423');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:15,344,0|35:1,72,0|36:5,72,0|37:29,5,0|86:10389|' WHERE (`Entry` = '4424');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:26,520,0|36:4,72,0|37:76,5,0|86:10388|' WHERE (`Entry` = '4425');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:3,72,0|36:5,72,0|37:78,5,0|86:10410|' WHERE (`Entry` = '4426');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:26,260,0|36:5,72,0|37:78,5,0|86:10406|' WHERE (`Entry` = '4427');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:15,344,0|35:1,72,0|36:5,72,0|37:29,5,0|86:10389|' WHERE (`Entry` = '4428');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:7,72,0|36:4,72,0|37:76,5,0|86:10385|' WHERE (`Entry` = '4429');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:3,72,0|36:5,72,0|37:76,5,0|86:10391|' WHERE (`Entry` = '4430');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:26,260,0|36:5,72,0|37:78,5,0|86:10406|' WHERE (`Entry` = '4431');

#Def proc Fix
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '10' WHERE (`Entry` = '28552') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '10' WHERE (`Entry` = '28552') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '10' WHERE (`Entry` = '28552') and (`CommandID` = '0') and (`CommandSequence` = '2');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '10' WHERE (`Entry` = '28552') and (`CommandID` = '0') and (`CommandSequence` = '3');

#Tank Set Boni
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,50,0|86:28452|' WHERE (`Entry` = '4408');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,50,0|86:28452|' WHERE (`Entry` = '4413');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,50,0|86:28452|' WHERE (`Entry` = '4416');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,50,0|86:28452|' WHERE (`Entry` = '4420');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,50,0|86:28452|' WHERE (`Entry` = '4424');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,50,0|86:28452|' WHERE (`Entry` = '4428');

#Choppa / SL
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:32,5,0|37:38:15,1|86:10160|' WHERE (`Entry` = '4409');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:32,5,0|37:38:15,1|86:10160|' WHERE (`Entry` = '4421');

#Mage DPS Classes
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:5,72,0|36:88,5,0|37:78,5,0|86:10386|' WHERE (`Entry` = '4410');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:5,72,0|36:88,5,0|37:78,5,0|86:10386|' WHERE (`Entry` = '4419');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:5,72,0|36:88,5,0|37:78,5,0|86:10386|' WHERE (`Entry` = '4426');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:5,72,0|36:88,5,0|37:78,5,0|86:10386|' WHERE (`Entry` = '4422');

#Engi
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,72,0|35:5,72,0|36:77,5,0|37:7,72,0|86:10862|' WHERE (`Entry` = '4411');

#Caster
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:4,72,0|36:32,5,0|37:78,5,0|86:10160|' WHERE (`Entry` = '4414');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:4,72,0|36:32,5,0|37:78,5,0|86:10160|' WHERE (`Entry` = '4427');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,72,0|35:4,72,0|36:32,5,0|37:78,5,0|86:10160|' WHERE (`Entry` = '4431');

#SW/SQ
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,72,0|35:7,72,0|36:38,15,1|37:77,5,0|86:28454|' WHERE (`Entry` = '4417');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,72,0|35:7,72,0|36:38,15,1|37:77,5,0|86:28454|' WHERE (`Entry` = '4423');

#WL/mrd
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:26,520,0|36:4,72,0|37:76,5,0|86:10399|' WHERE (`Entry` = '4418');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:26,520,0|36:4,72,0|37:76,5,0|86:10399|' WHERE (`Entry` = '4425');

#WP / DOK
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:86,5,0|37:7,76,0|86:10154|' WHERE (`Entry` = '4415');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:86,5,0|37:7,76,0|86:10154|' WHERE (`Entry` = '4430');

#WE / WH
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:7,72,0|36:6,72,0|37:76,5,0|86:10385|' WHERE (`Entry` = '4412');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:7,72,0|36:6,72,0|37:76,5,0|86:10385|' WHERE (`Entry` = '4429');

#Correction of Damage Boni
DELETE FROM `war_world`.`item_sets` WHERE (`Entry` = '3908');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:84,5,0|37:29,5,0|' WHERE (`Entry` = '4072');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:3,66,0|35:5,66,0|36:26,280,0|37:89,5,0|' WHERE (`Entry` = '4074');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:29,5,0|37:76,5,0|' WHERE (`Entry` = '4076');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,66,0|35:5,66,0|36:88,3,0|37:78,5,0|' WHERE (`Entry` = '4078');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:3,66,0|35:5,66,0|36:89,5,0|85:14187|' WHERE (`Entry` = '4079');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:84,5,0|37:29,5,0|' WHERE (`Entry` = '4080');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,66,0|35:5,66,0|36:7,66,0|37:77,5,0|' WHERE (`Entry` = '4081');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:86,5,0|37:76,5,0|' WHERE (`Entry` = '4082');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:84,5,0|37:29,5,0|' WHERE (`Entry` = '4084');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:3,66,0|35:5,66,0|36:32,5,0|37:89,5,0|' WHERE (`Entry` = '4083');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:86,5,1|37:76,5,0|' WHERE (`Entry` = '4085');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:3,66,0|35:5,66,0|36:32,5,0|37:89,5,0|' WHERE (`Entry` = '4086');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,66,0|35:5,66,0|36:7,66,0|37:77,5,0|' WHERE (`Entry` = '4087');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:84,5,0|37:29,5,0|' WHERE (`Entry` = '4088');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,66,0|35:5,66,0|36:7,66,0|37:77,5,0|' WHERE (`Entry` = '4089');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:3,66,0|35:5,66,0|36:26,280,0|37:89,5,0|' WHERE (`Entry` = '4090');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,66,0|35:5,66,0|36:88,3,0|37:78,5,0|' WHERE (`Entry` = '4091');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:84,5,0|37:29,5,0|' WHERE (`Entry` = '4092');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:29,5,0|37:76,5,0|' WHERE (`Entry` = '4093');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:3,66,0|35:5,66,0|36:89,5,0|85:14187|' WHERE (`Entry` = '4094');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:9,66,0|35:5,66,0|36:88,3,0|37:78,5,0|' WHERE (`Entry` = '4095');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:84,5,0|37:29,5,0|' WHERE (`Entry` = '4096');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,66,0|35:5,66,0|36:87,5,0|37:77,5,0|' WHERE (`Entry` = '4098');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:4,72,0|36:76,5,0|85:10389|38:24,5,1|' WHERE (`Entry` = '4385');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:76,5,0|36:29,5,0|85:10414|38:24,5,1|' WHERE (`Entry` = '4388');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:6,72,0|35:4,72,0|36:5,72,0|85:10388|38:24,5,1|' WHERE (`Entry` = '4389');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:5,76,0|35:1,76,0|36:38,24,1|85:10410|86:10752|' WHERE (`Entry` = '4541');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:5,76,0|35:1,76,0|36:38,24,1|85:10410|86:10939|' WHERE (`Entry` = '4538');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:5,76,0|35:1,76,0|36:38,24,1|85:10410|86:10752|' WHERE (`Entry` = '4545');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:40,10,0|35:5,30,0|36:80,60,0|37:38,15,0|86:10328|' WHERE (`Entry` = '6097');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:77,2,0|35:5,30,0|36:81,60,0|37:36,20,0|86:10327|' WHERE (`Entry` = '6098');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:38,7,0|35:4,30,0|36:80,60,0|85:10316|86:10332|' WHERE (`Entry` = '6099');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:86,2,0|35:1,30,0|36:29,3,0|37:76,2,1|86:10325|' WHERE (`Entry` = '6100');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:86,2,0|35:87,2,0|36:88,2,0|' WHERE (`Entry` = '3901');
#^^^P2^^^
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:3,66,0|35:5,66,0|36:83,5,0|37:89,5,0|' WHERE (`Entry` = '4074');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:3,66,0|35:5,66,0|36:83,5,0|37:89,5,0|' WHERE (`Entry` = '4090');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,66,0|35:5,66,0|36:86,5,0|37:76,5,0|' WHERE (`Entry` = '4089');

#Correction of BO Shield
UPDATE `war_world`.`item_infos` SET `Armor` = '424', `Dps` = '424', `Speed` = '0' WHERE (`Entry` = '5501643');

/*#SM WINGS REQUIRES GROUND - executed on Prod
INSERT INTO `war_world`.`ability_modifier_checks` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `ID`, `Sequence`, `CommandName`, `FailCode`, `PrimaryValue`, `SecondaryValue`, `ability_modifier_checks_ID`) VALUES ('9057', 'Wings of Heaven', '9057', 'Wings of Heaven', '0', '0', '1', 'IsGrounded', '1', '0', '0', '6c77fb67-875d-11e9-b8e1-5a000199677e');
*/
#Removal of unnecessary Willpower on non-healer sets
UPDATE `war_world`.`item_infos` SET `Stats` = '5:32;9:20;5:7;31:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434338');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:19;9:28;5:20;32:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434350');
UPDATE `war_world`.`item_infos` SET `Stats` = '5:14;7:10;9:29;78:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434290');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:27;5:8;4:15;31:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434302');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:27;4:16;7:14;76:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434223');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:27;7:15;4:8;29:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434235');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:32;5:18;7:12;83:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434259');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:35;5:19;4:13;32:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434283');
UPDATE `war_world`.`item_infos` SET `Stats` = '5:14;7:10;9:29;78:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434227');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:27;5:8;4:15;31:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434239');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:32;4:18;5:12;83:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434326');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:32;4:18;5:12;83:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434263');
UPDATE `war_world`.`item_infos` SET `Stats` = '5:32;9:20;5:7;31:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434275');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:19;9:28;5:20;32:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434287');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:27;4:16;7:14;76:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434298');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:27;7:15;4:8;29:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434310');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:35;5:19;4:13;32:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434358');
UPDATE `war_world`.`item_infos` SET `Stats` = '1:32;5:18;7:12;83:2;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '434334');

#Keep Lord Difficulty
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '2551');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '2742');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '2761');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '2771');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '2778');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '2781');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '2795');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '4642');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '5247');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '7111');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '24499');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '32678');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '32786');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '34537');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '34740');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '35252');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '35657');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '36340');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '37017');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '37489');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '37651');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '37763');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '37764');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '38538');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '706');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1679');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1889');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '3145');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '5625');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '5632');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '34735');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '40406');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '43179');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '46407');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '46561');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '46637');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '46760');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '47398');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000080');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000081');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000092');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000102');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000103');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000112');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '43374');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000008');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '778201');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '778200');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '778162');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '778140');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '778138');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000009');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000046');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000047');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000057');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000058');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000065');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000066');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000093');
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '43', `MaxLevel` = '43', `PowerModifier` = '1.2', `WoundsModifier` = '2.00' WHERE (`Entry` = '1000111');

#Keep Lord Ability
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '-10' WHERE (`Entry` = '14879') and (`CommandID` = '0') and (`CommandSequence` = '1');

#Update for SC weapon (chosen sword)
UPDATE `war_world`.`item_infos` SET `Career` = '4096', `Skills` = '1' WHERE (`Entry` = '5501631');
UPDATE `war_world`.`item_infos` SET `Career` = '4096' WHERE (`Entry` = '5501336');

#Shield Addition
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `Unk27`, `TwoHanded`, `ItemSet`, `Craftresult`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501359', 'Sentry\'s Defender of Reverence', '', '5', '1', '1309', '11', '4', '1', '16', '1', '424', '0', '424', '0', '40', '55', '0', '6:21;5:31;32:2;79:36;84:2;', '30', '1', '1', '56', '0', '1 1 0 0 3 4 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 ', '0', '0', '0', '0', '1', '0', '0', '0', '0', '0');
UPDATE `war_world`.`item_infos` SET `ObjectLevel` = '56', `DyeAble` = '0' WHERE (`Entry` = '5501645');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `Unk27`, `TwoHanded`, `ItemSet`, `Craftresult`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501321', 'Shieldbearer\'s Heater of Reverence', '', '5', '32', '4787', '11', '3', '512', '16', '1', '424', '0', '424', '0', '40', '55', '0', '4:31;5:20;28:2;29:2;83:1;', '30', '1', '1', '56', '0', '1 1 0 0 3 4 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 ', '0', '0', '0', '0', '1', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `Unk27`, `TwoHanded`, `ItemSet`, `Craftresult`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501368', 'Sentry\'s Pavise of Reverence', '5', '8', '3287', '11', '3', '65536', '16', '1', '424', '0', '424', '0', '40', '55', '0', '6:21;5:31;32:2;79:36;84:2;', '30', '1', '1', '56', '0', '1 1 0 0 3 4 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 ', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '0' WHERE (`Entry` = '5501638');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '0' WHERE (`Entry` = '5501643');

#Shield Addition to Merchant
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501359', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501321', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501368', '0', '(355,208436)(615,208470)');

#BG Weapons
UPDATE `war_world`.`item_infos` SET `Career` = '1048576', `Stats` = '1:30;5:22;32:3;76:2;80:17;' WHERE (`Entry` = '5501652');
UPDATE `war_world`.`item_infos` SET `Name` = 'Sentry\'s Painblade of Reverence', `Career` = '1048576', `Stats` = '4:22;5:30;79:28;83:2;84:2;' WHERE (`Entry` = '5501636');

#Dwarf 2h Skin fix
UPDATE `war_world`.`item_infos` SET `ModelId` = '5520' WHERE (`Entry` = '5501665');

#2h Kotbs
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `Unk27`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501306', 'Titan\'s Claymore of Reverence', '1', '32', '8822', '10', '4', '512', '2097153', '1', '0', '0', '880', '340', '40', '55', '0', '1:60;5:43;32:6;76:4;80:34;', '30', '2', '1', '56', '1 1 0 0 3 4 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 ', '1', '0', '1', '1', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `Unk27`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501349', 'Sentry\'s Claymore of Reverence', '1', '32', '8822', '10', '4', '512', '2097153', '1', '0', '0', '880', '340', '40', '55', '0', '4:43;5:60;79:52;83:4;84:4;', '30', '2', '1', '56', '1 1 0 0 3 4 2 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 ', '1', '0', '1', '1', '0', '0', '0', '0', '0');

#Kotbs Merchant 
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501306', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501349', '0', '(710,208436)(1230,208470)');

#DPS AM Correction
UPDATE `war_world`.`item_infos` SET `Stats` = '9:58;5:42;32:6;78:4;79:16;' WHERE (`Entry` = '5501615');

#IB Shield
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501676', '0', '(355,208436)(615,208470)');

#WP Correction
UPDATE `war_world`.`item_infos` SET `Dps` = '0', `Speed` = '0' WHERE (`Entry` = '5501679');
UPDATE `war_world`.`item_infos` SET `Stats` = '3:60;5:45;84:2;89:4;94:42;' WHERE (`Entry` = '5501623');
UPDATE `war_world`.`item_infos` SET `Name` = 'Apostle\'s Sledge of Reverence' WHERE (`Entry` = '5501623');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501335', 'Apostle\'s Battlehammer of Reverence', '3', '32', '1464', '10', '3', '2048', '4', '1', '0', '0', '610', '240', '40', '55', '0', '3:30;5:22;84:1;89:2;94:21;', '30', '1', '1', '55', '0', '0', '0', '0', '1', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501292', 'Titan\'s Battlehammer of Reverence', '3', '32', '1465', '10', '3', '2048', '4', '1', '0', '0', '610', '240', '40', '55', '0', '1:30;5:22;32:3;76:2;80:17;', '30', '1', '1', '55', '0', '0', '0', '0', '1', '0', '0', '0', '0', '0');

#WP Merchant 
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501335', '0', '(355,208436)(615,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501292', '0', '(355,208436)(615,208470)');

#WL Weapons
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501313', 'Titan\'s Greataxe of Reverence', '2', '8', '8998', '10', '4', '262144', '2097154', '1', '0', '0', '880', '340', '40', '55', '0', '1:60;5:43;32:6;76:4;80:34;', '30', '2', '1', '55', '0', '1', '0', '1', '1', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501356', 'Sentry\'s Greataxe of Reverence', '2', '8', '8999', '10', '4', '262144', '2097154', '1', '0', '0', '880', '340', '40', '55', '0', '4:43;5:60;79:52;83:4;84:4;', '30', '2', '1', '55', '0', '1', '0', '1', '1', '0', '0', '0', '0', '0');
#WL merchant
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501313', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501356', '0', '(710,208436)(1230,208470)');

#SM Correction
UPDATE `war_world`.`item_infos` SET `Name` = 'Shieldbearer\'s Pavise of Reverence' WHERE (`Entry` = '5501368');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:31;5:20;28:2;29:2;83:1;' WHERE (`Entry` = '5501368');

#SM Weapon
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `SellRequiredItems`, `TwoHanded`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501311', 'Titan\'s Masterblade of Reverence', '1', '8', '9021', '10', '4', '65536', '1', '1', '0', '0', '880', '340', '40', '55', '0', '1:60;5:43;32:6;76:4;80:34;', '30', '1', '1', '56', '0', NULL, '1', '1', '1', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501354', 'Sentry\'s Masterblade of Reverence', '1', '8', '9022', '10', '4', '65536', '1', '1', '0', '0', '880', '340', '40', '55', '0', '4:43;5:60;79:52;83:4;84:4;', '30', '1', '1', '56', '0', '1', '1', '1', '0', '0', '0', '0', '0');
#SM Merchant
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501311', '0', '(710,208436)(1230,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501354', '0', '(710,208436)(1230,208470)');

#SM Weapons 1h Fix
UPDATE `war_world`.`item_infos` SET `Career` = '196608' WHERE (`Entry` = '5501664');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('5501339', 'Sentry\'\'s Blade of Reverence', '11', '8', '3304', '10', '4', '196608', '1', '1', '0', '0', '610', '240', '40', '55', '0', '4:22;5:30;79:28;83:2;84:2;', '30', '1', '1', '56', '0', '0', '0', '1', '1', '0', '0', '0', '0', '0');
UPDATE `war_world`.`item_infos` SET `TalismanSlots` = '2' WHERE (`Entry` = '5501354');
UPDATE `war_world`.`item_infos` SET `TalismanSlots` = '2' WHERE (`Entry` = '5501311');

#SM Def 1h Merchant
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '430', '5501339', '0', '(355,208436)(615,208470)');

#HP Regen Correction
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501331');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501336');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501337');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501339');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501341');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501342');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501602');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501603');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501604');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501608');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501609');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501636');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:13;83:4;84:4;' WHERE (`Entry` = '5501347');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:13;83:4;84:4;' WHERE (`Entry` = '5501349');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:13;83:4;84:4;' WHERE (`Entry` = '5501352');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:13;83:4;84:4;' WHERE (`Entry` = '5501354');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:13;83:4;84:4;' WHERE (`Entry` = '5501356');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:13;83:4;84:4;' WHERE (`Entry` = '5501357');
UPDATE `war_world`.`item_infos` SET `Stats` = '4:43;5:60;79:13;83:4;84:4;' WHERE (`Entry` = '5501665');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501332');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501362');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501601');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;79:7;83:2;84:2;' WHERE (`Entry` = '5501622');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:45;5:60;79:12;83:4;84:4;' WHERE (`Entry` = '5501353');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:45;5:60;79:12;83:4;84:4;' WHERE (`Entry` = '5501358');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:45;5:60;79:12;83:4;84:4;' WHERE (`Entry` = '5501613');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:58;5:42;32:6;78:4;79:16;' WHERE (`Entry` = '5501611');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:9;84:2;' WHERE (`Entry` = '5501359');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:9;84:2;' WHERE (`Entry` = '5501361');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:9;84:2;' WHERE (`Entry` = '5501366');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:9;84:2;' WHERE (`Entry` = '5501370');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:9;84:2;' WHERE (`Entry` = '5501678');
UPDATE `war_world`.`item_infos` SET `Stats` = '6:21;5:31;32:2;79:9;84:2;' WHERE (`Entry` = '5501680');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:29;5:21;32:3;78:2;79:8;' WHERE (`Entry` = '5501338');
UPDATE `war_world`.`item_infos` SET `Stats` = '9:36;5:26;32:3;78:2;79:8;' WHERE (`Entry` = '5501367');
UPDATE `war_world`.`item_infos` SET `Stats` = '8:22;5:30;83:2;84:2;79:7;' WHERE (`Entry` = '5501360');

#Conq to merchant
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434216', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434217', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434218', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434219', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434220', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434221', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434222', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434223', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434224', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434225', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434226', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434227', '0', '(90,208436)(300,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434228', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434229', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434230', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434231', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434232', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434233', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434234', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434235', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434236', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434237', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434238', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434239', '0', '(90,208436)(300,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434252', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434253', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434254', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434255', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434256', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434257', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434258', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434259', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434260', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434261', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434262', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434263', '0', '(180,208436)(500,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434264', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434265', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434266', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434267', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434268', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434269', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434270', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434271', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434272', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434273', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434274', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434275', '0', '(240,208436)(650,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434276', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434277', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434278', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434279', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434280', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434281', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434282', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434283', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434284', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434285', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434286', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434287', '0', '(300,208436)(800,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434240', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434241', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434242', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434243', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434244', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434245', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434246', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434247', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434248', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434249', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434250', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '431', '434251', '0', '(180,208436)(400,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434288', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434289', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434290', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434291', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434292', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434293', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434294', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434295', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434296', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434297', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434298', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434299', '0', '(90,208436)(300,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434300', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434301', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434302', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434303', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434304', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434305', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434306', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434307', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434308', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434309', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434310', '0', '(90,208436)(300,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434311', '0', '(90,208436)(300,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434324', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434325', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434326', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434327', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434328', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434329', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434330', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434331', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434332', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434333', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434334', '0', '(180,208436)(500,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434335', '0', '(180,208436)(500,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434336', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434337', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434338', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434339', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434340', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434341', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434342', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434343', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434344', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434345', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434346', '0', '(240,208436)(650,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434347', '0', '(240,208436)(650,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434348', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434349', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434350', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434351', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434352', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434353', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434354', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434355', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434356', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434357', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434358', '0', '(300,208436)(800,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434359', '0', '(300,208436)(800,208470)');

INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434312', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434313', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434314', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434315', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434316', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434317', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434318', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434319', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434320', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434321', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434322', '0', '(180,208436)(400,208470)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '432', '434323', '0', '(180,208436)(400,208470)');

#Conq Merchant
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '10', '4552', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '12', '1113', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '20', '3134', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '21', '3130', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '22', '3129', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '23', '2963', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '24', '2274', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '28', '3127', '0', '0', '0');

INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505130', 'Vookz the Conqueror', '1214', '0', '0', '0', '40', '40', '66', '5', '0', '0', '130', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '431', '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505131', 'Scrab da Bestest', '1217', '0', '0', '0', '40', '40', '130', '5', '0', '0', '130', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '432', '0', '1.00', '1.00', '0', '0', '0');

INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505130', '10', '4957', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505130', '20', '2001', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505130', '21', '2003', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505130', '22', '2002', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505130', '24', '2004', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505130', '25', '3199', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505130', '12', '1274', '0', '0', '0');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505130', '28', '2000', '0', '0', '0');

#Conq & Sc weapon Renown // Rarity // Salvagability // RR and so on
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434216');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434217');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434218');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434220');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434219');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434221');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434222');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434223');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434224');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434225');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434226');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434227');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434228');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434229');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434230');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434231');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434232');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434233');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434234');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434235');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434236');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434237');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434238');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434239');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434252');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434253');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434254');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434255');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434256');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434257');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434259');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434260');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434258');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434261');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434262');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434263');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434264');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434265');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434266');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434267');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434268');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434269');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434270');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434271');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434272');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434273');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434274');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434275');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434276');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434277');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434278');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434279');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434280');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434281');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434282');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434283');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434284');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434285');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434286');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434287');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434288');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434289');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434290');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434291');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434292');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434293');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434294');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434295');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434296');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434297');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434359');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434358');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434357');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434356');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434355');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434354');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434353');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434352');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434351');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434350');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434349');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '45' WHERE (`Entry` = '434348');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434347');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434346');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434345');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434344');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434343');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434342');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434341');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434340');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434339');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434338');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434337');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '44' WHERE (`Entry` = '434336');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434335');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434334');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434333');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434332');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434331');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434330');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434329');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434328');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434327');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434326');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434325');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '43' WHERE (`Entry` = '434324');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434311');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434310');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434309');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434308');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434307');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434306');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434305');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434304');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434303');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434302');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434301');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '41' WHERE (`Entry` = '434300');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434299');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '40' WHERE (`Entry` = '434298');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434240');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434241');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434242');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434243');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434244');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434245');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434246');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434247');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434248');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434249');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434250');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434251');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434312');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434313');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434314');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434315');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434316');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434317');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434318');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434319');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434320');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434321');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434322');
UPDATE `war_world`.`item_infos` SET `MinRank` = '40', `MinRenown` = '42' WHERE (`Entry` = '434323');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4', `DyeAble` = '1' WHERE (`Entry` = '5501292');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4', `DyeAble` = '1' WHERE (`Entry` = '5501335');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4', `DyeAble` = '1' WHERE (`Entry` = '5501360');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4' WHERE (`Entry` = '5501623');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4' WHERE (`Entry` = '5501660');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4' WHERE (`Entry` = '5501665');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4', `DyeAble` = '1' WHERE (`Entry` = '5501677');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4' WHERE (`Entry` = '5501682');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1' WHERE (`Entry` = '5501371');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1' WHERE (`Entry` = '5501367');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501362');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501358');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501357');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501353');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501352');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501347');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501343');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501342');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501341');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501338');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501337');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501336');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501331');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '1', `Salvageable` = '1' WHERE (`Entry` = '5501332');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4' WHERE (`Entry` = '5501321');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4', `Salvageable` = '1' WHERE (`Entry` = '5501368');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4', `DyeAble` = '0' WHERE (`Entry` = '5501676');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4', `DyeAble` = '0', `Salvageable` = '1' WHERE (`Entry` = '5501678');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4', `DyeAble` = '0', `Salvageable` = '1' WHERE (`Entry` = '5501680');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '0', `Salvageable` = '1' WHERE (`Entry` = '5501361');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '0', `Salvageable` = '1' WHERE (`Entry` = '5501366');
UPDATE `war_world`.`item_infos` SET `DyeAble` = '0', `Salvageable` = '1' WHERE (`Entry` = '5501370');

#Removal of Conq from RvR drops
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434216') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434217') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434218') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434219') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434220') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434221') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434222') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434223') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434224') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434225') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434226') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434227') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434228') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434229') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434230') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434231') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434232') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434233') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434234') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434235') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434236') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434237') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434238') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434239') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434288') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434289') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434290') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434291') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434292') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434293') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434294') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434295') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434296') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434297') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434298') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434299') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434300') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434301') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434302') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434303') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434304') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434305') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434306') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434307') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434308') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434309') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434310') and (`Realm` = '2');

#Removal of Annihilator from RvR Drops
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434096') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434097') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434098') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434099') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434100') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434101') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434102') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434103') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434104') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434105') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434106') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434107') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434108') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434109') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434110') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434111') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434112') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434113') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434114') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434115') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434116') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434117') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434118') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434119') and (`Realm` = '1');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434156') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434157') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434158') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434159') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434160') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434161') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434162') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434163') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434164') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434165') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434166') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434167') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434168') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434169') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434170') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434171') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434172') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434173') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434174') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434175') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434176') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434177') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434178') and (`Realm` = '2');
DELETE FROM `war_world`.`rvr_player_gear_drop` WHERE (`ItemId` = '434179') and (`Realm` = '2');

#Trade Merchant - Currency Items
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('1298378528', 'War Crest Box', 'Right Click to receive 5 War Crests!', '0', '0', '1566', '0', '5', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('1298378529', 'Warlord Crest Chest', 'Right Click to receive 5 Warlord Crests!', '0', '0', '1566', '0', '5', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('1298378530', 'War Crest Chest', 'Right Click to receive 25 War Crests!', '0', '0', '1566', '0', '5', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('1298378531', 'Invader\'s Crest Box', 'Right Click to receive 5 Invader\'s Crests!', '0', '0', '1566', '0', '5', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('1298378532', 'Invader\'s Crest Chest', 'Right Click to receive 25 Invader\'s Crests!', '0', '0', '1566', '0', '5', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('1298378533', 'War Crest Box', 'Right Click to receive 5 War Crests!', '0', '0', '1566', '0', '5', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `TwoHanded`, `ItemSet`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`, `IsSiege`) VALUES ('1298378534', 'War Crest Chest', 'Right Click to receive 25 War Crests!', '0', '0', '1566', '0', '5', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
#Trade Items to Vendor
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '440', '208454', '0', '(15,208453)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '440', '1298378529', '0', '(75,208453)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '440', '1298378528', '0', '(1,208453)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '440', '1298378533', '0', '(1,208436)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '440', '1298378530', '0', '(5,208453)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '440', '1298378534', '0', '(25,208436)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '440', '1298378531', '0', '(1,208454)');
INSERT INTO `war_world`.`vendor_items` (`ItemGuid`, `VendorId`, `ItemId`, `Price`, `ReqItems`) VALUES ('0', '440', '1298378532', '0', '(5,208454)');

#Vendor Setup
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505140', 'Skaven Trademaster', '1953', '0', '0', '0', '40', '40', '66', '5', '0', '0', '10', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '440', '0', '1.00', '1.00', '0', '0', '0');
INSERT INTO `war_world`.`creature_protos` (`Entry`, `Name`, `Model1`, `Model2`, `MinScale`, `MaxScale`, `MinLevel`, `MaxLevel`, `Faction`, `Ranged`, `Icone`, `Emote`, `Title`, `Unk`, `Unk1`, `Unk2`, `Unk3`, `Unk4`, `Unk5`, `Unk6`, `Flag`, `CreatureType`, `CreatureSubType`, `TokUnlock`, `LairBoss`, `VendorID`, `Career`, `PowerModifier`, `WoundsModifier`, `Invulnerable`, `WeaponDPS`, `ImmuneToCC`) VALUES ('10505141', 'Skaven Trademaster', '1953', '0', '0', '0', '40', '40', '130', '5', '0', '0', '10', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '440', '0', '1.00', '1.00', '0', '0', '0');

#Currency Stacks
UPDATE `war_world`.`item_infos` SET `MaxStack` = '1000' WHERE (`Entry` = '208436');

#Currency Fix
UPDATE `war_world`.`item_infos` SET `Bind` = '1', `Unk27` = '1 0 0 0 3 6 0 8 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 ' WHERE (`Entry` = '208436');

#Conq Percentage Fix
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,72,0|35:7,72,0|36:38,15,0|37:77,5,0|86:28454|' WHERE (`Entry` = '4423');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:8,72,0|35:7,72,0|36:38,15,0|37:77,5,0|86:28454|' WHERE (`Entry` = '4417');
#BugFix!
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:32,5,0|37:38,15,0|86:10160|' WHERE (`Entry` = '4409');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:32,5,0|37:38,15,0|86:10160|' WHERE (`Entry` = '4421');

#Conq Healregen lowerage
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,40,0|86:28452|' WHERE (`Entry` = '4408');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,40,0|86:28452|' WHERE (`Entry` = '4413');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,40,0|86:28452|' WHERE (`Entry` = '4416');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,40,0|86:28452|' WHERE (`Entry` = '4420');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,40,0|86:28452|' WHERE (`Entry` = '4424');
UPDATE `war_world`.`item_sets` SET `BonusString` = '34:1,72,0|35:5,72,0|36:28,5,0|37:79,40,0|86:28452|' WHERE (`Entry` = '4428');

#Cosmetics for the Git Vendor
UPDATE `war_world`.`creature_items` SET `ModelId` = '3236' WHERE (`Entry` = '10505131') and (`SlotId` = '23');
INSERT INTO `war_world`.`creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) VALUES ('10505131', '27', '8118', '0', '0', '0');

#Bugfixes from Sheet
UPDATE `war_world`.`item_infos` SET `ModelId` = '8283' WHERE (`Entry` = '5501633');
UPDATE `war_world`.`item_infos` SET `Type` = '1' WHERE (`Entry` = '5501339');
UPDATE `war_world`.`item_infos` SET `SlotId` = '10' WHERE (`Entry` = '5501664');
UPDATE `war_world`.`item_infos` SET `SlotId` = '10' WHERE (`Entry` = '5501666');
UPDATE `war_world`.`item_infos` SET `Rarity` = '4' WHERE (`Entry` = '5501681');
UPDATE `war_world`.`item_infos` SET `SlotId` = '11', `Skills` = '16' WHERE (`Entry` = '5501638');
UPDATE `war_world`.`item_infos` SET `SlotId` = '10', `Skills` = '4' WHERE (`Entry` = '5501601');
UPDATE `war_world`.`item_infos` SET `SlotId` = '10' WHERE (`Entry` = '5501660');
UPDATE `war_world`.`item_infos` SET `SlotId` = '10' WHERE (`Entry` = '5501634');
UPDATE `war_world`.`item_infos` SET `SlotId` = '12' WHERE (`Entry` = '5501635');

#Remove Conq from existing merchant
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434216');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434217');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434218');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434219');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434220');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434221');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434222');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434223');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434224');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434225');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434226');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434227');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434228');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434229');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434230');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434231');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434232');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434233');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434234');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434235');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434236');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434237');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434238');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434239');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434240');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434241');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434242');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434243');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434244');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434245');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434246');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434247');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434248');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434249');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434250');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434251');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434252');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434253');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434254');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434255');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434256');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434257');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434258');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434259');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434260');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434261');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434262');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434263');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434264');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434265');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434266');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434267');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434268');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434269');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434270');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434271');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434272');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434273');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434274');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434275');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434276');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434277');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434278');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434279');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434280');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434281');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434282');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434283');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434284');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434285');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434286');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434287');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434288');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434289');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434290');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434291');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434292');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434293');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434294');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434295');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434296');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434297');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434298');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434299');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434300');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434301');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434302');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434303');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434304');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434305');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434306');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434307');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434308');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434309');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434310');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434311');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434312');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434313');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434314');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434315');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434316');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434317');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434318');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434319');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434320');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434321');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434322');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434323');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434324');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434325');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434326');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434327');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434328');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434329');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434330');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434331');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434332');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434333');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434334');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434335');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434336');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434337');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434338');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434339');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434340');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434341');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434342');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434343');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434344');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434345');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434346');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434347');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434348');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434349');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434350');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434351');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434352');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434353');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434354');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434355');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434356');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434357');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434358');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '131') and (`ItemId` = '434359');

#NPC SPAWNS
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7099879', '10505120', '162', '118998', '147552', '13322', '	4078', '0', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7099884', '10505121', '161', '442240', '151362', '17363', '850', '0', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7099885', '10505131', '161', '442339', '151438', '17387', '4020', '0', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7099880', '10505130', '162', '118632', '147546', '13322', '4090', '0', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7099881', '10505140', '162', '118585', '146859', '13482', '2034', '0', '0', '0', '0', '0', '0', '0', '1');
INSERT INTO `war_world`.`creature_spawns` (`Guid`, `Entry`, `ZoneId`, `WorldX`, `WorldY`, `WorldZ`, `WorldO`, `Icone`, `Emote`, `Faction`, `WaypointType`, `Level`, `Oid`, `RespawnMinutes`, `Enabled`) VALUES ('7099882', '10505141', '161', '440550', '152620', '17378', '2132', '0', '0', '0', '0', '0', '0', '0', '1');

#6th piece boni
#Tank
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '7' WHERE (`Entry` = '28552') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '7' WHERE (`Entry` = '28552') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '7' WHERE (`Entry` = '28552') and (`CommandID` = '0') and (`CommandSequence` = '2');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '7' WHERE (`Entry` = '28552') and (`CommandID` = '0') and (`CommandSequence` = '3');
#Engi
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`, `EventIDString`, `EventChance`, `RetriggerInterval`, `BuffLine`, `NoAutoUse`) VALUES ('10862', 'Move Faster!', '0', '0', 'InvokeBuff', '10867', 'Host', 'DirectDamageReceived', '7', '5000', '1', '0');
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('10867', 'Move Faster! Proc', '0', '0', 'ModifySpeed', '40', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxCopies`, `MaxStack`, `PersistsOnDeath`) VALUES ('10862', 'Move Faster!', 'Career', '1', '1', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `MaxCopies`, `MaxStack`, `Duration`, `Silent`) VALUES ('10867', 'Move Faster! Proc', '1', '1', '5', '1');


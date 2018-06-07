UPDATE `war_world`.`item_infos` SET `ModelId`='1913' WHERE `Entry`='5501638';

UPDATE `war_world`.`item_infos` SET `ModelId`='3353' WHERE `Entry`='5501639';

UPDATE `war_world`.`item_infos` SET `ModelId`='1926' WHERE `Entry`='5501640';

INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `Crafts`, `Unk27`, `SellRequiredItems`, `TwoHanded`, `ItemSet`, `Craftresult`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`) VALUES ('5501642', 'Titan\'s Chaosaxe of Reverence', '2', '64', '1920', '13', '4', '8192', '0', '1', '0', '0', '610', '240', '40', '55', '0', '1:30;7:22;76:2;32:3;80:15;', '30', '1', '1', '56', '1', '0', '0', '0', '0', '0', '0', '1', '1', '0', '0', '0', '0', '0');

INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ObjectLevel`, `UniqueEquiped`, `Unk27`, `SellRequiredItems`, `TwoHanded`, `ItemSet`, `Craftresult`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `Effects`, `TokUnlock2`) VALUES ('5501643', 'Titan\'s Blocka of Reverence', '5', '2', '1119', '11', '4', '16', '0', '1', '0', '0', '640', '240', '40', '55', '0', '1:30;4:22;28:3;47:500;33:3;', '30', '1', '1', '56', '1', '0', '0', '0', '0', '0', '1', '1', '0', '0', '0', '0', '0');


UPDATE `war_world`.`item_infos` SET `Crafts`=NULL WHERE `Entry`='5501329';

UPDATE `war_world`.`item_infos` SET `Crafts`=NULL, `Unk27`=NULL WHERE `Entry`='5501631';

UPDATE `war_world`.`item_infos` SET `Crafts`=NULL, `Unk27`=NULL WHERE `Entry`='5501642';

UPDATE `war_world`.`item_infos` SET `Unk27`=NULL WHERE `Entry`='5501633';

UPDATE `war_world`.`item_infos` SET `Unk27`=NULL WHERE `Entry`='5501640';
UPDATE `war_world`.`item_infos` SET `Unk27`=NULL WHERE `Entry`='5501641';

UPDATE `war_world`.`item_infos` SET `Unk27`=NULL WHERE `Entry`='5501643';
UPDATE `war_world`.`item_infos` SET `SellRequiredItems`=NULL WHERE `Entry`='5501329';

UPDATE `war_world`.`item_infos` SET `SellRequiredItems`=NULL, `Effects`=NULL WHERE `Entry`='5501631';

UPDATE `war_world`.`item_infos` SET `SellRequiredItems`=NULL, `Craftresult`=NULL, `Effects`=NULL WHERE `Entry`='5501642';

UPDATE `war_world`.`item_infos` SET `SellRequiredItems`=NULL, `Craftresult`=NULL, `Effects`=NULL WHERE `Entry`='5501643';

UPDATE `war_world`.`item_infos` SET `ItemSet`='0' WHERE `Entry`='5501632';

UPDATE `war_world`.`item_infos` SET `ItemSet`='0' WHERE `Entry`='5501633';
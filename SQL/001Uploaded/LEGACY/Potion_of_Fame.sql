delete from `war_world`.`buff_commands` where entry =  15060;
delete from `war_world`.`buff_infos` where entry =  15060;
delete from `war_world`.`item_infos` where entry =  2587;
delete from `war_world`.`abilities` where entry =  15060;
delete from `war_world`.`ability_commands` where entry =  15060;


INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `SecondaryValue`, `InvokeOn`, `Target`, `BuffLine`) VALUES ('15060', 'Fleeting Renown Boost', '0', '0', 'ModifyStat', '71', '25', '5', 'Host', '1');
INSERT INTO `war_world`.`buff_infos` (`Entry`, `Name`, `BuffClassString`, `MaxStack`, `Duration`, `PersistsOnDeath`, `CanRefresh`) VALUES ('15060', 'Fleeting Renown Boost', 'Persist', '1', '604800', '5', '1');
INSERT INTO `war_world`.`item_infos` (`Entry`, `Name`, `Description`, `Type`, `Race`, `ModelId`, `SlotId`, `Rarity`, `Career`, `Skills`, `Bind`, `Armor`, `SpellId`, `Dps`, `Speed`, `MinRank`, `MinRenown`, `StartQuest`, `Stats`, `SellPrice`, `TalismanSlots`, `MaxStack`, `ScriptName`, `ObjectLevel`, `UniqueEquiped`, `Crafts`, `Unk27`, `SellRequiredItems`, `TwoHanded`, `ItemSet`, `Craftresult`, `DyeAble`, `Salvageable`, `BaseColor1`, `BaseColor2`, `TokUnlock`, `TokUnlock2`) VALUES ('2587', 'Potion of Fame', 'Increases renown gained by 25% for 7 days.', '31', '0', '4682', '0', '4', '0', '0', '1', '0', '15060', '0', '0', '16', '0', '0', '0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;', '0', '0', '20', '', '40', '0', '', '0 1 0 1 3 2 0 0 0 0 0 0 0 0 0 0 0 0 0 4 0 0 0 0 0 0 0', '', '0', '0', '', '0', '0', '0', '0', '0', '0');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `Cooldown`, `EffectID`, `Specline`, `WeaponNeeded`, `IgnoreGlobalCooldown`) VALUES ('15060', '0', 'Fleeting renown boost', '300', '2751', 'Item', '0', '1');
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('15060', 'Fleeting renown boost', '0', '0', 'InvokeBuff', '15060', 'Caster');


#BG FoF
UPDATE `war_world`.`buff_commands` SET `BuffClassString` = NULL WHERE (`Entry` = '9346') and (`CommandID` = '0') and (`CommandSequence` = '0');

#Kotbs Shatter Confidence
UPDATE `war_world`.`abilities` SET `Range` = '5', `AIRange` = '5' WHERE (`Entry` = '8023');
UPDATE `war_world`.`abilities` SET `Range` = '5', `AIRange` = '5' WHERE (`Entry` = '8049');

#kotbs aura
UPDATE `war_world`.`buff_commands` SET `TertiaryValue` = '-189' WHERE (`Entry` = '8008') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `TertiaryValue` = '-189' WHERE (`Entry` = '8008') and (`CommandID` = '1') and (`CommandSequence` = '1');
UPDATE `war_world`.`buff_commands` SET `TertiaryValue` = '-189' WHERE (`Entry` = '8008') and (`CommandID` = '1') and (`CommandSequence` = '2');

#chosen aura
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '-4', `TertiaryValue` = '-189' WHERE (`Entry` = '8321') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '-4', `TertiaryValue` = '-189' WHERE (`Entry` = '8321') and (`CommandID` = '1') and (`CommandSequence` = '1');
UPDATE `war_world`.`buff_commands` SET `SecondaryValue` = '-4', `TertiaryValue` = '-189' WHERE (`Entry` = '8321') and (`CommandID` = '1') and (`CommandSequence` = '2');

#Bathing in Blood
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = 'Tactic' WHERE (`Entry` = '3260');
UPDATE `war_world`.`buff_infos` SET `TypeString` = 'Enchantment' WHERE (`Entry` = '3260');
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL, `TypeString` = NULL WHERE (`Entry` = '840');

UPDATE `war_world`.`item_infos` SET `SlotId` = '24' WHERE (`Entry` = '129837708');

UPDATE `war_world`.`item_infos` SET `Stats` = '4:15;6:11;9:33;78:3;0:0;0:0;0:0;0:0;0:0;0:0;0:0;0:0;' WHERE (`Entry` = '421176');
DELETE FROM `war_world`.`quests_creature_starter` WHERE (`Entry` = '60501');
DELETE FROM `war_world`.`quests_creature_starter` WHERE (`Entry` = '60509');
/* Removes custom starting quest */
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '1000225');
DELETE FROM `war_world`.`creature_protos` WHERE (`Entry` = '1000226');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '852309');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '852090');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '7210876');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '7210883');
/* Removes starting quest merchants */
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '50') and (`ItemId` = '1337');
DELETE FROM `war_world`.`vendor_items` WHERE (`ItemGuid` = '0') and (`VendorId` = '51') and (`ItemId` = '1337');
/* Removes winds of magic item from vendors */


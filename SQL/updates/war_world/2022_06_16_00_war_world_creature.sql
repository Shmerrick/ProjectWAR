-- Marauder
UPDATE `creature_protos` SET `Faction` = '129' WHERE `Entry` = '39'; 
UPDATE `creature_spawns` SET `Faction` = '129' WHERE `Entry` = '39'; 
UPDATE `creature_spawns` SET `Level` = '2', `RespawnMinutes` = '2' WHERE `Entry` = '39'; 
UPDATE `creature_protos` SET `MinLevel` = '2' , `MaxLevel` = '3' WHERE `Entry` = '39'; 
UPDATE `creature_spawns` SET `Emote` = '0' WHERE `Emote` = '12' AND `Entry` = '39'; 
-- Raven Hellservant
UPDATE `creature_protos` SET `Faction` = '129' WHERE `Entry` = '367'; 
UPDATE `creature_spawns` SET `Faction` = '129', `RespawnMinutes` = '2' WHERE `Entry` = '367'; 
UPDATE `creature_spawns` SET `Emote` = '0' WHERE `Emote` = '12' AND `Entry` = '367'; 
-- Griffon Striker
UPDATE `creature_protos` SET `MinLevel` = '2' , `MaxLevel` = '3' WHERE `Entry` = '30'; 
UPDATE `creature_spawns` SET `Level` = '2', `RespawnMinutes` = '2' WHERE `Entry` = '30'; 
UPDATE `creature_spawns` SET `Emote` = '0' WHERE `Emote` = '12' AND `Entry` = '30'; 
-- Marauder Torchbearer
UPDATE `creature_protos` SET `MinLevel` = '2' , `MaxLevel` = '3' , `Faction` = '129' WHERE `Entry` = '98324'; 
UPDATE `creature_spawns` SET `Level` = '2', `RespawnMinutes` = '2', `Emote` = '10' WHERE `Entry` = '98324'; 
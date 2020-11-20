-- Remove NPCs from T1 destro WC

DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '7098305');
DELETE FROM `war_world`.`creature_spawns` WHERE (`Guid` = '7098304');

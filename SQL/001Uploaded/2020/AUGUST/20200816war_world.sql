USE `war_world`

ALTER TABLE `pet_mastery_modifiers`
MODIFY `MasteryModifierAddition` smallint unsigned;
 
ALTER TABLE `creature_stats`
MODIFY `statvalue` INT;

ALTER TABLE `instance_infos`
MODIFY `orderexitzonejumpid` INT UNSIGNED,
MODIFY `destrexitzonejumpid` INT UNSIGNED;
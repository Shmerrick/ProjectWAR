-- Fixes data type errors between DB and Emu

ALTER TABLE `war_world`.`creature_stats` 
CHANGE COLUMN `StatValue` `StatValue` INT NULL DEFAULT '0' ;

ALTER TABLE `war_world`.`instance_infos` 
CHANGE COLUMN `OrderExitZoneJumpID` `OrderExitZoneJumpID` INT UNSIGNED NOT NULL ,
CHANGE COLUMN `DestrExitZoneJumpID` `DestrExitZoneJumpID` INT UNSIGNED NOT NULL ;

ALTER TABLE `war_world`.`pet_mastery_modifiers` 
CHANGE COLUMN `MasteryModifierAddition` `MasteryModifierAddition` INT UNSIGNED NULL DEFAULT '0' ;

ALTER TABLE `war_world`.`pet_mastery_modifiers` 
CHANGE COLUMN `MasteryModifierAddition` `MasteryModifierAddition` SMALLINT(2) UNSIGNED NULL DEFAULT '0' ;

ALTER TABLE `war_world`.`ability` 
CHANGE COLUMN `Disabled` `Disabled` TINYINT(1) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `CanClickOff` `CanClickOff` TINYINT(1) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `CantCrit` `CantCrit` TINYINT(1) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `EnemyTargetIgnoreLOS` `EnemyTargetIgnoreLOS` TINYINT(1) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `FriendlyTargetIgnoreLOS` `FriendlyTargetIgnoreLOS` TINYINT(1) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `Toggle` `Toggle` TINYINT(1) UNSIGNED NULL DEFAULT NULL ,
CHANGE COLUMN `CreateBinData` `CreateBinData` TINYINT(1) UNSIGNED NULL DEFAULT NULL ;

ALTER TABLE `war_world`.`ability_component` 
CHANGE COLUMN `Multipliers` `Multipliers` VARCHAR(255) NULL DEFAULT NULL ;

ALTER TABLE `war_world`.`ability_component_x_component` 
CHANGE COLUMN `Disabled` `Disabled` TINYINT(1) UNSIGNED NULL DEFAULT NULL ;

-- Unable to correct ability_expression. I don't know why.

ALTER TABLE `war_world`.`ability_line` 
CHANGE COLUMN `Disabled` `Disabled` TINYINT(1) UNSIGNED NULL DEFAULT NULL ;


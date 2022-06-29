ALTER TABLE `creature_smart_abilities` CHANGE `UniqueId` `Guid` INT(11) NOT NULL AUTO_INCREMENT; 
ALTER TABLE `creature_smart_abilities` CHANGE `Name` `SpellCastName` TEXT CHARSET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL;
ALTER TABLE `creature_smart_abilities` CHANGE `Speech` `SpellCastSpeech` TEXT CHARSET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL; 
ALTER TABLE `creature_smart_abilities` CHANGE `Condition` `SpellCondition` TEXT CHARSET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL; 
ALTER TABLE `creature_smart_abilities` CHANGE `Execution` `SpellCastExecution` TEXT CHARSET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL; 
ALTER TABLE `creature_smart_abilities` CHANGE `ExecuteChance` `SpellExecuteChance` INT(11) NOT NULL; 
ALTER TABLE `creature_smart_abilities` CHANGE `CoolDown` `SpellCastCoolDown` INT(11) NOT NULL; 
ALTER TABLE `creature_smart_abilities` CHANGE `Sound` `SpellCastSound` TEXT CHARSET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL; 
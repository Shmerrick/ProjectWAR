SET @PROTOENTRY := 364;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8566','20',@PROTOENTRY,'Raven, Heal ME!','0','50','0','1','1','0','0','1',NULL);
UPDATE `war_world`.`creature_protos` SET `Ranged` = '10' WHERE `Entry` = @PROTOENTRY; 


-- Raven Hellmagus^f
DELETE FROM `creature_abilities` WHERE `ProtoEntry`=98368;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
('98368','9506','0','9506','','0','0','0','1','1','1','0','4','20'); -- Black Horror
UPDATE `war_world`.`creature_protos` SET `Ranged` = '50' WHERE `Entry` = '98368'; 



-- Venombite Nachtspinner^m
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=1421;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
('1421','41','0','41','','0','0','0','1','1','0','0','0',NULL); -- Envenomed Fang

-- Grauer
SET @PROTOENTRY := 8889;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','1',NULL); 


SET @PROTOENTRY := 1423;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'9471','10',@PROTOENTRY,'','0','0','0','1','1','0','0','1',NULL); 

DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=1425;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
('1425','43','0','xxxxx43','','0','0','0','1','1','0','0','0',NULL); -- Roar

-- 


-- Raven Hellservant
SET @PROTOENTRY := 366;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8002','5',@PROTOENTRY,'','0','0','0','1','1','0','0','1',NULL); 

SET @PROTOENTRY := 3148;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','1',NULL); 

SET @PROTOENTRY := 1425;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','1',NULL); 
























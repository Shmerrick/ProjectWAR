SET @PROTOENTRY := 34190;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12025','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 34197;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12201','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34200;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1673','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 4699;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1747','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 4706;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1676','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL),
(@PROTOENTRY,'240','0',470601,'','0','0','0','0','1','1','0','1',NULL),
(@PROTOENTRY,'12725','20',470602,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34191;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4701;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1742','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4709;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12212','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4701;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8005','15',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 34947;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'49','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34210;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1664','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

-- Leaping Bite (Bite,Fangs,Squig,Monster,Mutant)
SET @PROTOENTRY := 34949;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12361','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);  

SET @PROTOENTRY := 34946;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12221','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 34963;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8418','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34941;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'606','60',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34956;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1364','0',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34996;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8005','15',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 35005;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1670','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35015;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1747','0',@PROTOENTRY,'WAAAAAGGHHH!!!!','0','30','0','0','1','0','0','1',NULL),
(@PROTOENTRY,'1744','30',3501501,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34672;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12285','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4709;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12285','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35000;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1761','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35002;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1747','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 4693;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12402','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 15923;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12221','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 136;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1354','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35008;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35011;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34995;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35014;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34994;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1664','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 15923;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12725','12',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 
 
SET @PROTOENTRY := 4711;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'436','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34223;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'7871','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 34218;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1930','10',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 4700;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1825','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 4710;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1825','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 4713;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1822','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 35006;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1667','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 1077;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12011','12',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 17060;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'438','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 131;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1761','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 130;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1900','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 6412;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12522','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

 SET @PROTOENTRY := 34592;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12321','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4707;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'7871','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 36386;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'9471','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 4695;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12452','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34597;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1352','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '20' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 261;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1745','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 128;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1745','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 72635;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8470','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 72634;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12522','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34990;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 19850;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12111','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34674;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1522','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36006;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'9506','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '50' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 34662;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12522','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34671;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'437','7',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35883;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12100','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36341;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'5627','12',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36332;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36320;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8399','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 4693;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12211','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 37906;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12202','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 15923;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12342','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36387;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12522','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35116;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12522','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36385;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12192','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35356;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12261','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 37192;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12261','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 19850;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12091','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36676;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12191','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36681;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12191','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 34671;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'49','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35002;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1673','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 35111;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1742','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 13403;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1904','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 36319;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1667','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 7869;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1459','0',@PROTOENTRY,'','0','50','0','0','1','0','0','1',NULL); 

SET @PROTOENTRY := 36412;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'606','60',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 37104;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1459','0',@PROTOENTRY,'','0','50','0','0','1','0','0','1',NULL),
(@PROTOENTRY,'1357','15',3710401,'','0','0','0','1','1','0','0','4',NULL),
(@PROTOENTRY,'7872','0',3710402,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 35114;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'435','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36408;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'7872','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 35949;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1829','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35948;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1827','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36408;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1827','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 17060;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'438','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 35110;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8005','15',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 1115;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8005','15',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 1109;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1667','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL), 
(@PROTOENTRY,'7872','0',110901,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 19919;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'7872','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 19918;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12122','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 19916;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 72782;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12121','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 36780;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12602','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 19917;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12453','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL),
(@PROTOENTRY,'12151','15',1991701,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 1116;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1459','0',@PROTOENTRY,'','0','50','0','0','1','0','0','1',NULL); 

SET @PROTOENTRY := 1080;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1673','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 257;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1910','0',@PROTOENTRY,'','0','60','0','0','1','0','0','1',NULL),
(@PROTOENTRY,'1900','0',25701,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 10194;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1670','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 10211;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1670','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 10200;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'437','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 10210;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3163;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12402','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 14249;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12053','15',@PROTOENTRY,'','0','0','0','1','1','0','0','1',NULL); 

SET @PROTOENTRY := 7874;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8005','15',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 19915;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'240','0',@PROTOENTRY,'','0','0','0','0','1','1','0','1',NULL),
(@PROTOENTRY,'8005','15',1991501,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 1085;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 1084;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1745','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 1083;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1676','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 36621;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12081','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 1082;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12121','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 36846;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'12081','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 36809;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1747','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 36791;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8005','15',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 36809;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8005','15',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 36839;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1761','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 100757;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1899','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 
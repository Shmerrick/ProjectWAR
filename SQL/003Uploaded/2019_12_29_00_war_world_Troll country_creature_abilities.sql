SET @PROTOENTRY := 3498;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3492;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5446;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3490;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3500;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'9506','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '50' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 3509;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5438;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3509;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3584;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1354','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);  

SET @PROTOENTRY := 3585;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1354','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 2308;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'9324','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 2297;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 12393;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5443;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'48','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 13018;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3585;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5533;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 10661;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 13885;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3509;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 5436;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1354','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3506;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5435;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5444;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'48','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5545;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'49','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 3506;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 12387;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3509;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3492;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5442;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 10661;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5443;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5441;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'54','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5435;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5545;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'438','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 26193;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'437','7',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5448;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'58','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 

SET @PROTOENTRY := 12836;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'437','7',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4651;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5545;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 2880;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8002','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 16351;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5132;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4386;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3493;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8418','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5923;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'240','0',@PROTOENTRY,'','0','0','0','0','1','1','0','1',NULL); 

SET @PROTOENTRY := 5015;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4997;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5005;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 561;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4397;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 11435;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4388;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'438','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4390;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'7871','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 561;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'48','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 13034;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 565;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4379;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4386;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4385;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5378;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

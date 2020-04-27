SET @PROTOENTRY := 364;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8548','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 367;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8002','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 366;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8315','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 98368;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8470','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 1421;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 7318;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5073;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 8889;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3148;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'439','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 1425;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 1423;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'9471','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 144;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4672;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'48','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4347;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 364;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8553','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 147;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8159','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 287;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'438','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 43;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1364','0',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 137;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8418','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 7310;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1364','0',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 142;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'240','0',@PROTOENTRY,'','0','0','0','0','1','1','0','1',NULL); 

SET @PROTOENTRY := 1419;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'438','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 331;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 37;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'49','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 658;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'437','7',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3519;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 666;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'47','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5475;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 10635;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 536;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'54','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 1;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8548','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY;

SET @PROTOENTRY := 3535;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5027;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3987;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3981;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8002','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3994;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 7305;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'46','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 1537;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5038;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3532;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1459','0',@PROTOENTRY,'','0','50','0','0','1','0','0','1',NULL); 

SET @PROTOENTRY := 4216;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3520;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8002','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4217;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4216;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 372;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'46','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3523;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1433','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3534;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1459','0',@PROTOENTRY,'','0','50','0','0','1','0','0','1',NULL); 

SET @PROTOENTRY := 2392;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'41','10',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3543;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8002','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3531;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1354','20',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3527;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8470','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 68;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'438','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3547;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8548','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 4122;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 3521;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 760;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8470','0',@PROTOENTRY,'','0','0','0','1','1','1','0','4',NULL); 
UPDATE `war_world`.`creature_protos` SET `Ranged` = '100' WHERE `Entry` = @PROTOENTRY; 

SET @PROTOENTRY := 3522;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'48','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 98354;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'240','0',@PROTOENTRY,'','0','0','0','0','1','1','0','1',NULL); 

SET @PROTOENTRY := 2392;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'48','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 2404;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'49','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL);

SET @PROTOENTRY := 4117;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'7871','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 3556;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'7871','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 7316;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4139;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8553','30',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4131;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 27047;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'8002','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 384;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4367;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'4459','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 4469;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'42','8',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 5260;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'7871','0',@PROTOENTRY,'','0','30','0','0','1','0','0','1',NULL);

SET @PROTOENTRY := 7842;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'1357','5',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 

SET @PROTOENTRY := 10429;
DELETE FROM `creature_abilities` WHERE  `ProtoEntry`=@PROTOENTRY;
INSERT INTO `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values
(@PROTOENTRY,'438','15',@PROTOENTRY,'','0','0','0','1','1','0','0','4',NULL); 
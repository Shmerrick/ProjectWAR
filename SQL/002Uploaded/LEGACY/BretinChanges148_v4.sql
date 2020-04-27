#WL Pounce
UPDATE `war_world`.`ability_commands` SET `EffectRadius`=NULL WHERE `Entry`='9186' and`CommandID`='1' and`CommandSequence`='0';
DELETE FROM `war_world`.`ability_commands` WHERE `Entry`='9186' and`CommandID`='2' and`CommandSequence`='0';
DELETE FROM `war_world`.`ability_damage_heals` WHERE `Entry`='9186' and`Index`='0' and`ParentCommandID`='2' and`ParentCommandSequence`='0';

#wh bullets
UPDATE `war_world`.`abilities` SET `StealthInteraction`=NULL WHERE `Entry`='8084';
UPDATE `war_world`.`abilities` SET `StealthInteraction`=NULL WHERE `Entry`='8089';
UPDATE `war_world`.`abilities` SET `StealthInteraction`=NULL WHERE `Entry`='8099';

#we kiss
UPDATE `war_world`.`abilities` SET `StealthInteraction`=NULL WHERE `Entry`='9402';
UPDATE `war_world`.`abilities` SET `StealthInteraction`=NULL WHERE `Entry`='9407';
UPDATE `war_world`.`abilities` SET `StealthInteraction`=NULL WHERE `Entry`='9412';

#magus mid tree
UPDATE `war_world`.`abilities` SET `PointCost`='13' WHERE `Entry`='8494';
UPDATE `war_world`.`abilities` SET `PointCost`='9' WHERE `Entry`='8502';

#DWLF swap DHDC
UPDATE `war_world`.`abilities` SET `MinimumRank`='15' WHERE `Entry`='1782';
UPDATE `war_world`.`abilities` SET `MinimumRank`='33' WHERE `Entry`='1778';

#EA
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='9d59bd87-a9b7-11e6-a4d7-00ff0731187a';
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='be255dc5-9c61-11e6-8ae8-00ff0731187a';
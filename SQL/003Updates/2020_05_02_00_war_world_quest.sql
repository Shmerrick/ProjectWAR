UPDATE `war_world`.`creature_protos` SET `Model1` = '1220' WHERE `Entry` = '31'; 
UPDATE `war_world`.`creature_protos` SET `Model2` = '1221' WHERE `Entry` = '31'; 
UPDATE `war_world`.`creature_protos` SET `MinLevel` = '3' , `MaxLevel` = '3' WHERE `Entry` = '31'; 
replace into `creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) values('31','10','1929','0','0','0');
replace into `creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) values('31','20','3020','0','0','0');
replace into `creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) values('31','21','3021','0','0','0');
replace into `creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) values('31','22','3022','0','0','0');
replace into `creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) values('31','24','3185','0','0','0');
replace into `creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) values('31','25','3194','0','0','0');
replace into `creature_items` (`Entry`, `SlotId`, `ModelId`, `EffectId`, `PrimaryColor`, `SecondaryColor`) values('31','28','3107','0','0','0');

replace into `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values('31','12501','100','3104','You found out the truth, NOW DIE !!!','0','0','0','0','1','1','0','1',NULL);
replace into `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values('31','12501','100','3102','We are the Raven\'s Horde!','0','0','0','0','1','1','0','1',NULL);
replace into `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values('31','12501','100','3103','I will let your guts out!','0','0','0','0','1','1','0','1',NULL);
replace into `creature_abilities` (`ProtoEntry`, `AbilityId`, `Cooldown`, `creature_abilities_ID`, `Text`, `TimeStart`, `ActivateAtHealthPercent`, `DisableAtHealthPercent`, `AbilityCycle`, `Active`, `ActivateOnCombatStart`, `RandomTarget`, `TargetFocus`, `MinRange`) values('31','12501','100','3101','Die, you scum!','0','0','0','0','1','1','0','1',NULL);
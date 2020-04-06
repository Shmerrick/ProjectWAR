-- working
UPDATE `war_world`.`buff_infos` SET `Silent` = '0' WHERE (`Entry` = '5063');
UPDATE `war_world`.`ability_damage_heals` SET `Index` = '1' WHERE (`Entry` = '5063') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `InvokeOn` = '7' WHERE (`Entry` = '5063') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`abilities` SET `IgnoreGlobalCooldown` = '0' WHERE (`Entry` = '5063');
UPDATE `war_world`.`abilities` SET `IgnoreGlobalCooldown` = '0' WHERE (`Entry` = '5092');
UPDATE `war_world`.`abilities` SET `Range` = '35', `MinimumRank` = '1', `AIRange` = '35' WHERE (`Entry` = '5063');
UPDATE `war_world`.`ability_commands` SET `EffectRadius` = '100', `FromAllTargets` = '1' WHERE (`Entry` = '5063') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `MaxCopies` = '1', `Duration` = '5' WHERE (`Entry` = '5063');
UPDATE `war_world`.`ability_commands` SET `FromAllTargets` = '1', `MaxTargets` = '6' WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '1');
UPDATE `war_world`.`ability_knockback_info` SET `Power` = '1200', `RangeExtension` = '60', `GravMultiplier` = '3' WHERE (`Entry` = '13713') and (`Id` = '0');
UPDATE `war_world`.`ability_knockback_info` SET `RangeExtension` = '300' WHERE (`Entry` = '13713') and (`Id` = '0');
UPDATE `war_world`.`buff_commands` SET `PrimaryValue` = '48', `TertiaryValue` = '5' WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `Duration` = '5' WHERE (`Entry` = '5092');
UPDATE `war_world`.`buff_commands` SET `EffectRadius` = '180', `MaxTargets` = '6' WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `BuffLine` = '1' WHERE (`Entry` = '5092') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_knockback_info` SET `RangeExtension` = '500' WHERE (`Entry` = '13713') and (`Id` = '0');
-- ability type 1
UPDATE `war_world`.`abilities` SET `AbilityType` = '1' WHERE (`Entry` = '5092');
-- working
UPDATE `war_world`.`ability_knockback_info` SET `RangeExtension` = '700' WHERE (`Entry` = '13713') and (`Id` = '0');
UPDATE `war_world`.`ability_commands` SET `FromAllTargets` = NULL, `AttackingStat` = '1' WHERE (`Entry` = '5063') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = 'Career', `Group` = '0', `Interval` = '1000', `CanRefresh` = '1' WHERE (`Entry` = '5063');
UPDATE `war_world`.`buff_commands` SET `EffectSource` = 'Caster', `BuffLine` = '1' WHERE (`Entry` = '5063') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `EffectSource` = 'Caster' WHERE (`Entry` = '5063') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`abilities` SET `AbilityType` = '1' WHERE (`Entry` = '5063');
UPDATE `war_world`.`quests` SET `OnCompletionQuest` = 'You have done well. Skull Lord Var\'lthrok and his supporters are now banished to his Bastion Stair. Claim your new weapon and send this daemon to hell once and for all.' WHERE (`Entry` = '1');
UPDATE `war_world`.`quests` SET `OnCompletionQuest` = 'You have done well. Skull Lord Var\'lthrok and his supporters are now banished to his Bastion Stair. Claim your new weapon and send this daemon to hell once and for all.' WHERE (`Entry` = '2');
UPDATE `war_world`.`quests_objectives` SET `ObjCount` = '1000' WHERE (`Guid` = '74835850');
UPDATE `war_world`.`quests_objectives` SET `ObjCount` = '1000' WHERE (`Guid` = '74835852');

# 13
UPDATE `war_world`.`quests_objectives` SET `ObjCount` = '250' WHERE (`Guid` = '74835852');
UPDATE `war_world`.`quests_objectives` SET `ObjCount` = '250' WHERE (`Guid` = '74835850');

UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '168899368');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '168944168');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204473704');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204473768');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204473832');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204476136');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204478248');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '204478312');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772268');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167782376');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167782248');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167782184');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '108003496');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '108003624');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '108856040');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772264');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772266');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772267');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772328');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167772329');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167775464');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167775848');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167776232');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167776360');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '167819500');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '170923752');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '171972648');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '173020904');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '174063721');
UPDATE `war_world`.`zone_jumps` SET `Enabled` = '0' WHERE (`Entry` = '174063726');

# Dye Reward Update
UPDATE `war_world`.`quests` SET `Choice` = '[129838021,7],[1,1]' WHERE (`Entry` = '2');
UPDATE `war_world`.`quests` SET `Choice` = '[129838021,7],[1,1]' WHERE (`Entry` = '1');

UPDATE `war_world`.`abilities` SET `EffectID` = '5968', `IgnoreGlobalCooldown` = '1' WHERE (`Entry` = '5968');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1631003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1642000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1653000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1664003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1951000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1952000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1953000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '1954000');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2601003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2602003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2603002');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2604003');
UPDATE `war_world`.`creature_abilities` SET `ActivateOnCombatStart` = '0' WHERE (`creature_abilities_ID` = '2605003');
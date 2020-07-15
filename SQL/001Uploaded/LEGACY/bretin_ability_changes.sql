## Crimson Death
UPDATE `war_world`.`buff_infos` SET `Duration`='5' WHERE `Entry`='9344';

## Detonation
UPDATE `war_world`.`buff_infos` SET `Duration`='3' WHERE `Entry`='3167';

## Machine Gun
UPDATE `war_world`.`buff_infos` SET `Interval`='500' WHERE `Entry`='22';
UPDATE `war_world`.`buff_infos` SET `Interval`='500' WHERE `Entry`='421';
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale`='1' WHERE `Entry`='22' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';


##Bugman's Brew ------------------------------- NOT RDY JUST YET!!!!!!!!!!
UPDATE `war_world`.`buff_infos` SET `AuraPropagation`=null WHERE `Entry`='32';

#1k1 Dark Blessings
UPDATE `war_world`.`buff_infos` SET `Duration`='10' WHERE `Entry`='9616';

#Mountain Spirit
UPDATE `war_world`.`buff_infos` SET `Duration`='10' WHERE `Entry`='1644';

#Sigmar's Shield
UPDATE `war_world`.`buff_infos` SET `Duration`=null WHERE `Entry`='3752';
UPDATE `war_world`.`buff_infos` SET `Duration`='20' WHERE `Entry`='3752';
UPDATE `war_world`.`buff_infos` SET `Interval`=NULL WHERE `Entry`='3752';



#'Ere We Go!
UPDATE `war_world`.`buff_infos` SET `MaxStack`='6' WHERE `Entry`='1902';

#Ere We Goes Again
UPDATE `war_world`.`buff_infos` SET `MaxStack`='6' WHERE `Entry`='1957';

#Flamethrower
UPDATE `war_world`.`abilities` SET `Cooldown`='5' WHERE `Entry`='27';
#Flamethrower
UPDATE `war_world`.`abilities` SET `Cooldown`='5' WHERE `Entry`='424';
#Warping Energy
UPDATE `war_world`.`abilities` SET `Cooldown`='5' WHERE `Entry`='445';


# lasting Chaos Daemon
UPDATE `war_world`.`buff_commands` SET `Target`='AllyOrSelf' WHERE `Entry`='8514' and`CommandID`='0' and`CommandSequence`='0';

# extra ammo turret
UPDATE `war_world`.`buff_commands` SET `Target`='AllyOrSelf' WHERE `Entry`='1556' and`CommandID`='0' and`CommandSequence`='0';

# sigmars shield
UPDATE `war_world`.`buff_commands` SET `InvokeOn`='0', `RetriggerInterval`='1' WHERE `Entry`='3752' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `PrimaryValue`='-20', `InvokeOn`='0', `EventIDString`='WasAttacked' WHERE `Entry`='3752' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`buff_infos` SET `Duration`='20' WHERE `Entry`='3752';

#rampage
UPDATE `war_world`.`buff_commands` SET `CommandName`='ModifyStat' WHERE `Entry`='1459' and`CommandID`='0' and`CommandSequence`='1';
UPDATE `war_world`.`buff_commands` SET `CommandName`='ModifyStat' WHERE `Entry`='1459' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `SecondaryValue`='1000' WHERE `Entry`='1459' and`CommandID`='0' and`CommandSequence`='0';
UPDATE `war_world`.`buff_commands` SET `SecondaryValue`='1000' WHERE `Entry`='1459' and`CommandID`='0' and`CommandSequence`='1';

## Energy of Vaul------------------------------- NOT RDY JUST YET!!!!!!!!!!
UPDATE `war_world`.`ability_commands` SET `Target`='AllyOrSelf' WHERE `Entry`='9274' and`CommandID`='2' and`CommandSequence`='0';

## Fury of Da Green------------------------------- NOT RDY JUST YET!!!!!!!!!!
UPDATE `war_world`.`ability_commands` SET `Target`='AllyOrSelf' WHERE `Entry`='1935' and`CommandID`='2' and`CommandSequence`='0';

#Machine Gun
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage`='5', `MaxDamage`='37' WHERE `Entry`='22' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage`='8', `MaxDamage`='71' WHERE `Entry`='421' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';
#Wrecking Ball
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage`='12', `MaxDamage`='93' WHERE `Entry`='8425' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';

# Force of Fury
DELETE FROM `war_world`.`ability_modifier_checks` WHERE `ability_modifier_checks_ID`='0e4e6448-2617-11e7-b55e-000c29d63948';
DELETE FROM `war_world`.`ability_commands` WHERE `Entry`='9346' and`CommandID`='0' and`CommandSequence`='0';

# Shrug It Off
DELETE FROM `war_world`.`buff_commands` WHERE `Entry`='1928' and`CommandID`='1' and`CommandSequence`='0';

# Magical Infusion
DELETE FROM `war_world`.`buff_commands` WHERE `Entry`='9272' and`CommandID`='1' and`CommandSequence`='0';

DELETE FROM `war_world`.`buff_commands` WHERE (`Entry` = '9347') and (`CommandID` = '0') and (`CommandSequence` = '1');
DELETE FROM `war_world`.`ability_modifier_checks` WHERE (`ability_modifier_checks_ID` = 'e2c90f40-260d-11e7-b55e-000c29d63948');
UPDATE `war_world`.`abilities` SET `WeaponNeeded` = '1' WHERE (`Entry` = '8272');
UPDATE `war_world`.`abilities` SET `Cooldown` = '30' WHERE (`Entry` = '1616');
UPDATE `war_world`.`abilities` SET `MoveCast` = NULL WHERE (`Entry` = '8575');
UPDATE `war_world`.`ability_commands` SET `AttackingStat` = NULL WHERE (`Entry` = '1374') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_commands` SET `AttackingStat` = NULL WHERE (`Entry` = '8339') and (`CommandID` = '0') and (`CommandSequence` = '0');
DELETE FROM `war_world`.`ability_commands` WHERE (`Entry` = '1914') and (`CommandID` = '0') and (`CommandSequence` = '2');
UPDATE `war_world`.`abilities` SET `MasteryTree` = '2', `Specline` = 'Path of Corruption', `PointCost` = '15' WHERE (`Entry` = '8373');
UPDATE `war_world`.`abilities` SET `MinimumRenown` = '0' WHERE (`Entry` = '2821');
UPDATE `war_world`.`abilities` SET `MinimumRenown` = '0' WHERE (`Entry` = '8049');
UPDATE `war_world`.`buff_commands` SET `CommandName` = 'ReapplyBuff', `PrimaryValue` = '8164', `SecondaryValue` = '100', `InvokeOn` = '5', `Target` = 'Caster', `EventIDString` = NULL, `EventCheck` = NULL, `EventCheckParam` = NULL, `EventChance` = NULL, `BuffLine` = NULL, `NoAutoUse` = NULL WHERE (`Entry` = '8197') and (`CommandID` = '0') and (`CommandSequence` = '0');
INSERT INTO `war_world`.`ability_modifier_checks` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `ID`, `Sequence`, `CommandName`, `FailCode`, `PrimaryValue`, `SecondaryValue`, `ability_modifier_checks_ID`) VALUES ('8197', 'Crown of Fire', '8164', 'Flames of Rhuin', '2', '0', '0', 'CasterTargetRelation', '2', '1', NULL, '1');
UPDATE `war_world`.`ability_modifier_checks` SET `ability_modifier_checks_ID` = '1c025806-da22-11e8-83a3-9c5c8e86a5c1' WHERE (`ability_modifier_checks_ID` = '1');
INSERT INTO `war_world`.`ability_modifiers` (`Entry`, `SourceAbility`, `Affecting`, `AffectedAbility`, `PreOrPost`, `Sequence`, `ModifierCommandName`, `PrimaryValue`, `ability_modifiers_ID`) VALUES ('8197', 'Crown of Fire', '8164', 'Flames of Rhuin', '2', '0', 'SetEventChance', '75', '7faa28ed-8b2b-11e6-b8e9-00ff0731187a');


#elite training
UPDATE `war_world`.`buff_infos` SET `FriendlyEffectID` = NULL WHERE (`Entry` = '9347');


#warping energy
UPDATE `war_world`.`abilities` SET `Cooldown`='5' WHERE `Entry`='56';

#Force of Fury
UPDATE `war_world`.`buff_commands` SET `CommandName`='ModifyStat' WHERE `Entry`='9346' and`CommandID`='1' and`CommandSequence`='0';
UPDATE `war_world`.`ability_commands` SET `Target`='Caster' WHERE `Entry`='9346' and`CommandID`='0' and`CommandSequence`='1';
INSERT INTO `war_world`.`ability_commands` (`Entry`, `AbilityName`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `Target`) VALUES ('9346', 'Force of Fury', '0', '0', 'InvokeBuff', '9346', 'Caster');

#Crown of Fire
UPDATE `war_world`.`ability_modifiers` SET `BuffLine`='1' WHERE `ability_modifiers_ID`='7faa28ed-8b2b-11e6-b8e9-00ff0731187a';

#Frozen Fury
UPDATE `war_world`.`ability_modifiers` SET `SecondaryValue`=NULL WHERE `ability_modifiers_ID`='7faa28ed-8b2b-11e6-b8e9-00ff0731187a';
UPDATE `war_world`.`ability_modifiers` SET `SecondaryValue`=NULL WHERE `ability_modifiers_ID`='7fa9ce45-8b2b-11e6-b8e9-00ff0731187a';

#Sigmar's Grace
UPDATE `war_world`.`ability_commands` SET `EffectRadius`='100' WHERE `Entry`='8269' and`CommandID`='0' and`CommandSequence`='0';

#Whirling Axe
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage`='10', `MaxDamage`='75' WHERE `Entry`='9188' and`Index`='1' and`ParentCommandID`='0' and`ParentCommandSequence`='0';

##Elite training
UPDATE `war_world`.`ability_commands` SET `Target` = 'Caster' WHERE (`Entry` = '9347') and (`CommandID` = '0') and (`CommandSequence` = '1');



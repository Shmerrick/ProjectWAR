#Flak Jacket
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = 'Tactic' WHERE (`Entry` = '1527');

#Daemonic Armor
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = 'Tactic' WHERE (`Entry` = '8492');

#Redirected Force
UPDATE `war_world`.`ability_damage_heals` SET `Undefendable` = '1' WHERE (`Entry` = '9032') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Smite
UPDATE `war_world`.`ability_commands` SET `Entry` = '0', `AbilityName` = 'Smite - OLD', `CommandID` = '0' WHERE (`Entry` = '8250') and (`CommandID` = '1') and (`CommandSequence` = '0');

#Essence Lash
UPDATE `war_world`.`ability_commands` SET `Entry` = '0', `AbilityName` = 'Essence Lash - OLD' WHERE (`Entry` = '9566') and (`CommandID` = '1') and (`CommandSequence` = '0');

#Demolishing Strike
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '606');

#Sprout Carapace
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '8373');

#1k1 Dark Blessing
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '9616');

#Cannon Smash
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '1576');

#Mountain Spirit
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '1644');

#Gift of Life
#Already Done

#Tzeentch's Talon
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '8605');

#Sweeping Disgorgement
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '8600');
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '8563');


#Exhaustive Strike (Marauder)
UPDATE `war_world`.`buff_infos` SET `CanRefresh` = NULL WHERE (`Entry` = '3060');

#Divine Aegis
UPDATE `war_world`.`buff_infos` SET `Name` = 'Divine Replenishment' WHERE (`Entry` = '8303');
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '8302');
UPDATE `war_world`.`buff_commands` SET `TertiaryValue` = '1320' WHERE (`Entry` = '8302') and (`CommandID` = '1') and (`CommandSequence` = '0');


#Khaine's Imbuement
UPDATE `war_world`.`buff_infos` SET `CanRefresh` = NULL WHERE (`Entry` = '3739');

#Hollow Points
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3838') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Tangling Wire
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3468') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Deep Incision
UPDATE `war_world`.`buff_infos` SET `CanRefresh` = NULL WHERE (`Entry` = '3197');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3197') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '9068') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Ensorcelled Agony
UPDATE `war_world`.`buff_infos` SET `CanRefresh` = NULL WHERE (`Entry` = '3338');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '9045') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3338') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Potent Enchantments
UPDATE `war_world`.`buff_infos` SET `CanRefresh` = '1' WHERE (`Entry` = '9047');
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'Spiritual' WHERE (`Entry` = '9047') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Rune of Fortune
UPDATE `war_world`.`ability_commands` SET `PrimaryValue` = '150' WHERE (`Entry` = '1611') and (`CommandID` = '0') and (`CommandSequence` = '1');

#Grimnir's Fury
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '1619') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Guilty Soul
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3757') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Backlash
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '800') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3595') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Lingering Fire
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1' WHERE (`Entry` = '3350') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3350') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Playing with Fire
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '8184') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

#Wildfire
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'Corporeal', `NoCrits` = '1' WHERE (`Entry` = '3423') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#None Shall Pass
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '9345') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

#Tainted Wound
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'Spiritual', `NoCrits` = '1' WHERE (`Entry` = '8368') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Khaines Imbuement
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3739') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Avenging the Debt
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '1381') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');

#Shield of Sun
UPDATE `war_world`.`buff_commands` SET `RetriggerInterval` = '2000' WHERE (`Entry` = '8025') and (`CommandID` = '0') and (`CommandSequence` = '0');

#Aegis of Orange Fire
UPDATE `war_world`.`ability_damage_heals` SET `Undefendable` = '1' WHERE (`Entry` = '8501') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

#Infernal Pain
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '3939') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Shadow Blades
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'RawDamage', `NoCrits` = '1' WHERE (`Entry` = '9076') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Waves of Chaos
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '8585') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '8572') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '8577') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '8574') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '150', `MaxDamage` = '150' WHERE (`Entry` = '8572') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '150', `MaxDamage` = '150' WHERE (`Entry` = '8574') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '150', `MaxDamage` = '150' WHERE (`Entry` = '8577') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '150', `MaxDamage` = '150' WHERE (`Entry` = '8585') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');


#Earthen Renewal
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'RawHealing' WHERE (`Entry` = '1426') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `NoCrits` = '1' WHERE (`Entry` = '1426') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Armor of Eternal Servitude
UPDATE `war_world`.`buff_infos` SET `BuffClassString` = NULL WHERE (`Entry` = '9382');



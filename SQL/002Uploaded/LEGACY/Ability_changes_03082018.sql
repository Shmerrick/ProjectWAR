## Updating ravage dmg to live. Increasing primary stat contribution to overall damage
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2.8' WHERE (`Entry` = '8323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Updating Relentless dmg to be viable. Less than t'ree hit combo but usable
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '37', `MaxDamage` = '281', `WeaponDamageScale` = '1', `StatDamageScale` = '1' WHERE (`Entry` = '8343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Updating Myrmidia's Fury dmg to be comparable to relentless
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '37', `MaxDamage` = '281', `WeaponDamageScale` = '1', `StatDamageScale` = '1' WHERE (`Entry` = '8031') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Updating DOK GRP heal to live value
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.75' WHERE (`Entry` = '9557') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Updating WP GRP Heal to live value
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.75' WHERE (`Entry` = '8247') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Updating WE Pierce Armor dmg from by .25 primary stat contribution
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.25' WHERE (`Entry` = '9421') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Updating ravage dmg to live. Increasing primary stat contribution to overall damage
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2.6' WHERE (`Entry` = '8323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Updating Relentless dmg to be viable. Less than t'ree hit combo but usable
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.9', `StatDamageScale` = '0.9' WHERE (`Entry` = '8343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Updating Myrmidia's Fury dmg to be comparable to relentless
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.9', `StatDamageScale` = '0.9' WHERE (`Entry` = '8031') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

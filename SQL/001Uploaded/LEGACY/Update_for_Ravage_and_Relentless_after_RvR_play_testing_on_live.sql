## Update relentless dmg based on player testing
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.8' WHERE (`Entry` = '8343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Updating Ravage dmg based on player testing
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2.3' WHERE (`Entry` = '8323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Myrmidia's Fury damage tune down
UPDATE war_world.ability_damage_heals SET WeaponDamageScale = '0.6', StatDamageScale = '0.8' WHERE (Entry = '8031') and (war_world.ability_damage_heals.Index = '1') and (ParentCommandID = '0') and (ParentCommandSequence = '0');
## Ravage and Relentless damage tune down
UPDATE war_world.ability_damage_heals SET StatDamageScale = '2.4' WHERE (Entry = '8323') and (war_world.ability_damage_heals.Index = '0') and (ParentCommandID = '0') and (ParentCommandSequence = '0');
UPDATE war_world.ability_damage_heals SET WeaponDamageScale = '0.7', StatDamageScale = '1' WHERE (Entry = '8343') and (war_world.ability_damage_heals.Index = '1') and (ParentCommandID = '0') and (ParentCommandSequence = '0');
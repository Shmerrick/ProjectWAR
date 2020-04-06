-- Rotten Arrer scale change from 1.6 to 1.0
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1853') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Poison Arrer scale change from 1.0 t0 3.0
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '1847') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Torment/Agonizing Wound Stat Contribution Fix
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8085') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9408') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Acid Arrow Stat Scale 1.0 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9104') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Poison Arrer Cast Scale Change
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1847') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Festering Arrow Cast Scale Change
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9085') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Mass Cast Time Bonus Fix greater than 1 set to 0
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '24') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '27') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8169') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8548') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8573') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9470') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9502') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9502') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9506') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9506') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1586') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1611') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1899') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1903') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1929') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1929') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1930') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1931') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1935') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1935') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8159') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8470') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8486') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8495') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8496') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8498') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1596') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1904') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1907') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1955') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '3878') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '8566') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9242') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9245') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9291') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '21421') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '22') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '422') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '424') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1508') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1536') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1538') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1762') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1776') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1821') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1840') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '1851') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '3320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '3320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9082') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9100') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9237') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9250') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9257') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9266') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9266') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9271') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9274') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9274') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9479') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9480') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '0' WHERE (`Entry` = '9480') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');

-- State Scale Fix for all cast time 0 abilities. When cast time = 0, mult is 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '24') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '27') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '422') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '22') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '424') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1508') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1536') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1538') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1586') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '1596') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1611') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1637') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1762') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.89' WHERE (`Entry` = '1776') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.6' WHERE (`Entry` = '1840') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1851') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1899') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1903') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '1904') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '1907') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1929') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1929') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1930') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1955') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3878') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '8159') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '8169') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8295') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '8470') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8486') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '8495') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '8496') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '8548') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '8566') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '8573') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.2' WHERE (`Entry` = '9100') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9231') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9237') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '9242') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '9245') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9250') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9257') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9266') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9266') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '9271') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9274') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9274') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '9470') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9479') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9480') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9480') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9502') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9502') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9506') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9506') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9585') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3.5' WHERE (`Entry` = '21421') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Weapon Damage Scale Fix
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9084') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- More Stat Contribution Scaling Changes
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '3') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '5') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '7') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '10') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '11') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '11') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '12') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '12') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '21') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '25') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '25') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '41') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '42') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '45') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '47') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '47') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '48') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '49') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '54') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '56') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '57') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.38' WHERE (`Entry` = '58') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '420') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '421') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '423') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '423') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '426') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '427') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '430') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '432') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '432') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '433') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '434') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '436') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '438') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '439') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '440') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '440') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '441') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '441') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '442') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '443') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '445') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '446') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '447') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '528') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '606') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '696') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '779') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '797') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '800') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1352') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1354') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1355') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1356') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1357') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1359') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.2' WHERE (`Entry` = '1361') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1362') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1364') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '1366') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1369') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1371') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1374') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1381') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '1384') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1385') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1419') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1420') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1425') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1430') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1431') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1432') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1433') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1434') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1435') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1436') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1437') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1438') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1439') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1443') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1447') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1449') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1461') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1462') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1463') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1464') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1465') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1489') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1494') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '1.5' WHERE (`Entry` = '1509') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.2' WHERE (`Entry` = '1517') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1520') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1522') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1523') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '1524') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1525') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1528') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.75' WHERE (`Entry` = '1530') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1531') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '1537') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.38' WHERE (`Entry` = '1539') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.38' WHERE (`Entry` = '1539') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '8', `StatDamageScale` = '1.2' WHERE (`Entry` = '1540') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1541') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1543') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1587') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '1588') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '1591') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1593') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1593') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1594') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1599') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '1600') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1603') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '0.38' WHERE (`Entry` = '1607') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1613') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.6' WHERE (`Entry` = '1617') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1639') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1656') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1664') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1665') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1666') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1667') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1669') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1670') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1672') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1676') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1677') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1678') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '1680') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1682') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1683') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1687') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1688') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1689') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1690') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1691') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1693') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1694') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1695') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1700') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1704') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1706') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1711') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1722') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1727') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1728') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1729') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1733') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1742') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1743') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1744') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1745') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1746') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1747') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.5' WHERE (`Entry` = '1748') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1749') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1750') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1751') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1755') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1759') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1761') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1770') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1772') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1773') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1774') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1795') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1795') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1820') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1824') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1824') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1825') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1826') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '1828') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1833') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1834') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1835') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1836') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '1.5' WHERE (`Entry` = '1837') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1838') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1839') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1846') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1848') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.2' WHERE (`Entry` = '1849') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '1850') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1854') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1858') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1861') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '1898') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1906') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1913') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1917') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1928') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1932') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '1933') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1951') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1955') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '1957') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3016') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '3054') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '3055') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '3195') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.23' WHERE (`Entry` = '3221') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3281') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.38' WHERE (`Entry` = '3283') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3290') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3310') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3342') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '0.3' WHERE (`Entry` = '3462') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '3468') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3534') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3551') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3552') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3553') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3554') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3558') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '3600') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '3611') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '3621') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3695') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '3737') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '.01' WHERE (`Entry` = '3739') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3781') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3819') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.23' WHERE (`Entry` = '3831') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.45' WHERE (`Entry` = '3846') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL WHERE (`Entry` = '3878') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.23' WHERE (`Entry` = '3881') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3896') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3901') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '3921') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '3922') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '3923') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '3924') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '3925') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3928') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3950') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3961') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.38' WHERE (`Entry` = '3993') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '3995') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3998') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '4626') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '4811') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '5132') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '5233') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '5238') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '0' WHERE (`Entry` = '5257') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '0' WHERE (`Entry` = '5478') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '5598') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '5598') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '5697') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8002') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8003') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8004') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8005') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8006') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8007') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8008') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8009') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8011') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8012') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8012') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8017') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8018') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8023') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '8025') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '8026') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8031') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.27' WHERE (`Entry` = '8032') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8035') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8037') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8038') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '8049') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8059') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8068') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8081') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8083') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8086') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8087') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8087') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8087') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8087') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8087') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8097') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '8098') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8100') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8101') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8109') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8112') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8113') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8115') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.5' WHERE (`Entry` = '8116') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8127') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8158') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8160') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8170') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8173') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8174') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8176') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8183') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8183') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8186') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8201') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8204') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8216') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8220') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8220') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8221') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8222') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8236') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8238') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8240') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8250') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8251') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8252') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8253') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8255') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8256') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8257') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '8264') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8267') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8268') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8272') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8279') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8288') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8314') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8315') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '8316') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8317') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '8318') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8319') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '8321') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8324') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8328') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8329') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8331') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8331') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8338') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8339') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.6' WHERE (`Entry` = '8343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '8344') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8346') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8347') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8349') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8367') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8368') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8378') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8392') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8392') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8393') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8394') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8395') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8396') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8399') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8400') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8401') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.01' WHERE (`Entry` = '8407') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '8409') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8412') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8413') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8414') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.38' WHERE (`Entry` = '8417') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8418') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8419') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8419') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8420') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8422') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '8423') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8424') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.6' WHERE (`Entry` = '8443') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8471') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.2' WHERE (`Entry` = '8472') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8475') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8479') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8481') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8483') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8484') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '8485') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.3' WHERE (`Entry` = '8494') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.45' WHERE (`Entry` = '8497') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.45' WHERE (`Entry` = '8497') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8500') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '8502') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '8541') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8549') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3', `StatDamageScale` = '1.5' WHERE (`Entry` = '8551') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1' WHERE (`Entry` = '8552') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '8556') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.9' WHERE (`Entry` = '8558') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3', `StatDamageScale` = '1.5' WHERE (`Entry` = '8560') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8564') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '0.38' WHERE (`Entry` = '8565') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '0.38' WHERE (`Entry` = '8565') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8568') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8569') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8571') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '8574') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8578') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8578') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.2' WHERE (`Entry` = '8579') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8585') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8602') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8611') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9002') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9003') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9004') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9006') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9006') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9007') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9010') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9011') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '9012') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9014') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.67' WHERE (`Entry` = '9017') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9019') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9020') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9022') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.86' WHERE (`Entry` = '9025') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9026') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9027') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9028') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9028') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '9029') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9030') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9030') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9031') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9032') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9034') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9045') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9047') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9058') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9068') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9069') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9075') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9076') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9083') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9086') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9087') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9088') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9093') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9095') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9096') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9098') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9099') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '9101') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9103') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9105') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '9106') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9108') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9110') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.2' WHERE (`Entry` = '9111') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9119') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9123') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9138') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9158') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9160') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9161') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9163') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9163') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9165') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9167') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9167') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9170') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9172') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9174') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9175') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '0.86' WHERE (`Entry` = '9176') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9186') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9186') and (`Index` = '0') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9190') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9191') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9192') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9193') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9194') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9209') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9236') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3', `StatDamageScale` = '1.5' WHERE (`Entry` = '9244') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '9253') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9267') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3', `StatDamageScale` = '1.5' WHERE (`Entry` = '9268') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9269') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3' WHERE (`Entry` = '9271') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9272') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9302') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9308') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9309') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9314') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9315') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9316') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9317') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9320') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9321') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '0.86' WHERE (`Entry` = '9323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9324') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9327') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9328') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9329') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '0.67' WHERE (`Entry` = '9331') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '9333') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9335') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9336') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9337') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9348') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9398') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9400') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9403') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9409') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.01' WHERE (`Entry` = '9411') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9413') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.33' WHERE (`Entry` = '9422') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9427') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9428') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9439') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9448') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9458') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '9471') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '9471') and (`Index` = '1') and (`ParentCommandID` = '3') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9472') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9478') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9482') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.72' WHERE (`Entry` = '9486') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.2' WHERE (`Entry` = '9488') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9489') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9498') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9522') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '9527') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9548') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9549') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9551') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9552') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9560') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9562') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9565') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '9566') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9568') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9569') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '1.5' WHERE (`Entry` = '9574') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9576') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9577') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3', `StatDamageScale` = '1.5' WHERE (`Entry` = '9578') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3', `StatDamageScale` = '1.5' WHERE (`Entry` = '9578') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9580') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9581') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9584') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9595') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '9598') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9606') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '10389') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '0.01' WHERE (`Entry` = '10392') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '12191') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '12192') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '3.5' WHERE (`Entry` = '12275') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '12371') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '12372') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '12401') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0' WHERE (`Entry` = '13072') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13084') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '0' WHERE (`Entry` = '13085') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13102') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13105') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13106') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13107') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13144') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13145') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13383') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13581') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '13581') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '14050') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '14419') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '14864') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '8', `StatDamageScale` = '0' WHERE (`Entry` = '14900') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9', `StatDamageScale` = '0' WHERE (`Entry` = '14938') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '20102') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '20224') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '20436') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0' WHERE (`Entry` = '20518') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '27773') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '27773') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '1.5' WHERE (`Entry` = '27960') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL, `StatDamageScale` = '0.01' WHERE (`Entry` = '28434') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Still Updating Abilities
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '1436') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3' WHERE (`Entry` = '1906') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3' WHERE (`Entry` = '1932') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3' WHERE (`Entry` = '3878') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9' WHERE (`Entry` = '4626') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '5478') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9' WHERE (`Entry` = '5697') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '7111') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '8098') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '8347') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '9' WHERE (`Entry` = '8579') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '9176') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '9323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '9331') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '9333') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '9348') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

-- Normalized WeaponDamageScale to 1
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8406') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1747') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1768') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1834') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '2804') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8244') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9101') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9106') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9554') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9091') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '3054') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '3055') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '3283') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8320') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');

-- Purge Records
DELETE FROM `war_characters`.`gmcommandlogs`;
DELETE FROM war_characters.character_deletions;
DELETE FROM war_characters.scenario_durations;

-- The following are corrections to damages delt
-- Aligned per client side values
-- Format will be iMin/iMax TO oMin/oMax
-- E.g. 20/40 TO 40/80
#Witch Huner
-- Torment Min Change from 25 to 50
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8085') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Torment Max Change from 187 to 212
UPDATE `war_world`.`ability_damage_heals` SET `MaxDamage` = '212' WHERE (`Entry` = '8085') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Absolution 8082
-- ONE		35/262 TO 53/280
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '53', `MaxDamage` = '280' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- TWO		46/348 TO 66/368
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '66', `MaxDamage` = '368' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
-- THREE	58/437 TO 79/458
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '79', `MaxDamage` = '458' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
-- FOUR		70/524 TO 92/546
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '92', `MaxDamage` = '546' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
-- FIVE		81/610 TO 104/633
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '104', `MaxDamage` = '633' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
-- Blessed Bullets of Cleansing 8089
-- 45/337 TO 56/348
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '56', `MaxDamage` = '348' WHERE (`Entry` = '8089') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Blessed Bullets of Confession 8099
-- 45/337 TO 56/348
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '56', `MaxDamage` = '348' WHERE (`Entry` = '8099') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Blessed Bullets of Purity 8084
-- 45/337 TO 56/348
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '56', `MaxDamage` = '348' WHERE (`Entry` = '8084') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Burn Armor 8091
-- NO CHANGE
-- Burn Away Lies 8116
-- Ability_damage_heals 3921-3925
-- ONE DOT		114/855 TO 144/885
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '144', `MaxDamage` = '885' WHERE (`Entry` = '3921') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- ONE END		59/448 TO 69/458
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '69', `MaxDamage` = '458' WHERE (`Entry` = '3921') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
-- TWO DOT		150/1134 TO 180/1164
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '180', `MaxDamage` = '1164' WHERE (`Entry` = '3922') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- TWO END		79/596 TO 89/606
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '89', `MaxDamage` = '606' WHERE (`Entry` = '3922') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
-- THREE DOT	189/1416 TO 219/1446
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '219', `MaxDamage` = '1446' WHERE (`Entry` = '3923') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- THREE END	98/734 TO 108/744
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '108', `MaxDamage` = '744' WHERE (`Entry` = '3923') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
-- FOUR DOT		225/1698 TO 255/1728
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '255', `MaxDamage` = '1728' WHERE (`Entry` = '3924') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- FOUR END		117/876 TO 127/886
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '127', `MaxDamage` = '886' WHERE (`Entry` = '3924') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
-- FIVE DOT		264/1980 TO 294/2010
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '294', `MaxDamage` = '2010' WHERE (`Entry` = '3925') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- FIVE END		136/1021 TO 146/1031
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '146', `MaxDamage` = '1031' WHERE (`Entry` = '3925') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
-- Burn, Heretic! 8093
-- ONE		84/648 TO 114/678
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '114', `MaxDamage` = '678' WHERE (`Entry` = '8093') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- WARNING
-- NO ACU SCALING. IDK WHY
-- WARNING
-- Confess! 8086
-- 35/262 TO 61/288
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '61', `MaxDamage` = '288' WHERE (`Entry` = '8086') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Dragon Gun 8110
-- I dont know how this ability works so im not fucking with it
-- Exit Wound 8113
-- ONE		40/300 TO 58/318
-- TWO		53/399 TO 73/419
-- THREE	66/501 TO 87/522
-- FOUR		80/600 TO 102/622
-- FIVE		93/699 TO 116/722
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '58', `MaxDamage` = '318' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '73', `MaxDamage` = '419' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '87', `MaxDamage` = '522' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '102', `MaxDamage` = '622' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '116', `MaxDamage` = '722' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
-- Fervor DAMAGE 8083
-- 10/75 TO 28/93
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '28', `MaxDamage` = '93' WHERE (`Entry` = '8083') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Fervor DOT 3781
-- 33/246 TO 48/261
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '48', `MaxDamage` = '261' WHERE (`Entry` = '3781') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Pistol Whip 8115
-- 45/337 TO 71/363
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '71', `MaxDamage` = '363' WHERE (`Entry` = '8115') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Punish The False 8112
-- 30/225 TO 56/251
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '56', `MaxDamage` = '251' WHERE (`Entry` = '8112') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Razor Strike
-- 35/262 TO 61/288
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '61', `MaxDamage` = '288' WHERE (`Entry` = '8081') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Repel Blasphemy 8109
-- 45/337 TO 71/363
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '71', `MaxDamage` = '363' WHERE (`Entry` = '8109') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Seeker's Blade 8097
-- 35/262 TO 61/288
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '61', `MaxDamage` = '288' WHERE (`Entry` = '8097') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Sever Blessing 8101
-- NONE
-- Silence The Heretic 8100
-- 30/225 TO 56/251
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '56', `MaxDamage` = '251' WHERE (`Entry` = '8100') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
-- Snap Shot 8080
-- 10/75 TO 21/86
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '21', `MaxDamage` = '86' WHERE (`Entry` = '8080') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
#Witch Elf
-- Agnoizing Wound Min from 25 to 50
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9408') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Agnoizing Wound Max from 200 to 212
UPDATE `war_world`.`ability_damage_heals` SET `MaxDamage` = '212' WHERE (`Entry` = '9408') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Ironbreaker
-- Axe Toss 1352
-- 10/75 TO 21/86
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '21', `MaxDamage` = '86' WHERE (`Entry` = '1352') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Heavy Blow 1354
-- Initial			20/150 TO 43/173
-- 25 Grudges		12/99 TO 57/144
-- 50 Grudges		18/147 TO 63/192
-- 75 Grudges		24/198 TO 69/243
-- 100 Grudges		33/246 TO 78/291
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '43' WHERE (`Entry` = '1354') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '57', `MaxDamage` = '144' WHERE (`Entry` = '1354') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '57', `MaxDamage` = '144' WHERE (`Entry` = '1354') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '63', `MaxDamage` = '192' WHERE (`Entry` = '1354') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '69', `MaxDamage` = '243' WHERE (`Entry` = '1354') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '78', `MaxDamage` = '291' WHERE (`Entry` = '1354') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
-- Grudging Blow 1355
-- 15/112 TO 38/135
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '38', `MaxDamage` = '135' WHERE (`Entry` = '1355') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Guarded Attack
-- 15/112 TO 38/135
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '38', `MaxDamage` = '135' WHERE (`Entry` = '1356') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Vengeful Strike 1357
-- 15/112 TO 38/135
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '38', `MaxDamage` = '135' WHERE (`Entry` = '1357') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Binding Grudge 1358
-- Initial			30/250 TO 70/290
-- 25 Grudges		50/405 TO 90/445
-- 50 Grudges		70/540 TO 110/580
-- 75 Grudges		90/690 TO 135/735
-- 100 Grudges		110/840 TO 155/885
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '70', `MaxDamage` = '290' WHERE (`Entry` = '1358') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '90', `MaxDamage` = '445' WHERE (`Entry` = '1358') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '110', `MaxDamage` = '580' WHERE (`Entry` = '1358') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '135', `MaxDamage` = '735' WHERE (`Entry` = '1358') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '155', `MaxDamage` = '885' WHERE (`Entry` = '1358') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
-- Punishing Blow 1359
-- Initial	10/75 TO 33/98
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '33', `MaxDamage` = '98' WHERE (`Entry` = '1359') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- DoT		NO CHANGE
-- Shield Sweep 1361
-- 20/150 TO 40/170
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40', `MaxDamage` = '170' WHERE (`Entry` = '1361') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
-- Kneecapper 1362
-- Initial			15/112 TO 38/135
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '38', `MaxDamage` = '135' WHERE (`Entry` = '1362') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Inspiring Attack 1364
-- 20/150 TO 43/173
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '43', `MaxDamage` = '173' WHERE (`Entry` = '1364') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Away With Ye 1365
-- 40/300 TO 63/323
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '63', `MaxDamage` = '323' WHERE (`Entry` = '1365') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Rune-Etched Axe 1366
-- Initial			15/122 TO 31/128
-- 25 Grudges		15/122 TO 31/128
-- 50 Grudges		15/122 TO 31/128
-- 75 Grudges		15/122 TO 31/128
-- 100 Grudges		15/122 TO 31/128
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '31', `MaxDamage` = '128' WHERE (`Entry` = '1366') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '31', `MaxDamage` = '128' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '31', `MaxDamage` = '128' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '31', `MaxDamage` = '128' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '31', `MaxDamage` = '128' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
-- Shield of Reprisal 1369
-- 30/225 TO 53/248
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '53', `MaxDamage` = '248' WHERE (`Entry` = '1369') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Stone Breaker 1371
-- 20/150 TO 43/173
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '43', `MaxDamage` = '173' WHERE (`Entry` = '1371') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Sever Blessing 1374
-- 10/75 TO 33/98
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '33', `MaxDamage` = '98' WHERE (`Entry` = '1374') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
-- Avenging the Debt 1381
-- Damage	30/225 TO 53/248
-- Heal		NO CHANGE
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '53', `MaxDamage` = '248' WHERE (`Entry` = '1381') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Cave-In 1384
-- Initial			25/185 TO 40/200
-- 25 Grudges		25/185 TO 40/200
-- 50 Grudges		50/370 TO 65/385
-- 75 Grudges		75/560 TO 90/575
-- 100 Grudges		100/745 TO 115/760
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40', `MaxDamage` = '200' WHERE (`Entry` = '1384') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40', `MaxDamage` = '200' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '65', `MaxDamage` = '385' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '90', `MaxDamage` = '575' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '115', `MaxDamage` = '760' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
-- Grudge-Born Fury 1385
-- 15/112 TO 150/635
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '150', `MaxDamage` = '635' WHERE (`Entry` = '1385') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Withering Blow 8328
-- 15/112 TO 38/135
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '38', `MaxDamage` = '135' WHERE (`Entry` = '8328') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Touch of Palsy 8338
-- Inital	10/75 TO 33/98
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.01' WHERE (`Entry` = '8338') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- DoT		NO CHANGE
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '33', `MaxDamage` = '98' WHERE (`Entry` = '8338') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Tooth of Tzeentch 8317
-- 20/150 TO 43/173
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '43', `MaxDamage` = '173' WHERE (`Entry` = '8317') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Tooth of Tzeentch 3787
-- NO CHANGE
-- Throwing Axe 8314
-- 10/75 TO 22/87
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '22', `MaxDamage` = '87' WHERE (`Entry` = '8314') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Tainted Wound 8368
-- NO CHANGE
-- Suppression 8335
-- 15/112 TO 38/135
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '38', `MaxDamage` = '135' WHERE (`Entry` = '8335') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Siphoned Energy 8367
-- Damage No reason where there is a value here
-- Healing 18/135 TO 21/157
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '21', `MaxDamage` = '157' WHERE (`Entry` = '8367') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
-- Sever Blessing 8339
-- 10/75 TO 33/89
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '33', `MaxDamage` = '89' WHERE (`Entry` = '8339') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
-- Seeping Wound 8320
-- PARENT 0 33/252 TO 72/291
-- PARENT 1 45/337 TO 72/291
-- MADE TACTIC NOT WORTHLESS. NOT 148 BUT W/E
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '72', `MaxDamage` = '291' WHERE (`Entry` = '8320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '72', `MaxDamage` = '291' WHERE (`Entry` = '8320') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
-- Repel 8329
-- 20/150 TO 43/173
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '43', `MaxDamage` = '173' WHERE (`Entry` = '8329') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Rending Blade 8344
-- NO CHANGE
-- Relentless 8343
-- 37/281 TO 29/146
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '29', `MaxDamage` = '146' WHERE (`Entry` = '8343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Ravage 8323
-- 20/150 TO 43/173
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '43', `MaxDamage` = '173' WHERE (`Entry` = '8323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Quake 8349
-- 25/187 TO 33/195
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '33', `MaxDamage` = '195' WHERE (`Entry` = '8349') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Oppression 8347
-- NO CHANGE
-- Enraged Blow 8315
-- 15/112 TO 38/135
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '38', `MaxDamage` = '135' WHERE (`Entry` = '8315') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Downfall 8346
-- 20/150 TO 43/173
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '43', `MaxDamage` = '173' WHERE (`Entry` = '8346') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Dizzying Blow 8324
-- 15/112 TO 38/135
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '38', `MaxDamage` = '135' WHERE (`Entry` = '8324') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Discordant Instability 8321
-- BUFF_COMMANDS
-- MINUS 4/MINUS 378 TO MINUS 4/MINUS 188
UPDATE `war_world`.`buff_commands` SET `TertiaryValue` = '-188' WHERE (`Entry` = '8321') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_commands` SET `TertiaryValue` = '-188' WHERE (`Entry` = '8321') and (`CommandID` = '1') and (`CommandSequence` = '1');
UPDATE `war_world`.`buff_commands` SET `TertiaryValue` = '-188' WHERE (`Entry` = '8321') and (`CommandID` = '1') and (`CommandSequence` = '2');
-- Discordant Fluctuation 8327
-- NO CHANGE
-- Corrupting Retribution 3411
-- NO CHANGE
-- Sprout Carapace 8373
-- Adding missing AP bonus
INSERT INTO `war_world`.`buff_commands` (`Entry`, `Name`, `CommandID`, `CommandSequence`, `CommandName`, `PrimaryValue`, `InvokeOn`, `Target`) VALUES ('8373', 'Sprout Carapace', '0', '4', 'ModifyStat', '100', '1', 'Group');
-- Cleave 8319
-- 20/150 TO 43/173
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '43', `MaxDamage` = '173' WHERE (`Entry` = '8319') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- Blast Wave 8331
-- 10/75 TO 24/89
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '24', `MaxDamage` = '89' WHERE (`Entry` = '8331') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '24', `MaxDamage` = '89' WHERE (`Entry` = '8331') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
#Weapon Scale Change BASELINE
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1528') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '3221') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1689') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '2810') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9026') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8031') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1385') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1538') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8026') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '14900') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8038') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8349') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8419') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '8419') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9006') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9006') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9028') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9028') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9030') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9030') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9428') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '13581') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '13581') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9032') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
--
--
UPDATE `war_world`.`ability_damage_heals` SET `CastTimeDamageMult` = '1';
#Ravage
-- Delay Change 500 to 0
UPDATE `war_world`.`abilities` SET `InvokeDelay` = '0	' WHERE (`Entry` = '8323');
-- StatScale Change 2.0 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- MinDamage to 20/NULL based on changes to source.
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20', `MaxDamage` = NULL WHERE (`Entry` = '8323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
--
-- ABILITY CHANGES
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '386') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '387') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '388') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '389') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '390') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '391') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '392') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
--
--
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '2') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '5') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '11') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '11') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '12') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '12') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '21') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '22') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '24') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '27') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '46') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '55') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '56') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '58') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '59') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '59') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '420') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '32') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '528') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '1352') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '1354') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1354') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1354') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '1354') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '1354') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1355') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1356') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1357') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '1358') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1358') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1358') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1358') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1358') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '1359') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '1359') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1361') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1362') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1364') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1365') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1366') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1366') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1369') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1371') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '1374') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
--
--
UPDATE `war_world`.`ability_damage_heals` SET `MaxDamage` = NULL;
UPDATE `war_world`.`ability_damage_heals` SET `DamageVariance` = NULL;
--
--
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '424') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '441') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '441') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '421') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '445') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '447') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '840') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '12') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '422') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '440') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '440') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '442') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '448') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '448') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '776') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3586' WHERE (`Entry` = '779') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3595', `MinDamage` = '25' WHERE (`Entry` = '800') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3260' WHERE (`Entry` = '840') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3260') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '840') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1354') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '0' WHERE (`Entry` = '1354') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3353' WHERE (`Entry` = '1359') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1359') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1381') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1381') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '0' WHERE (`Entry` = '1384') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '75' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '100' WHERE (`Entry` = '1384') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '1385') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '75' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '100' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1388') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3758', `StatDamageScale` = '.01' WHERE (`Entry` = '1406') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3829', `MinDamage` = '30', `StatDamageScale` = '1.5' WHERE (`Entry` = '1409') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1426') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1435') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3313', `MinDamage` = '35' WHERE (`Entry` = '1436') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '1460') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
--
--
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '1508') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1510') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1513') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '1514') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1515') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1521') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '31', `MinDamage` = '30' WHERE (`Entry` = '1524') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1536') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '1537') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '1538') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1539') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1539') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1539') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1540') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3072', `MinDamage` = '30' WHERE (`Entry` = '1543') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1576') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1586') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1587') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3868', `MinDamage` = '40' WHERE (`Entry` = '1588') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1589') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1590') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '1656' WHERE (`Entry` = '1591') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '47', `MinDamage` = '50' WHERE (`Entry` = '1593') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1593') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1596') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1597') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1598') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3551', `Index` = '0', `MinDamage` = '50' WHERE (`Entry` = '1599') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = NULL WHERE (`Entry` = '1593') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3870', `MinDamage` = '50' WHERE (`Entry` = '1600') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1604') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '1607') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3221') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3600') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3612') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '3613') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3621') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '3811') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '3928') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8165') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8169') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8174') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8176') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8181') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8185') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8189') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '8173' WHERE (`Entry` = '8204') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '8165', `MinDamage` = '50' WHERE (`Entry` = '8207') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8407') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8548') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8553') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3612', `MinDamage` = '50' WHERE (`Entry` = '8556') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3859', `MinDamage` = '40' WHERE (`Entry` = '8560') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8563') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3212') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '3338') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3461') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '3739') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '3757') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3913') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3915') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '3939') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3968') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '3995') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '100' WHERE (`Entry` = '4338') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '0' WHERE (`Entry` = '4811') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '0' WHERE (`Entry` = '5450') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '21563', `MinDamage` = '30000' WHERE (`Entry` = '5697') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9557') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8247') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1655') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8414') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9250') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9237') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9123') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9082') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8576') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1903') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1899') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1614') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9258') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '9581') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9499') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '9409') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3181', `MinDamage` = '50' WHERE (`Entry` = '9269') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8182') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3173', `MinDamage` = '50' WHERE (`Entry` = '1933') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1836') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '80' WHERE (`Entry` = '20375') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '9562', `MinDamage` = '50' WHERE (`Entry` = '9600') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '8252', `MinDamage` = '50' WHERE (`Entry` = '8292') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8112') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8100') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3002', `MinDamage` = '45' WHERE (`Entry` = '8099') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3783', `MinDamage` = '45' WHERE (`Entry` = '8089') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3782', `MinDamage` = '45' WHERE (`Entry` = '8084') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8377') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '3940') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3983', `MinDamage` = '40' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '9505') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '9475') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8502') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '1931') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '10703') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3739', `MinDamage` = '45' WHERE (`Entry` = '9584') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '9480') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '9480') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8565') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8565') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8501') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8183') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '6' WHERE (`Entry` = '8070') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1688') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3827', `MinDamage` = '20' WHERE (`Entry` = '1382') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8097') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8086') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8081') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3338', `MinDamage` = '30' WHERE (`Entry` = '9045') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3231', `MinDamage` = '50' WHERE (`Entry` = '9020') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3836', `MinDamage` = '0' WHERE (`Entry` = '8574') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8497') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8188') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1858') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3846', `MinDamage` = '30' WHERE (`Entry` = '1700') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8271') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageVariance` = '0' WHERE (`Entry` = '1447') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageVariance` = '0' WHERE (`Entry` = '1759') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageVariance` = '0' WHERE (`Entry` = '1435') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageVariance` = '0' WHERE (`Entry` = '1747') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageVariance` = '0' WHERE (`Entry` = '1613') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3988', `MinDamage` = '25' WHERE (`Entry` = '9428') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3931', `MinDamage` = '25' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9106') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3161', `MinDamage` = '15' WHERE (`Entry` = '1747') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '10780') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8494') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8471') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8087') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '3320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '3320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '8012') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3988', `MinDamage` = '25' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3931', `MinDamage` = '25' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '9162', `MinDamage` = '20' WHERE (`Entry` = '9216') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '9162') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8087') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '3951') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '3381') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1863') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1863') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3539', `MinDamage` = '35' WHERE (`Entry` = '9030') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3539', `MinDamage` = '35' WHERE (`Entry` = '9030') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3541', `MinDamage` = '35' WHERE (`Entry` = '9028') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3541', `MinDamage` = '35' WHERE (`Entry` = '9028') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3227', `MinDamage` = '30' WHERE (`Entry` = '8020') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '3956') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '2824') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
DELETE FROM `war_world`.`ability_damage_heals` WHERE (`Entry` = '2825') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '3930') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '3310') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '3290') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '3227') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '6' WHERE (`Entry` = '13660') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '13589') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '8334') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '8022') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '3490') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1776') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3988', `MinDamage` = '25' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8082') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9501') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8501') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '3595') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '9257') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '9257') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1904') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9342') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8331') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8331') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '3281') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8007') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '3306') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3909') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9245') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8573') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '95' WHERE (`Entry` = '5244') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '72' WHERE (`Entry` = '5156') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1965') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1965') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8248') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '7875') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '7880') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '7885') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '125' WHERE (`Entry` = '7221') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '360' WHERE (`Entry` = '7261') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '14031') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9084') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '8116') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3921') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3921') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3922') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3922') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3923') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3923') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3924') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3924') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3925') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3921', `MinDamage` = '50' WHERE (`Entry` = '3925') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '5243') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '1683', `MinDamage` = '50' WHERE (`Entry` = '1704') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8406') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '3411') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '325' WHERE (`Entry` = '5261') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3283', `MinDamage` = '35' WHERE (`Entry` = '1748') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '1821') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1822') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1829') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1847') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1848') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '1840') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '1846') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1853') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '1854') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '18' WHERE (`Entry` = '1858') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '1833', `MinDamage` = '30' WHERE (`Entry` = '1861') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1905') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1907') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '1951') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8003') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8080') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8367') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8367') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8314') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '3534') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '3901') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '3961') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8425') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8564') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3547', `MinDamage` = '20' WHERE (`Entry` = '9271') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '9500') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '9500') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8266') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8266') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '9579') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '9579') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8083') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9399') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8343') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9142') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9404') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9410') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9428') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8497') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8497') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3470', `MinDamage` = '50' WHERE (`Entry` = '1639') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '9479') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9381') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1425') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '3950') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '5244') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3959' WHERE (`Entry` = '3959') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3959', `MinDamage` = '30' WHERE (`Entry` = '9193') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8093') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3479', `MinDamage` = '35' WHERE (`Entry` = '8327') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3274') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9226') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3911', `MinDamage` = '50' WHERE (`Entry` = '1932') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3003', `MinDamage` = '50' WHERE (`Entry` = '8127') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9029') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8115') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8265') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8495') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9144') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9144') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8304') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8304') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9544') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '5142') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8489') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '65' WHERE (`Entry` = '4383') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '65' WHERE (`Entry` = '4383') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9138') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8347') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8069') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9164') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8031') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8486') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '125' WHERE (`Entry` = '13085') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '75' WHERE (`Entry` = '5236') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8575') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '10389') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '8' WHERE (`Entry` = '5230') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '8' WHERE (`Entry` = '5230') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '8' WHERE (`Entry` = '5230') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '8' WHERE (`Entry` = '5230') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '1618') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '55' WHERE (`Entry` = '12193') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9272') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9272') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8215') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8215') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '670') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1883') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1883') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1501') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '1794') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '9167', `MinDamage` = '50' WHERE (`Entry` = '9209') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9167') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '9100') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3907') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3998') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8557') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9386') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9573') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3427', `MinDamage` = '50' WHERE (`Entry` = '9595') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '1680', `MinDamage` = '30' WHERE (`Entry` = '1711') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3194' WHERE (`Entry` = '1795') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3194' WHERE (`Entry` = '1795') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '450' WHERE (`Entry` = '5241') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `Name` = 'Cleave', `MinDamage` = '45' WHERE (`Entry` = '5549') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '100' WHERE (`Entry` = '5598') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '250' WHERE (`Entry` = '5214') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '5214') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '5413') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '5413') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '5414') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '5414') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '8' WHERE (`Entry` = '4608') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '21283') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '12402') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '14945') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '14945') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '13682') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '1000' WHERE (`Entry` = '14381') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '1500' WHERE (`Entry` = '14050') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '6000' WHERE (`Entry` = '4624') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '400' WHERE (`Entry` = '13587') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '450' WHERE (`Entry` = '13060') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '450' WHERE (`Entry` = '13060') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '450' WHERE (`Entry` = '13071') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '450' WHERE (`Entry` = '13071') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '450' WHERE (`Entry` = '13074') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '450' WHERE (`Entry` = '13097') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '450' WHERE (`Entry` = '13097') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '435' WHERE (`Entry` = '5305') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '400' WHERE (`Entry` = '5232') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '345' WHERE (`Entry` = '5238') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '300' WHERE (`Entry` = '4980') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '300' WHERE (`Entry` = '23012') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '180' WHERE (`Entry` = '13563') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '300' WHERE (`Entry` = '5458') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '300' WHERE (`Entry` = '13527') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '300' WHERE (`Entry` = '13565') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '995' WHERE (`Entry` = '5008') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '976' WHERE (`Entry` = '13108') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '345' WHERE (`Entry` = '5025') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '945' WHERE (`Entry` = '13084') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '925' WHERE (`Entry` = '13081') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '550' WHERE (`Entry` = '5467') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '550' WHERE (`Entry` = '13064') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '5347') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '5700') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '12266') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '12371') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '12401') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '13091') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '500' WHERE (`Entry` = '23741') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '385' WHERE (`Entry` = '13078') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '325' WHERE (`Entry` = '5263') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '325' WHERE (`Entry` = '13107') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '325' WHERE (`Entry` = '13145') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '250' WHERE (`Entry` = '5214') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '4609') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '4980') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '5036') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '5048') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '5129') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '12372') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '13008') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '13563') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '13600') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '23012') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '195' WHERE (`Entry` = '13098') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '175' WHERE (`Entry` = '13102') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '175' WHERE (`Entry` = '13105') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '152' WHERE (`Entry` = '5237') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '150' WHERE (`Entry` = '5307') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '150' WHERE (`Entry` = '5413') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '150' WHERE (`Entry` = '13074') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '125' WHERE (`Entry` = '5414') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '8' WHERE (`Entry` = '4606') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '4608') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '5459') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '5598') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '5700') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '13079') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '14948') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '200' WHERE (`Entry` = '23734') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '995' WHERE (`Entry` = '5006') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '995' WHERE (`Entry` = '5233') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '606') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '627') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '648') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '671') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '672') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '690') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '692') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '692') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '694') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '696') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1420') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1494') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1495') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1580') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1581') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1582') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '1611') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1616') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '1617') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3878', `MinDamage` = '50' WHERE (`Entry` = '1627') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1645') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1645') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3001', `MinDamage` = '40' WHERE (`Entry` = '1648') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1649') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1650') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1657') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1689') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1722') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1727') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1728') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '1729') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1909') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '60' WHERE (`Entry` = '1911') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '1928') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '1929') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '1929') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1930') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1930') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3172', `MinDamage` = '30' WHERE (`Entry` = '1935') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3172', `MinDamage` = '30' WHERE (`Entry` = '1935') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '1935') and (`Index` = '0') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3219', `MinDamage` = '50' WHERE (`Entry` = '1945') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3045') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3050') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3197') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '3283') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3421') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3878') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3908') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3914') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '8' WHERE (`Entry` = '4606') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8114') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8443') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8302') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '2813') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '3896') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3400', `MinDamage` = '10' WHERE (`Entry` = '8338') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8339') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8349') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '9103') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8500') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8558') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8578') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8578') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '9026') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9171') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3956', `MinDamage` = '15' WHERE (`Entry` = '9172') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9266') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9266') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9478') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '9489') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '10' WHERE (`Entry` = '8101') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8344') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '2810') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '8315') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '8324') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '8328') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '15' WHERE (`Entry` = '8335') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '3819') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '8498') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '383', `MinDamage` = '30' WHERE (`Entry` = '8541') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3175', `MinDamage` = '30' WHERE (`Entry` = '9274') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3175', `MinDamage` = '30' WHERE (`Entry` = '9274') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '9502') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '9502') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '25701') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '30' WHERE (`Entry` = '25703') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8569') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8317') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8319') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8329') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '20' WHERE (`Entry` = '8346') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '35' WHERE (`Entry` = '8470') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '3781') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8239') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '9348') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9553') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '45' WHERE (`Entry` = '8109') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8158') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '25' WHERE (`Entry` = '8320') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3461', `MinDamage` = '50' WHERE (`Entry` = '9047') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9091') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9109') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9471') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3983', `MinDamage` = '40' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8417') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '9486') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '9' WHERE (`Entry` = '12192') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '14900') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '3016') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3983', `MinDamage` = '40' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3143', `MinDamage` = '50' WHERE (`Entry` = '9277') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8487') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '9088', `MinDamage` = '50' WHERE (`Entry` = '9119') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '9475', `MinDamage` = '50' WHERE (`Entry` = '9523') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3142', `MinDamage` = '50' WHERE (`Entry` = '8288') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3076', `MinDamage` = '50' WHERE (`Entry` = '8602') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3004', `MinDamage` = '50' WHERE (`Entry` = '9448') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '50' WHERE (`Entry` = '8562') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '125' WHERE (`Entry` = '21421') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3197', `MinDamage` = '50' WHERE (`Entry` = '9068') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '40' WHERE (`Entry` = '8527') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3050', `MinDamage` = '50' WHERE (`Entry` = '8394') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '550' WHERE (`Entry` = '9405') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3983', `MinDamage` = '40' WHERE (`Entry` = '8113') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '44' WHERE (`Entry` = '10531') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3045', `MinDamage` = '50' WHERE (`Entry` = '8403') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `MinDamage` = '44' WHERE (`Entry` = '10553') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
--
-- CHANGE 17
ALTER TABLE `war_world`.`ability_damage_heals` DROP COLUMN `MaxDamage`;
ALTER TABLE `war_world`.`ability_damage_heals` RENAME COLUMN `MinDamage` TO `BaseDamage`;
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1821') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
-- CHANGE 18
--
ALTER TABLE `war_world`.`ability_damage_heals` ADD `Multiplier` int(16);
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '100';
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '133' WHERE (`Entry` = '1821') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
ALTER TABLE `war_world`.`ability_damage_heals` 
CHANGE COLUMN `BaseDamage` `BaseDamage` SMALLINT(5) UNSIGNED NULL DEFAULT 0 ,
CHANGE COLUMN `Multiplier` `Multiplier` SMALLINT(5) UNSIGNED NULL DEFAULT 100 ;
ALTER TABLE `war_world`.`ability_damage_heals` DROP `CastTimeDamageMult`;
ALTER TABLE `war_world`.`ability_damage_heals` DROP `PriStatMultiplier`;
--
-- change 19
UPDATE `war_world`.`ability_commands` SET `Target` = 'Caster' WHERE (`Entry` = '1420') and (`CommandID` = '0') and (`CommandSequence` = '0');
UPDATE `war_world`.`buff_infos` SET `Duration` = '10' WHERE (`Entry` = '9616');
--
-- change 20
ALTER TABLE `war_world`.`ability_damage_heals` DROP `DamageVariance`;
ALTER TABLE `war_world`.`ability_damage_heals` DROP `WeaponDamageScale`;
--
-- Combat_insert.sql
#Detonation
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `BaseDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `StatDamageScale`, `HatredScale`, `HealHatredScale`, `Multiplier`) VALUES ('31', '0', 'Detonation', '30', 'Physical', '0', '0', '1', '1', '1', '133');
INSERT INTO `war_world`.`abilities` (`Entry`, `CareerLine`, `Name`, `EffectID`, `AbilityType`, `WeaponNeeded`) VALUES ('31', '0', 'Detonation', '0', '255', '0');

#Warped Claw
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `BaseDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `StatUsed`, `StatDamageScale`, `Multiplier`) VALUES ('60', '0', 'Warped Claw', '40', 'Elemental', '0', '0', '9', '1.5', '266');

#Breath of Change
INSERT INTO `war_world`.`ability_damage_heals` (`Entry`, `Index`, `Name`, `BaseDamage`, `DamageType`, `ParentCommandID`, `ParentCommandSequence`, `StatUsed`, `StatDamageScale`, `HatredScale`, `HealHatredScale`, `Multiplier`) VALUES ('61', '0', 'Breath of Change', '40', 'Elemental', '0', '0', '9', '1.5', '1', '1', '266');
--
-- Combat_rev.sql
#Seeping Wound Changes
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '3358', `BaseDamage` = '45', `StatDamageScale` = '0.01' WHERE (`Entry` = '8320') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1', `Multiplier` = '45' WHERE (`Entry` = '8320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Poison Arrer Multiplier from 100 to 200
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '200' WHERE (`Entry` = '1847') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Lots o Arrers
UPDATE `war_world`.`ability_damage_heals` SET `BaseDamage` = '30', `Multiplier` = '50' WHERE (`Entry` = '1834') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Cut Ya
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.9', `Multiplier` = '30' WHERE (`Entry` = '1829') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Yer Bleedin
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.9', `Multiplier` = '38' WHERE (`Entry` = '1822') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Big Claw
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1', `Multiplier` = '44' WHERE (`Entry` = '2') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Poisoned Spine
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '8', `BaseDamage` = '20', `StatDamageScale` = '0.45', `Multiplier` = '27' WHERE (`Entry` = '20102') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Spore Cloud 12
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '3', `StatDamageScale` = '1.5', `Multiplier` = '80' WHERE (`Entry` = '12') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '133' WHERE (`Entry` = '12') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '133' WHERE (`Entry` = '12') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');

#Squig Squeal
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '17', `BaseDamage` = '30', `HatredScale` = '6' WHERE (`Entry` = '442') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Git em
UPDATE `war_world`.`ability_damage_heals` SET `DisplayEntry` = '18' WHERE (`Entry` = '1824') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

#Penetrating Round
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '133' WHERE (`Entry` = '21') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Machine Gun
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '50' WHERE (`Entry` = '22') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Shock Grenade
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '133' WHERE (`Entry` = '24') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Flamethrower
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '133' WHERE (`Entry` = '27') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Bugmans Brew
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '133' WHERE (`Entry` = '32') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Leg Tear
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.9', `Multiplier` = '27' WHERE (`Entry` = '46') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `BaseDamage` = '35', `StatDamageScale` = '0.9', `Multiplier` = '27' WHERE (`Entry` = '3963') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Daemonic Consumption
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5', `Multiplier` = '22' WHERE (`Entry` = '55') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Warping Energy
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '133' WHERE (`Entry` = '56') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Flame Of Tzeentch
UPDATE `war_world`.`ability_damage_heals` SET `Multiplier` = '67' WHERE (`Entry` = '58') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Flames Of Change
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.56', `Multiplier` = '33' WHERE (`Entry` = '59') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.56', `Multiplier` = '33' WHERE (`Entry` = '59') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '448') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '448') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');

#Gift of Savagery
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.01', `Multiplier` = '45' WHERE (`Entry` = '3050') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '0.01', `Multiplier` = '45' WHERE (`Entry` = '8394') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Gift of Monstrosity
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'Healing', `StatUsed` = '1', `StatDamageScale` = '1.5', `Multiplier` = '46' WHERE (`Entry` = '3045') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1', `StatDamageScale` = '1.5', `Multiplier` = '46' WHERE (`Entry` = '8403') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Decay proc changes from Corp to CorpProc
#Changes Reflected in source too
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'CorporealProc' WHERE (`Entry` = '10067') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'CorporealProc' WHERE (`Entry` = '10068') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'CorporealProc' WHERE (`Entry` = '10069') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'CorporealProc' WHERE (`Entry` = '10070') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

#Concussion Physical to PhysicalProc
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'PhysicalProc' WHERE (`Entry` = '10057') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'PhysicalProc' WHERE (`Entry` = '10055') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `DamageType` = 'PhysicalProc' WHERE (`Entry` = '10058') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

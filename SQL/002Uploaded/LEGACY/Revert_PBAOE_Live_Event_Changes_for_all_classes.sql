	##Sorc Changes for event

##Black Horror update stat dmg from .35 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '.35' WHERE (`Entry` = '9506') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '.35' WHERE (`Entry` = '9506') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Update Disastrous Cascade from 2 to 2.4
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9503') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');


##Update Infernal Wave from 1.35 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.35' WHERE (`Entry` = '9488') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update pit of shades from 1.65 to 2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.65' WHERE (`Entry` = '9485') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Update pit of shades interval from 

## Update Ice Spikes from 2to 2.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9486') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

## Update surging pain from 1 to 2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9483') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update surging pain ap cost to 20 and specal cost to 0
UPDATE `war_world`.`abilities` SET `ApCost` = '0', `SpecialCost` = '20' WHERE (`Entry` = '9483');



	##BW Changes for event
	
##update scorched earth AP cost to 20 from 0 and special cost from 20 to 0
UPDATE `war_world`.`abilities` SET `ApCost` = '0', `SpecialCost` = '20' WHERE (`Entry` = '8163');


##Update Annihilate from 2 to 2.4
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '8187') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

## 	update spreading flames from 1.5 to 2.0
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8188') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update backdraft from 2 to 3
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '8189') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update Rain of Fire from 1.65 to 2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.65' WHERE (`Entry` = '8177') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update flame breath from 1.6 to 2.0
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.6' WHERE (`Entry` = '8171') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update flame breath to 0 CD
UPDATE `war_world`.`abilities` SET `Cooldown` = '10' WHERE (`Entry` = '8171');

##update fiery blast from 1.3 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.3' WHERE (`Entry` = '8166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.3' WHERE (`Entry` = '8166') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');

##Update fireball barage 1st tick from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8183') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update fireball barrage follow up ticks from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '.5' WHERE (`Entry` = '8183') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');

##Update scorched earth from 1 to 2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8163') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	## Engineer changes for event

#Update lightening rod from 0.9 to 1.4
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.9' WHERE (`Entry` = '1543') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update static discharge from 0.5 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1530') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update landmine from 0.66 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '.66' WHERE (`Entry` = '1524') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');

##update friction burn from 1.6 to 2.1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.6' WHERE (`Entry` = '1513') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update blunderbus blast from 0.8 to 1.8
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.8' WHERE (`Entry` = '1517') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update sticky bomb from 1.6 to 2.3 final tick
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.6' WHERE (`Entry` = '1539') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update Strafing run to use ballistic skill
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = NULL WHERE (`Entry` = '1540') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

## Update strafing run from 1 to 1.8
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1540') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update Napalm from 0.7 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.7' WHERE (`Entry` = '1537') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update fragmentation grenade from 1.9 to 2.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.9' WHERE (`Entry` = '1510') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update Firebomb damage from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1523') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update concussion grenade from from 1 to 1.4
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1531') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#Magus changes for event

##Update Agonizing Torrent from 0.33 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.33' WHERE (`Entry` = '8500') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update warpfire from from 0.75 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8485') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update Infernal blast from 1.6 to 2.0
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.6' WHERE (`Entry` = '8487') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update daemonic lash from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8472') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update daemonic infestation from 0.66 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.66' WHERE (`Entry` = '8541') and (`Index` = '1') and (`ParentCommandID` = '2') and (`ParentCommandSequence` = '0');

## Update Dissolving mist from 0.7 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.7' WHERE (`Entry` = '8494') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update warping blast from 1 to 1.4
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8483') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update pandemonium from 1.5 to 2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.5' WHERE (`Entry` = '8489') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update tzeentch's firestorm from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8498') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#Choppa changes for event
	
##update furious chopping from 1.25 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.25' WHERE (`Entry` = '1768') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');


##update lotsa choppin from 1.5 to 2
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1.5' WHERE (`Entry` = '1747') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update wot's da rush from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1774') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update git to da choppa from 1 to 1.8
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1776') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update yer all bleedin now from 1.0 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.0' WHERE (`Entry` = '3283') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#Slayer changes for event

##update no escape from 1.0 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.0' WHERE (`Entry` = '1463') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update Shatter Limbs from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1464') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update ID from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1465') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update ID from 0.25 to 0.75
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.25' WHERE (`Entry` = '3320') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');

##update wild swing from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1447') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update onslaught from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '3342') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update flurry from 1 to 1.6
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1435') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update rune of abs from 1 to 1.4
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '1457') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#Squig changes for event

##update indigestion from 0.8 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.8' WHERE (`Entry` = '1849') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update indigestion from 1 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '2814') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update bad gas from 0.5 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1850') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update big bouncin! from 0.5 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1851') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update kaboom! from 0.66 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.66' WHERE (`Entry` = '5') and (`Index` = '0') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

##update shoot thru from 0.66 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.66' WHERE (`Entry` = '1840') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update explodin arrer final tick from 0.5 to 1.0
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '3901') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#Shadow Warrior changes for event

##update sweeping slash from 1 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9107') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update barrage from 0.8 to 1.4
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.8' WHERE (`Entry` = '9111') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update lileth's arrow from 0.6 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.6' WHERE (`Entry` = '9100') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update flame arrow from 0.66 to 1.0
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.66' WHERE (`Entry` = '9088') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#chosen changes for event

##update quake from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8349') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update blast wave from 0.45 to 0.8
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.45' WHERE (`Entry` = '8331') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.45' WHERE (`Entry` = '8331') and (`Index` = '1') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');

##update rending blade from 1 to 1.45
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8344') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update rending blade effect angle from 140 to null
UPDATE `war_world`.`ability_commands` SET `EffectAngle` = '140' WHERE (`Entry` = '8344') and (`CommandID` = '0') and (`CommandSequence` = '0');

##update rending blade cooldown to 0 from 5
UPDATE `war_world`.`abilities` SET `Cooldown` = '5' WHERE (`Entry` = '8344');

	#White lion changes for event

##update echo from 2 to 2.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9187') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update slashing from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '9176') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#marauder changes for event

##update wave of mut from 0.95 to 1.25
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.95' WHERE (`Entry` = '8417') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update concussive jolt from 0.6 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.6' WHERE (`Entry` = '8423') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update wrecking ball from 1.16 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.16' WHERE (`Entry` = '8425') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update mouth of tzee from 0.75 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8397') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update demolition from 0.75 to 1.1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.75' WHERE (`Entry` = '8409') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#DOK changes for event

##update khaine's embrace from 2.15 to 3
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2.15' WHERE (`Entry` = '9557') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update devour essence from 1 to 1.6
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9578') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9578') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');

##update essence lash from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '9566') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update fell sac from 4 to 5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '4' WHERE (`Entry` = '9575') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#Warrior Priest changes for event

##update touch of divine from 2.15 to 3
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2.15' WHERE (`Entry` = '8247') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update smite from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8250') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update soulfire from 1.1 to 1.6
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1.1' WHERE (`Entry` = '8271') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#Zealot changes

##Update dust of pand from 2 to 3
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '8562') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');


	#Rune Priest changes

##update blessing of val from 2 to 3
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '3' WHERE (`Entry` = '1604') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');


	#AM Changes


##update blessing of isha from 1.34 to 3 ??
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '9245') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');


	#Shaman changes

##update gather round from 1.34 to 3??
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '2' WHERE (`Entry` = '1907') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#IB changes
	
##earthshatter to 0.9 from 0.5
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.5' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.5' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.5' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '0.5' WHERE (`Entry` = '1387') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');

	#Bo changes


##Waaaaaagh updated from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1695') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##big swing from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1672') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##da big from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '1678') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

	#BG changes

##Update wave of scorn to use str as primary stat LEAVE THIS IN
UPDATE `war_world`.`ability_damage_heals` SET `StatUsed` = '1' WHERE (`Entry` = '9348') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update wave of scorn 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9348') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update crimson death from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9344') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update monstrous rending for 0.66 to 1.2
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.66' WHERE (`Entry` = '9323') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');


	#SM changes

##update wrath of hoeth from 0.45 to 0.75
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.45' WHERE (`Entry` = '9017') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update phoenix's wing from 0.57 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.57' WHERE (`Entry` = '9025') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');


	#Wh

##update razor strike from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8081') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');



	# WE changes

##update on your knees to 1
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update on your knees from 0.67 to 1
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '1');

##update on your knees from 0.84 to 1.3
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '2');

##update on your knees from 1 to 1.6
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');

##update on your knees from 1.16 to 2
UPDATE `war_world`.`ability_damage_heals` SET `WeaponDamageScale` = '1' WHERE (`Entry` = '9422') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');

##update slice damage from 1 to 1.5
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '9398') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##Update ONK primary stat contribution




	#KOTBS

##update heaven's fury from 0.5 to 1
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '0.5' WHERE (`Entry` = '8038') and (`Index` = '0') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');

##update staggering impact from 1 to 1.6
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8032') and (`Index` = '1') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '0');


	#Magus rift update

##Magus rift cap set to 24
UPDATE `war_world`.`buff_commands` SET `MaxTargets` = '12' WHERE (`Entry` = '36') and (`CommandID` = '0') and (`CommandSequence` = '0');


##Engi Magnet UPDATE

##Engi Magnet cap set to 24
UPDATE `war_world`.`buff_commands` SET `MaxTargets` = '12' WHERE (`Entry` = '4') and (`CommandID` = '0') and (`CommandSequence` = '0');

##update dragon gun from 0.5 to 1.4
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '0') and (`ParentCommandSequence` = '4');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '0');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '1');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '2');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '3');
UPDATE `war_world`.`ability_damage_heals` SET `StatDamageScale` = '1' WHERE (`Entry` = '8110') and (`Index` = '2') and (`ParentCommandID` = '1') and (`ParentCommandSequence` = '4');

##Sorc changes

##update sorc pit of shades proc rate from 1500 to 900
UPDATE `war_world`.`buff_infos` SET `Interval` = '1500' WHERE (`Entry` = '9485');



##BW changes

##update BW Rain of fire proc rate from 1500 to 900

UPDATE `war_world`.`buff_infos` SET `Interval` = '1500' WHERE (`Entry` = '8177');##Sorc changes






-- SORC AND BW PBAOE CAREER RESOURCE CHANGE FOR EVENT

UPDATE `war_world`.`ability_commands` SET `PrimaryValue` = '20' WHERE (`Entry` = '8163') and (`CommandID` = '1') and (`CommandSequence` = '0');
UPDATE `war_world`.`ability_commands` SET `PrimaryValue` = '20' WHERE (`Entry` = '9483') and (`CommandID` = '1') and (`CommandSequence` = '0');













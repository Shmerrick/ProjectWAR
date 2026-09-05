-- 36_restore_gunbad_retail_levels.sql
--
-- Reverts the Return of Reckoning rescale of Mount Gunbad, and repairs four groups of
-- public-quest spawn rows whose Objective and Entry columns are transposed.
--
-- Authority: the official 1.4.8 capture
-- WAR-RE-Toolkit/libs/protocolservices/Packet Logs/INSTANCE_GUNBAD_PART1.txt.gz,
-- 11,516 F_CREATE_MONSTER frames. Packet ordinals are 1-based across both directions and
-- are reproducible with tools/validation/Read-OfficialPackets.ps1. F_CREATE_MONSTER payload
-- offsets read here: +0 OID, +6 Z, +8 client X, +12 client Y, +21 level, +44 state-block
-- length, then the NUL-terminated name.
--
-- Mount Gunbad had been levelled as a rank-40 dungeon: every creature 40-42, its four wing
-- bosses 41-44, and all 723 of its public-quest spawn rows at 42. The capture shows the
-- dungeon ran at ranks 21-33. Each level below is the single distinct level observed for
-- that creature across the capture, with the observation count recorded so the claim can be
-- rechecked. Creatures the capture does not contain are left alone rather than guessed at.
--
-- Placement is deliberately NOT changed. Converting the capture's client coordinates back to
-- world coordinates for zone 60 -- OffX/OffY 200/200 with the (1,9) instance atlas shift from
-- S_PLAYER_INITTED #286846, world = client - (shift << 13) + (Off << 12) -- reproduces the
-- rows already present: 460 of 525 instance spawns sit within 50 units of a same-creature
-- sighting, and 360 of 398 independently reconstructed public-quest spawn points fall within
-- 150 units of the pquest_spawns row already in the database. Gunbad's layout already matches
-- the live server; only its levels did not. (This corrects the BUG-058 note that an
-- exact-placement audit found no matches, which was measured without the atlas shift.)
--
-- Idempotent: every statement is a fixed assignment.

USE `war_world`;

-- 1. Creature levels for the dungeon population (instance_creature_spawns).
--
--    instance_creature_spawns.Level takes precedence over the prototype range
--    (Creature.cs: else if (Spawn.Level != 0) Level = Spawn.Level), so these are scoped to
--    Gunbad and cannot change a creature anywhere else.

UPDATE instance_creature_spawns SET Level = 21 WHERE ZoneID = 60 AND Entry = 37548; -- Kezzen (1 row(s), 27 obs)
UPDATE instance_creature_spawns SET Level = 23 WHERE ZoneID = 60 AND Entry = 35010; -- Blackfang Recluse (16 row(s), 228 obs)
UPDATE instance_creature_spawns SET Level = 23 WHERE ZoneID = 60 AND Entry = 36624; -- Blarpot the Old (1 row(s), 6 obs)
UPDATE instance_creature_spawns SET Level = 23 WHERE ZoneID = 60 AND Entry = 36608; -- Brood Mother Szikalax (1 row(s), 25 obs)
UPDATE instance_creature_spawns SET Level = 23 WHERE ZoneID = 60 AND Entry = 36619; -- Crazed Collecta Griznik (3 row(s), 46 obs)
UPDATE instance_creature_spawns SET Level = 23 WHERE ZoneID = 60 AND Entry = 34998; -- Redeye Mushroomist (11 row(s), 159 obs)
UPDATE instance_creature_spawns SET Level = 23 WHERE ZoneID = 60 AND Entry = 34999; -- Redeye Reapa (19 row(s), 282 obs)
UPDATE instance_creature_spawns SET Level = 23 WHERE ZoneID = 60 AND Entry = 36597; -- Young Blackfang Huntsman (10 row(s), 142 obs)
UPDATE instance_creature_spawns SET Level = 24 WHERE ZoneID = 60 AND Entry = 36622; -- Redeye Alchemist (9 row(s), 50 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 36598; -- Chipfang da Lit'l (1 row(s), 4 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 36551; -- Crystalspine Wyvern (4 row(s), 119 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 36552; -- Pestilent Tendril (10 row(s), 30 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 36553; -- Redeye Dreadrida (9 row(s), 136 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 36546; -- Redeye Squig Herda (43 row(s), 193 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 35007; -- Redeye Squig Traina (7 row(s), 72 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 38235; -- Squigling (17 row(s), 84 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 123551; -- Swarmin' Lit'l Squigling (45 row(s), 466 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 36550; -- Toof Maw (1 row(s), 2 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 36548; -- Young Crystalspine Wyvern (13 row(s), 622 obs)
UPDATE instance_creature_spawns SET Level = 26 WHERE ZoneID = 60 AND Entry = 36547; -- Bilebane the Rager (1 row(s), 5 obs)
UPDATE instance_creature_spawns SET Level = 26 WHERE ZoneID = 60 AND Entry = 38624; -- Kurga da Squig-Maker (1 row(s), 6 obs)
UPDATE instance_creature_spawns SET Level = 26 WHERE ZoneID = 60 AND Entry = 123554; -- Pestilent Crawler (2 row(s), 12 obs)
UPDATE instance_creature_spawns SET Level = 26 WHERE ZoneID = 60 AND Entry = 38626; -- Redeye Pen Wrangla (3 row(s), 53 obs)
UPDATE instance_creature_spawns SET Level = 26 WHERE ZoneID = 60 AND Entry = 38627; -- Redeye Squig Masta (13 row(s), 144 obs)
UPDATE instance_creature_spawns SET Level = 26 WHERE ZoneID = 60 AND Entry = 38236; -- Rotgorged Maggot (3 row(s), 19 obs)
UPDATE instance_creature_spawns SET Level = 27 WHERE ZoneID = 60 AND Entry = 38718; -- Didbin Darkhood (1 row(s), 19 obs)
UPDATE instance_creature_spawns SET Level = 27 WHERE ZoneID = 60 AND Entry = 38829; -- Glomp da Squig Masta^GunbadBoss (1 row(s), 2 obs)
UPDATE instance_creature_spawns SET Level = 27 WHERE ZoneID = 60 AND Entry = 38914; -- Morgit da Waaagher (1 row(s), 17 obs)
UPDATE instance_creature_spawns SET Level = 27 WHERE ZoneID = 60 AND Entry = 15101; -- Redeye Deathflinga (5 row(s), 79 obs)
UPDATE instance_creature_spawns SET Level = 27 WHERE ZoneID = 60 AND Entry = 35879; -- Redeye Night Goblin (13 row(s), 14 obs)
UPDATE instance_creature_spawns SET Level = 27 WHERE ZoneID = 60 AND Entry = 15928; -- Redeye Tunnel Runna (8 row(s), 76 obs)
UPDATE instance_creature_spawns SET Level = 28 WHERE ZoneID = 60 AND Entry = 38721; -- Blackfang Venom (1 row(s), 4 obs)
UPDATE instance_creature_spawns SET Level = 28 WHERE ZoneID = 60 AND Entry = 38719; -- Blackfang Webspinner (2 row(s), 7 obs)
UPDATE instance_creature_spawns SET Level = 28 WHERE ZoneID = 60 AND Entry = 15107; -- Redeye Spida Breaka (3 row(s), 7 obs)
UPDATE instance_creature_spawns SET Level = 29 WHERE ZoneID = 60 AND Entry = 38906; -- Deathshadow Dreadeye (3 row(s), 2 obs)
UPDATE instance_creature_spawns SET Level = 29 WHERE ZoneID = 60 AND Entry = 15626; -- Deathshadow Nightmare (4 row(s), 27 obs)
UPDATE instance_creature_spawns SET Level = 29 WHERE ZoneID = 60 AND Entry = 41619; -- Deceived Soul (2 row(s), 54 obs)
UPDATE instance_creature_spawns SET Level = 29 WHERE ZoneID = 60 AND Entry = 15632; -- Redeye Deathblasta (13 row(s), 127 obs)
UPDATE instance_creature_spawns SET Level = 29 WHERE ZoneID = 60 AND Entry = 38910; -- Redeye Deathdeala (15 row(s), 138 obs)
UPDATE instance_creature_spawns SET Level = 29 WHERE ZoneID = 60 AND Entry = 38912; -- Redeye Mushroom Addict (4 row(s), 48 obs)
UPDATE instance_creature_spawns SET Level = 30 WHERE ZoneID = 60 AND Entry = 42207; -- Wight Lord Solithex (1 row(s), 1 obs)
UPDATE instance_creature_spawns SET Level = 33 WHERE ZoneID = 60 AND Entry = 15102; -- 'Ard ta Feed^GunbadBoss (1 row(s), 13 obs)

--    Sighted outside the main Gunbad coordinate window used for the list above, on their
--    own boss maps: Gritzle the Wotsit at client 43211,131257 and Masta Mixa in the Lab at
--    client 35339,183683. Stated separately rather than folded into the same filter.
UPDATE instance_creature_spawns SET Level = 29 WHERE ZoneID = 60 AND Entry = 38913; -- Gritzle the Wotsit (1 row, 3 obs)
UPDATE instance_creature_spawns SET Level = 25 WHERE ZoneID = 60 AND Entry = 37967; -- Masta Mixa (1 row, 2 obs)

-- 2. Wing boss levels (instance_boss_spawns), which were 41, 42, 44 and 43.

UPDATE instance_boss_spawns SET Level = 27 WHERE Entry = 38829 AND InstanceID = 60; -- Glomp da Squig Masta (2 obs)
UPDATE instance_boss_spawns SET Level = 25 WHERE Entry = 37967 AND InstanceID = 60; -- Masta Mixa (2 obs)
UPDATE instance_boss_spawns SET Level = 33 WHERE Entry = 15102 AND InstanceID = 60; -- Ard ta Feed (13 obs)
UPDATE instance_boss_spawns SET Level = 30 WHERE Entry = 42207 AND InstanceID = 60; -- Wight Lord Solithex (1 obs)

-- 3. Public-quest spawn levels (pquest_spawns).
--
--    These are the creatures the nine Gunbad public quests actually spawn: PQuestObjective
--    builds one PQuestCreature per row and reads its level from this column, not from the
--    prototype. All 723 Gunbad rows were level 42.

UPDATE pquest_spawns SET Level = 23 WHERE ZoneId = 60 AND Entry = 36601; -- Blightbreath War Troll (26 row(s), 96 obs)
UPDATE pquest_spawns SET Level = 23 WHERE ZoneId = 60 AND Entry = 36602; -- Dreadmane Howla (10 row(s), 96 obs)
UPDATE pquest_spawns SET Level = 23 WHERE ZoneId = 60 AND Entry = 36616; -- Redeye Wrangla (20 row(s), 68 obs)
UPDATE pquest_spawns SET Level = 23 WHERE ZoneId = 60 AND Entry = 36620; -- Stonemaw War Troll (18 row(s), 84 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36605; -- Deathshadow Arbalest (6 row(s), 21 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36617; -- Deathshadow Bone Giant (4 row(s), 21 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36600; -- Deathshadow Haruspex (6 row(s), 21 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36596; -- Deathshadow Warrior (10 row(s), 36 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36615; -- Masta Wrangla Glix (1 row(s), 1 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36622; -- Redeye Alchemist (19 row(s), 50 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36613; -- Redeye Fanatic (22 row(s), 104 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36609; -- Redeye Howlagit (22 row(s), 51 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36603; -- Redeye Mixa (5 row(s), 48 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36606; -- Redeye Moonburna (4 row(s), 23 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 36611; -- Redeye Snotling (34 row(s), 99 obs)
UPDATE pquest_spawns SET Level = 24 WHERE ZoneId = 60 AND Entry = 37966; -- Shaman Verboom (1 row(s), 8 obs)
UPDATE pquest_spawns SET Level = 25 WHERE ZoneId = 60 AND Entry = 36598; -- Chipfang da Lit'l (1 row(s), 4 obs)
UPDATE pquest_spawns SET Level = 25 WHERE ZoneId = 60 AND Entry = 36612; -- Elder Kizzig da Waaagha (1 row(s), 1 obs)
UPDATE pquest_spawns SET Level = 25 WHERE ZoneId = 60 AND Entry = 37964; -- Herald of Solithex (1 row(s), 1 obs)
UPDATE pquest_spawns SET Level = 25 WHERE ZoneId = 60 AND Entry = 36556; -- Skewerin' Squig (30 row(s), 190 obs)
UPDATE pquest_spawns SET Level = 25 WHERE ZoneId = 60 AND Entry = 38631; -- Spikestabba Squig (7 row(s), 38 obs)
UPDATE pquest_spawns SET Level = 25 WHERE ZoneId = 60 AND Entry = 38235; -- Squigling (1 row(s), 84 obs)
UPDATE pquest_spawns SET Level = 25 WHERE ZoneId = 60 AND Entry = 36555; -- Stinkspewin' Squig (12 row(s), 34 obs)
UPDATE pquest_spawns SET Level = 26 WHERE ZoneId = 60 AND Entry = 38630; -- Deathspewin' Squig (14 row(s), 107 obs)
UPDATE pquest_spawns SET Level = 26 WHERE ZoneId = 60 AND Entry = 36549; -- Griblik da Stinka (1 row(s), 3 obs)
UPDATE pquest_spawns SET Level = 26 WHERE ZoneId = 60 AND Entry = 36554; -- Oozespawn Nurgling (68 row(s), 357 obs)
UPDATE pquest_spawns SET Level = 26 WHERE ZoneId = 60 AND Entry = 36545; -- Oozespawn Plaguebearer (22 row(s), 60 obs)
UPDATE pquest_spawns SET Level = 26 WHERE ZoneId = 60 AND Entry = 38628; -- Swarmin' Lit'l Squig (71 row(s), 750 obs)
UPDATE pquest_spawns SET Level = 26 WHERE ZoneId = 60 AND Entry = 38629; -- Warchargin' Squig (9 row(s), 78 obs)
UPDATE pquest_spawns SET Level = 27 WHERE ZoneId = 60 AND Entry = 38623; -- Foul Mouf da 'ungry (1 row(s), 5 obs)
UPDATE pquest_spawns SET Level = 27 WHERE ZoneId = 60 AND Entry = 38234; -- Garrolath the Poxbearer (1 row(s), 9 obs)
UPDATE pquest_spawns SET Level = 27 WHERE ZoneId = 60 AND Entry = 123553; -- Redeye Clubba (5 row(s), 51 obs)
UPDATE pquest_spawns SET Level = 27 WHERE ZoneId = 60 AND Entry = 15101; -- Redeye Deathflinga (3 row(s), 86 obs)
UPDATE pquest_spawns SET Level = 27 WHERE ZoneId = 60 AND Entry = 36887; -- Redeye Instigata (12 row(s), 125 obs)
UPDATE pquest_spawns SET Level = 27 WHERE ZoneId = 60 AND Entry = 36890; -- Redeye Mindmelta (10 row(s), 111 obs)
UPDATE pquest_spawns SET Level = 27 WHERE ZoneId = 60 AND Entry = 15628; -- Redeye Oaf Herda (12 row(s), 134 obs)
UPDATE pquest_spawns SET Level = 27 WHERE ZoneId = 60 AND Entry = 15920; -- Redeye Stompin' Giant (10 row(s), 110 obs)
UPDATE pquest_spawns SET Level = 28 WHERE ZoneId = 60 AND Entry = 38721; -- Blackfang Venom (33 row(s), 30 obs)
UPDATE pquest_spawns SET Level = 28 WHERE ZoneId = 60 AND Entry = 38719; -- Blackfang Webspinner (27 row(s), 15 obs)
UPDATE pquest_spawns SET Level = 28 WHERE ZoneId = 60 AND Entry = 15099; -- Blackfang Widow (13 row(s), 39 obs)
UPDATE pquest_spawns SET Level = 28 WHERE ZoneId = 60 AND Entry = 38904; -- Redeye Big Oaf (1 row(s), 3 obs)
UPDATE pquest_spawns SET Level = 28 WHERE ZoneId = 60 AND Entry = 15107; -- Redeye Spida Breaka (5 row(s), 23 obs)
UPDATE pquest_spawns SET Level = 29 WHERE ZoneId = 60 AND Entry = 38909; -- Blaz da Tamin' Masta (1 row(s), 1 obs)
UPDATE pquest_spawns SET Level = 29 WHERE ZoneId = 60 AND Entry = 15106; -- Deathshadow Archer (41 row(s), 424 obs)
UPDATE pquest_spawns SET Level = 29 WHERE ZoneId = 60 AND Entry = 15633; -- Deathshadow Construct (8 row(s), 114 obs)
UPDATE pquest_spawns SET Level = 29 WHERE ZoneId = 60 AND Entry = 38908; -- Deathshadow Knight (36 row(s), 381 obs)
UPDATE pquest_spawns SET Level = 29 WHERE ZoneId = 60 AND Entry = 15626; -- Deathshadow Nightmare (1 row(s), 27 obs)
UPDATE pquest_spawns SET Level = 29 WHERE ZoneId = 60 AND Entry = 15115; -- Deathshadow Specter (10 row(s), 91 obs)
UPDATE pquest_spawns SET Level = 29 WHERE ZoneId = 60 AND Entry = 41619; -- Deceived Soul (1 row(s), 54 obs)
UPDATE pquest_spawns SET Level = 29 WHERE ZoneId = 60 AND Entry = 38907; -- Velkyrrix (1 row(s), 2 obs)
UPDATE pquest_spawns SET Level = 30 WHERE ZoneId = 60 AND Entry = 41620; -- Arathremia (1 row(s), 2 obs)

-- 4. Prototype ranges for Gunbad-exclusive creatures.
--
--    Public-quest stage bosses can also be spawned by their scripts rather than from a row,
--    in which case they fall back to creature_protos.MinLevel/MaxLevel. Every prototype below
--    was checked to have no creature_spawns row and no instance_creature_spawns row outside
--    Gunbad. Stonemaw War Troll (36620) is deliberately excluded -- it has one Badlands
--    spawn, so only its Gunbad rows are levelled, above.

UPDATE creature_protos SET MinLevel = 28, MaxLevel = 28 WHERE Entry = 15099; -- Blackfang Widow
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 15101; -- Redeye Deathflinga
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 15106; -- Deathshadow Archer
UPDATE creature_protos SET MinLevel = 28, MaxLevel = 28 WHERE Entry = 15107; -- Redeye Spida Breaka
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 15115; -- Deathshadow Specter
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 15626; -- Deathshadow Nightmare
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 15628; -- Redeye Oaf Herda
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 15633; -- Deathshadow Construct
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 15920; -- Redeye Stompin' Giant
UPDATE creature_protos SET MinLevel = 26, MaxLevel = 26 WHERE Entry = 36545; -- Oozespawn Plaguebearer
UPDATE creature_protos SET MinLevel = 26, MaxLevel = 26 WHERE Entry = 36549; -- Griblik da Stinka
UPDATE creature_protos SET MinLevel = 26, MaxLevel = 26 WHERE Entry = 36554; -- Oozespawn Nurgling
UPDATE creature_protos SET MinLevel = 25, MaxLevel = 25 WHERE Entry = 36555; -- Stinkspewin' Squig
UPDATE creature_protos SET MinLevel = 25, MaxLevel = 25 WHERE Entry = 36556; -- Skewerin' Squig
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36596; -- Deathshadow Warrior
UPDATE creature_protos SET MinLevel = 25, MaxLevel = 25 WHERE Entry = 36598; -- Chipfang da Lit'l
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36600; -- Deathshadow Haruspex
UPDATE creature_protos SET MinLevel = 23, MaxLevel = 23 WHERE Entry = 36601; -- Blightbreath War Troll
UPDATE creature_protos SET MinLevel = 23, MaxLevel = 23 WHERE Entry = 36602; -- Dreadmane Howla
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36603; -- Redeye Mixa
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36605; -- Deathshadow Arbalest
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36606; -- Redeye Moonburna
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36609; -- Redeye Howlagit
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36611; -- Redeye Snotling
UPDATE creature_protos SET MinLevel = 25, MaxLevel = 25 WHERE Entry = 36612; -- Elder Kizzig da Waaagha
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36613; -- Redeye Fanatic
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36615; -- Masta Wrangla Glix
UPDATE creature_protos SET MinLevel = 23, MaxLevel = 23 WHERE Entry = 36616; -- Redeye Wrangla
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 36622; -- Redeye Alchemist
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 36887; -- Redeye Instigata
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 36890; -- Redeye Mindmelta
UPDATE creature_protos SET MinLevel = 25, MaxLevel = 25 WHERE Entry = 37964; -- Herald of Solithex
UPDATE creature_protos SET MinLevel = 24, MaxLevel = 24 WHERE Entry = 37966; -- Shaman Verboom
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 38234; -- Garrolath the Poxbearer
UPDATE creature_protos SET MinLevel = 25, MaxLevel = 25 WHERE Entry = 38235; -- Squigling
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 38623; -- Foul Mouf da 'ungry
UPDATE creature_protos SET MinLevel = 26, MaxLevel = 26 WHERE Entry = 38628; -- Swarmin' Lit'l Squig
UPDATE creature_protos SET MinLevel = 26, MaxLevel = 26 WHERE Entry = 38629; -- Warchargin' Squig
UPDATE creature_protos SET MinLevel = 26, MaxLevel = 26 WHERE Entry = 38630; -- Deathspewin' Squig
UPDATE creature_protos SET MinLevel = 25, MaxLevel = 25 WHERE Entry = 38631; -- Spikestabba Squig
UPDATE creature_protos SET MinLevel = 28, MaxLevel = 28 WHERE Entry = 38719; -- Blackfang Webspinner
UPDATE creature_protos SET MinLevel = 28, MaxLevel = 28 WHERE Entry = 38721; -- Blackfang Venom
UPDATE creature_protos SET MinLevel = 28, MaxLevel = 28 WHERE Entry = 38904; -- Redeye Big Oaf
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 38907; -- Velkyrrix
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 38908; -- Deathshadow Knight
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 38909; -- Blaz da Tamin' Masta
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 38913; -- Gritzle the Wotsit
UPDATE creature_protos SET MinLevel = 29, MaxLevel = 29 WHERE Entry = 41619; -- Deceived Soul
UPDATE creature_protos SET MinLevel = 30, MaxLevel = 30 WHERE Entry = 41620; -- Arathremia
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 123553; -- Redeye Clubba
UPDATE creature_protos SET MinLevel = 27, MaxLevel = 27 WHERE Entry = 38829; -- Glomp da Squig Masta
UPDATE creature_protos SET MinLevel = 25, MaxLevel = 25 WHERE Entry = 37967; -- Masta Mixa
UPDATE creature_protos SET MinLevel = 33, MaxLevel = 33 WHERE Entry = 15102; -- Ard ta Feed
UPDATE creature_protos SET MinLevel = 30, MaxLevel = 30 WHERE Entry = 42207; -- Wight Lord Solithex

-- 5. Public-quest spawn rows whose Objective and Entry columns are transposed.
--
--    43 Gunbad rows carry a creature entry in Objective and an objective id in Entry. Each
--    pair identifies itself: Objective 36556 is the creature Skewerin Squig and Entry 2296
--    is the objective Slay Squigs that names it, and so on. Because PQuestService keys its
--    spawn dictionary on Objective, these rows attach to no objective at all, while the
--    entry they do carry resolves to an unrelated creature -- 2296 is Cleansing Flame
--    Warrior, 2293 is Silken, 2299 is Henri Kopler, none of which belong in a greenskin
--    mine. Swapping the columns returns 35 spawns to Squig Crazy!, 4 to A Taint from Below
--    and 4 to Shadowweb Spawning Grounds, and stops the three Empire creatures spawning.
--
--    The same transposition exists outside Gunbad (49 rows in zone 1, 21 in zone 8, and
--    smaller counts in nine other zones). Those are left for a separate, separately
--    evidenced change rather than being swept up here.

UPDATE pquest_spawns SET Objective = 2296, Entry = 36556, Level = 25 WHERE ZoneId = 60 AND Objective = 36556 AND Entry = 2296; -- Skewerin Squig -> Slay Squigs (17 rows)
UPDATE pquest_spawns SET Objective = 2296, Entry = 36555, Level = 25 WHERE ZoneId = 60 AND Objective = 36555 AND Entry = 2296; -- Stinkspewin Squig -> Slay Squigs (18 rows)
UPDATE pquest_spawns SET Objective = 2299, Entry = 36545, Level = 26 WHERE ZoneId = 60 AND Objective = 36545 AND Entry = 2299; -- Oozespawn Plaguebearer -> A Taint from Below (4 rows)
UPDATE pquest_spawns SET Objective = 2293, Entry = 15099, Level = 28 WHERE ZoneId = 60 AND Objective = 15099 AND Entry = 2293; -- Blackfang Widow -> Blackfang Spiders (4 rows)

-- Not changed, and still open: 24 rows on objective 2293 (Shadowweb Spawning Grounds) name
-- creature prototype 387121, which does not exist, so they spawn nothing and log
-- 'missing creature prototype'. Matching their coordinates against the capture makes
-- Blackfang Hatchling (38720, level 28) the most frequent nearest sighting, at 8 of the 18
-- points with any sighting within 600 units -- suggestive but not conclusive, and Blackfang
-- Hatchling is not one of that objective's credited targets (15099 / 38719), so correcting
-- it would not change whether the quest can be completed. Left alone rather than guessed at;
-- the missing-prototype guard already stops it aborting the rest of the spawn set.

-- Verification.
SELECT
    (SELECT COUNT(*) FROM instance_creature_spawns WHERE ZoneID = 60 AND Level >= 40)      AS inst_rank40_plus,
    (SELECT COUNT(*) FROM instance_boss_spawns    WHERE InstanceID = 60 AND Level >= 40)   AS bosses_rank40_plus,
    (SELECT COUNT(*) FROM pquest_spawns           WHERE ZoneId = 60 AND Level >= 40)       AS pq_rank40_plus,
    (SELECT COUNT(*) FROM pquest_spawns s LEFT JOIN pquest_objectives o ON o.Guid = s.Objective
      WHERE s.ZoneId = 60 AND o.Guid IS NULL)                                              AS orphaned_pq_spawns;

-- Every kill objective of the nine Gunbad public quests, with how many of its own spawn
-- rows name a creature it actually credits.
SELECT o.Entry AS pquest, o.StageId, o.Objective, o.Count AS needed,
       (SELECT COUNT(*) FROM pquest_spawns s WHERE s.Objective = o.Guid) AS spawns,
       (SELECT COUNT(*) FROM pquest_spawns s WHERE s.Objective = o.Guid
          AND (s.Entry = o.ObjectId OR s.Entry = o.ObjectId2)) AS credited_spawns
  FROM pquest_objectives o
 WHERE o.Entry IN (181,507,508,510,511,512,513,514,515)
 ORDER BY o.Entry, o.StageId, o.Guid;

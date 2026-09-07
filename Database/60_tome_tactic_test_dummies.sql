-- 60_tome_tactic_test_dummies.sql
--
-- A test range for the tome tactic effects, in Thunder Mountain (zone 5).
--
-- Every tome tactic is conditional on the bestiary CreatureType of what you are fighting, so the
-- effects cannot be exercised without creatures of each line's type. Bots are not usable for this:
-- they are real Player characters, so TomeTacticService.GetCreatureType returns 0 and every tactic
-- is deliberately skipped (the effects are PvE-only -- the live tooltip says "closer to a Greenskin
-- mob (not a player)"). The existing target protos are no use either: only 6775 "Target Git" has a
-- real CreatureType, and the Practice Target and Invis * Target protos are all type 0.
--
-- One dummy per tactic line, plus a control with no line. The control matters: +5% damage is well
-- inside normal variance on a single hit, so the only reliable read is the same ability at the same
-- level against a typed dummy versus the control.
--
-- Faction 1 makes them hostile to both realms and aggressive. Unit.SetFaction derives
-- FactionId = faction / 8, and only FactionId 8-15 (Order) and 16-23 (Destruction) get a realm, so
-- FactionId 0 is REALMS_REALM_NEUTRAL; CombatInterface.IsEnemy is Realm inequality, so a neutral
-- creature is an enemy of everyone. faction % 2 == 1 sets Aggressive, which gives them an
-- AggressiveBrain and makes them walk into the aggro-range check being tested.
--
-- WeaponDPS 1 keeps them from being a threat while still landing hits (needed for the damage-taken
-- and defend-chance tactics). WoundsModifier 5 lets them survive sustained testing but still be
-- killable for the experience tactics.
--
-- Placement: (1421780..1425380, 923187, 11264), a line of ten 400 units apart, roughly 1,600 units
-- north of the Dragonslayer Ridge rally point. That band holds no existing spawns and the nearest
-- creature to either end is 4,330 and 2,343 units away, so they interfere with nothing. Reach them
-- with `.teleport`. Z is taken from the Dragonslayer Ridge rally point; if they float or sink,
-- adjust WorldZ rather than moving them.
--
-- Re-runnable: deletes its own protos and spawns first.

START TRANSACTION;

DELETE FROM `creature_spawns` WHERE `Entry` BETWEEN 999500 AND 999509;
DELETE FROM `creature_protos` WHERE `Entry` BETWEEN 999500 AND 999509;

INSERT INTO `creature_protos`
 (`Entry`,`Name`,`Model1`,`Model2`,`MinScale`,`MaxScale`,`MinLevel`,`MaxLevel`,`Faction`,`Ranged`,
  `Icone`,`Emote`,`Title`,`Unk`,`Unk1`,`Unk2`,`Unk3`,`Unk4`,`Unk5`,`Unk6`,`Flag`,`ScriptName`,
  `CreatureType`,`CreatureSubType`,`TokUnlock`,`LairBoss`,`VendorID`,`FigLeafData`,`Career`,
  `PowerModifier`,`WoundsModifier`,`Invulnerable`,`WeaponDPS`,`ImmuneToCC`,`Source`)
VALUES
 (999500,'Daemonic Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',10,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999501,'Beastial Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',1,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999502,'Giant Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',21,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999503,'Greenskin Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',15,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999504,'Chaos Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',11,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999505,'Mythical Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',22,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999506,'Man Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',16,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999507,'Skaven Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',18,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999508,'Undead Tactic Dummy',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',27,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0),
 (999509,'Control Dummy (no tactic line)',999,0,50,50,40,40,1,0,20,0,0,13,1,1000,0,0,54644,1,'37252677332640830','',0,0,'0',0,0,'0 0 0 76 1 10',0,1.00,5.00,0,1,1,0);

INSERT INTO `creature_spawns`
 (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`Icone`,`Emote`,`Faction`,`WaypointType`,
  `Level`,`Ward`,`Oid`,`RespawnMinutes`,`Enabled`)
VALUES
 (999500,5,1421780,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999501,5,1422180,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999502,5,1422580,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999503,5,1422980,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999504,5,1423380,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999505,5,1423780,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999506,5,1424180,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999507,5,1424580,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999508,5,1424980,923187,11264,2048,20,0,0,0,40,0,0,1,1),
 (999509,5,1425380,923187,11264,2048,20,0,0,0,40,0,0,1,1);

COMMIT;

-- Verification:
--   SELECT COUNT(*) FROM creature_protos WHERE Entry BETWEEN 999500 AND 999509;  -- 10
--   SELECT COUNT(*) FROM creature_spawns WHERE Entry BETWEEN 999500 AND 999509;  -- 10
--   -- every tactic line must be represented exactly once, plus the control:
--   SELECT t.Name, p.Entry FROM tome_tactic_lines t
--     JOIN tome_tactic_line_creature_types c ON c.AcId = t.AcId
--     JOIN creature_protos p ON p.CreatureType = c.CreatureType
--    WHERE p.Entry BETWEEN 999500 AND 999509 ORDER BY t.AcId;                     -- 9 rows

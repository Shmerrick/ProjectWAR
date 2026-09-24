-- 03_conform_ability_cooldowns_to_client.sql
--
-- Sets 1007 ability cooldowns to the client's, and gives creature and pet AI its own pacing column so
-- that a cooldown lowered to the client's does not make them cast more often.
--
-- POLICY. The 1.4.8 client is the arbiter; where it holds a value, ours conforms. data/bin/abilityexport.bin
-- holds each ability's cooldown in milliseconds and the Cooldown column is seconds (AbilityProcessor
-- multiplies by 1000), so every client value that is a whole number of seconds is taken: 813 rise or
-- appear, 194 fall or disappear. 49 client cooldowns are not whole seconds (4,500 ms and the like)
-- and cannot be held in this column; they wait for it to move to milliseconds.
--
-- AI PACING. Creature, pet and scripted-healer AI decide how often to use an ability from its cooldown
-- (AbilityMgr.cs creature abilities, Pet.cs, SimpleLVHealerBrain.cs). How often the live server's AI chose
-- to cast is not in the client at all, so the old value moves to a new server-only column, AICooldown,
-- and the AI waits for the larger of Cooldown and AICooldown -- the same split AIRange makes for range.
-- A cooldown that falls to the client's leaves creatures casting as before; one that rises slows them
-- to the client's cooldown, which the ability now has.
--
-- Both tables are written (CLAUDE.md hard rule 1). Each carries its own old value, so a row where
-- abilities and mythic_src_abilities had drifted apart still conforms in both.
--
-- Safe to re-run: every UPDATE matches only the old value, per table. Ability data is cached at
-- boot, so restart the server.

USE war_world;

SET @missing := (SELECT COUNT(*) = 0 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'mythic_src_abilities' AND COLUMN_NAME = 'AICooldown');
SET @ddl := IF(@missing, 'ALTER TABLE mythic_src_abilities ADD COLUMN AICooldown SMALLINT UNSIGNED NOT NULL DEFAULT 0 AFTER Cooldown', 'DO 0');
PREPARE ddl FROM @ddl;
EXECUTE ddl;
DEALLOCATE PREPARE ddl;

SET @missing := (SELECT COUNT(*) = 0 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'abilities' AND COLUMN_NAME = 'AICooldown');
SET @ddl := IF(@missing, 'ALTER TABLE abilities ADD COLUMN AICooldown SMALLINT UNSIGNED NOT NULL DEFAULT 0 AFTER Cooldown', 'DO 0');
PREPARE ddl FROM @ddl;
EXECUTE ddl;
DEALLOCATE PREPARE ddl;

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_03_client_cooldown;
CREATE TEMPORARY TABLE tmp_03_client_cooldown (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    OldSrc INT UNSIGNED NULL,
    OldAbl INT UNSIGNED NULL,
    ClientSeconds INT UNSIGNED NOT NULL
);

-- Entry, mythic_src_abilities.Cooldown and abilities.Cooldown where they differ from the client (NULL
-- read as 0; NULL here means that table already agrees), the client's cooldown in seconds.
INSERT INTO tmp_03_client_cooldown (Entry, OldSrc, OldAbl, ClientSeconds) VALUES
    (6, 20, 20, 0),  -- Death From Above
    (7, 13, 13, 0),  -- Spine Fling
    (8, 5, 5, 0),  -- Poisoned Spine
    (9, 8, 8, 0),  -- Gore
    (10, 20, 20, 0),  -- Head Butt
    (11, 4, 4, 0),  -- Goop Shootin'
    (12, 10, 10, 0),  -- Spore Cloud
    (18, 0, NULL, 10),  -- Git Em!
    (21, 10, 10, 0),  -- Penetrating Round
    (22, 8, 8, 0),  -- Machine Gun
    (24, 4, 4, 0),  -- Shock Grenade
    (25, 8, 8, 0),  -- High-Explosive Grenade
    (27, 5, 5, 0),  -- Flamethrower
    (28, 8, 8, 0),  -- Steam Vent
    (41, 5, 5, 0),  -- Bite
    (42, 10, 10, 0),  -- Claw Sweep
    (43, 30, 30, 0),  -- Terrifying Roar
    (44, 10, 10, 0),  -- Lion's Roar
    (45, 10, 10, 0),  -- Shred
    (46, 10, 10, 0),  -- Leg Tear
    (48, 5, 5, 0),  -- Maul
    (49, 20, 20, 0),  -- Gut Ripper
    (54, 5, 5, 0),  -- Daemonic Fire
    (55, 10, 10, 0),  -- Daemonic Consumption
    (56, 5, 5, 0),  -- Warping Energy
    (57, 8, 8, 0),  -- Coruscating Energy
    (58, 6, 6, 0),  -- Flame Of Tzeentch
    (59, 8, 8, 0),  -- Flames Of Change
    (81, 0, NULL, 4),  -- Test GTAE
    (85, 0, NULL, 10),  -- Sticky Grenade TEST For Nate
    (102, 0, NULL, 1),  -- Black Orc Console Test 3
    (105, 0, NULL, 1),  -- Choppa Console Test 3
    (108, 0, NULL, 1),  -- Shaman Console Test 3
    (111, 0, NULL, 1),  -- Hammerer Console Test 3
    (114, 0, NULL, 1),  -- Engineer Console Test 3
    (117, 0, NULL, 1),  -- Bright Wizard Console Test 3
    (120, 0, NULL, 1),  -- Magus Console Test 3
    (122, 0, NULL, 10),  -- Grasp of the Dead
    (123, 0, NULL, 10),  -- Gut Ripper
    (124, 0, NULL, 10),  -- Ravens Bite
    (125, 0, NULL, 10),  -- Storm of Ravens
    (136, 0, NULL, 10),  -- Black Fire Breath
    (137, 0, NULL, 5),  -- Black Fireball (Small)
    (138, 0, NULL, 10),  -- Black Fireball (AOE)
    (139, 0, NULL, 10),  -- Sun Fire Breath
    (140, 0, NULL, 5),  -- Sun Fireball (Small)
    (141, 0, NULL, 10),  -- Sun Fireball (AOE)
    (142, 0, NULL, 5),  -- Bite Single Target
    (143, 0, NULL, 10),  -- Multi Target Claw Swipe
    (144, 0, NULL, 60),  -- Stomp
    (156, 0, 0, 3),  -- Effect Test
    (157, 0, NULL, 3),  -- Melee Channel
    (158, 0, NULL, 3),  -- Test T1R1 Attack
    (159, 0, NULL, 3),  -- Test T1R2 Attack
    (160, 0, NULL, 3),  -- Test T1R3 Attack
    (161, 0, NULL, 3),  -- Test T1R4 Attack
    (162, 0, NULL, 3),  -- Test T1R5 Attack
    (163, 0, NULL, 3),  -- Test T1R6 Attack
    (164, 0, NULL, 3),  -- Test T1R7 Attack
    (165, 0, NULL, 3),  -- Test T1R8 Attack
    (166, 0, NULL, 3),  -- Test T1R9 Attack
    (167, 0, NULL, 3),  -- Test T1R10 Attack
    (168, 0, NULL, 3),  -- Test T2R1 Attack
    (169, 0, NULL, 3),  -- Test T2R2 Attack
    (170, 0, NULL, 3),  -- Test T2R3 Attack
    (171, 0, NULL, 3),  -- Test T2R4 Attack
    (172, 0, NULL, 3),  -- Test T2R5 Attack
    (173, 0, NULL, 3),  -- Test T2R6 Attack
    (174, 0, NULL, 3),  -- Test T2R7 Attack
    (175, 0, NULL, 3),  -- Test T2R8 Attack
    (176, 0, NULL, 3),  -- Test T2R9 Attack
    (177, 0, NULL, 3),  -- Test T2R10 Attack
    (178, 0, NULL, 3),  -- Test T3R1 Attack
    (179, 0, NULL, 3),  -- Test T3R2 Attack
    (180, 0, NULL, 3),  -- Test T3R3 Attack
    (181, 0, NULL, 3),  -- Test T3R4 Attack
    (182, 0, NULL, 3),  -- Test T3R5 Attack
    (183, 0, NULL, 3),  -- Test T3R6 Attack
    (184, 0, NULL, 3),  -- Test T3R7 Attack
    (185, 0, NULL, 3),  -- Test T3R8 Attack
    (186, 0, NULL, 3),  -- Test T3R9 Attack
    (187, 0, NULL, 3),  -- Test T3R10 Attack
    (189, 0, NULL, 3),  -- Test T2R5 Ranged Attack
    (190, 0, NULL, 3),  -- Test T2R5 Magic Attack
    (204, 0, NULL, 3),  -- Claw Swipe
    (225, 0, NULL, 10),  -- Prayer Of more Aura like Devotion
    (232, 0, NULL, 15),  -- New Taunt
    (233, 0, NULL, 15),  -- New AE Taunt
    (236, 0, NULL, 5),  -- Stunner Mob
    (237, 0, NULL, 5),  -- Knockdown Mob
    (238, 0, NULL, 6),  -- Knockback Mob
    (241, 0, NULL, 60),  -- Hold The Line!
    (242, 0, NULL, 60),  -- Juggernaut
    (379, 0, NULL, 10),  -- Detonation Test
    (385, 0, NULL, 10),  -- Squig Squeal
    (386, 0, NULL, 20),  -- Death From Above
    (387, 0, NULL, 8),  -- Gore
    (388, 0, NULL, 20),  -- Head Butt
    (389, 0, NULL, 4),  -- Goop Shootin'
    (390, 0, NULL, 10),  -- Spore Cloud
    (391, 0, NULL, 5),  -- Poisoned Spine
    (392, 0, NULL, 13),  -- Spine Fling
    (410, 0, NULL, 15),  -- Unearthly Shriek
    (609, 0, NULL, 60),  -- Revenge
    (612, 0, NULL, 60),  -- On Your Feet
    (632, 0, NULL, 60),  -- Concussion
    (634, 0, NULL, 60),  -- Flashing Blades
    (654, 0, NULL, 60),  -- Get Down!
    (655, 0, NULL, 60),  -- Blinding Speed
    (675, 0, NULL, 60),  -- Shimmering Cloak
    (676, 0, NULL, 60),  -- Warp Mind
    (691, 0, NULL, 60),  -- Doom And Darkness
    (693, 0, NULL, 60),  -- Wind Of Death
    (860, 0, NULL, 5),  -- Bite
    (861, 0, NULL, 10),  -- Claw Sweep
    (862, 0, NULL, 30),  -- Terrifying Roar
    (863, 0, NULL, 10),  -- Lion's Roar
    (864, 0, NULL, 10),  -- Shred
    (865, 0, NULL, 10),  -- Leg Tear
    (866, 0, NULL, 8),  -- Fang And Claw
    (867, 0, NULL, 20),  -- Maul
    (868, 0, NULL, 5),  -- Gut Ripper
    (1013, 0, NULL, 13),  -- VFX / Tag Shout TEST
    (1014, 0, NULL, 13),  -- VFX / Tag Null TEST
    (1020, 0, NULL, 300),  -- Portable Mailbox
    (1529, 10, 10, 0),  -- Burn Salve
    (1617, 0, 0, 60),  -- Rune of Battle
    (1841, 0, 0, 30),  -- Summon Squig
    (1842, 0, 0, 30),  -- Horned Squig
    (1843, 0, 0, 30),  -- Gas Squig
    (1844, 0, 0, 30),  -- Spiked Squig
    (1865, 0, NULL, 30),  -- Drop That!!
    (3149, 0, NULL, 10),  -- Detonation
    (3171, 0, 0, 5),  -- WAAAAAAAGH!
    (3177, 0, NULL, 10),  -- Rune-Etched Axe
    (3498, 0, NULL, 20),  -- Sub Ability
    (3500, 0, 0, 20),  -- Immolating Grasp
    (3637, 0, NULL, 120),  -- Oath Rune Of Spellbinding
    (3670, 0, NULL, 120),  -- Oath Rune of Power
    (3671, 0, NULL, 120),  -- Oath Rune of Sanctuary
    (3672, 0, NULL, 120),  -- Oath Rune Of Healing
    (3673, 0, NULL, 120),  -- Oath Rune of Fortune
    (3746, 0, NULL, 60),  -- Mark of Daemonic Fury
    (3747, 0, NULL, 60),  -- Mark of the Spell Destroyer
    (3748, 0, NULL, 60),  -- Mark of the Vortex
    (3749, 0, NULL, 60),  -- Mark of Tzeentch's Will
    (3859, 0, NULL, 60),  -- Vortex
    (3865, 0, NULL, 60),  -- Protection of Ancients Heal Blocker
    (3868, 0, NULL, 60),  -- Rune of Warding
    (3869, 0, NULL, 60),  -- Rune of Power
    (3870, 0, NULL, 60),  -- Rune of Iron
    (3947, 0, 0, 15),  -- Unearthly Shriek
    (3952, 0, NULL, 10),  -- Coordinated Strike
    (3981, 0, 0, 20),  -- Sundered Motion
    (3993, 0, 0, 10),  -- Cleansing Vitality
    (3997, 0, NULL, 20),  -- Dissolving Mist
    (3998, 0, 0, 10),  -- Leaping Alteration
    (3999, 0, NULL, 10),  -- Tzeentch's Grip
    (4020, 0, NULL, 12),  -- Ghostly Howl
    (4021, 0, NULL, 12),  -- Fire Breath
    (4022, 0, NULL, 15),  -- Buffet
    (4023, 0, NULL, 20),  -- Entangle
    (4024, 0, NULL, 10),  -- Vicious Charge
    (4025, 0, NULL, 6),  -- Impale
    (4026, 0, NULL, 6),  -- Eviscerate
    (4027, 0, NULL, 6),  -- Gore
    (4028, 0, NULL, 6),  -- Maul
    (4029, 0, NULL, 6),  -- Rend
    (4030, 0, NULL, 10),  -- Swipe
    (4031, 0, NULL, 8),  -- Taunting Strike
    (4032, 0, NULL, 20),  -- Engage
    (4033, 0, NULL, 9),  -- Crack Shot
    (4034, 0, NULL, 6),  -- Snipe
    (4035, 0, NULL, 12),  -- Volley
    (4036, 0, NULL, 12),  -- Blunderbus Blast
    (4037, 0, NULL, 2),  -- Fireball
    (4038, 0, NULL, 15),  -- Flaming Sword of Rhuin
    (4039, 0, NULL, 10),  -- The Burning Head
    (4040, 0, NULL, 8),  -- Fiery Blast
    (4041, 0, NULL, 15),  -- Conflagration of Doom
    (4042, 0, NULL, 2),  -- Rule of Burning Iron
    (4043, 0, NULL, 10),  -- Commandment of Brass
    (4044, 0, NULL, 10),  -- Transmutation of Lead
    (4045, 0, NULL, 8),  -- Distillation of Molten Silver
    (4046, 0, NULL, 10),  -- The Spirit of the Forge
    (4047, 0, NULL, 5),  -- Creeping Death
    (4048, 0, NULL, 5),  -- Crown of Taidron
    (4049, 0, NULL, 15),  -- Shades of Death
    (4050, 0, NULL, 10),  -- Pit of Shades
    (4051, 0, NULL, 15),  -- The Bear's Anger
    (4052, 0, NULL, 15),  -- The Oxen Stands
    (4053, 0, NULL, 2),  -- The Crow's Feast
    (4054, 0, NULL, 10),  -- The Beast Cowers
    (4055, 0, NULL, 8),  -- The Hunter's Spear
    (4056, 0, NULL, 15),  -- The Wolf Hunts
    (4057, 0, NULL, 15),  -- Portent of Far
    (4058, 0, NULL, 15),  -- Second Sign of Amul
    (4059, 0, NULL, 15),  -- Celestial Shield
    (4060, 0, NULL, 2),  -- Forked Lightning
    (4061, 0, NULL, 8),  -- Uranon's Thunderbolt
    (4062, 0, NULL, 60),  -- The Comet of Casandora
    (4063, 0, NULL, 2),  -- Burning Gaze
    (4064, 0, NULL, 15),  -- Pha's Illumination
    (4065, 0, NULL, 8),  -- Healing Energy
    (4066, 0, NULL, 10),  -- Dazzling Brightness
    (4067, 0, NULL, 8),  -- Cleansing Flare
    (4068, 0, NULL, 15),  -- Mistress of the Marsh
    (4069, 0, NULL, 2),  -- Master of the Wood
    (4070, 0, NULL, 180),  -- Gift of Life
    (4071, 0, NULL, 15),  -- The Howler Wind
    (4072, 0, NULL, 15),  -- The Rain Lord
    (4073, 0, NULL, 30),  -- Master of Stone
    (4074, 0, NULL, 2),  -- Dark Hand of Death
    (4075, 0, NULL, 8),  -- Steal Soul
    (4076, 0, NULL, 8),  -- Wind of Death
    (4077, 0, NULL, 15),  -- Doom and Darkness
    (4078, 0, NULL, 15),  -- Drain Life
    (4079, 0, NULL, 5),  -- Magnificent Buboes
    (4080, 0, NULL, 15),  -- Favoured Poxes
    (4081, 0, NULL, 8),  -- Effulgent Boils
    (4082, 0, NULL, 10),  -- Glistening Scabs
    (4083, 0, NULL, 8),  -- Glorious Afflictions
    (4084, 0, NULL, 10),  -- Sumptuous Pestilence
    (4085, 0, NULL, 8),  -- Red Fire of Alteration
    (4086, 0, NULL, 15),  -- Orange Fire of Transition
    (4087, 0, NULL, 15),  -- Yellow Fire of Transformation
    (4088, 0, NULL, 12),  -- Blue Fire of Metamorphosis
    (4089, 0, NULL, 2),  -- Indigo Fire of Change
    (4090, 0, NULL, 12),  -- Violet Fire of Tzeentch
    (4091, 0, NULL, 3),  -- Blissful Throes
    (4092, 0, NULL, 15),  -- Luxuroius Torment
    (4093, 0, NULL, 30),  -- Titillating Delusions
    (4094, 0, NULL, 15),  -- Enrapturing Spasms
    (4095, 0, NULL, 30),  -- Invocation of Nehek
    (4096, 0, NULL, 15),  -- Hand of Dust
    (4097, 0, NULL, 15),  -- Hellish Vigour
    (4098, 0, NULL, 2),  -- Gaze of Nagash
    (4099, 0, NULL, 15),  -- Vanhel's Danse Macabre
    (4100, 0, NULL, 12),  -- Curse of Years
    (4101, 0, NULL, 2),  -- Gaze of Gork
    (4102, 0, NULL, 5),  -- Brain Bursta
    (4103, 0, NULL, 10),  -- Gork'll Fix It
    (4104, 0, NULL, 15),  -- Fists of Gork
    (4105, 0, NULL, 15),  -- Waaagh
    (4106, 0, NULL, 10),  -- Mork Wants Ya
    (4107, 0, NULL, 15),  -- Bash 'Em Ladz
    (4108, 0, NULL, 10),  -- Bloodgruel
    (4109, 0, NULL, 10),  -- Braingobbler
    (4110, 0, NULL, 15),  -- Bullgorger
    (4111, 0, NULL, 2),  -- Bonecruncher
    (4112, 0, NULL, 15),  -- Toothcracker
    (4113, 0, NULL, 10),  -- Trollguts
    (4114, 0, NULL, 2),  -- Warp Lightning
    (4115, 0, NULL, 8),  -- Pestelent Breath
    (4116, 0, NULL, 8),  -- Vermintide
    (4117, 0, NULL, 15),  -- Pestilence
    (4118, 0, NULL, 15),  -- Death Frenzy
    (4119, 0, NULL, 10),  -- Mark of Fury
    (4120, 0, NULL, 10),  -- Mark of Tzeentch
    (4121, 0, NULL, 10),  -- Hunter's Mark
    (4122, 0, NULL, 10),  -- Mark of Khorne
    (4123, 0, NULL, 15),  -- Headbutt
    (4124, 0, NULL, 8),  -- Flurry
    (4125, 0, NULL, 15),  -- Syphon Life
    (4126, 0, NULL, 15),  -- Stomp
    (4127, 0, NULL, 15),  -- Whirlwind
    (4128, 0, NULL, 10),  -- Silence
    (4129, 0, NULL, 10),  -- Disarm
    (4130, 0, NULL, 10),  -- Cripple
    (4131, 0, NULL, 10),  -- Hush
    (4132, 0, NULL, 10),  -- Rust
    (4133, 0, NULL, 10),  -- Crippling Blast
    (4135, 0, NULL, 9),  -- Crack Shot
    (4136, 0, NULL, 6),  -- Snipe
    (4137, 0, NULL, 9),  -- Throw Axe
    (4138, 0, NULL, 9),  -- Throw Rock
    (4144, 0, NULL, 10),  -- Finishing Blow
    (4145, 0, NULL, 5),  -- Poison Wind Globe
    (4148, 0, NULL, 15),  -- Seething Plague
    (4149, 0, NULL, 10),  -- Aqshi Unbound
    (4150, 0, NULL, 15),  -- Resist Fear and Terror
    (4151, 0, NULL, 15),  -- Penetrate Magic Resistance
    (4152, 0, NULL, 15),  -- Penetrate Natural Armor
    (4153, 0, NULL, 15),  -- Penetrate Wards
    (4154, 0, NULL, 15),  -- Penetrate Ethereal
    (4155, 0, NULL, 3),  -- Firing
    (4156, 0, NULL, 3),  -- Firing
    (4157, 0, NULL, 3),  -- Firing
    (4158, 0, NULL, 3),  -- Firing
    (4159, 0, NULL, 3),  -- Firing
    (4180, 1800, 1800, 0),  -- Activating...
    (4181, 1800, 1800, 0),  -- Activating...
    (4184, 3600, 3600, 0),  -- Activating...
    (4195, 0, NULL, 45),  -- Rock Skin
    (4196, 0, NULL, 25),  -- Electric Skin
    (4197, 0, NULL, 25),  -- Electric Skin
    (4200, 0, NULL, 10),  -- Lileath's Tear
    (4206, 0, NULL, 20),  -- Bolt Thrower
    (4207, 0, NULL, 20),  -- Hydra Breath
    (4210, 0, NULL, 10),  -- Regen
    (4213, 0, NULL, 10),  -- Snare
    (4214, 0, NULL, 10),  -- Enraged
    (4215, 0, NULL, 9),  -- Average Raid Boss DMG Ability
    (4216, 0, NULL, 20),  -- HIgh DMG Raid Boss Ability
    (4217, 0, NULL, 20),  -- Entangling Webs
    (4218, 0, NULL, 10),  -- Hydra Bite
    (4219, 0, NULL, 10),  -- Glamour
    (4220, 0, NULL, 15),  -- Bloodsoil Poison
    (4223, 0, NULL, 10),  -- Power Channel Summon
    (4224, 0, NULL, 4),  -- Clynch Test Ability
    (4225, 0, NULL, 15),  -- Valaan's Sack
    (4227, 0, NULL, 10),  -- Fire Shot
    (4228, 0, NULL, 18),  -- Raid Boss Test Ability 1 (kick)
    (4229, 0, NULL, 13),  -- Raid Boss Test Ability 2 (AE Whirl)
    (4230, 0, NULL, 25),  -- Raid Boss Test Ability 3 (High Damage with Prep)
    (4231, 0, NULL, 20),  -- Ability for AZ
    (4235, 0, NULL, 10),  -- Chimeral Talisman
    (4236, 0, NULL, 9),  -- CHRIS - Long Duration Stun
    (4237, 0, NULL, 20),  -- Effulgent Boils
    (4239, 0, NULL, 20),  -- VOG
    (4240, 0, NULL, 9),  -- Beast Melee 1
    (4241, 0, NULL, 9),  -- Beast Melee 2
    (4242, 0, NULL, 9),  -- Beast Melee Cleave
    (4243, 0, NULL, 20),  -- Beast Melee Whirlwind
    (4244, 0, NULL, 9),  -- Beast Long Prep Melee 2
    (4245, 0, NULL, 9),  -- Beast Long Prep Melee 2
    (4246, 0, NULL, 9),  -- Beast Long Prep Stomp
    (4247, 0, NULL, 20),  -- Beast Roar
    (4248, 0, NULL, 9),  -- Beast Cast
    (4249, 0, NULL, 25),  -- Beast Cast PBAE
    (4250, 0, NULL, 9),  -- Beast Long Prep Cast
    (4251, 0, NULL, 25),  -- Beast Long Prep Cast PBAE
    (4253, 0, NULL, 9),  -- Humanoid Melee 1
    (4254, 0, NULL, 9),  -- Humanoid Melee 2
    (4255, 0, NULL, 9),  -- Humanoid Melee Cleave
    (4256, 0, NULL, 20),  -- Humanoid Melee Whirl
    (4257, 0, NULL, 9),  -- Humanoid Cast
    (4258, 0, NULL, 25),  -- Humanoid Cast PBAE
    (4259, 0, NULL, 9),  -- Humanoid Long Prep Cast
    (4260, 0, NULL, 25),  -- Humanoid Long Prep Cast PBAE
    (4261, 0, NULL, 9),  -- Humanoid Instant Cast
    (4262, 0, NULL, 25),  -- Humanoid Instant Cast PBAE
    (4264, 0, NULL, 10),  -- Chimeral Talisman
    (4271, 0, NULL, 5),  -- Arrow Storm
    (4273, 0, NULL, 5),  -- Creeping Death
    (4274, 0, NULL, 4),  -- Cleave
    (4275, 0, NULL, 5),  -- Cleave
    (4276, 0, NULL, 6),  -- Killing Blow
    (4277, 0, NULL, 20),  -- Sword Gorge
    (4278, 0, NULL, 5),  -- Shield Slam
    (4279, 0, NULL, 5),  -- Shield Wall
    (4280, 0, NULL, 5),  -- Volkmar - Jade Griffon
    (4281, 0, NULL, 4),  -- Volkmar - Staff of Command
    (4282, 0, NULL, 5),  -- Volkmar - Theogonist's Smite
    (4290, 0, NULL, 9),  -- ability
    (4291, 0, NULL, 20),  -- ability
    (4292, 0, NULL, 9),  -- ability
    (4300, 0, NULL, 9),  -- Assault of Khaine
    (4301, 0, NULL, 20),  -- Rival's Ruin
    (4302, 0, NULL, 9),  -- Assault of Khaine
    (4303, 0, NULL, 20),  -- Rival's Ruin
    (4304, 0, NULL, 9),  -- Severing Slaughter
    (4305, 0, NULL, 20),  -- Descending Doom
    (4306, 0, NULL, 9),  -- Severing Slaughter
    (4307, 0, NULL, 20),  -- Descending Doom
    (4308, 0, NULL, 9),  -- Severing Slaughter
    (4309, 0, NULL, 20),  -- Descending Doom
    (4310, 0, NULL, 9),  -- Impaling Ruin
    (4311, 0, NULL, 20),  -- Manticore Sting
    (4312, 0, NULL, 9),  -- Eviscerating Blow
    (4313, 0, NULL, 20),  -- Hydra Strike
    (4314, 0, NULL, 9),  -- Vaul's Hammer
    (4315, 0, NULL, 20),  -- Phoenix Wing
    (4316, 0, NULL, 9),  -- Vaul's Hammer
    (4317, 0, NULL, 20),  -- Phoenix Wing
    (4318, 0, NULL, 9),  -- Balanced Stroke
    (4319, 0, NULL, 20),  -- Asuryan's Mercy
    (4320, 0, NULL, 9),  -- Balanced Stroke
    (4321, 0, NULL, 20),  -- Asuryan's Mercy
    (4322, 0, NULL, 9),  -- Balanced Stroke
    (4323, 0, NULL, 20),  -- Asuryan's Mercy
    (4324, 0, NULL, 9),  -- Balanced Stroke
    (4325, 0, NULL, 20),  -- Asuryan's Mercy
    (4330, 0, NULL, 5),  -- Thunderous Blow
    (4331, 0, NULL, 25),  -- Ghal Maraz's Maelstrom
    (4333, 0, NULL, 30),  -- Holy Wrath
    (4334, 0, NULL, 15),  -- Divine Wrath
    (4335, 0, NULL, 12),  -- Cracking Blow
    (4336, 0, NULL, 12),  -- Deafening Screech
    (4337, 0, NULL, 12),  -- Deafening Screech
    (4338, 0, 0, 18),  -- Wing Buffet
    (4339, 0, NULL, 4),  -- Death's Render
    (4340, 0, NULL, 5),  -- Furious Attack
    (4342, 0, NULL, 4),  -- Death's Render
    (4343, 0, NULL, 18),  -- Wing Buffet
    (4350, 0, NULL, 25),  -- ability
    (4351, 0, NULL, 60),  -- ability
    (4355, 0, NULL, 5),  -- ability
    (4356, 0, NULL, 4),  -- ability
    (4357, 0, NULL, 9),  -- ability
    (4360, 0, NULL, 4),  -- ability
    (4361, 0, NULL, 10),  -- ability
    (4362, 0, NULL, 10),  -- Sprite Fire
    (4365, 0, NULL, 5),  -- Elemental Strike
    (4366, 0, NULL, 5),  -- Fire Breath
    (4367, 0, NULL, 10),  -- Exploding Flames
    (4369, 0, NULL, 7),  -- Heavy Strike
    (4370, 0, NULL, 25),  -- Heal
    (4371, 0, NULL, 10),  -- Mine Explosion
    (4373, 0, NULL, 10),  -- Kromil - Frontal Shot
    (4380, 3, 3, 0),  -- Torch of Lileath
    (4381, 0, NULL, 5),  -- Whitefire Fang
    (4382, 0, NULL, 20),  -- Pleasurevenom
    (4383, 0, 0, 20),  -- Painvenom
    (4384, 0, NULL, 20),  -- Whitefire Webbing
    (4406, 0, NULL, 18),  -- Pain-Fury Whirlwind
    (4408, 0, NULL, 5),  -- Razor Claws
    (4410, 0, NULL, 15),  -- Spider Lay Egg
    (4415, 0, NULL, 10),  -- Cleave
    (4417, 0, NULL, 10),  -- Guttural Shout
    (4418, 0, NULL, 10),  -- Ungor Hate Control
    (4420, 0, NULL, 20),  -- Disruptive Blow
    (4421, 0, NULL, 20),  -- Writhing Swipe
    (4422, 0, NULL, 20),  -- Writhing Blow
    (4425, 0, NULL, 1),  -- Writhing Energy
    (4426, 0, NULL, 20),  -- Writhing Pain
    (4427, 120, 120, 0),  -- Enrage
    (4428, 0, 0, 20),  -- Writhing Pleasure
    (4434, 0, NULL, 5),  -- Savage Cut
    (4435, 15, 15, 5),  -- Murderous Swing
    (4436, 0, NULL, 5),  -- Ruthless Hit
    (4438, 0, NULL, 5),  -- Unrestrained Blow
    (4439, 0, NULL, 5),  -- Ripping Tusk
    (4440, 0, NULL, 5),  -- Tearing Claw
    (4443, 0, NULL, 20),  -- Mindfall Venom
    (4449, 0, NULL, 15),  -- Azyr's Beckoning
    (4452, 0, NULL, 20),  -- Killing Blow
    (4453, 0, NULL, 20),  -- Hoof Stomp
    (4458, 3, 3, 5),  -- Horgulul Bite
    (4459, 3, 3, 5),  -- Dralel Bite
    (4460, 0, NULL, 10),  -- Sticky Spit
    (4462, 30, 30, 15),  -- Whitefire Web Bolt
    (4555, 0, NULL, 6),  -- Fear
    (4556, 0, NULL, 6),  -- Fear
    (4600, 0, NULL, 10),  -- Heavy Strike
    (4601, 0, NULL, 10),  -- Claw Strike
    (4602, 0, NULL, 10),  -- Burst of Flame
    (4603, 0, NULL, 10),  -- Corporeal Blast
    (4604, 0, NULL, 10),  -- Spirit Blast
    (4605, 0, NULL, 10),  -- Bleed
    (4606, 0, 0, 10),  -- Bloody Claw
    (4607, 0, NULL, 10),  -- On Fire
    (4608, 0, 0, 10),  -- Life Loss
    (4609, 0, 0, 10),  -- Spirit Drain
    (4610, 0, NULL, 10),  -- Whirling Dervish
    (4611, 0, NULL, 10),  -- Combustion
    (4612, 0, NULL, 10),  -- Blast Wave
    (4613, 0, NULL, 10),  -- Ethereal Wave
    (4614, 0, NULL, 10),  -- Sharp Projectile
    (4615, 0, NULL, 10),  -- Piercing Projectile
    (4616, 0, NULL, 10),  -- Volley
    (4617, 0, NULL, 10),  -- Elemental Strike
    (4618, 0, NULL, 10),  -- Corporeal Strike
    (4619, 0, NULL, 10),  -- Ethereal Strike
    (4620, 0, NULL, 10),  -- Elemental Rain
    (4621, 0, NULL, 10),  -- Corporeal Rain
    (4622, 0, NULL, 10),  -- Ethereal Rain
    (4623, 0, NULL, 10),  -- Shard Volley
    (4624, 0, 0, 10),  -- Elemental Circle
    (4625, 0, 0, 10),  -- Circle of Life Loss
    (4626, 0, 0, 10),  -- Drain Circle
    (4627, 0, NULL, 10),  -- Elemental Line
    (4628, 0, NULL, 10),  -- Corporeal Line
    (4629, 0, NULL, 10),  -- Ethereal Line
    (4630, 0, NULL, 10),  -- Elemental Breath
    (4631, 0, NULL, 10),  -- Corporeal Breath
    (4632, 0, NULL, 10),  -- Ethereal Breath
    (4633, 0, NULL, 25),  -- Stun
    (4634, 0, NULL, 25),  -- Stun
    (4635, 0, NULL, 25),  -- Concussion Projectile
    (4636, 0, NULL, 25),  -- Elemental Stun
    (4637, 0, NULL, 25),  -- Corporeal Stun
    (4638, 0, NULL, 25),  -- Ethereal Stun
    (4639, 0, NULL, 10),  -- Snare
    (4640, 0, NULL, 10),  -- Snare
    (4641, 0, NULL, 10),  -- Sticky Projectile
    (4642, 0, NULL, 10),  -- Elemental Snare
    (4643, 0, NULL, 10),  -- Corporeal Snare
    (4644, 0, NULL, 10),  -- Ethereal Snare
    (4645, 0, NULL, 10),  -- Root
    (4646, 0, NULL, 10),  -- Root
    (4647, 0, NULL, 10),  -- Web Projectile
    (4648, 0, NULL, 10),  -- Elemental Root
    (4649, 0, NULL, 10),  -- Corporeal Root
    (4650, 0, NULL, 10),  -- Ethereal Root
    (4651, 0, NULL, 10),  -- Knockback
    (4652, 0, NULL, 10),  -- Knockback
    (4653, 0, NULL, 10),  -- Elemental Thrust
    (4654, 0, NULL, 10),  -- Corporeal Thrust
    (4655, 0, NULL, 10),  -- Ethereal Thrust
    (4656, 0, NULL, 10),  -- Elemental Push
    (4657, 0, NULL, 10),  -- Corporeal Push
    (4658, 0, NULL, 10),  -- Ethereal Push
    (4659, 0, NULL, 10),  -- Whirling Knockback
    (4660, 0, NULL, 10),  -- Knockback Wave
    (4661, 0, NULL, 10),  -- Elemental Wave Push
    (4662, 0, NULL, 10),  -- Corporeal Wave Push
    (4663, 0, NULL, 10),  -- Ethereal Wave Push
    (4664, 0, NULL, 25),  -- Disabling Swing
    (4665, 0, NULL, 25),  -- Elemental Stop
    (4666, 0, NULL, 25),  -- Corporeal Stop
    (4667, 0, NULL, 25),  -- Ethereal Stop
    (4668, 0, NULL, 25),  -- Knockdown
    (4669, 0, NULL, 25),  -- Knockdown
    (4670, 0, NULL, 25),  -- Knockdown Shot
    (4671, 0, NULL, 25),  -- Elemental Knockdown
    (4672, 0, NULL, 25),  -- Corporeal Knockdown
    (4673, 0, NULL, 25),  -- Ethereal Knockdown
    (4674, 0, NULL, 25),  -- Disabling Whirl
    (4675, 0, NULL, 25),  -- Elemental Stop Blast
    (4676, 0, NULL, 25),  -- Corporeal Stop Blast
    (4677, 0, NULL, 25),  -- Ethereal Stop Blast
    (4678, 0, NULL, 25),  -- Disabling Wave
    (4679, 0, NULL, 25),  -- Elemental Stop Wave
    (4680, 0, NULL, 25),  -- Corporeal Stop Wave
    (4681, 0, NULL, 25),  -- Ethereal Stop Wave
    (4682, 0, NULL, 25),  -- Blunt Volley
    (4683, 0, NULL, 25),  -- Elemental Mass Thrust
    (4684, 0, NULL, 25),  -- Corporeal Mass Thrust
    (4685, 0, NULL, 25),  -- Ethereal Mass Thrust
    (4686, 0, NULL, 25),  -- Whirling Knockdown
    (4687, 0, NULL, 25),  -- Silence Swing
    (4688, 0, NULL, 25),  -- Elemental Silence
    (4689, 0, NULL, 25),  -- Corporeal Silence
    (4690, 0, NULL, 25),  -- Ethereal Silence
    (4691, 0, NULL, 25),  -- Silence Wave
    (4692, 0, NULL, 25),  -- Elemental Mass Silence
    (4693, 0, NULL, 25),  -- Corporeal Mass Silence
    (4694, 0, NULL, 25),  -- Ethereal Mass Silence
    (4695, 0, NULL, 25),  -- Silence Blast
    (4696, 0, NULL, 25),  -- Elemental Silence Blast
    (4697, 0, NULL, 25),  -- Corporeal Silence Blast
    (4698, 0, NULL, 25),  -- Ethereal Silence Blast
    (4699, 0, NULL, 25),  -- Disarm
    (4700, 0, NULL, 25),  -- Elemental Disarm
    (4701, 0, NULL, 25),  -- Corporeal Disarm
    (4702, 0, NULL, 25),  -- Ethereal Disarm
    (4703, 0, NULL, 25),  -- Disarm Wave
    (4704, 0, NULL, 25),  -- Elemental Mass Disarm
    (4705, 0, NULL, 25),  -- Corporeal Mass Disarm
    (4706, 0, NULL, 25),  -- Ethereal Mass Disarm
    (4707, 0, NULL, 25),  -- Disarm Blast
    (4708, 0, NULL, 25),  -- Ethereal Disarm Blast
    (4709, 0, NULL, 25),  -- Corporeal Disarm Blast
    (4710, 0, NULL, 25),  -- Ethereal Disarm Blast
    (4711, 0, NULL, 10),  -- Heal
    (4712, 0, NULL, 10),  -- Regen
    (4713, 0, NULL, 10),  -- Healing Blast
    (4714, 0, NULL, 10),  -- Healing Circle
    (4715, 0, NULL, 10),  -- Sensitive
    (4716, 0, NULL, 10),  -- Inept
    (4717, 0, NULL, 10),  -- Shaky Resolve
    (4718, 0, NULL, 10),  -- Pushover
    (4719, 0, NULL, 60),  -- Focused Rage
    (4720, 0, NULL, 60),  -- Iron Will
    (4721, 0, NULL, 60),  -- Resistance
    (4722, 0, NULL, 10),  -- Fear
    (4723, 0, NULL, 10),  -- Scream
    (4724, 0, NULL, 10),  -- Fury
    (4725, 0, NULL, 10),  -- Thick Skin
    (4726, 0, NULL, 10),  -- Resilience
    (4731, 0, NULL, 10),  -- Liquid Inspiration
    (4733, 0, NULL, 15),  -- Throwing ...
    (4734, 0, NULL, 10),  -- ability
    (4735, 0, NULL, 10),  -- Windstep's Curse
    (4736, 0, NULL, 15),  -- Jaln's Device
    (4804, 0, NULL, 5),  -- Bone Swarm
    (4806, 0, NULL, 5),  -- Death Metal
    (4807, 0, NULL, 5),  -- Molotov Cocktail
    (4808, 0, NULL, 5),  -- The Pox
    (4811, 0, 0, 10),  -- Crippling Stomp
    (4813, 0, NULL, 10),  -- Tick Tick Boom
    (4900, 0, NULL, 10),  -- Chris
    (4901, 0, NULL, 10),  -- Chris
    (4902, 0, NULL, 10),  -- Chris
    (4903, 0, NULL, 10),  -- Chris
    (4904, 0, NULL, 10),  -- Chris
    (4905, 0, NULL, 10),  -- Chris
    (4906, 0, NULL, 5),  -- Giant Fireball
    (4910, 0, NULL, 25),  -- Exorcism
    (4911, 0, NULL, 10),  -- Stream of Corruption
    (4912, 0, NULL, 10),  -- Pestilent Globule
    (4914, 0, NULL, 3),  -- Throw Fruit
    (4915, 0, NULL, 3),  -- Throw Vegatables
    (4916, 0, NULL, 16),  -- Conflagration of Doom
    (4917, 0, NULL, 7),  -- Conflagration of Doom
    (4918, 0, NULL, 10),  -- Pestilent Globule
    (4919, 0, NULL, 10),  -- Pestilent Globule 2
    (4923, 0, NULL, 5),  -- Theogonist's Smite
    (4924, 0, NULL, 5),  -- Staff of Command
    (4925, 0, NULL, 30),  -- Rebirth
    (4926, 0, NULL, 20),  -- Inferno Wave
    (4927, 0, NULL, 5),  -- Giant Fireball
    (4962, 0, NULL, 20),  -- ability
    (4963, 0, NULL, 30),  -- Phoenix Blade toss
    (4964, 0, NULL, 20),  -- Asuryans Will
    (4965, 0, NULL, 8),  -- Rage
    (4969, 0, NULL, 30),  -- Fury
    (4970, 0, NULL, 8),  -- Throw
    (4974, 0, NULL, 10),  -- Stun
    (4978, 0, NULL, 12),  -- Kick in the Gibblies
    (4980, 0, 0, 10),  -- Black Dragon Breath
    (4981, 0, NULL, 10),  -- Black Fireball
    (4982, 0, NULL, 10),  -- Black Fireball (AoE)
    (4983, 0, NULL, 10),  -- Sun Dragon Breath
    (4984, 0, NULL, 10),  -- Sun Fireball
    (4985, 0, NULL, 10),  -- Sun Fireball (AoE)
    (4988, 0, NULL, 20),  -- Black Horror
    (4990, 0, NULL, 10),  -- Forked Lightning
    (4991, 0, NULL, 8),  -- Radiant Strike
    (4993, 0, NULL, 5),  -- Blistering Heat
    (4999, 0, NULL, 30),  -- Whirlwind
    (5002, 0, NULL, 15),  -- Hypnotic Agent
    (5003, 0, NULL, 15),  -- Glory of the Raven God
    (5004, 0, NULL, 15),  -- Phlegmatic Spore
    (5005, 0, NULL, 15),  -- Shard of the Ravenshrine
    (5007, 5, 5, 10),  -- Shart
    (5008, 5, 5, 120),  -- Squig Stomp
    (5009, 0, NULL, 10),  -- Corrosive Vomit
    (5012, 15, 15, 5),  -- Squig Heal
    (5013, 0, NULL, 5),  -- Boneskin
    (5014, 0, NULL, 10),  -- Squig Breath
    (5015, 0, NULL, 10),  -- Squig Bounce
    (5016, 0, NULL, 5),  -- Mangle
    (5017, 0, NULL, 5),  -- Eye Gouge
    (5018, 0, NULL, 5),  -- Noogie
    (5019, 0, NULL, 10),  -- Fungal Strength
    (5020, 0, NULL, 10),  -- Fungal Shield
    (5021, 0, NULL, 10),  -- Fungal Heal
    (5022, 0, NULL, 15),  -- Panic
    (5023, 0, NULL, 15),  -- Revenge
    (5025, 5, 5, 0),  -- Jar o' Pummelin'
    (5026, 0, NULL, 5),  -- Flask of Grabity Fings
    (5033, 0, NULL, 15),  -- Throw Torch
    (5040, 0, NULL, 15),  -- Sticky Web
    (5042, 0, NULL, 15),  -- Mysterious Seal
    (5045, 0, NULL, 5),  -- Throw Mud
    (5049, 0, NULL, 15),  -- Poison
    (5051, 0, NULL, 5),  -- Consecrate Ground
    (5052, 0, NULL, 30),  -- Shield
    (5053, 0, NULL, 30),  -- Bor Graymane's Eye
    (5054, 0, NULL, 75),  -- Destroy Mind
    (5055, 0, NULL, 10),  -- War Sunder
    (5056, 0, NULL, 50),  -- Skull Cleave
    (5057, 0, NULL, 30),  -- Infectious Rage
    (5059, 0, NULL, 15),  -- Bloodwrath
    (5062, 0, NULL, 25),  -- Cleave In Two
    (5063, 0, NULL, 30),  -- Blood Roots
    (5064, 0, NULL, 35),  -- Rage of Khorne
    (5065, 0, NULL, 20),  -- Dead Silence
    (5066, 0, NULL, 20),  -- Bloodpulse
    (5067, 0, NULL, 5),  -- Mighty Strike
    (5068, 0, NULL, 10),  -- Blood Pool
    (5069, 0, NULL, 30),  -- Blood Mark
    (5070, 0, NULL, 15),  -- Breath of Change
    (5071, 0, NULL, 6),  -- Furious Howl
    (5076, 0, NULL, 15),  -- Bloodsoil Poison
    (5085, 0, NULL, 15),  -- Shadow Shard Syphon
    (5086, 0, NULL, 15),  -- Charm Captain Sualthin
    (5087, 0, NULL, 5),  -- Activating...
    (5088, 0, NULL, 5),  -- Activating...
    (5095, 0, NULL, 15),  -- Captivity Crystal
    (5100, 0, NULL, 5),  -- Nature's Grasp
    (5104, 0, NULL, 5),  -- Cleave
    (5105, 0, NULL, 10),  -- Lash
    (5118, 0, NULL, 30),  -- Whirlwind
    (5119, 0, NULL, 25),  -- Knockback
    (5120, 0, NULL, 5),  -- Cleave
    (5121, 0, NULL, 5),  -- Cleave
    (5122, 0, NULL, 20),  -- Gut Bounce
    (5125, 0, NULL, 10),  -- Earthkeepers Howl
    (5126, 0, NULL, 5),  -- Nature's Blast
    (5127, 0, NULL, 10),  -- Life Loss
    (5128, 0, NULL, 10),  -- Nature's Frailty
    (5129, 0, 0, 10),  -- Earthen Spew
    (5130, 0, NULL, 10),  -- Painful Dart
    (5131, 0, NULL, 10),  -- Horrible Pain
    (5132, 0, 0, 4),  -- Crippling Blow
    (5133, 0, NULL, 30),  -- Contagious Poison
    (5134, 0, NULL, 10),  -- Contagious Poison
    (5135, 0, NULL, 10),  -- Forceful Blast
    (5136, 0, NULL, 10),  -- Putrid Breath
    (5137, 5, 5, 4),  -- Crippling Thorns
    (5138, 0, NULL, 10),  -- Suffocation
    (5140, 0, NULL, 5),  -- Toxic Blast
    (5141, 0, NULL, 10),  -- Viletongues Spit
    (5142, 0, 0, 15),  -- Paralyzing Bite
    (5143, 0, NULL, 20),  -- Toxic Wave
    (5144, 0, NULL, 10),  -- Toxic Wave
    (5145, 0, NULL, 3),  -- Ethereal Scream
    (5148, 0, NULL, 5),  -- Twisted Wrath
    (5149, 0, NULL, 20),  -- Psyche Onslaught
    (5150, 0, NULL, 5),  -- Pain of the Vale
    (5151, 0, NULL, 5),  -- Pain of the Vale
    (5152, 0, NULL, 5),  -- Corrupted Loam
    (5153, 0, NULL, 5),  -- Pain Spike
    (5154, 0, NULL, 5),  -- Pain Spike
    (5155, 0, NULL, 5),  -- Spit of Corruption
    (5156, 0, 0, 5),  -- Branch Rake
    (5157, 0, NULL, 5),  -- Branch Rake
    (5158, 0, NULL, 5),  -- Vicious Claw
    (5159, 0, NULL, 5),  -- Vicious Claw
    (5160, 0, NULL, 5),  -- Lumbering Blow
    (5161, 0, NULL, 5),  -- Lumbering Blow
    (5162, 0, NULL, 5),  -- Cobber
    (5163, 0, NULL, 5),  -- Clobber
    (5164, 0, NULL, 5),  -- Brutish Hack
    (5165, 0, NULL, 5),  -- Brutish Hack
    (5166, 0, NULL, 5),  -- Wild Swing
    (5167, 0, NULL, 5),  -- Lucky Jab
    (5168, 0, NULL, 5),  -- Gore
    (5169, 0, NULL, 5),  -- Lacerate
    (5170, 0, NULL, 5),  -- Gash
    (5171, 0, NULL, 5),  -- Massive Strike
    (5172, 0, NULL, 5),  -- Massive Strike
    (5173, 0, NULL, 5),  -- Ravenous Blow
    (5174, 0, NULL, 5),  -- Flurry of Talons
    (5175, 0, NULL, 5),  -- Bear Heavy Strike
    (5176, 0, NULL, 5),  -- Wolf Heavy Strike
    (5177, 0, NULL, 5),  -- Hawk Heavy Strike
    (5178, 0, NULL, 10),  -- Bite
    (5179, 0, NULL, 10),  -- Pummel
    (5180, 0, NULL, 10),  -- Bleed
    (5181, 0, NULL, 5),  -- Cleave
    (5182, 0, NULL, 15),  -- R'khar's Rage
    (5183, 0, NULL, 10),  -- Putrid Breath
    (5184, 0, NULL, 25),  -- Porus' Blessing
    (5185, 0, NULL, 15),  -- Crippling Stomp
    (5186, 0, NULL, 15),  -- Chilling Breath
    (5187, 0, NULL, 10),  -- Lightning Strike
    (5188, 0, NULL, 10),  -- Bleed
    (5189, 0, NULL, 5),  -- Claw Strike
    (5191, 0, NULL, 10),  -- Crippling Fear
    (5192, 0, NULL, 5),  -- Cleave
    (5193, 0, NULL, 7),  -- Pummel
    (5194, 0, NULL, 7),  -- Knockback
    (5195, 0, NULL, 10),  -- Trample
    (5196, 0, NULL, 10),  -- Ragefire
    (5210, 0, NULL, 10),  -- Grenade Blast
    (5211, 15, 15, 0),  -- Heartstopping Snort
    (5215, 15, 15, 0),  -- Bloody Sun Charge
    (5216, 20, 20, 0),  -- Sticky Mushroom
    (5219, 0, NULL, 5),  -- Squig Heal
    (5220, 0, NULL, 20),  -- Hurlesson's Explosive Rune
    (5221, 0, NULL, 10),  -- Flame Thrower
    (5223, 0, NULL, 10),  -- Grenade Blast
    (5224, 0, NULL, 10),  -- Plauge of Boils
    (5225, 0, NULL, 10),  -- Waaagh! Overload
    (5229, 0, NULL, 10),  -- eadbutt
    (5230, 5, 5, 10),  -- Maw Belch
    (5232, 20, 20, 30),  -- Fungal Healing
    (5234, 0, NULL, 30),  -- Reapa Rush
    (5236, 5, 5, 1),  -- Moonflare
    (5240, 60, 60, 0),  -- Gitzappa Aura
    (5241, 15, 15, 30),  -- Kablooey
    (5242, 5, 5, 0),  -- Vile Pool
    (5243, 15, 15, 0),  -- Bye Bye
    (5258, 5, 5, 0),  -- Acidic Muck
    (5259, 5, 5, 10),  -- Acidic Arrer
    (5262, 5, 5, 0),  -- Squig Commanda
    (5263, 3, 3, 30),  -- Smash 'Em 'Ard
    (5304, 10, 10, 9),  -- Slimy Vomit
    (5323, 300, 300, 10),  -- Yesterday's Slop
    (5324, 5, 5, 0),  -- Yesterday's Grog
    (5347, 0, 0, 1),  -- Bestial Flurry
    (5400, 0, NULL, 5),  -- Cleave
    (5401, 0, NULL, 20),  -- Stomp
    (5402, 0, NULL, 5),  -- Spine Shot
    (5403, 0, NULL, 30),  -- Blood Frenzy
    (5404, 0, NULL, 10),  -- Charge
    (5405, 0, NULL, 5),  -- Cleave
    (5406, 0, NULL, 5),  -- Axe Throw
    (5407, 0, NULL, 15),  -- Unholy Wave
    (5408, 0, NULL, 20),  -- Hatred
    (5409, 0, NULL, 8),  -- Blast of the Dead
    (5410, 0, NULL, 6),  -- Scream
    (5411, 0, NULL, 5),  -- Crypt Blast
    (5412, 0, NULL, 15),  -- Touch of the Dead
    (5414, 5, 5, 10),  -- Poison Spit
    (5415, 0, NULL, 15),  -- Wracking Pain
    (5417, 0, NULL, 5),  -- Keeper Blast
    (5418, 0, NULL, 10),  -- Scatter Shot
    (5419, 0, NULL, 5),  -- Shot
    (5420, 0, NULL, 4),  -- Cleave
    (5421, 0, NULL, 5),  -- Sonic Screech
    (5422, 0, NULL, 10),  -- Charge
    (5423, 0, NULL, 5),  -- Blast of Devotion
    (5424, 0, NULL, 15),  -- Lifetap
    (5435, 0, NULL, 5),  -- Devastation
    (5458, 10, 10, 0),  -- Infectious Poison
    (5459, 10, 10, 0),  -- Infectious Poison
    (5460, 10, 10, 0),  -- Infectious Poison
    (5467, 0, 0, 20),  -- Tentacle Knock up
    (5478, 0, 0, 10),  -- Plucked Armor
    (5549, 0, 0, 5),  -- Cleave
    (5568, 0, 0, 10),  -- Whirlwind
    (5575, 0, 0, 10),  -- Enfeebling Shout
    (5576, 0, 0, 45),  -- Enfeeble
    (5594, 5, 5, 20),  -- Gut Spew
    (5627, 5, 5, 0),  -- Troll Vomit
    (5688, 0, 0, 20),  -- Low Blow
    (5693, 10, 10, 0),  -- I'm On Fire
    (5731, 0, 0, 10),  -- Throw Tomato
    (5802, 30, 30, 15),  -- Frenzy
    (5806, 15, 15, 25),  -- Disabling Strike
    (7090, 1800, 1800, 0),  -- Precision
    (7092, 60, 60, 300),  -- Warming Draught
    (7093, 60, 60, 300),  -- Vitalizing Draught
    (7094, 60, 60, 300),  -- Exhilirating Draught
    (7095, 60, 60, 300),  -- Energizing Draught
    (7096, 60, 60, 300),  -- Stimulating Draught
    (7097, 60, 60, 300),  -- Quickening Draught
    (7098, 60, 60, 300),  -- Charging Draught
    (7099, 60, 60, 300),  -- Invigorating Draught
    (7111, 300, 300, 120),  -- Nurgle's Bane Draught
    (7260, 300, 300, 120),  -- Fleeting Nurgle's Bane Elixir
    (7261, 300, 300, 120),  -- Brief Nurgle's Bane Elixir
    (7262, 300, 300, 120),  -- Nurgle's Bane Elixir
    (7263, 300, 300, 120),  -- Enduring Nurgle's Bane Elixir
    (7264, 300, 300, 120),  -- Lasting Nurgle's Bane Elixir
    (7871, 180, 180, 0),  -- Minor Potion Of Healing
    (7872, 180, 180, 0),  -- Potion Of Healing
    (7873, 180, 180, 0),  -- Major Potion Of Healing
    (7874, 180, 180, 0),  -- Fleeting Elixir of Allaying
    (7875, 120, 120, 0),  -- Brief Elixir of Allaying
    (7876, 180, 180, 0),  -- Elixir of Allaying
    (7877, 180, 180, 0),  -- Enduring Elixir of Allaying
    (7878, 180, 180, 0),  -- Lasting Elixir of Allaying
    (7879, 180, 180, 0),  -- Fleeting Elixir of Mending
    (7880, 120, 120, 0),  -- Brief Elixir of Mending
    (7881, 180, 180, 0),  -- Elixir of Mending
    (7882, 180, 180, 0),  -- Enduring Elixir of Mending
    (7883, 180, 180, 0),  -- Lasting Elixir of Mending
    (7884, 180, 180, 0),  -- Fleeting Elixir of Recovery
    (7885, 120, 120, 0),  -- Brief Elixir of Recovery
    (7886, 180, 180, 0),  -- Elixir of Recovery
    (7887, 180, 180, 0),  -- Enduring Elixir of Recovery
    (7888, 180, 180, 0),  -- Lasting Elixir of Recovery
    (7896, 300, 300, 0),  -- Lasting Screening Potion
    (7897, 300, 300, 0),  -- Fleeting Potion of Strength
    (7898, 300, 300, 0),  -- Brief Potion of Strength
    (7899, 300, 300, 0),  -- Potion of Strength
    (7900, 300, 300, 0),  -- Enduring Potion of Strength
    (7901, 300, 300, 0),  -- Lasting Potion of Strength
    (7902, 300, 300, 0),  -- Fleeting Potion of Knowledge
    (7903, 300, 300, 0),  -- Brief Potion of Knowledge
    (7904, 300, 300, 0),  -- Potion of Knowledge
    (7905, 300, 300, 0),  -- Enduring Potion of Knowledge
    (7906, 300, 300, 0),  -- Lasting Potion of Knowledge
    (7907, 300, 300, 0),  -- Fleeting Potion of Accuracy
    (7908, 300, 300, 0),  -- Brief Potion of Accuracy
    (7909, 300, 300, 0),  -- Potion of Accuracy
    (7910, 300, 300, 0),  -- Enduring Potion of Accuracy
    (7911, 300, 300, 0),  -- Lasting Potion of Accuracy
    (7912, 300, 300, 0),  -- Fleeting Liquid Fortitude
    (7913, 300, 300, 0),  -- Brief Liquid Fortitude
    (7914, 300, 300, 0),  -- Liquid Fortitude
    (7915, 300, 300, 0),  -- Enduring Liquid Fortitude
    (7916, 300, 300, 0),  -- Lasting Liquid Fortitude
    (7917, 300, 300, 0),  -- Fleeting Spirit Guard Potion
    (7918, 300, 300, 0),  -- Brief Spirit Guard Potion
    (7919, 300, 300, 0),  -- Spirit Guard Potion
    (7920, 300, 300, 0),  -- Enduring Spirit Guard Potion
    (7921, 300, 300, 0),  -- Lasting Spirit Guard Potion
    (7922, 300, 300, 0),  -- Fleeting Elemental Guard Potion
    (7923, 300, 300, 0),  -- Brief Elemental Guard Potion
    (7924, 300, 300, 0),  -- Elemental Guard Potion
    (7925, 300, 300, 0),  -- Enduring Elemental Guard Potion
    (7926, 300, 300, 0),  -- Lasting Elemental Guard Potion
    (7927, 300, 300, 0),  -- Fleeting Corporeal Guard Potion
    (7928, 300, 300, 0),  -- Brief Corporeal Guard Potion
    (7929, 300, 300, 0),  -- Corporeal Guard Potion
    (7930, 300, 300, 0),  -- Enduring Corporeal Guard Potion
    (7931, 300, 300, 0),  -- Lasting Corporeal Guard Potion
    (7932, 300, 300, 0),  -- Fleeting Bracing Unguent
    (7933, 300, 300, 0),  -- Brief Bracing Unguent
    (7934, 300, 300, 0),  -- Bracing Unguent
    (7935, 300, 300, 0),  -- Enduring Bracing Unguent
    (7936, 300, 300, 0),  -- Lasting Bracing Unguent
    (7962, 300, 300, 0),  -- Fleeting Potion of Wisdom
    (7963, 300, 300, 0),  -- Brief Potion of Wisdom
    (7964, 300, 300, 0),  -- Potion of Wisdom
    (7965, 300, 300, 0),  -- Enduring Potion of Wisdom
    (7966, 300, 300, 0),  -- Lasting Potion of Wisdom
    (8292, 30, 30, 0),  -- Grace of Sigmar
    (8490, 10, 10, 0),  -- Daemonic Resistance
    (8565, 0, 0, 30),  -- Tzeentch's Lash
    (9033, 10, 10, 0),  -- Aethyric Armor
    (9159, 0, 0, 15),  -- Call War Lion
    (9255, 0, 0, 60),  -- Wind Blast
    (9481, 0, 0, 60),  -- Obsessive Focus
    (9574, 0, 0, 60),  -- Soul Shielding
    (10347, 60, 60, 120),  -- Restraining Shot
    (10352, 60, 60, 120),  -- Lion's Savagery
    (10358, 300, 300, 120),  -- Sadist
    (10721, 15, 15, 0),  -- Gyrocaptain's Pocket Watch
    (10722, 15, 15, 0),  -- Steamtank Whistle
    (10726, 600, 600, 0),  -- Shroud of Imrathepis
    (10767, 600, 600, 0),  -- Shield of Sand
    (10768, 3600, 3600, 0),  -- Shield of Bones
    (10954, 3600, 3600, 0),  -- Fleet Stag Mantle
    (13134, 0, 0, 1),  -- Brain Bursta
    (13383, 0, 0, 40),  -- Crippling Enmity
    (14211, 300, 300, 0),  -- Magnificent Fireworks
    (14212, 300, 300, 0),  -- Impressive Fireworks
    (14213, 300, 300, 0),  -- Common Fireworks
    (14242, 300, 300, 0),  -- Grimnir's Mercy
    (14245, 10, 10, 0),  -- Burning Blood Potion
    (14378, 3, 3, 4),  -- Cannon
    (14379, 5, 5, 4),  -- Organ Gun
    (14382, 3, 3, 4),  -- Spear Chukka
    (14383, 5, 5, 4),  -- Cannon
    (14386, 3, 3, 4),  -- Cannon
    (14387, 5, 5, 4),  -- Hellblaster
    (14390, 3, 3, 4),  -- Hellcannon
    (14391, 5, 5, 4),  -- Tri-Barrel Hellcannon
    (14394, 3, 3, 4),  -- Ballista
    (14395, 5, 5, 4),  -- Repeater Bolt Thrower
    (14398, 3, 3, 4),  -- Ballista
    (14399, 5, 5, 4),  -- Repeater Bolt Thrower
    (14435, 8, 8, 10),  -- Boiling Oil
    (14440, 8, 8, 10),  -- Boiling Oil
    (14445, 8, 8, 10),  -- Boiling Oil
    (14450, 8, 8, 10),  -- Boiling Oil
    (14455, 8, 8, 10),  -- Boiling Oil
    (14460, 8, 8, 10),  -- Boiling Oil
    (14478, 3600, 3600, 0),  -- Teleporting
    (14479, 3600, 3600, 0),  -- Teleporting
    (14480, 60, 60, 0),  -- Teleporting
    (14864, 0, 0, 15),  -- Rupture Organ
    (14867, 0, 0, 180),  -- Iron Body
    (14873, 0, 0, 180),  -- Iron Body
    (14879, 0, 0, 180),  -- Iron Body
    (14885, 0, 0, 180),  -- Iron Body
    (14891, 15, 15, 180),  -- Iron Body
    (14897, 0, 0, 180),  -- Iron Body
    (14900, 0, 0, 15),  -- Clip Tendon
    (14938, 0, 0, 300),  -- Baleful Affliction
    (14945, 0, 0, 180),  -- Convalesce
    (14948, 0, 0, 15),  -- Impose Suffering
    (15059, 300, 300, 0),  -- Fleeting Renown Boost
    (15060, 300, 300, 0),  -- Lasting Renown Boost
    (15147, 3600, 3600, 0),  -- Glory of War
    (15148, 300, 300, 0),  -- Charger's Charm
    (15151, 300, 300, 0),  -- Chuffinbrau Ale
    (15159, 1800, 1800, 0),  -- Blessing of the Horned Rat
    (15161, 0, 0, 3),  -- Custard Pie
    (15169, 300, 300, 0),  -- The Librams of Insight
    (15171, 1800, 1800, 0),  -- Signet of the Cursed Company
    (15175, 1800, 1800, 0),  -- Call of the North
    (15177, 1800, 1800, 0),  -- Glory
    (15178, 1800, 1800, 0),  -- Seething
    (15181, 5, 5, 0),  -- Dwarf Beer Keg
    (15182, 3600, 3600, 0),  -- Last Call Keg
    (15183, 3600, 3600, 0),  -- Last Call Keg
    (15184, 300, 300, 0),  -- Explosive Ale
    (15185, 300, 300, 0),  -- Explosive Porter
    (15186, 300, 300, 0),  -- Explosive Stout
    (15187, 300, 300, 0),  -- Explosive Lager
    (15205, 10, 10, 0),  -- Minor Instructor's Boon
    (15981, 3600, 3600, 0),  -- Nepenthean Tonic
    (20003, 15, 15, 0),  -- Test
    (20224, 0, 0, 12),  -- Sapping Strike
    (20361, 10, 10, 0),  -- Squig Armor
    (20375, 0, 0, 16),  -- Acid Bomb
    (20436, 0, 0, 16),  -- Consume Essence
    (20518, 0, 0, 16),  -- Divine Strike
    (20649, 0, 0, 12),  -- Sun Scales
    (21283, 0, 0, 10),  -- Scorpion Strike
    (21320, 0, 0, 16),  -- Scorpion's Power
    (21372, 0, 0, 14),  -- Accelerated Intimation
    (21421, 0, 0, 14),  -- Aura of Restoration
    (22914, 1800, 1800, 0),  -- Flawless Bear Form
    (23012, 0, 0, 8),  -- Flame Breath
    (23584, 0, 0, 5),  -- Terminate
    (23666, 180, 180, 0),  -- Deploy Ram
    (23668, 180, 180, 0),  -- Deploy Cannon
    (23670, 180, 180, 0),  -- Deploy Organ Gun
    (23674, 180, 180, 0),  -- Deploy Supa-Chucka
    (23676, 180, 180, 0),  -- Deploy Orcapult
    (23680, 180, 180, 0),  -- Deploy Ballista
    (23682, 180, 180, 0),  -- Deploy Bolt Thrower
    (23686, 180, 180, 0),  -- Deploy Ballista
    (23688, 180, 180, 0),  -- Deploy Bolt Thrower
    (23741, 10, 10, 0),  -- Unstable Necromancy
    (23918, 180, 180, 0),  -- Deploy Ram
    (23960, 180, 180, 0),  -- Deploy Ram
    (23966, 180, 180, 0),  -- Deploy Ram
    (24664, 180, 180, 0),  -- Deploy Cannon
    (24666, 180, 180, 0),  -- Deploy Hellblaster
    (24670, 180, 180, 0),  -- Deploy Hellcannon
    (24672, 180, 180, 0),  -- Deploy Tri-Barrel Hellcannon
    (24770, 180, 180, 0),  -- Deploy Ram
    (24776, 180, 180, 0),  -- Deploy Ram
    (24824, 1, 1, 20),  -- Snare Net
    (27773, 20, 20, 10),  -- Get Down!
    (28300, 55, 55, 120);  -- Inexorable Force

UPDATE mythic_src_abilities m
  JOIN tmp_03_client_cooldown c ON c.Entry = m.Entry
   SET m.AICooldown = GREATEST(m.AICooldown, c.OldSrc), m.Cooldown = c.ClientSeconds
 WHERE COALESCE(m.Cooldown, 0) = c.OldSrc;

UPDATE abilities a
  JOIN tmp_03_client_cooldown c ON c.Entry = a.Entry
   SET a.AICooldown = GREATEST(a.AICooldown, c.OldAbl), a.Cooldown = c.ClientSeconds
 WHERE COALESCE(a.Cooldown, 0) = c.OldAbl;

DROP TEMPORARY TABLE tmp_03_client_cooldown;

COMMIT;

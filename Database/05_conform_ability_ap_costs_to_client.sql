-- 05_conform_ability_ap_costs_to_client.sql
--
-- Sets 1309 ability AP costs to the client's.
--
-- POLICY. The 1.4.8 client is the arbiter, and data/bin/abilityexport.bin's ApCost is the same unit as
-- ours. Only players spend AP (Unit.HasActionPoints is true for everything else), so the creature rows
-- below change nothing a creature does; they conform because the client says so.
--
-- NOT HERE: 20 channels. The server charges a channel's ApCost on every one-second tick
-- (NewChannelHandler) while the client ticks at its own ChannelInterval, so the same number means a
-- different total. They conform with the channel handler.
--
-- Both tables are written (CLAUDE.md hard rule 1). Each carries its own old value, so a row where
-- abilities and mythic_src_abilities had drifted apart still conforms in both.
--
-- Safe to re-run: every UPDATE matches only the old value, per table. Ability data is cached at
-- boot, so restart the server.

USE war_world;

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_05_client_ap;
CREATE TEMPORARY TABLE tmp_05_client_ap (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    OldSrc INT UNSIGNED NULL,
    OldAbl INT UNSIGNED NULL,
    ClientAp INT UNSIGNED NOT NULL
);

-- Entry, mythic_src_abilities.ApCost and abilities.ApCost where they differ from the client (NULL read
-- as 0; NULL here means that table already agrees), the client's AP cost.
INSERT INTO tmp_05_client_ap (Entry, OldSrc, OldAbl, ClientAp) VALUES
    (70, 0, NULL, 15),  -- Bolt Thrower
    (77, 0, NULL, 35),  -- Lightning Test 1 (proj to all from cast)
    (78, 0, NULL, 35),  -- Lightning Test 2 (ptp after CTAR)
    (85, 0, NULL, 25),  -- Sticky Grenade TEST For Nate
    (127, 0, NULL, 35),  -- Main Hand Weap
    (128, 0, NULL, 35),  -- Off Hand Weap
    (129, 0, NULL, 35),  -- Both Hand Weap
    (130, 0, NULL, 35),  -- Tags on Main (Both have fx trails)
    (131, 0, NULL, 35),  -- Tags on Off (Both have fx trails)
    (133, 0, NULL, 15),  -- Test Charm
    (136, 0, NULL, 35),  -- Black Fire Breath
    (137, 0, NULL, 35),  -- Black Fireball (Small)
    (138, 0, NULL, 35),  -- Black Fireball (AOE)
    (139, 0, NULL, 35),  -- Sun Fire Breath
    (140, 0, NULL, 35),  -- Sun Fireball (Small)
    (141, 0, NULL, 35),  -- Sun Fireball (AOE)
    (142, 0, NULL, 35),  -- Bite Single Target
    (143, 0, NULL, 35),  -- Multi Target Claw Swipe
    (144, 0, NULL, 35),  -- Stomp
    (156, 0, 0, 35),  -- Effect Test
    (158, 0, NULL, 35),  -- Test T1R1 Attack
    (159, 0, NULL, 35),  -- Test T1R2 Attack
    (160, 0, NULL, 35),  -- Test T1R3 Attack
    (161, 0, NULL, 35),  -- Test T1R4 Attack
    (162, 0, NULL, 35),  -- Test T1R5 Attack
    (163, 0, NULL, 35),  -- Test T1R6 Attack
    (164, 0, NULL, 35),  -- Test T1R7 Attack
    (165, 0, NULL, 35),  -- Test T1R8 Attack
    (166, 0, NULL, 35),  -- Test T1R9 Attack
    (167, 0, NULL, 35),  -- Test T1R10 Attack
    (168, 0, NULL, 35),  -- Test T2R1 Attack
    (169, 0, NULL, 35),  -- Test T2R2 Attack
    (170, 0, NULL, 35),  -- Test T2R3 Attack
    (171, 0, NULL, 35),  -- Test T2R4 Attack
    (172, 0, NULL, 35),  -- Test T2R5 Attack
    (173, 0, NULL, 35),  -- Test T2R6 Attack
    (174, 0, NULL, 35),  -- Test T2R7 Attack
    (175, 0, NULL, 35),  -- Test T2R8 Attack
    (176, 0, NULL, 35),  -- Test T2R9 Attack
    (177, 0, NULL, 35),  -- Test T2R10 Attack
    (178, 0, NULL, 35),  -- Test T3R1 Attack
    (179, 0, NULL, 35),  -- Test T3R2 Attack
    (180, 0, NULL, 35),  -- Test T3R3 Attack
    (181, 0, NULL, 35),  -- Test T3R4 Attack
    (182, 0, NULL, 35),  -- Test T3R5 Attack
    (183, 0, NULL, 35),  -- Test T3R6 Attack
    (184, 0, NULL, 35),  -- Test T3R7 Attack
    (185, 0, NULL, 35),  -- Test T3R8 Attack
    (186, 0, NULL, 35),  -- Test T3R9 Attack
    (187, 0, NULL, 35),  -- Test T3R10 Attack
    (189, 0, NULL, 35),  -- Test T2R5 Ranged Attack
    (190, 0, NULL, 35),  -- Test T2R5 Magic Attack
    (191, 0, NULL, 35),  -- Test Huge Melee Crit Buff
    (192, 0, NULL, 35),  -- Test Huge Ranged Crit Buff
    (193, 0, NULL, 35),  -- Test Huge Magic Crit Buff
    (199, 0, NULL, 15),  -- Chad's Test Knockback abil
    (228, 0, NULL, 35),  -- Loot buff
    (232, 0, NULL, 20),  -- New Taunt
    (233, 0, NULL, 20),  -- New AE Taunt
    (235, 0, NULL, 20),  -- PR- Tooltip Harbinger of Doom
    (242, 0, NULL, 45),  -- Juggernaut
    (410, 0, NULL, 20),  -- Unearthly Shriek
    (1200, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1201, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1202, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1203, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1204, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1205, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1206, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1207, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1208, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1209, 0, NULL, 20),  -- Anim Tag Test - Throw
    (1210, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1211, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1212, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1213, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1214, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1215, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1216, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1217, 0, NULL, 35),  -- Anim Tag Test - Instant
    (1218, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1219, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1220, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1221, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1222, 0, NULL, 35),  -- Anim Tag Test - Channel
    (1223, 0, NULL, 20),  -- Anim Tag Test - Throw
    (1224, 0, NULL, 20),  -- Anim Tag Test - Throw
    (1225, 0, NULL, 20),  -- Anim Tag Test - Throw
    (1226, 0, NULL, 20),  -- Anim Tag Test - Throw
    (1227, 0, NULL, 20),  -- Anim Tag Test - Throw
    (1228, 0, NULL, 20),  -- Anim Tag Test - Throw
    (1229, 0, NULL, 20),  -- Anim Tag Test - Throw
    (1775, 25, 25, 45),  -- Chop Fasta!
    (1865, 0, NULL, 20),  -- Drop That!!
    (1921, 0, NULL, 55),  -- Gork's Buffer
    (1922, 0, NULL, 55),  -- Speedy Hands
    (3498, 0, NULL, 150),  -- Sub Ability
    (3947, 0, 0, 20),  -- Unearthly Shriek
    (3997, 0, NULL, 35),  -- Dissolving Mist
    (4020, 0, NULL, 180),  -- Ghostly Howl
    (4021, 0, NULL, 240),  -- Fire Breath
    (4022, 0, NULL, 240),  -- Buffet
    (4023, 0, NULL, 160),  -- Entangle
    (4024, 0, NULL, 60),  -- Vicious Charge
    (4025, 0, NULL, 90),  -- Impale
    (4026, 0, NULL, 60),  -- Eviscerate
    (4027, 0, NULL, 80),  -- Gore
    (4028, 0, NULL, 90),  -- Maul
    (4029, 0, NULL, 60),  -- Rend
    (4030, 0, NULL, 180),  -- Swipe
    (4031, 0, NULL, 60),  -- Taunting Strike
    (4032, 0, NULL, 160),  -- Engage
    (4033, 0, NULL, 90),  -- Crack Shot
    (4034, 0, NULL, 60),  -- Snipe
    (4035, 0, NULL, 180),  -- Volley
    (4036, 0, NULL, 180),  -- Blunderbus Blast
    (4037, 0, NULL, 60),  -- Fireball
    (4038, 0, NULL, 80),  -- Flaming Sword of Rhuin
    (4039, 0, NULL, 210),  -- The Burning Head
    (4040, 0, NULL, 90),  -- Fiery Blast
    (4041, 0, NULL, 50),  -- Conflagration of Doom
    (4042, 0, NULL, 120),  -- Rule of Burning Iron
    (4043, 0, NULL, 160),  -- Commandment of Brass
    (4044, 0, NULL, 80),  -- Transmutation of Lead
    (4045, 0, NULL, 80),  -- Distillation of Molten Silver
    (4046, 0, NULL, 180),  -- The Spirit of the Forge
    (4047, 0, NULL, 30),  -- Creeping Death
    (4048, 0, NULL, 180),  -- Crown of Taidron
    (4049, 0, NULL, 80),  -- Shades of Death
    (4050, 0, NULL, 180),  -- Pit of Shades
    (4051, 0, NULL, 80),  -- The Bear's Anger
    (4052, 0, NULL, 80),  -- The Oxen Stands
    (4053, 0, NULL, 60),  -- The Crow's Feast
    (4054, 0, NULL, 160),  -- The Beast Cowers
    (4055, 0, NULL, 210),  -- The Hunter's Spear
    (4056, 0, NULL, 80),  -- The Wolf Hunts
    (4057, 0, NULL, 80),  -- Portent of Far
    (4058, 0, NULL, 80),  -- Second Sign of Amul
    (4059, 0, NULL, 120),  -- Celestial Shield
    (4060, 0, NULL, 60),  -- Forked Lightning
    (4061, 0, NULL, 180),  -- Uranon's Thunderbolt
    (4062, 0, NULL, 44),  -- The Comet of Casandora
    (4063, 0, NULL, 60),  -- Burning Gaze
    (4064, 0, NULL, 80),  -- Pha's Illumination
    (4065, 0, NULL, 160),  -- Healing Energy
    (4066, 0, NULL, 240),  -- Dazzling Brightness
    (4067, 0, NULL, 180),  -- Cleansing Flare
    (4068, 0, NULL, 160),  -- Mistress of the Marsh
    (4069, 0, NULL, 60),  -- Master of the Wood
    (4070, 0, NULL, 160),  -- Gift of Life
    (4071, 0, NULL, 160),  -- The Howler Wind
    (4072, 0, NULL, 160),  -- The Rain Lord
    (4073, 0, NULL, 180),  -- Master of Stone
    (4074, 0, NULL, 80),  -- Dark Hand of Death
    (4075, 0, NULL, 60),  -- Steal Soul
    (4076, 0, NULL, 60),  -- Wind of Death
    (4077, 0, NULL, 160),  -- Doom and Darkness
    (4078, 0, NULL, 180),  -- Drain Life
    (4079, 0, NULL, 110),  -- Magnificent Buboes
    (4080, 0, NULL, 240),  -- Favoured Poxes
    (4081, 0, NULL, 30),  -- Effulgent Boils
    (4082, 0, NULL, 80),  -- Glistening Scabs
    (4083, 0, NULL, 80),  -- Glorious Afflictions
    (4084, 0, NULL, 40),  -- Sumptuous Pestilence
    (4085, 0, NULL, 60),  -- Red Fire of Alteration
    (4086, 0, NULL, 80),  -- Orange Fire of Transition
    (4087, 0, NULL, 120),  -- Yellow Fire of Transformation
    (4088, 0, NULL, 90),  -- Blue Fire of Metamorphosis
    (4089, 0, NULL, 180),  -- Indigo Fire of Change
    (4090, 0, NULL, 60),  -- Violet Fire of Tzeentch
    (4091, 0, NULL, 60),  -- Blissful Throes
    (4092, 0, NULL, 80),  -- Luxuroius Torment
    (4093, 0, NULL, 240),  -- Titillating Delusions
    (4094, 0, NULL, 160),  -- Enrapturing Spasms
    (4095, 0, NULL, 240),  -- Invocation of Nehek
    (4096, 0, NULL, 80),  -- Hand of Dust
    (4097, 0, NULL, 160),  -- Hellish Vigour
    (4098, 0, NULL, 60),  -- Gaze of Nagash
    (4099, 0, NULL, 160),  -- Vanhel's Danse Macabre
    (4100, 0, NULL, 90),  -- Curse of Years
    (4101, 0, NULL, 60),  -- Gaze of Gork
    (4102, 0, NULL, 80),  -- Brain Bursta
    (4103, 0, NULL, 240),  -- Gork'll Fix It
    (4104, 0, NULL, 180),  -- Fists of Gork
    (4105, 0, NULL, 160),  -- Waaagh
    (4106, 0, NULL, 100),  -- Mork Wants Ya
    (4107, 0, NULL, 80),  -- Bash 'Em Ladz
    (4108, 0, NULL, 160),  -- Bloodgruel
    (4109, 0, NULL, 120),  -- Braingobbler
    (4110, 0, NULL, 160),  -- Bullgorger
    (4111, 0, NULL, 60),  -- Bonecruncher
    (4112, 0, NULL, 80),  -- Toothcracker
    (4113, 0, NULL, 160),  -- Trollguts
    (4114, 0, NULL, 90),  -- Warp Lightning
    (4115, 0, NULL, 180),  -- Pestelent Breath
    (4116, 0, NULL, 90),  -- Vermintide
    (4117, 0, NULL, 210),  -- Pestilence
    (4118, 0, NULL, 80),  -- Death Frenzy
    (4119, 0, NULL, 160),  -- Mark of Fury
    (4120, 0, NULL, 160),  -- Mark of Tzeentch
    (4121, 0, NULL, 160),  -- Hunter's Mark
    (4122, 0, NULL, 160),  -- Mark of Khorne
    (4123, 0, NULL, 60),  -- Headbutt
    (4124, 0, NULL, 120),  -- Flurry
    (4125, 0, NULL, 180),  -- Syphon Life
    (4126, 0, NULL, 240),  -- Stomp
    (4127, 0, NULL, 180),  -- Whirlwind
    (4128, 0, NULL, 180),  -- Silence
    (4129, 0, NULL, 180),  -- Disarm
    (4130, 0, NULL, 180),  -- Cripple
    (4131, 0, NULL, 44),  -- Hush
    (4132, 0, NULL, 44),  -- Rust
    (4133, 0, NULL, 44),  -- Crippling Blast
    (4135, 0, NULL, 90),  -- Crack Shot
    (4136, 0, NULL, 60),  -- Snipe
    (4137, 0, NULL, 90),  -- Throw Axe
    (4138, 0, NULL, 90),  -- Throw Rock
    (4144, 0, NULL, 90),  -- Finishing Blow
    (4145, 0, NULL, 60),  -- Poison Wind Globe
    (4148, 0, NULL, 240),  -- Seething Plague
    (4149, 0, NULL, 180),  -- Aqshi Unbound
    (4195, 0, NULL, 150),  -- Rock Skin
    (4196, 0, NULL, 150),  -- Electric Skin
    (4197, 0, NULL, 150),  -- Electric Skin
    (4200, 0, NULL, 40),  -- Lileath's Tear
    (4201, 0, NULL, 40),  -- Blessing of Khaine
    (4203, 0, NULL, 40),  -- Lantern Effect Buff
    (4207, 0, NULL, 40),  -- Hydra Breath
    (4210, 0, NULL, 40),  -- Regen
    (4211, 0, NULL, 40),  -- Dark Elf Looter Effect
    (4212, 0, NULL, 40),  -- Monster Stealth
    (4213, 0, NULL, 40),  -- Snare
    (4214, 0, NULL, 40),  -- Enraged
    (4215, 0, NULL, 40),  -- Average Raid Boss DMG Ability
    (4216, 0, NULL, 40),  -- HIgh DMG Raid Boss Ability
    (4217, 0, NULL, 160),  -- Entangling Webs
    (4219, 0, NULL, 40),  -- Glamour
    (4221, 0, NULL, 50),  -- Bloodsoil Poison
    (4223, 0, NULL, 40),  -- Power Channel Summon
    (4224, 0, NULL, 40),  -- Clynch Test Ability
    (4226, 0, NULL, 40),  -- Sharp Projectile
    (4227, 0, NULL, 40),  -- Fire Shot
    (4228, 0, NULL, 40),  -- Raid Boss Test Ability 1 (kick)
    (4229, 0, NULL, 40),  -- Raid Boss Test Ability 2 (AE Whirl)
    (4230, 0, NULL, 40),  -- Raid Boss Test Ability 3 (High Damage with Prep)
    (4231, 0, NULL, 150),  -- Ability for AZ
    (4232, 0, NULL, 1),  -- Summon Construct
    (4233, 0, NULL, 1),  -- Launch Player
    (4234, 0, NULL, 1),  -- Safe Fall
    (4235, 0, NULL, 40),  -- Chimeral Talisman
    (4236, 0, NULL, 40),  -- CHRIS - Long Duration Stun
    (4237, 0, NULL, 40),  -- Effulgent Boils
    (4238, 0, NULL, 1),  -- Clynch Rat
    (4239, 0, NULL, 40),  -- VOG
    (4240, 0, NULL, 40),  -- Beast Melee 1
    (4241, 0, NULL, 40),  -- Beast Melee 2
    (4242, 0, NULL, 40),  -- Beast Melee Cleave
    (4243, 0, NULL, 40),  -- Beast Melee Whirlwind
    (4244, 0, NULL, 40),  -- Beast Long Prep Melee 2
    (4245, 0, NULL, 40),  -- Beast Long Prep Melee 2
    (4246, 0, NULL, 40),  -- Beast Long Prep Stomp
    (4247, 0, NULL, 40),  -- Beast Roar
    (4248, 0, NULL, 40),  -- Beast Cast
    (4249, 0, NULL, 150),  -- Beast Cast PBAE
    (4250, 0, NULL, 40),  -- Beast Long Prep Cast
    (4251, 0, NULL, 150),  -- Beast Long Prep Cast PBAE
    (4253, 0, NULL, 40),  -- Humanoid Melee 1
    (4254, 0, NULL, 40),  -- Humanoid Melee 2
    (4255, 0, NULL, 40),  -- Humanoid Melee Cleave
    (4256, 0, NULL, 40),  -- Humanoid Melee Whirl
    (4257, 0, NULL, 40),  -- Humanoid Cast
    (4258, 0, NULL, 150),  -- Humanoid Cast PBAE
    (4259, 0, NULL, 40),  -- Humanoid Long Prep Cast
    (4260, 0, NULL, 150),  -- Humanoid Long Prep Cast PBAE
    (4261, 0, NULL, 40),  -- Humanoid Instant Cast
    (4262, 0, NULL, 150),  -- Humanoid Instant Cast PBAE
    (4264, 0, NULL, 40),  -- Chimeral Talisman
    (4274, 0, NULL, 40),  -- Cleave
    (4275, 0, NULL, 40),  -- Cleave
    (4276, 0, NULL, 40),  -- Killing Blow
    (4277, 0, NULL, 40),  -- Sword Gorge
    (4278, 0, NULL, 40),  -- Shield Slam
    (4279, 0, NULL, 1),  -- Shield Wall
    (4280, 0, NULL, 1),  -- Volkmar - Jade Griffon
    (4281, 0, NULL, 40),  -- Volkmar - Staff of Command
    (4282, 0, NULL, 40),  -- Volkmar - Theogonist's Smite
    (4290, 0, NULL, 40),  -- ability
    (4291, 0, NULL, 40),  -- ability
    (4292, 0, NULL, 40),  -- ability
    (4300, 0, NULL, 40),  -- Assault of Khaine
    (4301, 0, NULL, 40),  -- Rival's Ruin
    (4302, 0, NULL, 40),  -- Assault of Khaine
    (4303, 0, NULL, 40),  -- Rival's Ruin
    (4304, 0, NULL, 40),  -- Severing Slaughter
    (4305, 0, NULL, 40),  -- Descending Doom
    (4306, 0, NULL, 40),  -- Severing Slaughter
    (4307, 0, NULL, 40),  -- Descending Doom
    (4308, 0, NULL, 40),  -- Severing Slaughter
    (4309, 0, NULL, 40),  -- Descending Doom
    (4310, 0, NULL, 40),  -- Impaling Ruin
    (4311, 0, NULL, 40),  -- Manticore Sting
    (4312, 0, NULL, 40),  -- Eviscerating Blow
    (4313, 0, NULL, 40),  -- Hydra Strike
    (4314, 0, NULL, 40),  -- Vaul's Hammer
    (4315, 0, NULL, 40),  -- Phoenix Wing
    (4316, 0, NULL, 40),  -- Vaul's Hammer
    (4317, 0, NULL, 40),  -- Phoenix Wing
    (4318, 0, NULL, 40),  -- Balanced Stroke
    (4319, 0, NULL, 40),  -- Asuryan's Mercy
    (4320, 0, NULL, 40),  -- Balanced Stroke
    (4321, 0, NULL, 40),  -- Asuryan's Mercy
    (4322, 0, NULL, 40),  -- Balanced Stroke
    (4323, 0, NULL, 40),  -- Asuryan's Mercy
    (4324, 0, NULL, 40),  -- Balanced Stroke
    (4325, 0, NULL, 40),  -- Asuryan's Mercy
    (4326, 0, NULL, 40),  -- Kiss of Calamity
    (4327, 0, NULL, 40),  -- Reaping Bolt
    (4328, 0, NULL, 40),  -- Seafarer's Volley
    (4329, 0, NULL, 40),  -- Vengeance of Avelorn
    (4330, 0, NULL, 40),  -- Thunderous Blow
    (4332, 0, NULL, 1),  -- King Fight - Whirlwind Counter
    (4333, 0, NULL, 40),  -- Holy Wrath
    (4334, 0, NULL, 40),  -- Divine Wrath
    (4335, 0, NULL, 40),  -- Cracking Blow
    (4336, 0, NULL, 40),  -- Deafening Screech
    (4337, 0, NULL, 40),  -- Deafening Screech
    (4338, 0, 0, 40),  -- Wing Buffet
    (4339, 0, NULL, 40),  -- Death's Render
    (4340, 0, NULL, 40),  -- Furious Attack
    (4342, 0, NULL, 40),  -- Death's Render
    (4343, 0, NULL, 40),  -- Wing Buffet
    (4344, 0, NULL, 1),  -- King Fight - Avatar Glow Stage 1
    (4345, 0, NULL, 1),  -- King Fight - Avatar Glow Stage 2
    (4346, 0, NULL, 1),  -- King Fight - Avatar Glow Clear
    (4347, 0, NULL, 1),  -- Knockback
    (4350, 0, NULL, 150),  -- ability
    (4351, 0, NULL, 94),  -- ability
    (4355, 0, NULL, 44),  -- ability
    (4356, 0, NULL, 40),  -- ability
    (4357, 0, NULL, 40),  -- ability
    (4360, 0, NULL, 150),  -- ability
    (4361, 0, NULL, 150),  -- ability
    (4362, 0, NULL, 1),  -- Sprite Fire
    (4363, 0, NULL, 1),  -- Sprite Death Explosion
    (4365, 0, NULL, 40),  -- Elemental Strike
    (4366, 0, NULL, 40),  -- Fire Breath
    (4367, 0, NULL, 40),  -- Exploding Flames
    (4369, 0, NULL, 40),  -- Heavy Strike
    (4370, 0, NULL, 40),  -- Heal
    (4371, 0, NULL, 40),  -- Mine Explosion
    (4375, 0, NULL, 1),  -- Whitefire Essence
    (4376, 0, NULL, 1),  -- Spider Queen Add 1 Counter for damage buff
    (4377, 0, NULL, 1),  -- Essence Gather (Spider Queen Minion Enter)
    (4378, 0, NULL, 1),  -- Essence Bolt (Spider Queen Minion Enter)
    (4381, 0, NULL, 175),  -- Whitefire Fang
    (4382, 0, NULL, 100),  -- Pleasurevenom
    (4383, 0, 0, 100),  -- Painvenom
    (4384, 0, NULL, 100),  -- Whitefire Webbing
    (4389, 0, NULL, 1),  -- Derisolde the Undead Knockdown
    (4390, 0, NULL, 1),  -- Captain Syrkin's Corpse
    (4406, 0, NULL, 100),  -- Pain-Fury Whirlwind
    (4408, 0, NULL, 175),  -- Razor Claws
    (4410, 0, NULL, 44),  -- Spider Lay Egg
    (4411, 0, NULL, 1),  -- Brew Goo Boil
    (4415, 0, NULL, 100),  -- Cleave
    (4417, 0, NULL, 100),  -- Guttural Shout
    (4420, 0, NULL, 100),  -- Disruptive Blow
    (4421, 0, NULL, 100),  -- Writhing Swipe
    (4422, 0, NULL, 100),  -- Writhing Blow
    (4424, 0, NULL, 250),  -- Writhing Heal
    (4425, 0, NULL, 1),  -- Writhing Energy
    (4426, 0, NULL, 100),  -- Writhing Pain
    (4428, 0, 0, 100),  -- Writhing Pleasure
    (4434, 0, NULL, 100),  -- Savage Cut
    (4435, 0, 0, 100),  -- Murderous Swing
    (4436, 0, NULL, 100),  -- Ruthless Hit
    (4437, 0, NULL, 1),  -- Writhing Bolt
    (4438, 0, NULL, 100),  -- Unrestrained Blow
    (4439, 0, NULL, 100),  -- Ripping Tusk
    (4440, 0, NULL, 100),  -- Tearing Claw
    (4443, 0, NULL, 100),  -- Mindfall Venom
    (4453, 0, NULL, 100),  -- Hoof Stomp
    (4454, 0, NULL, 100),  -- Worm Erupt
    (4455, 0, NULL, 1),  -- Menhir Shard
    (4456, 0, NULL, 1),  -- Everqueen's Blessing
    (4457, 0, 0, 40),  -- Tail Slam
    (4458, 0, 0, 40),  -- Horgulul Bite
    (4459, 0, 0, 40),  -- Dralel Bite
    (4460, 0, NULL, 1),  -- Sticky Spit
    (4555, 0, NULL, 40),  -- Fear
    (4556, 0, NULL, 40),  -- Fear
    (4600, 0, NULL, 40),  -- Heavy Strike
    (4601, 0, NULL, 40),  -- Claw Strike
    (4602, 0, NULL, 40),  -- Burst of Flame
    (4603, 0, NULL, 40),  -- Corporeal Blast
    (4604, 0, NULL, 40),  -- Spirit Blast
    (4605, 0, NULL, 40),  -- Bleed
    (4606, 0, 0, 40),  -- Bloody Claw
    (4607, 0, NULL, 40),  -- On Fire
    (4608, 0, 0, 40),  -- Life Loss
    (4609, 0, 0, 40),  -- Spirit Drain
    (4610, 0, NULL, 40),  -- Whirling Dervish
    (4611, 0, NULL, 40),  -- Combustion
    (4612, 0, NULL, 40),  -- Blast Wave
    (4613, 0, NULL, 40),  -- Ethereal Wave
    (4614, 0, NULL, 40),  -- Sharp Projectile
    (4615, 0, NULL, 40),  -- Piercing Projectile
    (4616, 0, NULL, 40),  -- Volley
    (4617, 0, NULL, 40),  -- Elemental Strike
    (4618, 0, NULL, 40),  -- Corporeal Strike
    (4619, 0, NULL, 40),  -- Ethereal Strike
    (4620, 0, NULL, 40),  -- Elemental Rain
    (4621, 0, NULL, 40),  -- Corporeal Rain
    (4622, 0, NULL, 40),  -- Ethereal Rain
    (4623, 0, NULL, 40),  -- Shard Volley
    (4624, 0, 0, 40),  -- Elemental Circle
    (4625, 0, 0, 40),  -- Circle of Life Loss
    (4626, 0, 0, 40),  -- Drain Circle
    (4627, 0, NULL, 40),  -- Elemental Line
    (4628, 0, NULL, 40),  -- Corporeal Line
    (4629, 0, NULL, 40),  -- Ethereal Line
    (4630, 0, NULL, 40),  -- Elemental Breath
    (4631, 0, NULL, 40),  -- Corporeal Breath
    (4632, 0, NULL, 40),  -- Ethereal Breath
    (4633, 0, NULL, 40),  -- Stun
    (4634, 0, NULL, 40),  -- Stun
    (4635, 0, NULL, 40),  -- Concussion Projectile
    (4636, 0, NULL, 40),  -- Elemental Stun
    (4637, 0, NULL, 40),  -- Corporeal Stun
    (4638, 0, NULL, 40),  -- Ethereal Stun
    (4639, 0, NULL, 40),  -- Snare
    (4640, 0, NULL, 40),  -- Snare
    (4641, 0, NULL, 40),  -- Sticky Projectile
    (4642, 0, NULL, 40),  -- Elemental Snare
    (4643, 0, NULL, 40),  -- Corporeal Snare
    (4644, 0, NULL, 40),  -- Ethereal Snare
    (4645, 0, NULL, 40),  -- Root
    (4646, 0, NULL, 40),  -- Root
    (4647, 0, NULL, 40),  -- Web Projectile
    (4648, 0, NULL, 40),  -- Elemental Root
    (4649, 0, NULL, 40),  -- Corporeal Root
    (4650, 0, NULL, 40),  -- Ethereal Root
    (4651, 0, NULL, 40),  -- Knockback
    (4652, 0, NULL, 40),  -- Knockback
    (4653, 0, NULL, 40),  -- Elemental Thrust
    (4654, 0, NULL, 40),  -- Corporeal Thrust
    (4655, 0, NULL, 40),  -- Ethereal Thrust
    (4656, 0, NULL, 40),  -- Elemental Push
    (4657, 0, NULL, 40),  -- Corporeal Push
    (4658, 0, NULL, 40),  -- Ethereal Push
    (4659, 0, NULL, 40),  -- Whirling Knockback
    (4660, 0, NULL, 40),  -- Knockback Wave
    (4661, 0, NULL, 40),  -- Elemental Wave Push
    (4662, 0, NULL, 40),  -- Corporeal Wave Push
    (4663, 0, NULL, 40),  -- Ethereal Wave Push
    (4664, 0, NULL, 40),  -- Disabling Swing
    (4665, 0, NULL, 40),  -- Elemental Stop
    (4666, 0, NULL, 40),  -- Corporeal Stop
    (4667, 0, NULL, 40),  -- Ethereal Stop
    (4668, 0, NULL, 40),  -- Knockdown
    (4669, 0, NULL, 40),  -- Knockdown
    (4670, 0, NULL, 40),  -- Knockdown Shot
    (4671, 0, NULL, 40),  -- Elemental Knockdown
    (4672, 0, NULL, 40),  -- Corporeal Knockdown
    (4673, 0, NULL, 40),  -- Ethereal Knockdown
    (4674, 0, NULL, 40),  -- Disabling Whirl
    (4675, 0, NULL, 40),  -- Elemental Stop Blast
    (4676, 0, NULL, 40),  -- Corporeal Stop Blast
    (4677, 0, NULL, 40),  -- Ethereal Stop Blast
    (4678, 0, NULL, 40),  -- Disabling Wave
    (4679, 0, NULL, 40),  -- Elemental Stop Wave
    (4680, 0, NULL, 40),  -- Corporeal Stop Wave
    (4681, 0, NULL, 40),  -- Ethereal Stop Wave
    (4682, 0, NULL, 40),  -- Blunt Volley
    (4683, 0, NULL, 40),  -- Elemental Mass Thrust
    (4684, 0, NULL, 40),  -- Corporeal Mass Thrust
    (4685, 0, NULL, 40),  -- Ethereal Mass Thrust
    (4686, 0, NULL, 40),  -- Whirling Knockdown
    (4687, 0, NULL, 40),  -- Silence Swing
    (4688, 0, NULL, 40),  -- Elemental Silence
    (4689, 0, NULL, 40),  -- Corporeal Silence
    (4690, 0, NULL, 40),  -- Ethereal Silence
    (4691, 0, NULL, 40),  -- Silence Wave
    (4692, 0, NULL, 40),  -- Elemental Mass Silence
    (4693, 0, NULL, 40),  -- Corporeal Mass Silence
    (4694, 0, NULL, 40),  -- Ethereal Mass Silence
    (4695, 0, NULL, 40),  -- Silence Blast
    (4696, 0, NULL, 40),  -- Elemental Silence Blast
    (4697, 0, NULL, 40),  -- Corporeal Silence Blast
    (4698, 0, NULL, 40),  -- Ethereal Silence Blast
    (4699, 0, NULL, 40),  -- Disarm
    (4700, 0, NULL, 40),  -- Elemental Disarm
    (4701, 0, NULL, 40),  -- Corporeal Disarm
    (4702, 0, NULL, 40),  -- Ethereal Disarm
    (4703, 0, NULL, 40),  -- Disarm Wave
    (4704, 0, NULL, 40),  -- Elemental Mass Disarm
    (4705, 0, NULL, 40),  -- Corporeal Mass Disarm
    (4706, 0, NULL, 40),  -- Ethereal Mass Disarm
    (4707, 0, NULL, 40),  -- Disarm Blast
    (4708, 0, NULL, 40),  -- Ethereal Disarm Blast
    (4709, 0, NULL, 40),  -- Corporeal Disarm Blast
    (4710, 0, NULL, 40),  -- Ethereal Disarm Blast
    (4711, 0, NULL, 40),  -- Heal
    (4712, 0, NULL, 40),  -- Regen
    (4713, 0, NULL, 40),  -- Healing Blast
    (4714, 0, NULL, 40),  -- Healing Circle
    (4715, 0, NULL, 40),  -- Sensitive
    (4716, 0, NULL, 40),  -- Inept
    (4717, 0, NULL, 40),  -- Shaky Resolve
    (4718, 0, NULL, 40),  -- Pushover
    (4719, 0, NULL, 40),  -- Focused Rage
    (4720, 0, NULL, 40),  -- Iron Will
    (4721, 0, NULL, 40),  -- Resistance
    (4722, 0, NULL, 40),  -- Fear
    (4723, 0, NULL, 40),  -- Scream
    (4724, 0, NULL, 40),  -- Fury
    (4725, 0, NULL, 40),  -- Thick Skin
    (4726, 0, NULL, 40),  -- Resilience
    (4731, 0, NULL, 40),  -- Liquid Inspiration
    (4734, 0, NULL, 40),  -- ability
    (4735, 0, NULL, 40),  -- Windstep's Curse
    (4800, 0, NULL, 40),  -- Grasp of the Dead
    (4801, 0, NULL, 40),  -- Gut Ripper
    (4802, 0, NULL, 40),  -- Ravens Bite
    (4803, 0, NULL, 40),  -- Storm of Ravens
    (4962, 0, NULL, 100),  -- ability
    (4963, 0, NULL, 44),  -- Phoenix Blade toss
    (4964, 0, NULL, 44),  -- Asuryans Will
    (4965, 0, NULL, 100),  -- Rage
    (4970, 0, NULL, 100),  -- Throw
    (4974, 0, NULL, 40),  -- Stun
    (4978, 0, NULL, 40),  -- Kick in the Gibblies
    (4981, 0, NULL, 100),  -- Black Fireball
    (4982, 0, NULL, 100),  -- Black Fireball (AoE)
    (4983, 0, NULL, 100),  -- Sun Dragon Breath
    (4984, 0, NULL, 100),  -- Sun Fireball
    (4985, 0, NULL, 100),  -- Sun Fireball (AoE)
    (4987, 0, NULL, 1),  -- Explosion
    (4988, 0, NULL, 100),  -- Black Horror
    (4989, 0, NULL, 100),  -- Chillwind
    (4990, 0, NULL, 100),  -- Forked Lightning
    (4991, 0, NULL, 100),  -- Radiant Strike
    (4997, 0, NULL, 40),  -- Stun
    (5007, 0, 0, 180),  -- Shart
    (5009, 0, NULL, 240),  -- Corrosive Vomit
    (5011, 0, NULL, 240),  -- Slimy Vomit
    (5012, 0, 0, 160),  -- Squig Heal
    (5013, 0, NULL, 80),  -- Boneskin
    (5014, 0, NULL, 240),  -- Squig Breath
    (5015, 0, NULL, 180),  -- Squig Bounce
    (5016, 0, NULL, 60),  -- Mangle
    (5017, 0, NULL, 100),  -- Eye Gouge
    (5018, 0, NULL, 120),  -- Noogie
    (5024, 0, 0, 240),  -- Bottle of Seein' Stars
    (5026, 0, NULL, 240),  -- Flask of Grabity Fings
    (5083, 0, NULL, 80),  -- Blood Fist
    (5100, 0, NULL, 1),  -- Nature's Grasp
    (5101, 0, NULL, 1),  -- Nature's Wrath
    (5102, 0, NULL, 1),  -- Power of the Worldbearer
    (5103, 0, NULL, 1),  -- Power of the Worldbearer
    (5104, 0, NULL, 40),  -- Cleave
    (5105, 0, NULL, 40),  -- Lash
    (5106, 0, NULL, 1),  -- Lightning Bolt
    (5108, 0, NULL, 1),  -- Blast of the Corrupted
    (5109, 0, NULL, 1),  -- Enrage State 1
    (5110, 0, NULL, 1),  -- Enrage State 2
    (5111, 0, NULL, 1),  -- Enrage State 3
    (5112, 0, NULL, 1),  -- Enrage State 4
    (5113, 0, NULL, 1),  -- Enrage State 5
    (5114, 0, NULL, 1),  -- Enrage State 6
    (5115, 0, NULL, 1),  -- Enrage State 7
    (5116, 0, NULL, 1),  -- Enrage State 8
    (5119, 0, NULL, 100),  -- Knockback
    (5120, 0, NULL, 100),  -- Cleave
    (5121, 0, NULL, 100),  -- Cleave
    (5122, 0, NULL, 94),  -- Gut Bounce
    (5123, 0, NULL, 1),  -- Knockback
    (5124, 0, NULL, 1),  -- Enrage
    (5125, 0, NULL, 100),  -- Earthkeepers Howl
    (5126, 0, NULL, 200),  -- Nature's Blast
    (5127, 0, NULL, 100),  -- Life Loss
    (5128, 0, NULL, 100),  -- Nature's Frailty
    (5129, 0, 0, 100),  -- Earthen Spew
    (5130, 0, NULL, 200),  -- Painful Dart
    (5131, 0, NULL, 200),  -- Horrible Pain
    (5132, 0, 0, 100),  -- Crippling Blow
    (5135, 0, NULL, 100),  -- Forceful Blast
    (5136, 0, NULL, 100),  -- Putrid Breath
    (5137, 25, 25, 100),  -- Crippling Thorns
    (5138, 0, NULL, 100),  -- Suffocation
    (5139, 0, NULL, 1),  -- Corrupting Aura
    (5140, 0, NULL, 40),  -- Toxic Blast
    (5141, 0, NULL, 200),  -- Viletongues Spit
    (5142, 0, 0, 100),  -- Paralyzing Bite
    (5145, 0, NULL, 40),  -- Ethereal Scream
    (5146, 0, NULL, 1),  -- Explosion
    (5148, 0, NULL, 200),  -- Twisted Wrath
    (5149, 0, NULL, 100),  -- Psyche Onslaught
    (5150, 0, NULL, 200),  -- Pain of the Vale
    (5151, 0, NULL, 200),  -- Pain of the Vale
    (5152, 0, NULL, 200),  -- Corrupted Loam
    (5153, 0, NULL, 200),  -- Pain Spike
    (5154, 0, NULL, 200),  -- Pain Spike
    (5155, 0, NULL, 200),  -- Spit of Corruption
    (5156, 0, 0, 200),  -- Branch Rake
    (5157, 0, NULL, 200),  -- Branch Rake
    (5158, 0, NULL, 200),  -- Vicious Claw
    (5159, 0, NULL, 200),  -- Vicious Claw
    (5160, 0, NULL, 200),  -- Lumbering Blow
    (5161, 0, NULL, 200),  -- Lumbering Blow
    (5162, 0, NULL, 200),  -- Cobber
    (5163, 0, NULL, 200),  -- Clobber
    (5164, 0, NULL, 200),  -- Brutish Hack
    (5165, 0, NULL, 200),  -- Brutish Hack
    (5166, 0, NULL, 200),  -- Wild Swing
    (5167, 0, NULL, 200),  -- Lucky Jab
    (5168, 0, NULL, 200),  -- Gore
    (5169, 0, NULL, 200),  -- Lacerate
    (5170, 0, NULL, 200),  -- Gash
    (5171, 0, NULL, 200),  -- Massive Strike
    (5172, 0, NULL, 200),  -- Massive Strike
    (5173, 0, NULL, 200),  -- Ravenous Blow
    (5174, 0, NULL, 200),  -- Flurry of Talons
    (5175, 0, NULL, 200),  -- Bear Heavy Strike
    (5176, 0, NULL, 200),  -- Wolf Heavy Strike
    (5177, 0, NULL, 200),  -- Hawk Heavy Strike
    (5178, 0, NULL, 200),  -- Bite
    (5179, 0, NULL, 200),  -- Pummel
    (5180, 0, NULL, 100),  -- Bleed
    (5181, 0, NULL, 200),  -- Cleave
    (5182, 0, NULL, 100),  -- R'khar's Rage
    (5183, 0, NULL, 100),  -- Putrid Breath
    (5184, 0, NULL, 100),  -- Porus' Blessing
    (5185, 0, NULL, 100),  -- Crippling Stomp
    (5186, 0, NULL, 100),  -- Chilling Breath
    (5188, 0, NULL, 100),  -- Bleed
    (5189, 0, NULL, 200),  -- Claw Strike
    (5190, 0, NULL, 1),  -- Eldazar Split
    (5191, 0, NULL, 40),  -- Crippling Fear
    (5192, 0, NULL, 200),  -- Cleave
    (5193, 0, NULL, 100),  -- Pummel
    (5194, 0, NULL, 100),  -- Knockback
    (5195, 0, NULL, 100),  -- Trample
    (5196, 0, NULL, 100),  -- Ragefire
    (5199, 0, NULL, 40),  -- Corrupting Poison
    (5200, 0, NULL, 40),  -- Mark of the Grove
    (5209, 0, NULL, 40),  -- Rattling Bones
    (5210, 0, NULL, 40),  -- Grenade Blast
    (5219, 0, NULL, 160),  -- Squig Heal
    (5220, 0, NULL, 40),  -- Hurlesson's Explosive Rune
    (5223, 0, NULL, 40),  -- Grenade Blast
    (5224, 0, NULL, 40),  -- Plauge of Boils
    (5229, 0, NULL, 40),  -- eadbutt
    (5230, 0, 0, 40),  -- Maw Belch
    (5231, 0, 0, 40),  -- lectric Blast
    (5233, 0, 0, 100),  -- Sweeping Strike
    (5234, 0, NULL, 100),  -- Reapa Rush
    (5235, 0, 0, 100),  -- Wicked Bite
    (5236, 0, 0, 100),  -- Moonflare
    (5237, 0, 0, 100),  -- Fanatical Frenzy
    (5238, 0, 0, 100),  -- Whirl an' Twirl
    (5244, 0, 0, 100),  -- Brain Bursta
    (5257, 0, 0, 40),  -- Foul Wound
    (5968, 0, 0, 1),  -- Terror
    (12001, 0, 0, 100),  -- Ethereal Rake
    (12003, 0, 0, 25),  -- Ephemeral Haste
    (12004, 0, 0, 125),  -- Soul Tap
    (12005, 0, 0, 125),  -- Banshee Wail
    (12006, 0, 0, 125),  -- Gasp of the Unliving
    (12008, 0, 0, 100),  -- Tormenting Wail
    (12011, 0, 0, 100),  -- Swooping Bite
    (12012, 0, 0, 100),  -- Bat Screech
    (12013, 0, 0, 25),  -- Blind Strength
    (12014, 0, 0, 50),  -- Night Rigor
    (12015, 0, 0, 50),  -- Hinder Mobility
    (12016, 0, 0, 125),  -- Swirling Confusion
    (12017, 0, 0, 125),  -- Bat Screech
    (12021, 0, 0, 100),  -- Maul
    (12022, 0, 0, 100),  -- Ursine Roar
    (12023, 0, 0, 125),  -- Puncture Limb
    (12024, 0, 0, 75),  -- Sever Limb
    (12025, 0, 0, 125),  -- Ursine Roar
    (12026, 0, 0, 125),  -- Furious Roar
    (12031, 0, 0, 100),  -- Sanguine Howl
    (12032, 0, 0, 125),  -- Bleed
    (12033, 0, 0, 125),  -- Sanquine Howl
    (12034, 0, 0, 25),  -- Blood Fury
    (12035, 0, 0, 50),  -- Powerful Force
    (12036, 0, 0, 125),  -- Deaden Limbs
    (12038, 0, 0, 125),  -- Breath of Fear
    (12041, 0, 0, 100),  -- Daemonic Pierce
    (12042, 0, 0, 125),  -- Blood Strike
    (12043, 0, 0, 25),  -- Daemon Fury
    (12044, 0, 0, 125),  -- Bleeding Brain
    (12045, 0, 0, 125),  -- Bloody Roar
    (12046, 0, 0, 125),  -- Slamming Blow
    (12047, 0, 0, 25),  -- Blessing of Blood
    (12051, 0, 0, 100),  -- Gore
    (12052, 0, 0, 100),  -- Boar's Rage
    (12054, 0, 0, 125),  -- Uncontrolled Trample
    (12055, 0, 0, 125),  -- Disrupting Bash
    (12056, 0, 0, 50),  -- Dead Legs
    (12061, 0, 0, 125),  -- Raking Claws
    (12062, 0, 0, 125),  -- Chomp
    (12063, 0, 0, 25),  -- Lion's Pace
    (12064, 0, 0, 50),  -- Weaken Resolve
    (12065, 0, 0, 125),  -- Debilitating Wound
    (12066, 0, 0, 125),  -- Pounce
    (12067, 0, 0, 125),  -- Hallow Wound
    (12068, 0, 0, 25),  -- Subtle Evasion
    (12071, 0, 0, 100),  -- Savage Blow
    (12072, 0, 0, 100),  -- Stomping Hooves
    (12073, 0, 0, 25),  -- Power of the Pack
    (12074, 0, 0, 125),  -- Weak Limbs
    (12076, 0, 0, 125),  -- Roaring Adrenaline
    (12077, 0, 0, 125),  -- Stomping Hooves
    (12082, 0, 0, 100),  -- Lunging Maw
    (12083, 0, 0, 50),  -- Sapped Strength
    (12084, 0, 0, 125),  -- Speed of the Pack
    (12085, 0, 0, 125),  -- Mangle Arms
    (12087, 0, 0, 125),  -- Howling Nightmares
    (12091, 0, 0, 100),  -- Savage Jab
    (12092, 0, 0, 125),  -- Tail Spike
    (12093, 0, 0, 125),  -- Breath of Flame
    (12094, 0, 0, 50),  -- Fearful Gust
    (12095, 0, 0, 125),  -- Ground Tremble
    (12097, 0, 0, 125),  -- Immobile Shock
    (12100, 0, 0, 100),  -- Beak Jab
    (12101, 0, 0, 100),  -- Ripping Beak
    (12102, 0, 0, 125),  -- Stinging Breath
    (12103, 0, 0, 25),  -- Hardened Hide
    (12104, 0, 0, 125),  -- Forceful Strike
    (12106, 0, 0, 125),  -- Forceful Lash
    (12111, 0, 0, 100),  -- Ripping Gnash
    (12112, 0, 0, 125),  -- Snapping Jaw
    (12113, 0, 0, 125),  -- Devastating Roar
    (12114, 0, 0, 125),  -- Head Charge
    (12116, 0, 0, 50),  -- Weaken Resolve
    (12117, 0, 0, 125),  -- Headslam
    (12121, 0, 0, 100),  -- Jagged Claw
    (12122, 0, 0, 100),  -- Impaling Claws
    (12123, 0, 0, 125),  -- Life Transfer
    (12124, 0, 0, 125),  -- Daemonic Strength
    (12125, 0, 0, 125),  -- Zealous Shout
    (12131, 0, 0, 125),  -- Branch Scrape
    (12132, 0, 0, 125),  -- Open Wound
    (12133, 0, 0, 50),  -- Branch Regrowth
    (12134, 0, 0, 50),  -- Toughen Bark
    (12135, 0, 0, 125),  -- Constricting Boughs
    (12137, 0, 0, 125),  -- Distracting Pain
    (12138, 0, 0, 75),  -- Nature's Touch
    (12142, 0, 0, 100),  -- Blazing Combustion
    (12143, 0, 0, 100),  -- Fireball
    (12144, 0, 0, 75),  -- Fire Blast
    (12145, 0, 0, 50),  -- Daemonic Touch
    (12146, 0, 0, 125),  -- Blazing Combustion
    (12147, 0, 0, 125),  -- Abate Movement
    (12151, 0, 0, 100),  -- Rending Hook
    (12152, 0, 0, 125),  -- Unusual Strike
    (12153, 0, 0, 50),  -- Negate Aid
    (12154, 0, 0, 75),  -- Sever Nerve
    (12155, 0, 0, 125),  -- Chaotic Confusion
    (12156, 0, 0, 125),  -- Agitated Legs
    (12161, 0, 0, 100),  -- Savage Bite
    (12162, 0, 0, 100),  -- Bloodthirsty Howl
    (12163, 0, 0, 25),  -- Elemental Protection
    (12164, 0, 0, 125),  -- Crush Hands
    (12166, 0, 0, 125),  -- Bloodthirsty Howl
    (12167, 0, 0, 125),  -- Savage Lunge
    (12171, 0, 0, 100),  -- Daemonic Claw
    (12172, 0, 0, 125),  -- Fiendish Slash
    (12173, 0, 0, 25),  -- Daemon Fury
    (12174, 0, 0, 125),  -- Violent Winds
    (12175, 0, 0, 125),  -- Sweeping Claw
    (12176, 0, 0, 125),  -- Daemonic Fear
    (12177, 0, 0, 125),  -- Strong Winds
    (12181, 0, 0, 100),  -- Boney Fist
    (12182, 0, 0, 125),  -- Ghastly Wound
    (12183, 0, 0, 50),  -- Weaken Defenses
    (12184, 0, 0, 125),  -- Cannibalize
    (12185, 0, 0, 125),  -- Distracting Attack
    (12186, 0, 0, 125),  -- Clumsy Hands
    (12191, 0, 0, 100),  -- Drunken Blow
    (12192, 0, 0, 100),  -- Satisfying Belch
    (12193, 0, 0, 125),  -- Massive Sweep
    (12194, 0, 0, 125),  -- Ground Rumble
    (12195, 0, 0, 125),  -- Unsteady Ground
    (12196, 0, 0, 125),  -- Satisfying Belch
    (12201, 0, 0, 100),  -- Brute Force
    (12202, 0, 0, 100),  -- Bellowing Roar
    (12203, 0, 0, 75),  -- Bellowing Roar
    (12204, 0, 0, 50),  -- Stiff Limbs
    (12205, 0, 0, 125),  -- Toss Up
    (12207, 0, 0, 125),  -- Unsteady
    (12211, 0, 0, 100),  -- Talon Swipe
    (12212, 0, 0, 100),  -- Shredding Talons
    (12213, 0, 0, 25),  -- Mighty Strength
    (12214, 0, 0, 50),  -- Weaken Enemy
    (12215, 0, 0, 125),  -- Mind Attack
    (12216, 0, 0, 125),  -- Temper Movement
    (12221, 0, 0, 100),  -- Ripping Claws
    (12222, 0, 0, 100),  -- Harpy Screech
    (12223, 0, 0, 50),  -- Empty Mind
    (12224, 0, 0, 125),  -- Help Myself
    (12225, 0, 0, 50),  -- Surprising Force
    (12226, 0, 0, 125),  -- Harpy Screech
    (12231, 0, 0, 100),  -- Daemonic Smash
    (12232, 0, 0, 100),  -- Daemonic Fire
    (12233, 0, 0, 25),  -- Potent Favor
    (12234, 0, 0, 50),  -- Daemonic Hold
    (12235, 0, 0, 125),  -- Warping Energy
    (12236, 0, 0, 125),  -- Distracting Strike
    (12237, 0, 0, 125),  -- Unfortunate Circumstance
    (12238, 0, 0, 125),  -- Daemonic Slash
    (12241, 0, 0, 125),  -- Trample
    (12242, 0, 0, 125),  -- Strong Jaw
    (12243, 0, 0, 25),  -- Dense Muscle
    (12244, 0, 0, 25),  -- Ferocity
    (12245, 0, 0, 125),  -- Festering Bite
    (12246, 0, 0, 50),  -- Diseased Mind
    (12247, 0, 0, 125),  -- Open Sore
    (12248, 0, 0, 125),  -- Absorb Life
    (12251, 0, 0, 125),  -- Ferocious Bite
    (12252, 0, 0, 125),  -- Ruthless Jaws
    (12253, 0, 0, 50),  -- Hound's Cry
    (12254, 0, 0, 25),  -- Hound's Fury
    (12255, 0, 0, 125),  -- Clip Heels
    (12256, 0, 0, 125),  -- Distracting Nip
    (12257, 0, 0, 125),  -- Wild Bite
    (12261, 0, 0, 100),  -- Trampling Claws
    (12262, 0, 0, 125),  -- Huge Maw
    (12263, 0, 0, 125),  -- Roaring Breath
    (12264, 0, 0, 125),  -- Hydra Fire
    (12265, 0, 0, 125),  -- Encircling Pain
    (12266, 0, 0, 125),  -- Massive Swat
    (12267, 0, 0, 50),  -- Forceful Breath
    (12268, 0, 0, 125),  -- Fragile Ground
    (12271, 0, 0, 100),  -- Devour Soul
    (12272, 0, 0, 100),  -- Lingering Blast
    (12273, 0, 0, 100),  -- Bleed Life
    (12274, 0, 0, 50),  -- Rotting Mind
    (12275, 0, 0, 75),  -- Reclaim Life
    (12276, 0, 0, 75),  -- Whispered Silence
    (12277, 0, 0, 125),  -- Distracting Blast
    (12278, 0, 0, 75),  -- Grasping Bones
    (12279, 0, 0, 75),  -- Silence of the Dead
    (12282, 0, 0, 100),  -- Tail Swipe
    (12283, 0, 0, 125),  -- Head Clamp
    (12284, 0, 0, 50),  -- Weak Noise
    (12285, 0, 0, 50),  -- Tail Swipe
    (12286, 0, 0, 125),  -- Searing Roar
    (12291, 0, 0, 100),  -- Nibbling Bite
    (12292, 0, 0, 125),  -- Putrid Infection
    (12293, 0, 0, 25),  -- Strengthen Core
    (12294, 0, 0, 125),  -- Clamping Pain
    (12295, 0, 0, 125),  -- Brain Wriggle
    (12301, 0, 0, 100),  -- Lacerating Talons
    (12302, 0, 0, 100),  -- Booming Shriek
    (12303, 0, 0, 50),  -- Booming Shriek
    (12304, 0, 0, 25),  -- Rearing Fury
    (12305, 0, 0, 125),  -- Disrupting Claw
    (12306, 0, 0, 125),  -- Fearful Strike
    (12311, 0, 0, 100),  -- Trample
    (12312, 0, 0, 100),  -- Booming Roar
    (12313, 0, 0, 125),  -- Tramp
    (12314, 0, 0, 125),  -- Booming Roar
    (12315, 0, 0, 125),  -- Blasting Force
    (12317, 0, 0, 125),  -- Sweeping Claws
    (12321, 0, 0, 100),  -- Leprous Blow
    (12322, 0, 0, 125),  -- Annoying Pain
    (12323, 0, 0, 50),  -- Acid Tears
    (12324, 0, 0, 125),  -- Body Smash
    (12326, 0, 0, 125),  -- Grim Affliction
    (12332, 0, 0, 125),  -- Spider Bite
    (12333, 0, 0, 125),  -- Envenomed Fang
    (12334, 0, 0, 125),  -- Seeping Poison
    (12335, 0, 0, 50),  -- Infect Morale
    (12336, 0, 0, 75),  -- Enmesh Face
    (12337, 0, 0, 125),  -- Toxic Web
    (12338, 0, 0, 125),  -- Poisonous Web
    (12342, 0, 0, 100),  -- Diseased Bellow
    (12343, 0, 0, 125),  -- Infecting Swipe
    (12344, 0, 0, 50),  -- Diseased Bellow
    (12345, 0, 0, 125),  -- Rumbling Ground
    (12346, 0, 0, 125),  -- Cerebral Corrosion
    (12347, 0, 0, 125),  -- Brain Rot
    (12351, 0, 0, 100),  -- Pus Eruption
    (12352, 0, 0, 125),  -- Infected Gash
    (12353, 0, 0, 125),  -- Pus Eruption
    (12354, 0, 0, 25),  -- Diseased Anger
    (12355, 0, 0, 125),  -- Breath of Fear
    (12356, 0, 0, 125),  -- Disheartened Bash
    (12357, 0, 0, 125),  -- Putrify
    (12361, 0, 0, 100),  -- Leaping Bite
    (12362, 0, 0, 125),  -- Nibble
    (12363, 0, 0, 125),  -- Putrid Bite
    (12364, 0, 0, 125),  -- Diseased Wound
    (12365, 0, 0, 50),  -- Drain Defenses
    (12371, 0, 0, 100),  -- Crushing Blow
    (12372, 0, 0, 100),  -- Deafening Roar
    (12373, 0, 0, 125),  -- Damage Morale
    (12374, 0, 0, 75),  -- Blow to the Wrists
    (12375, 0, 0, 125),  -- Thunderous Roar
    (12376, 0, 0, 125),  -- Directed Smash
    (12377, 0, 0, 125),  -- Debilitating Sweep
    (12382, 0, 0, 125),  -- Lurch
    (12383, 0, 0, 125),  -- Coat of Slime
    (12384, 0, 0, 50),  -- Slick Hands
    (12385, 0, 0, 125),  -- Amorphous Punt
    (12386, 0, 0, 50),  -- Amorphous Punt
    (12387, 0, 0, 125),  -- Viscous Contamination
    (12388, 0, 0, 125),  -- Body Slam
    (12391, 0, 0, 100),  -- Goring Swipe
    (12392, 0, 0, 100),  -- Goring Lunge
    (12393, 0, 0, 25),  -- Roaring Power
    (12394, 0, 0, 50),  -- Strip Defenses
    (12395, 0, 0, 125),  -- Charging Punt
    (12397, 0, 0, 125),  -- Goring Lunge
    (12401, 0, 0, 100),  -- Crushing Pincer
    (12402, 0, 0, 100),  -- Envenomed Stinger
    (12403, 0, 0, 50),  -- Lingering Poison
    (12404, 0, 0, 125),  -- Poisoned Shout
    (12405, 0, 0, 125),  -- Flaring Pain
    (12406, 0, 0, 125),  -- Numbing Poison
    (12411, 0, 0, 100),  -- Screaming Lash
    (12412, 0, 0, 125),  -- Piercing Strike
    (12413, 0, 0, 125),  -- Screaming Wave
    (12414, 0, 0, 50),  -- Unwilling
    (12415, 0, 0, 125),  -- Whipping Lash
    (12417, 0, 0, 125),  -- Wild Lashing
    (12421, 0, 0, 100),  -- Phlegm Spit
    (12422, 0, 0, 125),  -- Little Swat
    (12423, 0, 0, 25),  -- Brain Power
    (12424, 0, 0, 50),  -- Super Smack
    (12425, 0, 0, 125),  -- Pounding Pain
    (12431, 0, 0, 125),  -- Head Pummel
    (12432, 0, 0, 125),  -- Pleasure Wound
    (12433, 0, 0, 25),  -- Blissful Protection
    (12434, 0, 0, 25),  -- Delectation in Pain
    (12435, 0, 0, 125),  -- Pleasure Feed
    (12436, 0, 0, 125),  -- Rough Smash
    (12437, 0, 0, 125),  -- Overwhelming Pain
    (12441, 0, 0, 100),  -- Spider Bite
    (12442, 0, 0, 100),  -- Envenomed Fangs
    (12443, 0, 0, 25),  -- Hurt Mind
    (12444, 0, 0, 50),  -- Web Blast
    (12445, 0, 0, 125),  -- Webbed Down
    (12446, 0, 0, 125),  -- Screeching Noise
    (12451, 0, 0, 100),  -- Ethereal Hand
    (12453, 0, 0, 125),  -- Ethereal Chill
    (12454, 0, 0, 50),  -- Strain Mind
    (12455, 0, 0, 125),  -- Pained Hush
    (12457, 0, 0, 125),  -- Disrupting Touch
    (12461, 0, 0, 125),  -- Thorny Paw
    (12462, 0, 0, 125),  -- Bark Blow
    (12463, 0, 0, 25),  -- Toughen Bark
    (12464, 0, 0, 50),  -- Dilute Strength
    (12465, 0, 0, 125),  -- Enliven Sap
    (12466, 0, 0, 125),  -- Scattered Weapons
    (12471, 0, 0, 125),  -- Fluttering Pain
    (12472, 0, 0, 125),  -- Winged Pain
    (12473, 0, 0, 25),  -- Minute Strength
    (12474, 0, 0, 125),  -- Disrupting Call
    (12475, 0, 0, 125),  -- Whispering Calm
    (12481, 0, 0, 100),  -- Chomp
    (12482, 0, 0, 125),  -- Leaping Attack
    (12483, 0, 0, 125),  -- Rebound
    (12484, 0, 0, 25),  -- Strengthen Skin
    (12485, 0, 0, 125),  -- Cornered Shout
    (12486, 0, 0, 125),  -- Squig Squeal
    (12487, 0, 0, 100),  -- Death From Above
    (12491, 0, 0, 125),  -- Furious Buck
    (12492, 0, 0, 125),  -- Antler Spear
    (12493, 0, 0, 125),  -- Snap Limb
    (12495, 0, 0, 50),  -- Impair Movement
    (12496, 0, 0, 50),  -- Charge
    (12501, 0, 0, 100),  -- Resounding Wallop
    (12502, 0, 0, 100),  -- Acidic Spit
    (12511, 0, 0, 100),  -- Crushing Bark
    (12512, 0, 0, 100),  -- Sprout Roots
    (12513, 0, 0, 25),  -- Harden Bark
    (12514, 0, 0, 125),  -- Recycle Health
    (12515, 0, 0, 50),  -- Sprout Roots
    (12516, 0, 0, 125),  -- Power of Nature
    (12521, 0, 0, 100),  -- Overhand Slam
    (12522, 0, 0, 100),  -- Foul Vomit
    (12523, 0, 0, 125),  -- Foul Vomit
    (12524, 0, 0, 125),  -- Strain Defenses
    (12525, 0, 0, 25),  -- Nature's Wrath
    (12526, 0, 0, 125),  -- Troll Regeneration
    (12531, 0, 0, 100),  -- Horn Smash
    (12532, 0, 0, 100),  -- Tuskgore Frenzy
    (12533, 0, 0, 25),  -- Tuskgore Frenzy
    (12534, 0, 0, 50),  -- Soften Hide
    (12535, 0, 0, 125),  -- Wound Legs
    (12536, 0, 0, 125),  -- Painful Roar
    (12541, 0, 0, 100),  -- Crushing Hooves
    (12542, 0, 0, 125),  -- Pristine Gnash
    (12543, 0, 0, 75),  -- Wounded Hands
    (12544, 0, 0, 125),  -- Break Morale
    (12545, 0, 0, 25),  -- Astute Blessing
    (12546, 0, 0, 125),  -- Forceful Pin
    (12547, 0, 0, 125),  -- Impair Mobility
    (12548, 0, 0, 125),  -- Salvaged Hate
    (12551, 0, 0, 100),  -- Assaulting Buffet
    (12552, 0, 0, 125),  -- Eye Peck
    (12553, 0, 0, 125),  -- Leg Wound
    (12554, 0, 0, 50),  -- Rough Winds
    (12562, 0, 0, 100),  -- Buffet
    (12563, 0, 0, 125),  -- Eye Gouge
    (12564, 0, 0, 25),  -- Speed of Flight
    (12565, 0, 0, 125),  -- Demoralizing Peck
    (12566, 0, 0, 125),  -- Wing Clip
    (12567, 0, 0, 125),  -- Wind Buffet
    (12571, 0, 0, 100),  -- Mangling Pincer
    (12572, 0, 0, 125),  -- Gnash
    (12573, 0, 0, 25),  -- Calculated Advantage
    (12574, 0, 0, 125),  -- Confusion
    (12576, 0, 0, 125),  -- Health Shift
    (12577, 0, 0, 125),  -- Life Displacement
    (12582, 0, 0, 125),  -- Savage Jaws
    (12583, 0, 0, 50),  -- Howling Confusion
    (12586, 0, 0, 125),  -- Distracting Bite
    (12587, 0, 0, 125),  -- Lunging Disarm
    (12588, 0, 0, 125),  -- Hallow Bite
    (12589, 0, 0, 25),  -- Protection of the Pack
    (12591, 0, 0, 100),  -- Scythe Gash
    (12592, 0, 0, 100),  -- Ethereal Emanation
    (12594, 0, 0, 50),  -- Fear
    (12595, 0, 0, 125),  -- Retrieve Life
    (12596, 0, 0, 125),  -- Shoul Shriek
    (12601, 0, 0, 100),  -- Chomping Maw
    (12602, 0, 0, 100),  -- Foul Breath
    (12603, 0, 0, 25),  -- Winged Fury
    (12604, 0, 0, 75),  -- Crushing Bite
    (12605, 0, 0, 125),  -- Head Punt
    (12607, 0, 0, 125),  -- Anchored Fear
    (12611, 0, 0, 100),  -- Lacerating Claws
    (12612, 0, 0, 100),  -- Earsplitting Howl
    (12613, 0, 0, 25),  -- Howling Fury
    (12614, 0, 0, 25),  -- Matted Fur
    (12615, 0, 0, 125),  -- Earsplitting Howl
    (12621, 0, 0, 125),  -- Gore
    (12622, 0, 0, 25),  -- Head Smash
    (12623, 0, 0, 75),  -- Fumbling Grip
    (12624, 0, 0, 50),  -- Deplete Brain Matter
    (12625, 0, 0, 125),  -- Headbutt
    (12627, 0, 0, 125),  -- Directed Bounce
    (12632, 0, 0, 125),  -- Squig Phlegm
    (12633, 0, 0, 125),  -- Squig Spit
    (12634, 0, 0, 50),  -- Poisoned Spine
    (12635, 0, 0, 125),  -- Splice Ankles
    (12636, 0, 0, 25),  -- Squig Power
    (12637, 0, 0, 25),  -- Tough Squig
    (12638, 0, 0, 125),  -- Coat Throats
    (12642, 0, 0, 100),  -- Rancid Blast
    (12643, 0, 0, 125),  -- Malodorous Pain
    (12644, 0, 0, 25),  -- Brains Over Brawn
    (12645, 0, 0, 50),  -- Spoiled Morale
    (12646, 0, 0, 50),  -- Gag
    (12647, 0, 0, 100),  -- Spore Cloud
    (12648, 0, 0, 75),  -- Deplete Armor
    (12651, 0, 0, 125),  -- Leveled Attack
    (12652, 0, 0, 125),  -- Leveled Wound
    (12653, 0, 0, 125),  -- Distract Mind
    (12654, 0, 0, 125),  -- Nagging Roar
    (12655, 0, 0, 125),  -- Unsteady Grip
    (12661, 0, 0, 125),  -- Unholy Strike
    (12662, 0, 0, 125),  -- Bloody Wound
    (12663, 0, 0, 50),  -- Vampiric Shroud
    (12665, 0, 0, 75),  -- Reap Voice
    (12666, 0, 0, 100),  -- Reap Soul
    (12667, 0, 0, 100),  -- Breathe Blood
    (12668, 0, 0, 75),  -- Feeding Darkness
    (12671, 0, 0, 125),  -- Unholy Blow
    (12672, 0, 0, 125),  -- Cursed Wound
    (12673, 0, 0, 25),  -- Unholy Strength
    (12674, 0, 0, 125),  -- Slow Movement
    (12675, 0, 0, 125),  -- Distracting Blow
    (12676, 0, 0, 125),  -- Soul Steal
    (12677, 0, 0, 125),  -- Drain Mind
    (12681, 0, 0, 125),  -- Bite
    (12682, 0, 0, 125),  -- Gushing Wound
    (12683, 0, 0, 125),  -- Claw Sweep
    (12684, 0, 0, 125),  -- Shred
    (12685, 0, 0, 125),  -- Terrifying Roar
    (12686, 0, 0, 125),  -- Lion's Roar
    (12691, 0, 0, 125),  -- Bestial Strike
    (12692, 0, 0, 125),  -- Deep Wound
    (12693, 0, 0, 25),  -- Might of the Herd
    (12694, 0, 0, 125),  -- Ground Crack
    (12695, 0, 0, 125),  -- Fearful Roar
    (12696, 0, 0, 125),  -- Distracting Blow
    (12697, 0, 0, 125),  -- Sweeping Jostle
    (12701, 0, 0, 125),  -- Beastly Strike
    (12702, 0, 0, 125),  -- Open Gash
    (12703, 0, 0, 25),  -- Might of the Herd
    (12704, 0, 0, 50),  -- Humiliation
    (12705, 0, 0, 125),  -- Useless Limbs
    (12707, 0, 0, 125),  -- Mighty Blow
    (12708, 0, 0, 125),  -- Fearful Roar
    (12712, 0, 0, 100),  -- Lightning Strike
    (12713, 0, 0, 125),  -- Bestial Affliction
    (12714, 0, 0, 25),  -- Bestial Fury
    (12715, 0, 0, 25),  -- Bestial Armor
    (12716, 0, 0, 125),  -- Earth Stomp
    (12717, 0, 0, 125),  -- Rattle Brain
    (12718, 0, 0, 125),  -- Throat Blast
    (12721, 0, 0, 125),  -- Wicked Strike
    (12722, 0, 0, 125),  -- Powerful Bash
    (12723, 0, 0, 25),  -- Tough Skin
    (12724, 0, 0, 75),  -- Drop That
    (12725, 0, 0, 125),  -- Earth Shake
    (12726, 0, 0, 125),  -- Earth Tremble
    (12731, 0, 0, 125),  -- Meaty Strike
    (12732, 0, 0, 125),  -- Painful Wound
    (12733, 0, 0, 25),  -- Monstrous Haste
    (12734, 0, 0, 50),  -- Insignificant Defenses
    (12735, 0, 0, 25),  -- Power Hungry
    (12736, 0, 0, 125),  -- Earth Smash
    (12738, 0, 0, 125),  -- Sweeping Cleave
    (12741, 0, 0, 125),  -- Strong Strike
    (12742, 0, 0, 125),  -- Savage Strike
    (12743, 0, 0, 25),  -- Power Hungry
    (12744, 0, 0, 125),  -- Debilitating Strike
    (12745, 0, 0, 50),  -- Strip Power
    (12746, 0, 0, 125),  -- Drop That
    (12748, 0, 0, 125),  -- Sweeping Toss
    (12751, 0, 0, 125),  -- Smash
    (12752, 0, 0, 125),  -- Pierced Wound
    (12753, 0, 0, 125),  -- Achilles Tear
    (12754, 0, 0, 50),  -- Fright
    (12755, 0, 0, 125),  -- Nudge
    (12757, 0, 0, 125),  -- Body Rush
    (12761, 0, 0, 125),  -- Draconic Strike
    (12762, 0, 0, 125),  -- Overhead Slam
    (12763, 0, 0, 25),  -- Dragon Fury
    (12764, 0, 0, 50),  -- Bleed Legs
    (12765, 0, 0, 125),  -- Crack Arms
    (12767, 0, 0, 125),  -- Charging Pain
    (12768, 0, 0, 125),  -- Trembling Ground
    (12771, 0, 0, 125),  -- Tainted Wound
    (12772, 0, 0, 50),  -- Chaotic Roar
    (12773, 0, 0, 125),  -- Ground Rumble
    (12774, 0, 0, 125),  -- Satisfying Belch
    (12775, 0, 0, 125),  -- Sweeping Punt
    (12781, 0, 0, 125),  -- Bone Crunch
    (12782, 0, 0, 125),  -- Sweeping Graze
    (12783, 0, 0, 25),  -- Healthy Bones
    (12784, 0, 0, 50),  -- Weaken Bones
    (12785, 0, 0, 125),  -- Crush Legs
    (12786, 0, 0, 125),  -- Arm Crush
    (12791, 0, 0, 125),  -- Creeping Death
    (12792, 0, 0, 50),  -- Diseased Brain
    (12793, 0, 0, 125),  -- Clumsy
    (12802, 0, 0, 100),  -- Scorching Flame
    (12803, 0, 0, 100),  -- Fireball
    (13002, 0, 0, 44),  -- Big Knock out
    (13003, 0, 0, 40),  -- Grasp of the Dead
    (13005, 0, 0, 20),  -- Creeping Rot
    (13006, 0, 0, 40),  -- Shkapsesafs Will
    (13007, 0, 0, 1),  -- Unsettling Aura
    (13008, 0, 0, 20),  -- Festering Rot
    (13011, 0, 0, 40),  -- Flames of Beyond
    (13012, 0, 0, 40),  -- Rotting Flesh
    (13013, 0, 0, 50),  -- Talon Rake
    (13015, 0, 0, 1),  -- Deathly Scream
    (13018, 0, 0, 100),  -- Seeping Wound
    (13064, 0, 0, 100),  -- Wretched Arrow
    (13071, 0, 0, 100),  -- Poison Arrer
    (13072, 0, 0, 100),  -- Stabbity
    (13073, 0, 0, 100),  -- Tremor
    (13074, 0, 0, 100),  -- Scalding Vomit
    (13075, 0, 0, 100),  -- Huge Fist
    (13076, 0, 0, 100),  -- Feral Bite
    (13077, 0, 0, 100),  -- 'Ead Stabbity
    (13078, 0, 0, 100),  -- Wide Swing
    (13079, 0, 0, 100),  -- Leave Me Alone
    (13080, 0, 0, 100),  -- Bolt of the Mourkain
    (13081, 0, 0, 100),  -- Boney Strike
    (13082, 0, 0, 100),  -- Piercing Projectile
    (13083, 0, 0, 100),  -- Brain Bursta
    (13084, 0, 0, 100),  -- Forceful Stab
    (13085, 0, 0, 100),  -- Mourkain Rift
    (13086, 0, 0, 100),  -- Frozen Touch
    (13087, 0, 0, 100),  -- Poison Arrer
    (13088, 0, 0, 100),  -- Stabbity
    (13089, 0, 0, 100),  -- Fling Spines
    (13090, 0, 0, 100),  -- Infecting Swipe
    (13091, 0, 0, 100),  -- Diseased Bellow
    (13092, 0, 0, 100),  -- Chomp
    (13093, 0, 0, 100),  -- Fling
    (13094, 0, 0, 100),  -- 'Ead Stabbity
    (13095, 0, 0, 100),  -- Swift Strike
    (13096, 0, 0, 100),  -- Vicious Bite
    (13097, 0, 0, 100),  -- Writhing Fangs
    (13100, 0, 0, 100),  -- Wide Swing
    (13101, 0, 0, 100),  -- Leave Me Alone
    (13102, 0, 0, 100),  -- Venom Cloud
    (13103, 0, 0, 100),  -- Touch of the Banshee
    (13105, 0, 0, 100),  -- Grotesque Belch
    (13108, 0, 0, 100),  -- Vile Vomit
    (13116, 0, 0, 40),  -- Gotrek's Push
    (13117, 0, 0, 40),  -- Low Blow
    (13118, 0, 0, 80),  -- Skilled Defenses
    (13125, 0, 0, 100),  -- Vicious Bite
    (13126, 0, 0, 100),  -- Writhing Fangs
    (13127, 0, 0, 100),  -- Poison Arrer
    (13128, 0, 0, 100),  -- Stabbity
    (13129, 0, 0, 100),  -- Feral Bite
    (13130, 0, 0, 100),  -- Poison Arrer
    (13131, 0, 0, 100),  -- Stabbity
    (13132, 0, 0, 100),  -- 'Ead Stabbity
    (13133, 0, 0, 100),  -- 'Ead Stabbity
    (13134, 30, 30, 100),  -- Brain Bursta
    (13135, 0, 0, 100),  -- Huge Fist
    (13136, 0, 0, 100),  -- Vicious Bite
    (13137, 0, 0, 100),  -- Writhing Fangs
    (13138, 0, 0, 100),  -- Vicious Scythe
    (13139, 0, 0, 100),  -- Boney Strike
    (13140, 0, 0, 100),  -- Vicious Bite
    (13141, 0, 0, 100),  -- Writhing Fangs
    (13142, 0, 0, 100),  -- Wide Swing
    (13143, 0, 0, 100),  -- Vicious Scythe
    (13144, 0, 0, 100),  -- Chomp
    (13145, 0, 0, 100),  -- Chomp
    (13146, 0, 0, 100),  -- Wail
    (13147, 0, 0, 100),  -- Brain Bursta
    (13148, 0, 0, 100),  -- Stir 'em Up
    (13149, 0, 0, 100),  -- Throw Jar
    (13150, 0, 0, 100),  -- Goo Ball
    (13151, 0, 0, 100),  -- Horn Gore
    (13152, 0, 0, 100),  -- Chomp
    (13153, 0, 0, 100),  -- Putrid Vomit
    (13154, 0, 0, 100),  -- Reapa Rush
    (13240, 0, 0, 100),  -- HW1 Melee
    (13241, 0, 0, 100),  -- HW2 Magic
    (13242, 0, 0, 100),  -- HW3 Magic
    (13252, 0, 0, 35),  -- Bloodthirsty Howl
    (13253, 0, 0, 25),  -- Crush Hands
    (13254, 0, 0, 45),  -- Savage Lunge
    (13257, 0, 0, 40),  -- Blood Strike
    (13258, 0, 0, 35),  -- Bloody Roar
    (13259, 0, 0, 45),  -- Slamming Blow
    (13261, 0, 0, 40),  -- Achilles Tear
    (13262, 0, 0, 45),  -- Smash
    (13309, 0, 0, 1),  -- Power of the Worldbearer
    (13310, 0, 0, 1),  -- Power of the Worldbearer
    (13311, 0, 0, 1),  -- Power of the Worldbearer
    (13312, 0, 0, 1),  -- Power of the Worldbearer
    (13318, 0, 0, 40),  -- Heavy Strike
    (13362, 0, 0, 30),  -- Phakth's Decimation
    (13364, 0, 0, 125),  -- Enduring Mutilation
    (13365, 0, 0, 125),  -- Tank - Morale Damage Shout
    (13367, 0, 0, 125),  -- Tank - Stun
    (13368, 0, 0, 125),  -- Tank - Armor Buff
    (13370, 0, 0, 125),  -- Debilitating Resolve
    (13371, 0, 0, 125),  -- Indiscriminant Torment
    (13373, 0, 0, 125),  -- Celestial Drought
    (13374, 0, 0, 125),  -- Toil of the Underworld
    (13375, 0, 0, 125),  -- Spite of Djaf
    (13376, 0, 0, 30),  -- Theft of Vitality
    (13378, 0, 0, 125),  -- Smothering Sands
    (13379, 0, 0, 125),  -- Deprivation of Resolve
    (13380, 0, 0, 125),  -- Aegis of Ualatp
    (13381, 0, 0, 125),  -- Overwhelming Gale
    (13421, 0, 0, 40),  -- Holy Wrath
    (13711, 0, 0, 100),  -- Warp Rage
    (13786, 0, 0, 1),  -- Crawling Multitude
    (13795, 0, 0, 1),  -- Terror
    (13870, 0, 0, 1),  -- Pulverizing Barrage
    (13900, 0, 0, 40),  -- Corrosive Slobber
    (13902, 0, 0, 1),  -- Leeching Bite
    (13903, 0, 0, 1),  -- Fetid Odor
    (13904, 0, 0, 1),  -- Mephitic Odor
    (13914, 0, 0, 175),  -- Glob of Pus
    (13915, 0, 0, 175),  -- USE ME
    (13916, 0, 0, 40),  -- Bilerot Spew
    (13917, 0, 0, 175),  -- Slimehound Slobber
    (13918, 0, 0, 40),  -- Bilerot Spew
    (13919, 0, 0, 40),  -- Fierce Blow
    (13920, 0, 0, 40),  -- Bilerot Sickness
    (13922, 0, 0, 40),  -- Maggotlord Spew
    (13923, 0, 0, 40),  -- Bilerot Sickness
    (13926, 0, 0, 40),  -- Death's Head
    (13927, 0, 0, 40),  -- Sneeze
    (13933, 0, 0, 40),  -- Devour
    (13935, 0, 0, 40),  -- Regurgitate
    (13936, 0, 0, 40),  -- Upset Stomach
    (13937, 0, 0, 40),  -- Warp Bomb
    (13956, 0, 0, 1),  -- Skiv's Blade Dance Counter
    (13959, 0, 0, 40),  -- Grey Seer Defile
    (13960, 0, 0, 175),  -- Tentacle Spit
    (13961, 0, 0, 40),  -- Malignant Fist
    (14864, 0, 0, 55),  -- Rupture Organ
    (14900, 0, 0, 55),  -- Clip Tendon
    (14948, 0, 0, 55),  -- Impose Suffering
    (20224, 0, 0, 125),  -- Sapping Strike
    (20361, 55, 55, 0),  -- Squig Armor
    (20375, 0, 0, 125),  -- Acid Bomb
    (20436, 35, 35, 125),  -- Consume Essence
    (20518, 35, 35, 125),  -- Divine Strike
    (20649, 0, 0, 25),  -- Sun Scales
    (21283, 0, 0, 125),  -- Scorpion Strike
    (21320, 0, 0, 125),  -- Scorpion's Power
    (21372, 0, 0, 75),  -- Accelerated Intimation
    (21421, 0, 0, 75),  -- Aura of Restoration
    (23737, 0, 0, 1),  -- Malachian Eggs
    (24824, 0, 0, 50);  -- Snare Net

UPDATE mythic_src_abilities m
  JOIN tmp_05_client_ap c ON c.Entry = m.Entry
   SET m.ApCost = c.ClientAp
 WHERE COALESCE(m.ApCost, 0) = c.OldSrc
   AND COALESCE(m.ChannelID, 0) = 0;

UPDATE abilities a
  JOIN tmp_05_client_ap c ON c.Entry = a.Entry
   SET a.ApCost = c.ClientAp
 WHERE COALESCE(a.ApCost, 0) = c.OldAbl
   AND COALESCE(a.ChannelID, 0) = 0;

DROP TEMPORARY TABLE tmp_05_client_ap;

COMMIT;

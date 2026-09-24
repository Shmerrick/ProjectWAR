-- 04_conform_ability_cast_times_to_client.sql
--
-- Sets 524 ability cast times to the client's.
--
-- POLICY. The 1.4.8 client is the arbiter. data/bin/abilityexport.bin and the CastTime column are both
-- milliseconds. Migration 02 already took every cast time a live packet capture also confirmed; this
-- takes the rest.
--
-- NOT HERE: 82 abilities that are channels by our ChannelID, a live channel start in the captures,
-- or the client's own ChannelInterval. The client keeps a channel's CastTime at 0 and its length in a
-- component's Duration, while the emulator stores the length in CastTime (NewChannelHandler refuses a
-- zero), so copying the client's 0 would turn a channel into an instant. They conform with the channel
-- handler, not with this column.
--
-- Both tables are written (CLAUDE.md hard rule 1). Each carries its own old value, so a row where
-- abilities and mythic_src_abilities had drifted apart still conforms in both.
--
-- Safe to re-run: every UPDATE matches only the old value, per table. Ability data is cached at
-- boot, so restart the server.

USE war_world;

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_04_client_cast_ms;
CREATE TEMPORARY TABLE tmp_04_client_cast_ms (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    OldSrc INT UNSIGNED NULL,
    OldAbl INT UNSIGNED NULL,
    ClientMs INT UNSIGNED NOT NULL
);

-- Entry, mythic_src_abilities.CastTime and abilities.CastTime where they differ from the client (NULL
-- read as 0; NULL here means that table already agrees), the client's cast time.
INSERT INTO tmp_04_client_cast_ms (Entry, OldSrc, OldAbl, ClientMs) VALUES
    (65, 0, NULL, 5000),  -- Runepriest's Strength
    (66, 0, NULL, 5000),  -- Runepriest's Armor
    (67, 0, NULL, 5000),  -- Runepriest's Ward
    (68, 0, NULL, 5000),  -- Runepriest's Regeneration
    (84, 0, NULL, 1000),  -- GTAE w/Proj for Nate
    (85, 0, NULL, 1000),  -- Sticky Grenade TEST For Nate
    (86, 0, NULL, 1000),  -- VFX Anim Bug TEST for Nate
    (124, 0, NULL, 2000),  -- Ravens Bite
    (125, 0, NULL, 2000),  -- Storm of Ravens
    (137, 0, NULL, 1000),  -- Black Fireball (Small)
    (138, 0, NULL, 3000),  -- Black Fireball (AOE)
    (140, 0, NULL, 1000),  -- Sun Fireball (Small)
    (141, 0, NULL, 3000),  -- Sun Fireball (AOE)
    (191, 0, NULL, 10000),  -- Test Huge Melee Crit Buff
    (192, 0, NULL, 10000),  -- Test Huge Ranged Crit Buff
    (193, 0, NULL, 10000),  -- Test Huge Magic Crit Buff
    (389, 0, NULL, 2000),  -- Goop Shootin'
    (390, 0, NULL, 2000),  -- Spore Cloud
    (1209, 0, NULL, 2000),  -- Anim Tag Test - Throw
    (1223, 0, NULL, 2000),  -- Anim Tag Test - Throw
    (1224, 0, NULL, 2000),  -- Anim Tag Test - Throw
    (1225, 0, NULL, 2000),  -- Anim Tag Test - Throw
    (1226, 0, NULL, 2000),  -- Anim Tag Test - Throw
    (1227, 0, NULL, 2000),  -- Anim Tag Test - Throw
    (1228, 0, NULL, 2000),  -- Anim Tag Test - Throw
    (1229, 0, NULL, 2000),  -- Anim Tag Test - Throw
    (3498, 0, NULL, 1000),  -- Sub Ability
    (3500, 0, 0, 1000),  -- Immolating Grasp
    (3551, 0, 0, 2000),  -- Lingering Rune of Mending
    (3552, 0, 0, 2000),  -- Lingering Gork'll Fix It
    (3553, 0, 0, 2000),  -- Lingering Divine Aid
    (3554, 0, 0, 2000),  -- Lingering Dark Medicine
    (3558, 0, 0, 2000),  -- Lingering Healing Energy
    (3981, 0, 0, 1000),  -- Sundered Motion
    (3993, 0, 0, 1000),  -- Cleansing Vitality
    (3997, 0, NULL, 1000),  -- Dissolving Mist
    (3998, 0, 0, 1000),  -- Leaping Alteration
    (3999, 0, NULL, 1000),  -- Tzeentch's Grip
    (4038, 0, NULL, 2000),  -- Flaming Sword of Rhuin
    (4039, 0, NULL, 3000),  -- The Burning Head
    (4041, 0, NULL, 1500),  -- Conflagration of Doom
    (4042, 0, NULL, 1500),  -- Rule of Burning Iron
    (4043, 0, NULL, 2000),  -- Commandment of Brass
    (4044, 0, NULL, 3000),  -- Transmutation of Lead
    (4045, 0, NULL, 2000),  -- Distillation of Molten Silver
    (4047, 0, NULL, 500),  -- Creeping Death
    (4049, 0, NULL, 2000),  -- Shades of Death
    (4050, 0, NULL, 3000),  -- Pit of Shades
    (4051, 0, NULL, 2000),  -- The Bear's Anger
    (4052, 0, NULL, 2000),  -- The Oxen Stands
    (4053, 0, NULL, 1500),  -- The Crow's Feast
    (4054, 0, NULL, 3000),  -- The Beast Cowers
    (4055, 0, NULL, 2500),  -- The Hunter's Spear
    (4056, 0, NULL, 1000),  -- The Wolf Hunts
    (4057, 0, NULL, 2000),  -- Portent of Far
    (4058, 0, NULL, 2000),  -- Second Sign of Amul
    (4059, 0, NULL, 2000),  -- Celestial Shield
    (4061, 0, NULL, 2500),  -- Uranon's Thunderbolt
    (4062, 0, NULL, 4000),  -- The Comet of Casandora
    (4063, 0, NULL, 2000),  -- Burning Gaze
    (4064, 0, NULL, 2000),  -- Pha's Illumination
    (4066, 0, NULL, 3500),  -- Dazzling Brightness
    (4067, 0, NULL, 3000),  -- Cleansing Flare
    (4068, 0, NULL, 3000),  -- Mistress of the Marsh
    (4069, 0, NULL, 1500),  -- Master of the Wood
    (4070, 0, NULL, 6000),  -- Gift of Life
    (4071, 0, NULL, 2000),  -- The Howler Wind
    (4072, 0, NULL, 2500),  -- The Rain Lord
    (4074, 0, NULL, 1500),  -- Dark Hand of Death
    (4076, 0, NULL, 2000),  -- Wind of Death
    (4077, 0, NULL, 3500),  -- Doom and Darkness
    (4080, 0, NULL, 3000),  -- Favoured Poxes
    (4081, 0, NULL, 500),  -- Effulgent Boils
    (4082, 0, NULL, 2000),  -- Glistening Scabs
    (4083, 0, NULL, 1500),  -- Glorious Afflictions
    (4084, 0, NULL, 1000),  -- Sumptuous Pestilence
    (4085, 0, NULL, 1500),  -- Red Fire of Alteration
    (4086, 0, NULL, 2000),  -- Orange Fire of Transition
    (4087, 0, NULL, 2000),  -- Yellow Fire of Transformation
    (4088, 0, NULL, 2500),  -- Blue Fire of Metamorphosis
    (4089, 0, NULL, 1000),  -- Indigo Fire of Change
    (4090, 0, NULL, 3000),  -- Violet Fire of Tzeentch
    (4092, 0, NULL, 2000),  -- Luxuroius Torment
    (4093, 0, NULL, 3500),  -- Titillating Delusions
    (4094, 0, NULL, 2000),  -- Enrapturing Spasms
    (4095, 0, NULL, 3500),  -- Invocation of Nehek
    (4096, 0, NULL, 2000),  -- Hand of Dust
    (4097, 0, NULL, 2000),  -- Hellish Vigour
    (4098, 0, NULL, 1500),  -- Gaze of Nagash
    (4099, 0, NULL, 2000),  -- Vanhel's Danse Macabre
    (4100, 0, NULL, 3500),  -- Curse of Years
    (4101, 0, NULL, 1500),  -- Gaze of Gork
    (4103, 0, NULL, 3500),  -- Gork'll Fix It
    (4104, 0, NULL, 1500),  -- Fists of Gork
    (4105, 0, NULL, 1000),  -- Waaagh
    (4106, 0, NULL, 2000),  -- Mork Wants Ya
    (4107, 0, NULL, 2000),  -- Bash 'Em Ladz
    (4108, 0, NULL, 2500),  -- Bloodgruel
    (4110, 0, NULL, 2000),  -- Bullgorger
    (4112, 0, NULL, 2000),  -- Toothcracker
    (4113, 0, NULL, 2000),  -- Trollguts
    (4114, 0, NULL, 2000),  -- Warp Lightning
    (4115, 0, NULL, 1000),  -- Pestelent Breath
    (4116, 0, NULL, 2000),  -- Vermintide
    (4117, 0, NULL, 3500),  -- Pestilence
    (4118, 0, NULL, 2000),  -- Death Frenzy
    (4119, 0, NULL, 2000),  -- Mark of Fury
    (4120, 0, NULL, 2000),  -- Mark of Tzeentch
    (4121, 0, NULL, 2000),  -- Hunter's Mark
    (4122, 0, NULL, 1000),  -- Mark of Khorne
    (4131, 0, NULL, 3000),  -- Hush
    (4145, 0, NULL, 500),  -- Poison Wind Globe
    (4148, 0, NULL, 2000),  -- Seething Plague
    (4149, 0, NULL, 1000),  -- Aqshi Unbound
    (4150, 0, NULL, 1000),  -- Resist Fear and Terror
    (4151, 0, NULL, 1000),  -- Penetrate Magic Resistance
    (4152, 0, NULL, 1000),  -- Penetrate Natural Armor
    (4153, 0, NULL, 1000),  -- Penetrate Wards
    (4154, 0, NULL, 1000),  -- Penetrate Ethereal
    (4155, 0, NULL, 1000),  -- Firing
    (4156, 0, NULL, 1000),  -- Firing
    (4157, 0, NULL, 1000),  -- Firing
    (4158, 0, NULL, 1000),  -- Firing
    (4159, 0, NULL, 1000),  -- Firing
    (4162, 0, NULL, 5000),  -- Hellish Vigour
    (4163, 0, NULL, 5000),  -- Magnificent Buboes
    (4165, 0, NULL, 5000),  -- Seething Plague
    (4195, 0, NULL, 1000),  -- Rock Skin
    (4196, 0, NULL, 1000),  -- Electric Skin
    (4197, 0, NULL, 1000),  -- Electric Skin
    (4200, 0, NULL, 2000),  -- Lileath's Tear
    (4205, 0, NULL, 2000),  -- Archmage Illidar Bolt
    (4210, 0, NULL, 2000),  -- Regen
    (4214, 0, NULL, 2000),  -- Enraged
    (4216, 0, NULL, 2000),  -- HIgh DMG Raid Boss Ability
    (4220, 0, NULL, 2000),  -- Bloodsoil Poison
    (4224, 0, NULL, 2000),  -- Clynch Test Ability
    (4225, 0, NULL, 3000),  -- Valaan's Sack
    (4226, 0, NULL, 1000),  -- Sharp Projectile
    (4227, 0, NULL, 2000),  -- Fire Shot
    (4229, 0, NULL, 1000),  -- Raid Boss Test Ability 2 (AE Whirl)
    (4230, 0, NULL, 2000),  -- Raid Boss Test Ability 3 (High Damage with Prep)
    (4231, 0, NULL, 2500),  -- Ability for AZ
    (4232, 0, NULL, 5000),  -- Summon Construct
    (4235, 0, NULL, 2000),  -- Chimeral Talisman
    (4238, 0, NULL, 5000),  -- Clynch Rat
    (4239, 0, NULL, 2000),  -- VOG
    (4243, 0, NULL, 2000),  -- Beast Melee Whirlwind
    (4247, 0, NULL, 2000),  -- Beast Roar
    (4248, 0, NULL, 1000),  -- Beast Cast
    (4249, 0, NULL, 2500),  -- Beast Cast PBAE
    (4250, 0, NULL, 1000),  -- Beast Long Prep Cast
    (4251, 0, NULL, 2500),  -- Beast Long Prep Cast PBAE
    (4256, 0, NULL, 2000),  -- Humanoid Melee Whirl
    (4257, 0, NULL, 1000),  -- Humanoid Cast
    (4258, 0, NULL, 2500),  -- Humanoid Cast PBAE
    (4259, 0, NULL, 1000),  -- Humanoid Long Prep Cast
    (4260, 0, NULL, 2500),  -- Humanoid Long Prep Cast PBAE
    (4261, 0, NULL, 1000),  -- Humanoid Instant Cast
    (4262, 0, NULL, 2500),  -- Humanoid Instant Cast PBAE
    (4264, 0, NULL, 2000),  -- Chimeral Talisman
    (4291, 0, NULL, 2000),  -- ability
    (4301, 0, NULL, 2000),  -- Rival's Ruin
    (4303, 0, NULL, 2000),  -- Rival's Ruin
    (4305, 0, NULL, 2000),  -- Descending Doom
    (4307, 0, NULL, 2000),  -- Descending Doom
    (4309, 0, NULL, 2000),  -- Descending Doom
    (4311, 0, NULL, 2000),  -- Manticore Sting
    (4313, 0, NULL, 2000),  -- Hydra Strike
    (4323, 0, NULL, 2000),  -- Asuryan's Mercy
    (4326, 0, NULL, 1000),  -- Kiss of Calamity
    (4327, 0, NULL, 1000),  -- Reaping Bolt
    (4331, 0, NULL, 1000),  -- Ghal Maraz's Maelstrom
    (4333, 0, NULL, 1000),  -- Holy Wrath
    (4338, 0, 0, 1000),  -- Wing Buffet
    (4341, 0, NULL, 4000),  -- Comet
    (4343, 0, NULL, 1000),  -- Wing Buffet
    (4365, 0, NULL, 1000),  -- Elemental Strike
    (4367, 0, NULL, 1000),  -- Exploding Flames
    (4368, 0, NULL, 2000),  -- Fireball
    (4369, 0, NULL, 1000),  -- Heavy Strike
    (4370, 0, NULL, 1000),  -- Heal
    (4371, 0, NULL, 1000),  -- Mine Explosion
    (4372, 0, NULL, 1500),  -- Bullet
    (4373, 0, NULL, 1000),  -- Kromil - Frontal Shot
    (4377, 0, NULL, 3000),  -- Essence Gather (Spider Queen Minion Enter)
    (4380, 0, 0, 1000),  -- Torch of Lileath
    (4406, 0, NULL, 1000),  -- Pain-Fury Whirlwind
    (4409, 0, NULL, 3000),  -- Essence Bolt (Beastman Reanimation)
    (4410, 0, NULL, 3000),  -- Spider Lay Egg
    (4424, 0, NULL, 3000),  -- Writhing Heal
    (4425, 0, NULL, 2000),  -- Writhing Energy
    (4426, 0, NULL, 1000),  -- Writhing Pain
    (4428, 0, 0, 1000),  -- Writhing Pleasure
    (4437, 0, NULL, 1000),  -- Writhing Bolt
    (4449, 0, NULL, 2000),  -- Azyr's Beckoning
    (4455, 0, NULL, 3000),  -- Menhir Shard
    (4456, 0, NULL, 3000),  -- Everqueen's Blessing
    (4555, 0, NULL, 2000),  -- Fear
    (4556, 0, NULL, 2000),  -- Fear
    (4613, 0, NULL, 2000),  -- Ethereal Wave
    (4625, 0, 0, 2000),  -- Circle of Life Loss
    (4627, 0, NULL, 2000),  -- Elemental Line
    (4628, 0, NULL, 2000),  -- Corporeal Line
    (4629, 0, NULL, 2000),  -- Ethereal Line
    (4631, 0, NULL, 2000),  -- Corporeal Breath
    (4632, 0, NULL, 2000),  -- Ethereal Breath
    (4635, 0, NULL, 2000),  -- Concussion Projectile
    (4638, 0, NULL, 2000),  -- Ethereal Stun
    (4654, 0, NULL, 2000),  -- Corporeal Thrust
    (4658, 0, NULL, 2000),  -- Ethereal Push
    (4661, 0, NULL, 2000),  -- Elemental Wave Push
    (4662, 0, NULL, 2000),  -- Corporeal Wave Push
    (4663, 0, NULL, 2000),  -- Ethereal Wave Push
    (4666, 0, NULL, 2000),  -- Corporeal Stop
    (4671, 0, NULL, 2000),  -- Elemental Knockdown
    (4672, 0, NULL, 2000),  -- Corporeal Knockdown
    (4673, 0, NULL, 2000),  -- Ethereal Knockdown
    (4675, 0, NULL, 2000),  -- Elemental Stop Blast
    (4676, 0, NULL, 2000),  -- Corporeal Stop Blast
    (4677, 0, NULL, 2000),  -- Ethereal Stop Blast
    (4679, 0, NULL, 2000),  -- Elemental Stop Wave
    (4680, 0, NULL, 2000),  -- Corporeal Stop Wave
    (4681, 0, NULL, 2000),  -- Ethereal Stop Wave
    (4684, 0, NULL, 2000),  -- Corporeal Mass Thrust
    (4689, 0, NULL, 2000),  -- Corporeal Silence
    (4694, 0, NULL, 2000),  -- Ethereal Mass Silence
    (4696, 0, NULL, 2000),  -- Elemental Silence Blast
    (4698, 0, NULL, 2000),  -- Ethereal Silence Blast
    (4700, 0, NULL, 2000),  -- Elemental Disarm
    (4701, 0, NULL, 2000),  -- Corporeal Disarm
    (4702, 0, NULL, 2000),  -- Ethereal Disarm
    (4704, 0, NULL, 2000),  -- Elemental Mass Disarm
    (4705, 0, NULL, 2000),  -- Corporeal Mass Disarm
    (4706, 0, NULL, 2000),  -- Ethereal Mass Disarm
    (4708, 0, NULL, 2000),  -- Ethereal Disarm Blast
    (4709, 0, NULL, 2000),  -- Corporeal Disarm Blast
    (4710, 0, NULL, 2000),  -- Ethereal Disarm Blast
    (4719, 0, NULL, 2000),  -- Focused Rage
    (4720, 0, NULL, 2000),  -- Iron Will
    (4721, 0, NULL, 2000),  -- Resistance
    (4722, 0, NULL, 2000),  -- Fear
    (4730, 0, NULL, 2000),  -- Magi Bolt
    (4733, 0, NULL, 1000),  -- Throwing ...
    (4735, 0, NULL, 2000),  -- Windstep's Curse
    (4736, 0, NULL, 1000),  -- Jaln's Device
    (4802, 0, NULL, 2000),  -- Ravens Bite
    (4803, 0, NULL, 2000),  -- Storm of Ravens
    (4804, 0, NULL, 2000),  -- Bone Swarm
    (4806, 0, NULL, 2000),  -- Death Metal
    (4807, 0, NULL, 2000),  -- Molotov Cocktail
    (4808, 0, NULL, 2000),  -- The Pox
    (4809, 0, NULL, 2000),  -- Lingering Plague
    (4811, 1000, 1000, 2000),  -- Crippling Stomp
    (4813, 0, NULL, 2000),  -- Tick Tick Boom
    (4815, 0, NULL, 2000),  -- Calumel Archmage Bolt
    (4864, 0, NULL, 4000),  -- Tchar'zanek Summon Fire Tornado
    (4865, 0, NULL, 4000),  -- Tchar'zanek Summon Wind Tornado
    (4866, 0, NULL, 4000),  -- Tchar'zanek Summon Water Tornado
    (4900, 0, NULL, 2000),  -- Chris
    (4901, 0, NULL, 1000),  -- Chris
    (4902, 0, NULL, 3000),  -- Chris
    (4903, 0, NULL, 3000),  -- Chris
    (4904, 0, NULL, 1000),  -- Chris
    (4907, 0, NULL, 4000),  -- Inferno Wave
    (4908, 0, NULL, 2000),  -- Conflagration of Doom
    (4910, 0, NULL, 6000),  -- Exorcism
    (4911, 0, NULL, 1000),  -- Stream of Corruption
    (4912, 0, NULL, 2000),  -- Pestilent Globule
    (4914, 0, NULL, 2000),  -- Throw Fruit
    (4915, 0, NULL, 2000),  -- Throw Vegatables
    (4916, 0, NULL, 1000),  -- Conflagration of Doom
    (4923, 0, NULL, 500),  -- Theogonist's Smite
    (4924, 0, NULL, 500),  -- Staff of Command
    (4925, 0, NULL, 3000),  -- Rebirth
    (4927, 0, NULL, 1000),  -- Giant Fireball
    (4965, 0, NULL, 2000),  -- Rage
    (4967, 0, NULL, 2000),  -- Ritual Kris of Khaine
    (4970, 0, NULL, 1000),  -- Throw
    (4988, 0, NULL, 3000),  -- Black Horror
    (4989, 0, NULL, 2000),  -- Chillwind
    (4995, 0, NULL, 3000),  -- Arkaneth Horn
    (4996, 0, NULL, 3000),  -- Aura of Disruption
    (5001, 0, NULL, 2000),  -- Paincaller's Wrath
    (5002, 0, NULL, 2000),  -- Hypnotic Agent
    (5003, 0, NULL, 3000),  -- Glory of the Raven God
    (5004, 0, NULL, 3000),  -- Phlegmatic Spore
    (5005, 0, NULL, 3000),  -- Shard of the Ravenshrine
    (5011, 0, NULL, 2000),  -- Slimy Vomit
    (5013, 0, NULL, 2000),  -- Boneskin
    (5014, 0, NULL, 1000),  -- Squig Breath
    (5026, 0, NULL, 1500),  -- Flask of Grabity Fings
    (5027, 0, NULL, 2000),  -- Fungal Spirits
    (5028, 0, NULL, 2000),  -- Brewer's Spirits
    (5029, 0, NULL, 2000),  -- Biting Spirits
    (5030, 0, NULL, 2000),  -- Fungal Spirits
    (5031, 0, NULL, 2000),  -- Brewer's Spirits
    (5032, 0, NULL, 2000),  -- Biting Spirits
    (5033, 0, NULL, 500),  -- Throw Torch
    (5040, 0, NULL, 1000),  -- Sticky Web
    (5042, 0, NULL, 1000),  -- Mysterious Seal
    (5045, 0, NULL, 1000),  -- Throw Mud
    (5047, 0, NULL, 3000),  -- Summon Sprites
    (5052, 0, NULL, 2000),  -- Shield
    (5053, 0, NULL, 3000),  -- Bor Graymane's Eye
    (5064, 0, NULL, 1000),  -- Rage of Khorne
    (5065, 0, NULL, 2000),  -- Dead Silence
    (5068, 0, NULL, 2000),  -- Blood Pool
    (5069, 0, NULL, 2000),  -- Blood Mark
    (5070, 0, NULL, 2000),  -- Breath of Change
    (5076, 0, NULL, 1500),  -- Bloodsoil Poison
    (5083, 0, NULL, 2000),  -- Blood Fist
    (5085, 0, NULL, 3000),  -- Shadow Shard Syphon
    (5086, 0, NULL, 3000),  -- Charm Captain Sualthin
    (5087, 0, NULL, 3000),  -- Activating...
    (5088, 0, NULL, 3000),  -- Activating...
    (5095, 0, NULL, 3000),  -- Captivity Crystal
    (5105, 0, NULL, 2000),  -- Lash
    (5107, 0, NULL, 2000),  -- Knockback
    (5108, 0, NULL, 1000),  -- Blast of the Corrupted
    (5119, 0, NULL, 1000),  -- Knockback
    (5120, 0, NULL, 500),  -- Cleave
    (5122, 0, NULL, 2000),  -- Gut Bounce
    (5123, 0, NULL, 1000),  -- Knockback
    (5125, 0, NULL, 2000),  -- Earthkeepers Howl
    (5126, 0, NULL, 1000),  -- Nature's Blast
    (5127, 0, NULL, 2000),  -- Life Loss
    (5128, 0, NULL, 1000),  -- Nature's Frailty
    (5129, 0, 0, 1000),  -- Earthen Spew
    (5130, 0, NULL, 2000),  -- Painful Dart
    (5131, 0, NULL, 2000),  -- Horrible Pain
    (5135, 0, NULL, 2000),  -- Forceful Blast
    (5136, 0, NULL, 2000),  -- Putrid Breath
    (5137, 0, 0, 1000),  -- Crippling Thorns
    (5138, 0, NULL, 2000),  -- Suffocation
    (5141, 0, NULL, 2000),  -- Viletongues Spit
    (5142, 0, 0, 1000),  -- Paralyzing Bite
    (5143, 0, NULL, 1000),  -- Toxic Wave
    (5147, 0, NULL, 5000),  -- Groundshaker Rumble
    (5148, 0, NULL, 1000),  -- Twisted Wrath
    (5149, 0, NULL, 1000),  -- Psyche Onslaught
    (5150, 0, NULL, 1000),  -- Pain of the Vale
    (5151, 0, NULL, 1000),  -- Pain of the Vale
    (5152, 0, NULL, 1000),  -- Corrupted Loam
    (5153, 0, NULL, 1000),  -- Pain Spike
    (5154, 0, NULL, 1000),  -- Pain Spike
    (5155, 0, NULL, 1000),  -- Spit of Corruption
    (5179, 0, NULL, 2000),  -- Pummel
    (5182, 0, NULL, 1000),  -- R'khar's Rage
    (5183, 0, NULL, 1000),  -- Putrid Breath
    (5184, 0, NULL, 1000),  -- Porus' Blessing
    (5185, 0, NULL, 2000),  -- Crippling Stomp
    (5186, 0, NULL, 1000),  -- Chilling Breath
    (5190, 0, NULL, 2000),  -- Eldazar Split
    (5191, 0, NULL, 1000),  -- Crippling Fear
    (5194, 0, NULL, 2000),  -- Knockback
    (5195, 0, NULL, 2000),  -- Trample
    (5196, 0, NULL, 1000),  -- Ragefire
    (5212, 0, NULL, 2000),  -- Mindrending Roar
    (5213, 0, NULL, 2000),  -- Point Blank Shot
    (5219, 0, NULL, 2000),  -- Squig Heal
    (5223, 0, NULL, 3000),  -- Grenade Blast
    (5230, 0, 0, 2000),  -- Maw Belch
    (5240, 1500, 1500, 0),  -- Gitzappa Aura
    (5323, 1000, 1000, 2000),  -- Yesterday's Slop
    (5401, 0, NULL, 2000),  -- Stomp
    (5402, 0, NULL, 1000),  -- Spine Shot
    (5409, 0, NULL, 2000),  -- Blast of the Dead
    (5410, 0, NULL, 2000),  -- Scream
    (5411, 0, NULL, 1000),  -- Crypt Blast
    (5412, 0, NULL, 1000),  -- Touch of the Dead
    (5415, 0, NULL, 1000),  -- Wracking Pain
    (5417, 0, NULL, 1000),  -- Keeper Blast
    (5423, 0, NULL, 1000),  -- Blast of Devotion
    (5424, 0, NULL, 3000),  -- Lifetap
    (5425, 0, NULL, 3000),  -- Channeling Stone
    (5426, 0, NULL, 3000),  -- Tablet Channel 2
    (5427, 0, NULL, 3000),  -- Tablet Channel 3
    (5549, 3000, 3000, 0),  -- Cleave
    (5594, 1000, 1000, 0),  -- Gut Spew
    (5731, 0, 0, 1000),  -- Throw Tomato
    (13383, 0, 0, 1000),  -- Crippling Enmity
    (14211, 3000, 3000, 2000),  -- Magnificent Fireworks
    (14213, 1000, 1000, 2000),  -- Common Fireworks
    (14242, 5000, 5000, 0),  -- Grimnir's Mercy
    (14478, 10000, 10000, 15000),  -- Teleporting
    (14479, 10000, 10000, 15000),  -- Teleporting
    (14480, 0, 0, 15000),  -- Teleporting
    (14684, 3000, 3000, 0),  -- Summon Mount
    (15151, 0, 0, 2000),  -- Chuffinbrau Ale
    (15171, 0, 0, 2000),  -- Signet of the Cursed Company
    (15175, 0, 0, 5000),  -- Call of the North
    (15181, 0, 0, 2000),  -- Dwarf Beer Keg
    (15188, 0, 0, 2000),  -- Imperial Hunting Hound
    (15189, 0, 0, 2000),  -- Imperial Hunting Hound
    (15190, 0, 0, 2000),  -- Warlord's Fell Hound
    (15191, 0, 0, 2000),  -- Warlord's Fell Hound
    (20361, 0, 0, 2000),  -- Squig Armor
    (21372, 0, 0, 2000),  -- Accelerated Intimation
    (23012, 1000, 1000, 0),  -- Flame Breath
    (23584, 0, 0, 1000),  -- Terminate
    (23666, 0, 0, 3000),  -- Deploy Ram
    (23668, 0, 0, 3000),  -- Deploy Cannon
    (23670, 0, 0, 3000),  -- Deploy Organ Gun
    (23674, 0, 0, 3000),  -- Deploy Supa-Chucka
    (23676, 0, 0, 3000),  -- Deploy Orcapult
    (23680, 0, 0, 3000),  -- Deploy Ballista
    (23682, 0, 0, 3000),  -- Deploy Bolt Thrower
    (23686, 0, 0, 3000),  -- Deploy Ballista
    (23688, 0, 0, 3000),  -- Deploy Bolt Thrower
    (23918, 0, 0, 3000),  -- Deploy Ram
    (23960, 0, 0, 3000),  -- Deploy Ram
    (23966, 0, 0, 3000),  -- Deploy Ram
    (24664, 0, 0, 3000),  -- Deploy Cannon
    (24666, 0, 0, 3000),  -- Deploy Hellblaster
    (24670, 0, 0, 3000),  -- Deploy Hellcannon
    (24672, 0, 0, 3000),  -- Deploy Tri-Barrel Hellcannon
    (24770, 0, 0, 3000),  -- Deploy Ram
    (24776, 0, 0, 3000),  -- Deploy Ram
    (27999, 0, 0, 1500),  -- WAR Tract
    (28600, 0, 0, 2000),  -- Hazy Two-Headed Hound
    (28601, 0, 0, 2000),  -- Arctic Two-Headed Hound
    (28602, 0, 0, 2000),  -- Earthy Two-Headed Hound
    (28603, 0, 0, 2000),  -- Blackened Two-Headed Hound
    (28610, 0, 0, 2000),  -- Steel Armored Mite
    (28611, 0, 0, 2000),  -- Yellow Armored Mite
    (28612, 0, 0, 2000),  -- Blue Armored Mite
    (28613, 0, 0, 2000),  -- Sage Armored Mite
    (28620, 0, 0, 2000),  -- Grey Drumming Bear
    (28621, 0, 0, 2000),  -- Sandy Drumming Bear
    (28622, 0, 0, 2000),  -- Brown Drumming Bear
    (28623, 0, 0, 2000),  -- Black Drumming Bear
    (28630, 0, 0, 2000),  -- Red Vested Powder Monkey
    (28631, 0, 0, 2000),  -- Blue Vested Powder Monkey
    (28632, 0, 0, 2000),  -- Green Vested Powder Monkey
    (28633, 0, 0, 2000),  -- Yellow Vested Powder Monkey
    (28640, 0, 0, 2000),  -- Sulfur Fart Squig
    (28641, 0, 0, 2000),  -- Albino Fart Squig
    (28642, 0, 0, 2000),  -- Red Fart Squig
    (28643, 0, 0, 2000),  -- Black Fart Squig
    (28650, 0, 0, 2000),  -- Cabbage Snotling Beggar
    (28651, 0, 0, 2000),  -- Yellow Snotling Beggar
    (28652, 0, 0, 2000),  -- Pale Snotling Beggar
    (28653, 0, 0, 2000),  -- Olive Snotling Beggar
    (28660, 0, 0, 2000),  -- Red Garbed Snotling Herald
    (28661, 0, 0, 2000),  -- Yellow Garbed Snotling Herald
    (28662, 0, 0, 2000),  -- Orange Garbed Snotling Herald
    (28663, 0, 0, 2000),  -- Blue Garbed Snotling Herald
    (28670, 0, 0, 2000),  -- Lemon Ugly Hound
    (28671, 0, 0, 2000),  -- Tri-colored Ugly Hound
    (28672, 0, 0, 2000),  -- Mahogany Ugly Hound
    (28673, 0, 0, 2000),  -- Black and White Ugly Hound
    (28680, 0, 0, 2000),  -- Purple Ravenspawn
    (28681, 0, 0, 2000),  -- Navy Ravenspawn
    (28682, 0, 0, 2000),  -- Ash Ravenspawn
    (28683, 0, 0, 2000),  -- Periwinkle Ravenspawn
    (28690, 0, 0, 2000),  -- Fiery Coal Dark Sprite
    (28691, 0, 0, 2000),  -- Bloody Dark Sprite
    (28692, 0, 0, 2000),  -- Icy Dark Sprite
    (28693, 0, 0, 2000),  -- Brooding Dark Sprite
    (28700, 0, 0, 2000),  -- Blue Drake
    (28701, 0, 0, 2000),  -- Red Drake
    (28702, 0, 0, 2000),  -- Green Drake
    (28703, 0, 0, 2000),  -- Aqua Drake
    (28710, 0, 0, 2000),  -- Golden Hawk
    (28711, 0, 0, 2000),  -- Silver Hawk
    (28712, 0, 0, 2000),  -- Russet Hawk
    (28713, 0, 0, 2000),  -- Copper Hawk
    (28720, 0, 0, 2000),  -- Brown Pitbull
    (28721, 0, 0, 2000),  -- Black Pitbull
    (28722, 0, 0, 2000),  -- White Pitbull
    (28723, 0, 0, 2000),  -- Yellow Pitbull
    (28730, 0, 0, 2000),  -- Brown-Feathered Griffonling
    (28731, 0, 0, 2000),  -- Black-Feathered Griffonling
    (28732, 0, 0, 2000),  -- Caramel-Feathered Griffonling
    (28733, 0, 0, 2000),  -- Violet-Feathered Griffonling
    (28740, 0, 0, 2000),  -- Blue Horror
    (28741, 0, 0, 2000),  -- Red Horror
    (28742, 0, 0, 2000),  -- Purple Horror
    (28743, 0, 0, 2000),  -- Green Horror
    (28750, 0, 0, 2000),  -- Dry Imp Skeleton
    (28751, 0, 0, 2000),  -- Stained Imp Skeleton
    (28752, 0, 0, 2000),  -- Charred Imp Skeleton
    (28753, 0, 0, 2000),  -- Sickly Imp Skeleton
    (28760, 0, 0, 2000),  -- Steel Iron Hawk
    (28761, 0, 0, 2000),  -- Copper Iron Hawk
    (28762, 0, 0, 2000),  -- Dark Steel Iron Hawk
    (28763, 0, 0, 2000),  -- Iron Hawk
    (28770, 0, 0, 2000),  -- Green Jack O' Lantern
    (28771, 0, 0, 2000),  -- Purple Jack O' Lantern
    (28772, 0, 0, 2000),  -- Brown Jack O' Lantern
    (28773, 0, 0, 2000),  -- Yellow Jack O' Lantern
    (28780, 0, 0, 2000),  -- Grey-caped Mannequin
    (28781, 0, 0, 2000),  -- Brown-caped Mannequin
    (28782, 0, 0, 2000),  -- White-caped Mannequin
    (28783, 0, 0, 2000),  -- Leaf-caped Mannequin
    (28790, 0, 0, 2000),  -- Blue Jester
    (28791, 0, 0, 2000),  -- White Jester
    (28792, 0, 0, 2000),  -- Yellow Jester
    (28793, 0, 0, 2000),  -- Black Jester
    (28800, 0, 0, 2000),  -- Brown Owl
    (28801, 0, 0, 2000),  -- Dark Brown Owl
    (28802, 0, 0, 2000),  -- White Owl
    (28803, 0, 0, 2000),  -- Sandy Owl
    (28810, 0, 0, 2000),  -- Orange Phoenix
    (28811, 0, 0, 2000),  -- Magenta Phoenix
    (28812, 0, 0, 2000),  -- Ice Blue Phoenix
    (28813, 0, 0, 2000),  -- Yellow Phoenix
    (28820, 0, 0, 2000),  -- Dark Blue Tzeentch Familiar
    (28821, 0, 0, 2000),  -- Purple Tzeentch Familiar
    (28822, 0, 0, 2000),  -- Charcoal Tzeentch Familiar
    (28823, 0, 0, 2000),  -- Black-Feathered Tzeentch Familiar
    (28830, 0, 0, 2000),  -- Chocolate Bulldog
    (28831, 0, 0, 2000),  -- Black Bulldog
    (28832, 0, 0, 2000),  -- White Fawn Bulldog
    (28833, 0, 0, 2000),  -- Tan Bulldog
    (28840, 0, 0, 2000),  -- Young Brown Boar
    (28841, 0, 0, 2000),  -- Young Pink Boar
    (28842, 0, 0, 2000),  -- Young Black Boar
    (28843, 0, 0, 2000),  -- Young Straw Boar
    (28850, 0, 0, 2000),  -- Olive Cold One
    (28851, 0, 0, 2000),  -- Dark Green Cold One
    (28852, 0, 0, 2000),  -- Red Cold One
    (28853, 0, 0, 2000);  -- Black Grape Cold One

UPDATE mythic_src_abilities m
  JOIN tmp_04_client_cast_ms c ON c.Entry = m.Entry
   SET m.CastTime = c.ClientMs
 WHERE COALESCE(m.CastTime, 0) = c.OldSrc
   AND COALESCE(m.ChannelID, 0) = 0;

UPDATE abilities a
  JOIN tmp_04_client_cast_ms c ON c.Entry = a.Entry
   SET a.CastTime = c.ClientMs
 WHERE COALESCE(a.CastTime, 0) = c.OldAbl
   AND COALESCE(a.ChannelID, 0) = 0;

DROP TEMPORARY TABLE tmp_04_client_cast_ms;

COMMIT;

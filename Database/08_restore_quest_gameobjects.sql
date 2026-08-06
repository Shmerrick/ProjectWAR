-- Restore gameobjects that quest objectives reference. WORLD database.
--
-- 221 quests have an ObjType=3 (gameobject) objective pointing at an
-- Entry absent from gameobject_protos. The objective can never be credited, so
-- the quest can never be completed. The same missing rows also abort PQ spawn
-- loops, so this fixes both.
--
-- Source is Return of Reckoning's public GraphQL API, whose gameobject ids align
-- with ours (verified on entries 5, 44, 5006, 2000489). It supplies name,
-- modelName and spawn positions.
--
-- The API gives modelName as a string, not the numeric DisplayID this schema
-- needs. That map was derived from the 796 distinct models across the
-- 2633 protos already present, pairing the API's modelName with our DisplayID.
--
-- Deliberately conservative. An entry is only restored when BOTH its model is
-- known (so DisplayID is real, not guessed) AND the API has spawn placement.
-- A guessed DisplayID renders invisible and a proto without a spawn is
-- unreachable - both look fixed while staying broken.
--
--   gameobject entries needed by blocked quests : 237
--   present in the RoR API                      : 130
--   fully resolvable (model + spawn)            : 91
--   restored here (those that complete a quest) : 74
--
--   quests blocked                              : 221
--   quests fully unblocked by this script       : 57
--   quests still blocked afterwards             : 164
--
-- Orientation (WorldO) is not exposed by the API and defaults to 0, so restored
-- objects may face arbitrarily. Cosmetic.
--
-- Re-runnable: every insert is guarded.

-- 583  Bone Pile
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 583, 'Bone Pile', 42
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=583);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 583, 101, 51838, 58175, 3720, 0, 42
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=583 AND `ZoneId`=101 AND `WorldX`=51838 AND `WorldY`=58175);

-- 598  Shrine to Ulric
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 598, 'Shrine to Ulric', 212
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=598);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 598, 101, 43481, 46185, 4000, 0, 212
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=598 AND `ZoneId`=101 AND `WorldX`=43481 AND `WorldY`=46185);

-- 636  Chest
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 636, 'Chest', 1658
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=636);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 636, 105, 63218, 1144, 16456, 0, 1658
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=636 AND `ZoneId`=105 AND `WorldX`=63218 AND `WorldY`=1144);

-- 662  Pile O' Bones
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 662, 'Pile O\' Bones', 42
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=662);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 662, 7, 21097, 10778, 4896, 0, 42
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=662 AND `ZoneId`=7 AND `WorldX`=21097 AND `WorldY`=10778);

-- 675  Bag
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 675, 'Bag', 47
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=675);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 675, 294, 41204, 44985, 16384, 0, 47
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=675 AND `ZoneId`=294 AND `WorldX`=41204 AND `WorldY`=44985);

-- 679  Meat Barrel
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 679, 'Meat Barrel', 354
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=679);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 679, 5, 12042, 38879, 10656, 0, 354
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=679 AND `ZoneId`=5 AND `WorldX`=12042 AND `WorldY`=38879);

-- 680  Summon Horn
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 680, 'Summon Horn', 844
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=680);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 680, 161, 28507, 16282, 16845, 0, 844
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=680 AND `ZoneId`=161 AND `WorldX`=28507 AND `WorldY`=16282);

-- 774  Ancestral Stein
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 774, 'Ancestral Stein', 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=774);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 774, 91, 39237, 18303, 11592, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=774 AND `ZoneId`=91 AND `WorldX`=39237 AND `WorldY`=18303);

-- 786  Forgotten Chest
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 786, 'Forgotten Chest', 293
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=786);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 786, 103, 11724, 35185, 13385, 0, 293
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=786 AND `ZoneId`=103 AND `WorldX`=11724 AND `WorldY`=35185);

-- 816  Brittle Brazier
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 816, 'Brittle Brazier', 127
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=816);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 816, 92, 23160, 38562, 11120, 0, 127
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=816 AND `ZoneId`=92 AND `WorldX`=23160 AND `WorldY`=38562);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 816, 92, 23954, 38803, 11120, 0, 127
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=816 AND `ZoneId`=92 AND `WorldX`=23954 AND `WorldY`=38803);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 816, 92, 24604, 38590, 11120, 0, 127
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=816 AND `ZoneId`=92 AND `WorldX`=24604 AND `WorldY`=38590);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 816, 92, 22771, 37889, 11120, 0, 127
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=816 AND `ZoneId`=92 AND `WorldX`=22771 AND `WorldY`=37889);

-- 883  Altar
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 883, 'Altar', 246
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=883);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 883, 101, 31724, 4234, 11026, 0, 246
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=883 AND `ZoneId`=101 AND `WorldX`=31724 AND `WorldY`=4234);

-- 895  Debris Pile
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 895, 'Debris Pile', 1795
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=895);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 895, 209, 39083, 61933, 6914, 0, 1795
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=895 AND `ZoneId`=209 AND `WorldX`=39083 AND `WorldY`=61933);

-- 942  High Elf Corpse
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 942, 'High Elf Corpse', 1707
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=942);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 942, 203, 43759, 47002, 14485, 0, 1707
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=942 AND `ZoneId`=203 AND `WorldX`=43759 AND `WorldY`=47002);

-- 955  Lover's Box
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 955, 'Lover\'s Box', 10
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=955);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 955, 109, 24983, 38519, 17019, 0, 10
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=955 AND `ZoneId`=109 AND `WorldX`=24983 AND `WorldY`=38519);

-- 957  Offering Basket
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 957, 'Offering Basket', 1942
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=957);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 957, 161, 32457, 38463, 16865, 0, 1942
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=957 AND `ZoneId`=161 AND `WorldX`=32457 AND `WorldY`=38463);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 957, 161, 32832, 38308, 16865, 0, 1942
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=957 AND `ZoneId`=161 AND `WorldX`=32832 AND `WorldY`=38308);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 957, 161, 32469, 38683, 16865, 0, 1942
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=957 AND `ZoneId`=161 AND `WorldX`=32469 AND `WorldY`=38683);

-- 1006  Death's Pike
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1006, 'Death\'s Pike', 821
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1006);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1006, 105, 21790, 18160, 16158, 0, 821
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1006 AND `ZoneId`=105 AND `WorldX`=21790 AND `WorldY`=18160);

-- 1009  Signal Kindling
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1009, 'Signal Kindling', 367
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1009);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1009, 100, 30446, 54815, 3228, 0, 367
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1009 AND `ZoneId`=100 AND `WorldX`=30446 AND `WorldY`=54815);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1009, 100, 25964, 54919, 3294, 0, 367
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1009 AND `ZoneId`=100 AND `WorldX`=25964 AND `WorldY`=54919);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1009, 100, 28017, 54687, 3236, 0, 367
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1009 AND `ZoneId`=100 AND `WorldX`=28017 AND `WorldY`=54687);

-- 1014  Kislevite Knapsack
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1014, 'Kislevite Knapsack', 979
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1014);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1014, 102, 45088, 61997, 12926, 0, 979
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1014 AND `ZoneId`=102 AND `WorldX`=45088 AND `WorldY`=61997);

-- 1022  Verentane's Bell
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1022, 'Verentane\'s Bell', 846
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1022);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1022, 108, 28525, 9309, 15483, 0, 846
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1022 AND `ZoneId`=108 AND `WorldX`=28525 AND `WorldY`=9309);

-- 1184  Broken Barrel
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1184, 'Broken Barrel', 323
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1184);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1184, 107, 12179, 757, 5128, 0, 323
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1184 AND `ZoneId`=107 AND `WorldX`=12179 AND `WorldY`=757);

-- 1191  Dead Elf
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1191, 'Dead Elf', 1706
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1191);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1191, 208, 24607, 45529, 3472, 0, 1706
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1191 AND `ZoneId`=208 AND `WorldX`=24607 AND `WorldY`=45529);

-- 1200  Ghyran's Altar
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1200, 'Ghyran\'s Altar', 1578
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1200);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1200, 208, 16795, 31992, 4154, 0, 1578
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1200 AND `ZoneId`=208 AND `WorldX`=16795 AND `WorldY`=31992);

-- 1815  Sealed Sarcophagus
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1815, 'Sealed Sarcophagus', 230
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1815);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1815, 2, 32579, 30552, 3613, 0, 230
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1815 AND `ZoneId`=2 AND `WorldX`=32579 AND `WorldY`=30552);

-- 1945  Tong Curr
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 1945, 'Tong Curr', 800
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=1945);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 1945, 103, 49155, 18525, 18776, 0, 800
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=1945 AND `ZoneId`=103 AND `WorldX`=49155 AND `WorldY`=18525);

-- 2017  Uthorin Banner
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2017, 'Uthorin Banner', 1712
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2017);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2017, 203, 57943, 63628, 9618, 0, 1712
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2017 AND `ZoneId`=203 AND `WorldX`=57943 AND `WorldY`=63628);

-- 2082  Ragebeast Feed
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2082, 'Ragebeast Feed', 47
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2082);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2082, 160, 28816, 33935, 16749, 0, 47
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2082 AND `ZoneId`=160 AND `WorldX`=28816 AND `WorldY`=33935);

-- 2546  Ancient Urn
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2546, 'Ancient Urn', 4200
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2546);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2546, 101, 64400, 23708, 8477, 0, 4200
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2546 AND `ZoneId`=101 AND `WorldX`=64400 AND `WorldY`=23708);

-- 20001  Bright Wizard College Map Piece II
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 20001, 'Bright Wizard College Map Piece II', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=20001);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 20001, 161, 29236, 19169, 16680, 0, 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=20001 AND `ZoneId`=161 AND `WorldX`=29236 AND `WorldY`=19169);

-- 200145  Skeleton Hangman
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200145, 'Skeleton Hangman', 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200145);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200145, 106, 39136, 4647, 4868, 0, 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200145 AND `ZoneId`=106 AND `WorldX`=39136 AND `WorldY`=4647);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200145, 106, 36003, 19078, 5593, 0, 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200145 AND `ZoneId`=106 AND `WorldX`=36003 AND `WorldY`=19078);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200145, 108, 26940, 18936, 9895, 0, 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200145 AND `ZoneId`=108 AND `WorldX`=26940 AND `WorldY`=18936);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200145, 117, 26127, 34722, 57247, 0, 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200145 AND `ZoneId`=117 AND `WorldX`=26127 AND `WorldY`=34722);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200145, 117, 26256, 35384, 57179, 0, 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200145 AND `ZoneId`=117 AND `WorldX`=26256 AND `WorldY`=35384);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200145, 117, 26142, 34970, 57005, 0, 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200145 AND `ZoneId`=117 AND `WorldX`=26142 AND `WorldY`=34970);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200145, 117, 26780, 35320, 56874, 0, 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200145 AND `ZoneId`=117 AND `WorldX`=26780 AND `WorldY`=35320);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200145, 109, 21928, 17690, 19352, 0, 192
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200145 AND `ZoneId`=109 AND `WorldX`=21928 AND `WorldY`=17690);

-- 200155  Rune of Decay
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200155, 'Rune of Decay', 4327
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200155);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200155, 106, 62953, 17913, 6414, 0, 4327
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200155 AND `ZoneId`=106 AND `WorldX`=62953 AND `WorldY`=17913);

-- 200156  Rune of Change
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200156, 'Rune of Change', 3459
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200156);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200156, 106, 64945, 18172, 6387, 0, 3459
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200156 AND `ZoneId`=106 AND `WorldX`=64945 AND `WorldY`=18172);

-- 200157  Rune of Rage
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200157, 'Rune of Rage', 930
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200157);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200157, 106, 65074, 19716, 6387, 0, 930
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200157 AND `ZoneId`=106 AND `WorldX`=65074 AND `WorldY`=19716);

-- 200158  R.I.P. Mithi
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200158, 'R.I.P. Mithi', 184
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200158);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200158, 105, 25496, 59576, 14760, 0, 184
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200158 AND `ZoneId`=105 AND `WorldX`=25496 AND `WorldY`=59576);

-- 200159  Horror's Passage Banner
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200159, 'Horror\'s Passage Banner', 898
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200159);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200159, 103, 41197, 59253, 15081, 0, 898
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200159 AND `ZoneId`=103 AND `WorldX`=41197 AND `WorldY`=59253);

-- 200160  Worm's Mouth Banner
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200160, 'Worm\'s Mouth Banner', 898
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200160);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200160, 103, 45284, 57156, 14336, 0, 898
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200160 AND `ZoneId`=103 AND `WorldX`=45284 AND `WorldY`=57156);

-- 200161  Monolith Bridge Banner
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200161, 'Monolith Bridge Banner', 898
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200161);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200161, 103, 33479, 60705, 15426, 0, 898
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200161 AND `ZoneId`=103 AND `WorldX`=33479 AND `WorldY`=60705);

-- 200165  Morr's Embrace
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200165, 'Morr\'s Embrace', 1686
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200165);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200165, 106, 24442, 7229, 3607, 0, 1686
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200165 AND `ZoneId`=106 AND `WorldX`=24442 AND `WorldY`=7229);

-- 200174  Ritual Skull
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200174, 'Ritual Skull', 9267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200174);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200174, 294, 39644, 42355, 16384, 0, 9267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200174 AND `ZoneId`=294 AND `WorldX`=39644 AND `WorldY`=42355);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200174, 107, 13834, 28138, 5035, 0, 9267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200174 AND `ZoneId`=107 AND `WorldX`=13834 AND `WorldY`=28138);

-- 200175  Ritual Site
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200175, 'Ritual Site', 917
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200175);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200175, 107, 16066, 28953, 4742, 0, 917
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200175 AND `ZoneId`=107 AND `WorldX`=16066 AND `WorldY`=28953);

-- 200176  Ritual Bell
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200176, 'Ritual Bell', 846
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200176);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200176, 107, 13623, 29672, 5002, 0, 846
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200176 AND `ZoneId`=107 AND `WorldX`=13623 AND `WorldY`=29672);

-- 200178  Ancient Tomes
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200178, 'Ancient Tomes', 1505
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200178);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200178, 190, 32897, 31622, 9709, 0, 1505
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200178 AND `ZoneId`=190 AND `WorldX`=32897 AND `WorldY`=31622);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200178, 190, 32895, 31685, 9709, 0, 1505
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200178 AND `ZoneId`=190 AND `WorldX`=32895 AND `WorldY`=31685);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200178, 107, 26208, 17216, 4783, 0, 1505
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200178 AND `ZoneId`=107 AND `WorldX`=26208 AND `WorldY`=17216);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200178, 107, 26969, 18563, 4904, 0, 1505
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200178 AND `ZoneId`=107 AND `WorldX`=26969 AND `WorldY`=18563);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200178, 107, 28353, 15696, 4901, 0, 1505
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200178 AND `ZoneId`=107 AND `WorldX`=28353 AND `WorldY`=15696);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200178, 107, 27032, 16584, 4814, 0, 1505
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200178 AND `ZoneId`=107 AND `WorldX`=27032 AND `WorldY`=16584);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200178, 190, 32465, 31822, 9643, 0, 1505
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200178 AND `ZoneId`=190 AND `WorldX`=32465 AND `WorldY`=31822);

-- 200179  Corrupted Supplies
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200179, 'Corrupted Supplies', 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200179);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 18714, 41868, 6351, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=18714 AND `WorldY`=41868);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 18501, 42127, 6351, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=18501 AND `WorldY`=42127);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 18274, 41784, 6306, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=18274 AND `WorldY`=41784);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 18596, 41755, 6292, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=18596 AND `WorldY`=41755);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 17527, 42925, 6525, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=17527 AND `WorldY`=42925);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 18074, 42446, 6525, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=18074 AND `WorldY`=42446);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 21525, 33027, 6271, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=21525 AND `WorldY`=33027);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 21023, 32764, 6269, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=21023 AND `WorldY`=32764);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 22146, 33413, 6294, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=22146 AND `WorldY`=33413);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 21235, 32961, 6256, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=21235 AND `WorldY`=32961);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 22266, 33565, 6269, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=22266 AND `WorldY`=33565);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 21949, 33359, 6262, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=21949 AND `WorldY`=33359);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 21717, 33103, 6313, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=21717 AND `WorldY`=33103);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 21303, 32833, 6325, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=21303 AND `WorldY`=32833);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200179, 108, 22392, 33600, 6293, 0, 1637
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200179 AND `ZoneId`=108 AND `WorldX`=22392 AND `WorldY`=33600);

-- 200193  Unholy Torch
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200193, 'Unholy Torch', 101
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200193);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200193, 108, 4854, 12078, 11631, 0, 101
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200193 AND `ZoneId`=108 AND `WorldX`=4854 AND `WorldY`=12078);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200193, 108, 4596, 12429, 11565, 0, 101
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200193 AND `ZoneId`=108 AND `WorldX`=4596 AND `WorldY`=12429);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200193, 108, 4377, 12151, 11594, 0, 101
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200193 AND `ZoneId`=108 AND `WorldX`=4377 AND `WorldY`=12151);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200193, 108, 4607, 11745, 11707, 0, 101
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200193 AND `ZoneId`=108 AND `WorldX`=4607 AND `WorldY`=11745);

-- 200281  Hair of the Dog
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200281, 'Hair of the Dog', 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200281);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200281, 162, 25612, 35713, 12680, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200281 AND `ZoneId`=162 AND `WorldX`=25612 AND `WorldY`=35713);

-- 200282  Humpback Ale
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200282, 'Humpback Ale', 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200282);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200282, 162, 30344, 28586, 12491, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200282 AND `ZoneId`=162 AND `WorldX`=30344 AND `WorldY`=28586);

-- 200283  The Cat's Meow
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200283, 'The Cat\'s Meow', 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200283);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200283, 162, 36259, 40280, 12494, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200283 AND `ZoneId`=162 AND `WorldX`=36259 AND `WorldY`=40280);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200283, 162, 36205, 40268, 12494, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200283 AND `ZoneId`=162 AND `WorldX`=36205 AND `WorldY`=40268);

-- 200284  Reikland Gold Standard
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200284, 'Reikland Gold Standard', 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200284);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200284, 162, 19279, 38005, 12485, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200284 AND `ZoneId`=162 AND `WorldX`=19279 AND `WorldY`=38005);

-- 200285  Golden Griffon Grog
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200285, 'Golden Griffon Grog', 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200285);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200285, 162, 26981, 42783, 13329, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200285 AND `ZoneId`=162 AND `WorldX`=26981 AND `WorldY`=42783);

-- 200286  Morrsmead
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200286, 'Morrsmead', 168
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200286);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200286, 162, 27903, 33913, 12996, 0, 168
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200286 AND `ZoneId`=162 AND `WorldX`=27903 AND `WorldY`=33913);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200286, 162, 27903, 33907, 12959, 0, 168
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200286 AND `ZoneId`=162 AND `WorldX`=27903 AND `WorldY`=33907);

-- 200287  Squire's Secret Still
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200287, 'Squire\'s Secret Still', 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200287);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200287, 162, 17495, 50078, 14091, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200287 AND `ZoneId`=162 AND `WorldX`=17495 AND `WorldY`=50078);

-- 200288  Fatal Firebreath Fuel
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200288, 'Fatal Firebreath Fuel', 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200288);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200288, 162, 32566, 33626, 12414, 0, 52
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200288 AND `ZoneId`=162 AND `WorldX`=32566 AND `WorldY`=33626);

-- 200290  Feast Hall Hooch
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200290, 'Feast Hall Hooch', 981
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200290);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200290, 161, 34705, 43678, 17114, 0, 981
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200290 AND `ZoneId`=161 AND `WorldX`=34705 AND `WorldY`=43678);

-- 200291  Choke & Boil Brew
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200291, 'Choke & Boil Brew', 981
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200291);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200291, 161, 28383, 35353, 16796, 0, 981
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200291 AND `ZoneId`=161 AND `WorldX`=28383 AND `WorldY`=35353);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200291, 161, 28404, 35486, 16769, 0, 981
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200291 AND `ZoneId`=161 AND `WorldX`=28404 AND `WorldY`=35486);

-- 200292  Boneyard Brew
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200292, 'Boneyard Brew', 981
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200292);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200292, 161, 19908, 39193, 17011, 0, 981
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200292 AND `ZoneId`=161 AND `WorldX`=19908 AND `WorldY`=39193);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200292, 161, 19912, 39193, 17010, 0, 981
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200292 AND `ZoneId`=161 AND `WorldX`=19912 AND `WorldY`=39193);

-- 200294  Dread Wine
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200294, 'Dread Wine', 168
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200294);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200294, 161, 23789, 44948, 17169, 0, 168
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200294 AND `ZoneId`=161 AND `WorldX`=23789 AND `WorldY`=44948);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 200294, 161, 23778, 44951, 17183, 0, 168
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=200294 AND `ZoneId`=161 AND `WorldX`=23778 AND `WorldY`=44951);

-- 333334  Drazsh Book of Grudges
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 333334, 'Drazsh Book of Grudges', 172
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=333334);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 333334, 3, 53799, 44305, 9072, 0, 172
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=333334 AND `ZoneId`=3 AND `WorldX`=53799 AND `WorldY`=44305);

-- 2000361  Koppel's Bedroll
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000361, 'Koppel\'s Bedroll', 58
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000361);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000361, 162, 25667, 54411, 13743, 0, 58
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000361 AND `ZoneId`=162 AND `WorldX`=25667 AND `WorldY`=54411);

-- 2000362  Koppel's Mug
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000362, 'Koppel\'s Mug', 141
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000362);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000362, 162, 30345, 28615, 12491, 0, 141
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000362 AND `ZoneId`=162 AND `WorldX`=30345 AND `WorldY`=28615);

-- 2000391  Box of Recovered Cogs
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000391, 'Box of Recovered Cogs', 1434
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000391);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000391, 9, 62847, 54435, 11003, 0, 1434
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000391 AND `ZoneId`=9 AND `WorldX`=62847 AND `WorldY`=54435);

-- 2000403  Nordland XI Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000403, 'Nordland XI Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000403);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000403, 106, 26392, 5236, 3773, 0, 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000403 AND `ZoneId`=106 AND `WorldX`=26392 AND `WorldY`=5236);

-- 2000404  Festenplatz Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000404, 'Festenplatz Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000404);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000404, 106, 35725, 10579, 4210, 0, 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000404 AND `ZoneId`=106 AND `WorldX`=35725 AND `WorldY`=10579);

-- 2000405  Harvest Shrine Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000405, 'Harvest Shrine Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000405);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000405, 106, 40230, 13981, 5150, 0, 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000405 AND `ZoneId`=106 AND `WorldX`=40230 AND `WorldY`=13981);

-- 2000406  Cannon Battery Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000406, 'Cannon Battery Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000406);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000406, 6, 53832, 11482, 9699, 0, 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000406 AND `ZoneId`=6 AND `WorldX`=53832 AND `WorldY`=11482);

-- 2000407  Stonemine Tower Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000407, 'Stonemine Tower Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000407);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000407, 6, 49928, 4999, 10335, 0, 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000407 AND `ZoneId`=6 AND `WorldX`=49928 AND `WorldY`=4999);

-- 2000408  Ironmane Outpost Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000408, 'Ironmane Outpost Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000408);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000408, 11, 18477, 6455, 7505, 0, 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000408 AND `ZoneId`=11 AND `WorldX`=18477 AND `WorldY`=6455);

-- 2000409  Lookout Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000409, 'Lookout Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000409);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000409, 11, 15937, 14843, 8505, 0, 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000409 AND `ZoneId`=11 AND `WorldX`=15937 AND `WorldY`=14843);

-- 2000417  Unshackled Host Crate
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000417, 'Unshackled Host Crate', 307
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000417);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000417, 100, 23008, 63555, 3331, 0, 307
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000417 AND `ZoneId`=100 AND `WorldX`=23008 AND `WorldY`=63555);

-- 2000418  Skaldbjorn Crate
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000418, 'Skaldbjorn Crate', 1747
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000418);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000418, 100, 36556, 51900, 3511, 0, 1747
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000418 AND `ZoneId`=100 AND `WorldX`=36556 AND `WorldY`=51900);

-- 2000419  Breuer's Regiment Crate
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000419, 'Breuer\'s Regiment Crate', 174
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000419);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000419, 106, 20294, 7144, 3349, 0, 174
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000419 AND `ZoneId`=106 AND `WorldX`=20294 AND `WorldY`=7144);

-- 2000420  Nemesis Landing Crate
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000420, 'Nemesis Landing Crate', 1657
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000420);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000420, 200, 33189, 48296, 4620, 0, 1657
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000420 AND `ZoneId`=200 AND `WorldX`=33189 AND `WorldY`=48296);

-- 2000421  Poisonblade Crate
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000421, 'Poisonblade Crate', 1657
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000421);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000421, 206, 41653, 1995, 7880, 0, 1657
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000421 AND `ZoneId`=206 AND `WorldX`=41653 AND `WorldY`=1995);

-- 2000422  Adunei Crate
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000422, 'Adunei Crate', 1520
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000422);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000422, 206, 27406, 1955, 7408, 0, 1520
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000422 AND `ZoneId`=206 AND `WorldX`=27406 AND `WorldY`=1955);

-- 2000423  Grimmenhagen Crate
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000423, 'Grimmenhagen Crate', 10
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000423);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000423, 106, 37453, 13468, 5413, 0, 10
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000423 AND `ZoneId`=106 AND `WorldX`=37453 AND `WorldY`=13468);

-- 2000425  Moonrise Tower Crate
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000425, 'Moonrise Tower Crate', 1520
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000425);
INSERT INTO `gameobject_spawns` (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`DisplayID`)
  SELECT 2000425, 206, 35031, 11625, 6989, 0, 1520
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_spawns` WHERE `Entry`=2000425 AND `ZoneId`=206 AND `WorldX`=35031 AND `WorldY`=11625);

-- Quests unblocked by this script:
--     8002  Bright Wizard College Opens
--    10100  Bloodgash Ogre
--    11015  An Oath to Uphold
--    15006  A Golden Discovery
--    20734  The Vessel of Azorgaron
--    24114  Explosive Solution
--    24120  The Hand of Gork
--    24203  Dragon Smashin'
--    29734  Stunties is Fer Scrappin'
--    30077  Misled
--    30107  Last Stand
--    30135  Ulric's Blessing
--    30150  Sigmarite Relic
--    30943  Desperate Gambit
--    31017  Grip of Darkness
--    35107  Unnatural Ability
--    35607  The Gathering Storm
--    35623  Legacy of the Drakecaller
--    35901  Birthright
--    35917  Champion of Ulthuan
--    35929  Awakening
--    36016  Precious Cargo
--    36045  Bonds of Fellowship - Deadly Enemy
--    40112  Bread Basket
--    40130  Bad Blood
--    40332  Man-cicle
--    40509  Overburdened
--    40513  Grimclan Trail
--    41300  Dogs of War
--    41301  Dogs of War
--    44616  The Scrying Game
--    45210  The Queen Mother's Blade
--    49036  The Grudges of Karak Drazh!
--    60045  The Undead March
--    60057  Bones That Be
--    60059  Grave Powers
--    60061  Night of the Living Dead
--    60076  A Power Rises
--    60084  Darkness Encroaches
--    60085  Darkness Encroaches
--    60086  Darkness Encroaches
--    60093  Catacomb Cleansing
--    60095  The Fall of Unterbaum
--    60105  Death and Decay
--    60106  Death and Decay
--    60140  Pub Crawl
--    60141  Better Ale 'ere?
--    60146  Pub Crawlers
--    60200  Thunderbolts and Lightning...
--    60204  Orders From your Emperor
--    60205  Orders From the High King
--    60207  Weapons For Ludwig
--    60209  Weapons For Alarielle
--    60213  Orders From Your Lord
--    60214  Orders From Your Warboss
--    60216  Weapons For Arbaal
--    60218  Weapons For Shadowblade

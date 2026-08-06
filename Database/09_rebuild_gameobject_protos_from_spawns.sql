-- Rebuild gameobject protos from spawn rows already present. WORLD database.
--
-- Follow-up to 08_restore_quest_gameobjects.sql, which sourced missing
-- gameobjects from Return of Reckoning's API. This one needs no external source
-- at all.
--
-- gameobject_spawns carries its own DisplayID, and 334 entries in this database
-- have spawn rows with no matching proto. For those the display id and the world
-- placement are both already here - only the proto row is missing, and that is
-- exactly the row a quest objective check looks for. The objects are placed in
-- the world but have no prototype, so nothing can be credited against them.
--
--   quests still blocked before this script : 164
--   entries recoverable from local spawns   : 334
--   ... needed by a blocked quest           : 39
--   restored here (complete a whole quest)  : 33
--   quests unblocked by this script         : 22
--
-- DisplayID is taken from the spawn rows themselves, so it is real rather than
-- inferred. Names come from the RoR API where it has them; a proto Name is NOT
-- NULL and a readable name beats a placeholder. Entries that would only partly
-- unblock a quest are not inserted.
--
-- Re-runnable: every insert is guarded.

-- 9959  Stone of Imrathir
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 9959, 'Stone of Imrathir', 1511
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=9959);

-- 9960  Stone of Melanar
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 9960, 'Stone of Melanar', 1511
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=9960);

-- 20000  Bright Wizard College Map Piece I
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 20000, 'Bright Wizard College Map Piece I', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=20000);

-- 131402  Gameobject 131402
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 131402, 'Gameobject 131402', 65535
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=131402);

-- 200141  Altar of the Bloodborne
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200141, 'Altar of the Bloodborne', 8601
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200141);

-- 200147  Nordlander Grave
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200147, 'Nordlander Grave', 213
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200147);

-- 200148  Knight's Crypt
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200148, 'Knight\'s Crypt', 5364
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200148);

-- 200149  Wight Grave
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200149, 'Wight Grave', 1686
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200149);

-- 200151  Withered Grave
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200151, 'Withered Grave', 1686
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200151);

-- 200152  Grave spades
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200152, 'Grave spades', 1687
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200152);

-- 200164  Warpstone Meteorite
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200164, 'Warpstone Meteorite', 347
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200164);

-- 200169  Old Coffin
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200169, 'Old Coffin', 230
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200169);

-- 200170  Old Brazier
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200170, 'Old Brazier', 298
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200170);

-- 200171  Old Statue
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200171, 'Old Statue', 4127
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200171);

-- 200200  Black Book of Arkhan
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200200, 'Black Book of Arkhan', 7615
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200200);

-- 200204  Tree of Rot
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200204, 'Tree of Rot', 217
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200204);

-- 200293  Monolith Malt
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200293, 'Monolith Malt', 7199
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200293);

-- 200295  Lover's Liqueur
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200295, 'Lover\'s Liqueur', 913
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200295);

-- 200297  Slaughterstout
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200297, 'Slaughterstout', 923
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200297);

-- 200298  Fleshrot Rotgut
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 200298, 'Fleshrot Rotgut', 926
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=200298);

-- 2000298  Party Kegs
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000298, 'Party Kegs', 5404
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000298);

-- 2000354  Dwarf Door Knocker
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000354, 'Dwarf Door Knocker', 7400
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000354);

-- 2000385  The One To Rule Them All
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000385, 'The One To Rule Them All', 14
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000385);

-- 2000388  Necromancer's Wellspring
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000388, 'Necromancer\'s Wellspring', 1583
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000388);

-- 2000395  Thane's Defense
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000395, 'Thane\'s Defense', 343
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000395);

-- 2000400  Forgotten Scroll
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000400, 'Forgotten Scroll', 1498
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000400);

-- 2000401  Forgotten Talisman
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000401, 'Forgotten Talisman', 6761
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000401);

-- 2000410  Altar Of Khaine Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000410, 'Altar Of Khaine Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000410);

-- 2000411  House Of Lorendith Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000411, 'House Of Lorendith Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000411);

-- 2000412  Shard Of Grief Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000412, 'Shard Of Grief Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000412);

-- 2000413  Tower Of Nightflame Map Piece
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000413, 'Tower Of Nightflame Map Piece', 267
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000413);

-- 2000470  Gameobject 2000470
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000470, 'Gameobject 2000470', 1759
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000470);

-- 2000498  Gameobject 2000498
INSERT INTO `gameobject_protos` (`Entry`,`Name`,`DisplayID`)
  SELECT 2000498, 'Gameobject 2000498', 869
  WHERE NOT EXISTS (SELECT 1 FROM `gameobject_protos` WHERE `Entry`=2000498);

-- Quests unblocked by this script:
--        0  Aqshy Sprite
--     8001  Bright Wizard College Opens
--    20800  Squigs Over
--    30001  Grimmenhagen Burning
--    34306  Balance
--    42843  A Lords' Game
--    49404  Far From Home
--    60040  A Host Divided
--    60051  Spirits of Old
--    60058  Ancient Spirits
--    60060  Night of the Living Dead
--    60074  A Power Rises
--    60082  Catacomb Corruption
--    60107  Death and Decay: Worthy Fate
--    60133  Cursed Lore
--    60147  Pleasin' Da Chaos Gits
--    60159  Finish With a Bang
--    60167  Yer back? 'An Alive Too? Erm...
--    60198  Morrslieb's Return
--    60199  Keg Crawl Across The Globe
--    60206  Orders From the Phoenix King
--    60215  Orders From the Witch King

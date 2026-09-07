-- 57_tome_tactic_line_creature_types.sql
--
-- BUG-116 (continued): which creature types each tome tactic line acts against.
--
-- Source: the monster-type operands in the client's own ability component data. Every tome tactic
-- component is gated by AbilityOperation 16 (MonsterType, per the WAR-RE-Toolkit
-- abilities-bin-file-exporter AbilityEnums.cs), and mythic_bin_ability.ComponentDataValues carries
-- the operand. Decoded codes, cross-read against the tactic descriptions in abilitydesc.txt:
--
--   1000 Daemons        2000/2620 Undead     3000 Animals      4100 Humans
--   4200 Dwarfs         4300 Elves           4400 Greenskins   4600 Ogres
--   4710 Skaven         4900 Beastmen        6100 Chaos Breeds 6200 Magical Beasts
--   6300 Dragonoids     6500 Trolls+Giants   7100 Forest Spirits
--
-- Two of those codes were confirmed independently against unrelated abilities that use the same
-- operand: Invocation of Nehek (4095) gates on 2000, and Burning Gaze (4063) on 2000 or 1000 --
-- both anti-undead/anti-daemon abilities.
--
-- Cross-checked empirically: grouping this database's bestiary fragments by the CreatureType of
-- the species that grant them reproduces these sets, exactly for Greenskin (15) and Skaven (18)
-- and with only ambiguous-subtype noise elsewhere. That check is why Dark Elves (12) are included
-- in the Man line: the client has a single "Elves" monster type (4300) where this schema splits
-- Elves (14) from Dark Elves (12).
--
-- Re-runnable: table dropped and rebuilt.

START TRANSACTION;

DROP TABLE IF EXISTS `tome_tactic_line_creature_types`;

CREATE TABLE `tome_tactic_line_creature_types` (
  `AcId`         SMALLINT UNSIGNED NOT NULL COMMENT 'tome_tactic_lines.AcId',
  `CreatureType` TINYINT UNSIGNED  NOT NULL COMMENT 'GameData.CreatureTypes',
  PRIMARY KEY (`AcId`, `CreatureType`),
  KEY `idx_creature_type` (`CreatureType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 330 Daemonic: DAEMONS_KHORNE/NURGLE/SLAANESH/TZEENTCH/UNMARKED
INSERT INTO `tome_tactic_line_creature_types` VALUES (330,6),(330,7),(330,8),(330,9),(330,10);
-- 331 Beastial: ANIMALS_BEASTS/CRITTER/INSECTS_ARACHNIDS/LIVESTOCK/REPTILES/BIRDS
INSERT INTO `tome_tactic_line_creature_types` VALUES (331,1),(331,2),(331,3),(331,4),(331,5),(331,31);
-- 332 Giant: HUMANOIDS_OGRES, MONSTERS_GIANTS, MONSTERS_TROLLS
INSERT INTO `tome_tactic_line_creature_types` VALUES (332,17),(332,21),(332,23);
-- 333 Greenskin: HUMANOIDS_GREENSKINS
INSERT INTO `tome_tactic_line_creature_types` VALUES (333,15);
-- 334 Chaos: HUMANOIDS_BEASTMEN, MONSTERS_CHAOS_BREEDS
INSERT INTO `tome_tactic_line_creature_types` VALUES (334,11),(334,19);
-- 335 Mythical: MONSTERS_DRAGONOIDS, MAGICAL_BEASTS, PLANTS_FOREST_SPIRITS
INSERT INTO `tome_tactic_line_creature_types` VALUES (335,20),(335,22),(335,24);
-- 336 Man: HUMANOIDS_DARK_ELVES, DWARFS, ELVES, HUMANS
INSERT INTO `tome_tactic_line_creature_types` VALUES (336,12),(336,13),(336,14),(336,16);
-- 337 Skaven: HUMANOIDS_SKAVEN
INSERT INTO `tome_tactic_line_creature_types` VALUES (337,18);
-- 338 Undead: UNDEAD_CONSTRUCTS/GREATER/SKELETONS/SPIRITS/WIGHTS/ZOMBIES
INSERT INTO `tome_tactic_line_creature_types` VALUES (338,25),(338,26),(338,27),(338,28),(338,29),(338,30);

COMMIT;

-- Verification:
--   SELECT COUNT(*) FROM tome_tactic_line_creature_types;              -- 31
--   -- no creature type may belong to two lines, or a tactic would match ambiguously:
--   SELECT CreatureType FROM tome_tactic_line_creature_types GROUP BY CreatureType HAVING COUNT(*) > 1;  -- 0 rows
--   -- every line must have at least one type:
--   SELECT l.AcId FROM tome_tactic_lines l LEFT JOIN tome_tactic_line_creature_types t ON t.AcId=l.AcId
--     WHERE t.AcId IS NULL;                                            -- 0 rows

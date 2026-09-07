-- 55_tome_tactic_lines_and_fragments.sql
--
-- BUG-116: tome tactics were entirely unimplemented. The 27 tactic abilities exist in
-- `abilities` as Category 16 rows (15100-15126, name and icon only) and their Tome unlock rows
-- exist in `tok_infos` Section 26 (6200-6226, Index 1-27, Flag = tier), but nothing bound the
-- two together and nothing tracked the fragments that gate them.
--
-- Every value here is read directly from the 1.4.8 client, not reconstructed:
--   * interface/interfacecore/tome/tactics/acid_entries.csv
--       "ACID ID, String ID, Thresh. 1, Reward 1, Thresh. 2, Reward 2, Thresh. 3, Reward 3"
--       -> the nine action counter ids (330-338), one per tactic line, and each line's three
--          fragment thresholds with the tactic entry each unlocks.
--   * interface/interfacecore/tome/tactics/tactic_entries.csv
--       -> tactic entry 1-27 maps to ability 15100-15126 in order, so Ability = 15099 + entry
--          and the matching Tome unlock is TokEntry = 6199 + entry.
--   * interface/interfacecore/tome/bestiary/species.csv
--       -> per species, ten (reward type, reward id) slots. Reward type 6 is
--          TOME_REWARD_ABILITY_COUNTER (GameData.Tome), and its reward id is the line's ACID.
--          Slot n of a species is Tome entry (its tok_bestiary.Kill1 + n - 1); verified against
--          Snotling, whose slot 8 title id 10726 matches its Kill10000 cell "4067;10726".
--
-- Cross-checked in game: the client's Greenskin fragment tooltip reads "Total Greenskin Tactic
-- Fragments unlocked: 0/5" with Outmaneuver the Dim / the Cunning / the Clever requiring 2 / 3 / 5,
-- which is exactly ACID 333 below and exactly the five fragment rows this script inserts for it.
--
-- Known gap: the client grants 140 fragments across 146 species, but two of them sit on client
-- species 46 ("Dwarven, Slayer"), which has no tok_bestiary row in this database, so 138 are
-- bound here. Both are Man-line (ACID 336) fragments, leaving that line 6 of its 8 fragments.
-- Its thresholds are 2/4/6, so tier 3 remains exactly reachable, with no slack. Tracked as
-- BUG-117; adding the missing species is a separate change.
--
-- Re-runnable: both tables are dropped and rebuilt.

START TRANSACTION;

DROP TABLE IF EXISTS `tome_tactic_fragments`;
DROP TABLE IF EXISTS `tome_tactic_lines`;

CREATE TABLE `tome_tactic_lines` (
  `AcId`        SMALLINT UNSIGNED NOT NULL COMMENT 'Client action counter id for this line',
  `LineIndex`   TINYINT UNSIGNED  NOT NULL COMMENT 'String ID in tome/tactics/tactic_ability_names.txt',
  `Name`        VARCHAR(32)       NOT NULL,
  `Threshold1`  SMALLINT UNSIGNED NOT NULL,
  `Tactic1`     SMALLINT UNSIGNED NOT NULL COMMENT 'abilities.Entry',
  `TokEntry1`   SMALLINT UNSIGNED NOT NULL COMMENT 'tok_infos.Entry, Section 26',
  `Threshold2`  SMALLINT UNSIGNED NOT NULL,
  `Tactic2`     SMALLINT UNSIGNED NOT NULL,
  `TokEntry2`   SMALLINT UNSIGNED NOT NULL,
  `Threshold3`  SMALLINT UNSIGNED NOT NULL,
  `Tactic3`     SMALLINT UNSIGNED NOT NULL,
  `TokEntry3`   SMALLINT UNSIGNED NOT NULL,
  PRIMARY KEY (`AcId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE `tome_tactic_fragments` (
  `TokEntry`    SMALLINT UNSIGNED NOT NULL COMMENT 'Bestiary tok_infos.Entry that grants the fragment',
  `AcId`        SMALLINT UNSIGNED NOT NULL COMMENT 'Tactic line counter it advances',
  PRIMARY KEY (`TokEntry`),
  KEY `idx_acid` (`AcId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT INTO `tome_tactic_lines` VALUES (330,1,'Daemonic',7,15100,6200,15,15101,6201,22,15102,6202);
INSERT INTO `tome_tactic_lines` VALUES (331,2,'Beastial',10,15103,6203,20,15104,6204,30,15105,6205);
INSERT INTO `tome_tactic_lines` VALUES (332,3,'Giant',5,15106,6206,10,15107,6207,15,15108,6208);
INSERT INTO `tome_tactic_lines` VALUES (333,4,'Greenskin',2,15109,6209,3,15110,6210,5,15111,6211);
INSERT INTO `tome_tactic_lines` VALUES (334,5,'Chaos',5,15112,6212,10,15113,6213,15,15114,6214);
INSERT INTO `tome_tactic_lines` VALUES (335,6,'Mythical',3,15115,6215,7,15116,6216,10,15117,6217);
INSERT INTO `tome_tactic_lines` VALUES (336,7,'Man',2,15118,6218,4,15119,6219,6,15120,6220);
INSERT INTO `tome_tactic_lines` VALUES (337,8,'Skaven',1,15121,6221,2,15122,6222,3,15123,6223);
INSERT INTO `tome_tactic_lines` VALUES (338,9,'Undead',4,15124,6224,8,15125,6225,12,15126,6226);

INSERT INTO `tome_tactic_fragments` VALUES (3004,331);
INSERT INTO `tome_tactic_fragments` VALUES (3006,331);
INSERT INTO `tome_tactic_fragments` VALUES (3014,331);
INSERT INTO `tome_tactic_fragments` VALUES (3016,331);
INSERT INTO `tome_tactic_fragments` VALUES (3026,331);
INSERT INTO `tome_tactic_fragments` VALUES (3028,331);
INSERT INTO `tome_tactic_fragments` VALUES (3036,334);
INSERT INTO `tome_tactic_fragments` VALUES (3038,334);
INSERT INTO `tome_tactic_fragments` VALUES (3045,334);
INSERT INTO `tome_tactic_fragments` VALUES (3054,334);
INSERT INTO `tome_tactic_fragments` VALUES (3056,334);
INSERT INTO `tome_tactic_fragments` VALUES (3064,334);
INSERT INTO `tome_tactic_fragments` VALUES (3066,334);
INSERT INTO `tome_tactic_fragments` VALUES (3075,330);
INSERT INTO `tome_tactic_fragments` VALUES (3084,330);
INSERT INTO `tome_tactic_fragments` VALUES (3094,331);
INSERT INTO `tome_tactic_fragments` VALUES (3096,331);
INSERT INTO `tome_tactic_fragments` VALUES (3104,335);
INSERT INTO `tome_tactic_fragments` VALUES (3175,330);
INSERT INTO `tome_tactic_fragments` VALUES (3185,331);
INSERT INTO `tome_tactic_fragments` VALUES (3195,334);
INSERT INTO `tome_tactic_fragments` VALUES (3205,330);
INSERT INTO `tome_tactic_fragments` VALUES (3215,330);
INSERT INTO `tome_tactic_fragments` VALUES (3234,330);
INSERT INTO `tome_tactic_fragments` VALUES (3244,330);
INSERT INTO `tome_tactic_fragments` VALUES (3255,335);
INSERT INTO `tome_tactic_fragments` VALUES (3265,331);
INSERT INTO `tome_tactic_fragments` VALUES (3285,330);
INSERT INTO `tome_tactic_fragments` VALUES (3344,334);
INSERT INTO `tome_tactic_fragments` VALUES (3374,334);
INSERT INTO `tome_tactic_fragments` VALUES (3396,335);
INSERT INTO `tome_tactic_fragments` VALUES (3398,335);
INSERT INTO `tome_tactic_fragments` VALUES (3514,330);
INSERT INTO `tome_tactic_fragments` VALUES (3524,334);
INSERT INTO `tome_tactic_fragments` VALUES (3535,330);
INSERT INTO `tome_tactic_fragments` VALUES (3544,338);
INSERT INTO `tome_tactic_fragments` VALUES (3556,338);
INSERT INTO `tome_tactic_fragments` VALUES (3558,338);
INSERT INTO `tome_tactic_fragments` VALUES (3565,338);
INSERT INTO `tome_tactic_fragments` VALUES (3574,331);
INSERT INTO `tome_tactic_fragments` VALUES (3576,331);
INSERT INTO `tome_tactic_fragments` VALUES (3586,332);
INSERT INTO `tome_tactic_fragments` VALUES (3588,332);
INSERT INTO `tome_tactic_fragments` VALUES (3594,332);
INSERT INTO `tome_tactic_fragments` VALUES (3604,333);
INSERT INTO `tome_tactic_fragments` VALUES (3606,333);
INSERT INTO `tome_tactic_fragments` VALUES (3645,336);
INSERT INTO `tome_tactic_fragments` VALUES (3655,332);
INSERT INTO `tome_tactic_fragments` VALUES (3674,331);
INSERT INTO `tome_tactic_fragments` VALUES (3676,331);
INSERT INTO `tome_tactic_fragments` VALUES (3685,331);
INSERT INTO `tome_tactic_fragments` VALUES (3704,330);
INSERT INTO `tome_tactic_fragments` VALUES (3714,332);
INSERT INTO `tome_tactic_fragments` VALUES (3724,334);
INSERT INTO `tome_tactic_fragments` VALUES (3726,334);
INSERT INTO `tome_tactic_fragments` VALUES (3786,330);
INSERT INTO `tome_tactic_fragments` VALUES (3788,330);
INSERT INTO `tome_tactic_fragments` VALUES (3796,331);
INSERT INTO `tome_tactic_fragments` VALUES (3798,331);
INSERT INTO `tome_tactic_fragments` VALUES (3804,330);
INSERT INTO `tome_tactic_fragments` VALUES (3814,330);
INSERT INTO `tome_tactic_fragments` VALUES (3825,338);
INSERT INTO `tome_tactic_fragments` VALUES (3834,331);
INSERT INTO `tome_tactic_fragments` VALUES (3836,331);
INSERT INTO `tome_tactic_fragments` VALUES (3844,330);
INSERT INTO `tome_tactic_fragments` VALUES (3855,335);
INSERT INTO `tome_tactic_fragments` VALUES (3876,330);
INSERT INTO `tome_tactic_fragments` VALUES (3878,330);
INSERT INTO `tome_tactic_fragments` VALUES (3884,332);
INSERT INTO `tome_tactic_fragments` VALUES (3886,332);
INSERT INTO `tome_tactic_fragments` VALUES (3894,332);
INSERT INTO `tome_tactic_fragments` VALUES (3935,333);
INSERT INTO `tome_tactic_fragments` VALUES (3946,336);
INSERT INTO `tome_tactic_fragments` VALUES (3948,336);
INSERT INTO `tome_tactic_fragments` VALUES (3964,334);
INSERT INTO `tome_tactic_fragments` VALUES (3966,334);
INSERT INTO `tome_tactic_fragments` VALUES (3974,330);
INSERT INTO `tome_tactic_fragments` VALUES (3976,330);
INSERT INTO `tome_tactic_fragments` VALUES (3994,337);
INSERT INTO `tome_tactic_fragments` VALUES (4014,331);
INSERT INTO `tome_tactic_fragments` VALUES (4024,331);
INSERT INTO `tome_tactic_fragments` VALUES (4026,331);
INSERT INTO `tome_tactic_fragments` VALUES (4035,330);
INSERT INTO `tome_tactic_fragments` VALUES (4044,337);
INSERT INTO `tome_tactic_fragments` VALUES (4048,337);
INSERT INTO `tome_tactic_fragments` VALUES (4054,338);
INSERT INTO `tome_tactic_fragments` VALUES (4056,338);
INSERT INTO `tome_tactic_fragments` VALUES (4066,333);
INSERT INTO `tome_tactic_fragments` VALUES (4068,333);
INSERT INTO `tome_tactic_fragments` VALUES (4074,331);
INSERT INTO `tome_tactic_fragments` VALUES (4076,331);
INSERT INTO `tome_tactic_fragments` VALUES (4086,335);
INSERT INTO `tome_tactic_fragments` VALUES (4088,335);
INSERT INTO `tome_tactic_fragments` VALUES (4096,331);
INSERT INTO `tome_tactic_fragments` VALUES (4097,331);
INSERT INTO `tome_tactic_fragments` VALUES (4105,335);
INSERT INTO `tome_tactic_fragments` VALUES (4116,332);
INSERT INTO `tome_tactic_fragments` VALUES (4118,332);
INSERT INTO `tome_tactic_fragments` VALUES (4125,332);
INSERT INTO `tome_tactic_fragments` VALUES (4136,332);
INSERT INTO `tome_tactic_fragments` VALUES (4138,332);
INSERT INTO `tome_tactic_fragments` VALUES (4144,332);
INSERT INTO `tome_tactic_fragments` VALUES (4146,332);
INSERT INTO `tome_tactic_fragments` VALUES (4166,334);
INSERT INTO `tome_tactic_fragments` VALUES (4168,334);
INSERT INTO `tome_tactic_fragments` VALUES (4174,338);
INSERT INTO `tome_tactic_fragments` VALUES (4184,335);
INSERT INTO `tome_tactic_fragments` VALUES (4194,338);
INSERT INTO `tome_tactic_fragments` VALUES (4206,331);
INSERT INTO `tome_tactic_fragments` VALUES (4208,331);
INSERT INTO `tome_tactic_fragments` VALUES (4214,331);
INSERT INTO `tome_tactic_fragments` VALUES (4218,331);
INSERT INTO `tome_tactic_fragments` VALUES (4225,338);
INSERT INTO `tome_tactic_fragments` VALUES (4234,338);
INSERT INTO `tome_tactic_fragments` VALUES (4246,331);
INSERT INTO `tome_tactic_fragments` VALUES (4248,331);
INSERT INTO `tome_tactic_fragments` VALUES (4255,335);
INSERT INTO `tome_tactic_fragments` VALUES (4265,332);
INSERT INTO `tome_tactic_fragments` VALUES (4276,338);
INSERT INTO `tome_tactic_fragments` VALUES (4278,338);
INSERT INTO `tome_tactic_fragments` VALUES (4286,336);
INSERT INTO `tome_tactic_fragments` VALUES (4288,336);
INSERT INTO `tome_tactic_fragments` VALUES (4295,330);
INSERT INTO `tome_tactic_fragments` VALUES (4304,335);
INSERT INTO `tome_tactic_fragments` VALUES (4314,330);
INSERT INTO `tome_tactic_fragments` VALUES (4324,338);
INSERT INTO `tome_tactic_fragments` VALUES (4334,330);
INSERT INTO `tome_tactic_fragments` VALUES (4344,334);
INSERT INTO `tome_tactic_fragments` VALUES (4354,336);
INSERT INTO `tome_tactic_fragments` VALUES (4364,335);
INSERT INTO `tome_tactic_fragments` VALUES (4402,338);
INSERT INTO `tome_tactic_fragments` VALUES (4407,338);
INSERT INTO `tome_tactic_fragments` VALUES (4412,338);
INSERT INTO `tome_tactic_fragments` VALUES (4417,338);
INSERT INTO `tome_tactic_fragments` VALUES (4422,338);
INSERT INTO `tome_tactic_fragments` VALUES (4427,338);
INSERT INTO `tome_tactic_fragments` VALUES (4432,338);
INSERT INTO `tome_tactic_fragments` VALUES (4437,338);

COMMIT;

-- Verification:
--   SELECT COUNT(*) FROM tome_tactic_lines;                    -- 9
--   SELECT COUNT(*) FROM tome_tactic_fragments;                -- 138
--   SELECT AcId, COUNT(*) FROM tome_tactic_fragments GROUP BY AcId ORDER BY AcId;
--     -- 330:24 331:32 332:16 333:5 334:18 335:12 336:6 337:3 338:22
--   -- every fragment threshold must be reachable from the fragments actually bound:
--   SELECT l.Name, l.Threshold3, COUNT(f.TokEntry) available
--     FROM tome_tactic_lines l LEFT JOIN tome_tactic_fragments f ON f.AcId = l.AcId
--     GROUP BY l.AcId HAVING available < l.Threshold3;         -- 0 rows

-- 54_populate_bestiary_action_counters.sql
--
-- BUG-115: every bestiary kill counter was dead because tok_bestiary.Bestiary_ID was NULL on
-- all 137 rows. Tok_Bestiary.Bestiary_ID is a non-nullable ushort, so the ORM read every row
-- back as 0 and TokInterface.AddKill then:
--   * funnelled all 137 species into shared kill bucket 0, so the client's per-species
--     counter never moved and every bestiary page displayed "Kills: 0";
--   * tested its Kill1/25/100/1000/10000/100000 milestone ladder against that shared global
--     count, so the 25th kill of anything awarded Kill25 for whichever species died 25th and
--     no other species ever received it. Past 100,000 total kills no bestiary tok fired at all;
--   * wrote characters_toks_kills.NPCEntry as 0, collapsing the whole bestiary to one row.
--
-- The correct values are the client's per-species action counter ids, from
-- interface/interfacecore/tome/bestiary/species.csv column "AC #". AddKill already pre-seeds
-- counter 495 as the total-kills counter, which is in that same id space and confirms the
-- column was always meant to hold client counter ids rather than the subtype.
--
-- IMPORTANT: tok_bestiary.Creature_Sub_Type is NOT the client's species id. They are separate
-- id spaces that coincide only at Basilisk = 1. Subtype 2 is "Bear" while client species 2 is
-- "Bat, Giant"; subtype 3 is "Boar" while species 3 is "Bear". Every pair below was resolved by
-- matching the species name (tok_infos name of the row's Kill1 entry) against the client's
-- data/strings/english/tome/bestiary/species_names.txt, then reading "AC #" from species.csv,
-- and each mapping was confirmed against that species' portrait filename.
--
-- 136 of 137 rows map. Subtype 68 "Hammerer" is deliberately left at 0: the 1.4.8 client has no
-- bestiary species for it at all (no species_names.txt entry and no portrait), so it has no
-- counter to bind. AddKill skips zero ids after this change rather than writing to counter 0.
--
-- Re-runnable: plain UPDATEs keyed by primary key.

START TRANSACTION;

UPDATE `tok_bestiary` SET `Bestiary_ID` = 1 WHERE `Creature_Sub_Type` = 1;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 3 WHERE `Creature_Sub_Type` = 2;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 10 WHERE `Creature_Sub_Type` = 3;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 2 WHERE `Creature_Sub_Type` = 4;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 68 WHERE `Creature_Sub_Type` = 5;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 80 WHERE `Creature_Sub_Type` = 6;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 102 WHERE `Creature_Sub_Type` = 7;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 125 WHERE `Creature_Sub_Type` = 8;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 625 WHERE `Creature_Sub_Type` = 18;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 103 WHERE `Creature_Sub_Type` = 19;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 108 WHERE `Creature_Sub_Type` = 20;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 626 WHERE `Creature_Sub_Type` = 21;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 27 WHERE `Creature_Sub_Type` = 29;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 84 WHERE `Creature_Sub_Type` = 30;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 22 WHERE `Creature_Sub_Type` = 31;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 8 WHERE `Creature_Sub_Type` = 32;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 9 WHERE `Creature_Sub_Type` = 33;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 54 WHERE `Creature_Sub_Type` = 34;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 81 WHERE `Creature_Sub_Type` = 35;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 71 WHERE `Creature_Sub_Type` = 36;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 88 WHERE `Creature_Sub_Type` = 37;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 98 WHERE `Creature_Sub_Type` = 38;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 25 WHERE `Creature_Sub_Type` = 39;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 134 WHERE `Creature_Sub_Type` = 40;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 29 WHERE `Creature_Sub_Type` = 41;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 23 WHERE `Creature_Sub_Type` = 42;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 82 WHERE `Creature_Sub_Type` = 43;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 24 WHERE `Creature_Sub_Type` = 44;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 52 WHERE `Creature_Sub_Type` = 45;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 79 WHERE `Creature_Sub_Type` = 46;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 85 WHERE `Creature_Sub_Type` = 47;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 104 WHERE `Creature_Sub_Type` = 48;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 132 WHERE `Creature_Sub_Type` = 49;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 18 WHERE `Creature_Sub_Type` = 50;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 19 WHERE `Creature_Sub_Type` = 51;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 21 WHERE `Creature_Sub_Type` = 52;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 28 WHERE `Creature_Sub_Type` = 53;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 130 WHERE `Creature_Sub_Type` = 54;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 133 WHERE `Creature_Sub_Type` = 55;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 4 WHERE `Creature_Sub_Type` = 56;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 5 WHERE `Creature_Sub_Type` = 57;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 35 WHERE `Creature_Sub_Type` = 58;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 6 WHERE `Creature_Sub_Type` = 59;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 7 WHERE `Creature_Sub_Type` = 60;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 270 WHERE `Creature_Sub_Type` = 61;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 30 WHERE `Creature_Sub_Type` = 62;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 272 WHERE `Creature_Sub_Type` = 63;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 273 WHERE `Creature_Sub_Type` = 64;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 271 WHERE `Creature_Sub_Type` = 65;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 41 WHERE `Creature_Sub_Type` = 66;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 253 WHERE `Creature_Sub_Type` = 67;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 250 WHERE `Creature_Sub_Type` = 69;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 252 WHERE `Creature_Sub_Type` = 70;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 251 WHERE `Creature_Sub_Type` = 71;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 269 WHERE `Creature_Sub_Type` = 72;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 74 WHERE `Creature_Sub_Type` = 73;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 267 WHERE `Creature_Sub_Type` = 74;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 266 WHERE `Creature_Sub_Type` = 75;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 268 WHERE `Creature_Sub_Type` = 76;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 254 WHERE `Creature_Sub_Type` = 77;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 255 WHERE `Creature_Sub_Type` = 78;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 61 WHERE `Creature_Sub_Type` = 79;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 62 WHERE `Creature_Sub_Type` = 80;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 65 WHERE `Creature_Sub_Type` = 81;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 91 WHERE `Creature_Sub_Type` = 82;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 94 WHERE `Creature_Sub_Type` = 83;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 256 WHERE `Creature_Sub_Type` = 84;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 107 WHERE `Creature_Sub_Type` = 85;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 110 WHERE `Creature_Sub_Type` = 86;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 257 WHERE `Creature_Sub_Type` = 87;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 260 WHERE `Creature_Sub_Type` = 88;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 13 WHERE `Creature_Sub_Type` = 89;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 263 WHERE `Creature_Sub_Type` = 90;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 47 WHERE `Creature_Sub_Type` = 91;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 58 WHERE `Creature_Sub_Type` = 92;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 259 WHERE `Creature_Sub_Type` = 94;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 265 WHERE `Creature_Sub_Type` = 95;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 262 WHERE `Creature_Sub_Type` = 96;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 261 WHERE `Creature_Sub_Type` = 97;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 258 WHERE `Creature_Sub_Type` = 98;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 264 WHERE `Creature_Sub_Type` = 99;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 66 WHERE `Creature_Sub_Type` = 100;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 89 WHERE `Creature_Sub_Type` = 102;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 90 WHERE `Creature_Sub_Type` = 103;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 127 WHERE `Creature_Sub_Type` = 104;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 100 WHERE `Creature_Sub_Type` = 105;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 105 WHERE `Creature_Sub_Type` = 106;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 12 WHERE `Creature_Sub_Type` = 107;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 20 WHERE `Creature_Sub_Type` = 108;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 38 WHERE `Creature_Sub_Type` = 109;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 53 WHERE `Creature_Sub_Type` = 110;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 73 WHERE `Creature_Sub_Type` = 111;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 135 WHERE `Creature_Sub_Type` = 112;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 117 WHERE `Creature_Sub_Type` = 114;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 36 WHERE `Creature_Sub_Type` = 115;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 11 WHERE `Creature_Sub_Type` = 116;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 126 WHERE `Creature_Sub_Type` = 117;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 60 WHERE `Creature_Sub_Type` = 118;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 59 WHERE `Creature_Sub_Type` = 119;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 26 WHERE `Creature_Sub_Type` = 120;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 72 WHERE `Creature_Sub_Type` = 121;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 131 WHERE `Creature_Sub_Type` = 122;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 86 WHERE `Creature_Sub_Type` = 123;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 96 WHERE `Creature_Sub_Type` = 124;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 119 WHERE `Creature_Sub_Type` = 125;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 113 WHERE `Creature_Sub_Type` = 126;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 114 WHERE `Creature_Sub_Type` = 127;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 115 WHERE `Creature_Sub_Type` = 128;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 112 WHERE `Creature_Sub_Type` = 129;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 40 WHERE `Creature_Sub_Type` = 130;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 109 WHERE `Creature_Sub_Type` = 131;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 111 WHERE `Creature_Sub_Type` = 132;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 137 WHERE `Creature_Sub_Type` = 133;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 620 WHERE `Creature_Sub_Type` = 134;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 118 WHERE `Creature_Sub_Type` = 135;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 136 WHERE `Creature_Sub_Type` = 137;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 621 WHERE `Creature_Sub_Type` = 138;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 622 WHERE `Creature_Sub_Type` = 139;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 623 WHERE `Creature_Sub_Type` = 140;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 124 WHERE `Creature_Sub_Type` = 141;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 83 WHERE `Creature_Sub_Type` = 142;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 624 WHERE `Creature_Sub_Type` = 143;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 120 WHERE `Creature_Sub_Type` = 144;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 627 WHERE `Creature_Sub_Type` = 145;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 106 WHERE `Creature_Sub_Type` = 146;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 55 WHERE `Creature_Sub_Type` = 147;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 56 WHERE `Creature_Sub_Type` = 148;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 57 WHERE `Creature_Sub_Type` = 149;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 123 WHERE `Creature_Sub_Type` = 150;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 128 WHERE `Creature_Sub_Type` = 151;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 122 WHERE `Creature_Sub_Type` = 152;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 121 WHERE `Creature_Sub_Type` = 153;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 69 WHERE `Creature_Sub_Type` = 154;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 95 WHERE `Creature_Sub_Type` = 156;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 129 WHERE `Creature_Sub_Type` = 157;
UPDATE `tok_bestiary` SET `Bestiary_ID` = 97 WHERE `Creature_Sub_Type` = 158;

-- Subtype 68 "Hammerer" has no client species and so no counter. Make its absence explicit as 0
-- rather than NULL, both so the NOT NULL below is satisfiable and so the ORM's non-nullable
-- ushort reads a value the code deliberately treats as "unbound" instead of an accidental one.
UPDATE `tok_bestiary` SET `Bestiary_ID` = 0 WHERE `Bestiary_ID` IS NULL;

-- Lock the column down so a future import cannot silently reintroduce NULL -> 0.
ALTER TABLE `tok_bestiary` MODIFY `Bestiary_ID` SMALLINT UNSIGNED NOT NULL DEFAULT 0;

COMMIT;

-- Verification:
--   SELECT COUNT(*) FROM tok_bestiary;                          -- 137
--   SELECT COUNT(*) FROM tok_bestiary WHERE Bestiary_ID > 0;    -- 136
--   SELECT COUNT(DISTINCT Bestiary_ID) FROM tok_bestiary WHERE Bestiary_ID > 0; -- 136 (no collisions)
--   SELECT Creature_Sub_Type FROM tok_bestiary WHERE Bestiary_ID = 0;           -- 68 (Hammerer) only

-- 77_restore_ability_effect_ids_from_client.sql
--
-- Takes ability EffectID from the client, and clears the fabricated values that were standing in
-- for it. Also names the source of the damage migration 76 repaired.
--
-- WHERE THE BAD DATA CAME FROM. mythic_src_abilities was bulk-populated from
-- data/gamedata/abilities.csv, imported into this database as mythic_csv_abilities, keyed on that
-- file's ID column. The signature is unmistakable:
--
--   3,601 of its IconId values equal mythic_csv_abilities.IconId  (abilities: 20)
--   4,193 of its EffectID values equal mythic_csv_abilities.EffectAbilityId
--   4,119 of its EffectID values are simply the row's own Entry -- because that CSV's
--         "Effect (Special)" column is its own row id on 5,086 of 5,184 rows
--
-- But abilities.csv is an art/animation authoring sheet, and its ID column is NOT the runtime
-- ability id. Against the client's own UI string table, data/strings/english/abilitynames.txt, it
-- agrees on 13 ids out of 3,115. "Hip Shot" is id 1520 to the client and row 692 in that CSV; 692
-- is Rampaging Siphon, a Disciple of Khaine ability. Every column copied across that join landed
-- on the wrong ability.
--
-- WHICH SOURCE IS AUTHORITATIVE. abilitynames.txt is what the client's UI renders from, so it
-- settles the id space. Measured against it:
--
--   mythic_bin_ability      12,865 match     69 differ   (99.5%)
--   abilities                3,878 match    297 differ
--   mythic_src_abilities     5,995 match    324 differ   (after migration 76)
--   mythic_csv_abilities        13 match  2,430 differ
--
-- mythic_bin_ability is the client's own ability records and is effectively exact; the 69 are
-- encoding artifacts and trailing whitespace. It is therefore the source for EffectID here. The
-- remaining name differences in the two server tables are deliberate and are left alone: mounts,
-- where the client uses one generic "Summon Mount" for rows the server names individually
-- ("Blue Roan Elven Mare"), and emulator disambiguation ("Enfeebling Strike Self AP",
-- "Obsessive Focus Debuff").
--
-- WHY EffectID MATTERS. It is written straight into the cast packets (AbilityProcessor.cs:432,
-- 945, 1074, 1094 and AbilityInterface.cs:696), so it is the visual the client plays for the
-- ability. IconId, by contrast, is stored and never read by any server code -- section C is
-- hygiene, not behaviour, and only removes values that are provably CSV-derived.
--
-- WHAT IS DELIBERATELY LEFT ALONE. 67 rows where the server carries an EffectID and the client's
-- record has none, and which do not carry the CSV signature. Most are unnamed emulator-authored
-- rows (2701-2709 all share EffectID 2751). A zero in the client record is equally consistent with
-- the import not having captured one, so there is no evidence to act on; taking them out would be
-- the same guessing that caused this. Recorded in docs/ABILITY_TABLE_ALIGNMENT.md.
--
-- Both parallel tables are written, per CLAUDE.md hard rule 1. Ability data is cached at boot, so
-- the server must be restarted.

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- A. Take the client's EffectID wherever it has one. 2,878 rows in
--    mythic_src_abilities, 1,176 in abilities.
-- ---------------------------------------------------------------------------

UPDATE mythic_src_abilities m
  JOIN mythic_bin_ability b ON b.ID = m.Entry
   SET m.EffectID = b.EffectID
 WHERE COALESCE(b.EffectID, 0) <> 0
   AND NOT (m.EffectID <=> b.EffectID);

UPDATE abilities a
  JOIN mythic_bin_ability b ON b.ID = a.Entry
   SET a.EffectID = b.EffectID
 WHERE COALESCE(b.EffectID, 0) <> 0
   AND NOT (a.EffectID <=> b.EffectID);

-- ---------------------------------------------------------------------------
-- B. Clear the fabricated ones: EffectID equal to the row's own Entry, matching the CSV column it
--    was copied from, where the client record says the ability has no effect. 2,441 rows; none in
--    abilities, which was never populated this way.
-- ---------------------------------------------------------------------------

UPDATE mythic_src_abilities m
  JOIN mythic_bin_ability b ON b.ID = m.Entry
  JOIN mythic_csv_abilities c ON c.AbilityId = m.Entry
   SET m.EffectID = 0
 WHERE COALESCE(b.EffectID, 0) = 0
   AND m.EffectID <> 0
   AND m.EffectID = m.Entry
   AND m.EffectID = c.EffectAbilityId;

-- ---------------------------------------------------------------------------
-- C. Clear provably CSV-derived IconId on the rows migration 76 could not reach -- those that
--    exist only in mythic_src_abilities, so had no correct counterpart to copy from. 2,955 rows.
--    No server code reads IconId; this stops the wrong value being mistaken for evidence later.
-- ---------------------------------------------------------------------------

UPDATE mythic_src_abilities m
  JOIN mythic_csv_abilities c ON c.AbilityId = m.Entry
  LEFT JOIN abilities a ON a.Entry = m.Entry
   SET m.IconId = 0
 WHERE a.Entry IS NULL
   AND c.IconId IS NOT NULL
   AND m.IconId = c.IconId
   AND m.IconId <> 0;

COMMIT;

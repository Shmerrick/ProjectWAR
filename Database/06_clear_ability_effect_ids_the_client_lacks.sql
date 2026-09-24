-- 06_clear_ability_effect_ids_the_client_lacks.sql
--
-- Clears 9 ability EffectIDs where the client's record has no effect.
--
-- POLICY. The 1.4.8 client is the arbiter. Migration 01 left these alone because nothing proved them
-- wrong; under the rule that the client wins, its record having no effect is the proof. EffectID goes
-- into the cast packets, so the client now plays no visual for these casts, as its own data says.
--
-- Both tables are written (CLAUDE.md hard rule 1). Each carries its own old value, so a row where
-- abilities and mythic_src_abilities had drifted apart still conforms in both.
--
-- Safe to re-run: every UPDATE matches only the old value, per table. Ability data is cached at
-- boot, so restart the server.

USE war_world;

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_06_effect_ids;
CREATE TEMPORARY TABLE tmp_06_effect_ids (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    OldSrc SMALLINT UNSIGNED NULL,
    OldAbl SMALLINT UNSIGNED NULL
);

-- Entry, mythic_src_abilities.EffectID and abilities.EffectID (NULL here means that table is already 0).
INSERT INTO tmp_06_effect_ids (Entry, OldSrc, OldAbl) VALUES
    (5025, 1054, 1054),  -- Jar o' Pummelin'
    (5240, 1043, 1043),  -- Gitzappa Aura
    (5347, 1430, 1430),  -- Bestial Flurry
    (10327, 3503, 3503),  -- Restraining Shot GIAB
    (10328, 3504, 3504),  -- Enough! GIAB
    (10332, 3508, 3508),  -- Lion's Savagery GIAB
    (10696, 3524, 3524),  -- Can't Slow Me Down GIAB
    (20649, 905, 905),  -- Sun Scales
    (27999, 218, 218);  -- WAR Tract

UPDATE mythic_src_abilities m
  JOIN tmp_06_effect_ids c ON c.Entry = m.Entry
   SET m.EffectID = 0
 WHERE m.EffectID = c.OldSrc;

UPDATE abilities a
  JOIN tmp_06_effect_ids c ON c.Entry = a.Entry
   SET a.EffectID = 0
 WHERE a.EffectID = c.OldAbl;

DROP TEMPORARY TABLE tmp_06_effect_ids;

COMMIT;

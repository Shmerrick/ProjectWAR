-- 01_restore_ability_effect_ids_from_client.sql
--
-- Takes 22 ability EffectIDs from the client that migration 77 could not reach, because the
-- toolkit import it copied from, mythic_bin_ability, is wrong or empty for them. Follow-up to
-- BUG-129.
--
-- WHY IT MATTERS. EffectID is written into the cast packets (AbilityProcessor.cs:432), so it is the
-- visual the client plays for the ability.
--
-- THE EVIDENCE. The client's own ability record is data/bin/abilityexport.bin, read directly by
-- `ClientDataMatrix crosswalk abilities` rather than through the import. Each value is checked a
-- second time inside the client, against the effect sheets data/gamedata/effects.csv and
-- abilities.csv, which are keyed by effect id and run to 5,426. Where the live-server packet
-- captures cover an ability, it is checked a third time against the EffectID the real 1.4.8 server
-- sent when a cast started (tools/captures/extract_use_ability.awk). Across the 1,435 abilities the
-- captures cover cleanly, that live value equals abilityexport.bin every time.
--
-- A. Four abilities name an effect the client does not have, or another ability's:
--
--      696 Divine Protection    9410 -> 236    no effect 9410 exists; 236 is "Divine Protection",
--                                              and the live server sent 236 on 57 cast starts
--     1712 Gork Smash!          8110 -> 877    no effect 8110 exists; 877 is "Gork Smash!"
--     3608 Blessing of Wrath   15981 -> 2082   15981 is an ability id, not an effect; 2082 is the
--                                              client's value (a shared effect, named
--                                              "Tzeentch Shall Remake You" in both sheets)
--    15981 Nepenthean Tonic      686 -> 4518   686 is "Acid Bomb"; 4518 is "Nepenthean Tonic"
--
--    The import holds 9410, 8110 and 15981 as well -- migration 77 copied them from it -- and 0
--    for Nepenthean Tonic.
--
-- B. Three abilities name an earlier authored effect instead of the one the live server sent:
--
--      425 Steam Vent            28 -> 85      live server: 85 on 12 of 12 cast starts
--      445 Warping Energy        46 -> 63      live server: 63 on 11 of 11
--      446 Coruscating Energy    47 -> 64      live server: 64 on 14 of 14
--
--    effects.csv holds 85, 63 and 64 as rows with no build-up, cast, impact or channel stage, while
--    28, 46 and 47 are fully authored under the same names -- which is what made the client's value
--    look doubtful. The captures settle it: 85, 63 and 64 are what the real server sent. The import
--    holds 0 for all three.
--
-- C. Five Puncture ranks, 3813-3817, carry 0 where the client has 2543, "Puncture".
--
-- D. Ten helper abilities -- the Orcapult trajectories 3725-3731, Rend Invisible Counters 3759 and
--    the Prayer of Devotion/Absolution Blockers 3778-3779 -- carry 3701 or 3702, "Mount Effects -
--    Shared" and "Mount Effects - Magus", where the client record has no effect at all. Each value
--    equals mythic_csv_abilities.EffectAbilityId for its own Entry: the abilities.csv signature
--    migration 77 cleared, in a form its section B did not match, since there the CSV value was
--    another row's id rather than the row's own.
--
-- LEFT ALONE, AND WHY. 5025, 5240, 5347, 10327, 10328, 10332, 10696, 20649 and 27999, whose client
-- record has no effect and whose value carries no CSV signature -- the class migration 77 also
-- left. The captures do not settle them: they hold only kind-2 frames with EffectID 0 for 5025,
-- 5240 and 5347, and kind-2 frames carry 0 even for abilities whose cast starts send a real effect.
--
-- Sections C and D, and 3608, touch mythic_src_abilities only in practice: abilities has no rows
-- for those entries. Both tables are written regardless. Safe to re-run: every UPDATE matches only
-- the wrong value. Ability data is cached at boot, so restart the server.

USE war_world;

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- A, B and C. Take the client's EffectID where ours is wrong or empty.
-- ---------------------------------------------------------------------------

DROP TEMPORARY TABLE IF EXISTS tmp_01_client_effect_ids;
CREATE TEMPORARY TABLE tmp_01_client_effect_ids (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    WrongEffectID SMALLINT UNSIGNED NOT NULL,
    ClientEffectID SMALLINT UNSIGNED NOT NULL
);

INSERT INTO tmp_01_client_effect_ids (Entry, WrongEffectID, ClientEffectID) VALUES
    (696, 9410, 236),     -- Divine Protection
    (1712, 8110, 877),    -- Gork Smash!
    (3608, 15981, 2082),  -- Blessing of Wrath
    (15981, 686, 4518),   -- Nepenthean Tonic
    (425, 28, 85),        -- Steam Vent
    (445, 46, 63),        -- Warping Energy
    (446, 47, 64),        -- Coruscating Energy
    (3813, 0, 2543),      -- Puncture
    (3814, 0, 2543),      -- Puncture
    (3815, 0, 2543),      -- Puncture
    (3816, 0, 2543),      -- Puncture
    (3817, 0, 2543);      -- Puncture

UPDATE mythic_src_abilities m
  JOIN tmp_01_client_effect_ids c ON c.Entry = m.Entry
   SET m.EffectID = c.ClientEffectID
 WHERE m.EffectID = c.WrongEffectID;

UPDATE abilities a
  JOIN tmp_01_client_effect_ids c ON c.Entry = a.Entry
   SET a.EffectID = c.ClientEffectID
 WHERE a.EffectID = c.WrongEffectID;

DROP TEMPORARY TABLE tmp_01_client_effect_ids;

-- ---------------------------------------------------------------------------
-- D. Clear CSV-derived "Mount Effects" values where the client record has no effect.
-- ---------------------------------------------------------------------------

UPDATE mythic_src_abilities m
  JOIN mythic_csv_abilities c ON c.AbilityId = m.Entry
   SET m.EffectID = 0
 WHERE m.Entry IN (3725, 3726, 3727, 3728, 3729, 3730, 3731, 3759, 3778, 3779)
   AND m.EffectID IN (3701, 3702)
   AND m.EffectID = c.EffectAbilityId;

UPDATE abilities a
  JOIN mythic_csv_abilities c ON c.AbilityId = a.Entry
   SET a.EffectID = 0
 WHERE a.Entry IN (3725, 3726, 3727, 3728, 3729, 3730, 3731, 3759, 3778, 3779)
   AND a.EffectID IN (3701, 3702)
   AND a.EffectID = c.EffectAbilityId;

COMMIT;

-- 00_fix_ability_timing_units.sql
--
-- Restores 29 ability timings stored in the wrong unit, in both ability tables. BUG-166.
--
-- THE UNITS. mythic_src_abilities.Cooldown is seconds: AbilityProcessor.StartAbility hands
-- Cooldown * 1000 to SetCooldown (AbilityProcessor.cs:75, :109). CastTime is milliseconds:
-- ABrain.TryStartNpcCast delays the chase by CastTime + 100 ms. The client's own ability records,
-- data/bin/abilityexport.bin, hold both in milliseconds. `ClientDataMatrix crosswalk abilities`
-- checks every row under exactly those rules, and 2,182 cooldowns and 1,694 cast times agree --
-- the evidence that the rules are right and that these rows are the exceptions.
--
-- A. 26 cooldowns hold the client's millisecond figure verbatim in the seconds column: 1000, 2000
--    or 3000 where the client has 1, 2 or 3 seconds. Through the x1000 a 3-second cooldown lasts
--    50 minutes, and AbilityProcessor.AllowStartCast refuses a cast while it runs, for creatures as
--    for players: ABrain.TryStartNpcCast goes through AbtInterface.StartCast. All 26 are
--    CareerLine 0 creature abilities.
--
-- B. 3 cast times hold the client's seconds in the milliseconds column: 2 where the client has
--    2000, so a 2-second cast is instant.
--
-- Every value is read from abilityexport.bin in the extracted 1.4.8 client; the toolkit import
-- mythic_bin_ability carries the same figures. Nothing else in these rows changes.
--
-- Safe to re-run: each UPDATE matches only the wrong value. Ability data is cached at boot, so
-- restart the server.

USE war_world;

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- A. Cooldown seconds that are really milliseconds. 26 rows in each table.
-- ---------------------------------------------------------------------------

DROP TEMPORARY TABLE IF EXISTS tmp_00_client_cooldown_ms;
CREATE TEMPORARY TABLE tmp_00_client_cooldown_ms (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    ClientMs INT UNSIGNED NOT NULL
);

INSERT INTO tmp_00_client_cooldown_ms (Entry, ClientMs) VALUES
    (12861, 1000),  -- Expose Soul
    (13020, 3000),  -- Lethargy
    (13080, 1000),  -- Bolt of the Mourkain
    (13087, 1000),  -- Poison Arrer
    (13089, 1000),  -- Fling Spines
    (13093, 1000),  -- Fling
    (13127, 2000),  -- Poison Arrer
    (13130, 1000),  -- Poison Arrer
    (13147, 1000),  -- Brain Bursta
    (13150, 1000),  -- Goo Ball
    (13389, 2000),  -- Lashing Claws
    (13392, 1000),  -- Sting of Sokth
    (13400, 2000),  -- Antediluvian Rage
    (13402, 2000),  -- Compel the Ephemeral
    (13409, 2000),  -- Primeval Umbrage
    (13429, 2000),  -- Compel the Ephemeral
    (13636, 2000),  -- Lector's Rage
    (13656, 2000),  -- Filth Explosion
    (13676, 2000),  -- Blast of Death
    (13680, 2000),  -- Way of Goradian
    (13681, 2000),  -- Vision of Goradian
    (13683, 1000),  -- TEST ABIL
    (13709, 3000),  -- Force Target Me
    (13914, 3000),  -- Glob of Pus
    (13934, 2000),  -- Stomach Bile
    (13960, 3000);  -- Tentacle Spit

UPDATE mythic_src_abilities m
  JOIN tmp_00_client_cooldown_ms c ON c.Entry = m.Entry
   SET m.Cooldown = c.ClientMs DIV 1000
 WHERE m.Cooldown = c.ClientMs;

UPDATE abilities a
  JOIN tmp_00_client_cooldown_ms c ON c.Entry = a.Entry
   SET a.Cooldown = c.ClientMs DIV 1000
 WHERE a.Cooldown = c.ClientMs;

DROP TEMPORARY TABLE tmp_00_client_cooldown_ms;

-- ---------------------------------------------------------------------------
-- B. Cast-time milliseconds that are really seconds. 3 rows in each table.
-- ---------------------------------------------------------------------------

DROP TEMPORARY TABLE IF EXISTS tmp_00_client_cast_ms;
CREATE TEMPORARY TABLE tmp_00_client_cast_ms (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    ClientMs INT UNSIGNED NOT NULL
);

INSERT INTO tmp_00_client_cast_ms (Entry, ClientMs) VALUES
    (5305, 2000),   -- Hard Stomp
    (12232, 1000),  -- Daemonic Fire
    (12631, 2000);  -- Spine Fling

UPDATE mythic_src_abilities m
  JOIN tmp_00_client_cast_ms c ON c.Entry = m.Entry
   SET m.CastTime = c.ClientMs
 WHERE m.CastTime * 1000 = c.ClientMs;

UPDATE abilities a
  JOIN tmp_00_client_cast_ms c ON c.Entry = a.Entry
   SET a.CastTime = c.ClientMs
 WHERE a.CastTime * 1000 = c.ClientMs;

DROP TEMPORARY TABLE tmp_00_client_cast_ms;

COMMIT;

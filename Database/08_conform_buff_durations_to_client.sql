-- 08_conform_buff_durations_to_client.sql
--
-- Sets 10 buff durations to the client's.
--
-- POLICY. The 1.4.8 client is the arbiter. An ability's components carry their durations in
-- data/bin/abilitycomponentexport.bin (milliseconds); the buff row that shares the ability's id
-- (Duration, seconds) is what the server applies. A row is taken only when the ability has exactly one
-- distinct non-zero component duration, so there is only one value the buff can mean.
--
-- NOT HERE: 7 flagged abilities with more than one candidate duration, where the buff row may
-- describe a component other than the tooltip's; they need the buff-to-component mapping.
--
-- Both buff tables are written, each with its own old value.
--
-- Safe to re-run: every UPDATE matches only the old value, per table. Ability data is cached at
-- boot, so restart the server.

USE war_world;

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_08_client_duration;
CREATE TEMPORARY TABLE tmp_08_client_duration (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    OldSrc INT UNSIGNED NULL,
    OldAbl INT UNSIGNED NULL,
    ClientSeconds INT UNSIGNED NOT NULL
);

-- Entry, mythic_src_buff_infos.Duration and buff_infos.Duration where they differ from the client (NULL
-- read as 0; NULL here means that table already agrees or has no row), the client's duration in seconds.
INSERT INTO tmp_08_client_duration (Entry, OldSrc, OldAbl, ClientSeconds) VALUES
    (21, 15, 15, 20),  -- Penetrating Round
    (651, 6, 6, 5),  -- Hail of Doom
    (1525, 2, 2, 3),  -- Self-Destruct
    (1684, 20, 20, 15),  -- Get 'Em
    (8367, 0, 0, 5),  -- Siphoned Energy
    (9177, 10, 10, 20),  -- Throat Bite
    (10356, 0, 0, 10),  -- Just a bit mor'
    (14242, 1, 1, 10),  -- Grimnir's Mercy
    (15059, 604800, 604800, 259200),  -- Fleeting Renown Boost
    (28300, 60, 60, 10);  -- Inexorable Force

UPDATE mythic_src_buff_infos b
  JOIN tmp_08_client_duration c ON c.Entry = b.Entry
   SET b.Duration = c.ClientSeconds
 WHERE COALESCE(b.Duration, 0) = c.OldSrc;

UPDATE buff_infos b
  JOIN tmp_08_client_duration c ON c.Entry = b.Entry
   SET b.Duration = c.ClientSeconds
 WHERE COALESCE(b.Duration, 0) = c.OldAbl;

DROP TEMPORARY TABLE tmp_08_client_duration;

COMMIT;

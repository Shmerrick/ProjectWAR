-- 07_conform_ability_damage_to_client.sql
--
-- Sets 12 abilities' base damage to the client's tooltip value.
--
-- POLICY. The 1.4.8 client is the arbiter. Its tooltip token names the component and value slot that
-- hold the damage, and the value is Values[slot] x Multipliers[slot] / 100 -- the reading
-- docs/ABILITY_DAMAGE_MODEL.md validates on the abilities where both sides can be compared.
--
-- ONLY WHERE THE PAIRING IS CERTAIN. A row is taken only when the ability has exactly one damage row
-- (Index 0, in each table) and its tooltip exactly one damage token, so the one number can only belong
-- to the one row. 7 further abilities disagree but have several damage rows or several damage
-- tokens; which row a token belongs to needs the component mapping, so they are not touched here.
--
-- MinDamage is the unscaled base the server scales by level, the same base the tooltip value is.
--
-- Both damage tables are written, each with its own old value.
--
-- Safe to re-run: every UPDATE matches only the old value, per table. Ability data is cached at
-- boot, so restart the server.

USE war_world;

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_07_client_damage;
CREATE TEMPORARY TABLE tmp_07_client_damage (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    OldSrc INT UNSIGNED NULL,
    OldAbl INT UNSIGNED NULL,
    ClientDamage INT UNSIGNED NOT NULL
);

-- Entry, the Index 0 MinDamage in mythic_src_ability_damage_heals and ability_damage_heals (NULL here
-- means that table already agrees or has no row), the client's tooltip value and the token it resolves.
INSERT INTO tmp_07_client_damage (Entry, OldSrc, OldAbl, ClientDamage) VALUES
    (1837, 45, 45, 30),  -- Drop That!! {COM_1_VAL0_DAMAGE}
    (1846, 40, 40, 15),  -- Shrapnel Arrer {COM_0_VAL0_DAMAGE}
    (1854, 75, 75, 45),  -- Behind Ya! {COM_0_VAL0_DAMAGE}
    (8101, 36, 36, 10),  -- Sever Blessing {COM_1_VAL0_DAMAGE}
    (8344, 36, 36, 20),  -- Rending Blade {COM_0_VAL0_SPIRITDAMAGE}
    (8347, 48, 48, 25),  -- Oppression {COM_0_VAL0_DAMAGE}
    (8377, 57, 57, 48),  -- Warping Embrace {COM_0_VAL0_DAMAGE}
    (8414, 52, 52, 25),  -- Gut Ripper {COM_0_VAL0_DAMAGE}
    (9110, 75, 75, 50),  -- Flanking Shot {COM_1_VAL0_DAMAGE}
    (9348, 48, 48, 40),  -- Wave of Scorn {COM_0_VAL0_DAMAGE}
    (9409, 55, 55, 30),  -- Throat Slitter {COM_1_VAL0_DAMAGE}
    (9581, 55, 55, 30);  -- Pillage Essence {COM_0_VAL0_DAMAGE}

UPDATE mythic_src_ability_damage_heals d
  JOIN tmp_07_client_damage c ON c.Entry = d.Entry
   SET d.MinDamage = c.ClientDamage
 WHERE d.`Index` = 0
   AND d.MinDamage = c.OldSrc;

UPDATE ability_damage_heals d
  JOIN tmp_07_client_damage c ON c.Entry = d.Entry
   SET d.MinDamage = c.ClientDamage
 WHERE d.`Index` = 0
   AND d.MinDamage = c.OldAbl;

DROP TEMPORARY TABLE tmp_07_client_damage;

COMMIT;

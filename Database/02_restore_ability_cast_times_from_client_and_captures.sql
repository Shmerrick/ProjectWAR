-- 02_restore_ability_cast_times_from_client_and_captures.sql
--
-- Takes 119 ability cast times from where the 1.4.8 client and the live server agree. BUG-167.
--
-- THE EVIDENCE, TWICE. data/bin/abilityexport.bin is the client's own record of each ability's cast
-- time. The live-server packet captures hold the cast time the real server applied: F_USE_ABILITY
-- (0xDA) carries it when a cast starts, and tools/captures/extract_use_ability.awk decodes it. Across
-- the 1,027 captures the two agree on 1,298 of the 1,312 abilities with a clean sample. Every row
-- below is one where they agree with each other -- the same value, in at least 80% of that
-- ability's cast-start frames -- and mythic_src_abilities says something else.
--
-- WHAT CHANGES.
--   113 rows whose CastTime is NULL or 0, an instant cast, where the client and the live
--   server both give a cast time: creature and world abilities, the pet and turret attacks 420-448,
--   and item abilities.
--   3 recall scrolls, 4180, 4181 and 4184, at 10 s where both say 15 s.
--   3 rows carrying a cast time where both say the cast is instant: 4980 Black Dragon Breath,
--   14420 Bulwark and 24824 Snare Net.
--
-- WHAT DOES NOT, AND WHY.
--   67 abilities where the client and the live server say 0 but ours carries a cast time are all
--   channels in our table, with ChannelID set. The emulator stores a channel's length in CastTime
--   (NewChannelHandler.cs) and sends 0 in the packet, exactly as the live server did, so they are
--   not faults. Every UPDATE below refuses a row with ChannelID set, which also leaves 5263
--   Smash 'Em 'Ard! -- ours a 10 s channel, the live server a 1 s cast -- for a decision.
--   Abilities no capture covers are not touched, even where the client alone disagrees with us.
--
-- Both tables are written; abilities holds 25 of these entries. Safe to re-run: each
-- row matches only its old value. Ability data is cached at boot, so restart the server.

USE war_world;

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_02_live_cast_ms;
CREATE TEMPORARY TABLE tmp_02_live_cast_ms (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    OurMs INT UNSIGNED NOT NULL,
    LiveMs INT UNSIGNED NOT NULL
);

-- Entry, our cast time (NULL read as 0), the client's and live server's cast time.
INSERT INTO tmp_02_live_cast_ms (Entry, OurMs, LiveMs) VALUES
    (420, 0, 2000),  -- Penetrating Round: 30 of 30 cast starts
    (422, 0, 2000),  -- Shock Grenade: 15 of 15 cast starts
    (423, 0, 1000),  -- High-Explosive Grenade: 10 of 10 cast starts
    (424, 0, 2000),  -- Flamethrower: 2 of 2 cast starts
    (440, 0, 2000),  -- Goop Shootin': 252 of 252 cast starts
    (441, 0, 2000),  -- Spore Cloud: 149 of 149 cast starts
    (443, 0, 3000),  -- Daemonic Fire: 15 of 15 cast starts
    (444, 0, 2000),  -- Daemonic Consumption: 11 of 11 cast starts
    (445, 0, 2000),  -- Warping Energy: 11 of 11 cast starts
    (448, 0, 1000),  -- Flames Of Change: 2 of 2 cast starts
    (1020, 0, 500),  -- Portable Mailbox: 2 of 2 cast starts
    (4037, 0, 1500),  -- Fireball: 26 of 26 cast starts
    (4040, 0, 2500),  -- Fiery Blast: 28 of 28 cast starts
    (4046, 0, 3000),  -- The Spirit of the Forge: 20 of 20 cast starts
    (4048, 0, 2500),  -- Crown of Taidron: 1 of 1 cast starts
    (4060, 0, 1500),  -- Forked Lightning: 1966 of 1966 cast starts
    (4065, 0, 2500),  -- Healing Energy: 2 of 2 cast starts
    (4073, 0, 3500),  -- Master of Stone: 1 of 1 cast starts
    (4078, 0, 1500),  -- Drain Life: 1 of 1 cast starts
    (4079, 0, 2000),  -- Magnificent Buboes: 1 of 1 cast starts
    (4091, 0, 2500),  -- Blissful Throes: 1 of 1 cast starts
    (4102, 0, 2500),  -- Brain Bursta: 4 of 4 cast starts
    (4109, 0, 2000),  -- Braingobbler: 1 of 1 cast starts
    (4111, 0, 1500),  -- Bonecruncher: 1 of 1 cast starts
    (4125, 0, 3000),  -- Syphon Life: 2 of 2 cast starts
    (4128, 0, 1500),  -- Silence: 2 of 2 cast starts
    (4130, 0, 2500),  -- Cripple: 10 of 10 cast starts
    (4133, 0, 3000),  -- Crippling Blast: 27 of 27 cast starts
    (4180, 10000, 15000),  -- Activating...: 8 of 8 cast starts
    (4181, 10000, 15000),  -- Activating...: 12 of 12 cast starts
    (4182, 0, 15000),  -- Activating...: 1 of 1 cast starts
    (4183, 0, 15000),  -- Activating...: 1 of 1 cast starts
    (4184, 10000, 15000),  -- Activating...: 1 of 1 cast starts
    (4223, 0, 2000),  -- Power Channel Summon: 20 of 20 cast starts
    (4315, 0, 2000),  -- Phoenix Wing: 16 of 16 cast starts
    (4317, 0, 2000),  -- Phoenix Wing: 31 of 31 cast starts
    (4319, 0, 2000),  -- Asuryan's Mercy: 2 of 2 cast starts
    (4321, 0, 2000),  -- Asuryan's Mercy: 8 of 8 cast starts
    (4325, 0, 2000),  -- Asuryan's Mercy: 27 of 27 cast starts
    (4328, 0, 1000),  -- Seafarer's Volley: 10 of 10 cast starts
    (4329, 0, 1000),  -- Vengeance of Avelorn: 6 of 6 cast starts
    (4350, 0, 2500),  -- ability: 14 of 14 cast starts
    (4351, 0, 2500),  -- ability: 8 of 8 cast starts
    (4355, 0, 1500),  -- ability: 21 of 21 cast starts
    (4356, 0, 2500),  -- ability: 4 of 4 cast starts
    (4357, 0, 1000),  -- ability: 36 of 36 cast starts
    (4360, 0, 1000),  -- ability: 1 of 1 cast starts
    (4361, 0, 1000),  -- ability: 2 of 2 cast starts
    (4602, 0, 2000),  -- Burst of Flame: 870 of 870 cast starts
    (4603, 0, 2000),  -- Corporeal Blast: 360 of 360 cast starts
    (4604, 0, 2000),  -- Spirit Blast: 680 of 680 cast starts
    (4607, 0, 2000),  -- On Fire: 722 of 722 cast starts
    (4608, 0, 2000),  -- Life Loss: 396 of 396 cast starts
    (4609, 0, 2000),  -- Spirit Drain: 154 of 154 cast starts
    (4611, 0, 2000),  -- Combustion: 17 of 17 cast starts
    (4612, 0, 2000),  -- Blast Wave: 41 of 41 cast starts
    (4614, 0, 2000),  -- Sharp Projectile: 731 of 731 cast starts
    (4615, 0, 2000),  -- Piercing Projectile: 1040 of 1040 cast starts
    (4616, 0, 2000),  -- Volley: 39 of 39 cast starts
    (4620, 0, 2000),  -- Elemental Rain: 192 of 192 cast starts
    (4621, 0, 2000),  -- Corporeal Rain: 58 of 58 cast starts
    (4622, 0, 2000),  -- Ethereal Rain: 188 of 188 cast starts
    (4623, 0, 2000),  -- Shard Volley: 20 of 20 cast starts
    (4624, 0, 2000),  -- Elemental Circle: 324 of 324 cast starts
    (4626, 0, 2000),  -- Drain Circle: 12 of 12 cast starts
    (4630, 0, 2000),  -- Elemental Breath: 9 of 9 cast starts
    (4636, 0, 2000),  -- Elemental Stun: 19 of 19 cast starts
    (4637, 0, 2000),  -- Corporeal Stun: 12 of 12 cast starts
    (4641, 0, 2000),  -- Sticky Projectile: 1057 of 1057 cast starts
    (4642, 0, 2000),  -- Elemental Snare: 2 of 2 cast starts
    (4643, 0, 2000),  -- Corporeal Snare: 315 of 315 cast starts
    (4644, 0, 2000),  -- Ethereal Snare: 262 of 262 cast starts
    (4647, 0, 2000),  -- Web Projectile: 251 of 251 cast starts
    (4648, 0, 2000),  -- Elemental Root: 128 of 128 cast starts
    (4649, 0, 2000),  -- Corporeal Root: 147 of 147 cast starts
    (4650, 0, 2000),  -- Ethereal Root: 67 of 67 cast starts
    (4653, 0, 2000),  -- Elemental Thrust: 8 of 8 cast starts
    (4655, 0, 2000),  -- Ethereal Thrust: 118 of 118 cast starts
    (4656, 0, 2000),  -- Elemental Push: 1 of 1 cast starts
    (4657, 0, 2000),  -- Corporeal Push: 29 of 29 cast starts
    (4665, 0, 2000),  -- Elemental Stop: 5 of 5 cast starts
    (4667, 0, 2000),  -- Ethereal Stop: 3 of 3 cast starts
    (4683, 0, 2000),  -- Elemental Mass Thrust: 1 of 1 cast starts
    (4685, 0, 2000),  -- Ethereal Mass Thrust: 3 of 3 cast starts
    (4688, 0, 2000),  -- Elemental Silence: 14 of 14 cast starts
    (4690, 0, 2000),  -- Ethereal Silence: 1 of 1 cast starts
    (4692, 0, 2000),  -- Elemental Mass Silence: 4 of 4 cast starts
    (4693, 0, 2000),  -- Corporeal Mass Silence: 1 of 1 cast starts
    (4697, 0, 2000),  -- Corporeal Silence Blast: 1 of 1 cast starts
    (4711, 0, 2000),  -- Heal: 278 of 278 cast starts
    (4712, 0, 2000),  -- Regen: 195 of 195 cast starts
    (4713, 0, 2000),  -- Healing Blast: 266 of 266 cast starts
    (4714, 0, 2000),  -- Healing Circle: 35 of 35 cast starts
    (4715, 0, 2000),  -- Sensitive: 270 of 270 cast starts
    (4716, 0, 2000),  -- Inept: 462 of 462 cast starts
    (4717, 0, 2000),  -- Shaky Resolve: 594 of 594 cast starts
    (4718, 0, 2000),  -- Pushover: 39 of 39 cast starts
    (4723, 0, 2000),  -- Scream: 35 of 35 cast starts
    (4812, 0, 1000),  -- Lifetap: 422 of 422 cast starts
    (4814, 0, 2000),  -- Dragon Orb Effect: 2 of 2 cast starts
    (4906, 0, 1000),  -- Giant Fireball: 3 of 3 cast starts
    (4962, 0, 2000),  -- ability: 1 of 1 cast starts
    (4963, 0, 3500),  -- Phoenix Blade toss: 12 of 12 cast starts
    (4964, 0, 3500),  -- Asuryans Will: 13 of 13 cast starts
    (4980, 2000, 0),  -- Black Dragon Breath: 27 of 27 cast starts
    (4981, 0, 1000),  -- Black Fireball: 20 of 20 cast starts
    (4982, 0, 3000),  -- Black Fireball (AoE): 18 of 18 cast starts
    (4984, 0, 1000),  -- Sun Fireball: 38 of 38 cast starts
    (4985, 0, 3000),  -- Sun Fireball (AoE): 43 of 43 cast starts
    (4990, 0, 3000),  -- Forked Lightning: 30 of 30 cast starts
    (4991, 0, 2000),  -- Radiant Strike: 22 of 22 cast starts
    (5051, 0, 1500),  -- Consecrate Ground: 1 of 1 cast starts
    (5236, 0, 2000),  -- Moonflare: 94 of 94 cast starts
    (5241, 0, 5000),  -- Kablooey: 1 of 1 cast starts
    (5244, 0, 3000),  -- Brain Bursta: 31 of 31 cast starts
    (14420, 15000, 0),  -- Bulwark: 7 of 7 cast starts
    (15169, 0, 2000),  -- The Librams of Insight: 2 of 2 cast starts
    (24824, 1000, 0),  -- Snare Net: 175 of 175 cast starts
    (24887, 0, 500);  -- Explosion: 5 of 5 cast starts

UPDATE mythic_src_abilities m
  JOIN tmp_02_live_cast_ms c ON c.Entry = m.Entry
   SET m.CastTime = c.LiveMs
 WHERE COALESCE(m.CastTime, 0) = c.OurMs
   AND COALESCE(m.ChannelID, 0) = 0;

UPDATE abilities a
  JOIN tmp_02_live_cast_ms c ON c.Entry = a.Entry
   SET a.CastTime = c.LiveMs
 WHERE COALESCE(a.CastTime, 0) = c.OurMs
   AND COALESCE(a.ChannelID, 0) = 0;

DROP TEMPORARY TABLE tmp_02_live_cast_ms;

COMMIT;

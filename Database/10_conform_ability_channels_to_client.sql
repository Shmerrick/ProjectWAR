-- 10_conform_ability_channels_to_client.sql
--
-- Moves every channel's length out of CastTime into its own column, and takes the channel's length,
-- tick, cast time and AP cost from the client, for the 87 channels in the ability tables.
--
-- POLICY. The 1.4.8 client is the arbiter; where the server reads a field differently, the server
-- changes (docs/DATABASE_FIDELITY_PLAN.md).
--
-- HOW THE TWO DIFFERED. The emulator kept a channel's length in CastTime, ticked every second and charged
-- ApCost on each tick (NewChannelHandler). The client keeps CastTime at 0 -- or at the cast that comes
-- before the channel -- and holds three separate things:
--   * whether it is a channel at all: FlagsRaw bit 22 of data/bin/abilityexport.bin, set on 99 of the 99
--     abilities the packet captures show channelling and on 1 of 1,663 they show casting;
--   * how long it lasts: the Duration of the ability's first timed component in
--     data/bin/abilitycomponentexport.bin, which is the length the live server sent in its channel-start
--     frames for 90 of 93 live channels (the rest are not ours);
--   * how often it spends AP: ChannelInterval, with ApCost the amount per tick. Our AP costs were the
--     client's rescaled to one-second ticks on 47 of the 48 channels where that can be tested, so the AP
--     drain barely moves; what changes is that it comes in the client's steps.
--
-- WHAT THIS WRITES. Two columns on both tables: ChannelDuration (ms) and ChannelInterval (ms, 0 where the
-- client gives none, which the server reads as once a second). For every row with a ChannelID:
-- ChannelDuration from the client's component (78 rows; the other 9 keep today's length because
-- the client has no component duration or no record); CastTime from the client (0 for a pure channel);
-- ApCost from the client; and the channel buff's Duration where the length is whole seconds (13 rows),
-- because the channel buff is that component. All 83 channels with a client record carry the client's
-- channel flag, so no ability changes from cast to channel or back here. The captures cover 64 of
-- them and agree with the client's length on 64.
--
-- NOT HERE. The client flags 127 more abilities as channels that ours does not channel. 112 have no buff
-- row and no commands at all, and the rest carry ability commands the channel handler would never run
-- (the teleport scrolls 14478-14480 among them), so converting them needs channel-end command execution
-- first. Buff tick intervals are not taken either: several channel buffs tick at a different interval
-- from their component, and which command each tick drives needs the component mapping.
--
-- ORDER. The server build that reads ChannelDuration maps both columns, and NewChannelHandler refuses a
-- channel without a length, so apply this before starting it. Safe to re-run: the columns are added
-- only if missing, the length and tick are fixed values, and every other UPDATE matches only the old
-- value. Ability data is cached at boot, so restart the server.

USE war_world;

SET @missing := (SELECT COUNT(*) = 0 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'mythic_src_abilities' AND COLUMN_NAME = 'ChannelDuration');
SET @ddl := IF(@missing, 'ALTER TABLE mythic_src_abilities ADD COLUMN ChannelDuration INT UNSIGNED NOT NULL DEFAULT 0 AFTER ChannelID', 'DO 0');
PREPARE ddl FROM @ddl;
EXECUTE ddl;
DEALLOCATE PREPARE ddl;

SET @missing := (SELECT COUNT(*) = 0 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'mythic_src_abilities' AND COLUMN_NAME = 'ChannelInterval');
SET @ddl := IF(@missing, 'ALTER TABLE mythic_src_abilities ADD COLUMN ChannelInterval SMALLINT UNSIGNED NOT NULL DEFAULT 0 AFTER ChannelDuration', 'DO 0');
PREPARE ddl FROM @ddl;
EXECUTE ddl;
DEALLOCATE PREPARE ddl;

SET @missing := (SELECT COUNT(*) = 0 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'abilities' AND COLUMN_NAME = 'ChannelDuration');
SET @ddl := IF(@missing, 'ALTER TABLE abilities ADD COLUMN ChannelDuration INT UNSIGNED NOT NULL DEFAULT 0 AFTER ChannelID', 'DO 0');
PREPARE ddl FROM @ddl;
EXECUTE ddl;
DEALLOCATE PREPARE ddl;

SET @missing := (SELECT COUNT(*) = 0 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE()
    AND TABLE_NAME = 'abilities' AND COLUMN_NAME = 'ChannelInterval');
SET @ddl := IF(@missing, 'ALTER TABLE abilities ADD COLUMN ChannelInterval SMALLINT UNSIGNED NOT NULL DEFAULT 0 AFTER ChannelDuration', 'DO 0');
PREPARE ddl FROM @ddl;
EXECUTE ddl;
DEALLOCATE PREPARE ddl;

START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS tmp_10_channels;
CREATE TEMPORARY TABLE tmp_10_channels (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    DurationSrc INT UNSIGNED NULL,
    DurationAbl INT UNSIGNED NULL,
    ChannelInterval SMALLINT UNSIGNED NOT NULL,
    OldCastSrc INT UNSIGNED NULL,
    OldCastAbl INT UNSIGNED NULL,
    ClientCast INT UNSIGNED NOT NULL,
    OldApSrc INT UNSIGNED NULL,
    OldApAbl INT UNSIGNED NULL,
    ClientAp INT UNSIGNED NULL
);

-- Entry; ChannelDuration for mythic_src_abilities and abilities (NULL: not a channel in that table);
-- ChannelInterval; each table's CastTime where it differs from the client's, and the client's; each
-- table's ApCost where it differs, and the client's (NULL: no client record, ours kept).
INSERT INTO tmp_10_channels (Entry, DurationSrc, DurationAbl, ChannelInterval, OldCastSrc, OldCastAbl, ClientCast, OldApSrc, OldApAbl, ClientAp) VALUES
    (7, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Spine Fling
    (22, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Machine Gun
    (28, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Steam Vent
    (57, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Coruscating Energy
    (58, 2000, 2000, 0, 2000, 2000, 0, NULL, NULL, 0),  -- Flame Of Tzeentch
    (421, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Machine Gun (no client component duration, ours kept)
    (425, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Steam Vent (no client component duration, ours kept)
    (436, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Spine Fling (no client component duration, ours kept)
    (446, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Coruscating Energy (no client component duration, ours kept)
    (447, 2000, 2000, 0, 2000, 2000, 0, NULL, NULL, 0),  -- Flame Of Tzeentch (no client component duration, ours kept)
    (607, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Raze
    (651, 5000, 5000, 0, 5500, 5500, 0, NULL, NULL, 0),  -- Hail of Doom
    (1378, 12000, 12000, 1000, 12000, 12000, 0, NULL, NULL, 20),  -- Hold The Line!
    (1385, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 20),  -- Grudge-Born Fury
    (1450, 10000, 10000, 1000, 10000, 10000, 0, NULL, NULL, 15),  -- Retribution
    (1457, 5000, 5000, 1000, 5000, 5000, 0, NULL, NULL, 15),  -- Rune of Absorption
    (1528, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 25),  -- Focused Fire
    (1580, 4000, 4000, 0, 4000, 4000, 0, NULL, NULL, 0),  -- Scattershot
    (1581, 4000, 4000, 0, 4000, 4000, 0, NULL, NULL, 0),  -- Artillery Barrage
    (1582, 4000, 4000, 0, 4000, 4000, 0, NULL, NULL, 0),  -- Fling Explosives
    (1614, 6000, 6000, 1000, 6000, 6000, 0, NULL, NULL, 20),  -- Rune of Burning
    (1685, 12000, 12000, 1000, 12000, 12000, 0, NULL, NULL, 20),  -- Hold The Line!
    (1689, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- T'ree Hit Combo
    (1692, 20000, 20000, 1000, 20000, 20000, 0, NULL, NULL, 15),  -- Can't Hit Me!
    (1762, 10000, 10000, 1000, 10000, 10000, 0, NULL, NULL, 15),  -- Bring It On
    (1768, 5000, 5000, 1000, 5000, 5000, 0, NULL, NULL, 15),  -- Furious Choppin'
    (1776, 6000, 6000, 2000, 6000, 6000, 0, 10, 10, 20),  -- Git To Da Choppa
    (1834, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 30),  -- Lots o' Arrers
    (1851, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 20),  -- Big Bouncin!
    (1903, 6000, 6000, 2000, 6000, 6000, 0, 13, 13, 25),  -- Bunch o' Waaagh
    (2801, 10000, 10000, 0, 10000, 10000, 0, NULL, NULL, NULL),  --  (no client record, ours kept)
    (2804, 5000, 5000, 0, 5000, 5000, 0, NULL, NULL, NULL),  --  (no client record, ours kept)
    (2810, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, NULL),  --  (no client record, ours kept)
    (2819, 20000, 20000, 0, 20000, 20000, 0, NULL, NULL, NULL),  --  (no client record, ours kept)
    (5239, 60000, 60000, 0, 9000, 9000, 0, 0, 0, 100),  -- Siphon o' da Mixa
    (5263, 10000, 10000, 0, 10000, 10000, 1000, NULL, NULL, 0),  -- Smash 'Em 'Ard
    (5347, 6000, 6000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Bestial Flurry
    (5568, 6000, 6000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Whirlwind
    (5598, 5000, 5000, 0, 6000, 6000, 0, NULL, NULL, 0),  -- Thorn Cloud
    (8014, 12000, 12000, 1000, 12000, 12000, 0, NULL, NULL, 20),  -- Hold The Line!
    (8031, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 25),  -- Myrmidia's Fury
    (8087, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 15),  -- Trial By Pain
    (8177, 9000, 9000, 1500, 10000, 10000, 0, 10, 10, 15),  -- Rain of Fire
    (8183, 3000, 3000, 1500, 3000, 3000, 0, 20, 20, 30),  -- Fireball Barrage
    (8185, 6000, 6000, 2000, 6000, 6000, 0, 13, 13, 25),  -- Withering Heat
    (8187, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 15),  -- Annihilate
    (8237, 5000, 5000, 1000, 5000, 5000, 0, NULL, NULL, 30),  -- Supplication
    (8244, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Divine Assault
    (8266, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Martyr's Blessing
    (8326, 12000, 12000, 1000, 12000, 12000, 0, NULL, NULL, 20),  -- Hold The Line!
    (8343, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 25),  -- Relentless
    (8406, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 25),  -- Convulsive Slashing
    (8425, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 30),  -- Wrecking Ball
    (8485, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 25),  -- Warpfire
    (8498, 6000, 6000, 2000, 6000, 6000, 0, 13, 13, 25),  -- Tzeentch's Firestorm
    (8502, 6000, 6000, 2000, 6000, 6000, 0, 15, 15, 30),  -- Indigo Fire of Change
    (8576, 6000, 6000, 1000, 6000, 6000, 0, NULL, NULL, 20),  -- Storm of Ravens
    (8578, 6000, 6000, 2000, 6000, 6000, 0, 13, 13, 25),  -- Chaotic Agitation
    (9021, 20000, 20000, 1000, 20000, 20000, 0, NULL, NULL, 20),  -- Wall of Darting Steel
    (9023, 12000, 12000, 1000, 12000, 12000, 0, NULL, NULL, 20),  -- Hold The Line!
    (9026, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Ether Dance
    (9101, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 25),  -- Rapid Fire
    (9106, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 20),  -- Swift Strikes
    (9142, 10000, 10000, 0, 10000, 10000, 0, NULL, NULL, 0),  -- Rain of Steel
    (9188, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 35),  -- Whirling Axe
    (9250, 6000, 6000, 2000, 6000, 6000, 0, 13, 13, 25),  -- Searing Touch
    (9258, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 20),  -- Funnel Essence
    (9309, 4000, 4000, 0, 4000, 4000, 0, NULL, NULL, 0),  -- Flames of the Phoenix
    (9326, 12000, 12000, 1000, 12000, 12000, 0, NULL, NULL, 20),  -- Hold The Line!
    (9343, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 25),  -- Enraged Beating
    (9345, 10000, 10000, 1000, 10000, 10000, 0, NULL, NULL, 20),  -- None Shall Pass
    (9399, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 15),  -- Ruthless Assault
    (9458, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Dance of Doom
    (9485, 9000, 9000, 1500, 10000, 10000, 0, 10, 10, 15),  -- Pit of Shades
    (9502, 6000, 6000, 1000, 6000, 6000, 0, NULL, NULL, 20),  -- Shadow Knives
    (9503, 3000, 3000, 1000, 3000, 3000, 0, NULL, NULL, 15),  -- Disastrous Cascade
    (9505, 6000, 6000, 2000, 6000, 6000, 0, 13, 13, 25),  -- Hand of Ruin
    (9554, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Rend Soul
    (9561, 5000, 5000, 1000, 5000, 5000, 0, NULL, NULL, 30),  -- Blood Offering
    (9579, 3000, 3000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Khaine's Refreshment
    (13059, 10000, 10000, 0, NULL, NULL, 3000, NULL, NULL, 0),  -- Vampiric Shadows
    (14435, 6000, 6000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Boiling Oil
    (14440, 6000, 6000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Boiling Oil
    (14445, 6000, 6000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Boiling Oil
    (14450, 6000, 6000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Boiling Oil
    (14455, 6000, 6000, 0, 3000, 3000, 0, NULL, NULL, 0),  -- Boiling Oil
    (14460, 6000, 6000, 0, 3000, 3000, 0, NULL, NULL, 0);  -- Boiling Oil

UPDATE mythic_src_abilities m
  JOIN tmp_10_channels c ON c.Entry = m.Entry
   SET m.ChannelDuration = c.DurationSrc, m.ChannelInterval = c.ChannelInterval
 WHERE m.ChannelID > 0
   AND c.DurationSrc IS NOT NULL;

UPDATE abilities a
  JOIN tmp_10_channels c ON c.Entry = a.Entry
   SET a.ChannelDuration = c.DurationAbl, a.ChannelInterval = c.ChannelInterval
 WHERE a.ChannelID > 0
   AND c.DurationAbl IS NOT NULL;

UPDATE mythic_src_abilities m
  JOIN tmp_10_channels c ON c.Entry = m.Entry
   SET m.CastTime = c.ClientCast
 WHERE m.ChannelID > 0
   AND COALESCE(m.CastTime, 0) = c.OldCastSrc;

UPDATE abilities a
  JOIN tmp_10_channels c ON c.Entry = a.Entry
   SET a.CastTime = c.ClientCast
 WHERE a.ChannelID > 0
   AND COALESCE(a.CastTime, 0) = c.OldCastAbl;

UPDATE mythic_src_abilities m
  JOIN tmp_10_channels c ON c.Entry = m.Entry
   SET m.ApCost = c.ClientAp
 WHERE m.ChannelID > 0
   AND COALESCE(m.ApCost, 0) = c.OldApSrc;

UPDATE abilities a
  JOIN tmp_10_channels c ON c.Entry = a.Entry
   SET a.ApCost = c.ClientAp
 WHERE a.ChannelID > 0
   AND COALESCE(a.ApCost, 0) = c.OldApAbl;

DROP TEMPORARY TABLE tmp_10_channels;

DROP TEMPORARY TABLE IF EXISTS tmp_10_channel_buffs;
CREATE TEMPORARY TABLE tmp_10_channel_buffs (
    Entry SMALLINT UNSIGNED NOT NULL PRIMARY KEY,
    OldSrc INT UNSIGNED NULL,
    OldAbl INT UNSIGNED NULL,
    ClientSeconds INT UNSIGNED NOT NULL
);

-- Entry, mythic_src_buff_infos.Duration and buff_infos.Duration where they differ from the channel's
-- length (NULL: that table already agrees or has no row), the length in seconds.
INSERT INTO tmp_10_channel_buffs (Entry, OldSrc, OldAbl, ClientSeconds) VALUES
    (5239, 9, 9, 60),  -- Siphon o' da Mixa
    (5347, 3, 3, 6),  -- Bestial Flurry
    (5568, 3, 3, 6),  -- Whirlwind
    (5598, 9, 9, 5),  -- Thorn Cloud
    (8177, 10, 10, 9),  -- Rain of Fire
    (9485, 10, 10, 9),  -- Pit of Shades
    (13059, 3, 3, 10),  -- Vampiric Shadows
    (14435, 5, 5, 6),  -- Boiling Oil
    (14440, 5, 5, 6),  -- Boiling Oil
    (14445, 5, 5, 6),  -- Boiling Oil
    (14450, 5, 5, 6),  -- Boiling Oil
    (14455, 5, 5, 6),  -- Boiling Oil
    (14460, 5, 5, 6);  -- Boiling Oil

UPDATE mythic_src_buff_infos b
  JOIN tmp_10_channel_buffs c ON c.Entry = b.Entry
   SET b.Duration = c.ClientSeconds
 WHERE COALESCE(b.Duration, 0) = c.OldSrc;

UPDATE buff_infos b
  JOIN tmp_10_channel_buffs c ON c.Entry = b.Entry
   SET b.Duration = c.ClientSeconds
 WHERE COALESCE(b.Duration, 0) = c.OldAbl;

DROP TEMPORARY TABLE tmp_10_channel_buffs;

COMMIT;

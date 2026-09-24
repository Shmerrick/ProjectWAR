-- 11_preserve_ability_cooldown_milliseconds.sql
-- Exact runtime cooldown storage. Apply after 00-10 and before starting the new build.
-- Evidence: extracted 1.4.8 data/bin/abilityexport.bin, uint32 LE cooldown at record +4;
-- each row below cites the absolute cooldown offset. Ability ID is at record +40.
-- Client UI easystem_tooltips/source/abilitytooltips.lua:100-108 explicitly renders
-- fractional cooldowns (including 1.5 seconds). No new ability identities are created.
-- Legacy Cooldown seconds remain intact. Initialize the new nullable ms column once;
-- NULL is also a runtime fallback for future legacy-authored rows. Guard client repairs
-- with each table's prior seconds value. Preserve slower server-authored AI pacing.
-- No character schema change. Item packets and in-memory item deadlines still use seconds;
-- runtime ability/item timers and modifiers preserve ms. Restart after application.
USE war_world;
SET @missing := (SELECT COUNT(*) = 0 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'abilities' AND COLUMN_NAME = 'CooldownMilliseconds');
SET @ddl := IF(@missing, 'ALTER TABLE abilities ADD COLUMN CooldownMilliseconds INT NULL DEFAULT NULL AFTER Cooldown', 'DO 0');
PREPARE ddl FROM @ddl; EXECUTE ddl; DEALLOCATE PREPARE ddl;
SET @missing := (SELECT COUNT(*) = 0 FROM information_schema.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'mythic_src_abilities' AND COLUMN_NAME = 'CooldownMilliseconds');
SET @ddl := IF(@missing, 'ALTER TABLE mythic_src_abilities ADD COLUMN CooldownMilliseconds INT NULL DEFAULT NULL AFTER Cooldown', 'DO 0');
PREPARE ddl FROM @ddl; EXECUTE ddl; DEALLOCATE PREPARE ddl;
START TRANSACTION;
UPDATE abilities SET CooldownMilliseconds = COALESCE(Cooldown,0) * 1000 WHERE CooldownMilliseconds IS NULL;
UPDATE mythic_src_abilities SET CooldownMilliseconds = COALESCE(Cooldown,0) * 1000 WHERE CooldownMilliseconds IS NULL;
DROP TEMPORARY TABLE IF EXISTS tmp_11_cooldowns;
CREATE TEMPORARY TABLE tmp_11_cooldowns (Entry SMALLINT UNSIGNED PRIMARY KEY, ClientMs INT NOT NULL, OldLegacy INT NULL, OldMythic INT NULL);
INSERT INTO tmp_11_cooldowns (Entry,ClientMs,OldLegacy,OldMythic) VALUES
(101,500,NULL,0), -- abilityexport.bin byte 196393
(104,500,NULL,0), -- abilityexport.bin byte 196975
(107,500,NULL,0), -- abilityexport.bin byte 197557
(110,500,NULL,0), -- abilityexport.bin byte 198139
(113,500,NULL,0), -- abilityexport.bin byte 198721
(116,500,NULL,0), -- abilityexport.bin byte 199303
(119,500,NULL,0), -- abilityexport.bin byte 199885
(1353,1500,1,1), -- abilityexport.bin byte 1082978
(3013,2500,NULL,0), -- abilityexport.bin byte 2340
(3355,4500,NULL,0), -- abilityexport.bin byte 56920
(3401,4500,NULL,0), -- abilityexport.bin byte 65844
(3402,4500,NULL,0), -- abilityexport.bin byte 66038
(3403,4500,NULL,0), -- abilityexport.bin byte 66232
(3404,4500,NULL,0), -- abilityexport.bin byte 66426
(3405,4500,NULL,0), -- abilityexport.bin byte 66620
(3406,4500,NULL,0), -- abilityexport.bin byte 66814
(3407,4500,NULL,0), -- abilityexport.bin byte 67008
(3408,4500,NULL,0), -- abilityexport.bin byte 67202
(3409,4500,NULL,0), -- abilityexport.bin byte 67396
(4437,999,NULL,0), -- abilityexport.bin byte 1430403
(4908,5500,NULL,0), -- abilityexport.bin byte 1472892
(4976,1500,NULL,0), -- abilityexport.bin byte 1480008
(4977,3500,NULL,0), -- abilityexport.bin byte 1480202
(5011,3500,NULL,0), -- abilityexport.bin byte 1486637
(5598,5500,0,0), -- abilityexport.bin byte 1597741
(8004,4500,4,4), -- abilityexport.bin byte 1175319
(8006,4500,4,4), -- abilityexport.bin byte 1175707
(8008,4500,4,4), -- abilityexport.bin byte 1176095
(8015,4500,4,4), -- abilityexport.bin byte 1177618
(8020,4500,4,4), -- abilityexport.bin byte 1178654
(8022,4500,4,4), -- abilityexport.bin byte 1179042
(8030,4500,4,4), -- abilityexport.bin byte 1180078
(8033,4500,4,4), -- abilityexport.bin byte 1180660
(8036,4500,4,4), -- abilityexport.bin byte 1181242
(8316,4500,4,4), -- abilityexport.bin byte 1219426
(8318,4500,4,4), -- abilityexport.bin byte 1219814
(8321,4500,4,4), -- abilityexport.bin byte 1220396
(8327,4500,4,4), -- abilityexport.bin byte 1221692
(8332,4500,4,4), -- abilityexport.bin byte 1222695
(8334,4500,4,4), -- abilityexport.bin byte 1223083
(8342,4500,4,4), -- abilityexport.bin byte 1224280
(8345,4500,4,4), -- abilityexport.bin byte 1224895
(8348,4500,4,4), -- abilityexport.bin byte 1225510
(9338,1500,1,1), -- abilityexport.bin byte 1315742
(12802,1999,1999,1999), -- abilityexport.bin byte 1652775
(12803,1999,1999,1999), -- abilityexport.bin byte 1652969
(13316,6250,6,6), -- abilityexport.bin byte 1722202
(13857,1500,1500,1500), -- abilityexport.bin byte 1819173
(13951,1100,1100,1100); -- abilityexport.bin byte 1832625
UPDATE abilities a JOIN tmp_11_cooldowns c ON c.Entry=a.Entry SET a.AICooldown=GREATEST(COALESCE(a.AICooldown,0),c.OldLegacy), a.CooldownMilliseconds=c.ClientMs WHERE a.CooldownMilliseconds=c.OldLegacy*1000;
UPDATE mythic_src_abilities a JOIN tmp_11_cooldowns c ON c.Entry=a.Entry SET a.AICooldown=GREATEST(COALESCE(a.AICooldown,0),c.OldMythic), a.CooldownMilliseconds=c.ClientMs WHERE a.CooldownMilliseconds=c.OldMythic*1000;
DROP TEMPORARY TABLE tmp_11_cooldowns;
COMMIT;

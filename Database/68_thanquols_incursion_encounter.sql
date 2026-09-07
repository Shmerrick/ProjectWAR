-- 68_thanquols_incursion_encounter.sql
--
-- Thanquol's Incursion (zone 410) encounter data.
--
-- SOURCE OF AUTHORITY: the official 1.4.8 packet corpus at
--   D:\Repos\Shmerrick\WAR-RE-Toolkit\libs\protocolservices\Packet Logs
-- specifically the three captures that contain the full run:
--   Tanquollincursion.txt.gz
--   THANQUOL INCURSION (FULL RUN WITH EMPTY IGNORE LIST).log.txt.gz
--   thanquollfull+RvR.txt.gz
-- Every string, objective count, stage order, stage timer and object position below was
-- decoded from F_OBJECTIVE_INFO (0xC1), F_CREATE_STATIC (0x14) and F_CREATE_MONSTER (0x0F)
-- in those captures. Nothing here is taken from Return of Reckoning.
--
-- The live server drives this zone as a PUBLIC QUEST (PQ entry 911, 0x038F), not as a
-- boss_spawn instance encounter. The capture shows a Setup stage followed by five numbered
-- stages:
--   Setup      - 300s, no objectives      "Take a moment to collect yourself"
--   Stage I    - destroy 2 of 4 contraptions
--   Stage II   - defeat Warlock Engineer Skeetk
--   Stage III  - destroy all 4 contraptions
--   Stage IV   - defeat Throt the Unclean
--   Stage V    - defeat Thanquol
--
-- COORDINATES. Captured object positions are client-space:
--   clientX = worldX - (zone.OffX << 12) + (instanceShiftX << 13)
-- Zone 410 has OffX = OffY = 16 and the captures ran with instanceShift 1 on both axes,
-- so worldX = clientX + 57344 and worldY = clientY + 57344. The stored zone-410 spawns
-- carry a further constant +53 / -52, which reproduces the existing rows exactly
-- (Boneripper capture 25746,26018 -> stored 83143,83310). The contraption rows below use
-- the same transform so the new data lands in the same frame as the spawns already there.
--
-- Two new columns are added to pquest_objectives; both default to the previous behaviour,
-- so existing PQ rows are unaffected:
--   StageTitle        - the long stage title the client shows in the tracker header
--                       ("Destroy Siphoning Contraptions"). StageName stays the short
--                       label ("Stage I"). The captures show the packet carries BOTH.
--   ClientObjectiveId - the ephemeral objective id the live server sent in
--                       F_OBJECTIVE_INFO. It is NOT the creature/gameobject entry:
--                       Gunbad PQ 181 sends 870/871 while its ObjectId column holds
--                       creature 15106. Zero keeps the old behaviour (send ObjectId).

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 1. Schema
-- ---------------------------------------------------------------------------

SET @col := (SELECT COUNT(*) FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'pquest_objectives'
               AND COLUMN_NAME = 'StageTitle');
SET @sql := IF(@col = 0,
    'ALTER TABLE pquest_objectives ADD COLUMN StageTitle VARCHAR(255) NOT NULL DEFAULT '''' AFTER StageName',
    'SELECT 1');
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;

SET @col := (SELECT COUNT(*) FROM information_schema.COLUMNS
             WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'pquest_objectives'
               AND COLUMN_NAME = 'ClientObjectiveId');
SET @sql := IF(@col = 0,
    'ALTER TABLE pquest_objectives ADD COLUMN ClientObjectiveId INT UNSIGNED NOT NULL DEFAULT 0 AFTER ObjectId6',
    'SELECT 1');
PREPARE s FROM @sql; EXECUTE s; DEALLOCATE PREPARE s;

-- ---------------------------------------------------------------------------
-- 2. Siphoning Contraption gameobject prototype
--
-- DisplayID 7454 read from F_CREATE_STATIC. Modelled on the Gunbad Nursery Slime
-- (proto 100515), the only other destructible PQ gameobject in the world database:
-- HealthPoints 1 and IsAttackable 0, with the client-side "attackable" bit carried in
-- the spawn's Unks field rather than in the prototype. Entry 100517 was free.
-- ---------------------------------------------------------------------------

INSERT INTO gameobject_protos
    (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock,
     Unk1, Unk2, Unk3, Unk4, CreatureSpawnText, UnksString, CreatureId, CreatureCount,
     CreatureCooldownMinutes, IsAttackable)
VALUES
    (100517, 'Siphoning Contraption', 7454, 50, 40, 0, 1, NULL, NULL,
     0, 0, 100, 0, NULL, '0', NULL, NULL, NULL, 0)
ON DUPLICATE KEY UPDATE Name = VALUES(Name), DisplayID = VALUES(DisplayID);

-- ---------------------------------------------------------------------------
-- 3. Public quest 911
--
-- PQType 0 and PQDifficult 0 are deliberate, not placeholders:
--   * PQDifficult 0 makes the difficulty byte serialise as 0xFF, which is what all six
--     captured F_OBJECTIVE_INFO packets carry.
--   * PQType 0 selects no gold-chest bag roll. This encounter's rewards are the
--     "Warpstone Supplies" containers looted from the three bosses (see the capture
--     "WARPSTONE SUPPLIES (SKEETK'S, THROT'S AND THANQUOL'S)_ITEMS IN BACKPACK"),
--     not a PQ loot bag, so GoldChestWorld* stay zero.
-- PinX/PinY are the boss pad, zone-local (world 83240/83275 minus OffX/OffY << 12).
-- ---------------------------------------------------------------------------

INSERT INTO pquest_info
    (Entry, Name, Type, Level, ZoneId, PinX, PinY, TokDiscovered, TokUnlocked, ChapterId,
     GoldChestWorldX, GoldChestWorldY, GoldChestWorldZ, PQType, PQDifficult, Chapter,
     PQTier, PQCraftingBag, PQAreaId, SoundPQEnd, RespawnID)
VALUES
    (911, 'Thanquol\'s Incursion', 0, 40, 410, 17704, 17739, 0, 0, 0,
     0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0)
ON DUPLICATE KEY UPDATE Name = VALUES(Name), ZoneId = VALUES(ZoneId);

-- ---------------------------------------------------------------------------
-- 4. Stages
--
-- Type 12 = QUEST_SCRIPTED_EVENT (Setup: no objectives, advances on its 300s timer)
-- Type 11 = QUEST_KILL_GO        (contraptions)
-- Type  2 = QUEST_KILL_MOB       (bosses)
--
-- ClientObjectiveId values 2531..2536 are the ids the live server sent.
-- ---------------------------------------------------------------------------

DELETE FROM pquest_spawns     WHERE Objective BETWEEN 185010 AND 185015;
DELETE FROM pquest_objectives WHERE Entry = 911;

INSERT INTO pquest_objectives
    (Guid, Entry, StageName, StageTitle, StageId, Type, Objective, Count, Description,
     ObjectId, ClientObjectiveId, TokCompleted, Time, NoRespawn, RespawnSeconds,
     SoundId, SoundDelay, SoundIteration)
VALUES
    (185010, 911, 'Setup',     'Setup',                              1, 12,
     'Setup', 1,
     'Take a moment to collect yourself',
     '0', 2531, 0, 300, 0, 0, 0, 1, 1),

    (185011, 911, 'Stage I',   'Destroy Siphoning Contraptions',     2, 11,
     'Siphoning Contraptions Destroyed', 2,
     'Those infernal Skaven machines are diverting resources from the surface! They must be stopped - Destroy them immediately!',
     '100517', 2532, 0, 0, 0, 0, 0, 1, 1),

    (185012, 911, 'Stage II',  'Dispatch Warlock Engineer Skeetk',   3, 2,
     'Warlock Engineer Skeetk', 1,
     'Make an example of this foul creature.',
     '99621', 2533, 0, 0, 0, 0, 0, 1, 1),

    (185013, 911, 'Stage III', 'Destroy the Siphoning Contraptions', 4, 11,
     'Siphoning Contraptions Destroyed', 4,
     'Finish what you started, and make sure all four of these warp-fueled monstrosities cease to function!',
     '100517', 2534, 0, 0, 0, 0, 0, 1, 1),

    (185014, 911, 'Stage IV',  'Dispatch Throt the Unclean',         5, 2,
     'Throt the Unclean', 1,
     'Make an example of this foul creature.',
     '99623', 2535, 0, 0, 0, 0, 0, 1, 1),

    (185015, 911, 'Stage V',   'Dispatch Thanquol',                  6, 2,
     'Thanquol', 1,
     'Make an example of this foul creature.',
     '99624', 2536, 0, 0, 0, 0, 0, 1, 1);

-- ---------------------------------------------------------------------------
-- 5. Siphoning Contraption spawns
--
-- All four contraptions are present for both contraption stages: the capture creates
-- four F_CREATE_STATIC objects at once, Stage I asks for two of them, and Stage III
-- ("make sure all four ... cease to function") asks for all four after the stage reset
-- respawns them.
--
-- Unks is GetUnk(0..5). GetUnk(0) = 15360 and GetUnk(2) = 20957 are constant across
-- every captured contraption. GetUnk(3) is 0x008 (the client "attackable" bit, the same
-- bit the Gunbad Nursery Slime spawns set) OR'd with a per-position variant in bits 8-9
-- -- 0x000, 0x200, 0x300, 0x100 -- plus 0x001, which the captures show flipping between
-- runs and is therefore live state rather than stored data. GetUnk(4)/(5) differ in all
-- three captures and are runtime values, so they are zero here.
--
--   client (X,Y,Z,O)         -> world (X,Y)
--   27203, 27079, 8698,  512 -> 84600, 84371
--   24606, 24633, 9275, 3777 -> 82003, 81925
--   23263, 27433, 8506, 3083 -> 80660, 84725
--   28014, 23130, 9275, 3879 -> 85411, 80422
-- ---------------------------------------------------------------------------

INSERT INTO pquest_spawns
    (pquest_spawns_ID, Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, Objective, Type,
     Emote, Level, Ward, SoundId, VfxState, AllowVfxUpdate, CaptureDuration, Unks, Unk3)
VALUES
    ('thanquol-contraption-s1-1', 100517, 410, 84600, 84371, 8698,  512, 185011, 2, 0, 40, 0, 0, 0, 1, 0, '15360 1 20957 9 0 0',   100),
    ('thanquol-contraption-s1-2', 100517, 410, 82003, 81925, 9275, 3777, 185011, 2, 0, 40, 0, 0, 0, 1, 0, '15360 1 20957 521 0 0', 100),
    ('thanquol-contraption-s1-3', 100517, 410, 80660, 84725, 8506, 3083, 185011, 2, 0, 40, 0, 0, 0, 1, 0, '15360 1 20957 777 0 0', 100),
    ('thanquol-contraption-s1-4', 100517, 410, 85411, 80422, 9275, 3879, 185011, 2, 0, 40, 0, 0, 0, 1, 0, '15360 1 20957 265 0 0', 100),
    ('thanquol-contraption-s3-1', 100517, 410, 84600, 84371, 8698,  512, 185013, 2, 0, 40, 0, 0, 0, 1, 0, '15360 1 20957 9 0 0',   100),
    ('thanquol-contraption-s3-2', 100517, 410, 82003, 81925, 9275, 3777, 185013, 2, 0, 40, 0, 0, 0, 1, 0, '15360 1 20957 521 0 0', 100),
    ('thanquol-contraption-s3-3', 100517, 410, 80660, 84725, 8506, 3083, 185013, 2, 0, 40, 0, 0, 0, 1, 0, '15360 1 20957 777 0 0', 100),
    ('thanquol-contraption-s3-4', 100517, 410, 85411, 80422, 9275, 3879, 185013, 2, 0, 40, 0, 0, 0, 1, 0, '15360 1 20957 265 0 0', 100);

COMMIT;

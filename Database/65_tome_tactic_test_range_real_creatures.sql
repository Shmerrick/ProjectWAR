-- 65_tome_tactic_test_range_real_creatures.sql
--
-- Replaces the invented dummy protos (migrations 60, 62, 63, 64) with spawn rows for creatures
-- that already exist and work in the world. No new prototypes: nothing here is authored by hand,
-- so there are no unvalidated Model, Flag, Icone, States or FigLeafData values to get wrong.
-- Those were the cause of the dummies being first invisible and then untargetable -- all of it came
-- from copying "Practice Target" (36987), a proto that is not spawned anywhere and was therefore
-- never proven to work.
--
-- One real creature per tome tactic line, chosen for: the right CreatureType (which is what selects
-- the tactic line), Faction 1 -- neutral so both realms can attack it, and odd so Unit.SetFaction
-- marks it Aggressive and gives it an AggressiveBrain, keeping the aggro-range tactics testable --
-- and the highest live spawn count available, so each is demonstrably a working world creature.
--
--   Line       Type  Entry    Creature                     live spawns elsewhere
--   Daemonic    10   42814    Pestilent Tentacle                 109
--   Beastial     1    2485    Lionmarch Hunter                   144
--   Giant       21    1703    Bubor                                1  (only Faction 1 giant)
--   Greenskin   15   20842    Mush 'unta Squig                    75
--   Chaos       11    1921    Mottled Gor                         46
--   Mythical    22   15688    Sootwing Woodspirit                 30
--   Man         16    6956    Drakk Hellcat                       84
--   Skaven      18    6947    Burrowing Fiend                     62
--   Undead      27   33584    Skeletal Warrior                    97
--   Control      0 2000701    Skulltaker Rager                    38  (no line -- the baseline)
--
-- The control has CreatureType 0, so no tactic can match it. It is the measurement tool: +-5% is
-- inside normal variance on a single hit, so the only reliable read is the same ability at the same
-- level against a typed creature versus this one.
--
-- Levels are set to 40 on the spawn row, which overrides the prototype's own range, so every one is
-- an even fight for a rank 40 character regardless of what the source creature normally is.
--
-- Placement is unchanged: a line of ten 200 units apart at Y 921987, 400-1,000 units from the
-- Dragonslayer Ridge rally point in Thunder Mountain, where nothing else spawns within ~1,900.
--   Reach them with:  .teleport map 5 1423580 921987 11264
--
-- These creatures keep their normal damage and health, so they fight back properly. That is what
-- makes the damage-taken, defend-chance and aggro-range tactics testable at all.
--
-- Re-runnable: removes the invented protos and every spawn this range owns before inserting.

START TRANSACTION;

-- Remove the hand-authored dummies from migrations 60/62/63/64 entirely.
DELETE FROM `creature_spawns` WHERE `Entry` BETWEEN 999500 AND 999509;
DELETE FROM `creature_protos` WHERE `Entry` BETWEEN 999500 AND 999509;

-- Remove any previous run of this range (identified by position, since the entries are shared
-- with real creatures elsewhere in the world and must not be deleted globally).
DELETE FROM `creature_spawns`
 WHERE `ZoneId` = 5 AND `WorldY` = 921987 AND `WorldZ` = 11264
   AND `WorldX` BETWEEN 1422680 AND 1424480;

INSERT INTO `creature_spawns`
 (`Entry`,`ZoneId`,`WorldX`,`WorldY`,`WorldZ`,`WorldO`,`Icone`,`Emote`,`Faction`,`WaypointType`,
  `Level`,`Ward`,`Oid`,`RespawnMinutes`,`Enabled`)
VALUES
 (  42814,5,1422680,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Daemonic  (type 10)
 (   2485,5,1422880,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Beastial  (type 1)
 (   1703,5,1423080,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Giant     (type 21)
 (  20842,5,1423280,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Greenskin (type 15)
 (   1921,5,1423480,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Chaos     (type 11)
 (  15688,5,1423680,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Mythical  (type 22)
 (   6956,5,1423880,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Man       (type 16)
 (   6947,5,1424080,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Skaven    (type 18)
 (  33584,5,1424280,921987,11264,2048,18,0,0,0,40,0,0,4,1), -- Undead    (type 27)
 (2000701,5,1424480,921987,11264,2048,18,0,0,0,40,0,0,4,1); -- Control   (type 0, no line)

COMMIT;

-- Verification:
--   SELECT s.WorldX, s.Entry, p.Name, p.CreatureType, p.Faction
--     FROM creature_spawns s JOIN creature_protos p ON p.Entry = s.Entry
--    WHERE s.ZoneId = 5 AND s.WorldY = 921987 ORDER BY s.WorldX;          -- 10 rows
--   SELECT COUNT(*) FROM creature_protos WHERE Entry BETWEEN 999500 AND 999509;  -- 0
--   -- every tactic line must be covered exactly once:
--   SELECT l.Name, p.Entry FROM tome_tactic_lines l
--     JOIN tome_tactic_line_creature_types c ON c.AcId = l.AcId
--     JOIN creature_spawns s ON s.ZoneId = 5 AND s.WorldY = 921987
--     JOIN creature_protos p ON p.Entry = s.Entry AND p.CreatureType = c.CreatureType
--    ORDER BY l.AcId;                                                     -- 9 rows

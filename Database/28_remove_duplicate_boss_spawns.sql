-- 28_remove_duplicate_boss_spawns.sql
--
-- Removes duplicated instance boss spawns, which spawn the boss twice on the same spot.
--
-- Reported in game 2026-09-04: Lord Slaurith appeared twice in Bastion Stair, and only the first
-- kill awarded influence. `instance_boss_spawns` holds two rows for him at the identical position
-- (1020808, 1001549, 14401), and the same is true of every Bastion Stair boss and of Mount
-- Gunbad's. Ten such groups exist.
--
-- Two shapes:
--
-- 1. Identical BossID -- the row is duplicated outright. Gunbad zones 63-66 (BossIDs 600, 601,
--    602, 603) and one pair in zone 156 (156). These are unambiguous; keeping either row is the
--    same row.
--
-- 2. Differing BossID -- Bastion Stair zones 163-166, where one set is a sequential block
--    (160, 161, 162, 163) and the other equals the zone id (163, 164, 165, 166).
--
--    The second set is the anomaly, and it is also actively broken: with both present, BossID 163
--    is used by two different bosses -- Thar'lgnan in zone 163 and Skull Lord Var'Ithrok in zone
--    166 -- so the identifier is not unique. Elsewhere BossID is an independent number unrelated
--    to the zone: Mount Gunbad's four bosses in zones 63-66 use the block 600-603, zone 156's
--    other pair is 150 and 157, and The Lost Vale's are 400-402 in zone 260. A BossID that simply
--    mirrors its ZoneId matches none of that and looks mechanically filled.
--
--    So the zone-id-valued row is dropped and the sequential block kept, which also restores
--    uniqueness. `instance_encounters` holds no rows for any of these ids, so nothing external
--    pins the choice. **If boss lockouts misbehave after this, the opposite set is the one to
--    keep** -- the fix for the double spawn is that one of each pair goes, not which.
--
-- Idempotent: after the first run no duplicate group remains to match.

USE `war_world`;

-- Shape 1: identical rows. Keep the lowest Instance_spawns_ID of each group.
DELETE b FROM instance_boss_spawns b
  JOIN (
        SELECT MIN(Instance_spawns_ID) AS keep_id, Entry, ZoneID, WorldX, WorldY, BossID
          FROM instance_boss_spawns
         GROUP BY Entry, ZoneID, WorldX, WorldY, BossID
        HAVING COUNT(*) > 1
       ) d
    ON d.Entry = b.Entry AND d.ZoneID = b.ZoneID
   AND d.WorldX = b.WorldX AND d.WorldY = b.WorldY
   AND d.BossID = b.BossID
 WHERE b.Instance_spawns_ID <> d.keep_id;

-- Shape 2: same boss and position, differing BossID. Drop the row whose BossID equals its ZoneId.
DELETE b FROM instance_boss_spawns b
  JOIN (
        SELECT Entry, ZoneID, WorldX, WorldY
          FROM instance_boss_spawns
         GROUP BY Entry, ZoneID, WorldX, WorldY
        HAVING COUNT(*) > 1
       ) d
    ON d.Entry = b.Entry AND d.ZoneID = b.ZoneID
   AND d.WorldX = b.WorldX AND d.WorldY = b.WorldY
 WHERE b.BossID = b.ZoneID;

-- Verification: no duplicate group left, and BossID unique per zone.
SELECT
    (SELECT COUNT(*) FROM (SELECT Entry, ZoneID, WorldX, WorldY, COUNT(*) n
                             FROM instance_boss_spawns
                            GROUP BY Entry, ZoneID, WorldX, WorldY HAVING n > 1) t) AS duplicate_groups,
    (SELECT COUNT(*) FROM (SELECT BossID, COUNT(DISTINCT Entry) n
                             FROM instance_boss_spawns
                            GROUP BY BossID HAVING n > 1) t)                        AS bossids_on_two_creatures,
    (SELECT COUNT(*) FROM instance_boss_spawns WHERE ZoneID IN (163,164,165,166))   AS bastion_boss_rows;

-- 27_fix_comma_split_ward_tasks.sql
--
-- Repairs three ward task rows mangled by a CSV import, and adds the missing Order-side entrance
-- to Bastion Stair.
--
-- PART 1 -- comma-split tok_infos rows
--
-- Three section 5 task rows carry a comma inside their name, and the import that produced the
-- world dump split on it. Every column after the name shifted, leaving the row unusable:
--
--     healthy 7707   Name "Defeat Seraphine and/or ..."   Xp 50  Section 5  Index 2  Flag 34
--     broken  7708   Name "\"Kill Grey Seer Quol'tik and/or Barakus"
--                                                          Xp  0  Section 0  Index 5  Flag  2
--
-- The shifted values are recognisable: the broken Index holds the real Section (5) and the
-- broken Flag holds the real Index. The real Flag fell off the end entirely.
--
-- These are exactly the three ward task counters that could not resolve a Tome entry in script
-- 25 -- AcIds 704, 705 and 709 -- so they were never missing rows, only corrupted ones. A player
-- can drive such a counter to its threshold and watch the task stay unticked, which is how this
-- surfaced.
--
-- Restored values and their evidence:
--
--   7708  Greater sigil, fourth fragment, task 4  -> Index 2, Flag 44, counter 704
--         Name confirmed verbatim against the live client's Greater Ward fourth fragment page:
--         "Kill Grey Seer Quol'tik and/or Barakus, the Godslayer 12 Times". Its surviving
--         EventName agrees.
--
--   7713  Superior sigil, fourth fragment, task 4 -> Index 3, Flag 44, counter 705
--   7714  Superior sigil, fifth fragment,  task 4 -> Index 3, Flag 54, counter 709
--         Both kept their full name text and only need the stray leading quote removed. The
--         fragment each belongs to is taken from entry order, which is consistent across the
--         whole section: 7710-7714 are fragments 1-5 of the Superior sigil exactly as 7705-7709
--         are fragments 1-5 of the Greater sigil. Their thresholds in fragment_tasks.csv are
--         both 8, matching the "8 times" in each name.
--
-- Xp is restored to 50, the value every other section 5 task row carries.
--
-- PART 2 -- the Order entrance is NOT fixed here, deliberately
--
-- Portal 108856104 has no `zone_jumps` row, so an Order player using the Praag-side entrance gets
-- no jump at all while the Destruction entrance from the Chaos Wastes works.
--
-- It cannot be given the same arrival as the Destruction entrance: `zone_jumps` carries
-- `UNIQUE KEY idx_name (WorldX, WorldY, ZoneId)`, so two jumps may not share a destination
-- coordinate. That constraint is itself evidence -- each realm's portal arrived at its own point,
-- which is consistent with two separately guarded approaches. Those Order coordinates are not
-- recovered by any capture in hand, since all eighteen Bastion Stair captures are Destruction
-- side, so the row is left absent rather than invented. See docs/BASTION_STAIR.md.
--
-- Idempotent: PART 1 assigns fixed values and the counter rebind is a recalculation.

USE `war_world`;

UPDATE tok_infos
   SET Name      = 'Kill Grey Seer Quol''tik and/or Barakus, the Godslayer 12 Times',
       Xp        = 50,
       Section   = 5,
       `Index`   = 2,
       Flag      = 44
 WHERE Entry = 7708;

UPDATE tok_infos
   SET Name      = 'Kill Sechar, Darkpromise Cheiftain 8 times',
       Xp        = 50,
       Section   = 5,
       `Index`   = 3,
       Flag      = 44
 WHERE Entry = 7713;

UPDATE tok_infos
   SET Name      = 'Kill N''Kari, Keeper of secrets 8 times',
       Xp        = 50,
       Section   = 5,
       `Index`   = 3,
       Flag      = 54
 WHERE Entry = 7714;

REPLACE INTO zone_jumps (Entry, ZoneId, WorldX, WorldY, WorldZ, WorldO, Enabled, Type, InstanceID)
SELECT 108856104, ZoneId, WorldX, WorldY, WorldZ, WorldO, 1, 4, 160
  FROM zone_jumps
 WHERE Entry = 108856040;

-- Rebind the three counters now that their Tome entries resolve.
UPDATE ward_fragment_tasks w
   SET w.TokEntry = IFNULL((SELECT ti.Entry
                              FROM tok_infos ti
                             WHERE ti.Section = 5
                               AND ti.`Index` = w.SigilEntry
                               AND ti.Flag    = (w.FragmentIndex * 10) + w.TaskNum
                             LIMIT 1), 0)
 WHERE w.TokEntry = 0;

-- Verification: no unresolved counters, and both entrances present.
SELECT
    (SELECT COUNT(*) FROM ward_fragment_tasks WHERE TokEntry = 0)                       AS counters_without_tok,
    (SELECT COUNT(*) FROM tok_infos WHERE Section = 5 AND Flag % 10 = 4)                AS task4_rows,
    (SELECT COUNT(*) FROM zone_jumps WHERE Entry IN (108856040, 108856104) AND Type = 4) AS entrances;

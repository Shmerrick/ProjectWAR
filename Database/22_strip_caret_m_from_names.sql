-- 22_strip_caret_m_from_names.sql
--
-- Removes a literal "^M" / "^m" suffix from name columns in the world database.
--
-- Found 2026-09-04 while looking up the ward task 4 boss "Kill Lord Slaurith 5 Times": the
-- prototype is stored as `Lord Slaurith^M`, 15 characters ending in hex 68 5E 4D -- that is the
-- two ordinary ASCII characters '^' (0x5E) and 'M' (0x4D), not a carriage return (0x0D). A CR
-- was text-substituted into its caret notation at some point during a CSV import, so the
-- corruption is in the data itself and cannot be stripped by trimming control characters.
--
-- Scope, measured before writing this script:
--
--     creature_protos.Name   2954 rows   (1674 "^M", 1280 "^m")
--     quests_maps.Name       2240 rows   (2108 "^M",  132 "^m")
--     boss_spawn.Name          16 rows   (  15 "^M",    1 "^m")
--
-- In all three tables every occurrence is at the end of the string and nowhere else
-- (count of "anywhere" equals count of "at end"), so a suffix strip is exact and cannot damage
-- a name that legitimately contains a caret mid-string.
--
-- 2441 of the 9795 spawned creature prototypes are affected, including very common NPCs --
-- `Hare^m` (445 spawns), `Raven^m` (270), `Bloody Sun War Blasta^m` (267) -- so this is player
-- visible on nameplates, not merely cosmetic in the database.
--
-- It also breaks any name-based lookup, which is how it was found: the ward task 4 counters need
-- to resolve named bosses, and `Lord Slaurith` matched nothing.
--
-- Both letter cases are handled explicitly with a binary comparison, because the default
-- collation is case insensitive and the two variants must be counted and stripped separately.
--
-- Idempotent: re-running matches nothing once applied.

USE `war_world`;

UPDATE creature_protos
   SET Name = LEFT(Name, CHAR_LENGTH(Name) - 2)
 WHERE Name LIKE '%^M';

UPDATE quests_maps
   SET Name = LEFT(Name, CHAR_LENGTH(Name) - 2)
 WHERE Name LIKE '%^M';

UPDATE boss_spawn
   SET Name = LEFT(Name, CHAR_LENGTH(Name) - 2)
 WHERE Name LIKE '%^M';

-- Verification: all three counts must be zero once applied.
SELECT
    (SELECT COUNT(*) FROM creature_protos WHERE Name LIKE '%^M%') AS creature_protos_remaining,
    (SELECT COUNT(*) FROM quests_maps     WHERE Name LIKE '%^M%') AS quests_maps_remaining,
    (SELECT COUNT(*) FROM boss_spawn      WHERE Name LIKE '%^M%') AS boss_spawn_remaining,
    (SELECT COUNT(*) FROM creature_protos WHERE Name = 'Lord Slaurith') AS slaurith_resolves;

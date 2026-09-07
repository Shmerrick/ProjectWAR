-- 63_tome_tactic_dummy_faction.sql
--
-- Migration 60 gave the tome tactic dummies Faction 1, reasoning that Unit.SetFaction maps
-- FactionId 0 to REALMS_REALM_NEUTRAL and CombatInterface.IsEnemy is realm inequality, so a
-- neutral creature is an enemy of both realms. That is true server-side but wrong in practice:
-- the client decides attackability itself from the Faction byte in the creature's spawn packet and
-- refuses a neutral target, so the dummies reported "target not attackable" and could not be hit.
--
-- Set to Faction 65 instead: FactionId 65 / 8 = 8, which SetFaction maps to REALMS_REALM_ORDER, and
-- 65 % 8 = 1 so Aggressive is set with Rank 0 (an ordinary, non-champion creature). That makes them
-- attackable by Destruction characters, and still aggressive so the aggro-range tactics remain
-- testable.
--
-- Chosen from evidence rather than derivation: the "Oathbearer" creatures spawned a couple of
-- thousand units from these dummies use Faction 67 (also FactionId 8, Order) and are attackable by
-- a Destruction character in this exact area. 65 is the same realm and aggression with the normal
-- rank instead of 67's rank 1.
--
-- LIMITATION: this makes them attackable by DESTRUCTION only. An Order character will see them as
-- friendly. Testing the tactics from Order needs a mirrored set on a Destruction faction such as
-- 129, which is what "Target Git" (6775) uses.
--
-- Re-runnable: plain UPDATEs keyed on entry.

START TRANSACTION;

UPDATE `creature_protos` SET `Faction` = 65 WHERE `Entry` BETWEEN 999500 AND 999509;

COMMIT;

-- Verification:
--   SELECT Entry, Name, Faction, FLOOR(Faction/8) AS FactionId, Faction % 8 AS AggroRank
--     FROM creature_protos WHERE Entry BETWEEN 999500 AND 999509 ORDER BY Entry;
--   -- FactionId must be 8 (Order) and Faction % 8 must be odd (aggressive) for every row.

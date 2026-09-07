-- 66_tome_tactic_range_order_faction.sql
--
-- The test range creatures were left on their prototypes' Faction 1 (neutral). The server would
-- allow that -- CombatInterface.IsEnemy is realm inequality and neutral differs from both realms --
-- but in practice they could not be attacked, so the client does not accept a neutral creature as a
-- valid target here. Aligned to Order instead so a Destruction character can fight them.
--
-- Faction 67: 67 / 8 = 8, which Unit.SetFaction maps to REALMS_REALM_ORDER, and 67 % 8 = 3 is odd
-- so Aggressive stays set and the aggro-range tactics remain testable. Chosen from evidence rather
-- than derivation -- the "Oathbearer" creatures a couple of thousand units from this range use
-- Faction 67 and are attackable by a Destruction character in this exact part of Thunder Mountain.
--
-- IMPORTANT: this is set on the SPAWN rows, not the prototypes. Creature.cs uses
-- `Spawn.Faction != 0 ? Spawn.Faction : Spawn.Proto.Faction`, so overriding it here affects only
-- these ten placements. Editing the prototypes would re-align every one of these creatures
-- everywhere in the world -- 144 Lionmarch Hunters, 109 Pestilent Tentacles and so on.
--
-- LIMITATION: Order alignment means only DESTRUCTION characters can attack them. For Order-side
-- testing, change these spawn rows to a Destruction faction such as 129.
--
-- Re-runnable: an UPDATE scoped to the range's coordinates.

START TRANSACTION;

UPDATE `creature_spawns`
   SET `Faction` = 67
 WHERE `ZoneId` = 5 AND `WorldY` = 921987 AND `WorldZ` = 11264
   AND `WorldX` BETWEEN 1422680 AND 1424480;

COMMIT;

-- Verification:
--   SELECT s.WorldX, s.Entry, p.Name, s.Faction AS spawn_faction, p.Faction AS proto_faction,
--          FLOOR(s.Faction/8) AS FactionId, s.Faction % 8 AS Aggro
--     FROM creature_spawns s JOIN creature_protos p ON p.Entry = s.Entry
--    WHERE s.ZoneId = 5 AND s.WorldY = 921987 ORDER BY s.WorldX;
--   -- spawn_faction 67 and FactionId 8 on all ten; proto_faction must still read 1 (unchanged).

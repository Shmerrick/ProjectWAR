-- 46_restore_bastion_stair_pq_objects.sql
--
-- Closes the three Bastion Stair data gaps `tools/validation/Get-DungeonReadiness.ps1` reports.
--
-- Authority: the sixteen official Bastion Stair captures under
-- WAR-RE-Toolkit/libs/protocolservices/Packet Logs/ ("BASTION STAIR - * WING ..."), parsed for
-- F_CREATE_STATIC (2,644 frames) and F_CREATE_MONSTER (6,182 frames). Zone 160 is an instance,
-- so capture coordinates convert as world = client + (Off << 12) - (shift << 13) with OffX/OffY
-- 240/240 and the (1,25) atlas shift, i.e. worldX = clientX + 974848 and worldY = clientY + 778240.
--
-- USE `war_world`;

USE `war_world`;

-- 1. The Brass Legion's seal object.
--
--    Objective 1552 "Seal Broken" (Type 3, use a game object) places one object of prototype
--    100536 at world 1015831,1016402,16210, and that prototype does not exist -- an instance of
--    BUG-072, so the objective could never be satisfied.
--
--    That position converts to client 40983,238162,16210, and the nearest static in the capture
--    is **"Khornite Altar", DisplayID 902, Unk3 100, at client 40984,238164,16210 -- two units
--    away**. The altar is sighted 20 times across the captures. An altar is also what a
--    "Seal Broken" interaction objective in a Khorne dungeon should be.
--
--    Remaining columns follow the prototypes restored by migrations 38, 41 and 43.

INSERT IGNORE INTO gameobject_protos
    (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock,
     Unk1, Unk2, Unk3, Unk4, UnksString, IsAttackable)
VALUES
    (100536, 'Khornite Altar', 902, 50, 1, 0, 1, NULL, NULL, 0, 0, 100, 0, '0', 0);

UPDATE gameobject_protos SET Name = 'Khornite Altar', DisplayID = 902, Unk3 = 100 WHERE Entry = 100536;

-- 2. Trail of Carnage's Bloodherd Champion.
--
--    Objective 1518 "Bloodherd Champion" is a kill objective, but it names prototype 2000687,
--    which exists in neither creature_protos nor gameobject_protos, and its single pquest_spawns
--    row is marked Type 2 (a game object) -- so a kill objective was pointed at a non-existent
--    object. Nothing could satisfy it.
--
--    The creature itself is real and already placed: prototype 2000682 "Bloodherd Champion^m",
--    level 34, with 10 creature_spawns rows in the Bastion Stair zones. The captures sight
--    "Bloodherd Champion^m" with exactly 10 distinct object ids, also at level 34, so the
--    existing population matches the live server and only the objective's target was wrong.
--    (A second prototype, 45089 "Bloodherd Champion", carries the same name and level but has no
--    spawns anywhere; the objective is pointed at the one that is actually in the world.)
--
--    With ordinary creature kills now crediting an attached public quest, re-targeting the
--    objective is sufficient and no creature is added. The bogus game-object spawn row is
--    removed because it can never resolve.

UPDATE pquest_objectives
   SET ObjectId = '2000682'
 WHERE Guid = 1518 AND Entry = 329 AND ObjectId = '2000687';

DELETE FROM pquest_spawns WHERE Objective = 1518 AND Entry = 2000687 AND Type = 2;

-- 3. Twenty-four unusable creature rows in Thar'lgnan's boss map.
--
--    Zone 163 carries 24 creature_spawns rows for prototype 2000689, which does not exist, so
--    they spawn nothing and only produce load errors. They could not be placed even if it did:
--    zone 163 has OffX/OffY 240, so its world coordinates run 983040 to 1048576, and these rows
--    sit at X around 22,523 and Y around 143,338 -- far outside the zone, and not valid pin
--    coordinates either, since Y exceeds the 65,535 pin ceiling.
--
--    Deleted as unusable on both counts rather than repaired, because nothing identifies what
--    they were meant to be.

DELETE FROM creature_spawns WHERE ZoneId = 163 AND Entry = 2000689;

-- Verification: no Bastion Stair public-quest object should reference a missing prototype, the
-- Bloodherd Champion objective should name a creature that is actually spawned, and zone 163
-- should have no unresolvable creature rows left.
SELECT
    (SELECT COUNT(*) FROM pquest_spawns s LEFT JOIN gameobject_protos g ON g.Entry = s.Entry
      WHERE s.ZoneId = 160 AND s.Type = 2 AND g.Entry IS NULL)                       AS bastion_missing_pq_objects,
    (SELECT COUNT(*) FROM creature_spawns c LEFT JOIN creature_protos p ON p.Entry = c.Entry
      WHERE c.ZoneId = 163 AND p.Entry IS NULL)                                      AS zone163_missing_creatures,
    (SELECT ObjectId FROM pquest_objectives WHERE Guid = 1518)                       AS bloodherd_target,
    (SELECT COUNT(*) FROM creature_spawns c WHERE c.Entry = 2000682)                 AS bloodherd_spawns;

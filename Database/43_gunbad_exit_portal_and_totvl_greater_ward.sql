-- 43_gunbad_exit_portal_and_totvl_greater_ward.sql
--
-- Two faults reported from in-client testing on 2026-09-05.
--
-- USE `war_world`;

USE `war_world`;

-- 1. The Mount Gunbad wing-boss exit portal prototype.
--
--    Killing a Gunbad wing boss threw a NullReferenceException. All four boss scripts --
--    ArdtaFeed, WightLordSolithex, MastaMixa and GlompdaSquigMasta -- call
--    `BasicGunbad.CreateExitPortal`, which looks up gameobject prototype 98878 and hands it
--    straight to `GameObject_spawn.BuildFromProto`. The prototype does not exist, so the throw
--    came out of `Unit.SetDeath` and the boss was torn down mid-removal. The session log records
--    it for both bosses the user killed:
--
--      21:38:15 EXCEPTION: Wight Lord Solithex in Region 66 ... NullReferenceException
--               at BasicGunbad.CreateExitPortal ... BasicGunbad.cs:line 154
--      21:40:51 EXCEPTION: 'Ard ta Feed in Region 65 ... same frame
--
--    Identified from the official capture INSTANCE_GUNBAD_PART1.txt.gz, which contains exactly
--    one portal object among its 30 distinct statics: "Gunbad Entrance Portal", DisplayID 1583,
--    Unk3 25700, sighted three times. One of those three sits at client 35339,183668 -- fifteen
--    units from Masta Mixa's own sighting at 35339,183683 in the Gunbad Lab, which is precisely
--    where `MastaMixa.cs` calls CreateExitPortal with the boss's own spawn position. That is the
--    exit portal this code creates, caught in the capture after the boss died.
--
--    Columns other than the three the capture states follow the prototypes restored by
--    migrations 38 and 41. The code now also guards the lookup, so a future gap costs the portal
--    rather than the kill.

INSERT IGNORE INTO gameobject_protos
    (Entry, Name, DisplayID, Scale, Level, Faction, HealthPoints, ScriptName, TokUnlock,
     Unk1, Unk2, Unk3, Unk4, UnksString, IsAttackable)
VALUES
    (98878, 'Gunbad Entrance Portal', 1583, 50, 1, 0, 1, NULL, NULL, 0, 0, 25700, 0, '0', 0);

UPDATE gameobject_protos
   SET Name = 'Gunbad Entrance Portal', DisplayID = 1583, Unk3 = 25700
 WHERE Entry = 98878;

-- 2. Tomb of the Vulture Lord carries no creature ward.
--
--    Reported after the dungeon became enterable: its Greater ward is not applied. Zone 179 has
--    all 393 instance creature rows and all 10 instance boss rows at 0.
--
--    Applied at the user's direction, mirroring how The Lost Vale -- the other Greater-ward
--    dungeon, and the closest comparable content -- stores it: every instance creature row and
--    every instance boss row at Greater (2), with the zone's world `creature_spawns` rows left
--    alone. Lost Vale is 1197/1197 and 15/15 at 2, and its world rows are not uniformly warded.
--    This is a content decision, not a capture-derived restoration.

UPDATE `instance_creature_spawns` SET `Ward` = 2 WHERE `ZoneID` = 179 AND `Ward` = 0;
UPDATE `instance_boss_spawns`     SET `Ward` = 2 WHERE `ZoneID` = 179 AND `Ward` = 0;

-- Verification: the portal prototype exists, and zone 179 now reads like zone 260.
SELECT Entry, Name, DisplayID, Unk3 FROM gameobject_protos WHERE Entry = 98878;

SELECT 'inst_creature' AS source, ZoneID, COUNT(*) AS rows_total, MIN(Ward) AS min_ward, MAX(Ward) AS max_ward
  FROM instance_creature_spawns WHERE ZoneID IN (179, 260) GROUP BY ZoneID
UNION ALL
SELECT 'inst_boss', ZoneID, COUNT(*), MIN(Ward), MAX(Ward)
  FROM instance_boss_spawns WHERE ZoneID IN (179, 260) GROUP BY ZoneID;

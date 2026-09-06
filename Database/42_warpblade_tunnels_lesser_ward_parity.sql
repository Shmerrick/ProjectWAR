-- 42_warpblade_tunnels_lesser_ward_parity.sql
--
-- Gives Warpblade Tunnels the Lesser creature ward its Altdorf counterpart already carries, so
-- the two realms' city dungeon sets match.
--
-- This is a content decision made at the user's explicit direction ("I want them to show on the
-- order side too"), not a capture-derived restoration. It is recorded as such rather than
-- dressed up as evidence.
--
-- The city dungeons pair by tier, and the ward assignment is symmetric in two of the three
-- pairs and not in the third:
--
--     tier   Altdorf set                       ward   Inevitable City set              ward
--     low    Sewers of Altdorf 152/153/169     0      Sacellum Dungeons 155/156/173    0
--     mid    Sigmar Crypts 176                 1      Warpblade Tunnels 154/177        0   <-
--     high   Bilerot Burrow 196                1      Bloodwrought Enclave 195         1
--
-- The pairing is supported by the stored level ranges: Sewers 0-20 against Sacellum 1-20,
-- Sigmar Crypts 1-42 against Warpblade 1-43, Bilerot 1-43 against Bloodwrought 40-42. The
-- result is two ward-bearing dungeons in one city's set and one in the other's, which is the
-- asymmetry reported from play.
--
-- The change mirrors exactly how Sigmar Crypts carries it: every instance creature row and
-- every instance boss row at Lesser (1), with the zone's world `creature_spawns` rows left at 0
-- (Sigmar Crypts has 143/143 and 8/8 at 1, and all 138 of its world rows at 0). Sewers and
-- Sacellum are deliberately untouched: they are already symmetric at 0.
--
-- Not established, and deliberately not claimed: how the live 1.4.8 server transmitted a
-- creature's sigil. The client has a first-class display for it -- `TargetUnitFrame` reads
-- `TargetInfo:UnitSigilEntryId`, resolves it through `TomeGetSigilDisplayInfo`, and
-- `interface/interfacecore/tome/sigils/sigil_entries.csv` defines exactly five entries whose
-- ability ids are 12975-12979 -- so retail plainly did show creature wards. But this server
-- sends the tier over `F_WARD_INFO` (0xDF), and that opcode appears in none of the nine city
-- dungeon captures examined, so it is not the retail transport. Whether this row change is
-- visible therefore depends on the private ward-sigil client component; the underlying
-- transport question is tracked separately.
--
-- Idempotent: fixed assignments guarded on the value being replaced.

USE `war_world`;

UPDATE `instance_creature_spawns` SET `Ward` = 1
 WHERE `ZoneID` IN (154, 177) AND `Ward` = 0;

UPDATE `instance_boss_spawns` SET `Ward` = 1
 WHERE `ZoneID` IN (154, 177) AND `Ward` = 0;

-- Verification: Warpblade should now read the same way Sigmar Crypts does -- instance creature
-- and boss rows at 1, world rows still 0.
SELECT 'inst_creature' AS source, ZoneID, COUNT(*) AS rows_total, MIN(Ward) AS min_ward, MAX(Ward) AS max_ward
  FROM instance_creature_spawns WHERE ZoneID IN (154, 176, 177) GROUP BY ZoneID
UNION ALL
SELECT 'inst_boss', ZoneID, COUNT(*), MIN(Ward), MAX(Ward)
  FROM instance_boss_spawns WHERE ZoneID IN (154, 176, 177) GROUP BY ZoneID
UNION ALL
SELECT 'world_spawns', ZoneId, COUNT(*), MIN(Ward), MAX(Ward)
  FROM creature_spawns WHERE ZoneId IN (154, 176, 177) GROUP BY ZoneId;

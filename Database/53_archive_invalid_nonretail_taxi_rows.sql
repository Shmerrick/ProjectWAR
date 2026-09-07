-- BUG-010: five zone_taxis rows name a destination that falls outside the zone they
-- point at (zones 62, 132, 139, 168, 204). Four of them (132, 139, 168, 204) are already
-- disabled in the base dump and ZoneService.NormalizeTaxiWorldPosition hides the fifth at
-- runtime, so nothing here changes what the server offers a player.
--
-- This script only preserves the rows verbatim in an archive table so a later repair has
-- the original coordinates to work from, following the precedent set by migration 51.
-- It deliberately does NOT delete or disable anything: absence from the inspected client
-- flight catalogs and packet captures proves the destinations are wrong, not that the
-- routes should not exist. Correct coordinates still have to come from a source of truth.
--
-- Re-runnable: the archive insert is guarded and the live table is never written.
USE `war_world`;
CREATE TABLE IF NOT EXISTS zone_taxis_unresolved LIKE zone_taxis;
START TRANSACTION;
INSERT IGNORE INTO zone_taxis_unresolved
SELECT * FROM zone_taxis
WHERE (ZoneID,RealmID,WorldX,WorldY,WorldZ,WorldO) IN
 ((62,1,212579,1500746,28951,1008),
  (132,0,125258,129373,13275,1763),
  (139,0,1247090,875283,14147,2525),
  (168,0,125258,129373,13275,1763),
  (204,0,1254816,928062,5720,932));
COMMIT;
SELECT * FROM zone_taxis_unresolved WHERE ZoneID IN(62,132,139,168,204);
SELECT COUNT(*) AS PreservedLiveRoutes FROM zone_taxis WHERE ZoneID IN(62,132,139,168,204);

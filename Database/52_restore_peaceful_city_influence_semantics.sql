-- The stock 1.4.8 client explicitly defines peaceful-city PQs as having no influence.
-- Evidence under C:/Users/Admin/Downloads/myps/interface/default/:
-- ea_alerttextwindow/source/alerttextwindow.lua:528-533 says peaceful cities have
-- PQs but no influence and intentionally do not have influenceids.csv definitions.
-- ea_objectivetrackers/source/publicquesttrackerwindow.lua:819-823 suppresses the
-- influence bar in GameDefs.PeacefulCityZoneIDs; the peaceful zones are 161/162
-- (easystem_utils/source/gamedefs.lua, PeacefulCityZoneIDs).
-- interface/interfacecore/maps/zone161 and zone162/influenceids.csv contain headers only.
-- These 42 inherited emulator bindings (280-283) incorrectly created missing-track
-- diagnostics and attempted awards in peaceful cities. Contested/siege maps are excluded.
-- Guard the observed values; preserve unrelated custom assignments and character history.
USE `war_world`;
START TRANSACTION;
UPDATE zone_areas SET OrderInfluenceId = 0
 WHERE ZoneId = 161 AND OrderInfluenceId = 280;
UPDATE zone_areas SET DestroInfluenceId = 0
 WHERE ZoneId = 161 AND DestroInfluenceId = 281;
UPDATE zone_areas SET OrderInfluenceId = 0
 WHERE ZoneId = 162 AND OrderInfluenceId = 282;
UPDATE zone_areas SET DestroInfluenceId = 0
 WHERE ZoneId = 162 AND DestroInfluenceId = 283;
COMMIT;
SELECT ZoneId, COUNT(*) AS AreaRows,
 SUM(COALESCE(OrderInfluenceId,0) <> 0 OR COALESCE(DestroInfluenceId,0) <> 0) AS NonzeroBindings
FROM zone_areas WHERE ZoneId IN (161,162) GROUP BY ZoneId;

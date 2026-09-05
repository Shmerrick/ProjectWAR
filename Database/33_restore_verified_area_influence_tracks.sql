-- Restore seven populated influence bindings that disagree with the exact
-- (Area Number, Realm) key in the extracted 1.4.8 client CSVs.
-- Source root: C:\Users\Admin\Downloads\myps\interface\interfacecore\maps
-- Sources (line numbers include the header):
-- zone011/influenceids.csv:4  -> area 63, realm 2, track 2
-- zone101/influenceids.csv:10 -> area 1,  realm 2, track 111
-- zone107/influenceids.csv:7  -> area 81, realm 1, track 96
-- zone120/influenceids.csv:4  -> area 2,  realm 2, track 90
-- zone120/influenceids.csv:5  -> area 3,  realm 1, track 106
-- zone209/influenceids.csv:2  -> area 61, realm 1, track 166
-- zone209/influenceids.csv:3  -> area 62, realm 1, track 167
-- Only change the observed erroneous values; preserve unrelated custom bindings.
-- NULL opposite-realm fields and client-zero/dynamic-track questions are excluded.

USE `war_world`;

START TRANSACTION;
UPDATE zone_areas SET DestroInfluenceId = 2
 WHERE ZoneId = 11 AND PieceId = 10 AND AreaId = 63 AND DestroInfluenceId = 3;
UPDATE zone_areas SET DestroInfluenceId = 111
 WHERE ZoneId = 101 AND PieceId = 1 AND AreaId = 1 AND DestroInfluenceId = 120;
UPDATE zone_areas SET OrderInfluenceId = 96
 WHERE ZoneId = 107 AND PieceId = 16 AND AreaId = 81 AND OrderInfluenceId = 95;
UPDATE zone_areas SET DestroInfluenceId = 90
 WHERE ZoneId = 120 AND PieceId = 2 AND AreaId = 2 AND DestroInfluenceId = 88;
UPDATE zone_areas SET OrderInfluenceId = 106
 WHERE ZoneId = 120 AND PieceId = 3 AND AreaId = 3 AND OrderInfluenceId = 104;
UPDATE zone_areas SET OrderInfluenceId = 166
 WHERE ZoneId = 209 AND PieceId = 5 AND AreaId = 61 AND OrderInfluenceId = 167;
UPDATE zone_areas SET OrderInfluenceId = 167
 WHERE ZoneId = 209 AND PieceId = 6 AND AreaId = 62 AND OrderInfluenceId = 166;
COMMIT;

SELECT ZoneId, PieceId, AreaId, OrderInfluenceId, DestroInfluenceId
FROM zone_areas
WHERE (ZoneId = 11 AND PieceId = 10) OR (ZoneId = 101 AND PieceId = 1)
   OR (ZoneId = 107 AND PieceId = 16) OR (ZoneId = 120 AND PieceId IN (2,3))
   OR (ZoneId = 209 AND PieceId IN (5,6))
ORDER BY ZoneId, PieceId;

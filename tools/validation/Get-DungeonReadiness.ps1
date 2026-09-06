param([string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'))
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
[xml]$config = Get-Content -LiteralPath (Join-Path $BuildRoot 'Configs/World.xml')
$db = $config.DocumentElement.WorldDatabase
if ($db.ConnectionType -ne 'DATABASE_MYSQL') { throw 'This audit requires the configured MySQL database.' }
[void][Reflection.Assembly]::LoadFrom((Join-Path $BuildRoot 'libs/MySql.Data.dll'))
$builder = New-Object MySql.Data.MySqlClient.MySqlConnectionStringBuilder
$builder.set_ConnectionString([string]$db.Custom)
$builder.set_Server([string]$db.Server)
$builder.set_Port([uint32]$db.Port)
$builder.set_Database(([string]$db.Database).Replace('%name%', 'world'))
$builder.set_UserID([string]$db.Username)
$builder.set_Password([string]$db.Password)
$connection = New-Object MySql.Data.MySqlClient.MySqlConnection($builder.get_ConnectionString())
function Read-DungeonQuery([string]$Sql) {
    $command = $connection.CreateCommand()
    $command.CommandText = $Sql
    $command.CommandTimeout = 30
    try {
        $reader = $command.ExecuteReader()
        try {
            while ($reader.Read()) {
                $row = [ordered]@{}
                for ($i = 0; $i -lt $reader.FieldCount; $i++) {
                    $row[$reader.GetName($i)] = if ($reader.IsDBNull($i)) { $null } else { $reader.GetValue($i) }
                }
                [pscustomobject]$row
            }
        } finally { $reader.Dispose() }
    } finally { $command.Dispose() }
}
try {
    $connection.Open()
    # SELECT-only snapshot of emulator data, not evidence of retail correctness or client completion.
    # Fixed scope includes Gunbad/Bastion proper and their four boss maps each.
    $quests = @(Read-DungeonQuery @'
SELECT q.ZoneId,q.Entry,q.Name,q.PQAreaId,
 (SELECT COUNT(*) FROM pquest_objectives o WHERE o.Entry=q.Entry AND o.Type<>0) AS Objectives,
 (SELECT COUNT(*) FROM pquest_spawns s JOIN pquest_objectives o ON o.Guid=s.Objective WHERE o.Entry=q.Entry AND o.Type<>0) AS Spawns,
 q.GoldChestWorldX,q.GoldChestWorldY,q.GoldChestWorldZ
FROM pquest_info q WHERE q.ZoneId IN (60,160) ORDER BY q.ZoneId,q.Entry
'@)
    $missing = @(Read-DungeonQuery @'
SELECT s.ZoneId,o.Entry AS Quest,s.Objective,s.Type,s.Entry AS Prototype,COUNT(*) AS SpawnRows
FROM pquest_spawns s
LEFT JOIN pquest_objectives o ON o.Guid=s.Objective
LEFT JOIN creature_protos c ON s.Type=1 AND c.Entry=s.Entry
LEFT JOIN gameobject_protos g ON s.Type=2 AND g.Entry=s.Entry
WHERE s.ZoneId IN (60,63,64,65,66,160,163,164,165,166)
 AND ((s.Type=1 AND c.Entry IS NULL) OR (s.Type=2 AND g.Entry IS NULL))
GROUP BY s.ZoneId,o.Entry,s.Objective,s.Type,s.Entry ORDER BY s.ZoneId,s.Objective
'@)
    $unattached = @(Read-DungeonQuery @'
SELECT s.ZoneId,s.Objective,o.Entry AS Quest,q.ZoneId AS QuestZone,COUNT(*) AS SpawnRows
FROM pquest_spawns s LEFT JOIN pquest_objectives o ON o.Guid=s.Objective
LEFT JOIN pquest_info q ON q.Entry=o.Entry
WHERE s.ZoneId IN (60,160) AND (q.Entry IS NULL OR q.ZoneId<>s.ZoneId OR o.Type=0)
GROUP BY s.ZoneId,s.Objective,o.Entry,q.ZoneId ORDER BY s.ZoneId,s.Objective
'@)
    $emptyObjectives = @(Read-DungeonQuery @'
SELECT q.ZoneId,q.Entry AS Quest,o.Guid,o.StageId,o.Type,o.Objective,o.Count,o.NoRespawn,
 o.ObjectId,o.ObjectId2,o.ObjectId3,o.ObjectId4,o.ObjectId5,o.ObjectId6
FROM pquest_objectives o JOIN pquest_info q ON q.Entry=o.Entry
WHERE q.ZoneId IN (60,160) AND o.Type<>0
 AND NOT EXISTS (SELECT 1 FROM pquest_spawns s WHERE s.Objective=o.Guid)
ORDER BY q.ZoneId,q.Entry,o.StageId,o.Guid
'@)
    $instances = @(Read-DungeonQuery @'
SELECT i.Entry,i.ZoneID,i.Name,i.OrderExitZoneJumpID,i.DestrExitZoneJumpID,
 oj.ZoneID AS OrderExitZone,dj.ZoneID AS DestructionExitZone
FROM instance_infos i
LEFT JOIN zone_jumps oj ON oj.Entry=i.OrderExitZoneJumpID AND oj.Entry<>0
LEFT JOIN zone_jumps dj ON dj.Entry=i.DestrExitZoneJumpID AND dj.Entry<>0
WHERE i.ZoneID IN (60,63,64,65,66,160,163,164,165,166) ORDER BY i.ZoneID
'@)
    $levels = @(Read-DungeonQuery @'
SELECT s.Source,s.ZoneId,COUNT(*) AS SpawnRows,
 SUM(c.Entry IS NULL) AS MissingPrototypes,
 MIN(CASE WHEN s.Level<>0 THEN s.Level ELSE c.MinLevel END) AS EffectiveMin,
 MAX(CASE WHEN s.Level<>0 THEN s.Level ELSE c.MaxLevel END) AS EffectiveMax,
 SUM(s.Ward<>0) AS WardedSpawns
FROM (
 SELECT 'world' AS Source,ZoneId,Entry,Level,Ward FROM creature_spawns WHERE ZoneId IN (60,63,64,65,66,160,163,164,165,166)
 UNION ALL SELECT 'instance',ZoneID,Entry,Level,Ward FROM instance_creature_spawns WHERE ZoneID IN (60,63,64,65,66,160,163,164,165,166)
 UNION ALL SELECT 'boss',ZoneID,Entry,Level,Ward FROM instance_boss_spawns WHERE ZoneID IN (60,63,64,65,66,160,163,164,165,166)
 UNION ALL SELECT 'pq',ZoneId,Entry,Level,Ward FROM pquest_spawns WHERE Type=1 AND ZoneId IN (60,63,64,65,66,160,163,164,165,166)
) s LEFT JOIN creature_protos c ON c.Entry=s.Entry
GROUP BY s.Source,s.ZoneId ORDER BY s.ZoneId,s.Source
'@)
    [pscustomobject]@{
        PublicQuests = $quests
        MissingPQPrototypes = $missing
        UnattachedOrCrossZonePQSpawns = $unattached
        ObjectivesWithoutOwnSpawns = $emptyObjectives
        InstanceExits = $instances
        StoredLevelAndWardCoverage = $levels
        Limits = 'Data audit only. Empty objective spawn sets can be scripted; investigate before treating them as bugs. Level ranges include friendly NPCs and unverified legacy rows, exclude scripted adds, and are not difficulty baselines. Client completion, loot, portals and lockouts need gameplay retests.'
    }
} finally { $connection.Dispose() }

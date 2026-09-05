param(
    [string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'),
    [string]$ExtractedRoot
)
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
function Read-HealthQuery([string]$Sql) {
    $command = $connection.CreateCommand()
    $command.CommandText = $Sql
    $command.CommandTimeout = 30
    try {
        $reader = $command.ExecuteReader()
        try {
            while ($reader.Read()) {
                $row = [ordered]@{}
                for ($i = 0; $i -lt $reader.FieldCount; $i++) { $row[$reader.GetName($i)] = $reader.GetValue($i) }
                [pscustomobject]$row
            }
        } finally { $reader.Dispose() }
    } finally { $command.Dispose() }
}
try {
    $connection.Open()
    # SELECT only: no ORM registration, migrations, character writes or service startup.
    Read-HealthQuery @'
SELECT COUNT(*) AS AreaRows,
 SUM((a.OrderInfluenceId <> 0 AND NOT EXISTS (SELECT 1 FROM chapter_infos o WHERE o.InfluenceEntry = a.OrderInfluenceId))
 OR (a.DestroInfluenceId <> 0 AND NOT EXISTS (SELECT 1 FROM chapter_infos d WHERE d.InfluenceEntry = a.DestroInfluenceId))) AS AreasWithMissingTracks
FROM zone_areas a
'@
    Read-HealthQuery @'
SELECT ZoneId, PieceId, OrderInfluenceId, DestroInfluenceId FROM zone_areas WHERE ZoneId IN (60,160) ORDER BY ZoneId,PieceId
'@
    Read-HealthQuery @'
SELECT Entry, ZoneId, Name, InfluenceEntry FROM chapter_infos WHERE Entry IN (1,2,5,6,128,129) OR InfluenceEntry IN (1,2,5,6,64,65) ORDER BY Entry
'@
    Read-HealthQuery @'
SELECT Entry, ZoneId, InfluenceEntry, Tier1InfluenceCount, Tier2InfluenceCount, Tier3InfluenceCount
FROM chapter_infos WHERE Tier3InfluenceCount > 65535 ORDER BY Entry
'@
    Read-HealthQuery @'
SELECT InfluenceEntry, COUNT(*) AS Chapters FROM chapter_infos
WHERE InfluenceEntry <> 0 GROUP BY InfluenceEntry HAVING COUNT(*) > 1
'@
    $zones = @(Read-HealthQuery 'SELECT ZoneId FROM zone_infos ORDER BY ZoneId')
    $pqZones = @(Read-HealthQuery 'SELECT ZoneId, COUNT(*) AS Quests FROM pquest_info GROUP BY ZoneId ORDER BY ZoneId')
    $zoneFolder = [string]$config.DocumentElement.ZoneFolder
    if (-not [IO.Path]::IsPathRooted($zoneFolder)) { $zoneFolder = Join-Path $BuildRoot $zoneFolder }
    $missingAreas = @($zones | Where-Object { -not (Test-Path -LiteralPath (Join-Path $zoneFolder ('zone{0:000}/areas{0:000}.png' -f [int]$_.ZoneId))) })
    $missingPQs = @($pqZones | Where-Object { -not (Test-Path -LiteralPath (Join-Path $zoneFolder ('zone{0:000}/pqarea{0:000}.png' -f [int]$_.ZoneId))) })
    [pscustomobject]@{
        ConfiguredZones = $zones.Count
        ZonesMissingAreaMap = $missingAreas.Count
        ZonesWithPQDefinitions = $pqZones.Count
        PQZonesMissingPQMap = $missingPQs.Count
        PQDefinitionsWithoutMap = ($missingPQs | Measure-Object Quests -Sum).Sum
    }
    Read-HealthQuery @'
SELECT COUNT(*) AS WardCounters, SUM(TokEntry = 0) AS UnboundWardCounters FROM ward_fragment_tasks
'@
    if ($ExtractedRoot) {
        $areas = @(Read-HealthQuery 'SELECT ZoneId, PieceId, AreaId, Realm AS AreaRealm, OrderInfluenceId, DestroInfluenceId FROM zone_areas ORDER BY ZoneId,PieceId')
        $clientMaps = @{}
        foreach ($area in $areas) {
            $zoneId = [int]$area.ZoneId
            if (-not $clientMaps.ContainsKey($zoneId)) {
                $path = Join-Path $ExtractedRoot ('interface/interfacecore/maps/zone{0:000}/influenceids.csv' -f $zoneId)
                $clientMaps[$zoneId] = if (Test-Path -LiteralPath $path) { @(Import-Csv -LiteralPath $path) } else { @() }
            }
            foreach ($realm in 1,2) {
                if ($area.AreaRealm -ne 0 -and $area.AreaRealm -ne $realm) { continue }
                $matches = @($clientMaps[$zoneId] | Where-Object { $_.'Area Number' -eq [string]$area.AreaId -and $_.Realm -eq [string]$realm })
                if ($matches.Count -ne 1) { continue }
                $expected = [uint32]$matches[0].'Influence ID'
                $actual = if ($realm -eq 1) { $area.OrderInfluenceId } else { $area.DestroInfluenceId }
                # NULL can intentionally suppress a track; report only populated disagreements.
                if ($actual -is [DBNull]) { continue }
                if ($actual -ne $expected) {
                    [pscustomobject]@{ ZoneId=$zoneId; PieceId=$area.PieceId; AreaId=$area.AreaId; Realm=$realm; ServerTrack=$actual; ClientTrack=$expected }
                }
            }
        }
    }
} finally { $connection.Dispose() }

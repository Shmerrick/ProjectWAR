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
    $columns = @('Guid','Entry','ZoneId','WorldX','WorldY','WorldZ','WorldO','Icone',
        'Emote','Faction','WaypointType','Level','Oid','RespawnMinutes','Enabled')
    $migration = Join-Path $PSScriptRoot '../../Database/51_archive_deleted_bastion_creature_placements.sql'
    $expected = @(Get-Content -LiteralPath $migration | Where-Object { $_ -match '^\(\d+,' })
    if ($expected.Count -ne 24) { throw 'Expected exactly 24 archived source records in migration 51.' }
    $actual = @(Read-DungeonQuery ('SELECT ' + ($columns -join ',') +
        ' FROM creature_spawns_unresolved WHERE Entry=2000689 AND ZoneId=163'))
    if ($actual.Count -ne 24) { throw 'Archive record count does not match migration 51.' }
    foreach ($line in $expected) {
        $values = $line.Trim('(',')',',',';').Split(',')
        if ($values.Count -ne $columns.Count) { throw 'Unexpected archive fixture column count.' }
        $row = @($actual | Where-Object { $_.Guid -eq [long]$values[0] })
        if ($row.Count -ne 1) { throw "Missing or repeated archive GUID $($values[0])." }
        for ($index = 0; $index -lt $columns.Count; $index++) {
            if ($null -eq $row[0].($columns[$index]) -or
                [long]$row[0].($columns[$index]) -ne [long]$values[$index]) {
                throw "Archive mismatch: GUID $($values[0]), column $($columns[$index])."
            }
        }
    }
    $live = @(Read-DungeonQuery 'SELECT Guid FROM creature_spawns WHERE Entry=2000689 AND ZoneId=163')
    if ($live.Count -ne 0) { throw 'Unresolved archived placements unexpectedly exist in the live table.' }

    # Exercise migration 47's actual count expression against SELECT-only derived fixtures:
    # one affected empty objective, one affected populated objective, one unrelated empty one.
    $auditSql = Get-Content -Raw (Join-Path $PSScriptRoot '../../Database/47_archive_unresolvable_pq_object_rows.sql')
    $start = $auditSql.IndexOf('(SELECT COUNT(*) FROM pquest_objectives o')
    if ($start -lt 0) { throw 'Cannot locate migration 47 audit expression.' }
    $end = $auditSql.IndexOf('AS kill_objectives_left_empty', $start)
    if ($end -lt 0) { throw 'Cannot locate migration 47 audit expression end.' }
    $expression = $auditSql.Substring($start, $end - $start)
    $expression = $expression.Replace('pquest_spawns_unresolved', '(SELECT 1 AS Objective UNION ALL SELECT 2)')
    $expression = $expression.Replace('pquest_spawns', '(SELECT 2 AS Objective)')
    $expression = $expression.Replace('pquest_objectives', '(SELECT 1 AS Guid, 2 AS Type UNION ALL SELECT 2,2 UNION ALL SELECT 3,2)')
    $result = @(Read-DungeonQuery ('SELECT ' + $expression + ' AS EmptyCount'))
    if ($result.Count -ne 1 -or $result[0].EmptyCount -ne 1) { throw 'Empty-objective audit failed its negative fixture.' }

    $schema = @(Read-DungeonQuery "SELECT COLUMN_NAME FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME='creature_spawns_unresolved'")
    foreach ($column in $columns) {
        if ($column -notin $schema.COLUMN_NAME) { throw "Archive schema missing $column." }
    }
    Write-Output 'PASS: all 24 archive records match every original column; no live placements; empty-objective audit detects the negative fixture.'
} finally { $connection.Dispose() }

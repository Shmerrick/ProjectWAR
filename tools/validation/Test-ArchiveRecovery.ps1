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
    # The 24 archived Bastion Stair placements, verbatim from the untouched base dump. These were
    # read out of the numbered migration that first archived them; that series has since been
    # folded into the base dumps, so the fixture is inlined here to keep the check independent of
    # the database it verifies. Reading the expected values from the database would be circular.
    $expected = @(
        '(1081553,2000689,163,23368,141477,8500,1069,18,0,1,0,33,2826,4,0)',
        '(1081554,2000689,163,22984,141500,8526,2867,18,0,1,0,33,2971,4,0)',
        '(1081555,2000689,163,23469,141925,8496,159,18,0,1,0,33,2847,4,0)',
        '(1081556,2000689,163,23491,142228,8484,1729,18,0,1,0,33,2957,4,0)',
        '(1081557,2000689,163,24988,141230,8566,3913,18,0,1,0,33,2958,4,0)',
        '(1081558,2000689,163,25222,141446,8560,1069,18,0,1,0,33,2960,4,0)',
        '(1081559,2000689,163,24455,142888,8540,2241,18,0,1,0,33,2959,4,0)',
        '(1081560,2000689,163,24584,142580,8508,193,18,0,1,0,33,2903,4,0)',
        '(1081561,2000689,163,22744,144334,8886,3447,18,0,1,0,33,2966,4,0)',
        '(1081562,2000689,163,23042,144481,8886,1035,18,0,1,0,33,2968,4,0)',
        '(1081563,2000689,163,22523,143338,8886,3709,18,0,1,0,33,2967,4,0)',
        '(1081564,2000689,163,22645,143587,8886,1763,18,0,1,0,33,2822,4,0)',
        '(1081565,2000689,163,25486,143607,8886,3857,18,0,1,0,33,2905,4,0)',
        '(1081566,2000689,163,25299,143713,8886,3242,18,0,1,0,33,2735,4,0)',
        '(1081567,2000689,163,24642,141999,8498,750,18,0,1,0,33,2973,4,0)',
        '(1081568,2000689,163,24381,141920,8484,3527,18,0,1,0,33,2824,4,0)',
        '(1081569,2000689,163,23799,143147,8562,227,18,0,1,0,33,2763,4,0)',
        '(1081570,2000689,163,23782,143353,8622,1956,18,0,1,0,33,2770,4,0)',
        '(1081571,2000689,163,23902,141873,8500,3606,18,0,1,0,33,2771,4,0)',
        '(1081572,2000689,163,24020,142078,8454,1820,18,0,1,0,33,2926,4,0)',
        '(1081573,2000689,163,23933,142381,8440,3891,18,0,1,0,33,2930,4,0)',
        '(1081574,2000689,163,24084,142558,8444,1251,18,0,1,0,33,2891,4,0)',
        '(1081575,2000689,163,25262,144715,8886,1649,18,0,1,0,33,2795,4,0)',
        '(1081576,2000689,163,25132,144535,8886,3697,18,0,1,0,33,2772,4,0)')
    if ($expected.Count -ne 24) { throw 'Expected exactly 24 archived source records in the fixture.' }
    $actual = @(Read-DungeonQuery ('SELECT ' + ($columns -join ',') +
        ' FROM creature_spawns_unresolved WHERE Entry=2000689 AND ZoneId=163'))
    if ($actual.Count -ne 24) { throw 'Archive record count does not match the 24-record fixture.' }
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

    # Exercise the archive audit's count expression against SELECT-only derived fixtures:
    # one affected empty objective, one affected populated objective, one unrelated empty one.
    # This expression came from the numbered migration that archived the unresolvable rows; it is
    # inlined here for the same reason as the fixture above.
    $expression = @'
(SELECT COUNT(*) FROM pquest_objectives o
      WHERE o.Type = 2
        AND EXISTS (SELECT 1 FROM pquest_spawns_unresolved a WHERE a.Objective = o.Guid)
        AND NOT EXISTS (SELECT 1 FROM pquest_spawns s WHERE s.Objective = o.Guid))
'@
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

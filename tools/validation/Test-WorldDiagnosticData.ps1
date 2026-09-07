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
 $liveSchema=@(Read-HealthQuery 'SHOW COLUMNS FROM zone_taxis')
 $archiveSchema=@(Read-HealthQuery 'SHOW COLUMNS FROM zone_taxis_unresolved')
 if($liveSchema.Count -ne $archiveSchema.Count){throw 'Taxi archive schema column count differs'}
 for($column=0;$column -lt $liveSchema.Count;$column++) {
  foreach($attribute in @('Field','Type','Null','Key','Default','Extra')) {
   if([string]$liveSchema[$column].$attribute -cne [string]$archiveSchema[$column].$attribute){throw 'Taxi archive schema differs'}
  }
 }
 $missing=@(Read-HealthQuery 'SELECT a.ZoneId FROM zone_areas a WHERE (a.OrderInfluenceId<>0 AND NOT EXISTS(SELECT 1 FROM chapter_infos c WHERE c.InfluenceEntry=a.OrderInfluenceId)) OR (a.DestroInfluenceId<>0 AND NOT EXISTS(SELECT 1 FROM chapter_infos c WHERE c.InfluenceEntry=a.DestroInfluenceId))')
 if($missing.Count -ne 0){throw 'Missing area influence references remain'}
 $cities=@(Read-HealthQuery 'SELECT ZoneId FROM zone_areas WHERE ZoneId IN(161,162) AND COALESCE(OrderInfluenceId,0)=0 AND COALESCE(DestroInfluenceId,0)=0')
 if($cities.Count -ne 42){throw 'Expected 42 peaceful city areas without influence'}
 $pq=@(Read-HealthQuery 'SELECT Entry FROM pquest_info WHERE ZoneId IN(161,162) AND ChapterId<>0')
 if($pq.Count -ne 0){throw 'Peaceful city PQ fallback influence remains'}
 $archived=@(Read-HealthQuery 'SELECT * FROM zone_taxis_unresolved WHERE ZoneID IN(62,132,139,168,204) ORDER BY ZoneID')
 $expected=@(
  @(62,1,212579,1500746,28951,1008,1),
  @(132,0,125258,129373,13275,1763,$null),
  @(139,0,1247090,875283,14147,2525,$null),
  @(168,0,125258,129373,13275,1763,$null),
  @(204,0,1254816,928062,5720,932,$null))
 if($archived.Count -ne 5){throw 'Expected all five original taxi records in archive'}
 $columns=@('ZoneID','RealmID','WorldX','WorldY','WorldZ','WorldO','Enable')
 for($row=0;$row -lt 5;$row++) {
  for($col=0;$col -lt $columns.Count;$col++) {
   $actual=$archived[$row].($columns[$col]); $wanted=$expected[$row][$col]
   if($null -eq $wanted){if($actual -isnot [DBNull]){throw 'Archive NULL changed'}}
   elseif([long]$actual -ne [long]$wanted){throw "Archive mismatch at row $row column $col"}
  }
  if($archived[$row].Tier -isnot [DBNull]){throw 'Archive Tier changed'}
 }
 $live=@(Read-HealthQuery 'SELECT ZoneID FROM zone_taxis WHERE ZoneID IN(62,132,139,168,204)')
 if($live.Count -ne 5){throw 'Expected all five live taxi records to remain'}
 $changed=@(Read-HealthQuery 'SELECT a.ZoneID FROM zone_taxis_unresolved a LEFT JOIN zone_taxis t ON t.ZoneID=a.ZoneID AND t.RealmID=a.RealmID WHERE a.ZoneID IN(62,132,139,168,204) AND (t.ZoneID IS NULL OR NOT(t.WorldX <=> a.WorldX AND t.WorldY <=> a.WorldY AND t.WorldZ <=> a.WorldZ AND t.WorldO <=> a.WorldO AND t.Tier <=> a.Tier AND t.`Enable` <=> a.`Enable`))')
 if($changed.Count -ne 0){throw 'Live taxi records differ from the preserved originals; review any intentional destination repairs'}
 'PASS: influence references and peaceful city data verified; all five live taxi records archived verbatim and left in place. Destination functionality remains unverified.'
} finally { $connection.Dispose() }

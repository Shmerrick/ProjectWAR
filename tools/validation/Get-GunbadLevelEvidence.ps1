param([string]$ToolkitRoot = 'D:/Repos/Shmerrick/WAR-RE-Toolkit')
$ErrorActionPreference = 'Stop'
# Read-only: exact-position/model/name matches, never nearest-neighbour level inference.
[xml]$config = Get-Content (Join-Path $PSScriptRoot '../../bin/Release/Configs/World.xml')
$db = $config.DocumentElement.WorldDatabase
[void][Reflection.Assembly]::LoadFrom((Resolve-Path (Join-Path $PSScriptRoot '../../bin/Release/libs/MySql.Data.dll')))
$builder = New-Object MySql.Data.MySqlClient.MySqlConnectionStringBuilder
$builder.set_ConnectionString([string]$db.Custom)
$builder.set_Server([string]$db.Server)
$builder.set_Port([uint32]$db.Port)
$builder.set_Database(([string]$db.Database).Replace('%name%', 'world'))
$builder.set_UserID([string]$db.Username)
$builder.set_Password([string]$db.Password)
$connection = New-Object MySql.Data.MySqlClient.MySqlConnection($builder.get_ConnectionString())
$connection.Open()
$lookup = @{}
try {
    $command = $connection.CreateCommand()
    $command.CommandText = 'SELECT s.Instance_spawns_ID,s.Entry,s.Level,s.WorldX,s.WorldY,s.WorldZ,p.Name,p.Model1,p.Model2 FROM instance_creature_spawns s JOIN creature_protos p ON p.Entry=s.Entry WHERE s.ZoneID=60'
    $reader = $command.ExecuteReader()
    while ($reader.Read()) {
        $row = [pscustomobject]@{Key=$reader.GetString(0);Entry=$reader.GetUInt32(1);OldLevel=$reader.GetByte(2);X=$reader.GetInt32(3);Y=$reader.GetInt32(4);Z=$reader.GetInt32(5);Name=$reader.GetString(6);Model1=$reader.GetUInt16(7);Model2=$reader.GetUInt16(8)}
        $key = "$($row.X),$($row.Y),$($row.Z)"
        if (!$lookup.ContainsKey($key)) {$lookup[$key] = @()}
        $lookup[$key] += $row
    }
    $reader.Dispose()
    $command.Dispose()
} finally {$connection.Dispose()}

function U32($b, $i) {16777216L*$b[$i]+65536L*$b[$i+1]+256L*$b[$i+2]+$b[$i+3]}
$matchesBySpawn = @{}
foreach ($capture in @('INSTANCE_GUNBAD_PART1.txt.gz','INSTANCE_GUNBAD_PART2.txt.gz')) {
    $zone=0; $shiftX=0; $shiftY=0; $initPacket=0
    & (Join-Path $PSScriptRoot 'Read-OfficialPackets.ps1') -CapturePath (Join-Path $ToolkitRoot "libs/protocolservices/Packet Logs/$capture") -OpcodePattern 'S_PLAYER_INITTED|F_CREATE_MONSTER' | ForEach-Object {
        $packet=$_; $b=$packet.Bytes
        if ($packet.Header -match 'S_PLAYER_INITTED') {
            $zone=256*$b[31]+$b[32]; $shiftX=256*$b[27]+$b[28]; $shiftY=256*$b[29]+$b[30]; $initPacket=$packet.Index
        } elseif ($zone -eq 60 -and $b.Length -ge 50) {
            $x=(U32 $b 11)+819200-($shiftX*8192); $y=(U32 $b 15)+819200-($shiftY*8192); $z=256*$b[9]+$b[10]
            $position="$x,$y,$z"
            if (!$lookup.ContainsKey($position)) {return}
            $start=49+$b[47]; $end=$start
            while ($end -lt $b.Length -and $b[$end] -ne 0) {$end++}
            if ($end -ge $b.Length) {return}
            $name=[Text.Encoding]::ASCII.GetString($b,$start,$end-$start) -replace '\^[mMfFnNp]$', ''
            $model=256*$b[21]+$b[22]
            foreach ($row in $lookup[$position]) {
                if (($row.Name -replace '\^[mMfFnNp]$', '') -cne $name -or ($model -ne $row.Model1 -and $model -ne $row.Model2)) {continue}
                if (!$matchesBySpawn.ContainsKey($row.Key)) {$matchesBySpawn[$row.Key]=@{}}
                $matchesBySpawn[$row.Key][[int]$b[24]]=[pscustomobject]@{Key=$row.Key;Entry=$row.Entry;OldLevel=$row.OldLevel;Level=$b[24];X=$row.X;Y=$row.Y;Z=$row.Z;Name=$name;Capture=$capture;Packet=$packet.Index;InitPacket=$initPacket}
            }
        }
    }
}
if ($matchesBySpawn.Count -eq 0) {
    Write-Warning 'No exact current instance placements matched both captures; no level changes proposed.'
}
foreach ($entry in $matchesBySpawn.GetEnumerator()) {
    if ($entry.Value.Count -eq 1) {$entry.Value.Values | Write-Output}
    else {Write-Warning "Conflicting captured levels for spawn $($entry.Key); no correction proposed."}
}

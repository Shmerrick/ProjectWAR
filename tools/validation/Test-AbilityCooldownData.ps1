param(
    [string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'),
    [string]$ExtractedRoot = 'C:/Users/Admin/Downloads/myps'
)
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$resolver = & (Join-Path $PSScriptRoot 'Use-BuildAssemblies.ps1') -BuildRoot $BuildRoot
try {
    [void][Reflection.Assembly]::LoadFrom((Join-Path $BuildRoot 'libs/MySql.Data.dll'))
    $assembly = [Reflection.Assembly]::LoadFrom((Join-Path $BuildRoot 'ClientDataMatrix.exe'))
    $parser = $assembly.GetType('ClientDataMatrix.Parsers.AbilityBinaryParser', $true)
    $clientPath = Join-Path $ExtractedRoot 'data/bin/abilityexport.bin'
    $records = $parser.GetMethod('ParseAbilityExport').Invoke($null, [object[]]@([string]$clientPath))
    $bytes = [IO.File]::ReadAllBytes($clientPath)
    $fractional = @{}
    foreach ($record in $records) {
        if ($record.Cooldown % 1000 -eq 0) { continue }
        if ([BitConverter]::ToUInt32($bytes, $record.ByteOffset + 4) -ne $record.Cooldown -or
            [BitConverter]::ToUInt16($bytes, $record.ByteOffset + 40) -ne $record.AbilityId) {
            throw 'Parsed cooldown does not match raw client bytes.'
        }
        $fractional[[int]$record.AbilityId] = [int]$record.Cooldown
    }
    [xml]$config = Get-Content -LiteralPath (Join-Path $BuildRoot 'Configs/World.xml')
    $db = $config.DocumentElement.WorldDatabase
    if ($db.ConnectionType -ne 'DATABASE_MYSQL') { throw 'MySQL Release configuration required.' }
    $builder = New-Object MySql.Data.MySqlClient.MySqlConnectionStringBuilder
    $builder.set_ConnectionString([string]$db.Custom)
    $builder.set_Server([string]$db.Server)
    $builder.set_Port([uint32]$db.Port)
    $builder.set_Database(([string]$db.Database).Replace('%name%', 'world'))
    $builder.set_UserID([string]$db.Username)
    $builder.set_Password([string]$db.Password)
    $connection = New-Object MySql.Data.MySqlClient.MySqlConnection($builder.ConnectionString)
    try {
        $connection.Open()
        foreach ($table in @('abilities', 'mythic_src_abilities')) {
            $command = $connection.CreateCommand()
            $command.CommandText = 'SELECT Entry,COALESCE(Cooldown,0),CooldownMilliseconds,COALESCE(AICooldown,0) FROM ' + $table
            $total = 0
            $exact = 0
            try {
                $reader = $command.ExecuteReader()
                try {
                    while ($reader.Read()) {
                        $entry = [int]$reader.GetValue(0)
                        $legacy = [int]$reader.GetValue(1) * 1000
                        if ($reader.IsDBNull(2)) { throw "$table ability $entry has not been migrated." }
                        $actual = [int]$reader.GetValue(2)
                        $expected = $legacy
                        if ($fractional.ContainsKey($entry)) {
                            $expected = $fractional[$entry]
                            $exact++
                            if ([Math]::Max($actual, [int]$reader.GetValue(3) * 1000) -lt $legacy) {
                                throw "$table ability $entry lost its existing AI pacing floor."
                            }
                        }
                        if ($actual -ne $expected) { throw "$table ability $entry expected $expected ms, found $actual." }
                        $total++
                    }
                } finally { $reader.Dispose() }
            } finally { $command.Dispose() }
            if ($total -eq 0 -or $exact -eq 0 -or ($table -eq 'mythic_src_abilities' -and $exact -ne 49)) {
                throw "$table cooldown coverage is incomplete."
            }
            Write-Output "PASS: $table has $total exact runtime cooldowns, including $exact fractional client values; legacy delays and AI floors preserved."
        }
    } finally { $connection.Dispose() }
} finally { $resolver.Dispose() }

param(
    [string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'),
    [string]$ExtractedRoot = 'C:/Users/Admin/Downloads/myps'
)
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$resolver = & (Join-Path $PSScriptRoot 'Use-BuildAssemblies.ps1') -BuildRoot $BuildRoot
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('ProjectWAR-upgrades-' + [Guid]::NewGuid().ToString('N') + '.bin')
try {
    $assembly = [Reflection.Assembly]::LoadFrom((Join-Path $BuildRoot 'ClientDataMatrix.exe'))
    $parser = $assembly.GetType('ClientDataMatrix.Parsers.AbilityBinaryParser', $true)
    $method = $parser.GetMethod('ParseUpgradeTableExport')
    $clientFile = Join-Path $ExtractedRoot 'data/bin/upgradetableexport.bin'
    $records = $method.Invoke($null, [object[]]@([string]$clientFile))
    if ($records.Count -ne 138 -or @($records | Where-Object { $_.Items.Count -ne 20 }).Count -ne 0) {
        throw 'Expected 138 client upgrade records with 20 items each.'
    }
    # Tiny malformed files must fail before allocating from an untrusted count.
    foreach ($count in @([uint32]::MaxValue, [uint32]1)) {
        $bytes = [byte[]]([BitConverter]::GetBytes([uint32]0) + [BitConverter]::GetBytes($count))
        [IO.File]::WriteAllBytes($fixture, $bytes)
        $rejected = $false
        try { [void]$method.Invoke($null, [object[]]@([string]$fixture)) }
        catch {
            $rejected = $_.Exception.GetBaseException() -is [IO.InvalidDataException]
            if (-not $rejected) { throw }
        }
        if (-not $rejected) { throw "Malformed count $count was accepted." }
    }
    [IO.File]::WriteAllBytes($fixture, [byte[]]::new(9))
    $rejected = $false
    try { [void]$method.Invoke($null, [object[]]@([string]$fixture)) }
    catch {
        $rejected = $_.Exception.GetBaseException() -is [IO.InvalidDataException]
        if (-not $rejected) { throw }
    }
    if (-not $rejected) { throw 'Trailing data was accepted.' }
    Write-Output 'PASS: 138 client upgrade records / 2760 items; oversized counts, truncation and trailing data rejected.'

    $abilityFile = Join-Path $ExtractedRoot 'data/bin/abilityexport.bin'
    $abilities = $parser.GetMethod('ParseAbilityExport').Invoke($null, [object[]]@([string]$abilityFile))
    $componentFile = Join-Path $ExtractedRoot 'data/bin/abilitycomponentexport.bin'
    $components = $parser.GetMethod('ParseAbilityComponentExport').Invoke($null, [object[]]@([string]$componentFile))
    $raw = [IO.File]::ReadAllBytes($abilityFile)
    foreach ($index in 0..7) {
        $entry = 24857 + $index
        $offset = 2002266 + 194 * $index
        $component = 26661 + $index
        # Independent fixed-offset check of the actual client bytes, not the SQL import.
        if ([BitConverter]::ToUInt16($raw, $offset + 40) -ne $entry -or
            [BitConverter]::ToUInt16($raw, $offset + 72) -ne $component) {
            throw "Client control record $entry no longer matches the cited byte offsets."
        }
        $record = @($abilities | Where-Object AbilityId -eq $entry)
        $operation = @($components | Where-Object ComponentId -eq $component)
        if ($record.Count -ne 1 -or $record[0].ByteOffset -ne $offset -or
            $record[0].ComponentIds[1] -ne $component -or $operation.Count -ne 1 -or
            $operation[0].Operation -ne 51) {
            throw "Parsed Skaven control $entry disagrees with its client component chain."
        }
    }
    Write-Output 'PASS: all eight Skaven controls link operation-51 components in the client BIN; no runtime transformation claim.'
} finally {
    if (Test-Path -LiteralPath $fixture) { Remove-Item -LiteralPath $fixture }
    $resolver.Dispose()
}

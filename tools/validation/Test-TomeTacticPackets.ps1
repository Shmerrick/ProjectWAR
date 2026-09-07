param([string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'))
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$output = Join-Path $BuildRoot 'TomeTacticPacketChecks.exe'
& $compiler /nologo /warnaserror+ /target:exe /platform:x64 "/out:$output" `
    "/reference:$BuildRoot/FrameWork.dll" "/reference:$BuildRoot/Common.dll" `
    /reference:System.Core.dll `
    (Join-Path $PSScriptRoot 'TomeTacticPacketChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Tome tactic packet check compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Tome tactic advance packets do not match the live capture.' }

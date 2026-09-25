param([string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'))
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$output = Join-Path $BuildRoot 'ItemCooldownPersistenceChecks.exe'
& "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /warnaserror+ /target:exe /platform:x64 "/out:$output" `
    "/reference:$BuildRoot/WorldServer.exe" "/reference:$BuildRoot/Common.dll" "/reference:$BuildRoot/FrameWork.dll" `
    "/reference:$BuildRoot/libs/MySql.Data.dll" "/reference:$BuildRoot/libs/BehaviourTree.dll" `
    /reference:System.Core.dll /reference:System.Data.dll /reference:System.Xml.dll `
    (Join-Path $PSScriptRoot 'ItemCooldownPersistenceChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Item cooldown check compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Item cooldown persistence checks failed.' }

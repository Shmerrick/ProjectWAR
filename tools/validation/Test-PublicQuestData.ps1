param([string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'))
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$output = Join-Path $BuildRoot 'PublicQuestDataChecks.exe'
& $compiler /nologo /warnaserror+ /target:exe /platform:x64 "/out:$output" `
    "/reference:$BuildRoot/WorldServer.exe" "/reference:$BuildRoot/Common.dll" `
    "/reference:$BuildRoot/FrameWork.dll" "/reference:$BuildRoot/libs/MySql.Data.dll" `
    /reference:System.Data.dll /reference:System.Xml.dll /reference:System.Core.dll `
    (Join-Path $PSScriptRoot 'PublicQuestDataChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'PQ data check compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'PQ data checks failed.' }

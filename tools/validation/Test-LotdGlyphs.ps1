param([string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'))
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$output = Join-Path $BuildRoot 'LotdGlyphChecks.exe'
& $compiler /nologo /warnaserror+ /target:exe /platform:x64 "/out:$output" `
    "/reference:$BuildRoot/libs/MySql.Data.dll" `
    /reference:System.Data.dll /reference:System.Xml.dll /reference:System.Core.dll `
    (Join-Path $PSScriptRoot 'LotdGlyphChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'LOTD glyph check compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'LOTD glyph data checks failed.' }

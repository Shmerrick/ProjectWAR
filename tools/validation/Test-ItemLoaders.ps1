param([string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'))
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$output = Join-Path $BuildRoot 'ItemLoaderChecks.exe'
& "$env:WINDIR/Microsoft.NET/Framework64/v4.0.30319/csc.exe" /nologo /warnaserror+ /target:exe /platform:x64 "/out:$output" `
    "/reference:$BuildRoot/Common.dll" "/reference:$BuildRoot/FrameWork.dll" "/reference:$BuildRoot/libs/MySql.Data.dll" `
    /reference:System.Core.dll /reference:System.Data.dll /reference:System.Xml.dll (Join-Path $PSScriptRoot 'ItemLoaderChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Item loader check compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Item loader validation failed.' }

param(
    [string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release')
)
$ErrorActionPreference = 'Stop'
$BuildRoot = (Resolve-Path -LiteralPath $BuildRoot).Path
$compiler = Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$output = Join-Path $BuildRoot 'RuntimeRegressionChecks.exe'
& $compiler /nologo /warnaserror+ /target:exe /platform:x64 "/out:$output" `
    "/reference:$BuildRoot/WorldServer.exe" "/reference:$BuildRoot/Common.dll" `
    "/reference:$BuildRoot/FrameWork.dll" /reference:System.Drawing.dll /reference:System.Core.dll `
    "/reference:$BuildRoot/libs/BehaviourTree.dll" `
    (Join-Path $PSScriptRoot 'RuntimeRegressionChecks.cs')
if ($LASTEXITCODE -ne 0) { throw 'Regression check compilation failed.' }
& $output
if ($LASTEXITCODE -ne 0) { throw 'Runtime regression checks failed.' }

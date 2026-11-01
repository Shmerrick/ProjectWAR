param(
    [string]$BuildRoot = (Join-Path $PSScriptRoot '../../bin/Release'),
    # Build the solution (Release/x64) first; fails on any compiler warning, per AGENTS.md rule 9.
    [switch]$Build,
    [string]$MSBuild = 'C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe'
)
# Runs every Test-*.ps1 in this folder, each in a fresh Windows PowerShell process as the
# September 24 audit did, plus the bug-tracker lint, and exits 1 if any of them fails. Several
# tests read the Release database configured in bin/Release/Configs; none starts a service.
# Passing proves the automated checks only: it is not in-client acceptance.
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../..')).Path

if ($Build) {
    $log = Join-Path ([System.IO.Path]::GetTempPath()) 'ProjectWAR-validation-build.log'
    & $MSBuild (Join-Path $repo 'ProjectWAR.sln') /p:Platform=x64 /p:Configuration=Release /t:Build /m /nologo /verbosity:minimal *> $log
    $buildExit = $LASTEXITCODE
    $warnings = @(Select-String -Path $log -Pattern ': warning ' | ForEach-Object { $_.Line.Trim() } | Sort-Object -Unique)
    if ($buildExit -ne 0 -or $warnings.Count -gt 0) {
        $warnings | ForEach-Object { Write-Host $_ }
        Write-Host "FAIL: build exit $buildExit with $($warnings.Count) warning(s); log at $log"
        exit 1
    }
    Write-Host 'PASS: Release/x64 build with no warnings.'
}

$logDir = Join-Path ([System.IO.Path]::GetTempPath()) ('ProjectWAR-validation-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Force -Path $logDir | Out-Null
$scripts = @(Get-ChildItem -Path $PSScriptRoot -Filter 'Test-*.ps1' | Sort-Object Name) + @(Get-Item (Join-Path $PSScriptRoot 'Get-BugTrackerSummary.ps1'))
$results = foreach ($script in $scripts) {
    $timer = [System.Diagnostics.Stopwatch]::StartNew()
    $log = Join-Path $logDir ($script.BaseName + '.log')
    $arguments = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $script.FullName)
    if ($script.BaseName -like 'Test-*' -and (Get-Command $script.FullName).Parameters.ContainsKey('BuildRoot')) {
        $arguments += @('-BuildRoot', $BuildRoot)
    }
    # Windows PowerShell turns a child's stderr into error records; under Stop the first failing
    # check would abort the run instead of being reported, so only the exit code decides.
    $ErrorActionPreference = 'Continue'
    & powershell.exe @arguments *> $log
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = 'Stop'
    [pscustomobject]@{ Check = $script.BaseName; Result = $(if ($exitCode -eq 0) { 'PASS' } else { 'FAIL' }); Seconds = [Math]::Round($timer.Elapsed.TotalSeconds, 1) }
}

$results | Format-Table -AutoSize | Out-String -Width 120 | Write-Host
$failed = @($results | Where-Object { $_.Result -ne 'PASS' })
Write-Host ("{0} of {1} checks passed. Logs: {2}" -f ($results.Count - $failed.Count), $results.Count, $logDir)
if ($failed.Count -gt 0) { exit 1 }

param(
    [string]$Tracker = (Join-Path $PSScriptRoot '../../docs/INTERNAL_BUG_TRACKER.md'),
    # List the rows in one canonical state, e.g. -Status Retest.
    [ValidateSet('', 'Open', 'InProgress', 'Retest', 'Resolved', 'Closed')]
    [string]$Status = ''
)
# Reads docs/INTERNAL_BUG_TRACKER.md and files every row under one of the five states its
# guidelines define. The status cells keep their historical wording; this is the one place that
# maps it, so "what is open" and "what awaits a client retest" have a single answer. Exits 1
# when a row is malformed (cell count differs from its table header) or its status matches no
# rule, so the tracker cannot drift out of the vocabulary unnoticed.
$ErrorActionPreference = 'Stop'

# First match wins. A fix awaiting the client is Retest; a fix that names part of the defect as
# still undone is In Progress; only then does a bare "Fixed" count as Resolved.
$rules = @(
    @{ State = 'Closed';     Pattern = "^(Won't fix|Retracted|Not a bug|Superseded|Duplicate of|Closed)" },
    @{ State = 'Open';       Pattern = '^(\*\*)?Open\b' },
    @{ State = 'InProgress'; Pattern = '^In Progress' },
    @{ State = 'Retest';     Pattern = 'retest|needs testing|in-client verification|awaiting in-client|untested in client|^User reports improvement' },
    @{ State = 'InProgress'; Pattern = 'remains open|still missing|other zones open|wrapper missing|^Largely resolved' },
    @{ State = 'Resolved';   Pattern = '^(Fixed|Resolved|Implemented|Applied|Verified)\b' }
)

$rows = New-Object System.Collections.Generic.List[object]
$problems = New-Object System.Collections.Generic.List[string]
$section = ''
$header = $null
$lineNumber = 0
foreach ($line in [System.IO.File]::ReadAllLines((Resolve-Path -LiteralPath $Tracker).Path)) {
    $lineNumber++
    if ($line.StartsWith('## ')) { $section = $line.Substring(3); $header = $null; continue }
    # Escaped pipes (\|) belong to the cell; only bare pipes separate cells.
    $cells = @([regex]::Split($line.Trim().Trim('|'), '(?<!\\)\|') | ForEach-Object { $_.Trim() })
    if ($line -match '^\| *ID *\|') { $header = $cells; continue }
    if ($line -notmatch '^\| *BUG-\d+ *\|') { continue }

    if ($null -eq $header -or $cells.Count -ne $header.Count) {
        $problems.Add("line ${lineNumber}: $($cells[0]) has $($cells.Count) cells against a $(@($header).Count)-column header")
        continue
    }
    $statusIndex = [array]::IndexOf($header, 'Status')
    if ($statusIndex -lt 0) { $statusIndex = $header.Count - 1 }
    $text = $cells[$statusIndex]
    $state = $null
    foreach ($rule in $rules) {
        if ($text -match $rule.Pattern) { $state = $rule.State; break }
    }
    if ($null -eq $state) {
        $problems.Add("line ${lineNumber}: $($cells[0]) status matches no rule: $($text.Substring(0, [Math]::Min(80, $text.Length)))")
        continue
    }
    $rows.Add([pscustomobject]@{ Id = $cells[0]; State = $state; Line = $lineNumber; Section = $section; Title = $cells[1] })
}

if ($Status) {
    $rows | Where-Object { $_.State -eq $Status } | Sort-Object { [int]($_.Id -replace '\D', '') } | ForEach-Object {
        "{0,-8} line {1,-4} {2}" -f $_.Id, $_.Line, $_.Title.Substring(0, [Math]::Min(100, $_.Title.Length))
    }
}
else {
    "{0} rows in {1}" -f $rows.Count, (Split-Path -Leaf $Tracker)
    foreach ($state in 'Open', 'InProgress', 'Retest', 'Resolved', 'Closed') {
        $ids = @($rows | Where-Object { $_.State -eq $state } | Sort-Object { [int]($_.Id -replace '\D', '') } | ForEach-Object { $_.Id -replace 'BUG-', '' })
        "{0,-10} {1,4}  {2}" -f $state, $ids.Count, ($ids -join ' ')
    }
    $repeated = @($rows | Group-Object Id | Where-Object { $_.Count -gt 1 })
    if ($repeated.Count -gt 0) {
        "Listed in more than one table (later sections may update earlier rows): " + (($repeated | ForEach-Object { $_.Name }) -join ', ')
    }
}

if ($problems.Count -gt 0) {
    $problems | ForEach-Object { Write-Host "FAIL: $_" }
    exit 1
}

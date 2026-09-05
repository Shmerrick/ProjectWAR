param(
    [Parameter(Mandatory=$true)][string]$CapturePath,
    [string]$OpcodePattern = 'F_OBJECTIVE_INFO|F_UPDATE_STATE|F_PLAYER_JUMP|F_CREATE_STATIC'
)
$ErrorActionPreference = 'Stop'
# Read-only gzip text decoder. Packet ordinals include both client and server frames.
function Read-Capture([string]$name) {
 $f=[IO.File]::OpenRead($name)
 $gz=New-Object IO.Compression.GZipStream($f,[IO.Compression.CompressionMode]::Decompress)
 $r=New-Object IO.StreamReader($gz)
 try { $r.ReadToEnd() } finally {$r.Dispose();$gz.Dispose();$f.Dispose()}
}

function Packets([string]$name,[string]$op) {
 $s=Read-Capture $name
 $blocks=[regex]::Split($s,'(?=\[(?:Server|Client)\] packet)')
 $index=0
 foreach($block in $blocks) {
  if($block -notmatch '^\[(Server|Client)\] packet') {continue}
  $index++
  if(($block -split '\r?\n',2)[0] -notmatch $op) {continue}
  $hex=[Collections.Generic.List[byte]]::new()
  foreach($line in ($block -split '\r?\n')) {
   if($line.StartsWith('|00 01 02 03 04 05 06 07')) {continue}
   if($line -match '^\|((?:[A-Fa-f0-9]{2} )+)') {
    foreach($h in ($Matches[1].Trim() -split ' ')) {$hex.Add([Convert]::ToByte($h,16))}
   }
  }
  [pscustomobject]@{Index=$index; Header=($block -split '\r?\n',2)[0]; Bytes=$hex.ToArray(); Text=[Text.Encoding]::ASCII.GetString($hex.ToArray())}
 }
}

Packets (Resolve-Path -LiteralPath $CapturePath).Path $OpcodePattern


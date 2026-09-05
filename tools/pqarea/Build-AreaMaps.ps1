param([string]$Spawns,[int]$Radius,[string]$Out,[int]$Uniform=0)
Add-Type -AssemblyName System.Drawing
$N=1024
function IdColor([int]$v){
  $n=$v-1; $rq=[Math]::Min(15,$n); $gq=$n-$rq
  return [System.Drawing.Color]::FromArgb(255, $rq*16, $gq*16, 0)
}
$bmp=New-Object System.Drawing.Bitmap($N,$N,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::None
if($Uniform -gt 0){
  $g.Clear((IdColor $Uniform))
} else {
  $g.Clear((IdColor 31))                     # 31 = no PQ here
  $byId=@{}
  foreach($line in Get-Content $Spawns){
    if($line -notmatch '\S'){continue}
    $p=$line.Split(','); $a=[int]$p[0]; $x=[int]$p[1]; $y=[int]$p[2]
    if($x -lt 0 -or $x -ge $N -or $y -lt 0 -or $y -ge $N){continue}
    if(-not $byId.ContainsKey($a)){ $byId[$a]=New-Object 'System.Collections.Generic.List[object]' }
    $byId[$a].Add(@($x,$y))
  }
  # Largest footprint first so smaller, tighter quests win the overlaps.
  foreach($a in ($byId.Keys | Sort-Object { -$byId[$_].Count })){
    $br=New-Object System.Drawing.SolidBrush((IdColor $a))
    foreach($pt in $byId[$a]){
      $g.FillEllipse($br, $pt[0]-$Radius, $pt[1]-$Radius, $Radius*2, $Radius*2)
    }
    $br.Dispose()
    Write-Output ("  area {0,2}: {1,4} discs" -f $a,$byId[$a].Count)
  }
}
$g.Dispose()
$bmp.Save($Out,[System.Drawing.Imaging.ImageFormat]::Png); $bmp.Dispose()
Write-Output "wrote $Out"

param([string]$Path,[string]$Label)
Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap($Path)
$w = $bmp.Width; $h = $bmp.Height
$rect = New-Object System.Drawing.Rectangle 0,0,$w,$h
$data = $bmp.LockBits($rect,[System.Drawing.Imaging.ImageLockMode]::ReadOnly,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$bytes = New-Object byte[] ($data.Stride*$h)
[System.Runtime.InteropServices.Marshal]::Copy($data.Scan0,$bytes,0,$bytes.Length)
$bmp.UnlockBits($data); $bmp.Dispose()

$stats = @{}
for ($y=0; $y -lt $h; $y++) {
  $row = $y*$data.Stride
  for ($x=0; $x -lt $w; $x++) {
    $o = $row + $x*4
    $b=$bytes[$o]; $g=$bytes[$o+1]; $r=$bytes[$o+2]
    $id = 1 + ($r -shr 4) + ($g -shr 4)
    if (-not $stats.ContainsKey($id)) { $stats[$id] = [pscustomobject]@{Id=$id;N=0;MinX=9999;MaxX=-1;MinY=9999;MaxY=-1;SumX=0;SumY=0} }
    $s=$stats[$id]; $s.N++; $s.SumX+=$x; $s.SumY+=$y
    if($x -lt $s.MinX){$s.MinX=$x}; if($x -gt $s.MaxX){$s.MaxX=$x}
    if($y -lt $s.MinY){$s.MinY=$y}; if($y -gt $s.MaxY){$s.MaxY=$y}
  }
}
Write-Output "=== $Label ($w x $h) ==="
$stats.Values | Sort-Object Id | ForEach-Object {
  $cx=[int]($_.SumX/$_.N); $cy=[int]($_.SumY/$_.N)
  "{0,3} px={1,-8} centroid=({2},{3}) bbox=({4},{5})-({6},{7})" -f $_.Id,$_.N,$cx,$cy,$_.MinX,$_.MinY,$_.MaxX,$_.MaxY
}

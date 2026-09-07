# Read-only evidence search; writes a CSV report, never game data.
# INIT/JUMP records following flight requests are candidates, not proof of causation.
# Instance INIT coordinates can use an instance atlas, not zone_infos world offsets.
# Layout references: toolkit libs/protocolservices/Server Packet Protocol/
# F_SWITCH_REGION.cs, S_PLAYER_INITTED.cs, F_PLAYER_JUMP.cs; flight menus are
# independently readable in MECHANIC_orderflymaster_alldestination(missing LoD).
param(
 [string]$CaptureRoot='D:/Repos/Shmerrick/WAR-RE-Toolkit/libs/protocolservices/Packet Logs',
 [string]$OutputPath=(Join-Path ([IO.Path]::GetTempPath()) 'ProjectWAR-taxi-capture-evidence.csv')
)
$ErrorActionPreference='Stop'
Add-Type -TypeDefinition @"
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
public static class TaxiCaptureAudit {
 public static int Menus;
 public static int Records;
 static string CsvName(string path){return "\""+Path.GetFileName(path).Replace("\"","\"\"")+"\"";}
 static int U16(List<byte> b,int p){return (b[p]<<8)|b[p+1];}
 static long U32(List<byte> b,int p){return ((long)U16(b,p)<<16)| (uint)U16(b,p+2);}
 public static void Scan(string path, TextWriter output){
  int ordinal=0, zone=0, selected=0, flightOrdinal=0; string header="";
  var bytes=new List<byte>(); var menu=new Dictionary<int,int>();
  Action flush=()=>{
   if(header.Contains("F_INTERACT_RESPONSE") && bytes.Count>=5 && bytes[3]==10){
    int count=bytes[4]; if(bytes.Count<5+8*count) throw new InvalidDataException(path+":"+ordinal);
    Menus++; Records+=count; menu.Clear();
    for(int i=0;i<count;i++){int p=5+i*8; int z=U16(bytes,p+5); menu[U16(bytes,p)]=z;
     if(z==62||z==132||z==139||z==168||z==204) output.WriteLine(CsvName(path)+","+ordinal+",MENU,"+z+",,,,,");
    }
   }
   if(header.Contains("F_FLIGHT") && bytes.Count>=16 && U16(bytes,12)==20){flightOrdinal=ordinal; selected=menu.ContainsKey(U16(bytes,14))?menu[U16(bytes,14)]:0;}
   if(header.Contains("F_SWITCH_REGION") && bytes.Count>=5) zone=U16(bytes,3);
   if(header.Contains("S_PLAYER_INITTED") && bytes.Count>=27){
    if(zone==62||zone==132||zone==139||zone==168||zone==204 || flightOrdinal!=0)
     output.WriteLine(CsvName(path)+","+ordinal+",INIT,"+zone+","+U32(bytes,15)+","+U32(bytes,19)+","+U16(bytes,11)+","+U16(bytes,23)+","+selected+":"+flightOrdinal);
    flightOrdinal=0; selected=0;
   }
   if(header.Contains("F_PLAYER_JUMP") && bytes.Count>=17 && flightOrdinal!=0){
    output.WriteLine(CsvName(path)+","+ordinal+",JUMP,"+selected+","+U32(bytes,3)+","+U32(bytes,7)+","+U16(bytes,13)+","+U16(bytes,15)+","+selected+":"+flightOrdinal);flightOrdinal=0;selected=0;
   }
  };
  using(var f=File.OpenRead(path))using(var gz=new GZipStream(f,CompressionMode.Decompress))using(var r=new StreamReader(gz)){
   string line; while((line=r.ReadLine())!=null){
    if(line.StartsWith("[Server] packet")||line.StartsWith("[Client] packet")){flush();ordinal++;header=line;bytes.Clear();continue;}
    if(!(header.Contains("F_INTERACT_RESPONSE")||header.Contains("F_FLIGHT")||header.Contains("F_SWITCH_REGION")||header.Contains("S_PLAYER_INITTED")||header.Contains("F_PLAYER_JUMP")))continue;
    if(line.StartsWith("|00 01 02 03 04 05 06 07")||!line.StartsWith("|"))continue;
    for(int p=1;p+2<line.Length && line[p+2]==' ';p+=3){byte b;if(!byte.TryParse(line.Substring(p,2),System.Globalization.NumberStyles.HexNumber,null,out b))break;bytes.Add(b);}
   } flush();
  }
 }
}
"@
$writer=[IO.StreamWriter]::new($OutputPath)
try {
 $writer.WriteLine('Capture,Packet,Kind,Zone,X,Y,Z,O,Flight')
 $files=@(Get-ChildItem -LiteralPath $CaptureRoot -Filter '*.gz' | Sort-Object Name)
 for($i=0;$i -lt $files.Count;$i++) {
  [TaxiCaptureAudit]::Scan($files[$i].FullName,$writer)
  if($i%100 -eq 0){Write-Output "Scanned $i / $($files.Count) captures"}
 }
} finally {$writer.Dispose()}
Write-Output "Scanned $($files.Count) captures, $([TaxiCaptureAudit]::Menus) flight menus, $([TaxiCaptureAudit]::Records) destination records."
Write-Output "Results: $OutputPath"


# Read-only actor/effect attribution; counts are effects, NOT casts or cooldowns.
# Layout: WAR-RE-Toolkit/libs/protocolservices/Server Packet Protocol/
# F_CREATE_MONSTER.cs and F_CAST_PLAYER_EFFECT.cs. All offsets include the 3-byte header.
param(
    [Parameter(Mandatory=$true)][string[]]$CapturePath,
    [string]$OutputPath = (Join-Path ([IO.Path]::GetTempPath()) 'ProjectWAR-encounter-evidence.csv')
)
$ErrorActionPreference = 'Stop'
Add-Type -TypeDefinition @'
using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text;
public static class EncounterCaptureAudit {
 sealed class Actor { public string Name; public int Model, Packet; }
 sealed class Evidence { public Actor Actor; public int Ability, First, Count; }
 static int U16(List<byte> b,int p) { return (b[p]<<8)|b[p+1]; }
 static string Csv(string s) { return "\""+s.Replace("\"","\"\"")+"\""; }
 public static string Scan(string path, TextWriter output) {
  int ordinal=0, creations=0, effects=0, attributed=0; string header="";
  var bytes=new List<byte>(); var actors=new Dictionary<int,Actor>();
  var evidence=new SortedDictionary<string,Evidence>(StringComparer.Ordinal);
  Action flush=()=> {
   if(header.Contains("F_SWITCH_REGION")) { actors.Clear(); return; }
   if(bytes.Count<5) return;
   int oid=U16(bytes,3);
   if(header.Contains("F_REMOVE_PLAYER")||header.Contains("F_CREATE_PLAYER")) { actors.Remove(oid); return; }
   if(header.Contains("F_CREATE_MONSTER")) {
    creations++;
    actors.Remove(oid);
    if(bytes.Count<50) return;
    int start=49+bytes[47], end=start;
    while(end<bytes.Count && bytes[end]!=0) end++;
    if(start>=bytes.Count || end==bytes.Count) return;
    actors[oid]=new Actor { Name=Encoding.UTF8.GetString(bytes.ToArray(),start,end-start), Model=U16(bytes,21), Packet=ordinal };
   }
   else if(header.Contains("F_CAST_PLAYER_EFFECT") && bytes.Count>=12) {
    effects++;
    int ability=U16(bytes,7);
    Actor actor;
    if(!actors.TryGetValue(oid,out actor)) {
     if(!(ability>=24857 && ability<=24864) && ability!=24873) return;
     actor=new Actor { Name="UNRESOLVED ACTOR "+oid, Packet=0, Model=0 };
    }
    else attributed++;
    string key=actor.Name+"|"+actor.Model+"|"+ability;
    Evidence row;
    if(!evidence.TryGetValue(key,out row)) {
     row=new Evidence { Actor=actor, Ability=ability, First=ordinal };
     evidence.Add(key,row);
    }
    row.Count++;
   }
  };
  using(var file=File.OpenRead(path)) using(var gz=new GZipStream(file,CompressionMode.Decompress)) using(var reader=new StreamReader(gz)) {
   string line;
   while((line=reader.ReadLine())!=null) {
    if(line.StartsWith("[Server] packet")||line.StartsWith("[Client] packet")) { flush(); ordinal++; header=line; bytes.Clear(); continue; }
    if(!header.StartsWith("[Server]") || !(header.Contains("F_CREATE_MONSTER")||header.Contains("F_CREATE_PLAYER")||header.Contains("F_REMOVE_PLAYER")||header.Contains("F_CAST_PLAYER_EFFECT"))) continue;
    if(!line.StartsWith("|")||line.StartsWith("|00 01 02 03 04 05 06 07")) continue;
    for(int p=1;p+2<line.Length && line[p+2]==' ';p+=3) {
     byte value; if(!byte.TryParse(line.Substring(p,2),System.Globalization.NumberStyles.HexNumber,null,out value)) break;
     bytes.Add(value);
    }
   }
   flush();
  }
  foreach(var row in evidence.Values)
   output.WriteLine(Csv(Path.GetFileName(path))+","+Csv(row.Actor.Name)+","+row.Actor.Model+","+row.Actor.Packet+","+row.Ability+","+row.First+","+row.Count);
  return Path.GetFileName(path)+": frames="+ordinal+", monster creates="+creations+", effects="+effects+", attributed effects="+attributed;
 }
}
'@
$writer = [IO.StreamWriter]::new($OutputPath)
try {
    $writer.WriteLine('Capture,Actor,Model,CreationPacket,Ability,FirstEffectPacket,EffectPackets')
    foreach ($capture in $CapturePath) {
        [EncounterCaptureAudit]::Scan((Resolve-Path -LiteralPath $capture).Path, $writer)
    }
} finally { $writer.Dispose() }
Write-Output "Evidence: $OutputPath"

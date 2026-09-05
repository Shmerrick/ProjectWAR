using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Common;
using FrameWork;
using WorldServer.Services.World;
using Color = System.Drawing.Color;

namespace WorldServer.Managers
{
    public struct AreaInfluence
    {
        public ushort AreaNumber;
        public byte Realm;
        public ushort InfluenceId;
    }

    /*
    public class MapPiece
    {
        public byte Id;
        public ushort ZoneId;
        public ushort PositionX, PositionY;
        public ushort SizeX, SizeY;
        public Color[,] Colors;
        public BitArray[] PieceMap { get; set; }
        public Zone_Area Area;

        public bool IsPvp(byte realm)
        {
            if (!Program.Config.OpenRvR && Area != null && Area.Realm != 0)
                return false;

            return true;
        }

        public bool IsRvR()
        {
            if (Area != null && Area.Realm == 0)
                return true;

            return false;
        }

        public bool IsOn(ushort pinX, ushort pinY, ushort zoneId)
        {
            if (ZoneId != zoneId)
                return false;

            if (pinX >= PositionX && pinX < PositionX + SizeX)
            {
                if (pinY >= PositionY && pinY < PositionY + SizeY && PieceMap[pinX - PositionX][pinY - PositionY])
                    return true;
            }

            return false;
        }
        

        public override string ToString()
        {
            return "Id:" + Id + ",Area:" + Area;
        }
    }
    
    */
    public class ClientZoneInfo
    {
        public ushort ZoneId;
        public string Folder;
        public List<AreaInfluence> Influences;
        public List<Zone_Area> Areas;
        public List<PQuest_Info> PQAreas;
        public Color[,] HeightMapOffset;
        public Color[,] HeightMapTerrain;
        public byte[,] AreaPixels;
        public byte[,] PQAreaPixels;

        public ClientZoneInfo(ushort zoneId)
        {
            ZoneId = zoneId;
            Influences = new List<AreaInfluence>();
            Folder = Path.Combine(Program.Config.ZoneFolder, $"zone{zoneId:000}");
            Areas = ZoneService.GetZoneAreas(zoneId).OrderBy(area => area.PieceId).ToList();

            try
            {
                //LoadHeightMap();
                LoadAreaMap();
                LoadPQAreaMap();
                LoadInfluences();

                //Log.Success("ClientFile", zoneId + " Loaded " + Influences.Count + " influence entries and " + Areas.Count + " area infos.");
            }
            catch (Exception e)
            {
                Log.Error("ClientFile", e.ToString());
            }
        }

        public void LoadHeightMap()
        {
            string filePath = Path.Combine(Folder, "offset.png");
            if (File.Exists(filePath))
            {
                int x, y;

                using (Bitmap map = new Bitmap(filePath))
                {
                    HeightMapOffset = new Color[map.Width, map.Height];
                    for (x = 0; x < map.Width; ++x)
                    {
                        for (y = 0; y < map.Height; ++y)
                        {
                            HeightMapOffset[x, y] = map.GetPixel(x, y);
                        }
                    }
                }

                filePath = Path.Combine(Folder, "terrain.png");
                using (Bitmap map = new Bitmap(filePath))
                {
                    HeightMapTerrain = new Color[map.Width, map.Height];
                    for (x = 0; x < map.Width; ++x)
                    {
                        for (y = 0; y < map.Height; ++y)
                        {
                            HeightMapTerrain[x, y] = map.GetPixel(x, y);
                        }
                    }
                }
            }
        }

        public void LoadAreaMap()
        {
            AreaPixels = LoadOverlay("areas");
        }

        //Use 1024x1024 PNG color overlay to define a PQ area.
        //Color must be different for each pq for the pq to function correctly.
        public void LoadPQAreaMap()
        {
            PQAreaPixels = LoadOverlay("pqarea");
        }

        private byte[,] LoadOverlay(string prefix)
        {
            string filePath = Path.Combine(Folder, prefix + $"{ZoneId:000}" + ".png");
            if (!File.Exists(filePath))
            {
                Log.Notice("ClientFile", "Missing zone overlay " + filePath + "; its area lookup is unavailable (BUG-041).");
                return null;
            }

            try
            {
                using (Bitmap map = new Bitmap(filePath))
                {
                    if (map.Width != 1024 || map.Height != 1024)
                    {
                        Log.Error("ClientFile", "Invalid zone overlay " + filePath + ": expected 1024x1024, got " + map.Width + "x" + map.Height);
                        return null;
                    }

                    var pixels = new byte[1024, 1024];
                    for (int x = 0; x < 1024; ++x)
                    {
                        for (int y = 0; y < 1024; ++y)
                        {
                            Color curPx = map.GetPixel(x, y);
                            pixels[x, y] = (byte)(1 + (curPx.R >> 4) + (curPx.G >> 4));
                        }
                    }
                    return pixels;
                }
            }
            catch (Exception e)
            {
                // One bad overlay must not prevent the independent overlay from loading.
                Log.Error("ClientFile", "Cannot load zone overlay " + filePath + ": " + e.Message);
                return null;
            }
        }

        public void LoadInfluences()
        {
            string filePath = Path.Combine(Folder, "influenceids.csv");
            if (!File.Exists(filePath))
                return;

            using (FileStream stream = File.OpenRead(filePath))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    reader.ReadLine();
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] datas = line.Split(',');
                        if (datas.Length < 3 || !ushort.TryParse(datas[0], out ushort number) ||
                            !byte.TryParse(datas[1], out byte realm) || !ushort.TryParse(datas[2], out ushort influence))
                        {
                            Log.Error("ClientFile", "Invalid influence row in " + filePath + ": " + line);
                            continue;
                        }
                        AreaInfluence area = new AreaInfluence { AreaNumber = number, Realm = realm, InfluenceId = influence };
                        Influences.Add(area);
                    }
                }
            }
        }

        public Zone_Area GetZoneAreaFor(ushort pinX, ushort pinY, ushort zoneId,ushort pinz = 0)
        {
            if (zoneId != ZoneId)
                return null;

            // Gunbad has a persistent zone-wide influence area, independent of local PQ
            // pieces. Client maps/zone060/influenceids.csv only binds area 31; official
            // INSTANCE_GUNBAD_PART1 #381/#71466 activates it across instance entries.
            // The legacy painted map's 1..8 pieces must not hide that enclosing area.
            if (zoneId == 60 && Areas != null)
                foreach (Zone_Area area in Areas)
                    if (area.AreaId == 31)
                        return area;

            if (AreaPixels == null)
                return null;

            byte areaId = AreaPixels[pinX >> 6, pinY >> 6];
           // Log.Error("areaid", "    " + areaId);
           // fix for black craig keep in the dungeon
            if(ZoneId == 3 && areaId > 20)
            {
                if(pinz < 8394)
                    areaId = 3;
                else
                    areaId -= 15;
            }
            if (Areas == null)
                return null;
            foreach (Zone_Area info in Areas)
                if (info.PieceId == areaId)
                    return info;
            return null;
        }

        public byte GetPQAreaFor(ushort pinX, ushort pinY, ushort zoneId)
        {
            return zoneId == ZoneId && PQAreaPixels != null ? PQAreaPixels[pinX >> 6, pinY >> 6] : (byte)0;
        }
    }

    public class HeightMapInfo
    {
        public HeightMapInfo(int zoneID)
        {
            ZoneID = zoneID;
            _heights = new Lazy<int[,]>(LoadHeights);
        }

        public readonly int ZoneID;
        private readonly Lazy<int[,]> _heights;

        public int GetHeight(int pinX, int pinY)
        {
            if (pinX < 0 || pinY < 0)
                return -1;

            int[,] heights = _heights.Value;
            pinX >>= 6;
            pinY >>= 6;
            if (heights == null || pinX >= heights.GetLength(0) || pinY >= heights.GetLength(1))
                return -1;

            return heights[pinX, pinY];
        }

        public void Load()
        {
            // Lazy publishes a complete immutable raster to concurrent callers exactly once.
            _ = _heights.Value;
        }

        private int[,] LoadHeights()
        {
            try
            {
                string folder = Path.Combine(Program.Config.ZoneFolder, $"zone{ZoneID:000}");
                using (var offset = new Bitmap(Path.Combine(folder, "offset.png")))
                using (var terrain = new Bitmap(Path.Combine(folder, "terrain.png")))
                {
                    if (offset.Size != terrain.Size || offset.Width > 1024 || offset.Height > 1024)
                        throw new InvalidDataException("Height rasters must have matching dimensions no larger than 1024x1024.");

                    var heights = new int[offset.Width, offset.Height];
                    for (int x = 0; x < offset.Width; x++)
                        for (int y = 0; y < offset.Height; y++)
                            heights[x, y] = (offset.GetPixel(x, y).R * 31 + terrain.GetPixel(x, y).R) * 16 - 30;
                    return heights;
                }
            }
            catch (Exception e)
            {
                Log.Error("HeightMap", "[" + ZoneID + "] Invalid HeightMap \n " + e);
                return null;
            }
        }
    }

    public static class ClientFileMgr
    {
        #region HeightMap Images

        private static readonly Dictionary<int, HeightMapInfo> Heights = new Dictionary<int, HeightMapInfo>();
        private static readonly object HeightsLock = new object();

        public static int GetHeight(int zoneID, int pinX, int pinY)
        {
            HeightMapInfo info;
            lock (HeightsLock)
            {
                if (!Heights.TryGetValue(zoneID, out info))
                {
                    info = new HeightMapInfo(zoneID);
                    Heights.Add(zoneID, info);
                }
            }

            int height = info.GetHeight(pinX, pinY);
            return height == -1 ? -1 : height / 2;
        }

        #endregion

        #region MapPiece and CSV

        public static Dictionary<ushort, ClientZoneInfo> ClientZoneFiles = new Dictionary<ushort, ClientZoneInfo>();

        public static ClientZoneInfo GetZoneInfo(ushort zoneId)
        {
            ClientZoneInfo info;
            lock (ClientZoneFiles)
            {
                if (!ClientZoneFiles.TryGetValue(zoneId, out info))
                {
                    info = new ClientZoneInfo(zoneId);
                    ClientZoneFiles.Add(zoneId, info);
                }
            }
            return info;
        }

        #endregion
    }
}

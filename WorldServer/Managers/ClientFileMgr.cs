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
        public byte[,] AreaPixels = new byte[1024, 1024];
        public byte[,] PQAreaPixels = new byte[1024, 1024];

        public ClientZoneInfo(ushort zoneId)
        {
            ZoneId = zoneId;
            Influences = new List<AreaInfluence>();
            Folder = Program.Config.ZoneFolder + "zone" + string.Format("{0:000}", zoneId) + "/";
            Areas = ZoneService.GetZoneAreas(zoneId).OrderBy(area => area.PieceId).ToList();

            try
            {
                //LoadHeightMap();
                LoadInfluences();
                LoadAreaMap();
                LoadPQAreaMap();

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
            string filePath = Path.Combine(Folder, "areas" + $"{ZoneId:000}" + ".png");
            if (File.Exists(filePath))
            {
                //Log.Success("LoadAreaMap", "Loading area map for zone ID "+ZoneId);
                using (Bitmap map = new Bitmap(filePath))
                {
                    for (int x = 0; x < 1024; ++x)
                    {
                        for (int y = 0; y < 1024; ++y)
                        {
                            Color curPx = map.GetPixel(x, y);
                            AreaPixels[x, y] = (byte)(1 + (curPx.R >> 4) + (curPx.G >> 4));
                        }
                    }
                }

                //Log.Success("LoadAreaMap", "Loaded area map for zone ID " + ZoneId);
            }

            //else Log.Error("LoadAreaMap", "No area map found for zone ID "+ZoneId);
        }

        //Use 1024x1024 PNG color overlay to define a PQ area.
        //Color must be different for each pq for the pq to function correctly.
        public void LoadPQAreaMap()
        {
            string filePath = Path.Combine(Folder, "pqarea" + $"{ZoneId:000}" + ".png");
            if (File.Exists(filePath))
            {
                //Log.Success("LoadPQAreaMap", "Loading PQ area map for zone ID " + ZoneId);
                using (Bitmap map = new Bitmap(filePath))
                {
                    for (int x = 0; x < 1024; ++x)
                    {
                        for (int y = 0; y < 1024; ++y)
                        {
                            Color curPx = map.GetPixel(x, y);
                            PQAreaPixels[x, y] = (byte)(1 + (curPx.R >> 4) + (curPx.G >> 4));
                        }
                    }
                }

                //Log.Success("LoadPQAreaMap", "Loaded PQ area map for zone ID " + ZoneId);
            }

            //else Log.Error("LoadPQAreaMap", "No PQ area map found for zone ID " + ZoneId);
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
                        AreaInfluence area = new AreaInfluence
                        {
                            AreaNumber = ushort.Parse(datas[0]),
                            Realm = byte.Parse(datas[1]),
                            InfluenceId = ushort.Parse(datas[2])
                        };
                        Influences.Add(area);
                    }
                }
            }
        }

        public Zone_Area GetZoneAreaFor(ushort pinX, ushort pinY, ushort zoneId,ushort pinz = 0)
        {
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
            return PQAreaPixels[pinX >> 6, pinY >> 6];
        }
    }

    public class HeightMapInfo
    {
        public HeightMapInfo(int zoneID)
        {
            ZoneID = zoneID;
        }

        public int ZoneID;
        public Bitmap Offset;
        public Bitmap Terrain;

        private bool _loaded;

        public int GetHeight(int pinX, int pinY)
        {
            Load();

            if (Offset == null || Terrain == null)
                return -1;

            pinX = (int)(pinX / 64f);
            pinY = (int)(pinY / 64f);
            Bitmap off = null;
            lock (Offset)
            { 
                off = (Bitmap)Offset.Clone();
            }

            Bitmap terr = null;
            lock (Terrain)
            {
                terr = (Bitmap)Terrain.Clone();
            }


            try
            {
                if (pinX < 0 || pinX > off.Width || pinX > terr.Width)
                    return -1;

                if (pinY < 0 || pinY > off.Height || pinY > terr.Height)
                    return -1;
            }
            catch
            {
                return -1;
            }

            float fZValue = 0;

            try
            {
                {
                    Color iColor = off.GetPixel(pinX, pinY);
                    fZValue += iColor.R * 31; // 0 -> 30
                }

                {
                    Color iColor = terr.GetPixel(pinX, pinY);
                    fZValue += iColor.R;
                }
            }
            catch (Exception e)
            {
                Log.Error("HeightMap", e.ToString());
            }

            fZValue *= 16;

            return (int)fZValue - 30;
        }

        public void Load()
        {
            if (_loaded)
                return;

            _loaded = true;

            try
            {
                Offset = new Bitmap(Program.Config.ZoneFolder + "zone" + string.Format("{0:000}", ZoneID) + "/offset.png"); // /zones/zone003/offset.png
                Terrain = new Bitmap(Program.Config.ZoneFolder + "zone" + string.Format("{0:000}", ZoneID) + "/terrain.png"); // /zones/zone003/offset.png
            }
            catch (Exception e)
            {
                Log.Error("HeightMap", "[" + ZoneID + "] Invalid HeightMap \n " + e);
            }
        }
    }

    public static class ClientFileMgr
    {
        #region HeightMap Images

        public static Dictionary<int, HeightMapInfo> Heights = new Dictionary<int, HeightMapInfo>();

        public static int GetHeight(int zoneID, int pinX, int pinY)
        {
            HeightMapInfo info;
            if (!Heights.TryGetValue(zoneID, out info))
            {
                Log.Success("HeightMap", "[" + zoneID + "] Loading Height Map..");
                info = new HeightMapInfo(zoneID);
                Heights.Add(zoneID, info);
            }

            return info.GetHeight(pinX, pinY) / 2;
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

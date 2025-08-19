using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using WorldServer.Services.World;
using Color = System.Drawing.Color;

// This file is all about loading and managing data from the game client's files.
// It reads things like maps and other data from images and CSV files to build the game world.
namespace WorldServer.Managers
{
    // A simple helper to hold information about which realm has influence over a specific area.
    public struct AreaInfluence
    {
        public ushort AreaNumber;
        public byte Realm;
        public ushort InfluenceId;
    }

    // This class holds all the information about a single zone that we load from the client files.
    // It's like a folder for all the map data for one part of the world.
    public class ClientZoneInfo
    {
        public ushort ZoneId;
        public string Folder;
        public List<AreaInfluence> Influences;
        public List<Zone_Area> Areas;
        public List<PQuest_Info> PQAreas;
        public Color[,] HeightMapOffset;
        public Color[,] HeightMapTerrain;
        // This is a 2D map of the zone, where each pixel represents an area.
        public byte[,] AreaPixels = new byte[1024, 1024];
        // This is a 2D map of the zone, where each pixel represents a Public Quest area.
        public byte[,] PQAreaPixels = new byte[1024, 1024];

        public ClientZoneInfo(ushort zoneId)
        {
            ZoneId = zoneId;
            Influences = new List<AreaInfluence>();
            Folder = Core.Config.ZoneFolder + "zone" + string.Format("{0:000}", zoneId) + "/";
            Areas = ZoneService.GetZoneAreas(zoneId).OrderBy(area => area.PieceId).ToList();

            try
            {
                // When a new ClientZoneInfo is created, it loads all the necessary data from the files.
                LoadInfluences();
                LoadAreaMap();
                LoadPQAreaMap();
            }
            catch (Exception e)
            {
                Log.Error("ClientFile", e.ToString());
            }
        }

        // This loads the height map images for the zone. (Currently commented out)
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

        // This loads the area map from an image file. Each pixel's color in the image
        // corresponds to a specific area in the zone.
        public void LoadAreaMap()
        {
            string filePath = Path.Combine(Folder, "areas" + $"{ZoneId:000}" + ".png");
            if (File.Exists(filePath))
            {
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
            }
        }

        // This loads the Public Quest area map from an image file. It works just like the area map,
        // but for Public Quests.
        public void LoadPQAreaMap()
        {
            string filePath = Path.Combine(Folder, "pqarea" + $"{ZoneId:000}" + ".png");
            if (File.Exists(filePath))
            {
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
            }
        }

        // This loads the influence data from a CSV file.
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

        // This figures out which area a player is in based on their coordinates.
        public Zone_Area GetZoneAreaFor(ushort pinX, ushort pinY, ushort zoneId, ushort pinz = 0)
        {
            byte areaId = AreaPixels[pinX >> 6, pinY >> 6];
            // fix for black craig keep in the dungeon
            if (ZoneId == 3 && areaId > 20)
            {
                if (pinz < 8394)
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

        // This figures out which Public Quest area a player is in.
        public byte GetPQAreaFor(ushort pinX, ushort pinY, ushort zoneId)
        {
            return PQAreaPixels[pinX >> 6, pinY >> 6];
        }
    }

    // This class holds the height map information for a zone.
    // A height map is like a topographical map that tells you how high the ground is at any point.
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

        // This gets the height of the ground at a specific x, y coordinate.
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

            float fZValue = 0.0f;

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

        // This loads the height map image files from the disk.
        public void Load()
        {
            if (_loaded)
                return;

            _loaded = true;

            try
            {
                Offset = new Bitmap(Core.Config.ZoneFolder + "zone" + string.Format("{0:000}", ZoneID) + "/offset.png"); // /zones/zone003/offset.png
                Terrain = new Bitmap(Core.Config.ZoneFolder + "zone" + string.Format("{0:000}", ZoneID) + "/terrain.png"); // /zones/zone003/offset.png
            }
            catch (Exception e)
            {
                Log.Error("HeightMap", "[" + ZoneID + "] Invalid HeightMap \n " + e);
            }
        }
    }

    // This is the Client File Manager. It keeps all the loaded client file data in memory
    // so we don't have to read it from the disk every time we need it. It's a cache.
    public static class ClientFileMgr
    {
        #region HeightMap Images

        // This holds all the loaded height map information.
        public static Dictionary<int, HeightMapInfo> Heights = new Dictionary<int, HeightMapInfo>();

        // This gets the height of the ground in a specific zone.
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

        #endregion HeightMap Images

        #region MapPiece and CSV

        // This holds all the loaded zone information.
        public static Dictionary<ushort, ClientZoneInfo> ClientZoneFiles = new Dictionary<ushort, ClientZoneInfo>();

        // This gets the zone information for a specific zone.
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

        #endregion MapPiece and CSV
    }
}
using FrameWork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using WorldServer.World.Physics;

namespace WorldServer.World.Map
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FixtureInfo
    {
        public float X1;
        public float Y1;
        public float Z1;

        public float X2;
        public float Y2;
        public float Z2;

        public int SurfaceType;
        public int UniqueID;
        public float Area { get; set; }

        public float Width
        {
            get
            {
                return X2 - X1;
            }
        }

        public float Height
        {
            get
            {
                return Y2 - Y1;
            }
        }

        public float Depth
        {
            get
            {
                return Z2 - Z1;
            }
        }
    }

    public class Occlusion : IOcclusionProvider
    {
        public bool Initialized { get; set; }

        public int TrisCount = 190;

        public Occlusion()
        {
        }

        public void InitZones(string path)
        {
            if (!Initialized)
            {
                InitZones(path, TrisCount);
                var tasks = new List<Task>();

                foreach (var file in Directory.GetFiles(path, "*.bin"))
                    WorldServer.World.Map.Occlusion.LoadZone(int.Parse(Path.GetFileNameWithoutExtension(file)));

                //Task.WhenAll(tasks).Wait();
                //Testing
                /* Stopwatch watch = Stopwatch.StartNew();
                 watch.Start();
                 for (int i = 0; i < 1000; ++i)
                 {
                     for (int j = 0; j < 1000; ++j)
                         Log.Info("[Z TEST]", $"{i} {j} {GetTerrainZ(1, i, j)}");
                 }
                 watch.Stop();
                 Log.Info("[Z TEST]", $"1000000 get terrainZ done in: {watch.ElapsedMilliseconds} ms. {(float)watch.ElapsedMilliseconds / 1000000f} ms per Z call");
                 watch = Stopwatch.StartNew();
                 OcclusionInfo result = new OcclusionInfo();
                 int notOccluded = 0;
                 int occluded = 0;
                 watch.Start();
                 for (int i = 0; i < 1000; ++i)
                 {
                     for (int j = 0; j < 1000; ++j)
                     {
                         Raytest(1, i, j, 65535f, i, j, 65535f, true, ref result);
                         if(result.Result != OcclusionResult.NotOccluded)
                         {
                             if(result.SurfaceType == SurfaceType.TERRAIN)
                             {
                                 Log.Info("[RAY TEST]", $"Hit terrain at: {result.HitX} {result.HitY} {result.HitZ}");
                             }
                             else if(result.SurfaceType == SurfaceType.FIXTURE)
                             {
                                 Log.Info("[RAY TEST]", $"Hit fixture ({result.FixtureID}) at: {result.HitX} {result.HitY} {result.HitZ}");
                             }else if(result.SurfaceType == SurfaceType.WATER_BOG)
                             {
                                 Log.Info("[RAY TEST]", $"Hit water at: {result.HitX} {result.HitY} {result.HitZ}");
                             }
                             occluded++;
                         }
                         else
                         {
                             notOccluded++;
                         }
                     }
                 }
                  watch.Stop();
                 Log.Info("[RAY TEST]", $"1000000 raytests done in: {watch.ElapsedMilliseconds} ms. {(float)watch.ElapsedMilliseconds / 1000000f} ms per Z call");
                 */
                Initialized = true;
            }
        }

        public int GetTerrainZ(int zoneId, int x, int y)
        {
            return Occlusion.Pin(zoneId, x, y);
        }

        public int Raytest(int zoneIDA,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ,
        bool terrain, ref OcclusionInfo result)
        {
            return Occlusion.SegmentIntersect(zoneIDA, zoneIDA,
                                              originX, originY, originZ,
                                              targetX, targetY, targetZ,
                                              terrain, true, TrisCount, ref result);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "Pin")]
        public static extern int Pin(int zoneId, int x, int y);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "InitZones")]
        private static extern void InitZones(string path, int triCount);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "LoadZone")]
        private static extern void LoadZoneInternal(int zoneID);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "LoadZone")]
        public static extern void LoadZone(int zoneID);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "SegmentIntersect")]
        public static extern int SegmentIntersect(int zoneIDA, int zoneIDB,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ,
        bool terrain, bool normalTest, int triCount, ref OcclusionInfo result);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "TerrainIntersect")]
        public static extern bool TerrainIntersect(int zoneIDA, int zoneIDB,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ, int triCount, ref OcclusionInfo result);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetFixtureCount")]
        public static extern int GetFixtureCount(int zoneID);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetFixtureInfo")]
        public static extern bool GetFixtureInfo(int zoneID, int index, ref FixtureInfo info);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "SetFixtureVisible")]
        public static extern bool SetFixtureVisible(int zoneID, UInt32 uniqueID, byte instanceID, bool visible);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetFixtureVisible")]
        public static extern bool GetFixtureVisible(int zoneID, UInt32 uniqueID, byte instanceID);

        public static bool SetFixtureVisible(uint doorID, bool visible)
        {
            if (doorID == 3169384)
                return true;

            var zoneID = ((int)doorID >> 20) & 0x3FF;
            int uniqueID = ((((int)doorID >> 30) & 0x3) << 14) | (((int)doorID >> 6) & 0x3FFF);
            int doorIndex = ((int)doorID & 0x3F) - 0x28;

            return SetFixtureVisible(zoneID, (uint)uniqueID, (byte)(doorIndex + 1), visible);
        }

        public static bool GetFixtureVisible(uint doorID)
        {
            var zoneID = ((int)doorID >> 20) & 0x3FF;
            int uniqueID = ((((int)doorID >> 30) & 0x3) << 14) | (((int)doorID >> 6) & 0x3FFF);
            int doorIndex = ((int)doorID & 0x3F) - 0x28;

            return GetFixtureVisible(zoneID, (uint)uniqueID, (byte)(doorIndex + 1));
        }
    }
}
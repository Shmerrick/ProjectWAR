using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WorldServer.World.Map
{
    public enum SurfaceType : int
    {
        SOLID = 0,
        //doors
        DOOR1 = 1,
        DOOR2 = 2,
        DOOR3 = 3,
        DOOR4 = 4,
        DOOR5 = 5,
        DOOR6 = 6,
        DOOR7 = 7,
        DOOR8 = 8,
        DOOR9 = 9,

        //waters
        WATER_GENERIC = 10,
        WATER_RIVER = 11,
        WATER_HOTSPRING = 12,
        WATER_OCEAN = 13,
        WATER_DIRTY = 14,
        WATER_STREAM = 15,
        WATER_TAINTED = 16,
        WATER_BOG = 17,
        WATER_ICY = 18,
        WATER_POISON = 19,
        WATER_LAKE = 20,
        WATER_MARSH = 21,
        WATER_MUCK = 22,

        //lavas
        LAVA = 23,
        LAVA_MAGMA = 24,

        //other
        TAR = 25,
        INSTANT_DEATH = 26,
        FIXTURE = 27,
        TERRAIN = 28,

        JUMP1 = 29,
        JUMP2 = 30,
        JUMP3 = 31,
        JUMP4 = 32,
        JUMP5 = 33,
        JUMP6 = 34,
        JUMP7 = 35,

    }

    public enum OcclusionResult : int
    {
        NotLoaded = -1,
        NotOccluded = 0,
        OccludedByGeometry = 1,
        OccludedByTerrain = 2,
        OccludedByWater = 3,
        OccludedByLava = 4,
        OccludedByDynamicObject = 5,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OcclusionInfo
    {
        public OcclusionResult Result;
        public float HitX;
        public float HitY;
        public float HitZ;
        public float SafeX;
        public float SafeY;
        public float SafeZ;
        public int FixtureID;
        public SurfaceType SurfaceType;
        public float WaterDepth;

        public override string ToString()
        {
            return "";
            //return $"Result:{Result} HitX:{HitX} HitY:{HitY} HitZ{HitZ} Surface:{SurfaceType} WaterDepth:{WaterDepth} Fixture{FixtureID}";
        }
    }

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

    public static class Occlusion
    {
        public static bool Initialized { get; private set; }

        public static void InitZones(string path)
        {
          
            
            if (!Initialized)
            {
                InitZones(path, 190);
                var tasks = new List<Task>();

                foreach (var file in Directory.GetFiles(path, "*.bin"))
                    tasks.Add(Task.Run(()=>WorldServer.World.Map.Occlusion.LoadZone(int.Parse(Path.GetFileNameWithoutExtension(file)))));

                Task.WhenAll(tasks).Wait();
                
                Initialized = true;
            }

           
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitZones(string path, int triCount);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "LoadZone")]
        private static extern void LoadZoneInternal(int zoneID);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void LoadZone(int zoneID);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SegmentIntersect(int zoneIDA, int zoneIDB,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ,
        bool terrain, bool normalTest, int triCount, ref OcclusionInfo result);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool TerrainIntersect(int zoneIDA, int zoneIDB,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ, int triCount, ref OcclusionInfo result);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetFixtureCount(int zoneID);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetFixtureInfo(int zoneID, int index, ref FixtureInfo info);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetFixtureVisible(int zoneID, UInt32 uniqueID, byte instanceID, bool visible);

        [SuppressUnmanagedCodeSecurity]
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
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

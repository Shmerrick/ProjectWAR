using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;

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

        /// <summary>Upper bound on zone ids; door ids encode the zone in 10 bits.</summary>
        private const int MaxZoneId = 1024;

        /// <summary>
        /// Per-zone load state. Indexed by zone id so the steady-state check on the LOS hot path is a
        /// single array read with no allocation and no lock.
        /// </summary>
        private static readonly bool[] _zoneLoaded = new bool[MaxZoneId];

        /// <summary>Zone ids that actually have a .bin on disk. Populated once by <see cref="InitZones(string)"/>.</summary>
        private static readonly HashSet<int> _availableZones = new HashSet<int>();

        private static readonly object _zoneLoadLock = new object();

        /// <summary>
        /// Prepares the native occlusion library and records which zones have collision data available,
        /// without loading any of it. Zone geometry is faulted in on demand by <see cref="EnsureZoneLoaded"/>.
        /// Loading all zones up front cost roughly 7 GB of native memory on a server with no players online.
        /// </summary>
        public static void InitZones(string path)
        {
            if (Initialized)
                return;

            InitZones(path, 190);

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path, "*.bin"))
                {
                    if (int.TryParse(Path.GetFileNameWithoutExtension(file), out int zoneId)
                        && zoneId >= 0 && zoneId < MaxZoneId)
                        _availableZones.Add(zoneId);
                }
            }

            Initialized = true;

            FrameWork.Log.Success("Occlusion",
                "Occlusion ready - " + _availableZones.Count + " zones available, loaded on demand");
        }

        /// <summary>
        /// Loads a zone's collision geometry the first time that zone is needed, then never again.
        /// Safe to call from any thread and from hot paths: the common case is one array read.
        /// Zones with no collision data on disk are marked resolved so they are not rescanned.
        /// </summary>
        public static void EnsureZoneLoaded(int zoneId)
        {
            if (!Initialized || zoneId < 0 || zoneId >= MaxZoneId)
                return;

            if (Volatile.Read(ref _zoneLoaded[zoneId]))
                return;

            lock (_zoneLoadLock)
            {
                if (_zoneLoaded[zoneId])
                    return;

                if (_availableZones.Contains(zoneId))
                {
                    LoadZone(zoneId);
                    FrameWork.Log.Debug("Occlusion", "Loaded collision data for zone " + zoneId);
                }

                Volatile.Write(ref _zoneLoaded[zoneId], true);
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
        [DllImport(@"WarZone64.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SegmentIntersect")]
        private static extern int SegmentIntersectNative(int zoneIDA, int zoneIDB,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ,
        bool terrain, bool normalTest, int triCount, ref OcclusionInfo result);

        /// <summary>
        /// Line-of-sight test between two points. Both zones are faulted in on first use, so callers
        /// never have to know whether the geometry is resident yet.
        /// </summary>
        public static int SegmentIntersect(int zoneIDA, int zoneIDB,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ,
        bool terrain, bool normalTest, int triCount, ref OcclusionInfo result)
        {
            EnsureZoneLoaded(zoneIDA);

            if (zoneIDB != zoneIDA)
                EnsureZoneLoaded(zoneIDB);

            return SegmentIntersectNative(zoneIDA, zoneIDB, originX, originY, originZ,
                targetX, targetY, targetZ, terrain, normalTest, triCount, ref result);
        }

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

            EnsureZoneLoaded(zoneID);

            return SetFixtureVisible(zoneID, (uint)uniqueID, (byte)(doorIndex + 1), visible);
        }

        public static bool GetFixtureVisible(uint doorID)
        {
            var zoneID = ((int)doorID >> 20) & 0x3FF;
            int uniqueID = ((((int)doorID >> 30) & 0x3) << 14) | (((int)doorID >> 6) & 0x3FFF);
            int doorIndex = ((int)doorID & 0x3F) - 0x28;

            EnsureZoneLoaded(zoneID);

            return GetFixtureVisible(zoneID, (uint)uniqueID, (byte)(doorIndex + 1));
        }
    }
}

using System.Runtime.InteropServices;

namespace WorldServer.World.Physics
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
            return $"Result:{Result} HitX:{HitX} HitY:{HitY} HitZ{HitZ} Surface:{SurfaceType} WaterDepth:{WaterDepth} Fixture{FixtureID}";
        }
    }

    public interface IOcclusionProvider
    {
        bool Initialized { get; set; }

        void InitZones(string path);

        int GetTerrainZ(int zoneId, int x, int y);

        int Raytest(int zoneID,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ,
        bool terrain, ref OcclusionInfo result);
    }
}
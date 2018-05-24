using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WarZoneLib
{
    public class RegionData
    {
        public static RegionInfo[] _regions = new RegionInfo[512];
        public static ZoneInfo[] _zones = new ZoneInfo[512];
        public static bool DebugEnable = false;
        
        public enum LoadResult
        {
            OK,                 // Zone data was loaded successfully
            NotFound,           // The file could not be found
            Invalid,            // The file contents are not valid zone data
            VersionMismatch,    // The file contents cannot be parsed because of a version mismatch
            NotImplemented,     // The file relies on a feature which is not yet implemented
            InternalError       // An unspecified error prevented loading
        };

        public static LoadResult LoadDataFile(uint regionID, String pathToFile, bool includeTerrain=false)
        {
            try
            {
                using (FileStream fileStream = File.OpenRead(pathToFile))
                    return LoadDataStream(fileStream, includeTerrain);
            }
            catch (FileNotFoundException)
            {
                return LoadResult.NotFound;
            }
            catch (NotImplementedException)
            {
                return LoadResult.NotImplemented;
            }
            catch
            {
                return LoadResult.InternalError;
            }
        }

        public static bool HasRegion(ushort regionID)
        {
            return _regions[regionID] != null && _regions[regionID].ID != 0;
        }
        public static LoadResult LoadDataStream(Stream stream, bool includeTerrain = false)
        {
            try
            {
                BinaryReader reader = new BinaryReader(stream);

                // Read the file signature and verify it
                Byte b0 = reader.ReadByte(), b1 = reader.ReadByte(), b2 = reader.ReadByte();
                if (b0 != 'R' || b1 != 'O' || b2 != 'R')
                    return LoadResult.Invalid;

                // Read the file format version and check it against what we expect
                Byte version = reader.ReadByte();
                if (version != FileFormatVersion)
                    return LoadResult.VersionMismatch;

                // Read the size of the header
                Byte headerSize = reader.ReadByte();

                // Seek to the start of the first chunk
                stream.Seek(headerSize, SeekOrigin.Begin);

                // Main read loop
                RegionInfo region = null;
                while (stream.Position < stream.Length)
                {
                    ChunkType chunkType = (ChunkType) reader.ReadUInt32();
                    uint chunkSize = reader.ReadUInt32();

                    long nextChunk = stream.Position + chunkSize;

                    switch (chunkType)
                    {
                        case ChunkType.Region:
                            region = LoadRegionChunk(reader);
                            break;
                        case ChunkType.Collision:
                            LoadCollisionChunk(reader);
                            break;
                        case ChunkType.BSP:
                            LoadBSPChunk(reader);
                            break;
                        case ChunkType.Terrain:
                            if(includeTerrain)
                                LoadTerrainChunk(reader);
                            break;
                    }

                    stream.Seek(nextChunk, SeekOrigin.Begin);
                }

             
               _regions[region.ID] = region;
                
                return LoadResult.OK;
            }
            catch(Exception e)
            {
                return LoadResult.InternalError;
            }
        }

        private static void LoadTerrainChunk(BinaryReader reader)
        {
            var zoneID = reader.ReadUInt32();
            if (_zones[zoneID] == null)
                _zones[zoneID] = new ZoneInfo();

            var width = reader.ReadUInt32();
            var height = reader.ReadUInt32();
            TerrainInfo t = new TerrainInfo()
            {
                Height = height,
                Width = width,
                HeightMap = new ushort[width, height]
            };

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    t.HeightMap[x, y] = (ushort)reader.ReadUInt32();
                }
            _zones[zoneID].Terrain = t;
        }

        public static bool SetZoneEnabled(int zoneId, bool enabled)
        {
            return true;
            //try
            //{
            //    if (_zones[zoneId] == null)
            //        return false;
            //    _zones[zoneId].Enabled = enabled;
            //    return true;
            //}
            //catch
            //{
            //    return false;
            //}
        }

        private static RegionInfo LoadRegionChunk(BinaryReader reader)
        {
            RegionInfo region = new RegionInfo();

            region.ID = reader.ReadUInt32();

            var zoneCount = reader.ReadUInt32();
            for (int i = 0; i < zoneCount; i++)
            {
               var zone = LoadZone(reader);
               zone.Region = region;
               region.Zones[zone.ID] = zone;
               _zones[zone.ID] = zone;
            }


            return region;
        }

        private static ZoneInfo LoadZone(BinaryReader reader)
        {
         
            var id = reader.ReadUInt32();
            if (_zones[id] == null)
                _zones[id] = new ZoneInfo();

             var zone = _zones[id];

            zone.ID = id;
            zone.OffsetX = reader.ReadUInt32();
            zone.OffsetY = reader.ReadUInt32();

            // These are not really needed right now, and are just hints anyway
            UInt32 numNifs = reader.ReadUInt32();
            UInt32 numFixtures = reader.ReadUInt32();

            return zone;
        }

        private static CollisionInfo LoadCollisionChunk(BinaryReader reader)
        {
            CollisionInfo collision = new CollisionInfo();

            uint zoneID = reader.ReadUInt32();

            // Read collision vertices
            uint vertexCount = reader.ReadUInt32();
            collision.Vertices = new Vector3[vertexCount];
            for (uint i = 0; i < vertexCount; i++)
                collision.Vertices[i] = ReadVector3(reader);

            // Read collision triangles
            uint triangleCount = reader.ReadUInt32();
            uint indexSize = reader.ReadUInt32();
            collision.Triangles = new TriangleInfo[triangleCount];
          //  int ii = 0;
            if (indexSize == 4)
            {
                for (uint i = 0; i < triangleCount; i++)
                {
                    collision.Triangles[i] = new TriangleInfo()
                    {
                        i0 = reader.ReadUInt32(),
                        i1 = reader.ReadUInt32(),
                        i2 = reader.ReadUInt32(),
                        fixture = reader.ReadUInt32()
                    };

                    if (!collision.Fixtures.ContainsKey(collision.Triangles[i].fixture))
                        collision.Fixtures[collision.Triangles[i].fixture] = new FixtureInfo();

                    collision.Fixtures[collision.Triangles[i].fixture].Triangles.Add(i);
                }
                //for (uint i = 0; i <3 * triangleCount; i++)
                //{
                //    collision.Indices[i] = reader.ReadUInt32();
                //}

            }
            else if (indexSize == 2)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new Exception();
            }


            if (_zones[zoneID] == null)
                _zones[zoneID] = new ZoneInfo();

            _zones[zoneID].Collision = collision;
            return collision;
        }

        private static void LoadBSPChunk(BinaryReader reader)
        {
            uint zoneID = reader.ReadUInt32();

            uint bspDepthCutoff = reader.ReadUInt32();
            uint bspTrianglesCutoff = reader.ReadUInt32();

            _zones[zoneID].Collision.BSP = ReadBSPNode(reader);
        }

        private static BSPNodeInfo ReadBSPNode(BinaryReader reader)
        {
            BSPNodeInfo node = new BSPNodeInfo();

            bool isLeaf = reader.ReadByte() != 0;

            if (isLeaf)
            {
                UInt32 numTriangles = reader.ReadUInt32();
                node.Triangles = new int[numTriangles];
                for (UInt32 i = 0; i < numTriangles; i++)
                    node.Triangles[i] = reader.ReadInt32();
            }
            else
            {
                node.P.N   = ReadVector3(reader);
                node.P.D   = reader.ReadSingle();
                node.Back  = ReadBSPNode(reader);
                node.Front = ReadBSPNode(reader);
            }

            return node;
        }

        private static Vector3 ReadVector3(BinaryReader reader)
        {
            Vector3 v;
            v.X = reader.ReadSingle();
            v.Y = reader.ReadSingle();
            v.Z = reader.ReadSingle();
            return v;
        }

        // Controls whether or not queries are executed.
        // Zone data can still be loaded while Enabled = false.
        public static bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        private static bool _enabled = true;

        public enum OcclusionResult
        {
            Occluded,           // The line or ray is occluded against the zone geometry
            NotOccluded,        // The line or ray is not occluded
            NotEnabled,         // The occlusion system is disabled
            NotLoaded,          // Zone data is not loaded for one or more zones in the query
            NotImplemented,     // The query relies on a feature that is not yet implemented
            InternalError       // An unspecified error occurred while processing the query
        };


        public static void HideDoor(bool hide, ushort zoneID, uint uniqueID, uint instance = 0)
        {
            uint result = (uint)
                  (
                      (int)((uniqueID & 0xC000) << 16) |
                      (int)((zoneID & 0x3FF) << 20) |
                      (int)((uniqueID & 0x3FFF) << 6) |
                      (int)(0x28 + instance)
                  );

            if (_zones[zoneID].ID != 0)
            {
                if (_zones[zoneID].Collision.Fixtures.ContainsKey(result))
                    _zones[zoneID].Collision.Fixtures[result].Hidden = hide;
            }
        }

        public static void HideDoor(bool hide, ushort zoneID, uint doorID)
        {
            if (_zones[zoneID] != null && _zones[zoneID].ID != 0)
            {
                var zone = _zones[zoneID];

                if (zone != null && zone.Collision.Fixtures.ContainsKey(doorID))
                    zone.Collision.Fixtures[doorID].Hidden = hide;
            }
        }


        public static ZoneInfo PinZone(uint regionID, int x, int y)
        {
            foreach (var zone in _zones.Where(e=>e != null && e.Region.ID == regionID).ToList())
            {
                if (x >= zone.OffsetX && x <= zone.OffsetX + 0xFFFF &&
                    y >= zone.OffsetY && y <= zone.OffsetY + 0xFFFF)
                    return zone;

            }
            return null;
        }

        public static OcclusionResult OcclusionQuery(
           ushort zoneId0, float x0, float y0, float z0,
           ushort zoneId1, float x1, float y1, float z1)
        {
            return OcclusionQuery(zoneId0, x0, y0, z0, zoneId1, x1, y1, z1);
        }

        public static Vector3 Lerp(Vector3 a, Vector3 b, double t)
        {
            var x = a.X + (b.X - a.X) * t;
            var y = a.Y + (b.Y - a.Y) * t;
            var z = a.Z + (b.Z - a.Z) * t;
            return new Vector3((float)x, (float)y, (float)z);
        }

        public static double GetDistance(Vector3 locA, Vector3 locB)
        {
            double deltaX = (int)locB.X - (int)locA.X;
            double deltaY = (int)locB.Y - (int)locA.Y;
            double deltaZ = (int)locB.Z - (int)locA.Z;

            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }

        public static OcclusionResult OcclusionQuery(
            ushort zoneId0, float x0, float y0, float z0,
            ushort zoneId1, float x1, float y1, float z1, ref Vector3 hitpoint)
        {

            OcclusionResult result;
            SegmentTestInfo info = new SegmentTestInfo();

            long timeBegin = DebugEnable ? Timer.Value : 0;

            try
            {
                ZoneInfo zone = _zones[zoneId0];

                if (zone == null || zone.Collision == null)
                {
                    result = OcclusionResult.NotLoaded;
                }
                else if (!zone.Enabled)
                {
                    return OcclusionResult.NotEnabled;
                }
                else if (zoneId0 != zoneId1)
                {
                    result = OcclusionResult.NotImplemented;
                }
                else
                {
                    x0 = 65535.0f - x0;
                    x1 = 65535.0f - x1;

                    Vector3 p0 = new Vector3(x0, y0, z0), p1 = new Vector3(x1, y1, z1);


                    if (SegmentTest(p0, p1, zone.Collision, zone.Collision.BSP, 0, ref info))
                    {
                        bool hit = true;
                        double nearest = GetDistance(p0, info.HitPoint);
                        var hitPoint = new Vector3(info.HitPoint.X, info.HitPoint.Y, info.HitPoint.Z);
                        hitpoint = info.HitPoint;

                        int count = 0;

                        //find the nearest to origin triangle hit
                        while (hit)
                        {
                            var nextPoint = Lerp(info.HitPoint,p0 , 0.01);
                            hit = SegmentTest(p0, nextPoint, zone.Collision, zone.Collision.BSP, 0, ref info);
               

                            if (hit && (info.HitPoint.X != hitPoint.X || info.HitPoint.Y != hitPoint.Y || info.HitPoint.Z != hitPoint.Z) &&
                                GetDistance(p0, info.HitPoint) < nearest)
                            {
                                hitpoint = info.HitPoint;
                            }

                            hitPoint = new Vector3(info.HitPoint.X, info.HitPoint.Y, info.HitPoint.Z);

                            if (count > 100)
                                break;

                            count++;
                        }
                        result = OcclusionResult.Occluded;
                        hitpoint.X = 65535.0f - hitpoint.X;

                    }
                    else
                    {
                        result = OcclusionResult.NotOccluded;
                    }

                }
            }
            catch
            {
                result = OcclusionResult.InternalError;
            }

            if (DebugEnable)
            {
                long microseconds = (Timer.Value - timeBegin) * 1000000 / Timer.Frequency;
                float timePerTri = microseconds / (float)info.NumTrianglesTested;

                String msg = String.Format(
                  "LOS Check: ({0}, {1}, {2}, {3}) -> ({4}, {5}, {6}, {7}) -> ",
                  zoneId0, x0, y0, z0, zoneId1, x1, y1, z1);

                Console.Write(msg);
                Console.WriteLine(result.ToString() + " [nodes=" + info.NumNodesTested + ", triangles=" + info.NumTrianglesTested + ", μs=" + microseconds + ", μs/tri=" + timePerTri + "]");
            }

            return result;

        }

        private struct SegmentTestInfo
        {
            public int NumNodesTested, NumTrianglesTested, MaxDepthReached;
            public TriangleInfo HitTriangle;
            public Vector3 HitPoint;
        };

        private static bool SegmentTest(Vector3 p0, Vector3 p1, CollisionInfo collision, BSPNodeInfo node, int depth, ref SegmentTestInfo info)
        {
            if (node == null)
                return false;

            info.NumNodesTested++;
            info.MaxDepthReached = Math.Max(info.MaxDepthReached, depth);

            if (node.IsLeaf)
            {
                TriangleInfo hitTriangle = new TriangleInfo();
                int tri = NodeTriangleTest(p0, p1, collision, node, ref info.HitPoint);
                info.NumTrianglesTested += (tri != -1) ? (tri + 1) : node.Triangles.Length;
                info.HitTriangle = hitTriangle;
                return tri != -1;
            }
            else
            {
                float d0 = Vector3.Dot(node.P.N, p0) - node.P.D;
                float d1 = Vector3.Dot(node.P.N, p1) - node.P.D;

                if (d0 < -Epsilon && d1 < -Epsilon)
                {
                    // Both points behind plane
                    return SegmentTest(p0, p1, collision, node.Back, depth + 1, ref info);
                }
                else if (d0 > Epsilon && d1 > Epsilon)
                {
                    // Both points in front of plane
                    return SegmentTest(p0, p1, collision, node.Front, depth + 1, ref info);
                }
                else
                {
                    // Points span plane
                    return SegmentTest(p0, p1, collision, node.Front, depth + 1, ref info) || SegmentTest(p0, p1, collision, node.Back, depth + 1, ref info);
                }
            }
        }

        private const float Epsilon = 0.00001f;

        private static int NodeTriangleTest(Vector3 p0, Vector3 p1, CollisionInfo collision, BSPNodeInfo node, ref Vector3 hitPoint)
        {
            if (node.Triangles.Length == 0)
                return -1;
            
            Vector3 d = (p1 - p0);

            float distanceToTarget = d.Length;

            // If the end point is basically the same as the start point, then there is no occlusion
            if (d.Length < Epsilon)
                return -1;

            d = d.Normalize();

            for (int i = 0; i < node.Triangles.Length; i++)
            {
                int t = node.Triangles[i];

                var triangle = collision.Triangles[t];
                var fixture = collision.Fixtures[triangle.fixture];

                if (fixture.Hidden)
                    return -1;

                Vector3 v0 = collision.Vertices[triangle.i0];
                Vector3 v1 = collision.Vertices[triangle.i1];
                Vector3 v2 = collision.Vertices[triangle.i2];

                float distance;
                if (RayTestTriangle(out distance, p0, d, v0, v1, v2, ref hitPoint))
                    if (distance < distanceToTarget)
                    {
                        return i;
                    }
            }

            return -1;
        }


        private static bool RayTestTriangle(
            out float distance,
            Vector3 o,
            Vector3 d,
            Vector3 v1,
            Vector3 v2,
            Vector3 v3,
            ref Vector3 hitPoint
            )
        {
            distance = 0;

            Vector3 e1, e2;
            Vector3 P, Q, T;
            float det, inv_det, t, u, v;

            e1 = v2 - v1;
            e2 = v3 - v1;

            P = Vector3.Cross(d, e2);
            det = Vector3.Dot(e1, P);

            if (det > -Epsilon && det < Epsilon)
                return false;
            inv_det = 1.0f / det;

            T = o - v1;
            u = Vector3.Dot(T, P) * inv_det;
            if (u < 0.0f || u > 1.0f)
                return false;

            Q = Vector3.Cross(T, e1);
            v = Vector3.Dot(d, Q) * inv_det;
            if (v < 0.0f || u + v > 1.0f)
                return false;

            t = Vector3.Dot(e2, Q) * inv_det;

            if (t > Epsilon)
            {
                distance = t;
                hitPoint = new Vector3(o.X + d.X * t, o.Y + d.Y * t, o.Z + d.Z * t);
                return true;
            }
            else
            {
                return false;
            }
        }


    private const int FileFormatVersion = 2; // The file format version we are expecting
    }
}

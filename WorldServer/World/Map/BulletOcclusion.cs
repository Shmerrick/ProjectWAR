using BulletSharp;
using BulletSharp.Math;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WorldServer.World.Physics;

namespace WorldServer.Physics
{
    public class BulletOcclusion : IOcclusionProvider
    {
        public bool Initialized { get; set; } = false;
        public PhysicsZone[] Zones { get; private set; }

        //public Dictionary<string, BvhTriangleMeshShape> Fixtures { get; set; }
        public void InitZones(string path)
        {
            if (!Initialized)
            {
                Zones = new PhysicsZone[500];
                //Fixtures = new Dictionary<string, BvhTriangleMeshShape>();
                /*if (File.Exists(Path.Combine(path, @"fixtures.bin"))) //fast loading
                {
                    using (BinaryReader reader = new BinaryReader(new FileStream(Path.Combine(path, @"fixtures.bin"), FileMode.Open)))
                    {
                        byte version = reader.ReadByte();
                        int amt = reader.ReadInt32();
                        for (int i = 0; i < amt; ++i)
                        {
                            string name = reader.ReadString();
                            byte[] bytes = reader.ReadBytes(reader.ReadInt32());
                            ObjModel model = new ObjModel();
                            model.Deserialize(bytes);
                            if (model.Vertices.Length > 0 && model.Polygons.Length > 0)
                            {
                                TriangleIndexVertexArray indexVertexArray = new TriangleIndexVertexArray(model.Polygons, model.Vertices);
                                BvhTriangleMeshShape shape = new BvhTriangleMeshShape(indexVertexArray, false, true);
                                Fixtures.Add(name, shape);
                                //Console.WriteLine($"Loaded model: {name}");
                            }
                            else
                            {
                                Log.Error("[OCCLUSION]", $"Model {name} has no faces or polygons!");
                            }
                        }
                    }
                }*/

                foreach (var dir in Directory.GetDirectories(WorldServer.Core.Config.ZoneFolder))
                {
                    if (Directory.Exists(Path.Combine(dir, "textures")))
                    {
                        Directory.Delete(Path.Combine(dir, "textures"), true);
                    }
                    if (File.Exists(Path.Combine(dir, "terrain.obj")))
                    {
                        File.Delete(Path.Combine(dir, "terrain.obj"));
                    }
                    string num = dir.Replace("zones/", "").Replace("zone", "");
                    num = RemoveZeros(num);
                    int zoneID = Convert.ToInt32(num);

                    PhysicsZone zone = PhysicsZone.Build(this);
                    zone.LoadInfo(dir);
                    zone.LoadFixtures(Path.Combine(path, $"{zoneID}.bin"));
                    Zones[zoneID] = zone;
                    Log.Info("[OCCLUSION]", $"Loaded zone #{zoneID}");
                }
                //Testing

                /*
                Stopwatch w = Stopwatch.StartNew();
                w.Start();
                for (int i = 0; i < 1000; ++i)
                {
                    for (int j = 0; j < 1000; ++j)
                        GetTerrainZ(1, i, j);
                }
                w.Stop();
                Log.Info("[Z TEST]", $"1000000 get terrainZ done in: {w.ElapsedMilliseconds} ms. {(float)w.ElapsedMilliseconds / 1000000f} ms per Z call");

                Stopwatch watch = Stopwatch.StartNew();
                OcclusionInfo result = new OcclusionInfo();
                watch.Start();
                for (int i = 0; i < 1000; ++i)
                {
                    for (int j = 0; j < 1000; ++j)
                    {
                        Raytest(1, i * 5, j * 5, 65535f, i * 5, j * 5, 0, true, ref result);
                        if (result.Result != OcclusionResult.NotOccluded)
                        {
                            if (result.SurfaceType == SurfaceType.TERRAIN)
                            {
                                // Log.Info("[RAY TEST]", $"Hit terrain at: {result.HitX} {result.HitY} {result.HitZ}");
                            }
                            else if (result.SurfaceType == SurfaceType.FIXTURE)
                            {
                                Log.Info("[RAY TEST]", $"Hit fixture ({result.FixtureID}) at: {result.HitX} {result.HitY} {result.HitZ}");
                            }
                            else if (result.SurfaceType == SurfaceType.WATER_BOG)
                            {
                                Log.Info("[RAY TEST]", $"Hit water at: {result.HitX} {result.HitY} {result.HitZ}");
                            }
                        }
                    }
                }
                watch.Stop();
                Log.Info("[RAY TEST]", $"1000000 raytests done in: {watch.ElapsedMilliseconds} ms. {(float)watch.ElapsedMilliseconds / 1000000f} ms per Z call");
                */
                Initialized = true;
            }
        }

        public string RemoveZeros(string input)
        {
            int startIndex = -1;
            for (int i = 0; i < input.Length; ++i)
            {
                if (input[i] == '0')
                {
                    startIndex = i;
                }
                else
                {
                    break;
                }
            }
            return input.Substring(startIndex + 1);
        }

        private static AllHitsRayResultCallback zCallback = null;

        public int GetTerrainZ(int zoneId, int x, int y)
        {
            if (!Initialized)
                return 0;

            if (zCallback == null)
            {
                zCallback = new AllHitsRayResultCallback(new BulletSharp.Math.Vector3(0, 0, 0), new BulletSharp.Math.Vector3(0, 0, 0));
                zCallback.Flags += (int)TriangleRaycastCallback.EFlags.KeepUnflippedNormal;
                zCallback.Flags += (int)TriangleRaycastCallback.EFlags.FilterBackfaces;
                zCallback.Flags += (int)TriangleRaycastCallback.EFlags.UseSubSimplexConvexCastRaytest;
            }
            zCallback.CollisionObject = null;
            zCallback.HitPointWorld.Clear();
            zCallback.CollisionObjects.Clear();
            zCallback.HitFractions.Clear();
            zCallback.HitNormalWorld.Clear();
            if (Zones[zoneId] != null)
            {
                PhysicsZone zone = Zones[zoneId];
                BulletSharp.Math.Vector3 from = new BulletSharp.Math.Vector3(x, y, 65535f);
                BulletSharp.Math.Vector3 to = new BulletSharp.Math.Vector3(x, y, 0f);
                zCallback.RayFromWorld = from;
                zCallback.RayToWorld = to;
                zone.RayTestRef(ref from, ref to, zCallback);
                if (zCallback.HasHit)
                {
                    return (int)zCallback.HitPointWorld[0].Z;
                }
                return -1;
            }
            return -1;
        }

        private static AllHitsRayResultCallback rCallback = null;

        public int Raytest(int zoneID,
        float originX, float originY, float originZ,
        float targetX, float targetY, float targetZ,
        bool terrain, ref OcclusionInfo result)
        {
            if (!Initialized)
            {
                result.Result = OcclusionResult.NotLoaded;
                return 0;
            }
            if (rCallback == null)
            {
                rCallback = new AllHitsRayResultCallback(new BulletSharp.Math.Vector3(0, 0, 0), new BulletSharp.Math.Vector3(0, 0, 0));
                rCallback.Flags += (int)TriangleRaycastCallback.EFlags.KeepUnflippedNormal;
                rCallback.Flags += (int)TriangleRaycastCallback.EFlags.FilterBackfaces;
                rCallback.Flags += (int)TriangleRaycastCallback.EFlags.UseSubSimplexConvexCastRaytest;
            }
            rCallback.ClosestHitFraction = -1;
            rCallback.CollisionObject = null;
            rCallback.HitPointWorld.Clear();
            rCallback.CollisionObjects.Clear();
            rCallback.HitFractions.Clear();
            rCallback.HitNormalWorld.Clear();
            if (Zones[zoneID] != null)
            {
                PhysicsZone zone = Zones[zoneID];
                BulletSharp.Math.Vector3 from = new BulletSharp.Math.Vector3(originX, originY, originZ);
                BulletSharp.Math.Vector3 to = new BulletSharp.Math.Vector3(targetX, targetY, targetZ);
                rCallback.RayFromWorld = from;
                rCallback.RayToWorld = to;

                zone.RayTestRef(ref from, ref to, rCallback);
                if (rCallback.HasHit)
                {
                    int length = rCallback.CollisionObjects.Count;
                    for (int i = 0; i < length; ++i)
                    {
                        CollisionObject colObj = rCallback.CollisionObjects[i];
                        if (i == 0 && terrain && colObj is TerrainCB)
                        {
                            result.Result = OcclusionResult.OccludedByTerrain;
                            result.HitX = rCallback.HitPointWorld[i].X;
                            result.HitY = rCallback.HitPointWorld[i].Y;
                            result.HitZ = rCallback.HitPointWorld[i].Z;
                            result.SurfaceType = SurfaceType.TERRAIN;
                            return 1;
                        }
                        if (colObj is FixtureCB fRb)
                        {
                            result.Result = OcclusionResult.OccludedByGeometry;
                            result.HitX = rCallback.HitPointWorld[i].X;
                            result.HitY = rCallback.HitPointWorld[i].Y;
                            result.HitZ = rCallback.HitPointWorld[i].Z;
                            result.SurfaceType = SurfaceType.FIXTURE;
                            result.FixtureID = fRb.ID;
                            return 1;
                        }
                        if (colObj is WaterCB)
                        {
                            result.Result = OcclusionResult.OccludedByWater;
                            result.HitX = rCallback.HitPointWorld[i].X;
                            result.HitY = rCallback.HitPointWorld[i].Y;
                            result.HitZ = rCallback.HitPointWorld[i].Z;
                            result.SurfaceType = SurfaceType.WATER_BOG;
                            return 1;
                        }
                    }
                    result.Result = OcclusionResult.NotOccluded;
                    return -1;
                }
                result.Result = OcclusionResult.NotOccluded;
                return -1;
            }
            result.Result = OcclusionResult.NotLoaded;
            return -1;
        }
    }

    public class FixtureRigidBody : RigidBody
    {
        public int ID { get; private set; }
        public int NifID { get; private set; }
        public string Name { get; private set; }

        public FixtureRigidBody(RigidBodyConstructionInfo info, int id, int nifID, string name) : base(info)
        {
            ID = id;
            NifID = nifID;
            Name = name;
        }
    }

    public class WaterRigidBody : RigidBody
    {
        public string Name { get; private set; }
        public string Type { get; private set; }

        public WaterRigidBody(RigidBodyConstructionInfo info, string name, string type) : base(info)
        {
            Type = type;
            Name = name;
        }
    }

    public class TerrainRigidBody : RigidBody
    {
        public TerrainRigidBody(RigidBodyConstructionInfo info) : base(info)
        {
        }
    }

    public class FixtureCB : CollisionObject
    {
        public int ID { get; private set; }
        public int NifID { get; private set; }
        public string Name { get; private set; }

        public FixtureCB(RigidBodyConstructionInfo info, int id, int nifID, string name) : base()
        {
            CollisionShape = info.CollisionShape;
            WorldTransform = info.StartWorldTransform;
            ID = id;
            NifID = nifID;
            Name = name;
        }
    }

    public class WaterCB : CollisionObject
    {
        public string Name { get; private set; }
        public string Type { get; private set; }

        public WaterCB(RigidBodyConstructionInfo info, string name, string type) : base()
        {
            CollisionShape = info.CollisionShape;
            WorldTransform = info.StartWorldTransform;
            Type = type;
            Name = name;
        }
    }

    public class TerrainCB : CollisionObject
    {
        public TerrainCB(RigidBodyConstructionInfo info) : base()
        {
            CollisionShape = info.CollisionShape;
            WorldTransform = info.StartWorldTransform;
        }
    }

    public class PhysicsZone : DiscreteDynamicsWorld
    {
        public BulletOcclusion Occlusion { get; private set; }
        public CollisionConfiguration CollisionConf { get; private set; }
        public BvhTriangleMeshShape Terrain { get; private set; }
        public Dictionary<int, NifInfo> ModelInfos { get; private set; } = new Dictionary<int, NifInfo>();

        private PhysicsZone(DefaultCollisionConfiguration conf, CollisionDispatcher dispatcher, ConstraintSolver solver, BroadphaseInterface broadphase) : base(dispatcher, broadphase, solver, conf)
        {
            CollisionConf = conf;
            Gravity = new BulletSharp.Math.Vector3(0, 0, 0);
        }

        private static T BytesToStruct<T>(byte[] rawData) where T : struct
        {
            T result = default(T);
            try
            {
                IntPtr rawDataPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(rawData.Length);
                System.Runtime.InteropServices.Marshal.Copy(rawData, 0, rawDataPtr, rawData.Length);

                result = (T)System.Runtime.InteropServices.Marshal.PtrToStructure(rawDataPtr, typeof(T));
                System.Runtime.InteropServices.Marshal.FreeHGlobal(rawDataPtr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }

        public unsafe void LoadInfo(string path)
        {
            //Dictionary<int, FixtureInfo> FixtureInfos = new Dictionary<int, FixtureInfo>();

            /*if (File.Exists(Path.Combine(path, "zoneInfo.bin"))) //new fast reading
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(Path.Combine(path, "zoneInfo.bin"), FileMode.Open)))
                {
                    byte version = reader.ReadByte();

                    int count = reader.ReadInt32();
                    int structSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(NifInfo));
                    byte[] arr = new byte[structSize];
                    for (int i = 0; i < count; ++i)
                    {
                        reader.Read(arr, 0, arr.Length);
                        NifInfo info = BytesToStruct<NifInfo>(arr);
                        ModelInfos.Add(info.ID, info);
                    }
                    count = reader.ReadInt32();
                    structSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FixtureInfo));
                    arr = new byte[structSize];
                    for (int i = 0; i < count; ++i)
                    {
                        reader.Read(arr, 0, arr.Length);
                        FixtureInfo info = BytesToStruct<FixtureInfo>(arr);
                        FixtureInfos.Add(info.ID, info);
                    }
                }
            }
            GC.Collect();

            List<WaterBody> bodies = new List<WaterBody>();
            if (File.Exists(Path.Combine(path, "water.bin")))
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(Path.Combine(path, "water.bin"), FileMode.Open)))
                {
                    byte version = reader.ReadByte();
                    int count = reader.ReadInt32();
                    bodies.Capacity = count;
                    for (int i = 0; i < count; ++i)
                    {
                        WaterBody body = new WaterBody();
                        body.Deserialize(reader);
                        bodies.Add(body);
                    }
                }
            }
            GC.Collect();*/

            if (File.Exists(Path.Combine(path, "terrain.bin"))) //new fast reading
            {
                FileInfo info = new FileInfo(Path.Combine(path, "terrain.bin"));
                if (info.Length > 0)
                {
                    using (BinaryReader reader = new BinaryReader(new FileStream(Path.Combine(path, "terrain.bin"), FileMode.Open)))
                    {
                        int amt = reader.ReadInt32();
                        for (int i = 0; i < amt; ++i)
                        {
                            float xOffset = reader.ReadSingle();
                            float yOffset = reader.ReadSingle();
                            int lenVerts = reader.ReadInt32();
                            byte[] arrVerts = reader.ReadBytes(lenVerts * sizeof(float));
                            int lenTris = reader.ReadInt32();
                            byte[] arrTris = reader.ReadBytes(lenTris * sizeof(int));

                            int[] indcs = new int[lenTris];
                            float[] verts = new float[lenVerts];
                            Buffer.BlockCopy(arrVerts, 0, verts, 0, arrVerts.Length);
                            Buffer.BlockCopy(arrTris, 0, indcs, 0, arrTris.Length);
                            TriangleIndexVertexArray indexVertexArray = new TriangleIndexVertexArray(indcs, verts);
                            Terrain = new BvhTriangleMeshShape(indexVertexArray, false, true);
                            DefaultMotionState myMotionState = new DefaultMotionState(Matrix.Translation(xOffset, yOffset, 0));
                            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0.0f, myMotionState, Terrain);
                            CollisionObject rigid = new TerrainCB(rbInfo);
                            rigid.UserIndex = 1;
                            rigid.CollisionFlags |= CollisionFlags.StaticObject;
                            rigid.CollisionFlags |= CollisionFlags.DisableVisualizeObject;
                            //AddCollisionObject(rigid, CollisionFilterGroups.StaticFilter, CollisionFilterGroups.None);
                            AddCollisionObject(rigid);
                            rigid.ForceActivationState(ActivationState.DisableSimulation);
                            rigid.WorldTransform = Matrix.Translation(xOffset, yOffset, 0);
                            rbInfo.Dispose();
                        }
                    }
                }
                else
                {
                    File.Delete(Path.Combine(path, "terrain.bin"));
                }
            }

            GC.Collect();
            /*
            foreach (int key in FixtureInfos.Keys)
            {
                FixtureInfo info = FixtureInfos[key];
                if (!ModelInfos.ContainsKey(info.NifID))
                {
                    Log.Debug("[OCCLUSION]", $"Missing fixture info: {info.NifID} - {info.Name} in {path}");
                    continue;
                }
                if (!Occlusion.Fixtures.ContainsKey(ModelInfos[info.NifID].ModelName))
                {
                    //  Log.Debug("[OCCLUSION]", $"Missing model: {ModelInfos[info.NifID].ModelName}");
                    continue;
                }
                NifInfo nif = ModelInfos[info.NifID];
                BvhTriangleMeshShape shape = Occlusion.Fixtures[nif.ModelName];

                Matrix startTransform = Matrix.Translation(new BulletSharp.Math.Vector3( //translation
                        65535.0f - (float)info.X,
                        (float)info.Y,
                        (float)info.Z
                    ));

                Matrix rotMatrix = Matrix.RotationAxis(new BulletSharp.Math.Vector3(-(float)info.XAxis, (float)info.YAxis, (float)info.ZAxis), (float)info.Angle3D);
                float angle2D = Clamp(info.O, nif.MinAngle, nif.MinAngle) / 180.0f * (float)Math.PI;
                Matrix rot2D = Matrix.RotationZ(angle2D);
                rotMatrix = rot2D * rotMatrix * Matrix.RotationZ((float)Math.PI);
                Matrix scaleMatrix = Matrix.Scaling(info.Scale / 100.0f, info.Scale / 100.0f, info.Scale / 100.0f);
                startTransform = scaleMatrix * rotMatrix * startTransform;
                DefaultMotionState myMotionState = new DefaultMotionState(startTransform);
                RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0.0f, myMotionState, shape);
                FixtureCB rigid = new FixtureCB(rbInfo, info.ID, info.NifID, info.Name);
                rigid.CollisionFlags |= CollisionFlags.StaticObject;
                rigid.CollisionFlags |= CollisionFlags.DisableVisualizeObject;
                rigid.UserIndex = 0;
                //AddCollisionObject(rigid, CollisionFilterGroups.DebrisFilter, CollisionFilterGroups.None);
                AddCollisionObject(rigid);
                rigid.WorldTransform = startTransform;
                rigid.ForceActivationState(ActivationState.DisableSimulation);

                rbInfo.Dispose();
            }
            GC.Collect();

            FixtureInfos.Clear();

            foreach (WaterBody body in bodies)
            {
                TriangleIndexVertexArray indexVertexArray = new TriangleIndexVertexArray(body.Polygons, body.Vertices);
                BvhTriangleMeshShape waterMesh = new BvhTriangleMeshShape(indexVertexArray, false, true);

                Matrix matrix = Matrix.Translation(-body.X, body.Y, -body.Z);
                DefaultMotionState myMotionState = new DefaultMotionState(matrix);
                RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0.0f, myMotionState, waterMesh);
                WaterCB rigid = new WaterCB(rbInfo, body.Name, body.Type);
                rigid.UserIndex = 2;
                rigid.CollisionFlags |= CollisionFlags.StaticObject;
                rigid.CollisionFlags |= CollisionFlags.DisableVisualizeObject;
                //AddCollisionObject(rigid, CollisionFilterGroups.DebrisFilter, CollisionFilterGroups.None);
                AddCollisionObject(rigid);
                rigid.WorldTransform = matrix;
                rigid.ForceActivationState(ActivationState.DisableSimulation);
                rbInfo.Dispose();
            }
            GC.Collect();

            bodies.Clear();*/
            UpdateAabbs();
            //ComputeOverlappingPairs();
            ForceUpdateAllAabbs = false;
        }

        public void LoadFixtures(string v)
        {
            if (!File.Exists(v))
            {
                Log.Notice("[OCCLUSION]", $"Fixtures file not found: {v}");
                return;
            }
            using (BinaryReader reader = new BinaryReader(new FileStream(v, FileMode.Open)))
            {
                reader.ReadBytes(16); // header, version, length
                reader.ReadInt32(); // region id
                reader.ReadInt32(); //zone count
                reader.ReadInt32(); //zoneId
                reader.ReadInt32(); // xOff
                reader.ReadInt32(); //yOff
                reader.ReadInt32(); //nif count
                reader.ReadInt32(); //fix count
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    ChunkType type = (ChunkType)reader.ReadInt32();
                    int length = reader.ReadInt32();
                    switch (type)
                    {
                        case ChunkType.Terrain:
                            {
                                break;
                            }
                        case ChunkType.Collision:
                            {
                                ReadCollision(reader);
                                break;
                            }
                        case ChunkType.Water:
                            {
                                ReadWater(reader);
                                break;
                            }
                        case ChunkType.Region:
                            {
                                break;
                            }
                    }
                    reader.BaseStream.Position += length;
                }
            }
        }

        private void ReadWater(BinaryReader reader)
        {
            reader.ReadInt32(); //regionId
            reader.ReadInt32(); //zoneId
            int vertCount = reader.ReadInt32();
            float[] verts = new float[vertCount * 3];
            for (int i = 0; i < vertCount; ++i)
            {
                verts[i * 3] = reader.ReadSingle();
                verts[i * 3 + 1] = reader.ReadSingle();
                verts[i * 3 + 2] = reader.ReadSingle();
            }
            reader.ReadInt32(); //fix count
            int indsCount = reader.ReadInt32(); // indexes count
            reader.ReadInt32(); //stride
            int[] inds = new int[indsCount * 3];
            for (int i = 0; i < indsCount; ++i)
            {
                inds[i * 3] = reader.ReadInt32();
                inds[i * 3 + 1] = reader.ReadInt32();
                inds[i * 3 + 2] = reader.ReadInt32();
                reader.ReadInt32();
            }
            TriangleIndexVertexArray indexVertexArray = new TriangleIndexVertexArray();
            IndexedMesh mesh = new IndexedMesh();
            mesh.Allocate(indsCount, vertCount);
            byte[] arr = new byte[vertCount * 3 * 4];
            using (UnmanagedMemoryStream stream = mesh.GetVertexStream())
            {
                Buffer.BlockCopy(verts, 0, arr, 0, arr.Length);
                stream.Write(arr, 0, arr.Length);
            }
            arr = new byte[indsCount * 3 * 4];
            using (UnmanagedMemoryStream stream = mesh.GetTriangleStream())
            {
                Buffer.BlockCopy(inds, 0, arr, 0, arr.Length);
                stream.Write(arr, 0, arr.Length);
            }
            indexVertexArray.AddIndexedMesh(mesh);
            if (vertCount > 0 && indsCount > 0)
            {
                BvhTriangleMeshShape waterMesh = new BvhTriangleMeshShape(indexVertexArray, false, true);

                Matrix matrix = Matrix.Translation(0, 0, 0);
                DefaultMotionState myMotionState = new DefaultMotionState(matrix);
                RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0.0f, myMotionState, waterMesh);
                WaterCB rigid = new WaterCB(rbInfo, "", "");
                rigid.UserIndex = 2;
                rigid.CollisionFlags |= CollisionFlags.StaticObject;
                rigid.CollisionFlags |= CollisionFlags.DisableVisualizeObject;
                //AddCollisionObject(rigid, CollisionFilterGroups.DebrisFilter, CollisionFilterGroups.None);
                AddCollisionObject(rigid);
                rigid.WorldTransform = matrix;
                rigid.ForceActivationState(ActivationState.DisableSimulation);
                rbInfo.Dispose();
                GC.Collect();
            }
        }

        private void ReadCollision(BinaryReader reader)
        {
            reader.ReadInt32(); //regionId
            reader.ReadInt32(); //zoneId

            int vertCount = reader.ReadInt32();
            float[] verts = new float[vertCount * 3];
            for (int i = 0; i < vertCount; ++i)
            {
                verts[i * 3] = reader.ReadSingle();
                verts[i * 3 + 1] = reader.ReadSingle();
                verts[i * 3 + 2] = reader.ReadSingle();
            }
            reader.ReadInt32(); //fix count
            int indsCount = reader.ReadInt32(); // indexes count
            reader.ReadInt32(); //stride
            int[] inds = new int[indsCount * 3];
            for (int i = 0; i < indsCount; ++i)
            {
                inds[i * 3] = reader.ReadInt32();
                inds[i * 3 + 1] = reader.ReadInt32();
                inds[i * 3 + 2] = reader.ReadInt32();
                reader.ReadInt32();
            }
            Matrix startTransform = Matrix.Translation(0, 0, 0);
            TriangleIndexVertexArray indexVertexArray = new TriangleIndexVertexArray();
            IndexedMesh mesh = new IndexedMesh();
            mesh.Allocate(indsCount, vertCount);
            byte[] arr = new byte[vertCount * 3 * 4];
            using (UnmanagedMemoryStream stream = mesh.GetVertexStream())
            {
                Buffer.BlockCopy(verts, 0, arr, 0, arr.Length);
                stream.Write(arr, 0, arr.Length);
            }
            arr = new byte[indsCount * 3 * 4];
            using (UnmanagedMemoryStream stream = mesh.GetTriangleStream())
            {
                Buffer.BlockCopy(inds, 0, arr, 0, arr.Length);
                stream.Write(arr, 0, arr.Length);
            }
            indexVertexArray.AddIndexedMesh(mesh);
            if (vertCount > 0 && indsCount > 0)
            {
                BvhTriangleMeshShape shape = new BvhTriangleMeshShape(indexVertexArray, false, true);
                DefaultMotionState myMotionState = new DefaultMotionState(startTransform);
                RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0.0f, myMotionState, shape);
                FixtureCB rigid = new FixtureCB(rbInfo, 0, 0, "");
                rigid.CollisionFlags |= CollisionFlags.StaticObject;
                rigid.CollisionFlags |= CollisionFlags.DisableVisualizeObject;
                rigid.UserIndex = 0;
                //AddCollisionObject(rigid, CollisionFilterGroups.DebrisFilter, CollisionFilterGroups.None);
                AddCollisionObject(rigid);
                rigid.WorldTransform = startTransform;
                rigid.ForceActivationState(ActivationState.DisableSimulation);

                rbInfo.Dispose();
            }
            GC.Collect();
        }

        private float Clamp(int value, float low, float high)
        {
            if (high < low)
            {
                float temp = high;
                high = low;// throw gcnew ArgumentException();
                low = temp;
            }
            if (value < low)
                return low;
            if (value > high)
                return high;
            return value;
        }

        public static PhysicsZone Build(BulletOcclusion occl)
        {
            DefaultCollisionConfiguration collisionConf = new DefaultCollisionConfiguration();
            SequentialImpulseConstraintSolver sol = new SequentialImpulseConstraintSolver();
            CollisionDispatcher dispatcher = new CollisionDispatcher(collisionConf);
            BroadphaseInterface broadphase = new DbvtBroadphase();
            PhysicsZone zone = new PhysicsZone(collisionConf, dispatcher, sol, broadphase);
            zone.Occlusion = occl;
            zone.ForceUpdateAllAabbs = false;
            zone.LatencyMotionStateInterpolation = false;
            return zone;
        }
    }

    public enum ChunkType
    {
        Undefined,
        Zone,
        NIF,
        Fixture,
        Terrain,
        Collision,
        BSP,
        Region,
        Water,
        Count,
    }
}
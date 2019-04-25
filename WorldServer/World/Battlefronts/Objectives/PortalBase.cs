using System;
using Common;
using Common.Database.World.BattleFront;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Battlefronts.Objectives
{
    /// <summary>
    /// Game object representing a portal around an objective
    /// allowing port to warcamp.
    /// </summary>
    class PortalBase : GameObject
    {
        private const uint PORTAL_PROTO_ENTRY = 242;
        private const uint PORTAL_DISPLAY_ID = 1583;

        private Random random = new Random();

        internal PortalBase(BattleFrontObject origin)
        {
            Spawn = CreateSpawn(origin);
        }

        internal PortalBase(int zoneId, int x, int y, int z, int o)
        {
            Spawn = CreateSpawn(zoneId, x, y, z, o);
        }

        internal PortalBase(GameObject_spawn spawn)
        {
            Spawn = spawn;
        }

        /// <summary>
        /// Creates a game object spawn entity from given Campaign portal object.
        /// </summary>
        /// <param name="battleFrontObject">Portal object providing raw data</param>
        /// <returns>newly created spawn entity</returns>
        private GameObject_spawn CreateSpawn(BattleFrontObject battleFrontObject)
        {
            GameObject_proto proto = GameObjectService.GetGameObjectProto(PORTAL_PROTO_ENTRY);
            proto = (GameObject_proto)proto.Clone();

            GameObject_spawn spawn = new GameObject_spawn();
            spawn.BuildFromProto(proto);

            // boule blanche : 3457
            // grosse boule blanche : 1675
            proto.Scale = 25;
            // spawn.DisplayID = 1675;
            spawn.DisplayID = 1675;
            spawn.ZoneId = battleFrontObject.ZoneId;

            Point3D worldPos = GetWorldPosition(battleFrontObject);
            spawn.WorldX = worldPos.X;
            spawn.WorldY = worldPos.Y;
            spawn.WorldZ = worldPos.Z;
            spawn.WorldO = battleFrontObject.O;

            return spawn;
        }

        /// <summary>
        /// Creates a game object spawn entity from given Campaign portal object.
        /// </summary>
        /// <param name="battleFrontObject">Portal object providing raw data</param>
        /// <returns>newly created spawn entity</returns>
        private GameObject_spawn CreateSpawn(int zoneId, int x, int y, int z, int o)
        {
            GameObject_proto proto = GameObjectService.GetGameObjectProto(PORTAL_PROTO_ENTRY);
            proto = (GameObject_proto)proto.Clone();

            GameObject_spawn spawn = new GameObject_spawn();
            spawn.BuildFromProto(proto);

            // boule blanche : 3457
            // grosse boule blanche : 1675
            proto.Scale = 25;
            // spawn.DisplayID = 1675;
            spawn.DisplayID = 1675;
            spawn.ZoneId = (ushort) zoneId;

            spawn.WorldX = x;
            spawn.WorldY = y;
            spawn.WorldZ = z;
            spawn.WorldO = o;

            spawn.IsValid = true;
            spawn.IsDeleted = false;
            spawn.Guid = 132456;
    
            return spawn;
        }

        protected Point3D GetWorldPosition(BattleFrontObject bObject)
        {
            Zone_Info zone = ZoneService.GetZone_Info(bObject.ZoneId);
            return ZoneService.GetWorldPosition(zone, (ushort)bObject.X, (ushort)bObject.Y, (ushort)bObject.Z);
        }

        protected void Teleport(Player player, int zoneId, Point3D targetPos)
        {
            Point2D randomPoint = targetPos.GetPointFromHeading((ushort)random.Next(0, 4096), 5);
            player.Teleport((ushort) zoneId, (uint)randomPoint.X, (uint)randomPoint.Y, (ushort)targetPos.Z, 0);
        }

    }
}

using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using SystemData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.Managers.Commands
{
    /// <summary>Game object commands under .go</summary>
    internal class GoCommands
    {

        [CommandAttribute(EGmLevel.DatabaseDev, "Spawn an Go")]
        public static void Spawn(Player plr, uint entry)
        {
            GameObject_proto proto = GameObjectService.GetGameObjectProto(entry);
            if (proto == null)
            {
                plr.SendClientMessage($"Invalid go entry({entry})", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return;
            }

            plr.UpdateWorldPosition();

            GameObject_spawn spawn = new GameObject_spawn();
            spawn.Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID();
            spawn.BuildFromProto(proto);
            spawn.WorldO = plr._Value.WorldO;
            spawn.WorldY = plr._Value.WorldY;
            spawn.WorldZ = plr._Value.WorldZ;
            spawn.WorldX = plr._Value.WorldX;
            spawn.ZoneId = plr.Zone.ZoneId;

            WorldMgr.Database.AddObject(spawn);

            plr.Region.CreateGameObject(spawn);
        }

        [CommandAttribute(EGmLevel.SourceDev, "Spawns a GameObject with the given proto entry at the local coordinates specified.")]
        public static void Local(Player plr, uint entry, int zoneX, int zoneY, int z, int heading)
        {
            if (plr.Zone == null)
            {
                plr.SendClientMessage("GAMEOBJECT LOCAL: Must be in a zone to use this command.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return;
            }

            int worldX = (plr.Zone.Info.OffX << 12) + (zoneX - 8192);
            int worldY = (plr.Zone.Info.OffY << 12) + (zoneY - 8192);

            GameObject_proto proto = GameObjectService.GetGameObjectProto(entry);
            if (proto == null)
            {
                plr.SendClientMessage($"GAMEOBJECT LOCAL: Unable to find a game object prototype of the specified entry {entry}", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return;
            }

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldX = worldX,
                WorldY = worldY,
                WorldZ = z,
                WorldO = heading,
                ZoneId = plr.Zone.ZoneId
            };

            spawn.BuildFromProto(proto);

            WorldMgr.Database.AddObject(spawn);

            plr.Region.CreateGameObject(spawn);

            plr.Teleport(plr.Zone.ZoneId, (uint)worldX, (uint)worldY, (ushort)z, 0);
        }

        [CommandAttribute(EGmLevel.DatabaseDev, "Delete the target (0=World,1=Database)")]
        public static void Remove(Player plr, bool database)
        {
            Object obj = GetObjectTarget(plr);
            if (!obj.IsGameObject())
            {
                plr.SendClientMessage($"GAMEOBJECT REMOVE: Target is not a gameobject", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                return;
            }

            obj.RemoveFromWorld();

            if (database)
                WorldMgr.Database.DeleteObject(obj.GetGameObject().Spawn);
        }

#if !DEBUG
        [CommandAttribute(EGmLevel.SourceDev, "Set the health of GO to value percent - 100 is 100%, 50 is 50%")]
#else
        [CommandAttribute(EGmLevel.DatabaseDev, "Set the health of GO to value percent - 100 is 100%, 50 is 50%")]
#endif
        public static bool Health(Player plr, int healthPercent)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null)
                return false;

            GameObject go = target.GetGameObject();
            if (go != null)
            {
                go.Health = Convert.ToUInt16(go.TotalHealth * healthPercent / 100);
            }

            return true;
        }
    }
}

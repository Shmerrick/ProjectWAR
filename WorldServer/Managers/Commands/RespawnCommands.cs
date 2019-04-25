using Common;
using System;
using System.Collections.Generic;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Respawn modification commands under .respawn</summary>
    internal class RespawnCommands
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool RespawnAdd(Player plr, ref List<string> values)
        {
            byte realm = (byte)GetInt(ref values);
            Zone_Respawn respawn = new Zone_Respawn
            {
                PinX = (ushort)plr.X,
                PinY = (ushort)plr.Y,
                PinZ = (ushort)plr.Z,
                WorldO = plr.Heading,
                ZoneID = plr.Zone.ZoneId,
                Realm = realm
            };
            WorldMgr.Database.AddObject(respawn);
            ZoneService.LoadZone_Respawn();

            GameObject_proto proto = GameObjectService.GetGameObjectProto(563);

            GameObject_spawn spawn = new GameObject_spawn
            {
                Guid = (uint)GameObjectService.GenerateGameObjectSpawnGUID(),
                WorldX = plr.WorldPosition.X,
                WorldY = plr.WorldPosition.Y,
                WorldZ = plr.WorldPosition.Z,
                WorldO = plr.Heading,
                ZoneId = plr.Zone.ZoneId
            };

            spawn.BuildFromProto(proto);
            plr.Region.CreateGameObject(spawn);

            GMCommandLog log = new GMCommandLog
            {
                PlayerName = plr.Name,
                AccountId = (uint)plr.Client._Account.AccountId,
                Command = "ADD RESPAWN TO " + plr.Zone.ZoneId + " " + (ushort)plr.X + " " + (ushort)plr.Y,
                Date = DateTime.Now
            };
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool RespawnModify(Player plr, ref List<string> values)
        {
            int id = GetInt(ref values);

            Zone_Respawn respawn = WorldMgr.Database.SelectObject<Zone_Respawn>("RespawnID=" + id);
            if (respawn == null)
                return false;

            respawn.PinX = (ushort)plr.X;
            respawn.PinY = (ushort)plr.Y;
            respawn.PinZ = (ushort)plr.Z;
            respawn.WorldO = plr.Heading;
            respawn.ZoneID = plr.Zone.ZoneId;
            respawn.Realm = (byte)plr.Realm;
            WorldMgr.Database.SaveObject(respawn);
            ZoneService.LoadZone_Respawn();

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool RespawnRemove(Player plr, ref List<string> values)
        {
            int id = GetInt(ref values);

            Zone_Respawn respawn = WorldMgr.Database.SelectObject<Zone_Respawn>("RespawnID=" + id);
            if (respawn != null)
            {
                WorldMgr.Database.DeleteObject(respawn);
                ZoneService.LoadZone_Respawn();
            }
            else
                return false;

            return true;
        }
    }
}

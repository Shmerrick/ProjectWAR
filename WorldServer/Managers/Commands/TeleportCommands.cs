using Common;
using System;
using System.Collections.Generic;
using SystemData;
using System.Linq;
using static WorldServer.Managers.Commands.GMUtils;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.Managers.Commands
{
    /// <summary>Contains the list of teleportation commands under .teleport</summary>
    internal class TeleportCommands
    {

        /// <summary>
        /// Teleports you to the specified world coordinates in a given zone (byte ZoneID , uint WorldX, uint WorldY, uint WorldZ)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool TeleportMap(Player plr, ref List<string> values)
        {
            int zoneID = GetZoneId(plr, ref values);
            if (zoneID == -1)
                return false;
            int worldX = GetInt(ref values);
            int worldY = GetInt(ref values);
            int worldZ = GetInt(ref values);

            plr.Teleport((ushort)zoneID, (uint)worldX, (uint)worldY, (ushort)worldZ, 0);

            GMCommandLog log = new GMCommandLog();
            log.PlayerName = plr.Name;
            log.AccountId = (uint)plr.Client._Account.AccountId;
            log.Command = "TELEPORT TO " + zoneID + " " + worldX + " " + worldY;
            log.Date = DateTime.Now;
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Teleport to the centre of the given map.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool TeleportCenter(Player plr, ref List<string> values)
        {
            int zoneID = GetZoneId(plr, ref values);
            if (zoneID == -1)
                return false;

            Point3D newPos = ZoneService.GetWorldPosition(ZoneService.GetZone_Info((ushort)zoneID), 33000, 33000, 2200);

            plr.Teleport((ushort)zoneID, (uint)newPos.X, (uint)newPos.Y, (ushort)newPos.Z, 0);

            return true;
        }

        /// <summary>
        /// Teleport to an objective.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name. One value - respawnId</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool TeleportObjective(Player plr, ref List<string> values)
        {
            IList<BattleFront_Objective> BattleFrontObjectives = WorldMgr.Database.SelectAllObjects<BattleFront_Objective>();
            var respawnToTravelTo = GetInt(ref values);

            var BattleFrontObjective = BattleFrontObjectives.SingleOrDefault(x => x.Entry == respawnToTravelTo);
            if (BattleFrontObjective == null)
                return false;

            var zone = ZoneService.GetZone_Info((ushort)BattleFrontObjective.ZoneId);

            // X+50 so you dont get stuck on flags on the objective
            plr.Teleport((ushort)BattleFrontObjective.ZoneId, (uint)BattleFrontObjective.X+50, (uint)BattleFrontObjective.Y, (ushort)BattleFrontObjective.Z, 0);

            return true;
        }

        /// <summary>
        /// Teleports you to a player's location (string playerName)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool TeleportAppear(Player plr, ref List<string> values)
        {
            string playerName = GetString(ref values);

            Player target = Player.GetPlayer(playerName);

            if (target == null)
            {
                plr.SendClientMessage($"TELEPORT APPEAR: {playerName} could not be found.");
                return true;
            }

            if (target.Zone == null)
                return false;

            plr.Teleport(target.Region, target.Zone.ZoneId, (uint)target.WorldPosition.X, (uint)target.WorldPosition.Y, (ushort)target.WorldPosition.Z, target.Heading);

            GMCommandLog log = new GMCommandLog
            {
                PlayerName = plr.Name,
                AccountId = (uint)plr.Client._Account.AccountId,
                Command = $"TELEPORTED TO PLAYER {target.Name} AT ZONE {target.Zone.ZoneId} LOCATION {target._Value.WorldX} {target._Value.WorldY}",
                Date = DateTime.Now
            };
            CharMgr.Database.AddObject(log);

            return true;
        }

        /// <summary>
        /// Summons a player/group to your location (string playerName optional GROUP)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool TeleportSummon(Player plr, ref List<string> values)
        {
            string playerName = GetString(ref values);
            bool group = false;

            if (values.Count > 1)
                group = GetString(ref values).ToUpper().Trim() == "GROUP";

            Player t = Player.GetPlayer(playerName);

            if (t == null)
            {
                plr.SendClientMessage("TELEPORT SUMMON: " + playerName + " could not be found.");
                return true;
            }

            var list = new List<Player>() { t };
            if (t.WorldGroup != null && group)
                list.AddRange(t.WorldGroup.GetPlayerListCopy().Where(e => e.Info.CharacterId != t.Info.CharacterId).ToList());

            foreach (var target in list)
            {
                target.Teleport(plr.Region, plr.Zone.ZoneId, (uint)plr.WorldPosition.X, (uint)plr.WorldPosition.Y, (ushort)plr.WorldPosition.Z, 0);
                target.IsSummoned = true;

                //allow summoned player to enter illegal area (if summoned to it), unset it after 30 seconds
                target.EvtInterface.AddEvent((player) =>
                {
                    ((Player)player).IsSummoned = false;
                }, 30000, 1, target);

                GMCommandLog log = new GMCommandLog();
                log.PlayerName = plr.Name;
                log.AccountId = (uint)plr.Client._Account.AccountId;
                log.Command = "SUMMON PLAYER " + target.Name + " TO " + plr.Zone.ZoneId + " " + plr._Value.WorldX + " " + plr._Value.WorldY;
                log.Date = DateTime.UtcNow;
                CharMgr.Database.AddObject(log);

                target.SendLocalizeString(plr.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BEEN_SUMMONED_TO_X);
            }

            return true;
        }

        /// <summary>
        /// Sets offline/online players coordinates in database (player_name byte byte ZoneID , uint WorldX, uint WorldY, uint WorldZ)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool TeleportSet(Player plr, ref List<string> values)
        {
            string playerName = GetString(ref values);
            int zoneID = GetZoneId(plr, ref values);
            if (zoneID == -1)
                return false;
            int worldX = GetInt(ref values);
            int worldY = GetInt(ref values);
            int worldZ = GetInt(ref values);

            Zone_Info zone = ZoneService.GetZone_Info((ushort)zoneID);
            if (zone == null)
                zone = ZoneService._Zone_Info[0];

            var existingChar = CharMgr.GetCharacter(Player.AsCharacterName(playerName), false);
            if (existingChar == null)
            {
                plr.SendClientMessage("Player with name '" + values[0] + "' not found.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return true;
            }

            var player = Player.GetPlayer(playerName);

            GMCommandLog log = new GMCommandLog
            {
                PlayerName = plr.Name,
                AccountId = (uint)plr.Client._Account.AccountId,
                Command = "TELEPORT offline player '" + existingChar.Name + "' TO " + zoneID + " " + worldX + " " + worldY,
                Date = DateTime.Now
            };
            CharMgr.Database.AddObject(log);

            if (player != null)
                player.Teleport((ushort)zoneID, (uint)worldX, (uint)worldY, (ushort)worldZ, 0);

            existingChar.Value.WorldX = worldX;
            existingChar.Value.WorldY = worldY;
            existingChar.Value.WorldZ = worldZ;
            existingChar.Value.ZoneId = zone.ZoneId;
            existingChar.Value.RegionId = zone.Region;

            CharMgr.Database.SaveObject(existingChar.Value);
            CharMgr.Database.ForceSave();

            return true;
        }
    }
}

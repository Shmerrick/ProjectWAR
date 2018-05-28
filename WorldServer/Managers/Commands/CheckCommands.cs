using System;
using System.Collections.Generic;
using static WorldServer.Managers.Commands.GMUtils;
using System.Text;
using WorldServer.Services.World;

namespace WorldServer.Managers.Commands
{
    /// <summary>Debugging commands under .check</summary>
    internal class CheckCommands
    {

        /// <summary>
        /// Check how many groups exist on the server.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckGroups(Player plr, ref List<string> values)
        {
            plr.SendClientMessage(Group.WorldGroups.Count + " groups on the server:");

            lock (Group.WorldGroups)
            {
                foreach (Group group in Group.WorldGroups)
                {
                    Player ldr = group.Leader;

                    if (ldr == null)
                        plr.SendClientMessage("Leaderless group");
                    else plr.SendClientMessage("Group led by " + ldr.Name);
                }
            }

            return true;
        }

        /// <summary>
        /// Check how many objects exist in the current region.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckObjects(Player plr, ref List<string> values)
        {
            plr.Region?.CountObjects(plr);

            return true;
        }

        public static bool GetServerPopulation(Player plr, ref List<string> values)
        {
            plr.SendClientMessage($"Server Population " +
                                  $"Online players : {Program.Rm.OnlinePlayers} " +
                                  $"Order : {Program.Rm.OrderCount} " +
                                  $"Destro : {Program.Rm.DestructionCount}");

            var message = String.Empty;

            foreach (var regionMgr in WorldMgr._Regions)
            {
                message += $"{regionMgr.RegionName} " +
                           $"Total : {regionMgr.Players.Count} " +
                           $"Order : {regionMgr.OrderPlayers} " +
                           $"Dest : {regionMgr.DestPlayers} \n";
            }
            plr.SendClientMessage(message);


            return true;
        }

        /// <summary>
        /// Finds all players currently in range.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool CheckPlayersInRange(Player plr, ref List<string> values)
        {
            StringBuilder str = new StringBuilder(256);
            int curOnLine = 0;

            lock (plr.PlayersInRange)
            {
                foreach (Player player in plr.PlayersInRange)
                {
                    if (curOnLine != 0)
                        str.Append(", ");
                    str.Append(player.Name);
                    str.Append(" (");
                    str.Append(player.Zone.Info.Name);
                    str.Append(")");

                    ++curOnLine;

                    if (curOnLine == 5)
                    {
                        plr.SendClientMessage(str.ToString());
                        curOnLine = 0;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Find the closest respawn point for the specified realm.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool FindClosestRespawn(Player plr, ref List<string> values)
        {
            byte realm = (byte)GetInt(ref values);

            plr.SendClientMessage("Closest respawn for " + (realm == 1 ? "Order" : "Destruction") + " is " +
                             WorldMgr.GetZoneRespawn(plr.Zone.ZoneId, realm, plr).RespawnID);

            return true;
        }

        /// <summary>
        /// Toggles logging outgoing packet volume.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool LogPackets(Player plr, ref List<string> values)
        {
            if (plr.Region == null)
                return false;

            plr.Region.TogglePacketLogging();

            plr.SendClientMessage(plr.Region.LogPacketVolume ? "Logging outgoing packet volume." : "No longer logging outgoing packet volume.");

            return true;
        }

        /// <summary>
        /// Displays the volume of outgoing packets over the defined period.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ReadPackets(Player plr, ref List<string> values)
        {
            plr.Region.SendPacketVolumeInfo(plr);

            return true;
        }

        /// <summary>
        /// Starts/Stops line of sight monitoring for selected target.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool StartStopLosMonitor(Player plr, ref List<string> values)
        {
            var target = plr.CbtInterface.GetCurrentTarget();

            if (target != null)
            {
                if (WarZoneLib.RegionData._zones[plr.Zone.ZoneId] != null && WarZoneLib.RegionData._zones[plr.Zone.ZoneId].ID != 0)
                {
                    plr.SendClientMessage("Zone " + plr.Zone.ZoneId + " Occlusion Info: Fixtures=" + WarZoneLib.RegionData._zones[plr.Zone.ZoneId].Collision.Fixtures.Count + " BSPTriangles="
                        + WarZoneLib.RegionData._zones[plr.Zone.ZoneId].Collision.BSP.Triangles);
                }
                else
                    plr.SendClientMessage("Zone " + plr.Zone.ZoneId + " Occlusion Info: NOT LOADED");

                plr.EvtInterface.AddEvent(() =>
                {
                    if (plr.LOSHit(target))
                        plr.SendClientMessage("LOS=YES " + DateTime.Now.Second);
                    else
                        plr.SendClientMessage("LOS=NO" + DateTime.Now.Second);
                }, 1000, 30);

            }
            return true;
        }
    }
}

using Common;
using FrameWork;
using System.Collections.Generic;
using SystemData;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Waypoint commands under .warpoint</summary>
    internal class WaypointCommands
    {

        /// <summary>
        /// Adds a waypoint on your current position to your current target.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcAddWaypoint(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || !target.IsCreature())
                return false;

            Waypoint wp = new Waypoint
            {
                X = (uint)plr.WorldPosition.X,
                Y = (uint)plr.WorldPosition.Y,
                Z = (uint)plr.WorldPosition.Z,
                WaitAtEndMS = 2000
            };

            target.GetUnit().AiInterface.AddWaypoint(wp);

            List<string> empty = null;
            return NpcListWaypoint(plr, ref empty);
        }

        /// <summary>
        /// Remove a waypoint from the target (int Id)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcRemoveWaypoint(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || !target.IsCreature())
                return false;

            int id = GetInt(ref values);
            AIInterface ia = target.GetCreature().AiInterface;

            Waypoint wp = ia.GetWaypoint(id);
            if (wp == null)
            {
                plr.SendMessage(0, "Server", "Invalid Waypoint ID. Use .waypoint list", ChatLogFilters.CHATLOGFILTERS_SHOUT);
                return true;
            }

            ia.RemoveWaypoint(wp);

            List<string> empty = null;
            return NpcListWaypoint(plr, ref empty);
        }

        /// <summary>
        /// Moves the specified waypoint of target to your position (int Id)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcMoveWaypoint(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || !target.IsCreature())
                return false;

            int id = GetInt(ref values);
            AIInterface ia = target.GetCreature().AiInterface;

            Waypoint wp = ia.GetWaypoint(id);
            if (wp == null)
            {
                plr.SendMessage(0, "Server", "Invalid Waypoint ID. Use .waypoint list", ChatLogFilters.CHATLOGFILTERS_SHOUT);
                return true;
            }
            wp.X = (ushort)plr.X;
            wp.Y = (ushort)plr.Y;
            wp.Z = (ushort)plr.Z;

            ia.SaveWaypoint(wp);

            List<string> empty = null;
            return NpcListWaypoint(plr, ref empty);
        }

        /// <summary>
        /// Shows the list of waypoints.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcListWaypoint(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || !target.IsCreature())
                return false;

            AIInterface ia = target.GetCreature().AiInterface;
            string message = "Waypoints :" + ia.Waypoints.Count + "\n";

            foreach (Waypoint wp in ia.Waypoints)
            {
                message += wp + "\n";
            }

            plr.SendMessage(0, "Server", message, ChatLogFilters.CHATLOGFILTERS_SHOUT);

            return true;
        }

        /// <summary>
        /// Shows information about the current waypoint.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool NpcInfoWaypoint(Player plr, ref List<string> values)
        {
            Object target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || !target.IsCreature())
                return false;

            AIInterface ia = target.GetCreature().AiInterface;
            string message = "";
            message += "Current = " + ia.CurrentWaypointID + ",NextTime=" + (ia.NextAllowedMovementTime - TCPManager.GetTimeStampMS()) + ",Started=" + ia.Started + ",Ended=" + ia.Ended + ",Back=" + ia.IsWalkingBack + ",Type=" + ia.CurrentWaypointType + ",State=" + ia.State;

            plr.SendMessage(0, "Server", message, ChatLogFilters.CHATLOGFILTERS_SHOUT);

            return true;
        }
    }
}

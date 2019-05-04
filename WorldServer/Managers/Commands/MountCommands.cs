using Common;
using System.Collections.Generic;
using SystemData;
using WorldServer.World.Objects;
using static WorldServer.Managers.Commands.GMUtils;

namespace WorldServer.Managers.Commands
{
    /// <summary>Mount commands under .mount</summary>
    internal class MountCommands
    {

        /// <summary>
        /// Changes the mount of the selected unit (int Entry)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool SetMountCommand(Player plr, ref List<string> values)
        {
            Unit target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || target.IsDead)
                return false;

            int entry = GetInt(ref values);

            Mount_Info info = new Mount_Info();

            info.Entry = (uint)entry;
            target.Mount((ushort)info.Entry);
            plr.SendClientMessage("Target mount : " + info.Name);

            return true;
        }

        /// <summary>
        /// Adds a new mount to the database (int Entry, int Speed, string Name)
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool AddMountCommand(Player plr, ref List<string> values)
        {
            Unit target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || target.IsDead)
                return false;

            int entry = GetInt(ref values);
            int speed = GetInt(ref values);
            string name = GetString(ref values);

            Mount_Info info = WorldMgr.Database.SelectObject<Mount_Info>("Entry=" + entry);

            if (info == null)
            {
                info = new Mount_Info();
                info.Entry = (uint)entry;
                info.Speed = (ushort)speed;
                info.Name = name;
                WorldMgr.Database.AddObject(info);

                target.Mount((ushort)info.Entry);
                plr.SendMessage(null, "Added mount to Database. Mount Name : " + info.Name, ChatLogFilters.CHATLOGFILTERS_SHOUT);
            }
            else
            {
                info.Entry = (uint)entry;
                info.Speed = (ushort)speed;
                info.Name = name;
                info.Dirty = true;
                WorldMgr.Database.AddObject(info);

                plr.SendMessage(null, "Modified mount " + info.Name, ChatLogFilters.CHATLOGFILTERS_SHOUT);
            }


            return true;
        }

        /// <summary>
        /// Removes the mount of the selected unit.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool RemoveMountCommand(Player plr, ref List<string> values)
        {
            Unit target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || target.IsDead)
                return false;

            target.Dismount();
            plr.SendMessage(null, "Target UnMount", ChatLogFilters.CHATLOGFILTERS_SHOUT);
            return true;
        }

        /// <summary>
        /// Shows the list of all mounts.
        /// </summary>
        /// <param name="plr">Player that initiated the command</param>
        /// <param name="values">List of command arguments (after command name)</param>
        /// <returns>True if command was correctly handled, false if operation was canceled</returns>
        public static bool ListMountsCommand(Player plr, ref List<string> values)
        {
            Unit target = plr.CbtInterface.GetCurrentTarget();
            if (target == null || target.IsDead)
                return false;

            List<Mount_Info> mounts = WorldMgr.Database.SelectAllObjects<Mount_Info>() as List<Mount_Info>;

            uint i = 1;
            foreach (Mount_Info info in mounts)
            {
                if (info.Id != i)
                {
                    info.Id = i;
                    info.Dirty = true;
                    WorldMgr.Database.SaveObject(info);
                }

                ++i;
                plr.SendMessage(null, info.Id + ": " + info.Speed + ":" + info.Name, ChatLogFilters.CHATLOGFILTERS_SHOUT);
            }
            return true;
        }

    }
}

using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WorldServer.World.Objects;
using WorldServer.World.Objects.PublicQuests;
using static WorldServer.Managers.Commands.GMUtils;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.Managers.Commands
{
    /// <summary>Event commands under .event
    /// Currently just a sketch, still working on it
    /// </summary>
    internal class EventCommands
    {
        // This allow to enable or disable event, .event enable 1 
        // to enable or .event enable 0 to disable
        public static bool EventEnable(Player plr, ref List<string> values)
        {
            int enabled = GetInt(ref values);
            Object obj = GetObjectTarget(plr);

            return false;
        }

        public static bool EventConvert(Player plr, ref List<string> values)
        {

            return false;
        }
    }
}
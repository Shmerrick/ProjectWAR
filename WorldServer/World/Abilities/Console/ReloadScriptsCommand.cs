using System.Collections.Generic;
using FrameWork;
using WorldServer.Managers;

namespace WorldServer.World.Abilities.Console
{
    [ConsoleHandler("reloadscripts", 0, "Reload Scripts")]
    public class CreateAccount : IConsoleHandler
    {
        public bool HandleCommand(string command, List<string> args)
        {
            WorldMgr.LoadScripts(true);
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using Common;
using FrameWork;

namespace AccountCacher
{
    [ConsoleHandler("reloadscripts", 0, "Reload Scripts")]
    public class CreateAccount : IConsoleHandler
    {
        public bool HandleCommand(string command, List<string> args)
        {
            WorldServer.WorldMgr.LoadScripts(true);
            return true;
        }
    }
}

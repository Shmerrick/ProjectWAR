using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using Common;
using FrameWork;

namespace AccountCacher
{
    [ConsoleHandler("create", 2, "New Account <Username,Password,GMLevel(0-31)>")]
    public class CreateAccount : IConsoleHandler
    {
        private string[] _bannedNames = {"zyklon", "fuck", "hitler", "nigger", "nigga", "faggot", "jihad", "muhajid"};

        public bool HandleCommand(string command, List<string> args)
        {
            string Username = args[0];
            string Password = args[1];
            int GmLevel = int.Parse(args[2]);

           return Program.AcctMgr.CreateAccount(Username, Password, GmLevel);
        }
    }
}

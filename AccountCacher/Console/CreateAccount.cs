using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using Common;
using FrameWork;

namespace AccountCacher {
    [ConsoleHandler("create", 2, "New Account <Username,Password,GMLevel(0-31)>")]
    public class CreateAccount : IConsoleHandler {
        private string[] _bannedNames = { "zyklon", "fuck", "hitler", "nigger", "nigga", "faggot", "jihad", "muhajid" };

        public bool HandleCommand(string command, List<string> args) {
            string Username = args[0];
            string Password = args[1];
            int GmLevel = int.Parse(args[2]);

            return Program.AcctMgr.CreateAccount(Username, Password, GmLevel);
        }
    }

    [ConsoleHandler("reset", 2, "Reset Password <Username,Password>")]
    public class ResetPassword : IConsoleHandler
    {
        public bool HandleCommand(string command, List<string> args)
        {
            string userName = args[0];
            string password = args[1];

            var account = Program.AcctMgr.LoadAccount(userName);
            if (account == null)
            {
                Log.Error("ResetPassword", $"Could not locate {userName} to reset password");
                return false;
            }
            else
            {
                account.Password = password;
                account.CryptPassword = Account.ConvertSHA256(userName + ":" + password);
                AccountMgr.Database.SaveObject(account);
                AccountMgr.Database.ForceSave();
                Log.Success("ResetPassword", $"Password reset for {userName} to {password}");
            }
           

            return true;

        }
    }
}

using Common;
using FrameWork;
using System.Collections.Generic;

namespace AccountCacher
{
    [ConsoleHandler("create", 2, "New Account <Username,Password,GMLevel(0-40)>")]
    public class CreateAccount : IConsoleHandler
    {
        private string[] _bannedNames = { "zyklon", "fuck", "hitler", "nigger", "nigga", "faggot", "jihad", "muhajid" };

        public bool HandleCommand(string command, List<string> args)
        {
            string Username = args[0];
            string Password = args[1];
            string Mail = args[2];
            int GmLevel = int.Parse(args[3]);

            return Core.AcctMgr.CreateAccount(Username, Password, Mail, GmLevel, 0);
        }
    }

    [ConsoleHandler("reset", 2, "Reset Password <Username,Password>")]
    public class ResetPassword : IConsoleHandler
    {
        public bool HandleCommand(string command, List<string> args)
        {
            string userName = args[0];
            string password = args[1];

            var account = Core.AcctMgr.LoadAccount(userName);
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
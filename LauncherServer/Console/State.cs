using System;
using System.Collections.Generic;
using AuthenticationServer.Server;
using FrameWork;

namespace AuthenticationServer.Console
{
    [ConsoleHandler("state", 1, "Server State")]
    public class State : IConsoleHandler
    {
        public bool HandleCommand(string command, List<string> args)
        {
            ServerState State;

            if(!Enum.TryParse(args[0], out State))
            {
                Log.Error("ServerState", "Invalid State");
                return false;
            }

            PatchMgr.SetServerState(State);
            Log.Success("ServerState", "Server state is now " + State);

            return true;
        }
    }
}

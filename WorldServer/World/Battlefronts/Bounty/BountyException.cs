using System;
using NLog;

namespace WorldServer.World.Battlefronts.Bounty
{
    public class BountyException : Exception
    {
        private static readonly Logger BountyLogger = LogManager.GetLogger("BountyLogger");

        public BountyException(string s)
        {
            BountyLogger.Error($"Exception : {s}");
        }
    }
}
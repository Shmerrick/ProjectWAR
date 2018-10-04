using System;
using GameData;

namespace WorldServer.World.Battlefronts.Bounty
{
    /// <summary>
    /// The actual class to be added to the ContributionManagerInstance dictionary
    /// </summary>
    public class PlayerContribution
    {
        public byte ContributionId { get; set; }
        public long Timestamp { get; set; }

        public override string ToString()
        {
            return $"{ContributionId} {(ContributionDefinitions)ContributionId} ({FrameWork.TCPManager.GetTimeStamp() - Timestamp} sec ago)";
        }
    }
}
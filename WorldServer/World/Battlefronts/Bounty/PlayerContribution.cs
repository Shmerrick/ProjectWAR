namespace WorldServer.World.Battlefronts.Bounty
{
    /// <summary>
    /// The actual class to be added to the Contribution dictionary
    /// </summary>
    public class PlayerContribution
    {
        public byte ContributionId { get; set; }
        public long Timestamp { get; set; }
    }
}
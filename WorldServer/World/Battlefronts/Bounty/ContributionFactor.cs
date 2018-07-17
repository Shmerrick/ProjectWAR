namespace WorldServer.World.Battlefronts.Bounty
{
    /// <summary>
    /// Structure to hold the defintion of contributions.
    /// </summary>
    public class ContributionFactor
    {
        public byte ContributionId { get; set; }
        public string ContributionDescription { get; set; }
        public byte ContributionValue { get; set; }
        public byte MaxContributionCount { get; set; }

    }
}
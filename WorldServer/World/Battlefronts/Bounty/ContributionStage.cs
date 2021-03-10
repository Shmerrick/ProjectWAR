namespace WorldServer.World.Battlefronts.Bounty
{
    public class ContributionStage
    {
        public short ContributionStageMax { get; set; }
        public string Description { get; set; }
        public short ContributionStageSum { get; set; }
        public short ContributionStageCount { get; set; }
        public short ContributionStageValue { get; set; }

        public override string ToString()
        {
            return $"{Description} {ContributionStageSum}/{ContributionStageMax * ContributionStageValue}";
        }
    }
}
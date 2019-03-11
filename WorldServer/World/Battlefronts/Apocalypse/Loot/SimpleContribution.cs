namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class SimpleContribution
    {
        public uint Killer { get; set; }
        public uint Victim { get; set; }
        public uint Timestamp { get; set; }
    }

    public class SimpleCampaignObjectiveContribution
    {
        public uint CharacterId { get; set; }
        public uint CampaignObjectiveId { get; set; }
        public uint Timestamp { get; set; }
    }
}

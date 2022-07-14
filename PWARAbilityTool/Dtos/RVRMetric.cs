using System;

namespace PWARAbilityTool
{
    public class RVRMetric
    {
        public int MetricId { get; set; }
        public int Tier { get; set; }
        public int BattlefrontId { get; set; }
        public int OrderVictoryPoints { get; set; }
        public int DestructionVictoryPoints { get; set; }
        public string BattlefrontName { get; set; }
        public int OrderPlayersInLake { get; set; }
        public int DestructionPlayersInLake { get; set; }
        public int Locked { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

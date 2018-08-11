using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Apocalypse
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

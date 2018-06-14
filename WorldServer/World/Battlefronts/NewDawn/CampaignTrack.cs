using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData;

namespace WorldServer.World.Battlefronts.NewDawn
{
    public class CampaignTrack
    {
        // The "race" of this Track
        public Pairing CampaignPairing { get; set; }
        // Stages of the CampaignTrack
        public List<RacialPair> CampaignStages { get; set; }

        public CampaignTrack()
        {
            CampaignStages = new List<RacialPair>();
        }
    }

    
}

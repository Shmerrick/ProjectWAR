using GameData;
using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.NewDawn
{
    public class UpperTierRacialPairManager : RacialPairManager
    {
        public List<CampaignTrack> CampaignTracks { get; set; }


        public UpperTierRacialPairManager()
        {
            var ct = new CampaignTrack
            {
                CampaignPairing = Pairing.PAIRING_EMPIRE_CHAOS,
                CampaignStages = new List<RacialPair>
                {
                    new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T2_EMP_CHAOS, Pairing.PAIRING_EMPIRE_CHAOS, "T2 Empire/Chaos", 2, 14),
                    new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T3_EMP_CHAOS, Pairing.PAIRING_EMPIRE_CHAOS, "T3 Empire/Chaos", 3, 6),
                    new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T4_EMP_CHAOS, Pairing.PAIRING_EMPIRE_CHAOS, "T4 Empire/Chaos", 4, 11)
                }
            };

            //CampaignTracks = new List<CampaignTrack>
            //{
            //    new CampaignTrack{ 
            //        Pairing.PAIRING_EMPIRE_CHAOS,

            //        }
                    
            //};

            RacialPairList = new List<RacialPair>
            {
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T2_EMP_CHAOS, Pairing.PAIRING_EMPIRE_CHAOS, "T2 Empire/Chaos", 2, 14),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T3_EMP_CHAOS, Pairing.PAIRING_EMPIRE_CHAOS, "T3 Empire/Chaos", 3, 6),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T4_EMP_CHAOS, Pairing.PAIRING_EMPIRE_CHAOS, "T4 Empire/Chaos", 4, 11),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T2_EMP_CHAOS, Pairing.PAIRING_ELVES_DARKELVES, "T2 High Elf/Dark Elf", 2, 15),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T3_EMP_CHAOS, Pairing.PAIRING_ELVES_DARKELVES, "T3 High Elf/Dark Elf", 3, 16),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T4_EMP_CHAOS, Pairing.PAIRING_ELVES_DARKELVES, "T4 High Elf/Dark Elf", 4, 4),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T2_EMP_CHAOS, Pairing.PAIRING_GREENSKIN_DWARVES, "T2 Dwarf/Greenskin", 2, 12),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T3_EMP_CHAOS, Pairing.PAIRING_GREENSKIN_DWARVES, "T3 Dwarf/Greenskin", 3, 10),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T4_EMP_CHAOS, Pairing.PAIRING_GREENSKIN_DWARVES, "T4 Dwarf/Greenskin", 4, 2),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T5_LOTD, Pairing.PAIRING_LAND_OF_THE_DEAD, "T5 Land of the Dead", 5, 9)
            };
        }
    }
}
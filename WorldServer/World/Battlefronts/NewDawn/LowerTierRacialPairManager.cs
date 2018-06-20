using System.Collections.Generic;
using GameData;

namespace WorldServer.World.Battlefronts.NewDawn
{
    public class LowerTierRacialPairManager : RacialPairManager
    {
        public LowerTierRacialPairManager()
        {
            RacialPairList = new List<RacialPair>
            {
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T1_EMP_CHAOS, Pairing.PAIRING_EMPIRE_CHAOS, "T1 Empire/Chaos", 1, 8),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T1_ELF_DARKELF, Pairing.PAIRING_ELVES_DARKELVES, "T1 High Elf/Dark Elf", 1, 3),
                new RacialPair(RacialPairIdentifier.RACIAL_PAIR_T1_DWARF_GREENSKIN, Pairing.PAIRING_GREENSKIN_DWARVES, "T1 Dwarf/Greenskin", 1, 1),
            };
        }
    }
}
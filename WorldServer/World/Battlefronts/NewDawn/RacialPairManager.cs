using System;
using System.Collections.Generic;
using GameData;

namespace WorldServer.World.BattleFronts.NewDawn
{
    public class RacialPairManager
    {
       
        public List<RacialPair> RacialPairList { get; set; }

        public virtual RacialPair GetByRegion(int regionId)
        {
            foreach (var racialPair in RacialPairList)
            {
                if (racialPair.RegionId == regionId)
                    return racialPair;
            }
            return null;
        }

        public virtual RacialPair GetByPair(Pairing pair, int tier)
        {
            foreach (var racialPair in RacialPairList)
            {
                if ((racialPair.Pairing == pair) && (racialPair.Tier == tier))
                    return racialPair;
            }
            return null;
        }
        /// <summary>
        /// Give a race pairing, select a race pairing that is not that one.
        /// </summary>
        /// <param name="activeRacialPairing"></param>
        /// <returns></returns>
        public Pairing GetRandomRace(Pairing activeRacialPairing)
        {
            Random rnd = new Random();
            int selection = rnd.Next(0, 1);

            switch (activeRacialPairing)
            {
                case Pairing.PAIRING_ELVES_DARKELVES:
                    return selection == 0 ? Pairing.PAIRING_GREENSKIN_DWARVES : Pairing.PAIRING_EMPIRE_CHAOS;
                case Pairing.PAIRING_EMPIRE_CHAOS:
                    return selection == 0 ? Pairing.PAIRING_GREENSKIN_DWARVES : Pairing.PAIRING_ELVES_DARKELVES;
                case Pairing.PAIRING_GREENSKIN_DWARVES:
                    return selection == 0 ? Pairing.PAIRING_ELVES_DARKELVES : Pairing.PAIRING_EMPIRE_CHAOS;

            }

            return Pairing.PAIRING_EMPIRE_CHAOS;
        }
    }
}
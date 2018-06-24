using System.Collections.Generic;

namespace WorldServer.World.BattleFronts.NewDawn
{
    public static class RacialPairHelper
    {
        public static bool Equals(RacialPair first, RacialPair second)
        {
            if (first.Tier == second.Tier)
            {
                if (first.Pairing == second.Pairing)
                {
                    if (first.PairingName == second.PairingName)
                        return true;
                }
            }
            return false;
        }
    }
}
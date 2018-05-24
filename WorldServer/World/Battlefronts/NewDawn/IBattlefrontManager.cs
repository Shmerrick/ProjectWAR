using System.Collections.Generic;

namespace WorldServer.World.Battlefronts.NewDawn
{
    public interface IBattlefrontManager
    {
        RacialPairManager RacialPairManager { get; set; }

        /// <summary>
        /// For each pairing set their active status = false.
        /// </summary>
        void ResetActivePairings();

        /// <summary>
        /// Returns the Active battlefront pair.
        /// </summary>
        /// <returns></returns>
        RacialPair GetActivePairing();

        /// <summary>
        /// Set the Active battlefront pair. Set all others to be inactive.
        /// </summary>
        /// <returns></returns>
        RacialPair SetActivePairing(RacialPair newActivePair);

        /// <summary>
        /// Advance the battlefront pairing on lock of a pairing (or start of the server).
        /// Pairings logic is (if nothing active, set random T2 pairing), otherwise follow the racial pairings (eg T2 Chaos -> T3 Chaos -> T4 Chaos).
        /// On lock of T4 select random T2 pairing.
        /// </summary>
        RacialPair AdvancePairing();

        /// <summary>
        /// Return the next tier if the zone pair locks.
        /// </summary>
        /// <param name="activeTier"></param>
        /// <returns></returns>
        int GetNextTier(int activeTier);

    }
}
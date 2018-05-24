using GameData;
using System;
using System.Collections.Generic;
using NLog;

// ReSharper disable InconsistentNaming

namespace WorldServer.World.Battlefronts.NewDawn
{
    public class LowerTierBattlefrontManager : IBattlefrontManager
    {
        private static Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// List of Racial Pairs that should be dealt with using the LowerTier rules.
        /// </summary>
        public RacialPairManager RacialPairManager { get; set; }
        /// <summary>
        /// The RacialPair that is currently active.
        /// </summary>
        public RacialPair ActiveRacialPair { get; set; }

        /// <summary>
        /// Set the Active Pairing to be null. Not expected to be needed.
        /// </summary>
        public void ResetActivePairings()
        {
            ActiveRacialPair = null;
        }

        /// <summary>
        /// Returns the Active battlefront pair.
        /// </summary>
        /// <returns></returns>
        public RacialPair GetActivePairing() => ActiveRacialPair;

        /// <summary>
        /// Set and return the Active battlefront pair. 
        /// </summary>
        /// <returns></returns>
        public RacialPair SetActivePairing(RacialPair newActivePair)
        {
            ActiveRacialPair = newActivePair;

            return newActivePair;
        }

        /// <summary>
        /// Advance the battlefront pairing on lock of a pairing (or start of the server).
        /// Pairings logic is (if nothing active, set random T1 pairing), otherwise select next T1 pair
        /// </summary>
        public RacialPair AdvancePairing()
        {
            var activePairing = GetActivePairing();
            _logger.Debug($"About to Advance Pairing. Currently Active Pair : {activePairing.ToString()}");

            // If nothing is active, return T1 Emp/Chaos
            if (activePairing == null)
            {
                return SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));
            }
            else
            {
                switch (activePairing.Pairing)
                {
                    case Pairing.PAIRING_ELVES_DARKELVES:
                        return SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_GREENSKIN_DWARVES, 1));
                    case Pairing.PAIRING_EMPIRE_CHAOS:
                        return SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_ELVES_DARKELVES, 1));
                    case Pairing.PAIRING_GREENSKIN_DWARVES:
                        return SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));
                }

                return SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));
            }
        }

        /// <summary>
        /// Return the next tier if the zone pair locks.
        /// </summary>
        /// <param name="activeTier"></param>
        /// <returns></returns>
        public int GetNextTier(int activeTier)
        {
            switch (activeTier)
            {
                case 1: return 1;
                default: return 1;
            }
        }

        public LowerTierBattlefrontManager()
        {
            RacialPairManager = new LowerTierRacialPairManager();
            ActiveRacialPair = SetInitialPairActive();
        }

        public RacialPair SetInitialPairActive()
        {
            return SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 1));
        }

    }
}
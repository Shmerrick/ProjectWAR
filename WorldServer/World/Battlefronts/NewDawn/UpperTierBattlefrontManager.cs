using System;
using System.Collections.Generic;
using GameData;
using NLog;

// ReSharper disable InconsistentNaming

namespace WorldServer.World.Battlefronts.NewDawn
{
    public class UpperTierBattlefrontManager : IBattlefrontManager
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
        /// Pairings logic is (if nothing active, set random T2 pairing), otherwise follow the racial pairings (eg T2 Chaos -> T3 Chaos -> T4 Chaos).
        /// On lock of T4 select random T2 pairing.
        /// </summary>
        public RacialPair AdvancePairing()
        {
            // Current pairing that has just locked.
            var activePairing = GetActivePairing();
            _logger.Debug($"About to Advance Pairing. Currently Active Pair : {activePairing?.ToString()}");

            // If nothing is active, return T2 Emp/Chaos
            if (activePairing == null)
            {
                return SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2));
            }
            else
            {
                var nextTier = GetNextTier(activePairing.Tier);
                _logger.Debug($"Next Tier = {nextTier} ");

                if (nextTier == 5)
                {
                    return SetActivePairing(this.RacialPairManager.GetByPair(Pairing.PAIRING_LAND_OF_THE_DEAD, nextTier));
                }
                else
                {
                    // This racial group
                    var activeRacialPairing = activePairing.Pairing;
                    // If Tier 5 has locked, start a random Tier 2.
                    if ((nextTier == 2) && (activePairing.Tier == 5))
                    {
                        var race = this.RacialPairManager.GetRandomRace(activeRacialPairing);
                        _logger.Debug($"T4->T2 race : {race} ");
                        _logger.Debug($"New Active Pair : {this.RacialPairManager.GetByPair(race, nextTier).ToString()}");
                        return SetActivePairing(this.RacialPairManager.GetByPair(race, nextTier));
                    }
                }
                
                return SetActivePairing(this.RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, nextTier));
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
                case 2: return 3;
                case 3: return 4;
                case 4: return 2;
                default: return 2;
            }
        }

       

        public UpperTierBattlefrontManager()
        {
            RacialPairManager = new UpperTierRacialPairManager();
        }

        public RacialPair SetInitialPairActive()
        {
            return SetActivePairing(RacialPairManager.GetByPair(Pairing.PAIRING_EMPIRE_CHAOS, 2));
        }

      
    }
    
}
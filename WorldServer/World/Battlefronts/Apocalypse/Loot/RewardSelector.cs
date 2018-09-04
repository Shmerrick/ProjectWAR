using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    /// <summary>
    /// Select a number players from an eligible list of players to assign rewards to.
    /// </summary>
    public class RewardSelector : IRewardSelector
    {
        public IRandomGenerator RandomGenerator { get; set; }

        public RewardSelector( IRandomGenerator randomGenerator)
        {
            RandomGenerator = randomGenerator;
        }

        /// <summary>
        /// Increase the number of possible awards based upon the number of eligible Players.
        /// </summary>
        /// <param name="eligiblePlayers"></param>
        /// <returns></returns>
        public byte DetermineNumberOfAwards(uint eligiblePlayers)
        {
            byte numberOfAwards = 0;
            // Simple set for low pop for now. TODO base this upon population sizes and % chance to win a bag per flip.
            if (eligiblePlayers == 0)
                numberOfAwards= 0;
            else
            {
                if (eligiblePlayers < 10)
                    numberOfAwards = 4;
                else
                {
                    if (eligiblePlayers < 20)
                        numberOfAwards = 6;
                    else
                    {
                        numberOfAwards = (byte) (eligiblePlayers < 40 ? 12 : 20);
                    }
                }
            }
            if (eligiblePlayers < numberOfAwards)
                numberOfAwards = (byte) eligiblePlayers;

            return numberOfAwards;
        }


        public List<uint> SelectAwardedPlayers(List<uint> randomisedPlayers, byte numberOfAwards)
        {
            if (randomisedPlayers == null)
                return null;
            if (randomisedPlayers.Count == 0)
                return null;

            return randomisedPlayers.Take(numberOfAwards).ToList();

        }


        /// <summary>
        /// To remove bias, randomise the list of eligible players
        /// </summary>
        /// <param name="nonRandomisedPlayers"></param>
        /// <returns></returns>
        public List<uint> RandomisePlayerList(List<uint> nonRandomisedPlayers)
        {
            if (nonRandomisedPlayers == null)
                return null;

            int n = nonRandomisedPlayers.Count;

            for (int i = nonRandomisedPlayers.Count - 1; i > 1; i--)
            {
                var rnd = RandomGenerator.Generate(i + 1);

                var value = nonRandomisedPlayers[rnd];
                nonRandomisedPlayers[rnd] = nonRandomisedPlayers[i];
                nonRandomisedPlayers[i] = value;
            }

            return nonRandomisedPlayers;
        }
    }
}

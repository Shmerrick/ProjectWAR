using System;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class RandomGenerator : IRandomGenerator
    {
        public int Generate(int max)
        {
            return (new Random()).Next(max);
        }
    }
}

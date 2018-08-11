using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class RandomGenerator : IRandomGenerator
    {
        public int Generate(int max)
        {
            return (new Random()).Next(max);
        }
    }
}

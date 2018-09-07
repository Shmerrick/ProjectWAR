using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public interface IRandomGenerator
    {
        int Generate(int max);
    }
}

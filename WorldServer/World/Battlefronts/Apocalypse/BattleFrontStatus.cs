using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class ApocBattleFrontStatus
    {
        public int BattleFrontId { get; set; }
        public Realms LockingRealm { get; set; }
        public VictoryPointProgress FinalVictoryPoint { get; set; }
        public int OpenTimeStamp { get; set; }
        public int LockTimeStamp { get; set; }
        public bool Locked { get; set; }

    }

}

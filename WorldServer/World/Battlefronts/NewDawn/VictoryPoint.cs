using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.NewDawn
{
    public class VictoryPoint
    {
        private readonly Object thisLock = new Object();
        private float _dVP;
        public float DestructionVictoryPoints
        {
            get { return _dVP; }
            set
            {
                lock (thisLock)
                {
                    _dVP = value;
                }
            }
        }

        private float _oVP;
        public float OrderVictoryPoints
        {
            get { return _oVP; }
            set
            {
                lock (thisLock)
                {
                    _oVP = value;
                }
            }
        }

        public VictoryPoint()
        {
            OrderVictoryPoints = 0;
            DestructionVictoryPoints = 0;
        }

        public VictoryPoint(int orderPoints, int destructionPoints)
        {
            OrderVictoryPoints = orderPoints;
            DestructionVictoryPoints = destructionPoints;
        }
    }
}

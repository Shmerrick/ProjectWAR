using System;
using System.Linq;
using WorldServer.World.Objects;

namespace WorldServer.World.AI
{
    public class RangedBrain : ABrain
    {
        public bool potionUsed { get; set; }
        public int nextDetauntAvailable { get; set; }

        public RangedBrain(Unit myOwner)
            : base(myOwner)
        {
            potionUsed = false;
            nextDetauntAvailable = 0;

        }

        public override void Think(long tick)
        {
            base.Think(tick);

            if (_unit.PlayersInRange.Count > 0)
            {
                var enemyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm != _unit.Realm).ToList();
                if (enemyPlayers.Count() > 0)
                {
                    _unit.MvtInterface.TacticalWithdrawl(enemyPlayers[0], 30, 60, false, true);
                }
            }
        }

        internal void Think()
        {
            throw new NotImplementedException();
        }
    }
}

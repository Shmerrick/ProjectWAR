using System.Linq;

namespace WorldServer
{
    public class PassiveBrain : ABrain
    {
        public PassiveBrain(Unit myOwner)
            : base(myOwner)
        {
        }

        public override void Think()
        {
            base.Think();

            if (_unit.PlayersInRange.Count > 0)
            {
                var enemyPlayers = _unit.GetPlayersInRange(30, false).Where(x => x.Realm != _unit.Realm).ToList();
                if (enemyPlayers.Count() > 0)
                {
                    _unit.MvtInterface.TacticalWithdrawl(enemyPlayers[0], 30, 30, false, true);
                }
            }
        }
    }
}

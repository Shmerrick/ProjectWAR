using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Objects;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class SiegeMerchantLocationComparitor : ILocationComparitor
    {
        public int ComparisonRange = 200;

        public bool InRange(Player player)
        {
            var creaturesInRange = player.GetInRange<Creature>(ComparisonRange);
            foreach (var creature in creaturesInRange)
            {
                if (creature.IsSiegeMerchant() && creature.Realm == player.Realm && (creature is KeepCreature))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
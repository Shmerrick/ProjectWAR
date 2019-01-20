namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class SiegeMerchantLocationComparitor : ILocationComparitor
    {
        public int ComparisonRange = 100;

        public bool InRange(Player player)
        {
            var creaturesInRange = player.GetInRange<Creature>(ComparisonRange);
            foreach (var creature in creaturesInRange)
            {
                if (creature.IsSiegeMerchant() && creature.Realm == player.Realm)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
using System.Linq;
using GameData;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class RamSpawnFlagComparitor : ILocationComparitor
    {
        public Realms PlayerRealm { get; }
        public int ComparisonRange = 150;

        public RamSpawnFlagComparitor(Realms playerRealm)
        {
            PlayerRealm = playerRealm;
        }

        public bool InRange(Player player)
        {
            if (player.Realm != PlayerRealm)
                return false;

            var objectsInRange = player.GetInRange<GameObject>(ComparisonRange);
            foreach (var test in objectsInRange)
            {
                switch (PlayerRealm)
                {
                    case Realms.REALMS_REALM_DESTRUCTION when test.Entry == 666572:
                        return true;
                    case Realms.REALMS_REALM_ORDER when test.Entry == 666571:
                        return true;

                    default:
                        return false;

                }
            }
            return false;
        }
    }
}
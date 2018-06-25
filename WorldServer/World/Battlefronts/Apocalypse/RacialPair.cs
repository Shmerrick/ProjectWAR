using GameData;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class RacialPair
    {

        public RacialPairIdentifier Id { get; set; }
        public Pairing Pairing { get; set; }
        public string PairingName { get; set; }
        public int Tier { get; set; }
        public int RegionId { get; set; }

        public RacialPair(RacialPairIdentifier id, Pairing pairing, string pairingName, int tier, int regionId)
        {
            Id = id;
            Pairing = pairing;
            PairingName = pairingName;
            Tier = tier;
            RegionId = regionId;
        }

        public override string ToString()
        {
            return $"Pair:{PairingName} Tier:{Tier} Region:{RegionId}";
        }
    }
}
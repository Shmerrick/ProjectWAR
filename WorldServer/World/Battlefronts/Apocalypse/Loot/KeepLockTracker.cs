using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldServer.World.Battlefronts.Apocalypse.Loot
{
    public class KeepLockTracker
    {
        public uint CharacterId { get; set; }
        public int RandomBonus { get; set; }
        public int PairingBonus { get; set; }
        public int GreenBagBonus { get; set; }
        public int WhiteBagBonus { get; set; }
        public int BlueBagBonus { get; set; }
        public int PurpleBagBonus { get; set; }
        public int GoldBagBonus { get; set; }
        public int ZoneContribution { get; set; }

        public override string ToString()
        {
            return $"Character {CharacterId}, Base {ZoneContribution} Random {RandomBonus} Pairing {PairingBonus} Bag Bonus : {GoldBagBonus}/{PurpleBagBonus}/{BlueBagBonus}/{GreenBagBonus}/{WhiteBagBonus}";
        }
    }

}

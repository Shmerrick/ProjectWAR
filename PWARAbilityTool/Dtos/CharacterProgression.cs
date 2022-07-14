using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PWARAbilityTool
{
    public class CharacterProgression
    {
        public int CharacterId { get; set; }
        public int Level { get; set; }
        public long Xp { get; set; }
        public int RenownRank { get; set; }
        public long Money { get; set; }
        public DateTime Timestamp { get; set; }
        public long Renown { get; set; }
    }
}

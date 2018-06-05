using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApocalypseAPI.Models
{
    public class Character
    {
        public int CharacterId { get; set; }
        public string Name { get; set; }
        public int CharacterLevel { get; set; }
        public int RenownLevel { get; set; }
        public int Career { get; set; }
        public int Realm { get; set; }
        public int ZoneId { get; set; }
        
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterUtility
{
    public class Character
    {
        public int CharacterId { get; set; }
        public string Name { get; set; }
        public int RealmId { get; set; }
        public int AccountId { get; set; }
        public int SlotId { get; set; }
        public int ModelId { get; set; }
        public int Career { get; set; }
        public int CareerLine { get; set; }
        public int Race { get; set; }
        public int Sex { get; set; }
        public string Surname { get; set; }
        public int RenownRank { get; set; }
        public int Level { get; set; }
        public int Realm { get; set; }

        public CharacterInfo BaseCharacterInfo { get; set; }
        

        public Character()
        {
            BaseCharacterInfo = new CharacterInfo();
        }



    }
}

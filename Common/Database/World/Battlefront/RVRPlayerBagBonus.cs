using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "character_bag_bonus", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RVRPlayerBagBonus : DataObject
    {
        [PrimaryKey (AutoIncrement = true)]
        public long BonusId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int GoldBag { get; set; }

        [DataElement(AllowDbNull = false)]
        public int PurpleBag { get; set; }

        [DataElement(AllowDbNull = false)]
        public int BlueBag { get; set; }

        [DataElement(AllowDbNull = false)]
        public int GreenBag { get; set; }

        [DataElement(AllowDbNull = false)]
        public int WhiteBag { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }

        [DataElement(AllowDbNull = false)]
        public int CharacterId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string CharacterName { get; set; }


        public override string ToString()
        {
            return $"Id : {BonusId}. {CharacterName} ({CharacterId}). {GoldBag}/{PurpleBag}/{BlueBag}/{GreenBag}/{WhiteBag}";
        }
    }
}

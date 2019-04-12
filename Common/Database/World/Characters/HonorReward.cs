using System;
using FrameWork;

namespace Common.Database.World.Characters
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "honor_rewards", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class HonorReward : DataObject
    {
        [PrimaryKey]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int HonorRank { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Realm { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Class { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Cooldown { get; set; }

        [DataElement(AllowDbNull = false)]
        public int MaxCount { get; set; }
        [DataElement(AllowDbNull = false)]
        public int ItemCount { get; set; }
    }
}

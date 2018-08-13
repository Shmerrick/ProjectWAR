using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{

    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_zone_lock_reward", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RVRZoneLockReward : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int RewardId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Rarity { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RRBand { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Class { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ItemCount { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte CanAwardDuplicate { get; set; }

        
    }


}

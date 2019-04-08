using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{

    [DataTable(PreCache = false, TableName = "rvr_reward_keep_items", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RVRRewardKeepItems : DataObject
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

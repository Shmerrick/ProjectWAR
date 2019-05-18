using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{

    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_keep_lock_reward", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RVRKeepLockReward : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int RewardId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Renown { get; set; }

        [DataElement(AllowDbNull = false)]
        public int XP { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Influence { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ItemCount { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RRBand { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Money { get; set; }



    }


}

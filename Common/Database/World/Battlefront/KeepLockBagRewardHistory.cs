using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_keep_lock_bag_reward_history", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class KeepLockBagRewardHistory : DataObject
    {
        [PrimaryKey (AutoIncrement = true)]
        public long HistoryId { get; set; }

        [DataElement(AllowDbNull = false), PrimaryKey]
        public int ZoneId { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }

        [DataElement(AllowDbNull = false)]
        public int LockingRealm { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public int CharacterId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string CharacterName { get; set; }

        [DataElement(AllowDbNull = false)]
        public int BagRarity { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string ZoneName { get; set; }

        [DataElement(AllowDbNull = false)]
        public string ItemName { get; set; }

        [DataElement(AllowDbNull = false)]
        public int KeepId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string KeepName { get; set; }

    }
}

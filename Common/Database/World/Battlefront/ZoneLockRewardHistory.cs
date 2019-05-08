using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_zone_lock_bag_reward_history", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class ZoneLockRewardHistory : DataObject
    {
        [PrimaryKey]
        public long LockId { get; set; }

        [DataElement(AllowDbNull = false), PrimaryKey]
        public int RegionId { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Tier { get; set; }

        [DataElement(AllowDbNull = false)]
        public int LockingRealm { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public int CharacterId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string CharacterName { get; set; }

        [DataElement(AllowDbNull = false)]
        public int BagRarity { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Contribution { get; set; }


    }
}

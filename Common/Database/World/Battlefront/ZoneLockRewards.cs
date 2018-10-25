using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_zone_lock_rewards", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class ZoneLockRewards : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int LockId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int CharacterId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int CharacterStatus { get; set; }

        [DataElement(AllowDbNull = false)]
        public int RRAwarded { get; set; }

        [DataElement(AllowDbNull = false)]
        public int INFAwarded { get; set; }

        [DataElement(AllowDbNull = false)]
        public int MoneyAwarded { get; set; }

        [DataElement(AllowDbNull = false)]
        public int BagAwarded { get; set; }

    }
}

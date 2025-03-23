using FrameWork;
using System;

namespace Common.Database.World.LiveEvents
{
    [DataTable(PreCache = false, TableName = "liveevent_reward_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class LiveEventReward_Info : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint LiveEventId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint RewardGroupId { get; set; } //1 to 3

        [DataElement(AllowDbNull = false)]
        public uint ItemId { get; set; }

        public Item_Info Item { get; set; }
    }
}
using FrameWork;
using System;

namespace Common.Database.World.Characters
{
    [DataTable(PreCache = false, TableName = "honor_reward_cooldown", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class HonorRewardCooldown : DataObject
    {

        [PrimaryKey]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public long Cooldown { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint CharacterId { get; set; }

     
    }
}

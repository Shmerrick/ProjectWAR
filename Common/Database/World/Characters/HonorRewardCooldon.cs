using FrameWork;
using System;

namespace Common.Database.World.Characters
{
    [DataTable(PreCache = false, TableName = "character_honor_reward_cooldown", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class HonorRewardCooldown : DataObject
    {

        
        [PrimaryKey]
        public uint CharacterId { get; set; }

        [PrimaryKey]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public long Cooldown { get; set; }

     
    }
}

using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_player_kill_reward_history", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PlayerKillRewardHistory : DataObject
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
        public int KillerCharacterId { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public int VictimCharacterId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ItemId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string ItemName { get; set; }

        [DataElement(AllowDbNull = false)]
        public string KillerCharacterName { get; set; }

        [DataElement(AllowDbNull = false)]
        public string VictimCharacterName { get; set; }

    }
}

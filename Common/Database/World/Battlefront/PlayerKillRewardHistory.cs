using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "rvr_player_kill_reward_history", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PlayerKillRewardHistory : DataObject
    {
        [PrimaryKey(AutoIncrement=true)]
        public int KillId { get; set; }

        [DataElement(AllowDbNull = false), PrimaryKey]
        public int ZoneId { get; set; }
        
        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }

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

        [DataElement(AllowDbNull = false)]
        public string ZoneName { get; set; }


    }
}

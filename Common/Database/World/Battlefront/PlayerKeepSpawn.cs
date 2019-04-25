using FrameWork;
using System;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "player_keep_spawn", DatabaseName = "World",
        BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PlayerKeepSpawn : DataObject
    {
        [PrimaryKey] public int KeepId { get; set; }

       
        [DataElement(AllowDbNull = false)]
        public int DefenderKeepSafeX { get; set; }
        [DataElement(AllowDbNull = false)]
        public int DefenderKeepSafeY { get; set; }
        [DataElement(AllowDbNull = false)]
        public int DefenderKeepSafeZ { get; set; }
        [DataElement(AllowDbNull = false)]
        public int OrderFeedZoneId { get; set; }
        [DataElement(AllowDbNull = false)]
        public int DestructionFeedZoneId { get; set; }

       
    }
}
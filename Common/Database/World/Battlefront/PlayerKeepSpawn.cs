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
        public int AttackerX { get; set; }
        [DataElement(AllowDbNull = false)]
        public int AttackerY { get; set; }
        [DataElement(AllowDbNull = false)]
        public int AttackerZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public int DefenderKeepSafeX { get; set; }
        [DataElement(AllowDbNull = false)]
        public int DefenderKeepSafeY { get; set; }
        [DataElement(AllowDbNull = false)]
        public int DefenderKeepSafeZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public int DefenderKeepUnderAttackX { get; set; }
        [DataElement(AllowDbNull = false)]
        public int DefenderKeepUnderAttackY { get; set; }
        [DataElement(AllowDbNull = false)]
        public int DefenderKeepUnderAttackZ { get; set; }

    }
}
using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    [DataTable(PreCache = false, TableName = "keep_spawn_points", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class KeepSiegeSpawnPoints : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int Id { get; set; }

        [DataElement(AllowDbNull = false)]
        public int KeepId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int X { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Y { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Z { get; set; }

        [DataElement(AllowDbNull = false)]
        public int O { get; set; }

        [DataElement(AllowDbNull = false)]
        public int SiegeType { get; set; }
    }
}

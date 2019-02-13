using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "BattleFront_objectives", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BattleFront_Objective : DataObject
    {
        [PrimaryKey]
        public int Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort RegionId { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneId { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Name { get; set; }

        [DataElement(AllowDbNull = false)]
        public int X { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Y { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Z { get; set; }

        [DataElement(AllowDbNull = false)]
        public int O { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint TokDiscovered { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint TokUnlocked { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool KeepSpawn { get; set; }

        public List<BattleFront_Guard> Guards;
    }
}

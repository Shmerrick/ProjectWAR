using System;
using FrameWork;

namespace Common
{

    [DataTable(PreCache = false, TableName = "rallypoints", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class RallyPoint : DataObject
    {
        [PrimaryKey]
        public ushort Id { get; set; }

        [DataElement()]
        public uint CreatureId { get; set; }

        [DataElement()]
        public string Name { get; set; }

        [DataElement()]
        public ushort ZoneID { get; set; }

        [DataElement()]
        public uint WorldX { get; set; }

        [DataElement()]
        public uint WorldY { get; set; }

        [DataElement()]
        public ushort WorldZ { get; set; }

        [DataElement()]
        public ushort WorldO { get; set; }
    }
}

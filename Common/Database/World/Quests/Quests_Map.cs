using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "quests_maps", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Quest_Map : DataObject
    {
        [DataElement()]
        public ushort Entry { get; set; }

        [DataElement()]
        public byte Id { get; set; }

        [DataElement()]
        public string Name { get; set; }

        [DataElement()]
        public string Description { get; set; }

        [DataElement()]
        public ushort ZoneId { get; set; }

        [DataElement()]
        public ushort Icon { get; set; }

        [DataElement()]
        public ushort X { get; set; }

        [DataElement()]
        public ushort Y { get; set; }

        [DataElement()]
        public ushort Unk { get; set; }

        [DataElement()]
        public byte When { get; set; }
    }
}

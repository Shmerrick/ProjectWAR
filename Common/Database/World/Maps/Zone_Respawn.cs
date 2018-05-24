using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "zone_respawns", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Zone_Respawn : DataObject
    {
        [PrimaryKey(AutoIncrement = true)]
        public int RespawnID { get; set; }

        [DataElement(AllowDbNull = false)]
        public int ZoneID { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Realm { get; set; }

        [DataElement(AllowDbNull=false)]
        public ushort PinX { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort PinY { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort PinZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort WorldO { get; set; }

        [DataElement(AllowDbNull = false)]
        public int InZoneID { get; set; }
    }
}

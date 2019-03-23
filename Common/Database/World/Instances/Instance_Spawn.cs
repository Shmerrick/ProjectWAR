using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_creature_spawns", DatabaseName = "World")]
    [Serializable]
    public class Instance_Spawn : DataObject
    {
        private string _Instance_spawns_ID;

        [PrimaryKey(AutoIncrement = false)]
        public string Instance_spawns_ID
        {
            get { return _Instance_spawns_ID; }
            set { _Instance_spawns_ID = value; Dirty = true; }
        }

        [DataElement]
        public uint Entry { get; set; }

        [DataElement]
        public byte Realm { get; set; }

        [DataElement]
        public byte Level { get; set; }

        [DataElement]
        public byte Emote { get; set; }

        [DataElement]
        public ushort ZoneID { get; set; }

        [DataElement]
        public uint ConnectedbossId { get; set; }

        [DataElement]
        public uint SpawnGroupID { get; set; }

        [DataElement]
        public int WorldX { get; set; }

        [DataElement]
        public int WorldY { get; set; }

        [DataElement]
        public int WorldZ { get; set; }

        [DataElement]
        public uint WorldO { get; set; }


    }
}

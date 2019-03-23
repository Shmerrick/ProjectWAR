using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_Boss_spawns", DatabaseName = "World")]
    [Serializable]
    public class Instance_Boss_Spawn : DataObject
    {
        private string _Instance_spawns_ID;

        [PrimaryKey(AutoIncrement = false)]
        public string Instance_spawns_ID
        {
            get { return _Instance_spawns_ID; }
            set { _Instance_spawns_ID = value; Dirty = true; }
        }

        private string _name;

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string Name
        {
            get { return _name; }

            set
            {
                GenderedName = value;

                int caratPos = value.IndexOf("^", StringComparison.Ordinal);

                if (caratPos == -1)
                    _name = value;
                else
                    _name = value.Substring(0, caratPos);
            }
        }
        public string GenderedName;

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
        public ushort InstanceID { get; set; }

        [DataElement]
        public uint bossId { get; set; }

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

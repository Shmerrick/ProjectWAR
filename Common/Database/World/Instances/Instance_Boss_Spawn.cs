using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_Boss_spawns", DatabaseName = "World")]
    [Serializable]
    public class Instance_Boss_Spawn : DataObject
    {
        public string GenderedName;
        private string _Instance_spawns_ID;

        private string _name;

        [DataElement]
        public uint bossId { get; set; }

        [DataElement]
        public byte Emote { get; set; }

        [DataElement]
        public uint Entry { get; set; }

        [PrimaryKey(AutoIncrement = false)]
        public string Instance_spawns_ID
        {
            get { return _Instance_spawns_ID; }
            set { _Instance_spawns_ID = value; Dirty = true; }
        }
        [DataElement]
        public ushort InstanceID { get; set; }

        [DataElement]
        public byte Level { get; set; }

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
        [DataElement]
        public byte Realm { get; set; }
        [DataElement]
        public uint SpawnGroupID { get; set; }

        [DataElement]
        public uint WorldO { get; set; }

        [DataElement]
        public int WorldX { get; set; }

        [DataElement]
        public int WorldY { get; set; }

        [DataElement]
        public int WorldZ { get; set; }

        [DataElement]
        public ushort ZoneID { get; set; }
    }
}
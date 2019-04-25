using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_encounters", DatabaseName = "World")]
    [Serializable]
    public class Instance_Encounter : DataObject
    {
        private string _Instance_Boss_ID;

        [DataElement]
        public string Instance_Boss_ID
        {
            get { return _Instance_Boss_ID; }
            set { _Instance_Boss_ID = value; Dirty = true; }
        }

        [DataElement]
        public uint Zone { get; set; }

        [DataElement]
        public uint bossId { get; set; }

        [DataElement]
        public uint InstanceID { get; set; }

        [DataElement]
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}

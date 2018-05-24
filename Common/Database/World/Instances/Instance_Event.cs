using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_events", DatabaseName = "World")]
    [Serializable]
    public class Instance_Event : DataObject
    {
        [DataElement]
        public uint Entry { get; set; }

        [DataElement]
        public uint InstanceID { get; set; }

        [DataElement]
        public uint EncounterID { get; set; }

        [DataElement]
        public uint InstanceSpawnID { get; set; }

        [DataElement]
        public uint InstanceObjectID { get; set; }

        [DataElement]
        public ushort EventType { get; set; }

        [DataElement]
        public string Note { get; set; }

        public List<Instance_Attribute> Attributes = new List<Instance_Attribute>();
        public List<Instance_Event_Command> Commands = new List<Instance_Event_Command>();
    }
}

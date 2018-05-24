using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_event_commands", DatabaseName = "World")]
    [Serializable]
    public class Instance_Event_Command : DataObject
    {
        [DataElement]
        public uint Entry { get; set; }

        [DataElement]
        public uint InstanceEventID { get; set; }

        [DataElement]
        public ushort CommandType { get; set; }

        [DataElement]
        public string Note { get; set; }

        public List<Instance_Attribute> Attributes = new List<Instance_Attribute>();
    }
}

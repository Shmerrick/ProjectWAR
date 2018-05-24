using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{

    [DataTable(PreCache = false, TableName = "instance_scripts", DatabaseName = "World")]
    [Serializable]
    public class Instance_Script : DataObject
    {
        [DataElement]
        public uint Entry { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public uint InstanceID { get; set; }

        [DataElement]
        public string Script { get; set; }

        public object Object;

        public override string ToString()
        {
            return Name;
        }
    }
}

using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_lockouts", DatabaseName = "World")]
    [Serializable]
    public class Instance_Lockouts : DataObject
    {
        [DataElement]
        public string InstanceID { get; set; }

        [DataElement]
        public string Bosseskilled { get; set; }
    }
}

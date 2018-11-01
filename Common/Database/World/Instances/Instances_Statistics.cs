using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_statistics", DatabaseName = "World")]
    [Serializable]
    public class Instances_Statistics : DataObject
    {
        [DataElement]
        public string InstanceID { get; set; }

        [DataElement]
        public string lockouts_InstanceID { get; set; }

        [DataElement]
        public string playerIDs { get; set; }

        [DataElement]
        public string ttkPerBoss { get; set; }
        
        [DataElement]
        public string deathCountPerBoss { get; set; }

        [DataElement]
        public string attemptsPerBoss { get; set; }
    }
}

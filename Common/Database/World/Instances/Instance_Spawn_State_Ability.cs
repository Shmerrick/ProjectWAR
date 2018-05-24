using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_spawn_state_abilities", DatabaseName = "World")]
    [Serializable]
    public class Instance_Spawn_State_Ability : DataObject
    {
        [DataElement]
        public uint Entry { get; set; }

        [DataElement]
        public uint InstanceSpawnStateID { get; set; }

        [DataElement]
        public uint AbilityID { get; set; }

        [DataElement]
        public string Note { get; set; }

        public List<Instance_Attribute> Attributes = new List<Instance_Attribute>();

        public override string ToString()
        {
            return Note;
        }
    }
}

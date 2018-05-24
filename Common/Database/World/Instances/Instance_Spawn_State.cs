using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_spawn_states", DatabaseName = "World")]
    [Serializable]
    public class Instance_Spawn_State : DataObject
    {
        [DataElement]
        public uint Entry { get; set; }

        [DataElement]
        public uint InstanceSpawnID { get; set; }

        [DataElement]
        public uint InstanceObjectID { get; set; }


        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public string Note { get; set; }


        public List<Instance_Spawn_State_Ability> Abilities = new List<Instance_Spawn_State_Ability>();
        public List<Instance_Attribute> Attributes = new List<Instance_Attribute>();


        public override string ToString()
        {
            return Name;
        }
    }
}

using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_objects", DatabaseName = "World")]
    [Serializable]
    public class Instance_Object : DataObject
    {
        [DataElement]
        public uint Entry { get; set; }

        [DataElement]
        public uint InstanceID { get; set; }

        [DataElement]
        public uint EncounterID { get; set; }

        [DataElement]
        public uint DoorID { get; set; }

        [DataElement]
        public uint GameObjectSpawnID { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public uint WorldX { get; set; }

        [DataElement]
        public uint WorldY { get; set; }

        [DataElement]
        public uint WorldZ { get; set; }

        [DataElement]
        public uint WorldO { get; set; }

        [DataElement]
        public uint DisplayID { get; set; }

        [DataElement]
        public uint VfxState { get; set; }


        public List<Instance_Spawn_State> Scripts = new List<Instance_Spawn_State>();
        public List<Instance_Attribute> Attributes = new List<Instance_Attribute>();
        public List<Instance_Event> Events = new List<Instance_Event>();
   
        public override string ToString()
        {
            return Name;
        }
    }
}

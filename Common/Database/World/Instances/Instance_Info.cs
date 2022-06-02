using FrameWork;
using System;
using System.Collections.Generic;

namespace Common
{
    [DataTable(PreCache = false, TableName = "instance_infos", DatabaseName = "World")]
    [Serializable]
    public class Instance_Info : DataObject
    {
        public List<Instance_Attribute> Attributes = new List<Instance_Attribute>();

        public List<Instance_Encounter> Encounters = new List<Instance_Encounter>();

        public List<Instance_Event> Events = new List<Instance_Event>();

        public List<Instance_Spawn> Monsters = new List<Instance_Spawn>();

        public List<Instance_Object> Objects = new List<Instance_Object>();

        public List<Instance_Script> Scripts = new List<Instance_Script>();

        [DataElement]
        public uint DestrExitZoneJumpID { get; set; }

        [DataElement]
        public ushort Entry { get; set; }

        [DataElement]
        public uint LockoutTimer { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public uint OrderExitZoneJumpID { get; set; }

        [DataElement]
        public uint TrashRespawnTimer { get; set; }

        [DataElement]
        public byte WardsNeeded { get; set; }

        [DataElement]
        public ushort ZoneID { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
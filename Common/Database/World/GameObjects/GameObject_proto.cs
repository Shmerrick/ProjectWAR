﻿using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "gameobject_protos", DatabaseName = "World")]
    [Serializable]
    public class GameObject_proto : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(Varchar = 255)]
        public string Name { get; set; }

        [DataElement(AllowDbNull = true)]
        public ushort DisplayID { get; set; }

        [DataElement(AllowDbNull = true)]
        public ushort Scale { get; set; }

        [DataElement(AllowDbNull = true)]
        public byte Level { get; set; }

        [DataElement(AllowDbNull = true)]
        public byte Faction { get; set; }

        [DataElement(AllowDbNull = true)]
        public uint HealthPoints { get; set; }

        [DataElement(AllowDbNull = true)]
        public string TokUnlock { get; set; }

        [DataElement(AllowDbNull = true)]
        public ushort[] Unks { get; set; } = new ushort[6];

        public ushort GetUnk(int Id)
        {
            if (Id >= Unks.Length)
                return 0;

            return Unks[Id];
        }

        [DataElement()]
        public byte Unk1 { get; set; }

        [DataElement()]
        public byte Unk2 { get; set; }

        [DataElement()]
        public uint Unk3 { get; set; }

        [DataElement()]
        public uint Unk4 { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = true)]
        public string ScriptName { get; set; }

        [DataElement()] // Used to spawn NPCs from GOs
        public uint CreatureSpawnId { get; set; }
		
		[DataElement()] // Used to spawn NPCs from GOs
        public uint QuestCreatureId { get; set; }
		
        [DataElement()] // Used to spawn NPCs from GOs
        public uint QuestCreatureCount { get; set; }
		
        [DataElement()] // Used to spawn NPCs from GOs
        public uint CreatureSpawnCount { get; set; }

        [DataElement()] // Used to spawn NPCs from GOs
        public string CreatureSpawnText { get; set; }

        [DataElement()] // Used to spawn NPCs from GOs
        public uint CreatureSpawnCooldownMinutes { get; set; }

        [DataElement(AllowDbNull = true)]
        public byte CreatureSpawnIsAttackable { get; set; }
    }
}
using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "gameobject_spawns", DatabaseName = "World")]
    [Serializable]
    public class GameObject_spawn : DataObject
    {
        public GameObject_proto Proto;

        [PrimaryKey(AutoIncrement = true)]
        public uint Guid { get; set; }

        [DataElement()]
        public uint Entry { get; set; }

        [DataElement()]
        public ushort ZoneId { get; set; }

        [DataElement()]
        public int WorldX { get; set; }

        [DataElement()]
        public int WorldY { get; set; }

        [DataElement()]
        public int WorldZ { get; set; }

        [DataElement()]
        public int WorldO { get; set; }

        [DataElement()]
        public uint DisplayID { get; set; }

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

        [DataElement()]
        public uint VfxState { get; set; }

        [DataElement()]
        public uint AllowVfxUpdate { get; set; }

        [DataElement()]
        public uint DoorId { get; set; }

        [DataElement()]
        public string TokUnlock { get; set; }

        [DataElement()]
        public uint SoundId { get; set; }

        [DataElement()]
        public string AlternativeName { get; set; }

        public void BuildFromProto(GameObject_proto Proto)
        {
            if (Proto == null)
            {
                return;
            }
            this.Proto = Proto;
            Entry = Proto.Entry;
            Unks = Proto.Unks;
            DisplayID = Proto.DisplayID;
        }
    }
}
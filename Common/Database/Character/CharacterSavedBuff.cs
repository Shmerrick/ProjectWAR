using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "character_saved_buffs", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class CharacterSavedBuff : DataObject
    {
        [PrimaryKey]
        public ushort BuffId { get; set; }

        [PrimaryKey]
        public uint CharacterId { get; set; }
        [DataElement]
        public uint EndTimeSeconds { get; set; }

        [DataElement]
        public byte Level { get; set; }

        [DataElement]
        public byte StackLevel { get; set; }
    }
}
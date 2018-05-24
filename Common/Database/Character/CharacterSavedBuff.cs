using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "character_saved_buffs", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class CharacterSavedBuff : DataObject
    {
        [PrimaryKey]
        public uint CharacterId { get; set; }

        [PrimaryKey]
        public ushort BuffId { get; set; }

        [DataElement]
        public byte Level { get; set; }

        [DataElement]
        public byte StackLevel { get; set; }

        [DataElement]
        public uint EndTimeSeconds { get; set; }
    }
}

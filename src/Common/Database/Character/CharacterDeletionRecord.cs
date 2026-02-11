using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "character_deletions", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class CharacterDeletionRecord : DataObject
    {
        [DataElement]
        public string DeletionIP { get; set; }

        [DataElement]
        public int AccountID { get; set; }

        [DataElement]
        public string AccountName { get; set; }

        [DataElement]
        public uint CharacterID { get; set; }

        [DataElement]
        public string CharacterName { get; set; }

        [DataElement]
        public int DeletionTimeSeconds { get; set; }
    }
}
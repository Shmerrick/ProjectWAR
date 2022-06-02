using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "character_abilities", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Character_ability : DataObject
    {
        [DataElement()]
        public ushort AbilityID { get; set; }

        [DataElement()]
        public int CharacterID { get; set; }
        [DataElement()]
        public int LastCast { get; set; }
    }
}
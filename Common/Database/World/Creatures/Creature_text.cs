using FrameWork;
using System;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "creature_texts", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Creature_text : DataObject
    {
        [DataElement()]
        public uint Entry { get; set; }

        [DataElement()]
        public string Text { get; set; }
    }
}
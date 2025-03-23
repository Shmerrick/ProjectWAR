using FrameWork;
using System;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "creature_items", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Creature_item : DataObject
    {
        private uint _Entry;
        private ushort _SlotId;
        private ushort _ModelId;
        private uint _EffectId;
        private ushort _PrimaryColor;
        private ushort _SecondaryColor;

        [PrimaryKey]
        public uint Entry
        {
            get { return _Entry; }
            set { _Entry = value; Dirty = true; }
        }

        [PrimaryKey]
        public ushort SlotId
        {
            get { return _SlotId; }
            set { _SlotId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort ModelId
        {
            get { return _ModelId; }
            set { _ModelId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint EffectId
        {
            get { return _EffectId; }
            set { _EffectId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort PrimaryColor
        {
            get { return _PrimaryColor; }
            set { _PrimaryColor = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort SecondaryColor
        {
            get { return _SecondaryColor; }
            set { _SecondaryColor = value; Dirty = true; }
        }
    }
}
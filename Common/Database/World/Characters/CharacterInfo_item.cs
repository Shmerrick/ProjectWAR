using FrameWork;
using System;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "characterinfo_items", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class CharacterInfo_item : DataObject
    {
        private byte _CareerLine;
        private uint _Entry;
        private ushort _SlotId;
        private ushort _Count;
        private uint _ModelId;

        [PrimaryKey]
        public byte CareerLine
        {
            get { return _CareerLine; }
            set { _CareerLine = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
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
        public ushort Count
        {
            get { return _Count; }
            set { _Count = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint ModelId
        {
            get { return _ModelId; }
            set { _ModelId = value; Dirty = true; }
        }
    }
}
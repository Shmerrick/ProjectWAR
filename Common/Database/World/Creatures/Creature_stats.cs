using FrameWork;
using System;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "creature_stats", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Creature_stats : DataObject
    {
        private string _UUID;
        private uint _ProtoEntry;
        private uint _StatId;
        private int _StatValue;
        private string _Comment;

        [DataElement(AllowDbNull = false)]
        public uint ProtoEntry
        {
            get { return _ProtoEntry; }
            set { _ProtoEntry = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint StatId
        {
            get { return _StatId; }
            set { _StatId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int StatValue
        {
            get { return _StatValue; }
            set { _StatValue = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = true)]
        public string Comment
        {
            get { return _Comment; }
            set { _Comment = null; Dirty = true; }
        }

        [PrimaryKey]
        public string UUID
        {
            get { return _UUID; }
            set { _UUID = value; Dirty = true; }
        }
    }
}
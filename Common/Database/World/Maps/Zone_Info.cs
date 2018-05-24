using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "zone_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Zone_Info : DataObject
    {
        private ushort _ZoneId;
        private string _Name;
        private byte _MinLevel;
        private byte _MaxLevel;
        private int _Type;
        private byte _Pairing;
        private int _Tier;
        private ushort _Price;
        private ushort _Region;
        private int _OffX;
        private int _OffY;
        private bool _Collision;
        private bool _illegal;

        [PrimaryKey]
        public ushort ZoneId
        {
            get { return _ZoneId; }
            set { _ZoneId = value; Dirty = true; }
        }

        [DataElement(Varchar=255)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte MinLevel
        {
            get { return _MinLevel; }
            set { _MinLevel = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte MaxLevel
        {
            get { return _MaxLevel; }
            set { _MaxLevel = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int Type
        {
            get { return _Type; }
            set { _Type = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int Tier
        {
            get { return _Tier; }
            set { _Tier = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Pairing
        {
            get { return _Pairing; }
            set { _Pairing = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Price
        {
            get { return _Price; }
            set { _Price = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Region
        {
            get { return _Region; }
            set { _Region = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int OffX
        {
            get { return _OffX; }
            set { _OffX = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int OffY
        {
            get { return _OffY; }
            set { _OffY = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool Collision
        {
            get { return _Collision; }
            set { _Collision = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool Illegal
        {
            get { return _illegal; }
            set { _illegal = value; Dirty = true; }
        }
    }
}

using System;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "character_bag_pools", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Characters_bag_pools : DataObject
    {

        private int _CharacterId;
        private int _BagType;
        private int _BagPool_Value;

        public Characters_bag_pools()
        {
        }

        public Characters_bag_pools(int CharacterId,int Bag_Type, int BagPool_Value)
        {
            _CharacterId = CharacterId;
            _BagType = Bag_Type;
            _BagPool_Value = BagPool_Value;
        }

        [PrimaryKey]
        public int CharacterId
        {
            get { return _CharacterId; }
            set { _CharacterId = value; Dirty = true; }
        }
        [PrimaryKey]
        public int Bag_Type
        {
            get { return _BagType; }
            set { _BagType = value; Dirty = true; }
        }
        [DataElement(AllowDbNull = false)]
        public int BagPool_Value
        {
            get { return _BagPool_Value; }
            set { _BagPool_Value = value; Dirty = true; }
        }
    }
}

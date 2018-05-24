using System;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "character_influences", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Characters_influence : DataObject
    {

        private int _CharacterId;
        private ushort _InfluenceId;
        private uint _InfluenceCount;
        private bool _Tier_1_Itemtaken;
        private bool _Tier_2_Itemtaken;
        private bool _Tier_3_Itemtaken;

        public Characters_influence()
        {
        }

        public Characters_influence(int CharacterId, ushort InfluenceId, uint InfluenceCount)
        {
            _CharacterId = CharacterId;
            _InfluenceId = InfluenceId;
            _InfluenceCount = InfluenceCount;
            _Tier_1_Itemtaken = false;
            _Tier_2_Itemtaken = false;
            _Tier_3_Itemtaken = false;
        }

        [PrimaryKey]
        public int CharacterId
        {
            get { return _CharacterId; }
            set { _CharacterId = value; Dirty = true; }
        }
        [PrimaryKey]
        public ushort InfluenceId
        {
            get { return _InfluenceId; }
            set { _InfluenceId = value; }
        }
        [DataElement(AllowDbNull = false)]
        public uint InfluenceCount
        {
            get { return _InfluenceCount; }
            set { _InfluenceCount = value; Dirty = true; }
        }
        [DataElement(AllowDbNull = false)]
        public bool Tier_1_Itemtaken
        {
            get { return _Tier_1_Itemtaken; }
            set { _Tier_1_Itemtaken = value; Dirty = true; }
        }
        [DataElement(AllowDbNull = false)]
        public bool Tier_2_Itemtaken
        {
            get { return _Tier_2_Itemtaken; }
            set { _Tier_2_Itemtaken = value; Dirty = true; }
        }
        [DataElement(AllowDbNull = false)]
        public bool Tier_3_Itemtaken
        {
            get { return _Tier_3_Itemtaken; }
            set { _Tier_3_Itemtaken = value; Dirty = true; }
        }
    }
}

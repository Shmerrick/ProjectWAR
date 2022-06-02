using FrameWork;
using System;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "guild_ranks", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Guild_rank : DataObject
    {
        private bool _Enabled;
        private uint _GuildId;
        private string _Name;
        private string _Permissions;
        private byte _RankId;
        [DataElement(AllowDbNull = false)]
        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; Dirty = true; }
        }

        [PrimaryKey]
        public uint GuildId
        {
            get { return _GuildId; }
            set { _GuildId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Permissions
        {
            get { return _Permissions; }
            set { _Permissions = value; Dirty = true; }
        }

        [PrimaryKey]
        public byte RankId
        {
            get { return _RankId; }
            set { _RankId = value; Dirty = true; }
        }
    }
}
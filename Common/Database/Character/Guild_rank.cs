using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Valeur Fixe d'un character
    [DataTable(PreCache = false, TableName = "guild_ranks", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Guild_rank : DataObject
    {
        private uint _GuildId;
        private byte _RankId;
        private string _Name;
        private string _Permissions;
        private bool _Enabled;

        [PrimaryKey]
        public uint GuildId
        {
            get { return _GuildId; }
            set { _GuildId = value; Dirty = true; }
        }

        [PrimaryKey]
        public byte RankId
        {
            get { return _RankId; }
            set { _RankId = value; Dirty = true; }
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

        [DataElement(AllowDbNull = false)]
        public bool Enabled
        {
            get { return _Enabled; }
            set { _Enabled = value; Dirty = true; }
        }
    }
}
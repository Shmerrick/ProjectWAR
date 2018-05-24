using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "guild_members", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Guild_member : DataObject
    {
        private uint _GuildId;
        private uint _CharacterId;
        private byte _RankId;
        private string _PublicNote;
        private string _OfficerNote;
        private uint _JoinDate;
        private uint _LastSeen;
        private bool _RealmCaptain;
        private bool _AllianceOfficer;
        private bool _StandardBearer;
        private bool _GuildRecruiter;
        private ulong _RenownContributed;
        private byte _Tithe;
        private ulong _TitheContributed;

        [DataElement(AllowDbNull = false)]
        public uint GuildId
        {
            get { return _GuildId; }
            set { _GuildId = value; Dirty = true; }
        }

        [PrimaryKey]
        public uint CharacterId
        {
            get { return _CharacterId; }
            set { _CharacterId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte RankId
        {
            get { return _RankId; }
            set { _RankId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string PublicNote
        {
            get { return _PublicNote; }
            set { _PublicNote = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string OfficerNote
        {
            get { return _OfficerNote; }
            set { _OfficerNote = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint JoinDate
        {
            get { return _JoinDate; }
            set { _JoinDate = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint LastSeen
        {
            get { return _LastSeen; }
            set { _LastSeen = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool RealmCaptain
        {
            get { return _RealmCaptain; }
            set { _RealmCaptain = value; Dirty = true; }
        }

        public bool AllianceOfficer
        {
            get { return _AllianceOfficer; }
            set { _AllianceOfficer = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool StandardBearer
        {
            get { return _StandardBearer; }
            set { _StandardBearer = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool GuildRecruiter
        {
            get { return _GuildRecruiter; }
            set { _GuildRecruiter = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ulong RenownContributed
        {
            get { return _RenownContributed; }
            set { _RenownContributed = value; Dirty = true; }
        }
        
        [DataElement(AllowDbNull = false)]
        public byte Tithe
        {
            get { return _Tithe; }
            set { _Tithe = value; Dirty = true; }
        }
        
        [DataElement(AllowDbNull = false)]
        public ulong TitheContributed
        {
            get { return _TitheContributed; }
            set { _TitheContributed = value; Dirty = true; }
        }

        public Character Member;
    }
}
using FrameWork;
using System;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "guild_members", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Guild_member : DataObject
    {
        public Character Member;
        private bool _AllianceOfficer;
        private uint _CharacterId;
        private uint _GuildId;
        private bool _GuildRecruiter;
        private uint _JoinDate;
        private uint _LastSeen;
        private string _OfficerNote;
        private string _PublicNote;
        private byte _RankId;
        private bool _RealmCaptain;
        private ulong _RenownContributed;
        private bool _StandardBearer;
        private byte _Tithe;
        private ulong _TitheContributed;

        public bool AllianceOfficer
        {
            get { return _AllianceOfficer; }
            set { _AllianceOfficer = value; Dirty = true; }
        }

        [PrimaryKey]
        public uint CharacterId
        {
            get { return _CharacterId; }
            set { _CharacterId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint GuildId
        {
            get { return _GuildId; }
            set { _GuildId = value; Dirty = true; }
        }
        [DataElement(AllowDbNull = false)]
        public bool GuildRecruiter
        {
            get { return _GuildRecruiter; }
            set { _GuildRecruiter = value; Dirty = true; }
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
        public string OfficerNote
        {
            get { return _OfficerNote; }
            set { _OfficerNote = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string PublicNote
        {
            get { return _PublicNote; }
            set { _PublicNote = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte RankId
        {
            get { return _RankId; }
            set { _RankId = value; Dirty = true; }
        }
        [DataElement(AllowDbNull = false)]
        public bool RealmCaptain
        {
            get { return _RealmCaptain; }
            set { _RealmCaptain = value; Dirty = true; }
        }
        [DataElement(AllowDbNull = false)]
        public ulong RenownContributed
        {
            get { return _RenownContributed; }
            set { _RenownContributed = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool StandardBearer
        {
            get { return _StandardBearer; }
            set { _StandardBearer = value; Dirty = true; }
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
    }
}
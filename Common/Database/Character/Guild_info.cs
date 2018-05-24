using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "guild_info", DatabaseName = "Characters")]
    [Serializable]
    public class Guild_info : DataObject
    {
        private uint _GuildId;
        private string _Name;
        private byte _Level;
        private byte _Realm;
        private uint _LeaderId;
        private int _CreateDate;
        private string _Motd;
        private string _AboutUs;
        private uint _Xp;
        private ulong _Renown;
        private byte _Tax;
        private ulong _Money;
        private byte[] _guildvaultpurchased;
        private string _Heraldry;
        private string _Banners;
        private ushort[] _GuildTacticsPurchased;

        // Used on recruitment
        private string _BriefDescription;
        private string _Summary;
        private byte _PlayStyle;
        private byte _Atmosphere;
        private uint _CareersNeeded;
        private byte _Interests;
        private byte _ActivelyRecruiting;
        private byte _RanksNeeded;
        private uint _AllianceId;

        public Guild_info()
            : base()
        {

        }

        [PrimaryKey]
        public uint GuildId
        {
            get { return _GuildId; }
            set { _GuildId = value; Dirty = true; }
        }

        [DataElement(Unique = true, AllowDbNull = false, Varchar = 255)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Level
        {
            get { return _Level; }
            set { _Level = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Realm
        {
            get { return _Realm; }
            set { _Realm = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint LeaderId
        {
            get { return _LeaderId; }
            set { _LeaderId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public int CreateDate
        {
            get { return _CreateDate; }
            set { _CreateDate = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Motd
        {
            get { return _Motd; }
            set { _Motd = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string AboutUs
        {
            get { return _AboutUs; }
            set { _AboutUs = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Xp
        {
            get { return _Xp; }
            set { _Xp = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ulong Renown
        {
            get { return _Renown; }
            set { _Renown = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string BriefDescription
        {
            get { return _BriefDescription; }
            set { _BriefDescription = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Summary
        {
            get { return _Summary; }
            set { _Summary = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte PlayStyle
        {
            get { return _PlayStyle; }
            set { _PlayStyle = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Atmosphere
        {
            get { return _Atmosphere; }
            set { _Atmosphere = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint CareersNeeded
        {
            get { return _CareersNeeded; }
            set { _CareersNeeded = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Interests
        {
            get { return _Interests; }
            set { _Interests = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte ActivelyRecruiting
        {
            get { return _ActivelyRecruiting; }
            set { _ActivelyRecruiting = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte RanksNeeded
        {
            get { return _RanksNeeded; }
            set { _RanksNeeded = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Tax
        {
            get { return _Tax; }
            set { _Tax = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ulong Money
        {
            get { return _Money; }
            set { _Money = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte[] guildvaultpurchased
        {
            get
            {
                if (_guildvaultpurchased == null || _guildvaultpurchased.Length < 5)
                {
                    _guildvaultpurchased = new byte[5];
                }
                return _guildvaultpurchased;
            }
            set
            {
                _guildvaultpurchased = value;
                if (_guildvaultpurchased == null || _guildvaultpurchased.Length < 5)
                {
                    _guildvaultpurchased = new byte[5];
                }
                Dirty = true; 
            }
        }

        [DataElement(AllowDbNull = false)]
        public string Banners
        {
            get { return _Banners; }
            set { _Banners = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Heraldry
        {
            get { return _Heraldry; }
            set { _Heraldry = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort[] GuildTacticsPurchased
        {
            get
            {
                if (_GuildTacticsPurchased == null || _GuildTacticsPurchased.Length < 5)
                {
                    _GuildTacticsPurchased = new ushort[40];
                }
                return _GuildTacticsPurchased;
            }
            set
            {
                _GuildTacticsPurchased = value;
                if (_GuildTacticsPurchased == null || _GuildTacticsPurchased.Length < 5)
                {
                    _GuildTacticsPurchased = new ushort[40];
                }
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull = true)]
        public uint AllianceId
        {
            get { return _AllianceId; }
            set { _AllianceId = value; Dirty = true; }
        }

        public Dictionary<ushort, GuildVaultItem>[] Vaults = 
        {
            new Dictionary<ushort, GuildVaultItem>(),
            new Dictionary<ushort, GuildVaultItem>(),
            new Dictionary<ushort, GuildVaultItem>(),
            new Dictionary<ushort, GuildVaultItem>(),
            new Dictionary<ushort, GuildVaultItem>()
        };

        public Dictionary<uint, Guild_member> Members;
        public Dictionary<byte, Guild_rank> Ranks;
        public Dictionary<byte, Guild_event> Event = new Dictionary<byte, Guild_event>();
        public List<Guild_log> Logs;
    }
}

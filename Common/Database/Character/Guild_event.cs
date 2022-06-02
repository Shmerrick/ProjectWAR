using FrameWork;
using System;
using System.Collections.Generic;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "guild_event", DatabaseName = "Characters")]
    [Serializable]
    public class Guild_event : DataObject
    {
        public List<KeyValuePair<uint, bool>> _Signups = new List<KeyValuePair<uint, bool>>();
        private bool _Alliance;
        private uint _Begin;
        private uint _CharacterId;
        private string _Description;
        private uint _End;
        private uint _GuildId;
        private bool _Locked;
        private string _Name;
        private byte _SlotId;
        public Guild_event()
            : base()
        {
        }

        [DataElement(AllowDbNull = false)]
        public bool Alliance
        {
            get { return _Alliance; }
            set { _Alliance = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Begin
        {
            get { return _Begin; }
            set { _Begin = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint CharacterId
        {
            get { return _CharacterId; }
            set { _CharacterId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Description
        {
            get { return _Description; }
            set { _Description = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint End
        {
            get { return _End; }
            set { _End = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint GuildId
        {
            get { return _GuildId; }
            set { _GuildId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool Locked
        {
            get { return _Locked; }
            set { _Locked = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public String Signups
        {
            get
            {
                string Str = "";
                foreach (KeyValuePair<uint, bool> plr in _Signups)
                {
                    Str += plr.Key + ":" + plr.Value + ";";
                }
                return Str;
            }
            set
            {
                string[] Split = value.Split(';');
                _Signups.Clear();
                foreach (string Str in Split)
                {
                    if (Str.Length > 0)
                        _Signups.Add(new KeyValuePair<uint, bool>(UInt32.Parse(Str.Split(':')[0]), Boolean.Parse(Str.Split(':')[1]))); Dirty = true;
                }
            }
        }

        [DataElement(AllowDbNull = false)]
        public byte SlotId
        {
            get { return _SlotId; }
            set { _SlotId = value; Dirty = true; }
        }
    }
}
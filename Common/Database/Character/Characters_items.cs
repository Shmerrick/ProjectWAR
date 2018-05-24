using System;
using System.Collections.Generic;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "characters_items", DatabaseName = "Characters")]
    [Serializable]
    public class CharacterItem : DataObject
    {
        private long _guid;
        private uint _characterId;
        private uint _entry;
        private ushort _slotId;
        private uint _modelId;
        private ushort _counts;
        public List<Talisman> _Talismans = new List<Talisman>();
        private ushort _primaryDye;
        private ushort _secondaryDye;
        private bool _boundtoPlayer;

        private uint _alternateAppereanceEntry;
        private long _nextAllowedUseTime;


        [DataElement(AllowDbNull = false)]
        public long Guid
        {
            get { return _guid; }
            set { _guid = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint CharacterId
        {
            get { return _characterId; }
            set { _characterId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Entry
        {
            get { return _entry; }
            set { _entry = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort SlotId
        {
            get { return _slotId; }
            set { _slotId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint ModelId
        {
            get { return _modelId; }
            set { _modelId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Counts
        {
            get { return _counts; }
            set { _counts = value; Dirty = true; }
        }

        //[DataElement]
        public long NextAllowedUseTime
        {
            get { return _nextAllowedUseTime; }
            set { _nextAllowedUseTime = value; Dirty = true; }
        }

        public ushort RemainingCooldown
        {
            get
            {
                int curTime = TCPManager.GetTimeStamp();
                if (curTime >= NextAllowedUseTime)
                    return 0;
                return (ushort)(NextAllowedUseTime - curTime);
            }
        }

        // Note:
        //
        // If the Timer is implemented, this field may no longer be big enough to hold the data
        // - check first!
        [DataElement(Varchar = 40)]
        public string Talismans
        {
            get
            {
                string str = "";
                foreach (Talisman tali in _Talismans)
                {
                    if (_Talismans == null)
                        return "";
                    str += tali.Entry + ":" + tali.Slot + ":" + tali.Fused + ":" + tali.Timer + ";";
                }
                return str;
            }
            set
            {
                string[] split = value.Split(';');
                _Talismans.Clear();
                foreach (string str in split)
                {
                    if(str.Length > 0)
                        _Talismans.Add(new Talisman(str));
                }
            }
        }

        [DataElement(AllowDbNull = false)]
        public ushort PrimaryDye
        {
            get { return _primaryDye; }
            set { _primaryDye = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort SecondaryDye
        {
            get { return _secondaryDye; }
            set { _secondaryDye = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public bool BoundtoPlayer
        {
            get { return _boundtoPlayer; }
            set { _boundtoPlayer = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Alternate_AppereanceEntry
        {
            get { return _alternateAppereanceEntry; }
            set { _alternateAppereanceEntry = value; Dirty = true; }
        }


    }
    public class Talisman
    {
        public uint Entry = 0;
        public byte Fused = 1;
        public byte Slot = 0;
        public uint Timer = 0;

        public Talisman(String text)
        {
            if (text.Length < 5)
                return;
            Entry = UInt32.Parse(text.Split(':')[0]);
            Slot = (byte)UInt16.Parse(text.Split(':')[1]);
            Fused = (byte)UInt16.Parse(text.Split(':')[2]);
            Timer = UInt32.Parse(text.Split(':')[3]);
        }
        public Talisman(uint entry,byte slot,byte fused,uint timer)
        {
            this.Entry = entry;
            this.Slot = slot;
            this.Fused = fused;
            this.Timer = timer;
        }
    }
}

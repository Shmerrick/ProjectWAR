using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "Guild_Vault_Item", DatabaseName = "Characters")]
    [Serializable]
    public class GuildVaultItem : DataObject
    {
        private uint _GuildId;
        private uint _Entry;
        private byte _VaultId;
        private ushort _SlotId;
        private ushort _Counts;
        public List<Talisman> _Talismans = new List<Talisman>();
        private ushort _PrimaryDye;
        private ushort _SecondaryDye;
        public uint LockedPlayerId;

        [PrimaryKey]
        public uint GuildId
        {
            get { return _GuildId; }
            set { _GuildId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Entry
        {
            get { return _Entry; }
            set { _Entry = value; Dirty = true; }
        }

        [PrimaryKey]
        public byte VaultId
        {
            get { return _VaultId; }
            set { _VaultId = value; Dirty = true; }
        }

        [PrimaryKey]
        public ushort SlotId
        {
            get { return _SlotId; }
            set { _SlotId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Counts
        {
            get { return _Counts; }
            set { _Counts = value; Dirty = true; }
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
                string Str = "";
                foreach (Talisman tali in _Talismans)
                {
                    if (_Talismans == null)
                        return "";
                    Str += tali.Entry + ":" + tali.Slot + ":" + tali.Fused + ":" + tali.Timer + ";";
                }
                return Str;
            }
            set
            {
                string[] Split = value.Split(';');
                _Talismans.Clear();
                foreach (string Str in Split)
                {
                    if(Str.Length > 0)
                        _Talismans.Add(new Talisman(Str));
                }
            }
        }

        [DataElement(AllowDbNull = false)]
        public ushort PrimaryDye
        {
            get { return _PrimaryDye; }
            set { _PrimaryDye = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort SecondaryDye
        {
            get { return _SecondaryDye; }
            set { _SecondaryDye = value; Dirty = true; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "auctions", DatabaseName = "Characters")]
    [Serializable]
    public class Auction : DataObject
    {
        private ulong _AuctionId;
        private byte _Realm;
        private uint _SellerId;
        private uint _ItemId;
        public List<Talisman> _Talismans = new List<Talisman>();
        private ushort _PrimaryDye;
        private ushort _SecondaryDye;
        private uint _SellPrice;
        private uint _StartTime;
        private ushort _Count;

        [PrimaryKey]
        public ulong AuctionId
        {
            get { return _AuctionId; }
            set { _AuctionId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Realm
        {
            get { return _Realm; }
            set { _Realm = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public uint SellerId
        {
            get { return _SellerId; }
            set { _SellerId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint ItemId
        {
            get { return _ItemId; }
            set { _ItemId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint SellPrice
        {
            get { return _SellPrice; }
            set { _SellPrice = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public ushort Count
        {
            get { return _Count; }
            set { _Count = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value; Dirty = true; }
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
                    if (Str.Length > 0)
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

        public Item_Info Item;
        public Character Seller;
    }

}
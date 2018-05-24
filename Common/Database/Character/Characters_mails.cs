using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "characters_mails", DatabaseName = "Characters")]
    [Serializable]
    public class Character_mail : DataObject
    {
        private int _Guid;
        private byte _AuctionType;
        private uint _CharacterId;
        private uint _CharacterIdSender;
        private string _SenderName;
        private string _ReceiverName;
        private uint _SendDate;
        private uint _ReadDate;
        private string _Title;
        private string _Content;
        private uint _Money;
        private bool _Cr;
        private bool _Opened;

        [PrimaryKey(AutoIncrement = true)]
        public int Guid
        {
            get { return _Guid; }
            set { _Guid = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte AuctionType
        {
            get { return _AuctionType; }
            set { _AuctionType = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint CharacterId
        {
            get { return _CharacterId; }
            set { _CharacterId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint CharacterIdSender
        {
            get { return _CharacterIdSender; }
            set { _CharacterIdSender = value; Dirty = true; }
        }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string SenderName
        {
            get { return _SenderName; }
            set { _SenderName = value; Dirty = true; }
        }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string ReceiverName
        {
            get { return _ReceiverName; }
            set { _ReceiverName = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint SendDate
        {
            get { return _SendDate; }
            set { _SendDate = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint ReadDate
        {
            get { return _ReadDate; }
            set { _ReadDate = value; Dirty = true; }
        }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string Title
        {
            get { return _Title; }
            set { _Title = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Content
        {
            get { return _Content; }
            set { _Content = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public uint Money
        {
            get { return _Money; }
            set { _Money = value; Dirty = true; }
        }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public bool Cr
        {
            get { return _Cr; }
            set { _Cr = value; Dirty = true; }
        }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public bool Opened
        {
            get { return _Opened; }
            set { _Opened = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string ItemsString
        {
            get
            {
                string Value = "";
                foreach (MailItem Item in Items)
                {
                    String aditionaldata = Item.GetSaveString();
                    if(aditionaldata == null)
                        Value += Item.id + ":" + Item.count + "|";
                    else
                        Value += Item.id + ":"+aditionaldata+":" + Item.count + "|";
                }
                return Value;
            }
            set
            {
                if (value.Length <= 0)
                    return;

                string[] Objs = value.Split('|');

                foreach (string Obj in Objs)
                {
                    if (Obj.Length <= 0)
                        continue;

                    string[] ItemInfo = Obj.Split(':');

                    if(ItemInfo.Length == 2)
                        Items.Add(new MailItem(uint.Parse(ItemInfo[0]),null, ushort.Parse(ItemInfo[1])));
                    else
                        Items.Add(new MailItem(uint.Parse(ItemInfo[0]), ItemInfo[1], ushort.Parse(ItemInfo[2])));
                }
                Dirty = true;
            }
        }

        public List<MailItem> Items = new List<MailItem>();
    }
}

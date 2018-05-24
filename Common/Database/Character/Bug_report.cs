using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "bug_report", DatabaseName = "Characters")]
    [Serializable]
    public class Bug_report : DataObject
    {
        private uint _AccountId;
        private uint _CharacterId;
        private ushort _ZoneId;
        private ushort _X;
        private ushort _Y;
        private uint _Time;
        private byte _Type;
        private byte _Category;
        private string _Message;
        private string _ReportType;
        private string _Assigned;

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public uint AccountId
        {
            get { return _AccountId; }
            set { _AccountId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public uint CharacterId
        {
            get { return _CharacterId; }
            set { _CharacterId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public ushort ZoneId
        {
            get { return _ZoneId; }
            set { _ZoneId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public ushort X
        {
            get { return _X; }
            set { _X = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public ushort Y
        {
            get { return _Y; }
            set { _Y = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false, Varchar = 255)]
        public uint Time
        {
            get { return _Time; }
            set { _Time = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Type
        {
            get { return _Type; }
            set { _Type = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public byte Category
        {
            get { return _Category; }
            set { _Category = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Message
        {
            get { return _Message; }
            set { _Message = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string ReportType
        {
            get { return _ReportType; }
            set { _ReportType = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string FieldSting
        {
            get
            {
                string Value = "";
                foreach (KeyValuePair<uint, string> Item in Fields)
                    Value += Item.Key + ":" + Item.Value.Replace(":", " ") + "|";
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

                    string[] FieldInfo = Obj.Split(':');
                    Fields.Add(new KeyValuePair<uint, string>(uint.Parse(FieldInfo[0]), FieldInfo[1]));
                }
                Dirty = true;
            }
        }

        [DataElement(AllowDbNull = true)]
        public string Assigned
        {
            get { return _Assigned; }
            set { _Assigned = value; Dirty = true; }
        }

        public List<KeyValuePair<uint, string>> Fields = new List<KeyValuePair<uint, string>>();
    }
}
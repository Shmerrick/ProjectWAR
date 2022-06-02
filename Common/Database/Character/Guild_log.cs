using FrameWork;
using System;

namespace Common
{
    // Fixed value of a character
    [DataTable(PreCache = false, TableName = "guild_logs", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Guild_log : DataObject
    {
        private uint _GuildId;
        private string _Text;
        private uint _Time;
        private byte _Type;
        [DataElement(AllowDbNull = false)]
        public uint GuildId
        {
            get { return _GuildId; }
            set { _GuildId = value; Dirty = true; }
        }

        [DataElement(AllowDbNull = false)]
        public string Text
        {
            get { return _Text; }
            set { _Text = value; Dirty = true; }
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "guild_alliance_info", DatabaseName = "Characters")]
    [Serializable]
    public class Guild_Alliance_info : DataObject
    {
        private uint _AllianceId;
        private string _Name;

        [PrimaryKey]
        public uint AllianceId
        {
            get { return _AllianceId; }
            set { _AllianceId = value; Dirty = true; }
        }

        [DataElement(Unique = true, AllowDbNull = false, Varchar = 255)]
        public string Name
        {
            get { return _Name; }
            set { _Name = value; Dirty = true; }
        }

        public List<uint> Members = new List<uint>();
    }
}

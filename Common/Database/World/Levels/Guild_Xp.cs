using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "guild_xp", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Guild_Xp : DataObject
    {
        public byte _Level;
        public uint _Xp;

        [PrimaryKey]
        public byte Level
        {
            get { return _Level; }
            set { _Level = value; }
        }

        [DataElement()]
        public uint Xp
        {
            get { return _Xp; }
            set { _Xp = value; }
        }
    }
}

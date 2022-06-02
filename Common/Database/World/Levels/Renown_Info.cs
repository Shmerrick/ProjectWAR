using FrameWork;
using System;

namespace Common
{
    [DataTable(PreCache = false, TableName = "renown_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Renown_Info : DataObject
    {
        public byte _Level;
        public uint _Renown;

        [PrimaryKey]
        public byte Level
        {
            get { return _Level; }
            set { _Level = value; }
        }

        [DataElement()]
        public uint Renown
        {
            get { return _Renown; }
            set { _Renown = value; }
        }
    }
}
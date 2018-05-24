using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "gmcommandlogs", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class GMCommandLog : DataObject
    {
        [DataElement()]
        public uint AccountId { get; set; }

        [DataElement(Varchar=255)]
        public string PlayerName { get; set; }

        [DataElement()]
        public string Command { get; set; }

        [DataElement()]
        public DateTime Date { get; set; }
    }
}

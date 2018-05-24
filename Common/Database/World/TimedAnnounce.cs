using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "timedannounces", DatabaseName = "World")]
    [Serializable]
    public class TimedAnnounce : DataObject
    {
        [DataElement()]
        public string SenderName { get; set; }

        [DataElement()]
        public string Message { get; set; }

        [DataElement()]
        public ushort ZoneId { get; set; }

        [DataElement()]
        public byte Realm { get; set; }

        [DataElement()]
        public byte Type { get; set; }

        [DataElement()]
        public int NextTime { get; set; }
    }
}

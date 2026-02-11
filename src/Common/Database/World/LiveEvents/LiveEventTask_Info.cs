using FrameWork;
using System;
using System.Collections.Generic;

namespace Common.Database.World.LiveEvents
{
    [DataTable(PreCache = false, TableName = "liveevent_task_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class LiveEventTasks_Info : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint LiveEventId { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public string Description { get; set; }

        [DataElement(AllowDbNull = false)]
        public int TotalTasks { get; set; }

        public List<LiveEventSubTasks_Info> Tasks { get; set; } = new List<LiveEventSubTasks_Info>();
    }
}
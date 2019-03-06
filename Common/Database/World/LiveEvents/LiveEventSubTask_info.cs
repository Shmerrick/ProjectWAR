using FrameWork;
using System;

namespace Common.Database.World.LiveEvents
{
    [DataTable(PreCache = false, TableName = "liveevent_subtask_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class LiveEventSubTasks_Info : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint LiveEventTaskId { get; set; }

        [DataElement]
        public string Name { get; set; }

        [DataElement]
        public string Description { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint TaskCount { get; set; }
    }
}

using FrameWork;
using System;
using System.Collections.Generic;

namespace Common.Database.World.LiveEvents
{
    [DataTable(PreCache = false, TableName = "liveevent_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class LiveEvent_Info : DataObject
    {
        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Title { get; set; }

        [DataElement(AllowDbNull = false)]
        public string SubTitle { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Description { get; set; }

        [DataElement(AllowDbNull = false)]
        public string TasksDescription { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint ImageId { get; set; } //ID from data\gamedata\liveeventimages.csv DDS image must be found in interface\interfacecore\tome\achievements\images

        [DataElement]
        public DateTime StartDate { get; set; }

        [DataElement]
        public DateTime EndDate { get; set; }

        [DataElement]
        public bool Allowed { get; set; } //if true, will be presented to player if current date is within Start and End date

        public List<LiveEventReward_Info> Rewards { get; set; } = new List<LiveEventReward_Info>();
        public List<LiveEventTasks_Info> Tasks { get; set; } = new List<LiveEventTasks_Info>();
    }
}
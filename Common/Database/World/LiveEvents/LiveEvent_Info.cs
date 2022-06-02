using FrameWork;
using System;
using System.Collections.Generic;

namespace Common.Database.World.LiveEvents
{
    [DataTable(PreCache = false, TableName = "liveevent_infos", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class LiveEvent_Info : DataObject
    {
        [DataElement]
        public bool Allowed { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Description { get; set; }

        [DataElement]
        public DateTime EndDate { get; set; }

        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint ImageId { get; set; }

        public List<LiveEventReward_Info> Rewards { get; set; } = new List<LiveEventReward_Info>();

        [DataElement]
        public DateTime StartDate { get; set; }

        [DataElement(AllowDbNull = false)]
        public string SubTitle { get; set; }

        //ID from data\gamedata\liveeventimages.csv DDS image must be found in interface\interfacecore\tome\achievements\images
        //if true, will be presented to player if current date is within Start and End date
        public List<LiveEventTasks_Info> Tasks { get; set; } = new List<LiveEventTasks_Info>();

        [DataElement(AllowDbNull = false)]
        public string TasksDescription { get; set; }

        [DataElement(AllowDbNull = false)]
        public string Title { get; set; }
    }
}
using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "campaign_objective_buff", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class CampaignObjectiveBuff : DataObject
    {
        [PrimaryKey]
        public int BuffId { get; set; }

        [PrimaryKey]
        public int ObjectiveId { get; set; }


        [DataElement(AllowDbNull = false)]
        public string BuffName { get; set; }

    }
}

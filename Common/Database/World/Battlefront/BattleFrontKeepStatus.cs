using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    // Fixed value of a character 
    [DataTable(PreCache = false, TableName = "battlefront_keep_status", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BattleFrontKeepStatus : DataObject
    {
        [DataElement(AllowDbNull = false), PrimaryKey]
        public int KeepId { get; set; }

        [DataElement(AllowDbNull = false)]
        public int Status { get; set; }
    }
}

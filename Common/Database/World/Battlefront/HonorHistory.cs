using System;
using FrameWork;

namespace Common.Database.World.Battlefront
{
    [DataTable(PreCache = false, TableName = "honor_history", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class HonorHistory : DataObject
    {

        [PrimaryKey(AutoIncrement = true)]
        public int AnalyticsId { get; set; }
        [DataElement(AllowDbNull = false)]
        public DateTime Timestamp { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint CharacterId { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint CurrentHonorPoints { get; set; }
        [DataElement(AllowDbNull = false)]
        public uint OldHonorPoints { get; set; }
        [DataElement(AllowDbNull = false)]
        public int RateOfChange { get; set; }
        [DataElement(AllowDbNull = false)]
        public string CharacterName { get; set; }


    }
}
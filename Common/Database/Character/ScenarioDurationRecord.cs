using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "scenario_durations", DatabaseName = "Characters", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class ScenarioDurationRecord : DataObject
    {
        [PrimaryKey(AutoIncrement=true)]
        public int Guid { get; set; }

        [DataElement]
        public ushort ScenarioId { get; set; }

        [DataElement]
        public byte Tier { get; set; }

        [DataElement]
        public long StartTime { get; set; }

        [DataElement]
        public uint DurationSeconds { get; set; }
    }
}

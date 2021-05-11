using FrameWork;
using System;

namespace Common//new
{
    [DataTable(PreCache = false, TableName = "ability_component", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class AbilityComponent : DataObject
    {
        [PrimaryKey]
        public long ID { get; set; }

        [PrimaryKey]
        public string A00 { get; set; }

        [PrimaryKey]
        public string Values { get; set; }

        [PrimaryKey]
        public string Multipliers { get; set; }

        [PrimaryKey]
        public long ActivationDelay { get; set; }

        [PrimaryKey]
        public long Duration { get; set; }

        [PrimaryKey]
        public long Flags { get; set; }

        [PrimaryKey]
        public long IconAlwaysVisible { get; set; }

        [PrimaryKey]
        public long Operation { get; set; }

        [PrimaryKey]
        public long Interval { get; set; }

        [PrimaryKey]
        public int Radius { get; set; }

        [PrimaryKey]
        public int ConeAngle { get; set; }

        [PrimaryKey]
        public int FlightSpeed { get; set; }

        [PrimaryKey]
        public int A15 { get; set; }

        [PrimaryKey]
        public byte MaxTargets { get; set; }

        [PrimaryKey]
        public string Description { get; set; }
    }
}
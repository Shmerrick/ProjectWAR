using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FrameWork;

namespace Common.Database.World.MythicAbility
{
    [DataTable(PreCache = false, TableName = "ability_component", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]

    public class AbilityComponent : DataObject
    {
        [PrimaryKey]
        public ushort ID { get; set; }

        [PrimaryKey]
        public string A00 { get; set; }

        [PrimaryKey]
        public string Values { get; set; }

        [PrimaryKey]
        public ushort Multipliers { get; set; }

        [PrimaryKey]
        public ushort ActivationDelay { get; set; }

        [PrimaryKey]
        public ushort Duration { get; set; }

        [PrimaryKey]
        public ushort Flags { get; set; }

        [PrimaryKey]
        public ushort IconAlwaysVisible { get; set; }

        [PrimaryKey]
        public ushort Operation { get; set; }

        [PrimaryKey]
        public ushort Interval { get; set; }

        [PrimaryKey]
        public ushort Radius { get; set; }

        [PrimaryKey]
        public ushort ConeAngle { get; set; }

        [PrimaryKey]
        public ushort FlightSpeed { get; set; }

        [PrimaryKey]
        public ushort A15 { get; set; }

        [PrimaryKey]
        public ushort MaxTargets { get; set; }

        [PrimaryKey]
        public ushort Description { get; set; }
    }
}
using System;
using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "scenario_objects", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Scenario_Object : DataObject
    {
        [DataElement]
        public ushort ScenarioId { get; set; }

        [DataElement]
        public ushort Identifier { get; set; }

        [DataElement(Varchar=255)]
        public string ObjectiveName { get; set; }

        [DataElement]
        public uint ProtoEntry { get; set; }

        [DataElement(Varchar = 255)]
        public string Type { get; set; }

        [DataElement]
        public byte PointGain { get; set; }

        [DataElement]
        public byte PointOverTimeGain { get; set; }

        [DataElement]
        public int WorldPosX { get; set; }

        [DataElement]
        public int WorldPosY { get; set; }

        [DataElement]
        public ushort PosZ { get; set; }

        [DataElement]
        public ushort Heading { get; set; }

        [DataElement]
        public string CaptureObjectiveText { get; set; }

        [DataElement]
        public string CaptureObjectiveDescription { get; set; }

        [DataElement]
        public string HoldObjectiveText { get; set; }

        [DataElement]
        public string HoldObjectiveDescription { get; set; }

        [DataElement]
        public string CaptureAnnouncement { get; set; }

        [DataElement]
        public byte Realm { get; set; }
    }
}

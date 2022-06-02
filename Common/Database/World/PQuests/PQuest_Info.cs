using FrameWork;
using System;
using System.Collections.Generic;

namespace Common
{
    [DataTable(PreCache = false, TableName = "pquest_info", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class PQuest_Info : DataObject
    {
        public List<PQuest_Objective> Objectives;

        public ushort OffX;

        public ushort OffY;

        [DataElement(AllowDbNull = false)]
        public byte Chapter { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint ChapterId { get; set; }

        [PrimaryKey]
        public uint Entry { get; set; }

        [DataElement(AllowDbNull = false)]
        public int GoldChestWorldX { get; set; }

        [DataElement(AllowDbNull = false)]
        public int GoldChestWorldY { get; set; }

        [DataElement(AllowDbNull = false)]
        public int GoldChestWorldZ { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Level { get; set; }

        [DataElement(Varchar = 255, AllowDbNull = false)]
        public string Name { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint PinX { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint PinY { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte PQAreaId { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte PQCraftingBag { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte PQDifficult { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte PQTier { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte PQType { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint RespawnID { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint SoundPQEnd { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint TokDiscovered { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint TokUnlocked { get; set; }

        [DataElement(AllowDbNull = false)]
        public byte Type { get; set; }
        [DataElement(AllowDbNull = false)]
        public ushort ZoneId { get; set; }
    }
}
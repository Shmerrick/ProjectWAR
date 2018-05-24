using System;

using FrameWork;

namespace Common
{

    [DataTable(PreCache = false, TableName = "zone_areas", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Zone_Area : DataObject
    {
        [PrimaryKey]
        public ushort ZoneId { get; set; }

        [DataElement]
        public string AreaName { get; set; }

        [DataElement]
        public ushort AreaId { get; set; }

        [DataElement]
        public byte Realm { get; set; }

        [PrimaryKey]
        public byte PieceId { get; set; }

        [DataElement]
        public uint OrderInfluenceId { get; set; }

        [DataElement]
        public uint DestroInfluenceId { get; set; }

        [DataElement]
        public ushort TokExploreEntry { get; set; }

        [DataElement]
        public ushort OrderRespawnId { get; set; }

        [DataElement]
        public ushort DestroRespawnId { get; set; }

        public bool IsRvR => Realm == 0;

        public override string ToString()
        {
            return AreaName + " AreaId: " + AreaId + " PieceId: " + PieceId;
        }
    }
}

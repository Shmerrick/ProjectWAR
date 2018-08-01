using FrameWork;
using System;

namespace Common.Database.World.BattleFront
{
    /// <summary>
    /// Generic object located in world whitch role depends on its type.
    /// </summary>
    [DataTable(PreCache = false, TableName = "BattleFront_objects", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class BattleFrontObject : DataObject
    {
        /// <summary>Object unique identifier.</summary>
        [PrimaryKey]
        public int Entry { get; set; }

        /// <summary>Region id, strictly positive.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort RegionId { get; set; }

        /// <summary>Zone id, strictly positive.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort ZoneId { get; set; }

        /// <summary>Zone id, strictly positive.</summary>
        [DataElement(AllowDbNull = false)]
        public ushort Type { get; set; }

        /// <summary>Owner realm, order, destro, or none ( 1 / 2 or 0).</summary>
        [DataElement(AllowDbNull = false)]
        public ushort Realm { get; set; }

        /// <summary>Objective bound to the object, 0 if none.</summary>
        [DataElement(AllowDbNull = false)]
        public int ObjectiveID { get; set; }

        /// <summary>Local X coordinate in zone</summary>
        [DataElement(AllowDbNull = false)]
        public int X { get; set; }

        /// <summary>Local Y coordinate in zone</summary>
        [DataElement(AllowDbNull = false)]
        public int Y { get; set; }

        /// <summary>Local Z coordinate in zone</summary>
        [DataElement(AllowDbNull = false)]
        public int Z { get; set; }

        /// <summary>Orientation, 0 if Type == WARCAMP_ENTRANCE</summary>
        [DataElement(AllowDbNull = false)]
        public int O { get; set; }

    }
}

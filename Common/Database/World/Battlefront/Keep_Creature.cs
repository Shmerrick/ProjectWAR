using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "keep_creatures", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Keep_Creature : DataObject, IComparable<Keep_Creature>
    {
        [DataElement(AllowDbNull = false)]
        public int KeepId { get; set; }

        [DataElement(AllowDbNull = false)]
        public ushort ZoneId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint OrderId { get; set; }

        [DataElement(AllowDbNull = false)]
        public uint DestroId { get; set; }

        [PrimaryKey]
        public int X { get; set; }

        [PrimaryKey]
        public int Y { get; set; }

        [PrimaryKey]
        public int Z { get; set; }

        [DataElement(AllowDbNull = false)]
        public int O { get; set; }

        [DataElement(AllowDbNull = false)]
        public bool KeepLord { get; set; }

		[DataElement(AllowDbNull = false)]
		public bool IsPatrol { get; set; }

		[DataElement(AllowDbNull = false)]
		public int WaypointGUID { get; set; }

		public int CompareTo(Keep_Creature other)
		{
			if (other == null) return 1;
			return WaypointGUID.CompareTo(other.WaypointGUID);
		}
	}
}

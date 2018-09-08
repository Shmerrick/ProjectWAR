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

		public Keep_Creature CreateDeepCopy()
		{
			return new Keep_Creature()
			{
				KeepId = KeepId,
				ZoneId = ZoneId,
				OrderId = OrderId + ShuffleCreatureIdOffset(3),
				DestroId = DestroId + ShuffleCreatureIdOffset(3),
				X = X + ShuffleWorldCoordinateOffset(10, 100),
				Y = Y + ShuffleWorldCoordinateOffset(10, 100),
				Z = Z + ShuffleWorldCoordinateOffset(10, 100),
				O = O,
				KeepLord = KeepLord,
				IsPatrol = IsPatrol,
				WaypointGUID = WaypointGUID
			};
		}

		/// <summary>
		/// calculates random offset in range of from to to
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static int ShuffleWorldCoordinateOffset(int from, int to)
		{
			Random rnd = new Random();
			bool sign = rnd.NextDouble() > 0.5;
			int offset = Convert.ToInt32(from + rnd.NextDouble() * 100);
			if (offset > to) offset = to;
			return sign ? offset : -offset;
		}

		/// <summary>
		/// calculates creatureID offset of maximum max
		/// </summary>
		/// <param name="max"></param>
		/// <returns></returns>
		public static uint ShuffleCreatureIdOffset(int max)
		{
			Random rnd = new Random();
			return Convert.ToUInt32(Math.Floor(rnd.NextDouble() * 3));
		}
	}
}

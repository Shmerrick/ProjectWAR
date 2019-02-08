using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FrameWork;

namespace Common
{
    [DataTable(PreCache = false, TableName = "keep_creatures", DatabaseName = "World", BindMethod = EBindingMethod.StaticBound)]
    [Serializable]
    public class Keep_Creature : DataObject
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

		public Keep_Creature CreateDeepCopy()
		{
			#region customValues for keeps

			uint orderId = 0, destroId = 0;
			bool customValues = false;
			if (KeepId == 30)
			{
				uint off = ShuffleCreatureIdOffset(2);
				if (off == 0)
				{
					orderId = 778172;
					destroId = 1000064;
				}
				else
				{
					orderId = OrderId + off;
					destroId = DestroId + off;
				}
				customValues = true;
			}
			else if (KeepId == 16 || KeepId == 15 || KeepId == 17 || KeepId == 18 || KeepId == 6)
			{
				uint off = ShuffleCreatureIdOffset(2);

				orderId = OrderId + off * 2;
				destroId = DestroId + off * 2;
				customValues = true;
			}
			else if (KeepId == 19)
			{
				uint off = ShuffleCreatureIdOffset(2);
				if (off == 2)
				{
					orderId = 777948;
					destroId = 777947;
				}
				else
				{
					orderId = OrderId + off;
					destroId = DestroId + off;
				}
				customValues = true;
			}
			else if (KeepId == 20)
			{
				uint off = ShuffleCreatureIdOffset(2);
				if (off == 2)
				{
					orderId = OrderId + 3;
					destroId = DestroId + 3;
				}
				else
				{
					orderId = OrderId + off;
					destroId = DestroId + off;
				}
				customValues = true;
			}
			else if (KeepId == 8)
			{
				uint off = ShuffleCreatureIdOffset(2);
				if (off == 2)
				{
					orderId = OrderId + off;
					destroId = DestroId + 3;
				}
				else if (off == 1)
				{
					orderId = OrderId + off;
					destroId = DestroId + 2;
				}
				else
				{
					orderId = OrderId + off;
					destroId = DestroId + off;
				}
				customValues = true;
			}
			#endregion

			return new Keep_Creature()
			{
				KeepId = KeepId,
				ZoneId = ZoneId,
				OrderId = customValues ? orderId : OrderId + ShuffleCreatureIdOffset(2),
				DestroId = customValues ? destroId : DestroId + ShuffleCreatureIdOffset(2),
				X = X + ShuffleWorldCoordinateOffset(10, 100),
				Y = Y + ShuffleWorldCoordinateOffset(10, 100),
				Z = Z,
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
			return Convert.ToUInt32(Math.Round(rnd.NextDouble() * max));
		}
	}
}

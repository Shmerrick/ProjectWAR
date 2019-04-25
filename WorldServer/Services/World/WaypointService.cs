using Common;
using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WorldServer.Services.World
{
    [Service]
    public class WaypointService : ServiceBase
    {
        public static Lookup<uint, Waypoint> LookupWaypoints;
        public static IList<Waypoint> TableWaypoints;

       [LoadingFunction(true)]
        public static void LoadNpcWaypoints()
        {
            Log.Debug("WorldMgr", "Loading Npc Waypoints...");

            TableWaypoints = Database.SelectAllObjects<Waypoint>();
            LookupWaypoints = (Lookup<uint, Waypoint>)TableWaypoints.ToLookup(W => W.GUID, W => W);

            if (TableWaypoints != null)
                Log.Success("LoadNpcWaypoints", "Loaded " + TableWaypoints.Count + " Waypoints");
        }

        public static List<Waypoint> GetNextWayPoint(uint first, List<Waypoint> matchingList)
        {
            var match = TableWaypoints.SingleOrDefault(x => x.GUID == first);
            if (match != null)
            {
                matchingList.Add(match);
                if (match.NextWaypointGUID != 0)
                    GetNextWayPoint(match.NextWaypointGUID, matchingList);
            }
            return matchingList;

        }

        public static List<Waypoint> GetNpcWaypoints(uint initialWayPoint)
        {
            //var match = TableWaypoints.SingleOrDefault(x => x.GUID == initialWayPoint);
            //if (match != null)
            //{
            //    var result = GetNextWayPoint(match.GUID, new List<Waypoint>());
            //    return result;
            //}
            //return null;

            return TableWaypoints.Where(x => x.CreatureSpawnGUID == initialWayPoint).ToList();


            //IEnumerable<Waypoint> NpcWaypoints = LookupWaypoints[WayPointUID];
            //return NpcWaypoints.ToList();
        }

        public static List<Waypoint> GetKeepNpcWaypoints(int infoWaypointGuid)
        {
            return TableWaypoints.Where(x => x.GameObjectSpawnGUID == infoWaypointGuid).ToList();
        }

        public static void DatabaseAddWaypoint(Waypoint AddWp)
        {
            Database.AddObject(AddWp);
            Database.ForceSave();
        }

        public static void DatabaseSaveWaypoint(Waypoint SaveWp)
        {
            Database.SaveObject(SaveWp);
            Database.ForceSave();
        }

        public static void DatabaseDeleteWaypoint(Waypoint DeleteWp)
        {
            Database.DeleteObject(DeleteWp);
            Database.ForceSave();
        }

		/// <summary>
		/// calculates waypoint offset in range of from to to
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		public static int ShuffleWaypointOffset(int from, int to)
		{
			Random rnd = new Random();
			bool sign = rnd.NextDouble() > 0.5;
			int offset = Convert.ToInt32(from + rnd.NextDouble() * 100);
			if (offset > to) offset = to;
			return sign ? offset : -offset;
		}
	}
}

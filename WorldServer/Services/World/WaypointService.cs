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

        [LoadingFunction(true)]
        public static void LoadNpcWaypoints()
        {
            Log.Debug("WorldMgr", "Loading Npc Waypoints...");

            IList<Waypoint> TableWaypoints = Database.SelectAllObjects<Waypoint>();
            LookupWaypoints = (Lookup<uint, Waypoint>)TableWaypoints.ToLookup(W => W.CreatureSpawnGUID, W => W);

            if (TableWaypoints != null)
                Log.Success("LoadNpcWaypoints", "Loaded " + TableWaypoints.Count + " Waypoints");
        }

        public static List<Waypoint> GetNpcWaypoints(uint CreatureSpawnGuid)
        {
            IEnumerable<Waypoint> NpcWaypoints = LookupWaypoints[CreatureSpawnGuid];
            return NpcWaypoints.ToList();
        }

        public static void DatabaseAddWaypoint(Waypoint AddWp)
        {
            Database.AddObject(AddWp);
        }

        public static void DatabaseSaveWaypoint(Waypoint SaveWp)
        {
            Database.SaveObject(SaveWp);
        }

        public static void DatabaseDeleteWaypoint(Waypoint DeleteWp)
        {
            Database.DeleteObject(DeleteWp);
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

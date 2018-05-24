using Common;
using FrameWork;
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

    }
}

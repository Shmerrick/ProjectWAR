using Common;
using FrameWork;
using System.Collections.Generic;

namespace WorldServer.Services.World
{
    [Service]
    public class RallyPointService : ServiceBase
    {
        public static List<RallyPoint> RallyPoints;

        [LoadingFunction(true)]
        public static void LoadRallyPoints()
        {
            Log.Debug("WorldMgr", "Loading RallyPoints...");

            RallyPoints = Database.SelectAllObjects<RallyPoint>() as List<RallyPoint>;

            Log.Success("RallyPoint", "Loaded " + RallyPoints.Count + " RallyPoints");
        }

        public static RallyPoint GetRallyPoint(uint Id)
        {
            foreach (RallyPoint point in RallyPoints)
                if (point.Id == Id)
                    return point;
            return null;
        }

        public static RallyPoint GetRallyPointFromNPC(uint CreatureId)
        {
            foreach (RallyPoint point in RallyPoints)
                if (point.CreatureId == CreatureId)
                    return point;
            return null;
        }

    }
}

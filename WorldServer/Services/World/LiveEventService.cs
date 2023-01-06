using Common.Database.World.LiveEvents;
using FrameWork;
using System.Collections.Generic;
using System.Linq;

namespace WorldServer.Services.World
{
    [Service]
    internal class LiveEventService : ServiceBase
    {
        public static List<LiveEvent_Info> LiveEvents = new List<LiveEvent_Info>();

        [LoadingFunction(true)]
        public static void LoadLiveEvents()
        {
            Log.Debug("WorldMgr", "Loading  LiveEvent_Info...");

            var liveEvents = Database.SelectAllObjects<LiveEvent_Info>().ToDictionary(e => e.Entry, e => e);

            var rewards = Database.SelectAllObjects<LiveEventReward_Info>();
            var tasks = Database.SelectAllObjects<LiveEventTasks_Info>().ToDictionary(e => e.Entry, e => e);
            var subTasks = Database.SelectAllObjects<LiveEventSubTasks_Info>();

            foreach (var reward in rewards)
            {
                if (liveEvents.ContainsKey(reward.LiveEventId))
                    liveEvents[reward.LiveEventId].Rewards.Add(reward);
            }

            foreach (var task in tasks.Values)
            {
                if (liveEvents.ContainsKey(task.LiveEventId))
                    liveEvents[task.LiveEventId].Tasks.Add(task);
            }

            foreach (var task in subTasks)
            {
                if (tasks.ContainsKey(task.LiveEventTaskId))
                    tasks[task.LiveEventTaskId].Tasks.Add(task);
            }

            LiveEvents = liveEvents.Values.ToList();

            Log.Success("LiveEvent_Info", "Loaded " + LiveEvents.Count + " LiveEvent_Info");
        }
    }
}
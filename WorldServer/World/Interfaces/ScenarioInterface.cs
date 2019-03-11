using FrameWork;
using WorldServer.World.Objects;
using WorldServer.World.Scenarios;

namespace WorldServer.World.Interfaces
{
    public class ScenarioInterface : BaseInterface
    {
        public Scenario PendingScenario;

        public bool BlockQueue;

        public bool PendingQueuePop;

        public bool HasPendingScenario => PendingQueuePop || PendingScenario != null;

        public Scenario Scenario;
        public int ScenarioEntryZoneId;
        public int ScenarioEntryWorldX;
        public int ScenarioEntryWorldY;
        public int ScenarioEntryWorldZ;
        public int ScenarioEntryWorldO;

        public void ClearPendingScenario()
        {
            PendingScenario = null;
            PendingQueuePop = false;
        }

        public bool ValidForScenario()
        {
            Player owner = (Player) _Owner;

            return owner.IsInWorld() && !owner.PendingDisposal && !owner.IsDisposed && owner.Faction != 64 && (owner.WorldGroup == null || !owner.WorldGroup.IsWarband);
        }

        private int _nextMessageTime;

        public bool IsBlocked()
        {
            if (!BlockQueue)
                return false;

            int time = TCPManager.GetTimeStamp();

            if (time > _nextMessageTime)
            {
                ((Player)_Owner).SendClientMessage("You have recently left a scenario or refused a scenario join request, and have been temporarily prevented from queueing.");
                _nextMessageTime = time + 5;
            }

            return true;
        }
    }
}
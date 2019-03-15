using SystemData;
using Common.Database.World.BattleFront;
using FrameWork;
using WorldServer.NetWork.Handler;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Battlefronts.Objectives
{
    /// <summary>
    /// Game object representing a portal allowing teleport to the gatehouse
    /// </summary>
    class PortalToGatehouse : PortalBase
    { 
        private const string NAME_START = "Portal to gatehouse ";

        /// <summary>Portal targets depending on realm</summary>
        private PortalBase targetPortal;
        private Point3D targetPosition;

        private long _nextAvailableTimestamp;

        public PortalToGatehouse(int zoneId, int x1, int y1, int z1, int o1, int x2, int y2, int z2, int o2, string name)
            : base(zoneId, x1, y1, z1, o1)
        {
            Name = NAME_START + name;
            Spawn.Proto.Name = Name; // For debug purpose only

            //var target = new PortalBase(zoneId, x2, y2, z2, o2);

            //targetPortal = target;
            targetPosition = new Point3D(x2, y2,z2);
        }


        public override void SendInteract(Player player, InteractMenu menu)
        {
            long now = TCPManager.GetTimeStampMS();
            if (now < _nextAvailableTimestamp)
            {
                player.SendClientMessage("This portal is still unstable. Please wait a moment before trying again.", ChatLogFilters.CHATLOGFILTERS_SAY);
                return;
            }

            _nextAvailableTimestamp = now + BattleFrontConstants.PORTAL_DELAY;

            Teleport(player, (int) targetPortal.ZoneId, targetPosition);
        }
    }
}

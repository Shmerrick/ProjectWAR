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
    /// Game object representing a portal around an objective
    /// allowing port to warcamp.
    /// </summary>
    class PortalToObjective : PortalBase
    {
        private const string NAME_START = "Portal to ";

        /// <summary>Portal targets depending on realm</summary>
        private BattleFrontObject _target;
        private Point3D _targetPos;

        private long _nextAvailableTimestamp;

        public PortalToObjective(BattleFrontObject origin, BattleFrontObject target, string name)
            : base(origin)
        {
            Name = NAME_START + name;
            Spawn.Proto.Name = Name; // For debug purpose only

            _target = target;
            _targetPos = GetWorldPosition(target);
        }

        public PortalToObjective(PortalToObjective other)
            : base(other.Spawn)
        {
            Name = other.Name;
            _target = other._target;
            _targetPos = other._targetPos;
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

            Teleport(player, _target.ZoneId, _targetPos);
        }
    }
}

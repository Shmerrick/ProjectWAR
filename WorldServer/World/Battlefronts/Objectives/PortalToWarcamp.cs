using SystemData;
using Common.Database.World.BattleFront;
using GameData;
using WorldServer.NetWork.Handler;
using WorldServer.World.Objects;
using WorldServer.World.Positions;

namespace WorldServer.World.Battlefronts.Objectives
{
    /// <summary>
    /// Game object representing a portal around an objective
    /// allowing port to warcamp.
    /// </summary>
    class PortalToWarcamp : PortalBase
    {
        private const string NAME = "Portal to warcamp";

        /// <summary>Portal targets depending on realm</summary>
        private BattleFrontObject _orderTarget, _destroTarget;
        private Point3D _orderTargetPos, _destroTargetPos;

        public PortalToWarcamp(
            BattleFrontObject origin,
            BattleFrontObject orderTarget, BattleFrontObject destroTarget)
            : base(origin)
        {
            Name = NAME;
            Spawn.Proto.Name = NAME; // For debug purpose only

            _orderTarget = orderTarget;
            _destroTarget = destroTarget;

            _orderTargetPos = GetWorldPosition(orderTarget);
            _destroTargetPos = GetWorldPosition(destroTarget);
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (player.CbtInterface.IsInCombat)
            {
                player.SendClientMessage("Can't use this portal while in combat.", ChatLogFilters.CHATLOGFILTERS_SAY);
                return;
            }

            // Gets the port target
            BattleFrontObject target;
            Point3D targetPos;
            if (player.Realm == Realms.REALMS_REALM_ORDER)
            {
                target = _orderTarget;
                targetPos = _orderTargetPos;
            }
            else
            {
                target = _destroTarget;
                targetPos = _destroTargetPos;
            }

            Teleport(player, target.ZoneId, targetPos);
        }

    }
}

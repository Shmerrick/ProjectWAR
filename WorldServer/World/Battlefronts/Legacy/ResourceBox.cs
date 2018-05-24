using SystemData;
using GameData;
using WorldServer.Scenarios.Objects;

namespace WorldServer
{ 
    class ResourceBox : HoldObject
    {
        // Codeword p0tat0 This is for DoomsDay event and cannot be done by switch, default type here was BattlefrontFlag, not ProximityFlag
        //public BattlefrontFlag Objective;
        public World.Battlefronts.Objectives.ProximityFlag Objective;

        public ResourceBox(uint identifier, string name, Point3D homePosition, ushort buffId, int groundResetTime, InteractAction onPickupAction, BallAction onDropAction, BallAction onResetAction, BuffQueueInfo.BuffCallbackDelegate onBuffCallback, ushort groundModelId, ushort homeModelId) 
            : base(identifier, name, homePosition, buffId, groundResetTime, onPickupAction, onDropAction, onResetAction, onBuffCallback, groundModelId, homeModelId)
        {
            HoldResetTimeSeconds = 360;
        }

        public override void SendInteract(Player player, InteractMenu menu)
        {
            if (!player.ValidInTier(Region.GetTier(), true))
            {
                player.SendClientMessage("You are too low level to interact with resources here", ChatLogFilters.CHATLOGFILTERS_C_ABILITY_ERROR);
                return;
            }

            base.SendInteract(player, menu);
        }

        public override void ResetFromGround()
        {
            X = Zone.CalculPin((uint)HomePosition.X, true);
            Y = Zone.CalculPin((uint)HomePosition.Y, false);
            Z = HomePosition.Z;

            OnLoad();

            WorldPosition.X = HomePosition.X;
            WorldPosition.Y = HomePosition.Y;
            WorldPosition.Z = HomePosition.Z;

            SetOffset((ushort)(HomePosition.X >> 12), (ushort)(HomePosition.Y >> 12));

            SetHeldState(EHeldState.Inactive);

            foreach (Player plr in Region.Players)
                plr.SendClientMessage($"The supplies from {Objective.ObjectiveName} have decayed. New supplies will be sent.", Objective.OwningRealm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);

            OnResetAction?.Invoke(this);
        }

        public override void ResetFromHeld()
        {
            Holder?.SendClientMessage("You have held the supplies for too long and they have decayed.");
            Holder?.SendClientMessage("You have held the supplies for too long and they have decayed.", ChatLogFilters.CHATLOGFILTERS_C_WHITE);

            foreach (Player plr in Region.Players)
                plr.SendClientMessage($"The supplies from {Objective.ObjectiveName} have decayed. New supplies will be sent.", Objective.OwningRealm == Realms.REALMS_REALM_ORDER ? ChatLogFilters.CHATLOGFILTERS_C_ORDER_RVR_MESSAGE : ChatLogFilters.CHATLOGFILTERS_C_DESTRUCTION_RVR_MESSAGE);

            ResetTo(EHeldState.Inactive);
        }
    }
}

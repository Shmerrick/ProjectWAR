using FrameWork;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class GroupHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_GROUP_COMMAND, (int)eClientState.WorldEnter, "onGroupCommand")]
        public static void F_GROUP_COMMAND(BaseClient client, PacketIn packet)
        {
            GameClient cclient = (GameClient)client;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            Player player = cclient.Plr;
            Group worldGroup = cclient.Plr.WorldGroup;

            uint groupId = 0;
            bool isLeader = false;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                    isLeader = worldGroup._warbandHandler.Leader == player;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                            isLeader = worldGroup.Leader == player;
                        }
                    }
                }
            }
            packet.Skip(3); // unk
            byte subGroup = packet.GetUint8();
            byte state = packet.GetUint8();


            switch (state)
            {
                case 2: // Accept invitation
                    player.GrpInterface.AcceptInvitation();
                    break;
                case 6: // Decline invitation
                    player.GrpInterface.RejectInvitation();
                    break;
                case 3: // Leave group
                    if (groupId != 0)
                        Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.PlayerLeave, player));
                    break;
                case 4: // loot roundrobin
                    if (groupId != 0 && isLeader)
                        Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.ChangeLootOption, player, subGroup.ToString()));
                    break;
                case 5: // Set master looter
                    Player newMasterLooter;
                    lock (Player._Players)
                        Player.PlayersByCharId.TryGetValue(subGroup, out newMasterLooter);
                    if (newMasterLooter != null)
                        if (groupId != 0 && isLeader)
                            Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.ChangeMasterLooter, player, newMasterLooter.Name));
                    break;
                case 10: // switch leader
                    Player newLeader;
                    lock (Player._Players)
                        Player.PlayersByCharId.TryGetValue(subGroup, out newLeader);
                    if (newLeader != null)
                        if (groupId != 0 && isLeader)
                            Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.ChangeLeader, cclient.Plr, newLeader.Name));
                    break;
                case 12:
                    if (groupId != 0 && isLeader)
                        Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.ChangeNeedOnUse, player));
                    break;
                case 13:
                    player.ScnInterface.Scenario.AddPlayerToGroup(player, subGroup);
                    break;
                case 14:
                    player.ScnInterface.Scenario.RemovePlayerFromGroup(player);
                    break;
                case 15: // Warband invitation acceptance
                    player.GrpInterface.AcceptInvitation();
                    break;
                case 16: // Warband invitation rejection
                    player.GrpInterface.RejectInvitation();
                    break;
                case 17: // Make set mainassist
                    Player newMainAssist;
                    lock (Player._Players)
                        Player.PlayersByCharId.TryGetValue(subGroup, out newMainAssist);
                    if (newMainAssist != null)
                        if (groupId != 0 && isLeader)
                            Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.ChangeMainAssist, player, newMainAssist.Name));
                    break;
                case 18: // autolootinrvr
                    if (groupId != 0)
                        Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.ChangeAutoLoot, player));
                    break;
                default:
                    Log.Error("GroupHandler", "Unsupported type: " + state);
                    break;
            }
        }
    }
}

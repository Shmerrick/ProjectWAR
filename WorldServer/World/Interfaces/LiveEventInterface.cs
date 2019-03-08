using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorldServer.Services.World;

namespace WorldServer
{
    public class LiveEventInterface : BaseInterface
    {
        Player _myPlayer;

        public override void SetOwner(Object owner)
        {
            _Owner = owner;
            _myPlayer = owner as Player;
        }

        public void SendLiveEvents()
        {
            var activeEvents = LiveEventService.LiveEvents.Where(e => e.Allowed).ToList();

            if (activeEvents.Count > 0)
            {
                PacketOut packet = new PacketOut((byte)Opcodes.F_BAG_INFO);
                packet.WriteByte(10);

                foreach (var liveEvent in activeEvents)
                {
                    packet.WriteUInt32R(liveEvent.Entry);

                    packet.WriteVarIntString(liveEvent.Title);
                    packet.WriteVarIntString(liveEvent.SubTitle);
                    packet.WriteVarIntString(liveEvent.Description);
                    packet.WriteVarIntString(liveEvent.TasksDescription);

                    packet.WriteUInt32R(liveEvent.ImageId);
                    packet.WriteUInt32R(0); //unk2
                    packet.WriteUInt32R(0); //unk3
                    packet.WriteByte(1); //is player allowed to participate
                    packet.WriteUInt32R(0); //progress

                    var rewardGroups = liveEvent.Rewards.GroupBy(e => e.RewardGroupId).ToList();

                    packet.WriteByte((byte)rewardGroups.Count); //write reward group count

                    for (int rewardGroup = 0; rewardGroup < rewardGroups.Count; rewardGroup++)
                    {
                        var group = rewardGroups[rewardGroup];

                        packet.WriteUInt32R(group.Key);

                        var pos = packet.Position;
                        packet.WriteUInt32R(0); //items length


                        packet.WriteUInt32R(0); //unk5
                        packet.WriteUInt32R(0); //unk6
                        packet.WriteByte(1); //unk7
                        packet.WriteByte(0); //unk8
                        packet.WriteByte((byte)group.ToList().Count);

                        foreach (var item in group)
                        {
                            Item.BuildItem(ref packet, null, ItemService.GetItem_Info(item.ItemId), null, 0, 0);
                        }

                        var endPos = packet.Position;
                        packet.Position = pos;
                        packet.WriteUInt32R((uint)((endPos - pos) - 4));
                        packet.Position = endPos;
                    }

                    packet.WriteByte((byte)liveEvent.Tasks.Count);
                    foreach (var task in liveEvent.Tasks)
                    {
                        packet.WriteUInt32R(task.Entry);
                        packet.WriteVarIntString(task.Name);
                        packet.WriteVarIntString(task.Description);
                        packet.WriteUInt32R(0); //unk9
                        packet.WriteUInt32R(0); //unk10
                        packet.WriteUInt32R(0); //completed count by player
                        packet.WriteUInt32R((uint)task.TotalTasks); //total
                        packet.WriteUInt32R(0); //unk11
                        packet.WriteByte(0); //unk12

                        packet.WriteByte((byte)task.Tasks.Count);
                        foreach (var subTask in task.Tasks)
                        {
                            packet.WriteUInt32R(subTask.Entry);
                            packet.WriteVarIntString(subTask.Name);
                            packet.WriteUInt32R(0); //unk13
                            packet.WriteByte(0); //unk14
                            packet.WriteUInt32R(0); //unk15
                            packet.WriteUInt32R(0); //completed count by player
                            packet.WriteUInt32R(subTask.TaskCount); //total
                            packet.WriteUInt32R(0); //unk16
                            packet.WriteUInt16(0); //unk17
                        }
                    }
                    packet.WriteUInt32R(1); //show popup
                    _myPlayer.SendPacket(packet);
                }
            }

        }
    }
}
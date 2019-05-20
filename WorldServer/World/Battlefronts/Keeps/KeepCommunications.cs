using FrameWork;
using GameData;
using NLog;
using System.Linq;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Battlefronts.Keeps
{
    public class KeepCommunications : IKeepCommunications
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void SendKeepStatus(Player plr, BattleFrontKeep keep)
        {
            if (keep.Region == null)
                return;

            //var doors = keep.Doors.FindAll(x =>
            //    x.Info.Number != (int) KeepDoorType.None && x.Info.GameObjectId == 100 && x.GameObject.PctHealth > 0);

            var doors = keep.Doors.FindAll(x => x.Info.GameObjectId == 100);

            var innerDoor = keep.Doors.SingleOrDefault(x => x.Info.Number == (int)KeepDoorType.InnerMain);

            var Out = new PacketOut((byte)Opcodes.F_KEEP_STATUS, 26);
            Out.WriteByte(keep.Info.KeepId);
            {
                Out.WriteByte(keep.KeepStatus == KeepStatus.KEEPSTATUS_LOCKED ? (byte)1 : (byte)keep.KeepStatus);
                Out.WriteByte(0); // ?
                Out.WriteByte((byte)keep.Realm);
                Out.WriteByte((byte)doors.Count);
                Out.WriteByte(keep.Rank); // Rank
                if (doors.Count > 0)
                    if (innerDoor != null)
                        Out.WriteByte((byte)((innerDoor.GameObject.PctHealth))); // Door health
                    else
                    {
                        Out.WriteByte(0);
                    }
                else
                    Out.WriteByte(0);
                Out.WriteByte(0); // Next rank %
            }

            Out.Fill(0, 18);

            if (plr != null)
                plr.SendPacket(Out);
            else
                lock (Player._Players)
                {
                    foreach (var player in Player._Players)
                        player.SendCopy(Out);
                }

            _logger.Trace($"F_KEEP_STATUS {keep.Info.Name} Status : {keep.KeepStatus} ");
        }

    }


}

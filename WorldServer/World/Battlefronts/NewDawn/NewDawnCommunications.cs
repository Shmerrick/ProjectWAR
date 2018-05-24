using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameWork;
using GameData;

namespace WorldServer.World.Battlefronts.NewDawn
{
    public class NewDawnCommunications
    {
        public void SendFlagLeft(Player plr, int id)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_UPDATE, 8);

            Out.WriteUInt32((uint)id);
            Out.WriteUInt32(0);
            plr.SendPacket(Out);
        }

        
    }
}

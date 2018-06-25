using FrameWork;

namespace WorldServer.World.Battlefronts.Apocalypse
{
    public class ApocCommunications
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

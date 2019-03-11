using FrameWork;

namespace WorldServer.NetWork.Handler
{
    public class Unk1 : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_UNK1, (int)eClientState.WorldEnter, "onUnk1")]
        public static void F_UNK1(BaseClient client, PacketIn packet)
        {

        }
    }
}

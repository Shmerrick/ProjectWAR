using Common;
using FrameWork;
using WorldServer.Managers;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;

namespace WorldServer.NetWork.Handler
{
    public class MailHandlers : IPacketHandler
    {
        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_MAIL, (int)eClientState.Playing, "onMail")]
        public static void F_MAIL(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;

            if (!cclient.IsPlaying() || !cclient.Plr.IsInWorld())
                return;

            Player Plr = cclient.Plr;

            Plr.MlInterface.CheckMailExpired();

            byte Type = packet.GetUint8();

            switch (Type)
            {
                case 0: // Mailbox closed
                    {
                        if (Plr.GldInterface.IsInGuild() && Plr.GldInterface.Guild.GuildVaultUser.Contains(Plr))
                            Plr.GldInterface.Guild.GuildVaultClosed(Plr);

                        if (Plr.CurrentSiege != null && Plr.CurrentSiege.SiegeInterface.IsDeployed)
                            Plr.CurrentSiege.SiegeInterface.RemovePlayer(Plr);


                    } break;
                case 1: // Mail sent
                    {
                        Plr.MlInterface.SendPacketMail(packet);
                    } break;
                case 2: // Open mail
                case 3: // Return mail
                case 4: // Delete mail
                case 5: // Mark as read/unread
                case 7: // Take Item
                case 8: // Take money
                    {
                        byte page = packet.GetUint8();
                        uint guid = ByteOperations.ByteSwap.Swap(packet.GetUint32());

                        Plr.MlInterface.MailInteract((MailInteractType)Type, guid, packet);
                    }
                    break;
            }
        }

        public static void SendCOD(string receiverName, Player sender, uint amount)
        {
            Character receiver = CharMgr.GetCharacter(receiverName, false);

            // Sender may have deleted character, oh well. No one gets the money.
            if (receiver == null)
                return;

            sender.MlInterface.SendMail(receiver, "COD Payment", "Here is your COD payment", amount, false);
        }
    }
}

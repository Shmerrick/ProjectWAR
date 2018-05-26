using System.Collections.Generic;
using FrameWork;

namespace AuthenticationServer.Server
{
    public class TCPServer : TCPManager
    {
        public bool AllowPatching = true;

        protected override BaseClient GetNewClient()
        {
            Client client = new Client(this);

            return client;
        }

        public void DispatchPatcket(PacketOut packet)
        {
            foreach (var c in GetClients<Client>())
            {
                c.SendTCPRaw(packet);
            }
        }

        public static List<string> LoadAddons()
        {
            return null;
        }
    }
}

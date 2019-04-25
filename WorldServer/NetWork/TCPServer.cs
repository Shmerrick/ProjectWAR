using FrameWork;

namespace WorldServer.NetWork
{
    public class TCPServer : TCPManager
    {
        public TCPServer()
        {
            PacketOut.SizeLen       = sizeof(ushort);
            PacketOut.OpcodeInLen   = false;
            PacketOut.SizeInLen     = false;
            PacketOut.OpcodeReverse = false;
            PacketOut.SizeReverse   = false;
            PacketOut.Struct        = PackStruct.SizeAndOpcode;
        }

        protected override BaseClient GetNewClient()
        {
            GameClient client = new GameClient(this);

            return client;
        }

        public GameClient GetClientByAccount(GameClient Me,int AccountId)
        {
            lock(Clients)
                for(int i=0;i<Clients.Length;++i)
                    if (Clients[i] != null && Clients[i] != Me)
                    {
                        GameClient client = Clients[i] as GameClient;
                        if (client.HasAccount() && client._Account.AccountId == AccountId)
                            return client;
                    }

            return null;
        }

        /*
        public Player GetPlayerByName(string Name)
        {
            lock(Clients)
                for(int i=0;i<Clients.Length;++i)
                    if (Clients[i] != null)
                    {
                        GameClient client = (Clients[i] as GameClient);
                        if (client.IsPlaying() && client.HasPlayer() && client.Plr.Name.ToLower() == Name.ToLower())
                            return client.Plr;
                    }

            return null;
        }
        */
    }
}

using FrameWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WorldServer.API
{
    public class Server
    {
        private Dictionary<int, Client> _clients = new Dictionary<int, Client>();

        public Dictionary<int, Client> Clients
        {
            get { return _clients; }
            set { _clients = value; }
        }
        private IPEndPoint _endPoint;
        private string _name = "";

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private bool _online;
        private AsyncCallback _asyncAcceptCallback;
        private int _port;
        private string _address;
        private TcpListener _listener;
        private readonly int _bufferSize;
        private Protocol _proto = new Protocol();
        public Server(string address, int port, int maxConnections, int bufferSize = 0xFFFF)
        {
            _address = address;
            _asyncAcceptCallback = new AsyncCallback(BeginAccept);
            _port = port;
            _endPoint = new IPEndPoint(IPAddress.Parse(address), _port);
            Start();

        }

        public void Start()
        {
            _online = true;
            _listener = new TcpListener(_port);
            _listener.Server.ReceiveBufferSize = _bufferSize;
            _listener.Server.SendBufferSize = _bufferSize;
            _listener.Server.NoDelay = true;
            _listener.Server.Blocking = true;
            _listener.Start();
            _listener.BeginAcceptTcpClient(BeginAccept, this);
            Log.Success("API", "WorldServer API started " + _address + ":" + _port);
        }


        public virtual void DeleteClient(Client client)
        {
            lock (_clients)
            {
                if (_clients.ContainsKey(client.ID))
                    _clients.Remove(client.ID);
            }
            client.Socket.Disconnect(false);
            _proto.RemoveClient(client);
        }

        public void OnFrame(Client client, ApiPacket frame)
        {
            _proto.OnFrame(client, frame);
        }

        private void BeginAccept(IAsyncResult ar)
        {
            try
            {
                if (!_online)
                    return;
                Socket acceptSocket = _listener.EndAcceptSocket(ar);
                acceptSocket.SendBufferSize = _bufferSize;
                acceptSocket.ReceiveBufferSize = _bufferSize;
                acceptSocket.NoDelay = true;
                acceptSocket.Blocking = false;
                Client client = new Client(this);
                client.ID = client.GetHashCode();


                lock (_clients)
                {
                    _clients[client.ID] = client;
                }
                try
                {

                    client.Socket = acceptSocket;

                    client.Receive();

                }
                catch (Exception e)
                {
                    DeleteClient(client);
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                if (_listener != null && _online)
                {
                    _listener.BeginAcceptSocket(_asyncAcceptCallback, this);
                }
            }
        }
    }
}

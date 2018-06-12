using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.Reflection;
using System.Linq;

namespace FrameWork
{
    public class TCPManager
    {
        #region Manager

        private static Dictionary<string, TCPManager> _Tcps = new Dictionary<string, TCPManager>();
        private static TCPManager ConvertTcp<T>() where T : TCPManager, new()
        {
            if (!typeof(T).IsSubclassOf(typeof(TCPManager)))
                return null;

            T Tcp = new T();
            TCPManager TTcp = Tcp;

            return TTcp;
        }
        public static bool Listen<T>(int port, string Name) where T : TCPManager, new()
        {
            try
            {
                if (Name.Length <= 0)
                    return false;

                if (port <= 0)
                    return false;

                if (_Tcps.ContainsKey(Name))
                    return false;

                TCPManager Tcp = ConvertTcp<T>();

                if (Tcp == null || !Tcp.Start(port))
                {
                    Log.Error(Name, "Can not start server on port : " + port);
                    return false;
                }

                _Tcps.Add(Name, Tcp);
            }
            catch (Exception e)
            {
                Log.Error("Listen", "Error : " + e);
                return false;
            }

            return true;
        }
        public static bool Connect<T>(string IP, int port, string Name) where T : TCPManager, new()
        {
            if (Name.Length <= 0)
                return false;

            if (port <= 0)
                return false;

            if (_Tcps.ContainsKey(Name))
                return false;

            TCPManager Tcp = ConvertTcp<T>();
            if (Tcp == null || Tcp.Start(IP, port))
            {
                Log.Error(Name, "Can not connect to : " + IP + ":" + port);
                return false;
            }

            _Tcps.Add(Name, Tcp);

            return true;
        }
        public static T GetTcp<T>(string Name)
        {
            if (_Tcps.ContainsKey(Name))
                return (T)Convert.ChangeType(_Tcps[Name], typeof(T));

            return (T)Convert.ChangeType(null, typeof(T));
        }

        #endregion

        private TcpListener Listener;
        private Socket Client;
        private readonly AsyncCallback _asyncAcceptCallback;

        private bool LoadCryptHandlers = true;
        private bool LoadPacketHandlers = true;

        public readonly PacketFunction[] m_packetHandlers = new PacketFunction[0xFF];
        private readonly byte[] stateRequirement = new byte[0xFF];

        public readonly Dictionary<string,ICryptHandler> m_cryptHandlers = new Dictionary<string,ICryptHandler>();
        private static readonly DateTime EpochDateTime = new DateTime(1970, 1, 1);
        #region Clients

        // Liste des clients connectés
        public static int MAX_CLIENT = 65000;

        private ReaderWriterLockSlim ClientRWLock = new ReaderWriterLockSlim();
        public BaseClient[] Clients = new BaseClient[MAX_CLIENT];
        public int GetClientCount()
        {
            int Count = 0;

            LockReadClients();

            for (int i = 0; i < Clients.Length; ++i)
                if (Clients[i] != null)
                    ++Count;

            UnLockReadClients();

            return Count;
        }

        public void GenerateId(BaseClient Client)
        {
            LockWriteClients();

            for (int i = 10; i < Clients.Length; ++i)
            {
                if (Clients[i] == null)
                {
                    Client.Id = i;
                    Clients[i] = Client;
                    break;
                }
            }

            UnLockWriteClients();
        }

        public void RemoveClient(BaseClient Client)
        {
            LockWriteClients();

            if (Clients[Client.Id] == Client)
                Clients[Client.Id] = null;

            UnLockWriteClients();
        }
        public void RemoveClient(int Id)
        {
            LockWriteClients();

                if (Id >= 0 && Id < Clients.Length)
                    Clients[Id] = null;

           UnLockWriteClients();
        }

        public BaseClient GetClient(int Id)
        {
            LockReadClients();

            if (Id >= 0 && Id < Clients.Length)
                return Clients[Id];

            UnLockReadClients();

            return null;
        }

        public void LockReadClients()
        {
            ClientRWLock.EnterReadLock();
        }
        public void UnLockReadClients()
        {
            ClientRWLock.ExitReadLock();
        }
        public void LockWriteClients()
        {
            ClientRWLock.EnterWriteLock();
        }
        public void UnLockWriteClients()
        {
            ClientRWLock.ExitWriteLock();
        }

        #endregion

        public TCPManager()
        {
            _asyncAcceptCallback = new AsyncCallback(ConnectingThread);
        }

        public static int GetTimeStamp()
        {
            return (int)(DateTime.UtcNow - EpochDateTime).TotalSeconds;
        }

        public static long GetTimeStampMS()
        {
            return (long)(DateTime.UtcNow - EpochDateTime).TotalMilliseconds;
        }

        public List<T> GetClients<T>() where T : BaseClient
        {
            return Clients.Where(e => e != null && e.Socket != null && e.Socket.Connected).Select(e => (T)e).ToList();
        }

        private bool InitSocket(int port)
        {
            try
            {
                Listener = new TcpListener(IPAddress.Any, port);
                Listener.Server.ReceiveBufferSize = BUF_SIZE;
                Listener.Server.SendBufferSize = BUF_SIZE;
                Listener.Server.NoDelay = false;
                Listener.Server.Blocking = false;

                AllocatePacketBuffers();
            }
            catch (Exception e)
            {
                Log.Error("InitSocket", e.ToString());
                return false;
            }

            return true;
        }

        private bool InitSocket(string Ip,int port)
        {
            try
            {
                IPHostEntry LSHOST = Dns.GetHostEntry(Ip);

                IPEndPoint EndPoint = new IPEndPoint(LSHOST.AddressList[0], port);

                Log.Error("InitSocket", $"{LSHOST.AddressList[0]}, {EndPoint.Address.ToString()}, {port}");

                Client = new Socket(EndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                Client.Connect(EndPoint);

                if (Client.Connected)
                    ConnectingThread((IAsyncResult)Client);

                AllocatePacketBuffers();
            }
            catch (Exception e)
            {
                Log.Error("InitSocket", e.ToString());
                return false;
            }

            return true;
        }

        public virtual bool Start(string IpToConnect, int port)
        {
            Log.Info("TCPManager", "Starting...");

            if (!InitSocket(IpToConnect,port))
                return false;

            try
            {
                if(LoadPacketHandlers)
                    LoadPacketHandler();

                if(LoadCryptHandlers)
                    LoadCryptHandler();

                Client.Connect(IpToConnect, port);

                if (!Client.Connected)
                    return false;

                Log.Success("TCPManager", "Client connected at : " + IpToConnect+":"+port);
            }
            catch (SocketException e)
            {
                Log.Error("TCPManager", e.ToString());
                return false;
            }


            return true;
        }

        // Start Socket and threads
        public virtual bool Start(int port)
        {
            Log.Info("TCPManager", $"Starting...{port}");

            if (!InitSocket(port))
                return false;

            try
            {
                LoadPacketHandler();
                LoadCryptHandler();

                Listener.Start();
                Listener.BeginAcceptTcpClient(ConnectingThread, this);

                Log.Success("TCPManager", "Server listening to : " + Listener.LocalEndpoint);
            }
            catch (SocketException e)
            {
                Log.Error("TCPManager", e.ToString());
                return false;
            }


            return true;
        }

        // Stop all threads and incomming connections
        public virtual void Stop()
        {
            Log.Debug("TCPManager", "TCP Manager shutdown [" + Listener.LocalEndpoint + "]");

            try
            {
                if (Listener != null)
                {
                    Listener.Stop();

                    Log.Debug("TCPManager", "Stop incoming connections");
                }
            }
            catch (Exception e)
            {
               Log.Error("TCPManager", e.ToString());
            }

            lock (Clients.SyncRoot)
            {
                for (int i = 0; i < Clients.Length; ++i)
                {
                    if (Clients[i] != null)
                    {
                        Clients[i].CloseConnections();
                        Clients[i] = null;
                    }
                }
            }
        }

        // Unused
        public void SendToAll(PacketOut Packet)
        {
            if (!Packet.Finalized)
                Packet.WritePacketLength();

            LockReadClients();

            for (int i = 0; i < Clients.Length; ++i)
            {
                if (Clients[i] != null)
                    Clients[i].SendAsynchronousTCP(Packet.ToArray());
            }

            UnLockReadClients();
        }

        #region Packet buffer pool

        // Taille maximal des packets
        public int BUF_SIZE = 65536; //131072;

        // Taille minimal du buffer pool
        public int POOL_SIZE = 1000;

        // Liste des packets
        private Queue<byte[]> m_packetBufPool;

        public int PacketPoolSize
        {
            get { return m_packetBufPool.Count; }
        }

        //  Allocation d'un nouveau packet
        private void AllocatePacketBuffers()
        {
            m_packetBufPool = new Queue<byte[]>(POOL_SIZE);
            for (int i = 0; i < POOL_SIZE; i++)
            {
                m_packetBufPool.Enqueue(new byte[BUF_SIZE]);
            }

            Log.Debug("TCPManager", "Allocation of the buffer pool : " + POOL_SIZE);
        }

        // Demande d'un nouveau packet
        public byte[] AcquirePacketBuffer()
        {
            lock (m_packetBufPool)
            {
                if (m_packetBufPool.Count < 1)
                {
                    Log.Notice("TCPManager", "The buffer pool is empty!");
                    AllocatePacketBuffers();
                }

                return m_packetBufPool.Dequeue();
            }
        }

        // Relachement d'un packet
        public void ReleasePacketBuffer(byte[] buf)
        {
            if (buf == null)
                return;

            lock (m_packetBufPool)
            {
                m_packetBufPool.Enqueue(buf);
            }
        }

        protected virtual BaseClient GetNewClient()
        {
            return new BaseClient(this);
        }

        #endregion

        // Thread for incomming connection
        private void ConnectingThread(IAsyncResult ar)
        {
            Socket sock = null;

            try
            {
                if (Listener == null && Client == null)
                    return;

                if(Listener != null)
                    sock = Listener.EndAcceptSocket(ar);

                sock.SendBufferSize = BUF_SIZE;
                sock.ReceiveBufferSize = BUF_SIZE;
                sock.NoDelay = false;
                sock.Blocking = false;

                BaseClient baseClient = null;

                try
                {
                    string ip = sock.Connected ? sock.RemoteEndPoint.ToString() : "socket disconnected";
                    if(Listener != null)
                        Log.Debug("TCPManager", "New Connection : " + ip);

                    if(Client != null)
                        Log.Debug("TCPManager", "New connection to : " + ip);

                    baseClient = GetNewClient();
                    baseClient.Socket = sock;

                    baseClient.OnConnect();
                    baseClient.BeginReceive();
                }
                catch (SocketException e)
                {
                    if (baseClient != null)
                        Disconnect(baseClient, $"ConnectingThread: { Enum.GetName(typeof(SocketError), e.ErrorCode) } ({ e.Message })");
                }
                catch (Exception e)
                {
                    Log.Error("TCPManager", e.ToString());

                    if (baseClient != null)
                        Disconnect(baseClient, "Exception within ConnectingThread");
                }
            }
            catch
            {
                if (sock != null) // Ne pas laisser le socket ouvert
                {
                    try
                    {
                        sock.Close();
                    }
                    catch
                    {
                    }
                }
            }
            finally
            {
                if (Listener != null) // Quoi qu'il arrive on continu d'écouter le socket
                {
                    Listener.BeginAcceptSocket(_asyncAcceptCallback, this);
                }
            }
        }

        public virtual bool Disconnect(BaseClient baseClient, string reason)
        {
            RemoveClient(baseClient);

            try
            {
                baseClient.OnDisconnect(reason);
                baseClient.CloseConnections();
            }
            catch (Exception e)
            {
                Log.Error("TCPManager", e.ToString());

                return false;
            }

            return true;
        }

        // PacketFunction
        public void LoadPacketHandler()
        {
            Log.Info("TCPManager", "Loading the Packet Handler");

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    // Pick up a class
                    if (type.IsClass != true)
                        continue;

                    foreach (MethodInfo m in type.GetMethods())
                        foreach (object at in m.GetCustomAttributes(typeof(PacketHandlerAttribute), false))
                        {
                            PacketHandlerAttribute attr = (PacketHandlerAttribute) at;
                            PacketFunction handler = (PacketFunction)Delegate.CreateDelegate(typeof(PacketFunction), m);

                            Log.Debug("TCPManager", $"Registering handler for opcode : " +
                                                    $"{attr.Opcode.ToString("X8")} " +
                                                    $"{handler.Method.Module}" +
                                                    $"{handler.Method.DeclaringType.AssemblyQualifiedName}");
                            m_packetHandlers[attr.Opcode] = handler;
                            stateRequirement[attr.Opcode] = (byte)attr.State;
                        }
                }
            }
        }

        //Charge les systemes de cryptographie
        public void LoadCryptHandler()
        {
            Log.Info("TCPManager", "Loading Crypt Handler");

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    // Pick up a class
                    if (type.IsClass != true)
                        continue;

                    CryptAttribute[] crypthandler =
                        (CryptAttribute[])type.GetCustomAttributes(typeof(CryptAttribute), true);

                    if (crypthandler.Length > 0)
                    {
                        Log.Debug("TCPManager", "Registering crypt " + crypthandler[0]._CryptName);
                        m_cryptHandlers.Add(crypthandler[0]._CryptName, (ICryptHandler)Activator.CreateInstance(type));
                    }
                }
            }
        }

        public ICryptHandler GetCrypt(string name)
        {
            if (m_cryptHandlers.ContainsKey(name))
                return m_cryptHandlers[name];
            else 
                return null;
        }

        // Enregistre un handler
        public void RegisterPacketHandler(int packetCode, PacketFunction handler)
        {
            m_packetHandlers[packetCode] = handler;
        }

        public HashSet<ulong> Errors = new HashSet<ulong>();
        public void HandlePacket(BaseClient client, PacketIn packet)
        {
            //#if DEBUG
            Log.Debug("HandlePacket", $"Packet : {packet.Opcode} ({packet.Opcode.ToString("X8")})");
            Log.Dump("HandlePacket", packet.ToArray(), 0, packet.ToArray().Length);
            //#endif

            if (client == null)
            {
                Log.Error("TCPManager", "Client == null");
                return;
            }

            PacketFunction packetHandler = null;

            if (packet.Opcode < (ulong)m_packetHandlers.Length)
                packetHandler = m_packetHandlers[packet.Opcode];
            else if (!Errors.Contains(packet.Opcode))
            {
                Errors.Add(packet.Opcode);
                Log.Error("TCPManager", $"Can not handle :{packet.Opcode} ({packet.Opcode.ToString("X8")})");
            }

            if (packetHandler != null)
            {
                /*

                The reflection code below seems to have been used to verify the client was in the correct state before the packet was handled.
                However, it didn't actually work; eliminating the reflection and using an array implementation broke the emu and testing confirmed
                the original check was broken, so I've eliminated it for now.


                if (stateRequirement[packet.Opcode] > client.State)
                {
                    Log.Error("TCPManager", $"Can not handle packet ({packet.Opcode.ToString("X8")}), Invalid client state {client.State} (expected {stateRequirement[packet.Opcode]}) ({client.GetIp()})");
                    PacketHandlerAttribute[] packethandlerattribs = (PacketHandlerAttribute[])packetHandler.GetType().GetCustomAttributes(typeof(PacketHandlerAttribute), true);
                    if (packethandlerattribs.Length > 0)
                    { 
                        Log.Error("TCPManager", $"Old code provided state {packethandlerattribs[0].State}");
                        if (packethandlerattribs[0].State > client.State)
                            return;
                    }
                    else
                    {
                        Log.Error("TCPManager", "Old code was stateless");
                    }
                }
                */

                try
                {
                    packetHandler.Invoke(client,packet);
                }
                catch (Exception e)
                {
                    Log.Error("TCPManager", $"Packet handler error :{packet.Opcode} {e}");
                }
            }
            else if (!Errors.Contains(packet.Opcode))
            {
                Errors.Add(packet.Opcode);
                Log.Error("TCPManager", $"Can not Handle opcode :{packet.Opcode} ({packet.Opcode.ToString("X8")})");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace WorldServer.API
{
    public class Client
    {
        public int ID { get; set; }
        private static ushort _pidRef = 1001;
        private ushort _pid = 0;
        private Queue<ApiPacket> _inQueue = new Queue<ApiPacket>();
        private Queue<ApiPacket> _outQueue = new Queue<ApiPacket>();
        private Server _server;
        private Socket _socket;
        private CircularBuffer _buffer = new CircularBuffer(0xFFFF);
        private byte[] _data = new byte[0xFFFF];
        private byte[] _sendData = new byte[0xFFFF];
        private bool _sending = false;
        private byte[] _key;
        private byte[] _tmpEncKey = new byte[256];


        public ushort PID
        {
            get
            {
                return _pid;
            }
        }

        public Queue<ApiPacket> InQueue
        {
            get { return _inQueue; }
            set { _inQueue = value; }
        }
      

        public byte[] Key
        {
            get
            {
                return _key;
            }
            set
            {
                _key = value;
            }
        }

        public Socket Socket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        public Client(Server server)
        {
            _server = server;
            _pid = _pidRef++;
        }

        public void Receive()
        {
            if (_socket != null && _socket.Connected)
            {
                _socket.BeginReceive(_data, 0, _data.Length, SocketFlags.None, BeginReceive, this);
            }
        }

        public ApiPacket CreatePacket()
        {
            return new ApiPacket(Opcodes.OK);
        }


        private ApiPacket GetNextFrame()
        {
            int frameSize = 0;
            if (_buffer.Level < 4)
            {
                return null;
            }

            frameSize = (int)_buffer.ReadInt32();

            if (_buffer.Level < frameSize + 1)
            {
                return null;
            }

            ApiPacket packet = CreatePacket();
            packet.LoadFrame(frameSize, _buffer);

            _buffer.Discard(frameSize + 1);
            return packet;
        }


        protected readonly Queue<ApiPacket> _tcpQueue = new Queue<ApiPacket>();
        public bool _sendingTcp = false;
        public byte[] _sendBuffer = new byte[0xFFFF];


        public void Disconnect()
        {
            _server.DeleteClient(this);

        }
        public void SendPacket(ApiPacket frame, bool finish = true)
        {
            if (_socket.Connected)
            {
                try
                {
                    if (frame != null)
                    {

                        if (finish)
                            frame.FinishPacket();
                        lock (_tcpQueue)
                        {
                            _tcpQueue.Enqueue(frame);
                        }

                        bool exit = false;
                        if (_sendingTcp)
                        {
                            return;
                        }
                    }


                    List<ArraySegment<byte>> _sendBuffers = new List<ArraySegment<byte>>();

                    lock (_tcpQueue)
                    {
                        while (_tcpQueue.Count > 0)
                        {
                            var v = _tcpQueue.Dequeue();
                            if (v != null)
                            {
                                byte[] dd = new byte[v.Offset];
                                System.Buffer.BlockCopy(v.Data, 0, dd, 0, v.Offset);
                                _sendBuffers.Add(new ArraySegment<byte>(dd, 0, v.Offset));
                            }
                        }
                    }

                    if (_sendBuffers.Count > 0)
                    {
                        _sendingTcp = true;
                        _socket.BeginSend(_sendBuffers, SocketFlags.None, SendCallback, this);
                    }

                }
                catch (Exception e)
                {
                }
                finally
                {

                }
            }
        }

        protected virtual void SendCallback(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            try
            {
                int sent = client._socket.EndSend(ar);

                client._sendingTcp = false;



                SendPacket(null);


            }
            catch (Exception)
            {
                //  _server.DeleteClient(this);
            }
        }


        private void BeginReceive(IAsyncResult ar)
        {
            Client client = (Client)ar.AsyncState;
            try
            {
                int packetSize = client.Socket.EndReceive(ar); //get size of incomming packet
                if (packetSize > 0)
                {
                    _buffer.Write(_data, packetSize);
                    ApiPacket packet = null;
                    while ((packet = GetNextFrame()) != null)
                    {
                        _server.OnFrame(this, packet);
                    }

                    client.Receive();
                }
                else
                {
                    _server.DeleteClient(client);
                }
            }
            catch (Exception)
            {
               // _server._log.Append("Client disconnected", LogType.NET);
            }
        }
    }
}

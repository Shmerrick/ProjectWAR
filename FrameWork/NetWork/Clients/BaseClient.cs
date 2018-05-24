/*
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
 
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;

namespace FrameWork
{
    public class BaseClient
    {
        // Appeler lorsque le client recoit des données
        private static readonly AsyncCallback ReceiveCallback = OnReceiveHandler;
        public static bool DisconnectOnNullByte = true;

        public long Id { get; set; }
        public bool PacketLog = false;
        public int State { get; set; }

        #region Buffer&Socket

        protected byte[] _pBuf = new byte[2048];
        protected int _pBufOffset;
        protected Socket _socket;

        public byte[] ReceiveBuffer
        { get { return _pBuf; } }

        public int ReceiveBufferOffset
        {
            get { return _pBufOffset; }
            set { _pBufOffset = value; }
        }

        public Socket Socket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        #endregion

        #region Crypto

        private Dictionary<ICryptHandler, CryptKey[]> m_crypts = new Dictionary<ICryptHandler, CryptKey[]>();

        public bool AddCrypt(string name, CryptKey cKey,CryptKey dKey)
        {
            ICryptHandler Handler = Server.GetCrypt(name);

            if (Handler == null)
                return false;

            if (cKey == null)
                cKey = Handler.GenerateKey(this);

            if (dKey == null)
                dKey = Handler.GenerateKey(this);

            Log.Debug("Crypt", "Add crypt : " + name);

            if (m_crypts.ContainsKey(Handler))
                m_crypts[Handler] = new CryptKey[] { cKey , dKey };
            else 
                m_crypts.Add(Handler, new CryptKey[] { cKey , dKey } );

            return true;
        }

        public void Decrypt(PacketIn inStream)
        {
            if (m_crypts.Count <= 0)
                return;

            ulong opcode = inStream.Opcode;
            int size = (int)inStream.Size;
            int startPos = (int)inStream.Position;

            foreach (KeyValuePair<ICryptHandler, CryptKey[]> entry in m_crypts)
            {
                //Log.Debug("Decrypt", "Decrypt with " + entry.Key + ",Size="+inStream.Size);

                entry.Key.Decrypt(entry.Value[1], inStream.GetBuffer(), startPos, size);

                inStream.Opcode = opcode;
                inStream.Size = (ulong)size;
            }

            //Log.Tcp("Decrypt", inStream.GetBuffer(), 0, inStream.GetBuffer().Length);
            inStream.Position = startPos;
        }

        private static readonly ThreadLocal<byte[]> TLSendBuffer = new ThreadLocal<byte[]>(() => new byte[65535]);

        /// <summary>Copies the packet into the thread-local TCP send buffer, encrypting it if required. </summary>
        /// <param name="packet">A network packet.</param>
        /// <param name="destOffset">The offset to copy this packet to in the local send buffer.</param>
        /// <returns>The number of bytes to send, or 0 if encryption failed.</returns>
        public int EncryptAndBuffer(PacketOut packet, int destOffset)
        {
            int packetLength = (int) packet.Length;

            if (packetLength > 65535)
            { 
                Log.Error("EncryptAndBuffer", "Packet length is greater than send buffer size!");
                return 0;
            }

            // Create a copy of the bytes to be encrypted
            Buffer.BlockCopy(packet.GetBuffer(), 0, TLSendBuffer.Value, destOffset, packetLength);

            if (m_crypts.Count > 0)
            {
                // Figure out the size of the header
                int headerPos = 0;
                headerPos += PacketOut.SizeLen;
                if (PacketOut.OpcodeInLen)
                    headerPos += packet.OpcodeLen;

                try
                {
                    // Encrypt the copy in-place, skipping the header bytes
                    foreach (KeyValuePair<ICryptHandler, CryptKey[]> cryptEntry in m_crypts)
                        cryptEntry.Key.Crypt(cryptEntry.Value[0], TLSendBuffer.Value, destOffset + headerPos, packetLength - headerPos);
                }
                catch (Exception e)
                {
                    Log.Error("BaseClient", "Crypt Error : " + e);
                    return 0;
                }
            }

            return packetLength;
        }

        #endregion

        private string _ip;

        public string GetIp()
        {
                if (!string.IsNullOrEmpty(_ip))
                    return _ip;

                Socket s = _socket;
                if (s != null && s.Connected && s.RemoteEndPoint != null)
                {
                    _ip = s.RemoteEndPoint.ToString();
                    return _ip;
                }

                return "disconnected";
        }

        public TCPManager Server { get; }

        public BaseClient(TCPManager srvr)
        {
            srvr.GenerateId(this);

            Server = srvr;

            if (srvr != null)
                _pBuf = srvr.AcquirePacketBuffer();

            m_tcpSendBuffer = srvr.AcquirePacketBuffer();

            _pBufOffset = 0;
        }

        public virtual void OnConnect()
        {

        }
        protected virtual void OnReceive(byte[] packetBuffer)
        {

        }
        public virtual void OnDisconnect(string reason)
        {

        }

        public void BeginReceive()
        {
            if (_socket != null && _socket.Connected)
            {
                int bufSize = _pBuf.Length;

                if (_pBufOffset >= bufSize) //Do we have space to receive?
                {
                    Log.Debug("Client", GetIp() + " disconnection was due to a buffer overflow!");
                    Log.Debug("Client", "_pBufOffset=" + _pBufOffset + "; buf size=" + bufSize);
                    Log.Debug("Client", _pBuf.ToString());

                    Server.Disconnect(this, "Buffer overflow in BeginReceive");
                }
                else
                {
                    _socket.BeginReceive(_pBuf, _pBufOffset, bufSize - _pBufOffset, SocketFlags.None, ReceiveCallback, this);
                }
            }
        }

        private static void OnReceiveHandler(IAsyncResult ar)
        {
            if (ar == null)
                return;

            BaseClient baseClient = null;

            try
            {
                baseClient = (BaseClient)ar.AsyncState;
                int numBytes = baseClient.Socket.EndReceive(ar);

                if (numBytes > 0 || (numBytes <=0 && DisconnectOnNullByte == false))
                {
                    Log.Tcp(baseClient.GetIp(), baseClient.ReceiveBuffer, 0, numBytes);

                    int bufferSize = baseClient.ReceiveBufferOffset + numBytes;

                    byte[] packetStream = new byte[bufferSize];
                    Buffer.BlockCopy(baseClient.ReceiveBuffer, 0, packetStream, 0, bufferSize);
                    baseClient.ReceiveBufferOffset = 0;
                    baseClient.OnReceive(packetStream);

                    baseClient.BeginReceive();
                }
               else
                {
                    Log.Debug("BaseClient","disconnection of client (" + baseClient.GetIp() + "), received bytes=" + numBytes);

                    baseClient.Server.Disconnect(baseClient, "Exiting");
                }
            }
            catch (ObjectDisposedException)
            {
                if (baseClient != null)
                    baseClient.Server.Disconnect(baseClient, "ObjectDisposedException in OnReceiveHandler");
            }
            catch (SocketException e)
            {
                if (baseClient != null)
                {
                    Log.Debug("BaseClient",string.Format("{0}  {1}", baseClient.GetIp(), e.Message));

                    baseClient.Server.Disconnect(baseClient, $"OnReceiveHandler: { Enum.GetName(typeof(SocketError), e.ErrorCode) } ({ e.Message })");
                }
            }
            catch (Exception e)
            {
                Log.Error("BaseClient",e.ToString());

                if (baseClient != null)
                    baseClient.Server.Disconnect(baseClient, "Exception in OnReceiveHandler");
            }
        }
        
        //private bool _blockSend = false;

        public void CloseConnections()
        {
            if (_socket != null && _socket.Connected)
            {
                try
                {
                    //_blockSend = true;
                    _socket.Shutdown(SocketShutdown.Send);
                }
                catch (Exception e)
                {
                    Log.Error("BaseClient", "CloseConnections (Shutdown): "+e);
                }

                try
                {
                    _socket.Close();
                }
                catch (Exception e)
                {
                    Log.Error("BaseClient", "CloseConnections (Close): " + e);
                }
            }

            byte[] buff = _pBuf;
            if (buff != null)
            {
                _pBuf = null;
                Server.ReleasePacketBuffer(buff);
            }
        }

        public void Disconnect(string reason)
        {
            try
            {
                Server.Disconnect(this, reason);
            }
            catch (Exception e)
            {
                Log.Error("Baseclient", e.ToString());
            }
        }

		#region TCP

        // Buffer en train d'être envoyé
		protected byte[] m_tcpSendBuffer;

        // Liste des packets a sender
		protected readonly Queue<byte[]> m_tcpQueue = new Queue<byte[]>(256);

        // True si un send est en cours
		protected bool m_sendingTcp;

        // Envoi un packet
        public virtual void SendPacket(PacketOut packet)
        {
            if (!packet.Finalized)
                packet.WritePacketLength();

            //Send the encrypted packet
            SendPacketNoBlock(packet);
            // Crypt(packet);
            // SendAsynchronousTCP(TLSendBuffer.Value);
        }

        public virtual bool SendPacketNoBlock(PacketOut packet)
        {
            Log.Debug("BaseClient", $"Socket Connected : {Socket.Connected} Packet : {packet.Opcode}", true);
            if (!Socket.Connected)
                return false;

            if (packet == null)
                return true;

            return SendThreadLocalBuffer(EncryptAndBuffer(packet, 0));
        }

        public virtual bool SendPacketsNoBlock(List<PacketOut> packetList, int lengthPerSend)
        {
            if (!Socket.Connected)
                return false;

            if (packetList == null)
                return true;

            int bufferLength = 0;

            // Encrypt the packets
            for (int index = 0; index < packetList.Count; index++)
            {
                // Send if approaching 8KB
                if (bufferLength > 0 && bufferLength + packetList[index].Length >= lengthPerSend)
                {
                    if (!SendThreadLocalBuffer(bufferLength))
                        return false;

                    // Remove sent packets and reset buffer length
                    packetList.RemoveRange(0, index);
                    index = 0;
                    bufferLength = 0;
                }

                bufferLength += EncryptAndBuffer(packetList[index], bufferLength);
            }

            //Log.Info("SendPacketsNoBlock", "Send Length: "+bufferLength);

            return SendThreadLocalBuffer(bufferLength);
        }

        private bool SendThreadLocalBuffer(int bufferLength)
        {
            if (bufferLength == 0)
                return false;

            // Send the buffer
            SocketError errorCode;

            //if (_blockSend)
            //    return false;

            int sentBytes = Socket.Send(TLSendBuffer.Value, 0, bufferLength, SocketFlags.None, out errorCode);

            if (errorCode == SocketError.Success)
            {
                if (sentBytes < bufferLength)
                    Log.Error(GetIp(), $"Partial send ({sentBytes}/{bufferLength})");
                return true;
            }

            // Socket write buffer full.
            if (errorCode == SocketError.WouldBlock)
                return false;

            Log.Error("BaseClient", "Socket.Send() returned " + errorCode);
            Disconnect("Socket send failure");
            return false;
        }

        public bool SendTCP(byte[] buffer)
        {
            if (!Socket.Connected)
                return false;

            SocketError errorCode;
            Socket.Send(buffer, 0, buffer.Length, SocketFlags.None, out errorCode);

            if (errorCode == SocketError.Success)
                return true;
            if (errorCode == SocketError.WouldBlock)
                return false;

            Log.Error("BaseClient", "Socket.Send() returned " + errorCode);
            Disconnect("Socket send failure");
            return false;
        }

        public void SendAsynchronousTCP(byte[] buf)
		{
			if (m_tcpSendBuffer == null)
				return;

			//Check if client is connected
            if (!Socket.Connected)
                return;

            try
            {
                lock (m_tcpQueue)
                {
                    if (m_sendingTcp)
                    {
                        m_tcpQueue.Enqueue(buf);
                        return;
                    }
						
                    m_sendingTcp = true;
                }

                Log.Tcp(m_crypts.Count <= 0 ? "SendTCP: " : "Crypted: ", buf, 0, buf.Length);

                Buffer.BlockCopy(buf, 0, m_tcpSendBuffer, 0, buf.Length);

                int start = Environment.TickCount;

                Socket.BeginSend(m_tcpSendBuffer, 0, buf.Length, SocketFlags.None, m_asyncTcpCallback, this);

                int took = Environment.TickCount - start;
                if (took > 100)
                    Log.Notice("BaseClient","SendTCP.BeginSend took "+ took);
            }
            catch (Exception e)
            {
                // assure that no exception is thrown into the upper layers and interrupt game loops!
                Log.Error("BaseClient", "SendTCP : " + e);
                Server.Disconnect(this, "Exception in SendTCP");
            }
		}

        public void SendAsynchronousTCP2(byte[] buf)
        {
            if (m_tcpSendBuffer == null)
                return;

            //Check if client is connected
            if (!Socket.Connected)
                return;

            try
            {
                lock (m_tcpQueue)
                {
                    if (m_sendingTcp)
                    {
                        m_tcpQueue.Enqueue(buf);
                        return;
                    }

                    m_sendingTcp = true;
                }

               // Log.Tcp(m_crypts.Count <= 0 ? "SendTCP: " : "Crypted: ", buf, 0, buf.Length);

               // Buffer.BlockCopy(buf, 0, m_tcpSendBuffer, 0, buf.Length);

                int start = Environment.TickCount;

                Socket.BeginSend(buf, 0, buf.Length, SocketFlags.None, m_asyncTcpCallback, this);

                //int took = Environment.TickCount - start;
                //if (took > 100)
                //    Log.Notice("BaseClient", "SendTCP.BeginSend took " + took);
            }
            catch (Exception e)
            {
                // assure that no exception is thrown into the upper layers and interrupt game loops!
                Log.Error("BaseClient", "SendTCP : " + e);
                Server.Disconnect(this, "Exception in SendTCP");
            }
        }

        protected static readonly AsyncCallback m_asyncTcpCallback = AsyncTcpSendCallback;

		protected static void AsyncTcpSendCallback(IAsyncResult ar)
		{
			if (ar == null)
			{
				Log.Error("BaseClient","AsyncSendCallback: ar == null");
				return;
			}

            BaseClient client = (BaseClient)ar.AsyncState;

			try
			{
                Queue<byte[]> q = client.m_tcpQueue;

				int sent = client.Socket.EndSend(ar);

				int count = 0;
                byte[] data = client.m_tcpSendBuffer;

                if (data == null)
                {
                    Log.Error("TcpCallBack", "Data == null");
                    return;
                }

				lock (q)
				{
					if (q.Count > 0)
					{
						count = CombinePackets(data, q, data.Length, client);
					}
					if (count <= 0)
					{
                        client.m_sendingTcp = false;
						return;
					}
				}

				int start = Environment.TickCount;

                if (client.m_crypts.Count <= 0)
                    Log.Tcp("SendTCPAs", data, 0, count);
                else
                    Log.Tcp("CryptedAs", data, 0, count);

				client.Socket.BeginSend(data, 0, count, SocketFlags.None, m_asyncTcpCallback, client);

				int took = Environment.TickCount - start;

            }
			catch (ObjectDisposedException)
			{
                client.Server.Disconnect(client, "ObjectDisposedException within TCPSendCallback");
			}
			catch (SocketException e)
			{
                client.Server.Disconnect(client, $"TCPSendCallback: { Enum.GetName(typeof(SocketError), e.ErrorCode) } ({ e.Message })");
            }
			catch (Exception)
			{
                client.Server.Disconnect(client, "Exception within TCPSendCallback");
			}
		}

		private static int CombinePackets(byte[] buf, Queue<byte[]> q, int length, BaseClient client)
		{
			int i = 0;
			byte[] pak = q.Dequeue();
			Buffer.BlockCopy(pak, 0, buf, i, pak.Length);
			i += pak.Length;
			return i;
		}

		public virtual void SendTCPRaw(PacketOut packet)
		{
            if (!packet.Finalized)
                packet.WritePacketLength();

            SendAsynchronousTCP2((byte[]) packet.GetBuffer().Clone());
		}

        #endregion


        public static T ByteToType<T>(PacketIn packet)
        {
            BinaryReader reader = new BinaryReader(packet);
            byte[] bytes = reader.ReadBytes(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));

            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = (T)System.Runtime.InteropServices.Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return theStructure;
        }

    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using Common;
using FrameWork;
using WorldServer.World.Objects;

namespace WorldServer.NetWork
{
    public enum eClientState
    {
        NotConnected = 0x00,
        Connecting = 0x01,
        CharScreen = 0x02,
        WorldEnter = 0x03,
        Playing = 0x04,
        Linkdead = 0x05,
        Disconnected = 0x06
    } ;

    public class GameClient : BaseClient
    {
        private readonly object _syncLock = new object();
        public Account _Account = null;
        public Player Plr = null;
        private Thread _logThread = null;
        private readonly AutoResetEvent _packetLogWakeEvent = new AutoResetEvent(false);
        private readonly object _packetLogThreadLock = new object();
        private volatile bool _packetLogThreadStopRequested;
        private List<string> _packetLog = new List<string>();

        private CircularBuffer<object> _pLogBuf = new CircularBuffer<object>(100);

        public CircularBuffer<object> PLogBuf
        {
            get
            {
                return _pLogBuf;
            }
        }

        public GameClient(TCPManager srv)
            : base(srv)
        {

        }

        public override void OnConnect()
        {
            Log.Debug("WorldServer..Connection", GetIp());
            State = (int)eClientState.Connecting;
        }
        public override void OnDisconnect(string reason)
        {
            string ipString = GetIp();

            if (ipString == "disconnected")
                ipString = "Disconnected client";

            Log.Debug($"{ _Account?.Username ?? "Unknown user" } ({ipString}) disconnected", reason);

            if (_logThread != null)
                StopPacketLogThread();

            if (Plr != null)
            {
                Plr.Client = null;
                Plr.Destroy();
            }
        }

        public bool IsPlaying()
        {
            return State == (int)eClientState.Playing;
        }
        public bool HasPlayer()
        {
            return Plr != null;
        }
        public bool HasAccount()
        {
            return _Account != null;
        }

        public ushort SequenceID,SessionID,Unk1;
        public byte Unk2;

        private void LogInPacket(PacketIn packet)
        {
            EnsurePacketLogThread();

            lock(_packetLog)
            {
                _packetLog.Add(Utils.ToLogHexString((byte)packet.Opcode, false, packet.ToArray()));
            }

            _packetLogWakeEvent.Set();
        }

        private void LogOutPacket(PacketOut packet)
        {
            EnsurePacketLogThread();

            lock (_packetLog)
            {
                _packetLog.Add(Utils.ToLogHexString((byte)packet.Opcode, true, packet.ToArray()));
            }

            _packetLogWakeEvent.Set();
        }

        private void EnsurePacketLogThread()
        {
            if (_logThread != null)
                return;

            lock (_packetLogThreadLock)
            {
                if (_logThread != null)
                    return;

                _packetLogThreadStopRequested = false;
                _logThread = new Thread(PacketLogThread)
                {
                    IsBackground = true,
                    Name = "GameClientPacketLog"
                };
                _logThread.Start();
            }
        }

        private void StopPacketLogThread()
        {
            Thread logThread;
            lock (_packetLogThreadLock)
            {
                logThread = _logThread;
                if (logThread == null)
                    return;

                _packetLogThreadStopRequested = true;
                _packetLogWakeEvent.Set();
            }

            try
            {
                if (!logThread.Join(2000))
                    Log.Notice("GameClient", "Timed out while waiting for the packet log thread to stop.");
            }
            catch (Exception ex)
            {
                Log.Error("GameClient", "Failed to stop packet log thread: " + ex.Message);
            }

            lock (_packetLogThreadLock)
            {
                if (ReferenceEquals(_logThread, logThread))
                    _logThread = null;
            }
        }

        private void PacketLogThread()
        {
            try
            {
                while (true)
                {
                    FlushPacketLog();
                    if (_packetLogThreadStopRequested)
                        return;

                    Socket socket = Socket;
                    if (socket == null || !socket.Connected)
                        return;

                    _packetLogWakeEvent.WaitOne(5000);
                }
            }
            finally
            {
                lock (_packetLogThreadLock)
                {
                    if (ReferenceEquals(_logThread, Thread.CurrentThread))
                        _logThread = null;
                }
            }
        }
        private void FlushPacketLog()
        {
            var packets = new List<String>();
            lock(_packetLog)
            {
                packets = _packetLog.ToList();
                _packetLog.Clear();
            }

            if (_Account != null)
            {
                if (!Directory.Exists("PacketLogs"))
                    Directory.CreateDirectory("PacketLogs");

                string file = Path.Combine("PacketLogs", _Account.Username + ".txt");
                StringBuilder log = new StringBuilder();
                foreach (var packet in packets)
                    log.AppendLine(packet);

                File.AppendAllText(file, log.ToString());
            }
        }

        public override void SendPacket(PacketOut packet)
        {
            if(PacketLog)
                LogOutPacket(packet);

            PLogBuf.Enqueue(packet);

            base.SendPacket(packet);
        }

        public override bool SendPacketNoBlock(PacketOut packet)
        {
            if (PacketLog)
                LogOutPacket(packet);

            PLogBuf.Enqueue(packet);


            return base.SendPacketNoBlock(packet);
        }

        public override bool SendPacketsNoBlock(List<PacketOut> packetList, int lengthPerSend)
        {
            foreach (PacketOut packet in packetList)
            {
                if (PacketLog)
                    LogOutPacket(packet);

                PLogBuf.Enqueue(packet);
            }

            return base.SendPacketsNoBlock(packetList, lengthPerSend);
        }

        /// <summary>Bytes in a frame besides its payload: the 2-byte length, the 8-byte header and the 2-byte trailer.</summary>
        private const int FrameOverhead = 12;

        /// <summary>
        /// Splits the received bytes into frames and dispatches each complete one. TCP does not keep frame
        /// boundaries, so a frame can arrive across several reads; its bytes were previously discarded,
        /// and the next read then parsed the middle of that frame as a header. The incomplete tail is now
        /// kept at the front of the receive buffer, still encrypted, and completed by the next read.
        /// </summary>
        protected override void OnReceive(byte[] packetBuffer)
        {
            // Wrap the input stream in a PacketIn
            PacketIn inStream = new PacketIn(packetBuffer, 0, packetBuffer.Length, true, true);

            lock (_syncLock)
            {
                int offset = 0;

                // Bounded: every pass consumes one whole frame of at least FrameOverhead bytes.
                while (packetBuffer.Length - offset >= 2)
                {
                    int payloadSize = Marshal.ConvertToUInt16(packetBuffer, offset);

                    if (packetBuffer.Length - offset < payloadSize + FrameOverhead)
                        break;

                    DispatchFrame(inStream, offset, payloadSize);
                    offset += payloadSize + FrameOverhead;
                }

                RetainPartialFrame(packetBuffer, offset);
            }
        }

        private void DispatchFrame(PacketIn inStream, int offset, int payloadSize)
        {
            inStream.Position = offset + 2;
            inStream.Size = (ulong)(payloadSize + FrameOverhead - 2);
            Decrypt(inStream);

            SequenceID = inStream.GetUint16();
            SessionID = inStream.GetUint16();
            Unk1 = inStream.GetUint16();
            Unk2 = inStream.GetUint8();
            byte opcode = inStream.GetUint8();

            byte[] bPack = new byte[payloadSize + 2];
            inStream.Read(bPack, 0, bPack.Length);

            PacketIn packet = new PacketIn(bPack, 0, bPack.Length)
            {
                Opcode = opcode,
                Size = (ulong)payloadSize
            };

            if (PacketLog)
            {
                LogInPacket(packet);
            }

            PLogBuf.Enqueue(packet);

            if (Plr != null && Plr.IsInWorld())
                Plr.ReceivePacket(packet);
            else
                Server.HandlePacket(this, packet);

            Log.Tcp("PacketSize", bPack, 0, bPack.Length);
        }

        /// <summary>Moves the undispatched tail of a read to the front of the receive buffer for the next read to complete.</summary>
        private void RetainPartialFrame(byte[] packetBuffer, int offset)
        {
            int remaining = packetBuffer.Length - offset;

            if (remaining <= 0)
                return;

            // A frame longer than the receive buffer could never complete, and keeping its head would
            // only fill the buffer; the stream cannot be resynchronised past it.
            int frameSize = remaining >= 2 ? Marshal.ConvertToUInt16(packetBuffer, offset) + FrameOverhead : 2;

            if (frameSize > ReceiveBuffer.Length)
            {
                Log.Error("OnReceive", "Frame of " + frameSize + " bytes exceeds the " + ReceiveBuffer.Length + "-byte receive buffer.");
                Server.Disconnect(this, "Oversized frame");
                return;
            }

            Buffer.BlockCopy(packetBuffer, offset, ReceiveBuffer, 0, remaining);
            ReceiveBufferOffset = remaining;
        }
    }
}

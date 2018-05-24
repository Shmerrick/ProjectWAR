
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

using FrameWork;

namespace LobbyServer
{
    public class Client : BaseClient
    {
        public string Username = "";
        public string Token = "";

        public Client(TCPManager srv)
            : base(srv)
        {
            RSACryptoServiceProvider prov = new RSACryptoServiceProvider();
        }

        public override void OnConnect()
        {
            Log.Info("LobbyServer..Connection", GetIp());
        }

        public override void OnDisconnect(string reason)
        {
            Log.Info("Disconnection", GetIp() + " (" + reason + ")");
        }

        private ushort _opcode;
        private int _expectedSize;
        private bool _expectData;

        protected override void OnReceive(byte[] packetBuffer)
        {
            lock (this)
            {
                PacketIn packet = new PacketIn(packetBuffer, 0, packetBuffer.Length);
                long bytesLeft = packet.Length;

                Log.Debug("OnReceive", $"Packet Received : Size {bytesLeft}, OpCode {packet.Opcode}");

                while (bytesLeft > 0)
                {
                    if (!_expectData)
                    {
                        long startPos = packet.Position;
                        _expectedSize = packet.DecodeMythicSize();
                        long endPos = packet.Position;

                        long diff = endPos - startPos;
                        bytesLeft -= diff;
                        if (_expectedSize <= 0)
                        {
                            packet.Opcode = packet.GetUint8();
                            packet.Size = (ulong)_expectedSize;
                            Server.HandlePacket(this, packet);
                            return;
                        }

                        if (bytesLeft <= 0)
                            return;

                        _opcode = packet.GetUint8();
                        bytesLeft -= 1;

                        _expectData = true;
                    }
                    else
                    {
                        _expectData = false;
                        if (bytesLeft >= _expectedSize)
                        {
                            long pos = packet.Position;

                            packet.Opcode = _opcode;
                            packet.Size = (ulong)_expectedSize;

                            Server.HandlePacket(this, packet);

                            bytesLeft -= _expectedSize;
                            packet.Position = pos;
                            packet.Skip(_expectedSize);
                        }
                        else
                        {
                            Log.Error("OnReceive", "Data count incorrect :" + bytesLeft + " != " + _expectedSize);
                        }
                    }
                }

                packet.Dispose();
            }
        }

        public void SendTCPCuted(PacketOut outPacket)
        {

            long pSize = outPacket.Length - outPacket.OpcodeLen - PacketOut.SizeLen; // Size = Size-len-opcode

            byte[] packet = new byte[pSize];
            outPacket.Position = outPacket.OpcodeLen + PacketOut.SizeLen;
            outPacket.Read(packet, 0, (int)(pSize));

            List<byte> header = new List<byte>(5);
            int itemcount = 1;
            while (pSize > 0x7f)
            {
                header.Add((byte)((byte)(pSize) | 0x80));
                pSize >>= 7;
                itemcount++;
                if (itemcount >= header.Capacity + 10)
                    header.Capacity += 10;
            }

            header.Add((byte)(pSize));
            header.Add((byte)(outPacket.Opcode));

            Log.Tcp("Header", header.ToArray(), 0, header.Count);
            Log.Tcp("Packet", packet, 0, packet.Length);

            //Log.Dump("Header", header.ToArray(), 0, header.Count);
            //Log.Dump("Packet", packet, 0, packet.Length);

            SendTCP(header.ToArray());
            SendTCP(packet);

            outPacket.Dispose();
        }

        public void SendSegments(PacketOut Out)
        {
            long pSize = Out.Length - Out.OpcodeLen - PacketOut.SizeLen; // Size = Size-len-opcode

            Out.Position = Out.OpcodeLen + PacketOut.SizeLen;
            //Out.Read(Packet, 0, (int)(PSize));

            List<byte> header = new List<byte>(5);
            int itemcount = 1;
            while (pSize > 0x7f)
            {
                header.Add((byte)((byte)(pSize) | 0x80));
                pSize >>= 7;
                itemcount++;
                if (itemcount >= header.Capacity + 10)
                    header.Capacity += 10;
            }

            header.Add((byte)(pSize));
            header.Add((byte)(Out.Opcode));

            Log.Tcp("Header", header.ToArray(), 0, header.Count);
            SendTCP(header.ToArray());


            // ugly needs to fix
            byte[] buffer;
            long bytesleft = pSize;
            int start = 0;
            while (pSize > 1460)
            {
                if (bytesleft < 1460) break;

                 buffer = new byte[(start + 1460) - start];
                 Out.Read(buffer, start, (start + 1460));
                 SendTCP(buffer);
                start += 1461;
                bytesleft -= 1461;
            }

            if (bytesleft > 0)
            {
                buffer = new byte[(start + bytesleft) - start];
                Out.Read(buffer, start, (int)(start + bytesleft));
                SendTCP(buffer);
            }


        }
    }
}

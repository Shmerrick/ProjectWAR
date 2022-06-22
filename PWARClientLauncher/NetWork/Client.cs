using NLog;
using PWARClientLauncher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using WarZoneLib;

namespace Launcher
{
    public static class Client
    {
        public static int Version = 1;
        public static string LocalServerIP = "127.0.0.1";
        public static int LocalServerPort = 8000;
        public static int TestServerPort = 8000;
        public static bool Started;

        public static string User;
        public static string authToken;
        public static string Language = "English";

        // TCP
        public static Socket _Socket;

        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Print(string Message)
        {
            MainWindow.Acc.Print(Message);
        }

        public static bool Connect(string ip, int port)
        {
            try
            {
                if (_Socket != null)
                    _Socket.Close();

                _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _logger.Info($"Connecting to Launcher Server {ip}:{port}");
                _Socket.Connect(ip, port);

                int size = sizeof(uint);
                uint on = 1;
                uint keepAliveInterval = 10000; //Send a packet once every 10 seconds.
                uint retryInterval = 1000; //If no response, resend every second.
                byte[] inArray = new byte[size * 3];
                Array.Copy(BitConverter.GetBytes(on), 0, inArray, 0, size);
                Array.Copy(BitConverter.GetBytes(keepAliveInterval), 0, inArray, size, size);
                Array.Copy(BitConverter.GetBytes(retryInterval), 0, inArray, size * 2, size);
                _Socket.IOControl(IOControlCode.KeepAliveValues, inArray, null);

                BeginReceive();

                SendCheck();
            }
            catch (Exception e)
            {
                MessageBox.Show("Can not connect to : " + ip + ":" + port + "\n" + e.Message);
                return false;
            }

            return true;
        }

        public static void Close()
        {
            try
            {
                if (_Socket != null)
                    _Socket.Close();
            }
            catch (Exception)
            {
            }
        }

        public static void UpdateLanguage()
        {
            if (Language.Length <= 0)
                return;

            int LangueId = 1;
            switch (Language)
            {
                case "French":
                    LangueId = 2;
                    break;

                case "English":
                    LangueId = 1;
                    break;

                case "Deutch":
                    LangueId = 3;
                    break;

                case "Italian":
                    LangueId = 4;
                    break;

                case "Spanish":
                    LangueId = 5;
                    break;

                case "Korean":
                    LangueId = 6;
                    break;

                case "Chinese":
                    LangueId = 7;
                    break;

                case "Japanese":
                    LangueId = 9;
                    break;

                case "Russian":
                    LangueId = 10;
                    break;
            };

            string CurDir = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(CurDir + "\\..\\user\\");

                StreamReader Reader = new StreamReader("UserSettings.xml");
                string line = "";
                string TotalStream = "";
                while ((line = Reader.ReadLine()) != null)
                {
                    Print(line);
                    int Pos = line.IndexOf("Language id=");
                    if (Pos > 0)
                    {
                        Pos = line.IndexOf("\"") + 1;
                        int Pos2 = line.LastIndexOf("\"");
                        line = line.Remove(Pos, Pos2 - Pos);
                        line = line.Insert(Pos, "" + LangueId);
                    }

                    TotalStream += line + "\n";
                }
                Reader.Close();

                StreamWriter Writer = new StreamWriter("UserSettings.xml", false);
                Writer.Write(TotalStream);
                Writer.Flush();
                Writer.Close();
            }
            catch (Exception e)
            {
                Print("Writing : " + e);
            }
        }

        public static void UpdateRealms()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.CL_INFO);
            SendTCP(Out);
        }

        #region Sender

        // Buffer en train d'être envoyé
        private static byte[] m_tcpSendBuffer = new byte[65000];

        // Liste des packets a sender
        private static readonly Queue<byte[]> m_tcpQueue = new Queue<byte[]>(256);

        // True si un send est en cours
        private static bool m_sendingTcp;

        // Envoi un packet
        public static void SendTCP(PacketOut packet)
        {
            _logger.Info($"Sending TCP Packet {packet.Opcode}");
            //Fix the packet size
            packet.WritePacketLength();

            //Get the packet buffer
            byte[] buf = packet.GetBuffer(); //packet.WritePacketLength sets the Capacity

            //Send the buffer
            SendTCP(buf);
        }

        public static void SendTCP(byte[] buf)
        {
            if (m_tcpSendBuffer == null)
                return;

            //Check if client is connected
            if (_Socket.Connected)
            {
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

                    Buffer.BlockCopy(buf, 0, m_tcpSendBuffer, 0, buf.Length);

                    _Socket.BeginSend(m_tcpSendBuffer, 0, buf.Length, SocketFlags.None, m_asyncTcpCallback, null);
                }
                catch (Exception e)
                {
                    _logger.Trace($"{e.Message}");
                    Close();
                }
            }
        }

        private static readonly AsyncCallback m_asyncTcpCallback = AsyncTcpSendCallback;

        private static void AsyncTcpSendCallback(IAsyncResult ar)
        {
            try
            {
                Queue<byte[]> q = m_tcpQueue;

                int sent = _Socket.EndSend(ar);

                int count = 0;
                byte[] data = m_tcpSendBuffer;

                if (data == null)
                    return;

                lock (q)
                {
                    if (q.Count > 0)
                    {
                        //						Log.WarnFormat("async sent {0} bytes, sending queued packets count: {1}", sent, q.Count);
                        count = CombinePackets(data, q, data.Length);
                    }
                    if (count <= 0)
                    {
                        //						Log.WarnFormat("async sent {0} bytes", sent);
                        m_sendingTcp = false;
                        return;
                    }
                }

                _Socket.BeginSend(data, 0, count, SocketFlags.None, m_asyncTcpCallback, null);
            }
            catch (Exception)
            {
                Close();
            }
        }

        private static int CombinePackets(byte[] buf, Queue<byte[]> q, int length)
        {
            int i = 0;
            do
            {
                var pak = q.Peek();
                if (i + pak.Length > buf.Length)
                {
                    if (i == 0)
                    {
                        q.Dequeue();
                        continue;
                    }
                    break;
                }

                Buffer.BlockCopy(pak, 0, buf, i, pak.Length);
                i += pak.Length;

                q.Dequeue();
            } while (q.Count > 0);

            return i;
        }

        public static void SendTCPRaw(PacketOut packet)
        {
            SendTCP((byte[])packet.GetBuffer().Clone());
        }

        #endregion Sender

        #region Receiver

        private static readonly AsyncCallback ReceiveCallback = OnReceiveHandler;
        private static byte[] _pBuf = new byte[2048];

        private static void OnReceiveHandler(IAsyncResult ar)
        {
            try
            {
                int numBytes = _Socket.EndReceive(ar);
                _logger.Debug($"Recieving {numBytes} bytes");

                if (numBytes > 0)
                {
                    byte[] buffer = _pBuf;
                    int bufferSize = numBytes;

                    PacketIn pack = new PacketIn(buffer, 0, bufferSize);
                    OnReceive(pack);
                    BeginReceive();
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"Exception : {ex.Message}");
            }
        }

        public static void BeginReceive()
        {
            _logger.Debug($"Socket Connected {_Socket.Connected}");

            if (_Socket != null && _Socket.Connected)
            {
                int bufSize = _pBuf.Length;

                if (0 >= bufSize) //Do we have space to receive?
                {
                    Close();
                }
                else
                {
                    _Socket.BeginReceive(_pBuf, 0, bufSize, SocketFlags.None, ReceiveCallback, null);
                }
            }
        }

        #endregion Receiver

        public static void OnReceive(PacketIn packet)
        {
            lock (packet)
            {
                packet.Size = packet.GetUint32();
                packet.Opcode = packet.GetUint8();
                _logger.Debug($"OnReceive Packet size : {packet.Size} opCode : {packet.Opcode}");

                Handle(packet);
            }
        }

        #region Packet

        public static void Handle(PacketIn packet)
        {
            if (!Enum.IsDefined(typeof(Opcodes), (byte)packet.Opcode))
            {
                Print($"Invalid opcode : {packet.Opcode:X02}");
                return;
            }

            _logger.Trace($"HandlePacket{packet}");

            switch ((Opcodes)packet.Opcode)
            {
                case Opcodes.LCR_CHECK:

                    byte Result = packet.GetUint8();

                    switch ((CheckResult)Result)
                    {
                        case CheckResult.LAUNCHER_OK:
                            Start();
                            break;

                        case CheckResult.LAUNCHER_VERSION:
                            string Message = packet.GetString();
                            Print(Message);
                            Close();
                            break;

                        case CheckResult.LAUNCHER_FILE:
                            Client.UpdateWarData(Encoding.ASCII.GetBytes(packet.GetString()));
                            break;
                    }
                    break;

                case Opcodes.LCR_START:

                    MainWindow.Acc.ReceiveStart();

                    byte response = packet.GetUint8();
                    _logger.Debug($"HandlePacket. Response Code : {response}");

                    if (response == 1) //invalud user/pass
                    {
                        _logger.Warn($"Invalid User / Pass");
                        MessageBox.Show("Invalid User / Pass");

                        return;
                    }
                    else if (response == 2) //banned
                    {
                        _logger.Warn($"Account is banned");
                        MessageBox.Show("Account is banned");

                        return;
                    }
                    else if (response == 3) //account not active
                    {
                        _logger.Warn($"Account is not active");
                        MessageBox.Show("Account is not active");

                        return;
                    }
                    else if (response > 3)
                    {
                        _logger.Error($"Unknown Response");
                        MessageBox.Show("Unknown Response");

                        return;
                    }
                    else
                    {
                        authToken = packet.GetString();
                        _logger.Info($"Authentication Token Received : {authToken}");
                        try
                        {
                            var warDirectory = Directory.GetParent(".");
                            MessageBox.Show("Patching..");
                            patchExe();
                            UpdateWarData();
                            MessageBox.Show("Patched. Starting WAR.exe");

                            _logger.Info($"Double checking mythlogin file exists.");
                            if (!File.Exists("mythloginserviceconfig.xml"))
                            {
                                _logger.Warn($"{"mythloginserviceconfig.xml"} does not exist.");
                                MessageBox.Show("Cannot locate mythloginserviceconfig.xml");
                                return;
                            }
                            // Use world.myp to determine whether we are in the correct directory.
                            if (!File.Exists(warDirectory.FullName + "\\world.myp"))
                            {
                                _logger.Warn($"{warDirectory.FullName + "\\world.myp"} does not exist.");
                                MessageBox.Show("Is your launcher in the Launcher folder?");
                                return;
                            }

                            _logger.Info($"Starting Client {warDirectory.FullName}\\WAR.exe");

                            if (MainWindow.Acc.AllowWarClientLaunch)
                            {
                                Process process = new Process
                                {
                                    StartInfo =
                                    {
                                        WorkingDirectory = warDirectory.FullName,
                                        FileName = "WAR.exe",
                                        Arguments = " --acctname=" + Convert.ToBase64String(Encoding.ASCII.GetBytes(User)) + " --sesstoken=" +
                                                    Convert.ToBase64String(Encoding.ASCII.GetBytes(authToken))
                                    }
                                };
                                _logger.Info($"Starting process WAR.exe (in {warDirectory})");
                                process.Start();
                                Directory.SetCurrentDirectory(warDirectory.FullName);
                            }
                            else
                            {
                                _logger.Info($"Not launching WAR.Exe (in {warDirectory}) " + " --acctname=" + Convert.ToBase64String(Encoding.ASCII.GetBytes(User)) + " --sesstoken=" +
                                             Convert.ToBase64String(Encoding.ASCII.GetBytes(authToken)));
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Info($"Failed to start Client {e.ToString()}");
                            MessageBox.Show("Failed to start client.");
                        }
                    }

                    break;

                case Opcodes.LCR_INFO:
                    {
                        _logger.Info($"Processing LCR_INFO : Number Realms : {packet.GetUint8()} Name : {packet.GetString()} Parsed : {packet.GetParsedString()}");
                    }
                    break;

                case Opcodes.LCR_CREATE:

                    byte respons = packet.GetUint8();
                    _logger.Debug($"HandlePacket. Response Code : {respons}");

                    if (respons == 0) //invalud user/pass
                    {
                        _logger.Warn($"Account Name busy!");
                        MessageBox.Show("Account Name busy!");

                        return;
                    }
                    else if (respons == 1) //success
                    {
                        _logger.Warn($"Account created!");
                        MessageBox.Show("Account created!");
                        return;
                    }
                    else if (respons == 2) //banned
                    {
                        _logger.Warn($"Account is banned!");
                        MessageBox.Show("Account is banned!");

                        return;
                    }

                    break;
            }
        }

        public static void Start()
        {
            if (Started)
                return;

            Started = true;
        }

        public static void SendCheck()
        {
            _logger.Info("Starting SendCheck (CL_CHECK)");
            PacketOut Out = new PacketOut((byte)Opcodes.CL_CHECK);
            Out.WriteUInt32((uint)Version);

            FileInfo Info = new FileInfo("mythloginserviceconfig.xml");
            if (Info.Exists)
            {
                Out.WriteByte(1);
                Out.WriteUInt64((ulong)Info.Length);
            }
            else
            {
                Out.WriteByte(0);
            }

            SendTCP(Out);
        }

        public static void patchExe()
        {
            if (MainWindow.Acc.AllowServerPatch)
            {
                _logger.Info("Patching WAR.exe");
                using (Stream stream = new FileStream(Directory.GetCurrentDirectory() + "\\..\\WAR.exe", FileMode.OpenOrCreate))
                {
                    int encryptAddress = (0x00957FBE + 3) - 0x00400000;
                    stream.Seek(encryptAddress, SeekOrigin.Begin);
                    stream.WriteByte(0x01);

                    //0x90 == 144
                    //0x57 == 87
                    //0x8B == 139
                    //0xF8 == 248
                    //0xEB == 235
                    //0x32 == 50

                    //0x934b468a ==147.75.70.138

                    byte[] decryptPatch1 = { 0x90, 0x90, 0x90, 0x90, 0x57, 0x8B, 0xF8, 0xEB, 0x32 };
                    int decryptAddress1 = (0x009580CB) - 0x00400000;
                    stream.Seek(decryptAddress1, SeekOrigin.Begin);
                    stream.Write(decryptPatch1, 0, 9);

                    byte[] decryptPatch2 = { 0x90, 0x90, 0x90, 0x90, 0xEB, 0x08 };
                    int decryptAddress2 = (0x0095814B) - 0x00400000;
                    stream.Seek(decryptAddress2, SeekOrigin.Begin);
                    stream.Write(decryptPatch2, 0, 6);

                    //stream.WriteByte(0x01);
                }
                _logger.Info("Done patching WAR.exe");
            }
            else
            {
                _logger.Info("Not Patching WAR.exe");
            }
        }

        public static void UpdateWarData(byte[] data)
        {
            try
            {
                _logger.Info("Updating mythloginserviceconfig.xml");
                using (FileStream fs = new FileStream("mythloginserviceconfig.xml", FileMode.CreateNew, FileAccess.Write))
                {
                    fs.Write(data, 0, data.Length);
                }
            }
            catch (Exception e)
            {
                Print(e.ToString());
                _logger.Info(e.ToString());
            }
        }

        public static void UpdateWarData()
        {
            try
            {
                _logger.Info("Updating data.myp");
                FileStream fileStream = File.Open("..\\data.myp", FileMode.Open, FileAccess.ReadWrite);
                MYP myp = new MYP(MythicPackage.ART, (Stream)fileStream);

                if (File.Exists("\\mythloginserviceconfig.xml") == false)
                {
                    _logger.Error("Missing file : mythloginserviceconfig.xml");
                    return;
                }
                using (FileStream reader = new FileStream("\\mythloginserviceconfig.xml", FileMode.Open))
                {
                    byte[] data = new byte[reader.Length];
                    reader.Read(data, 0, data.Length);
                    myp.UpdateFile(0x0B3E7AC0C6762BF7, data);
                }
                myp.Save();
                fileStream.Close();
            }
            catch (Exception e)
            {
                Print(e.ToString());
                _logger.Info(e.ToString());
            }
        }

        #endregion Packet
    }
}
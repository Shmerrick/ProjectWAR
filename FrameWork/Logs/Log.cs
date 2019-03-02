using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using NLog;

namespace FrameWork
{
    public static class Log
    {
        private static readonly Encoding encoding = new UTF8Encoding(true);
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static LogConfig _config = new LogConfig();
        private static Thread _thread;

        private class Entry
        {
            public string Name, Message;
            public DateTime Time;
            public ConsoleColor Color;
        };

        private static List<Entry> _entries = new List<Entry>();
        private static ManualResetEventSlim _entriesEvent = new ManualResetEventSlim(false);
        private static object _writeLock = new object();

        static bool isMono = Type.GetType("Mono.Runtime") != null;

        static Entry getNextEntry()
        {
            while (true)
            {
                _entriesEvent.Wait();

                lock (_writeLock)
                {
                    if (_entries.Count > 0)
                    {
                        // Get the next entry
                        Entry entry = _entries[0];
                        _entries.RemoveAt(0);

                        // If we ran out of log entries, reset the event
                        if (_entries.Count == 0)
                            _entriesEvent.Reset();

                        return entry;
                    }
                    else
                    {
                        // Woke up for no reason, reset the event so it doesn't happen again
                        _entriesEvent.Reset();
                    }
                }
            }
        }

        private static void threadStart()
        {
            while (true)
            {
                try
                {
                    Entry entry = getNextEntry();

                    if (entry != null)
                        Output(entry.Name, entry.Message, entry.Color);
                }
                catch
                {
                    // Can't safely output anything
                }
            }
        }

        static void AdjustConsoleHeight()
        {
            int h = Console.WindowWidth - 20;
            if (Console.BufferHeight != h)
                Console.BufferHeight = h;
        }

        private static void Output(string name, string message, ConsoleColor color)
        {
            Output(name, message, color, DateTime.UtcNow);
        }

        private static void Output(string name, string message, ConsoleColor color, DateTime time)
        {
            string text = "[" + time.ToString("HH:mm:ss") + "] " + name + " : " + message;

            if (!isMono)
                AdjustConsoleHeight();

            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.White;

            if (DumpFile != null && FSDump != null)
            {
                byte[] info = encoding.GetBytes(text + "\n");
                FSDump.Write(info, 0, info.Length);
                FSDump.Flush();
            }
        }

        public static LogConfig Config
        {
            get
            {
                return _config;
            }
        }

        private static FileInfo DumpFile;
        private static FileStream FSDump;

        public static void Init(LogConfig Config)
        {
            InitInstance(Config);
        }

        public static bool InitLog(string LogConf, string PrefileName)
        {
            try
            {
                LogConfig Conf = new LogConfig(0);
                Conf.PreFileName = PrefileName;

                if (LogConf.Length > 0)
                    Conf.LoadInfoFromFile(LogConf);

                Init(Conf);
            }
            catch (Exception e)
            {
                Error("InitLog", "Error : " + e);
                return false;
            }

            Notice("InitLog", "Logger initialized");
            return true;
        }

        public static bool InitLog(LogInfo Info, string PrefileName)
        {
            LogConfig Config = new LogConfig(Info);
            Config.PreFileName = PrefileName;
            Init(Config);
            return true;
        }

        public static void InitInstance(LogConfig Config)
        {
            try
            {
                if (Config == null)
                    Config = new LogConfig();

                string FileDir = Directory.GetCurrentDirectory() + Config.LogFolder;
                string BackDir = Directory.GetCurrentDirectory() + Config.LogFolder + "/Backup";

                try
                {
                    Directory.CreateDirectory(FileDir);
                    Directory.CreateDirectory(BackDir);
                }
                catch (Exception)
                {

                }

                FileDir += "/" + Config.PreFileName + Config.FileName;
                BackDir += "/" + Config.PreFileName + "." + DateTime.UtcNow.Hour + "h." + DateTime.UtcNow.Minute + "m." + DateTime.UtcNow.Second + "s" + Config.FileName;

                if (DumpFile == null)
                {
                    DumpFile = new FileInfo(FileDir);
                    if (DumpFile.Exists)
                        DumpFile.MoveTo(BackDir);

                    DumpFile = new FileInfo(FileDir);

                    if (FSDump != null)
                        FSDump.Close();

                    FSDump = DumpFile.Create();
                }

                if (Config != null)
                    _config = Config;
            }
            catch (Exception)
            {
                Console.WriteLine("Log : Log file already in use.");

                if (Config != null)
                    Config.Info.Dump = false;
            }

            _thread = new Thread(threadStart);
            _thread.IsBackground = true; // Detach the thread so it won't block exit
            _thread.Priority = ThreadPriority.BelowNormal;
            _thread.Start();
        }

        public static void XTexte(string name, string message, ConsoleColor Color, bool sync = false)
        {
            if (sync)
            {
                Output(name, message, Color);
            }
            else
            {
                Entry e = new Entry();
                e.Name = name;
                e.Message = message;
                e.Color = Color;

                lock (_writeLock)
                {
                    e.Time = DateTime.UtcNow;
                    _entries.Add(e);
                    _entriesEvent.Set();
                }
            }
        }

        public static void Enter() // Saute une ligne
        {
            lock (_config)
            {
                Console.WriteLine("");
            }
        }

        public static void Info(string name, string message, bool sync = false)
        {
            if (_config.Info.Info)
                Log._logger.Info("I "+ name + " " + message, ConsoleColor.White, sync);
        }

        public static void Info(string name, string message, ConsoleColor c)
        {
            if (_config.Info.Info)
                Log._logger.Info("I " + name +" "+ message, c);
        }

        public static void Success(string name, string message, bool sync = false)
        {
            if (_config.Info.Successs)
                Log._logger.Info("S " + name + " " + message, ConsoleColor.Green, sync);
        }

        public static void Notice(string name, string message, bool sync = false)
        {
            if (_config.Info.Notice)
                Log._logger.Warn("N " + name + " " + message, ConsoleColor.Yellow, sync);
        }

        public static void Error(string name, string message, bool sync = false)
        {
            if (_config.Info.Error)
                Log._logger.Error("E " + name + " " + message, ConsoleColor.Red, sync);
        }

        public static void Debug(string name, string message, bool sync = false)
        {
            if (_config.Info.Debug)
                Log._logger.Debug("D " + name + " " + message, ConsoleColor.Blue, sync);
        }

        public static void Dump(string name, string message, bool sync = false)
        {
            if (_config.Info.Dump)
                Log._logger.Trace("D " + name + " " + message, ConsoleColor.Gray, sync);
        }

        public static bool CanDump()
        {
            return _config.Info.Dump;
        }

        public static void Tcp(string name, MemoryStream Packet, bool Force = false, bool sync = false)
        {
            if (Force || _config.Info.Tcp)
            {
                byte[] Buff = Packet.ToArray();
                Log._logger.Trace("P " + name +" "+ Hex(Buff, 0, Buff.Length), ConsoleColor.Gray, sync);
            }
        }

        public static void Tcp(string name, byte[] dump, int start, int len, bool Force=false, bool sync = false)
        {
            if (Force || _config.Info.Tcp)
                Log._logger.Trace("P " + " " + name, Hex(dump, start, len), ConsoleColor.Gray, sync);
        }

        public static void Dump(string name, MemoryStream Packet, bool Force = false, bool sync = false)
        {
            if (Force || _config.Info.Dump)
            {
                byte[] Buff = Packet.ToArray();
                Log._logger.Trace("U " + " " + name, Hex(Buff, 0, Buff.Length), ConsoleColor.Gray, sync);
            }
        }

        public static void Dump(string name, byte[] dump, int start, int len, bool Force = false, bool sync = false)
        {
            if (_config.Info.Dump)
                Log._logger.Trace("U " + " " + name, Hex(dump,start,len), ConsoleColor.Gray, sync);
        }

        public static void Compare(string Name, byte[] First, byte[] Second, bool sync = false)
        {
            if (_config.Info.Dump)
                return;

            if (First.Length != Second.Length)
                Error("Name", "First.Length(" + First.Length + ") != Second.Length(" + Second.Length + ")");

            StringBuilder hex = new StringBuilder();

            for (int i = 0; i < Math.Max(First.Length, Second.Length); i += 16)
            {
                hex.Append("\n");

                bool LastDiff = false;
                for (int j = 0; j < 16; ++j)
                {
                    if (j + i < First.Length)
                    {
                        if (j + i < Second.Length)
                        {
                            if (First[j + i] != Second[j + i] && !LastDiff)
                            {
                                LastDiff = true;
                                hex.Append("[");
                            }
                            else if (First[j + i] == Second[j + i] && LastDiff)
                            {
                                LastDiff = false;
                                hex.Append("]");
                            }
                        }
                        else if (LastDiff)
                        {
                            LastDiff = false;
                            hex.Append("]");
                        }
                          

                        byte val = First[j + i];
                        //hex.Append(" ");
                        hex.Append(First[j + i].ToString("X2"));
                        if (j == 3 || j == 7 || j == 11)
                            hex.Append("");
                    }
                    else
                    {
                        hex.Append("  ");
                    }
                }
                if (LastDiff)
                {
                    LastDiff = false;
                    hex.Append("]");
                }

                hex.Append(" || ");

                LastDiff = false;
                for (int j = 0; j < 16; ++j)
                {
                    if (j + i < Second.Length)
                    {
                        if (j + i < First.Length)
                        {
                            if (First[j + i] != Second[j + i] && !LastDiff)
                            {
                                LastDiff = true;
                                hex.Append("[");
                            }
                            else if (First[j + i] == Second[j + i] && LastDiff)
                            {
                                LastDiff = false;
                                hex.Append("]");
                            }
                        }
                        else if (LastDiff)
                        {
                            LastDiff = false;
                            hex.Append("]");
                        }

                        byte val = Second[j + i];
                        //hex.Append(" ");
                        hex.Append(Second[j + i].ToString("X2"));
                        if (j == 3 || j == 7 || j == 11)
                            hex.Append("");
                    }
                    else
                    {
                        if (LastDiff)
                        {
                            LastDiff = false;
                            hex.Append("]");
                        }
  
                        hex.Append("  ");
                    }
                }
            }

            Log._logger.Trace("C " + Name+" " + hex.ToString(), ConsoleColor.Gray, sync);
        }

        public static string Hex(byte[] dump, int start, int len)
        {
            var hexDump = new StringBuilder();

            try
            {
                int end = start + len;
                for (int i = start; i < end; i += 16)
                {
                    StringBuilder text = new StringBuilder();
                    StringBuilder hex = new StringBuilder();
                    hex.Append("\n");

                    for (int j = 0; j < 16; j++)
                    {
                        if (j + i < end)
                        {
                            byte val = dump[j + i];
                            hex.Append(" ");
                            hex.Append(dump[j + i].ToString("X2"));
                            if (j == 3 || j == 7 || j == 11)
                                hex.Append(" ");
                            if (val >= 32 && val <= 127)
                            {
                                text.Append((char)val);
                            }
                            else
                            {
                                text.Append(".");
                            }
                        }
                        else
                        {
                            hex.Append("   ");
                            text.Append("  ");
                        }
                    }
                    hex.Append("  ");
                    hex.Append("//"+text);
                    hexDump.Append(hex);
                }
            }
            catch (Exception e)
            {
                Error("HexDump", e.ToString());
            }

            return hexDump.ToString();
        }
    }
}

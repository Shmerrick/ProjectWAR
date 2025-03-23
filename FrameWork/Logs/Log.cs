using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace FrameWork
{
    public static class Log
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static LogConfig _config = new LogConfig();

        public static bool InitLog(LogInfo Info, string PrefileName)
        {
            LogConfig Config = new LogConfig(Info);
            Config.PreFileName = PrefileName;
            InitInstance(Config);
            return true;
        }

        public static void InitInstance(LogConfig Config)
        {
            try
            {
                if (Config == null)
                    Config = new LogConfig();

                if (Config != null)
                    _config = Config;
            }
            catch (Exception)
            {
                Console.WriteLine("Log : Log file already in use.");

                if (Config != null)
                    Config.Info.Dump = false;
            }
        }

        public static void Info(string name, string message, bool sync = false)
        {
            if (_config.Info.Info)
                _logger.Info(name + " " + message, ConsoleColor.White, sync);
        }

        public static void Info(string name, string message, ConsoleColor c)
        {
            if (_config.Info.Info)
                _logger.Info(name + " " + message, c);
        }

        public static void Success(string name, string message, bool sync = false)
        {
            if (_config.Info.Successs)
                _logger.Info(name + " " + message, ConsoleColor.Green, sync);
        }

        public static void Notice(string name, string message, bool sync = false)
        {
            if (_config.Info.Notice)
                _logger.Warn(name + " " + message, ConsoleColor.Yellow, sync);
        }

        public static void Error(string name, string message, bool sync = false)
        {
            if (_config.Info.Error)
                _logger.Error(name + " " + message, ConsoleColor.Red, sync);
        }

        public static void Debug(string name, string message, bool sync = false)
        {
            if (_config.Info.Debug)
                _logger.Debug(name + " " + message, ConsoleColor.Blue, sync);
        }

        public static void Dump(string name, string message, bool sync = false)
        {
            if (_config.Info.Dump)
                _logger.Trace(name + " " + message, ConsoleColor.Gray, sync);
        }

        public static void Tcp(string name, MemoryStream Packet, bool Force = false, bool sync = false)
        {
            if (Force || _config.Info.Tcp)
            {
                byte[] Buff = Packet.ToArray();
                _logger.Trace(name + " " + Hex(Buff, 0, Buff.Length), ConsoleColor.Gray, sync);
            }
        }

        public static void Tcp(string name, byte[] dump, int start, int len, bool Force = false, bool sync = false)
        {
            if (Force || _config.Info.Tcp)
                _logger.Trace(name, Hex(dump, start, len), ConsoleColor.Gray, sync);
        }

        public static void Dump(string name, MemoryStream Packet, bool Force = false, bool sync = false)
        {
            if (Force || _config.Info.Dump)
            {
                byte[] Buff = Packet.ToArray();
                _logger.Trace(name, Hex(Buff, 0, Buff.Length), ConsoleColor.Gray, sync);
            }
        }

        public static void Dump(string name, byte[] dump, int start, int len, bool Force = false, bool sync = false)
        {
            if (_config.Info.Dump)
                _logger.Trace(name, Hex(dump, start, len), ConsoleColor.Gray, sync);
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

            _logger.Trace(Name + " " + hex.ToString(), ConsoleColor.Gray, sync);
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
                    hex.Append("//" + text);
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
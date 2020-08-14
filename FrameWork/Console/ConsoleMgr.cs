using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading;

namespace FrameWork
{
    public class ConsoleMgr
    {
        public readonly Dictionary<string, IConsoleHandler> m_consoleHandlers = new Dictionary<string,IConsoleHandler>();

        private static ConsoleMgr Instance;
        private bool _IsRunning = true;
        private DateTime _start_time = DateTime.UtcNow;

        public static void Start()
        {
            if (Instance != null)
                return;

            Instance = new ConsoleMgr();

            try
            {
                Instance.LoadConsoleHandler();
            }
            catch (Exception e)
            {
                Log.Error("ConsoleMgr", "Can not load : " + e);
            }

            while (Instance._IsRunning)
            {
                string line;

                try
                {
                    line = Console.ReadLine();
                }
                catch
                {
                    // Closed console
                    break;
                }

                if ( line == null )
                {
                    break;
                }

                    
                if (line.StartsWith("."))
                {
                    line = line.Substring(1);

                    if (!Instance.ExecuteCommand(line))
                        Log.Error("ConsoleMgr", "Command not found");
                }else CleanLine(line.Length);
            }
        }

        public static void CleanLine(int size)
        {
            string clear = new string(' ',size);
            Console.CursorLeft = 0;
            Console.CursorTop -= 1;
            Console.Write(clear);
            Console.CursorLeft = 0;
        }

        public static void Stop()
        {
            Instance._IsRunning = false;
        }

        public static void WaitAndExit(int WaitTime)
        {
            Thread.Sleep(WaitTime);
            Environment.Exit(0);
        }

        public static string GetUptime
        {
            get
            {
                DateTime Time = new DateTime(DateTime.UtcNow.Ticks - Instance._start_time.Ticks);
                return Time.ToString("T");
            }
        }

        private void LoadConsoleHandler()
        {
            Log.Info("ConsoleMgr", "Commands list : ");

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Log.Dump("ConsoleMgr", "Attempting to load : " +  assembly.FullName);
                foreach (Type type in assembly.GetTypes())
                {
                    Log.Dump("ConsoleMgr", "Attempting to load : " + type.FullName);
                    // Pick up a class
                    if (type.IsClass != true)
                        continue;

                    object[] attrib = type.GetCustomAttributes(typeof(ConsoleHandlerAttribute), true);
                    if (attrib.Length <= 0)
                        continue;

                    var consoleHandlerAttribs =
                        (ConsoleHandlerAttribute[])type.GetCustomAttributes(typeof(ConsoleHandlerAttribute), true);

                    if (consoleHandlerAttribs.Length > 0)
                    {
                        Log.Info("ConsoleMgr", "." + consoleHandlerAttribs[0].Command + " : " + consoleHandlerAttribs[0].Description);
                        RegisterHandler(consoleHandlerAttribs[0].Command, (IConsoleHandler)Activator.CreateInstance(type));
                    }
                    Log.Dump("ConsoleMgr", type.FullName);
                }
                Log.Dump("ConsoleMgr", assembly.FullName);
            }
        }

        private void RegisterHandler(string command, IConsoleHandler Handler)
        {
            m_consoleHandlers.Add(command,Handler);
        }

        private bool ExecuteCommand(string line)
        {
            string command;
            List<string> args = new List<string>();

            int a = line.IndexOf(' ');

            if (a == -1)
                a = line.Length;

            command = line.Substring(0, a);
            line = line.Remove(0,a);

            if (command.Length <= 0)
                return false;
            
            IConsoleHandler Handler = null;

            if (!Instance.m_consoleHandlers.ContainsKey(command))
                return false;

            Handler = Instance.m_consoleHandlers[command];

            string[] Args = line.Split(' ');

            foreach (string str in Args)
                if( str.Length > 1 || (str.Length == 1 && str[0] != ' ') )
                    args.Add(str);

            var consoleHandlerAttribs = (ConsoleHandlerAttribute[])Handler.GetType().GetCustomAttributes(typeof(ConsoleHandlerAttribute), true);

            if (consoleHandlerAttribs[0].ArgCount != 0 && consoleHandlerAttribs[0].ArgCount > args.Count)
                Log.Error("ConsoleMgr", "Invalid parameter count : " + args.Count + " / " + consoleHandlerAttribs[0].ArgCount);
            else
            {
                try
                {
                    if (!Handler.HandleCommand(command, args))
                        Log.Error("ConsoleMgr", "Invalid command parameter !");
                }
                catch (Exception e)
                {
                    Log.Error("ConsoleMgr", e.ToString());
                }
            }

            return true;
        }

        static int GetInt(List<string> args)
        {
            if (args.Count <= 0)
                return -999;

           int result = int.Parse(args[0]);
           args.RemoveAt(0);

           return result;
        }

        static bool GetBool(List<string> args)
        {
            return GetInt(args) > 0;
        }

        static string GetTotalString(List<string> args, int num)
        {
            string Total = "";

            foreach (string str in args)
                Total = Total + " " + str;

            Total = Total.Remove(1);

            args.Clear();

            return Total;
        }

    }
}

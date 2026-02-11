using System;
using System.IO;
using System.Reflection;

namespace FrameWork.Misc
{
    public class CrashGuard
    {
        public static string GetRoot()
        {
            try
            {
                return Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            }
            catch
            {
                return "";
            }
        }

        public static string GetTimeStamp()
        {
            DateTime now = DateTime.Now;

            return String.Format("{0}-{1}-{2}-{3}-{4}-{5}",
                now.Day,
                now.Month,
                now.Year,
                now.Hour,
                now.Minute,
                now.Second
            );
        }
        
        public static string Combine(string path1, string path2)
        {
            if (path1.Length == 0)
                return path2;

            return Path.Combine(path1, path2);
        }

        public static void GenerateCrashReport(UnhandledExceptionEventArgs e)
        {
            System.Console.WriteLine("Crash: Generating report...");

            try
            {
                string timeStamp = GetTimeStamp();
                string fileName = String.Format("{1}-Crash {0}.log", timeStamp, Assembly.GetExecutingAssembly().GetName());

                string root = GetRoot();
                string filePath = Combine(root, fileName);

                using (StreamWriter op = new StreamWriter(filePath))
                {
                    Version ver = Assembly.GetCallingAssembly().GetName().Version;

                    op.WriteLine("Server Crash Report");
                    op.WriteLine("===================");
                    op.WriteLine();
                    op.WriteLine("App Version {0}.{1}, Build {2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
                    op.WriteLine("Operating System: {0}", Environment.OSVersion);
                    op.WriteLine(".NET Framework: {0}", Environment.Version);
                    op.WriteLine("Time: {0}", DateTime.Now);

                    op.WriteLine("Exception:");
                    op.WriteLine(e.ExceptionObject);
                    op.WriteLine();
                }

                System.Console.WriteLine("done");
            }
            catch
            {
                System.Console.WriteLine("failed");
            }
        }
    }
}
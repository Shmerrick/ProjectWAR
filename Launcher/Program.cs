using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Launcher
{
    static class Program
    {
        public static string WarFolder = Application.StartupPath;
        /// <summary>
        /// Main entry point of the application.
        /// </ summary>
        [STAThread]
        static void Main(string[] args)
        {
            var allowLocal = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "local")
                    allowLocal = true;

                if (args[i].ToLower() == "warfolder" && i + 1 < args.Length)
                {
                    WarFolder = args[++i];
                    if (!Directory.Exists(WarFolder))
                        throw new Exception($"War foulder {WarFolder} not found");
                }
            }       

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ApocLauncher(allowLocal));


        }
    }
}

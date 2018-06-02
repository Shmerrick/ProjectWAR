using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Launcher
{
    static class Program
    {
        /// <summary>
        /// Main entry point of the application.
        /// </ summary>
        [STAThread]
        static void Main(string[] args)
        {
            var allowLocal = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[0] == "local")
                    allowLocal = true;

            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ApocLauncher(allowLocal));


        }
    }
}

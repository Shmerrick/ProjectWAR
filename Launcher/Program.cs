using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CommandLine;

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
            var loadLocal = false;
            var allowMYPPatch = true;
            var allowServerPatch = false;
            var allowWarClientLaunch = true;

            Parser.Default.ParseArguments<CommandLineOptions>(args).WithParsed<CommandLineOptions>(o =>
            {
                loadLocal = o.LaunchLocal;
                allowMYPPatch = o.NoClientPatch;
                allowServerPatch = o.NoServerPatch;
                allowWarClientLaunch = o.NoLaunch;
            });
                

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ApocLauncher(loadLocal, allowMYPPatch, allowServerPatch, allowWarClientLaunch));


        }
    }
}

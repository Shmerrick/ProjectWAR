using System;
using System.Windows.Forms;

namespace Launcher
{
    internal static class Program
    {
        /// <summary>
        /// Main entry point of the application.
        /// </ summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WarLauncher());
        }
    }
}
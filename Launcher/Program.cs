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
        static void Main()
        {            
            //if (!Client.Connect())
            //    Environment.Exit(0);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Accueil());


        }
    }
}

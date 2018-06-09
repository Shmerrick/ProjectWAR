using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApocalypseAPI
{
    public class Program
    {
        //public static void Main(string[] args)
        //{
        //    BuildWebHost(args).Run();
        //}

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //    .UseKestrel()
        //        .UseStartup<Startup>()
        //        .Build();
        public static IConfiguration Configuration { get; set; }

        public static void Main(string[] args)
        {
            //var builder = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("appsettings.json");

            //Configuration = builder.Build();

            var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var directoryPath = Path.GetDirectoryName(exePath);

            //var host = new WebHostBuilder()
            //    .UseKestrel()
            //    .UseContentRoot(directoryPath)
            //    .UseStartup<Startup>()
            //    .CaptureStartupErrors(true)
            //    .Build();

            var host = WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                    .UseStartup<Startup>()
                    .Build();

            if (Debugger.IsAttached || args.Contains("--debug=true"))
            {
                // Can run as ApocalypseAPI.exe --debug=true
                host.Run();
            }
            else
            {
                //sc create MyService binPath= g:\temp\1\ApocalypseAPI.exe
                host.RunAsService();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Launcher
{
    public class ServerConfig
    {
        public string ZoneDirectoryLocation { get; set; }


        public ServerConfig()
        {
            ZoneDirectoryLocation = "Hi There";
        }

        


        public string GetTestIpAddress()
        {

            var IpAddress = System.Configuration.ConfigurationManager.AppSettings["ServerTestIPAddress"];

            return IpAddress;

            //string ServPath = Directory.GetCurrentDirectory();
            //if (!File.Exists(ServPath))
            //{

            //}
        }
    }
}


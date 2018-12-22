using CommandLine;

namespace Launcher
{
    public class CommandLineOptions
    {
        [Option("nolaunch", HelpText = "Do not launcher the WAR client")]
        public bool NoLaunch { get; set; }
        [Option("noclientpatch", HelpText = "Do not patch the WAR client")]
        public bool NoClientPatch { get; set; }
        [Option("noserverpatch", HelpText = "Do not patch the WAR client (crypt)")]
        public bool NoServerPatch { get; set; }
        [Option("local", HelpText = "Launch for local WAR Server")]
        public bool LaunchLocal { get; set; }


    }
}

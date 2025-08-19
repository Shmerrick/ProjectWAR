using AccountCacher;
using Common;
using FrameWork;
using FrameWork.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading;
using WorldServer.Configs;
using WorldServer.Managers;
using WorldServer.NetWork;
using WorldServer.Services.World;
using WorldServer.World.Auction;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Objects;

namespace WorldServer
{
    // This is like the big boss of our game world. It starts everything up and keeps it running.
    internal class Core
    {
        // This holds all the settings for our game world, like how much experience you get for defeating a monster.
        public static WorldConfigs Config;
        // This holds all the settings for player accounts.
        public static AccountConfig AccountConfig;
        // This is a helper that lets our game world talk to the account server.
        public static RpcClient Client;
        // This is a shortcut to get to the account manager, which handles all the player accounts.
        public static AccountMgr AcctMgr => Client?.GetServerObject<AccountMgr>();
        // This is the main server that listens for players trying to connect to the game.
        public static TCPServer Server;
        // This represents the "realm" or the specific server that players are on.
        public static Realm Rm;
        // This is a timer that does things at a specific time, like checking if auctions have ended.
        private static Timer _timer;
        // This is our game server program.
        private static Process m_Process;

        // This tells us where the game server program is located on the computer.
        public static string ExePath
        {
            get
            {
                return Assembly.GetEntryAssembly().Location;
            }
        }

        // These are like on/off switches for different modes the server can run in.
        // Debug mode helps us find and fix problems.
        public static bool Debug { get; private set; }
        // Dev mode is for when we are building new things for the game.
        public static bool Dev { get; private set; }
        // HighPriority mode makes the game server run faster.
        public static bool HighPriority { get; private set; }
        // LoadPhysics mode turns on the game's physics, like how objects fall.
        public static bool LoadPhysics { get; private set; }

        // This helps us measure time very accurately.
        private static readonly double _HighFrequency = 1000.0 / Stopwatch.Frequency;
        // This gives us the current time in the game world.
        public static long TickCount => (long)Ticks;

        // This gives us the current time in a very precise way.
        public static double Ticks => Stopwatch.GetTimestamp() * _HighFrequency;

        // This gets all the special settings we started the server with, like "-debug" or "-dev".
        public static string Arguments
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (Debug)
                    Utils.Separate(sb, "-debug", " ");

                if (Dev)
                    Utils.Separate(sb, "-dev", " ");

                if (HighPriority)
                    Utils.Separate(sb, "-priority", " ");

                if (LoadPhysics)
                    Utils.Separate(sb, "-physics", " ");

                return sb.ToString();
            }
        }

        // This is where everything starts when we run the game server.
        [STAThread]
        private static void Main(string[] args)
        {
            // This makes sure that if something bad happens, we have a way to handle it.
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);
            // This is for when someone tries to close the server, we can do some cleanup first.
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnClose);
            // This gets the current program that is running.
            m_Process = Process.GetCurrentProcess();

            Log.Info("", "-------------------- World Server ---------------------", ConsoleColor.DarkRed);

            // We are setting up the server to run in a special mode for developers.
            Debug = true;
            HighPriority = true;
            Dev = true;
            LoadPhysics = true;

            if (Dev)
            {
                WorldMgr.ServerMode = "DEV"; // Developer mode
            }
            else
            {
                WorldMgr.ServerMode = "PRD"; // Production (live) mode
            }

            // This gets the version of our game server.
            Version ver = Assembly.GetEntryAssembly().GetName().Version;

            // This prints out the version information to the screen.
            Utils.PushColor(ConsoleColor.Cyan);
            Console.WriteLine("DagonUO Version {0}.{1}, Build {2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
            Console.WriteLine("Core: Running on .NET Framework Version {0}.{1}.{2}", Environment.Version.Major, Environment.Version.Minor, Environment.Version.Build);
            Utils.PopColor();
            Console.WriteLine("ARGOG DAN");
            Utils.PushColor(ConsoleColor.Cyan);
            string s = Arguments;

            if (s.Length > 0)
                Console.WriteLine("Core: Running with arguments: {0}", s);

            try
            {
                // If we are in high priority mode, we tell the computer to give our server more attention.
                if (HighPriority)
                {
                    Console.WriteLine("Core: Set process priority to Above Normal");
                    System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
                }
            }
            catch (Exception ex)
            {
                // If something went wrong, we would write it down here.
                //Server.Diagnostics.ExceptionLogging.LogException(ex);
            }

            // This checks if the server is using a special way to clean up memory, which is good for performance.
            if (GCSettings.IsServerGC)
                Console.WriteLine("Core: Server garbage collection mode enabled");

            Log.Info("", "Core: running in " + WorldMgr.ServerMode + " mode");
            Utils.PopColor();
            Utils.PushColor(ConsoleColor.Gray);
            // This loads all our settings from files.
            ConfigMgr.LoadConfigs();
            Config = ConfigMgr.GetConfig<WorldConfigs>();
            AccountConfig = ConfigMgr.GetConfig<AccountConfig>();

            // This sets up the logging system, so we can write down what's happening in the server.
            if (!Log.InitLog(Config.LogLevel, "WorldServer"))
                ConsoleMgr.WaitAndExit(2000);

#if DEBUG
            // This is a special tool for developers to look at what's happening inside the server while it's running.
            API.Server api = null;
            if (Config.EnableAPI)
            {
                try
                {
                    api = new API.Server(Config.APIAddress, Config.APIPort, 100);
                }
                catch (Exception e)
                {
                    Log.Error("API", "Unable to start API server: " + e.Message);
                }
            }
#endif

            // This connects to the database that stores all the character information.
            CharMgr.Database = DBManager.Start(Config.CharacterDatabase.Total(), Config.CharacterDatabase.ConnectionType, "Characters", Config.CharacterDatabase.Database);
            if (CharMgr.Database == null)
                ConsoleMgr.WaitAndExit(2000);
            else if (!Config.PreloadAllCharacters)
                CharMgr.Database.RegisterAction(CharMgr.LoadPendingCharacters);

            // This starts a timer to check for expired auctions every 12 hours.
            _timer = new Timer(AuctionHouse.CheckAuctionExpiry, null, new TimeSpan(0, 12, 0, 0), new TimeSpan(0, 24, 0, 0));

            // This connects to the database that stores all the world information, like maps and monsters.
            WorldMgr.Database = DBManager.Start(Config.WorldDatabase.Total(), Config.CharacterDatabase.ConnectionType, "World", Config.WorldDatabase.Database);
            if (WorldMgr.Database == null)
                ConsoleMgr.WaitAndExit(2000);

            // This randomly picks a starting area for the big realm vs realm battles.
            WorldMgr.StartingPairing = WorldMgr.Database.ExecuteQueryInt("SELECT FLOOR(RAND() * 3) + 1");

            // This checks to make sure all the game files are in the right place.
            if (!Directory.Exists("Zones"))
            {
                Log.Error("Directory Check", "Zones directory does not exist");
                ConsoleMgr.WaitAndExit(2000);
            }

            if (!Directory.Exists("Abilities"))
            {
                Log.Error("Directory Check", "Abilities directory does not exist");
                ConsoleMgr.WaitAndExit(2000);
            }

            // This connects our world server to the account server, so we can get player account information.
            Client = new RpcClient("WorldServer-" + Config.RealmId, Config.AccountCacherInfo.RpcLocalIp, 1);
            if (!Client.Start(Config.AccountCacherInfo.RpcServerIp, Config.AccountCacherInfo.RpcServerPort))
                ConsoleMgr.WaitAndExit(2000);

            // This gets information about our specific server (realm).
            Rm = AcctMgr.GetRealm(Config.RealmId);

            if (Rm == null)
            {
                Log.Error("WorldServer", "Realm (" + Config.RealmId + ") not found");
                return;
            }

            // This starts loading all the game data.
            LoaderMgr.Start();
            // This cleans up old data about realm vs realm battles.
            Log.Debug("Battlefront Manager", "Clearing rvr_metrics");
            WorldMgr.Database.ExecuteNonQuery("DELETE FROM rvr_metrics WHERE TIMESTAMP NOT BETWEEN DATE_SUB(UTC_TIMESTAMP(), INTERVAL 60 DAY) AND UTC_TIMESTAMP()");

            // This sets up the big realm vs realm campaigns.
            Log.Debug("Battlefront Manager", "Creating Upper Tier Campaign Manager");
            if (RVRProgressionService._RVRProgressions.Count == 0)
            {
                Log.Error("RVR Progression", "NO RVR Progressions in DB");
                return;
            }
            WorldMgr.UpperTierCampaignManager = new UpperTierCampaignManager(RVRProgressionService._RVRProgressions.Where(x => x.Tier == 4).ToList(), WorldMgr._Regions);
            Log.Debug("Battlefront Manager", "Creating Lower Tier Campaign Manager");
            WorldMgr.LowerTierCampaignManager = new LowerTierCampaignManager(RVRProgressionService._RVRProgressions.Where(x => x.Tier == 1).ToList(), WorldMgr._Regions);
            Log.Debug("Battlefront Manager", "Getting Progression based upon rvr_progression.LastOpenedZone");
            WorldMgr.UpperTierCampaignManager.GetActiveBattleFrontFromProgression();
            WorldMgr.LowerTierCampaignManager.GetActiveBattleFrontFromProgression();
            Log.Debug("Battlefront Manager", "Attaching Campaigns to Regions");
            // This attaches the campaigns to the different areas in the game world.
            WorldMgr.AttachCampaignsToRegions();

            Log.Debug("Battlefront Manager", "Locking Battlefronts");
            WorldMgr.UpperTierCampaignManager.LockBattleFrontsAllRegions(4);
            WorldMgr.LowerTierCampaignManager.LockBattleFrontsAllRegions(1);

            Log.Debug("Battlefront Manager", "Opening Active battlefronts");
            WorldMgr.UpperTierCampaignManager.OpenActiveBattlefront();
            WorldMgr.LowerTierCampaignManager.OpenActiveBattlefront();

            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);

            // This starts the part of the server that listens for players trying to connect.
            if (!TCPManager.Listen<TCPServer>(Rm.Port, "World"))
                ConsoleMgr.WaitAndExit(2000);

            Server = TCPManager.GetTcp<TCPServer>("World");

            // This tells the account server that our world server is ready and how many players are on it.
            AcctMgr.UpdateRealm(Client.Info, Rm.RealmId);
            AcctMgr.UpdateRealmCharacters(Rm.RealmId, (uint)CharMgr.Database.GetObjectCount<Character>("Realm=1"), (uint)CharMgr.Database.GetObjectCount<Character>("Realm=2"));

            // PrintCommands();

            // This starts the console so we can type commands to the server.
            ConsoleMgr.Start();
        }

        // This method is for shutting down the server.
        public static void Kill(bool restart)
        {
            // HandleClosed();

            // If we want to restart the server, this will do it.
            if (restart)
                Process.Start(ExePath, Arguments);

            // This stops the server program.
            m_Process.Kill();
        }

        // This is what happens when a really bad error occurs that we didn't expect.
        private static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("onError", e.ExceptionObject.ToString());
            // We create a report about the crash to help us figure out what went wrong.
            GenerateCrashReport(e);
        }

        // This creates a file with information about a crash.
        private static void GenerateCrashReport(UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Crash: Generating report...");

            try
            {
                // This creates a file name with the date and time of the crash.
                string timeStamp = CrashGuard.GetTimeStamp();
                string fileName = String.Format("WorldServer-Crash {0}.log", timeStamp);

                string root = CrashGuard.GetRoot();
                string filePath = CrashGuard.Combine(root, fileName);

                // This writes all the crash information to the file.
                using (StreamWriter op = new StreamWriter(filePath))
                {
                    Version ver = Assembly.GetCallingAssembly().GetName().Version;

                    op.WriteLine("Server Crash Report");
                    op.WriteLine("===================");
                    op.WriteLine();
                    op.WriteLine("ProjectWAR Version {0}.{1}, Build {2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
                    op.WriteLine("Operating System: {0}", Environment.OSVersion);
                    op.WriteLine(".NET Framework: {0}", Environment.Version);
                    op.WriteLine("Time: {0}", DateTime.Now);

                    op.WriteLine("Exception:");
                    op.WriteLine(e.ExceptionObject);
                    op.WriteLine();

                    op.WriteLine("Clients:");

                    try
                    {
                        // This gets a list of all the players who were online when the crash happened.
                        List<Player> states = Player._Players;

                        op.WriteLine("- Count: {0}", states.Count);

                        for (int i = 0; i < states.Count; ++i)
                        {
                            Player state = states[i];

                            op.Write("+ {0}:", state);

                            Account a = state.Client._Account;

                            if (a != null)
                                op.Write(" (account = {0})", a.Username);
                            op.Write(" (mobile = 0x{0:X} '{1}')", state.CharacterId, state.Name);

                            op.WriteLine();
                        }
                    }
                    catch
                    {
                        op.WriteLine("- Failed");
                    }
                }

                Console.WriteLine("done");
            }
            catch
            {
                Console.WriteLine("failed");
            }
        }

        // This is called when someone tries to close the server.
        public static void OnClose(object obj, object Args)
        {
            Log.Info("Closing", "Closing the server");

            // This makes sure everything is saved and cleaned up before the server closes.
            WorldMgr.Stop();
            Player.Stop();
        }
    }
}
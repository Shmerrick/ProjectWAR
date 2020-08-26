using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

using Common;
using FrameWork;
using WorldServer.Configs;
using WorldServer.Managers;
using WorldServer.NetWork;
using WorldServer.Services.World;
using WorldServer.World.Auction;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Apocalypse.Loot;
using WorldServer.World.Objects;
using System.Diagnostics;

namespace WorldServer
{
    class Program
    {
        public static WorldConfigs Config;
        public static RpcClient Client;
        public static AccountMgr AcctMgr => Client?.GetServerObject<AccountMgr>();
        public static TCPServer Server;
        public static Realm Rm;
        private static Timer _timer;

        private static readonly double _HighFrequency = 1000.0 / Stopwatch.Frequency;
        public static long TickCount => (long)Ticks;

        public static double Ticks => Stopwatch.GetTimestamp() * _HighFrequency;

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnClose);

            Log.Info("", "-------------------- World Server ---------------------", ConsoleColor.DarkRed);

            // Default the server to DEV mode.
            if (args.Length == 0)
                WorldMgr.ServerMode = "DEV";
            else
            {
                if (args.Length == 1)
                {
                    if (args[0] == "DEV")
                    {
                        WorldMgr.ServerMode = "DEV";
                    }
                    if (args[0] == "PRD")
                    {
                        WorldMgr.ServerMode = "PRD";
                    }
                }
                else
                {
                    WorldMgr.ServerMode = "DEV";
                }
            }  

            Log.Info("", "SERVER running in " + WorldMgr.ServerMode + " mode", ConsoleColor.Cyan);
            

            // Loading all configs files
            ConfigMgr.LoadConfigs();
            Config = ConfigMgr.GetConfig<WorldConfigs>();

            // Loading log level from file
            if (!Log.InitLog(Config.LogLevel, "WorldServer"))
                ConsoleMgr.WaitAndExit(2000);

#if DEBUG
            API.Server api = null;
            if (Config.EnableAPI)
            {
                try
                {
                    api = new API.Server(Config.APIAddress, Config.APIPort,100);
                }
                catch (Exception e)
                {
                    Log.Error("API", "Unable to start API server: " + e.Message);
                }
            }
#endif

            CharMgr.Database = DBManager.Start(Config.CharacterDatabase.Total(), Config.CharacterDatabase.ConnectionType, "Characters", Config.CharacterDatabase.Database);
            if (CharMgr.Database == null)
                ConsoleMgr.WaitAndExit(2000);
            else if (!Config.PreloadAllCharacters)
                CharMgr.Database.RegisterAction(CharMgr.LoadPendingCharacters);

            _timer = new Timer(AuctionHouse.CheckAuctionExpiry, null, new TimeSpan(0, 12, 0, 0), new TimeSpan(0, 24, 0, 0));

            WorldMgr.Database = DBManager.Start(Config.WorldDatabase.Total(), Config.CharacterDatabase.ConnectionType, "World", Config.WorldDatabase.Database);
            if (WorldMgr.Database == null)
                ConsoleMgr.WaitAndExit(2000);

            WorldMgr.StartingPairing = WorldMgr.Database.ExecuteQueryInt("SELECT FLOOR(RAND() * 3) + 1");

            // Ensure directory structure is correct
            if (!Directory.Exists("Zones"))
            {
                Log.Error("Directory Check", "Zones directory does not exist");
                ConsoleMgr.WaitAndExit(2000);
            }
            if (!Directory.Exists("Scripts"))
            {
                Log.Error("Directory Check", "Scripts directory does not exist");
                ConsoleMgr.WaitAndExit(2000);
            }
            if (!Directory.Exists("World"))
            {
                Log.Error("Directory Check", "World directory does not exist");
                ConsoleMgr.WaitAndExit(2000);
            }
            if (!Directory.Exists("Abilities"))
            {
                Log.Error("Directory Check", "Abilities directory does not exist");
                ConsoleMgr.WaitAndExit(2000);
            }



            Client = new RpcClient("WorldServer-" + Config.RealmId, Config.AccountCacherInfo.RpcLocalIp, 1);
            if (!Client.Start(Config.AccountCacherInfo.RpcServerIp, Config.AccountCacherInfo.RpcServerPort))
                ConsoleMgr.WaitAndExit(2000);

            Rm = AcctMgr.GetRealm(Config.RealmId);

            if (Rm == null)
            {
                Log.Error("WorldServer", "Realm (" + Config.RealmId + ") not found");
                return;
            }

            LoaderMgr.Start();
            // Clean up rvr_metrics
            Log.Info("Battlefront Manager", "Clearing rvr_metrics", ConsoleColor.Cyan);
            WorldMgr.Database.ExecuteNonQuery("DELETE FROM rvr_metrics WHERE TIMESTAMP NOT BETWEEN DATE_SUB(UTC_TIMESTAMP(), INTERVAL 60 DAY) AND UTC_TIMESTAMP()");

            Log.Info("Battlefront Manager", "Creating Upper Tier Campaign Manager", ConsoleColor.Cyan);
            if (RVRProgressionService._RVRProgressions.Count == 0)
            {
                Log.Error("RVR Progression", "NO RVR Progressions in DB");
                return;
            }
            WorldMgr.UpperTierCampaignManager = new UpperTierCampaignManager(RVRProgressionService._RVRProgressions.Where(x => x.Tier == 4).ToList(), WorldMgr._Regions);
            Log.Info("Battlefront Manager", "Creating Lower Tier Campaign Manager", ConsoleColor.Cyan);
            WorldMgr.LowerTierCampaignManager = new LowerTierCampaignManager(RVRProgressionService._RVRProgressions.Where(x => x.Tier == 1).ToList(), WorldMgr._Regions);
            Log.Info("Battlefront Manager", "Getting Progression based upon rvr_progression.LastOpenedZone", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.GetActiveBattleFrontFromProgression();
            WorldMgr.LowerTierCampaignManager.GetActiveBattleFrontFromProgression();
            Log.Info("Battlefront Manager", "Attaching Campaigns to Regions", ConsoleColor.Cyan);
            // Attach Battlefronts to regions
            WorldMgr.AttachCampaignsToRegions();

            Log.Info("Battlefront Manager", "Locking Battlefronts", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.LockBattleFrontsAllRegions(4);
            WorldMgr.LowerTierCampaignManager.LockBattleFrontsAllRegions(1);

            Log.Info("Battlefront Manager", "Opening Active battlefronts", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.OpenActiveBattlefront();
            WorldMgr.LowerTierCampaignManager.OpenActiveBattlefront();

            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);

            if (!TCPManager.Listen<TCPServer>(Rm.Port, "World"))
                ConsoleMgr.WaitAndExit(2000);

            Server = TCPManager.GetTcp<TCPServer>("World");

            AcctMgr.UpdateRealm(Client.Info, Rm.RealmId);
            AcctMgr.UpdateRealmCharacters(Rm.RealmId, (uint)CharMgr.Database.GetObjectCount<Character>("Realm=1"), (uint)CharMgr.Database.GetObjectCount<Character>("Realm=2"));

            ConsoleMgr.Start();
        }

        static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("onError", e.ExceptionObject.ToString());
            GenerateCrashReport(e);
        }

        private static string GetRoot()
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
        private static string GetTimeStamp()
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

        private static string Combine(string path1, string path2)
        {
            if (path1.Length == 0)
                return path2;

            return Path.Combine(path1, path2);
        }

        private static void GenerateCrashReport(UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Crash: Generating report...");

            try
            {
                string timeStamp = GetTimeStamp();
                string fileName = String.Format("Crash {0}.log", timeStamp);

                string root = GetRoot();
                string filePath = Combine(root, fileName);

                using (StreamWriter op = new StreamWriter(filePath))
                {
                    Version ver = Assembly.GetCallingAssembly().GetName().Version;

                    op.WriteLine("Server Crash Report");
                    op.WriteLine("===================");
                    op.WriteLine();
                    op.WriteLine("WarEmu Version {0}.{1}, Build {2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
                    op.WriteLine("Operating System: {0}", Environment.OSVersion);
                    op.WriteLine(".NET Framework: {0}", Environment.Version);
                    op.WriteLine("Time: {0}", DateTime.Now);

                    op.WriteLine("Exception:");
                    op.WriteLine(e.ExceptionObject);
                    op.WriteLine();

                    op.WriteLine("Clients:");

                    try
                    {
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
        public static void OnClose(object obj, object Args)
        {
            Log.Info("Closing", "Closing the server");

            WorldMgr.Stop();
            Player.Stop();
        }
    }
}

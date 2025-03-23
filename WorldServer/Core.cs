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
    internal class Core
    {
        public static WorldConfigs Config;
        public static AccountConfig AccountConfig;
        public static RpcClient Client;
        public static AccountMgr AcctMgr => Client?.GetServerObject<AccountMgr>();
        public static TCPServer Server;
        public static Realm Rm;
        private static Timer _timer;
        private static Process m_Process;

        public static string ExePath
        {
            get
            {
                return Assembly.GetEntryAssembly().Location;
            }
        }

        public static bool Debug { get; private set; }
        public static bool Dev { get; private set; }
        public static bool HighPriority { get; private set; }
        public static bool LoadPhysics { get; private set; }

        private static readonly double _HighFrequency = 1000.0 / Stopwatch.Frequency;
        public static long TickCount => (long)Ticks;

        public static double Ticks => Stopwatch.GetTimestamp() * _HighFrequency;

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

        [STAThread]
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnClose);
            m_Process = Process.GetCurrentProcess();

            Log.Info("", "-------------------- World Server ---------------------", ConsoleColor.DarkRed);

            // WorldServer mode load
            Debug = true;
            HighPriority = true;
            Dev = true;
            LoadPhysics = true;

            if (Dev)
            {
                WorldMgr.ServerMode = "DEV";
            }
            else
            {
                WorldMgr.ServerMode = "PRD";
            }

            Version ver = Assembly.GetEntryAssembly().GetName().Version;

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
                if (HighPriority)
                {
                    Console.WriteLine("Core: Set process priority to Above Normal");
                    System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
                }
            }
            catch (Exception ex)
            {
                //Server.Diagnostics.ExceptionLogging.LogException(ex);
            }

            if (GCSettings.IsServerGC)
                Console.WriteLine("Core: Server garbage collection mode enabled");

            Log.Info("", "Core: running in " + WorldMgr.ServerMode + " mode");
            Utils.PopColor();
            Utils.PushColor(ConsoleColor.Gray);
            // Loading all configs files
            ConfigMgr.LoadConfigs();
            Config = ConfigMgr.GetConfig<WorldConfigs>();
            AccountConfig = ConfigMgr.GetConfig<AccountConfig>();

            // Loading log level from file
            if (!Log.InitLog(Config.LogLevel, "WorldServer"))
                ConsoleMgr.WaitAndExit(2000);

#if DEBUG
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
            Log.Debug("Battlefront Manager", "Clearing rvr_metrics");
            WorldMgr.Database.ExecuteNonQuery("DELETE FROM rvr_metrics WHERE TIMESTAMP NOT BETWEEN DATE_SUB(UTC_TIMESTAMP(), INTERVAL 60 DAY) AND UTC_TIMESTAMP()");

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
            // Attach Battlefronts to regions
            WorldMgr.AttachCampaignsToRegions();

            Log.Debug("Battlefront Manager", "Locking Battlefronts");
            WorldMgr.UpperTierCampaignManager.LockBattleFrontsAllRegions(4);
            WorldMgr.LowerTierCampaignManager.LockBattleFrontsAllRegions(1);

            Log.Debug("Battlefront Manager", "Opening Active battlefronts");
            WorldMgr.UpperTierCampaignManager.OpenActiveBattlefront();
            WorldMgr.LowerTierCampaignManager.OpenActiveBattlefront();

            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);

            if (!TCPManager.Listen<TCPServer>(Rm.Port, "World"))
                ConsoleMgr.WaitAndExit(2000);

            Server = TCPManager.GetTcp<TCPServer>("World");

            AcctMgr.UpdateRealm(Client.Info, Rm.RealmId);
            AcctMgr.UpdateRealmCharacters(Rm.RealmId, (uint)CharMgr.Database.GetObjectCount<Character>("Realm=1"), (uint)CharMgr.Database.GetObjectCount<Character>("Realm=2"));

            // PrintCommands();

            ConsoleMgr.Start();
        }

        public static void Kill(bool restart)
        {
            // HandleClosed();

            if (restart)
                Process.Start(ExePath, Arguments);

            m_Process.Kill();
        }

        private static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("onError", e.ExceptionObject.ToString());
            GenerateCrashReport(e);
        }

        private static void GenerateCrashReport(UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Crash: Generating report...");

            try
            {
                string timeStamp = CrashGuard.GetTimeStamp();
                string fileName = String.Format("WorldServer-Crash {0}.log", timeStamp);

                string root = CrashGuard.GetRoot();
                string filePath = CrashGuard.Combine(root, fileName);

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
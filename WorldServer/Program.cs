using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Runtime.InteropServices;

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
        private static Timer _botTimer;
        private static API.BotEditorHttpServer _botEditorApi;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectory(string lpPathName);
        
        private static void ResetBattlefrontProgressionsOnStartup()
        {
            ResetBattlefrontProgressions(WorldMgr.UpperTierCampaignManager);
            ResetBattlefrontProgressions(WorldMgr.LowerTierCampaignManager);

            RVRProgressionService.SaveRVRProgression(WorldMgr.UpperTierCampaignManager.BattleFrontProgressions);
            RVRProgressionService.SaveRVRProgression(WorldMgr.LowerTierCampaignManager.BattleFrontProgressions);
        }

        public static void SaveRuntimeState()
        {
            if (WorldMgr.UpperTierCampaignManager?.BattleFrontProgressions != null)
                RVRProgressionService.SaveRVRProgression(WorldMgr.UpperTierCampaignManager.BattleFrontProgressions);

            if (WorldMgr.LowerTierCampaignManager?.BattleFrontProgressions != null)
                RVRProgressionService.SaveRVRProgression(WorldMgr.LowerTierCampaignManager.BattleFrontProgressions);

            CharMgr.Database?.ForceSave();
            WorldMgr.Database?.ForceSave();
        }

        public static void UpdateRealmPopulationSnapshot()
        {
            if (Rm == null || AcctMgr == null)
                return;

            Rm.OnlinePlayers = (uint)Player.GetPlayerCount();
            AcctMgr.UpdateRealm(Rm.RealmId, Rm.OnlinePlayers, Player.OrderCount, Player.DestruCount);
        }

        private static void ResetBattlefrontProgressions(IBattleFrontManager manager)
        {
            foreach (var progression in manager.BattleFrontProgressions)
            {
                progression.DestroVP = 0;
                progression.OrderVP = 0;
                progression.LastOpenedZone = 0;
                progression.LastOwningRealm = progression.DefaultRealmLock;
            }

            var defaultBattleFront = manager.GetActiveBattleFrontFromProgression();
            if (defaultBattleFront != null)
                defaultBattleFront.LastOpenedZone = 1;
        }


        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);
            RegisterShutdownHandlers();

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

            // Ensure native dependencies moved under /libs are discoverable by DllImport.
            string libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs");
            if (Directory.Exists(libsPath))
                SetDllDirectory(libsPath);

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
            if (!Directory.Exists("World"))
            {
                Log.Error("Directory Check", "World directory does not exist");
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

            if (Config.ResetBattlefrontsOnStartup)
            {
                Log.Info("Battlefront Manager", "ResetBattlefrontsOnStartup enabled - resetting battlefront progression", ConsoleColor.Cyan);
                ResetBattlefrontProgressionsOnStartup();
            }

            Log.Info("Battlefront Manager", "Getting Progression based upon rvr_progression.LastOpenedZone", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.GetActiveBattleFrontFromProgression();
            WorldMgr.LowerTierCampaignManager.GetActiveBattleFrontFromProgression();

            Log.Info("Battlefront Manager", "Attaching Campaigns to Regions", ConsoleColor.Cyan);
            WorldMgr.AttachCampaignsToRegions();

            Log.Info("Battlefront Manager", "Locking Battlefronts", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.LockBattleFrontsAllRegions(4, Config.ResetBattlefrontsOnStartup);
            WorldMgr.LowerTierCampaignManager.LockBattleFrontsAllRegions(1, Config.ResetBattlefrontsOnStartup);

            Log.Info("Battlefront Manager", "Opening Active battlefronts", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.OpenActiveBattlefront();
            WorldMgr.LowerTierCampaignManager.OpenActiveBattlefront();

            WorldMgr.UpdateRegionCaptureStatus(WorldMgr.LowerTierCampaignManager, WorldMgr.UpperTierCampaignManager);

            if (!TCPManager.Listen<TCPServer>(Rm.Port, "World"))
                ConsoleMgr.WaitAndExit(2000);

            Server = TCPManager.GetTcp<TCPServer>("World");

            AcctMgr.UpdateRealm(Client.Info, Rm.RealmId);
            AcctMgr.UpdateRealmCharacters(Rm.RealmId, (uint)CharMgr.Database.GetObjectCount<Character>("Realm=1"), (uint)CharMgr.Database.GetObjectCount<Character>("Realm=2"));

            // Reconcile the realm population against reality now that we are listening but nobody has
            // connected yet. Relying on the shutdown path alone leaves a stale count behind whenever the
            // process is killed or crashes, which then advertises phantom players to the launcher.
            UpdateRealmPopulationSnapshot();

            Log.Info("GameCommands", "Available Game Commands:");
            WorldServer.Managers.Commands.CommandsBuilder.ListAllCommands(WorldServer.Managers.Commands.CommandDeclarations.BaseCommand);

            Log.Info("Bot Manager", "Initializing Bot Management Services", ConsoleColor.Cyan);
            BotManager.Instance.Initialize();
            if (DynamicBotManager.AutoManagementEnabled)
            {
                DynamicBotManager.Instance.Start();
                _botTimer = new Timer(DynamicBotManager.Instance.Update, null, 120000, 60000);
            }
            else
                Log.Info("Bot Manager", "Dynamic bot auto-management disabled for manual bot validation.", ConsoleColor.Cyan);

            if (Config.EnableBotEditorAPI)
            {
                try
                {
                    _botEditorApi = new API.BotEditorHttpServer(Config.BotEditorAPIAddress, Config.BotEditorAPIPort);
                    _botEditorApi.Start();
                }
                catch (Exception e)
                {
                    Log.Error("BotEditorAPI", "Unable to start bot editor API: " + e.Message);
                }
            }

            if (Environment.UserInteractive)
            {
                ConsoleMgr.Start();
            }
            else
            {
                Log.Info("Program", "Running in non-interactive mode, blocking with ManualResetEvent");
                new System.Threading.ManualResetEvent(false).WaitOne();
            }
        }

        // Console control events we translate into a graceful shutdown. CTRL_CLOSE (the window's X,
        // and what Process.CloseMainWindow sends) and the logoff/shutdown events were previously
        // unhandled, which is why a launcher stop never persisted anything.
        private const int CTRL_C_EVENT = 0;
        private const int CTRL_BREAK_EVENT = 1;
        private const int CTRL_CLOSE_EVENT = 2;
        private const int CTRL_LOGOFF_EVENT = 5;
        private const int CTRL_SHUTDOWN_EVENT = 6;

        private delegate bool ConsoleCtrlHandler(int ctrlType);

        /// <summary>Held in a static field: the delegate is passed to native code and must outlive the call.</summary>
        private static ConsoleCtrlHandler _consoleCtrlHandler;

        /// <summary>0 until a shutdown starts. Several signals can arrive at once, so shutdown runs once.</summary>
        private static int _shutdownStarted;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlHandler handler, [MarshalAs(UnmanagedType.Bool)] bool add);

        /// <summary>
        /// Routes every termination signal Windows will give us into <see cref="Shutdown"/>. Ctrl+C alone
        /// was not enough: the launcher stops services by closing them, and a hard Process.Kill delivers
        /// no signal at all, so the launcher must close rather than kill for any of this to run.
        /// </summary>
        private static void RegisterShutdownHandlers()
        {
            Console.CancelKeyPress += OnClose;
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown("process exit");

            _consoleCtrlHandler = ctrlType =>
            {
                switch (ctrlType)
                {
                    case CTRL_C_EVENT:
                    case CTRL_BREAK_EVENT:
                    case CTRL_CLOSE_EVENT:
                    case CTRL_LOGOFF_EVENT:
                    case CTRL_SHUTDOWN_EVENT:
                        // Windows allows roughly five seconds here before terminating us regardless, which
                        // is why Shutdown does the persistence work first and the tidy-up afterwards.
                        Shutdown("console control event " + ctrlType);
                        return true;
                    default:
                        return false;
                }
            };

            if (!SetConsoleCtrlHandler(_consoleCtrlHandler, true))
                Log.Error("Shutdown", "SetConsoleCtrlHandler failed; only Ctrl+C will shut down cleanly.");
        }

        static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("onError", e.ExceptionObject.ToString());
        }

        public static void OnClose(object obj, object Args)
        {
            Shutdown("console cancel key");
        }

        /// <summary>
        /// Persists everything that only exists in memory, then releases resources. Safe to call from any
        /// signal handler and safe to call more than once; only the first caller does the work.
        /// </summary>
        public static void Shutdown(string reason)
        {
            if (Interlocked.CompareExchange(ref _shutdownStarted, 1, 0) != 0)
                return;

            Log.Info("Closing", "Closing the server (" + reason + ")");

            // Persistence first, and each step guarded: the shutdown window is short and bounded, so one
            // failing step must not cost us the remaining ones.
            TryShutdownStep("save runtime state", SaveRuntimeState);
            TryShutdownStep("stop world manager", WorldMgr.Stop);
            TryShutdownStep("disconnect players", Player.Stop);

            // Players are gone at this point, so publish a zeroed population rather than leaving the
            // last live figure in the realm record.
            TryShutdownStep("zero realm population", UpdateRealmPopulationSnapshot);
            TryShutdownStep("stop bot editor API", () => _botEditorApi?.Stop());

            // Log targets are async-wrapped, so flush before the process goes away or the shutdown
            // record itself is lost.
            TryShutdownStep("flush logs", () => NLog.LogManager.Shutdown());
        }

        private static void TryShutdownStep(string description, Action step)
        {
            try
            {
                step();
            }
            catch (Exception e)
            {
                Log.Error("Shutdown", "Failed to " + description + ": " + e);
            }
        }
    }
}

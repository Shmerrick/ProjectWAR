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

        static void onError(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error("onError", e.ExceptionObject.ToString());
        }

        public static void OnClose(object obj, object Args)
        {
            Log.Info("Closing", "Closing the server");

            _botEditorApi?.Stop();
            SaveRuntimeState();
            WorldMgr.Stop();
            Player.Stop();

            // Players are gone at this point, so publish a zeroed population rather than leaving the
            // last live figure in the realm record.
            UpdateRealmPopulationSnapshot();
        }
    }
}








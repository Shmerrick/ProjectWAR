using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;

using Common;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Battlefronts.Apocalypse;

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
        // DEV - Development mode, PRD - Production Mode. 
        public static string ServerMode;

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnClose);

            Log.Texte("", "-------------------- World Server ---------------------", ConsoleColor.DarkRed);

            // Default the server to DEV mode.
            if (args.Length == 0)
                ServerMode = "DEV";
            else
            {
                if (args.Length == 1)
                {
                    if (args[0] == "DEV")
                    {
                        ServerMode = "DEV";
                    }
                    if (args[0] == "PRD")
                    {
                        ServerMode = "PRD";
                    }
                }
                else
                {
                    ServerMode = "DEV";
                }
            }  



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
                Log.Texte("Directory Check", "Zones directory does not exist", ConsoleColor.Red);
                ConsoleMgr.WaitAndExit(2000);
            }
            if (!Directory.Exists("Scripts"))
            {
                Log.Texte("Directory Check", "Scripts directory does not exist", ConsoleColor.Red);
                ConsoleMgr.WaitAndExit(2000);
            }
            if (!Directory.Exists("World"))
            {
                Log.Texte("Directory Check", "World directory does not exist", ConsoleColor.Red);
                ConsoleMgr.WaitAndExit(2000);
            }
            if (!Directory.Exists("Abilities"))
            {
                Log.Texte("Directory Check", "Abilities directory does not exist", ConsoleColor.Red);
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
            Log.Texte("Battlefront Manager", "Creating Upper Tier Campaign Manager", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager = new UpperTierCampaignManager(RVRProgressionService._RVRProgressions.Where(x=>x.Tier == 4).ToList(), WorldMgr._Regions);
            Log.Texte("Battlefront Manager", "Creating Lower Tier Campaign Manager", ConsoleColor.Cyan);
            WorldMgr.LowerTierCampaignManager = new LowerTierCampaignManager(RVRProgressionService._RVRProgressions.Where(x => x.Tier == 1).ToList(), WorldMgr._Regions);
            Log.Texte("Battlefront Manager", "Resetting Progression", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.ResetBattleFrontProgression();
            WorldMgr.LowerTierCampaignManager.ResetBattleFrontProgression();
            Log.Texte("Battlefront Manager", "Attaching Battlefronts to Regions", ConsoleColor.Cyan);
            // Attach Battlefronts to regions
            WorldMgr.AttachUpperTierBattleFronts(RVRProgressionService._RVRProgressions.Where(x => x.Tier == 4).ToList());

            Log.Texte("Battlefront Manager", "Locking Battlefronts", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.LockBattleFrontsAllRegions(4);
            WorldMgr.LowerTierCampaignManager.LockBattleFrontsAllRegions(1);

            Log.Texte("Battlefront Manager", "Opening Active battlefronts", ConsoleColor.Cyan);
            WorldMgr.UpperTierCampaignManager.OpenActiveBattlefront();
            WorldMgr.LowerTierCampaignManager.OpenActiveBattlefront();

            WorldMgr.UpdateRegionCaptureStatus();
                
            
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
        }

        public static void OnClose(object obj, object Args)
        {
            Log.Info("Closing", "Closing the server");

            WorldMgr.Stop();
            Player.Stop();
        }
    }
}

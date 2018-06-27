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

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(onError);
            Console.CancelKeyPress += new ConsoleCancelEventHandler(OnClose);

            Log.Texte("", "-------------------- World Server ---------------------", ConsoleColor.DarkRed);

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

            WorldMgr.UpperTierBattleFrontManager = new UpperTierBattleFrontManager(RVRProgressionService._RVRProgressions.Where(x=>x.Tier == 4).ToList(), WorldMgr._Regions);

            WorldMgr.LowerTierBattleFrontManager = new LowerTierBattleFrontManager(RVRProgressionService._RVRProgressions.Where(x => x.Tier == 1).ToList(), WorldMgr._Regions);
            WorldMgr.UpperTierBattleFrontManager.ResetBattleFrontProgression();
            WorldMgr.LowerTierBattleFrontManager.ResetBattleFrontProgression();
            // Attach Battlefronts to regions
            WorldMgr.AttachBattleFronts();

            WorldMgr.UpperTierBattleFrontManager.LockBattleFrontsAllRegions(4);
            Log.Texte("Creating Upper Tier BattleFront Manager", WorldMgr.UpperTierBattleFrontManager.ActiveBattleFrontName, ConsoleColor.Cyan);

            WorldMgr.LowerTierBattleFrontManager.LockBattleFrontsAllRegions(1);
            Log.Texte("Creating Lower Tier BattleFront Manager", WorldMgr.LowerTierBattleFrontManager.ActiveBattleFrontName, ConsoleColor.Cyan);

            WorldMgr.UpperTierBattleFrontManager.OpenActiveBattlefront();
            WorldMgr.LowerTierBattleFrontManager.OpenActiveBattlefront();


            Log.Texte("StartingPairing: ", WorldMgr.StartingPairing.ToString(), ConsoleColor.Cyan);


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

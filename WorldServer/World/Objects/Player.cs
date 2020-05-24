using Common;
using Common.Database.World.BattleFront;
using FrameWork;
using GameData;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using SystemData;
using WorldServer.Managers;
using WorldServer.Managers.Commands;
using WorldServer.NetWork;
using WorldServer.NetWork.Handler;
using WorldServer.Services.World;
using WorldServer.World.Abilities;
using WorldServer.World.Abilities.Buffs;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Abilities.Components;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Battlefronts.Keeps;
using WorldServer.World.Interfaces;
using WorldServer.World.Map;
using WorldServer.World.Objects.PublicQuests;
using WorldServer.World.Positions;
using WorldServer.World.Scenarios;
using WorldServer.World.Scenarios.Objects;
using BattleFrontStatus = WorldServer.World.Battlefronts.Apocalypse.BattleFrontStatus;
using Exception = System.Exception;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    public class ResurrectionInfo
    {
        public Unit Caster;
        public Point3D RezPosition;
        public ushort Entry;
        public byte PctResHealth;
        public bool CausesSickness;

        public ResurrectionInfo(Unit caster, Point3D rezPosition, ushort entry, byte pctResHealth, bool causesSickness)
        {
            Caster = caster;
            RezPosition = rezPosition;
            Entry = entry;
            PctResHealth = pctResHealth;
            CausesSickness = causesSickness;
        }
    }

    public class Player : Unit
    {
        #region Statics

        public static int DISCONNECT_TIME = 20 * Time.InMilliseconds;
        public static int AUTO_SAVE_TIME = 10 * Time.Minute;

        /// <summary>Influence bonus depending on players' race</summary>
        private const double RACIAL_INF_FACTOR = 1.5;

        public static List<Player> _Players = new List<Player>();
        public static Dictionary<uint, Player> PlayersByCharId = new Dictionary<uint, Player>();
        public static uint OrderCount;
        public static uint DestruCount;
        // The bounty level for this player. 
        public int BaseBountyValue => (_Value.Level) + (2 * _Value.RenownRank);
        public float AAOBonus { get; set; }

        public string InstanceID { get; set; } = string.Empty;

        public static void AddPlayer(Player newPlayer)
        {
            bool Found = false;
            lock (_Players)
            {
                if (!_Players.Contains(newPlayer))
                {
                    Found = true;
                    _Players.Add(newPlayer);
                    PlayersByCharId.Add(newPlayer.Info.CharacterId, newPlayer);
                    if (newPlayer.Realm == Realms.REALMS_REALM_ORDER)
                        ++OrderCount;
                    else
                        ++DestruCount;

                    Program.Rm.OnlinePlayers = (uint)_Players.Count;
                    Program.AcctMgr.UpdateRealm(Program.Rm.RealmId, Program.Rm.OnlinePlayers, OrderCount, DestruCount);

                }
            }

            if (Found)
            {
                newPlayer._Value.Online = true;
                CharMgr.Database.SaveObject(newPlayer._Value);
            }
        }

        public void KneelDown(ushort targetOid, bool kneel, ushort duration = 0)
        {
            PacketOut Out = null;
            if (kneel)
            {
                Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                Out.WriteByte(0);
                Out.WriteByte(1);
                Out.WriteByte(1);
                Out.WriteByte(5);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteUInt16(duration);
                Out.WriteUInt16(targetOid);
                Out.WriteByte(0);
                Out.WriteByte(0);
                SendPacket(Out);
            }
            else
            {

                Out = new PacketOut((byte)Opcodes.F_SET_ABILITY_TIMER, 12);
                Out.WriteByte(0);
                Out.WriteByte(1);
                Out.WriteByte(1);
                Out.Fill(0, 9);
                SendPacket(Out);
            }

            Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 13);
            Out.WriteUInt16(Oid);
            Out.WriteByte(0x1B);
            Out.WriteByte((byte)(kneel ? 1 : 0));
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16(targetOid);
            Out.WriteByte(0);
            Out.WriteByte(0);
            DispatchPacket(Out, false);
        }

        public static void RemovePlayer(Player oldPlayer)
        {
            bool found = false;

            lock (_Players)
            {
                if (_Players.Remove(oldPlayer))
                {
                    found = true;

                    PlayersByCharId.Remove(oldPlayer.Info.CharacterId);

                    if (oldPlayer.Info.Realm == (byte)Realms.REALMS_REALM_ORDER)
                        --OrderCount;
                    else
                        --DestruCount;

                    Program.Rm.OnlinePlayers = (uint)_Players.Count;
                    Program.AcctMgr.UpdateRealm(Program.Rm.RealmId, Program.Rm.OnlinePlayers, OrderCount, DestruCount);

                    if (oldPlayer.BroadcastRank)
                        GmMgr.NotifyGMOffline(oldPlayer);
                }
            }

            if (found)
            {
                oldPlayer._Value.Online = false;
                oldPlayer._Value.DisconcetTime = TCPManager.GetTimeStamp();
                CharMgr.Database.SaveObject(oldPlayer._Value);
            }
        }
        public static Player GetPlayer(string name)
        {
            lock (_Players)
                return _Players.Find(plr => name.Equals(plr.Name, StringComparison.OrdinalIgnoreCase));
        }
        public static Player GetPlayer(uint characterId)
        {
            lock (_Players)
                return _Players.Find(plr => plr.CharacterId == characterId);
        }
        public static Player CreatePlayer(GameClient client, Character Char)
        {
            GameClient other = ((TCPServer)client.Server).GetClientByAccount(client, Char.AccountId);
            if (other != null)
            {
                other.Disconnect("Null account in CreatePlayer");
                return null;
            }

            lock (_Players)
            {
                if (PlayersByCharId.ContainsKey(Char.CharacterId))
                    return PlayersByCharId[Char.CharacterId];
            }

            return new Player(client, Char);
        }
        public static List<Player> GetPlayers(string name, string guildName, ushort career, ushort zoneId, byte minLevel, byte maxLevel, Player caller = null)
        {
            List<Player> players = new List<Player>();

            lock (_Players)
            {
                foreach (Player curPlayer in _Players)
                {
                    if (curPlayer == null || curPlayer.IsDisposed || !curPlayer.IsInWorld())
                        continue;

                    if (curPlayer.SocInterface.Hide && caller != null && caller.GmLevel == 1)
                        continue;

                    if ((name.Length > 0 && !curPlayer.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                        || (career != 0 && career != curPlayer.Info.Career)
                        || (zoneId != 255 && curPlayer.Zone.ZoneId != zoneId)
                        || (curPlayer.Level < minLevel)
                        || (curPlayer.Level > maxLevel)
                        || (!string.IsNullOrEmpty(guildName) && (curPlayer.GldInterface.Guild == null || curPlayer.GldInterface.GetGuildName() != guildName))
                        )
                        continue;

                    players.Add(curPlayer);
                }
            }

            return players;
        }
        public static void Stop()
        {
            foreach (Player plr in _Players)
                plr.Quit();
        }

        #endregion

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");
        private static readonly Logger DeathLogger = LogManager.GetLogger("DeathLogger");

        private ushort _maxActionPoints;

        public Character Info;
        public Character_value _Value;

        // This is used by State of the realm
        public bool SoREnabled = false;

        public bool DebugMode;

        public GameClient Client { get; set; }
        public string GenderedName { get; }

        public MovementHandlers.GROUNDTYPE GroundType { get; set; }

        public bool IsAFK = false;
        public bool IsAutoAFK = false;
        public string AFKMessage = "";
        // This is used by Halloween event
        public bool Spooky = false;
        public long deathTime = 0;

        public BattleFrontStatus ActiveBattleFrontStatus => GetBattlefrontManager(Region.RegionId).GetActiveCampaign().GetActiveBattleFrontStatus();
        public BountyManager BountyManagerInstance => GetBattlefrontManager(Region.RegionId).BountyManagerInstance;
        public ImpactMatrixManager ImpactMatrixManager => GetPlayerImpactMatrixManager();

        private ImpactMatrixManager GetPlayerImpactMatrixManager()
        {
            if (ScnInterface.Scenario == null)
                return GetBattlefrontManager(Region.RegionId).ImpactMatrixManagerInstance;
            else
                return ScenarioMgr.ImpactMatrixManagerInstance;
        }

        public void SpreadSpooky(object list)
        {
            var Params = (List<object>)list;

            Player plr = (Player)Params[0];

            int count = 0;

            foreach (Player player in plr.PlayersInRange.ToList())
            {
                if (player != null && plr.GetDistanceToObject(player) < 31 && !player.Spooky)
                {
                    count++;
                    Spookify(player);
                }

                if (count > 3)
                {
                    break;
                }
            }
        }

        public static bool Spookify(Player plr)
        {
            if (!plr.Spooky)
            {
                var target = plr;

                int modelID = 0;

                if (plr.Realm == Realms.REALMS_REALM_ORDER)
                {
                    if (plr.Info.Race == 1)
                    {
                        if (plr.Info.Sex == 0)
                        {
                            modelID = 1026;
                        }
                        else
                        {
                            modelID = 1027;
                        }
                    }
                    else if (plr.Info.Race == 4)
                    {
                        if (plr.Info.Sex == 0)
                        {
                            modelID = 1035;
                        }
                        else
                        {
                            modelID = 1036;
                        }
                    }
                    else if (plr.Info.Race == 6)
                    {
                        if (plr.Info.Sex == 0)
                        {
                            modelID = 156;
                        }
                        else
                        {
                            modelID = 156;
                        }
                    }

                }
                else
                {
                    if (plr.Info.Race == 2) // Orc
                    {
                        modelID = 1028;
                    }
                    else if (plr.Info.Race == 3) // Goblin
                    {
                        modelID = 1029;
                    }
                    else if (plr.Info.Race == 5) // DE
                    {
                        if (plr.Info.Sex == 0)
                        {
                            modelID = 1037;
                        }
                        else
                        {
                            modelID = 1038;
                        }
                    }

                    else if (plr.Info.Race == 7) // Chaos
                    {

                        if (plr.Info.CareerFlags == 4096)
                        {
                            modelID = 1034;
                        }
                        else if (plr.Info.CareerFlags == 8192)
                        {
                            modelID = 1034;
                        }
                        else if (plr.Info.Sex == 0)
                        {
                            modelID = 156;
                        }
                        else
                        {
                            modelID = 156;
                        }
                    }

                }

                target.ImageNum = (ushort)modelID;

                var Out = new PacketOut((byte)Opcodes.F_PLAYER_IMAGENUM); //F_PLAYER_INVENTORY
                Out.WriteUInt16(target.Oid);
                Out.WriteUInt16((ushort)modelID);
                Out.Fill(0, 18);
                target.DispatchPacket(Out, true);

                plr.Spooky = true;

                Random random = new Random();
                ushort vfx = 0;
                switch (random.Next(1, 3))
                {
                    case 1:
                        vfx = 2498;
                        break;
                    case 2:
                        vfx = 3155;
                        break;
                }

                plr.PlayEffect(vfx);

                var prms = new List<object>() { plr };
                plr.EvtInterface.AddEvent(plr.SpreadSpooky, 120 * 1000, 0, prms);
                plr.SetGearShowing(2, false);

            }
            return true;
        }

        // End of Halloween stuff

        public string ChatName
        {
            get
            {
                if (GmLevel == 1)
                    return Name;
                else
                {
                    return Name + " [Staff]";
                }
            }
        }

        private bool _broadcastRank;

        public bool BroadcastRank
        {
            get { return _broadcastRank; }
            set
            {
                _broadcastRank = value;

                if (_broadcastRank)
                {

                    if (Utils.HasFlag(GmLevel, (int)EGmLevel.Management))
                        UpdateLastName("[Lead]");

                    else if (Utils.HasFlag(GmLevel, (int)EGmLevel.SourceDev))
                        UpdateLastName("[Dev]");

                    else if (Utils.HasFlag(GmLevel, (int)EGmLevel.DatabaseDev))
                        UpdateLastName("[DB]");

                    else if (Utils.HasFlag(GmLevel, (int)EGmLevel.AnyGM))
                        UpdateLastName("[GM]");

                    else
                        UpdateLastName("[Tester]");
                }
                else
                    UpdateLastName(Info.Surname ?? "");
            }
        }

        public SocialInterface SocInterface;
        public TokInterface TokInterface;
        public MailInterface MlInterface;
        public ScenarioInterface ScnInterface;
        public GuildInterface GldInterface;
        public GroupInterface GrpInterface;
        public TacticsInterface TacInterface;
        public CareerInterface CrrInterface;
        public RenownInterface RenInterface;
        public CultivationInterface CultivInterface;
        public CraftingApoInterface CraftApoInterface;
        public CraftingTalInterface CraftTalInterface;
        public GatheringInterface GatherInterface;
        public LiveEventInterface LiveEventInterface;

        public uint CharacterId => Info?.CharacterId ?? 0;
        public int GmLevel => Client?._Account.GmLevel ?? 0;

        private bool _initInProgress = false;

        private bool _initialized = false;
        public bool Initialized => _initialized;

        public int noSurname => Client?._Account.noSurname ?? 0;

        public ushort ImageNum; //overlay npc model with F_PLAYER_IMAGENUM if set 
        public List<byte> EffectStates = new List<byte>(); //list of active effects (ie: mutations, city champ mode, etc)

        public override string Name
        {
            get
            {
                if (Info != null && Info.TempFirstName != null)
                    return Info.TempFirstName;
                return base.Name;
            }

            set
            {
                base.Name = value;
            }
        }

        public Player(GameClient client, Character info)
        {
            Client = client;
            Info = info;
            _Value = info.Value;

            /*if (GmLevel > 0)
                Name = char.ToUpper(_Client._Account.Username[0]) + _Client._Account.Username.Substring(1);
            else*/
            Name = info.Name;
            GenderedName = Name + (info.Sex == 0 ? "^M" : "^F");
            Realm = (Realms)info.Realm;
            SetPVPFlag(false);

            EvtInterface = AddInterface<EventInterface>();
            SocInterface = AddInterface<SocialInterface>();
            TokInterface = AddInterface<TokInterface>();
            MlInterface = AddInterface<MailInterface>();
            ScnInterface = AddInterface<ScenarioInterface>();
            GldInterface = AddInterface<GuildInterface>();
            GrpInterface = AddInterface<GroupInterface>();
            OSInterface = AddInterface<ObjectStateInterface>();
            TacInterface = AddInterface<TacticsInterface>();
            RenInterface = AddInterface<RenownInterface>();
            CrrInterface = AddInterface(CareerInterface.GetInterfaceFor(this, Info.CareerLine)) as CareerInterface;
            CultivInterface = AddInterface<CultivationInterface>();
            CraftTalInterface = AddInterface<CraftingTalInterface>();
            CraftApoInterface = AddInterface<CraftingApoInterface>();
            GatherInterface = AddInterface<GatheringInterface>();
            LiveEventInterface = AddInterface<LiveEventInterface>();

            EvtInterface.AddEventNotify(EventName.OnMove, CancelQuit);
            EvtInterface.AddEventNotify(EventName.OnReceiveDamage, CancelQuit);
            EvtInterface.AddEventNotify(EventName.OnDealDamage, CancelQuit);
            EvtInterface.AddEventNotify(EventName.OnStartCasting, CancelQuit);

            EvtInterface.AddEventNotify(EventName.OnDealDamage, CheckHotSpot);

            BroadcastRank = true;

            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_LASTNAME);
            Out.WriteUInt16(Oid);
            Out.WritePascalString(ChatName);
            DispatchPacket(Out, true);


        }

        public override void OnLoad()
        {
            if (Client == null)
            {
                Log.Error("Player OnLoad", "No client.");
                return;
            }

            Client.State = (int)eClientState.WorldEnter;

            // Handle temporary Exile (warned while offline)
            if (Client._Account?.Banned == 2)
            {
                Client._Account.Banned = TCPManager.GetTimeStamp() + 60;
                Program.AcctMgr.UpdateAccount(Client._Account);
            }

            if (!_initialized && !_initInProgress)
            {
                EvtInterface._Owner = this;
                EvtInterface.AddEventNotify(EventName.Playing, Save);
                EvtInterface.Start();

                ItmInterface.Load(CharMgr.GetItemsForCharacter(Info));
                StsInterface.Load(CharMgr.GetCharacterInfoStats(Info.CareerLine, _Value.Level));
                QtsInterface.Load(Info.Quests);
                TokInterface.Load(Info.Toks, Info.TokKills);
                SocInterface.Load();
                MlInterface.Load(Info.Mails);
                GldInterface.Load(Guild.Guild.GetGuildFromLeader(Info.CharacterId));
                AbtInterface.Load(); // used to send mastery here
                OSInterface.Load();
                TacInterface.Load();
                RenInterface.Load();
                // SendChapterBar();
                StsInterface.ApplyStats();

                if (_Value.GatheringSkill < 3)
                    GatherInterface.Load();
                if (_Value.GatheringSkill == 3)
                    CultivInterface.Load();
                if (_Value.CraftingSkill == 4)
                    CraftApoInterface.Load();
                if (_Value.CraftingSkill == 5)
                    CraftTalInterface.Load();

                ScenarioMgr.CheckQuitter(this);
                ScenarioMgr.SendScenarioStatus(this, 0, null);

                SetLevel(_Value.Level);
                SetRenownLevel(_Value.RenownRank);
                SetMaxActionPoints(_Value.RenownRank);

                SendLocalizeString(/*Program.Config.Motd*/ _motd, ChatLogFilters.CHATLOGFILTERS_CITY_ANNOUNCE, Localized_text.TEXT_SERVER_MOTD);
                MlInterface.SendMailCount();
                //SendRvrTracker();
                SendInfluenceInfo();
                SendRVRStats();
                SendLockouts();

                WorldMgr.SendKeepStatus(this);

                // Generate rested experience
                if (_Value.LastSeen > 0)
                {
                    if (Level == Program.Config.RankCap)
                        _Value.RestXp = 0;

                    else
                    {
                        int diffSeconds = TCPManager.GetTimeStamp() - _Value.LastSeen;

                        float restHours = Math.Min(168, diffSeconds * 0.00027f);

                        Xp_Info curLev = XpRenownService.GetXp_Info(_Value.Level);

                        if (curLev == null)
                            _Value.RestXp = 0;
                        else
                        {
                            // A rank 17 in the old game managed to fill his bars completely over 4 days of rest.
                            // 150% in 4 days => 37% every day => ~12% every 8 hours. 1.5% an hour?
                            // Assume about 1% an hour, with capital city boost, for players at rank 39.
                            float restXpFactorPerHour = (1 - (Level / 40f) * 0.5f) * 0.01f;

                            // Capital city bonus x2
                            if (_Value.ZoneId == 162 || _Value.ZoneId == 161)
                                restHours *= 2;

                            // Rest XP can't exceed 1.5 times the XP required for the current level
                            _Value.RestXp = Math.Min((uint)(_Value.RestXp + restHours * restXpFactorPerHour * curLev.Xp), (uint)(curLev.Xp * 1.5f));
                        }

                        if (StsInterface.GetTotalStat(Stats.XpReceived) == 0)
                            _Value.RestXp = 0;
                        _Value.LastSeen = TCPManager.GetTimeStamp();
                    }
                }


            }

            base.OnLoad();

            if (!_initialized && !_initInProgress)
            {
                _initInProgress = true;

                // _initialized is being set in StartInit()
                StartInit();
                _initInProgress = false;
            }

            // this is to check if the talisman window was still open if yes move all items back to the inventory
            ItmInterface.TalismanCheck();



            // Add any pending XP or Renown
            if (_Value.PendingXp > 0 || _Value.PendingRenown > 0)
            {
                SendClientMessage("You were rewarded in your absence for contributions made to a recent battle.", ChatLogFilters.CHATLOGFILTERS_RVR);
                AddXp(_Value.PendingXp, false, false);
                _Value.PendingXp = 0;
                AddRenown(_Value.PendingRenown, false);
                _Value.PendingRenown = 0;

                CharMgr.Database.SaveObject(_Value);
            }

            if (GmLevel > 1)
            {

                //if the loaded player has the GM tag (though we exclude DB people) we make them avilable to the gmlist
                if (GmLevel >= (int)EGmLevel.AnyGM && !GmMgr.GmList.Contains(this))
                {
                    SendClientMessage("You have been added to the GM Account List");
                    GmMgr.NotifyGMOnline(this);
                }

            }

            // This is Terror debuff - with this you cannot be ressurected
            if (BuffInterface.GetBuff(5968, this) != null)
            {
                BuffInterface.RemoveBuffByEntry(5968);
            }

        }

        private void SetMaxActionPoints(byte valueRenownRank)
        {
            if (valueRenownRank >= 65 && valueRenownRank < 75)
            {
                MaxActionPoints = 275;
            }
            else if (valueRenownRank >= 75)
            {
                MaxActionPoints = 300;
            }
            else
            {
                MaxActionPoints = 250;
            }
        }

        public void StartInit()
        {
            // Zaru: checking here a try block so that init is able to fail
            try
            {
                RemovePlayer(this);
                Client.State = (int)eClientState.WorldEnter;

                _isCriticallyWounded = false;
                // Block 1
                SendMoney();
                SocInterface.SendSocialLists();
                SendSpeed(Speed);
                StsInterface.SendRenownStats();
                SendRealmBonus();
                SendInited();
                TacInterface.HandleTactics(_Value.GetTactics());
                TacInterface.SendTactics();

                /*SendUpdatehv();// tempary fix for hunters vale pq*/

                // Block 2
                QtsInterface.SendQuests();
                LiveEventInterface.SendLiveEvents();

                SendXpTable();
                if (GldInterface.IsInGuild())
                    GldInterface.Guild.SendGuildInfo(this);

                WorldMgr.GeneralScripts.OnWorldPlayerEvent("SEND_PACKAGES", this, null);

                // Zaru: here it is always: initialized = false
                //if (!_initialized)
                {
                    SendXp();
                    SendRenown();
                    SendStats();


                    //if the loaded player has the GM tag (though we exclude DB people) we make them avilable to the gmlist
                    if (GmLevel >= (int)EGmLevel.AnyGM && !GmMgr.GmList.Contains(this))
                    {
                        SendClientMessage("You have been added to the GM Account List");
                        GmMgr.NotifyGMOnline(this);
                    }
                }
                //if gm toggled invincibility and switched zone then it should still be active.
                if (IsInvulnerable && GmLevel > 1)
                {
                    string temp = "3";
                    List<string> paramValue = temp.Split(' ').ToList();
                    BaseCommands.SetEffectState(this, ref paramValue);
                }
                TokInterface.SendAllToks();
                SendRankUpdate(this);
                SendSkills();
                SendBestiary();
                SendPlayedTime();
                ItmInterface.SendAllItems(this);

                Health = TotalHealth;

                if (PriorityGroup == null)
                    Group.SendNullGroup(this);
                else if (WorldGroup != null && ScnInterface.Scenario == null)
                    WorldGroup.NotifyMemberLoaded();

                SendHealth();

                PacketOut Outl = new PacketOut((byte)Opcodes.S_PLAYER_LOADED, 2);
                Outl.WriteUInt16(0);
                SendPacket(Outl);

                SendSpeed(Speed);

                //if (_Value.Tactic1 != 0)
                //    TacInterface.HandleTactics(_Value.GetTactics());

                SendMoraleAbilities();

                SendStats();

                AbtInterface.SendAbilityLevels();
                AbtInterface.ReloadMastery();
                AbtInterface.SendMasteryPointsUpdate();

                SendClientData();


                /*{
					PacketOut Out = new PacketOut((byte)Opcodes.F_INFLUENCE_INFO);
					Out.WriteHexStringBytes("00000000");
					SendPacket(Out);
				}*/

                ScnInterface.Scenario?.OnPlayerPushed(this);

                OSInterface.SendObjectStates(this);

                // Incorrect sending follows.
                DispatchUpdateState((byte)StateOpcode.RenownTitle, _Value.RenownRank);
                DispatchUpdateState((byte)StateOpcode.ToKTitle, _Value.TitleId);
                SendHelmCloakShowing();

                if (MountID != 0)
                    SendMount(this);

                if (Info.CareerLine == (byte)CareerLine.CAREERLINE_MAGUS)
                    SendDisc(this);

                LoadChannels();

                SendInitComplete();

                if (ImageNum != 0)
                    EvtInterface.AddEvent(SendImageNum, 15000, 1);


                WorldMgr.SendZoneFightLevel(this);

                // Zaru: here it is always: initialized = false
                //if (!_initialized)
                CrrInterface.NotifyInitialized();

                // Same as before.
                //AbtInterface.ReloadMastery();

                //Log.info(Name, "EndInit: Oid " + Oid);
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Could not initialise player : {Name} {ex.Message} {ex.StackTrace}");
                // init failed!
                _initialized = false;
            }
        }

        public void OnClientLoaded()
        {
            PendingDumpStatic = false;
            CrrInterface.NotifyClientLoaded();
        }

        private long _lastLevelResourceAdd;
        private const uint RENOWN_UPDATE_INTERVAL = 5000;
        private const uint PING_TIMEOUT = 30000;

        public enum EDisconnectType
        {
            Unclean,
            Clean,
            Crash
        }

        public EDisconnectType DisconnectType = EDisconnectType.Unclean;

        private void SendEffectStates(Player player)
        {
            foreach (var effectID in EffectStates)
            {
                var Out = new PacketOut((byte)Opcodes.F_OBJECT_EFFECT_STATE);

                Out.WriteUInt16(Oid);
                Out.WriteByte(1);
                Out.WriteByte(effectID);
                Out.WriteByte((byte)(1));
                Out.WriteByte(0);

                if (player == null)
                    DispatchPacket(Out, true);
                else
                    player.SendPacket(Out);
            }
        }
        private void SendImageNum()
        {
            if (ImageNum != 0)
            {
                var Out = new PacketOut((byte)Opcodes.F_PLAYER_IMAGENUM);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16((ushort)ImageNum);
                Out.Fill(0, 18);
                SendPacket(Out);
            }
        }
        public override void Update(long msTick)
        {
            if (Client == null)
            {
                if (DisconnectType == EDisconnectType.Unclean && !IsDisposed && CbtInterface.IsInCombat && CbtInterface.IsPvp)
                {
                    DeathLogger.Debug($"Unclean disconnect for {Name}. Damage sources = {DamageSources.Count}");

                    if (DamageSources.Count > 0)
                        SetDeath(DamageSources.Keys.First());

                    lock (PlayersInRange)
                    {
                        foreach (Player plr in PlayersInRange)
                        {
                            DeathLogger.Debug($"In range = {plr.Name}");

                            if (plr.Realm != Realm)
                                plr.SendClientMessage($"{Name} disconnected uncleanly from the server.");
                        }
                    }
                }

                Dispose();
                return;
            }

            if (LastKeepAliveTime != 0 && LastKeepAliveTime + PING_TIMEOUT < msTick)
            {
                Client.Disconnect("Ping timeout");
                if (!IsDisposed && CbtInterface.IsInCombat && CbtInterface.IsPvp)
                {
                    DeathLogger.Debug($"Ping timeout for {Name}. Damage sources = {DamageSources.Count}");

                    if (DamageSources.Count > 0)
                        SetDeath(DamageSources.Keys.First());

                    lock (PlayersInRange)
                    {
                        foreach (Player plr in PlayersInRange)
                        {
                            DeathLogger.Debug($"In range = {plr.Name}");

                            if (plr.Realm != Realm)
                                plr.SendClientMessage($"{Name} disconnected uncleanly from the server and has been murdered for their cowardice.");
                        }
                    }
                }
                Dispose();
                return;
            }

            UpdateMorale(msTick);

            if (!IsDead)
                UpdateActionPoints(msTick);

            base.Update(msTick);

            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();

            //stopWatch.Start();
            UpdatePackets();

            //if (stopWatch.ElapsedMilliseconds >= 40)
            //    Log.Error("Region" + Region?.RegionId, "Updating " + Name + " (Packets) took " + stopWatch.ElapsedMilliseconds + "ms!");

            if (msTick - _lastLevelResourceAdd > RENOWN_UPDATE_INTERVAL)
            {
                if (_pendingXP > 0)
                {
                    if (PriorityGroup != null)
                        PriorityGroup.AddXpCount(this, _pendingXP);
                    else
                        AddXp(_pendingXP, false, false);
                    _pendingXP = 0;
                }


                _lastLevelResourceAdd = msTick + RENOWN_UPDATE_INTERVAL;

                if (_pingSampleCount < 10)
                    UpdatePing();
            }

            if (_speedPenCount > 0 && msTick > _nextSpeedPenLiftTime)
            {
                --_speedPenCount;
                if (_speedPenCount > 1)
                    StsInterface.Speed = (ushort)(100 - 5 * (_speedPenCount - 1));
                else
                    StsInterface.Speed = 100;
                SendSpeed();
                _nextSpeedPenLiftTime += 1000;
            }

           
            ForceCloseMobsToWander(200);


            if (StealthLevel == 0 || msTick - _lastStealthCheck <= STEALTH_CHECK_INTERVAL)
                return;

            CheckStealth();
            _lastStealthCheck = msTick + STEALTH_CHECK_INTERVAL;



        }

        private void ForceCloseMobsToWander(int distance)
         {


            // Simple random seed.
            var random = new Random(Convert.ToInt32(DateTime.Now.ToString("ss")));
            var creaturesClose = GetInRange<Creature>(distance).Take(StaticRandom.Instance.Next(2, 6));
            // Filter the creatures - less than equal to level 43, not hero or above, not siege, not vendors, not questline.
            var creaturesToWander = creaturesClose.Where(
                x => x.Level <= 43
                && x.Spawn.Proto.Unk2 <= 1001
                && x.Spawn.Proto.CreatureType != 32
                && x.Spawn.Proto.VendorID == 0
                && x.Spawn.Proto.LairBoss == false
                && x.Spawn.Proto.Title == 0
                && x.Spawn.Proto.Emote >= 0
                && !(x is Keep_Creature)
                && x.Spawn.Proto.FinishingQuests == null
                     && !(x is Pet)
                && x.Spawn.Proto.StartingQuests == null);
            foreach (var creature in creaturesToWander)
            {
                // Not in original position.

                // If the mob is not at their spawn point, move them back.  ** Bad performance below
                if (creature.MvtInterface.MoveState == MovementInterface.EMoveState.None)
                {
                    if (!BetweenRanges(creature.Spawn.WorldX - 10, creature.Spawn.WorldX + 10, creature.WorldPosition.X))
                    {
                        var returnHome = new Point3D(creature.Spawn.WorldX, creature.Spawn.WorldY, creature.Spawn.WorldZ);
                        //SendClientMessage($"Asking {creature.Name} to return home");
                        creature.MvtInterface.SetBaseSpeed(50);//50
                        creature.MvtInterface.Move(returnHome);
                        creature.MvtInterface.SetBaseSpeed(100);// 100
                    }
                    else
                    {
                        var point = CalculatePoint(random, 100, creature.Spawn.WorldX, creature.Spawn.WorldY);//200

                        if (creature.LOSHit((ushort)ZoneId, new Point3D(point.X, point.Y, creature.Z)))
                        {
                            //SendClientMessage($"Asking {creature.Name} to move from {creature.Spawn.WorldX},{creature.Spawn.WorldY} to {point.X},{point.Y}");
                            creature.MvtInterface.SetBaseSpeed(50);// 50
                            creature.MvtInterface.Move(point.X, point.Y, creature.Z);
                            creature.MvtInterface.SetBaseSpeed(100);// 100


                            if (creature.MvtInterface.MoveState == MovementInterface.EMoveState.None)
                            {
                                creature.MvtInterface.Move(point.X, point.Y, creature.Z);
                            }

                            if (creature.LOSHit((ushort)ZoneId, new Point3D(point.X, point.Y, creature.Z)))
                            {
                                var returnHome = new Point3D(creature.Spawn.WorldX, creature.Spawn.WorldY, creature.Spawn.WorldZ);

                                creature.MvtInterface.Move(returnHome);

                                creature.MvtInterface.SetBaseSpeed(50);// 100
                         
                            }

                        }
                    }
                }
            }
            
        }

        public static bool BetweenRanges(int a, int b, int number)
        {
            return (a <= number && number <= b);
        }
        private Point2D CalculatePoint(Random random, int radius, int originX, int originY)
        {
            var angle = random.NextDouble() * Math.PI * 2;
            var pointRadius = Math.Sqrt(random.NextDouble()) * radius;
            var x = originX + pointRadius * Math.Cos(angle);
            var y = originY + pointRadius * Math.Sin(angle);
            return new Point2D((int)x, (int)y);
        }

        #region Stuck

        private bool _pendingStuck;
        private long _stuckClearTime;

        public void HandleStuck()
        {
            if (IsBanned)
            {
                if (_lastExileWarned + 30 < TCPManager.GetTimeStamp())
                {
                    TimeSpan exileSpan = TimeSpan.FromSeconds(Client._Account.Banned - TCPManager.GetTimeStamp());

                    string timeString = (exileSpan.Days > 0 ? exileSpan.Days + " days, " : "") + (exileSpan.Hours > 0 ? exileSpan.Hours + " hours, " : "") + (exileSpan.Minutes > 0 ? exileSpan.Minutes + " minutes." : exileSpan.Seconds + " seconds.");

                    SendClientMessage("Your account has been exiled for the following reason:\n" + Client._Account.BanReason + "\nYou may return to the world in " + timeString + "\nThis timer will continue to run even if you are offline.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    _lastExileWarned = TCPManager.GetTimeStamp();
                }
                else
                {
                    SendClientMessage("You try to will yourself back onto the planet. Unsurprisingly, this isn't working out for you.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                }

                return;
            }

            if (CbtInterface.IsInCombat || (ScnInterface.Scenario != null && !ScnInterface.Scenario.HasEnded))
                return;

            if (TCPManager.GetTimeStampMS() < _stuckClearTime)
            {
                CancelQuit(null, null);

                CharacterInfo info = CharMgr.GetCharacterInfo(Info.Career);
                Teleport(info.ZoneId, (uint)info.WorldX, (uint)info.WorldY, (ushort)info.WorldZ, (ushort)info.WorldO);
            }

            else
            {
                if (GmLevel == 1)
                {
                    _stuckClearTime = TCPManager.GetTimeStampMS() + 5000;
                    SendClientMessage("You will be transported to your binding point upon logout.\nTo use the old handling of /stuck instead and warp to Nordland or Norsca, enter the /stuck command again within 5 seconds.");
                    Quit(false, true);
                }

                else
                {
                    CharacterInfo info = CharMgr.GetCharacterInfo(Info.Career);
                    Teleport(info.ZoneId, (uint)info.WorldX, (uint)info.WorldY, (ushort)info.WorldZ, (ushort)info.WorldO);
                }
            }
        }

        #endregion

        //Function to instantly kill this player
        public void Terminate()
        {
            ReceiveDamage(this, int.MaxValue);

            PacketOut damageOut = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 24);

            damageOut.WriteUInt16(Oid);
            damageOut.WriteUInt16(Oid);
            damageOut.WriteUInt16(23584); // Terminate

            damageOut.WriteByte(0);
            damageOut.WriteByte(0); // DAMAGE EVENT
            damageOut.WriteByte(7);

            damageOut.WriteZigZag(-30000);
            damageOut.WriteByte(0);

            DispatchPacketUnreliable(damageOut, true, this);
        }

        public override void Dispose()
        {

            QtsInterface.PublicQuest?.RemovePlayer(this, true);
            CurrentKeep?.RemovePlayer(this);

            SendLeave();
            StopQuit();

            EvtInterface.Notify(EventName.Leave, this, null);

            RemovePlayer(this);

            ScnInterface.Scenario?.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.RemovePlayer, this));

            // Reset to nearest spawn point if logging while dead or too close to BattlefieldObjective/Keep
            if (CurrentArea != null && Zone != null && ScnInterface.Scenario == null)
            {
                if (CurrentArea.IsRvR)
                {

                    // NEWDAWN
                    if (Region.Campaign != null)
                    {
                        Region.Campaign.NotifyLeftLake(this);
                    }
                    else
                    {
                        Region?.Campaign?.NotifyLeftLake(this);
                    }
                }

                if (IsDead || !ValidInTier(Zone.Info.Tier, true) || CurrentArea.IsRvR && (CurrentObjectiveFlag != null || CurrentKeep != null))
                {
                    var closestRespawn = WorldMgr.GetZoneRespawn(Zone.ZoneId, (byte)Realm, this);

                    if (closestRespawn != null)
                    {
                        _Value.WorldX = closestRespawn.X;
                        _Value.WorldZ = closestRespawn.Z;
                        _Value.WorldY = closestRespawn.Y;
                        _Value.WorldO = 0;
                        _Value.ZoneId = (ushort)closestRespawn.ZoneId;
                    }
                }

                ForceSave();
            }

            else if (_pendingStuck || ScnInterface.Scenario != null)
            {
                // Warp a player to their bind point on logout if they were stuck.
                RallyPoint rallyPoint = RallyPointService.GetRallyPoint(Info.Value.RallyPoint);

                if (rallyPoint != null)
                {
                    _Value.WorldX = (int)rallyPoint.WorldX;
                    _Value.WorldZ = rallyPoint.WorldZ;
                    _Value.WorldY = (int)rallyPoint.WorldY;
                    _Value.WorldO = rallyPoint.WorldO;
                    _Value.ZoneId = rallyPoint.ZoneID;

                    Zone_Info info = ZoneService.GetZone_Info(rallyPoint.ZoneID);
                    if (info == null)
                        return;

                    RegionMgr newRegion = WorldMgr.GetRegion(info.Region, true);
                    _Value.RegionId = newRegion.RegionId;
                }

                _pendingStuck = false;

                ForceSave();
            }

            else Save();

            ClearTrackedDamage();

            if (GmMgr.GmList.Contains(this))
                GmMgr.NotifyGMOffline(this);

            base.Dispose();

            if (Client != null)
            {
                Client.Plr = null;
                Client.State = (int)eClientState.CharScreen;
            }
        }

        #region Channel / Chat

        // This is actually done in the following format for the Region and Region-RvR channels:
        // [10 - Main Map | 11 - LoTD]
        // [Tier from 0-4]
        // [Pairing from 1-3 (DG/EC/HD)]
        // [2]
        // [1/2] for client index

        private const string REGION_CHANNEL = "104421";
        private const string REGION_RVR_CHANNEL = "104422";
        private readonly List<string> _channels = new List<string>();
        public int ChannelCount => _channels?.Count ?? 0;

        public long LastEmoteTime { get; set; }

        private bool _channelsLoaded;

        /// <summary>Prevents this player from speaking in Advice chat.</summary>
        public bool AdviceBlocked => Client?._Account.IsAdviceBlocked ?? false;

        /// <summary>Causes messages from this player to be sent to them and only to them.</summary>
        public bool StealthMuted => Client?._Account.IsStealthMuted ?? false;

        public bool IsBanned => Client?._Account.IsBanned ?? false;
        public bool IsSummoned = false;

        private long _lastExileWarned;

        private int _throttleCount;
        private long _lastMessageTime;

        public bool ShouldThrottle()
        {
#if DEBUG
            return false;
#endif
            if (IsBanned)
            {
                if (_lastExileWarned + 30 < TCPManager.GetTimeStamp())
                {
                    TimeSpan exileSpan = TimeSpan.FromSeconds(Client._Account.Banned - TCPManager.GetTimeStamp());

                    string timeString = (exileSpan.Days > 0 ? exileSpan.Days + " days, " : "") + (exileSpan.Hours > 0 ? exileSpan.Hours + " hours, " : "") + (exileSpan.Minutes > 0 ? exileSpan.Minutes + " minutes." : exileSpan.Seconds + " seconds.");

                    SendClientMessage("You have been exiled for the following reason:\n" + Client._Account.BanReason + "\nYou may return to the world in " + timeString + "\nThis timer will continue to run even if you are offline.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                    _lastExileWarned = TCPManager.GetTimeStamp();
                }
                else
                    SendClientMessage("You suddenly remember that in space, no one can hear you scream.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return true;
            }

            long deltaTime = TCPManager.GetTimeStampMS() - _lastMessageTime;

            _lastMessageTime = TCPManager.GetTimeStampMS();

            if (deltaTime < 750)
            {
                ++_throttleCount;

                if (_throttleCount >= 4)
                {
                    if (_throttleCount > 10)
                    {
                        PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_QUIT, 4);
                        Out.WriteHexStringBytes("01000000");
                        SendPacket(Out);
                        return true;
                    }

                    if (_throttleCount > 30)
                        Client?.Disconnect("Excess flood");

                    SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SLOW_DOWN_CHAT_THROTTLE);

                    return true;
                }
            }

            else if (_throttleCount > 0)
            {
                if (deltaTime > 5000)
                    _throttleCount = 0;
                else --_throttleCount;
            }

            return false;
        }

        public bool BlocksChatFrom(Player sender)
        {
            // Can't block incoming GM messages.
            if (sender.GmLevel > 1)
                return false;

            // Exiled players reject all chat.
            if (IsBanned)
                return true;

            // Stealth mutes always function.
            if (this != sender && sender.StealthMuted)
                return true;

            // GMs are not allowed to use Ignore.
            if (GmLevel > 1)
                return false;

            if (SocInterface.HasIgnore(sender.CharacterId))
                return true;

            return false;
        }

        private void LoadChannels()
        {
            if (!_channelsLoaded)
            {
                _channels.Add(REGION_CHANNEL);
                _channels.Add(REGION_RVR_CHANNEL);
                _channels.Add("General");
                _channels.Add("Trade");
                _channels.Add("LFG");
                _channels.Add("Off-Topic");
                _channels.Add("Roleplay");
                _channels.Add("Russian");

                _channelsLoaded = true;
            }

            SendChannels();
        }

        private void ClearChannels()
        {
            _channels.Clear();

            SendChannels();
        }

        private void SendChannels()
        {
            PacketOut channelPacket = new PacketOut((byte)Opcodes.F_CHANNEL_LIST, 2 + _channels.Count * 10);
            channelPacket.WriteByte((byte)_channels.Count);
            for (byte i = 0; i < _channels.Count; ++i)
            {
                channelPacket.WriteByte((byte)(i + 1)); // Actually channel number, not index!
                channelPacket.WritePascalString(_channels[i]);
            }
            channelPacket.WriteByte(0);

            SendPacket(channelPacket);
        }

        private static string _motd = "\nWelcome to WAR: Apocalypse\n"
            + "Rank Cap is: " + Program.Config.RankCap
            + "\nRenown Cap is: " + Program.Config.RenownCap
            + "\nThe server rules are available in-game by entering the command '.rules'.";

        #endregion

        // Network

        #region Ping

        private readonly Ping _pinger = new Ping();
        private byte[] _pingBuffer = new byte[32];

        public ushort Latency;
        private ushort _pingSum;
        private bool _pendingPingRequest, _pingInit;
        private byte _pingSampleCount;

        public void UpdatePing()
        {
            if (_pendingPingRequest)
                return;

            _pendingPingRequest = true;

            if (!_pingInit)
            {
                _pinger.PingCompleted += OnPingReply;
                _pingBuffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                _pingInit = true;
            }

            _pinger.SendAsync(Client.GetIp().Split(':')[0], 500, _pingBuffer, new PingOptions(64, true));

        }

        private void OnPingReply(object sender, PingCompletedEventArgs args)
        {
            _pendingPingRequest = false;

            if (args.Cancelled || args.Reply == null || args.Reply.Status != IPStatus.Success)
                return;

            _pingSum += (ushort)(args.Reply.RoundtripTime);

            ++_pingSampleCount;

            if (_pingSampleCount == 10)
            {
                Latency = (ushort)(_pingSum / 10);

                SendClientMessage("Average round trip time: " + Latency + "ms", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
            }
        }

        #endregion

        #region Packets

        private List<PacketIn> _packetIn = new List<PacketIn>(20);
        private List<PacketOut> _packetOut = new List<PacketOut>(20);

        private List<PacketIn> _internalIn = new List<PacketIn>(20);

        public bool LogPacketFailures;

        public void ReceivePacket(PacketIn packet)
        {
            if (Client == null)
                return;

            if (IsInWorld())
                lock (_packetIn)
                    _packetIn.Add(packet);
            else
                Client.Server.HandlePacket(Client, packet);
        }
        public void SendPacket(PacketOut Out)
        {
            if (Client == null)
                return;

            if (Out == null)
                throw new NullReferenceException("Null packet.");

            if (!Out.Finalized)
                Out.WritePacketLength();

            if (IsInWorld())
            {
                lock (_packetOut)
                    _packetOut.Add(Out);

                if (Region != null && Region.LogPacketVolume)
                    Region.NotifyOutgoingPacket((byte)Out.Opcode, (uint)Out.Length);
            }

            else
                Client.SendPacket(Out);
        }

        public override void DispatchPacketUnreliable(PacketOut Out, bool sendToSelf, Unit sender)
        {
            if (sendToSelf)
                SendPacket(Out);

            if (PlayersInRange.Count > 100)
            {
                if (sender != this)
                    (sender as Player)?.SendPacket(Out);

                return;
            }

            if (!IsActive)
                return;

            lock (PlayersInRange)
                foreach (Player player in PlayersInRange)
                    if (player != this)
                        player.SendCopy(Out);
        }


        public override void DispatchPacket(PacketOut Out, bool sendToSelf, bool playerstate = false)
        {
            if (sendToSelf)
                SendPacket(Out);

            if (!IsActive)
                return;
            // packet throtling for heavy trafic
            byte modulolongrange = 0;
            byte modulomidrange = 0;

            if (WorldMgr.WorldSettingsMgr.GetMovementPacketThrotle() == 1 && playerstate)
            {
                if (PlayersInRange.Count > 500)
                {
                    modulolongrange = 8;
                    modulomidrange = 4;
                }
                else if (PlayersInRange.Count > 350)
                {
                    modulolongrange = 4;
                    modulomidrange = 2;
                }
                else if (PlayersInRange.Count > 250)
                {
                    modulolongrange = 2;
                    modulomidrange = 1;
                }
            }

            lock (PlayersInRange)
                foreach (Player player in PlayersInRange)
                {
                    if (WorldMgr.WorldSettingsMgr.GetMovementPacketThrotle() == 1)
                    {
                        if (player != this)
                        {
                            if (playerstate && modulolongrange != 0 && modulomidrange != 0)
                            {
                                int dis = player.GetDistance(this);
                                if (dis > 200 && SendCounter % modulolongrange != 0)
                                    continue;
                                else if (dis > 100 && SendCounter % modulomidrange != 0)
                                    continue;
                                else
                                    player.SendCopy(Out);
                            }
                            else
                                player.SendCopy(Out);
                        }
                    }
                    else
                    {
                        player.SendCopy(Out);
                    }
                }

            //packets of group members should be sent to all members of group in same zone regardless of distance
            if (PriorityGroup != null)
            {
                lock (PriorityGroup)
                {
                    if (PriorityGroup != null)
                    {
                        foreach (var partyMember in PriorityGroup.GetPlayerListCopy().Where(
                            e => e != null
                            && e.Faction == Faction
                            && e.Zone != null
                            && e.Zone.ZoneId == Zone.ZoneId
                            && GetDistanceSquare(e) > 400))
                        {
                            if (partyMember != this)
                                partyMember.SendCopy(Out);
                        }

                    }
                }
            }
        }

        public void SendCopy(PacketOut Out)
        {
            SendPacket(Out);
            /*
            Out.WritePacketLength();
            byte[] Buf = Out.ToArray();
            PacketOut packet = new PacketOut(0, Out.Capacity) {Position = 0};
            packet.Write(Buf, 0, Buf.Length);
            SendPacket(packet);
            */

        }
        public void GetPacketIn(bool Clear)
        {
            _internalIn.Clear();

            lock (_packetIn)
            {
                _internalIn.AddRange(_packetIn);
                if (Clear) _packetIn.Clear();
            }
        }
        public void UpdatePackets()
        {
            if (Client == null)
                return;

            GetPacketIn(true);
            foreach (PacketIn packet in _internalIn)
                Client.Server.HandlePacket(Client, packet);

            lock (_packetOut)
            {
                if (Program.Config.PacketCollateLength == 0)
                {
                    // Send packets until no more can be sent, or an error occurs
                    while (_packetOut.Count > 0)
                    {
                        if (!Client.SendPacketNoBlock(_packetOut[0]))
                            break;

                        _packetOut.RemoveAt(0);
                    }
                }

                else
                {
                    if (_packetOut.Count > 0)
                    {
                        if (Client.SendPacketsNoBlock(_packetOut, Program.Config.PacketCollateLength))
                            _packetOut.Clear();
                    }
                }
            }
        }

        #endregion

        #region Senders

        public void SendSpeed()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_MAX_VELOCITY, 4);
            if (_isCriticallyWounded)
                Out.WriteUInt16((ushort)(Speed * 0.4f));
            else Out.WriteUInt16(Speed);
            Out.WriteByte((byte)(Speed != 0 ? 1 : 0));
            Out.WriteByte(100);
            SendPacket(Out);

        }

        public void SendSpeed(ushort newSpeed)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_MAX_VELOCITY, 4);
            Out.WriteUInt16(newSpeed);
            Out.WriteByte((byte)(newSpeed != 0 ? 1 : 0));
            Out.WriteByte(100);
            SendPacket(Out);
        }

        public void SendMoney()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_WEALTH, 8);
            Out.WriteUInt32(0);
            Out.WriteUInt32(_Value.Money);
            SendPacket(Out);
        }
        public void SendHealth()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_HEALTH, 24);
            Out.WriteUInt32(Health);
            Out.WriteUInt32(TotalHealth);
            Out.WriteUInt16(ActionPoints); //  Actionpoints
            Out.WriteUInt16(MaxActionPoints); //  MaxAction
            Out.WriteUInt16(_morale); // Control le cercle bleu
            Out.WriteUInt16(0x0E10); // Idem
            SendPacket(Out);
        }
        public void SendInited()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.S_PLAYER_INITTED, 64);

            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            Out.WriteUInt32(Info.CharacterId);
            Out.WriteUInt16((ushort)_Value.WorldZ);
            Out.WriteUInt16(0);
            Out.WriteUInt32((uint)_Value.WorldX);
            Out.WriteUInt32((uint)_Value.WorldY);
            Out.WriteUInt16((ushort)_Value.WorldO);
            Out.WriteByte(0);
            Out.WriteByte((byte)Realm);
            Out.WriteUInt16(0); // XOFFSET - INSTANCING
            Out.WriteUInt16(0); // YOFFSET - INSTANCING
            Out.WriteUInt16(Zone.Info.Region);
            Out.WriteUInt16(1); // instance id
            Out.WriteByte(0);
            Out.WriteByte(Info.Career);
            Out.Fill(0, 6);
            Out.WritePascalString(Program.Rm.Name);
            Out.Fill(0, 3);

            SendPacket(Out);
        }
        public void SendSkills()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO, 12);
            Out.WriteByte(3); // Skills
            Out.Fill(0, 3);
            Out.WriteByte(Info.CareerLine);
            Out.WriteByte(Info.Race);
            Out.WriteUInt32R(_Value.Skills);
            Out.WriteUInt16(_Value.RallyPoint);
            SendPacket(Out);

        }/*// temp fix hunters vale should be that it reads the data from database
        public void SendUpdatehv()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
            Out.WriteUInt16(0016);   // area id
            Out.WriteByte(0x11);
            Out.WriteByte(2);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.Fill(0, 4);
            SendPacket(Out);

            //TOVL temp fix should be that it reads the data from database
            Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE);
            Out.WriteUInt16(0031);   // area id
            Out.WriteByte(0x11);
            Out.WriteByte(2);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.Fill(0, 4);
            SendPacket(Out);
        }*/
        public void SendXpTable()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_EXPERIENCE_TABLE, 2048);
            Out.WritePacketString(@"|1C 00 00 00 0A 96 00 00 00 18 C4 00 00 |................|
|00 28 F0 00 00 00 39 EE 00 00 00 4F B0 00 00 00 |.(....9....O....|
|65 FE 00 00 00 82 32 00 00 00 9E C0 00 00 00 BE |e.....2.........|
|96 00 00 00 E2 04 03 00 00 00 00 00 00 01 05 0E |................|
|00 00 01 30 24 03 00 00 00 00 00 00 01 5A CC 00 |...0$........Z..|
|00 01 89 84 03 00 00 00 00 00 00 01 BC 88 00 00 |................|
|01 EE 74 03 00 00 00 00 00 00 02 29 CA 00 00 02 |..t........)....|
|63 90 03 00 00 00 00 00 00 02 A0 BC 04 00 00 00 |c...............|
|00 00 00 02 D2 6C 03 00 00 00 00 00 00 03 09 62 |.....l.........b|
|03 00 00 00 00 00 00 03 51 2E 03 00 00 00 00 00 |........Q.......|
|00 03 9F 80 03 00 00 00 00 00 00 03 EC 38 03 00 |.............8..|
|00 00 00 00 00 04 3E 04 03 00 00 00 00 00 00 04 |......>.........|
|88 64 03 00 00 00 00 00 00 04 FA 1A 03 00 00 00 |.d..............|
|00 00 00 05 9A 24 03 00 00 00 00 00 00 06 44 24 |.....$........D$|
|03 00 00 00 00 04 00 00 00 00 00 00 06 FC 2A 03 |..............*.|
|00 00 00 00 00 00 07 CE C0 03 00 00 00 00 00 00 |................|
|08 A1 9C 03 00 00 00 00 00 00 09 7F E0 03 00 00 |................|
|00 00 00 00 0A B3 42 03 00 00 00 00 00 00 0B 6E |......B........n|
|A4 03 00 00 00 00 00 00 0C 2E 02 03 00 00 00 00 |................|
|00 00 0D 00 FC 03 00 00 00 00 00 00 0D CC 8A 03 |................|
|00 00 00 00 00 00 0E A1 96 03 00 00 00 00 04 00 |................|
|00 00 00 05 00 00 00 0A 06 00 00 00 00 05 00 00 |................|
|00 50 06 00 00 00 00 05 00 00 00 E6 06 00 00 00 |.P..............|
|00 05 00 00 01 B8 06 00 00 00 00 05 00 00 02 DA |................|
|06 00 00 00 00 05 00 00 04 38 06 00 00 00 00 05 |.........8......|
|00 00 05 DC 06 00 00 00 00 05 00 00 07 D0 06 00 |................|
|00 00 00 05 00 00 0A 00 06 00 00 00 00 05 00 00 |................|
|0C 76 06 00 00 00 00 04 00 00 00 00 05 00 00 0F |.v..............|
|32 06 00 00 00 00 05 00 00 12 2A 06 00 00 00 00 |2.........*.....|
|05 00 00 15 72 06 00 00 00 00 05 00 00 18 F6 06 |....r...........|
|00 00 00 00 05 00 00 1C B6 06 00 00 00 00 05 00 |................|
|00 20 BC 06 00 00 00 00 05 00 00 25 08 06 00 00 |. .........%....|
|00 00 05 00 00 29 90 06 00 00 00 00 05 00 00 2E |.....)..........|
|54 06 00 00 00 00 05 00 00 33 5E 06 00 00 00 00 |T........3^.....|
|04 00 00 00 00 05 00 00 38 A4 06 00 00 00 00 05 |........8.......|
|00 00 3E 30 06 00 00 00 00 05 00 00 43 EE 06 00 |..>0........C...|
|00 00 00 05 00 00 49 F2 06 00 00 00 00 05 00 00 |......I.........|
|50 32 06 00 00 00 00 05 00 00 56 AE 06 00 00 00 |P2........V.....|
|00 05 00 00 5D 66 06 00 00 00 00 05 00 00 64 64 |....]f........dd|
|06 00 00 00 00 05 00 00 6B 94 06 00 00 00 00 05 |........k.......|
|00 00 73 00 06 00 00 00 00 04 00 00 00 00 05 00 |..s.............|
|00 7A A8 06 00 00 00 00 05 00 00 82 8C 06 00 00 |.z..............|
|00 00 05 00 00 8A A2 06 00 00 00 00 05 00 00 92 |................|
|FE 06 00 00 00 00 05 00 00 9B 8C 06 00 00 00 00 |................|
|05 00 00 A4 4C 06 00 00 00 00 05 00 00 AD 52 06 |....L.........R.|
|00 00 00 00 05 00 00 B6 8A 06 00 00 00 00 05 00 |................|
|00 BF F4 06 00 00 00 00 05 00 00 C9 9A 06 00 00 |................|
|00 00 03 00 00 00 00 05 00 00 D3 72 06 00 00 00 |...........r....|
|00 05 00 00 DD 86 06 00 00 00 00 05 00 00 E7 CC |................|
|06 00 00 00 00 05 00 00 F2 44 06 00 00 00 00 05 |.........D......|
|00 00 FC F8 06 00 00 00 00 04 00 00 00 00 05 00 |................|
|01 07 D4 06 00 00 00 00 05 00 01 12 EC 06 00 00 |................|
|00 00 05 00 01 1E 36 06 00 00 00 00 05 00 01 29 |......6........)|
|BC 06 00 00 00 00 05 00 01 35 6A 06 00 00 00 00 |.........5j.....|
|03 00 00 00 00 05 00 01 41 4A 06 00 00 00 00 05 |........AJ......|
|00 01 4E CE 06 00 00 00 00 05 00 01 5E 1E 06 00 |..N.........^...|
|00 00 00 05 00 01 6F 4E 06 00 00 00 00 05 00 01 |......oN........|
|82 90 06 00 00 00 00 05 00 01 98 16 06 00 00 00 |................|
|00 05 00 01 B0 12 06 00 00 00 00 05 00 01 CA AC |................|
|06 00 00 00 00 05 00 01 E8 20 06 00 00 00 00 05 |......... ......|
|00 02 08 A0 06 00 00 00 00 03 00 00 00 00 05 00 |................|
|02 2C 68 06 00 00 00 00 05 00 02 53 A0 06 00 00 |.,h........S....|
|00 00 05 00 02 7E 84 06 00 00 00 00 05 00 02 AD |.....~..........|
|5A 06 00 00 00 00 05 00 02 E0 54 06 00 00 00 00 |Z.........T.....|
|04 00 00 00 00 05 00 03 17 C2 06 00 00 00 00 05 |................|
|00 03 53 CC 06 00 00 00 00 05 00 03 94 B8 06 00 |..S.............|
|00 00 00 05 00 03 DA D6 06 00 00 00 00 05 00 04 |................|
|26 62 06 00 00 00 00 03 00 00 00 00 05 00 04 77 |&b.............w|
|A2 06 00 00 00 00 05 00 04 CE DC 06 00 00 00 00 |................|
|05 00 05 2C 56 06 00 00 00 00 05 00 05 90 6A 06 |...,V.........j.|
|00 00 00 00 05 00 05 FB 54 06 00 00 00 00 04 00 |........T.......|
|00 00 00 05 00 06 6D 6E 06 00 00 00 00 05 00 06 |......mn........|
|E6 FE 06 00 00 00 00 05 00 07 68 54 06 00 00 00 |..........hT....|
|00 05 00 07 F1 CA 06 00 00 00 00 05 00 08 83 BA |................|
|06 00 00 00 00 04 00 00 00 00 05 00 0A 47 3D 06 |.............G=.|
|00 00 00 00 05 00 0A D4 41 06 00 00 00 00 05 00 |........A.......|
|0B 61 43 06 00 00 00 00 05 00 0B EE 46 06 00 00 |.aC.........F...|
|00 00 04 00 00 00 00 05 00 0C 7B 48 06 00 00 00 |..........{H....|
|00 05 00 0D 08 4A 06 00 00 00 00 05 00 0D 95 4C |.....J.........L|
|06 00 00 00 00 05 00 0E 22 4F 06 00 00 00 00 04 |........O......|
|00 00 00 00 05 00 0E AF 51 06 00 00 00 00 05 00 |........Q.......|
|0F 3C 53 06 00 00 00 00 05 00 0F C9 55 06 00 00 |.<S.........U...|
|00 00 05 00 10 56 58 06 00 00 00 00 04 00 00 00 |.....VX.........|
|00 05 00 10 E3 5A 06 00 00 00 00 05 00 11 70 5C |.....Z........p\|
|06 00 00 00 00 05 00 11 FD 5F 06 00 00 00 00 05 |........._......|
|00 12 8A 61 06 00 00 00 00 04 00 00 00 00 05 00 |...a............|
|13 17 63 06 00 00 00 00 05 00 13 A4 65 06 00 00 |..c.........e...|
|00 00 05 00 14 31 68 06 00 00 00 00 05 00 14 BE |.....1h.........|
|6A 06 00 00 00 00 04 00 00 00 00 00 00 00 00 00 |j...............|
|00                                              |.               |");
            SendPacket(Out);
        }
        public void SendXp()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_EXPERIENCE, 16);
            Out.WriteUInt32(_Value.Xp); // cur xp
            Out.WriteUInt32(_currentXp?.Xp ?? 0); // xp required for this level

            if (_currentXp != null && _Value.RestXp != 0)
                Out.WriteUInt32((uint)((_Value.RestXp / (float)_currentXp.Xp) * 100000)); // rested experience factor
            else
                Out.WriteUInt32(0);

            Out.WriteByte(1); // type of xp
            Out.WritePascalString(""); // optional, sent only with BattlefieldObjective capture
            Out.WriteByte(0);
            Out.WriteByte(0);
            SendPacket(Out);
        }
        public void SendLevelUp(Dictionary<byte, ushort> diff)
        {
            SendRankUpdate(null);

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_LEVEL_UP, 96);
            Out.WriteUInt32(0);
            Out.WriteByte((byte)diff.Count);
            foreach (KeyValuePair<byte, ushort> stat in diff)
            {
                Out.WriteByte(stat.Key);
                Out.WriteUInt16(stat.Value);
            }
            SendPacket(Out);
        }
        public void SendRankUpdate(Player Plr)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_RANK_UPDATE, 4);
            Out.WriteByte(Level);
            Out.WriteByte(0);
            Out.WriteUInt16(Oid);

            if (Plr == null)
                DispatchPacket(Out, true);
            else
                Plr.SendPacket(Out);
        }
        public void SendRenown(RewardType type = RewardType.None, string text = null)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_RENOWN, 12);
            Out.WriteUInt32(_Value.Renown);
            Out.WriteUInt32(CurrentRenown.Renown);
            Out.WriteByte(_Value.RenownRank);
            Out.WriteByte((byte)type);
            Out.WriteByte(RenInterface.GetAvailablePoints());
            if (text == null)
                Out.WriteByte(0);
            else
                Out.WritePascalString(text);
            SendPacket(Out);
        }

        public void SendRVRStats()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_RVR_STATS, 39);
            Out.WriteUInt32(_Value.RVRKills);
            Out.WriteUInt32(0);  //??
            Out.WriteUInt32(0);  //??
            Out.Fill(0, 24);
            SendPacket(Out);
        }

        public void SendLockouts()
        {
            List<string> lockouts = _Value.GetAllLockouts();

            if (lockouts.Count == 0)
                return;

            //Check if LockoutTimer Expired and remove the Lockout if needed
            for (int i = 0; i < lockouts.Count; i++)
            {

                if ((Convert.ToInt64(lockouts[i].Split(':')[1])) < TCPManager.GetTimeStamp())
                {
                    _Value.RemoveLockout(lockouts[i]);
                    InstanceService._InstanceLockouts.Remove(lockouts[i]);
                    lockouts.RemoveAt(i);
                    i--;
                }
            }

            if (lockouts.Count == 0)
                return;

            //Send all Lockouts
            PacketOut Out = new PacketOut((byte)Opcodes.F_ACTION_COUNTER_INFO, 0);
            Out.WriteByte(1);
            Out.Fill(0, 2);
            Out.WriteByte((byte)lockouts.Count);
            Out.Fill(0, 8);
            foreach (string lockout in lockouts)
            {
                Out.WriteUInt32(Convert.ToUInt32((Convert.ToInt64(lockout.Split(':')[1]) - TCPManager.GetTimeStamp()) / 60));
                Out.Fill(0, 2);
                InstanceService._InstanceInfo.TryGetValue(uint.Parse(lockout.Split(':')[0].Replace("~", "")), out Instance_Info info);
                Out.WritePascalString(info.Name);

                List<string> deadBosses = new List<string>();
                for (int i = 2; i < lockout.Split(':').Length; i++)
                {
                    deadBosses.Add(lockout.Split(':')[i]);
                }

                for (int i = 0; i < 16; i++)
                {
                    if (i < deadBosses.Count)
                    {
                        InstanceService._InstanceBossSpawns.TryGetValue(uint.Parse(lockout.Split(':')[0].Replace("~", "")), out List<Instance_Boss_Spawn> bosses);
                        uint Bossentry = 0;

                        foreach (Instance_Boss_Spawn bs in bosses)
                        {
                            if (bs.bossId.ToString() == deadBosses[i])
                                Bossentry = bs.Entry;
                        }

                        Out.WriteByte(0);
                        var boss = CreatureService.GetCreatureProto(Bossentry);
                        if (boss == null)
                            Out.WritePascalString("");
                        else
                            Out.WritePascalString(boss.Name);
                    }
                    else
                    {
                        Out.Fill(0, 2);
                    }
                }
                Out.Fill(0, 8);
            }
            SendPacket(Out);
        }

        public void SendBestiary()
        {

            PacketOut Out = new PacketOut((byte)Opcodes.F_ACTION_COUNTER_INFO);
            TokInterface.SendBestiary(ref Out);

            SendPacket(Out);

        }

        public void SendRvrTracker()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 13);
            Out.WriteByte(0);
            Out.WriteByte(1);
            Out.WriteByte(0x11);
            Out.WriteByte(2);
            Out.WriteByte(1);
            Out.Fill(0, 5);
            SendPacket(Out);
        }

        public void SendPlayedTime()
        {
            _lastCheckedTime = (uint)TCPManager.GetTimeStamp();

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAY_TIME_STATS, 12);
            Out.WriteInt32(Time.Day);
            Out.WriteInt32(Time.Hour);
            Out.WriteInt32(Time.Minute);
            SendPacket(Out);
        }

        public void SendSniff(string str)
        {
            string result = "";
            using (StringReader reader = new StringReader(str))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    result += line.Substring(1, 48).Replace(" ", string.Empty);
                }
            }

            result = result.Remove(0, 4);
            byte opcode = Convert.ToByte(result.Substring(0, 2), 16);
            result = result.Remove(0, 2);

            PacketOut Out = new PacketOut(opcode, result.Length / 2);
            Out.WriteHexStringBytes(result);

            SendPacket(Out);
        }

        private void SendRealmBonus()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_REALM_BONUS, 3);
            Out.WriteByte(Realm == Realms.REALMS_REALM_ORDER ? (byte)1 : (byte)2);
            Out.WriteUInt16(0);

            SendPacket(Out);
        }

        public void SendClientData()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CLIENT_DATA, 1032);
            Out.WriteUInt16(0);
            Out.WriteUInt16(1024);
            Out.WriteByte(0);

            Out.Write(Info.ClientData.Data, 0, Info.ClientData.Data.Length);

            SendPacket(Out);
        }

        public void SendMoraleAbilities()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_MORALE_LIST, 12);
            for (byte i = 1; i <= 4; i++)
            {
                ushort morale = _Value.GetMorale(i);
                Out.WriteUInt16(morale);
                if (morale != 0)
                    AbtInterface.AssignMorale(morale, i);
            }
            Out.Fill(0, 3);
            SendPacket(Out);

        }
        public void SendInitComplete()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_INIT_COMPLETE, 2);
            Out.WriteUInt16R(Oid);
            SendPacket(Out);
        }

        private ushort _currentAreaId;

        public void SendChapterBar()
        {
            if (CurrentArea == null)
                return;

#if DEBUG
            //Log.Info("new area ", "id " + CurrentArea.AreaId);
#endif

            PacketOut Out;
            if (CurrentArea.IsRvR)
            {
                //notify entering lakes, required to show rvr tracker. TODO move it to switch_region
                Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
                Out.WriteUInt16(Zone.ZoneId);   //zoneID
                Out.WriteByte(0x11);
                Out.WriteByte(1);
                Out.WriteByte(1);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.Fill(0, 2);
                SendPacket(Out);

                //show rvr tracker
                Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
                Out.WriteUInt16(_currentAreaId);   //zoneID
                Out.WriteByte(0x11);
                Out.WriteByte(2);
                Out.WriteByte(1);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.Fill(0, 2);
                SendPacket(Out);
            }
            if (_currentAreaId > 0)
            {
                Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
                Out.WriteUInt16(_currentAreaId);   // area id
                //Out.WriteUInt16(1);   // area id
                Out.WriteByte(0x11);
                Out.WriteByte(2);
                Out.WriteByte(2);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.Fill(0, 2);
                SendPacket(Out);
            }

            _currentAreaId = CurrentArea.AreaId;

            Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(_currentAreaId);   // area id
            //Out.WriteUInt16(1);   // area id
            Out.WriteByte(0x11);
            Out.WriteByte(2);
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.Fill(0, 4);
            SendPacket(Out);
        }
        public void SendTitleUpdate()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_STATE, 10);
            Out.WriteUInt16(Oid);
            Out.WriteByte((byte)StateOpcode.ToKTitle);
            Out.Fill(0, 3);    // unknown
            Out.WriteUInt16(_Value.TitleId);

            SendPacket(Out);
            foreach (Player p in PlayersInRange)
                p.SendCopy(Out);
        }

        public void SendLocalizeString(ChatLogFilters filter, Localized_text localizeEntry)
        {
            int filterInt = (int)filter;

            PacketOut Out = new PacketOut((byte)Opcodes.F_LOCALIZED_STRING, 64);
            if (filterInt > 0 && filterInt < 256)
            {
                Out.WriteByte(2);
                Out.WriteByte((byte)filter);
            }
            else
            {
                Out.WriteUInt16((ushort)filter);
            }
            Out.WriteUInt16(0);
            Out.WriteUInt16((ushort)localizeEntry);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            Out.WriteByte(1);
            Out.WriteByte(1);

            Out.WriteByte(0);
            Out.WriteByte(0);
            SendPacket(Out);
        }

        public void SendLocalizeString(string message, ChatLogFilters filter, Localized_text localizeEntry)
        {
            int filterInt = (int)filter;

            PacketOut Out = new PacketOut((byte)Opcodes.F_LOCALIZED_STRING, message.Length + 64);
            if (filterInt > 0 && filterInt < 256)
            {
                Out.WriteByte(2);
                Out.WriteByte((byte)filter);
            }
            else
            {
                Out.WriteUInt16((ushort)filter);
            }
            Out.WriteUInt16(0);
            Out.WriteUInt16((ushort)localizeEntry);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            Out.WriteByte(1);
            Out.WriteByte(1);

            // Out.WriteByte(0);
            Out.WriteShortString(message);
            SendPacket(Out);
        }

        public void SendLocalizeString(string message, ushort filter, Localized_text localizeEntry)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_LOCALIZED_STRING, 64);
            Out.WriteUInt16((ushort)filter);

            Out.WriteUInt16(0);
            Out.WriteUInt16((ushort)localizeEntry);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            Out.WriteByte(1);
            Out.WriteByte(1);

            Out.WriteByte(0);
            Out.WritePascalString(message);
            SendPacket(Out);
        }

        public void SendLocalizeString(string[] messages, ChatLogFilters filter, Localized_text localizeEntry)
        {
            int filterInt = (int)filter;

            PacketOut Out = new PacketOut((byte)Opcodes.F_LOCALIZED_STRING, 64);
            if (filterInt > 0 && filterInt < 256)
            {
                Out.WriteByte(2);
                Out.WriteByte((byte)filter);
            }
            else
            {
                Out.WriteUInt16((ushort)filter);
            }
            Out.WriteUInt16(0);
            Out.WriteUInt16((ushort)localizeEntry);
            Out.WriteUInt16(0);
            Out.WriteByte(0);

            Out.WriteByte((byte)messages.Length);
            for (byte i = 0; i < messages.Length; ++i)
            {
                Out.WriteByte(1);
                Out.WriteByte(0);
                Out.WritePascalString(messages[i]);
            }
            SendPacket(Out);
        }


        public void SendDialog(Dialog type, ushort Value)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SHOW_DIALOG, 6);
            Out.WriteUInt16((ushort)type);
            Out.WriteUInt16(0);
            Out.WriteUInt16(Value);
            SendPacket(Out);
        }
        public void SendDialog(Dialog type, ushort objectId, ushort value)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SHOW_DIALOG, 6);
            Out.WriteUInt16((ushort)type);
            Out.WriteUInt16(objectId);
            Out.WriteUInt16(value);
            SendPacket(Out);
        }
        public void SendDialog(Dialog type, string text)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SHOW_DIALOG, 32);
            Out.WriteUInt16((ushort)type);
            Out.WriteByte(0);
            Out.WritePascalString(text);
            SendPacket(Out);
        }
        public void SendDialog(Dialog type, string text, string text2)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SHOW_DIALOG, 48);
            Out.WriteUInt16((ushort)type);
            Out.WriteByte(0);
            Out.WriteStringToZero(text);
            Out.WriteByte(0);
            Out.WriteStringToZero(text2);
            SendPacket(Out);
        }

        public void SendLeave()
        {
            DispatchUpdateState(4, 0); // Unflag client for RvR on quit

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_QUIT, 2);
            Out.WriteByte(0);
            Out.WriteByte((byte)(CloseClient ? 0 : 1)); // 1 = Page de sélection des perso, 0 = Exit
            SendPacket(Out);
        }

        public override void Say(string message, ChatLogFilters chatFilter = ChatLogFilters.CHATLOGFILTERS_SAY)
        {
            if (string.IsNullOrEmpty(message))
                return;

            if (ShouldThrottle())
                return;

            foreach (Player receiver in PlayersInRange.ToArray())
            {
                if (receiver.BlocksChatFrom(this))
                    continue;

                if (receiver.Realm == Realm || receiver.GmLevel > 1 || GmLevel > 1 || (Program.Config.ChatBetweenRealms && chatFilter != ChatLogFilters.CHATLOGFILTERS_SHOUT))
                    receiver.SendMessage(this, message, chatFilter);
                else
                    receiver.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_REALM_RESTRICTED_SAY);
            }

            SendMessage(this, message, chatFilter);
        }

        public void SendMessage(ushort senderOid, string senderName, string messageText, ChatLogFilters chatFilter, byte channelNum = 0)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CHAT, 128);
            Out.WriteUInt16(senderOid);
            Out.WriteByte((byte)chatFilter);
            Out.WriteByte(channelNum);
            Out.Fill(0, 3); // FIRST BYTE - SUBCHANNEL.
            Out.WritePascalString(senderName);
            Out.WriteUInt16((ushort)(messageText.Length + 1));
            Out.WriteStringBytes(messageText);
            Out.WriteByte(0);

            int l = messageText.IndexOf("<LINK");
            if (l >= 0) // We have a link
            {
                Out.WriteByte(1);
                long p = Out.Position;
                Out.WriteByte(0);

                int Count = 0;

                do
                {
                    string[] Link = messageText.Substring(l + 12).Split(' ')[0].TrimEnd('"').Split(':');

                    if (Link[0] == "ITEM")
                    {
                        uint ItemId = uint.Parse(Link[1]);
                        Item_Info Info = ItemService.GetItem_Info(ItemId);
                        if (Info != null)
                        {
                            ++Count;
                            Out.WriteByte(3); // Item
                            Item.BuildItem(ref Out, null, Info, null, 0, 1);

                            if (Info.ItemSet != 0 && ItmInterface != null)
                                ItmInterface.SendItemSetInfoToPlayer(this, Info.ItemSet);
                        }
                    }
                    else if (Link[0] == "GUILD")
                    {
                        uint GuildId = uint.Parse(Link[1]);
                        Guild.Guild Guild = World.Guild.Guild.GetGuild(GuildId);
                        if (Guild != null)
                        {
                            ++Count;
                            Out.WriteByte(5); // Guild
                            World.Guild.Guild.BuildGuild(ref Out, Guild);
                        }
                    }
                    else if (Link[0] == "QUEST")
                    {
                        ushort QuestId = ushort.Parse(Link[1]);
                        Quest Quest = QuestService.GetQuest(QuestId);
                        if (Quest != null)
                        {
                            ++Count;
                            Out.WriteByte(4); // Quest
                            QuestsInterface.BuildQuestInteract(Out, QuestId, 0, 0);
                            Out.WriteUInt16(0);
                            Out.WriteByte(2);
                            QuestsInterface.BuildQuestHeader(Out, this, Quest, true);
                            QuestsInterface.BuildQuestRewards(Out, this, Quest);
                            Out.WriteByte(0);
                            Out.WriteByte((byte)Quest.Objectives.Count);

                            foreach (Quest_Objectives Objective in Quest.Objectives)
                            {
                                Out.WriteByte(0);
                                Out.WriteByte((byte)Objective.ObjCount);
                                Out.WriteByte(0);
                                Out.WriteShortString(Objective.Description);
                            }
                            Out.WriteByte(0);
                        }

                    }
                } while ((l = messageText.IndexOf("<LINK", l + 12)) >= 0);

                Out.Position = p;
                Out.WriteByte((byte)Count);
                Out.Position = Out.Length;
            }

            SendPacket(Out);
        }

        public void SendMessage(Object Sender, string Text, ChatLogFilters Filter)
        {
            if (Sender == null)
                SendMessage(0, "", Text, Filter, 0);
            else SendMessage(Sender.Oid, (Sender is Player ? ((Player)Sender).ChatName : Sender.Name), Text, Filter, 0);
        }

        public void SendMessage(string Text, ChatLogFilters Filter)
        {
            SendMessage(0, Text, "", Filter, 0);
        }
        public void SendClientMessage(string Text, ChatLogFilters filter = ChatLogFilters.CHATLOGFILTERS_SAY, bool debugOnly = false)
        {
#if !DEBUG
            if (debugOnly)
                return;
#endif
            SendLocalizeString(Text, filter, Localized_text.CHAT_TAG_DEFAULT);
        }
        public void SendObjectiveText(string Text)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_OBJECTIVE_INFO, 32);
            Out.WriteUInt32(0); // Entry
            Out.WriteByte(0); // 1
            Out.WriteByte(1); // 2
            Out.WriteByte(0); // 1
            Out.WriteUInt16(0);
            Out.WriteStringToZero(Text);
            Out.WriteUInt16(0);
            Out.WriteUInt16(0); // LastUpdatedTime
            Out.WriteUInt16(0);
            Out.WriteByte(0);
            SendPacket(Out);
        }

        public override void SendMeTo(Player plr)
        {
            if (plr == null || plr.IsDisposed || plr.Client == null)
                return;

            if (IsDisposed || Client == null || (StealthLevel != 0 && !_spotters.Contains(plr)) || IsBanned || plr.IsBanned)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_CREATE_PLAYER, 128);
            Out.WriteUInt16((ushort)Client.Id);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(Info.ModelId);
            Out.WriteUInt16(Info.CareerLine);
            Out.WriteUInt16((ushort)Z);
            Out.WriteUInt16(Zone.ZoneId);

            Out.WriteUInt16((ushort)X);
            Out.WriteUInt16((ushort)Y);
            Out.WriteUInt16(Heading);

            Out.WriteByte(_Value.Level);
            Out.WriteByte(StsInterface.BolsterLevel); // Effective level

            int showHerald = _Value.GearShow;

            if ((showHerald & 4) > 0) //FIX: 4 turns on radar around player, not heraldry, heraldry visiblity is handled in F_UPDATE_STATE AND F_PLAYER_INVENTORY
                showHerald &= ~(4);

            Out.WriteByte((byte)showHerald); // Can also control collision detection

            Out.WriteByte((byte)(Faction + (IsDead ? 1 : 0))); // Faction
            Out.WriteUInt16(_Value.TitleId); // Player Title ID

            Out.Write(Info.bTraits, 0, Info.bTraits.Length);
            Out.Fill(0, 8);
            Out.WriteUInt16(CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ENEMY)?.Oid ?? 0);  // Offensive Target OID
            Out.WriteUInt16(CbtInterface.GetTarget(TargetTypes.TARGETTYPES_TARGET_ALLY)?.Oid ?? 0);  // Defensive Target OID

            Out.WriteByte(Info.Race);
            Out.WriteByte(Info.Sex);
            Out.WriteByte(_Value.RenownRank);
            Out.WriteByte(PctHealth);
            Out.Fill(0, 8);

            if (Info.TempFirstName == null)
                Out.WritePascalString(GenderedName);
            else
                Out.WritePascalString(Info.TempFirstName);


            if (Info.TempLastName != null)
                Out.WritePascalString(Info.TempLastName);
            else if (CrrInterface.ExperimentalMode)
                Out.WritePascalString(Info.Surname + "*^M");
            else
                Out.WritePascalString(Info.Surname + "^M"); // Last name always takes ^M as its gender marker

            Out.WritePascalString(GldInterface.GetGuildName()); // guild name
            Out.Fill(0, 4);

            plr.SendPacket(Out);

            //OSInterface.SendObjectStates(Plr);

            base.SendMeTo(plr);

            if (Info.CareerLine == (byte)CareerLine.CAREERLINE_MAGUS)
            {
                SendDisc(plr);

                Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(475); // Magus' badge
                Out.WriteByte(1); // disc effect
                Out.WriteByte(0);
                Out.WriteByte(1);
                Out.WriteByte(0);

                plr.SendPacket(Out);

                Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(475); // Magus' badge
                Out.WriteByte(6); // unk
                Out.WriteByte(0);
                Out.WriteByte(1);
                Out.WriteByte(0);

                plr.SendPacket(Out);

                Out = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 10);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(475); // Magus' badge
                Out.WriteByte(7); // unk
                Out.WriteByte(0);
                Out.WriteByte(1);
                Out.WriteByte(0);

                plr.SendPacket(Out);
            }

            //send overlayed NPC model
            if (ImageNum != 0)
            {
                Out = new PacketOut((byte)Opcodes.F_PLAYER_IMAGENUM);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16((ushort)ImageNum);
                Out.Fill(0, 18);
                plr.SendPacket(Out);
            }

            if (EffectStates.Count > 0)
            {
                plr.EvtInterface.AddEvent(() =>
                {
                    SendEffectStates(plr);

                }, 1000, 1);
            }
        }

        public void SendTradeSkill(byte skill, byte level)
        {
            if (_Value.CraftingBags != 4)
            {
                if (_Value.CraftingBags == 0 && (_Value.GatheringSkillLevel >= 100 || _Value.CraftingSkillLevel >= 100))
                    _Value.CraftingBags = 1;
                if (_Value.CraftingBags == 1 && (_Value.GatheringSkillLevel >= 100 && _Value.CraftingSkillLevel >= 100) || (_Value.GatheringSkillLevel == 200 || _Value.CraftingSkillLevel == 200))
                    _Value.CraftingBags = 2;
                if (_Value.CraftingBags == 2 && (_Value.GatheringSkillLevel >= 100 && _Value.CraftingSkillLevel == 200) || (_Value.GatheringSkillLevel == 200 && _Value.CraftingSkillLevel >= 100))
                    _Value.CraftingBags = 3;
                if (_Value.CraftingBags == 3 && _Value.GatheringSkillLevel == 200 && _Value.CraftingSkillLevel == 200)
                    _Value.CraftingBags = 4;
            }


            PacketOut Out = new PacketOut((byte)Opcodes.F_TRADE_SKILL_UPDATE, 6);
            Out.WriteByte(skill);   // skill  1  Butchering  2 Scavaging 3 Cultivation 4 Apotekari 5 Talisman 6  Salvage
            Out.Fill(0, 2);
            Out.WriteByte(level);   // lvl
            Out.WriteByte((byte)ItmInterface.GetMaxCraftingItemSlots());   // bagspace
            Out.WriteByte(0);
            SendPacket(Out);
        }

        public override void SetZone(ZoneMgr newZone)
        {
            if (newZone == null)
                throw new NullReferenceException("NULL ZoneMgr was passed in SetZone.");
            Zone = newZone;
            _Value.ZoneId = newZone.ZoneId;
            //if (!MoveBlock)
            //CheckZoneValidity();
        }

        public bool MoveBlock;

        public void SendSwitchRegion(ushort zoneID)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SWITCH_REGION, 20);
            Out.WriteUInt16(zoneID);
            Out.Fill(0, 5);
            Out.WriteByte(1); // Instance ID?
            Out.WriteByte(2);
            Out.Fill(0, 11);
            SendPacket(Out);
        }

        #endregion

        //

        #region Standard

        public Standard PlantedStandard { get; set; }

        public WeaponStance WeaponStance;

        public void Standard(uint SpellId, bool bypassGuildChecks = false)
        {
            if (!bypassGuildChecks)
            {
                if (!GldInterface.IsInGuild())
                {
                    SendClientMessage("You are not in a guild, so cannot use a Guild Standard");
                    return;
                }
                if (GldInterface.Guild.Info.Level < 6)
                {
                    SendClientMessage("Your Guild has not achieved high enough standing, so cannot use a Guild Standard");
                    return;
                }
            }
            else
            {
                SendClientMessage("Standard - Bypassing Guild Checks");
            }

            byte banner = 0;

            if (SpellId == 14500)
                banner = 0;
            else if (SpellId == 14501)
                banner = 1;
            else if (SpellId == 14502)
                banner = 2;

            if (SpellId == 14501 && GldInterface.Guild.Info.Level < 18)
                return;

            if (SpellId == 14502 && GldInterface.Guild.Info.Level < 30)
                return;

            if (WeaponStance == WeaponStance.Standard)
            {
                WeaponStance = WeaponStance.Melee;
                SendStance(WeaponStance);

                //AbtInterface.RemoveGrantedAbility(14508);

                foreach (uint buffspellid in GldInterface.Guild.GetBannerBuffs(banner))
                {
                    if (buffspellid > 0)
                        //should be done differently banner buffs might have changed since calling the banner
                        Log.Info("removeBannerBuff ", " " + buffspellid);
                }
            }
            else
            {
                if (ItmInterface.GetItemInSlot(14) == null)
                {
                    return;
                }

                if (ItmInterface.GetItemInSlot(14).Info.SpellId != SpellId)
                {
                    return;
                }

                if (PlantedStandard != null)
                {
                    SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GUILD_BANNER_WARNING_ACTIVE);
                    return;
                }

                Guild_member gm;
                if (GldInterface.Guild.Info.Members.TryGetValue(CharacterId, out gm))
                {
                    if (gm.StandardBearer)
                    {
                        WeaponStance = WeaponStance.Standard;
                        SendStance(WeaponStance);
                        //this.GetPlayer().PlayEffect();

                        //AbtInterface.GrantAbility(14508);

                        //add temp spell 14508  Plant standard
                        //add standards blessing AbilityID = 14524
                        //add movement speed 1 25% ?

                        if (GldInterface.Guild.Info.Level >= 12)
                        {
                            //add slam spell AbilityID = 14527
                        }
                        if (GldInterface.Guild.Info.Level >= 22)
                        {
                            //add ralley spell AbilityID = 14526
                        }
                        if (GldInterface.Guild.Info.Level >= 27)
                        {
                            //add movement speed 2 (35%)
                        }
                        if (GldInterface.Guild.Info.Level >= 39)
                        {
                            //add movement speed 3 (45%)
                        }


                        foreach (uint buffspellid in GldInterface.Guild.GetBannerBuffs(banner))
                        {
                            if (buffspellid > 0)
                                Log.Info("AddBannerBuff ", " " + buffspellid);
                        }
                    }
                }
            }
        }

        public void PlantStandard()
        {
            if (WeaponStance == WeaponStance.Standard)
            {
                Creature_spawn spawn = new Creature_spawn();
                spawn.Guid = (uint)CreatureService.GenerateCreatureSpawnGUID();
                Creature_proto proto = new Creature_proto();
                proto.MinScale = 50;
                proto.MaxScale = 50;
                spawn.BuildFromProto(proto);
                spawn.WorldO = _Value.WorldO;
                spawn.WorldY = _Value.WorldY;
                spawn.WorldZ = _Value.WorldZ;
                spawn.WorldX = _Value.WorldX;
                spawn.ZoneId = Zone.ZoneId;
                spawn.Icone = 18;
                spawn.WaypointType = 0;
                spawn.Proto.MinLevel = spawn.Proto.MaxLevel = GldInterface.Guild.Info.Level;

                uint SpellId = ItmInterface.GetItemInSlot(14).Info.SpellId;
                byte banner = 0;

                if (SpellId == 14500)
                    banner = 0;
                else if (SpellId == 14501)
                    banner = 1;
                else if (SpellId == 14502)
                    banner = 2;


                PlantedStandard = new Standard(spawn, this, banner);
                Region.AddObject(PlantedStandard, spawn.ZoneId);

                Standard(0);
                ItmInterface.DeleteItem(14, 1);

            }
        }

        public void SendStance(WeaponStance stance)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_GET_ITEM, 8);
            Out.Fill(0, 2);
            Out.WriteByte((byte)stance);
            Out.Fill(0, 2);
            SendPacket(Out);

            ItmInterface.SendEquipped(null);
        }

        #endregion

        #region Mounting

        private NewBuff _mountBuff;

        public void MountFromBuff(NewBuff mountBuff, ushort mountID, ushort mountArmor, float speedFactor)
        {
            _mountBuff = mountBuff;

            if (Info.CareerLine == (byte)CareerLine.CAREERLINE_MAGUS)
            {
                _mountDisc = mountID;
                MountArmor = mountArmor;
                SendDisc();
            }
            else
            {
                MountID = mountID;
                MountArmor = mountArmor;
                SendMount();
            }

            Pet pet = CrrInterface.GetTargetOfInterest() as Pet;
            if (pet != null && !pet.PendingDisposal)
            {
                ((CombatInterface_Pet)pet.CbtInterface).IgnoreDamageEvents = true;
                pet.AiInterface.Debugger?.SendClientMessage("[MR] Scaling pet speed by " + speedFactor + ".");
                pet.MvtInterface.ScaleSpeed(speedFactor);
                pet.MvtInterface.Follow(this, 10, 20);
            }
        }

        public void MountBackpackFromBuff(NewBuff mountBuff, ushort backpackModelId, float speedFactor)
        {
            _mountBuff = mountBuff;

            SetGearShowing(2, false);

            Item back = ItmInterface.GetItemInSlot((ushort)ItemSlots.ITEMSLOTS_BACK);

            if (back != null && back.ModelId != backpackModelId)
                CloakModel = ItmInterface.GetItemInSlot((ushort)ItemSlots.ITEMSLOTS_BACK).ModelId;

            ItmInterface.SendBackpack(this, 13, backpackModelId);

            Pet pet = CrrInterface.GetTargetOfInterest() as Pet;
            if (pet != null && !pet.PendingDisposal)
            {
                ((CombatInterface_Pet)pet.CbtInterface).IgnoreDamageEvents = true;
                pet.AiInterface.Debugger?.SendClientMessage("[MR] Scaling pet speed by " + speedFactor + ".");
                pet.MvtInterface.ScaleSpeed(speedFactor);
                pet.MvtInterface.Follow(this, 10, 20);
            }
        }

        public void DismountBackpackFromBuff()
        {
            SetGearShowing(2, false);

            ItmInterface.SendBackpack(this, 13, 0);

            Pet pet = CrrInterface.GetTargetOfInterest() as Pet;
            if (pet != null && !pet.PendingDisposal)
            {
                ((CombatInterface_Pet)pet.CbtInterface).IgnoreDamageEvents = false;
                pet.AiInterface.Debugger?.SendClientMessage("[MR] Restoring pet speed factor to " + pet.SpeedMult + ".");
                pet.MvtInterface.ScaleSpeed(pet.SpeedMult);
            }

            CurrentSiege?.SiegeInterface.RemovePlayer(this);
        }

        public bool IsMounted => _mountBuff != null && !_mountBuff.BuffHasExpired;

        public uint CloakModel;

        public HoldObject HeldObject;

        public override void Dismount()
        {
            if (_mountBuff != null && !_mountBuff.BuffHasExpired)
            {
                _mountBuff.BuffHasExpired = true;
                SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DISMOUNTED);
            }
        }

        public void DismountFromBuff()
        {
            if (Info.CareerLine == (byte)CareerLine.CAREERLINE_MAGUS)
            {
                _mountDisc = 0;
                MountArmor = 0;
                SendDisc();
            }
            else
            {
                MountID = 0;
                MountArmor = 0;

                PacketOut Out = new PacketOut((byte)Opcodes.F_MOUNT_UPDATE, 20);
                Out.WriteUInt16(Oid);
                Out.Fill(0, 18);

                DispatchPacket(Out, true);
            }

            Pet pet = CrrInterface.GetTargetOfInterest() as Pet;
            if (pet != null && !pet.PendingDisposal)
            {
                ((CombatInterface_Pet)pet.CbtInterface).IgnoreDamageEvents = false;
                pet.AiInterface.Debugger?.SendClientMessage("[MR] Restoring pet speed factor to " + pet.SpeedMult + ".");
                pet.MvtInterface.ScaleSpeed(pet.SpeedMult);
            }

            CurrentSiege?.SiegeInterface.RemovePlayer(this);
        }

        private static readonly ushort[] _discIDs = { 507, 1356, 1357, 1358, 6390 };

        private ushort _mountDisc;

        private void SendDisc(Player Plr = null)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_MAGUS_DISC_UPDATE, 4);

            Out.WriteUInt16(Oid);
            if (_mountDisc != 0)
                Out.WriteUInt16(_mountDisc);
            else
                Out.WriteUInt16(_discIDs[(ushort)((Level) * 0.1f)]);

            if (Plr == null)
                DispatchPacket(Out, true);
            else Plr.SendPacket(Out);
        }

        #endregion

        #region Appearance/Titles/Name

        public void SetGearShowing(byte aBit, bool bEnabled)
        {
            if (bEnabled)
                _Value.GearShow |= aBit;
            else
                _Value.GearShow &= (byte)~aBit;

            SendHelmCloakShowing();
        }
        public void SendHelmCloakShowing()
        {
            DispatchUpdateState((byte)StateOpcode.HelmCloak,
                            (byte)((_Value.GearShow & 1) > 0 ? 1 : 0),
                            (byte)((_Value.GearShow & 2) > 0 ? 1 : 0),
                            (byte)((_Value.GearShow & 4) > 0 ? 1 : 0));
        }

        public void SetTitleId(ushort titleId)
        {
            // Calling code is responsible for ensuring this title is unlocked for player
            _Value.TitleId = titleId;
            SendTitleUpdate();
        }
        public void SetLastName(string lastName)
        {
            Info.Surname = lastName;

            UpdateLastName(lastName);

            CharMgr.Database.SaveObject(Info);
        }

        private void UpdateLastName(string lastName)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_UPDATE_LASTNAME, 32);
            Out.WriteUInt16(Oid);
            Out.WritePascalString(CrrInterface.ExperimentalMode ? lastName + "*" : lastName);
            DispatchPacket(Out, true);
        }

        #endregion

        // Resources

        #region Xp

        private Xp_Info _currentXp;

        private uint _xpPool;
        private uint _pendingXP;

        public void SetLevel(byte newLevel)
        {
            _currentXp = XpRenownService.GetXp_Info(newLevel);
            _Value.Level = newLevel;
            Dictionary<byte, ushort> values = ApplyLevel();

            if (Loaded && _initialized)
            {
                SendLevelUp(values);
                SendXp();
            }

            // Reset the bounty score for the player upon gaining an XP Level
            BountyManagerInstance?.ResetCharacterBounty(CharacterId, this);


            //Check area for bolster
            CheckArea();

            EvtInterface.Notify(EventName.OnLevelUp, this, null);
        }


        /// <summary>
        /// Used for kill rewards.
        /// </summary>
        public void AddXp(uint xp, float scaleFactor, bool shouldPool, bool scalesWithRest)
        {
            // _logger.Trace($"Player {this.Name}");
            // _logger.Trace($"AddXp. XP {xp} scalefactor {scaleFactor} shouldPool {shouldPool} scaleswithRest {scalesWithRest}");

            var statScaleFactor = StsInterface.GetTotalStat(Stats.XpReceived);
            // Instructions Boon
            if (statScaleFactor <= 0)
                scaleFactor = 0;

            scaleFactor += statScaleFactor * 0.01f;
            // _logger.Trace($"AddXp. Scalefactor {scaleFactor}");
            xp = (uint)(xp * scaleFactor);
            // _logger.Debug($"PVP AddXp. XP {xp} for {this.Name} Scale: {scaleFactor}");
            InternalAddXp(xp, shouldPool, scalesWithRest);
        }
        public void AddXp(uint xp, bool shouldPool, bool scalesWithRest)
        {
            // _logger.Trace($"Player {this.Name}");
            // _logger.Trace($"AddXp. XP {xp} shouldPool {shouldPool} scaleswithRest {scalesWithRest}");
            float scaleFactor = 1 + StsInterface.GetTotalStat(Stats.XpReceived) * 0.01f;
            // _logger.Trace($"AddXp. Scalefactor {scaleFactor} EffectiveLevel {EffectiveLevel} Level {Level}");
            if (scaleFactor < 0)
                scaleFactor = 0;
            if (EffectiveLevel == Level)
            {
                xp = (uint)(xp * scaleFactor);
            }
            // _logger.Debug($"PVE AddXp. XP {xp} for {this.Name} Scale: {scaleFactor}");
            InternalAddXp(xp, shouldPool, scalesWithRest);
        }
        private void InternalAddXp(uint xp, bool shouldPool, bool scalesWithRest)
        {
            RewardLogger.Trace($"{xp} XP awarded to {Name}");
            if (shouldPool)
                _xpPool += (uint)(xp * 0.25f);

            EvtInterface.Notify(EventName.OnAddXP, null, xp);

            if (_currentXp == null)
                return;

            if (Level >= Program.Config.RankCap)
                return;

            if (scalesWithRest && _Value.RestXp > 0)
            {
                uint bonusXpFactor = Math.Min(_Value.RestXp, xp);

                xp += bonusXpFactor;

                if (_Value.RestXp > xp)
                    _Value.RestXp -= xp;
                else
                    _Value.RestXp = 0;
            }

            if (xp > 80000)
            {
                Log.Error("AddKillXP (mult)", "Player received outrageous xp  (" + xp + ") - " + "(" + shouldPool.ToString() + ") - " + Environment.StackTrace);
                SendClientMessage("You somehow gained an amount of xp larger than the system allows (" + xp + "). This gain has been prevented.");
                return;
            }

            if (xp + _Value.Xp > _currentXp.Xp)
                LevelUp(xp + _Value.Xp - _currentXp.Xp);
            else
            {
                _Value.Xp += xp;
                SendXp();
            }
        }

        public void AddPendingXP(uint addAmount)
        {
            _pendingXP += addAmount;
        }
        public void LevelUp(uint restXp)
        {

            if (Level >= Program.Config.RankCap)
                return;

            _Value.Xp = 0;
            SetLevel((byte)(Level + 1));

            if (_currentXp == null)
                return;

            InternalAddXp(restXp, false, false);

            AbtInterface.OnPlayerLeveled((byte)(Level - 1), Level);

            // Reset the bounty score for the player upon gaining an XP Level
            BountyManagerInstance?.ResetCharacterBounty(CharacterId, this);

            RemoveBolster();
            TryBolster(0, CurrentArea);

            if (IsBanned && !MoveBlock && Zone != null && Zone.ZoneId != 175)
                Teleport(175, 1530613, 106135, 4297, 1700);
            else
                CheckArea();
        }
        public Dictionary<byte, ushort> ApplyLevel()
        {
            Dictionary<byte, ushort> Diff = new Dictionary<byte, ushort>();

            List<CharacterInfo_stats> newStats = CharMgr.GetCharacterInfoStats(Info.CareerLine, _Value.Level);
            if (newStats == null || newStats.Count <= 0)
                return Diff;

            foreach (CharacterInfo_stats stat in newStats)
            {
                ushort Base = StsInterface.GetBaseStat(stat.StatId);

                if (stat.StatValue > Base)
                    Diff.Add(stat.StatId, (ushort)(stat.StatValue - Base));

                StsInterface.SetBaseStat((Stats)stat.StatId, stat.StatValue);
            }

            StsInterface.ApplyStats();
            return Diff;
        }

        #endregion

        #region Renown

        public Renown_Info CurrentRenown;

        private uint _renownPool;
        private uint _pendingRenown;

        public void SetRenownLevel(byte level)
        {
            if (level < _Value.RenownRank)
                _Value.Renown = 0;

            CurrentRenown = XpRenownService.GetRenown_Info(level);
            _Value.RenownRank = level;
            //_Value.Renown = 0;

            // Reset the bounty score for the player upon gaining an XP Level
            BountyManagerInstance?.ResetCharacterBounty(CharacterId, this);

            if (level % 10 == 0)
                DispatchUpdateState(8, _Value.RenownRank); // Update renown title.

            if (Loaded && _initialized)
                SendRenown();
        }

        // Used for destroying mobs that give renown (such as siege)
        public void AddRenown(uint renown, float scaleFactor, bool shouldPool)
        {
            scaleFactor += StsInterface.GetTotalStat(Stats.RenownReceived) * 0.01f;
            renown = (uint)(renown * scaleFactor);
            AddRenown(renown, shouldPool);
        }

        public void AddNonScalingRenown(uint renown, bool shouldPool, RewardType type = RewardType.None, string rewardString = null)
        {
            if (renown == 0)
                return;
            RewardLogger.Trace($"{renown} RP (non-scaling) awarded to {Name} for {rewardString} ");
            InternalAddRenown(renown, shouldPool, type, rewardString);
        }

        public void AddRenown(uint renown, bool shouldPool, RewardType type = RewardType.None, string rewardString = null)
        {
            if (renown == 0)
                return;

            // apply renown rate from server
            if (Program.Config.RenownRate > 0)
                renown *= (uint)Program.Config.RenownRate;

            // apply aao bonus
            if ((ScnInterface == null || ScnInterface.Scenario == null)
                && (type == RewardType.None || type == RewardType.Kill || type == RewardType.Assist))
            {
                renown = Convert.ToUInt32(Math.Round((1f + AAOBonus) * renown, 0));
            }

            RewardLogger.Trace($"{renown} RP awarded to {Name} for {rewardString} [AAO:{Convert.ToUInt32(Math.Round((1f + AAOBonus)))}]");
            InternalAddRenown(renown, shouldPool, type, rewardString);
        }

        private void InternalAddRenown(uint renown, bool shouldPool, RewardType type = RewardType.None, string rewardString = null)
        {
            if (!_initialized)
            {
                Log.Error("InternalAddRenown", "Attempted to add renown to uninitialized player " + Name + "\n" + Environment.StackTrace);
                return;
            }

            Character_value value = _Value;

            if (value == null)
                throw new NullReferenceException("NULL Character_value for " + Name);

            if (renown > 500000)
            {
                renown = 500000;
                Log.Error("AddKillRenown (mult)", "Player received outrageous renown level (" + renown + ") - " + "(" + type.ToString() + ") - " + "(" + shouldPool.ToString() + ") - " + Environment.StackTrace);
                SendClientMessage("You somehow gained an amount of renown larger than the system allows (" + renown + "). You have been given the cap.");
                return;
            }

            RewardLogger.Trace($"{renown} RP awarded to {Name} for {rewardString}");

            EvtInterface.Notify(EventName.OnAddRenown, this, renown);

            if (shouldPool)
                _renownPool += (uint)(renown * 0.25f);

            if (value.RenownRank >= Program.Config.RenownCap)
                return;

            if (CurrentRenown == null)
                throw new NullReferenceException("NULL CurrentRenown for " + Name);

            if (value.Renown + renown > CurrentRenown.Renown)
            {
                RenownUp(value.Renown + renown - CurrentRenown.Renown);
                AbtInterface.SendMasteryPointsUpdate();
            }
            else
            {
                value.Renown += renown;
                SendRenown(type, rewardString);
            }
        }

        public void AddKillRenown(uint renown, float scaleFactor, Player killer, Player victim, int participants = 1)
        {
            if (renown > 10000)
            {
                Log.Error("AddKillRenown (mult)", "Player received outrageous renown level (" + renown + ") - " + "(" + killer.Name + ") - " + "(" + victim.Name + ") - " + "(" + participants.ToString() + ") - " + "(" + scaleFactor.ToString() + ") - " + Environment.StackTrace);
                SendClientMessage("You somehow gained an amount of renown larger than the system allows (" + renown + "). This gain has been prevented.");
                return;
            }

            RewardLogger.Trace($"Kill Renown {renown} RP awarded to {killer.Name} for kiling {victim.Name} #participants {participants}");

            scaleFactor += StsInterface.GetTotalStat(Stats.RenownReceived) * 0.01f;

            var bonus = (scaleFactor - 1) >= 0 ? (uint)(renown * (scaleFactor - 1)) : 0;

            //if (bonus > 0)
            //    killer.SendClientMessage($"You gain {bonus} Renown points for fighting near a Battlefield Objective!");



            renown = (uint)(renown * scaleFactor);
            AddKillRenown(renown, killer, victim, participants);
        }

        public void AddKillRenown(uint renown, Player killer, Player victim, int participants = 1)
        {
            // removed as not required in bounty system.
            return;

            int aaoMult = 0;
            Realms aaoRealm = Realms.REALMS_REALM_NEUTRAL;
            if (Region != null && Region.Campaign != null)
            {

                if (Region.Campaign != null)
                {
                    aaoMult = Math.Abs(Region.Campaign.AgainstAllOddsMult);
                    if (aaoMult != 0)
                        aaoRealm = Region.Campaign.AgainstAllOddsMult > 0 ? Realms.REALMS_REALM_DESTRUCTION : Realms.REALMS_REALM_ORDER;
                }
                if (aaoMult != 0 && aaoRealm != Realms.REALMS_REALM_NEUTRAL && Realm != aaoRealm)
                    renown = Math.Max(1, renown);
            }

            if (Program.Config.RenownRate > 0)
                renown *= (uint)Program.Config.RenownRate;

            renown = (uint)(renown * GetKillRewardScaler(victim));

            if (Constants.DoomsdaySwitch > 0 && renown < 20)
            {
                if (Realm == aaoRealm || aaoRealm == Realms.REALMS_REALM_NEUTRAL)
                {
                    renown = 20;
                }
            }
            if (Constants.DoomsdaySwitch > 0 && renown < 1)
                renown = 1;

            if (renown == 0)
                return;

            EvtInterface.Notify(EventName.OnAddRenown, this, renown);

            if (renown > 80000)
            {
                if (victim != null && killer != null)
                    Log.Error("AddKillRenown (mult)", "Player received outrageous renown level (" + renown + ") - " + "(" + killer.Name + ") - " + "(" + victim.Name + ") - " + "(" + participants.ToString() + ") - " + Environment.StackTrace);
                SendClientMessage("You somehow gained an amount of renown larger than the system allows (" + renown + "). This gain has been prevented.");
                return;
            }

            _renownPool += (uint)(renown * 0.25f);


            // RB   4/17/2016   Lock Renown Rank to double the Career Rank. Cannot gain renown if Renown Rank is higher than 2*Career Rank.
            if (_Value.RenownRank >= Program.Config.RenownCap || (_Value.Level < 32 && _Value.RenownRank > (2 * _Value.Level)))
                return;

            RewardType type = this == killer ? RewardType.Kill : RewardType.Assist;
            string text = this == killer ? victim.Name : killer.Name;

            RewardLogger.Trace($"Type : {type} Text : {text}");

            // If the amount of renown you have + the renown you've gained is more than required to level...
            if (_Value.Renown + renown > CurrentRenown.Renown)
            {
                // If your renown rank is below twice your career rank, add the renown level as normal.
                if (_Value.RenownRank < (2 * _Value.Level))
                {
                    RenownUp(_Value.Renown + renown - CurrentRenown.Renown);
                    AbtInterface.SendMasteryPointsUpdate();
                }
                // Else, if your RR is equal or greater than twice your career rank, do not let the renown advance enough to level.
                else if (_Value.RenownRank >= (2 * _Value.Level))
                {
                    _Value.Renown = CurrentRenown.Renown - 1;
                    SendRenown(type, text);
                }

            }
            else
            {
                _Value.Renown += renown;
                SendRenown(type, text);

            }
        }

        public void RenownUp(uint remainder)
        {
            // RB   4/17/2016   Lock Renown Rank to Double Career Rank. Cannot gain renown if Renown Rank is higher than 2*Career Rank.
            if (_Value.RenownRank >= Program.Config.RenownCap || (_Value.Level < 32 && _Value.RenownRank >= (2 * _Value.Level)))
                return;

            CurrentRenown = XpRenownService.GetRenown_Info((byte)(_Value.RenownRank + 1));
            if (CurrentRenown == null)
                return;

            _Value.RenownRank += 1;
            _Value.Renown = 0;
            if (remainder > 0)
                InternalAddRenown(remainder, true);
            else SendRenown();

            /*
            CurrentRenown = WorldMgr.GetRenown_Info((byte)(_Value.RenownRank+1));
            if (CurrentRenown == null)
                return;

            SetRenownLevel((byte)(_Value.RenownRank + 1));
            AddRenown(Rest);
             */
            // Reset the bounty score for the player upon gaining an RR
            BountyManagerInstance?.ResetCharacterBounty(CharacterId, this);
        }

        public void AddPendingRenown(uint addAmount)
        {
            //_pendingRenown += addAmount;
        }

        public float GetKillRewardScaler(Player victim)
        {
            int rankDiff = (AdjustedLevel + AdjustedRenown) - (victim.AdjustedLevel + victim.AdjustedRenown);

            return Clamp(-0.0289f * rankDiff + 1.1397f, 0f, 10f);
        }

        #endregion

        #region Influence

        public void AddInfluence(ushort chapter, ushort value)
        {
            _logger.Debug($"Adding influence for {Name} to {value} for chapter {chapter}");
            if (chapter == 0)
                return;

            Chapter_Info info = ChapterService.GetChapterEntry(chapter);
            if (info == null)
            {
                _logger.Debug($"chapter {chapter} not found");
                return;
            }

            if (Info.Influences == null)
                Info.Influences = new List<Characters_influence>();

            Characters_influence infl = null;

            infl = Info.Influences.SingleOrDefault(x => x.InfluenceId == chapter);
            
            //foreach (Characters_influence inf in Info.Influences)
            //    if (inf.InfluenceId == chapter) { infl = inf; break; }

            if (Region != null && Region.Matches((Races)Info.Race))
                value = (ushort)Math.Ceiling(value * RACIAL_INF_FACTOR);

            if (infl == null)
            {
                _logger.Debug($"chapter influence not found - adding");
                infl = new Characters_influence((int)Info.CharacterId, chapter, value);
                Info.Influences.Add(infl);
                CharMgr.Database.AddObject(infl);
            }
            else
            {
                infl.InfluenceCount += value;
                _logger.Debug($"chapter influence found - modifying");
            }

            // If influence > max influence for the chapter.
            if (infl.InfluenceCount > info.Tier3InfluenceCount)
                infl.InfluenceCount = (ushort)info.Tier3InfluenceCount;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INFLUENCE_UPDATE, 12);
            Out.WriteByte(0);
            Out.WriteByte((byte)chapter);
            Out.Fill(0, 4);
            Out.WriteUInt16((ushort)infl.InfluenceCount);          // needs curretn value + mew value
            Out.WriteByte(1);
            Out.Fill(0, 3);
            SendPacket(Out);


        }

        public void SetInfluence(ushort chapter, ushort value)
        {
            _logger.Debug($"Setting influence for {Name} to {value} for chapter {chapter}");
            Chapter_Info info = ChapterService.GetChapterEntry(chapter);
            if (info == null)
            {
                _logger.Debug($"chapter {chapter} not found");
                return;
            }

            if (value > info.Tier3InfluenceCount)
                value = (ushort)info.Tier3InfluenceCount;


            if (Info.Influences == null)
                Info.Influences = new List<Characters_influence>();

            Characters_influence infl = Info.Influences.Find(x => x.InfluenceId == chapter);

            if (infl == null)
            {
                _logger.Debug($"chapter influence not found - adding");
                infl = new Characters_influence((int)Info.CharacterId, chapter, value);
                Info.Influences.Add(infl);
                CharMgr.Database.AddObject(infl);
            }
            else
            {
                _logger.Debug($"chapter influence found - modifying");
                infl.InfluenceCount = value;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_INFLUENCE_UPDATE, 12);
            Out.WriteByte(0);
            Out.WriteByte((byte)chapter);
            Out.Fill(0, 4);
            Out.WriteUInt16(value);
            Out.WriteByte(1);
            Out.Fill(0, 3);
            SendPacket(Out);
        }

        [PacketHandler(PacketHandlerType.TCP, (int)Opcodes.F_INFLUENCE_DETAILS, (int)eClientState.Playing, "F_INFLUENCE_DETAILS")]
        public static void F_INFLUENCE_DETAILS(BaseClient client, PacketIn packet)
        {
            GameClient cclient = client as GameClient;
            byte chapter = (byte)packet.GetUint16();
            cclient.Plr?.SendInfluenceItems(chapter);
        }

        public void SendInfluenceInfo()
        {

            if (Info.Influences == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_INFLUENCE_INFO, 64);
            Out.WriteByte(0);
            Out.WriteByte((byte)Info.Influences.Count);
            Out.WriteByte(0);
            foreach (Characters_influence Obj in Info.Influences)
            {
                Out.WriteByte((byte)Obj.InfluenceId);
                Out.WriteUInt32(Obj.InfluenceCount);
                Out.WriteByte(0);
            }
            Out.WriteByte(0);

            SendPacket(Out);

        }

        public void SendInfluenceItems(byte chapter)
        {
            Chapter_Info info = ChapterService.GetChapterEntry(chapter);
            if (info == null)
                return;

            if (info.CreatureEntry == 0)
                return;

            Characters_influence chapterinf = null;

            if (Info.Influences != null)
                foreach (Characters_influence Obj in Info.Influences)
                {
                    if (Obj.InfluenceId == chapter)
                        chapterinf = Obj;
                }

            List<Chapter_Reward> itemlist = new List<Chapter_Reward>();

            PacketOut Out = new PacketOut((byte)Opcodes.F_INFLUENCE_DETAILS, 128);
            Out.WriteByte(0);
            Out.WriteByte(chapter);

            itemlist = ItmInterface.GetChapterRewards(1, info);

            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16((ushort)info.Tier1InfluenceCount);        //influnce costs
            Out.WriteByte(chapterinf == null ? (byte)1 : chapterinf.Tier_1_Itemtaken ? (byte)2 : (byte)1);
            Out.Fill(0, 6);
            Out.WriteUInt16(0x01F4);        // ??
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteByte((byte)itemlist.Count);      // ammount of items

            foreach (Chapter_Reward chapterReward in itemlist)
            {
                Item.BuildItem(ref Out, null, chapterReward.Item, null, 0, 0);
            }

            itemlist = ItmInterface.GetChapterRewards(2, info);

            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16((ushort)info.Tier2InfluenceCount);        //influnce costs
            Out.WriteByte(chapterinf == null ? (byte)1 : chapterinf.Tier_2_Itemtaken ? (byte)2 : (byte)1);
            Out.Fill(0, 6);
            Out.WriteUInt16(0x01F4);       // ??
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteByte((byte)itemlist.Count);      // ammount of items

            foreach (Chapter_Reward chapterReward in itemlist)
            {
                Item.BuildItem(ref Out, null, chapterReward.Item, null, 0, 0);
            }

            itemlist = ItmInterface.GetChapterRewards(3, info);

            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteUInt16((ushort)info.Tier3InfluenceCount);        //influnce costs
            Out.WriteByte(chapterinf == null ? (byte)1 : chapterinf.Tier_3_Itemtaken ? (byte)2 : (byte)1);
            Out.Fill(0, 6);
            Out.WriteUInt16(0x01F4);   // ??
            Out.WriteByte(1);
            Out.WriteByte(0);
            Out.WriteByte((byte)itemlist.Count);      // ammount of items

            foreach (Chapter_Reward chapterReward in itemlist)
            {
                Item.BuildItem(ref Out, null, chapterReward.Item, null, 0, 0);
            }
            SendPacket(Out);
        }

        #endregion
        #region Money

        public bool HasMoney(uint Money) { return _Value.Money >= Money; }
        public bool RemoveMoney(uint Money)
        {
            if (!HasMoney(Money))
                return false;

            _Value.Money -= Money;
            SendMoney();
            return true;
        }
        public uint GetMoney() { return _Value.Money; }
        public void AddMoney(uint Money)
        {
            _Value.Money += Money;
            SendMoney();
        }

        #endregion

        // Combat

        #region Health/Damage

        /// <summary>
        /// Provides an opportunity for this unit to modify incoming ability damage from enemies.
        /// </summary>
        public override void ModifyDamageIn(AbilityDamageInfo incDamage)
        {
            // no need : base.ModifyDamageIn(incDamage);

            if (WarcampFarmScaler < 1f)
            {
                if (incDamage.Damage > 0) // When invoked after direct damage is computed
                {
                    incDamage.Damage /= WarcampFarmScaler;
                    if (incDamage.Damage > 30000)
                        incDamage.Damage = 30000;
                }
                else // When invoked prior dot damage compute
                    incDamage.DamageBonus /= WarcampFarmScaler;
            }
        }

        /// <summary>
        /// Provides an opportunity for this unit to modify outgoing ability damage it deals.
        /// </summary>
        public override void ModifyDamageOut(AbilityDamageInfo outDamage)
        {
            // no need : base.ModifyDamageOut(outDamage);

            if (WarcampFarmScaler < 1f)
            {
                if (outDamage.Damage > 0) // When invoked after direct damage is computed
                    outDamage.Damage *= WarcampFarmScaler;
                else // When invoked prior dot damage compute
                    outDamage.DamageReduction *= WarcampFarmScaler;
            }
        }

        /// <summary>Scaler applied to damage received or dealed when farming warcamps</summary>
        /// <remarks>The scaler is hidden to player, lower than 1 if debuffed (intended feature)</remarks>
        public volatile float WarcampFarmScaler = 1f;

        /// <summary>
        /// Provides an opportunity for this unit to modify incoming ability damage from enemies.
        /// </summary>
        public override void ModifyHealIn(AbilityDamageInfo incHeal)
        {
            base.ModifyHealIn(incHeal);
        }

        /// <summary>
        /// Provides an opportunity for this unit to modify outgoing ability damage it deals.
        /// </summary>
        public override void ModifyHealOut(AbilityDamageInfo outHeal)
        {
            base.ModifyHealOut(outHeal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="healAmount"></param>
        /// <returns></returns>

        public override int ReceiveHeal(Unit caster, uint healAmount, float healHatredScale = 1.00f)
        {
            int pointsHealed = base.ReceiveHeal(caster, healAmount, healHatredScale);

            Player pCaster = caster as Player;

            if (pCaster != null && pCaster != this && pointsHealed > 0)
            {
                // Handle XP/Renown gain for healing.
                if (_xpPool > 0 && _renownPool > 0)
                {
                    if (pCaster.ScnInterface.Scenario == null || pCaster.ScenarioGroup != null)
                    {
                        int healBaseScale = pointsHealed;

                        // Force solo healers to join scenario groups.
                        if (pCaster.ScenarioGroup != null)
                            healBaseScale = (int)(healBaseScale * (pCaster.ScenarioGroup.TotalMemberCount / 6f));

                        int xAmount = healBaseScale / 5;

                        if (xAmount >= _xpPool)
                            _xpPool = 0;
                        else _xpPool = (uint)(_xpPool - xAmount);

                        pCaster.AddPendingXP((uint)xAmount);

                        int rAmount = healBaseScale / 25;

                        if (rAmount >= _renownPool)
                            _renownPool = 0;
                        else _renownPool -= (uint)rAmount;

                        pCaster.AddPendingRenown((uint)rAmount);
                    }
                }

                // Check PQ contribution.
                QtsInterface.NotifyHealingReceived(pCaster, (uint)pointsHealed);
            }

            SendHealth();

            if (PctHealth > 10 && _isCriticallyWounded)
            {
                _isCriticallyWounded = false;
                SendSpeed();
            }

            // This will generate hate from healing for NPCs
            if (caster != null && caster.IsPlayer() && CbtInterface.IsInCombat && healAmount > 0)
            {
                GetHealAggro(caster.Oid).HealingReceived = (ulong)(((ulong)pointsHealed + (ulong)((healAmount - pointsHealed) * 1.0f)) * (healHatredScale * 0.5f));
                GetHealAggro(caster.Oid).HealingTotal += (ulong)pointsHealed;
                ulong i = GetHealAggro(caster.Oid).HealingReceived;
                GetHealAggro(caster.Oid).HealingReceivedTime = TCPManager.GetTimeStampMS();
            }

            return pointsHealed;
        }

        /// <summary>
        /// We are using aggro list from combat interface to store the details about healing - 
        /// this is needed for hate management
        /// </summary>

        AggroInfo HealAggroInfo;
        public Dictionary<ushort, AggroInfo> HealAggros = new Dictionary<ushort, AggroInfo>();

        public AggroInfo GetHealAggro(ushort oid)
        {
            if (HealAggros.TryGetValue(oid, out HealAggroInfo))
                return HealAggroInfo;

            HealAggroInfo = new AggroInfo(oid);
            HealAggros.Add(HealAggroInfo.Oid, HealAggroInfo);

            return HealAggroInfo;
        }

        public override bool ReceiveDamage(Unit caster, uint damage, float hatredScale = 1f, uint mitigation = 0)
        {
            bool wasKilled = false;
            Player creditedPlayer = null;

            if (IsDead || IsDisposed || IsInvulnerable)
                return false;

            if (caster.Realm != Realm)
                creditedPlayer = caster.CbtInterface.CreditedPlayer;

            lock (DamageApplicationLock)
            {
                if (IsDead)
                    return false;
                if (damage >= _health)
                {
                    // Receiving lethal damage while in the Duel faction causes surrender
                    if (Faction == 64)
                    {
                        SurrenderDuel();
                        return false;
                    }

                    // Lethal damage from members of our realm will be rejected if we're not its caster
                    if (caster.Realm == Realm && this != caster && damage != int.MaxValue)
                        return false;

                    wasKilled = true;

                    damage = _health;
                    Health = 0;

                    if (creditedPlayer != null)
                    {
                        PendingTotalDamageTaken += damage;
                        AddTrackedDamage(creditedPlayer, damage);
                    }

                    TotalDamageTaken = PendingTotalDamageTaken;
                    PendingTotalDamageTaken = 0;

                }
                else
                {
                    _health = Math.Max(0, _health - damage);

                    if (creditedPlayer != null)
                    {
                        PendingTotalDamageTaken += damage;
                        AddTrackedDamage(creditedPlayer, damage);
                    }
                }
            }

            if (caster != this)
            {
                CbtInterface.OnTakeDamage(caster, damage, 1f);
                caster.CbtInterface.OnDealDamage(this, damage);

                if (QtsInterface.PublicQuest != null && caster is PQuestCreature)
                    QtsInterface.NotifyReceivedPQuestMobHit((PQuestCreature)caster, damage);
            }

            LastHitOid = caster.Oid;

            if (wasKilled)
                SetDeath(caster);

            else if (!_isCriticallyWounded && PctHealth < 11)
            {
                _isCriticallyWounded = true;
                SendSpeed();
            }

            return wasKilled;
        }

        public const ushort RespawnTime = 600; // LastUpdatedTime to AutoResurrect in Seconds. 10 Minutes in Official Servers

        private void SendPlayerDeath()
        {
            DeathLogger.Trace($"0x31 {Oid} {RespawnTime}");
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_DEATH, 4);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(RespawnTime);
            SendPacket(Out);
        }

        protected override void SetDeath(Unit killer)
        {
            DeathLogger.Debug($"Victim : {Name} killed by {killer.Name} in {killer.Region?.RegionName}");
            base.SetDeath(killer);

            if (WeaponStance == WeaponStance.Standard)
                PlantStandard();

            SendPlayerDeath();
            deathTime = TCPManager.GetTimeStampMS();

            // RB   5/24/2016   Players get off siege when they die
            if (CurrentSiege != null)
            {
                CurrentSiege.SiegeInterface.RemovePlayer(this);
                CurrentSiege = null;
            }
            DeathLogger.Trace($"Victim : {Name} calling AutomaticRespawnPlayer ");
            EvtInterface.AddEvent(AutomaticRespawnPlayer, RespawnTime * 1000, 1); // If the player don't resurrect. autoresurrect in 10 Minutes.

            BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.OnDie, null, killer);

            CrrInterface.SetResource(0, false);

            if (this == killer)
                return;

            Pet pet = killer as Pet;
            Player playerKiller = (pet != null) ? pet.Owner : killer as Player;

            if (playerKiller != null)
            {
                _Value.RVRDeaths++;

                RecordKillTracking(this, playerKiller, deathTime);

                byte subtype = 0;

                switch (Info.CareerLine)
                {
                    case 1:
                        subtype = 69;
                        break;
                    case 2:
                        subtype = 68;
                        break;
                    case 3:
                        subtype = 70;
                        break;
                    case 4:
                        subtype = 67;
                        break;
                    case 5:
                        subtype = 77;
                        break;
                    case 6:
                        subtype = 78;
                        break;
                    case 7:
                        subtype = 84;
                        break;
                    case 8:
                        subtype = 87;
                        break;
                    case 9:
                        subtype = 98;
                        break;
                    case 10:
                        subtype = 94;
                        break;
                    case 11:
                        subtype = 88;
                        break;
                    case 12:
                        subtype = 97;
                        break;
                    case 13:
                        subtype = 90;
                        break;
                    case 14:
                        subtype = 96;
                        break;
                    case 15:
                        subtype = 99;
                        break;
                    case 16:
                        subtype = 95;
                        break;
                    case 17:
                        subtype = 75;
                        break;
                    case 18:
                        subtype = 74;
                        break;
                    case 19:
                        subtype = 76;
                        break;
                    case 20:
                        subtype = 72;
                        break;
                    case 21:
                        subtype = 61;
                        break;
                    case 22:
                        subtype = 65;
                        break;
                    case 23:
                        subtype = 63;
                        break;
                    case 24:
                        subtype = 64;
                        break;
                }

                if (playerKiller.PriorityGroup != null)
                {
                    DeathLogger.Trace($"Victim : {Name} playerKiller.PriorityGroup Leader : {playerKiller.PriorityGroup?.Leader?.Name} ");
                    foreach (var pg in playerKiller.PriorityGroup.Members)
                    {
                        DeathLogger.Trace($"Victim : {Name} Group Member : {pg.Name} ");
                    }
                }

                if (playerKiller.PriorityGroup != null)
                {
                    List<Player> curMembers = playerKiller.PriorityGroup.GetPlayersCloseTo(playerKiller, 150);
                    if (curMembers != null)
                    {
                        foreach (Player subPlayer in curMembers)
                        {
                            DeathLogger.Trace($"Victim : {Name} curMembers {subPlayer.Name} ");
                            subPlayer.TokInterface.AddKill(subtype);
                        }
                    }

                }
                else
                    playerKiller.TokInterface.AddKill(subtype);
            }

            // Clearing heal aggro...
            HealAggros = new Dictionary<ushort, AggroInfo>();
            // Only do this if not in an SC
            if (ScnInterface.Scenario == null)
            {
                var battleFrontManager = GetBattlefrontManager(Region.RegionId);
                // Reset this characters bounty to their base bounty.
                battleFrontManager.BountyManagerInstance.ResetCharacterBounty(CharacterId, this);
                // Reset the impacts on this character.
                battleFrontManager.ImpactMatrixManagerInstance.ClearImpacts(CharacterId);
            }
            else
            {
                // In a Scenario
                ScenarioMgr.ImpactMatrixManagerInstance.ClearImpacts(CharacterId);
            }

            // inform instance that the player was killed
            if (!string.IsNullOrEmpty(InstanceID))
            {
                WorldMgr.InstanceMgr?.HandlePlayerSetDeath(this, killer);
            }

            ///
            /// Increment the players killed in range value.
            /// 
            if (CurrentKeep != null)
            {
                if (killer is Player)
                {
                    CurrentKeep.PlayersKilledInRange++;
                    CurrentKeep?.OnPlayerKilledInRange(this, killer);
                }
            }
        }

        // For a given regionId, find the correct battlefront manager
        public IBattleFrontManager GetBattlefrontManager(ushort regionId)
        {
            // 31 - Mourkain temple - need a impactmatrix at least for each scen... possibly a BF manager
            //if (regionId == 31)
            //return 

            foreach (var regionMgr in WorldMgr._Regions)
            {
                if (regionMgr.RegionId == regionId)
                {
                    if (regionMgr.GetTier() == 4)
                        return WorldMgr.UpperTierCampaignManager;
                    else
                    {
                        return WorldMgr.LowerTierCampaignManager;
                    }
                }
            }
            DeathLogger.Warn($"Could not locate Battlefront Manager for player {Name} in region {regionId}");
            return WorldMgr.UpperTierCampaignManager;
        }

        private void RecordKillTracking(Player victim, Player killer, long timestamp)
        {
            if (killer == null)
                return;

            if (victim == null)
                return;

            if (killer.GldInterface == null)
                return;

            var tracker = new KillTracker
            {
                Timestamp = timestamp,
                KillerAccountId = killer.Info.AccountId,
                KillerCharacterId = (ushort)killer.Info.CharacterId,
                VictimAccountId = victim.Info.AccountId,
                VictimCharacterId = (int)victim.Info.CharacterId,
                RegionId = victim.Region.RegionId,
                ZoneId = victim.CurrentArea.ZoneId
            };

            if (killer.GldInterface.IsInGuild())
                tracker.KillerGuildId = (int)killer.GldInterface.Guild.Info.GuildId;

            WorldMgr.Database.AddObject(tracker);
        }

        #endregion

        #region Rewards

        internal readonly Dictionary<uint, long> _recentLooters = new Dictionary<uint, long>();
        private int _lastKillerAccountId;
        private int _lastKillerKillCount;

        private const long SOLO_DROP_INTERVAL = 2 * 60 * 1000;

        private int _lastPvPDeathSeconds;

        protected override void HandleDeathRewards(Player killer)
        {
            if (ChickenDebuff != null)
                return;

            if (killer == this)
            {
                if (DamageSources.Count == 0)
                    return;

                killer = DamageSources.Keys.First();
            }

            Scenario currentScenario = killer.ScnInterface.Scenario;

            // Manage player deaths in a scenario
            if (currentScenario != null)
            {
                HandleXPRenown(killer, 1f);

                if (!currentScenario.SoloBlock(killer, false) && !currentScenario.PreventKillReward())
                    GenerateLoot(killer.PriorityGroup != null ? killer.PriorityGroup.GetGroupLooter(killer) : killer, 5f);
            }

            #region RvR Bonus Mod and Rewards

            else
            {

                if (Region.Campaign.PreventKillReward() || (killer.Client?._Account != null && CheckKillFarm(killer)))
                    return;

                // Distribute Player Kill Rewards
                if (ActiveBattleFrontStatus != null)
                {
                    var influenceId = GetKillerInfluenceId(killer);

                    // If the victim was a realm captain, give extra rewards
                    if (ActiveBattleFrontStatus.DestructionRealmCaptain?.CharacterId == CharacterId)
                    {
                        ActiveBattleFrontStatus.RewardManagerInstance.RealmCaptainKill(this, killer, influenceId, PlayersByCharId);
                        ActiveBattleFrontStatus.RemoveAsRealmCaptain(this);
                    }
                    if (ActiveBattleFrontStatus.OrderRealmCaptain?.CharacterId == CharacterId)
                    {
                        ActiveBattleFrontStatus.RewardManagerInstance.RealmCaptainKill(this, killer, influenceId, PlayersByCharId);
                        ActiveBattleFrontStatus.RemoveAsRealmCaptain(this);
                    }

                    ActiveBattleFrontStatus.RewardManagerInstance.DistributePlayerKillRewards(this, killer, AAOBonus, influenceId, PlayersByCharId);

                }

                // Record the recent killers of this toon.
                if (!_recentLooters.ContainsKey(killer.CharacterId))
                    _recentLooters.Add(killer.CharacterId, TCPManager.GetTimeStampMS() + SOLO_DROP_INTERVAL);
                else _recentLooters[killer.CharacterId] = TCPManager.GetTimeStampMS() + SOLO_DROP_INTERVAL;

                killer.Region.Campaign.VictoryPointProgress.AddPlayerKill(killer.Realm);
                killer.SendClientMessage($"+2 VP awarded for assisting your realm secure this campaign.", ChatLogFilters.CHATLOGFILTERS_RVR);
            }

            #endregion
        }


        /// <summary>
        /// Update a character's contribution & bounty
        /// </summary>
        /// <param name="characterId"></param>
        /// <param name="contributionDefinitionId"></param>

        public void UpdatePlayerBountyEvent(byte contributionDefinitionId)
        {
            if (ActiveBattleFrontStatus != null)
            {
                // If player is not in the active zone, stop getting contribution.
                if (ZoneId != ActiveBattleFrontStatus.ZoneId)
                    return;

                // Add contribution for this kill to the killer.
                ActiveBattleFrontStatus.ContributionManagerInstance.UpdateContribution(CharacterId, contributionDefinitionId);

                var definition = new BountyService().GetDefinition(contributionDefinitionId);

                // Add bounty to the death blow killer  
                BountyManagerInstance.AddCharacterBounty(CharacterId, definition.ContributionValue);
                //SendClientMessage($"[Contrib]:+{definition.ContributionValue} {definition.ContributionDescription}");
                RewardLogger.Info($"+++ Update player Bounty character Id : {CharacterId} Contribution Def : {contributionDefinitionId} ({definition.ContributionDescription})");
            }
            else
            {
                RewardLogger.Error($"ActiveBattlefrontStatus is null");
            }
        }

        private ushort GetKillerInfluenceId(Player killer)
        {
            if (killer.CurrentArea != null && killer.CurrentArea.IsRvR)
            {
                return (killer.Realm == Realms.REALMS_REALM_DESTRUCTION) ? (ushort)killer.CurrentArea.DestroInfluenceId : (ushort)killer.CurrentArea.OrderInfluenceId;
            }
            return 0;
        }

        /// <summary>
        /// Grants XP, Renown, Influence, ToK kill incrementation and kill contribution credit to all players inflicting damage.
        /// </summary>
        /// <param name="killer"></param>
        /// <param name="bonusMod"> x >= 1.0f </param>
        /// <param name="activeBattleFrontStatus"></param>
        private void HandleXPRenown(Player killer, float bonusMod, World.Battlefronts.Apocalypse.BattleFrontStatus activeBattleFrontStatus = null)
        {
            List<Player> damageSourceRemovals = new List<Player>();


            #region Initialize reward values

            float deathRewardScaler = 1f;

            if (_lastPvPDeathSeconds > 0)
                deathRewardScaler = Math.Min(1f, (TCPManager.GetTimeStamp() - _lastPvPDeathSeconds) / (ScnInterface.Scenario == null ? 300f : 60f));

            _lastPvPDeathSeconds = TCPManager.GetTimeStamp();

            if (bonusMod == 0)
                bonusMod = 1;

            uint totalXP = (uint)(WorldMgr.GenerateXPCount(killer, this) * bonusMod * (1f + killer.AAOBonus) * deathRewardScaler);
            uint totalRenown = (uint)(WorldMgr.GenerateRenownCount(killer, this) * bonusMod * (1f + killer.AAOBonus) * deathRewardScaler);

            if (Constants.DoomsdaySwitch > 0 && totalRenown < 100)
                totalRenown = 100;

            ushort influenceId = 0;
            uint totalInfluence = 0;

            if (killer.CurrentArea != null && killer.CurrentArea.IsRvR)
            {
                influenceId = (killer.Realm == Realms.REALMS_REALM_DESTRUCTION) ? (ushort)killer.CurrentArea.DestroInfluenceId : (ushort)killer.CurrentArea.OrderInfluenceId;
                totalInfluence = (uint)(100 * bonusMod * (1f + killer.AAOBonus) * deathRewardScaler);
            }

            killer.AddXp(totalXP, 1, false, false);
            //killer.AddRenown(totalRenown, false, RewardType.Kill, $"Killing {Name}");
            killer.AddInfluence(influenceId, (ushort)totalInfluence);
            
            #endregion

            #region Remove players irrelevant to the kill

            // Players too far away from the dead player, or being solo cancer in scenarios, get nothing.
            foreach (Player plr in DamageSources.Keys)
            {
                if (plr.GetDistanceTo(this) > 300 || (plr.ScnInterface.Scenario != null && plr.ScnInterface.Scenario.SoloBlock(plr)))
                {
                    TotalDamageTaken -= DamageSources[plr];
                    damageSourceRemovals.Add(plr);

                    RewardLogger.Debug($"Removed Player {plr.Name}");
                }
            }

            if (damageSourceRemovals.Count > 0)
                foreach (var plr in damageSourceRemovals)
                    DamageSources.Remove(plr);

            if (DamageSources.Count == 0 || TotalDamageTaken == 0)
                return;

            #endregion

            uint sumDamage = 0;
            foreach (KeyValuePair<Player, uint> kvpair in DamageSources)
            {
                sumDamage += kvpair.Value;
            }

            foreach (KeyValuePair<Player, uint> kvpair in DamageSources)
            {
                //#region Get reward values for this player
                Player curPlayer = kvpair.Key;

                // Prevent farming low levels for kill quests, and also stop throttled kills
                if (curPlayer.EffectiveLevel <= EffectiveLevel + 10)
                    curPlayer.QtsInterface.HandleEvent(Objective_Type.QUEST_KILL_PLAYERS, Info.CareerLine, 1);

                curPlayer.EvtInterface.Notify(EventName.OnKill, killer, null);
                curPlayer._Value.RVRKills++;
                curPlayer.SendRVRStats();

                if (curPlayer.PriorityGroup != null)
                {
                    var memberCount = curPlayer.PriorityGroup.Members.Count;
                    foreach (var priorityGroupMember in curPlayer.PriorityGroup.Members)
                    {
                        uint rr = (uint) ((totalRenown * kvpair.Value) / (sumDamage * memberCount));
                        priorityGroupMember.AddRenown(rr, false, RewardType.Kill);
                        RewardLogger.Debug($"{priorityGroupMember.Name} received {rr} for assisting kill on {this.Name}");
                        //priorityGroupMember.SendClientMessage($"{priorityGroupMember.Name} received {rr} for assisting kill on {this.Name}. {totalRenown} * {kvpair.Value} / {sumDamage} * {memberCount}");
                    }
                }
                else
                {
                    uint rr = (uint) ((totalRenown * kvpair.Value) / (sumDamage * 1));
                    curPlayer.AddRenown(rr, false, RewardType.Kill);
                    RewardLogger.Debug($"{curPlayer.Name} received {rr} for solo kill on {this.Name}");
                    //curPlayer.SendClientMessage($"{curPlayer.Name} received {rr} for solo kill on {this.Name}. {totalRenown} * {kvpair.Value} / {sumDamage} * {1}");
                }
            }
        }

        public override void GenerateLoot(Player looter, float dropMod)
        {
            if (ScnInterface.Scenario == null)
            {
                if (looter.WorldGroup == null)
                    looter.AddMoney((uint)(Level * 12 * dropMod));
                else
                    looter.WorldGroup.AddMoney(looter, (uint)(Level * 12 * dropMod));
            }

            lootContainer = LootsMgr.GenerateLoot(this, looter, 1f);

            if (lootContainer == null)
                return;

            looter.PriorityGroup?.GroupLoot(looter, lootContainer);
            lootContainer.TakeAll(looter, true);
            SetLootable(false, looter);
        }

        private bool CheckKillFarm(Player killer)
        {
            int killerAccountId = killer.Client._Account.AccountId;

            if (killer.Client._Account.GmLevel > 1)
                return false;

            if (_lastKillerAccountId == killerAccountId)
            {
                ++_lastKillerKillCount;

                if (_lastKillerKillCount > 4)
                {
                    string message = "Suspicious kill pattern detected: " + killer.Name + " has " + _lastKillerKillCount + " kills against " + Name;

                    PlayerUtil.SendGMBroadcastMessage(_Players, message);
                    
                    return true;
                }
            }

            else
            {
                _lastKillerAccountId = killerAccountId;
                _lastKillerKillCount = 1;
            }

            return false;
        }

        #endregion

        #region Respawning

        public void AutomaticRespawnPlayer()
        {
            RespawnPlayer();
        }

        public void PreRespawnPlayer()
        {
            // Remove automatic respawn function
            EvtInterface.RemoveEvent(AutomaticRespawnPlayer);

            if (!EvtInterface.HasEvent(RespawnPlayer))
            {
                if (ScnInterface.Scenario != null)
                {
                    int respDelay = ScnInterface.Scenario.GetRespawnDelay();
                    SendDialog(Dialog.Respawning, (ushort)respDelay);
                    EvtInterface.AddEvent(RespawnPlayer, respDelay * 1000, 1);
                }

                else
                {
                    SendDialog(Dialog.Respawning, 5);
                    EvtInterface.AddEvent(RespawnPlayer, 5000, 1);
                }
            }
        }

        public void RespawnPlayer()
        {
            EvtInterface.RemoveEvent(AutomaticRespawnPlayer);
            EvtInterface.RemoveEvent(RespawnPlayer);

            // This is Terror debuff - with this you cannot be ressurected
            if (BuffInterface.GetBuff(5968, this) != null)
            {
                BuffInterface.RemoveBuffByEntry(5968);
            }

            if (!IsDead)
                return;

            // Resurrection sickness when manually releasing
            if (Level > 6 && ScnInterface.Scenario == null)
            {
                BuffInfo buffInfo = AbilityMgr.GetBuffInfo((ushort)GameBuffs.BattleFatigue);

                if (CurrentArea != null && CurrentArea.IsRvR)
                    buffInfo.Duration = 180;

                BuffInterface.QueueBuff(new BuffQueueInfo(this, Level, buffInfo));
            }

            var spawnPoint = WorldMgr.GetZoneRespawn(Zone.ZoneId, (byte)Realm, this);
            _logger.Debug($"Respawning player {Name} in Zone {spawnPoint.ToString()}");
            // SendClientMessage($"DEBUG ONLY : Respawning player {this.Name} in Zone {spawnPoint.ToString()}");
            if (spawnPoint != null)
            {

                Teleport((ushort)spawnPoint.ZoneId, (uint)spawnPoint.X, (uint)spawnPoint.Y, (ushort)spawnPoint.Z, Heading);
            }
            else
            {
                _logger.Warn($"Spawnpoint is null for player {Name}, ZoneId {ZoneId}");
            }

            RezUnit();

            //_isResurrecting = false;
        }

        public override void RezUnit()
        {
            RezUnit(null, 0, null, 100, false, null);
        }

        public void RezUnit(ushort abilityEntry, int percentHealth, bool causesSickness)
        {
            RezUnit(null, abilityEntry, null, percentHealth, causesSickness, null);
        }

        public void RezUnit(Point3D worldCoords, ushort abilityEntry, Unit instigator, int percentHealth, bool causesSickness, AbilityDamageInfo damageInfo)
        {
            /*if (!IsDead || _isResurrecting)
                return;

            lock (_resurrectionSync)
            {
                if (!IsDead || _isResurrecting)
                    return;
                _isResurrecting = true;
            }*/

            if (!IsDead)
                return;

            if (instigator == null)
                instigator = this;

            if (worldCoords != null)
                IntraRegionTeleport((uint)worldCoords.X, (uint)worldCoords.Y, (ushort)worldCoords.Z, 0);

            EvtInterface.RemoveEvent(AutomaticRespawnPlayer);
            EvtInterface.RemoveEvent(RespawnPlayer);
            _isCriticallyWounded = false;
            SendSpeed();

            Health = 1;

            SendHit(instigator.Oid);

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_CLEAR_DEATH, 4);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(0);
            DispatchPacket(Out, true);

            States.Remove((byte)CreatureState.Dead); // Death State
            _health = (uint)(TotalHealth * percentHealth * 0.01f);

            if (abilityEntry > 0)
            {
                PacketOut rezHealPacket = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 14);

                rezHealPacket.WriteUInt16(instigator.Oid);
                rezHealPacket.WriteUInt16(Oid);
                rezHealPacket.WriteUInt16(abilityEntry); // 00 00 07 D D

                rezHealPacket.WriteByte(damageInfo?.CastPlayerSubID ?? 3);
                rezHealPacket.WriteByte(0); // DAMAGE EVENT
                rezHealPacket.WriteByte(7); //7    o 42

                rezHealPacket.WriteZigZag((int)Health);
                rezHealPacket.WriteByte(0);

                DispatchPacket(rezHealPacket, true);
            }

            SendHit(instigator.Oid);

            Region.UpdateRange(this, true);

            SendHealth();

            WorldGroup?.NotifyMemberLoaded();
            ScenarioGroup?.NotifyMemberLoaded();

            foreach (Player player in PlayersInRange)
                SendMeTo(player);

            if (causesSickness)
                BuffInterface.QueueBuff(new BuffQueueInfo(this, EffectiveLevel, AbilityMgr.GetBuffInfo((ushort)GameBuffs.ResSickness)));

            // Alter Fate
            if (abilityEntry == 697)
            {
                BuffInterface.QueueBuff(new BuffQueueInfo(this, EffectiveLevel, AbilityMgr.GetBuffInfo(3884)));
                BuffInterface.QueueBuff(new BuffQueueInfo(this, EffectiveLevel, AbilityMgr.GetBuffInfo(3876)));
            }

            BuffInterface.NotifyCombatEvent((byte)BuffCombatEvents.OnResurrect, null, instigator);
            deathTime = 0;



            //_isResurrecting = false;
        }

        #endregion

        #region RvR

        public BattleFrontKeep CurrentKeep { get; set; }
        public Creature CurrentSiege { get; set; }
        public RvRStructure Palisade { get; set; }
        public BattlefieldObjective CurrentObjectiveFlag { get; set; }

        #endregion

        #region Stats

        public void SendStats()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_STATS, 75);
            ItmInterface.BuildStats(ref Out);
            StsInterface.BuildStats(ref Out);
            Out.WriteByte(0);
            SendPacket(Out);
        }

        public override void UpdateSpeed()
        {
            SendSpeed(Speed);
        }

        #endregion

        #region Action Points

        private int _actionPointTimer;
        private long _lastAPCheck = TCPManager.GetTimeStampMS();

        private ushort _actionPoints;

        public ushort ActionPoints
        {
            get { return _actionPoints; }
            set
            {
                if (value > MaxActionPoints)
                    value = MaxActionPoints;

                if (_actionPoints != value)
                {
                    _actionPoints = value;

                    SendHealth();
                }
            }
        }

        public ushort MaxActionPoints { get; set; }


        public byte PctAp => (byte)((ActionPoints * 100) / MaxActionPoints);

        public override bool HasActionPoints(int amount)
        {
            return _actionPoints >= amount;
        }

        /// <summary><para>Adds the amount specified to the player's action points.</para>
        /// <para>Returns the change in action points.</para></summary>
        public override int ModifyActionPoints(int amount)
        {
            int deltaAP;

            if (amount > 0)
            {
                if (_actionPoints == MaxActionPoints)
                    return 0;

                deltaAP = (Math.Min(MaxActionPoints - _actionPoints, amount));
            }
            else if (amount < 0)
            {
                if (_actionPoints == 0)
                    return 0;

                deltaAP = Math.Max(-_actionPoints, amount);
            }
            else
                return 0;

            ActionPoints = (ushort)(_actionPoints + deltaAP);

            return deltaAP;
        }

        /// <summary><para>Removes the amount specified from the player's action points.</para>
        /// <para>Returns false if player doesn't have enough AP.</para></summary>
        public override bool ConsumeActionPoints(ushort amount)
        {
            if (_actionPoints < amount)
                return false;

            ActionPoints -= amount;

            return true;
        }

        public bool Panicked { get; set; }

        public void ChangePanicState(bool panicked)
        {
            Panicked = panicked;

            if (Panicked)
            {
                ActionPoints = 0;
                ResetMorale();
                CrrInterface.NotifyPanicked();
            }
        }

        public void UpdateActionPoints(long tick)
        {
            if (!AbtInterface.IsCasting() && !AbtInterface.IsOnGlobalCooldown())
            {
                _actionPointTimer += (ushort)((tick - _lastAPCheck) * StsInterface.GetStatPercentageModifier(Stats.ActionPointRegen));

                byte count = 0;
                while (_actionPointTimer >= ACTION_REGEN_TIME)
                {
                    _actionPointTimer -= (ushort)ACTION_REGEN_TIME;
                    count++;
                }

                if (count > 0 && ActionPoints < MaxActionPoints)
                {
                    ActionPoints += (ushort)(count * ((25 + StsInterface.GetBonusStat(Stats.ActionPointRegen)) / 2));

                    if (ActionPoints > MaxActionPoints)
                        ActionPoints = MaxActionPoints;
                }
            }
            _lastAPCheck = tick;
        }

        #endregion

        #region Morale

        private const ushort MORALE_REGEN_HOLD = 10000;

        private ushort _morale;

        public long NextMoraleRegen;

        public byte MoraleLevel
        {
            get
            {
                if (_morale < 360)
                    return 0;
                if (_morale < 720)
                    return 1;
                if (_morale < 1800)
                    return 2;
                if (_morale < 3600)
                    return 3;
                return 4;
            }
        }

        public byte PctMorale => (byte)(_morale / 36);

        public void ResetMorale()
        {
            _morale = 0;
            SendHealth();
        }

        public void AddMorale(int toAdd)
        {
            if (_morale == 3600)
                return;

            _morale += (ushort)toAdd;

            if (_morale > 3600)
                _morale = 3600;

            SendHealth();
        }

        public void SetMorale(int newVal)
        {
            _morale = (ushort)Clamp(newVal, 0, 3600);
        }

        public void ConsumeMorale(int toConsume)
        {
            if (_morale == 0)
                return;

            if (toConsume >= _morale)
                _morale = 0;

            else _morale -= (ushort)toConsume;

            SendHealth();
        }


        public void UpdateMorale(long tick)
        {
            if (tick >= NextMoraleRegen)
            {
                NextMoraleRegen = tick + CR_REGEN_TIME;

                if (IsDead)
                {
                    if (_morale > 0)
                    {
                        _morale = 0;
                        SendHealth();
                    }
                }

                else if (!Panicked && tick <= CbtInterface.LastInteractionTime + MORALE_REGEN_HOLD)
                {
                    if (_morale < 3600)
                    {
                        int baseMorale = 10;

                        if (WorldGroup != null && WorldGroup.MemberCount > 1)
                        {
                            baseMorale = 36;
                        }

                        AddMorale(baseMorale + StsInterface.GetTotalStat(Stats.MoraleRegen));
                    }
                }
                else if (_morale > 0)
                    ConsumeMorale(Panicked ? 360 : 100);
            }
        }

        #endregion

        #region CrowdControl

        /// <summary>
        /// Knocks the player backwards and grants the Immovable buff.
        /// </summary>
        /// <param name="angle">The angle of knock, in degrees away from the horizontal.</param>
        /// <param name="power"></param>
        public override void ApplyKnockback(Unit caster, AbilityKnockbackInfo kbInfo)
        {
            if (IsImmovable || NoKnockbacks)
            {
                NotifyImmune(caster, kbInfo.Entry);
                return;
            }

            Point3D puntedWorldPos;

            Player player = caster as Player;
            if (player != null)
                puntedWorldPos = GetHistoricalPosition(TCPManager.GetTimeStampMS() - player.Latency);

            else
                puntedWorldPos = WorldPosition;

            PacketOut Out = new PacketOut((byte)Opcodes.F_KNOCKBACK, 64);

            Out.WriteUInt32((uint)puntedWorldPos.X);   // DefenderWorldPosX (defender will be moved to this pos when kb starts)
            Out.WriteUInt32((uint)puntedWorldPos.Y);   // DefenderWorldPosY
            Out.WriteUInt32((uint)caster.WorldPosition.X); // AttackerWorldPosX (acts as source of knockback)
            Out.WriteUInt32((uint)caster.WorldPosition.Y);  // AttackerWorldPosY
            Out.Fill(0, 10);
            Out.WriteUInt16(kbInfo.Power);  // KickForce
            Out.Fill(0, 3);
            Out.WriteByte(kbInfo.Angle);
            Out.Fill(0, 10);
            Out.WriteUInt16(kbInfo.RangeExtension);  // KickForce2
            Out.Fill(0, 3);
            Out.WriteByte(kbInfo.GravMultiplier); // THIS MATTERS.
            Out.WriteUInt16((ushort)caster.WorldPosition.Z);
            Out.WriteUInt16((ushort)puntedWorldPos.Z);
            Out.WriteByte(kbInfo.Unk); // If other one is 1, this will usually be 1
            Out.WriteByte(0);
            Out.WriteUInt16(Oid);
            Out.Fill(0, 4);

            lock (MovementCCLock)
            {
                if (IsImmovable || NoKnockbacks)
                {
                    NotifyImmune(caster, kbInfo.Entry);
                    return;
                }

                IsImmovable = true;
            }

            DispatchPacket(Out, true);
            PuntedBy = caster;
            FallGuard = true;
            IsMoving = true;
            _knockbackTime = DateTime.Now;

            // Immovable
            BuffInterface.QueueBuff(new BuffQueueInfo(this, EffectiveLevel, AbilityMgr.GetBuffInfo(408)));

            AbtInterface.Cancel(true);
        }

        public void PulledBy(Unit caster, ushort val1, ushort val2, bool grantsImmunity = true, bool fivefeetoverride = false)
        {
            if (IsImmovable || NoKnockbacks)
                return;


            float pullOffsetFactor;

            // Offset the pull by a random factor of between 5 and 15 feet of the caster's position.
            if (fivefeetoverride == false)
                pullOffsetFactor = Math.Min(1f, (10f + StaticRandom.Instance.Next(10)) / GetDistanceToObject(caster));
            //if the 5 feet override is true then set it to 5 feet instead.
            else
                pullOffsetFactor = 1f / GetDistanceToObject(caster);

            if (Zone.ZoneId != caster.Zone.ZoneId)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CATAPULT, 30);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(Zone.ZoneId);
                Out.WriteUInt16((ushort)X); // Start X
                Out.WriteUInt16((ushort)Y); // Start Y
                Out.WriteUInt16((ushort)Z); // Start Z
                Out.WriteUInt16(caster.Zone.ZoneId);
                Out.WriteUInt16((ushort)(caster.X)); // Start X
                Out.WriteUInt16((ushort)(caster.Y)); // Start Y
                Out.WriteUInt16((ushort)(caster.Z + 60)); // Start Z
                Out.WriteUInt16(val1);
                Out.WriteByte((byte)val2);
                Out.Fill(0, 19);

                DispatchPacket(Out, true);
            }

            if (Zone.ZoneId == caster.Zone.ZoneId)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_CATAPULT, 30);
                Out.WriteUInt16(Oid);
                Out.WriteUInt16(Zone.ZoneId);
                Out.WriteUInt16((ushort)X); // Start X
                Out.WriteUInt16((ushort)Y); // Start Y
                Out.WriteUInt16((ushort)Z); // Start Z
                Out.WriteUInt16(caster.Zone.ZoneId);
                Out.WriteUInt16((ushort)(caster.X + (X - caster.X) * pullOffsetFactor)); // Start X
                Out.WriteUInt16((ushort)(caster.Y + (Y - caster.Y) * pullOffsetFactor)); // Start Y
                Out.WriteUInt16((ushort)(caster.Z + 60 + (Z - caster.Z) * pullOffsetFactor)); // Start Z
                Out.WriteUInt16(val1);
                Out.WriteByte((byte)val2);
                Out.Fill(0, 19);

                DispatchPacket(Out, true);
            }

            lock (MovementCCLock)
            {
                if (IsImmovable || NoKnockbacks)
                    return;

                if (grantsImmunity)
                    IsImmovable = true;
            }


            FallGuard = true;
            IsMoving = true;
            PuntedBy = caster;
            // Immovable
            if (grantsImmunity)
                BuffInterface.QueueBuff(new BuffQueueInfo(this, EffectiveLevel, AbilityMgr.GetBuffInfo(408)));

            AbtInterface.Cancel(true);
        }

        public void Catapult(ZoneMgr targetZone, Point3D target, ushort flightTime, ushort arcHeight)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CATAPULT, 30);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(Zone.ZoneId);
            Out.WriteUInt16((ushort)X); // Start X
            Out.WriteUInt16((ushort)Y); // Start Y
            Out.WriteUInt16((ushort)Z); // Start Z
            Out.WriteUInt16(targetZone.ZoneId);
            Out.WriteUInt16((ushort)(target.X - (targetZone.Info.OffX << 12)));
            Out.WriteUInt16((ushort)(target.Y - (targetZone.Info.OffY << 12)));
            Out.WriteUInt16((ushort)(target.Z + 100)); // Start Z
            Out.WriteUInt16(arcHeight);
            Out.WriteByte((byte)flightTime);
            Out.Fill(0, 19);


            DispatchPacket(Out, true);

            AbtInterface.Cancel(true);
        }

        private long _nextWindsHit;

        /// <summary>
        /// Knocks the player backwards and grants the Immovable buff.
        /// </summary>
        public void ApplyWindsKnockback(Unit caster, AbilityKnockbackInfo kbInfo)
        {
            lock (MovementCCLock)
            {
                if (_nextWindsHit > TCPManager.GetTimeStampMS() || IsImmovable || NoKnockbacks)
                {
                    NotifyImmune(caster, kbInfo.Entry);
                    return;
                }

                _nextWindsHit = TCPManager.GetTimeStampMS() + 1500;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_KNOCKBACK, 64);

            Out.WriteUInt32((uint)WorldPosition.X);   // DefenderWorldPosX (defender will be moved to this pos when kb starts)
            Out.WriteUInt32((uint)WorldPosition.Y);   // DefenderWorldPosY
            Out.WriteUInt32((uint)caster.WorldPosition.X); // AttackerWorldPosX (acts as source of knockback)
            Out.WriteUInt32((uint)caster.WorldPosition.Y);  // AttackerWorldPosY
            Out.Fill(0, 10);
            Out.WriteUInt16(kbInfo.Power);  // KickForce
            Out.Fill(0, 3);
            Out.WriteByte(kbInfo.Angle);
            Out.Fill(0, 10);
            Out.WriteUInt16(kbInfo.RangeExtension);  // KickForce2
            Out.Fill(0, 3);
            Out.WriteByte(kbInfo.GravMultiplier); // THIS MATTERS.
            Out.WriteUInt16((ushort)caster.WorldPosition.Z);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteByte(kbInfo.Unk); // If other one is 1, this will also be 1
            Out.WriteByte(0);
            Out.WriteUInt16(Oid);
            Out.Fill(0, 4);

            lock (MovementCCLock)
            {
                if (IsImmovable || NoKnockbacks)
                {
                    NotifyImmune(caster, kbInfo.Entry);
                    return;
                }
            }

            DispatchPacket(Out, true);
            PuntedBy = caster;
            FallGuard = true;
            IsMoving = true;
            _knockbackTime = DateTime.Now;

            AbtInterface.Cancel(true);
        }
        public void ApplyKnockback(Point3D point, ushort power, byte angle, ushort ext, byte gravity, byte unk)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_KNOCKBACK, 64);

            Out.WriteUInt32((uint)WorldPosition.X);   // DefenderWorldPosX (defender will be moved to this pos when kb starts)
            Out.WriteUInt32((uint)WorldPosition.Y);   // DefenderWorldPosY
            Out.WriteUInt32((uint)point.X); // AttackerWorldPosX (acts as source of knockback)
            Out.WriteUInt32((uint)point.Y);  // AttackerWorldPosY
            Out.Fill(0, 10);
            Out.WriteUInt16(power);  // KickForce
            Out.Fill(0, 3);
            Out.WriteByte(angle);
            Out.Fill(0, 10);
            Out.WriteUInt16(ext);  // KickForce2
            Out.Fill(0, 3);
            Out.WriteByte(gravity); // THIS MATTERS.
            Out.WriteUInt16((ushort)point.Z);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteByte(unk); // If other one is 1, this will usually be 1
            Out.WriteByte(0);
            Out.WriteUInt16(Oid);
            Out.Fill(0, 4);

            _knockbackTime = DateTime.Now;

            DispatchPacket(Out, true);
        }

        /// <summary>
        /// Knocks the player backwards, but will not apply Immovable.
        /// </summary>
        public void ApplySelfKnockback(AbilityKnockbackInfo kbInfo)
        {
            if (NoKnockbacks)
                return;

            Point2D kbOffset = GetOffsetFromHeading(Heading, 10);

            PacketOut Out = new PacketOut((byte)Opcodes.F_KNOCKBACK, 64);

            Out.WriteUInt32((uint)WorldPosition.X);   // DefenderWorldPosX (defender will be moved to this pos when kb starts)
            Out.WriteUInt32((uint)WorldPosition.Y);   // DefenderWorldPosY

            // Convert pin to world, if points would lie on same map
            if (kbOffset.X > X && kbOffset.Y > Y)
            {
                int x = (X + kbOffset.X) > 32768 ? (X + kbOffset.X) - 32768 : (X + kbOffset.X);
                int y = (Y + kbOffset.Y) > 32768 ? (Y + kbOffset.Y) - 32768 : (Y + kbOffset.Y);

                kbOffset.X = (XOffset << 12) + (x & 0x00000FFF);
                kbOffset.Y = (YOffset << 12) + (y & 0x00000FFF);
                WorldPosition.Z = Z;

                Out.WriteUInt32((uint)(kbOffset.X));
                Out.WriteUInt32((uint)(kbOffset.Y));
            }
            else
            {
                Out.WriteUInt32((uint)(WorldPosition.X + kbOffset.X)); // AttackerWorldPosX (acts as source of knockback)
                Out.WriteUInt32((uint)(WorldPosition.Y + kbOffset.Y));  // AttackerWorldPosY
            }
            Out.Fill(0, 10);
            Out.WriteUInt16(kbInfo.Power);
            Out.Fill(0, 3);
            Out.WriteByte(kbInfo.Angle);
            Out.Fill(0, 10);
            Out.WriteUInt16(kbInfo.RangeExtension);  // KickForce2
            Out.Fill(0, 3);
            Out.WriteByte(kbInfo.GravMultiplier); // THIS MATTERS.
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteUInt16((ushort)WorldPosition.Z);
            Out.WriteByte(kbInfo.Unk); // If other one is 1, this will also be 1
            Out.WriteByte(0);
            Out.WriteUInt16(Oid);
            Out.Fill(0, 4);

            DispatchPacket(Out, true);

            IsMoving = true;
            _knockbackTime = DateTime.Now;
        }

        #endregion

        #region Stealth

        private long _lastStealthCheck;
        private const ushort STEALTH_CHECK_INTERVAL = 1000;
        private const ushort STEALTH_DETECT_RANGE_MAX = 40;

        public byte StealthLevel { get; private set; }

        private HashSet<Player> _spotters = new HashSet<Player>();

        public void Cloak(byte stealthLevel)
        {
            StealthLevel = stealthLevel;

            _lastStealthCheck = TCPManager.GetTimeStampMS();

            foreach (Player Plr in PlayersInRange)
            {
                if (Plr.Realm != Realm || StealthLevel == 2)
                    SendRemove(Plr);
            }
        }

        private void CheckStealth()
        {
            ushort myInitiative = (ushort)StsInterface.GetTotalStat(Stats.Initiative);

            foreach (Player plr in PlayersInRange)
            {
                // Only GMs can see players in GM stealth
                if (StealthLevel == 2)
                {
                    if (_spotters.Contains(plr))
                        continue;
                    if (plr.GmLevel > 1)
                    {
                        _spotters.Add(plr);
                        SendMeTo(plr);
                    }
                    continue;
                }

                if (plr.Realm == Realm)
                    continue;

                // See all while in GM stealth...
                if (plr.StealthLevel == 2)
                {
                    if (!_spotters.Contains(plr))
                    {
                        _spotters.Add(plr);
                        SendMeTo(plr);
                    }

                    continue;
                }

                // More difficult for a target to spot a stealther if the approach is from the side or back
                float spotDistMod = plr.IsObjectInFront(this, 90) ? 1f : (plr.IsObjectInFront(this, 240) ? 0.4f : 0.2f);
                // If stationary, much more difficult to spot
                if (!IsMoving)
                    spotDistMod *= 0.2f;

                if (_spotters.Contains(plr))
                {
                    if (!IsWithinRadiusFeet(plr, (int)(STEALTH_DETECT_RANGE_MAX * spotDistMod)))
                    {
                        //Plr.Say("Lost track of " + _Info.Name, SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
                        _spotters.Remove(plr);
                        SendRemove(plr);
                    }
                }

                else if (IsWithinRadiusFeet(plr, (int)(STEALTH_DETECT_RANGE_MAX * spotDistMod)))
                {
                    int dist = GetDistanceToObject(plr);
                    float detectionBonusFactor = (float)plr.StsInterface.GetTotalStat(Stats.Initiative) / myInitiative;

                    int checkDifficulty = (int)((5 + (STEALTH_DETECT_RANGE_MAX - dist)) * detectionBonusFactor * spotDistMod);
                    int checkRoll = StaticRandom.Instance.Next(100);
                    //Plr.Say("Detection: dist:"+dist+", check difficulty " + checkDifficulty + "%, rolled " + checkRoll + ".", SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
                    if (checkRoll < checkDifficulty)
                    {
                        //Plr.Say("Detected " + _Info.Name +".", SystemData.ChatLogFilters.CHATLOGFILTERS_SAY);
                        _spotters.Add(plr);
                        SendMeTo(plr);
                    }
                }
            }
        }

        public void Uncloak()
        {
            StealthLevel = 0;

            foreach (Player Plr in PlayersInRange)
            {
                if (!_spotters.Contains(Plr))
                    SendMeTo(Plr);
            }

            _spotters.Clear();
        }

        #endregion

        #region Bolster

        private byte _bolsterTier;

        /// <summary>
        /// Returns true if the player is either native to the given tier or debolstering to it.
        /// </summary>
        public bool ValidInTier(int tier, bool checkDebolster)
        {
            if (tier < 1 || tier > 4)
                return true;

            var validInTier = (Level >= Constants.MinTierLevel[tier - 1] && Level <= Constants.MaxTierLevel[tier - 1]) || (checkDebolster && (_bolsterTier == tier || (_bolsterTier == 2 && tier == 3) || (_bolsterTier == 3 && tier == 2)));
            // _logger.Trace($"Player : {this.Name} validity in tier : {validInTier}");
            return validInTier;
        }

        public bool ShouldDebolster(int tier)
        {
            if (tier == 4 || tier == 0)
                return false;

            return Level > Constants.MaxTierLevel[tier - 1];
        }

        public bool ShouldChicken(int tier, bool sendMessage)
        {
            if (GmLevel > 1 && BroadcastRank)
                return false;

            // Can't chicken within scenarios atm
            if (ScnInterface.Scenario != null)
                return false;

            if (tier == 0 || tier > 3 || Level <= Constants.MaxTierLevel[tier - 1])
                return false;

            switch (tier)
            {
                case 1:
                    // This disables debolster for now
                    if (Constants.DisableDebolster)
                    {
                        SendClientMessage("Debolster is currently disabled. Happy playing in T2-T4!");
                        return true;
                    }

                    if (AbtInterface.GetTotalSpent() > 2)
                    {
                        if (sendMessage)
                            SendClientMessage("You have more than 2 career mastery points allocated, which makes you too powerful for this area.");
                        return true;
                    }
                    if (RenInterface.PointsSpent > 13)
                    {
                        if (sendMessage)
                            SendClientMessage("You have more than 9 renown mastery points allocated, which makes you too powerful for this area.");
                        return true;
                    }
                    if (ItmInterface.EquippedGearAbove(13))
                    {
                        if (sendMessage)
                            SendClientMessage("You have equipped gear with a rank requirement higher than 13, which makes you too powerful for this area.");
                        return true;
                    }
                    if (ItmInterface.HasTalismansInGear())
                    {
                        if (sendMessage)
                            SendClientMessage("You have equipped gear with talismans, which makes you too powerful for this area.");
                        return true;
                    }
                    return false;
                case 2:
                case 3:
                    if (AbtInterface.GetTotalSpent() > 13)
                    {
                        if (sendMessage)
                            SendClientMessage("You have more than 13 career mastery points allocated, which makes you too powerful for this area.");
                        return true;
                    }
                    if (RenInterface.PointsSpent > 28)
                    {
                        if (sendMessage)
                            SendClientMessage("You have more than 28 renown mastery points allocated, which makes you too powerful for this area.");
                        return true;
                    }
                    if (ItmInterface.EquippedGearAbove(28))
                    {
                        if (sendMessage)
                            SendClientMessage("You have equipped gear with a rank requirement higher than 28, which makes you too powerful for this area.");
                        return true;
                    }
                    return false;
            }

            return false;
        }

        public bool ShouldBolster(int tier, bool isScenario, Zone_Area newArea)
        {
            if (tier == 0 || tier > 4)
                return false;

            if (newArea == null || newArea != null && !newArea.IsRvR)
                return false;

            /*if (isScenario)
            {
                switch (tier)
                {
                    case 1:
                        return Level < 15;
                    case 2:
                    case 3:
                        return Level > 15 && Level < 30;
                    case 4:
                        return Level > 30;
                }
            }
            return Level < (tier * 10) + 1 && Level < 40;*/

            if (ShouldChicken(tier, false))
                return false;

            return Level >= Constants.MinTierLevel[tier - 1] && Level != Constants.MaxTierLevel[tier - 1];
        }

        public void TryBolster(int curTier, Zone_Area newArea)
        {
            if (curTier == 0)
                curTier = ScnInterface.Scenario?.Tier ?? Zone.Info.Tier;

            if (ShouldBolster(curTier, ScnInterface.Scenario != null, newArea))
            {
                if (_bolsterTier > 0)
                    RemoveBolster();

                _bolsterTier = (byte)curTier;
                BuffInterface.QueueBuff(new BuffQueueInfo(this, 1, AbilityMgr.GetBuffInfo((ushort)(GameBuffs.BolsterBase + curTier))));
            }
        }

        /// <summary>
        /// Induces either chicken or debolster depending on the target's situation.
        /// </summary>
        public void TryDebolster(int tier)
        {
            if (ShouldChicken(tier, true))
            {
                RemoveBolster();
                BuffInterface.QueueBuff(new BuffQueueInfo(this, Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Chicken), AssignChickenBuff));
            }
        }

        public bool CheckDebolsterValid()
        {
            if (ShouldChicken(_bolsterTier, true))
            {
                RemoveBolster();
                BuffInterface.QueueBuff(new BuffQueueInfo(this, Level, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Chicken), AssignChickenBuff));
                return false;
            }

            return true;
        }

        public void RemoveBolster()
        {
            if (Level < 40)
            {
                //because auras become stupid OP if left in a bolstered state, we remove them on removal of bolster
                ushort[] auras = new ushort[] { 8004, 8022, 8030, 8033, 8006, 8036, 8008, 8020, 8342, 8334, 8316, 8345, 8332, 8318, 8321, 8327, 8348 };
                foreach (ushort element in auras)
                {
                    if (BuffInterface.GetBuff(element, this) != null)
                        BuffInterface.RemoveBuffByEntry(element);
                }
            }

            if (Constants.DoomsdaySwitch > 0)
            {
                BuffInterface.RemoveBuffByEntry((ushort)(14312));
                BuffInterface.RemoveBuffByEntry((ushort)(14313));
                BuffInterface.RemoveBuffByEntry((ushort)(14314));
                BuffInterface.RemoveBuffByEntry((ushort)(14315));
                BuffInterface.RemoveBuffByEntry((ushort)(14316));
            }
            else
            {
                BuffInterface.RemoveBuffByEntry((ushort)(14312 + _bolsterTier));
            }

            StsInterface.BolsterFactor = 1f;
            ItmInterface.BolsterFactor = 0f;
        }

        public void ApplyBolster(byte newMaxLevel)
        {
            if (newMaxLevel > 0)
            {
                // Because of the delay between queuing and application of the buff, the player may have attempted to change his gear or spec
                // If he does he'll eat a chicken instead
                if (!CheckDebolsterValid())
                    return;

                /*if (ScnInterface.Scenario != null)
                {
                    switch (_bolsterTier)
                    {
                        case 1:
                            if (Level > 8)
                                newMaxLevel = 11;
                            else if (Level > 5)
                                newMaxLevel = 12;
                            else
                                newMaxLevel = 13;
                            break;
                        case 2:
                            if (Level > 18)
                                newMaxLevel = 21;
                            else if (Level > 15)
                                newMaxLevel = 22;
                            else
                                newMaxLevel = 23;
                            break;
                        case 3:
                            if (Level > 28)
                                newMaxLevel = 31;
                            else if (Level > 25)
                                newMaxLevel = 32;
                            else
                                newMaxLevel = 33;
                            break;
                    }
                }
                else */

                switch (_bolsterTier)
                {
                    case 1:
                        if (Level < 15)
                            newMaxLevel = 16;
                        break;
                        /*
                    case 2:
                    case 3:
                        if (Level > 27)
                            newMaxLevel = 30;
                        else if (Level > 22)
                            newMaxLevel = 33;
                        else
                            newMaxLevel = 36;
                        break;
                        */
                }

                if (Constants.DoomsdaySwitch > 0 && Level > 15 && ScnInterface.Scenario == null)
                {
                    if (Level == 40)
                        return;

                    World_Settings Settings = WorldMgr.Database.SelectObject<World_Settings>("SettingId = 1");

                    if (Settings != null)
                        newMaxLevel = (byte)Settings.Setting;

                    if (Level > 30)
                        newMaxLevel = 40;

                    if (newMaxLevel <= Level)
                        return;

                }

            }

            // Debolster lifted
            else if (AdjustedLevel < Level)
                _adjustedRenown = 0;

            List<CharacterInfo_stats> newStats = CharMgr.GetCharacterInfoStats(Info.CareerLine, newMaxLevel == 0 ? Level : newMaxLevel);

            foreach (CharacterInfo_stats stat in newStats)
                StsInterface.SetBaseStat((Stats)stat.StatId, stat.StatValue);

            StsInterface.BolsterLevel = newMaxLevel;

            // Applying bolster
            if (newMaxLevel > Level)
            {
                StsInterface.BolsterFactor = (newMaxLevel == 0 ? 1f : (newMaxLevel + 1) / (float)(Level + 1));
                ItmInterface.BolsterFactor = (newMaxLevel == 0 ? 0f : (newMaxLevel + 1) / (float)(Level + 1) - 1f);
                SendClientMessage($"Your Battle Rank has been increased to {newMaxLevel}!\nThis effect will persist until you leave the RvR area.");
            }

            // Applying debolster
            else if (newMaxLevel > 0 && newMaxLevel < Level) // Debolster, need to reload tactics
            {
                BuffInterface.RemoveBuffsAboveLevel(newMaxLevel);

                Pet pet = CrrInterface.GetTargetOfInterest() as Pet;
                pet?.Dismiss(null, null);

                _adjustedRenown = Math.Min(RenownRank, (byte)(_bolsterTier * 10));

                SendClientMessage($"Your Battle Rank has been reduced to {newMaxLevel}!\nThis effect will persist until you leave the RvR area or violate the conditions for debolstering.");
            }

            else
            {
                StsInterface.BolsterFactor = (newMaxLevel == 0 ? 1f : (newMaxLevel + 1) / (float)(Level + 1));
                ItmInterface.BolsterFactor = (newMaxLevel == 0 ? 0f : (newMaxLevel + 1) / (float)(Level + 1) - 1f);
                SendClientMessage($"Your Battle Rank of {Level} has been restored.");
                Pet pet = CrrInterface.GetTargetOfInterest() as Pet;
                pet?.Dismiss(null, null);
            }

            if (!PendingDisposal)
            {
                StsInterface.ApplyStats();
                TacInterface.ReloadTactics();
            }
        }

        #endregion

        #region Dueling

        /*
        TEXT_DUEL_ERROR_NOT_CHALLENGED,	//You are not currently considering a duel.
        TEXT_DUEL_YOU_DECLINE,	//You decline the duel.
        TEXT_DUEL_X_DECLINES,	//<<1>> declines your duel.
        TEXT_DUEL_OFFER_CANCELLED,	//The offer to duel has been cancelled.
        TEXT_DUEL_YOU_ACCEPT,	//You have accepted the duel!  Begin fighting in 5 seconds!
        TEXT_DUEL_X_ACCEPTS,	//<<1>> has accepted your duel!  Begin fighting in 5 seconds!
        TEXT_DUEL_YOU_SURRENDER,	//You surrender your duel.
        TEXT_DUEL_X_SURRENDERS,	//<<1>> surrenders to you in a duel.
        TEXT_DUEL_ERROR_YOU_ALREADY_CHALLENGED,	//You have already issued a challenge for a duel. /duel cancel to remove it.
        TEXT_DUEL_ERROR_ALREADY_DUELING,	//You are already in a duel.  /duel surrender to end it.
        TEXT_DUEL_ERROR_ALREADY_BEEN_CHALLENGED,	//You have already been challanged to a duel. /duel decline to decline it.
        TEXT_DUEL_ERROR_FULL_HEALTH,	//You must be at full health to enter a duel.
        TEXT_DUEL_ERROR_TARGET_FULL_HEALTH,	//Your target must be at full health to enter a duel.
        TEXT_DUEL_ERROR_IN_COMBAT,	//You cannot enter a duel after engaging in combat recently.
        TEXT_DUEL_ERROR_RVR_ZONE,	//This is an RvR area.  You cannot duel here.
        TEXT_DUEL_ERROR_NO_TARGET,	//You need to target a player to duel.
        TEXT_DUEL_ERROR_TARGET_NOT_FRIENDLY,	//You must target an ally to duel.
        TEXT_DUEL_ERROR_TARGET_IN_COMBAT,	//Your target must disengage from combat before dueling.
        TEXT_DUEL_ERROR_TARGET_ALREADY_CHALLENGED,	//Your target has already issued a duel challenge.
        TEXT_DUEL_ERROR_TARGET_ALREADY_IN_DUEL,	//Your target is already in a duel.
        TEXT_DUEL_ERROR_TARGET_ALREADY_BEEN_CHALLENGED,	//Your target has already been issued a duel challange.
        TEXT_YOU_CHALLENGE_X_TO_DUEL,	//You have challenged <<1>> to a duel.
        TEXT_X_CHALLENGES_YOU_TO_DUEL,	//<<1>> challenges you to a duel.
        TEXT_DUEL_ERROR_TARGET_IN_RVR_ZONE,	//Your target is in an RvR area.  You cannot duel there.
         */

        private Player Aggressor;
        private Player Defender;

        private byte _oldFaction;

        public void SetDuelFaction(bool bDuelling)
        {
            if (bDuelling)
            {
                if (Faction != 64)
                {
                    _oldFaction = Faction;
                    Faction = 64;
                }

                _xpPool = 0;
                _renownPool = 0;
            }
            else if (Faction == 64)
                Faction = _oldFaction;

            if (IsActive && IsInWorld() && Loaded)
            {
                foreach (Player Plr in PlayersInRange)
                {
                    if (Plr.HasInRange(this))
                    {
                        SendRemove(Plr);
                        SendMeTo(Plr);
                    }
                }
            }
        }

        public void OfferDuel()
        {
            if (Defender != null)
            {
                SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_DUEL_ERROR_YOU_ALREADY_CHALLENGED);
                return;
            }

            if (Aggressor != null)
            {
                SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_DUEL_ERROR_ALREADY_BEEN_CHALLENGED);
                return;
            }

            if (CurrentArea != null && CurrentArea.IsRvR)
            {
                SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_DUEL_ERROR_RVR_ZONE);
                return;
            }

            if (!CbtInterface.IsPvp)
            {
                SendClientMessage("Must be flagged to duel.");
                return;
            }

            Defender = CbtInterface.GetCurrentTarget() as Player;

            if (Defender == null)
            {
                SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_DUEL_ERROR_NO_TARGET);
                return;
            }

            if (Defender.CurrentArea != null && Defender.CurrentArea.IsRvR)
            {
                SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_DUEL_ERROR_TARGET_IN_RVR_ZONE);
                return;
            }

            Defender.Aggressor = this;

            Defender.SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_X_CHALLENGES_YOU_TO_DUEL);
            SendLocalizeString(Defender.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_YOU_CHALLENGE_X_TO_DUEL);
        }

        public void RespondToDuel(bool bAccept)
        {
            if (Aggressor == null)
            {
                SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_ERROR_NOT_CHALLENGED);
                return;
            }

            if (!bAccept)
            {
                Aggressor.SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_X_DECLINES);
                SendLocalizeString(Aggressor.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_YOU_DECLINE);
                Aggressor.Defender = null;
                Aggressor = null;
            }
            else
            {
                if (CurrentArea != null && CurrentArea.IsRvR)
                {
                    SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_ERROR_RVR_ZONE);
                    return;
                }
                if (Aggressor.CurrentArea != null && Aggressor.CurrentArea.IsRvR)
                {
                    SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_ERROR_TARGET_IN_RVR_ZONE);
                    return;
                }
                if (CbtInterface.IsInCombat || Aggressor.CbtInterface.IsInCombat)
                {
                    SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_ERROR_IN_COMBAT);
                    return;
                }

                if (!CbtInterface.IsPvp)
                {
                    SendClientMessage("Must be flagged to accept a duel.");
                    return;
                }

                SetDuelFaction(true);
                Aggressor.SetDuelFaction(true);
                Aggressor.SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_X_ACCEPTS);
                SendLocalizeString(Aggressor.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_YOU_ACCEPT);
            }
        }

        public void RescindDuelOffer()
        {
            if (Defender != null)
            {
                Defender.SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_OFFER_CANCELLED);
                SendLocalizeString(Defender.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_OFFER_CANCELLED);
                Defender = null;
            }
        }

        public void SurrenderDuel()
        {
            if (Faction != 64)
                return;

            if (Aggressor != null)
            {
                SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_YOU_SURRENDER);
                Aggressor.SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_X_SURRENDERS);

                Aggressor.SetDuelFaction(false);

                Aggressor.Defender = null;
                Aggressor = null;
            }

            else if (Defender != null)
            {
                SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_YOU_SURRENDER);
                Defender.SendLocalizeString(Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_DUEL_X_SURRENDERS);

                Defender.SetDuelFaction(false);

                Defender.Aggressor = null;
                Defender = null;
            }

            SetDuelFaction(false);
        }

        #endregion

        #region RvR Flagging

        private bool _isRvRCountdown;
        public NewBuff ChickenDebuff { get; set; }

        public void SetRvRCountdown(bool toggle)
        {
            if (_isRvRCountdown == toggle)
                return;

            _isRvRCountdown = toggle;

            if (_isRvRCountdown)
            {
                EvtInterface.AddEvent(RvRCountdownEnd, 10000, 1);
                if (ShouldChicken(Zone.Info.Tier, true))
                    DispatchUpdateState((byte)StateOpcode.RvRFlag, 1, 1);
                else DispatchUpdateState((byte)StateOpcode.RvRFlag, 1);
            }

            else
            {
                EvtInterface.RemoveEvent(RvRCountdownEnd);
                DispatchUpdateState((byte)StateOpcode.RvRFlag, 0); // RvR cancel
            }
        }

        public void RvRCountdownEnd()
        {
            if (!CbtInterface.IsPvp)
                ((CombatInterface_Player)CbtInterface).EnablePvp();

            _isRvRCountdown = false;
        }

        public void SetPVPFlag(bool state)
        {
            if (state == false)
                Faction = (byte)(Realm == Realms.REALMS_REALM_DESTRUCTION ? 8 : 6);
            else
                Faction = (byte)(Realm == Realms.REALMS_REALM_DESTRUCTION ? 72 : 68);

            if (!_initialized)
                return;

            if (ShouldChicken(Zone.Info.Tier, false))
                DispatchUpdateState((byte)StateOpcode.RvRFlag, state ? (byte)2 : (byte)0, 1); // chicken!
            else
                DispatchUpdateState((byte)StateOpcode.RvRFlag, state ? (byte)2 : (byte)0); // unflag for RvR on client

            // Pulldown if high level player flags in a lower level zone
            if (state)
            {
                if (ScnInterface.Scenario == null && ShouldDebolster(Zone.Info.Tier))
                    TryDebolster(ScnInterface.Scenario?.Tier ?? Zone.Info.Tier);
            }

            else
            {
                if (ChickenDebuff != null)
                {
                    ChickenDebuff.BuffHasExpired = true;
                    ChickenDebuff = null;
                }

                if (ScnInterface.Scenario == null && ShouldDebolster(Zone.Info.Tier))
                    RemoveBolster();
            }

            if (IsActive && IsInWorld() && Loaded && !MoveBlock)
                lock (PlayersInRange)
                    foreach (Player plr in PlayersInRange)
                        SendMeTo(plr);
        }

        public void AssignChickenBuff(NewBuff chickendebuff)
        {
            ChickenDebuff = chickendebuff;
        }

        #endregion

        // 

        #region Quit

        public int DisconnectTime = DISCONNECT_TIME; // 20 Secondes = 20000
        public bool CloseClient;
        public bool Leaving;
        public bool StopQuit()
        {
            if (Leaving)
            {
                EvtInterface.RemoveEvent(Quit);
                DisconnectTime = DISCONNECT_TIME;
                Leaving = false;
                return true;
            }

            return false;
        }
        public bool CancelQuit(Object Sender, object Args)
        {
            if (StopQuit())
            {
                _pendingStuck = false;
                SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_CANCELLED_LOGOUT);
            }

            return false;
        }

        public void Quit()
        {
            Quit(false, false);
        }
        public void Quit(bool closeClient, bool moveToBind)
        {
            try
            {
                if ((GmLevel == 0) || (GmLevel == 1))
                {
                    if (IsMoving)
                    {
                        SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_MUST_NOT_MOVE_TO_QUIT);
                        return;
                    }

                    if (CbtInterface.IsInCombat || ScnInterface.Scenario != null)
                    {
                        SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CANT_QUIT_IN_COMBAT);
                        return;
                    }

                    if (AbtInterface.IsCasting())
                    {
                        SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CANT_QUIT_YOURE_CASTING);
                        return;
                    }

                    if (IsDead)
                    {
                        SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_CANT_QUIT_YOURE_DEAD);
                        return;
                    }
                }
                else
                {
                    SendClientMessage("Allowing fast quit due to GM level", ChatLogFilters.CHATLOGFILTERS_C_WHITE);
                }

                if (DisconnectTime >= DISCONNECT_TIME)
                {
                    EvtInterface.AddEvent(Quit, 5000, 5);
                }


                Leaving = true;


                _pendingStuck = moveToBind;

                SendLocalizeString("" + DisconnectTime / 1000, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_YOU_WILL_LOG_OUT_IN_X_SECONDS);
                DisconnectTime -= 5000;
                CloseClient = closeClient;

                if (!IsDisposed && (DisconnectTime < 0 || GmLevel > 1 || (GmLevel == 1 && (CurrentArea == null)))) // Leave
                {
                    DisconnectType = EDisconnectType.Clean;
                    if (GmMgr.GmList.Contains(this))
                        GmMgr.NotifyGMOffline(this);

                    Destroy();
                }
            }
            catch (Exception e)
            {
                Log.Error("Quit", e.ToString());
            }
        }

        public bool Save(Object sender, object args)
        {
            EvtInterface.AddEvent(Save, AUTO_SAVE_TIME, 0);
            return true;
        }
        public override void Save()
        {
            CalculatePlayedTime();
            _Value.LastSeen = TCPManager.GetTimeStamp();

            CharMgr.Database.SaveObject(_Value);

            if (Info.Influences != null)
                foreach (Characters_influence obj in Info.Influences)
                    CharMgr.Database.SaveObject(obj);
            if (Info.Bag_Pools != null)
                foreach (Characters_bag_pools obj in Info.Bag_Pools)
                    CharMgr.Database.SaveObject(obj);
            base.Save();
        }

        public void ForceSave()
        {
            UpdateWorldPosition();
            CalculatePlayedTime();
            CharMgr.Database.SaveObject(_Value);


            if (Info.Influences != null)
                foreach (Characters_influence Obj in Info.Influences)
                    CharMgr.Database.SaveObject(Obj);
            if (Info.Bag_Pools != null)
                foreach (Characters_bag_pools Obj in Info.Bag_Pools)
                    CharMgr.Database.SaveObject(Obj);
            base.Save();

            //CharMgr.Database.ForceSave();
        }

        public void SaveCharacterInfo()
        {
            CharMgr.Database.SaveObject(Info);
            base.Save();
        }

        public long LastKeepAliveTime { get; set; }

        #endregion

        #region Positions

        public DateTime? LastInteractTime { get; set; }
        private const int MAX_POSITION_HISTORY = 8;

        private class PositionHistory
        {
            public readonly Point3D Pos = new Point3D();
            public long Timestamp;
        }

        private readonly PositionHistory[] _positionHistory = new PositionHistory[MAX_POSITION_HISTORY];

        private long _lastPositionHistoryShift;

        public override bool SetPosition(ushort pinX, ushort pinY, ushort pinZ, ushort heading, ushort zoneId, bool sendState = false)
        {
            if (Client == null)
            {
                Log.Error(Name, "No CLIENT in Teleport!");
                Destroy();
                return false;
            }

            if (Client.State != (int)eClientState.Playing && !IsDisposed)
            {
                Client.State = (int)eClientState.Playing;
                AddPlayer(this);
                EvtInterface.Notify(EventName.Playing, this, null);
            }

            if (MoveBlock)
                return false;

            long timestamp = TCPManager.GetTimeStampMS();

            bool updated = base.SetPosition(pinX, pinY, pinZ, heading, zoneId, sendState);

            _Value.WorldX = WorldPosition.X;
            _Value.WorldY = WorldPosition.Y;
            _Value.WorldZ = WorldPosition.Z;
            _Value.WorldO = heading;

            // Position history

            if (_positionHistory[0] != null)
            {
                // Update in place if delta time too short
                if (timestamp - _lastPositionHistoryShift < 50)
                {
                    _positionHistory[0].Pos.X = WorldPosition.X;
                    _positionHistory[0].Pos.Y = WorldPosition.Y;
                    _positionHistory[0].Pos.Z = WorldPosition.Z;
                    _positionHistory[0].Timestamp = TCPManager.GetTimeStampMS();
                }

                else
                {
                    for (int i = MAX_POSITION_HISTORY - 1; i > 0; --i)
                    {
                        if (_positionHistory[i - 1].Timestamp == 0)
                            continue;

                        _positionHistory[i].Pos.X = _positionHistory[i - 1].Pos.X;
                        _positionHistory[i].Pos.Y = _positionHistory[i - 1].Pos.Y;
                        _positionHistory[i].Pos.Z = _positionHistory[i - 1].Pos.Z;
                        _positionHistory[i].Timestamp = _positionHistory[i - 1].Timestamp;

                    }

                    _positionHistory[0].Pos.X = WorldPosition.X;
                    _positionHistory[0].Pos.Y = WorldPosition.Y;
                    _positionHistory[0].Pos.Z = WorldPosition.Z;
                    _positionHistory[0].Timestamp = timestamp;

                    _lastPositionHistoryShift = timestamp;
                }
            }

            else
            {
                for (int i = 0; i < MAX_POSITION_HISTORY; ++i)
                    _positionHistory[i] = new PositionHistory();

                _positionHistory[0].Pos.X = WorldPosition.X;
                _positionHistory[0].Pos.Y = WorldPosition.Y;
                _positionHistory[0].Pos.Z = WorldPosition.Z;
                _positionHistory[0].Timestamp = timestamp;

                _lastPositionHistoryShift = timestamp;
            }

            if (_nextAreaCheckTime < timestamp)
            {
                _nextAreaCheckTime = timestamp + 2000;

                if (IsBanned && !MoveBlock && Zone != null && Zone.ZoneId != 175)
                    Teleport(175, 1530613, 106135, 4297, 1700);
                else
                    CheckArea();
            }

            return updated;
        }

        public void ZoneTeleport(ushort x, ushort y, ushort z, ushort heading)
        {
            if (x == 0 || y == 0)
                return;

            if (Zone == null)
                return;

            Point3D worldDest = ZoneService.GetWorldPosition(Zone.Info, x, y, z);
            IntraRegionTeleport((uint)worldDest.X, (uint)worldDest.Y, (ushort)worldDest.Z, heading);
        }

        public void IntraRegionTeleport(uint worldX, uint worldY, ushort worldZ, ushort heading)
        {
            if (worldX == 0 || worldY == 0 || Zone == null)
                return;

            uint destOffX = worldX >> 12;
            uint destOffY = worldY >> 12;

            Zone_Info newZone = Region.GetZone((ushort)destOffX, (ushort)destOffY);

            if (newZone == null)
                return;

            ZoneMgr newZoneMgr = Region.GetZoneMgr(newZone.ZoneId);

            PacketOut Out = new PacketOut((byte)Opcodes.F_PLAYER_JUMP, 20);
            Out.WriteUInt32(worldX);
            Out.WriteUInt32(worldY);
            Out.WriteUInt16(Oid);
            Out.WriteUInt16(worldZ);
            Out.WriteUInt16(heading);
            Out.Fill(0, 5);
            Out.WriteByte(1);
            SendPacket(Out);

            X = newZoneMgr.CalculPin(worldX, true);
            Y = newZoneMgr.CalculPin(worldY, false);

            SetPosition((ushort)X, (ushort)Y, worldZ, heading, newZone.ZoneId);

            if (CurrentSiege is Siege)
                CurrentSiege.SetPosition((ushort)X, (ushort)Y, worldZ, heading, newZone.ZoneId);
        }

        public void Teleport(ushort zoneID, uint worldX, uint worldY, ushort worldZ, ushort worldO)
        {
            if (IsBanned && zoneID != 175)
            {
                SendClientMessage("You try to teleport yourself back onto the world, but it's just too far away.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return;
            }

            Zone_Info destination = ZoneService.GetZone_Info(zoneID);
            if (destination == null)
                return;

            if (Zone != null && zoneID != Zone.ZoneId)
            {
                QtsInterface.PublicQuest?.RemovePlayer(this, true);
                CurrentKeep?.RemovePlayer(this);
                //WorldMgr.InstanceMgr?.RemovePlayerFromInstances(this);
            }

            // Change Region , so change thread and maps
            if (Zone == null || Zone.Info.Region != destination.Region)
            {
                if (destination.Type == 4)
                {
                    Zone_jump jump = new Zone_jump();
                    jump.ZoneID = destination.ZoneId;
                    jump.WorldX = worldX;
                    jump.WorldY = worldY;
                    jump.WorldZ = worldZ;
                    jump.WorldO = worldO;

                    WorldMgr.InstanceMgr.ZoneIn(this, (byte)destination.Type, jump);
                }
                else
                {
                    RegionMgr newRegion = WorldMgr.GetRegion(destination.Region, true);

                    if (newRegion != null)
                        Teleport(newRegion, destination.ZoneId, worldX, worldY, worldZ, worldO);
                }
            }
            else // Teleport in current region
            {
                FallGuard = true;
                IntraRegionTeleport(worldX, worldY, worldZ, worldO);
            }
        }

        public void Teleport(RegionMgr region, ushort zoneID, uint worldX, uint worldY, ushort worldZ, ushort worldO)
        {
            if (region == null)
                return;

            if (IsBanned && zoneID != 175)
            {
                SendClientMessage("You try to teleport yourself back onto the world, but it's just too far away.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return;
            }

            RegionMgr oldRegion = Region;

            if (Zone != null && zoneID != Zone.ZoneId)
            {
                QtsInterface.PublicQuest?.RemovePlayer(this, true);
                CurrentKeep?.RemovePlayer(this);
            }

            if (Region == region)
                IntraRegionTeleport(worldX, worldY, worldZ, worldO);
            else
            {
                _Value.WorldX = (int)worldX;
                _Value.WorldZ = worldZ;
                _Value.WorldY = (int)worldY;
                _Value.WorldO = worldO;
                _Value.ZoneId = zoneID;
                _Value.RegionId = region.RegionId;

                if (region.AddObject(this, zoneID))
                {
                    PacketOut Out = new PacketOut((byte)Opcodes.F_WORLD_ENTER, 64);
                    Out.WriteUInt16(0x0608); // TODO
                    Out.Fill(0, 20);
                    Out.WriteString("38699", 5);
                    Out.WriteString("38700", 5);
                    Out.WriteString("0.0.0.0", 20);
                    SendPacket(Out);

                    SetOffset((ushort)(worldX >> 12), (ushort)(worldY >> 12), false);
                    SendSwitchRegion(zoneID);

                    MoveBlock = true;

                    OnRegionChanged(oldRegion);
                }
            }
        }

        private void OnRegionChanged(RegionMgr oldRegion)
        {
            // Purge buff appearance
            BuffInterface?.RemoveAllBuffs();
            //BuffInterface?.Update(TCPManager.GetTimeStampMS());

            if (CbtInterface.IsInCombat)
                CbtInterface.LeaveCombat();

            if (StsInterface.BolsterLevel > 0)
                RemoveBolster();

            try
            {
                if (WorldGroup != null)
                {
                    lock (WorldGroup)
                    {
                        if (WorldGroup != null)
                        {
                            if (WorldGroup._warbandHandler != null)
                            {
                                lock (WorldGroup._warbandHandler)
                                {
                                    if (WorldGroup._warbandHandler != null)
                                    {
                                        WorldGroup._warbandHandler.WarbandCompositionDirty = true;
                                        WorldGroup._warbandHandler.WarbandStatusDirty = true;
                                    }
                                }
                            }
                            else
                            {
                                WorldGroup._groupCompositionDirty = true;
                                WorldGroup._groupStatusDirty = true;
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            if (CbtInterface.IsPvp)
            {
                if (ChickenDebuff != null)
                {
                    ChickenDebuff.BuffHasExpired = true;
                    ChickenDebuff = null;
                }

                ((CombatInterface_Player)CbtInterface).DisablePvp(0, true);
            }

            if (CurrentArea != null && CurrentArea.IsRvR)
            {
                // NEWDAWN
                if (Region.Campaign != null)
                {
                    oldRegion?.Campaign?.NotifyLeftLake(this);
                }
                else
                {
                    oldRegion?.Campaign?.NotifyLeftLake(this);
                }
            }


            Scenario sc = ScnInterface.Scenario;

            if (sc != null && oldRegion != null && oldRegion.Scenario == sc)
                sc.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.NotifyPlayerLeft, this));
        }

        #region Fall/Jump

        private bool _wasGrounded = true;
        public bool WasGrounded
        {
            get { return _wasGrounded; }
            set
            {
                if (value && !_wasGrounded)
                {
                    _apexHit = false;
                    FallGuard = false;
                }

                _wasGrounded = value;

            }
        }

        private bool _apexHit;

        private byte _fallState;
        public byte FallState
        {
            get { return _fallState; }
            set
            {
                _fallState = value;
                if (value > 3)
                    _apexHit = true;
                // Slow players when they jump.
                else if (CbtInterface.IsInCombat && value == 2)
                    _slowOnLand = true;
            }
        }

        public bool FallGuard { get; private set; }
        public Unit PuntedBy { get; set; }
        /// <summary> Number of packets received which indicated that the player was airborne. Set to -1 when the player lands. </summary>
        public sbyte AirCount;
        private bool _slowOnLand;

        public void CalculateFallDamage(bool terminal = false)
        {
            /*
            // Bunny hop detection!
            if (_slowOnLand)
            {
                if (!FallGuard)
                    ApplyLandPenalty();
                _slowOnLand = false;
            }
            */

            if (!terminal && (!_apexHit || FallGuard || FallState >= 21))
                return;

            uint fallDamage = (uint)(MaxHealth * Math.Pow((23 - FallState) * 0.5, 1.5) * 0.05f);

            if (terminal)
                fallDamage = MaxHealth;

            // Deal the damage
            ReceiveDamage(this, fallDamage);

            PacketOut Outl = new PacketOut((byte)Opcodes.F_CAST_PLAYER_EFFECT, 16);

            Outl.WriteUInt16(Oid);
            Outl.WriteUInt16(Oid);
            Outl.WriteUInt16(0); // 00 00 07 D D

            Outl.WriteByte(0);
            Outl.WriteByte(11); // DAMAGE EVENT
            Outl.WriteByte(7);   //7    o 42

            Outl.WriteZigZag(-(int)fallDamage);
            Outl.WriteByte(0);

            SendPacket(Outl);

            if (IsDead && CbtInterface.IsPvp)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_DEATHSPAM, 96);

                Out.WriteByte((byte)("falling".Length + 1));
                Out.WriteHexStringBytes("6504");
                Out.WriteByte(Info.Realm == (byte)Realms.REALMS_REALM_ORDER ? (byte)2 : (byte)1); // faction
                Out.WriteByte(0);
                Out.WriteStringBytes("falling");
                Out.WriteByte(0);

                Out.WriteByte((byte)(Name.Length + 1)); // len for weapon name
                Out.WriteHexStringBytes("4207");
                Out.WriteByte(Info.Realm == (byte)Realms.REALMS_REALM_ORDER ? (byte)2 : (byte)1); // faction
                Out.WriteByte(1);
                Out.WriteStringBytes(Info.Name);
                Out.WriteByte(0);

                string areaName = GetAreaName();

                Out.WriteByte((byte)(areaName.Length + 1));
                Out.WriteHexStringBytes("B0FE0000");
                Out.WriteUInt16R(0);
                Out.WriteByte(0);
                Out.WriteByte(0); // attack type
                Out.WriteByte(1); // len for weapon name
                Out.WriteStringBytes(areaName);
                Out.WriteByte(0);
                Out.WriteStringBytes("");
                Out.WriteByte(0);
                Out.WriteByte(0);

                lock (_Players)
                {
                    foreach (Player player in _Players)
                    {
                        if (player.Region == Region)
                            player.SendPacket(Out);
                    }
                }

            }
        }

        private int _speedPenCount;
        private long _nextSpeedPenLiftTime;

        public void ApplyLandPenalty()
        {
            if (_speedPenCount < 4)
            {
                ++_speedPenCount;

                if (_speedPenCount > 1)
                    StsInterface.Speed = (ushort)(100 - 5 * (_speedPenCount - 1));
                else
                    StsInterface.Speed = 100;
                SendSpeed();
            }
            _nextSpeedPenLiftTime = TCPManager.GetTimeStampMS() + 2000;
        }

        #endregion

        #region Area Detection

        public Zone_Area CurrentArea;
        public byte CurrentPQArea;
        private long _nextAreaCheckTime;

        private void CheckArea()
        {
            if (MoveBlock)
                return;

            byte pqarea = Zone.ClientInfo.GetPQAreaFor((ushort)X, (ushort)Y, Zone.ZoneId);
            //Log.Info("pqarea", "  " + pqarea);

            //overlaying pqs  we will use 2 shaedes of color where they overlap and decide by z if its color 1 or color 2
            if (Zone.ZoneId == 60 && pqarea == 24)
            {
                if ((ushort)Z > 23873)
                    pqarea = 7;
                else
                    pqarea = 16;
            }
            else if (pqarea != 29 && pqarea != 31 && Zone.ZoneId == 3 && pqarea > 16)  // pqs above the keep black craig
            {
                if ((ushort)Z > 8326 && pqarea != 30)
                    pqarea -= 14;
                else if ((ushort)Z < 8326)
                    pqarea = 30;
                else
                    pqarea = 31;
            }
            Log.Dump("CheckArea", " PQ Area : " + pqarea);

            if (CurrentPQArea != pqarea)
            {
                if (pqarea == 31)
                {
                    QtsInterface.PublicQuest?.RemovePlayer(this, false);
                    CurrentKeep?.RemovePlayer(this);
                }
                else if (pqarea > 28)  // keeps
                {
                    foreach (var keep in Region.Campaign.Keeps)
                    {
                        if (keep.Info.ZoneId == Zone.ZoneId && keep.Info.PQuest?.PQAreaId == pqarea)
                        {
                            Log.Info("CheckArea", " Adding Keep : " + keep.Info.Name);
                            keep.AddPlayer(this);
                        }
                    }
                }
                else
                {
                    foreach (KeyValuePair<uint, PublicQuest> pq in Region.PublicQuests)
                    {
                        if (pq.Value.Info.ZoneId == Zone.ZoneId && pqarea == pq.Value.Info.PQAreaId)
                        {
                            pq.Value.AddPlayer(this);
                        }
                    }
                }
                CurrentPQArea = pqarea;
            }

            Zone_Area newArea = Zone.ClientInfo.GetZoneAreaFor((ushort)X, (ushort)Y, Zone.ZoneId, (ushort)Z);
            //if ((newArea == null && CurrentArea == null) || newArea != CurrentArea)
            if (newArea != CurrentArea)
            {
                if (newArea != null)
                {
                    if (CurrentArea == null || CurrentArea.AreaName != newArea.AreaName)
                    {
                        SendLocalizeString(newArea.AreaName ?? "an unknown area of the world.", ChatLogFilters.CHATLOGFILTERS_ZONE_AREA, Localized_text.TEXT_YOU_HAVE_ENTERED_AREA_X);
                        SendLocalizeString(newArea.AreaName ?? "Unknown Area", ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_ZONE_X);
                    }

                    TokInterface.AddTok(newArea.TokExploreEntry);

                    if (Program.Config.OpenRvR || newArea.IsRvR)
                    {
                        if (!CbtInterface.IsPvp && !_isRvRCountdown)
                            SetRvRCountdown(true);

                        // Entering the RvR zone while in duel state cancels duel
                        if (Faction == 64)
                        {
                            SurrenderDuel();
                            return;
                        }
                        if (StsInterface.BolsterLevel == 0)
                            TryBolster(0, newArea);

                        if (ScnInterface.Scenario == null)
                        {
                            if (CurrentArea == null || !CurrentArea.IsRvR || CurrentArea.ZoneId != newArea.ZoneId)
                            {
                                // NEWDAWN
                                if (Region.Campaign != null)
                                    Region.Campaign.NotifyEnteredLake(this);

                            }
                        }
                    }

                    else
                    {
                        if (StsInterface.BolsterLevel > 0 && (Zone?.Info == null || !ShouldDebolster(Zone.Info.Tier)))
                            RemoveBolster();

                        if (!CbtInterface.IsPvp && _isRvRCountdown)
                            SetRvRCountdown(false);

                        if (CurrentArea != null && CurrentArea.IsRvR)
                        {
                            Region.Campaign?.NotifyLeftLake(this);
                        }
                    }
                }

                else
                {
                    if (CurrentArea != null && CurrentArea.IsRvR)
                    {
                        Region.Campaign?.NotifyLeftLake(this);
                    }

                    if (CurrentArea != null)
                        SendLocalizeString(CurrentArea.AreaName, ChatLogFilters.CHATLOGFILTERS_ZONE_AREA, Localized_text.TEXT_YOU_HAVE_LEFT_AREA_X);
                }

                CurrentArea = newArea;
                SendChapterBar();
            }
            //     else 
            //SendClientMessage("Same Area");
        }

        public string GetAreaName()
        {
            return CurrentKeep?.Info.Name ?? CurrentObjectiveFlag?.Name ?? CurrentArea?.AreaName ?? Zone.Info.Name;
        }

        private bool _pendingRescue;
        /*
        private void CheckZoneValidity()
        {
            if (Zone != null && Zone.Info.Illegal)
            {
                if (GmLevel == 0 && !_pendingRescue && !IsSummoned)
                {
                    Log.Info("World", Name + " entered an illegal area (" + Zone.Info.Name + ")");

                    SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_ERROR_INVALID_AREA_ENTRY);
                    SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_C_WHITE, Localized_text.TEXT_ERROR_INVALID_AREA_ENTRY);

                    _pendingRescue = true;
                    EvtInterface.AddEvent(Rescue, 5000, 1);
                }
            }

            else if (_pendingRescue)
            {
                EvtInterface.RemoveEvent(Rescue);
                _pendingRescue = false;
            }
        }
        */
        public void Rescue()
        {
            RallyPoint rallyPoint = RallyPointService.GetRallyPoint(Info.Value.RallyPoint);

            if (rallyPoint != null)
            {
                if (IsBanned)
                    Teleport(175, 1530613, 106135, 4297, 1700);
                else Teleport(rallyPoint.ZoneID, rallyPoint.WorldX, rallyPoint.WorldY, rallyPoint.WorldZ, rallyPoint.WorldO);
                SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_WARNING_SAFE_POINT_RESCUE);
            }
        }

        #endregion

        #endregion

        #region Range

        private static GameObject _myLocationObject;
        private static GameObject _myAttackerObject;

        private int _enemiesInRange;

        public override int GetAbilityRangeTo(Unit caster)
        {
            Player player = caster as Player;

            if (player == null)
                return GetDistanceToObject(caster, true);

            Point3D desiredLocation = GetHistoricalPosition(TCPManager.GetTimeStampMS() - player.Latency);

#if DEBUG && POSITION_INDICATE

            if (_myLocationObject == null)
            {
                GameObject_spawn spawn = new GameObject_spawn
                {
                    Guid = (uint) WorldMgr.GenerateGameObjectSpawnGUID(),
                    WorldO = Heading,
                    WorldX = desiredLocation.X,
                    WorldY = desiredLocation.Y,
                    WorldZ = (ushort)desiredLocation.Z,
                    ZoneId = Zone.ZoneId,
                };
                spawn.BuildFromProto(WorldMgr.GetGameObjectProto(191));

                _myLocationObject = new GameObject(spawn);
                Region.AddObject(_myLocationObject, spawn.ZoneId);
            }

            else if (_myLocationObject.Zone != null)
            {
                _myLocationObject.SendRemove(null);

                _myLocationObject.Spawn.WorldX = desiredLocation.X;
                _myLocationObject.Spawn.WorldY = desiredLocation.Y;
                _myLocationObject.Spawn.WorldZ = (ushort)desiredLocation.Z;

                _myLocationObject.SetPosition(
                    ZoneMgr.CalculPin(Zone.Info, desiredLocation.X, true),
                    ZoneMgr.CalculPin(Zone.Info, desiredLocation.Y, false),
                    (ushort) desiredLocation.Z,
                    0,
                    Zone.ZoneId);

                
                _myLocationObject.SendMeTo(player);
                _myLocationObject.SendMeTo(this);
            }

            if (_myAttackerObject == null)
            {
                GameObject_spawn spawn = new GameObject_spawn
                {
                    Guid = (uint) WorldMgr.GenerateGameObjectSpawnGUID(),
                    WorldO = Heading,
                    WorldX = player.WorldPosition.X,
                    WorldY = player.WorldPosition.Y,
                    WorldZ = (ushort)player.WorldPosition.Z,
                    ZoneId = Zone.ZoneId,
                };
                spawn.BuildFromProto(WorldMgr.GetGameObjectProto(191));

                _myAttackerObject = new GameObject(spawn);
                Region.AddObject(_myAttackerObject, spawn.ZoneId);
            }

            else if (_myAttackerObject.Zone != null)
            {
                _myAttackerObject.SendRemove(null);

                _myAttackerObject.Spawn.WorldX = player.WorldPosition.X;
                _myAttackerObject.Spawn.WorldY = player.WorldPosition.Y;
                _myAttackerObject.Spawn.WorldZ = (ushort) player.WorldPosition.Z;

                _myAttackerObject.SetPosition((ushort) caster.X, (ushort) caster.Y, (ushort) caster.Z, 0, Zone.ZoneId);

                _myAttackerObject.SendMeTo(player);
                _myAttackerObject.SendMeTo(this);
            }

#endif

            int dist = Math.Max(0, (int)(caster.GetDistanceToWorldPoint(desiredLocation) - (BaseRadius + player.BaseRadius)));

#if DEBUG && POSITION_INDICATE
            player.SendClientMessage("Shifted position back in time by "+player.Latency+" ms - distance to target: "+dist+"ft.");
#endif
            return dist;
        }

        public Point3D GetHistoricalPosition(long desiredTimestamp)
        {
            Point3D desiredLocation = new Point3D(WorldPosition);

            for (int i = 0; i < MAX_POSITION_HISTORY && _positionHistory[i] != null; ++i)
            {
                if (desiredTimestamp >= _positionHistory[i].Timestamp || i == MAX_POSITION_HISTORY - 1)
                {
                    if (i == 0 || i == MAX_POSITION_HISTORY - 1)
                    {
                        desiredLocation.X = _positionHistory[i].Pos.X;
                        desiredLocation.Y = _positionHistory[i].Pos.Y;
                        desiredLocation.Z = _positionHistory[i].Pos.Z;
                    }

                    else // Interpolate between this point and the next most recent one.
                    {
                        Point3D oldPoint = _positionHistory[i].Pos;
                        Point3D newPoint = _positionHistory[i - 1].Pos;

                        float lerpFactor = (desiredTimestamp - _positionHistory[i].Timestamp) / (float)(_positionHistory[i - 1].Timestamp - _positionHistory[i].Timestamp);

                        desiredLocation.X = oldPoint.X + (int)((newPoint.X - oldPoint.X) * lerpFactor);
                        desiredLocation.Y = oldPoint.Y + (int)((newPoint.Y - oldPoint.Y) * lerpFactor);
                        desiredLocation.Z = oldPoint.Z + (int)((newPoint.Z - oldPoint.Z) * lerpFactor);
                    }

                    break;
                }
            }

            return desiredLocation;
        }

        public override void ClearRange(bool fromNewRegion = false)
        {
            if (DebugMode)
                SendClientMessage(fromNewRegion ? "Clearing range from new region" : "Clearing range from RemoveObject");

            AiInterface.ClearRange();

            // When leaving a region, notify players within that this player left if the region is still open
            if (!fromNewRegion)
                SendRemove(null);

            List<Object> rangedObjects = new List<Object>();

            lock (ObjectsInRange)
                rangedObjects.AddRange(ObjectsInRange);

            // Remove this object from other objects' ranged lists
            foreach (Object rangedObject in rangedObjects)
                rangedObject.RemoveInRange(this);

            lock (PlayersInRange)
            {
                PlayersInRange.Clear();
                _enemiesInRange = 0;
            }

            lock (ObjectsInRange)
                ObjectsInRange.Clear();
        }

        public Object GetObjectInRange(int oid)
        {
            lock (ObjectsInRange)
            {
                foreach (Object obj in ObjectsInRange)
                {
                    if (obj.Oid == oid)
                        return !obj.IsDisposed ? obj : null;
                }
            }

            return null;
        }

        /// <summary>
        /// Called by the Region manager when another object comes into this object's range.
        /// </summary>
        public override void AddInRange(Object obj)
        {
            if (obj == null)
                return;

            lock (ObjectsInRange)
                ObjectsInRange.Add(obj);

            if (obj is Unit)
                AiInterface.AddRange((Unit)obj);

            Player plr = obj as Player;

            if (plr != null)
            {
                lock (PlayersInRange)
                {
                    PlayersInRange.Add(plr);

                    if (plr.Realm != Realm)
                        _enemiesInRange++;
                }
            }
        }

        /// <summary>
        /// Called by the Region manager when an object leaves this object's range, and by another object when it clears its ranged object lists.
        /// </summary>
        /// <param name="obj"></param>
        public override void RemoveInRange(Object obj)
        {
            if (obj == null)
                return;

            lock (ObjectsInRange)
            {
                if (!ObjectsInRange.Contains(obj))
                    return;

                ObjectsInRange.Remove(obj);
            }

            if (obj is Unit)
                AiInterface.RemoveRange((Unit)obj);

            Player plr = obj as Player;

            if (plr != null)
            {
                lock (PlayersInRange)
                {
                    if (PlayersInRange.Remove(plr) && plr.Realm != Realm)
                        _enemiesInRange--;
                }
            }

            obj.SendRemove(this);
        }

        #endregion

        #region Player State Send

        /// <summary> Indicates that the server should broadcast the next PLAYER_STATE2 packet received. </summary>
        public bool ForceSendPosition { get; set; }
        /// <summary> Set when the player has sent a PLAYER_STATE2 packet. </summary>
        public long LastStateRecvTime { get; set; }

        public byte SendCounter = 0;

        public override void SendState(Player plr = null)
        {
        }

        #endregion

        #region Info

        public override string ToString()
        {
            string Info = "";

            Info += "Name=" + Name + ",Ip=" + (Client != null ? Client.GetIp() + " ClientId " + Client.Id : "Disconnected") + base.ToString();
            Info += $"CurrentArea={CurrentArea?.AreaName}/{CurrentArea?.IsRvR}/{CurrentArea?.DestroInfluenceId}/{CurrentArea?.OrderInfluenceId}";
            return Info;
        }

        public static string AsCharacterName(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length <= 1)
                return name.ToUpper();
            return char.ToUpper(name[0]) + name.Substring(1);
        }

        #endregion

        #region Group

#warning MOVE TO GROUPINTERFACE

        public Group PriorityGroup => ScenarioGroup ?? WorldGroup;
        public Group ScenarioGroup { get; set; }
        public Group WorldGroup { get; set; }

        public void SetGroup(Group group, bool broadcast = true)
        {
            if (PendingDisposal)
                return;

            WorldGroup = group;
            WorldGroup.AddMember(this, broadcast);
        }

        public void SetScenarioGroup(Group group)
        {
            ScenarioGroup = group;
            ScenarioGroup.AddScenarioMember(this);
        }

        #endregion

        #region Played LastUpdatedTime

        uint _lastCheckedTime = (uint)TCPManager.GetTimeStampMS();

        public long NextJumpTime;

        public void CalculatePlayedTime()
        {
            uint CurrentTime = (uint)TCPManager.GetTimeStamp();

            uint dTime = CurrentTime - _lastCheckedTime;

            _lastCheckedTime = CurrentTime;

            _Value.PlayedTime += dTime;
        }

        #endregion

        public bool CheckHotSpot(Object sender, object args)
        {
            if (!IsInWorld())
                return false;

            if (!(sender is Player))
                return false;

            var targetPlayer = (Player)sender;



            if (Zone != null)
            {
                var halfway = Point2D.Lerp(new Point2D(targetPlayer.X, targetPlayer.Y), new Point2D(X, Y), 0.5f);
                Zone.AddHotspotDamage(halfway.X, halfway.Y);
            }

            return false;

        }

        public Pet Companion { get; set; }
        public bool PendingDumpStatic;

        #region Lockouts

        public bool HasLockout(ushort zoneId, uint bossId)
        {
            string lockout = _Value.GetLockout(zoneId);
            if (lockout == null)
                return false;

            if (lockout.Contains(bossId.ToString()))
                return true;

            //var split = lockout.Split(':');
            //for (int i = 2; i < split.Length; i++)
            //{
            //	if (uint.Parse(split[i]).Equals(BossId))
            //		return true;
            //}

            return false;
        }

        #endregion

        public void GroupRefresh()
        {
            if (PriorityGroup == null)
                return;

            if (PriorityGroup.IsWarband)
                SendClientMessage($"Warband...");
            else
            {

                SendClientMessage($"Party...");
            }

            foreach (var member in PriorityGroup.Members)
            {
                SendClientMessage($"Member {member.Name} is in your group.");
            }

            SendClientMessage($"Leader is {PriorityGroup.Leader.Name}");

            if (WorldGroup.PartyOpen)
                SendClientMessage($"Group is open..");
            else
                SendClientMessage($"Group is closed..");


        }


        public bool GetCountOfPlayerItems(int itemId, int maxCount)
        {
            return ItmInterface.HasItemCountInInventory((uint)itemId, (ushort)maxCount);
        }

        public bool IsValidForReward(Player victim)
        {
            // Do not reward if player is in another zone to the victim (is afk, or not in pvp)
            if ((ZoneId != victim.ZoneId) || (IsAFK) || (!CbtInterface.IsPvp) || (IsDisposed) || (PendingDisposal))
            {
                RewardLogger.Debug(
                    $"+ Skipping rewards for {Name} ({CharacterId}) - different zone / afk/ not pvp/scen to victim");
                return false;
            }
                    
            if (GetObjectInRange(victim.Oid) ==null)
            {
                RewardLogger.Debug(
                    $"+ Skipping rewards for {Name} ({CharacterId})  - distance from victim");
                return false;
            }

            return true;
        }
    }
}


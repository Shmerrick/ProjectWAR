using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.World.BattleFronts;
using WorldServer.World.BattleFronts.Objectives;
using NLog;
using WorldServer.World.Battlefronts.Apocalypse;

namespace WorldServer
{
    public class LootRoll
    {
        public bool Completed;
        public long StartTime;
        public Item_Info Item;
        public byte LootID;

        private readonly List<Player> _playersGreeding = new List<Player>();
        private readonly List<Player> _playersNeeding = new List<Player>();
        private readonly List<Player> _playersPassing = new List<Player>();

        public LootRoll(byte lootID, Item_Info item)
        {
            LootID = lootID;
            Item = item;
            StartTime = TCPManager.GetTimeStampMS();
        }

        public int ProcessVote(Player voter, ushort vote)
        {
            if (_playersNeeding.Contains(voter) || _playersGreeding.Contains(voter))
            {
                voter.SendClientMessage("You've already rolled on this item.", ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL);
                return -1;
            }

            switch (vote)
            {
                case 0:
                    // Explicitly check whether a player can use this item.
                    // Need on use doesn't appear to work on the client side, so we change Need rolls to Greed
                    // for items not usable by the player's career or race.
                    if (!voter.ItmInterface.CanUseItemType(Item))
                    {
                        voter.SendClientMessage("Your Need vote was changed to Greed because you cannot use this item.", ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL);
                        goto case 1;
                    }
                    _playersNeeding.Add(voter);
                    voter.SendLocalizeString(Item.Name, ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_YOU_SELECT_NEED_FOR);
                    return 0;
                case 1:
                    _playersGreeding.Add(voter);
                    voter.SendLocalizeString(Item.Name, ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_YOU_SELECT_GREED_FOR);
                    return 1;
                case 2:
                    _playersPassing.Add(voter);
                    voter.SendLocalizeString(Item.Name, ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_YOU_SELECT_PASS_FOR);
                    return 2;
            }

            return -1;
        }

        public int GetVoteCount()
        {
            return _playersGreeding.Count + _playersNeeding.Count + _playersPassing.Count;
        }

        public bool GetWinner(Group gr)
        {
            int value;
            int highestRand = 0;
            Player highestPlayer = null;

            if (_playersNeeding.Count > 0)
            {
                foreach (Player plr in gr.Members)
                    plr.SendLocalizeString(Item.Name, ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_NEED_ROLL_HEADER);

                foreach (Player plr in _playersNeeding)
                {
                    value = StaticRandom.Instance.Next(1, 100);
                    if (value > highestRand)
                    {
                        highestRand = value;
                        highestPlayer = plr;
                    }
                    plr.SendLocalizeString(value.ToString(), ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_YOU_ROLL_NUMBER);
                    foreach (Player mem in gr.Members)
                    {
                        if (plr != mem)
                            mem.SendLocalizeString(new string[] { plr.Name, value.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_NAME_ROLLS_NUMBER);
                    }
                }

            }

            else if (_playersGreeding.Count > 0)
            {
                foreach (Player player in gr.Members)
                    player.SendLocalizeString(Item.Name, ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_GREED_ROLL_HEADER);

                foreach (Player player in _playersGreeding)
                {
                    if (player != null)
                    {
                        value = StaticRandom.Instance.Next(1, 100);
                        if (value > highestRand)
                        {
                            highestRand = value;
                            highestPlayer = player;
                        }
                        player.SendLocalizeString(value.ToString(), ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_YOU_ROLL_NUMBER);
                        foreach (Player mem in gr.Members)
                        {
                            if (player != mem)
                                mem.SendLocalizeString(new string[] { player.Name, value.ToString() }, ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, Localized_text.TEXT_NAME_ROLLS_NUMBER);
                        }
                    }
                }
            }

            if (highestPlayer != null)
            {
                highestPlayer.SendLocalizeString(new[] { Item.Name, "1" }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_YOU_RECEIVE_ITEM_X);
                foreach (Player mem in gr.Members)
                {
                    if (mem != highestPlayer)
                        mem.SendLocalizeString(new[] { highestPlayer.Name, Item.Name, "1" }, ChatLogFilters.CHATLOGFILTERS_LOOT, Localized_text.TEXT_PLAYER_X_RECEIVES_ITEM_X);
                }
                highestPlayer.ItmInterface.CreateItem(Item, 1);
            }

            Completed = true;
            return true;
        }
    }

    public enum EGroupAction
    {
        PlayerJoin,
        PlayerLeave,
        PlayerKick,
        ChangeLeader,
        ChangeMainAssist,
        ChangeMasterLooter,
        ChangeLootOption,
        ChangeAutoLoot,
        ChangeNeedOnUse,
        OpenParty,
        CloseParty,
        WarbandMove,
        WarbandSwap,
        FormWarband
    }

    public class GroupAction
    {
        public EGroupAction Action;
        public Player Instigator;
        public string ActionString;

        public GroupAction(EGroupAction action)
        {
            Action = action;
        }

        public GroupAction(EGroupAction action, Player instigator)
        {
            Action = action;
            Instigator = instigator;
        }

        public GroupAction(EGroupAction action, string actionString)
        {
            Action = action;
            ActionString = actionString;
        }

        public GroupAction(EGroupAction action, Player instigator, string actionString)
        {
            Action = action;
            Instigator = instigator;
            ActionString = actionString;
        }
    }

    public class Group
    {
        #region Static

        private static readonly Logger RewardLogger = LogManager.GetLogger("RewardLogger");

        private static int _maxGroupID;
        public static int GetNextGroupId()
        {
            if (_maxGroupID > 60000)
                Log.Error("Groups", "Group ID is approaching maximum value!");

            if (_maxGroupID == ushort.MaxValue)
                Interlocked.Exchange(ref _maxGroupID, 0);

            return Interlocked.Increment(ref _maxGroupID);
        }
        public static List<Group> WorldGroups = new List<Group>();

        private static readonly ReaderWriterLockSlim GroupLockStatic = new ReaderWriterLockSlim();

        public static void SendWorldGroups(Player player)
        {
            List<Group> groups = GetWorldGroups(player.Info.Realm);

            byte numOpenGroups = 0;
            foreach (Group g in groups)
            {
                if (g.PartyOpen)
                    numOpenGroups++;
            }

            PacketOut Out = new PacketOut((byte)Opcodes.F_SOCIAL_NETWORK);
            Out.WriteUInt16(0);
            Out.WriteByte(0x0A);
            Out.WriteByte(numOpenGroups);
            Out.WriteByte(0);

            foreach (Group group in groups)
            {
                if (group.PartyOpen)
                    group.BuildOpenPartyInfo(player, Out);
            }

            player.SendPacket(Out);
        }
        public static List<Group> GetWorldGroups(int realm)
        {
            List<Group> groups = new List<Group>();

            GroupLockStatic.EnterReadLock();
            try
            {
                lock (Group.WorldGroups)
                {
                    foreach (Group group in WorldGroups)
                    {
                        if (group.Leader != null && group.Leader.IsInWorld() && group.Leader.Info.Realm == realm)
                            groups.Add(group);
                    }
                }
            }
            finally
            {
                GroupLockStatic.ExitReadLock();
            }

            return groups;
        }

        #endregion

        #region RVR Areas polygons
        //TODO: Move these rvr lake region definitions into db.
        public static Dictionary<ushort, List<ushort>> ZoneRVRAreas = new Dictionary<ushort, List<ushort>>() {
            /*Nordland*/    {106, new List<ushort>() { 26816, 21120, 24064, 20224, 22080, 18624, 22848, 16000, 22656, 13248, 21760, 9152, 23488, 3648, 26240, 2048, 28288, 3392, 30784, 2880, 32896, 1600, 34176, 576, 35392, 64, 39680, 768, 39872, 2368, 38336, 3520, 38208, 5248, 38976, 6528, 40960, 6592, 41792, 7616, 41216, 8576, 41920, 9728, 42624, 11456, 42624, 13952, 41344, 16000, 39360, 16640, 37056, 15552, 34176, 14528, 31168, 14912, 28480, 17024, 29888, 18880, 28544, 20608 } },
            /*Avelorn*/     {202, new List<ushort>() {11136,64896,14912,62784,19712,60032,19328,54144,21632,52032,26944,50112,30208,49472,32960,51712,35968,53952,39616,55616,42112,58176,47040,61824,50240,65024 } },
            /*Barack Varr*/ {7,   new List<ushort>() {12224,31616,14080,28864,16896,27968,20160,29056,24128,29184,26048,28480,27712,29824,27968,32128,30592,30976,33536,30976,35648,31744,37056,28928,38656,28672,40576,30016,43136,31168,44608,29824,45056,26560,43392,23424,42432,18560,43200,14144,47616,13632,51072,15744,50688,20608,51136,27008,50944,31040,49472,34432,47680,36608,44160,39808,39616,41280,36800,42368,33280,46144,31936,48320,29824,47168,28352,42240,26368,42432,23680,42624,20736,42368,19776,40512,18496,36608,15360,33280 } },
            /*Caledor*/     {203, new List<ushort>() {0,26112,3520,25088,6336,25280,11840,26816,15936,25600,17856,24960,20864,26496,23744,26368,29056,25600,32448,24384,35456,25536,42432,28352,49728,26240,53696,25536,57984,26560,61632,25152,64512,24832,65152,24384,65216,39680,61568,37952,58624,37760,54848,37248,52544,38016,49024,40512,45696,41344,42432,38912,37312,39488,35968,41216,33024,42176,30208,41664,29120,38080,26688,36608,23296,36672,17472,37248,14208,36672,11072,37248,7424,39808,3904,40896,1536,40192,0,39488} },
            /*Chrace*/      {206, new List<ushort>() {25344,3648,24832,8320,21696,10432,22080,12480,25088,12992,28608,13696,31168,14080,34048,14912,37120,15104,40576,11328,42624,9856,45248,9216,47104,7872,48832,4800,49408,1984,48512,256,23680,128,} },
            /*Dragonwake*/  {205, new List<ushort>() {1984,28096,4800,30592,7104,32576,10816,33088,16000,29376,17792,26496,19072,23488,22528,22016,25344,21184,26560,24512,28224,26240,32704,27520,39424,28224,44800,30080,48768,32384,51840,32896,56896,32896,60480,30720,63552,30784,65408,31296,65216,43200,62272,42432,58752,43072,54080,44096,51840,45760,49280,45056,46016,44864,42624,46016,40064,46720,37696,44480,32832,43840,29120,44608,26560,45440,24256,46912,20032,47104,16768,46464,14208,44544,11520,43392,9408,43200,7744,40768,6144,38720,1408,36864,0,36800,128,27392,} },
            /*High Pass*/   {102, new List<ushort>() {832,64896,1472,61888,1408,58624,1472,55360,2944,54336,5760,54144,7040,55360,6912,58176,7616,60992,9920,61696,12800,62080,15616,59328,17792,59712,19968,60992,24512,58560,26496,56768,29568,55616,31168,55808,32640,59584,33536,60736,35648,59072,38208,58752,39872,60992,42752,61824,45248,60928,45696,58176,46016,54848,47168,54080,49984,54592,51776,56896,53760,59776,55040,62272,54464,64704,54464,65088,576,65088,} },
            /*Karding Valley*/{9, new List<ushort>() {26496,2496,24192,3840,23424,5056,27072,8256,29056,10880,27648,13184,29184,14912,30464,16640,28928,21312,26688,26368,25792,27648,23040,26560,21376,23808,19200,24832,20480,27648,19968,29760,14656,29568,7360,29824,3648,31424,2816,33152,4224,34752,8000,36544,13824,38400,22720,40896,25536,42240,26304,44608,24256,48576,20032,49536,18112,51136,19456,53184,24000,50944,25600,51840,23104,57216,22592,59968,25792,60480,28928,61952,29632,64768,40192,65152,39232,59776,36864,56704,37824,53888,40832,51328,42432,49984,45440,48512,48832,46656,50240,44032,52224,41792,56256,39808,57984,38208,57856,36288,51584,33088,47552,26752,45760,21824,46208,16768,47424,14784,47616,12032,48128,9152,46976,8256,45696,11776,45440,13632,43456,13184,40640,11264,39168,9664,39680,7744,38784,6400,37248,4288,35968,2112,34880,256,27072,256,} },
            /*Marshes of Mad*/{1, new List<ushort>() {29312,26688,32832,25536,35968,27200,37312,29184,37952,30592,40320,32000,41728,34240,40640,39360,38080,41920,36544,43840,33280,42880,29312,42240,26688,44608,23936,47616,20608,50880,17216,51392,13888,50624,11008,46912,12224,40640,14336,38208,16640,36160,19776,35008,23040,33088,25280,30720,} },
             /*Norsca*/      {100, new List<ushort>() {35456,64896,36352,61440,37376,58176,36544,56320,30080,55808,26560,55488,24960,53632,28160,52544,30912,51328,34048,50560,34432,52032,34944,51776,34944,50368,36096,49792,37120,48960,37824,49472,38400,50624,38464,52224,39680,53184,41472,54336,43904,53696,45248,55168,44864,57856,44288,60160,43584,61440,41984,62272,41728,63744,41728,64960,} },
            /*Ostland*/     {107, new List<ushort>() {16000,0,16256,1920,16960,3840,18112,6400,20416,7360,23488,6656,25664,5056,26752,6720,26368,8000,25600,9408,27392,10880,29184,10304,30720,10560,31232,11968,33280,11392,37376,9408,39168,10432,41216,10304,43200,9600,45440,9216,47680,8000,49216,6912,49280,5760,50688,6464,52352,7104,54656,6912,55744,6656,57280,7744,58880,7360,58432,5760,57024,4352,55040,4032,50944,3904,48384,3200,47872,2240,47872,1088,46912,128,} },
            /*Praag*/       {105, new List<ushort>() {47488,65024,41984,58304,35840,55680,33536,49472,37888,44864,42496,42176,41728,36480,44096,33152,45056,29632,41472,22592,45568,15936,45952,11136,42496,3968,36352,2048,36352,64,31296,0,31104,4544,25984,6848,21120,7872,20736,13312,20480,18368,22976,21888,29696,21440,32128,23360,31168,26688,25152,29120,20800,31168,19520,35328,19648,39232,21504,43264,23360,48384,25344,50304,23616,54016,24896,59584,24896,63360,22912,65216,} },
            /*Reikland*/    {109, new List<ushort>() {14208,64768,14592,60032,15232,54656,15616,50688,15104,46144,15744,42944,15808,39168,18112,34176,20480,32000,22016,28480,24128,24384,25984,20608,25984,17856,25984,16704,25280,16192,23040,15488,22400,14080,23040,11712,24640,9920,24448,8576,23040,5888,21376,2496,19200,256,46656,128,45568,3136,44160,6144,43456,8448,42432,10432,39616,12992,37376,15040,37120,17472,37312,20992,38016,24192,39104,26240,39232,26944,37376,30976,37120,33344,36544,36288,35840,39616,34304,42944,32384,46848,30912,50624,31296,54592,31168,57024,30208,59520,29632,61696,30080,63680,30464,64640,} },
            /*Saphery*/     {208, new List<ushort>() {13248,0,13184,3136,12992,7168,13184,10816,13376,15936,15488,18560,19328,17344,24192,14784,28480,12736,32896,12992,35968,15232,39680,15360,43840,13824,45248,10816,46400,5888,46976,2432,47104,256,} },
            /*Talabecland*/ {108, new List<ushort>() {2368,0,2112,3520,4032,6400,7424,5760,9984,2688,12544,768,16000,768,18176,3136,18624,5888,19072,7424,22592,7552,25664,9408,29184,12032,32832,12928,37312,11776,38720,10560,39488,7040,40064,4544,42368,2624,45056,3584,47616,5056,52480,4224,54400,1728,55296,0,} },
            /*Badlands*/    {2, new List<ushort>()   {10688,45056,10880,41664,12288,39104,12928,36096,14656,33984,17216,32640,20288,30784,23744,29760,26688,33856,28352,36352,30592,34176,34176,33728,36096,34944,36864,37568,36800,41024,37824,44032,39232,45504,40896,46144,42944,46720,46592,47616,47424,49216,48832,51264,50368,54912,51584,60864,52032,64768,20224,64896,19200,59456,17472,56064,14528,52480,11584,49216,} },
            /*BlightedIsle*/{200, new List<ushort>() {21632,65152,22080,62080,23808,59968,24512,57920,27456,57728,29056,55040,29120,53184,28928,49472,32576,46016,37120,44992,40128,47360,41088,49920,44032,52032,47680,52224,48768,54208,47296,57984,47360,60480,48576,63232,48960,64896,} },
            /*Shadowlands*/ {201, new List<ushort>() {23104,65088,24512,60672,24384,56896,23936,54464,24128,51968,24192,47552,25408,43648,26112,39232,26112,34304,28160,33024,31744,31680,34944,33024,38080,33152,40576,33280,44544,33088,46464,36672,48768,39424,46848,42496,44800,47296,43584,49408,43904,51968,44736,54400,45312,57664,45568,61824,45440,64768,} },
            /*TM*/          {5, new List<ushort>()   {30272,2944,30592,5760,30784,9152,31488,12544,32320,14336,29056,15808,23296,17344,19456,19968,16640,23680,15232,26112,13824,28480,11776,28800,9472,27200,7168,26112,4480,27008,128,27968,64,42624,5632,42176,8128,40384,11136,38016,13824,36800,17088,40768,19584,44608,25536,49280,28160,51648,28224,53440,27200,55168,26496,56960,27904,60288,29312,62080,32000,64896,37312,64704,35008,61632,36352,60288,39104,59584,39680,57664,38720,55296,36608,54464,34944,55552,34176,56320,31488,54912,30976,53504,32192,52544,39680,51840,44608,49792,51072,46272,54208,41344,56320,38528,59136,36992,60416,35264,59072,32192,57600,30208,56000,28928,52800,25856,51072,23552,50624,20608,47552,18176,44480,18304,40128,18880,36160,17792,34880,15360,34688,13632,36096,14144,37504,14208,38464,12288,38208,10176,37376,9472,35456,8320,35584,5184,35968,2944,36288,256,30272,64,30720,128,} },
            /*TrollCountry*/{101, new List<ushort>() {1728,52288,2432,49984,5952,48768,7296,49408,10368,52352,13376,53376,17152,56448,18880,58304,23680,57472,31936,57152,35136,56704,37312,58176,38528,59072,42880,59136,46848,58624,48576,59520,49280,61440,48704,64000,49024,64768,49024,64960,15104,64896,14592,61952,13376,60160,10624,60160,9600,61568,8448,63488,6464,64512,3904,64448,1536,61888,1024,56896,1088,53888,} },
        };

        public static bool InPoly(List<ushort> poly, ushort x, ushort y)
        {
            int i, j;
            int nvert = poly.Count / 2;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                ushort px = poly[(i * 2)];
                ushort py = poly[(i * 2) + 1];

                if (((py > y) != (poly[(j * 2) + 1] > y)) &&
                 (x < (poly[(j * 2)] - px) * (y - py) / (poly[(j * 2) + 1] - py) + px))
                    c = !c;
            }
            return c;
        }

        public static bool PlayerInRvR(Player player)
        {
            return ZoneRVRAreas.ContainsKey(player.Zone.ZoneId) && InPoly(ZoneRVRAreas[player.Zone.ZoneId], (ushort)player.X, (ushort)player.Y);
        }

        #endregion

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public WarbandHandler _warbandHandler;
        public bool IsWarband => _warbandHandler != null;

        public bool IsScenarioGroup { get; }

        public ushort GroupId;

        public Player Leader => _warbandHandler?.Leader ?? _leader;
        private Player _leader;
        private Player _masterLooter;
        private Player _mainAssist;
        public bool PartyOpen { get; set; }

        public Realms Realm { get; private set; }

        public Group() { }

        public Group(WarbandHandler wb, bool isScenarioGroup)
        {
            GroupId = (ushort)GetNextGroupId();
            _warbandSlave = true;
            _warbandHandler = wb;
            IsScenarioGroup = isScenarioGroup;
        }

        public void Initialize(Player leader, Player member)
        {
            GroupId = (ushort)GetNextGroupId();

            leader.SetGroup(this, false);
            SetLeader(leader);
            SetMainAssist(leader);

            member.SetGroup(this);

            _groupCompositionDirty = true;
            _groupStatusDirty = true;

            GroupLockStatic.EnterWriteLock();
            try
            {
                lock (Group.WorldGroups)
                {
                    WorldGroups.Add(this);
                }
            }
            finally { GroupLockStatic.ExitWriteLock(); }
        }

        public void InitializeSolo(Player leader)
        {
            GroupId = (ushort)GetNextGroupId();

            leader.SetGroup(this, false);
            SetLeader(leader);
            SetMainAssist(leader);

            _groupCompositionDirty = true;
            _groupStatusDirty = true;

            GroupLockStatic.EnterWriteLock();
            try
            {
                lock (Group.WorldGroups)
                {
                    WorldGroups.Add(this);
                }
            }
            finally { GroupLockStatic.ExitWriteLock(); }
        }

        private long _nextTick;
        public readonly ReaderWriterLockSlim _updateRWLock = new ReaderWriterLockSlim();

        /// <summary>Used to prevent a new leader from updating the group before the previous leader has finished doing so.</summary>
        private bool _changingLeader;

        public void Update(long tick)
        {

            if (_changingLeader)
            {
                _changingLeader = false;
                return;
            }

            if (_warbandSlave)
            {
                //multiple groups can handle the same warband, need to make sure they don't all try at the same time BUT still process their info
                //also scenario groups are handled by the scenario manager, so dont process them. like, at all.
                if (_warbandHandler != null && !(_warbandHandler is ScenarioGroupsHandler))
                {
                    _warbandHandler.Update(tick);
                }
            }
            else
            {
                if (_groupActions.Count > 0)
                    ProcessGroupActions();

                if (_groupCompositionDirty && !_warbandSlave && Members.Count > 1)
                {
                    SendGroupComposition();

                    foreach (Player m in Members)
                    {
                        SendGroupOidListToMember(m);
                    }
                    _groupCompositionDirty = false;
                }
                if (_groupStatusDirty && !_warbandSlave && Members.Count > 1)
                {
                    SendGroupStatus();
                    _groupStatusDirty = false;
                }

                UpdateLootRolls(tick);
            }

            _changingLeader = false;
            /*
            if (scenario && nextSCtick < Tick)
            {
                PacketOut Out = new PacketOut((byte)Opcodes.F_SCENARIO_PLAYER_INFO);
                Out.WriteByte(2);
                Out.WriteByte(0x8E);
                Out.WriteByte(0);
                Out.WriteByte((byte)(Members.Count + Scnotingroup.Count));
                foreach (Player Plr in Members)
                {
                    //Out.WriteBitPack(Plr.CharacterId);
                    Out.WriteUInt32(Plr.CharacterId);
                    Out.WriteUInt32((uint)Plr.X);
                    Out.WriteUInt32((uint)Plr.Y);
                }
                foreach (Player Plr in Scnotingroup)
                {
                    Out.WriteBitPack(Plr.CharacterId);
                    //Out.WriteUInt32(Plr.CharacterId);
                    Out.WriteUInt32((uint)Plr.WorldPosition.X);
                    Out.WriteUInt32((uint)Plr.WorldPosition.Y);
                }
                SendToScenarioGroup(null, Out);
                SendToScenarioGroup(Scnotingroup, Out);
                nextSCtick = Tick + 3000;
            }
            /*
            if (isWarband && nextWBtick < Tick)
            {
                byte group1 = 1, group2 = 1, group3 = 1, group4 = 1;
                PacketOut Out = new PacketOut(0xCC);
                Out.WriteByte(0x6C);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(3);
                Out.WriteByte(0);
                Out.WriteByte((byte)Members.Count);
                foreach (Player Plr in Members)
                {
                    //Out.WriteBitPack(Plr.CharacterId);
                    Out.WriteUInt32(Plr.CharacterId);
                    Out.WriteByte(Plr.warbandSubgroup);
                    switch (Plr.warbandSubgroup)
                    {
                        case 1: Out.WriteByte(group1++); break;
                        case 2: Out.WriteByte(group2++); break;
                        case 3: Out.WriteByte(group3++); break;
                        case 4: Out.WriteByte(group4++); break;
                    }
                    Out.WriteByte(0x30);
                    //Out.WriteByte(Plr.PctHealth);
                    Out.WriteByte(Plr.PctAp);
                    Out.WriteByte(0);
                }
                SendToGroup(Out);
            }
            */
        }

        /// <summary> Called when the leader disconnects.</summary>
        public void ExitUpdate(long tick)
        {
            if (_warbandSlave)
            {
                _warbandHandler.ExitUpdate(tick);
                return;
            }

            lock (_groupActions)
                ProcessGroupActions();
        }

        #region GroupActions

        private readonly List<GroupAction> _groupActions = new List<GroupAction>();

        public static readonly List<KeyValuePair<uint, GroupAction>> _pendingGroupActions = new List<KeyValuePair<uint, GroupAction>>();

        public bool _groupStatusDirty, _groupCompositionDirty;

        public static void EnqueueGroupAction(uint GroupId, GroupAction action)
        {
            lock (_pendingGroupActions)
            {
                if (_pendingGroupActions != null)
                    _pendingGroupActions.Add(new KeyValuePair<uint, GroupAction>(GroupId, action));
            }
        }

        public void EnqueuePendingGroupAction(GroupAction action)
        {
            lock (_groupActions)
            {
                if (_warbandSlave)
                    _warbandHandler.EnqueueWarbandAction(action);
                else
                    _groupActions.Add(action);
            }

        }

        private void ProcessGroupActions()
        {
            List<GroupAction> groupActions = new List<GroupAction>();

            lock (_groupActions)
            {
                groupActions.AddRange(_groupActions);
                _groupActions.Clear();
            }

            foreach (GroupAction action in groupActions)
            {
                switch (action.Action)
                {
                    case EGroupAction.PlayerJoin:
                        if (!HasMaxMembers && !_warbandSlave && !Members.Contains(action.Instigator))
                            action.Instigator.SetGroup(this);
                        else
                            action.Instigator.GrpInterface.SetGroupState(EGroupJoinState.None);
                        break;
                    case EGroupAction.PlayerLeave:
                        if (!_warbandSlave)
                            RemoveMember(action.Instigator);
                        else // Leaving MUST be processed
                            _warbandHandler.RemoveMember(action.Instigator);
                        break;
                    case EGroupAction.PlayerKick:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        if (_warbandSlave)
                            break;
                        foreach (Player player in Members)
                            if (player.Name == action.ActionString)
                            {
                                RemoveMember(player);
                                goto End;
                            }
                        action.Instigator.SendLocalizeString(action.ActionString, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_TARGET_NOT_MEMBER);
                        break;
                    case EGroupAction.ChangeLeader:
                        if (_warbandSlave)
                            break;
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        foreach (Player player in Members)
                            if (player.Name == action.ActionString)
                            {
                                _changingLeader = true;
                                SetLeader(player);
                                goto End;
                            }

                        action.Instigator.SendLocalizeString(action.ActionString, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_TARGET_NOT_MEMBER);
                        break;
                    case EGroupAction.ChangeMainAssist:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        foreach (Player player in Members)
                            if (player.Name == action.ActionString)
                            {
                                SetMainAssist(player);
                                goto End;
                            }
                        action.Instigator.SendLocalizeString(action.ActionString, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_TARGET_NOT_MEMBER);
                        break;
                    case EGroupAction.ChangeMasterLooter:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        foreach (Player player in Members)
                            if (player.Name == action.ActionString)
                            {
                                SetMasterLooter(player);
                                goto End;
                            }
                        action.Instigator.SendLocalizeString(action.ActionString, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_TARGET_NOT_MEMBER);
                        break;
                    case EGroupAction.ChangeAutoLoot:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        ToggleRvRAutoLoot();
                        break;
                    case EGroupAction.ChangeNeedOnUse:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        ToggleNeedOnUse();
                        break;
                    case EGroupAction.OpenParty:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        SetPartyOpenStatus(true);
                        break;
                    case EGroupAction.CloseParty:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        SetPartyOpenStatus(false);
                        break;
                    case EGroupAction.FormWarband:
                        if (_warbandSlave)
                            break;
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_GROUP_LEADER);
                            break;
                        }
                        FormWarband();
                        break;
                }

                End:
                continue;
            }
        }

        #endregion

        #region Group Loot

        private byte _lootId;
        private byte _nextLooter;
        private byte _lootOption = 1;
        private byte _lootThreshold = 2;
        private bool _needOnUse = true;
        private bool _rvrAutoLoot = true;

        private readonly List<LootRoll> _activeLootRolls = new List<LootRoll>();

        public void UpdateLootRolls(long tick)
        {
            if (_nextTick < tick && _activeLootRolls.Count > 0)
            {
                foreach (LootRoll Lr in _activeLootRolls)
                {
                    if (Lr.GetVoteCount() >= Members.Count || Lr.StartTime + 61000 < tick)
                        Lr.GetWinner(this);
                }

                _activeLootRolls.RemoveAll(lootRollers => lootRollers.Completed);

                _nextTick = tick + 1000;
            }
        }

        public void GroupLoot(Player looter, LootContainer lootContainer)
        {
            for (byte i = 0; i < lootContainer.LootInfo.Count; ++i)
            {
                if (lootContainer.LootInfo[i] == null)
                    continue;
                if (lootContainer.LootInfo[i].Item.Rarity >= _lootThreshold)
                {
                    _activeLootRolls.Add(new LootRoll(++_lootId, lootContainer.LootInfo[i].Item));

                    PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE);
                    Out.WriteByte(0x07);
                    Out.WriteByte(0x10);
                    Out.WriteByte(0x6F);
                    Out.WriteByte(_lootId);
                    Out.WriteByte(0x00);
                    Item.BuildItem(ref Out, null, lootContainer.LootInfo[i].Item, null, 0, 0);
                    SendToGroup(Out, true);

                    lootContainer.LootInfo.RemoveAt(i);
                    --i;
                }
            }
        }

        public void LootVote(Player voter, ushort itemId, ushort vote)
        {
            foreach (LootRoll lr in _activeLootRolls)
            {
                if (lr.LootID == itemId)
                {
                    int result = lr.ProcessVote(voter, vote);

                    if (result != -1)
                    {
                        Localized_text t = Localized_text.TEXT_SELECTS_PASS_FOR;

                        switch (result)
                        {
                            case 0:
                                t = Localized_text.TEXT_SELECTS_NEED_FOR;
                                break;
                            case 1:
                                t = Localized_text.TEXT_SELECTS_GREED_FOR;
                                break;
                        }

                        string[] text = { voter.Name, lr.Item.Name };
                        foreach (var member in Members)
                        {
                            if (member != voter)
                                member.SendLocalizeString(text, ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL, t);
                        }
                    }
                }
            }
        }

        public Player GetGroupLooter(Player initialLooter)
        {
            //TODO Free for All Master Loot
            switch (_lootOption)
            {
                case 0:
                    if (_nextLooter < Members.Count - 1)
                        _nextLooter++;
                    else
                        _nextLooter = 0;
                    return Members[_nextLooter];
                case 1:
                    return initialLooter;
                case 2:
                    return _masterLooter;
            }
            return null;
        }

        private void SetLootOption(byte val)
        {
            // 0 round robin
            // 1 free for all
            // 2 masterloot

            _lootOption = val;
            _groupStatusDirty = true;
        }

        private void ToggleNeedOnUse()
        {
            //OnGroupCompositionChanged();
            _needOnUse = !_needOnUse;
            _groupStatusDirty = true;
        }

        private void ToggleRvRAutoLoot()
        {
            _rvrAutoLoot = !_rvrAutoLoot;

            _groupStatusDirty = true;
        }

        public void AssignGroupInfo(WarbandHandler wb)
        {
            wb.LootOption = _lootOption;
            wb.LootThreshold = _lootThreshold;
            wb.NeedOnUse = _needOnUse;
            wb.RvRAutoLoot = _rvrAutoLoot;
            wb.MainAssist = _mainAssist;
        }

        #endregion

        #region Composition Change Events

        public void SendGroupComposition()
        {

            int disabled = WorldMgr.WorldSettingsMgr.GetGenericSetting(17);

            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
            Out.WriteByte(0x06); // Group info
            Out.WriteByte(2);
            Out.WriteVarUInt(GroupId);
            Out.WriteByte((byte)Members.Count);
            foreach (Player plr in Members)
            {
                Out.WriteVarUInt(plr.CharacterId);
                Out.WriteByte(0x0F); // ?
                Out.WriteByte(plr.Info.ModelId);
                Out.WriteByte(plr.Info.Race);
                Out.WriteByte(plr.Level);
                Out.WriteByte(plr.StsInterface.BolsterLevel); // Real level? shows ^ - x = this byte
                Out.WriteByte(plr.Info.CareerLine);
                Out.WriteByte((byte)plr.Realm);
                Out.WriteByte((byte)(plr == Leader ? 1 : 0)); // Will be 1 for at least one member. Perhaps Leader?
                Out.WriteByte(0);
                Out.WriteByte(1); // Online = 1, Offline = 0
                Out.WriteByte((byte)plr.Name.Length);
                Out.Fill(0, 3);
                Out.WriteStringBytes(plr.Name);
                Out.WriteByte((byte)plr.GldInterface.GetGuildName().Length);
                Out.Fill(0, 3);
                Out.WriteStringBytes(plr.GldInterface.GetGuildName());
                if (disabled == 1)
                {
                    Out.WriteVarUInt((uint)plr.X);
                    Out.WriteVarUInt((uint)plr.Y);
                    Out.WriteVarUInt((uint)plr.Z);
                }
                else
                {

                    Out.WriteVarUInt(0);
                    Out.WriteVarUInt(0);
                    Out.WriteVarUInt(0);
                }

                byte[] data = { 0x27, 0x25, 0x05, 0x40, 0x00, 0x00 };
                byte[] data2 = { 0x27, 0x25, 0x05, 0x40, 0x01, 0x02 };

                // FIXME: Best guess right now is this data is actually 4 bytes + 2 varints.
                //        Most likely it has something to do with state that is only relevant
                //        when the player is online, because it is part of a block of data
                //        (plr.X through plr.PctMorale) that only gets sent when online=1.
                if (disabled == 1)
                    Out.Write(data2);
                else
                    Out.Write(data);

                if (disabled == 1)
                    Out.WriteZigZag(plr._Value.ZoneId);
                else
                    Out.WriteVarUInt(0);


                Out.WriteByte(1);

                Out.WriteByte(plr.PctHealth);
                Out.WriteByte(plr.PctAp); // action points
                Out.WriteByte(plr.PctMorale);
            }

            foreach (Player player in Members)
            {
                if (player.ScenarioGroup == null)
                    player.SendCopy(Out);
            }
            //Only send this info once.
            //SendGroupOidList();

            //Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
            //Out.WriteByte(0x06); // Group info
            //Out.WriteUInt16(0x0001);
            //Out.WriteUInt16(0x0000);
            //Out.WriteUInt32(Leader.CharacterId);
            //Out.WriteByte((byte)Members.Count);
            //foreach (Player Plr in Members)
            //{
            //    Out.WriteByte(Plr.Level);
            //    Out.WriteByte(Plr.StsInterface.BolsterLevel);
            //    Out.WriteByte(0x1);
            //    Out.WriteByte(Plr.PctHealth);
            //    Out.WriteByte(Plr.PctAp);
            //    Out.WriteByte(0x00);
            //    Out.WriteByte(0x00);
            //    Out.WriteByte(0x00);
            //    Out.WriteByte(Plr.Info.ModelId);
            //    Out.WriteUInt16(Plr.Oid);
            //    Out.WriteUInt32(Plr.CharacterId);

            //    if (Plr.GetPet() != null)
            //    {
            //        Out.WriteUInt16(Plr.GetPet().Oid);
            //        Out.WriteByte(Plr.GetPet().PctHealth);
            //    }
            //    else
            //    {
            //        Out.WriteUInt16(0x0000);
            //        Out.WriteByte(0x00);
            //    }

            //    Out.WriteUInt16(0x1000);
            //    Out.WritePascalString(Plr.Name);
            //    Out.WriteByte(0); // Packed
            //    Out.WriteByte(Plr.Info.CareerLine);
            //}

            //foreach (Player player in Members)
            //{
            //    if (player.ScenarioGroup == null)
            //        player.SendCopy(Out);
            //}

            _groupStatusDirty = true;
        }

        public void SendGroupOidListToMember(Player member)
        {

            if (Members.Count > 6)
                return;

            int OutOfZone = 0;

            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
            Out.WriteByte(6);
            Out.WriteByte(4);
            Out.WriteVarUInt(GroupId);

            foreach (Player plr in Members)
            {
                if (plr == null || plr.Region != member.Region)
                {
                    OutOfZone++;
                    continue;
                }
                Out.WriteVarUInt(plr.CharacterId);
                Out.WriteVarUInt(plr.Oid);
                Out.WriteByte(0);
            }
            // For any unused spots in the party, just write a 0.
            for (int j = (Members.Count - OutOfZone); j < 6; j++)
                Out.WriteByte(0);

            member.SendPacket(Out);
        }

        #endregion

        #region Member Management

        public List<Player> Members = new List<Player>();

        public bool IsEmpty => Members.Count == 0;
        public bool HasMaxMembers => Members.Count == 6;
        public bool RejectsMembers => _warbandHandler?.IsFull ?? Members.Count == 6;

        private readonly ReaderWriterLockSlim _memberRWLock = new ReaderWriterLockSlim();

        public bool AddMember(Player plr, bool broadcast = true)
        {

            _memberRWLock.EnterWriteLock();
            if (HasMaxMembers)
            {
                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_PARTY_IS_FULL);
                plr.GrpInterface.SetGroupState(EGroupJoinState.None);
                _memberRWLock.ExitWriteLock();
                return false;
            }

            if (_groupQueuedFor.Count > 0)
            {
                lock (_groupQueuedFor)
                {
                    if (_groupQueuedFor.Count > 0)
                        LatentDequeueAll();
                }
            }

            Members.Add(plr);

            plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, _warbandSlave ? Localized_text.TEXT_BG_YOU_WERE_ADDED : Localized_text.TEXT_YOU_JOIN_PARTY);

            if (plr != Leader && Leader != null)
                plr.SendLocalizeString(Leader.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_NEW_PARTY_LEADER);

            foreach (Player grpPlr in Members)
                grpPlr?.SendLocalizeString(plr.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_PLAYER_JOIN_PARTY);

            if (broadcast)
            {
                _groupCompositionDirty = true;
                _groupStatusDirty = true;
            }

            plr.GrpInterface.SetGroupState(EGroupJoinState.Grouped);

            _memberRWLock.ExitWriteLock();
            return true;

        }

        public int TotalMemberCount => _warbandSlave ? _warbandHandler.MemberCount : Members.Count;
        public int MemberCount => Members.Count;

        #region Scenario Queue Handling

        public bool CanQueueFor(ushort scenarioId)
        {
            lock (_groupQueuedFor)
            {
                if (_groupQueuedFor.Contains(scenarioId))
                    return false;

                _groupQueuedFor.Add(scenarioId);
                return true;
            }
        }

        public bool IsQueuedForAny()
        {
            lock (_groupQueuedFor)
                return _groupQueuedFor.Count > 0;
        }

        public bool CanDequeue(ushort scenarioId)
        {
            lock (_groupQueuedFor)
            {
                if (!_groupQueuedFor.Contains(scenarioId))
                    return false;

                _groupQueuedFor.Remove(scenarioId);
                return true;
            }
        }

        /// <summary>
        /// <para>Removes a group from all scenario queues.</para>
        /// </summary>
        public void LatentDequeueAll()
        {
            lock (_groupQueuedFor)
            {
                foreach (var scenarioId in _groupQueuedFor)
                    WorldMgr.ScenarioMgr.DequeueGroup(this, scenarioId);
            }
        }

        /// <summary>
        /// <para>Removes a group from all scenario queues.</para>
        /// <para>Only to be called from the ScenarioManager's thread.</para>
        /// </summary>
        public void DirectDequeueAll()
        {
            lock (_groupQueuedFor)
            {
                List<ushort> queuedFor = new List<ushort>(_groupQueuedFor);
                foreach (var scenarioId in queuedFor)
                    WorldMgr.ScenarioMgr.RemoveGroup(this, scenarioId);
            }
        }

        #endregion

        public List<Player> GetPlayerList()
        {
            return Members;
        }

        public List<Player> GetPlayerListCopy(Player toExclude = null)
        {
            if (toExclude == null)
            {
                _memberRWLock.EnterReadLock();
                try
                {
                    return new List<Player>(Members);
                }
                finally
                {
                    _memberRWLock.ExitReadLock();
                }
            }

            List<Player> members = new List<Player>();
            _memberRWLock.EnterReadLock();
            try
            {
                foreach (Player player in Members)
                    if (player != toExclude)
                        members.Add(player);
                return members;
            }
            finally
            {
                _memberRWLock.ExitReadLock();
            }
        }

        public void GetPlayerList(List<Player> list)
        {
            _memberRWLock.EnterReadLock();
            try { list.AddRange(Members); }
            finally { _memberRWLock.ExitReadLock(); }
        }

        public HashSet<Player> GetPlayerSet()
        {
            _memberRWLock.EnterReadLock();
            try
            {
                return new HashSet<Player>(Members);
            }
            finally { _memberRWLock.ExitReadLock(); }
        }

        public List<Player> GetPlayersCloseTo(Unit target, int maxDist)
        {
            List<Player> playerList = new List<Player>();
            _memberRWLock.EnterReadLock();
            try
            {
                foreach (Player player in Members)
                {
                    if (player == target || player.ObjectWithinRadiusFeet(target, maxDist))
                        playerList.Add(player);
                }
            }
            finally { _memberRWLock.ExitReadLock(); }

            return playerList;
        }

        public int GetPlayerCountWithinDist(Unit target, int maxDist)
        {
            int count = 0;

            _memberRWLock.EnterReadLock();
            try
            {
                foreach (Player player in Members)
                {
                    if (player != target && player.ObjectWithinRadiusFeet(target, maxDist))
                        ++count;
                }
            }
            finally { _memberRWLock.ExitReadLock(); }

            return count;
        }

        public List<Unit> GetUnitList(Player plr)
        {
            List<Unit> memberUnits = new List<Unit>();

            _memberRWLock.EnterReadLock();
            try { memberUnits.AddRange(Members); }
            finally { _memberRWLock.ExitReadLock(); }

            foreach (Player player in Members)
            {
                Unit pet = player.CrrInterface.GetTargetOfInterest() as Pet;

                if (pet != null)
                    memberUnits.Add(pet);
            }

            return memberUnits;
        }

        /// <summary>
        /// Gets a member's object if it is in tge given zone.
        /// </summary>
        /// <param name="zoneId">Zone identifier to search member in</param>
        /// <param name="clientId">Supposed client identifier</param>
        /// <returns>Player world object or null if not found</returns>
        /// <remarks>The returned player is always in a legal state</remarks>
        public Player GetMember(ushort zoneId, ushort clientId)
        {
            _memberRWLock.EnterReadLock();
            try
            {
                int i = 0;

                for (; i < Members.Count; ++i)
                {
                    Player player = Members[i];
                    if (player != null && player.Client != null && player.Client.Id == clientId
                        && player.Zone != null && player.Zone.ZoneId == zoneId)
                        return Members[i];
                }
                return null;
            }
            finally { _memberRWLock.ExitReadLock(); }
        }

        public bool HasMember(Unit unit)
        {
            if (unit is Pet)
                return false;

            int i = 0;

            _memberRWLock.EnterReadLock();
            try
            {
                for (; i < Members.Count; ++i)
                {
                    if (Members[i] == unit)
                        return true;
                }
                return false;
            }
            finally { _memberRWLock.ExitReadLock(); }
        }

        public bool HasUnit(Unit unit)
        {
            if (unit is Pet)
                unit = ((Pet)unit).Owner;

            _memberRWLock.EnterReadLock();
            try
            {
                return Members.Contains(unit);
            }
            finally { _memberRWLock.ExitReadLock(); }
        }

        public Player GetLeader()
        {
            if (_warbandHandler != null)
                return _warbandHandler.Leader;
            return Leader;
        }

        private void SetLeader(Player player)
        {
            if (player == null)
                return;

            if (_warbandHandler != null)
            {
                _warbandHandler.SetLeader(player);
                return;
            }

            _leader = player;
            Realm = player.Realm;
            player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_YOU_NOW_PARTY_LEADER);

            _groupCompositionDirty = true;
        }

        private void SetMainAssist(Player newMain)
        {
            _mainAssist = newMain;
            _groupCompositionDirty = true;

            foreach (Player player in Members)
                player.SendLocalizeString(newMain.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_MAIN_ASSIST_SET);
        }

        private void SetMasterLooter(Player player)
        {
            _masterLooter = player;
            _groupCompositionDirty = true;
        }

        private void SetPartyOpenStatus(bool value)
        {
            PartyOpen = value;
            Leader.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, PartyOpen ? Localized_text.TEXT_OPARTY_OPEN_ON : Localized_text.TEXT_OPARTY_OPEN_OFF);

            PacketOut Out = new PacketOut((byte)Opcodes.F_GROUP_STATUS);
            Out.WriteUInt16(GroupId);
            Out.WriteByte(0x02);
            Out.WriteByte(0x00);
            Out.WriteByte((byte)(PartyOpen ? 1 : 0));
            Out.Fill(0, 3);

            foreach (Player player in Members)
            {
                if (player.ScenarioGroup == null)
                    player.SendCopy(Out);
            }
        }

        public bool RemoveMember(Player player, bool swap = false)
        {
            if (!Members.Contains(player))
                return false;

            if (_groupQueuedFor.Count > 0)
            {
                lock (_groupQueuedFor)
                {
                    if (_groupQueuedFor.Count > 0)
                        LatentDequeueAll();
                }
            }

            _memberRWLock.EnterWriteLock();

            Members.Remove(player);

            _memberRWLock.ExitWriteLock();

            player.BuffInterface.RemoveGroupBuffsNotFrom(player.ScenarioGroup?.GetPlayerSet());

            foreach (Player member in Members)
            {
                if (member.ScenarioGroup == null || member.ScenarioGroup != player.ScenarioGroup)
                    member.BuffInterface.RemoveGroupBuffsFrom(player);
            }

            if (swap)
                return true;

            player.WorldGroup = null;
            SendExitingGroup(player);

            player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, _warbandSlave ? Localized_text.TEXT_BG_YOU_LEFT : Localized_text.TEXT_YOU_LEFT_PARTY);
            player.EvtInterface.Notify(EventName.OnLeaveGroup, null, null);

            foreach (Player grpPlr in Members)
                grpPlr.SendLocalizeString(player.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_X_LEFT_PARTY);

            player.GrpInterface.SetGroupState(EGroupJoinState.None);

            if (!_warbandSlave && Members.Count < 2)
            {
                SendMemberRemoved(player.CharacterId);
                DeleteGroup();
                return true;
            }

            if (player == _leader)
                SetLeader(Members.First());

            SendMemberRemoved(player.CharacterId);

            _groupCompositionDirty = true;

            return true;
        }

        private void DeleteGroup()
        {
            foreach (Player plr in Members)
            {
                if (plr == null)
                    continue;

                plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_YOU_LEFT_PARTY);

                SendExitingGroup(plr);
                if (plr.WorldGroup == this)
                    plr.WorldGroup = null;
                else if (plr.ScenarioGroup == this)
                    plr.ScenarioGroup = null;

                if (plr == _leader)
                    _leader = null;

                plr.GrpInterface.SetGroupState(EGroupJoinState.None);

                plr.BuffInterface.RemoveGroupBuffsNotFrom(plr.ScenarioGroup?.GetPlayerSet());
            }

            if (_groupQueuedFor.Count > 0)
            {
                lock (_groupQueuedFor)
                {
                    if (_groupQueuedFor.Count > 0)
                        LatentDequeueAll();
                }
            }

            _memberRWLock.EnterWriteLock();
            Members.Clear();
            _memberRWLock.ExitWriteLock();
            RemoveFromWorldGroups();
        }

        public void RemoveFromWorldGroups()
        {
            GroupLockStatic.EnterWriteLock();
            try
            {
                lock (Group.WorldGroups)
                {
                    WorldGroups.Remove(this);
                }
            }
            finally
            {
                GroupLockStatic.ExitWriteLock();
            }
        }

        public void NotifyMemberLoaded()
        {
            if (_warbandHandler != null)
                _warbandHandler.NotifyMemberLoaded();
            else
                _groupCompositionDirty = true;
        }

        #endregion

        #region Senders

        const byte TYPE_OPTIONS = 1;
        const byte TYPE_OPEN_STATUS = 2;

        public void SendGroupStatus()
        {
            /*
            // Leader?
            PacketOut Out = new PacketOut((byte)Opcodes.F_GROUP_STATUS);
            Out.Fill(0, 4);
            Out.WriteByte(0x13);
            Out.WriteByte(1);
            Out.WriteBitPack(Leader.CharacterId);
            Out.WriteByte(0);

            foreach (Player player in Members)
            {
                if (player.ScenarioGroup == null)
                    player.SendCopy(Out);
            }
            */

            // Group options

            PacketOut Out = new PacketOut((byte)Opcodes.F_GROUP_STATUS);
            Out.WriteUInt16(GroupId);
            Out.WriteByte(TYPE_OPTIONS);
            Out.WriteByte(0);
            Out.WriteByte(_lootOption);
            Out.WriteByte(_lootThreshold);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte(_needOnUse ? (byte)1 : (byte)0);
            Out.WriteByte(_rvrAutoLoot ? (byte)1 : (byte)0);
            Out.WriteUInt32(IsScenarioGroup ? 0 : _mainAssist.CharacterId);
            Out.WriteUInt32(IsScenarioGroup ? 0 : Leader.CharacterId);

            foreach (Player player in Members)
            {
                if (player.ScenarioGroup == null)
                    player.SendCopy(Out);
            }


            // Party open closed Packet

            Out = new PacketOut((byte)Opcodes.F_GROUP_STATUS);
            Out.WriteUInt16(GroupId);
            Out.WriteByte(TYPE_OPEN_STATUS);
            Out.WriteByte(0);
            Out.WriteByte((byte)(PartyOpen ? 1 : 0));
            Out.Fill(0, 3);
            SendToGroup(Out);
        }

        public void SendMemberRemoved(uint charId)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
            Out.WriteByte(6);
            Out.WriteByte(1);
            Out.WriteVarUInt(GroupId);
            Out.WriteVarUInt(charId);

            SendToGroup(Out);
        }

        public void SendToGroup(PacketOut Out, bool shouldLock = false)
        {
            if (shouldLock)
            {
                _memberRWLock.EnterReadLock();
                try
                {
                    foreach (Player player in Members)
                        player.SendCopy(Out);
                }
                finally { _memberRWLock.ExitReadLock(); }
            }

            else
            {
                foreach (Player player in Members)
                    player.SendCopy(Out);
            }
        }

        public void SendExitingGroup(Player player)
        {
            if (player == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
            Out.WriteByte(6);
            Out.WriteByte(2);
            Out.WriteVarUInt(GroupId);
            Out.WriteByte(0);
            player.SendPacket(Out);
        }

        public static void SendNullGroup(Player player)
        {
            if (player == null)
                return;

            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
            Out.WriteByte(6);
            Out.WriteByte(2);
            Out.WriteVarUInt(0x8C11);
            Out.WriteByte(0);
            player.SendPacket(Out);
        }

        public static void SendNullScenarioGroup(Player player)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
            Out.WriteByte(6);
            Out.WriteUInt16(1);
            Out.WriteUInt16(0);
            Out.WriteUInt32(0x00212CDA);
            Out.WriteByte(0);

            player.SendPacket(Out);
        }

        public void SendMessageToGroup(Player sender, string text)
        {
            _memberRWLock.EnterReadLock();
            try
            {
                foreach (Player player in Members)
                    player.SendMessage(sender, text, IsScenarioGroup ? ChatLogFilters.CHATLOGFILTERS_SCENARIO_GROUPS : ChatLogFilters.CHATLOGFILTERS_GROUP);
            }
            finally { _memberRWLock.ExitReadLock(); }
        }

        public void SendMessageToWarband(Player sender, string text)
        {
            _warbandHandler?.SendMessageToWarband(sender, text);
        }

        public void PartyRoll(Player sender, string text)
        {
            string[] rollString = { sender.Name, StaticRandom.Instance.Next(1, 100).ToString() };

            sender.SendLocalizeString(rollString[1], ChatLogFilters.CHATLOGFILTERS_GROUP, Localized_text.TEXT_YOU_ROLL_NUMBER);
            foreach (Player plr in Members)
                plr.SendLocalizeString(rollString, ChatLogFilters.CHATLOGFILTERS_GROUP, Localized_text.TEXT_NAME_ROLLS_NUMBER);
        }

        #endregion

        #region Warband

        /// <summary>
        /// Indicates that this group forms part of a warband or scenario set, and so it will not run its own Update().
        /// </summary>
        private bool _warbandSlave;

        public void FormWarband()
        {
            if (_warbandHandler != null)
                return;

            _warbandSlave = true;
            _warbandHandler = new WarbandHandler(this);

            _leader = null;
        }

        #endregion

        #region Scenario Handling

        private readonly List<ushort> _groupQueuedFor = new List<ushort>();

        public bool AddScenarioMember(Player player)
        {
            if (Members.Contains(player))
            {
                return false;
            }
            if (HasMaxMembers)
            {
                player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_PARTY_IS_FULL);
                return false;
            }
            _memberRWLock.EnterWriteLock();
            Members.Add(player);
            _memberRWLock.ExitWriteLock();

            player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SCENARIO_GROUP_CHAT_INSTRUCTIONS);

            if (player.WorldGroup != null)
                player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SCENARIO_GROUP_INVITE_WARNING);

            foreach (Player grpPlr in Members)
            {
                if (grpPlr != player)
                    grpPlr?.SendLocalizeString(player.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_PLAYER_JOIN_PARTY);
            }
            return true;
        }

        public bool RemoveScenarioMember(Player player)
        {
            if (!Members.Contains(player))
            {
                return false;
            }
            _memberRWLock.EnterWriteLock();
            Members.Remove(player);
            _memberRWLock.ExitWriteLock();
            player.BuffInterface.RemoveGroupBuffsNotFrom(player.WorldGroup?.GetPlayerSet());

            player.ScenarioGroup = null;
            player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_YOU_LEFT_PARTY);
            player.EvtInterface.Notify(EventName.OnLeaveGroup, null, null);

            foreach (Player member in Members)
            {
                if (member.WorldGroup == null || member.WorldGroup != player.WorldGroup)
                    member.BuffInterface.RemoveGroupBuffsFrom(player);
            }

            SendNullScenarioGroup(player);

            return true;
        }

        public void ClearScenarioGroup()
        {
            while (Members.Count > 0)
                RemoveScenarioMember(Members.First());
        }

        public PacketOut BuildCompositionChangedPacket()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
            Out.WriteByte(0x06); // Group info
            Out.WriteByte(0);
            Out.WriteByte(1);
            Out.WriteUInt16(0x0000);

            uint leader = Members.Count > 0 ? Members[0].CharacterId : 0;
            Out.WriteUInt32(leader); // Leader
            Out.WriteByte((byte)Members.Count);

            foreach (Player plr in Members)
            {
                Out.WriteByte(plr.Level);
                Out.WriteByte(plr.StsInterface.BolsterLevel);
                Out.WriteByte(0x1);
                Out.WriteByte(plr.PctHealth);
                Out.WriteByte(plr.PctAp);
                Out.WriteByte(plr.MoraleLevel);
                Out.WriteByte(0x00);
                Out.WriteByte(0x00);
                Out.WriteByte(plr.Info.ModelId);
                Out.WriteUInt16(plr.Oid);
                Out.WriteUInt32(plr.CharacterId);

                if (plr.GetPet() != null)
                {
                    Out.WriteUInt16(plr.GetPet().Oid);
                    Out.WriteByte(plr.GetPet().PctHealth);
                }
                else
                {
                    Out.WriteUInt16(0x0000);
                    Out.WriteByte(0x00);
                }
                Out.WriteUInt16(0x0000);
                Out.WritePascalString(plr.Name);
                Out.WriteByte(0); // Packed
                Out.WriteByte(plr.Info.CareerLine);
            }

            return Out;
        }

        #endregion

        #region Reward Distribution

        private const int MAX_SHARE_DIST = 300;

        public void AddXpFromKill(Player killer, Unit victim, float bonusMod)
        {
            List<Player> members = GetPlayersCloseTo(killer, MAX_SHARE_DIST);
            double lvlSum = 0;
            double xpQuotient = 0;
            long curTime = TCPManager.GetTimeStampMS();

            foreach (var mbr in members)
            {
                lvlSum += mbr.Level;
            }

            _logger.Trace($"Killer : {killer.Name} Victim : {victim.Name} Bonus : {bonusMod} Members : {members.Count} lvlSum : {lvlSum}");

            if (members.Count == 0)
            {
                killer.AddXp((uint)(WorldMgr.GenerateXPCount(killer, victim) * bonusMod), true, true);
                return;
            }

            foreach (Player plr in members)
            {
                _logger.Trace($"Player : {plr.Name} Victim Level : {victim.Level} CurTime : {curTime} DeathTime : {plr.deathTime}");

                //if ((plr.Level > victim.Level + 10 && !victim.IsPlayer()) || (plr.IsDead && curTime - plr.deathTime > 120000)) // player can no longer gain xp when dead for longer than 2 min
                //{
                //    return;
                //}
                //else
                //{
                    xpQuotient = (plr.Level / (lvlSum / 100)) / 100;
                    _logger.Trace($"xpQuotient : {xpQuotient}");

                    plr.AddXp((uint)((WorldMgr.GenerateXPCount(plr, victim) * bonusMod) * xpQuotient), true, true);
                //}
            }
        }

        public void AddXpCount(Player killer, uint xpCount)
        {
            List<Player> members = GetPlayersCloseTo(killer, MAX_SHARE_DIST);
            double lvlSum = 0;
            double xpQuotient = 0;
            long curTime = TCPManager.GetTimeStampMS();

            foreach (var mbr in members)
            {
                lvlSum += mbr.Level;
            }

            if (members.Count == 0)
            {
                killer.AddXp(xpCount, true, true);
                return;
            }

            foreach (Player player in members)
                if (!player.IsDead || (player.IsDead && curTime - player.deathTime < 120000))
                {
                    xpQuotient = (player.Level / (lvlSum / 100)) / 100;
                    player.AddXp((uint)(xpCount * xpQuotient), true, true);
                }
        }

        public void AddInfluenceCount(Player killer, ushort chapter, ushort value)
        {
            List<Player> members = GetPlayersCloseTo(killer, MAX_SHARE_DIST);
            long curTime = TCPManager.GetTimeStampMS();

            if (members.Count == 0)
            {
                killer.AddInfluence(chapter, value);
                return;
            }

            foreach (Player plr in members)
                if (!plr.IsDead || (plr.IsDead && curTime - plr.deathTime < 120000))
                {
                    plr.AddInfluence(chapter, (ushort)(value / members.Count));
                }
        }

        //public void AddRenownFromKill(Player killer, Player victim, float bonusMod)
        //{
        //    List<Player> members = GetPlayersCloseTo(killer, MAX_SHARE_DIST);
        //    long curTime = TCPManager.GetTimeStampMS();

        //    if (members.Count == 0)
        //    {
        //        killer.AddKillRenown((uint)(WorldMgr.GenerateRenownCount(killer, victim) * bonusMod), killer, victim);
        //        return;
        //    }

        //    _logger.Trace($"Killer : {killer.Name} Victim : {victim.Name} bonus : {bonusMod} members : {members.Count}");

        //    foreach (Player plr in members)
        //        if (!plr.IsDead || (plr.IsDead && curTime - plr.deathTime < 120000))
        //        {
        //            _logger.Trace($"Group RP : Player : {plr.Name} Victim {victim.Name} members: {members.Count} ");
        //            plr.AddKillRenown((uint)((WorldMgr.GenerateRenownCount(plr, victim) * bonusMod) / members.Count), killer, victim);
        //        }
        //}

        //public void AddPendingRenown(Player contributor, uint rCount)
        //{
        //    List<Player> members = GetPlayersCloseTo(contributor, MAX_SHARE_DIST);

        //    if (members.Count == 0)
        //    {
        //        contributor.AddRenown(rCount, false);
        //        return;
        //    }

        //    foreach (Player plr in members)
        //        plr.AddRenown((uint)(rCount / members.Count), false);
        //}

        public void AddMoney(Player looter, uint money)
        {
            List<Player> members = GetPlayersCloseTo(looter, MAX_SHARE_DIST);

            if (members == null || members.Count == 0)
            {
                if (looter.GldInterface.IsInGuild())
                    looter.GldInterface.ApplyTaxTithe(ref money);

                looter.AddMoney(money);
                return;
            }

            foreach (Player plr in members)
            {
                if (plr.GldInterface.IsInGuild())
                    plr.GldInterface.ApplyTaxTithe(ref money);
                plr.AddMoney((uint)(money / members.Count));
            }
        }

        public void HandleKillRewards(Unit victim, Player killer, float bonusMod, uint xp, uint renown, ushort influenceId, ushort influence, float transferenceFactor, CampaignObjective closestFlag)
        {
            List<Player> members = GetPlayersCloseTo(victim, MAX_SHARE_DIST);

            Player playerVictim = victim as Player;

            foreach (Player curPlayer in members)
            {
                uint xpShare = (uint)(xp / members.Count);
                uint renownShare = (uint)(renown / members.Count);
                try
                {
                    RewardLogger.Debug($"Cur player {curPlayer.Name} XP Share {xpShare} RP Share {renownShare}");

                    if (curPlayer.ScnInterface.Scenario == null || !curPlayer.ScnInterface.Scenario.DeferKillReward(curPlayer, xpShare, renownShare))
                    {
                        RewardLogger.Trace($"Add Xp {xpShare} to {curPlayer.Name}");
                        curPlayer.AddXp(xpShare, bonusMod, true, true);
                        if (playerVictim != null)
                        {
                            RewardLogger.Trace($"Add Kill Renown (Victim is a player)");
                            curPlayer.AddKillRenown(renownShare, bonusMod, killer, playerVictim, members.Count());
                        }
                        else
                        {
                            RewardLogger.Trace($"Player not victim");
                            curPlayer.AddRenown(renownShare, bonusMod, true);
                        }
                    }
                    if (influenceId != 0)
					{
						// ZARU: multiplied infl with 3
						curPlayer.AddInfluence(influenceId, (ushort)((influence * 3) / members.Count));
					}

                    if (closestFlag != null && closestFlag.State != StateFlags.ZoneLocked)
                    {
                        if (playerVictim != null)
                            closestFlag.RewardManager.AddDelayedRewardsFrom(curPlayer, playerVictim, (uint)(xpShare * transferenceFactor), (uint)(renownShare * transferenceFactor));

                        RewardLogger.Trace($"Adding contribution to Campaign: {curPlayer.Name} ");
                        curPlayer.Region.Campaign.AddContribution(curPlayer, (uint)(renownShare * bonusMod));
                    }
                    RewardLogger.Trace($"Level Check. Current player : {curPlayer.EffectiveLevel} Victim : {victim.EffectiveLevel}");
                    // Prevent farming low levels for kill quests, and also stop throttled kills
                    if (playerVictim != null)
                    {
                        if (curPlayer.EffectiveLevel <= victim.EffectiveLevel + 10)
                            curPlayer.QtsInterface.HandleEvent(Objective_Type.QUEST_KILL_PLAYERS, playerVictim.Info.CareerLine, 1, true);

                        curPlayer.EvtInterface.Notify(EventName.OnKill, killer, null);
                        curPlayer._Value.RVRKills++;
                        curPlayer.SendRVRStats();
                    }
                }
                catch (Exception e)
                {
                    RewardLogger.Error($"Exception : {e.Message} {e.StackTrace}");
                    throw;
                }
            }
        }

        #endregion

        private void BuildOpenPartyInfo(Player player, PacketOut Out)
        {
            if (_warbandSlave)
                _warbandHandler.BuildOpenPartyInfo(player, Out);

            else
            {
                Out.WriteUInt16(0);
                Out.WritePascalString(Leader.Name);
                Out.WriteByte(0);
                Out.WriteByte(1); /*2- warband*/
                Out.WriteUInt16(0);

                //Out.WriteByte(0); // 03: PQ - 02: RVR - 01: PVE - 00: Any - 04: SCE - 05: DUN
                if (Leader.ScnInterface.Scenario != null)
                    Out.WriteByte(4);
                else if (PlayerInRvR(Leader))
                    Out.WriteByte(2);
                else if (Leader.QtsInterface.PublicQuest != null && Leader.QtsInterface.PublicQuest.ObjectWithinRadiusFeet(Leader, 1000))
                    Out.WriteByte(3);
                else
                    Out.WriteByte(1);

                Out.WriteByte(Leader.Level);
                Out.WriteUInt16(Leader.Info.CareerLine);
                Out.WriteUInt16(Leader.Zone.ZoneId);
                Out.WriteByte(0);

                //PQ name
                if (Leader.QtsInterface.PublicQuest != null)
                    Out.WritePascalString(Leader.QtsInterface.PublicQuest.Name);
                else
                    Out.WriteByte(0);

                if (HasMember(player))
                    Out.WriteUInt16(0);
                else if (player.Zone.Region.RegionId != Leader.Zone.Region.RegionId)
                    Out.WriteUInt16(350); //if leader is in different region, give it 6min
                else
                    Out.WriteUInt16((ushort)(player.GetDistanceToObject(Leader) / 10));


                Out.WriteByte(8);
                Out.WriteUInt64(0x0000003D524000); //group data

                List<Player> members = GetPlayerListCopy();

                Out.WriteByte((byte)members.Count);
                Out.WriteByte((byte)members.Count);

                foreach (Player member in GetPlayerListCopy())
                {
                    Out.WriteByte(0);
                    Out.WritePascalString(member.Name);
                    Out.WriteByte(member.Level);
                    Out.WriteByte(0);
                    Out.WriteUInt16(member.Info.CareerLine);
                    Out.WriteByte(0); // sometimes 3
                }
            }
        }
    }
}

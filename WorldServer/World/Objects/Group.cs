using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SystemData;
using Common;
using FrameWork;
using GameData;
using NLog;
using WorldServer.Managers;
using WorldServer.World.Battlefronts.Apocalypse;
using WorldServer.World.Battlefronts.Objectives;
using WorldServer.World.Interfaces;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
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

            // on lockout automatically pass
            try
            {
                if (voter.Zone != null && (voter.Zone.Info.Type == 4 || voter.Zone.Info.Type == 5 || voter.Zone.Info.Type == 6))
                {
                    if (WorldMgr.InstanceMgr.HasLockoutFromCurrentBoss(voter))
                    {
                        _playersPassing.Add(voter);
                        voter.SendClientMessage("You've got already lockout on loot from this boss.", ChatLogFilters.CHATLOGFILTERS_LOOT_ROLL);
                        return 2;
                    }
                }
            }
            catch (Exception e) { Log.Error("Exception", e.Message + "\r\n" + e.StackTrace); }
            
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
        private int _zoneType = -1;

        private readonly List<LootRoll> _activeLootRolls = new List<LootRoll>();

        public void UpdateLootRolls(long tick)
        {
            if (_nextTick < tick && _activeLootRolls.Count > 0)
            {
                long startTime = _activeLootRolls.FirstOrDefault().StartTime;
                foreach (LootRoll Lr in _activeLootRolls)
                {
                    if (Lr.GetVoteCount() >= Members.Count || Lr.StartTime + 61000 < tick)
                        Lr.GetWinner(this);
                }

                _activeLootRolls.RemoveAll(lootRollers => lootRollers.Completed);

                // apply lockout
                try
                {
                    if ((_activeLootRolls.Count == 0 || startTime + 61000 < tick) && _zoneType != -1 && (_zoneType == 4 || _zoneType == 5 || _zoneType == 6))
                    {
                        List<Player> subGroup = Members.Where(x => !string.IsNullOrEmpty(x.InstanceID) && x.InstanceID == GetLeader().InstanceID).ToList();
                        WorldMgr.InstanceMgr.ApplyLockout(GetLeader().InstanceID, subGroup);
                    }
                }
                catch (Exception e) { Log.Error("Exception", e.Message + "\r\n" + e.StackTrace); }
                
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

		public void SubGroupLoot(Player looter, LootContainer lootContainer, List<Player> subGroup)
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
					SendToSubGroup(Out, subGroup, true);

                    lootContainer.LootInfo.RemoveAt(i);
                    --i;
                }
            }
        }

        public void LootVote(Player voter, ushort itemId, ushort vote)
        {
            if (voter.Zone != null && voter.Zone.Info != null)
                _zoneType = voter.Zone.Info.Type;

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

		public void SendToSubGroup(PacketOut Out, List<Player> subGroup, bool shouldLock = false)
		{
			if (shouldLock)
			{
				_memberRWLock.EnterReadLock();
				try
				{
					foreach (Player player in subGroup)
						player.SendCopy(Out);
				}
				finally { _memberRWLock.ExitReadLock(); }
			}

			else
			{
				foreach (Player player in subGroup)
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

        public void HandleKillRewards(Unit victim, Player killer, float bonusMod, uint xp, uint renown, ushort influenceId, ushort influence, float transferenceFactor, BattlefieldObjective closestFlag)
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

            if (Leader == null)
                _logger.Warn($"Leader is null for BuildOpenPartyInfo. Player {player}");

            if (WorldMgr.RVRArea.GetZoneRVRAreas() == null)
            {
                _logger.Warn($"GetZoneRVRAreas is null. Player {player}");
            }

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
                else if (RVRArea.IsPlayerInRvR(Leader, WorldMgr.RVRArea.GetZoneRVRAreas()))
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

        public Player SelectRandomPlayer()
        {
            try
            {
                var selected = StaticRandom.Instance.Next(MemberCount);
                return this.Members[selected];
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

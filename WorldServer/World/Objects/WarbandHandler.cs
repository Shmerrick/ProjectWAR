using System;
using System.CodeDom;
using FrameWork;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading;
using SystemData;
using GameData;
using WorldServer.World.Objects;

namespace WorldServer
{
    /// <summary>This class assumes control of 4 Groups, which are slaves to it, to implement Warband handling.</summary>
    public class WarbandHandler
    {
        protected int MaxGroups;

        protected Group[] Groups;
        protected readonly List<Player> AllMembers = new List<Player>();

        public bool IsFull => AllMembers.Count == 24;
        public bool IsEmpty => AllMembers.Count == 0;


        public uint ZeroIndexGroupId = 0;

        public Player Leader { get; private set; }
        public byte LootOption { get; set; }
        public byte LootThreshold { get; set; }
        public bool NeedOnUse { get; set; }
        public bool RvRAutoLoot { get; set; }
        public Player MainAssist { get; set; }
        public Player MasterLooter { get; set; }

        public readonly ReaderWriterLockSlim _updateRWLock = new ReaderWriterLockSlim();
        public bool PartyOpen
        {
            get { return Groups[0].PartyOpen; }
            set { Groups[0].PartyOpen = value; }
        }

        private readonly ReaderWriterLockSlim _membersRWLock = new ReaderWriterLockSlim();

        public WarbandHandler()
        {
            MaxGroups = 4;
            Groups = new Group[MaxGroups];
        }

        public WarbandHandler(Group initialGroup)
        {
            MaxGroups = 4;
            Groups = new Group[MaxGroups];

            Groups[0] = initialGroup;
            ZeroIndexGroupId = Groups[0].GroupId;
            Leader = Groups[0].Leader;
            Groups[0].AssignGroupInfo(this);

            for (int i = 1; i < MaxGroups; ++i)
                Groups[i] = new Group(this, false);

            _membersRWLock.EnterWriteLock();
            try
            {
                foreach (Player member in Groups[0].Members)
                    AllMembers.Add(member);
            }
            finally { _membersRWLock.ExitWriteLock(); }
        }

        private bool _changingLeader;

        public virtual void Update(long tick)
        {
            if (_changingLeader)
            {
                _changingLeader = false;
                return;
            }

            if (_warbandActions.Count > 0)
                ProcessWarbandActions();

            if (WarbandCompositionDirty)
            {
                SendWarbandComposition();
                foreach (Player m in AllMembers)
                {
                    try
                    {
                        SendPartyOidListsToMembers(m);
                    }
                    catch
                    {
                        continue;
                    }
                }
                WarbandCompositionDirty = false;
            }

            if (WarbandStatusDirty)
            {
                SendWarbandStatus();
                WarbandStatusDirty = false;
            }

            for (int i = 0; i < MaxGroups; ++i)
            {
                if (Groups[i] != null)
                    Groups[i].UpdateLootRolls(tick);
            }

            _changingLeader = false;
        }

        public void ExitUpdate(long tick)
        {
            lock (_warbandActions)
                ProcessWarbandActions();
        }

        #region GroupActions

        private readonly List<GroupAction> _warbandActions = new List<GroupAction>();

        public bool WarbandStatusDirty = true;
        public bool WarbandCompositionDirty = true;

        public void EnqueueWarbandAction(GroupAction action)
        {
            lock (_warbandActions)
                _warbandActions.Add(action);
        }

        private void ProcessWarbandActions()
        {
            List<GroupAction> warbandActions = new List<GroupAction>();

            lock (_warbandActions)
            {
                warbandActions.AddRange(_warbandActions);
                _warbandActions.Clear();
            }

            foreach (GroupAction action in warbandActions)
            {
                switch (action.Action)
                {
                    case EGroupAction.PlayerJoin:
                        if (!IsFull && !AllMembers.Contains(action.Instigator))
                            AddMember(action.Instigator);
                        else
                            action.Instigator.GrpInterface.SetGroupState(EGroupJoinState.None);
                        break;
                    case EGroupAction.PlayerLeave:
                        RemoveMember(action.Instigator);
                        break;
                    case EGroupAction.PlayerKick:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_ASSISTANT);
                            break;
                        }

                        foreach (Player player in AllMembers)
                            if (player.Name == action.ActionString)
                            {
                                RemoveMember(player);
                                goto End;
                            }
                        action.Instigator.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_PLAYER_NOT_IN_BG);
                        break;

                    case EGroupAction.ChangeLeader:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_GROUP_NOT_LEADER);
                            break;
                        }
                        foreach (Player player in AllMembers)
                            if (player.Name == action.ActionString)
                            {
                                _changingLeader = true;
                                SetLeader(player);
                                goto End;
                            }
                        action.Instigator.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_PLAYER_NOT_IN_BG);
                        break;
                    case EGroupAction.ChangeMainAssist:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        foreach (Player player in AllMembers)
                            if (player.Name == action.ActionString)
                            {
                                SetMainAssist(player);
                                goto End;
                            }
                        action.Instigator.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_PLAYER_NOT_IN_BG);
                        break;
                    case EGroupAction.ChangeLootOption:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        SetLootOption(Convert.ToByte(action.ActionString));
                        break;
                    case EGroupAction.ChangeMasterLooter:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        foreach (Player player in AllMembers)
                            if (player.Name == action.ActionString)
                            {
                                SetMasterLooter(player);
                                goto End;
                            }
                        action.Instigator.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_PLAYER_NOT_IN_BG);
                        break;
                    case EGroupAction.ChangeAutoLoot:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        ToggleRvRAutoLoot();
                        break;
                    case EGroupAction.ChangeNeedOnUse:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        ToggleNeedOnUse();
                        break;
                    case EGroupAction.OpenParty:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        SetPartyOpenStatus(true);
                        break;
                    case EGroupAction.CloseParty:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        SetPartyOpenStatus(false);
                        break;
                    case EGroupAction.WarbandMove:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        string name = action.ActionString.Substring(0, action.ActionString.Length - 2);
                        action.ActionString = action.ActionString.Remove(0, action.ActionString.Length - 2);
                        WarbandMove(name, Convert.ToByte(action.ActionString));
                        break;
                    case EGroupAction.WarbandSwap:
                        if (action.Instigator != Leader)
                        {
                            action.Instigator.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_NOT_LEADER);
                            break;
                        }
                        string[] vals = action.ActionString.Split(' ');
                        WarbandSwap(vals[0], vals[1]);
                        break;
                }

                End:
                continue;
            }
        }

        #endregion

        #region Member Management

        private void AddMember(Player player)
        {
            for (int i = 0; i < MaxGroups; ++i)
            {
                //To check for instabilities when joining an open warband from a different zone
                for (int c = 0; c < MaxGroups; ++c)
                {
                    if (Groups[c].Leader == null)
                        continue;

                    int disabled = WorldMgr.WorldSettingsMgr.GetGenericSetting(17);

                    if (player.Region == null
                    || Groups[c].Leader.Region == null
                    || player.Region != Groups[c].Leader.Region && disabled == 1)
                    {
                        player.SendClientMessage("Currently joining an open warband from a different region is disabled");
                        player.GrpInterface.SetGroupState(EGroupJoinState.None);
                        return;
                    }

                    /*
                   if (player.CurrentArea.ZoneId != Groups[c].Leader.CurrentArea.ZoneId && Groups[c].Leader != null)
                   {
                       if (player.CurrentArea.ZoneId == 161 || player.CurrentArea.ZoneId == 162)
                       {
                           player.SendClientMessage("Currently joining a warband from the capital is disabled");
                           player.GrpInterface.SetGroupState(EGroupJoinState.None);
                           return;
                       }

                   } */
                }
                if (Groups[i].HasMaxMembers)
                    continue;

                

                player.SetGroup(Groups[i]);

                _membersRWLock.EnterWriteLock();
                try { AllMembers.Add(player); }
                finally { _membersRWLock.ExitWriteLock(); }

                WarbandCompositionDirty = true;

                return;
            }

            player.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_BG_COULDNT_ADD_YOU);
            player.GrpInterface.SetGroupState(EGroupJoinState.None);
        }

        public void SetLeader(Player player)
        {
            if (player == null || player == Leader)
                return;

            if (!AllMembers.Contains(player))
            {
                Leader.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_PLAYER_NOT_IN_BG);
                return;
            }

            Leader.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_LEADER_DEMOTE);

            Leader = player;

            _membersRWLock.EnterWriteLock();
            try
            {
                AllMembers.Remove(Leader);
                AllMembers.Insert(0, Leader);
            }
            finally {  _membersRWLock.ExitWriteLock(); }

            player.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_LEADER_PROMOTE);

            WarbandCompositionDirty = true;
        }

        private void SetMainAssist(Player newMain)
        {
            MainAssist = newMain;
            WarbandStatusDirty = true;

            foreach (Player player in AllMembers)
                player.SendLocalizeString(newMain.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_MAIN_ASSIST_SET);
        }

        private void SetMasterLooter(Player player)
        {
            MasterLooter = player;
            WarbandStatusDirty = true;
        }

        private void SetPartyOpenStatus(bool value)
        {
            Groups[0].PartyOpen = value;
            Leader.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, PartyOpen ? Localized_text.TEXT_WARBAND_NOW_PUBLIC : Localized_text.TEXT_WARBAND_NOW_PRIVATE);

            PacketOut Out = new PacketOut((byte)Opcodes.F_GROUP_STATUS, 8);
            Out.WriteUInt16(Groups[0].GroupId);
            Out.WriteByte(0x02);
            Out.WriteByte(0x00);
            Out.WriteByte((byte)(PartyOpen ? 1 : 0));
            Out.Fill(0, 3);
            SendToWarband(Out);
        }

        public int MemberCount => AllMembers.Count;

        private void WarbandMove(string name, byte groupIndex)
        {
            groupIndex -= 1;

            if (groupIndex > 4)
            { 
                Leader.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_INVALID_MOVE_INDEX);
                return;
            }

            if (Groups[groupIndex].HasMaxMembers)
                return;

            for (int i = 0; i < MaxGroups; ++i)
            {
                foreach (Player plr in Groups[i].Members)
                {
                    if (plr.Name.Equals(name))
                    {
                        if (groupIndex == i)
                            return;

                        Groups[i].RemoveMember(plr, true);
                        plr.SetGroup(Groups[groupIndex]);
                        WarbandCompositionDirty = true;
                        return;
                    }
                }
            }
        }

        private void WarbandSwap(string name, string name2)
        {
            Player player = null;
            Player player2 = null;

            if (name == name2)
                return;

            foreach (Player plr in AllMembers)
            {
                if (plr.Name.Equals(name))
                {
                    player = plr;
                    if (player2 != null)
                        break;
                }
                if (plr.Name.Equals(name2))
                {
                    player2 = plr;
                    if (player != null)
                        break;
                }
            }

            if (player == null || player2 == null)
            {
                Leader.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_PLAYER_NOT_IN_BG);
                return;
            }

            Group group1 = player.WorldGroup;
            Group group2 = player2.WorldGroup;

            group1.RemoveMember(player, true);
            group2.RemoveMember(player2, true);

            player2.SetGroup(group1);
            player.SetGroup(group2);

            WarbandCompositionDirty = true;
        }

        public void RemoveMember(Player player)
        {
            for (int i = 0; i < MaxGroups; ++i)
            {
                if (Groups[i].IsEmpty || !Groups[i].RemoveMember(player))
                    continue;

                _membersRWLock.EnterWriteLock();
                try { AllMembers.Remove(player); }
                finally { _membersRWLock.ExitWriteLock(); }

                if (AllMembers.Count == 0)
                    Groups[0].RemoveFromWorldGroups();

                if (player == Leader && AllMembers.Count > 0)
                    SetLeader(AllMembers.First());

                WarbandCompositionDirty = true;

                return;
            }

            Leader.SendLocalizeString(ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_PLAYER_NOT_IN_BG);
        }

        private List<Player> GetPlayerListCopy()
        {
            _membersRWLock.EnterReadLock();
            try { return new List<Player>(AllMembers); }
            finally { _membersRWLock.ExitReadLock(); }
        }

        #endregion

        private void SetLootOption(byte val)
        {
            // 0 round robin
            // 1 free for all
            // 2 masterloot

            LootOption = val;
            WarbandStatusDirty = true;
        }

        private void ToggleNeedOnUse()
        {
            NeedOnUse = !NeedOnUse;
            WarbandStatusDirty = true;
        }

        private void ToggleRvRAutoLoot()
        {
            RvRAutoLoot = !RvRAutoLoot;

            WarbandStatusDirty = true;
        }

        public void NotifyMemberLoaded()
        {
            WarbandCompositionDirty = true;
        }

        #region Senders

        protected virtual void SendWarbandComposition()
        {
            #region Warband Update

            int disabled = WorldMgr.WorldSettingsMgr.GetGenericSetting(17);
            PacketOut Out = new PacketOut((byte) Opcodes.F_CHARACTER_INFO);
            Out.WriteByte(0x06); // Group info
            Out.WriteByte(3);
            Out.WriteByte(1); // Unknown

            for (int i = 0; i < MaxGroups; ++i)
            {
                if (Groups[i] == null || Groups[i].IsEmpty)
                    Out.WriteVarUInt(0); // Null party
                else
                {
                    Out.WriteVarUInt(Groups[i].GroupId);
                    Out.WriteByte((byte) Groups[i].Members.Count);

                    foreach (Player plr in Groups[i].Members)
                    {
                        Out.WriteVarUInt(plr.CharacterId);
                        Out.WriteByte(0x0F); // ?
                        Out.WriteByte(plr.Info.ModelId);
                        Out.WriteVarUInt(plr.Info.Race);
                        Out.WriteByte(plr.Level);
                        Out.WriteByte(plr.StsInterface.BolsterLevel); // Real level? shows ^ - x = this byte
                        Out.WriteByte(plr.Info.CareerLine);
                        Out.WriteByte((byte) plr.Realm);
                        Out.WriteByte((byte) (plr == Leader ? 1 : 0)); // Will be 1 for at least one member. Perhaps Leader?
                        Out.WriteByte(0);
                        Out.WriteByte(1); // Online = 1, Offline = 0
                        Out.WriteByte((byte) plr.Name.Length);
                        Out.Fill(0, 3);
                        Out.WriteStringBytes(plr.Name);
                        Out.WriteByte((byte) plr.GldInterface.GetGuildName().Length);
                        Out.Fill(0, 3);
                        Out.WriteStringBytes(plr.GldInterface.GetGuildName());
                        Out.WriteVarUInt((uint)plr.X);
                        Out.WriteVarUInt((uint)plr.Y);
                        Out.WriteVarUInt((uint)plr.Z);
                        byte[] data = { 0x27, 0x25, 0x05, 0x40, 0x00, 0x00 };
                        byte[] data2 = { 0x27, 0x25, 0x05, 0x40, 0x01, 0x02};

                        // FIXME: Best guess right now is this data is actually 4 bytes + 2 varints.
                        //        Most likely it has something to do with state that is only relevant
                        //        when the player is online, because it is part of a block of data
                        //        (plr.X through plr.PctMorale) that only gets sent when online=1.
                        Out.Write(data2);
                        Out.WriteZigZag(plr._Value.ZoneId);

                        Out.WriteByte(2);

                        Out.WriteByte(plr.PctHealth);
                        Out.WriteByte(plr.PctAp); // action points
                        Out.WriteByte(plr.PctMorale);
                    }
                }
            }

            SendToWarband(Out);

            WarbandStatusDirty = true;

            #endregion
        }
        protected virtual void SendPartyOidListsToMembers(Player member)
        {
            // This next block sends an F_CHARACTER_INFO that tells the game client which
            // ObjectIDs correspond to given CharacterIDs.  This is required for the warband
            // frames to update in response to F_PLAYER_STATE2 packets.
            for (int i = 0; i < MaxGroups; i++)
            {

                if (Groups[i] != null && !Groups[i].IsEmpty)
                {
                    // Send an F_CHARACTER_INFO (BE 06 04 version) with
                    // this party's PartyID, followed by a list of CharacterID
                    // and ObjectID for every member of the party.
                    PacketOut Out = new PacketOut((byte)Opcodes.F_CHARACTER_INFO);
                    Out.WriteByte(6);
                    Out.WriteByte(4);
                    Out.WriteVarUInt(Groups[i].GroupId);

                    foreach (Player plr in Groups[i].Members)
                    {

                        int OutOfZone = 0;
                        if (plr == null || plr.Region != member.Region)
                        {
                            OutOfZone++;
                            continue;
                        }
                        Out.WriteVarUInt(plr.CharacterId);
                        Out.WriteVarUInt(plr.Oid);
                        Out.WriteByte(0);
                        // For any unused spots in the party, just write a 0.
                        for (int j = Groups[i].Members.Count - OutOfZone; j < 6; j++)
                            Out.WriteByte(0);

                    }
                    member.SendPacket(Out);
                }
            }
        }
        protected virtual void SendWarbandStatus()
        {
            // Leader
            PacketOut Out = new PacketOut((byte)Opcodes.F_GROUP_STATUS, 18);
            Out.Fill(0, 4);
            Out.WriteByte(0x13);
            Out.WriteByte(1);
            Out.WriteVarUInt(Leader.CharacterId);
            Out.WriteByte(0);
            SendToWarband(Out);

            // Group options
            Out = new PacketOut((byte)Opcodes.F_GROUP_STATUS, 20);
            Out.WriteUInt16(Groups[0].GroupId);
            Out.WriteByte(0x01);      //1    
            Out.WriteByte(0x00);     // Setting FF hides the status of members.
            Out.WriteByte(LootOption);      // round robin = 0 free for all = 1 master loot = 2
            Out.WriteByte(LootThreshold);      //1   loottresh
            Out.WriteUInt32(MasterLooter?.CharacterId ?? 0); // Master Looter
            Out.WriteByte(NeedOnUse ? (byte)1 : (byte)0);      // need on use
            Out.WriteByte(RvRAutoLoot ? (byte)1 : (byte)0);      //2  rvr autoloot
            Out.WriteUInt32(MainAssist.CharacterId);
            Out.WriteUInt32(Leader.CharacterId);
            SendToWarband(Out);
        }

        protected void SendToWarband(PacketOut Out)
        {
            _membersRWLock.EnterReadLock();
            try
            {
                foreach (Player member in AllMembers)
                    member.SendCopy(Out);
            }
            finally {  _membersRWLock.ExitReadLock(); }

        }
        public virtual void SendMessageToWarband(Player sender, string text)
        {
            _membersRWLock.EnterReadLock();
            try
            {
                foreach (Player member in AllMembers)
                member.SendMessage(sender, text, ChatLogFilters.CHATLOGFILTERS_BATTLEGROUP);
            }
            finally { _membersRWLock.ExitReadLock(); }
        }

        #endregion

        public void BuildOpenPartyInfo(Player player, PacketOut Out)
        {
            Out.WriteUInt16(0);
            Out.WritePascalString(Leader.Name);
            Out.WriteByte(0);
            Out.WriteByte(2); /*2- warband*/
            Out.WriteUInt16(0);

            //Out.WriteByte(0); // 03: PQ - 02: RVR - 01: PVE - 00: Any - 04: SCE - 05: DUN
            if (Leader.ScnInterface.Scenario != null)
                Out.WriteByte(4);
            else if (RVRArea.IsPlayerInRvR(Leader, WorldMgr.RVRArea.GetZoneRVRAreas()))
                Out.WriteByte(2);
            else if (Leader.QtsInterface.PublicQuest!= null && Leader.QtsInterface.PublicQuest.ObjectWithinRadiusFeet(Leader, 1000))
                Out.WriteByte(3);
            else
                Out.WriteByte(1);

            Out.WriteByte(Leader.Level);
            Out.WriteUInt16(Leader.Info.CareerLine);
            Out.WriteUInt16(Leader.Zone.ZoneId);
            Out.WriteByte(0);

            //PQ name
            if (Leader.QtsInterface.PublicQuest!= null)
                Out.WritePascalString(Leader.QtsInterface.PublicQuest.Name);
            else
                Out.WriteByte(0);

            _membersRWLock.EnterReadLock();
            try
            {
                if (AllMembers.Contains(player))
                    Out.WriteUInt16(0);
                else if (player.Zone.Region.RegionId != Leader.Zone.Region.RegionId)
                    Out.WriteUInt16(350); //if leader is in different region, give it 6min
                else
                    Out.WriteUInt16((ushort)(player.GetDistanceToObject(Leader) / 10));
            }
            finally { _membersRWLock.ExitReadLock(); }

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

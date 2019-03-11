using System.Collections.Generic;
using SystemData;
using FrameWork;
using GameData;
using WorldServer.Managers;
using WorldServer.World.Objects;
using Object = WorldServer.World.Objects.Object;

namespace WorldServer.World.Interfaces
{
    public enum EGroupJoinState
    {
        None,
        Invited,
        PendingJoin,
        Grouped,
        Disposed
    }

    public class GroupInvitation
    {
        public Player InvitedPlayer { get; }
        public long ExpireTimeMs { get; }
        public int Reply;

        public GroupInvitation(Player inv, long expireTimeMs)
        {
            InvitedPlayer = inv;
            ExpireTimeMs = expireTimeMs;
        }
    }

    public class GroupInterface : BaseInterface
    {
        private Player _myPlayer;
        private EGroupJoinState _groupJoinState;
        private readonly object _groupSync = new object();

        public override bool Load()
        {
            _Owner.EvtInterface.AddEventNotify(EventName.Leave, OnPlayerLeave, true);
            _myPlayer = (Player) _Owner;
            return base.Load();
        }

        public override void Update(long tick)
        {
            if (_groupInvitations.Count > 0)
                UpdateGroupInvitations(tick);
        }

        #region Events

        /// <summary>Determines whether a player is permitted to join or be invited to a party.</summary>
        public bool AttemptGroupStateChange(EGroupJoinState newState)
        {
            lock (_groupSync)
            {
                // Disallow being invited or attempting to join an existing party if in group, inviting, invited or pending a join
                if (_groupJoinState != EGroupJoinState.None || _inviteCount > 0)
                    return false;

                _groupJoinState = newState;
                //_myPlayer.DebugMessage("New state: " + Enum.GetName(typeof(EGroupJoinState), (byte)_groupJoinState));
                return true;
            }
        }

        /// <summary>Direct setter, ignores the current state. For use within a particular action chain, but never to initiate such a chain.</summary>
        public void SetGroupState(EGroupJoinState newState)
        {
            lock (_groupSync)
            {
                _groupJoinState = newState;
                //_myPlayer.DebugMessage("New state: " + Enum.GetName(typeof(EGroupJoinState), (byte)_groupJoinState));
            }
        }

        private bool TryInviteLock()
        {
            // Allow a player to send invites only if not considering an invite to another party
            // or in the process of joining another party. Sending multiple concurrent invitations is permitted
            lock (_groupSync)
            {
                if (_groupJoinState == EGroupJoinState.Grouped || _groupJoinState == EGroupJoinState.None)
                {
                    ++_inviteCount;
                   // _myPlayer.SendClientMessage("Invite sent - count: "+_inviteCount);
                    return true;
                }
                return false;
            }
        }

        private void ReleaseInviteLock()
        {
            lock (_groupSync)
            {
                --_inviteCount;
               // _myPlayer.SendClientMessage("Invite removed - count: " + _inviteCount);
            }
        }

        #endregion

        #region Group Invitations

        private Player _invitedBy;

        private readonly List<GroupInvitation> _groupInvitations = new List<GroupInvitation>();
        private readonly List<GroupInvitation> _toProcess = new List<GroupInvitation>();

        private int _inviteCount;

        /// <summary> Attempts to invite the selected player to form a group.</summary>
        public bool TryInvite(Player invited)
        {
            if (invited == null || _myPlayer == null)
                return false;

            if (invited.IsBanned)
            {
                _myPlayer.SendClientMessage("You tried to invite "+invited.Name+" to your group, but your invitation was lost in the winds of Chaos.");
                return false;
            }

            if (_myPlayer.IsBanned)
            {
                _myPlayer.SendClientMessage("You called out to " + invited.Name + ", but they were unable to hear you.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return false;
            }

            //Disable cross zone invites to check if that causes WB crashing
            int disabled = WorldMgr.WorldSettingsMgr.GetGenericSetting(17);
            if (!_myPlayer.ZoneId.HasValue && disabled == 1)
            {
                _myPlayer.SendClientMessage("Group invites from someone in loading screen is disabled");
                invited.SendClientMessage(_myPlayer.Name + " tried to invite you, but he is now in loading screen so the invite was stopped");
                return false;
            }
            if (!invited.ZoneId.HasValue && disabled == 1)
            {
                _myPlayer.SendClientMessage("Group invites to someone in loading screen is disabled");
                invited.SendClientMessage(_myPlayer.Name + " tried to invite you, but you are in loading screen so the invite was stopped");
                return false;
            }
            if (invited.Region != _myPlayer.Region && disabled == 1)
            {
                _myPlayer.SendClientMessage("Group Invites across regions are currently disabled to investigate WB crashing");
                invited.SendClientMessage(_myPlayer.Name + " Tried to invite you from another zone, but cross zone invites are currently disabled");
                return false;
            }
            /*
            if (invited.CurrentArea.ZoneId != _myPlayer.CurrentArea.ZoneId)
            {
                if (invited.CurrentArea.ZoneId == 161 || invited.CurrentArea.ZoneId == 162)
                {
                    _myPlayer.SendClientMessage("Group invites to someone from the capital is currently disabled");
                    invited.SendClientMessage(_myPlayer.Name + " tried to invite you, but you are in the capital so the invite was stopped");
                    return false;
                }
            }
            */
            if (!TryInviteLock())
                return false;

            if (!invited.GrpInterface.AttemptGroupStateChange(EGroupJoinState.Invited))
            {
                ReleaseInviteLock();
                return false;
            }
            
            lock(_groupInvitations)
                _groupInvitations.Add(new GroupInvitation(invited, TCPManager.GetTimeStampMS() + 65000));

            invited.GrpInterface.NotifyInvitationReceived(_myPlayer, _myPlayer.WorldGroup != null && _myPlayer.WorldGroup.IsWarband);

            _myPlayer.SendLocalizeString(invited.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_GROUP_INVITE_BEGIN);

            return true;
        }

        private void UpdateGroupInvitations(long tick)
        {
            lock (_groupInvitations)
            {
                for (int i = 0; i < _groupInvitations.Count; ++i)
                {
                    if (_groupInvitations[i].ExpireTimeMs < tick)
                    {
                        _groupInvitations.RemoveAt(i);
                        --i;
                        continue;
                    }

                    if (_groupInvitations[i].Reply == 0)
                        continue;

                    _toProcess.Add(_groupInvitations[i]);
                    _groupInvitations.RemoveAt(i);
                    --i;
                }
            }

            if (_toProcess.Count <= 0)
                return;

            foreach (GroupInvitation gInv in _toProcess)
            {
                if (gInv.Reply == 1)
                    ProcessAccepted(gInv.InvitedPlayer);
                else
                    ProcessRejected(gInv.InvitedPlayer);
            }

            _toProcess.Clear();
        }

        public void NotifyInvitationReceived(Player invitingPlayer, bool isWarband)
        {
            _invitedBy = invitingPlayer;

            _myPlayer.SendLocalizeString(_invitedBy.Name, ChatLogFilters.CHATLOGFILTERS_SAY, isWarband? Localized_text.TEXT_BG_YOU_WERE_INVITED : Localized_text.TEXT_GROUP_YOU_WERE_INVITED);
            _myPlayer.SendDialog(isWarband ? Dialog.WarbandInvite : Dialog.PartyInvite, _invitedBy.Name);
        }

        public void AcceptInvitation()
        {
            lock (_groupSync)
            {
                if (_invitedBy != null)
                {
                    _invitedBy.GrpInterface.NotifyInvitationResponse(_myPlayer, 1);
                    _invitedBy = null;
                }
            }
        }
        public void RejectInvitation()
        {
            lock (_groupSync)
            {
                if (_invitedBy != null)
                {
                    _invitedBy.GrpInterface.NotifyInvitationResponse(_myPlayer, -1);
                    _invitedBy = null;
                }
            }
        }

        public void NotifyInvitationResponse(Player invited, int response)
        {
            lock (_groupInvitations)
                foreach (GroupInvitation gInv in _groupInvitations)
                {
                    if (gInv.InvitedPlayer == invited)
                    {
                        gInv.Reply = response;
                        break;
                    }
                }
        }

        private void ProcessAccepted(Player invited)
        {
            Group group;

            if (_myPlayer.WorldGroup != null)
            {
                group = _myPlayer.WorldGroup;

                _myPlayer.SendLocalizeString(invited.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_BG_ACCEPTED_YOUR_INVITE);

                if (group.RejectsMembers)
                {
                    invited.GrpInterface.SetGroupState(EGroupJoinState.None);
                    invited.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_PARTY_IS_FULL);
                }
                else
                {
                    invited.GrpInterface.SetGroupState(EGroupJoinState.PendingJoin);
                    Group worldGroup = group;
                    uint groupId = 0;
                    if (worldGroup != null)
                    {
                        lock (worldGroup)
                        {
                            if (worldGroup != null)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    lock (worldGroup._warbandHandler)
                                    {
                                        if (worldGroup._warbandHandler != null)
                                        {
                                            groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                        }
                                    }
                                }
                                else
                                {
                                    groupId = worldGroup.GroupId;
                                }
                            }
                        }
                    }
                    if (groupId != 0)
                        Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.PlayerJoin, invited));
                }
            }
            else
            {
                group = new Group();
                group.Initialize(_myPlayer, invited);
            }

            ReleaseInviteLock();
        }
        private void ProcessRejected(Player invited)
        {
            _myPlayer.SendLocalizeString(invited.Name, ChatLogFilters.CHATLOGFILTERS_USER_ERROR, Localized_text.TEXT_ERROR_INVITE_DECLINED);
            invited.GrpInterface.SetGroupState(EGroupJoinState.None);
            ReleaseInviteLock();
        }

        #endregion

        public bool OnPlayerLeave(Object sender, object args)
        {
            Player plr = _Owner as Player;

            if (plr == null)
                return false;

            lock (_groupInvitations)
            {
                foreach (GroupInvitation gInv in _groupInvitations)
                    ProcessRejected(gInv.InvitedPlayer);
                _groupInvitations.Clear();
            }
            uint groupId = 0;
            Group worldGroup = plr.WorldGroup;
            if (worldGroup != null)
            {
                lock (worldGroup)
                {
                    if (worldGroup != null)
                    {
                        if (worldGroup._warbandHandler != null)
                        {
                            lock (worldGroup._warbandHandler)
                            {
                                if (worldGroup._warbandHandler != null)
                                {
                                    groupId = worldGroup._warbandHandler.ZeroIndexGroupId;
                                }
                            }
                        }
                        else
                        {
                            groupId = worldGroup.GroupId;
                        }
                    }
                }
            }
            if (groupId != 0)
                Group.EnqueueGroupAction(groupId, new GroupAction(EGroupAction.PlayerLeave, plr));
            return true;
        }
       
    }
}
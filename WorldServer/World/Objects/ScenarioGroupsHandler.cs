using System.Collections.Generic;
using FrameWork;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Objects
{
    /// <summary>
    /// <para>The manager for the six scenario groups, as well as for the solo players therein.</para>
    /// <para>Unlike a regular warband, responsibility for updating this class lies with the scenario 
    /// that created it, and as the players within a scenario are on the same thread, this class is thread safe.</para>
    /// </summary>
    public class ScenarioGroupsHandler : WarbandHandler
    {
        private readonly List<Player> _soloists = new List<Player>();

        private bool[] _groupsDirty = new bool[6];

        public ScenarioGroupsHandler()
        {
            MaxGroups = 6;
            Groups = new Group[MaxGroups];

            for (int i = 0; i < MaxGroups; ++i)
                Groups[i] = new Group(this, true);
        }

        public override void Update(long tick)
        {
            if (WarbandCompositionDirty)
            {
                SendWarbandComposition();
                //SendPartyOidLists();
                WarbandCompositionDirty = false;
            }

            for (int i = 0; i < MaxGroups; ++i)
            {
                if (Groups[i] != null)
                    Groups[i].UpdateLootRolls(tick);

                if (_groupsDirty[i])
                {
                    SendScenarioGroupCompUpdate(i);
                    _groupsDirty[i] = false;
                }
            }
        }

        #region Member Management

        public void AddScenarioMember(Player player)
        {
            for (int i = 0; i < MaxGroups; ++i)
            {
                if (Groups[i].HasMaxMembers)
                    continue;

                player.SetScenarioGroup(Groups[i]);

                AllMembers.Add(player);

                WarbandCompositionDirty = true;

                _groupsDirty[i] = true;

                break;
            }
        }

        public void AddMemberToGroup(Player player, int groupIndex)
        {
            if (!AllMembers.Contains(player))
                return;

            if (!_soloists.Contains(player))
                return;

            if (Groups[groupIndex - 1].HasMaxMembers)
                return;

            _soloists.Remove(player);

            player.SetScenarioGroup(Groups[groupIndex-1]);

            WarbandCompositionDirty = true;

            _groupsDirty[groupIndex-1] = true;
        }

        public void RemoveMemberFromGroup(Player player)
        {
            if (!AllMembers.Contains(player))
                return;

            if (_soloists.Contains(player))
                return;

            for (int i = 0; i < MaxGroups; ++i)
            {
                if (Groups[i].RemoveScenarioMember(player))
                {
                    _soloists.Add(player);

                    WarbandCompositionDirty = true;

                    _groupsDirty[i] = true;

                    break;
                }
            }
        }

        public void RemoveScenarioMember(Player player)
        {
            if (!AllMembers.Contains(player))
                return;


            AllMembers.Remove(player);


            if (_soloists.Contains(player))
                _soloists.Remove(player);
            else
            {
                for (int i = 0; i < MaxGroups; ++i)
                {
                    if (Groups[i].RemoveScenarioMember(player))
                    {
                        _groupsDirty[i] = true;
                        break;
                    }
                }
            }

            WarbandCompositionDirty = true;
        }

        #endregion

        #region Senders
        protected override void SendWarbandComposition()
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_SCENARIO_PLAYER_INFO);
            Out.WriteByte(3);
            Out.WriteByte(0);
            Out.WriteByte(0);
            Out.WriteByte((byte)AllMembers.Count);
            for (int i = 0; i < MaxGroups; ++i)
            {
                for (byte j = 0; j < Groups[i].Members.Count; ++j)
                {
                    Player curMember = Groups[i].Members[j];

                    Out.WriteUInt32(curMember.CharacterId);
                    Out.WriteByte((byte)(i + 1));
                    Out.WriteByte((byte)(j + 1));
                    Out.WriteByte(curMember.PctHealth);
                    Out.WriteByte(curMember.PctAp);
                    Out.WriteByte(curMember.PctMorale);
                }
            }
            foreach (Player soloPlayer in _soloists)
            {
                Out.WriteUInt32(soloPlayer.CharacterId);
                Out.WriteByte(0);
                Out.WriteByte(0);
                Out.WriteByte(soloPlayer.PctHealth);
                Out.WriteByte(soloPlayer.PctAp);
                Out.WriteByte(0);
            }
            SendToWarband(Out);
        }

        public void SendScenarioGroupCompUpdate(int groupIndex)
        {
            PacketOut Out = Groups[groupIndex].BuildCompositionChangedPacket();

            foreach (Player member in Groups[groupIndex].Members)
                member.SendPacket(Out);
            //foreach (Player member in _soloists)
             //   member.SendPacket(Out);
        }

        protected override void SendPartyOidListsToMembers(Player member) { }
        protected override void SendWarbandStatus() { }
        public override void SendMessageToWarband(Player sender, string text) { }
        #endregion

        /// <summary>
        /// Determines whether there exists a partially filled group other than the one specified.
        /// </summary>
        public bool PartialGroupAvailable(Group exclude)
        {
            for (int i = 0; i < MaxGroups; ++i)
            {
                if (Groups[i] == exclude)
                    continue;

                if (!Groups[i].HasMaxMembers && !Groups[i].IsEmpty)
                    return true;
            }

            return false;
        }

        // Resolve all loot rolls.
        // Boot all players.
        public void NotifyScenarioClosed()
        {
            AllMembers.Clear();
            _soloists.Clear();

            for (int i = 0; i < MaxGroups; ++i)
                Groups[i].ClearScenarioGroup();
        }
    }
}
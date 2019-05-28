using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SystemData;
using Common;
using FrameWork;
using GameData;
using WorldServer.Services.World;
using WorldServer.World.Abilities.CareerInterfaces;
using WorldServer.World.Battlefronts.Bounty;
using WorldServer.World.Interfaces;
using WorldServer.World.Objects;
using Opcodes = WorldServer.NetWork.Opcodes;

namespace WorldServer.World.Scenarios
{
    enum ScenarioQueueType
    {
        Standard,
        Premade,
        Duo
    }
    enum EPendingQueueAction
    {
        PQA_Add,
        PQA_Remove,
        /// <summary>Unused</summary>
        PQA_RemoveAll
    }

    abstract class PendingQueueAction
    {
        public EPendingQueueAction Action { get; set; }
        public ushort ScenarioId { get; set; }
    }

    class PlayerQueueAction : PendingQueueAction
    {
        public Player MyPlayer { get; }

        public PlayerQueueAction(Player myPlayer, EPendingQueueAction action, ushort scenarioId)
        {
            MyPlayer = myPlayer;
            Action = action;
            ScenarioId = scenarioId;
        }
    }

    class GroupQueueAction : PendingQueueAction
    {
        public Group MyGroup { get; }

        public Vector2 BalanceVector { get; } = new Vector2();

        public float BalanceVectorMag;

        public List<Player> CurMembers { get; set; } 

        public GroupQueueAction(Group grp, EPendingQueueAction action, ushort scenarioId)
        {
            MyGroup = grp;
            Action = action;
            ScenarioId = scenarioId;
        }
    }

    /// <summary>
    /// Archetype priority when searching to fill a scenario with solo players.
    /// </summary>
    enum EArchetypePriority
    {
        Tanks,
        NoTanks,
        Dps,
        NoDps,
        Healers,
        NoHealers,
        None
    }

    class SoloQueueHandler
    {
        private const int TANK_QUEUE = 0;
        private const int DPS_QUEUE = 1;
        private const int HEALER_QUEUE = 2;

        public int TotalQueued => _archetypePlayerLists[0].Count + _archetypePlayerLists[1].Count + _archetypePlayerLists[2].Count;

        private readonly List<Player>[] _archetypePlayerLists = { new List<Player>(), new List<Player>(), new List<Player>() };

        public void ValidationHack()
        {
            for (int i = 0; i < 2; ++i)
            {
                for (int j = 0; j < _archetypePlayerLists[i].Count; ++j)
                {
                    if (!_archetypePlayerLists[i][j].ScnInterface.ValidForScenario())
                    {
                        _archetypePlayerLists[i].RemoveAt(j);
                        --j;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a player to current queues depending on his archetype.
        /// </summary>
        /// <param name="player">Player to add</param>
        public void AddPlayer(Player player)
        {
            EArchetype archetype = player.CrrInterface.GetArchetype();
            _archetypePlayerLists[(byte)archetype].Add(player);
        }

        private Player GetFromAndRemove(int queueNum)
        {
            Player plr = _archetypePlayerLists[queueNum].First();
            _archetypePlayerLists[queueNum].RemoveAt(0);
            //--TotalQueued;

            return plr;
        }

        /// <summary>Gets a player to fill in for the group queue, respecting balance if possible.</summary>
        public Player GetPlayer(Vector2 curBalance, bool forceBalance)
        {
            EArchetypePriority archetypePriority = EArchetypePriority.None;

            if (!curBalance.IsNullVector())
            {
                float bias = curBalance.CosineOfAngleWithUp();

                if (bias >= 0.866)
                    archetypePriority = EArchetypePriority.NoHealers;
                else if (bias <= -0.866)
                    archetypePriority = EArchetypePriority.Healers;
                else if (bias >= 0)
                    archetypePriority = curBalance.X < 0 ? EArchetypePriority.Dps : EArchetypePriority.Tanks;
                else
                    archetypePriority = curBalance.X < 0 ? EArchetypePriority.NoTanks : EArchetypePriority.NoDps;
            }

            // Fetch most desirable player
            Player fetchedPlayer = null;

            switch (archetypePriority)
            {
                case EArchetypePriority.None: // Prioritize utility classes when balanced due to general abundance of DPS
                    if (_archetypePlayerLists[HEALER_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(HEALER_QUEUE);
                    else if (_archetypePlayerLists[TANK_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(TANK_QUEUE);
                    else if (_archetypePlayerLists[DPS_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(DPS_QUEUE);
                    return fetchedPlayer;

                case EArchetypePriority.Healers:
                    if (_archetypePlayerLists[HEALER_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(HEALER_QUEUE);
                    else if (!forceBalance)
                    {
                        if (_archetypePlayerLists[TANK_QUEUE].Count != 0)
                            fetchedPlayer = GetFromAndRemove(TANK_QUEUE);
                        else if (_archetypePlayerLists[DPS_QUEUE].Count != 0)
                            fetchedPlayer = GetFromAndRemove(DPS_QUEUE);
                    }
                    return fetchedPlayer;

                case EArchetypePriority.NoDps: // Always prioritize healers if there are a ton of DPS
                    if (_archetypePlayerLists[HEALER_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(HEALER_QUEUE);
                    else if (_archetypePlayerLists[TANK_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(TANK_QUEUE);
                    else if (_archetypePlayerLists[DPS_QUEUE].Count != 0 && !forceBalance)
                        fetchedPlayer = GetFromAndRemove(DPS_QUEUE);
                    return fetchedPlayer;

                case EArchetypePriority.NoHealers:
                    if (_archetypePlayerLists[DPS_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(DPS_QUEUE);
                    else if (_archetypePlayerLists[TANK_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(TANK_QUEUE);
                    else if (_archetypePlayerLists[HEALER_QUEUE].Count != 0 && !forceBalance)
                        fetchedPlayer = GetFromAndRemove(HEALER_QUEUE);
                    return fetchedPlayer;

                case EArchetypePriority.Dps:
                    if (_archetypePlayerLists[DPS_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(DPS_QUEUE);
                    else if (!forceBalance)
                    {
                        if (_archetypePlayerLists[TANK_QUEUE].Count != 0)
                            fetchedPlayer = GetFromAndRemove(TANK_QUEUE);
                        else if (_archetypePlayerLists[HEALER_QUEUE].Count != 0)
                            fetchedPlayer = GetFromAndRemove(HEALER_QUEUE);
                    }
                    return fetchedPlayer;

                case EArchetypePriority.Tanks:
                    if (_archetypePlayerLists[TANK_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(TANK_QUEUE);
                    else if (!forceBalance)
                    {
                        if (_archetypePlayerLists[DPS_QUEUE].Count != 0)
                            fetchedPlayer = GetFromAndRemove(DPS_QUEUE);
                        else if (_archetypePlayerLists[HEALER_QUEUE].Count != 0)
                            fetchedPlayer = GetFromAndRemove(HEALER_QUEUE);
                    }
                    return fetchedPlayer;

                case EArchetypePriority.NoTanks:
                    if (_archetypePlayerLists[DPS_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(DPS_QUEUE);
                    else if (_archetypePlayerLists[HEALER_QUEUE].Count != 0)
                        fetchedPlayer = GetFromAndRemove(HEALER_QUEUE);
                    else if (_archetypePlayerLists[TANK_QUEUE].Count != 0 && !forceBalance)
                        fetchedPlayer = GetFromAndRemove(TANK_QUEUE);
                    return fetchedPlayer;
                default:
                    throw new InvalidOperationException("Impossible??");
            }
        }

        public bool RemovePlayer(Player player)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (_archetypePlayerLists[i].Remove(player))
                {
                    //--TotalQueued;
                    return true;
                }
            }

            return false;
        }

        public bool Contains(Player player)
        {
            for (int i = 0; i < 3; ++i)
            {
                if (_archetypePlayerLists[i].Contains(player))
                    return true;
            }

            return false;
        }

        public int GetMaxInitiallyFieldable()
        {
            int minCount = _archetypePlayerLists[0].Count;
            if (_archetypePlayerLists[1].Count < minCount)
                minCount = _archetypePlayerLists[1].Count;
            if (_archetypePlayerLists[2].Count < minCount)
                minCount = _archetypePlayerLists[2].Count;

            int maxFieldable = 0;

            for (int i = 0; i < 3; ++i)
                maxFieldable += _archetypePlayerLists[i].Count == minCount ? minCount : minCount + 1;

            return maxFieldable;
        }

        public List<Player> GetPlayers(int requiredSize)
        {
            byte currentStack = 0;

            List<Player> players = new List<Player>();

            while (requiredSize > 0)
            {
                if (_archetypePlayerLists[currentStack].Count > 0)
                {
                    players.Add(GetFromAndRemove(currentStack));
                    --requiredSize;
                }

                ++currentStack;

                if (currentStack == 3)
                    currentStack = 0;
            }

            return players;
        }

        public List<Player> GetAndRemoveAll()
        {
            List<Player> players = new List<Player>();
            for (int i = 0; i < 3; ++i)
            {
                players.AddRange(_archetypePlayerLists[i]);
                _archetypePlayerLists[i].Clear();
            }

            return players;
        } 
    }

    class GroupQueueHandler
    {
        // Order ascending (2 => 6)
        public readonly List<GroupQueueAction>[] QueuedGroups = new List<GroupQueueAction>[5];
        private readonly int[] _index = { -1, -1, -1, -1, -1};

        public bool HasGroups => QueuedGroups.Any(group => group.Count > 0);
        public bool HasFullGroups => QueuedGroups[4].Count > 0;

        public GroupQueueHandler()
        {
            for (int i = 0; i < 5; ++i)
                QueuedGroups[i] = new List<GroupQueueAction>();
        }

        /// <summary> Returns every possible combination of groups which results in a player total of, at most, maxSize.</summary>
        /// <returns>An array of lists of Groups, whose index represents the total number of players within that entry.</returns>
        public List<GroupQueueAction>[] GetCombinations(int maxSize, int minGroupSize, int maxGroupSize)
        {
            List<GroupQueueAction>[] groupCombos = new List<GroupQueueAction>[maxSize];
            int sum = 0;

            int minIndex = minGroupSize - 2; // i = 0
            int maxIndex = maxGroupSize - 1; //  i < 5

            // Tries every possible combination of groups.
            for (;;)
            {
                ++_index[minIndex];

                // Check if we are as far along in the current group list as we can go.
                // If so, and we have groups left to check, reset the current index and increment the index above.
                // If no groups remain for checking, return the found combinations.
                if (_index[minIndex] >= QueuedGroups[minIndex].Count)
                {
                    for (int i = minIndex; i < maxIndex; ++i)
                    {
                        if (_index[i] < QueuedGroups[i].Count)
                            break;

                        _index[i] = -1;

                        if (i + 1 == maxIndex || i == maxSize)
                        {
                            // The next index is non-existent or would escape our set bounds.
                            // Reset position indices and return all found groups.

                            for (int j = minIndex; j < maxIndex; ++j)
                                _index[j] = -1;

                            return groupCombos;
                        }

                        ++_index[i + 1];
                    }
                }

                // Sum up the number of players contained in this combination.
                for (int i = minIndex; i < maxIndex; ++i)
                {
                    if (_index[i] != -1)
                        sum += (_index[i] + 1) * (i + 2);
                }

                if (sum == 0)
                    continue;

                // Sum is too high, so skip to the end.
                if (sum > maxSize)
                {
                    _index[minIndex] = (byte)QueuedGroups[minIndex].Count;
                    sum = 0;
                    continue;
                }

                // Prefer including larger groups within a scenario.
                // As we progress up the list, we know we are including larger groups,
                // so we should overwrite.
                if (groupCombos[sum - 1] == null)
                    groupCombos[sum - 1] = new List<GroupQueueAction>();
                else groupCombos[sum - 1].Clear();

                for (int i = minIndex; i < maxIndex; ++i)
                {
                    if (_index[i] == -1)
                        continue;
                    for (int j = 0; j <= _index[i]; ++j)
                        groupCombos[sum - 1].Add(QueuedGroups[i][j]);
                }

                sum = 0;
            }
        }

        public GroupQueueAction GetFullGroup()
        {
            GroupQueueAction myAction = QueuedGroups[4].First();
            QueuedGroups[4].RemoveAt(0);
            return myAction;
        }

        public void AddGroup(GroupQueueAction newGroup)
        {
            List<GroupQueueAction> groupList = QueuedGroups[newGroup.CurMembers.Count - 2];

            for (int i = 0; i <= groupList.Count; ++i)
            {
                if (i == groupList.Count)
                {
                    groupList.Add(newGroup);
                    break;
                }

                // More balanced groups are prioritized for selection.
                if (newGroup.BalanceVectorMag < groupList[i].BalanceVectorMag)
                {
                    groupList.Insert(i, newGroup);
                    break;
                }
            }
        }

        public bool RemoveGroup(GroupQueueAction oldGroup)
        {
            bool removed = false;

            for (int i = 0; i < 5; ++i)
            {
                List<GroupQueueAction> curList = QueuedGroups[i];

                if (curList == null)
                    continue;

                int len = curList.Count;
                // Overpenetration because it bugged out before.
                for (int j = 0; j < len; ++j)
                {
                    if (curList[j].MyGroup == oldGroup.MyGroup)
                    {
                        if (oldGroup.CurMembers == null)
                            oldGroup.CurMembers = curList[j].CurMembers;
                        removed = true;
                        curList.RemoveAt(j);
                        --j;
                        --len;
                    }
                }
            }

            return removed;
        }

        public List<GroupQueueAction> GetAndRemoveAll()
        {
            List<GroupQueueAction> groups = new List<GroupQueueAction>();

            for (int i = 0; i < 5; ++i)
            {
                groups.AddRange(QueuedGroups[i]);
                QueuedGroups[i].Clear();
            }

            return groups;
        }
    }

    public class ScenarioMgr
    {
        /// <summary>Runnable instances of scenarios, indexed by info / tier number</summary>
        private readonly Dictionary<Scenario_Info, Dictionary<byte, List<Scenario>>> _instances = new Dictionary<Scenario_Info, Dictionary<byte, List<Scenario>>>();

        /// <summary>List of queued solo players, indexed by info / tier number / [0,1] for [order,destro]</summary>
        private readonly Dictionary<Scenario_Info, Dictionary<byte, SoloQueueHandler[]>> _queuedPlayers = new Dictionary<Scenario_Info, Dictionary<byte, SoloQueueHandler[]>>();
        /// <summary>List of queued groups, indexed by info / tier number / [0,1] for [order,destro]</summary>
        private readonly Dictionary<Scenario_Info, Dictionary<byte, GroupQueueHandler[]>> _queuedGroups = new Dictionary<Scenario_Info, Dictionary<byte, GroupQueueHandler[]>>();

        /// <summary>Pending queue actions mainly updated by Scenario TCP handler</summary>
        private readonly List<PendingQueueAction> _pendingQueueActions = new List<PendingQueueAction>();

        public static ImpactMatrixManager ImpactMatrixManagerInstance { get; set; }


        /// <summary>Flag to ask for delayed manager stop</summary>
        private bool Running = true;
        /// <summary>Dedicated event of the manager</summary>
        private readonly EventInterface _evtInterface;

        private const byte QUEUE_ASSEMBLY_NO_MATCH = 128;
        private const byte QUEUE_ASSEMBLY_SOLO_ONLY = 255;

        /// <summary>List of enabled scenarios (table scenario_objects)</summary>
        private static List<Scenario_Info> _activeScenarios;

        /// <summary>Current pickup scenario name, for user display on log in</summary>
        public static string PickupScenarioName;

        /// <summary>
        /// Builds and starts the scenario manager thread.
        /// </summary>
        /// <param name="activeScenarios">List of enabled scenarios (table scenario_objects)</param>
        public ScenarioMgr(List<Scenario_Info> activeScenarios)
        {
            ImpactMatrixManagerInstance = new ImpactMatrixManager();

            foreach (Scenario_Info scenario in ScenarioService.Scenarios)
            {
                _instances.Add(scenario, new Dictionary<byte, List<Scenario>>());
                _queuedPlayers.Add(scenario, new Dictionary<byte, SoloQueueHandler[]> ());
                _queuedGroups.Add(scenario, new Dictionary<byte, GroupQueueHandler[]>());

                for (byte i = 1; i <= 4; i++) // per tier
                {
                    _instances[scenario].Add(i, new List<Scenario>());
                    _queuedPlayers[scenario].Add(i, new [] { new SoloQueueHandler(), new SoloQueueHandler() });
                    _queuedGroups[scenario].Add(i, new[] { new GroupQueueHandler(), new GroupQueueHandler()});
                }

                if (scenario.QueueType == 2) // duo ??
                    PickupScenarioName = scenario.Name;
            }

            _activeScenarios = activeScenarios;

            _evtInterface = new EventInterface();
            new Thread(Update).Start();

            _evtInterface.AddEvent(QueueUpdate, 15000, 0);
        }

        /// <summary>Asks for a delayed stop of the manager</summary>
        public void Stop()
        {
            Running = false;
        }

        const int SCENARIO_UPDATE_INTERVAL = 100;

        #region Updating

        private long _lastUpdateTime;

        public int SecondsSinceUpdate => (int)((TCPManager.GetTimeStampMS() - _lastUpdateTime)/1000);

        public void Update()
        {
            while (Running)
            {
                long start = TCPManager.GetTimeStampMS();

                try
                {
                    ProcessPendingQueue();

                    // Fill / Create scenarios
                    _evtInterface.Update(start);

                    #region Remove ended scenarios

                    List<Scenario> closed = new List<Scenario>();

                    foreach (var key in _instances.Keys)
                    {
                        foreach (var key2 in _instances[key].Keys)
                        {
                            if (closed.Count > 0)
                                closed = new List<Scenario>();

                            foreach (Scenario scenario in _instances[key][key2])
                            {
                                if (scenario != null)
                                {
                                    if (scenario.IsClosed)
                                        closed.Add(scenario);
                                    else
                                        scenario.Update(start);
                                }
                            }

                            _instances[key][key2].RemoveAll(i => closed.Contains(i));
                        }
                    }

                    #endregion

                }
                catch (Exception e)
                {
                    Log.Error("ScenarioMgr", e.ToString());
                }

                /*
                if (Program.Rm.NextRotationTime < TCPManager.GetTimeStamp())
                {
                    RotateScenarios();
                    Program.Rm.NextRotationTime += 60*60*24*7;
                    Program.AcctMgr.UpdateRealmScenarioRotationTime(Program.Rm.RealmId, Program.Rm.NextRotationTime);
                }
                */

                long curTime = TCPManager.GetTimeStampMS();

                _lastUpdateTime = curTime;

                long elapsed = curTime - start;
                if (elapsed < SCENARIO_UPDATE_INTERVAL)
                    Thread.Sleep((int)(SCENARIO_UPDATE_INTERVAL - elapsed));
            }
        }

        /// <summary>
        /// Processes all pending queue requests to add/remove players/groups in this manager's internal lists.
        /// </summary>
        private void ProcessPendingQueue()
        {
            int pendingActionCount = 0;
            lock (_pendingQueueActions)
                pendingActionCount = _pendingQueueActions.Count;

            if (pendingActionCount == 0)
                return; // No pending request -> nothing to do
            
            PendingQueueAction[] myActions;
            lock (_pendingQueueActions)
            {
                myActions = _pendingQueueActions.ToArray();
                _pendingQueueActions.Clear();
            }

            foreach (PendingQueueAction action in myActions)
            {
                switch (action.Action)
                {
                    case EPendingQueueAction.PQA_Add:
                        if (action is GroupQueueAction)
                        {
                            try
                            {
                                AddGroup((GroupQueueAction) action);
                            }
                            catch
                            {
                                Log.Error("ScenarioMgr", "Exception thrown from AddGroup");
                                ((GroupQueueAction) action).MyGroup.LatentDequeueAll();
                            }
                        }
                        else AddPlayer((PlayerQueueAction) action);
                        break;
                    case EPendingQueueAction.PQA_Remove:
                        if (action is GroupQueueAction)
                        {
                            try
                            {
                                RemoveGroup((GroupQueueAction)action);
                            }
                            catch (Exception)
                            {
                                Log.Error("ScenarioMgr", "Exception thrown from RemoveGroup");
                            }
                        }
                                
                        else RemovePlayer((PlayerQueueAction) action);
                        break;
                }
            }
        }

        public void QueueUpdate()
        {
            Scenario_Info[] scenarioInfo = Randomize(_activeScenarios);

            foreach (Scenario_Info info in scenarioInfo)
            {
                foreach (byte tier in _instances[info].Keys)
                    TryFillScenario(info, tier);
            }

            foreach (Scenario_Info info in scenarioInfo)
            {
                foreach (byte tier in _instances[info].Keys)
                    TryCreateScenario(info, tier);
            }
        }

        #endregion

        private static T[] Randomize<T>(List<T> list)
        {
            T[] outArray = list.ToArray();

            int n = outArray.Length;

            while (n > 1)
            {
                int k = StaticRandom.Instance.Next(n--);
                T temp = outArray[n];
                outArray[n] = outArray[k];
                outArray[k] = temp;
            }

            return outArray;
        }

        #region Creation/Fill

        private void TryFillScenario(Scenario_Info info, byte tier)
        {
            ProcessPendingQueue(); // Why here ? already invoked in main Update() method

            #region Remove Invalid Player Entries
            // Hack to remove players in unexpected state (logged out, warband bug...)
            _queuedPlayers[info][tier][0].ValidationHack();
            _queuedPlayers[info][tier][1].ValidationHack();

            #endregion

            foreach (Scenario scenario in _instances[info][tier])
            {
                try
                {
                    if (scenario.HasEnded || scenario.IsFull())
                        continue;

                    if (_queuedPlayers[info][tier][0].TotalQueued == 0 && _queuedPlayers[info][tier][1].TotalQueued == 0 && !_queuedGroups[info][tier][0].HasGroups && !_queuedGroups[info][tier][1].HasGroups)
                        return;

                    switch ((ScenarioQueueType)info.QueueType)
                    {
                        case ScenarioQueueType.Standard:
                            if (!_queuedGroups[info][tier][0].HasGroups && !_queuedGroups[info][tier][1].HasGroups)
                                TryFillWithSolos(scenario, tier);
                            else
                                TryFillWithGroups(scenario, tier, 1, 6);
                            break;
                        case ScenarioQueueType.Premade:
                            if (scenario.IsPickup)
                                TryFillWithSolos(scenario, tier);
                            //else
                            //    TryFillWithGroups(scenario, tier, 6, 6);
                            break;
                        case ScenarioQueueType.Duo:
                            if (!_queuedGroups[info][tier][0].HasGroups && !_queuedGroups[info][tier][1].HasGroups)
                                TryFillWithSolos(scenario, tier);
                            else
                                TryFillWithGroups(scenario, tier, 1, 2);
                            break;
                    }
                }

                catch (IndexOutOfRangeException e)
                {
                    Log.Error("ScenarioMgr", "Failed to iterate over scenario array for " + info.Name + " on tier " + tier + " (" + e + ")");
                }
            }
        }

        private void TryCreateScenario(Scenario_Info info, byte tier)
        {
            ProcessPendingQueue(); // Why here ? already invoked in main Update() method

            #region Remove Invalid Player Entries
            // Hack to remove players in unexpected state (logged out, warband bug...)
            _queuedPlayers[info][tier][0].ValidationHack();
            _queuedPlayers[info][tier][1].ValidationHack();

            #endregion

            switch ((ScenarioQueueType)info.QueueType)
            {
                case ScenarioQueueType.Premade:
                    if (_queuedGroups[info][tier][0].HasFullGroups && _queuedGroups[info][tier][1].HasFullGroups)
                        TryCreate6V6Scenario(info, tier);
                    TryCreatePugScenario(info, tier, true);
                    break;

                case ScenarioQueueType.Standard:
                    if (!_queuedGroups[info][tier][0].HasGroups && !_queuedGroups[info][tier][1].HasGroups)
                        TryCreatePugScenario(info, tier, false);
                    else
                        TryCreateWithGroups(info, tier, 1, 6, false);
                    break;
                case ScenarioQueueType.Duo:
                    if (!_queuedGroups[info][tier][0].HasGroups && !_queuedGroups[info][tier][1].HasGroups)
                        TryCreatePugScenario(info, tier, true);
                    else
                    {
                        TryCreateWithGroups(info, tier, 1, 2, true);
                    }
                    break;
            }
        }

        private void TryFillWithSolos(Scenario scenario, byte tier)
        {
            Vector2[] balanceVectors = { new Vector2(scenario.BalanceVectors[0]), new Vector2(scenario.BalanceVectors[1]) };

            Scenario_Info info = scenario.Info;

            int[] counts = { scenario.GetTotalTeamCount(0), scenario.GetTotalTeamCount(1) };

            int disparity = Math.Abs(counts[0] - counts[1]);

            if (disparity > 0)
            {
                int smallerTeam = counts[0] < counts[1] ? 0 : 1;

                while (disparity > 0)
                {
                    Player toAdd = _queuedPlayers[info][tier][smallerTeam].GetPlayer(balanceVectors[smallerTeam], false);

                    if (toAdd == null)
                        return;

                    AddPlayerToScenarioTeam(scenario, toAdd);

                    AddToBalanceVector(balanceVectors[smallerTeam], toAdd);
                    --disparity;
                }

                if (scenario.IsFull())
                    return;
            }

            while (scenario.CanAcceptPair() && _queuedPlayers[info][tier][0].TotalQueued > 0 && _queuedPlayers[info][tier][1].TotalQueued > 0)
            {
                Player orderPlayer = _queuedPlayers[info][tier][0].GetPlayer(balanceVectors[0], true);
                Player destroPlayer = _queuedPlayers[info][tier][1].GetPlayer(balanceVectors[1], true);

                if (orderPlayer == null || destroPlayer == null)
                    break;

                AddPlayerToScenarioTeam(scenario, orderPlayer);
                AddToBalanceVector(balanceVectors[0], orderPlayer);
                AddPlayerToScenarioTeam(scenario, destroPlayer);
                AddToBalanceVector(balanceVectors[1], destroPlayer);
            }
        }

        private void TryCreatePugScenario(Scenario_Info info, byte tier, bool favoursPug)
        {

            if (!favoursPug && info.Name == PickupScenarioName)
                return;

            // Check that the number of players that each side can field (while still respecting 
            // 2/2/2 balance as well as is possible) is above the scenario's minimum player threshold.
            int orderPlayerCount = _queuedPlayers[info][tier][0].GetMaxInitiallyFieldable();

            int destroPlayerCount = _queuedPlayers[info][tier][1].GetMaxInitiallyFieldable();

            int desiredPlayerCount = Math.Min(orderPlayerCount, destroPlayerCount);

            // Dev server case
            if (info.MinPlayers == 0)
            {
                if (orderPlayerCount > 0 || destroPlayerCount > 0)
                {
                    Scenario devScenario = CreateInstance(info, tier);

                    AddPlayerToScenarioTeam(devScenario, orderPlayerCount > 0 ? _queuedPlayers[info][tier][0].GetPlayers(1) : _queuedPlayers[info][tier][1].GetPlayers(1));
                }
                return;
            }

            if (destroPlayerCount < info.MinPlayers)
                return;


            if (orderPlayerCount < info.MinPlayers)
                return;

          

           

            List<Player> orderPlayers = _queuedPlayers[info][tier][0].GetPlayers(desiredPlayerCount);
            List<Player> destroPlayers = _queuedPlayers[info][tier][1].GetPlayers(desiredPlayerCount);

            Scenario newScenario = CreateInstance(info, tier);
            newScenario.IsPickup = favoursPug;

            for (int i = 0; i < desiredPlayerCount; i++)
            {
                AddPlayerToScenarioTeam(newScenario, orderPlayers);
                AddPlayerToScenarioTeam(newScenario, destroPlayers);
            }
        }

        private void TryCreateWithGroups(Scenario_Info info, byte tier, int minUnit, int maxUnit, bool favoursPug)
        {
            // Return early if no groups exist to create with and solo fill is not permitted.
            if (minUnit > 1 && !_queuedGroups[info][tier][0].HasGroups && !_queuedGroups[info][tier][1].HasGroups)
                return;

            //Never create a PUG scenario as a normal match.
            if (!favoursPug && info.Name == PickupScenarioName)
                return;

            List<GroupQueueAction>[][] groupCombinations =
            {
                _queuedGroups[info][tier][0].GetCombinations(info.MaxPlayers, Math.Max(minUnit, 2), maxUnit),
                _queuedGroups[info][tier][1].GetCombinations(info.MaxPlayers, Math.Max(minUnit, 2), maxUnit)
            };

            // Holds the index of the slot which can be used (perhaps in combination with solo players) for index+1 players to be added.
            byte[][] combinationIndexFor =
            {
                new byte[info.MaxPlayers],
                new byte[info.MaxPlayers]
            };

            byte minPlayers = (byte)(info.MinPlayers == 0 ? info.MinPlayers : info.MinPlayers - 1);

            #region Go through the entries working out if, in combination with solo players, a given exact number of players can be added to the scenario.
            for (int team = 0; team < 2; ++team)
            {
                int lastAllGroupedMatchIndex = -1;

                for (byte playerCount = minPlayers; playerCount < info.MaxPlayers; ++playerCount)
                {
                    // Direct match, so use the field value. This becomes the last known index with a direct match.
                    if (groupCombinations[team][playerCount] != null)
                    {
                        lastAllGroupedMatchIndex = playerCount;
                        combinationIndexFor[team][playerCount] = (byte)lastAllGroupedMatchIndex;
                    }
                    // Otherwise, check if the number can be made up using the solo queue members, if the queue allows this.
                    else if (minUnit == 1 && lastAllGroupedMatchIndex + _queuedPlayers[info][tier][team].TotalQueued >= playerCount)
                    {
                        // No previous direct matches, so we should pull from the solo queue only.
                        // Else use the previous index - we'll check the value within against the count index later.
                        // If they don't match, we pull from the solo queue to make up the shortfall.
                        //The PUG scenario will never create a match where the PUG scenario is open as a non-PUG match.
                        if (lastAllGroupedMatchIndex < 1)
                            combinationIndexFor[team][playerCount] = QUEUE_ASSEMBLY_SOLO_ONLY;
                        else
                            combinationIndexFor[team][playerCount] = (byte)lastAllGroupedMatchIndex;
                    }
                    else combinationIndexFor[team][playerCount] = QUEUE_ASSEMBLY_NO_MATCH;
                }
            }

            #endregion

            #region Determine the maximum number of players which can be added at once while maintaining scenario balance.

            int smallTeam = 1;
            int bigTeam = 0;

            // The zero-based (i.e. zero = 1 player) index in the array of possible groups of players which should be selected 
            // to add players from. This may point to a location which doesn't have enough grouped players to satisfy the need, 
            // in which case solo players are used to make up the shortfall.
            uint[] playersNeeded = { QUEUE_ASSEMBLY_NO_MATCH, QUEUE_ASSEMBLY_NO_MATCH };

            for (uint i = (uint)info.MaxPlayers - 1; i >= minPlayers; --i)
            {
                // We have players in the smaller team available at this num.
                // Check to see if the larger team can field exactly this number
                // of players, or one less.
                try
                {
                    if (combinationIndexFor[smallTeam][i] != QUEUE_ASSEMBLY_NO_MATCH)
                    {
                        if (combinationIndexFor[bigTeam][i] != QUEUE_ASSEMBLY_NO_MATCH)
                        {
                            playersNeeded[bigTeam] = i;
                            playersNeeded[smallTeam] = i;
                            break;
                        }

                        if (i - 1 >= info.MinPlayers && i > 0 &&
                            combinationIndexFor[bigTeam][i - 1] != QUEUE_ASSEMBLY_NO_MATCH)
                        {
                            playersNeeded[bigTeam] = i - 1;
                            playersNeeded[smallTeam] = i;
                            break;
                        }

                        if (i + 1 < info.MaxPlayers &&
                            combinationIndexFor[bigTeam][i + 1] != QUEUE_ASSEMBLY_NO_MATCH)
                        {
                            playersNeeded[bigTeam] = i + 1;
                            playersNeeded[smallTeam] = i;
                            break;
                        }
                    }
                }
                catch
                {
                    playersNeeded[bigTeam] = QUEUE_ASSEMBLY_NO_MATCH;
                    playersNeeded[smallTeam] = QUEUE_ASSEMBLY_NO_MATCH;
                    break;
                }
            }
            #endregion


            if (playersNeeded[0] == QUEUE_ASSEMBLY_NO_MATCH || playersNeeded[1] == QUEUE_ASSEMBLY_NO_MATCH)
                return;

            StringBuilder sb = new StringBuilder(256, 512);

            sb.Append($"{info.Name} - {playersNeeded[0] + 1} vs {playersNeeded[1] + 1}");

            #region Create the scenario and add the players to it.

            Scenario newScenario = CreateInstance(info, tier);
            newScenario.IsPickup = favoursPug;

            List<Player>[] addedPlayers = { new List<Player>(), new List<Player>() };

            for (int teamIndex = 0; teamIndex < 2; ++teamIndex)
            {
                Vector2 curBalance = new Vector2();

                if (combinationIndexFor[teamIndex][playersNeeded[teamIndex]] != QUEUE_ASSEMBLY_SOLO_ONLY)
                {
                    foreach (GroupQueueAction groupQueueInfo in groupCombinations[teamIndex][combinationIndexFor[teamIndex][playersNeeded[teamIndex]]])
                    {
                        curBalance.Add(groupQueueInfo.BalanceVector);

                        groupQueueInfo.MyGroup.DirectDequeueAll();
                        foreach (Player member in groupQueueInfo.CurMembers)
                            addedPlayers[teamIndex].Add(member);
                    }

                    // This index wasn't completely full and needs to be supplemented with solo players.
                    if (combinationIndexFor[teamIndex][playersNeeded[teamIndex]] != playersNeeded[teamIndex])
                    {
                        for (int j = 0; j < playersNeeded[teamIndex] - combinationIndexFor[teamIndex][playersNeeded[teamIndex]]; ++j)
                        {
                            Player player = _queuedPlayers[info][tier][teamIndex].GetPlayer(curBalance, false);
                            AddToBalanceVector(curBalance, player);
                            addedPlayers[teamIndex].Add(player);
                        }
                    }
                }

                // Pull from the solo queue only, using as balanced a composition as is possible, but still pulling as many as required from the lists.
                else
                    addedPlayers[teamIndex].AddRange(_queuedPlayers[info][tier][teamIndex].GetPlayers((int)(playersNeeded[teamIndex] + 1)));

                /*
                #region Build scenario manager string.

                sb.Append((teamIndex == 0 ? "Order:" : "Destruction:") + "\n");

                for (int i = 0; i < addedPlayers[teamIndex].Count; ++i)
                {
                    if (i == 0)
                        sb.Append(addedPlayers[teamIndex][i].Name);
                    else sb.Append(", " + addedPlayers[teamIndex][i].Name);
                }

                if (teamIndex == 0)
                    sb.Append("\n");

                #endregion
                */
            }

            #endregion

            #region Notify players of the scenario teams.
            for (int i = 0; i < 2; ++i)
            {
                foreach (Player player in addedPlayers[i])
                {
                    player.SendMessage(0, "Scenario Manager", sb.ToString(), ChatLogFilters.CHATLOGFILTERS_SHOUT);
                    AddPlayerToScenarioTeam(newScenario, player);
                }
            }
            #endregion
        }

        private void TryFillWithGroups(Scenario scenario, byte tier, int minUnit, int maxUnit)
        {
            Scenario_Info info = scenario.Info;

            // Return early if no groups exist to fill with and solo fill is not permitted.
            if (minUnit > 1 && !_queuedGroups[info][tier][0].HasGroups && !_queuedGroups[info][tier][1].HasGroups)
                return;

            List<GroupQueueAction>[][] groupCombinations = new List<GroupQueueAction>[2][]
            {
                _queuedGroups[info][tier][0].GetCombinations(info.MaxPlayers, Math.Max(minUnit, 2), maxUnit),
                _queuedGroups[info][tier][1].GetCombinations(info.MaxPlayers, Math.Max(minUnit, 2), maxUnit)
            };

            // Free space left in the scenario teams.
            #warning Is sometimes negative o_O
            int[] teamSpace =
            {
                Math.Max(0, scenario.Info.MaxPlayers - scenario.GetTotalTeamCount(0)),
                Math.Max(0, scenario.Info.MaxPlayers - scenario.GetTotalTeamCount(1))
            };


            // Holds the index of the slot which can be used (perhaps in combination with solo players) for index+1 players to be added.
            byte[][] combinationIndexFor =
            {
                new byte[teamSpace[0]],
                new byte[teamSpace[1]]
            };

            uint deltaSpace = (uint)Math.Abs(teamSpace[0] - teamSpace[1]);

            // Go through the entries working out if, in combination with solo players, a given exact number of players can be added to the scenario.
            for (int team = 0; team < 2; ++team)
            {
                int lastFullCombinationIndex = -1;

                for (byte playerCount = 0; playerCount < teamSpace[team]; ++playerCount)
                {
                    // Direct match, so use the field value.
                    // This becomes the last known index with a direct match.
                    if (groupCombinations[team][playerCount] != null)
                    {
                        lastFullCombinationIndex = playerCount;
                        combinationIndexFor[team][playerCount] = (byte)lastFullCombinationIndex;
                    }
                    // Otherwise, check if the number can be made up using the solo queue members.
                    else if (minUnit == 1 && lastFullCombinationIndex + _queuedPlayers[info][tier][team].TotalQueued >= playerCount)
                    {
                        // No previous direct matches, so mark 255 to let it be known that we should pull from the solo queue only.
                        // Else use the previous index - we'll check the value within against the count index later.
                        // If they don't match, we pull from the solo queue to make up the shortfall.
                        if (lastFullCombinationIndex < 1)
                            combinationIndexFor[team][playerCount] = QUEUE_ASSEMBLY_SOLO_ONLY;
                        else
                            combinationIndexFor[team][playerCount] = (byte)lastFullCombinationIndex;
                    }
                    else combinationIndexFor[team][playerCount] = QUEUE_ASSEMBLY_NO_MATCH;
                }
            }

            int smallTeam = teamSpace[1] > teamSpace[0] ? 1 : 0;
            int bigTeam = teamSpace[1] > teamSpace[0] ? 0 : 1;

            // The zero-based (i.e. zero = 1 player) index in the array of possible groups of players which should be selected 
            // to add players from. This may point to a location which doesn't have enough grouped players to satisfy the need, 
            // in which case solo players are used to make up the shortfall.

            uint[] playersNeeded = { QUEUE_ASSEMBLY_NO_MATCH, QUEUE_ASSEMBLY_NO_MATCH };


            // Determine the maximum number of players which can be added at once while maintaining scenario balance.
            for (int i = teamSpace[smallTeam] - 1; i >= 0; --i)
            {
                // We have players in the smaller team available at this num.
                // Check to see if the larger team can field exactly this number
                // of players, or one less.
                if (combinationIndexFor[smallTeam][i] != QUEUE_ASSEMBLY_NO_MATCH)
                {
                    // If positive, means that we should still check against the bigger team to see if they can field players, too.
                    if (i - deltaSpace >= 0)
                    {
                        if (combinationIndexFor[bigTeam][i - deltaSpace] != QUEUE_ASSEMBLY_NO_MATCH)
                        {
                            playersNeeded[bigTeam] = (uint)(i - deltaSpace);
                            playersNeeded[smallTeam] = (uint)i;
                            break;
                        }

                        if (i - deltaSpace - 1 >= 0 &&
                            combinationIndexFor[bigTeam][i - deltaSpace - 1] != QUEUE_ASSEMBLY_NO_MATCH)
                        {
                            playersNeeded[bigTeam] = (uint)(i - deltaSpace - 1);
                            playersNeeded[smallTeam] = (uint)i;
                            break;
                        }
                    }

                    // We can add outright, because the large team has at 
                    // least as many players as we want to add.
                    else
                    {
                        playersNeeded[smallTeam] = (uint)i;
                        playersNeeded[bigTeam] = QUEUE_ASSEMBLY_NO_MATCH;
                        break;
                    }
                }
            }

            // Check the results.
            for (int teamIndex = 0; teamIndex < 2; ++teamIndex)
            {
                if (playersNeeded[teamIndex] == QUEUE_ASSEMBLY_NO_MATCH)
                    continue;

                Vector2 balanceVector = new Vector2(scenario.BalanceVectors[teamIndex]);

                if (combinationIndexFor[teamIndex][playersNeeded[teamIndex]] != QUEUE_ASSEMBLY_SOLO_ONLY)
                {
                    foreach (GroupQueueAction groupQueueInfo in groupCombinations[teamIndex][combinationIndexFor[teamIndex][playersNeeded[teamIndex]]])
                    {
                        balanceVector.Add(groupQueueInfo.BalanceVector);

                        groupQueueInfo.MyGroup.DirectDequeueAll();
                        foreach (Player member in groupQueueInfo.CurMembers)
                            AddPlayerToScenarioTeam(scenario, member);

                    }

                    // This index wasn't completely full and needs to be supplemented with solo players.
                    if (combinationIndexFor[teamIndex][playersNeeded[teamIndex]] != playersNeeded[teamIndex])
                    {
                        for (int j = 0; j < playersNeeded[teamIndex] - combinationIndexFor[teamIndex][playersNeeded[teamIndex]]; ++j)
                        {
                            Player next = _queuedPlayers[info][tier][teamIndex].GetPlayer(balanceVector, false);
                            AddPlayerToScenarioTeam(scenario, next);
                            AddToBalanceVector(balanceVector, next);
                        }

                    }
                }

                else
                {
                    for (int j = 0; j <= playersNeeded[teamIndex]; ++j)
                    {
                        Player next = _queuedPlayers[info][tier][teamIndex].GetPlayer(balanceVector, false);
                        AddPlayerToScenarioTeam(scenario, next);
                        AddToBalanceVector(balanceVector, next);
                    }
                }
            }
        }

        private void TryCreate6V6Scenario(Scenario_Info info, byte tier)
        {
            while (_queuedGroups[info][tier][0].HasFullGroups && _queuedGroups[info][tier][1].HasFullGroups)
            {
                GroupQueueAction[] groupQueueInfos = { _queuedGroups[info][tier][0].GetFullGroup(), _queuedGroups[info][tier][1].GetFullGroup() };

                Scenario newScenario = CreateInstance(info, tier);
                newScenario.IsPickup = false;

                for (int teamIndex = 0; teamIndex < 2; ++teamIndex)
                {
                    groupQueueInfos[teamIndex].MyGroup.DirectDequeueAll();

                    foreach (Player member in groupQueueInfos[teamIndex].CurMembers)
                        AddPlayerToScenarioTeam(newScenario, member);
                }
            }
        }

        #endregion

        #region Player/Group Queue Functions

        /// <summary>Requests player enqueue after packet handling (latent).</summary>
        /// <param name="player">Queueing player, not null</param>
        /// <param name="scenarioId">scenarios_info.ScenarioId</param>
        public void EnqueuePlayer(Player player, int scenarioId)
        {
            Group grp = player.PriorityGroup;

           
            if (player.IsBanned)
            {
                player.SendClientMessage("You request to join a battle... but there was no one to listen.", ChatLogFilters.CHATLOGFILTERS_EMOTE);
                return;
            }

            lock (_pendingQueueActions)
                _pendingQueueActions.Add(new PlayerQueueAction(player, EPendingQueueAction.PQA_Add, (ushort)scenarioId));
        }

        /// <summary>Requests player dequeue either after packet handling or warband join (latent).</summary>
        /// <param name="player">Queueing player, not null</param>
        /// <param name="scenarioId">scenarios_info.ScenarioId</param>
        public void DequeuePlayer(Player player, int scenarioId)
        {
            lock (_pendingQueueActions)
                _pendingQueueActions.Add(new PlayerQueueAction(player, EPendingQueueAction.PQA_Remove, (ushort)scenarioId));
        }

        /// <summary>Requests player enqueue after packet handling (latent).</summary>
        /// <param name="player">Queueing player, not null</param>
        /// <param name="scenarioId">scenarios_info.ScenarioId</param>
        public void EnqueueGroup(Group group, int scenarioId)
        {
            lock (_pendingQueueActions)
                _pendingQueueActions.Add(new GroupQueueAction(group, EPendingQueueAction.PQA_Add, (ushort)scenarioId));
        }

        /// <summary>Requests player enqueue after packet handling (latent).</summary>
        /// <param name="player">Queueing player, not null</param>
        /// <param name="scenarioId">scenarios_info.ScenarioId</param>
        public void DequeueGroup(Group group, int scenarioId)
        {
            lock (_pendingQueueActions)
                _pendingQueueActions.Add(new GroupQueueAction(group, EPendingQueueAction.PQA_Remove, (ushort)scenarioId));
        }
        
        /// <summary>
        /// Tries to add the given player to current queued list (direct)
        /// and sends this operation result to client.
        /// </summary>
        /// <param name="playerAction">Initial enqueue request</param>
        private void AddPlayer(PlayerQueueAction playerAction)
        {
            Player player = playerAction.MyPlayer;

            if (player.ScnInterface.PendingScenario != null)
            {
                player.SendClientMessage("You cannot queue when you have a scenario pop pending.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (player.Faction == 64)
            {
                player.SendClientMessage("You cannot queue for a scenario while in duelling mode.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (player.ScnInterface.Scenario != null)
            {
                player.SendClientMessage("You cannot queue from within a scenario.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (player.WorldGroup != null && player.WorldGroup.IsQueuedForAny())
            {
                player.SendClientMessage("Your group is queued for a scenario, so you cannot queue alone.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            byte tier = GetScenarioTier(player.Level);

            Scenario_Info info = _activeScenarios.FirstOrDefault(scen => scen.ScenarioId == playerAction.ScenarioId);

            if (info == null || !_queuedPlayers.ContainsKey(info))
            {
                player.SendClientMessage("The scenario you attempted to queue for (" + playerAction.ScenarioId + ") is no longer available or does not exist.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (_queuedPlayers[info][tier][(byte)player.Realm - 1].Contains(player))
            {
                player.SendClientMessage("You are already queued for " + info.Name + "!", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            _queuedPlayers[info][tier][(byte)player.Realm - 1].AddPlayer(player);
            SendScenarioStatus(player, ScenarioUpdateType.Queued, info);

            //Log.Success("ScenarioMgr", "Added " + player.Name + " to " + info.Name + "'s queue.");
        }

        /// <summary>
        /// (Tries to) add the given player to current queued list (direct)
        /// and sends this operation result to client.
        /// </summary>
        /// <param name="playerAction">Initial dequeue request</param>
        private void RemovePlayer(PlayerQueueAction playerAction)
        {
            Scenario_Info info = ScenarioService.GetScenario_Info(playerAction.ScenarioId);

            if (info == null)
            {
                Log.Error("ScenarioMgr", "Attempted to remove " + playerAction.MyPlayer.Name + " from non-existent scenario " + playerAction.ScenarioId);
                return;
            }

            foreach (byte tier in _instances[info].Keys)
                _queuedPlayers[info][tier][(byte)playerAction.MyPlayer.Realm-1].RemovePlayer(playerAction.MyPlayer);


            SendScenarioStatus(playerAction.MyPlayer, ScenarioUpdateType.Leave, info);

            //Log.Success("ScenarioMgr", "Removed " + playerInfo.MyPlayer.Name + " from " + info.Name + "'s queue.");
        }

        /// <summary>
        /// Clears current queued players list of the given player
        /// and sends this operation result to client.
        /// </summary>
        /// <param name="player">Player to remove from all queue lists</param>
        private void RemovePlayerFromAll(Player player)
        {
            foreach (Scenario_Info info in _instances.Keys)
            {
                foreach (byte tier in _instances[info].Keys)
                {
                    if (_queuedPlayers[info][tier][(byte)player.Realm - 1].RemovePlayer(player))
                        SendScenarioStatus(player, ScenarioUpdateType.Leave, info);
                }
            }

            //Log.Success("ScenarioMgr", "Removed " + player.Name + " from all solo queues.");
        }

        /// <summary>
        /// Tries to add the given group to current queued list (direct)
        /// and sends this operation result to client.
        /// </summary>
        /// <param name="groupAction">Initial enqueue request</param>
        private void AddGroup(GroupQueueAction groupAction)
        {
            Player leader = groupAction.MyGroup.Leader;

            if (leader == null)
                return;

            if (groupAction.MyGroup.IsWarband)
            {
                leader.SendClientMessage("Queuing within a Warband is not yet supported.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            List<Player> grpMembers = groupAction.MyGroup.GetPlayerListCopy();

            if (grpMembers.Count < 2)
            {
                leader.SendClientMessage("Your group somehow has too few members to queue.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            int leaderTier = GetScenarioTier(leader.Level);

            foreach (Player gMember in grpMembers)
            {
                if (gMember.ScnInterface.PendingScenario != null)
                {
                    leader.SendClientMessage(gMember.Name + " has a pending scenario.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }

                if (gMember.Faction == 64)
                {
                    leader.SendClientMessage(gMember.Name + " is in duel mode.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }

                if (gMember.ScnInterface.BlockQueue)
                {
                    leader.SendClientMessage(gMember.Name + " is prevented from queueing.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }

                if (gMember.ScnInterface.Scenario != null)
                {
                    leader.SendClientMessage(gMember.Name + " is already in a scenario.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }

                if (GetScenarioTier(gMember.Level) != leaderTier)
                {
                    leader.SendClientMessage(gMember.Name + " is not in the same Tier bracket as " + leader.Name + ".", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }
            }

            // Queue checks passed - add.

            Scenario_Info info = _activeScenarios.FirstOrDefault(scen => scen.ScenarioId == groupAction.ScenarioId);

            if (info == null || !_queuedGroups.ContainsKey(info))
            {
                leader.SendClientMessage("The scenario you attempted to queue for (" + groupAction.ScenarioId + ") is no longer available or does not exist.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            groupAction.CurMembers = grpMembers;

            if (info.QueueType == (int)ScenarioQueueType.Premade && groupAction.CurMembers.Count < 6)
            {
                leader.SendClientMessage("You can't group queue for " + info.Name + " without a full group!", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (!groupAction.MyGroup.CanQueueFor(groupAction.ScenarioId))
            {
                leader.SendClientMessage("You are already queued for " + info.Name + "!", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            GroupQueueHandler curGrpHandler = _queuedGroups[info][(byte) leaderTier][leader.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1];

            if (curGrpHandler == null)
                return;

            for (int i = 0; i < 5; ++i)
            {
                if (curGrpHandler.QueuedGroups[i] != null && curGrpHandler.QueuedGroups[i].Any(x => x.MyGroup == groupAction.MyGroup))
                {
                    Log.Error("ScenarioMgr", "FAILURE - DOUBLE GROUP ADD ATTEMPT!");
                    leader.SendClientMessage("Your group is somehow already queued for this scenario???", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return;
                }
            }

            //Log.Success("ScenarioMgr", "Added " + leader.Name + "'s group to " + info.Name + "'s queue.");

            foreach (var member in grpMembers)
            {
                RemovePlayerFromAll(member);
                SendScenarioStatus(member, ScenarioUpdateType.GroupQueued, info);

                AddToBalanceVector(groupAction.BalanceVector, member);
            }

            groupAction.BalanceVectorMag = groupAction.BalanceVector.Magnitude;

            _queuedGroups[info][(byte) leaderTier][leader.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1].AddGroup(groupAction);
        }

        public void RemoveGroup(Group group, ushort scenarioId)
        {
            RemoveGroup(new GroupQueueAction(group, EPendingQueueAction.PQA_Remove, scenarioId));
        }

        private void RemoveGroup(GroupQueueAction groupAction)
        {
            Scenario_Info info = ScenarioService.GetScenario_Info(groupAction.ScenarioId);

            if (info == null)
            {
                Log.Error("ScenarioMgr", "Attempted to remove a group from a non-existent scenario.");
                return;
            }

            if (groupAction.MyGroup == null)
            {
                Log.Error("ScenarioMgr", "NULL GROUP IN RemoveGroup!");
                return;
            }

            int targetRealm = groupAction.MyGroup.Realm == Realms.REALMS_REALM_ORDER ? 0 : 1;

            bool bRemoved = false;

            if (!groupAction.MyGroup.CanDequeue(groupAction.ScenarioId))
            {
                groupAction.MyGroup.Leader?.SendClientMessage("You aren't queued for " + info.Name + ".", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                return;
            }

            if (groupAction.MyGroup.Leader == null)
                Log.Notice("ScenarioMgr", "Removing a leaderless group from " + info.Name);
            //else
             //   Log.Success("ScenarioMgr", "Removing " + groupInfo.MyGroup.Leader.Name + "'s group from " + info.Name);

            foreach (byte tier in _instances[info].Keys)
                bRemoved = bRemoved | _queuedGroups[info][tier][targetRealm].RemoveGroup(groupAction);

            if (!bRemoved)
                Log.Error("ScenarioMgr", "!!!!Failed to remove any group from RemoveGroup!!!!");

            else
            {
                foreach (Player member in groupAction.CurMembers)
                {
                    if (member == null)
                        Log.Error("ScenarioMgr", "NULL MEMBER in groupInfo.CurMembers in RemoveGroup");
                    else
                        SendScenarioStatus(member, ScenarioUpdateType.Leave, info);
                }
            }
        }

        #endregion

        #region Scenario Rotation

        const int MAX_SCENARIOS = 2;

        /*
        public void RotateScenarios()
        {
            RemoveAllSoloPlayers();
            RemoveAllGroups();

            List<Scenario_Info> newActiveList = ScenarioService.Scenarios.FindAll(sc => sc.Enabled == 2);

            Scenario_Info[] classicalList = Randomize(ScenarioService.Scenarios.FindAll(sc => !sc.DeferKills && sc.Enabled != 2));
            Scenario_Info[] objectiveList = Randomize(ScenarioService.Scenarios.FindAll(sc => sc.DeferKills && sc.Enabled != 2));

            int objectiveCount = 3; //(int)((MAX_SCENARIOS - newActiveList.Count) * 0.44f); // 3 scenarios if 7 or 8 can be chosen
            int classicCount = MAX_SCENARIOS - (newActiveList.Count + objectiveCount);

            for (int i = 0; i < classicalList.Length; ++i)
            {
                classicalList[i].Enabled = i < classicCount ? (byte)1 : (byte)0;
                WorldMgr.Database.SaveObject(classicalList[i]);

                if (i < classicCount)
                    newActiveList.Add(classicalList[i]);
            }

            for (int i = 0; i < objectiveList.Length; ++i)
            {
                objectiveList[i].Enabled = i < objectiveCount ? (byte)1 : (byte)0;
                WorldMgr.Database.SaveObject(objectiveList[i]);

                if (i < objectiveCount)
                    newActiveList.Add(objectiveList[i]);
            }

            _activeScenarios = newActiveList;

            lock (Player._Players)
            {
                foreach (Player player in Player._Players)
                {
                    SendScenarioStatus(player, ScenarioUpdateType.List, null);
                    player.SendClientMessage("The available scenario list has been updated.", ChatLogFilters.CHATLOGFILTERS_CSR_TELL_RECEIVE);
                }
            }
        }
        */

        private void RemoveAllSoloPlayers()
        {
            foreach (Scenario_Info info in _instances.Keys)
            {
                foreach (byte tier in _instances[info].Keys)
                {
                    for (int realmIndex = 0; realmIndex < 2; ++realmIndex)
                    {
                        List<Player> removedPlayers = _queuedPlayers[info][tier][realmIndex].GetAndRemoveAll();
                        foreach (Player player in removedPlayers)
                            SendScenarioStatus(player, ScenarioUpdateType.Leave, info);
                    }
                }
            }
        }

        private void RemoveAllGroups()
        {
            foreach (Scenario_Info info in _instances.Keys)
            {
                foreach (byte tier in _instances[info].Keys)
                {
                    for (int realmIndex = 0; realmIndex < 2; ++realmIndex)
                    {
                        List<GroupQueueAction> removedGroups = _queuedGroups[info][tier][realmIndex].GetAndRemoveAll();
                        foreach (GroupQueueAction group in removedGroups)
                            foreach (Player member in group.CurMembers)
                                SendScenarioStatus(member, ScenarioUpdateType.Leave, info);
                    }
                }
            }
        }

        #endregion

        #region Archetype Balance

        public static void AddToBalanceVector(Vector2 balanceVector, Player player)
        {
            // Calculate balance vector.
            switch (player.CrrInterface.GetArchetype())
            {
                case EArchetype.ARCHETYPE_Tank:
                    balanceVector.X -= 0.866f;
                    balanceVector.Y -= 0.5f;
                    break;
                case EArchetype.ARCHETYPE_DPS:
                    balanceVector.X += 0.866f;
                    balanceVector.Y -= 0.5f;
                    break;
                case EArchetype.ARCHETYPE_Healer:
                    balanceVector.Y += 1f;
                    break;
            }
        }

        public static void RemoveFromBalanceVector(Vector2 balanceVector, Player player)
        {
            switch (player.CrrInterface.GetArchetype())
            {
                case EArchetype.ARCHETYPE_Tank:
                    balanceVector.X += 0.866f;
                    balanceVector.Y += 0.5f;
                    break;
                case EArchetype.ARCHETYPE_DPS:
                    balanceVector.X -= 0.866f;
                    balanceVector.Y += 0.5f;
                    break;
                case EArchetype.ARCHETYPE_Healer:
                    balanceVector.Y -= 1f;
                    break;
            }
        }

        #endregion

        private Scenario CreateInstance(Scenario_Info info, byte tier)
        {
            Scenario scenario;

            switch (info.Type)
            {
                case 1:
                    scenario = new DominationScenario(info, tier);
                    break;
                case 2:
                    scenario = new MurderballScenario(info, tier);
                    break;
                case 3:
                    scenario = new DoubleDominationScenario(info, tier);
                    break;
                case 4:
                    scenario = new DropBombScenario(info, tier);
                    break;
                case 5:
                    scenario = new DropPartScenario(info, tier);
                    break;
                case 6:
                    scenario = new DominationScenarioPushCenter(info, tier);
                    break;
                case 7:
                    scenario = new DominationScenarioEC(info, tier);
                    break;
                case 8:
                    scenario = new DominationScenarioKhaine(info, tier);
                    break;
                case 9:
                    scenario = new FlagDominationScenario(info, tier);
                    break;
                case 10:
                    scenario = new CaptureTheFlagScenario(info, tier);
                    break;
                case 11:
                    scenario = new DominationScenarioPush(info, tier);
                    break;
                default:
                    throw new ArgumentException($"The provided scenario type of {info.Type} is not valid.");
            }
            
            if (_instances.ContainsKey(info))
                _instances[info][tier].Add(scenario);
            else
                Log.Error("ScenarioMgr", "Tried creating non-existant scenario id = '" + info.ScenarioId + "'");

            return scenario;
        }

        private static void BuildScenarioList(PacketOut Out)
        {
            Out.WriteByte((byte)_activeScenarios.Count);
            foreach (Scenario_Info scenario in _activeScenarios)
            {
                Out.WriteUInt16(0);
                Out.WriteUInt16(scenario.ScenarioId);
            }
        }

        public static void SendScenarioStatus(Player plr, ScenarioUpdateType status, Scenario_Info scenario)
        {
            PacketOut Out = new PacketOut((byte)Opcodes.F_INTERACT_RESPONSE, 64);
            Out.WriteByte(9);
            Out.WriteByte((byte)status);

            switch (status)
            {
                case ScenarioUpdateType.List:
                    BuildScenarioList(Out);
                    break;
                case ScenarioUpdateType.Queued:
                    plr.SendLocalizeString(scenario.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SCENARIO_JOIN_SOLO);
                    
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(scenario.ScenarioId);

                    break;
                case ScenarioUpdateType.Leave:
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(scenario.ScenarioId);
                    break;
                case ScenarioUpdateType.Pop:
                    Out.WriteUInt16(0);
                    Out.WriteUInt16(scenario.ScenarioId);
                    //play a sound when a pop occurs
                    //plr.PlaySound(705,false);
                    break;
                case ScenarioUpdateType.GroupQueued:
                    if (plr.WorldGroup.Leader == plr)
                        plr.SendLocalizeString("", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SCENARIO_JOIN_GROUP_LEADER);

                    plr.SendLocalizeString(scenario.Name, ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.TEXT_SCENARIO_JOIN_GROUP);

                    Out.WriteUInt16(0);
                    Out.WriteUInt16(scenario.ScenarioId);
                    break;
            }

            plr.SendPacket(Out);
        }

        public PacketOut BuildScenarioInfo(Scenario scenario)
        {
            int timeLeft;
            PacketOut Out = new PacketOut((byte)Opcodes.F_SCENARIO_INFO, 20);
            if (!scenario.HasStarted)
            {
                timeLeft = (60 - (int)((TCPManager.GetTimeStampMS() - scenario.StartTime) / 1000));
            }
            else if (scenario.HasEnded)
            {
                timeLeft = (120 - (int)((TCPManager.GetTimeStampMS() - scenario.EndTime) / 1000));
            }
            else
            {
                timeLeft = (900 - (int)((TCPManager.GetTimeStampMS() - scenario.StartTime) / 1000));
            }
            Out.WriteUInt32((uint)timeLeft);//time
            Out.WriteUInt32((uint)timeLeft);//time
            Out.WriteByte((byte)((!scenario.HasEnded && scenario.HasStarted) ? 1 : 0)); //in progress (0 - not started/game over)
            Out.WriteByte(0);//unk
            Out.WriteUInt16(500);//max score (winning score)
            Out.WriteUInt16((ushort)scenario.Score[0]);
            Out.WriteUInt16((ushort)scenario.Score[1]);
            Out.WriteUInt32(scenario.Info.ScenarioId);
            return Out;
        }

        #region Scenario Add/Remove

        private void AddPlayerToScenarioTeam(Scenario scenario, Player player)
        {
            scenario.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.AddPendingPlayer, player));
            player.ScnInterface.PendingQueuePop = true;

            scenario.IncrementPlayers(player);

            foreach (Scenario_Info info in _queuedPlayers.Keys)
            {
                foreach (var tier in _queuedPlayers[info].Keys)
                    _queuedPlayers[info][tier][(byte)player.Realm - 1].RemovePlayer(player);
            }
        }

        private void AddPlayerToScenarioTeam(Scenario scenario, List<Player> players)
        {
            Player player = players.First();
            scenario.EnqueueScenarioAction(new ScenarioQueueAction(EScenarioQueueAction.AddPendingPlayer, player));
            player.ScnInterface.PendingQueuePop = true;

            scenario.IncrementPlayers(player);

            foreach (Scenario_Info info in _queuedPlayers.Keys)
            {
                foreach (var tier in _queuedPlayers[info].Keys)
                    _queuedPlayers[info][tier][(byte)player.Realm - 1].RemovePlayer(player);
            }

            players.Remove(player);
        }

        #endregion

        public void ScenariosInfo(Player plr)
        {
            plr.SendLocalizeString("Queue last updated "+SecondsSinceUpdate+" seconds ago.", ChatLogFilters.CHATLOGFILTERS_SAY, Localized_text.CHAT_TAG_MONSTER_EMOTE);

            foreach (Scenario_Info info in _instances.Keys)
            {
                foreach (byte tier in _instances[info].Keys)
                {
                    plr.SendLocalizeString(info.Name + " Tier " + tier, ChatLogFilters.CHATLOGFILTERS_SAY,
                        Localized_text.CHAT_TAG_MONSTER_EMOTE);

                    foreach (Scenario scenario in _instances[info][tier])
                    {
                        if (!scenario.HasEnded)
                        {
                            plr.SendLocalizeString(
                                "Order Players: " + scenario.Players[0].Count + " Destro Players: " +
                                scenario.Players[1].Count + " Order Score: " + scenario.Score[0] + " Destro Score: " +
                                scenario.Score[1], ChatLogFilters.CHATLOGFILTERS_MISC,
                                Localized_text.CHAT_TAG_MONSTER_EMOTE);

                        }
                    }
                }
            }
        }

        public static byte GetScenarioTier(byte level)
        {
            if (level <= 15)
                return 1;
            if (level > 30)
                return 4;
            return 3;
            //return (byte)(level > 2 ? (level - 2)/10 + 1 : 1);
        }

        #region Quitters

        private static readonly Dictionary<int, int> _quitterDurations = new Dictionary<int, int>();

        public static void CheckQuitter(Player plr)
        {
            int quitTimeEnd = 0;

            int curTime = TCPManager.GetTimeStamp();

            lock (_quitterDurations)
                if (_quitterDurations.ContainsKey(plr.Info.AccountId))
                {
                    quitTimeEnd = _quitterDurations[plr.Info.AccountId];
                    if (quitTimeEnd < curTime)
                    {
                        _quitterDurations.Remove(plr.Info.AccountId);
                        return;
                    }
                }

            //if (quitTimeEnd > 0)
            //{
            //    BuffInfo info = AbilityMgr.GetBuffInfo((ushort) GameBuffs.Quitter);
            //    info.Duration = (ushort)(quitTimeEnd - TCPManager.GetTimeStamp());
            //    plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, 1, info));
            //}
        }

        public static void UpdateQuitter(Player plr)
        {
            if (plr.GmLevel > 1)
                return;

            lock (_quitterDurations)
                _quitterDurations[plr.Info.AccountId] = TCPManager.GetTimeStamp() + 600;

            //plr.BuffInterface.QueueBuff(new BuffQueueInfo(plr, 1, AbilityMgr.GetBuffInfo((ushort)GameBuffs.Quitter)));
        }

        #endregion
    }
}
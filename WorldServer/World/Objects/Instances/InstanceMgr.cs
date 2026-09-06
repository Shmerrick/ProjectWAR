using System;
using System.Collections.Generic;
using System.Linq;
using SystemData;
using Common;
using FrameWork;
using WorldServer.Services.World;
using WorldServer.World.Objects.Instances.TomboftheVultureLord;

namespace WorldServer.World.Objects.Instances
{
    public class InstanceMgr
    {
        private Dictionary<ushort, Instance> _instances = new Dictionary<ushort, Instance>();

        public InstanceMgr()
        {

        }
		
        /// <summary>
        /// Player cap for an ordinary group instance. Raids use 24 and realm instances are
        /// uncapped; both are decided per entry in <see cref="ZoneIn"/>, never stored.
        /// </summary>
        private const byte GROUP_INSTANCE_MAX_PLAYERS = 6;

        public bool ZoneIn(Player player, byte instancetyp, Zone_jump Jump = null)
        {
            // jump type 4 = realm 5 = raid 6 = group instances
            ushort zoneID;

            if (Jump == null)
            {
                zoneID = player.Info.Value.ZoneId;
            }
            else
                zoneID = Jump.ZoneID;

            Instance_Info II;
            if (!InstanceService._InstanceInfo.TryGetValue(zoneID, out II) || II == null)
            {
                Log.Error("ZoneIn", "No instance_infos row for zone " + zoneID);
                player.SendClientMessage("This dungeon is not configured on this server.");
                return false;
            }

            ushort InstanceMainID = II.Entry;
            
            ushort instanceid = 0;

            // Capacity belongs to this entry, not to the manager. It used to be a field that was
            // raised to 24 for a raid or dropped to 0 for a realm instance and never put back, so
            // the first Gunbad or Bastion Stair entry left every later group instance on the wrong
            // cap for the rest of the server's life -- including the branch below that turns a
            // group member away with "this instance is already full".
            byte maxplayers = instancetyp == 5 ? (byte)24 : GROUP_INSTANCE_MAX_PLAYERS;

            // Realm instance (jump type 4): one persistent copy per realm rather than one per
            // group. Every player of a realm shares their realm's dungeon, and it is uncapped.
            //
            // Selection is keyed on the realm alone, so a player always returns to the same copy,
            // and the instance is never closed while empty (see Instance.CheckInstanceEmpty), so
            // its public quests stay in cycle and its creatures keep respawning between visits.
            if (instancetyp == 4)
            {
                byte realm = (byte)player.Realm;

                if (realm == 0)
                {
                    Log.Error("ZoneIn", "Player " + player.Name + " has no realm; cannot enter realm instance for zone " + zoneID + ".");
                    return false;
                }

                maxplayers = 0; // realm instances are uncapped

                lock (_instances)
                {
                    foreach (KeyValuePair<ushort, Instance> ii in _instances)
                    {
                        if (ii.Value.Info != null && ii.Value.Info.Entry == II.Entry && ii.Value.Realm == realm)
                        {
                            instanceid = ii.Key;
                            break;
                        }
                    }
                }

                if (instanceid == 0)
                    instanceid = Create_realm_instance(player, Jump, realm, II);

                if (instanceid == 0)
                {
                    Log.Error("ZoneIn", "Could not open a realm instance of zone " + zoneID + " for realm " + realm + ".");
                    player.SendClientMessage("This dungeon could not be opened. Please try again.");
                    return false;
                }

                return Join_Instance(player, instanceid, Jump, InstanceMainID, maxplayers);
            }

            // instance handling
            lock (_instances)
            {
                foreach (KeyValuePair<ushort, Instance> ii in _instances)
                {
                    if (IsGroupInstanceCandidate(ii.Value, II))
                    {
                        // solo
                        if (player.PriorityGroup == null)
                            // enter if instance with name found
                            if (ii.Value.Players.Contains(player))
                            {
                                instanceid = ii.Key; // enter
                                break;
                            }
                            else // create new instance if not
                            {
                                continue; // Another copy may contain this player.
                            }
                        else // group
                        {
                            // player == leader
                            if (player == player.PriorityGroup.GetLeader())
                            {
                                // instance found with leaders name in it
                                if (ii.Value.Players.Contains(player.PriorityGroup.GetLeader()))
                                {
                                    instanceid = ii.Key; // enter
                                    break;
                                }
                                else // create new instance if not
                                {
                                    continue; // Keep looking for the leader's copy.
                                }
                            }
                            else
                            {
                                // instance found with leaders name in it
                                if (ii.Value.Players.Contains(player.PriorityGroup.GetLeader()))
                                {
                                    if (ii.Value.Players.Contains(player))
                                    {
                                        instanceid = ii.Key; // enter
                                        break;
                                    }
                                    else
                                    {
                                        if (ii.Value.Players.Count < maxplayers)
                                        {
                                            instanceid = ii.Key; // enter
                                            break;
                                        }
                                        else
                                        {
                                            player.SendClientMessage("This instance is already full. Please find another group or switch the group leader to open a new instance.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                                            return false;
                                        }
                                    }
                                }
                                else
                                    continue;
                            }
                        }
                    }
                }
            }

            if (instanceid == 0 && Jump == null)
				return false;

			// create new instance
			if (instanceid == 0)
			{
				instanceid = Create_new_instance(player, Jump);
			}
            
            if (!Join_Instance(player, instanceid, Jump, InstanceMainID, maxplayers))
				return false;

			return true;
		}

        internal static bool IsGroupInstanceCandidate(Instance instance, Instance_Info destination)
        {
            // Gunbad's cave and boss maps share Entry 60. Entry alone can select the
            // realm cave for a group boss portal, whose destination is outside that region.
            return instance != null && destination != null && instance.Realm == 0
                && instance.ZoneID == destination.ZoneID
                && instance.Info != null && instance.Info.Entry == destination.Entry;
        }

		private TimeSpan GetLockoutTimer(Player plr, ushort zoneID)
		{
			string lockout = plr._Value.GetLockout(zoneID);
			if (lockout == null)
				return new TimeSpan(0);
			else
			{
				return new TimeSpan(Math.Abs(int.Parse(lockout.Split(':')[1]) - TCPManager.GetTimeStampMS()));
			}
		}

        /// <summary>
        /// Realm instance ids occupy a small reserved block at the bottom, and the dynamic
        /// allocator starts above it. Putting the fixed ids high instead would leave them
        /// reachable: group and raid instances take the first free id counting upwards, so a
        /// long-running server could eventually allocate into the reserved range and collide.
        /// Reserving the bottom makes that impossible by construction.
        ///
        /// A realm id is 1 + (Instance_Info.Entry * 4) + realm, and the highest dungeon entry is
        /// in the low hundreds, so the block is comfortably inside its ceiling.
        /// </summary>
        private const ushort REALM_INSTANCE_ID_BASE = 1;

        /// <summary>First id the dynamic allocator may use. Everything below is reserved.</summary>
        private const ushort DYNAMIC_INSTANCE_ID_MIN = 2000;

        /// <summary>
        /// The fixed id of a realm's copy of a dungeon.
        ///
        /// Realm instances are permanent, so their ids must be derived rather than allocated: a
        /// first-free id would shift whenever instances open in a different order, and instance
        /// lockouts are keyed "ZoneID:ID", so a shifted id silently orphans every lockout that
        /// referenced it. Deriving from the dungeon and the realm gives the same id for the life
        /// of the server and across restarts.
        /// </summary>
        private static ushort GetRealmInstanceId(Instance_Info info, byte realm)
        {
            int id = REALM_INSTANCE_ID_BASE + (info.Entry * 4) + realm;

            if (id >= DYNAMIC_INSTANCE_ID_MIN)
            {
                Log.Error("GetRealmInstanceId", "Instance " + info.Entry + " realm " + realm +
                          " derives id " + id + ", which is outside the reserved range below " + DYNAMIC_INSTANCE_ID_MIN + ".");
                return 0;
            }

            return (ushort)id;
        }

        /// <summary>
        /// Opens a realm's permanent copy of a dungeon at its fixed id.
        /// </summary>
        private ushort Create_realm_instance(Player player, Zone_jump Jump, byte realm, Instance_Info info)
        {
            ushort id = GetRealmInstanceId(info, realm);

            if (id == 0)
                return 0;

            lock (_instances)
            {
                if (_instances.ContainsKey(id))
                    return id;

                Instance_Lockouts deadbosses = ResolveCharacterLockout(player._Value, info.Entry);

                _instances.Add(id, new Instance(Jump.ZoneID, id, realm, deadbosses));
            }

            Log.Success("Opening Realm Instance", "Instance ID " + id + "  Realm " + realm + "  Map: " + info.Name);
            return id;
        }

        /// <summary>
        /// Opens a new instance. <paramref name="realm"/> is 0 for the ordinary group and raid
        /// instances and the player's realm for a realm instance, which both selects the realm's
        /// spawns and marks the instance as persistent.
        /// </summary>
        private ushort Create_new_instance(Player player, Zone_jump Jump, byte realm = 0)
        {
            lock (_instances)
            {
                // Starts above the reserved block so a dynamic id can never collide with a
                // realm instance's fixed id, however many instances have been opened.
                for (ushort i = DYNAMIC_INSTANCE_ID_MIN; i < ushort.MaxValue ; i++)
                {
                    if (!_instances.ContainsKey(i))
                    {
                        if (Jump.ZoneID == 179)
                        {
                            TOTVL ints = null;
                            Instance_Lockouts deadbosses = ResolveCharacterLockout(
                                (player.PriorityGroup?.GetLeader() ?? player)._Value, Jump.InstanceID);
                            ints = new TOTVL(Jump.ZoneID, i, realm, deadbosses);
                            _instances.Add(i, ints);
                            return i;
                        }
                        else
                        {
                            Instance ints = null;
                            Instance_Lockouts deadbosses = ResolveCharacterLockout(
                                (player.PriorityGroup?.GetLeader() ?? player)._Value, Jump.InstanceID);
                            ints = new Instance(Jump.ZoneID, i, realm, deadbosses);
                            _instances.Add(i, ints);
                            return i;
                        }
                    }
                }
            }
            return 0;
        }

        internal static Instance_Lockouts ResolveCharacterLockout(Character_value value, ushort zoneId)
        {
            string saved = value?.GetLockout(zoneId);
            if (string.IsNullOrWhiteSpace(saved))
                return null;

            string[] parts = saved.Split(':');
            if (parts.Length < 3 || !int.TryParse(parts[1], out int expires) || expires <= TCPManager.GetTimeStamp())
                return null;

            // Character records include :boss:boss; the world dictionary key does not.
            // Use this character's own progress, not another group's same-day record.
            var bosses = new SortedSet<uint>();
            for (int i = 2; i < parts.Length; ++i)
                if (uint.TryParse(parts[i], out uint boss) && boss != 0)
                    bosses.Add(boss);

            return bosses.Count == 0 ? null : new Instance_Lockouts
            {
                InstanceID = "~" + zoneId + ":" + expires,
                Bosseskilled = string.Join(":", bosses)
            };
        }

        private bool Join_Instance(Player player, ushort Instanceid, Zone_jump Jump, ushort InstancemainID, byte maxplayers)
        {
            lock (_instances)
            {
                if (!_instances.TryGetValue(Instanceid, out Instance inst))
                    return false;

                if (inst.Realm == 0 && InstanceService._InstanceBossSpawns.TryGetValue(InstancemainID, out List<Instance_Boss_Spawn> bosses))
                {
                    foreach (Instance_Boss_Spawn boss in bosses)
                    {
                        if (boss.ZoneID == inst.ZoneID && player.HasLockout(inst.ZoneID, boss.bossId) && !inst.IsBossKilled(boss.bossId))
                        {
                            player.SendClientMessage("Your lockout does not allow you to join a fresh copy of this encounter.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                            return false;
                        }
                    }
                }

                if (inst.EncounterInProgress)
                {
                    player.SendClientMessage("There is an Encounter in progress you cannot enter now", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }

                if (maxplayers == 0 || inst.Players.Count < maxplayers)
                {
                    if (Jump != null && Jump.ZoneID == 179)
                        ((TOTVL)inst).AddPlayer(player, Jump);
                    else
                        inst.AddPlayer(player, Jump);
                }
                else
                {
                    player.SendClientMessage("Instance is full.", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }
            }
            return true;
        }

        public void CloseInstance(Instance inst, ushort ID)
        {
            inst.Players = new List<Player>();
            _instances.TryGetValue(ID, out inst);
            _instances.Remove(ID);
            inst = null;
        }

        public void SendInstanceInfo(Player plr, ushort instanceid)
        {
            if (instanceid == 0)
            {
                plr.SendClientMessage("Total instances =" + _instances.Count(), ChatLogFilters.CHATLOGFILTERS_SAY);
                lock (_instances)
                {
                    foreach (KeyValuePair<ushort, Instance> i in _instances)
                    {
                        plr.SendClientMessage("Instance id = " + i.Key + "  map= " + i.Value.Info.Name + "  Players: " + i.Value.Region.Players.Count, ChatLogFilters.CHATLOGFILTERS_SAY);
                    }
                }
            }
            else
            {
                _instances.TryGetValue(instanceid, out Instance i);
                if (i == null)
                {
                    plr.SendClientMessage("Instance id = " + instanceid + "not found", ChatLogFilters.CHATLOGFILTERS_SAY);
                    return;
                }
                plr.SendClientMessage("Instance id = " + instanceid + "  Map= " + i.Info.Name + "  Players: " + i.Region.Players.Count, ChatLogFilters.CHATLOGFILTERS_SAY);
                string players = string.Empty;
                foreach (Player pl in i.Region.Players)
                {
                    players += pl.Name + "  ,";
                }
                plr.SendClientMessage("Players: " + players, ChatLogFilters.CHATLOGFILTERS_SAY);
            }
        }

        public void HandlePlayerSetDeath(Player plr, Unit killer)
        {
            if (killer is World.Objects.Instances.InstanceBossSpawn boss)
                boss.PlayerDeathsCount++;
        }

        public void ApplyLockout(string instanceId, List<Player> players)
        {
            if (_instances == null || string.IsNullOrEmpty(instanceId) || players == null || players.Count == 0)
                return;
            try
            {
                string[] split = instanceId.Split(':');
                if (split.Length < 2)
                    return;

                if (!ushort.TryParse(split[0], out ushort zoneId))
                    return;

                if (!ushort.TryParse(split[1], out ushort localInstanceId))
                    return;

                _instances.TryGetValue(localInstanceId, out Instance inst);
                if (inst == null || inst.CurrentBossId == 0)
                    return;

                inst.ApplyLockout(players.Where(x => x != null && !x.HasLockout(zoneId, inst.CurrentBossId)).ToList());
            }
            catch (Exception e)
            {
                Log.Error("Exception", e.Message + "\r\n" + e.StackTrace);
            }
        }

        public bool HasLockoutFromCurrentBoss(Player plr)
        {
            if (_instances == null || plr == null || string.IsNullOrEmpty(plr.InstanceID))
                return false;

            try
            {
                string[] split = plr.InstanceID.Split(':');
                if (split.Length < 2)
                    return false;

                if (!ushort.TryParse(split[1], out ushort localInstanceId))
                    return false;

                _instances.TryGetValue(localInstanceId, out Instance inst);
                if (inst == null || inst.CurrentBossId == 0)
                    return false;

                return plr.HasLockout(inst.ZoneID, inst.CurrentBossId);
            }
            catch (Exception e)
            {
                Log.Error("Exception", e.Message + "\r\n" + e.StackTrace);
            }
            return false;
        }

        public void RemovePlayerFromInstances(Player plr)
        {
            if (_instances == null || plr == null)
                return;

            lock (_instances)
            {
                foreach (Instance instance in _instances.Values)
                {
                    lock (instance.Players)
                    {
                        if (!instance.Players.Remove(plr))
                            continue;

                        InstanceService.SavePlayerIDs(instance.ZoneID + ":" + instance.ID, instance.Players);
                    }
                }
            }
        }
    }
}

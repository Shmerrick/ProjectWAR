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
		
        private byte _maxplayers = 6;

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

			// Group Raid Instance
			if (instancetyp == 5)
                _maxplayers = 24;

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

                _maxplayers = 0;

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

                return Join_Instance(player, instanceid, Jump, InstanceMainID);
            }

            // instance handling
            lock (_instances)
            {
                foreach (KeyValuePair<ushort, Instance> ii in _instances)
                {
                    if (ii.Value.Info.Entry == II.Entry)
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
                                instanceid = 0; // create new instance
                                break;
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
                                    instanceid = 0; // create new instance
                                    break;
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
                                        if (ii.Value.Players.Count < _maxplayers)
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
            
            if (!Join_Instance(player, instanceid, Jump, InstanceMainID))
				return false;

			return true;
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

                Instance_Lockouts deadbosses = null;
                if (player._Value.GetLockout(Jump.InstanceID) != null)
                    InstanceService._InstanceLockouts.TryGetValue(player._Value.GetLockout(Jump.InstanceID), out deadbosses);

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
                            Instance_Lockouts deadbosses = null;
							if (player._Value.GetLockout(Jump.InstanceID) != null)
							{
								if (player.PriorityGroup == null) // solo player gets his own lockouts
									InstanceService._InstanceLockouts.TryGetValue(player._Value.GetLockout(Jump.InstanceID), out deadbosses);
								else // group players gets the lockout of the leader
									InstanceService._InstanceLockouts.TryGetValue(player.PriorityGroup.GetLeader()._Value.GetLockout(Jump.InstanceID), out deadbosses);
							}
							ints = new TOTVL(Jump.ZoneID, i, realm, deadbosses);
                            _instances.Add(i, ints);
                            return i;
                        }
                        else
                        {
                            Instance ints = null;
                            Instance_Lockouts deadbosses = null;
                            if (player._Value.GetLockout(Jump.InstanceID) != null)
							{	
								if (player.PriorityGroup == null) // solo player gets his own lockouts
									InstanceService._InstanceLockouts.TryGetValue(player._Value.GetLockout(Jump.InstanceID), out deadbosses);
								else if (player.PriorityGroup.GetLeader()._Value.GetLockout(Jump.InstanceID) != null) // group players gets the lockout of the leader
                                    InstanceService._InstanceLockouts.TryGetValue(player.PriorityGroup.GetLeader()._Value.GetLockout(Jump.InstanceID), out deadbosses);
							}
                            ints = new Instance(Jump.ZoneID, i, realm, deadbosses);
                            _instances.Add(i, ints);
                            return i;
                        }
                    }
                }
            }
            return 0;
        }

        private bool Join_Instance(Player player, ushort Instanceid, Zone_jump Jump, ushort InstancemainID)
        {
            lock (_instances)
            {
                _instances.TryGetValue(Instanceid, out Instance inst);

                if (inst.EncounterInProgress)
                {
                    player.SendClientMessage("There is an Encounter in progress you cannot enter now", ChatLogFilters.CHATLOGFILTERS_USER_ERROR);
                    return false;
                }

                if (_maxplayers == 0 || inst.Players.Count < _maxplayers)
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

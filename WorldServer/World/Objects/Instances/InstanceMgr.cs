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
            InstanceService._InstanceInfo.TryGetValue(zoneID, out II);
            ushort InstanceMainID = II.Entry;
            
            ushort instanceid = 0;
			
			// Group Raid Instance
			if (instancetyp == 5)
                _maxplayers = 24;

            // instance handling
            lock (_instances)
            {
                foreach (KeyValuePair<ushort, Instance> ii in _instances)
                {
                    if (ii.Value.ZoneID == zoneID)
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

        private ushort Create_new_instance(Player player, Zone_jump Jump)
        {
            lock (_instances)
            {
                for (ushort i = 1; i < ushort.MaxValue ; i++)
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
							ints = new TOTVL(Jump.ZoneID, i, 0, deadbosses);
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
                            ints = new Instance(Jump.ZoneID, i, 0, deadbosses);
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
            if (_instances == null && string.IsNullOrEmpty(instanceId))
                return;
            try
            {
                _instances.TryGetValue(ushort.Parse(instanceId.Split(':')[1]), out Instance inst);
                inst?.ApplyLockout(players.Where(x => !x.HasLockout(ushort.Parse(instanceId.Split(':')[1]), inst.CurrentBossId)).ToList());
            }
            catch (Exception e)
            {
                Log.Error("Exception", e.Message + "\r\n" + e.StackTrace);
            }
        }

        public bool HasLockoutFromCurrentBoss(Player plr)
        {
            if (_instances == null && (plr == null || string.IsNullOrEmpty(plr.InstanceID)))
                return false;

            try
            {
                _instances.TryGetValue(ushort.Parse(plr.InstanceID.Split(':')[1]), out Instance inst);
                return plr.HasLockout(plr.Zone.ZoneId, inst.CurrentBossId);
            }
            catch (Exception e)
            {
                Log.Error("Exception", e.Message + "\r\n" + e.StackTrace);
            }
            return false;
        }

        public void RemovePlayerFromInstances(Player plr)
        {
            if (_instances == null)
                return;

            for (int i = 0; i < _instances.Count; i++)
            {
                if (_instances.Values.ElementAt(i).Players.Contains(plr))
                    _instances.Values.ElementAt(i).Players.Remove(plr);
            }
        }
    }
}
